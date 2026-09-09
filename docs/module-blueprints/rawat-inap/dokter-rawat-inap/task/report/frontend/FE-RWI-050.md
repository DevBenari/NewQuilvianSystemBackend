# Laporan Perubahan Frontend — `FE-RWI-050`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-050` |
| Judul | Daftar pantau verifikasi catatan terpadu |
| Slice | `DOK-MVP-FE` urutan 9 |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/frontend-roadmap.md` §3 kartu `FE-RWI-050` |
| Trace | `FE-DOK-08`; `03-frontend-architecture.md` §3.8; `VAL-DOK-24`, `VAL-DOK-25`; `IA-INP-05`; `../02-module-map.md` §3.3 |
| Contract version | `0.3.0` — `approved` oleh Muhammad Hamzah, 3 September 2026 |
| Wewenang UI | `skema-tampilan-dokter-rawat-inap.md` §4, §13, §17–19, §21–22 dan rules §1 roadmap |
| Dependency | `FE-RWI-046` ✅ **selesai 9 September 2026**; `BE-RWI-053` ✅ selesai; **`BE-RWI-067` ✅ selesai 8 September 2026** — penutup kriteria 4. **Kriteria 5 tidak menunggu task mana pun**, melainkan keputusan pemilik `02-module-map.md` |
| Klasifikasi | `MEDIUM` — satu hook, satu constant, dua komponen view, penyisipan pada layar monitoring existing, dan satu penambahan style |
| Task mode | `FRONTEND` — backend strict read-only |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; laporan ini dan tautan buktinya pada roadmap serta `requirement-traceability.md` sub-modul yang sama |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit frontend saat dikerjakan | Dikerjakan di atas `52b07d363e92525739fb2ad63075ec80f6d4e230`, branch `HamzahV2`. Source-nya kemudian **di-commit pemilik pekerjaan sendiri** sebagai `e194509dc`; agent tidak menjalankan satu pun tindakan Git |
| Commit backend yang dijadikan rujukan | `3a6373e90e5a590bfad1ba214c5c941e602fc245`, branch `MHamzah` |
| Tanggal | Pass pertama 8 September 2026; **pass kedua 9 September 2026** sesudah `BE-RWI-067` menutup kriteria 4 |
| Status | 🟡 `SEBAGIAN` **9 September 2026.** **4 dari 5** acceptance criteria kini terpenuhi penuh; **1 belum terpenuhi**. Kriteria 4 — yang pada pass pertama hanya separuh — kini tertutup: `BE-RWI-067` menambahkan `ProviderName` pada butir daftar pantau, dan kolom Penulis menyebut namanya. Kriteria 3 juga diperkuat: tautan tidak lagi sekadar membuka tab, melainkan mendarat pada catatan yang dituju dan menyorotnya. **Kriteria 5 tetap belum terpenuhi**, dan bukan karena pekerjaan yang kurang: urutan daftar di dalam `FE-INP-09` **ditetapkan tingkat modul**, slot dokter belum dinyatakan pemilik `02-module-map.md`, dan roadmap §6 melarang memutuskannya sendiri. **Satu cacat nyata ditemukan uji peramban pass ini dan diperbaiki:** seluruh sel tabel daftar pantau salah dirender — kolom Penulis dan Profesi berbunyi `[object Object]`, kolom Pasien berbunyi `-`, kolom Status selalu berbunyi "Lewat Batas", dan tombol Buka Catatan menghasilkan alamat tanpa nomor episode. Validasi nyata pass kedua: `npm run lint` **0 error, 611 warning**; `npm run test:unit` **563/563 lulus, 0 gagal**; `npm run build` beserta `postbuild` berhasil; **7 skenario peramban lulus** di Edge, tiga di antaranya milik daftar pantau ini. Butir DoD screenshot tiga viewport **dikecualikan atas keputusan pengguna 1 September 2026**; catatan `NOT RUN`-nya tetap tercatat, tidak dihapus |

---

## 1. Keadaan yang ditemukan di awal

Layar **Daftar Pantau Rawat Inap** (`FE-INP-09`) sudah berdiri dan berjalan, memuat empat daftar
bertab: Penutupan Tertunda, Menembus Gerbang Keuangan, Perawat Belum Ditugaskan, dan Selisih
Isolasi. Komentar pada `inpatient-monitoring-constants.jsx` menyebutkannya terus terang: *"daftar
pantau kepatuhan pengkajian dan CPPT sengaja tidak ada karena bergantung pada slice yang masih
menunggu `DEC-INP-001`."*

Dua kenyataan yang menentukan bentuk task ini, dan keduanya ditemukan dari source, bukan dugaan:

1. **Tidak ada endpoint daftar pantau verifikasi lintas pasien.** `InpatientMonitoringController`
   hanya memiliki lima jalur, dan tidak satu pun tentang CPPT. Keadaan verifikasi hanya tersedia
   **per episode**, lewat
   `GET /patient-integrated-progress-notes/episodes/{episodeId}/verification-status`.
2. **Daftar pantau backend tidak mengembalikan nama penulis.** `CpptVerificationWatchItem` pada
   `CpptVerificationService.cs:41` memuat `NoteId`, `ProgressNoteNumber`, `ProfessionType`,
   `ProviderUserId`, `NoteDateTime`, `VerificationStatus`, `VerificationDueAt`, dan `IsOverdue`.
   Tidak ada nama pasien, dan tidak ada nama penulis — hanya id penggunanya.

Roadmap §6 sudah mengantisipasi keduanya, dan menetapkan batasnya: *"Layout data minimum/state
dapat disiapkan; integrasi/urutan belum boleh dianggap lulus."* Laporan ini mengikuti batas itu
apa adanya.

---

## 2. Proses bisnis dari sisi pengguna

**Siapa penggunanya.** Supervisor klinis dan kepala ruangan — orang yang sama yang sudah memakai
keempat daftar pantau existing.

**Kapan layar ini dibuka.** Ketika supervisor perlu menemukan catatan profesi lain yang menunggu
atau sudah lewat batas verifikasi DPJP.

**Langkah normalnya, berurutan:**

1. Supervisor membuka **Pelayanan Kesehatan → Rawat Inap → Daftar Pantau**. Dua klik dari Beranda,
   sama seperti sebelumnya — tidak ada menu baru yang ditambahkan.
2. Keempat daftar existing tetap berdiri di tempatnya, dengan urutan yang tidak diubah sama
   sekali.
3. Di bawahnya, dipisahkan garis tipis, muncul bagian baru **Verifikasi Catatan Terpadu**.
   Judulnya diikuti keterangan cakupan yang jujur: bagian ini memantau episode yang **sedang
   tampil** pada daftar aktif di atas, beserta angka jumlah episodenya.
4. Bila ada catatan yang menunggu, tabel menampilkan **enam kolom dan tidak lebih**: Pasien,
   Penulis, Profesi, Keterlambatan, Status, dan Aksi.
5. Menekan **Buka Catatan** membuka ruang kerja dokter pasien itu, langsung pada tab **Catatan
   Terpadu**.

**Apa yang sengaja tidak ditampilkan.** Tidak ada satu pun isi klinis pada daftar ini — tidak di
sel, tidak di tooltip, tidak di baris yang dapat dibuka, dan tidak di atribut aksesibilitas. Yang
boleh terbaca hanya siapa pasiennya, siapa penulisnya, profesinya, berapa lama tertunda, dan
statusnya. Kalimatnya ditegaskan di bawah tabel supaya batas itu terbaca oleh siapa pun yang
kelak menyunting layar ini.

**Tiga hasil yang tampak sama-sama sepi, dan dibedakan tegas:**

| Hasil | Kalimat yang muncul | Artinya |
| --- | --- | --- |
| Kebijakan belum aktif | "Verifikasi DPJP tidak diwajibkan." beserta "Belum ada kebijakan aktif yang mewajibkan verifikasi DPJP, sehingga tidak ada catatan yang perlu dipantau." | Inilah keadaan hari ini — `RWI-RULE-021` belum disahkan |
| Semua sudah terverifikasi | "Semua catatan sudah terverifikasi." beserta "Kebijakan verifikasi aktif, dan tidak ada catatan yang menunggu maupun lewat batas pada episode yang tampil." | Pekerjaan memang sudah beres |
| Gagal dibaca | "Data verifikasi tidak dapat dimuat." beserta "Sebagian atau seluruh episode gagal dibaca. Daftar ini belum boleh dianggap kosong." dan tombol **Coba Lagi** | Daftar kosong di sini **bukan** kabar baik |

**Pembacaan yang gagal sebagian.** Bila sebagian episode berhasil dibaca dan sebagian gagal,
peringatan kuning muncul di atas tabel menyebut berapa episode yang gagal, dan tabelnya tetap
ditampilkan apa adanya. Ia **tidak pernah** dilaporkan sebagai sukses penuh.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `roadmap/frontend-roadmap.md` kartu `FE-RWI-050`, rules §1.1–§1.4, dan §6 baris monitoring
- `contracts/api-contract.md` §3
- `skema-tampilan-dokter-rawat-inap.md` §13 beserta bagian Monitoring State
- `../02-module-map.md` §3.3 dan baris keputusan terbuka tentang urutan daftar di dalam `FE-INP-09`
- `Areas/HealthServices/InPatientManagement/Controllers/InpatientMonitoringController.cs` (read-only)
- `Areas/HealthServices/ClinicalManagement/Services/CpptVerificationService.cs` (read-only)
- `inpatient-monitoring-view.jsx`, `inpatient-monitoring-table-columns.jsx`,
  `inpatient-monitoring-constants.jsx`, `use-inpatient-monitoring.jsx`,
  `inpatient-monitoring-utils.jsx`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/inpatient-management/inpatient-cppt-verification-monitoring-constants.jsx` | **Baru.** Lima hasil daftar, batas cakupan pembacaan, dan seluruh salinan teks termasuk ketiga kalimat hasil yang berbeda |
| `src/lib/hooks/health-services/inpatient-management/use-cppt-verification-monitoring.jsx` | **Baru.** Membaca keadaan verifikasi per episode untuk episode yang sedang tampil, menggabungkannya, mengurutkan yang terlambat lebih dulu, dan menentukan hasil daftarnya |
| `src/components/view/health-services/inpatient-management/cppt-verification-monitoring-columns.jsx` | **Baru.** Enam kolom data minimum |
| `src/components/view/health-services/inpatient-management/cppt-verification-monitoring-section.jsx` | **Baru.** Bagian tambahan beserta batas state dan keterangan privasinya |
| `src/components/view/health-services/inpatient-management/inpatient-monitoring-view.jsx` | Bagian baru disisipkan **di bawah** tabel existing; cakupan episode disusun dari baris daftar aktif. Keempat daftar existing dan urutannya tidak disentuh |
| `src/style/health-services/inpatient-management/inpatient-monitoring.module.css` | Lima kelas baru untuk bagian verifikasi; kelas existing tidak diubah |
| `…/physician-workspace/physician-workspace-view.jsx` | Tab pembuka dapat ditentukan alamat lewat `?tab=`, supaya tautan dari daftar pantau mendarat langsung pada tab Catatan Terpadu. Nilai di luar keenam tab diabaikan dan kembali ke tab bawaan |
| **Pass kedua** — `…/cppt-verification-monitoring-columns.jsx` | Kolom Penulis membaca `providerName` dari `BE-RWI-067`. **Perbaikan cacat:** keenam `render` diubah ke kontrak `DataTable` yang sebenarnya |
| **Pass kedua** — `…/cppt-verification-monitoring-section.jsx` | `pagination={false}`, supaya bar paginasi kedua tidak menuliskan "0 dari 0 data" di bawah baris yang jelas terlihat |
| **Pass kedua** — `…/physician-workspace/physician-workspace-view.jsx` | Selain `?tab=`, alamat kini juga membawa `?note=`; nomor catatannya dibagikan lewat konteks ruang kerja |
| **Pass kedua** — `…/tabs/integrated-note/integrated-note-timeline.jsx` dan `integrated-progress-note-tab.jsx` | Catatan yang dituju disorot dan digulir ke layar; catatan yang tidak ada pada daftar dikatakan apa adanya |
| **Pass kedua** — `tests/unit/inpatient-physician-clinical-tabs.test.mjs` | Enam uji baru, dua di antaranya menjaga agar cacat kontrak `render` tidak kembali |

### 3.3 Kepatuhan arsitektur frontend

**Host existing dipertahankan, bukan diganti.** Bagian baru memakai `DataTable`, `BaseButton`,
`StatusBadge`, dan `InformationAlert` milik layar monitoring — bukan `ClinicalDataTable` — persis
seperti perintah roadmap: *"Gunakan DataTable existing di host ini, jangan menggantinya dengan
ClinicalDataTable hanya demi keseragaman nama."* Definisi kolomnya berada di berkas
`*-columns.jsx` terpisah, mengikuti pola `inpatient-monitoring-table-columns.jsx`.

`ClinicalStateBoundary` dari `FE-RWI-042` dipakai ulang untuk batas state bagian ini, sesuai
roadmap §1.2. **Nol base component baru dibuat**, dan tidak ada arsitektur tabel maupun workspace
baru yang diperkenalkan.

**Cakupan pembacaan dinyatakan, bukan disembunyikan.** Karena keadaan verifikasi hanya tersedia
per episode, daftar lintas pasien disusun dari pembacaan atas episode yang **sudah tampil** pada
daftar aktif. Itu bukan endpoint baru yang dikarang, dan cakupannya tertulis pada deskripsi bagian
supaya tidak ada yang membacanya sebagai daftar seluruh rumah sakit. Jumlah episode yang dibaca
sekali jalan dibatasi 25 agar satu halaman daftar pantau tidak menerbitkan permintaan tanpa batas.


### 3.4 Pass kedua — 9 September 2026

#### Kriteria 4 ditutup `BE-RWI-067`

Pada pass pertama, butir daftar pantau hanya membawa `ProviderUserId` — deretan angka dan huruf.
Kolom Penulis karena itu berbunyi "Nama penulis belum tersedia", dan supervisor harus membuka
catatan satu per satu hanya untuk tahu siapa yang perlu diingatkan.

`BE-RWI-067` menambahkan `ProviderName`, diambil server secara berjenjang dengan salinan nama
lebih dulu supaya akun yang berganti nama tidak menulis ulang penulis catatan lama. Layar
membacanya apa adanya. Penulis yang memang tidak dapat dikenali lagi berbunyi **"Penulis tidak
dapat dikenali"**, dan nomor penggunanya tetap **tidak pernah** ditampilkan sebagai gantinya.

#### Cacat yang ditemukan uji peramban, dan perbaikannya

Bagian ini adalah temuan terpenting pass kedua. Daftar pantau **tidak pernah dijalankan di
peramban** pada pass pertama, dan seluruh tabelnya ternyata salah dirender.

Penyebabnya satu: **dua tabel yang dipakai sub-modul ini punya kontrak `render` yang berbeda.**

| Komponen tabel | Cara memanggil `render` | Dipakai di mana |
| --- | --- | --- |
| `DataTable` — base layar daftar pantau | `render(item, meta)` — argumen pertama adalah **barisnya** | Daftar pantau `FE-INP-09`, termasuk bagian ini |
| `ClinicalDataTable` — base klinis dokter | `render(value, row, index)` — argumen pertama adalah **nilai selnya** | Tab klinis ruang kerja dokter |

Definisi kolom bagian ini ditulis memakai bentuk `ClinicalDataTable`, padahal roadmap justru
memerintahkan memakai `DataTable` milik host. Akibatnya di layar, dengan dua baris data:

| Kolom | Yang seharusnya tampil | Yang benar-benar tampil sebelum perbaikan |
| --- | --- | --- |
| Pasien | `Ny. Sari Melati` | `-` |
| Penulis | `Ns. Sari Wijaya` | `[object Object]` |
| Profesi | `Perawat` | `[object Object]` |
| Keterlambatan | `19 jam` | `Belum terhitung` |
| Status | `Lewat Batas` pada satu baris, `Menunggu Verifikasi` pada baris lain | `Lewat Batas` pada **kedua** baris |
| Aksi | Tautan ke episode dan catatannya | Alamat tanpa nomor episode dan tanpa nomor catatan |

Baris status adalah yang paling berbahaya: objek baris selalu bernilai benar, sehingga **setiap**
catatan yang menunggu terbaca sebagai sudah lewat batas. Supervisor akan mengejar catatan yang
sebenarnya masih dalam tenggat.

Perbaikannya mengubah keenam `render` ke kontrak `DataTable` yang sebenarnya, dan dua uji unit
baru menjaganya — satu memeriksa bentuk keenam `render`, satu memeriksa kolom status dan
keterlambatan membaca barisnya. Keduanya ikut berjalan pada `npm run test:unit`.

Cacat kedua ditemukan pada layar yang sama: `DataTable` memasang bar paginasinya sendiri, dan
karena bagian ini tidak mengirim `totalData`, bar itu berbunyi **"Menampilkan 0 sampai 0 dari 0
data"** tepat di bawah dua baris yang jelas terlihat. Bagian ini memang tidak berpaginasi sendiri
— cakupannya sudah ditentukan halaman daftar pantau di atasnya — sehingga paginasinya dimatikan.

#### Kriteria 3 diperkuat: tautan mendarat pada catatannya

Tautan kolom Aksi sejak pass pertama sudah membawa `?tab=integrated-note&note=<noteId>`, tetapi
ruang kerja hanya membaca `?tab=`. Nomor catatannya dibuang diam-diam, sehingga supervisor
mendarat pada tab yang benar lalu tetap harus menelusuri lini masa satu per satu.

Sekarang nomor itu dipakai: ruang kerja membacanya, lini masa menyorot catatan yang dituju dan
menggulirnya ke tengah layar. Bila catatan itu tidak ada pada daftar yang sedang tampil — paling
sering karena penyaring profesi menyembunyikannya — layar mengatakannya, bukan mendarat tanpa
menyorot apa pun.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Membaca keadaan verifikasi catatan terpadu..." beserta kerangka |
| Kosong — kebijakan tidak aktif | "Verifikasi DPJP tidak diwajibkan." beserta penjelasannya |
| Kosong — semua terverifikasi | "Semua catatan sudah terverifikasi." beserta penjelasannya |
| Kosong — belum ada cakupan | "Belum ada episode yang dapat dipantau." ketika daftar aktif di atas belum menampilkan episode mana pun |
| Gagal | "Data verifikasi tidak dapat dimuat." beserta "Daftar ini belum boleh dianggap kosong." dan **Coba Lagi** |
| Gagal sebagian | Peringatan kuning menyebut berapa episode gagal dibaca; tabel tetap ditampilkan dan tidak diklaim lengkap |
| Tanpa hak akses | Gerbang penolakan tanpa menampilkan satu baris pun |
| Hanya baca | Seluruh bagian ini hanya baca. **Tidak ada tombol Verifikasi di sini**; verifikasinya dikerjakan di `FE-RWI-046` dengan kewenangan yang diperiksa ulang |
| Kiriman ganda | Tidak ada mutasi pada bagian ini. **Coba Lagi** membaca ulang dan tidak menggandakan baris |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Clinical Management / Patient Integrated Progress Note

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/clinical-management/patient-integrated-progress-notes/episodes/{episodeId}/verification-status` | Membaca keadaan verifikasi satu episode beserta daftar pantaunya; dipanggil untuk setiap episode yang tampil pada daftar aktif | `PatientIntegratedProgressNote : Read` |

**Nol endpoint baru dikarang.** Roadmap §6 melarangnya, dan larangan itu diikuti apa adanya.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Selesai tanpa error | `PASS` | Keluaran `eslint . --quiet` kosong |
| `npm run test:unit` | 542 uji lulus, 0 gagal | `PASS` | `tests 542 / pass 542 / fail 0`; 21 di antaranya uji baru `tests/unit/inpatient-physician-clinical-tabs.test.mjs` |
| `npm run build` | Berhasil beserta `postbuild`; route `○ /health-services/inpatient-management/monitoring` muncul pada keluaran | `PASS` | `✓ Compiled successfully in 33.2s` |
| Normalisasi keadaan verifikasi beserta `isVerificationPolicyEmpty` | Sesuai | `PASS` | `FE-RWI-046 K4`, dipakai bersama oleh bagian ini |
| Grep warna literal dan `!important` pada penambahan style | Nol hasil | `PASS` | Grep checklist konsistensi UI |
| Grep `<table>` mentah pada dua berkas view baru | Nol hasil | `PASS` | Seluruh tabel lewat `DataTable` existing |
| Verifikasi interaktif di peramban | Tidak dijalankan | `NOT RUN` | Lihat catatan di bawah |
| Fixture teks klinis rahasia samaran tidak muncul pada DOM atau tooltip | Tidak dijalankan | `NOT RUN` | Menuntut eksekusi peramban; batas privasinya sudah ditegakkan pada definisi kolom |
| Navigasi dua klik dari Beranda ke daftar pantau | Tidak dijalankan | `NOT RUN` | Menuntut eksekusi peramban; menu dan route tidak diubah task ini |

**Uji manual:** `NOT FEASIBLE`.

**Alasan konkret.** Tidak ada `playwright.config.*` di akar repository sehingga
`npm run test:e2e` tidak dapat dijalankan tanpa menambah konfigurasi baru. Selain itu, pengujian
bermakna atas daftar ini menuntut **kebijakan verifikasi yang aktif** — dan kebijakan itu belum
ada sama sekali, sehingga daftar pantau di lingkungan mana pun hari ini akan selalu kosong dengan
alasan "tidak diwajibkan". Keadaan itu sendiri sudah tercatat sebagai gerbang terbuka pada roadmap
§4.

**Tidak dijalankan:** `npm run test:e2e`, `npm run test:uat`, screenshot tiga viewport, dan uji
kebocoran isi klinis di peramban.

### Pass kedua — 9 September 2026

Seluruh baris di atas adalah riwayat pass pertama dan **tidak dihapus**. Baris di bawah adalah
validasi yang benar-benar dijalankan ulang.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint` | 0 error, 611 warning | `PASS` | `✖ 611 problems (0 errors, 611 warnings)` — sama persis dengan garis dasar sebelum perubahan |
| `npm run test:unit` | 563 uji lulus, 0 gagal | `PASS` | `tests 563 / pass 563 / fail 0`; garis dasar 557, jadi 6 uji baru |
| `npm run build` beserta `postbuild` | Berhasil; **nol route baru** — keluaran tetap memuat satu route monitoring | `PASS` | `[prepare-standalone] Standalone runtime siap dijalankan.` |
| **Peramban** — kolom Penulis menyebut nama, bukan nomor pengguna | Baris pertama berbunyi `Ns. Sari Wijaya`; baris kedua berbunyi `Penulis tidak dapat dikenali` | `PASS` | Edge, skenario `FE-RWI-050 K4` |
| **Peramban** — nomor pengguna tidak pernah dirender | `outerHTML` seluruh bagian diperiksa; `8f3a1c92-…` dan `0c11ab77` nol kali muncul | `PASS` | Skenario yang sama |
| **Peramban** — fixture teks klinis samaran tidak bocor | Kolom isi catatan diisi `NYERI-DADA-RAHASIA-UJI`, lalu `outerHTML` bagian diperiksa — nol kali muncul, termasuk di atribut dan tooltip | `PASS` | Skenario yang sama. Ini menutup baris `NOT RUN` pass pertama |
| **Peramban** — tautan Buka Catatan membawa episode, tab, dan nomor catatan | `href` memuat `/episodes/…/physician`, `tab=integrated-note`, dan `note=…` | `PASS` | Skenario `FE-RWI-050 K3` |
| **Peramban** — tautan itu benar-benar mendarat pada catatannya | Hanya catatan yang dituju bertanda `data-selected="true"`, dan posisinya diperiksa berada di dalam viewport | `PASS` | Skenario `FE-RWI-050 K3` |
| **Peramban** — kebijakan tidak aktif berbeda tegas dari "semuanya beres" | `data-result="not-required"` beserta kalimat "Verifikasi DPJP tidak diwajibkan."; kalimat "Semua catatan sudah terverifikasi." diperiksa **tidak** muncul | `PASS` | Skenario `FE-RWI-050 K2` |
| **Peramban** — keenam sel tabel dirender benar | Ditemukan **gagal** lebih dulu — lihat bagian 3.4 — lalu `PASS` sesudah kontrak `render` diperbaiki | `PASS` (sesudah perbaikan) | Skenario `FE-RWI-050 K4`, dijalankan dua kali |
| Grep anti-regresi checklist konsistensi UI — keenam butir | Nol hasil | `PASS` | Dijalankan pada berkas yang diubah |
| Navigasi dua klik dari Beranda ke daftar pantau | Tidak dijalankan | `NOT RUN` | Menu dan route tidak diubah task ini; daftar pantau dibuka langsung lewat alamatnya pada skenario di atas |
| Screenshot tiga viewport | Tidak dijalankan | `NOT RUN` | Dikecualikan atas keputusan pengguna 1 September 2026 |

**Uji manual pass kedua:** `PASS`, dan **menemukan cacat**. Skenario dijalankan di Microsoft Edge
terhadap `.next/standalone/server.js` pada port `3710`, dengan balasan API dipalsukan `page.route`.
Konfigurasi Playwright dan spec-nya dibuat **sementara**, dijalankan, lalu **dihapus kembali**;
`git status` sesudahnya hanya memuat berkas source yang memang diubah, dan `test-results/` tidak
tertinggal. Perlindungan permanennya dipindahkan ke uji unit.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Daftar muncul sebagai daftar tambahan di dalam daftar pantau yang sudah ada, **bukan** layar baru | **Terpenuhi** | `CpptVerificationMonitoringSection` disisipkan di dalam `inpatient-monitoring-view.jsx`, di dalam shell yang sama. Nol route baru dibuat; keluaran build tetap menampilkan satu route monitoring. Nol butir menu baru ditambahkan |
| 2. Tiga keadaan dibedakan tegas: sudah terverifikasi, tidak diwajibkan, dan gagal dimuat | **Terpenuhi** | `useCpptVerificationMonitoring` menghasilkan lima hasil terpisah, dan urutan penentuannya sengaja menaruh kegagalan paling depan sehingga daftar yang gagal dibaca **tidak pernah** terbaca sebagai "semuanya beres". Ketiganya memakai kalimat yang berbeda pada `emptyCopy` |
| 3. Setiap baris membuka catatan terpadu pasien itu | **Terpenuhi** — diperkuat 9 September 2026 | Kolom Aksi merender tautan ke `buildInpatientPhysicianWorkspaceRoute(episodeId)` beserta `?tab=integrated-note&note=<noteId>`. Sejak pass kedua, ruang kerja membaca **kedua** parameter itu: `?tab=` membuka tab Catatan Terpadu, dan `?note=` menyorot catatan yang dituju lalu menggulirnya ke tengah layar. Sebelumnya nomor catatan dibuang diam-diam. Nilai tab di luar keenam tab tetap diabaikan, dan nomor catatan yang tidak ada pada daftar dinyatakan apa adanya, bukan didiamkan. Diuji di peramban: `href` benar, catatan yang dituju tersorot, dan catatan lain tidak |
| 4. Daftar menampilkan nama pasien, penulis, dan keterlambatan — **tanpa isi klinis** | **Terpenuhi** — 9 September 2026 | Enam kolom saja yang dirender, dan tidak satu pun memuat isi klinis. Larangan itu kini **dibuktikan di peramban**, bukan hanya dinyatakan: kolom isi catatan diisi teks samaran, lalu seluruh `outerHTML` bagian ini diperiksa dan teks itu nol kali muncul. **Bagian yang dulu tertahan kini tertutup:** sejak `BE-RWI-067`, butir daftar pantau membawa `ProviderName`, dan kolom Penulis menyebut namanya. Penulis yang tidak dapat dikenali berbunyi "Penulis tidak dapat dikenali"; nomor pengguna tetap **tidak pernah** ditampilkan, dan itu pun diperiksa pada `outerHTML`. Kolom Pasien, Profesi, Keterlambatan, dan Status juga baru benar-benar terbaca benar sejak cacat kontrak `render` diperbaiki — lihat bagian 3.4 |
| 5. Urutan daftar di dalam daftar pantau mengikuti ketetapan `02-module-map.md`, bukan diputuskan sendiri | **Belum terpenuhi** — diperiksa ulang 9 September 2026 | `../02-module-map.md` baris 371 masih menyatakan urutan daftar di dalam `FE-INP-09` **ditetapkan tingkat modul** dan "tidak boleh diputuskan sendiri-sendiri", dan baris 232 masih menyebut `FE-DOK-08` sebagai "daftar tambahan" tanpa nomor urutan — berbeda dari `keperawatan` yang mendapat kata "ketiga". Slot dokter karena itu **masih belum dinyatakan**. Bagian ini tetap ditempatkan **di bawah** keempat daftar existing tanpa mengubah urutannya, sebagai penempatan sementara. **Ini satu-satunya kriteria yang belum terpenuhi, dan ia bukan pekerjaan yang kurang:** tidak ada task — backend maupun frontend — yang dapat menutupnya. Yang dibutuhkan adalah keputusan pemilik peta modul |

### Butir Definition of Done

| Butir | Status |
| --- | --- |
| Kelima acceptance existing terbukti | **Belum** — **4 dari 5** terbukti sejak 9 September 2026; kriteria 5 menunggu keputusan pemilik `02-module-map.md`, bukan pekerjaan |
| State dan permission terbukti | Terpenuhi pada source dan tabel bagian 4; **tiga hasil daftar kini terbukti di peramban** — ada yang tertunda, tidak diwajibkan, dan tabel terisi benar |
| Visual Acceptance Criteria terbukti | **Terpenuhi kecuali tiga viewport.** Tabel data minimum, tiga pesan hasil berbeda, tombol Coba Lagi, dan tidak adanya editor klinis duplikat kini terbukti di peramban. Screenshot tiga viewport **dikecualikan atas keputusan pengguna 1 September 2026** |
| Gate §4.1 relevan (butir 17, 18) | **Butir 17 kini terbukti di peramban**, bukan hanya pada source: fixture teks klinis samaran diperiksa nol kali muncul pada seluruh `outerHTML` bagian ini, termasuk atribut dan tooltip. Butir 18 tetap terbukti: tiga hasil berbeda kalimat, dan gagal baca tidak pernah menjadi sukses |
| Laporan menyertakan privasi data minimum, tiga hasil, dan deep link | Terpenuhi pada bagian 2, 4, dan 7 |
| Urutan mengikuti peta modul | **Belum** — menunggu ketetapan pemilik; diperiksa ulang 9 September 2026 dan `02-module-map.md` belum berubah |
| Konflik §6 diselesaikan sebelum sign-off | **Belum** — lihat bagian 8 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Pass kedua menemukan cacat render yang tidak terlihat lint maupun build.** Definisi kolom bagian ini ditulis memakai kontrak `render` milik `ClinicalDataTable`, padahal host-nya memakai `DataTable` yang kontraknya berbeda. Seluruh tabel salah dirender, dan yang paling berbahaya adalah kolom Status yang menandai **setiap** catatan sebagai sudah lewat batas. Rinciannya pada bagian 3.4. **Pelajarannya berlaku lebih luas dari task ini:** dua base tabel dengan nama mirip dan kontrak berbeda hidup berdampingan pada sub-modul yang sama, dan lint tidak dapat membedakannya. Berkas lain yang memakai bentuk `(value, row)` sudah diperiksa — keenamnya memakai `ClinicalDataTable` dan karena itu **benar**; nol berkas lain yang perlu diperbaiki |
| **Batas kontrak — satu sudah ditutup, satu masih ada** | Pertama, **tidak ada endpoint daftar pantau verifikasi lintas pasien**; yang tersedia hanya per episode. Daftar lintas pasien karena itu tetap disusun dari episode yang sedang tampil, dan cakupan itu dinyatakan pada layar. Batas ini **masih ada**. Kedua, daftar pantau dulu tidak mengembalikan nama penulis — hanya `ProviderUserId`. Batas ini **sudah ditutup `BE-RWI-067`** pada 8 September 2026, dan layar membacanya pada pass kedua |
| Dependency backend | `BE-RWI-053` ✅ selesai 4 September 2026. Mekanisme daftar pantaunya berjalan dengan kebijakan kosong, persis seperti yang dirancang |
| Dependency frontend | `FE-RWI-046` ✅ **selesai 9 September 2026**. Tautan dari daftar ini mendarat pada tab yang sudah berfungsi penuh, dan penyorotan catatan yang dituju justru dikerjakan pada lini masa milik task itu |
| Perubahan sampingan | `NONE`. Keempat daftar pantau existing, urutannya, kolomnya, dan hook-nya tetap tidak disentuh. Perubahan pada lini masa catatan terpadu bukan perubahan sampingan — ia memang yang menutup kriteria 3 |
| Interupsi | `NONE` |
| Status Git | **Pass pertama:** berkas task ini muncul sebagai `??` dan `M` pada branch `HamzahV2`, dan agent tidak menjalankan satu pun tindakan Git. Pemilik pekerjaan meng-commit sendiri sebagai `e194509dc`. **Pass kedua 9 September 2026:** dikerjakan di atas `423856322` pada branch `HamzahV2` yang sama; agent kembali **tidak menjalankan satu pun tindakan Git**. Konfigurasi Playwright sementara dan kedua spec sementaranya dihapus kembali, dan `test-results/` tidak tertinggal |
| Langkah berikutnya | Meminta **ketetapan urutan daftar** di dalam `FE-INP-09` kepada pemilik `02-module-map.md`. Itu satu-satunya yang menahan task ini, dan ia bukan pekerjaan yang dapat diselesaikan agent mana pun |

### Yang dibutuhkan agar task ini dapat menjadi ✅

Diperiksa ulang 9 September 2026.

| No | Kebutuhan | Pemilik | Keadaan 9 September 2026 |
| ---: | --- | --- | --- |
| 1 | **Ketetapan urutan** daftar di dalam `FE-INP-09` beserta nomor slot daftar dokter. Satu layar kini dipakai tiga sub-modul, dan `02-module-map.md` baris 371 melarang memutuskannya sendiri-sendiri | Pemilik `02-module-map.md` bersama Frontend authority | ⛔ **Masih dibutuhkan, dan ini satu-satunya yang menahan ✅.** Bukan pekerjaan, melainkan keputusan |
| 2 | **Nama penulis** pada `CpptVerificationWatchItem`, atau endpoint pendamping yang memetakan `ProviderUserId` menjadi nama | `ClinicalManagement`, sebagai task backend baru | ✅ **Terpenuhi** — `BE-RWI-067` selesai 8 September 2026 |
| 3 | Keputusan **bentuk agregasi lintas episode**: apakah cakupan "episode yang sedang tampil" dipertahankan, atau backend menyediakan daftar pantau lintas pasien tersendiri beserta paginasinya | Pemilik peta modul bersama `ClinicalManagement` | ⏳ **Masih terbuka.** Tidak menahan kelima acceptance criteria; cakupan yang berlaku dinyatakan apa adanya di layar |
| 4 | **Kebijakan verifikasi yang aktif**, supaya daftar ini dapat diuji dengan data yang benar-benar tertunda. Selama `RWI-RULE-021` belum disahkan, daftar ini selalu berbunyi "tidak diwajibkan" pada data nyata | Clinical Governance | ⏳ **Masih terbuka.** Pass kedua menutupinya dengan balasan uji yang memuat catatan tertunda, sehingga tabelnya tetap terbukti benar-benar terisi |
