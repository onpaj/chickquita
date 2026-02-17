/**
 * Formats a count with the correct Czech grammatical form.
 *
 * Czech has three plural forms:
 * - singular: exactly 1
 * - paucal (few): 2–4
 * - genitive (many): 0 or 5+
 *
 * @example
 * formatCzechCount(1, 'slepice', 'slepice', 'slepic') // "1 slepice"
 * formatCzechCount(2, 'kohout', 'kohouti', 'kohoutů') // "2 kohouti"
 * formatCzechCount(5, 'kohout', 'kohouti', 'kohoutů') // "5 kohoutů"
 * formatCzechCount(0, 'kuře', 'kuřata', 'kuřat')      // "0 kuřat"
 */
export function formatCzechCount(
  count: number,
  singular: string,
  paucal: string,
  genitive: string
): string {
  if (count === 1) {
    return `${count} ${singular}`;
  } else if (count >= 2 && count <= 4) {
    return `${count} ${paucal}`;
  } else {
    return `${count} ${genitive}`;
  }
}
