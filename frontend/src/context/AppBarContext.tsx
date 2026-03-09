import { createContext, useContext, useState, useCallback } from 'react';
import type { ReactNode } from 'react';

interface AppBarState {
  title: string | null;
  onBack: (() => void) | null;
}

interface AppBarContextValue extends AppBarState {
  setAppBar: (state: Partial<AppBarState>) => void;
  resetAppBar: () => void;
}

const AppBarContext = createContext<AppBarContextValue>({
  title: null,
  onBack: null,
  setAppBar: () => {},
  resetAppBar: () => {},
});

export function AppBarProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AppBarState>({ title: null, onBack: null });

  const setAppBar = useCallback((newState: Partial<AppBarState>) => {
    setState((prev) => ({ ...prev, ...newState }));
  }, []);

  const resetAppBar = useCallback(() => {
    setState({ title: null, onBack: null });
  }, []);

  return (
    <AppBarContext.Provider value={{ ...state, setAppBar, resetAppBar }}>
      {children}
    </AppBarContext.Provider>
  );
}

// eslint-disable-next-line react-refresh/only-export-components
export const useAppBar = () => useContext(AppBarContext);
