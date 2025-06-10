// features/assignments/layout/AssignmentGrades.tsx
import {useParams} from "react-router-dom";
import {useGrading} from "../../../hooks/useGrading.ts";
import {useAssignments} from "../../../hooks/useAssignments.ts";
import {Table, Td, Th, Tr} from "../../../components/shared/Table";
import {FallbackItem} from "../../../components/shared/StyledComponents";
import styled from "styled-components";

const GradesContainer = styled.div`
    padding: 2rem;
`;

const Header = styled.div`
    margin-bottom: 2rem;

    h2 {
        margin: 0 0 0.5rem 0;
        color: ${props => props.theme.colors.gray[800]};
    }

    p {
        color: ${props => props.theme.colors.gray[500]};
        margin: 0;
    }
`;

const GradeStatus = styled.span<{ $isFinalized: boolean }>`
    display: inline-flex;
    align-items: center;
    padding: 0.25rem 0.75rem;
    border-radius: 9999px;
    font-size: 0.875rem;
    font-weight: 500;
    background: ${props => props.$isFinalized
            ? props.theme.colors.success + '20'
            : props.theme.colors.danger + '20'};
    color: ${props => props.$isFinalized
            ? props.theme.colors.success
            : props.theme.colors.danger};
`;

const AssignmentGrades = () => {
  const {assignmentId} = useParams<{ assignmentId: string }>();
  const {getGradingResultsByAssignmentQuery} = useGrading({assignmentId: assignmentId});
  const {getAssignmentByIdQuery} = useAssignments({assignmentId});

  const {data: gradingResults, isLoading: isGradesLoading} = getGradingResultsByAssignmentQuery;
  const {data: assignment, isLoading: isAssignmentLoading} = getAssignmentByIdQuery;

  if (isGradesLoading || isAssignmentLoading) {
    return <FallbackItem>Завантаження оцінок...</FallbackItem>;
  }

  if (!gradingResults || !assignment) {
    return <FallbackItem>Оцінок не знайдено</FallbackItem>;
  }

  console.log(gradingResults);

  return (
    <GradesContainer>
      <Header>
        <h2>{assignment.title} - Оцінки</h2>
        <p>Всього завантажено: {gradingResults.length}</p>
      </Header>

      <Table>
        <thead>
        <tr>
          <Th>Студент</Th>
          <Th>Дата завантаження</Th>
          <Th>Оцінка</Th>
          <Th>Статус</Th>
          <Th>Дії</Th>
        </tr>
        </thead>
        <tbody>
        {gradingResults.map((result) => (
          <Tr key={result.id}>
            <Td>{result.studentName}</Td>
            <Td>
              {result.submissionDate
                ? new Date(result.submissionDate).toLocaleDateString()
                : 'N/A'
              }
            </Td>
            <Td>
              {!result.editedGrade && !result.generatedGrade ?
                'Завантаження...' :
                `${result.editedGrade ?? result.generatedGrade ?? 0} / 100`
              }
            </Td>
            <Td>
              <GradeStatus $isFinalized={result.isFinalized ?? false}>
                {result.isFinalized ? 'Виставити' : 'В очікуванні'}
              </GradeStatus>
            </Td>
            <Td>
              <a href={`/assignments/${assignmentId}/students?student=${result.studentId}`}>
                Переглянути документ
              </a>
            </Td>
          </Tr>
        ))}
        </tbody>
      </Table>
    </GradesContainer>
  );
};

export default AssignmentGrades;