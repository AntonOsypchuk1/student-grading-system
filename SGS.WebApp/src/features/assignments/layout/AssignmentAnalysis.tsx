import {useParams} from "react-router-dom";
import {PdfPreview} from "../sub/PdfPreview.tsx";
import {useGrading} from "../../../hooks/useGrading.ts";
import {useDocuments} from "../../../hooks/useDocuments.ts";
import {useAuthContext} from "../../../auth/AuthProvider.tsx";
import {AnalysisResults} from "../sub/AnalysisResults.tsx";
import {MainContent} from "../../../components/shared/StyledComponents.ts";

const AssignmentAnalysis = () => {
  const {assignmentId} = useParams<{ assignmentId: string }>();
  const {user} = useAuthContext();

  const {getDocumentByUserIdAndAssignmentId} = useDocuments({userId: user.data?.id, assignmentId});
  const {data: document} = getDocumentByUserIdAndAssignmentId;
  const {getAnalysisResultsQuery} = useGrading({docId: document?.id});
  const {data: analysisResult} = getAnalysisResultsQuery;

  return (
    <div>
      {document && (
        <MainContent>
          <PdfPreview documentId={document.id}/>
          <AnalysisResults analysis={analysisResult}/>
        </MainContent>
      )}
    </div>
  );
};

export default AssignmentAnalysis;