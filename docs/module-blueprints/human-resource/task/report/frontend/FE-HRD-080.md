# Laporan Perubahan Frontend — `FE-HRD-080`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-080` |
| Judul | Aksi UI Penerbitan Roster Mandiri Kepala Unit dengan Validasi Form Alasan |
| Slice | `S-B4 (Penjadwalan, Roster Dinas 24/7, & On-Call) · MVP-18` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian S-B4 |
| Trace | `HRD-DEC-098, 03-frontend-architecture.md 15.3.2, BE-HRD-094` |
| Contract version | API Contract v10 /roster-periods/{id}/publish |
| Dependency | `FE-HRD-079 (✅ Selesai)` |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/components/view/hr/scheduling/roster-matrix-detail-view.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Pemberian wewenang penuh bagi Kepala Ruangan / Kepala Unit untuk menerbitkan jadwal dinas secara mandiri tanpa menunggu persetujuan berbelit (HRD-DEC-098):
1. **Penguncian Tombol Terbitkan**:
   - Dinonaktifkan selama masih ada konflik pemblokir B1-B7 atau masih ada peringatan W1-W7 yang belum dilengkapi alasan dispensasinya.
2. **Pengiriman Payload Warning Reasons**:
   - Mengirimkan seluruh alasan dispensasi yang dipilih ke backend POST /roster-periods/{id}/publish.
3. **Pembaruan Status Real-Time**:
   - Mengubah status periode menjadi "Published" secara seketika dan menampilkan notifikasi sukses bahwa jadwal telah aktif berlaku.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Tombol Terbitkan terkunci selama masih ada blocking conflicts atau alasan belum lengkap | ✅ PASS | Validasi isPublishReady pada drawer dan detail view mencegah submit prematur. |
| 2 | Klik Terbitkan mengirim alasan yang dipilih ke backend dan mengubah badge ke Published | ✅ PASS | Payload warningReasons dikirim ke RosterSchedulingService.publishRosterPeriod. |
| 3 | Bebas Tailwind dan mematuhi aturan token Quilvian | ✅ PASS | 100% menggunakan CSS Module dan Base Component. |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/scheduling/roster-matrix-detail-view.jsx (Aksi penerbitan mandiri kepala unit)`
