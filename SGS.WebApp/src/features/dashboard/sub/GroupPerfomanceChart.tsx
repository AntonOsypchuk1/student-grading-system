import {Bar} from 'react-chartjs-2';
import {GroupPerformance} from "../types/TeacherDashboardTypes.ts";

export const GroupsPerformanceChart = ({groups}: { groups: GroupPerformance[] }) => {
  const data = {
    labels: groups.map(g => g.groupName),
    datasets: [
      {
        label: 'Average Grade',
        data: groups.map(g => g.averageGrade),
        backgroundColor: 'rgba(54, 162, 235, 0.5)',
      },
      {
        label: 'Submission Rate',
        data: groups.map(g => g.submissionRate * 100),
        backgroundColor: 'rgba(75, 192, 192, 0.5)',
      },
    ],
  };

  const options = {
    responsive: true,
    scales: {
      y: {
        beginAtZero: true,
        max: 100,
      },
    },
  };

  return <Bar data={data} options={options}/>;
};