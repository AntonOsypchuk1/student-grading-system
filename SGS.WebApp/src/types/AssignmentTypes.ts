export interface CreateAssignmentCommand {
  title: string;
  description: string;
  maxAttempts: number;
  dueDate: string;
  groupId: string;
  courseId: string;
}

export interface AssignmentDto {
  id: string;
  title: string;
  description: string;
  maxAttempts: number;
  dueDate: string;
  groupId: string;
  courseId: string;
}