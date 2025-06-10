export interface AnalysisResultDto {
  id: string;
  documentId: string;
  criteriaId: string;
  gradeId: string;
  isApproved: boolean;
  createdAt: string;
  generatedSummary: string;
  strengths: string[];
  areasForImprovement: string[];
  grade?: GradeDto;
}

export interface CourseGradingDto {
  courseId: string;
  courseName: string;
  groupId: string;
  groupName: string;
  assignmentResults: AssignmentResultsDto[];
}

export interface AssignmentResultsDto {
  id: string;
  results: GradingResultDto[];
}

export interface GradingResultDto {
  id: string;
  assignmentId: string;
  studentId: string;
  documentId: string;
  studentName: string;
  generatedGrade?: number;
  editedGrade?: number;
  isFinalized?: boolean;
  submissionDate?: Date;
}

export interface GradeDto {
  id: string;
  documentId: string;
  assignmentId: string;
  generatedGrade: number;
  editedGrade: number | null;
  isFinalized: boolean;
}

export interface EditGradeCommand {
  resultId?: string; // if needed for identifying which analysis result to update
  gradeId: string;
  editedGrade: number;
  isFinalized: boolean;
}
