import {useForm} from 'react-hook-form';
import {useAuthContext} from "../../auth/AuthProvider.tsx";
import {FormInput} from '../../components/form/FormInput';
import {FormSubmitButton} from '../../components/form/FormSubmitButton';
import {AuthCard, AuthContainer, AuthDescription, AuthFooter, AuthHeader, AuthLink, AuthTitle, Form} from './styles';
import {useState} from "react";
import {LoginForm} from "./types.ts";

export function LoginPage() {
  const {login} = useAuthContext();
  const [error, setError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: {errors, isSubmitting},
  } = useForm<LoginForm>();

  const onSubmit = async (data: LoginForm) => {
    try {
      setError(error);
      await login(data);
      window.location.replace("/");
    } catch (err) {
      setError("Invalid email or password: " + (err as Error).message);
    }
  };

  return (
    <AuthContainer>
      <AuthCard>
        <AuthHeader>
          <AuthTitle>Welcome back</AuthTitle>
          <AuthDescription>Sign in to your account</AuthDescription>
        </AuthHeader>

        <Form onSubmit={handleSubmit(onSubmit)}>
          <FormInput
            label="Email"
            name="email"
            type="email"
            register={register}
            rules={{
              required: 'Email is required',
              pattern: {
                value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                message: 'Invalid email address'
              }
            }}
            errors={errors}
            placeholder="your@email.com"
          />

          <FormInput
            label="Password"
            name="password"
            type="password"
            register={register}
            rules={{
              required: 'Password is required',
              minLength: {
                value: 3,
                message: 'Password must be at least 6 characters'
              }
            }}
            errors={errors}
            placeholder="••••••••"
          />

          <FormSubmitButton
            isSubmitting={isSubmitting}
            text="Sign in"
            loadingText="Signing in..."
          />
        </Form>

        <AuthFooter>
          Don't have an account?{' '}
          <AuthLink to="/register">Create an account</AuthLink>
        </AuthFooter>
      </AuthCard>
    </AuthContainer>
  );
}