import { Outlet } from "react-router-dom";
import PageHeader from "../components/PageHeader";

const AppLayout = () => {
  return (
    <div>
      <PageHeader />
      <main>
        <Outlet />
      </main>
    </div>
  );
};

export default AppLayout;
