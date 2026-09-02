# Sub-modul `episode-rawat-inap` — Blueprint Manifest

Sub-modul dari modul [`rawat-inap`](../blueprint-manifest.md), bentuk `COMPOSITE` sejak
`RWI-DEC-082`. Identitas modul, snapshot SHA, hash masukan hulu, dan registry sub-modul dipegang
manifest tingkat modul. Berkas ini memegang **status desain, `contract_versions`,
`artifact_hashes`, approval, dan dependency sub-modul ini sendiri**.

| Field | Value |
|---|---|
| `submodule_slug` | `episode-rawat-inap` |
| `blueprint_id` | `RWI-BP-001` — satu untuk seluruh modul |
| `revision` | `5` — satu angka, dipegang tingkat modul |
| `status` | **`approved`** — revision `4` disetujui **Muhammad Hamzah** 2026-08-24 lewat `RWI-DEC-074`; revision `3` sebelumnya lewat `RWI-DEC-067` |
| `prefix` | Entity `Inp`; task `BE-RWI-###` dan `FE-RWI-###` |
| `approved_by` | **Muhammad Hamzah** — Product/Domain owner, ditunjuk `RWI-DEC-061` |
| `approved_at` | `2026-08-24` |
| `rumpun kemampuan` | Episode, tempat tidur, penanggung jawab, pemulangan, penutupan |
| `kemampuan` | **16** — `CAP-001` s.d. `CAP-011`, `CAP-017`, `CAP-018`, `CAP-019`, `CAP-026`, `CAP-028`, sesuai `RWI-DEC-083` |
| `uji pemecahan` | **5/5** syarat `bentuk-blueprint.md` bagian 4.1 |
| `design_snapshot_at` | `2026-08-24` untuk revision `4`; migrasi bentuk 2026-09-02 tidak mengubah isi desain |

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
| [`02-backend-architecture.md`](./02-backend-architecture.md) | `0.5` | `draft` | `b1bb39dc0c4da1d1e14b362cc5d0a85b8452a17d780f4a59a79ab93b43c6504f` |
| [`03-frontend-architecture.md`](./03-frontend-architecture.md) | `0.5` | `draft` | `7be9f2b30409c23841161daaccc29c167ebeb0238a4b82ac1522826e9ca3df42` |
| [`04-prd-to-mvp.md`](./04-prd-to-mvp.md) | `0.4.1` | `draft` | `f8c71b8479a0423ca68795427198a674e2dd9ad01939fa692a4c38a4d3457533` |
| [`05-skema-tampilan.md`](./05-skema-tampilan.md) | `0.4` | `draft` | `f74a845433ba64806ee1cd945f8ca515228af2a470082c4095f95f682ceed09e` |
| [`data/data-dictionary.md`](./data/data-dictionary.md) | `0.4` | `draft` | `85551a5a5c966685937aa97cf79cc40c5b247e902d151a4daa6a132540e7f170` |
| [`erd/00-context-erd.md`](./erd/00-context-erd.md) | `0.3` | `draft` | `73eaa7d0c6d0567a37380679b4c9c0fd150d75a8851e7b6fd4b1d5f4e28a41e4` |
| [`erd/01-inpatient-episode.md`](./erd/01-inpatient-episode.md) | `0.3` | `draft` | `7f21508a0f66470b9b6b1d625359636882b89481318b16da7472735e916449eb` |
| [`erd/02-inpatient-configuration.md`](./erd/02-inpatient-configuration.md) | `0.1` | `draft` | `3645ee9d1788270ee7cef88d2cc6b74beddddec0a1a5d2b538e45c25c66f2065` |
| [`contracts/api-contract.md`](./contracts/api-contract.md) | `0.4.0` | `draft` | `0357e52c2e35a0812d439758c74887c17b93c1bc24e1443b9a0ad6787061d14a` |
| [`contracts/state-transition-matrix.md`](./contracts/state-transition-matrix.md) | `0.4.0` | `draft` | `35e8e769461a05b32da5d9e6d11ef92dc45c254b2c1a7d4eb08d228a5d9c1fc7` |
| [`contracts/validation-matrix.md`](./contracts/validation-matrix.md) | `0.4.0` | `draft` | `995b355dae2073f84f286095d36fa74cb2ad912ff07e2f7525a1bbfa9b96c82b` |
| [`contracts/integration-contract.md`](./contracts/integration-contract.md) | `0.4.0` | `draft` | `99ef4d4fb982987fa25b51dc49720344366a6bb42d31f8c7c6b153070a62aab0` |
| [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md) | `0.4.0` | `draft` | `c345c10b90344777484ff4444105504070d543b4ecc3587c2155be41aeb7bad9` |
| [`testing/acceptance-test-matrix.md`](./testing/acceptance-test-matrix.md) | `0.4.0` | `draft` | `357cb6ca9b35b9c2a2ce55597dd2cad5c68bd132c4d40a903f07e4d693b3a45c` |

Revision `0.5` pada kedua berkas arsitektur menandai **satu-satunya** perubahan isi saat migrasi:
pemindahan tabel kepemilikan data dan peta butir menu ke tingkat modul. `04-prd-to-mvp.md` naik ke
`0.4.1` karena enam keterangan basi `DEC-INP-001` diperbaiki. Sisanya hanya berpindah tempat, atau
berubah hanya pada rujukan path, sehingga revision-nya tidak bergerak.

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
| API | `0.4.0` | `0.4.0` | `draft` | **Tidak** — nol endpoint bergeser |
| State transition | `0.4.0` | `0.4.0` | `draft` | **Tidak** |
| Validation | `0.4.0` | `0.4.0` | `draft` | **Tidak** |
| Integration | `0.4.0` | `0.4.0` | `draft` | **Tidak** |
| Permission dan audit | `0.4.0` | `0.4.0` | `draft` | **Tidak** |
| Acceptance test | `0.4.0` | `0.4.0` | `draft` | **Tidak** |
| PRD ke MVP | `0.4.0` | `0.4.0` | `draft` | Isinya berubah hanya pada enam keterangan basi `DEC-INP-001`; artefaknya naik ke `0.4.1`, set kontraknya tidak |

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
