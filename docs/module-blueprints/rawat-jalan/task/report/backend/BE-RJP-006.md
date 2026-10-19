# Laporan Perubahan Backend — `BE-RJP-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RJP-006` |
| Judul | Buat reservasi dan kueri |
| Slice | `MVP-1`, `EPIC RP-04` |
| Roadmap | `docs/module-blueprints/rawat-jalan/roadmap/priority-reservation-backend-roadmap.md` bagian `BE-RJP-006` |
| Trace | `FR-RP-030` s.d. `032`, `FR-RP-062`; `RJ-PRI-DEC-007`, `008`, `011`, `012`, `021`; `VAL-RP-01` s.d. `06`; `INV-RP-01` s.d. `03`, `05`, `06` |
| Contract version | `RJ-PRI-RESV-001@1.0.0` `approved`; rework 19 Okt 2026: `@1.1.0` `approved` (`RJ-PRI-DEC-032`) — `api-contract.md` *Outpatient Priority Reservation*; `permission-audit-matrix.md` RP-1 |
| Dependency | `BE-RJP-005` ✅ |
| Klasifikasi | `HIGH` — skor 8: diperiksa 20+ (2), diubah 4 (0), logika tinggi (2: kunci, snapshot, hash), kontrak baru (2), keamanan berkaitan (1: kode link), workflow (1) |
| Task mode | `BACKEND` (`CROSS-REPO MODE`, `RJ-PRI-DEC-031`) |
| Target tulis | Controller, service, dan DTO baru di `Areas/HealthServices/RegistrationManagement/`; `Program.cs` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `a6499cdd` (`sukmagp`), working tree belum di-commit |
| Tanggal | 2026-10-16; rework 2026-10-19 |
| Status | ✅ Selesai — runtime 26/26 `PASS`; rework 19 Okt 2026 runtime 11/11 `PASS` (bagian 8) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` |
| Pemilik / prefix | Registration, `Reg` (registry `ACTIVE`) |
| Keberlakuan | `NEW CODE` (controller, service, DTO); `TOUCHED LEGACY` (`Program.cs`) |
| Arketipe endpoint | Transaksi ber-lifecycle. Tidak ada `GET /options`, `PATCH /{id}/status`, maupun `DELETE /{id}` |
| QBE yang berlaku | `QBE-SVC-001` (controller tanpa `ApplicationDbContext`), `QBE-API-001` (`ApiResponse<T>`), `QBE-CODE-001`/`003`/`006` (nomor lewat `NumberSeriesAllocator`, bukan Count/Max+1), `QBE-DTO-001`, `QBE-VAL-001`, `QBE-PERM-001` |
| Role access | `[AccessController(ControllerName = "OutpatientPriorityReservation")]`. Setiap action punya `[AccessAction]` dan `[AccessPermission]` dengan argumen sama: `Read` (`AccessTypes.Read`) dan `Create` (`AccessTypes.Create`). Tidak ada pemeriksaan nama peran |

---

## 1. Masalah yang diselesaikan

Admisi belum bisa mencatat reservasi pasien prioritas yang datang lewat WA atau telepon (PDF 2A–2B). Sesudah task ini,
admisi bisa mengecek kelayakan pasien, membuat reservasi, lalu mendapat link konfirmasi kedatangan dan teks WA siap kirim.

## 2. Proses bisnis

1. Pasien member menelepon dan meminta datang pada tanggal tertentu.
2. Admisi mencari pasien, lalu sistem mengecek kelayakan: member Aktif hari ini, tanggal hari ini sampai 30 hari ke
   depan, belum punya reservasi terbuka, dan belum punya kunjungan Rawat Jalan aktif pada tanggal itu.
3. Admisi menekan Buat. Sistem menerbitkan nomor `RSV`, membuat kode link acak, dan menyimpan snapshot klasifikasi member.
4. Respons memuat link konfirmasi **sekali saja**, teks WA, dan No. HP pasien. Admisi mengirim pesan lewat `wa.me`.

Contoh dari uji: pasien Gold dibuatkan reservasi untuk 18 Okt 2026. Sistem menjawab nomor `RSV-20261016-0006`, status
Menunggu Kedatangan, link `http://localhost:3000/priority-arrival/<43 karakter>`, dan teks WA:

```text
Reservasi pasien prioritas Anda sudah tercatat.
Nomor reservasi: RSV-20261016-0006
Tanggal kunjungan: Minggu, 18 Oktober 2026

Saat tiba di rumah sakit pada tanggal tersebut, buka link berikut lalu tekan "Saya sudah tiba":
<link>

Petugas kami akan menghampiri Anda di Ruang Tunggu Prioritas.
```

Pesan tidak menyebut poli maupun dokter (`RJ-PRI-FE-002`). Kalimat persisnya `DEV_DISCRETION`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`NumberSeriesAllocator` dan `NumberAllocationRequest`, `OutpatientEncounterRules` (`WhereBlocksRegistration`),
`PatientEncounterController` (kunci per pasien `RJ-DOC-REV-BE-007`), `PatientMembershipStatusResolver`,
`OutpatientQueueClassificationService`, `BbkBloodBankProcedureController` (pola controller tipis), `BillingInvoiceService`
(pola `ILIKE` ber-escape), `LoggerService`, `ApplicationUser`, `MstPatient`.

### 3.2 Berkas yang berubah

| Berkas | Status | Perubahan |
| --- | --- | --- |
| `Areas/HealthServices/RegistrationManagement/Controllers/OutpatientPriorityReservationController.cs` | Baru | `GET /`, `/summary`, `/eligibility`, `/{id}`, `POST /`. Controller hanya meneruskan ke service |
| `Areas/HealthServices/RegistrationManagement/Services/OutpatientPriorityReservationService.cs` | Baru | Kueri daftar, ringkasan, detail, kelayakan; perintah buat; pembuat respons terbit; bantuan bersama (hash kode, status, riwayat) untuk task berikutnya |
| `Areas/HealthServices/RegistrationManagement/DTOS/OutpatientPriorityReservationDtos.cs` | Baru | Semua DTO kontrak RP, termasuk yang dipakai `BE-RJP-007` |
| `Program.cs` | Diperbarui | `AddScoped<OutpatientPriorityReservationService>()` |

### 3.3 Aturan yang diterapkan

| Aturan | Penerapan |
| --- | --- |
| `VAL-RP-01` | Pasien tidak ada, terhapus, atau nonaktif: `404` "Data pasien tidak ditemukan." |
| `VAL-RP-02` | Klasifikasi pada hari ini (WIB) lewat resolver CRM bukan member: `409` |
| `VAL-RP-03`, `04` | Tanggal < hari ini atau > hari ini + `MaxDaysAhead` (30): `400` dengan batas tanggal |
| `VAL-RP-05` | Reservasi terbuka pada tanggal itu: `409` menyebut nomor yang ada. Lapis kedua: unique index bersyarat (`23505`) dijawab `409` |
| `VAL-RP-06` | Kunjungan RJ berpoliklinik yang masih menghalangi (`WhereBlocksRegistration`) pada tanggal itu: `409` |
| `INV-RP-01` | Pemeriksaan diulang di dalam transaksi, di bawah `pg_advisory_xact_lock(hashtext('REG_ENCOUNTER_ACTIVE_<patientId>'))`. Kuncinya sama dengan pendaftaran kunjungan, sehingga reservasi dan kunjungan untuk pasien yang sama tidak saling mendahului |
| `INV-RP-03` | Snapshot klasifikasi (`IsPrioritySnapshot`, level, audience, display mode, membership, tingkat) diambil sekali saat dibuat |
| `INV-RP-05` | Kode 32 byte acak (`RandomNumberGenerator`), base64url 43 karakter. Yang disimpan hanya hex SHA-256 huruf kecil. Kode hanya ada di respons `POST /` |
| `INV-RP-06` | Riwayat `CREATED` ditulis dalam transaksi yang sama |
| Nomor | `NumberSeriesAllocator`, `SequenceKey` `REG_PRIORITY_RESERVATION_NUMBER`, prefix `RSV`, reset `DAILY`, 4 digit. Bentuk `RSV-yyyyMMdd-NNNN` (`RJ-PRI-OQ-002`: mengikuti format allocator). Deret dibuat otomatis oleh allocator pada pemakaian pertama, jadi tidak perlu seed |

### 3.4 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Lima endpoint baru, aditif |
| Database | Tidak ada perubahan schema. `has-pending-model-changes` bersih |
| Keamanan | Log controller hanya memuat `EntityId`, nomor reservasi, status, controller, dan aksi. Tanpa kode, hash, No. HP, checklist, maupun alasan |
| Hak akses | Butir baru `OutpatientPriorityReservation : Read` dan `Create` muncul di layar Akses Role |

## 4. Dokumentasi endpoint

#### Health Services / Registration Management / Outpatient Priority Reservation

Base URL: `api/v1/health-services/registration-management/outpatient-priority-reservations`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar reservasi | `OutpatientPriorityReservation : Read` | `visitDateFrom`, `visitDateTo` (bawaan hari ini), `status?`, `search?` (≥ 3 karakter), `pageNumber`, `pageSize` (≤ 100) | `ApiResponse<PagedResult<PriorityReservationListItemResponse>>` | Tersedia |
| `GET` | `/summary` | Jumlah per status pada satu tanggal | `OutpatientPriorityReservation : Read` | `visitDate?` | `ApiResponse<PriorityReservationSummaryResponse>` | Tersedia |
| `GET` | `/eligibility` | Cek kelayakan | `OutpatientPriorityReservation : Read` | `patientId`, `visitDate?` | `ApiResponse<PriorityReservationEligibilityResponse>` | Tersedia |
| `GET` | `/{id}` | Detail dan riwayat | `OutpatientPriorityReservation : Read` | — | `ApiResponse<PriorityReservationDetailResponse>` | Tersedia |
| `POST` | `/` | Buat reservasi | `OutpatientPriorityReservation : Create` | `{ "patientId": "…", "visitDate": "2026-10-18" }` | `ApiResponse<PriorityReservationIssuedResponse>` | Tersedia |

Kode status: `200` berhasil; `400` isian atau tanggal salah, kata kunci terlalu pendek; `403` tanpa hak; `404` pasien
atau reservasi tidak ada; `409` bukan member aktif, reservasi terbuka sudah ada, atau kunjungan RJ aktif sudah ada.

### 4.1 Delta terhadap kontrak

| Hal | Isi |
| --- | --- |
| Saklar `Enabled = false` | `POST /` menjawab `409` "Reservasi prioritas sedang dinonaktifkan." Kontrak belum menyebut kasus ini; tidak ada perilaku bisnis baru selain mematikan fitur |
| Bentuk tanggal di respons | `visitDate` diserialisasi sebagai `2026-10-18T00:00:00`, sama seperti respons Registration lain. Request menerima `yyyy-MM-dd` |
| Folder DTO | Desain menyebut `DTOs/`; folder fisiknya `DTOS/` (namespace `...RegistrationManagement.DTOs`), sama dengan DTO Registration lain. Tidak dirapikan |
| Kabar realtime | `PriorityReservationChanged` dikerjakan di `BE-RJP-010`, sesuai roadmap |

## 5. Verifikasi

Data samaran: lima pasien `RJPR UJI ...` dibuat lewat API admin. Membership Gold Aktif dan Menunggu Pembayaran disisipkan
lewat SQL data uji. Backend uji `https://localhost:7185` terhadap `QuilvianNewDevSukma`.

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| Build `-p:UseSharedCompilation=false` ke scratchpad | 0 error. Tidak ada warning dari berkas task ini | `PASS` |
| `has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` |
| QBE Strict, 4 berkas | VIOLATION 0, REVIEW 0, INFO 0 | `PASS` |
| `UAT-RP-07` a–f — Gold Aktif, reservasi lusa | `200`, Menunggu Kedatangan, `RSV-20261016-0006`, link 43 karakter, teks WA tanpa poli/dokter, No. HP dari master, snapshot `MBR-GLD-001`, riwayat `CREATED` dengan nama pelaku | `PASS` |
| `R-RP-23` (bagian kolom) | Kolom berisi hex SHA-256 64 karakter dari kode. Detail tidak memuat kode, hash, maupun link | `PASS` |
| Snapshot | `IsPrioritySnapshot = true`, level 2, membership terisi | `PASS` |
| `R-RP-10` | Tidak ada `RegPatientEncounter`, `RegQueue`, maupun baris di `GET /outpatient-encounters` untuk pasien itu | `PASS` |
| `R-RP-08` a, b / `UAT-RP-08` | Pasien Reguler dan pasien Menunggu Pembayaran: `409` `VAL-RP-02` | `PASS` |
| `UAT-RP-09` | Reservasi kedua tanggal sama: `409` "Pasien sudah punya reservasi prioritas RSV-20261016-0006 untuk 18 Oktober 2026." | `PASS` |
| `R-RP-09` paralel | Dua `POST` bersamaan dari dua sesi: `[200, 409]`, satu baris terbuka | `PASS` |
| `VAL-RP-03`, `04`, `01`, `06` | `400`, `400` (batas 15 November 2026), `404`, `409` | `PASS` |
| Batas 30 hari | Tepat hari ke-30 diterima | `PASS` |
| `GET /eligibility` | `OPEN_RESERVATION_EXISTS` (dengan nomor), `NOT_ACTIVE_MEMBER`, `DATE_OUT_OF_RANGE`, `ELIGIBLE`, `ACTIVE_ENCOUNTER_EXISTS`, `404` | `PASS` |
| `GET /` | Cari nama pasien: 1 baris dengan No. RM dan status, tanpa checklist. Kata kunci 2 karakter `400`. Rentang terbalik `400` | `PASS` |
| `GET /summary`, `GET /{id}` acak | Ringkasan menghitung Menunggu Kedatangan; id acak `404` | `PASS` |
| Log | 15 berkas log dan log konsol diperiksa terhadap 7 nilai rahasia (kode, hash, No. HP): tidak ada yang bocor | `PASS` |

Skrip: `rt_rjp006.py` di scratchpad sesi. Total 26/26 `PASS`.

**Tidak dijalankan:** `dotnet test` — `NOT RUN`. Uji `403` untuk pengguna tanpa hak tidak dijalankan karena akun uji yang
tersedia hanya SuperAdmin; atribut akses diperiksa QBE.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `R-RP-08` | Terpenuhi | a, b |
| `R-RP-09` termasuk paralel | Terpenuhi | Uji paralel |
| `R-RP-10` | Terpenuhi | DB dan `GET /outpatient-encounters` |
| `R-RP-23` kolom hanya hash | Terpenuhi | Bagian kolom. Link lama/baru diuji di `BE-RJP-007` dan `BE-RJP-012` |
| `UAT-RP-07` s.d. `09` | Terpenuhi | Lihat tabel |
| Log tanpa kode, hash, atau No. HP | Terpenuhi | Pemeriksaan log |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nomor `RSV-20261016-0001` s.d. `0005` terbit dari run uji pertama yang terhenti di tengah (login paralel di skrip uji). Nomor itu menempel pada reservasi uji yang sah dan bukan cacat produk |
| Masalah yang diketahui | — |
| Risiko tersisa | Bila `PublicArrivalBaseUrl` lupa disetel per lingkungan, link menunjuk ke `localhost`. Wajib dicek saat deploy |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Belum di-commit. Tidak ada stage, commit, push, merge, atau deploy |
| Langkah berikutnya | `BE-RJP-007` (perintah petugas) |

## 8. Rework 19 Oktober 2026 — kontrak `1.1.0` (`RJ-PRI-DEC-032`)

### 8.1 Pemicu dan proses bisnis

Owner meminta layar Daftar Reservasi Prioritas mengikuti pola modul lain: filter Tanggal Awal, Tanggal Akhir, dan
Periode, serta kolom Tanggal Dibuat, Dibuat Oleh, dan Diubah Oleh. Owner memilih agar rentang tanggal menyaring
**tanggal kunjungan**. Contoh: periode "7 Hari ke Depan" menampilkan semua pasien yang dijadwalkan datang 19–25 Oktober,
walaupun reservasinya dibuat seminggu sebelumnya.

Endpoint daftar sudah menerima `visitDateFrom`/`visitDateTo` sejak `1.0.0`. Yang belum ada: ringkasan untuk rentang yang
sama, dan siapa yang membuat dan mengubah reservasi.

### 8.2 Perubahan

| Berkas | Status | Isi |
| --- | --- | --- |
| `Areas/HealthServices/RegistrationManagement/DTOS/OutpatientPriorityReservationDtos.cs` | Diperbarui | `PriorityReservationListItemResponse` + `CreatedAt`, `CreatedByName`, `UpdatedAt`, `UpdatedByName`; `PriorityReservationSummaryResponse` + `VisitDateFrom`, `VisitDateTo` (`VisitDate` dipertahankan) |
| `Areas/HealthServices/RegistrationManagement/Controllers/OutpatientPriorityReservationController.cs` | Diperbarui | `GET /summary` menerima `visitDateFrom`, `visitDateTo`, dan `visitDate` lama; `400` bila tanggal awal setelah tanggal akhir |
| `Areas/HealthServices/RegistrationManagement/Services/OutpatientPriorityReservationService.cs` | Diperbarui | `GetSummaryAsync(from, to)` dengan validasi rentang; `FillAuditNamesAsync` (satu kueri audit + satu kueri nama per halaman); proyeksi daftar mengisi `CreatedAt`/`UpdatedAt`; konfirmasi lewat link dan penutupan otomatis kini mengosongkan `UpdateBy` |

Aturan "Diubah Oleh": diambil dari riwayat terakhir. Petugas memakai namanya; perubahan dari link konfirmasi tertulis
`Link Pasien`; penutupan otomatis tertulis `Sistem`. Reservasi yang belum pernah berubah sejak dibuat: `updatedAt` dan
`updatedByName` kosong.

Temuan yang ikut diperbaiki: sebelumnya konfirmasi lewat link dan penutupan otomatis mengubah `UpdateDateTime` tetapi
membiarkan `UpdateBy` berisi petugas terakhir. Akibatnya kolom audit bawaan bisa menunjuk petugas yang tidak melakukan
perubahan. Kini keduanya mengosongkan `UpdateBy`.

### 8.3 Endpoint yang berubah

**Health Services / Registration Management / Outpatient Priority Reservation**

| Method | Path | Request | Response | Hak akses | Kode status |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/registration-management/outpatient-priority-reservations` | `visitDateFrom`, `visitDateTo`, `status?`, `search?`, `pageNumber`, `pageSize` (tidak berubah) | Item + `createdAt`, `createdByName`, `updatedAt`, `updatedByName` | `OutpatientPriorityReservation : Read` | `200`, `400`, `401`, `403` |
| `GET` | `/api/v1/health-services/registration-management/outpatient-priority-reservations/summary` | `visitDateFrom`, `visitDateTo` (bawaan hari ini); `visitDate` lama tetap diterima | + `visitDateFrom`, `visitDateTo` | `OutpatientPriorityReservation : Read` | `200`, `400` (rentang terbalik), `401`, `403` |

Contoh respons item (dipangkas):

```json
{
  "reservationNumber": "RSV-20261019-0064",
  "status": 2,
  "createdAt": "2026-10-19T07:15:23.950036Z",
  "createdByName": "SuperAdmin",
  "updatedAt": "2026-10-19T07:15:28.758335Z",
  "updatedByName": "Link Pasien"
}
```

Kompatibilitas: aditif. Konsumen `1.0.0` yang memanggil `summary?visitDate=` mendapat hasil yang sama persis (R8).
Tidak ada perubahan atribut akses, sehingga Backend Governance Preflight di atas tetap berlaku (`QBE-SVC-001`,
`QBE-API-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-PERM-001`).

### 8.4 Verifikasi

| Langkah | Hasil |
| --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:UseSharedCompilation=false -o <scratchpad>/be-build` | `PASS` — 0 error; 0 warning di berkas task (warning lain sudah ada sebelumnya, mis. `DoctorQueueController.cs` CS8602 dan paket `SixLabors.ImageSharp`) |
| `dotnet ef migrations has-pending-model-changes --configuration Release` | `PASS` — tidak ada perubahan model; tidak ada migration |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` (3 berkas) | `PASS` — VIOLATION 0, REVIEW 0 |
| Runtime HTTP `https://localhost:7185` terhadap `QuilvianNewDevSukma` | `PASS` 11/11, rincian di bawah |
| `dotnet test` | `NOT RUN` — tidak ada project test (pola Bank Darah) |

| ID | Skenario | Hasil |
| --- | --- | --- |
| R1 | Daftar hari ini memuat keempat field audit (56 item) | `PASS` |
| R2 | `createdAt` terisi; `createdByName` = akun pembuat (`SuperAdmin`) | `PASS` |
| R3 | Dikonfirmasi lewat link: `updatedByName = "Link Pasien"` | `PASS` |
| R4 | Dikembalikan ke Belum Tiba oleh petugas: `updatedByName` = nama petugas | `PASS` |
| R5 | Belum pernah berubah (reservasi besok): `updatedAt` dan `updatedByName` kosong | `PASS` |
| R6 | Ditutup otomatis (riwayat terakhir `NO_SHOW` dari sistem, 8 item): `updatedByName = "Sistem"` | `PASS` |
| R7 | Ringkasan rentang 30 hari ke belakang s.d. 30 hari ke depan: jumlah semua status 75 = `totalData` daftar 75; `visitDateFrom`/`visitDateTo` di respons | `PASS` |
| R8 | `summary?visitDate=` sama persis dengan rentang satu hari | `PASS` |
| R9 | Tanggal awal setelah tanggal akhir: `400` "Tanggal awal tidak boleh setelah tanggal akhir." di ringkasan dan daftar | `PASS` |
| R10 | Tanpa login: `401` | `PASS` |
| R11 | Kirim ulang link (`UpdateBy` = petugas) lalu pasien konfirmasi: `UpdateBy` dikosongkan, daftar menulis `Link Pasien` | `PASS` |

Catatan data uji: dua reservasi Tidak Datang (`RSV-20261019-0049`, `0055`) dibuat langsung lewat SQL pada uji
sebelumnya tanpa riwayat. Keduanya menampilkan kolom "Diubah Oleh" kosong, dan itu benar karena tidak ada riwayat
perubahan.

### 8.5 Catatan penutup rework

| Hal | Isi |
| --- | --- |
| Migration / database | Tidak ada migration. Data uji: satu pasien `BE-UPDBY` dan reservasinya, serta `RSV-20261019-0068` (pasien `FE-PUBLIC` baru) di `QuilvianNewDevSukma` |
| Backend user | Backend user di port `7184` (`bin/Debug`) tidak dihentikan. Field baru baru terlihat di sana sesudah user menjalankan ulang backend-nya |
| Status Git | Belum di-commit. Tidak ada stage, commit, push, merge, atau deploy |
| Langkah berikutnya | `FE-RJP-004` rework (filter, kolom, tombol di kartu filter) dan `FE-RJP-005` (warna tombol) |
