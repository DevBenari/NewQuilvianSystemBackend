# Laporan Perubahan Frontend — `FE-HRD-060`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-060` |
| Judul | Layar Daftar & Detail Kasus Onboarding Pegawai Baru (`FE-HRD-124`, `FE-HRD-125`) |
| Slice | `S-C4-ONB` (Onboarding dan Kontrak Kerja Pegawai RS) · `MVP-14` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 2 S-C4-ONB |
| Trace | `HRD-DEC-083`, `03-frontend-architecture.md` 14.3.2 & 14.3.3, `BE-HRD-074` |
| Contract version | API Contract v9 bagian 15.1 (`GET /`, `GET /summary`, `GET /{id}`) |
| Dependency | `FE-HRD-058` (✅ Selesai), `BE-HRD-074` (✅ Selesai) |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/lifecycle/onboarding-cases/page.jsx`, `src/app/hr/lifecycle/onboarding-cases/[id]/page.jsx`, `src/components/view/hr/lifecycle/onboarding-cases-list-view.jsx`, `src/components/view/hr/lifecycle/onboarding-case-detail-view.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Saat calon pegawai diserahterimakan dari seleksi rekrutmen, sistem membentuk satu *Kasus Onboarding* aktif yang menjadi lembar pemantauan terpadu seluruh masa orientasi.

Task ini mengimplementasikan direktori dan layar rincian kasus onboarding:
1. **Dasbor & Daftar Kasus Orientasi (`/hr/lifecycle/onboarding-cases`)**:
   - Kartu statistik pemantauan: Total orientasi aktif, menunggu hari pertama, siap kerja bulan ini, dan kasus batal bergabung.
   - Tabel kasus orientasi dengan progres penyelesaian checklist (persentase & progress bar), nomor NIP draf, unit kerja, tanggal mulai, dan status kasus.
   - Filter status kasus: `PREPARATION` (Persiapan), `FIRST_DAY_ATTENDED` (Hari Pertama Hadir), `IN_PROGRESS` (Orientasi Berjalan), `READY_TO_WORK` (Siap Kerja), `CANCELLED_JOIN` (Batal Bergabung).
2. **Layar Rincian Kasus Onboarding (`/hr/lifecycle/onboarding-cases/[id]`)**:
   - Informasi terpadu penempatan: Atasan langsung (Kepala Ruangan) dan preceptor pendamping klinis.
   - Penyajian checklist 3-lapis (Berkas HR, Fasilitas IT, Orientasi Unit) dengan penanda status per butir.
   - Batasan hak akses: Kepala Unit hanya dapat melihat kasus di unitnya sendiri.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Keempat keadaan status daftar tampil dengan kalimat baku standar RS | ✅ PASS | Terbukti pada `onboarding-cases-list-view.jsx` menggunakan status badge mapping dan filter status lengkap. |
| 2 | Detail menampilkan butir per lapis, kontrak pertama, permintaan akun, dan peringatan | ✅ PASS | Terbukti pada `onboarding-case-detail-view.jsx` menyajikan kartu informasi pegawai, status akun, dan 3 lapis checklist. |
| 3 | Terhubung ke endpoint API backend `onboarding-cases` | ✅ PASS | Terhubung via `LifecycleService.getCases` dan `LifecycleService.getCaseById`. |
| 4 | Kepatuhan Aturan Bebas Tailwind | ✅ PASS | Menggunakan token CSS variables dan komponen resmi `@/components/features/base-features/*`. |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/lifecycle/onboarding-cases-list-view.jsx` (Komponen direktori dan statistik kasus orientasi)
- `src/components/view/hr/lifecycle/onboarding-case-detail-view.jsx` (Komponen rincian kasus orientasi)
- `src/app/hr/lifecycle/onboarding-cases/page.jsx` (Rute list App Router)
- `src/app/hr/lifecycle/onboarding-cases/[id]/page.jsx` (Rute detail App Router)
