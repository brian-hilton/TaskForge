import { useEffect, useState } from "react";
import { getJob } from "../api/jobApi"; // Adjust path if necessary
import { Job } from "../api/jobApi"; // Assuming you have a Job interface

interface JobDetailsProps {
  jobId: number;
}

const JobDetails = ({ jobId }: JobDetailsProps) => {
  const [job, setJob] = useState<Job | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchJob = async () => {
      try {
        const data = await getJob(jobId);
        setJob(data);
      } catch (err) {
        setError("Failed to load job data.");
      } finally {
        setLoading(false);
      }
    };

    fetchJob();
  }, [jobId]);

  if (loading) return <p>Loading...</p>;
  if (error) return <p>{error}</p>;
  if (!job) return <p>No job found.</p>;

  return (
    <div>
      <h2>Job Details</h2>
      <p>
        <strong>ID:</strong> {job.id}
      </p>
      <p>
        <strong>Name:</strong> {job.name}
      </p>
      <p>
        <strong>Status:</strong> {job.status}
      </p>
      <p>
        <strong>Location:</strong> {job.location}
      </p>
      <p>
        <strong>User ID:</strong> {job.userId ?? "N/A"}
      </p>
      <p>
        <strong>Created:</strong>{" "}
        {new Date(job.createdDate).toLocaleDateString()}
      </p>
      <p>
        <strong>Updated:</strong>{" "}
        {new Date(job.updatedDate).toLocaleDateString()}
      </p>
      <p>
        <strong>Due Date:</strong>{" "}
        {job.dueDate ? new Date(job.dueDate).toLocaleDateString() : "N/A"}
      </p>
    </div>
  );
};

export default JobDetails;
