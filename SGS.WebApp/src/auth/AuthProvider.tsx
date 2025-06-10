import {createContext, ReactNode, useContext, useEffect} from "react";
import {LoginDto, RegisterUserCommand, StudentDto, TeacherDto, UserDto} from "../types/AuthTypes.ts";
import {useAuth} from "../hooks/useAuth.ts";
import {UseQueryResult} from "@tanstack/react-query";

type AuthContextType = {
  user: UseQueryResult<UserDto | StudentDto | TeacherDto, Error>;
  isLoading: boolean;
  login: (data: LoginDto) => Promise<void>;
  register: (data: RegisterUserCommand) => Promise<void>;
  logout: () => void;
};

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({children}: { children: ReactNode }) => {
  const {
    loginMutation,
    registerMutation,
    refreshTokenMutation,
    logoutMutation,
    currentUserQuery,
  } = useAuth();

  const isLoading = currentUserQuery.isLoading;

  useEffect(() => {
    const token = localStorage.getItem("refreshToken");
    if (token) {
      refreshTokenMutation.mutate({refreshToken: token});
    }
  }, []);

  const login = async (creds: LoginDto) => {
    await loginMutation.mutateAsync(creds);
  };

  const register = async (data: RegisterUserCommand) => {
    await registerMutation.mutateAsync(data);
  };

  const logout = async () => {
    const token = localStorage.getItem("refreshToken");
    if (token) {
      await logoutMutation.mutateAsync({refreshToken: token});
    }

    window.location.reload();
  };

  return (
    <AuthContext.Provider
      value={{
        user: currentUserQuery,
        isLoading,
        login,
        register,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuthContext = () => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuthContext must be inside AuthProvider");
  return ctx;
};
