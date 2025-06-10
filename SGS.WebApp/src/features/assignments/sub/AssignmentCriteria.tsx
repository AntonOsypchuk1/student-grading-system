import styled from 'styled-components';
import {useCriteria} from "../../../hooks/useCriteria.ts";

const CriteriaContainer = styled.div`
    margin-top: 24px;
    padding: 24px;
    background: white;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
`;

const CriteriaList = styled.ul`
    list-style: none;
    padding: 0;
`;

const CriteriaItem = styled.li`
    padding: 12px 0;
    border-bottom: 1px solid #eee;

    &:last-child {
        border-bottom: none;
    }
`;

interface AssignmentCriteriaProps {
  assignmentId: string;
}

const AssignmentCriteria = ({assignmentId}: AssignmentCriteriaProps) => {
  const {getGradingCriteriaByAssignmentId} = useCriteria(undefined, assignmentId);

  if (getGradingCriteriaByAssignmentId.isLoading) {
    return <div>Завантаження критеріїв...</div>;
  }

  if (getGradingCriteriaByAssignmentId.isError) {
    return <div>Помилка завантаження критеріїв оцінювання</div>;
  }

  const criteria = getGradingCriteriaByAssignmentId.data;
  console.log(criteria);

  return (
    <CriteriaContainer>
      <h3>Критерії оцінювання</h3>
      <CriteriaList>
        {criteria?.map((criterion) => (
          <CriteriaItem key={criterion.id}>
            <strong>{criterion.title}</strong> - {criterion.description}
            <div>Бали: {criterion.maxGrade}</div>
          </CriteriaItem>
        ))}
      </CriteriaList>
    </CriteriaContainer>
  );
};

export default AssignmentCriteria;