export interface CreateGradingCriteriaCommand {
  title: string;
  description: string;
  maxGrade: number;
  weight: number;
}

export interface CreateBatchGradingCriteriaCommand {
  criteria: CreateGradingCriteriaCommand[];
}

export interface GradingCriteriaDto {
  id: string;
  title: string;
  description: string;
  maxGrade: number;
  weight: number;
}