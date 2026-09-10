# Peta Area dan Modul

Diaudit pada backend `f2c5090` dan frontend `847be1fc0`.

Berkas ini menjawab satu pertanyaan: **sistem ini terdiri dari apa saja, dan siapa pemilik
tiap bagian.**

## Peta area

| Area | Isi singkat | Entity | Controller | Pemilik proses bisnis |
| --- | --- | ---: | ---: | --- |
| `Corporate` | Kepegawaian, penggajian, kehadiran, pengembangan pegawai | 337 | 137 | Human Resource |
| `HealthServices` | Pelayanan pasien dari pendaftaran sampai farmasi dan billing | 149 | 109 | Belum ditentukan |
| `Administrator` | Pengaturan sistem, hak akses, data induk administratif | 18 | 13 | Belum ditentukan |
| `Global/Shared` | Versi aplikasi, hak akses, dan identitas pengguna | 12 | 2 | Belum ditentukan |
| `SelfServices` | Layanan mandiri pegawai dan biometrik | 0 | 15 | Human Resource |

Kolom **Pemilik proses bisnis** diisi `Belum ditentukan` bila tidak ada bukti tertulis siapa
yang berwenang mengubah aturannya. Ini bukan kelalaian pemindaian; menebak pemilik justru
melanggar batas kewenangan skill scan. Seluruh baris `Belum ditentukan` masuk zona konflik
`KF-3` pada berkas 05.

`SelfServices` memiliki controller tetapi tidak memiliki entity sendiri. Modul itu bekerja di
atas entity milik Human Resource.

## Modul di dalam area HealthServices

| Modul | Prefix entity | Entity | Sebaran tingkat | Pemilik data |
| --- | --- | ---: | --- | --- |
| Billing Management | `Bil`, `Mst` | 35 | 32 `L2`, 3 `L3` | Belum ditentukan |
| Master Data | `Mst` | 34 | 31 `L4`, 3 `L3` | Belum ditentukan |
| Pharmacy Management | `Mst`, `Trx` | 17 | 5 `L4`, 10 `L3`, 2 `L2` | Belum ditentukan |
| Clinical Management | `Trx` | 14 | 14 `L4` | Belum ditentukan |
| Operating Room Management | `Opr` | 13 | 13 `L3` | Belum ditentukan |
| InPatient Management | `Inp` | 11 | 9 `L3`, 2 `L2` | Belum ditentukan |
| Emergency Installation | `Trx`, `Mst` | 9 | 9 `L4` | Belum ditentukan |
| Patient Management | `Mst` | 7 | 7 `L4` | Belum ditentukan |
| Registration Management | `Trx` | 4 | 4 `L4` | Belum ditentukan |
| Laboratory Management | `Lab`, `Mst` | 4 | 1 `L3`, 3 `L2` | Belum ditentukan |
| Clinical Billing Integration | `Trx` | 1 | 1 `L2` | Belum ditentukan |

Yang menonjol dari tabel ini:

**Billing Management adalah modul terbesar di HealthServices, tetapi hampir seluruhnya masih
`L2`.** Tabelnya sudah ada, controller-nya sebagian sudah ada, tetapi belum ada satu pun layar
frontend yang memanggilnya. Modul lain yang berencana mengirim tagihan ke sana perlu tahu ini.

**Operating Room Management seluruhnya `L3`, bukan `L4`.** Layar frontend-nya sudah dibuat,
tetapi penentuan Consumer pada pemindaian ini memakai perbandingan base URL, dan modul itu
baru saja ditambahkan sehingga sebagian pemanggilannya belum tercakup pada perbandingan
awalan. Rinciannya ada pada berkas 02.

## Modul di dalam area Corporate

Seluruhnya di bawah Human Resource. Sepuluh terbesar:

| Modul | Entity |
| --- | ---: |
| Master Data | 87 |
| Workforce Core | 21 |
| Lifecycle Management | 21 |
| Recruitment Management | 20 |
| Payroll Management | 19 |
| Credentialing Management | 18 |
| Leave Management | 17 |
| Attendance Management | 14 |
| Learning and Development | 13 |
| Business Travel Management | 13 |

Sisanya sepuluh modul lagi dengan 7 sampai 11 entity: Workforce Planning, Scheduling
Management, Performance Management, Overtime Management, Occupational Health Management,
Benefit Management, Workflow Management, HR Service Management, Employee Relation Management,
dan Expense Management.

## Catatan struktur

Bagian ini penting untuk modul baru. Developer cenderung meniru folder terdekat, sehingga
penyimpangan menyebar bila tidak ditandai.

| Temuan | Lokasi | Keterangan |
| --- | --- | --- |
| Folder `DTOS` huruf besar | `Areas/HealthServices/RegistrationManagement/DTOS` | Area lain memakai `DTOs`. Utang teknis, jangan ditiru |
| Folder `Dtos` huruf kecil | `Areas/HealthServices/BillingManagement/Billing/Dtos` dan `.../Cashier/Dtos` | Area lain memakai `DTOs` |
| Model di dalam pohon configuration | `Repositories/Configurations/Corporate/HumanResource/MasterData/EmployeeRelation/Models/` | Empat model diletakkan di dalam folder EF configuration, bukan di `Areas/`. Rinciannya pada zona konflik `KF-005` |
| Route memakai token `[controller]` | `Controllers/AuthController.cs` dan `Controllers/VersionController.cs` | Keduanya memakai `Route("api/v1/[controller]")`. Base URL-nya baru terbentuk saat runtime sehingga tidak dapat dibandingkan dengan pemakaian frontend |

## Batas pemindaian ini

Yang **tidak** diperiksa:

- isi database sungguhan, termasuk apakah migration benar-benar sudah diterapkan;
- service eksternal dan environment produksi;
- apakah sebuah layar frontend benar-benar dapat dicapai pengguna dari menu.

Ketiganya memerlukan lingkungan berjalan, bukan pembacaan source.
