# Roadmap backend perbaikan kelas perawatan

Revision 1, 2026-10-12. Blueprint `dokter-rawat-inap`; kontrak `RWI-KELAS-006-1.0`; approval pelaksanaan: instruksi pemilik 2026-10-12. Snapshot BE `d908988248f6652ed7f2ce84aae9eb3e38dd36a0`, FE `affcfcb4ea9f5177b1ff9a3ca0c09d15183af248`; perubahan Billing yang sudah ada dipertahankan. ID setelah maksimum BE-RWI-222/FE-RWI-238. Legenda: ✅ selesai, 🟡 sebagian, ⛔ terblokir, tanpa tanda belum dikerjakan.

## Grafik Urutan Dependency

```text
BE-RWI-223
BE-RWI-226
```

| Gelombang | Boleh mulai setelah | Task |
| --- | --- | --- |
| 1 | Kontrak aturan kelas disetujui lewat plan | BE-RWI-223 |
| 1 | Jawaban pemilik K-02 | BE-RWI-226 |

## Task

### BE-RWI-223 — Kecocokan kelas pada aturan cakupan klinis

Status: belum dikerjakan. Dependency: tidak ada.

Outcome: aturan kelas lain tidak menanggung item pasien; item tetap tersedia. Requirement A-02/A-03, FIX-DOK-006-04, K-03. Kontrak [RWI-KELAS-006-1.0](../contracts/kelas-perawatan-repair-006.md). Reuse: `MstInsuranceCoverageRule.PatientClassId`, `InsuranceCoverageService`. Cakupan: filter kandidat dan urutan kelas pada resolver klinis; integrasi kelas efektif/Billing didaftarkan pada task berikutnya setelah konteksnya ditelusuri.

Acceptance: kelas null atau sama yang cocok; kelas lain tidak cocok; khusus kelas menang pada sasaran setara; katalog tidak ditambah filter kelas. Verifikasi: review kontrak/source, restore/build aplikasi, review diff. Risiko: aturan umum tetap bisa cocok bila dikonfigurasi demikian. Pemilik Clinical. DoD: implementasi, build, laporan tracked, traceability. QBE preflight saat eksekusi memakai AGENTS.md dan governance canonical, bukan pola legacy sebagai wewenang.

### BE-RWI-226 — Tarif umum dan penahanan resep tanpa tarif

Status: belum dikerjakan. Dependency: tidak ada. Requirement FIX-DOK-006-03, K-02. Kontrak RWI-KELAS-006-1.0 SHA256 `439623315c8cb4922e4794b7efb2218228d9e88fbd5f25e1473decd5087cd112`. Outcome: obat lintas kelas tetap tampil; tarif umum boleh dipakai; tarif kelas lain tidak dipilih sembarang; simpan reguler/racikan ditahan bila tidak ada tarif sesuai/umum. Reuse InsuranceCoverageService, readiness katalog, seluruh service penyimpanan resep existing. Acceptance: NotCovered tetap IsValid dan dapat disimpan; ConfigurationMissing tetap invalid; tarif nol sah dibedakan dari ketiadaan tarif. Verifikasi: restore/build, review jalur penyimpanan dan cakupan, kontrak API. DoD: source, bukti, laporan dan traceability. Pemilik Clinical/Pharmacy. Risiko: fallback tunai legacy tidak boleh memilih tarif beda kelas.
