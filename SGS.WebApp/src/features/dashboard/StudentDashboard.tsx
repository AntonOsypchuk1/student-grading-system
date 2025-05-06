import {useAuthContext} from "../../auth/AuthProvider";
import {useAssignments} from "../../hooks/useAssignments.ts";

export default function StudentDashboard() {
  const {user} = useAuthContext();
  const {data: assignments, isLoading: isAssignmentsLoading} = useAssignments();
  const isLoading = user.isLoading || isAssignmentsLoading;

  if (isLoading) return <div>Loading assignments…</div>;

  const upcoming = (assignments || [])
    .filter(a => new Date(a.dueDate) > new Date())
    .slice(0, 5);

  return (
    <div className="p-6">
      <h2 className="text-2xl font-semibold mb-4">Welcome, {user.fullName}</h2>
      <section className="mb-8">
        <h3 className="text-xl mb-2">Upcoming Deadlines</h3>
        {/*<ul className="space-y-2">*/}
        {/*  {upcoming.length*/}
        {/*    ? upcoming.map(a => (*/}
        {/*      <li key={a.id} className="p-4 border rounded flex justify-between">*/}
        {/*        <span>{a.title}</span>*/}
        {/*        <span className="text-sm text-gray-500">*/}
        {/*            {new Date(a.dueDate).toLocaleDateString()}*/}
        {/*          </span>*/}
        {/*      </li>*/}
        {/*    ))*/}
        {/*    : <li>No upcoming assignments</li>}*/}
        {/*</ul>*/}
      </section>

      <section>
        <h3 className="text-xl mb-2">Submit a New Assignment</h3>
        <a
          href="/assignments/upload"
          className="inline-block px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
        >
          Go to Upload
        </a>
      </section>
    </div>
  );
}
