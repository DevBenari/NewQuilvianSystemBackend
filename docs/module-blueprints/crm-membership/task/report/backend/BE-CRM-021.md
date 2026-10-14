# Laporan Perubahan Backend — `BE-CRM-021`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-CRM-021` |
| Judul | Kiosk hanya membaca Aktif |
| Slice | `MVP-2`, `EPIC CRM-05` |
| Roadmap | `docs/module-blueprints/crm-membership/roadmap/backend-roadmap.md` § `BE-CRM-021` |
| Trace | `FR-CRM-043`; `CRM-DEC-018`; `CRM-CAP-012`, `CRM-CAP-016`; integrasi `INT-03` |
| Contract version | API `v2`, integrasi `v1`, `approved` |
| Dependency | `BE-CRM-010` ✅ |
| Klasifikasi | `LIGHT`: tiga titik baca dialihkan ke resolver |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/PatientManagement/MasterData/Controllers/`, `Areas/HealthServices/RegistrationManagement/` (titik sentuh kiosk), `Tests/...`, laporan ini |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `83ef27bd` + perubahan yang belum di-commit |
| Tanggal | 2026-10-13 |
| Status | Selesai |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `PatientManagement` (pemilik aturan) dan `RegistrationManagement` (`Reg`, konsumen kiosk) |
| Keberlakuan | `TOUCHED LEGACY`: `KioskPatientLookupService`, `KioskScanSessionController`, `PatientMembershipController.GetPatientMembershipOptions` |
| QBE yang berlaku | `QBE-SVC-001` (aturan tunggal milik resolver, konsumen tidak menulis aturannya sendiri) |

## 1. Masalah yang diperbaiki

Kiosk menganggap membership "aktif" bila penanda `IsActive` bernilai `true`, tanpa melihat
status dan tanggal. Akibatnya membership yang sudah habis atau belum dibayar bisa dipakai pasien
untuk mencari dirinya lewat nomor member.

## 2. Proses bisnis

1. Pasien memindai atau mengetik nomor member di kiosk.
2. Kiosk mencari pasien hanya di antara membership yang **Aktif pada hari ini (WIB)** menurut aturan tunggal J1. Membership yang lewat tanggal akhir tidak ikut walaupun proses harian belum sempat menandainya Kedaluwarsa.
3. Pencarian nomor member asuransi tidak berubah.

Contoh: Gold `MBR-000001` berakhir 12 Okt 2026, dan pada 1 Mei 2027 statusnya masih tertulis
Aktif (proses harian belum jalan). Kiosk tetap menjawab "tidak ditemukan".

Daftar pilihan membership (`GET /options`, `/kiosk/options`, `/admin/options`) dengan
`onlyActive=true` (bawaan) juga hanya mengembalikan yang Aktif dalam periode.

## 3. Perubahan yang dikerjakan

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/Services/KioskPatientLookupService.cs` | Inject resolver. Cabang `MemberNumber` memakai `QueryActiveOn(hari ini)` |
| `Areas/HealthServices/RegistrationManagement/Controllers/KioskScanSessionController.cs` | Inject resolver. Pencarian pasien lewat nomor member memakai membership Aktif hari ini. **Titik baca ketiga**, tidak tercatat di roadmap, ditemukan saat pengerjaan dan diikutkan karena tujuannya sama |
| `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientMembershipController.cs` | `GetPatientMembershipOptions`: `onlyActive` memakai resolver, tidak lagi `IsActive` |
| `Tests/.../MembershipLifecycleAndKioskTests.cs` | 2 uji untuk task ini |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Bentuk tidak berubah. Isi hasil lebih ketat (hanya Aktif dalam periode) |
| Database | Baca saja |
| Keamanan/Auth | Policy `KioskRead` tidak berubah. Menutup celah pemakaian nomor member yang sudah habis atau belum dibayar |

## 4. Dokumentasi endpoint

#### Health Services / Patient Management / Master Data / Patient Membership

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/options`, `/kiosk/options` | Pilihan membership Aktif dalam periode | Policy `KioskRead` |
| `GET` | `/admin/options` | idem | `PatientMembership : Read` |

Pencarian kiosk (`KioskPatientLookupService`, `KioskScanSessionController`) tidak berubah rute
maupun izinnya.

## 5. Verifikasi

| Skenario | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Kiosk: nomor member Aktif (`AT-23`) | `Found` | `PASS` | Uji SQLite |
| Kiosk: Aktif tetapi tanggal akhir lewat / Menunggu Pembayaran | `NotFound` | `PASS` | idem |
| Pilihan membership | Hanya yang Aktif dalam periode (bukan yang lewat tanggal, bukan Perlu Verifikasi) | `PASS` | idem |
| `dotnet test` seluruh project | 166 lulus | `PASS` | Keluaran test |
| QBE checker | 0 VIOLATION | `PASS` | Keluaran checker |
| Pencarian lewat `KioskScanSessionController` | — | `NOT RUN` | Controller kiosk tidak diuji langsung. Perubahannya satu kondisi `Where` yang memakai subkueri resolver yang sama |

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Nomor member kedaluwarsa tidak ditemukan kiosk | Terpenuhi | Uji kiosk |
| Yang Aktif ditemukan (`AT-23`) | Terpenuhi | Uji kiosk |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Menyentuh dua berkas milik Registration/kiosk. **Owner kiosk perlu diberi tahu**, sesuai catatan risiko roadmap |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | `NONE` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Belum di-commit. Berkas uji ada di `MembershipLifecycleAndKioskTests.cs` (perlu `git add -f`) |
| Langkah berikutnya | `BE-CRM-015` + `FE-CRM-001` |
