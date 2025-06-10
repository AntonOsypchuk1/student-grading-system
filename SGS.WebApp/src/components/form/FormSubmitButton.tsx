import styled, {keyframes} from 'styled-components';

interface FormSubmitButtonProps {
  isSubmitting: boolean;
  text: string;
  loadingText: string;
  variant?: "primary" | "success";
}

const spin = keyframes`
    from {
        transform: rotate(0deg);
    }
    to {
        transform: rotate(360deg);
    }
`;

const StyledButton = styled.button<{ variant: "primary" | "success" }>`
    width: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 0.75rem 1rem;
    color: ${props => props.theme.colors.white};
    font-weight: 500;
    border-radius: 0.5rem;
    transition: all ${props => props.theme.transitions.default};
    position: relative;
    box-shadow: ${props => props.theme.shadows.md};

    background: linear-gradient(to right,
    ${props => props.variant === "primary"
            ? `${props.theme.colors.primary}, ${props.theme.colors.primaryHover}`
            : `${props.theme.colors.success}, ${props.theme.colors.successHover}`});

    &:hover {
        transform: scale(1.02);
        box-shadow: ${props => props.theme.shadows.lg};
    }

    &:disabled {
        opacity: 0.5;
        transform: none;
        cursor: not-allowed;
    }
`;

const LoadingSpinner = styled.svg`
    animation: ${spin} 1s linear infinite;
    margin-right: 0.75rem;
    margin-left: -0.25rem;
    height: 1.25rem;
    width: 1.25rem;
`;

export function FormSubmitButton({
                                   isSubmitting,
                                   text,
                                   loadingText,
                                   variant = "primary",
                                 }: FormSubmitButtonProps) {
  return (
    <StyledButton type="submit" disabled={isSubmitting} variant={variant}>
      {isSubmitting && (
        <LoadingSpinner
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
        >
          <circle
            opacity="0.25"
            cx="12"
            cy="12"
            r="10"
            stroke="currentColor"
            strokeWidth="4"
          />
          <path
            opacity="0.75"
            fill="currentColor"
            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
          />
        </LoadingSpinner>
      )}
      {isSubmitting ? loadingText : text}
    </StyledButton>
  );
}