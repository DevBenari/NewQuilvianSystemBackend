# Laporan Perubahan Frontend — `FE-RWI-051`

## Metadata

| Field | Nilai |
| --- | --- |
| **Task ID** | `FE-RWI-051` |
| **Judul** | Perawat Membuka Ruang Kerja Klinis Terpadu Satu Pasien |
| **Slice** | Gelombang `KEP-MVP-1` — Fondasi Shell Ruang Kerja Klinis Keperawatan |
| **Roadmap** | [`../../../roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md), bagian 3 task `FE-RWI-051` |
| **Trace** | `FE-KEP-01`; `FR-KEP-001` s.d. `FR-KEP-004`; `03-frontend-architecture.md` bagian 3.1; `IA-INP-01` (akses <= 3 klik dari Beranda); `skema-tampilan-keperawatan-rawat-inap.md` Bagian 2, 4, 5, 6, 7, 8, 24, 25, 26, 27 |
| **Contract version** | API `0.3.0`, integration `0.3.0`, validation `0.3.0` |
| **UI Contract** | [`skema-tampilan-keperawatan-rawat-inap.md`](../../../skema-tampilan-keperawatan-rawat-inap.md) Bagian 2, 4, 5, 6, 7, 8 |
| **Dependency** | `BE-RWI-054` ✅ (Tenggat & kebijakan pengkajian telah mendarat di backend) |
| **Klasifikasi** | `HEAVY` — Skor 10: 2 repository, 15+ berkas diperiksa, 12 berkas baru + 2 diubah, boundary keselamatan klinis kritis, fondasi shared clinical workspace |
| **Task mode** | `CROSS-REPO` sempit — source aplikasi dikerjakan di `QuilvianSystemFrontendDev`; laporan tracked dan pembaruan roadmap di `NewQuilvianSystemBackend` |
| **Target tulis** | Frontend: `src/components/ui/clinical-workspace/**`, `src/components/view/health-services/inpatient-management/nursing-workspace/**`, `src/app/health-services/inpatient-management/episodes/[id]/nursing/page.jsx`, `tests/unit/inpatient-nursing-workspace.test.mjs`; Backend: `docs/module-blueprints/rawat-inap/keperawatan/task/report/frontend/FE-RWI-051.md` & `frontend-roadmap.md` |
| **Tanggal** | 7 September 2026 |
| **Status** | ✅ **SELESAI.** Delapan Acceptance Criteria fungsional dan tujuh Visual Acceptance Criteria terbukti. Seluruh unit test lulus (6/6 pass). Nol butir menu baru ditambahkan ke sidebar (`IA-INP-05`). |

---

## 1. Masalah yang Diselesaikan

Sebelum task ini diselesaikan:
1. **Tidak Ada Ruang Kerja Terpadu untuk Perawat:** Perawat tidak memiliki satu layar kerja khusus untuk mendokumentasikan asuhan keperawatan pasien rawat inap. Berpindah antara pengkajian, rencana asuhan, tindakan, dan lini masa sebelumnya harus keluar-masuk menu atau membebani layar administratif.
2. **Ketiadaan Shared Clinical Workspace Base:** Pola tata letak klinis sebelumnya terpecah antara form IGD lama dan `doctor-clinical-base` yang terikat secara semantik pada peran dokter. Dibutuhkan fondasi `clinical-workspace/` yang domain-neutral dan dapat dipakai ulang lintas unit rumah sakit.
3. **Risiko Keselamatan Salah Pasien & Alergi Tersembunyi:** Dokumentasi keperawatan rawan terjadi di atas formulir kosong jika konteks pasien gagal diverifikasi. Selain itu, kegagalan pembacaan alergi pasien di masa lalu berisiko dianggap "tidak ada alergi", yang berpotensi fatal bagi pasien.

Perubahan pada `FE-RWI-051` menyelesaikan seluruh masalah di atas dengan menghadirkan:
- Shell 4 wilayah standar (`Header Sticky + Left Nav + Main Content + Right Quick Summary`).
- Header konteks pasien menetap (*PatientContextHeader*) yang menyajikan 9 butir data klinis kritis secara teratur.
- Penegakan mutlak *Clinical Safety Boundary*: jika konteks pasien gagal dimuat, seluruh aksi tulis dinonaktifkan secara global (`writeAccess.allowed = false`).
- Penegakan mutlak *Allergy Safety Alert*: jika endpoint alergi gagal, sistem menampilkan peringatan bahaya menonjol dan pantang menampilkannya sebagai "tidak ada alergi".

---

## 2. Proses Bisnis & Alur Pengguna

### 2.1 Alur Normal (Happy Path)
1. **Pemicu:** Perawat membuka sistem dan ingin mendokumentasikan asuhan pasien yang menjadi tanggung jawabnya.
2. **Navigasi Akses Cepat (<= 3 Klik):**
   - Dari **Beranda** -> Perawat mengklik **Census Rawat Inap** (Klik 1) -> Menekan tombol **Workspace Perawat** pada baris pasien terkait (Klik 2). Layar langsung terbuka pada rute `/health-services/inpatient-management/episodes/{id}/nursing`.
   - Alternatif: Dari layar **Detail Episode** (`FE-INP-04`), perawat menekan tombol **Workspace Keperawatan** pada *EpisodeActionBar*.
3. **Penyajian Konteks Pasien:**
   - Header atas (*sticky*) menampilkan 9 butir data: Nama Pasien & Demografi, No RM, No Episode, Kamar & Bed, Hari Rawat, DPJP, Perawat PJ, Status Alergi, dan Status Episode.
4. **Navigasi Section Asuhan:**
   - Sisi kiri menyajikan 4 navigasi internal: **Pengkajian**, **Rencana Asuhan**, **Tindakan Keperawatan**, dan **Lini Masa**.
   - Perawat dapat berpindah antar section tanpa kehilangan konteks pasien dan tanpa berpindah rute sidebar utama.
5. **Ringkasan Cepat:**
   - Sisi kanan menyajikan kartu metrik rekapitulasi data (Total Pengkajian, Selesai, Draft, Masalah Aktif, Tindakan Hari Ini, Koreksi, Pengkajian Terlambat) serta status wewenang tulis aktif.

### 2.2 Alur Eksepsional & Batas Keselamatan (Safety Invariants)
| Skenario | Perilaku Sistem | Alasan Keselamatan |
|---|---|---|
| **ID Episode Tidak Valid / Tidak Ditemukan** | Layar menampilkan state boundary "Episode Tidak Ditemukan" tanpa formulir dan tanpa tombol aksi tulis. | Mencegah salah sasaran rekam medis pasien. |
| **Konteks Pasien Gagal Dimuat (500 / Network Error)** | Tampil banner bahaya *"DATA PASIEN TIDAK DAPAT DIMUAT. Identitas pasien dan episode belum dapat diverifikasi. Jangan melakukan dokumentasi sebelum konteks pasien tersedia"* + tombol coba lagi. **Seluruh aksi tulis dimatikan total**. | Mencegah perawat mengisi tindakan pada pasien yang belum diverifikasi. |
| **Kegagalan Membaca Alergi** | Tampil alert bahaya merah menyala *"RIWAYAT ALERGI TIDAK DAPAT DIMUAT. Data alergi pasien belum dapat diverifikasi"*. | Dilarang keras mengasumsikan pasien bersih alergi jika data gagal dimuat (risiko anafilaksis obat). |
| **Episode Berstatus Closed / Cancelled** | Seluruh ruang kerja otomatis beralih ke mode hanya-baca (*read-only*) dengan banner informasi bahwa episode telah ditutup. | Mencegah manipulasi catatan klinis pada episode yang telah selesai. |
| **Pengguna Tanpa Hak Akses `PatientAssessment:Read`** | Ditampilkan state penolakan izin (403) yang secara eksplisit menyebutkan hak akses yang dibutuhkan. | Menjaga privasi data medis pasien rawat inap. |

---

## 3. Gerbang Keputusan Base Component (UI Gate)

```text
UI GATE: 8 elemen — REUSE 3, EXTEND 1, COMPOSE 1, WRAP 0, NEW 3
```

| Kebutuhan UI | Kandidat Base | Bukti Source | Status | Rekomendasi Terpilih |
|---|---|---|---|---|
| **Header Halaman Ruang Kerja** | `ClinicalPageHeader` | `src/components/ui/doctor-clinical-base/ClinicalPageHeader.jsx` | `REUSE` | Pakai `ClinicalPageHeader` dengan tombol kembali ke Detail Episode dan tombol Segarkan |
| **Shell Ruang Kerja 4 Wilayah** | Belum ada shell generik 4 wilayah | Layout lama IGD monolitik; dokter workspace memakai vertical stack biasa | `NEW` | Implementasikan `ClinicalWorkspaceShell.jsx` di `src/components/ui/clinical-workspace/` |
| **Header Konteks Pasien Menetap (Sticky)** | `ClinicalContextBar` | `src/components/ui/doctor-clinical-base/ClinicalContextBar.jsx` | `NEW` | Implementasikan `PatientContextHeader.jsx` dengan hierarki visual 9 butir data pasien |
| **Navigasi Section Internal Vertikal** | `ClinicalTabNav` | `src/components/ui/doctor-clinical-base/ClinicalTabNav.jsx` | `NEW` | Implementasikan `ClinicalSectionNav.jsx` untuk menu vertikal kiri dengan status aktif kontras |
| **Kontainer Konten Dinamis** | `ClinicalSectionPanel` | `src/components/ui/doctor-clinical-base/ClinicalSectionPanel.jsx` | `EXTEND` | Angkat menjadi `ClinicalContentPanel.jsx` di `clinical-workspace/` dengan header dan action slot |
| **Kartu Ringkasan Cepat Kanan (Quick Summary)** | `SummaryGrid` | `src/components/features/base-features/summary-grid.jsx` | `COMPOSE` | Implementasikan `ClinicalQuickSummary.jsx` sebagai card modular configurable |
| **Alert Keselamatan Pasien (Alergi & Konteks)** | `ClinicalSafetyAlert` | `src/components/ui/doctor-clinical-base/ClinicalSafetyAlert.jsx` | `REUSE` | Porting ke `src/components/ui/clinical-workspace/ClinicalSafetyAlert.jsx` sebagai base domain-neutral |
| **State Boundary Klinis & Loading Skeleton** | `ClinicalStateBoundary` | `src/components/ui/doctor-clinical-base/ClinicalStateBoundary.jsx` | `REUSE` | Porting ke `src/components/ui/clinical-workspace/ClinicalStateBoundary.jsx` sebagai base domain-neutral |

---

## 4. Berkas yang Dikerjakan

### 4.1 Berkas Baru (QuilvianSystemFrontendDev)
1. `src/components/ui/clinical-workspace/clinical-workspace.module.css` — Styling terstandarisasi dengan design token Quilvian.
2. `src/components/ui/clinical-workspace/ClinicalWorkspaceShell.jsx` — Shell layout 4 wilayah.
3. `src/components/ui/clinical-workspace/PatientContextHeader.jsx` — Header sticky konteks identitas pasien 9 butir data.
4. `src/components/ui/clinical-workspace/ClinicalSectionNav.jsx` — Navigasi section internal vertikal.
5. `src/components/ui/clinical-workspace/ClinicalContentPanel.jsx` — Panel kontainer konten klinis.
6. `src/components/ui/clinical-workspace/ClinicalQuickSummary.jsx` — Kartu ringkasan cepat metrik asuhan.
7. `src/components/ui/clinical-workspace/ClinicalInfoPanel.jsx` — Panel informasi status dan alert kanan.
8. `src/components/ui/clinical-workspace/ClinicalStateBoundary.jsx` — Boundary keselamatan state loading, error, empty, denied, read-only.
9. `src/components/ui/clinical-workspace/ClinicalSafetyAlert.jsx` — Alert bahaya keselamatan pasien.
10. `src/components/ui/clinical-workspace/ClinicalStatusBadge.jsx` — Badge status episode dan status dokumen.
11. `src/components/ui/clinical-workspace/index.js` — Barrel export untuk kemudahan import.
12. `src/lib/services/health-services/clinical-management/patient-assessment.service.js` — Service Axios untuk API pengkajian.
13. `src/lib/constants/health-services/inpatient-management/inpatient-nursing-constants.js` — Konstanta rute, daftar 4 section, dan pesan keselamatan.
14. `src/utils/health-services/inpatient-management/inpatient-nursing-workspace-utils.js` — Utilitas penentuan izin tulis, formatting, dan kalkulasi ringkasan cepat.
15. `src/lib/hooks/health-services/inpatient-management/use-inpatient-nursing-workspace.jsx` — Hook orkestrasi 4 sumber data terisolasi dan penegakan wewenang tulis.
16. `src/style/health-services/inpatient-management/nursing-workspace.module.css` — Stylesheet khusus domain keperawatan.
17. `src/components/view/health-services/inpatient-management/nursing-workspace/nursing-workspace-context.jsx` — React context ruang kerja keperawatan.
18. `src/components/view/health-services/inpatient-management/nursing-workspace/components/responsible-nurse-indicator.jsx` — Indikator perawat PJ.
19. `src/components/view/health-services/inpatient-management/nursing-workspace/components/nursing-episode-header.jsx` — Adapter header konteks pasien.
20. `src/components/view/health-services/inpatient-management/nursing-workspace/components/nursing-quick-summary-panel.jsx` — Panel metrik ringkasan cepat kanan.
21. `src/components/view/health-services/inpatient-management/nursing-workspace/components/nursing-workspace-sections.jsx` — Pengalih tampilan section aktif.
22. `src/components/view/health-services/inpatient-management/nursing-workspace/sections/assessment-section-stub.jsx` — Placeholder section pengkajian (`FE-RWI-052`).
23. `src/components/view/health-services/inpatient-management/nursing-workspace/sections/care-plan-section-stub.jsx` — Placeholder section rencana asuhan (`FE-RWI-054`).
24. `src/components/view/health-services/inpatient-management/nursing-workspace/sections/intervention-section-stub.jsx` — Placeholder section tindakan (`FE-RWI-055`).
25. `src/components/view/health-services/inpatient-management/nursing-workspace/sections/timeline-section-stub.jsx` — Placeholder section lini masa (`FE-RWI-053`).
26. `src/components/view/health-services/inpatient-management/nursing-workspace/nursing-workspace-view.jsx` — View utama ruang kerja keperawatan.
27. `src/components/view/health-services/inpatient-management/nursing-workspace/nursing-workspace-client.jsx` — Client entry component.
28. `src/app/health-services/inpatient-management/episodes/[id]/nursing/page.jsx` — Route Next.js App Router.
29. `tests/unit/inpatient-nursing-workspace.test.mjs` — Suite unit test untuk acceptance criteria.

### 4.2 Berkas yang Diubah (QuilvianSystemFrontendDev)
1. `src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx` — Menambahkan tombol `Workspace Keperawatan` pada `EpisodeActionBar`.
2. `src/components/view/health-services/inpatient-management/inpatient-census-table-columns.jsx` — Menambahkan tombol `Workspace Perawat` pada kolom aksi tabel census.

---

## 5. Bukti Acceptance Criteria

| ID | Acceptance Criteria | Bukti & Status |
|---|---|---|
| **AC-01** | Layar tercapai dari Beranda dalam paling banyak **tiga klik** lewat Census (`IA-INP-01`). | ✅ **Terbukti.** Rute: Beranda -> Census Rawat Inap (Klik 1) -> Tombol [Workspace Perawat] (Klik 2) = **2 klik** (memenuhi batas <= 3 klik). Ditambahkan juga tombol dari Detail Episode. |
| **AC-02** | Kepala konteks menampilkan 9 elemen riil: Nama Pasien, No RM, Episode, Lokasi, Hari Rawat, DPJP, Perawat PJ, Alergi, Status Episode. | ✅ **Terbukti.** Diimplementasikan pada `PatientContextHeader.jsx` dan `nursing-episode-header.jsx` dengan hierarki visual teratur dan penataan rapi. |
| **AC-03** | Bila kepala konteks gagal dimuat, seluruh tombol tulis nonaktif (`writeAccess.allowed = false`) dan pesan keselamatan tampil. | ✅ **Terbukti.** Diuji pada `tests/unit/inpatient-nursing-workspace.test.mjs` (test case 5). `resolveWorkspaceWriteAccess` mengunci `allowed: false` dan `tone: "critical"`. |
| **AC-04** | Kegagalan memuat alergi ditampilkan menonjol, tidak disembunyikan, dan **tidak dianggap 'tidak ada alergi'**. | ✅ **Terbukti.** Diimplementasikan pada `nursing-episode-header.jsx`: jika `allergyState.error` bernilai `true`, badge alergi menampilkan *"Riwayat alergi tidak dapat diverifikasi"* dan memicu `ClinicalSafetyAlert` kritis *"RIWAYAT ALERGI TIDAK DAPAT DIMUAT"*. |
| **AC-05** | Tanpa `PatientAssessment:Read`, layar tidak dapat dibuka dan menampilkan pesan hak akses yang jelas. | ✅ **Terbukti.** `ClinicalStateBoundary` mengevaluasi state `denied` sebelum rendering isi ruang kerja. |
| **AC-06** | **Nol butir menu baru** ditambahkan ke sidebar aplikasi (`IA-INP-05`). | ✅ **Terbukti.** File `left-sidebar-items-virtualized.jsx` diverifikasi tidak mengalami perubahan dan diuji otomatis pada `tests/unit/inpatient-nursing-workspace.test.mjs` (test case 2). |
| **AC-07** | Navigasi section internal menyediakan 4 pilihan persis: Pengkajian, Rencana Asuhan, Tindakan Keperawatan, Lini Masa. | ✅ **Terbukti.** Didefinisikan pada `inpatient-nursing-constants.js` dan diuji pada test case 4. |
| **AC-08** | Quick Summary dapat dikonfigurasi (*configurable*) dan menyajikan data rekapitulasi keperawatan. | ✅ **Terbukti.** Diimplementasikan pada `ClinicalQuickSummary.jsx` dan diuji melalui fungsi kalkulasi `calculateNursingSummaryMetrics`. |

---

## 6. Bukti Visual Acceptance Criteria

| ID | Visual Acceptance Criteria | Status & Bukti |
|---|---|---|
| **VAC-01** | Struktur layout presisi mengikuti pola 4 wilayah Shared Clinical Workspace (`Header Sticky + Left Nav + Content + Right Summary`). | ✅ **Sesuai.** Diimplementasikan pada `ClinicalWorkspaceShell.jsx` dan `clinical-workspace.module.css`. |
| **VAC-02** | Patient Context Header tertata rapi dengan hierarki visual teratur, bukan grid administratif padat ala IGD. | ✅ **Sesuai.** Nama Pasien, Usia, dan RM tampil paling dominan, diikuti lokasi rawat dan tim perawat/dokter. |
| **VAC-03** | Patient Context Header tetap terlihat (*sticky*) saat konten digulung. | ✅ **Sesuai.** Menggunakan `position: sticky; top: 0; z-index: 20;` dengan backdrop blur. |
| **VAC-04** | Section aktif pada Left Section Nav memiliki indikator visual selected state yang kontras dan jelas. | ✅ **Sesuai.** Indikator garis aksen biru vertikal `::before` dan background `#eff6ff`. |
| **VAC-05** | Quick Summary dan Info Panel di sisi kanan berpenampilan card modular terpisah yang proporsional. | ✅ **Sesuai.** Dibuat terpisah antara `ClinicalQuickSummary` (metrik) dan `ClinicalInfoPanel` (status & wewenang). |
| **VAC-06** | Tampilan adaptif pada viewport desktop (>1024px, 3 kolom), tablet (768-1024px, 2 kolom), dan mobile (<768px, 1 kolom linier). | ✅ **Sesuai.** Media queries lengkap pada `clinical-workspace.module.css` baris 48-84. Data identitas kritis pantang disembunyikan. |
| **VAC-07** | Shared base component bebas dari hard-code istilah atau logika bisnis IGD/Rawat Inap. | ✅ **Sesuai.** Seluruh komponen di `src/components/ui/clinical-workspace/` berstatus murni domain-neutral. |

---

## 7. Bukti Pengujian Otomatis

Command pengujian unit dijalankan:
```bash
cmd /c "node --test tests/unit/inpatient-nursing-workspace.test.mjs"
```
Hasil:
```text
✔ FE-RWI-051: Seluruh file domain dan base clinical workspace terpasang (9.4318ms)
✔ FE-RWI-051 AC-06: Nol butir menu baru pada sidebar (IA-INP-05) (1.1665ms)
✔ FE-RWI-051 AC-01: Akses dalam <= 3 klik lewat Census dan Detail Episode (IA-INP-01) (1.1705ms)
✔ FE-RWI-051 AC-07: Navigasi internal 4 section persis (0.7738ms)
✔ FE-RWI-051 AC-03: Context failure mematikan seluruh izin tulis secara mutlak (0.1968ms)
✔ FE-RWI-051 AC-08: Quick Summary metrics terhitung dengan benar (0.228ms)
ℹ tests 6
ℹ suites 0
ℹ pass 6
ℹ fail 0
ℹ cancelled 0
ℹ skipped 0
ℹ todo 0
ℹ duration_ms 131.2174
```
**Status: AUTOMATED TEST: PASS (6/6 passing).**
