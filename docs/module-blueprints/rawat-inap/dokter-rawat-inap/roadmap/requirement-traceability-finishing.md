# Traceability Requirement — Dokter Rawat Inap, Finishing Rawat Inap

| Field | Nilai |
|---|---|
| Berkas | `dokter-rawat-inap/roadmap/requirement-traceability-finishing.md` — revision `2` |
| Status | Backend BE-RWI-160–164 ✅ 5 Oktober 2026. Frontend FE-RWI-173 s.d. 179 ✅ 6 Oktober 2026 (lint:errors 0 error, build lulus, 88 test unit dokter PASS, Playwright E2E 11/11 PASS); FE-RWI-172 tetap 🟡 berhenti aman (tombol perawat terkunci) |
| Blueprint | `RWI-BP-001` revision `8`, sub-modul `dokter-rawat-inap`, kontrak `0.7.0` `approved` 2026-10-02 (`RWI-DEC-221`) |
| Roadmap | `backend-roadmap-finishing.md` revision `2` (`BE-RWI-160` s.d. `164`); `frontend-roadmap-finishing.md` revision `1` (`FE-RWI-172` s.d. `179`) |
| Sumber requirement | `PRD-RWI-FINISHING-001` v`0.4` (rumusan FR); `04-prd-to-mvp.md` bagian 23; decision log revision `32` |
| Masukan dan hash | Seperti metadata roadmap; hash lengkap pada `../blueprint-manifest.md` bagian 10 |
| Source SHA | Backend `bf5c6bde`; frontend `f74758af5` |
| Gate | `evidence/02-requirement-completeness-gate.md` revision `1.10` bagian 19 dan koreksi 19.12 |

**Cara membaca bukti.** Backend: verifikasi API/kontrak, pencarian kode, verifikasi proses bisnis, dan runtime bila tersedia — bukan automated test (`rules/backend/TEST_POLICY.md`). Frontend: lint, build, dan verifikasi manual kontrol interaktif. Butir yang tidak dijalankan ditulis `NOT RUN`.

## 1. Matriks requirement → desain → kontrak → task → bukti

| Requirement | Keputusan | Desain | Kontrak `0.7.0` | Task backend | Task frontend | Bukti verifikasi | Status |
|---|---|---|---|---|---|---|---|
| `FR-RWF-030` Perawat memesan Lab/Radiologi atas instruksi dokter | `RWI-DEC-114`, `153`, `168` | Backend 12.1 | API 13.1 | `BE-RWI-104` ✅ (`backend-roadmap-v2.md`; regresi `NOT RUN`) | `FE-RWI-172` | Tombol perawat tetap terkunci aman `disabled={true}` sesuai klausul gerbang `RWI-DEC-168` karena probe runtime backend 401 tanpa sesi live | 🟡 Berhenti aman / Tombol terkunci |
| `FR-RWF-031` Konsultasi Gizi lewat modul Gizi | `RWI-DEC-171` | Backend 12.3–12.5 | API 13.2, 13.3 | `BE-RWI-161`, `BE-RWI-164` | `FE-RWI-174` | Source, build, 88 test unit dokter, dan Playwright E2E PASS: pesanan lewat adapter, peminta akun dokter login (`AC-RWF-032`, `UAT-RWF-07`) | ✅ Selesai |
| `FR-RWF-032` Pesanan darah lewat modul Bank Darah | `RWI-DEC-171` | Backend 12.3–12.5 | API 13.2, 13.4 | `BE-RWI-162`, `BE-RWI-164` | `FE-RWI-174` | Source, build, 88 test unit dokter, dan Playwright E2E PASS: pesanan darah lewat adapter, komponen dari master, peminta dokter (`AC-RWF-033`, `UAT-RWF-08`) | ✅ Selesai |
| `FR-RWF-033` Status dan hasil dibaca dari modul pemilik | `RWI-DEC-171` (7) | Backend 12.11 | Endpoint baca modul pemilik (sudah ada) | — (`EXISTING / REUSE`) | `FE-RWI-173`, `FE-RWI-174` | Source, build, dan Playwright E2E PASS: daftar pesanan & hasil dibaca langsung dari modul pemilik | ✅ Selesai |
| `FR-RWF-034` Status tanggungan dan harga saat memilih pemeriksaan | `RWI-DEC-218`, `219` (menggantikan tafsiran G-05) | Backend 12.13 | API 13.2 `coverage-status` | `BE-RWI-163` | `FE-RWI-172`, `173`, `176`, `177`; Pemesanan Ruangan Bedah: `episode-rawat-inap` `FE-RWI-193` | Resolver tarif `coverage-status` berfungsi; harga berlabel "perkiraan — tagihan final di kasir"; tanpa harga tetap 120.000 atau Rp 0; Playwright E2E PASS | ✅ Selesai |
| `FR-RWF-035` Rehab Medik *placeholder*; Hemodialisa tetap | `RWI-DEC-108`; `RWI-DEC-222` (menutup `DEC-INP-019`) | `04-prd-to-mvp.md` 23.8 | — | — | `FE-RWI-179` | Nol `procedureId` buatan; kartu Rehab status "Integrasi belum tersedia" tanpa request jaringan; Playwright E2E PASS (`RWI-AC-341`, `342`) | ✅ Selesai |
| `FR-RWF-036` Aturan pemesan seragam | `RWI-DEC-171`, `188` | Backend 12.2 (`INV-RWF-20`, `21`) | API 13.2; validasi `VAL-RWF-60`, `61`, `65` | `BE-RWI-164`; Lab/Rad: `BE-RWI-104` ✅ | `FE-RWI-172`, `FE-RWI-174` | Adapter rawat inap menetapkan peminta dari dokter login atau dokter penugasan aktif; unit test dan Playwright E2E PASS | ✅ Selesai |
| `FR-RWF-037` Verifikasi dokter | `RWI-DEC-188`, `191` | Backend 12.2 (`INV-RWF-22`, `23`) | API 13.3, 13.4; validasi `VAL-RWF-63`, `64` | `BE-RWI-161`, `BE-RWI-162`; diet: `keperawatan` `BE-RWI-166` | `FE-RWI-175` | Enam sumber verifikasi instruksi dimuat mandiri; ExpectedVersion terjaga; 403 & 409 ditangani; Playwright E2E PASS | ✅ Selesai |
| `FR-RWF-038` Pesanan bukan tagihan; pesanan ganda ikut modul pemilik | `RWI-DEC-171` (5) | Backend 12.2 (`INV-RWF-24`) | API 13.2 (`confirm-duplicate`) | `BE-RWI-164` | `FE-RWI-174` | Dialog konfirmasi duplikat Bank Darah; pesanan tidak menghasilkan baris tagihan prematur | ✅ Selesai |
| `FR-RWF-070` Katalog tindakan rawat inap | `RWI-DEC-165` (1) | Backend 12.5, 12.6 | API 13.5 | `BE-RWI-160` | `FE-RWI-176` | Hook tindakan mengirim `careSetting=Inpatient` dan `audience`; resolver tarif aktif; Playwright E2E PASS (`AC-RWF-070`) | ✅ Selesai |
| `FR-RWF-081`, `082` (sisi dokter) Ringkasan operasi di ruang kerja dokter | `RWI-DEC-213` | Backend 12.13 (penanda klinis) | `episode-rawat-inap` API 11 (`post-operative-summary`) | `episode-rawat-inap` `BE-RWI-180` | `FE-RWI-178` | Penanda muncul hanya untuk kasus `Completed`; klik membuka `PostOpSummaryDrawer` baca-saja tanpa Terima/Tolak; 8 tab dokter utuh; Playwright E2E PASS | ✅ Selesai |
| `NFR-RWF-10` Pemeriksaan penugasan gagal tertutup untuk pesanan darah | `RWI-DEC-171` | — | API 13.2 | `BE-RWI-164` | — | Source dan validasi statis PASS: [BE-RWI-164](../task/report/backend/BE-RWI-164.md); bukti API/proses bisnis/runtime NOT RUN. Rencana pembuktian: Konteks penugasan tidak terbaca → ditolak (`INT-RWF-16`) | ✅ Backend selesai 5 Oktober 2026 (build `0 Error(s)`); runtime `NOT RUN`, dikecualikan atas instruksi pengguna 5 Oktober 2026 |
| `NFR-RWF-11` Daftar verifikasi tetap tampil sebagian | `RWI-DEC-188` | FE 11.4 | — | — | `FE-RWI-175` | Isolasi error per sumber: kegagalan Diet tidak menyembunyikan 5 sumber lainnya; Playwright E2E PASS | ✅ Selesai |

## 2. Definition of Done PRD → bukti

| Butir DoD (`04-prd-to-mvp.md` 23.19) | Task | Bukti |
|---|---|---|
| Gizi dan Bank Darah tidak lagi *placeholder* | `BE-RWI-164`, `FE-RWI-174` | Backend ✅ [BE-RWI-164](../task/report/backend/BE-RWI-164.md); Frontend ✅ [FE-RWI-174](../task/report/frontend/FE-RWI-174.md); Playwright E2E PASS |
| Pesanan perawat selalu punya dokter berpenugasan dan terverifikasi | `BE-RWI-161`, `162`, `164`; `FE-RWI-175` | Backend ✅ [BE-RWI-161](../task/report/backend/BE-RWI-161.md), [BE-RWI-162](../task/report/backend/BE-RWI-162.md), [BE-RWI-164](../task/report/backend/BE-RWI-164.md); Frontend ✅ [FE-RWI-175](../task/report/frontend/FE-RWI-175.md); Playwright E2E PASS |
| Alur pesanan poliklinik tidak berubah | `BE-RWI-161`, `BE-RWI-162` | Verifikasi proses bisnis regresi `AC-RWF-036`; penelusuran source ✅ (pemanggil lama tetap `NotRequired`); regresi runtime `NOT RUN` |
| Katalog rawat inap benar | `BE-RWI-160`, `FE-RWI-176` | Backend ✅ [BE-RWI-160](../task/report/backend/BE-RWI-160.md); Frontend ✅ [FE-RWI-176](../task/report/frontend/FE-RWI-176.md); Playwright E2E PASS |

## 3. Gerbang rilis implementasi

| Temuan | Task | Status |
|---|---|---|
| IMP-RWF-01, IMP-RWF-02 (Gizi, Bank Darah) | `FE-RWI-174` | ✅ Selesai / Ditutup |
| IMP-RWF-03 (Rehab Medik) | `FE-RWI-179` | ✅ Selesai / Ditutup |
| IMP-RWF-05 (Lab/Radiologi harga tetap dan katalog contoh) | `FE-RWI-173` | ✅ Selesai / Ditutup |
| IMP-RWF-06 (order tindakan "Ditanggung" bawaan dan harga `0`) — temuan perencanaan 2 Oktober 2026 | `FE-RWI-176` | ✅ Selesai / Ditutup |

## 4. Gap dan catatan

| ID | Gap | Penanganan | Pemilik |
|---|---|---|---|
| `DEC-INP-019` | Rehab Medik dibuka atau tetap *placeholder* (KK-3 vs `RWI-DEC-108`/`FR-RWF-035`) | **TERTUTUP** lewat `grill-me` 2026-10-02 (`RWI-DEC-222`: tetap *placeholder*); `FE-RWI-179` unblocked | Muhammad Hamzah |
| IMP-RWF-04 | Daftar pemicu capability map 19.9 tidak lengkap | `trace-existing-capabilities` impact scan ruang kerja dokter; bukan task delivery | Muhammad Hamzah |
| IMP-RWF-06 | Belum tercatat di gate `1.10` | Dicatat di sini; dimasukkan pada evaluasi gate berikutnya | Muhammad Hamzah |
| — | Regresi `BE-RWI-104` `NOT RUN`, padahal `RWI-DEC-168` mensyaratkan "terbukti berjalan" | Langkah pertama `FE-RWI-172` | Muhammad Hamzah |
| — | Register 🟡 dan kartu ✅ berbeda untuk `FE-RWI-143` s.d. `146` di `frontend-roadmap-v2.md` | Dicatat untuk pemilik roadmap itu; tidak diubah di sini | Muhammad Hamzah |
| — | `BE-RWI-128` dipakai dua task (`backend-roadmap-v2.md` dan `integrasi-billing/roadmap/backend-roadmap.md`) | Dicatat pada manifest modul (`task_id_integrity`) | Pemilik peta modul |

Tidak adanya automated test backend **bukan** gap (`rules/backend/TEST_POLICY.md`).

## 5. Bukti implementasi backend — 5 Oktober 2026

BE-RWI-160–164 selesai dalam batas **coding saja** sesuai instruksi user. Satu laporan canonical dibuat per task di task/report/backend; QBE Strict ExplicitFiles 25 source PASS, parsing C# tanpa error dan 59 pemeriksaan statis PASS. Sembilan endpoint yang ditambah/disentuh memiliki permission yang cocok dengan metadata controller. Build, migration, database, API dan runtime NOT RUN; tidak menandai DoD PRD atau readiness rilis sebagai lulus.

R10 dan R11 menunggu user membuat/review/menerapkan migration bagi enam field verifikasi model order Gizi/darah. Migration AddRawatInapFinishing yang berhasil user terapkan sebelumnya tidak mencakup enam field baru ini. Enum GziInstructionVerificationStatus sudah tersedia untuk Keperawatan BE-RWI-166; implementasi diet dan task frontend tetap scope terpisah. Tautan laporan menjadi bukti source; skenario UAT dan acceptance sebelumnya tetap tercantum pada kartu roadmap dan laporan, belum diuji runtime.

**Pembaruan migration 2026-10-05:** `20261005050735_AddNutritionAndBloodInstructionVerification` beserta Designer dan model snapshot sudah tersedia di workspace saat pemeriksaan ulang; tidak dibuat ulang atau diubah oleh agent. Review statis membuktikan enam kolom, empat index dan dua FK Restrict, serta target model Designer sama dengan snapshot. Bukti terbaru: laporan BE-RWI-161/162/164 bagian 7.1 dan catatan akhir roadmap backend. Catatan sebelumnya tentang file belum dibuat dipertahankan sebagai riwayat. Penerapan database belum dibuktikan; agent tidak menjalankan build, dotnet ef atau database update. Preferensi user agar task schema berikutnya menyertakan file migration dicatat dalam AGENTS.md.

**Pembaruan bukti 5 Oktober 2026 (penandaan selesai).** Atas instruksi pengguna, `BE-RWI-160`–`164` ✅: build terintegrasi `0 Error(s)`, migration R10/R11 diterapkan ke database development, setiap acceptance criteria dicocokkan ulang terhadap source pada HEAD `0a108994`. Requirement yang masih punya task frontend tetap 🟡. Uji API dan runtime `NOT RUN`, dikecualikan atas instruksi pengguna 5 Oktober 2026.


## Bukti akhir implementasi frontend — 6 Oktober 2026

| Task | Status dan bukti |
| --- | --- |
| FE-RWI-172 | 🟡 Prasyarat runtime BE-RWI-104 belum terbukti; probe Lab HTTP 401 tanpa sesi. Tombol perawat tetap terkunci aman `disabled={true}` sesuai klausul gerbang `RWI-DEC-168`. [Laporan](../task/report/frontend/FE-RWI-172.md) |
| FE-RWI-173 | ✅ Selesai — Form Lab/Radiologi dokter tanpa harga tetap 120.000 dan tanpa katalog contoh (IMP-RWF-05 ditutup). Lint exit 0, build lulus, Playwright E2E PASS. [Laporan](../task/report/frontend/FE-RWI-173.md) |
| FE-RWI-174 | ✅ Selesai — Konsultasi Gizi & Bank Darah lewat adapter Rawat Inap (IMP-RWF-01 & 02 ditutup). Lint exit 0, build lulus, Playwright E2E PASS. [Laporan](../task/report/frontend/FE-RWI-174.md) |
| FE-RWI-175 | ✅ Selesai — Enam sumber instruksi diverifikasi secara terpisah dengan ExpectedVersion; isolasi error diet; pesan 403 & 409. Lint exit 0, build lulus, Playwright E2E PASS. [Laporan](../task/report/frontend/FE-RWI-175.md) |
| FE-RWI-176 | ✅ Selesai — Katalog tindakan rawat inap dengan perkiraan harga, tanpa data karangan (IMP-RWF-06 ditutup). Lint exit 0, build lulus, Playwright E2E PASS. [Laporan](../task/report/frontend/FE-RWI-176.md) |
| FE-RWI-177 | ✅ Selesai — Harga obat di tab Resep berlabel "perkiraan — tagihan final di kasir"; guard IsCoverageApplicable. Lint exit 0, build lulus, Playwright E2E PASS. [Laporan](../task/report/frontend/FE-RWI-177.md) |
| FE-RWI-178 | ✅ Selesai — Penanda "Pasca operasi" membuka ringkasan operasi baca-saja PostOpSummaryDrawer tanpa tombol Terima/Tolak. 8 tab dokter utuh. Lint exit 0, build lulus, Playwright E2E PASS. [Laporan](../task/report/frontend/FE-RWI-178.md) |
| FE-RWI-179 | ✅ Selesai — Rehab Medik di tab Penunjang dokter status "Integrasi belum tersedia" tanpa form order dan nol request jaringan (IMP-RWF-03 dicabut). Lint exit 0, build lulus, Playwright E2E PASS. [Laporan](../task/report/frontend/FE-RWI-179.md) |

### Ringkasan Validasi Otomatis & Status DoD — 6 Oktober 2026

1. **Lint Errors:** `npm.cmd run lint:errors` — PASS (exit 0). 0 lint error.
2. **Production Build:** `npm.cmd run build` — PASS (exit 0). Turbopack standalone output sukses penuh.
3. **Unit Tests:** `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-*.test.mjs ...` — PASS (88/88 test files dokter rawat inap PASS).
4. **Playwright E2E:** `tests/e2e/inpatient-doctor-finishing.spec.mjs` — PASS (11/11 scenarios) mencakup seluruh skenario penunjang, katalog, harga, verifikasi 6 sumber (403/409), dan penanda pasca operasi.
5. **Klausul Gerbang FE-RWI-172:** Sesuai `RWI-DEC-168`, tombol order perawat tetap terkunci aman `disabled={true}` dan task berhenti aman sebagai 🟡 sampai modul Lab live menyediakan sesi UAT interaktif.

