import { z } from 'zod';
import type { TFunction } from 'i18next';

/**
 * Factory function to create Zod validation schema for creating a new daily record.
 * Uses i18n translation function for localized error messages.
 *
 * Validation rules:
 * - flockId: Required UUID
 * - recordDate: Required, ISO 8601 date string, cannot be in future
 * - eggCount: Required, non-negative integer
 * - notes: Optional, max 500 characters
 *
 * @param t - i18next translation function
 * @returns Zod schema with localized error messages
 */
export const createDailyRecordSchemaFactory = (t: TFunction) =>
  z.object({
    flockId: z
      .string({ message: t('dailyRecords.validation.flockIdString') })
      .min(1, { message: t('dailyRecords.validation.flockIdRequired') })
      .uuid({ message: t('dailyRecords.validation.flockIdUuid') }),

    recordDate: z
      .string({ message: t('dailyRecords.validation.recordDateString') })
      .min(1, { message: t('dailyRecords.validation.recordDateRequired') })
      .regex(/^\d{4}-\d{2}-\d{2}$/, {
        message: t('dailyRecords.validation.recordDateFormat'),
      })
      .refine(
        (date) => {
          // Parse the date string as YYYY-MM-DD (treats as local date)
          const [year, month, day] = date.split('-').map(Number);
          const recordDate = new Date(year, month - 1, day);
          const today = new Date();
          today.setHours(0, 0, 0, 0);
          return recordDate <= today;
        },
        {
          message: t('dailyRecords.validation.recordDateFuture'),
        }
      ),

    eggCount: z
      .number({ message: t('dailyRecords.validation.eggCountNumber') })
      .int({ message: t('dailyRecords.validation.eggCountInteger') })
      .nonnegative({ message: t('dailyRecords.validation.eggCountNonnegative') }),

    notes: z
      .preprocess(
        (val) => {
          if (val === '' || val === null) return undefined;
          return val;
        },
        z
          .string({ message: t('dailyRecords.validation.notesString') })
          .max(500, { message: t('dailyRecords.validation.notesMaxLength') })
          .optional()
      ),
  });

/**
 * Factory function to create Zod validation schema for updating an existing daily record.
 * Only allows updating eggCount and notes (recordDate and flockId are immutable).
 * Uses i18n translation function for localized error messages.
 *
 * Validation rules:
 * - id: Required UUID
 * - eggCount: Required, non-negative integer
 * - notes: Optional, max 500 characters
 *
 * @param t - i18next translation function
 * @returns Zod schema with localized error messages
 */
export const updateDailyRecordSchemaFactory = (t: TFunction) =>
  z.object({
    id: z
      .string({ message: t('dailyRecords.validation.recordIdString') })
      .min(1, { message: t('dailyRecords.validation.recordIdRequired') })
      .uuid({ message: t('dailyRecords.validation.recordIdUuid') }),

    eggCount: z
      .number({ message: t('dailyRecords.validation.eggCountNumber') })
      .int({ message: t('dailyRecords.validation.eggCountInteger') })
      .nonnegative({ message: t('dailyRecords.validation.eggCountNonnegative') }),

    notes: z
      .preprocess(
        (val) => {
          if (val === '' || val === null) return undefined;
          return val;
        },
        z
          .string({ message: t('dailyRecords.validation.notesString') })
          .max(500, { message: t('dailyRecords.validation.notesMaxLength') })
          .optional()
      ),
  });

/**
 * Factory function to create Zod validation schema for querying daily records with filters.
 * All fields are optional to allow flexible filtering.
 * Uses i18n translation function for localized error messages.
 *
 * Validation rules:
 * - flockId: Optional UUID
 * - startDate: Optional, ISO 8601 date string
 * - endDate: Optional, ISO 8601 date string, must be after or equal to startDate
 *
 * @param t - i18next translation function
 * @returns Zod schema with localized error messages
 */
export const getDailyRecordsParamsSchemaFactory = (t: TFunction) =>
  z
    .object({
      flockId: z
        .string({ message: t('dailyRecords.validation.flockIdString') })
        .uuid({ message: t('dailyRecords.validation.flockIdUuid') })
        .optional(),

      startDate: z
        .string({ message: t('dailyRecords.validation.startDateString') })
        .regex(/^\d{4}-\d{2}-\d{2}$/, {
          message: t('dailyRecords.validation.startDateFormat'),
        })
        .optional(),

      endDate: z
        .string({ message: t('dailyRecords.validation.endDateString') })
        .regex(/^\d{4}-\d{2}-\d{2}$/, {
          message: t('dailyRecords.validation.endDateFormat'),
        })
        .optional(),
    })
    .refine(
      (data) => {
        if (data.startDate && data.endDate) {
          const start = new Date(data.startDate);
          const end = new Date(data.endDate);
          return start <= end;
        }
        return true;
      },
      {
        message: t('dailyRecords.validation.endDateAfterStart'),
        path: ['endDate'],
      }
    );

// Default schemas with English messages for backwards compatibility and type inference
// These are used primarily for TypeScript type inference and tests
export const createDailyRecordSchema = z.object({
  flockId: z.string().min(1).uuid(),
  recordDate: z
    .string()
    .min(1)
    .regex(/^\d{4}-\d{2}-\d{2}$/)
    .refine((date) => {
      const [year, month, day] = date.split('-').map(Number);
      const recordDate = new Date(year, month - 1, day);
      const today = new Date();
      today.setHours(0, 0, 0, 0);
      return recordDate <= today;
    }),
  eggCount: z.number().int().nonnegative(),
  notes: z.preprocess(
    (val) => (val === '' || val === null ? undefined : val),
    z.string().max(500).optional()
  ),
});

export const updateDailyRecordSchema = z.object({
  id: z.string().min(1).uuid(),
  eggCount: z.number().int().nonnegative(),
  notes: z.preprocess(
    (val) => (val === '' || val === null ? undefined : val),
    z.string().max(500).optional()
  ),
});

export const getDailyRecordsParamsSchema = z
  .object({
    flockId: z.string().uuid().optional(),
    startDate: z.string().regex(/^\d{4}-\d{2}-\d{2}$/).optional(),
    endDate: z.string().regex(/^\d{4}-\d{2}-\d{2}$/).optional(),
  })
  .refine(
    (data) => {
      if (data.startDate && data.endDate) {
        const start = new Date(data.startDate);
        const end = new Date(data.endDate);
        return start <= end;
      }
      return true;
    },
    { path: ['endDate'] }
  );

/**
 * Inferred TypeScript types from Zod schemas.
 * These types can be used throughout the application for type-safe validation.
 */
export type CreateDailyRecordInput = z.infer<typeof createDailyRecordSchema>;
export type UpdateDailyRecordInput = z.infer<typeof updateDailyRecordSchema>;
export type GetDailyRecordsParamsInput = z.infer<typeof getDailyRecordsParamsSchema>;
