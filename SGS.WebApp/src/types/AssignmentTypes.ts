import {
  CreateGradingCriteriaCommand,
  GradingCriteriaDto,
  UpdateGradingCriteriaCommand
} from "./GradingCriteriaTypes.ts";

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

export interface UpdateAssignmentCommand {
  id: string;
  groupId: string;
  title: string;
  description: string;
  maxAttempts: number;
  dueDate: string;
  gradingCriteria: UpdateGradingCriteriaCommand[];
}