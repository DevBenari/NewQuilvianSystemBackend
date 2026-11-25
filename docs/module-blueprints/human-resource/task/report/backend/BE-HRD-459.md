# Laporan Perubahan Backend — `BE-HRD-459`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-459` |
| Nomor lama | `BE-HRD-449` — dinomori ulang saat merge `QuilvianIntegrationBackend` 23 November 2026; nomor lama kini milik amandemen `S-A7-V2` (lihat `blueprint-manifest.md` revisi `69`) |
| Judul | Panel Kontrak Berakhir memakai `ContractRenewalBuckets` |
| Slice | `S-PKB` — Paket B Navigasi SDM / `MVP-75` · `EPIC HRD-125` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 31, kartu `BE-HRD-459` |
| Trace | `FR-PKB-02`; `HRD-DEC-245`; `INT-HR-115`; `AT-HRD-PKB-04` |
| Contract version | `v30` — API 36.1 (`contractExpiry`); State 34.1 — **approved** 23 November 2026 |
| Dependency | `BE-HRD-457` 🟡 (kode ada; AC 1 menunggu uji DB dev), `BE-HRD-458` ✅ |
| Klasifikasi | `LIGHT` |
| Task mode | `BACKEND` — instruksi pengguna 23 November 2026 |
| Target tulis | `WorkforceCore/Services/HrdWorkforceDashboardService.cs`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `7e4c3774` (branch `MHamzah`); source sudah di-commit pengguna di `9062bd61` |
| Tanggal | 23 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpetakan ke source; build 0 error, QBE Strict `PASS`; uji API runtime 23 November 2026 `PASS` (`AT-HRD-PKB-04`) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkforceCore` (membaca `LifecycleManagement`) |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-SVC-001` |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Dashboard SDM perlu menampilkan kontrak yang segera berakhir dengan angka yang sama persis dengan layar Kontrak, supaya Rina tidak melihat dua angka berbeda untuk hal yang sama.

## 2. Proses bisnis

1. Saat `/summary` dipanggil, panel Kontrak Berakhir dihitung dengan predikat `ContractRenewalBuckets` yang sama dengan layar Kontrak dan tanggal acuan yang sama (`ReferenceDate()`, tanggal UTC).
2. Hasil: total berakhir dalam 60 hari, H-60, H-14, H-7, menunggu keputusan, lewat jatuh tempo.
3. Bila salah satu kueri gagal, seluruh `/summary` gagal (`500`); tidak ada angka sebagian yang dikirim sebagai angka lengkap.

**Contoh.** Pada 20 Desember 2026 layar Kontrak menunjukkan 12 kontrak segera berakhir (5 H-60, 3 H-14, 4 H-7). Dashboard menunjukkan `totalWithin60Days = 12`, `h60Count = 5`, `h14Count = 3`, `h7Count = 4`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`ContractRenewalBuckets.cs`, `HrdEmploymentContractService.cs` (`GetContractSummaryAsync`, `GetRenewalSummaryAsync`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkforceCore/Services/HrdWorkforceDashboardService.cs` | `GetContractExpiryAsync` memakai `ExpiringWithinWindow`, `Stage(H60/H14/H7)`, `PendingDecisionPredicate`, `OverduePredicate` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Ruas `contractExpiry.*` pada `/summary` |
| Database | Tidak ada |
| Keamanan/Auth | `NOT APPLICABLE` (dijaga hak dashboard) |

**Delta:** `operationalDate` pada respons adalah tanggal WIB, sedangkan panel kontrak memakai tanggal acuan layar Kontrak (tanggal UTC). Antara 00.00–07.00 WIB keduanya berbeda satu hari; panel kontrak sengaja mengikuti layar Kontrak agar angkanya identik (`HRD-DEC-245`).

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Workforce Core / Workforce Dashboard

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/workforce-dashboard/summary` | Ruas `contractExpiry` | `HrdWorkforceDashboard : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` penuh Paket B | 0 error; tanpa warning di berkas task | `PASS` | Log build 23 November 2026 |
| QBE Strict (19 berkas) | `PASS` | `PASS` | Keluaran checker |
| Penelusuran kesamaan dengan layar Kontrak | Predikat dan tanggal acuan identik; layar Kontrak tanpa filter lokasi = dashboard | `PASS` | `GetContractExpiryAsync` vs `HrdEmploymentContractService` |
| Uji API runtime di DB dev (`QuilvianNewDevHamzah`), 23 November 2026 — backend `9062bd61` di port lokal 5199, akun superadmin + akun non-admin: `contractExpiry` vs `employment-contracts/summary` dan `renewal-summary` (`AT-HRD-PKB-04`) | Identik (seluruhnya 0; DB dev: 34 pegawai aktif; 0 kontrak, 0 lisensi, 0 SPK/RKK, 0 sanksi, 0 evaluasi kandidat) | `PASS` | Skrip uji API (27 skenario: 26 `PASS`, 1 gagal karena login akun uji atasan ditolak `401` — masalah akun, bukan kode) |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Angka panel sama persis dengan `renewal-summary` dan `ExpiringSoonCount` pada data yang sama (`AT-HRD-PKB-04`) | Terpenuhi | Satu sumber predikat dan tanggal acuan |
| 2. Kegagalan kueri menggagalkan seluruh `/summary` (`500`) | Terpenuhi | Tidak ada `try/catch` per panel; pengecualian diteruskan ke middleware galat |
| DoD: laporan tracked, tanda status roadmap | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada |
| Masalah yang diketahui | Delta tanggal acuan (lihat 3.3) |
| Risiko tersisa | Rendah |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Source sudah di-commit pengguna (`9062bd61`) |
| Langkah berikutnya | `BE-HRD-461` |
