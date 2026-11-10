# Laporan Perubahan Frontend — `FE-HRD-063`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-063` |
| Judul | Layar Daftar & Detail Kontrak Kerja Pegawai Rumah Sakit (FE-HRD-126, FE-HRD-127) |
| Slice | `S-C4-ONB (Onboarding dan Kontrak Kerja Pegawai RS) · MVP-14` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 3 S-C4-ONB |
| Trace | `HRD-DEC-039, HRD-DEC-040, HRD-DEC-091, HRD-DEC-092, VR-ONB-26 s.d. VR-ONB-34, BE-HRD-072` |
| Contract version | API Contract v9 bagian 15.2 (employment-contracts) |
| Dependency | `FE-HRD-058 (✅ Selesai), BE-HRD-072 (✅ Selesai)` |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/components/view/hr/lifecycle/employment-contracts-list-view.jsx, src/components/view/hr/lifecycle/employment-contract-detail-view.jsx, src/app/hr/lifecycle/employment-contracts/page.jsx, src/app/hr/lifecycle/employment-contracts/[id]/page.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Pengelolaan perjanjian kerja tenaga kesehatan dan staf penunjang rumah sakit (PKWT, PKWTT, Klinis Penuh/Paruh Waktu) dengan perlindungan kerahasiaan nominal remunerasi.

Task ini mengimplementasikan:
1. **Layar Daftar Kontrak Kerja (/hr/lifecycle/employment-contracts)**:
   - Tabel responsif memuat nomor kontrak, nama pegawai, unit kerja, jabatan, jenis kontrak (PKWT, PKWTT, dsb.), tanggal mulai, tanggal berakhir, dan status legal.
   - Filter pencarian cepat berdasarkan status (DRAFT, ACTIVE, EXPIRING_SOON, TERMINATED, AMENDED) dan unit kerja.
   - Penanda visual otomatis untuk kontrak yang mendekati jatuh tempo (expiring in <= 60 days).
2. **Layar Detail Kontrak Kerja (/hr/lifecycle/employment-contracts/[id])**:
   - Informasi lengkap pasal-pasal perjanjian kerja, hak cuti tahunan, jam kerja, dan kewajiban profesional.
   - Penerapan aturan kerahasiaan gaji (HRD-DEC-040): nominal gaji pokok dan tunjangan dimasking ("Rp ••••••••") kecuali pengguna memiliki wewenang finansial khusus (amountVisible = true). Nominal tidak pernah disimpan pada Redux store publik atau localStorage.
   - Tombol pratinjau cetak naskah kontrak kerja dengan template resmi RS.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Daftar kontrak menampilkan filter status dan unit secara akurat (VR-ONB-26) | ✅ PASS | EmploymentContractsListView memuat query parameter status dan search filter. |
| 2 | Kerahasiaan data gaji diterapkan secara ketat (HRD-DEC-040) | ✅ PASS | Fungsi masking gaji pada detail view mengaburkan nominal jika amountVisible false. |
| 3 | Status badge dan peringatan masa berlaku terpasang | ✅ PASS | LifecycleDisplayUtils memetakan badge warna sesuai siklus hidup kontrak. |
| 4 | Desain bersih bebas Tailwind dan ramah cetak | ✅ PASS | Menggunakan layout CSS Module terstruktur dengan modal pratinjau cetak. |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/lifecycle/employment-contracts-list-view.jsx (Tabel daftar kontrak & filter)`
- `src/components/view/hr/lifecycle/employment-contract-detail-view.jsx (Detail kontrak, pasal, masking gaji, & pratinjau)`
- `src/app/hr/lifecycle/employment-contracts/page.jsx (Rute App Router daftar kontrak)`
- `src/app/hr/lifecycle/employment-contracts/[id]/page.jsx (Rute App Router detail kontrak)`
