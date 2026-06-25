import axios from "axios";

const axiosClient = axios.create({
  baseURL: "http://localhost:5156/api",
});

export default axiosClient;
