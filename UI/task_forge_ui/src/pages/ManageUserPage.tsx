import React, { useEffect, useState } from "react";
import { fetchAllUserRoles, fetchUserById } from "../services/userService";
import UserCard from "../components/UserCard";

interface User {
  id: number;
  name: string;
  role: string;
}

const ManageUserPage: React.FC = () => {
  const [users, setUsers] = useState<User[]>([]);

  useEffect(() => {
    loadUsers();
  }, []);

  const loadUsers = async () => {
    try {
      const rolesData = await fetchAllUserRoles();

      // Fetch user details based on role data
      const userPromises = rolesData.map(
        async (roleEntry: { userId: number; roleId: number }) => {
          const userData = await fetchUserById(roleEntry.userId);
          return {
            id: userData.id,
            name: userData.name,
            role: `Role ID: ${roleEntry.roleId}`,
          };
        }
      );

      const usersList = await Promise.all(userPromises);
      setUsers(usersList);
    } catch (error) {
      console.error("Error fetching users", error);
    }
  };

  return (
    <div className="container mt-4">
      <h2>Manage Users</h2>
      <div className="row">
        {users.map((user) => (
          <div key={user.id} className="col-md-6 mb-4">
            <UserCard user={user} />
          </div>
        ))}
      </div>
    </div>
  );
};

export default ManageUserPage;
