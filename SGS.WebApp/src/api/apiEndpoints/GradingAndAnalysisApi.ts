import api from "../api.ts";
import {AnalysisResultDto, CourseGradingDto, EditGradeCommand, GradingResultDto} from "../../types/AnalysisTypes.ts";

export const analyzeDocument = async (docId: string): Promise<AnalysisResultDto> => {
  const response = await api.post<AnalysisResultDto>(`/grading/analyze/${docId}`);
  return response.data;
};

export const getAnalysisResults = async (docId: string): Promise<AnalysisResultDto> => {
  const response = await api.get<AnalysisResultDto>(`/grading/by-document/${docId}`);
  return response.data;
};

export const editGrade = async (data: EditGradeCommand): Promise<AnalysisResultDto> => {
  const response = await api.put<AnalysisResultDto>("/grading/edit", data);
  return response.data;
};

export const getGradingResultsByAssignment = async (assignmentId: string): Promise<GradingResultDto[]> => {
  const response = await api.get<GradingResultDto[]>(`/grading/assignments/${assignmentId}`);
  return response.data;
};

export const getGradingResultByCourseAndGroup = async (courseId: string, groupId: string): Promise<CourseGradingDto> => {
  const response = await api.get<CourseGradingDto>(`/grading/course/${courseId}&groupId=${groupId}`);
  return response.data;
};

export const finalizeGrade = async (gradeId: string): Promise<AnalysisResultDto> => {
  const response = await api.put<AnalysisResultDto>(`/grading/finalize/${gradeId}`);
  return response.data;
};