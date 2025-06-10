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
    formState: {errors, isSubmitting},
  } = useForm<RegisterForm>();


  const onSubmit = async (data: RegisterForm) => {
    try {
      setError(error);
      const {confirmPassword, ...payload} = data;
      if (payload.password !== confirmPassword) {
        setError("Паролі не співпадають");
        return;
      }
      await registerUser(payload);
      window.location.replace("/login");
    } catch {
      setError("Реєстрація не вдалася. Будь ласка, спробуйте ще раз.");
    }
  };

  return (
    <AuthContainer>
      <AuthCard>
        <AuthHeader>
          <AuthTitle>Створити обліковий запис</AuthTitle>
        </AuthHeader>

        <Form onSubmit={handleSubmit(onSubmit)}>
          <FormInput
            label="Повне імʼя"
            name="fullName"
            register={register}
            rules={{required: 'Необхідно вказати повне ім\'я'}}
            errors={errors}
            placeholder="John Doe"
          />

          <FormInput
            label="Електронна пошта"
            name="email"
            type="email"
            register={register}
            rules={{
              required: 'Обов\'язкова наявність електронної пошти',
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
              required: 'Необхідно ввести пароль',
              minLength: {
                value: 6,
                message: 'Пароль повинен містити щонайменше 6 символів'
              }
            }}
            errors={errors}
            placeholder="••••••••"
          />

          <FormSelect
            label="Роль"
            name="role"
            options={roleOptions}
            register={register}
            rules={{required: 'Роль обов\'язкова'}}
            errors={errors}
          />

          <FormSubmitButton
            isSubmitting={isSubmitting}
            text="Створити акаунт"
            loadingText="Створюємо акаунт..."
          />
        </Form>

        <AuthFooter>
          Вже маєте обліковий запис?{' '}
          <AuthLink to="/login">Увійдіть в систему</AuthLink>
        </AuthFooter>
      </AuthCard>
    </AuthContainer>
  );
}
