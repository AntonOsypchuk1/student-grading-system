import {Outlet, useParams} from 'react-router-dom';
import {useNavigation} from '../../../context/NavigationContext.tsx';
import {useEffect} from 'react';

export default function CourseLayout() {
  const {courseId} = useParams<{ courseId: string }>();
  const {setLinks} = useNavigation();

  useEffect(() => {
    setLinks([
      {title: 'Курс', path: `/courses/${courseId}`},
      {title: 'Завдання', path: `/courses/${courseId}/assignments`},
      {title: 'Оцінки', path: `/courses/${courseId}/grades`}
    ]);
  }, [courseId, setLinks]);

  return <Outlet/>;
}