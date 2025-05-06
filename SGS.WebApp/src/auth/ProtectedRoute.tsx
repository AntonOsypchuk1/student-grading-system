import {Navigate, Outlet} from "react-router-dom";
import {useAuthContext} from "./AuthProvider.tsx";

export const ProtectedRoute = ({redirectTo = "/login"}) => {
  const {user} = useAuthContext();

  if (user === undefined) {
    return <div>Loading…</div>;
  }

  return user ? <Outlet/> : <Navigate to={redirectTo}/>;
};
