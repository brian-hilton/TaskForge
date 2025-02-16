import React, { createContext, useState, useContext, useEffect } from "react";
import { fetchUserRoles } from "../services/dashboardService";

interface User {
  id: number;
  name: string;
  email: string;
  role: string; // Ensure role is stored here
}

interface AuthContextType {
  user: User | null;
  login: (userData: User) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const [user, setUser] = useState<User | null>(() => {
    const storedUser = localStorage.getItem("user");
    return storedUser ? JSON.parse(storedUser) : null;
  });

  useEffect(() => {
    if (user && !user.role) {
      fetchUserRoles(user.id)
        .then((roles) => {
          if (roles.length > 0) {
            const updatedUser = { ...user, role: roles[roles.length - 1].name };
            setUser(updatedUser);
            localStorage.setItem("user", JSON.stringify(updatedUser)); // Save to localStorage
          }
        })
        .catch(() => setUser({ ...user, role: "Unknown" }));
    }
  }, [user]);

  const login = (userData: User) => {
    fetchUserRoles(userData.id).then((roles) => {
      const updatedUser = { ...userData, role: roles[roles.length - 1].name };
      localStorage.setItem("user", JSON.stringify(updatedUser));
      setUser(updatedUser);
    });
  };

  const logout = () => {
    localStorage.removeItem("user");
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
};
