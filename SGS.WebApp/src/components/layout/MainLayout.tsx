import React from "react";
import {Outlet} from "react-router-dom";
import styled from 'styled-components';
import Navbar from "./Navbar";
import Footer from "./Footer";
import {useAuthContext} from "../../auth/AuthProvider";
import {Sidebar} from "./Sidebar";
import {NavigationProvider} from "../../context/NavigationContext.tsx";
import {SubNavbar} from "./SubNavbar.tsx";

const LayoutWrapper = styled.div`
    min-height: 100vh;
    background-color: ${props => props.theme.colors.gray[50]};
`;

const Container = styled.div`
    display: flex;
    max-width: 1280px;
    margin: 0 auto;
    padding: 2rem 1rem;
    height: 95vh;
`;

const Main = styled.main<{ hasUser: boolean }>`
    flex-grow: 1;
    margin-left: 2rem;
    padding: 1rem;
    border-radius: 1rem;
    border: 1px solid ${props => props.theme.colors.gray[300]};
    max-height: 88vh;
`;

const MainLayout: React.FC = () => {
  const {user} = useAuthContext();

  return (
    <NavigationProvider>
      <LayoutWrapper>
        <Navbar/>
        <Container>
          {user && (
            <Sidebar/>
          )}
          <Main hasUser={Boolean(user)}>
            <SubNavbar/>
            <Outlet/>
          </Main>
        </Container>
        <Footer/>
      </LayoutWrapper>
    </NavigationProvider>
  );
};

export default MainLayout;