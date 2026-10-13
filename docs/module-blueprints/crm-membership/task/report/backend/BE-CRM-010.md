# Laporan Perubahan Backend — `BE-CRM-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-CRM-010` |
| Judul | Resolver member aktif |
| Slice | `MVP-1`, `EPIC CRM-05` |
| Roadmap | `docs/module-blueprints/crm-membership/roadmap/backend-roadmap.md` § `BE-CRM-010` |
| Trace | `FR-CRM-040`, `FR-CRM-041` (sisi penulis); J1 `03-domain-architecture.md`; `CRM-DEC-005`; `INV-11`; `CRM-CAP-016` |
| Contract version | Integrasi `v1` (`INT-01` s.d. `INT-03`), `approved` |
| Dependency | `BE-CRM-001` ✅ |
| Klasifikasi | `MEDIUM`: satu service baru, registrasi DI, 15 uji unit |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/PatientManagement/Membership/Services/`, `Program.cs`, `Tests/...PatientManagementTests/`, laporan ini |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `761bfe9d` + perubahan yang belum di-commit |
| Tanggal | 2026-10-13 |
| Status | Selesai. Konsumen (antrean RJ, kiosk, proses terjadwal) belum memakainya. Itu task `BE-CRM-020`/`021`/`022` |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `HealthServices` / `PatientManagement` / `Membership` (`Pat`, `ACTIVE`, terdaftar 2026-10-13) |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-SVC-001` (aturan dimiliki service, bukan controller), `QBE-MOD-001` (ditempatkan di submodul pemilik), `QBE-TXN-001` (penulisan proyeksi ikut transaksi pemanggil, tanpa `SaveChanges` sendiri) |

## 1. Masalah yang diperbaiki

Ada dua aturan "member aktif" yang bisa memberi jawaban berbeda (`CRM-CAP-016`). Nurse
station membaca penanda pasien yang hanya melihat `IsActive && IsPrimary`, sedangkan antrean RJ
memeriksa status dan tanggal. Contoh: Gold yang berakhir 12 Okt 2027 tetap tampil "Pasien
Membership Gold" di nurse station pada 2028, sementara antrean menganggapnya Reguler.

## 2. Proses bisnis

Aturan tunggal yang kini tersedia untuk semua konsumen:

1. Tentukan tanggal WIB yang ditanya. Bawaannya hari ini menurut zona `PatientMembership:TimeZone` (bawaan `Asia/Jakarta`).
2. Pasien **member aktif** bila punya membership berstatus Aktif, belum dihapus, dengan `PeriodStartDate ≤ tanggal ≤ ExpiredDate`, dan tingkatnya aktif.
3. Selain itu pasien **Reguler**: Menunggu Pembayaran, Terjadwal, Perlu Verifikasi, Kedaluwarsa, Dibatalkan, Di-upgrade, Aktif tanpa periode lengkap (fail-closed), atau tingkat nonaktif.
4. `SyncPatientProjectionAsync` menyalin jawaban itu ke `MstPatient` (`IsMember`, `DefaultMembershipTierId`, `ActivePatientMembershipId`). Tidak menulis apa pun bila sudah sama.

Contoh batas hari: Gold berakhir 12 Okt 2027. Pukul 23.30 WIB tanggal 12 masih Aktif. Pukul
00.01 WIB tanggal 13 (saat jam UTC masih menunjukkan 12 Okt 17.01) sudah Reguler.

## 3. Perubahan yang dikerjakan

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PatientManagement/Membership/Services/PatientMembershipStatusResolver.cs` | Baru: `GetTodayWib`, `ToWibDate`, `QueryActiveOn`, `GetActiveAsync`, `GetActiveForPatientsAsync`, `SyncPatientProjectionAsync`, serta record `PatientActiveMembership` |
| `Program.cs` | `AddScoped<PatientMembershipStatusResolver>()` + `using` |
| `Tests/QuilvianSystemBackend.PatientManagementTests/MembershipStatusResolverTests.cs` | Baru, 15 uji |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` (endpoint status aktif dikerjakan `BE-CRM-014`) |
| Database | Baca saja. Penulisan proyeksi hanya bila pemanggil menyimpan |
| Keamanan/Auth | `NOT APPLICABLE` |

## 4. Dokumentasi endpoint

`NOT APPLICABLE`.

## 5. Verifikasi

| Skenario | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `Aktif_DalamPeriode_TerbacaMember` | Membership dan tingkat dikembalikan | `PASS` | Uji unit SQLite |
| `HariTerakhir_Pukul2330Wib_MasihAktif_Pukul0001Besok_Reguler` (`AT-09`) | Sesuai | `PASS` | idem |
| `StatusSelainAktif_TerbacaReguler` × 6 status (`AT-03`) | Semua Reguler | `PASS` | idem |
| Aktif tanpa periode / sebelum tanggal mulai / tingkat nonaktif / terhapus | Semua Reguler | `PASS` | idem |
| `Batch_HanyaPasienAktifYangMuncul` | Hanya pasien Aktif | `PASS` | idem |
| `SyncProyeksi_MengisiLaluMengosongkan_DanTidakMenulisBilaSama` | Terisi, tidak menulis ulang, lalu kosong setelah lewat tanggal akhir | `PASS` | idem |
| `QueryActiveOn_TerjemahKePostgreSql_TanpaKoneksi` | SQL PostgreSQL memuat filter status dan rentang tanggal | `PASS` | `ToQueryString` dengan provider Npgsql, tanpa koneksi |
| `Di_MembuatResolverTanpaTimeProviderTerdaftar` | DI membuat resolver (`ValidateOnBuild`) | `PASS` | idem |
| `dotnet test` seluruh project | 95 lulus | `PASS` | Keluaran test |
| QBE checker | 0 VIOLATION | `PASS` | Keluaran checker |

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 23.30 WIB di hari terakhir: Aktif. 00.01 WIB keesokannya: Reguler | Terpenuhi | `AT-09` |
| Menunggu/Terjadwal/Perlu Verifikasi: Reguler | Terpenuhi | `AT-03` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Aturan sengaja lebih ketat dari `OutpatientQueueClassificationService`, yang menerima `ExpiredDate` kosong sebagai "tanpa batas". Penyelarasan antrean RJ adalah `BE-CRM-022` (⛔ owner rawat-jalan) |
| Masalah yang diketahui | 14 pasien di devYoga bertanda `IsMember = true` tanpa membership. Belum ada pemanggil yang menyelaraskan mereka. Akan disapu proses terjadwal `BE-CRM-020` |
| Risiko tersisa | `NONE` untuk perilaku runtime yang ada (belum ada konsumen) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat laporan `BE-CRM-023` |
| Langkah berikutnya | `BE-CRM-011` (daftar membership), `BE-CRM-014` (lookup), `BE-CRM-021` |
