import {createGlobalStyle} from 'styled-components';

export const GlobalStyles = createGlobalStyle`
    @import url('https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap');

    *, *::before, *::after {
        box-sizing: border-box;
        margin: 0;
        padding: 0;
    }

    html {
        font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto,
        Oxygen, Ubuntu, Cantarell, 'Open Sans', 'Helvetica Neue', sans-serif;
        -webkit-font-smoothing: antialiased;
        -moz-osx-font-smoothing: grayscale;
    }

    body {
        line-height: 1.5;
        color: ${props => props.theme.colors.gray[800]};
    }

    button {
        font-family: inherit;
        border: none;
        cursor: pointer;
        background: none;
        padding: 0;
    }

    a {
        color: inherit;
        text-decoration: none;
    }

    input,
    textarea,
    select {
        font-family: inherit;
    }
`;