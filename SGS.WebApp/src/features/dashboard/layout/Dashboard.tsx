import {useAuth} from "../../../hooks/useAuth.ts";
import TeacherDashboard from "./TeacherDashboard.tsx";
import StudentDashboard from "./StudentDashboard.tsx";

const Dashboard = () => {
  const {user} = useAuth();

  return (
    <>
      {user?.role === 'Teacher'
        ? (<TeacherDashboard/>)
        : (<StudentDashboard/>)}
    </>
  );
};

export default Dashboard;