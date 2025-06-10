import {BrowserRouter, Navigate, Route, Routes} from "react-router-dom";
import {QueryClient, QueryClientProvider} from "@tanstack/react-query";
import {AuthProvider} from "./auth/AuthProvider.tsx";
import {ProtectedRoute} from "./auth/ProtectedRoute";
import {ThemeProvider} from "styled-components";
import {theme} from "./styles/theme.ts";
import {GlobalStyles} from "./styles/GlobalStyles.ts";

// public pages
import {LoginPage} from "./features/auth/Login";
import {RegisterPage} from "./features/auth/Register";

// protected pages
import CourseDetail from "./features/courses/layout/CourseDetail.tsx";
import AssignmentDetails from "./features/assignments/layout/AssignmentDetails.tsx";
import MainLayout from "./components/layout/MainLayout.tsx";
import Dashboard from "./features/dashboard/layout/Dashboard.tsx";
import CourseLayout from "./features/courses/layout/CourseLayout.tsx";
import CourseGrades from "./features/courses/layout/CourseGrades.tsx";
import CourseAssignments from "./features/courses/layout/CourseAssignments.tsx";
import AssignmentForm from "./features/assignments/sub/AssignmentForm.tsx";
import AssignmentLayout from "./features/assignments/layout/AssignmentLayout.tsx";
import AssignmentStudents from "./features/assignments/layout/AssignmentStudents.tsx";
import AssignmentGrades from "./features/assignments/layout/AssignmentGrades.tsx";
import AssignmentAnalysis from "./features/assignments/layout/AssignmentAnalysis.tsx";

const queryClient = new QueryClient();

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <ThemeProvider theme={theme}>
          <GlobalStyles/>
          <BrowserRouter>
            <Routes>
              <Route path="/login" element={<LoginPage/>}/>
              <Route path="/register" element={<RegisterPage/>}/>

              <Route element={<ProtectedRoute/>}>
                <Route element={<MainLayout/>}>
                  <Route path="/" element={<Dashboard/>}/>

                  <Route path="courses" element={<CourseLayout/>}>
                    <Route path=":courseId" element={<CourseDetail/>}/>
                    <Route path=":courseId/grades" element={<CourseGrades/>}/>
                    <Route path=":courseId/assignments" element={<CourseAssignments/>}/>
                    <Route path=":courseId/assignments/:assignmentId" element={<CourseAssignments/>}/>
                    <Route path=":courseId/assignments/new" element={<AssignmentForm/>}/>
                  </Route>

                  <Route path="assignments" element={<AssignmentLayout/>}>
                    {/*<Route path="upload" element={<AssignmentUpload/>}/>*/}
                    <Route path=":assignmentId" element={<AssignmentDetails/>}/>
                    <Route path=":assignmentId/submission" element={<AssignmentAnalysis/>}/>
                    <Route path=":assignmentId/students" element={<AssignmentStudents/>}/>
                    <Route path=":assignmentId/grades" element={<AssignmentGrades/>}/>
                  </Route>
                </Route>
              </Route>

              <Route path="*" element={<Navigate to="/" replace/>}/>
            </Routes>
          </BrowserRouter>
        </ThemeProvider>
      </AuthProvider>
    </QueryClientProvider>
  );
}

export default App;
