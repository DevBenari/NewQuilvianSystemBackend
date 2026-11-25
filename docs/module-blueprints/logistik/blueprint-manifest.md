# Logistik & Manajemen Persediaan — Blueprint Manifest

| Field | Nilai |
| --- | --- |
| `blueprint_id` | `LGS-BP-001` |
| `revision` | `5` — keputusan `LGS-DEC-003` (atasan unit), `LGS-DEC-004` (otorisasi CITO), `LGS-DEC-005` (verifikasi susulan CITO), `LGS-DEC-006` (batas aktivasi) `approved`; verifikasi susulan CITO diterapkan |
| `status` | `DISCOVERY` — struktur modul dan persetujuan permintaan unit sudah disetujui; keputusan operasional, kepemilikan domain, alur Goods Receipt, dan valuasi masih terbuka |
| Modul | `logistik` — Logistik & Manajemen Persediaan (Health Services) |
| `module_prefix` (usulan) | `LGS` untuk ID dokumen; prefix entity `Lgs` = *Logistik* **belum terdaftar** di `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (`LGS-OQ-016`) |
| `current_phase` | `LGS-PH-000` — penggalian keputusan (wawancara pemilik proses) |
| Tanggal dibuat | 2026-11-17 |
| `updated_at` | 2026-11-17 |
| `last_verified_at` | belum diverifikasi |
| `supersedes` | tidak ada |

## Owners

| Wewenang | Pemilik |
| --- | --- |
| Pemrakarsa | Ikbal Yuliyanto, pemilik modul Farmasi, Operasi, dan Gizi |
| Product/domain | **Belum ditetapkan** (`LGS-OQ-015`) |
| Barang medis (obat, alkes, BMHP) | Pemilik modul Farmasi, mengikuti keputusan `PHA-DEC-*` yang sudah `approved`; posisi akhirnya menunggu `LGS-OQ-001` |
| Pembelian dan tanda terima barang | Pemilik modul Finance (`FIN-DEC-045`) |
| Registry prefix | Pemilik registry (`LGS-OQ-016`) |

## Approval

| Field | Nilai |
| --- | --- |
| `approved_by` | Ikbal Yuliyanto, untuk `LGS-DEC-001` sampai `LGS-DEC-006` saja |
| `approved_at` | 2026-11-17 |
| Catatan | Yang disetujui: struktur modul (`LGS-DEC-001`) dan persetujuan permintaan barang unit beserta turunannya (`LGS-DEC-002`..`006`). Pekerjaan kategori A pada `00-interview-decisions.md` bagian 9 dikerjakan sebagai task ad-hoc (`task/report/**`) atas instruksi tertulis pemrakarsa; migration dan eksekusi database tetap tidak diizinkan |

## Snapshot source

| Field | Nilai |
| --- | --- |
| `backend_source_sha` | `d946b20b` (branch `Ikbal`, setara `origin/QuilvianIntegrationBackend` per 2026-11-17) |
| `frontend_source_sha` | `1e9adc790` (branch `Ikbalv2`, memuat integration per 2026-11-17) ditambah perubahan menu Logistik yang belum di-commit (lihat `00-interview-decisions.md` bagian 6) |

## Contract versions

Belum ada kontrak. Kontrak baru disusun setelah keputusan pada `00-interview-decisions.md` disetujui dan arsitektur domain siap.

## Dependency aktif

| ID | Dependency | Pemilik | Status |
| --- | --- | --- | --- |
| `LGS-DEP-001` | Keputusan stok obat pada blueprint `pharmacy` (`PHA-DEC-003`, `010`, `012`, `019`, `020`, `022`, `030`, `031`; `PHA-OQ-003` masih `open`) | Pemilik modul Farmasi | `approved` di modul asal, kecuali `PHA-OQ-003` |
| `LGS-DEP-002` | Siklus Purchasing/AP: Purchase Order, Goods Receipt, retur pembelian (`FIN-DEC-045`), dengan BLM sebagai sumber utama PO dan GR (`FIN-DEC-261`, `263`, `268`) | Pemilik modul Finance | `approved` di modul asal |
| `LGS-DEP-003` | Pendaftaran prefix entity di registry | Pemilik registry | belum diajukan |

## Readiness hulu

| Tahap | Artefak | Hasil |
| --- | --- | --- |
| Wawancara | `00-interview-decisions.md` r5 | Enam keputusan `approved` (`LGS-DEC-001`..`006`); `LGS-OQ-004` dan `020`..`023` ditutup; 18 pertanyaan masih terbuka (`LGS-OQ-001`..`003`, `005`..`019`) |
| Capability map | `01-existing-capability-map.md` r2 | 35 kemampuan: 12 Ready to reuse, 8 Reuse with adapter, 4 Extend, 8 Missing, 1 Conflict (`LGS-CAP-07`), 2 Unknown. Impact scan sesudah sinkron integration 2026-11-17; SHA sama dengan snapshot |
| Requirement gate | — | Belum dijalankan |
| Arsitektur domain | — | Belum dijalankan |

## Riwayat revisi

| Revisi | Tanggal | Perubahan |
| --- | --- | --- |
| 5 | 2026-11-17 | `LGS-DEC-003`..`006` dicatat; verifikasi susulan CITO (endpoint, daftar, panel); izin Approve eksplisit di service; pengendalian batas aktivasi; dokumen `evidence/05-aktivasi-batas-persetujuan.md` dan `06-dataset-uji-persetujuan.md`; test backend dipecah per jenis; laporan `LGS-BE-ADH-04` dan `LGS-FE-ADH-04` |
| 4 | 2026-11-17 | Penyempurnaan `LGS-DEC-002` atas instruksi pemrakarsa: kewenangan atasan unit dari struktur organisasi yang ada, otorisasi CITO yang tidak dapat dilewati peran SuperAdmin, aturan peralihan permintaan lama (`LGS-OQ-023`), usulan verifikasi susulan CITO (`evidence/04-usulan-verifikasi-susulan-cito.md`), test backend; laporan `LGS-BE-ADH-03` dan `LGS-FE-ADH-03` |
| 3 | 2026-11-17 | `LGS-DEC-002` (persetujuan permintaan barang unit) `approved`, menutup `LGS-OQ-004`; pertanyaan lanjutan `LGS-OQ-020` (atasan per unit), `021` (peran otorisasi CITO), `022` (validasi atasan susulan untuk CITO); laporan task ad-hoc `LGS-BE-ADH-02` dan `LGS-FE-ADH-02` |
| 2 | 2026-11-17 | `LGS-DEC-001` (struktur modul) `approved`; pemisahan pekerjaan READY/REQUIRES DECISION; laporan task ad-hoc `LGS-BE-ADH-01` dan `LGS-FE-ADH-01` |
| 1 | 2026-11-17 | Identitas blueprint dibuat dalam `MODULE BLUEPRINT MODE` atas izin pemrakarsa. Isi diambil dari dokumen usulan alur yang diserahkan ke analis pada 13 November 2026. Pada hari yang sama snapshot disegarkan ke `d946b20b` / `1e9adc790` sesudah sinkron integration (status saja, revisi tidak naik) |
