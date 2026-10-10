# Sub-modul `episode-rawat-inap` — Blueprint Manifest

Sub-modul dari modul [`rawat-inap`](../blueprint-manifest.md), bentuk `COMPOSITE` sejak
`RWI-DEC-082`. Identitas modul, snapshot SHA, hash masukan hulu, dan registry sub-modul dipegang
manifest tingkat modul. Berkas ini memegang **status desain, `contract_versions`,
`artifact_hashes`, approval, dan dependency sub-modul ini sendiri**.

| Field | Value |
|---|---|
| `submodule_slug` | `episode-rawat-inap` |
| `blueprint_id` | `RWI-BP-001` — satu untuk seluruh modul |
| `revision` | **`11`** — draft Bed Management, bagian14, 10 Oktober2026. Riwayat metadata: **`10`** — amandemen Alur Admisi Pendaftaran pada bagian 13, ditulis 2026-10-10, **disetujui 2026-10-10 atas instruksi pengguna "setujui dan lakukan /plan-module-delivery"**, menyerap revisi tim analisis bisnis (Mba Ilma) via `RWI-DEC-267` s.d. `RWI-DEC-273` dan `RWI-AC-388` s.d. `RWI-AC-395`. Sebelumnya **`9`** — amandemen Workspace PPRI pada bagian 12, ditulis 2026-10-07, **disetujui 2026-10-08 lewat `RWI-DEC-265`**. Sebelumnya **`8`** — Finishing Rawat Inap (`approved`, `RWI-DEC-221`) |
| `status` | **`draft`** — Bed Management belum disetujui; content approval lama tetap pada scope sebelumnya. Riwayat metadata: **`approved`** — amandemen revision `10` / kontrak `0.11.0` disetujui pengguna 10 Oktober 2026 (bagian 13). Riwayat: **`approved`** — amandemen revision `9` / kontrak `0.11.0` **disetujui Muhammad Hamzah 2026-10-08 lewat `RWI-DEC-265`** (bagian 12); revision `8` / kontrak `0.10.0` **disetujui 2026-10-02 lewat `RWI-DEC-221`** |
| `contract_versions` | **`0.12.0`** — satu set target Bed Management, draft; sumber kebenaran current. Riwayat metadata: **`0.11.0`** — amandemen alur admisi pendaftaran (`approved` 10 Oktober 2026); Workspace PPRI (`approved` 2026-10-08 lewat `RWI-DEC-265`); Finishing tetap berpegang pada **`0.10.0`** (`approved` 2026-10-02 lewat `RWI-DEC-221`) |
| `upstream_realignment` | **`REALIGNED_APPROVED` sejak 2026-09-16** lewat `RWI-DEC-150` |
| `prefix` | Entity `Inp`; task `BE-RWI-###` dan `FE-RWI-###` |
| `approved_by` | null untuk revision11. Riwayat metadata: **Muhammad Hamzah** (Product/Domain owner, `RWI-DEC-061`) / Pengguna |
| `approved_at` | null untuk revision11. Riwayat metadata: **`2026-10-10`** untuk revision `10` / kontrak `0.11.0` (Alur Admisi Pendaftaran); **`2026-10-08`** untuk revision `9` / kontrak `0.11.0` lewat `RWI-DEC-265`; **`2026-10-02`** untuk revision `8` / kontrak `0.10.0` lewat `RWI-DEC-221` |
| `rumpun kemampuan` | Episode, tempat tidur, penanggung jawab, pemulangan, penutupan, admisi & consent |
| `kemampuan` | **16** — `CAP-001` s.d. `CAP-011`, `CAP-017`, `CAP-018`, `CAP-019`, `CAP-026`, `CAP-028`, sesuai `RWI-DEC-083`; **ditambah Workspace PPRI** `CAP-RWA-01` s.d. `CAP-RWA-17` tanpa `CAP-RWA-04` (`RWI-DEC-227`); **ditambah Amandemen Admisi & Digital Consent** (`RWI-DEC-267` s.d. `273`) |
| `uji pemecahan` | **5/5** syarat `bentuk-blueprint.md` bagian 4.1 |
| `design_snapshot_at` | 2026-10-10 untuk Bed Management; Riwayat metadata: `2026-08-24` untuk revision `4`; migrasi bentuk 2026-09-02 |
| `updated_at` | 2026-10-10 — amandemen Bed Management draft; Riwayat metadata: `2026-10-10` — persetujuan amandemen alur admisi pendaftaran (revisi Mba Ilma: `RWI-DEC-267` s.d. `RWI-DEC-273`). Sebelumnya `2026-10-08` — approval revision `9` / kontrak `0.11.0` (`RWI-DEC-265`) |

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
| ../../../../../artifacts/bed-management/01-existing-capability-map.md | BM-AUD-20261010-01 rev1; source-audited, Swagger section7 | 50e0e1525804260331d3a830fb379532f802a3ff31e43cd3b4df4416fa42f38b |
| ../02-module-map.md | 6 draft; section9 Bed Management | dd31850cf99d1ef3dda5030693405ffe57b6989b3dfecf3330c4d7f9a8665c84 |
| Previous child manifest snapshot | revision10 approved; input history before amendment | b661259ee1812418ab0e78bc03b7c0f6dc19dda05f5bfe18b6c847843d568a8c |

Input links: [decisions](../00-interview-decisions.md), [gate](../evidence/02-requirement-completeness-gate.md), [audit](../../../../../artifacts/bed-management/01-existing-capability-map.md), [module map](../02-module-map.md). `input_hash` tiap artefak Bed Management merujuk tabel ini; PRD26.1 memuat hash artefak teknis saat diturunkan. `DOMAIN_ARCHITECTURE_NOT_RUN` khusus BM: gate siap untuk bounded design langsung; arsitektur domain old scope lain tetap berlaku.

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

Hash berikut mendeteksi drift pada **draft current**; tabel hash lama adalah bukti versi terdahulu. Manifest ini tidak menghitung hash dirinya sendiri. Contract set baru tidak mengubah last_changed_in pada extra contract lain yang tidak disentuh.

| Artefak relatif | Status / last_changed_in | SHA256 |
| --- | --- | --- |
| 02-backend-architecture.md | draft / 0.12.0 | 0d696cfc7a5e2a01734508e518978c5db764de450c03472f30e7b1e5fb74b1ad |
| data/data-dictionary.md | draft / 0.12.0 | f411d5b87f54ddb53b241b78c51f5ab4ebd22100c6a3e07aae8b192263e87098 |
| contracts/api-contract.md | draft / 0.12.0 | ddd31d88a469239e6f731eea2fe3023499fd0137ae3e35c3de284b03a1c7c531 |
| contracts/state-transition-matrix.md | draft / 0.12.0 | 78a286e0f8441dd626c5e9fb6687ecdc549d7410d8e7867a1cdc45a12bd274e5 |
| contracts/validation-matrix.md | draft / 0.12.0 | a8a225a84f51b9a1e455285edfc0458c830ef1775d59f767d6e49e1664bf1dce |
| contracts/permission-audit-matrix.md | draft / 0.12.0 | 893c0c2d24e8ed3de0f25b42aec853d5b05e22bc89c0acf06371e6949f680960 |
| contracts/integration-contract.md | draft / 0.12.0 | 6ea617368b475f2493c59994181e7b8238574f1913f8857cb0095e4c84ac3714 |
| 03-frontend-architecture.md | draft / 0.12.0 | 5f18ddfbf44e7af2b8192f407a1cf6c625e6b38e7a1699ffe0bbabec308165d4 |
| testing/acceptance-test-matrix.md | draft / 0.12.0 | c8c67cb6504021e68832b2b42b9d272f3209f6435b025bdab88c88380d0f26af |
| flowcharts/00-alur-utama.md | draft / 0.12.0 | a2a5d5897366620db03972cdad6fa32ec807af6539b3f8522588ea3ae71ed13b |
| flowcharts/09-bed-reservation-release.md | draft / 0.12.0 | d7015fe25aa0b71af88b97b0756bc59e88f3034d66c6dbedc601ffb67f475837 |
| flowcharts/10-bed-transfer.md | draft / 0.12.0 | adff6804dbc83f16f1ff99c6ace63bcb80cf409bdf54aa0452d61019cbc19609 |
| flowcharts/11-bed-cleaning-readiness.md | draft / 0.12.0 | a2bd18ac9d8b49125e69e6fd929e1d19423149c3dc8118f4a7dd42cab35f838c |
| flowcharts/12-bed-closure-reopen.md | draft / 0.12.0 | a9340f628728ef16c556b3f2bf1e718e0826c8b095a89b3b1f47dd0f54f3abb4 |
| flowcharts/13-bed-usage-history-correction.md | draft / 0.12.0 | 92d3e1eaf472c44ec7e94592bd4039b651cebf0a98f69ab9545dde8ff6161651 |
| flowcharts/14-bed-uncertain-outcome.md | draft / 0.12.0 | 114cd5d9bd6b8cd3a8b6d28019810c8362f5935fe25ac2d267b9ed2f90edd788 |
| 04-prd-to-mvp.md | draft / 0.12.0 | b877c31dcc96ba7d33b4e66fc0c379f46cb27e4bf4d2519cd688da337f507cdd |

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

Pada pemeriksaan sebelum manifest:20 bagian PRD,31AC,12UAT,9diagram structurally checked, tabel/tautan lokal/git diff check lulus;12body sejarah sebelum amandemen tetap sama setelah normalisasi header.62fingerprints dan HEAD BE/FE sama; frontend status tetap28. Pemeriksaan final manifest/hash/report melanjutkan bukti tersebut. Mermaid rendering dan aplikasi/API/PostgreSQL/migration/UAT runtime **NOT_RUN**. Rincian final ada pada [laporan validasi](./testing/bed-management-design-validation.md).

### 14.7 Supplement bukti source

| Source backend tambahan dibaca | SHA256 snapshot desain |
| --- | --- |
| Areas/HealthServices/MasterData/Controllers/RoomController.cs | ea4e84c98dcf4f1384add868f431ab6ab629ee6b38c69d261621218337fb794a |
| Areas/HealthServices/MasterData/Controllers/ServiceUnitController.cs | 541dd37182d46e9a565642ac7cdaa894325bb4a0f32f469ebab047ee425979b8 |
| Areas/HealthServices/MasterData/Controllers/PatientClassController.cs | 7d5838c242f27957ae818f79fee6a9b819140f16daf80f8c70bc5f655a603b59 |
| Repositories/Configurations/HealthServices/MstBedConfiguration.cs | 5673d6bd77a9a41605b90fec8def0c062e58aed2a348ff247ebd7c6cfc23a0a5 |
| Models/ApplicationUser.cs | 214a431732b187a98f4e618d20defeebcc6938069f38b6d3e413bd5aec90f45b |

Source tambahan digunakan untuk memastikan hierarchy writer actual, path EF config MasterData dan ApplicationUser. Fingerprints original62 tetap unchanged; tidak menulis ulang audit as-is atau decision log.
