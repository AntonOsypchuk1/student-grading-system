import {AssignmentDto} from "./AssignmentTypes.ts";
import {StudentsGroupDto} from "./GroupTypes.ts";
import {TeacherDto} from "./AuthTypes.ts";

export interface CreateCourseCommand {
  title: string;
  description: string;
}

export interface CourseDto {
  id: string;
  title: string;
  description: string;
  assignments: AssignmentDto[];
  studentsGroups: StudentsGroupDto[];
  teachers: TeacherDto[];
}