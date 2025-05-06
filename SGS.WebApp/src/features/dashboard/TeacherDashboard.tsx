export default function TeacherDashboard() {
  return (
    <div className="flex h-screen">
      <Sidebar/>
      <div className="flex-1 flex flex-col">
        <Header/>
        <main className="p-6 bg-gray-50 flex-1 overflow-auto">
          <h2 className="text-2xl font-semibold mb-4">Welcome, Teacher!</h2>
          <section>
            <h3 className="text-xl font-medium mb-2">Pending Submissions</h3>
            <ul className="space-y-2">
              {submissions?.map(s => (
                <li key={s.id} className="p-4 bg-white rounded shadow">
                  {s.studentName}: {s.assignmentTitle}
                </li>
              ))}
            </ul>
          </section>
        </main>
      </div>
    </div>
  );
}