import { useEffect, useReducer } from "react";
import {
  Button,
  FormControl,
  MenuItem,
  Select,
  TextField,
} from "@mui/material";
import AppDialog from "../components/AppDialog";
import styles from "../styles/BookFormDialog.module.scss";
import { useQuery } from "@tanstack/react-query";
import { getBookChanges } from "../api/booksApi";
import BookChangesTable from "./BookChangesTable";

const initialState = {
  title: "",
  shortDescription: "",
  publishDate: "",
  authorIds: [],
};

const reducer = (state, action) => {
  switch (action.type) {
    case "resetForm":
      return initialState;
    case "loadBook":
      return action.payload;
    case "changeValue": {
      const { name, value } = action.payload;
      return {
        ...state,
        [name]: value,
      };
    }
    default:
      return state;
  }
};

const BookFormDialog = ({ isOpen, mode, book, authors, onClose, onSave }) => {
  const [state, dispatch] = useReducer(reducer, initialState);
  const { title, shortDescription, publishDate, authorIds } = state;

  const { data: changesResult = {} } = useQuery({
    queryKey: ["bookChanges", book?.id],
    queryFn: () =>
      getBookChanges(book.id, {
        page: 1,
        pageSize: 10,
        sortDirection: "desc",
      }),
    enabled: isOpen && mode === "edit" && !!book?.id,
  });
  const changes = changesResult.items ?? [];

  useEffect(() => {
    if (!isOpen) return;
    if (mode === "edit" && book) {
      dispatch({
        type: "loadBook",
        payload: {
          title: book.title,
          shortDescription: book.shortDescription,
          publishDate: book.publishDate,
          authorIds: book.authors.map((author) => author.id),
        },
      });
    } else {
      dispatch({ type: "resetForm" });
    }
  }, [isOpen]);

  const handleChangeValue = (event) => {
    const { name, value } = event.target;
    dispatch({ type: "changeValue", payload: { name, value } });
  };

  const handleSave = () => {
    onSave({
      title: title.trim(),
      shortDescription: shortDescription.trim(),
      publishDate: publishDate,
      authorIds: authorIds,
    });
  };

  const selectedAuthorNames = authorIds
    .map((authorId) => authors.find((author) => author.id === authorId)?.name)
    .filter(Boolean)
    .join(", ");

  return (
    <AppDialog
      isOpen={isOpen}
      title={mode === "edit" ? "Edit Book" : "Add Book"}
      onClose={onClose}
      size="large"
    >
      <div className={styles["book-form-dialog"]}>
        <div className="input-group">
          <span>Title</span>
          <TextField
            name="title"
            value={title}
            onChange={handleChangeValue}
            fullWidth
            size="small"
          />
        </div>

        <div className="input-group">
          <span>Short description</span>
          <TextField
            name="shortDescription"
            value={shortDescription}
            onChange={handleChangeValue}
            multiline
            minRows={3}
            maxRows={3}
            size="small"
            fullWidth
          />
        </div>
        <div className="input-group">
          <span>Publish date</span>
          <TextField
            name="publishDate"
            type="date"
            size="small"
            value={publishDate}
            onChange={handleChangeValue}
            fullWidth
            InputLabelProps={{
              shrink: true,
            }}
          />
        </div>
        <div className="input-group">
          <span>Authors</span>
          <FormControl fullWidth size="small">
            <Select
              name="authorIds"
              multiple
              value={authorIds}
              onChange={handleChangeValue}
              renderValue={() => selectedAuthorNames}
            >
              {authors.map((author) => (
                <MenuItem key={author.id} value={author.id}>
                  {author.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </div>
        <section className={styles["action-buttons"]}>
          <Button
            variant="contained"
            disabled={
              !title.trim() ||
              !shortDescription.trim() ||
              !publishDate ||
              authorIds.length === 0
            }
            size="small"
            onClick={handleSave}
          >
            Save
          </Button>
          <Button onClick={onClose} variant="outlined" size="small">
            Cancel
          </Button>
        </section>
        {mode === "edit" && (
          <section className={styles["history"]}>
            <h2 className={styles["history__title"]}>History of changes</h2>
            {changes.length === 0 ? (
              <p>No changes recorded yet.</p>
            ) : (
              <BookChangesTable changes={changes} />
            )}
          </section>
        )}
      </div>
    </AppDialog>
  );
};

export default BookFormDialog;
