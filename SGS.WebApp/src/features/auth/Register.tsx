import {useForm} from 'react-hook-form';
import {FormInput} from '../../components/form/FormInput';
import {FormSelect} from '../../components/form/FormSelect';
import {FormSubmitButton} from '../../components/form/FormSubmitButton';
import {AuthCard, AuthContainer, AuthFooter, AuthHeader, AuthLink, AuthTitle, Form} from './styles';
import {useAuthContext} from "../../auth/AuthProvider.tsx";
import {useState} from 'react';
import {RegisterForm} from "./types.ts";

const roleOptions = [
  {value: 'Student', label: 'Student'},
  {value: 'Teacher', label: 'Teacher'}
];

export function RegisterPage() {
  const {register: registerUser} = useAuthContext();
  const [error, setError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    // watch,
    formState: {errors, isSubmitting},
  } = useForm<RegisterForm>();

  // const password = watch("password", "");

  const onSubmit = async (data: RegisterForm) => {
    try {
      setError(error);
      const {confirmPassword, ...payload} = data;
      if (payload.password !== confirmPassword) {
        setError("Passwords do not match");
        return;
      }
      await registerUser(payload);
      window.location.replace("/login");
    } catch {
      setError("Registration failed. Please try again.");
    }
  };

  return (
    <AuthContainer>
      <AuthCard>
        <AuthHeader>
          <AuthTitle>Create an account</AuthTitle>
        </AuthHeader>

        <Form onSubmit={handleSubmit(onSubmit)}>
          <FormInput
            label="Full Name"
            name="fullName"
            register={register}
            rules={{required: 'Full name is required'}}
            errors={errors}
            placeholder="John Doe"
          />

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
                value: 6,
                message: 'Password must be at least 6 characters'
              }
            }}
            errors={errors}
            placeholder="••••••••"
          />

          <FormSelect
            label="Role"
            name="role"
            options={roleOptions}
            register={register}
            rules={{required: 'Role is required'}}
            errors={errors}
          />

          <FormSubmitButton
            isSubmitting={isSubmitting}
            text="Create account"
            loadingText="Creating account..."
          />
        </Form>

        <AuthFooter>
          Already have an account?{' '}
          <AuthLink to="/login">Sign in</AuthLink>
        </AuthFooter>
      </AuthCard>
    </AuthContainer>
  );
}
