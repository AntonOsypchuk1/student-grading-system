import {useForm} from "react-hook-form";

type UploadForm = {
  courseId: string;
  file: FileList;
};

export default function AssignmentUpload() {
  const {register, handleSubmit, formState: {errors, isSubmitting}} = useForm<UploadForm>();
  const {mutateAsync: upload} = useUploadAssignment();

  const onSubmit = async (data: UploadForm) => {
    const formData = new FormData();
    formData.append("file", data.file[0]);
    formData.append("courseId", data.courseId);
    await upload(formData);
    alert("Uploaded!");
  };

  return (
    <div className="p-6 max-w-lg mx-auto">
      <h2 className="text-2xl font-semibold mb-4">Submit Assignment</h2>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <div>
          <label className="block mb-1">Course ID</label>
          <input
            {...register("courseId", {required: "Course is required"})}
            className="w-full p-2 border rounded"
          />
          {errors.courseId && <p className="text-red-600 text-sm">{errors.courseId.message}</p>}
        </div>

        <div>
          <label className="block mb-1">File</label>
          <input
            {...register("file", {required: "File is required"})}
            type="file"
            className="w-full"
          />
          {errors.file && <p className="text-red-600 text-sm">{errors.file.message}</p>}
        </div>

        <button
          type="submit"
          disabled={isSubmitting}
          className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50"
        >
          {isSubmitting ? "Uploading…" : "Upload"}
        </button>
      </form>
    </div>
  );
}
