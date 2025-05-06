import {UserDto} from "../../types/AuthenticationTypes.ts";
import api from "../api.ts";

export const getCurrentUser = async (): Promise<UserDto> => {
  const response = await api.get<UserDto>("/users/me");
  return response.data;
};