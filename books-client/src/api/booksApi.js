import axiosClient from "./axiosClient";

export const getBooks = async () => {
  const response = await axiosClient.get("/books");
  return response.data;
};

export const getBookById = async (id) => {
  const response = await axiosClient.get(`/books/${id}`);
  return response.data;
};

export const getAuthors = async () => {
  const response = await axiosClient.get("/books/authors");
  return response.data;
};

export const createBook = async (book) => {
  const response = await axiosClient.post("/books", book);
  return response.data;
};

export const updateBook = async (id, book) => {
  const response = await axiosClient.put(`/books/${id}`, book);
  return response.data;
};

export const getBookChanges = async (bookId, queryParams) => {
  const response = await axiosClient.get(`/books/${bookId}/changes`, {
    params: queryParams,
  });

  return response.data;
};
