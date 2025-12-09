import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../api/api";
import { fetchUsers, toUserMap, type User } from "../api/users";

type Post = {
  id: number;
  message: string;
  fromUserId: number;
  toUserId: number;
  fromUsername?: string;
  toUsername?: string;
  createdAt: string;
};

type ApiError = {
  response?: { data?: { error?: string; message?: string } };
  message?: string;
};

export default function Timeline() {
  const navigate = useNavigate();
  const [posts, setPosts] = useState<Post[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [error, setError] = useState("");
  const userId = localStorage.getItem("userId");

  const userMap = useMemo(() => toUserMap(users), [users]);

  const loadUsers = async () => {
    try {
      const all = await fetchUsers();
      setUsers(all);
    } catch {
      // ignore for now
    }
  };

  const loadTimeline = async () => {
    setError("");
    try {
      const res = await api.get("/post/timeline");
      setPosts(res.data);
    } catch (err: unknown) {
      const apiErr = err as ApiError;
      const msg =
        apiErr.response?.data?.error ||
        apiErr.response?.data?.message ||
        "Kunde inte hämta tidslinje";
      setError(msg);
    }
  };

  useEffect(() => {
    if (!userId) {
      navigate("/login");
      return;
    }
    // eslint-disable-next-line react-hooks/set-state-in-effect
    loadUsers();
    // eslint-disable-next-line react-hooks/set-state-in-effect
    loadTimeline();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [userId, navigate]);

  return (
    <section className="page timeline-page">
      <header className="page-header">
        <div>
          <p className="eyebrow">Tidslinje</p>
          <h1 className="page-title">Inlägg från personer du följer</h1>
          <p className="page-subtitle">Se vad ditt nätverk delar just nu.</p>
        </div>
      </header>
      {error && <p className="text-danger fw-semibold">{error}</p>}
      <div className="card feed-card">
        <div className="card-header">
          <h2 className="card-title">Flöde</h2>
        </div>
        <div className="card-body">
          <ul className="list">
            {posts.map((p) => (
              <li key={p.id} className="list-item column post-card-item">
                <div className="list-top">
                  <div className="d-flex align-items-center gap-2">
                    <span className="pill">{p.fromUsername || userMap[p.fromUserId] || p.fromUserId}</span>
                  </div>
                  <small className="muted">{new Date(p.createdAt).toLocaleString()}</small>
                </div>
                <p className="post-message">{p.message}</p>
              </li>
            ))}
            {posts.length === 0 && (
              <li className="list-item">Inga inlägg ännu från de du följer.</li>
            )}
          </ul>
        </div>
      </div>
    </section>
  );
}
