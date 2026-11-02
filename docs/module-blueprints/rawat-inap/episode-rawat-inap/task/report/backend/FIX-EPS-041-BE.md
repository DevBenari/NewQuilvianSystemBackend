# Laporan Perubahan Backend — `FIX-EPS-041-BE`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FIX-EPS-041-BE` (task payung bagian backend dan data `PLAN-REPAIR-EPS-041`; preseden `FIX-EPS-040-BE`) |
| Judul | Perbaikan temuan uji Admisi Pasien Lama, Bayi Baru Lahir, dan Transfer IGD & Poliklinik — bagian backend `FIX-EPS-041-01`, `-04`, `-09`, `-10`, `-14`, `-15`, `-17` serta data `-05`, `-11`, `-13`, `-16` |
| Slice | Admisi Rawat Inap (`FE-INP-03`) — antrean transfer, kandidat ibu bayi, kelayakan admisi, papan tempat tidur |
| Roadmap | **Tidak ada entri roadmap.** Register status dipegang [PLAN-REPAIR-EPS-041](../../../docs/plan-repair/plan-repair-041-temuan-uji-admisi-lama-bayi-transfer.md) bagian 1 |
| Trace | [ISSUE-EPS-041](../../../docs/issue/issue-041-temuan-uji-admisi-lama-bayi-transfer.md); K-01 (VIP 316/317 isolasi), K-02 (transfer batal tetap tertutup), K-03 (lanjutkan bayi ber-RM; nonaktifkan RM yatim), K-04 (`RSMMC`; NICU tidak dipindah), K-05 (batas teknis), K-06 — disetujui pemilik 26-10-2026 |
| Contract version | Perluasan tanpa mengubah kolom lama: [kontrak 006 revisi 041](../../../contracts/admission-transfer-repair-006.md), [`api-contract.md` §18.2](../../../contracts/api-contract.md), [kontrak 022 revisi 041](../../../contracts/newborn-admission-repair-022.md) |
| Dependency | Tidak ada; `FIX-EPS-041-FE` memakai hasilnya |
| Klasifikasi | `MEDIUM` — 14 berkas, satu area (`InPatientManagement`), tanpa migration, tanpa endpoint baru |
| Task mode | `BACKEND` untuk source (dikerjakan sesi 26-10-2026); sesi 27-10-2026 hanya verifikasi, laporan, dan dokumen atas instruksi pemilik |
| Target tulis | Source: `Areas/HealthServices/InPatientManagement/**` (sudah di commit). Sesi ini: dokumen `docs/module-blueprints/rawat-inap/episode-rawat-inap/**` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | Source: `1872b399` (`MHamzah`, 26-10-2026 16:28); diverifikasi pada `ee56c659` |
| Tanggal | 2026-10-27 |
| Status | Source dan data selesai; build PASS; runtime diverifikasi lewat uji browser 27-10-2026. 9 perbaikan terbukti penuh, 2 sebagian (lihat bagian 6). Pembaruan 27-10-2026: migration serah terima transfer dibuat terlihat oleh EF dan diterapkan di DB Hamzah (bagian 8) |

**Backend Governance Preflight.** Area `HealthServices`, modul `InPatientManagement`, prefix `Inp` berstatus `ACTIVE` pada
registry. Keberlakuan `TOUCHED LEGACY` (DTO, query, service existing); tidak ada entity, endpoint, permission, atau
migration baru. Aturan QBE yang berlaku: QBE-DTO-001 (kolom respons tambahan), QBE-VAL-001 (`[Range]` deposit),
QBE-API-001 (envelope `statusCode`), QBE-TXN-001 (transaksi `admit` tetap satu). `AUTOMATED TEST: NOT APPLICABLE —
backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

---

## 1. Masalah yang diperbaiki

- Antrean transfer tidak membawa kebutuhan isolasi/oksigen/monitoring, DPJP usulan IGD, diagnosis dan kelas Poli; gender,
  NIK, dan nomor HP selalu kosong. Akibatnya pasien suspek TB ditawari bangsal biasa dan DPJP terisi dokter jaga IGD.
- `admit` membungkus semua kegagalan reservasi menjadi `BED_NOT_READY`, sehingga layar tidak tahu sebab sebenarnya
  (misalnya isolasi), dan respons 201 berbadan `statusCode: 200`.
- Kandidat ibu tidak membedakan bayi yang sudah diadmisi dari bayi ber-RM tanpa episode.
- Peringatan admisi Draft lain memakai jam UTC dan bulan berbahasa Inggris ("26 October 2026 03:44").
- `RequestedDepositAmount` jalur biasa tanpa batas atas; `PlannedDepositAmount` jalur biasa tidak terisi; papan tempat tidur
  tidak membawa waktu pesan sehingga "Dipesan Pada" hilang sesudah F5.
- Data: tidak ada satu pun bed isolasi, 9 RM bayi yatim, kode RS label kosong, dua kamar ODC ber-`RoomNumber` sama.

---

## 2. Proses bisnis

1. **IGD/Poli menerbitkan permintaan rawat inap.** Antrean (`GET /admission-transfers`) kini membawa kebutuhan IGD dan isi
   surat Poli. Contoh: permintaan IGD Reza Firmansyah → `needsIsolation: true`, `proposedInpatientDoctorName: "dr. Rendy
   Pangalila"`, `decidedByDoctorName: "dr. Joni Abraham Pratama"`.
2. **Petugas memesan bed.** `available-beds` dengan `patientId` + `requiresIsolation` (sudah didukung server) hanya
   meloloskan bed isolasi VIP 316/317 untuk pasien itu.
3. **`admit` dijalankan** dalam satu transaksi. Bila reservasi menolak, kode aturannya diteruskan (`422 ISOLATION_REQUIRED`)
   dan tidak ada data yang tersisa. Sukses → HTTP 201 dengan `statusCode: 201`.
4. **Admisi bayi.** Kandidat ibu menyebut berapa bayi sudah diadmisi dan berapa masih menunggu; bayi nonaktif tidak dihitung.
5. **Pasien Lama.** Peringatan Draft lain berbunyi "…sejak 26 Oktober 2026 pukul 11.19…" (WIB). Deposit di atas
   Rp 9.999.999.999.999.999 ditolak 400 dengan kalimat Indonesia; Rp 5.000.000 tersimpan sebagai `PlannedDepositAmount`.
6. **Jalur tidak normal.** Admisi transfer yang dibatalkan tetap tidak kembali ke antrean (K-02); permintaan dengan pasien
   yang sedang dirawat tetap ditolak `PATIENT_ALREADY_ADMITTED` oleh `OpenAdmissionAsync`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

PLAN-REPAIR-EPS-041, ISSUE-EPS-041, tiga laporan uji 26-10; `InpAdmissionTransferService*.cs`,
`InpBedOccupancyService*.cs`, `InpEpisodeService*.cs` (admisi, kelayakan, kandidat bayi, penugasan dokter),
`OutpatientEncounterRules.cs`, DTO terkait, migration `20261022130000_MakeTransferHandoverPlacementsNullable`.

### 3.2 Berkas yang berubah

Semua di `Areas/HealthServices/InPatientManagement/` pada commit `1872b399`.

| Berkas | Perubahan |
| --- | --- |
| `DTOs/InpatientAdmissionTransferDtos.cs` | FIX-01: `NeedsIsolation`, `NeedsIntensiveCare`, `NeedsOxygenSupport`, `NeedsSpecialMonitoring`, `EstimatedLengthOfStayDays`, `ProposedInpatientDoctorId/Name`, `Diagnosis`, `RequestedRoomClass` |
| `Services/InpAdmissionTransferService.Queries.cs` | FIX-01: kedua proyeksi (IGD dari `EmgAdmissionRequest` berstatus `Signed`, Poli dari `CliDoctorCertificate`) mengisi kolom baru serta `Gender`/`NationalId`/`PhoneNumber` |
| `Services/InpAdmissionTransferService.cs` | FIX-04: meneruskan `bedResult.Code`; `BED_NOT_READY` hanya bila kode kosong |
| `Controllers/InpatientAdmissionTransferAdmitController.cs` | FIX-09: `statusCode` badan 201 |
| `DTOs/InpatientNewbornDtos.cs`, `Services/InpEpisodeService.NewbornCandidates.cs` | FIX-10: `AdmittedBabyCount`, `PendingBabyCount`, `RegisteredBabies[].IsPendingAdmission`; bayi nonaktif dikecualikan |
| `Services/InpEpisodeNumberService.cs`, `Services/InpEpisodeService.AdmissionEligibility.cs`, `Services/InpEpisodeService.cs` | FIX-14: `HospitalTimeZoneAsync`; peringatan Draft memakai zona RS dan bulan Indonesia |
| `DTOs/InpatientEpisodeDtos.cs` | FIX-15: `[Range]` `RequestedDepositAmount` 0–9.999.999.999.999.999 dengan pesan Indonesia |
| `Services/InpEpisodeService.cs` | FIX-17: `PlannedDepositAmount` diisi pada jalur non-transfer |
| `DTOs/InpatientBedManagementDtos.cs`, `DTOs/InpatientBedOccupancyDtos.cs`, `Services/InpBedManagementQueryService.cs`, `Services/InpBedOccupancyService.BedManagement.cs` | FIX-17: `ReservationReservedAt` pada papan dan konteks pemegang bed |

**Data `QuilvianNewDevHamzah`** (diterapkan 26-10-2026 dengan `test-with-agy/scripts/admisi-1026-datafix-041.py` atas
izin pemilik; diperiksa ulang 27-10-2026 dengan kueri baca-saja):

| Perbaikan | Perubahan data | Keadaan 27-10-2026 |
| --- | --- | --- |
| FIX-05 (K-01) | Kamar dan bed VIP 316, VIP 317 ditandai isolasi | `MstBed.IsIsolationBed` aktif: 2 (316-A, 317-A) |
| FIX-11 (K-03) | 9 RM bayi yatim dinonaktifkan (RM 42, 43, 44, 48, 50, 51, 54, 55, 56); RM 35 dan 74 sengaja dibiarkan | Kueri P-02 (bayi aktif tanpa kunjungan): 1 baris |
| FIX-13 (K-04) | `MstInpatientSetting.PatientLabelHospitalCode = 'RSMMC'`; NICU tidak dipindah unit | Nilai `RSMMC` terbaca |
| FIX-16 (P-01) | `RoomNumber` ODC 321A/321B diperbaiki | `321A`, `321B`; denah Lantai 3 menampilkan ODC 322, 321A, 321B, 320, 319 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Kolom respons tambahan (antrean transfer, kandidat ibu, papan); `errors.code` reservasi asli dan `statusCode: 201` pada `admit`. Tidak ada kolom lama yang berubah nama/nilai. Dokumen kontrak diperbarui 27-10-2026 |
| Database | Task ini tidak membuat schema/migration. Data dev Hamzah dikoreksi (tabel di atas). Temuan lingkungan: migration `20261022130000_MakeTransferHandoverPlacementsNullable` tidak pernah terlihat oleh EF sehingga belum diterapkan di DB mana pun lewat `database update`; **diperbaiki dan diterapkan di DB Hamzah 27-10-2026** (bagian 8) |
| Keamanan/Auth | `NOT APPLICABLE` — tidak ada endpoint/permission baru; kolom identitas (`NationalId`, `PhoneNumber`) memakai permission `InpatientAdmissionTransfer : Read` yang sama dengan nama/RM |

---

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Admission Transfer

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/admission-transfers` | Antrean dengan kolom kebutuhan/DPJP usulan/diagnosis/kelas | `InpatientAdmissionTransfer : Read` |
| `GET` | `/api/v1/health-services/inpatient-management/admission-transfers/{id}?sourceType=` | Detail sumber dengan kolom yang sama | `InpatientAdmissionTransfer : Read` |
| `POST` | `/api/v1/health-services/inpatient-management/admission-transfers/admit` | Admisi atomik; kode galat asli; `statusCode: 201` | `InpatientEpisode : Create` |

#### Health Services / Inpatient Management / Inpatient Episode

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/episodes/newborn-mother-candidates` | Hitungan bayi diadmisi/menunggu | `InpatientEpisode : Read` |
| `GET` | `/api/v1/health-services/inpatient-management/episodes/admission-eligibility` | Peringatan Draft lain berformat WIB | `InpatientEpisode : Read` |
| `POST` | `/api/v1/health-services/inpatient-management/episodes` | `[Range]` deposit; `PlannedDepositAmount` | `InpatientEpisode : Create` |

#### Health Services / Inpatient Management / Bed Occupancy

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/bed-occupancies/bed-board` | `ReservationReservedAt` | sesuai controller existing |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -o <folder sementara>` (27-10-2026) | 0 error, 417 warning lama (mis. CS1573, EF1002), 5 menit 43 detik | `PASS` | keluaran perintah |
| Runtime backend dari hasil build (`https://localhost:7184`) | Aktif; dihentikan sesudah uji | `PASS` | log backend |
| FIX-01 — IGD isolasi `EMG-AGY-1027-11` | `needsIsolation: true`, tanpa chip Oksigen; DPJP usulan dr. Rendy ≠ diputuskan dr. Joni | `PASS` | uji browser s1 |
| FIX-01 — Poli `SPRI/2026/10/AGY1027-14` | `diagnosis: "Demam Berdarah Dengue"`, `requestedRoomClass: "Kelas I"` | `PASS` | uji browser s2c |
| FIX-04 — `admit` isolasi ke bed non-isolasi (request dicegat di browser) | `422`, `errors.code: "ISOLATION_REQUIRED"`, tidak ada episode/kunjungan tersisa | `PASS` | uji s11 + kueri DB |
| FIX-09 — `admit` sukses | HTTP 201, badan `statusCode: 201` | `PASS` | `network.log` |
| FIX-10 — kandidat ibu | Ny. Maria Veronica: `registered 1, pending 1, admitted 0`; ibu lain konsisten | `PASS` | `GET newborn-mother-candidates` |
| FIX-14 — Draft `RI-261026111914-67CACD` (dibuat 04.19 UTC) | "…sejak 26 Oktober 2026 pukul 11.19…" | `PASS` | `GET admission-eligibility` RM 62 |
| FIX-17 — Pasien Lama deposit Rp 5.000.000 | `PlannedDepositAmount = 5000000.00`, 1 order `DPO-20261027-00001` | `PASS` | kueri DB |
| FIX-17 — `ReservationReservedAt` | "Dipesan Pada 27 Okt 2026, 10.31" sesudah F5 | `PASS` | uji s1-j |
| Log backend selama uji | 0 galat dari perubahan ini; `ERR` insert `CliTransferHandover` (`FromPlacementId` NOT NULL) setiap `admit` transfer — sebabnya diperbaiki sesudah uji (bagian 8) | `EXISTING / ENVIRONMENT ISSUE` | log backend |

Uji manual: `PASS` (lewat layar aplikasi; rincian di [laporan uji](../../../testing/live-testing/laporan-live-testing-verifikasi-perbaikan-041-admisi.md)).

**Tidak dijalankan:** FIX-04 butir 2 (bed sedang dibersihkan → `BED_NOT_READY`) — tidak ada bed berstatus pembersihan pada
jalur uji; FIX-15 butir 3 (server menolak nominal di atas batas dengan 400) — layar kini menahan nominal itu sebelum
dikirim, sehingga sisi server hanya terbukti di source (`[Range]`) dan build; pengukuran waktu respons antrean (risiko
FIX-01) tidak dilakukan.

---

## 6. Acceptance criteria dan Definition of Done

| Perbaikan | Kriteria | Status | Bukti |
| --- | --- | --- | --- |
| FIX-01 | 1 `needsIsolation: true`, `needsOxygenSupport: false` | Terpenuhi | s1 |
| | 2 DPJP usulan ≠ dokter yang memutuskan | Terpenuhi | s1, s3 |
| | 3 Diagnosis dan kelas Poli | Terpenuhi | s2c |
| | 4 Kolom lama tidak berubah | Terpenuhi | `isCito`, `dispositionReason`, `decidedByDoctor*` tetap dipakai layar |
| FIX-04 | 1 Isolasi → 422 `ISOLATION_REQUIRED` | Terpenuhi | s11 |
| | 2 Bed dibersihkan tetap `BED_NOT_READY` | **Belum diuji** (hanya source) | — |
| FIX-05 | 1 Jumlah bed isolasi > 0 | Terpenuhi | 2 bed |
| | 2 Pasien isolasi dapat dipesankan; non-isolasi ditolak di bed itu | Terpenuhi | VIP 316-A dipesan; VIP 316/317 nonaktif untuk RM 61 |
| FIX-09 (BE) | 4 HTTP 201 dengan `statusCode: 201` | Terpenuhi | `network.log` |
| FIX-10 (BE) | Hitungan diadmisi/menunggu, bayi nonaktif dikecualikan | Terpenuhi | kandidat ibu |
| FIX-11 | 1 Keputusan tertulis setiap RM P-02 | Terpenuhi | riwayat plan 26-10 (9 dinonaktifkan, RM 35/74 dibiarkan) |
| | 2 P-02 hanya bayi yang masih menunggu | Terpenuhi | 1 baris |
| FIX-13 | Label memuat kode RS yang diputuskan | **Sebagian** — pengaturan `RSMMC` tersimpan; label bayi tidak dicetak pada sesi ini; NICU tidak dipindah sesuai K-04 | kueri DB |
| FIX-14 | Format WIB dan bulan Indonesia | Terpenuhi | RM 62 |
| FIX-15 (BE) | 3 Server menolak di atas batas dengan 400 | **Belum diuji runtime** (source + build) | — |
| FIX-16 | Nama/nomor kamar ODC unik | Terpenuhi | denah Lantai 3 |
| FIX-17 (BE) | 3 `ReservedAt` sesudah F5; 4 `PlannedDepositAmount` | Terpenuhi | s1-j; DB |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 417 warning build lama, tidak ada yang berasal dari berkas task ini |
| Masalah yang diketahui | **Konsulen saat admisi ditolak aturan server** (`InpEpisodeService.Assignments`: dokter pendukung hanya untuk episode Admitted/DischargePending), sehingga `FIX-EPS-041-08` butir 1/4 tidak dapat dipenuhi tanpa keputusan pemilik (usulan K-07 di laporan FE dan laporan uji). Aturan ini tidak diubah |
| Risiko tersisa | Migration serah terima sudah diterapkan di DB Hamzah (bagian 8), tetapi belum ada `admit` transfer sesudahnya, sehingga dokumen serah terima yang benar-benar tersimpan belum terbukti. Rincian risiko migration di bagian 8.6 |
| Perubahan sampingan | Source `InPatientManagement`: `NONE` pada sesi ini. Di luar modul: tiga berkas `Migrations/` untuk perbaikan migration (bagian 8.3), atas permintaan pemilik |
| Interupsi | `NONE` |
| Status Git | Dokumen di `docs/module-blueprints/rawat-inap/episode-rawat-inap/` (laporan, plan-repair 041/006, kontrak 006/022, `api-contract.md`, `05-skema-tampilan.md`, traceability, laporan uji); `Migrations/ApplicationDbContextModelSnapshot.cs` (M) dan dua `.Designer.cs` baru (bagian 8.3). Working tree juga memuat perubahan modul Human Resource (`Areas/Corporate/HumanResource/**`, `Program.cs`, migration `20261027104500_….cs`) yang **bukan** bagian task ini; berkasnya tidak disunting, hanya dilengkapi Designer dan snapshot. Belum ada commit |
| Langkah berikutnya | Pemilik memutuskan K-07; commit kedua `.Designer.cs` dan snapshot bersama migration-nya; buktikan dokumen serah terima tersimpan pada `admit` transfer berikutnya; uji FIX-04 butir 2 dengan bed berstatus menunggu pembersihan |

---

## 8. Pembaruan 27-10-2026 — migration serah terima transfer diterapkan

### 8.1 Masalah

Pemilik menjalankan `dotnet ef database update` untuk menerapkan migration serah terima transfer, dan perintah berhenti
dengan `PendingModelChangesWarning`: *"The model for context 'ApplicationDbContext' has pending changes. Add a new
migration before updating the database."* Ratusan baris "sentinel value" yang tercetak sebelumnya hanya peringatan dan
tidak menghentikan perintah.

Dampaknya di rumah sakit: setiap kali petugas admisi menerima pasien transfer dari IGD, sistem mencoba membuat dokumen
serah terima IGD → rawat inap. Pasien IGD belum punya tempat tidur rawat inap, sehingga kolom `FromPlacementId` harus
boleh kosong. Di database kolom itu masih wajib diisi, jadi dokumennya selalu gagal dibuat. Admisinya sendiri tetap
tersimpan, sehingga kegagalan ini tidak terlihat di layar (temuan VR-02 laporan uji).

### 8.2 Sebab

EF Core hanya mengenali migration yang memiliki atribut `[Migration("…")]`. Atribut itu ada di berkas `.Designer.cs`, atau
di `Migrations/MigrationMetadata.g.cs` untuk migration yang Designer-nya diarsipkan. Dua migration ditulis tangan tanpa
keduanya:

| Migration | Akibat |
| --- | --- |
| `20261022130000_MakeTransferHandoverPlacementsNullable` (`BE-RWI-241`, commit `d79e3ed1`) | Tidak pernah terlihat oleh EF; `database update` melewatinya tanpa pesan. Snapshot model sudah diselaraskan oleh `20261023062116_AddInpEpisodeExternalReferral`, yang sengaja tidak mengulang operasinya karena menganggap migration ini sudah berjalan. Akibatnya perubahan ini tidak pernah sampai ke database mana pun lewat EF |
| `20261027104500_AddOnBehalfFieldsToAttendanceCorrectionRequest` (Human Resource, belum di-commit) | Model entity `HrdAttendanceCorrectionRequest` sudah memuat 4 kolom baru, tetapi snapshot tidak diperbarui. Selisih inilah yang menghentikan `database update` |

Bukti: `dotnet ef migrations add` percobaan pada salinan source di folder sementara menghasilkan selisih yang **hanya**
berisi 4 kolom, 2 indeks, dan 1 foreign key `HrdAttendanceCorrectionRequest`, sama persis dengan isi migration HR
tulisan tangan. Tidak ada selisih model lain.

### 8.3 Perbaikan

| Berkas | Perubahan |
| --- | --- |
| `Migrations/20261027104500_AddOnBehalfFieldsToAttendanceCorrectionRequest.Designer.cs` | Baru: atribut `[DbContext]`/`[Migration]` dan `BuildTargetModel`. Dihasilkan EF pada salinan sementara, lalu diberi ID dan nama class migration HR yang sudah ada |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | +26 baris, hanya bagian `HrdAttendanceCorrectionRequest` (kolom, indeks, relasi `InitiatedByUser`) |
| `Migrations/20261022130000_MakeTransferHandoverPlacementsNullable.Designer.cs` | Baru: hanya atribut, tanpa `BuildTargetModel`. Polanya sama dengan entri `MigrationMetadata.g.cs`; migration ini tidak bermuatan data |

Berkas `.cs` kedua migration tidak diubah. `tooling/migrations/Update-MigrationHistory.ps1` tidak dijalankan; kedua
Designer baru ikut dikompilasi seperti migration baru biasa, dan jumlah Designer di `Migrations/History/` tidak berubah.
Wewenang: perbaikan berkas migration dan eksekusi database atas permintaan pemilik 27-10-2026.

### 8.4 Eksekusi database

`dotnet ef database update` dijalankan dari salinan sementara ke `QuilvianNewDevHamzah`, supaya DLL di repo tidak
terkunci. Isi folder `Migrations/` salinan itu dicek identik dengan repo lewat hash MD5. Keluaran:

```text
Applying migration '20261022130000_MakeTransferHandoverPlacementsNullable'.
Applying migration '20261027104500_AddOnBehalfFieldsToAttendanceCorrectionRequest'.
Done.
```

### 8.5 Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` |
| `dotnet ef migrations list` sebelum update | Tepat dua `(Pending)`: `20261022130000_…` dan `20261027104500_…` | `PASS` |
| `__EFMigrationsHistory` sesudah update | Kedua ID tercatat | `PASS` |
| Kolom `CliTransferHandover` | `FromPlacementId`, `ToPlacementId` nullable | `PASS` |
| Indeks `UX_CliTransferHandover_ToPlacement` | Unik dengan `WHERE ("ToPlacementId" IS NOT NULL)` | `PASS` |
| Kolom `HrdAttendanceCorrectionRequest` | `InitiatedByUserId`, `IsOnBehalf` (NOT NULL), `OnBehalfReason`, `OnBehalfNotifiedAt` ada | `PASS` |
| `dotnet build QuilvianSystemBackend.csproj -o <folder sementara>` dari repo | 0 error, 420 warning (417 pada build pagi; selisih 3 tidak ditelusuri) | `PASS` |
| `admit` transfer menghasilkan baris `CliTransferHandover` | Belum dijalankan | `NOT RUN` |

### 8.6 Sisa risiko

- **Dokumen serah terima belum terbukti tersimpan.** Belum ada `admit` transfer sesudah migration diterapkan; dibuktikan
  pada uji transfer berikutnya (misalnya pembuktian FIX-09 butir 5) dengan kueri `CliTransferHandover`.
- **Migration HR ikut diterapkan ke DB Hamzah.** Bila pemilik HR membuat ulang migration itu, ketiga berkasnya (`.cs`,
  `.Designer.cs`, bagian snapshot) dihapus dulu sebelum `migrations add`, supaya tidak ada dua migration untuk kolom yang
  sama.
- **Belum di-commit.** Sesudah kedua Designer dan snapshot di-commit, `database update` di DB rekan lain ikut menerapkan
  perubahan serah terima. Migration ini menghapus indeks lalu membuatnya ulang, sehingga tetap aman di DB yang sudah
  menjalankan skrip manual `Migrations/sql/20261022130000_MakeTransferHandoverPlacementsNullable.sql`.
- **Pola yang perlu dihindari.** Migration baru selalu dibuat lewat `dotnet ef migrations add`. Migration tulisan tangan
  tanpa Designer tidak pernah diterapkan, dan `database update` tidak memberi tanda apa pun.
