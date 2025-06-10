import {useFieldArray, useForm} from 'react-hook-form';
import {FormInput} from "../../../components/form/FormInput.tsx";
import {CreateAssignmentCommand} from "../../../types/AssignmentTypes.ts";
import {useGroups} from "../../../hooks/useGroups.ts";
import {FormSelect} from "../../../components/form/FormSelect.tsx";
import {useParams} from "react-router-dom";
import {useCriteriaMutation} from "../../../hooks/useCriteria.ts";
import {useAssignmentsMutation} from "../../../hooks/useAssignments.ts";
import {CreateGradingCriteriaCommand} from "../../../types/GradingCriteriaTypes.ts";
import {Button, CriteriaGroup, Error, Field, FieldGroup, Form} from '../styles.ts';


const AssignmentForm = () => {
  const {
    register,
    control,
    handleSubmit,
    formState: {errors, isSubmitting}
  } = useForm<CreateAssignmentCommand>({
    defaultValues: {
      gradingCriteria: [{title: '', description: '', maxGrade: 0, weight: 0}]
    }
  });

  const {fields, append, remove} = useFieldArray({
    name: 'gradingCriteria',
    control
  });

  const {courseId} = useParams<{ courseId: string }>();
  const {getGroupsByCourseIdQuery} = useGroups({courseId});
  const {data: groups} = getGroupsByCourseIdQuery;
  const groupNames = groups?.map(group =>
    ({value: group.id, label: group.name})) || [];
  const {createAssignmentMutation} = useAssignmentsMutation();
  const {createMultipleGradingCriteria} = useCriteriaMutation();

  const onSubmit = async (data: CreateAssignmentCommand) => {
    try {
      data.courseId = courseId!;
      const {gradingCriteria, ...payload} = data;

      if (gradingCriteria) {
        const assignment = await createAssignmentMutation.mutateAsync(payload);
        if (assignment) {
          const criteriaPayload = gradingCriteria.map((criteria: CreateGradingCriteriaCommand) => ({
            ...criteria,
            assignmentId: assignment.id
          }));
          await createMultipleGradingCriteria.mutateAsync({criteria: criteriaPayload});
          window.location.replace(`/courses/${courseId}/assignments`);
        }
      }
    } catch (error) {
      console.error('Error submitting form:', error);
    }
  };

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
          />
        </Field>
      </FieldGroup>

      <Field>
        <label>Grading Criteria</label>
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
          onClick={() => append({title: '', description: '', maxGrade: 0, weight: 0})}
          disabled={fields.length >= 5}
        >
          Додати критерій
        </Button>
      </Field>

      <Button type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'Створюємо...' : 'Створити завдання'}
      </Button>
    </Form>
  );
};

export default AssignmentForm;