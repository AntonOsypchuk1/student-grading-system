import api from "../api.ts";
import {AnalysisResultDto} from "../../types/AnalisysTypes.ts";
import {EditGradeCommand} from "../../types/GradingTypes.ts";

export const analyzeDocument = async (docId: string): Promise<AnalysisResultDto> => {
  const response = await api.post<AnalysisResultDto>(`/grading/analyze/${docId}`);
  return response.data;
};

export const getAnalysisResults = async (docId: string): Promise<AnalysisResultDto> => {
  const response = await api.get<AnalysisResultDto>(`/grading/results/${docId}`);
  return response.data;
};

export const editGrade = async (data: EditGradeCommand): Promise<AnalysisResultDto> => {
  const response = await api.put<AnalysisResultDto>("/grading/edit", data);
  return response.data;
};

export const getGradingResultsByAssignment = async (assignmentId: string): Promise<AnalysisResultDto[]> => {
  const response = await api.get<AnalysisResultDto[]>(`/grading/assignments/${assignmentId}`);
  return response.data;
};

export const finalizeGrade = async (gradeId: string): Promise<AnalysisResultDto> => {
  const response = await api.put<AnalysisResultDto>(`/grading/finalize/${gradeId}`);
  return response.data;
};