export interface LoginDto {
  email: string;
  password: string;
}

export interface RegisterUserCommand {
  fullName: string;
  email: string;
  password: string;
  role: "Student" | "Teacher";
}

export interface AuthResponseDto {
  token: string;
  refreshToken: string;
  user: UserDto;
}

export interface UserDto {
  id: string;
  fullName: string;
  email: string;
  role: string;
}