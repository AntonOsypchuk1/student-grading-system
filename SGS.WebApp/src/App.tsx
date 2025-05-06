import {BrowserRouter, Navigate, Route, Routes} from "react-router-dom";
import {QueryClient, QueryClientProvider} from "@tanstack/react-query";
import {AuthProvider} from "./auth/AuthProvider.tsx";
import {ProtectedRoute} from "./auth/ProtectedRoute";

// public pages
import Login from "./features/auth/Login";
import Register from "./features/auth/Register";

// protected pages
import StudentDashboard from "./features/dashboard/StudentDashboard";
import TeacherDashboard from "./features/dashboard/TeacherDashboard";
import CourseList from "./features/courses/CourseList";
import CourseDetail from "./features/courses/CourseDetail";
import AssignmentUpload from "./features/assignments/AssignmentUpload";
import AssignmentManage from "./features/assignments/AssignmentManage";
import CriteriaList from "./features/criteria/CriteriaList";
import CriteriaForm from "./features/criteria/CriteriaForm";

const queryClient = new QueryClient();

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <BrowserRouter>
          <Routes>
            <Route path="/login" element={<Login/>}/>
            <Route path="/register" element={<Register/>}/>

            <Route element={<ProtectedRoute/>}>
              {/*TODO: Add check and provide the dashboard considering the user's role*/}
              <Route path="/" element={<StudentDashboard/>}/>
              <Route path="dashboard/teacher" element={<TeacherDashboard/>}/>

              <Route path="courses">
                <Route index element={<CourseList/>}/>
                <Route path=":id" element={<CourseDetail/>}/>
              </Route>

              <Route path="assignments">
                <Route path="upload" element={<AssignmentUpload/>}/>
                <Route path="manage" element={<AssignmentManage/>}/>
              </Route>

              <Route path="criteria">
                <Route index element={<CriteriaList/>}/>
                <Route path="new" element={<CriteriaForm/>}/>
                <Route path=":id" element={<CriteriaForm/>}/>
              </Route>
            </Route>

            <Route path="*" element={<Navigate to="/" replace/>}/>
          </Routes>
        </BrowserRouter>
      </AuthProvider>
    </QueryClientProvider>
  );
}

export default App;
