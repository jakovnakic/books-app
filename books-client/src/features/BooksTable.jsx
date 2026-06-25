import { DataGrid } from "@mui/x-data-grid";
import { Button } from "@mui/material";

const BooksTable = ({ books, onEditBook }) => {
  const columns = [
    {
      field: "title",
      headerName: "Title",
      flex: 1,
      minWidth: 180,
      sortable: true,
      filterable: true,
    },
    {
      field: "authors",
      headerName: "Authors",
      flex: 2,
      minWidth: 220,
      sortable: true,
      filterable: true,
      valueGetter: (value) => {
        return value.map((author) => author.name).join(", ");
      },
    },
    {
      field: "publishDate",
      headerName: "Publish Date",
      flex: 0.7,
      sortable: true,
      filterable: true,
    },
    {
      field: "shortDescription",
      headerName: "Description",
      flex: 1.5,
      minWidth: 250,
      sortable: true,
      filterable: true,
    },
    {
      field: "actions",
      headerName: "Actions",
      width: 120,
      sortable: false,
      filterable: false,
      align: "center",
      headerAlign: "center",
      renderCell: (params) => (
        <Button
          variant="outlined"
          size="small"
          onClick={() => onEditBook(params.row)}
        >
          Edit
        </Button>
      ),
    },
  ];

  return (
    <DataGrid
      rows={books}
      columns={columns}
      getRowId={(row) => row.id}
      pageSizeOptions={[5, 10, 25]}
      disableRowSelectionOnClick
      initialState={{
        pagination: {
          paginationModel: {
            pageSize: 5,
          },
        },
      }}
      sx={{
        "& .MuiDataGrid-columnHeaderTitle": {
          fontWeight: 700,
        },
      }}
    />
  );
};

export default BooksTable;
