import { useEffect, useMemo, useState } from "react";
import api from "../api/api";
import { fetchUsers, toUserMap, type User } from "../api/users";

export default function Follow() {
  const [users, setUsers] = useState<User[]>([]);
  const [search, setSearch] = useState("");
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [following, setFollowing] = useState<number[]>([]);
  const [friends, setFriends] = useState<number[]>([]);
  const userMap = useMemo(() => toUserMap(users), [users]);
  const currentUserId = localStorage.getItem("userId");

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

  const loadFollowing = async (userId: number) => {
    try {
      const res = await api.get(`/follow/following/${userId}`);
      setFollowing(res.data.map((f: any) => f.followedId));
    } catch (err: any) {
      const msg =
        err.response?.data?.error ||
        err.response?.data?.message ||
        "Kunde inte hämta följda";
      setError(msg);
    }
  };

  const loadFriends = async () => {
    try {
      const res = await api.get(`/follow/friends`);
      setFriends(res.data as number[]);
    } catch (err: any) {
      const msg =
        err.response?.data?.error ||
        err.response?.data?.message ||
        "Kunde inte hämta vänner";
      setError(msg);
    }
  };

  useEffect(() => {
    loadUsers();
    if (currentUserId) {
      loadFollowing(Number(currentUserId));
      loadFriends();
    }
  }, [currentUserId]);

  const visibleUsers = users.filter((u) =>
    u.username.toLowerCase().includes(search.trim().toLowerCase())
  );

  const followUser = async (userId: number, username: string) => {
    setError("");
    setMessage("");
    try {
      await api.post("/follow", { followedId: userId });
      setMessage(`Du följer nu ${username}`);
      if (currentUserId) {
        loadFollowing(Number(currentUserId));
        loadFriends();
      }
    } catch (err: any) {
      const msg =
        err.response?.data?.error ||
        err.response?.data?.message ||
        "Kunde inte följa användare";
      setError(msg);
    }
  };

  const unfollowUser = async (userId: number, username: string) => {
    setError("");
    setMessage("");
    try {
      await api.delete(`/follow/${userId}`);
      setMessage(`Du slutade följa ${username}`);
      if (currentUserId) {
        loadFollowing(Number(currentUserId));
        loadFriends();
      }
    } catch (err: any) {
      const msg =
        err.response?.data?.error ||
        err.response?.data?.message ||
        "Kunde inte avfölja användare";
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
              {following.includes(u.id) ? (
                <button
                  className="danger"
                  onClick={() => unfollowUser(u.id, u.username)}
                >
                  Avfölj
                </button>
              ) : (
                <button onClick={() => followUser(u.id, u.username)}>
                  Följ
                </button>
              )}
              <button onClick={() => loadFollowers(u.id)}>Följare</button>
            </div>
          </div>
        ))}
      </div>

      <div className="stack">
        <h3>Jag följer</h3>
        <p className="muted">
          {following.length > 0
            ? following.map((id) => `@${userMap[id] ?? id}`).join(", ")
            : "Du följer ingen ännu."}
        </p>

        <h3>Mina vänner (följer varandra)</h3>
        <p className="muted">
          {friends.length > 0
            ? friends.map((id) => `@${userMap[id] ?? id}`).join(", ")
            : "Inga mutuals ännu."}
        </p>
      </div>
    </div>
  );
}
