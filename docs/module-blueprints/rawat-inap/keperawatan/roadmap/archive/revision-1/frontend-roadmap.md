# Roadmap Delivery Frontend — Sub-modul Keperawatan Rawat Inap

> ## ⚠ ROADMAP INI **`STALE`** SEJAK 2026-09-02 SORE
>
> `RWI-DEC-091` mengubah bentuk koreksi dokumen keperawatan, dan `03-frontend-architecture.md`
> sudah naik ke revision `0.2`. Dua task berubah bentuk:
>
> | Task | Yang berubah |
> | --- | --- |
> | `FE-RWI-052` | Dialog **amandemen** menjadi dialog **tambah koreksi**. Isi asli tetap tampil apa adanya; koreksi muncul sebagai baris addendum bernomor urut di bawahnya |
> | `FE-RWI-055` | Sama, untuk catatan tindakan |
>
> `FE-RWI-054` rencana asuhan **tidak berubah** — perubahan rencana tetap berversi, bukan addendum.

## Metadata

```yaml
module_id: rawat-inap
module_name: InPatientManagement
blueprint_id: RWI-BP-001
blueprint_revision: 5
blueprint_shape: COMPOSITE
submodule: keperawatan
blueprint_root: docs/module-blueprints/rawat-inap/keperawatan/
roadmap_revision: 1
status: DRAFT_STALE
roadmap_mode: FORWARD_TEST
approval_gate: BLUEPRINT_NOT_YET_APPROVED
approved_by: null
approved_at: null
owners:
  - "Product/Domain: Muhammad Hamzah (RWI-DEC-061)"
  - "Frontend authority: sesuai decision log; IA-INP-01 s.d. IA-INP-05 mengikat"
  - "Security/Privacy: OPEN"
contract_versions: 0.2.0
input_revisions:
  blueprint-manifest.md (sub-modul): 5
  00-interview-decisions.md: 11
  02-module-map.md: 1
  03-frontend-architecture.md: 0.1
  04-prd-to-mvp.md: 0.2
backend_source_sha: 93b3227c431401d8f586dec4e1fb25fbf41766e3
frontend_source_sha: 8d6e0998c16d60e19a9f43949758e8895d5c47d0
task_id_range: FE-RWI-051 .. FE-RWI-056
screens: 6
new_menu_items: 0
```

---

## 0. Peringatan yang tidak boleh dilewati

### 0.1 Roadmap ini **belum boleh dieksekusi siapa pun**

Blueprint sub-modul `keperawatan` berstatus **`draft`** dan belum pernah disetujui pemiliknya.
Seluruh task di bawah ini berstatus `BLOCKED` pada gerbang yang sama, dan **tidak satu pun boleh
dikirim ke `/qv-fe`**. Penjelasan lengkapnya ada pada
[`backend-roadmap.md`](./backend-roadmap.md) bagian 0.1.

### 0.2 Nol butir menu baru

`02-module-map.md` bagian 3 mencatat kuota sembilan butir menu `IA-INP-05` **sudah penuh dipakai**
`episode-rawat-inap`. Keputusan 2026-09-02 menetapkan sub-modul ini mendapat **nol butir menu
tingkat dua**; keenam layarnya menjadi **layar anak**.

| Layar | Jalan masuknya | Butir hak akses penjaga |
| --- | --- | --- |
| `FE-KEP-01` Ruang Kerja Keperawatan | `FE-INP-04` Detail Episode, dan baris pasien pada `FE-INP-01` Census | `PatientAssessment : Read` |
| `FE-KEP-02` Pengkajian | `FE-KEP-01` | `PatientAssessment : Create` / `Read` |
| `FE-KEP-03` Lini Masa | `FE-KEP-01` | `PatientAssessment : Read` |
| `FE-KEP-04` Rencana Asuhan | `FE-KEP-01` | `NursingCarePlan : Read` / `Create` / `Update` |
| `FE-KEP-05` Catatan Tindakan | `FE-KEP-01` | `NursingIntervention : Read` / `Create` |
| `FE-KEP-06` Daftar Pantau Kepatuhan | `FE-INP-09` Daftar Pantau, sebagai **daftar ketiga** | `PatientAssessment : Read` |

Menambahkan butir menu baru di sini **melanggar** keputusan itu dan merusak `IA-INP-05`.

### 0.3 Setiap layar wajib punya backend-nya lebih dulu

Seluruh endpoint yang dipakai keenam layar berstatus **`Rencana (belum tersedia)`** kecuali tiga
endpoint baca pengkajian yang sudah ada. Task frontend **MUST NOT** dimulai sebelum task backend
pasangannya selesai dan endpoint-nya benar-benar dapat dipanggil.

Pelajaran ini mahal dan sudah pernah terjadi di modul ini: pembahasan ulang arsitektur frontend
2026-08-27 menemukan **sembilan operasi HTTP** yang sudah jadi tetapi tidak pernah dipanggil satu
layar pun, dan satu layar yang tidak dapat dicapai siapa pun.

### 0.4 Yang tetap `DEV_DISCRETION`

Warna, jarak, ikon, pilihan component library, dan tata letak di dalam wilayah layar **tidak**
dikunci roadmap ini. Yang dikunci hanya **isi, sumber data, keterjangkauan, dan butir hak akses per
tombol** — sesuai `03-frontend-architecture.md` bagian 3.

---

## 1. Gelombang dan urutan dependency

| Gelombang | Task | Layar | Prasyarat backend |
| --- | --- | --- | --- |
| **`KEP-MVP-1`** | `FE-RWI-051` | `FE-KEP-01` Ruang Kerja | `BE-RWI-054` |
| **`KEP-MVP-1`** | `FE-RWI-052` | `FE-KEP-02` Pengkajian | `BE-RWI-056`, `BE-RWI-057` |
| **`KEP-MVP-1`** | `FE-RWI-053` | `FE-KEP-03` Lini Masa | `BE-RWI-058` |
| **`KEP-MVP-2`** | `FE-RWI-054` | `FE-KEP-04` Rencana Asuhan | `BE-RWI-059`, `BE-RWI-060` |
| **`KEP-MVP-3`** | `FE-RWI-055` | `FE-KEP-05` Catatan Tindakan | `BE-RWI-061`, `BE-RWI-062` |
| **`KEP-MVP-4`** | `FE-RWI-056` | `FE-KEP-06` Daftar Pantau | `BE-RWI-064` |

```text
FE-RWI-051 ─┬─> FE-RWI-052 ─> FE-RWI-053
            ├─> FE-RWI-054
            └─> FE-RWI-055

FE-RWI-056  (berdiri sendiri, menempel pada FE-INP-09)
```

---

## 2. Task

### ⛔ `FE-RWI-051` — Perawat punya satu tempat kerja untuk satu pasien

| Field | Isi |
| --- | --- |
| **Status** | ⛔ `BLOCKED` — gerbang approval bagian 0.1 |
| **Outcome** | Perawat membuka satu halaman dan melihat seluruh dokumentasi pasien rawat inap yang menjadi tanggung jawabnya, tanpa berpindah-pindah menu |
| **Trace** | `FE-KEP-01`; `03-frontend-architecture.md` bagian 3.1; `IA-INP-01` tiga klik dari Beranda |
| **Kontrak** | `contracts/api-contract.md` `0.2.0` grup Patient Assessment; konteks episode lewat `INT-KEP-02` |
| **Reuse** | Base component dan design token Quilvian yang sudah ada; pola detail episode `FE-INP-04`. **Keputusan reuse atau buat baru diambil saat eksekusi berdasarkan bukti source**, bukan diputuskan di sini |
| **Scope** | Route `…/episodes/{id}/nursing`; kerangka ruang kerja; kepala konteks pasien dan episode; jalan masuk ke `FE-KEP-02` s.d. `FE-KEP-05`; jalan masuk dari `FE-INP-04` dan baris census `FE-INP-01` |
| **Dependency** | `BE-RWI-054` |
| **Acceptance criteria** | 1. Layar tercapai dari Beranda dalam paling banyak **tiga klik** lewat Census. 2. Kepala konteks menampilkan pasien, episode, ruangan, dan DPJP dari data episode yang sebenarnya. 3. Tanpa `PatientAssessment : Read`, layar tidak dapat dibuka. 4. **Nol butir menu baru** ditambahkan |
| **Verification** | Telusur tiga klik dari Beranda; pemeriksaan sidebar sebelum dan sesudah; uji tanpa hak akses |
| **Risk/blocker** | Menambahkan butir menu tingkat dua akan melanggar `IA-INP-05`. Owner: Frontend authority |
| **DoD** | Route, kerangka layar, kepala konteks, empat jalan masuk anak, dua jalan masuk induk; nol butir menu baru; lint lulus; build lulus |

---

### ⛔ `FE-RWI-052` — Perawat mengisi, menyelesaikan, dan membetulkan pengkajian

| Field | Isi |
| --- | --- |
| **Status** | ⛔ `BLOCKED` — gerbang approval bagian 0.1 |
| **Outcome** | Perawat mengisi pengkajian awal dan pengkajian ulang di sistem, dan dapat membetulkan yang sudah final lewat amandemen beralasan |
| **Trace** | `FE-KEP-02`; `FR-KEP-005`, `FR-KEP-006`, `FR-KEP-008`, `FR-KEP-009`; `03-frontend-architecture.md` bagian 3.2 |
| **Kontrak** | `contracts/api-contract.md` `0.2.0` `POST /`, `GET /{id}`, `PATCH /{id}/amend`; `contracts/validation-matrix.md` `0.2.0` |
| **Reuse** | Pola formulir bertahap yang dipakai alur admisi `FE-INP-16` s.d. `FE-INP-19` |
| **Scope** | Formulir pengkajian awal dan ulang; tombol Selesaikan; dialog amandemen dengan alasan wajib; penanda jenis pengkajian; penyajian pesan penolakan dari `validation-matrix` |
| **Dependency** | `FE-RWI-051`, `BE-RWI-056`, `BE-RWI-057` |
| **Acceptance criteria** | 1. Percobaan membuat pengkajian awal **kedua** menampilkan pesan yang mengarahkan ke pengkajian ulang, bukan pesan teknis. 2. Amandemen tanpa alasan ditolak di layar sebelum dikirim. 3. Pengkajian final tidak menampilkan tombol sunting langsung. 4. Pengiriman ganda tidak menghasilkan dua pengkajian |
| **Verification** | Skenario pengkajian awal kedua; skenario amandemen tanpa alasan; uji tekan tombol dua kali |
| **Risk/blocker** | Bila butir konsistensi `04-prd-to-mvp.md` bagian 20.1 dijawab berbeda, bentuk dialog koreksinya berubah. Owner: Muhammad Hamzah |
| **DoD** | Formulir dua jenis, dialog amandemen, penanganan empat keadaan, pesan sesuai validation matrix; lint lulus; build lulus |

---

### ⛔ `FE-RWI-053` — Perkembangan pasien terbaca sebagai garis waktu, bukan angka terakhir

| Field | Isi |
| --- | --- |
| **Status** | ⛔ `BLOCKED` — gerbang approval bagian 0.1 |
| **Outcome** | Perawat dan DPJP melihat apakah nyeri pasien membaik atau memburuk sejak masuk, langsung dari satu layar |
| **Trace** | `FE-KEP-03`; `FR-KEP-007`; `AC-CAP012-02`; `03-frontend-architecture.md` bagian 3.3 |
| **Kontrak** | `contracts/api-contract.md` `0.2.0` `GET /episodes/{episodeId}/timeline` dan `GET /episodes/{episodeId}/due-status` |
| **Reuse** | Pola tabel dan penyajian data Quilvian yang sudah ada |
| **Scope** | Lini masa per jenis pengukuran: nyeri, risiko jatuh, gizi; penanda keadaan tenggat; keadaan kosong yang berbunyi jelas |
| **Dependency** | `FE-RWI-051`, `BE-RWI-058` |
| **Acceptance criteria** | 1. Seluruh pengukuran tampil terurut waktu, bukan hanya yang terakhir. 2. Keadaan kosong membedakan "belum ada pengkajian" dari "tidak dapat dimuat". 3. Ketika master kebijakan kosong, layar berbunyi "tidak dipantau", **bukan** "terlambat" |
| **Verification** | Skenario tiga pengukuran; skenario master kosong; skenario gagal memuat |
| **Risk/blocker** | Menyamakan "kosong" dengan "gagal" menyesatkan pembaca klinis. Owner: Frontend authority |
| **DoD** | Lini masa tiga jenis pengukuran, penanda tenggat, tiga keadaan terbedakan; lint lulus; build lulus |

---

### ⛔ `FE-RWI-054` — Perawat menetapkan masalah, tujuan, dan evaluasi asuhan

| Field | Isi |
| --- | --- |
| **Status** | ⛔ `BLOCKED` — gerbang approval bagian 0.1 |
| **Outcome** | Rencana asuhan keperawatan tersusun di sistem beserta riwayat perubahannya, menggantikan catatan kertas |
| **Trace** | `FE-KEP-04`; `FR-KEP-012` s.d. `FR-KEP-017`; `03-frontend-architecture.md` bagian 3.4 |
| **Kontrak** | `contracts/api-contract.md` `0.2.0` grup Nursing Care Plan, tujuh endpoint |
| **Reuse** | Pola daftar induk-anak yang sudah dipakai layar penempatan tempat tidur |
| **Scope** | Daftar butir masalah; formulir tambah dan ubah butir; dialog evaluasi; dialog tutup butir dengan alasan wajib; panel riwayat versi; keadaan hanya-baca ketika episode `Closed` |
| **Dependency** | `FE-RWI-051`, `BE-RWI-059`, `BE-RWI-060` |
| **Acceptance criteria** | 1. Butir dapat dinyatakan tercapai hanya bila evaluasinya sudah diisi; tombolnya nonaktif sebelum itu. 2. Riwayat versi menampilkan **penulis dan waktu asli** tiap versi. 3. Pada episode `Closed`, seluruh tombol ubah hilang dan riwayat tetap terbaca |
| **Verification** | Skenario tutup butir tanpa evaluasi; pemeriksaan penulis pada riwayat versi; skenario episode tertutup |
| **Risk/blocker** | Masalah keperawatan ditulis sebagai teks sampai katalog SDKI diputuskan; layar **tidak boleh** mengunci bentuknya ke katalog yang belum ada. Owner: Clinical governance |
| **DoD** | Daftar butir, tiga dialog, panel riwayat, keadaan hanya-baca; lint lulus; build lulus |

---

### ⛔ `FE-RWI-055` — Perawat mencatat tindakan yang sudah dilakukan

| Field | Isi |
| --- | --- |
| **Status** | ⛔ `BLOCKED` — gerbang approval bagian 0.1 |
| **Outcome** | Tindakan keperawatan tercatat beserta waktu, pelaku, dan hasilnya. Kegagalan tagihan terlihat sebagai keadaan tersendiri yang tidak menghapus catatan klinis |
| **Trace** | `FE-KEP-05`; `FR-KEP-018` s.d. `FR-KEP-022`; `AC-CAP014-01`, `AC-CAP014-02`; `03-frontend-architecture.md` bagian 3.5 |
| **Kontrak** | `contracts/api-contract.md` `0.2.0` grup Nursing Intervention, lima endpoint |
| **Reuse** | Pola pencegahan pengiriman ganda yang sudah dipakai alur admisi |
| **Scope** | Formulir catat tindakan; daftar tindakan per episode terurut waktu; tombol Finalkan; dialog amandemen beralasan; penanda keadaan pengiriman tagihan |
| **Dependency** | `FE-RWI-051`, `BE-RWI-061`, `BE-RWI-062` |
| **Acceptance criteria** | 1. Menekan tombol simpan dua kali menghasilkan **satu** baris tindakan. 2. Tindakan mendadak dapat dicatat tanpa memilih butir rencana asuhan. 3. Ketika pengiriman tagihan gagal, catatan klinis **tetap tampil** dan penanda kegagalannya terpisah. 4. Bagi pengguna yang bukan penulis dan bukan kepala ruangan, tombol amandemen tidak tersedia |
| **Verification** | Uji tekan ganda; skenario tanpa rencana asuhan; skenario tagihan gagal; uji dua peran berbeda |
| **Risk/blocker** | Menampilkan kegagalan tagihan sebagai kegagalan pencatatan klinis akan membuat perawat mengulang tindakan yang sudah tersimpan. Owner: Frontend authority |
| **DoD** | Formulir, daftar, tombol finalkan, dialog amandemen, penanda tagihan; empat skenario lulus; lint lulus; build lulus |

---

### ⛔ `FE-RWI-056` — Kepala ruangan menemukan pengkajian yang tertinggal

| Field | Isi |
| --- | --- |
| **Status** | ⛔ `BLOCKED` — gerbang approval bagian 0.1 |
| **Outcome** | Daftar pantau **ketiga** akhirnya ada. `RWI-RULE-023` menuntutnya sejak awal, dan roadmap `episode-rawat-inap` mencatatnya sebagai gap yang tertahan karena bergantung pada dokumentasi klinis |
| **Trace** | `FE-KEP-06`; `FR-KEP-024`, `FR-KEP-025`, `FR-KEP-026`; `RWI-RULE-023`, `RWI-DEC-032`; `03-frontend-architecture.md` bagian 3.6 |
| **Kontrak** | `contracts/api-contract.md` `0.2.0` endpoint daftar pantau kepatuhan |
| **Reuse** | **Layar `FE-INP-09` Daftar Pantau yang sudah ada.** Task ini menambah satu daftar ke dalamnya, **bukan** membuat layar baru |
| **Scope** | Satu daftar tambahan pada `FE-INP-09`; penyaringan ruangan dan keadaan tenggat; keadaan kosong yang berbunyi benar |
| **Dependency** | `BE-RWI-064` |
| **Acceptance criteria** | 1. Daftar muncul sebagai daftar **ketiga** pada `FE-INP-09`, bukan sebagai layar atau butir menu baru. 2. Daftar kosong berbunyi **"sudah tepat waktu"**, bukan "tidak ada data". 3. Layar tercapai dari Beranda dalam **dua klik**. 4. Tidak ada tombol pada daftar ini yang menahan pekerjaan klinis mana pun |
| **Verification** | Telusur dua klik; skenario daftar kosong; pemeriksaan urutan daftar pada `FE-INP-09` |
| **Risk/blocker** | Urutan daftar di dalam `FE-INP-09` kini dipakai **tiga** sub-modul dan **tidak boleh** diputuskan sendiri-sendiri — `02-module-map.md` bagian 6. Owner: Frontend authority bersama pemilik `episode-rawat-inap` |
| **DoD** | Satu daftar tambahan, penyaringan, keadaan kosong berbunyi benar, nol layar baru, nol butir menu baru; lint lulus; build lulus |

---

## 3. Coverage gap requirement ke test

| Requirement | Layar pemilik | Task | Catatan |
| --- | --- | --- | --- |
| `FR-KEP-001` s.d. `FR-KEP-004` | `FE-KEP-01` | `FE-RWI-051` | Pintu masuk; dibuktikan telusur tiga klik |
| `FR-KEP-005` s.d. `FR-KEP-011` | `FE-KEP-02`, `FE-KEP-03` | `FE-RWI-052`, `FE-RWI-053` | Lengkap |
| `FR-KEP-012` s.d. `FR-KEP-017` | `FE-KEP-04` | `FE-RWI-054` | Lengkap |
| `FR-KEP-018` s.d. `FR-KEP-023` | `FE-KEP-05` | `FE-RWI-055` | `FR-KEP-023` CPPT dibuktikan di backend; frontend hanya membacanya |
| `FR-KEP-024` s.d. `FR-KEP-026` | `FE-KEP-06` | `FE-RWI-056` | Lengkap |
| `FR-KEP-027`, `FR-KEP-028` | — | **Tidak ada, disengaja** | `EPIC KEP-06` `DEFERRED` oleh `RWI-DEC-089` |

**Nol layar tanpa task, dan nol task tanpa layar.** Setiap layar pada
`03-frontend-architecture.md` bagian 1 punya tepat satu task pemilik, dan setiap layar sudah
dinyatakan sebagai layar anak beserta induknya pada bagian 0.2 — sehingga tidak ada layar yang
tidak dapat dicapai siapa pun.
