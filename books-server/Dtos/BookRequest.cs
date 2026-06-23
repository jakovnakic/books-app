using System.ComponentModel.DataAnnotations;

namespace books_server.Dtos;

public class BookRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string ShortDescription { get; set; } = string.Empty;

    [Required]
    public DateOnly PublishDate { get; set; }

    public List<string> Authors { get; set; } = new();
}