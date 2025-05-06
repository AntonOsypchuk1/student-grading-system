export default function AssignmentManage() {
  const {data: subs, isLoading} = useAssignmentsToGrade();

  if (isLoading) return <div>Loading submissions…</div>;

  return (
    <div className="p-6">
      <h2 className="text-2xl font-semibold mb-4">Manage Assignments</h2>
      <table className="w-full table-auto border-collapse">
        <thead>
        <tr>
          <th className="border px-2 py-1">Student</th>
          <th className="border px-2 py-1">Assignment</th>
          <th className="border px-2 py-1">Status</th>
          <th className="border px-2 py-1">Action</th>
        </tr>
        </thead>
        <tbody>
        {subs?.map(s => (
          <tr key={s.id}>
            <td className="border px-2 py-1">{s.studentName}</td>
            <td className="border px-2 py-1">{s.assignmentTitle}</td>
            <td className="border px-2 py-1">{s.status}</td>
            <td className="border px-2 py-1">
              <a href={`/assignments/manage?submissionId=${s.id}`} className="text-blue-600">
                Grade
              </a>
            </td>
          </tr>
        )) || (
          <tr>
            <td colSpan={4} className="text-center py-4">No submissions</td>
          </tr>
        )}
        </tbody>
      </table>
    </div>
  );
}
