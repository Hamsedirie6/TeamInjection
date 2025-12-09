import { NavLink, useNavigate } from "react-router-dom";

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
    <nav className="nav app-navbar">
      <div className="nav-left">
        <NavLink to="/" className="brand">
          Social
        </NavLink>
        <NavLink to="/posts" className={({ isActive }) => `nav-link ${isActive ? "active" : ""}`}>
          Skapa inlägg
        </NavLink>
        <NavLink to="/timeline" className={({ isActive }) => `nav-link ${isActive ? "active" : ""}`}>
          Tidslinje
        </NavLink>
        <NavLink to="/dm" className={({ isActive }) => `nav-link ${isActive ? "active" : ""}`}>
          DM
        </NavLink>
        <NavLink to="/follow" className={({ isActive }) => `nav-link ${isActive ? "active" : ""}`}>
          Följ
        </NavLink>
      </div>
      <div className="nav-right">
        {username ? (
          <>
            <span className="user-chip">{username}</span>
            <button className="btn btn-outline-light btn-sm" onClick={handleLogout}>
              Logga ut
            </button>
          </>
        ) : (
          <>
            <NavLink to="/login" className="btn btn-outline-light btn-sm">
              Logga in
            </NavLink>
            <NavLink to="/register" className="btn btn-primary btn-sm">
              Registrera
            </NavLink>
          </>
        )}
      </div>
    </nav>
  );
}
