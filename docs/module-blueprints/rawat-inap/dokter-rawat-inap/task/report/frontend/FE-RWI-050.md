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
| Dependency | `FE-RWI-046` 🟡 sebagian; `BE-RWI-053` ✅ selesai |
| Klasifikasi | `MEDIUM` — satu hook, satu constant, dua komponen view, penyisipan pada layar monitoring existing, dan satu penambahan style |
| Task mode | `FRONTEND` — backend strict read-only |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; laporan ini dan tautan buktinya pada roadmap serta `requirement-traceability.md` sub-modul yang sama |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit frontend saat dikerjakan | Dikerjakan di atas `52b07d363e92525739fb2ad63075ec80f6d4e230`, branch `HamzahV2`. Source-nya kemudian **di-commit pemilik pekerjaan sendiri** sebagai `e194509dc`; agent tidak menjalankan satu pun tindakan Git |
| Commit backend yang dijadikan rujukan | `3a6373e90e5a590bfad1ba214c5c941e602fc245`, branch `MHamzah` |
| Tanggal | 8 September 2026 |
| Status | 🟡 `SEBAGIAN`. **3 dari 5** acceptance criteria terpenuhi penuh, **1 terpenuhi separuh**, **1 belum terpenuhi**. Kriteria 4 tertahan kontrak: daftar pantau backend mengembalikan `providerUserId`, bukan nama penulis. Kriteria 5 **belum terpenuhi** karena urutan daftar di dalam `FE-INP-09` **ditetapkan tingkat modul** dan slot dokter belum dinyatakan pemilik peta modul — roadmap §6 melarang memutuskannya sendiri |

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

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Daftar muncul sebagai daftar tambahan di dalam daftar pantau yang sudah ada, **bukan** layar baru | **Terpenuhi** | `CpptVerificationMonitoringSection` disisipkan di dalam `inpatient-monitoring-view.jsx`, di dalam shell yang sama. Nol route baru dibuat; keluaran build tetap menampilkan satu route monitoring. Nol butir menu baru ditambahkan |
| 2. Tiga keadaan dibedakan tegas: sudah terverifikasi, tidak diwajibkan, dan gagal dimuat | **Terpenuhi** | `useCpptVerificationMonitoring` menghasilkan lima hasil terpisah, dan urutan penentuannya sengaja menaruh kegagalan paling depan sehingga daftar yang gagal dibaca **tidak pernah** terbaca sebagai "semuanya beres". Ketiganya memakai kalimat yang berbeda pada `emptyCopy` |
| 3. Setiap baris membuka catatan terpadu pasien itu | **Terpenuhi** | Kolom Aksi merender tautan ke `buildInpatientPhysicianWorkspaceRoute(episodeId)` beserta `?tab=integrated-note&note=<noteId>`. `physician-workspace-view.jsx` membaca `?tab=` dan membuka tab Catatan Terpadu langsung; nilai di luar keenam tab diabaikan |
| 4. Daftar menampilkan nama pasien, penulis, dan keterlambatan — **tanpa isi klinis** | **Terpenuhi separuh.** Bagian "tanpa isi klinis" terbukti; nama penulis **tertahan kontrak** | Enam kolom saja yang dirender, dan tidak satu pun memuat isi klinis — tidak di sel, tidak di tooltip, tidak di atribut. Nama pasien diambil dari baris daftar pantau host. **Yang belum:** `CpptVerificationWatchItem` hanya membawa `ProviderUserId`, bukan nama. Kolom Penulis karena itu berbunyi "Nama penulis belum tersedia" — id pengguna **sengaja tidak** ditampilkan, karena id bukan nama dan tidak menolong pembacanya |
| 5. Urutan daftar di dalam daftar pantau mengikuti ketetapan `02-module-map.md`, bukan diputuskan sendiri | **Belum terpenuhi** | `../02-module-map.md` baris 371 menyatakan urutan daftar di dalam `FE-INP-09` **ditetapkan tingkat modul** dan "tidak boleh diputuskan sendiri-sendiri". Slot dokter belum dinyatakan. Bagian ini karena itu ditempatkan **di bawah** keempat daftar existing tanpa mengubah urutannya, sebagai penempatan sementara yang menunggu ketetapan pemilik peta modul |

### Butir Definition of Done

| Butir | Status |
| --- | --- |
| Kelima acceptance existing terbukti | **Belum** — kriteria 4 separuh, kriteria 5 belum |
| State dan permission terbukti | Terpenuhi pada source dan tabel bagian 4; belum diverifikasi di peramban |
| Visual Acceptance Criteria terbukti | **Belum diverifikasi di peramban.** Dikecualikan atas keputusan pengguna bahwa e2e dan `.mjs` bukan gerbang selesai |
| Gate §4.1 relevan (butir 17, 18) | Butir 17 terbukti pada source: nol isi klinis pada sel, tooltip, baris yang dapat dibuka, maupun atribut aksesibilitas. Butir 18 terbukti: tiga hasil berbeda kalimat, dan gagal baca tidak pernah menjadi sukses |
| Laporan menyertakan privasi data minimum, tiga hasil, dan deep link | Terpenuhi pada bagian 2, 4, dan 7 |
| Urutan mengikuti peta modul | **Belum** — menunggu ketetapan pemilik |
| Konflik §6 diselesaikan sebelum sign-off | **Belum** — lihat bagian 8 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` pada scope task ini |
| **Masalah yang diketahui — dua batas kontrak** | Pertama, **tidak ada endpoint daftar pantau verifikasi lintas pasien**; yang tersedia hanya per episode. Daftar lintas pasien karena itu disusun dari episode yang sedang tampil, dan cakupan itu dinyatakan pada layar. Kedua, **daftar pantau tidak mengembalikan nama penulis** — hanya `ProviderUserId`. Keduanya **tidak diperbaiki dari sini**: task bermode `FRONTEND`, backend strict read-only, dan roadmap §6 melarang mengarang endpoint baru |
| Dependency backend | `BE-RWI-053` ✅ selesai 4 September 2026. Mekanisme daftar pantaunya berjalan dengan kebijakan kosong, persis seperti yang dirancang |
| Dependency frontend | `FE-RWI-046` 🟡 sebagian. Tautan dari daftar ini mendarat pada tab yang sudah berfungsi; keterbatasan nama verifikator pada `FE-RWI-046` tidak menghalangi navigasinya |
| Perubahan sampingan | `NONE`. Keempat daftar pantau existing, urutannya, kolomnya, dan hook-nya tidak disentuh |
| Interupsi | `NONE` |
| Status Git | Saat pekerjaan agent selesai, berkas task ini muncul sebagai `??` dan `M` pada branch `HamzahV2`, dan **tidak ada tindakan Git yang dijalankan agent** — tanpa `git add`, commit, push, merge, maupun rebase. Pemilik pekerjaan kemudian meng-commit sendiri sebagai `e194509dc`, sehingga `git status --short` pada repository frontend kini bersih |
| Langkah berikutnya | Meminta ketetapan urutan daftar di dalam `FE-INP-09` kepada pemilik `02-module-map.md`, dan mengajukan dua penambahan kontrak di bawah |

### Yang dibutuhkan agar task ini dapat menjadi ✅

| No | Kebutuhan | Pemilik |
| ---: | --- | --- |
| 1 | **Ketetapan urutan** daftar di dalam `FE-INP-09` beserta nomor slot daftar dokter. Satu layar kini dipakai tiga sub-modul, dan `02-module-map.md` baris 371 melarang memutuskannya sendiri-sendiri | Pemilik `02-module-map.md` bersama Frontend authority |
| 2 | **Nama penulis** pada `CpptVerificationWatchItem`, atau endpoint pendamping yang memetakan `ProviderUserId` menjadi nama | `ClinicalManagement`, sebagai task backend baru |
| 3 | Keputusan **bentuk agregasi lintas episode**: apakah cakupan "episode yang sedang tampil" dipertahankan, atau backend menyediakan daftar pantau lintas pasien tersendiri beserta paginasinya | Pemilik peta modul bersama `ClinicalManagement` |
| 4 | **Kebijakan verifikasi yang aktif**, supaya daftar ini dapat diuji dengan data yang benar-benar tertunda. Selama `RWI-RULE-021` belum disahkan, daftar ini selalu berbunyi "tidak diwajibkan" | Clinical Governance |
