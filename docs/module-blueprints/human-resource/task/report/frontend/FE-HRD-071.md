# Laporan Perubahan Frontend — `FE-HRD-071`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-071` |
| Judul | Penyesuaian Kompatibilitas Layar Eksisting: Tab Riwayat Kontrak Read-Only & Tautan Navigasi Modul Terpadu |
| Slice | `S-C4-ONB (Onboarding dan Kontrak Kerja Pegawai RS) · MVP-14` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 3 S-C4-ONB |
| Trace | `HRD-DEC-039, VR-ONB-28, BE-HRD-065` |
| Contract version | API Contract v9 bagian 15.2 (read-only contract reflection) |
| Dependency | `FE-HRD-063 (✅ Selesai), BE-HRD-065 (✅ Selesai)` |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/components/features/workforce/workforce-profile-editor-view.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Pencegahan konflik data dan pemeliharaan integritas arsitektur dengan menjadikan tab riwayat kontrak pada profil master pegawai lama bersifat baca-saja (read-only) dan mengarahkannya ke modul kontrak resmi.

Task ini mengimplementasikan:
1. **Penguncian Tab Kontrak pada Workforce Profile Editor**:
   - Menonaktifkan form input/tambah kontrak manual lama di `workforce-profile-editor-view.jsx` (VR-ONB-28).
   - Menampilkan spanduk pemberitahuan informasi (InformationAlert) yang mengumumkan bahwa seluruh pengelolaan perjanjian kerja, perpanjangan, dan pengesahan telah dialihkan ke Modul Onboarding & Kontrak Kerja.
   - Menyediakan tombol tautan navigasi langsung "Kelola Kontrak Kerja di Modul Resmi →" menuju `/hr/lifecycle/employment-contracts`.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Tab kontrak pada layar profil pegawai terkunci menjadi baca-saja (VR-ONB-28) | ✅ PASS | WorkforceProfileEditorView mengunci input kontrak lama dan menghapus form edit manual. |
| 2 | Spanduk pemberitahuan dan tautan navigasi resmi terpasang | ✅ PASS | Menampilkan InformationAlert dan tautan router ke /hr/lifecycle/employment-contracts. |
| 3 | Tidak ada regresi fungsi pada tab lain di profil pegawai | ✅ PASS | Tab informasi umum, keluarga, pendidikan, dan penempatan tetap berfungsi normal. |
| 4 | Bebas Tailwind dan mematuhi aturan token Quilvian | ✅ PASS | Memanfaatkan Base Component dan InformationAlert terstandar. |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/features/workforce/workforce-profile-editor-view.jsx (Penguncian tab kontrak & integrasi banner navigasi)`
