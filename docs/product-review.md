# Product Review — Chickquita

*Review date: 2026-03-17*

---

## Co aplikace umí

Chickquita je mobilní PWA pro malé/střední chovatele slepic. Jádro produktu funguje dobře:

- Správa kurníků a hejn (včetně složení hejna — slepice, kohouti, kuřata — s historií změn)
- Denní záznamy vajec
- Nákupy/náklady (krmení, vitamíny, veterina…)
- Dashboard se základními metrikami (vejce dnes, tento týden, cena za vejce)
- Grafy produkce a nákladů
- Offline PWA — funguje bez internetu

---

## Co chybí — seřazeno podle dopadu

### 1. Tržby a zisk

Aplikace sleduje náklady ale ne příjmy. Farmář prodá vejce, ale nikde to nezaznamená. Chybí:

- Záznam prodeje vajec (počet, cena za kus, kupující)
- P&L přehled (příjmy − náklady = zisk/ztráta)
- Graf rentability v čase

Tohle je největší slepá skvrna — bez tržeb je "cena za vejce" jen polovina příběhu.

### 2. Míra snášky (laying rate)

Klíčová metrika pro každého chovatele. Vzorec: vejce / počet slepic × 100 %. Ideál je 75–90 %. Data v aplikaci jsou, výpočet a zobrazení chybí. Mohlo by být na detailu hejna i v grafech.

### 3. Upozornění a připomínky

- Push notifikace: "Nezapomněl jsi zadat dnes vejce?" (pokud do 20h chybí záznam)
- Alert: kuřata dosáhla ~20 týdnů → brzy začnou snášet
- Alert: chybějící záznamy za posledních N dní

### 4. Správa zásob

Farmář koupí 50 kg krmiva. Aplikace to zaznamená jako nákup, ale neví, kolik ještě zbývá. Chybí odhad spotřeby a "dochází krmivo" alert.

### 5. Export dat

Nic nelze exportovat. Farmář potřebuje přehled nákladů pro daňové účely — PDF nebo Excel report za zvolené období by byl velice ceněný.

### 6. Zdravotní deník

Teď jsou veterinární náklady jen typ nákupu. Chybí dedikovaný zdravotní log: nemoc, vakcinace, úhyn (a důvod). Úhyn slepice se dá sice zaznamenat přes "změna složení hejna", ale není to explicitní a chybí důvod/diagnóza.

### 7. Srovnání hejn

Grafy jsou agregované za farmu. Neexistuje pohled "hejno A vs hejno B" — která skupina slepic je výnosnější? Které krmivo funguje lépe?

### 8. Roční srovnání

Produkce tento měsíc vs stejný měsíc loni. Sezonní vzorce (slepice snáší méně v zimě) jsou viditelné jen pokud má uživatel data přes rok, ale aplikace je nijak nevyzdvihuje.

### 9. Cílová produkce / plán

Farmář by si mohl nastavit cíl (např. 200 vajec/týden) a aplikace by zobrazovala plnění. Motivační prvek + early warning.

### 10. Breed / rasa hejna

Různé rasy mají různou produktivitu a různé ceny vajec. Teď se hejno identifikuje volným textem (identifier). Číselník ras by umožnil benchmarking a automatické doporučení očekávané míry snášky.

---

## Celkový dojem

Aplikace má solidní základ pro *sledování vstupů* (vejce, náklady). Chybí *výstupová strana* (tržby, zisk) a *prediktivní/alertovací vrstva*. To jsou dva největší skoky v hodnotě pro uživatele. Ostatní body jsou postupná vylepšení UX a analytiky.
