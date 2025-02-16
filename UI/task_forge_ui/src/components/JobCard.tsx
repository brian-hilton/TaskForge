import React, { useState } from "react";
import { deleteJob } from "../services/jobService";
import JobEditModal from "./JobEditModal";
import { useAuth } from "../context/AuthContext";

interface Job {
  id: number;
  name: string;
  location: string;
  status: string;
  createdDate: string;
  dueDate?: string;
}

const JobCard: React.FC<{ job: Job; refreshJobs: () => void }> = ({
  job,
  refreshJobs,
}) => {
  const { user } = useAuth();
  const [showEditModal, setShowEditModal] = useState(false);

  const handleDelete = async () => {
    if (window.confirm("Are you sure you want to delete this job?")) {
      await deleteJob(job.id);
      refreshJobs();
    }
  };

  return (
    <div className="card p-3 shadow-sm">
      <h4>{job.name}</h4>
      <p>
        <strong>Location:</strong> {job.location}
      </p>
      <p>
        <strong>Status:</strong> {job.status}
      </p>
      <p>
        <strong>Created:</strong>{" "}
        {new Date(job.createdDate).toLocaleDateString()}
      </p>
      {job.dueDate && (
        <p>
          <strong>Due:</strong> {new Date(job.dueDate).toLocaleDateString()}
        </p>
      )}

      {user?.role === "project manager" && (
        <div className="d-flex gap-2">
          <button
            className="btn btn-warning"
            onClick={() => setShowEditModal(true)}
          >
            Edit
          </button>
          <button className="btn btn-danger" onClick={handleDelete}>
            Delete
          </button>
        </div>
      )}

      <JobEditModal
        show={showEditModal}
        handleClose={() => setShowEditModal(false)}
        job={job}
        refreshJobs={refreshJobs}
      />
    </div>
  );
};

export default JobCard;
