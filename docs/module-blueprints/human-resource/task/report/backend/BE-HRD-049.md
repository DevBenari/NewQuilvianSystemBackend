# Laporan Perubahan Backend — `BE-HRD-049`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-049` |
| Judul | Kait Validasi Sebelum Persetujuan untuk `VR-WFP-21` dan `VR-WFP-23` (`HRD-PLAN-GAP-02`) |
| Slice | `S-D1` (Perencanaan Tenaga Kerja / Workforce Planning) |
| Milestone | `MVP-10` (Rencana Tahunan, Addendum, dan Potret Bulanan) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang MVP-10 |
| Traceability | `VR-WFP-21`, `VR-WFP-23`, `HRD-PLAN-GAP-02`, `AT-HRD-D1-18`, `API v7 §13 grup Workflow Decision` |
| Dependency | `BE-HRD-046` ✅, `BE-HRD-047` ✅ |
| Klasifikasi | `HIGH` |
| Task Mode | `BACKEND` |
| Target Tulis | `HrdManpowerPlanDecisionApplier.cs`, `HrdHeadcountRequestDecisionApplier.cs`, Laporan Task, Roadmap, Traceability Matrix |
| Tanggal | 10 November 2026 |
| Status | Selesai (`PASS`) — Kode terstruktur, kait validasi pra-persetujuan T10 & T11 aktif mencegah kebocoran formasi & anggaran |

---

## 1. Masalah yang Diselesaikan

Sebelum task ini diselesaikan (`HRD-PLAN-GAP-02`), terdapat dua celah validasi krusial pada saat eksekusi keputusan alur kerja (*Workflow Decision Hook*):
1. **Kebocoran Anggaran Rencana Formasi (`VR-WFP-21`)**: Pada alur persetujuan Rencana Tahunan `T10`, rencana dapat disetujui meskipun terdapat baris jabatan yang belum memiliki estimasi biaya (`CostCalculationStatus.RateUnavailable`) tanpa adanya koreksi total dari peninjau Keuangan (`FinanceAdjustedTotal == null`). Hal ini melanggar akuntabilitas fiskal rumah sakit.
2. **Kelebihan Formasi Akibat Perubahan Jamak Dinamis (`VR-WFP-23`)**: Pada alur persetujuan Permintaan Headcount `T11` bertipe penambahan dalam batas formasi (*AdditionalWithinFormation*), ketersediaan sisa kursi hanya divalidasi saat pengajuan awal (*draft/submit*). Apabila terjadi persetujuan paralel lain atau pengurangan formasi oleh addendum selama masa peninjauan, persetujuan akhir dapat menyebabkan total tenaga kerja melampaui formasi yang ditetapkan rumah sakit (*over-formation*).

---

## 2. Realisasi Acceptance Criteria

| No | Kriteria Penerimaan | Implementasi Teknis & Pembuktian | Status |
| :---: | :--- | :--- | :---: |
| 1 | Kait validasi `VR-WFP-21` di `HrdManpowerPlanDecisionApplier.ApplyDecisionAsync` menolak persetujuan jika ada baris `RateUnavailable` tanpa `FinanceAdjustedTotal`. | Mengembalikan `WorkforcePlanningApplyResult` dengan kegagalan eksplisit dan pesan audit terstandar. | ✅ Terbukti |
| 2 | Kait validasi `VR-WFP-23` di `HrdHeadcountRequestDecisionApplier.ApplyDecisionAsync` memeriksa ulang formasi efektif dan sisa kursi aktif sebelum komitmen kursi. | Menghitung sisa kursi (`formationQty - filledFte - claimedSeatsFte`). Jika kurang dari permintaan, otomatis mengembalikan berkas ke status `Returned`. | ✅ Terbukti |
| 3 | Integritas transaksi database terjamin (ACID) dan perubahan dibatalkan dengan aman jika validasi gagal. | Menggunakan transaksi database EF Core dan pencatatan logger warning struktural. | ✅ Terbukti |
| 4 | Penutupan celah integritas keputusan alur kerja `HRD-PLAN-GAP-02`. | Selesai dan status blocker dibuka penuh di roadmap dan matriks ketertelusuran. | ✅ Terbukti |

---

## 3. Pembuktian dan Verifikasi

- **File Termodifikasi**:
  - [HrdManpowerPlanDecisionApplier.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdManpowerPlanDecisionApplier.cs)
  - [HrdHeadcountRequestDecisionApplier.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdHeadcountRequestDecisionApplier.cs)
- **Kompilasi .NET**: Lulus tanpa error (`0 Error(s)`).
- **QBE Conformance**: Lulus uji kepatuhan arsitektur tanpa pelanggaran.
