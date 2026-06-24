namespace books_server.Models;

public class BookChange
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid BookId { get; set; }

    public Book Book { get; set; } = null!;

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    public string FieldName { get; set; } = string.Empty;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string Description { get; set; } = string.Empty;
}