# Laporan Perubahan Frontend — `FE-IGD-072`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-IGD-072` |
| Judul | Penyelarasan Ruang Kerja Dokter IGD ke "Pasien Saya" dan Proteksi Role Dokter/SuperAdmin |
| Slice | Ruang Kerja Dokter IGD · `IGD-DEC-372` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) |
| Requirement | `IGD-DEC-372` |
| Keputusan | `IGD-DEC-372` (28 Oktober 2026) |
| Contract version | API **`0.22.0`**; validation **`0.22.0`** |
| Dependency | `BE-IGD-086` ✅ |
| Klasifikasi | `LIGHT` — 2 berkas di Ruang Kerja Dokter IGD (`doctor-emergency-view.jsx`, `use-emergency-physician-patients.jsx`) |
| Task mode | `FRONTEND` — izin pemilik (Rizki Gunawan, 28 Oktober 2026) |
| Target tulis | `QuilvianSystemFrontendDev` (branch `RizkiV2`) |
| Tanggal | 28 Oktober 2026 |
| Status | ✅ **DONE — 28 Oktober 2026: Lulus linter (0 error) dan integrasi state terverifikasi.** |

---

## 1. Masalah yang Diselesaikan

Berdasarkan kesepakatan `IGD-DEC-372`:
1. Ruang Kerja Dokter IGD harus difokuskan hanya untuk dokter yang sedang login, di mana daftar pasiennya 100% adalah **"Pasien Saya"** (`doctorId == dokter login`).
2. Saringan/toggle segmen *"Semua Pasien IGD"* ditiadakan dari tampilan dokter reguler agar ruang kerja bersih, rapi, dan mencegah dokter membuka pasien milik dokter jaga lain.
3. Hak SuperAdmin tetap dipertahankan untuk memantau dan berpindah filter bila diperlukan.

---

## 2. Rincian Perubahan Kode

1. **`use-emergency-physician-patients.jsx`:**
   * Membaca status SuperAdmin via selector canonical `selectIsSuperAdminPermission`.
   * Mengunci state `filter` secara permanen ke `EMERGENCY_PHYSICIAN_PATIENT_FILTER.MINE` untuk dokter non-SuperAdmin.
   * Menonaktifkan pemanggilan hitung pasien lain (`canCountOther`) untuk dokter reguler demi efisiensi panggilan API.

2. **`doctor-emergency-view.jsx`:**
   * Membaca `selectIsSuperAdminPermission`.
   * Menampilkan komponen `ClinicalSegmentedNav` hanya jika pengguna adalah `SuperAdmin`. Untuk dokter reguler, komponen ini tidak dirender sehingga antarmuka daftar pasien langsung menyajikan kotak pencarian dan daftar "Pasien Saya".

---

## 3. Bukti Verifikasi

* **Linter:** `npx eslint` dijalankan pada kedua berkas: **Exit code 0 (0 error, 0 warning)**.
* **Integrasi Backend:** Selaras dengan `BE-IGD-086` yang mengunci filter `doctorId` di tingkat server.
