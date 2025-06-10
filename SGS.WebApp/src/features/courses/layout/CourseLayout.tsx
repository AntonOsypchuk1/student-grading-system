import {Outlet, useParams} from 'react-router-dom';
import {useNavigation} from '../../../context/NavigationContext.tsx';
import {useEffect} from 'react';

export default function CourseLayout() {
  const {courseId} = useParams<{ courseId: string }>();
  const {setLinks} = useNavigation();

  useEffect(() => {
    setLinks([
      {title: 'Overview', path: `/courses/${courseId}`},
      {title: 'Assignments', path: `/courses/${courseId}/assignments`},
      // {title: 'Students', path: `/courses/${courseId}/students`},
      {title: 'Grades', path: `/courses/${courseId}/grades`}
    ]);
  }, [courseId, setLinks]);

  return <Outlet/>;
}