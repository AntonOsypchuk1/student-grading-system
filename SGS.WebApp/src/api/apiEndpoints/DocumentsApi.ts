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

export const getDocumentsByUser = async (userId: string): Promise<DocumentDto[]> => {
  const response = await api.get<DocumentDto[]>(`/documents/user/${userId}`);
  return response.data;
};