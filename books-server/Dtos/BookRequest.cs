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

    [MinLength(1, ErrorMessage = "At least one author must be selected.")]
    public List<Guid> AuthorIds { get; set; } = new();
}