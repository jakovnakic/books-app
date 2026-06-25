import { Outlet } from "react-router-dom";
import PageHeader from "../components/PageHeader";
import styles from "../styles/AppLayout.module.scss";

const AppLayout = () => {
  return (
    <div className={styles["container"]}>
      <PageHeader />
      <main className={styles["main"]}>
        <Outlet />
      </main>
    </div>
  );
};

export default AppLayout;
