namespace books_server.Dtos;

public class BookChangeDto
{
    public Guid Id { get; set; }

    public Guid BookId { get; set; }

    public DateTime ChangedAt { get; set; }

    public string FieldName { get; set; } = string.Empty;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string Description { get; set; } = string.Empty;
}