# Laporan Perubahan Backend — `BE-FIN-090`

> **Catatan penamaan berkas.** Mengikuti konvensi yang sudah berlaku pada 18 laporan backend
> modul ini (huruf besar, tanpa judul ringkas) — lihat catatan serupa pada `BE-FIN-086.md`.

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-090` |
| Judul | Periode yang belum punya rekap kas harian dinyatakan sebagai keadaan, bukan dijawab angka nol |
| Slice | `REV-16B` — amandemen pasca-approval revisi 16 |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian `REV-16B` |
| Trace | `FR-FIN-190`; `FIN-DEC-152`; `FIN-DES-098` |
| Contract version | `FIN-API-1.7` G.4 — **PERUBAHAN MEMUTUS** |
| Dependency | `BE-FIN-067` — **terpenuhi**: `CalculateCashVarianceAsync` sudah ada di source sejak task itu |
| Klasifikasi | `MEDIUM` — perubahan tipe pada DTO respons publik (`decimal` → `decimal?`), satu ruas baru, logika kalkulasi diubah. Nol migration — ruas DTO, bukan kolom basis data |
| Task mode | `BACKEND` — diberikan eksplisit pengguna 4 Oktober 2026 |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/AccountingIntegration/` |
| Tanggal | 4 Oktober 2026 |
| Status | 🟡 **Source selesai.** `dotnet build` **NOT RUN** (instruksi pengguna: tanpa build otomatis). Nol migration, nol database. Uji manual `K.5.1`..`K.5.6` **belum dilaporkan** |

---

## 0. Urutan rilis yang dipakai — wajib dicatat sesuai DoD task ini

Kontrak `FIN-API-1.7` G.4 menetapkan task ini **PERUBAHAN MEMUTUS**, tetapi urutan rilisnya
**berbeda** dari perubahan memutus revisi 14 (`FIN-API-1.5` F.8) yang menuntut layar dirilis
lebih dulu. Di sini: **backend boleh rilis lebih dulu**.

**Kenapa boleh.** Satu-satunya konsumen di dalam repository — layar Snapshot Saldo Subledger
(`FE-FIN-029`, `src/utils/finance/monitoring/subledger-snapshot-utils.jsx`) — **sudah** tahan
nilai kosong, dan **sudah** benar secara struktural sebelum task ini dikerjakan:

| Pembacaan frontend yang diverifikasi | Hasilnya |
| --- | --- |
| `resolveCashVarianceState` menyimpulkan "ada rekap" dari `dailyCashSnapshotDate`, **bukan** dari `dailyCashClosingBalance` | Logika gating tidak tersentuh perubahan tipe ruas lain |
| `cash-variance-section.jsx` hanya memanggil `formatMoney(data.dailyCashClosingBalance)` ketika `state.hasDailyRecap` bernilai benar | Nilai `null` yang baru **tidak pernah dibaca** sebagai angka pada keadaan tanpa rekap |
| `toNumber(null)` pada util frontend memulangkan `0` tanpa melempar galat (`Number(null)` = `0`, finite) | Bahkan bila suatu saat terbaca, tidak crash |

**Simpulan:** layar lama **tidak rusak** oleh perubahan ini — ia sudah menyimpulkan keadaan dari
`dailyCashSnapshotDate` yang memang sudah `null` sejak sebelum task ini. Penyesuaian
`FE-FIN-036` (belum dikerjakan) berupa **berhenti menyimpulkan** dan **mulai membaca**
`HasDailyCashSnapshot` secara eksplisit — perbaikan ketepatan, bukan perbaikan kerusakan.

---

## 1. Perubahan

### 1.1 Cacat yang diperbaiki

Sebelum task ini: `dailyCashClosingBalance = lastDailySnapshot?.ClosingBalance ?? 0.00m` —
periode **tanpa** rekap kas harian diam-diam diisi `0`. Akibatnya `varianceAmount = 0 - posisi`
hampir selalu bukan nol, sehingga `HasVariance` bernilai `true` **justru ketika sebenarnya tidak
ada data untuk dibandingkan** — kebalikan dari maknanya.

### 1.2 Source

| Berkas | Perubahan |
| --- | --- |
| `DTOs/SubledgerPositionDtos.cs` | `CashVarianceResponse.DailyCashClosingBalance`: `decimal` → `decimal?`. `VarianceAmount`: `decimal` → `decimal?`. `HasDailyCashSnapshot` (`bool`) ditambah |
| `Services/FinanceSubledgerBalanceCalculator.cs` | `CalculateCashVarianceAsync`: `hasDailyCashSnapshot` dihitung eksplisit dari `lastDailySnapshot != null`; `dailyCashClosingBalance` **tidak lagi** memakai `?? 0.00m`; `varianceAmount` dihitung hanya bila `hasDailyCashSnapshot`, selainnya `null`; `HasVariance` diberi penjaga `hasDailyCashSnapshot &&` di depan pemeriksaan selisih |

### 1.3 Kenapa `HasVariance` butuh penjaga eksplisit, bukan hanya mengandalkan null-propagation

`varianceAmount != 0.00m` pada `decimal?` memakai operator perbandingan terangkat C#: `null !=
0.00m` bernilai **`true`** (nilai `null` tidak pernah sama dengan angka apa pun). Tanpa penjaga
`hasDailyCashSnapshot &&` di depannya, `HasVariance` akan kembali bernilai `true` saat rekap
tidak ada — persis cacat yang task ini perbaiki, hanya berpindah tempat. Penjaga eksplisit
menjaga arti `HasVariance` tetap benar terlepas dari detail perilaku null-propagation.

### 1.4 Ruas yang **tidak** disentuh, dan kenapa penting dicatat

`CalculatedCashPosition` dan `ExplainingMovements` **tidak diubah logikanya** — keduanya
dihitung dari buku mutasi kas yang sudah berjalan, independen dari ada/tidaknya rekap harian.
Task ini **tidak** membuatnya ikut kosong ketika rekap tidak ada; posisi kas terhitung dan
mutasi penjelas tetap terkirim apa adanya (`K.5.3`, `K.5.4`).

---

## 2. Validasi

| Pemeriksaan | Status | Keterangan |
| --- | :--: | --- |
| `dotnet build` | `NOT RUN` | Instruksi eksplisit pengguna pada giliran ini. **Belum terkompilasi** |
| Pembacaan statis — kurung kurawal seimbang | `PASS` | Service 33=33; DTO 44=44 |
| Pembacaan statis — `ClosingBalance` adalah `decimal` non-nullable pada entity, sehingga `?.ClosingBalance` otomatis terangkat menjadi `decimal?` | `PASS` | `FinDailyCashSnapshot.cs` baris 36 |
| Pembacaan statis — nol konsumen lain `CashVarianceResponse` di dalam repository | `PASS` | Hanya `FinanceAccountingEventsController.cs`, yang meneruskan DTO apa adanya tanpa mengakses ruasnya |
| Pembacaan statis — kelas `CashVarianceResponse` **lain** (BillingManagement/Cashier) **tidak** tersentuh | `PASS` | Nama kelas sama, namespace berbeda; diverifikasi berkas dan isinya terpisah total |
| Pembacaan statis — konsumen frontend tahan nilai kosong | `PASS` | §0 di atas |
| Uji manual `K.5.1`..`K.5.6` | `BELUM DILAPORKAN` | Butuh build dan data rekap kas harian pada beberapa periode |

---

## 3. Acceptance Criteria

| No | Kriteria | Status | Bukti |
|:--:|---|:--:|---|
| 1 | Periode tanpa rekap: `DailyCashClosingBalance` dan `VarianceAmount` kosong (**bukan** `0`) | Terpenuhi (source); belum dikompilasi | §1.2 |
| 2 | Periode tanpa rekap: `HasVariance` salah | Terpenuhi (source) | §1.3 |
| 3 | Periode tanpa rekap: `HasDailyCashSnapshot` salah | Terpenuhi (source) | `hasDailyCashSnapshot = lastDailySnapshot != null` |
| 4 | Posisi kas terhitung dan mutasi penjelas **tetap** dikirim pada keadaan tanpa rekap | Terpenuhi (source) | §1.4 |
| 5 | Periode dengan rekap, angka sama: selisih `0`, `HasVariance` salah | Terpenuhi (source) | `varianceAmount = closing - calculated` = `0` ketika sama; `hasDailyCashSnapshot && (0 != 0)` = salah |
| 6 | Periode dengan rekap, angka berbeda: kedua angka terkirim, `HasVariance` benar | Terpenuhi (source) | `hasDailyCashSnapshot && (selisih != 0)` = benar |

---

## 4. Risiko dan Hal yang Harus Diketahui

1. **Belum dikompilasi.** Risiko paling mungkin: galat tipe pada ternary
   `hasDailyCashSnapshot ? dailyCashClosingBalance - calculatedCashPosition : null` bila
   compiler tidak menyimpulkan `decimal?` dengan benar dari cabang `null`. Perlu `dotnet build`
   sebelum dipercaya.
2. **Perubahan memutus pada kontrak publik.** Pemanggil eksternal (bila ada di luar repository
   yang tidak dapat diperiksa dari sini) yang membaca `DailyCashClosingBalance`/`VarianceAmount`
   sebagai `decimal` non-nullable berpotensi gagal deserialisasi. **Di dalam** repository ini
   terverifikasi nol konsumen demikian (§2).
3. **Dua kelas bernama `CashVarianceResponse` di codebase.** Ditemukan saat audit — satu milik
   Finance (`SubledgerPositionDtos.cs`, disentuh task ini), satu milik Cashier Shift
   (`CashierShiftDtos.cs`, konsep "selisih kas shift" yang berbeda, **tidak** disentuh). Dicatat
   di sini supaya task berikutnya tidak keliru mengira keduanya sama.
4. **Tidak ada uji otomatis ditambahkan.** Sesuai `rules/backend/TEST_POLICY.md` — bukan
   coverage gap.

---

## 5. Status Git (backend)

```text
 M Areas/Corporate/FinanceManagement/AccountingIntegration/DTOs/SubledgerPositionDtos.cs
 M Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceSubledgerBalanceCalculator.cs
```

(Perubahan lain yang belum di-commit berasal dari task sebelumnya, termasuk `BE-FIN-089` pada
berkas `AccountingIntegration` yang sama.) Tidak ada stage, commit, push, pull, merge, rebase,
atau deploy.

---

## 6. Dampak ke Frontend

`FE-FIN-036` (REV-16 frontend, belum dikerjakan) menyesuaikan `FE-FIN-029` agar membaca
`HasDailyCashSnapshot` secara eksplisit, menggantikan kesimpulan dari `DailyCashSnapshotDate`
yang kosong. **Bukan** perbaikan cacat — perilaku yang dilihat pengguna tidak berubah, hanya
dasar kesimpulannya berpindah dari terkaan ke penanda resmi (§0 di atas sudah membuktikan layar
lama tetap benar tanpa penyesuaian ini).

---

## 7. Yang Belum Dikerjakan

| Hal | Keadaan |
| --- | --- |
| `dotnet build` | **NOT RUN** — instruksi pengguna |
| Uji manual `K.5.1`..`K.5.6` | **BELUM DILAPORKAN** |
| `contracts/api-contract.md` G.4 | Sudah menyebut "bentuk response BERUBAH MEMUTUS" sejak revisi 16 disetujui — **tidak perlu** perubahan lagi, hanya menunggu label "Tersedia" sesudah build dan uji manual |
| `roadmap/01-backend-roadmap.md` | Baris `BE-FIN-090` **MUST** diperbarui: ⬜ → 🟡 |

### Langkah yang perlu pengguna jalankan

1. `dotnet build` — pastikan perubahan terkompilasi, termasuk pemeriksaan tipe `decimal?`.
2. Uji manual: panggil `GET /subledger-balances/{period}/variance` untuk (a) periode **tanpa**
   `FinDailyCashSnapshot` — harus `DailyCashClosingBalance: null`, `VarianceAmount: null`,
   `HasVariance: false`, `HasDailyCashSnapshot: false`, sementara `CalculatedCashPosition` dan
   `ExplainingMovements` tetap berisi; (b) periode **dengan** rekap dan angka sama; (c) periode
   **dengan** rekap dan angka berbeda.
3. Setelah itu task dapat ditandai ✅ pada roadmap.
