import styled from 'styled-components';
import {Link} from 'react-router-dom';

export const AuthContainer = styled.div`
    min-height: 100vh;
    display: flex;
    align-items: center;
    justify-content: center;
    background-color: ${props => props.theme.colors.gray[50]};
    padding: 1rem;
`;

export const AuthCard = styled.div`
    background-color: ${props => props.theme.colors.white};
    padding: 2rem;
    border-radius: 0.5rem;
    box-shadow: ${props => props.theme.shadows.md};
    width: 100%;
    max-width: 28rem;
`;

export const AuthHeader = styled.div`
    text-align: center;
    margin-bottom: 2rem;
`;

export const AuthTitle = styled.h1`
    font-size: 1.5rem;
    font-weight: 600;
    color: ${props => props.theme.colors.gray[800]};
    margin-bottom: 0.5rem;
`;

export const AuthDescription = styled.p`
    color: ${props => props.theme.colors.gray[500]};
    font-size: 0.875rem;
`;

export const Form = styled.form`
    display: flex;
    flex-direction: column;
    gap: 1.5rem;
`;

export const AuthFooter = styled.div`
    text-align: center;
    margin-top: 1.5rem;
    font-size: 0.875rem;
    color: ${props => props.theme.colors.gray[500]};
`;

export const AuthLink = styled(Link)`
    color: ${props => props.theme.colors.primary};
    text-decoration: none;
    font-weight: 500;

    &:hover {
        text-decoration: underline;
    }
`;