# Laporan Perubahan Backend — `BE-CRM-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-CRM-003` |
| Judul | Iuran di master tingkat |
| Slice | `MVP-0`, `EPIC CRM-01` |
| Roadmap | `docs/module-blueprints/crm-membership/roadmap/backend-roadmap.md` § `BE-CRM-003` |
| Trace | `CRM-DEC-004`, `CRM-DEC-006`; `VAL-02`, `VAL-03`; `contracts/api-contract.md` v2 bagian Membership Tier |
| Contract version | API `v2`, `approved` (Yoga Aji Pratama, 2026-10-13) |
| Dependency | `BE-CRM-001` ✅ (kolom `AnnualFeeAmount` sudah diterapkan ke devYoga) |
| Klasifikasi | `LIGHT`: satu field di DTO, validasi, metadata form, dan satu filter pilihan |
| Task mode | `BACKEND` |
| Target tulis | `Areas/Administrator/MasterData/`, `Tests/QuilvianSystemBackend.PatientManagementTests/`, laporan ini |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `761bfe9d` + perubahan `BE-CRM-001`/`002` yang belum di-commit |
| Tanggal | 2026-10-13 |
| Status | Selesai |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `Administrator` / `MasterData` (`Mst`, `ACTIVE`) |
| Keberlakuan | `TOUCHED LEGACY`: `MembershipTierController` memakai `ApplicationDbContext` langsung. Pola itu tidak diubah (legacy ratchet) |
| QBE yang berlaku | `QBE-VAL-001` (validasi nilai iuran), `QBE-API-001` (bentuk `ApiResponse` dan 400 yang sudah ada), `QBE-DTO-001` (lewat DTO, bukan entity) |

## 1. Masalah yang diperbaiki

Petugas belum bisa mengisi berapa iuran tiap tingkat. Tanpa nominal itu, pendaftaran membership
tidak bisa menetapkan tagihan, dan aturan `VAL-03` ("tingkat dengan iuran 0 tidak bisa
didaftarkan") tidak punya data. Layar pendaftaran juga belum bisa meminta pilihan tingkat yang
hanya berisi empat tingkat berbayar.

## 2. Proses bisnis

1. Admin master data membuka Tingkat Keanggotaan, lalu mengisi **Nominal Iuran**, misalnya Gold Rp 2.400.000.
2. Nilai negatif ditolak dengan pesan "Nominal iuran tidak boleh kurang dari 0." (kode 400).
3. Layar Pendaftaran Membership (nanti `FE-CRM-003`) memanggil pilihan tingkat dengan
   `registrableOnly=true`, sehingga yang tampil hanya Silver, Gold, Platinum, Executive (`CRM-DEC-006`).

## 3. Perubahan yang dikerjakan

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Administrator/MasterData/DTOs/MembershipTierDtos.cs` | `AnnualFeeAmount` di request (Create/Update, `[Range(0, 999999999999)]`), response, dan option response |
| `Areas/Administrator/MasterData/Controllers/MembershipTierController.cs` | Simpan saat create/update, petakan di tiga respons, validasi < 0, metadata form `annualFeeAmount`, query `registrableOnly` di `GET /options` |
| `Tests/QuilvianSystemBackend.PatientManagementTests/MembershipReadAndTierControllerTests.cs` | Uji baru (bersama `BE-CRM-023`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tambahan, tidak breaking: field `annualFeeAmount` di request/response tingkat, query opsional `registrableOnly` |
| Database | `NOT APPLICABLE`: kolom sudah dibuat `BE-CRM-001` |
| Keamanan/Auth | `NOT APPLICABLE`: izin `MembershipTier : *` tidak berubah |

## 4. Dokumentasi endpoint

#### Administrator / Master Data / Membership Tier

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Tambah tingkat, kini dengan `annualFeeAmount` | `MembershipTier : Create` |
| `PUT` | `/{id}` | Ubah tingkat, kini dengan `annualFeeAmount` | `MembershipTier : Update` |
| `GET` | `/options?registrableOnly=true` | Pilihan tingkat untuk pendaftaran membership (4 tingkat berbayar) | `MembershipTier : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `Tingkat_IuranTersimpanDanTerbacaKembali` | 2.400.000 tersimpan dan terbaca di detail | `PASS` | Uji controller, SQLite in-memory |
| `Tingkat_IuranNegatif_Ditolak400` | 400 dengan pesan di atas | `PASS` | idem |
| `PilihanTingkat_RegistrableOnly_HanyaEmpatTingkatBerbayar` | Dari 8 tipe, hanya Silver/Gold/Platinum/Executive | `PASS` | idem |
| `dotnet test` PatientManagementTests (seluruh) | 95 lulus, 0 gagal | `PASS` | Keluaran test |
| QBE checker `ReportOnly` | 0 VIOLATION | `PASS` | Keluaran checker |
| Uji endpoint lewat HTTP ke devYoga | — | `NOT RUN` | Menulis master data bersama butuh izin. Controller diuji langsung sebagai gantinya |

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Nilai negatif 400 | Terpenuhi | `Tingkat_IuranNegatif_Ditolak400` |
| Nilai 2.400.000 tersimpan dan terbaca kembali | Terpenuhi | `Tingkat_IuranTersimpanDanTerbacaKembali` |
| Pilihan tingkat untuk pendaftaran hanya empat tingkat berbayar | Terpenuhi | `PilihanTingkat_RegistrableOnly_...` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `[Range]` di DTO hanya ditegakkan pipeline MVC. Uji memanggil controller langsung, sehingga yang dibuktikan adalah validasi di `ValidateRequestAsync`. Keduanya menolak nilai negatif |
| Masalah yang diketahui | Nominal iuran keempat tingkat belum diisi di devYoga (`CRM-OQ-001`) |
| Risiko tersisa | `NONE` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat laporan `BE-CRM-023` (dikerjakan bersama) |
| Langkah berikutnya | `FE-CRM-006` (isian iuran di form tingkat) |
