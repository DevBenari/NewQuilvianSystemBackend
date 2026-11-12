# Laporan Perubahan Frontend — `FE-HRD-067`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-067` |
| Judul | Layar Daftar & Formulir Batch Inisialisasi Kontrak Kerja Awal Pegawai (FE-HRD-134, FE-HRD-135) |
| Slice | `S-C4-ONB (Onboarding dan Kontrak Kerja Pegawai RS) · MVP-16` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 3 S-C4-ONB |
| Trace | `HRD-DEC-096, VR-ONB-64 s.d. VR-ONB-69, BE-HRD-082` |
| Contract version | API Contract v9 bagian 15.5 (initial-contract-batches) |
| Dependency | `FE-HRD-058 (✅ Selesai), BE-HRD-082 (✅ Selesai)` |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/components/view/hr/lifecycle/initial-contract-batches-list-view.jsx, src/components/view/hr/lifecycle/initial-contract-batch-detail-view.jsx, src/app/hr/lifecycle/initial-contract-batches/page.jsx, src/app/hr/lifecycle/initial-contract-batches/[id]/page.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Fasilitas migrasi dan registrasi borongan (*batch import/initialization*) kontrak kerja pegawai aktif yang sudah ada di rumah sakit ke dalam sistem baru.

Task ini mengimplementasikan:
1. **Layar Daftar Batch Inisialisasi (/hr/lifecycle/initial-contract-batches)**:
   - Riwayat batch impor data kontrak awal, total pegawai yang tercakup, tanggal inisialisasi, status eksekusi (DRAFT, COMMITTED, REJECTED), dan pembuat batch.
2. **Layar Buat & Detail Batch Inisialisasi (/hr/lifecycle/initial-contract-batches/[id])**:
   - Tabel input baris kontrak multi-pegawai dengan validasi NIK, nomor kontrak lama, jenis kontrak, masa berlaku, dan jabatan.
   - Pengecekan duplikasi: Mencegah pegawai yang sama dimasukkan lebih dari satu kali dalam satu batch (VR-ONB-66).
   - Aksi Komitmen Batch (Commit Batch): Mengonfirmasi pengesahan data massal ke basis data operasional kontrak rumah sakit.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Pencegahan duplikasi data pegawai dalam satu batch (VR-ONB-66) | ✅ PASS | Validasi form batch memeriksa keunikan NIP/NIK pada setiap baris data. |
| 2 | Aksi komitmen batch terhubung ke endpoint backend | ✅ PASS | Terhubung via LifecycleService.commitInitialContractBatch. |
| 3 | Penanganan status batch terkunci setelah dieksekusi | ✅ PASS | Batch dengan status COMMITTED atau REJECTED dikunci dari perubahan. |
| 4 | Bebas Tailwind dan mematuhi aturan token Quilvian | ✅ PASS | 100% menggunakan CSS Module dan Base Component. |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/lifecycle/initial-contract-batches-list-view.jsx (Tabel riwayat batch inisialisasi)`
- `src/components/view/hr/lifecycle/initial-contract-batch-detail-view.jsx (Editor baris data batch & aksi komitmen)`
- `src/app/hr/lifecycle/initial-contract-batches/page.jsx (Rute App Router daftar batch)`
- `src/app/hr/lifecycle/initial-contract-batches/[id]/page.jsx (Rute App Router detail batch)`
