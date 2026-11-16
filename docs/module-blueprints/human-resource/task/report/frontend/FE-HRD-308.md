# Laporan Penyelesaian Task: FE-HRD-308 Eliminasi Mock & Refactor Layar Pensiun Kandidat & Kasus Riil (retirement-list-view.jsx)

## 1. Informasi Task
* **Task ID**: `FE-HRD-308`
* **Slice / MVP**: `S-C5-SEP` (Submenu 2: Pensiun / Retirement)
* **Nama Fitur**: Eliminasi Mock & Refactor Layar Pensiun Kandidat & Kasus Riil (`retirement-list-view.jsx`)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-16
* **Penanggung Jawab**: Frontend Engineering Agent

---

## 2. Ringkasan Eksekutif & Kebutuhan Bisnis Rumah Sakit
Perencanaan masa purnabakti tenaga kesehatan (dokter spesialis, perawat senior, apoteker) dan tenaga penunjang rumah sakit menuntut kepastian hukum dan kesinambungan layanan medis (*clinical continuity*). Dalam regulasi ketenagakerjaan dan kebijakan rumah sakit di Indonesia, usia pensiun normal tidak seragam untuk seluruh jenis profesi: dokter spesialis atau tenaga medis tertentu seringkali memiliki batas usia pensiun berbeda (misalnya 60 atau 65 tahun) dibandingkan staf administrasi umum (56–58 tahun).

Sebelum task ini dikerjakan, layar `retirement-list-view.jsx` memiliki sejumlah kecacatan operasional kritis:
1. Menyimpan data contoh `DEFAULT_FALLBACK_CASES` dan `DEFAULT_FALLBACK_ELIGIBLE` berisi simulasi kasus pensiun yang tidak ada di database.
2. Mematok usia pensiun secara sepihak dan hardcode pada angka 58 tahun di sisi browser klien tanpa membaca kebijakan/peraturan kepegawaian rumah sakit.
3. Melakukan pembaruan status palsu di dalam blok `catch` serta menampilkan pesan sukses fiktif saat backend gagal memproses persetujuan atau serah terima.

Melalui implementasi `FE-HRD-308`:
* **Pembersihan Total Data Tiruan**: Konstanta `DEFAULT_FALLBACK_CASES` dan `DEFAULT_FALLBACK_ELIGIBLE` telah dihapus seutuhnya.
* **Integrasi Dua Tab Riil**: Tab **Kasus Pensiun** dan tab **Calon Pensiun** mengambil data proyeksi dan kasus aktif langsung dari backend melalui `SeparationService.getRetirements()`.
* **Eliminasi Hardcode Usia Pensiun**: Batas usia dan kriteria kelayakan pensiun kini diperoleh secara dinamis dari respons server atau konfigurasi kebijakan RS, bukan asumsi kaku angka 58 tahun di antarmuka.
* **Validasi Aksi Handoff Mutlak**: Fitur handoff ke proses pemisahan/offboarding hanya diizinkan untuk kasus pensiun yang telah berstatus `Approved` secara sah oleh pejabat berwenang di backend.

---

## 3. Rincian Implementasi Teknis

### 3.1 Berkas yang Dimodifikasi
* `QuilvianSystemFrontendDev/src/components/view/hr/separation/retirement-list-view.jsx`

### 3.2 Poin-Poin Perbaikan Kode
1. **Pembersihan Fallback State**:
   ```javascript
   // Sebelum: const [cases, setCases] = useState(DEFAULT_FALLBACK_CASES);
   // Sesudah:
   const [cases, setCases] = useState([]);
   const [eligibleStaff, setEligibleStaff] = useState([]);
   ```
2. **Penghapusan Usia Pensiun Hardcoded 58**:
   Logika kalkulasi usia dan status kelayakan yang sebelumnya membandingkan secara mentah `age >= 58` dirombak agar membaca field `retirementAge` atau `policyAge` dari DTO respon backend, atau menampilkan indikator proyeksi tanggal lahir asli dari data kepegawaian master.
3. **Pembersihan Blok Catch pada Aksi Approval & Handoff**:
   Menghapus pembaruan array lokal secara manipulatif pada penanganan error:
   ```javascript
   } catch (err) {
     const errorMsg = err?.response?.data?.message || err?.message || 'Gagal memproses persetujuan pensiun.';
     pushToast(errorMsg, 'error');
   }
   ```
4. **Siklus Refetch & Validasi Modal**:
   Form rencana pensiun mewajibkan pemilihan pegawai aktif riil melalui `ResourceFilterSelect`, penetapan tanggal efektif, dan kategori pensiun (Normal, Dini, Medis). Setelah submit sukses, antarmuka otomatis me-refresh data dari server.

---

## 4. Bukti Verifikasi Mutu Rekayasa
* **Pemeriksaan Linter CI/CD**:
  `npx eslint src/components/view/hr/separation/retirement-list-view.jsx` ➔ **0 errors, 0 warnings (PASS)**.
* **Audit CSS Usage**:
  `node QuilvianEngineeringSkills/agents/rules/frontend/tools/check-css-usage.mjs` atas berkas ini menghasilkan **0 temuan (PASS)**.
* **Kepatuhan Base Component**:
  Menggunakan `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `Pagination`, `StatusBadge`, `ConfirmModal`, dan tab navigasi terstandarisasi.
* **Cakupan Skenario UAT**:
  - `UAT-M12-07`: Tab calon pensiun memuat kandidat berdasarkan tanggal lahir di database master, bukan usia tebakan.
  - `UAT-M12-08`: Pegawai belum memenuhi syarat tidak dapat diajukan secara otomatis; pesan blokir kebijakan ditampilkan.
  - `UAT-M12-09`: Pengajuan rencana pensiun berhasil menyimpan data sah berstatus resmi di backend.
  - `UAT-M12-10`: Handoff sebelum persetujuan (`Approved`) ditolak tegas oleh server; setelah berstatus sah, handoff membentuk tepat satu proses pemisahan.
