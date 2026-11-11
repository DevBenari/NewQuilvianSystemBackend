# Laporan Perubahan Frontend — `FE-HRD-070`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-070` |
| Judul | Layar Daftar Kerja Terpadu (Worklist) "Perlu Tindakan" & Indikator Notifikasi Menu (FE-HRD-123) |
| Slice | `S-C4-ONB (Onboarding dan Kontrak Kerja Pegawai RS) · MVP-16` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 3 S-C4-ONB |
| Trace | `HRD-DEC-098, VR-ONB-81 s.d. VR-ONB-83, BE-HRD-084` |
| Contract version | API Contract v9 bagian 15.8 (worklist) |
| Dependency | `FE-HRD-058 (✅ Selesai), BE-HRD-084 (✅ Selesai)` |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/components/view/hr/lifecycle/lifecycle-worklist-view.jsx, src/app/hr/lifecycle/worklist/page.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Pusat kendali operasional staf HRD untuk memantau seluruh butir tugas onboarding dan kontrak kerja yang mendesak atau tertunda.

Task ini mengimplementasikan:
1. **Layar Perlu Tindakan (/hr/lifecycle/worklist)**:
   - Tabulasi tugas mendesak:
     - Kasus Onboarding mendekati hari pertama kerja.
     - Kontrak PKWT yang akan jatuh tempo dalam 30/60 hari.
     - Evaluasi masa percobaan (probation) yang telah mencapai bulan ke-3.
     - Evaluasi preceptorship klinis yang belum disahkan.
     - Permintaan akses IT pegawai baru yang belum dipenuhi.
   - Tombol pintasan langsung "Buka Kasus" atau "Buka Kontrak" menuju layar detail terkait.
2. **Sinkronisasi Badge Notifikasi**:
   - Menghitung total item pending untuk ditampilkan pada indikator badge menu sidebar Onboarding & Kontrak.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Tabulasi worklist mengelompokkan kategori tugas secara jelas | ✅ PASS | LifecycleWorklistView membagi daftar kerja berdasarkan modul dan tingkat urgensi. |
| 2 | Tautan aksi cepat mengarahkan pengguna ke layar detail yang tepat | ✅ PASS | Setiap butir kerja menyediakan tautan langsung ke halaman detail bersangkutan. |
| 3 | Data ditarik secara real-time dari endpoint worklist backend | ✅ PASS | Terhubung via LifecycleService.getWorklist. |
| 4 | Bebas Tailwind dan mematuhi aturan token Quilvian | ✅ PASS | 100% menggunakan CSS Module dan Base Component. |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/lifecycle/lifecycle-worklist-view.jsx (Layar daftar kerja terpadu & filter urgensi)`
- `src/app/hr/lifecycle/worklist/page.jsx (Rute App Router worklist)`
