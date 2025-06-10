import {CourseDto} from "./CourseTypes.ts";

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

export interface StudentDto extends UserDto {
  groupId: string;
}

export interface TeacherDto extends UserDto {
  courses: CourseDto[] | undefined;
}
