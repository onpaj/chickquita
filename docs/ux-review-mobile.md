# UX Review — Mobile (Chickquita)

**Datum:** 2026-03-09
**Scope:** Mobilní UX, navigace, information architecture
**Podklad:** Analýza zdrojového kódu (App.tsx, BottomNavigation.tsx, všechny Page komponenty)

---

## Shrnutí

App má solidní základ, ale na mobilu se v navigaci tlačí 6 položek do bottom baru, Settings je dostupný duplicitně ze dvou míst a Daily Records + Statistics jsou zbytečně rozděleny. Níže jsou konkrétní problémy a navrhovaná řešení.

---

## 1. Bottom navigation — příliš mnoho položek

### Problém

`BottomNavigation.tsx` obsahuje **6 položek**:

```
Dashboard | Coops | Daily Records | Statistics | Purchases | Settings
```

MUI `BottomNavigation` je navržen pro 3–5 položek. Na běžném 390px displeji to znamená, že každá ikona má ~65px a label se začíná zkracovat nebo obtékat. Šest položek je nad ergonomickým limitem.

### Navrhované řešení

Snížit na **4 položky** sloučením a přesunem:

```
Dashboard | Records | Purchases | Coops
```

- **Settings** přesunout výhradně pod `AccountCircleIcon` v AppBaru (ten tam už je a naviguje na `/settings` — jen ho zpřístupnit víc, viz bod 3)
- **Daily Records + Statistics** sloučit do jedné položky „Záznamy" s interními taby (viz bod 2)

---

## 2. Daily Records a Statistics — zbytečně oddělené sekce

### Problém

Daily Records a Statistics jsou dvě separátní navigační položky, ale obsah je úzce propojený — Statistics jsou agregace Daily Records. Uživatel mentálně přepíná mezi „zadáváním dat" a „prohlížením výsledků", ale v navigaci musí skákat mezi dvěma sekcemi.

### Navrhované řešení

Sloučit do jedné stránky `/records` se dvěma taby:

```
[ Záznamy ]  [ Statistiky ]
```

- Tab „Záznamy" = dnešní `DailyRecordsListPage`
- Tab „Statistiky" = dnešní `StatisticsPage`
- URL se může měnit na `/records/list` a `/records/stats` pro přímé odkazování

**Výhoda:** Ušetří 1 slot v bottom navu, zlepší kontextuální návaznost — uživatel vidí záznamy a hned přepne na statistiky.

---

## 3. Settings — duplicitní přístupový bod

### Problém

Settings jsou dostupné ze **dvou míst**:
1. Bottom navigation (`SettingsIcon`)
2. `AccountCircleIcon` v AppBaru (App.tsx, řádek ~55)

To je zbytečná redundance a zároveň plýtvání cenným místem v bottom navu.

### Navrhované řešení

- **Odstranit** Settings z bottom navigace
- **Zvýraznit** AccountCircleIcon v AppBaru — přidat tooltip nebo label „Profil", případně zobrazit avatar uživatele místo generické ikony (data jsou dostupná přes `useUser()`)
- Na `SettingsPage` přidat přímý odkaz na Clerk profile management (editace jména, emailu, změna hesla)

---

## 4. Dashboard — Quick Action Cards duplicují navigaci

### Problém

`DashboardPage.tsx` zobrazuje 5 Quick Action Cards:
- Add Daily Record *(FAB to samé)*
- Manage Coops *(= bottom nav Coops)*
- Manage Flocks *(= bottom nav Coops → Flock)*
- Track Purchases *(= bottom nav Purchases)*
- View Statistics *(= bottom nav Records/Statistics)*

Na mobilu toto přidává dlouhý scroll pod stats widgety a nepřináší žádnou přidanou hodnotu, protože vše je dostupné z bottom navu.

### Navrhované řešení

**Zrušit Quick Action Cards** nebo zredukovat pouze na:
- „Přidat denní záznam" card (pokud nemá data → místo FABu)
- Zbytek zahodit

Dashboard by měl být co nejkratší — **4 stat widgety + FAB** jsou dostačující.

---

## 5. FAB (Floating Action Button) — viditelnost a umístění

### Problém

FAB s `+` je na dashboardu zobrazen **pouze pokud `hasData === true`** (tj. existují aktivní hejna). Nový uživatel nebo uživatel bez hejn FAB nevidí vůbec a je odkázán na Quick Action Cards (viz bod 4).

Navíc FAB existuje duplicitně — Dashboard i DailyRecordsListPage ho mají, oba otevírají QuickAddModal.

### Navrhované řešení

- FAB zobrazit vždy, ne pouze při `hasData` — když nejsou hejna, zobrazit disabled stav s tooltipem „Nejprve přidej hejno"
- FAB přidat i do dalších stránek kde má smysl (Purchases stránka pro přidání nákupu)
- Zvážit, zda FAB na `DailyRecordsListPage` a Dashboardu neduplikuje — stačí jeden, pokud `/records` bude jeden screen

---

## 6. Statistics stránka — filtrování na mobilu

### Problém

`StatisticsPage.tsx` zobrazuje filtry (date range toggle + custom date pickers + coop/flock dropdowns) jako vždy rozbalený blok. Na mobilu to tvoří ~200px scrollu před tím, než uživatel vidí první chart.

### Navrhované řešení

- Filtry zabalit do **collapsible sekce** (Accordion nebo expandable Card) s titulkem „Filtry" + ikona `FilterListIcon`
- Výchozí stav: sbaleno s popisem aktuálního filtru (např. „Posledních 30 dní, všechna hejna")
- Rozbalení odkryje detailní filtry

---

## 7. AppBar — chybí kontext na detail stránkách

### Problém

Na všech stránkách (včetně CoopDetailPage, FlockDetailPage) AppBar zobrazuje staticky „Chickquita". CoopDetailPage sice má tlačítko `ArrowBackIcon` uvnitř stránky, ale AppBar neposkytuje kontext o tom, kde uživatel je.

### Navrhované řešení

- Přidat dynamický titulek do AppBaru podle aktuální stránky (přes React Context nebo v `App.tsx` dle `location.pathname`)
- Na detail stránkách zobrazit `ArrowBackIcon` přímo v AppBaru (vlevo), titulek stránky uprostřed — standardní Android/iOS pattern
- Tím lze zrušit duplicitní back buttony uvnitř stránek

---

## 8. Coops → Flocks navigace — 4 úrovně hloubky

### Problém

Navigační strom:
```
Coops → Coop Detail → Flocks → Flock Detail → History
```

To jsou 4–5 úrovní stacku bez jasné breadcrumb navigace. Uživatel se může snadno ztratit.

### Navrhované řešení

- Na `CoopDetailPage` zobrazit hejna přímo inline (expand/collapse sekce), ne jako separátní navigační krok
- Nebo přidat breadcrumb komponentu pod AppBar (malý text: „Kurníky > Kur č. 1 > Hejna > Jarní hejno")
- `FlockHistoryPage` sloučit jako tab na `FlockDetailPage`

---

## Navrhovaná nová struktura bottom navu

| # | Tab | Ikona | Popis |
|---|-----|-------|-------|
| 1 | Dashboard | `DashboardIcon` | Stat widgety + FAB |
| 2 | Záznamy | `AssignmentIcon` | Denní záznamy + Statistiky (taby) |
| 3 | Nákupy | `ShoppingCartIcon` | Purchases |
| 4 | Kurníky | `HomeWorkIcon` | Coops + Flocks |

Settings dostupné výhradně přes AppBar profil ikonu (vpravo nahoře).

---

## Prioritizace

| Priorita | Úprava | Dopad | Složitost |
|----------|--------|-------|-----------|
| 🔴 High | Sloučit Daily Records + Statistics do „Záznamy" | Velký (navigace) | Střední |
| 🔴 High | Odstranit Settings z bottom navu | Velký (prostor) | Nízká |
| 🟡 Medium | Zrušit Quick Action Cards na dashboardu | Střední (čistota) | Nízká |
| 🟡 Medium | Collapsible filtry na Statistics | Střední (usability) | Nízká |
| 🟡 Medium | Dynamický AppBar titulek + back button | Střední (orientace) | Střední |
| 🟢 Low | FAB vždy viditelný (disabled stav) | Nízký | Nízká |
| 🟢 Low | Breadcrumb na Coop/Flock detail | Nízký | Nízká |
