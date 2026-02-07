import { describe, it, expect } from 'vitest';
import {
  createDailyRecordSchema,
  updateDailyRecordSchema,
  getDailyRecordsParamsSchema,
} from '../dailyRecords.schema';

describe('createDailyRecordSchema', () => {
  describe('flockId validation', () => {
    it('should accept valid UUID', () => {
      const validData = {
        flockId: '123e4567-e89b-12d3-a456-426614174000',
        recordDate: '2025-02-07',
        eggCount: 10,
      };
      const result = createDailyRecordSchema.safeParse(validData);
      expect(result.success).toBe(true);
    });

    it('should reject missing flockId', () => {
      const invalidData = {
        recordDate: '2025-02-07',
        eggCount: 10,
      };
      const result = createDailyRecordSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('ID hejna musí být textový řetězec');
      }
    });

    it('should reject invalid UUID format', () => {
      const invalidData = {
        flockId: 'not-a-uuid',
        recordDate: '2025-02-07',
        eggCount: 10,
      };
      const result = createDailyRecordSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('ID hejna musí být platný UUID formát');
      }
    });

    it('should reject non-string flockId', () => {
      const invalidData = {
        flockId: 12345 as any,
        recordDate: '2025-02-07',
        eggCount: 10,
      };
      const result = createDailyRecordSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('ID hejna musí být textový řetězec');
      }
    });
  });

  describe('recordDate validation', () => {
    it('should accept today\'s date', () => {
      const today = new Date().toISOString().split('T')[0];
      const validData = {
        flockId: '123e4567-e89b-12d3-a456-426614174000',
        recordDate: today,
        eggCount: 10,
      };
      const result = createDailyRecordSchema.safeParse(validData);
      expect(result.success).toBe(true);
    });

    it('should accept past date', () => {
      const validData = {
        flockId: '123e4567-e89b-12d3-a456-426614174000',
        recordDate: '2025-01-01',
        eggCount: 10,
      };
      const result = createDailyRecordSchema.safeParse(validData);
      expect(result.success).toBe(true);
    });

    it('should reject future date', () => {
      const futureDate = new Date();
      futureDate.setDate(futureDate.getDate() + 1);
      const futureDateString = futureDate.toISOString().split('T')[0];

      const invalidData = {
        flockId: '123e4567-e89b-12d3-a456-426614174000',
        recordDate: futureDateString,
        eggCount: 10,
      };
      const result = createDailyRecordSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('Datum záznamu nemůže být v budoucnosti');
      }
    });

    it('should reject missing recordDate', () => {
      const invalidData = {
        flockId: '123e4567-e89b-12d3-a456-426614174000',
        eggCount: 10,
      };
      const result = createDailyRecordSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('Datum záznamu musí být textový řetězec');
      }
    });

    it('should reject invalid date format', () => {
      const invalidData = {
        flockId: '123e4567-e89b-12d3-a456-426614174000',
        recordDate: '07/02/2026', // Invalid format
        eggCount: 10,
      };
      const result = createDailyRecordSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('Datum záznamu musí být ve formátu YYYY-MM-DD');
      }
    });

    it('should reject non-string recordDate', () => {
      const invalidData = {
        flockId: '123e4567-e89b-12d3-a456-426614174000',
        recordDate: 20260207 as any,
        eggCount: 10,
      };
      const result = createDailyRecordSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('Datum záznamu musí být textový řetězec');
      }
    });
  });

  describe('eggCount validation', () => {
    it('should accept zero eggs', () => {
      const validData = {
        flockId: '123e4567-e89b-12d3-a456-426614174000',
        recordDate: '2025-02-07',
        eggCount: 0,
      };
      const result = createDailyRecordSchema.safeParse(validData);
      expect(result.success).toBe(true);
    });

    it('should accept positive egg count', () => {
      const validData = {
        flockId: '123e4567-e89b-12d3-a456-426614174000',
        recordDate: '2025-02-07',
        eggCount: 25,
      };
      const result = createDailyRecordSchema.safeParse(validData);
      expect(result.success).toBe(true);
    });

    it('should reject negative egg count', () => {
      const invalidData = {
        flockId: '123e4567-e89b-12d3-a456-426614174000',
        recordDate: '2025-02-07',
        eggCount: -5,
      };
      const result = createDailyRecordSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('Počet vajec nemůže být záporný');
      }
    });

    it('should reject decimal egg count', () => {
      const invalidData = {
        flockId: '123e4567-e89b-12d3-a456-426614174000',
        recordDate: '2025-02-07',
        eggCount: 10.5,
      };
      const result = createDailyRecordSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('Počet vajec musí být celé číslo');
      }
    });

    it('should reject missing eggCount', () => {
      const invalidData = {
        flockId: '123e4567-e89b-12d3-a456-426614174000',
        recordDate: '2025-02-07',
      };
      const result = createDailyRecordSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('Počet vajec musí být číslo');
      }
    });

    it('should reject non-number eggCount', () => {
      const invalidData = {
        flockId: '123e4567-e89b-12d3-a456-426614174000',
        recordDate: '2025-02-07',
        eggCount: '10' as any,
      };
      const result = createDailyRecordSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('Počet vajec musí být číslo');
      }
    });
  });

  describe('notes validation', () => {
    it('should accept undefined notes', () => {
      const validData = {
        flockId: '123e4567-e89b-12d3-a456-426614174000',
        recordDate: '2025-02-07',
        eggCount: 10,
      };
      const result = createDailyRecordSchema.safeParse(validData);
      expect(result.success).toBe(true);
    });

    it('should accept null notes and transform to undefined', () => {
      const validData = {
        flockId: '123e4567-e89b-12d3-a456-426614174000',
        recordDate: '2025-02-07',
        eggCount: 10,
        notes: null,
      };
      const result = createDailyRecordSchema.safeParse(validData);
      expect(result.success).toBe(true);
      if (result.success) {
        expect(result.data.notes).toBeUndefined();
      }
    });

    it('should accept empty string and transform to undefined', () => {
      const validData = {
        flockId: '123e4567-e89b-12d3-a456-426614174000',
        recordDate: '2025-02-07',
        eggCount: 10,
        notes: '',
      };
      const result = createDailyRecordSchema.safeParse(validData);
      expect(result.success).toBe(true);
      if (result.success) {
        expect(result.data.notes).toBeUndefined();
      }
    });

    it('should accept valid notes within character limit', () => {
      const validData = {
        flockId: '123e4567-e89b-12d3-a456-426614174000',
        recordDate: '2025-02-07',
        eggCount: 10,
        notes: 'Some notes about today\'s production',
      };
      const result = createDailyRecordSchema.safeParse(validData);
      expect(result.success).toBe(true);
    });

    it('should reject notes exceeding 500 characters', () => {
      const longNotes = 'a'.repeat(501);
      const invalidData = {
        flockId: '123e4567-e89b-12d3-a456-426614174000',
        recordDate: '2025-02-07',
        eggCount: 10,
        notes: longNotes,
      };
      const result = createDailyRecordSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('Poznámky nesmí překročit 500 znaků');
      }
    });

    it('should accept notes exactly at 500 character limit', () => {
      const maxLengthNotes = 'a'.repeat(500);
      const validData = {
        flockId: '123e4567-e89b-12d3-a456-426614174000',
        recordDate: '2025-02-07',
        eggCount: 10,
        notes: maxLengthNotes,
      };
      const result = createDailyRecordSchema.safeParse(validData);
      expect(result.success).toBe(true);
    });

    it('should reject non-string notes', () => {
      const invalidData = {
        flockId: '123e4567-e89b-12d3-a456-426614174000',
        recordDate: '2025-02-07',
        eggCount: 10,
        notes: 12345 as any,
      };
      const result = createDailyRecordSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('Poznámky musí být textový řetězec');
      }
    });
  });
});

describe('updateDailyRecordSchema', () => {
  describe('id validation', () => {
    it('should accept valid UUID', () => {
      const validData = {
        id: '123e4567-e89b-12d3-a456-426614174000',
        eggCount: 10,
      };
      const result = updateDailyRecordSchema.safeParse(validData);
      expect(result.success).toBe(true);
    });

    it('should reject missing id', () => {
      const invalidData = {
        eggCount: 10,
      };
      const result = updateDailyRecordSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('ID denního záznamu musí být textový řetězec');
      }
    });

    it('should reject invalid UUID format', () => {
      const invalidData = {
        id: 'not-a-uuid',
        eggCount: 10,
      };
      const result = updateDailyRecordSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('ID denního záznamu musí být platný UUID formát');
      }
    });

    it('should reject non-string id', () => {
      const invalidData = {
        id: 12345 as any,
        eggCount: 10,
      };
      const result = updateDailyRecordSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('ID denního záznamu musí být textový řetězec');
      }
    });
  });

  describe('eggCount validation', () => {
    it('should accept zero eggs', () => {
      const validData = {
        id: '123e4567-e89b-12d3-a456-426614174000',
        eggCount: 0,
      };
      const result = updateDailyRecordSchema.safeParse(validData);
      expect(result.success).toBe(true);
    });

    it('should accept positive egg count', () => {
      const validData = {
        id: '123e4567-e89b-12d3-a456-426614174000',
        eggCount: 25,
      };
      const result = updateDailyRecordSchema.safeParse(validData);
      expect(result.success).toBe(true);
    });

    it('should reject negative egg count', () => {
      const invalidData = {
        id: '123e4567-e89b-12d3-a456-426614174000',
        eggCount: -5,
      };
      const result = updateDailyRecordSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('Počet vajec nemůže být záporný');
      }
    });

    it('should reject decimal egg count', () => {
      const invalidData = {
        id: '123e4567-e89b-12d3-a456-426614174000',
        eggCount: 10.5,
      };
      const result = updateDailyRecordSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('Počet vajec musí být celé číslo');
      }
    });

    it('should reject missing eggCount', () => {
      const invalidData = {
        id: '123e4567-e89b-12d3-a456-426614174000',
      };
      const result = updateDailyRecordSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('Počet vajec musí být číslo');
      }
    });
  });

  describe('notes validation', () => {
    it('should accept undefined notes', () => {
      const validData = {
        id: '123e4567-e89b-12d3-a456-426614174000',
        eggCount: 10,
      };
      const result = updateDailyRecordSchema.safeParse(validData);
      expect(result.success).toBe(true);
    });

    it('should accept null notes and transform to undefined', () => {
      const validData = {
        id: '123e4567-e89b-12d3-a456-426614174000',
        eggCount: 10,
        notes: null,
      };
      const result = updateDailyRecordSchema.safeParse(validData);
      expect(result.success).toBe(true);
      if (result.success) {
        expect(result.data.notes).toBeUndefined();
      }
    });

    it('should accept empty string and transform to undefined', () => {
      const validData = {
        id: '123e4567-e89b-12d3-a456-426614174000',
        eggCount: 10,
        notes: '',
      };
      const result = updateDailyRecordSchema.safeParse(validData);
      expect(result.success).toBe(true);
      if (result.success) {
        expect(result.data.notes).toBeUndefined();
      }
    });

    it('should accept valid notes within character limit', () => {
      const validData = {
        id: '123e4567-e89b-12d3-a456-426614174000',
        eggCount: 10,
        notes: 'Updated notes',
      };
      const result = updateDailyRecordSchema.safeParse(validData);
      expect(result.success).toBe(true);
    });

    it('should reject notes exceeding 500 characters', () => {
      const longNotes = 'a'.repeat(501);
      const invalidData = {
        id: '123e4567-e89b-12d3-a456-426614174000',
        eggCount: 10,
        notes: longNotes,
      };
      const result = updateDailyRecordSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('Poznámky nesmí překročit 500 znaků');
      }
    });
  });
});

describe('getDailyRecordsParamsSchema', () => {
  describe('flockId validation', () => {
    it('should accept valid UUID', () => {
      const validData = {
        flockId: '123e4567-e89b-12d3-a456-426614174000',
      };
      const result = getDailyRecordsParamsSchema.safeParse(validData);
      expect(result.success).toBe(true);
    });

    it('should accept undefined flockId', () => {
      const validData = {};
      const result = getDailyRecordsParamsSchema.safeParse(validData);
      expect(result.success).toBe(true);
    });

    it('should reject invalid UUID format', () => {
      const invalidData = {
        flockId: 'not-a-uuid',
      };
      const result = getDailyRecordsParamsSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('ID hejna musí být platný UUID formát');
      }
    });

    it('should reject non-string flockId', () => {
      const invalidData = {
        flockId: 12345 as any,
      };
      const result = getDailyRecordsParamsSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('ID hejna musí být textový řetězec');
      }
    });
  });

  describe('date range validation', () => {
    it('should accept valid startDate', () => {
      const validData = {
        startDate: '2026-01-01',
      };
      const result = getDailyRecordsParamsSchema.safeParse(validData);
      expect(result.success).toBe(true);
    });

    it('should accept valid endDate', () => {
      const validData = {
        endDate: '2026-02-28',
      };
      const result = getDailyRecordsParamsSchema.safeParse(validData);
      expect(result.success).toBe(true);
    });

    it('should accept valid date range (startDate <= endDate)', () => {
      const validData = {
        startDate: '2026-01-01',
        endDate: '2026-02-28',
      };
      const result = getDailyRecordsParamsSchema.safeParse(validData);
      expect(result.success).toBe(true);
    });

    it('should accept when startDate equals endDate', () => {
      const validData = {
        startDate: '2026-02-07',
        endDate: '2026-02-07',
      };
      const result = getDailyRecordsParamsSchema.safeParse(validData);
      expect(result.success).toBe(true);
    });

    it('should reject when endDate is before startDate', () => {
      const invalidData = {
        startDate: '2026-02-28',
        endDate: '2026-01-01',
      };
      const result = getDailyRecordsParamsSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('Datum konce musí být stejné nebo pozdější než datum začátku');
        expect(result.error.issues[0].path).toContain('endDate');
      }
    });

    it('should reject invalid startDate format', () => {
      const invalidData = {
        startDate: '01/01/2026',
      };
      const result = getDailyRecordsParamsSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('Datum začátku musí být ve formátu YYYY-MM-DD');
      }
    });

    it('should reject invalid endDate format', () => {
      const invalidData = {
        endDate: '28/02/2026',
      };
      const result = getDailyRecordsParamsSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('Datum konce musí být ve formátu YYYY-MM-DD');
      }
    });

    it('should reject non-string startDate', () => {
      const invalidData = {
        startDate: 20260101 as any,
      };
      const result = getDailyRecordsParamsSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('Datum začátku musí být textový řetězec');
      }
    });

    it('should reject non-string endDate', () => {
      const invalidData = {
        endDate: 20260228 as any,
      };
      const result = getDailyRecordsParamsSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('Datum konce musí být textový řetězec');
      }
    });
  });

  describe('complex validation scenarios', () => {
    it('should accept all valid parameters together', () => {
      const validData = {
        flockId: '123e4567-e89b-12d3-a456-426614174000',
        startDate: '2026-01-01',
        endDate: '2026-02-28',
      };
      const result = getDailyRecordsParamsSchema.safeParse(validData);
      expect(result.success).toBe(true);
    });

    it('should accept empty object (all optional)', () => {
      const validData = {};
      const result = getDailyRecordsParamsSchema.safeParse(validData);
      expect(result.success).toBe(true);
    });

    it('should accept flockId with startDate only', () => {
      const validData = {
        flockId: '123e4567-e89b-12d3-a456-426614174000',
        startDate: '2026-01-01',
      };
      const result = getDailyRecordsParamsSchema.safeParse(validData);
      expect(result.success).toBe(true);
    });

    it('should accept flockId with endDate only', () => {
      const validData = {
        flockId: '123e4567-e89b-12d3-a456-426614174000',
        endDate: '2026-02-28',
      };
      const result = getDailyRecordsParamsSchema.safeParse(validData);
      expect(result.success).toBe(true);
    });
  });
});
