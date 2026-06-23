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