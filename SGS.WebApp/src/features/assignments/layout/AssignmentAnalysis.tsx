import {useParams} from "react-router-dom";
import {PdfPreview} from "../sub/PdfPreview.tsx";
import {useGrading} from "../../../hooks/useGrading.ts";
import {useDocuments} from "../../../hooks/useDocuments.ts";
import {useAuthContext} from "../../../auth/AuthProvider.tsx";
import {AnalysisResults} from "../sub/AnalysisResults.tsx";

const AssignmentAnalysis = () => {
  const {assignmentId} = useParams<{ assignmentId: string }>();
  // const {getAssignmentByIdQuery} = useAssignments({assignmentId});
  // const {data: assignment} = getAssignmentByIdQuery;
  const {user} = useAuthContext();

  console.log(assignmentId);

  const {getDocumentByUserIdAndAssignmentId} = useDocuments({userId: user.data?.id, assignmentId});
  const {data: document} = getDocumentByUserIdAndAssignmentId;
  const {getAnalysisResultsQuery} = useGrading({docId: document?.id});
  const {data: analysisResult} = getAnalysisResultsQuery;

  console.log(document);
  console.log(analysisResult);

  return (
    <div>
      {document && (
        <>
          <PdfPreview documentId={document.id}/>
          <AnalysisResults analysis={analysisResult}/>
        </>
      )}
    </div>
  );
};

export default AssignmentAnalysis;