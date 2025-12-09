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
  const [followers, setFollowers] = useState<number[]>([]);
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

  const loadMyFollowers = async (userId: number) => {
    try {
      const res = await api.get(`/follow/followers/${userId}`);
      setFollowers(res.data.map((f: any) => f.followerId));
    } catch (err: any) {
      const msg =
        err.response?.data?.error ||
        err.response?.data?.message ||
        "Kunde inte hämta följare";
      setError(msg);
    }
  };

  useEffect(() => {
    loadUsers();
    if (currentUserId) {
      const id = Number(currentUserId);
      loadFollowing(id);
      loadFriends();
      loadMyFollowers(id);
    }
  }, [currentUserId]);

  const visibleUsers = users.filter((u) => {
    const matches = u.username.toLowerCase().includes(search.trim().toLowerCase());
    const isSelf = currentUserId ? Number(currentUserId) === u.id : false;
    return matches && !isSelf;
  });

  const followUser = async (userId: number, username: string) => {
    setError("");
    setMessage("");
    try {
      await api.post("/follow", { followedId: userId });
      setMessage(`Du följer nu ${username}`);
      if (currentUserId) {
        const id = Number(currentUserId);
        loadFollowing(id);
        loadFriends();
        loadMyFollowers(id);
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
        const id = Number(currentUserId);
        loadFollowing(id);
        loadFriends();
        loadMyFollowers(id);
      }
    } catch (err: any) {
      const msg =
        err.response?.data?.error ||
        err.response?.data?.message ||
        "Kunde inte avfölja användare";
      setError(msg);
    }
  };

  return (
    <section className="page follow-page">
      <header className="page-header">
        <div>
          <p className="eyebrow">Nätverk</p>
          <h1 className="page-title">Utforska och följ användare</h1>
          <p className="page-subtitle">Bygg ditt nätverk och hitta nya vänner.</p>
        </div>
        <input
          className="search-input"
          placeholder="Sök användare..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </header>

      {error && <p className="text-danger fw-semibold">{error}</p>}
      {message && <p className="text-success fw-semibold">{message}</p>}

      <div className="follow-grid four-cols">
        <div className="card">
          <div className="card-header">
            <h2 className="card-title">Mina följare</h2>
          </div>
          <div className="card-body scroll-area">
            <ul className="list">
              {followers.length > 0 ? (
                followers.map((id) => (
                  <li key={id} className="list-item">
                    <span className="pill">@{userMap[id] ?? id}</span>
                  </li>
                ))
              ) : (
                <li className="list-item">Inga följare ännu.</li>
              )}
            </ul>
          </div>
        </div>

        <div className="card">
          <div className="card-header">
            <h2 className="card-title">Jag följer</h2>
          </div>
          <div className="card-body scroll-area">
            <ul className="list">
              {following.length > 0 ? (
                following.map((id) => (
                  <li key={id} className="list-item">
                    <span className="pill">@{userMap[id] ?? id}</span>
                  </li>
                ))
              ) : (
                <li className="list-item">Du följer ingen ännu.</li>
              )}
            </ul>
          </div>
        </div>

        <div className="card">
          <div className="card-header">
            <h2 className="card-title">Gemensamma vänner</h2>
          </div>
          <div className="card-body scroll-area">
            <ul className="list">
              {friends.length > 0 ? (
                friends.map((id) => (
                  <li key={id} className="list-item">
                    <span className="pill">@{userMap[id] ?? id}</span>
                  </li>
                ))
              ) : (
                <li className="list-item">Inga mutuals ännu.</li>
              )}
            </ul>
          </div>
        </div>

        <div className="card">
          <div className="card-header">
            <h2 className="card-title">Alla användare</h2>
          </div>
          <div className="card-body scroll-area">
            <div className="user-grid">
              {visibleUsers.map((u) => (
                <div key={u.id} className="user-card">
                  <div>
                    <p className="username">@{u.username}</p>
                  </div>
                  <div className="row">
                    {following.includes(u.id) ? (
                      <button
                        className="btn btn-outline-danger btn-sm"
                        onClick={() => unfollowUser(u.id, u.username)}
                      >
                        Avfölj
                      </button>
                    ) : (
                      <button
                        className="btn btn-primary btn-sm"
                        onClick={() => followUser(u.id, u.username)}
                      >
                        Följ
                      </button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
