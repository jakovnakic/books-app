import styles from "../styles/PageHeader.module.scss";

const PageHeader = () => {
  return (
    <header className={styles["bar"]}>
      <h1 className={styles["bar__heading"]}>Books App</h1>
    </header>
  );
};

export default PageHeader;
