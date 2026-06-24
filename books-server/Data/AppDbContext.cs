using books_server.Models;
using Microsoft.EntityFrameworkCore;

namespace books_server.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<BookAuthor> BookAuthors => Set<BookAuthor>();
    public DbSet<BookChange> BookChanges => Set<BookChange>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Book>(entity =>
        {
            entity.Property(book => book.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(book => book.ShortDescription)
                .HasMaxLength(1000);
        });

        modelBuilder.Entity<Author>(entity =>
        {
            entity.Property(author => author.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasIndex(author => author.Name)
                .IsUnique();
        });

        modelBuilder.Entity<Author>().HasData(
            new Author { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "William Shakespeare" },
            new Author { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "George Orwell" },
            new Author { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Jane Austen" },
            new Author { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Fyodor Dostoevsky" },
            new Author { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = "Mary Shelley" },
            new Author { Id = Guid.Parse("66666666-6666-6666-6666-666666666666"), Name = "Agatha Christie" },
            new Author { Id = Guid.Parse("77777777-7777-7777-7777-777777777777"), Name = "Ernest Hemingway" },
            new Author { Id = Guid.Parse("88888888-8888-8888-8888-888888888888"), Name = "Virginia Woolf" },
            new Author { Id = Guid.Parse("99999999-9999-9999-9999-999999999999"), Name = "Mark Twain" },
            new Author { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Name = "Charles Dickens" },
            new Author { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Name = "Franz Kafka" },
            new Author { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), Name = "Harper Lee" }
);

        modelBuilder.Entity<BookAuthor>(entity =>
        {
            entity.HasKey(bookAuthor => new
            {
                bookAuthor.BookId,
                bookAuthor.AuthorId
            });

            entity.HasOne(bookAuthor => bookAuthor.Book)
                .WithMany(book => book.BookAuthors)
                .HasForeignKey(bookAuthor => bookAuthor.BookId);

            entity.HasOne(bookAuthor => bookAuthor.Author)
                .WithMany(author => author.BookAuthors)
                .HasForeignKey(bookAuthor => bookAuthor.AuthorId);
        });

        modelBuilder.Entity<BookChange>(entity =>
        {
            entity.Property(change => change.FieldName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(change => change.Description)
                .IsRequired()
                .HasMaxLength(1000);

            entity.HasOne(change => change.Book)
                .WithMany(book => book.Changes)
                .HasForeignKey(change => change.BookId);
        });
    }
}