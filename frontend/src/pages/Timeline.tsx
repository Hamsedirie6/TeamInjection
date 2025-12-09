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

export default function Timeline() {
  const navigate = useNavigate();
  const [posts, setPosts] = useState<Post[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [error, setError] = useState("");
  const username = localStorage.getItem("username");
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
    } catch (err: any) {
      const msg =
        err.response?.data?.error ||
        err.response?.data?.message ||
        "Kunde inte hämta tidslinje";
      setError(msg);
    }
  };

  useEffect(() => {
    if (!userId) {
      navigate("/login");
      return;
    }
    loadUsers();
    loadTimeline();
  }, [userId, navigate]);

  return (
    <div className="card">
      <div className="card-header">
        <div>
          <p className="eyebrow">Tidslinje</p>
          <h1>Inlägg från personer du följer</h1>
        </div>
        {username && <span className="pill">Inloggad som {username}</span>}
      </div>
      {error && <p className="error">{error}</p>}
      <ul className="list">
        {posts.map((p) => (
          <li key={p.id} className="list-item column">
            <div className="list-top">
              <span className="pill">
                {p.fromUsername || userMap[p.fromUserId] || p.fromUserId}
              </span>
              <small>
                {p.fromUsername || userMap[p.fromUserId] || p.fromUserId} →{" "}
                {p.toUsername || userMap[p.toUserId] || p.toUserId}
              </small>
            </div>
            <p className="post-message">{p.message}</p>
          </li>
        ))}
        {posts.length === 0 && (
          <li className="list-item">Inga inlägg ännu från de du följer.</li>
        )}
      </ul>
    </div>
  );
}
