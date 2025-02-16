import { Link } from "react-router-dom";
import {
  FaClipboardList,
  FaUsers,
  FaProjectDiagram,
  Fa500Px,
  FaAd,
} from "react-icons/fa";
import { Fa0, Fa42Group, Fa9, FaArrowsToDot } from "react-icons/fa6";

const Sidebar = () => {
  return (
    <div
      style={{
        width: "250px",
        height: "100vh",
        backgroundColor: "#4A4A8C",
        color: "white",
        padding: "20px",
        position: "fixed",
        left: 0,
        top: 0,
        display: "flex",
        flexDirection: "column",
        boxShadow: "2px 0 5px rgba(0,0,0,0.2)",
      }}
    >
      <h2 style={{ textAlign: "center", marginBottom: "30px" }}>Task Forge</h2>
      <ul style={{ listStyle: "none", padding: 0 }}>
        <li style={{ marginBottom: "15px" }}>
          <Link
            to="/dashboard"
            style={{
              textDecoration: "none",
              color: "white",
              display: "flex",
              alignItems: "center",
            }}
          >
            <FaClipboardList style={{ marginRight: "10px" }} /> Dashboard
          </Link>
        </li>
        <li style={{ marginBottom: "15px" }}>
          <Link
            to="/job-queue"
            style={{
              textDecoration: "none",
              color: "white",
              display: "flex",
              alignItems: "center",
            }}
          >
            <FaProjectDiagram style={{ marginRight: "10px" }} /> Job Queue
          </Link>
        </li>
        <li style={{ marginBottom: "15px" }}>
          <Link
            to="/crews"
            style={{
              textDecoration: "none",
              color: "white",
              display: "flex",
              alignItems: "center",
            }}
          >
            <FaArrowsToDot style={{ marginRight: "10px" }} /> Crews
          </Link>
        </li>

        <li style={{ marginBottom: "15px" }}>
          <Link
            to="/users"
            style={{
              textDecoration: "none",
              color: "white",
              display: "flex",
              alignItems: "center",
            }}
          >
            <FaUsers style={{ marginRight: "10px" }} /> Users
          </Link>
        </li>
      </ul>
    </div>
  );
};

export default Sidebar;
