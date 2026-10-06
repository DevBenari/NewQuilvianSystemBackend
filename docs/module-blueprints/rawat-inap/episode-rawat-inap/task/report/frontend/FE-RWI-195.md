# Laporan Implementasi Task FE-RWI-195: Catatan Pra-Operasi Dua Sisi Berversi

## Metadata Task
- **Task ID**: `FE-RWI-195`
- **Modul**: Rawat Inap (`inpatient-management`) & Kamar Operasi (`operating-room-management`)
- **Status**: Selesai
- **Requirement / Keputusan**: `FR-RWF-045`, `FR-RWF-048`, `FR-RWF-090`, `RWI-DEC-173`, `RWI-DEC-174`, `RWI-DEC-199`, `AC-RWF-042`, `AC-RWF-045`, `AC-RWF-046`, `AC-RWF-093`, `AC-RWF-094`, `UAT-RWF-21`
- **Kontrak**: Frontend 13.2, 13.4.3 (`FE-INP-27`), API 11.3
- **Pemilik**: Muhammad Hamzah

---

## 1. Ringkasan Perubahan
1. **API Service (`ward-pre-op.service.js`)**:
   - Membangun service untuk endpoint `/v1/health-services/operating-room-management/cases/{caseId}/preparation/ward-pre-op`:
     - `getWardPreOp(caseId)`: mengambil catatan pra-operasi versi aktif/terbaru atau templat.
     - `getWardPreOpVersions(caseId)`: mengambil riwayat seluruh versi.
     - `saveWardPreOpDraft(caseId, payload)`: menyimpan draf atau membuat versi baru jika versi sebelumnya perlu diperbarui.
     - `sendWardPreOp(caseId, payload, idempotencyKey)`: mengirim draf ke Kamar Operasi dengan Idempotency-Key.
     - `confirmWardPreOp(caseId, payload, idempotencyKey)`: mengonfirmasi butir dan penandaan pra-operasi di Kamar Operasi.
2. **Laci Catatan Pra-Operasi Berversi (`ward-pre-op-drawer.jsx`)**:
   - Checklist Persiapan per Kelompok:
     - Dikelompokkan berdasarkan nama kelompok master data.
     - Bila master kosong, menampilkan teks "Butir persiapan belum diatur di Master Data."
     - Menampilkan indikator badge wajib dan checkbox verifikasi pengirim & penerima.
   - Potret Tanda Vital & Nyeri:
     - Bila tanda vital belum ada, menampilkan peringatan dan menonaktifkan tombol Kirim ("Catat tanda vital pasien lebih dulu").
     - Menampilkan TD, Nadi, RR, Suhu, SpO2, dan Nyeri dari snapshot beku.
   - Penandaan Area Operasi & Sisi Tubuh (Tanpa Unggah Foto):
     - Pilihan sisi tubuh: Left, Right, Bilateral, NotApplicable.
     - Jika sisi penandaan tidak konsisten dengan pesanan kasus (`caseLaterality`), menampilkan pesan peringatan `OPR-WPO-001` tepat di bawah pilihan sisi.
     - Kanvas interaktif penandaan titik tubuh koordinat grafis (0–100%) tanpa unggah berkas gambar.
   - Banner Kasus Ditunda & Pembuatan Versi Baru (`UAT-RWF-21`):
     - Saat status catatan adalah `NeedsUpdate` (4), menampilkan banner kuning "Perlu diperbarui setelah penundaan" dan tombol "Buat versi baru".
   - Aturan Akun Pengirim vs Penerima (`OPR-WPO-002`):
     - Akun pengirim tidak dapat mengonfirmasi catatannya sendiri; menampilkan teks pemberitahuan "Konfirmasi harus oleh akun lain (OPR-WPO-002)" dan tombol konfirmasi disembunyikan.
3. **Integrasi Ruang Kerja Keperawatan**:
   - Aksi baris "Pra-operasi" pada `patient-surgery-cases-table.jsx` membuka `WardPreOpDrawer` di dalam `surgery-booking-section.jsx`.

---

## 2. Bukti Verifikasi
- **Unit Test**: `tests/unit/ward-pre-op.test.mjs`
  - Hasil: `3 passed, 0 failed` (100% lulus).
  - Menguji keutuhan fungsi service kontrak API 11.3, validasi ketiadaan tanda vital, pesan master kosong, deteksi ketidaksesuaian sisi OPR-WPO-001, proteksi akun pengirim OPR-WPO-002, banner status NeedsUpdate UAT-RWF-21, dan integrasi drawer.
