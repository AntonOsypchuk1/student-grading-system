import {useMutation, useQuery, useQueryClient} from "@tanstack/react-query";
import {CreateCourseCommand} from "../types/CourseTypes";
import {
  assignGroupToCourse,
  assignTeacherToCourse,
  createCourse,
  getCourseById,
  getCoursesByStudent,
  getCoursesByTeacher
} from "../api/apiEndpoints/AssignmentAndCourseApi"


export function useCourses(id: string) {
  const queryClient = useQueryClient();

  const getCourseByIdQuery = useQuery({
    queryKey: ["courses", id],
    queryFn: () => getCourseById(id),
    enabled: !!id
  });

  const createCourseMutation = useMutation({
    mutationFn: (data: CreateCourseCommand) => createCourse(data),
    onSuccess: () => {
      queryClient.invalidateQueries({queryKey: ["courses"]});
    },
  });

  return {
    getCourseByIdQuery,
    createCourseMutation
  };
}

export function useAssignTeacherToCourse(courseId: string, teacherId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => assignTeacherToCourse(courseId, teacherId),
    onSuccess: () => {
      queryClient.invalidateQueries({queryKey: ["courses"]});
    },
  });
}

export function useAssignGroupToCourse(courseId: string, groupId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => assignGroupToCourse(courseId, groupId),
    onSuccess: () => {
      queryClient.invalidateQueries({queryKey: ["courses"]});
    },
  });
}

export function useCoursesByStudent(studentId?: string) {
  return useQuery({
    queryKey: ['courses', 'student', studentId],
    queryFn: () => getCoursesByStudent(studentId!),
    enabled: !!studentId,
  });
}

export function useCoursesByTeacher(teacherId?: string) {
  return useQuery({
    queryKey: ['courses', 'teacher', teacherId],
    queryFn: async () => await getCoursesByTeacher(teacherId!),
    enabled: !!teacherId,
  });
}