import {
  AppBar,
  Avatar,
  Button,
  Container,
  IconButton,
  Paper,
  Stack,
  TableRow,
  TableContainer,
  Table,
  TableBody,
  TableCell,
  TextField,
  Toolbar,
  Typography,
  Card,
  CardContent,
  CardActions,
  List,
  ListItem,
  ListItemText,
  Snackbar,
} from "@mui/material";
import MenuIcon from "@mui/icons-material/Menu";
import { styled } from "@mui/material/styles";

import React from "react";
import { Article, useGame } from "../game";
import { Box } from "@mui/system";
import Root from "../components/Root";
import wikiLinkParser from "../utilities/wikiLinkParser";
import SwipableEdgeDrawer from "../components/SwipableEdgeDrawer";

const Item = styled(Paper)(({ theme }) => ({
  ...theme.typography.body2,
  padding: theme.spacing(1),
  textAlign: "center",
  color: theme.palette.text.secondary,
  display: "flex",
  alignItems: "center",
  justifyContent: "center",
  flexDirection: "column",
  marginBottom: 20,
}));

const StyledTableRow = styled(TableRow)(({ theme }) => ({
  "&:nth-of-type(odd)": {
    // backgroundColor: theme.palette.action.hover,
  },
  // hide last border
  "&:last-child td, &:last-child th": {
    border: 0,
  },
}));

function HowToPlayCard({ onChooseArticle }: { onChooseArticle: () => void }) {
  const game = useGame();

  return (
    <Card>
      <CardContent>
        <Typography gutterBottom variant="h5" component="div">
          How to play
        </Typography>

        <Typography variant="body2" color="text.secondary">
          One player is The Guesser. All of the other players must each secretly pick an article
          from wikipedia, and memorise as much information as possible about it.
          <br />
          When the round starts, one of the articles will be selected at random and everyone will
          see the title. The guesser's job is to figure out which player the article belongs to, by
          asking the other players questions. If your article was chosen, then you must tell the
          truth. If your article wasn't chosen, then you will have to make it up!
        </Typography>
      </CardContent>
      <CardActions>
        <Button size="small" onClick={() => game?.becomeGuesser()}>
          Be the Guesser
        </Button>
        <Button size="small" onClick={() => onChooseArticle()}>
          Choose my article
        </Button>
      </CardActions>
    </Card>
  );
}

export default function GameScreen() {
  const game = useGame();
  const [open, setOpen] = React.useState(false);
  const [displayedEventCount, setDisplayedEventCount] = React.useState(0);

  const [article, setArticle] = React.useState<Article | null>(null);

  if (!game) {
    return <div>loading...</div>;
  }

  const guesserName = game.state?.game?.players?.find((x) => x.isGuesser)?.name;
  const clue = game.state?.game?.clue;
  const options = game.state?.game?.options;
  const iAmGuesser = guesserName === game?.state?.game?.username;

  function renderContent() {
    if (game?.state?.game?.revealed) {
      const article = game.state?.game.answer?.article;

      return (
        <Paper>
          <Box sx={{ p: 2, pb: 2 }}>
            <Typography variant="overline" display="block">
              The article is
            </Typography>
            <Typography gutterBottom variant="h5" component="div">
              {article?.name}
            </Typography>
            {article?.description && (
              <Typography variant="body2" sx={{ marginBottom: 1 }}>
                {article?.description}
              </Typography>
            )}
            <Typography variant="body2" color="text.secondary" sx={{ marginBottom: 1 }}>
              {article?.extract}
            </Typography>
            <Typography variant="body1" sx={{ marginBottom: 4 }}>
              It was {game.state?.game.answer?.username}'s article!
            </Typography>
            <Button variant="contained" onClick={() => game?.reset()}>
              Reset and play again
            </Button>
          </Box>
        </Paper>
      );
    }
    if (game?.state?.game?.inPlay) {
      return (
        <>
          <Paper>
            <Box sx={{ p: 2, pb: 0 }}>
              <Typography variant="overline" display="block">
                The article is
              </Typography>
              <Typography gutterBottom variant="h5" component="div">
                {clue}
              </Typography>
              <Typography variant="body2" gutterBottom>
                {guesserName} must figure out whose it is!
              </Typography>
            </Box>
            <List sx={{ bgcolor: "background.paper" }}>
              {options?.map((x) => (
                <ListItem>
                  <ListItemText primary={x} />
                  {iAmGuesser && <Button onClick={() => game?.makeGuess(x)}>Choose</Button>}
                </ListItem>
              ))}
            </List>
          </Paper>
        </>
      );
    }

    const isFullyReady =
      game?.state?.game?.players?.some((x) => x.isGuesser) === true &&
      !game?.state?.game?.players?.some((x) => !x.isGuesser && !x.hasArticle) === true;

    return (
      <>
        <HowToPlayCard onChooseArticle={() => setOpen(true)} />

        <Paper sx={{ mt: 2, p: 2 }}>
          <Box sx={{ mb: 2 }}>
            <Button variant="contained" disabled={!isFullyReady} onClick={() => game?.startRound()}>
              Start Round
            </Button>
          </Box>
          {isFullyReady ? (
            <Typography>Everyone is ready, lets go!</Typography>
          ) : (
            <Typography>Waiting for a guesser and all other players to be ready.</Typography>
          )}
        </Paper>
      </>
    );
  }

  // let content = (
  <>
    <Typography>Waiting to start</Typography>

    {!game.state?.game?.players?.some((x) => x.isGuesser) ? (
      <Typography>A guesser needs to be chosen</Typography>
    ) : (
      game.state?.game?.players?.find((x) => x.isGuesser)?.name + " is the guesser."
    )}
    {game.state?.game?.players?.some((x) => !x.isGuesser && !x.hasArticle) && (
      <Typography>Waiting for all players to be ready</Typography>
    )}
  </>;
  // );

  // if (game.state?.game?.inPlay) {
  //   content = (
  //     <>
  //       <Typography>
  //         {game.state?.game?.players?.find((x) => x.isGuesser)?.name} is the guesser.
  //       </Typography>
  //     </>
  //   );
  // }

  const events = game?.state?.game?.events;
  const lastEvent = events?.slice(events.length - 1);

  return (
    <Root>
      <Snackbar
        key={displayedEventCount}
        anchorOrigin={{ vertical: "top", horizontal: "center" }}
        open={displayedEventCount < (events?.length ?? 0)}
        autoHideDuration={6000}
        onClose={(event, reason) => setDisplayedEventCount(events?.length ?? 0)}
        message={lastEvent}
      />
      <AppBar position="static">
        <Container maxWidth="sm">
          <Toolbar disableGutters>
            <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
              Game {game.state?.game?.gameCode}
            </Typography>
            <Button color="inherit" onClick={() => game.leaveGame()}>
              Leave
            </Button>
          </Toolbar>
        </Container>
      </AppBar>
      <Container maxWidth="sm" sx={{ marginTop: 2 }}>
        <Typography variant="subtitle1">Players</Typography>
        <TableContainer sx={{ marginTop: 1, marginBottom: 2 }} component={Paper}>
          <Table size="small" aria-label="a dense table">
            <TableBody>
              {game.state?.game?.players?.map((player) => (
                <StyledTableRow key={player.name}>
                  <TableCell component="th" scope="row">
                    {player.name}
                  </TableCell>
                  <TableCell align="right">
                    {player.isGuesser ? "GUESSER" : player.hasArticle ? "READY" : ""}
                  </TableCell>
                </StyledTableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>

        {renderContent()}
      </Container>

      <SwipableEdgeDrawer
        open={open}
        setOpen={(value: boolean) => setOpen(value)}
        title={
          <Container maxWidth="sm">
            <Typography sx={{ p: 2, color: "text.secondary" }}>Your Article</Typography>
          </Container>
        }
      >
        <Container maxWidth="sm">
          <ArticlePicker />
          {/* <button onClick={() => game.leaveGame()}>Leave</button>
          <button onClick={() => game.becomeGuesser()}>Become guesser</button>
          <button onClick={() => game.reset()}>Reset</button>
          <button onClick={() => game.startRound()}>Start round</button>
          <button onClick={() => article && game.setArticle(article!)}>Set Article</button> */}
        </Container>
      </SwipableEdgeDrawer>
      {/* <div>{JSON.stringify(game.state)}</div> */}
    </Root>
  );
}

interface WikipediaApiResult {
  id: string;
  type: string;
  title?: string;
  description?: string;
  extract?: string;
}

function ArticlePicker() {
  const game = useGame();

  const [link, setLink] = React.useState("");
  const [result, setResult] = React.useState<WikipediaApiResult | null>();

  React.useEffect(() => {
    var parseResult = wikiLinkParser(link);

    if (parseResult.valid) {
      fetch(`https://en.wikipedia.org/api/rest_v1/page/summary/${parseResult.id}`)
        .then((x) => x.json())
        .then((x) => {
          setResult({ ...x, id: parseResult.id });
        });
    }
  }, [link]);

  const currentArticle = game?.state?.game?.article;
  if (currentArticle) {
    return (
      <Box sx={{ marginTop: 2 }}>
        <Box sx={{ marginTop: 2, marginBottom: 2 }}>
          <Typography gutterBottom variant="h5" component="div">
            {currentArticle.name}
          </Typography>

          {currentArticle.description && (
            <Typography variant="body2" sx={{ marginBottom: 1 }}>
              {currentArticle.description}
            </Typography>
          )}
          <Typography variant="body2" color="text.secondary">
            {currentArticle.extract}
          </Typography>
        </Box>

        <Button
          size="small"
          variant="contained"
          color="secondary"
          onClick={() => game?.removeArticle()}
        >
          Change
        </Button>
      </Box>
    );
  }

  const success = result?.type === "standard";
  const title = result?.title;
  const description = result?.description;
  const extract = result?.extract;
  const id = result?.id;

  return (
    <Box sx={{ marginTop: 2 }}>
      <TextField
        label="Paste your Wikipedia URL here"
        variant="outlined"
        sx={{ width: "100%" }}
        value={link}
        onChange={(e) => setLink(e.target.value)}
      />

      <Box sx={{ marginTop: 2, marginBottom: 2 }}>
        <Typography gutterBottom variant="h5" component="div">
          {title}
        </Typography>

        {description && (
          <Typography variant="body2" sx={{ marginBottom: 1 }}>
            {description}
          </Typography>
        )}
        <Typography variant="body2" color="text.secondary">
          {extract}
        </Typography>
      </Box>

      <Button
        size="small"
        onClick={() =>
          game?.setArticle({
            id: id!,
            name: title!,
            description: description!,
            extract: extract!,
          })
        }
        disabled={!success}
        variant="contained"
      >
        Save
      </Button>
    </Box>
  );
}
