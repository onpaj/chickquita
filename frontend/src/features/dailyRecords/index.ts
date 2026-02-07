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
