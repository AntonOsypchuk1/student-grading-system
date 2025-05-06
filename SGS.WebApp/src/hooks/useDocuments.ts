import {useMutation, useQuery, useQueryClient} from "@tanstack/react-query";
import * as documentApi from "../api/apiEndpoints/DocumentsApi.ts";

export const useDocuments = (userId: string) => {
  const queryClient = useQueryClient();

  const documentsQuery = useQuery({
    queryKey: ["documents", userId],
    queryFn: () => documentApi.getDocumentsByUser(userId),
    enabled: !!userId
  });

  const uploadMutation = useMutation({
    mutationFn: (formData: FormData) => documentApi.uploadDocument(formData),
    onSuccess: () => {
      queryClient.invalidateQueries({queryKey: ["documents", userId]});
    },
  });

  return {documentsQuery, uploadMutation};
};
