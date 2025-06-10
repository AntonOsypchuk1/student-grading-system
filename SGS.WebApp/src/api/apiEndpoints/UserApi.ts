import {StudentDto, TeacherDto, UserDto} from "../../types/AuthTypes.ts";
import api from "../api.ts";

export const getCurrentUser = async (): Promise<UserDto | StudentDto | TeacherDto> => {
  const response = await api.get<UserDto | StudentDto | TeacherDto>("/users/me");
  return response.data;
};