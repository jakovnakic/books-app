import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Button } from "@mui/material";
import { createBook, getAuthors, getBooks, updateBook } from "../api/booksApi";
import BooksTable from "../features/BooksTable";
import styles from "../styles/BooksPage.module.scss";
import { useReducer } from "react";
import BookFormDialog from "../features/BookFormDialog";

const initialState = {
  selectedBook: null,
  isBookDialogOpen: false,
};

const reducer = (state, action) => {
  switch (action.type) {
    case "openAddBookDialog":
      return {
        selectedBook: null,
        isBookDialogOpen: true,
      };
    case "openEditBookDialog":
      return {
        selectedBook: action.payload,
        isBookDialogOpen: true,
      };
    case "closeBookDialog":
      return {
        ...state,
        isBookDialogOpen: false,
      };
    default:
      return state;
  }
};

const BooksPage = () => {
  const [state, dispatch] = useReducer(reducer, initialState);
  const { selectedBook, isBookDialogOpen } = state;

  const queryClient = useQueryClient();

  const { data: books = [], isLoading } = useQuery({
    queryKey: ["books"],
    queryFn: getBooks,
  });

  const { data: authors = [] } = useQuery({
    queryKey: ["authors"],
    queryFn: getAuthors,
  });

  const createBookMutation = useMutation({
    mutationFn: createBook,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["books"] });
      handleCloseBookDialog();
    },
  });

  const updateBookMutation = useMutation({
    mutationFn: ({ id, book }) => updateBook(id, book),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["books"] });
      handleCloseBookDialog();
    },
  });

  const handleAddBook = () => {
    dispatch({ type: "openAddBookDialog" });
  };

  const handleEditBook = (book) => {
    dispatch({ type: "openEditBookDialog", payload: book });
  };

  const handleCloseBookDialog = () => {
    dispatch({ type: "closeBookDialog" });
  };

  const handleSaveBook = (bookFormData) => {
    if (selectedBook) {
      updateBookMutation.mutate({
        id: selectedBook.id,
        book: bookFormData,
      });
      return;
    }

    createBookMutation.mutate(bookFormData);
  };

  if (!isLoading && books.length == 0) return <p>No books found...</p>;

  return (
    <>
      <div className={styles["books-page"]}>
        <header className={styles["header"]}>
          <p className={styles["header__description"]}>
            View and manage your books
          </p>
          <Button variant="contained" onClick={handleAddBook}>
            Add Book
          </Button>
        </header>
        <BooksTable books={books} onEditBook={handleEditBook} />
      </div>
      <BookFormDialog
        isOpen={isBookDialogOpen}
        mode={selectedBook ? "edit" : "add"}
        book={selectedBook}
        authors={authors}
        onClose={handleCloseBookDialog}
        onSave={handleSaveBook}
      />
    </>
  );
};

export default BooksPage;
