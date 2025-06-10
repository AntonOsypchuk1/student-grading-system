import {useParams} from "react-router-dom";
import {useCourses} from "../../../hooks/useCourses";
import {useGroups} from "../../../hooks/useGroups";
import {useAssignments} from "../../../hooks/useAssignments";
import {useState} from "react";
import {
  AssignmentCard,
  AssignmentDescription,
  AssignmentHeader,
  AssignmentsList,
  AssignmentTitle,
  ContentGrid,
  CourseContainer,
  CourseDescription,
  CourseHeader,
  CourseStats,
  CourseTitle,
  DueDate,
  ErrorMessage,
  FilterInfo,
  GroupCard,
  GroupCount,
  GroupDetails,
  GroupHeader,
  GroupName,
  GroupsContainer,
  LeftSection,
  LoadingWrapper,
  SectionHeader,
  SectionTitle,
  StatBox,
  StudentItem,
  StudentsList
} from "../styles.ts";
import {StatLabel, StatNumber} from "../../dashboard/styles.tsx";
import {MainContent} from "../../../components/shared/StyledComponents.ts";

export default function CourseDetail() {
  const {courseId} = useParams<{ courseId: string }>();
  const {getCourseByIdQuery} = useCourses(courseId!);
  const {getGroupsByCourseIdQuery} = useGroups({courseId: courseId!});
  const [selectedGroupId, setSelectedGroupId] = useState<string | null>(null);

  const {getAssignmentsByGroupAndCourseQuery} = useAssignments({
    groupId: selectedGroupId ?? undefined,
    courseId: courseId!
  });

  const {data: course, isLoading: courseLoading} = getCourseByIdQuery;
  const {data: groups, isLoading: groupsLoading} = getGroupsByCourseIdQuery;
  const {data: assignments, isLoading: assignmentsLoading} = getAssignmentsByGroupAndCourseQuery;

  if (courseLoading || groupsLoading) {
    return <LoadingWrapper>Loading course details...</LoadingWrapper>;
  }

  if (!course) return <ErrorMessage>Course not found</ErrorMessage>;

  return (
    <CourseContainer>
      <CourseHeader>
        <div>
          <CourseTitle>{course.title}</CourseTitle>
          <CourseDescription>{course.description}</CourseDescription>
        </div>
        <CourseStats>
          <StatBox>
            <StatNumber>{groups?.length || 0}</StatNumber>
            <StatLabel>Groups</StatLabel>
          </StatBox>
          <StatBox>
            <StatNumber>{assignments?.length || 0}</StatNumber>
            <StatLabel>Assignments</StatLabel>
          </StatBox>
        </CourseStats>
      </CourseHeader>

      <ContentGrid>
        <LeftSection>
          <SectionTitle>Groups</SectionTitle>
          <GroupsContainer>
            {groups?.map(group => (
              <GroupCard
                key={group.id}
                isSelected={selectedGroupId === group.id}
                onClick={() => setSelectedGroupId(group.id)}
              >
                <GroupHeader>
                  <GroupName>{group.name}</GroupName>
                  <GroupCount>{group.students?.length || 0} students</GroupCount>
                </GroupHeader>
                {selectedGroupId === group.id && (
                  <GroupDetails>
                    <StudentsList>
                      {group.students?.map(student => (
                        <StudentItem key={student.id}>
                          {student.fullName}
                        </StudentItem>
                      ))}
                    </StudentsList>
                  </GroupDetails>
                )}
              </GroupCard>
            ))}
          </GroupsContainer>
        </LeftSection>

        <MainContent>
          <SectionHeader>
            <SectionTitle>Assignments</SectionTitle>
            <FilterInfo>
              {selectedGroupId
                ? `Showing for ${groups?.find(g => g.id === selectedGroupId)?.name}`
                : 'Showing all assignments'}
            </FilterInfo>
          </SectionHeader>

          {assignmentsLoading ? (
            <LoadingWrapper>Loading assignments...</LoadingWrapper>
          ) : (
            <AssignmentsList>
              {assignments?.map(assignment => (
                <AssignmentCard key={assignment.id}>
                  <AssignmentHeader>
                    <AssignmentTitle>{assignment.title}</AssignmentTitle>
                    <DueDate>Due: {new Date(assignment.dueDate).toLocaleDateString()}</DueDate>
                  </AssignmentHeader>
                  <AssignmentDescription>
                    {assignment.description}
                  </AssignmentDescription>
                </AssignmentCard>
              ))}
            </AssignmentsList>
          )}
        </MainContent>
      </ContentGrid>
    </CourseContainer>
  );
}
