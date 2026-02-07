/**
 * Validation schemas for DailyRecords feature.
 * Exports Zod schemas and inferred TypeScript types for client-side form validation.
 */

export {
  createDailyRecordSchema,
  updateDailyRecordSchema,
  getDailyRecordsParamsSchema,
  type CreateDailyRecordInput,
  type UpdateDailyRecordInput,
  type GetDailyRecordsParamsInput,
} from './dailyRecords.schema';
