import {DocumentDto} from "../../types/DocumentTypes.ts";
import api from "../api.ts";

export const uploadDocument = async (formData: FormData): Promise<DocumentDto> => {
  const response = await api.post<DocumentDto>("/documents/upload", formData, {
    headers: {
      "Content-Type": "multipart/form-data",
    },
  });
  return response.data;
};

export const getDocumentById = async (id: string): Promise<DocumentDto> => {
  const response = await api.get<DocumentDto>(`/documents/${id}`);
  return response.data;
};

export const getDocumentByUserIdAndAssignmentId = async (userId: string, assignmentId: string): Promise<DocumentDto> => {
  const response = await api.get<DocumentDto>(`/documents?userId=${userId}&assignmentId=${assignmentId}`);
  return response.data;
}

export const getDocumentsByAssignmentId = async (assignmentId: string): Promise<DocumentDto[]> => {
  const response = await api.get<DocumentDto[]>(`/documents/assignment/${assignmentId}`);
  return response.data;
}

export const getDocumentsByUser = async (userId: string): Promise<DocumentDto[]> => {
  const response = await api.get<DocumentDto[]>(`/documents/user/${userId}`);
  return response.data;
};

export const downloadDocument = async (documentId: string): Promise<Blob> => {
  const response = await api.get<Blob>(`/documents/${documentId}/download`, {
    responseType: 'blob',
    headers: {
      'Accept': 'application/pdf,application/octet-stream'
    }
  });
  return response.data;
}

export const deleteDocument = async (documentId: string): Promise<boolean> => {
  const response = await api.delete<boolean>(`/documents/${documentId}`);
  return response.data;
}