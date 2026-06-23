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
            .Select(book => new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                ShortDescription = book.ShortDescription,
                PublishDate = book.PublishDate,
                Authors = book.BookAuthors
                    .Select(bookAuthor => bookAuthor.Author.Name)
                    .OrderBy(authorName => authorName)
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<BookDto?> GetBookByIdAsync(int id)
    {
        return await _dbContext.Books
            .AsNoTracking()
            .Include(book => book.BookAuthors)
                .ThenInclude(bookAuthor => bookAuthor.Author)
            .Where(book => book.Id == id)
            .Select(book => new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                ShortDescription = book.ShortDescription,
                PublishDate = book.PublishDate,
                Authors = book.BookAuthors
                    .Select(bookAuthor => bookAuthor.Author.Name)
                    .OrderBy(authorName => authorName)
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<BookDto> CreateBookAsync(BookRequest request)
    {
        var normalizedAuthors = NormalizeAuthorNames(request.Authors);

        var book = new Book
        {
            Title = request.Title.Trim(),
            ShortDescription = request.ShortDescription.Trim(),
            PublishDate = request.PublishDate
        };

        foreach (var authorName in normalizedAuthors)
        {
            var author = await GetOrCreateAuthorAsync(authorName);

            book.BookAuthors.Add(new BookAuthor
            {
                Book = book,
                Author = author
            });
        }

        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();

        return await GetBookByIdAsync(book.Id)
            ?? throw new InvalidOperationException("Created book could not be found.");
    }

    public async Task<BookDto?> UpdateBookAsync(int id, BookRequest request)
    {
        var book = await _dbContext.Books
            .Include(book => book.BookAuthors)
                .ThenInclude(bookAuthor => bookAuthor.Author)
            .FirstOrDefaultAsync(book => book.Id == id);

        if (book is null){
            return null;
        }

        book.Title = request.Title.Trim();
        book.ShortDescription = request.ShortDescription.Trim();
        book.PublishDate = request.PublishDate;

        await UpdateBookAuthorsAsync(book, request.Authors);

        await _dbContext.SaveChangesAsync();

        return await GetBookByIdAsync(book.Id);
    }

    private async Task UpdateBookAuthorsAsync(Book book, List<string> requestedAuthors)
    {
        var normalizedRequestedAuthors = NormalizeAuthorNames(requestedAuthors);

        var existingBookAuthors = book.BookAuthors.ToList();

        foreach (var bookAuthor in existingBookAuthors)
        {
            var authorName = bookAuthor.Author.Name;

            if (!normalizedRequestedAuthors.Contains(authorName, StringComparer.OrdinalIgnoreCase))
            {
                book.BookAuthors.Remove(bookAuthor);
            }
        }

        var existingAuthorNames = book.BookAuthors
            .Select(bookAuthor => bookAuthor.Author.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var authorName in normalizedRequestedAuthors)
        {
            if (existingAuthorNames.Contains(authorName))
            {
                continue;
            }

            var author = await GetOrCreateAuthorAsync(authorName);

            book.BookAuthors.Add(new BookAuthor
            {
                BookId = book.Id,
                AuthorId = author.Id,
                Book = book,
                Author = author
            });
        }
    }

    private async Task<Author> GetOrCreateAuthorAsync(string authorName)
    {
        var existingAuthor = await _dbContext.Authors
            .FirstOrDefaultAsync(author => author.Name.ToLower() == authorName.ToLower());

        if (existingAuthor is not null)
        {
            return existingAuthor;
        }

        var newAuthor = new Author
        {
            Name = authorName
        };

        _dbContext.Authors.Add(newAuthor);

        return newAuthor;
    }

    private static List<string> NormalizeAuthorNames(List<string> authors)
    {
        return authors
            .Where(author => !string.IsNullOrWhiteSpace(author))
            .Select(author => author.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(author => author)
            .ToList();
    }
}