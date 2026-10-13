# Traceability perbaikan kelas perawatan

2026-10-12; kontrak RWI-KELAS-006-1.0 SHA256 `439623315c8cb4922e4794b7efb2218228d9e88fbd5f25e1473decd5087cd112`. Approval instruksi pemilik untuk implementasi seluruh plan dan keputusan harga K-02. Diperbarui 2026-10-13 pada penutupan [PLAN-REPAIR-DOK-006](../../dokter-rawat-inap/docs/plan-repair/plan-repair-006-kelas-perawatan-dan-cakupan-asuransi.md).

| Requirement | Task | Bukti | Status |
| --- | --- | --- | --- |
| FIX-DOK-006-02 | BE-RWI-224 | [Laporan](../task/report/backend/BE-RWI-224.md); `InpServiceClassResolver`, `EncounterInsuranceService`, `InpEpisodeService`, `InpBedOccupancyService`; perbaikan label kelas penutupan; build penutupan 0 error; QBE 13 berkas PASS | ✅ Selesai 2026-10-13 |
| FIX-DOK-006-06 | FE-RWI-240 | [Laporan](../task/report/frontend/FE-RWI-240.md); pembayaran/konfirmasi tanpa kelas, `selectedBedId`, deposit dan transfer dari kamar; unit 5/5 PASS; ESLint terarah exit 0; build Next.js PASS; uji manual NOT FEASIBLE | ✅ Selesai 2026-10-13 |

Requirement lain pada plan yang sama: FIX-DOK-006-03/04/05 pada [dokter-rawat-inap](../../dokter-rawat-inap/roadmap/requirement-traceability-kelas-perawatan.md); bagian Billing FIX-DOK-006-02/04 pada [integrasi-billing](../../integrasi-billing/roadmap/requirement-traceability-kelas-perawatan.md).
