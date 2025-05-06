export interface EditGradeCommand {
  resultId?: string; // if needed for identifying which analysis result to update
  gradeId: string;
  editedGrade: number;
  isFinalized: boolean;
}