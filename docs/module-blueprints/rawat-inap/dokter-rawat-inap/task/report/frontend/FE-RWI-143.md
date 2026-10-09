# Laporan Perubahan Frontend — `FE-RWI-143`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-143` |
| Judul | Refactor Tab Penunjang: Navigasi Sub-Tab 2 Level, Grid 6 Kartu Modern, & Sub-View Shell Container |
| Gelombang | Rencana Kerja Penunjang Medis Gelombang 1 |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/frontend-roadmap-v2.md` kartu `FE-RWI-143` |
| Dokumen Rencana | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/rencana-kerja/penunjang-medis/penunjang-medis.md` (Rev 2.0 DISETUJUI) |
| Wewenang UI | `rencana-kerja/penunjang-medis/penunjang-medis.md` §7.1, §7.2; `frontend-roadmap-v2.md` kartu `FE-RWI-143` |
| Dependency | `FE-RWI-076` ✅ selesai |
| Klasifikasi | `HIGH` — Refactor navigasi tab penunjang medis dari tampilan datar menjadi arsitektur 2-level sub-tab terpadu dengan sub-view shell container dan 6 kartu layanan aktif |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; repositori backend untuk dokumen roadmap & laporan task |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ `SELESAI`. Seluruh acceptance criteria terverifikasi lewat uji unit otomatis (22/22 PASS), audit eslint bersih (0 error, 0 warning), dan keselarasan desain token Quilvian |

---

## 1. Keadaan yang ditemukan di awal

Sebelum pengerjaan `FE-RWI-143`:
1. Tab Penunjang Medis pada `FE-DOK-13` menyajikan overview kartu di mana 3 layanan (Gizi, Rehab Medik, Bank Darah) berstatus "Integrasi belum tersedia" yang memblokir alur kerja dokter.
2. Saat dokter mengklik salah satu kartu layanan penunjang, tidak ada struktur dedicated sub-view dengan tombol kembali yang jelas; dokter kesulitan kembali ke overview tanpa me-refresh atau mengklik tab utama.
3. Belum ada arsitektur navigasi sub-tab 2-level yang memisahkan antara `[Formulir Pemesanan]` dan `[Riwayat & Hasil Pemeriksaan]`, sehingga alur pemesanan penunjang masih bertumpuk dalam satu layar modal kecil.

---

## 2. Proses bisnis dari sisi pengguna

1. Dokter membuka tab **Penunjang Medis** pada ruang kerja dokter rawat inap (`FE-DOK-09`).
2. Tampilan awal menyajikan **Overview 6 Kartu Interaktif**:
   - **Laboratorium**: menampilkan total pesanan live, jumlah hasil final, ringkasan pesanan, dan tombol "Buka Layanan".
   - **Radiologi**: menampilkan total pesanan live, modalitas, hasil final, dan tombol "Buka Layanan".
   - **Gizi / Konsultasi Gizi**: menampilkan total pesanan konsultasi gizi, status asuhan, dan tombol "Buka Layanan".
   - **Rehabilitasi Medik**: menampilkan total pesanan tindakan terapi fisik, status sesi, dan tombol "Buka Layanan".
   - **Hemodialisa**: menampilkan total pesanan tindakan HD, prioritas Cito/Rutin, dan tombol "Buka Layanan".
   - **Bank Darah**: menampilkan total permintaan darah, status proses BDRS, dan tombol "Buka Layanan".
3. Saat dokter memilih salah satu kartu (misalnya Laboratorium atau Gizi), sistem membuka **Dedicated Sub-View Shell Container**:
   - Di bagian kiri atas terdapat tombol jelas `[← Kembali ke Pilihan Layanan]` yang mengembalikan dokter ke ringkasan 6 kartu kapan saja tanpa reload.
   - Header menampilkan Judul Layanan, Ikon, Konteks Kamar & Bed Pasien, serta Badge "Layanan Terhubung".
   - Di bawah header terdapat bar sub-tab horizontal modern:
     - `[📝 Formulir Pemesanan]` — formulir interaktif untuk membuat permintaan CPOE baru.
     - `[📊 Riwayat & Hasil Pemeriksaan (N)]` — tabel riwayat seluruh pesanan pada episode ini lengkap dengan counter badge dinamis.
4. Pergantian antar sub-tab berjalan instan tanpa me-reload data dari server.
5. Saat pasien/episode berganti, sistem secara aman mereset state ke `activeServiceKey = "all"` dan `activeSubTab = "form"` (Patient Switch Safety).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diubah dan dibuat

| Berkas | Status | Perubahan |
| :--- | :---: | :--- |
| `src/lib/constants/health-services/inpatient-management/inpatient-supporting-service-constants.jsx` | Ubah | Mengaktifkan seluruh 6 layanan (`isAvailable: true`, badge "Tersedia"); menambahkan `SUPPORTING_SUB_TABS` (`FORM`, `HISTORY`) dan label sub-tab |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-supporting-service.jsx` | Ubah | Menambahkan state, effect loader, dan mutasi pemesanan untuk Gizi, Bank Darah, dan Rehab Medik; mengekspos token refresh |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-landing-grid.jsx` | Ubah | Mengintegrasikan metrik live dan preview orders untuk seluruh 6 layanan penunjang |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-service-tab.jsx` | Ubah | Mengimplementasikan navigasi sub-tab 2 level, container sub-view dengan tombol kembali, serta integrasi switch formulir dan riwayat |
| `src/style/health-services/inpatient-management/physician-supporting-service.module.css` | Ubah | Menambahkan token styling untuk container sub-view, tombol navigasi kembali, dan bar sub-tab horizontal |
| `tests/unit/inpatient-supporting-service-modernisasi.test.mjs` | Baru | 7 unit test memverifikasi ketersediaan 6 layanan, navigasi 2 level, sub-view shell, dan integrasi komponen |

### 3.2 Tabel keputusan base component

| Elemen UI | Sumber Komponen | Status | Rationale |
| :--- | :--- | :---: | :--- |
| **Header Tab Penunjang** | `DoctorSupportingHeader` | `REUSE` | Mempertahankan header klinis terpadu dokter |
| **Navigasi Segmen Antarlayanan** | `ClinicalSegmentedNav` | `REUSE` | Memfasilitasi loncat antar layanan dengan badge counter live |
| **Tombol Kembali ke Landing** | `BaseButton` (`variant="outline"`, `size="sm"`) | `REUSE` | Tombol `[← Kembali ke Pilihan Layanan]` konsisten dengan design system |
| **Bar Sub-Tab Horizontal** | Button tabs dengan token CSS Quilvian | `COMPOSE` | Dirangkai memakai base tokens dengan counter badge bulat |
| **Grid 6 Kartu Overview** | `SupportingLandingGrid` (`BaseButton`, `ClinicalAuditBadge`) | `COMPOSE` | Merangkai 6 kartu layanan bergradien lembut |

> **UI GATE**: Seluruh elemen berstatus `REUSE` atau `COMPOSE`. Nol elemen `NEW` yang melanggar governance.

---

## 4. Peta acceptance criteria

| AC | Deskripsi | Bukti Implementasi & Verifikasi |
| :--- | :--- | :--- |
| **AC-1** | Navigasi 6 kartu dan sub-view mulus tanpa reload | `SupportingServiceTab` mengelola state `activeServiceKey` dan `activeSubTab` secara reaktif di memory tanpa navigasi URL page reload |
| **AC-2** | Tombol kembali berfungsi | Tombol `[← Kembali ke Pilihan Layanan]` (`data-testid="btn-back-to-landing"`) mereset `activeServiceKey` ke `"all"` |
| **AC-3** | Seluruh 6 kartu memiliki container sub-view aktif | Masing-masing dari 6 layanan memiliki dedicated view lengkap dengan sub-tab `[Formulir]` dan `[Riwayat]` |
| **AC-4** | Reset state aman saat pasien berganti | `trackedEpisodeId` mendeteksi pergantian pasien dan mereset `activeServiceKey` ke `"all"` serta `activeSubTab` ke `"form"` |

---

## 5. Bukti verifikasi & eksekusi

- **Linting**:
  `npx eslint src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/`
  Hasil: **0 errors, 0 warnings (PASS)**.
- **Automated Test**:
  `node --test tests/unit/inpatient-supporting-service*.test.mjs`
  Hasil: **22 tests PASS (0 fail, 0 skipped)**.
- **Manual Test**:
  Simulasi pergantian tab `activeServiceKey` dari "all" ke "laboratory", perpindahan sub-tab dari "form" ke "history", dan penekanan tombol kembali ke landing: **PASS**.
