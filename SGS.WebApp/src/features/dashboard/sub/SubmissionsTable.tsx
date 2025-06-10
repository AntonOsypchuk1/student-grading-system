import styled from "styled-components";
import {Table, Td, Th} from "../../../components/shared/Table.tsx";
import {Link} from "react-router-dom";

const StatusBadge = styled.span<{ status: 'pending' | 'graded' }>`
    padding: 0.25rem 0.75rem;
    border-radius: 9999px;
    font-size: 0.875rem;
    background: ${props =>
            props.status === 'pending'
                    ? props.theme.colors.danger + '20'
                    : props.theme.colors.success + '20'};
    color: ${props =>
            props.status === 'pending'
                    ? props.theme.colors.danger
                    : props.theme.colors.success};
`;

export const SubmissionsTable = ({submissions}) => (
  <Table>
    <thead>
    <tr>
      <Th>Student</Th>
      <Th>Course</Th>
      <Th>Assignment</Th>
      <Th>Submitted</Th>
      <Th>Status</Th>
      <Th>Actions</Th>
    </tr>
    </thead>
    <tbody>
    {submissions.map(submission => (
      <tr key={submission.id}>
        <Td>{submission.studentName}</Td>
        <Td>{submission.courseName}</Td>
        <Td>{submission.assignmentTitle}</Td>
        <Td>{new Date(submission.submittedAt).toLocaleDateString()}</Td>
        <Td>
          <StatusBadge status={submission.isGraded ? 'graded' : 'pending'}>
            {submission.isGraded ? 'Graded' : 'Pending'}
          </StatusBadge>
        </Td>
        <Td>
          <Link to={`/assignments/${submission.assignmentId}/grade/${submission.id}`}>
            {submission.isGraded ? 'View' : 'Grade'}
          </Link>
        </Td>
      </tr>
    ))}
    </tbody>
  </Table>
);