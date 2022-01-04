import { styled } from "@mui/material/styles";
import { grey } from "@mui/material/colors";

export default styled("div")(({ theme }) => ({
  height: "100vh",
  backgroundColor: theme.palette.mode === "light" ? grey[100] : theme.palette.background.default,
  overflowY: "auto",
}));
