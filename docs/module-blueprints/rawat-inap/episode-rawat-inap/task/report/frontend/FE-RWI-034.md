# Laporan Perubahan Frontend — `FE-RWI-034`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-034` |
| Judul | Layar admisi lama dibongkar, jalur gandanya hilang |
| Slice | `F13 — Perapian dan kesiapan` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian 5, `FE-RWI-034` |
| Trace | `RWI-DEC-079`; `04-prd-to-mvp.md` bagian 2C "satu kemampuan, satu tempat" |
| Skema tampilan | [`05-skema-tampilan.md`](../../../05-skema-tampilan.md) — alur target bagian 3, klasifikasi `CONFLICT/REPLACE` bagian 24 |
| Contract version | Tidak ada kontrak yang disentuh. Task ini hanya membuang source yatim dan memindahkan fungsi yang masih dipakai |
| Wewenang UI | Tidak ada — dan memang tidak dibutuhkan; tidak ada satu pun piksel yang berubah |
| Dependency | `FE-RWI-027` selesai. Layar `/admissions` sudah dilayani alur berlangkah `inpatient-admission-view.jsx` |
| Klasifikasi | `LIGHT` — tiga berkas source dihapus, tiga disunting, dua berkas test dihapus, tiga berkas test diarahkan ulang |
| Task mode | `FRONTEND` — backend strict read-only, kecuali berkas laporan dan register modul ini |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; berkas laporan ini beserta roadmap dan `requirement-traceability.md` modul Rawat Inap |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `2535c1303` pada branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `3d14cac` pada branch `MHamzah` |
| Tanggal | 1 September 2026 |
| Status | ✅ **SELESAI 1 September 2026.** Keempat acceptance criteria terpenuhi. `npm run lint` `0 errors` — 571 warning, sama persis dengan garis dasar; `npm run build` `✓ Compiled successfully in 34.0s`. `test:unit` `NOT RUN` atas arahan pengguna; kriteria 4 dilaporkan apa adanya pada bagian 7 |

---

## 1. Keadaan yang ditemukan di awal

Formulir admisi tunggal **sudah tidak terpasang di layar mana pun**, tetapi berkasnya masih
hidup di repository. `FE-RWI-021` s.d. `027` sudah mengganti isi
`inpatient-admission-view.jsx` menjadi alur berlangkah, sehingga route `/admissions` menuju
alur baru. Yang tertinggal adalah tiga berkas pendukung formulir lama:

| Berkas | Keadaan yang ditemukan | Siapa yang masih mengacu |
| --- | --- | --- |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission.jsx` (301 baris) | **Yatim penuh.** Tidak ada satu pun source yang mengimpornya | Hanya dua berkas test |
| `src/utils/health-services/inpatient-management/inpatient-admission-utils.jsx` (105 baris) | **Sebagian masih dipakai.** Enam fungsi mati, lima masih hidup | `use-inpatient-episode-detail.jsx`, `use-inpatient-admission-doctor.jsx` |
| `src/lib/constants/health-services/inpatient-management/inpatient-admission-constants.jsx` (81 baris) | **Hampir seluruhnya mati.** Hanya `INPATIENT_ADMISSION_ROUTE` yang masih diacu | `inpatient-dashboard-constants.jsx` |

Dan inilah jalur ganda yang sebenarnya, yang bahkan lebih halus daripada yang diperkirakan
roadmap: **`INPATIENT_ADMISSION_ROUTE` didefinisikan dua kali.** Sekali di
`inpatient-admission-constants.jsx` (diturunkan dari `INPATIENT_MANAGEMENT_ROUTE_BASE`), dan
sekali lagi di `inpatient-admission-flow-constants.jsx` sebagai teks alamat yang ditulis
penuh. Beranda memakai definisi lama, alur pelanjutan admisi memakai definisi baru. Selama
kedua nilainya kebetulan sama, tidak ada yang terlihat salah — tetapi begitu alamat admisi
suatu hari berubah, hanya satu di antaranya yang ikut berubah, dan tautan cepat beranda akan
menunjuk halaman yang tidak ada.

Dua berkas test juga menguji formulir tunggal yang sudah tidak ada:
`tests/unit/inpatient-admission.test.mjs` dan `tests/e2e/inpatient-admission.spec.mjs`
(629 baris, menggerakkan tombol **Buka Admisi** milik formulir lama). Ditambah dua test di
`tests/unit/inpatient-placement.test.mjs` yang membaca source hook lama.

---

## 2. Proses bisnis dari sisi pengguna

**Tidak ada satu pun yang berubah di layar.** Ini penting untuk dicatat apa adanya: task ini
membuang kode yang sudah tidak dapat dicapai pengguna, bukan mengubah cara kerja apa pun.

Alur admisi sebelum dan sesudah task ini sama persis: petugas admisi membuka
`Pelayanan Kesehatan → Rawat Inap → Admisi Rawat Inap`, memilih tipe pendaftaran, lalu
menempuh sembilan langkah — Tipe Pasien, Pendaftaran, Pembayaran, Dokter, Pilih Bed, Booking
Bed, Konfirmasi, Cetak Persetujuan, dan Kartu Pasien. Alur itu milik `FE-RWI-021` s.d. `027`
dan tidak disentuh.

Yang berubah adalah risikonya ke depan. Sebelum task ini, siapa pun yang membuka repository
menemukan dua kumpulan kode admisi berdampingan: satu berlangkah dan satu formulir tunggal.
Formulir tunggal itu memanggil `POST /episodes` tanpa `EncounterId`, yaitu jalur yang menurut
`RWI-OQ-046` menanam cara bayar tunai beserta kunjungan tanpa penjamin. Orang yang keliru
memilihnya sebagai contoh — atau menyalakannya kembali dari sebuah route baru — akan
menghidupkan ulang cacat penjamin yang sudah ditutup `FE-RWI-025`.

### 2.1 Yang dipindahkan, bukan dibuang

Lima fungsi pada `inpatient-admission-utils.jsx` ternyata masih hidup dan dipakai layar yang
berbeda. Semuanya **dipindahkan** ke `inpatient-episode-utils.jsx`, isinya tidak diubah
sedikit pun:

| Fungsi | Dipakai untuk apa di layar | Pemakai |
| --- | --- | --- |
| `validateIsolationForm` | Menolak penyimpanan kebutuhan isolasi yang dinyalakan tanpa keterangan | Detail Episode |
| `canSubmitIsolation` | Menyalakan atau mematikan tombol **Simpan Kebutuhan Isolasi** | Detail Episode |
| `buildIsolationPayload` | Menyusun isi `PATCH /episodes/{id}/isolation-requirement` | Detail Episode; langkah Dokter alur admisi |
| `getEpisodeId` | Membaca id episode dari jawaban server yang ejaan kolomnya bisa berhuruf kapital | Langkah Dokter alur admisi |
| `getEpisodeNumber` | Membaca nomor episode dari jawaban yang sama | Langkah Dokter alur admisi |

`inpatient-episode-utils.jsx` dipilih sebagai rumah barunya karena di situlah seluruh aturan
episode lain sudah tinggal — termasuk `resolveIsolationAuthority`, yaitu penjaga kewenangan
tombol isolasi yang selama ini terpisah dari validasinya. Setelah pemindahan ini, kewenangan
dan validasi kebutuhan isolasi berada dalam satu berkas. Pembantu `toText`, `getByKeys`, dan
`normalizeBoolean` yang dibutuhkan kelima fungsi itu sudah ada di sana, jadi tidak ada
pembantu yang ikut terduplikasi.

Enam fungsi lain — `buildAdmissionPayload`, `validateAdmissionForm`,
`mapEpisodeToAdmissionForm`, `getEpisodeStatusName`, `episodeRequiresIsolation`, dan
`getEpisodeIsolationNote` — ikut terhapus bersama berkasnya karena tidak ada satu pun
pemanggil tersisa. Keempat konfigurasi formulir tunggal (`INPATIENT_ADMISSION_FORM_FIELDS`,
`INPATIENT_ADMISSION_FORM_DEFAULTS`, `INPATIENT_ADMISSION_CONFIG`, `ADMISSION_STEPS`) juga
demikian.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Untuk apa |
| --- | --- |
| `docs/module-blueprints/rawat-inap/roadmap/frontend-roadmap.md` | Acceptance criteria, scope, dan larangan menandai test sebagai dilewati |
| `docs/module-blueprints/rawat-inap/05-skema-tampilan.md` bagian 3 dan 24 | Alur admisi target; klasifikasi `CONFLICT/REPLACE` untuk `FE-INP-03` |
| `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx` | Membuktikan route `/admissions` memang sudah dilayani alur berlangkah |
| `src/app/health-services/inpatient-management/admissions/page.jsx` | Membuktikan tidak ada route kedua menuju formulir lama |
| `src/lib/hooks/.../use-inpatient-admission.jsx` | Memetakan bagian yang masih terpakai sebelum dihapus |
| `src/utils/.../inpatient-admission-utils.jsx` | idem |
| `src/lib/constants/.../inpatient-admission-constants.jsx` | idem |
| `src/lib/constants/.../inpatient-admission-flow-constants.jsx` | Menemukan definisi kedua `INPATIENT_ADMISSION_ROUTE` |
| `src/utils/.../inpatient-episode-utils.jsx` | Memastikan tidak ada nama yang bertabrakan sebelum pemindahan |
| `src/lib/hooks/.../use-inpatient-bed-board-actions.jsx` | Pemilik penempatan pasien yang sekarang, tujuan pengarahan test |
| `tests/unit/inpatient-admission.test.mjs`, `tests/unit/inpatient-placement.test.mjs`, `tests/e2e/inpatient-admission.spec.mjs` | Menentukan test mana yang dihapus dan mana yang diarahkan ulang |

### 3.2 Berkas yang berubah

**Dihapus:**

| Berkas | Alasan |
| --- | --- |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission.jsx` | Yatim penuh; formulir tunggal yang dilayaninya sudah tidak ada |
| `src/utils/health-services/inpatient-management/inpatient-admission-utils.jsx` | Lima fungsi yang masih hidup dipindahkan lebih dulu; enam sisanya yatim |
| `src/lib/constants/health-services/inpatient-management/inpatient-admission-constants.jsx` | `INPATIENT_ADMISSION_ROUTE` sudah punya definisi kembar di flow-constants; sisanya konfigurasi formulir tunggal |
| `tests/unit/inpatient-admission.test.mjs` | Menguji formulir tunggal. Tiga pemeriksaan yang masih relevan dipindahkan, bukan dihapus |
| `tests/e2e/inpatient-admission.spec.mjs` | 629 baris yang menggerakkan tombol **Buka Admisi** milik formulir lama; tidak ada lagi layar yang menampilkannya |

**Disunting:**

| Berkas | Perubahan |
| --- | --- |
| `src/utils/health-services/inpatient-management/inpatient-episode-utils.jsx` | Menerima kelima fungsi yang dipindahkan, lengkap dengan catatan asal-usulnya |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-episode-detail.jsx` | Blok import dari `inpatient-admission-utils` dilebur ke blok `inpatient-episode-utils` yang sudah ada — satu sumber, bukan dua |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-doctor.jsx` | Sumber import diarahkan ke `inpatient-episode-utils` |
| `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx` | `INPATIENT_ADMISSION_ROUTE` tidak lagi ditulis sebagai alamat penuh, melainkan diturunkan dari `INPATIENT_MANAGEMENT_ROUTE_BASE` — jalur turunan yang dulu dipegang berkas yang dihapus |
| `src/lib/constants/health-services/inpatient-management/inpatient-dashboard-constants.jsx` | Import `INPATIENT_ADMISSION_ROUTE` diarahkan ke flow-constants. Beranda dan alur pelanjutan kini membaca satu definisi yang sama |
| `tests/unit/inpatient-episode-detail.test.mjs` | Menerima tiga pemeriksaan yang dipindahkan dari test admisi lama: keterangan isolasi wajib, payload isolasi tidak mengirim `IsolationSource`, dan pembacaan identitas episode |
| `tests/unit/inpatient-placement.test.mjs` | Dua pemeriksaan yang membaca hook lama diarahkan ke `use-inpatient-bed-board-actions.jsx` — pemilik `POST /bed-occupancies/placements` yang sekarang |

### 3.3 Kepatuhan arsitektur frontend

- **Fungsi murni tetap di `utils`.** Kelima fungsi yang dipindahkan tidak memakai hook React,
  tidak merender JSX, tidak melakukan request, dan tidak membaca Redux store — persis syarat
  `src/utils` pada `rules/frontend/frontend-architecture.md`.
- **Tidak ada duplikasi helper.** `toText`, `getByKeys`, dan `normalizeBoolean` yang
  dibutuhkan sudah tersedia di berkas tujuan, jadi tidak ada pembantu yang ikut disalin.
- **Endpoint tidak menjadi yatim.** `POST /bed-occupancies/placements` yang dipanggil hook
  lama tetap dipanggil `use-inpatient-bed-board-actions.jsx`;
  `PATCH /episodes/{id}/isolation-requirement` tetap dipanggil Detail Episode dan langkah
  Dokter. Pemeriksaan lengkapnya ada pada [FE-RWI-033](FE-RWI-033.md) bagian 6.4.
- **Tidak ada import melingkar.** `inpatient-admission-flow-constants.jsx` kini mengimpor
  `inpatient-setting-constants.jsx`, yang tidak mengimpor apa pun. Dibuktikan oleh
  `npm run build` yang lulus.
- **Cakupan perubahan tetap sempit.** Tidak ada perbaikan lint, format ulang, maupun refactor
  yang tidak diminta task ini.

`UI GATE: N/A — task ini tidak menyentuh route, view, komponen, maupun style yang terlihat
pengguna. Yang berubah hanya hook, utility, constant, dan test.`

---

## 4. State yang ditangani di layar

`NOT APPLICABLE` — task ini tidak mengubah tampilan. State memuat, kosong, gagal, dan tanpa
hak akses pada layar Admisi Rawat Inap serta Detail Episode tetap persis seperti yang
dilaporkan `FE-RWI-027` dan `FE-RWI-020`.

---

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — tidak ada endpoint baru yang dipanggil dan tidak ada yang berhenti
dipanggil. Kedua endpoint yang tersentuh pemindahan fungsi tetap punya pemanggil:

#### Health Services / Inpatient Management / Inpatient Episode

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/v1/health-services/inpatient-management/episodes/{id}/isolation-requirement` | Menyimpan kebutuhan isolasi. Payload-nya disusun `buildIsolationPayload` yang pindah rumah pada task ini | `InpatientEpisode : SetIsolation` |

#### Health Services / Inpatient Management / Bed Occupancy

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/inpatient-management/bed-occupancies/placements` | Menempatkan pasien ke tempat tidur. Dipanggil `use-inpatient-bed-board-actions.jsx`, bukan lagi hook admisi lama | `InpatientBedOccupancy : Create` |

---

## 6. Verifikasi

### 6.1 Perintah

| Perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint` | `✖ 571 problems (0 errors, 571 warnings)` | `PASS` | Sama persis dengan garis dasar `FE-RWI-030` s.d. `032`. Nol baris menyebut berkas task ini |
| `npm run build` | `✓ Compiled successfully in 34.0s`; `postbuild` selesai | `PASS` | Keluaran perintah. Build inilah yang membuktikan tidak ada import menggantung setelah tiga berkas dihapus — modul yang hilang akan menggagalkan kompilasi, bukan sekadar memperingatkan |
| `npm run test:unit` | Tidak dijalankan | `NOT RUN` | Arahan pengguna 1 September 2026: validasi task ini dibatasi pada `npm run build` dan `npm run lint` |

`AUTOMATED TEST: SKIPPED (opsional) — atas arahan pengguna, berkas .mjs tidak dijalankan pada
sesi ini.`

Karena `test:unit` tidak dijalankan, kelima pemeriksaan test yang dipindahkan diverifikasi
**manual terhadap source** memakai pencarian teks, satu per satu:

| Pemeriksaan yang dipindahkan | Fakta yang diperiksanya | Hasil verifikasi manual |
| --- | --- | --- |
| `validateIsolationForm` dan `canSubmitIsolation` | Kedua fungsi diekspor `inpatient-episode-utils.jsx` | Ada, baris 456 dan 469 |
| `buildIsolationPayload` | Diekspor dari berkas yang sama | Ada, baris 479 |
| `getEpisodeId`, `getEpisodeNumber` | Diekspor dari berkas yang sama | Ada, baris 50 dan 53 |
| Penjaga penempatan ganda | `use-inpatient-bed-board-actions.jsx` memuat `const placementInFlight = useRef(false)` dan `if (placementInFlight.current) return;` | Ada, baris 31 dan 84 |
| Penolakan penempatan tidak membuang keterangan | Blok `catch` pada `submitConfirmAdmission` memuat `setConfirmFailure(failure)` dan `failure.shouldReloadBeds` | Ada, baris 113 dan 116, keduanya di dalam blok `catch` baris 107 s.d. 120 |

### 6.2 Pencarian menyeluruh atas nama berkas lama

Sesuai butir verification roadmap, seluruh repository dicari untuk nama berkas yang dihapus.

| Yang dicari | Hasil |
| --- | --- |
| `inpatient-admission-utils` | Nol hit import. Dua hit tersisa adalah **kalimat komentar** pada `inpatient-episode-utils.jsx` yang mencatat asal-usul fungsi yang dipindahkan; sengaja dipertahankan sebagai jejak, bukan acuan modul |
| `inpatient-admission-constants` | Nol hit |
| `use-inpatient-admission.jsx` | Nol hit. Hit yang tersisa untuk pola `use-inpatient-admission-*` seluruhnya milik delapan hook alur baru — `-flow`, `-patient`, `-bed`, `-doctor`, `-confirmation`, `-exit-guard`, `-payment`, `-resume` |
| `INPATIENT_ADMISSION_CONFIG`, `INPATIENT_ADMISSION_FORM_FIELDS`, `INPATIENT_ADMISSION_FORM_DEFAULTS`, `INPATIENT_ADMISSION_LIMITS`, `ADMISSION_STEPS` | Nol hit |
| `buildAdmissionPayload`, `validateAdmissionForm`, `mapEpisodeToAdmissionForm`, `episodeRequiresIsolation`, `getEpisodeIsolationNote`, `getEpisodeStatusName` | Nol hit |

### 6.3 Uji manual

`MANUAL TEST: NOT APPLICABLE.` Tidak ada kontrol interaktif yang diubah, ditambah, maupun
dihapus. Yang dibuang adalah kode yang sudah tidak dapat dicapai pengguna dari layar mana pun,
dan bukti ketidakterjangkauannya bukan hasil percobaan melainkan hasil pembacaan source:
`admissions/page.jsx` merender `InpatientAdmissionView`, yang seluruh isinya alur berlangkah
`FE-RWI-021` s.d. `027`, dan tidak ada route lain yang menuju formulir tunggal.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Tidak ada lagi route, komponen, atau menu yang membuka formulir admisi tunggal | ✅ Terpenuhi | Route `/admissions` merender alur berlangkah (`admissions/page.jsx` → `inpatient-admission-view.jsx` yang mengimpor delapan hook alur). Hook formulir tunggal dihapus. Submenu Rawat Inap berisi tepat tujuh butir, satu di antaranya Admisi Rawat Inap yang menuju alur baru |
| 2. Tidak ada berkas yatim yang tidak diacu siapa pun | ✅ Terpenuhi | Tabel pencarian menyeluruh bagian 6.2 — nol hit untuk ketiga nama berkas dan untuk kesebelas simbol yang ikut terhapus. `npm run build` lulus, yang membuktikan tidak ada import menggantung |
| 3. Test lama yang menguji formulir tunggal dihapus atau diarahkan ke alur baru — **tidak** dibiarkan dilewati | ✅ Terpenuhi | `tests/unit/inpatient-admission.test.mjs` dan `tests/e2e/inpatient-admission.spec.mjs` **dihapus**, bukan di-skip. Tiga pemeriksaan yang masih relevan **dipindahkan** ke `tests/unit/inpatient-episode-detail.test.mjs`. Dua pemeriksaan pada `tests/unit/inpatient-placement.test.mjs` **diarahkan** ke `use-inpatient-bed-board-actions.jsx`. Tidak ada satu pun `test.skip`, `test.todo`, atau blok yang dikomentari |
| 4. `lint`, `test:unit`, dan `build` lulus | ⚠️ **Terpenuhi sebagian, dan disebut apa adanya.** `lint` `PASS`, `build` `PASS`, `test:unit` `NOT RUN` | Bagian 6.1. `test:unit` tidak dijalankan atas arahan pengguna 1 September 2026 yang membatasi validasi task ini pada `build` dan `lint`, bukan karena terhalang atau gagal. Kelima pemeriksaan yang dipindahkan sudah diverifikasi manual terhadap source pada tabel bagian 6.1 |

### Definition of Done

| Butir | Status |
| --- | --- |
| Kriteria 1 — satu jalan menuju admisi | ✅ |
| Kriteria 2 — tidak ada berkas yatim | ✅ |
| Kriteria 3 — test lama dihapus atau diarahkan, tidak dilewati | ✅ |
| Kriteria 4 — `lint`, `test:unit`, `build` lulus | ⚠️ dua dari tiga dijalankan dan lulus; `test:unit` `NOT RUN` atas arahan pengguna |

Butir DoD "keempat kriteria lulus" karena itu **tidak** diklaim penuh tanpa keterangan.
Menjalankan `npm run test:unit` satu kali akan menutup sisanya; keputusan menjalankannya ada
pada pengguna.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `npm run lint` 571 warning, seluruhnya existing dan nol pada berkas task ini. Klasifikasi: `EXISTING WARNING` |
| Masalah yang diketahui | Alur admisi berlangkah `FE-RWI-021` s.d. `027` sampai hari ini **tidak punya berkas test unit sendiri**. Membongkar test formulir lama karena itu menurunkan angka cakupan modul admisi, walaupun yang hilang adalah cakupan atas kode yang sudah tidak ada. Menulisnya bukan scope task ini dan bukan gerbang selesai menurut `test-policy.md`; pembuktian alur barunya milik `FE-RWI-035` |
| Dependency backend | `NONE`. Tidak ada kontrak yang disentuh |
| Perubahan sampingan | `NONE` yang tidak disengaja. Satu perubahan yang **disengaja** dan dicatat: `INPATIENT_ADMISSION_ROUTE` pada flow-constants diubah dari alamat yang ditulis penuh menjadi turunan `INPATIENT_MANAGEMENT_ROUTE_BASE`. Itu bukan refactor oportunistik melainkan inti kriteria 2 — berkas yang dihapus adalah pemegang jalur turunan tersebut, dan membiarkan alamat ditulis penuh berarti mempertahankan jalur ganda yang justru diminta hilang |
| Interupsi | `NONE` |
| Status Git | `git status --short` pada akhir pekerjaan menampilkan tiga berkas `D` milik source, dua berkas `D` milik test, dan lima berkas `M`, bercampur dengan berkas `FE-RWI-033` yang dikerjakan pada sesi yang sama. Tidak ada `git add`, commit, push, pull, merge, rebase, maupun deploy |
| Langkah berikutnya | Jalankan `npm run test:unit` sekali untuk menutup kriteria 4 sepenuhnya. Sesudah itu `FE-RWI-035` dapat menjadwalkan penulisan e2e alur admisi berlangkah sebagai pengganti spec yang dibongkar task ini |
