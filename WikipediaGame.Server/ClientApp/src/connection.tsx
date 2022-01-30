import { HubConnectionBuilder, HubConnection } from "@microsoft/signalr";
import React, { ReactNode } from "react";
import makeUUID from "./utilities/makeUUID";

const ConnectionContext = React.createContext<HubConnection | null>(null);

interface HubConnectionWithId extends HubConnection {
  playerId: string;
}

export function ConnectionProvider({ children }: { children: ReactNode }) {
  const [connection, setConnection] = React.useState<HubConnectionWithId | null>(null);

  const playerId = React.useMemo(() => {
    let id = sessionStorage.getItem("playerId");
    if (!id) id = makeUUID();
    sessionStorage.setItem("playerId", id);
    return id;
  }, []);

  React.useEffect(() => {
    async function setupConnection() {
      const newConnection = new HubConnectionBuilder()
        .withUrl("/hubs/play", {
          accessTokenFactory: () => playerId,
        })
        .withAutomaticReconnect()
        .build();

      await newConnection.start();
      setConnection(Object.assign(newConnection, { playerId }));

      newConnection.onclose(() => {
        setConnection(null);
      });

      newConnection.onreconnected(() => {
        setConnection(Object.assign(newConnection, { playerId }));
      });

      newConnection.on("Ping", () => newConnection.send("AckPing"));
    }

    setupConnection();
  }, []);

  return <ConnectionContext.Provider value={connection}>{children}</ConnectionContext.Provider>;
}

export function useConnection(): HubConnection | null {
  return React.useContext(ConnectionContext);
}
