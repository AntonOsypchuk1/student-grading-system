export interface CreateGradingCriteriaCommand {
  userId: string;
  criteriaDescription: string;
}

export interface GradingCriteriaDto {
  id: string;
  userId: string;
  criteriaDescription: string;
  createdAt: string;
}