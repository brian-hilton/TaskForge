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
    <div className="bg-purple-700 text-white p-6 rounded-2xl shadow-lg w-full max-w-md">
      <h2 className="text-2xl font-bold mb-2">{job.name}</h2>
      <p className="text-sm text-purple-300 mb-4">{job.location}</p>

      <div className="grid grid-cols-2 gap-2 text-sm">
        <p>
          <span className="font-semibold">Status:</span> {job.status}
        </p>
        <p>
          <span className="font-semibold">User:</span> {job.userId ?? "N/A"}
        </p>
        <p>
          <span className="font-semibold">Created:</span>{" "}
          {new Date(job.createdDate).toLocaleDateString()}
        </p>
        <p>
          <span className="font-semibold">Updated:</span>{" "}
          {new Date(job.updatedDate).toLocaleDateString()}
        </p>
        <p>
          <span className="font-semibold">Due:</span>{" "}
          {job.dueDate ? new Date(job.dueDate).toLocaleDateString() : "N/A"}
        </p>
      </div>
    </div>
  );
};

export default JobDetails;
