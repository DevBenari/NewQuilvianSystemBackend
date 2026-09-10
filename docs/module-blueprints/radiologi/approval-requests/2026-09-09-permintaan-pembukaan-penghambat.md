# Permintaan Pembukaan Penghambat Pengembangan — Modul Radiologi

| Field | Value |
|---|---|
| `request_id` | `RAD-REQ-001` |
| `tanggal` | 2026-09-09, diperbarui 2026-09-10 |
| `pengaju` | Yoga Aji Pratama — Product/Domain Owner Radiologi (`RAD-DEC-014`) |
| `rujukan` | `blueprint-manifest.md` revision `7`; `04-prd-to-mvp.md` bagian 20 |
| `status` | **`dijawab sebagian`** — butir 1 dan 3 selesai, butir 2 masih terbuka. Lihat bagian 0.1 |
| `sifat` | Operasional. **Bukan** artefak desain — tidak masuk daftar hash manifest |

Dokumen ini dapat diteruskan apa adanya. Setiap bagian berdiri sendiri: penerima cukup membaca
bagian yang menyebut namanya.

---

## 0.1 Hasil — Diperbarui 2026-09-10

Persetujuan disampaikan pemilik modul Radiologi pada 2026-09-10.

### Selesai — dua butir

| No | Yang diminta | Siapa yang menyetujui | Apa yang dikerjakan |
|---:|---|---|---|
| 1 | Menaikkan registry ke `ACTIVE` | Muhammad Hamzah, pemegang registry | Baris 22 `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` dinaikkan ke `ACTIVE`; entri riwayat 2026-09-10 ditambahkan |
| 3 | Memperbaiki pernyataan modul IGD | Rizki, pemilik modul IGD | Keterangan pada `EmergencyOrderKind.cs` dan pesan pada layar penunjang IGD diperbarui |

**Temuan saat mengerjakan butir 1.** Registry canonical pada repo `QuilvianEngineeringSkills`
ternyata **sudah** mencatat `Rad` sebagai `ACTIVE` sejak 2026-09-09. Yang tertinggal hanya
salinan backend — pola selisih yang sama persis dengan `ACC-DEP-007`. Jadi pekerjaan ini
menjadi **propagasi**, bukan persetujuan baru, dan itu dicatat apa adanya pada entri riwayat.

Muncul satu hal baru: salinan registry pada plugin cache `quilvian-engineering-skills/0.1.0`
masih tertulis `PLANNED`. Dicatat sebagai `RAD-OPEN-010`.

**Batas butir 3.** Yang dikerjakan hanya perbaikan teks. Penyambungan pemesanan radiologi IGD
ke endpoint resmi **belum** dikerjakan, dan memang sengaja menunggu frontend Radiologi Rilis 1
sesuai catatan bagian 3.5. Peninjauan status `IGD-DEC-099` juga belum dilakukan dan tetap milik
pemilik modul IGD.

### Masih terbuka — satu butir

| No | Yang dibutuhkan | Kenapa belum selesai |
|---:|---|---|
| 2 | Pemetaan empat sebutan peran ke peran Quilvian | Yang diminta adalah **empat baris isian**, bukan izin. Menyetujui permintaan tidak memberi tahu peran mana yang setara |

**Butir 2 kini menjadi satu-satunya penghambat `/plan-module-delivery`.**

---

## 0. Ringkasan — Apa yang Diminta

Blueprint modul Radiologi sudah selesai dirancang. Sembilan belas berkas, mencakup keputusan,
audit kemampuan, penilaian kelengkapan, arsitektur domain, arsitektur backend dan frontend,
ERD, lima kontrak, matriks pengujian, dan dokumen PRD ke MVP.

Yang menahan pekerjaan berikutnya **bukan** keputusan bisnis yang belum diambil. Seluruh
keputusan yang dibutuhkan sudah diambil. Yang belum terjadi adalah **penerapannya**.

| No | Yang diminta | Kepada | Menahan |
|---:|---|---|---|
| 1 | Menaikkan modul Radiologi pada registry dari `PLANNED` menjadi `ACTIVE` | Pemegang registry prefix | Seluruh entity `Rad*` baru, termasuk tabel hasil bacaan |
| 2 | Memetakan empat sebutan peran ke peran Quilvian yang sebenarnya | Pemilik modul + Administrator | Penegakan aturan pengesahan hasil bacaan dan aturan keselamatan |
| 3 | Memperbaiki pernyataan modul IGD yang sudah tidak benar | Pemilik modul IGD | Titik sentuh pemesanan radiologi dari IGD |

Butir 1 dan 2 **memblokir** `/plan-module-delivery`. Butir 3 tidak memblokir, tetapi
menyesatkan pengguna setiap hari selama dibiarkan.

---

## 1. Untuk Pemegang Registry Prefix

### 1.1 Yang diminta

Ubah baris 22 pada `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`:

| Sekarang | Diminta |
|---|---|
| `HealthServices` \| `RadiologyManagement / Radiology` \| `BUSINESS DOMAIN / MODULE` \| `Rad` \| **`PLANNED`** | ... \| **`ACTIVE`** |

Tambahkan entri riwayat perubahan yang mencatat tiga hal apa adanya:

| Yang dicatat | Isinya |
|---|---|
| Tanggal keputusan | 28 Agustus 2026, `RJ-BIL-DEC-014`, Sukma Giri |
| Tanggal penerapan | Tanggal registry benar-benar diperbaiki |
| Catatan selisih | Delapan tabel `Rad*` dibuat **sebelum** penerapan itu, lewat migration 28 dan 31 Agustus 2026 |

Samakan juga salinan registry pada suite Skill `QuilvianEngineeringSkills`, karena keduanya
hari ini sama-sama tertulis `PLANNED`.

### 1.2 Mengapa ini bukan permintaan keputusan baru

Kenaikan status ini **sudah pernah diputuskan dan disetujui**.

| Sumber | Isinya |
|---|---|
| `RJ-BIL-DEC-014` pada blueprint `rawat-jalan` | Menunjuk Sukma Giri sebagai Product/Domain Owner `RadiologyManagement`, **dan menaikkan lifecycle modul dari `PLANNED` menjadi `ACTIVE`**. Disetujui 28 Agustus 2026 |
| `Areas/HealthServices/BillingManagement/Operational/Constants/BillingSourceContract.cs` baris 11-13 | Menulis bahwa kenaikan itu **sudah terjadi**: "Radiology terdaftar sejak `RJ-BIL-BE-004`, setelah `RJ-BIL-DEC-014` menunjuk pemilik modulnya dan menaikkan prefix `Rad` pada registry ke `ACTIVE`" |
| Berkas registry | Masih `PLANNED`, tanpa satu pun entri riwayat untuk Radiologi |

Modul Laboratorium diperlakukan dengan pola yang sama dan **punya** entri riwayatnya, tertanggal
2026-09-02, lewat permintaan `LAB-REQ-002`. Radiologi hanya menyusul.

Jadi yang diminta di sini adalah **menjalankan keputusan yang sudah sah**, bukan mengambil
keputusan baru.

### 1.3 Mengapa dicatat jujur, bukan ditulis mundur

Selisih waktu antara keputusan dan penerapannya adalah informasi yang berguna bagi audit: ia
menunjukkan tata kelola sempat tertinggal dari kode, dan itu pola yang perlu diketahui supaya
tidak berulang.

Menuliskan tanggal penerapan seolah-olah 28 Agustus akan menghapus jejak itu. Cara
penyelesaian ini sudah dikunci `RAD-DEC-007` oleh pemilik modul.

### 1.4 Yang tertahan selama ini belum dilakukan

`QBE-MOD-002` menahan pembuatan entity operasional baru untuk modul berstatus `PLANNED`.
Akibatnya:

| Yang tertahan | Slice |
|---|---|
| Tabel `RadReport` — wadah hasil bacaan radiolog | `S9` |
| Tabel `RadReportVersion` — koreksi berversi | `S10` |
| Gelombang `MVP-0` seluruhnya | — |

Hasil bacaan radiolog adalah **alasan utama Rilis 1 ada**. Tanpa registry dinaikkan, rantai
pelayanan berhenti pada "foto sudah diambil" dan tidak menghasilkan apa pun yang dapat dipakai
merawat pasien.

### 1.5 Satu pertanyaan yang perlu dijawab bersamaan

Delapan tabel `Rad*` yang sudah rilis dibuat **ketika** registry masih `PLANNED`. Perlu
diputuskan mana yang berlaku:

| Pilihan | Akibatnya |
|---|---|
| Dicatat sebagai pengecualian yang disahkan pada `QBE_EXCEPTIONS.json` | Pelanggaran yang lalu tercatat resmi |
| Dianggap registry yang lupa diperbarui, dan cukup dinaikkan sekarang | Lebih sederhana; selisihnya tetap tercatat di entri riwayat |

Pengaju **tidak** mengusulkan salah satunya. Ini wewenang pemegang registry.

### 1.6 Satu permintaan tambahan yang lebih kecil

Migration 3 pada rencana Radiologi menambahkan tiga kolom penanda cito pada tabel `RadOrder`
yang **sudah ada**. Ini bukan entity baru, sehingga secara harfiah `QBE-MOD-002` tidak
menahannya.

Mohon dikonfirmasi bahwa tafsir itu benar, supaya tidak ada perbedaan pemahaman di tengah
jalan.

---

## 2. Untuk Pemilik Modul dan Administrator

### 2.1 Yang diminta

Empat sebutan peran dipakai keputusan modul Radiologi. **Belum satu pun dipetakan** ke peran
yang benar-benar terpasang di Quilvian. Mohon ditetapkan padanannya.

| Sebutan dalam keputusan | Dipakai untuk | Peran Quilvian |
|---|---|---|
| **Dokter radiolog** | Mengesahkan dan merilis hasil bacaan (`RAD-DEC-003`) | _mohon diisi_ |
| **Penanggung jawab klinis** | Mengesahkan aturan keselamatan (`RAD-DEC-005`) | _mohon diisi_ |
| **DPJP** | Mengesahkan pelewatan gerbang keselamatan darurat (`RAD-DEC-008`) | _mohon diisi_ |
| **Dokter jaga senior** | Sama seperti DPJP | _mohon diisi_ |

### 2.2 Mengapa ini memblokir, padahal terlihat seperti urusan konfigurasi

Aturan pengesahan hasil bacaan tidak dapat ditegakkan tanpa peta ini.

> **Contoh nyata.** `RAD-DEC-003` menyatakan dokter radiolog boleh mengesahkan draf bacaannya
> sendiri, tetapi residen tidak boleh. Ketika sistem menerima permintaan pengesahan, ia harus
> menjawab satu pertanyaan: **apakah orang ini seorang dokter radiolog?** Tanpa peta peran,
> pertanyaan itu tidak dapat dijawab, dan aturannya hanya menjadi tulisan di dokumen.

Hal yang sama berlaku untuk pengesahan aturan keselamatan, yang menentukan boleh atau tidaknya
seorang pasien disinari.

### 2.3 Dua pemisahan wewenang yang mohon dijaga saat menyusun peran

| No | Ketentuan | Alasan |
|---:|---|---|
| 1 | `RadSafetyRule : Create` dan `RadSafetyRule : Approve` **wajib dipegang peran yang berbeda** | Memberikan keduanya kepada satu peran meniadakan seluruh guna pengesahan berjenjang |
| 2 | `RadReport : Validate` **tidak dengan sendirinya** membolehkan pengesahan sendiri | Pemeriksaannya ada di dalam service, berdasarkan peran penulis draf. Peta peran menentukan hasil pemeriksaan itu |

### 2.4 Bila peran yang cocok belum ada

Bila ternyata belum ada peran Quilvian yang setara dengan salah satu sebutan di atas, mohon
dinyatakan demikian. Pembuatan peran baru adalah pekerjaan tersendiri, dan lebih baik diketahui
sekarang daripada saat implementasi berjalan.

---

## 3. Untuk Pemilik Modul IGD

### 3.1 Yang diminta

Tiga tindakan, satu di antaranya mendesak.

| No | Tindakan | Mendesak? |
|---:|---|---|
| 1 | Memperbaiki teks layar yang menyatakan "modul Radiologi belum ada" | **Ya** |
| 2 | Menyambungkan pemesanan radiologi IGD ke endpoint resmi | Sebaiknya menunggu frontend Radiologi Rilis 1 |
| 3 | Meninjau `IGD-DEC-099` yang masih berstatus `draft` | Ya |

### 3.2 Duduk perkaranya

Modul IGD memberi tahu penggunanya bahwa pemesanan radiologi harus ditempuh di luar sistem.
Pernyataan itu **benar ketika ditulis**, dan menjadi salah tiga hari kemudian.

| Tanggal | Kejadian |
|---|---|
| 26 Agustus 2026 | `IGD-DEC-099` menunda pemesanan radiologi **sampai pemilik `RadiologyManagement` ditunjuk** |
| 27 Agustus 2026 | Kode IGD ditulis dengan keterangan "modul Radiologi belum ada" |
| 28 Agustus 2026 | `RJ-BIL-DEC-014` menunjuk pemilik `RadiologyManagement` — **prasyarat penundaan gugur** |
| 31 Agustus 2026 | Modul Radiologi rilis, `POST /rad-orders` tersedia |
| 9 September 2026 | Teks IGD masih menyatakan modul belum ada |

Buktinya:

| Berkas | Isinya |
|---|---|
| `Areas/HealthServices/EmergencyInstallationManagement/Enums/EmergencyOrderKind.cs` | "pemesanan radiologi adalah kebutuhan klinis IGD meski modul Radiologi belum ada, sehingga pesanannya dibuat di luar sistem" |
| `QuilvianSystemFrontendDev/.../emergency-assessment-diagnostic-support-tab.jsx` baris 189 | "Pemeriksaan radiologi juga belum dapat dipesan lewat sistem — modul Radiologi belum ada" |

### 3.3 Akibatnya bagi pasien

Perawat dan dokter IGD diarahkan memesan radiologi di luar sistem. Pesanan yang ditempuh
begitu:

- tidak menghasilkan study;
- **tidak melewati gerbang keselamatan** — tidak ada skrining kehamilan, tidak ada pemeriksaan
  implan logam, tidak ada pemeriksaan alergi kontras;
- tidak menerbitkan fakta kelayakan tagih.

Butir kedua yang paling perlu diperhatikan.

### 3.4 Yang sudah diputuskan modul Radiologi

`RAD-DEC-009` menetapkan:

1. IGD memakai endpoint yang **sama** dengan poli dan rawat inap. Tidak ada perlakuan khusus.
2. Pesanan berjenis `External` yang terlanjur tercatat **dibiarkan sebagai riwayat**, tidak
   dipindahkan.

Alasan butir kedua: pesanan lama tidak punya study, tidak punya jawaban gerbang keselamatan,
dan tidak punya penilaian mutu. Memindahkannya berarti membuat catatan pemeriksaan untuk
kejadian yang tidak pernah tercatat. Riwayat yang tidak seragam lebih jujur daripada riwayat
rapi yang sebagian dikarang.

### 3.5 Catatan urutan

Endpoint sudah siap, tetapi petugas radiologi **belum punya layar** untuk menindaklanjuti
pesanan yang masuk. Penyambungan sebaiknya menunggu frontend Radiologi Rilis 1, supaya pesanan
tidak menumpuk tanpa ada yang mengerjakan.

Perbaikan teks pada butir 1 **tidak perlu menunggu apa pun**.

---

## 4. Cara Menjawab Permintaan Ini

Cukup nyatakan per butir. Tidak perlu format khusus.

| Butir | Yang dibutuhkan |
|---:|---|
| 1 | Konfirmasi registry sudah dinaikkan, beserta pilihan pada bagian 1.5 dan 1.6 |
| 2 | Empat baris peta peran pada tabel 2.1 |
| 3 | Konfirmasi dari pemilik modul IGD bahwa ketiga tindakan diterima |

Setelah butir 1 dan 2 dijawab, `04-prd-to-mvp.md` dapat naik status dan
`/plan-module-delivery` dapat dijalankan.

---

## 5. Riwayat Dokumen

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-09 | Permintaan pertama. Tiga butir untuk tiga penerima berbeda. | `terbuka` |
