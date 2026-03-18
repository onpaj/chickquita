import { createContext, useContext } from 'react';
import type { ReactNode } from 'react';
import { useUserSettings } from '../hooks/useUserSettings';

interface UserSettingsContextValue {
  singleCoopMode: boolean;
  isLoading: boolean;
}

const UserSettingsContext = createContext<UserSettingsContextValue>({
  singleCoopMode: false,
  isLoading: true,
});

export function UserSettingsProvider({ children }: { children: ReactNode }) {
  const { data, isLoading } = useUserSettings();

  return (
    <UserSettingsContext.Provider
      value={{
        singleCoopMode: data?.singleCoopMode ?? false,
        isLoading,
      }}
    >
      {children}
    </UserSettingsContext.Provider>
  );
}

// eslint-disable-next-line react-refresh/only-export-components
export const useUserSettingsContext = () => useContext(UserSettingsContext);
