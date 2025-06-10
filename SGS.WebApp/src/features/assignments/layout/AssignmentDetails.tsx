import styled from 'styled-components';
import {useAuthContext} from "../../../auth/AuthProvider.tsx";
import AssignmentDescription from "../sub/AssignmentDescription.tsx";
import AssignmentCriteria from '../sub/AssignmentCriteria.tsx';
import AssignmentSubmission from "../sub/AssignmentSubmission.tsx";
import {useAssignments} from "../../../hooks/useAssignments.ts";
import {ErrorMessage, LoadingSpinner} from '../../../components/shared/StyledComponents.ts';
import {useParams} from "react-router-dom";

const PageContainer = styled.div`
    max-width: 1200px;
    margin: 0 auto;
    padding: 24px;
    overflow: auto;
    height: 90%;
`;

const AssignmentDetails = () => {
  const {assignmentId} = useParams<{ assignmentId: string }>();
  const {user} = useAuthContext();
  const isStudent = user.data?.role.toLowerCase() === 'student';
  const {getAssignmentByIdQuery} = useAssignments({assignmentId});
  const {data: assignment, isLoading, isError} = getAssignmentByIdQuery;

  if (isLoading) {
    return (
      <PageContainer>
        <LoadingSpinner/>
      </PageContainer>
    );
  }

  if (isError || !assignment) {
    return (
      <PageContainer>
        <ErrorMessage>Failed to load assignment details</ErrorMessage>
      </PageContainer>
    );
  }

  return (
    <PageContainer>
      <AssignmentDescription assignment={assignment}/>
      <AssignmentCriteria assignmentId={assignment.id}/>
      {isStudent && (
        <AssignmentSubmission
          assignmentId={assignment.id}
          userId={user.data?.id}
        />
      )}
    </PageContainer>
  );
};

export default AssignmentDetails;