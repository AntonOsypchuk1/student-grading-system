import styled from 'styled-components';
import {AssignmentDto} from '../../../types/AssignmentTypes.ts';
import {eachDayOfInterval, endOfMonth, format, isSameDay, startOfMonth} from 'date-fns';

const CalendarContainer = styled.div`
    width: 100%;
`;

const CalendarGrid = styled.div`
    display: grid;
    grid-template-columns: repeat(7, 1fr);
    gap: 4px;
`;

const DayCell = styled.div<{ isToday?: boolean; hasAssignment?: boolean }>`
    aspect-ratio: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 4px;
    cursor: pointer;
    position: relative;
    background: ${props =>
  props.isToday
    ? props.theme.colors.primary + '20'
    : props.hasAssignment
      ? props.theme.colors.gray[100]
      : 'transparent'
};

    &:hover {
        background: ${props => props.theme.colors.gray[100]};
    }
`;

const AssignmentIndicator = styled.div`
    position: absolute;
    bottom: 2px;
    width: 4px;
    height: 4px;
    border-radius: 50%;
    background: ${props => props.theme.colors.primary};
`;

interface CalendarProps {
  assignments: AssignmentDto[];
  selectedDate: Date;
  onDateSelect: (date: Date) => void;
}

export function Calendar({assignments, selectedDate, onDateSelect}: CalendarProps) {
  const monthStart = startOfMonth(selectedDate);
  const monthEnd = endOfMonth(selectedDate);
  const days = eachDayOfInterval({start: monthStart, end: monthEnd});

  const hasAssignment = (date: Date) => {
    return assignments.some(assignment =>
      isSameDay(new Date(assignment.dueDate), date)
    );
  };

  return (
    <CalendarContainer>
      <h4>{format(selectedDate, 'MMMM yyyy')}</h4>
      <CalendarGrid>
        {['Нд', 'Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб'].map(day => (
          <div key={day}>{day}</div>
        ))}
        {days.map((day: Date) => (
          <DayCell
            key={day.toString()}
            isToday={isSameDay(day, new Date())}
            hasAssignment={hasAssignment(day)}
            onClick={() => onDateSelect(day)}
          >
            {format(day, 'd')}
            {hasAssignment(day) && <AssignmentIndicator/>}
          </DayCell>
        ))}
      </CalendarGrid>
    </CalendarContainer>
  );
}