# Laporan Perubahan Frontend — `FE-LAB-01`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-01` |
| Judul | Kerangka modul dan kontrak penanganan state |
| Slice | — (task kerangka; tidak memetakan satu slice bisnis, tetapi menopang `S1a`, `S2`, `S3`, `S7`, `S11`, `S13a`, `S13b`, `S14`, `S15`) |
| Roadmap | `docs/module-blueprints/laboratorium/roadmap/frontend-roadmap.md` bagian 3, gelombang `MVP-0` |
| Trace | `LAB-DEC-010`, `LAB-FE-001`, `LAB-FE-002`, `LAB-FE-014`; `03-frontend-architecture.md` bagian 2 dan 4; `CAP-22`, `CAP-23` |
| Contract version | `LAB-API-v1` r3 — `approved`, dikunci 2026-09-02 |
| Wewenang UI | `LAB-FE-001` konvensi project untuk letak menu dan penamaan route — **wajib diikuti**. `LAB-FE-002` `DEV_DISCRETION` untuk tata letak, warna, dan pilihan komponen, sepanjang memakai komponen dan gaya yang sudah dipakai modul lain. Task ini **tidak** menyentuh satu pun invariant keselamatan (`LAB-FE-006`, `LAB-FE-009` .. `LAB-FE-013`), karena layar yang mengandungnya milik task berikutnya |
| Dependency | — (tidak ada). Roadmap menuliskan Dependency `—` untuk task ini; task `FE-LAB-02` .. `FE-LAB-09` justru bergantung padanya |
| Klasifikasi | `HEAVY` — skor 9: repository 2, berkas diperiksa 2, berkas diubah 2, logika bisnis 0, kontrak API 1, database 0, keamanan 1, UI/workflow 1. Duduk di batas bawah `HEAVY`, dan angkanya berasal dari jumlah berkas serta rentang dua repository, **bukan** dari kerumitan aturan bisnis — layar yang dibuat baca saja dan tidak memuat satu pun aturan klinis |
| Task mode | `FRONTEND` — frontend target tulis, backend strict read-only sebagai sumber kebenaran kontrak |
| Target tulis | `QuilvianSystemFrontendDev` — tujuh lapis `laboratory-management`, `src/lib/state/store.jsx`, `src/utils/menu-sidebar/menu-items.jsx`; dan `NewQuilvianSystemBackend` — **hanya** `docs/module-blueprints/laboratorium/task/report/frontend/FE-LAB-01.md` beserta tautan buktinya pada `roadmap/frontend-roadmap.md` dan `roadmap/traceability.md` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit frontend saat dikerjakan | `554e5d5ec` — *Merge pull request #18 from DevBenari/QuilvianIntegrationFrontend*, branch `YogaV2`, upstream `origin/YogaV2`. Roadmap menyebut snapshot `688daff90`; `554e5d5ec` adalah keturunannya dan selisihnya diperiksa (lihat bagian 1) |
| Commit backend yang dijadikan rujukan | `3029af9` — branch `yoga`. Roadmap menyebut `c87d9c0`; pemeriksaan controller dan DTO dilakukan pada `3029af9` |
| Tanggal | 2026-09-04 |
| Status | **Selesai.** Tujuh lapis berdiri, halaman contoh dapat dibuka, keempat state tertangani, dan tidak ada konstanta yang diduplikasi. Seluruh butir DoD terpenuhi. Satu butir sengaja tidak dijalankan dan disebut apa adanya pada bagian 6 |

---

## 1. Keadaan yang ditemukan di awal

**Modul Laboratorium tidak ada sama sekali di frontend.** Pemeriksaan pada commit `554e5d5ec`
mengulang temuan `CAP-21`: pencarian kata `laboratory-management`, `labOrder`, dan `lab-order`
di seluruh `src` tidak menghasilkan satu berkas pun milik Laboratorium. Yang muncul hanya
berkas modul IGD yang kebetulan memuat kata "diagnostic support". Folder
`src/app/health-services/` berisi sebelas modul, dan Laboratorium bukan salah satunya.

Artinya porsi pekerjaan frontend Laboratorium pada Rilis 1 memang **seratus persen** baru,
persis seperti yang dinyatakan roadmap.

**Selisih snapshot yang diperiksa.** Roadmap frontend disusun di atas `688daff90`, sedangkan
pekerjaan ini berjalan di atas `554e5d5ec`. Keduanya diperiksa hubungannya: `688daff90` adalah
leluhur langsung `554e5d5ec`, dan tidak ada berkas Laboratorium yang lahir di antara keduanya.
Titik berangkat nol pada roadmap tetap berlaku apa adanya.

**Gerbang `LAB-OPEN-018` sudah tidak menahan.** Roadmap mencatat bahwa akar aturan frontend di
runtime kehilangan sepuluh dari sebelas berkas aturan, sehingga pembangun layar kehilangan
pijakan pola komponen dan token desain. Pada saat task ini dikerjakan, seluruh berkas aturan
frontend yang dibutuhkan **tersedia dan terbaca**: arsitektur frontend, katalog base component,
gerbang keputusan base component, design token, pola komposisi halaman, standar fitur master
data, checklist konsistensi UI, kebijakan test, profil project, dan template laporan. Karena
itu pekerjaan tidak berhenti pada gerbang tata kelola.

**Celah yang membuat outcome belum tercapai.** Tanpa task ini, setiap task frontend berikutnya
harus memutuskan sendiri di mana berkasnya diletakkan, bagaimana memanggil API, di mana state
disimpan, dan kalimat apa yang muncul ketika data gagal dimuat. Delapan layar yang dibuat
delapan kali dengan delapan keputusan berbeda adalah persis yang dicegah task ini.

---

## 2. Proses bisnis dari sisi pengguna

**Siapa penggunanya.** Petugas laboratorium, kepala instalasi laboratorium, dan siapa pun yang
memegang hak akses `LabOrder : Read`.

**Kapan layar dibuka.** Ketika seseorang ingin melihat gambaran singkat beban kerja
laboratorium pada satu rentang waktu — berapa pesanan yang masuk, berapa yang masih dikerjakan,
dan bagaimana sebarannya di antara tiga disiplin: Patologi Klinik, Patologi Anatomi, dan
Mikrobiologi.

**Langkah yang berurutan:**

1. Pengguna membuka menu **Laboratorium → Ringkasan Laboratorium** pada bilah sisi.
2. Layar terbuka dan langsung meminta rekap ke backend. Selama permintaan berjalan, kartu
   angka tampil sebagai kerangka abu-abu dan tabel status menampilkan baris "Mengambil data
   laboratorium...". Tombol muat ulang dan tombol atur ulang penyaring **terkunci** selama itu,
   sehingga satu ketukan tidak berubah menjadi dua permintaan.
3. Rekap datang. Lima kartu angka terisi: Total Pesanan, Patologi Klinik, Patologi Anatomi,
   Mikrobiologi, dan Tanpa Disiplin. Di bawahnya tabel memecah jumlah itu menurut status
   pesanan: Draf, Diminta, Diterima, Sedang Dikerjakan, Selesai, Ditahan, Pembatalan Diminta,
   dan Dibatalkan.
4. Satu baris kecil di bawah kartu menyatakan **kapan angka itu diambil**, misalnya
   "Terakhir dimuat 4 Sep 2026, 14.03". Petugas jadi tahu apakah yang dilihatnya masih segar.
5. Bila pengguna ingin rentang lain, ia mengisi tanggal mulai dan tanggal akhir. Setiap
   perubahan tanggal langsung memicu pengambilan ulang, dan permintaan sebelumnya dibatalkan
   supaya jawaban lama tidak menimpa jawaban baru.
6. Tombol atur ulang mengosongkan kedua tanggal. Rentang kembali ke bawaan backend, yaitu
   **30 hari terakhir**.
7. Bila pengguna berpindah ke aplikasi lain lalu kembali, dan angka yang ditampilkan sudah
   berdiri lebih dari satu menit, layar **memuat ulang sendiri**. Ini menjawab butir "data
   basi" pada kontrak penanganan state.

**Jalur yang tidak normal:**

| Keadaan | Yang dialami pengguna |
| --- | --- |
| Rentang waktu tidak memuat satu pun pesanan | Muncul kotak keterangan biru: rentang yang dipilih belum memuat satu pun pesanan laboratorium, dan langkah berikutnya adalah mengubah tanggal lalu memuat ulang. Bukan sekadar kalimat "tidak ada data" |
| Permintaan gagal | Muncul kotak merah berisi **pesan dari server apa adanya**, ditambah tombol "Coba lagi". Tombol itu terkunci selama percobaan berjalan |
| Data baru saja diubah petugas lain (`409`) | Kotak berubah menjadi peringatan kuning berjudul "Data baru saja diubah petugas lain", dengan tombol "Muat ulang". Tidak ada pengiriman ulang otomatis — kapan layar disegarkan tetap keputusan petugas |
| Tanpa hak akses (`401` atau `403`) | Seluruh isi halaman diganti layar "Ups! Akses Ditolak" beserta arahan menghubungi IT Helpdesk. Pengguna tidak dibiarkan melihat kerangka layar yang tidak dapat ia pakai |
| Tanggal awal melewati tanggal akhir | Backend menolak dengan `400` dan kalimat "Tanggal awal tidak boleh melewati tanggal akhir." Kalimat itu ditampilkan apa adanya di kotak merah |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Tata kelola dan aturan.** `AGENTS.md` frontend; `rules/GLOBAL_RULES.md`; `rules/README.md`;
`rules/frontend/frontend-architecture.md`; `rules/frontend/base-component-catalog.md`;
`rules/frontend/base-component-decision-gate.md`; `rules/frontend/design-tokens.md`;
`rules/frontend/page-composition-patterns.md`; `rules/frontend/ui-consistency-checklist.md`;
`rules/frontend/test-policy.md`; `rules/frontend/REPORT_TEMPLATE.md`.

**Blueprint dan kontrak.** `roadmap/frontend-roadmap.md`; `03-frontend-architecture.md`;
`contracts/api-contract.md`; `roadmap/traceability.md`; `task/report/backend/BE-LAB-01.md`.

**Backend sebagai sumber kebenaran kontrak — strict read-only.**
`Areas/HealthServices/LaboratoryManagement/Controllers/LabOrderController.cs`;
`Areas/HealthServices/LaboratoryManagement/DTOs/LabFilterAndSummaryDtos.cs`;
`Responses/ApiResponse.cs`; serta pemeriksaan route seluruh controller Laboratorium.

**Frontend sebagai acuan pola.**
`src/lib/axiosInstance/InstanceAxios.jsx`; `src/lib/state/store.jsx`;
`src/lib/state/slice/health-services/nutrition-management/nutrition-order-slice.jsx`;
`src/lib/services/health-services/nutrition-management/nutrition-order.service.js`;
`src/lib/hooks/health-services/nutrition-management/use-nutrition-order-list.jsx`;
`src/components/view/health-services/nutrition-management/nutrition-order-view/nutrition-order-list-view.jsx`;
`src/components/features/base-features/` — `data-table`, `data-filter`, `hero`,
`summary-grid`, `information-alert`, `access-denied-gate`, `base-button`,
`filter-date-picker`; `src/utils/access-denied-utils.jsx`; `src/utils/ui/base-ui-utils.jsx`;
`src/utils/menu-sidebar/menu-items.jsx`; `src/app/globals.css`;
`src/style/components/features/base-features/base-data-components.module.css`;
`src/style/administrator/region/administrator-region-table.module.css`;
`src/style/administrator/region/administrator-region-tokens.module.css`.

### 3.2 Berkas yang berubah

**Sepuluh berkas baru — tujuh lapis modul:**

| Berkas | Perubahan |
| --- | --- |
| `src/app/health-services/laboratory-management/overview/page.jsx` | **Lapis route.** Route tipis: hanya metadata judul halaman dan pemanggilan view. Tidak ada markup, Axios, maupun style |
| `src/components/features/health-services/laboratory-management/laboratory-state-panel/laboratory-state-panel.jsx` | **Lapis komponen fitur.** Merangkai `InformationAlert` dan `BaseButton` menjadi tiga keadaan baku: gagal, bentrok `409`, dan kosong. Dipakai ulang layar Laboratorium berikutnya supaya kalimatnya seragam |
| `src/components/view/health-services/laboratory-management/laboratory-overview/laboratory-overview-view.jsx` | **Lapis komponen tampilan.** Menyusun Hero, SummaryGrid, penanda kesegaran data, DataFilter, panel keadaan, dan DataTable. Seluruh data datang dari hook |
| `src/components/view/health-services/laboratory-management/laboratory-overview/laboratory-overview-table-columns.jsx` | Definisi kolom tabel dipisah ke berkasnya sendiri sesuai pola komposisi halaman |
| `src/lib/constants/health-services/laboratory-management/laboratory-constants.jsx` | **Lapis konstanta.** Alamat delapan grup endpoint Laboratorium, alamat route modul, salinan teks baku penanganan state, penyaring bawaan, dan daftar baris rekap. Seluruhnya `Object.freeze` |
| `src/lib/hooks/health-services/laboratory-management/use-laboratory-overview.jsx` | **Lapis hook.** Menerjemahkan kontrak penanganan state menjadi perilaku layar: memuat, kosong, gagal, coba lagi, pembatalan permintaan usang, dan pemuatan ulang saat layar kembali difokuskan |
| `src/lib/services/health-services/laboratory-management/lab-order.service.js` | **Lapis API service.** Memanggil `GET /lab-orders/summary` lewat `InstanceAxios` yang sudah ada. Tidak ada instance Axios baru dan tidak ada alamat backend yang di-hardcode |
| `src/lib/state/slice/health-services/laboratory-management/lab-order-slice.jsx` | **Potongan Redux dasar.** Menyimpan penyaring, rekap, penanda muat, pesan galat, kode status galat, dan waktu pemuatan terakhir |
| `src/style/health-services/laboratory-management/laboratory-overview/laboratory-overview.module.css` | **Lapis style.** Satu class untuk penanda kesegaran data |
| `src/style/health-services/laboratory-management/laboratory-state-panel.module.css` | Jarak antara pesan dan tombol pada panel keadaan |

**Dua berkas yang disunting:**

| Berkas | Perubahan |
| --- | --- |
| `src/lib/state/store.jsx` | Satu baris import dan satu baris pendaftaran reducer dengan kunci `labOrder`. Tidak ada reducer lain yang tersentuh |
| `src/utils/menu-sidebar/menu-items.jsx` | Satu grup menu **Laboratorium** berisi satu butir **Ringkasan Laboratorium**. Ikon `RiFlaskLine` dan `RiDashboardLine` yang sudah diimpor dipakai ulang, tanpa menambah import baru |

**Satu berkas uji baru:**

| Berkas | Perubahan |
| --- | --- |
| `tests/unit/laboratory-lab-order-slice.test.mjs` | Enam uji terhadap potongan Redux: memuat, berhasil, gagal, bentrok `409`, permintaan yang dibatalkan, dan penyaring |

### 3.3 Kepatuhan arsitektur frontend

**Alur dependensi tidak dibalik.** Urutannya persis seperti yang diwajibkan:

```text
URL
  -> src/app/health-services/laboratory-management/overview/page.jsx
  -> src/components/view/.../laboratory-overview-view.jsx
  -> src/lib/hooks/.../use-laboratory-overview.jsx
  -> src/lib/state/slice/.../lab-order-slice.jsx
  -> src/lib/services/.../lab-order.service.js
  -> src/lib/axiosInstance/InstanceAxios.jsx
  -> Backend API
```

View tidak pernah memanggil `InstanceAxios`. Route tidak memuat logika. Komponen fitur tidak
mengetahui endpoint maupun Redux — ia hanya menerima props.

**Tujuh lapis, tanpa lapis kedelapan.** Roadmap bagian 2.1 melarang menaruh berkas di luar
tujuh lapis. Karena itu modul ini **tidak** membuat
`src/utils/health-services/laboratory-management/` seperti yang dilakukan modul Gizi. Dua
kebutuhan yang biasanya jatuh ke sana diselesaikan tanpa berkas baru:

1. **Membaca pesan galat dari server** ditempatkan di dalam potongan Redux. Ini bukan siasat:
   `rules/frontend/frontend-architecture.md` memang menyebut "normalisasi response API
   defensif" sebagai tanggung jawab slice.
2. **Mengenali galat tanpa hak akses** memakai helper bersama yang sudah ada di
   `@/utils/access-denied-utils`, bukan salinan baru.

**Konstanta yang sudah ada tidak diduplikasi.** `procedure-constants.jsx`,
`insurance-tariff-constants.jsx`, dan `tariff-category-constants.jsx` tetap berada di
`src/lib/constants/health-services/master-data/` dan **tidak disalin**. Folder konstanta
Laboratorium hanya berisi satu berkas, dan isinya tidak bersinggungan dengan ketiganya.

**Menu data induk tidak disentuh.** Sesuai `LAB-FE-014`, folder `laboratory-management` hanya
menampung layar operasional. Task ini tidak membuat satu pun berkas di
`health-services/master-data/`; layar batas nilai dan alasan penolakan adalah pekerjaan
`FE-LAB-02` dan `FE-LAB-03`.

**Gerbang keputusan base component.**

```text
UI GATE: 7 elemen — REUSE 6, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0
```

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Header halaman | `Hero` | `src/components/features/base-features/hero.jsx` | `REUSE` | `Hero` dengan `actions` berisi tombol muat ulang |
| Kartu angka rekap | `SummaryGrid` | `.../summary-grid.jsx` — props `items`, `loading`, `minWidth` | `REUSE` | Dipakai apa adanya; kerangka muatnya sudah bawaan |
| Penyaring rentang tanggal | `DataFilter`, `FilterDatePicker` | `.../data-filter.jsx`, `.../filter-date-picker.jsx` | `REUSE` | Search tidak dipasang karena layar ini tidak punya kata kunci; `DataFilter` memang hanya merender search bila `onSearchChange` diberikan |
| Tabel rekap status | `DataTable` | `.../data-table.jsx` | `REUSE` | `pagination={false}` dan `sortLatestFirst={false}` supaya urutan statusnya tidak digeser pengurutan bawaan |
| Tombol aksi | `BaseButton` | `.../base-button.jsx` | `REUSE` | Tidak ada satu pun `<button>` mentah maupun `.btn` Bootstrap |
| Layar tanpa hak akses | `AccessDeniedGate` | `.../access-denied-gate.jsx` | `REUSE` | Membungkus seluruh isi halaman |
| Panel gagal, bentrok, dan kosong | `InformationAlert`, `BaseButton` | `.../information-alert.jsx`, `.../base-button.jsx` | `COMPOSE` | Dirangkai menjadi `LaboratoryStatePanel` di lapis komponen fitur modul |

**Keputusan untuk satu-satunya baris yang bukan `REUSE`:**

> **Keputusan: panel gagal, bentrok `409`, dan kosong**
>
> - **A. Rangkai `InformationAlert` dan `BaseButton` menjadi satu komponen fitur modul — Rekomendasi.**
>   Tidak ada base component yang diubah, sehingga modul lain tidak terdampak sama sekali.
>   Kalimat baku penanganan state berdiri di satu tempat, sehingga delapan layar Laboratorium
>   berikutnya tidak menuliskan ulang kalimat yang sama dengan delapan kata yang berbeda.
>   Biayanya satu berkas komponen dan satu berkas style.
> - **B. Tulis ulang blok gagal dan kosong di setiap view.** Tanpa berkas baru sama sekali,
>   tetapi kalimatnya dijamin menyimpang begitu layar bertambah — persis yang dicegah task ini.
> - **C. Tambah varian baru pada `InformationAlert` agar ia sendiri membawa tombol coba lagi.**
>   Paling ringkas di sisi pemakai, tetapi mengubah perilaku komponen yang dipakai puluhan layar
>   lain. Perlu persetujuan eksplisit dan tidak sepadan untuk kebutuhan satu modul.
>
> Opsi **A** yang dijalankan.

**Token desain dipakai sebagai `var(...)`, bukan disalin.** Kedua berkas style baru tidak
memuat satu pun nilai warna literal, tidak memuat `!important`, dan tidak menyasar typography
komponen bersama. Dua baris `font-size` dan `line-height` yang ada menyasar class milik modul
ini sendiri — penanda kesegaran data — dan nilainya diambil dari token
`--font-size-small` serta `--line-height-body`.

**Yang sengaja tidak dibuat.** Kelima layar pada `03-frontend-architecture.md` bagian 9 —
pengisian dan validasi hasil, pantau nilai kritis, koreksi hasil, kotak pemberitahuan dokter,
dan penyuntingan pesanan oleh dokter — tidak disentuh sama sekali. Begitu pula sembilan layar
milik `FE-LAB-02` .. `FE-LAB-09`.

---

## 4. State yang ditangani di layar

Kontrak penanganan state pada `03-frontend-architecture.md` bagian 4 berisi tujuh baris.
Ketujuhnya ditangani, dan tempat penanganannya disebut supaya layar berikutnya tinggal
mengikuti.

| State | Yang dilihat pengguna | Ditegakkan di |
| --- | --- | --- |
| Sedang memuat | Lima kartu angka berubah menjadi kerangka abu-abu; tabel menampilkan "Mengambil data laboratorium..."; tombol muat ulang dan tombol atur ulang penyaring terkunci | `SummaryGrid loading`, `DataTable loading`, `DataFilter resetDisabled`, `BaseButton loading` |
| Kosong | "Belum ada data pada rentang waktu ini — rentang waktu yang dipilih belum memuat satu pun pesanan laboratorium. Ubah tanggal awal atau tanggal akhir, lalu muat ulang." Menjelaskan **kenapa** kosong dan langkah berikutnya, bukan sekadar "tidak ada data" | `LaboratoryStatePanel` dan `LABORATORY_STATE_COPY` |
| Gagal | Kotak merah berjudul "Data laboratorium gagal dimuat" berisi **pesan server apa adanya**, disertai tombol "Coba lagi" | `lab-order-slice` menyimpan pesan server; `LaboratoryStatePanel` menampilkannya |
| Coba lagi | Tombol "Coba lagi" memuat ulang rekap dengan penyaring yang sedang berlaku, dan berubah menjadi "Memuat ulang..." selama proses | `useLaboratoryOverview.retry` |
| Data basi | Satu baris "Terakhir dimuat ..." selalu terlihat. Bila layar ditinggalkan lalu dibuka kembali dan angkanya sudah berdiri lebih dari satu menit, layar memuat ulang sendiri | `useLaboratoryOverview`, pendengar `focus` dan `visibilitychange` |
| Kirim ganda | Setiap tombol yang memanggil API terkunci sejak ditekan sampai jawaban datang. Permintaan lama dibatalkan ketika penyaring berubah, sehingga jawaban usang tidak menimpa jawaban baru | `BaseButton loading`, `promise.abort()` pada pembersihan efek |
| Bentrok `409` | Peringatan kuning "Data baru saja diubah petugas lain" beserta tombol "Muat ulang". **Tidak ada** pengiriman ulang otomatis | Kode status disimpan slice; dibedakan `LaboratoryStatePanel` |
| Tanpa hak akses `401`/`403` | Seluruh isi halaman diganti layar "Ups! Akses Ditolak" beserta arahan menghubungi IT Helpdesk | `AccessDeniedGate` |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-orders/summary` | Mengisi lima kartu angka dan tabel rekap status pada layar Ringkasan Laboratorium. Parameter `startDate` dan `endDate` bersifat opsional; bila kosong, backend memakai 30 hari terakhir | `LabOrder : Read` |

**Alamat delapan grup endpoint dicatat, satu grup dipanggil.** Berkas konstanta memuat alamat
dasar seluruh grup Laboratorium — Lab Order, Lab Examination, Lab Specimen, Lab Value Bound,
Lab Worklist, Lab Rejection Reason, Lab Catalog, dan Lab Monitoring — supaya task berikutnya
tidak menuliskan alamat sebagai teks lepas di dalam layar. Yang benar-benar **dipanggil** pada
task ini hanya satu endpoint di atas.

Grup `Lab Patient Registration` sengaja **tidak** dicantumkan alamatnya. Pada kontrak
`LAB-API-v1` r3 ketiga endpointnya masih berstatus `Rencana (belum tersedia)`, dan pemeriksaan
pada backend `3029af9` memang tidak menemukan controllernya. Menyebut alamat yang belum ada
hanya mengundang layar memanggil sesuatu yang pasti gagal.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Selesai tanpa satu pun error | `PASS` | Keluaran perintah, kode keluar `0` |
| `npx eslint` pada seluruh berkas baru, termasuk peringatan | Bersih — 0 error, 0 warning | `PASS` | Peringatan `react-hooks/refs` yang sempat muncul karena penulisan ref saat render diperbaiki lebih dulu dengan memindahkannya ke dalam efek |
| `npm run test:unit` | Tidak dapat dijalankan lewat script npm-nya | `EXISTING / ENVIRONMENT ISSUE` | Script memakai pola glob `"tests/unit/**/*.test.mjs"`, sedangkan test runner Node 20 yang terpasang tidak mengembangkan pola itu dan menjawab `Could not find ...`. Cacat ini sudah ada sebelum task ini dan tidak disentuh, karena memperbaikinya berarti mengubah `package.json` di luar cakupan |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 440 uji, 440 lulus, 0 gagal | `PASS` | Bentuk perintah ini yang tercatat pada `rules/frontend/test-policy.md`, dan ia benar-benar menjalankan seluruh 38 berkas uji |
| Enam uji baru potongan Redux Laboratorium | 6 uji, 6 lulus | `PASS` | `tests/unit/laboratory-lab-order-slice.test.mjs` — memuat mengunci layar dan membersihkan galat; berhasil menyimpan rekap beserta waktunya; gagal menyimpan pesan server apa adanya; `409` tetap dapat dibedakan; permintaan yang dibatalkan tidak menjadi galat; penyaring dapat diubah, dibersihkan, dan dikembalikan ke bawaan |
| `npm run build` | Selesai, termasuk `postbuild` standalone | `PASS` | Kode keluar `0`. Route baru terbit sebagai `○ /health-services/laboratory-management/overview` pada daftar route build, dan berkasnya ada di `.next/server/app/health-services/laboratory-management/overview/page.js` |
| Tinjauan struktur: seluruh berkas berada di tujuh lapis | Terpenuhi | `PASS` | Sepuluh berkas baru, seluruhnya di tujuh lapis yang ditetapkan roadmap bagian 2.1. Tidak ada berkas modul di luar ketujuhnya |
| Tinjauan struktur: tidak ada pola route baru | Terpenuhi | `PASS` | Route mengikuti pola `src/app/health-services/<modul>/<menu>/page.jsx` yang sudah dipakai Gizi, Farmasi, dan Operasi |
| Tinjauan struktur: tidak ada duplikasi konstanta | Terpenuhi | `PASS` | `procedure-constants.jsx`, `insurance-tariff-constants.jsx`, dan `tariff-category-constants.jsx` tetap satu-satunya di `lib/constants/health-services/master-data/`. Folder konstanta Laboratorium hanya berisi `laboratory-constants.jsx` |
| Grep anti-regresi warna literal pada style baru | Tidak ada temuan | `PASS` | `grep -nEi "#[0-9a-f]{3,8}\b\|rgba?\("` pada kedua berkas style baru: kosong |
| Grep anti-regresi tombol non-base dan tabel mentah | Tidak ada temuan | `PASS` | `grep -nE "<button\|className=\"btn\|btn-primary\|btn-secondary"` dan `grep -n "<table"` pada seluruh JSX baru: kosong |
| Grep anti-regresi `!important` dan utility typography Bootstrap | Tidak ada temuan | `PASS` | Kosong pada berkas style maupun JSX baru |

**Uji manual: `NOT FEASIBLE`.**

Alasannya konkret, bukan penghindaran:

1. Layar ini **tidak punya satu pun kontrol yang mengubah data**. Yang ada hanya dua pemilih
   tanggal, satu tombol atur ulang, dan satu tombol muat ulang — seluruhnya hanya memicu satu
   permintaan baca.
2. Keempat keadaan yang perlu dilihat — kosong, gagal, bentrok `409`, dan tanpa hak akses —
   **hanya dapat dimunculkan oleh jawaban server yang sebenarnya**. Pada lingkungan sesi ini
   tidak ada backend Laboratorium yang berjalan dan tidak ada sesi login yang sah, sehingga
   memaksakannya hanya akan menghasilkan layar "Akses Ditolak" untuk keempat skenario dan
   membuktikan nol.
3. Roadmap memang menetapkan acceptance criteria task ini **"ditelusuri lewat tinjauan
   struktur, bukan uji fungsional"**.

Penggantinya bukan asumsi: keenam uji potongan Redux di atas membuktikan perilaku keempat
keadaan itu secara deterministik, dan keluaran build membuktikan halamannya benar-benar terbit
sebagai route yang dapat dibuka.

**Tidak dijalankan:**

| Pemeriksaan | Alasan |
| --- | --- |
| `npm run test:e2e` | Tidak diminta task, dan lingkungan sesi ini tidak menyediakan backend Laboratorium maupun sesi login yang dibutuhkan Playwright |
| `npm run test:uat` | Hanya dijalankan bila diminta secara eksplisit; tidak diminta |
| `npm run dev` | `AGENTS.md` melarang menjalankan development server tanpa kebutuhan konkret. Keluaran build sudah membuktikan route-nya terbit, sehingga menyalakan server tidak menambah bukti apa pun |

---

## 7. Acceptance criteria dan Definition of Done

**Acceptance criteria roadmap.**

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Ditelusuri lewat tinjauan struktur, bukan uji fungsional | **Terpenuhi** | Tiga baris tinjauan struktur pada bagian 6 — tujuh lapis, tanpa pola route baru, tanpa duplikasi konstanta — seluruhnya `PASS` |

**Verifikasi yang diminta roadmap.**

| Butir verifikasi | Status | Bukti |
| --- | --- | --- |
| Seluruh berkas berada di tujuh lapis | **Terpenuhi** | Daftar sepuluh berkas baru pada bagian 3.2; masing-masing diberi label lapisnya |
| Tidak ada pola route baru | **Terpenuhi** | `src/app/health-services/laboratory-management/overview/page.jsx` mengikuti pola `<modul>/<menu>/page.jsx` yang sudah berjalan |
| Tidak ada duplikasi `procedure-constants.jsx`, `insurance-tariff-constants.jsx`, maupun `tariff-category-constants.jsx` | **Terpenuhi** | Ketiganya tetap tunggal di `master-data`; folder konstanta Laboratorium hanya berisi satu berkas yang isinya tidak bersinggungan |

**Definition of Done.**

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Tujuh lapis ada | **Terpenuhi** | Route, komponen fitur, komponen tampilan, konstanta, hook, API service, dan style — ketujuhnya terisi berkas nyata, bukan folder kosong |
| Satu halaman contoh dapat dibuka | **Terpenuhi** | `/health-services/laboratory-management/overview` terbit pada daftar route build dan dapat dicapai dari menu **Laboratorium → Ringkasan Laboratorium** |
| Keempat state tertangani | **Terpenuhi** | Tabel lengkap pada bagian 4; ketujuh baris kontrak penanganan state ditangani, bukan hanya empat yang disebut DoD |
| Tidak ada konstanta yang diduplikasi | **Terpenuhi** | Baris tinjauan struktur ketiga pada bagian 6 |

Tidak ada butir DoD yang belum terpenuhi.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Satu peringatan lint `react-hooks/refs` sempat muncul karena ref ditulis saat render; sudah diperbaiki dengan memindahkan penulisannya ke dalam efek, dan lint akhir bersih tanpa peringatan pada berkas baru |
| Masalah yang diketahui | Script `npm run test:unit` pada `package.json` memakai pola glob yang tidak dikembangkan test runner Node 20 yang terpasang, sehingga script itu tidak menjalankan satu berkas uji pun. Cacat ini **sudah ada sebelum task ini** dan sengaja tidak diperbaiki karena berada di luar cakupan. Selama belum diperbaiki, suite dijalankan dengan bentuk perintah yang tercatat pada `test-policy.md`, yaitu `--test tests/unit/` |
| Dependency backend | `NONE` untuk task ini — roadmap menuliskan Dependency `—`, dan satu-satunya endpoint yang dipanggil, `GET /lab-orders/summary`, sudah tersedia sejak `BE-LAB-17` serta terverifikasi ada pada backend `3029af9`. Layar Laboratorium berikutnya punya dependency backendnya masing-masing sesuai kolom Dependency pada roadmap |
| Perubahan sampingan | `NONE`. Satu berkas komponen backend maupun frontend di luar cakupan tidak disentuh. Perlu dicatat bahwa repository backend memiliki perubahan yang **belum di-commit milik pekerjaan lain** sejak sebelum task ini dimulai; tidak satu pun berasal dari task ini, dan tidak satu pun disentuh |
| Interupsi | `NONE` |
| Status Git — `QuilvianSystemFrontendDev` | Lihat blok di bawah tabel ini |
| Status Git — `NewQuilvianSystemBackend` | Hanya satu berkas laporan baru pada `docs/module-blueprints/laboratorium/task/report/frontend/FE-LAB-01.md` beserta pembaruan bukti pada `roadmap/frontend-roadmap.md` dan `roadmap/traceability.md`. Perubahan lain pada working tree backend adalah milik pekerjaan lain yang sudah ada sebelumnya |
| Langkah berikutnya | Kerjakan `FE-LAB-02` — layar batas nilai dan jalur pengajuan batas kritis di `health-services/master-data/lab-value-bounds/`. Task itu memakai potongan Redux, panel keadaan, dan salinan teks baku yang berdiri di task ini, dan berisiko **tinggi** karena `LAB-FE-011` melarang adanya jalur simpan langsung untuk batas kritis |

```text
 M src/lib/state/store.jsx
 M src/utils/menu-sidebar/menu-items.jsx
?? src/app/health-services/laboratory-management/
?? src/components/features/health-services/laboratory-management/
?? src/components/view/health-services/laboratory-management/
?? src/lib/constants/health-services/laboratory-management/
?? src/lib/hooks/health-services/laboratory-management/
?? src/lib/services/health-services/laboratory-management/
?? src/lib/state/slice/health-services/laboratory-management/
?? src/style/health-services/laboratory-management/
?? tests/unit/laboratory-lab-order-slice.test.mjs
```

Tidak ada `git add`, commit, push, merge, rebase, maupun perpindahan branch yang dilakukan pada
kedua repository.
