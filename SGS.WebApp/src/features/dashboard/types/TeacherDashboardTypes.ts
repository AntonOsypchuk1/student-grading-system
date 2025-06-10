export interface TeacherStats {
  totalStudents: number;
  averageGrade: number;
  completionRate: number;
  pendingReviews: number;
}

export interface ActivityItem {
  id: string;
  type: 'submission' | 'grade' | 'announcement';
  title: string;
  description: string;
  timestamp: string;
  course: string;
  student?: string;
}

export interface GroupPerformance {
  groupId: string;
  groupName: string;
  averageGrade: number;
  submissionRate: number;
  studentCount: number;
}