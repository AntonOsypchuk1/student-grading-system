import styled from 'styled-components';
import {AssignmentDto} from "../../../types/AssignmentTypes.ts";

interface AssignmentDescriptionProps {
  assignment: AssignmentDto;
}

const DescriptionContainer = styled.div`
    padding: 24px;
    background: white;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
`;

const Title = styled.h3`
    margin-bottom: 16px;
`;

const AssignmentDescription = ({assignment}: AssignmentDescriptionProps) => {
  return (
    <DescriptionContainer>
      <Title>{assignment.title}</Title>
      <div>{assignment.description}</div>
      <div style={{marginTop: '16px'}}>
        <p><strong>Due date:</strong> {new Date(assignment.dueDate).toLocaleDateString()}</p>
        {assignment.maxAttempts && <p><strong>Maximum attempts:</strong> {assignment.maxAttempts}</p>}
      </div>
    </DescriptionContainer>
  );
};

export default AssignmentDescription;