import {CreateGradingCriteriaCommand, GradingCriteriaDto} from "./GradingCriteriaTypes.ts";

export interface CreateAssignmentCommand {
  groupId: string;
  courseId: string;
  title: string;
  description: string;
  maxAttempts: number;
  dueDate: string;
  gradingCriteria?: CreateGradingCriteriaCommand[];
}

export interface AssignmentDto {
  id: string;
  title: string;
  description: string;
  maxAttempts: number;
  dueDate: string;
  groupId: string;
  courseId: string;
  gradingCriteria: GradingCriteriaDto[] | undefined;
}