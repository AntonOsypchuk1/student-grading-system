import api from "../api.ts";
import {
  CreateBatchGradingCriteriaCommand,
  CreateGradingCriteriaCommand,
  GradingCriteriaDto
} from "../../types/GradingCriteriaTypes.ts";

export const createGradingCriteria = async (data: CreateGradingCriteriaCommand): Promise<GradingCriteriaDto> => {
  const response = await api.post<GradingCriteriaDto>("/criteria/create", data);
  return response.data;
};

export const createMultipleGradingCriteria = async (data: CreateBatchGradingCriteriaCommand): Promise<GradingCriteriaDto> => {
  const response = await api.post<GradingCriteriaDto>("/criteria/create-multiple", data);
  return response.data;
};

export const getGradingCriteriaById = async (id: string): Promise<GradingCriteriaDto> => {
  const response = await api.get<GradingCriteriaDto>(`/criteria/${id}`);
  return response.data;
};

export const getAllGradingCriteria = async (): Promise<GradingCriteriaDto[]> => {
  const response = await api.get<GradingCriteriaDto[]>("/criteria/all");
  return response.data;
};

export const getGradingCriteriaByAssignmentId = async (assignmentId: string): Promise<GradingCriteriaDto[]> => {
  const response = await api.get<GradingCriteriaDto[]>(`/criteria/assignment/${assignmentId}`);
  return response.data;
}