import styled from 'styled-components';

export const Table = styled.table`
    width: 100%;
    border-collapse: collapse;
    background: white;
    border-radius: 8px;
    overflow: hidden;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
`;

export const Th = styled.th`
    padding: 1rem;
    text-align: left;
    background: ${props => props.theme.colors.gray[50]};
    color: ${props => props.theme.colors.gray[700]};
    font-weight: 500;
    border-bottom: 1px solid ${props => props.theme.colors.gray[100]};
`;

export const Td = styled.td`
    padding: 1rem;
    border-bottom: 1px solid ${props => props.theme.colors.gray[100]};
    color: ${props => props.theme.colors.gray[500]};
`;

export const Tr = styled.tr`
    &:hover {
        background: ${props => props.theme.colors.gray[50]};
    }
`;