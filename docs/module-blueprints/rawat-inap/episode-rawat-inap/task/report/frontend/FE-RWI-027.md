# Laporan Perubahan Frontend — `FE-RWI-027`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-027` |
| Judul | Alur ditutup tanpa menempatkan pasien — titik tulis 3 |
| Slice | Langkah **Konfirmasi** pada alur admisi dua jalur; titik tulis ketiga |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian 4, entri `FE-RWI-027` |
| Trace | `RWI-DEC-076`; `03-frontend-architecture.md` 3A.2 langkah 7, 3A.4 titik tulis 3, 3A.5, 3A.7; `05-skema-tampilan.md` bagian 3.9 dan 3.13; bagian 6 privasi |
| Contract version | API `0.4.0`. Kontrak dibaca langsung dari source backend `InpatientEpisodeController.cs` dan `InpatientEpisodeDtos.cs` |
| Wewenang UI | Susunan ringkasan `DEV_DISCRETION`. Batas yang tetap dipatuhi: penanda isolasi tanpa keterangannya, status dari server, dan wilayah yang dapat diubah menyebut sendiri apa yang terkunci |
| Dependency | `FE-RWI-026` (**source selesai 1 September 2026**). Tidak ada dependency backend baru: `PUT /episodes/{id}` sudah ada sejak `BE-RWI-008` |
| Klasifikasi | `HEAVY` — skor 9: repository 0, berkas diperiksa 2, berkas diubah 2, logika bisnis 2, kontrak API 1, database 0, keamanan/auth 0, UI/workflow 2 |
| Task mode | `FRONTEND` — backend strict read-only, kecuali laporan dan register modul ini |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; `NewQuilvianSystemBackend` **hanya** untuk laporan ini beserta tautan buktinya |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `55f91b6cd`, branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `85439d32a884adcde3067304774151b317b058a2`, branch `MHamzah` |
| Tanggal | 1 September 2026 |
| Status | ✅ **SELESAI.** Kelima acceptance criteria terpenuhi dan dibuktikan runtime di peramban. `npm run lint` `0 errors`; `npm run build` `✓ Compiled successfully`; verifikasi peramban `37/37 PASS` |

---

## 1. Keadaan yang ditemukan di awal

Sebelum task ini, langkah **Konfirmasi** hanya berupa kerangka. Berkas
`inpatient-admission-view.jsx` memiliki komponen `PendingStep` yang menampilkan kalimat
*"Isi operasional langkah ini akan dilengkapi oleh task lanjutan"* untuk setiap slug langkah
yang belum punya komponennya sendiri. Slug `confirmation` jatuh ke sana.

Akibatnya, petugas yang sudah menyelesaikan titik tulis 1 dan 2 tidak punya layar untuk
meninjau isian, tidak dapat membetulkan unit layanan atau kelas perawatan yang keliru, dan
tidak diberi tahu bahwa pasien **belum** dirawat.

Tiga celah lain yang ditemukan sewaktu memeriksa source:

| Celah | Bukti |
| --- | --- |
| `normalizeEpisodeDetail` tidak mengeluarkan `motherEpisodeId` | `inpatient-episode-utils.jsx` baris 57–110 sebelum perubahan. `UpdateAdmissionAsync` **mengganti** kolomnya, sehingga hubungan ibu dan bayi rawat gabung akan terputus tanpa ada yang meminta |
| `normalizeEpisodeDetail` tidak mengeluarkan `encounterId` | Sama. Dibutuhkan `FE-RWI-028` untuk membaca penjamin dari kunjungan |
| Tidak ada dialog keluar alur | `05-skema-tampilan.md` 3.13 memintanya sejak revision 0.4; belum ada di source mana pun |

---

## 2. Proses bisnis dari sisi pengguna

**Siapa penggunanya.** Petugas admisi rawat inap di meja pendaftaran.

**Kapan layar ini dibuka.** Sesudah tempat tidur dipesan pada langkah Booking Bed, yaitu
langkah ke-7 pada jalur pasien baru dan ke-8 pada jalur pasien lama.

**Langkah yang dilakukan berurutan:**

1. Petugas menekan **Lanjut ke Konfirmasi** dari langkah Booking Bed.
2. Layar membaca ulang episode dari server, lalu menampilkan **empat kartu ringkasan**:
   - **Pasien** — nama, nomor rekam medis, jenis kelamin, tanggal lahir, nomor HP, alamat.
   - **Penjamin** — cara bayar, nama penjamin, nomor polis atau nomor karyawan, kelas perawatan.
   - **Perawatan** — nomor episode, nomor kunjungan, unit layanan, DPJP, kebutuhan isolasi,
     dan status episode.
   - **Tempat Tidur** — nama tempat tidur, kamar, status pemesanan, dan **sisa waktunya**.
3. Di bawahnya ada wilayah **Yang Masih Dapat Diubah** berisi tiga isian: unit layanan,
   kelas perawatan, dan catatan admisi. Wilayah ini menyebut sendiri bahwa DPJP dan
   penjamin **tidak** dapat diubah dari sini, beserta alasannya.
4. Dua peringatan selalu tampil: bahwa mengunci admisi **tidak** menempatkan pasien, dan
   bahwa langkah berikutnya adalah cetak persetujuan lalu konfirmasi kedatangan pasien di
   Papan Tempat Tidur.
5. Petugas menekan **Kunci Admisi & Cetak Persetujuan**. Bila ada isian yang berubah,
   perubahannya dikirim ke server; bila tidak ada, tidak ada yang dikirim sama sekali.
   Keduanya sama-sama meneruskan ke langkah cetak persetujuan.

**Contoh konkret.** Petugas keliru memilih Ruang Melati padahal pasien seharusnya masuk
Ruang Anggrek. Pada langkah Konfirmasi ia mengganti isian Unit Layanan menjadi Ruang
Anggrek, lalu menekan Kunci Admisi. Satu permintaan `PUT` dikirim berisi unit yang baru,
kelas yang lama, hubungan episode ibu yang lama, dan catatan yang lama. Bila ia tidak
mengubah apa pun, nol permintaan `PUT` dikirim — episode tidak ditulis ulang hanya untuk
menambah satu baris audit yang tidak berarti apa-apa.

**Jalur tidak normal:**

| Keadaan | Yang terjadi di layar |
| --- | --- |
| Episode belum terbaca — misalnya halaman dimuat ulang di langkah ini | Seluruh ringkasan disembunyikan, diganti kalimat merah yang meminta petugas kembali ke langkah Dokter |
| Detail episode gagal dibaca dari server | Kalimat merah beserta tombol **Coba Lagi**; ringkasan yang tampil ditandai belum tentu mutakhir |
| Tempat tidur belum dipesan | Peringatan kuning yang meminta petugas kembali ke langkah Booking Bed |
| Isian unit atau kelas dikosongkan | Isian ditandai merah dan penguncian ditahan sebelum permintaan dikirim |
| Server menolak dengan 409 — episode sudah tidak `Draft` lagi | Pesan server ditampilkan apa adanya dan detail episode dimuat ulang, sehingga yang terbaca di layar menjadi keadaan yang sebenarnya |
| Petugas meninggalkan alur | Dialog **Tinggalkan Admisi?** muncul menyebut nomor episode, statusnya, tempat tidur yang masih dipesan beserta sisa waktunya, dan bahwa admisi dapat dilanjutkan dari Daftar Kerja Episode |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk apa diperiksa |
| --- | --- |
| `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Controllers/InpatientEpisodeController.cs` | Bentuk dan hak akses `PUT /episodes/{id}` |
| `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/DTOs/InpatientEpisodeDtos.cs` | Kolom `UpdateAdmissionRequest` dan `InpatientEpisodeDetailResponse` |
| `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs` | Perilaku `UpdateAdmissionAsync`, khususnya penggantian `MotherEpisodeId` |
| `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx` | Cara langkah dipilih dan controller dipasang |
| `src/components/view/health-services/inpatient-management/inpatient-admission-bed-step.jsx` | Pola visual langkah terdekat yang dipakai ulang |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx` | Bentuk pemesanan dan sisa waktunya |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-doctor.jsx` | Sumber `episodeId`, `payerSummary`, dan `serviceUnitSelect` |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-payment.jsx` | Sumber `patientClassSelect` |
| `src/utils/health-services/inpatient-management/inpatient-episode-utils.jsx` | Normalisasi detail episode yang dipakai ulang |
| `05-skema-tampilan.md` bagian 3.9, 3.12, 3.13, 4.3 | Kerangka layar, pola kata tombol, dialog keluar, privasi |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx` | Ditambah `INPATIENT_CLOSING_STEP_SLUGS`, `INPATIENT_CONFIRMATION_MESSAGES`, `INPATIENT_CONFIRMATION_LIMITS`, dan `INPATIENT_ADMISSION_EXIT_DIALOG`. Kalimat yang mengikat disimpan sebagai constant supaya tidak dapat hilang tanpa terlihat |
| `src/utils/health-services/inpatient-management/inpatient-admission-confirmation-utils.jsx` | **Baru.** Fungsi murni: penyusun nilai awal isian, penyusun payload `PUT`, pembanding perubahan, pemeriksa isian, dan keempat penyusun baris kartu ringkasan |
| `src/utils/health-services/inpatient-management/inpatient-episode-utils.jsx` | `normalizeEpisodeDetail` kini juga mengeluarkan `motherEpisodeId` dan `encounterId`. Keduanya tambahan, tidak mengubah kolom yang sudah ada |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-confirmation.jsx` | **Baru.** Controller langkah Konfirmasi: membaca episode, memegang isian yang masih dapat diubah, membandingkan perubahan, dan menjalankan titik tulis 3 |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-exit-guard.jsx` | **Baru.** Penjaga keluar alur beserta isi dialog yang menyesuaikan keadaan |
| `src/components/view/health-services/inpatient-management/inpatient-admission-confirmation-step.jsx` | **Baru.** Layar langkah Konfirmasi |
| `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx` | Langkah `confirmation` dipasangkan ke komponennya; controller konfirmasi dan penjaga keluar dipasang di level view; dialog keluar dirender sekali di level halaman |
| `src/style/health-services/inpatient-management/inpatient-admission.module.css` | Ditambah `confirmationStatus`, `confirmationGrid`, `confirmationEditablePanel`, `confirmationFieldGrid`. Seluruhnya memakai token |

### 3.3 Kepatuhan arsitektur frontend

Alur dependensinya lurus sesuai `rules/frontend/frontend-architecture.md`:

```text
URL (?step=confirmation)
  -> inpatient-admission-view.jsx (komposisi)
  -> use-inpatient-admission-confirmation.jsx (controller)
  -> inpatient-episode.service.js -> InstanceAxios
  -> inpatient-admission-confirmation-utils.jsx (fungsi murni)
  -> inpatient-admission-confirmation-step.jsx (tampilan)
```

Komponen existing yang dipakai ulang tanpa diubah: `BaseDetailCard`, `BaseButton`,
`BaseTextAreaField`, `ResourceFilterSelect`, `StatusBadge`, `InformationAlert`,
`ConfirmModal`. Tidak ada base component baru, tidak ada Axios instance baru, dan tidak
ada arsitektur state paralel — `serviceUnitSelect` diambil dari controller langkah Dokter
dan `patientClassSelect` dari controller langkah Pembayaran, bukan dibuat ulang.

**Delta kontrak yang dicatat.** `05-skema-tampilan.md` 3.9 menyebut `BaseEditorForm` untuk
wilayah Yang Masih Dapat Diubah. Source langkah Dokter yang bersebelahan memakai
`ResourceFilterSelect` plus `BaseTextAreaField` langsung, karena `BaseEditorForm` adalah
bagian internal `BaseEditorView` untuk halaman create/update penuh, bukan untuk satu wilayah
di dalam langkah alur. Sesuai urutan presedensi, source yang berlaku dan selisihnya dicatat
di sini.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kartu Pasien dan Perawatan menampilkan kalimat *"Mengambil detail episode rawat inap..."*, bukan layar kosong |
| Kosong | Kartu yang datanya belum ada menampilkan kalimatnya sendiri: *"Data pasien belum terbaca di layar ini."*, *"Tempat tidur belum dipesan untuk pasien ini."* |
| Gagal | Kalimat merah *"Gagal membaca detail episode dari server..."* beserta tombol **Coba Lagi** |
| Tanpa hak akses | `PUT /episodes/{id}` yang ditolak 403 menampilkan pesan server apa adanya pada baris merah di bawah isian; layar tidak mengarang kalimat pengganti |
| Episode belum ada | Seluruh ringkasan disembunyikan dan diganti kalimat yang menunjuk langkah Dokter |
| Sedang menyimpan | Tombol utama berbunyi *"Mengunci..."* dan seluruh isian dinonaktifkan |
| Sudah terkunci | Isian dinonaktifkan dan tombol utama berganti menjadi **Lanjut ke Cetak Persetujuan** |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Episode

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/inpatient-management/episodes/{id}` | Membaca ulang episode untuk keempat kartu ringkasan dan menyeed isian yang masih dapat diubah | `InpatientEpisode : Read` |
| `PUT` | `/v1/health-services/inpatient-management/episodes/{id}` | Menyimpan perubahan unit layanan, kelas perawatan, dan catatan admisi — **hanya bila ada yang berubah** | `InpatientEpisode : Update` |

**Yang sengaja TIDAK dipanggil:** `POST /v1/health-services/inpatient-management/bed-occupancies/placements`.
`RWI-DEC-076` menetapkan Kelayakan Penempatan diperiksa **ulang** saat pasien benar-benar
tiba; memanggilnya di meja admisi meloloskan tempat tidur yang keburu tidak layak. Nol
permintaan penempatan dibuktikan pada bagian 6.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint` | `0 errors`, 571 warning | `PASS` | Jumlah warning sama persis dengan garis dasar `FE-RWI-026`; nol warning pada berkas task ini |
| `npm run build` | `✓ Compiled successfully in 29.7s` | `PASS` | Keluaran perintah beserta `postbuild` yang lulus |
| `npm run test:unit` dan `node --test` | Tidak dijalankan | `NOT RUN` | Pengujian berbasis `.mjs` dilewati atas **instruksi eksplisit pengguna** pada sesi ini |
| **Verifikasi peramban Edge, alur penuh** | **37/37 PASS** | `PASS` | Rincian di bawah |

**Catatan waktu pengukuran.** Angka `npm run lint` dan `npm run build` di atas diukur ketika
seluruh perubahan task ini sudah selesai. Sesudah itu, working tree frontend menerima
**pekerjaan paralel yang bukan milik task ini** (implementasi pembatalan admisi). Lint ulang
yang discope hanya ke berkas milik task ini dijalankan sesudah pekerjaan paralel itu masuk dan
tetap `0 problems`. Angka lint dan build untuk keseluruhan repository perlu diukur ulang oleh
pemilik pekerjaan paralel tersebut, bukan diklaim di laporan ini.

### 6.1 Verifikasi manual di peramban

**Uji manual: `PASS`.**

Dijalankan dengan Microsoft Edge lewat Playwright, memakai pola `tests/e2e/route-smoke.spec.mjs`
yang sudah ada: cookie sesi dicetak lokal dan **seluruh** panggilan `/v1/**` dilayani stub.
Konsekuensinya **nol permintaan menyentuh backend bersama** dan nol data uji tertinggal di
sana. Alurnya disusuri betulan dari langkah Pembayaran sampai langkah terakhir.

| Skenario | Hasil |
| --- | --- |
| Ringkasan memuat pasien, penjamin, kelas, unit, DPJP, isolasi, dan tempat tidur | `PASS` |
| Penanda isolasi terbaca sebagai kata **Ya** | `PASS` |
| Sisa waktu pemesanan terbaca `00:14:5x` pada kartu Tempat Tidur | `PASS` |
| Status episode yang tampil adalah yang dijawab server | `PASS` |
| Keterangan isolasi **tidak** ikut tampil | `PASS` |
| Layar tidak menyatakan pasien sudah dirawat | `PASS` |
| Layar menyebut langkah berikutnya konfirmasi kedatangan di Papan Tempat Tidur | `PASS` |
| Wilayah yang dapat diubah menyebut sendiri apa yang terkunci | `PASS` |
| Mengubah unit lalu mengunci mengirim **tepat satu** `PUT` | `PASS` |
| Payload `PUT` mempertahankan `motherEpisodeId` | `PASS` |
| Mengunci **tanpa** mengubah apa pun mengirim **nol** `PUT`, dan tetap meneruskan ke cetak | `PASS` |
| Nol permintaan `/placements` di sepanjang alur | `PASS` |
| Dialog keluar menyebut nomor episode, statusnya, tempat tidur, dan Daftar Kerja Episode | `PASS` |
| Nol `pageerror` di seluruh layar | `PASS` |

Isi payload `PUT` yang benar-benar terkirim:

```json
{
  "serviceUnitId": "aaaaaaa1-0000-4000-8000-000000000009",
  "patientClassId": "bbbbbbb2-0000-4000-8000-000000000002",
  "motherEpisodeId": "ccccccc3-0000-4000-8000-000000000003",
  "notes": "Catatan admisi awal"
}
```

Seluruh permintaan yang terkirim di sepanjang alur, tanpa satu pun `/placements`:

```text
POST .../patient-encounters/admin
POST .../inpatient-management/episodes
GET  .../bed-occupancies/bed-board
GET  .../bed-occupancies/available-beds
POST .../bed-occupancies/reservations
GET  .../inpatient-management/episodes/{id}
PUT  .../inpatient-management/episodes/{id}
```

### 6.2 Checklist konsistensi UI

Keenam grep anti-regresi bersih pada berkas baru:

| Pemeriksaan | Hasil |
| --- | --- |
| Warna literal pada stylesheet baru | kosong |
| `font-size` / `font-weight` / `line-height` di luar token | kosong |
| `<button>` mentah atau kelas `.btn` Bootstrap | kosong |
| `<table>` tanpa `data-flat-table` | kosong |
| Utility typography Bootstrap di dalam blok tabel | kosong |
| `!important` baru | kosong |

**Tidak dijalankan:**

- `npm run test:unit`, `node --test`, dan `npm run test:e2e` — seluruh pengujian berbasis
  `.mjs` dilewati atas instruksi eksplisit pengguna pada sesi ini.
- Verifikasi peramban pada 6.1 dijalankan sebagai **uji manual berbantuan peramban**, bukan
  sebagai suite test. Skripnya tidak ditambahkan ke repository dan sudah dihapus setelah
  verifikasi selesai.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Ringkasan memuat pasien, penjamin, kelas, unit, DPJP, kebutuhan isolasi, dan tempat tidur yang dipesan | Terpenuhi | Empat kartu pada `inpatient-admission-confirmation-step.jsx`; verifikasi peramban baris "027-AC1" |
| 2. Perubahan isian admisi tersimpan lewat `PUT /episodes/{id}` | Terpenuhi | `lockAdmission` pada `use-inpatient-admission-confirmation.jsx`; payload `PUT` yang benar-benar terkirim ada di bagian 6.1, beserta bukti nol `PUT` ketika tidak ada yang berubah |
| 3. Layar **tidak** memanggil `POST /placements` dan **tidak** menyatakan pasien sudah dirawat | Terpenuhi | Daftar permintaan pada 6.1 tanpa satu pun `/placements`; kalimat `INPATIENT_CONFIRMATION_MESSAGES.notPlacedYet` terbukti tampil |
| 4. Layar menyatakan langkah berikutnya adalah konfirmasi kedatangan pada papan tempat tidur | Terpenuhi | `INPATIENT_CONFIRMATION_MESSAGES.nextStepNotice`; verifikasi peramban baris "027-AC4" |
| 5. Menutup alur setelah titik tulis 1 memunculkan peringatan yang menyebut episode `Draft` sudah terbentuk dan dapat dilanjutkan dari daftar kerja | Terpenuhi | `use-inpatient-admission-exit-guard.jsx` beserta `ConfirmModal` di level halaman; verifikasi peramban baris "027-AC5" |

**Definition of Done — "kelima kriteria lulus; e2e ada dan lulus":**

- Kelima kriteria **lulus** dan dibuktikan runtime.
- Butir "e2e ada" **belum terpenuhi** dalam bentuk berkas `tests/e2e/` yang ditambahkan ke
  repository. Dua alasannya disebut apa adanya: repository tidak memiliki
  `playwright.config.*` sehingga suite resminya tidak dapat dijalankan, dan pengguna secara
  eksplisit meminta pengujian berbasis `.mjs` dilewati pada sesi ini. Pembuktian perilakunya
  tetap dilakukan lewat peramban sungguhan seperti pada 6.1.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `npm run lint` menghasilkan 571 warning, sama persis dengan garis dasar dan nol pada berkas task ini |
| Masalah yang diketahui | **`UNRELATED EXISTING ISSUE`** — panel *"Penjamin dari Langkah Pembayaran"* pada langkah Dokter (`FE-RWI-025`) merender **"Data tidak ditemukan."**. Penyebabnya `BaseDetailCard` tidak menerima `children`, sedangkan panel itu mengirim isinya sebagai `children`. Terlihat pada verifikasi peramban. Tidak diperbaiki karena di luar scope task ini; layak menjadi butir perbaikan tersendiri |
| Masalah yang diketahui | Setelah episode dibuat, keadaannya hanya hidup selama sesi layar. Memuat ulang halaman pada langkah Konfirmasi mengembalikan kalimat "episode belum terbaca" karena hanya `entry`, `step`, dan `patientId` yang tersimpan pada URL. Perilaku ini sama dengan langkah tempat tidur dan sejalan dengan `RWI-UI-GAP-003`; melanjutkan admisi tertinggal dimiliki `FE-RWI-032` |
| Masalah yang diketahui | Dialog keluar alur menjaga aksi keluar yang disediakan layar dan penutupan tab. Perpindahan halaman lewat menu samping tidak dapat ditahan karena App Router tidak menyediakan kait pembatalan navigasi; peringatan bawaan peramban dipasang untuk penutupan tab dan pemuatan ulang, tetapi kalimatnya dimiliki peramban |
| Dependency backend | `NONE`. Ketiga endpoint yang dipakai sudah ada sejak `BE-RWI-008` dan `BE-RWI-009` |
| Perubahan sampingan | `NONE`. Dua kolom yang ditambahkan pada `normalizeEpisodeDetail` bersifat tambahan dan tidak mengubah kolom yang sudah ada; suite `inpatient-episode-detail.test.mjs` tetap 16/16 |
| Interupsi | `NONE` |
| Status Git | Berkas yang **diubah task ini**: `inpatient-admission-view.jsx`, `inpatient-episode-detail-view.jsx`, `inpatient-admission-flow-constants.jsx`, `inpatient-episode-constants.jsx`, `inpatient-admission-encounter.service.js`, `inpatient-admission.module.css`, `inpatient-admission-encounter-utils.jsx`, `inpatient-episode-utils.jsx`, ditambah sembilan berkas baru dan satu route baru. Working tree juga memuat **pekerjaan paralel yang bukan milik task ini** — implementasi pembatalan admisi pada `inpatient-episode-worklist-view.jsx`, `use-inpatient-episode-worklist.jsx`, `use-inpatient-episode-detail.jsx`, serta bagian pembatalan pada `inpatient-episode-detail-view.jsx`, `inpatient-episode-utils.jsx`, dan `inpatient-episode-constants.jsx`. Pekerjaan itu tidak disentuh, tidak dibatalkan, dan tidak diklaim di sini. Tidak ada stage, commit, maupun push |
| Langkah berikutnya | `FE-RWI-034` — membongkar layar admisi lama sekarang bergantung pada task ini dan sudah dapat dimulai |
