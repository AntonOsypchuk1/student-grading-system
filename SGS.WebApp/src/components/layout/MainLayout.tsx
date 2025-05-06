import React from "react";

import {Outlet} from "react-router-dom";
import Navbar from "./Navbar.tsx";
import Footer from "./Footer.tsx";
import Sidebar from "./Sidebar.tsx";

const MainLayout: React.FC = () => {
  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar/>

      <main className="container mx-auto px-4 py-8">
        <Sidebar/>
        <Outlet/>
      </main>

      <Footer/>
    </div>
  );
};

export default MainLayout;