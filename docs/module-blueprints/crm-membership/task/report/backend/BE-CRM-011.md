# Laporan Perubahan Backend — `BE-CRM-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-CRM-011` |
| Judul | Daftar membership baru |
| Slice | `MVP-1`, `EPIC CRM-02` |
| Roadmap | `docs/module-blueprints/crm-membership/roadmap/backend-roadmap.md` § `BE-CRM-011` |
| Trace | `FR-CRM-010`, `FR-CRM-011`; `CRM-DEC-003`, `004`, `006`, `012`, `023`; `VAL-01` s.d. `VAL-06`; `contracts/permission-audit-matrix.md` (`PatientMembership : Create`) |
| Contract version | API `v2`, validasi `v1`, izin `v1`, semua `approved` |
| Dependency | `BE-CRM-002` ✅, `BE-CRM-003` ✅, `BE-CRM-010` ✅ |
| Klasifikasi | `MEDIUM`: satu service baru (perintah daftar), satu aksi controller, satu DTO, 11 uji |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/PatientManagement/`, `Program.cs`, `Tests/...PatientManagementTests/`, laporan ini |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `761bfe9d` + perubahan yang belum di-commit |
| Tanggal | 2026-10-13 |
| Status | Selesai. Uji konkurensi sungguhan di PostgreSQL (`AT-12`) belum dijalankan |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `HealthServices` / `PatientManagement` / `Membership` (`Pat`, `ACTIVE`) untuk service; `MasterData` untuk controller dan DTO |
| Keberlakuan | `NEW CODE`: `PatientMembershipService`, `RegisterPatientMembershipRequest`, aksi `RegisterPatientMembershipForAdmin`. `TOUCHED LEGACY`: `PatientMembershipController` |
| QBE yang berlaku | `QBE-SVC-001` (aturan dan transaksi di service), `QBE-CODE-001`/`002`/`003`/`004`/`005` (nomor dari `NumberSeriesAllocator`, deret dan format milik modul, unik di DB), `QBE-VAL-001`, `QBE-TXN-001`, `QBE-LOG-001` (log memuat aktor lewat `LoggerService`), `QBE-PERM-001`, `QBE-DTO-001` |

## 1. Masalah yang diperbaiki

Sebelumnya `POST /admin` menerima isian bebas: status, tanggal, nomor member, poin, dan belanja
diketik petugas. Membership bisa langsung Aktif tanpa membayar, dan nomor member bisa salah
ketik. Sekarang petugas CRM cukup memilih pasien dan tingkat. Sistem yang menetapkan sisanya.

## 2. Proses bisnis

1. **Pelaku:** petugas CRM dengan izin `PatientMembership : Create`.
2. **Pemicu:** pasien membeli membership.
3. **Langkah:**
   1. Petugas mengirim `POST /admin` berisi pasien, tingkat, dan catatan opsional.
   2. Sistem mengunci pasien itu (kunci per pasien di PostgreSQL), sehingga petugas kedua yang menyimpan bersamaan harus menunggu.
   3. Sistem memeriksa berurutan: pasien aktif ber-No. RM (404 bila tidak), tingkat berbayar yang aktif (400), iuran tingkat > 0 (409), tidak ada membership Aktif (409), tidak ada yang Menunggu Pembayaran (409).
   4. Sistem mengambil nomor member dari deret `PAT_MEMBERSHIP_NUMBER`.
   5. Membership tersimpan berstatus **Menunggu Pembayaran** dengan batas bayar = waktu simpan + 7 hari (dapat diatur lewat `PatientMembership:PaymentDueDays`), iuran disalin dari tingkat, dan satu riwayat `REGISTERED`.
4. **Hasil:** pasien tetap terbaca Reguler sampai iuran lunas (`EPIC CRM-08`).

Contoh: 13 Okt 2026 pukul 09.00 WIB, Ibu Sari didaftarkan Gold (iuran Rp 2.400.000). Hasilnya
nomor `MBR-000001`, status Menunggu Pembayaran, dan batas bayar 20 Okt 2026 pukul 09.00 WIB.

**Jalur tidak normal:**

| Keadaan | Kode | Pesan |
| --- | --- | --- |
| Pasien tidak ada atau nonaktif | 404 | "Pasien tidak ditemukan atau tidak aktif. Daftarkan pasien lebih dulu." |
| Tingkat Regular/Corporate/Family/Other/nonaktif | 400 | "Tingkat ini tidak bisa dipilih untuk pendaftaran membership." |
| Iuran tingkat 0 | 409 | "Nominal iuran tingkat Silver belum diisi. Hubungi admin master data." Nomor tidak terbit |
| Pasien masih Aktif | 409 | "Pasien sudah member Gold sampai 12 Okt 2027. Gunakan perpanjangan atau upgrade." |
| Sudah ada yang Menunggu | 409 | "Pasien ini sudah punya pendaftaran yang menunggu pembayaran (MBR-000001)." |
| Dua petugas lolos bersamaan (kunci terlewati) | 409 | Index unik `UX_..._Patient_Pending` menolak. Pesan meminta muat ulang |

## 3. Perubahan yang dikerjakan

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PatientManagement/Membership/Services/PatientMembershipService.cs` | Baru: `RegisterAsync`, `PatientMembershipCommandResult`, kunci per pasien, deret nomor, riwayat |
| `Areas/HealthServices/PatientManagement/MasterData/DTOs/PatientMembershipDtos.cs` | `RegisterPatientMembershipRequest` (pasien, tingkat, catatan ≤ 250) |
| `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientMembershipController.cs` | Inject service. `POST /admin` kini `RegisterPatientMembershipForAdmin` (request baru, 200/400/404/409) |
| `Program.cs` | `AddScoped<PatientMembershipService>()` |
| `Tests/QuilvianSystemBackend.PatientManagementTests/MembershipRegisterTests.cs` | Baru, 11 uji |
| `Tests/QuilvianSystemBackend.PatientManagementTests/MembershipReadAndTierControllerTests.cs` | Helper menyesuaikan konstruktor controller |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Berubah:** `POST /admin` menerima `RegisterPatientMembershipRequest`, bukan `CreatePatientMembershipRequest`. Frontend saat ini memanggil `POST /` (bukan `/admin`), jadi tidak putus. `POST /` dan `POST /kiosk` lama masih ada sampai `BE-CRM-015` |
| Database | Menulis `MstPatientMembership`, `PatMembershipStatusHistory`, dan `NumNumberSeries` (deret baru `PAT_MEMBERSHIP_NUMBER` dibuat otomatis saat pertama dipakai). Tanpa migration |
| Keamanan/Auth | `PatientMembership : Create`. Log hanya memuat id membership dan status, tanpa catatan atau data pasien |

## 4. Dokumentasi endpoint

#### Health Services / Patient Management / Master Data / Patient Membership

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/admin` | Mendaftarkan membership baru (Menunggu Pembayaran) | `PatientMembership : Create` |

Kode status: 200 berhasil (berisi detail membership), 400 isian/tingkat tidak sah, 403 tanpa
izin, 404 pasien tidak ada, 409 bertabrakan dengan membership yang ada.

## 5. Verifikasi

| Skenario | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `Daftar_Berhasil_...` (`AT-02`) | Status 7, `MBR-000001`, batas bayar +7 hari, iuran tersalin, riwayat `REGISTERED`, proyeksi pasien tetap Reguler | `PASS` | Uji SQLite |
| `Daftar_DuaPasienBerurutan_NomorNaik` | `MBR-000001`, `MBR-000002` | `PASS` | idem |
| `PasienTidakAda_404` (`AT-01`), `PasienNonaktif_404` | 404, tidak ada baris | `PASS` | idem |
| `TingkatCorporate_400` (`AT-20`) | 400 | `PASS` | idem |
| `TingkatTanpaIuran_409_TanpaMenghabiskanNomor` | 409, deret nomor tidak tersentuh | `PASS` | idem |
| `MasihAktif_409_...` (`AT-11`) | Pesan menyebut tingkat dan tanggal akhir | `PASS` | idem |
| `SudahAdaMenunggu_409_MenyebutNomornya` | 409, satu baris saja | `PASS` | idem |
| `CatatanLebihDari250_400`, `BatasBayar_IkutKonfigurasi` | Sesuai | `PASS` | idem |
| `Controller_PostAdmin_200DenganDetail_Dan409DenganPesan` | 200 berisi detail, lalu 409 dengan pesan | `PASS` | idem |
| `dotnet test` seluruh project | 106 lulus | `PASS` | Keluaran test |
| QBE checker | 0 VIOLATION | `PASS` | Keluaran checker |
| `AT-12` konkurensi di PostgreSQL | — | `NOT RUN` | Butuh menulis ke devYoga di luar transaksi yang bisa di-rollback. Bukti yang ada: index unik `UX_..._Patient_Pending` menolak baris kedua (laporan `BE-CRM-002` §8) dan kunci `pg_advisory_xact_lock` mengikuti pola `OutpatientQueueNumberAllocator` |
| `AT-19` (403 tanpa izin), `AT-26` | — | `NOT RUN` / tercakup desain | `AT-19` butuh pipeline otorisasi berjalan. `AT-26`: DTO tidak punya field nomor member, jadi isian itu diabaikan model binder |

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-01` | Terpenuhi | `PasienTidakAda_404` |
| `AT-02` | Terpenuhi | `Daftar_Berhasil_...` |
| `AT-11` | Terpenuhi | `MasihAktif_409_...` |
| `AT-12` | **Belum terpenuhi** (sebagian: index terbukti, uji paralel belum) | Lihat §5 |
| `AT-20` | Terpenuhi | `TingkatCorporate_400` |
| `AT-26` | Terpenuhi secara desain | DTO tanpa `memberNumber` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Aksi baru memakai helper baca lama controller (`BuildBaseQuery`, `MapDetailResponse`) untuk menyusun respons, agar tidak menggandakan pemetaan. Perintahnya sendiri sepenuhnya di service. Checker QBE tidak menandainya |
| Masalah yang diketahui | Format nomor `MBR-000001` (deret `NEVER`, 6 digit), bukan contoh ilustrasi `MBR-2610-000123` di desain. Deret baru diwajibkan `NEVER` oleh `DEC-PLT-004`. Format final tetap konfigurasi (`CRM-GAP-012`) |
| Risiko tersisa | `POST /` dan `POST /kiosk` lama (isian bebas, bisa membuat Aktif) masih hidup sampai `BE-CRM-015`. Membuat Aktif kedua lewat jalur itu kini ditolak database sebagai 500 |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas uji baru `MembershipRegisterTests.cs` diabaikan git (`/Tests/*`), perlu `git add -f`. Belum di-commit |
| Langkah berikutnya | `BE-CRM-012` (perpanjangan dan upgrade), `BE-CRM-013` (batal, verifikasi, catatan, riwayat), `BE-CRM-014` (lookup) |
