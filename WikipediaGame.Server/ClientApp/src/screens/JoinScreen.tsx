import React from "react";
import { useGame } from "../game";

export default function JoinScreen() {
  const game = useGame();

  const [name, setName] = React.useState("");
  const [gameCode, setGameCode] = React.useState("");

  return (
    <div>
      <div>Create game</div>
      <input
        value={name}
        onChange={(e) => setName(e.target.value)}
        placeholder="What is  your name?"
      />
      <button onClick={() => game?.createGame(name)}>Create</button>
      <div>Or join game</div>
      <input
        value={gameCode}
        onChange={(e) => setGameCode(e.target.value)}
        placeholder="Gamecode"
      />
      <button onClick={() => game?.joinGame(name, gameCode)}>Join</button>
    </div>
  );
}
