import React, { useState } from "react";
import { createJob } from "../services/jobService";
import { useAuth } from "../context/AuthContext";

const JobFormModal: React.FC<{
  show: boolean;
  handleClose: () => void;
  refreshJobs: () => void;
}> = ({ show, handleClose, refreshJobs }) => {
  const { user } = useAuth(); // Get user ID from auth context
  const [jobData, setJobData] = useState({
    name: "",
    location: "",
    status: "pending",
  });
  const [error, setError] = useState("");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user) {
      setError("Error: User not found");
      return;
    }

    const payload = {
      UserId: user.id, // Include logged-in user ID
      Name: jobData.name,
      Status: jobData.status,
      Location: jobData.location,
    };

    try {
      await createJob(payload);
      refreshJobs();
      handleClose();
    } catch (err) {
      console.error("Error creating job:", err);
      setError("Failed to create job. Please try again.");
    }
  };

  if (!show) return null; // Prevents rendering when hidden

  return (
    <div className="modal show d-block" tabIndex={-1} role="dialog">
      <div className="modal-dialog">
        <div className="modal-content">
          <div className="modal-header">
            <h4 className="modal-title">Create Job</h4>
            <button type="button" className="close" onClick={handleClose}>
              &times;
            </button>
          </div>
          <div className="modal-body">
            {error && <p className="text-danger">{error}</p>}
            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label>Job Name</label>
                <input
                  type="text"
                  className="form-control"
                  value={jobData.name}
                  onChange={(e) =>
                    setJobData({ ...jobData, name: e.target.value })
                  }
                  required
                />
              </div>

              <div className="form-group">
                <label>Location</label>
                <input
                  type="text"
                  className="form-control"
                  value={jobData.location}
                  onChange={(e) =>
                    setJobData({ ...jobData, location: e.target.value })
                  }
                  required
                />
              </div>

              <div className="form-group">
                <label>Status</label>
                <input
                  type="text"
                  className="form-control"
                  value={jobData.status}
                  onChange={(e) =>
                    setJobData({ ...jobData, status: e.target.value })
                  }
                  required
                />
              </div>

              <button type="submit" className="btn btn-primary mt-3">
                Create Job
              </button>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
};

export default JobFormModal;
