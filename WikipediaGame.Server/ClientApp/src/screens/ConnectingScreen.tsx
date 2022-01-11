import React from "react";
import Root from "../components/Root";
import {
  AppBar,
  Box,
  Card,
  CardContent,
  CardActions,
  Container,
  Toolbar,
  Typography,
  TextField,
  Button,
  Skeleton,
  Backdrop,
  CircularProgress,
} from "@mui/material";

export default function ConnectingScreen() {
  const [timedOut, setTimedOut] = React.useState(false);

  React.useEffect(() => {
    const timeout = setTimeout(() => {
      setTimedOut(true);
    }, 2000);

    return () => clearTimeout(timeout);
  }, []);

  return (
    <Root>
      <AppBar position="static">
        <Container maxWidth="sm">
          <Toolbar disableGutters>
            <Skeleton width={120} />
          </Toolbar>
        </Container>
      </AppBar>

      <Container maxWidth="sm" sx={{ marginTop: 2 }}>
        <Card>
          <CardContent>
            <Typography gutterBottom variant="h5">
              <Skeleton width={200} />
            </Typography>

            <TextField sx={{ mt: 2 }} disabled fullWidth />
          </CardContent>
          <CardActions>
            <Button size="small" disabled>
              <Skeleton width={100} />
            </Button>
          </CardActions>
        </Card>
      </Container>

      {timedOut && (
        <Backdrop sx={{ color: "#fff", zIndex: (theme) => theme.zIndex.drawer + 1 }} open>
          <Box sx={{ textAlign: "center" }}>
            <Typography variant="h5" sx={{ mb: 4 }}>
              Reconnecting...
            </Typography>
            <CircularProgress color="inherit" />
          </Box>
        </Backdrop>
      )}
    </Root>
  );
}
