import styled from 'styled-components';
import {useEffect, useState} from 'react';
import {LoadingSpinner} from "../../../components/shared/StyledComponents.ts";
import {useDocuments} from "../../../hooks/useDocuments.ts";

const PreviewContainer = styled.div`
    padding: 24px;
    background: white;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    overflow: auto;
`;

const IframeContainer = styled.div`
    width: 100%;
    height: 500px;
    border: 1px solid ${props => props.theme.colors.gray[100]};
    border-radius: 4px;
    overflow: hidden;
`;

interface PdfPreviewProps {
  documentId: string;
}

export const PdfPreview = ({documentId}: PdfPreviewProps) => {
  const [isLoading, setIsLoading] = useState(true);
  const [pdfUrl, setPdfUrl] = useState<string | null>(null);

  const {downloadDocumentQuery} = useDocuments({documentId});
  const {data: documentContent, isLoading: isDocumentLoading, isError} = downloadDocumentQuery;

  useEffect(() => {
    if (documentContent) {
      const url = URL.createObjectURL(documentContent);
      setPdfUrl(url);

      return () => {
        URL.revokeObjectURL(url);
      };
    }
  }, [documentContent]);

  if (isDocumentLoading || !pdfUrl) {
    return (
      <PreviewContainer>
        <LoadingSpinner/>
      </PreviewContainer>
    );
  }

  if (isError) {
    return (
      <PreviewContainer>
        <div>Помилка завантаження документа: {downloadDocumentQuery.error.message}</div>
      </PreviewContainer>
    );
  }


  return (
    <PreviewContainer>
      <h2>Попередній перегляд документа</h2>
      <IframeContainer>
        {isLoading && <LoadingSpinner/>}
        <iframe
          src={`${pdfUrl}#toolbar=0`}
          width="100%"
          height="100%"
          onLoad={() => setIsLoading(false)}
          style={{border: 'none'}}
          title="Попередній перегляд PDF"
        />
      </IframeContainer>
    </PreviewContainer>
  );
};
