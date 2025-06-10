import 'styled-components';

interface ColorGrades {
  50: string;
  100: string;
  300: string;
  400: string;
  500: string;
  700: string;
  800: string;
}

interface Typography {
  fontFamily: {
    sans: string;
  };
  fontWeight: {
    light: string;
    normal: string;
    medium: string;
    semibold: string;
    bold: string;
  };
  fontSize: {
    xs: string;
    sm: string;
    base: string;
    lg: string;
    xl: string;
    '2xl': string;
    '3xl': string;
    '4xl': string;
  };
  lineHeight: {
    none: string;
    tight: string;
    snug: string;
    normal: string;
    relaxed: string;
    loose: string;
  };
}

declare module 'styled-components' {
  export interface DefaultTheme {
    colors: {
      primary: string;
      primaryHover: string;
      danger: string;
      dangerHover: string;
      success: string;
      successHover: string;
      gray: ColorGrades;
      white: string;
    };
    typography: Typography;
    shadows: {
      sm: string;
      md: string;
      lg: string;
    };
    transitions: {
      default: string;
    };
  }
}