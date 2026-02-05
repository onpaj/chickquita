import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';

import csTranslation from '../locales/cs/translation.json';
import enTranslation from '../locales/en/translation.json';

const resources = {
  cs: {
    translation: csTranslation,
  },
  en: {
    translation: enTranslation,
  },
};

i18n
  // Detect user language
  // Learn more: https://github.com/i18next/i18next-browser-languageDetector
  .use(LanguageDetector)
  // Pass the i18n instance to react-i18next
  .use(initReactI18next)
  // Initialize i18next
  // For all options read: https://www.i18next.com/overview/configuration-options
  .init({
    resources,
    fallbackLng: 'cs', // Czech is the default fallback language
    debug: import.meta.env.DEV, // Enable debug mode in development

    interpolation: {
      escapeValue: false, // React already escapes values to prevent XSS
    },

    detection: {
      // Order of language detection methods
      order: ['localStorage', 'navigator'],
      // Cache user language preference
      caches: ['localStorage'],
      // localStorage key name
      lookupLocalStorage: 'i18nextLng',
    },

    // Default language (Czech)
    lng: 'cs',

    // Supported languages
    supportedLngs: ['cs', 'en'],

    // Namespace
    ns: ['translation'],
    defaultNS: 'translation',
  });

export default i18n;
