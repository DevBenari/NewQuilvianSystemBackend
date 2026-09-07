# Roadmap Delivery Frontend — Sub-modul Keperawatan Rawat Inap

> ## ✅ ROADMAP INI **BOLEH DIEKSEKUSI** SEJAK 5 SEPTEMBER 2026
>
> Revision `2` menggantikan revision `1` yang berstatus `DRAFT_STALE`. Dua hal berubah:
>
> | Yang berubah | Pada revision `1` | Menjadi revision `2` |
> | --- | --- | --- |
> | Gerbang approval | Keenam task `BLOCKED` karena blueprint belum disetujui | **Approval sudah turun** lewat `RWI-DEC-092`, Muhammad Hamzah, 3 September 2026 |
> | Bentuk koreksi di layar | Dialog **amandemen** yang menghasilkan versi baru | Dialog **tambah koreksi**. Isi asli tetap tampil apa adanya; koreksi muncul sebagai addendum bernomor urut di bawahnya — `RWI-DEC-091` |
>
> `FE-RWI-054` Rencana Asuhan **tidak berubah**: perubahan rencana asuhan tetap berversi, bukan
> beraddendum, karena ia perkembangan klinis dan bukan pembetulan kesalahan.
>
> Revision `1` disimpan apa adanya di [`archive/revision-1/`](./archive/revision-1/frontend-roadmap.md).

## Metadata

```yaml
module_id: rawat-inap
module_name: InPatientManagement
blueprint_id: RWI-BP-001
blueprint_revision: 5
blueprint_shape: COMPOSITE
submodule: keperawatan
blueprint_root: docs/module-blueprints/rawat-inap/keperawatan/
roadmap_revision: 2
status: APPROVED
roadmap_mode: DELIVERY
approval_gate: BLUEPRINT_APPROVED
approved_by: "Muhammad Hamzah - Product/Domain owner (RWI-DEC-061)"
approved_at: "2026-09-03"
approval_decision: RWI-DEC-092
owners:
  - "Product/Domain: Muhammad Hamzah (RWI-DEC-061)"
  - "Frontend authority: sesuai decision log; IA-INP-01 s.d. IA-INP-05 mengikat"
  - "Security/Privacy: OPEN"
contract_versions: 0.3.0
input_revisions:
  blueprint-manifest.md (sub-modul): 5
  00-interview-decisions.md: 13
  02-module-map.md: 1
  03-frontend-architecture.md: 0.2
  04-prd-to-mvp.md: 0.3
planning_source_sha:
  backend: 7d4bf2b91d39265eab4453a230ba324f95866962 (branch MHamzah, dibaca 5 September 2026)
  frontend: eb505a9ea20d99505a69ff0e6ea428ec9a551dc5 (branch HamzahV2, dibaca 5 September 2026)
task_id_range: FE-RWI-051 .. FE-RWI-056
task_count: 6
screens: 6
new_menu_items: 0
```

---

## 0. Lima peringatan yang tidak boleh dilewati

### 0.1 Gerbang approval sudah dicabut

Blueprint sub-modul `keperawatan` berstatus **`approved`** sejak 3 September 2026 lewat
`RWI-DEC-092`. Keenam task di bawah **boleh dikirim ke `/qv-fe`**, dengan satu syarat mutlak yang
dijelaskan bagian 0.3. Penjelasan lengkap tentang apa yang ikut dan tidak ikut tercabut ada pada
[`backend-roadmap.md`](./backend-roadmap.md) bagian 0.1.

### 0.2 Nol butir menu baru

`02-module-map.md` bagian 3 mencatat kuota sembilan butir menu `IA-INP-05` **sudah penuh dipakai**
`episode-rawat-inap`. Keputusan 2 September 2026 menetapkan sub-modul ini mendapat **nol butir menu
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

Approval blueprint **tidak** mengubah aturan ini. Seluruh endpoint yang dipakai keenam layar
berstatus **`Rencana (belum tersedia)`** kecuali tiga endpoint baca pengkajian yang sudah ada di
source. Task frontend **MUST NOT** dimulai sebelum task backend pasangannya selesai dan
endpoint-nya benar-benar dapat dipanggil.

Pelajaran ini mahal dan sudah pernah terjadi di modul ini: pembahasan ulang arsitektur frontend
27 Agustus 2026 menemukan **sembilan operasi HTTP** yang sudah jadi tetapi tidak pernah dipanggil
satu layar pun, dan satu layar yang tidak dapat dicapai siapa pun.

Endpoint yang **sudah tersedia** di `BE@7d4bf2b`, dan karena itu sudah dapat dipanggil hari ini:

| Endpoint | Dipakai layar |
| --- | --- |
| `GET /api/v1/health-services/clinical-management/patient-assessments/episodes/{episodeId}` | `FE-KEP-01`, `FE-KEP-02` |
| `GET /api/v1/health-services/clinical-management/patient-assessments/{id}` | `FE-KEP-02` |
| `POST /api/v1/health-services/clinical-management/patient-assessments` | `FE-KEP-02` |
| `GET /api/v1/health-services/medical-record-management/clinical-note-addendums/by-document/{documentKind}/{documentId}` | `FE-KEP-02`, `FE-KEP-03`, `FE-KEP-05` |

### 0.4 Bentuk koreksi di layar berubah, dan bedanya terlihat pengguna

Ini perubahan revision `2` yang paling terasa di layar. `RWI-DEC-091` membedakan **koreksi** dari
**perkembangan**, dan keduanya tampil berbeda:

| Yang dikoreksi | Bentuk di layar | Kenapa |
| --- | --- | --- |
| Pengkajian keperawatan yang sudah selesai | Isi asli tetap tampil apa adanya. Koreksi muncul sebagai **baris addendum bernomor** di bawahnya, beserta alasan, penulis, dan waktunya. Status pengkajian **tetap** "Selesai" | Pembetulan kesalahan. Sama persis seperti dokumen dokter, sehingga satu lembar rekam medis tidak memuat dua bentuk koreksi |
| Catatan tindakan yang sudah final | Sama | Pembetulan kesalahan |
| Butir rencana asuhan | Panel **riwayat versi**, bukan addendum. Versi lama menyimpan penulis dan waktu **aslinya** | Perubahannya bukan pembetulan melainkan perkembangan klinis — PRD `CAP-013` aturan 5 |

**Contoh supaya tidak tertukar.** Ns. Sari salah menulis skor nyeri 7 padahal seharusnya 4 — itu
**koreksi**, dan skor 7 tetap terbaca dengan addendum di bawahnya yang menyebut angka benarnya.
Sementara itu nyeri Tn. Budi memang turun dari 7 ke 4 setelah obat masuk — itu **perkembangan**,
dan tercatat sebagai pengkajian ulang yang baru, bukan koreksi atas yang lama.

### 0.5 Yang tetap `DEV_DISCRETION`

Warna, jarak, ikon, pilihan component library, dan tata letak di dalam wilayah layar **tidak**
dikunci roadmap ini. Yang dikunci hanya **isi, sumber data, keterjangkauan, dan butir hak akses per
tombol** — sesuai `03-frontend-architecture.md` bagian 3 dan 7.

Keputusan reuse atau buat baru untuk setiap elemen UI diambil **saat eksekusi berdasarkan bukti
source frontend**, bukan diputuskan di roadmap ini.

---

## 1. Cara membaca roadmap ini

**Arti tanda status pada dokumen ini.**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

Hari ini keenam task berstatus **belum dikerjakan**, dan **tidak satu pun** bertanda ⛔. Yang
menahan mereka bukan gerbang, melainkan urutan: setiap layar menunggu backend pasangannya
mendarat.

---

## 2. Gelombang dan urutan dependency

| Gelombang | Task | Layar | Prasyarat backend |
| --- | --- | --- | --- |
| **`KEP-MVP-1`** | `FE-RWI-051` | `FE-KEP-01` Ruang Kerja | `BE-RWI-054` |
| **`KEP-MVP-1`** | `FE-RWI-052` | `FE-KEP-02` Pengkajian | `BE-RWI-056`, `BE-RWI-065`, `BE-RWI-057` |
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

`FE-RWI-052` kini menunggu **tiga** task backend, bukan dua. `BE-RWI-065` masuk sebagai prasyarat
karena dialog koreksi tidak punya makna sebelum pengkajian yang selesai benar-benar terkunci:
tanpa penguncian, tombol "Sunting" masih berfungsi dan pengguna tidak akan pernah memakai
koreksi.

---

## 3. Task

### `FE-RWI-051` — Perawat punya satu tempat kerja untuk satu pasien

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan. **Tidak terblokir**; menunggu `BE-RWI-054` |
| **Outcome** | Perawat membuka satu halaman dan melihat seluruh dokumentasi pasien rawat inap yang menjadi tanggung jawabnya, tanpa berpindah-pindah menu |
| **Trace** | `FE-KEP-01`; `FR-KEP-001` s.d. `FR-KEP-004`; `03-frontend-architecture.md` bagian 3.1; `IA-INP-01` tiga klik dari Beranda |
| **Kontrak** | `contracts/api-contract.md` `0.3.0` grup Patient Assessment; konteks episode lewat `INT-KEP-02` |
| **Reuse** | Base component dan design token Quilvian yang sudah ada; pola detail episode `FE-INP-04`. **Keputusan reuse atau buat baru diambil saat eksekusi berdasarkan bukti source**, bukan diputuskan di sini |
| **Scope** | Route `…/episodes/{id}/nursing`; kerangka ruang kerja; kepala konteks pasien dan episode; jalan masuk ke `FE-KEP-02` s.d. `FE-KEP-05`; jalan masuk dari `FE-INP-04` dan baris census `FE-INP-01` |
| **Dependency** | `BE-RWI-054` |
| **Acceptance criteria** | 1. Layar tercapai dari Beranda dalam paling banyak **tiga klik** lewat Census. 2. Kepala konteks menampilkan pasien, episode, ruangan, DPJP, dan hari perawatan dari data episode yang sebenarnya. 3. **Bila kepala konteks gagal dimuat, seluruh tombol tulis nonaktif** dan pesan gagal beserta tombol coba lagi tampil. 4. Kegagalan memuat alergi **ditampilkan menonjol**, tidak disembunyikan. 5. Tanpa `PatientAssessment : Read`, layar tidak dapat dibuka. 6. **Nol butir menu baru** ditambahkan |
| **Verification** | Telusur tiga klik dari Beranda; pemeriksaan sidebar sebelum dan sesudah; uji tanpa hak akses; **skenario konteks gagal dimuat** lalu pemeriksaan bahwa tombol tulis benar-benar mati |
| **Risk/blocker** | Formulir kosong di atas konteks yang belum pasti adalah cara paling mudah mencatat sesuatu pada pasien yang salah. Kriteria 3 adalah aturan keselamatan, bukan kenyamanan. Menambahkan butir menu tingkat dua akan melanggar `IA-INP-05`. Owner: Frontend authority |
| **DoD** | Route, kerangka layar, kepala konteks, empat jalan masuk anak, dua jalan masuk induk; nol butir menu baru; keenam acceptance criteria terbukti; `npm run lint` lulus; `npm run build` lulus |

---

### `FE-RWI-052` — Perawat mengisi, menyelesaikan, dan membetulkan pengkajian

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan. **Tidak terblokir**; menunggu `BE-RWI-056`, `BE-RWI-065`, `BE-RWI-057` |
| **Outcome** | Perawat mengisi pengkajian awal dan pengkajian ulang di sistem, dan dapat membetulkan yang sudah selesai lewat **koreksi beralasan** yang tidak menghapus isi aslinya |
| **Trace** | `FE-KEP-02`; `FR-KEP-005`, `FR-KEP-006`, `FR-KEP-008`, `FR-KEP-009`; `RWI-DEC-091`, `RWI-AC-175`; `03-frontend-architecture.md` bagian 3.2 |
| **Kontrak** | `contracts/api-contract.md` `0.3.0` `POST /`, `GET /{id}`, `PATCH /{id}/complete`, `POST /{id}/addendums`, `GET /{id}/addendums`; `contracts/validation-matrix.md` `VAL-KEP-08`, `VAL-KEP-11`, `VAL-KEP-12` |
| **Perubahan bentuk dari revision `1`** | Dialog **amandemen** menjadi dialog **tambah koreksi**. Isi asli tetap tampil apa adanya; koreksi muncul sebagai baris addendum bernomor urut di bawahnya. Layar **tidak** menampilkan status "Diamandemen" — status pengkajian tetap "Selesai" |
| **Reuse** | Pola formulir bertahap yang dipakai alur admisi `FE-INP-16` s.d. `FE-INP-19` |
| **Scope** | Formulir pengkajian awal dan ulang bertujuh kelompok isian; tombol Simpan dan Selesaikan; **dialog tambah koreksi dengan alasan wajib**; daftar koreksi bernomor; penanda jenis pengkajian; penanda tenggat; penyajian pesan penolakan apa adanya dari validation matrix |
| **Dependency** | `FE-RWI-051`, `BE-RWI-056`, `BE-RWI-065`, `BE-RWI-057` |
| **Acceptance criteria** | 1. Percobaan membuat pengkajian awal **kedua** menampilkan pesan yang mengarahkan ke pengkajian ulang, bukan pesan teknis. 2. Koreksi tanpa alasan ditolak **di layar sebelum dikirim**. 3. Pengkajian yang sudah selesai **tidak menampilkan tombol sunting langsung**; yang tersedia hanya "Tambah koreksi". 4. Isi asli pengkajian tetap tampil sesudah dikoreksi, dan koreksinya tampil sebagai baris bernomor beserta alasan, penulis, dan waktunya. 5. Pengiriman ganda tidak menghasilkan dua pengkajian. 6. Penanda tenggat berbunyi **"Batas waktu belum ditetapkan"** ketika master kebijakan kosong, dan **tidak** menahan pengisian |
| **Verification** | Skenario pengkajian awal kedua; skenario koreksi tanpa alasan; skenario koreksi berulang lalu pemeriksaan nomor urutnya; uji tekan tombol dua kali; skenario master kebijakan kosong |
| **Risk/blocker** | Menampilkan koreksi sebagai "versi baru yang menggantikan" akan menghapus makna klinis isi aslinya di mata pembaca. Yang benar: isi asli di atas, koreksi di bawahnya. Owner: Frontend authority |
| **DoD** | Formulir dua jenis, dialog tambah koreksi, daftar koreksi bernomor, penanda tenggat, penanganan keenam keadaan, pesan sesuai validation matrix; `npm run lint` lulus; `npm run build` lulus |

---

### `FE-RWI-053` — Perkembangan pasien terbaca sebagai garis waktu, bukan angka terakhir

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan. **Tidak terblokir**; menunggu `BE-RWI-058` |
| **Outcome** | Perawat dan DPJP melihat apakah nyeri pasien membaik atau memburuk sejak masuk, langsung dari satu layar |
| **Trace** | `FE-KEP-03`; `FR-KEP-007`; `AC-CAP012-02`; `03-frontend-architecture.md` bagian 3.3 |
| **Kontrak** | `contracts/api-contract.md` `0.3.0` `GET /episodes/{episodeId}/timeline` dan `GET /episodes/{episodeId}/due-status` |
| **Reuse** | Pola tabel dan penyajian data Quilvian yang sudah ada |
| **Scope** | Lini masa per jenis pengukuran: nyeri, risiko jatuh, skrining gizi; penanda keadaan tenggat; **penanda koreksi** pada baris yang pernah dikoreksi beserta nomor urut addendum-nya; keadaan kosong yang berbunyi jelas |
| **Dependency** | `FE-RWI-051`, `BE-RWI-058` |
| **Acceptance criteria** | 1. Seluruh pengukuran tampil terurut waktu, bukan hanya yang terakhir. 2. Nilai lama **tidak pernah** hilang dari lini masa. 3. Keadaan kosong membedakan "belum ada pengkajian" dari "tidak dapat dimuat", dan keduanya berbeda dari "batas waktu belum ditetapkan". 4. Baris yang pernah dikoreksi membawa penanda koreksi beserta nomor urutnya, dan isi aslinya tetap tampil di atasnya |
| **Verification** | Skenario tiga pengukuran nyeri berurutan; skenario master kebijakan kosong; skenario gagal memuat; skenario baris yang sudah dikoreksi |
| **Risk/blocker** | Menyamakan "kosong" dengan "gagal" menyesatkan pembaca klinis: yang pertama berarti pasien belum dikaji, yang kedua berarti data ada tetapi tidak terbaca. Keduanya terlihat sama di layar tetapi menuntut tindakan yang berlawanan. Owner: Frontend authority |
| **DoD** | Lini masa tiga jenis pengukuran, penanda tenggat, penanda koreksi, tiga keadaan terbedakan; `npm run lint` lulus; `npm run build` lulus |

---

### `FE-RWI-054` — Perawat menetapkan masalah, tujuan, dan evaluasi asuhan

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan. **Tidak terblokir**; menunggu `BE-RWI-059` dan `BE-RWI-060` |
| **Outcome** | Rencana asuhan keperawatan tersusun di sistem beserta riwayat perubahannya, menggantikan catatan kertas |
| **Trace** | `FE-KEP-04`; `FR-KEP-012` s.d. `FR-KEP-017`; `RWI-AC-177`; `03-frontend-architecture.md` bagian 3.4 |
| **Kontrak** | `contracts/api-contract.md` `0.3.0` grup Nursing Care Plan, tujuh endpoint |
| **Perubahan bentuk dari revision `1`** | **Tidak ada, dan itu disengaja.** `RWI-DEC-091` sengaja **tidak** memindahkan rencana asuhan ke mesin addendum. Panel riwayat versi tetap panel riwayat versi |
| **Reuse** | Pola daftar induk-anak yang sudah dipakai layar penempatan tempat tidur |
| **Scope** | Daftar butir masalah; formulir tambah dan ubah butir; dialog evaluasi; dialog tutup butir dengan alasan wajib; panel riwayat versi; keadaan hanya-baca ketika episode `Closed` |
| **Dependency** | `FE-RWI-051`, `BE-RWI-059`, `BE-RWI-060` |
| **Acceptance criteria** | 1. Butir dapat dinyatakan tercapai **hanya** bila evaluasinya sudah diisi; tombolnya nonaktif sebelum itu, beserta keterangan kenapa. 2. Riwayat versi menampilkan **penulis dan waktu asli** tiap versi, bukan penulis yang mengubah. 3. Perubahan butir tampil sebagai **versi baru**, **bukan** sebagai addendum. 4. Pada episode `Closed`, seluruh tombol ubah hilang dan riwayat tetap terbaca |
| **Verification** | Skenario tutup butir tanpa evaluasi; **pemeriksaan penulis pada riwayat versi**; skenario episode tertutup; pemeriksaan bahwa layar ini tidak memanggil endpoint addendum sama sekali |
| **Risk/blocker** | Masalah keperawatan ditulis sebagai teks sampai katalog SDKI/SLKI/SIKI diputuskan; layar **tidak boleh** mengunci bentuk isiannya ke katalog yang belum ada. **Tidak memblokir.** Owner: Clinical governance |
| **DoD** | Daftar butir, tiga dialog, panel riwayat versi, keadaan hanya-baca, keempat acceptance criteria terbukti; `npm run lint` lulus; `npm run build` lulus |

---

### `FE-RWI-055` — Perawat mencatat tindakan yang sudah dilakukan

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan. **Tidak terblokir**; menunggu `BE-RWI-061` dan `BE-RWI-062` |
| **Outcome** | Tindakan keperawatan tercatat beserta waktu, pelaku, dan hasilnya. Kegagalan tagihan terlihat sebagai keadaan tersendiri yang tidak menghapus catatan klinis |
| **Trace** | `FE-KEP-05`; `FR-KEP-018` s.d. `FR-KEP-022`; `AC-CAP014-01`, `AC-CAP014-02`, `AC-CAP014-03`; `RWI-AC-176`; `03-frontend-architecture.md` bagian 3.5 |
| **Kontrak** | `contracts/api-contract.md` `0.3.0` grup Nursing Intervention, enam endpoint |
| **Perubahan bentuk dari revision `1`** | Dialog **amandemen** menjadi dialog **tambah koreksi**, sama seperti `FE-RWI-052`. Status catatan tetap "Final" sesudah dikoreksi |
| **Reuse** | Pola pencegahan pengiriman ganda yang sudah dipakai alur admisi |
| **Scope** | Formulir catat tindakan; daftar tindakan per episode **terurut waktu tindakan, bukan waktu pencatatan**; tombol Finalkan; dialog tambah koreksi beralasan; daftar koreksi bernomor; penanda keadaan pengiriman tagihan |
| **Dependency** | `FE-RWI-051`, `BE-RWI-061`, `BE-RWI-062` |
| **Acceptance criteria** | 1. Menekan tombol simpan dua kali menghasilkan **satu** baris tindakan; tombol dinonaktifkan selama permintaan berjalan. 2. Tindakan mendadak dapat dicatat **tanpa** memilih butir rencana asuhan. 3. Ketika pengiriman tagihan gagal, catatan klinis **tetap tampil** dan penanda kegagalannya terpisah — **bukan** sebagai galat halaman. 4. Bagi pengguna yang bukan penulis dan bukan kepala ruangan, tombol tambah koreksi **tidak tersedia**. 5. Catatan yang sudah final tidak menampilkan tombol sunting langsung |
| **Verification** | Uji tekan ganda; skenario tanpa rencana asuhan; skenario tagihan gagal; uji dua peran berbeda — perawat pelaksana dan kepala ruangan |
| **Risk/blocker** | Menampilkan kegagalan tagihan sebagai kegagalan pencatatan klinis akan membuat perawat mengulang tindakan yang sudah tersimpan. **Contoh:** infus sudah terpasang dan tercatat, tetapi layar berbunyi "Gagal menyimpan" karena Billing mati; perawat memasang ulang infus. Yang benar: catatan tampil normal dengan penanda kecil "Tagihan belum terkirim". Owner: Frontend authority |
| **DoD** | Formulir, daftar terurut waktu tindakan, tombol Finalkan, dialog tambah koreksi, penanda tagihan; kelima acceptance criteria terbukti; `npm run lint` lulus; `npm run build` lulus |

---

### `FE-RWI-056` — Kepala ruangan menemukan pengkajian yang tertinggal

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan. **Tidak terblokir**; menunggu `BE-RWI-064` |
| **Outcome** | Daftar pantau **ketiga** akhirnya ada. `RWI-RULE-023` menuntutnya sejak awal, dan roadmap `episode-rawat-inap` mencatatnya sebagai gap yang tertahan karena bergantung pada dokumentasi klinis |
| **Trace** | `FE-KEP-06`; `FR-KEP-024`, `FR-KEP-025`, `FR-KEP-026`; `RWI-RULE-023`, `RWI-DEC-032`; `03-frontend-architecture.md` bagian 3.6 |
| **Kontrak** | `contracts/api-contract.md` `0.3.0` endpoint daftar pantau kepatuhan |
| **Reuse** | **Layar `FE-INP-09` Daftar Pantau yang sudah ada.** Task ini menambah satu daftar ke dalamnya, **bukan** membuat layar baru |
| **Scope** | Satu daftar tambahan pada `FE-INP-09`; penyaringan ruangan dan keadaan tenggat; keadaan kosong yang berbunyi benar; setiap baris membuka ruang kerja pasiennya |
| **Dependency** | `BE-RWI-064` |
| **Acceptance criteria** | 1. Daftar muncul sebagai daftar **ketiga** pada `FE-INP-09`, bukan sebagai layar atau butir menu baru. 2. Daftar kosong berbunyi **"Seluruh pengkajian sudah tepat waktu"**, bukan "tidak ada data". 3. Ketika kebijakan belum diisi, daftar berbunyi **"Batas waktu pengkajian belum ditetapkan, sehingga keterlambatan belum dapat dihitung"** — dan itu **berbeda** dari kriteria 2. 4. Layar tercapai dari Beranda dalam **dua klik**. 5. Tidak ada tombol pada daftar ini yang menahan pekerjaan klinis mana pun. 6. Daftar **tidak** menampilkan isi klinis, hanya nama pasien, lokasi, dan keterlambatan |
| **Verification** | Telusur dua klik; skenario daftar kosong; skenario kebijakan kosong; pemeriksaan urutan daftar pada `FE-INP-09`; pemeriksaan bahwa tidak ada kolom berisi catatan bebas |
| **Risk/blocker** | Urutan daftar di dalam `FE-INP-09` kini dipakai **tiga** sub-modul dan **tidak boleh** diputuskan sendiri-sendiri — `02-module-map.md` bagian 6. Owner: Frontend authority bersama pemilik `episode-rawat-inap` |
| **DoD** | Satu daftar tambahan, penyaringan, dua bunyi keadaan kosong yang berbeda, nol layar baru, nol butir menu baru; keenam acceptance criteria terbukti; `npm run lint` lulus; `npm run build` lulus |

---

## 3.1 Register status task

| Task | Layar | Gelombang | Status | Laporan |
| --- | --- | --- | :---: | --- |
| `FE-RWI-051` | `FE-KEP-01` Ruang Kerja Keperawatan | `KEP-MVP-1` | tanpa tanda | — |
| `FE-RWI-052` | `FE-KEP-02` Pengkajian | `KEP-MVP-1` | tanpa tanda | — |
| `FE-RWI-053` | `FE-KEP-03` Lini Masa | `KEP-MVP-1` | tanpa tanda | — |
| `FE-RWI-054` | `FE-KEP-04` Rencana Asuhan | `KEP-MVP-2` | tanpa tanda | — |
| `FE-RWI-055` | `FE-KEP-05` Catatan Tindakan | `KEP-MVP-3` | tanpa tanda | — |
| `FE-RWI-056` | `FE-KEP-06` Daftar Pantau Kepatuhan | `KEP-MVP-4` | tanpa tanda | — |

Laporan task ditulis ke `<blueprint-root>/task/report/frontend/<TASK-ID>.md` sesuai
`rules/rule-output/lokasi-laporan-task.md`.

---

## 4. Ketergantungan test dan satu gerbang yang tetap terbuka

| Yang dibutuhkan | Kenapa | Keadaan |
| --- | --- | --- |
| Episode berstatus `Admitted` beserta perawat penanggung jawabnya | Seluruh layar butuh konteks | Tersedia lewat `episode-rawat-inap` |
| Sekurang-kurangnya satu baris master kebijakan batas waktu, **dan** satu skenario tanpa baris itu sama sekali | Menguji penanda tenggat pada kedua keadaan — `VAL-KEP-17` | Menyusul bersama `BE-RWI-055` |
| Peran perawat pelaksana, kepala ruangan, dan DPJP terpisah | Menguji `AC-CAP014-03` dan kolom baca-saja DPJP | Perlu disiapkan saat eksekusi |
| **Data master rawat inap yang layak — `RWI-UI-GAP-007`** | Uji ujung ke ujung yang bermakna | **Masih terbuka.** Ia menahan **uji e2e yang bermakna**, bukan pembangunan layar. Owner: pemilik data master bersama `RWI-DEC-063`/`BE-RWI-002` |

`RWI-UI-GAP-007` sengaja **tidak** dijadikan blocker task mana pun. Menahan enam layar karena data
master contoh belum rapi akan menukar pekerjaan nyata dengan kelengkapan administrasi. Yang benar:
layar dibangun, dan butir DoD yang menuntut e2e dinyatakan belum terpenuhi apa adanya sampai
gapnya tertutup.

---

## 5. Coverage gap requirement ke test

| Requirement | Layar pemilik | Task | Catatan |
| --- | --- | --- | --- |
| `FR-KEP-001` s.d. `FR-KEP-004` | `FE-KEP-01` | `FE-RWI-051` | Pintu masuk; dibuktikan telusur tiga klik |
| `FR-KEP-005`, `FR-KEP-006`, `FR-KEP-008`, `FR-KEP-009` | `FE-KEP-02` | `FE-RWI-052` | Lengkap, termasuk bentuk koreksi baru |
| `FR-KEP-007`, `FR-KEP-010`, `FR-KEP-011` | `FE-KEP-03` | `FE-RWI-053` | Lengkap |
| `FR-KEP-012` s.d. `FR-KEP-017` | `FE-KEP-04` | `FE-RWI-054` | Lengkap |
| `FR-KEP-018` s.d. `FR-KEP-023` | `FE-KEP-05` | `FE-RWI-055` | `FR-KEP-023` catatan terpadu dibuktikan di backend; frontend hanya membacanya |
| `FR-KEP-024` s.d. `FR-KEP-026` | `FE-KEP-06` | `FE-RWI-056` | Lengkap |
| `FR-KEP-027`, `FR-KEP-028` | — | **Tidak ada, disengaja** | `EPIC KEP-06` `DEFERRED` oleh `RWI-DEC-089` |

**Nol layar tanpa task, dan nol task tanpa layar.** Setiap layar pada
`03-frontend-architecture.md` bagian 1 punya tepat satu task pemilik, dan setiap layar sudah
dinyatakan sebagai layar anak beserta induknya pada bagian 0.2 — sehingga tidak ada layar yang
tidak dapat dicapai siapa pun.

---

## 6. Yang sengaja tidak dibuat roadmap ini

| Yang ditolak | Alasan |
| --- | --- |
| Butir menu tingkat dua untuk sub-modul ini | Kuota `IA-INP-05` sudah penuh, dan pekerjaan perawat berputar pada satu pasien, bukan pada daftar dokumen |
| Layar baru untuk daftar pantau kepatuhan | `FE-INP-09` sudah ada dan sudah memuat dua daftar. Yang ketiga menempel di sana |
| Layar cetak | Tidak ada layar cetak pada sub-modul ini — `03-frontend-architecture.md` bagian 6 |
| Layar pemakaian alat | `EPIC KEP-06` `DEFERRED` oleh `RWI-DEC-089` |
| Layar asuhan gizi | Modul Gizi `PLANNED`. Yang ada di sini hanya hasil skrining pada pengkajian |
| **Dialog "amandemen" yang menghasilkan versi baru pada pengkajian dan tindakan** | Dicabut `RWI-DEC-091`. Bentuknya kini koreksi beraddendum. Pada rencana asuhan, riwayat versi **tetap** dipakai |
| Menampilkan isi klinis pada daftar pantau maupun tooltip | `03-frontend-architecture.md` bagian 6: catatan bebas hanya tampil pada layar detail |
