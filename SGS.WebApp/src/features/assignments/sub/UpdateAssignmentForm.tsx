import {useFieldArray, useForm} from 'react-hook-form';
import {FormInput} from "../../../components/form/FormInput.tsx";
import {UpdateAssignmentCommand} from "../../../types/AssignmentTypes.ts";
import {useGroups} from "../../../hooks/useGroups.ts";
import {FormSelect} from "../../../components/form/FormSelect.tsx";
import {useParams} from "react-router-dom";
import {useAssignments, useAssignmentsMutation} from "../../../hooks/useAssignments.ts";
import {Button, CriteriaGroup, Error, Field, FieldGroup, Form} from '../styles.ts';
import {useEffect} from "react";

const UpdateAssignmentForm = () => {
  const {assignmentId} = useParams<{ assignmentId: string }>();
  const {getAssignmentByIdQuery} = useAssignments({assignmentId});
  const assignment = getAssignmentByIdQuery.data;

  const {
    register,
    control,
    handleSubmit,
    reset,
    formState: {errors, isSubmitting}
  } = useForm<UpdateAssignmentCommand>({
    defaultValues: {
      gradingCriteria: [{title: '', description: '', maxGrade: 0, weight: 0}]
    }
  });

  useEffect(() => {
    if (assignment) {
      reset({
        id: assignment.id,
        groupId: assignment.groupId,
        title: assignment.title,
        description: assignment.description,
        maxAttempts: assignment.maxAttempts,
        dueDate: assignment.dueDate,
        gradingCriteria: assignment.gradingCriteria?.map(criteria => ({
          id: criteria.id,
          title: criteria.title,
          description: criteria.description,
          maxGrade: criteria.maxGrade,
          weight: criteria.weight
        })) || []
      });
    }
  }, [assignment, reset]);

  const {fields, append, remove} = useFieldArray({
    name: 'gradingCriteria',
    control
  });

  if (fields.length != assignment?.gradingCriteria?.length) {
    fields.splice(0, fields.length, ...(assignment?.gradingCriteria || []).map(criteria => ({
      id: criteria.id,
      title: criteria.title,
      description: criteria.description,
      maxGrade: criteria.maxGrade,
      weight: criteria.weight
    })));
  }

  const {getGroupsByCourseIdQuery} = useGroups({courseId: assignment?.courseId});
  const {data: groups} = getGroupsByCourseIdQuery;
  const groupNames = groups?.map(group =>
    ({value: group.id, label: group.name})) || [];
  const {updateAssignmentMutation} = useAssignmentsMutation();

  const onSubmit = async (data: UpdateAssignmentCommand) => {
    console.log(data)
    try {
      await updateAssignmentMutation.mutateAsync(data);
      window.location.replace(`/assignments/${assignment?.id}`);
    } catch (error) {
      console.error('Error updating assignment:', error);
    }
  };

  if (getAssignmentByIdQuery.isLoading) {
    return <div>Loading...</div>;
  }

  return (
    <Form onSubmit={handleSubmit(onSubmit)}>
      <Field>
        <FormInput
          label="Назва завдання"
          register={register}
          name="title"
          rules={{required: 'Введіть назву завдання'}}
          errors={errors}
          type="text"
        />
        {errors.title && <Error>{errors.title.message}</Error>}
      </Field>

      <Field>
        <FormInput
          label="Опис завдання"
          register={register}
          {...register('description', {required: 'Введіть опис завдання'})}
          rows={4}
          errors={errors}
        />
      </Field>

      <FieldGroup>
        <Field>
          <FormInput
            label="Строк подання"
            name="dueDate"
            register={register}
            rules={{required: 'Введіть строк подання'}}
            type="date"
            errors={errors}
            defaultValue={assignment?.dueDate ? new Date(assignment.dueDate).toISOString().split('T')[0] : ''}
          />
        </Field>
        <Field>
          <FormInput
            style={{maxWidth: '100%', width: '100%', textAlign: 'left'}}
            label="Макс. спроб"
            register={register}
            {...register(`maxAttempts`, {
              required: 'Введіть макс. спроби',
              min: {value: 1, message: 'Min value is 1'},
              max: {value: 10, message: 'Max value is 10'}
            })}
            type="number"
            placeholder="Макс. спроби"
            errors={errors}
            min={1}
            max={10}
          />
        </Field>
        <Field>
          <FormSelect
            label="Група"
            options={groupNames}
            register={register}
            {...register('groupId', {required: 'Виберіть групу'})}
            errors={errors}
            defaultValue={assignment?.groupId}
          />
        </Field>
      </FieldGroup>

      <Field>
        <label>Критерії оцінювання</label>
        {fields.map((field, index) => (
          <CriteriaGroup key={field.id}>
            <FormInput
              type="text"
              register={register}
              {...register(`gradingCriteria.${index}.title`, {
                required: 'Введіть назву критерію'
              })}
              placeholder="Назва критерію"
              errors={errors}
            />
            <FormInput
              type="text"
              register={register}
              {...register(`gradingCriteria.${index}.description`, {
                required: 'Введіть опис критерію'
              })}
              placeholder="Опис критерію"
              errors={errors}
            />
            <FormInput
              register={register}
              {...register(`gradingCriteria.${index}.maxGrade`, {
                required: 'Макс. бали обов\'язкові',
                min: {value: 1, message: 'Min value is 1'},
                max: {value: 100, message: 'Max value is 100'}
              })}
              type="number"
              placeholder="Макс. бал"
              errors={errors}
              min={1}
              max={100}
            />
            <FormInput
              register={register}
              {...register(`gradingCriteria.${index}.weight`, {
                required: 'Вага критерію обов\'язкова',
                min: {value: 0.1, message: 'Min value is 0.1'},
                max: {value: 1, message: 'Max value is 1'}
              })}
              type="number"
              placeholder="Вага"
              errors={errors}
              min={0}
              max={1}
              step={0.1}
            />
            <Button style={{background: "#dc2626"}} disabled={fields.length <= 1} type="button"
                    onClick={() => remove(index)}>
              X
            </Button>
          </CriteriaGroup>
        ))}
        <Button
          type="button"
          onClick={() => append({id: '', title: '', description: '', maxGrade: 0, weight: 0})}
          disabled={fields.length >= 5}
        >
          Додати критерій
        </Button>
      </Field>

      <Button type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'Оновлюємо...' : 'Оновити завдання'}
      </Button>
    </Form>
  );
};

export default UpdateAssignmentForm;