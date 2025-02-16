import React, { useEffect, useState } from "react";
import axios from "axios";
import { Modal, Button, ListGroup } from "react-bootstrap";

interface User {
  id: number;
  name: string;
}

interface CrewMember {
  crewId: number;
  userId: number;
  role: string;
}

interface ManageCrewModalProps {
  show: boolean;
  handleClose: () => void;
  crewId: number;
  crewName: string;
}

const ManageCrewModal: React.FC<ManageCrewModalProps> = ({
  show,
  handleClose,
  crewId,
  crewName,
}) => {
  const [availableUsers, setAvailableUsers] = useState<User[]>([]);
  const [crewMembers, setCrewMembers] = useState<CrewMember[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedUser, setSelectedUser] = useState<number | null>(null);

  useEffect(() => {
    if (show) {
      fetchCrewMembers();
      fetchAvailableUsers();
    }
  }, [show]);

  const fetchCrewMembers = async () => {
    try {
      const { data } = await axios.get(
        `http://localhost:5272/crews/get-all-members?crewId=${crewId}`
      );
      const membersWithNames = await Promise.all(
        data.map(async (member: CrewMember) => {
          const { data: user } = await axios.get(
            `http://localhost:5272/get-user?userId=${member.userId}`
          );
          return { ...member, name: user.name };
        })
      );
      setCrewMembers(membersWithNames);
    } catch (error) {
      console.error("Error fetching crew members", error);
    }
  };

  const fetchAvailableUsers = async () => {
    try {
      const { data } = await axios.get(
        `http://localhost:5272/get-top-users?top=200`
      );
      const crewUserIds = new Set(crewMembers.map((member) => member.userId));
      const filteredUsers = data.filter(
        (user: User) => !crewUserIds.has(user.id)
      );
      setAvailableUsers(filteredUsers);
    } catch (error) {
      console.error("Error fetching users", error);
    } finally {
      setLoading(false);
    }
  };

  const handleAddMember = async (role: string) => {
    if (!selectedUser) return;

    try {
      await axios.post(
        `http://localhost:5272/crews/add-member?crewId=${crewId}&userId=${selectedUser}&role=${role}`
      );
      fetchCrewMembers(); // Refresh crew members after adding
    } catch (error) {
      console.error("Error adding member", error);
    }
  };

  const handleRemoveMember = async (userId: number) => {
    const confirmRemove = window.confirm(
      "Are you sure you want to remove this crew member?"
    );
    if (!confirmRemove) return;

    try {
      await axios.delete(
        `http://localhost:5272/crews/delete-member?crewId=${crewId}&userId=${userId}`
      );
      fetchCrewMembers(); // Refresh crew members after removal
    } catch (error) {
      console.error("Error removing member", error);
    }
  };

  return (
    <Modal show={show} onHide={handleClose}>
      <Modal.Header closeButton>
        <Modal.Title>Manage Crew: {crewName}</Modal.Title>
      </Modal.Header>
      <Modal.Body>
        {loading ? (
          <p>Loading...</p>
        ) : (
          <>
            <h5>Current Members</h5>
            <ListGroup>
              {crewMembers.map((member) => (
                <ListGroup.Item
                  key={member.userId}
                  className="d-flex justify-content-between align-items-center"
                >
                  <div>
                    <strong>{member.name}</strong> -{" "}
                    <span className="text-muted">{member.role}</span>
                  </div>
                  <Button
                    variant="danger"
                    size="sm"
                    onClick={() => handleRemoveMember(member.userId)}
                  >
                    Remove
                  </Button>
                </ListGroup.Item>
              ))}
            </ListGroup>

            <h5 className="mt-3">Add Member</h5>
            <select
              className="form-control mb-2"
              onChange={(e) => setSelectedUser(Number(e.target.value))}
            >
              <option value="">Select User</option>
              {availableUsers.map((user) => (
                <option key={user.id} value={user.id}>
                  {user.name}
                </option>
              ))}
            </select>
            <Button
              variant="primary"
              disabled={!selectedUser}
              onClick={() => handleAddMember("worker")}
            >
              Add as Worker
            </Button>
          </>
        )}
      </Modal.Body>
    </Modal>
  );
};

export default ManageCrewModal;
