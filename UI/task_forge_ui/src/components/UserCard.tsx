import React from "react";
import { updateUser, deleteUser } from "../services/userService";

interface User {
  id: number;
  name: string;
  role: string;
}

const UserCard: React.FC<{ user: User }> = ({ user }) => {
  return (
    <div className="card p-3 shadow-sm">
      <h4>{user.name}</h4>
      <p>
        <strong>Role:</strong> {user.role}
      </p>

      <div className="d-flex gap-2">
        <button
          className="btn btn-warning"
          onClick={() => updateUser(user.id, { name: user.name })}
        >
          Edit (Stub)
        </button>
        <button className="btn btn-danger" onClick={() => deleteUser(user.id)}>
          Delete (Stub)
        </button>
      </div>
    </div>
  );
};

export default UserCard;
