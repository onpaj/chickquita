import { createContext, useContext } from 'react';
import type { ReactNode } from 'react';
import { useUserSettings } from '../hooks/useUserSettings';

interface UserSettingsContextValue {
  singleCoopMode: boolean;
  currency: string;
  isLoading: boolean;
}

const UserSettingsContext = createContext<UserSettingsContextValue>({
  singleCoopMode: true,
  currency: 'CZK',
  isLoading: true,
});

export function UserSettingsProvider({ children }: { children: ReactNode }) {
  const { data, isLoading } = useUserSettings();

  return (
    <UserSettingsContext.Provider
      value={{
        singleCoopMode: data?.singleCoopMode ?? true,
        currency: data?.currency ?? 'CZK',
        isLoading,
      }}
    >
      {children}
    </UserSettingsContext.Provider>
  );
}

// eslint-disable-next-line react-refresh/only-export-components
export const useUserSettingsContext = () => useContext(UserSettingsContext);
