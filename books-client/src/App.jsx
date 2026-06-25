import { Route, Routes } from "react-router-dom";
import AppLayout from "./layouts/AppLayout";
import BooksPage from "./pages/BooksPage";

const App = () => {
  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route path="/" element={<BooksPage />} />
      </Route>
    </Routes>
  );
};

export default App;
