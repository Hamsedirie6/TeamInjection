import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../api/api";
import {
  fetchUsers,
  resolveUserIdByUsername,
  toUserMap,
  type User,
} from "../api/users";

type DirectMessage = {
  id: number;
  senderId: number;
  receiverId: number;
  message: string;
};

export default function DirectMessages() {
  const navigate = useNavigate();
  const [receiverUsername, setReceiverUsername] = useState("");
  const [message, setMessage] = useState("");
  const [conversation, setConversation] = useState<DirectMessage[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [error, setError] = useState("");
  const senderId = localStorage.getItem("userId");

  const userMap = useMemo(() => toUserMap(users), [users]);

  const loadUsers = async () => {
    try {
      const all = await fetchUsers();
      setUsers(all);
    } catch {
      // ignore silently for now
    }
  };

  const loadConversation = async () => {
    if (!senderId || !receiverUsername.trim()) return;
    try {
      const receiverId = await resolveUserIdByUsername(receiverUsername);
      if (!receiverId) {
        setError("Kunde inte hitta mottagare med det användarnamnet");
        return;
      }
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
    if (!receiverUsername.trim() || !message.trim()) return;
    setError("");
    try {
      const receiverId = await resolveUserIdByUsername(receiverUsername);
      if (!receiverId) {
        setError("Kunde inte hitta mottagare med det användarnamnet");
        return;
      }
      await api.post("/directmessage", {
        receiverId,
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
    loadUsers();
  }, [senderId, navigate]);

  useEffect(() => {
    loadConversation();
  }, [receiverUsername]);

  return (
    <div className="card">
      <h1>Direktmeddelanden</h1>
      {error && <p className="error">{error}</p>}
      <div className="stack">
        <input
          placeholder="Mottares användarnamn"
          value={receiverUsername}
          onChange={(e) => setReceiverUsername(e.target.value)}
          list="user-suggestions"
        />
        <datalist id="user-suggestions">
          {users.map((u) => (
            <option key={u.id} value={u.username} />
          ))}
        </datalist>
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
              {userMap[m.senderId] ?? m.senderId} →{" "}
              {userMap[m.receiverId] ?? m.receiverId}
            </span>
            <p>{m.message}</p>
          </li>
        ))}
      </ul>
    </div>
  );
}
