import React from "react";
import { useGame } from "../game";
import Root from "../components/Root";
import { Link } from "react-router-dom";
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
  IconButton,
} from "@mui/material";
import { useParams } from "react-router-dom";

import BackIcon from "@mui/icons-material/ArrowBack";

export default function JoinScreen() {
  const game = useGame();
  const gameCode: string = useParams().gameCode!;

  const [name, setName] = React.useState("");

  return (
    <Root>
      <AppBar position="static">
        <Container maxWidth="sm">
          <Toolbar disableGutters>
            <IconButton
              size="large"
              edge="start"
              color="inherit"
              aria-label="menu"
              sx={{ mr: 2 }}
              component={Link}
              to="/"
            >
              <BackIcon />
            </IconButton>
          </Toolbar>
        </Container>
      </AppBar>

      <Container maxWidth="sm" sx={{ marginTop: 2 }}>
        <Card>
          <CardContent>
            <Typography gutterBottom variant="h5">
              Join Game
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
            <Button size="small" onClick={() => game?.joinGame(name, gameCode)}>
              Join
            </Button>
          </CardActions>
        </Card>
      </Container>
    </Root>
  );
}
