import React from "react";
import logo from "./logo.svg";
import "./App.css";
import { ConnectionProvider, useConnection } from "./connection";

function Form() {
  const connection = useConnection();

  const [name, setName] = React.useState("");
  const [gameCode, setGameCode] = React.useState("");
  const [log, setLog] = React.useState<any[]>([]);

  function handleCreate() {
    connection?.send("CreateGame", name);
  }
  function handleJoin() {
    connection?.send("JoinGame", name, gameCode);
  }
  function handleLeave() {
    connection?.send("LeaveGame");
  }

  React.useEffect(() => {
    if (connection) {
      connection.on("Update", (data: any) => {
        setLog((l) => [...l, data]);
      });
    }
  }, [connection]);

  return (
    <div>
      <div>Create game</div>
      <input
        value={name}
        onChange={(e) => setName(e.target.value)}
        placeholder="What is  your name?"
      />
      <button onClick={handleCreate}>Create</button>
      <div>Or join game</div>
      <input
        value={gameCode}
        onChange={(e) => setGameCode(e.target.value)}
        placeholder="Gamecode"
      />
      <button onClick={handleJoin}>Join</button>
      <button onClick={handleLeave}>Leave</button>

      {log.map((x) => (
        <div>{JSON.stringify(x)}</div>
      ))}
    </div>
  );
}

function App() {
  return (
    <ConnectionProvider>
      <Form />
    </ConnectionProvider>
  );
}

export default App;
