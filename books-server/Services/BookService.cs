using books_server.Data;
using books_server.Dtos;
using books_server.Models;
using Microsoft.EntityFrameworkCore;

namespace books_server.Services;

public class BookService
{
    private readonly AppDbContext _dbContext;

    public BookService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<BookDto>> GetBooksAsync()
    {
        return await _dbContext.Books
            .AsNoTracking()
            .Include(book => book.BookAuthors)
                .ThenInclude(bookAuthor => bookAuthor.Author)
            .OrderBy(book => book.Title)
            .Select(book => MapBookToDto(book))
            .ToListAsync();
    }

    public async Task<BookDto?> GetBookByIdAsync(Guid id)
    {
        var book = await _dbContext.Books
            .AsNoTracking()
            .Include(book => book.BookAuthors)
                .ThenInclude(bookAuthor => bookAuthor.Author)
            .FirstOrDefaultAsync(book => book.Id == id);

        return book is null ? null : MapBookToDto(book);
    }

    public async Task<List<AuthorDto>> GetAuthorsAsync()
    {
        return await _dbContext.Authors
            .AsNoTracking()
            .OrderBy(author => author.Name)
            .Select(author => new AuthorDto
            {
                Id = author.Id,
                Name = author.Name
            })
            .ToListAsync();
    }

    public async Task<BookDto> CreateBookAsync(BookRequest request)
    {
        var authorIds = request.AuthorIds.Distinct().ToList();

        if (authorIds.Count == 0)
        {
            throw new ArgumentException("At least one author must be selected.");
        }

        var authors = await _dbContext.Authors
            .Where(author => authorIds.Contains(author.Id))
            .ToListAsync();

        if (authors.Count != authorIds.Count)
        {
            throw new ArgumentException("One or more selected authors do not exist.");
        }

        var book = new Book
        {
            Title = request.Title.Trim(),
            ShortDescription = request.ShortDescription.Trim(),
            PublishDate = request.PublishDate,
            BookAuthors = authors
                .Select(author => new BookAuthor
                {
                    AuthorId = author.Id
                })
                .ToList()
        };

        _dbContext.Books.Add(book);

        book.Changes.Add(new BookChange
        {
            Book = book,
            FieldName = "Book",
            OldValue = null,
            NewValue = book.Title,
            Description = $"Book \"{book.Title}\" was created."
        });

        foreach (var author in authors.OrderBy(author => author.Name))
        {
            book.Changes.Add(new BookChange
            {
                Book = book,
                FieldName = "Authors",
                OldValue = null,
                NewValue = author.Name,
                Description = $"Author \"{author.Name}\" was added."
            });
        }

        await _dbContext.SaveChangesAsync();

        return await GetBookByIdAsync(book.Id)
            ?? throw new InvalidOperationException("Created book could not be found.");
    }

    public async Task<BookDto?> UpdateBookAsync(Guid id, BookRequest request)
    {
        var book = await _dbContext.Books
            .FirstOrDefaultAsync(book => book.Id == id);

        if (book is null)
        {
            return null;
        }

        var authorIds = request.AuthorIds.Distinct().ToList();

        if (authorIds.Count == 0)
        {
            throw new ArgumentException("At least one author must be selected.");
        }

        var requestedAuthors = await _dbContext.Authors
            .Where(author => authorIds.Contains(author.Id))
            .ToListAsync();

        if (requestedAuthors.Count != authorIds.Count)
        {
            throw new ArgumentException("One or more selected authors do not exist.");
        }

        var currentAuthors = await _dbContext.BookAuthors
            .AsNoTracking()
            .Where(bookAuthor => bookAuthor.BookId == book.Id)
            .Select(bookAuthor => bookAuthor.Author)
            .ToListAsync();

        var changes = new List<BookChange>();

        changes.AddRange(CreateBasicFieldChanges(book, request));
        changes.AddRange(CreateAuthorChanges(book.Id, currentAuthors, requestedAuthors));

        book.Title = request.Title.Trim();
        book.ShortDescription = request.ShortDescription.Trim();
        book.PublishDate = request.PublishDate;

        var currentAuthorIds = currentAuthors
            .Select(author => author.Id)
            .ToHashSet();

        var requestedAuthorIds = requestedAuthors
            .Select(author => author.Id)
            .ToHashSet();

        var authorIdsToRemove = currentAuthorIds
            .Where(authorId => !requestedAuthorIds.Contains(authorId))
            .ToList();

        if (authorIdsToRemove.Count > 0)
        {
            await _dbContext.BookAuthors
                .Where(bookAuthor =>
                    bookAuthor.BookId == book.Id &&
                    authorIdsToRemove.Contains(bookAuthor.AuthorId))
                .ExecuteDeleteAsync();
        }

        var authorsToAdd = requestedAuthors
            .Where(author => !currentAuthorIds.Contains(author.Id))
            .ToList();

        foreach (var author in authorsToAdd)
        {
            _dbContext.BookAuthors.Add(new BookAuthor
            {
                BookId = book.Id,
                AuthorId = author.Id
            });
        }

        _dbContext.BookChanges.AddRange(changes);

        await _dbContext.SaveChangesAsync();

        return await GetBookByIdAsync(book.Id);
    }

    public async Task<PagedResult<BookChangeDto>?> GetBookChangesAsync(
    Guid bookId,
    BookChangeQueryParameters queryParameters)
    {
        var bookExists = await _dbContext.Books
            .AsNoTracking()
            .AnyAsync(book => book.Id == bookId);

        if (!bookExists)
        {
            return null;
        }

        var page = queryParameters.Page < 1 ? 1 : queryParameters.Page;
        var pageSize = queryParameters.PageSize < 1 ? 10 : queryParameters.PageSize;

        if (pageSize > 50)
        {
            pageSize = 50;
        }

        var query = _dbContext.BookChanges
            .AsNoTracking()
            .Where(change => change.BookId == bookId);

        if (!string.IsNullOrWhiteSpace(queryParameters.FieldName))
        {
            var fieldName = queryParameters.FieldName.Trim();

            query = query.Where(change => change.FieldName == fieldName);
        }

        var sortDirection = queryParameters.SortDirection.ToLower();

        query = sortDirection == "asc"
            ? query.OrderBy(change => change.ChangedAt)
            : query.OrderByDescending(change => change.ChangedAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(change => new BookChangeDto
            {
                Id = change.Id,
                BookId = change.BookId,
                ChangedAt = change.ChangedAt,
                FieldName = change.FieldName,
                OldValue = change.OldValue,
                NewValue = change.NewValue,
                Description = change.Description
            })
            .ToListAsync();

        return new PagedResult<BookChangeDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    private static List<BookChange> CreateBasicFieldChanges(Book book, BookRequest request)
    {
        var changes = new List<BookChange>();

        var newTitle = request.Title.Trim();
        var newShortDescription = request.ShortDescription.Trim();

        if (book.Title != newTitle)
        {
            changes.Add(new BookChange
            {
                BookId = book.Id,
                FieldName = "Title",
                OldValue = book.Title,
                NewValue = newTitle,
                Description = $"Title was changed to \"{newTitle}\"."
            });
        }

        if (book.ShortDescription != newShortDescription)
        {
            changes.Add(new BookChange
            {
                BookId = book.Id,
                FieldName = "ShortDescription",
                OldValue = book.ShortDescription,
                NewValue = newShortDescription,
                Description = "Short description was changed."
            });
        }

        if (book.PublishDate != request.PublishDate)
        {
            changes.Add(new BookChange
            {
                BookId = book.Id,
                FieldName = "PublishDate",
                OldValue = book.PublishDate.ToString("yyyy-MM-dd"),
                NewValue = request.PublishDate.ToString("yyyy-MM-dd"),
                Description = $"Publish date was changed to {request.PublishDate:yyyy-MM-dd}."
            });
        }

        return changes;
    }

    private static List<BookChange> CreateAuthorChanges(
    Guid bookId,
    List<Author> currentAuthors,
    List<Author> requestedAuthors)
    {
        var changes = new List<BookChange>();

        var currentAuthorIds = currentAuthors
            .Select(author => author.Id)
            .ToHashSet();

        var requestedAuthorIds = requestedAuthors
            .Select(author => author.Id)
            .ToHashSet();

        var addedAuthors = requestedAuthors
            .Where(author => !currentAuthorIds.Contains(author.Id))
            .OrderBy(author => author.Name)
            .ToList();

        var removedAuthors = currentAuthors
            .Where(author => !requestedAuthorIds.Contains(author.Id))
            .OrderBy(author => author.Name)
            .ToList();

        foreach (var author in addedAuthors)
        {
            changes.Add(new BookChange
            {
                BookId = bookId,
                FieldName = "Authors",
                OldValue = null,
                NewValue = author.Name,
                Description = $"Author \"{author.Name}\" was added."
            });
        }

        foreach (var author in removedAuthors)
        {
            changes.Add(new BookChange
            {
                BookId = bookId,
                FieldName = "Authors",
                OldValue = author.Name,
                NewValue = null,
                Description = $"Author \"{author.Name}\" was removed."
            });
        }

        return changes;
    }

    private static BookDto MapBookToDto(Book book)
    {
        return new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            ShortDescription = book.ShortDescription,
            PublishDate = book.PublishDate,
            Authors = book.BookAuthors
                .Select(bookAuthor => new AuthorDto
                {
                    Id = bookAuthor.Author.Id,
                    Name = bookAuthor.Author.Name
                })
                .OrderBy(author => author.Name)
                .ToList()
        };
    }
}