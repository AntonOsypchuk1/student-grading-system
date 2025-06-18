import styled from 'styled-components';
import {AssignmentDto} from "../../../types/AssignmentTypes.ts";
import {Link} from "react-router-dom";

interface AssignmentDescriptionProps {
  assignment: AssignmentDto;
  isStudent?: boolean;
}

const DescriptionContainer = styled.div`
    padding: 24px;
    background: white;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
`;

const Title = styled.h3`
    margin-bottom: 16px;
    display: flex;
    justify-content: space-between;
`;

const UpdateButton = styled(Link)`
    display: inline-block;
    padding: 6px 12px;
    background-color: ${props => props.theme.colors.primary};
    color: white;
    font-weight: 450;
    border-radius: 4px;
    text-decoration: none;
    margin-bottom: 20px;
    font-size: 12px;

    &:hover {
        background-color: ${props => props.theme.colors.primaryHover};
    }
`;

const AssignmentDescription = ({assignment, isStudent}: AssignmentDescriptionProps) => {
  return (
    <DescriptionContainer>
      <Title>
        {assignment.title}
        {!isStudent && (
          <UpdateButton to={`/assignments/${assignment.id}/update`}>
            Редагувати завдання
          </UpdateButton>
        )}
      </Title>
      <div>{assignment.description}</div>
      <div style={{marginTop: '16px'}}>
        <p><strong>Крайня дата:</strong> {new Date(assignment.dueDate).toLocaleDateString()}</p>
        {assignment.maxAttempts && <p><strong>Макс. спроб:</strong> {assignment.maxAttempts}</p>}
      </div>
    </DescriptionContainer>
  );
};

export default AssignmentDescription;