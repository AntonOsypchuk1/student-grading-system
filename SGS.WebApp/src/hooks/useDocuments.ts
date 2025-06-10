import {useMutation, useQuery, useQueryClient} from "@tanstack/react-query";
import * as documentApi from "../api/apiEndpoints/DocumentsApi.ts";
import {deleteDocument} from "../api/apiEndpoints/DocumentsApi.ts";

interface UseDocumentProps {
  userId?: string;
  documentId?: string;
  assignmentId?: string;
}

export const useDocuments = (props: UseDocumentProps) => {
  const queryClient = useQueryClient();

  const getDocumentsByUserId = useQuery({
    queryKey: ["documents", props?.userId],
    queryFn: async () => {
      if (!props?.userId) {
        throw new Error("User ID is required");
      }
      return await documentApi.getDocumentsByUser(props.userId)
    },
    enabled: !!props?.userId
  });

  const getDocumentById = useQuery({
    queryKey: ["document", props?.documentId],
    queryFn: async () => {
      if (!props?.documentId) {
        throw new Error("Document ID is required");
      }
      return await documentApi.getDocumentById(props.documentId)
    },
    enabled: !!props?.documentId
  });

  const getDocumentsByAssignmentIdQuery = useQuery({
    queryKey: ["documentsByAssignment", props?.assignmentId],
    queryFn: async () => {
      if (!props?.assignmentId) {
        throw new Error("Assignment ID is required");
      }
      return await documentApi.getDocumentsByAssignmentId(props.assignmentId);
    },
    enabled: !!props?.assignmentId,
  });

  const getDocumentByUserIdAndAssignmentId = useQuery({
    queryKey: ["documentByUser"],
    queryFn: async () => {
      if (!props?.userId || !props?.assignmentId) {
        throw new Error("User ID and Assignment ID are required");
      }
      return await documentApi.getDocumentByUserIdAndAssignmentId(props.userId, props.assignmentId);
    },
    enabled: !!props?.userId && !!props?.assignmentId,
  });

  const downloadDocumentQuery = useQuery({
    queryKey: ["documentContent", props?.documentId],
    queryFn: async () => {
      if (!props?.documentId) {
        throw new Error("Document ID is required");
      }
      return await documentApi.downloadDocument(props.documentId);
    },
    enabled: !!props?.documentId,
    staleTime: 5 * 60 * 1000
  });

  const deleteDocumentMutation = useMutation({
    mutationFn: (documentId: string) => deleteDocument(documentId),
    onSuccess: () => {
      queryClient.invalidateQueries({queryKey: ["documents", props.userId, "document", props.documentId]});
    },
  });

  const uploadDocumentMutation = useMutation({
    mutationFn: (formData: FormData) => documentApi.uploadDocument(formData),
    onSuccess: () => {
      queryClient.invalidateQueries({queryKey: ["documents", props.userId]});
    },
  });

  return {
    getDocumentsByUserId,
    getDocumentById,
    getDocumentsByAssignmentIdQuery,
    getDocumentByUserIdAndAssignmentId,
    uploadDocumentMutation,
    downloadDocumentQuery,
    deleteDocumentMutation
  };
};
