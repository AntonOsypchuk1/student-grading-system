import {AssignmentDto, CreateAssignmentCommand, UpdateAssignmentCommand} from "../../types/AssignmentTypes.ts";
import api from "../api.ts";
import {CourseDto, CreateCourseCommand} from "../../types/CourseTypes.ts";

// assignments API
export const createAssignment = async (data: CreateAssignmentCommand): Promise<AssignmentDto> => {
  const response = await api.post<AssignmentDto>("/assignments/create", data);
  return response.data;
};

export const updateAssignment = async (id: string, data: UpdateAssignmentCommand): Promise<AssignmentDto> => {
  const response = await api.put<AssignmentDto>(`/assignments/update/${id}`, data);
  return response.data;
}

// export const getAssignments = async (): Promise<AssignmentDto[]> => {
//   const response = await api.get<AssignmentDto[]>(`/assignments`);
//   return response.data;
// }

export const getAssignmentById = async (id: string): Promise<AssignmentDto> => {
  const response = await api.get<AssignmentDto>(`/assignments/${id}`);
  return response.data;
};

export const getAssignmentsByGroupAndCourse = async (groupId: string | undefined, courseId: string): Promise<AssignmentDto[]> => {
  if (!groupId) {
    const response = await api.get<AssignmentDto[]>(`/assignments/course?courseId=${courseId}`);
    return response.data;
  }

  const response = await api.get<AssignmentDto[]>(`/assignments/course?courseId=${courseId}&groupId=${groupId}`);
  return response.data;
};

export const getAssignmentsByGroup = async (groupId: string): Promise<AssignmentDto[]> => {
  const response = await api.get<AssignmentDto[]>(`/assignments/group/${groupId}`);
  return response.data;
}

// course API
export const createCourse = async (data: CreateCourseCommand): Promise<CourseDto> => {
  const response = await api.post<CourseDto>("/courses/create", data);
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

export const assignTeacherToCourse = async (courseId: string, teacherId: string): Promise<boolean> => {
  const response = await api.post<boolean>(`/courses/${courseId}/teachers/${teacherId})`);
  return response.data;
}

export const assignGroupToCourse = async (courseId: string, groupId: string): Promise<boolean> => {
  const response = await api.post<boolean>(`/courses/${courseId}/groups/${groupId})`);
  return response.data;
}