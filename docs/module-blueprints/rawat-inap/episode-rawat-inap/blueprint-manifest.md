# Sub-modul `episode-rawat-inap` — Blueprint Manifest

Sub-modul dari modul [`rawat-inap`](../blueprint-manifest.md), bentuk `COMPOSITE` sejak
`RWI-DEC-082`. Identitas modul, snapshot SHA, hash masukan hulu, dan registry sub-modul dipegang
manifest tingkat modul. Berkas ini memegang **status desain, `contract_versions`,
`artifact_hashes`, approval, dan dependency sub-modul ini sendiri**.

| Field | Value |
|---|---|
| `submodule_slug` | `episode-rawat-inap` |
| `blueprint_id` | `RWI-BP-001` — satu untuk seluruh modul |
| `revision` | **`19`** — Delivery Planning Peta Bangsal Denah, bagian 24, **`APPROVED_DESIGN_PLANNED`** (`RWI-DEC-352`, `RWI-DEC-353`, planning `PETA-DEL-20261020-01`, kontrak `0.18.0`). Riwayat metadata: `18` Desain Peta Bangsal Denah draft (bagian 23); `17` — Delivery Planning Prioritas Kamar Pasien Member di Antrean Transfer & Lencana Tier, bagian 22, **`APPROVED_DESIGN_PLANNED`** (`RWI-DEC-338` s.d. `341`, planning `TRF-DEL-20261019-01`, kontrak `0.17.0`). Riwayat metadata: `16` Realtime Bed Selection planned (`RWI-DEC-337`, `RTB-DEL-20261017-01`, `0.16.0`); `15` realtime draft; `14` Pendaftaran Admisi ODC; `13` Transfer Pasien Masuk & PPJA; `12` Integrasi Denah (`RWI-DEC-324`); `11` draft Bed Management |
| `status` | **`approved`** untuk revision 19 Peta Bangsal Denah (`0.18.0` `approved_design_planned`, `RWI-DEC-352`); **`approved`** untuk revision 17 Prioritas Member & Lencana Tier (`0.17.0`), revision 16 Realtime Bed Selection (`0.16.0`), revision 14 ODC (`0.15.0`), revision 13 Transfer Pasien Masuk (`0.14.0`), dan revision 12 Integrasi Denah (`0.13.0`); Bed Management revision 11 tetap **`draft`** pada scope terpisah |
| `contract_versions` | **`0.18.0`** — Peta Bangsal Denah (`approved_design_planned`, `RWI-DEC-352`, 2026-10-20); `0.17.0` — Prioritas Kamar Pasien Member di Antrean Transfer & Lencana Tier (`approved_design_planned`, `RWI-DEC-338` s.d. `341`, 2026-10-19); `0.16.0` Notifikasi Realtime Pemilihan Bed (`approved_design_planned`, `RWI-DEC-337`); `0.15.0` ODC (`approved_design_planned`); `0.14.0` Transfer Pasien Masuk (`approved`); `0.13.0` Integrasi Denah (`approved`); `0.12.0` Bed Management draft; `0.11.0` Admisi & Workspace PPRI (`RWI-DEC-265`) |
| `upstream_realignment` | **`REALIGNED_APPROVED` sejak 2026-09-16** lewat `RWI-DEC-150` |
| `prefix` | Entity `Inp`; task `BE-RWI-###` dan `FE-RWI-###` |
| `approved_by` | **Muhammad Hamzah** (Revision 19 Peta Bangsal Denah lewat `RWI-DEC-352`, 20 Oktober 2026; Revision 17 lewat `/plan-module-delivery` 19 Oktober 2026; revision 16 melalui `RWI-DEC-337`; revision 14 lewat `/plan-module-delivery`; revision 13 & 12 melalui approval 2026-10-16) |
| `approved_at` | `2026-10-20` untuk revision 19; `2026-10-19` untuk revision 17; `2026-10-17` untuk revision 16 dan 14; `2026-10-16` untuk revision 13 & 12 |
| `rumpun kemampuan` | Episode, tempat tidur, penanggung jawab, pemulangan, penutupan, admisi & consent, serah terima transfer, admisi ODC, notifikasi realtime pemilihan bed, prioritas antrean transfer kamar member & lencana tier |
| `kemampuan` | **16** — `CAP-001` s.d. `CAP-011`, `CAP-017`, `CAP-018`, `CAP-019`, `CAP-026`, `CAP-028`, sesuai `RWI-DEC-083`; **ditambah Workspace PPRI** `CAP-RWA-01` s.d. `CAP-RWA-17` tanpa `CAP-RWA-04` (`RWI-DEC-227`); **ditambah Amandemen Admisi & Digital Consent** (`RWI-DEC-267` s.d. `273`); **ditambah Integrasi Denah ↔ Bed Management** `DNH-CG-01` s.d. `06` (`RWI-DEC-298` s.d. `323`); **ditambah Transfer Pasien Masuk & PPJA** (`FR-TRF-001` s.d. `007`); **ditambah Pendaftaran Admisi Pasien ODC** (`FR-ODC-001` s.d. `007`, `RWI-DEC-327` s.d. `332`); **ditambah Prioritas Kamar Member & Lencana Tier** (`FR-TRF-008` s.d. `012`, `RWI-DEC-338` s.d. `341`) |
| `uji pemecahan` | **5/5** syarat `bentuk-blueprint.md` bagian 4.1 |
| `design_snapshot_at` | 2026-10-20 untuk Peta Bangsal Denah; 2026-10-19 untuk Prioritas Kamar Pasien Member di Antrean Transfer & Lencana Tier; 2026-10-17 untuk Realtime Bed & ODC; 2026-10-16 untuk Transfer Pasien Masuk; 2026-10-15 untuk Integrasi Denah |
| `updated_at` | 2026-10-20 — delivery planning Peta Bangsal Denah (`PETA-DEL-20261020-01`, kontrak `0.18.0` `approved_design_planned`, bagian 24). Sebelumnya desain draft bagian 23; sebelumnya 2026-10-19 — perencanaan delivery Prioritas Kamar Pasien Member di Antrean Transfer & Lencana Tier (`TRF-DEL-20261019-01`, kontrak `0.17.0`) |

**Bukti migration diperbarui 5 Oktober 2026.** Build project melalui `dotnet ef database update` berhasil dan `20261005033044_AddRawatInapFinishing` diterapkan sampai `Done.` menurut output pengguna. Cakupan aktual: `I1` + `I2`, `K8` + `E4`, `E5`, `E6`, dan `E7`; enam task pemilik perubahan skema. [Bukti lengkap](task/report/backend/BE-RWI-172.md#51-pembaruan-bukti-5-oktober-2026). `I1`/`I2` dikemas sebagai satu migration gabungan; catatan kesesuaian kriteria ada di `BE-RWI-149`. Nama database/lingkungan tidak disebut; API, regresi, rollback, frontend, data seeder, serta kesiapan rilis belum dibuktikan output ini. Revision, snapshot SHA desain, kontrak, artifact hash persetujuan, dan approval roadmap dipertahankan.

---

## 0. Apa yang berubah pada sub-modul ini saat migrasi bentuk

**Isi desain tidak berubah sama sekali.** Migrasi `SINGLE` → `COMPOSITE` 2026-09-02 memindahkan
berkas dan memindahkan tiga tabel ke tingkat modul. Tidak ada tabel baru, kolom baru, endpoint baru,
aturan baru, maupun kontrak yang naik versi.

| Yang berubah | Rinciannya |
|---|---|
| Letak seluruh artefak | Dari `rawat-inap/` menjadi `rawat-inap/episode-rawat-inap/`, termasuk `roadmap/` dan `task/` |
| Kamus data | `erd/data-dictionary.md` → `data/data-dictionary.md`, memenuhi `blueprint-output-contract.md` bagian 1.2. Isinya tidak disunting; 13 rujukan ke path lama diperbaiki |
| Tabel kepemilikan data | Naik ke [`../02-module-map.md`](../02-module-map.md) bagian 2. Yang tinggal di `02-backend-architecture.md` bagian 2 hanya kelompok data milik sub-modul ini |
| Peta butir menu | Naik ke [`../02-module-map.md`](../02-module-map.md) bagian 3 |
| Urutan migration antar sub-modul | Naik ke [`../02-module-map.md`](../02-module-map.md) bagian 3.4 sebagai gelombang `M1` dan `M2` |
| Satu baris kepemilikan | "Dokumentasi klinis, resep, tindakan" keluar dari sub-modul ini; `RWI-DEC-083` memberikannya ke `keperawatan` dan `dokter-rawat-inap` |
| Keterangan basi `DEC-INP-001` | **Enam** keterangan pada `04-prd-to-mvp.md` bagian 7, 8, 14, dan 16, ditambah tiga pada `../evidence/02-requirement-completeness-gate.md`. `DEC-INP-001` sudah tertutup `RWI-DEC-062` sejak 2026-08-21 |

---

## 1. Peringatan sebelum membaca

Seluruh dokumen desain pada folder ini berstatus `draft` sebagai artefak, walaupun revision `4`
sudah **disetujui pemilik** sebagai keputusan. Penulisan source code sudah dibuka lewat
`RWI-DEC-067`, satu task per pengerjaan mengikuti roadmap dan gerbang task terkini.

Dua gerbang blueprint lama masih terbuka: kesiapan data master, dan persetujuan pemilik
`EmergencyInstallationManagement` yang hanya menahan `INP-S09`. Impact scan frontend 28 Agustus 2026
menambahkan tujuh gerbang `RWI-UI-GAP-001` s.d. `007` pada roadmap frontend revision `5` draft;
masing-masing hanya menahan task yang ditunjuk di sana.

---

## 2. Daftar artefak dan hash

| Artefak | Revision | Status | SHA-256 |
|---|---|---|---|
| [`02-backend-architecture.md`](./02-backend-architecture.md) | `0.7` | `draft` | hash dihitung ulang saat approval `0.8.0` |
| [`03-frontend-architecture.md`](./03-frontend-architecture.md) | `0.7` | `draft` | hash dihitung ulang saat approval `0.8.0` |
| [`04-prd-to-mvp.md`](./04-prd-to-mvp.md) | `0.6.1` | `draft` | `99814d79eb09cce6b2a528deff6751d89915914a8f17b03f36e3c9c351c02a23` |
| [`05-skema-tampilan.md`](./05-skema-tampilan.md) | `0.4` | `draft` | `f74a845433ba64806ee1cd945f8ca515228af2a470082c4095f95f682ceed09e` |
| [`data/data-dictionary.md`](./data/data-dictionary.md) | `0.5` | `draft` | hash dihitung ulang saat approval `0.8.0` |
| [`erd/00-context-erd.md`](./erd/00-context-erd.md) | `0.3` | `draft` | `73eaa7d0c6d0567a37380679b4c9c0fd150d75a8851e7b6fd4b1d5f4e28a41e4` |
| [`erd/01-inpatient-episode.md`](./erd/01-inpatient-episode.md) | `0.3` | `draft` | `7f21508a0f66470b9b6b1d625359636882b89481318b16da7472735e916449eb` |
| [`erd/02-inpatient-configuration.md`](./erd/02-inpatient-configuration.md) | `0.1` | `draft` | `3645ee9d1788270ee7cef88d2cc6b74beddddec0a1a5d2b538e45c25c66f2065` |
| [`contracts/api-contract.md`](./contracts/api-contract.md) | `0.8.0` | `draft` | hash dihitung ulang saat approval `0.8.0` |
| [`contracts/state-transition-matrix.md`](./contracts/state-transition-matrix.md) | `0.4.0` | `draft` | `35e8e769461a05b32da5d9e6d11ef92dc45c254b2c1a7d4eb08d228a5d9c1fc7` |
| [`contracts/validation-matrix.md`](./contracts/validation-matrix.md) | `0.8.0` | `draft` | hash dihitung ulang saat approval `0.8.0` |
| [`contracts/integration-contract.md`](./contracts/integration-contract.md) | `0.4.0` | `draft` | `99ef4d4fb982987fa25b51dc49720344366a6bb42d31f8c7c6b153070a62aab0` |
| [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md) | `0.6.1` | `draft` | `a0ba4ad5f8f4d587dff5c8321fc2c33fe1be825c32cb2fed06099b1e9858d6e9` |
| [`testing/acceptance-test-matrix.md`](./testing/acceptance-test-matrix.md) | `0.4.0` | `draft` | `357cb6ca9b35b9c2a2ce55597dd2cad5c68bd132c4d40a903f07e4d693b3a45c` |

> **Pemutakhiran kedua 2026-09-08 sore — `/qv-trace` setelah merge.** Empat artefak naik ke
> `0.6.1`: kolom `EpisodeId` dibatalkan (`RWI-FACT-017`), rute refund dikembalikan ke `BIL-API-0.4`,
> dan dua aturan validasi dicabut karena sudah dijaga unique index. Baseline SHA ikut bergeser ke
> `44099e4` backend dan `30db3734` frontend.
>
> **Pemutakhiran 2026-09-08 — Amendment Pass deposit admisi.** Lima artefak di atas naik karena
> `RWI-DEC-093` s.d. `RWI-DEC-096`. `04-prd-to-mvp.md` naik dua tingkat sekaligus, `0.4.1` → `0.6.0`,
> karena revisi `0.5.0` sempat disusun di luar pohon blueprint pada `docs/Modul-RS/Rawat-Inap/` lalu
> diport masuk; berkas asalnya ditandai `SUPERSEDED`. `state-transition-matrix.md`,
> `integration-contract.md`, dan `acceptance-test-matrix.md` **tidak** ikut naik: deposit tidak
> menambah status episode dan tidak mengubah integrasi yang sudah tercatat. Kedua roadmap ditandai
> `replan_required: true` dan hash inputnya sengaja dibiarkan lama.

Revision `0.5` pada kedua berkas arsitektur menandai **satu-satunya** perubahan isi saat migrasi:
pemindahan tabel kepemilikan data dan peta butir menu ke tingkat modul. `04-prd-to-mvp.md` naik ke
`0.4.1` karena enam keterangan basi `DEC-INP-001` diperbaiki. Sisanya hanya berpindah tempat, atau
berubah hanya pada rujukan path, sehingga revision-nya tidak bergerak.

### 2.1 Roadmap dan traceability

Ditulis `/qv-plan`, bukan skill desain. Ketiganya di-resync ke masukan revision `5` pada
2026-09-02, lalu **ditulis ulang 2026-09-08** untuk memuat slice deposit `EPIC RI-35`.

| Artefak | Revision | Status | Gerbang |
|---|---|---|---|
| [`roadmap/backend-roadmap.md`](./roadmap/backend-roadmap.md) | `4` | `DRAFT` | `BLUEPRINT_APPROVED` — 36 dari 39 task selesai. Dua task deposit dibatalkan dan dua lagi dipindahkan ke roadmap `billing-kasir` pada 2026-09-08. Revision `4` menambah tujuh task deposit — dua di antaranya sudah dibatalkan — dan **belum disetujui**; approval revision `3` tidak meluas ke sana. **Dua** di antaranya `BLOCKED` oleh `RWI-OQ-053` |
| [`roadmap/frontend-roadmap.md`](./roadmap/frontend-roadmap.md) | `7` | `DRAFT` | `UI_SCHEMA_APPROVAL_REQUIRED` — **diperbarui 12 September 2026: 24 dari 28 task sub-modul ini selesai.** Yang tersisa empat: `FE-RWI-035` 🟡 6 dari 8 kriteria, serta `FE-RWI-059`, `FE-RWI-060`, dan `FE-RWI-061`. ~~Empat task deposit `FE-RWI-058` s.d. `FE-RWI-061` seluruhnya menunggu `RWI-UI-GAP-008`~~ — **gerbang itu ditutup 12 September 2026** dengan menulis skema `FE-INP-20` pada `05-skema-tampilan.md` bagian 3.5A, revision naik `0.4` → `0.5`, status `draft` belum disetujui pemilik. `FE-RWI-058` ✅ selesai; ketiga task deposit sisanya ternyata tertahan endpoint Billing yang **nol barisnya ada**, bukan skema |
| [`roadmap/requirement-traceability.md`](./roadmap/requirement-traceability.md) | `7` | `DRAFT` | Mengikuti roadmap frontend. Bagian `EPIC RI-35` ditambahkan dengan kolom AC **sengaja kosong**; penomoran `RWI-AC-181` dan seterusnya milik `/qv-design` |
| **[`roadmap/backend-roadmap-v2.md`](./roadmap/backend-roadmap-v2.md)** ★ | `1` | **`APPROVED`** | `BLUEPRINT_APPROVED` `RWI-DEC-150` — **berkas baru 2026-09-16**, fase `RLN-PH-07`. Sembilan task `BE-RWI-079` s.d. `BE-RWI-087`, gelombang `RI-V2-1` s.d. `RI-V2-3`. Berkas lama tetap berlaku untuk task revision `4`–`6` |
| **[`roadmap/frontend-roadmap-v2.md`](./roadmap/frontend-roadmap-v2.md)** ★ | `1` | **`APPROVED`** | `BLUEPRINT_APPROVED` `RWI-DEC-150` — **berkas baru 2026-09-16**. Empat task `FE-RWI-063` s.d. `FE-RWI-066` |
| **[`roadmap/requirement-traceability-v2.md`](./roadmap/requirement-traceability-v2.md)** ★ | `1` | **`APPROVED`** | Sebelas FR `FR-RI-191` s.d. `FR-RI-201`; nol gap requirement ke bukti |

Revision `3` dan `6` berlingkup `INPUT_RESYNC_ONLY`: nol task ditambah, diubah, atau dihapus.
Revision `4` backend dan `7` frontend berlingkup `DEPOSIT_SLICE`: sebelas task baru ditambahkan,
nol task lama diubah atau dihapus.

Dua kontrak tambahan di luar himpunan canonical, dipertahankan apa adanya:

| Artefak | Keterangan |
|---|---|
| [`contracts/bed-board-reservation-metadata-contract.md`](./contracts/bed-board-reservation-metadata-contract.md) | Kontrak turunan papan tempat tidur |
| [`contracts/encounter-company-guarantor-contract.md`](./contracts/encounter-company-guarantor-contract.md) | Kontrak turunan penjamin perusahaan |

---

## 3. Contract version

`contract_versions` sub-modul ini bergerak sendiri; angka `keperawatan` dan `dokter-rawat-inap`
tidak ikut terseret.

| Kontrak | Version | `last_changed_in` | Status | Berubah isinya pada migrasi bentuk? |
|---|---|---|---|---|
| API | `0.4.0` | `0.6.1` | `draft` | **Ya** — bagian Deposit Rawat Inap baru, `episodeId` wajib pada top-up, `GET /monitoring/deposit-shortfall` |
| State transition | `0.4.0` | `0.4.0` | `draft` | **Tidak** |
| Validation | `0.4.0` | `0.6.1` | `draft` | **Ya** — bagian 8A deposit, termasuk dua baris peringatan yang sengaja tanpa kode kesalahan |
| Integration | `0.4.0` | `0.4.0` | `draft` | **Tidak** |
| Permission dan audit | `0.4.0` | `0.6.1` | `draft` | **Ya** — bagian 2.8 `BillingDeposit`, dua aksi baru `Settle` dan `Refund`; `InpatientDeposit` dicabut |
| Acceptance test | `0.4.0` | `0.4.0` | `draft` | **Tidak** |
| PRD ke MVP | `0.4.0` | `0.6.1` | `draft` | **Ya** — `EPIC RI-35` deposit dan settlement, `FR-RI-163` s.d. `FR-RI-178`, `UAT-34` s.d. `UAT-44`, `EPIC RI-35` dipecah dua gelombang |

**Tidak satu pun kontrak dinaikkan versinya oleh migrasi bentuk.** Menaikkannya akan membuat pembaca
mengira ada endpoint atau aturan yang bergeser, padahal tidak ada.

---

## 4. Penyimpangan struktur yang diketahui

`blueprint-output-contract.md` bagian 1.2 menuntut `flowcharts/00-alur-utama.md` pada setiap
`<blueprint-root>`. Sub-modul ini **belum punya**, dan itu **bukan** akibat migrasi bentuk.

| Hal | Keadaannya |
|---|---|
| Sebabnya | Blueprint ini disusun revision `1` s.d. `4` memakai struktur `erd/`, sebelum kontrak mengganti ERD dengan flowchart alur proses |
| Yang menggantikan sementara | `erd/` tiga berkas untuk relasi entity, dan `04-prd-to-mvp.md` bagian 9 `FLOW-RI-MVP-001` untuk urutan langkah petugas |
| Yang tetap hilang | Jalur **gagal** dalam bentuk diagram, beserta tabel langkah pendampingnya |
| Kenapa tidak dibuat sekarang | Membuatnya adalah pekerjaan desain baru, bukan pemindahan berkas. Migrasi bentuk sengaja tidak menyelipkan desain baru |
| Kapan ditutup | Saat `episode-rawat-inap` berikutnya disentuh `/qv-design`, atau lebih awal bila diminta pemilik |

Dicatat di sini supaya pembaca dapat membedakan "memang belum dibuat" dari "terlupa ditulis".

---

## 5. Scope yang dirancang

| Slice | Nama | Epic |
|---|---|---|
| `INP-S01` | Admisi dan pemesanan tempat tidur | `EPIC RI-21`, `EPIC RI-22` |
| `INP-S02` | Penempatan, census, lama dirawat | `EPIC RI-23`, `EPIC RI-24` |
| `INP-S03` | Perpindahan dan pindah kelas | `EPIC RI-26` |
| `INP-S04` | Penugasan perawat | `EPIC RI-25` |
| `INP-S07` sebagian | Keputusan pulang dan resume, tiga cara pulang | `EPIC RI-27` |
| `INP-S08` sebagian | Daftar periksa, kelayakan keuangan, penutupan | `EPIC RI-28` |
| `INP-S11` | Kelayakan penempatan menurut jenis kelamin dan isolasi | `EPIC RI-34` |
| `INP-S12` | Bayi baru lahir dan boks bayi | `EPIC RI-33` |
| `INP-S13` | Riwayat status, audit, dua daftar pantau | `EPIC RI-29`, `EPIC RI-30` |
| `INP-S14` | Pengaturan yang dapat diubah admin | `EPIC RI-31` |
| — | Perbaikan tempat tidur dan pembatasan wewenang status | `EPIC RI-32` |

## 6. Scope yang sengaja tidak dirancang di sub-modul ini

| Slice | Kenapa bukan milik sub-modul ini |
|---|---|
| `INP-S05` dokumentasi klinis dan visite | **Berpindah sub-modul** 2026-09-02. `RWI-DEC-083` memberikannya ke `keperawatan` dan `dokter-rawat-inap`. **Bukan** lagi ditahan `DEC-INP-001`, yang tertutup `RWI-DEC-062` |
| `INP-S06` resep dan obat pulang | Sama — kini `CAP-023` milik `dokter-rawat-inap` |
| `INP-S09` serah terima IGD | Tetap ditahan `DEC-INP-002`; pemiliknya Rizki Gunawan, persetujuan formalnya belum tercatat |
| `INP-S10` persetujuan umum | Tetap ditahan `DEC-INP-003`. Cetak tanpa simpan tersedia lewat `RWI-DEC-077` |
| `INP-S15` interoperabilitas SATUSEHAT | Tetap ditahan `DEC-INP-005` |
| Serah terima klinis antar shift | Tetap ditahan `DEC-INP-006` |
| Cara pulang meninggal dan kabur | Tetap ditahan `DEC-INP-007` |

## 7. Dependency sub-modul ini

| Bergantung pada | Untuk apa | Keadaan |
|---|---|---|
| `RegistrationManagement` | Kunjungan sebagai jangkar episode | Tersedia |
| `MasterData` HealthServices | Tempat tidur, kamar, unit layanan, kelas | Tersedia; persetujuan menulis `MstBed.BedStatus` diberikan `RWI-DEC-062` |
| `Corporate HR Workforce` | Dokter dan pegawai | Tersedia |
| `ClinicalManagement` | Surat keterangan medis | Tersedia |
| `BillingManagement` | Kelayakan keuangan | **Tidak dibutuhkan MVP** — ditandai manual kasir. Sumber kebenarannya terbuka, `RWI-OQ-047` |
| `EmergencyInstallationManagement` | Serah terima disposisi `RANAP` | **Belum disetujui** — `DEC-INP-002`, hanya menahan `INP-S09` |
| `keperawatan` | Tidak ada | Sub-modul ini **tidak bergantung** padanya |
| `dokter-rawat-inap` | Tidak ada | Sub-modul ini **tidak bergantung** padanya |

Dua baris terakhir adalah alasan sub-modul ini boleh berjalan sendiri.

## 8. Yang tidak boleh diubah blueprint hilir

Diwariskan dari arsitektur domain bagian N.5. Perubahan pada butir berikut wajib kembali ke skill
hulu, bukan diselesaikan pada tahap perencanaan atau implementasi:

1. Kepemilikan data pada [`../02-module-map.md`](../02-module-map.md) bagian 2.
2. Kedudukan `MstBed.BedStatus` sebagai **salinan**, bukan sumber kebenaran.
3. Sepuluh invariant `INV-INP-01` sampai `INV-INP-10` beserta cara menjaganya.
4. Bentuk **berperiode** pada `InpDoctorAssignment`, `InpNurseAssignment`, dan `InpBedPlacement`.
5. Kedudukan `InpCorrectionSession` sebagai konsep tersendiri, bukan status episode keenam.
6. Kebutuhan isolasi sebagai **atribut episode**, bukan atribut pasien dan bukan status.
7. Aturan pencampuran kamar diperiksa dari **penghuni yang sedang ada**, bukan dari penanda pada
   `MstRoom`.

## 9. Langkah berikutnya untuk sub-modul ini

| Kondisi | Skill |
|---|---|
| Empat pertanyaan memblokir pada `04-prd-to-mvp.md` bagian 20.2 sudah terjawab | `/qv-plan` |
| Pertanyaan tidak memblokir ingin ditutup lebih dulu | `/qv-grill` Amendment Pass |
| `backend_commit_sha` atau `frontend_commit_sha` berubah | `/qv-trace` impact scan |
| `flowcharts/` ingin dilengkapi | `/qv-design` untuk sub-modul ini saja |

---

## 10. Amandemen terbatas revision `7` — penyelarasan `PRD-RWI-V2-001` ★ 15 September 2026

**Status sub-modul: `approved`.** ~~`draft`~~ — **disetujui Muhammad Hamzah 2026-09-16 lewat `RWI-DEC-150`**.
Pilihan pemilik 15 September 2026: **amandemen terbatas** — hanya yang diminta `dokter-rawat-inap` `0.6.0` dan
`keperawatan` `0.5.0`. Approval revision `6` / kontrak `0.8.0` tetap sah untuk isi yang tidak berubah; task `✅` tetap
sah; **task baru kini boleh diturunkan dari revision ini** lewat `plan-module-delivery` fase `RLN-PH-07`. Approval
ini adalah approval **desain dan kontrak**, bukan wewenang menulis source, migration, database, maupun deployment.

### 10.1 Tabel artefak dan hash

| Artefak | Revision | Status | SHA-256 |
| --- | --- | --- | --- |
| [`02-backend-architecture.md`](./02-backend-architecture.md) | **`0.8`** — bagian 11 | `approved` | `062580a56bde353f67c12f0456bbd2833ad826c7cb4c22f9b6f1dcb6f5227cda` |
| [`03-frontend-architecture.md`](./03-frontend-architecture.md) | **`0.8`** — bagian 12 | `approved` | `efd00c4b73b4144cbe96fb9bc091a6b2940a0e5a2f409d9e1a2b687e0b9271ac` |
| [`04-prd-to-mvp.md`](./04-prd-to-mvp.md) | **`0.8.0`** — bagian 22 | `approved` | `d1550e4e72f548363b588c127dfb747176bfda3a64c608c5f6a5988c0341a2d3` |
| [`data/data-dictionary.md`](./data/data-dictionary.md) | **`0.5`** — bagian 18 | `approved` | `e3c05e2043856034ca9d6756c6466f5abfc5567947a1a9e5a4267f6430397407` |
| [`contracts/api-contract.md`](./contracts/api-contract.md) | **`0.9.0`** — bagian 10 | `approved` | `6219c2c53e29ac31631573699a20db13c5e684e0f961e8d2ab30f7934325ddec` |
| [`contracts/state-transition-matrix.md`](./contracts/state-transition-matrix.md) | **`0.9.0`** — bagian 8 | `approved` | `89d3948b58f23b13a8543f8382eaa042ef8659f45ffd3bd9f9321dbe78b72439` |
| [`contracts/validation-matrix.md`](./contracts/validation-matrix.md) | **`0.9.0`** — bagian 13 | `approved` | `501f9ce338eb201da3ff697c8adee67e7c9de86aff68efcc4aa3c14de7141b91` |
| [`contracts/integration-contract.md`](./contracts/integration-contract.md) | **`0.9.0`** — bagian 8 | `approved` | `a78d8250d5664c205bb8598ca8800a56d13c3a39ded2f8a989a554d49381ce8a` |
| [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md) | **`0.9.0`** — bagian 8 | `approved` | `ace9f1f53156e2a9454a31fc5183131302bbbdfe1a5981dbeaa6921617eb727b` |
| [`testing/acceptance-test-matrix.md`](./testing/acceptance-test-matrix.md) | **`0.9.0`** — bagian 18 | `approved` | `b5b290f221934ec1a6136066d47cee4b80ca256899db5bd750d839eb72a3a140` |

> **Status artefak 2026-09-16.** 10 artefak revision `7` naik dari `draft` menjadi `approved` lewat `RWI-DEC-150`. Baris bertanda "tidak bergerak" sudah `approved` sejak revision sebelumnya. SHA-256 di atas adalah hash saat approval dan menjadi acuan deteksi perubahan berikutnya.

Tiga kontrak pendamping (`bed-board-reservation-metadata`, `encounter-company-guarantor`,
`encounter-payment-source-change`), `erd/`, dan `05-skema-tampilan.md` **tidak bergerak**.

### 10.2 Yang berubah terhadap bagian 7 dan 8

| Bagian | Perubahan |
| --- | --- |
| 7 Dependency | Dua baris "Tidak ada" untuk `keperawatan` dan `dokter-rawat-inap` **tidak lagi sepenuhnya benar**: langkah penutupan 4–6 memanggil service yang dirancang kedua sub-modul itu dan `MedicalRecordManagement`. Sub-modul ini tetap **boleh berjalan sendiri**, karena langkah yang service-nya belum ada tidak dipasang (`02-backend-architecture.md` 11.8) |
| 8 butir 3 | Invariant bertambah `INV-INP-11` s.d. `13` — berlaku setelah revision `7` disetujui |
| 8 butir 4 | Bentuk berperiode `InpDoctorAssignment` tetap; kolom tujuan tidak mengubahnya |

### 10.3 Dependency dan gerbang — revision `7`

| Bergantung pada | Untuk apa | Keadaan | Menahan |
| --- | --- | --- | --- |
| `dokter-rawat-inap` `0.6.0` | `PatientProcedureOrderService`, `CpptVerificationService` | Dirancang pada pass yang sama, `draft` | `FR-RI-199`; angka Perlu Review |
| `keperawatan` `0.5.0` | `MedicationAdministrationService` | Dirancang pada pass yang sama, `draft` | `FR-RI-200` |
| Yoga Aji Pratama — `MedicalRecordManagement` | Pemberitahuan pemanggil penguncian | **Belum diberitahu** | Rilis `RI-V2-1` |
| Pemilik Billing | Nasib pesanan tertunda tertagih | Belum dibahas | Tidak menahan; daftar pantau tersedia |
| Pemilik klinis | Isi minimal resume | **Belum ditunjuk** | Gerbang produksi |

### 10.4 Handoff

| Field | Nilai |
| --- | --- |
| `blueprint_id` / `revision` | `RWI-BP-001` / `7` |
| `contract_versions` | `0.9.0` |
| `approval_status` | **`approved`** — `approved_by` Muhammad Hamzah, `approved_at` 2026-09-16, lewat `RWI-DEC-150` |
| `blocking_questions` | **Nol** — `04-prd-to-mvp.md` 22.7 seluruhnya tidak memblokir |
| `next_owner` | ~~Approval pemilik atas revision `7`~~ **SELESAI 2026-09-16** — `plan-module-delivery` untuk `RI-V2-1` s.d. `RI-V2-3` |

---

## 11. Amandemen kontrak `0.10.0` — Finishing Rawat Inap ★ 1 Oktober 2026

**Status amandemen: `approved`** — disetujui Muhammad Hamzah 2026-10-02 lewat `RWI-DEC-221`. Revision `7` / kontrak `0.9.0` tetap `approved` (`RWI-DEC-150`) untuk isi yang tidak disentuh. Revision ini juga **melahirkan folder `flowcharts/`** untuk sub-modul ini. Approval dicatat atas pernyataan pemilik pada sesi 2 Oktober 2026; ini approval desain dan kontrak, **bukan** wewenang menulis source, migration, database, maupun deployment. **Diselaraskan 2 Oktober 2026** dengan decision log revision `31` (`RWI-DEC-206` s.d. `220`) tanpa menaikkan versi kontrak; artefak yang terdampak memuat bagian Penyelarasan decision log revision `31`.

| Field | Nilai |
| --- | --- |
| `contract_version` | `0.9.0` → **`0.10.0`** (`approved` 2026-10-02, `RWI-DEC-221`) |
| `input_revision` | `PRD-RWI-FINISHING-001` v`0.4`; decision log revision `31` (diselaraskan 2 Oktober 2026); gate `1.9` bagian 18; capability map `1.6` bagian 19 |
| `backend_commit_sha` | Audit `c8e99ce5`; diperiksa ulang terhadap HEAD **`8d96a978`** (1 dan 2 Oktober 2026) — perubahan sesudah audit hanya saringan pencarian census dan daftar pantau; tidak menyentuh area yang dirancang |
| `frontend_commit_sha` | Audit `22ad67330`; diperiksa ulang terhadap HEAD **`bf5af8090`** — gaya tampilan, pencarian, label menu Dashboard; tidak menyentuh area yang dirancang |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` — batas domain diambil dari keputusan pemilik dan source yang dibaca |
| Kemampuan | `CAP-RWF-07`, `08`, `16`, `18`, `19`, `22`, `23` — slice `INP-S27`, `INP-S31`, `INP-S32` (gate `1.9` `PARTIALLY_READY`; penahannya ditutup `RWI-DEC-207`), `INP-S33`, `INP-S36`, `INP-S37` |
| Keputusan | `RWI-DEC-173` s.d. `177`, `182`, `189`, `196`, `197`, `199`, `201`, `204`, `205`; penyelarasan `RWI-DEC-207`, `208`, `213` s.d. `220` |
| Peta modul | `../02-module-map.md` revision `4` bagian 7 — kepemilikan data, menu, urutan migration lintas sub-modul |

### 11.1 Tabel artefak dan hash

| Artefak | Bagian | Status | SHA-256 |
| --- | --- | --- | --- |
| [`02-backend-architecture.md`](./02-backend-architecture.md) | revision `0.9` — bagian 12 | `approved` | `b02d92c008b3aa198f81e5b578e6a9a814b2271fcd26d0192790fe145017e00e` |
| [`03-frontend-architecture.md`](./03-frontend-architecture.md) | revision `0.9` — bagian 13 | `approved` | `e31154bd950a1b66b894500431ce7b2f15c6f0bcbbb3b2cace1b5dc3c8ed3a1a` |
| [`04-prd-to-mvp.md`](./04-prd-to-mvp.md) | bagian 23 | `approved` | `df5e0b98e02a9a6f9e02974113188973fd1e94695bf3f48a6d56258b147f0176` |
| [`contracts/api-contract.md`](./contracts/api-contract.md) | bagian 11 | `approved` | `958fcd2c74d0f7164addd2fb4d4beda8d798bbb316476aeeaec8ce190216fd75` |
| [`contracts/state-transition-matrix.md`](./contracts/state-transition-matrix.md) | bagian 9 | `approved` | `d848afe62b1b6ca92253808aa081616e4a6ee23fed1651c0105ec625ffc2fd17` |
| [`contracts/validation-matrix.md`](./contracts/validation-matrix.md) | bagian 14 | `approved` | `705ee1fb47cda72e9cc63929c12a9dde35dccb8a19c11238c7638cd016b9810b` |
| [`contracts/integration-contract.md`](./contracts/integration-contract.md) | bagian 9 | `approved` | `db7aab67e75da4bcef45c1511edacde515648692c4086d82f3ffce1041c0810e` |
| [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md) | bagian 9 | `approved` | `ef1076fa7b987dd9636d680cc0630aedc9d7f8c152135c8fd1e54b17d42f58b4` |
| [`data/data-dictionary.md`](./data/data-dictionary.md) | revision `0.6` — bagian 19 | `approved` | `87f201ceea160ee8b2a84965bf6827dbbee5a8cbcdec5489de3b8073bea530df` |
| [`testing/acceptance-test-matrix.md`](./testing/acceptance-test-matrix.md) | bagian 20 | `approved` | `8e1e2b5ded25c3eb210f7febffee5e3589502df6022b504dd4d2803385420772` |
| [`flowcharts/00-alur-utama.md`](./flowcharts/00-alur-utama.md) | baru | `approved` | `020aac4d84fdeec48e9159e045fb9e2aa8b1b4f367c9945d4ef953e3be9d6f2e` |
| [`flowcharts/01-pemesanan-dan-pra-operasi.md`](./flowcharts/01-pemesanan-dan-pra-operasi.md) | baru | `approved` | `47d1825d6dc3f45a5cfb2114046750139f742449e489f384cfb86bc6fd5cc6dd` |
| [`flowcharts/02-serah-terima-dan-biaya-operasi.md`](./flowcharts/02-serah-terima-dan-biaya-operasi.md) | baru | `approved` | `62ec02f30f192022ec4bc7fb1bb46e6f7d91da9174a524bf132f9418ebba7585` |
| [`flowcharts/03-admisi-dari-kamar-pulih.md`](./flowcharts/03-admisi-dari-kamar-pulih.md) | baru | `approved` | `f43bf5fc42f71b66bcee12d5ff75cd0dc7fd8d328053a87aaa1d61eea6555135` |
| [`flowcharts/04-serah-terima-transfer.md`](./flowcharts/04-serah-terima-transfer.md) | baru | `approved` | `230c402acd848ba502d063a5ce436ffb6a9cdd819a537edfd8ef0fb160bae713` |
| [`roadmap/backend-roadmap-finishing.md`](./roadmap/backend-roadmap-finishing.md) | revision `1` — `BE-RWI-172` s.d. `184` | `DRAFT` — menunggu approval roadmap | `af6a2fe59dd17f58c948591a8cb586f70ea54bd9bcd107fccb79efa3a540ade3` |
| [`roadmap/frontend-roadmap-finishing.md`](./roadmap/frontend-roadmap-finishing.md) | revision `1` — `FE-RWI-192` s.d. `201` | `DRAFT` — menunggu approval roadmap | `d603b5e9e6c8f557721e5036cca21fef7db200b573b85e91f894ce9fd23106a4` |
| [`roadmap/requirement-traceability-finishing.md`](./roadmap/requirement-traceability-finishing.md) | revision `1` — requirement → task → bukti | `DRAFT` | `6a3736d08c81ca5d652701ffd5c55e263e91e803c220b850076a9b087d6abc88` |

SHA-256 di atas adalah **hash saat approval 2 Oktober 2026** (`RWI-DEC-221`) dan menjadi acuan deteksi perubahan berikutnya.

### 11.2 Dependency dan gerbang

| Bergantung pada | Untuk apa | Keadaan | Menahan |
| --- | --- | --- | --- |
| Kamar Operasi — Ikbal Yulianto | Pra-operasi, pemisahan permission serah terima, efek kasus selesai (`RWI-DEC-191`); permintaan admisi dari kamar pulih dan status `Rejected` (`RWI-DEC-208`) | **Disetujui** | Tidak |
| Billing — Yasmina | Sumber `OPERATING_ROOM` dan resolusi tarif komponen (`RWI-DEC-192`, `196`); tautan biaya operasi kunjungan asal ke invoice `RANAP` (`RWI-DEC-207`, dirancang `integrasi-billing` 9.14) | **Diputuskan dan disetujui** | Tidak |
| `keperawatan` `0.6.0` | Daftar kolom `MstTariff` (12.14); menu 7 `FE-KEP-07` dan sub-menu Catatan Pra-Operasi `FE-KEP-24` | Dirancang pada pass yang sama, `draft` | Tidak |
| `integrasi-billing` `1.1.0` | `CorrectsPlacementId` untuk laporan transfer; ketukan pintu `ADMISSION_CONFIRMED` | Dirancang pada pass yang sama, `draft` | Laporan transfer (`P2`) |
| Clinical | Ekstraksi `PatientProcedureExecutionService` dari `PatientProcedureController` (perubahan struktur, perilaku endpoint tetap) | Milik Muhammad Hamzah | Biaya tindakan operasi |
| Pemilik klinis | Isi awal butir persiapan bedah; nilai jenis anestesi rencana | Belum disahkan | Gerbang produksi |

### 11.3 Handoff

| Field | Nilai |
| --- | --- |
| `blueprint_id` / `contract_version` | `RWI-BP-001` / `0.10.0` |
| `approval_status` | **`approved`** — `approved_by` Muhammad Hamzah, `approved_at` 2026-10-02, lewat `RWI-DEC-221` |
| `blocking_questions` | **Nol.** `DEC-INP-018` ditutup `RWI-DEC-207`; `RWI-OQ-114` disetujui `RWI-DEC-208`; `UI-RWF-03` s.d. `05` diputuskan `RWI-DEC-213` s.d. `216`. Tersisa gerbang produksi isi butir persiapan bedah dan jenis anestesi rencana |
| `next_owner` | ~~Approval pemilik atas kontrak `0.10.0` (Muhammad Hamzah)~~ **selesai 2026-10-02 (`RWI-DEC-221`)** → ~~`plan-module-delivery`~~ **roadmap Finishing revision `1` `DRAFT` ditulis 2026-10-02** (`BE-RWI-172` s.d. `184`, `FE-RWI-192` s.d. `201`). `BE-RWI-172` adalah satu-satunya migration bersama (`K8` + `E4`); `E6` (`BE-RWI-181`) sesudah `E5` dan sebelum `I6`. Langkah berikutnya: approval pemilik atas roadmap, lalu `build-module-backend` dan `build-module-frontend` mengikuti tabel gelombang |

---

## 12. Amandemen kontrak `0.11.0` — Workspace PPRI ★ 7 Oktober 2026

**Status amandemen: `approved`** — Muhammad Hamzah, 2026-10-08 (`RWI-DEC-265`); ditulis `design-business-module` 2026-10-07 sebagai `draft`. `RWI-OQ-126` dan `RWI-OQ-127` ditutup `RWI-DEC-266`. Roadmap ditulis `plan-module-delivery` 2026-10-08 berstatus **`DRAFT`**. Revision `8` / kontrak `0.10.0` tetap `approved` (`RWI-DEC-221`) dan tidak disunting; task Finishing tetap berpegang padanya (`RWI-DEC-227`). Amandemen ini ditulis sebagai bagian baru di setiap artefak, sehingga histori approval Finishing tidak tertimpa.

| Field | Nilai |
| --- | --- |
| `contract_version` | `0.10.0` (`approved`) → **`0.11.0`** (`approved` 2026-10-08, `RWI-DEC-265`) |
| `input_revision` | `PRD-RWI-ADMISI-001` v`0.2` (SHA-256 `f1fd336f562d6936629d50f8f849fa7767e2dbea2b24678da9596de60dcc1192`); decision log revision `38` (SHA-256 `7d2de4af684ef8111d248642e501a376e19c6ba6ecb4af6c1030517de0ac5e98`) saat desain, revision `39` (SHA-256 `fff74127072a3d3f625c3c9a425d98fbacddd7033d68b52041a4f41da6f00db2`) saat approval; gate `1.11` bagian 20; capability map `1.7` bagian 20 (SHA-256 `e8e454cbaed3f0bfd39793bbb4b82860ed9eb921b9154c24554b2bf06fb6e3da`) |
| `backend_commit_sha` | Audit `671191eb`; desain pada HEAD **`fdf85a0701a93b7f7c56424de5b3726990b3b485`** — perbedaannya enam berkas harga penjamin yang sudah dinilai (`RWI-FACT-065`) |
| `frontend_commit_sha` | Audit `27889662a`; diperiksa ulang pada HEAD **`dd2cbf7c0f5e1b0dc22b6480aabb84ee8edeb2be`** — tidak menggeser baris yang dikutip (`03-frontend-architecture.md` 14) |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` — gate `1.11` 20.11 |
| Kemampuan | `CAP-RWA-01` s.d. `CAP-RWA-17` tanpa `04`; slice `INP-S38` s.d. `S45`, `S47` siap; `S46` sebagian; `S48` dan `INP-S10` tidak dirancang |
| Keputusan | `RWI-DEC-225` s.d. `266`; fakta `RWI-FACT-060` s.d. `067` |
| Peta modul | [`../02-module-map.md`](../02-module-map.md) revision `5` bagian 8 |

### 12.1 Tabel artefak dan hash

| Artefak | Bagian | Status | SHA-256 |
| --- | --- | --- | --- |
| [`02-backend-architecture.md`](./02-backend-architecture.md) | revision `0.10` — bagian 13 | `approved` | `0a74b298ccb8928406276bd64d3ea6e77044849e0ab442fa51d847fa5b174f17` |
| [`03-frontend-architecture.md`](./03-frontend-architecture.md) | revision `0.10` — bagian 14 | `approved` | `9ce17040cee9b6bd5747bcaaa88007e2555c8a55a8a13ce0584dfd65caa0966b` |
| [`04-prd-to-mvp.md`](./04-prd-to-mvp.md) | bagian 24 | `approved` | `25758e1cb83502a960c5c39ee0417ae3bf621cc95ae9589f7996cbef144af8a2` |
| [`contracts/api-contract.md`](./contracts/api-contract.md) | bagian 12 | `approved` | `17119194de35148afac90d2e85b2029542097a4a93b9b9734c0f7486a00ec276` |
| [`contracts/state-transition-matrix.md`](./contracts/state-transition-matrix.md) | bagian 10 | `approved` | `4336f1f5119a1c55a92dffdf47cd90266a73b45b4027d683f01a4db1bfe7bf5e` |
| [`contracts/validation-matrix.md`](./contracts/validation-matrix.md) | bagian 15 | `approved` | `7c649c324ed3608bef7baea8d0e9440b223f2c37b1984958e160d2925b0f8ac7` |
| [`contracts/integration-contract.md`](./contracts/integration-contract.md) | bagian 10 | `approved` | `e864a12d711f91808401143ab0bed4cd0ff0c8cd10ae9f9e136678bf09da1809` |
| [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md) | bagian 10 | `approved` | `04cc3b28c2d02b099e4dcc277fc5cc26b3224b2316278b75c12a6c6e41afe816` |
| [`data/data-dictionary.md`](./data/data-dictionary.md) | revision `0.7` — bagian 20 | `approved` | `c81b07420e94c96c2efe0090e1f756e587e6786a13ead2333b0deb2e4c8623ad` |
| [`testing/acceptance-test-matrix.md`](./testing/acceptance-test-matrix.md) | bagian 21 | `approved` | `0c34160f1d9f7882786073334b44498016848da8efb77e6f5e685a759f129a78` |
| [`flowcharts/00-alur-utama.md`](./flowcharts/00-alur-utama.md) | bagian 3 baru | `approved` (bagian 3 lewat `RWI-DEC-265`; bagian 1–2 sebelumnya) | `5e6bc9824a4943a4a43b65061c9c06e89be0c55f0169e4d8649c1fe3b2124fc2` |
| [`flowcharts/05-workspace-ppri-siklus-dokumen.md`](./flowcharts/05-workspace-ppri-siklus-dokumen.md) | baru | `approved` | `a7c1835fa3b261f477acee130f202cb4e33bd04eb897d07243ae8748761a87d6` |
| [`flowcharts/06-serah-terima-pasien-baru.md`](./flowcharts/06-serah-terima-pasien-baru.md) | baru | `approved` | `fd806df0a8a5edd5b85bd6acd78830fb7f9c4bf261f3da8077751eacc80e6b28` |
| [`flowcharts/07-gelang-label-dan-ipd.md`](./flowcharts/07-gelang-label-dan-ipd.md) | baru | `approved` | `408180fe399b2ef8cc6ff8f78feb352cc1adb9ba6fc6e990ab86548f683ec1b6` |
| [`flowcharts/08-pelunasan-deposit.md`](./flowcharts/08-pelunasan-deposit.md) | baru | `approved` | `fa089b1a17c679f9587abb19d1c3a82ab3816d4a14f847e6dd11173c0868c230` |
| [`roadmap/backend-roadmap-workspace-ppri.md`](./roadmap/backend-roadmap-workspace-ppri.md) | revision `1` — `BE-RWI-185` s.d. `203` | **`DRAFT`** | `df7110d34d6e332640918d3b2c40a801fbcc1a60f18d83b1c57003f3badfa9d8` |
| [`roadmap/frontend-roadmap-workspace-ppri.md`](./roadmap/frontend-roadmap-workspace-ppri.md) | revision `1` — `FE-RWI-210` s.d. `221` | **`DRAFT`** | `eb33bcec99f06ef9d3df0e06d68394830ef787cee53fee95a5d7330e4d8eaa8d` |
| [`roadmap/requirement-traceability-workspace-ppri.md`](./roadmap/requirement-traceability-workspace-ppri.md) | revision `1` | **`DRAFT`** | `08ec7c60064c6c065925a7e7426c16b63babf57c77f85bc11b2c89437cdf01f3` |

Hash lima belas artefak desain di atas adalah hash **saat approval** 8 Oktober 2026 (`RWI-DEC-265`); hash saat desain ditulis 7 Oktober 2026 diganti karena baris status setiap berkas berubah dari `draft` menjadi `approved`. Isi desain tidak berubah; yang bertambah hanya baris status, catatan approval `02-backend-architecture.md` 13.16, dan baris pertanyaan terbuka yang ditutup `RWI-DEC-266`. Hash `../02-module-map.md` revision `5` saat approval: `19445a0a37673b07a779c98cc820e01ae82a3b1e7f501cce51bd53a862372478`. Hash tiga berkas roadmap adalah hash saat ditulis berstatus `DRAFT` dan dihitung ulang saat roadmap disetujui. Hash bagian 11.1 tidak lagi cocok dengan isi berkas karena bagian baru ditambahkan; isi Finishing yang disetujui tidak berubah satu baris pun, dan yang menjadi acuan isinya adalah bagian 11 s.d. 12 pada masing-masing berkas.

### 12.2 Dependency dan gerbang

| Bergantung pada | Untuk apa | Keadaan | Menahan |
|---|---|---|---|
| Pemilik `PatientManagement` | Service baca identitas, relasi, kontak darurat; ekstraksi pembentuk QR (`RWI-OQ-126`) | **Disetujui `RWI-DEC-266`**, 2026-10-08 | Tidak — `BE-RWI-187` gelombang 1 |
| Pemilik HR Master Data | Service baca profil rumah sakit untuk kop (`RWI-OQ-127`) | **Disetujui `RWI-DEC-266`**, 2026-10-08 | Tidak — `BE-RWI-188` gelombang 1 |
| Pemilik `RegistrationManagement` | Service baca dokter perujuk luar (`RWI-OQ-128`) | Menunggu | Tidak — cadangan garis kosong; `BE-RWI-190` ⛔ |
| Billing — Yasmina | Method baca tarif kamar harian (`RWI-OQ-129`); ringkasan deposit dan harga penjamin dipakai apa adanya | Menunggu untuk method baru | Tidak — cadangan "lihat kasir"; `BE-RWI-191` ⛔ |
| `ClinicalManagement` — Muhammad Hamzah | Perluasan konteks penjamin, method surat pengantar, service alergi | Disetujui lewat `RWI-DEC-264` | Tidak |
| `MasterData` — seluruh tim | Kolom `MstInpatientClearanceItem` dan `MstInpatientSetting` | Gerbang `RWI-DEC-193` | `E9` |
| Admin Akses Role | Peran CRO, supervisor admisi, pemegang `SignAsHeadNurse` (`RWI-OQ-124`) | Menunggu | UAT, bukan pembangunan |
| Yasmina | `DEC-INP-020` tarif visit dan catatan biaya bedah | Terbuka | Hanya `EPIC-RWA-09` (di luar gelombang): `BE-RWI-203` ⛔, `FE-RWI-221` ⛔ |
| Pemilik privasi/hukum | `DEC-INP-003` | Belum ditunjuk (`RWI-OQ-116`) | Hanya `EPIC-RWA-02`, `13`; gerbang produksi G-35 |
| Tim keselamatan pasien; clinical governance | Aturan gelang (G-38); nilai kepercayaan belum di ruang kerja klinis (G-42) | Gerbang produksi | Produksi |

### 12.3 Handoff

| Field | Nilai |
| --- | --- |
| `blueprint_id` / `contract_version` | `RWI-BP-001` / `0.11.0` |
| `approval_status` | **`approved`** — Muhammad Hamzah, 2026-10-08 (`RWI-DEC-265`). Roadmap Workspace PPRI **`DRAFT`**; approval roadmap tindakan terpisah dan bukan wewenang migration, database, maupun deployment |
| `roadmap` | `roadmap/backend-roadmap-workspace-ppri.md` (`BE-RWI-185` s.d. `203`), `roadmap/frontend-roadmap-workspace-ppri.md` (`FE-RWI-210` s.d. `221`), `roadmap/requirement-traceability-workspace-ppri.md` — revision `1`, `DRAFT`, ditulis 2026-10-08 |
| `blocking_questions` | Tidak ada untuk `RWA-MVP-0` s.d. `RWA-MVP-2`; `RWI-OQ-126`, `127` ditutup `RWI-DEC-266`. Di luar gelombang: `DEC-INP-020` (`BE-RWI-203`, `FE-RWI-221`), `DEC-INP-003` (`EPIC-RWA-02`, `13`). Tidak memblokir pembangunan: `RWI-OQ-128` (`BE-RWI-190`) dan `129` (`BE-RWI-191`) dengan cadangan aman; `RWI-OQ-121`, `123`, `125`; `RWI-OQ-124` memblokir UAT saja |
| `next_owner` | Muhammad Hamzah: approval kedua roadmap Workspace PPRI (`DRAFT`); meneruskan `RWI-OQ-128` ke pemilik `RegistrationManagement` dan `RWI-OQ-129` ke Yasmina. Sesudah approval roadmap → `build-module-backend` gelombang 1 (`BE-RWI-185`, `187`, `188`, `189`); frontend dimulai sesudah prasyarat `[BE]`-nya ✅ (`FE-RWI-210`, `211` menunggu `BE-RWI-186`; `FE-RWI-212` menunggu `BE-RWI-195`). ID bebas berikutnya: `BE-RWI-204`, `FE-RWI-222` |

---

## 13. Amandemen Alur Admisi Pendaftaran ★ Revisi Mba Ilma (`RWI-DEC-267` s.d. `RWI-DEC-273`)

Ditulis 10 Oktober 2026 sebagai respon atas evaluasi alur admisi pendaftaran oleh tim analisis bisnis (Mba Ilma). **Status: `approved`** — disetujui pengguna 10 Oktober 2026 atas instruksi eksplisit "setujui dan lakukan /plan-module-delivery".

### 13.1 Perubahan Desain dan Kontrak

| Artefak Terkait | Revision | Status | Cakupan Perubahan |
|---|:---:|:---:|---|
| [`05-skema-tampilan.md`](./05-skema-tampilan.md) | `0.7` | `approved` | Alur PB 12 langkah & PL 10 langkah; rujukan ringkas teks; 3 kategori pasien; kontak darurat max 13 digit; split layout PL; unit tujuan sembunyi; *dedicated step* Form Persetujuan & TTD Digital; pratinjau siap cetak |
| [`03-frontend-architecture.md`](./03-frontend-architecture.md) | `0.11` | `approved` | Penataan 12 langkah PB & 10 langkah PL; 4 titik tulis; invariant hukum penanda tangan bayi; General Consent PPRI *print-ready* tanpa re-input (`RWI-AC-395`) |
| [`flowcharts/00-alur-utama.md`](./flowcharts/00-alur-utama.md) | — | `approved` | Penambahan Bagian 0: Alur Admisi Pendaftaran Terpadu; penyelarasan Bagian 3 PPRI General Consent |
| [`contracts/validation-matrix.md`](./contracts/validation-matrix.md) | `0.11.0` | `approved` | Penambahan `VAL-ADM-01` s.d. `VAL-ADM-05` (kontak darurat 13 digit, kelengkapan rujukan, default ranap, TTD digital, invariant penanda tangan bayi) |
| [`04-prd-to-mvp.md`](./04-prd-to-mvp.md) | `0.11.0` | `approved` | Penambahan Bagian 25: Amandemen Alur Admisi Pendaftaran (`FR-RI-179` s.d. `FR-RI-186` dan `RWI-AC-388` s.d. `RWI-AC-395`) |
| [`roadmap/backend-roadmap-admisi-pendaftaran.md`](./roadmap/backend-roadmap-admisi-pendaftaran.md) | revision `1` | `APPROVED` | 2 task backend: `BE-RWI-204` (kontak darurat 13 digit & default ranap), `BE-RWI-205` (persistence persetujuan & TTD digital, invariant bayi) |
| [`roadmap/frontend-roadmap-admisi-pendaftaran.md`](./roadmap/frontend-roadmap-admisi-pendaftaran.md) | revision `1` | `APPROVED` | 5 task frontend: `FE-RWI-222` s.d. `FE-RWI-226` (masking kontak darurat, alur 12 langkah PB, split layout 10 langkah PL, kanvas TTD digital, print-ready PPRI) |
| [`roadmap/requirement-traceability-admisi-pendaftaran.md`](./roadmap/requirement-traceability-admisi-pendaftaran.md) | revision `1` | `APPROVED` | Matriks ketertelusuran penuh 8 FR, 7 keputusan bisnis, dan 8 kriteria penerimaan tanpa coverage gap |

### 13.2 Dampak Dependency dan Roadmap
- Perubahan ini tidak merusak tabel entitas backend yang sudah ada; penambahan data digital signature dan rujukan ringkas melengkapi state admisi dan dokumen persetujuan.
- Roadmap delivery telah dibentuk ke dalam vertical slice independen berukuran kecil: 2 task backend (`BE-RWI-204`, `BE-RWI-205`) dan 5 task frontend (`FE-RWI-222` s.d. `FE-RWI-226`).
- Tidak ada blocking questions terbuka untuk amandemen ini. Seluruh kriteria penerimaan memiliki penanggung jawab task terarah.

### 13.3 Handoff ke Implementasi
- **Next Owner:**
  - Backend: `build-module-backend` untuk task `BE-RWI-204` (prasyarat kontrak) lalu `BE-RWI-205`.
  - Frontend: `build-module-frontend` untuk task `FE-RWI-222` (dimulai setelah `BE-RWI-204` selesai), berlanjut hingga `FE-RWI-226`.
- **ID Bebas Berikutnya:**
  - Backend: `BE-RWI-206`
  - Frontend: `FE-RWI-227`

## 14. Amandemen Bed Management — 10 Oktober 2026

**Current revision11: draft.** Set kontrak current hanya ditetapkan field `contract_versions` di atas; setiap file merujuk set ini lewat `last_changed_in`. Previous approval revision10/0.11.0 berlaku pada isi sebelumnya, bukan amandemen Bed Management. `approved_by: null`, `approved_at: null`. Tidak ada source aplikasi, migration, database, deployment atau Git mutation di fase ini.

### 14.1 Masukan dan hash

| Input | Revision/readiness | SHA256 |
| --- | --- | --- |
| ../00-interview-decisions.md | 46; DEC-274–294 product closed, AC-396–426 | 41dd035d62e7804dff7e796712e25410152fae8e67b9444257ba73e0a6a30b3d |
| ../evidence/02-requirement-completeness-gate.md | 1.12; BM-RCG-20261010-01; enam bounded capabilities READY_FOR_DOMAIN_DESIGN | 418066b0f721717fcb44392efc2d410b474987e030ea49352368e90a628dab2a |
| Audit historis: artifacts/bed-management/01-existing-capability-map.md | BM-AUD-20261010-01 rev1; arsip tidak tersedia pada sesi11 Oktober2026, hash ini tidak diverifikasi ulang | 50e0e1525804260331d3a830fb379532f802a3ff31e43cd3b4df4416fa42f38b |
| ../02-module-map.md | 6 draft; section9 Bed Management | d503f8016d14a7dda6afe3d3579496d113d93811dcda4bf014ea06a71da34d9f |
| Previous child manifest snapshot | revision10 approved; input history before amendment | b661259ee1812418ab0e78bc03b7c0f6dc19dda05f5bfe18b6c847843d568a8c |

Input links: [decisions](../00-interview-decisions.md), [gate](../evidence/02-requirement-completeness-gate.md), [impact scan current](../evidence/bed-management-impact-scan-20261011.md), [module map](../02-module-map.md). `input_hash` tiap artefak Bed Management merujuk tabel ini beserta14.8; PRD26.1 memuat hash artefak teknis saat diturunkan. Audit asli dan snapshot manifest sebelum desain merupakan provenance historis, bukan berkas yang tersedia untuk validasi ulang. `DOMAIN_ARCHITECTURE_NOT_RUN` khusus BM: gate siap untuk bounded design langsung; arsitektur domain old scope lain tetap berlaku.

### 14.2 Owner, authority dan approval

| Kewenangan | Pemilik/batas | Bukti |
| --- | --- | --- |
| Produk/domain/API | Muhammad Hamzah | DEC-061; produk BM DEC-274–294; desain target draft |
| MasterData | Seluruh tim MasterData; bukan satu pengembang saja | DEC-193/FACT-070; proof class BM-G01 |
| Frontend | Batas menu/tab/status dikonfirmasi produk, cosmetics DEV_DISCRETION | DEC-292; FE15 PROPOSED visual |
| HK/perawat/PPI | Fungsi yang dipilih produk; nama/shift/SOP actual belum terbukti | BM-G02; tidak AI-approve SOP |
| Security/privacy/access | Pemilik privacy OPEN; actual grant/scope belum terbukti | BM-G03; deny default |
| Approved_by / approved_at current | null / null | Approval manusia belum diberikan untuk amandemen ini |

### 14.3 Artefak current dan hash

Hash berikut mendeteksi drift pada **draft current**, disinkronkan11 Oktober2026 memakai SHA256 byte file aktual. Snapshot source menyimpan hash LF tambahan untuk membedakan perubahan akhir baris; file source tidak dinormalisasi. Tabel hash lama pada bagian terdahulu adalah bukti versi terdahulu. Manifest ini tidak menghitung hash dirinya sendiri. Contract set baru tidak mengubah last_changed_in pada extra contract lain yang tidak disentuh.

| Artefak relatif | Status / last_changed_in | SHA256 |
| --- | --- | --- |
| 02-backend-architecture.md | draft / 0.12.0 | 110761fb71f7dbfabea35ae394229805a97230c32c3493585f7c8175a0cb16ee |
| data/data-dictionary.md | draft / 0.12.0 | d660c2b8e260a5d45eced4c900b63416a43e09b39ed6c98ba67372901e3c9bd3 |
| contracts/api-contract.md | draft / 0.12.0 | 07efda0575841b26c3b21207ef0a193949908e081b42367638dd3361c1c404ca |
| contracts/state-transition-matrix.md | draft / 0.12.0 | 78a286e0f8441dd626c5e9fb6687ecdc549d7410d8e7867a1cdc45a12bd274e5 |
| contracts/validation-matrix.md | draft / 0.12.0 | a8a225a84f51b9a1e455285edfc0458c830ef1775d59f767d6e49e1664bf1dce |
| contracts/permission-audit-matrix.md | draft / 0.12.0 | 893c0c2d24e8ed3de0f25b42aec853d5b05e22bc89c0acf06371e6949f680960 |
| contracts/integration-contract.md | draft / 0.12.0 | 894f8df47fed471d167110375d0511d3af3a1ace94262ef47a7f4cb0ecddca31 |
| 03-frontend-architecture.md | draft / 0.12.0 | 41a0bb7c341b0be4c1f140278c760fd5edcebc89b4c822af53c2d57ed5d0b26b |
| testing/acceptance-test-matrix.md | draft / 0.12.0 | 8306cf781b25ce251ee781599306cc762a5c82edf3e3b39e8e849bdd5d6c2ca5 |
| flowcharts/00-alur-utama.md | draft / 0.12.0 | 51f97afcc472607e96f350d785d0b066e4d833dd4bfe2c06127127787ba2ec6d |
| flowcharts/09-bed-reservation-release.md | draft / 0.12.0 | b8a967e51464947f8af5c72a204643503951a9f9f279e4224b0ed9eaa2bcf857 |
| flowcharts/10-bed-transfer.md | draft / 0.12.0 | 97771fe2f1e9bd60bac9237f5036e55b45834c728d69f2a3fe204f01f05604d5 |
| flowcharts/11-bed-cleaning-readiness.md | draft / 0.12.0 | f077535b933af60a1ba816991c3f4e5eb244577634829445261b3f2b8e192ca0 |
| flowcharts/12-bed-closure-reopen.md | draft / 0.12.0 | d2b4e6b7375e23257c4b3b2a3f65a1d1752c2bf6d9f0905cd1e99e98cf9ee27e |
| flowcharts/13-bed-usage-history-correction.md | draft / 0.12.0 | 3a054a8516ebf953982c18add7a9c5cdf22e9ae6d74af1486ea4021e830b6b68 |
| flowcharts/14-bed-uncertain-outcome.md | draft / 0.12.0 | 13d7bf111fdfaf26505cd343d7f64cff6709244ecf05d673a4728a7d31d8a2d4 |
| 04-prd-to-mvp.md | draft / 0.12.0 | 74d6aedf3b2165e111243e064ba32a93331b38fea5a3cf9c9308a618d35e09de |

### 14.4 Scope, compatibility dan supersession

BM-CG01 monitoring,02 reservation/release,03 transfer,04 readiness software workflow,05 closure/reopen,06 usage history. Epic BM-01..06, FR-BM-001..012, UAT-701..712, NFR-701..706. Mapping31AC pada testing22, sourcecap BM-CAP-01..17 pada modulemap9; BM-CAP-06 dan alternatif two-phase/offline/checklist/tariff engine di luar MVP.

DEC-281/282 menyupersede hanya aturan bed used release yang dahulu Available. Raw enum BedStatus0..7 tetap; source target memakai readiness substate untuk enam status BA. Tetap TTL120m server, one-step atomic transfer, existing DPJP/folio/eligibility, correction versions dan Billing canonical1.1.0. Same Grade dipilih manual, kelas official proof tidak ditebak.

Perubahan request mutation (key/versions/category/reason) memerlukan coordinated cutover semua consumers. Parent/child approval old tidak membuat amandemen approved; task/roadmap lama tetap terikat baseline/task kontraknya masing-masing. Tidak membuat roadmap/task baru atau menandai kesiapan runtime.

### 14.5 Activation gates dan readiness

| Gate | Bukti belum tersedia | Dampak spesifik |
| --- | --- | --- |
| BM-G01 | Arah/isi/equivalence urutan kelas actual | Comparison lintas kelas fail-closed sampai proof; same valid class ID tetap dapat Same |
| BM-G02 | SOP cleaning/inspection/downtime dan assignment nyata | Aktivasi aksi HK/verifier/reconcile bergantung SOP; tidak checklist buatan |
| BM-G03 | Permissions/accounts/unit-bed-episode scopes/privacy actual | Grant/identity dibatasi, OperationalOnly PHI-free; unknown deny |
| BM-G04 | Repairs F01–07, API/PG/integrasi/migration/UI/UAT proof | Tidak menyatakan siap runtime; source audit/65 old tests bukan target proof |

Tidak ada product OPEN DECISION tersisa. Gate fakta tidak dibuka ulang sebagai wawancara; slice lain yang tidak bergantung dapat didesain/direncanakan setelah approval desain. Jika proof baru mengubah lifecycle/data/authority, lakukan focused reassessment pada slice terkait. Development lock memerlukan approval manusia; produksi memerlukan gates yang relevan.

### 14.6 Validasi desain dan batas bukti

Catatan sesi10 Oktober menyebut20 bagian PRD,31AC,12UAT,9diagram, pemeriksaan sejarah, dan62fingerprint yang cocok. Catatan itu bersifat historis; daftar62 asli tidak tersedia untuk dijalankan ulang. Sesi11 Oktober memeriksa ulang dokumen current dan47fingerprint source pilihan pada HEAD terbaru, mempertahankan keputusan/gate/capability map root, serta memperbarui inventaris menjadi39endpoint dengan wrapper admisi transfer IGD. Mermaid rendering dan aplikasi/API/PostgreSQL/migration/UAT runtime **NOT_RUN**. Hasil aktual dan batas pemeriksaan ada pada [laporan validasi](./testing/bed-management-design-validation.md).

### 14.7 Supplement bukti source

| Source backend tambahan dibaca | SHA256 snapshot desain |
| --- | --- |
| Areas/HealthServices/MasterData/Controllers/RoomController.cs | ea4e84c98dcf4f1384add868f431ab6ab629ee6b38c69d261621218337fb794a |
| Areas/HealthServices/MasterData/Controllers/ServiceUnitController.cs | 541dd37182d46e9a565642ac7cdaa894325bb4a0f32f469ebab047ee425979b8 |
| Areas/HealthServices/MasterData/Controllers/PatientClassController.cs | 7d5838c242f27957ae818f79fee6a9b819140f16daf80f8c70bc5f655a603b59 |
| Repositories/Configurations/HealthServices/MstBedConfiguration.cs | 5673d6bd77a9a41605b90fec8def0c062e58aed2a348ff247ebd7c6cfc23a0a5 |
| Models/ApplicationUser.cs | 214a431732b187a98f4e618d20defeebcc6938069f38b6d3e413bd5aec90f45b |

Source tambahan digunakan untuk memastikan hierarchy writer actual, path EF config MasterData dan ApplicationUser. Catatan sesi10 Oktober menyebut fingerprints original62 tetap unchanged; daftar aslinya tidak tersedia untuk diperiksa ulang pada sesi11 Oktober. Audit as-is dan decision log tidak ditulis ulang.

### 14.8 Bukti finalisasi11 Oktober2026

Current impact scan memakai BE `c5d3b5bfdc15c014a2399d904d7c4c2207505905`, FE `5aa2c7c70754e6131e4243b5ea3ee5f768b7a5d2`. Working tree awal kedua repository bersih; perubahan sesi ini hanya dokumen blueprint. Klaim62fingerprint pada14.7 adalah catatan sesi lama dan tidak dianggap hasil pemeriksaan ulang. Snapshot baru memuat47source terpilih dan3masukan hulu.

Penyempurnaan draft: wrapper reserve internal `InpAdmissionTransferService` masuk M3/API13.8/backend14.14; nama event Billing diperbaiki ke BED_OCCUPIED/OCCUPANCY_CORRECTED; allowlist ResultKind kamus data diselaraskan dengan master hierarchy. Scope produk/31AC/approval/kontrak target0.12.0 tetap. Tidak menulis atau menjalankan implementasi.

| Artefak relatif | Status | SHA256 byte aktual |
| --- | --- | --- |
| ../evidence/bed-management-impact-scan-20261011.md | draft / BM-IMP-20261011-01 rev1 | 68ccb413da7a3398e944a2505e8d22890298ba5bad2258d223f9dbd351ed7079 |
| ../evidence/bed-management-source-snapshot-20261011.json | snapshot source current; bukan fingerprint62 lama | 7faa06ba0dca2c4aa6dfb47510be966d467629bab87f8cfd181cd513bbd3bc45 |
| testing/bed-management-design-validation.md | laporan validasi current; runtime NOT_RUN | 66e3d6c7a7eda93d477a801e5adea4cdd6a5e2110ed0da0487d1e611d00a7d92 |

Approval manusia belum diberikan untuk amandemen. Sesudah pemilik menyetujui draft dan kontraknya, langkah berikutnya `plan-module-delivery`; BM-G01–04 tetap dependency aktivasi operasi yang relevan. Dokumen upstream dengan link artifacts lama dipertahankan read-only; status arsip yang tidak tersedia dijelaskan pada bukti current, tidak disamarkan sebagai tautan yang lulus.

## 15. Delivery Bed Management — 11 Oktober 2026

Permintaan pengguna menjalankan plan-module-delivery ditangani sebagai **perencanaan DRAFT/FORWARD-TEST** menurut pengecualian skill untuk task BLOCKED. Permintaan planning tidak dicatat sebagai approval amandemen. Revision desain child11/parent10 dan contract set target0.12.0 tetap; approved_by/approved_at amandemen tetap null. Status sebelumnya pada14.8 adalah keadaan saat finalisasi desain; registrasi ini mencatat fase planning berikutnya.

| Field | Nilai |
| --- | --- |
| planning_id / revision / status | BM-DEL-20261011-01 / 1 / DRAFT/FORWARD-TEST |
| Task backend | 14 task BE-RWI-208–221; seluruhnya ⛔ BM-A |
| Task frontend | 7 task FE-RWI-231–237; seluruhnya ⛔ BM-A |
| Gate BM-A | Approval manusia atas amandemen/desain/kontrak0.12.0 serta roadmap/task terkait dan revalidasi hash; owner Muhammad Hamzah/pemilik berwenang |
| Dependency / coverage | Grafik teks29 pasangan;12FR,6NFR,31AC,21decision,12UAT dan39endpoint terpetakan |
| Source SHA BE / FE saat planning | db85bede83c0ddbc9aaa412c576f2c3fd7447492 / 5aa2c7c70754e6131e4243b5ea3ee5f768b7a5d2 |
| Hash/provenance | Snapshot24 input dan manifest sebelum registrasi pada delivery-lock. Diff dari design HEAD c5d3b5b hanya20 dokumen;47fingerprint source diperiksa ulang. Lock draft bukan implementation lock |
| Scope trace | Bed Management decision46 + input SHA256; DEC/AC/label0.12.0 yang dipakai ulang roadmap admisi IGD lama tidak memberi approval atau bukti slice ini |
| Gate penerapan | BM-G01–04 tetap terbuka dan spesifik per slice; bukan approval source/role/database/runtime |
| Laporan task / runtime | Belum ada laporan implementasi; build/API/PostgreSQL/migration/browser/UAT NOT_RUN |
| Next handoff | Sesudah approval/relock dan authority execution: BE-RWI-208/209 terlebih dahulu; FE-RWI-231 setelah BE-RWI-211 terbukti; tidak menjalankan builder pada planning ini |
| ID bebas sesudah alokasi ini | BE-RWI-222; FE-RWI-238. Tetap scan ulang saat alokasi berikutnya |

| Artefak relatif | Status | SHA256 byte aktual |
| --- | --- | --- |
| [roadmap/backend-roadmap-bed-management.md](./roadmap/backend-roadmap-bed-management.md) | DRAFT/FORWARD-TEST rev1 | 017f44850a2e3888f956a298f787b548b60788f7cb44c2209d29cc5113ea95f6 |
| [roadmap/frontend-roadmap-bed-management.md](./roadmap/frontend-roadmap-bed-management.md) | DRAFT/FORWARD-TEST rev1 | 5716d1ef007ecb593615f148332c70b6c3d1aa50d4083cb4488bdcb11ebb6a6e |
| [roadmap/requirement-traceability-bed-management.md](./roadmap/requirement-traceability-bed-management.md) | DRAFT/FORWARD-TEST rev1 | 9445e15f8a784226e198696710265a299e493c69a1251503df97f1cf07a11139 |
| [roadmap/bed-management-delivery-lock.json](./roadmap/bed-management-delivery-lock.json) | DRAFT/FORWARD-TEST rev1 | 1d78636725038c2c32c216035a6c0cb14a6662515499eff12d21952276097260 |

Pemeriksaan aktual dan gap bukti dicatat pada [traceability delivery](./roadmap/requirement-traceability-bed-management.md). Tidak ada approval baru yang diinferensikan, source aplikasi/lockfile yang ditulis, migration/database yang dijalankan, atau Git mutation. Tabel artefak desain14.3/14.8 tidak berubah.

## 16. Delta PLAN-REPAIR-DOK-006 — Kelas perawatan mengikuti kamar ★ 13 Oktober 2026

**Status: `SELESAI`** — task sub-modul ini ditutup 13 Oktober 2026. Plan, issue, dan kontrak milik `dokter-rawat-inap`; register lengkapnya pada [dokter-rawat-inap bagian 11](../dokter-rawat-inap/blueprint-manifest.md). Amandemen desain dan delivery pada bagian 13–15 tidak berubah. Uji UI/runtime belum dilakukan.

| Field | Nilai |
| --- | --- |
| Plan / kontrak | [PLAN-REPAIR-DOK-006](../dokter-rawat-inap/docs/plan-repair/plan-repair-006-kelas-perawatan-dan-cakupan-asuransi.md) / [RWI-KELAS-006-1.0](../dokter-rawat-inap/contracts/kelas-perawatan-repair-006.md), SHA256 `439623315c8cb4922e4794b7efb2218228d9e88fbd5f25e1473decd5087cd112` |
| Task | ✅ BE-RWI-224 (FIX-DOK-006-02), ✅ FE-RWI-240 (FIX-DOK-006-06) |
| Source | Implementasi di-commit `8551291f` / `2d0a6890e`; perbaikan penutupan 2 baris `EncounterInsuranceService.cs` belum di-commit |
| Amandemen hulu sub-modul ini | `03-frontend-architecture.md`, `05-skema-tampilan.md`, `contracts/api-contract.md`, `contracts/integration-contract.md`, `contracts/validation-matrix.md` |

| Artefak relatif | Status | SHA256 byte working copy (CRLF) saat penutupan |
| --- | --- | --- |
| [roadmap/backend-roadmap-kelas-perawatan.md](./roadmap/backend-roadmap-kelas-perawatan.md) | revision `1`; BE-RWI-224 ✅ | `6d13021089bd46bd6ee3fce0dc198f6e74298b72d86adbf6e8eb24c2e9291b6f` |
| [roadmap/frontend-roadmap-kelas-perawatan.md](./roadmap/frontend-roadmap-kelas-perawatan.md) | revision `1`; FE-RWI-240 ✅ | `170c2c3885a6e0c83ff66206efcdf241a986f4e54eec4ff2dade3867443e4caa` |
| [roadmap/requirement-traceability-kelas-perawatan.md](./roadmap/requirement-traceability-kelas-perawatan.md) | FIX-DOK-006-02/06 ✅ | `43ea53bf3b545e9ee5ff0158244ff5d6c9710f6de48a154bff07e763a0a8ca1a` |
| [task/report/backend/BE-RWI-224.md](./task/report/backend/BE-RWI-224.md) | ✅ | `3fd6f241c4f16bc70ce186e0521bad81e9182742d0dd6fcfe4c53142679dc2f9` |
| [task/report/frontend/FE-RWI-240.md](./task/report/frontend/FE-RWI-240.md) | ✅ | `794c220b6496103fb4c08a0ecc3ae94addc4af9e2d91eceaa2024561165871aa` |

ID task RWI terpakai sampai BE-RWI-226 dan FE-RWI-240; tetap pindai ulang ruang ID global sebelum alokasi berikutnya.

## 17. Amandemen Integrasi Denah ↔ Bed Management — 15 Oktober 2026

**Current revision 12: `approved`** — Muhammad Hamzah, 2026-10-16 (`RWI-DEC-324`). Set kontrak current `0.13.0` ditetapkan field `contract_versions` di atas; setiap berkas yang isinya berubah mencatat `last_changed_in: 0.13.0`. Approval revision 10 / `0.11.0` tetap berlaku untuk isi sebelumnya; amandemen Bed Management revision 11 / `0.12.0` tetap draft. `approved_by: Muhammad Hamzah`, `approved_at: 2026-10-16`. Desain ini ditulis `design-business-module` pada 15 Oktober 2026 dan tidak menyentuh source aplikasi, database, roadmap, maupun Git.

### 17.1 Masukan dan hash

| Input | Revision/readiness | SHA256 |
| --- | --- | --- |
| ../00-interview-decisions.md | 50; DEC-298–323 (301 dan 309 superseded; 314 dan 317–320 asumsi draft), AC-436–461 | bd326629280af2451824f85648ec2e05b8a5ae96cb1b42153fa6ea75f30223ce |
| ../evidence/02-requirement-completeness-gate.md | 1.13; DNH-RCG-20261015-01; enam DNH-CG `READY_FOR_DOMAIN_DESIGN` | 54da1ceefb103b691ea87390b841b20fef866d130edb099e86e7c4fa8879f738 |
| ../evidence/denah-bed-impact-scan-20261015.md | BM-IMP-20261015-01 rev 1; 41 kemampuan | 5ec075d8fe708c5c575bcc5b52715450de60aa9282f70d548f211f4f2b9f0669 |
| ../evidence/denah-bed-source-snapshot-20261015.json | Snapshot BE `ca09cae9` / FE `4d9bf608` | 05d997365ca75ad2d3c53689d911422871eba0ae5aaf8aab97c8af2db8ab58f1 |
| docs/denah-map-ruangan-rs-mmc/rencana-integrasi-denah-bed.md | Rencana integrasi, pass TUNTAS; diselaraskan dengan desain ini (kode kamar sistem, tabel API, status Fase 0) | 89be57c2b58b95b4133c5a6972d6e35e7dfc4060b4fd5257c92aa990e8245e15 |
| docs/denah-map-ruangan-rs-mmc/rencana-kelas-kamar.md | Kelas setiap kamar denah | 2b8f86ce0525b532eb86748544ecf78b498350dea5b8e09594cb56da49832609 |
| ../02-module-map.md | 7 draft; bagian 10 | 14f28a4529bc1183215bad0c598c41f0250c140a3791aadd50ed7259fc780601 |
| Manifest anak sebelum amandemen ini | revision 11 draft | b8fc1d0d9f9bbbb53a5b48090169f09da4fc1d82afdf7e1ed11c31bb68371dba |

`DOMAIN_ARCHITECTURE_NOT_RUN` untuk slice ini: pemilik setiap data sudah terbukti di impact scan dan gate mengizinkan desain langsung.

### 17.2 Owner, authority, dan approval

| Kewenangan | Pemilik/batas | Bukti |
| --- | --- | --- |
| Produk/domain/API | Muhammad Hamzah | RWI-DEC-061; keputusan produk DEC-298–323 |
| MasterData | Seluruh tim MasterData | RWI-DEC-193; kelas resmi `DNH-G01` |
| Frontend | Desain denah tetap; hal yang didelegasikan tercatat `DEV_DISCRETION` di frontend 16.1 | RWI-DEC-299 |
| Billing | Yasmina, untuk tarif kelas/unit baru | RWI-OQ-143; `DNH-G02` |
| Klinis dan privasi | Kriteria HD/LD milik SOP keperawatan; pemilik privasi OPEN | RWI-OQ-141; RWI-DEC-006; `DNH-G04` |
| approved_by / approved_at current | Muhammad Hamzah / 2026-10-16 | `RWI-DEC-324`: "oke lanjutkan" bersama perintah `plan-module-delivery`, sesudah agent menyatakan approval diperlukan sebelum langkah itu |

### 17.3 Artefak current dan hash

SHA-256 byte berkas apa adanya (CRLF) sesudah pencatatan persetujuan `RWI-DEC-324` dan aturan keras `RWI-DEC-325` (16 Oktober 2026). Berkas yang tidak berubah sejak 14.3 (flowchart 09–14) tetap memakai hash di 14.3.

| Artefak relatif | Status / last_changed_in | SHA256 |
| --- | --- | --- |
| 02-backend-architecture.md | approved / 0.13.0 | e15de1bb2cb1bb4375f8d5719e5d487cb5d1169da24bf3c67fe9f759b8d3f6ae |
| 03-frontend-architecture.md | approved / 0.13.0 | 590f6b2871462da36da123d8509c8f125c89014fdaf3fde6422d012def90ec57 |
| 04-prd-to-mvp.md | approved / 0.13.0 | e7c41612db6248bf19fab384fdd780b93b8f28fd2d779c3ae36dde9f106d78d0 |
| data/data-dictionary.md | approved / 0.13.0 | eed76fe1e3252bf8e7eea1437dfdbeaa3b552ea7cec2c90220390713bd83b6b7 |
| contracts/api-contract.md | approved / 0.13.0 | d94a0ea46ddfb0e18392d547480f9d0f19ccb0c4527d588faaa5b85e7039f075 |
| contracts/state-transition-matrix.md | approved / 0.13.0 | f8b3f20d3c28bd38135e8b405d78b3c432a69dffda8d01fcfa757cb53ef690b2 |
| contracts/validation-matrix.md | approved / 0.13.0 | 637d3d54781dbeafb6312e2a8a6a30cd49f41ca34d8a646e59ad5c5b84f77c20 |
| contracts/permission-audit-matrix.md | approved / 0.13.0 | 3ddc6c84ef3aa3da39cdd277e302b77219c86444810556c3a1ed4a665f3fcf7d |
| contracts/integration-contract.md | approved / 0.13.0 | a317131a351ee74306a59d8f6e94ab9c36cd333008ef116cf4b9d1d981fe6acb |
| testing/acceptance-test-matrix.md | approved / 0.13.0 | 7c0cd9f50e7355955658b32e878ef8aac7b79853e5d49b5ff07751e047494d9a |
| flowcharts/00-alur-utama.md | approved / 0.13.0 | 46f12d164ca387b1890c495a7b4a035cae5906f41b0c54af2489bad1fdba79be |
| flowcharts/15-denah-pilih-bed.md | approved / 0.13.0; berkas baru | 806f143b8cc7977641f0aa05623d1083a1bc258974a2f58d1a97cede60817693 |
| flowcharts/16-denah-master-bed.md | approved / 0.13.0; berkas baru | a2bfeeed93b7cb8c60024131de25032dd8d65df85822d0056cfb4bb6462647ce |
| flowcharts/17-impor-denah.md | approved / 0.13.0; berkas baru | 6bd9114be0c548a860c0ac1e0588117914d672924fd94c376bb7656520703a68 |
| flowcharts/18-ketergantungan-pasien.md | approved / 0.13.0; berkas baru | cfb57c3d0b9ee01ecc97e0bc222aca1ac5323e69726d2335abeda07aa0af6285 |

### 17.4 Scope, compatibility, dan supersession

Kemampuan: `DNH-CG-01` tampilan denah dan status kamar, `02` struktur lokasi dan master, `03` impor, `04` kelas resmi, `05` HD/LD, `06` kelayakan tanpa jenis kelamin. Epic DNH-01..06, FR-DNH-001..016 (FR-DNH-004 OPEN DECISION karena RWI-OQ-144), NFR-801..807, UAT-801..812, DNH-AT-436..461 dan DNH-AT-901..913. Disposisi seluruh 41 `DNH-CAP` ada di module map 10.4.

| Perubahan | Dampak |
| --- | --- |
| Kolom baru `MstRoom` (3), `InpEpisode` (5); tabel baru `InpCareDependencyHistory`, `MstFloorPlanImportRun` | Aditif; M-DNH-1..3 |
| Kolom jenis kelamin `MstRoom`/`MstBed`, field DTO, aturan 4 dan 5 | **Breaking**; dihapus satu gelombang backend–frontend (M-DNH-4) |
| `ComparisonResult` bernilai `Special` | Nilai baru pada field existing |
| Kode kamar kamar baru dari pengalokasi bersama (`DNH-DES-01`) | Perbaikan utang teknis; format kode tetap |
| Tampilan kartu dan tabel bed | Dihapus; endpoint `bed-board` dipertahankan |

Supersession: RWI-DEC-315 menggantikan seluruh aturan jenis kelamin bed (RWI-DEC-301, sisa RWI-RULE-012 bagian B); RWI-DEC-299/320 menggantikan `DEV_DISCRETION` tata letak Monitoring dari RWI-DEC-292; RWI-DEC-323 menggantikan RWI-DEC-309; RWI-DEC-306 mengamandemen RWI-DEC-048 untuk importer eksplisit. Enam status bed, TTL 120 menit, transfer satu langkah, dan siklus kesiapan bagian 14 tidak berubah.

### 17.5 Gate penerapan dan pertanyaan terbuka

| Gate | Bukti belum tersedia | Dampak spesifik |
| --- | --- | --- |
| `DNH-G01` | Pemetaan kelas lama ke kelas resmi oleh MasterData dan konfigurasi urutan | D-DNH-1; tanpa itu transfer antarkelas biasa ditolak |
| `DNH-G02` | Tarif kelas dan unit yang benar-benar baru (Billing) | Perkiraan biaya menampilkan "Tarif belum tersedia" |
| `DNH-G03` | Fakta RS: susunan disahkan, nama unit, nurse station, nomor 321 ganda, kamar 520–522 | Impor produksi |
| `DNH-G04` | Kriteria klinis HD/LD dan persetujuan privasi | Penayangan HD/LD di produksi |
| `DNH-G05` | Penugasan akses per unit dan pengesahan bed produksi | Pembukaan bangsal produksi |
| `BM-G01`–`G04` | Tetap seperti 14.5 | Tetap seperti 14.5 |

Pertanyaan terbuka: RWI-OQ-144 (memblokir FR-DNH-004 saja), RWI-OQ-141, 142, 143 (tidak memblokir desain), `DNH-UNK-01`/`02` (isi DB Hamzah; butuh izin baca terpisah). Asumsi draft RWI-DEC-314 dan 317–320 serta pilihan desain backend 15 menunggu konfirmasi pemilik bersama approval desain.

### 17.6 Validasi desain dan batas bukti

| Pemeriksaan | Hasil | Bukti / batas |
| --- | --- | --- |
| Lebar kolom tabel | PASS | Seluruh tabel bagian baru dan berkas yang disunting konsisten; jumlah masalah sama dengan sebelum amandemen (0) |
| `git diff --check` | PASS | Folder `docs/module-blueprints/rawat-inap/` |
| Rujukan keputusan | PASS | 66 ID `RWI-DEC/AC/OQ/FACT/RULE` ditemukan di decision log revision 50; 50 ID `DNH-CAP/MIS/UNK` ditemukan di `BM-IMP-20261015-01` |
| ID internal | PASS | VAL-DNH 23, GUARD-DNH 7, INV-DNH 10, DNH-AT 39, FR-DNH 16, NFR 7: semua yang dirujuk terdefinisi |
| UAT dan endpoint | PASS | UAT-801..812 identik di PRD dan matriks uji; endpoint PRD 27.13 ada di API contract 14.2 |
| Tautan lokal | PASS | Tidak ada tautan rusak pada bagian baru dan flowchart 15–18 |
| Newline | PASS | Berkas CRLF tetap CRLF; flowchart baru CRLF seperti flowchart lain |
| Kesegaran source | PASS | HEAD BE `ca09cae9` dan FE `4d9bf608` sama dengan snapshot `BM-IMP-20261015-01`; perubahan working tree paralel (issue 022, admisi) tidak dinilai |
| Source/migration/test aplikasi | NOT RUN | Fase dokumentasi; pembuktian pada delivery |

### 17.7 Langkah berikutnya

Approval diberikan 16 Oktober 2026 (`RWI-DEC-324`), bersama aturan keras frontend `RWI-DEC-325`; roadmap delivery terdaftar pada bagian 18. Ruang ID task `BE-RWI`/`FE-RWI` dipakai bersama dan harus dipindai ulang saat alokasi. Planning Bed Management `BM-DEL-20261011-01` (bagian 15) tidak diubah oleh amandemen ini.

## 18. Delivery Integrasi Denah ↔ Bed Management — 16 Oktober 2026

`plan-module-delivery` dijalankan sesudah persetujuan desain `RWI-DEC-324`. Roadmap ini mengikat isi Integrasi Denah kontrak `0.13.0`; roadmap Bed Management `BM-DEL-20261011-01` (bagian 15) tidak diubah.

| Field | Nilai |
| --- | --- |
| planning_id / revision / status | `DNH-DEL-20261016-01` / 1 / APPROVED_DESIGN_PLANNED |
| Task backend | 9 task `BE-RWI-232` s.d. `240`; gelombang 1–4; empat task membuat migration M-DNH-1 s.d. 4 |
| Task frontend | 8 task `FE-RWI-246` s.d. `253`; gelombang 1–5; **aturan keras `RWI-DEC-325`** tercantum pada setiap kartu |
| Approval | Desain: `RWI-DEC-324`. Eksekusi tiap task lewat `build-module-backend`/`build-module-frontend` dengan task ID-nya |
| Coverage | 16 FR (FR-DNH-004 OPEN DECISION tanpa task; FR-DNH-015 lewat langkah operasional D-DNH-1), 7 NFR, 26 AC (2 superseded), 13 skenario aturan desain, 12 UAT, 20 endpoint terdampak |
| Source SHA BE / FE | `ca09cae909597442385ce4513be9c803549be10e` / `4d9bf608dbbaddc96a2b4acfc8723753b5dd5028` |
| Hash / provenance | 22 masukan + 2 manifest sebelum registrasi pada lock |
| Langkah operasional | D-DNH-1 kelas resmi, D-DNH-2 pembersihan DB Hamzah, D-DNH-3 impor dev, D-DNH-4 produksi — bukan task, wewenang terpisah |
| Laporan task / runtime | Belum ada; build, API, PostgreSQL, peramban, dan UAT `NOT RUN` |
| Next handoff | Gelombang 1: `BE-RWI-232`, `BE-RWI-233`, `BE-RWI-234`, `FE-RWI-246` |
| ID bebas sesudah alokasi | `BE-RWI-241`; `FE-RWI-254`. Tetap pindai ulang sebelum alokasi berikutnya |

| Artefak relatif | Status | SHA256 byte aktual |
| --- | --- | --- |
| [roadmap/backend-roadmap-integrasi-denah.md](./roadmap/backend-roadmap-integrasi-denah.md) | rev 1; 9 task, grafik 9 pasangan | d52849af63689957de5be5e194ca978311061d595831d9495953dfd18ab7ef9d |
| [roadmap/frontend-roadmap-integrasi-denah.md](./roadmap/frontend-roadmap-integrasi-denah.md) | rev 1; 8 task, grafik 13 pasangan, aturan keras RWI-DEC-325 | c3c00f0776e1d58f51696c357d7de21e41127a2804bed54eba834d552ac58723 |
| [roadmap/requirement-traceability-integrasi-denah.md](./roadmap/requirement-traceability-integrasi-denah.md) | rev 1; FR/AC/NFR/UAT/endpoint ke task | 292d17dc2f28dc40871645619ebe80f292b54cbd192a199cc3dcc440611a5b6d |
| [roadmap/integrasi-denah-delivery-lock.json](./roadmap/integrasi-denah-delivery-lock.json) | rev 1; 22 masukan + 2 manifest sebelum registrasi | db4f6bd6f700e2edf8686e24349fc55f02c37f33df160cc67a4e0427754e1de8 |

---

## 19. Amandemen Pendaftaran Admisi Pasien ODC (One Day Care) — 16 Oktober 2026

**Current revision 14: `draft`** — Menunggu approval Muhammad Hamzah. Set kontrak target `0.15.0` ditetapkan pada metadata sub-modul; setiap berkas yang isinya berubah mencatat perubahan target `0.15.0`. Baseline revision 13 (Transfer Pasien Masuk & PPJA) dan revision 12 (Integrasi Denah ↔ Bed Management, `RWI-DEC-324`) berstatus `approved`. Desain ini disusun oleh `design-business-module` pada 16 Oktober 2026 menyerap hasil wawancara pass tuntas `RWI-DEC-327` s.d. `RWI-DEC-332` (Revision 53).

### 19.1 Masukan dan Hash

| Input | Status / Keterangan | SHA-256 |
|---|---|---|
| `../00-interview-decisions.md` | Revision 53; Wawancara Pass Tuntas ODC (`RWI-DEC-327` s.d. `332`, `RWI-AC-463` s.d. `468`) | bd326629280af2451824f85648ec2e05b8a5ae96cb1b42153fa6ea75f30223ce |
| Snapshot Git Backend | Branch `MHamzah` @ `def6cf6ab5f945cb7bf228847d90fcb10a2d2d94` | — |
| Snapshot Git Frontend | Branch `HamzahV2` @ `fc9aea6d62418460493899cb669832f032e48328` | — |
| Manifest Sub-modul sebelum amandemen | Revision 13 (`approved`) | — |

### 19.2 Owner, Otoritas, dan Approval

| Kewenangan | Pemilik / Batas | Keterangan |
|---|---|---|
| Produk / Domain / API | Muhammad Hamzah | Pemilik modul Rawat Inap (`RWI-DEC-061`), pengambil keputusan ODC (`RWI-DEC-327` s.d. `332`) |
| Clinical Governance | Muhammad Hamzah / Komite Medis | DPJP rawat inap wajib ditetapkan saat admisi ODC (`RWI-DEC-328`) |
| Approval Desain & Delivery | Muhammad Hamzah | **Disetujui 17 Oktober 2026** lewat pemanggilan `/plan-module-delivery` |

### 19.3 Artefak yang Diamendemen dan Hash Byte Aktual

| Artefak Relatif | Status / Kontrak | SHA-256 |
|---|---|---|
| `02-backend-architecture.md` | `approved` / `0.15.0` (Bagian 18) | `bc86e36ec6d98a9ab2d5f751d0d4f42d718a874f118abf323f7f42b73fdc68dc` |
| `03-frontend-architecture.md` | `approved` / `0.15.0` (Bagian 19) | `00479ae31b78a30801d7838b30726c5814b1e2d979e6b980e7da2f539ff639a6` |
| `04-prd-to-mvp.md` | `approved` / `0.15.0` (Bagian 29) | `ef1bc144557cfab4d85c7535bd7a2b939a2b0001d14368d0fad346d9da4feee4` |
| `data/data-dictionary.md` | `approved` / `0.15.0` (Bagian 25) | `11f6ea236f84169761da2eff2e385849e383c5db5516848d05caf4e0fc3c02a1` |
| `contracts/api-contract.md` | `approved` / `0.15.0` (Bagian 16) | `af460ad374aafe767cbf54a682e0a839b6b75c3cc5933757d4392bf37f63d75c` |
| `contracts/state-transition-matrix.md` | `approved` / `0.15.0` | `80e1a4eb06b3a9f50479362a188ae3893bd3ce79f00095afa676bb412a1b98dd` |
| `contracts/validation-matrix.md` | `approved` / `0.15.0` | `931ffc27e0dac6f2c204dbf7fe3c46c3d2f2ce40b4d48d9189a3c491c7f2ecab` |
| `flowcharts/20-alur-admisi-odc.md` | `approved` / `0.15.0` (berkas baru) | `978e5ccd4d54aeb09fc7072632117ddb4857ca6d39e6ee7383393716dfd5cd2d` |

### 19.4 Ringkasan Amandemen Desain Teknis ODC

1. **Alur Admisi ODC (Langkah 1 s.d. 8 Wizard):**
   - Langkah 2 menyajikan kartu pilihan tipe pasien ke-4: `One Day Care (ODC)` di samping `Umum/Pribadi`, `BPJS Kesehatan`, dan `Asuransi/Perusahaan`.
   - Pembentukan episode rawat inap (< 24 jam) dengan status awal `Admitted`, wajib menugaskan DPJP Utama, serta mencatat klausa edukasi batasan 24 jam pada formulir General Consent.
   - Langkah 6 menyaring bed dan kamar secara otomatis (`isOdcBed=true`), melarang keras penempatan pasien ODC di bed reguler saat admisi (`ODC_BED_REQUIRED`).
2. **Validasi Backend & Proteksi Bed ODC:**
   - Invariant backend `EvaluatePlacementEligibilityCore` menegakkan: pasien ODC wajib menempati bed ODC (`ODC_BED_REQUIRED`), dan pasien non-ODC dilarang menempati bed ODC (`ODC_BED_RESERVED`).
   - Pengecualian Transfer: Mutasi transfer pasien ODC yang dirawat > 24 jam dikecualikan dari invariant ODC (`InpPlacementContext.Transfer`), sehingga pasien dapat langsung dipindahkan ke bed rawat inap reguler tanpa merombak episode atau membatalkan admisi (`RWI-DEC-331`).
3. **Database & API Extension:**
   - Database: **0 migrasi EF Core baru** (menggunakan kolom existing `MstBed.IsOdcBed` dan `MstRoom.IsOdcRoom`).
   - API: Endpoint `GET /bed-occupancies/available-beds` diperluas dengan query parameter `isOdcBed` / `isOdcRoom` dan field respon `isOdcBed`.

### 19.5 Delivery Pendaftaran Admisi Pasien ODC — 17 Oktober 2026

`plan-module-delivery` dijalankan pada 17 Oktober 2026. Roadmap ini mengikat isi Pendaftaran Admisi Pasien ODC kontrak `0.15.0`.

| Field | Nilai |
| --- | --- |
| planning_id / revision / status | `ODC-DEL-20261017-01` / 1 / `APPROVED_DESIGN_PLANNED` |
| Task backend | 2 task `BE-RWI-248` s.d. `249`; gelombang 1 dan 2 |
| Task frontend | 3 task `FE-RWI-260` s.d. `262`; gelombang 1 dan 2 |
| Approval | Muhammad Hamzah (17 Oktober 2026 lewat `/plan-module-delivery`). Eksekusi tiap task lewat `build-module-backend` / `build-module-frontend` dengan task ID-nya |
| Coverage | 7 FR (`FR-ODC-001` s.d. `007`), 6 AC (`RWI-AC-463` s.d. `468`), 5 Validasi (`VAL-ADM-10` s.d. `14`), 3 Skenario UAT (`UAT-ODC-01` s.d. `03`) — **Nol Celah (Zero Coverage Gap)** |
| Source SHA BE / FE | `def6cf6ab5f945cb7bf228847d90fcb10a2d2d94` / `fc9aea6d62418460493899cb669832f032e48328` |
| Laporan task / runtime | Belum ada; task berstatus `NOT_STARTED` menunggu pemanggilan builder |
| ID bebas sesudah alokasi | **`BE-RWI-250`**; **`FE-RWI-263`**. Tetap pindai ulang sebelum alokasi berikutnya |

| Artefak Relatif | Status | SHA-256 Byte Aktual |
| --- | --- | --- |
| [roadmap/backend-roadmap-admisi-odc.md](./roadmap/backend-roadmap-admisi-odc.md) | rev 1; 2 task, grafik 1 pasangan | `d0c9cf01acd6f1320d1c878cf7bd998804c4f41546bbb0c76d7b2c246c5d9fad` |
| [roadmap/frontend-roadmap-admisi-odc.md](./roadmap/frontend-roadmap-admisi-odc.md) | rev 1; 3 task, grafik 3 pasangan | `a691877d50fe0ce207bfa0d932c05f0045135b8524290327949a19d7e43f6128` |
| [roadmap/requirement-traceability-admisi-odc.md](./roadmap/requirement-traceability-admisi-odc.md) | rev 1; FR/AC/VAL/UAT ke task | `c2ffb791a5186fa59c0647a5bfa94e63828c241037d33781f658460598b384ff` |
| [roadmap/admisi-odc-delivery-lock.json](./roadmap/admisi-odc-delivery-lock.json) | rev 1; delivery lock snapshot | `0a817fbd1422669d9061fbade896ee79ff2b7c37a86b36243863f296a57be88e` |

## 20. Manifest revision 15 — realtime pemilihan bed, 17 Oktober 2026

| Field | Nilai |
|---|---|
| Blueprint ID | `RWI-BP-001/episode-rawat-inap` |
| Revision | `15` |
| Contract set | `0.16.0` (`draft`) |
| Status | **draft — menunggu approval pemilik blueprint** |
| Scope | SignalR same-unit alert setelah reservation/placement/target transfer committed |
| Requirement | `RWI-DEC-333`–`336`, `RWI-AC-469`–`480` |
| Capability | `RTB-CAP-01`–`08`; `RTB-IMP-20261017-01` |
| Readiness | `RTB-RCG-20261017-01`: `READY_FOR_DOMAIN_DESIGN` |
| Domain architecture | `DOMAIN_ARCHITECTURE_NOT_RUN` |
| Flow | `FLOW-RI-MVP-021` |
| Epic/FR/UAT | `EPIC RI-42`–`44`; `FR-RI-202`–`211`; `UAT-901`–`916` |
| Production gate | `RWI-OQ-151` security/privacy sign-off nama lengkap |

### 20.1 Artifact integrity register

Hash berikut adalah SHA-256 byte aktual setelah amandemen contract set `0.16.0` berstatus `draft`.

| Artefak relatif | Isi amandemen | SHA-256 |
|---|---|---|
| [02-backend-architecture.md](./02-backend-architecture.md) | Post-commit publisher, hub group auth, failure policy | `ab9e97948c984a20932424c445d3ebacf8476ed413164eaa7caea88dece51829` |
| [03-frontend-architecture.md](./03-frontend-architecture.md) | Connection lifecycle, alert, refetch, reconnect | `63942cc9cbffa28daf84cff669b8da5eb282a8914ad1e3ce75f9ab7f1d83447c` |
| [04-prd-to-mvp.md](./04-prd-to-mvp.md) | PRD bagian 30, epic/FR/DoD/delivery boundary | `a5dcad65b36c5bd22d7f7f07c4ccb657cce6c5d79d695e71862e4b912b743acf` |
| [data/data-dictionary.md](./data/data-dictionary.md) | DTO transient dan klasifikasi data sensitif | `d9f1671b577f65ab2bea087b74282cc49591167f9f8d66919772b76bab30302f` |
| [contracts/api-contract.md](./contracts/api-contract.md) | Hub methods, event, payload, HTTP trigger unchanged | `3795aeb9189a335b4b086ed9c2c06606ee027e7366585bbda3fce9604e745daa` |
| [contracts/state-transition-matrix.md](./contracts/state-transition-matrix.md) | Event mengikuti committed state, tanpa state baru | `7d48c76d4d1eada9d493eee02cb891004756f88e6eb4695479fe83f1e22398b6` |
| [contracts/validation-matrix.md](./contracts/validation-matrix.md) | `VAL-RTB-01`–`12` | `0eb08840030851ff12d426af52ebcec8357a5eb9745cc78d3df1060720d034ec` |
| [contracts/integration-contract.md](./contracts/integration-contract.md) | Internal SignalR sequence/failure/reconciliation | `dd8fdc8722c9ec554aa9c407df5dcf34cd60fad638e9f5f3225976ed17cabc51` |
| [contracts/permission-audit-matrix.md](./contracts/permission-audit-matrix.md) | Unit scope, PHI, audit/log redaction | `1625c48a3f377c0d74c36e13320afaf1d2ef6784bdd4489d6471bef07ee4913e` |
| [testing/acceptance-test-matrix.md](./testing/acceptance-test-matrix.md) | `UAT-901`–`916` | `c4474cb8707ddec14ec780368f8f86faa4700ef300d50cedddce34c11d18de86` |
| [flowcharts/00-alur-utama.md](./flowcharts/00-alur-utama.md) | Index flow realtime | `1dfd7d7bbf3206d191225b30f73740aa41f3f1c2081c28af8f1809447e9fe073` |
| [flowcharts/21-notifikasi-realtime-pemilihan-bed.md](./flowcharts/21-notifikasi-realtime-pemilihan-bed.md) | Happy/failure/reconnect flow | `b862d70a539503f79b24b744889ec9f2a9a71a6d0d9d0792f209c75dec1f6a67` |

### 20.2 Approval dan batas delivery

- Pada snapshot revision 15 ini contract set belum approved; status historis tersebut kemudian
  digantikan approval `RWI-DEC-337` dan registrasi delivery pada bagian 21.
- Revision 15 sendiri tidak membuat roadmap/task; roadmap kanonis berada pada revision 16.
- Aktivasi produksi tetap diblokir sampai `RWI-OQ-151` ditutup oleh owner security/privacy.
- Tidak ada source aplikasi, database, migration, atau Git operation yang menjadi bagian revision ini.

## 21. Delivery Notifikasi Realtime Konflik Pemilihan Bed — 17 Oktober 2026

`plan-module-delivery` dijalankan sesudah approval `RWI-DEC-337`. Revision 16 mengikat contract set
`0.16.0` yang dirancang pada revision 15; hash revision 15 tetap menjadi snapshot draft historis,
sedangkan register berikut adalah snapshot approved/planned yang kanonis.

| Field | Nilai |
|---|---|
| planning_id / revision / status | `RTB-DEL-20261017-01` / 1 / `APPROVED_DESIGN_PLANNED` |
| Approval | Muhammad Hamzah, 17 Oktober 2026, `RWI-DEC-337` melalui permintaan melanjutkan `plan-module-delivery` |
| Task backend | `BE-RWI-250` fondasi subscription/publisher; `BE-RWI-251` event post-commit |
| Task frontend | `FE-RWI-265` client; `FE-RWI-266` hook; `FE-RWI-267` integrasi alert/refetch |
| Dependency awal | `BE-RWI-250` dan `FE-RWI-265` boleh dimulai paralel |
| Coverage | 10 FR, 12 AC, 12 VAL, 16 UAT — **Zero Coverage Gap** |
| Source snapshot | BE `d420c2fbe094a2d70776c44955233bce9c87bda8`; FE `0accf387138a3fcc1ff40fbb5bdeb960076b4d59` |
| Production gate | `RWI-OQ-151` tetap OPEN; memblokir aktivasi nama lengkap di produksi, bukan implementation nonproduction |
| Laporan/runtime | Belum ada; seluruh 5 task `NOT_STARTED` |
| ID berikutnya | `BE-RWI-252`; `FE-RWI-268`; wajib scan ulang sebelum alokasi baru |

| Artefak relatif | Status | SHA-256 byte aktual |
|---|---|---|
| [roadmap/backend-roadmap-realtime-bed-selection.md](./roadmap/backend-roadmap-realtime-bed-selection.md) | rev 1; 2 task; 1 pasangan dependency | `ea40156e576dd646ad72addda9433b91869f7f6e1162d9fc4bb66dccfd05a489` |
| [roadmap/frontend-roadmap-realtime-bed-selection.md](./roadmap/frontend-roadmap-realtime-bed-selection.md) | rev 1; 3 task; 3 pasangan dependency | `d323f688b405bc16dc8eaa40373feb1cc4296389c365e80f0efd20e5913a6b3c` |
| [roadmap/requirement-traceability-realtime-bed-selection.md](./roadmap/requirement-traceability-realtime-bed-selection.md) | FR/AC/VAL/UAT ke task; zero gap | `c6024b92d8d8aecbf2ff2f3c55621f84db2f5997400c939b240c10e063a7a70f` |
| [roadmap/realtime-bed-selection-delivery-lock.json](./roadmap/realtime-bed-selection-delivery-lock.json) | Contract/input hash lock dan alokasi ID | `ac3067095997190cd6c05be39c6e8e314b51a16890e990446715df5953f73716` |

Eksekusi tetap tepat satu task per pemanggilan builder. Roadmap ini tidak memberi izin perubahan
database/deployment dan tidak mengubah source aplikasi.

---

## 22. Manifest revision 17 — Prioritas Kamar Pasien Member di Antrean Transfer & Lencana Tier, 19 Oktober 2026

`plan-module-delivery` dijalankan pada 19 Oktober 2026. Roadmap ini mengikat isi Prioritas Kamar Pasien Member di Antrean Transfer & Lencana Tier kontrak `0.17.0` untuk persiapan demo operasional Selasa, 20 Oktober 2026 (`IGD-REQ-004`).

| Field | Nilai |
|---|---|
| Blueprint ID | `RWI-BP-001/episode-rawat-inap` |
| Revision | **`17`** |
| planning_id / revision / status | `TRF-DEL-20261019-01` / 1 / `APPROVED_DESIGN_PLANNED` |
| Contract set | **`0.17.0`** (`approved_design_planned`) |
| Approval | Muhammad Hamzah (19 Oktober 2026 via `/plan-module-delivery`). Eksekusi per task via `build-module-backend` / `build-module-frontend` |
| Task Backend | 2 task: `BE-RWI-252` (Gelombang 1), `BE-RWI-253` (Gelombang 2) |
| Task Frontend | 2 task: `FE-RWI-268` (Gelombang 1), `FE-RWI-269` (Gelombang 2) |
| Scope Fungsional | Pengurutan cerdas antrean transfer rawat inap (CITO → Member Prioritas → Reguler) dan visualisasi lencana tier/CITO pada tabel antrean dan wizard admisi transfer |
| Keputusan Bisnis | `RWI-DEC-338` s.d. `RWI-DEC-341` (4 Keputusan) |
| Functional Requirements | `FR-TRF-008` s.d. `FR-TRF-012` (5 FR) |
| Kriteria Penerimaan | `RWI-AC-481` s.d. `RWI-AC-493` (13 AC) |
| Kasus Uji & UAT | `TRF-AT-01` s.d. `TRF-AT-16` (16 Butir Uji), `UAT-TRF-01` s.d. `UAT-TRF-07` (7 Skenario) — **Zero Coverage Gap** |
| Permintaan Terkait | `IGD-REQ-004` (Disetujui penuh), `IGD-REQ-003` butir 5 (Disetujui via `IGD-REQ-004`) |
| Source Snapshot | BE `MHamzah@7a4d443fcdf0c9d33cc1a9f4be81522b4fadaefb`; FE `HamzahV2@5ad45a6944b3c320038cc48bb288471cd8f2910a` |
| Dampak Database | **0 tabel baru, 0 kolom baru, 0 migrasi EF Core** (menggunakan tabel existing CRM dan IGD) |
| ID berikutnya | `BE-RWI-254`; `FE-RWI-270`; wajib scan ulang sebelum alokasi baru |

### 22.1 Ringkasan Desain Teknis Kontrak 0.17.0

1. **Backend Service & Query**:
   - DTO `InpatientAdmissionTransferItemResponse` diperluas dengan field aditif: `isCito: bool`, `isPriorityMember: bool`, `memberTierName: string?`, `memberPriorityLevel: int?`.
   - Method `BuildTransferQuery` memproyeksikan left-join ke `EmgAdmissionRequest` (status `Signed`) dan `MstPatientMembership` join `MstMembershipTier` (`Active`, tanggal valid hari ini, `PriorityQueue == true`).
   - Algoritma pengurutan multi-kriteria: `OrderByDescending(x => x.IsCito).ThenByDescending(x => x.IsPriorityMember).ThenBy(x => x.MemberPriorityLevel ?? int.MaxValue).ThenBy(x => x.DecidedAt).ThenBy(x => x.TransferId)`.
   - Fail-safe fallback jika data CRM / IGD null tanpa error HTTP 500.
2. **Frontend UI & Visual Lencana**:
   - `InpatientAdmissionTransferList.jsx`: Kolom pasien menampilkan `StatusBadge` merah `[CITO]` dan `StatusBadge` kuning/emas `[TierName]`.
   - `InpatientAdmissionTransferReviewStep.jsx`: Header ringkasan pasien menampilkan lencana CITO dan Tier Membership saat petugas admisi meninjau pasien transfer.
   - Denah Bed Management bangsal umum tetap bersih dari lencana membership demi privasi (`RWI-DEC-341`, `RWI-AC-488`).
   - Petugas admisi tetap memilih kamar dan tempat tidur secara manual melalui Denah Bed Management (tanpa auto-booking server).

### 22.2 Integritas Artefak Delivery Roadmap

Hash berikut adalah SHA-256 byte aktual dari berkas delivery yang dihasilkan pada revision 17:

| Artefak Relatif | Status | SHA-256 Byte Aktual |
|---|---|---|
| [roadmap/backend-roadmap-prioritas-antrean-member.md](./roadmap/backend-roadmap-prioritas-antrean-member.md) | rev 1; 2 task, grafik 1 pasangan | `3f1f61aa7dda0e78344472de38681c17f51ac7e066a4c8fa521fd7177e1f24c0` |
| [roadmap/frontend-roadmap-prioritas-antrean-member.md](./roadmap/frontend-roadmap-prioritas-antrean-member.md) | rev 1; 2 task, grafik 1 pasangan | `9851c3d863f77f1772dab22f94e0ebb072b3aad3f439978bfa9a3a6287975cef` |
| [roadmap/requirement-traceability-prioritas-antrean-member.md](./roadmap/requirement-traceability-prioritas-antrean-member.md) | rev 1; FR/AC/VAL/UAT ke task; zero gap | `80ea82c9c90b5474c0ceb40c3f6d0f3a4858fbedd18a065f939a5dab4b1057e2` |
| [roadmap/prioritas-antrean-member-delivery-lock.json](./roadmap/prioritas-antrean-member-delivery-lock.json) | rev 1; delivery lock snapshot | `233023844cfc934a587c2449f5154e1084c434d6589d910a2f8b9a214138109e` |

Eksekusi tetap tepat satu task per pemanggilan builder (`build-module-backend` / `build-module-frontend`). Roadmap ini tidak memberi izin perubahan database/deployment dan tidak mengubah source aplikasi secara liar di luar scope task.

---

## 23. Manifest revision 18 — Peta Bangsal Denah, 20 Oktober 2026

**Status: `draft` — menunggu approval pemilik.** Desain amandemen rupa denah Bed Management. Approval
revision 17 dan sebelumnya tetap berlaku untuk isinya masing-masing.

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001/episode-rawat-inap` |
| Revision | **`18`** |
| Contract set | **`0.18.0`** (`draft`) |
| Status | **draft** — `approved_by` dan `approved_at` belum ada |
| Scope | Rupa denah Bed Management menjadi peta bangsal di Monitoring Bed, Pilih Bed, transfer, dan Master Bed; tanpa perubahan backend, API, maupun data |
| Requirement | `RWI-DEC-343`–`351` (`350` dan `351` asumsi draft), `RWI-AC-498`–`507`, `RWI-CON-018` ditutup, `RWI-FE-009`–`012` |
| Capability | `DNH-CAP-34`, `36`, `37`, `41`; gate `DNH-CG-01`, `DNH-CG-05` |
| Readiness | `DNH-RCG-20261015-01` (gate 1.13) `READY_FOR_DOMAIN_DESIGN`; tidak dinilai ulang karena tidak ada kemampuan baru |
| Domain architecture | `DOMAIN_ARCHITECTURE_NOT_RUN` |
| Flow | `FLOW-DNH-MVP-001` dan `FLOW-DNH-MVP-002` tidak berubah |
| Epic/FR/NFR/UAT | `EPIC DNH-07` (`FR-DNH-017`–`024`); `EPIC DNH-08` `OPEN DECISION` (`FR-DNH-025`); `NFR-808`–`813`; `UAT-813`–`819`; `DNH-AT-498`–`507` dan `DNH-AT-914`–`923` |
| Terbuka | `PETA-OPEN-01` — warna titik status kartu dan pop up; memblokir `EPIC DNH-08` saja |
| Source snapshot | BE `6e0f2b1b58a2192a0f54f0fdff718bc77670e3bd`; FE `a491b05afcb6b3ed70218ccaa557007719d45ac3` |
| Dampak database | 0 tabel baru, 0 kolom baru, 0 migration |

> Snapshot historis revision 18. Persetujuan `RWI-DEC-352` dan registrasi delivery ada di bagian 24;
> hash kanonis sesudah persetujuan tersimpan di lock `PETA-DEL-20261020-01`.

### 23.1 Masukan dan hash

| Input | Revision/readiness | SHA-256 |
| --- | --- | --- |
| ../00-interview-decisions.md | 62; DEC-343–351, AC-498–507, CON-018, FE-009–012 | `48dc49dc39851a2fa650968a3391b57375accaf61aa3b0d9dee99d07d56e4ca6` |
| ../evidence/02-requirement-completeness-gate.md | 1.13; `DNH-RCG-20261015-01` | `d1ddeeda87aa2888397deecada8027296dbd3734669ae00ca210fb044431ae5b` |
| ../evidence/denah-bed-impact-scan-20261015.md | `BM-IMP-20261015-01` rev 1; `DNH-CAP-34`–`41` | `813430d0a005f71392ed657bc7de0c0e52e466cecc7d74f15e8110345832f3cb` |
| docs/map-bed/planning/planning-redesain-tampilan-denah-bed.md | Planning; keputusan disetujui, desain draft | `47bb7cb82d5854cfcc9d61ddf72855ca5462a182e627d89d16256e2921f8dec0` |
| ../02-module-map.md | Tidak berubah; bagian 10 | `a135c59c4b55ea97e240d463511a3e2a41ba22051e398ee4e5bb6d1c0b8b651a` |
| Manifest anak sebelum amandemen ini | revision 17 | `d13364becfca202a57eaf99e62261b7fafbf00746c519523864637eaa0c8e6cf` |
| Manifest modul sebelum amandemen ini | revision 14 | `fe538d8ae312213589d868419ff4f6e4dcae035a0fe5fc9f4bb653302c9b1dcf` |

### 23.2 Owner, authority, dan approval

| Kewenangan | Pemilik/batas | Bukti |
| --- | --- | --- |
| Produk/domain | Muhammad Hamzah | `RWI-DEC-061` |
| Pemberi arahan perubahan | Atasan pengguna; dicatat atas nama Muhammad Hamzah | `RWI-DEC-343`, `RWI-FACT-079` |
| Frontend | Pengembang dalam batas frontend 21.3; `RWI-FE-009`–`012` | Decision log revision 62 |
| Klinis dan privasi | Penayangan HD/LD di produksi tetap gate `DNH-G04`; pemilik privasi `OPEN` | Bagian 17.5 |
| approved_by / approved_at | Belum ada | Approval adalah tindakan manusia |

### 23.3 Artefak dan hash

SHA-256 byte berkas apa adanya (CRLF) sesudah amandemen ini.

| Artefak relatif | Status / last_changed_in | SHA-256 |
| --- | --- | --- |
| 03-frontend-architecture.md | draft / 0.18.0 — bagian 21; catatan penggantian pada 16 dan 16.7 | `a4a6946f1553ad03ef11a9049cfb918d6ec791a518072ba446a6d25fbe4733a1` |
| 04-prd-to-mvp.md | draft / 0.18.0 — bagian 32; catatan pada `NFR-801` dan `UAT-801` | `4613e7812fb574ba89e7149adb0123c9f66ecddf7d462dafff6d9134f979a5d5` |
| testing/acceptance-test-matrix.md | draft / 0.18.0 — bagian 26; catatan pada `DNH-AT-911` dan `UAT-801` | `cee9f576a7d5edbe0fb428ab09bd0183bd57bf647e675aa1ae647d1da4e765fa` |
| 02-backend-architecture.md | Tidak berubah | `ae492732dbf78d668e1cd54ee900a4a235ddb6c4a7e139d6deb823b91ff9f120` |
| data/data-dictionary.md | Tidak berubah | `10e498c55eac6e9ded2419059eb5c80708e7f4df70a0fc754c817984806290fb` |
| contracts/api-contract.md | Tidak berubah | `ba624bce05bbb47421b8169da15f060955099a23ac3753fb9117fbb4889ae125` |
| contracts/state-transition-matrix.md | Tidak berubah | `3e644053d085c87d83d9bce9602db1c7c2893f2e03f1a4679013454629e6cb3e` |
| contracts/validation-matrix.md | Tidak berubah | `c5d1e40b343cfc28ab372f1ce14f6fc4314057f49c830d0abb404e7358205b67` |
| contracts/integration-contract.md | Tidak berubah | `a31fde2eb3467486b460bb16cf1ccb37503cc4507bff343ca567310220d05480` |
| contracts/permission-audit-matrix.md | Tidak berubah | `9fc9aab72951d9b839088ce874a4f0dd647a15ca5ed3467c76f3655a17c2cd8a` |
| flowcharts/15-denah-pilih-bed.md | Tidak berubah | `2d8a933b0f04518879a850a6bb24c06465fa57317195ee7ba1913c383b6fe53b` |
| flowcharts/16-denah-master-bed.md | Tidak berubah | `a2bfeeed93b7cb8c60024131de25032dd8d65df85822d0056cfb4bb6462647ce` |

### 23.4 Scope, compatibility, dan supersession

| Perubahan | Dampak |
| --- | --- |
| Gambar CAD diganti peta bangsal | Hanya rupa; posisi kamar, kunci denah, dan data tetap |
| Rupa status kembali ke `RWI-DEC-303`/`316` | Perubahan perilaku yang terlihat: tepi teal menjadi hijau, kamar penuh berwarna menjadi abu berlabel, aturan HD suara terbanyak menjadi "sekurang-kurangnya satu HD" dari server |
| Kamar penuh dan kamar Master dapat difokus keyboard | Perbaikan aksesibilitas (frontend 21.2 no. 10) |
| Ringkasan lantai dan angka tab | Aditif; dihitung dari respons yang sudah ada |
| Backend, API, database | Tidak berubah |

Yang digantikan: frontend 16.1 (brief rupa dan batas `DEV_DISCRETION`), 16.4 (baris
`floor-plan-drawing`), 16.6 (kalimat bukti tampilan), dan 16.7 butir 3; PRD `NFR-801` (sebagian) dan
`UAT-801` (kalimat terakhir); acceptance `DNH-AT-911` (sebagian) dan `UAT-801` (kalimat terakhir);
`RWI-DEC-325` butir 3 digantikan `RWI-DEC-343` di decision log. Rupa status dari commit frontend
`685844a34` dicabut `RWI-DEC-346`.

### 23.5 Gate dan pertanyaan terbuka

| Gate / butir | Isi | Dampak |
| --- | --- | --- |
| `PETA-OPEN-01` | Warna titik status di kartu dan pop up (frontend 21.12) | `EPIC DNH-08` tidak masuk gelombang sampai diputuskan; disarankan dijawab sebelum contoh Lantai 3 ditunjukkan |
| Asumsi `RWI-DEC-350`, `RWI-DEC-351` | Label kamar tanpa penilaian; tampilan campuran tidak dirilis | Tidak memblokir; berlaku sampai pemilik menyatakan lain |
| Pengesahan contoh Lantai 3 (`RWI-DEC-349`) | Atasan melihat Lantai 3, Muhammad Hamzah mencatat pengesahannya | Syarat mulai `MVP-PETA-2` |
| `DNH-G04` | Kriteria klinis HD/LD dan persetujuan privasi | Tetap; penayangan HD/LD di produksi |

### 23.6 Validasi desain dan batas bukti

| Pemeriksaan | Hasil | Bukti / batas |
| --- | --- | --- |
| `git diff --check` | PASS | Tiga berkas desain dan planning |
| Akhir baris | PASS | Berkas CRLF tetap CRLF; planning tetap LF seperti saat dibuat |
| Rujukan keputusan | PASS | 30 ID `RWI-*` pada bagian baru ditemukan di decision log revision 62 |
| ID internal | PASS | `FR-DNH-017`–`025`, `NFR-808`–`813`, `UAT-813`–`819`, `DNH-AT-498`–`507`, `DNH-AT-914`–`923`: semua yang dirujuk terdefinisi |
| UAT | PASS | `UAT-813`–`819` identik di PRD 32.18 dan acceptance 26.4 |
| Tautan lokal dan lebar tabel | PASS | Bagian 21, 26, dan 32 |
| Kesegaran source | PASS | FE `a491b05af` bersih; BE `6e0f2b1b`. Perubahan dokumen paralel (roadmap prioritas member, laporan `BE-RWI-252`/`253`) tidak dinilai |
| Source, build, dan test aplikasi | NOT RUN | Fase dokumentasi; pembuktian pada delivery |

Butir yang diketahui tidak sinkron dan **tidak** diperbaiki karena di luar amandemen ini:

1. API contract 14.2 masih melabeli tiga endpoint denah "Rencana (belum tersedia)" padahal sudah ada
   di kode; isi kontrak tidak bergerak pada `0.18.0`.
2. Kepala frontend architecture menyebut `0.17` `draft`, sedangkan manifest ini menyebut revision 17
   `approved`.
3. Frontend architecture punya dua bagian bernomor 20 (realtime dan lencana tier).
4. Manifest modul belum mencatat child revision 17.

### 23.7 Langkah berikutnya

Approval desain oleh Muhammad Hamzah, sebaiknya bersama jawaban `PETA-OPEN-01`, lalu
`plan-module-delivery` untuk task contoh Lantai 3 dan lima lantai lain. ID frontend berikutnya saat ini
`FE-RWI-272`; pindai ulang sebelum alokasi. Revision ini tidak membuat roadmap dan tidak mengubah source
aplikasi, database, maupun Git.

---

## 24. Delivery Peta Bangsal Denah — 20 Oktober 2026

`plan-module-delivery` dijalankan sesudah persetujuan `RWI-DEC-352`. Revision 19 mengikat set kontrak
`0.18.0` yang dirancang pada revision 18. Hash bagian 23 tetap snapshot draft historis; snapshot
approved/planned yang kanonis ada di lock di bawah.

| Field | Nilai |
|---|---|
| planning_id / revision / status | `PETA-DEL-20261020-01` / 1 / `APPROVED_DESIGN_PLANNED` |
| Approval | Muhammad Hamzah, 20 Oktober 2026: "iya saya setujui semua ya atas nama saya Muhammad Hamzah" → `RWI-DEC-352` (desain dan asumsi `RWI-DEC-350`/`351`) dan `RWI-DEC-353` (palet titik, menutup `RWI-OQ-165`) |
| Task backend | Tidak ada (`RWI-DEC-343`); roadmap backend memuat alasannya |
| Task frontend | 8 task: contoh Lantai 3 `FE-RWI-272` s.d. `FE-RWI-275`; lima lantai lain `FE-RWI-276` s.d. `FE-RWI-278`; penutupan `FE-RWI-279` |
| Dependency awal | `FE-RWI-272` gelombang 1; tidak ada prasyarat backend |
| Gerbang | `DEC-PETA-L3`: atasan mengesahkan contoh Lantai 3 dan Muhammad Hamzah mencatatnya sebagai keputusan baru; menahan `FE-RWI-276` s.d. `FE-RWI-279` |
| Coverage | 9 FR, 11 AC, 6 NFR, 8 UAT, 21 butir uji — **Zero Coverage Gap** |
| Source snapshot | BE `6e0f2b1b58a2192a0f54f0fdff718bc77670e3bd`; FE `a491b05afcb6b3ed70218ccaa557007719d45ac3` |
| Laporan / runtime | Belum ada; seluruh task `NOT_STARTED` atau ⛔ menunggu gerbang |
| ID berikutnya | `FE-RWI-280`; `BE-RWI-254` tetap. `FE-RWI-271` dilewati karena roadmap paralel prioritas member menyebutnya sebagai ID berikutnya. Wajib pindai ulang sebelum alokasi baru |

| Artefak relatif | Status | SHA-256 byte aktual |
|---|---|---|
| [roadmap/frontend-roadmap-peta-bangsal-denah.md](./roadmap/frontend-roadmap-peta-bangsal-denah.md) | rev 1; 8 task; 9 pasangan dependency dan 2 garis syarat gerbang | `b24cfd81100ff36be9e0d4692fb0eca35a6e360c7c84558d028d8d6bf493b3cf` |
| [roadmap/backend-roadmap-peta-bangsal-denah.md](./roadmap/backend-roadmap-peta-bangsal-denah.md) | rev 1; nol task beserta alasannya | `98144e9a2e45a0edd5a2d404349345c7bc24fbda38ef7e35f84d72c1edd8a339` |
| [roadmap/requirement-traceability-peta-bangsal-denah.md](./roadmap/requirement-traceability-peta-bangsal-denah.md) | rev 1; FR, AC, NFR, UAT, butir uji ke task; zero gap | `f0b2af462cddbb38ec60cdd1bdd07160bac95e76aefcdc0089d74485152af900` |
| [roadmap/peta-bangsal-denah-delivery-lock.json](./roadmap/peta-bangsal-denah-delivery-lock.json) | rev 1; 19 hash masukan dan 2 manifest sebelum registrasi | `20a782ef7e643983c3528a5139f5afc9aa9e8a537646a5ebd5295a73de609477` |

Eksekusi tetap tepat satu task per pemanggilan `build-module-frontend`. Roadmap ini tidak memberi izin
perubahan database maupun deployment, dan tidak mengubah source aplikasi.
