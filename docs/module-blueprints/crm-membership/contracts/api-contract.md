# Kontrak API — CRM Membership

| Field | Nilai |
|---|---|
| Contract version | `v2` (as-is = `v1` di `01-existing-capability-map.md`) |
| Status | `approved` |
| Owner | Yoga Aji Pratama. `approved_by` Yoga Aji Pratama / `approved_at` 2026-10-13 |
| Input revision | `02-backend-architecture.md` r1, `03-domain-architecture.md` r1, `00-interview-decisions.md` (`CRM-DEC-001` s.d. `028`) |
| Backend SHA | `1aecf642` |
| Dampak kompatibilitas | **Breaking** untuk `POST /`, `POST /kiosk`, `PUT`, `PATCH status`, `DELETE` (dihapus). Konsumen yang terdampak hanya frontend halaman patient-memberships (dialihkan ke rute `/admin`). Kiosk hanya memakai `GET /options`, sehingga tidak terdampak (`CRM-DEC-018`) |

Semua respons memakai `ApiResponse<T>`. Daftar memakai `PagedResult<T>`.

### Health Services / Patient Management / Master Data / Patient Membership

Base URL: `api/v1/health-services/patient-management/master-data/patient-memberships`

#### Endpoint baru

| Method | Path | Kegunaan | Hak akses | Request | Response | Keputusan | Status |
|---|---|---|---|---|---|---|---|
| `POST` | `/admin` | Mendaftarkan membership baru (Menunggu Pembayaran) | `PatientMembership : Create` | `RegisterPatientMembershipRequest` | `ApiResponse<PatientMembershipDetailResponse>` | `CRM-DEC-003`, `004`, `006`, `023` | **Rencana (belum tersedia)**. Path sudah ada, tetapi isi request dan perilakunya baru |
| `POST` | `/admin/{id}/renewals` | Mendaftarkan perpanjangan untuk membership Aktif `{id}` | `PatientMembership : Create` | `RenewPatientMembershipRequest` | `ApiResponse<PatientMembershipDetailResponse>` | `CRM-DEC-024` | **Rencana (belum tersedia)** |
| `POST` | `/admin/{id}/upgrades` | Mendaftarkan upgrade dari membership Aktif `{id}` | `PatientMembership : Create` | `UpgradePatientMembershipRequest` | `ApiResponse<PatientMembershipDetailResponse>` | `CRM-DEC-012` | **Rencana (belum tersedia)** |
| `POST` | `/admin/{id}/cancel` | Membatalkan (permintaan pasien atau salah input) | `PatientMembership : Cancel` | `CancelPatientMembershipRequest` | `ApiResponse<PatientMembershipDetailResponse>` | `CRM-DEC-013`, `025` | **Rencana (belum tersedia)** |
| `POST` | `/admin/{id}/verify` | Mengakui atau menolak membership lama (Perlu Verifikasi) | `PatientMembership : Verify` | `VerifyLegacyPatientMembershipRequest` | `ApiResponse<PatientMembershipDetailResponse>` | `CRM-DEC-021` | **Rencana (belum tersedia)** |
| `PATCH` | `/admin/{id}/notes` | Mengubah catatan (satu-satunya ubah yang diizinkan) | `PatientMembership : Update` | `UpdatePatientMembershipNotesRequest` | `ApiResponse<PatientMembershipDetailResponse>` | `CRM-DEC-022` | **Rencana (belum tersedia)** |
| `GET` | `/admin/{id}/status-histories` | Riwayat status satu membership | `PatientMembership : Read` | - | `ApiResponse<List<PatientMembershipStatusHistoryResponse>>` | `CRM-DEC-013` | **Rencana (belum tersedia)** |
| `GET` | `/admin/patient-lookup` | Mencari pasien lewat No. RM/NIK/No. Telp beserta status member masing-masing | `PatientMembership : Read` | `PatientMembershipLookupQuery` (`keyword`, paging) | `ApiResponse<PagedResult<PatientMembershipLookupResponse>>` | `CRM-DEC-010` | **Rencana (belum tersedia)** |
| `GET` | `/admin/patients/{patientId}/active` | Status member aktif pasien pada tanggal tertentu (bawaan hari ini WIB) | `PatientMembership : Read` | query `date?` | `ApiResponse<PatientActiveMembershipResponse>` (kosong = Reguler) | J1 | **Rencana (belum tersedia)** |

#### Endpoint lama yang dipertahankan

| Method | Path | Kegunaan | Hak akses | Perubahan |
|---|---|---|---|---|
| `GET` | `/admin`, `/admin/summary`, `/admin/filters/metadata`, `/admin/options`, `/admin/{id}` | Daftar, ringkasan, metadata, pilihan, detail | `PatientMembership : Read` | Response diperluas (`RegistrationType`, `PeriodStartDate`, `PaymentDueAt`, `FeeAmountSnapshot`, `CancellationCategory`, `RowVersion`). Ringkasan menambah hitungan per status baru |
| `GET` | `/`, `/kiosk`, `/filters/metadata`, `/kiosk/filters/metadata`, `/options`, `/kiosk/options` | Akses baca kiosk | Policy `KioskRead` | Tetap. `/options` hanya menampilkan membership Aktif |
| `GET` | `/summary`, `/{id}` | Alias baca | `PatientMembership : Read` | Tetap |

#### Endpoint lama yang dihapus

| Method | Path | Alasan | Pengganti |
|---|---|---|---|
| `POST` | `/`, `/kiosk` | Kiosk tidak boleh membuat membership (`CRM-DEC-018`) | `POST /admin` |
| `PUT` | `/{id}`, `/admin/{id}` | Mengubah pasien/tingkat/tanggal dilarang (`CRM-DEC-022`) | `PATCH /admin/{id}/notes`, `/upgrades`, `/cancel` |
| `PATCH` | `/{id}/status`, `/admin/{id}/status` | Status tidak boleh dibalik tanpa alasan (`CRM-DEC-013`) | `/cancel`, `/verify` |
| `DELETE` | `/{id}`, `/admin/{id}` | Riwayat tidak boleh hilang (`CRM-DEC-013`) | `/cancel` |

### Administrator / Master Data / Membership Tier

Base URL: `api/v1/administrator/master-data/membership-tiers`

| Method | Path | Kegunaan | Hak akses | Perubahan | Status |
|---|---|---|---|---|---|
| `POST`, `PUT` | `/`, `/{id}` | Tambah/ubah tingkat | Izin yang sudah ada | Request dan response menambah `AnnualFeeAmount` (≥ 0) | **Rencana (belum tersedia)** untuk kolom baru |

## Bentuk Request Utama

| DTO | Field | Aturan |
|---|---|---|
| `RegisterPatientMembershipRequest` | `patientId` (uuid, wajib), `membershipTierId` (uuid, wajib), `notes` (≤ 250, opsional) | Lihat `validation-matrix.md` `VAL-01` s.d. `VAL-06` |
| `RenewPatientMembershipRequest` | `notes?`, `expectedRowVersion` | `VAL-07`, `VAL-08` |
| `UpgradePatientMembershipRequest` | `membershipTierId`, `notes?`, `expectedRowVersion` | `VAL-09` |
| `CancelPatientMembershipRequest` | `category` (`PatientRequest`/`InputError`), `reason` (10–500), `expectedRowVersion` | `VAL-10` s.d. `VAL-12` |
| `VerifyLegacyPatientMembershipRequest` | `decision` (`Accept`/`Reject`), `periodStartDate?`, `periodEndDate?`, `reason`, `expectedRowVersion` | `VAL-13` s.d. `VAL-15` |
| `UpdatePatientMembershipNotesRequest` | `notes` (≤ 250), `expectedRowVersion` | `VAL-16` |

Contoh permintaan daftar baru (data samaran):

```json
{ "patientId": "8f0c...e21", "membershipTierId": "1a2b...9c0", "notes": "Daftar via telepon" }
```

Contoh respons ringkas: `memberNumber` = `MBR-2610-000123`, `membershipStatus` = `PendingPayment`,
`paymentDueAt` = `2026-10-20T09:00:00+07:00`, `feeAmountSnapshot` = `2400000.00`.

## Kode Status

| Kode | Arti bagi pengguna | Contoh |
|---|---|---|
| 200 | Berhasil | Pendaftaran tersimpan dan nomor member terbit |
| 400 | Isian tidak lengkap atau tidak sah | Alasan pembatalan kurang dari 10 karakter |
| 403 | Tidak punya izin | Admisi mencoba mendaftarkan membership. Akun kiosk mencoba membuat |
| 404 | Membership atau pasien tidak ditemukan | `{id}` sudah dihapus |
| 409 | Bertabrakan dengan keadaan saat ini | Pasien sudah punya pendaftaran Menunggu Pembayaran. Data sudah diubah petugas lain (`RowVersion` beda). Perpanjangan di luar jendela 30 hari |
| 410 | Endpoint lama sudah dipensiunkan | Pemanggilan `PUT /{id}` dari klien lama, dengan pesan menunjuk pengganti |
| 500 | Kesalahan server | Gagal menyimpan |
