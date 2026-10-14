# Laporan Perubahan Backend — `BE-CRM-023`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-CRM-023` |
| Judul | Respons baca diperluas |
| Slice | `MVP-0`, `EPIC CRM-01` |
| Roadmap | `docs/module-blueprints/crm-membership/roadmap/backend-roadmap.md` § `BE-CRM-023` |
| Trace | `CRM-FE-005`; `03-frontend-architecture.md` §3; `04-prd-to-mvp.md` §8 (pengganti pengingat WA); `contracts/api-contract.md` v2 (endpoint lama yang dipertahankan) |
| Contract version | API `v2`, `approved` |
| Dependency | `BE-CRM-001` ✅, `BE-CRM-010` ✅ (tanggal WIB untuk filter jatuh tempo) |
| Klasifikasi | `MEDIUM`: DTO + pemetaan + ringkasan + dua filter pada endpoint legacy |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/PatientManagement/MasterData/`, `Tests/...PatientManagementTests/`, laporan ini |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `761bfe9d` + perubahan yang belum di-commit |
| Tanggal | 2026-10-13 |
| Status | Selesai |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `PatientManagement` / `MasterData` (`Mst`) |
| Keberlakuan | `TOUCHED LEGACY`: endpoint baca lama tetap memakai `ApplicationDbContext` langsung (legacy ratchet, tidak ditulis ulang) |
| QBE yang berlaku | `QBE-DTO-001`, `QBE-PAGE-001` (filter ditambahkan ke paging yang sudah ada), `QBE-API-001` |

## 1. Masalah yang diperbaiki

Daftar dan detail membership belum menampilkan data siklus hidup baru: jenis pendaftaran,
tanggal mulai periode, batas bayar, iuran, alasan batal, dan `RowVersion` (dibutuhkan frontend
untuk perintah ubah). Petugas CRM juga belum bisa menyaring "Perlu Verifikasi" atau "berakhir 30
hari lagi". Filter terakhir ini adalah pengganti pengingat WA selama MVP.

## 2. Proses bisnis

1. Petugas CRM membuka Daftar Membership.
2. Filter `membershipStatus=9` menampilkan data lama yang menunggu verifikasi. `membershipStatus=7` menampilkan yang menunggu pembayaran.
3. Filter `expiringWithinDays=30` menampilkan membership Aktif yang tanggal akhirnya antara hari ini dan 30 hari ke depan (kalender WIB). Contoh: hari ini 1 Des 2026, sehingga yang berakhir 20 Des 2026 tampil dan yang berakhir 18 Jun 2027 tidak.
4. Ringkasan menghitung Menunggu Pembayaran, Terjadwal, Perlu Verifikasi, Dibatalkan, dan "berakhir dalam 30 hari".
5. Alasan pembatalan hanya tampil di **detail**, tidak di daftar, karena bisa memuat cerita pribadi pasien.

## 3. Perubahan yang dikerjakan

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PatientManagement/MasterData/DTOs/PatientMembershipDtos.cs` | `PatientMembershipResponse` + 10 field (`RegistrationType`(+Name), `PeriodStartDate`, `PaymentDueAt`, `ActivatedAt`, `FeeAmountSnapshot`, `PreviousPatientMembershipId`, `CancellationCategory`(+Name), `RowVersion`). `PatientMembershipDetailResponse` + `CancellationReason`. Ringkasan + 5 hitungan |
| `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientMembershipController.cs` | Inject `PatientMembershipStatusResolver`. Pemetaan field baru di daftar dan detail. Query `membershipStatus` dan `expiringWithinDays` (1–366) di `GET /`, `/kiosk`, `/admin`. Hitungan ringkasan baru. Helper `ApplyExpiringWithinDays` |
| `Tests/QuilvianSystemBackend.PatientManagementTests/MembershipReadAndTierControllerTests.cs` | Baru, 7 uji (3 untuk `BE-CRM-003`, 4 untuk task ini) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tambahan, tidak breaking. Field respons baru dan dua query opsional. Konsumen lama mengabaikan field baru |
| Database | Baca saja |
| Keamanan/Auth | Izin tidak berubah. `CancellationReason` (sensitif) dibatasi ke detail yang dijaga `PatientMembership : Read` |

## 4. Dokumentasi endpoint

#### Health Services / Patient Management / Master Data / Patient Membership

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/admin?membershipStatus=&expiringWithinDays=` | Daftar membership dengan filter status dan jatuh tempo | `PatientMembership : Read` |
| `GET` | `/admin/summary` | Ringkasan, kini dengan hitungan per status siklus hidup | `PatientMembership : Read` |
| `GET` | `/admin/{id}` | Detail, kini dengan field siklus hidup dan alasan batal | `PatientMembership : Read` |

Rute `GET /` dan `/kiosk` (policy `KioskRead`) ikut mendapat field dan filter yang sama karena
memakai aksi yang sama. Pengalihan frontend ke `/admin` adalah `FE-CRM-001`.

## 5. Verifikasi

| Skenario | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `Daftar_FilterStatusMenunggu_HanyaMenunggu_DanFieldBaruTerisi` | 1 dari 5 baris, field baru terisi | `PASS` | Uji controller SQLite |
| `Daftar_BerakhirDalam30Hari_HanyaAktifYangJatuhTempo` | Hanya yang berakhir 20 Des 2026 | `PASS` | idem |
| `Ringkasan_MenghitungStatusBaru` | Menunggu 1, Perlu Verifikasi 1, Dibatalkan 1, Terjadwal 0, jatuh tempo 1 | `PASS` | idem |
| `Detail_MemuatAlasanBatal_DaftarTidak` | Detail memuat alasan; tipe daftar tidak punya properti itu | `PASS` | idem |
| `dotnet test` seluruh project | 95 lulus | `PASS` | Keluaran test |
| QBE checker | 0 VIOLATION | `PASS` | Keluaran checker |
| HTTP ke backend yang berjalan | — | `NOT RUN` | Controller diuji langsung. Uji HTTP menunggu frontend (`FE-CRM-004`) |

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Field baru terisi | Terpenuhi | Uji filter status |
| Filter `membershipStatus=7` hanya mengembalikan Menunggu Pembayaran | Terpenuhi | Uji filter status |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Berkas uji baru diabaikan git.** `.gitignore:396` berisi `/Tests/*`. Berkas uji lama ter-track karena ditambahkan paksa. `MembershipStatusResolverTests.cs` dan `MembershipReadAndTierControllerTests.cs` perlu `git add -f` agar ikut commit |
| Masalah yang diketahui | Hitungan lama `ExpiredPatientMembership` masih memakai `ExpiredDate < sekarang` (UTC), tidak diubah agar tidak mengganti makna field yang sudah dipakai |
| Risiko tersisa | `NONE` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` 12 berkas source (gabungan `BE-CRM-001`/`002`/`003`/`010`/`023`), `??` folder `Membership/`, empat berkas migration, folder konfigurasi baru, folder `task/`. Dua berkas uji baru ada di disk tetapi diabaikan git (lihat Peringatan). Belum di-commit |
| Langkah berikutnya | `BE-CRM-011` (daftar membership baru lewat service) |
