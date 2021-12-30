import React from "react";
import logo from "./logo.svg";
import "./App.css";
import { ConnectionProvider, useConnection } from "./connection";

function Form() {
  const connection = useConnection();

  const [name, setName] = React.useState("");
  const [log, setLog] = React.useState<any[]>([]);

  function handleClick() {
    connection?.send("SayHello", name);
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
      <input
        value={name}
        onChange={(e) => setName(e.target.value)}
        placeholder="What is  your name?"
      />
      <button onClick={handleClick}>Send</button>
      <div>{JSON.stringify(log)}</div>
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
