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
  sentAt: string;
};

type Thread = {
  otherUserId: number;
  lastMessage: DirectMessage;
};

export default function DirectMessages() {
  const navigate = useNavigate();
  const [receiverUsername, setReceiverUsername] = useState("");
  const [selectedUserId, setSelectedUserId] = useState<number | null>(null);
  const [message, setMessage] = useState("");
  const [conversation, setConversation] = useState<DirectMessage[]>([]);
  const [threads, setThreads] = useState<Thread[]>([]);
  const [unread, setUnread] = useState<DirectMessage[]>([]);
  const [lastUnreadAt, setLastUnreadAt] = useState<string | null>(null);
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

  const loadThreads = async () => {
    try {
      const res = await api.get<Thread[]>("/directmessage/threads");
      setThreads(res.data);
    } catch (err: any) {
      const msg =
        err.response?.data?.error ||
        err.response?.data?.message ||
        "Kunde inte hämta konversationer";
      setError(msg);
    }
  };

  const loadConversation = async (otherUserId: number) => {
    if (!senderId) return;
    try {
      const res = await api.get(
        `/directmessage/conversation/${senderId}/${otherUserId}`
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

  const deleteMessage = async (messageId: number) => {
    if (!senderId) return;
    setError("");
    try {
      await api.delete(`/directmessage/${messageId}`);
      if (selectedUserId) await loadConversation(selectedUserId);
      await loadThreads();
    } catch (err: any) {
      const msg =
        err.response?.data?.error ||
        err.response?.data?.message ||
        "Kunde inte radera meddelande";
      setError(msg);
    }
  };

  const loadUnread = async () => {
    try {
      const res = await api.get<DirectMessage[]>("/directmessage/unread", {
        params: lastUnreadAt ? { since: lastUnreadAt } : undefined,
      });

      if (res.data.length > 0) {
        const latest = res.data[res.data.length - 1].sentAt;
        setLastUnreadAt(latest);

        setUnread((prev) => {
          const knownIds = new Set(prev.map((m) => m.id));
          const merged = [...prev];
          res.data.forEach((m) => {
            if (!knownIds.has(m.id)) merged.push(m);
          });
          return merged.sort(
            (a, b) => new Date(a.sentAt).getTime() - new Date(b.sentAt).getTime()
          );
        });

        if (
          selectedUserId &&
          res.data.some(
            (m) =>
              m.senderId === selectedUserId || m.receiverId === selectedUserId
          )
        ) {
          await loadConversation(selectedUserId);
        }
      }
    } catch {
      // ignore polling errors
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
      const receiverId =
        selectedUserId ?? (await resolveUserIdByUsername(receiverUsername));
      if (!receiverId) {
        setError("Kunde inte hitta mottagare med det användarnamnet");
        return;
      }
      await api.post("/directmessage", {
        receiverId,
        message,
      });
      setMessage("");
      setSelectedUserId(receiverId);
      setLastUnreadAt(null);
      await loadConversation(receiverId);
      await loadThreads();
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
    loadThreads();
    loadUnread();
  }, [senderId, navigate]);

  useEffect(() => {
    if (!receiverUsername.trim()) {
      setSelectedUserId(null);
      return;
    }
    resolveUserIdByUsername(receiverUsername).then((id) => {
      setSelectedUserId(id);
      if (id) loadConversation(id);
    });
  }, [receiverUsername]);

  useEffect(() => {
    const interval = setInterval(() => {
      loadUnread();
      loadThreads();
    }, 4000);
    return () => clearInterval(interval);
  }, [selectedUserId, lastUnreadAt]);

  const selectThread = (otherUserId: number) => {
    const username = userMap[otherUserId] ?? String(otherUserId);
    setReceiverUsername(username);
    setSelectedUserId(otherUserId);
    loadConversation(otherUserId);
  };

  return (
    <div className="card">
      <div className="card-header">
        <div>
          <p className="eyebrow">Direktmeddelanden</p>
          <h1>Chattar och notifieringar</h1>
        </div>
      </div>

      {error && <p className="error">{error}</p>}

      <div className="grid two-cols gap">
        <div className="stack">
          <h3>Konversationer</h3>
          <ul className="list">
            {threads.map((t) => (
              <li
                key={t.otherUserId}
                className="list-item clickable"
                onClick={() => selectThread(t.otherUserId)}
              >
                <div className="list-top">
                  <strong>{userMap[t.otherUserId] ?? t.otherUserId}</strong>
                  <small>{new Date(t.lastMessage.sentAt).toLocaleString()}</small>
                </div>
                <p className="muted">{t.lastMessage.message}</p>
              </li>
            ))}
            {threads.length === 0 && (
              <li className="list-item">Inga konversationer än.</li>
            )}
          </ul>

          <h3>Notifieringar</h3>
          <ul className="list">
            {unread.map((m) => (
              <li key={m.id} className="list-item">
                <div className="list-top">
                  <span className="pill">Från {userMap[m.senderId] ?? m.senderId}</span>
                  <small>{new Date(m.sentAt).toLocaleTimeString()}</small>
                </div>
                <p className="muted">{m.message}</p>
              </li>
            ))}
            {unread.length === 0 && (
              <li className="list-item">Inga nya meddelanden.</li>
            )}
          </ul>
        </div>

        <div className="stack">
          <h3>Aktiv chatt</h3>
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

          <ul className="list">
            {conversation.map((m) => (
              <li key={m.id} className="list-item">
                <div className="list-top">
                  <span className="pill">
                    {userMap[m.senderId] ?? m.senderId} → {" "}
                    {userMap[m.receiverId] ?? m.receiverId}
                  </span>
                  <small>{new Date(m.sentAt).toLocaleTimeString()}</small>
                  {String(m.senderId) === senderId && (
                    <button
                      className="danger small"
                      onClick={() => deleteMessage(m.id)}
                    >
                      Radera
                    </button>
                  )}
                </div>
                <p>{m.message}</p>
              </li>
            ))}
            {conversation.length === 0 && (
              <li className="list-item">Ingen konversation vald.</li>
            )}
          </ul>
        </div>
      </div>
    </div>
  );
}
