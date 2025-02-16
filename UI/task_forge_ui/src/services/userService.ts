const BASE_URL = "http://localhost:5272";


export const fetchAllUserRoles = async () => {
  const response = await fetch(`${BASE_URL}/get-all-user-roles`);
  return response.json();
};

export const fetchUserById = async (userId: number) => {
  const response = await fetch(`${BASE_URL}/get-user?userId=${userId}`);
  return response.json();
};

// implement this
export const createUser = async (userData: object) => {
  console.log("Stub: Create User", userData);
};

export const updateUser = async (userId: number, userData: object) => {
  console.log("Stub: Update User", userId, userData);
};

export const deleteUser = async (userId: number) => {
  console.log("Stub: Delete User", userId);
};
