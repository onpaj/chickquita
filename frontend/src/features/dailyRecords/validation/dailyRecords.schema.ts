import { z } from 'zod';

/**
 * Zod validation schema for creating a new daily record.
 * Includes Czech error messages for all validation rules.
 *
 * Validation rules:
 * - flockId: Required UUID
 * - recordDate: Required, ISO 8601 date string, cannot be in future
 * - eggCount: Required, non-negative integer
 * - notes: Optional, max 500 characters
 */
export const createDailyRecordSchema = z.object({
  flockId: z
    .string({ message: 'ID hejna musí být textový řetězec' })
    .min(1, { message: 'ID hejna je povinné' })
    .uuid({ message: 'ID hejna musí být platný UUID formát' }),

  recordDate: z
    .string({ message: 'Datum záznamu musí být textový řetězec' })
    .min(1, { message: 'Datum záznamu je povinné' })
    .regex(/^\d{4}-\d{2}-\d{2}$/, {
      message: 'Datum záznamu musí být ve formátu YYYY-MM-DD',
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
        message: 'Datum záznamu nemůže být v budoucnosti',
      }
    ),

  eggCount: z
    .number({ message: 'Počet vajec musí být číslo' })
    .int({ message: 'Počet vajec musí být celé číslo' })
    .nonnegative({ message: 'Počet vajec nemůže být záporný' }),

  notes: z
    .preprocess(
      (val) => {
        if (val === '' || val === null) return undefined;
        return val;
      },
      z
        .string({ message: 'Poznámky musí být textový řetězec' })
        .max(500, { message: 'Poznámky nesmí překročit 500 znaků' })
        .optional()
    )
});

/**
 * Zod validation schema for updating an existing daily record.
 * Only allows updating eggCount and notes (recordDate and flockId are immutable).
 * Includes Czech error messages for all validation rules.
 *
 * Validation rules:
 * - id: Required UUID
 * - eggCount: Required, non-negative integer
 * - notes: Optional, max 500 characters
 */
export const updateDailyRecordSchema = z.object({
  id: z
    .string({ message: 'ID denního záznamu musí být textový řetězec' })
    .min(1, { message: 'ID denního záznamu je povinné' })
    .uuid({ message: 'ID denního záznamu musí být platný UUID formát' }),

  eggCount: z
    .number({ message: 'Počet vajec musí být číslo' })
    .int({ message: 'Počet vajec musí být celé číslo' })
    .nonnegative({ message: 'Počet vajec nemůže být záporný' }),

  notes: z
    .preprocess(
      (val) => {
        if (val === '' || val === null) return undefined;
        return val;
      },
      z
        .string({ message: 'Poznámky musí být textový řetězec' })
        .max(500, { message: 'Poznámky nesmí překročit 500 znaků' })
        .optional()
    )
});

/**
 * Zod validation schema for querying daily records with filters.
 * All fields are optional to allow flexible filtering.
 * Includes Czech error messages for all validation rules.
 *
 * Validation rules:
 * - flockId: Optional UUID
 * - startDate: Optional, ISO 8601 date string
 * - endDate: Optional, ISO 8601 date string, must be after or equal to startDate
 */
export const getDailyRecordsParamsSchema = z
  .object({
    flockId: z
      .string({ message: 'ID hejna musí být textový řetězec' })
      .uuid({ message: 'ID hejna musí být platný UUID formát' })
      .optional(),

    startDate: z
      .string({ message: 'Datum začátku musí být textový řetězec' })
      .regex(/^\d{4}-\d{2}-\d{2}$/, {
        message: 'Datum začátku musí být ve formátu YYYY-MM-DD',
      })
      .optional(),

    endDate: z
      .string({ message: 'Datum konce musí být textový řetězec' })
      .regex(/^\d{4}-\d{2}-\d{2}$/, {
        message: 'Datum konce musí být ve formátu YYYY-MM-DD',
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
      message: 'Datum konce musí být stejné nebo pozdější než datum začátku',
      path: ['endDate'],
    }
  );

/**
 * Inferred TypeScript types from Zod schemas.
 * These types can be used throughout the application for type-safe validation.
 */
export type CreateDailyRecordInput = z.infer<typeof createDailyRecordSchema>;
export type UpdateDailyRecordInput = z.infer<typeof updateDailyRecordSchema>;
export type GetDailyRecordsParamsInput = z.infer<typeof getDailyRecordsParamsSchema>;
