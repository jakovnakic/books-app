using books_server.Dtos;
using books_server.Services;
using Microsoft.AspNetCore.Mvc;

namespace books_server.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private readonly BookService _bookService;

    public BooksController(BookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public async Task<ActionResult<List<BookDto>>> GetBooks()
    {
        var books = await _bookService.GetBooksAsync();

        return Ok(books);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookDto>> GetBookById(Guid id)
    {
        var book = await _bookService.GetBookByIdAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        return Ok(book);
    }

    [HttpGet("authors")]
    public async Task<ActionResult<List<AuthorDto>>> GetAuthors()
    {
        var authors = await _bookService.GetAuthorsAsync();

        return Ok(authors);
    }

    [HttpPost]
    public async Task<ActionResult<BookDto>> CreateBook(BookRequest request)
    {
        try
        {
            var createdBook = await _bookService.CreateBookAsync(request);

            return CreatedAtAction(
                nameof(GetBookById),
                new { id = createdBook.Id },
                createdBook
            );
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BookDto>> UpdateBook(Guid id, BookRequest request)
    {
        try
        {
            var updatedBook = await _bookService.UpdateBookAsync(id, request);

            if (updatedBook is null)
            {
                return NotFound();
            }

            return Ok(updatedBook);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }
}