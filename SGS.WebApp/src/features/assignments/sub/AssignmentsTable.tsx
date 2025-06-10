import styled from 'styled-components';
import {Table, Td, Th, Tr} from '../../../components/shared/Table';
import {AssignmentDto} from '../../../types/AssignmentTypes.ts';
import {DocumentDto} from '../../../types/DocumentTypes.ts';
import {Link} from 'react-router-dom';

const GradeStatus = styled.span<{ $isSubmitted: boolean }>`
    display: inline-flex;
    align-items: center;
    padding: 0.25rem 0.75rem;
    border-radius: 9999px;
    font-size: 0.875rem;
    font-weight: 500;
    background: ${props => props.$isSubmitted
            ? props.theme.colors.success + '20'
            : props.theme.colors.danger + '20'};
    color: ${props => props.$isSubmitted
            ? props.theme.colors.success
            : props.theme.colors.danger};
`;

interface AssignmentsTableProps {
  assignments: AssignmentDto[];
  documents?: DocumentDto[];
  showGrade?: boolean;
}

export function AssignmentsTable({
                                   assignments,
                                   documents = [],
                                   showGrade = false
                                 }: AssignmentsTableProps) {
  const submittedAssignmentsMap = new Map(
    documents.map(doc => [doc.assignmentId, doc])
  );

  return (
    <Table>
      <thead>
      <tr>
        <Th>Assignment</Th>
        <Th>Due Date</Th>
        <Th>Status</Th>
        {showGrade && <Th>Grade</Th>}
        <Th>Actions</Th>
      </tr>
      </thead>
      <tbody>
      {assignments.map((assignment) => {
        const document = submittedAssignmentsMap.get(assignment.id);
        const isSubmitted = !!document;
        const isPastDue = new Date(assignment.dueDate) < new Date();

        return (
          <Tr key={assignment.id}>
            <Td>
              <Link to={`/assignments/${assignment.id}`}>
                {assignment.title}
              </Link>
            </Td>
            <Td>{new Date(assignment.dueDate).toLocaleDateString()}</Td>
            <Td>
              <GradeStatus $isSubmitted={isSubmitted}>
                {isSubmitted
                  ? 'Submitted'
                  : isPastDue
                    ? 'Past Due'
                    : 'Pending'}
              </GradeStatus>
            </Td>
            {showGrade && (
              <Td>
                {document?.grade ?? 'N/A'}
              </Td>
            )}
            <Td>
              <Link to={`/assignments/${assignment.id}`}>
                {isSubmitted ? 'View Submission' : 'Submit'}
              </Link>
            </Td>
          </Tr>
        );
      })}
      </tbody>
    </Table>
  );
}