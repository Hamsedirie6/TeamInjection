import { useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../api/api";

export default function Login() {
  const navigate = useNavigate();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const login = async () => {
    setError("");
    setLoading(true);
    try {
      const res = await api.post("/user/login", { username, password });
      localStorage.setItem("jwt", res.data.token);
      localStorage.setItem("username", res.data.username);
      localStorage.setItem("userId", String(res.data.userId));
      navigate("/");
    } catch (err: any) {
      const message =
        err.response?.data?.error ||
        err.response?.data?.message ||
        "Login misslyckades";
      setError(message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="card">
      <h1>Logga in</h1>
      {error && <p className="error">{error}</p>}
      <input
        placeholder="Användarnamn"
        value={username}
        onChange={(e) => setUsername(e.target.value)}
      />
      <input
        placeholder="Lösenord"
        type="password"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
      />
      <button onClick={login} disabled={loading}>
        {loading ? "Loggar in..." : "Logga in"}
      </button>
    </div>
  );
}
