// features/courses/layout/CourseGrades.tsx
import {useParams} from "react-router-dom";
import {useState} from "react";
import styled from "styled-components";
import {useGroups} from "../../../hooks/useGroups";
import {
  FallbackItem,
  MainContent,
  Sidebar,
  SidebarItem,
  SidebarList,
} from "../../../components/shared/StyledComponents";
import {useGrading} from "../../../hooks/useGrading.ts";
import {Table, Td, Th, Tr} from "../../../components/shared/Table.tsx";

const Container = styled.div`
    display: grid;
    grid-template-columns: 250px 1fr;
    gap: 2rem;
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

const GradeCell = styled.div<{ $isFinalized: boolean }>`
    display: inline-flex;
    align-items: center;
    padding: 0.25rem 0.75rem;
    border-radius: 4px;
    font-weight: 500;
    background: ${props => props.$isFinalized
            ? props.theme.colors.success + '20'
            : props.theme.colors.danger + '20'};
    color: ${props => props.$isFinalized
            ? props.theme.colors.success
            : props.theme.colors.danger};
`;

const CourseGrades = () => {
  const {courseId} = useParams<{ courseId: string }>();
  const [selectedGroupId, setSelectedGroupId] = useState<string | null>(null);

  const {getGroupsByCourseIdQuery} = useGroups({courseId});
  const {getGradingResultByCourseAndGroupQuery} = useGrading({
    groupId: selectedGroupId ?? undefined,
    courseId
  });

  const {data: groups, isLoading: isGroupsLoading} = getGroupsByCourseIdQuery;
  const {data: gradingResults, isLoading: isGradingLoading} = getGradingResultByCourseAndGroupQuery;

  console.log("groups: ", groups);
  console.log("gradingResults: ", gradingResults?.assignmentResults);
  if (isGroupsLoading) {
    return <FallbackItem>Loading groups...</FallbackItem>;
  }

  if (!groups?.length) {
    return <FallbackItem>No groups found for this course</FallbackItem>;
  }

  return (
    <Container>
      <Sidebar>
        <h3>Groups</h3>
        <SidebarList>
          {groups.map(group => (
            <SidebarItem
              key={group.id}
              isActive={selectedGroupId === group.id}
              onClick={() => setSelectedGroupId(group.id)}
            >
              {group.name}
            </SidebarItem>
          ))}
        </SidebarList>
      </Sidebar>

      <MainContent>
        {!selectedGroupId ? (
          <FallbackItem>Select a group to view grades</FallbackItem>
        ) : isGradingLoading ? (
          <FallbackItem>Loading grades...</FallbackItem>
        ) : !gradingResults?.assignmentResults.length ? (
          <FallbackItem>No assignments found for this group</FallbackItem>
        ) : (
          <>
            <Header>
              <h2>{groups.find(g => g.id === selectedGroupId)?.name} - Grades</h2>
              <p>Total assignments: {gradingResults.assignmentResults.length}</p>
            </Header>

            <Table>
              <thead>
              <tr>
                <Th>Student</Th>
                {gradingResults.assignmentResults.map((assignment, index) => (
                  <Th key={assignment.id}>{index + 1}</Th>
                ))}
                <Th>Average</Th>
              </tr>
              </thead>
              <tbody>
              {Array.from(new Set(gradingResults.assignmentResults.flatMap(ar =>
                ar.results.map(r => r.studentId)))).map(studentId => {
                const studentResults = gradingResults.assignmentResults.map(assignment => {
                  return assignment.results.find(result => result.studentId === studentId);
                });

                const studentName = studentResults.find(r => r)?.studentName || 'Unknown Student';

                // Calculate average from valid grades
                const validGrades = studentResults
                  .filter(result => result?.editedGrade || result?.generatedGrade || 100)
                  .map(result => result?.editedGrade || result?.generatedGrade || 0);
                const average = validGrades.length
                  ? (validGrades.reduce((a, b) => a + b, 0) / validGrades.length).toFixed(2)
                  : 'N/A';

                return (
                  <Tr key={studentId}>
                    <Td>{studentName}</Td>
                    {studentResults.map((result, index) => (
                      <Td key={`${studentId}-${index}`}>
                        {result ? (
                          <GradeCell $isFinalized={result.isFinalized || false}>
                            {result.editedGrade ?? result.generatedGrade ?? 'N/A'}
                          </GradeCell>
                        ) : (
                          'Not submitted'
                        )}
                      </Td>
                    ))}
                    <Td>{average}</Td>
                  </Tr>
                );
              })}
              </tbody>
            </Table>
          </>
        )}
      </MainContent>
    </Container>
  );
};

export default CourseGrades;