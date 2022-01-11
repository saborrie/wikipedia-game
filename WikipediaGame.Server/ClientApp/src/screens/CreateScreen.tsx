import React from "react";
import { useGame } from "../game";
import Root from "../components/Root";
import {
  AppBar,
  Card,
  CardContent,
  CardActions,
  Container,
  Paper,
  Toolbar,
  Typography,
  TextField,
  Button,
} from "@mui/material";

export default function CreateScreen() {
  const game = useGame();

  const [name, setName] = React.useState("");

  return (
    <Root>
      <AppBar position="static">
        <Container maxWidth="sm">
          <Toolbar disableGutters>
            <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
              Wikipedia Game
            </Typography>
          </Toolbar>
        </Container>
      </AppBar>

      <Container maxWidth="sm" sx={{ marginTop: 2 }}>
        <Card>
          <CardContent>
            <Typography gutterBottom variant="h5">
              Create new game
            </Typography>

            <TextField
              sx={{ mt: 2 }}
              value={name}
              onChange={(e) => setName(e.target.value)}
              label="What is  your name?"
              fullWidth
            />
          </CardContent>
          <CardActions>
            <Button size="small" onClick={() => game?.createGame(name)}>
              Create
            </Button>
          </CardActions>
        </Card>
      </Container>
    </Root>
  );
}
