# Rawat Inap — Blueprint Manifest

| Field | Value |
|---|---|
| `blueprint_id` | `RWI-BP-001` |
| `revision` | `4` |
| `status` | `approved` — revision `4` disetujui **Muhammad Hamzah** 2026-08-24 lewat `RWI-DEC-074`; revision `3` sebelumnya lewat `RWI-DEC-067` |
| `module` | `rawat-inap` / `InPatientManagement`, prefix entity `Inp` |
| `registry_lifecycle` | `ACTIVE` — dinaikkan dari `PLANNED` 2026-08-24 lewat `RWI-DEC-068`. Wewenang eksekusi database dan deployment tetap terpisah |
| `design_snapshot_at` | `2026-08-24` untuk revision `4`; `2026-08-21` untuk revision `3` |
| `backend_commit_sha` | `5afb54bd75281648010e50ef14f43ca1f80d8efd` (branch `MHamzah`) |
| `frontend_commit_sha` | `dec4fdeff07c3c96ad9f07f41f184c54cf771371` (branch `HamzahV2`) |
| `owners` | Product/Domain: **Muhammad Hamzah**, ditunjuk `RWI-DEC-061` 2026-08-21; jabatan formal belum diisi. Clinical governance: **sebagian terisi** — keputusan isolasi dan jenis kelamin diambil pemilik yang sama lewat `RWI-DEC-064`, cakupan peran selebihnya belum dinyatakan. Security/Privacy: `OPEN`. API dan Frontend authority: sesuai decision log |
| `approved_by` | **Muhammad Hamzah** — Product/Domain owner, ditunjuk `RWI-DEC-061` |
| `approved_at` | `2026-08-24` |
| `requirement_readiness` | `PARTIALLY_READY` |
| `domain_architecture_revision` | `0.1` |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_PARTIAL` |
| `scope` | **Sepuluh** slice: sembilan yang dinyatakan siap arsitektur domain, ditambah `INP-S11` yang terbuka sejak `RWI-DEC-064`. Tujuh slice lain sengaja tidak dirancang |
| `compatibility_impact` | **Tiga belas** tabel baru. **Nol perubahan kolom pada tabel modul lain oleh task Rawat Inap** — janji itu tetap utuh dan tetap diuji lewat `BE-RWI-003` kriteria 5. Di luar itu, `RWI-RULE-029` aturan 2 menuntut satu kolom baru `OriginEncounterId` pada `TrxPatientEncounter` milik `RegistrationManagement`; kolom itu **dikerjakan modul IGD** lewat `IGD-DEC-075`, bukan oleh blueprint ini — lihat `RWI-DEC-073`. **Dua** perubahan perilaku: `PATCH /beds/{id}/availability`, dan penempatan jalur IGD yang kini menunggu event `Tiba` milik IGD sesuai `RWI-DEC-072`. Satu perbaikan pemanggilan di frontend |

---

## 0. Status kesegaran

**Blueprint sudah sejalan dengan masukannya.** Revision `4` menyerap empat keputusan Amendment
Pass 2026-08-24; revision `3` sebelumnya menyerap tiga keputusan penutupan butir organisasi
2026-08-21. Rincian penyerapan terakhir ada pada bagian 0.2.

| Keputusan | Sudah masuk ke |
|---|---|
| `RWI-DEC-064` jenis kelamin dan isolasi **menolak** penempatan | `02-backend-architecture.md` §1.7 (Kelayakan Penempatan tumbuh dari tiga aturan menjadi delapan), `contracts/validation-matrix.md`, `contracts/api-contract.md`, `testing/` bagian 2A, `04-prd-to-mvp.md` `EPIC RI-34` |
| `RWI-DEC-065` kebutuhan isolasi menjadi atribut episode | Enam kolom pada `InpEpisode` beserta enum `InpIsolationSource`, satu endpoint, satu daftar pantau, `GUARD-INP-04`, `erd/`, `03-frontend-architecture.md` `FE-INP-15`, `FR-RI-158` s.d. `FR-RI-161` |
| `RWI-DEC-066` seluruh kamar tidak boleh ditempati campur | Aturan 6 pada Kelayakan Penempatan, dijalankan dengan membaca penghuni yang sedang ada. **Tanpa kolom baru pada `MstRoom`**, `FR-RI-154` s.d. `FR-RI-157` |

`RWI-DEC-061` sampai `RWI-DEC-063` tidak mengubah isi desain; keduanya mengisi kolom `owners` dan
mencabut tiga dari empat gerbang implementasi pada bagian 7.1.

### 0.0 Yang diserap revision `2`

| Keputusan | Sudah masuk ke |
|---|---|
| `RWI-DEC-054` satu pasien satu episode yang hadir | `02-backend-architecture.md` §1.3 dan §1.5, `erd/`, `contracts/validation-matrix.md`, `testing/`, `04-prd-to-mvp.md` `FR-RI-148` |
| `RWI-DEC-055` kepergian fisik pasien | Seluruh berkas. Endpoint baru, kolom baru, nilai enum baru, `INV-INP-01` dilonggarkan |
| `RWI-DEC-056` penanda rawat gabung bayi | `erd/`, `contracts/validation-matrix.md`, `04-prd-to-mvp.md` `FR-RI-152` |
| `RWI-DEC-057` versi resume pulang | Tabel `InpDischargeSummaryRevision` beserta ERD, kamus data, DDL, dan `FR-RI-153` |

`RWI-DEC-053` sengaja tidak mengubah apa pun: riwayat lokasi tetap dimiliki `InpBedPlacement`.

### 0.1 Satu artefak hulu yang kini tertinggal

[`evidence/03-hospital-domain-architecture.md`](./evidence/03-hospital-domain-architecture.md)
masih revision `0.1` dan **belum** memuat perubahan Amendment Pass. Yang perlu diselaraskan:

| Bagian | Yang perlu diperbarui |
|---|---|
| `INV-INP-01` | Dilonggarkan untuk episode `DischargePending` yang kepergiannya sudah dicatat |
| Invariant baru `INV-INP-10` | Satu pasien satu episode yang benar-benar hadir |
| `CMD-INP-15` | Perintah bisnis baru: catat pasien sudah meninggalkan ruangan |
| `ARCH-GAP-002` s.d. `ARCH-GAP-005` | Keempatnya sudah tertutup oleh `RWI-DEC-054` s.d. `RWI-DEC-057` |

**Ini tidak diselesaikan di sini dengan sengaja.** Bounded context, batas aggregate, invariant, dan
lifecycle adalah wewenang `/hospital-domain-architect`; skill penyusun blueprint dilarang
merancangnya ulang. Yang dilakukan revision `2` hanyalah **menerapkan keputusan pemilik** yang sudah
tercatat pada decision log, bukan mengarang konsep domain baru.

Selisih ini **tidak memblokir** pemakaian blueprint, karena isi blueprint dan decision log sudah
sejalan. Yang tertinggal hanya catatan arsitektur domainnya.

---

### 0.2 Yang diserap revision `4` — Amendment Pass 2026-08-24

Empat keputusan lahir dari tiga usulan lintas modul yang datang dari blueprint IGD. Keempatnya
sudah `approved` pada decision log dan **seluruhnya sudah masuk** ke berkas di bawah.

| Keputusan | Sudah masuk ke |
|---|---|
| `RWI-DEC-070` pelonggaran mesin klinis meluas ke `Emergency` | Decision log: `RWI-RULE-026` aturan 3 s.d. 6 beserta blok prasyarat tipe kunjungan. **Tidak ada berkas desain yang berubah** — dokumentasi klinis rawat inap memang di luar scope sesuai `02-backend-architecture.md` bagian 2 |
| `RWI-DEC-071` justifikasi `RWI-DEC-041` ditulis ulang | Decision log: `RWI-RULE-029` bagian “Keadaan yang menjadi masalah”. Keputusannya tidak berubah, jadi tidak ada desain yang bergeser |
| `RWI-DEC-072` waktu tiba milik IGD | `02-backend-architecture.md` §1.7 aturan **9** dan §4.5; `erd/00-context-erd.md` §1 dan §2 (`CTX_EMG`); `erd/data-dictionary.md` `StartDateTime`; `contracts/api-contract.md`, `validation-matrix.md`, `state-transition-matrix.md`, `integration-contract.md` (`INT-INP-06`); `testing/acceptance-test-matrix.md` bagian 2 dan 14 |
| `RWI-DEC-073` `OriginEncounterId` dikerjakan IGD | Field `compatibility_impact`; `02-backend-architecture.md` bagian 2; `erd/00-context-erd.md` §2; `erd/data-dictionary.md` catatan kolom modul lain; `contracts/integration-contract.md` (`INT-INP-07`) |

Sepuluh acceptance criteria baru `RWI-AC-140` s.d. `RWI-AC-149` sudah masuk matriks acceptance
test: satu di bagian 2 karena dapat diuji sekarang, sembilan di bagian 14 karena miliknya slice
yang di luar scope.

**Temuan yang paling penting dari penyerapan ini.** Ketiga usulan IGD ternyata **tidak menahan
satu pun task MVP.** Aturan 9 pada Kelayakan Penempatan hanya menyala bila episode lahir dari
serah terima IGD, dan jalur itu adalah `INP-S09` yang sengaja tidak dirancang pada revisi ini.
`BE-RWI-011` karena itu **tidak terkunci**; ia hanya bertambah satu acceptance criteria penjaga,
yaitu `RWI-AC-147`, yang membuktikan jalur datang langsung tidak ikut berubah.

**Satu artefak sengaja tidak disentuh.** `03-frontend-architecture.md` tidak berubah: pesan
penolakan baru muncul lewat bentuk jawaban 422 yang sudah ada, yaitu daftar aturan yang gagal,
sehingga layar tidak perlu komponen baru.

---
## 1. Peringatan sebelum membaca

Seluruh dokumen pada folder ini berstatus `draft`. Tidak satu pun boleh dipakai sebagai izin
menulis source code.

Dua gerbang implementasi masih terbuka: kesiapan data master, dan persetujuan pemilik
`EmergencyInstallationManagement` yang hanya menahan `INP-S09`. Modul `InPatientManagement`
berstatus `ACTIVE` pada registry sejak `RWI-DEC-068`, dan penulisan source code sudah dibuka
lewat `RWI-DEC-067` — satu task per pengerjaan mengikuti roadmap.

---

## 2. Daftar artefak dan hash

| Artefak | Revision | Status | SHA-256 |
|---|---|---|---|
| [`00-interview-decisions.md`](./00-interview-decisions.md) | `6` | `draft` | `775e92c9d974b646c484d88553bc7f5dcbb4cf6539425ed7ddb7c02c59ec2dfd` |
| [`01-existing-capability-map.md`](./01-existing-capability-map.md) | `1.2` | `source-audited` | `567d7f7ea57537f419efca28d551e965524d27ea1889a00cc7707d17ec74c3b6` |
| [`evidence/02-requirement-completeness-gate.md`](./evidence/02-requirement-completeness-gate.md) | `1.0` | `CURRENT` | `cd59325e17ee6ec66b7d9e331ba7cd1c94a20ce4651c9dd77e22070e550fbed9` |
| [`evidence/03-hospital-domain-architecture.md`](./evidence/03-hospital-domain-architecture.md) | `0.1` | `draft` | `721268f11edd4aff047b6fcf03fce28e4f051cb4d1cf5134c32d11f0f52615d3` |
| [`02-backend-architecture.md`](./02-backend-architecture.md) | `0.4` | `draft` | `50c6c0986a4b9cda4443d8cb515a038804f1de1f92d36d0c4dc2ab12d5f4baea` |
| [`03-frontend-architecture.md`](./03-frontend-architecture.md) | `0.3` | `draft` | `3c5c55b20ed7b10d49fe2dd2487c03c6a174fe852ec4a5b9dfc5f8bd0b429c4a` |
| [`04-prd-to-mvp.md`](./04-prd-to-mvp.md) | `0.4.0` | `draft` | `58b1f281d15d2c5e00ca296762cc1d2968a287363481df68da1ae3e8d0a8f51a` |
| [`erd/00-context-erd.md`](./erd/00-context-erd.md) | `0.3` | `draft` | `2d7baab5a4c3c76fa11149faa8fc99bc7ca0b2aefbe30f5b8327089f2ed3b4e0` |
| [`erd/01-inpatient-episode.md`](./erd/01-inpatient-episode.md) | `0.3` | `draft` | `aaf6aa46591d78a8e48ea7fd02ff1900525e329b5f34f85b098fae9a3ebc17c7` |
| [`erd/02-inpatient-configuration.md`](./erd/02-inpatient-configuration.md) | `0.1` | `draft` | `3645ee9d1788270ee7cef88d2cc6b74beddddec0a1a5d2b538e45c25c66f2065` |
| [`erd/data-dictionary.md`](./erd/data-dictionary.md) | `0.4` | `draft` | `85551a5a5c966685937aa97cf79cc40c5b247e902d151a4daa6a132540e7f170` |
| [`contracts/api-contract.md`](./contracts/api-contract.md) | `0.4.0` | `draft` | `a451e778e37a6596977ce6c2c9e24bc1548cd9dd4efa9a63e642ba02539b709b` |
| [`contracts/state-transition-matrix.md`](./contracts/state-transition-matrix.md) | `0.4.0` | `draft` | `35e8e769461a05b32da5d9e6d11ef92dc45c254b2c1a7d4eb08d228a5d9c1fc7` |
| [`contracts/validation-matrix.md`](./contracts/validation-matrix.md) | `0.4.0` | `draft` | `6ff47efa675605e78bcdb8836fb636bd8744a1c07f2522508aa64261fd3f838d` |
| [`contracts/integration-contract.md`](./contracts/integration-contract.md) | `0.4.0` | `draft` | `e6e86731ae4da27f482e6f659336a74cb0d2d9465f6a04e26fa7bcc6ac331fe1` |
| [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md) | `0.4.0` | `draft` | `50a48e990ac9aaf1d97fc6f7448fd60f513292fd7da717faaaba2eced4d4e19b` |
| [`testing/acceptance-test-matrix.md`](./testing/acceptance-test-matrix.md) | `0.4.0` | `draft` | `357cb6ca9b35b9c2a2ce55597dd2cad5c68bd132c4d40a903f07e4d693b3a45c` |

Hash di atas dipakai mendeteksi perubahan yang tidak tercatat. Bila salah satu berubah tanpa
revision naik, blueprint dianggap tidak konsisten.

---

## 3. Contract version

| Kontrak | Version | Status | Berubah isinya pada `0.4.0`? |
|---|---|---|---|
| API | `0.4.0` | `draft` | Ya — satu penolakan baru dan satu perubahan asal waktu mulai. Tidak ada endpoint baru |
| State transition | `0.4.0` | `draft` | Ya — satu prasyarat baru pada perpindahan ke `Aktif` |
| Validation | `0.4.0` | `draft` | Ya — satu aturan penolakan baru dan satu aturan penanganan waktu |
| Integration | `0.4.0` | `draft` | Ya — dua integrasi **arah baca** baru, `INT-INP-06` dan `INT-INP-07`. Arah tulis tidak bertambah |
| Permission dan audit | `0.4.0` | `draft` | **Tidak.** Tidak ada aktor, endpoint, atau kewenangan yang bergeser |
| Acceptance test | `0.4.0` | `draft` | Ya — satu skenario baru di bagian 2, sembilan butir di bagian 14 |
| PRD ke MVP | `0.4.0` | `draft` | **Tidak.** Hanya nama pemilik `DEC-INP-002` dan lifecycle registry yang dicatat |

Dua kontrak yang **tidak** berubah isinya tetap dinaikkan versinya supaya seluruh kontrak sebaris
dan mudah dicocokkan. Keduanya memuat catatan tegas kenapa isinya tidak bertambah, supaya pembaca
berikutnya tidak mengira ada yang terlupa.

**Catatan penting untuk `0.4.0`.** Empat kontrak berubah isinya, tetapi seluruh perubahan itu
hanya menyala pada jalur serah terima IGD — `INP-S09`, di luar scope revisi ini. Untuk setiap
endpoint, aturan, dan transisi yang benar-benar dipakai MVP, `0.4.0` berperilaku sama persis
dengan `0.3.0`. Tidak ada task berjalan yang perlu diulang karenanya.

---

## 4. Rantai masukan

```text
00-interview-decisions.md  rev 5  (grill-me: Scope + Closure + Amendment Pass)
        |
01-existing-capability-map.md  (trace-existing-capabilities)
        |
evidence/02-requirement-completeness-gate.md  (PARTIALLY_READY)
        |
evidence/03-hospital-domain-architecture.md  (DOMAIN_ARCHITECTURE_PARTIAL)
        |
02-backend-architecture.md + 03-frontend-architecture.md + erd/ + contracts/ + testing/
        |
04-prd-to-mvp.md
```

Setiap tahap hanya meneruskan slice yang dinyatakan siap oleh tahap sebelumnya.

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
| `INP-S12` | Bayi baru lahir dan boks bayi | `EPIC RI-33` |
| `INP-S13` | Riwayat status, audit, dua daftar pantau | `EPIC RI-29`, `EPIC RI-30` |
| `INP-S14` | Pengaturan yang dapat diubah admin | `EPIC RI-31` |
| `INP-S11` | Kelayakan penempatan menurut jenis kelamin dan isolasi | `EPIC RI-34` |
| — | Perbaikan tempat tidur dan pembatasan wewenang status | `EPIC RI-32` |

## 6. Scope yang sengaja tidak dirancang

| Slice | Decision ID |
|---|---|
| `INP-S05` dokumentasi klinis dan visite | `DEC-INP-001` |
| `INP-S06` resep dan obat pulang | `DEC-INP-001` |
| `INP-S09` serah terima IGD | `DEC-INP-002` |
| `INP-S10` persetujuan umum | `DEC-INP-003` |
| `INP-S15` interoperabilitas SATUSEHAT | `DEC-INP-005` |
| Serah terima klinis antar shift | `DEC-INP-006` |
| Cara pulang meninggal dan kabur | `DEC-INP-007` |

**`INP-S11` keluar dari daftar ini pada revision `3`.** `DEC-INP-004` tertutup 2026-08-21 lewat
`RWI-DEC-064` sampai `RWI-DEC-066`, sehingga slice itu berpindah ke bagian 5. Tujuh slice tersisa,
bukan delapan.

---

## 7. Design gate

Blueprint ini adalah desain target, bukan spesifikasi implementasi yang disetujui.

### 7.1 Gerbang implementasi — desain boleh, source code belum

| Gate | Keadaannya |
|---|---|
| Persetujuan pemilik modul tetangga | **TERBUKA SEBAGIAN.** Dicabut 2026-08-21 oleh `RWI-DEC-062` untuk `ClinicalManagement`, `PharmacyManagement`, dan `MasterData` HealthServices. Bagian `EmergencyInstallationManagement` terbuka kembali 2026-08-24 lewat `RWI-DEC-069` — pemiliknya **Rizki Gunawan**, persetujuan formalnya belum tercatat. Hanya menahan `INP-S09`, yang memang di luar MVP |
| Kesiapan data master | **MASIH TERBUKA.** Penanggung jawabnya sudah ditetapkan `RWI-DEC-063` — Admin Master Data / Tim Master Data, target 22 Agustus 2026. Gerbang ini tertutup begitu datanya benar-benar terisi, bukan begitu penanggung jawabnya ditunjuk. Sejak revision `3` syaratnya bertambah: penanda jenis kelamin, isolasi, dan boks bayi harus **benar**, karena kini menolak penempatan |
| Perbaikan tombol tempat tidur | Hari ini selalu gagal 404. Lihat `RWI-DEC-049`. Pekerjaan perbaikan, bukan keputusan |
| Test regresi modul tetangga | Tidak ada satu pun test yang menjaga jalur poliklinik, IGD, dan farmasi. Lihat `RWI-DEC-051`. Pekerjaan uji, bukan keputusan |
| ~~Registry lifecycle~~ | **DICABUT** 2026-08-24 oleh `RWI-DEC-068`. Modul `InPatientManagement` naik `PLANNED` → `ACTIVE`. Wewenang eksekusi database di luar lokal dan deployment tetap terpisah |

### 7.2 Gerbang sebelum produksi — klinis dan privasi

| Gate | Keadaannya |
|---|---|
| Clinical governance owner | **Sebagian terisi.** Keputusan isolasi dan jenis kelamin diambil Muhammad Hamzah lewat `RWI-DEC-064`; belum dinyatakan apakah penunjukan itu mencakup seluruh peran clinical governance |
| Security/privacy owner | Belum ditunjuk |
| ~~`RWI-RULE-012` isolasi dan jenis kelamin~~ | **BERUBAH BENTUK** 2026-08-21. Aturannya sudah final lewat `RWI-DEC-064` s.d. `RWI-DEC-066` dan dirancang sebagai `EPIC RI-34`. Yang tersisa bukan lagi menunggu keputusan, melainkan menunggu epic itu lolos uji |
| `RWI-RULE-021` batas waktu klinis | Gerbang keras. Masih menunggu pemilik klinis |
| `RWI-RULE-025` persetujuan umum | Gerbang keras. `DEC-INP-003` |
| Masa simpan riwayat | `RWI-OQ-035`, keputusan hukum. Sudah dijawab `RWI-DEC-060`, menunggu pemilik hukum |

---

## 8. Yang tidak boleh diubah blueprint hilir

Diwariskan dari arsitektur domain bagian N.5. Perubahan pada butir berikut wajib kembali ke skill
hulu, bukan diselesaikan pada tahap perencanaan atau implementasi:

1. Kepemilikan data pada tabel kepemilikan `02-backend-architecture.md` bagian 2.
2. Kedudukan `MstBed.BedStatus` sebagai **salinan**, bukan sumber kebenaran.
3. Sepuluh invariant `INV-INP-01` sampai `INV-INP-09` beserta cara menjaganya.
4. Bentuk **berperiode** pada `InpDoctorAssignment`, `InpNurseAssignment`, dan `InpBedPlacement`.
   Menggantinya dengan satu kolom yang ditimpa akan menghapus riwayat yang dibutuhkan resume,
   billing, dan interoperabilitas.
5. Kedudukan `InpCorrectionSession` sebagai konsep tersendiri, bukan status episode keenam.
6. **Ditambahkan revision `3`:** kedudukan kebutuhan isolasi sebagai **atribut episode**, bukan
   atribut pasien dan bukan status. Memindahkannya ke pasien akan membuat penanda satu masa
   perawatan ikut terbawa ke perawatan berikutnya.
7. **Ditambahkan revision `3`:** aturan pencampuran kamar diperiksa dari **penghuni yang sedang
   ada**, bukan dari penanda pada `MstRoom`. Menggantinya dengan kolom pada master akan menambah
   perubahan kolom pada tabel modul lain, dan `RWI-DEC-066` menolaknya secara tegas.

---

## 9. Pemicu impact scan

Blueprint ditandai stale dan wajib melewati impact scan bila salah satu berikut berubah:

| Yang berubah | Yang harus ditinjau ulang |
|---|---|
| `backend_commit_sha` atau `frontend_commit_sha` | Capability map lebih dulu, lalu seluruh kontrak |
| `Areas/HealthServices/MasterData/` tempat tidur, kamar, unit layanan, kelas | `erd/`, `contracts/api-contract.md`, `EPIC RI-22`, `RI-23`, `RI-32`, dan sejak revision `3` juga `EPIC RI-34` — Kelayakan Penempatan membaca `MstBed.IsForMale`, `IsForFemale`, `IsIsolationBed`, `IsForNewborn`, dan `RoomId` |
| `Areas/HealthServices/RegistrationManagement/` | `INV-INP-04`, `EPIC RI-21` |
| `Areas/HealthServices/ClinicalManagement/` atau `PharmacyManagement/` | `DEC-INP-001`; slice yang dihentikan mungkin dapat dibuka |
| `Areas/HealthServices/BillingManagement/` | `RWI-RULE-028` aturan 7; sumber kelayakan keuangan mungkin berpindah |
| `Repositories/ApplicationDbContext.cs` | Rencana migration |
| `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Prefix dan lifecycle modul |
| Munculnya berkas berawalan `Inp` | Seluruh status `Baru` wajib dinilai ulang |

---

## 10. Riwayat revision

| Revision | Tanggal | Ringkasan |
|---|---|---|
| `4` | 2026-08-24 | **Disetujui Muhammad Hamzah lewat `RWI-DEC-074`.** Menyerap empat keputusan Amendment Pass 2026-08-24 yang lahir dari tiga usulan lintas modul milik blueprint IGD. Kelayakan Penempatan tumbuh menjadi **sembilan** aturan; dua integrasi arah baca baru, `INT-INP-06` ke IGD dan `INT-INP-07` ke Registrasi; `InpBedPlacement.StartDateTime` berubah **asal nilainya** untuk jalur serah terima tanpa satu kolom pun berubah bentuk; sepuluh acceptance criteria baru. **Nol tabel baru, nol kolom baru, nol endpoint baru.** Seluruh perubahan hanya menyala pada `INP-S09` yang di luar scope, sehingga tidak satu pun task MVP tertahan |
| `3` | 2026-08-21 | Menyerap tiga keputusan penutupan butir organisasi. **Satu epic baru `EPIC RI-34`** — satu-satunya epic baru sejak blueprint disusun — beserta 9 functional requirement, 5 skenario UAT, dan 26 skenario acceptance test. Enam kolom baru pada `InpEpisode`, satu enum `InpIsolationSource`, dua endpoint baru, satu penjaga service `GUARD-INP-04`, satu daftar pantau baru, satu layar `FE-INP-15`. Kelayakan Penempatan tumbuh dari tiga aturan menjadi delapan. **Nol tabel baru dan nol perubahan kolom pada tabel modul lain.** `INP-S11` berpindah dari slice yang dihentikan menjadi slice yang dirancang. Satu gerbang produksi dan tiga gerbang implementasi berubah keadaan. Kolom `owners` terisi |
| `2` | 2026-08-21 | Menyerap empat keputusan Amendment Pass. Satu tabel baru `InpDischargeSummaryRevision`, tiga kolom baru pada `InpEpisode`, satu nilai enum baru, satu endpoint baru, satu invariant baru `INV-INP-10`, dan `INV-INP-01` dilonggarkan. 6 functional requirement baru, 5 skenario UAT baru, 23 skenario acceptance test baru. Tidak ada kemampuan `MUST HAVE` yang dicabut dan tidak ada epic baru |
| `1` | 2026-08-21 | Blueprint pertama. Dua bounded context, satu aggregate root, dua belas tabel baru, nol perubahan kolom pada tabel modul lain. Sembilan slice dirancang, delapan sengaja dihentikan. 13 epic, 47 functional requirement, 23 skenario UAT, 82 skenario acceptance test |

---

## 11. Langkah berikutnya

| Kondisi | Skill |
|---|---|
| Empat pertanyaan memblokir pada `04-prd-to-mvp.md` bagian 20.2 sudah terjawab dan owner menyetujui blueprint | `/qv-plan` |
| Tujuh pertanyaan tidak memblokir ingin ditutup lebih dulu | `/qv-grill` Amendment Pass |
| Salah satu SHA berubah | `/qv-trace` impact scan |
| Slice yang dihentikan ingin dibuka | Tutup Decision ID-nya lebih dulu lewat `/qv-grill`, lalu ulangi dari `requirement-completeness-gate` |

**Blueprint ini `MUST NOT` diteruskan ke `/qv-plan` sebelum empat pertanyaan memblokir terjawab.**
