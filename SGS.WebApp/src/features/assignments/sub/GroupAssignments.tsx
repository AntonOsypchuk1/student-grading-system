import styled from 'styled-components';
import {useAssignments} from '../../../hooks/useAssignments.ts';
import {AssignmentsList} from './AssignmentsList.tsx';

const GroupContainer = styled.div`
    margin-bottom: 2rem;
    overflow: scroll;
    border-right: none;
`;

const GroupHeader = styled.h2`
    font-size: 1.25rem;
    margin-bottom: 1rem;
`;

interface GroupAssignmentsProps {
  groupId: string;
  groupName: string;
  courseId: string;
}

export function GroupAssignments({groupId, groupName, courseId}: GroupAssignmentsProps) {
  const {getAssignmentsByGroupAndCourseQuery} = useAssignments({courseId, groupId});
  const {data: assignments, isLoading} = getAssignmentsByGroupAndCourseQuery;

  if (isLoading) return <div>Завантаження завдань...</div>;
  if (!assignments?.length) return <div>Завдань не знайдено</div>;

  return (
    <GroupContainer>
      <GroupHeader>{groupName}</GroupHeader>
      {!isLoading && (
        <AssignmentsList assignments={assignments}/>
      )}
    </GroupContainer>
  );
}