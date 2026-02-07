// Export types and API client for daily records feature
export {
  dailyRecordsApi,
  type DailyRecordDto,
  type CreateDailyRecordRequest,
  type UpdateDailyRecordRequest,
  type GetDailyRecordsParams,
} from './api/dailyRecordsApi';

// Export hooks for daily records feature
export {
  useDailyRecords,
  useDailyRecordsByFlock,
  useCreateDailyRecord,
  useUpdateDailyRecord,
  useDeleteDailyRecord,
} from './hooks/useDailyRecords';

// Export validation schemas for daily records feature
export {
  createDailyRecordSchema,
  updateDailyRecordSchema,
  getDailyRecordsParamsSchema,
  type CreateDailyRecordInput,
  type UpdateDailyRecordInput,
  type GetDailyRecordsParamsInput,
} from './validation';
