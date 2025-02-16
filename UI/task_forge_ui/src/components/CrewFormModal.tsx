import React, { useEffect, useState } from "react";
import { Modal, Button, Form } from "react-bootstrap";
import { updateCrewByCrewId } from "../services/crewService";
import axios from "axios";

interface User {
  id: number;
  name: string;
}

interface Crew {
  id: number;
  supervisorId: number | null;
  name: string;
}

interface CrewFormModalProps {
  show: boolean;
  handleClose: () => void;
  refreshCrews: () => void;
  crew?: Crew | null; // If null, it's for creating a new crew
}

const CrewFormModal: React.FC<CrewFormModalProps> = ({
  show,
  handleClose,
  refreshCrews,
  crew,
}) => {
  const [crewName, setCrewName] = useState("");
  const [selectedSupervisor, setSelectedSupervisor] = useState<number | null>(
    null
  );
  const [supervisors, setSupervisors] = useState<User[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (show) {
      fetchSupervisors();
      if (crew) {
        setCrewName(crew.name);
        setSelectedSupervisor(crew.supervisorId || null);
      } else {
        setCrewName("");
        setSelectedSupervisor(null);
      }
    }
  }, [show, crew]);

  const fetchSupervisors = async () => {
    try {
      // Get all user roles
      const { data: roles } = await axios.get(
        "http://localhost:5272/get-all-user-roles"
      );

      // Filter to only get users with roleId: 1 (Supervisors)
      const supervisorRoles = roles.filter(
        (role: { roleId: number }) => role.roleId === 1
      );

      // Fetch full user info for supervisors
      const supervisorPromises = supervisorRoles.map(
        async (role: { userId: number }) => {
          const { data: user } = await axios.get(
            `http://localhost:5272/get-user?userId=${role.userId}`
          );
          return { id: user.id, name: user.name };
        }
      );

      const supervisorList = await Promise.all(supervisorPromises);
      setSupervisors(supervisorList);
    } catch (error) {
      console.error("Error fetching supervisors", error);
    }
  };

  const handleSubmit = async () => {
    setIsSubmitting(true);
    try {
      if (crew) {
        // ✅ Update existing crew using correct JSON format
        await updateCrewByCrewId(crew.id, {
          Name: crewName,
          SupervisorId: selectedSupervisor,
        });
      } else {
        // ✅ Create new crew
        await axios.post(
          `http://localhost:5272/crews/create-crew?name=${crewName}`
        );
      }
      refreshCrews(); // Refresh crew list after successful operation
      handleClose();
    } catch (error) {
      console.error("Error saving crew", error);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal show={show} onHide={handleClose}>
      <Modal.Header closeButton>
        <Modal.Title>{crew ? "Edit Crew" : "Create New Crew"}</Modal.Title>
      </Modal.Header>
      <Modal.Body>
        <Form>
          <Form.Group className="mb-3">
            <Form.Label>Crew Name</Form.Label>
            <Form.Control
              type="text"
              placeholder="Enter crew name"
              value={crewName}
              onChange={(e) => setCrewName(e.target.value)}
              disabled={isSubmitting}
            />
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label>Assign Supervisor</Form.Label>
            <Form.Select
              value={selectedSupervisor || ""}
              onChange={(e) => setSelectedSupervisor(Number(e.target.value))}
              disabled={isSubmitting}
            >
              <option value="">Select Supervisor</option>
              {supervisors.map((user) => (
                <option key={user.id} value={user.id}>
                  {user.name}
                </option>
              ))}
            </Form.Select>
          </Form.Group>
        </Form>
      </Modal.Body>
      <Modal.Footer>
        <Button
          variant="secondary"
          onClick={handleClose}
          disabled={isSubmitting}
        >
          Cancel
        </Button>
        <Button
          variant="primary"
          onClick={handleSubmit}
          disabled={isSubmitting || !crewName}
        >
          {crew ? "Update Crew" : "Create Crew"}
        </Button>
      </Modal.Footer>
    </Modal>
  );
};

export default CrewFormModal;
