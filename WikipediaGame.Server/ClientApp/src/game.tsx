import React from "react";
import { useConnection } from "./connection";

export interface Article {
  id: string;
  name: string;
  description: string;
  extract: string;
}

export interface ConnectionUpdate {
  inGame: boolean;
  game?: GameView;
}

export interface GameView {
  gameCode?: string;
  players?: PlayerView[];
  events?: string[];
  inPlay: boolean;
  revealed: boolean;
  answer?: AnswerView;
  article?: Article;
  clue?: string;
  options?: string[];
  username: string;
}

export interface PlayerView {
  name?: string;
  isGuesser: boolean;
  hasArticle: boolean;
}

export interface ArticleView {
  id?: string;
  name?: string;
  description?: string;
  extract?: string;
}

export interface AnswerView {
  username?: string;
  article?: ArticleView;
}

export interface Game {
  connected: boolean;
  state?: ConnectionUpdate;
  createGame: (name: string) => void;
  joinGame: (name: string, gameCode: string) => void;
  leaveGame: () => void;
  becomeGuesser: () => void;
  reset: () => void;
  startRound: () => void;
  makeGuess: (username: string) => void;
  setArticle: (article: Article) => void;
  removeArticle: () => void;
}

const GameContext = React.createContext<Game | null>(null);

export function GameProvider({ children }: { children: React.ReactNode }) {
  const connection = useConnection();

  const [state, setState] = React.useState<ConnectionUpdate | undefined>();

  React.useEffect(() => {
    if (connection) {
      connection.on("Update", (data: ConnectionUpdate) => {
        setState(data);
      });
    }
  }, [connection]);

  const game: Game = {
    connected: Boolean(connection),
    state,
    createGame: (name: string) => connection?.send("CreateGame", name),
    joinGame: (name: string, gameCode: string) => connection?.send("JoinGame", name, gameCode),
    leaveGame: () => connection?.send("LeaveGame"),
    becomeGuesser: () => connection?.send("BecomeGuesser"),
    reset: () => connection?.send("Reset"),
    startRound: () => connection?.send("StartRound"),
    makeGuess: (username: string) => connection?.send("MakeGuess", username),
    setArticle: ({ id, name, description, extract }: Article) =>
      connection?.send("SetArticle", id, name, description, extract),
    removeArticle: () => connection?.send("RemoveArticle"),
  };

  return <GameContext.Provider value={game}>{children}</GameContext.Provider>;
}

export function useGame(): Game | null {
  return React.useContext(GameContext);
}
