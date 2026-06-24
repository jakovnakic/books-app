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
        await _dbContext.SaveChangesAsync();

        return await GetBookByIdAsync(book.Id)
            ?? throw new InvalidOperationException("Created book could not be found.");
    }

    public async Task<BookDto?> UpdateBookAsync(Guid id, BookRequest request)
    {
        var book = await _dbContext.Books
            .Include(book => book.BookAuthors)
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

        var authors = await _dbContext.Authors
            .Where(author => authorIds.Contains(author.Id))
            .ToListAsync();

        if (authors.Count != authorIds.Count)
        {
            throw new ArgumentException("One or more selected authors do not exist.");
        }

        book.Title = request.Title.Trim();
        book.ShortDescription = request.ShortDescription.Trim();
        book.PublishDate = request.PublishDate;

        book.BookAuthors.Clear();

        foreach (var author in authors)
        {
            book.BookAuthors.Add(new BookAuthor
            {
                BookId = book.Id,
                AuthorId = author.Id
            });
        }

        await _dbContext.SaveChangesAsync();

        return await GetBookByIdAsync(book.Id);
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