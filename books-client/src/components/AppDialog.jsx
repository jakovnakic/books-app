import { Dialog, IconButton } from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import styles from "../styles/AppDialog.module.scss";

const AppDialog = ({ isOpen, title, children, onClose }) => {
  return (
    <Dialog open={isOpen} onClose={onClose}>
      <div className={styles["dialog"]}>
        <header className={styles["header"]}>
          <h1 className={styles["header__heading"]}>{title}</h1>
          <IconButton onClick={onClose} size="small">
            <CloseIcon />
          </IconButton>
        </header>
        {children}
      </div>
    </Dialog>
  );
};

export default AppDialog;
