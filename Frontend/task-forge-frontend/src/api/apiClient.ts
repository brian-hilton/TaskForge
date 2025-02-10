import axios from 'axios';

// Axios instance, set base url
const apiClient = axios.create({
  baseURL: 'http://localhost:5272/',  
  headers: {
    'Content-Type': 'application/json',
  },
});

// You can also add response interceptors if needed
apiClient.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    // Handle errors globally (e.g., redirect to login on 401 error)
    if (error.response.status === 401) {
      // Handle token expiration, etc.
    }
    return Promise.reject(error);
  }
);

export default apiClient;