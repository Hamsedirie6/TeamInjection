import { BrowserRouter, Route, Routes } from "react-router-dom";
import "./App.css";
import Navbar from "./components/Navbar";
import DirectMessages from "./pages/DirectMessages";
import Follow from "./pages/Follow";
import Login from "./pages/Login";
import Posts from "./pages/Posts";
import Register from "./pages/Register";
import Timeline from "./pages/Timeline";

function App() {
  return (
    <BrowserRouter>
      <div className="app-shell">
        <Navbar />
        <main>
          <Routes>
            <Route path="/" element={<Posts />} />
            <Route path="/timeline" element={<Timeline />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/dm" element={<DirectMessages />} />
            <Route path="/follow" element={<Follow />} />
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  );
}

export default App;
