import api from "../api.ts";
import {CreateGroupCommand, StudentsGroupDto} from "../../types/GroupTypes.ts";

export const assignStudentToGroup = async (groupId: string, studentId: string): Promise<boolean> => {
  const response = await api.post<boolean>(`/groups/${groupId}/student/${studentId}`);
  return response.data;
};

// export const getAllGroups = async (): Promise<StudentsGroupDto> => {
//   const response = await api.get<StudentsGroupDto>("/groups");
//   return response.data;
// }

export const getGroupsByCourseId = async (courseId: string): Promise<StudentsGroupDto[]> => {
  const response = await api.get<StudentsGroupDto[]>(`/groups/course/${courseId}`);
  return response.data;
}

export const getGroupById = async (id: string): Promise<StudentsGroupDto> => {
  const response = await api.get<StudentsGroupDto>(`/groups/${id}`);
  return response.data;
}

export const getGroupByName = async (name: string): Promise<StudentsGroupDto> => {
  const response = await api.get<StudentsGroupDto>(`/groups/name/${name}`);
  return response.data;
}

export const createGroup = async (data: CreateGroupCommand): Promise<StudentsGroupDto> => {
  const response = await api.post<StudentsGroupDto>("/groups/create", data);
  return response.data;
}

export const getGroupByCurrentStudent = async (): Promise<StudentsGroupDto> => {
  const response = await api.get<StudentsGroupDto>("/groups/student/me");
  return response.data;
}
