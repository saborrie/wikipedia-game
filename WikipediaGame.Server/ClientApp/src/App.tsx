import CssBaseline from "@mui/material/CssBaseline";
import { ConnectionProvider } from "./connection";
import { GameProvider, useGame } from "./game";

import GameScreen from "./screens/GameScreen";
import JoinScreen from "./screens/JoinScreen";

function Router() {
  const game = useGame();

  if (!game?.connected) {
    return <div>Connecting...</div>;
  }

  if (game?.state?.inGame) {
    return <GameScreen />;
  }

  return <JoinScreen />;
}

function App() {
  return (
    <>
      <CssBaseline />
      <ConnectionProvider>
        <GameProvider>
          <Router />
        </GameProvider>
      </ConnectionProvider>
    </>
  );
}

export default App;
