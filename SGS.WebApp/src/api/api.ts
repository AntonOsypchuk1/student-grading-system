import axios, {AxiosError} from "axios";

interface RetryConfig {
  retry: number;
  retryDelay: number;
  retryCount: number;
}

const api = axios.create({
  baseURL: "/api",
  headers: {
    "Content-Type": "application/json",
  },
  withCredentials: true,
});

api.defaults.retry = 2;
api.defaults.retryDelay = 1000;
api.defaults.retryCount = 0;

api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const config = error.config as RetryConfig & any;

    if (!config || !config.retry) {
      return Promise.reject(error);
    }

    config.retryCount = config.retryCount || 0;

    if (config.retryCount >= config.retry) {
      console.error('API Error (Max retries reached):', {
        url: config.url,
        method: config.method,
        status: error.response?.status,
        data: error.response?.data,
        retryCount: config.retryCount
      });
      return Promise.reject(error);
    }

    config.retryCount += 1;

    const delay = config.retryDelay * Math.pow(2, config.retryCount - 1);

    console.log(`Retrying request (${config.retryCount}/${config.retry}) after ${delay}ms`);

    await new Promise(resolve => setTimeout(resolve, delay));

    return api(config);
  }
);


api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("accessToken");
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    console.log('Request Config:', {
      url: config.url,
      method: config.method,
      headers: config.headers,
      data: config.data,
      retry: config.retry,
      retryCount: config.retryCount
    });
    return config;
  },
  (error) => Promise.reject(error)
);


export default api;
