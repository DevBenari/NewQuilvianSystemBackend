# Laporan Perubahan Backend — `BE-RWI-094`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-094` |
| Judul | Jenis catatan pada CPPT — migration R3 |
| Slice | Gelombang 2 — `DOK-V2-1` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-094` |
| Trace | `FR-DOK-085`; `RWI-DEC-140`, `RWI-DEC-141`; `VAL-DOK-59`, `VAL-DOK-59a` |
| Contract version | `0.6.0` |
| Dependency | `BE-RWI-079` [BE-INP] — field assignment purpose tersedia pada working tree aktif |
| Klasifikasi | `HEAVY` — schema, migration, snapshot, API, validasi profesi, dan filter timeline |
| Task mode | `BACKEND` |
| Target tulis | `ClinicalManagement`, configuration EF, migration R3, dan dokumentasi delivery |
| Model | GPT-5 |
| Commit backend saat dikerjakan | `36db5e6d1f1e6ee8b3a3dce8aa6c9d4adcc8060a`, branch `MHamzah` |
| Tanggal | 2026-09-16 |
| Status | ✅ **SELESAI 16 September 2026** atas instruksi pemilik untuk menandai selesai tanpa build maupun eksekusi migration |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area/module | `HealthServices / ClinicalManagement` |
| Registry/prefix | Modul terdaftar `ACTIVE`; tidak ada entity atau prefix baru |
| QBE relevan | `QBE-ENUM-001`, `QBE-CFG-001`, `QBE-DB-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-VAL-001` |
| Kepemilikan tabel | `TrxPatientIntegratedProgressNote` tetap milik `ClinicalManagement`; tidak dibuat tabel tandingan pada `InPatientManagement` |
| Otorisasi database | Hanya berkas migration dibuat. Migration **tidak dijalankan** ke database mana pun |

## 1. Masalah yang diperbaiki

Sebelumnya CPPT hanya menyimpan profesi penulis. Itu tidak cukup untuk membedakan SOAP
keperawatan, catatan keperawatan naratif, catatan perkembangan dokter, dan catatan profesi lain.
Selain itu, payload `ProfessionType` tidak boleh dipercaya untuk memutuskan jenis catatan karena
klien dapat mengaku sebagai profesi lain.

## 2. Proses bisnis dan aturan data

1. Backend menemukan profesi klinis dari relasi akun ke dokter, pegawai, atau pengguna eksternal.
2. Akun tanpa relasi profesi klinis aktif ditolak `403` (`VAL-DOK-59a`).
3. `NoteKind` divalidasi terhadap profesi akun: dokter hanya `PhysicianNote`; perawat/bidan hanya
   `NursingSoap` atau `NursingNarrative`; profesi klinis lain hanya `OtherProfessionNote`.
4. `Unspecified` hanya untuk entri legacy. Nilai itu tidak dapat dipilih untuk catatan baru.
5. `ProviderUserId`, `ProfessionType`, dan `ProfessionName` pada create berasal dari server.
6. Timeline dapat disaring dengan query `noteKind`.

## 3. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `CpptNoteKind.cs` | Enum stabil: `Unspecified=0`, `PhysicianNote=1`, `NursingSoap=2`, `NursingNarrative=3`, `OtherProfessionNote=4` |
| `TrxPatientIntegratedProgressNote.cs` | Properti persisted `NoteKind` dengan default CLR `Unspecified` |
| `TrxPatientIntegratedProgressNoteConfiguration.cs` | Konversi integer, default `0`, required, dan indeks episode-kind-time |
| `CpptNoteKindPolicy.cs` | Pemetaan jenis sah per profesi, default untuk request kosong, dan larangan `Unspecified` pada data baru |
| `InpatientClinicalContextService.cs` | Resolver profesi klinis dari relasi akun; payload bukan sumber kewenangan |
| `PatientIntegratedProgressNoteController.cs` | Validasi create/update, author server-side, jenis catatan response, dan filter timeline |
| `PatientIntegratedProgressNoteDtos.cs` | Field `NoteKind` dan labelnya pada request/response |
| `20260916002000_AddCpptNoteKind.cs` | Migration R3: kolom `integer NOT NULL DEFAULT 0` dan indeks |
| `ApplicationDbContextModelSnapshot.cs` | Snapshot kolom dan indeks R3 |

Nama indeks: `IX_TrxPatientIntegratedProgressNote_Episode_Kind_Time` pada
`(InpEpisodeId, NoteKind, NoteDateTime)`.

## 4. Dokumentasi endpoint

Base path: `/api/v1/health-services/clinical-management/patient-integrated-progress-notes`.

| Method | Path | Perubahan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Menerima `NoteKind`; profesi dan author diturunkan dari akun; mismatch `400`, relasi profesi tidak ada `403` | `PatientIntegratedProgressNote : Create` |
| `POST` | `/from-consultation/{consultationId}` | Menyimpan jenis `PhysicianNote` dan memastikan akun berprofesi dokter | `PatientIntegratedProgressNote : Create` |
| `PUT` | `/{id}` | Mempertahankan `Unspecified` legacy bila request tidak mengubahnya; nilai baru tetap divalidasi terhadap profesi akun | `PatientIntegratedProgressNote : Update` |
| `GET` | `/episodes/{episodeId}` | Menambah filter `noteKind` dan mengembalikan `NoteKind` beserta label | `PatientIntegratedProgressNote : Read` |

## 5. Dampak database dan rollback

- `Up`: menambah kolom dengan default `0`; tidak melakukan backfill tebakan.
- `Up`: membuat indeks sesuai data dictionary.
- `Down`: menolak rollback bila ada baris dengan `NoteKind <> 0`, agar klasifikasi klinis yang
  sudah ditulis tidak hilang diam-diam.
- Migration dan SQL **tidak dieksekusi**. Tidak ada koneksi atau perubahan database.

## 6. Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| QBE checker `Strict`, working tree | **PASS** — 17 berkas, nol finding |
| `git diff --check` | **PASS** — tidak ada whitespace error |
| Konsistensi model/config/migration/snapshot | **PASS statis** — nama kolom, default `0`, tipe integer, dan nama indeks sama |
| Sumber profesi | **PASS statis** — resolver membaca relasi akun; controller tidak memakai payload sebagai sumber keputusan |
| Legacy | **PASS statis** — migration hanya default `Unspecified`; tidak ada SQL backfill jenis |
| Filter timeline | **PASS statis** — query `noteKind` diterapkan sebelum paging |
| `dotnet build` | **NOT RUN — instruksi eksplisit pengguna** |
| `dotnet ef database update` / Postgres sekali pakai | **NOT RUN — tidak ada otorisasi eksekusi database dan pengguna meminta implementasi tanpa build** |
| Uji runtime API | **NOT RUN** |

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 — kolom `NoteKind` ada | Terpenuhi pada source migration/model | Model, config, migration, snapshot |
| AC-2 — kombinasi profesi/jenis divalidasi | Terpenuhi secara statis | Resolver account profession dan `SahUntukCatatanBaru` |
| AC-3 — legacy `Unspecified` tanpa tebakan | Terpenuhi pada migration | Default `0`, tanpa update/backfill |
| AC-4 — timeline dapat difilter | Terpenuhi secara statis | Filter query `noteKind` dan indeks pendukung |
| Laporan, roadmap, traceability | Terpenuhi | Laporan ini dan pembaruan dokumen delivery |

## 8. Catatan penutup

Task ditandai selesai sesuai instruksi pemilik. Bukti kompilasi, penerapan migration, dan perilaku
runtime tetap menunggu verifikasi mandiri pemilik. Tidak ada operasi Git atau deployment.
