import {useMutation, useQuery, useQueryClient} from "@tanstack/react-query";
import * as gradingApi from "../api/apiEndpoints/GradingAndAnalysisApi";
import {EditGradeCommand} from "../types/AnalysisTypes.ts";

interface UseGradingProps {
  docId?: string;
  assignmentId?: string;
  courseId?: string;
  groupId?: string;
}

export const useGrading = (props: UseGradingProps) => {
  const queryClient = useQueryClient();

  const getAnalysisResultsQuery = useQuery({
    queryKey: ["analysisResults", props?.docId],
    queryFn: async () => {
      if (!props?.docId) {
        throw new Error("Document ID is required.");
      }
      return await gradingApi.getAnalysisResults(props.docId)
    },
    enabled: !!props?.docId,
  });

  const getGradingResultsByAssignmentQuery = useQuery({
    queryKey: ["analysisResults", props?.assignmentId],
    queryFn: async () => {
      if (!props?.assignmentId) {
        throw new Error("Assignment ID is required.");
      }
      return await gradingApi.getGradingResultsByAssignment(props.assignmentId!)
    },
    enabled: !!props?.assignmentId
  });

  const getGradingResultByCourseAndGroupQuery = useQuery({
    queryKey: ["grades", props?.courseId, props?.groupId],
    queryFn: async () => {
      if (!props?.courseId || !props?.groupId) {
        throw new Error("CourseId and GroupId are required.");
      }
      return await gradingApi.getGradingResultByCourseAndGroup(props.courseId, props.groupId);
    },
    enabled: !!props?.courseId || !!props?.groupId
  });

  const editGradeMutation = useMutation({
    mutationFn: (data: EditGradeCommand) => gradingApi.editGrade(data),
    onSuccess: () => {
      queryClient.invalidateQueries({queryKey: ["grades"]});
    },
  });

  const finalizeGradeMutation = useMutation({
    mutationFn: (gradeId: string) => gradingApi.finalizeGrade(gradeId),
    onSuccess: () => {
      queryClient.invalidateQueries({queryKey: ["grades"]});
    },
  });

  const analyzeDocumentMutation = useMutation({
    mutationFn: (docId: string) => gradingApi.analyzeDocument(docId),
    onSuccess: () => {
      queryClient.invalidateQueries({queryKey: ["analysisResults"]});
    },
  });

  return {
    analyzeDocumentMutation,
    getAnalysisResultsQuery,
    getGradingResultByCourseAndGroupQuery,
    getGradingResultsByAssignmentQuery,
    editGradeMutation,
    finalizeGradeMutation,
  };
};
