import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../api/api";

type Post = {
  id: number;
  message: string;
  fromUserId: number;
  toUserId: number;
};

export default function Posts() {
  const navigate = useNavigate();
  const [message, setMessage] = useState("");
  const [posts, setPosts] = useState<Post[]>([]);
  const [error, setError] = useState("");
  const userId = localStorage.getItem("userId");

  const loadPosts = async () => {
    if (!userId) return;
    try {
      const res = await api.get(`/post/${userId}`);
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
        fromUserId: Number(userId),
        toUserId: Number(userId),
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

  useEffect(() => {
    if (!userId) {
      navigate("/login");
      return;
    }
    loadPosts();
  }, [userId]);

  return (
    <div className="card">
      <h1>Mina inlägg</h1>
      {error && <p className="error">{error}</p>}
      <div className="stack">
        <input
          placeholder="Skriv ett inlägg..."
          value={message}
          onChange={(e) => setMessage(e.target.value)}
        />
        <button onClick={createPost}>Publicera</button>
      </div>
      <ul className="list">
        {posts.map((p) => (
          <li key={p.id} className="list-item">
            <span className="pill">#{p.id}</span>
            <div>
              <p>{p.message}</p>
              <small>
                Från: {p.fromUserId} → Till: {p.toUserId}
              </small>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
}
