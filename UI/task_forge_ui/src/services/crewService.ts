const BASE_URL = "http://localhost:5272";

export const fetchAllCrews = async () => {
  const response = await fetch(`${BASE_URL}/crews/all-crews`);
  return response.json();
};

export const fetchUserById = async (userId: number) => {
  const response = await fetch(`${BASE_URL}/get-user?userId=${userId}`);
  return response.json();
};

export const fetchCrewMembers = async (crewId: number) => {
    const response = await fetch(`${BASE_URL}/crews/get-all-members?crewId=${crewId}`);
    return response.json();
  };

export const updateCrewByCrewId = async (crewId: number, crewData: object) => {
  const response = await fetch(`${BASE_URL}/crews/update-crew?crewId=${crewId}`, {
    method: "PATCH",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(crewData),
  });
  return response.json();
};