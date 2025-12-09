import { useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../api/api";

type ApiError = {
  response?: { data?: { error?: string; message?: string } };
  message?: string;
};

export default function Register() {
  const navigate = useNavigate();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const register = async () => {
    setError("");
    setLoading(true);
    try {
      await api.post("/user/create", { username, password });
      navigate("/login");
    } catch (err: unknown) {
      const apiErr = err as ApiError;
      const message =
        apiErr.response?.data?.error ||
        apiErr.response?.data?.message ||
        apiErr.message ||
        "Registrering misslyckades";
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
            <p className="eyebrow">Ny här?</p>
            <h1 className="page-title">Registrera dig</h1>
            <p className="page-subtitle">Skapa ett konto och börja dela.</p>
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
            autoComplete="new-password"
          />
          <button className="btn btn-primary" onClick={register} disabled={loading}>
            {loading ? "Registrerar..." : "Registrera"}
          </button>
          <p className="muted text-center mb-0">
            Har du redan ett konto?{" "}
            <a className="link" href="/login">
              Logga in
            </a>
          </p>
        </div>
      </div>
    </section>
  );
}
