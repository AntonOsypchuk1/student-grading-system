import {useCourses} from "../../hooks/useCourses";

export default function CourseList() {
  const {data: courses, isLoading} = useCourses();

  if (isLoading) return <div>Loading courses…</div>;

  return (
    <div className="p-6">
      <h2 className="text-2xl font-semibold mb-4">Courses</h2>
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {courses?.map(course => (
          <a
            key={course.id}
            href={`/courses/${course.id}`}
            className="p-4 border rounded hover:shadow"
          >
            <h3 className="text-lg font-medium">{course.title}</h3>
            <p className="text-gray-500 text-sm">{course.description}</p>
          </a>
        )) || <p>No courses found.</p>}
      </div>
    </div>
  );
}
