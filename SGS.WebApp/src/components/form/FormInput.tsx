import styled, {css} from 'styled-components';
import {FieldErrors, RegisterOptions, UseFormRegister} from 'react-hook-form';

const InputWrapper = styled.div`
    position: relative;
`;

const Label = styled.label`
    display: block;
    font-size: 0.875rem;
    font-weight: 500;
    color: ${props => props.theme.colors.gray[700]};
    margin-bottom: 0.25rem;
`;

const baseInputStyles = css<{ hasError: boolean }>`
    border-radius: 0.5rem;
    background-color: ${props => props.theme.colors.white};
    border: 1px solid ${props => props.hasError ? props.theme.colors.danger : props.theme.colors.gray[300]};
    transition: all ${props => props.theme.transitions.default};
    box-shadow: ${props => props.theme.shadows.sm};

    &:focus {
        outline: none;
        box-shadow: 0 0 0 2px ${props => props.hasError ?
                `${props.theme.colors.danger}33` :
                `${props.theme.colors.primary}33`};
    }

    &::placeholder {
        color: ${props => props.theme.colors.gray[400]};
    }
`;

const StyledInput = styled.input<{ hasError: boolean }>`
    ${baseInputStyles};
    width: 100%;
    padding: 0.625rem 1rem;
`;

const StyledNumberInput = styled.input<{ hasError: boolean }>`
    ${baseInputStyles};
    max-width: 80px;
    padding: 0.625rem;
    text-align: center;
`;

const StyledTextArea = styled.textarea<{ hasError: boolean }>`
    ${baseInputStyles};
    width: 100%;
    padding: 0.625rem 1rem;
    min-height: 100px;
    resize: vertical;
    font-family: inherit;
    line-height: 1.5;
`;

const ErrorText = styled.p`
    position: absolute;
    bottom: -1.5rem;
    color: ${props => props.theme.colors.danger};
    font-size: 0.875rem;
`;

interface FormInputProps {
  label?: string;
  name: string;
  register: UseFormRegister<any>;
  rules?: RegisterOptions;
  errors: FieldErrors;
  type?: string | 'number' | 'text' | 'email' | 'password';
  placeholder?: string;
  step?: string | number;
  min?: string | number;
  max?: string | number;
  rows?: number;
  style?: React.CSSProperties;
}

export function FormInput({
                            label,
                            name,
                            register,
                            rules,
                            errors,
                            type = 'text',
                            placeholder,
                            step,
                            min,
                            max,
                            rows,
                            style
                          }: FormInputProps) {
  const renderInput = () => {
    switch (type) {
      case 'number':
        return (
          <StyledNumberInput
            {...register(name, rules)}
            id={name}
            type={type}
            placeholder={placeholder}
            hasError={Boolean(errors[name])}
            step={step}
            min={min}
            max={max}
            style={style}
          />
        );
      case 'textarea':
        return (
          <StyledTextArea
            {...register(name, rules)}
            id={name}
            placeholder={placeholder}
            hasError={Boolean(errors[name])}
            rows={rows}
            style={style}
          />
        );
      default:
        return (
          <StyledInput
            {...register(name, rules)}
            id={name}
            type={type}
            placeholder={placeholder}
            hasError={Boolean(errors[name])}
            style={style}
          />
        );
    }
  };

  return (
    <InputWrapper>
      <Label htmlFor={name}>{label}</Label>
      {renderInput()}
      {errors[name] && (
        <ErrorText>{errors[name]?.message as string}</ErrorText>
      )}
    </InputWrapper>
  );
}