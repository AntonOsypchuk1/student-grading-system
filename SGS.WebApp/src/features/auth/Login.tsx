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
      setError("Неправильна адреса електронної пошти або пароль: " + (err as Error).message);
    }
  };

  return (
    <AuthContainer>
      <AuthCard>
        <AuthHeader>
          <AuthTitle>З поверненням!</AuthTitle>
          <AuthDescription>Увійдіть до свого облікового запису</AuthDescription>
        </AuthHeader>

        <Form onSubmit={handleSubmit(onSubmit)}>
          <FormInput
            label="Електронна пошта"
            name="email"
            type="email"
            register={register}
            rules={{
              required: 'Електронна пошта обов\'язкова',
              pattern: {
                value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                message: 'Неправильна адреса електронної пошти'
              }
            }}
            errors={errors}
            placeholder="your@email.com"
          />

          <FormInput
            label="Пароль"
            name="password"
            type="password"
            register={register}
            rules={{
              required: 'Пароль обов\'язковий',
              minLength: {
                value: 3,
                message: 'Пароль повинен містити щонайменше 6 символів'
              }
            }}
            errors={errors}
            placeholder="••••••••"
          />

          <FormSubmitButton
            isSubmitting={isSubmitting}
            text="Увійдіть в систему"
            loadingText="Виконуємо вхід..."
          />
        </Form>

        <AuthFooter>
          Не маю облікового запису{' '}
          <AuthLink to="/register">Створити акаунт</AuthLink>
        </AuthFooter>
      </AuthCard>
    </AuthContainer>
  );
}