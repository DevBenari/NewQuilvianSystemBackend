# ISSUE-036 — Ketiadaan Tombol Batal Pendaftaran pada Tahapan Sebelum Pemilihan Bed Admisi Bayi

```yaml
issue_id: ISSUE-EPS-036
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Admisi Rawat Inap — Langkah 1 s.d. 4 Admisi Bayi Baru Lahir (FE-INP-028)"
sumber_laporan: "Laporan pengguna 22 Oktober 2026: 'pada admisi bayi pada step sebelum pemilihan bed belum ada button batalkan admisi , coba ikuti admisi rawat inap. setahu ku , jika belum pemilihan bed maka bisa di batalkan tidak perlu alasan , coba di cek saja pada admisi rawat inap biasa pasien lama dan baru' — Petugas membutuhkan aksi pembatalan pendaftaran lokal pada langkah awal admisi bayi tanpa input alasan."
tanggal_issue: "2026-10-22"
status: SELESAI
tanggal_selesai: "2026-10-22"
keparahan_tertinggi: Sedang
source_sha_backend: "6406fc76 (MHamzah)"
source_sha_frontend: "381ec25fa (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-036-tombol-batal-pendaftaran-sebelum-pemilihan-bed-bayi.md
```

---

## 1. Ringkasan Masalah

Pada alur admisi rawat inap standar pasien lama dan pasien baru, setiap langkah sebelum pemilihan/pemesanan tempat tidur memiliki tombol **"Batal Pendaftaran"** (`variant="ghost"`) di sebelah kiri bilah aksi bawah. Tombol ini memicu dialog konfirmasi peringatan (`ConfirmModal`) yang membersihkan isian lokal dan mengembalikan petugas ke menu utama tanpa menuntut alasan pembatalan server.

Namun, pada alur **Admisi Bayi Baru Lahir**, tombol ini tidak tersedia pada Langkah 1 (*Pilih Ibu Persalinan*), Langkah 2 (*Identitas Bayi*), Langkah 3 (*Perawatan dan DPJP Bayi*), maupun Langkah 4 fase pemilihan tempat tidur (*Penempatan dan Pemesanan*). Petugas yang ingin membatalkan atau memulai ulang pendaftaran bayi terpaksa me-refresh peramban web atau menekan tombol kembali berulang kali.

---

## 2. Akar Masalah Teknis

1. **Prop `onResetAdmission` Terputus**:
   Pada `inpatient-admission-view.jsx`, fungsi penanganan reset lokal `handleOpenResetAdmission` tidak dioperasikan ke komponen-komponen langkah bayi (`NewbornContextStep`, `NewbornIdentityStep`, `NewbornCareStep`, dan `NewbornPlacementStep`).
2. **Helper `Actions` Tidak Merender Tombol Batal**:
   Komponen `Actions` pada `inpatient-newborn-admission-steps.jsx` hanya merender tombol `Kembali` dan `Simpan / Lanjut`.
3. **Sub-Langkah Pemilihan Bed**:
   Pada `NewbornPlacementStep`, sub-langkah sebelum pemesanan (`InpatientAdmissionBedSelectionStep`) tidak dipasangi prop `onResetAdmission`, sehingga tombol batal tidak muncul.

---

## 3. Batas Perilaku Bisnis yang Diperbaiki

- **Sebelum Pemilihan Bed / Reservasi Aktif (Langkah 1 s.d. 4 sebelum booking)**:
  Tombol **`Batal Pendaftaran`** tampil dengan varian `ghost` di kiri. Mengklik tombol memunculkan konfirmasi lokal *"Batalkan Pendaftaran?"* **tanpa memerlukan input alasan pembatalan**, karena belum ada episode aktif atau pemesanan tempat tidur di basis data server.
- **Sesudah Pemesanan Bed (Langkah 4 booking s.d. Konfirmasi)**:
  Tombol berubah menjadi **`Batalkan Admisi`** dengan varian `danger`, yang membuka modal pembatalan resmi server dengan **kewajiban mengisi alasan pembatalan** (minimal 10 karakter) untuk membatalkan episode dan melepas reservasi bed di bangsal.
