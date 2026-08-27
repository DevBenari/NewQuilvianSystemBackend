# IGD Blueprint Manifest

| Field | Value |
|---|---|
| `blueprint_id` | `IGD-BP-001` |
| `revision` | `4` |
| `status` | `approved sebagian` — disetujui Product/Domain Owner; tiga gate go-live tetap terbuka, lihat [Approval 2026-08-14](#approval-2026-08-14) |
| `module` | `igd` |
| `design_snapshot_at` | `2026-08-14` |
| `backend_commit_sha` | `e5331a015fa416a89454b435de0014455f0326d8` |
| `frontend_commit_sha` | `08c84d371ed90640189ce1758019184b0a955e13` |
| `owners` | Product/Domain: pemilik suite skill sebagai pemegang sementara sesuai `IGD-DEC-046`, nama perlu diisi. Clinical governance dan Security/Privacy: `OPEN`, menjadi syarat go-live. Registration, Emergency Installation, Clinical, Pharmacy, Finance, Diagnostic Services, Integration, dan Frontend authority sesuai decision log |
| `approved_by` | Product/Domain Owner sementara sesuai `IGD-DEC-046` — **nama orang belum diisi** |
| `approved_at` | `2026-08-14` |
| `input_revisions` | `00-interview-decisions.md` revision `4`; `01-existing-capability-map.md` revision `2` |
| `input_hashes` | Decisions `sha256:aa2eb549f6725d6e6ea1067eb874caae7323100ab8ee8baa9b0cc29a8e1f87a3`; capability map `sha256:ee02f0697226da3de9b6046a28a86594498a520b8a6a7b6843321f00e3d8da51` |
| `contract_versions` | API `0.2.0`; state `0.2.0`; validation `0.2.0`; integration `0.2.0`; permission/audit `0.2.0` — seluruhnya `approved` dan hash terkunci pada tabel di bawah |
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

Hash berikut dihitung ulang pada 14 Agustus 2026 setelah pencatatan approval, dan inilah hash
yang **terkunci**. Frontend hanya boleh berjalan paralel terhadap hash ini.

| Artifact | SHA-256 |
|---|---|
| `00-interview-decisions.md` | `aa2eb549f6725d6e6ea1067eb874caae7323100ab8ee8baa9b0cc29a8e1f87a3` |
| `01-existing-capability-map.md` | `ee02f0697226da3de9b6046a28a86594498a520b8a6a7b6843321f00e3d8da51` |
| `02-backend-architecture.md` | `458319edf12722cfba24fc4dafbf955642be76853f59f58e0b618db5c3583ff8` |
| `03-frontend-architecture.md` | `14399dcca9ea21821aea24f99ebd42e990e44c5e2e0c4bc0e4d1958142a5f6e1` |
| `erd/00-context-erd.md` | `3cfc8da39e12f6721aeaabacea2280491e0073777eb58f2d4f8afddbf7fd4d19` |
| `erd/emergency-episode.md` | `a53dc2c83037ca6ac72907a4eb8daee0b8f4e57903db6f519aa40777c6a556a9` |
| `erd/data-dictionary.md` | `2f1d614d400c557dbd53675e7d35b5180f7fb9b3c8a726b204c224305fa8ba73` |
| `contracts/api-contract.md` | `f64dea9e9c98a269091b18a5b72d817dc1bf263cdc7692e8a957055dfdb77719` |
| `contracts/state-transition-matrix.md` | `208ddc38ff2367210d8783c29b8d9b2e0b09fa7691a51317006fa848145add5f` |
| `contracts/validation-matrix.md` | `b4bc0a86b8122e9ff20749f9c25497fabea78e49bb0ecf1fbd6a83eac26169ee` |
| `contracts/integration-contract.md` | `79e9d928a2a810d2b8c4fe4987cacd17468bc32c8f4b37fc0b7cbf92f72150ca` |
| `contracts/permission-audit-matrix.md` | `18c36104ca7917136f5cb7d6672ec60d20cd6493579c63b9fc25f014453db83f` |
| `testing/acceptance-test-matrix.md` | `91d5db395a4e999ab14879da6f52a6d6069d689544f559ae8b9a5656f19204d5` |

Manifest tidak menghitung hash dirinya sendiri. Setiap perubahan material setelah approval
manusia memerlukan revisi baru beserta impact scan backend dan frontend sesuai aturan pada
`01-existing-capability-map.md`.

### Koreksi `input_hashes`

Nilai `input_hashes` pada revisi sebelumnya tidak cocok dengan isi berkasnya, persis seperti
yang sudah diperingatkan pada penutup Closure Pass 2026-08-14. Nilai lama
`sha256:a81b1002…` untuk decision log dan `sha256:6d64897f…` untuk capability map tidak
pernah diperbarui. Keduanya diganti dengan hasil hitung ulang di atas.

---

## Approval 2026-08-14

| Field | Nilai |
|---|---|
| Yang menyetujui | Product/Domain Owner sementara sesuai `IGD-DEC-046`; nama orang belum diisi |
| Tanggal | 14 Agustus 2026 |
| Bentuk persetujuan | Pernyataan lisan pemilik proses pada sesi perencanaan delivery |
| Cakupan | Scope, workflow, urutan prioritas, status `Completed`, penanda pelampauan target respons beserta hosted service, dan seluruh kontrak versi `0.2.0` |

Rincian lengkap beserta batas kewenangan ada pada bagian **Approval 2026-08-14** di
`00-interview-decisions.md`.

### Gate yang tetap terbuka setelah approval ini

| Gate | Menunggu | Akibat pada delivery |
|---|---|---|
| Target waktu triase level 2 sampai 5 | SOP triase MMC | Kode boleh dibangun; nilai `MaxWaitingMinutes` tidak boleh ditebak dan tetap `TargetUnconfigured` |
| Pemisahan kewenangan SuperAdmin | Security/privacy owner | Boleh dibangun dan diuji; **tidak boleh diaktifkan di produksi** |
| Break-glass akses darurat | Security/privacy owner | Wajib tersedia lebih dulu sebelum pemisahan SuperAdmin diaktifkan |
| Skema kategori triase sebagai aturan klinis | Clinical governance owner | Tetap berstatus baseline regulasi, bukan persetujuan klinis |
| `GovernanceAssignment` bernama | Sponsor governance MMC | `approved_by` masih berupa peran, bukan orang |

### Impact scan 2026-08-14

| Repository | SHA blueprint | SHA saat perencanaan | Hasil |
|---|---|---|---|
| Backend | `e5331a0` | `389e5167553b6df9cdd650af9d72f08362a844c3` | Nol berkas `.cs`, `.csproj`, dan `.json` berubah. Seluruh perubahan hanya pada dokumen blueprint dan skill |
| Frontend | `08c84d371` | `08c84d371ed90640189ce1758019184b0a955e13` | Tidak berubah sama sekali |

Bukti source pada `01-existing-capability-map.md` karena itu masih sahih dan tidak perlu
diaudit ulang sebelum implementasi dimulai.
