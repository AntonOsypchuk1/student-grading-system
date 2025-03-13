import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Upload from "../pages/Upload";
import Results from "../pages/Results";
import Dashboard from "../pages/Dashboard.tsx";

const AppRoutes = () => {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Dashboard />} />
        <Route path="/upload" element={<Upload />} />
        <Route path="/results" element={<Results />} />
      </Routes>
    </Router>
  );
};

export default AppRoutes;
