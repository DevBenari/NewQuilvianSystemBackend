# PRD → MVP MODUL RAWAT INAP

**Produk:** Quilvian Hospital Information System  
**Modul:** Rawat Inap  
**Dokumen:** Product Requirements Document — MVP  
**Status:** TARGET PROPOSAL  
**Target Backend:** NewQuilvianSystemBackend / `master`  
**Baseline Commit:** `5103e68eec5529540d369673c8a4e2651be0344b`  
**Scope MVP:** Admission → Bed → Pelayanan → Transfer → Discharge → Bed Release

---

# 1. Executive Summary

## 1.1 Tujuan Produk

Modul Rawat Inap harus menyediakan satu alur operasional yang terhubung mulai pasien dinyatakan perlu menjalani rawat inap sampai episode perawatan ditutup dan bed kembali tersedia.

MVP tidak ditujukan untuk memindahkan seluruh 28 capability legacy sekaligus.

MVP difokuskan pada **vertical slice operasional minimum yang benar-benar dapat menjalankan satu pasien secara end-to-end**:

**Admission → Penempatan Bed → Census → Perawatan → Dokumentasi Dokter/Perawat → Resep → Transfer → Resume Pulang → Closure → Bed Release.**

Peta Modul Rawat Inap sebelumnya mengidentifikasi 28 capability dan 12 Analysis Unit yang mencakup admission, bed management, census, dokumen, pengkajian, dokter, farmasi, transfer, billing dan discharge.

---

# 2. Product Problem

Saat ini backend target telah mempunyai banyak foundation generik Health Services, tetapi belum mempunyai bounded context operasional Rawat Inap yang mengelola lifecycle:

- admission rawat inap;
- reservation/occupancy bed;
- inpatient census;
- penugasan perawat;
- histori perpindahan bed;
- discharge readiness;
- pelepasan bed secara atomik;
- hubungan seluruh clinical record dengan satu episode rawat inap.

**REPOSITORY FACT — BE V2**

`EncounterType` sudah mendukung:

`Outpatient`, `Emergency`, `Inpatient`, `MedicalCheckup`, dan `Telemedicine`. 

Namun implementasi `PatientEncounterController` masih mempunyai business logic rawat jalan, termasuk default patient class `"RAWAT JALAN"` dan queue/screening oriented workflow.  

Artinya:

**enum Inpatient sudah ada, tetapi operational inpatient lifecycle belum boleh dianggap selesai.**

---

# 3. Product Vision

Membentuk Rawat Inap sebagai **episode pelayanan longitudinal**, bukan kumpulan halaman terpisah.

Satu episode harus menjadi anchor untuk:

`Pasien`
→ `Encounter`
→ `Inpatient Episode`
→ `Bed/Location`
→ `Perawat`
→ `DPJP`
→ `Assessment`
→ `CPPT`
→ `Diagnosis`
→ `Procedure`
→ `Prescription`
→ `Transfer`
→ `Discharge Summary`
→ `Closure`.

Semua aktivitas klinis dan administratif harus dapat ditelusuri kembali kepada pasien, episode, lokasi, petugas, waktu, dan statusnya.

---

# 4. MVP Boundary

## 4.1 Titik Mulai MVP

MVP dimulai ketika:

1. pasien sudah teridentifikasi di Patient Master;
2. keputusan untuk rawat inap sudah tersedia;
3. sumber pembayaran/penjamin dapat ditentukan;
4. DPJP atau dokter penanggung jawab dapat ditentukan;
5. admission officer akan mengaktifkan episode rawat inap.

Daftar tunggu dan workflow referral yang kompleks **bukan prerequisite MVP**.

---

## 4.2 Titik Akhir MVP

MVP selesai ketika:

1. DPJP menetapkan pasien dapat keluar;
2. resume pulang telah diselesaikan;
3. administrative/financial clearance telah terpenuhi atau dikonfirmasi melalui integration contract;
4. episode ditutup;
5. bed dilepas;
6. bed kembali berstatus tersedia;
7. histori episode tetap tersimpan dan tidak hilang.

---

# 5. Target Actors

| Actor | Tanggung Jawab MVP |
|---|---|
| Petugas Admisi | Membuat admission, memilih penjamin, DPJP, kelas, kamar dan bed |
| Kepala Perawat | Melihat census dan menetapkan perawat |
| Perawat | Assessment, vital sign, CPPT/catatan, tindakan dasar, transfer |
| DPJP / Dokter | Kajian klinis, diagnosis, SOAP/CPPT, tindakan, resep dan keputusan pulang |
| Farmasi | Menerima dan memproses prescription dari encounter |
| Kasir/Billing | Memberikan status financial clearance |
| Supervisor | Correction/reopen tertentu dengan alasan dan audit |
| Sistem | Menjaga encounter, occupancy bed, audit dan lifecycle state |

**NEEDS CONFIRMATION:** nama jabatan serta mapping role organisasi aktual rumah sakit tidak dikunci oleh PRD ini.

---

# 6. MVP Capability Selection

## 6.1 MUST HAVE — MVP

| Capability | CAP Legacy | Keputusan MVP |
|---|---|---|
| Pilih pasien existing | CAP-002 | MUST |
| Tentukan penjamin/metode pembayaran | CAP-003 | MUST |
| Tentukan DPJP | CAP-004 | MUST |
| Cari availability bed | CAP-005 | MUST |
| Booking/assign bed + aktivasi episode | CAP-006 | MUST |
| Census pasien aktif | CAP-008 | MUST |
| Penugasan perawat dasar | CAP-011 | MUST |
| Pengkajian awal pasien | CAP-012 | MUST |
| Catatan/tindakan keperawatan dasar | CAP-014 | MUST |
| Transfer kamar/bed | CAP-017 | MUST |
| SOAP / dokumentasi dokter | CAP-020 | MUST |
| CPPT | CAP-021 | MUST |
| Kajian dokter | CAP-022 | MUST |
| Prescription rawat inap | CAP-023 | MUST |
| Tindakan dokter | CAP-024 | MUST |
| Visit dokter | CAP-025 | MUST |
| Resume medis/pulang | CAP-026 | MUST |
| Release bed dan closure | CAP-028 | MUST |

---

# 7. Capability Ditunda Setelah MVP

| Capability | CAP | Alasan |
|---|---|---|
| Waiting list kompleks | CAP-001 | Admission langsung cukup untuk vertical slice pertama |
| Cetak kartu/gelang/label lengkap | CAP-007 | Printing dapat dibangun setelah workflow stabil |
| Handover/edukasi/privacy/IPD lengkap | CAP-009 | Patient Consent dapat direuse terlebih dahulu |
| Deposit/estimasi/benefit lengkap | CAP-010 | Menunggu operational Billing domain |
| Full SDKI Nursing Care Plan | CAP-013 | Assessment + nursing notes menjadi MVP |
| Penunjang medis end-to-end | CAP-015 | Cross-module integration phase |
| Pemakaian alat | CAP-016 | Tidak memblokir core episode |
| Booking operasi | CAP-018 | Dependency Instalasi Operasi |
| Full running bill | CAP-019 | Operational billing belum terbukti lengkap |
| Full Nutrition Care | CAP-027 | Dependency modul Gizi |

---

# 8. Target MVP Business Flow

## FLOW-RI-MVP-001 — Admission sampai Closure

```text
Keputusan Rawat Inap
        ↓
Cari / Pilih Pasien
        ↓
Tentukan Penjamin
        ↓
Tentukan DPJP
        ↓
Pilih Kelas / Ruang / Bed
        ↓
Validasi Availability
        ↓
Lock + Assign Bed
        ↓
Aktifkan Encounter Rawat Inap
        ↓
Pasien Masuk Census
        ↓
Assign Perawat
        ↓
Pengkajian Awal + Vital Sign
        ↓
Pelayanan Dokter dan Perawat
        ↓
CPPT / Diagnosis / Procedure
        ↓
Prescription bila diperlukan
        ↓
┌──────── Transfer diperlukan? ────────┐
│ YA                                   │ TIDAK
↓                                      ↓
Pilih Bed Baru                     Lanjut Perawatan
↓
Atomic Transfer
↓
Release Bed Lama
└──────────────────────┬───────────────┘
                       ↓
              Keputusan Pulang DPJP
                       ↓
                Resume Discharge
                       ↓
                Clearance Check
                       ↓
                    Close
                       ↓
                 Release Bed
```

---

# 9. Epic MVP

# EPIC RI-01 — Admission Rawat Inap

## Goal

Petugas Admisi dapat membentuk episode rawat inap dari pasien existing.

## Functional Requirements

### FR-RI-001
Sistem harus memungkinkan petugas mencari pasien berdasarkan minimal:

- nomor rekam medis;
- nama pasien;
- identitas yang tersedia pada Patient Master.

### FR-RI-002
Petugas memilih:

- pasien;
- sumber admission;
- DPJP;
- service unit;
- kelas pasien;
- penjamin/payment source.

### FR-RI-003
Sistem membuat encounter dengan tipe:

`EncounterType.Inpatient`.

### FR-RI-004
Admission rawat inap **tidak boleh menggunakan business rule kelas RAWAT JALAN** yang saat ini terdapat pada generic encounter creation.

### FR-RI-005
Satu admission menghasilkan satu active inpatient episode sebagai anchor lifecycle.

## Backend Disposition

**PatientEncounter:** EXTEND.

Generic `PatientEncounter` tetap menjadi encounter utama, tetapi create inpatient membutuhkan use case berbeda dari kiosk/rawat jalan.

---

# EPIC RI-02 — Bed Selection & Occupancy

## Goal

Menjamin satu bed tidak dapat digunakan dua pasien pada waktu bersamaan.

Backend saat ini telah memiliki Bed master, filter room/service unit/patient class, `BedStatus`, `IsReservable`, summary Available/Occupied, serta endpoint option. 

## Functional Requirements

### FR-RI-010
Petugas dapat mencari bed berdasarkan:

- service unit;
- room;
- patient class;
- availability;
- gender compatibility jika digunakan;
- isolation;
- intensive care;
- reservable.

### FR-RI-011
Bed hanya dapat dipilih apabila:

`IsActive = true`

dan memenuhi state availability yang diizinkan.

### FR-RI-012
Assignment bed harus transactional.

Jika dua user mencoba mengambil bed yang sama:

**hanya satu transaksi yang boleh berhasil.**

### FR-RI-013
Saat pasien benar-benar masuk:

`Bed → Occupied`.

### FR-RI-014
Saat episode selesai:

`Bed → Available`.

### FR-RI-015
Perubahan bed harus mempunyai histori.

---

# EPIC RI-03 — Census Rawat Inap

## Goal

Memberikan satu authoritative worklist pasien rawat inap aktif.

## Informasi Minimum

Census harus menampilkan:

| Kelompok | Informasi |
|---|---|
| Pasien | RM, nama, usia, jenis kelamin |
| Episode | nomor encounter, admission time |
| Lokasi | unit, room, bed, kelas |
| Clinical | DPJP |
| Nursing | assigned nurse |
| Payment | tipe pembayaran/penjamin |
| Status | episode status |
| LOS | lama dirawat berdasarkan admission time |

## Filter

Minimal:

- unit;
- room;
- kelas;
- DPJP;
- nurse;
- status;
- penjamin;
- pencarian pasien.

---

# EPIC RI-04 — Penugasan Perawat

## Goal

Kepala Perawat dapat menentukan siapa yang bertanggung jawab terhadap pasien.

## Functional Requirements

### FR-RI-030
Pasien aktif dapat memiliki nurse assignment.

### FR-RI-031
Assignment mempunyai:

- nurse;
- assigned by;
- assigned at;
- active from;
- active until;
- note.

### FR-RI-032
Pergantian perawat tidak menghapus histori assignment sebelumnya.

### FR-RI-033
MVP belum membutuhkan delegation engine kompleks.

---

# EPIC RI-05 — Nursing Assessment

Backend sudah mempunyai generic `PatientAssessment` yang terhubung dengan `Encounter`, `Patient`, `ServiceUnit`, dokter dan assessment status. Terdapat juga query active assessment berdasarkan encounter. 

## MVP

Reuse generic assessment sebagai foundation lalu EXTEND untuk kebutuhan rawat inap.

## Minimum Assessment

- keluhan;
- keadaan umum;
- vital sign;
- alergi;
- nyeri;
- risiko jatuh;
- status nutrisi dasar;
- kesadaran;
- ketergantungan/activity;
- catatan perawat.

Full SDKI/NANDA pathway belum menjadi blocker MVP.

---

# EPIC RI-06 — Clinical Documentation

## Repository Foundation

Backend Clinical Management telah memiliki antara lain:

- Patient Assessment;
- Patient Clinical Document;
- Patient Consent;
- Patient Diagnosis;
- Patient Integrated Progress Note;
- Patient Procedure;
- Patient Vital Sign;
- Doctor Consultation. 

CPPT juga sudah menjadi entity/controller tersendiri dan dapat difilter berdasarkan `encounterId`, patient, assessment, vital sign, doctor/service unit/provider dan profession. 

## MVP Requirements

Dokter harus dapat:

- melihat konteks episode;
- mencatat assessment;
- mencatat SOAP;
- menambahkan diagnosis;
- menambahkan tindakan;
- memasukkan CPPT;
- membuat prescription;
- mencatat visit;
- membuat keputusan discharge.

Perawat harus dapat:

- mencatat vital sign;
- menambahkan CPPT;
- mencatat perkembangan/tindakan keperawatan dasar.

Semua record wajib terkait dengan:

`PatientId + EncounterId + Actor + Timestamp`.

---

# EPIC RI-07 — Medication Handoff

Prescription backend sudah mempunyai encounter-based filtering serta status dokter, pembayaran, fulfillment dan approval. 

Repository Pharmacy Management juga telah mempunyai controller untuk prescription, item, compound, preparation dan review. 

## MVP

Rawat Inap tidak membuat pharmacy engine baru.

Rawat Inap harus:

1. membuat prescription dari konteks episode;
2. mengirimkan encounter dan patient context;
3. menampilkan status prescription;
4. mengarahkan proses selanjutnya ke Pharmacy Management.

## Out of Scope MVP

Medication Administration Record lengkap dan bedside medication administration belum menjadi bagian MVP kecuali kemudian ditetapkan sebagai mandatory go-live requirement.

---

# EPIC RI-08 — Consent

Backend V2 telah mempunyai `PatientConsent` yang mendukung hubungan dengan patient, encounter, assessment, consultation, procedure dan clinical document serta status agreement, verification dan approval. 

## MVP

Reuse Patient Consent untuk:

- persetujuan pasien/wali;
- consent tindakan bila diperlukan;
- signer;
- status consent;
- verification;
- audit.

General Consent/Handover/Education/Privacy legacy tidak otomatis dianggap sama dengan Patient Consent.

Mapping dokumen tersebut masuk phase berikutnya.

---

# EPIC RI-09 — Transfer Pasien

## Goal

Memindahkan pasien tanpa menyebabkan double occupancy atau kehilangan histori lokasi.

## Atomic Transfer

Satu transaction logical harus melakukan:

```text
Validate Episode
      ↓
Validate Target Bed
      ↓
Lock Target Bed
      ↓
Create Transfer Record
      ↓
Close Current Bed Assignment
      ↓
Occupy Target Bed
      ↓
Release Previous Bed
      ↓
Update Episode Current Location
      ↓
Commit
```

Jika satu langkah gagal:

**seluruh transfer harus rollback.**

## Data Transfer

Minimal:

- episode;
- bed lama;
- bed baru;
- unit/room lama;
- unit/room baru;
- alasan;
- requested by;
- transferred by;
- timestamp.

---

# EPIC RI-10 — Discharge & Closure

## Goal

Menutup episode tanpa meninggalkan bed occupied atau record klinis tidak lengkap.

## Minimum Gates

Episode hanya dapat ditutup apabila:

### Clinical Gate
- keputusan discharge tersedia;
- DPJP tersedia;
- resume pulang tersedia.

### Administrative Gate
- status administrative clearance terpenuhi.

### Financial Gate
- status financial clearance tersedia dari Billing/Kasir.

### Bed Gate
- active bed assignment ditemukan.

## Atomic Closure

```text
Validate Discharge
        ↓
Validate Resume
        ↓
Validate Clearance
        ↓
Close Active Bed Assignment
        ↓
Release Bed
        ↓
Close Inpatient Episode
        ↓
Complete Encounter
        ↓
Commit
```

Jika gagal:

bed dan encounter tidak boleh berubah setengah jalan.

---

# 10. Proposed State Model

## 10.1 Inpatient Episode

**AGENT TARGET PROPOSAL**

```text
Draft
  ↓
Admitted
  ↓
InCare
  ↓
DischargePending
  ↓
Closed
```

Alternate:

```text
Draft/Admitted → Cancelled
```

Transfer **tidak** membuat episode baru.

Transfer hanya membuat location/bed assignment baru di dalam episode yang sama.

---

# 11. Proposed Bed Assignment State

```text
Reserved
   ↓
Occupied
   ↓
Released
```

Alternate:

```text
Reserved → Cancelled
```

## Invariant Utama

Pada waktu yang sama:

**1 Bed maksimal mempunyai 1 active Occupied assignment.**

Dan:

**1 inpatient episode maksimal mempunyai 1 active primary bed.**

---

# 12. Backend Target Architecture

## AGENT TARGET PROPOSAL

Buat bounded context operasional khusus:

`HealthServices / InpatientManagement`

dan jangan memasukkan logic admission rawat inap secara berlebihan ke PatientEncounterController rawat jalan.

## Reuse

| Existing Backend | Disposition |
|---|---|
| MstPatient | REUSE |
| TrxPatientEncounter | EXTEND / ANCHOR |
| MstBed | REUSE |
| MstRoom | REUSE |
| PatientAssessment | EXTEND |
| PatientVitalSign | REUSE |
| PatientDiagnosis | REUSE |
| PatientProcedure | REUSE |
| PatientIntegratedProgressNote | REUSE / EXTEND |
| PatientConsent | REUSE / EXTEND |
| Prescription | REUSE / EXTEND |
| DoctorConsultation | EXTEND |

## New Logical Objects

Nama berikut merupakan **logical entity proposal**, bukan final physical table naming:

| Logical Entity | Responsibility |
|---|---|
| InpatientEpisode | State utama episode rawat inap |
| InpatientBedAssignment | Reservation/occupancy/history bed |
| InpatientNurseAssignment | Penanggung jawab perawat |
| InpatientTransfer | Histori perpindahan lokasi |
| InpatientDischarge | Keputusan dan metadata discharge |
| InpatientClearance | Ringkasan gate administratif/financial |

Final nama entity/table/prefix mengikuti governance backend dan harus dikunci sebelum implementasi.

---

# 13. API Capability Target

MVP membutuhkan contract kategori berikut.

## Admission

```text
POST   /inpatient/episodes
GET    /inpatient/episodes
GET    /inpatient/episodes/{id}
```

## Bed

```text
GET    /inpatient/beds/available
POST   /inpatient/episodes/{id}/bed-assignments
```

## Census

```text
GET    /inpatient/census
```

## Nurse Assignment

```text
POST   /inpatient/episodes/{id}/nurse-assignments
GET    /inpatient/episodes/{id}/nurse-assignments
```

## Transfer

```text
POST   /inpatient/episodes/{id}/transfers
GET    /inpatient/episodes/{id}/transfers
```

## Discharge

```text
POST   /inpatient/episodes/{id}/discharge
POST   /inpatient/episodes/{id}/close
```

**Catatan:** route merupakan target product contract dan belum merupakan repository fact.

---

# 14. Authorization Matrix

| Capability | Admisi | Kepala Perawat | Perawat | Dokter/DPJP | Billing | Supervisor |
|---|---:|---:|---:|---:|---:|---:|
| Create admission | ✓ |  |  |  |  | ✓ |
| Assign bed | ✓ |  |  |  |  | ✓ |
| Census | ✓ | ✓ | ✓ | ✓ |  | ✓ |
| Nurse assignment |  | ✓ |  |  |  | ✓ |
| Nursing assessment |  | ✓ | ✓ |  |  |  |
| CPPT perawat |  | ✓ | ✓ |  |  |  |
| CPPT dokter |  |  |  | ✓ |  |  |
| Diagnosis/procedure |  |  |  | ✓ |  |  |
| Prescription |  |  |  | ✓ |  |  |
| Transfer |  | ✓ | ✓ | sesuai SOP |  | ✓ |
| Discharge decision |  |  |  | ✓ |  |  |
| Financial clearance |  |  |  |  | ✓ | ✓ |
| Close episode | sesuai SOP |  |  | sesuai SOP |  | ✓ |

**NEEDS CONFIRMATION:** ownership final transition discharge/closure dengan operasional rumah sakit.

---

# 15. Billing Boundary

**REPOSITORY FACT — BE V2**

Pada baseline yang diperiksa, `BillingManagement` baru mempunyai subdomain `MasterData`; operational billing flow belum terlihat sebagai domain transaksi setara kebutuhan full inpatient billing. 

Karena itu MVP Rawat Inap **tidak boleh membuat financial ledger sendiri**.

## MVP Contract

Rawat Inap hanya membutuhkan interface:

```text
FinancialClearanceStatus:
- Pending
- Cleared
- Blocked
```

Operational deposit, invoice, payment allocation, refund, excess dan insurance settlement tetap milik domain Billing/Kasir.

---

# 16. Regulatory & Interoperability Guardrails

**REGULATORY FACT**

Permenkes No. 24 Tahun 2022 tentang Rekam Medis berstatus berlaku dan mewajibkan fasilitas pelayanan kesehatan termasuk rumah sakit menyelenggarakan Rekam Medis Elektronik serta menjaga keamanan, kerahasiaan, keutuhan dan ketersediaan data.

Implikasi MVP:

- clinical record tidak boleh hard-delete sembarangan;
- actor dan waktu perubahan harus dapat diaudit;
- hak akses harus dikontrol;
- koreksi record harus tetap mempertahankan traceability.

**OFFICIAL INTEROPERABILITY FACT**

Playbook SATUSEHAT Rawat Inap mendefinisikan satu rangkaian rawat inap sebagai `Encounter`, termasuk timeline lokasi, diagnosis, observation, procedure dan discharge-related data. Dokumentasi juga menunjukkan perubahan lokasi/bed perlu direpresentasikan sebagai histori location dalam encounter.

Implikasi desain:

`PatientEncounter` tetap tepat dijadikan anchor lintas layanan, sedangkan InpatientEpisode menangani state operasional internal yang tidak cocok dipaksakan ke workflow antrean rawat jalan.

---

# 17. Non-Functional Requirements

## NFR-001 — Atomicity

Admission, transfer dan discharge yang mengubah occupancy bed wajib transactional.

## NFR-002 — Concurrency

Sistem wajib mencegah double assignment terhadap bed yang sama.

## NFR-003 — Auditability

Setiap perubahan penting mencatat:

- actor;
- date/time;
- action;
- previous state;
- new state;
- reason jika diperlukan.

## NFR-004 — Authorization

Seluruh endpoint menggunakan permission berbasis capability/action.

## NFR-005 — Soft Delete / Correction

Record klinis/finalized transaction tidak dihapus fisik melalui flow operasional biasa.

## NFR-006 — Traceability

Semua clinical transaction harus dapat ditelusuri ke:

`Patient → Encounter → Inpatient Episode`.

## NFR-007 — Time

Timestamp harus konsisten dengan model backend dan siap dipetakan ke UTC untuk interoperabilitas.

---

# 18. MVP UAT Scenarios

## UAT-01 — Admission Normal

**Given:** pasien existing, DPJP tersedia, bed available.  
**When:** admisi membuat admission.  
**Then:** inpatient encounter + episode terbentuk dan bed menjadi occupied.

## UAT-02 — Double Bed Prevention

**Given:** dua petugas memilih bed yang sama.  
**When:** submit hampir bersamaan.  
**Then:** hanya satu assignment berhasil.

## UAT-03 — Census

Setelah admission selesai, pasien langsung muncul dalam census unit yang tepat.

## UAT-04 — Nursing Assessment

Perawat assigned dapat menyimpan assessment dan vital sign pada episode yang benar.

## UAT-05 — Clinical Documentation

Dokter dapat membuat clinical documentation, diagnosis, CPPT dan tindakan yang terhubung ke encounter.

## UAT-06 — Prescription

Dokter membuat prescription dan Farmasi dapat menemukannya menggunakan encounter yang sama.

## UAT-07 — Transfer

Pasien berpindah dari Bed A ke Bed B.

Setelah berhasil:

- Bed A = Available;
- Bed B = Occupied;
- histori Bed A tetap ada;
- episode tetap sama.

## UAT-08 — Transfer Collision

Transfer ke bed yang sudah diambil transaksi lain harus gagal tanpa melepas bed lama.

## UAT-09 — Discharge Blocked

Episode tidak dapat ditutup apabila mandatory discharge gate belum terpenuhi.

## UAT-10 — Discharge Successful

Jika semua gate terpenuhi:

- episode Closed;
- encounter Completed;
- bed Available;
- history tetap tersedia.

## UAT-11 — Unauthorized Access

User tanpa permission tidak dapat melakukan clinical/administrative action terkait.

---

# 19. MVP Definition of Done

MVP dianggap selesai hanya jika satu pasien dapat menjalani flow berikut tanpa manipulasi database manual:

```text
Pasien
→ Admission
→ Bed Assignment
→ Active Census
→ Nurse Assignment
→ Nursing Assessment
→ Doctor Documentation
→ CPPT
→ Diagnosis/Procedure
→ Prescription
→ Transfer Bed
→ Discharge Decision
→ Resume
→ Clearance
→ Episode Closure
→ Bed Release
```

Dan lima invariant berikut harus lulus:

| ID | Invariant |
|---|---|
| INV-01 | Tidak terjadi double occupied bed |
| INV-02 | Satu episode hanya memiliki satu active primary bed |
| INV-03 | Semua clinical record terhubung ke patient + encounter |
| INV-04 | Closure tidak dapat melewati mandatory gate |
| INV-05 | Release bed dan close episode tidak meninggalkan partial state |

---

# 20. Delivery Priority

## MVP-0 — Foundation

Lock:

`Encounter ↔ InpatientEpisode ↔ BedAssignment`

beserta state dan concurrency rule.

## MVP-1 — Admission & Bed

Implement:

Admission → bed selection → occupancy → census.

## MVP-2 — Nursing

Implement:

Nurse assignment → assessment → vital → nursing note/CPPT.

## MVP-3 — Doctor & Medication

Integrasikan:

consultation → diagnosis → procedure → CPPT → prescription.

## MVP-4 — Transfer

Implement atomic bed transfer dan location history.

## MVP-5 — Discharge

Implement:

discharge decision → resume → clearance → closure → bed release.

## POST-MVP

Waiting list, full consent document pack, printing, SDKI care planning penuh, penunjang, operasi, alat, gizi dan financial workflow lengkap.

---

# 21. Key Product Decisions

## DEC-RI-001
**Rawat Inap memakai `PatientEncounter` sebagai encounter anchor.**

Tetapi inpatient lifecycle tidak menggunakan mentah workflow PatientEncounter rawat jalan.

## DEC-RI-002
**Inpatient operational state dipisahkan melalui InpatientEpisode.**

Tujuannya menghindari merusak queue/state machine rawat jalan.

## DEC-RI-003
**Bed Master direuse.**

Tidak membuat master bed kedua.

Yang dibuat adalah transactional assignment/occupancy layer.

## DEC-RI-004
**Clinical foundation direuse sebanyak mungkin.**

Assessment, vital sign, diagnosis, procedure, CPPT, consent dan prescription tidak dibuat ulang hanya karena pasien merupakan pasien rawat inap.

## DEC-RI-005
**Transfer adalah location transition dalam episode yang sama.**

Tidak membuat encounter baru.

## DEC-RI-006
**Billing tetap bounded context lain.**

Rawat Inap hanya mengonsumsi clearance/status yang diperlukan.

## DEC-RI-007
**Closure dan bed release wajib atomic.**

Tidak boleh ada episode Closed dengan bed tetap occupied karena transaction gagal di tengah.

---

# 22. Open Questions Before Development Lock

| ID | Question | Status |
|---|---|---|
| OQ-RI-001 | Apakah admission wajib selalu berasal dari referral/encounter sebelumnya? | NEEDS CONFIRMATION |
| OQ-RI-002 | Apakah bed perlu state `Reserved` sebelum `Occupied`? | NEEDS CONFIRMATION |
| OQ-RI-003 | Berapa lama reservation boleh aktif? | NEEDS CONFIRMATION |
| OQ-RI-004 | Siapa yang mempunyai authority final transfer? | NEEDS CONFIRMATION |
| OQ-RI-005 | Apakah unit tujuan wajib accept transfer sebelum perpindahan? | NEEDS CONFIRMATION |
| OQ-RI-006 | Siapa yang secara sistem mengeksekusi final closure: perawat, admisi atau billing? | NEEDS CONFIRMATION |
| OQ-RI-007 | Apa exact mandatory discharge checklist rumah sakit? | NEEDS CONFIRMATION |
| OQ-RI-008 | Apakah financial clearance blocking atau hanya warning? | NEEDS CONFIRMATION |
| OQ-RI-009 | General Consent apa saja yang mandatory saat admission? | NEEDS CONFIRMATION |
| OQ-RI-010 | Apakah bayi baru lahir/ICU/isolasi masuk MVP pertama atau flow khusus? | NEEDS CONFIRMATION |
| OQ-RI-011 | Apakah full Nursing Care Plan SDKI mandatory pada MVP pertama? | NEEDS CONFIRMATION |
| OQ-RI-012 | Siapa yang berhak reopen episode setelah Closed dan dalam kondisi apa? | NEEDS CONFIRMATION |

---

# 23. Final MVP Scope

## MVP Rawat Inap

**Admission + Bed + Census + Nursing + Doctor + Prescription + Transfer + Discharge**

Bukan:

**seluruh fitur Rawat Inap legacy dipindahkan sekaligus.**

Dengan strategi ini MVP membangun **tulang punggung episode rawat inap terlebih dahulu**, sementara capability lintas modul dan administrasi lanjutan ditambahkan di atas anchor yang sama pada fase berikutnya.

---

# 24. Recommended Next Step

Sebelum source code dibuat, lakukan **Product/Business Grill-Me untuk 12 Open Questions** dan kemudian lock tiga kontrak paling kritis:

**Contract A — Inpatient Episode State Machine**

```text
Admission
→ InCare
→ DischargePending
→ Closed
```

**Contract B — Bed State & Concurrency**

```text
Available
→ Reserved/Occupied
→ Released
→ Available
```

**Contract C — Atomic Transfer & Closure**

```text
Encounter
+ Episode
+ Bed Assignment
+ Transfer
+ Discharge
```

Setelah tiga kontrak tersebut dikunci, backend MVP dapat dipecah menjadi implementation task tanpa risiko besar mengulang struktur database atau state machine di tengah development.