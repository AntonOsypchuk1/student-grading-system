// src/components/Form/SelectField.tsx
import React from "react";
import { FieldError, UseFormRegisterReturn } from "react-hook-form";

interface Option {
  value: string;
  label: string;
}

interface SelectFieldProps extends React.SelectHTMLAttributes<HTMLSelectElement> {
  label: string;
  error?: FieldError;
  registration: UseFormRegisterReturn;
  options: Option[];
}

const SelectField: React.FC<SelectFieldProps> = ({
  label,
  error,
  registration,
  options,
  ...selectProps
}) => {
  return (
    <div className="mb-4">
      <label className="block text-sm font-medium text-gray-700 mb-1">{label}</label>
      <select
        {...registration}
        {...selectProps}
        className={`w-full px-3 py-2 border ${
          error ? "border-red-500" : "border-gray-300"
        } rounded-md focus:outline-none focus:ring focus:border-blue-300`}
      >
        {options.map((opt) => (
          <option key={opt.value} value={opt.value}>
            {opt.label}
          </option>
        ))}
      </select>
      {error && <p className="text-red-500 text-xs mt-1">{error.message}</p>}
    </div>
  );
};

export default SelectField;
