import { useEffect, useMemo, useState } from "react";
import api from "../api/api";
import { fetchUsers, toUserMap, type User } from "../api/users";

export default function Follow() {
  const [users, setUsers] = useState<User[]>([]);
  const [search, setSearch] = useState("");
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const userMap = useMemo(() => toUserMap(users), [users]);

  const loadUsers = async () => {
    try {
      const all = await fetchUsers();
      setUsers(all);
    } catch (err: any) {
      const msg =
        err.response?.data?.error ||
        err.response?.data?.message ||
        err.message ||
        "Kunde inte hämta användare";
      setError(msg);
    }
  };

  useEffect(() => {
    loadUsers();
  }, []);

  const visibleUsers = users.filter((u) =>
    u.username.toLowerCase().includes(search.trim().toLowerCase())
  );

  const followUser = async (userId: number, username: string) => {
    setError("");
    setMessage("");
    try {
      await api.post("/follow", { followedId: userId });
      setMessage(`Du följer nu ${username}`);
    } catch (err: any) {
      const msg =
        err.response?.data?.error ||
        err.response?.data?.message ||
        "Kunde inte följa användare";
      setError(msg);
    }
  };

  const loadFollowers = async (userId: number) => {
    try {
      const res = await api.get(`/follow/followers/${userId}`);
      const followerNames = res.data
        .map((f: any) => userMap[f.followerId] ?? f.followerId)
        .join(", ");
      setMessage(
        followerNames
          ? `${userMap[userId] ?? userId} har följare: ${followerNames}`
          : `${userMap[userId] ?? userId} har inga följare ännu`
      );
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
      <div className="card-header">
        <div>
          <p className="eyebrow">Nätverk</p>
          <h1>Följ användare</h1>
        </div>
        <input
          placeholder="Sök användare..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </div>

      {error && <p className="error">{error}</p>}
      {message && <p className="success">{message}</p>}

      <div className="user-grid">
        {visibleUsers.map((u) => (
          <div key={u.id} className="user-card">
            <div>
              <p className="username">@{u.username}</p>
              <p className="muted">ID: {u.id}</p>
            </div>
            <div className="row">
              <button onClick={() => followUser(u.id, u.username)}>
                Följ
              </button>
              <button onClick={() => loadFollowers(u.id)}>Följare</button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
