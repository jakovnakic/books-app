# Books App

Small full-stack app for managing books.

## 1. Start the backend

Open a terminal in the project root and go to the backend folder:
```bash
cd books-server
```
Restore the backend packages:
```bash
dotnet restore
```
Apply the database migrations:
```bash
dotnet ef database update
```
Start the backend:
```bash
dotnet run
```
The backend should now be running on:
```text
http://localhost:5156
```
Swagger is available here:
```text
http://localhost:5156/swagger
```

## 2. Start the frontend

Open a second terminal in the project root and go to the frontend folder:
```bash
cd books-client
```
Install the frontend packages:
```bash
npm install
```
Start the frontend:
```bash
npm run dev
```
The frontend should now be running on:
```text
http://localhost:5173
```
Open that URL in the browser to test the app.

## Notes

The backend uses SQLite, so no separate database server is needed.
Authors are already populated in the database. When adding or editing a book, you can select authors from the existing list.
If the backend starts on a different port than `5156`, update the frontend API base URL in:
```text
books-client/src/api/axiosClient.js
```