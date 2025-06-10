import styled from 'styled-components';
import {AssignmentDto} from '../../../types/AssignmentTypes.ts';
import {Link} from "react-router-dom";

const List = styled.div`
    display: grid;
    gap: 1rem;
    overflow-block: visible;
    max-height: 60vh;
`;

const AssignmentCard = styled.div`
    padding: 1.5rem;
    border: 1px solid ${props => props.theme.colors.gray[300]};
    border-radius: 8px;
    background: white;
    transition: all 0.2s ease;

    &:hover {
        transform: translateY(-2px);
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
        border-color: ${props => props.theme.colors.gray[300]};
    }
`;

const AssignmentLink = styled(Link)`
    text-decoration: none;
    color: inherit;
`;

const Title = styled.h3`
    font-size: 1.125rem;
    color: ${props => props.theme.colors.gray[800]};
    margin: 0 0 0.5rem 0;
`;

const Description = styled.p`
    font-size: 0.875rem;
    color: ${props => props.theme.colors.gray[500]};
    margin: 0 0 1rem 0;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    //overflow: hidden;
`;

const Meta = styled.div`
    display: flex;
    gap: 1rem;
    font-size: 0.875rem;
    color: ${props => props.theme.colors.gray[500]};
`;

const FallbackItem = styled.div`
    text-align: center;
    padding: 2rem;
    color: ${props => props.theme.colors.gray[500]}
`

interface AssignmentsListProps {
  assignments: AssignmentDto[];
}

export function AssignmentsList({assignments}: AssignmentsListProps) {
  if (!assignments?.length) {
    return (
      <FallbackItem>
        Завдань не було знайдено
      </FallbackItem>
    );
  }

  return (
    <List>
      {assignments.map(assignment => (
        <AssignmentLink
          key={assignment.id}
          to={`/assignments/${assignment.id}`}
        >
          <AssignmentCard>
            <Title>{assignment.title}</Title>
            <Description>{assignment.description}</Description>
            <Meta>
              <span>Строк: {new Date(assignment.dueDate).toLocaleDateString()}</span>
              <span>•</span>
              <span>Макс. спроб: {assignment.maxAttempts}</span>
            </Meta>
          </AssignmentCard>
        </AssignmentLink>
      ))}
    </List>
  );
}