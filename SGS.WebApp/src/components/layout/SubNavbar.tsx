import styled from 'styled-components';
import {Link, useLocation} from 'react-router-dom';
import {useNavigation} from '../../context/NavigationContext';

const Nav = styled.nav`
    margin-bottom: 1rem;
    border-bottom: 1px solid ${props => props.theme.colors.gray[300]};
`;

const NavList = styled.ul`
    display: flex;
    gap: 1rem;
`;

const NavItem = styled(Link)<{ active: boolean }>`
    padding: 0.5rem;
    color: ${props => props.active ? props.theme.colors.primary : props.theme.colors.gray[500]};
    border-bottom: 2px solid ${props => props.active ? props.theme.colors.primary : 'transparent'};

    &:hover {
        color: ${props => props.theme.colors.primary};
    }
`;

export const SubNavbar = () => {
  const {links} = useNavigation();
  const location = useLocation();

  return (
    <>
      {links.length > 0 && (
        <Nav>
          <NavList>
            {links.map(link => (
              <NavItem
                key={link.path}
                to={link.path}
                active={location.pathname === link.path}
              >
                {link.title}
              </NavItem>
            ))}
          </NavList>
        </Nav>
      )}
    </>
  );
};