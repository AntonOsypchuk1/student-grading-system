// components/assignments/AssignmentAnalysis.tsx
import {useForm} from 'react-hook-form';
import styled from 'styled-components';
import {useState} from 'react';
import {useDocuments} from "../../../hooks/useDocuments.ts";
import {useNavigate} from "react-router-dom";

const SubmissionContainer = styled.div`
    margin-top: 24px;
    padding: 24px;
    background: white;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
`;

const FileUploadContainer = styled.div`
    margin-top: 24px;
    margin-bottom: 16px;
`;

const UploadArea = styled.div<{ isDragActive?: boolean; hasError?: boolean }>`
    border: 2px dashed ${props =>
            props.hasError
                    ? props.theme.colors.danger
                    : props.isDragActive
                            ? props.theme.colors.primary
                            : props.theme.colors.gray[300]};
    border-radius: 8px;
    padding: 24px;
    text-align: center;
    background: ${props =>
            props.isDragActive
                    ? `${props.theme.colors.primary}11`
                    : props.theme.colors.white};
    transition: all ${props => props.theme.transitions.default};
    cursor: pointer;

    &:hover {
        border-color: ${props => props.theme.colors.primary};
    }
`;

const HiddenInput = styled.input`
    display: none;
`;

const FileInfo = styled.div`
    margin-top: 16px;
    padding: 12px;
    background: ${props => props.theme.colors.gray[100]};
    border-radius: 4px;
    display: flex;
    align-items: center;
    gap: 12px;
`;

const FileIcon = styled.div`
    color: ${props => props.theme.colors.primary};
    font-size: 24px;
`;

const FileDetails = styled.div`
    flex: 1;
`;

const FileName = styled.div`
    font-weight: 500;
    color: ${props => props.theme.colors.gray[800]};
`;

const FileSize = styled.div`
    font-size: 0.875rem;
    color: ${props => props.theme.colors.gray[500]};
`;

const RemoveButton = styled.button`
    background: none;
    border: none;
    color: ${props => props.theme.colors.danger};
    cursor: pointer;
    padding: 4px 8px;
    border-radius: 4px;

    &:hover {
        background: ${props => `${props.theme.colors.danger}11`};
    }
`;

const SubmitButton = styled.button`
    background-color: ${props => props.theme.colors.primary};
    color: white;
    padding: 12px 24px;
    border-radius: 8px;
    border: none;
    font-weight: 500;
    cursor: pointer;
    width: 100%;

    &:disabled {
        opacity: 0.5;
        cursor: not-allowed;
    }

    &:hover:not(:disabled) {
        background-color: ${props => props.theme.colors.primary};
    }
`;

const ErrorText = styled.p`
    color: ${props => props.theme.colors.danger};
    font-size: 0.875rem;
    margin-top: 8px;
`;

interface SubmissionForm {
  file: FileList;
}

interface AssignmentSubmissionProps {
  assignmentId: string;
  userId: string | undefined;
}

const formatFileSize = (bytes: number) => {
  if (bytes === 0) return '0 Bytes';
  const k = 1024;
  const sizes = ['Bytes', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return `${parseFloat((bytes / Math.pow(k, i)).toFixed(2))} ${sizes[i]}`;
};

const AssignmentSubmission = ({assignmentId, userId}: AssignmentSubmissionProps) => {
  const [dragActive, setDragActive] = useState(false);
  const {register, handleSubmit, formState: {errors, isSubmitting}, watch, setValue} = useForm<SubmissionForm>();
  const selectedFile = watch('file')?.[0];
  const {uploadDocumentMutation} = useDocuments({});
  const navigate = useNavigate();

  const onSubmit = async (data: SubmissionForm) => {
    try {
      const formData = new FormData();
      formData.append('File', data.file[0]);
      formData.append('UserId', userId ?? '');
      formData.append('AssignmentId', assignmentId);
      console.log(assignmentId, userId, data.file[0]);

      await uploadDocumentMutation.mutateAsync(formData);
      navigate(`/assignments/${assignmentId}`);
    } catch (error) {
      console.error('Error uploading file:', error);
    }
  };

  const handleDrag = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (e.type === "dragenter" || e.type === "dragover") {
      setDragActive(true);
    } else if (e.type === "dragleave") {
      setDragActive(false);
    }
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setDragActive(false);
    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
      const file = e.dataTransfer.files[0];
      if (file.type === 'application/pdf') {
        setValue('file', e.dataTransfer.files);
      }
    }
  };

  const removeFile = () => {
    // @ts-expect-error-error
    setValue('file', undefined);
  };

  return (
    <SubmissionContainer>
      <h2>Submit Your Work</h2>
      <form onSubmit={handleSubmit(onSubmit)}>
        <FileUploadContainer>
          <UploadArea
            isDragActive={dragActive}
            hasError={!!errors.file}
            onDragEnter={handleDrag}
            onDragLeave={handleDrag}
            onDragOver={handleDrag}
            onDrop={handleDrop}
            onClick={() => document.getElementById('file-upload')?.click()}
          >
            {selectedFile ? (
              <FileInfo>
                <FileIcon>📄</FileIcon>
                <FileDetails>
                  <FileName>{selectedFile.name}</FileName>
                  <FileSize>{formatFileSize(selectedFile.size)}</FileSize>
                </FileDetails>
                <RemoveButton type="button" onClick={removeFile}>
                  Remove
                </RemoveButton>
              </FileInfo>
            ) : (
              <>
                <p>Drag and drop your PDF file here, or click to select</p>
                <p style={{fontSize: '0.875rem', color: '#666', marginTop: '8px'}}>
                  Supported format: PDF
                </p>
              </>
            )}
          </UploadArea>
          <HiddenInput
            id="file-upload"
            type="file"
            accept=".pdf"
            {...register('file', {
              required: 'Please select a file',
              validate: {
                isPDF: (files) =>
                  !files?.[0] || files[0]?.type === 'application/pdf' || 'Only PDF files are allowed'
              }
            })}
          />
          {errors.file && (
            <ErrorText>{errors.file.message}</ErrorText>
          )}
        </FileUploadContainer>
        <SubmitButton type="submit" disabled={isSubmitting || !selectedFile}>
          {isSubmitting ? 'Uploading...' : 'Submit Assignment'}
        </SubmitButton>
      </form>
    </SubmissionContainer>
  );
};

export default AssignmentSubmission;