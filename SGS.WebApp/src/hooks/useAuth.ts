import {LoginDto, RegisterUserCommand} from "../types/AuthenticationTypes.ts";
import {useMutation, useQuery, useQueryClient} from "@tanstack/react-query";
import * as authApi from "../services/apiEndpoints/AuthApi.ts";
import * as userApi from "../services/apiEndpoints/UserApi.ts";
import {RefreshTokenRequestDto} from "../types/TokenTypes.ts";

export const useAuth = () => {
  const queryClient = useQueryClient();

  const loginMutation = useMutation({
    mutationFn: (data: LoginDto) => authApi.login(data),
    onSuccess: (data) => {
      localStorage.setItem("accessToken", data.token);
      localStorage.setItem("refreshToken", data.refreshToken);

      queryClient.setQueryData(["currentUser"], data.user);
    },
});

  const registerMutation = useMutation({
    mutationFn: (data: RegisterUserCommand) => authApi.registerUser(data)
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

  return { loginMutation, registerMutation, refreshTokenMutation, currentUserQuery };
};
