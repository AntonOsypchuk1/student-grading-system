import {createContext, ReactNode, useContext, useEffect} from "react";
import {LoginDto, RegisterUserCommand, UserDto} from "../types/AuthenticationTypes.ts";
import {useAuth} from "../hooks/useAuth.ts";
import {UseQueryResult} from "@tanstack/react-query";

type AuthContextType = {
  user: UseQueryResult<UserDto, Error>;
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
    currentUserQuery,
  } = useAuth();

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

  const logout = () => {
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");

    window.location.reload();
  };

  return (
    <AuthContext.Provider
      value={{
        user: currentUserQuery,
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
