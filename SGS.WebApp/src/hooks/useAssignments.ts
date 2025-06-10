import {useMutation, useQueries, useQuery, useQueryClient} from "@tanstack/react-query";
import {CreateAssignmentCommand} from "../types/AssignmentTypes";
import * as assignmentApi from "../api/apiEndpoints/AssignmentAndCourseApi"
import {StudentsGroupDto} from "../types/GroupTypes.ts";
import {CourseDto} from "../types/CourseTypes.ts";

interface UseAssignmentProps {
  assignmentId?: string;
  groupId?: string;
  courseId?: string;
}

export const useAssignments = (props: UseAssignmentProps) => {
  const getAssignmentByIdQuery = useQuery({
    queryKey: ["assignments", props?.assignmentId],
    queryFn: async () => {
      if (!props?.assignmentId) {
        throw new Error("Assignment ID is required");
      }
      return await assignmentApi.getAssignmentById(props.assignmentId);
    },
    enabled: !!props?.assignmentId,
  });

  const getAssignmentsByGroupAndCourseQuery = useQuery({
    queryKey: ["assignments", props?.groupId, props?.courseId],
    queryFn: async () => {
      if (!props?.courseId) {
        throw new Error("Course ID is required");
      }
      return await assignmentApi.getAssignmentsByGroupAndCourse(props.groupId, props.courseId)
    },
    enabled: !!props?.courseId,
  });

  const getAssignmentsByGroupQuery = useQuery({
    queryKey: ["assignments", props?.groupId],
    queryFn: async () => {
      if (!props?.groupId) {
        throw new Error("Group ID is required");
      }
      return await assignmentApi.getAssignmentsByGroup(props.groupId);
    },
    enabled: !!props?.groupId,
  });

  return {getAssignmentByIdQuery, getAssignmentsByGroupAndCourseQuery, getAssignmentsByGroupQuery};
};

export const useAssignmentsMutation = () => {
  const queryClient = useQueryClient();

  const createAssignmentMutation = useMutation({
    mutationFn: (data: CreateAssignmentCommand) => assignmentApi.createAssignment(data),
    onSuccess: () => {
      queryClient.invalidateQueries({queryKey: ["assignments"]});
    },
  });

  return {createAssignmentMutation};
}

export const useAssignmentsByGroups = (groups: StudentsGroupDto[], courses: CourseDto[]) => {
  return useQueries({
    queries: groups?.flatMap(group =>
      courses.map(course => ({
        queryKey: ['assignments', group.id, course.id],
        queryFn: async () => await assignmentApi.getAssignmentsByGroupAndCourse(group.id, course.id),
        enabled: !!group.id && !!course.id,
      }))
    )
  });
};

