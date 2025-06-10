import {useNavigate, useParams} from "react-router-dom";
import {useGroups} from "../../../hooks/useGroups.ts";
import {ErrorMessage, LoadingWrapper} from "../styles.ts";
import styled from 'styled-components';
import {GroupAssignments} from "../../assignments/sub/GroupAssignments.tsx";
import {useAuthContext} from "../../../auth/AuthProvider.tsx";
import {useState} from "react";
import {FallbackItem, MainContent,} from "../../../components/shared/StyledComponents.ts";
import GroupsSidebar from "../sub/GroupsSidebar.tsx";

const LayoutContainer = styled.div<{ hasSidebar: boolean }>`
    position: relative;
    display: grid;
    grid-template-columns: ${({hasSidebar}) =>
            hasSidebar ? '250px 1fr' : '1fr'};
    gap: 20px;
    height: 78vh;
`;

const FloatingButton = styled.button`
    position: absolute;
    bottom: 0;
    right: 0;
    padding: 0.75rem 1.5rem;
    background-color: ${props => props.theme.colors.primary};
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    font-weight: 500;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
    z-index: 10;

    &:hover {
        opacity: 0.9;
        transform: translateY(-1px);
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
    }

    transition: all 0.2s ease;
`;

export default function CourseAssignments() {
  const {courseId} = useParams<{ courseId: string }>();
  const {user} = useAuthContext();
  const userRole = user.data?.role.toLowerCase();
  const navigate = useNavigate();
  const [selectedGroupId, setSelectedGroupId] = useState<string | null>(null);
  const {getGroupsByCourseIdQuery, getGroupByCurrentStudentQuery} = useGroups({courseId});
  const {data: groups, isLoading} = getGroupsByCourseIdQuery;

  if (userRole === 'student' && !selectedGroupId) {
    const {data: group} = getGroupByCurrentStudentQuery;
    if (group) {
      setSelectedGroupId(group.id);
    } else {
      return <ErrorMessage>You are not assigned to any group</ErrorMessage>;
    }
  }

  const handleAddAssignment = () => {
    navigate(`/courses/${courseId}/assignments/new`);
  };

  if (isLoading) return <LoadingWrapper>Loading groups...</LoadingWrapper>;
  if (!groups?.length) return <ErrorMessage>No groups found</ErrorMessage>;

  return (
    <LayoutContainer hasSidebar={userRole === 'teacher'}>
      {userRole === 'teacher' && (
        <GroupsSidebar
          groups={groups}
          selectedGroupId={selectedGroupId}
          onGroupSelect={setSelectedGroupId}
        />
      )}

      <MainContent>
        <h3>Assignments</h3>
        {selectedGroupId ? (
          <GroupAssignments
            groupId={selectedGroupId}
            groupName={groups.find(g => g.id === selectedGroupId)?.name || ''}
            courseId={courseId!}
          />
        ) : (
          <FallbackItem>
            <svg
              width="64"
              height="64"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
            >
              <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/>
              <polyline points="14 2 14 8 20 8"/>
              <line x1="16" y1="13" x2="8" y2="13"/>
              <line x1="16" y1="17" x2="8" y2="17"/>
              <line x1="10" y1="9" x2="8" y2="9"/>
            </svg>
            <p style={{marginTop: '1rem'}}>
              Select a group to view its assignments
            </p>
          </FallbackItem>
        )}
      </MainContent>

      {user?.data?.role.toLowerCase() === 'teacher' && (
        <FloatingButton onClick={handleAddAssignment}>
          Add Assignment
        </FloatingButton>
      )}
    </LayoutContainer>
  );
}