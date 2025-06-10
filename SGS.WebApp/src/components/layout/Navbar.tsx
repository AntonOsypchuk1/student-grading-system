import React from "react";
import {Link, useNavigate} from "react-router-dom";
import styled from 'styled-components';
import {useAuthContext} from "../../auth/AuthProvider";

const Nav = styled.nav`
    background-color: ${props => props.theme.colors.white};
    box-shadow: ${props => props.theme.shadows.md};
`;

const NavContainer = styled.div`
    max-width: 80rem;
    margin: 0 auto;
    padding: 0 1rem;
    @media (min-width: 640px) {
        padding: 0 1.5rem;
    }
    @media (min-width: 1024px) {
        padding: 0 2rem;
    }
`;

const NavContent = styled.div`
    display: flex;
    justify-content: space-between;
    height: 4rem;
`;

const LogoContainer = styled.div`
    display: flex;
    align-items: center;
`;

const Logo = styled(Link)`
    display: flex;
    align-items: center;
    text-decoration: none;
`;

const LogoText = styled.span`
    font-size: 1.25rem;
    font-weight: 700;
    color: ${props => props.theme.colors.gray[800]};
`;

const NavActions = styled.div`
    display: flex;
    align-items: center;
    gap: 1rem;
`;

const WelcomeText = styled.span`
    color: ${props => props.theme.colors.gray[700]};
`;

const LoginLink = styled(Link)`
    color: ${props => props.theme.colors.gray[500]};
    transition: color ${props => props.theme.transitions.default};
    text-decoration: none;

    &:hover {
        color: ${props => props.theme.colors.gray[800]};
    }
`;

const RegisterLink = styled(Link)`
    background-color: ${props => props.theme.colors.primary};
    color: ${props => props.theme.colors.white};
    padding: 0.5rem 1rem;
    border-radius: 0.5rem;
    transition: background-color ${props => props.theme.transitions.default};
    text-decoration: none;

    &:hover {
        background-color: ${props => props.theme.colors.primaryHover};
    }
`;

const LogoutButton = styled.button`
    background-color: ${props => props.theme.colors.danger};
    color: ${props => props.theme.colors.white};
    padding: 0.5rem 1rem;
    border-radius: 0.5rem;
    transition: background-color ${props => props.theme.transitions.default};

    &:hover {
        background-color: ${props => props.theme.colors.dangerHover};
    }
`;

const Navbar: React.FC = () => {
  const {user, logout} = useAuthContext();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  return (
    <Nav>
      <NavContainer>
        <NavContent>
          <LogoContainer>
            <Logo to="/">
              <LogoText>EduPortal</LogoText>
            </Logo>
          </LogoContainer>

          <NavActions>
            {user ? (
              <>
                <WelcomeText>Welcome, {user.data!.fullName}</WelcomeText>
                <LogoutButton onClick={handleLogout}>
                  Logout
                </LogoutButton>
              </>
            ) : (
              <>
                <LoginLink to="/login">Login</LoginLink>
                <RegisterLink to="/register">Register</RegisterLink>
              </>
            )}
          </NavActions>
        </NavContent>
      </NavContainer>
    </Nav>
  );
};

export default Navbar;