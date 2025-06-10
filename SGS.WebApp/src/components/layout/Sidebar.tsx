import styled from 'styled-components';
import {useCoursesByStudent, useCoursesByTeacher} from "../../hooks/useCourses";
import {useAuthContext} from "../../auth/AuthProvider.tsx";

const SidebarContainer = styled.div`
    width: 16rem;
    height: 88vh;
    padding: 1rem;
    border: 1px solid ${props => props.theme.colors.gray[300]};
    border-radius: 1rem;
`;

const Nav = styled.nav`
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
`;

const NavLink = styled.a`
    display: block;
    border-radius: 0.375rem;
    transition: background-color ${props => props.theme.transitions.default};

    &:hover {
        text-decoration: underline;
    }
`;

const SubLink = styled(NavLink)`
    font-size: 0.875rem;
    padding: 0.75rem 1rem;
    color: ${props => props.theme.colors.gray[700]};
`;

const SubSectionTitle = styled.div`
    color: ${props => props.theme.colors.gray[700]};
`;

export const Sidebar = () => {
  const {user: {data: user}} = useAuthContext();
  const {data: teacherCourses} = useCoursesByTeacher(
    user?.role === 'Teacher' ? user.id : undefined
  );
  const {data: studentCourses} = useCoursesByStudent(
    user?.role === 'Student' ? user.id : undefined
  );

  if (!user) return null;

  return (
    <SidebarContainer>
      <Nav>
        <NavLink href="/dashboard">Інформаційна панель</NavLink>

        {user?.role === 'Teacher' ? (
          <div>
            <SubSectionTitle>Мої курси</SubSectionTitle>
            {teacherCourses?.map(course => (
              <SubLink key={course.id} href={`/courses/${course.id}`}>
                {course.title}
              </SubLink>
            ))}
          </div>
        ) : (
          <div>
            <SubSectionTitle>Мої курси</SubSectionTitle>
            {studentCourses?.map(course => (
              <SubLink key={course.id} href={`/courses/${course.id}`}>
                {course.title}
              </SubLink>
            ))}
          </div>
        )}
      </Nav>
    </SidebarContainer>
  );
};