import { cs, enUS } from 'date-fns/locale';
import { useTranslation } from 'react-i18next';

/**
 * Returns the appropriate date-fns locale based on the current i18n language.
 * Centralizes locale detection to avoid duplication across components.
 */
export function useDateLocale() {
  const { i18n } = useTranslation();
  return i18n.language === 'cs' ? cs : enUS;
}
