# Laporan Penyelesaian Task: FE-HRD-329 Eliminasi Mock & Refactor Layar Pengunduran Diri Data Nyata (resignation-list-view.jsx)

## 1. Informasi Task
* **Task ID**: `FE-HRD-329`
* **Slice / MVP**: `S-C5-SEP` (Submenu 1: Pengunduran Diri / Resignation)
* **Nama Fitur**: Eliminasi Mock & Refactor Layar Pengunduran Diri Data Nyata (`resignation-list-view.jsx`)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-16
* **Penanggung Jawab**: Frontend Engineering Agent

---

## 2. Ringkasan Eksekutif & Kebutuhan Bisnis Rumah Sakit
Pengunduran diri (resignation) tenaga medis, perawat, atau staf penunjang rumah sakit memerlukan tata kelola administrasi yang sangat akurat. Jadwal dinas bangsal 24/7 dan keselamatan pasien sangat bergantung pada kepastian tanggal efektif pengunduran diri serta masa pemberitahuan (*one month notice*).

Sebelum implementasi task ini, tampilan antarmuka `resignation-list-view.jsx` masih memuat konstanta data contoh `DEFAULT_FALLBACK_RESIGNATIONS` (rekaman demo `res-demo-001`, `res-demo-002`, `res-demo-003`). Keberadaan data tiruan ini berbahaya karena staf HRD dapat mengira ada proses pengunduran diri aktif yang harus ditindaklanjuti padahal tidak ada transaksi nyata di database. Selain itu, penanganan blok `catch` pada fungsi mutasi sebelumnya memalsukan pembaruan status lokal (*fake optimistic update*) dan memicu toast sukses palsu meskipun jaringan terputus atau server mengembalikan error.

Melalui perbaikan pada `FE-HRD-329`:
1. **Pembersihan Total Data Fiktif**: Seluruh array fallback `DEFAULT_FALLBACK_RESIGNATIONS` dan ID tiruan telah dieliminasi secara tuntas dari berkas antarmuka.
2. **Kejujuran Antarmuka Zero-Data State**: Ketika database rumah sakit belum memiliki catatan pengunduran diri pada periode berjalan, antarmuka dengan jujur menyajikan *empty state* informatif (0 data), bukan menampilkan pegawai contoh fiktif.
3. **Penegakan Validasi Server & Error Handling Riil**: Seluruh aksi mutasi (pengajuan, persetujuan dengan catatan, penolakan beralasan, serta handoff ke checklist offboarding) hanya akan memperbarui antarmuka pengguna apabila server ASP.NET Core memberikan respons sukses `200 OK`. Jika terjadi penolakan hak akses (`403 Forbidden`) atau konflik bisnis (`409 Conflict`), modal dialog tetap terbuka dengan pesan kesalahan asli dari server tanpa kehilangan input pengguna.

---

## 3. Rincian Implementasi Teknis

### 3.1 Berkas yang Dimodifikasi
* `QuilvianSystemFrontendDev/src/components/view/hr/separation/resignation-list-view.jsx`

### 3.2 Poin-Poin Perbaikan Kode
1. **Eliminasi Fallback Array**:
   Menghapus definisi `DEFAULT_FALLBACK_RESIGNATIONS` beserta seluruh referensi fallback di `fetchData` dan handler aksi. Data state `resignations` dan `summary` diinisialisasi murni dari hasil API:
   ```javascript
   // Sebelum: const items = res?.items || DEFAULT_FALLBACK_RESIGNATIONS;
   // Sesudah:
   const items = Array.isArray(res?.items) ? res.items : (Array.isArray(res) ? res : []);
   setResignations(items);
   ```
2. **Pembersihan Blok Catch dari Fake Toast**:
   Pada fungsi penanganan pengajuan (`handleCreateSubmit`), persetujuan (`handleApproveConfirm`), penolakan (`handleRejectConfirm`), dan handoff (`handleHandoffConfirm`), kode lama yang melakukan mutasi array lokal dan menampilkan toast `"disetujui (offline fallback)"` telah dihapus total. Sebagai gantinya, error diekstrak secara semantik:
   ```javascript
   } catch (err) {
     const errorMsg = err?.response?.data?.message || err?.message || 'Gagal menyetujui pengajuan.';
     pushToast(errorMsg, 'error');
   }
   ```
3. **Penyelarasan Komponen Input**:
   Menggunakan `ResourceFilterSelect` untuk pemilihan pegawai aktif rumah sakit yang terdaftar sah pada master data, mencegah pengajuan atas nama ID sembarang.
4. **Siklus Refetch Otomatis**:
   Setelah setiap mutasi berhasil divalidasi server, antarmuka secara otomatis menjalankan `fetchData()` untuk memuat ulang daftar dan ringkasan metrik secara sinkron langsung dari database backend.

---

## 4. Bukti Verifikasi Mutu Rekayasa
* **Pemeriksaan Linter CI/CD**:
  `npx eslint src/components/view/hr/separation/resignation-list-view.jsx` ➔ **0 errors, 0 warnings (PASS)**.
* **Audit CSS Usage**:
  `node QuilvianEngineeringSkills/agents/rules/frontend/tools/check-css-usage.mjs` atas berkas ini menghasilkan **0 temuan (PASS)**.
* **Kepatuhan Base Component**:
  Menggunakan `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `Pagination`, `StatusBadge`, `ConfirmModal`, dan `InformationAlert` standar Quilvian tanpa custom inline style atau kelas utilitas Tailwind yang dilarang.
* **Cakupan Skenario UAT**:
  Memenuhi acceptance criteria untuk skenario:
  - `UAT-M12-01`: GET list saat DB 0 menghasilkan tabel kosong jujur (bukan data demo).
  - `UAT-M12-02`: Error jaringan / 403 menampilkan notifikasi error tanpa memunculkan record Ahmad/Dewi contoh.
  - `UAT-M12-03`: Create pengajuan sukses menyimpan record sah dan terbaca ulang saat reload.
  - `UAT-M12-04`: Persetujuan dan penolakan hanya berhasil jika divalidasi backend.
  - `UAT-M12-05`: Handoff ganda dicegah oleh verifikasi status server.
  - `UAT-M12-06`: Validasi notice period dan form invalid memunculkan pesan error jelas dari server.
