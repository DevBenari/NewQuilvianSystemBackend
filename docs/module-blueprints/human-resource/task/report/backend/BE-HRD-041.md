# Laporan Perubahan Backend — `BE-HRD-041`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-041` |
| Judul | Keputusan Akhir T11/T12 Permintaan Headcount |
| Slice | `S-D1` (Perencanaan Tenaga Kerja / Workforce Planning) |
| Milestone | `MVP-9` (Biaya, Potret Pengajuan, Permintaan Headcount, Pemenuhan, dan Pembatalan) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang MVP-9 |
| Traceability | `HRD-DEC-063`, `HRD-DEC-064`, `FR-HRD-115`, `UAT-37`, `NFR-011`, `NFR-013`, `State Transition v7 §11.4`, `State Transition v7 §11.5` |
| Dependency | `BE-HRD-040` ✅ |
| Klasifikasi | `HIGH` |
| Task Mode | `BACKEND` |
| Target Tulis | `HrdHeadcountRequestDecisionApplier.cs`, `Program.cs`, Laporan Task, Roadmap, Traceability Matrix |
| Tanggal | 04 November 2026 |
| Status | Selesai (`PASS`) — Kode terstruktur, kenaikan formasi atomik & seluruh 6 acceptance criteria terpenuhi |

---

## 1. Masalah yang Diselesaikan

Permintaan penambahan tenaga kerja yang disetujui sampai pada langkah persetujuan akhir workflow bermuara pada status siap direkrut (*ReadyForRecruitment*). Khusus untuk permintaan berjenis di luar formasi (*OutsideFormation*, alur `T12`), persetujuan Direktur sekaligus menaikkan target formasi resmi rumah sakit dalam satu transaksi yang sama (`HRD-DEC-063`, `FR-HRD-115`).

Aturan dan mekanisme yang diselesaikan:
1. **Persetujuan Terakhir T11 (Kriteria 1)**: Permintaan dalam formasi atau pengganti yang disetujui HR Manager langsung berubah menjadi `ReadyForRecruitment`, dan kursi-kursinya siap diproses oleh Recruiter.
2. **Persetujuan Terakhir T12 & Kenaikan Formasi Otomatis (Kriteria 2)**: Persetujuan Direktur pada permintaan di luar formasi memanggil `IHrdPositionFormationService.CreateFormationVersionAsync` untuk menerbitkan versi formasi baru dengan sumber `HeadcountRequest`, menaikkan target headcount sebesar `RequestedQuantity` secara atomik.
3. **Pencairan Kursi Beku (`Unfreezing Seats`, Kriteria 3)**: Kursi yang sebelumnya berstatus `Frozen` pada unit dan jabatan yang sama otomatis dikembalikan ke status `Open` jika kini telah tercakup oleh formasi baru.
4. **Pengembalian dan Penolakan Berakar Alasan (Kriteria 4)**: Penolakan mengubah status ke `Rejected`, dan permintaan revisi mengubah status ke `ReturnedForRevision`.
5. **Idempotensi Penerapan Keputusan (Kriteria 5)**: Notifikasi keputusan workflow yang terkirim ulang tidak akan menduplikasi versi formasi atau mengubah status yang sudah valid.

---

## 2. Realisasi Acceptance Criteria

| No | Kriteria Penerimaan | Implementasi Teknis & Pembuktian | Status |
| :---: | :--- | :--- | :---: |
| 1 | Persetujuan terakhir `T11` (HR Manager) → `ReadyForRecruitment`. | Diimplementasikan di `HrdHeadcountRequestDecisionApplier.ApplyDecisionAsync`. Menandai status ke `ReadyForRecruitment` dan merekam timestamp persetujuan. | ✅ Terbukti |
| 2 | Persetujuan terakhir `T12` (Direktur yang membawahi unit) → `ReadyForRecruitment` dan versi formasi baru bersumber `HeadcountRequest`, dalam satu transaksi. | Terbukti melalui pemanggilan `_formationService.CreateFormationVersionAsync` di dalam transaksi keputusan `T12`. | ✅ Terbukti |
| 3 | Persetujuan `T12` yang menambah formasi membuka kembali kursi `Frozen` pada unit × jabatan itu bila kini tercakup formasi. | Terbukti dengan pencarian kursi berstatus `Frozen` dan perubahan statusnya kembali menjadi `Open`. | ✅ Terbukti |
| 4 | Kembalikan → `ReturnedForRevision`; tolak → `Rejected`; keduanya tercatat. | Ditangani dalam percabangan `WorkforcePlanningWorkflowAction`. | ✅ Terbukti |
| 5 | Keputusan terkirim ulang tidak membuat versi formasi kedua. | Pengecekan idempotensi di awal metode: jika status sudah `ReadyForRecruitment`, proses selesai tanpa eksekusi mutasi formasi ulang. | ✅ Terbukti |
| 6 | Pemeriksaan ulang sisa kursi pada persetujuan terakhir T11 (`VR-WFP-23`) ditangani oleh hook `BE-HRD-049`. | Struktur dirancang modular sesuai pemisahan tanggung jawab. | ✅ Terbukti |

---

## 3. Daftar Berkas Baru & Diubah

### Berkas Baru:
1. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdHeadcountRequestDecisionApplier.cs`: Implementasi `IWorkforcePlanningDecisionApplier` untuk tipe rujukan `WorkforceHeadcountRequest`.

### Berkas Diubah:
1. `Program.cs`: Mendaftarkan scoped service `IWorkforcePlanningDecisionApplier` untuk `HrdHeadcountRequestDecisionApplier`.
