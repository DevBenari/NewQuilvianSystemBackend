# Laporan Penyelesaian Task: FE-HRD-331 Eliminasi Mock & Filter Horizon Jatuh Tempo Layar Akhir Kontrak (contract-expiry-view.jsx)

## 1. Informasi Task
* **Task ID**: `FE-HRD-331`
* **Slice / MVP**: `S-C5-SEP` (Submenu 3: Akhir Kontrak / Contract Expiry)
* **Nama Fitur**: Eliminasi Mock & Filter Horizon Jatuh Tempo Layar Akhir Kontrak (`contract-expiry-view.jsx`)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-16
* **Penanggung Jawab**: Frontend Engineering Agent

---

## 2. Ringkasan Eksekutif & Kebutuhan Bisnis Rumah Sakit
Pengelolaan tenaga kerja dengan status Perjanjian Kerja Waktu Tertentu (PKWT) di lingkungan rumah sakit—seperti perawat kontrak, radiografer, petugas laboratorium, dan tenaga administrasi—tunduk pada regulasi ketenagakerjaan ketat (UU Cipta Kerja dan PP 35/2021). Manajemen rumah sakit wajib memantau masa berlaku kontrak setidaknya 30, 60, atau 90 hari sebelum jatuh tempo untuk memutuskan apakah kontrak diperpanjang, dialihkan menjadi pegawai tetap (PKWTT), atau diakhiri dengan pemberian uang kompensasi PKWT.

Sebelum implementasi task ini:
1. Layar `contract-expiry-view.jsx` mengandalkan array dummy `DEFAULT_FALLBACK_CONTRACTS` (`ctr-demo-001`, `ctr-demo-002`, `ctr-demo-003`).
2. Pemeriksaan masa kontrak (*fixed-term evaluation*) hanya dijalankan secara simulasi di sisi peramban dengan asumsi default 24 atau 36 bulan tanpa menghitung riwayat adendum nyata.
3. Filter rentang waktu jatuh tempo tidak terhubung ke parameter query backend `ExpiringInDays`.
4. Aksi keputusan pembaharuan kontrak di sisi klien menampilkan notifikasi sukses palsu di blok `catch` saat API backend mengalami kendala.

Melalui perbaikan pada `FE-HRD-331`:
* **Pembersihan Total Data Tiruan**: Konstanta `DEFAULT_FALLBACK_CONTRACTS` telah dihapus seutuhnya. Jika belum ada kontrak yang mendekati jatuh tempo pada horizon yang dipilih, layar jujur menampilkan 0 data.
* **Filter Horizon 30/60/90 Hari Riil**: Parameter filter horizon jatuh tempo (`expiringInDays: 30`, `60`, `90`) diteruskan langsung ke backend API `SeparationService.getExpiringContracts()` untuk menyaring rekaman database riil.
* **Evaluasi Legal Server-Side**: Pemeriksaan masa berlaku kontrak PKWT dan rekomendasi hukum ketenagakerjaan bersumber dari kalkulasi valid backend tanpa asumsi batas durasi fiktif.
* **Modal Keputusan T16 Berintegritas**: Modal penetapan keputusan (perpanjangan kontrak vs pengakhiran kerja) hanya akan memperbarui status di antarmuka apabila permintaan mutasi berhasil di-*commit* oleh backend.

---

## 3. Rincian Implementasi Teknis

### 3.1 Berkas yang Dimodifikasi
* `QuilvianSystemFrontendDev/src/components/view/hr/separation/contract-expiry-view.jsx`

### 3.2 Poin-Poin Perbaikan Kode
1. **Eliminasi Fallback Array & Penataan State**:
   ```javascript
   // Sebelum: const items = res?.items || DEFAULT_FALLBACK_CONTRACTS;
   // Sesudah:
   const items = Array.isArray(res?.items) ? res.items : (Array.isArray(res) ? res : []);
   setContracts(items);
   ```
2. **Penyambungan Parameter Horizon ke Backend**:
   Filter status dan horizon jatuh tempo diteruskan ke service:
   ```javascript
   const res = await SeparationService.getExpiringContracts({
     status: activeTab === 'all' ? undefined : activeTab,
     expiringInDays: horizonDays,
     search: searchTerm,
     page: currentPage,
     pageSize
   });
   ```
3. **Pembersihan Simulasi Check & Fake Toast**:
   Fungsi evaluasi kontrak (`handleRunCheck`) dan penyimpanan keputusan (`handleSaveDecision`) telah dibersihkan dari pembaruan array palsu di dalam blok `catch`:
   ```javascript
   } catch (err) {
     const errorMsg = err?.response?.data?.message || err?.message || 'Gagal menyimpan keputusan kontrak.';
     pushToast(errorMsg, 'error');
   }
   ```
4. **Optimasi React Hook (`useCallback` & `useMemo`)**:
   Fungsi handler modal dibungkus dalam `useCallback` dan dimasukkan ke dalam daftar dependensi `columns` untuk mencegah re-render berlebih dan menjamin kepatuhan aturan React Hooks.

---

## 4. Bukti Verifikasi Mutu Rekayasa
* **Pemeriksaan Linter CI/CD**:
  `npx eslint src/components/view/hr/separation/contract-expiry-view.jsx` ➔ **0 errors, 0 warnings (PASS)**.
* **Audit CSS Usage**:
  `node QuilvianEngineeringSkills/agents/rules/frontend/tools/check-css-usage.mjs` atas berkas ini menghasilkan **0 temuan (PASS)**.
* **Kepatuhan Base Component**:
  Menggunakan `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `Pagination`, `StatusBadge`, `ConfirmModal`, dan filter terintegrasi.
* **Cakupan Skenario UAT**:
  - `UAT-M12-11`: Penyaringan kontrak berdasarkan horizon 30, 60, dan 90 hari menampilkan data riil dari database server.
  - `UAT-M12-12`: Evaluasi kelayakan perpanjangan kontrak menampilkan masa kerja akurat dari backend tanpa asumsi durasi sepihak.
  - `UAT-M12-13`: Keputusan perpanjangan (Extend) tidak memicu terminasi; keputusan pengakhiran (NotRenew) meneruskan proses ke offboarding secara sah.
  - `UAT-M12-14`: Riwayat keputusan dan notifikasi tersimpan permanen di database dan terbaca kembali setelah pengguna logout dan login ulang.
