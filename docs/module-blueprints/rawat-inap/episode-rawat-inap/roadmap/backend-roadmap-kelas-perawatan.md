# Roadmap backend perbaikan kelas perawatan

Revision 1, 2026-10-12; owner episode-rawat-inap. Approval instruksi pemilik untuk seluruh PLAN-REPAIR-DOK-006 dan jawaban K-01/K-02 2026-10-12. Kontrak [RWI-KELAS-006-1.0](../../dokter-rawat-inap/contracts/kelas-perawatan-repair-006.md), SHA256 `439623315c8cb4922e4794b7efb2218228d9e88fbd5f25e1473decd5087cd112`. Source BE `d908988248f6652ed7f2ce84aae9eb3e38dd36a0`, FE `affcfcb4ea9f5177b1ff9a3ca0c09d15183af248`. Perubahan Billing yang sudah ada dipertahankan. Legenda: ✅ selesai, 🟡 sebagian, ⛔ terblokir; cermin [BE] berasal roadmap episode.

## Grafik Urutan Dependency

```text
BE-RWI-224
```

| Gelombang | Boleh mulai setelah | Task |
| --- | --- | --- |
| 1 | Tidak ada | BE-RWI-224 |

## Task

### BE-RWI-224 — Kelas layanan dari riwayat penempatan

Status: belum dikerjakan. Dependency: Tidak ada. Requirement: A-01–A-05, FIX-DOK-006-02; keputusan K-01/K-02/K-03 sesuai kontrak. Outcome dan scope: Resolver periode kamar dan konteks Clinical. Sebelum penempatan null; transfer berakhir tetap sah; koreksi diganti dikecualikan. Rawat jalan/IGD tetap memakai kelas kunjungan.

Reuse: model, service, hook, API dan base component existing; tidak membuat tabel baru. Acceptance: seluruh perilaku pada scope di atas dan contoh kamar kelas 1/item hanya dijamin kelas 2 pada plan, tanpa menyembunyikan item. Verifikasi: review kontrak/diff/proses bisnis; restore/build aplikasi backend dan QBE preflight. Risiko: consumer bersama harus diperiksa. DoD: source, validasi, laporan tracked, traceability. Wewenang hanya source dan dokumentasi, tanpa eksekusi database/deploy. QBE preflight/conformance dilaksanakan saat eksekusi dari AGENTS.md dan governance canonical.
