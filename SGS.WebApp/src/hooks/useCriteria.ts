import {useMutation, useQuery, useQueryClient} from "@tanstack/react-query";
import {CreateGradingCriteriaCommand} from "../types/GradingCriteriaTypes";
import * as criteriaApi from "../services/apiEndpoints/GradingCriteriaApi";

export const useCriteria = (id?: string) => {
  const queryClient = useQueryClient();

  const getCriteriaByIdQuery = useQuery({
    queryKey: ["criteria", id],
    queryFn: () => criteriaApi.getGradingCriteriaById(id!),
    enabled: !!id,
  });

  const getAllCriteriaQuery = useQuery({
    queryKey: ["criteria", "all"],
    queryFn: () => criteriaApi.getAllGradingCriteria(),
  });

  const createCriteriaMutation = useMutation({
    mutationFn: (data: CreateGradingCriteriaCommand) => criteriaApi.createGradingCriteria(data),
    onSuccess: () => {
      queryClient.invalidateQueries({queryKey: ["criteria", "all"]});
    },
  });

  return {createCriteriaMutation, getCriteriaByIdQuery, getAllCriteriaQuery};
};
