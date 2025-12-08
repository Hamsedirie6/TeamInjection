import { Link, useNavigate } from "react-router-dom";

export default function Navbar() {
  const navigate = useNavigate();
  const username = localStorage.getItem("username");

  const handleLogout = () => {
    localStorage.removeItem("jwt");
    localStorage.removeItem("username");
    localStorage.removeItem("userId");
    navigate("/login");
  };

  return (
    <nav className="nav">
      <div className="nav-left">
        <Link to="/" className="brand">
          Social
        </Link>
        <Link to="/">Inlägg</Link>
        <Link to="/dm">DM</Link>
        <Link to="/follow">Följ</Link>
      </div>
      <div className="nav-right">
        {username ? (
          <>
            <span className="user-chip">{username}</span>
            <button onClick={handleLogout}>Logga ut</button>
          </>
        ) : (
          <>
            <Link to="/login">Logga in</Link>
            <Link to="/register">Registrera</Link>
          </>
        )}
      </div>
    </nav>
  );
}
