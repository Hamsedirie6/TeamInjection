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
      navigate("/posts");
    } catch (err: any) {
      const message =
        err.response?.data?.error ||
        err.response?.data?.message ||
        err.message ||
        "Login misslyckades";
      setError(message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <section className="page auth-page">
      <div className="card auth-card">
        <div className="card-header">
          <div>
            <p className="eyebrow">Välkommen</p>
            <h1 className="page-title">Logga in</h1>
            <p className="page-subtitle">Fortsätt till ditt flöde.</p>
          </div>
        </div>
        <div className="card-body stack">
          {error && <p className="text-danger fw-semibold">{error}</p>}
          <input
            placeholder="Användarnamn"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            autoComplete="username"
          />
          <input
            placeholder="Lösenord"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            autoComplete="current-password"
          />
          <button className="btn btn-primary" onClick={login} disabled={loading}>
            {loading ? "Loggar in..." : "Logga in"}
          </button>
          <p className="muted text-center mb-0">
            Har du inget konto?{" "}
            <a className="link" href="/register">
              Registrera dig
            </a>
          </p>
        </div>
      </div>
    </section>
  );
}
