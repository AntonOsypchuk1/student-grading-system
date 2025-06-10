import React from "react";
import styled from 'styled-components';

const FooterContainer = styled.footer`
    background-color: ${props => props.theme.colors.gray[800]};
    color: ${props => props.theme.colors.gray[300]};
`;

const FooterContent = styled.div`
    max-width: 80rem;
    margin: 0 auto;
    padding: 2rem 1rem;

    @media (min-width: 640px) {
        padding: 2rem 1.5rem;
    }
    @media (min-width: 1024px) {
        padding: 2rem;
    }
`;

const Grid = styled.div`
    display: grid;
    grid-template-columns: 1fr;
    gap: 2rem;

    @media (min-width: 768px) {
        grid-template-columns: repeat(3, 1fr);
    }
`;

const SectionTitle = styled.h3`
    font-size: 1.125rem;
    font-weight: 600;
    margin-bottom: 1rem;
`;

const SectionText = styled.p`
    font-size: 0.875rem;
`;

const ContactList = styled.ul`
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
`;

const ContactItem = styled.li`
    font-size: 0.875rem;
`;

const Copyright = styled.div`
    margin-top: 2rem;
    padding-top: 2rem;
    border-top: 1px solid ${props => props.theme.colors.gray[700]};
    text-align: center;
`;

const CopyrightText = styled.p`
    font-size: 0.875rem;
`;

const Footer: React.FC = () => {
  return (
    <FooterContainer>
      <FooterContent>
        <Grid>
          <div>
            <SectionTitle>About Us</SectionTitle>
            <SectionText>
              StudentGradingSystem - це ваша комплексна платформа для управління навчанням,
              розроблений для покращення навчального процесу як для студентів, так і для викладачів.
            </SectionText>
          </div>

          <div>
            <SectionTitle>Контакти</SectionTitle>
            <ContactList>
              <ContactItem>Email: support@sgs.com</ContactItem>
              <ContactItem>Телефон: (555) 123-4567</ContactItem>
              <ContactItem>Адреса: 123 Education St, Learning City</ContactItem>
            </ContactList>
          </div>
        </Grid>

        <Copyright>
          <CopyrightText>
            © {new Date().getFullYear()} StudentGradingSystem. Всі права захищені.
          </CopyrightText>
        </Copyright>
      </FooterContent>
    </FooterContainer>
  );
};

export default Footer;