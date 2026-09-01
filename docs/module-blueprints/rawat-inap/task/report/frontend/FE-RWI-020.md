# Laporan Perubahan Frontend — `FE-RWI-020`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-020` |
| Judul | Setiap episode dapat ditemukan, termasuk yang tertinggal |
| Slice | `F8 — Keterjangkauan` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) revision `3`, bagian 5 |
| Trace | `03-frontend-architecture.md` `FE-INP-16`, `IA-INP-02`, `IA-INP-03`, `IA-INP-04`, `IA-INP-05`, bagian 4.1, 5.1, 5.2, 6, dan 11A |
| Contract version | API `0.4.0`, Permission/Audit `0.4.0`, Validation `0.4.0` — seluruhnya berstatus `APPROVED` pada roadmap revision `3` |
| Wewenang UI | Nama menu, urutan kolom, dan bentuk penyaring `DEV_DISCRETION`. **Batasnya:** kelima nilai status wajib dapat dipilih |
| Dependency | Tidak ada. Kedua endpoint yang dipakai sudah tersedia di backend |
| Klasifikasi | `HEAVY` — skor 9: dua repository 2; lebih dari 20 berkas diperiksa 2; lima berkas source baru dan dua diubah 2; logika moderat 1; mengonsumsi kontrak yang sudah ada 1; database 0; menyentuh aturan privasi tanpa mengubah otorisasi 1 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` pada branch `HamzahV2` (upstream `origin/HamzahV2`) untuk source; `NewQuilvianSystemBackend` **hanya** untuk berkas laporan ini beserta tautan buktinya pada roadmap dan `requirement-traceability.md` |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `72531b8a51f7cc358557fff2e4b0ecf86b67065b` |
| Commit backend yang dijadikan rujukan | `f102020611fc3d605fdef1949a3af23da93e4215` |
| Tanggal | 2026-08-28 |
| Status | **Selesai sebagian.** Kriteria 1, 3, 4, dan 5 terpenuhi. Kriteria 2 **belum terpenuhi** karena kontrak backend tidak menyediakan datanya — rinciannya pada bagian 7 dan 8 |

---

## 1. Keadaan yang ditemukan di awal

Modul Rawat Inap sudah punya tiga belas route dan delapan butir menu, tetapi **tidak satu pun
di antaranya dapat menemukan episode menurut statusnya**. Yang paling dekat adalah layar
Pasien Sedang Dirawat (census), dan menurut definisinya census hanya memuat pasien yang sedang
menempati tempat tidur.

Akibatnya dua hal berikut tidak dapat dilakukan siapa pun:

| Yang tidak dapat dilakukan | Buktinya |
| --- | --- |
| Menemukan admisi yang ditinggal di tengah jalan | Episode berstatus `Draft` belum menempati tempat tidur, sehingga tidak pernah masuk census. Dibuktikan dari `InpCensusQueryService` yang membaca baris penempatan aktif |
| Mencapai layar sesi koreksi | Tautan `Sesi Koreksi` hanya dirender di dalam layar detail episode (`inpatient-episode-detail-view.jsx:377`), dan satu-satunya jalan menuju detail episode adalah baris census — yang tidak pernah memuat episode `Closed` |

Layar sesi koreksi sendiri **sudah selesai dikerjakan** lewat `FE-RWI-018`; yang hilang hanya
jalan menuju ke sana. Inilah keadaan yang dijadikan aturan tersendiri oleh `IA-INP-04`: layar
yang tidak terjangkau dari mana pun dihitung belum selesai, walaupun kodenya ada dan test-nya
lulus.

Dua operasi HTTP yang menganggur sejak revision `0.3` — `GET /episodes` dan
`GET /episodes/filters/metadata` — memang ditujukan untuk layar ini, sesuai bagian 11A.
Keduanya diperiksa langsung ke source backend saat ini dan terbukti sudah ada.

---

## 2. Proses bisnis dari sisi pengguna

**Siapa yang memakainya.** Setiap peran yang punya hak akses `InpatientEpisode : Read` —
petugas admisi, perawat, kepala ruangan, DPJP, kasir, dan supervisor.

**Kapan dibuka.** Ketika seseorang perlu mencari satu episode tertentu dan tidak tahu pasti
pasiennya sedang di ruangan atau tidak. Contoh yang paling sering: petugas admisi yang
kemarin sore meninggalkan pendaftaran seorang pasien di tengah jalan, atau supervisor yang
perlu membetulkan catatan pada episode yang sudah ditutup.

**Langkah pemakaian, berurutan.**

1. Pengguna membuka menu **Rawat Inap → Daftar Kerja Episode**.
2. Layar langsung menampilkan seluruh episode, diurutkan dari admisi yang paling baru dibuka.
   Berbeda dari census, daftar ini memuat **semua** status sekaligus — yang sedang disiapkan,
   yang sedang dirawat, yang boleh pulang, yang sudah ditutup, dan yang dibatalkan.
3. Pengguna mempersempit daftar memakai penyaring: rentang tanggal admisi dibuka, unit
   layanan, kelas perawatan, status episode, dan kotak pencarian untuk nama pasien, nomor
   rekam medis, atau nomor episode. Setiap kali penyaring diubah, pembacaan kembali ke
   halaman pertama supaya pengguna tidak melihat halaman kosong hanya karena halaman aktifnya
   melampaui hasil baru.
4. Setiap baris memberi tahu keadaan episode dalam kalimat, bukan sekadar nama status.
   Contohnya: baris berstatus **Sedang disiapkan** disertai keterangan "Pasien belum tentu ada
   di kamar", sedangkan baris **Rencana pulang** disertai "Sudah boleh pulang, tempat tidur
   masih dipegang". Dua kalimat itu berbeda tindakan lanjutannya, karena itu tidak boleh
   disamakan.
5. Pengguna menekan tombol **Detail Episode** pada baris yang dicari, dan mendarat di ruang
   kerja episode. Dari sana seluruh layar per-episode dapat dicapai — perpindahan pasien,
   pencatatan kepergian, kebutuhan isolasi, keputusan pulang, kelayakan keuangan, penutupan,
   dan **sesi koreksi** bagi supervisor pada episode yang sudah ditutup.

**Jalur tidak normal.**

- **Tidak ada hasil.** Layar menjelaskan kenapa kosong sekaligus memberi tahu jalan keluarnya:
  admisi yang ditinggal berstatus *Sedang disiapkan*, sedangkan episode yang perlu dikoreksi
  berstatus *Sudah ditutup*.
- **Gagal memuat.** Pesan dari server ditampilkan apa adanya, ditambah tombol **Coba Lagi**.
  Layar tidak menerjemahkan ulang alasan penolakan menjadi kalimat umum.
- **Tanpa hak akses.** Layar tidak dibuka sama sekali; yang muncul adalah halaman "Ups! Akses
  Ditolak", bukan daftar kosong yang menyesatkan.
- **Pilihan penyaring gagal diambil.** Bila `GET /episodes/filters/metadata` tidak menjawab,
  kelima pilihan status tetap tersedia dari daftar cadangan di layar. Pilihan unit layanan dan
  kelas perawatan sebaliknya dibiarkan kosong — keduanya master data yang hanya server tahu
  isinya, dan mengarangnya akan membuat petugas menyaring memakai unit yang tidak ada.
- **Pengguna berpindah aplikasi lalu kembali.** Daftar dibaca ulang begitu layar difokuskan
  kembali, mengikuti bagian 5.2 yang menandai daftar kerja berisiko basi sedang.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Tata kelola:** `AGENTS.md` frontend; `agents/rules/frontend-architecture.md`;
`agents/rules/REPORT_TEMPLATE.md`.

**Blueprint dan kontrak:** kartu task `FE-RWI-020` pada `frontend-roadmap.md` revision `3`;
`blueprint-manifest.md` revision `4`; `03-frontend-architecture.md` revision `0.4` bagian 2,
2A, 2B, 2C, 4.0, 4.1, 5.1, 5.2, 6, 9, 10, dan 11A; `contracts/api-contract.md` bagian
Inpatient Episode dan Bed Occupancy.

**Source backend (dibaca, tidak diubah):** `InpatientEpisodeController.cs`;
`InpEpisodeService.Reads.cs` (`GetEpisodeListAsync`, `GetFilterMetadataAsync`,
`BuildFilteredEpisodeQuery`, `BuildEpisodeStatusOptions`, `ExpireDueDraftEpisodesAsync`);
`InpatientEpisodeReadDtos.cs`; `InpatientEpisodeDtos.cs`; `InpatientBedOccupancyDtos.cs`;
`InpatientMonitoringDtos.cs`; `InpatientSharedDtos.cs`; `Enums/InpEpisodeStatus.cs`;
`InpBedOccupancyService.cs` bagian papan tempat tidur dan pemesanan.

**Source frontend:** modul referensi visual terdekat `inpatient-census-view.jsx` beserta
hook, constants, dan utils-nya; `inpatient-monitoring-view.jsx`; `inpatient-bed-drift-view.jsx`;
`inpatient-episode-detail-view.jsx`; `inpatient-api.service.js`;
`inpatient-episode.service.js`; `inpatient-management-slice.jsx`;
`inpatient-episode-constants.jsx`; `inpatient-episode-utils.jsx`;
`inpatient-setting-utils.jsx`; base component `hero.jsx`, `data-filter.jsx`,
`data-table.jsx`, `filter-select.jsx`, `filter-date-picker.jsx`, `status-badge.jsx`,
`base-button.jsx`, `information-alert.jsx`, `access-denied-gate.jsx`,
`pagination/pagination.jsx`; `menu-items.jsx`; pola test `tests/unit/inpatient-census.test.mjs`
dan `tests/e2e/inpatient-census.spec.mjs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/inpatient-management/inpatient-episode-worklist-constants.jsx` | **Baru.** Route, nilai bawaan penyaring, pilihan ukuran halaman, daftar cadangan lima status, daftar kolom yang diizinkan dan yang terlarang, kalimat wajib per status sesuai tabel 4.1, dan pembangun tautan berpenyaring untuk beranda |
| `src/utils/health-services/inpatient-management/inpatient-episode-worklist-utils.jsx` | **Baru.** Fungsi murni: penyusun query, penyaring izin kolom, normalisasi daftar dan metadata, pemeriksa kolom terlarang, penerjemah status, dan penyusun teks lokasi |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-episode-worklist.jsx` | **Baru.** Controller layar: membaca kedua endpoint, mengelola penyaring dan paginasi, membaca penyaring status dari URL, dan memuat ulang saat layar difokuskan kembali |
| `src/components/view/health-services/inpatient-management/inpatient-episode-worklist-view.jsx` | **Baru.** Komposisi layar dari base component yang sudah ada |
| `src/app/health-services/inpatient-management/episodes/page.jsx` | **Baru.** Route tipis berisi metadata judul dan pemanggilan view |
| `src/lib/state/slice/health-services/inpatient-management/inpatient-management-slice.jsx` | **Diubah.** Satu kunci resource baru `episodeWorklist` pada `INPATIENT_RESOURCE_KEYS` |
| `src/utils/menu-sidebar/menu-items.jsx` | **Diubah.** Satu butir menu `Daftar Kerja Episode` di bawah Rawat Inap |
| `tests/unit/inpatient-episode-worklist.test.mjs` | **Baru.** Tiga belas test unit atas fungsi murni |
| `tests/e2e/inpatient-episode-worklist.spec.mjs` | **Baru.** Sembilan skenario Playwright |

### 3.3 Kepatuhan arsitektur frontend

**Alur dependensi** mengikuti `agents/rules/frontend-architecture.md` apa adanya:

```text
/health-services/inpatient-management/episodes
  -> src/app/.../episodes/page.jsx            (route tipis, metadata saja)
  -> inpatient-episode-worklist-view.jsx      (komposisi visual)
  -> use-inpatient-episode-worklist.jsx       (controller)
  -> inpatient-episode.service.js             (fondasi FE-RWI-002)
  -> InstanceAxios                            (gerbang HTTP tunggal)
```

| Aturan | Cara dipenuhi |
| --- | --- |
| Route hanya entry point | `page.jsx` berisi `metadata` dan satu pemanggilan view |
| View tidak memanggil Axios | Seluruh pembacaan lewat hook |
| Tidak ada Axios instance baru | Memakai `inpatientEpisodeService` yang sudah dibuat `FE-RWI-002` |
| Endpoint bukan magic string | Base URL berasal dari `INPATIENT_API_BASE_URLS.episodes` |
| Slice terdaftar di store | Reducer `inpatientManagement` sudah terdaftar sejak `FE-RWI-002`; task ini hanya menambah satu kunci resource |
| Request dapat dibatalkan | Kedua pembacaan meneruskan `AbortController.signal` ke Axios |
| Utility berupa fungsi murni | Tidak memakai hook React, tidak merender JSX, tidak membaca Redux |
| Transformasi domain bukan di markup | Penerjemahan status, lokasi, dan query berada di `utils`, bukan di view |

**Kunci resource baru, dan alasannya.** Layar per-episode yang sudah ada memakai kunci
`episodes` untuk menyimpan **satu objek detail**, sedangkan daftar kerja menyimpan **satu
halaman berisi banyak baris**. Bila keduanya berbagi kunci, membuka daftar kerja sesudah
menutup layar detail akan membuat daftar terbuka dengan sisa objek detail yang bentuknya
berbeda. Karena itu ditambahkan satu kunci `episodeWorklist` pada daftar yang sudah ada —
bukan arsitektur state baru, melainkan pemakaian registry yang memang disediakan slice ini.

**Gerbang keputusan base component.**

`UI GATE: 11 elemen — REUSE 11, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Yang dipakai |
| --- | --- | --- | --- | --- |
| Header halaman | `Hero` | `base-features/hero.jsx`, dipakai census dan 100+ view | REUSE | `eyebrow`, `title`, `description` |
| Wadah penyaring dan pencarian | `DataFilter` | `base-features/data-filter.jsx` | REUSE | `searchPlacement="before-last"`, `onReset` |
| Penyaring rentang tanggal | `FilterDatePicker` | `base-features/filter-date-picker.jsx`, dipakai 108 view | REUSE | Dua buah, nilai `YYYY-MM-DD` |
| Penyaring unit layanan | `FilterSelect` | `base-features/filter-select.jsx`, dipakai 115 view | REUSE | Opsi dari `filters/metadata` |
| Penyaring kelas perawatan | `FilterSelect` | idem | REUSE | Opsi dari `filters/metadata` |
| Penyaring status episode | `FilterSelect` | idem | REUSE | Lima opsi dari server, satu opsi "Semua status" |
| Penyaring jumlah baris | `FilterSelect` | idem, pola sama dengan census | REUSE | `icon="☷"` |
| Tabel daftar | `DataTable` | `base-features/data-table.jsx`, dipakai 112 view | REUSE | `sortLatestFirst={false}` karena server sudah menyortir |
| Penanda status dan isolasi | `StatusBadge` | `base-features/status-badge.jsx`, dipakai 108 view | REUSE | `status` diisi nada `pending`/`active`/`warning`/`info`/`inactive` |
| Paginasi | `Pagination` | `features/pagination/pagination.jsx` | REUSE | Lewat `PaginationComponent` |
| Pesan gagal, tombol coba lagi, penjaga hak akses | `InformationAlert`, `BaseButton`, `AccessDeniedGate` | `base-features/` | REUSE | Pola sama persis dengan census |

Tidak ada elemen berstatus `NEW` maupun `EXTEND`, sehingga tidak ada keputusan user yang
tertahan pada gerbang ini. Tidak ada berkas style baru sama sekali; layar memakai
`base-data-components.module.css` yang sudah dipakai seluruh layar daftar Rawat Inap.

**Dua penyimpangan yang disengaja dari modul referensi census, beserta alasannya.**

| Hal | Census | Daftar kerja | Alasan |
| --- | --- | --- | --- |
| Sumber opsi unit layanan dan kelas | `ResourceFilterSelect` + `useSelectResource` (master lengkap) | `FilterSelect` + `GET /episodes/filters/metadata` | Metadata hanya menawarkan unit bertipe `Inpatient` dan kelas ber-`IsForInpatient`. Master lengkap akan menawarkan unit poliklinik yang hasilnya selalu kosong. Selain itu bagian 11A memang menugaskan endpoint ini kepada `FE-INP-16` — memakainya adalah cara endpoint tersebut berhenti menganggur |
| Tombol pada kolom aksi | `<Link className="btn btn-outline-primary btn-sm">` | `<BaseButton as={Link} variant="secondary" size="sm">` | Checklist konsistensi UI melarang class `.btn` Bootstrap untuk aksi baru. `BaseButton` sudah mendukung prop `as`, sehingga tidak ada base component yang diubah |

Penyimpangan kedua menimbulkan selisih tampilan kecil terhadap lima layar daftar Rawat Inap
yang sudah ada, yang semuanya masih memakai `.btn btn-outline-primary btn-sm`. Selisih itu
**tidak diperbaiki** pada task ini karena berada di luar cakupannya; dicatat sebagai utang
teknis pada bagian 8.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Penanda memuat milik `DataTable` beserta kalimat "Mengambil daftar kerja episode...", bukan layar kosong |
| Kosong | "Tidak ada episode yang cocok dengan penyaring ini." disertai jalan keluarnya: "Lepaskan penyaring status atau perlebar rentang tanggalnya. Admisi yang ditinggal berstatus Sedang disiapkan, sedangkan episode yang perlu dikoreksi berstatus Sudah ditutup." |
| Gagal | Pesan dari server ditampilkan apa adanya di dalam `InformationAlert` merah, ditambah tombol **Coba Lagi** yang dinonaktifkan selama pembacaan ulang berjalan |
| Tanpa hak akses | Halaman "Ups! Akses Ditolak" menggantikan seluruh isi layar lewat `AccessDeniedGate`; tabelnya tidak dirender sama sekali |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Episode

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/inpatient-management/episodes/filters/metadata` | Mengisi pilihan status episode, unit layanan rawat inap, kelas perawatan rawat inap, dan pilihan jumlah baris | `InpatientEpisode : Read` |
| `GET` | `/v1/health-services/inpatient-management/episodes` | Membaca daftar episode beserta penyaring `search`, `serviceUnitId`, `patientClassId`, `episodeStatus`, `startDate`, `endDate`, dan paginasinya | `InpatientEpisode : Read` |

**Kedua endpoint inilah yang berhenti menganggur** oleh task ini. Keduanya tercatat pada
`03-frontend-architecture.md` bagian 11A sebagai operasi yang menganggur pada revision `0.3`
dengan pemilik baru `FE-INP-16`. Tujuh operasi menganggur lainnya masih menunggu task
berikutnya.

Catatan perilaku yang perlu diketahui pembaca laporan: pembacaan daftar dan ringkasan di
backend **menjalankan perhitungan kedaluwarsa episode `Draft` lebih dulu**
(`ExpireDueDraftEpisodesAsync`). Artinya membuka layar ini ikut menggugurkan admisi yang sudah
telantar melewati `DraftEpisodeExpiryHours`. Itu perilaku backend yang memang disengaja
(`RWI-DEC-030` memilih tidak memakai program penjadwal), bukan efek samping layar.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` atas kesembilan berkas yang ditambah dan diubah | Berhasil tanpa error dan tanpa peringatan | `PASS` | Keluaran perintah kosong |
| `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-episode-worklist.test.mjs` | 13 test, 13 lulus | `PASS` | `ℹ tests 13 / ℹ pass 13 / ℹ fail 0` |
| `node --import ./tests/helpers/register.mjs --test "tests/unit/*.test.mjs"` (seluruh suite) | 258 test, 257 lulus, 1 gagal | `EXISTING / ENVIRONMENT ISSUE` | Yang gagal `tests/unit/auth-security.test.mjs`, karena mengimpor `src/utils/auth/base-login-utils.jsx` sedangkan berkas yang ada bernama `base-login-utils.js`. Berkas test itu terakhir disentuh commit `0fff36596`, jauh sebelum task ini, dan tidak berhubungan dengan Rawat Inap. Tidak diperbaiki karena di luar cakupan |
| `npm run test:unit` | Gagal sebelum test mana pun berjalan | `EXISTING / ENVIRONMENT ISSUE` | `ERR_UNSUPPORTED_DIR_IMPORT` pada `tests/unit`. Script meneruskan **direktori** ke `node --test`, dan resolver alias repository tidak mendukungnya pada Node `v24.13.0` yang terpasang. Suite yang sama berjalan normal bila diberi pola berkas. Bukan akibat perubahan task ini |
| `npm run lint:errors` seluruh repository | Tidak dijalankan | `NOT RUN` | Dihentikan pengguna |
| `npm run build` | Tidak dijalankan | `NOT RUN` | Pengguna meminta pengujian dilewati |
| `npm run test:e2e` | Tidak dijalankan | `NOT RUN` | Pengguna meminta pengujian dilewati. Repository juga tidak memiliki `playwright.config.*` dan spec-nya membutuhkan aplikasi yang sedang berjalan pada `UAT_BASE_URL` |

**Uji manual:** `NOT FEASIBLE`.

Alasan konkretnya: memverifikasi kontrol interaktif di layar ini menuntut aplikasi berjalan
(`npm run dev` atau `npm run build` lalu `npm run start`), dan pengguna secara eksplisit
meminta pengujian dilewati pada sesi ini. Kontrol yang **belum** diverifikasi secara manual
karena itu adalah: daftar pilihan pada kelima penyaring, label dan keadaan terpilihnya,
pengaruhnya terhadap permintaan dan hasil, tombol atur ulang, gabungan beberapa penyaring
sekaligus, dan paginasi.

Sebagai gantinya, kesembilan skenario Playwright pada
`tests/e2e/inpatient-episode-worklist.spec.mjs` **sudah ditulis** dan menutup persis daftar di
atas — termasuk pemeriksaan bahwa nilai `episodeStatus=0` benar-benar sampai ke server. Spec
itu **belum dijalankan**, dan laporan ini tidak mengklaim hasilnya.

**Tidak dijalankan:** lint seluruh repository, build, dan e2e — atas permintaan pengguna.
Kesimpulan bahwa kode ini bebas dari kesalahan build karena itu **belum dapat ditegakkan**;
yang sudah ditegakkan hanya lint atas berkas yang diubah dan test unit atas fungsi murninya.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Kelima nilai status episode dapat disaring, termasuk `Draft`, `Cancelled`, dan `Closed` | **Terpenuhi** | Pilihan status diambil dari `GET /episodes/filters/metadata` yang mengembalikan kelima nilai, dengan daftar cadangan lima nilai bila metadata gagal. Test unit `kriteria 1: kelima nilai status dapat disaring, dan Draft yang bernilai nol tidak dijatuhkan` membuktikan nilai `0` benar-benar terkirim sebagai `episodeStatus=0` — jebakan terbesar layar ini, karena `Draft` bernilai nol dan penyaring yang memakai pemeriksaan kebenaran biasa akan membuangnya diam-diam |
| 2. Baris `Draft` yang masih memegang pemesanan tempat tidur terbeda dari yang pemesanannya sudah gugur, dan sisa waktunya terbaca | **Belum terpenuhi** | **Terhalang kontrak backend.** Rinciannya di bawah tabel ini |
| 3. Setiap baris membuka detail episode | **Terpenuhi** | Kolom aksi merender tautan ke `buildInpatientEpisodeDetailRoute(item.id)`. Test unit membuktikan setiap baris membawa `id` episode; skenario e2e `kriteria 3` menelusuri jalur penuh baris → detail → sesi koreksi |
| 4. Kolom sensitif — diagnosis, catatan episode, keterangan isolasi — tidak muncul | **Terpenuhi** | Dua lapis, mengikuti pola `FE-RWI-008`. Lapis pertama: `EPISODE_WORKLIST_ALLOWED_FIELDS` menjatuhkan apa pun di luar daftar sebelum data masuk pohon React — test unit menyuntikkan `notes`, `isolationNote`, `diagnosis`, dan `clinicalSummary` lalu membuktikan keempatnya hilang. Lapis kedua: `findForbiddenEpisodeWorklistFields` memeriksa payload yang benar-benar sampai ke browser. Dibaca langsung dari DTO backend, `InpatientEpisodeListItemResponse` memang tidak memuat kolom sensitif; lapis pertama karena itu penjaga terhadap perubahan di kemudian hari, bukan terhadap keadaan hari ini |
| 5. Keempat keadaan daftar bagian 5.1 terpenuhi | **Terpenuhi** | Keempatnya diuraikan pada bagian 4 laporan ini dan ditutup empat skenario e2e |

### Kenapa kriteria 2 belum terpenuhi

Kriteria ini menuntut dua hal yang **tidak ada satu pun endpoint backend menyediakannya**:

1. apakah sebuah episode `Draft` masih memegang pemesanan tempat tidur; dan
2. sisa waktu pemesanan itu.

Buktinya, dibaca langsung dari source backend pada commit `f102020611`:

| Yang diperiksa | Temuan |
| --- | --- |
| `InpatientEpisodeListItemResponse` | Tidak punya kolom pemesanan sama sekali. `CurrentBedName` dan `CurrentRoomName` dibaca dari `InpBedPlacement` — **penempatan**, bukan pemesanan. Episode `Draft` menurut definisinya belum punya penempatan, sehingga kedua kolom itu selalu kosong untuk baris `Draft` |
| `InpatientEpisodeDetailResponse` | Sama. Tidak memuat pemesanan aktif maupun waktu gugurnya |
| Seluruh 44 endpoint modul Rawat Inap | Kolom `ExpiresAt` hanya dikembalikan `BedReservationResponse`, dan DTO itu hanya dipakai `POST /bed-occupancies/reservations` dan `PATCH /bed-occupancies/reservations/{id}/cancel`. **Tidak ada satu pun operasi baca** yang mengembalikannya |
| `GET /bed-occupancies/bed-board` | Punya `IsReserved` dan `HoldingEpisodeNumber`, tetapi **tanpa** `ExpiresAt`. Endpoint ini juga berada di luar cakupan task, dan memakainya berarti membaca papan seluruh rumah sakit pada setiap pembacaan daftar |

Bagian 5.2 blueprint sendiri menegaskan bahwa "sisa waktu pemesanan pada baris `Draft` wajib
berasal dari jawaban server terakhir". Karena jawaban server tidak pernah memuatnya, angka itu
**tidak boleh dikarang di layar** — menghitungnya sendiri berarti menampilkan tenggat yang
tidak ada dasarnya, dan petugas akan melepas tempat tidur yang sebenarnya masih dipegang, atau
sebaliknya.

Sesuai batas keselamatan lintas repository pada `AGENTS.md`, backend **tidak diubah** dan
bagian task ini dihentikan. Pilihan penyelesaiannya disampaikan kepada pemilik modul.

Yang **sudah** dikerjakan sejauh kontrak mengizinkan: baris `Draft` terbaca jelas sebagai
"Sedang disiapkan" beserta kalimat "Pasien belum tentu ada di kamar", dan kolom lokasinya
berbunyi "Belum menempati tempat tidur" — bukan tanda hubung yang terbaca seperti data hilang.

### Definition of Done

| Butir DoD | Status |
| --- | --- |
| Kelima kriteria lulus | **Belum** — empat dari lima. Kriteria 2 terhalang kontrak backend |
| E2E ada dan lulus | **Sebagian** — spec berisi sembilan skenario sudah ada, tetapi **belum dijalankan** atas permintaan pengguna |
| Laporan menyebut endpoint mana yang berhenti menganggur | **Terpenuhi** — `GET /episodes` dan `GET /episodes/filters/metadata`, lihat bagian 5 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan lint pada berkas yang ditambah maupun diubah |
| Masalah yang diketahui | (1) Kriteria 2 terhalang kontrak backend — lihat bagian 7. (2) `tests/unit/auth-security.test.mjs` gagal karena mengimpor `base-login-utils.jsx` sedangkan berkas yang ada `.js`; masalah lama, tidak berhubungan, tidak diperbaiki. (3) `npm run test:unit` gagal pada Node `v24.13.0` karena script meneruskan direktori ke `node --test`; suite berjalan normal bila diberi pola berkas |
| Utang teknis di luar cakupan | Lima layar daftar Rawat Inap yang sudah ada (`census`, `monitoring`, `bed-drift`, `closure`, `episode-detail`) masih memakai `.btn btn-outline-primary btn-sm` pada tombol barisnya, yang dilarang checklist konsistensi UI. Layar baru ini memakai `BaseButton`. Penyeragamannya layak dikerjakan sekaligus, bukan satu per satu — `FE-RWI-033` adalah tempat yang wajar |
| Dependency backend | `NONE`. Kedua endpoint yang dipakai sudah tersedia dan dibaca langsung dari source |
| Selisih snapshot yang ditemukan | Metadata roadmap menyebut `source_commits.backend` = `5afb54bd75...`. Pada commit itu folder `Areas/HealthServices/InPatientManagement` **belum berisi implementasinya**; source modul Rawat Inap baru masuk pada commit-commit sesudahnya. Kontrak yang dipakai task ini karena itu diverifikasi terhadap source backend **saat ini** (`f102020611`), yang isinya cocok dengan `contracts/api-contract.md` `0.4.0`. Selisih ini tidak menghalangi task, tetapi metadata roadmap layak dimutakhirkan |
| Perubahan sampingan | `NONE` |
| Interupsi | Pengguna menghentikan `npm run lint:errors` dan meminta pengujian dilewati. Pekerjaan dilanjutkan dari kondisi terverifikasi terakhir; tidak ada perubahan yang diulang maupun digandakan |
| Status Git | `M src/lib/state/slice/health-services/inpatient-management/inpatient-management-slice.jsx`<br>`M src/utils/menu-sidebar/menu-items.jsx`<br>`?? src/app/health-services/inpatient-management/episodes/page.jsx`<br>`?? src/components/view/health-services/inpatient-management/inpatient-episode-worklist-view.jsx`<br>`?? src/lib/constants/health-services/inpatient-management/inpatient-episode-worklist-constants.jsx`<br>`?? src/lib/hooks/health-services/inpatient-management/use-inpatient-episode-worklist.jsx`<br>`?? src/utils/health-services/inpatient-management/inpatient-episode-worklist-utils.jsx`<br>`?? tests/e2e/inpatient-episode-worklist.spec.mjs`<br>`?? tests/unit/inpatient-episode-worklist.test.mjs`<br>Tidak ada `git add`, commit, maupun push |
| Langkah berikutnya | Jalankan `npm run lint:errors`, `npm run build`, lalu e2e ketika pengujian dibuka kembali; sesudah itu putuskan penyelesaian kriteria 2 bersama pemilik Backend/API |

---

## Pemeriksaan ulang status — 1 September 2026

Task ini **tidak** dinaikkan menjadi selesai ketika `FE-RWI-021`, `022`, `023`, `024`, dan `026`
ditutup pada 1 September 2026. Alasannya berbeda jenis: kelima task itu source-nya lengkap dan
yang kurang hanya bukti, sedangkan di sini yang kurang adalah source-nya sendiri.

| Yang diperiksa | Hasil |
| --- | --- |
| `inpatient-episode-worklist-view.jsx` | Tidak memakai `holdingEpisodeId`, `reservationId`, maupun `reservationExpiresAt` |
| `use-inpatient-episode-worklist.jsx` | Sama; tidak memanggil `GET /bed-occupancies/bed-board` |
| `inpatient-episode-worklist-utils.jsx` | Sama |
| `inpatient-episode-worklist-constants.jsx` | Hanya memuat label status `Sedang disiapkan`; tidak ada kolom pemesanan |
| Pemakai ketiga field itu di seluruh `src/` | Hanya `use-inpatient-admission-bed.jsx` dan `inpatient-bed-utils.jsx`, keduanya milik `FE-RWI-026` |

Akibatnya kriteria 2 tetap **belum terpenuhi**: baris `Draft` yang masih memegang pemesanan belum
terbeda dari yang pemesanannya sudah gugur, dan sisa waktunya belum terbaca.

Blocker backendnya sudah gugur — `BE-RWI-036` menyediakan `HoldingEpisodeId`, `ReservationId`,
dan `ReservationExpiresAt` pada `GET /bed-occupancies/bed-board` lewat kontrak approved
`RWI-BED-BOARD-RESERVATION-001 1.0.0`. Yang tersisa murni pekerjaan frontend: menggabungkan
metadata board itu ke baris `Draft` pada daftar kerja.

**Keputusan pengecualian verifikasi 1 September 2026 tidak berlaku untuk task ini**, karena
pengecualian itu hanya melepas butir DoD e2e/`.mjs`/uji manual, bukan acceptance criteria yang
source-nya belum ada.
