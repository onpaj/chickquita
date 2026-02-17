import { describe, it, expect } from 'vitest';
import { formatCzechCount } from '../czechPlurals';

describe('formatCzechCount', () => {
  it('uses singular form for count === 1', () => {
    expect(formatCzechCount(1, 'kohout', 'kohouti', 'kohoutů')).toBe('1 kohout');
    expect(formatCzechCount(1, 'slepice', 'slepice', 'slepic')).toBe('1 slepice');
    expect(formatCzechCount(1, 'kuře', 'kuřata', 'kuřat')).toBe('1 kuře');
  });

  it('uses paucal form for counts 2–4', () => {
    expect(formatCzechCount(2, 'kohout', 'kohouti', 'kohoutů')).toBe('2 kohouti');
    expect(formatCzechCount(3, 'kohout', 'kohouti', 'kohoutů')).toBe('3 kohouti');
    expect(formatCzechCount(4, 'kohout', 'kohouti', 'kohoutů')).toBe('4 kohouti');
    expect(formatCzechCount(2, 'slepice', 'slepice', 'slepic')).toBe('2 slepice');
    expect(formatCzechCount(2, 'kuře', 'kuřata', 'kuřat')).toBe('2 kuřata');
  });

  it('uses genitive form for count === 0', () => {
    expect(formatCzechCount(0, 'kohout', 'kohouti', 'kohoutů')).toBe('0 kohoutů');
    expect(formatCzechCount(0, 'slepice', 'slepice', 'slepic')).toBe('0 slepic');
    expect(formatCzechCount(0, 'kuře', 'kuřata', 'kuřat')).toBe('0 kuřat');
  });

  it('uses genitive form for counts >= 5', () => {
    expect(formatCzechCount(5, 'kohout', 'kohouti', 'kohoutů')).toBe('5 kohoutů');
    expect(formatCzechCount(10, 'slepice', 'slepice', 'slepic')).toBe('10 slepic');
    expect(formatCzechCount(100, 'kuře', 'kuřata', 'kuřat')).toBe('100 kuřat');
  });
});
