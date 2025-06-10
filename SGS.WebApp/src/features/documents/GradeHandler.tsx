// components/documents/GradeEditor.tsx
import React, {useEffect, useState} from 'react';
import styled from 'styled-components';
import {useGrading} from '../../hooks/useGrading';
import {AnalysisResultDto} from "../../types/AnalysisTypes.ts";

const GradeSection = styled.div`
    display: flex;
    gap: 1rem;
    align-items: center;
`;

const GradeBox = styled.div<{ isFinal: boolean }>`
    display: inline-flex;
    align-items: center;
    padding: 0.5rem 1rem;
    border-radius: 4px;
    background: ${props => props.isFinal
            ? props.theme.colors.success + '20'
            : props.theme.colors.danger + '20'};
    color: ${props => props.isFinal
            ? props.theme.colors.success
            : props.theme.colors.danger};
    font-weight: 500;
`;

const EditableGradeBox = styled(GradeBox)`
    input {
        width: 50px;
        padding: 0.25rem;
        border: 1px solid ${props => props.theme.colors.gray[300]};
        border-radius: 4px;
        text-align: center;
        font-size: 1rem;
        margin: 0 0.25rem;
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

const EditButton = styled(ActionButton)`
    color: ${props => props.theme.colors.primary};
    border-color: ${props => props.theme.colors.primary};
`;

const FinalizeButton = styled(ActionButton)`
    background: ${props => props.theme.colors.success};
    color: white;
    border-color: ${props => props.theme.colors.success};

    &:hover {
        background: ${props => props.theme.colors.success}ee;
    }
`;

interface GradeEditorProps {
  documentId: string;
  maxGrade?: number;
  analysisResult: AnalysisResultDto;
}

export const GradeEditor: React.FC<GradeEditorProps> = ({
                                                          documentId,
                                                          maxGrade = 100,
                                                          analysisResult
                                                        }) => {
  const [isEditing, setIsEditing] = useState(false);
  const [editedGrade, setEditedGrade] = useState<number | null>(null);
  const [grade, setGrade] = useState<number | null>(null);

  const {
    editGradeMutation,
    finalizeGradeMutation
  } = useGrading({docId: documentId});

  useEffect(() => {
    if (analysisResult?.grade) {
      const newGrade = analysisResult.grade.editedGrade ?? analysisResult.grade.generatedGrade;
      setGrade(newGrade);
    }
  }, [analysisResult?.grade]);

  const handleEditGrade = () => {
    if (!analysisResult?.grade?.id) return;
    setIsEditing(!isEditing);
    setEditedGrade(grade || 0);
  };

  const handleFinalizeGrade = () => {
    if (!analysisResult?.grade?.id) return;

    const confirmed = window.confirm('Ви впевнені що хочете виставити фінальну оцінку? Цю дію неможа відмінити.');
    if (!confirmed) return;

    finalizeGradeMutation.mutate(analysisResult?.grade.id);
  };

  const handleGradeSubmit = () => {
    if (!analysisResult?.grade?.id || editedGrade === null) return;

    if (editedGrade < 0 || editedGrade > maxGrade) {
      alert(`Будь-ласка, введіть правильно від 0 до ${maxGrade}`);
      return;
    }

    editGradeMutation.mutate({
      gradeId: analysisResult.grade.id,
      editedGrade: editedGrade,
      isFinalized: false
    });

    setGrade(editedGrade);
    setIsEditing(false);
  };

  const handleGradeChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = parseInt(e.target.value, 10);
    if (!isNaN(value)) {
      setEditedGrade(value);
    }
  };

  return (
    <GradeSection>
      <EditableGradeBox isFinal={analysisResult.isApproved}>
        Оцінка: {isEditing ? (
        <>
          <input
            type="number"
            value={editedGrade ?? ''}
            onChange={handleGradeChange}
            onBlur={handleGradeSubmit}
            min={0}
            max={maxGrade}
            onKeyDown={(e) => {
              if (e.key === 'Enter') {
                handleGradeSubmit();
              } else if (e.key === 'Escape') {
                setIsEditing(false);
                setEditedGrade(null);
              }
            }}
            autoFocus
          />
        </>
      ) : (
        editGradeMutation.isPending ? (
          <>Завантаження...</>
        ) : (
          <>{`${grade} / ${maxGrade}`}</>
        )
      )}
      </EditableGradeBox>
      {!analysisResult.isApproved && (
        <>
          <EditButton onClick={handleEditGrade}>Коригувати</EditButton>
          <FinalizeButton onClick={handleFinalizeGrade}>Виставити</FinalizeButton>
        </>
      )}
    </GradeSection>
  );
};