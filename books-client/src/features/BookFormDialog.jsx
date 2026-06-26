import { useEffect, useReducer } from "react";
import {
  Button,
  FormControl,
  IconButton,
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

  fieldName: "all",
  sortDirection: "desc",
  page: 1,
  totalPages: 1,
  pageSize: 5,
};

const reducer = (state, action) => {
  switch (action.type) {
    case "resetForm":
      return {
        ...initialState,
        fieldName: state.fieldName,
        sortDirection: state.sortDirection,
      };
    case "loadBook":
      return {
        ...state,
        ...action.payload,
        page: 1,
      };
    case "changeValue": {
      const { name, value } = action.payload;
      return {
        ...state,
        [name]: value,
      };
    }
    case "changeHistoryFilter": {
      const { name, value } = action.payload;
      return {
        ...state,
        [name]:
          name == "pageSize" || name == "totalPages" ? Number(value) : value,
        page: 1,
      };
    }
    case "goToFirstPage":
      return {
        ...state,
        page: 1,
      };
    case "goToPreviousPage":
      return {
        ...state,
        page: Math.max(state.page - 1, 1),
      };
    case "goToNextPage":
      return {
        ...state,
        page: state.page + 1,
      };
    case "goToLastPage":
      return {
        ...state,
        page: state.totalPages,
      };
    default:
      return state;
  }
};

const BookFormDialog = ({ isOpen, mode, book, authors, onClose, onSave }) => {
  const [state, dispatch] = useReducer(reducer, initialState);
  const {
    title,
    shortDescription,
    publishDate,
    authorIds,
    fieldName,
    sortDirection,
    page,
    pageSize,
  } = state;

  const { data: changesResult = {}, isLoading: isLoadingChangesResult } =
    useQuery({
      queryKey: [
        "bookChanges",
        book?.id,
        fieldName,
        sortDirection,
        page,
        pageSize,
      ],
      queryFn: () =>
        getBookChanges(book.id, {
          page,
          pageSize,
          fieldName: fieldName == "all" ? "" : fieldName,
          sortDirection,
        }),
      enabled: isOpen && mode === "edit" && !!book?.id,
    });

  const changes = changesResult.items ?? [];
  const totalPages = changesResult.totalPages ?? 1;
  const currentPage = changesResult.page ?? page;

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

  useEffect(() => {
    if (!isLoadingChangesResult) {
      dispatch({
        type: "changeValue",
        payload: { name: "totalPages", value: totalPages },
      });
    }
  }, [isLoadingChangesResult, changesResult]);

  const handleSave = () => {
    onSave({
      title: title.trim(),
      shortDescription: shortDescription.trim(),
      publishDate: publishDate,
      authorIds: authorIds,
    });
  };

  const handleChangeHistoryFilter = (event) => {
    const { name, value } = event.target;

    dispatch({
      type: "changeHistoryFilter",
      payload: { name, value },
    });
  };

  const handleFirstPageButton = () => {
    dispatch({ type: "goToFirstPage" });
  };

  const handlePreviousPageButton = () => {
    dispatch({ type: "goToPreviousPage" });
  };

  const handleNextPageButton = () => {
    dispatch({ type: "goToNextPage" });
  };

  const handleLastPageButton = () => {
    dispatch({ type: "goToLastPage" });
  };

  const selectedAuthorNames = authorIds
    .map((authorId) => authors.find((author) => author.id === authorId)?.name)
    .filter(Boolean)
    .join(", ");

  const pageButtonStyle = {
    border: "1px solid",
    borderColor: "primary.main",
    color: "primary.main",
    width: "3rem",
    height: "3rem",
    "&:disabled": {
      borderColor: "action.disabled",
    },
  };

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
          <Button
            onClick={onClose}
            variant="outlined"
            size="small"
            color="error"
          >
            Cancel
          </Button>
        </section>
        {mode === "edit" && (
          <section className={styles["history"]}>
            <h2 className={styles["history__title"]}>History of changes</h2>
            <div className={styles["history__controls"]}>
              <div className={styles["history__control"]}>
                <span>Filter field</span>
                <FormControl fullWidth size="small">
                  <Select
                    name="fieldName"
                    value={fieldName}
                    onChange={handleChangeHistoryFilter}
                  >
                    <MenuItem value="all">All fields</MenuItem>
                    <MenuItem value="Title">Title</MenuItem>
                    <MenuItem value="ShortDescription">
                      Short description
                    </MenuItem>
                    <MenuItem value="PublishDate">Publish date</MenuItem>
                    <MenuItem value="Authors">Authors</MenuItem>
                  </Select>
                </FormControl>
              </div>
              <div className={styles["history__control"]}>
                <span>Sort direction</span>
                <FormControl fullWidth size="small">
                  <Select
                    name="sortDirection"
                    value={sortDirection}
                    onChange={handleChangeHistoryFilter}
                  >
                    <MenuItem value="desc">Newest first</MenuItem>
                    <MenuItem value="asc">Oldest first</MenuItem>
                  </Select>
                </FormControl>
              </div>
              <div className={styles["history__control"]}>
                <span>Rows per page</span>

                <FormControl fullWidth size="small">
                  <Select
                    name="pageSize"
                    value={pageSize}
                    onChange={handleChangeHistoryFilter}
                  >
                    <MenuItem value={2}>2</MenuItem>
                    <MenuItem value={3}>3</MenuItem>
                    <MenuItem value={4}>4</MenuItem>
                    <MenuItem value={5}>5</MenuItem>
                  </Select>
                </FormControl>
              </div>
            </div>

            {changes.length === 0 ? (
              <p>No changes recorded yet.</p>
            ) : (
              <>
                <div className={styles["history__table"]}>
                  <BookChangesTable changes={changes} />
                </div>
                <div className={styles["history__pagination"]}>
                  <IconButton
                    variant="outlined"
                    size="small"
                    disabled={currentPage <= 1}
                    onClick={handleFirstPageButton}
                    sx={pageButtonStyle}
                  >
                    &lt;&lt;
                  </IconButton>
                  <IconButton
                    variant="outlined"
                    size="small"
                    disabled={currentPage <= 1}
                    onClick={handlePreviousPageButton}
                    sx={pageButtonStyle}
                  >
                    &lt;
                  </IconButton>
                  <span>
                    Page {currentPage} of {totalPages}
                  </span>
                  <IconButton
                    variant="outlined"
                    size="small"
                    disabled={currentPage >= totalPages}
                    onClick={handleNextPageButton}
                    sx={pageButtonStyle}
                  >
                    &gt;
                  </IconButton>
                  <IconButton
                    variant="outlined"
                    size="small"
                    disabled={currentPage >= totalPages}
                    onClick={handleLastPageButton}
                    sx={pageButtonStyle}
                  >
                    &gt;&gt;
                  </IconButton>
                </div>
              </>
            )}
          </section>
        )}
      </div>
    </AppDialog>
  );
};

export default BookFormDialog;
