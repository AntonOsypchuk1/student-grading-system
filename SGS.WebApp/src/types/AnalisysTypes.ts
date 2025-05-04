export interface AnalysisResultDto {
  id: string;
  documentId: string;
  criteriaId: string;
  gradeId: string;
  generatedSummary: string;
  isApproved: boolean;
  createdAt: string;
}