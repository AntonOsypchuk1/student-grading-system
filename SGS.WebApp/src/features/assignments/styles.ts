import styled from "styled-components";

export const Form = styled.form`
    display: flex;
    flex-direction: column;
    gap: 1.5rem;
    max-width: 600px;
`;

export const Field = styled.div`
    display: flex;
    flex-direction: column;
    width: 100%;
    gap: 0.5rem;
`;

export const FieldGroup = styled.div`
    display: flex;
    flex-direction: row;
    justify-content: space-between;
    flex-grow: 1;
    gap: 0.5rem;
`;

export const CriteriaGroup = styled.div`
    display: flex;
    justify-content: space-between;
`;

export const Error = styled.span`
    color: ${props => props.theme.colors.danger};
    font-size: 0.875rem;
`;

export const Button = styled.button`
    padding: 0.625rem 1rem;
    margin-top: 0.3rem;
    background-color: ${props => props.theme.colors.primary};
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;

    &:disabled {
        opacity: 0.5;
        cursor: not-allowed;
    }
`;