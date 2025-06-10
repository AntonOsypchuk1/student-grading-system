import {RegisterUserCommand} from "../../types/AuthTypes.ts";

export interface LoginForm {
  email: string;
  password: string;
}

export type RegisterForm = RegisterUserCommand & {
  confirmPassword: string;
};

export interface AuthResponseDto {
  token: string;
  user: {
    id: string;
    email: string;
    fullName: string;
    role: 'Student' | 'Teacher';
  };
}