import React from "react";
import UserInfoCard from "../components/UserInfoCard";

const DashboardPage: React.FC = () => {
  return (
    <div className="container mt-4">
      <h2>Dashboard</h2>
      <UserInfoCard />
    </div>
  );
};

export default DashboardPage;
