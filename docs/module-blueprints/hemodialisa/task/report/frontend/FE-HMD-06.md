# Laporan Perubahan Frontend — `FE-HMD-06`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HMD-06` |
| Judul | Lembar Kerja Kesiapan Unit Shift dan Pengolahan Air (`FE-HMD-05` pada Arsitektur) |
| Slice | `MVP-1` — Master Data Mesin, Station, dan Lembar Kesiapan Unit |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/frontend-roadmap.md` bagian 4.2 |
| Trace | `FR-HMD-040`, `FR-HMD-041`, `FR-HMD-042`, `FE-HMD-05`, `CAP-15`, `CAP-24`, `NFR-004`, `HMD-VAL-100` s.d. `HMD-VAL-103`; `contracts/api-contract.md` Grup Hemodialysis Unit Readiness; `contracts/state-transition-matrix.md` Bagian 4 & 7 |
| Contract version | `HMD-CONTRACT-v1` — status `approved` (18 September 2026) |
| Wewenang UI | `DEV_DISCRETION` untuk pemilihan ikon status, tata letak seksi filter dan kartu air, penataan form checklist 5 butir, dan dialog konfirmasi modal |
| Dependency | `FE-HMD-02` (selesai), `FE-HMD-05` (selesai), `BE-HMD-06` (selesai) |
| Klasifikasi | `MEDIUM` — skor 7: repository 0, berkas diperiksa 7, berkas dibuat 4, berkas diubah 2, logika 2, kontrak API 2, database 0, UI 2 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — `src/utils/health-services/hemodialysis-management/hemodialysis-unit-readiness-display-utils.js`, `src/lib/state/slice/health-services/hemodialysis-management/hemodialysisUnitReadinessSlice.js`, `src/lib/state/store.jsx`, `src/style/health-services/hemodialysis-management/hemodialysis-unit-readiness.module.css`, `src/components/view/health-services/hemodialysis-management/unit-readiness/hemodialysis-unit-readiness-view.jsx`, `src/app/health-services/hemodialysis-management/unit-readiness/page.jsx`, `tests/unit/hemodialysis-unit-readiness.test.mjs` |
| Model | Gemini 3.8 Flash |
| Commit frontend saat dikerjakan | `7fa29bf` pada branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `89028993` pada branch `MHamzah` |
| Tanggal | 22 September 2026 |
| Status | ✅ Selesai — seluruh acceptance criteria terverifikasi dengan unit test otomatis Node.js, ESLint 0 error 0 warning, dan `next build` kompilasi 100% sukses |

---

## 1. Keadaan yang Ditemukan di Awal

Sebelum task ini dikerjakan:
1. Rute App Router `/health-services/hemodialysis-management/unit-readiness/page.jsx` hanya berupa komponen placeholder statis sederhana tanpa integrasi API atau interaksi data operasional.
2. Belum ada antarmuka digital bagi Koordinator Unit Hemodialisa untuk melakukan inspeksi harian pra-shift terhadap 5 komponen kritikal: kelaikan mesin HD, kesiapan tempat tidur/station, kelaikan instalasi pengolahan air (*water treatment* RO), ketersediaan obat & BMHP, serta kesiapan tenaga perawat mahir dan DPJP.
3. Parameter masa berlaku uji air (*WaterResultValidityHours*) yang telah disetel pada `FE-HMD-05` belum diterapkan pada antarmuka kesiapan shift, sehingga risiko pelayanan dialisis berjalan dengan air yang kedaluwarsa secara mikrobiologis atau endotoksin masih terbuka.
4. Endpoint backend `HmdUnitReadinessController` (`GET`, `POST /`, `PUT /{id}/items`, `POST /{id}/declare-ready`, `POST /{id}/declare-not-ready`) yang diimplementasikan pada `BE-HMD-06` belum memiliki representasi visual, penanganan state, maupun validasi penguncian tombol di frontend.

---

## 2. Proses Bisnis dari Sisi Pengguna (User Journey)

### 2.1 Pemilihan Tanggal dan Shift Pelayanan
1. Koordinator Unit HD membuka menu *Pelayanan Kesehatan > Hemodialisa > Kesiapan Unit*.
2. Sistem secara otomatis memuat tanggal hari ini dan shift aktif bawaan (contoh: *Pagi (07.00 - 12.00 WIB)*).
3. Pengguna dapat memilih tanggal lain atau berpindah antar shift (*Pagi*, *Siang*, *Sore*):
   - Jika lembar pemeriksaan untuk tanggal dan shift tersebut sudah pernah dibuat, data detail langsung dimuat beserta hasil 5 butir checklist dan pita statusnya.
   - Jika lembar belum ada, `ClinicalStateBoundary` menyajikan state `empty` yang informatif (*"Lembar Kesiapan Belum Dibuat"*) lengkap dengan tombol aksi *"Buka Lembar Shift Ini"*.

### 2.2 Pemeriksaan Kelaikan Instalasi Pengolahan Air (Water Treatment) & Peringatan Kritis (AC-1)
1. Layar menyajikan kartu khusus **Pemeriksaan Instalasi Pengolahan Air (Water Treatment)**:
   - Menampilkan batas kebijakan unit (`720 Jam` / `~30 Hari Kalender` atau nilai terkini dari database).
   - Menampilkan tanggal uji lab terakhir, tanggal akhir masa berlaku, dan sisa hari kelaikan.
2. **Penegakan Keselamatan Air Kedaluwarsa (AC-1)**:
   - Apabila tanggal hasil uji air berumur lebih dari batas masa berlaku (misal berumur 35 hari pada batas 30 hari), kartu air seketika menampilkan lencana merah *"Air Belum Laik / Kedaluwarsa"*.
   - Komponen peringatan klinis `ClinicalSafetyAlert` bertone `critical` muncul mencolok di atas daftar butir:
     > *"Uji air kedaluwarsa 5 hari lalu. Unit tidak dapat dinyatakan siap."*
   - Tombol **"Nyatakan Unit Siap" terkunci nonaktif (disabled)** secara mutlak. Pengguna tidak dapat meloloskan deklarasi unit siap selama hasil air belum diperbarui ke tanggal yang laik.

### 2.3 Pengisian Hasil 5 Butir Standar Kesiapan Operasional Shift
1. Koordinator memeriksa 5 butir standar yang terpetakan dari master checklist:
   - **Mesin Hemodialisa** (`Machine` / 1): Kesiapan mesin siap operasional dan bebas alarm.
   - **Station & Tempat Tidur** (`Station` / 2): Kebersihan ruang dan kalibrasi timbangan.
   - **Instalasi Pengolahan Air** (`Water` / 3): Kelaikan air RO, TDS, konduktivitas, dan uji mikrobiologi. Dilengkapi kolom *Tanggal Hasil* dan *Nomor Referensi/Lab*.
   - **BMHP & Obat Dialisis** (`Supply` / 4): Ketersediaan dializer, konsentrat asam/bikarbonat, blood tubing line, fistula needle, dan heparin.
   - **Tenaga Dokter & Perawat** (`Staff` / 5): Kehadiran DPJP on-call dan perawat mahir dialisis bersertifikat.
2. Setiap butir memiliki 3 pilihan radio yang responsif:
   - `Terpenuhi` (`Met` = 1)
   - `Tidak Terpenuhi` (`NotMet` = 2)
   - `Tidak Berlaku` (`NotApplicable` = 3)
3. Koordinator dapat mengklik tombol **"Simpan Pemeriksaan"** (`PUT /{id}/items`) untuk menyimpan draf isian kapan saja tanpa harus langsung menyatakan siap.

### 2.4 Deklarasi Formal Unit Siap Melayani (AC-2)
1. Ketika seluruh butir wajib (5 butir) telah ditandai `Terpenuhi` dan hasil air terbukti laik (masih dalam masa berlaku):
   - Peringatan butir penahan hilang dari antarmuka.
   - Tombol **"Nyatakan Unit Siap"** menjadi aktif berwarna hijau.
2. Koordinator menekan tombol **"Nyatakan Unit Siap"**:
   - Sistem membuka dialog `ConfirmModal` untuk konfirmasi formal.
   - Setelah menekan konfirmasi, request `POST /{id}/declare-ready` dikirim ke backend.
   - Backend memvalidasi integritas data, mencatat `DeclaredByUserId` (Koordinator) dan `DeclaredAt` (waktu server).
3. **Perubahan Pita Status (AC-2)**:
   - Pita status di bagian atas berubah seketika menjadi hijau bertuliskan:
     > **"Unit Siap untuk Shift Pagi"**
     > *Seluruh butir kelaikan mesin, station, air RO, dan tenaga telah diverifikasi. Pelayanan sesi cuci darah dapat berjalan.*
     > *Dinyatakan oleh: Ns. Hendra, S.Kep (22 September 2026, 06.30 WIB)*
   - Kartu ringkasan metrik `SummaryGrid` merefleksikan status *Unit Siap Melayani* dan 5 dari 5 Butir Terpenuhi.

### 2.5 Deklarasi Unit Tidak Siap dengan Alasan Wajib Tertulis
1. Apabila terjadi kendala fatal sebelum atau selama shift (misal kebocoran sistem RO atau kekurangan tenaga):
   - Koordinator menekan tombol **"Nyatakan Unit Tidak Siap"**.
2. Sistem membuka modal dialog `ConfirmModal` dengan input alasan operasional wajib (`requireReason = true`).
3. Sistem memvalidasi input alasan secara ketat:
   - Tombol konfirmasi terkunci bila kolom alasan kosong atau hanya spasi.
   - Alasan minimal 5 karakter wajib disertakan (misal: *"Pompa distribusi air RO utama mengalami penurunan tekanan drastis"*).
4. Setelah submit `POST /{id}/declare-not-ready`:
   - Pita status berubah menjadi merah mencolok: *"Unit Dinyatakan Belum Siap untuk Shift Pagi"*.
   - Alasan operasional tertulis ditampilkan di pita status untuk transparansi seluruh staf.
   - Sesuai prinsip keselamatan `BE-HMD-06`, sesi yang sedang berjalan tidak dibatalkan otomatis, tetapi pembukaan sesi baru pada shift ini tertahan oleh gerbang `HMD-VAL-042`.

---

## 3. Perubahan yang Dikerjakan

### 3.1 Berkas yang Diperiksa
- `docs/module-blueprints/hemodialisa/03-frontend-architecture.md` Bagian 4.4 & 4.5 (`FE-HMD-05`)
- `docs/module-blueprints/hemodialisa/contracts/api-contract.md` Grup Hemodialysis Unit Readiness
- `docs/module-blueprints/hemodialisa/contracts/state-transition-matrix.md` Bagian 4 & 7
- `docs/module-blueprints/hemodialisa/task/report/backend/BE-HMD-06.md`
- `NewQuilvianSystemBackend/Areas/HealthServices/HemodialysisManagement/Controllers/HmdUnitReadinessController.cs`
- `NewQuilvianSystemBackend/Areas/HealthServices/HemodialysisManagement/DTOs/HmdUnitReadinessDtos.cs`
- `QuilvianSystemFrontendDev/src/lib/services/health-services/hemodialysis-management/hmdUnitReadinessService.js`

### 3.2 Berkas yang Dibuat dan Berubah

| Berkas | Status | Deskripsi Perubahan |
| --- | --- | --- |
| `src/utils/health-services/hemodialysis-management/hemodialysis-unit-readiness-display-utils.js` | Baru | Utilitas murni: kalkulasi kelaikan uji air (AC-1), evaluasi kelengkapan butir wajib (AC-2), perhitungan hari kalender kedaluwarsa, builder metrik `SummaryGrid`, dan resolver badge status |
| `src/lib/state/slice/health-services/hemodialysis-management/hemodialysisUnitReadinessSlice.js` | Baru | Redux slice mengelola data lembar kesiapan, filter tanggal & shift, mutasi draf butir lokal, status modal, dan thunk 6 aksi backend |
| `src/lib/state/store.jsx` | Diubah | Mendaftarkan `hemodialysisUnitReadinessReducer` sebagai slice `hemodialysisUnitReadiness` pada store pusat aplikasi |
| `src/style/health-services/hemodialysis-management/hemodialysis-unit-readiness.module.css` | Baru | CSS Module styling untuk kartu filter, 3 varian pita status (Ready, NotReady, Draft), kartu uji air RO, tabel checklist responsif, dan sticky action footer |
| `src/components/view/health-services/hemodialysis-management/unit-readiness/hemodialysis-unit-readiness-view.jsx` | Baru | View utama lembar kesiapan unit: integrasi Hero, SummaryGrid, ClinicalStateBoundary, ClinicalSafetyAlert air (AC-1), tabel 5 butir, pita status hijau (AC-2), action bar, dan 3 modal konfirmasi |
| `src/app/health-services/hemodialysis-management/unit-readiness/page.jsx` | Diubah | Mengganti placeholder statis dengan merender `HemodialysisUnitReadinessView` secara dinamis di App Router |
| `tests/unit/hemodialysis-unit-readiness.test.mjs` | Baru | Suite pengujian unit otomatis mencakup integritas berkas, kelaikan air kedaluwarsa (AC-1), gerbang kelengkapan butir wajib (AC-2), builder metrik, dan reducers |

---

## 4. Keputusan UI & Reuse (UI Gate: 10 Elemen)

```text
UI GATE CHECKLIST — FE-HMD-06:
1. Header Halaman (Hero Header): REUSE Base Hero (src/components/features/base-features/hero.jsx)
   - Judul: "Lembar Kesiapan Unit Shift"
   - Deskripsi operasional dan eyebrow "Pelayanan Kesehatan / Hemodialisa".
2. Pemilih Tanggal: REUSE Form.Control type="date" (React-Bootstrap)
   - Input tanggal kalender standar YYYY-MM-DD dengan styling token Quilvian.
3. Pemilih Shift: REUSE Form.Select (React-Bootstrap)
   - Dropdown pilihan shift: Pagi (1), Siang (2), Sore (3) lengkap dengan rentang jam WIB.
4. Kartu Ringkasan (Summary Grid): REUSE Base SummaryGrid (src/components/features/base-features/summary-grid.jsx)
   - 4 metrik: Status Kesiapan Shift, Butir Wajib Terpenuhi, Kelaikan Pengolahan Air, dan Penyelesai Deklarasi.
5. Penanganan State 4-Kondisi: REUSE ClinicalStateBoundary (src/components/ui/doctor-clinical-base/ClinicalStateBoundary.jsx)
   - Loading State, Error State (dengan tombol Coba Lagi), Empty State (tombol Buka Lembar Baru), dan Content State.
6. Peringatan Keselamatan Air (AC-1): REUSE ClinicalSafetyAlert (src/components/ui/doctor-clinical-base/ClinicalSafetyAlert.jsx)
   - Peringatan bertone "critical" saat uji laboratorium air kedaluwarsa atau belum diisi.
7. Tabel Checklist 5 Butir: REUSE React-Bootstrap Table, Form.Check (radio), Form.Control
   - Tabel interaktif untuk memilih status Terpenuhi / Tidak Terpenuhi / Tidak Berlaku, input tanggal hasil, dan catatan.
8. Modal Buat Lembar Shift Baru: REUSE Base ConfirmModal (src/components/features/base-features/confirm-modal.jsx)
   - Dialog konfirmasi pembentukan lembar pemeriksaan baru untuk tanggal dan shift terpilih.
9. Modal Deklarasi Siap (AC-2): REUSE Base ConfirmModal (src/components/features/base-features/confirm-modal.jsx)
   - Dialog konfirmasi formal perubahan status unit menjadi Ready.
10. Modal Deklarasi Tidak Siap: REUSE Base ConfirmModal (src/components/features/base-features/confirm-modal.jsx)
    - Dialog konfirmasi bertone "danger" dengan requireReason=true untuk input alasan operasional minimal 5 karakter.
```

> **UI Gate Statement**: `UI GATE: 10 elemen — REUSE 10, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`. Tidak ada komponen dasar visual baru yang dibuat.

---

## 5. State yang Ditangani di Layar

| State Boundary | Pemicu / Kondisi | Tampilan Antarmuka yang Dihasilkan |
| --- | --- | --- |
| **Loading** | `loading === true && !detail` | Indikator memuat data klinis dengan pesan *"Memuat Lembar Kesiapan Unit..."* |
| **Empty** | `!detail && !loading && !error` | Tampilan kosong terstandarisasi dengan pesan *"Lembar Kesiapan Belum Dibuat"* beserta tombol aksi *"Buka Lembar Kesiapan Baru"* |
| **Error** | `error` terisi saat memuat data | Banner error dari server beserta tombol *"Coba Lagi"* |
| **Content — DRAFT** | Lembar ada, status `Draft` | Formulir checklist 5 butir dapat disunting, pita kuning informatif, tombol Simpan dan deklarasi |
| **Content — READY** | Lembar ada, status `Ready` | Pita hijau formal *"Unit Siap untuk Shift [Nama Shift]"* (**AC-2**), info penyelesai dan waktu |
| **Content — NOT READY** | Lembar ada, status `NotReady` | Pita merah *"Unit Dinyatakan Belum Siap"*, alasan operasional tertulis tampil mencolok |
| **Safety Blocked** | Hasil uji air kedaluwarsa / butir wajib belum `Met` | Komponen `ClinicalSafetyAlert` merah menyala (**AC-1**) dan tombol *"Nyatakan Unit Siap"* terkunci nonaktif |

---

## 6. Verifikasi Berbasis Bukti

| Skenario atau Perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/hemodialysis-unit-readiness.test.mjs` | 5 test suites (Integritas Berkas, Kelaikan Air AC-1, Gerbang Deklarasi Siap AC-2, Builder Metrik, dan Redux Reducers) seluruhnya `PASS` (0 failed) | `PASS` | Log Node Test Runner 22 September 2026 |
| ESLint check atas seluruh berkas target | `0 Error(s)`, `0 Warning(s)` | `PASS` | Log ESLint CLI 22 September 2026 |
| `next build` kompilasi produksi Next.js | Kompilasi 100% sukses tanpa kendala build | `PASS` | Log kompilasi Next.js 22 September 2026 |
| Penegakan AC-1 Air Kedaluwarsa | Uji air umur 35 hari pada batas 30 hari menghasilkan pesan *"Uji air kedaluwarsa 5 hari lalu. Unit tidak dapat dinyatakan siap."* dan tombol siap dinonaktifkan | `PASS` | Unit test `FE-HMD-06 Water Treatment Validity (AC-1)` |
| Penegakan AC-2 Deklarasi Siap | Seluruh 5 butir `Met` dan air laik mengubah pita status menjadi *"Unit Siap untuk Shift Pagi"* | `PASS` | Unit test `FE-HMD-06 Mandatory Checklist & Declare Ready Gate (AC-2)` |
| Penegakan Validasi Alasan Tidak Siap | Alasan operasional wajib disertakan minimal 5 karakter melalui `ConfirmModal` (`requireReason = true`) | `PASS` | `ConfirmModal` dengan penanganan `cleanReason` |

---

## 7. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Air kedaluwarsa menampilkan alert merah dan mengunci tombol siap (AC-1) | Terpenuhi | `ClinicalSafetyAlert` tone critical aktif dan tombol *"Nyatakan Unit Siap"* terkunci `disabled={!readinessEvaluation.canDeclare}` |
| 2. Deklarasi siap berhasil mengubah pita status menjadi "Unit Siap untuk Shift [Nama Shift]" (AC-2) | Terpenuhi | Pita status hijau `statusBannerReady` aktif dengan teks dinamis `Unit Siap untuk Shift ${detail.ShiftName}` dan metadata penyelesai |
| 3. Deklarasi tidak siap mewajibkan alasan operasional tertulis | Terpenuhi | `ConfirmModal` dengan `requireReason=true` dan validasi submit alasan non-kosong |
| 4. Form pembentukan lembar kesiapan baru saat tanggal/shift belum ada | Terpenuhi | Tombol aksi pada empty state dan modal `ConfirmModal` pembentukan lembar baru |
| DoD: Seluruh berkas selesai, lulus unit test otomatis, ESLint 0 error 0 warning, dan lolos `next build` | Terpenuhi | 5 test PASS, ESLint bersih, kompilasi Next.js sukses |

---

## 8. Catatan Penutup

| Hal | Isi |
| --- | --- |
| Hubungan dengan Backend | Sinkron 100% dengan `BE-HMD-06` (`HmdUnitReadinessController`, DTOs, dan `HmdUnitReadinessService`) |
| Penggunaan Komponen Baku | Menepati konstitusi Quilvian dengan memakai ulang 10 komponen baku tanpa komponen visual baru |
| Perubahan Sampingan | `NONE` — tidak ada berkas di luar ruang lingkup yang diubah |
| Status Git | Berkas siap di-commit manual oleh pengguna (tanpa git push otomatis) |
| Langkah Berikutnya | Melanjutkan ke task berikutnya pada roadmap: `FE-HMD-07` (Formulir Permintaan HD Terintegrasi pada Rawat Inap) |
