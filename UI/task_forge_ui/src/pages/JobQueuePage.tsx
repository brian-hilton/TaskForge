import React, { useEffect, useState } from "react";
import { fetchAllJobs } from "../services/jobService";
import JobCard from "../components/JobCard";
import JobFormModal from "../components/JobFormModal";
import { useAuth } from "../context/AuthContext";

const JobQueuePage: React.FC = () => {
  const { user } = useAuth();
  const [jobs, setJobs] = useState<Job[]>([]);
  const [showCreateModal, setShowCreateModal] = useState(false);

  useEffect(() => {
    loadJobs();
  }, []);

  const loadJobs = async () => {
    try {
      const jobsData = await fetchAllJobs();
      const sortedJobs = jobsData.sort(
        (a: Job, b: Job) =>
          new Date(a.createdDate).getTime() - new Date(b.createdDate).getTime()
      );
      setJobs(sortedJobs);
    } catch (error) {
      console.error("Error loading jobs", error);
    }
  };

  return (
    <div className="container mt-4">
      <h2>Job Queue</h2>
      {user?.role === "project manager" && (
        <button
          className="btn btn-success mb-3"
          onClick={() => setShowCreateModal(true)}
        >
          + Create New Job
        </button>
      )}

      <div className="row">
        {jobs.map((job) => (
          <div key={job.id} className="col-md-6 mb-4">
            <JobCard job={job} refreshJobs={loadJobs} />
          </div>
        ))}
      </div>

      <JobFormModal
        show={showCreateModal}
        handleClose={() => setShowCreateModal(false)}
        refreshJobs={loadJobs}
      />
    </div>
  );
};

export default JobQueuePage;
