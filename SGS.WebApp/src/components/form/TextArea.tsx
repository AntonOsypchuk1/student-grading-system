// src/components/Form/TextAreaField.tsx
import React from "react";
import { FieldError, UseFormRegisterReturn } from "react-hook-form";

interface TextAreaFieldProps extends React.TextareaHTMLAttributes<HTMLTextAreaElement> {
  label: string;
  error?: FieldError;
  registration: UseFormRegisterReturn;
}

const TextAreaField: React.FC<TextAreaFieldProps> = ({ label, error, registration, ...textAreaProps }) => {
  return (
    <div className="mb-4">
      <label className="block text-sm font-medium text-gray-700 mb-1">{label}</label>
      <textarea
        {...registration}
        {...textAreaProps}
        className={`w-full px-3 py-2 border ${
          error ? "border-red-500" : "border-gray-300"
        } rounded-md focus:outline-none focus:ring focus:border-blue-300`}
      />
      {error && <p className="text-red-500 text-xs mt-1">{error.message}</p>}
    </div>
  );
};

export default TextAreaField;
