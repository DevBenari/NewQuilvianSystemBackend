# Laporan Perubahan Backend — `BE-AST-027`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-AST-027` |
| Judul | Pratinjau dan proses penyusutan |
| Slice | `NLI-S2` (gelombang `MVP-4`) |
| Roadmap | `nilai/roadmap/backend-roadmap.md` `REV-1`, kartu `BE-AST-027` |
| Trace | `CAP-09.4`; `AST-DEC-013`, `014`, `036`, `039`, `042`; `AST-DES-009`, `010`, `011`, `033`; `AST-RULE-040`, `041`, `045`, `046`, `047`; `contracts/api-contract.md` 2.6; `contracts/validation-matrix.md` `AST-VAL-NLI-010..016`; `flowcharts/01-proses-penyusutan.md` |
| Contract version | `AST-NLI-1.0.0` — `approved` 2026-11-12 (`AST-DEC-051`) |
| Dependency | `BE-AST-026` ✅ (`ResolveForUnitsAsync`, akun kontrol), `BE-AST-025` ✅ (tabel), `BE-AST-015` 🟡 (buku nilai, kebijakan, kotak keluar) |
| Klasifikasi | `HIGH` — perhitungan keuangan, satu transaksi atas banyak tabel, kejadian ke Akuntansi; tanpa migration |
| Task mode | `BACKEND` — perintah Rizki 2026-11-24 "commit lalu lanjut BE-AST-027" |
| Target tulis | `Areas/Corporate/AssetManagement/Valuation/{Services,Controllers,DTOs}/`, `AssetManagementServiceCollectionExtensions.cs` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `1f353c45` (branch `rizkiG`) |
| Tanggal | 24 November 2026 |
| Status | 🟡 SEBAGIAN 24 November 2026 — source selesai, QBE Strict 7 berkas `PASS`, build Rizki DLL 11:41:45 memuat `AssetDepreciationRunService`; **belum diuji** (atas perintah Rizki: perbarui status lalu berhenti). Uji rumus dan runtime, termasuk proses November 2026 di DB dev (disetujui Rizki), belum dijalankan |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AssetManagement` / `Valuation` |
| Pemilik / prefix registry | `AssetManagement / Valuation / Nilai Aset`, prefix `Ast`, `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-API-*`, `QBE-PERM-*` (resource baru `AssetDepreciationRun` `Read`/`Process`), `QBE-SVC-*` |
| Tidak berlaku | `QBE-DB-*` — tanpa migration (tabel dari `BE-AST-025`) |

---

## 1. Masalah yang diperbaiki

Buku nilai aset belum pernah disusutkan: belum ada pratinjau, proses bulanan, baris per aset, maupun kejadian
`AST-PENYUSUTAN` ke Akuntansi (`AST-DEC-039`).

---

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `Valuation/Services/DepreciationCalculator.cs` | Baru — fungsi murni publik: jumlah bulanan `AST-DES-009` (dibulatkan ke rupiah penuh, bulan terakhir menyerap sisa), jadwal beberapa bulan, penyesuaian koreksi harga `AST-RULE-047` |
| `Valuation/Services/AssetDepreciationRunService.cs` | Baru — pratinjau dan proses: urutan periode, rencana per aset (estimasi, unit akhir bulan, pusat biaya, penyesuaian), galat/peringatan, satu transaksi tulis |
| `Valuation/Services/AssetDepreciationRunService.Query.cs` | Baru — daftar proses, rincian (total per kelompok × pusat biaya, ringkasan kotak keluar), baris per aset |
| `Valuation/Controllers/AssetDepreciationRunsController.cs` | Baru — resource `AssetDepreciationRun` (`Read`, `Process`), 5 operasi API 2.6 |
| `Valuation/DTOs/AssetDepreciationRunDtos.cs` | Baru — query, permintaan proses, pratinjau, proses, baris, galat |
| `Valuation/Services/AssetValuationRules.cs` | + kode `AST-VAL-NLI-010..016` dan pesannya |
| `AssetManagementServiceCollectionExtensions.cs` | + `AssetDepreciationRunService` |

Tanpa baris komentar.

### 2.1 Aturan di source

| Aturan | Source |
| --- | --- |
| Penyusutan bulanan = (nilai buku − residu) ÷ sisa umur, dibulatkan ke rupiah penuh; bulan terakhir = sisa yang dapat disusutkan | `DepreciationCalculator.MonthlyAmount` |
| Aset ikut proses periode P bila buku dikapitalisasi dan disusutkan, aset `Terdaftar` milik badan hukum, mulai penyusutan ≤ P, belum disusutkan untuk P, dan buku `Aktif` atau dihentikan sesudah P (`AST-RULE-041`) | `BuildPlanAsync` |
| Umur dan residu diambil dari kebijakan kelompok yang berlaku **pada periode proses**, kecuali aset punya estimasi sendiri (`AST-DES-033`); hasilnya disimpan ke buku | `ResolveEstimate`, `ProcessAsync` |
| Unit pada **akhir bulan** dari `AstAssetPlacement` (mulai sebelum awal bulan berikut WIB, belum berakhir saat itu); pusat biaya lewat `AssetSubledgerSetupService.ResolveForUnitsAsync` (`AST-RULE-046`, `AST-DES-011`) | `BuildPlanAsync` |
| Bulan yang terlewat dikejar sebagai penyesuaian `TerlambatDicatat`, atau `Pemulihan` bila ada entri pemulihan sesudah bulan terakhir disusutkan (`AST-DES-010`) | `BuildPlanAsync` |
| Entri `KoreksiHargaPerolehan` pada periode P → penyesuaian `KoreksiHarga` = akumulasi seharusnya − akumulasi tercatat (`AST-RULE-047`) | `DepreciationCalculator.PriceCorrectionAdjustment` |
| Galat pratinjau: unit belum jelas pusat biayanya atau aset belum ditempatkan pada akhir bulan (`014`), kelompok tanpa umur manfaat (`015`); peringatan kelompok tanpa akun kontrol (`016`) | `DescribeIssuesAsync` |
| Periode pertama = bulan berlakunya parameter nilai pertama badan hukum; berikutnya berurutan; tidak boleh melewati bulan berjalan (`010`, `011`, `012`); proses ditolak bila pratinjau bergalat (`013`) | `ReadSequenceAsync`, `CheckBlockerAsync`, `ProcessAsync` |
| Satu transaksi: proses, baris, entri `Penyusutan`/`PenyesuaianPenyusutan` (berpusat biaya), buku, dan kotak keluar `AST-PENYUSUTAN` satu per (kelompok, pusat biaya), tanggal akuntansi akhir periode, komponen = kode kelompok; status awal `Held` selama kode belum dinyalakan | `ProcessAsync` |
| Proses ganda bersamaan → unik `(LegalEntityId, Period)` → 409 `011`; buku berubah di tengah proses → 409 `028` | `ProcessAsync` |

### 2.2 Delta terhadap kontrak dan desain

| Delta | Alasan |
| --- | --- |
| Bulan terlewat digabung menjadi **satu** baris penyesuaian per alasan, dengan catatan rentang bulan pada entri nilai; flowchart 01 menulis satu baris per bulan | Indeks unik `(RunId, AssetId, AdjustmentReason)` dari kamus data (`BE-AST-025`) |
| Periode pertama = parameter nilai pertama | Kontrak tidak menyebut periode awal; `AST-DES-032` menetapkan parameter pertama sebagai go-live |
| Aset belum ditempatkan pada akhir bulan = galat `014` (varian pesan) | Pusat biaya tidak dapat ditentukan tanpa unit; tidak ada keputusan pembebanan aset di gudang |
| Pratinjau memuat `nextAllowedPeriod`, `blocker`, `processedRunId`, `lineCount`; `GET /{id}/lines` + `search`, `adjustmentOnly` | Layar `FE-AST-020` |
| DTO di berkas `AssetDepreciationRunDtos.cs`, bukan `AssetValuationDtos.cs` | Ukuran berkas |

---

## 3. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| QBE `-Mode Strict` (7 berkas) | `VIOLATION: 0`, `REVIEW: 0`, `PASS` | `PASS` |
| Baris komentar baru | 0 | `PASS` |
| Build | DLL 2026-11-24 11:41:45 (Rizki), memuat `AssetDepreciationRunService` | `PASS` |
| Uji rumus dari DLL (`AT-NLI-01`, `05`, `06`, `07`, `08`) | **Belum dijalankan** — skrip siap: `QuilvianSystemFrontendDev/test-with-agy/asset-management/be-ast-027-20261124/cek-kalkulator.fsx` | `NOT RUN` |
| Runtime pratinjau dan proses November 2026 (`AT-NLI-03`, `09`, `30`, `31`, `32`, `37`, `403`) | **Belum dijalankan** — Rizki menyetujui proses November 2026 di DB dev (permanen); uji ditahan atas perintah Rizki | `NOT RUN` |

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Source | Bukti | Status |
|---|---|---|---|
| 1. Rumus `AST-DES-009`; contoh monitor Rp 1.416.667, bulan ke-60 menutup tepat | `DepreciationCalculator` | Belum diuji | 🟡 terpetakan |
| 2. Penyesuaian terlambat dicatat, pemulihan, koreksi harga | `BuildPlanAsync` | Belum diuji; pemulihan dan koreksi harga baru dapat diuji runtime sesudah `BE-AST-036` dan `BE-AST-028` | 🟡 terpetakan |
| 3. Pusat biaya unit pada akhir bulan | `BuildPlanAsync` (placement + `ResolveForUnitsAsync`) | Belum diuji | 🟡 terpetakan |
| 4. Sekali per periode, berurutan, satu transaksi | `CheckBlockerAsync`, `ProcessAsync` | Belum diuji | 🟡 terpetakan |
| 5. Kotak keluar satu baris per (kelompok, pusat biaya) | `ProcessAsync` | Belum diuji | 🟡 terpetakan |
| 6. Aset yang dihapus pada periode itu tidak disusutkan | Saringan `BookStatus`/`StoppedAt` | Belum diuji; baru dapat diuji runtime sesudah `BE-AST-035` | 🟡 terpetakan |

| DoD | Status |
|---|---|
| `V-BE` QBE, build | Terpenuhi |
| `V-BE` uji rumus dan runtime | **Belum** |
| Laporan, roadmap, traceability | Ditulis (status 🟡) |
| Commit | Belum |

---

## 5. Langkah berikutnya

1. Jalankan `cek-kalkulator.fsx` terhadap DLL 11:41:45.
2. Pratinjau November 2026 (tanpa menulis): periksa jumlah aset, total per kelompok × pusat biaya, galat, peringatan; uji `010` (Desember) dan `012` (Oktober).
3. Bila angkanya benar, proses November 2026 sekali di DB dev (disetujui Rizki), lalu periksa baris, buku, entri, kotak keluar `Held`, proses ulang 409 `011`, dan akun tanpa hak 403.
4. Sisa uji `BE-AST-015` (`AT-NLI-08` saldo awal, `AT-NLI-33`, 403) dijalankan bersama langkah 2–3.
