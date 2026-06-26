import { DataGrid } from "@mui/x-data-grid";

const BookChangesTable = ({ changes }) => {
  const columns = [
    {
      field: "changedAt",
      headerName: "Changed At",
      flex: 1,
      minWidth: 180,
      sortable: false,
      filterable: false,
      valueGetter: (value) => {
        return new Date(value).toLocaleString();
      },
    },
    {
      field: "fieldName",
      headerName: "Field",
      flex: 0.7,
      minWidth: 130,
      sortable: false,
      filterable: false,
    },
    {
      field: "description",
      headerName: "Description",
      flex: 2,
      minWidth: 300,
      sortable: false,
      filterable: false,
    },
  ];

  return (
    <DataGrid
      rows={changes}
      columns={columns}
      getRowId={(row) => row.id}
      disableRowSelectionOnClick
      hideFooter
      sx={{
        "& .MuiDataGrid-columnHeaderTitle": {
          fontWeight: 700,
        },
      }}
    />
  );
};

export default BookChangesTable;
