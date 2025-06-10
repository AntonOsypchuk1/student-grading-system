import {LoginDto, RegisterUserCommand} from "../types/AuthTypes.ts";
import {useMutation, useQuery, useQueryClient} from "@tanstack/react-query";
import * as authApi from "../api/apiEndpoints/AuthApi.ts";
import * as userApi from "../api/apiEndpoints/UserApi.ts";
import {RefreshTokenRequestDto} from "../types/TokenTypes.ts";

export const useAuth = () => {
  const queryClient = useQueryClient();

  const loginMutation = useMutation({
    mutationFn: async (data: LoginDto) => {
      try {
        return await authApi.login(data);
      } catch (error) {
        console.error('Login Mutation Error:', error);
        throw error;
      }
    },
    onSuccess: (data) => {
      localStorage.setItem("accessToken", data.token);
      localStorage.setItem("refreshToken", data.refreshToken);
      queryClient.setQueryData(["currentUser"], data.user);
    },
    onError: (error) => {
      console.error('Login Mutation Error Handler:', error);
    }
  });

  const registerMutation = useMutation({
    mutationFn: (data: RegisterUserCommand) => authApi.registerUser(data)
  });

  const logoutMutation = useMutation({
    mutationFn: (data: RefreshTokenRequestDto) => authApi.logout(data),
    onSuccess: () => {
      localStorage.removeItem("accessToken");
      localStorage.removeItem("refreshToken");

      queryClient.invalidateQueries({queryKey: ["currentUser"]});
    }
  });

  const refreshTokenMutation = useMutation({
    mutationFn: (data: RefreshTokenRequestDto) => authApi.refreshToken(data),
    onSuccess: (data) => {
      localStorage.setItem("accessToken", data.token);
      localStorage.setItem("refreshToken", data.refreshToken);

      queryClient.setQueryData(["currentUser"], data.user);
    },
  });

  const currentUserQuery = useQuery({
    queryKey: ["currentUser"],
    queryFn: () => userApi.getCurrentUser(),
    enabled: !!localStorage.getItem("accessToken"),
  });

  const user = currentUserQuery.data;

  return {
    user,
    loginMutation,
    logoutMutation,
    registerMutation,
    refreshTokenMutation,
    currentUserQuery
  };
};
