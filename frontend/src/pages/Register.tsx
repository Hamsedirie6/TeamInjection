import { useState } from "react";
import api from "../api/api";

export default function Register() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const register = async () => {
    setMessage("");
    setError("");
    setLoading(true);
    try {
      await api.post("/user/create", { username, password });
      setMessage("Konto skapat! Du kan nu logga in.");
    } catch (err: any) {
      const msg =
        err.response?.data?.error ||
        err.response?.data?.message ||
        err.message ||
        "Registrering misslyckades";
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="card">
      <h1>Skapa konto</h1>
      {error && <p className="error">{error}</p>}
      {message && <p className="success">{message}</p>}
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
      <button onClick={register} disabled={loading}>
        {loading ? "Registrerar..." : "Registrera"}
      </button>
    </div>
  );
}
