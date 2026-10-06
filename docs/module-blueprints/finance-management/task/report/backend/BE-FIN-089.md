# Laporan Perubahan Backend — `BE-FIN-089`

> **Catatan penamaan berkas.** Mengikuti konvensi yang sudah berlaku pada 17 laporan backend
> modul ini (huruf besar, tanpa judul ringkas) — lihat catatan serupa pada `BE-FIN-086.md`.

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-089` |
| Judul | Layar batch dapat menampilkan siapa yang menyetujui, bukan hanya kapan |
| Slice | `REV-16B` — amandemen pasca-approval revisi 16 |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian `REV-16B` |
| Trace | `FR-FIN-187`; `FIN-DEC-151`; `FIN-DES-095` |
| Contract version | `FIN-API-1.7` G.2 |
| Dependency | `BE-FIN-082` 🟡 — `ApprovedBy` sudah ada pada `FinOpeningItemBatch` dan diisi `ApproveAsync`; **cukup untuk task ini** walau `BE-FIN-082` sendiri belum ✅ |
| Klasifikasi | `MEDIUM` — 3 berkas (DTO, service, controller); 6 titik panggil controller diubah; nol migration |
| Task mode | `BACKEND` — diberikan eksplisit pengguna 4 Oktober 2026 |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/AccountingIntegration/` |
| Tanggal | 4 Oktober 2026 |
| Status | 🟡 **Source selesai.** `dotnet build` **NOT RUN** (instruksi pengguna: tanpa build otomatis). Nol migration, nol database. Uji manual `K.1.4` (pola yang sama) **belum dilaporkan** |

---

## 1. Perubahan

### 1.1 Satu keputusan desain: dua pola pencarian nama, untuk dua bentuk respons berbeda

Task ini menyentuh **dua** bentuk respons dengan kebutuhan berbeda:

| Bentuk respons | Jumlah baris | Pola yang dipakai | Alasan |
| --- | --- | --- | --- |
| `GET /` (daftar berpaging) | Banyak baris sekaligus | **Batch** — satu kueri untuk seluruh nama penyetuju pada halaman itu | Acceptance criteria eksplisit: "daftar berpaging **tidak** menimbulkan satu kueri per baris" |
| `GET /{id}`, `POST /` (unggah), `POST /{id}/reupload`, `POST /{id}/validate`, `POST /{id}/declare-accounting-opening`, `POST /{id}/approve`, `POST /{id}/reject` | Satu baris | **Tunggal** — satu kueri untuk satu pengguna | Respons tunggal; batch dictionary untuk satu ID adalah overhead tanpa manfaat |

Pola batch mengikuti `FinanceOpeningBalanceService.GetUserNamesAsync` (dipakai `BE-FIN-084`). Pola
tunggal mengikuti `DirectPaymentThresholdService.GetUserNameAsync` (`BE-FIN-086`, giliran
sebelumnya). Keduanya disalin persis struktur dan gaya nullability-nya — ini **bukan** pola baru.

### 1.2 Source

| Berkas | Perubahan |
| --- | --- |
| `DTOs/OpeningItemBatchDtos.cs` | `ApprovedByName` (`string?`) ditambah pada `OpeningItemBatchResponse` — otomatis diwarisi `OpeningItemBatchDetailResponse` |
| `Services/FinanceOpeningItemBatchService.cs` | `GetUserNamesAsync` (batch, privat, instance); `GetUserNameAsync` (tunggal, privat, instance); `MapWithName` (sinkron, privat, menyisipkan nama dari kamus yang sudah diambil); `MapWithNameAsync` dan `MapDetailWithNameAsync` (publik, instance, membungkus `Map`/`MapDetail` statis yang sudah ada **tanpa mengubahnya**); `GetPagedAsync` memanggil `GetUserNamesAsync` sekali untuk seluruh item satu halaman sebelum memetakan; `GetByIdAsync` memanggil `MapDetailWithNameAsync` |
| `Controllers/FinanceOpeningItemBatchesController.cs` | 6 titik panggil (`Upload`, `Reupload`, `Validate`, `DeclareAccountingOpening`, `Approve`, `Reject`) diubah dari pemanggilan statis `FinanceOpeningItemBatchService.Map(entity)`/`MapDetail(entity, rows)` menjadi `await _service.MapWithNameAsync(entity, cancellationToken)`/`MapDetailWithNameAsync(...)` |

### 1.3 Kenapa `Map`/`MapDetail` statis TIDAK diubah, melainkan dibungkus

Keduanya dipakai di banyak tempat (6 titik panggil controller, plus internal `MapDetail`
memanggil `Map`). Mengubah tanda tangannya menjadi `async` akan memaksa seluruh pemanggil
berubah sekaligus, termasuk kemungkinan pemanggil lain yang belum diperiksa. Membungkusnya
dengan metode instance baru (`MapWithNameAsync`/`MapDetailWithNameAsync`) adalah perubahan
aditif murni: `Map`/`MapDetail` tetap ada, tetap statis, tetap dapat dipanggil tanpa
`DbContext` oleh kode mana pun yang mungkin bergantung padanya di luar berkas ini.

### 1.4 Kenapa respons mutasi (Upload, Approve, dst.) juga diubah, bukan hanya daftar

`ApprovedBy` **bersama** untuk seluruh bentuk respons (field yang sama di kelas dasar). Bila
hanya daftar yang diisi `ApprovedByName`, respons `POST /{id}/approve` — tepat saat batch
disetujui — akan menunjukkan `ApprovedBy: <guid>, ApprovedByName: null`. Itu justru momen yang
paling penting ditampilkan namanya: petugas baru saja menekan tombol Setuju dan ingin melihat
konfirmasi siapa yang bertindak. Membiarkannya `null` di sana akan membuat task ini terasa
belum selesai pada jalur yang paling sering dipakai.

---

## 2. Validasi

| Pemeriksaan | Status | Keterangan |
| --- | :--: | --- |
| `dotnet build` | `NOT RUN` | Instruksi eksplisit pengguna pada giliran ini. **Belum terkompilasi** |
| Pembacaan statis — kurung kurawal seimbang | `PASS` | Service 141=141; Controller 39=39; DTO 53=53 |
| Pembacaan statis — nol pemanggilan statis `Map`/`MapDetail` tersisa di controller | `PASS` | `rg "FinanceOpeningItemBatchService\." Controllers/FinanceOpeningItemBatchesController.cs` → nol hasil |
| Pembacaan statis — `_dbContext.Users` dan pola seleksi nama sama dengan precedent | `PASS` | Dibandingkan `FinanceOpeningBalanceService.GetUserNamesAsync`, `DirectPaymentThresholdService.GetUserNameAsync` |
| Pembacaan statis — `ValidateAsync` memulangkan tipe yang cocok dengan `MapDetailWithNameAsync` | `PASS` | `Task<(FinOpeningItemBatch Entity, List<OpeningItemBatchRowValidationResult> Rows)>` |
| Pembacaan statis — `GetPagedAsync` memanggil pencarian nama tepat sekali per halaman | `PASS` | `GetUserNamesAsync(items.Select(x => x.ApprovedBy), ...)` dipanggil **sesudah** `ToListAsync`, **sebelum** `.Select(x => MapWithName(x, names))` — satu kueri nama untuk seluruh halaman |
| Uji manual `K.1.4` (pola yang sama) | `BELUM DILAPORKAN` | Butuh build dan data batch yang sudah disetujui |

---

## 3. Acceptance Criteria

| No | Kriteria | Status | Bukti |
|:--:|---|:--:|---|
| 1 | `ApprovedByName` berisi nama tampilan penyetuju | Terpenuhi (source); belum dikompilasi | `GetUserNameAsync`/`GetUserNamesAsync` |
| 2 | `null` bila belum disetujui atau penggunanya tidak ditemukan | Terpenuhi (source) | Pemeriksaan `userId is null \|\| userId == Guid.Empty`; `TryGetValue` kembali `false` untuk ID tak ditemukan |
| 3 | Nama **tidak** disimpan ke tabel Finance | Terpenuhi (source) | `ApprovedByName` hanya ada di DTO respons; nol kolom baru pada `FinOpeningItemBatch` atau configuration-nya |
| 4 | Daftar berpaging **tidak** menimbulkan satu kueri per baris | Terpenuhi (source) | §1.4 tabel validasi baris 6 |

---

## 4. Risiko dan Hal yang Harus Diketahui

1. **Belum dikompilasi.** Risiko paling mungkin: salah tipe pada ekspresi ternary
   `TryGetValue(...) ? name : null`, atau penamaan parameter `CancellationToken` yang tidak
   konsisten dengan metode pemanggil. Perlu `dotnet build` sebelum dipercaya.
2. **Dependency `BE-FIN-082` sendiri belum ✅** (risiko tersisa pada transaksi/advisory lock
   belum diuji berjalan, dicatat laporannya sendiri). Task ini **tidak** menambah risiko itu —
   ia hanya membaca `ApprovedBy` yang sudah ada, tidak menyentuh jalur `ApproveAsync`.
3. **`MapWithNameAsync`/`MapDetailWithNameAsync` menambah satu kueri `_dbContext.Users` per
   permintaan** pada enam endpoint mutasi (Upload, Reupload, Validate, DeclareOpening, Approve,
   Reject) yang sebelumnya nol kueri tambahan. Ini biaya yang disengaja dan kecil — satu baris
   per permintaan, bukan N+1 — dan hanya pada endpoint yang **tidak** berpaging.
4. **Tidak ada uji otomatis ditambahkan.** Sesuai `rules/backend/TEST_POLICY.md` — bukan
   coverage gap.

---

## 5. Status Git (backend)

```text
 M Areas/Corporate/FinanceManagement/AccountingIntegration/Controllers/FinanceOpeningItemBatchesController.cs
 M Areas/Corporate/FinanceManagement/AccountingIntegration/DTOs/OpeningItemBatchDtos.cs
 M Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceOpeningItemBatchService.cs
```

(Perubahan lain yang belum di-commit berasal dari task sebelumnya.) Tidak ada stage, commit,
push, pull, merge, rebase, atau deploy.

---

## 6. Dampak ke Frontend

`FE-FIN-035` (REV-16 frontend, belum dikerjakan) menunggu rilis ini bersama `BE-FIN-088`: layar
batch **MUST** menampilkan `ApprovedByName` dan menulis keterangan bila `null` — **MUST NOT**
menampilkan `ApprovedBy` (GUID) mentah.

---

## 7. Yang Belum Dikerjakan

| Hal | Keadaan |
| --- | --- |
| `dotnet build` | **NOT RUN** — instruksi pengguna |
| Uji manual `K.1.4` (pola yang sama) | **BELUM DILAPORKAN** |
| `contracts/api-contract.md` G.2 | Tidak perlu diubah — endpoint sudah ada, hanya satu ruas respons bertambah |
| `roadmap/01-backend-roadmap.md` | Baris `BE-FIN-089` **MUST** diperbarui: ⬜ → 🟡 |

### Langkah yang perlu pengguna jalankan

1. `dotnet build` — pastikan perubahan terkompilasi.
2. Uji manual: setujui satu batch (`POST /{id}/approve`), amati `ApprovedByName` pada respons
   dan pada `GET /{id}` sesudahnya; buka daftar (`GET /`) berisi beberapa batch yang sudah
   disetujui pengguna berbeda-beda, amati setiap `ApprovedByName` benar untuk barisnya
   masing-masing.
3. Setelah itu task dapat ditandai ✅ pada roadmap.
