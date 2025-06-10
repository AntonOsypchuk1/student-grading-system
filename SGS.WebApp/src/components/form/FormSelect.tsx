import styled from 'styled-components';
import {FieldErrors, RegisterOptions, UseFormRegister} from 'react-hook-form';

interface Option {
  value: string;
  label: string;
}

interface FormSelectProps {
  label: string;
  name: string;
  options: Option[];
  register: UseFormRegister<any>;
  rules?: RegisterOptions;
  errors: FieldErrors;
}

const SelectWrapper = styled.div`
    position: relative;
`;

const Label = styled.label`
    display: block;
    font-size: 0.875rem;
    font-weight: 500;
    color: ${props => props.theme.colors.gray[700]};
    margin-bottom: 0.25rem;
`;

const SelectContainer = styled.div`
    position: relative;
`;

const StyledSelect = styled.select<{ hasError: boolean }>`
    width: 100%;
    padding: 0.625rem 1rem;
    padding-right: 2.5rem;
    border-radius: 0.5rem;
    appearance: none;
    background-color: ${props => props.theme.colors.white};
    border: 1px solid ${props => props.hasError ?
            props.theme.colors.danger :
            props.theme.colors.gray[300]};
    transition: all ${props => props.theme.transitions.default};
    box-shadow: ${props => props.theme.shadows.sm};

    &:focus {
        outline: none;
        box-shadow: 0 0 0 2px ${props => props.hasError ?
                `${props.theme.colors.danger}33` :
                `${props.theme.colors.primary}33`};
    }
`;

const SelectIcon = styled.div`
    position: absolute;
    right: 0.5rem;
    top: 50%;
    transform: translateY(-50%);
    pointer-events: none;
    color: ${props => props.theme.colors.gray[700]};
`;

const ErrorText = styled.p`
    margin-top: 0.25rem;
    color: ${props => props.theme.colors.danger};
    font-size: 0.875rem;
`;

export function FormSelect({
                             label,
                             name,
                             options,
                             register,
                             rules,
                             errors,
                           }: FormSelectProps) {
  return (
    <SelectWrapper>
      <Label>{label}</Label>
      <SelectContainer>
        <StyledSelect
          {...register(name, rules)}
          hasError={Boolean(errors[name])}
        >
          <option value="">Select an option</option>
          {options.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </StyledSelect>
        <SelectIcon>
          <svg width="16" height="16" viewBox="0 0 20 20" fill="currentColor">
            <path d="M9.293 12.95l.707.707L15.657 8l-1.414-1.414L10 10.828 5.757 6.586 4.343 8z"/>
          </svg>
        </SelectIcon>
      </SelectContainer>
      {errors[name] && (
        <ErrorText>{errors[name]?.message as string}</ErrorText>
      )}
    </SelectWrapper>
  );
}