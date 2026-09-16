# PRD TO MVP FINAL — RAWAT INAP V2
## Episode, Dokter Rawat Inap, Keperawatan/Pengkajian Rawat Inap, dan UI/UX Clinical Workspace

> **PRD ID:** `PRD-RWI-V2-001`  
> **Product:** Quilvian System V2  
> **Module:** Rawat Inap  
> **Version:** `2.0 FINAL`  
> **Status:** `FINAL — AUTHORITY FOR BLUEPRINT REALIGNMENT`  
> **Tanggal:** 14 September 2026  
> **Target Path:** `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/04-prd-to-mvp-final.md`

---

# 0. STATUS DAN OTORITAS DOKUMEN

Dokumen ini menjadi **Product Requirement + MVP Authority** untuk penyelarasan ulang modul Rawat Inap V2.

Dokumen ini mengikat:

```text
rawat-inap/
├── episode/
├── keperawatan/
└── dokter-rawat-inap/
```

Urutan authority setelah dokumen ini disahkan:

```text
PRD TO MVP FINAL RAWAT INAP V2
        ↓
Module Blueprint
        ↓
Frontend Architecture
Backend Architecture
API Contract
Validation Matrix
Data Dictionary
        ↓
Frontend Roadmap
Backend Roadmap
        ↓
Implementation
        ↓
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
└── docs/
    └── module-blueprints/
        └── rawat-inap/
            ├── episode/
            ├── keperawatan/
            └── dokter-rawat-inap/
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
│
├── EPISODE
│   └── konteks perjalanan pasien
│
├── DOKTER RAWAT INAP
│   └── pelayanan dan dokumentasi dokter
│
└── KEPERAWATAN / PENGKAJIAN RAWAT INAP
    └── pengkajian dan pelayanan keperawatan
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

## BR-RWI-001 — Episode adalah clinical anchor

Semua dokumentasi Rawat Inap harus terkait:

```text
PatientId
EncounterId
InpEpisodeId
```

---

## BR-RWI-002 — Tidak boleh fake queue

Rawat Inap tidak boleh membuat antrean palsu untuk menggunakan flow Rawat Jalan.

---

## BR-RWI-003 — Patient context tidak dapat dipindahkan

Update document tidak boleh mengubah document menjadi milik pasien/episode lain.

---

## BR-RWI-004 — Draft boleh tidak lengkap

```text
Draft
→ incomplete allowed
```

---

## BR-RWI-005 — Complete harus tervalidasi

```text
Complete
→ required clinical validation
```

Validasi harus server-side.

---

## BR-RWI-006 — Final document immutable

Dokumen final tidak diedit langsung.

Gunakan:

```text
Addendum
Correction
Revision
```

---

## BR-RWI-007 — Unknown bukan negative

```text
Belum dikaji
≠
Tidak ada masalah
```

Contoh:

```text
Belum dinilai nyeri
≠
Tidak nyeri
```

---

## BR-RWI-008 — Actor berasal dari authenticated user

Frontend tidak bebas menentukan:

```text
DoctorId
NurseId
AuthorId
VerifierId
PerformedBy
```

---

## BR-RWI-009 — Permission bukan hanya nama role

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

## BR-RWI-010 — Clinical save dipisahkan dari billing failure

Target:

```text
Clinical Save
        ↓
Commit
        ↓
Billing Fact / Event
```

---

## BR-RWI-011 — Idempotency per command

Retry request yang sama menggunakan key yang sama.

Aksi baru menggunakan key baru.

---

## BR-RWI-012 — Clinical time berbeda dengan recorded time

Simpan secara jelas bila diperlukan:

```text
ClinicalAt
CreatedAt
CompletedAt
SignedAt
CorrectedAt
```

---

## BR-RWI-013 — Order bukan performed

```text
Order
≠
Performed
≠
Result
≠
Billing
```

---

## BR-RWI-014 — SOAP bukan Visit

SOAP adalah dokumentasi perkembangan.

Visit adalah event visite dokter.

---

## BR-RWI-015 — Prescription header bukan resep lengkap

Minimal resep klinis harus mempunyai item.

---

## BR-RWI-016 — Discharge Planning bukan Discharge

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

# 7. FINAL UI DECISION — DOKTER RAWAT INAP

## UI-DEC-DOK-001 — EXACT LAYOUT PARITY

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
┌──────────────────────────────────────────────────────────────────────┐
│ GLOBAL APP HEADER                                                    │
├──────────────┬───────────────────────────────────────────────────────┤
│              │ DOKTER RAWAT INAP                 SUMMARY METRICS     │
│ GLOBAL       │ Kelola pelayanan klinis pasien                       │
│ SIDEBAR      ├─────────────────┬─────────────────────────────────────┤
│              │                 │                                     │
│              │ DAFTAR PASIEN   │ PATIENT / EPISODE WORKSPACE         │
│              │ RAWAT INAP      │                                     │
│              │                 │ Patient Context Header              │
│              │ Search          │                                     │
│              │                 │ SOAP | CPPT | KAJIAN | ...         │
│              │ Patient A       │                                     │
│              │ Patient B       │ Clinical Content                    │
│              │ Patient C       │                                     │
│              │                 │                                     │
└──────────────┴─────────────────┴─────────────────────────────────────┘
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
────────────────────────────────────────────────
🔍 Cari nama / No. RM / kamar...
```

Patient card:

```text
Budi Santoso
RM 00-12-34-56

Mawar 302 • Bed 2
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
RM 00-12-34-56 • Laki-laki • 57 tahun

Episode: RWI-20260914-001
Hari Rawat ke-3

Mawar 302 • Bed 2
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
⚠ Alergi Penicillin
⚠ Risiko Jatuh Tinggi
⚠ Isolasi Contact Precaution
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

# 16. CONTENT DESIGN — SOAP

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
────────────────────────────────────────────────────

Status: Draft
Terakhir disimpan: 14:35

1. Tanda Vital
────────────────────────────────────────────
TD       HR       RR       Suhu       SpO2

2. Subjective
────────────────────────────────────────────
[ textarea ]

3. Objective
────────────────────────────────────────────
[ textarea ]

4. Assessment
────────────────────────────────────────────
[ diagnosis / problem / textarea ]

5. Plan
────────────────────────────────────────────
[ textarea ]

────────────────────────────────────────────
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
14 Sep 2026 • 14:35
dr. Ahmad Sp.PD
Completed

S: Sesak berkurang...
O: ...
A: ...
P: ...

[Lihat Detail]
```

---

# 17. CONTENT DESIGN — CPPT

## Tujuan

Menampilkan catatan perkembangan terintegrasi antar profesi.

## Layout

```text
CPPT

[ Semua ] [ Dokter ] [ Perawat ] [ Profesi Lain ]
Tanggal: [ .... ]    Status: [ .... ]

────────────────────────────────────────────

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
✓ Diverifikasi DPJP
```

## UI Rules

- gunakan vertical timeline;
- tampilkan profesi, author, clinical time, status;
- verification indicator terlihat tetapi tidak dominan;
- source document dapat dilihat;
- jangan mengelompokkan profesi berdasarkan string fallback;
- DPJP verification mengikuti authority backend.

---

# 18. CONTENT DESIGN — KAJIAN PASIEN

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
14 Sep 2026 • 10:30
Completed
Dokter: dr. Ahmad Sp.PD

[ Detail ]
```

```text
Kajian Ulang #1
15 Sep 2026 • 08:15
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

# 19. CONTENT DESIGN — RESEP

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
┌───────────────────────────┬───────────────────────────────┐
│ DAFTAR OBAT               │ DRAFT RESEP                  │
│                           │                               │
│ Search obat...            │ Paracetamol                  │
│                           │ 500 mg                        │
│ Paracetamol          [+]  │ 3 x sehari                   │
│ Cefixime             [+]  │ 5 hari                       │
│ ...                       │                               │
│                           │ [hapus]                       │
│                           │                               │
│                           │ [ + Tambah Item ]             │
└───────────────────────────┴───────────────────────────────┘
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

# 20. CONTENT DESIGN — TINDAKAN

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

# 21. CONTENT DESIGN — RESUME MEDIS

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

# 22. CONTENT DESIGN — VISIT

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

# 23. CONTENT DESIGN — PENUNJANG MEDIS

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
┌──────────────────┐ ┌──────────────────┐
│ Radiologi        │ │ Laboratorium     │
│ 2 order          │ │ 5 order          │
│ 1 hasil baru     │ │ 2 hasil baru     │
└──────────────────┘ └──────────────────┘

┌──────────────────┐ ┌──────────────────┐
│ Gizi             │ │ Rehab Medik      │
└──────────────────┘ └──────────────────┘

┌──────────────────┐ ┌──────────────────┐
│ Hemodialisa      │ │ Bank Darah       │
└──────────────────┘ └──────────────────┘
```

## Result

Hasil final harus memiliki jalur membaca actual result.

Contoh Lab:

```text
Darah Lengkap
Status: FINAL

Hb         10.2 g/dL ↓
Leukosit   12.400    ↑

[Lihat Detail Hasil]
```

---

# 24. FINAL UI DECISION — KEPERAWATAN / PENGKAJIAN RAWAT INAP

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
│
├── PENGKAJIAN PASIEN
│   ├── Kajian Umum
│   ├── Resiko Jatuh
│   ├── Monitoring Nyeri
│   ├── Assement Edukasi
│   ├── Pengawasan Harian Pasien
│   ├── Evaluasi Awal
│   └── Perencanaan Pulang
│
├── ASUHAN KEPERAWATAN
│   ├── Vital Sign
│   ├── SOAP
│   ├── Catatan Terintegrasi
│   ├── Tindakan Harian
│   ├── Obat & Alkes
│   └── Catatan Keperawatan
│
├── TINDAKAN
│   ├── Order Tindakan
│   └── History Tindakan
│
├── PENUNJANG MEDIS
│   ├── Radiologi
│   ├── Laboratorium
│   ├── Rehab Medik
│   ├── Konsultasi Gizi
│   ├── Hemodialisa
│   └── Bank Darah
│
├── PEMAKAIAN ALAT
│   ├── Order Alat Kesehatan
│   └── History Alat Kesehatan
│
├── TRANSFER PASIEN
│   ├── Form Transfer
│   └── History Transfer
│
├── PEMESANAN RUANGAN BEDAH
│   ├── Bedah Operasi
│   └── Bedah Obgyn
│
└── TAGIHAN PASIEN
```

---

# 26. PENGKAJIAN PASIEN — LAYOUT

Pertahankan layout existing:

```text
Patient Information
────────────────────────────────────────────
Left Internal Navigation
│
└── Main Content
    └── Secondary Tabs
        └── Form / History
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

████████████░░░ 71%

Status: Draft
```

Status group:

```text
✓ Selesai
! Perlu perhatian
○ Belum diisi
```

Secondary tab dapat mempunyai indicator:

```text
Kajian Umum        ✓
Resiko Jatuh       ✓
Monitoring Nyeri   !
Assement Edukasi   ○
```

---

# 28. CONTENT DESIGN — KAJIAN UMUM

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
────────────────────────────
[fields]

2. Kondisi Umum
────────────────────────────
[fields]

3. Pernapasan
────────────────────────────
[fields]

4. Integritas Kulit
────────────────────────────
[fields]

5. Nutrisi
────────────────────────────
[fields]

6. Eliminasi
────────────────────────────
[fields]

7. Status Fungsional
────────────────────────────
[fields]
```

Jangan membuat semua field menjadi satu form datar panjang.

---

# 29. CONTENT DESIGN — RESIKO JATUH

Layout:

```text
RESIKO JATUH

Instrumen: [ .... ]
Versi: [ .... ]

Penilaian
────────────────────────────
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

# 30. CONTENT DESIGN — MONITORING NYERI

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

Skala: [0—10]

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

# 31. CONTENT DESIGN — ASSEMENT EDUKASI

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

# 32. CONTENT DESIGN — PENGAWASAN HARIAN PASIEN

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
────────────────────────

Nyeri
────────────────────────

Intake
────────────────────────
Infus
Oral
NGT
Darah
Obat

Output
────────────────────────
Urin
Feses
NGT
Lain

Balance
────────────────────────

Diet & Mobilisasi
────────────────────────
```

Total/balance sebaiknya dihitung dari data terstruktur.

---

# 33. CONTENT DESIGN — EVALUASI AWAL

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

# 34. CONTENT DESIGN — PERENCANAAN PULANG

Layout:

```text
PERENCANAAN PULANG

Kebutuhan Pulang
────────────────────────

Caregiver / Pendamping
────────────────────────

Kontrol / Follow-up
────────────────────────

Obat & Edukasi
────────────────────────

Peralatan / Home Care
────────────────────────

Transportasi
────────────────────────

Hambatan
────────────────────────

Status Rencana
────────────────────────
```

Perencanaan Pulang tidak menutup episode.

---

# 35. CONTENT DESIGN — ASUHAN KEPERAWATAN

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
──────────────────────────────
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
14 Sep 2026 • 14:35
Siti Rahma, Ns.

Catatan:
...

[Detail]
```

---

# 42. CONTENT DESIGN — TINDAKAN KEPERAWATAN

```text
TINDAKAN

Order Tindakan
History Tindakan
```

Order tidak otomatis performed.

---

# 43. CONTENT DESIGN — PENUNJANG MEDIS KEPERAWATAN

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

# 44. CONTENT DESIGN — PEMAKAIAN ALAT

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

# 45. CONTENT DESIGN — TRANSFER PASIEN

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

# 46. CONTENT DESIGN — PEMESANAN RUANGAN BEDAH

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

# 47. CONTENT DESIGN — TAGIHAN PASIEN

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
Card radius 8–12px
Input radius 6–8px
Button radius 6–8px
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
Page Title      20–24px semibold
Section Title   16–18px semibold
Body            13–14px
Supporting      12–13px
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
Patient ✓
Episode ✓
Bed ✓
Alergi ✕
SOAP ✓
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
SOAP •
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

# 56. MVP-0 — CONTRACT STABILIZATION

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

# 57. MVP-1 — SHARED WORKSPACE

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

# 58. MVP-2 — KEPERAWATAN

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

# 59. MVP-3 — DOKTER

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

# 60. MVP-4 — CROSS MODULE

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

# 61. UI ACCEPTANCE CRITERIA — DOKTER

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

# 62. UI ACCEPTANCE CRITERIA — KEPERAWATAN

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

- **AC-RWI-001** — Wrong patient guard.
- **AC-RWI-002** — Draft incomplete allowed.
- **AC-RWI-003** — Final completeness validation.
- **AC-RWI-004** — Final immutable.
- **AC-RWI-005** — Correction preserves original.
- **AC-RWI-006** — Unknown safety.
- **AC-RWI-007** — Read after write.
- **AC-RWI-008** — Partial edit safety.
- **AC-RWI-009** — Permission consistency.
- **AC-RWI-010** — SOAP complete tidak silent-finalize lifecycle lain.
- **AC-RWI-011** — Non-DPJP tidak otomatis verify CPPT.
- **AC-RWI-012** — New prescription command tidak dianggap retry.
- **AC-RWI-013** — Header prescription tanpa item bukan lengkap.
- **AC-RWI-014** — Multiple valid visit allowed.
- **AC-RWI-015** — Billing failure tidak menghapus clinical record.
- **AC-RWI-016** — Closed episode read-only.
- **AC-RWI-017** — Unsaved draft tidak silent loss.
- **AC-RWI-018** — Allergy error bukan no allergy.
- **AC-RWI-019** — Final result dapat dibuka.
- **AC-RWI-020** — No duplicate owner.

---

# 64. PROHIBITED IMPLEMENTATION

```text
DILARANG:

❌ Fake queue untuk Rawat Inap.
❌ Copy tabel V1 tanpa domain mapping.
❌ Menu = tabel.
❌ Actor bebas dari payload.
❌ Role string sebagai authority tunggal.
❌ Unknown menjadi negative.
❌ Final document diedit langsung.
❌ Diagnosis multi-value comma-separated.
❌ Order = performed = billing.
❌ SOAP = Visit.
❌ Prescription header = complete prescription.
❌ Discharge planning = discharge.
❌ Signed resume = closed episode.
❌ Duplicate hasil Lab/Radiologi di Rawat Inap.
❌ Success sebelum transaksi wajib benar-benar berhasil.
❌ Magic status number FE/BE berbeda.
❌ Silent overwrite.
❌ Silent discard draft.
❌ Mock clinical data untuk terlihat selesai.
```

---

# 65. PROHIBITED UI — DOKTER

```text
❌ Membuat layout Rawat Inap "mirip" tetapi berbeda struktur.
❌ Mengubah posisi patient list.
❌ Mengubah width panel tanpa alasan teknis.
❌ Mengubah tab horizontal menjadi sidebar.
❌ Membuat card style khusus Rawat Inap.
❌ Membuat empty-state pattern baru.
❌ Membuat breakpoint berbeda tanpa kebutuhan.
❌ Menyalin business process antrean Rawat Jalan.
```

---

# 66. PROHIBITED UI — KEPERAWATAN

```text
❌ Rombak total layout yang sudah aman.
❌ Menghilangkan menu V1 tanpa keputusan eksplisit.
❌ Membuat form panjang tanpa section.
❌ Semua content dijadikan table.
❌ Semua section dijadikan nested card.
❌ Menampilkan success palsu untuk integrasi yang belum tersedia.
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
✓ menjadi context anchor

DOKTER
✓ layout sama dengan Rawat Jalan V2
✓ 8 content utama tersedia
✓ business process tetap Rawat Inap
✓ tidak memakai fake queue
✓ draft aman
✓ lifecycle benar

KEPERAWATAN
✓ layout existing dipertahankan
✓ content mengikuti kebutuhan V1
✓ pengkajian memiliki progress/completeness
✓ care plan dan intervention V2 tetap hidup
✓ tidak kehilangan capability V2

SHARED
✓ actor tervalidasi
✓ permission konsisten
✓ correction traceable
✓ integration owner jelas
✓ tidak ada silent data loss
✓ tidak ada dummy integration
```

---

# 69. REQUIRED BLUEPRINT REALIGNMENT

Setelah dokumen ini menjadi authority, update:

```text
rawat-inap/
├── blueprint-manifest.md
├── 04-prd-to-mvp-final.md
│
├── keperawatan/
│   ├── 02-module-map.md
│   ├── 03-frontend-architecture.md
│   ├── 04-prd-to-mvp.md
│   ├── skema-tampilan-keperawatan-rawat-inap.md
│   ├── api-contract.md
│   ├── validation-matrix.md
│   ├── data/data-dictionary.md
│   └── roadmap/
│       ├── backend-roadmap.md
│       └── frontend-roadmap.md
│
└── dokter-rawat-inap/
    ├── 02-module-map.md
    ├── 03-frontend-architecture.md
    ├── 04-prd-to-mvp.md
    ├── skema-tampilan-dokter-rawat-inap.md
    ├── api-contract.md
    ├── validation-matrix.md
    ├── data/data-dictionary.md
    └── roadmap/
        ├── backend-roadmap.md
        └── frontend-roadmap.md
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
