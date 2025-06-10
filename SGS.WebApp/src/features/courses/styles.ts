import styled from "styled-components";
import {Link} from "react-router-dom";

export const Container = styled.div`
`;

export const Title = styled.h2`
    font-size: ${props => props.theme.typography.fontSize['xl']};
    font-weight: ${props => props.theme.typography.fontWeight.semibold};
    color: ${props => props.theme.colors.gray[800]};
`;

export const Description = styled.p`
    color: ${props => props.theme.colors.gray[700]};
    line-height: ${props => props.theme.typography.lineHeight.relaxed};
`;

export const SectionTitle = styled.h3`
    font-size: ${props => props.theme.typography.fontSize.xl};
    font-weight: ${props => props.theme.typography.fontWeight.medium};
    color: ${props => props.theme.colors.gray[800]};
`;

export const AssignmentsList = styled.ul`
    list-style-type: disc;
    list-style-position: inside;
`;

export const AssignmentItem = styled.li`
`;

export const AssignmentLink = styled.a`
    color: ${props => props.theme.colors.primary};
    text-decoration: none;
    transition: all ${props => props.theme.transitions.default};

    &:hover {
        text-decoration: underline;
        color: ${props => props.theme.colors.primaryHover};
    }
`;

export const LoadingWrapper = styled.div`
    display: flex;
    justify-content: center;
    align-items: center;
    height: 200px;
    color: ${props => props.theme.colors.gray[500]};
    font-size: ${props => props.theme.typography.fontSize.lg};
`;

export const ErrorMessage = styled.div`
    color: ${props => props.theme.colors.danger};
    text-align: center;
    padding: 4rem;
`;

export const Table = styled.table`
    width: 100%;
    border-collapse: separate;
    border-spacing: 0;
`;

export const Th = styled.th`
    padding: 0.75rem;
    border-width: 0px 1px 1px 0;
    border-style: solid;
    border-color: ${props => props.theme.colors.gray[300]};
    text-align: center;

    &:first-child {
        border-left-width: 0;
    }

    &:last-child {
        border-right-width: 0;
    }
`;

export const Td = styled.td`
    padding: 0.75rem;
    border-width: 0 1px 1px 0;
    border-style: solid;
    border-color: ${props => props.theme.colors.gray[300]};

    &:first-child {
        border-left-width: 0;
    }

    &:last-child {
        border-right-width: 0;
    }
`;

export const THead = styled.thead`
    tr:first-child {
        th {
            border-top-width: 1px;
        }
    }
`;

export const TBody = styled.tbody`
    tr:last-child {
        td {
            border-bottom-width: 1px;
        }
    }
`;

export const CourseContainer = styled.div`
    padding: 2rem;
    height: 100%;
    overflow-y: auto;
`;

export const CourseHeader = styled.div`
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    margin-bottom: 2rem;
`;

export const CourseTitle = styled.h1`
    font-size: 2rem;
    color: ${props => props.theme.colors.gray[800]};
    margin-bottom: 0.5rem;
`;

export const CourseDescription = styled.p`
    color: ${props => props.theme.colors.gray[500]};
    max-width: 600px;
`;

export const CourseStats = styled.div`
    display: flex;
    gap: 1rem;
`;

export const StatBox = styled.div`
    display: flex;
    justify-content: space-around;
    align-items: center;
    padding: 1rem;
    background: white;
    border-radius: 8px;
    min-width: 120px;
    text-align: center;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
`;

export const ContentGrid = styled.div`
    display: grid;
    grid-template-columns: 300px 1fr;
    gap: 2rem;
    height: calc(100% - 100px);
`;

export const GroupCard = styled.div<{ isSelected: boolean }>`
    background: white;
    border-radius: 8px;
    padding: 1rem;
    cursor: pointer;
    transition: all 0.2s ease;
    border: 2px solid ${props =>
            props.isSelected ? props.theme.colors.primary : 'transparent'};

    &:hover {
        transform: translateY(-2px);
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    }
`;

export const AssignmentCard = styled.div`
    background: white;
    border-radius: 8px;
    padding: 1rem;
    margin-bottom: 1rem;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
    transition: all 0.2s ease;

    &:hover {
        transform: translateY(-2px);
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    }
`;

export const LeftSection = styled.div`
    display: flex;
    flex-direction: column;
    gap: 1.5rem;
    overflow-y: auto;
`;

export const GroupsContainer = styled.div`
    display: flex;
    flex-direction: column;
    gap: 1rem;
`;

export const GroupHeader = styled.div`
    display: flex;
    justify-content: space-between;
    align-items: center;
`;

export const GroupName = styled.h4`
    margin: 0;
    font-size: 1rem;
    color: ${props => props.theme.colors.gray[800]};
`;

export const GroupCount = styled.span`
    font-size: 0.875rem;
    color: ${props => props.theme.colors.gray[500]};
`;

export const GroupDetails = styled.div`
    margin-top: 1rem;
    padding-top: 1rem;
    border-top: 1px solid ${props => props.theme.colors.gray[100]};
`;

export const StudentsList = styled.div`
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
`;

export const StudentItem = styled.div`
    padding: 0.5rem;
    background: ${props => props.theme.colors.gray[50]};
    border-radius: 4px;
    font-size: 0.875rem;
    color: ${props => props.theme.colors.gray[700]};
`;

export const SectionHeader = styled.div`
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1.5rem;
`;

export const FilterInfo = styled.span`
    font-size: 0.875rem;
    color: ${props => props.theme.colors.gray[500]};
`;

export const AssignmentHeader = styled.div`
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    margin-bottom: 1rem;
`;

export const AssignmentTitle = styled.h3`
    margin: 0;
    font-size: 1.125rem;
    color: ${props => props.theme.colors.gray[800]};
`;

export const DueDate = styled.span`
    font-size: 0.875rem;
    color: ${props => props.theme.colors.gray[500]};
`;

export const AssignmentDescription = styled.p`
    color: ${props => props.theme.colors.gray[500]};
    font-size: 0.875rem;
`;

export const AssignmentFooter = styled.div`
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-top: 1rem;
    padding-top: 1rem;
    border-top: 1px solid ${props => props.theme.colors.gray[100]};
`;

export const SubmissionsCount = styled.span`
    font-size: 0.875rem;
    color: ${props => props.theme.colors.gray[500]};
`;

export const ViewDetailsButton = styled(Link)`
    padding: 0.5rem 1rem;
    background: ${props => props.theme.colors.primary};
    color: white;
    border-radius: 4px;
    text-decoration: none;
    font-size: 0.875rem;
    transition: all 0.2s ease;

    &:hover {
        background: ${props => props.theme.colors.primary}ee;
    }
`;