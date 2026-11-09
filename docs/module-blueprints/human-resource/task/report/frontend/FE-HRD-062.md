# Laporan Perubahan Frontend — `FE-HRD-062`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-062` |
| Judul | Siap Kerja (Ready to Work), Pembatalan Bergabung (Cancel Join), & Tautan Serah Terima Rekrutmen |
| Slice | `S-C4-ONB (Onboarding dan Kontrak Kerja Pegawai RS) · MVP-15` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 3 S-C4-ONB |
| Trace | `HRD-DEC-087, HRD-DEC-088, VR-ONB-11 s.d. VR-ONB-15, BE-HRD-076, FE-HRD-056` |
| Contract version | API Contract v9 bagian 15.1 (ready-to-work, cancel-join) |
| Dependency | `FE-HRD-061 (✅ Selesai), BE-HRD-076 (✅ Selesai), FE-HRD-056 (✅ Selesai)` |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/components/view/hr/lifecycle/onboarding-case-detail-view.jsx, src/components/view/hr/recruitment/hiring-handoffs-view.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Menjamin bahwa pegawai baru yang telah menyelesaikan seluruh checklist orientasi dapat dinyatakan siap bertugas secara sah, serta menangani skenario pembatalan bergabung sebelum hari pertama kerja tanpa kebocoran data.

Task ini mengimplementasikan:
1. **Aksi Nyatakan Siap Bekerja (Ready to Work)**:
   - Memastikan tombol hanya aktif jika seluruh butir checklist orientasi (3 lapis: RS, Unit, Fasilitas/IT) yang bertanda wajib telah diselesaikan (100% complete).
   - Memanggil endpoint backend POST `/api/v1/corporate/human-resource/onboarding-cases/{id}/ready-to-work`.
   - Mengubah status kasus menjadi `COMPLETED` dan menerbitkan notifikasi kesiapan bertugas.
2. **Aksi Batalkan Bergabung (Cancel Join)**:
   - Sesuai aturan bisnis RS (VR-ONB-13 s.d. VR-ONB-15), pembatalan hanya dapat dilakukan sebelum presensi hari pertama dikonfirmasi (`PREPARATION` atau `FIRST_DAY_ATTENDED`).
   - Membuka modal pembatalan bergabung dengan field alasan wajib dan dokumen pendukung opsional.
   - Memanggil endpoint POST `/api/v1/corporate/human-resource/onboarding-cases/{id}/cancel-join`.
3. **Tautan Navigasi Serah Terima Rekrutmen**:
   - Menghubungkan layar Serah Terima Rekrutmen (`/hr/recruitment/handoffs`) ke kasus onboarding terkait (`/hr/lifecycle/onboarding-cases/{id}`) sehingga staf rekrutmen dapat memantau progres orientasi pegawai baru secara transparan.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Tombol Siap Bekerja menolak aksi jika masih ada butir checklist wajib yang belum lengkap (VR-ONB-11) | ✅ PASS | Pemeriksaan status kelengkapan checklist pada OnboardingCaseDetailView mencegah submit premature. |
| 2 | Modal Batalkan Bergabung mewajibkan pengisian alasan pembatalan (VR-ONB-14) | ✅ PASS | Validasi form modal memvalidasi field reason sebelum submit. |
| 3 | Layar Serah Terima Rekrutmen menampilkan tautan langsung ke Kasus Onboarding | ✅ PASS | HiringHandoffsView diperbarui dengan tombol "Buka Kasus Onboarding →". |
| 4 | Bebas Tailwind dan mematuhi standar token Quilvian | ✅ PASS | 100% menggunakan CSS Module dan Base Component Quilvian. |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/lifecycle/onboarding-case-detail-view.jsx (Aksi siap kerja & modal pembatalan bergabung)`
- `src/components/view/hr/recruitment/hiring-handoffs-view.jsx (Tautan navigasi lintas modul rekrutmen ke onboarding)`
