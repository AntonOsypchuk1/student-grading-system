import React, {createContext, ReactNode, useContext} from 'react';

type NavLink = {
  title: string;
  path: string;
};

type NavigationContextType = {
  links: NavLink[];
  setLinks: (links: NavLink[]) => void;
};

const NavigationContext = createContext<NavigationContextType>({
  links: [],
  setLinks: () => {
  }
});

export const NavigationProvider = ({children}: { children: ReactNode }) => {
  const [links, setLinks] = React.useState<NavLink[]>([]);

  return (
    <NavigationContext.Provider value={{links, setLinks}}>
      {children}
    </NavigationContext.Provider>
  );
};

export const useNavigation = () => useContext(NavigationContext);