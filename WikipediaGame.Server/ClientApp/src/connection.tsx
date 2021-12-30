import { HubConnectionBuilder, HubConnection } from "@microsoft/signalr";
import React, { ReactNode } from "react";

const ConnectionContext = React.createContext<HubConnection | null>(null);

export function ConnectionProvider({ children }: { children: ReactNode }) {
  const [connection, setConnection] = React.useState<HubConnection | null>(null);

  React.useEffect(() => {
    async function setupConnection() {
      const newConnection = new HubConnectionBuilder()
        .withUrl("/hubs/play")
        .withAutomaticReconnect()
        .build();

      await newConnection.start();
      setConnection(newConnection);
    }

    setupConnection();
  }, []);

  return <ConnectionContext.Provider value={connection}>{children}</ConnectionContext.Provider>;
}

export function useConnection(): HubConnection | null {
  return React.useContext(ConnectionContext);
}
