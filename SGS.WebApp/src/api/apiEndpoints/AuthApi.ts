import {AuthResponseDto, LoginDto, RegisterUserCommand, UserDto} from "../../types/AuthTypes.ts";
import api from "../api.ts";
import {RefreshTokenRequestDto} from "../../types/TokenTypes.ts";

export const login = async (data: LoginDto): Promise<AuthResponseDto> => {
  const response = await api.post<AuthResponseDto>("/auth/login", data);
  return response.data;
};

export const registerUser = async (data: RegisterUserCommand): Promise<UserDto> => {
  const response = await api.post<UserDto>("/auth/register", data);
  return response.data;
};

export const refreshToken = async (data: RefreshTokenRequestDto): Promise<AuthResponseDto> => {
  const response = await api.post<AuthResponseDto>("/auth/refresh", data);
  return response.data;
};

export const logout = async (data: RefreshTokenRequestDto): Promise<Response> => {
  const response = await api.post<Response>("/auth/logout", data);
  return response.data;
}