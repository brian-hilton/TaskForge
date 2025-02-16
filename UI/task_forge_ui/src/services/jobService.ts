const BASE_URL = "http://localhost:5272";

export const fetchAllJobs = async () => {
  const response = await fetch(`${BASE_URL}/jobs/all-jobs`);
  return response.json();
};

export const fetchJobById = async (jobId: number) => {
  const response = await fetch(`${BASE_URL}/jobs?jobId=${jobId}`);
  return response.json();
};

export const createJob = async (jobData: object) => {
  const response = await fetch(`${BASE_URL}/user/create-job`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(jobData),
  });
  return response.json();
};

export const updateJob = async (jobId: number, jobData: object) => {
  const response = await fetch(`${BASE_URL}/jobs/update-job?jobId=${jobId}`, {
    method: "PATCH",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(jobData),
  });
  return response.json();
};

export const deleteJob = async (jobId: number) => {
  return fetch(`${BASE_URL}/jobs/delete-job?jobId=${jobId}`, { method: "DELETE" });
};
