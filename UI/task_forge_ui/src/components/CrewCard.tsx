import React from "react";

interface CrewCardProps {
  crewId: number;
  crewName: string;
  supervisorName: string;
  members: string[];
  onManageCrew: () => void;
  onEditCrew: () => void;
  onDeleteCrew: () => void;
  userRole: string;
}

const CrewCard: React.FC<CrewCardProps> = ({
  crewId,
  crewName,
  supervisorName,
  members,
  onManageCrew,
  onEditCrew,
  onDeleteCrew,
  userRole,
}) => {
  return (
    <div className="card p-3 shadow-sm">
      <h4>{crewName}</h4>
      <p>
        <strong>Supervisor:</strong> {supervisorName}
      </p>
      <p>
        <strong>Members:</strong>{" "}
        {members.length > 0 ? members.join(", ") : "No members"}
      </p>

      {userRole === "project manager" && (
        <div className="d-flex gap-2">
          <button className="btn btn-primary" onClick={onManageCrew}>
            Manage Members
          </button>
          <button className="btn btn-warning" onClick={onEditCrew}>
            Edit Crew
          </button>
          <button className="btn btn-danger" onClick={onDeleteCrew}>
            Delete Crew
          </button>
        </div>
      )}
    </div>
  );
};

export default CrewCard;
