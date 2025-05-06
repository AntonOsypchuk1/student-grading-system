import {useCriteria} from "../../hooks/useCriteria";

export default function CriteriaList() {
  const {data: criteria, isLoading} = useCriteria();

  if (isLoading) return <div>Loading criteria…</div>;

  return (
    <div className="p-6">
      <div className="flex justify-between mb-4">
        <h2 className="text-2xl font-semibold">Grading Criteria</h2>
        <a
          href="/criteria/new"
          className="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700"
        >
          New Criterion
        </a>
      </div>

      <ul className="space-y-2">
        {criteria?.map(c => (
          <li
            key={c.id}
            className="p-4 border rounded flex justify-between items-center"
          >
            <span>{c.name}</span>
            <a href={`/criteria/${c.id}`} className="text-blue-600 hover:underline">
              Edit
            </a>
          </li>
        )) || <li>No criteria defined.</li>}
      </ul>
    </div>
  );
}
