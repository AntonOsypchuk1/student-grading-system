import {Navigate, Outlet, useLocation} from "react-router-dom";
import {useAuthContext} from "./AuthProvider";

export const ProtectedRoute = () => {
  const {user, isLoading} = useAuthContext();
  const location = useLocation();

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  if (user.status === 'error' || !user.data) {
    return <Navigate to="/login" state={{from: location}} replace/>;
  }

  return <Outlet/>;
};