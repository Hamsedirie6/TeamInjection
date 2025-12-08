import { useState } from "react";
import api from "../api/api";

type FollowRow = {
  id: number;
  followerId: number;
  followedId: number;
};

export default function Follow() {
  const [targetId, setTargetId] = useState("");
  const [followers, setFollowers] = useState<FollowRow[]>([]);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  const followUser = async () => {
    setError("");
    setMessage("");
    if (!targetId.trim()) return;
    try {
      await api.post("/follow", { followedId: Number(targetId) });
      setMessage(`Du följer nu användare ${targetId}`);
    } catch (err: any) {
      const msg =
        err.response?.data?.error ||
        err.response?.data?.message ||
        "Kunde inte följa användare";
      setError(msg);
    }
  };

  const loadFollowers = async () => {
    setError("");
    setMessage("");
    if (!targetId.trim()) return;
    try {
      const res = await api.get(`/follow/followers/${targetId}`);
      setFollowers(res.data);
    } catch (err: any) {
      const msg =
        err.response?.data?.error ||
        err.response?.data?.message ||
        "Kunde inte hämta följare";
      setError(msg);
    }
  };

  return (
    <div className="card">
      <h1>Följ & Följare</h1>
      {error && <p className="error">{error}</p>}
      {message && <p className="success">{message}</p>}
      <div className="stack">
        <input
          placeholder="Användar-ID"
          value={targetId}
          onChange={(e) => setTargetId(e.target.value)}
        />
        <div className="row">
          <button onClick={loadFollowers}>Hämta följare</button>
          <button onClick={followUser}>Följ användare</button>
        </div>
      </div>
      <ul className="list">
        {followers.map((f) => (
          <li key={f.id} className="list-item">
            <span className="pill">#{f.id}</span>
            <p>
              {f.followerId} följer {f.followedId}
            </p>
          </li>
        ))}
      </ul>
    </div>
  );
}
