# Laporan Perubahan Frontend — `FE-LAB-03`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-03` |
| Judul | Layar alasan penolakan sampel |
| Slice | `S11` — alasan penolakan sampel (`roadmap/frontend-roadmap.md` bagian 3, gelombang `MVP-0`) |
| Roadmap | `docs/module-blueprints/laboratorium/roadmap/frontend-roadmap.md` bagian 3 |
| Trace | `FR-07.5`, `FR-06.1`, `FR-06.2`; `LAB-DEC-019`, `BR-15`; `LAB-FE-012`, `LAB-FE-014`; `VAL-36`, `VAL-37`, `VAL-38`; `03-frontend-architecture.md` bagian 3.5, 4, dan 5 |
| Contract version | `LAB-API-v1` r3 — `approved`, dikunci 2026-09-02. Grup Lab Rejection Reason |
| Wewenang UI | `LAB-FE-012` **invariant keselamatan** — penanda kesalahan internal dan penanda wajib catatan terlihat terkunci sejak awal, bukan sekadar gagal saat disimpan. `LAB-FE-014` konvensi project — layar data induk berada di `health-services/master-data/`. `LAB-FE-002` `DEV_DISCRETION` untuk tata letak dan pilihan komponen |
| Dependency | `FE-LAB-01` — **selesai**. Endpoint dari `BE-LAB-06` — **selesai**, dan ketujuh endpointnya diverifikasi langsung pada source backend |
| Klasifikasi | `HEAVY` — skor 10: repository 2, berkas diperiksa 2, berkas diubah 2, logika bisnis 1, kontrak API 1, database 0, keamanan 1, UI/workflow 1. Duduk di batas bawah `HEAVY`, dan angkanya berasal dari jumlah berkas serta rentang dua repository |
| Task mode | `FRONTEND` — frontend target tulis, backend strict read-only sebagai sumber kebenaran kontrak |
| Target tulis | `QuilvianSystemFrontendDev` — `health-services/master-data/lab-rejection-reasons` pada lapisnya, `src/lib/state/store.jsx`, `src/utils/menu-sidebar/menu-items.jsx`, dan satu berkas uji; serta `NewQuilvianSystemBackend` — **hanya** `docs/module-blueprints/laboratorium/task/report/frontend/FE-LAB-03.md` beserta tautan buktinya pada `roadmap/` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit frontend saat dikerjakan | `443270f3f`, branch `YogaV2`, upstream `origin/YogaV2` |
| Commit backend yang dijadikan rujukan | `2dfc4f2`, branch `yoga` |
| Tanggal | 2026-09-04 |
| Status | **Selesai.** Layar daftar, formulir tambah, dan formulir ubah berdiri; kedua penanda sistem terlihat terkunci pada keduanya; aksi aktif/nonaktif dan jalur penyetelan penanda tersedia. Dua batas yang berada **di luar** kendali task ini dicatat pada bagian 8 |

---

## 1. Keadaan yang ditemukan di awal

**Tidak ada satu pun layar alasan penolakan di frontend.** Yang ada hanya jalur baca lama
`GET /lab-specimens/rejection-reasons`, yang dipakai petugas saat menolak sampel dan memang
tidak dimaksudkan untuk pengelolaan. Belum ada berkas pengelolaannya sama sekali.

**Ketujuh endpoint pengelolaannya sudah ada.** Pemeriksaan langsung pada
`LabRejectionReasonController.cs` menemukan `GET /filters/metadata`, `GET /summary`,
`GET /`, `POST /`, `PUT /{id}`, `PUT /{id}/activation`, dan `PUT /{id}/system-flags`.
Statusnya pada `contracts/api-contract.md` masih tertulis `Rencana (belum tersedia)` untuk
lima di antaranya — selisih pembukuan yang sama dengan yang sudah dicatat `FE-LAB-02`, dan
sekali lagi hanya menyentuh kolom status, bukan bentuk kontraknya.

**Satu hal yang menentukan bentuk layar: grup ini tidak punya `GET /{id}`.** Tidak ada
endpoint detail satu baris. Konsekuensinya dua:

1. Fitur ini **tidak punya halaman detail**, dan itu memang tidak diminta roadmap — cakupannya
   menyebut daftar, formulir tambah dan ubah, pengurutan, tombol aktif/nonaktif, dan penanda
   terkunci.
2. Formulir ubah tidak dapat memuat barisnya sendiri lewat penunjuk. Ia mengambilnya dari
   daftar yang sudah dimuat; bila alamatnya dibuka langsung, daftar dimuat ulang sekali lalu
   barisnya dicari. Bila tetap tidak ditemukan, formulirnya **tidak dirender** dan pengguna
   diminta membuka ulang dari daftar — bukan disuguhi formulir kosong yang berisiko menyimpan
   data yang salah.

**Dua perilaku backend yang menentukan bentuk isian.**

| Perilaku | Akibatnya di layar |
| --- | --- |
| `CreateLabRejectionReasonRequest` **tidak memuat** ruas penanda kesalahan internal maupun penanda wajib catatan | Kedua penanda tidak mungkin diisi saat menambah baru, sehingga menampilkannya sebagai isian akan menyesatkan |
| `PUT /{id}` menolak `403` bila permintaannya menyelipkan salah satu penanda (`VAL-37`) | Kedua penanda juga tidak boleh menjadi isian saat mengubah; satu-satunya jalur penyetelan adalah `PUT /{id}/system-flags` |

Keduanya sejalan dengan `LAB-FE-012`: yang dituntut bukan sekadar penolakan saat menyimpan,
melainkan kolom yang **terlihat terkunci sejak awal**.

**Satu kemampuan yang tidak dimiliki frontend.** Aplikasi ini tidak menyimpan daftar
permission per pengguna. Yang tersedia di sisi klien hanya **peran** — lewat
`selectUserRole` dan cookie sesi. Tidak ada satu pun tempat yang dapat menjawab "apakah
pengguna ini memegang `LabRejectionReason : SystemFlag`". Bagaimana batas itu disiasati
dijelaskan pada bagian 3.3.

---

## 2. Proses bisnis dari sisi pengguna

**Siapa penggunanya.** Kepala instalasi laboratorium menambah alasan penolakan, mengubah nama,
keterangan, dan urutan tampilnya, serta mengaktifkan atau menonaktifkannya. Administrator
sistem — dan hanya dia — menyetel kedua penanda sistem.

**Kenapa pemisahan itu ada.** Penanda kesalahan internal menentukan **siapa menanggung biaya
pengambilan ulang**. Bila sebuah alasan ditandai kesalahan internal, biaya pengambilan ulangnya
ditanggung rumah sakit; bila tidak, biaya itu dapat dibebankan kepada pasien. Karena itu
penandanya tidak diserahkan kepada yang mengelola daftarnya sehari-hari.

### 2.1 Mengelola daftar alasan

1. Pengguna membuka **Pelayanan Kesehatan → Master Data → Alasan Penolakan Sampel**.
2. Layar daftar menampilkan lima kartu rekap — total, aktif, nonaktif, berapa yang ditandai
   kesalahan internal, dan berapa yang menuntut catatan — lalu tabelnya.
3. Tabel menampilkan urutan tampil, kode, nama, keterangan, kedua penanda sistem sebagai
   lencana, status, dan kolom aksi.
4. Tombol **+ Tambah Alasan Penolakan** membuka formulir. Kode alasan diketik pengguna dan
   dinormalkan menjadi huruf kapital — sama seperti yang dilakukan backend — supaya yang
   terlihat saat mengetik sama dengan yang tersimpan.
5. Urutan tampil menentukan susunan alasan pada daftar pilihan yang dilihat petugas saat
   menolak sampel.

### 2.2 Kolom yang terlihat terkunci

Pada formulir tambah maupun ubah, di bawah isian, berdiri panel **Penanda Sistem** berisi
kedua penanda:

- masing-masing menampilkan nilainya yang berlaku sebagai lencana — misalnya "Bukan kesalahan
  internal" — dalam bidang bergaris putus-putus;
- masing-masing membawa label **Terkunci — hanya administrator sistem**;
- di bawahnya satu kotak peringatan menjelaskan kenapa: kedua penanda menentukan siapa
  menanggung biaya pengambilan ulang, sehingga penyetelannya hanya dapat dilakukan
  administrator sistem lewat aksi **Setel Penanda Sistem** pada halaman daftar.

Pengguna karena itu tahu **sebelum mencoba**, bukan setelah gagal menyimpan.

### 2.3 Mengaktifkan dan menonaktifkan

Kolom aksi memuat tombol **Nonaktifkan** untuk baris yang aktif dan **Aktifkan** untuk yang
nonaktif. Keduanya meminta konfirmasi lebih dulu, dan pesan konfirmasinya menyebut akibatnya
apa adanya: alasan yang dinonaktifkan tidak lagi muncul saat petugas menolak sampel.

### 2.4 Menyetel penanda sistem

Bagi pengguna berperan administrator, kolom aksi memuat satu tombol tambahan **Setel Penanda
Sistem**. Tombol itu membuka kotak berisi dua kotak centang dan satu isian alasan penyetelan,
yang tersimpan pada catatan log agar keputusannya dapat ditelusuri.

### 2.5 Jalur yang tidak normal

| Keadaan | Yang dialami pengguna |
| --- | --- |
| Kode alasan sudah dipakai data lain | Backend menjawab `409` beserta pesan `VAL-36` — "Kode alasan ini sudah dipakai data lain, jadi tidak bisa disimpan." — dan pesannya muncul apa adanya sebagai notifikasi merah |
| Menonaktifkan alasan aktif terakhir | Backend menjawab `422` beserta pesan `VAL-38` — "Sekurang-kurangnya satu alasan penolakan harus tetap aktif." — dan pesannya muncul apa adanya. Konfirmasi sebelumnya sudah menyebutkan syarat itu |
| Bukan pemegang kewenangan menekan Setel Penanda Sistem | Backend menjawab `403` beserta pesan `VAL-37`, dan pesannya muncul apa adanya. Kotak penyetelan sendiri sudah memuat keterangan bahwa penolakan berarti akun belum memegang kewenangan itu |
| Formulir ubah dibuka langsung dari alamatnya | Daftar dimuat ulang sekali lalu barisnya dicari. Bila tidak ditemukan, muncul ajakan membuka ulang dari daftar data, dan formulirnya tidak dirender |
| Backend menambah ruas terkunci ketiga | Layar membandingkan `SystemFlagFields` dari metadata dengan ruas yang dikenalinya, dan menampilkan peringatan kuning berisi nama ruas yang belum dikenali — bukan diam-diam membiarkannya dapat disunting |
| Tanpa hak akses | Seluruh isi halaman diganti layar "Ups! Akses Ditolak" |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Tata kelola dan aturan.** `AGENTS.md` frontend; `CLAUDE.md` frontend;
`rules/frontend/frontend-architecture.md`; `rules/frontend/master-data-feature-standard.md`;
`rules/frontend/base-component-catalog.md`; `rules/frontend/base-component-decision-gate.md`;
`rules/frontend/design-tokens.md`; `rules/frontend/ui-consistency-checklist.md`;
`rules/frontend/test-policy.md`; `rules/frontend/REPORT_TEMPLATE.md`.

**Blueprint dan kontrak.** `roadmap/frontend-roadmap.md` bagian 3;
`03-frontend-architecture.md` bagian 3.5 dan 5; `contracts/api-contract.md` grup Lab
Rejection Reason; `contracts/validation-matrix.md` `VAL-36` .. `VAL-38`;
`testing/acceptance-test-matrix.md` `AC-26`; `task/report/backend/BE-LAB-06.md`.

**Backend sebagai sumber kebenaran kontrak — strict read-only.**
`Areas/HealthServices/LaboratoryManagement/Controllers/LabRejectionReasonController.cs`;
`.../DTOs/LabRejectionReasonDtos.cs`; `.../DTOs/LabFilterAndSummaryDtos.cs`;
`.../Services/LabRejectionReasonService.cs`; `.../Services/LabFilterMetadataFactory.cs`.

**Frontend sebagai acuan pola.** Fitur `health-services/master-data/lab-value-bounds` yang
baru saja berdiri pada `FE-LAB-02`, modul rujukan `hr/master-data/job-level`,
`components/view/health-services/billing-management/master-data/register/register-view.jsx`
sebagai preseden aksi status pada halaman daftar,
`components/features/base-features/` (confirm-modal, base-form-control, data-table,
data-filter, status-badge, information-alert, toast-stack),
`lib/state/slice/auth/login-slice.jsx` untuk ketersediaan peran di sisi klien, dan
`src/app/globals.css` untuk token desain.

### 3.2 Berkas yang berubah

**Delapan berkas inti:**

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/master-data/lab-rejection-reasons/lab-rejection-reason-constants.jsx` | `LAB_REJECTION_REASON_CONFIG` beserta `LAB_REJECTION_REASON_SYSTEM_FLAG_FIELDS` dan salinan teks kewenangannya. Nama kedua ruas terkunci ditulis sama persis dengan `SystemFlagFields` yang diumumkan backend |
| `src/lib/state/slice/health-services/master-data/master-data-lab-rejection-reason-slice.jsx` | Tujuh thunk yang memetakan tepat ke tujuh endpoint. Tidak ada thunk detail, karena `GET /{id}` memang tidak ada |
| `src/utils/health-services/master-data/lab-rejection-reasons/lab-rejection-reason-utils.jsx` | Fungsi murni: pembacaan payload, pembentukan form, validasi, tiga pembentuk payload yang terpisah, pemeriksaan selisih ruas terkunci, dan penentuan tampil-tidaknya aksi penanda sistem |
| `src/lib/hooks/.../use-master-data-lab-rejection-reason.jsx` | Controller halaman daftar: penyaring, aksi aktif/nonaktif, dan kotak penyetelan penanda sistem |
| `src/lib/hooks/.../use-master-data-lab-rejection-reason-editor.jsx` | Controller formulir tambah dan ubah, termasuk pemuatan ulang daftar sebagai pengganti endpoint detail yang tidak ada |
| `src/components/view/.../master-data-lab-rejection-reason-view.jsx` | Layar daftar beserta kolom aksi dan kedua kotak konfirmasinya |
| `src/components/view/.../add/lab-rejection-reason-form-view.jsx` | Formulir tambah dan ubah beserta panel penanda sistem yang terkunci |
| `src/style/health-services/master-data/lab-rejection-reasons/lab-rejection-reason.module.css` | Kolom aksi, panel, penanda terkunci, dan isi kotak penyetelan. Seluruh nilai memakai token |

**Lima berkas route:**

`src/app/health-services/master-data/lab-rejection-reasons/` — `page.jsx`,
`lab-rejection-reasons-client.jsx`, `create/page.jsx`, `[slug]/route-token.js`, dan
`[slug]/update/page.jsx`. Tidak ada `[slug]/page.jsx` karena tidak ada halaman detail.

**Satu berkas uji dan dua berkas yang disunting:**

| Berkas | Perubahan |
| --- | --- |
| `tests/unit/lab-rejection-reason-utils.test.mjs` | Sembilan uji terhadap fungsi murni, empat di antaranya menjaga `LAB-FE-012`, `BR-15`, dan `VAL-37` |
| `src/lib/state/store.jsx` | Satu baris import dan satu baris pendaftaran reducer dengan kunci `masterDataLabRejectionReason` |
| `src/utils/menu-sidebar/menu-items.jsx` | Satu butir menu **Alasan Penolakan Sampel** tepat setelah Batas Nilai Pemeriksaan |

### 3.3 Kepatuhan arsitektur frontend

**Alur dependensi tidak dibalik**, dan bentuknya mengikuti standar fitur master data dengan
empat selisih yang disengaja:

| Selisih | Alasan |
| --- | --- |
| **Tujuh thunk, bukan sembilan** | Grup ini tidak punya `GET /options`, `GET /{id}`, maupun `DELETE /{id}`. Yang ada sebagai gantinya `PUT /{id}/activation` dan `PUT /{id}/system-flags` |
| **Tanpa halaman detail** | Tidak ada endpoint detail. Roadmap juga tidak memintanya. Barisnya dibuka langsung ke formulir ubah lewat klik dua kali maupun tombol Perbarui |
| **Aksi aktif/nonaktif dan penanda sistem berada di halaman daftar** | `AGENTS.md` melarang aksi Aktifkan/Nonaktifkan pada halaman detail master data — dan halaman detail memang tidak ada. Polanya sudah ada pada master data Register Kasir |
| **Satu berkas CSS Module ditambahkan** | Alasannya sama seperti `FE-LAB-02`: kolom aksi dan penanda terkunci menuntut penataan yang tidak disediakan `base-data-components.module.css`, dan alternatifnya inline style yang dilarang checklist UI |

**Bagaimana `LAB-FE-012` ditegakkan — empat lapis.**

1. **Tidak pernah menjadi isian.** `getVisibleFields` untuk kedua mode tidak pernah memuat
   `isInternalHospitalError` maupun `requiresNote`. Dijaga uji `S1`.
2. **Tidak pernah ikut pada payload.** `buildCreatePayload` dan `buildUpdatePayload` tidak
   menyertakan keduanya, sehingga permintaan ubah tidak pernah memicu `403` `VAL-37` karena
   kelalaian layar. Dijaga uji `S3` dan `S4`.
3. **Terlihat terkunci, dengan nilainya.** Panel Penanda Sistem menampilkan nilai yang berlaku
   sebagai lencana, label **Terkunci — hanya administrator sistem**, keterangan kenapa, dan
   kotak peringatan yang menyebutkan jalur penyetelan yang benar.
4. **Selisih ruas terkunci terbaca.** Bila backend suatu saat mengumumkan ruas terkunci
   ketiga pada `SystemFlagFields`, layar menampilkan peringatan berisi namanya alih-alih
   diam-diam membiarkannya dapat disunting. Dijaga uji `S6`.

**Bagaimana kewenangan administrator didekati — dan batasnya.**

Roadmap meminta kolom itu "aktif bagi administrator sistem". Yang dapat dikerjakan frontend
dibatasi kenyataan berikut: **tidak ada daftar permission per pengguna di sisi klien**, dan
kedua penanda memang tidak dapat disetel lewat `POST` maupun `PUT` biasa oleh siapa pun —
termasuk administrator. Karena itu:

- ruas pada formulir **terkunci bagi semua orang**, dan itu benar secara teknis maupun
  perilaku;
- administrator menyetelnya lewat aksi terpisah **Setel Penanda Sistem** yang memanggil
  `PUT /{id}/system-flags`, satu-satunya jalur yang memang dapat mengubahnya;
- aksi itu **hanya ditampilkan bagi peran administrator**, memakai peran sebagai pendekatan
  atas permission — sejalan dengan `03-frontend-architecture.md` bagian 4 yang meminta kontrol
  tanpa kewenangan disembunyikan, bukan dibiarkan lalu gagal;
- ketika peran tidak terbaca, aksinya tetap ditampilkan dan backend yang menolak.
  Menyembunyikannya dalam keadaan itu justru menutup jalan bagi administrator yang sah.

Batas ini dicatat pada bagian 8 sebagai kebutuhan yang sebenarnya berada di luar task ini:
selama frontend belum menerima daftar permission, penyembunyian tombol hanya dapat sedekat
peran.

**Gerbang keputusan base component.**

```text
UI GATE: 8 elemen — REUSE 7, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0
```

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Header halaman | `Hero` | `.../hero.jsx` | `REUSE` | Dipakai apa adanya |
| Kartu rekap | `SummaryGrid` | `.../summary-grid.jsx` | `REUSE` | Lima kartu dari `GET /summary` |
| Penyaring dan pencarian | `DataFilter`, `FilterDatePicker`, `FilterSelect` | `.../data-filter.jsx` | `REUSE` | Urutan baku dipertahankan |
| Tabel daftar | `DataTable` | `.../data-table.jsx` | `REUSE` | `sortLatestFirst={false}` supaya urutan tampil tidak digeser pengurutan bawaan |
| Lencana penanda dan status | `StatusBadge` | `.../status-badge.jsx` | `REUSE` | Dipakai untuk status dan kedua penanda sistem |
| Konfirmasi aktif/nonaktif | `ConfirmModal` | `.../confirm-modal.jsx` | `REUSE` | Pesannya menyebut akibat dan syarat `VAL-38` |
| Formulir tambah dan ubah | `BaseEditorView` | `.../base-editor-view.jsx` | `REUSE` | `afterForm` menampung panel penanda terkunci |
| Kotak penyetelan penanda sistem | `ConfirmModal` + `BaseCheckboxField` + `BaseTextAreaField` | `.../base-form-control.jsx` | `COMPOSE` | Dirangkai sebagai isi `children` `ConfirmModal` |

**Keputusan untuk satu-satunya baris yang bukan `REUSE`:**

> **Keputusan: kotak penyetelan penanda sistem**
>
> - **A. Rangkai isian ke dalam `children` `ConfirmModal` — Rekomendasi.** Tidak ada komponen
>   baru dan tidak ada base component yang berubah. Perilaku modal, tombol, dan penguncian
>   selama proses tetap milik `ConfirmModal`, dan yang ditambahkan hanya dua kotak centang
>   serta satu isian alasan yang sudah punya kontrak visualnya sendiri.
> - **B. Buat route tersendiri untuk penyetelan penanda.** Lebih longgar ruangnya, tetapi
>   menambah satu layar untuk perubahan dua kotak centang, dan menjauhkan penyetelan dari
>   baris yang sedang dilihat.
> - **C. Jadikan kedua penanda dapat disunting langsung pada tabel.** Paling singkat, tetapi
>   menghapus jeda konfirmasi pada penanda yang menentukan siapa menanggung biaya — persis
>   yang tidak diinginkan `LAB-DEC-019`.
>
> Opsi **A** yang dijalankan.

**Token desain.** Berkas style barunya tidak memuat satu pun nilai warna, radius, bayangan,
atau jarak sebagai literal. Tidak ada `!important`, tidak ada inline style untuk nilai statis,
dan tidak ada selector yang menyasar typography komponen bersama.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kartu rekap menjadi kerangka abu-abu; tabel menampilkan "Mengambil data alasan penolakan..."; seluruh tombol aksi terkunci selama proses |
| Kosong | "Data alasan penolakan tidak ditemukan." disertai ajakan mengganti penyaring atau menambah data baru |
| Gagal | Pesan dari server ditampilkan apa adanya — kotak merah pada halaman daftar, notifikasi merah untuk kegagalan aksi. `VAL-36`, `VAL-37`, dan `VAL-38` sampai ke pengguna dengan kalimat aslinya |
| Tanpa hak akses | Seluruh isi halaman diganti layar "Ups! Akses Ditolak" lewat `AccessDeniedGate` |
| Tanpa kewenangan penanda sistem | Tombol Setel Penanda Sistem tidak dirender bagi peran non-administrator |
| Kirim ganda | Seluruh tombol aksi terkunci sejak ditekan sampai jawaban datang; permintaan lama dibatalkan ketika penyaring berubah |
| Baris tidak ditemukan | Formulir ubah tidak dirender; muncul ajakan membuka ulang dari daftar data |
| Kontrak berubah di luar sepengetahuan layar | Peringatan kuning berisi nama ruas terkunci yang belum dikenali |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Rejection Reason

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-rejection-reasons/filters/metadata` | Ukuran halaman dan daftar ruas terkunci yang dibandingkan layar | `LabRejectionReason : Read` |
| `GET` | `/v1/health-services/laboratory-management/lab-rejection-reasons/summary` | Lima kartu rekap | `LabRejectionReason : Read` |
| `GET` | `/v1/health-services/laboratory-management/lab-rejection-reasons` | Tabel daftar, sekaligus sumber data formulir ubah karena tidak ada endpoint detail | `LabRejectionReason : Read` |
| `POST` | `/v1/health-services/laboratory-management/lab-rejection-reasons` | Menyimpan alasan baru | `LabRejectionReason : Create` |
| `PUT` | `/v1/health-services/laboratory-management/lab-rejection-reasons/{id}` | Mengubah nama, keterangan, dan urutan tampil — **tanpa** kode dan **tanpa** penanda sistem | `LabRejectionReason : Update` |
| `PUT` | `/v1/health-services/laboratory-management/lab-rejection-reasons/{id}/activation` | Tombol Aktifkan dan Nonaktifkan | `LabRejectionReason : Update` |
| `PUT` | `/v1/health-services/laboratory-management/lab-rejection-reasons/{id}/system-flags` | Aksi Setel Penanda Sistem | `LabRejectionReason : SystemFlag` |

Jalur baca lama `GET /lab-specimens/rejection-reasons` **tidak** disentuh task ini. Ia tetap
menjadi jalur bagi petugas yang sedang menolak sampel, dan menjadi cakupan `FE-LAB-07`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Selesai tanpa satu pun error | `PASS` | Kode keluar `0` |
| `npx eslint` pada berkas baru, termasuk peringatan | 0 error, 3 peringatan | `EXISTING WARNING` | Dua `react-hooks/set-state-in-effect` dari pola resolusi token route yang sama dengan modul rujukan, dan satu catatan React Compiler `Compilation Skipped`. Dua peringatan `react-hooks/refs` yang sempat muncul **diperbaiki lebih dulu**: penanda kemajuan pemuatan ulang dipindahkan dari `useRef` menjadi state, karena nilainya memang ikut menentukan apa yang dirender |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 457 uji, 457 lulus, 0 gagal | `PASS` | Seluruh suite dijalankan, termasuk sembilan uji baru |
| Uji `S1` — penanda sistem tidak pernah menjadi isian | Lulus | `PASS` | `getVisibleFields` untuk mode tambah maupun ubah tidak pernah memuat kedua penanda (`LAB-FE-012`) |
| Uji `S2` — kode alasan hanya pada formulir tambah | Lulus | `PASS` | `BR-15` dijaga sejak di layar |
| Uji `S3` — payload tambah tanpa penanda sistem, kode dinormalkan | Lulus | `PASS` | Menjawab baris `AC-26` "penanda bernilai bawaan, tidak dapat diisi dari permintaan" |
| Uji `S4` — payload ubah tanpa kode dan tanpa penanda sistem | Lulus | `PASS` | Isian penanda yang diselipkan pun tidak ikut terkirim, sehingga `VAL-37` tidak pernah terpicu oleh kelalaian layar |
| Uji `S6` — selisih ruas terkunci terbaca | Lulus | `PASS` | Ruas ketiga yang belum dikenali dikembalikan sebagai selisih, bukan diabaikan |
| Uji `S7` — aksi penanda sistem hanya tampil bagi administrator | Lulus | `PASS` | Peran non-administrator tidak melihat tombolnya; peran yang tidak terbaca tetap melihatnya |
| Uji `S8` dan `S9` — validasi isian dan label penanda | Lulus | `PASS` | Isian wajib, tag HTML ditolak, dan label penanda memakai bahasa yang dipahami petugas |
| `npm run build` | Selesai, termasuk `postbuild` standalone | `PASS` | Kode keluar `0`. Ketiga route terbit: `/lab-rejection-reasons`, `/create`, dan `/[slug]/update` |
| Grep anti-regresi warna literal, `!important`, dan inline style statis | Tidak ada temuan | `PASS` | Seluruh nilai style memakai token `var(...)` |
| Grep anti-regresi tombol non-base dan tabel mentah | Tidak ada temuan | `PASS` | Seluruh tombol `BaseButton`; seluruh tabel `DataTable` |

**Uji manual: `NOT FEASIBLE`.**

1. Ketiga aturan yang paling menentukan — `VAL-36`, `VAL-37`, dan `VAL-38` — **hanya dapat
   dimunculkan oleh jawaban server yang sebenarnya**, dan lingkungan sesi ini tidak
   menyediakan backend Laboratorium maupun sesi login yang sah.
2. Pembuktian "terkunci bagi kepala instalasi, dapat disetel administrator sistem" menuntut
   **dua akun dengan kewenangan berbeda**. Satu sesi tidak cukup.
3. Peran pemegang `LabRejectionReason : SystemFlag` sendiri **belum ditetapkan** manajemen
   rumah sakit — dicatat `BE-LAB-06` dan `roadmap/traceability.md` bagian 4.3 — sehingga jalur
   penyetelan belum dapat dijalankan siapa pun secara sah.

Penggantinya bukan asumsi: empat dari sembilan uji unit menjaga persis aturan yang tidak boleh
dilanggar layar, dan keluaran build membuktikan ketiga route terbit.

**Tidak dijalankan:**

| Pemeriksaan | Alasan |
| --- | --- |
| `npm run test:e2e` | Tidak diminta task, dan lingkungan tidak menyediakan backend maupun dua akun berbeda kewenangan |
| `npm run test:uat` | Hanya dijalankan bila diminta secara eksplisit |
| `npm run dev` | `AGENTS.md` melarang menjalankan development server tanpa kebutuhan konkret |

---

## 7. Acceptance criteria dan Definition of Done

**`AC-26` — lima barisnya, satu per satu.**

| Baris `AC-26` | Status di sisi layar | Bukti |
| --- | --- | --- |
| Kepala instalasi menambah alasan; penanda bernilai bawaan dan tidak dapat diisi dari permintaan | **Terpenuhi** | Uji `S1` dan `S3`: kedua penanda bukan isian, dan tidak ikut pada payload tambah |
| **Gagal** — kepala instalasi mencoba mengubah penanda kesalahan internal, dijawab `403` `VAL-37` | **Terpenuhi, dan dicegah lebih awal** | Uji `S4`: layar tidak pernah mengirim kedua penanda pada permintaan ubah, sehingga jalur ini tidak dapat dipicu dari UI. Bila tetap terjadi, pesan `VAL-37` ditampilkan apa adanya |
| Administrator sistem menyetel penanda kesalahan internal | **Terpenuhi** | Aksi Setel Penanda Sistem memanggil `PUT /{id}/system-flags` beserta alasan penyetelan yang tersimpan pada log |
| **Gagal** — menambah alasan dengan kode yang sudah dipakai, dijawab `409` `VAL-36` | **Terpenuhi** | Pesan server ditampilkan apa adanya sebagai notifikasi merah |
| **Gagal** — menonaktifkan alasan terakhir yang masih aktif, dijawab `422` `VAL-38` | **Terpenuhi** | Pesan server ditampilkan apa adanya, dan konfirmasi sebelumnya sudah menyebutkan syaratnya |

**Definition of Done.**

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Layar ada di `master-data/lab-rejection-reasons/` | **Terpenuhi** | Ketiga route terbit pada keluaran build, seluruhnya di bawah `health-services/master-data/lab-rejection-reasons` sesuai `LAB-FE-014` |
| Penanda terkunci terlihat | **Terpenuhi** | Panel Penanda Sistem menampilkan nilai yang berlaku, label terkunci, keterangan, dan jalur penyetelan yang benar — pada formulir tambah maupun ubah. Dijaga uji `S1` |
| Seluruh pesan gagal tampil dalam bahasa yang dipahami petugas | **Terpenuhi** | Pesan `VAL-36`, `VAL-37`, dan `VAL-38` diteruskan apa adanya; keduanya memang sudah ditulis dalam kalimat yang dapat dibaca petugas |

Tidak ada butir DoD yang belum terpenuhi.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tiga peringatan lint tersisa: dua `react-hooks/set-state-in-effect` yang sama polanya dengan modul rujukan, dan satu catatan React Compiler. Dua peringatan `react-hooks/refs` diperbaiki lebih dulu, bukan dibiarkan |
| Masalah yang diketahui — 1 | **Grup endpoint ini tidak punya `GET /{id}`.** Akibatnya fitur ini tidak dapat punya halaman detail, dan formulir ubah harus memuat ulang daftar untuk menemukan barisnya. Bentuk ini berfungsi dan gagal dengan sopan, tetapi berbeda dari fitur master data lain. Bila backend menambahkan `GET /{id}`, formulir ubah dapat disederhanakan dan halaman detail dapat dibangun. **Perbaikannya milik backend** |
| Masalah yang diketahui — 2 | **Frontend tidak menerima daftar permission per pengguna.** Yang tersedia hanya peran, sehingga penyembunyian aksi Setel Penanda Sistem hanya dapat sedekat peran — bukan sedekat `LabRejectionReason : SystemFlag` yang sebenarnya. Selama itu belum ada, sebagian pengguna berperan administrator yang tidak memegang kewenangan itu masih akan melihat tombolnya dan menerima `403`. Perbaikannya menuntut backend mengirimkan daftar permission pada sesi login |
| Masalah yang diketahui — 3 | **Status kontrak usang**, sama seperti yang sudah dicatat `FE-LAB-02`: lima endpoint grup ini masih tertulis `Rencana (belum tersedia)` pada `contracts/api-contract.md` padahal seluruhnya sudah ada sejak `BE-LAB-06` |
| Dependency backend | `NONE` yang menahan. Ketujuh endpoint sudah ada dan diverifikasi langsung pada source backend `2dfc4f2` |
| Perubahan sampingan | `NONE`. Jalur baca lama `GET /lab-specimens/rejection-reasons` tidak disentuh sama sekali |
| Interupsi | `NONE` |
| Status Git | Lihat blok di bawah tabel ini |
| Langkah berikutnya | Gelombang `MVP-0` tinggal menyisakan `FE-LAB-04` — tampilan tarif laboratorium baca saja dan komponen pemilih katalog, dengan endpoint pasangannya `BE-LAB-07` yang sudah selesai. Risikonya rendah, dan godaan yang perlu dijaga adalah menambahkan tombol ubah pada menu tarif, yang dilarang `LAB-DEC-033` |

```text
 M src/lib/state/store.jsx
 M src/utils/menu-sidebar/menu-items.jsx
?? src/app/health-services/master-data/lab-rejection-reasons/
?? src/components/view/health-services/master-data/lab-rejection-reasons/
?? src/lib/constants/health-services/master-data/lab-rejection-reasons/
?? src/lib/hooks/health-services/master-data/lab-rejection-reasons/
?? src/lib/state/slice/health-services/master-data/master-data-lab-rejection-reason-slice.jsx
?? src/style/health-services/master-data/lab-rejection-reasons/
?? src/utils/health-services/master-data/lab-rejection-reasons/
?? tests/unit/lab-rejection-reason-utils.test.mjs
```

Blok di atas hanya memuat perubahan `FE-LAB-03`. Perubahan `FE-LAB-02` yang belum di-commit
masih berdiri berdampingan pada working tree yang sama.

Tidak ada `git add`, commit, push, merge, rebase, maupun perpindahan branch yang dilakukan pada
kedua repository.
