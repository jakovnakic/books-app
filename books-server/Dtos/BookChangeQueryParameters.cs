namespace books_server.Dtos;

public class BookChangeQueryParameters
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? FieldName { get; set; }

    public string SortDirection { get; set; } = "desc";
}