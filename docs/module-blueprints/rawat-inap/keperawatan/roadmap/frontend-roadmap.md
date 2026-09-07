# Roadmap Delivery Frontend — Sub-modul Keperawatan Rawat Inap

> ## ✅ ROADMAP INI **BOLEH DIEKSEKUSI** SEJAK 5 SEPTEMBER 2026 (REVISION 3 — 7 SEPTEMBER 2026)
>
> Revision `3` memperbarui revision `2` (5 September 2026) dan revision `1` (`DRAFT_STALE`). Tiga hal fundamental diperbarui:
>
> | Yang berubah | Pada revision `2` | Menjadi revision `3` |
> | --- | --- | --- |
> | **UI Contract & Authority** | Tata letak, pemilihan component library, dan komposisi layar dinyatakan `DEV_DISCRETION` | **Terkunci mutlak** mengikuti [`skema-tampilan-keperawatan-rawat-inap.md`](../skema-tampilan-keperawatan-rawat-inap.md) sebagai **UI Contract / Visual Implementation Contract**. `DEV_DISCRETION` hanya tersisa untuk detail mikro-styling (warna detail, pixel spacing, ikon, micro-animation) |
> | **Base Component Strategy** | Hanya menyebut generik "reuse base component Quilvian" tanpa arsitektur konkret | **Mengunci Shared Clinical Workspace Base** (`src/components/ui/clinical-workspace/`) dengan Rawat Inap sebagai *reference implementation / first adopter*, reuse form base (`src/components/ui/form-pemeriksaan-ui/`), dan migrasi bertahap dari `doctor-clinical-base` tanpa coupling permanen nama |
> | **Kelengkapan Kontrak Task (FE-RWI-051 .. 056)** | Task hanya memuat 10 field umum, tanpa arsitektur domain, state boundary, responsivitas, dan visual acceptance criteria | **Setiap task dilengkapi 20 field wajib** (UI Contract, Layout, Existing Base, Base To Extend, New Base, Domain Components, State Handling, Permission Behaviour, Episode State Behaviour, Responsive Behaviour, Visual Acceptance Criteria, dll.) |
>
> Riwayat revision sebelumnya disimpan apa adanya:
> - Revision `1`: [`archive/revision-1/frontend-roadmap.md`](./archive/revision-1/frontend-roadmap.md)
> - Revision `2`: disetujui lewat `RWI-DEC-092`, Muhammad Hamzah, 3 September 2026

## Metadata

```yaml
module_id: rawat-inap
module_name: InPatientManagement
blueprint_id: RWI-BP-001
blueprint_revision: 5
blueprint_shape: COMPOSITE
submodule: keperawatan
blueprint_root: docs/module-blueprints/rawat-inap/keperawatan/
roadmap_revision: 3
status: APPROVED
roadmap_mode: DELIVERY
approval_gate: BLUEPRINT_APPROVED
approved_by: "Muhammad Hamzah - Product/Domain owner (RWI-DEC-061)"
approved_at: "2026-09-03"
approval_decision: RWI-DEC-092
owners:
  - "Product/Domain: Muhammad Hamzah (RWI-DEC-061)"
  - "Frontend authority: sesuai decision log; IA-INP-01 s.d. IA-INP-05 mengikat; UI Contract skema-tampilan-keperawatan-rawat-inap.md mengikat"
  - "Security/Privacy: OPEN"
contract_versions: 0.3.0
input_revisions:
  blueprint-manifest.md (sub-modul): 5
  00-interview-decisions.md: 13
  02-module-map.md: 1
  03-frontend-architecture.md: 0.2
  04-prd-to-mvp.md: 0.3
  skema-tampilan-keperawatan-rawat-inap.md: 1.0 (UI Contract mengikat untuk FE-RWI-051 s.d. FE-RWI-056)
planning_source_sha:
  backend: 7d4bf2b91d39265eab4453a230ba324f95866962 (branch MHamzah, dibaca 5 September 2026)
  frontend: eb505a9ea20d99505a69ff0e6ea428ec9a551dc5 (branch HamzahV2, dibaca 5 September 2026)
task_id_range: FE-RWI-051 .. FE-RWI-056
task_count: 6
screens: 6
new_menu_items: 0
```

---

## 0. Aturan Konstitusi Frontend & Peringatan Wajib

### 0.1 Gerbang approval sudah dicabut

Blueprint sub-modul `keperawatan` berstatus **`approved`** sejak 3 September 2026 lewat
`RWI-DEC-092`. Keenam task di bawah **boleh dikirim ke eksekusi frontend**, dengan syarat mutlak:
backend pasangannya sudah tersedia (bagian 0.3). Penjelasan lengkap tentang apa yang ikut dan tidak ikut
tercabut ada pada [`backend-roadmap.md`](./backend-roadmap.md) bagian 0.1.

### 0.2 Nol butir menu baru

`02-module-map.md` bagian 3 mencatat kuota sembilan butir menu `IA-INP-05` **sudah penuh dipakai**
`episode-rawat-inap`. Keputusan 2 September 2026 menetapkan sub-modul ini mendapat **nol butir menu
tingkat dua**; keenam layarnya menjadi **layar anak**.

| Layar | Jalan masuknya | Butir hak akses penjaga |
| --- | --- | --- |
| `FE-KEP-01` Ruang Kerja Keperawatan | `FE-INP-04` Detail Episode, dan baris pasien pada `FE-INP-01` Census | `PatientAssessment : Read` |
| `FE-KEP-02` Pengkajian | `FE-KEP-01` (navigasi section internal) | `PatientAssessment : Create` / `Read` |
| `FE-KEP-03` Lini Masa | `FE-KEP-01` (navigasi section internal) | `PatientAssessment : Read` |
| `FE-KEP-04` Rencana Asuhan | `FE-KEP-01` (navigasi section internal) | `NursingCarePlan : Read` / `Create` / `Update` |
| `FE-KEP-05` Catatan Tindakan | `FE-KEP-01` (navigasi section internal) | `NursingIntervention : Read` / `Create` |
| `FE-KEP-06` Daftar Pantau Kepatuhan | `FE-INP-09` Daftar Pantau, sebagai **daftar ketiga** | `PatientAssessment : Read` |

> **PENTING:** Section navigation pada `FE-KEP-01` (Pengkajian, Rencana Asuhan, Tindakan Keperawatan,
> Lini Masa) adalah **NAVIGASI INTERNAL RUANG KERJA PASIEN**, BUKAN menu sidebar baru! Menambahkan
> butir menu baru di sidebar melanggar `IA-INP-05`.

### 0.3 Setiap layar wajib punya backend-nya lebih dulu

Approval blueprint **tidak** mengubah aturan ini. Task frontend **MUST NOT** dimulai sebelum task
backend pasangannya selesai dan endpoint-nya benar-benar dapat dipanggil.

Endpoint yang **sudah tersedia** di `BE@7d4bf2b`, dan sudah dapat dipanggil:

| Endpoint | Dipakai layar |
| --- | --- |
| `GET /api/v1/health-services/clinical-management/patient-assessments/episodes/{episodeId}` | `FE-KEP-01`, `FE-KEP-02` |
| `GET /api/v1/health-services/clinical-management/patient-assessments/{id}` | `FE-KEP-02` |
| `POST /api/v1/health-services/clinical-management/patient-assessments` | `FE-KEP-02` |
| `GET /api/v1/health-services/medical-record-management/clinical-note-addendums/by-document/{documentKind}/{documentId}` | `FE-KEP-02`, `FE-KEP-03`, `FE-KEP-05` |

### 0.4 Bentuk koreksi di layar berbeda dari perkembangan

`RWI-DEC-091` membedakan **koreksi** dari **perkembangan**, dan keduanya tampil berbeda di UI:

| Yang dikoreksi | Bentuk di layar | Kenapa |
| --- | --- | --- |
| Pengkajian keperawatan yang sudah selesai | Isi asli tetap tampil apa adanya. Koreksi muncul sebagai **baris addendum bernomor** di bawahnya, beserta alasan, penulis, dan waktunya. Status pengkajian **tetap** "Completed" | Pembetulan kesalahan administratif/klinis. Tidak ada tombol [Sunting] langsung |
| Catatan tindakan yang sudah final | Sama persis: isi asli tetap terlihat, koreksi berupa **addendum bernomor** | Pembetulan kesalahan. Tidak ada tombol [Sunting] langsung |
| Butir rencana asuhan | Panel **riwayat versi (Version History)**, bukan addendum. Versi lama menyimpan penulis dan waktu **aslinya** | Perkembangan klinis pasien (`CAP-013` aturan 5), bukan koreksi salah ketik |

### 0.5 Perubahan Authority Roadmap: Batasan `DEV_DISCRETION` Baru

Pada revision `2`, tata letak dan bentuk komponen berada di bawah `DEV_DISCRETION`. Sejak revision `3`,
dokumen [`skema-tampilan-keperawatan-rawat-inap.md`](../skema-tampilan-keperawatan-rawat-inap.md)
resmi menjadi **UI CONTRACT / VISUAL IMPLEMENTATION CONTRACT** yang mengikat.

```text
┌─────────────────────────────────────────────────────────────────────────────┐
│ ATURAN AUTHORITY ROADMAP REVISION 3                                         │
├─────────────────────────────────────────────────────────────────────────────┤
│ DEV_DISCRETION MASIH BERLAKU UNTUK:                                         │
│ - warna detail (hex / token color shading);                                 │
│ - pixel spacing mikro (padding / gap 4px vs 8px dalam batasan design token);│
│ - ikon visual spesifik;                                                     │
│ - micro-animation & transition timing;                                      │
│ - minor visual styling;                                                     │
│ - pilihan icon library yang selaras Quilvian design system.                 │
│                                                                             │
│ DEV_DISCRETION TIDAK LAGI BERLAKU UNTUK (TERKUNCI MUTLAK):                  │
│ - Struktur layout utama Clinical Workspace;                                 │
│ - Patient Context Header & 9 item identitas wajib;                          │
│ - Left Section Navigation & 4 section internal;                             │
│ - Main Clinical Content paneling;                                           │
│ - Right Quick Summary & Context/Alert paneling;                             │
│ - Screen composition per task;                                              │
│ - Mandatory state handling (Loading, Empty, Error, 403, Closed, Double-Sub);│
│ - Penggunaan Timeline vs Master-Detail;                                     │
│ - Addendum display vs Revision History display;                             │
│ - Billing state placement (terisolasi dari clinical save state);             │
│ - Responsive region behaviour (Desktop, Tablet, Mobile).                     │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 0.6 Strategi Shared Clinical Workspace Base & Reference Implementation

Layout Keperawatan Rawat Inap terinspirasi dari pola Pengkajian IGD existing. Namun:
- **JANGAN** menjadikan source IGD existing sebagai base secara mentah;
- **JANGAN** memindahkan technical debt atau visual defect IGD ke Rawat Inap.

Strategi canonical yang wajib dieksekusi:

```text
IGD EXISTING
     ↓  (ambil konsep 4 wilayah: Header + Nav + Content + Summary)
SHARED CLINICAL WORKSPACE BASE  (src/components/ui/clinical-workspace/)
     ↓
RAWAT INAP KEPERAWATAN  (first adopter / reference implementation yang bersih)
     ↓  (setelah stabil & terverifikasi)
REFACTOR IGD  (IGD direfactor menyusul menggunakan shared base yang sama)
```

> **CATATAN PENTING:** Rencana refactor IGD adalah fase masa depan dan **BUKAN BLOCKER** bagi pembangunan
> Keperawatan Rawat Inap. Keperawatan Rawat Inap membangun dan menggunakan shared base ini secara mandiri.

**Daftar Base Components yang Dibangun pada `src/components/ui/clinical-workspace/`:**

1. Shell & Layout Context:
   - `ClinicalWorkspaceShell.jsx`: Shell 4 wilayah fleksibel.
   - `PatientContextHeader.jsx`: Header konteks pasien sticky & rapi (non-grid administratif).
   - `ClinicalSectionNav.jsx`: Navigasi section kiri vertikal berbasis konfigurasi props.
   - `ClinicalContentPanel.jsx`: Kontainer konten klinis utama.
   - `ClinicalQuickSummary.jsx`: Panel ringkasan cepat kanan berbasis konfigurasi metrik.
   - `ClinicalInfoPanel.jsx`: Panel peringatan klinis / context status kanan bawah.
   - `ClinicalStateBoundary.jsx`: Boundary penanganan loading, error, empty, dan permission denied.
   - `ClinicalSafetyAlert.jsx`: Komponen banner peringatan keselamatan klinis (alergi, jatuh tempo).
   - `ClinicalStatusBadge.jsx`: Badge status dokumen dan status klinis.
   - `index.js`: Re-export clean interface.

2. Specialized Base Components (dibuat sesuai kebutuhan task):
   - `ClinicalTimeline.jsx` & `ClinicalTimelineItem.jsx`: Visual lini masa klinis kronologis (`FE-RWI-053`, `FE-RWI-055`).
   - `ClinicalDocumentMeta.jsx`: Metadata dokumen (penulis, tanggal/jam, peran).
   - `ClinicalDeadlineBadge.jsx`: Penanda tenggat waktu & sisa durasi / "Batas waktu belum ditetapkan" (`FE-RWI-052`).
   - `ClinicalCompletionBar.jsx`: Progress bar kelengkapan bagian pengkajian (X/7 bagian) (`FE-RWI-052`).
   - `ClinicalAddendumList.jsx` & `ClinicalAddendumItem.jsx`: Tampilan addendum koreksi bernomor urut (`FE-RWI-052`, `FE-RWI-055`).
   - `ClinicalRevisionHistory.jsx`: Panel riwayat versi rencana asuhan dengan author & timestamp asli (`FE-RWI-054`).
   - `ClinicalValidationSummary.jsx`: Ringkasan rincian bagian yang belum lengkap saat finalisasi (`FE-RWI-052`).
   - `ClinicalActionGuard.jsx`: Penjaga tombol aksi bersyarat dengan tooltip alasan nonaktif (`FE-RWI-054`).

> **ATURAN NETRAL DOMAIN:** Dilarang keras membuat 3 base terpisah seperti `doctor-clinical-base`,
> `nursing-clinical-base`, atau `igd-clinical-base` untuk fungsi generik yang sama. Komponen pada
> `clinical-workspace/` harus netral terhadap domain medis/perawat.

### 0.7 Strategi Reuse Existing Frontend & Migrasi

1. **Existing Form Base:**
   Gunakan komponen terverifikasi pada `src/components/ui/form-pemeriksaan-ui/`:
   - `BaseModal`
   - `BaseSelectField`
   - `BaseTextField`
   - `BaseTextareaField`
   - `BaseCheckboxCard`
   - `BaseSimpleCheckbox`
2. **Migrasi Komponen Generik `doctor-clinical-base`:**
   Komponen generik yang saat ini berada di `src/components/ui/doctor-clinical-base/` (seperti `ClinicalStateBoundary`,
   `ClinicalSafetyAlert`, `ClinicalStatusBadge`) dapat direfactor/dipindahkan bertahap ke `src/components/ui/clinical-workspace/`.
   Sub-modul Keperawatan Rawat Inap **TIDAK BOLEH** bergantung permanen pada nama folder `doctor-clinical-base`.

### 0.8 Struktur Folder Domain Keperawatan Rawat Inap

Pemisahan tanggung jawab (*separation of concerns*) wajib dijaga. **Dilarang keras membuat satu giant component**
untuk seluruh Nursing Workspace.

Arah struktur folder domain target:

```text
src/components/view/
└── health-services/
    └── inpatient-management/
        └── nursing-workspace/
            ├── nursing-workspace-client.jsx
            ├── nursing-workspace-view.jsx
            │
            ├── components/
            │   ├── nursing-episode-header.jsx
            │   ├── responsible-nurse-indicator.jsx
            │   └── nursing-workspace-sections.jsx
            │
            ├── sections/
            │   ├── assessment/
            │   │   ├── assessment-section.jsx
            │   │   ├── assessment-form-nav.jsx
            │   │   └── groups/ (7 kelompok isian klinis)
            │   ├── timeline/
            │   │   ├── assessment-timeline-section.jsx
            │   │   └── timeline-filter-bar.jsx
            │   ├── care-plan/
            │   │   ├── care-plan-section.jsx
            │   │   ├── care-plan-master-list.jsx
            │   │   └── care-plan-detail-view.jsx
            │   └── intervention/
            │       ├── nursing-intervention-section.jsx
            │       └── intervention-timeline-item.jsx
            │
            └── modals/
                ├── complete-assessment-modal.jsx
                ├── add-correction-modal.jsx
                ├── care-plan-item-modal.jsx
                ├── care-plan-evaluation-modal.jsx
                ├── close-care-plan-item-modal.jsx
                └── nursing-intervention-modal.jsx
```

### 0.9 Aturan Global Koreksi vs Perkembangan / Revisi

Roadmap menetapkan pemisahan tegas antara pembetulan kesalahan (*correction*) dan perkembangan klinis (*progression*):

```text
1. PENGKAJIAN KEPERAWATAN (STATUS COMPLETED)
   Kasus: Perawat salah input angka skor nyeri (misal 7 padahal 4).
   Mekanisme: KOREKSI VIA ADDENDUM BERNOMOR (#1, #2, dst.).
   Aturan: Isi asli tetap terlihat di atas, addendum di bawah memuat alasan wajib, penulis, dan waktu.
           Status dokumen tetap "Completed". Tombol [Sunting] DILARANG ada.

2. CATATAN TINDAKAN KEPERAWATAN (STATUS FINALIZED)
   Kasus: Perawat salah menulis lokasi pemasangan infus (tangan kanan padahal tangan kiri).
   Mekanisme: KOREKSI VIA ADDENDUM BERNOMOR.
   Aturan: Tindakan asli tetap terlihat. Addendum mencatat pembetulan dan alasan. Status tetap "Finalized".
           Tombol [Sunting] DILARANG ada.

3. RENCANA ASUHAN KEPERAWATAN
   Kasus: Target nyeri diubah dari skala 5 menjadi 3 karena kondisi pasien membaik.
   Mekanisme: RIWAYAT VERSI (VERSION HISTORY: Versi 1, Versi 2, Versi 3 - CURRENT).
   Aturan: Menggunakan ClinicalRevisionHistory. Wajib mempertahankan penulis asli dan waktu asli
           pembuatan tiap versi. DILARANG diubah menjadi addendum koreksi kesalahan!

4. PENGKAJIAN ULANG (REASSESSMENT)
   Kasus: Nyeri pasien benar-benar turun dari 7 ke 4 setelah obat analgetik masuk.
   Mekanisme: DOKUMEN PENGKAJIAN ULANG BARU.
   Aturan: Tercatat sebagai titik waktu baru di Lini Masa (FE-RWI-053). Bukan koreksi atas dokumen lama.
```

### 0.10 Matriks Penanganan State Global & Non-Equivalences

Setiap layar yang berinteraksi dengan data asinkron wajib menangani 8 state berikut:
`LOADING`, `EMPTY`, `ERROR`, `RETRY`, `READ_ONLY`, `PERMISSION_DENIED`, `EPISODE_CLOSED`, dan `REQUEST_IN_PROGRESS` (pencegahan double submit).

**Lima Prinsip Non-Equivalence (Dilarang Keras Disamakan):**
1. **EMPTY ≠ ERROR:** Keadaan belum ada data klinis (misal belum ada pengkajian) berbeda total dari kegagalan jaringan/server memuat data.
2. **NO POLICY ≠ ON TIME:** Master kebijakan belum diatur (`MstClinicalAssessmentPolicy` kosong) berarti "Batas waktu belum ditetapkan", BUKAN berarti "tepat waktu" dan BUKAN error. Pengkajian tetap dapat dilakukan tanpa countdown palsu.
3. **BILLING FAILED ≠ CLINICAL SAVE FAILED:** Kegagalan dispatch tagihan ke Billing TIDAK BOLEH menggagalkan catatan klinis tindakan. Tindakan klinis tetap tersimpan dengan penanda kecil "Tagihan belum terkirim", tanpa full-page error.
4. **CORRECTION ≠ CLINICAL PROGRESSION:** Addendum koreksi hanya untuk pembetulan salah tulis; perubahan kondisi pasien harus tercatat sebagai versi baru atau pengkajian ulang.
5. **FINAL ≠ EDITABLE:** Dokumen yang sudah berstatus `Completed` atau `Finalized` tidak boleh menampilkan tombol [Sunting]. Perubahan hanya dapat melalui [Tambah Koreksi].

### 0.11 Kontrak Responsif Minimum

Struktur UI wajib usable dan adaptif pada 3 breakpoint utama:

```text
DESKTOP (> 1024px):
┌────────────────────────────────────────────────────────────────────────────┐
│ PATIENT CONTEXT HEADER (Sticky)                                            │
├─────────────────┬────────────────────────────────────────┬─────────────────┤
│ Left Nav        │ Main Clinical Content                  │ Quick Summary   │
│ (~220px)        │ (Fluid)                                │ (~260px)        │
│                 │                                        ├─────────────────┤
│                 │                                        │ Context / Alert │
└─────────────────┴────────────────────────────────────────┴─────────────────┘

TABLET (768px - 1024px):
┌────────────────────────────────────────────────────────────────────────────┐
│ PATIENT CONTEXT HEADER                                                     │
├─────────────────┬──────────────────────────────────────────────────────────┤
│ Left Nav        │ Main Clinical Content                                    │
│                 ├──────────────────────────────────────────────────────────┤
│                 │ Quick Summary & Alerts (Collapsible / Bottom Region)     │
└─────────────────┴──────────────────────────────────────────────────────────┘

MOBILE (< 768px):
┌────────────────────────────────────────────────────────────────────────────┐
│ PATIENT CONTEXT HEADER (Ringkas, Alert Alergi & Status Episode Tetap Muncul)│
├────────────────────────────────────────────────────────────────────────────┤
│ Internal Section Selector (Dropdown / Horizontal Segmented Control)        │
├────────────────────────────────────────────────────────────────────────────┤
│ Main Clinical Content (Full Width)                                         │
├────────────────────────────────────────────────────────────────────────────┤
│ Quick Summary Cards                                                        │
├────────────────────────────────────────────────────────────────────────────┤
│ Context & Alert Region                                                     │
└────────────────────────────────────────────────────────────────────────────┘
```

> **ATURAN KESELAMATAN PADA MOBILE:** Informasi identitas pasien kritis (Nama, No RM, Lokasi Bed,
> Tanda Bahaya Alergi, dan Status Episode) **DILARANG KERAS DISEMBUNYIKAN** pada viewport mobile.

### 0.12 Visual Acceptance Gate Global (22 Kriteria Kelulusan Visual)

Implementasi sub-modul Keperawatan Rawat Inap **BELUM BOLEH DIANGGAP SELESAI (DONE)** bila ditemukan salah satu dari 22 pelanggaran berikut:
1. Layout tidak mengikuti pola 4-wilayah Shared Clinical Workspace (`Header + Left Nav + Main Content + Right Summary`).
2. Informasi pasien masih berupa grid administratif IGD yang padat dan sulit dibaca.
3. Patient context tidak selalu terlihat saat konten digulung.
4. Peringatan kegagalan memuat alergi tersembunyi atau dianggap "tidak ada alergi".
5. Kegagalan memuat konteks pasien/episode masih mengizinkan aksi tulis (tombol tulis tidak disabled).
6. Menu section Keperawatan di-hardcode di shared base component (bukan berbasis props config).
7. Quick Summary tidak configurable.
8. Pengkajian Keperawatan tidak memiliki 7 kelompok isian yang jelas dalam 1 layar.
9. Dokumen pengkajian/tindakan yang sudah berstatus selesai masih menampilkan tombol [Sunting].
10. Koreksi dokumen menghapus, menimpa, atau menggantikan isi teks asli.
11. Addendum koreksi tidak memuat nomor urut, nama penulis, waktu koreksi, dan alasan wajib.
12. Lini masa pengkajian hanya menampilkan nilai terakhir (kehilangan riwayat perkembangan).
13. Rencana Asuhan menggunakan addendum alih-alih Version History.
14. Version History pada Rencana Asuhan kehilangan nama pembuat dan waktu asli versi terkait.
15. Formulir pencatatan tindakan mewajibkan kaitan ke Rencana Asuhan (tindakan cito gagal dicatat).
16. Kegagalan dispatch Billing ditampilkan sebagai kegagalan penyimpanan klinis atau full-page error.
17. Catatan tindakan final masih memiliki tombol Sunting langsung.
18. Episode berstatus `Closed` masih menyediakan tombol aksi mutasi (tambah/ubah/koreksi aktif).
19. Layar Daftar Pantau Kepatuhan menampilkan isi catatan klinis bebas (melanggar privasi pasien).
20. State data kosong, kebijakan belum ada, dan kegagalan jaringan tidak dapat dibedakan.
21. Tata letak rusak atau informasi keselamatan hilang saat dibuka di tablet atau mobile.
22. Shared base component memuat logika bisnis atau penamaan spesifik IGD atau Rawat Inap.

---

## 1. Cara Membaca Roadmap Ini

**Arti tanda status pada dokumen ini:**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria, Visual Acceptance criteria, dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi kriteria penerimaan belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

Hari ini keenam task berstatus **belum dikerjakan**, dan **tidak satu pun** bertanda ⛔. Yang
menahan mereka bukan gerbang approval, melainkan ketersediaan backend pasangannya.

---

## 2. Gelombang dan Urutan Dependency

| Gelombang | Task | Layar | Prasyarat Backend |
| --- | --- | --- | --- |
| **`KEP-MVP-1`** | `FE-RWI-051` ✅ | `FE-KEP-01` Ruang Kerja Keperawatan | `BE-RWI-054` |
| **`KEP-MVP-1`** | `FE-RWI-052` ✅ | `FE-KEP-02` Pengkajian Keperawatan | `BE-RWI-056`, `BE-RWI-065`, `BE-RWI-057` |
| **`KEP-MVP-1`** | `FE-RWI-053` | `FE-KEP-03` Lini Masa Pengkajian | `BE-RWI-058` |
| **`KEP-MVP-2`** | `FE-RWI-054` | `FE-KEP-04` Rencana Asuhan | `BE-RWI-059`, `BE-RWI-060` |
| **`KEP-MVP-3`** | `FE-RWI-055` | `FE-KEP-05` Catatan Tindakan | `BE-RWI-061`, `BE-RWI-062` |
| **`KEP-MVP-4`** | `FE-RWI-056` | `FE-KEP-06` Daftar Pantau Kepatuhan | `BE-RWI-064` |

```text
FE-RWI-051 (✅) ─┬─> FE-RWI-052 (✅) ─> FE-RWI-053
                 ├─> FE-RWI-054
                 └─> FE-RWI-055

FE-RWI-056  (berdiri sendiri, menempel pada FE-INP-09 Daftar Pantau)
```

---

## 3. Spesifikasi Detail Task Frontend (FE-RWI-051 .. FE-RWI-056)

### `FE-RWI-051` — Perawat Membuka Ruang Kerja Klinis Terpadu Satu Pasien

| Field | Spesifikasi & Kontrak |
| --- | --- |
| **Task ID** | `FE-RWI-051` |
| **Status** | ✅ **Selesai 7 September 2026.** Laporan perubahan tracked tersedia di [`../task/report/frontend/FE-RWI-051.md`](../task/report/frontend/FE-RWI-051.md). Seluruh 8 AC dan 7 Visual AC terbukti, 6 unit test lulus, 0 lint error. |
| **Outcome** | Perawat membuka satu ruang kerja klinis terpadu untuk satu pasien rawat inap yang menjadi tanggung jawabnya, melihat identitas lengkap, tanda bahaya alergi, ringkasan cepat, serta berpindah antar dokumentasi asuhan tanpa berganti rute sidebar |
| **Trace** | `FE-KEP-01`; `FR-KEP-001` s.d. `FR-KEP-004`; `03-frontend-architecture.md` bagian 3.1; `IA-INP-01` (tiga klik dari Beranda); `skema-tampilan-keperawatan-rawat-inap.md` Bagian 2, 4, 5, 6, 7, 8, 24, 25, 26, 27 |
| **Kontrak** | `contracts/api-contract.md` `0.3.0` grup Patient Assessment; konteks episode lewat `INT-KEP-02` (`GET /api/v1/health-services/inpatient-management/inpatient-episodes/{id}`); data alergi lewat `GET /api/v1/health-services/clinical-management/patient-allergies` |
| **Dependency** | `BE-RWI-054` |
| **UI Contract** | [`skema-tampilan-keperawatan-rawat-inap.md`](../skema-tampilan-keperawatan-rawat-inap.md) Bagian 2, 4, 5, 6, 7, 8 |
| **Layout** | **Pola Shared Clinical Workspace Shell 4 Wilayah:**<br>1. **Atas:** `PatientContextHeader` selalu terlihat (sticky), menampilkan 9 butir data pasien.<br>2. **Kiri:** `ClinicalSectionNav` vertikal dengan 4 section internal (`Pengkajian`, `Rencana Asuhan`, `Tindakan Keperawatan`, `Lini Masa`).<br>3. **Tengah:** `ClinicalContentPanel` dinamis sesuai section aktif.<br>4. **Kanan:** `ClinicalQuickSummary` (rekap pengkajian, masalah aktif, tindakan, koreksi, pengkajian terlambat) + `ClinicalInfoPanel` (status pengkajian & alert klinis). |
| **Existing Base Components** | `src/components/ui/doctor-clinical-base/ClinicalStateBoundary.jsx` (diadaptasi), `ClinicalSafetyAlert.jsx`, `ClinicalStatusBadge.jsx`; pola layout detail episode `FE-INP-04` |
| **Base Components To Extend** | Mengabstraksi komponen generik dari `doctor-clinical-base/` menjadi domain-neutral di `clinical-workspace/` tanpa ikatan nama dokter |
| **New Base Components** | `src/components/ui/clinical-workspace/`:<br>- `ClinicalWorkspaceShell.jsx`<br>- `PatientContextHeader.jsx`<br>- `ClinicalSectionNav.jsx`<br>- `ClinicalContentPanel.jsx`<br>- `ClinicalQuickSummary.jsx`<br>- `ClinicalInfoPanel.jsx`<br>- `ClinicalStateBoundary.jsx`<br>- `ClinicalSafetyAlert.jsx`<br>- `ClinicalStatusBadge.jsx`<br>- `index.js` |
| **Domain Components** | `src/components/view/health-services/inpatient-management/nursing-workspace/`:<br>- `nursing-workspace-client.jsx`<br>- `nursing-workspace-view.jsx`<br>- `components/nursing-episode-header.jsx`<br>- `components/responsible-nurse-indicator.jsx`<br>- `components/nursing-workspace-sections.jsx` |
| **State Handling** | - **Context Loading:** Skeleton header & shell layout.<br>- **Context Success:** Render 9 data identitas pasien.<br>- **Context Failure (CRITICAL):** Tampil pesan keselamatan *"DATA PASIEN TIDAK DAPAT DIMUAT. Identitas pasien dan episode belum dapat diverifikasi. Jangan melakukan dokumentasi sebelum konteks pasien tersedia."* + tombol `[Coba Lagi]`. **SELURUH AKSI TULIS (CREATE/UPDATE) WAJIB DISABLED SECARA GLOBAL**.<br>- **Allergy Success:** Tampil badge alergi mencolok bila ada, atau *"Tidak ada alergi tercatat"* bila bersih.<br>- **Allergy Failure (CRITICAL):** Tampil alert keselamatan menonjol: *"⚠ RIWAYAT ALERGI TIDAK DAPAT DIMUAT. Data alergi pasien belum dapat diverifikasi."* **DILARANG KERAS DIANGGAP 'TIDAK ADA ALERGI'**.<br>- **403 Forbidden:** State penolakan izin yang menyebut hak akses yang dibutuhkan.<br>- **Episode Closed:** Banner informasi bahwa episode sudah ditutup dan seluruh workspace berstatus hanya-baca. |
| **Permission Behaviour** | - Membaca workspace: `PatientAssessment : Read` atau `InpatientEpisode : Read`.<br>- Perawat Pelaksana & Kepala Ruangan: Akses penuh baca dan buka section tulis.<br>- DPJP: Akses baca saja (seluruh tombol tulis nonaktif).<br>- Pengguna tanpa hak akses: Tampil 403 informatif dengan penunjuk role berwenang. |
| **Episode State Behaviour** | - Episode `Admitted`: Beroperasi penuh untuk baca dan aksi tulis.<br>- Episode `Closed`: Otomatis beralih ke mode read-only; seluruh form/tombol mutasi disembunyikan; data riwayat tetap dapat ditelusuri.<br>- Auto-refetch context saat window refocus untuk mendeteksi perubahan status episode seketika. |
| **Responsive Behaviour** | - **Desktop (>1024px):** 3 kolom penuh (Header atas, Left Nav ~220px, Content fluid, Quick Summary ~260px).<br>- **Tablet (768-1024px):** 2 kolom (Header atas, Left Nav kiri, Content fluid; Quick Summary turun ke bawah konten atau menjadi collapsible drawer).<br>- **Mobile (<768px):** 1 kolom linier (Header ringkas -> Section Selector horizontal/dropdown -> Content -> Quick Summary -> Alert). Data identitas pasien kritis & alergi pantang disembunyikan. |
| **Scope** | Route `…/episodes/{id}/nursing`; kerangka ruang kerja 4-wilayah; kepala konteks pasien dan episode; 4 navigasi section internal; jalan masuk dari `FE-INP-04` dan baris census `FE-INP-01` (maks 3 klik); penegakan state boundary keselamatan; nol butir menu baru di sidebar. |
| **Acceptance Criteria** | 1. Layar tercapai dari Beranda dalam paling banyak **tiga klik** lewat Census (`IA-INP-01`).<br>2. Kepala konteks menampilkan 9 elemen riil: Nama Pasien, No RM, Episode, Lokasi (Kamar/Bed), Hari Rawat, DPJP, Perawat PJ, Alergi, Status Episode.<br>3. **Bila kepala konteks gagal dimuat, seluruh tombol tulis nonaktif** dan pesan keselamatan beserta tombol coba lagi tampil.<br>4. Kegagalan memuat alergi **ditampilkan menonjol**, tidak disembunyikan, dan tidak dianggap "tidak ada alergi".<br>5. Tanpa `PatientAssessment : Read`, layar tidak dapat dibuka dan menampilkan pesan hak akses yang jelas.<br>6. **Nol butir menu baru** ditambahkan ke sidebar aplikasi (`IA-INP-05`).<br>7. Navigasi section internal menyediakan 4 pilihan: Pengkajian, Rencana Asuhan, Tindakan Keperawatan, Lini Masa.<br>8. Quick Summary dapat dikonfigurasi (*configurable*) dan menyajikan data rekapitulasi keperawatan. |
| **Visual Acceptance Criteria** | 1. Struktur layout presisi mengikuti visual contract [`skema-tampilan-keperawatan-rawat-inap.md`](../skema-tampilan-keperawatan-rawat-inap.md) Bagian 2 dan 5.<br>2. Patient Context Header tertata rapi dengan hierarki visual teratur (Nama & RM prioritas utama), bukan grid administratif padat ala IGD existing.<br>3. Patient Context Header tetap terlihat (*sticky*) saat konten utama digulung.<br>4. Section aktif pada Left Section Nav memiliki indikator visual selected state yang kontras dan jelas.<br>5. Quick Summary dan Info/Alert Panel di sisi kanan berpenampilan card modular terpisah yang proporsional.<br>6. Tampilan adaptif pada viewport desktop, tablet, dan mobile tanpa merusak keterbacaan data.<br>7. Shared base component bebas dari hard-code istilah atau logika bisnis IGD. |
| **Verification** | Telusur tiga klik dari Beranda; pemeriksaan sidebar sebelum dan sesudah; uji tanpa hak akses; simulasi kegagalan konteks pasien (verifikasi seluruh aksi tulis mati); simulasi kegagalan endpoint alergi (verifikasi alert bahaya muncul); pengujian responsif pada resolusi 1440px, 768px, dan 375px. |
| **Risk / Blocker** | Formulir kosong di atas konteks pasien yang belum pasti adalah risiko keselamatan serius (salah pasien). Kriteria 3 dan 4 adalah aturan keselamatan mutlak. Owner: Frontend Authority. |
| **DoD** | Shell workspace 4 wilayah terpasang; seluruh New Base Components tersedia di `src/components/ui/clinical-workspace/`; struktur domain `nursing-workspace/` rapi; seluruh AC dan Visual AC terbukti; `npm run lint` lulus; `npm run build` lulus. |

---

### `FE-RWI-052` — Perawat Mengisi, Menyelesaikan, dan Membetulkan Pengkajian

| Field | Spesifikasi & Kontrak |
| --- | --- |
| **Task ID** | `FE-RWI-052` |
| **Status** | ✅ **Selesai 7 September 2026.** Laporan perubahan tracked tersedia di [`../task/report/frontend/FE-RWI-052.md`](../task/report/frontend/FE-RWI-052.md). Seluruh 6 AC dan 5 Visual AC terbukti, 8 unit test lulus, 0 lint error. |
| **Outcome** | Perawat mengisi pengkajian awal dan pengkajian ulang dalam 1 layar terstruktur (7 kelompok isian), memantau tenggat waktu, menyelesaikan pengkajian hingga terkunci, dan membetulkan pengkajian final lewat koreksi beralasan via addendum tanpa menghapus isi aslinya |
| **Trace** | `FE-KEP-02`; `FR-KEP-005`, `FR-KEP-006`, `FR-KEP-008`, `FR-KEP-009`; `RWI-DEC-091`, `RWI-AC-175`; `03-frontend-architecture.md` bagian 3.2; `skema-tampilan-keperawatan-rawat-inap.md` Bagian 9, 10, 11, 12, 24, 25, 27 |
| **Kontrak** | `contracts/api-contract.md` `0.3.0` `POST /api/v1/health-services/clinical-management/patient-assessments`, `GET /{id}`, `PATCH /{id}/complete`, `POST /{id}/addendums`, `GET /{id}/addendums`; `GET /episodes/{id}/due-status`; `contracts/validation-matrix.md` `VAL-KEP-08`, `VAL-KEP-11`, `VAL-KEP-12`, `VAL-KEP-17` |
| **Dependency** | `FE-RWI-051`, `BE-RWI-056`, `BE-RWI-065`, `BE-RWI-057` |
| **UI Contract** | [`skema-tampilan-keperawatan-rawat-inap.md`](../skema-tampilan-keperawatan-rawat-inap.md) Bagian 9 (Screen structure & 7 kelompok), Bagian 10 (Penanda tenggat & policy empty), Bagian 11 (Penyelesaian pengkajian), Bagian 12 (Koreksi addendum) |
| **Layout** | **Satu Layar Terpadu dengan Internal Section Navigation (DILARANG memecah menjadi 7 route/page terpisah):**<br>1. **Header Form:** Radio pemilih Jenis Pengkajian (`Pengkajian Awal` vs `Pengkajian Ulang`), Status badge (`Draft`, `Completed`), dan `ClinicalDeadlineBadge`.<br>2. **Body (2 Kolom Internal):** Kiri = Navigasi 7 Bagian Pengkajian (`Kajian Umum`, `Risiko Jatuh`, `Nyeri`, `Skrining Gizi`, `Kemandirian`, `Edukasi`, `Rencana Pemulangan`). Kanan = Form isian kelompok aktif.<br>3. **Footer:** `ClinicalCompletionBar` (misal *"Progress 5/7 bagian selesai"*) + Tombol aksi `[Simpan Draft]` dan `[Selesaikan Pengkajian]`.<br>4. **Area Koreksi:** Bila status `Completed`, tampil tombol `[Tambah Koreksi]` dan daftar `ClinicalAddendumList` di bawah isian asli. |
| **Existing Base Components** | `src/components/ui/form-pemeriksaan-ui/BaseTextField.jsx`, `BaseTextareaField.jsx`, `BaseSelectField.jsx`, `BaseCheckboxCard.jsx`, `BaseSimpleCheckbox.jsx`, `BaseModal.jsx` |
| **Base Components To Extend** | Integrasi field komponen form pemeriksaan ke dalam kelompok isian pengkajian |
| **New Base Components** | `src/components/ui/clinical-workspace/`:<br>- `ClinicalDeadlineBadge.jsx`<br>- `ClinicalCompletionBar.jsx`<br>- `ClinicalValidationSummary.jsx`<br>- `ClinicalAddendumList.jsx`<br>- `ClinicalAddendumItem.jsx` |
| **Domain Components** | `src/components/view/health-services/inpatient-management/nursing-workspace/`:<br>- `sections/assessment/assessment-section.jsx`<br>- `sections/assessment/assessment-form-nav.jsx`<br>- `sections/assessment/groups/` (7 form kelompok klinis)<br>- `modals/complete-assessment-modal.jsx`<br>- `modals/add-correction-modal.jsx` |
| **State Handling** | - **Draft / InProgress:** Seluruh input form dapat diedit; tombol Simpan dan Selesaikan aktif.<br>- **Completed (Final):** Dokumen dikunci penuh (*read-only*). **TOMBOL [SUNTING] DILARANG TAMPIL**. Tombol `[Tambah Koreksi]` aktif bagi yang berwenang.<br>- **Kebijakan Batas Waktu Kosong (`MstClinicalAssessmentPolicy` kosong):** Penanda tenggat berbunyi *"○ Batas waktu belum ditetapkan"*. Pengkajian **TETAP DAPAT DIISI DAN DISELESAIKAN** secara normal tanpa countdown buatan.<br>- **Validasi Kelengkapan Gagal:** Modal `ClinicalValidationSummary` memunculkan rincian bagian yang belum lengkap (misal: *"• Risiko Jatuh, • Skrining Gizi"*) sesuai `VAL-KEP-08`.<br>- **Double Submit:** Tombol simpan/selesaikan dinonaktifkan seketika saat diklik hingga respons selesai.<br>- **Pencegahan Pengkajian Awal Kedua (422):** Menampilkan dialog informatif yang mengarahkan perawat memilih Pengkajian Ulang, bukan pesan galat teknis. |
| **Permission Behaviour** | - Mengisi & menyelesaikan pengkajian: Perawat penanggung jawab & Kepala Ruangan.<br>- Menambah koreksi pada pengkajian selesai: Penulis asli dokumen ATAU Kepala Ruangan.<br>- DPJP & Peran Lain: Hanya-baca (*read-only*); tombol simpan, selesaikan, dan koreksi disembunyikan. |
| **Episode State Behaviour** | - Episode `Admitted`: Dapat membuat, mengedit draft, menyelesaikan, dan mengoreksi.<br>- Episode `Closed`: Seluruh form terkunci hanya-baca; seluruh tombol mutasi dan tambah koreksi disembunyikan. |
| **Responsive Behaviour** | - **Desktop:** Navigasi 7 bagian di sisi kiri form (~200px), isian aktif di sisi kanan.<br>- **Tablet:** Navigasi 7 bagian dapat berupa segmented bar scroll horizontal di atas form.<br>- **Mobile:** Dropdown pemilih kelompok isian di atas form; sticky completion bar di bawah. Modal koreksi tampil full-screen sheet. |
| **Scope** | Form pengkajian awal dan ulang bertujuh kelompok isian; tombol Simpan dan Selesaikan; dialog tambah koreksi dengan alasan wajib; daftar koreksi bernomor; penanda jenis pengkajian; penanda tenggat waktu; penyajian pesan penolakan validation matrix apa adanya. |
| **Acceptance Criteria** | 1. Percobaan membuat pengkajian awal **kedua** menampilkan pesan ramah yang mengarahkan ke pengkajian ulang, bukan pesan teknis.<br>2. Koreksi tanpa alasan ditolak **di layar sebelum dikirim** ke server.<br>3. Pengkajian yang sudah selesai **tidak menampilkan tombol sunting langsung**; yang tersedia hanya tombol "Tambah koreksi" bagi pengguna berwenang.<br>4. Isi asli pengkajian tetap tampil sesudah dikoreksi, dan koreksinya tampil sebagai baris addendum bernomor urut beserta alasan, penulis, dan waktunya. Status dokumen asli **tetap "Completed"**.<br>5. Pengiriman ganda (*double submission*) dicegah secara visual dan request in-progress.<br>6. Penanda tenggat berbunyi **"Batas waktu belum ditetapkan"** ketika master kebijakan kosong, dan **tidak** menahan pengisian. |
| **Visual Acceptance Criteria** | 1. Satu layar pengkajian terpadu yang memuat 7 kelompok isian klinis secara teratur (bukan 7 route terpisah).<br>2. Progress kelengkapan pengkajian divisualisasikan dengan jelas (`ClinicalCompletionBar`).<br>3. Dokumen selesai terkunci dengan visual banner penguncian dokumen yang tegas.<br>4. Addendum koreksi tersaji rapi bernomor urut (`#1`, `#2`, dst.) tepat di bawah konten asli.<br>5. Ringkasan bagian yang belum lengkap tersaji berupa bullet list yang mudah dipahami saat konfirmasi penyelesaian. |
| **Verification** | Skenario pengkajian awal kedua; skenario simpan draft; skenario penyelesaian dengan bagian kosong; skenario koreksi tanpa alasan (harus diblokir form); skenario koreksi berulang dan pemeriksaan nomor urut addendum; uji klik ganda; skenario master kebijakan kosong. |
| **Risk / Blocker** | Menampilkan tombol edit langsung pada pengkajian final atau menghapus isi asli saat koreksi melanggar legalitas rekam medis. Solusi: lock view state saat status `Completed`. Owner: Frontend Authority. |
| **DoD** | Form pengkajian 7 kelompok terintegrasi; reuse `form-pemeriksaan-ui` terbukti; modal penyelesaian dan koreksi addendum berfungsi; seluruh AC dan Visual AC terbukti; `npm run lint` lulus; `npm run build` lulus. |

---

### `FE-RWI-053` — Perkembangan Pasien Terbaca Sebagai Garis Waktu, Bukan Angka Terakhir

| Field | Spesifikasi & Kontrak |
| --- | --- |
| **Task ID** | `FE-RWI-053` |
| **Status** | Belum dikerjakan. **Tidak terblokir**; menunggu backend `BE-RWI-058` |
| **Outcome** | Perawat dan DPJP melihat apakah nyeri, risiko jatuh, dan status gizi pasien membaik atau memburuk sejak masuk melalui visual lini masa kronologis tanpa kehilangan data historis |
| **Trace** | `FE-KEP-03`; `FR-KEP-007`, `FR-KEP-010`, `FR-KEP-011`; `AC-CAP012-02`; `03-frontend-architecture.md` bagian 3.3; `skema-tampilan-keperawatan-rawat-inap.md` Bagian 13, 24, 25, 27 |
| **Kontrak** | `contracts/api-contract.md` `0.3.0` `GET /api/v1/health-services/clinical-management/patient-assessments/episodes/{episodeId}/timeline` dan `GET /api/v1/health-services/clinical-management/patient-assessments/episodes/{episodeId}/due-status` |
| **Dependency** | `FE-RWI-051`, `BE-RWI-058` |
| **UI Contract** | [`skema-tampilan-keperawatan-rawat-inap.md`](../skema-tampilan-keperawatan-rawat-inap.md) Bagian 13 (Lini Masa Pengkajian) |
| **Layout** | **Visual Vertical Timeline Card:**<br>1. **Filter Header:** Filter chips `[Semua]`, `[Nyeri]`, `[Risiko Jatuh]`, `[Gizi]` + badge status tenggat.<br>2. **Timeline Track:** Garis waktu vertikal terurut kronologis menurun (waktu terbaru di posisi paling atas).<br>3. **Timeline Node Card:** Tiap kartu memuat: penanda waktu pengkajian, jenis pengukuran, skor/nilai, nama petugas pengkaji, penanda tren (misal: *"↓ Membaik dari 5"*), dan penanda koreksi jika baris pernah dikoreksi (misal: `[Koreksi #1]`). |
| **Existing Base Components** | `src/components/ui/doctor-clinical-base/ClinicalStatusBadge.jsx` |
| **Base Components To Extend** | Pola data fetching & filter bar Quilvian |
| **New Base Components** | `src/components/ui/clinical-workspace/`:<br>- `ClinicalTimeline.jsx`<br>- `ClinicalTimelineItem.jsx`<br>- `ClinicalDocumentMeta.jsx` |
| **Domain Components** | `src/components/view/health-services/inpatient-management/nursing-workspace/`:<br>- `sections/timeline/assessment-timeline-section.jsx`<br>- `sections/timeline/timeline-filter-bar.jsx`<br>- `sections/timeline/timeline-measurement-card.jsx` |
| **State Handling** | - **Loading:** Skeleton vertical timeline cards.<br>- **Empty (Belum Ada Pengkajian):** Pesan ramah *"Belum ada pengkajian untuk pasien ini"* + tombol `[Buat Pengkajian]` (jika berwenang).<br>- **Error (Gagal Memuat):** Pesan *"Lini masa pengkajian tidak dapat dimuat"* + tombol `[Coba Lagi]`.<br>- **No Policy (Batas Waktu Belum Ditetapkan):** Indikator status menyatakan *"○ Batas waktu belum ditetapkan"*, timeline tetap tampil normal.<br>- **PENTING:** KETIGA STATE DI ATAS WAJIB DITAMPILKAN BERBEDA SECARA VISUAL DAN SEMANTIK. DILARANG MEMAKAI EMPTY STATE YANG SAMA! |
| **Permission Behaviour** | - Membaca lini masa: `PatientAssessment : Read` (Perawat, Kepala Ruangan, DPJP, Ahli Gizi).<br>- DPJP dan Ahli Gizi: Mode baca mutlak, tidak ada tombol buat/koreksi. |
| **Episode State Behaviour** | Lini masa tetap dapat dibaca secara penuh baik saat episode `Admitted` maupun saat episode sudah `Closed`. |
| **Responsive Behaviour** | - **Desktop & Tablet:** Garis waktu vertikal dengan margin lebar, kartu pengukuran luas, dan teks metadata lengkap.<br>- **Mobile:** Garis waktu vertikal rapat; filter chips scroll horizontal; ukuran badge menyesuaikan layar sempit. |
| **Scope** | Lini masa per jenis pengukuran: nyeri, risiko jatuh, skrining gizi; filter jenis pengukuran; penanda keadaan tenggat; penanda koreksi pada baris yang pernah dikoreksi beserta nomor urut addendum-nya; tiga keadaan khusus terbedakan. |
| **Acceptance Criteria** | 1. Seluruh pengukuran tampil **terurut waktu**, bukan hanya yang terakhir.<br>2. Nilai lama **tidak pernah** hilang atau tertimpa dari lini masa saat ada pengkajian baru (`AC-CAP012-02`).<br>3. Keadaan khusus **membedakan secara mutlak**: (a) "belum ada pengkajian", (b) "data tidak dapat dimuat", dan (c) "batas waktu belum ditetapkan".<br>4. Baris yang pernah dikoreksi membawa penanda koreksi beserta nomor urutnya, dan isi aslinya tetap tampil di atasnya. |
| **Visual Acceptance Criteria** | 1. Visual utama berupa timeline berbasis card vertikal kronologis, bukan tabel administratif kaku.<br>2. Tren perubahan nilai (membaik, memburuk, stabil) divisualisasikan dengan simbol atau aksen warna yang kontras dan harmonis.<br>3. Filter chip memiliki visual active/selected state yang tegas.<br>4. Penanda koreksi tampil menonjol dengan badge khusus (misal badge kuning bertuliskan `[Koreksi #1]`). |
| **Verification** | Skenario tiga pengukuran nyeri berurutan; pengujian filter Nyeri, Jatuh, dan Gizi; verifikasi nilai historis tetap utuh; skenario master kebijakan kosong; simulasi kegagalan koneksi; skenario data yang memiliki addendum koreksi. |
| **Risk / Blocker** | Menyamakan "kosong" dengan "gagal" menyesatkan klinisi (yang pertama berarti pasien belum diperiksa, yang kedua berarti data ada tapi sistem gagal baca). Tindakannya berlawanan secara klinis. Owner: Frontend Authority. |
| **DoD** | `ClinicalTimeline` dan `ClinicalTimelineItem` terpasang di `src/components/ui/clinical-workspace/`; filter aktif; tiga keadaan terbedakan secara visual; seluruh AC dan Visual AC lolos; `npm run lint` lulus; `npm run build` lulus. |

---

### `FE-RWI-054` — Perawat Menetapkan Masalah, Tujuan, Evaluasi, dan Memantau Riwayat Versi Asuhan

| Field | Spesifikasi & Kontrak |
| --- | --- |
| **Task ID** | `FE-RWI-054` |
| **Status** | Belum dikerjakan. **Tidak terblokir**; menunggu backend `BE-RWI-059` dan `BE-RWI-060` |
| **Outcome** | Rencana asuhan keperawatan tersusun di sistem dalam pola Master-Detail beserta riwayat versi perubahannya (*Version History*), menggantikan catatan kertas tanpa mencampuradukkannya dengan addendum koreksi |
| **Trace** | `FE-KEP-04`; `FR-KEP-012` s.d. `FR-KEP-017`; `RWI-AC-177`; `03-frontend-architecture.md` bagian 3.4; `skema-tampilan-keperawatan-rawat-inap.md` Bagian 14, 15, 16, 24, 25, 27 |
| **Kontrak** | `contracts/api-contract.md` `0.3.0` grup Nursing Care Plan (tujuh endpoint: `GET /episodes/{id}`, `POST /items`, `PUT /items/{id}`, `POST /items/{id}/evaluations`, `PATCH /items/{id}/close`, `PATCH /items/{id}/reopen`, `GET /items/{id}/revisions`) |
| **Dependency** | `FE-RWI-051`, `BE-RWI-059`, `BE-RWI-060` |
| **UI Contract** | [`skema-tampilan-keperawatan-rawat-inap.md`](../skema-tampilan-keperawatan-rawat-inap.md) Bagian 14 (Master-Detail), Bagian 15 (Riwayat Rencana Asuhan), Bagian 16 (Episode Closed) |
| **Layout** | **Pola Master-Detail 2 Panel Berdampingan:**<br>1. **Kiri (Master Panel):** Daftar Masalah Keperawatan berstatus `ACTIVE`, `RESOLVED`, `DISCONTINUED` + Tombol `[+ Tambah Masalah]`.<br>2. **Kanan (Detail Panel):** Detail Masalah aktif memuat Masalah, Tujuan, Rencana Intervensi, Evaluasi Terkini, dan Status.<br>3. **Bawah Detail:** Panel Riwayat Perubahan Versi (`ClinicalRevisionHistory`) menyajikan kronologi perkembangan versi dokumen.<br>4. **Aksi:** Tombol `[Ubah]`, `[Tambah Evaluasi]`, dan `[Nyatakan Tercapai]`. |
| **Existing Base Components** | `src/components/ui/form-pemeriksaan-ui/BaseModal.jsx`, `BaseTextField.jsx`, `BaseTextareaField.jsx`, `BaseSelectField.jsx`; `src/components/ui/doctor-clinical-base/ClinicalStatusBadge.jsx` |
| **Base Components To Extend** | Pola daftar induk-anak yang dipakai layar penempatan tempat tidur rawat inap |
| **New Base Components** | `src/components/ui/clinical-workspace/`:<br>- `ClinicalRevisionHistory.jsx`<br>- `ClinicalActionGuard.jsx` |
| **Domain Components** | `src/components/view/health-services/inpatient-management/nursing-workspace/`:<br>- `sections/care-plan/care-plan-section.jsx`<br>- `sections/care-plan/care-plan-master-list.jsx`<br>- `sections/care-plan/care-plan-detail-view.jsx`<br>- `modals/care-plan-item-modal.jsx`<br>- `modals/care-plan-evaluation-modal.jsx`<br>- `modals/close-care-plan-item-modal.jsx` |
| **State Handling** | - **Loading:** Skeleton pada panel master dan panel detail.<br>- **Empty:** Pesan *"Belum ada masalah keperawatan"* + tombol `[+ Tambah Masalah]`.<br>- **Error:** Banner error gagal memuat data asuhan + `[Coba Lagi]`.<br>- **Evaluasi Belum Ada (Action Guard):** Tombol `[Nyatakan Tercapai]` **DISABLED / TIDAK DAPAT DIKLIK**, disertai tooltip/keterangan: *"Masalah asuhan belum dapat dinyatakan tercapai sebelum evaluasi dicatat"*.<br>- **Episode Closed (CRITICAL):** Tampil banner *"EPISODE TELAH DITUTUP. Rencana asuhan hanya dapat dibaca."* Seluruh tombol aksi mutasi (`Tambah Masalah`, `Ubah`, `Tambah Evaluasi`, `Nyatakan Tercapai`) **DISEMBUNYIKAN (HIDDEN)**. Seluruh riwayat versi tetap dapat dibaca. |
| **Permission Behaviour** | - Membaca rencana asuhan: `NursingCarePlan : Read` (Perawat, Kepala Ruangan, DPJP).<br>- Menambah, mengubah, mengevaluasi: `NursingCarePlan : Create` / `Update` (Perawat PJ & Kepala Ruangan).<br>- Membuka kembali butir tercapai (`reopen`): HANYA Kepala Ruangan.<br>- DPJP: Akses baca saja (seluruh tombol mutasi tersembunyi). |
| **Episode State Behaviour** | - Episode `Admitted`: Operasi rencana asuhan berjalan penuh.<br>- Episode `Closed`: Seluruh tombol mutasi disembunyikan; layar berstatus hanya-baca permanen. |
| **Responsive Behaviour** | - **Desktop:** 2 panel berdampingan (Master ~35% lebar, Detail ~65% lebar).<br>- **Tablet:** Master ~40%, Detail ~60% atau master list dapat diciutkan (*collapsible*).<br>- **Mobile:** Tampilan berganti antara Master List dan Detail View (klik item membuka tampilan detail satu layar penuh dengan tombol kembali `[← Daftar Masalah]`). |
| **Scope** | Daftar butir masalah; formulir tambah dan ubah butir; dialog evaluasi; dialog tutup butir dengan alasan wajib; panel riwayat versi; keadaan hanya-baca ketika episode `Closed`. |
| **Acceptance Criteria** | 1. Butir dapat dinyatakan tercapai **hanya** bila evaluasinya sudah diisi; tombolnya nonaktif sebelum itu, beserta keterangan alasan penonaktifannya.<br>2. Riwayat versi menampilkan **penulis dan waktu asli** tiap versi, bukan penulis yang mengubah (`AC-CAP013-02`).<br>3. Perubahan butir tampil sebagai **versi baru (Version History)**, **BUKAN** sebagai addendum koreksi kesalahan.<br>4. Pada episode `Closed`, seluruh tombol ubah/tambah hilang dan riwayat tetap terbaca.<br>5. Input masalah keperawatan tidak dikunci ke katalog yang belum diputuskan (input teks terstruktur). |
| **Visual Acceptance Criteria** | 1. Layout Master-Detail terbagi tegas dengan scrolling mandiri pada masing-masing panel.<br>2. Status `ACTIVE`, `RESOLVED`, dan `DISCONTINUED` memiliki badge pembeda visual yang tegas.<br>3. Panel `ClinicalRevisionHistory` menyajikan kartu riwayat terurut versi terbaru (`CURRENT`) ke versi terlama.<br>4. Tooltip atau teks penjelasan saat tombol `[Nyatakan Tercapai]` dinonaktifkan tampil elegan dan informatif. |
| **Verification** | Skenario tutup butir tanpa evaluasi (verifikasi tombol disabled); penambahan evaluasi lalu penutupan butir; skenario perubahan rencana dan verifikasi lahir versi baru; verifikasi penulis asli pada riwayat versi; pengujian pada episode Closed; pemeriksaan bahwa layar tidak memanggil endpoint addendum. |
| **Risk / Blocker** | Mengubah rencana asuhan menjadi addendum mengaburkan batas antara perkembangan klinis pasien dan kesalahan administrasi. Masalah keperawatan ditulis teks bebas sampai katalog SDKI diputuskan. Owner: Clinical Governance & Frontend Authority. |
| **DoD** | Layout Master-Detail terpasang; `ClinicalRevisionHistory` terimplementasi di `src/components/ui/clinical-workspace/`; tiga modal berfungsi; seluruh AC dan Visual AC lolos uji; `npm run lint` lulus; `npm run build` lulus. |

---

### `FE-RWI-055` — Perawat Mencatat Tindakan Keperawatan dan Mengisolasi Kegagalan Tagihan

| Field | Spesifikasi & Kontrak |
| --- | --- |
| **Task ID** | `FE-RWI-055` |
| **Status** | Belum dikerjakan. **Tidak terblokir**; menunggu backend `BE-RWI-061` dan `BE-RWI-062` |
| **Outcome** | Tindakan keperawatan tercatat beserta waktu riil pelaksanaan, pelaku, dan hasilnya; tindakan cito dapat dicatat tanpa rencana asuhan; catatan final dikoreksi lewat addendum beralasan; dan kegagalan tagihan billing terlihat sebagai keadaan tersendiri tanpa merusak catatan klinis |
| **Trace** | `FE-KEP-05`; `FR-KEP-018` s.d. `FR-KEP-022`; `AC-CAP014-01`, `AC-CAP014-02`, `AC-CAP014-03`; `RWI-AC-176`; `03-frontend-architecture.md` bagian 3.5; `skema-tampilan-keperawatan-rawat-inap.md` Bagian 17, 18, 19, 20, 24, 25, 27 |
| **Kontrak** | `contracts/api-contract.md` `0.3.0` grup Nursing Intervention (enam endpoint: `GET /episodes/{id}`, `POST /`, `PATCH /{id}/finalize`, `GET /{id}/billing-dispatch`, `POST /{id}/addendums`, `GET /{id}/addendums`); `contracts/state-transition-matrix.md` Bagian 3 & 3.1 |
| **Dependency** | `FE-RWI-051`, `BE-RWI-061`, `BE-RWI-062` |
| **UI Contract** | [`skema-tampilan-keperawatan-rawat-inap.md`](../skema-tampilan-keperawatan-rawat-inap.md) Bagian 17 (Chronological timeline tindakan), Bagian 18 (Form tindakan), Bagian 19 (Isolasi billing failure), Bagian 20 (Koreksi addendum) |
| **Layout** | **Chronological Vertical Timeline Tindakan:**<br>1. **Header:** Judul *"Catatan Tindakan Keperawatan"* + Tombol `[+ Catat Tindakan]`.<br>2. **Timeline List:** Terurut menurun berdasarkan **WAKTU TINDAKAN** (bukan waktu pencatatan).<br>3. **Timeline Card:** Tiap baris memuat: Waktu tindakan, nama tindakan, perawat pelaku, teks hasil tindakan, tautan Rencana Asuhan (atau *"Tidak ditautkan"*), status klinis (`✓ FINAL`), status tagihan (`✓ Terkirim` / `⚠ TAGIHAN BELUM TERKIRIM`), dan tombol `[Tambah Koreksi]`.<br>4. **Form Modal:** Modal catat tindakan memuat field Tindakan *, Waktu tindakan *, Hasil, dan dropdown Rencana Asuhan Terkait (**OPSIONAL**). |
| **Existing Base Components** | `src/components/ui/form-pemeriksaan-ui/BaseModal.jsx`, `BaseTextField.jsx`, `BaseTextareaField.jsx`, `BaseSelectField.jsx`; `src/components/ui/doctor-clinical-base/ClinicalStatusBadge.jsx` |
| **Base Components To Extend** | Memanfaatkan `ClinicalTimeline` dan `ClinicalTimelineItem` dari `src/components/ui/clinical-workspace/` |
| **New Base Components** | `src/components/ui/clinical-workspace/ClinicalAddendumList.jsx` & `ClinicalAddendumItem.jsx` (dipakai bersama dengan `FE-RWI-052`) |
| **Domain Components** | `src/components/view/health-services/inpatient-management/nursing-workspace/`:<br>- `sections/intervention/nursing-intervention-section.jsx`<br>- `sections/intervention/intervention-timeline-item.jsx`<br>- `modals/nursing-intervention-modal.jsx`<br>- `modals/add-correction-modal.jsx` |
| **State Handling** | - **Loading:** Skeleton timeline tindakan.<br>- **Empty:** Pesan *"Belum ada tindakan tercatat hari ini"* + tombol `[+ Catat Tindakan]`.<br>- **Double Submit:** Tombol simpan dinonaktifkan seketika saat diklik; request menyertakan header/kunci idempotency.<br>- **Finalized:** Catatan final **TIDAK MENAMPILKAN TOMBOL [SUNTING]**. Tombol `[Tambah Koreksi]` aktif bagi yang berwenang.<br>- **Billing Dispatch Failure (CRITICAL STATE):** Jika pencatatan klinis sukses tetapi dispatch Billing gagal, CATATAN KLINIS TETAP TAMPIL NORMAL DENGAN BADGE *"✓ Catatan tersimpan"* dan penanda terpisah *"⚠ TAGIHAN BELUM TERKIRIM"*. **DILARANG KERAS MENAMPILKAN FULL-PAGE ERROR ATAU MENYATAKAN 'GAGAL MENYIMPAN TINDAKAN'**.<br>- **Episode Closed:** Tombol catat tindakan dan tombol koreksi disembunyikan; seluruh catatan hanya dapat dibaca. |
| **Permission Behaviour** | - Mencatat tindakan: `NursingIntervention : Create` (Perawat pelaksana mana pun di unit).<br>- Finalisasi: Penulis catatan atau Kepala Ruangan.<br>- Menambah Koreksi: HANYA penulis asli catatan ATAU Kepala Ruangan (`AC-CAP014-03`). Pengguna lain dan DPJP tidak memiliki tombol koreksi.<br>- DPJP: Akses baca saja. |
| **Episode State Behaviour** | - Episode `Admitted`: Dapat mencatat tindakan, memfinalkan, dan mengoreksi.<br>- Episode `Closed`: Seluruh aksi mutasi disembunyikan; riwayat tindakan tetap dapat dibaca. |
| **Responsive Behaviour** | - **Desktop & Tablet:** Timeline vertikal dengan badge status klinis dan billing sejajar di sisi kanan baris tindakan.<br>- **Mobile:** Timeline rapat, badge klinis dan billing tertumpuk rapi di bawah rincian tindakan. Modal form responsif memenuhi layar penuh. |
| **Scope** | Formulir catat tindakan; daftar tindakan per episode terurut waktu tindakan, bukan waktu pencatatan; tombol Finalkan; dialog tambah koreksi beralasan; daftar koreksi bernomor; penanda keadaan pengiriman tagihan; penanganan isolasi kegagalan billing. |
| **Acceptance Criteria** | 1. Menekan tombol simpan dua kali menghasilkan **satu** baris tindakan; tombol dinonaktifkan selama permintaan berjalan.<br>2. Tindakan mendadak dapat dicatat **tanpa** memilih butir rencana asuhan (kolom rencana asuhan opsional).<br>3. Ketika pengiriman tagihan gagal, catatan klinis **tetap tampil normal** dan penanda kegagalannya terpisah — **bukan** sebagai galat halaman atau kegagalan simpan klinis (`AC-CAP014-02`).<br>4. Bagi pengguna yang bukan penulis dan bukan kepala ruangan, tombol tambah koreksi **tidak tersedia** (`AC-CAP014-03`).<br>5. Catatan yang sudah final tidak menampilkan tombol sunting langsung. |
| **Visual Acceptance Criteria** | 1. Timeline terurut kronologis rapi berdasarkan waktu pelaksanaan tindakan.<br>2. Status kegagalan tagihan tersaji sebagai penanda baris/item badge yang jelas tanpa mengganggu status sukses klinis (misal: *"✓ Catatan tersimpan · ⚠ Tagihan belum terkirim"*).<br>3. Form modal memperjelas bahwa pilihan Rencana Asuhan bersifat opsional.<br>4. Koreksi catatan tindakan tersaji sebagai addendum bernomor di bawah tindakan asli dengan rincian alasan, penulis, dan waktu koreksi. |
| **Verification** | Uji tekan ganda pada tombol simpan; skenario pencatatan tanpa rencana asuhan; simulasi kegagalan sistem Billing (verifikasi catatan klinis tetap ada dan badge tagihan muncul); uji hak akses dengan dua peran berbeda (penulis vs non-penulis); skenario tambah koreksi beralasan. |
| **Risk / Blocker** | Menampilkan kegagalan tagihan sebagai kegagalan klinis membuat perawat mengulang tindakan pada fisik pasien (misal memasang infus dua kali). Ini risiko keselamatan fatal! Owner: Frontend Authority. |
| **DoD** | Form tindakan terpasang; timeline terurut waktu tindakan; isolasi penanda tagihan terbukti; dialog koreksi addendum berfungsi; seluruh AC dan Visual AC lolos uji; `npm run lint` lulus; `npm run build` lulus. |

---

### `FE-RWI-056` — Kepala Ruangan Memantau Kepatuhan Pengkajian pada Layar Daftar Pantau Existing

| Field | Spesifikasi & Kontrak |
| --- | --- |
| **Task ID** | `FE-RWI-056` |
| **Status** | Belum dikerjakan. **Tidak terblokir**; menunggu backend `BE-RWI-064` |
| **Outcome** | Kepala ruangan dan supervisor menemukan pengkajian yang belum dikerjakan atau terlambat melalui daftar ketiga pada layar Daftar Pantau existing (`FE-INP-09`), tanpa membuat rute baru dan tanpa membocorkan isi klinis bebas |
| **Trace** | `FE-KEP-06`; `FR-KEP-024`, `FR-KEP-025`, `FR-KEP-026`; `RWI-RULE-023`, `RWI-DEC-032`; `03-frontend-architecture.md` bagian 3.6; `skema-tampilan-keperawatan-rawat-inap.md` Bagian 21, 22, 25, 27 |
| **Kontrak** | `contracts/api-contract.md` `0.3.0` endpoint daftar pantau kepatuhan pengkajian (`GET /api/v1/health-services/clinical-management/patient-assessments/episodes/{id}/due-status` per episode census) |
| **Dependency** | `BE-RWI-064` |
| **UI Contract** | [`skema-tampilan-keperawatan-rawat-inap.md`](../skema-tampilan-keperawatan-rawat-inap.md) Bagian 21 (Tampilan Daftar Pantau) dan Bagian 22 (State Daftar Pantau) |
| **Layout** | **Data Table Terintegrasi sebagai Daftar Ketiga pada Layar Existing `FE-INP-09` (Daftar Pantau Rawat Inap):**<br>1. **Tab Navigation:** Muncul sebagai tab/daftar ketiga pada `inpatient-monitoring-view.jsx`. DILARANG membuat halaman/route baru.<br>2. **Filter Bar:** Ruangan `[Semua / Kamar]` dan Status Tenggat `[Semua / Belum Ada / Terlambat]`.<br>3. **Data Table Kolom Minimum:** Pasien (Nama, No RM), Kamar/Bed, Status Pengkajian, Tenggat, Status Kepatuhan (Keterlambatan/Sisa Waktu), dan Aksi `[Buka Pasien]` menuju `FE-RWI-051`.<br>4. **PRIVASI KETAT:** DILARANG KERAS MENAMPILKAN ISI KLINIS BEBAS (keluhan, skor nyeri bebas, catatan perawat, diagnosis, dll.). |
| **Existing Base Components** | `src/components/view/health-services/inpatient-management/inpatient-monitoring-view.jsx`, `inpatient-monitoring-table-columns.jsx`; Base Table & Pagination Quilvian |
| **Base Components To Extend** | Memperluas tab navigasi dan kolom tabel pada `inpatient-monitoring-view.jsx` |
| **New Base Components** | Tidak ada (memanfaatkan table base existing modul Rawat Inap) |
| **Domain Components** | `src/components/view/health-services/inpatient-management/monitoring/nursing-assessment-compliance-table.jsx` (atau diekspor langsung dari struktur monitoring inpatient existing) |
| **State Handling** | - **Loading:** Skeleton table rows.<br>- **State A (Tepat Waktu):** *"✓ Seluruh pengkajian sudah tepat waktu."*<br>- **State B (Kebijakan Belum Ditetapkan):** *"○ BATAS WAKTU PENGKAJIAN BELUM DITETAPKAN. Keterlambatan belum dapat dihitung."*<br>- **State C (Error Gagal Memuat):** *"⚠ Data kepatuhan pengkajian tidak dapat dimuat."* + tombol `[Coba Lagi]`.<br>- **PENTING:** STATE A, B, DAN C WAJIB DIBEDAKAN SECARA MUTLAK DI TAMPILAN. STATE B BUKAN STATE A, DAN BUKAN PULA STATE C! |
| **Permission Behaviour** | Membaca daftar pantau: `PatientAssessment : Read` (Kepala Ruangan, Supervisor Keperawatan, Perawat, DPJP). Tidak ada tombol pada daftar pantau ini yang menahan atau memblokir pekerjaan klinis. |
| **Episode State Behaviour** | Hanya menampilkan episode rawat inap yang sedang aktif (`Admitted`) yang membutuhkan pemantauan kepatuhan pengkajian. |
| **Responsive Behaviour** | - **Desktop:** Tabel penuh dengan filter dropdown di atasnya.<br>- **Tablet:** Tabel dengan scroll horizontal lancar dan kolom aksi tetap terjangkau.<br>- **Mobile:** Tabel beralih menjadi daftar kartu kepatuhan ringkas per pasien (Nama, Kamar, Status Keterlambatan, Tombol Buka Ruang Kerja). |
| **Scope** | Satu daftar tambahan pada `FE-INP-09`; penyaringan ruangan dan keadaan tenggat; pembedaan tiga bunyi keadaan khusus; setiap baris membuka ruang kerja keperawatan pasien terkait (`FE-RWI-051`); penegakan aturan privasi klinis. |
| **Acceptance Criteria** | 1. Daftar muncul sebagai daftar **ketiga** pada `FE-INP-09`, bukan sebagai layar atau butir menu baru.<br>2. Daftar kosong berbunyi **"Seluruh pengkajian sudah tepat waktu"**, bukan "tidak ada data".<br>3. Ketika kebijakan belum diisi, daftar berbunyi **"Batas waktu pengkajian belum ditetapkan, sehingga keterlambatan belum dapat dihitung"** — dan itu **berbeda** dari kriteria 2.<br>4. Layar tercapai dari Beranda dalam **dua klik** (`IA-INP-01`).<br>5. Tidak ada tombol pada daftar ini yang menahan atau memblokir pekerjaan klinis mana pun.<br>6. Daftar **tidak** menampilkan isi klinis bebas, hanya nama pasien, lokasi, tenggat, dan keterlambatan. |
| **Visual Acceptance Criteria** | 1. Tampil menyatu secara visual sebagai tab ketiga yang serasi pada layar `FE-INP-09` existing.<br>2. Kolom keterlambatan memiliki indikator visual kontras (misal badge merah lembut untuk terlambat, kuning untuk mendekati tenggat).<br>3. Tampilan tabel bersih tanpa tumpukan teks catatan klinis.<br>4. Visual state A (tepat waktu), state B (kebijakan belum ada), dan state C (error) terbedakan secara tegas dan jelas. |
| **Verification** | Telusur dua klik dari Beranda; skenario daftar kosong; skenario master kebijakan kosong; verifikasi urutan tab pada `FE-INP-09`; pengujian klik baris membuka ruang kerja pasien; audit inspeksi elemen bahwa tidak ada catatan klinis bebas yang lolos ke DOM/console. |
| **Risk / Blocker** | Urutan daftar di dalam `FE-INP-09` dipakai tiga sub-modul dan tidak boleh diputuskan sepihak. Menampilkan teks klinis melanggar aturan privasi pasien. Owner: Frontend Authority bersama pemilik `episode-rawat-inap`. |
| **DoD** | Tab ketiga terpasang pada `FE-INP-09`; filter ruangan dan status berfungsi; tiga state terbukti terpisah; seluruh AC dan Visual AC lolos uji; `npm run lint` lulus; `npm run build` lulus. |

---

## 3.1 Register Status Task

| Task | Layar | Gelombang | Status | Laporan |
| --- | --- | --- | :---: | --- |
| `FE-RWI-051` | `FE-KEP-01` Ruang Kerja Keperawatan | `KEP-MVP-1` | tanpa tanda | — |
| `FE-RWI-052` | `FE-KEP-02` Pengkajian Keperawatan | `KEP-MVP-1` | tanpa tanda | — |
| `FE-RWI-053` | `FE-KEP-03` Lini Masa Pengkajian | `KEP-MVP-1` | tanpa tanda | — |
| `FE-RWI-054` | `FE-KEP-04` Rencana Asuhan | `KEP-MVP-2` | tanpa tanda | — |
| `FE-RWI-055` | `FE-KEP-05` Catatan Tindakan | `KEP-MVP-3` | tanpa tanda | — |
| `FE-RWI-056` | `FE-KEP-06` Daftar Pantau Kepatuhan | `KEP-MVP-4` | tanpa tanda | — |

Laporan task wajib ditulis ke `<blueprint-root>/task/report/frontend/<TASK-ID>.md` sesuai
`rules/rule-output/lokasi-laporan-task.md`.

---

## 4. Ketergantungan Test dan Satu Gerbang yang Tetap Terbuka

| Yang dibutuhkan | Kenapa | Keadaan |
| --- | --- | --- |
| Episode berstatus `Admitted` beserta perawat penanggung jawabnya | Seluruh layar butuh konteks | Tersedia lewat `episode-rawat-inap` |
| Sekurang-kurangnya satu baris master kebijakan batas waktu, **dan** satu skenario tanpa baris itu sama sekali | Menguji penanda tenggat pada kedua keadaan — `VAL-KEP-17` | Menyusul bersama `BE-RWI-055` |
| Peran perawat pelaksana, kepala ruangan, dan DPJP terpisah | Menguji `AC-CAP014-03` dan kolom baca-saja DPJP | Perlu disiapkan saat eksekusi |
| **Data master rawat inap yang layak — `RWI-UI-GAP-007`** | Uji ujung ke ujung yang bermakna | **Masih terbuka.** Menahan **uji e2e yang bermakna**, bukan pembangunan layar. Owner: pemilik data master bersama `RWI-DEC-063`/`BE-RWI-002` |

`RWI-UI-GAP-007` sengaja **tidak** dijadikan blocker task mana pun. Layar dibangun, dan butir DoD yang
menuntut e2e dinyatakan belum terpenuhi apa adanya sampai gap tertutup.

---

## 5. Coverage Gap Requirement ke Test

| Requirement | Layar pemilik | Task | Catatan |
| --- | --- | --- | --- |
| `FR-KEP-001` s.d. `FR-KEP-004` | `FE-KEP-01` | `FE-RWI-051` | Pintu masuk; dibuktikan telusur tiga klik |
| `FR-KEP-005`, `FR-KEP-006`, `FR-KEP-008`, `FR-KEP-009` | `FE-KEP-02` | `FE-RWI-052` | Lengkap, termasuk 7 kelompok isian dan bentuk koreksi addendum |
| `FR-KEP-007`, `FR-KEP-010`, `FR-KEP-011` | `FE-KEP-03` | `FE-RWI-053` | Lengkap, visual timeline card terurut kronologis |
| `FR-KEP-012` s.d. `FR-KEP-017` | `FE-KEP-04` | `FE-RWI-054` | Lengkap, Master-Detail dan Version History |
| `FR-KEP-018` s.d. `FR-KEP-023` | `FE-KEP-05` | `FE-RWI-055` | `FR-KEP-023` catatan terpadu; isolasi status tagihan billing |
| `FR-KEP-024` s.d. `FR-KEP-026` | `FE-KEP-06` | `FE-RWI-056` | Lengkap, daftar ketiga pada `FE-INP-09` |
| `FR-KEP-027`, `FR-KEP-028` | — | **Tidak ada, disengaja** | `EPIC KEP-06` `DEFERRED` oleh `RWI-DEC-089` |

---

## 6. Yang Sengaja Tidak Dibuat Roadmap Ini

| Yang ditolak | Alasan |
| --- | --- |
| Butir menu tingkat dua untuk sub-modul ini | Kuota sembilan menu `IA-INP-05` sudah penuh; perawat bekerja pada konteks satu pasien |
| Layar baru untuk daftar pantau kepatuhan | `FE-INP-09` sudah ada dan memuat dua daftar; kepatuhan pengkajian menjadi daftar ketiga |
| Layar cetak | Tidak ada kebutuhan cetak mandiri pada sub-modul ini — `03-frontend-architecture.md` bagian 6 |
| Layar pemakaian alat | `EPIC KEP-06` `DEFERRED` oleh `RWI-DEC-089` |
| Layar asuhan gizi mandiri | Modul Gizi `PLANNED`; yang ada hanya hasil skrining pada pengkajian keperawatan |
| Dialog "amandemen" yang menghasilkan versi baru pada pengkajian & tindakan | Dicabut `RWI-DEC-091`. Pengkajian & tindakan final memakai koreksi beraddendum |
| Menampilkan isi klinis bebas pada daftar pantau maupun tooltip | `03-frontend-architecture.md` bagian 6: catatan klinis bebas hanya tampil pada layar detail |
| Tiga basis komponen clinical workspace terpisah per domain | `skema-tampilan-keperawatan-rawat-inap.md`: basis komponen klinis harus netral domain |
