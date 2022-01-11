import CssBaseline from "@mui/material/CssBaseline";
import { ConnectionProvider } from "./connection";
import { GameProvider, useGame } from "./game";

import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";

import GameScreen from "./screens/GameScreen";
import JoinScreen from "./screens/JoinScreen";
import CreateScreen from "./screens/CreateScreen";
import ConnectingScreen from "./screens/ConnectingScreen";

function Router() {
  const game = useGame();

  if (!game?.connected) {
    return <ConnectingScreen />;
  }

  if (game?.state?.inGame) {
    return (
      <Routes>
        <Route path={`/${game?.state?.game?.gameCode}`} element={<GameScreen />} />
        <Route path="*" element={<Navigate replace to={`/${game?.state?.game?.gameCode}`} />} />
      </Routes>
    );

    // <GameScreen />;
  }

  return (
    <Routes>
      <Route path="/:gameCode" element={<JoinScreen />} />
      <Route path="/" element={<CreateScreen />} />
    </Routes>
  );
}

function App() {
  return (
    <>
      <CssBaseline />
      <BrowserRouter>
        <ConnectionProvider>
          <GameProvider>
            <Router />
          </GameProvider>
        </ConnectionProvider>
      </BrowserRouter>
    </>
  );
}

export default App;
