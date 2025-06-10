import {useAuth} from "../../../hooks/useAuth.ts";
import {useCoursesByStudent} from "../../../hooks/useCourses.ts";
import {useState} from "react";
import {useAssignments} from "../../../hooks/useAssignments.ts";
import {useGroups} from "../../../hooks/useGroups.ts";
import {Calendar} from "../sub/Calendar.tsx";
import {useDocuments} from "../../../hooks/useDocuments.ts";
import {AssignmentsTable} from "../../assignments/sub/AssignmentsTable.tsx";
import {AssignmentsList} from "../../assignments/sub/AssignmentsList.tsx";
import {
  DashboardContainer,
  DashboardContent,
  DashboardHeader,
  LeftPanel,
  MainPanel,
  StatItem,
  StatLabel,
  StatNumber,
  Stats,
  Title,
  Widget,
  WidgetTitle
} from "../styles.tsx";

export default function StudentDashboard() {
  const {user} = useAuth();
  const {data: courses, isLoading: coursesLoading} = useCoursesByStudent(user?.id);
  const {getGroupByCurrentStudentQuery} = useGroups({});
  const {getAssignmentsByGroupQuery} = useAssignments({groupId: getGroupByCurrentStudentQuery.data?.id});
  const {data: assignments, isLoading: assignmentsLoading} = getAssignmentsByGroupQuery;

  const {getDocumentsByUserId} = useDocuments({userId: user?.id})
  const {data: userDocuments, isLoading: documentsLoading} = getDocumentsByUserId;

  const [selectedDate, setSelectedDate] = useState(new Date());

  if (coursesLoading || assignmentsLoading || documentsLoading)
    return <div>Завантаження...</div>;

  const submittedAssignmentsMap = new Map(
    userDocuments?.map(doc => [doc.assignmentId, doc]) || []
  );

  const pendingAssignments = assignments?.filter(
    assignment => !submittedAssignmentsMap.has(assignment.id)
  ) || [];

  const submittedAssignments = assignments?.filter(
    assignment => submittedAssignmentsMap.has(assignment.id)
  ) || [];

  const upcomingDeadlines = pendingAssignments
    .filter(assignment => new Date(assignment.dueDate) > new Date())
    .sort((a, b) => new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime())
    .slice(0, 5);


  return (
    <DashboardContainer>
      <DashboardHeader>
        <Title>Інформаційна панель</Title>
        <Stats>
          <StatItem>
            <StatNumber>{courses?.length || 0}</StatNumber>
            <StatLabel>Курси</StatLabel>
          </StatItem>
          <StatItem>
            <StatNumber>{pendingAssignments.length}</StatNumber>
            <StatLabel>В очікуванні</StatLabel>
          </StatItem>
          <StatItem>
            <StatNumber>{submittedAssignments.length}</StatNumber>
            <StatLabel>Завершено</StatLabel>
          </StatItem>
        </Stats>
      </DashboardHeader>

      <DashboardContent>
        <LeftPanel>
          <Widget>
            <WidgetTitle>Календар</WidgetTitle>
            <Calendar
              assignments={assignments || []}
              selectedDate={selectedDate}
              onDateSelect={setSelectedDate}
            />
          </Widget>
        </LeftPanel>

        <MainPanel>
          <Widget>
            <WidgetTitle>Всі завдання</WidgetTitle>
            <AssignmentsTable
              assignments={assignments || []}
              documents={userDocuments}
              showGrade={true}
            />
          </Widget>
          <Widget>
            <WidgetTitle>Найближчі дедлайни</WidgetTitle>
            <AssignmentsList
              assignments={upcomingDeadlines}
            />
          </Widget>
        </MainPanel>
      </DashboardContent>
    </DashboardContainer>
  );
}