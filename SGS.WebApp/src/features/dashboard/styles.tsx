import styled from "styled-components";

export const DashboardContainer = styled.div`
    display: flex;
    flex-direction: column;
    overflow: hidden;
    height: 83vh;
`;

export const DashboardContent = styled.div`
    display: grid;
    grid-template-columns: 350px 1fr; // Fixed width for calendar
    gap: 2rem;
    height: 100%;
    overflow: hidden;
`;

export const LeftPanel = styled.div`
    display: flex;
    flex-direction: column;
    gap: 2rem;
    overflow-y: auto;
    padding-right: 1rem;
`;

export const MainPanel = styled.div`
    display: flex;
    flex-direction: column;
    padding-right: 1rem;
    gap: 2rem;
    overflow-y: auto;
`;

export const Widget = styled.div`
    background: white;
    border-radius: 12px;
    padding: 1.5rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
    height: fit-content;
`;

export const DashboardHeader = styled.div`
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 2rem;
`;

export const Title = styled.h2`
    font-size: 1.5rem;
    font-weight: 600;
    color: ${props => props.theme.colors.gray[800]};
`;

export const Stats = styled.div`
    display: flex;
    gap: 2rem;
`;

export const StatItem = styled.div`
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: 0.5rem;
`;

export const StatNumber = styled.span`
    font-size: 1.25rem;
    font-weight: 600;
    color: ${props => props.theme.colors.primary};
`;

export const StatLabel = styled.span`
    font-size: 0.875rem;
    color: ${props => props.theme.colors.gray[500]};
`;

export const WidgetTitle = styled.h3`
    display: flex;
    justify-content: space-between;
    font-size: 1.25rem;
    margin-bottom: 1rem;
    color: ${props => props.theme.colors.gray[800]};
`;

export const CourseStats = styled.div`
    display: flex;
    justify-content: space-between;
    padding: 1rem;
    background: ${props => props.theme.colors.gray[50]};
    border-radius: 8px;
    margin: 1rem 0;

    div {
        font-size: 0.875rem;
        color: ${props => props.theme.colors.gray[500]};
    }
`;

export const GroupsList = styled.div`
    margin-top: 1rem;
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
`;

export const GroupItem = styled.div`
    padding: 0.75rem;
    background: ${props => props.theme.colors.gray[50]};
    border-radius: 6px;
    font-size: 0.875rem;
    color: ${props => props.theme.colors.gray[700]};
    transition: all 0.2s ease;

    &:hover {
        background: ${props => props.theme.colors.gray[100]};
    }
`;
