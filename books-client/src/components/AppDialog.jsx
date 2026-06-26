import { Dialog, IconButton } from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import styles from "../styles/AppDialog.module.scss";

const AppDialog = ({ isOpen, title, size, children, onClose }) => {
  return (
    <Dialog open={isOpen} onClose={onClose} maxWidth={false}>
      <div
        className={`${styles["dialog"]} ${size ? styles[`dialog--${size}`] : ""}`}
      >
        <header className={styles["header"]}>
          <h1 className={styles["header__heading"]}>{title}</h1>
          <IconButton onClick={onClose} size="small">
            <CloseIcon />
          </IconButton>
        </header>
        <main className={styles["main"]}>{children}</main>
      </div>
    </Dialog>
  );
};

export default AppDialog;
