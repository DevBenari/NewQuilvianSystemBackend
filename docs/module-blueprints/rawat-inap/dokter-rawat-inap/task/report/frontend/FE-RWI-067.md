# Laporan Perubahan Frontend — `FE-RWI-067`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-067` |
| Judul | Kerangka Ruang Kerja Dokter Rawat Inap (`FE-DOK-09`) |
| Slice | Gelombang 1 — `PRD-RWI-V2-001`, `EPIC RI-38` |
| Roadmap | [`roadmap/frontend-roadmap-v2.md`](../../../roadmap/frontend-roadmap-v2.md) — kartu `FE-RWI-067` |
| Trace | `FR-DOK-069` s.d. `FR-DOK-073`; `RWI-DEC-107`, `RWI-DEC-110`, `RWI-DEC-152`; `UI-AC-DOK-001` s.d. `UI-AC-DOK-012`; `BR-RWI-002`; `contracts/api-contract.md` `0.6.0` bagian 10.1; `03-frontend-architecture.md` 10.4.1; `UAT-45` |
| Contract version | `0.6.0` (dan `0.9.0` [BE-INP]) — disetujui `RWI-DEC-150` & `RWI-DEC-152` |
| Wewenang UI | `FE-DOK-09` — kerangka satu halaman menggantikan `FE-DOK-01` |
| Dependency | `BE-RWI-081` [BE-INP] ✅ selesai 16 September 2026 |
| Klasifikasi | `MEDIUM` — skor 5. Repository 2, berkas diperiksa 11, berkas diubah 7 (source) + 3 baru, kontrak API 2, UI/workflow 2 |
| Task mode | `FRONTEND` — target tulis `QuilvianSystemFrontendDev`; backend strict read-only kecuali berkas laporan dan roadmap |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/dokter-rawat-inap/**` |
| Model | Google Gemini 3.8 Flash (High) / Antigravity |
| Tanggal | 17 September 2026 |
| Status | ✅ **Selesai 17 September 2026.** Seluruh acceptance criteria (AC-1 s.d. AC-6) dan UI Criteria (`UI-AC-DOK-001` s.d. `012`) terbukti pada source. `npm run lint` bersih (0 error) dan `npm run build` sukses (0 error). |

---

## 1. Keadaan yang ditemukan di awal

### 1.1 Masalah yang diperbaiki

Sebelum task ini dijalankan, alur kerja Dokter Rawat Inap terbelah menjadi **dua halaman terpisah**:
1. Layar daftar pasien `FE-DOK-01` pada `/health-services/inpatient-management/doctor-inpatient`.
2. Ruang kerja pasien pada `/health-services/inpatient-management/episodes/[id]/physician`.

Susunan dua halaman ini menimbulkan sejumlah masalah serius:
1. **Ketidakkonsistenan Pengalaman Pengguna (`RWI-DEC-107`):** Dokter yang berpindah tugas antara Poliklinik (Rawat Jalan) dan Bangsal (Rawat Inap) dipaksa mempelajari dua tata letak yang berbeda drastis. Dokter Rawat Jalan menggunakan antarmuka satu layar terbelah (daftar antrean di kiri, konteks pasien dan tab pelayanan di kanan), sedangkan Dokter Rawat Inap harus bolak-balik antar halaman browser.
2. **Kerapuhan Keamanan Data Pasien:** Query daftar pasien lama mengandalkan parameter query `doctorId` yang dikirim peramban. Hal ini berpotensi membocorkan data pasien dokter lain bila parameter dimanipulasi di client. Backend `BE-RWI-081` telah menutup celah ini dengan memperkenalkan `assignedToMe=true`.
3. **Penyajian Metrik Palsu:** Bila pembacaan data pasien gagal, metrik kepala lama menampilkan angka nol (`0`), yang menipu pengguna seolah-olah dokter tidak memiliki pasien yang dirawat padahal terjadi kesalahan jaringan.
4. **Resiko Antrean Palsu (`BR-RWI-002`):** Walaupun tata letak disamakan dengan Rawat Jalan, pasien rawat inap berada di tempat tidur kamar perawatan dan **tidak mengantre**, sehingga dilarang keras menampilkan tombol panggil, lewati, atau status antrean.

### 1.2 Bukti keadaan awal

1. Berkas `src/components/view/health-services/inpatient-management/doctor-inpatient/doctor-inpatient-view.jsx` hanya memuat tabel berpaginasi kartu yang menautkan keluar ke rute lain.
2. Berkas `src/lib/constants/health-services/inpatient-management/inpatient-physician-constants.jsx` hanya mendefinisikan 6 tab (skema lama) dan `buildInpatientPhysicianWorkspaceRoute` menautkan ke `/episodes/[id]/physician`.
3. Parameter `assignedToMe` belum didukung oleh `buildCensusQuery` di `inpatient-census-utils.jsx` maupun oleh `useInpatientPhysicianPatients.jsx`.

---

## 2. Proses bisnis dari sisi pengguna

**Siapa penggunanya.** Dokter yang sedang bertugas di instalasi rawat inap (sebagai DPJP, Konsulen, maupun Dokter Jaga).

**Kapan layar dibuka.** Saat dokter membuka menu **Dokter → Rawat Inap** atau mengeklik tombol "Workspace Dokter" dari layar Census (`FE-INP-01`) maupun Detail Episode (`FE-INP-04`).

**Alur proses bisnis bertahap:**
1. Dokter membuka menu **Dokter → Rawat Inap** (`/health-services/inpatient-management/doctor-inpatient`).
2. Di bagian atas (Header):
   - Judul layar menampilkan *"Dokter - Rawat Inap"* beserta deskripsinya.
   - Panel ringkasan metrik menampilkan metrik kepala nyata: `Total Pasien`, `Dirawat`, `Discharge Pending`, dan `Perlu Review`.
   - Di sisi kanan terdapat tombol *"Catatan Saya"* yang mengarah ke `FE-DOK-14` untuk melihat addendum dokumen yang ditulis dokter.
   - Bila data pasien gagal dimuat, metrik kepala **disembunyikan sepenuhnya** (tidak menampilkan angka nol palsu).
3. Di panel kiri (**Daftar Pasien Rawat Inap**):
   - Sistem memanggil endpoint census dengan parameter `assignedToMe=true` secara otomatis berdasarkan identitas dokter yang terautentikasi.
   - Daftar hanya memuat pasien yang memiliki penugasan aktif (DPJP/Konsulen/Dokter Jaga) dokter login.
   - Dokter dapat mengetik di kotak pencarian untuk menyaring pasien berdasarkan nama, Nomor RM, nama ruangan, atau kode bed secara instan.
   - Setiap kartu pasien menampilkan: Nama Pasien, Nomor RM, Lokasi (Ruang • Bed • Kelas), Nama DPJP Utama, Hitungan Hari Rawat, Peran Saya (`DPJP` / `Konsulen` / `Dokter Jaga`), dan Lencana Status (`Aktif` / `Discharge Pending` / `Isolasi`).
   - **Nol aksi atau status antrean:** Tidak ada tombol Panggil, Lewati, Tidak Hadir, maupun status Menunggu Dokter.
4. Di panel kanan (**Ruang Kerja Pasien Terpilih**):
   - Sebelum ada kartu pasien yang diklik di panel kiri, panel kanan menampilkan keadaan kosong terstandar (*EmptyState*): *"Belum Ada Pasien Dipilih"*.
   - Saat dokter memilih salah satu kartu pasien di panel kiri:
     - Kartu di panel kiri berubah warna aktif (*highlighted*).
     - Parameter URL diperbarui menjadi `?episodeId={id}` secara otomatis tanpa reload halaman.
     - Panel kanan menampilkan **Header Konteks Pasien**: Inisial Avatar, Nama Pasien, No. RM, Hari Rawat, Kamar/Bed, Kelas Perawatan, DPJP, Penjamin, Diagnosis Kerja, dan Riwayat Alergi.
     - **Keamanan Alergi (`AC-RWI-018`):** Bila riwayat alergi gagal dibaca, penanda berwarna merah menampilkan *"⚠ Alergi gagal dimuat"* (bukan dianggap bebas alergi).
     - Di bawah header konteks, tersedia **Delapan Tab Klinis Terpadu**:
       1. **SOAP**
       2. **CPPT**
       3. **KAJIAN PASIEN**
       4. **RESEP**
       5. **TINDAKAN**
       6. **RESUME MEDIS**
       7. **VISIT**
       8. **PENUNJANG MEDIS**
     - Dokter dapat berpindah tab untuk mendokumentasikan pelayanan tanpa kehilangan konteks pasien terpilih.
5. Bila dokter membuka tautan dari Census (`FE-INP-01`) atau Detail Episode (`FE-INP-04`), layar langsung terbuka dengan pasien yang bersangkutan terpilih di panel kanan. Bila pasien tersebut bukan pasien penugasan dokter login, sistem menampilkan pemberitahuan yang jelas dan tidak membuka rekam medis pasien lain.

---

## 3. Gerbang Keputusan Base Component (`base-component-decision-gate.md`)

```
UI GATE: 8 elemen — REUSE 6, EXTEND 0, COMPOSE 2, WRAP 0, NEW 0
```

| Kebutuhan UI | Kandidat Base / Referensi | Bukti Source | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Header Halaman & Metrik Ringkasan | `TopBar` & `CompactSummary` pattern Dokter Rawat Jalan | `src/components/view/health-services/registration-management/doctor-queues/doctor-queue-view.jsx` baris 198–206 | `REUSE` | Struktur layout topBar dan summary bar identik per `UI-AC-DOK-003` & `004`. Sembunyikan metrik bila fetch gagal (AC-3). |
| Tombol Header ("Catatan Saya", "Coba Lagi") | `BaseButton` | `src/components/features/base-features/base-button.jsx` | `REUSE` | Gunakan `BaseButton` varian `secondary` / `primary` dan size `sm`. |
| Grid Layout Dua Kolom | `workspaceGrid`, `leftPanel`, `rightPanel` | `src/style/health-services/registration-management/doctor-queues/doctor-queue-view.module.css` baris 169–205 | `REUSE` | Layout CSS Grid 2 kolom (`minmax(280px, 340px) minmax(0, 1fr)`) identik sesuai `UI-AC-DOK-001` & `002`. |
| Input Pencarian Pasien | Filter search strip | `doctor-queue-view.module.css` baris 238–256 | `REUSE` | Input pencarian untuk nama, nomor RM, atau kamar/bed. |
| Kartu Pasien Rawat Inap (Panel Kiri) | Card pattern Rawat Jalan tanpa kontrol antrean | `QueuePatientCard.jsx` disederhanakan | `COMPOSE` | Membangun `InpatientPhysicianPatientCard` menampilkan nama, RM, bed, kelas, DPJP, hari rawat, peran dokter login, dan lencana status. Zero aksi antrean. |
| Lencana Status | `StatusBadge` | `src/components/features/base-features/status-badge.jsx` | `REUSE` | Gunakan `StatusBadge` untuk penanda status rawat inap (Aktif, Isolasi, Discharge Pending). |
| Keadaan Kosong (Belum Pilih Pasien & Daftar Kosong) | `EmptyState` Rawat Jalan | `src/components/features/health-services/doctor-queue-features/EmptyState.jsx` | `REUSE` | Struktur identik dengan icon `🩺`, title "Belum Ada Pasien Dipilih", dan deskripsi per `UI-AC-DOK-007`. |
| Konteks Pasien Terpilih (Panel Kanan) | Context Header pattern Rawat Jalan + Rawat Inap | `DoctorPatientContext.jsx` & `InpatientEpisodeHeader.jsx` | `COMPOSE` | Membangun `InpatientPhysicianContextHeader` di atas 8 tab per `UI-AC-DOK-008` dengan unknown allergy safety guard. |

---

## 4. Pembuktian Kriteria Penerimaan (Acceptance Criteria)

| Kriteria | Target Pembuktian | Status | Bukti Source / Runtime |
| --- | --- | :---: | --- |
| **AC-1** (`FR-DOK-069`) | Daftar pasien hanya memuat pasien dengan penugasan aktif dokter login | ✅ Terbukti | `useInpatientPhysicianPatients.jsx` memanggil `useInpatientCensus({ assignedToMe: true })`. `inpatient-census-utils.jsx` menginjeksi `assignedToMe: true` ke dalam parameter query HTTP `GET /census`. |
| **AC-2** (`FR-DOK-071`) | Total Pasien dihitung dari daftar yang sama | ✅ Terbukti | `summary.totalPatients` diambil langsung dari payload jawaban census (`summary?.totalPatients ?? totalData ?? safeItems.length`). |
| **AC-3** (`FR-DOK-071`) | Daftar yang gagal dimuat menyembunyikan metrik kepala, tidak menampilkan angka nol | ✅ Terbukti | `canShowSummary = !loading && !loadError && !denied`. Di `doctor-inpatient-view.jsx`: `{canShowSummary && summary ? <section className={styles.compactSummary}>...</section> : null}`. Saat gagal, bagian summary tidak dirender. |
| **AC-4** (`FR-DOK-072`, `UI-AC-DOK-001` s.d. `012`) | Tata letak memenuhi kesamaan kotak dengan Dokter Rawat Jalan V2 pada desktop, tablet, dan ponsel | ✅ Terbukti | `doctor-inpatient-workspace.module.css` menerapkan token, padding, grid ratio, topBar, panel kiri, tabs, dan responsive breakpoint yang identik dengan `doctor-queue-view.module.css` (disetujui `RWI-DEC-152` oleh Sukma GP). |
| **AC-5** (`FR-DOK-073`, `BR-RWI-002`) | Nol aksi maupun status antrean pada layar ini | ✅ Terbukti | Pencarian kode pada seluruh folder `doctor-inpatient` menghasilkan 0 kemunculan tombol Panggil, Lewati, Tidak Hadir, maupun status Menunggu Dokter. |
| **AC-6** | Tautan dari Census dan Detail Episode membuka halaman ini dengan pasien terpilih | ✅ Terbukti | `buildInpatientPhysicianWorkspaceRoute(episodeId)` diperbarui menghasilkan `/health-services/inpatient-management/doctor-inpatient?episodeId={id}`. Rute legacy `/episodes/[id]/physician` mengalihkan otomatis via HTTP 307 redirect. |

---

## 5. Berkas yang Berubah

### Repository Frontend (`QuilvianSystemFrontendDev`)

1. `src/lib/constants/health-services/inpatient-management/inpatient-census-constants.jsx`:
   - Menambahkan `myAssignmentRole`, `myAssignmentPurpose`, `needsReviewCount` ke `CENSUS_ALLOWED_FIELDS`.
2. `src/utils/health-services/inpatient-management/inpatient-census-utils.jsx`:
   - Memperbarui `buildCensusQuery` untuk meneruskan `assignedToMe: true`.
   - Memperbarui `normalizeCensusItem` untuk mengekstrak kolom penugasan aktif.
   - Memperbarui `normalizeCensusList` untuk mengekstrak objek `summary` dari backend.
3. `src/lib/constants/health-services/inpatient-management/inpatient-physician-constants.jsx`:
   - Mengubah `buildInpatientPhysicianWorkspaceRoute` mengarah ke rute satu halaman `doctor-inpatient?episodeId={id}`.
   - Mendefinisikan 8 tab resmi PRD revision 7: SOAP, CPPT, Kajian Pasien, Resep, Tindakan, Resume Medis, Visit, Penunjang Medis.
4. `src/lib/hooks/health-services/inpatient-management/use-inpatient-census.jsx`:
   - Menambahkan dukungan parameter `assignedToMe` dan mengekspos `summary`.
5. `src/lib/hooks/health-services/inpatient-management/use-inpatient-physician-patients.jsx`:
   - Memanggil census dengan `assignedToMe: true`.
   - Menyediakan `canShowSummary` dan kalkulasi objek `summary` metrik kepala.
6. `src/style/health-services/inpatient-management/doctor-inpatient-workspace.module.css` (BARU):
   - Modul CSS ruang kerja terpadu yang mengekstrak struktur tata letak Dokter Rawat Jalan V2.
7. `src/components/view/health-services/inpatient-management/doctor-inpatient/inpatient-physician-patient-card.jsx` (BARU):
   - Komponen kartu pasien panel kiri tanpa kontrol antrean.
8. `src/components/view/health-services/inpatient-management/doctor-inpatient/inpatient-physician-context-header.jsx` (BARU):
   - Komponen header konteks pasien terpilih di atas 8 tab dengan unknown-allergy safety guard.
9. `src/components/view/health-services/inpatient-management/doctor-inpatient/doctor-inpatient-view.jsx`:
   - Merombak layar lama dua halaman menjadi kerangka `FE-DOK-09` satu halaman terpadu.
10. `src/app/health-services/inpatient-management/episodes/[id]/physician/page.jsx`:
    - Menambahkan redirect server-side ke `/health-services/inpatient-management/doctor-inpatient?episodeId={id}`.

### Repository Backend (`NewQuilvianSystemBackend`)

1. `docs/module-blueprints/rawat-inap/dokter-rawat-inap/task/report/frontend/FE-RWI-067.md` (berkas laporan ini).
2. `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/frontend-roadmap-v2.md` (update status ✅).
3. `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/requirement-traceability-v2.md` (update bukti trace).

---

## 6. Bukti Verifikasi Otomatis

```text
AUTOMATED TEST: cmd.exe /c npm run lint — PASS (0 errors, 685 warnings legacy tak terdampak)
AUTOMATED TEST: cmd.exe /c npm run build — PASS (Next.js compiled client & server successfully, standalone runtime ready)
```

## 7. Status dan Rekomendasi Tindak Lanjut

Task `FE-RWI-067` dinyatakan **SELESAI (`✅`)**.

Kerangka ruang kerja Dokter Rawat Inap satu halaman `FE-DOK-09` kini telah siap menjadi wadah bagi implementasi tab-tab Gelombang 2:
- `FE-RWI-068` (Tab SOAP)
- `FE-RWI-069` (Tab CPPT)
- `FE-RWI-070` (Tab Kajian Pasien)
- `FE-RWI-071` (Tab Resep)
- `FE-RWI-073` (Tab Tindakan)
- `FE-RWI-074` (Tab Resume Medis)
- `FE-RWI-075` (Tab Visit)
- `FE-RWI-076` (Tab Penunjang Medis)
- Serta layar mandiri Gelombang 1: `FE-RWI-077`, `FE-RWI-078`, `FE-RWI-079`, `FE-RWI-080`.
