import {useMutation, useQuery, useQueryClient} from "@tanstack/react-query";
import {CreateAssignmentCommand} from "../types/AssignmentTypes";
import * as assignmentApi from "../api/apiEndpoints/AssignmentAndCourseApi"

interface UseAssignmentProps {
  assignmentId: string;
  groupId: string;
}

export const useAssignments = (props: UseAssignmentProps) => {
  const queryClient = useQueryClient();

  const getAssignmentByIdQuery = useQuery({
    queryKey: ["assignments", props.assignmentId],
    queryFn: () => assignmentApi.getAssignmentById(props.assignmentId),
    enabled: !!props.assignmentId,
  });

  const getAssignmentsByGroupQuery = useQuery({
    queryKey: ["assignments", "group", props.groupId],
    queryFn: () => assignmentApi.getAssignmentsByGroup(props.groupId),
    enabled: !!props.groupId,
  });

  const createAssignmentMutation = useMutation({
    mutationFn: (data: CreateAssignmentCommand) => assignmentApi.createAssignment(data),
    onSuccess: () => {
      queryClient.invalidateQueries({queryKey: ["assignments"]});
    },
  });

  return {createAssignmentMutation, getAssignmentByIdQuery, getAssignmentsByGroupQuery};
};
