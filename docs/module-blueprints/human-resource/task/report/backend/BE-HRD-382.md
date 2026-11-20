# Laporan Perubahan Backend — `BE-HRD-382`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-382` |
| Judul | Surat Tugas multi-pegawai: rancangan data peserta dan migration |
| Slice | `S-FIN-ADM` / Surat Tugas (alias `ST-IMP (data peserta)`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.1, kartu `BE-HRD-382` |
| Trace | `ADM-Q-03` = A (keputusan pemilik 20 November 2026); baseline `BE-HRD-109` |
| Contract version | Kontrak Administrasi Kepegawaian (`docs/update/administrasi-kepegawaian`); endpoint baru dicatat sebagai delta |
| Dependency | `{ADM-Q-03}` — ditutup 20 November 2026 |
| Klasifikasi | `HEAVY` — entity + migration baru |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `HrdAssignmentLetter`, `HrdAssignmentLetterParticipant`, konfigurasinya, migration `20261120093000`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `a7c37c56` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria terpetakan ke source; build, QBE lulus; Migration `20261120093000_AddHrdAssignmentLetter` ditulis, **belum dijalankan** — endpoint Surat Tugas dan registri baru berfungsi sesudah migration dijalankan |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `HrServiceManagement` |
| Keberlakuan | `NEW CODE` (Surat Tugas, registri) + `TOUCHED LEGACY` (Surat Keterangan) |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

`HrdOfficialDutyRequest` (dinas luar presensi) hanya menyimpan satu `WorkforceProfileId`; tidak ada aggregate Surat Tugas yang memuat banyak pegawai. Pemilik memutuskan `ADM-Q-03` pilihan A: satu surat dengan tabel peserta.

## 2. Proses bisnis

1. Aggregate baru `HrdAssignmentLetter` (prefix registry `Hrd`) menyimpan kegiatan, dasar, uraian tugas, tujuan, periode, unit pengaju, status, dan aktor tiap tahap.
2. Tabel anak `HrdAssignmentLetterParticipant`: satu baris per pegawai (unik per surat), snapshot nama/kode/jabatan/unit, peran dalam tim, peringatan benturan, dan tautan ke penugasan presensi saat terbit.
3. Peserta harus aktif dan (tanpa penanda `AssignmentLetter : ManageAllUnits`) berada di unit yang dipimpin pengaju; di luar itu `403`, nonaktif `422`.
4. Surat Tugas tidak mengubah jabatan, gaji, kontrak, atau kewenangan klinis. Saat terbit, setiap peserta mendapat satu `HrdOfficialDutyRequest` disetujui lewat service presensi existing (hari tugas tercatat OnDuty).

**Contoh.** Kepala IGD menugaskan 1 dokter dan 2 perawat IGD mengawal rujukan 2 hari → satu surat, tiga baris peserta.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`HrdOfficialDutyRequest`, `OfficialDutyRequestService`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (prefix `Hrd`), pola migration manual `20261118100000`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/HrServiceManagement/Models/HrdAssignmentLetter.cs` | Entity baru |
| `Areas/Corporate/HumanResource/HrServiceManagement/Models/HrdAssignmentLetterParticipant.cs` | Entity baru |
| `Repositories/Configurations/Corporate/HumanResource/HrServiceManagement/HrdAssignmentLetterConfiguration.cs` | Konfigurasi + indeks unik (nomor, hash, peserta per surat) |
| `Migrations/20261120093000_AddHrdAssignmentLetter.cs` | Migration manual idempoten (belum dijalankan) |
| `Areas/Corporate/HumanResource/HrServiceManagement/Services/AssignmentLetterService.cs` | Validasi peserta (`ResolveParticipantsAsync`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru (lihat `BE-HRD-383`) |
| Database | 2 tabel baru; **migration belum dijalankan** |
| Keamanan/Auth | Penanda baru `AssignmentLetter : ManageAllUnits` |

## 4. Dokumentasi endpoint

#### HR Services - Assignment Letters (Surat Tugas)

| Method | Path | Kegunaan | Hak akses |
| `GET` | `/api/v1/corporate/human-resource/assignment-letters/{id}/document` | Naskah terbit (HTML) | `AssignmentLetter : Read` |
| `POST` | `/api/v1/corporate/human-resource/assignment-letters` | Susun draf | `AssignmentLetter : Create` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error; himpunan warning (tanpa nomor baris) sama dengan sebelum perubahan — tambahan 6 instans berasal dari commit pengguna `a7c37c56` pada `HrdInitialPlacementBatchService.cs`, bukan dari perubahan ini | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (18 berkas slice Administrasi Kepegawaian) | 18 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: dua peserta atau lebih | Tersimpan sebagai baris anak, dimuat ulang lewat detail | `PASS` | `CreateAsync`/`GetByIdAsync` |
| Penelusuran: peserta nonaktif / di luar unit | `422` / `403` | `PASS` | `ResolveParticipantsAsync` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Satu pengajuan menyimpan dua pegawai atau lebih dan utuh setelah dimuat ulang | Terpenuhi | tabel peserta + indeks unik |
| (2) Peserta nonaktif/di luar unit pengaju ditolak server | Terpenuhi | validasi peserta |
| (3) Migration ditulis sesudah keputusan, dijalankan dengan izin terpisah | Terpenuhi | migration `20261120093000` ditulis, belum dijalankan |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Wajib sebelum dipakai | Jalankan migration `20261120093000_AddHrdAssignmentLetter` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |
