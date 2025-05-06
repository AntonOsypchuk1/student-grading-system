import {AssignmentDto, CreateAssignmentCommand} from "../../types/AssignmentTypes.ts";
import api from "../api.ts";
import {CourseDto, CreateCourseCommand} from "../../types/CourseTypes.ts";

export const createAssignment = async (data: CreateAssignmentCommand): Promise<AssignmentDto> => {
  const response = await api.post<AssignmentDto>("/assignments/create", data);
  return response.data;
};

export const getAssignments = async (): Promise<AssignmentDto[]> => {
  const response = await api.get<AssignmentDto[]>(`/assignments`);
  return response.data;
}

export const getAssignmentById = async (id: string): Promise<AssignmentDto> => {
  const response = await api.get<AssignmentDto>(`/assignments/${id}`);
  return response.data;
};

export const getAssignmentsByGroup = async (groupId: string): Promise<AssignmentDto[]> => {
  const response = await api.get<AssignmentDto[]>(`/assignments/group/${groupId}`);
  return response.data;
};

export const getCourseById = async (id: string): Promise<CourseDto> => {
  const response = await api.get<CourseDto>(`/courses/${id}`);
  return response.data;
};

export const getCoursesByTeacher = async (teacherId: string): Promise<CourseDto[]> => {
  const response = await api.get<CourseDto[]>(`/courses/teacher/${teacherId}`);
  return response.data;
};

export const getCoursesByStudent = async (studentId: string): Promise<CourseDto[]> => {
  const response = await api.get<CourseDto[]>(`/courses/student/${studentId}`);
  return response.data;
};

export const createCourse = async (data: CreateCourseCommand): Promise<CourseDto> => {
  const response = await api.post<CourseDto>("/courses/create", data);
  return response.data;
};