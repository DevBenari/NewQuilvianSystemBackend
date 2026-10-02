# Impact Scan Kemampuan — Daftar Pasien Rawat Jalan

| Field | Nilai |
|---|---|
| Blueprint | `rawat-jalan` (umbrella `RJ-BIL-BP-001`), scope `RJ-DOC` |
| Mode | Impact scan terbatas untuk amendment `2026-10-02` |
| Keputusan yang diaudit | `RJ-DOC-DEC-011` sampai `RJ-DOC-DEC-020`, `RJ-DOC-FE-005` sampai `RJ-DOC-FE-009` ([00-interview-decisions.md](00-interview-decisions.md), bagian *Amendment Pass 2026-10-02*) |
| Backend SHA | `245f0464` |
| Frontend SHA | `b7e9b7fd4` |
| Capability map induk | [01-existing-capability-map-prd-v2.md](01-existing-capability-map-prd-v2.md) (SHA BE `063d38b`). Berkas ini **menambah**, bukan menggantikan, map induk |
| Kontrak yang berlaku | Tidak ada kontrak baru. Kontrak as-is `PatientEncounterController` dicatat di bagian 3 |
| Status | `CURRENT` — `CONFLICT-DP-1` ditutup `RJ-DOC-DEC-022`, `CONFLICT-DP-2` ditutup `RJ-DOC-DEC-021`, `UNK-DP-1` terjawab (lihat bagian 5) |

## 1. Batas audit

Yang diperiksa hanya kemampuan yang disentuh layar baru:

- data kunjungan dan penandanya;
- endpoint daftar dan batal kunjungan;
- cara menentukan "dokter yang login" dan "perawat yang login";
- pola hak akses tambahan;
- perpindahan status kunjungan yang menyangkut status 6;
- pemakai definisi "kunjungan masih berjalan";
- base component frontend serta pola menu/route.

Aturan internal antrean, billing, IGD, dan Rawat Inap **tidak** diaudit. Database tidak
di-query; angka data hanya dikutip dari laporan yang sudah ada.

## 2. Peta kemampuan

| ID | Kebutuhan | Pemilik | Bukti (`path#symbol@SHA`) | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `CAP-DP-01` | Membedakan kunjungan Rawat Jalan dari IGD/Rawat Inap (`RJ-DOC-OQ-009`) | Registration | `RegPatientEncounter.cs#EncounterType@245f0464`; `EncounterType.cs` (`Outpatient=1`, `Emergency=2`, `Inpatient=3`); `InpEpisodeService.cs:1158`; `EncounterIntakeService.cs:326-345`; `EmergencyEncounterReconciliation.cs:298-301` | Reuse with adapter | `EncounterType == Outpatient` saja **tidak cukup**: (a) pasien penunjang langsung (lab/radiologi walk-in) juga `Outpatient`, tanpa klinik, tanpa antrean, `IsDoctorRequired=false`; (b) ada kunjungan IGD lama bertipe `Outpatient` yang punya `EmgVisit`. Penyaring Rawat Jalan yang aman: `EncounterType == Outpatient` **dan** `ClinicId` terisi **dan** tidak punya `EmgVisit` | Lihat `CONFLICT-DP-1` |
| `CAP-DP-02` | Daftar kunjungan yang disaring per pengguna | Registration | `PatientEncounterController.cs#GetPatientEncounters` (`GET /`, `GET /admin`, baris 191-195) + `BuildQuery` (baris 1240-1302) | Extend | Endpoint sekarang **tidak menyaring per pengguna**: siapa pun pemegang `PatientEncounter : Read` membaca semua kunjungan. Butuh endpoint baru yang menyaring di server (`RJ-DOC-DEC-013`). Filter yang sudah ada (tanggal, status, klinik, dokter, pencarian no. RM/nama/no. kunjungan) dapat dipakai ulang | Endpoint lama tetap terbuka lebar; di luar scope, dicatat sebagai temuan |
| `CAP-DP-03` | Menentukan dokter yang login | Registration (antrean dokter) | `DoctorQueueController.cs#ResolveAllowedDoctorIdAsync` (baris 919-960) | Reuse with adapter | Method `private` di controller. Urutan pencarian: klaim `doctor_id` → `workforce_profile_id` → `MstDoctor.WorkforceProfileId` → kecocokan email. Cabang SuperAdmin memakai nama role (`IsCurrentUserSuperAdminAsync`) dan **tidak boleh ditiru** (`RJ-DOC-DEC-014`). Perlu diangkat ke service bersama tanpa cabang SuperAdmin | Kecocokan lewat email: salah isi email di master dokter = salah cakupan |
| `CAP-DP-04` | Menentukan cluster perawat yang login | Registration (nurse station) | `NurseStationQueueController.cs#GetAllowedClusterIdsAsync`, `#ResolveCurrentEmployeeAsync`, `#GetClinicIdsByClusterIdsAsync` (baris 892 dst.) | Reuse with adapter | Sama: `private`, bercabang SuperAdmin. Urutan: klaim `employee_id` → `workforce_profile_id` → email → `MstNurseStationClusterStaff.EmployeeId` → klinik cluster | Sama seperti `CAP-DP-03` |
| `CAP-DP-05` | Hak "lihat semua" tanpa nama role (`RJ-DOC-OQ-010`) | Shared Platform | `RadiologyExplicitPermissions.cs` (`[assembly: AccessExplicitPermission(...)]`); `AccessPermissionService.cs#HasAccessAsync@245f0464` | Missing — pola **Ready to reuse** | Butir baru belum ada. Pola penanda eksplisit sudah dipakai Radiologi (`RadReport : ActAsRadiologist`): butir terdaftar sekali, dapat dicentang di layar Akses Role, dan diperiksa lewat `HasAccessAsync(user, resource, action)` | — |
| `CAP-DP-06` | Membatalkan kunjungan dengan guard | Registration | `PatientEncounterController.cs#CancelEncounter` (baris 1070-1112), `#CancelQueuesByEncounterAsync`, `QueueRealtimeService.NotifyQueueCancelledAsync`; `PatientEncounterDtos.cs#PatientEncounterCancelRequest` | Reuse with adapter | Logika batal (isi kolom batal, batalkan antrean, kabari layar antrean) dapat dipakai ulang. Guard baru (status 0-5, cakupan pengguna) dipasang di endpoint baru (`RJ-DOC-DEC-017`) | — |
| `CAP-DP-07` | Pemanggil `PATCH /patient-encounters/{id}/cancel` lama (`RJ-DOC-OQ-008`) | — | Pencarian `patient-encounters` dan `/cancel` di `V2QuilvianSystemFrontendDev/src@b7e9b7fd4`: tidak ada pemanggil; tidak ada pemanggil internal backend | Ready to reuse | **Frontend tidak memanggil endpoint ini sama sekali.** Satu-satunya jejak pemakaian adalah uji runtime `RJ-DOC-REV-BE-007` (R4). Mengetatkan endpoint lama ternyata tidak memutus layar mana pun | Pihak luar (Postman/integrasi) tidak dapat dilacak dari source |
| `CAP-DP-08` | Jalan keluar dari status 6 `Sedang Konsultasi` (`RJ-DOC-OQ-007`) | Clinical + Registration | Masuk ke 6: `DoctorQueueController.cs:419` (mulai konsultasi dari antrean), `DoctorConsultationController.cs:630` (buat konsultasi tanpa langsung selesai). Keluar ke 7: `ConsultationFinalizationService.cs:156`. **Batal konsultasi** `DoctorConsultationController.cs#CancelConsultation` (baris 1106-1180): hanya mengubah konsultasi, **tidak** menyentuh kunjungan maupun antrean. Tidak hadir dari antrean dokter (`DoctorQueueController.cs:625-631`) hanya dari `Skipped`/`CalledByDoctor` | Conflict | Bila dokter membatalkan konsultasi, kunjungan **tertahan di status 6 selamanya**: tidak ada endpoint yang memindahkannya, dan `RJ-DOC-DEC-016` menolak pembatalan status 6 | Lihat `CONFLICT-DP-2` |
| `CAP-DP-09` | Definisi pemblokir pendaftaran terpisah dari akses rekam medis (`RJ-DOC-DEC-019`) | Registration / Medical Record | `MedicalRecordAccessAuditService.cs#KunjunganMasihBerjalan` (baris 73-96) dipakai di baris 286 dan 328 (akses rekam medis) serta `PatientEncounterController.cs#FindActiveEncounterAsync` (baris 1311-1318) | Extend | Pendaftaran perlu definisi sendiri (status < 7). Definisi rekam medis tidak disentuh. Catatan: pemblokir berlaku untuk **semua** jenis kunjungan, termasuk IGD, Rawat Inap, dan penunjang | Lihat `CONFLICT-DP-1` |
| `CAP-DP-10` | Penutupan otomatis kunjungan yang ditinggalkan | Registration | `KioskEncounterClosureService.cs` | Ready to reuse (tidak diubah) | Sudah menutup kunjungan **kiosk penunjang** yang tidak dilanjutkan menjadi `NoShow` pada akhir hari. Tidak berlaku untuk kunjungan poliklinik | — |
| `CAP-DP-11` | Kerangka layar: hero, summary card, filter, tabel | Frontend platform | `components/features/base-features/hero.jsx`, `summary-grid.jsx`, `data-filter.jsx`, `data-table.jsx`, `status-badge.jsx`, `row-action-menu.jsx`, `confirm-modal.jsx`, `access-denied-gate.jsx`, `toast-stack.jsx`, `filter-date-picker.jsx`, `filter-select.jsx`; `components/features/pagination/pagination` | Ready to reuse | Contoh lengkap pemakaian keempatnya: `components/view/administrator/master-data/bank/administrator-bank-view.jsx` | — |
| `CAP-DP-12` | Menu dan route | Frontend platform | `utils/menu-sidebar/menu-items.jsx:1340-1357` (grup `healthServicesRegistrationManagement`; entri "Daftar Kunjungan" dikomentari); pola `app/health-services/registration-management/nurse-station-queue/page.jsx` → `*-client.jsx` → `*-view.jsx` | Ready to reuse | Entri baru disisipkan setelah "Skrining Pasien". Route `patient-encounters` belum ada | — |
| `CAP-DP-13` | Menyembunyikan tombol menurut hak akses | Frontend platform | `lib/hooks/auth/use-permission.jsx#usePermission(resource, action)`, `#useEffectivePermissions` | Ready to reuse | Server tetap wajib menolak 403 | — |

## 3. Kontrak as-is yang disentuh

**Grup Swagger:** `Health Services / Registration Management / Patient Encounter`
(`api/v1/health-services/registration-management/patient-encounters`)

| Method | Path | Hak akses | Perilaku sekarang |
|---|---|---|---|
| `GET` | `/`, `/admin` | `PatientEncounter : Read` | Daftar semua kunjungan, tanpa penyaringan per pengguna |
| `GET` | `/summary`, `/admin/summary` | `PatientEncounter : Read` | Ringkasan semua kunjungan |
| `PATCH` | `/{id}/cancel`, `/admin/{id}/cancel` | `PatientEncounter : Update` | Menolak hanya bila `CompletedAt` terisi; status 6-8 tetap bisa dibatalkan |
| `PATCH` | `/{id}/status` | `PatientEncounter : Update` | Ubah status bebas. Dilarang dipakai Rawat Jalan untuk `Completed` (`RJ-E2E-DEC-007`) |

**Grup Swagger:** konsultasi dokter (`DoctorConsultationController`)

| Method | Path | Hak akses | Efek ke status kunjungan |
|---|---|---|---|
| `PATCH` | `/{id}/complete` | — | Ke `ConsultationCompleted` (7) lewat `ConsultationFinalizationService` |
| `PATCH` | `/{id}/cancel` | `DoctorConsultation : Cancel` | **Tidak ada.** Kunjungan tetap di status sebelumnya (biasanya 6) |

## 4. Conflict yang butuh keputusan pemilik

### `CONFLICT-DP-1` — Kunjungan penunjang dan IGD ikut memblokir, tetapi tidak tampil di daftar

**Fakta:**
- Pemblokir pendaftaran berlaku untuk semua jenis kunjungan.
- Layar baru hanya menampilkan kunjungan Rawat Jalan berklinik (`RJ-DOC-DEC-012`).

**Contoh:** pasien datang ke laboratorium tanpa lewat kiosk. Kunjungannya `Outpatient`, tanpa
klinik, berstatus `Registered`, dan tidak pernah ditutup. Besoknya pasien ingin ke Poli Dalam
dan ditolak, tetapi kunjungan lab itu **tidak muncul** di Daftar Pasien Rawat Jalan, sehingga
petugas kembali buntu.

**Pilihan untuk dibahas:**
- Daftar ikut menampilkan kunjungan penunjang `Outpatient` tanpa klinik, khusus bagi pemegang
  "lihat semua".
- Pemblokir pendaftaran hanya menghitung kunjungan Rawat Jalan berklinik.
- Biarkan, dan tangani di modul penunjang.

### `CONFLICT-DP-2` — Status 6 bisa tertahan selamanya

**Fakta:**
- Membatalkan konsultasi tidak memindahkan status kunjungan.
- `RJ-DOC-DEC-016` menolak pembatalan kunjungan berstatus 6.
- `RJ-DOC-DEC-020` mengarahkan ke workspace dokter, tetapi workspace dokter tidak punya jalan keluar.

**Contoh:** dr. A memanggil pasien (status 6), membuka konsultasi, lalu pasien pergi. dr. A
membatalkan konsultasi. Kunjungan tetap status 6, pasien diblokir di setiap pendaftaran
berikutnya, dan tidak ada tombol yang bisa menutupnya.

**Pilihan untuk dibahas:**
- Layar baru boleh membatalkan kunjungan status 6 **asalkan** tidak ada konsultasi yang aktif
  (semuanya sudah batal atau belum pernah dibuat).
- Pembatalan konsultasi ikut mengembalikan kunjungan ke status sebelumnya. Ini mengubah aturan
  klinis di luar scope.

## 5. Unknown

| ID | Isi | Cara menjawab |
|---|---|---|
| `UNK-DP-1` | **Terjawab** (query read-only `QuilvianNewDevSukma`, 2 Okt 2026, izin pemilik). Kunjungan aktif 159: RJ berklinik status 3 = 92, 4 = 18, 5 = 16, 6 = 15 (7 tanpa konsultasi aktif, 8 dengan), 7 = 11, 8 = 2; Outpatient tanpa klinik status 5 = 1; `Emergency` tanpa `EmgVisit` status 5 = 4. Tanggal tertua 24 Jun 2026. ENC-RSMMC-00146: RJ berklinik, status 3, tanpa konsultasi | — |
| `UNK-DP-2` | Cara sidebar memetakan entri menu ke butir hak akses | Dibaca saat desain frontend (`left-sidebar-items-virtualized.jsx`) |

## 6. Pemicu impact scan berikutnya

- `DoctorQueueController`, `NurseStationQueueController`, `DoctorConsultationController`,
  `PatientEncounterController`, atau `MedicalRecordAccessAuditService` berubah setelah `245f0464`.
- Base component pada `components/features/base-features/` berubah setelah `b7e9b7fd4`.
