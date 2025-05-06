// src/components/Form/SubmitButton.tsx
import React from "react";

interface SubmitButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  text: string;
}

const SubmitButton: React.FC<SubmitButtonProps> = ({ text, ...buttonProps }) => {
  return (
    <button
      type="submit"
      className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
      {...buttonProps}
    >
      {text}
    </button>
  );
};

export default SubmitButton;
