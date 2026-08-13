# IGD Blueprint Manifest

| Field | Value |
|---|---|
| `blueprint_id` | `IGD-BP-001` |
| `revision` | `4` |
| `status` | `draft` |
| `module` | `igd` |
| `design_snapshot_at` | `2026-08-14` |
| `backend_commit_sha` | `e5331a015fa416a89454b435de0014455f0326d8` |
| `frontend_commit_sha` | `08c84d371ed90640189ce1758019184b0a955e13` |
| `owners` | Product/Domain: pemilik suite skill sebagai pemegang sementara sesuai `IGD-DEC-046`, nama perlu diisi. Clinical governance dan Security/Privacy: `OPEN`, menjadi syarat go-live. Registration, Emergency Installation, Clinical, Pharmacy, Finance, Diagnostic Services, Integration, dan Frontend authority sesuai decision log |
| `approved_by` | `—` |
| `approved_at` | `—` |
| `input_revisions` | `00-interview-decisions.md` revision `4`; `01-existing-capability-map.md` revision `2` |
| `input_hashes` | Decisions `sha256:a81b10021302a73956d51c91d97c6551ae4fdf059e40c88e81355d2e420b9892`; capability map `sha256:6d64897f65bf4a0aecc7c81452162a6b6e89553356a7d02ede981c40875cdbc8` |
| `contract_versions` | API `0.2.0-draft`; state `0.2.0-draft`; validation `0.2.0-draft`; integration `0.2.0-draft`; permission/audit `0.2.0-draft` |
| `compatibility_impact` | Penambahan nilai `Completed` pada `EmergencyVisitStatus` berpotensi memutus pemakai yang memetakan status secara eksklusif. Tiga endpoint baru bersifat aditif. Pemisahan kewenangan SuperAdmin mengubah perilaku authorization di seluruh aplikasi, bukan hanya IGD. |

## Perubahan pada revision 4

| Area | Perubahan |
|---|---|
| Kepemilikan keputusan | Product/Domain Owner ditetapkan sementara; clinical governance dan security/privacy tetap terbuka sebagai syarat go-live |
| Skema triase | Baseline Permenkes 47/2018 dengan skala lima level ATS atau ESI; warna sebagai pengelompokan |
| Penutupan klinis | Status `Completed` ditambahkan setelah `Disposed` |
| Kewenangan SuperAdmin | Dipisahkan antara endpoint teknis dan endpoint klinis atau bisnis |
| Bentuk dokumen | Mengikuti kontrak keluaran baru: class diagram, arsitektur folder, status model, rencana migration, rencana data master awal, dan kamus data bertingkat |
| ERD | Kotak entity memuat kolom beserta penanda `PK`, `FK`, dan `UK`, bukan hanya nama tabel dan garis relasi |
| Skema DDL | Kamus data menyertakan bentuk DDL PostgreSQL untuk tabel `Diperbarui`, beserta migration dan cara mundurnya |

## Design gate

Ini adalah desain target, bukan spesifikasi implementasi yang telah disetujui. Gate berikut
tetap berlaku sebelum produksi:

| Gate | Keterangan |
|---|---|
| Clinical governance owner | Belum ditunjuk. Seluruh keputusan klinis pada revisi ini memakai regulasi sebagai baseline, bukan persetujuan klinis |
| Security/privacy owner | Belum ditunjuk. Pemisahan kewenangan SuperAdmin dan mekanisme break-glass memerlukan persetujuannya |
| SOP triase MMC | Target waktu tunggu level 2 sampai 5 belum dapat dikonfigurasi |
| Data master awal | Enam tabel master belum terisi; modul tidak dapat dipakai tanpa ini |
| Scope resource pada authorization | Belum ada di kode |
| Break-glass akses darurat | Belum ada di kode |

Gate yang belum terpenuhi berarti menolak tindakan privileged, integrasi, atau finansial yang
terdampak. Gate **tidak pernah** memblokir pelayanan klinis darurat.

## Artifact hashes

| Artifact | SHA-256 |
|---|---|
| `02-backend-architecture.md` | `c8bfe6a0a8e766062feee97207149dcea311796c7fd44f171b31871763023da5` |
| `03-frontend-architecture.md` | `dc44cb3fa193ca9dc7f461fc4b84cf36dc39214a56011e6653d6a56470991656` |
| `erd/00-context-erd.md` | `3cfc8da39e12f6721aeaabacea2280491e0073777eb58f2d4f8afddbf7fd4d19` |
| `erd/emergency-episode.md` | `a53dc2c83037ca6ac72907a4eb8daee0b8f4e57903db6f519aa40777c6a556a9` |
| `erd/data-dictionary.md` | `2f1d614d400c557dbd53675e7d35b5180f7fb9b3c8a726b204c224305fa8ba73` |
| `contracts/api-contract.md` | `d702684db9acba76f4d24dc040ce28687dfe2093632ed8d6c70149d8d790294e` |
| `contracts/state-transition-matrix.md` | `99899352893ce1e5a1e47f2c0d14394574d136a3a54beecef9b5bc4b747f63a4` |
| `contracts/validation-matrix.md` | `f6b1b1fa99bc89013cbafb2d8bb50e35b6f886ed163df294f89f3d71671b36eb` |
| `contracts/integration-contract.md` | `b6a12e30537bf655667ddf9cb8335f0577b94c17906a9fa1494fac3037e35cf2` |
| `contracts/permission-audit-matrix.md` | `07b1640bdf41a2c9785453ef309f83eb234b445c3592bc083d134ac51db001be` |
| `testing/acceptance-test-matrix.md` | `91d5db395a4e999ab14879da6f52a6d6069d689544f559ae8b9a5656f19204d5` |

Manifest tidak menghitung hash dirinya sendiri. Setiap perubahan material setelah approval
manusia memerlukan revisi baru beserta impact scan backend dan frontend sesuai aturan pada
`01-existing-capability-map.md`.
