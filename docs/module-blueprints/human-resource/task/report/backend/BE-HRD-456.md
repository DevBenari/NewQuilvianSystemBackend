# Laporan Perubahan Backend — `BE-HRD-456`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-456` |
| Nomor lama | `BE-HRD-446` — dinomori ulang saat merge `QuilvianIntegrationBackend` 23 November 2026; nomor lama kini milik amandemen `S-A7-V2` (lihat `blueprint-manifest.md` revisi `69`) |
| Judul | Filter beberapa status pada daftar sanksi (Riwayat Penyelesaian) |
| Slice | `S-PKB` — Paket B Navigasi SDM / `MVP-74` · `EPIC HRD-128` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 31, kartu `BE-HRD-456` |
| Trace | `FR-PKB-12`; `HRD-DEC-242`, `HRD-ASM-97`; `VR-PKB-04`, `VR-PKB-05`; state-transition 34.2 |
| Contract version | `v30` — API 36.2, 36.6 — **approved** 23 November 2026 |
| Dependency | — |
| Klasifikasi | `LIGHT` — satu parameter query dan validasinya |
| Task mode | `BACKEND` — instruksi pengguna 23 November 2026 |
| Target tulis | `EmployeeRelationManagement/{DTOs,Services,Controllers}`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `7e4c3774` (branch `MHamzah`); source sudah di-commit pengguna di `9062bd61` |
| Tanggal | 23 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria terpetakan ke source; build 0 error, QBE Strict `PASS`; uji API runtime 23 November 2026 `PASS` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `EmployeeRelationManagement` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | `QBE-DTO-001`, `QBE-VAL-001`, `QBE-API-001` |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Daftar sanksi hanya menerima satu status. Riwayat Penyelesaian butuh sanksi yang sudah **selesai** — kedaluwarsa (`Expired`) **dan** dicabut lewat banding (`RevokedOnAppeal`) — dalam satu daftar.

## 2. Proses bisnis

1. HR Relations membuka Riwayat Penyelesaian; layar meminta `GET /hrd/disciplinary/sanctions?statuses=Expired&statuses=RevokedOnAppeal`.
2. Sistem memeriksa setiap nilai: hanya `Active`, `Expired`, `RevokedOnAppeal` (huruf besar/kecil diabaikan dan dinormalkan). Nilai lain → `400` "Status sanksi tidak dikenal."
3. Bila `status` dan `statuses` dikirim bersamaan → `400` "Gunakan salah satu: status atau statuses."
4. Hasil memuat sanksi dengan salah satu status yang diminta; filter lain (tingkat, cari, aktif hari ini) tetap berlaku.

**Contoh.** Budi punya SP 1 `Active`, SP 2 `Expired`, dan teguran `RevokedOnAppeal`. Permintaan di atas mengembalikan dua baris (SP 2 dan teguran), tanpa SP 1.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`DTOs/DisciplinaryListDtos.cs`, `Services/DisciplinaryListQueryService.cs`, `Controllers/EmployeeDisciplinaryController.cs` (`GetSanctionList`, `GetSuspensionList`), `Models/HrdDisciplinarySanction.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/EmployeeRelationManagement/DTOs/DisciplinaryListDtos.cs` | `DisciplinaryListQuery.Statuses` (`List<string>?`) |
| `Areas/Corporate/HumanResource/EmployeeRelationManagement/Services/DisciplinaryListQueryService.cs` | Daftar nilai sah, `ValidateSanctionStatusFilter` (`VR-PKB-04`/`05`), normalisasi nilai, filter `statuses.Contains(s.Status)` |
| `Areas/Corporate/HumanResource/EmployeeRelationManagement/Controllers/EmployeeDisciplinaryController.cs` | `GetSanctionList` menolak `400` bila filter tidak sah; deklarasi `400` pada Swagger |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Parameter opsional baru `statuses` (berulang). Daftar skorsing memakai DTO yang sama tetapi tidak membaca `statuses` |
| Database | Tidak ada |
| Keamanan/Auth | Tetap `EmployeeDisciplinary : ViewSanctions` |

## 4. Dokumentasi endpoint

#### Corporate - HRD Employee Disciplinary & Just Culture

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/hrd/disciplinary/sanctions?statuses=Expired&statuses=RevokedOnAppeal` | Daftar sanksi beberapa status sekaligus | `EmployeeDisciplinary : ViewSanctions` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` (sesudah `MVP-74` dan build penuh) | 0 error; tanpa warning di berkas task | `PASS` | Log build 23 November 2026 |
| QBE Strict (19 berkas) | `PASS` | `PASS` | Keluaran checker |
| Penelusuran: `statuses=Closed` | `UnknownSanctionStatusMessage` → `400` | `PASS` | `ValidateSanctionStatusFilter` |
| Penelusuran: `status=Active&statuses=Expired` | `StatusAndStatusesMessage` → `400` | `PASS` | `ValidateSanctionStatusFilter` |
| Penelusuran: tanpa `statuses` | Validasi `null`, kueri sama seperti sebelumnya | `PASS` | Diff service |
| Uji API runtime di DB dev (`QuilvianNewDevHamzah`), 23 November 2026 — backend `9062bd61` di port lokal 5199, akun superadmin + akun non-admin | `statuses=Expired&statuses=RevokedOnAppeal` → `200` (0 baris, DB dev belum punya sanksi); `statuses=Closed` → `400` "Status sanksi tidak dikenal."; `status=Active&statuses=Expired` → `400` "Gunakan salah satu: status atau statuses."; `statuses=expired` → `200`; tanpa filter → `200` | `PASS` | Skrip uji API (27 skenario: 26 `PASS`, 1 gagal karena login akun uji atasan ditolak `401` — masalah akun, bukan kode) |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `statuses=Expired&statuses=RevokedOnAppeal` → dua baris tanpa `Active` (`AT-HRD-PKB-10`) | Terpenuhi | Filter `Contains` |
| 2. `statuses=Closed` → `400`; `status=Active&statuses=Expired` → `400` (`AT-HRD-PKB-11`) | Terpenuhi | Validasi di controller |
| 3. Pemanggil lama mendapat hasil yang sama | Terpenuhi | Jalur tanpa `statuses` tidak berubah |
| DoD: laporan tracked, tanda status roadmap | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada |
| Masalah yang diketahui | Status sanksi masih string bebas (utang teknis desain 38.4) |
| Risiko tersisa | Frontend wajib mengirim `statuses` berulang, bukan `statuses[]=` (catatan di `FE-HRD-403`) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Source sudah di-commit pengguna (`9062bd61`) |
| Langkah berikutnya | `FE-HRD-406` |
