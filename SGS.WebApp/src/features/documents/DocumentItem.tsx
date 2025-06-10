import styled from "styled-components";
import {DocumentDto} from "../../types/DocumentTypes.ts";
import {FallbackItem} from "../../components/shared/StyledComponents.ts";
import {useGrading} from "../../hooks/useGrading.ts";
import {useDocuments} from "../../hooks/useDocuments.ts";
import {GradeEditor} from "./GradeHandler.tsx";

const AnalysisSection = styled.div`
    margin-top: 2rem;
`;

const AnalysisSubSection = styled.div`
    overflow: auto;
    height: 50vh;
`;

const AnalysisHeader = styled.div`
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1.5rem;
`;

const AnalysisCard = styled.div`
    background: white;
    border-radius: 8px;
    padding: 1.5rem;
    border: 1px solid ${props => props.theme.colors.gray[300]};
    margin-bottom: 1rem;

    h4 {
        color: ${props => props.theme.colors.gray[700]};
        margin: 0 0 0.75rem 0;
        font-size: 1rem;
    }

    p {
        color: ${props => props.theme.colors.gray[500]};
        margin: 0;
        line-height: 1.5;
    }
`;

const StrengthsList = styled.ul`
    margin: 0;
    padding-left: 1.5rem;

    li {
        color: ${props => props.theme.colors.gray[500]};
        margin-bottom: 0.5rem;

        &:last-child {
            margin-bottom: 0;
        }
    }
`;

const ActionButton = styled.button`
    padding: 0.5rem 1rem;
    border-radius: 4px;
    border: 1px solid ${props => props.theme.colors.gray[300]};
    background: white;
    cursor: pointer;

    &:hover {
        background: ${props => props.theme.colors.gray[50]};
    }
`;

const SubmitButton = styled.button`
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.625rem 1rem;
    background-color: ${props => props.theme.colors.primary};
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    font-weight: 500;

    &:disabled {
        opacity: 0.7;
        cursor: not-allowed;
    }
`;

const TextContent = styled.div`
    white-space: pre-wrap;
    line-height: 1.5;
    color: ${props => props.theme.colors.gray[500]};
`;

const DocumentCard = styled.div`
    background: white;
    border-radius: 8px;
    padding: 1.5rem;
    border: 1px solid ${props => props.theme.colors.gray[300]};
    margin-bottom: 1rem;
`;

const DocumentActions = styled.div`
    display: flex;
    gap: 1rem;
    margin-top: 1rem;
`;

interface DocumentItemProps {
  documentDto: DocumentDto;
  maxGrade?: number;
  onView?: () => void;
}

const DocumentItem = ({documentDto, maxGrade, onView}: DocumentItemProps) => {
  const {
    getAnalysisResultsQuery,
    analyzeDocumentMutation,
  } = useGrading({docId: documentDto.id});
  const {downloadDocumentQuery} = useDocuments({documentId: documentDto.id});
  const {data: analysisResult} = getAnalysisResultsQuery;

  const handleDownload = async () => {
    try {
      const result = await downloadDocumentQuery.refetch();
      if (result.data) {
        const blob = new Blob([result.data], {type: 'application/pdf'});
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `document-${documentDto.id}.pdf`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      }
    } catch (error) {
      console.error('Помилка завантаження документа:', error);
    }
  };

  console.log(analysisResult);

  return (
    <>
      <DocumentCard>
        <h3>Поданий документ</h3>
        <p>Подано: {new Date(documentDto.uploadedAt).toLocaleString()}</p>
        <DocumentActions>
          <ActionButton onClick={onView}>
            Переглянути
          </ActionButton>
          <ActionButton onClick={handleDownload}>
            Завантажити
          </ActionButton>
        </DocumentActions>
      </DocumentCard>

      <AnalysisSection>
        <AnalysisHeader>
          <h3>Результати аналізу</h3>
          {analysisResult ? (
            <GradeEditor
              documentId={documentDto.id}
              maxGrade={maxGrade}
              analysisResult={analysisResult}
            />
          ) : (
            <SubmitButton
              onClick={() => analyzeDocumentMutation.mutate(documentDto.id)}
              disabled={analyzeDocumentMutation.isPending}
            >
              {analyzeDocumentMutation.isPending ? (
                <>Аналізуємо...</>
              ) : (
                "Почати аналіз"
              )}
            </SubmitButton>
          )}
        </AnalysisHeader>


        {analysisResult ? (
          <AnalysisSubSection>
            <AnalysisCard>
              <h4>Підсумок</h4>
              <TextContent>{analysisResult.generatedSummary}</TextContent>
            </AnalysisCard>

            <AnalysisCard>
              <h4>Сильні сторони</h4>
              {analysisResult.strengths.length > 0 ? (
                <StrengthsList>
                  {analysisResult.strengths.map((strength, index) => (
                    <li key={index}>{strength}</li>
                  ))}
                </StrengthsList>
              ) : (
                <p>Сильних сторін не виявлено</p>
              )}
            </AnalysisCard>

            <AnalysisCard>
              <h4>Області для покращення</h4>
              {analysisResult.areasForImprovement.length > 0 ? (
                <StrengthsList>
                  {analysisResult.areasForImprovement.map((area, index) => (
                    <li key={index}>{area}</li>
                  ))}
                </StrengthsList>
              ) : (
                <p>Областей для покращення не виявлено</p>
              )}
            </AnalysisCard>
          </AnalysisSubSection>
        ) : (
          <FallbackItem>Результати аналізу відсутні</FallbackItem>
        )}
      </AnalysisSection>
    </>
  );
};

export default DocumentItem;