namespace books_server.Dtos;

public class BookDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string ShortDescription { get; set; } = string.Empty;

    public DateOnly PublishDate { get; set; }

    public List<AuthorDto> Authors { get; set; } = new();
}