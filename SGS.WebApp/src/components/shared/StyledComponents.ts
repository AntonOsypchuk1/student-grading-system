import styled from 'styled-components';

export const Container = styled.div`
    padding: 1.5rem;
`;

export const Title = styled.h2`
    font-size: 1.5rem;
    font-weight: 600;
    margin-bottom: 1rem;
`;

export const Form = styled.form`
    display: flex;
    flex-direction: column;
    gap: 1rem;
    max-width: 28rem;
    margin: 0 auto;
`;

export const FormGroup = styled.div`
    display: flex;
    flex-direction: column;
    gap: 0.25rem;
`;

export const Label = styled.label`
    display: block;
    text-transform: capitalize;
`;

export const Input = styled.input`
    width: 100%;
    padding: 0.5rem;
    border: 1px solid #e2e8f0;
    border-radius: 0.25rem;

    &:focus {
        outline: none;
        border-color: #2563eb;
        box-shadow: 0 0 0 1px #2563eb;
    }
`;

export const Button = styled.button`
    padding: 0.5rem 1rem;
    background-color: #2563eb;
    color: white;
    border-radius: 0.25rem;
    border: none;
    cursor: pointer;

    &:hover {
        background-color: #1d4ed8;
    }

    &:disabled {
        opacity: 0.5;
        cursor: not-allowed;
    }
`;

export const Grid = styled.div`
    display: grid;
    grid-template-columns: repeat(1, 1fr);
    gap: 1rem;

    @media (min-width: 768px) {
        grid-template-columns: repeat(2, 1fr);
    }

    @media (min-width: 1024px) {
        grid-template-columns: repeat(3, 1fr);
    }
`;

export const Sidebar = styled.aside`
    border-right: 1px solid ${props => props.theme.colors.gray[300]};
    height: 78vh;
    padding-right: 0.75rem;
`;

export const SidebarList = styled.ul`
    list-style: none;
    padding: 0;
    margin: 0;
`;

export const SidebarItem = styled.li<{ isActive: boolean }>`
    padding: 0.5rem 0.5rem;
    margin-bottom: 0.5rem;
    border-radius: 0.375rem;
    cursor: pointer;
    background: ${props => props.isActive ? props.theme.colors.primary + '15' : 'transparent'};
    color: ${props => props.isActive ? props.theme.colors.primary : props.theme.colors.gray[700]};
    font-weight: ${props => props.isActive ? '500' : '400'};

    &:hover {
        background: ${props => props.isActive ? props.theme.colors.primary + '15' : props.theme.colors.gray[100]};
        text-decoration: underline;
    }
`;

export const MainContent = styled.main`
    height: 78vh;
`;

export const FallbackItem = styled.div`
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    height: 50vh;
    color: ${props => props.theme.colors.gray [500]};
`;

export const LoadingSpinner = styled.div`
    border: 4px solid ${props => props.theme.colors.gray[100]};
    border-top: 4px solid ${props => props.theme.colors.primary};
    border-radius: 50%;
    width: 40px;
    height: 40px;
    animation: spin 1s linear infinite;
    margin: 2rem auto;

    @keyframes spin {
        0% {
            transform: rotate(0deg);
        }
        100% {
            transform: rotate(360deg);
        }
    }
`;

export const ErrorMessage = styled.div`
    color: ${props => props.theme.colors.danger};
    text-align: center;
    padding: 2rem;
    background: ${props => `${props.theme.colors.danger}11`};
    border-radius: 8px;
    margin: 2rem 0;
`;