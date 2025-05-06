import {useMutation, useQuery, useQueryClient} from "@tanstack/react-query";
import * as gradingApi from "../api/apiEndpoints/GradingAndAnalysisApi";
import {EditGradeCommand} from "../types/GradingTypes";

export const useGrading = (docId?: string, assignmentId?: string) => {
  const queryClient = useQueryClient();

  const getAnalysisResultsQuery = useQuery({
    queryKey: ["grading", "results", docId],
    queryFn: () => gradingApi.getAnalysisResults(docId!),
    enabled: !!docId,
  });

  const getGradingResultsByAssignmentQuery = useQuery({
    queryKey: ["grading", "assignments", assignmentId],
    queryFn: () => gradingApi.getGradingResultsByAssignment(assignmentId!),
    enabled: !!assignmentId
  });

  const editGradeMutation = useMutation({
    mutationFn: (data: EditGradeCommand) => gradingApi.editGrade(data),
    onSuccess: () => {
      queryClient.invalidateQueries({queryKey: ["grading"]});
    },
  });

  const finalizeGradeMutation = useMutation({
    mutationFn: (gradeId: string) => gradingApi.finalizeGrade(gradeId),
    onSuccess: () => {
      queryClient.invalidateQueries({queryKey: ["grading"]});
    },
  });

  const analyzeDocumentMutation = useMutation({
    mutationFn: (docId: string) => gradingApi.analyzeDocument(docId),
    onSuccess: () => {
      queryClient.invalidateQueries({queryKey: ["grading"]});
    },
  });

  return {
    analyzeDocumentMutation,
    getAnalysisResultsQuery,
    editGradeMutation,
    getGradingResultsByAssignmentQuery,
    finalizeGradeMutation,
  };
};
