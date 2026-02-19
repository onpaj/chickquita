import { describe, it, expect } from 'vitest';
import { isPurchaseDto, PurchaseType, QuantityUnit } from '../purchase.types';
import type { PurchaseDto } from '../purchase.types';

describe('isPurchaseDto', () => {
  const validPurchaseDto: PurchaseDto = {
    id: '123e4567-e89b-12d3-a456-426614174000',
    tenantId: '223e4567-e89b-12d3-a456-426614174001',
    coopId: '323e4567-e89b-12d3-a456-426614174002',
    name: 'Chicken Feed - Premium',
    type: PurchaseType.Feed,
    amount: 450.5,
    quantity: 25,
    unit: QuantityUnit.Kg,
    purchaseDate: '2026-02-07T00:00:00Z',
    consumedDate: '2026-02-10T00:00:00Z',
    notes: 'High quality organic feed',
    createdAt: '2026-02-07T10:30:00Z',
    updatedAt: '2026-02-07T10:30:00Z',
  };

  describe('valid PurchaseDto objects', () => {
    it('should return true for valid PurchaseDto with all properties', () => {
      expect(isPurchaseDto(validPurchaseDto)).toBe(true);
    });

    it('should return true for valid PurchaseDto with null coopId', () => {
      const purchase: PurchaseDto = {
        ...validPurchaseDto,
        coopId: null,
      };
      expect(isPurchaseDto(purchase)).toBe(true);
    });

    it('should return true for valid PurchaseDto with null consumedDate', () => {
      const purchase: PurchaseDto = {
        ...validPurchaseDto,
        consumedDate: null,
      };
      expect(isPurchaseDto(purchase)).toBe(true);
    });

    it('should return true for valid PurchaseDto with null notes', () => {
      const purchase: PurchaseDto = {
        ...validPurchaseDto,
        notes: null,
      };
      expect(isPurchaseDto(purchase)).toBe(true);
    });

    it('should return true for valid PurchaseDto with all nullable fields null', () => {
      const purchase: PurchaseDto = {
        ...validPurchaseDto,
        coopId: null,
        consumedDate: null,
        notes: null,
      };
      expect(isPurchaseDto(purchase)).toBe(true);
    });

    it('should return true for PurchaseDto with PurchaseType.Vitamins', () => {
      const purchase: PurchaseDto = {
        ...validPurchaseDto,
        type: PurchaseType.Vitamins,
      };
      expect(isPurchaseDto(purchase)).toBe(true);
    });

    it('should return true for PurchaseDto with PurchaseType.Bedding', () => {
      const purchase: PurchaseDto = {
        ...validPurchaseDto,
        type: PurchaseType.Bedding,
      };
      expect(isPurchaseDto(purchase)).toBe(true);
    });

    it('should return true for PurchaseDto with PurchaseType.Toys', () => {
      const purchase: PurchaseDto = {
        ...validPurchaseDto,
        type: PurchaseType.Toys,
      };
      expect(isPurchaseDto(purchase)).toBe(true);
    });

    it('should return true for PurchaseDto with PurchaseType.Veterinary', () => {
      const purchase: PurchaseDto = {
        ...validPurchaseDto,
        type: PurchaseType.Veterinary,
      };
      expect(isPurchaseDto(purchase)).toBe(true);
    });

    it('should return true for PurchaseDto with PurchaseType.Other', () => {
      const purchase: PurchaseDto = {
        ...validPurchaseDto,
        type: PurchaseType.Other,
      };
      expect(isPurchaseDto(purchase)).toBe(true);
    });

    it('should return true for PurchaseDto with QuantityUnit.Pcs', () => {
      const purchase: PurchaseDto = {
        ...validPurchaseDto,
        unit: QuantityUnit.Pcs,
      };
      expect(isPurchaseDto(purchase)).toBe(true);
    });

    it('should return true for PurchaseDto with QuantityUnit.L', () => {
      const purchase: PurchaseDto = {
        ...validPurchaseDto,
        unit: QuantityUnit.L,
      };
      expect(isPurchaseDto(purchase)).toBe(true);
    });

    it('should return true for PurchaseDto with QuantityUnit.Package', () => {
      const purchase: PurchaseDto = {
        ...validPurchaseDto,
        unit: QuantityUnit.Package,
      };
      expect(isPurchaseDto(purchase)).toBe(true);
    });

    it('should return true for PurchaseDto with QuantityUnit.Other', () => {
      const purchase: PurchaseDto = {
        ...validPurchaseDto,
        unit: QuantityUnit.Other,
      };
      expect(isPurchaseDto(purchase)).toBe(true);
    });

    it('should return true for PurchaseDto with amount as integer', () => {
      const purchase: PurchaseDto = {
        ...validPurchaseDto,
        amount: 500,
      };
      expect(isPurchaseDto(purchase)).toBe(true);
    });

    it('should return true for PurchaseDto with zero amount', () => {
      const purchase: PurchaseDto = {
        ...validPurchaseDto,
        amount: 0,
      };
      expect(isPurchaseDto(purchase)).toBe(true);
    });

    it('should return true for PurchaseDto with decimal quantity', () => {
      const purchase: PurchaseDto = {
        ...validPurchaseDto,
        quantity: 12.5,
      };
      expect(isPurchaseDto(purchase)).toBe(true);
    });
  });

  describe('invalid objects - null or non-object', () => {
    it('should return false for null', () => {
      expect(isPurchaseDto(null)).toBe(false);
    });

    it('should return false for undefined', () => {
      expect(isPurchaseDto(undefined)).toBe(false);
    });

    it('should return false for string', () => {
      expect(isPurchaseDto('not an object')).toBe(false);
    });

    it('should return false for number', () => {
      expect(isPurchaseDto(123)).toBe(false);
    });

    it('should return false for boolean', () => {
      expect(isPurchaseDto(true)).toBe(false);
    });

    it('should return false for array', () => {
      expect(isPurchaseDto([validPurchaseDto])).toBe(false);
    });
  });

  describe('invalid objects - missing required properties', () => {
    it('should return false when id is missing', () => {
      const { id: _id, ...purchaseWithoutId } = validPurchaseDto;
      expect(isPurchaseDto(purchaseWithoutId)).toBe(false);
    });

    it('should return false when tenantId is missing', () => {
      const { tenantId: _tenantId, ...purchaseWithoutTenantId } = validPurchaseDto;
      expect(isPurchaseDto(purchaseWithoutTenantId)).toBe(false);
    });

    it('should return false when name is missing', () => {
      const { name: _name, ...purchaseWithoutName } = validPurchaseDto;
      expect(isPurchaseDto(purchaseWithoutName)).toBe(false);
    });

    it('should return false when type is missing', () => {
      const { type: _type, ...purchaseWithoutType } = validPurchaseDto;
      expect(isPurchaseDto(purchaseWithoutType)).toBe(false);
    });

    it('should return false when amount is missing', () => {
      const { amount: _amount, ...purchaseWithoutAmount } = validPurchaseDto;
      expect(isPurchaseDto(purchaseWithoutAmount)).toBe(false);
    });

    it('should return false when quantity is missing', () => {
      const { quantity: _quantity, ...purchaseWithoutQuantity } = validPurchaseDto;
      expect(isPurchaseDto(purchaseWithoutQuantity)).toBe(false);
    });

    it('should return false when unit is missing', () => {
      const { unit: _unit, ...purchaseWithoutUnit } = validPurchaseDto;
      expect(isPurchaseDto(purchaseWithoutUnit)).toBe(false);
    });

    it('should return false when purchaseDate is missing', () => {
      const { purchaseDate: _purchaseDate, ...purchaseWithoutPurchaseDate } = validPurchaseDto;
      expect(isPurchaseDto(purchaseWithoutPurchaseDate)).toBe(false);
    });

    it('should return false when createdAt is missing', () => {
      const { createdAt: _createdAt, ...purchaseWithoutCreatedAt } = validPurchaseDto;
      expect(isPurchaseDto(purchaseWithoutCreatedAt)).toBe(false);
    });

    it('should return false when updatedAt is missing', () => {
      const { updatedAt: _updatedAt, ...purchaseWithoutUpdatedAt } = validPurchaseDto;
      expect(isPurchaseDto(purchaseWithoutUpdatedAt)).toBe(false);
    });
  });

  describe('invalid objects - wrong property types', () => {
    it('should return false when id is not a string', () => {
      const purchase = {
        ...validPurchaseDto,
        id: 123,
      };
      expect(isPurchaseDto(purchase)).toBe(false);
    });

    it('should return false when id is empty string', () => {
      const purchase = {
        ...validPurchaseDto,
        id: '',
      };
      expect(isPurchaseDto(purchase)).toBe(false);
    });

    it('should return false when tenantId is not a string', () => {
      const purchase = {
        ...validPurchaseDto,
        tenantId: 123,
      };
      expect(isPurchaseDto(purchase)).toBe(false);
    });

    it('should return false when coopId is not a string or null', () => {
      const purchase = {
        ...validPurchaseDto,
        coopId: 123,
      };
      expect(isPurchaseDto(purchase)).toBe(false);
    });

    it('should return false when name is not a string', () => {
      const purchase = {
        ...validPurchaseDto,
        name: 123,
      };
      expect(isPurchaseDto(purchase)).toBe(false);
    });

    it('should return false when name is empty string', () => {
      const purchase = {
        ...validPurchaseDto,
        name: '',
      };
      expect(isPurchaseDto(purchase)).toBe(false);
    });

    it('should return false when type is not a number', () => {
      const purchase = {
        ...validPurchaseDto,
        type: 'Feed',
      };
      expect(isPurchaseDto(purchase)).toBe(false);
    });

    it('should return false when type is invalid enum value', () => {
      const purchase = {
        ...validPurchaseDto,
        type: 999,
      };
      expect(isPurchaseDto(purchase)).toBe(false);
    });

    it('should return false when amount is not a number', () => {
      const purchase = {
        ...validPurchaseDto,
        amount: '450.5',
      };
      expect(isPurchaseDto(purchase)).toBe(false);
    });

    it('should return false when quantity is not a number', () => {
      const purchase = {
        ...validPurchaseDto,
        quantity: '25',
      };
      expect(isPurchaseDto(purchase)).toBe(false);
    });

    it('should return false when unit is not a number', () => {
      const purchase = {
        ...validPurchaseDto,
        unit: 'Kg',
      };
      expect(isPurchaseDto(purchase)).toBe(false);
    });

    it('should return false when unit is invalid enum value', () => {
      const purchase = {
        ...validPurchaseDto,
        unit: 999,
      };
      expect(isPurchaseDto(purchase)).toBe(false);
    });

    it('should return false when purchaseDate is not a string', () => {
      const purchase = {
        ...validPurchaseDto,
        purchaseDate: new Date(),
      };
      expect(isPurchaseDto(purchase)).toBe(false);
    });

    it('should return false when consumedDate is not a string or null', () => {
      const purchase = {
        ...validPurchaseDto,
        consumedDate: new Date(),
      };
      expect(isPurchaseDto(purchase)).toBe(false);
    });

    it('should return false when notes is not a string or null', () => {
      const purchase = {
        ...validPurchaseDto,
        notes: 123,
      };
      expect(isPurchaseDto(purchase)).toBe(false);
    });

    it('should return false when createdAt is not a string', () => {
      const purchase = {
        ...validPurchaseDto,
        createdAt: new Date(),
      };
      expect(isPurchaseDto(purchase)).toBe(false);
    });

    it('should return false when updatedAt is not a string', () => {
      const purchase = {
        ...validPurchaseDto,
        updatedAt: new Date(),
      };
      expect(isPurchaseDto(purchase)).toBe(false);
    });
  });

  describe('edge cases', () => {
    it('should return false for empty object', () => {
      expect(isPurchaseDto({})).toBe(false);
    });

    it('should return false for object with extra properties only', () => {
      const purchase = {
        extraProp: 'value',
      };
      expect(isPurchaseDto(purchase)).toBe(false);
    });

    it('should return true for valid PurchaseDto with extra properties', () => {
      const purchase = {
        ...validPurchaseDto,
        extraProp: 'value',
      };
      expect(isPurchaseDto(purchase)).toBe(true);
    });

    it('should return false when id is null', () => {
      const purchase = {
        ...validPurchaseDto,
        id: null,
      };
      expect(isPurchaseDto(purchase)).toBe(false);
    });

    it('should return false when tenantId is null', () => {
      const purchase = {
        ...validPurchaseDto,
        tenantId: null,
      };
      expect(isPurchaseDto(purchase)).toBe(false);
    });

    it('should return false when amount is NaN', () => {
      const purchase = {
        ...validPurchaseDto,
        amount: NaN,
      };
      expect(isPurchaseDto(purchase)).toBe(false);
    });

    it('should return true when amount is Infinity', () => {
      const purchase = {
        ...validPurchaseDto,
        amount: Infinity,
      };
      expect(isPurchaseDto(purchase)).toBe(true);
    });

    it('should return true for PurchaseDto with negative amount', () => {
      const purchase = {
        ...validPurchaseDto,
        amount: -100,
      };
      expect(isPurchaseDto(purchase)).toBe(true);
    });
  });
});
