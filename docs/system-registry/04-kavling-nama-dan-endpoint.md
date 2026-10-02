# Kavling Nama dan Endpoint

Diaudit pada backend `f2c5090`.

Berkas ini mencegah dua modul mengambil nama atau alamat yang sama.

## Aturan prefix entity

| Prefix | Untuk apa | Contoh | Terpakai |
| --- | --- | --- | ---: |
| `Trx` | Data kejadian atau transaksi pelayanan | `TrxPatientEncounter` | 220 |
| `Mst` | Data induk yang jarang berubah | `MstClinic` | 180 |
| `Wfp` | Profil dan siklus kerja pegawai | `WfpPayroll` | 40 |
| `Bil` | Tagihan dan kasir | `BilInvoice` | 28 |
| `Hrd` | Kehadiran dan hubungan kepegawaian | `HrdAttendanceDaily` | 15 |
| `Opr` | Kamar operasi | `OprCase` | 13 |
| `Inp` | Rawat inap | `InpEpisode` | 11 |
| `Sys` | Kebutuhan teknis dan pengaturan sistem | `SysAccessPolicy` | 6 |
| `Lab` | Laboratorium | `LabSpecimen` | 1 |
| `ApplicationUser` | Identitas pengguna | `ApplicationUserOrganization` | 2 |

Seluruh 516 entity memakai salah satu prefix di atas. Tidak ada entity tanpa prefix yang
dikenali.

Prefix `Lab` baru dipakai satu entity, sedangkan entity laboratorium lainnya masih memakai
`Mst`. Ini belum tentu salah, tetapi perlu diketahui modul baru agar tidak bingung memilih.

## Nama yang sudah dipakai

Seluruh 516 nama entity tercantum pada [berkas 06](06-indeks-entity.md). Periksa daftar itu
sebelum menetapkan nama entity baru. Nama yang sudah dipakai tidak boleh diambil ulang,
walaupun berada di area berbeda.

Prefix yang **belum pernah dipakai** dan karena itu tersedia untuk modul baru:

| Prefix | Belum dipakai |
| --- | --- |
| `Gz` atau `Nut` | Gizi atau nutrisi |
| `Rad` | Radiologi |
| `Inv` | Persediaan |
| `Ast` | Aset |

Tidak ada satu pun entity bernama mengandung `Gizi`, `Nutrition`, `Diet`, atau `Nutri` di
seluruh sistem. Modul Gizi akan berangkat dari nol untuk entity-nya sendiri, tetapi tetap
memakai data bersama pada [berkas 03](03-kepemilikan-data-bersama.md).

## Base URL yang sudah dipakai

| Awalan | Jumlah controller |
| --- | ---: |
| `api/v1/corporate/human-resource/...` | 134 |
| `api/v1/health-services/master-data/...` | 34 |
| `api/v1/health-services/clinical-management/...` | 16 |
| `api/v1/self-services/human-resource/...` | 13 |
| `api/v1/health-services/billing-management/...` | 13 |
| `api/v1/administrator/master-data/...` | 12 |
| `api/v1/health-services/emergency-installation-management/...` | 9 |
| `api/v1/health-services/pharmacy-management/...` | 8 |
| `api/v1/health-services/patient-management/...` | 7 |
| `api/v1/health-services/registration-management/...` | 6 |
| `api/v1/health-services/operating-room-management/...` | 5 |
| `api/v1/health-services/inpatient-management/...` | 5 |

Modul baru wajib memakai awalan yang konsisten dengan areanya. Modul Gizi, bila kelak dibuat,
akan memakai `api/v1/health-services/<nama-modul>/...`.

## Grup Swagger yang sudah terdaftar

Terdapat **269 grup `[Tags(...)]` unik** pada 276 controller. Sebarannya:

| Awalan grup | Jumlah |
| --- | ---: |
| `Corporate / ...` | 137 |
| `Health Services / ...` | 107 |
| `Self Services / ...` | 15 |
| `Administrator / ...` | 13 |
| `01-Authentication` dan `02-Version` | 2 |

Registry hanya memuat sebaran grup, bukan seluruh endpoint. Rincian endpoint per modul adalah
tugas `/trace-existing-capabilities`.

## Base URL dan grup yang dipakai lebih dari satu controller

Berbagi base URL tidak otomatis salah. Yang perlu diperiksa adalah apakah pembagiannya
disengaja.

| Base URL | Controller | Disengaja? |
| --- | --- | --- |
| `/v1/health-services/operating-room-management/cases` | `OperatingRoomCaseController`, `OperatingRoomScheduleController` | Ya. Permission berbeda per aksi, jadi dipisah controller dengan base sama |
| `/v1/health-services/operating-room-management/cases/{caseId}/execution` | `OperatingRoomExecutionController`, `OperatingRoomRecoveryController`, `OperatingRoomMaterialController` | Ya. Alasan yang sama |
| `/v1/health-services/billing-management/billing/patient-funds` | Dua controller | Belum diperiksa |
| `/v1/corporate/human-resource/workflow-instances` | Dua controller | Belum diperiksa |
| `/v1/[controller]` | `AuthController`, `VersionController` | Token belum diganti; base URL baru terbentuk saat runtime |

Dua baris terakhir masuk zona konflik `KF-006` dan `KF-007`.

## Controller tanpa grup Swagger

Dua controller tidak memiliki atribut `[Tags(...)]`, sehingga endpoint-nya masuk grup bawaan
di halaman Swagger dan sulit ditemukan pembaca:

| Controller | Base URL |
| --- | --- |
| `Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionPreparationController.cs` | `/v1/health-services/pharmacy-management/prescription-preparations` |
| `Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionReviewController.cs` | `/v1/health-services/pharmacy-management/prescription-reviews` |
