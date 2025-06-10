import styled from "styled-components";
import {ActivityItem} from "../types/TeacherDashboardTypes.ts";

const ActivityItemDiv = styled.div`
    padding: 1rem;
    border-bottom: 1px solid ${props => props.theme.colors.gray[100]};

    &:last-child {
        border-bottom: none;
    }
`;

const ActivityTime = styled.span`
    font-size: 0.875rem;
    color: ${props => props.theme.colors.gray[500]};
`;

const ActivityTitle = styled.h4`
    margin: 0.5rem 0;
    color: ${props => props.theme.colors.gray[800]};
`;

const ActivityDescription = styled.p`
    color: ${props => props.theme.colors.gray[500]};
    font-size: 0.875rem;
`;

export const ActivityFeed = ({activities}: { activities: ActivityItem[] }) => (
  <div>
    {activities.map(activity => (
      <ActivityItemDiv key={activity.id}>
        <ActivityTime>{new Date(activity.timestamp).toLocaleString()}</ActivityTime>
        <ActivityTitle>{activity.title}</ActivityTitle>
        <ActivityDescription>{activity.description}</ActivityDescription>
      </ActivityItemDiv>
    ))}
  </div>
);