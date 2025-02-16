import React, { useEffect, useState } from "react";
import { fetchUserRoles } from "../services/dashboardService";
import { useAuth } from "../context/AuthContext";

const UserInfoCard: React.FC = () => {
  const { user } = useAuth();
  const [role, setRole] = useState<string>("");

  useEffect(() => {
    if (user) {
      fetchUserRoles(user.id)
        .then((roles) => {
          if (roles.length > 0) {
            setRole(roles[roles.length - 1].name); // Use last role in list
          }
        })
        .catch(() => setRole("Unknown"));
    }
  }, [user]);

  if (!user) {
    return <p>Loading user information...</p>;
  }

  return (
    <div className="card p-3 shadow-sm">
      <h3>Welcome, {user.name}!</h3>
      <p>
        <strong>Role:</strong> {role || "Loading..."}
      </p>
    </div>
  );
};

export default UserInfoCard;
