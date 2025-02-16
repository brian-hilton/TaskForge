import React, { useState, useEffect } from "react";
import { updateJob } from "../services/jobService";

interface Job {
  id: number;
  name: string;
  location: string;
  status: string;
  dueDate?: string;
}

const JobEditModal: React.FC<{
  show: boolean;
  handleClose: () => void;
  job: Job;
  refreshJobs: () => void;
}> = ({ show, handleClose, job, refreshJobs }) => {
  const [jobData, setJobData] = useState({
    name: "",
    location: "",
    status: "",
    dueDate: "",
  });

  useEffect(() => {
    if (job) {
      setJobData({
        name: job.name,
        location: job.location,
        status: job.status,
        dueDate: job.dueDate || "",
      });
    }
  }, [job]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await updateJob(job.id, jobData);
    refreshJobs();
    handleClose();
  };

  if (!show) return null; // Prevent modal from rendering when hidden

  return (
    <div className="modal show d-block" tabIndex={-1} role="dialog">
      <div className="modal-dialog">
        <div className="modal-content">
          <div className="modal-header">
            <h4 className="modal-title">Edit Job</h4>
            <button type="button" className="close" onClick={handleClose}>
              &times;
            </button>
          </div>
          <div className="modal-body">
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

              <div className="form-group">
                <label>Due Date</label>
                <input
                  type="datetime-local"
                  className="form-control"
                  value={jobData.dueDate}
                  onChange={(e) =>
                    setJobData({ ...jobData, dueDate: e.target.value })
                  }
                />
              </div>

              <button type="submit" className="btn btn-primary mt-3">
                Update Job
              </button>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
};

export default JobEditModal;
