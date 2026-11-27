# Laporan Perubahan Backend — `BE-HRD-389`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-389` |
| Judul | Guard server mutasi langsung field sensitif profil |
| Slice | `S-FIN-M04` / Profil (alias `FIN-HRD-04-BE-003`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.2, kartu `BE-HRD-389` |
| Trace | `HRD-DEC-052`; instruksi Menu 04 `FIN-HRD-04-BE-003`; `UAT-04-08`, `09`, `42` |
| Contract version | Kontrak Data Induk existing; tidak ada endpoint baru |
| Dependency | — |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `MasterData/Workforce/Controllers/*.cs`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpetakan ke source; build dan QBE lulus; uji API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `MasterData/Workforce` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-VAL-001, QBE-LOG-001, QBE-API-001 |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

`HRD-DEC-052` menetapkan NIK/paspor, NPWP, dan status perkawinan hanya boleh berubah lewat Menu 06 (Pengajuan Perubahan Data) dengan bukti dan verifikator berbeda. Guard `409` sudah ada di tiga `PUT`, tetapi pembandingnya bocor:

1. **Huruf paspor dibuang sebelum dibandingkan.** Paspor `AB123456` diganti `CD123456` dianggap sama (`123456`), sehingga lolos.
2. **Jenis identitas tidak dikunci.** KTP bisa diganti Paspor dengan nomor yang sama.
3. **Tidak ada jejak field apa yang berubah** pada perubahan non-sensitif; log hanya mencatat "berhasil diperbarui".

## 2. Proses bisnis

1. Petugas membuka form Data Induk Pegawai/Dokter/Pengguna Eksternal dan menyimpan (`PUT` penuh).
2. Server membandingkan nilai lama dan baru setelah normalisasi resolver identitas:
   - nomor identitas atau jenis identitas berubah → `409 SENSITIVE_IDENTITY_FIELD_LOCKED`, arahan ke Menu 06;
   - status perkawinan berubah (kecuali dari `Unknown`, yaitu pengisian pertama) → `409 SENSITIVE_MARITAL_STATUS_LOCKED`;
   - NPWP Pengguna Eksternal berubah → `409 SENSITIVE_TAX_NUMBER_LOCKED`.
3. Nilai sama → lolos; identitas tidak ditulis ulang oleh `PUT`.
4. Perubahan non-sensitif (alamat, kontak, dll.) tersimpan dengan `UpdateBy`/`UpdateDateTime`, dan log aplikasi mencatat **nama** kolom yang berubah (`ChangedFields`) beserta aktornya. Nilai kolom tidak disalin ke log supaya data pribadi tidak bocor ke log.

**Contoh.** Perawat B (paspor `AB123456`) mengirim `PUT` dengan nomor `CD123456` → `409`. Mengirim `PUT` dengan nomor sama dan alamat baru → `200`, log berisi `ChangedFields: ["Address"]`.

**Jalur tidak normal.** Dokter/Pengguna Eksternal lama yang belum punya nomor identitas boleh mengisinya sekali; nomor wajib valid dan tidak boleh milik profil lain (`BE-HRD-387`).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Tiga `PUT` master, `PATCH /{id}/status`, `PATCH /{id}/user-account/*` (tidak menyentuh field sensitif), `WorkforceProfileSyncService` (jalur apply tiket layanan — jalur resmi pasca-persetujuan, bukan bypass), `EmployeeProfileChangeService` (Menu 06).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `EmployeeController.cs` | Pembanding identitas memakai normalisasi resolver + kunci jenis identitas; `PUT` tidak lagi menulis ulang `IdentityType`/`IdentityNumber`; audit `ChangedFields` |
| `DoctorController.cs`, `ExternalUserController.cs` | Sama; pengisian identitas pertama kali divalidasi dan diperiksa pemiliknya; audit `ChangedFields` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route/DTO tetap; perubahan jenis identitas kini juga `409` |
| Database | Tanpa migration |
| Keamanan/Auth | Menutup bypass perubahan identitas paspor dan jenis identitas; tidak ada data pribadi baru di log |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Master Data / Employee

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `/api/v1/corporate/human-resource/master-data/employees/{id}` | Ubah data non-sensitif; field sensitif `409` | `Employee : Update` |

#### Corporate / Human Resource / Master Data / Doctor

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `/api/v1/corporate/human-resource/master-data/doctors/{id}` (`/admin/{id}`) | Idem | `Doctor : Update` |

#### Corporate / Human Resource / Master Data / External User

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `/api/v1/corporate/human-resource/master-data/external-users/{id}` | Idem, termasuk kunci NPWP | `ExternalUser : Update` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Berhasil, 0 error, 608 warning (himpunan warning identik dengan sebelum perubahan) | `PASS` | Keluaran build |
| QBE Strict berkas Menu 04 | 20 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: paspor `AB123456` → `CD123456` | `409` (dulu lolos) | `PASS` (penelusuran) | normalisasi resolver di guard |
| Penelusuran: jenis `KTP` → `Paspor`, nomor sama | `409` | `PASS` (penelusuran) | `identityTypeChanged` |
| Penelusuran: `PUT` nilai sama + alamat baru | `200`, `ChangedFields` berisi `Address` | `PASS` (penelusuran) | `CaptureChangedFieldNames` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE`.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Perubahan field sensitif lewat PUT ditolak `409` | Terpenuhi | Guard identitas, jenis identitas, status perkawinan, NPWP di tiga `PUT` |
| (2) Field non-sensitif tetap dapat diubah dan diaudit | Terpenuhi | `UpdateBy`/`UpdateDateTime` + log `ChangedFields` beraktor |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah yang diketahui | `HRD-DEC-052` menyebut audit nilai lama/baru; yang tercatat nama kolom dan aktor, nilai tidak disalin ke log demi privasi. Riwayat nilai lengkap tersedia lewat jalur Menu 06 |
| Risiko tersisa | Bukti runtime belum ada |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat `BE-HRD-402` |
| Langkah berikutnya | `BE-HRD-390` |
