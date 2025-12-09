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
};

export default function Posts() {
  const navigate = useNavigate();
  const [message, setMessage] = useState("");
  const [posts, setPosts] = useState<Post[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [error, setError] = useState("");
  const userId = localStorage.getItem("userId");
  const username = localStorage.getItem("username");

  const userMap = useMemo(() => toUserMap(users), [users]);

  const loadUsers = async () => {
    try {
      const all = await fetchUsers();
      setUsers(all);
    } catch {
      // no-op
    }
  };

  const loadPosts = async () => {
    if (!userId) return;
    try {
      const res = await api.get("/post/timeline");
      setPosts(res.data);
    } catch (err: any) {
      const msg =
        err.response?.data?.error ||
        err.response?.data?.message ||
        "Kunde inte hämta inlägg";
      setError(msg);
    }
  };

  const createPost = async () => {
    if (!userId) {
      navigate("/login");
      return;
    }
    if (!message.trim()) return;
    setError("");
    try {
      await api.post("/post", {
        message,
      });
      setMessage("");
      await loadPosts();
    } catch (err: any) {
      const msg =
        err.response?.data?.error ||
        err.response?.data?.message ||
        "Kunde inte skapa inlägg";
      setError(msg);
    }
  };

  const deletePost = async (postId: number) => {
    if (!userId) return;
    setError("");
    try {
      await api.delete(`/post/${postId}`);
      await loadPosts();
    } catch (err: any) {
      const msg =
        err.response?.data?.error ||
        err.response?.data?.message ||
        "Kunde inte radera inlägg";
      setError(msg);
    }
  };

  useEffect(() => {
    if (!userId) {
      navigate("/login");
      return;
    }
    loadUsers();
    loadPosts();
  }, [userId]);

  return (
    <div className="card">
      <div className="card-header">
        <div>
          <p className="eyebrow">Flöde</p>
          <h1>Mina inlägg</h1>
        </div>
        {username && <span className="pill">Inloggad som {username}</span>}
      </div>
      {error && <p className="error">{error}</p>}
      <div className="composer">
        <textarea
          placeholder="Vad vill du dela?"
          value={message}
          onChange={(e) => setMessage(e.target.value)}
          rows={3}
        />
        <div className="row end">
          <button onClick={createPost}>Publicera</button>
        </div>
      </div>
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
              {String(p.fromUserId) === userId && (
                <button
                  className="danger small"
                  onClick={() => deletePost(p.id)}
                >
                  Radera
                </button>
              )}
            </div>
            <p className="post-message">{p.message}</p>
          </li>
        ))}
      </ul>
    </div>
  );
}
