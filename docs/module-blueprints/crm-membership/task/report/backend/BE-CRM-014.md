# Laporan Perubahan Backend — `BE-CRM-014`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-CRM-014` |
| Judul | Lookup dan status aktif |
| Slice | `MVP-1`, `EPIC CRM-02` dan `EPIC CRM-05` |
| Roadmap | `docs/module-blueprints/crm-membership/roadmap/backend-roadmap.md` § `BE-CRM-014` |
| Trace | `FR-CRM-014`; `CRM-DEC-010`; `VAL-22`; J1 `03-domain-architecture.md`; `03-frontend-architecture.md` §5 (NIK/No. Telp tersamar) |
| Contract version | API `v2`, `approved` |
| Dependency | `BE-CRM-010` ✅ |
| Klasifikasi | `LIGHT`: dua kueri baca di service, dua endpoint, empat DTO, 7 uji |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/PatientManagement/`, `Tests/...PatientManagementTests/`, laporan ini |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `83ef27bd` + perubahan yang belum di-commit |
| Tanggal | 2026-10-13 |
| Status | Selesai |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `HealthServices` / `PatientManagement` / `Membership` (`Pat`, `ACTIVE`) |
| Keberlakuan | `NEW CODE` (kueri di service, DTO, aksi). `TOUCHED LEGACY`: `PatientMembershipController` |
| QBE yang berlaku | `QBE-SVC-001` (kueri di service), `QBE-PAGE-001` (paging), `QBE-DTO-001`, `QBE-PERM-001` |

## 1. Masalah yang diperbaiki

Layar Pendaftaran Membership butuh langkah pertama: mencari pasien lewat No. RM, NIK, atau
No. Telp, lalu melihat apakah pasien itu sudah member atau sedang menunggu bayar. Sebelumnya
pencarian pasien hanya tersedia di master pasien, tanpa status member, dan menampilkan NIK serta
No. Telp utuh.

## 2. Proses bisnis

1. Petugas CRM mengetik minimal 3 karakter No. RM, NIK, atau No. Telp.
2. Sistem menampilkan **semua** pasien aktif yang cocok, tanpa memilih otomatis (`CRM-DEC-010`). Setiap baris memuat nama, tanggal lahir, No. RM, NIK dan No. Telp tersamar, status member (tingkat dan tanggal akhir bila Aktif), dan nomor pendaftaran yang masih menunggu bayar.
3. Petugas memilih satu pasien, lalu lanjut mendaftar (`POST /admin`).

Contoh: No. Telp `0812-0000-1234` dipakai Ibu Sari (Gold sampai 12 Okt 2027) dan anaknya Dika
(Reguler). Keduanya tampil, dengan NIK Ibu Sari tersamar `3174********0001` dan No. Telp
`0812*****234`.

**Status aktif** (`GET /admin/patients/{patientId}/active?date=`) menjawab "member atau Reguler"
pada tanggal WIB tertentu (bawaan hari ini) dengan aturan tunggal J1. Endpoint ini dipakai
layar admisi dan konsumen lain.

**Jalur tidak normal:** kata kunci kurang dari 3 karakter → 400 "Ketik minimal 3 karakter No. RM,
NIK, atau No. Telp."; pasien tidak ada → 404; pasien nonaktif tidak ikut dicari.

## 3. Perubahan yang dikerjakan

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PatientManagement/Membership/Services/PatientMembershipService.cs` | `LookupPatientsAsync`, `GetPatientActiveMembershipAsync`, penyamar `Mask` |
| `Areas/HealthServices/PatientManagement/MasterData/DTOs/PatientMembershipDtos.cs` | `PatientMembershipLookupQuery`, `PatientMembershipLookupResponse`, `PatientMembershipActiveSummaryResponse`, `PatientActiveMembershipResponse` |
| `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientMembershipController.cs` | `GET /admin/patient-lookup`, `GET /admin/patients/{patientId}/active` |
| `Tests/.../MembershipLookupTests.cs` | Baru, 6 uji |
| `Tests/.../MembershipPermissionRegistryTests.cs` | Dua aksi baru ikut diperiksa |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Dua endpoint baru sesuai API `v2` |
| Database | Baca saja |
| Keamanan/Auth | `PatientMembership : Read`. NIK dan No. Telp disamarkan **di backend** (4 depan + 4 belakang untuk NIK, 4 depan + 3 belakang untuk No. Telp), jadi nilai utuh tidak pernah sampai ke layar CRM |

## 4. Dokumentasi endpoint

#### Health Services / Patient Management / Master Data / Patient Membership

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/admin/patient-lookup?keyword=&pageNumber=&pageSize=` | Mencari pasien aktif beserta status member | `PatientMembership : Read` |
| `GET` | `/admin/patients/{patientId}/active?date=` | Status member pasien pada tanggal WIB tertentu | `PatientMembership : Read` |

## 5. Verifikasi

| Skenario | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| No. Telp dipakai dua pasien (`AT-18`) | Keduanya tampil dengan status masing-masing; NIK/No. Telp tersamar | `PASS` | Uji controller SQLite |
| Cari No. RM dan NIK; pasien nonaktif | Ditemukan; yang nonaktif tidak | `PASS` | idem |
| Pendaftaran menunggu | Nomor pendaftaran ikut tampil | `PASS` | idem |
| Kata kunci kosong / 2 karakter | 400 | `PASS` | idem |
| Status aktif hari ini / sebelum tanggal mulai / pasien tidak ada | Member / Reguler / 404 | `PASS` | idem |
| `PermissionRegistryValidator` atas 9 aksi baru | Tanpa temuan | `PASS` | `MembershipPermissionRegistryTests` |
| `dotnet test` seluruh project | 154 lulus | `PASS` | Keluaran test |
| QBE checker | 0 VIOLATION | `PASS` | Keluaran checker |

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-18` lulus | Terpenuhi | Uji No. Telp dua pasien |
| NIK/No. Telp tidak masuk log | Terpenuhi | Kueri baca tidak memanggil `LoggerService` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Pencarian memakai "mengandung" (seperti master pasien), bukan "sama persis", jadi kata kunci pendek bisa memunculkan banyak pasien. Paging maksimal 100 per halaman |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | `NONE` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Belum di-commit. Berkas uji baru `MembershipLookupTests.cs` diabaikan git (`/Tests/*`), perlu `git add -f` |
| Langkah berikutnya | `BE-CRM-015` (penutupan rute kiosk, pensiun endpoint lama, `IsMember` tidak lagi diterima; rilis bersama `FE-CRM-001`) |
