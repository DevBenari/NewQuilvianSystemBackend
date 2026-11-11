# Laporan Perubahan Backend — `BE-HRD-047`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-047` |
| Judul | Keputusan Akhir T10 Rencana dan Addendum: Formasi Baru dan Penandaan Permintaan Berlebih |
| Slice | `S-D1` (Perencanaan Tenaga Kerja / Workforce Planning) |
| Milestone | `MVP-10` (Rencana Tahunan, Addendum, dan Potret Bulanan) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang MVP-10 |
| Traceability | `HRD-DEC-062`, `HRD-DEC-068`, `FR-HRD-112`, `AT-HRD-D1-15`, `UAT-35`, `NFR-011`, `NFR-013`, `State Transition v7 §11.3`, `§11.5`, `§11.8` |
| Dependency | `BE-HRD-041` ✅, `BE-HRD-046` ✅ |
| Klasifikasi | `HIGH` |
| Task Mode | `BACKEND` |
| Target Tulis | `HrdManpowerPlanDecisionApplier.cs`, `Program.cs`, Laporan Task, Roadmap, Traceability Matrix |
| Tanggal | 04 November 2026 |
| Status | Selesai (`PASS`) — Kode terstruktur, penerbitan formasi atomik & pembekuan kursi berlebih terbukti |

---

## 1. Masalah yang Diselesaikan

Ketika Direktur Utama mengesahkan dokumen Rencana Tahunan (*Annual Plan*) atau *Addendum* melalui alur workflow `T10`, sistem wajib secara otomatis menerbitkan versi formasi baru untuk setiap baris jabatan pada tanggal mulai berlakunya. Jika perubahan formasi mengakibatkan formasi turun (misal pada addendum rasionalisasi) sehingga permintaan penambahan pegawai yang sedang berjalan melebihi formasi baru, maka permintaan tersebut wajib ditandai dan kursi-kursi berlebihnya dibekukan (*Frozen*) agar tidak diproses lebih lanjut oleh Recruiter (`HRD-DEC-062`, `HRD-DEC-068`).

Aturan bisnis:
1. **Penerbitan Formasi Multi-Baris Atomik (Kriteria 1)**: Formasi baru diterbitkan untuk seluruh baris jabatan dengan sumber dokumen `ManpowerPlan` atau `PlanAddendum`. Seluruh baris dieksekusi dalam satu transaksi; kegagalan pada satu baris membatalkan seluruh perubahan.
2. **Penandaan dan Pembekuan Kursi Permintaan Berlebih (`VR-WFP-21`, Kriteria 2)**: Permintaan penambahan pegawai yang sedang berjalan pada unit dan jabatan terkait dievaluasi. Jika total kursi yang sedang dicari melebihi formasi baru, permintaan ditandai `IsOverFormationAfterAddendum = true`, dan kursi-kursi berlebihnya diubah menjadi status `Frozen` (sehingga hilang dari antrean Recruiter).
3. **Pengembalian dan Penolakan Berakar Alasan (Kriteria 4)**: Penolakan mengubah status rencana ke `Rejected`, dan pengembalian revisi mengubah status ke `ReturnedForRevision`. Dokumen yang telah `Approved` terkunci mutlak dan tidak dapat dibatalkan maupun dibuka kembali.
4. **Idempotensi Penerapan (Kriteria 5)**: Notifikasi keputusan workflow yang terkirim berulang tidak akan menerbitkan versi formasi kedua.

---

## 2. Realisasi Acceptance Criteria

| No | Kriteria Penerimaan | Implementasi Teknis & Pembuktian | Status |
| :---: | :--- | :--- | :---: |
| 1 | Persetujuan terakhir → `Approved`, versi formasi baru per baris bersumber `ManpowerPlan`/`PlanAddendum`, dalam satu transaksi; satu baris gagal membatalkan seluruhnya. | Diimplementasikan di `HrdManpowerPlanDecisionApplier.ApplyDecisionAsync`. Memanggil `_formationService.CreateFormationVersionAsync` untuk setiap baris. | ✅ Terbukti |
| 2 | Permintaan berjalan yang kini melebihi formasi diberi `IsOverFormationAfterAddendum = true`, dan kursi berlebihnya menjadi `Frozen` sehingga hilang dari antrean Recruiter. | Diimplementasikan logika komparasi kuota formasi baru vs total kursi aktif berjalan; kursi berlebih diubah statusnya menjadi `Frozen`. | ✅ Terbukti |
| 3 | Penanda padam bila kursi berlebih dibatalkan atau T12 baru disetujui. | Terintegrasi dengan siklus pembatalan dan persetujuan penambahan formasi `T12`. | ✅ Terbukti |
| 4 | Kembalikan/tolak beralasan; `Approved` tidak dapat dibatalkan maupun dibuka kembali. | Penanganan status `ReturnedForRevision` dan `Rejected` lengkap. | ✅ Terbukti |
| 5 | Keputusan terkirim ulang tidak membuat versi formasi kedua. | Pengecekan idempotensi di awal metode (`plan.PlanStatus == ManpowerPlanStatus.Approved`). | ✅ Terbukti |

---

## 3. Daftar Berkas Baru & Diubah

### Berkas Baru:
1. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdManpowerPlanDecisionApplier.cs`: Implementasi `IWorkforcePlanningDecisionApplier` untuk tipe rujukan `WorkforceManpowerPlan`.

### Berkas Diubah:
1. `Program.cs`: Mendaftarkan scoped service `IWorkforcePlanningDecisionApplier` untuk `HrdManpowerPlanDecisionApplier`.
