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
    <section className="page posts-page">
      <header className="page-header">
        <div>
          <p className="eyebrow">Flöde</p>
          <h1 className="page-title">Mina inlägg</h1>
          <p className="page-subtitle">Dela något nytt och håll koll på ditt flöde.</p>
        </div>
        {username && <span className="pill subtle">Inloggad som {username}</span>}
      </header>

      {error && <p className="text-danger fw-semibold">{error}</p>}

      <div className="card composer-card">
        <div className="card-header">
          <h2 className="card-title">Skriv ett inlägg</h2>
        </div>
        <div className="card-body">
          <textarea
            placeholder="Vad vill du dela?"
            value={message}
            onChange={(e) => setMessage(e.target.value)}
            rows={4}
            className="w-100"
          />
          <div className="row end mt-2">
            <button className="btn btn-primary" onClick={createPost}>
              Publicera
            </button>
          </div>
        </div>
      </div>

      <div className="card feed-card">
        <div className="card-header">
          <h2 className="card-title">Dina inlägg</h2>
        </div>
        <div className="card-body">
          <ul className="list">
            {posts.map((p) => (
              <li key={p.id} className="list-item column post-card-item">
                <div className="list-top">
                  <div className="d-flex align-items-center gap-2">
                    <span className="pill">{p.fromUsername || userMap[p.fromUserId] || p.fromUserId}</span>
                    <small className="muted">
                      {p.fromUsername || userMap[p.fromUserId] || p.fromUserId} →{" "}
                      {p.toUsername || userMap[p.toUserId] || p.toUserId}
                    </small>
                  </div>
                  {String(p.fromUserId) === userId && (
                    <button
                      className="btn btn-outline-danger btn-sm"
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
      </div>
    </section>
  );
}
