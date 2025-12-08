import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../api/api";

type DirectMessage = {
  id: number;
  senderId: number;
  receiverId: number;
  message: string;
};

export default function DirectMessages() {
  const navigate = useNavigate();
  const [receiverId, setReceiverId] = useState("");
  const [message, setMessage] = useState("");
  const [conversation, setConversation] = useState<DirectMessage[]>([]);
  const [error, setError] = useState("");
  const senderId = localStorage.getItem("userId");

  const loadConversation = async () => {
    if (!senderId || !receiverId) return;
    try {
      const res = await api.get(
        `/directmessage/conversation/${senderId}/${receiverId}`
      );
      setConversation(res.data);
    } catch (err: any) {
      const msg =
        err.response?.data?.error ||
        err.response?.data?.message ||
        "Kunde inte hämta konversation";
      setError(msg);
    }
  };

  const sendMessage = async () => {
    if (!senderId) {
      navigate("/login");
      return;
    }
    if (!receiverId || !message.trim()) return;
    setError("");
    try {
      await api.post("/directmessage", {
        receiverId: Number(receiverId),
        message,
      });
      setMessage("");
      await loadConversation();
    } catch (err: any) {
      const msg =
        err.response?.data?.error ||
        err.response?.data?.message ||
        "Kunde inte skicka meddelande";
      setError(msg);
    }
  };

  useEffect(() => {
    if (!senderId) {
      navigate("/login");
      return;
    }
  }, [senderId, navigate]);

  useEffect(() => {
    loadConversation();
  }, [receiverId]);

  return (
    <div className="card">
      <h1>Direktmeddelanden</h1>
      {error && <p className="error">{error}</p>}
      <div className="stack">
        <input
          placeholder="Mottares ID"
          value={receiverId}
          onChange={(e) => setReceiverId(e.target.value)}
        />
        <input
          placeholder="Meddelande"
          value={message}
          onChange={(e) => setMessage(e.target.value)}
        />
        <button onClick={sendMessage}>Skicka</button>
      </div>
      <ul className="list">
        {conversation.map((m) => (
          <li key={m.id} className="list-item">
            <span className="pill">
              {m.senderId} → {m.receiverId}
            </span>
            <p>{m.message}</p>
          </li>
        ))}
      </ul>
    </div>
  );
}
