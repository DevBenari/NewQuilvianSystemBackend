# Laporan Penyelesaian Task: FE-HRD-310 Eliminasi Mock & Server-Only Severance Calculation Layar Pemberhentian (termination-list-view.jsx)

## 1. Informasi Task
* **Task ID**: `FE-HRD-310`
* **Slice / MVP**: `S-C5-SEP` (Submenu 4: Pemberhentian / Termination / PHK)
* **Nama Fitur**: Eliminasi Mock & Server-Only Severance Calculation Layar Pemberhentian (`termination-list-view.jsx`)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-16
* **Penanggung Jawab**: Frontend Engineering Agent

---

## 2. Ringkasan Eksekutif & Kebutuhan Bisnis Rumah Sakit
Pemutusan Hubungan Kerja (PHK) di lingkungan rumah sakit—baik karena pelanggaran disiplin berat (SP3), ketidakmampuan medis permanen, maupun restrukturisasi unit—merupakan keputusan hukum berisiko tinggi. Kesalahan atau manipulasi rumus uang pesangon (UP), uang penghargaan masa kerja (UPMK), dan uang penggantian hak (UPH) dapat menimbulkan gugatan Perselisihan Hubungan Industrial (PHI) dan sanksi audit keuangan.

Sebelum pelaksanaan task ini:
1. Layar `termination-list-view.jsx` menyimpan data demo `DEFAULT_FALLBACK_TERMINATIONS`.
2. Kalkulasi pesangon dijalankan dengan rumus hardcode di sisi browser (`baseSalary * Math.min(yearsOfService, 9)` + formula sepihak) yang berisiko tidak sesuai dengan variasi pasal dalam PP 35/2021 (faktor pengali 0.5x, 0.75x, 1x, atau 2x tergantung alasan PHK).
3. Tombol dan modal persetujuan Direktur Keuangan dapat dimanipulasi dengan fake toast pada blok penanganan error (`catch`).

Melalui implementasi `FE-HRD-310`:
* **Pembersihan Total Data Fiktif**: Seluruh rekaman contoh dieliminasi, menjamin empty state bersih saat belum ada kasus PHK.
* **Kalkulasi Pesangon Murni Server-Side**: Logika kalkulasi lokal di browser dihapus seutuhnya. Antarmuka kini memanggil `SeparationService.calculateSeverance()` ke backend API `SeveranceCalculationEngine` yang secara otoritatif menghitung rincian UP, UPMK, UPH, dan faktor pasal regulasi PP 35/2021 berdasarkan alasan PHK dan masa kerja resmi pegawai. Jika API gagal atau koneksi terputus, antarmuka dengan jujur menampilkan indikator kegagalan kalkulasi tanpa memunculkan angka simulasi tebakan.
* **Pengamanan Hak Otorisasi Keuangan**: Tombol dan modal persetujuan Finance dilindungi gerbang izin `Hrd.Separation.FinanceApprove`. Mutasi hanya berhasil bila backend memvalidasi sesi dan menyimpan riwayat audit pejabat penyetuju.

---

## 3. Rincian Implementasi Teknis

### 3.1 Berkas yang Dimodifikasi
* `QuilvianSystemFrontendDev/src/components/view/hr/separation/termination-list-view.jsx`

### 3.2 Poin-Poin Perbaikan Kode
1. **Penghapusan Fallback State**:
   ```javascript
   // Sebelum: const [terminations, setTerminations] = useState(DEFAULT_FALLBACK_TERMINATIONS);
   // Sesudah:
   const [terminations, setTerminations] = useState([]);
   ```
2. **Kalkulator Pesangon Terhubung ke Engine Backend**:
   Menghapus seluruh rumus kalkulasi matematika lokal di komponen form:
   ```javascript
   const handleCalculateSeverance = async () => {
     if (!formData.employeeId || !formData.reason) return;
     try {
       setCalculating(true);
       const result = await SeparationService.calculateSeverance({
         employeeId: formData.employeeId,
         separationReason: formData.reason,
         yearsOfService: calculatedYears
       });
       setSeveranceData(result);
     } catch (err) {
       setSeveranceData(null);
       pushToast(err?.response?.data?.message || 'Gagal menghitung pesangon dari server.', 'error');
     } finally {
       setCalculating(false);
     }
   };
   ```
3. **Pembersihan Blok Catch pada Persetujuan Keuangan**:
   Menghapus manipulasi status lokal dan toast sukses palsu:
   ```javascript
   } catch (err) {
     const errorMsg = err?.response?.data?.message || err?.message || 'Gagal menyetujui terminasi.';
     pushToast(errorMsg, 'error');
   }
   ```
4. **Siklus Refetch & Validasi Komponen**:
   Menggunakan `ResourceFilterSelect` untuk memilih pegawai target yang terkena PHK, validasi alasan terminasi, dan otomatis menjalankan pemuatan ulang data via `fetchData()` setelah transaksi selesai.

---

## 4. Bukti Verifikasi Mutu Rekayasa
* **Pemeriksaan Linter CI/CD**:
  `npx eslint src/components/view/hr/separation/termination-list-view.jsx` ➔ **0 errors, 0 warnings (PASS)**.
* **Audit CSS Usage**:
  `node QuilvianEngineeringSkills/agents/rules/frontend/tools/check-css-usage.mjs` atas berkas ini menghasilkan **0 temuan (PASS)**.
* **Kepatuhan Base Component**:
  Menggunakan `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `Pagination`, `StatusBadge`, `ConfirmModal`, dan komponen terstandarisasi.
* **Cakupan Skenario UAT**:
  - `UAT-M12-15`: Inisiasi PHK dari data master pegawai uji menghasilkan nomor register sah dari server.
  - `UAT-M12-16`: Rincian nominal pesangon dihitung murni dari engine backend; tidak ada angka estimasi buatan peramban bila API gagal.
  - `UAT-M12-17`: Persetujuan Finance oleh role berwenang tersimpan lengkap dengan jejak audit; percobaan oleh pengguna non-Finance ditolak dengan status HTTP 403.
  - `UAT-M12-18`: Pengajuan ganda atau status sengketa dicegah oleh pengamanan integritas data server.
