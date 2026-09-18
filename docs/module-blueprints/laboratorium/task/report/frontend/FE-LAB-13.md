# Laporan Perubahan Frontend — `FE-LAB-13`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-13` |
| Judul | Kiosk: pilihan layanan Laboratorium |
| Slice | `MVP-5b`, `EPIC-LAB-11` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian 6c |
| Trace | `LAB-DEC-051`, `LAB-DEC-052`, BR-46; wewenang lintas modul `LAB-REQ-006` §1.1; `AC-93` |
| Contract version | Sesi kiosk milik `registration-management`; bentuknya ditetapkan `LAB-REQ-006` §1.1 dan sudah berdiri di source lewat `BE-EXT-04` ✅ serta `BE-EXT-04b` ✅ |
| Wewenang UI | Mengikuti pola layar kiosk yang sudah berjalan — huruf besar, sasaran sentuh lebar, langkah yang dapat dibatalkan. Pasien bukan petugas: nol istilah teknis, dan kata "disiplin" tidak muncul di layar mana pun |
| Dependency | `BE-EXT-04` ✅, `BE-EXT-04b` ✅. **Bukan** `BE-EXT-05`, yang masih ⛔ dan memang tidak dibutuhkan task ini |
| Klasifikasi | `MEDIUM` — 3 berkas baru, 3 berkas diubah, satu cabang alur baru pada layar yang sudah berjalan, nol arsitektur baru, nol berkas gaya diubah |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; repository backend hanya untuk laporan ini beserta tautan buktinya |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `4031fd3d7` pada branch `YogaV2` (perubahan task ini belum di-stage maupun di-commit) |
| Commit backend yang dijadikan rujukan | `e2152709` pada branch `yoga` (dibaca saja, nol berkas backend diubah) |
| Tanggal | 2026-09-16 |
| Status | **`SELESAI`** — keempat butir DoD terpenuhi dan terbukti pada aplikasi yang benar-benar berjalan |

---

## 1. Keadaan yang ditemukan di awal

### 1.1 Kiosknya sudah ada, dan sebagian besar dugaan awal justru memperkecil pekerjaan

Roadmap sudah mencatat bahwa layar kiosk berdiri di repository ini. Itu benar: `src/app/kiosk/`
beserta alur `registration/new-patient`, `registration/old-patient`, `registration/patient-card`,
dan `registration/doctor-schedule`. Alur Pasien Lama bahkan **sudah** memiliki langkah bernama
`Layanan & Dokter`. Yang belum ada hanyalah **tujuan layanannya** — layar itu hanya mengenal
poliklinik dan dokter.

### 1.2 Tiga temuan yang menentukan bentuk pekerjaan, dan ketiganya datang dari membaca source

Ketiganya tidak terlihat dari kartu task, dan ketiganya mengubah jawaban atas pertanyaan
"di mana pilihan Laboratorium dipasang".

**Temuan pertama — kedua ruas hanya dapat ditulis sekali seumur sesi.** `BE-EXT-04b` membuka
jalur tulis `TargetService` dan `HasPhysicianRequest` pada `POST .../kiosk-scan-sessions/scan-result`,
tetapi **sengaja tidak** membangun jalur ubah; laporannya menulis alasannya apa adanya. Akibatnya
konkret: pertanyaan tujuan layanan harus dijawab **sebelum** sesi kiosk dibentuk, bukan sesudahnya.

**Temuan kedua — pada alur Pasien Lama, sesi dibentuk di langkah pertama.**
`kiosk-old-patient-step-find.jsx` memanggil `createOldPatientScanSession` saat kartu pasien
dipindai, yaitu pada langkah `Identifikasi` — lima langkah **sebelum** `Layanan & Dokter`.
Memasang pertanyaan tujuan layanan pada langkah `Layanan & Dokter` karena itu mustahil menulis
apa pun ke sesinya.

**Temuan ketiga, dan ini yang paling menentukan — sesi dari alur Pasien Baru tidak akan pernah
terbaca panel Laboratorium.** Pada alur itu sesi dibentuk di akhir, lalu kunjungan dibentuk
membawa `kioskScanSessionId`-nya. `PatientEncounterController.cs:686` kemudian menandai sesi itu
`IsUsedForRegistration = true`. Panel "Menunggu dari Kiosk" milik `FE-LAB-14` menyaring dengan
`onlyUsableForRegistration=true`, yang berarti `IsUsedForRegistration == false`. Sesi bertujuan
Laboratorium dari alur Pasien Baru karena itu **hilang pada detik yang sama ia dibuat**.

Kesimpulan yang ditarik dari ketiganya, dan yang kemudian dikonfirmasi pemilik modul: sesi
bertujuan Laboratorium hanya sampai ke petugas bila alur kiosknya **berhenti sebelum kunjungan
poliklinik terbentuk**.

### 1.3 Temuan keempat, ditemukan sebelum satu baris pun dijalankan

`Program.cs` memanggil `builder.Services.AddControllers();` **tanpa** satu pun
`JsonStringEnumConverter`, dan pencarian di seluruh source backend menghasilkan nol kemunculan.
Artinya enum pada body JSON hanya terbaca sebagai **angka**. Mengirim `"Laboratory"` — bentuk
yang dipakai `FE-LAB-14` pada penyaring `targetService` — akan dijawab `400` saat deserialisasi.
Bedanya: milik `FE-LAB-14` adalah query string, dan pengikatan query memang menerima nama enum.

Nilainya karena itu dikirim sebagai `2`, dan alasannya ditulis pada berkas konstanta supaya tidak
"dirapikan" menjadi string oleh pembaca berikutnya.

### 1.4 Tiga keputusan yang ditanyakan, bukan dikarang

Ketiga hal berikut menentukan bentuk layar dan tidak ada pada roadmap. Ketiganya ditanyakan
kepada pemilik modul sebelum implementasi dimulai, dan jawabannya dipakai apa adanya:

| Pertanyaan | Jawaban pemilik modul |
| --- | --- |
| Pilihan Laboratorium dipasang pada alur kiosk yang mana | **Pasien Lama saja.** Alur Pasien Baru menyusul sebagai slice tersendiri |
| Sesudah memilih Laboratorium, alur berhenti di mana | **Berhenti sesudah identitas terbaca**, lalu layar menutup dengan pesan agar pasien menuju loket Laboratorium |
| Cabang Poliklinik menuliskan tujuan layanannya atau tidak | **Tidak menuliskan apa pun.** Muatannya tetap persis seperti sebelum task ini ada |

---

## 2. Proses bisnis dari sisi pengguna

**Penggunanya pasien, tanpa pendamping.** Layar dibuka dari menu utama kiosk, kartu
`Pendaftaran Pasien Lama`.

### 2.1 Jalur Laboratorium — membawa surat dokter

1. **Tujuan Layanan.** Dua kartu sentuh besar: `Poliklinik` dan `Laboratorium`. Pasien menekan
   `Laboratorium`.
2. **Pertanyaan surat dokter.** Layar berganti isi — bukan berpindah langkah — menjadi
   *"Apakah Anda Membawa Surat dari Dokter?"* dengan dua kartu: `Ya, Saya Membawa` dan
   `Tidak, Periksa Sendiri`. Bila pasien keliru memilih layanan, tombol
   `Ganti Pilihan Layanan` di bawah mengembalikannya ke langkah 1.
3. **Identifikasi.** Pasien memindai kartu pasiennya atau mengetik No. RM, NIK, nama lengkap,
   atau nomor HP. Ketika kartunya dipindai, sesi kiosk terbentuk **membawa tujuan layanan dan
   jawaban surat dokternya sekaligus**.
4. **Review Data.** Data pasien ditampilkan. Tombolnya berbunyi
   `Data Benar, Lanjut ke Laboratorium` — bukan `Data Benar, Lanjut` seperti jalur poliklinik.
5. **Selesai.** Layar penutup menampilkan nama pasien, No. Rekam Medis, nomor pendaftaran,
   jalur yang dipilih, dan langkah berikutnya. Kiosk kembali ke menu utama otomatis dalam
   15 detik, atau lebih cepat bila tombol `Selesai` ditekan.

**Yang dilihat petugas laboratorium sesudahnya:** pasien tersebut muncul pada panel
"Menunggu dari Kiosk" di layar pendaftaran laboratorium (`FE-LAB-14`).

### 2.2 Jalur Laboratorium — memeriksakan diri sendiri

Sama persis, dengan dua perbedaan. Pada langkah 2 pasien menekan `Tidak, Periksa Sendiri`, dan
layar penutup menampilkan *"Petugas Laboratorium akan membantu menentukan pemeriksaan yang Anda
butuhkan."* alih-alih pengingat menyerahkan surat.

### 2.3 Jalur Laboratorium lewat pencarian ketikan

Pasien yang **mengetik** No. RM-nya, bukan memindai kartu, tidak pernah membentuk sesi kiosk —
dulu maupun sekarang. Untuk jalur Laboratorium sesinya dibentuk **pada saat pasien menekan
`Data Benar, Lanjut ke Laboratorium`**, yaitu sesudah ia melihat datanya sendiri dan
membenarkannya. Ini disengaja: sesi yang dibentuk lebih awal berarti kartu yang salah baca
langsung memasukkan orang lain ke antrean laboratorium.

### 2.4 Jalur Poliklinik — tidak ada satu pun perilaku yang berubah

Pasien menekan `Poliklinik` pada langkah 1, lalu alurnya berjalan persis seperti sebelum task
ini ada: `Identifikasi` → `Review Data` → `Jenis Kunjungan` → `Pembayaran` →
`Layanan & Dokter` → `Konfirmasi` → `Cetak Antrean`. Muatan yang dikirim ke sesi kiosk **nol
ruas tambahan**; bukan berisi nilai kosong, melainkan memang tidak memuat ruasnya sama sekali.

### 2.5 Jalur tidak normal

| Keadaan | Yang terjadi |
| --- | --- |
| Pasien salah memilih layanan | Tombol `Ganti Pilihan Layanan` tersedia pada langkah pertanyaan surat dokter **dan** pada langkah Identifikasi. Sampai identitasnya dibaca, nol sesi kiosk terbentuk |
| Kartu pasien tidak terbaca | Pop-up `Kartu pasien belum terbaca` — perilaku yang sudah ada, tidak disentuh |
| Data pasien tidak ditemukan | Pop-up `Data pasien tidak ditemukan` — perilaku yang sudah ada, tidak disentuh |
| Pencatatan sesi Laboratorium gagal | Pesan `Pendaftaran Laboratorium belum tersimpan. Silakan coba lagi atau hubungi petugas terdekat.` Pasien tetap di Review Data dan dapat mencoba lagi. Pesan teknis dari backend **sengaja tidak diteruskan** ke layar |
| Pasien tidak punya NIK tersimpan | Sesinya tetap tercatat lewat namanya, tetapi tidak dapat dicocokkan otomatis ke rekam medis. Layar penutup menambahkan baris `Perlu Diperhatikan` yang meminta pasien menyebutkan nama dan No. Rekam Medis kepada petugas |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Blueprint dan roadmap:** `roadmap/frontend-roadmap.md` bagian 6c, `roadmap/backend-roadmap.md`
`BE-EXT-04`/`BE-EXT-04b`/`BE-EXT-05`, `roadmap/traceability.md`, `00-interview-decisions.md`
BR-46 beserta `LAB-DEC-051`/`LAB-DEC-052` dan `AC-93`, `testing/acceptance-test-matrix.md`,
`approval-requests/2026-09-15-persetujuan-bagian-lab-di-kiosk.md`.

**Backend, dibaca saja:** `KioskScanSessionController.cs`, `KioskScanSessionDtos.cs`,
`KioskServiceTarget.cs`, `PatientEncounterController.cs`, `Program.cs`.

**Frontend:** seluruh berkas alur `kiosk/registration/old-patient` dan `new-patient`,
`use-kiosk-old-patient-registration.jsx`, `kiosk-old-patient-registration.service.js`,
`kiosk-new-patient-registration.service.js`, `kiosk-new-patient-submit.helpers.jsx`,
`kiosk-home-view.jsx`, `lab-kiosk-session.service.js` milik `FE-LAB-14`,
`kiosk-old-patient-view.module.css`, dan `tests/e2e/lab-patient-search-paging.spec.mjs` sebagai
rujukan pola verifikasi layar.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/kiosk/registration/kiosk-old-patient-service-target.constants.js` | **Baru.** Nilai enum tujuan layanan beserta alasan mengapa dikirim sebagai angka, kedua pilihan layanan, kedua jawaban surat dokter, dan seluruh salinan teks layarnya |
| `src/lib/hooks/kiosk/registration/kiosk-service-target-rules.js` | **Baru.** Aturan murni: apakah cabangnya Laboratorium, apakah pertanyaan surat dokter sudah terjawab, pembentukan ruas tambahan muatan, dan pembentukan muatan sesi untuk pasien yang identitasnya sudah dikenali layar |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-service-target.jsx` | **Baru.** Langkah Tujuan Layanan beserta panel pertanyaan surat dokternya |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-lab-handoff.jsx` | **Baru.** Layar penutup jalur Laboratorium |
| `src/lib/hooks/kiosk/registration/use-kiosk-old-patient-registration.jsx` | Langkah `serviceTarget` dan `labHandoff` ditambahkan, daftar langkah dibuat bergantung cabang, keadaan tujuan layanan dan jawaban surat dokter disimpan, `handleReviewContinue` mencabangkan ke pencatatan sesi Laboratorium |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-view.jsx` | Kedua langkah baru dirender, judul halamannya ditambahkan, footer langkah Tujuan Layanan dan footer `Ganti Pilihan Layanan` pada langkah Identifikasi, label tombol Review Data dibuat sadar cabang |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-find.jsx` | Satu properti `scanSessionExtraPayload` yang disebarkan ke muatan pembuatan sesi. Objek kosong pada cabang poliklinik |
| `tests/unit/kiosk-service-target-rules.test.mjs` | **Baru.** 11 uji unit |

**Nol berkas gaya diubah.** Tidak satu baris pun ditambahkan ke
`kiosk-old-patient-view.module.css`.

### 3.3 Kepatuhan arsitektur frontend

**Alur dependensinya mengikuti pola yang sudah ada:** route tipis → view → langkah → hook →
service → `fetch` bersama milik alur kiosk. Nol Axios instance baru, nol slice Redux baru, nol
abstraksi generik, nol Pages Router.

**Kelas visual dipakai ulang, bukan disalin.** Langkah Tujuan Layanan memakai kelas
`oldPatientTypeChoice*` milik langkah Jenis Pasien, dan layar penutup memakai kelas
`ticketModern*` milik langkah Cetak Antrean. Keduanya memang berbentuk sama persis — dua kartu
sentuh besar pada yang pertama, dan kartu "selesai" berisi ringkasan beserta hitungan mundur pada
yang kedua. Menyalin sekitar 250 baris CSS beserta seluruh media query-nya hanya akan melahirkan
dua salinan yang kelak menyimpang diam-diam.

**Satu penyimpangan terhadap checklist konsistensi UI, dan alasannya.**
`ui-consistency-checklist.md` bagian C melarang `<button>` mentah untuk aksi baru dan menuntut
`BaseButton`. Layar kiosk **seluruhnya** memakai `<button>` mentah beserta kelas modulnya sendiri
— sasaran sentuh besar untuk pasien berdiri tanpa pendamping, bukan kontrol padat untuk petugas.
Berkas baru mengikuti tetangganya, sebagaimana diperintahkan `AGENTS.md` dan wewenang UI task ini
yang berbunyi "mengikuti pola layar kiosk yang sudah berjalan". Menerapkan `BaseButton` di sini
berarti memasukkan bahasa desain layar administrasi ke layar pasien.

**Satu peringatan lint sengaja tidak diwariskan.** `kiosk-old-patient-step-ticket.jsx` membawa
peringatan `react-hooks/set-state-in-effect` karena menyetel ulang hitungan mundurnya di dalam
efek. Layar penutup yang baru menyalin bentuk visualnya tetapi **tidak** menyalin baris itu —
nilainya sudah penuh dari `useState`. Peringatan pada berkas lama tidak disentuh, sesuai aturan
cakupan perubahan.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Teks `Memuat halaman pendaftaran pasien lama...` sebelum hak akses kiosk selesai diperiksa — perilaku yang sudah ada. Saat sesi Laboratorium sedang dicatat, tombolnya berubah menjadi `Menyimpan...` dan kedua tombol footer dinonaktifkan |
| Kosong | `NOT APPLICABLE` — layar ini tidak menampilkan daftar. Keadaan "pasien tidak ditemukan" ditangani pop-up milik langkah Identifikasi yang sudah ada |
| Gagal | Pencatatan sesi gagal: `Pendaftaran Laboratorium belum tersimpan. Silakan coba lagi atau hubungi petugas terdekat.` Pasien tetap di Review Data dan tombolnya kembali aktif. Gagal memindai atau mencari: pop-up yang sudah ada |
| Tanpa hak akses | Teks `Mengalihkan ke halaman yang sesuai...` lalu dialihkan — perilaku yang sudah ada. Akun bukan kiosk dialihkan ke `/administrator`, yang belum masuk dialihkan ke `/login` |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Registration Management / Kiosk Scan Session

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/registration-management/kiosk-scan-sessions/scan-result` | Mencatat sesi kiosk beserta tujuan layanan dan jalur permintaan dokternya | `KioskScanSession : Create`, di bawah `KioskReadPolicy` |

**Muatan tambahan yang dikirim task ini**, dan hanya pada cabang Laboratorium:

| Ruas | Nilai | Catatan |
| --- | --- | --- |
| `targetService` | `2` | `KioskServiceTarget.Laboratory`, dikirim sebagai **angka** — lihat bagian 1.3 |
| `hasPhysicianRequest` | `true` atau `false` | `true` membawa surat dokter, `false` memeriksakan diri sendiri. `null` berarti belum ditanyakan, dan itulah yang tetap berlaku pada cabang poliklinik |

Pada jalur pencarian ketikan, muatannya juga membawa `identityNumber`, `identityType`,
`fullName`, dan `isManualInput: true`. `identityNumber` dikirim karena endpoint ini **tidak**
menerima `patientId`; ia mencocokkan pasiennya sendiri lewat `FindPatientAsync`, yang membaca
NIK, nomor kartu asuransi, dan nomor member — **bukan** nomor rekam medis.

#### Health Services / Patient Management / Master Data / Patient

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/patient-management/master-data/patients/options` | Pencarian pasien pada langkah Identifikasi — **sudah dipakai sebelumnya**, tidak disentuh | `Patient : Read` |
| `GET` | `/v1/health-services/patient-management/master-data/patients/{id}` | Mengambil rincian pasien terpilih — **sudah dipakai sebelumnya**, tidak disentuh | `Patient : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` atas kedelapan berkas task ini | **Nol keluaran** — nol error, nol peringatan | `PASS` | Keluaran perintah |
| `npm run lint:errors` seluruh repository | Nol keluaran | `PASS` | Keluaran perintah |
| Uji unit baru | **11 dari 11 lolos** | `PASS` | `tests/unit/kiosk-service-target-rules.test.mjs` |
| Seluruh uji unit repository | **963 dari 963 lolos** | `PASS` | 952 uji lama lolos tanpa satu pun disentuh, ditambah 11 uji baru |
| `npm run build` — build produksi | `Compiled successfully`, `Standalone runtime siap dijalankan` | `PASS` | Keluaran perintah |
| **Layar** — pilihan Laboratorium tersedia | Lolos | `PASS` | 6.2 `S1` |
| **Layar** — muatan cabang poliklinik tidak berubah sama sekali | Lolos | `PASS` | 6.2 `S2` |
| **Layar** — jalur surat dokter tercatat pada sesi | Lolos | `PASS` | 6.2 `S3` |
| **Layar** — alur Laboratorium tidak membentuk kunjungan poliklinik | Lolos | `PASS` | 6.2 `S3` |
| **Layar** — jalur pencarian ketikan mencatat sesi saat pasien mengonfirmasi | Lolos | `PASS` | 6.2 `S4` |
| **Layar** — pilihan layanan dapat diganti sebelum identitas dibaca | Lolos | `PASS` | 6.2 `S5` |

**Uji manual: `PASS`.**

**Tidak dijalankan:** `npm run test:e2e` dan `npm run test:uat` — tidak diminta task ini, dan
rangkaian e2e repository menuntut lingkungan tersendiri. Verifikasi layar dijalankan terpisah
lewat harness sementara, dijelaskan pada 6.3.

### 6.1 Satu temuan tentang perkakas, dicatat apa adanya

`npm run test:unit` **tidak dapat dijalankan apa adanya pada mesin ini**, dan penyebabnya bukan
perubahan task ini. Scriptnya berbunyi `--test "tests/unit/**/*.test.mjs"`, sedangkan Node yang
terpasang adalah `v20.20.2`; dukungan pola glob pada `--test` baru ada sejak Node 21. Keluarannya:
`Could not find '...tests\unit\**\*.test.mjs'`.

Angka 963 di atas diperoleh dengan menjalankan bentuk direktori yang setara —
`node --import ./tests/helpers/register.mjs --test tests/unit/` — yaitu bentuk yang juga tertulis
pada `rules/frontend/test-policy.md`. **`package.json` tidak disentuh**; menyesuaikannya adalah
perubahan dependency/perkakas yang berdiri sendiri. Diklasifikasikan
`EXISTING / ENVIRONMENT ISSUE`.

### 6.2 Verifikasi layar terhadap aplikasi yang benar-benar berjalan

Dijalankan terhadap build produksi standalone pada `127.0.0.1:3710`, jawaban server dipalsukan
lewat route Playwright, dan **setiap permintaan yang tiba dicatat** supaya pernyataan "nol
permintaan" dapat dibuktikan, bukan diasumsikan. **5 dari 5 lolos.**

| Butir | Bukti |
| --- | --- |
| `S1` — layar membuka pada langkah Tujuan Layanan | Judul `Pilih Tujuan Layanan` tampil beserta kedua kartu `Poliklinik` dan `Laboratorium` |
| `S1` — kata "disiplin" nol kemunculan | Seluruh teks halaman dibaca lalu dicocokkan; nol kemunculan |
| `S2` — **muatan cabang poliklinik persis seperti sebelumnya** | Kunci muatan diperiksa satu per satu: **nol `targetService`, nol `hasPhysicianRequest`**, dan `isManualInput` tetap `false` |
| `S2` — alur poliklinik berlanjut seperti biasa | Sesudah kartu dipindai: `Review Data Pasien` → `Data Benar, Lanjut` → `Pilih Jenis Pasien` |
| `S3` — pertanyaan surat dokter muncul sesudah Laboratorium dipilih | Judul `Apakah Anda Membawa Surat dari Dokter?` tampil |
| `S3` — **sesi membawa kedua ruas** | Muatan `POST scan-result` berisi `targetService: 2` dan `hasPhysicianRequest: true` |
| `S3` — **nol kunjungan poliklinik terbentuk** | Sesudah layar penutup tampil dan ditunggu, permintaan `POST` ke `/patient-encounters` tercatat **nol** |
| `S3` — nol sesi kedua | Sesi kiosk tetap **tepat 1** sesudah pasien menekan lanjut dari Review Data |
| `S4` — pencarian ketikan tidak membentuk sesi lebih awal | Sesudah pasien ditemukan dan Review Data tampil, sesi kiosk tercatat **nol** |
| `S4` — sesi dicatat saat pasien mengonfirmasi | Tepat 1 sesi, berisi `targetService: 2`, `hasPhysicianRequest: false`, `identityNumber` sama dengan NIK pasien, dan `isManualInput: true` |
| `S5` — pilihan layanan dapat diganti dua kali | Dari panel pertanyaan surat dokter dan dari langkah Identifikasi, keduanya kembali ke `Pilih Tujuan Layanan`; sampai titik itu sesi kiosk tercatat **nol** |

### 6.3 Alat verifikasinya

Konfigurasi dan spesifikasi Playwright **sementara**, dihapus sesudah selesai. **Nol berkas uji
layar tertinggal di repository**, dan `test-results/` ikut dibersihkan — dibuktikan lewat
`git status --short` pada bagian 8.

---

## 7. Acceptance criteria dan Definition of Done

### 7.1 Definition of Done

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Pilihan Laboratorium tersedia | **Terpenuhi** | Kartu `Laboratorium` pada langkah pertama alur Pasien Lama — 6.2 `S1` |
| Jalur permintaan dokter tercatat pada sesi | **Terpenuhi** | `hasPhysicianRequest` terkirim `true` pada 6.2 `S3` dan `false` pada 6.2 `S4`, bersama `targetService: 2` |
| **Nol perilaku alur kiosk lama yang berubah** | **Terpenuhi** | Muatan cabang poliklinik nol ruas tambahan, dan langkah sesudahnya berjalan persis seperti sebelumnya — 6.2 `S2`. Satu perbedaan yang memang tak terhindarkan disebut apa adanya di bawah tabel ini |
| Uji lama lulus tanpa diubah | **Terpenuhi** | 952 uji lama lolos tanpa satu berkas pun disentuh; totalnya kini 963 |

**Satu perbedaan yang memang tak terhindarkan, dan disebut apa adanya.** Pasien alur Pasien Lama
kini melihat **satu layar tambahan di depan**, yaitu pilihan tujuan layanan, sebelum sampai ke
Identifikasi. Pilihan itu tidak dapat diletakkan di tempat lain — lihat bagian 1.2 — dan baris
Verifikasi pada kartu task memang berbunyi "alur lama yang **tidak memilih Laboratorium**
berperilaku persis seperti sebelumnya", yang mengandaikan pilihannya ada. Sesudah `Poliklinik`
ditekan, nol perilaku berikutnya yang berubah.

### 7.2 Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-93` — sesi kiosk yang bertambah ruas tujuan layanan dan jalur permintaan **tidak mengubah satu pun perilaku sesi kiosk yang sudah ada**; 16 sesi yang sudah tersimpan tetap terbaca | **Terpenuhi dari sisi frontend** | Muatan cabang poliklinik tidak bertambah satu ruas pun — 6.2 `S2`, dan uji unit yang menegaskan objeknya kosong, bukan berisi `null`. Sisi datanya sudah terbukti terpisah lewat `T-93a` pada [`BE-EXT-04.md`](../backend/BE-EXT-04.md) |

Baris uji `T-93b` — *"alur kiosk yang tidak menyebut tujuan layanan berperilaku persis seperti
sebelumnya — nol penolakan baru"* — kini punya bukti dari sisi layar, bukan hanya dari sisi
backend.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol peringatan baru pada kedelapan berkas task ini. `kiosk-old-patient-step-ticket.jsx` membawa satu peringatan `react-hooks/set-state-in-effect` yang **sudah ada sebelumnya** dan tidak disentuh — `EXISTING WARNING` |
| Masalah yang diketahui | **Pertama:** pasien yang **tidak punya NIK tersimpan** menghasilkan sesi yang tidak dapat dicocokkan otomatis ke rekam medisnya, karena `FindPatientAsync` tidak membaca nomor rekam medis. Sesinya tetap tercatat lewat nama, dan layar penutup memberi tahu pasien agar menyebutkan datanya kepada petugas. **Kedua:** `npm run test:unit` tidak dapat dijalankan apa adanya pada Node 20 — lihat 6.1 |
| Dependency backend | `BE-EXT-04` ✅ dan `BE-EXT-04b` ✅ keduanya selesai; nol penahan. **`BE-EXT-05` masih ⛔**, dan akibatnya di layar disebut terang: pasien Laboratorium **belum memperoleh kunjungan maupun nomor antrean** di kiosk. Layar berhenti pada pesan agar menuju loket Laboratorium. Ini bukan kekurangan yang didiamkan, melainkan batas yang ditetapkan `AC-45` beserta `LAB-OPEN-025` dan `LAB-OPEN-026` |
| Perubahan sampingan | `NONE`. Delapan belas berkas lain yang tampak berubah pada `git status` adalah milik `FE-LAB-14`, `FE-LAB-15`, dan `FE-LAB-16` yang belum di-commit; nol di antaranya disentuh task ini |
| Interupsi | `NONE` |
| Status Git | Dilampirkan pada 8.1 |
| Langkah berikutnya | **Gelombang `MVP-5b` tuntas.** Yang tersisa pada `MVP-5c` tinggal `FE-LAB-17`, yang masih ⛔ dan menunggu tiga keputusan pemilik modul. Bila alur **Pasien Baru** juga hendak membawa pilihan Laboratorium, itu perlu dibuka sebagai task tersendiri — dan perlu diketahui lebih dulu bahwa sesinya akan langsung tertandai terpakai kecuali cabangnya ikut berhenti sebelum kunjungan dibentuk |

### 8.1 Status Git

Branch `YogaV2`, upstream `origin/YogaV2`. **Nol `git add`, nol commit, nol push.**

Berkas milik `FE-LAB-13`:

```text
 M src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-find.jsx
 M src/components/view/kiosk/registration/old-patient/kiosk-old-patient-view.jsx
 M src/lib/hooks/kiosk/registration/use-kiosk-old-patient-registration.jsx
?? src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-lab-handoff.jsx
?? src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-service-target.jsx
?? src/lib/constants/kiosk/registration/kiosk-old-patient-service-target.constants.js
?? src/lib/hooks/kiosk/registration/kiosk-service-target-rules.js
?? tests/unit/kiosk-service-target-rules.test.mjs
```

Berkas lain pada working tree berasal dari `FE-LAB-14`, `FE-LAB-15`, dan `FE-LAB-16` yang belum
di-commit, dan tidak disentuh task ini.
