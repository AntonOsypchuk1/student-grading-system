export interface UploadDocumentCommand {
  userId: string;
  assignmentId: string;
  fileName: string;
  // Note: in the application layer, we use byte[] for file content,
  // but in the API call from React we use FormData so this is not directly used.
}

export interface DocumentDto {
  id: string;
  userId: string;
  assignmentId: string;
  filePath: string;
  uploadedAt: string;
}