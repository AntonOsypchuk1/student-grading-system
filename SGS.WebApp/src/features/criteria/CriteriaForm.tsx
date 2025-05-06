import React, {useEffect} from "react";
import {useForm} from "react-hook-form";
import {useNavigate, useParams} from "react-router-dom";

type FormValues = { name: string; description: string; weight: number };

export default function CriteriaForm() {
  const {id} = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEdit = Boolean(id);

  const {data: existing, isLoading: loadingExisting} = useCriteriaById(id!);
  const create = useCreateCriteria();
  const update = useUpdateCriteria();

  const {register, handleSubmit, reset, formState: {errors, isSubmitting}} =
    useForm<FormValues>();

  useEffect(() => {
    if (existing) {
      reset({
        name: existing.name,
        description: existing.description,
        weight: existing.weight,
      });
    }
  }, [existing, reset]);

  if (loadingExisting) return <div>Loading…</div>;

  const onSubmit = async (data: FormValues) => {
    if (isEdit) {
      await update.mutateAsync({id: id!, ...data});
    } else {
      await create.mutateAsync(data);
    }
    navigate("/criteria");
  };

  return (
    <div className="p-6 max-w-md mx-auto">
      <h2 className="text-2xl font-semibold mb-4">
        {isEdit ? "Edit Criterion" : "New Criterion"}
      </h2>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        {["name", "description"].map(field => (
          <div key={field}>
            <label className="block mb-1 capitalize">{field}</label>
            <input
              {...register(field as keyof FormValues, {required: `${field} is required`})}
              className="w-full p-2 border rounded"
            />
            {errors[field as keyof FormValues] && (
              <p className="text-red-600 text-sm">
                {errors[field as keyof FormValues]?.message}
              </p>
            )}
          </div>
        ))}

        <div>
          <label className="block mb-1">Weight (%)</label>
          <input
            {...register("weight", {required: "Weight is required", min: 0})}
            type="number"
            className="w-full p-2 border rounded"
          />
          {errors.weight && <p className="text-red-600 text-sm">{errors.weight.message}</p>}
        </div>

        <button
          type="submit"
          disabled={isSubmitting}
          className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50"
        >
          {isSubmitting ? "Saving…" : "Save Criterion"}
        </button>
      </form>
    </div>
  );
}
