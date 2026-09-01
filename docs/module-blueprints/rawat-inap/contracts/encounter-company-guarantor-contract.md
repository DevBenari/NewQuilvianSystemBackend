# Kontrak Encounter dengan Penjamin Perusahaan

## Metadata

| Field | Nilai |
| --- | --- |
| `contract_id` | `RWI-ENC-PAYER-001` |
| `contract_version` | `1.0.0` |
| Status | `APPROVED` |
| Disetujui oleh | Muhammad Hamzah — Product/Domain owner |
| Tanggal persetujuan | 31 Agustus 2026 |
| Dasar persetujuan | Pemilik memilih opsi A: mempertahankan Tunai, Asuransi, dan Penjamin Perusahaan serta menyetujui penambahan kontrak backend encounter |
| Trace | `RWI-CAP-002`, `RWI-DEC-075`, `RWI-UI-GAP-002`, `BE-RWI-035`, `FE-RWI-025` |
| Snapshot backend | `64d7419415e473968d752d873ca02e1ae1fcded8` |
| Snapshot frontend | `786bd247db47a3b7c97b8c08fb6ec633f57d0c72` |
| Dampak kompatibilitas | Aditif. Nilai dan perilaku Tunai serta Asuransi dipertahankan |

Kontrak ini adalah addendum lintas modul untuk endpoint milik `RegistrationManagement` yang
dipakai alur admisi Rawat Inap. Kontrak `API 0.4.0` Rawat Inap tetap berlaku dan tidak dinaikkan,
karena endpoint di bawah bukan endpoint baru milik controller `inpatient`.

## 1. Hasil bisnis yang dikunci

Petugas admisi dapat membuat satu kunjungan dengan tepat satu sumber pembayaran: Tunai,
Asuransi, atau Penjamin Perusahaan. Ketika Penjamin Perusahaan dipilih, backend memeriksa bahwa
hubungan pasien dengan perusahaan masih sah lalu menyimpan referensi dan snapshot-nya bersama
kunjungan dalam satu transaksi database.

Contoh: pasien samaran **Budi Santoso** tercatat sebagai karyawan perusahaan **PT Sehat Sentosa**
dengan nomor karyawan `EMP-00125`. Petugas memilih kartu perusahaan tersebut. Encounter yang
berhasil dibuat tetap menunjuk kartu pasien-perusahaan itu walaupun nama perusahaan atau nama
benefit plan di master data kemudian diperbarui.

## 2. Proses bisnis

| Unsur | Ketentuan |
| --- | --- |
| Tujuan | Membawa penjamin perusahaan yang dipilih pada langkah Pembayaran sampai menjadi sumber pembayaran encounter |
| Pelaku | Petugas yang memiliki `PatientEncounter : Create` |
| Pemicu | Petugas menyelesaikan langkah Dokter pada alur admisi dan frontend membuat encounter |
| Prasyarat | Pasien, unit layanan, dan kartu pasien-perusahaan sudah ada; kartu dan perusahaan aktif; kartu eligible; tanggal kunjungan berada dalam masa berlaku |
| Hasil akhir | Satu `TrxPatientEncounter` dan satu `TrxPatientEncounterGuarantor` tersimpan atomik dengan tipe serta snapshot penjamin perusahaan |

Langkah utama:

1. Frontend mengirim `POST /admin` dengan `PaymentType = 3` dan
   `PatientCompanyGuarantorId` terpilih.
2. Backend memastikan pengguna memiliki hak akses create encounter.
3. Backend memastikan kartu itu milik `PatientId` yang sama, aktif, eligible, dan berlaku pada
   tanggal encounter. Master perusahaan yang dirujuk juga harus aktif.
4. Backend menolak payload campuran. Untuk Penjamin Perusahaan,
   `PaymentMethodId` dan `PatientInsuranceId` harus kosong.
5. Backend menyimpan encounter dan sumber pembayarannya dalam transaksi yang sama.
6. Response mengembalikan tipe pembayaran, referensi kartu pasien-perusahaan, referensi
   perusahaan, dan snapshot yang dapat dibaca manusia.

Tidak ada perubahan status encounter baru pada addendum ini. Status encounter tetap mengikuti
aturan endpoint create yang sudah ada.

## 3. Kontrak nilai tipe pembayaran

| Nama | Nilai angka | Arti |
| --- | ---: | --- |
| `Cash` | `1` | Pasien membayar dengan metode pembayaran tunai/non-penjamin yang tersedia saat registrasi |
| `Insurance` | `2` | Encounter dijamin oleh kartu asuransi pasien |
| `CompanyGuarantor` | `3` | Encounter dijamin oleh hubungan pasien dengan perusahaan |

Nilai `Cash = 1` dan `Insurance = 2` tidak boleh diubah. `CompanyGuarantor = 3` adalah penambahan
aditif sehingga payload lama tetap mempunyai arti yang sama.

## 4. Matriks request dan validasi

Field baru pada `PatientEncounterCreateRequest`:

| Field | Tipe | Wajib | Keterangan |
| --- | --- | --- | --- |
| `PatientCompanyGuarantorId` | `Guid?` | Hanya saat `PaymentType = 3` | ID `MstPatientCompanyGuarantor` yang dipilih untuk pasien encounter |

| `PaymentType` | `PaymentMethodId` | `PatientInsuranceId` | `PatientCompanyGuarantorId` |
| --- | --- | --- | --- |
| `Cash` (`1`) | Boleh kosong; bila diisi harus aktif dan tersedia untuk registrasi | Harus kosong | Harus kosong |
| `Insurance` (`2`) | Harus kosong | Wajib, aktif, eligible, milik pasien yang sama, dan berlaku pada tanggal encounter | Harus kosong |
| `CompanyGuarantor` (`3`) | Harus kosong | Harus kosong | Wajib, aktif, eligible, milik pasien yang sama, dan berlaku pada tanggal encounter; perusahaan induknya juga aktif |

Tanggal awal dan akhir masa berlaku bersifat inklusif. Contoh: bila kartu perusahaan berlaku
1–31 Agustus 2026, encounter tanggal 31 Agustus masih boleh dibuat, sedangkan encounter tanggal
1 September ditolak.

Flag `IsNeedGuaranteeLetter`, `IsNeedEmployeeVerification`, dan
`IsAllowExcessPaymentByPatient` tetap merupakan informasi master. Task ini tidak mengarang alur
surat jaminan baru dan tidak menjadikannya syarat penolakan tambahan.

Contoh penolakan:

- `PaymentType = 3`, tetapi `PatientCompanyGuarantorId` kosong: kode `400`, pesan menjelaskan
  bahwa penjamin perusahaan wajib dipilih.
- Kartu perusahaan milik pasien A dipakai untuk encounter pasien B: kode `400`, tanpa
  membocorkan detail pasien A.
- `PaymentType = 3` sekaligus mengirim `PatientInsuranceId`: kode `400`, karena satu encounter
  hanya boleh mempunyai satu sumber pembayaran.

## 5. Data yang disimpan

`TrxPatientEncounterGuarantor` tetap menjadi tabel sumber pembayaran satu-ke-satu milik
encounter. Implementasi menambahkan referensi berikut:

| Field target | Sumber | Aturan |
| --- | --- | --- |
| `PatientCompanyGuarantorId` | `MstPatientCompanyGuarantor.Id` | Diisi hanya untuk `CompanyGuarantor` |
| `CompanyGuarantorId` | `MstPatientCompanyGuarantor.CompanyGuarantorId` | Diisi hanya untuk `CompanyGuarantor` |
| `PaymentSourceNameSnapshot` | Nama perusahaan | Snapshot, tidak ikut berubah ketika master diubah |
| `CompanyGuarantorCodeSnapshot` | Kode perusahaan | Snapshot untuk audit dan tampilan |
| `EmployeeNumberSnapshot` | Nomor karyawan | Snapshot untuk membedakan hubungan pasien-perusahaan |
| `EmployeeNameSnapshot` | Nama karyawan | Snapshot, boleh kosong |
| `BenefitPlanCodeSnapshot` | Kode benefit plan | Memakai field snapshot existing, boleh kosong |
| `PlanNameSnapshot` | Nama benefit plan | Memakai field snapshot existing, boleh kosong |
| `ClassNameSnapshot` | Nama kelas | Memakai field snapshot existing, boleh kosong |
| `EffectiveStartDateSnapshot` / `EffectiveEndDateSnapshot` | Masa berlaku kartu | Memakai field snapshot existing, boleh kosong sesuai master |
| `IsEligible` | Eligibility kartu saat registrasi | Harus `true` untuk create yang berhasil |
| `IsPolicyActive` | Hasil pemeriksaan masa berlaku pada tanggal encounter | Harus `true` untuk create yang berhasil |

`PaymentMethodId`, `PatientInsuranceId`, dan `InsuranceProviderId` harus `null` pada baris
`CompanyGuarantor`. Index unik `EncounterId` existing tetap menjamin satu encounter hanya punya
satu sumber pembayaran.

Migration EF boleh dibuat dalam `BE-RWI-035`, tetapi penerapannya ke database bersama/target
memerlukan otorisasi terpisah dan tidak termasuk kontrak pengerjaan task ini.

## 6. Response yang ditambahkan

`PatientEncounterPaymentResponse` menambahkan field berikut secara aditif:

| Field | Tipe | Isi untuk `CompanyGuarantor` |
| --- | --- | --- |
| `PatientCompanyGuarantorId` | `Guid?` | Referensi kartu pasien-perusahaan |
| `CompanyGuarantorId` | `Guid?` | Referensi master perusahaan |
| `CompanyGuarantorCodeSnapshot` | `string?` | Kode perusahaan saat registrasi |
| `EmployeeNumberSnapshot` | `string?` | Nomor karyawan saat registrasi |
| `EmployeeNameSnapshot` | `string?` | Nama karyawan saat registrasi |

Field generic existing seperti `PaymentSourceNameSnapshot`, `BenefitPlanCodeSnapshot`,
`PlanNameSnapshot`, `ClassNameSnapshot`, dan masa berlaku juga terisi. Untuk Tunai dan Asuransi,
kelima field baru di atas bernilai `null`.

Response summary encounter menambahkan penghitung `CompanyGuarantorEncounter`. Filter metadata
otomatis menampilkan opsi `CompanyGuarantor` dengan label **Penjamin Perusahaan**.

## 7. Endpoint bergaya Swagger

### Health Services / Registration Management / Patient Encounter

Base URL: `api/v1/health-services/registration-management/patient-encounters`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/admin` | Membuat encounter petugas dengan tepat satu sumber pembayaran, termasuk Penjamin Perusahaan | `PatientEncounter : Create` | Body `PatientEncounterCreateRequest` | `ApiResponse<PatientEncounterCreateResponse>` |

Status endpoint setelah `BE-RWI-035`: **Tersedia**. Sebelum task tersebut selesai, dukungan
Penjamin Perusahaan pada endpoint ini berstatus **Rencana (belum tersedia)**.

Kode response:

- `200`: encounter dan sumber pembayaran berhasil tersimpan.
- `400`: payload tidak konsisten, kartu tidak cocok dengan pasien, kartu/perusahaan tidak aktif,
  tidak eligible, atau di luar masa berlaku.
- `401`: pengguna belum terautentikasi.
- `403`: pengguna tidak memiliki `PatientEncounter : Create`.
- `500`: transaksi gagal disimpan; encounter dan sumber pembayaran tidak boleh tersimpan
  separuh.

Route `POST /` dan `POST /kiosk` tidak menerima `CompanyGuarantor` dalam kontrak ini. Keduanya
tetap mengikuti kemampuan existing Tunai/Asuransi. Implementasi harus memisahkan jalur internal
admin dari kiosk walaupun source saat ini mendelegasikan method admin ke method kiosk.

## 8. Batas cakupan

Termasuk:

- enum, request/response DTO, validasi server, persistence, mapping list/detail/create, summary,
  konfigurasi EF, migration code, dan automated test backend;
- regresi Tunai dan Asuransi;
- pembuktian bahwa route admin menerima Penjamin Perusahaan dan route kiosk menolaknya.

Tidak termasuk:

- perubahan source frontend `FE-RWI-025`;
- alur pengunggahan atau persetujuan surat jaminan;
- perhitungan tagihan, limit tahunan, sisa limit, co-payment, dan excess pasien;
- penerapan migration ke database bersama/target;
- perubahan hak akses di luar `PatientEncounter : Create`.

## 9. Bukti source saat kontrak dikunci

| Bukti | Temuan |
| --- | --- |
| Backend `64d7419…`, `EncounterPaymentType.cs` | Hanya `Cash = 1` dan `Insurance = 2` |
| Backend `64d7419…`, `PatientEncounterDtos.cs` | Request hanya membawa `PaymentMethodId` dan `PatientInsuranceId` |
| Backend `64d7419…`, `PatientEncounterController.cs` | Validasi menolak tipe selain Tunai/Asuransi; route `/admin` dijaga `PatientEncounter : Create` tetapi mendelegasikan proses ke method kiosk |
| Backend `64d7419…`, `TrxPatientEncounterGuarantor.cs` | Persistence hanya mempunyai referensi Tunai/Asuransi |
| Backend `64d7419…`, `MstPatientCompanyGuarantor.cs` | Hubungan pasien-perusahaan sudah tersedia lengkap dengan perusahaan, nomor karyawan, benefit plan, masa berlaku, status aktif, dan eligibility |

## 10. Gerbang implementasi

`BE-RWI-035` boleh mulai karena keputusan produk dan kontrak target sudah `APPROVED`. Pelaksana
tetap wajib menjalankan QBE preflight dari `AGENTS.md` backend pada waktu eksekusi, memeriksa
drift source dari snapshot di atas, dan berhenti bila perubahan baru membuat kontrak ini tidak
lagi aman diterapkan.
