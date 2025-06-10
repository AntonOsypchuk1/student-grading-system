import {useMutation, useQueries, useQuery, useQueryClient} from "@tanstack/react-query";
import * as studentsGroupApi from "../api/apiEndpoints/StudentsGroupApi";
import {CreateGroupCommand} from "../types/GroupTypes.ts";
import {useAuthContext} from "../auth/AuthProvider.tsx";


type UseGroupsProps = {
  courseId?: string;
  groupId?: string;
  name?: string;
}

export const useGroups = (props: UseGroupsProps) => {
  const queryClient = useQueryClient();
  const {user} = useAuthContext();

  // const getAllGroupsQuery = useQuery({
  //   queryKey: ["groups"],
  //   queryFn: () => studentsGroupApi.getAllGroups(),
  // });

  const getGroupsByCourseIdQuery = useQuery({
    queryKey: ["groups", props.courseId],
    queryFn: async () => {
      if (!props.courseId) {
        throw new Error("Course ID is required");
      }
      return await studentsGroupApi.getGroupsByCourseId(props.courseId)
    },
    enabled: !!props.courseId,
  });

  const getGroupByIdQuery = useQuery({
    queryKey: ["group", props.groupId],
    queryFn: async () => {
      if (!props.groupId) {
        throw new Error("Group ID is required");
      }
      return await studentsGroupApi.getGroupById(props.groupId)
    },
    enabled: !!props.groupId,
  });

  const getGroupByNameQuery = useQuery({
    queryKey: ["group", props.name],
    queryFn: async () => {
      if (!props.name) {
        throw new Error("Group name is required");
      }
      return await studentsGroupApi.getGroupByName(props.name)
    },
    enabled: !!props.name,
  });

  const getGroupByCurrentStudentQuery = useQuery({
    queryKey: ["group-student"],
    queryFn: () => studentsGroupApi.getGroupByCurrentStudent(),
    enabled: user.data?.role.toLowerCase() === 'student',
  });

  const assignStudentToGroupMutation = useMutation({
    mutationFn: ({groupId, studentId}: {
      groupId: string,
      studentId: string
    }) => studentsGroupApi.assignStudentToGroup(groupId, studentId),
    onSuccess: () => {
      queryClient.invalidateQueries({queryKey: ["group"]});
    },
  });

  const createGroupMutation = useMutation({
    mutationFn: (data: CreateGroupCommand) => studentsGroupApi.createGroup(data),
    onSuccess: () => {
      queryClient.invalidateQueries({queryKey: ["groups"]});
    },
  });

  return {
    // getAllGroupsQuery,
    getGroupByCurrentStudentQuery,
    getGroupsByCourseIdQuery,
    getGroupByIdQuery,
    getGroupByNameQuery,
    assignStudentToGroupMutation,
    createGroupMutation
  };
}

export const useGroupsForCourses = (courseIds: string[]) => {
  return useQueries({
    queries: courseIds.map(courseId => ({
      queryKey: ['groups', courseId],
      queryFn: async () => await studentsGroupApi.getGroupsByCourseId(courseId),
      enabled: !!courseId,
    }))
  });
}