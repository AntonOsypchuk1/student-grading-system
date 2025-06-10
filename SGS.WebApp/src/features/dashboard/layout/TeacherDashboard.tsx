import {
  CourseStats,
  DashboardContainer,
  DashboardContent,
  DashboardHeader,
  GroupItem,
  GroupsList,
  LeftPanel,
  MainPanel,
  StatItem,
  StatLabel,
  StatNumber,
  Stats,
  Title,
  Widget,
  WidgetTitle
} from "../styles";
import {useAuth} from "../../../hooks/useAuth";
import {useCoursesByTeacher} from "../../../hooks/useCourses";
import {AssignmentDto} from "../../../types/AssignmentTypes";
import {AssignmentsList} from "../../assignments/sub/AssignmentsList";
import {Link} from "react-router-dom";
import {LoadingSpinner} from "../../../components/shared/StyledComponents";
import {useAssignmentsByGroups} from "../../../hooks/useAssignments.ts";
import {useMemo} from "react";
import {CourseDto} from "../../../types/CourseTypes.ts";
import {StudentsGroupDto} from "../../../types/GroupTypes.ts";
import {ErrorMessage} from "../../courses/styles.ts";

export default function TeacherDashboard() {
  const {user} = useAuth();
  const {data: rawCourses, isLoading: coursesLoading, error: coursesError} = useCoursesByTeacher(user?.id);

  const courses: CourseDto[] = rawCourses ?? [];

  const allGroups = useMemo(() => {
    const flat = courses.flatMap(c => c.studentsGroups ?? []);
    const map = new Map<string, StudentsGroupDto>();
    for (const g of flat) map.set(g.id, g);
    return Array.from(map.values());
  }, [courses]);

  console.log(courses)
  console.log(allGroups)

  const assignmentQueries = useAssignmentsByGroups(allGroups, courses ?? []);
  const allAssignments = useMemo(() =>
      assignmentQueries
        .filter(query => query.data && !query.isError)
        .flatMap(query => query.data)
        .filter(Boolean),
    [assignmentQueries]
  );

  const isLoading = coursesLoading ||
    assignmentQueries.some(query => query.isLoading);

  const hasErrors = coursesError ||
    assignmentQueries.some(query => query.isError);

  const stats = useMemo(() => ({
    totalCourses: courses?.length ?? 0,
    totalGroups: allGroups.length,
    totalAssignments: allAssignments.length,
    activeAssignments: allAssignments.filter(assignment =>
      assignment?.dueDate && new Date(assignment.dueDate) > new Date()
    ).length
  }), [courses, allGroups, allAssignments]);

  const assignmentsByCourse = useMemo(() =>
      courses?.reduce<Record<string, AssignmentDto[]>>((acc, course) => {
        acc[course.id] = allAssignments.filter(assignment =>
          assignment?.courseId === course.id
        ) as AssignmentDto[];
        return acc;
      }, {}) ?? {},
    [courses, allAssignments]
  );

  if (isLoading) {
    return (
      <DashboardContainer>
        <LoadingSpinner/>
      </DashboardContainer>
    );
  }

  if (hasErrors) {
    return (
      <DashboardContainer>
        <ErrorMessage>
          Виникла помилка під час завантаження інформаційної панелі. Будь ласка, спробуйте пізніше.
        </ErrorMessage>
      </DashboardContainer>
    );
  }

  return (
    <DashboardContainer>
      <DashboardHeader>
        <Title>Інформаційна панель вчителя</Title>
        <Stats>
          <StatItem>
            <StatNumber>{stats.totalCourses}</StatNumber>
            <StatLabel>Курси</StatLabel>
          </StatItem>
          <StatItem>
            <StatNumber>{stats.totalGroups}</StatNumber>
            <StatLabel>Групи</StatLabel>
          </StatItem>
          <StatItem>
            <StatNumber>{stats.activeAssignments}</StatNumber>
            <StatLabel>Активні завдання</StatLabel>
          </StatItem>
        </Stats>
      </DashboardHeader>

      <DashboardContent>
        <LeftPanel>
          {courses?.map(course => (
            <Widget key={course.id}>
              <WidgetTitle>
                {course.title}
                <small><Link to={`/courses/${course.id}`}>Перейти</Link></small>
              </WidgetTitle>
              <CourseStats>
                <div>
                  Групи: {course.studentsGroups?.length ?? 0}
                </div>
                <div>
                  Завдання: {assignmentsByCourse[course.id]?.length ?? 0}
                </div>
              </CourseStats>
              <GroupsList>
                {course.studentsGroups?.map(group => (
                  <GroupItem key={group?.id}>
                    {group?.name}
                  </GroupItem>
                ))}
              </GroupsList>

              <GroupsList>
                {allGroups
                  .filter(group => group?.courses?.some(c => c?.id === course.id))
                  .map(group => (
                    <GroupItem key={group?.id}>
                      {group?.name}
                    </GroupItem>
                  ))}
              </GroupsList>
            </Widget>
          ))}
        </LeftPanel>

        <MainPanel>
          <WidgetTitle>Нещодавні завдання</WidgetTitle>
          <AssignmentsList
            assignments={allAssignments
              .filter(assignment =>
                assignment?.dueDate && !isNaN(new Date(assignment.dueDate).getTime())
              )
              .sort((a, b) => {
                const dateB = new Date(b!.dueDate).getTime();
                const dateA = new Date(a!.dueDate).getTime();
                return dateB - dateA;
              })
              .slice(0, 5) as AssignmentDto[]}
          />

        </MainPanel>
      </DashboardContent>
    </DashboardContainer>
  );
}