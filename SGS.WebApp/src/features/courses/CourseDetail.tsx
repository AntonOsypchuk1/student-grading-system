import {useParams} from "react-router-dom";

export default function CourseDetail() {
  const {id} = useParams<{ id: string }>();
  const {data: course, isLoading} = useCourse(id!);

  if (isLoading) return <div>Loading course…</div>;
  if (!course) return <div>Course not found</div>;

  return (
    <div className="p-6">
      <h2 className="text-2xl font-semibold mb-4">{course.title}</h2>
      <p className="mb-6">{course.description}</p>

      <h3 className="text-xl mb-2">Assignments</h3>
      <ul className="list-disc list-inside">
        {course.assignments.length
          ? course.assignments.map(a => (
            <li key={a.id}>
              <a
                href={`/assignments/upload?courseId=${course.id}`}
                className="text-blue-600 hover:underline"
              >
                {a.title} (due {new Date(a.dueDate).toLocaleDateString()})
              </a>
            </li>
          ))
          : <li>No assignments yet</li>}
      </ul>
    </div>
  );
}
