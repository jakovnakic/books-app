import { useQuery } from "@tanstack/react-query";
import { Button } from "@mui/material";
import { getBooks } from "../api/booksApi";
import BooksTable from "../features/BooksTable";
import styles from "../styles/BooksPage.module.scss";

const BooksPage = () => {
  const { data: books = [], isLoading } = useQuery({
    queryKey: ["books"],
    queryFn: getBooks,
  });

  const handleAddBook = () => {
    console.log("Add book clicked");
  };

  const handleEditBook = (book) => {
    console.log("Edit book clicked", book);
  };

  if (!isLoading && books.length == 0) return <p>No books found...</p>;

  return (
    <div className={styles["books-page"]}>
      <header className={styles["books-page__header"]}>
        <p>View and manage your books</p>
        <Button variant="contained" onClick={handleAddBook}>
          Add Book
        </Button>
      </header>
      <BooksTable books={books} onEditBook={handleEditBook} />
    </div>
  );
};

export default BooksPage;
