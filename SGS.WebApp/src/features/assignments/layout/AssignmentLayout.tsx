import {Outlet, useParams} from 'react-router-dom';
import {useNavigation} from '../../../context/NavigationContext.tsx';
import {useEffect} from 'react';
import {useAuthContext} from "../../../auth/AuthProvider.tsx";


export default function AssignmentLayout() {
  const {assignmentId: assignmentId} = useParams<{ assignmentId: string }>();
  const {setLinks} = useNavigation();

  const {user} = useAuthContext();
  const userRole = user.data?.role.toLowerCase();

  useEffect(() => {
    const links = [
      {title: 'Overview', path: `/assignments/${assignmentId}`},
    ];
    if (userRole === 'teacher')
      links.push(
        {title: 'Students', path: `/assignments/${assignmentId}/students`},
        {title: 'Grades', path: `/assignments/${assignmentId}/grades`}
      );
    else if (userRole === 'student')
      links.push(
        {title: 'Submission', path: `/assignments/${assignmentId}/submission`}
      );

    setLinks(links);
  }, [assignmentId, setLinks, userRole]);

  return <Outlet/>;
};
