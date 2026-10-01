# Laporan Perubahan Backend — `BE-FIN-041`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-041` |
| Judul | Pembayaran supplier punya tempat untuk mencatat porsi yang dilunasi deposit |
| Slice | `REV-4` — Purchasing/AP (`EPIC FIN-15`), gelombang `R4-1` (bebas, boleh paralel) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task) dan bagian "Gelombang eksekusi — `REV-4`" |
| Trace | `FIN-DES-045`; `02-backend-architecture.md` §D.6, D.7 baris 3; `erd/data-dictionary.md` §D.1, §D.4(a) |
| Contract version | AMENDMENT REVISI 5 (`02-backend-architecture.md`, `locked` 26 September 2026) |
| Dependency | — (tabel `FinPayment` sudah ada, dibuat `BE-FIN-020` ✅) |
| Klasifikasi | `LIGHT` — satu repository (skor 0); ≤8 berkas diperiksa (skor 0); 4 berkas diubah (skor 1, tepat di batas — dihitung konservatif); logika bisnis sederhana, murni penambahan kolom + rumus constraint (skor 0); kontrak API — nol dampak, `NOT APPLICABLE` (skor 0); database — dampak schema pada tabel yang sudah berjalan (skor 2, faktor tertinggi dinaikkan satu tingkat karena tabel sudah berjalan); keamanan/auth — nol dampak (skor 0); UI/workflow — nol dampak (skor 0). Total 3 + kenaikan 1 tingkat (tabel berjalan) → `MEDIUM` secara konservatif |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Payable/Models/FinPayment.cs`, `Repositories/Configurations/Corporate/FinanceManagement/Payable/FinPaymentConfiguration.cs`, `Migrations/20260928120000_AddDepositAppliedAmountToFinPayment.cs` (+ `.Designer.cs`), `Migrations/ApplicationDbContextModelSnapshot.cs`, `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` (tanda status) |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Belum di-commit — lihat Status Git bagian 7 (working tree `Yasmina`, HEAD `45f2bb7c`) |
| Tanggal | 28 September 2026 |
| Status | 🟡 **SEBAGIAN.** Source dan berkas migration selesai; **otorisasi pembuatan berkas migration diberikan eksplisit pengguna 28 September 2026** (menjawab prasyarat #13 yang sebelumnya "Belum"). `dotnet build` dan **eksekusi migration** ke database **sengaja belum dijalankan** — keduanya memerlukan otorisasi eksplisit terpisah (`dotnet build` menunggu instruksi umum "jangan lakukan build otomatis" yang berlaku sepanjang sesi ini; eksekusi migration adalah wewenang terpisah dari pembuatan berkas per `AGENTS.md` Keselamatan Database, dan belum diminta) |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, `FinPayment` (pembayaran keluar ke supplier) tidak punya tempat mencatat bahwa
sebagian utang dilunasi memakai Deposit Retur (kredit dari retur pembelian, `BE-FIN-035`) alih-alih
kas yang benar-benar ditransfer. Tanpa kolom ini, `BE-FIN-036` (pemakaian Deposit Retur sebagai
sumber dana pembayaran) tidak dapat dibangun sama sekali — tidak ada tempat menyimpan berapa
banyak yang "dibayar" lewat deposit vs. yang benar-benar keluar dari rekening bank.

**Contoh konkret.** Utang ke PT Contoh Farma Rp 10.000.000. RS punya Deposit Retur Rp 2.500.000
dari retur bulan lalu. Tanpa kolom ini: tidak ada cara mencatat bahwa Rp 2.500.000 dari pelunasan
ini berasal dari deposit, bukan kas — sistem akan mengira seluruh Rp 10.000.000 (dikurangi potongan/
tambahan biasa) benar-benar ditransfer dari rekening bank, padahal senyatanya hanya
Rp 7.500.000 yang ditransfer. Sesudah task ini (kolomnya tersedia, nilainya baru benar-benar dipakai
`BE-FIN-036`): `NetTransferAmount` dapat dihitung `TotalAmount − DeductionAmount + AdditionAmount −
DepositAppliedAmount`, sehingga jumlah yang benar-benar ditransfer akurat.

---

## 2. Proses bisnis

Task ini **tidak menambah proses bisnis baru** — murni menyiapkan tempat penyimpanan (kolom +
constraint) yang akan dipakai proses bisnis `BE-FIN-036` (reservasi/pelepasan/penerapan Deposit
Retur pada siklus hidup pembayaran). Sebelum `BE-FIN-036` mengisi kolom ini, nilainya **selalu 0**
untuk setiap pembayaran, lama maupun baru — nol perubahan perilaku yang dapat diamati pengguna
sampai `BE-FIN-036` selesai.

**Jalur tidak normal yang relevan bagi task ini sendiri:**

- Kolom `DepositAppliedAmount` diberi `NOT NULL DEFAULT 0` — baris `FinPayment` lama otomatis
  mendapat nilai 0 tanpa backfill manual.
- Constraint `CK_FinPayment_NetTransfer` (bentuk lama) diganti bentuk baru yang menyertakan
  `DepositAppliedAmount`. Karena seluruh baris lama bernilai 0 pada kolom baru ini, rumus baru
  menghasilkan hasil yang **identik** dengan rumus lama untuk baris-baris tersebut — constraint
  baru tidak menolak satu pun baris lama.
- Constraint baru `CK_FinPayment_DepositApplied` menolak nilai negatif pada kolom ini — tidak
  relevan untuk baris lama (selalu 0), baru relevan mulai `BE-FIN-036`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `AGENTS.md` (backend) bagian Keselamatan Database dan Aturan Entity Framework — memastikan
  pemisahan wewenang implementasi source vs. pembuatan migration vs. eksekusi migration
- `docs/module-blueprints/finance-management/erd/data-dictionary.md` §D.1 (kolom baru), §D.4(a)
  (bentuk DDL persis: `ALTER TABLE`, `DROP`/`ADD CONSTRAINT`)
- `docs/module-blueprints/finance-management/02-backend-architecture.md` §D.2 (`FIN-DES-045`),
  §D.3 (tabel kepemilikan data), §D.6-D.7 (rencana migration REVISI 5, baris migration ke-3)
- `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` (kartu task
  `BE-FIN-041`, prasyarat #13, tabel migration REV-4, grafik dependency)
- `Areas/Corporate/FinanceManagement/Payable/Models/FinPayment.cs` dan
  `Repositories/Configurations/Corporate/FinanceManagement/Payable/FinPaymentConfiguration.cs`
  (`BE-FIN-020` ✅) — bentuk existing sebelum diubah
- `Migrations/20260926110000_AddSourcePurchasingInvoiceIdToSupplierPayable.cs` dan
  `.Designer.cs`, serta `20260923060000_FixFinReceiptTenderRequiredForReversal.cs` — pola migration
  tangan ("ringkas", tanpa `BuildTargetModel` penuh) yang ditiru persis untuk migration `ALTER TABLE`
  pada tabel yang sudah berjalan
- `Migrations/ApplicationDbContextModelSnapshot.cs` bagian entity `FinPayment` — sumber kebenaran
  model yang diperbarui manual (bukan hasil `dotnet ef migrations add`)
- `Migrations/MigrationMetadata.g.cs` — diperiksa dan dipastikan **tidak perlu disentuh**: berkas
  itu hanya memuat migration lama yang Designer-nya sudah diarsipkan ke `Migrations/History/`,
  bukan migration baru

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Payable/Models/FinPayment.cs` | Tambah properti `DepositAppliedAmount` (default `0m`); perbarui komentar invariant `NetTransferAmount` dan ringkasan kelas |
| `Repositories/Configurations/Corporate/FinanceManagement/Payable/FinPaymentConfiguration.cs` | Tambah `entity.Property(x => x.DepositAppliedAmount)` (presisi 18,2, default 0); ganti `CK_FinPayment_NetTransfer` ke rumus baru; tambah `CK_FinPayment_DepositApplied` |
| `Migrations/20260928120000_AddDepositAppliedAmountToFinPayment.cs` | **Baru.** `Up()`: `AddColumn` + drop/add dua constraint (rumus baru + constraint nilai). `Down()`: kebalikannya persis, dengan catatan aman hanya selama belum ada baris `DepositAppliedAmount > 0` |
| `Migrations/20260928120000_AddDepositAppliedAmountToFinPayment.Designer.cs` | **Baru.** Bentuk ringkas (pola `AddSourcePurchasingInvoiceIdToSupplierPayable`) — tanpa `BuildTargetModel` penuh; sumber kebenaran model tetap `ApplicationDbContextModelSnapshot.cs` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Tambah `b.Property<decimal>("DepositAppliedAmount")` pada entity `FinPayment`; tambah `CK_FinPayment_DepositApplied`; perbarui `CK_FinPayment_NetTransfer` ke rumus baru |
| `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` | Tanda status `BE-FIN-041` → 🟡 di seluruh titik kemunculan; prasyarat #13 diperbarui — otorisasi pembuatan berkas migration **diberikan** 28 September 2026 (eksekusi tetap terpisah, belum); pembaruan blocker `BE-FIN-036` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — task ini tidak menyentuh controller/DTO/endpoint apa pun |
| Database | **Mengubah tabel yang sudah berjalan** (`FinPayment`, dibuat `BE-FIN-020`). Migration `AddDepositAppliedAmountToFinPayment` dibuat (berkas `.cs`/`.Designer.cs` + `ApplicationDbContextModelSnapshot.cs`) dengan **otorisasi eksplisit pengguna 28 September 2026**. **Eksekusi migration ke database MASIH BELUM DIJALANKAN** — wewenang terpisah, menunggu instruksi eksplisit lain sesuai `AGENTS.md` Keselamatan Database. Migration bersifat aditif murni (`ADD COLUMN ... DEFAULT 0` + penggantian dua check constraint), dapat dijalankan tanpa downtime, dan `Down()`-nya tercatat aman hanya selama belum ada baris `DepositAppliedAmount > 0` |
| Keamanan/Auth | `NOT APPLICABLE` — tidak ada perubahan otorisasi/endpoint |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menyentuh endpoint apa pun.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | — | `NOT RUN` | Sengaja tidak dijalankan — instruksi eksplisit pengguna berlaku sepanjang sesi ini ("jangan lakukan build otomatis") |
| Migration diterapkan di lingkungan pengembangan (`dotnet ef database update`) | — | `NOT RUN` | Eksekusi migration adalah wewenang terpisah dari pembuatan berkas (`AGENTS.md` Keselamatan Database) — belum diminta secara eksplisit pada task ini |
| Pembuktian constraint baru tidak menolak baris lama | Secara matematis identik untuk `DepositAppliedAmount = 0` (`X = A - B + C` sama dengan `X = A - B + C - 0`) | `NOT RUN` (pembuktian aljabar, bukan eksekusi database) | Rumus `CK_FinPayment_NetTransfer` baru pada `FinPaymentConfiguration.cs` dan migration |
| Review `ApplicationDbContextModelSnapshot.cs` konsisten dengan `FinPaymentConfiguration.cs` | Properti, presisi, default, dan kedua nama constraint sama persis di kedua berkas | `NOT RUN` (review manual) | Diff kedua berkas |

Uji manual: `NOT FEASIBLE` — memerlukan migration benar-benar diterapkan ke database, yang belum
diotorisasi.

**AUTOMATED TEST:** `NOT APPLICABLE` — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

**Tidak dijalankan:** `dotnet build`, eksekusi migration, dan `Invoke-QbeConformanceCheck.ps1` —
ketiganya menunggu instruksi eksplisit terpisah.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Kolom `DepositAppliedAmount` (default 0) pada entity + configuration | Terpenuhi (source) | `FinPayment.cs`, `FinPaymentConfiguration.cs` |
| `CK_FinPayment_NetTransfer` diganti rumus baru | Terpenuhi (source) | `FinPaymentConfiguration.cs`, migration, `ApplicationDbContextModelSnapshot.cs` |
| `CK_FinPayment_DepositApplied` | Terpenuhi (source) | sama seperti di atas |
| Migration `AddDepositAppliedAmountToFinPayment` | Terpenuhi (berkas dibuat, otorisasi eksplisit) | Migration `.cs`/`.Designer.cs` |
| Seluruh baris `FinPayment` lama tetap lolos constraint baru tanpa backfill | Terpenuhi secara desain (pembuktian aljabar) — **belum diverifikasi langsung ke database** karena migration belum dieksekusi | Lihat bagian 5 |
| `NetTransferAmount` hitungan service untuk pembayaran lama tidak berubah | Terpenuhi — `FinancePaymentService` tidak disentuh task ini sama sekali; service tetap menghitung `NetTransferAmount` tanpa mengetahui kolom baru sampai `BE-FIN-036` menyambungkannya | Nol perubahan pada `FinancePaymentService.cs` |
| `dotnet build` berhasil | **Belum terpenuhi** | Sengaja `NOT RUN` |
| Migration diterapkan di lingkungan pengembangan | **Belum terpenuhi** | Sengaja `NOT RUN` — wewenang terpisah |

Task ini **belum** dapat ditandai `✅` — dua butir validasi terakhir belum terpenuhi.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada |
| Masalah yang diketahui | Tidak ada — task ini murni aditif sesuai kontrak `erd/data-dictionary.md` §D.4(a), tanpa deviasi |
| Risiko tersisa | Migration belum dieksekusi ke database mana pun — kolom `DepositAppliedAmount` baru ada di source, belum ada di skema database sesungguhnya. `BE-FIN-036` yang membaca/menulis kolom ini lewat EF akan gagal terhadap database yang belum menjalankan migration ini. `Down()` tercatat eksplisit hanya aman selama belum ada baris `DepositAppliedAmount > 0` — risiko ini relevan baru setelah `BE-FIN-036` berjalan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` pada task ini sendiri — dilanjutkan dari sesi yang sama tempat `BE-FIN-035` baru selesai; instruksi "jangan lakukan build otomatis" yang berlaku sudah dipatuhi sejak awal task ini (tidak ada percobaan `dotnet build` pada task ini) |
| Status Git | `git status --short` pada akhir pekerjaan mencakup task ini **ditambah** sisa perubahan `BE-FIN-035` yang belum di-commit pada sesi yang sama: `M Areas/Corporate/FinanceManagement/Payable/Models/FinPayment.cs`, `M Repositories/Configurations/Corporate/FinanceManagement/Payable/FinPaymentConfiguration.cs`, `M Migrations/ApplicationDbContextModelSnapshot.cs`, `?? Migrations/20260928120000_AddDepositAppliedAmountToFinPayment.cs`, `?? Migrations/20260928120000_AddDepositAppliedAmountToFinPayment.Designer.cs`, `?? docs/module-blueprints/finance-management/task/report/backend/BE-FIN-041.md`, plus berkas `BE-FIN-035` yang sudah dilaporkan terpisah pada laporannya sendiri. Tidak ada perubahan lain milik pengguna yang tersentuh |
| Langkah berikutnya | (1) Pemilik repository menjalankan `dotnet build`; (2) pemilik repository mengotorisasi dan menjalankan eksekusi migration `AddDepositAppliedAmountToFinPayment` ke lingkungan pengembangan (wewenang terpisah dari pembuatan berkas yang sudah diberikan hari ini); (3) setelah keduanya, `BE-FIN-036` dapat mulai dikerjakan — inilah prasyarat terakhirnya |
