import {useMutation, useQuery, useQueryClient} from "@tanstack/react-query";
import {CreateBatchGradingCriteriaCommand, CreateGradingCriteriaCommand} from "../types/GradingCriteriaTypes";
import * as criteriaApi from "../api/apiEndpoints/GradingCriteriaApi";

export const useCriteria = (criteriaId?: string, assignmentId?: string) => {
  const getCriteriaByIdQuery = useQuery({
    queryKey: ["criteria", criteriaId],
    queryFn: async () => {
      if (!criteriaId) {
        throw new Error("ID is required to fetch criteria");
      }
      return await criteriaApi.getGradingCriteriaById(criteriaId)
    },
    enabled: !!criteriaId,
  });

  const getAllCriteriaQuery = useQuery({
    queryKey: ["criteria"],
    queryFn: () => criteriaApi.getAllGradingCriteria(),
  });

  const getGradingCriteriaByAssignmentId = useQuery({
    queryKey: ["criteria", criteriaId],
    queryFn: async () => {
      if (!assignmentId) {
        throw new Error("Assignment ID is required to fetch criteria");
      }
      return await criteriaApi.getGradingCriteriaByAssignmentId(assignmentId);
    },
    enabled: !!assignmentId,
  });

  return {
    getCriteriaByIdQuery,
    getAllCriteriaQuery,
    getGradingCriteriaByAssignmentId
  };
};

export const useCriteriaMutation = () => {
  const queryClient = useQueryClient();

  const createCriteriaMutation = useMutation({
    mutationFn: (data: CreateGradingCriteriaCommand) => criteriaApi.createGradingCriteria(data),
    onSuccess: () => {
      queryClient.invalidateQueries({queryKey: ["criteria"]});
    },
  });

  const createMultipleGradingCriteria = useMutation({
    mutationFn: (data: CreateBatchGradingCriteriaCommand) => criteriaApi.createMultipleGradingCriteria(data),
    onSuccess: () => {
      queryClient.invalidateQueries({queryKey: ["criteria"]});
    },
  });

  return {
    createCriteriaMutation,
    createMultipleGradingCriteria
  };
};
