# Rawat Inap — Blueprint Manifest

Manifest **tingkat modul**. Pada bentuk `COMPOSITE` berkas ini memegang identitas modul, snapshot
SHA, hash masukan hulu, dan **registry sub-modul**. Status desain, `contract_versions`,
`artifact_hashes`, approval, dan dependency masing-masing sub-modul dipegang manifest **di dalam**
sub-modul itu sendiri.

| Field | Value |
|---|---|
| `blueprint_id` | `RWI-BP-001` |
| `revision` | `5` |
| `blueprint_shape` | **`COMPOSITE`** — ditetapkan `RWI-DEC-082` 2026-09-02 |
| `shape_decided_by` | **`USER_CONFIRMED`** — Muhammad Hamzah; agent menyarankan, pemilik memutuskan |
| `status` | **`approved`** — **diturunkan, bukan ditulis tangan.** Sejak `RWI-DEC-092` pada 2026-09-03, **ketiga sub-modul `approved`**. Lihat bagian 1.1 |
| `module` | `rawat-inap` / `InPatientManagement`, prefix entity `Inp` |
| `registry_lifecycle` | `ACTIVE` — dinaikkan dari `PLANNED` 2026-08-24 lewat `RWI-DEC-068`. Wewenang eksekusi database dan deployment tetap terpisah |
| `design_snapshot_at` | `2026-09-02` untuk revision `5`; `2026-08-24` untuk revision `4`; `2026-08-21` untuk revision `3` |
| `backend_commit_sha` | `5afb54bd75281648010e50ef14f43ca1f80d8efd` (branch `MHamzah`) |
| `frontend_commit_sha` | `dec4fdeff07c3c96ad9f07f41f184c54cf771371` (branch `HamzahV2`) |
| `last_focused_impact_scan` | `2026-09-02`, hanya slice `dokter-rawat-inap`; backend `93b3227c431401d8f586dec4e1fb25fbf41766e3`, frontend `863f24b0d1617069310c04e5770b47fd1b518b5b` |
| `focused_scan_result` | Capability map `CURRENT` untuk slice dokter. `02-module-map.md` bagian dokter dan seluruh artefak `dokter-rawat-inap/` sudah **diamendemen 2026-09-02** dan kini `CURRENT` terhadap `BE@93b3227` serta `FE@863f24b` |
| `last_focused_requirement_gate` | `2026-09-02`, revision `1.3`; seluruh tujuh capability Dokter Rawat Inap `READY_FOR_DOMAIN_DESIGN`. `DEC-INP-008` ditutup oleh `RWI-DEC-084` dan `RWI-DEC-085`. Overall modul tetap `PARTIALLY_READY` karena slice lain tidak dinilai ulang |
| `baseline_requirement` | **`PRD-RWI-FINAL-001` v1.0.0** — `docs/Modul-RS/Rawat-Inap/PRD_Final_Rawat_Inap_100_Persen.md`. **Menggantikan batas scope revision `4`** lewat `RWI-DEC-080`. Scope modul menjadi **28 kemampuan** `CAP-001` s.d. `CAP-028` |
| `owners` | Product/Domain: **Muhammad Hamzah**, ditunjuk `RWI-DEC-061`; jabatan formal belum diisi. Clinical governance: **sebagian terisi** — keputusan isolasi dan jenis kelamin diambil pemilik yang sama lewat `RWI-DEC-064`, cakupan peran selebihnya belum dinyatakan. Security/Privacy: `OPEN`. API dan Frontend authority: sesuai decision log |
| `approved_by` | **Per sub-modul.** `episode-rawat-inap` 2026-08-24, `dokter-rawat-inap` 2026-09-03, `keperawatan` 2026-09-03 — seluruhnya Muhammad Hamzah |
| `approved_at` | — lihat registry bagian 1 |
| `requirement_readiness` | `PARTIALLY_READY` |
| `domain_architecture_revision` | `0.2` — amendment Dokter Rawat Inap, 2026-09-02 |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_PARTIAL` untuk modul. **`DOMAIN_ARCHITECTURE_READY`** untuk scope `dokter-rawat-inap`, yaitu `CAP-015` dan `CAP-020` s.d. `CAP-025` |
| `compatibility_impact` | **Tiga belas** tabel baru, seluruhnya milik `episode-rawat-inap`. **Nol tabel baru** dari `keperawatan` dan `dokter-rawat-inap` — `RWI-DEC-081` menaruh seluruh tabel dokumentasi klinis pada `ClinicalManagement`. **Nol perubahan kolom pada tabel modul lain oleh task Rawat Inap**; janji itu tetap utuh dan tetap diuji lewat `BE-RWI-003` kriteria 5. `RWI-RULE-029` aturan 2 menuntut kolom `OriginEncounterId` pada `TrxPatientEncounter`, dan kolom itu **dikerjakan modul IGD** lewat `IGD-DEC-075`, bukan blueprint ini — `RWI-DEC-073`. **Dua** perubahan perilaku: `PATCH /beds/{id}/availability`, dan penempatan jalur IGD yang menunggu event `Tiba` milik IGD sesuai `RWI-DEC-072` |

---

## 0. Yang diserap revision `5` — migrasi bentuk `SINGLE` → `COMPOSITE`

Revision `5` adalah **revisi material struktur**, bukan revisi isi desain. Tidak ada tabel baru,
kolom baru, endpoint baru, aturan baru, maupun kontrak yang naik versi. Yang berubah adalah letak
berkas dan granularitas approval.

| Keputusan | Sudah masuk ke |
|---|---|
| `RWI-DEC-080` `PRD-RWI-FINAL-001` menggantikan batas scope revision `4`; modul menjadi 28 kemampuan | Field `baseline_requirement`; `02-module-map.md` bagian 4; koreksi **enam** keterangan basi pada `episode-rawat-inap/04-prd-to-mvp.md`, tersebar di bagian 7, 8, 14, dan 16 |
| `RWI-DEC-081` `ClinicalManagement` pemilik tabel dokumentasi klinis; Rawat Inap hanya workspace dan kontrak | `02-module-map.md` bagian 2.3; manifest kedua sub-modul baru bagian 2, lengkap dengan larangan membuat tabel tandingan |
| `RWI-DEC-082` `blueprint_shape: COMPOSITE`, tiga sub-modul, `shape_decided_by: USER_CONFIRMED` | Field `blueprint_shape` dan `shape_decided_by`; registry bagian 1; seluruh struktur folder |
| `RWI-DEC-083` pemetaan 28 kemampuan, nol yatim | `02-module-map.md` bagian 4, lengkap dengan pemeriksaan kemampuan yatim bagian 4.4 |
| `RWI-OQ-047` **terbuka** — sumber kebenaran kelayakan keuangan | `02-module-map.md` bagian 2.4 dan bagian 6, ditulis **apa adanya** sebagai "belum diputuskan"; `episode-rawat-inap/02-backend-architecture.md` bagian 2 |

### 0.1 Tiga dokumen basi yang diperbaiki

Ketiganya masih menyatakan `DEC-INP-001` terbuka, padahal `RWI-DEC-062` sudah menutupnya
2026-08-21 — dan sejak `RWI-DEC-080` keterangan itu bertabrakan langsung dengan scope baru.

| Dokumen | Yang diperbaiki |
|---|---|
| `episode-rawat-inap/04-prd-to-mvp.md` bagian 7, 8, 14, 16 | **Enam** keterangan pada empat bagian. Dokumentasi klinis keluar dari MVP sub-modul ini **karena berpindah pemilik ke sub-modul lain yang belum dirancang**, bukan karena `DEC-INP-001` |
| `evidence/02-requirement-completeness-gate.md` bagian 4.10 | Dua baris `Belum ada` → **`SUDAH ADA`**, beserta pemberi dan tanggalnya |
| `evidence/02-requirement-completeness-gate.md` bagian 5.2 dan 6 | Butir 1 `MISSING`/`BLOCKING` → `CONFIRMED`/tidak memblokir; blok `DEC-INP-001` diberi kepala **TERTUTUP** |

Mentahnya dipertahankan dengan coretan, bukan dihapus, supaya jejak pertanyaan aslinya tetap
terbaca.

### 0.2 Perubahan struktur

| Gerakan | Yang dilakukan |
|---|---|
| ① Ekstrak | `02-module-map.md` lahir di tingkat modul: registry sub-modul, tabel kepemilikan data seluruh modul, peta butir menu + urutan migration, dan pemetaan kemampuan ke sub-modul |
| ② Pindahkan | Seluruh artefak revision `4` pindah ke `episode-rawat-inap/`, **termasuk `roadmap/` dan `task/`**. `erd/data-dictionary.md` → `data/data-dictionary.md` memenuhi `blueprint-output-contract.md`; 13 rujukan ke path lama diperbaiki |
| ③ Buat | `keperawatan/` dan `dokter-rawat-inap/`, masing-masing manifest + himpunan 11 berkas berisi satu baris alasan bersebab |

### 0.3 Yang diserap revision `4` — Amendment Pass 2026-08-24

| Keputusan | Sudah masuk ke |
|---|---|
| `RWI-DEC-070` pelonggaran mesin klinis meluas ke `Emergency` | Decision log: `RWI-RULE-026` aturan 3 s.d. 6 |
| `RWI-DEC-071` justifikasi `RWI-DEC-041` ditulis ulang | Decision log: `RWI-RULE-029` |
| `RWI-DEC-072` waktu tiba milik IGD | `episode-rawat-inap/` — `02-backend-architecture.md` §1.7 aturan 9 dan §4.5; `erd/00-context-erd.md`; `data/data-dictionary.md`; keempat kontrak; `testing/` bagian 2 dan 14 |
| `RWI-DEC-073` `OriginEncounterId` dikerjakan IGD | Field `compatibility_impact`; `episode-rawat-inap/02-backend-architecture.md`; `erd/00-context-erd.md` §2; `contracts/integration-contract.md` (`INT-INP-07`) |

Riwayat penyerapan revision `1` s.d. `3` ada pada bagian 10.

### 0.4 Satu artefak hulu yang masih tertinggal

[`evidence/03-hospital-domain-architecture.md`](./evidence/03-hospital-domain-architecture.md)
masih revision `0.1` dan belum memuat perubahan Amendment Pass 2026-08-21 maupun 2026-09-02.

| Bagian | Yang perlu diperbarui |
|---|---|
| `INV-INP-01` | Dilonggarkan untuk episode `DischargePending` yang kepergiannya sudah dicatat |
| Invariant baru `INV-INP-10` | Satu pasien satu episode yang benar-benar hadir |
| `CMD-INP-15` | Perintah bisnis baru: catat pasien sudah meninggalkan ruangan |
| `ARCH-GAP-002` s.d. `ARCH-GAP-005` | Keempatnya sudah tertutup `RWI-DEC-054` s.d. `RWI-DEC-057` |
| `CTX-CLI` dan `CTX-PHM` | Keduanya masih "Belum ditentukan, lihat `DEC-INP-001`". Sejak `RWI-DEC-081` pemiliknya **sudah** ditentukan: `ClinicalManagement` |

**Ini sengaja tidak diselesaikan di sini.** Bounded context, batas aggregate, invariant, dan
lifecycle adalah wewenang `/qv-domain`; skill penyusun blueprint dilarang merancangnya ulang.
Selisih ini **tidak memblokir** pemakaian blueprint, karena isi blueprint dan decision log sudah
sejalan.

---

## 1. Registry sub-modul

```yaml
blueprint_shape: COMPOSITE
shape_decided_by: USER_CONFIRMED
submodules:
  - slug: episode-rawat-inap
    prefix: BE-RWI / FE-RWI
    kemampuan: 16
    uji_pemecahan: 5/5
    status: approved
    approved_by: Muhammad Hamzah
    approved_at: 2026-08-24
    contract_versions: 0.4.0
  - slug: keperawatan
    prefix: BE-RWI / FE-RWI
    kemampuan: 5
    uji_pemecahan: 3/5
    status: approved
    approved_by: Muhammad Hamzah
    approved_at: 2026-09-03
    contract_versions: 0.3.0
    designed_at: 2026-09-02
    amended_at: 2026-09-02
    catatan: dirancang lalu diamandemen menyerap RWI-DEC-089; CAP-016 kini DEFERRED dan RWI-OQ-048
      tertutup. Roadmap DRAFT/FORWARD_TEST sudah ditulis 2026-09-02: 11 task backend dan 6 task
      frontend, seluruhnya BLOCKED menunggu approval blueprint sub-modul ini.
      Butir konsistensi mesin koreksi ditutup RWI-DEC-091. Disetujui RWI-DEC-092 pada 2026-09-03;
      roadmap kini DRAFT_STALE dan wajib ditulis ulang /qv-plan menjadi revision 2 berstatus APPROVED
      sebelum satu pun task dikirim ke builder
  - slug: dokter-rawat-inap
    prefix: BE-RWI / FE-RWI
    kemampuan: 7
    uji_pemecahan: 3/5
    status: approved
    approved_by: Muhammad Hamzah
    approved_at: 2026-09-03
    contract_versions: 0.3.0
    designed_at: 2026-09-02
    catatan: disetujui 2026-09-03 untuk 13 artefak revision 0.3 / kontrak 0.3.0; domain architecture READY; nol pertanyaan memblokir; approval desain BUKAN izin implementasi, migration, maupun deployment
```

| Sub-modul | Rumpun kemampuan | Kemampuan | Status | Manifest sub-modul |
|---|---|:---:|---|---|
| [`episode-rawat-inap/`](./episode-rawat-inap/) | Episode, tempat tidur, penanggung jawab, pemulangan, penutupan | 16 | `approved` | [manifest](./episode-rawat-inap/blueprint-manifest.md) |
| [`keperawatan/`](./keperawatan/) | Pengkajian, asuhan, tindakan keperawatan, gizi, pemakaian alat | 5 | **`approved`** — Muhammad Hamzah, 2026-09-03, lewat `RWI-DEC-092`. `CAP-016` `DEFERRED`; roadmap wajib ditulis ulang `/qv-plan` revision `2` | [manifest](./keperawatan/blueprint-manifest.md) |
| [`dokter-rawat-inap/`](./dokter-rawat-inap/) | SOAP, CPPT, kajian medis, resep, tindakan, visite, penunjang | 7 | **`approved`** — disetujui Muhammad Hamzah 2026-09-03; revision `0.3`, kontrak `0.3.0`; siap `plan-module-delivery` | [manifest](./dokter-rawat-inap/blueprint-manifest.md) |

### 1.1 Cara `status` modul diturunkan

`bentuk-blueprint.md` bagian 7 menyatakan status modul pada bentuk `COMPOSITE` **MUST NOT** ditulis
tangan. Aturannya:

| Keadaan baris registry | Status modul |
|---|---|
| Semua `approved` | `approved` |
| **Campur** | **`partial`** |
| Belum ada yang jalan | `draft` |

Hari ini: **tiga `approved` + nol `draft`** = **`approved`**. Menuliskannya tangan akan membuat modul
dapat terlihat `approved` sementara dua sub-modulnya belum dirancang sama sekali.

### 1.2 Dua sub-modul baru `draft`, bukan `BLOCKED`

`bentuk-blueprint.md` gerakan ③ menyatakan sub-modul yang batas kepemilikan datanya belum diputuskan
lahir `BLOCKED`. Kedua sub-modul baru **tidak** dalam keadaan itu: `RWI-DEC-081` sudah menetapkan
pemilik tabelnya (`ClinicalManagement`), `RWI-DEC-062` sudah memberikan persetujuannya, dan
`RWI-DEC-083` sudah memetakan kemampuannya. Yang tersisa adalah **pekerjaan desain**, ditambah satu
penghalang teknis — *shared inpatient clinical context resolver*, `PRD-RWI-FINAL-001` bagian 30.3.

`BLOCKED` berarti menunggu orang; `draft` berarti menunggu pekerjaan.

---

## 2. Daftar artefak tingkat modul dan hash

Hanya empat berkas ini yang hidup di tingkat modul. Artefak desain masing-masing sub-modul dicatat
pada manifest sub-modulnya.

| Artefak | Revision | Status | SHA-256 |
|---|---|---|---|
| [`00-interview-decisions.md`](./00-interview-decisions.md) | `14` | `draft / Amendment Pass approval Keperawatan` | `2684bfe93ac4338e12a2229fbef0d7378a93a3f175d91f8adf2dab70f210f030` |
| [`01-existing-capability-map.md`](./01-existing-capability-map.md) | `1.3` | `source-audited / focused-impact-scan` | `0155b345abea61f1b69e6adaf48ee91056b5efaf7fa672ea6300e0546bf4db03` |
| [`02-module-map.md`](./02-module-map.md) | `1` | `draft` — bagian dokter **diamendemen 2026-09-02**: gelombang migration M0, nama `CliPhysicianVisit`, Radiologi masuk MVP, dan satu butir menu ter-commit yang wajib dicabut | `29c761eed6a3fdc3a4d76c2803fde6e956a19784c4b3a14fc27d30e81e5a5d08` |
| [`evidence/02-requirement-completeness-gate.md`](./evidence/02-requirement-completeness-gate.md) | `1.4` | `CURRENT / focused keperawatan INP-S16` | `03bdcb31d8bb51478e2b9cf8a78392bd0c0b2e0ca5a60de79e566e5b9c5f2f9c` |
| [`evidence/03-hospital-domain-architecture.md`](./evidence/03-hospital-domain-architecture.md) | `0.2` | `draft / amendment Dokter Rawat Inap` | `226c6ef1e4bfec544c366b265fe1e4530e80c510da33c1a9eaf2e62161d0b717` |

`evidence/02-requirement-completeness-gate.md` naik dari `1.0` ke `1.1` karena tiga keterangan basi
`DEC-INP-001` diperbaiki. Isinya selebihnya tidak disentuh.

**Amandemen 2026-09-02 sore — penyerapan `RWI-DEC-089`.** `00-interview-decisions.md` naik ke revision `11` (Amendment Pass pemakaian alat), dan `02-module-map.md` berubah pada bagian 2.4, 4, serta 6 karena `CAP-016` berpindah dari `OPEN DECISION` menjadi `DEFERRED`. Delapan artefak `keperawatan/` ikut berubah dan `contract_versions` sub-modul itu naik ke `0.2.0`; tiga kontrak yang isinya tidak bergerak tetap `last_changed_in: 0.1.0`. Perubahan ini **tidak** menaikkan `revision` blueprint tingkat modul, karena tidak ada arsitektur target, kontrak lintas modul, atau kepemilikan data yang berubah — yang berubah adalah disposisi satu kemampuan yang sudah punya sub-modul pemilik.

Hash `02-module-map.md` dinormalkan pada validasi lifecycle 2026-09-02 dari nilai metadata lama
`62be6c…` ke SHA-256 isi committed `6be5e5…`. Pada hari yang sama berkasnya **benar-benar berubah**
karena amendment slice dokter, sehingga hashnya kini `d94b70…`. Perubahan itu menyentuh peta butir
menu dan urutan migration — dua hal yang memang hanya boleh hidup di berkas ini — dan tidak
menaikkan revision blueprint tingkat modul.

Hash dipakai mendeteksi perubahan yang tidak tercatat. Bila salah satu berubah tanpa revision naik,
blueprint dianggap tidak konsisten.

---

## 3. Struktur berkas

```text
rawat-inap/                              ◄── TINGKAT MODUL
├── blueprint-manifest.md                     berkas ini
├── 00-interview-decisions.md                 satu wawancara untuk seluruh modul
├── 01-existing-capability-map.md             satu audit untuk seluruh modul
├── 02-module-map.md                          hanya lahir pada COMPOSITE
├── evidence/                                 keluaran skill hulu
│
├── episode-rawat-inap/                  ◄── <blueprint-root> #1  approved
├── keperawatan/                         ◄── <blueprint-root> #2  draft
└── dokter-rawat-inap/                   ◄── <blueprint-root> #3  draft
```

Folder yang memuat `blueprint-manifest.md` adalah sub-modul; `evidence/` bukan.

| Hitungan | Nilai |
|---|---|
| Berkas pasti menurut kontrak | 4 tingkat modul + (11 × 3 sub-modul) = **37** |
| Yang ada hari ini | 4 tingkat modul + 3 manifest sub-modul + 33 himpunan + berkas di luar himpunan canonical |
| Berkas di luar himpunan canonical | `episode-rawat-inap/` — `05-skema-tampilan.md`, `erd/` (3), dua kontrak turunan, `archive/`, `roadmap/`, `task/report/` (77 laporan). Kehadirannya **bukan** penyimpangan struktur |
| Penyimpangan struktur yang diketahui | `episode-rawat-inap/flowcharts/` belum ada — dicatat pada manifest sub-modul itu bagian 4 |

---

## 4. Rantai masukan

```text
00-interview-decisions.md  rev 8  (grill-me: Scope + Closure + Amendment terdahulu + Amendment CAP-025 tuntas)
        |
01-existing-capability-map.md  (trace-existing-capabilities)
        |
evidence/02-requirement-completeness-gate.md  (PARTIALLY_READY)
        |
evidence/03-hospital-domain-architecture.md  (modul PARTIAL; scope dokter READY sejak revision 0.2)
        |
02-module-map.md  ◄── bentuk COMPOSITE; membagi 28 kemampuan ke tiga sub-modul
        |
        +--> episode-rawat-inap/  arsitektur + kontrak + PRD  →  roadmap  →  task
        +--> keperawatan/         dirancang, draft
        +--> dokter-rawat-inap/   domain architecture READY; artefak desain CURRENT rev 0.2, menunggu approval
```

Baseline requirement `PRD-RWI-FINAL-001` masuk pada tahap `00-interview-decisions.md` lewat
`RWI-DEC-080`, menggantikan batas scope revision `4`.

---

## 5. Design gate

Blueprint ini adalah desain target, bukan spesifikasi implementasi yang disetujui.

### 5.1 Gerbang implementasi

| Gate | Keadaannya | Menahan sub-modul |
|---|---|---|
| Persetujuan pemilik modul tetangga | **TERBUKA SEBAGIAN.** Dicabut 2026-08-21 oleh `RWI-DEC-062` untuk `ClinicalManagement`, `PharmacyManagement`, dan `MasterData` HealthServices. Bagian `EmergencyInstallationManagement` terbuka kembali 2026-08-24 lewat `RWI-DEC-069`; pemiliknya **Rizki Gunawan**, persetujuan formalnya belum tercatat | `episode-rawat-inap`, hanya `INP-S09` |
| Kesiapan data master | **MASIH TERBUKA.** Penanggung jawabnya `RWI-DEC-063`. Gerbang tertutup begitu datanya benar-benar terisi. Sejak revision `3` syaratnya bertambah: penanda jenis kelamin, isolasi, dan boks bayi harus **benar** | `episode-rawat-inap` |
| *Shared inpatient clinical context resolver* | Impact scan mengklasifikasikan `INT-DOK-01` sebagai **`Missing`** dan defect null-queue sebagai **`Repair`**. `INT-KEP-01` dan `INT-DOK-01` wajib dikerjakan bersama; amendment desain boleh berjalan, planning/build dokter belum boleh | `keperawatan`, `dokter-rawat-inap` |
| Pelonggaran batas satu konsultasi per kunjungan dan satu resep aktif | Impact scan mengklasifikasikan `INT-DOK-02` sebagai **`Extend`**. Scope wajib terbatas pada `Inpatient`/`Emergency` dan menjaga regresi `Outpatient`/`MCU` | `dokter-rawat-inap` |
| Consumer frontend Dokter Rawat Inap | **`Conflict`.** Route/menu ter-commit memakai hook, service, state, dan aksi antrean dokter rawat jalan; tidak membaca episode/census/DPJP dan melanggar keputusan layar anak tanpa menu tingkat dua | `dokter-rawat-inap`; menahan sign-off, planning frontend, dan rilis |
| Perbaikan tombol tempat tidur | Hari ini selalu gagal 404. `RWI-DEC-049`. Pekerjaan perbaikan, bukan keputusan | `episode-rawat-inap` |
| Test regresi modul tetangga | Tidak ada satu pun test yang menjaga jalur poliklinik, IGD, dan farmasi. `RWI-DEC-051`. Pekerjaan uji, bukan keputusan | Ketiganya |
| ~~Registry lifecycle~~ | **DICABUT** 2026-08-24 oleh `RWI-DEC-068` | — |

### 5.2 Gerbang sebelum produksi — klinis dan privasi

| Gate | Keadaannya |
|---|---|
| Clinical governance owner | **Sebagian terisi.** Keputusan isolasi dan jenis kelamin diambil Muhammad Hamzah lewat `RWI-DEC-064`; belum dinyatakan apakah penunjukan itu mencakup seluruh peran clinical governance |
| Security/privacy owner | Belum ditunjuk |
| `RWI-RULE-021` batas waktu klinis | Gerbang keras. Masih menunggu pemilik klinis. Kini menahan `keperawatan` dan `dokter-rawat-inap` |
| `RWI-RULE-025` persetujuan umum | Gerbang keras. `DEC-INP-003` |
| Masa simpan riwayat | `RWI-OQ-035`, keputusan hukum. Sudah dijawab `RWI-DEC-060`, menunggu pemilik hukum |
| ~~`RWI-RULE-012` isolasi dan jenis kelamin~~ | **BERUBAH BENTUK** 2026-08-21. Aturannya final lewat `RWI-DEC-064` s.d. `RWI-DEC-066`, dirancang sebagai `EPIC RI-34` |

---

## 6. Butir terbuka tingkat modul

| Butir | Isinya | Yang ditahannya |
|---|---|---|
| `RWI-OQ-047` | Sumber kebenaran *Financial Clearance*: `PRD-RWI-FINAL-001` bagian 23.1 menaruhnya pada Billing Management, sedangkan `RWI-RULE-028` aturan 7 memilikinya **sementara** lewat `InpFinancialClearance` | **Satu baris** pada `02-module-map.md` bagian 2.4. Tidak menahan desain, tidak menahan task berjalan |
| `RWI-OQ-034` | Persetujuan pemilik `EmergencyInstallationManagement` | `INP-S09` saja |
| `RWI-OQ-035`, `038`, `039` | Sudah dijawab, menunggu pemilik klinis atau hukum | Gerbang produksi |
| `RWI-OQ-045`, `046` | Keputusan implementasi nonblocking | Tidak menahan apa pun |
| Butir menu dua sub-modul baru | Kuota sembilan butir `IA-INP-05` sudah penuh dipakai `episode-rawat-inap` | Ditetapkan saat kedua sub-modul dirancang |

---

## 7. Yang tidak boleh diubah blueprint hilir

Perubahan pada butir berikut wajib kembali ke skill hulu, bukan diselesaikan pada tahap perencanaan
atau implementasi:

1. Kepemilikan data pada [`02-module-map.md`](./02-module-map.md) bagian 2.
2. **Baru revision `5`:** larangan `keperawatan` dan `dokter-rawat-inap` membuat tabel tandingan
   untuk dokumentasi klinis — `RWI-DEC-081`.
3. **Baru revision `5`:** pemetaan 28 kemampuan ke sub-modul — `RWI-DEC-083`. Memindahkan sebuah
   kemampuan antar sub-modul adalah keputusan pemilik, bukan kerapian struktur.
4. Kedudukan `MstBed.BedStatus` sebagai **salinan**, bukan sumber kebenaran.
5. Sepuluh invariant `INV-INP-01` sampai `INV-INP-10` beserta cara menjaganya.
6. Bentuk **berperiode** pada `InpDoctorAssignment`, `InpNurseAssignment`, dan `InpBedPlacement`.
7. Kedudukan `InpCorrectionSession` sebagai konsep tersendiri, bukan status episode keenam.
8. Kebutuhan isolasi sebagai **atribut episode**, bukan atribut pasien dan bukan status.
9. Aturan pencampuran kamar diperiksa dari **penghuni yang sedang ada**, bukan dari penanda pada
   `MstRoom`.

---

## 8. Pemicu impact scan

| Yang berubah | Yang harus ditinjau ulang |
|---|---|
| `backend_commit_sha` atau `frontend_commit_sha` | Capability map lebih dulu, lalu seluruh kontrak setiap sub-modul |
| `Areas/HealthServices/MasterData/` tempat tidur, kamar, unit layanan, kelas | `episode-rawat-inap/` — `erd/`, `contracts/api-contract.md`, `EPIC RI-22`, `RI-23`, `RI-32`, `RI-34` |
| `Areas/HealthServices/RegistrationManagement/` | `INV-INP-04`, `EPIC RI-21` |
| `Areas/HealthServices/ClinicalManagement/` atau `PharmacyManagement/` | **`keperawatan/` dan `dokter-rawat-inap/` seluruhnya.** Sejak `RWI-DEC-081` kedua modul itu adalah **pemilik tabel** kedua sub-modul tersebut, bukan lagi sekadar tetangga yang menahan slice |
| `Areas/HealthServices/BillingManagement/` | `RWI-RULE-028` aturan 7 dan **`RWI-OQ-047`**; sumber kelayakan keuangan mungkin berpindah |
| `Repositories/ApplicationDbContext.cs` | Rencana migration, `02-module-map.md` bagian 3.4 |
| `agents/rules/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Prefix dan lifecycle modul |
| Munculnya berkas berawalan `Inp` | Seluruh status `Baru` wajib dinilai ulang |
| `PRD_Final_Rawat_Inap_100_Persen.md` | Pemetaan 28 kemampuan pada `02-module-map.md` bagian 4 |

### 8.1 Hasil impact scan terfokus terbaru

Impact scan 2026-09-02 pada backend `93b3227c431401d8f586dec4e1fb25fbf41766e3` dan frontend
`863f24b0d1617069310c04e5770b47fd1b518b5b` selesai **hanya untuk** Dokter Rawat Inap. Snapshot
revision `5` di metadata sengaja tidak diganti karena sub-modul lain belum dipindai ulang; SHA
terbaru dicatat terpisah agar snapshot desain dan bukti audit tidak tercampur.

| Scope | Keadaan | Konsekuensi lifecycle |
|---|---|---|
| [`01-existing-capability-map.md`](./01-existing-capability-map.md) bagian 15 | `CURRENT` untuk `CAP-015`, `CAP-020`–`CAP-025`, `INT-DOK-01`, `INT-DOK-02`, frontend, authorization, dan test | Menjadi input kanonis amendment |
| `02-module-map.md` bagian Dokter Rawat Inap | **`CURRENT`** sejak 2026-09-02 | Pernyataan Radiologi, nama entity visite, dan urutan migration sudah diperbaiki |
| Seluruh artefak `dokter-rawat-inap/` | **`approved`** pada revision `0.3` | Boleh dipakai `plan-module-delivery`. Perubahan sesudah ini membuat revision baru dan memicu impact scan kedua repository |
| Status sub-modul / modul | Tetap `draft` / `partial` | Staleness artefak tidak mengubah aturan derivasi status komposit |

---

## 9. Riwayat revision

| Revision | Tanggal | Ringkasan |
|---|---|---|
| `5` | 2026-09-02 | **Migrasi bentuk `SINGLE` → `COMPOSITE`.** Menyerap `RWI-DEC-080` s.d. `RWI-DEC-083`. `PRD-RWI-FINAL-001` menggantikan batas scope revision `4`; modul menjadi 28 kemampuan. Tiga sub-modul lahir: `episode-rawat-inap` (16 kemampuan, `approved`), `keperawatan` (5, `draft`), `dokter-rawat-inap` (7, `draft`). `02-module-map.md` lahir; tabel kepemilikan data, peta butir menu, dan urutan migration lintas sub-modul naik ke sana. Kamus data pindah `erd/` → `data/`. Tiga dokumen basi `DEC-INP-001` diperbaiki. Status modul **diturunkan** menjadi `partial`. **Nol tabel baru, nol kolom baru, nol endpoint baru, nol kontrak naik versi** |
| `4` | 2026-08-24 | **Disetujui Muhammad Hamzah lewat `RWI-DEC-074`.** Menyerap empat keputusan Amendment Pass dari tiga usulan lintas modul milik blueprint IGD. Kelayakan Penempatan tumbuh menjadi sembilan aturan; dua integrasi arah baca baru; `InpBedPlacement.StartDateTime` berubah asal nilainya untuk jalur serah terima; sepuluh acceptance criteria baru. Nol tabel, kolom, dan endpoint baru |
| `3` | 2026-08-21 | Menyerap tiga keputusan penutupan butir organisasi. Satu epic baru `EPIC RI-34` beserta 9 functional requirement, 5 skenario UAT, 26 skenario acceptance test. Enam kolom baru pada `InpEpisode`, satu enum, dua endpoint, satu penjaga service, satu daftar pantau, satu layar. `INP-S11` berpindah dari slice yang dihentikan menjadi slice yang dirancang |
| `2` | 2026-08-21 | Menyerap empat keputusan Amendment Pass. Satu tabel baru `InpDischargeSummaryRevision`, tiga kolom baru, satu nilai enum baru, satu endpoint baru, satu invariant baru `INV-INP-10`, `INV-INP-01` dilonggarkan |
| `1` | 2026-08-21 | Blueprint pertama. Dua bounded context, satu aggregate root, dua belas tabel baru, nol perubahan kolom pada tabel modul lain. Sembilan slice dirancang, delapan sengaja dihentikan. 13 epic, 47 functional requirement, 23 skenario UAT, 82 skenario acceptance test |

---

## 10. Langkah berikutnya

| Kondisi | Skill | Untuk sub-modul |
|---|---|---|
| Empat pertanyaan memblokir pada `04-prd-to-mvp.md` bagian 20.2 terjawab dan owner menyetujui | `/qv-plan` | `episode-rawat-inap` |
| ~~Focused requirement gate Dokter Rawat Inap selesai~~ | ~~`hospital-domain-architect` amendment~~ | **SELESAI 2026-09-02.** Hasilnya `DOMAIN_ARCHITECTURE_READY` untuk ketujuh capability, pada `evidence/03-hospital-domain-architecture.md` Bagian Kedua |
| ~~Domain amendment ketujuh capability siap~~ | ~~`design-business-module` amendment~~ | **SELESAI 2026-09-02**, dua putaran. Revision `0.2` menyerap arsitektur domain; revision `0.3` menyerap `RWI-DEC-086` s.d. `RWI-DEC-088` |
| **Owner menyetujui revision `0.2` dan pertanyaan memblokir dijawab** | `plan-module-delivery` | `dokter-rawat-inap`; pecah menjadi task backend dan frontend berbasis vertical slice |
| `RWI-OQ-047` ingin ditutup | `/qv-grill` Amendment Pass | Tingkat modul |
| Salah satu SHA berubah | `/qv-trace` impact scan | Seluruhnya |
| Batas domain dokumentasi klinis ingin ditetapkan lebih dulu | `/qv-domain` (opsional) | `keperawatan`, `dokter-rawat-inap` |

**Sub-modul `dokter-rawat-inap` sudah `approved` sejak 2026-09-03** dan boleh diteruskan ke
`plan-module-delivery`. `keperawatan` tetap `draft` dan dinilai dari manifestnya sendiri; status
modul karena itu tetap `partial`, diturunkan dari baris sub-modul dan bukan ditulis tangan.

**Approval itu menyetujui desain, bukan memberi izin menulis source.** Wewenang implementasi,
migration, dan deployment tetap terpisah, sebagaimana registry kepemilikan prefix juga hanya memberi
hak penamaan. Gerbang produksi pada `dokter-rawat-inap/blueprint-manifest.md` bagian 6 tetap
berlaku apa adanya.
