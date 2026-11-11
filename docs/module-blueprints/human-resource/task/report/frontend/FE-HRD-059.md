# Laporan Perubahan Frontend — `FE-HRD-059`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-059` |
| Judul | Layar Pengelolaan Template Onboarding (`FE-HRD-132`, `FE-HRD-133`) & Registrasi Menu Sidebar |
| Slice | `S-C4-ONB` (Onboarding dan Kontrak Kerja Pegawai RS) · `MVP-14` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 2 S-C4-ONB |
| Trace | `HRD-DEC-086`, `03-frontend-architecture.md` 14.2 & 14.3.7, `VR-ONB-20` s.d. `VR-ONB-25`, `BE-HRD-070` |
| Contract version | API Contract v9 bagian 15.2 grup `Onboarding Templates` |
| Dependency | `FE-HRD-058` (✅ Selesai), `BE-HRD-070` (✅ Selesai) |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/lifecycle/onboarding-templates/page.jsx`, `src/app/hr/lifecycle/onboarding-templates/[id]/page.jsx`, `src/components/view/hr/lifecycle/onboarding-templates-list-view.jsx`, `src/components/view/hr/lifecycle/onboarding-template-detail-view.jsx`, `src/utils/menu-sidebar/menu-items.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Setiap pegawai baru yang masuk ke rumah sakit membutuhkan daftar periksa (checklist) orientasi yang terstruktur agar tidak ada syarat kepatuhan hukum, fasilitas kerja, atau pelatihan keselamatan pasien yang terlewat.

Task ini mengimplementasikan pengelolaan template orientasi 3-lapis:
1. **Daftar & Pembuatan Template Onboarding (`/hr/lifecycle/onboarding-templates`)**:
   - Menampilkan daftar template orientasi yang aktif, jumlah butir periksa, dan tanggal pembaruan.
   - Modal pembuatan template baru untuk peruntukan unit atau kelompok profesi tertentu (misal: Keperawatan, Penunjang Medis, Staf Administrasi).
2. **Editor Butir Checklist 3-Lapis (`/hr/lifecycle/onboarding-templates/[id]`)**:
   - Tiga lapis orientasi: Lapis 1 (Berkas HR), Lapis 2 (Fasilitas & Akses IT/SIMRS), Lapis 3 (Orientasi Unit Kerja).
   - Penegakan **Butir Sistem Terkunci**: Butir wajib standar RS (verifikasi STR, tanda tangan kontrak, ID card, orientasi SPO) ditandai dengan badge "🔒 Butir Sistem" dan tidak dapat dihapus atau diubah jenisnya (`VR-ONB-20` s.d. `VR-ONB-25`).
   - Penambahan butir kustom unit dengan flag wajib (*mandatory*) dan kewajiban upload bukti lampiran.
3. **Registrasi Menu Sidebar**:
   - Mendaftarkan grup navigasi "Onboarding & Kontrak" pada sidebar utama portal HRD.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Grup menu terdaftar di sidebar di bawah HRD dan butir menu terhubung ke rute template | ✅ PASS | Terbukti pada `src/utils/menu-sidebar/menu-items.jsx` grup `corporateLifecycleGroup`. |
| 2 | Butir sistem tidak dapat dihapus atau diubah jenisnya di layar (`VR-ONB-20` s.d. `VR-ONB-25`) | ✅ PASS | Terbukti pada `onboarding-template-detail-view.jsx` tombol hapus disembunyikan untuk butir dengan `isSystem: true`. |
| 3 | Pesan validasi tampil informatif di dekat tombol simpan | ✅ PASS | Menggunakan `InformationAlert` dan notifikasi feedback inline. |
| 4 | Kepatuhan Aturan Bebas Tailwind | ✅ PASS | Menggunakan token CSS variables dan komponen standar Quilvian (`BaseButton`, `Hero`, `DataTable`). |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/lifecycle/onboarding-templates-list-view.jsx` (Komponen daftar template)
- `src/components/view/hr/lifecycle/onboarding-template-detail-view.jsx` (Komponen konfigurasi 3-lapis template)
- `src/app/hr/lifecycle/onboarding-templates/page.jsx` (Rute list App Router)
- `src/app/hr/lifecycle/onboarding-templates/[id]/page.jsx` (Rute detail App Router)
- `src/utils/menu-sidebar/menu-items.jsx` (Registrasi grup sidebar Onboarding & Kontrak)
