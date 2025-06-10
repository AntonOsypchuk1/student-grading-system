import {useParams} from "react-router-dom";
import {useGroups} from "../../../hooks/useGroups.ts";
import {useAssignments} from "../../../hooks/useAssignments.ts";
import {useDocuments} from "../../../hooks/useDocuments.ts";
import {
  FallbackItem,
  MainContent,
  Sidebar,
  SidebarItem,
  SidebarList,
} from "../../../components/shared/StyledComponents.ts";
import {useState} from "react";
import styled from "styled-components";
import DocumentItem from "../../documents/DocumentItem.tsx";
import {PdfPreview} from "../sub/PdfPreview.tsx";


const Container = styled.div`
    display: grid;
    grid-template-columns: 250px 1fr;
    gap: 2rem;
`;

const BackButton = styled.button`
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.5rem 1rem;
    margin-bottom: 1rem;
    background: none;
    border: none;
    color: ${props => props.theme.colors.primary};
    cursor: pointer;
    font-weight: 500;

    &:hover {
        text-decoration: underline;
    }
`;

const AssignmentStudents = () => {
  const {assignmentId} = useParams<{ assignmentId: string }>();
  const [selectedStudentId, setSelectedStudentId] = useState<string | null>(null);
  const [isPreviewMode, setIsPreviewMode] = useState(false);

  const {getAssignmentByIdQuery} = useAssignments({assignmentId});
  const {groupId} = getAssignmentByIdQuery.data ?? {};

  const {getGroupByIdQuery} = useGroups({groupId: groupId});
  const {data: group, isLoading: isGroupLoading} = getGroupByIdQuery;

  const {
    getDocumentByUserIdAndAssignmentId,
  } = useDocuments({
    userId: selectedStudentId ?? undefined,
    assignmentId: assignmentId
  });

  const {data: document, isLoading: isDocumentLoading} = getDocumentByUserIdAndAssignmentId;

  if (isGroupLoading) return <FallbackItem>Loading group information...</FallbackItem>;
  if (!group) return <FallbackItem>Group not found</FallbackItem>;
  if (!group.students?.length) return <FallbackItem>No students in this group</FallbackItem>;

  return (
    <Container>
      <Sidebar>
        <h3>{group.name} • Students</h3>
        <SidebarList>
          {group.students.map(student => (
            <SidebarItem
              key={student.id}
              isActive={selectedStudentId === student.id}
              onClick={() => setSelectedStudentId(student.id)}
            >
              {student.fullName}
            </SidebarItem>
          ))}
        </SidebarList>
      </Sidebar>

      <MainContent>
        {!selectedStudentId ? (
          <FallbackItem>
            <svg width="64" height="64" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/>
              <circle cx="9" cy="7" r="4"/>
              <path d="M23 21v-2a4 4 0 0 0-3-3.87"/>
              <path d="M16 3.13a4 4 0 0 1 0 7.75"/>
            </svg>
            <p>Select a student to view their submission and analysis results</p>
          </FallbackItem>
        ) : isDocumentLoading ? (
          <FallbackItem>Loading student's submission...</FallbackItem>
        ) : !document ? (
          <FallbackItem>No submission found for this student</FallbackItem>
        ) : isPreviewMode ? (
          <>
            <BackButton onClick={() => setIsPreviewMode(false)}>
              ← Back to Submission
            </BackButton>
            <PdfPreview documentId={document.id}/>
          </>
        ) : (
          <DocumentItem
            documentDto={document}
            maxGrade={100}
            onView={() => setIsPreviewMode(true)}
          />
        )}
      </MainContent>

    </Container>
  );
};

export default AssignmentStudents;