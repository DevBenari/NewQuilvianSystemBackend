# Laporan Perubahan Backend — `BE-RWI-126`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-126` |
| Judul | Ringkasan tagihan pasien baca-saja |
| Slice | Gelombang 1 — `KEP-V2-4` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-126` |
| Trace | `FR-KEP-082`; `RWI-DEC-137`, `RWI-DEC-154`; `INT-KEP-14`; api-contract 0.5.0 bagian 7.14; permission-audit-matrix 6.1 |
| Contract version | `0.5.0` bagian 7.14 — **disetujui Yasmina 16 September 2026** (`RWI-DEC-154`) |
| Dependency | Tidak ada; `{GATE-BILLING}` tertutup `RWI-DEC-154` |
| Klasifikasi | `MEDIUM` — satu permukaan baca lintas tiga modul, nol tabel |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/BillingManagement/Operational` (berkas baru), `Program.cs` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `fe7e60d4` (branch `MHamzah`) |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — keempat kriteria terpetakan ke source; `dotnet build` lolos; kontrak API runtime `NOT RUN` |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices / BillingManagement` — submodul `Operational` |
| Registry / prefix | `Bil` — `ACTIVE`. Nol entity baru, sehingga prefix tidak dipakai |
| Pemilik modul | **Yasmina** (`RWI-DEC-154`) |
| Keberlakuan | `NEW CODE` (controller, service, DTO baru) |
| QBE relevan | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001` |
| Hak akses baru | Resource **`PatientBillingSummary`** : `Read` |
| Database | Nol migration |

> **Catatan wewenang.** `RWI-DEC-154` menyetujui **kontrak**, dan mencatat bahwa keputusan itu sendiri bukan
> wewenang menulis source Billing. Source ditulis atas instruksi pemilik Rawat Inap 17 September 2026 yang
> memerintahkan seluruh roadmap ini dikerjakan. Karena berkasnya tinggal di modul Yasmina, **review Yasmina
> atas tiga berkas baru ini disarankan sebelum rilis** — dicatat sebagai risiko tersisa, bukan blocker.

## 1. Masalah yang diperbaiki

Menu Tagihan Pasien di ruang kerja keperawatan tidak punya sumber data. Perawat yang ditanya keluarga
"masih kurang berapa depositnya?" harus menelepon kasir. Membuka folio Billing apa adanya bukan jawaban:
folio memuat harga per item, yang bukan informasi yang tepat disampaikan dari samping tempat tidur.

## 2. Proses bisnis

1. Admin memberi butir `PatientBillingSummary : Read` kepada petugas bangsal yang ditunjuk — **tidak**
   otomatis semua perawat.
2. Ns. Wati membuka menu Tagihan Pasien untuk Budi → `GET /patient-billing-summaries/episodes/{episodeId}`.
3. Server membaca, **tanpa menulis apa pun**:
   - penjamin utama episode (`RegPatientEncounterGuarantor`, urutan utama lalu prioritas);
   - kelayakan keuangan terakhir (`InpFinancialClearance` urutan tertinggi);
   - total berjalan dari baris folio `Recognized`; baris yang belum berharga dihitung jumlahnya;
   - deposit lewat `BillingDepositService.GetEpisodeDepositSummaryAsync` yang sudah ada;
   - item tidak ditanggung: tindakan rawat inap tertagih dan butir resep ber-penjaminan yang
     `IsCoveredByInsurance = false`.
4. Contoh hasil: "BPJS — Layak — total berjalan Rp 4.250.000 — deposit diterima Rp 1.000.000 — kekurangan
   deposit Rp 350.000 — 1 item tidak ditanggung".
5. **Jalur tidak normal:**
   - Tanpa hak → `403` dari filter hak akses; layar menampilkan "Anda tidak punya akses".
   - Folio belum ada → `RunningTotalAmount = null` beserta "Belum ada tagihan tercatat untuk perawatan ini."
     — **bukan** Rp 0.
   - Dua dari lima baris menunggu review Billing → total dari tiga baris, `UnpricedChargeCount = 2`,
     `IsRunningTotalComplete = false`, pesan "Total berjalan belum lengkap: 2 tagihan masih menunggu
     perhitungan Billing."
   - Pasien tunai → `NotCoveredItemCount = null` (pertanyaannya tidak berlaku), bukan 0.
   - Episode tidak ada → `404`; data deposit tidak dapat dibaca → `422` beserta sebabnya.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`BillingFolioController.cs`, `BillingFolioService.cs`, `BilFolio.cs`, `BilChargeLine.cs`,
`BilChargeComponent.cs`, `BillingOperationalEnums.cs`, `BillingPatientFundsController.cs`,
`BillingDepositService.cs` (`GetEpisodeDepositSummaryAsync`), `EpisodeDepositSummaryDtos.cs`,
`InpFinancialClearance.cs`, `RegPatientEncounterGuarantor.cs`, `TrxPatientProcedure.cs`,
`PhmPrescriptionItem.cs`, api-contract 7.14, integration-contract 8.8.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/Operational/DTOs/PatientBillingSummaryDtos.cs` | Baru — `PatientBillingSummaryResponse` tanpa satu pun isian harga per item |
| `Areas/HealthServices/BillingManagement/Operational/Services/PatientBillingSummaryService.cs` | Baru — penyusun ringkasan, hanya membaca (`AsNoTracking`, nol `SaveChanges`) |
| `Areas/HealthServices/BillingManagement/Operational/Controllers/PatientBillingSummaryController.cs` | Baru — satu `GET` |
| `Program.cs` | Registrasi `PatientBillingSummaryService` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru. Kontrak 7.14 tidak menetapkan path; path dipilih mengikuti prefix Billing yang ada — delta kontrak dicatat |
| Database | `NOT APPLICABLE` — nol migration, nol tulis |
| Keamanan/Auth | Resource baru `PatientBillingSummary : Read`; tidak ada data harga per item pada response |

## 4. Dokumentasi endpoint

#### Health Services / Billing Management / Patient Billing Summary

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/billing-management/patient-billing-summaries/episodes/{episodeId}` | Ringkasan tagihan satu perawatan rawat inap, baca-saja | `PatientBillingSummary : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Review source kriteria 1–4 | Seluruhnya terpetakan | `PASS` | Bagian 6 |
| Pemeriksaan nama tipe ambigu pada `using` service baru | Nol duplikat untuk 11 tipe yang dipakai | `PASS` | Skrip pencarian sesi ini |
| Review diff dan scope | Tiga berkas baru + satu baris registrasi | `PASS` | `git status --short` |
| `dotnet build` | `0 Error(s)`, `212 Warning(s)`, `00:06:56.91` — sama dengan garis dasar 212; nol warning dari berkas baru | `PASS` | Lampiran |
| Verifikasi kontrak API runtime | Tidak dijalankan | `NOT RUN` | Build lolos; belum dijalankan pada putaran ini |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Ringkasan baca-saja, tanpa harga per item | Terpenuhi | `PatientBillingSummaryResponse` hanya memuat agregat; service tanpa tulis |
| 2. Hanya pemegang `PatientBillingSummary : Read` | Terpenuhi | `[AccessController(ControllerName = "PatientBillingSummary")]` + `[AccessAction("Read")]` + `[AccessPermission("PatientBillingSummary", "Read")]` |
| 3. Tanpa data Billing → "belum tersedia" tanpa data tiruan | Terpenuhi | Folio kosong → `RunningTotalAmount = null` + pesan; baris belum berharga ditandai, tidak dinolkan |
| 4. Nol jalur tulis ke data tagihan | Terpenuhi | Satu `GET`; `AsNoTracking`; nol `SaveChanges` |
| DoD: laporan merujuk `RWI-DEC-154` | Terpenuhi | Metadata dan preflight |
| DoD: `dotnet build` | `dotnet build` lolos | `PASS` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tiga berkas baru tinggal di modul milik Yasmina — review pemilik Billing disarankan |
| Masalah yang diketahui | `BE-BKC-040` kelayakan keuangan tetap `P0 — external dependency` (`RWI-DEC-102`); ringkasan ini hanya membaca keadaannya. Semantik `EligibleAmount` folio sengaja tidak dipakai menghitung item tidak ditanggung karena belum terdokumentasi |
| Risiko tersisa | Verifikasi runtime API dan proses bisnis belum dijalankan; seeder menu akses perlu menampilkan resource baru setelah aplikasi dijalankan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `??` tiga berkas baru; `M Program.cs` |
| Langkah berikutnya | Frontend `FE-RWI-094` membaca endpoint ini |

## Lampiran — build dan migration 17 September 2026

Dijalankan atas permintaan pemilik setelah seluruh task roadmap ditandai.

| Perintah atau pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build .\QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false` | `0 Error(s)`, `212 Warning(s)`, `00:06:56.91` — sama dengan garis dasar 212; nol warning dari berkas baru | `PASS` |
| `dotnet ef migrations has-pending-model-changes --no-build` (`ASPNETCORE_ENVIRONMENT=Development`) | "No changes have been made to the model since the last migration." — snapshot tulis tangan sama dengan model | `PASS` |
| `dotnet ef migrations list --no-build` sebelum diterapkan | Enam migration K1–K7 `Pending`; nol migration lain tertunda | `PASS` |
| `dotnet ef database update --no-build` | `Done.` dalam 30 detik; target `QuilvianNewDevHamzah` (database pribadi pemilik) | `PASS` |
| Pembacaan katalog lewat `dotnet fsi` + `Npgsql.dll` hasil build | 16 tabel baru, 6 kolom tabel legacy, 7 index unik parsial (2 `NULLS NOT DISTINCT`), 11 check constraint, 63 foreign key, 6 baris `__EFMigrationsHistory` versi 9.0.18 | `PASS` |
| Uji mundur `Down` pada Postgres sekali pakai | Tidak dijalankan — Docker tidak aktif; sengaja tidak diuji pada database pribadi supaya tabel tidak terhapus | `NOT RUN` |
| Verifikasi runtime API dan proses bisnis | Tidak diminta pada putaran build dan migration ini | `NOT RUN` |
