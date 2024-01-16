import axios from 'axios';

const baseURL = process.env.REACT_APP_API_URL

const httpClient = axios.create({
  baseURL
  // Other Axios configurations...
});

export default httpClient

