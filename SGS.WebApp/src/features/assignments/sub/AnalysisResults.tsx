import styled from 'styled-components';
import {AnalysisResultDto} from "../../../types/AnalysisTypes.ts";

const AnalysisContainer = styled.div`
    margin-top: 24px;
    padding: 24px;
    background: white;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
`;

const GradeSection = styled.div`
    display: flex;
    align-items: center;
    gap: 16px;
    margin-bottom: 24px;
    padding: 16px;
    background: ${props => props.theme.colors.gray[50]};
    border-radius: 8px;
`;

const Grade = styled.div`
    font-size: 2rem;
    font-weight: bold;
    color: ${props => props.theme.colors.primary};
`;

const FeedbackSection = styled.div`
    margin-top: 24px;
`;

interface AnalysisResultsProps {
  analysis: AnalysisResultDto | undefined;
}

export const AnalysisResults = ({analysis}: AnalysisResultsProps) => {
  if (!analysis) {
    return (
      <AnalysisContainer>
        <p>Цей документ ще не було проаналізовано.</p>
      </AnalysisContainer>
    );
  }

  return (
    <AnalysisContainer>
      <h2>Результат аналізу</h2>
      <GradeSection>
        <Grade>{analysis.grade?.editedGrade ?? analysis.grade?.generatedGrade}</Grade>
        <div>
          <div style={{fontSize: '0.875rem', color: '#666'}}>
            Проаналізовано: {new Date(analysis.createdAt).toLocaleDateString()}
          </div>
        </div>
      </GradeSection>

      <FeedbackSection>
        <h3>Результат</h3>
        <p>{analysis.generatedSummary}</p>
        <br/>
        <h3>Сильні сторони</h3>
        <ul>{analysis.strengths.map(s => (
          <li key={s}>{s}</li>
        ))}</ul>
        <br/>
        <h3>Області для покращення</h3>
        <ul>{analysis.areasForImprovement.map(a => (
          <li key={a}>{a}</li>
        ))}</ul>
      </FeedbackSection>
    </AnalysisContainer>
  );
};
