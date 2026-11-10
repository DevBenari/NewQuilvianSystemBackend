# Laporan Perubahan Frontend — `FE-HRD-069`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-069` |
| Judul | Layar Pengaturan Parameter Lifecycle, Ambang Batas Pengingat, & Penomoran Dokumen (FE-HRD-137) |
| Slice | `S-C4-ONB (Onboarding dan Kontrak Kerja Pegawai RS) · MVP-16` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 3 S-C4-ONB |
| Trace | `HRD-DEC-097, VR-ONB-75 s.d. VR-ONB-80, BE-HRD-069` |
| Contract version | API Contract v9 bagian 15.7 (lifecycle-settings) |
| Dependency | `FE-HRD-058 (✅ Selesai), BE-HRD-069 (✅ Selesai)` |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/components/view/hr/lifecycle/lifecycle-settings-view.jsx, src/app/hr/lifecycle/settings/page.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Konfigurasi kebijakan rumah sakit terkait siklus kepegawaian: ambang batas pengingat kontrak habis, durasi default orientasi, dan pola nomor dokumen kontrak.

Task ini mengimplementasikan:
1. **Layar Pengaturan Siklus Kepegawaian (/hr/lifecycle/settings)**:
   - Form pengaturan batas hari pengingat kontrak (peringatan pertama: 60 hari, peringatan kedua: 30 hari, peringatan ketiga: 7 hari).
   - Validasi nilai rentang (VR-ONB-80): Mengunci batasan angka logis (misalnya peringatan kontrak harus antara 1 s.d. 180 hari).
   - Format pola nomor surat perjanjian kerja (misal: `[URUT]/RS-KTR/[BULAN]/[TAHUN]`).
   - Wewenang perubahan: Hanya dapat disunting oleh Kepala Bagian SDM / HR Director dengan audit log lengkap.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Validasi rentang nilai pengingat kontrak diterapkan ketat (VR-ONB-80) | ✅ PASS | Validasi form pada LifecycleSettingsView membatasi input pada rentang yang sah. |
| 2 | Formulir tersimpan ke endpoint API pengaturan lifecycle | ✅ PASS | Terhubung ke LifecycleService.saveSettings. |
| 3 | Penguncian hak akses administratif terpasang | ✅ PASS | Akses tombol simpan dilindungi oleh wewenang Administrator HR. |
| 4 | Bebas Tailwind dan mematuhi aturan token Quilvian | ✅ PASS | 100% menggunakan CSS Module dan Base Component. |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/lifecycle/lifecycle-settings-view.jsx (Form pengaturan parameter & notifikasi)`
- `src/app/hr/lifecycle/settings/page.jsx (Rute App Router pengaturan lifecycle)`
