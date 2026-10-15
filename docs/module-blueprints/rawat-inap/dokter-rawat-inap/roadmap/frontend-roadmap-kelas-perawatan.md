# Roadmap frontend perbaikan kelas perawatan

Revision 1, 2026-10-12. Blueprint `dokter-rawat-inap`; kontrak `RWI-KELAS-006-1.0`; approval: instruksi pemilik 2026-10-12. Snapshot BE `d908988248f6652ed7f2ce84aae9eb3e38dd36a0`, FE `affcfcb4ea9f5177b1ff9a3ca0c09d15183af248`. Legenda: ✅ selesai, 🟡 sebagian, ⛔ terblokir, tanpa tanda belum dikerjakan.

## Grafik Urutan Dependency

```text
FE-RWI-239 ✅
```

| Gelombang | Boleh mulai setelah | Task |
| --- | --- | --- |
| 1 | Kontrak status existing terkunci | FE-RWI-239 |

## Task

### ✅ FE-RWI-239 — Label cakupan resep menurut status

Status: ✅ **SELESAI 13 Oktober 2026.** Keempat acceptance criteria terpetakan ke `getDrugCoverageInfo` dan pemakai shared rawat inap/IGD/rawat jalan. `inpatient-class-coverage.test.mjs` 5/5 `PASS`; ESLint terarah 17 berkas exit 0; build produksi Next.js `PASS` pada sesi implementasi; 14 berkas unit terkait 87/92 dengan 5 kegagalan lama di luar task. Uji manual `NOT FEASIBLE`, runtime tidak tersedia. Bukti: [laporan](../task/report/frontend/FE-RWI-239.md). Dependency: tidak ada. Outcome: resep tidak menampilkan `Cover 0%` sebagai ditanggung. Requirement FIX-DOK-006-05. Kontrak [RWI-KELAS-006-1.0](../contracts/kelas-perawatan-repair-006.md). Reuse: helper `getDrugCoverageInfo` dan komponen resep bersama. Scope: pemetaan status dan pemeriksaan pemakai shared, tanpa kalkulasi asuransi di frontend. UI: DEV_DISCRETION untuk teks penjelas; komponen existing dipakai kembali.

Acceptance: Covered/Partial/NotCovered/NeedApproval/Unknown dibedakan; tanpa asuransi tidak diberi lencana pertanggungan; data lama tanpa status tidak otomatis ditanggung karena nilai nol; semua pemakai helper menerima hasil konsisten. Verifikasi: unit mapping, lint, suite unit, build, manual bila runtime tersedia. Risiko/pemilik: shared Clinical/Pharmacy, regresi rawat jalan/IGD wajib diperiksa. DoD: source, validasi, laporan tracked, traceability.
