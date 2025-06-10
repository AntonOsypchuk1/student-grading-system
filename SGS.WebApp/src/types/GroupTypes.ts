import {StudentDto} from "./AuthTypes.ts";
import {CourseDto} from "./CourseTypes.ts";

export interface StudentsGroupDto {
  id: string;
  name: string;
  year: number | null;
  createdAt: Date;
  updatedAt: Date | null;
  students: StudentDto[];
  courses: CourseDto[];
  // teachers: UserDto[];
}

export interface CreateGroupCommand {
  name: string;
  description: string | null;
}