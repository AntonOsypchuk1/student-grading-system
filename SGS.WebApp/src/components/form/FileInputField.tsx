import React from "react";

interface FileInputFieldProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label: string;
  onFileChange: (file: File | null) => void;
}

const FileInputField: React.FC<FileInputFieldProps> = ({ label, onFileChange, ...props }) => {
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      onFileChange(e.target.files[0]);
    } else {
      onFileChange(null);
    }
  };

  return (
    <div className="mb-4">
      <label className="block text-sm font-medium text-gray-700 mb-1">{label}</label>
      <input type="file" onChange={handleChange} {...props} />
    </div>
  );
};

export default FileInputField;
