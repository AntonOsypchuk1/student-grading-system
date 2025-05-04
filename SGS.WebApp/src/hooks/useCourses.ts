import {useMutation, useQuery, useQueryClient} from "@tanstack/react-query";
import {CreateCourseCommand} from "../types/CourseTypes";
import * as courseApi from "../services/apiEndpoints/AssignmentAndCourseApi"

interface UseCourseProps {
  courseId?: string;
  teacherId?: string;
  studentId?: string;
}

export const useCourses = (props: UseCourseProps) => {
  const queryClient = useQueryClient();

  const getCourseByIdQuery = useQuery({
    queryKey: ["courses", props.courseId],
    queryFn: () => courseApi.getCourseById(props.courseId!),
    enabled: !!props.courseId
  });

  const getCoursesByTeacherQuery = useQuery({
    queryKey: ["courses", "teacher", props.teacherId],
    queryFn: () => courseApi.getCoursesByTeacher(props.teacherId!),
    enabled: !!props.teacherId
  });

  const getCoursesByStudentQuery = useQuery({
    queryKey: ["courses", "student", props.studentId],
    queryFn: () => courseApi.getCoursesByStudent(props.studentId!),
    enabled: !!props.studentId
  });

  const createCourseMutation = useMutation({
    mutationFn: (data: CreateCourseCommand) => courseApi.createCourse(data),
    onSuccess: () => {
      queryClient.invalidateQueries({queryKey: ["courses"]});
    },
  });

  return {
    createCourseMutation,
    getCourseByIdQuery,
    getCoursesByTeacherQuery,
    getCoursesByStudentQuery
  };
};
