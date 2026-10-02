# PRD TO MVP FINAL â€” RAWAT INAP V2
## Episode, Dokter Rawat Inap, Keperawatan/Pengkajian Rawat Inap, dan UI/UX Clinical Workspace

> **PRD ID:** `PRD-RWI-V2-001`<br>
> **Product:** Quilvian System V2<br>
> **Module:** Rawat Inap<br>
> **Version:** `2.0 FINAL`<br>
> **Status:** `FINAL â€” AUTHORITY FOR BLUEPRINT REALIGNMENT`<br>
> **Tanggal:** 14 September 2026<br>
> **Target Path:** `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/04-prd-to-mvp-final.md`

---

# 0. STATUS DAN OTORITAS DOKUMEN

Dokumen ini menjadi **Product Requirement + MVP Authority** untuk penyelarasan ulang modul Rawat Inap V2.

Dokumen ini mengikat:

```text
rawat-inap/
â”œâ”€â”€ episode/
â”œâ”€â”€ keperawatan/
â””â”€â”€ dokter-rawat-inap/
```

Urutan authority setelah dokumen ini disahkan:

```text
PRD TO MVP FINAL RAWAT INAP V2
        â†“
Module Blueprint
        â†“
Frontend Architecture
Backend Architecture
API Contract
Validation Matrix
Data Dictionary
        â†“
Frontend Roadmap
Backend Roadmap
        â†“
Implementation
        â†“
UAT
```

Apabila dokumen lama bertentangan dengan dokumen FINAL ini, dokumen lama harus direvisi.

---

# 1. BASELINE COMPARE

## Frontend V1

```text
Repository : DevBenari/QuilvianSystemFrontendDev
Branch     : MHamzah
```

## Frontend V2

```text
Repository : DevBenari/QuilvianSystemFrontendDev
Branch     : HamzahV2
```

## Backend V1

```text
Repository : DevBenari/QuilvianSystemBackendDev
Branch     : QuilvianSta
```

## Backend V2

```text
Repository : DevBenari/NewQuilvianSystemBackend
Branch     : MHamzah
```

Blueprint yang menjadi bahan review:

```text
NewQuilvianSystemBackend/
â””â”€â”€ docs/
    â””â”€â”€ module-blueprints/
        â””â”€â”€ rawat-inap/
            â”œâ”€â”€ episode/
            â”œâ”€â”€ keperawatan/
            â””â”€â”€ dokter-rawat-inap/
```

---

# 2. PRODUCT DECISION UTAMA

Keputusan FINAL:

> **Business capability penting yang sudah dikenal pengguna V1 dipertahankan, tetapi technical debt, struktur penyimpanan, status, permission, dan business logic V1 tidak boleh disalin mentah ke V2.**

Formula target:

```text
V1 Business Capability
        +
V2 Domain Architecture
        +
V2 Design System
        +
Perbaikan Clinical Logic
        +
Data Integrity
        +
Auditability
        =
RAWAT INAP V2 FINAL
```

---

# 3. PRODUCT VISION

Rawat Inap V2 harus:

1. familiar bagi pengguna V1;
2. mempunyai UI yang konsisten dengan Quilvian V2;
3. berpusat pada episode rawat inap;
4. menjaga konteks pasien selama dokter/perawat bekerja;
5. mengurangi perpindahan halaman;
6. membedakan data klinis, order, pelaksanaan, hasil, dan billing;
7. mempunyai lifecycle dokumen yang jelas;
8. tidak kehilangan data draft secara diam-diam;
9. tidak membuat duplicate table hanya karena ada menu;
10. dapat dikembangkan tanpa merombak fondasi utama.

---

# 4. BOUNDARY MODUL

```text
RAWAT INAP
â”‚
â”œâ”€â”€ EPISODE
â”‚   â””â”€â”€ konteks perjalanan pasien
â”‚
â”œâ”€â”€ DOKTER RAWAT INAP
â”‚   â””â”€â”€ pelayanan dan dokumentasi dokter
â”‚
â””â”€â”€ KEPERAWATAN / PENGKAJIAN RAWAT INAP
    â””â”€â”€ pengkajian dan pelayanan keperawatan
```

Episode tetap menjadi:

```text
Patient
Encounter
InpEpisode
Service Unit
Room
Bed
Admission
Doctor Assignment
Nurse/Unit Context
Episode Status
Discharge Context
```

Episode **bukan** tempat menaruh semua data klinis.

---

# 5. SHARED BUSINESS RULES

## BR-RWI-001 â€” Episode adalah clinical anchor

Semua dokumentasi Rawat Inap harus terkait:

```text
PatientId
EncounterId
InpEpisodeId
```

---

## BR-RWI-002 â€” Tidak boleh fake queue

Rawat Inap tidak boleh membuat antrean palsu untuk menggunakan flow Rawat Jalan.

---

## BR-RWI-003 â€” Patient context tidak dapat dipindahkan

Update document tidak boleh mengubah document menjadi milik pasien/episode lain.

---

## BR-RWI-004 â€” Draft boleh tidak lengkap

```text
Draft
â†’ incomplete allowed
```

---

## BR-RWI-005 â€” Complete harus tervalidasi

```text
Complete
â†’ required clinical validation
```

Validasi harus server-side.

---

## BR-RWI-006 â€” Final document immutable

Dokumen final tidak diedit langsung.

Gunakan:

```text
Addendum
Correction
Revision
```

---

## BR-RWI-007 â€” Unknown bukan negative

```text
Belum dikaji
â‰ 
Tidak ada masalah
```

Contoh:

```text
Belum dinilai nyeri
â‰ 
Tidak nyeri
```

---

## BR-RWI-008 â€” Actor berasal dari authenticated user

Frontend tidak bebas menentukan:

```text
DoctorId
NurseId
AuthorId
VerifierId
PerformedBy
```

---

## BR-RWI-009 â€” Permission bukan hanya nama role

Authority:

```text
Permission
+
Authenticated Actor
+
Clinical Relationship
+
Episode Context
```

---

## BR-RWI-010 â€” Clinical save dipisahkan dari billing failure

Target:

```text
Clinical Save
        â†“
Commit
        â†“
Billing Fact / Event
```

---

## BR-RWI-011 â€” Idempotency per command

Retry request yang sama menggunakan key yang sama.

Aksi baru menggunakan key baru.

---

## BR-RWI-012 â€” Clinical time berbeda dengan recorded time

Simpan secara jelas bila diperlukan:

```text
ClinicalAt
CreatedAt
CompletedAt
SignedAt
CorrectedAt
```

---

## BR-RWI-013 â€” Order bukan performed

```text
Order
â‰ 
Performed
â‰ 
Result
â‰ 
Billing
```

---

## BR-RWI-014 â€” SOAP bukan Visit

SOAP adalah dokumentasi perkembangan.

Visit adalah event visite dokter.

---

## BR-RWI-015 â€” Prescription header bukan resep lengkap

Minimal resep klinis harus mempunyai item.

---

## BR-RWI-016 â€” Discharge Planning bukan Discharge

Perencanaan pulang tidak menutup episode.

---

# 6. SHARED CLINICAL WORKSPACE

Seluruh workspace klinis Rawat Inap menggunakan patient context yang konsisten.

Minimal context:

```text
No. RM
Nama Pasien
Umur
Jenis Kelamin
Episode
Tanggal Masuk
Hari Rawat
Unit
Kamar
Bed
Kelas
DPJP
Alergi
Penjamin
Diagnosis / Problem Summary
Important Alert
```

Safety state:

```text
Loading
Loaded
Empty
Error
Access Denied
Read Only
Conflict
```

---

# 7. FINAL UI DECISION â€” DOKTER RAWAT INAP

## UI-DEC-DOK-001 â€” EXACT LAYOUT PARITY

**Workspace Dokter Rawat Inap V2 WAJIB menggunakan layout Dokter Rawat Jalan V2 yang sudah FIX secara sama.**

Ini **bukan**:

```text
inspirasi
mirip
satu design language saja
```

Tetapi:

```text
LAYOUT BASE = SAMA
CONTENT = BERBEDA SESUAI DOMAIN
BUSINESS PROCESS = RAWAT INAP
```

---

# 8. BAGIAN DOKTER YANG WAJIB SAMA DENGAN RAWAT JALAN

Wajib sama:

```text
Global page composition
Main workspace position
Header height
Header padding
Header alignment
Header radius
Summary metrics placement
Patient list position
Patient list width
Patient list header
Patient search placement
Patient card structure
Selected patient state
Left panel scroll
Right workspace position
Right workspace width
Patient context placement
Primary tab placement
Tab height
Tab spacing
Active tab style
Clinical content container
Content padding
Empty state
Loading state
Responsive behavior
```

Perbedaan hanya diperbolehkan pada:

```text
Text label
Patient data
Episode data
Menu content
Clinical forms
Business states
Actions
Clinical alerts
```

---

# 9. LAYOUT DOKTER RAWAT INAP FINAL

```text
â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
â”‚ GLOBAL APP HEADER                                                    â”‚
â”œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”¬â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”¤
â”‚              â”‚ DOKTER RAWAT INAP                 SUMMARY METRICS     â”‚
â”‚ GLOBAL       â”‚ Kelola pelayanan klinis pasien                       â”‚
â”‚ SIDEBAR      â”œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”¬â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”¤
â”‚              â”‚                 â”‚                                     â”‚
â”‚              â”‚ DAFTAR PASIEN   â”‚ PATIENT / EPISODE WORKSPACE         â”‚
â”‚              â”‚ RAWAT INAP      â”‚                                     â”‚
â”‚              â”‚                 â”‚ Patient Context Header              â”‚
â”‚              â”‚ Search          â”‚                                     â”‚
â”‚              â”‚                 â”‚ SOAP | CPPT | KAJIAN | ...         â”‚
â”‚              â”‚ Patient A       â”‚                                     â”‚
â”‚              â”‚ Patient B       â”‚ Clinical Content                    â”‚
â”‚              â”‚ Patient C       â”‚                                     â”‚
â”‚              â”‚                 â”‚                                     â”‚
â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”´â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”´â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
```

---

# 10. HEADER DOKTER RAWAT INAP

Gunakan layout header yang sama dengan Workspace Dokter Rawat Jalan.

Content:

```text
Dokter Rawat Inap

Kelola pelayanan dan dokumentasi klinis pasien selama episode rawat inap.
```

Metric hanya ditampilkan bila datanya nyata.

Candidate:

```text
Total Pasien
Dirawat
Discharge Pending
Perlu Review
```

Jangan membuat metric dummy hanya untuk memenuhi jumlah badge.

---

# 11. PANEL DAFTAR PASIEN DOKTER

Struktur mengikuti Rawat Jalan:

```text
Daftar Pasien Rawat Inap                    [12]
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
ðŸ” Cari nama / No. RM / kamar...
```

Patient card:

```text
Budi Santoso
RM 00-12-34-56

Mawar 302 â€¢ Bed 2
Kelas I

DPJP: dr. Ahmad Sp.PD
Hari Rawat ke-3
```

Optional badge:

```text
AKTIF
DISCHARGE PENDING
ISOLASI
```

Dilarang menampilkan internal UUID atau technical code yang tidak diperlukan user.

---

# 12. SELECTED PATIENT STATE

Selected state wajib mengikuti style Rawat Jalan.

Boleh memakai:

```text
accent border
selected background
selected indicator
```

Tidak boleh membuat style terpisah khusus Rawat Inap jika pattern Rawat Jalan sudah tersedia.

---

# 13. EMPTY STATE DOKTER

Gunakan layout empty state Rawat Jalan.

Content:

```text
Belum Ada Pasien Dipilih

Pilih pasien Rawat Inap dari panel kiri
untuk membuka workspace klinis pasien.
```

Tidak boleh memakai:

```text
Klik Konsultasi
Menunggu Dokter
Dipanggil
```

---

# 14. PATIENT CONTEXT DOKTER

Shell sama dengan patient context Rawat Jalan.

Data yang ditampilkan:

```text
Budi Santoso
RM 00-12-34-56 â€¢ Laki-laki â€¢ 57 tahun

Episode: RWI-20260914-001
Hari Rawat ke-3

Mawar 302 â€¢ Bed 2
Kelas I

DPJP
dr. Ahmad Sp.PD

Penjamin
BPJS

Alergi
Penicillin

Diagnosis
Pneumonia
```

Clinical alert contoh:

```text
âš  Alergi Penicillin
âš  Risiko Jatuh Tinggi
âš  Isolasi Contact Precaution
```

---

# 15. NAVIGASI DOKTER FINAL

Urutan final:

```text
SOAP
CPPT
KAJIAN PASIEN
RESEP
TINDAKAN
RESUME MEDIS
VISIT
PENUNJANG MEDIS
```

Style tab harus sama dengan tab Workspace Dokter Rawat Jalan.

---

# 16. CONTENT DESIGN â€” SOAP

## Tujuan

Mendokumentasikan perkembangan pasien oleh dokter.

## Sub-content

```text
Form SOAP
Riwayat SOAP
Koreksi / Addendum
```

## Layout

```text
SOAP
Catat perkembangan kondisi pasien selama episode Rawat Inap.

[ + SOAP Baru ]

Form SOAP | Riwayat SOAP | Koreksi
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

Status: Draft
Terakhir disimpan: 14:35

1. Tanda Vital
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
TD       HR       RR       Suhu       SpO2

2. Subjective
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
[ textarea ]

3. Objective
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
[ textarea ]

4. Assessment
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
[ diagnosis / problem / textarea ]

5. Plan
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
[ textarea ]

â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
[ Simpan Draft ]      [ Selesaikan SOAP ]
```

## UI Rules

- clinical form dibuat section-based;
- form tidak menjadi card dalam card berlebihan;
- action bar boleh sticky;
- Complete harus menampilkan efek yang sebenarnya;
- draft yang belum disimpan harus terdeteksi.

## Riwayat SOAP

Lebih baik timeline/card daripada table besar:

```text
14 Sep 2026 â€¢ 14:35
dr. Ahmad Sp.PD
Completed

S: Sesak berkurang...
O: ...
A: ...
P: ...

[Lihat Detail]
```

---

# 17. CONTENT DESIGN â€” CPPT

## Tujuan

Menampilkan catatan perkembangan terintegrasi antar profesi.

## Layout

```text
CPPT

[ Semua ] [ Dokter ] [ Perawat ] [ Profesi Lain ]
Tanggal: [ .... ]    Status: [ .... ]

â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

14 September 2026

10:30
PERAWAT
Siti Rahma, Ns.
Catatan ...
Status: Final

12:45
DOKTER
dr. Ahmad Sp.PD
Catatan ...
âœ“ Diverifikasi DPJP
```

## UI Rules

- gunakan vertical timeline;
- tampilkan profesi, author, clinical time, status;
- verification indicator terlihat tetapi tidak dominan;
- source document dapat dilihat;
- jangan mengelompokkan profesi berdasarkan string fallback;
- DPJP verification mengikuti authority backend.

---

# 18. CONTENT DESIGN â€” KAJIAN PASIEN

## Tujuan

Kajian medis awal dan ulang.

## Header

```text
KAJIAN PASIEN

[ Riwayat Kajian ]     [ + Kajian Baru ]
```

## Daftar Kajian

```text
Kajian Awal Medis
14 Sep 2026 â€¢ 10:30
Completed
Dokter: dr. Ahmad Sp.PD

[ Detail ]
```

```text
Kajian Ulang #1
15 Sep 2026 â€¢ 08:15
Completed

[ Detail ]
```

## Form

```text
1. Keluhan Utama
2. Riwayat Penyakit Sekarang
3. Riwayat Medis Relevan
4. Pemeriksaan Fisik
5. Tanda Vital
6. Diagnosis Kerja / Problem List
7. Rencana Terapi
```

Dokter dapat melihat reference nursing assessment dengan:

```text
source
clinical time
author
status
```

---

# 19. CONTENT DESIGN â€” RESEP

## Tujuan

Peresepan obat selama episode Rawat Inap.

## Primary Content

```text
Buat Resep
Template Resep
History Resep
Resep Harian
```

## Layout Target

```text
â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”¬â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
â”‚ DAFTAR OBAT               â”‚ DRAFT RESEP                  â”‚
â”‚                           â”‚                               â”‚
â”‚ Search obat...            â”‚ Paracetamol                  â”‚
â”‚                           â”‚ 500 mg                        â”‚
â”‚ Paracetamol          [+]  â”‚ 3 x sehari                   â”‚
â”‚ Cefixime             [+]  â”‚ 5 hari                       â”‚
â”‚ ...                       â”‚                               â”‚
â”‚                           â”‚ [hapus]                       â”‚
â”‚                           â”‚                               â”‚
â”‚                           â”‚ [ + Tambah Item ]             â”‚
â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”´â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
```

## Minimum Prescription Item

```text
Medication
Dose
Dose Unit
Frequency
Duration
Route
Signa
Instruction
Quantity
Note
```

Header tanpa item tidak dianggap resep lengkap.

---

# 20. CONTENT DESIGN â€” TINDAKAN

## Sub-content

```text
Form Tindakan
Riwayat Tindakan
```

## Form

```text
Tindakan
Jumlah
Prioritas
Clinical Reason
Instruksi
```

## History

```text
Tindakan          Waktu          Status
------------------------------------------------
Nebulisasi        14:30          Requested
EKG               15:10          Performed
```

Status klinis jangan dicampur dengan status billing.

---

# 21. CONTENT DESIGN â€” RESUME MEDIS

## Primary Content

```text
Resume Rawat Inap
Resume ODC
History Resume
```

## Layout

```text
RESUME MEDIS

Status: Draft

1. Diagnosis
2. Ringkasan Perawatan
3. Pemeriksaan Penting
4. Tindakan
5. Obat / Terapi
6. Kondisi Saat Pulang
7. Rencana Kontrol
8. Edukasi
```

Prefill boleh menggunakan data clinical source, tetapi dokter tetap review sebelum final.

Display source bila relevan.

---

# 22. CONTENT DESIGN â€” VISIT

## Header

```text
VISIT

[ Riwayat Visit ]    [ + Catat Visit ]
```

## History

```text
14 Sep 2026
08:30
dr. Ahmad Sp.PD
Morning Visit

14 Sep 2026
17:10
dr. Ahmad Sp.PD
Follow-up Visit
```

SOAP tidak otomatis menghasilkan visit.

---

# 23. CONTENT DESIGN â€” PENUNJANG MEDIS

## Menu

```text
Radiologi
Laboratorium
Gizi
Hemodialisa
Bank Darah
Rehab Medik
```

## Landing Grid

```text
â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â” â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
â”‚ Radiologi        â”‚ â”‚ Laboratorium     â”‚
â”‚ 2 order          â”‚ â”‚ 5 order          â”‚
â”‚ 1 hasil baru     â”‚ â”‚ 2 hasil baru     â”‚
â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜ â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜

â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â” â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
â”‚ Gizi             â”‚ â”‚ Rehab Medik      â”‚
â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜ â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜

â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â” â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
â”‚ Hemodialisa      â”‚ â”‚ Bank Darah       â”‚
â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜ â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
```

## Result

Hasil final harus memiliki jalur membaca actual result.

Contoh Lab:

```text
Darah Lengkap
Status: FINAL

Hb         10.2 g/dL â†“
Leukosit   12.400    â†‘

[Lihat Detail Hasil]
```

---

# 24. FINAL UI DECISION â€” KEPERAWATAN / PENGKAJIAN RAWAT INAP

Untuk **Keperawatan/Pengkajian Rawat Inap**, layout V2 yang sudah ada dinilai cukup aman dan **tidak perlu dirombak total**.

Keputusan:

> **Pertahankan shell/layout Pengkajian Rawat Inap V2 existing. Fokus perubahan pada isi menu, hierarchy content, completeness state, dan kemampuan V1 yang belum terwakili.**

Jadi:

```text
LAYOUT V2 KEPERAWATAN
=
DIPERTAHANKAN

CONTENT / MENU
=
DISELARASKAN DENGAN V1 + PERBAIKAN V2
```

---

# 25. STRUKTUR KEPERAWATAN FINAL

```text
KEPERAWATAN RAWAT INAP
â”‚
â”œâ”€â”€ PENGKAJIAN PASIEN
â”‚   â”œâ”€â”€ Kajian Umum
â”‚   â”œâ”€â”€ Resiko Jatuh
â”‚   â”œâ”€â”€ Monitoring Nyeri
â”‚   â”œâ”€â”€ Assement Edukasi
â”‚   â”œâ”€â”€ Pengawasan Harian Pasien
â”‚   â”œâ”€â”€ Evaluasi Awal
â”‚   â””â”€â”€ Perencanaan Pulang
â”‚
â”œâ”€â”€ ASUHAN KEPERAWATAN
â”‚   â”œâ”€â”€ Vital Sign
â”‚   â”œâ”€â”€ SOAP
â”‚   â”œâ”€â”€ Catatan Terintegrasi
â”‚   â”œâ”€â”€ Tindakan Harian
â”‚   â”œâ”€â”€ Obat & Alkes
â”‚   â””â”€â”€ Catatan Keperawatan
â”‚
â”œâ”€â”€ TINDAKAN
â”‚   â”œâ”€â”€ Order Tindakan
â”‚   â””â”€â”€ History Tindakan
â”‚
â”œâ”€â”€ PENUNJANG MEDIS
â”‚   â”œâ”€â”€ Radiologi
â”‚   â”œâ”€â”€ Laboratorium
â”‚   â”œâ”€â”€ Rehab Medik
â”‚   â”œâ”€â”€ Konsultasi Gizi
â”‚   â”œâ”€â”€ Hemodialisa
â”‚   â””â”€â”€ Bank Darah
â”‚
â”œâ”€â”€ PEMAKAIAN ALAT
â”‚   â”œâ”€â”€ Order Alat Kesehatan
â”‚   â””â”€â”€ History Alat Kesehatan
â”‚
â”œâ”€â”€ TRANSFER PASIEN
â”‚   â”œâ”€â”€ Form Transfer
â”‚   â””â”€â”€ History Transfer
â”‚
â”œâ”€â”€ PEMESANAN RUANGAN BEDAH
â”‚   â”œâ”€â”€ Bedah Operasi
â”‚   â””â”€â”€ Bedah Obgyn
â”‚
â””â”€â”€ TAGIHAN PASIEN
```

---

# 26. PENGKAJIAN PASIEN â€” LAYOUT

Pertahankan layout existing:

```text
Patient Information
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
Left Internal Navigation
â”‚
â””â”€â”€ Main Content
    â””â”€â”€ Secondary Tabs
        â””â”€â”€ Form / History
```

Tidak perlu diubah menjadi layout Dokter Rawat Jalan.

Yang diperbaiki:

- hierarchy content;
- status per bagian;
- progress;
- form grouping;
- history;
- error/loading;
- completeness.

---

# 27. PROGRESS PENGKAJIAN

Tambahkan progress:

```text
Pengkajian Awal
5 dari 7 bagian selesai

â–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–‘â–‘â–‘ 71%

Status: Draft
```

Status group:

```text
âœ“ Selesai
! Perlu perhatian
â—‹ Belum diisi
```

Secondary tab dapat mempunyai indicator:

```text
Kajian Umum        âœ“
Resiko Jatuh       âœ“
Monitoring Nyeri   !
Assement Edukasi   â—‹
```

---

# 28. CONTENT DESIGN â€” KAJIAN UMUM

Capability V1 yang perlu dipertahankan:

```text
Sumber Data Pasien
Pernapasan
Integritas Kulit
Nutrisi
Eliminasi
Ketergantungan
Status Fungsional
Psikososial
Alat Bantu
Catatan Relevan
```

## Layout

Gunakan accordion/section yang terstruktur:

```text
KAJIAN UMUM

1. Sumber Data Pasien
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
[fields]

2. Kondisi Umum
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
[fields]

3. Pernapasan
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
[fields]

4. Integritas Kulit
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
[fields]

5. Nutrisi
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
[fields]

6. Eliminasi
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
[fields]

7. Status Fungsional
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
[fields]
```

Jangan membuat semua field menjadi satu form datar panjang.

---

# 29. CONTENT DESIGN â€” RESIKO JATUH

Layout:

```text
RESIKO JATUH

Instrumen: [ .... ]
Versi: [ .... ]

Penilaian
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
[indikator / pertanyaan]

Skor Total
[ xx ]

Kategori
[ Rendah / Sedang / Tinggi ]

Catatan / Pencegahan
[ textarea ]
```

Clinical instrument harus disahkan clinical owner.

---

# 30. CONTENT DESIGN â€” MONITORING NYERI

Bedakan state:

```text
Belum dinilai
Tidak nyeri
Ada nyeri
Tidak dapat dinilai
```

Layout:

```text
MONITORING NYERI

Status Nyeri
( ) Tidak nyeri
( ) Ada nyeri
( ) Tidak dapat dinilai

Skala: [0â€”10]

Lokasi
[ .... ]

Karakter
[ .... ]

Faktor Pencetus
[ .... ]

Intervensi
[ .... ]

Reassessment
[ .... ]
```

Riwayat lebih baik menggunakan timeline/trend.

---

# 31. CONTENT DESIGN â€” ASSEMENT EDUKASI

Target:

```text
ASSEMENT EDUKASI

Penerima Edukasi
Pasien / Keluarga / Caregiver

Kebutuhan Edukasi
[ .... ]

Hambatan
[ .... ]

Materi
[ .... ]

Metode
[ .... ]

Evaluasi Pemahaman
[ .... ]
```

Jangan hanya satu `EducationNote`.

---

# 32. CONTENT DESIGN â€” PENGAWASAN HARIAN PASIEN

Pertahankan capability V1:

```text
Vital Sign
Pain
Intake
Output
Fluid Balance
Blood Glucose
Diet
Mobilization
Monitoring Time
```

Layout:

```text
PENGAWASAN HARIAN PASIEN

[ + Catat Pengawasan ]

Riwayat | Form

Tanda Vital
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

Nyeri
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

Intake
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
Infus
Oral
NGT
Darah
Obat

Output
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
Urin
Feses
NGT
Lain

Balance
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

Diet & Mobilisasi
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
```

Total/balance sebaiknya dihitung dari data terstruktur.

---

# 33. CONTENT DESIGN â€” EVALUASI AWAL

Catatan penting:

Pada V1, Evaluasi Awal mempunyai unsur Manajemen Pelayanan Pasien.

Karena itu jangan langsung dipetakan menjadi Nursing Initial Assessment.

Layout sementara:

```text
EVALUASI AWAL

Riwayat Evaluasi | Form Evaluasi
```

Content:

```text
Identifikasi/Skrining
Identifikasi Masalah
Harapan/Sasaran
Perencanaan Pelayanan
Dukungan
Aspek Finansial
Aspek Legal
Discharge Planning
```

Owner final harus dikunci melalui open decision.

---

# 34. CONTENT DESIGN â€” PERENCANAAN PULANG

Layout:

```text
PERENCANAAN PULANG

Kebutuhan Pulang
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

Caregiver / Pendamping
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

Kontrol / Follow-up
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

Obat & Edukasi
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

Peralatan / Home Care
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

Transportasi
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

Hambatan
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

Status Rencana
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
```

Perencanaan Pulang tidak menutup episode.

---

# 35. CONTENT DESIGN â€” ASUHAN KEPERAWATAN

Sub-content mengikuti V1:

```text
Vital Sign
SOAP
Catatan Terintegrasi
Tindakan Harian
Obat & Alkes
Catatan Keperawatan
```

Rencana Asuhan V2 existing tidak dibuang.

Rencana asuhan dapat ditempatkan pada Asuhan Keperawatan sebagai capability internal.

---

# 36. VITAL SIGN KEPERAWATAN

Gunakan:

```text
Riwayat Vital Sign
Input Vital Sign & Pain Assessment
Grafik Pemeriksaan
```

Tampilan trend harus memanfaatkan data structured.

---

# 37. SOAP KEPERAWATAN

Jika digunakan oleh policy RS:

```text
SOAP Keperawatan
Riwayat
Correction
```

Jangan mencampurkan SOAP perawat dengan SOAP dokter sebagai satu owner data tanpa source/profession.

---

# 38. CATATAN TERINTEGRASI

Gunakan projection CPPT / integrated progress note existing.

Tampilkan:

```text
Author
Profession
Clinical Time
Source
Verification
```

---

# 39. TINDAKAN HARIAN

Layout:

```text
TINDAKAN HARIAN

[ + Catat Tindakan ]

Riwayat Tindakan
â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
Waktu | Tindakan | Perawat | Hasil
```

Performed intervention berbeda dengan order.

---

# 40. OBAT & ALKES

Keperawatan tidak mengambil alih prescription ownership.

Content dapat meliputi:

```text
Order yang aktif
Pemberian sesuai authority
Pemakaian Alkes
Riwayat
```

Domain owner tetap harus jelas.

---

# 41. CATATAN KEPERAWATAN

Gunakan timeline/card:

```text
14 Sep 2026 â€¢ 14:35
Siti Rahma, Ns.

Catatan:
...

[Detail]
```

---

# 42. CONTENT DESIGN â€” TINDAKAN KEPERAWATAN

```text
TINDAKAN

Order Tindakan
History Tindakan
```

Order tidak otomatis performed.

---

# 43. CONTENT DESIGN â€” PENUNJANG MEDIS KEPERAWATAN

Gunakan content V1:

```text
Radiologi
Laboratorium
Rehab Medik
Konsultasi Gizi
Hemodialisa
Bank Darah
```

Rawat Inap hanya menjadi integration surface.

---

# 44. CONTENT DESIGN â€” PEMAKAIAN ALAT

```text
PEMAKAIAN ALAT

Order Alat Kesehatan
History Alat Kesehatan
```

Jika contract inventory/asset belum siap:

```text
Integrasi belum tersedia
```

Dilarang membuat dummy persistence.

---

# 45. CONTENT DESIGN â€” TRANSFER PASIEN

```text
TRANSFER PASIEN

Form Transfer
History Transfer
```

Form minimal:

```text
Asal Unit
Tujuan Unit
Waktu
Kondisi Pasien
Catatan Klinis
Handover
Pelaksana
Penerima
```

Transfer klinis tidak otomatis sama dengan mutasi bed.

---

# 46. CONTENT DESIGN â€” PEMESANAN RUANGAN BEDAH

```text
PEMESANAN RUANGAN BEDAH

Bedah Operasi
Bedah Obgyn
```

Status:

```text
Requested
Confirmed
Scheduled
Cancelled
```

Request bukan confirmed schedule.

---

# 47. CONTENT DESIGN â€” TAGIHAN PASIEN

Keperawatan hanya mendapatkan tampilan sesuai permission.

Default:

```text
Read Only Summary
```

Jangan membuat:

```text
edit billing
posting accounting
payment confirmation
```

di workspace perawat kecuali ada requirement terpisah.

---

# 48. VISUAL RULES KEPERAWATAN

Layout V2 dipertahankan, tetapi perbaiki:

- hierarchy section;
- secondary tab clarity;
- progress;
- status badge;
- form spacing;
- empty state;
- error state;
- history layout;
- sticky action pada form panjang bila diperlukan.

Jangan melakukan redesign total yang mengubah pola existing.

---

# 49. CLINICAL FORM VISUAL STANDARD

Gunakan:

```text
Section
Group
Field
```

Spacing:

```text
4
8
12
16
24
32
```

Rekomendasi:

```text
Card radius 8â€“12px
Input radius 6â€“8px
Button radius 6â€“8px
```

Hindari:

```text
heavy gradient
heavy shadow
nested card berlebihan
giant header
```

---

# 50. TYPOGRAPHY

Rekomendasi:

```text
Page Title      20â€“24px semibold
Section Title   16â€“18px semibold
Body            13â€“14px
Supporting      12â€“13px
```

Ikuti design token repository jika sudah tersedia.

---

# 51. TABLE VS TIMELINE

Gunakan table untuk:

```text
order
result list
procedure list
visit list singkat
```

Gunakan timeline/card untuk:

```text
SOAP
CPPT
nursing note
long clinical history
```

---

# 52. ERROR STATE

Contoh:

```text
Gagal memuat Pengkajian

Data pasien dan episode tetap tersedia.
Pengkajian belum dapat dibaca.

[Coba Lagi]
```

Jangan menghilangkan seluruh workspace hanya karena satu API gagal.

---

# 53. PARTIAL FAILURE

Contoh:

```text
Patient âœ“
Episode âœ“
Bed âœ“
Alergi âœ•
SOAP âœ“
```

Workspace tetap berjalan sesuai safety rule.

---

# 54. READ-ONLY STATE

Episode Closed:

```text
Episode Selesai

Dokumentasi klinis berada dalam mode hanya-baca.
```

Correction tetap mengikuti permission.

---

# 55. UNSAVED CHANGES

Jika ada perubahan:

```text
SOAP â€¢
```

Sticky status:

```text
Perubahan belum disimpan
```

Saat pindah:

```text
[ Simpan Draft ]
[ Tetap di Halaman ]
[ Buang Perubahan ]
```

---

# 56. MVP-0 â€” CONTRACT STABILIZATION

Wajib sebelum redesign dinyatakan selesai.

## Keperawatan

```text
assessment pagination
assessment status
fall-risk enum
fall-risk scoring
detail-before-edit
unknown vs false
permission fallback
```

## Dokter

```text
SOAP finalization semantics
CPPT DPJP verification
idempotency per command
prescription header-item workflow
unsaved draft guard
```

## Shared

```text
enum contract
episode guard
actor resolution
error state
permission contract
```

---

# 57. MVP-1 â€” SHARED WORKSPACE

Deliverable:

```text
Patient Context
Episode Context
Allergy
Bed
DPJP
Alerts
Permission
Closed State
Loading/Error
Unsaved Guard
```

---

# 58. MVP-2 â€” KEPERAWATAN

Deliverable:

```text
8 menu internal
7 content Pengkajian
Initial/Reassessment
Daily Monitoring
Care Plan
Care Plan Revision
Nursing Intervention
Education
Discharge Planning
Timeline
Correction
```

---

# 59. MVP-3 â€” DOKTER

Deliverable:

```text
Exact layout parity dengan Dokter Rawat Jalan
SOAP
CPPT
KAJIAN PASIEN
RESEP
TINDAKAN
RESUME MEDIS
VISIT
PENUNJANG MEDIS
```

---

# 60. MVP-4 â€” CROSS MODULE

Target:

```text
Gizi
Rehab Medik
Hemodialisa
Bank Darah
Transfer
Operating Room
Equipment
Billing Summary
```

---

# 61. UI ACCEPTANCE CRITERIA â€” DOKTER

## UI-AC-DOK-001
Jika content text disamarkan, layout utama Dokter Rawat Inap dan Rawat Jalan harus terlihat sama.

## UI-AC-DOK-002
Posisi dan lebar patient list sama.

## UI-AC-DOK-003
Workspace header sama secara structure, height, spacing, dan alignment.

## UI-AC-DOK-004
Summary metric placement sama.

## UI-AC-DOK-005
Search dan patient card pattern sama.

## UI-AC-DOK-006
Selected patient state sama.

## UI-AC-DOK-007
Empty state structure sama.

## UI-AC-DOK-008
Patient context berada pada posisi yang sama.

## UI-AC-DOK-009
Primary tab menggunakan implementation/style yang sama.

## UI-AC-DOK-010
Clinical content boundary/padding sama.

## UI-AC-DOK-011
Responsive behavior sama.

## UI-AC-DOK-012
Perbedaan hanya karena content/business process Rawat Inap.

---

# 62. UI ACCEPTANCE CRITERIA â€” KEPERAWATAN

## UI-AC-KEP-001
Layout existing Keperawatan V2 tetap dipertahankan.

## UI-AC-KEP-002
Menu/content mengikuti capability V1 yang disepakati.

## UI-AC-KEP-003
Secondary navigation jelas.

## UI-AC-KEP-004
Status tiap bagian dapat terlihat.

## UI-AC-KEP-005
Pengkajian mempunyai progress.

## UI-AC-KEP-006
Form mempunyai section hierarchy.

## UI-AC-KEP-007
History panjang memakai timeline/card bila lebih tepat.

## UI-AC-KEP-008
Error tidak menjadi empty state.

## UI-AC-KEP-009
Unsaved changes dilindungi.

---

# 63. BUSINESS ACCEPTANCE CRITERIA

- **AC-RWI-001** â€” Wrong patient guard.
- **AC-RWI-002** â€” Draft incomplete allowed.
- **AC-RWI-003** â€” Final completeness validation.
- **AC-RWI-004** â€” Final immutable.
- **AC-RWI-005** â€” Correction preserves original.
- **AC-RWI-006** â€” Unknown safety.
- **AC-RWI-007** â€” Read after write.
- **AC-RWI-008** â€” Partial edit safety.
- **AC-RWI-009** â€” Permission consistency.
- **AC-RWI-010** â€” SOAP complete tidak silent-finalize lifecycle lain.
- **AC-RWI-011** â€” Non-DPJP tidak otomatis verify CPPT.
- **AC-RWI-012** â€” New prescription command tidak dianggap retry.
- **AC-RWI-013** â€” Header prescription tanpa item bukan lengkap.
- **AC-RWI-014** â€” Multiple valid visit allowed.
- **AC-RWI-015** â€” Billing failure tidak menghapus clinical record.
- **AC-RWI-016** â€” Closed episode read-only.
- **AC-RWI-017** â€” Unsaved draft tidak silent loss.
- **AC-RWI-018** â€” Allergy error bukan no allergy.
- **AC-RWI-019** â€” Final result dapat dibuka.
- **AC-RWI-020** â€” No duplicate owner.

---

# 64. PROHIBITED IMPLEMENTATION

```text
DILARANG:

âŒ Fake queue untuk Rawat Inap.
âŒ Copy tabel V1 tanpa domain mapping.
âŒ Menu = tabel.
âŒ Actor bebas dari payload.
âŒ Role string sebagai authority tunggal.
âŒ Unknown menjadi negative.
âŒ Final document diedit langsung.
âŒ Diagnosis multi-value comma-separated.
âŒ Order = performed = billing.
âŒ SOAP = Visit.
âŒ Prescription header = complete prescription.
âŒ Discharge planning = discharge.
âŒ Signed resume = closed episode.
âŒ Duplicate hasil Lab/Radiologi di Rawat Inap.
âŒ Success sebelum transaksi wajib benar-benar berhasil.
âŒ Magic status number FE/BE berbeda.
âŒ Silent overwrite.
âŒ Silent discard draft.
âŒ Mock clinical data untuk terlihat selesai.
```

---

# 65. PROHIBITED UI â€” DOKTER

```text
âŒ Membuat layout Rawat Inap "mirip" tetapi berbeda struktur.
âŒ Mengubah posisi patient list.
âŒ Mengubah width panel tanpa alasan teknis.
âŒ Mengubah tab horizontal menjadi sidebar.
âŒ Membuat card style khusus Rawat Inap.
âŒ Membuat empty-state pattern baru.
âŒ Membuat breakpoint berbeda tanpa kebutuhan.
âŒ Menyalin business process antrean Rawat Jalan.
```

---

# 66. PROHIBITED UI â€” KEPERAWATAN

```text
âŒ Rombak total layout yang sudah aman.
âŒ Menghilangkan menu V1 tanpa keputusan eksplisit.
âŒ Membuat form panjang tanpa section.
âŒ Semua content dijadikan table.
âŒ Semua section dijadikan nested card.
âŒ Menampilkan success palsu untuk integrasi yang belum tersedia.
```

---

# 67. OPEN DECISIONS

## OD-RWI-001
Instrumen Fall Risk final.

## OD-RWI-002
Required field policy per jenis pengkajian.

## OD-RWI-003
CPPT verification delegation.

## OD-RWI-004
Resume ODC contract.

## OD-RWI-005
Medication reconciliation ownership.

## OD-RWI-006
Template resep ownership.

## OD-RWI-007
Definisi Resep Harian.

## OD-RWI-008
Owner Evaluasi Awal / MPP.

## OD-RWI-009
Pemakaian Alat activation.

## OD-RWI-010
Billing summary visibility.

---

# 68. DEFINITION OF DONE

Rawat Inap V2 dinyatakan selesai apabila:

```text
EPISODE
âœ“ menjadi context anchor

DOKTER
âœ“ layout sama dengan Rawat Jalan V2
âœ“ 8 content utama tersedia
âœ“ business process tetap Rawat Inap
âœ“ tidak memakai fake queue
âœ“ draft aman
âœ“ lifecycle benar

KEPERAWATAN
âœ“ layout existing dipertahankan
âœ“ content mengikuti kebutuhan V1
âœ“ pengkajian memiliki progress/completeness
âœ“ care plan dan intervention V2 tetap hidup
âœ“ tidak kehilangan capability V2

SHARED
âœ“ actor tervalidasi
âœ“ permission konsisten
âœ“ correction traceable
âœ“ integration owner jelas
âœ“ tidak ada silent data loss
âœ“ tidak ada dummy integration
```

---

# 69. REQUIRED BLUEPRINT REALIGNMENT

Setelah dokumen ini menjadi authority, update:

```text
rawat-inap/
â”œâ”€â”€ blueprint-manifest.md
â”œâ”€â”€ 04-prd-to-mvp-final.md
â”‚
â”œâ”€â”€ keperawatan/
â”‚   â”œâ”€â”€ 02-module-map.md
â”‚   â”œâ”€â”€ 03-frontend-architecture.md
â”‚   â”œâ”€â”€ 04-prd-to-mvp.md
â”‚   â”œâ”€â”€ skema-tampilan-keperawatan-rawat-inap.md
â”‚   â”œâ”€â”€ api-contract.md
â”‚   â”œâ”€â”€ validation-matrix.md
â”‚   â”œâ”€â”€ data/data-dictionary.md
â”‚   â””â”€â”€ roadmap/
â”‚       â”œâ”€â”€ backend-roadmap.md
â”‚       â””â”€â”€ frontend-roadmap.md
â”‚
â””â”€â”€ dokter-rawat-inap/
    â”œâ”€â”€ 02-module-map.md
    â”œâ”€â”€ 03-frontend-architecture.md
    â”œâ”€â”€ 04-prd-to-mvp.md
    â”œâ”€â”€ skema-tampilan-dokter-rawat-inap.md
    â”œâ”€â”€ api-contract.md
    â”œâ”€â”€ validation-matrix.md
    â”œâ”€â”€ data/data-dictionary.md
    â””â”€â”€ roadmap/
        â”œâ”€â”€ backend-roadmap.md
        â””â”€â”€ frontend-roadmap.md
```

---

# 70. FINAL PRODUCT DECISION

## Dokter Rawat Inap

> **LAYOUT HARUS SAMA dengan Dokter Rawat Jalan V2 yang sudah FIX.**

Perbedaan hanya pada:

```text
data
label domain
patient/episode context
clinical content
business state
business action
```

Menu FINAL:

```text
SOAP
CPPT
KAJIAN PASIEN
RESEP
TINDAKAN
RESUME MEDIS
VISIT
PENUNJANG MEDIS
```

---

## Keperawatan / Pengkajian Rawat Inap

> **Layout V2 existing tetap dipertahankan.**

Fokus:

```text
menambahkan capability V1
merapikan content hierarchy
menambahkan progress/completeness
memperbaiki safety/state
mempertahankan V2 care-plan/timeline/addendum
```

Menu FINAL:

```text
PENGKAJIAN PASIEN
ASUHAN KEPERAWATAN
TINDAKAN
PENUNJANG MEDIS
PEMAKAIAN ALAT
TRANSFER PASIEN
PEMESANAN RUANGAN BEDAH
TAGIHAN PASIEN
```

---

# 71. IMPLEMENTATION ORDER

```text
1. Adopt PRD FINAL
2. Fix MVP-0 contract issues
3. Realign blueprint dokter
4. Realign blueprint keperawatan
5. Implement shared guards
6. Rombak layout Dokter Rawat Inap menggunakan base Rawat Jalan
7. Lengkapi content Dokter
8. Lengkapi content Keperawatan mengikuti V1
9. Cross-module integration
10. UAT
11. Close MVP
```

---

# 72. APPROVAL RECORD

```yaml
prd_id: PRD-RWI-V2-001
version: 2.0
status: FINAL
authority: RAWAT_INAP_PRODUCT_MVP
doctor_layout_authority: DOKTER_RAWAT_JALAN_V2
nursing_layout_authority: EXISTING_RAWAT_INAP_V2
doctor_content_target:
  - SOAP
  - CPPT
  - KAJIAN_PASIEN
  - RESEP
  - TINDAKAN
  - RESUME_MEDIS
  - VISIT
  - PENUNJANG_MEDIS
nursing_content_target:
  - PENGKAJIAN_PASIEN
  - ASUHAN_KEPERAWATAN
  - TINDAKAN
  - PENUNJANG_MEDIS
  - PEMAKAIAN_ALAT
  - TRANSFER_PASIEN
  - PEMESANAN_RUANGAN_BEDAH
  - TAGIHAN_PASIEN
```

---

**END OF DOCUMENT**
