# Laporan Perubahan Frontend — `FE-LAB-05`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-05` |
| Judul | Layar pendaftaran pasien laboratorium |
| Slice | `S13a`, `S13b` — pendaftaran datang langsung dan rujukan luar (`roadmap/frontend-roadmap.md` bagian 4, gelombang `MVP-1`) |
| Roadmap | `docs/module-blueprints/laboratorium/roadmap/frontend-roadmap.md` bagian 4 |
| Trace | `FR-08.1` .. `FR-08.5`; `LAB-DEC-032`, `LAB-DEC-035`; `VAL-40` .. `VAL-45`; `AC-44`, `AC-45`, `AC-46`, `AC-50` |
| Contract version | `LAB-API-v1` r3 — `approved`, dikunci 2026-09-02. Grup Lab Patient Registration, **tersedia** sejak `BE-LAB-08` |
| Wewenang UI | Cakupan layar dikunci roadmap: pencarian pasien, formulir datang langsung, formulir rujukan luar dengan **pemilihan** perujuk dari daftar. Bentuk visual `DEV_DISCRETION`; **keberadaan pemilihan dari daftar tidak** (`LAB-DEC-035`, `AC-50`) |
| Dependency | `FE-LAB-01` **selesai**; `BE-LAB-08` **selesai** 2026-09-07; `BE-EXT-02` **selesai** untuk entity — **endpoint daftarnya belum ada dan dibangun pada sesi ini**, lihat bagian 1 |
| Klasifikasi | `HEAVY` — skor 12: repository 2, berkas diperiksa 2, berkas diubah 2, logika bisnis 2, kontrak API 1, database 0, keamanan 1, UI/workflow 2 |
| Task mode | `CROSS-REPO` — keduanya target tulis atas instruksi eksplisit pemilik modul pada sesi ini |
| Target tulis | `QuilvianSystemFrontendDev` — lapis modul `laboratory-management`, registry select Health Services, store Redux, dan satu berkas uji. `NewQuilvianSystemBackend` — dua endpoint baca data induk perujuk beserta service, DTO, dan ujinya; laporan ini; serta tautan buktinya pada `roadmap/` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit frontend saat dikerjakan | `72f050b50`, branch `YogaV2` |
| Commit backend saat dikerjakan | `302ff7b`, branch `yoga` |
| Tanggal | 2026-09-07 |
| Status | **Selesai.** Keempat butir DoD terpenuhi. Satu batas verifikasi dicatat apa adanya pada bagian 6 |

---

## 1. Keadaan yang ditemukan di awal

### 1.1 Penahan lama sudah dicabut

Task ini ditandai **`BLOCKED`** sejak 2026-09-04 karena `BE-LAB-08` belum ada sama sekali.
Penahan itu dicabut pada 2026-09-07: ketiga endpoint tersedia, dan buktinya diperiksa langsung
pada source backend, bukan pada dokumen.

| Yang dicari | Hasil |
| --- | --- |
| `LabPatientRegistrationController` | **ada** |
| Route `lab-patient-registrations` | **ada**, tiga endpoint |
| DTO `RegisterLabWalkInRequest`, `RegisterLabExternalReferralRequest` | **ada** |
| Laporan `BE-LAB-08.md` | **ada**, status `SELESAI` |

### 1.2 Penahan baru ditemukan, dan ditutup pada sesi yang sama

Formulir rujukan luar wajib menawarkan **pemilihan** instansi dan dokter perujuk dari daftar.
Pemeriksaan menemukan daftar itu tidak punya sumber:

> `MstReferralInstitution` dan `MstReferralDoctor` ada beserta `DbSet`-nya, tetapi **tidak
> punya satu pun endpoint**. Laporan `BE-EXT-02` menyatakannya sendiri:
> *"`QBE-API-001`, `QBE-PERM-001`, `QBE-SVC-001` — tidak ada endpoint, service, maupun nomor
> bisnis pada rilis ini"*.

Yang membuat ini bukan sekadar kekurangan: butir **Verifikasi `BE-EXT-02`** berbunyi *"Kedua
data induk **dapat dipilih dari daftar**"*. Butir itu **tidak pernah terpenuhi**, dan baru
terlihat sekarang karena konsumennya baru ada.

Tiga pilihan diajukan kepada pemilik modul, dan yang dipilih adalah **membangun dua endpoint
bacanya lebih dulu**. Tanpa itu, satu-satunya cara mengisi perujuk adalah mengetiknya — persis
yang dilarang `LAB-DEC-035` dan `AC-50`.

---

## 2. Proses bisnis

### 2.1 Alur normal — pasien datang langsung (`AC-44`)

| Langkah | Yang dilakukan petugas | Yang terjadi |
| ---: | --- | --- |
| 1 | Membuka layar pendaftaran | Layar **langsung membuat kunci idempotensi** |
| 2 | Mengetik nomor rekam medis atau nama | Daftar pasien tersaring, tertunda 350 ms supaya tiap huruf tidak menjadi satu permintaan |
| 3 | Mengklik satu baris pasien | Baris bertanda **Dipilih**, dan keterangan pasien terpilih muncul di atas |
| 4 | Menekan **Daftar Datang Langsung** | Formulir terbuka dengan pasien sudah terisi |
| 5 | Memilih unit layanan dan cara bayar, lalu **Daftarkan** | Isian diteruskan ke Registrasi |
| 6 | — | Panel hasil menampilkan nomor kunjungan dan identitas pasien |
| 7 | Menekan **Buat Pesanan Laboratorium** | Formulir pesanan terbuka dengan kunjungan **sudah terisi** |

### 2.2 Alur normal — rujukan luar (`AC-46`, `AC-50`)

Sama seperti di atas, dengan satu panel tambahan **Asal Rujukan** berisi tiga isian wajib:

| Isian | Bentuknya | Kenapa begitu |
| --- | --- | --- |
| Instansi perujuk | **Pilihan dari daftar** | Sebagai teks bebas, "Klinik Sehat Sentosa", "Kl. Sehat Sentosa", dan "sehat sentosa" terhitung tiga instansi berbeda |
| Dokter perujuk | **Pilihan dari daftar**, tersaring menurut instansinya | Backend menolak dokter yang tidak berpraktik pada instansi itu; daftar tanpa penyaring hanya menawarkan pilihan yang pasti ditolak |
| Nomor surat rujukan | Isian teks | Nomor surat memang teks, dan tidak ada daftar yang dapat menggantikannya |

Kotak dokter **nonaktif sejak awal** selama instansi belum dipilih, disertai keterangannya —
bukan dibiarkan aktif lalu gagal saat disimpan. Mengganti instansi **mengosongkan** dokter yang
sudah dipilih, karena dokter itu berpraktik pada instansi sebelumnya.

### 2.3 Menekan Simpan dua kali (`VAL-45`)

> Petugas menekan **Daftarkan**. Jaringan lambat. Ia menekan lagi.

Kunci idempotensi dibuat **saat formulir dibuka**, bukan saat tombol ditekan, lalu dipakai
ulang oleh setiap penekanan pada percobaan yang sama. Backend mengembalikan kunjungan yang sama
beserta penanda `isReplay`, dan layar memperlakukannya sebagai **keberhasilan**:

> *"Pendaftaran ini sudah tercatat sebelumnya. Kunjungan yang sama dikembalikan, bukan
> kunjungan baru."*

**Kenapa waktu pembuatan kuncinya menentukan.** Bila kunci dibuat pada penekanan tombol,
penekanan kedua membawa kunci berbeda, backend melihatnya sebagai pendaftaran baru, dan lahirlah
kunjungan kedua — persis yang hendak dicegah. Kunci baru hanya dibuat ketika petugas menekan
**Daftarkan Pasien Lain**.

### 2.4 Jalur tidak normal

| Keadaan | Yang terlihat di layar |
| --- | --- |
| Isian wajib belum lengkap | Galat melekat pada ruasnya, ditambah satu toast; **tidak ada permintaan dikirim** |
| Instansi diketik, bukan dipilih | Tidak mungkin terjadi — **tidak ada kotak isian namanya** |
| Registrasi menolak `422`, `403`, atau `503` | Pesan dari server ditampilkan **apa adanya**, tanpa diganti kalimat buatan layar (`VAL-40`) |
| Cara bayar Asuransi tanpa kartu | Ditahan di layar sebelum dikirim |
| Penjamin perusahaan | Tidak ditawarkan sama sekali — backend menolaknya, jadi menampilkannya hanya membuat petugas mencoba lalu gagal |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk menetapkan |
| --- | --- |
| `roadmap/frontend-roadmap.md` bagian 2 dan 4 | Cakupan, tujuh lapis wajib, invariant, dan DoD |
| `contracts/api-contract.md`, `contracts/validation-matrix.md` | Bentuk permintaan, jawaban, dan bunyi pesan penolakan |
| `task/report/backend/BE-LAB-08.md` | Kewajiban `idempotencyKey` dan arti `isReplay` |
| `Areas/.../LabPatientRegistrationController.cs` beserta DTO-nya | Kontrak sebenarnya pada source, bukan tebakan dari dokumen |
| `use-lab-order-form.jsx`, `lab-order-form-view.jsx`, `lab-order-slice.jsx` | Pola formulir, hook, dan potongan Redux terdekat |
| `lab-order-urgency-rules.js` beserta ujinya | Pola aturan murni yang dapat diuji tanpa merender |
| `health-service-select-resources.js`, `use-select-resource.jsx` | Pola select berantai dan penyaringnya |
| `base-component-catalog.md` | Memastikan komponen dasarnya sudah ada, bukan dibuat baru |

### 3.2 Berkas yang berubah — `QuilvianSystemFrontendDev`

| Berkas | Perubahan |
| --- | --- |
| `.../constants/.../lab-patient-registration-constants.jsx` | **Baru.** Route, pilihan cara bayar, kolom tabel, dan seluruh salinan teks |
| `.../services/.../lab-patient-registration.service.js` | **Baru.** Tiga pemanggilan endpoint |
| `.../state/slice/.../lab-patient-registration-slice.jsx` | **Baru.** Pencarian pasien dan dua jalur pendaftaran, beserta penyimpanan kode status galat |
| `.../hooks/.../lab-registration-rules.js` | **Baru.** Aturan murni: kunci idempotensi, validasi, dan penyusunan muatan |
| `.../hooks/.../use-lab-patient-search.jsx` | **Baru.** Controller layar pencarian |
| `.../hooks/.../use-lab-patient-registration-form.jsx` | **Baru.** Controller kedua formulir |
| `.../view/.../lab-patient-registrations/lab-patient-search-view.jsx` | **Baru.** Layar pencarian |
| `.../view/.../lab-patient-search-table-columns.jsx` | **Baru.** Kolom tabel beserta penanda terpilih |
| `.../view/.../form/lab-patient-registration-form-view.jsx` | **Baru.** Kedua formulir dan panel hasil |
| `src/app/.../lab-patient-registrations/{,walk-in,external-referral}/page.jsx` | **Baru.** Tiga route |
| `src/style/.../lab-patient-registrations/lab-patient-registration.module.css` | **Baru.** Dua kelas tambahan; sisanya memakai ulang kelas layar pesanan |
| `.../hooks/select/health-service/health-service-select-resources.js` | Tiga resource baru: `patientInsurances`, `referralInstitutions`, `referralDoctors` |
| `src/lib/state/store.jsx` | Satu potongan Redux didaftarkan |
| `.../constants/.../laboratory-constants.jsx` | Alamat grup `lab-patient-registrations` dan route-nya |
| `.../hooks/.../use-lab-order-form.jsx` | Membaca `encounterId` dan `encounterNumber` dari query |
| `.../view/.../lab-orders/form/lab-order-form-view.jsx` | Meneruskan label kunjungan yang terbawa |
| `src/app/.../lab-orders/create/page.jsx` | Dibungkus `Suspense`, syarat `useSearchParams` |
| `tests/unit/lab-registration-rules.test.mjs` | **Baru.** Tiga belas uji |

### 3.3 Berkas yang berubah — `NewQuilvianSystemBackend`

Dikerjakan atas instruksi eksplisit pemilik modul untuk mencabut penahan pada bagian 1.2.

| Berkas | Perubahan |
| --- | --- |
| `.../MasterData/DTOs/ReferralMasterDataDtos.cs` | **Baru.** Dua penyaring dan dua bentuk jawaban |
| `.../MasterData/Services/ReferralMasterDataService.cs` | **Baru.** Dua jalur baca; **nol** jalur ubah |
| `.../MasterData/Controllers/ReferralInstitutionController.cs` | **Baru.** Satu endpoint `GET /options` |
| `.../MasterData/Controllers/ReferralDoctorController.cs` | **Baru.** Satu endpoint `GET /options`, dapat disaring menurut instansi |
| `Program.cs` | Satu baris pendaftaran service |
| `Tests/.../ReferralMasterDataOptionsTests.cs` | **Baru.** Delapan uji |

### 3.4 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API Laboratorium | **Tidak ada perubahan.** Layar mengonsumsi `LAB-API-v1` r3 apa adanya |
| Kontrak API Master Data | **Aditif.** Dua endpoint baca baru, di luar `LAB-API-v1` karena data induknya milik Master Data |
| Database | **Tidak ada dampak sama sekali.** Nol tabel, nol kolom, nol migration |
| Keamanan/Auth | Endpoint baru memakai `ReferralInstitution : Read` dan `ReferralDoctor : Read`, terdaftar otomatis lewat `[AccessController]` dan `[AccessAction]`. Ketiga layar memakai `AccessDeniedGate` yang sudah baku |

### 3.5 Keputusan dan selisih yang perlu diketahui

| No | Butir | Penjelasan |
| ---: | --- | --- |
| 1 | **Satu hook melayani kedua formulir** | Isian intinya sama persis; yang membedakan hanya tiga ruas rujukan. Dua hook terpisah akan menyalin aturan idempotensi dan penanganan penolakan dua kali, dan salinan itulah yang kelak bercabang. Modenya diteruskan sebagai satu parameter |
| 2 | **Aturan murni dipisah ke `lab-registration-rules.js`** | Mengikuti `lab-order-urgency-rules.js` yang juga tinggal di lapis hook. Modul Laboratorium sengaja tidak membuka lapis `utils` tersendiri, dan pemisahan ini membuat tiga belas uji dapat berjalan tanpa merender apa pun |
| 3 | **Kunci idempotensi dibuat saat formulir dibuka** | Ini keputusan paling menentukan pada task ini, dan paling mudah dilanggar tanpa disadari. Alasannya ada pada bagian 2.3 |
| 4 | **`isReplay` diperlakukan sebagai keberhasilan** | Menampilkannya sebagai kegagalan akan mendorong petugas menekan Simpan sekali lagi — dan itu justru memperbesar risiko yang hendak dicegah |
| 5 | **Panel hasil menggantikan formulir setelah berhasil** | Membiarkan formulir terbuka hanya mengundang penekanan Simpan berikutnya yang tidak lagi diperlukan |
| 6 | **Penjamin perusahaan tidak ditawarkan** | Backend menolaknya `422`; menampilkan pilihan yang pasti ditolak hanya membuang waktu petugas |
| 7 | **`useLabOrderForm` disentuh, walau miliknya `FE-LAB-06`** | Tambahannya aditif: membaca dua parameter query bila ada. Tanpa ini, butir DoD *"alur menyambung ke pembuatan pesanan"* hanya terpenuhi sebagai pindah halaman, dan petugas harus mencari ulang kunjungan yang baru saja ia buat sendiri |
| 8 | **Pencarian pasien memakai batas baris, bukan pagination bernomor** | Mengikuti bentuk endpointnya, yang menerima `limit` dan mengembalikan `List<T>`, bukan `PagedResult<T>` |
| 9 | **Dokter perujuk selalu tersaring menurut instansi** | Bukan sekadar kenyamanan: backend menolak dokter yang tidak berpraktik pada instansi yang dipilih |

---

## 4. Dokumentasi endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Patient Registration

| Method | Path | Dipakai layar | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/patient-search` | Layar pencarian pasien | `LabPatientRegistration : Read` |
| `POST` | `/walk-in` | Formulir datang langsung | `LabPatientRegistration : Create` |
| `POST` | `/external-referral` | Formulir rujukan luar | `LabPatientRegistration : Create` |

#### Health Services / Master Data / Referral Institution

| Method | Path | Dipakai layar | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/options` | Pilihan instansi perujuk | `ReferralInstitution : Read` |

#### Health Services / Master Data / Referral Doctor

| Method | Path | Dipakai layar | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/options` | Pilihan dokter perujuk, tersaring menurut instansi | `ReferralDoctor : Read` |

---

## 5. Verifikasi

| Perintah atau skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| `npm run lint:errors` | Bersih, tanpa keluaran | `PASS` |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | `pass 484, fail 0` | `PASS` |
| `tests/unit/lab-registration-rules.test.mjs` | `pass 13, fail 0` | `PASS` |
| `npm run build` | `Compiled successfully`; ketiga route baru terbentuk | `PASS` |
| Backend — `dotnet msbuild -t:Compile` | Nol error | `PASS` |
| Backend — `dotnet test --filter ReferralMasterData` | `Passed: 17, Failed: 0` | `PASS` |

`AUTOMATED TEST: npm run test:unit — BLOCKED (environment)`, dijalankan sebagai
`node --import ./tests/helpers/register.mjs --test tests/unit/` — **PASS**.

> **Kenapa perintah npm-nya diganti bentuk.** `package.json` memanggil runner dengan pola glob
> `"tests/unit/**/*.test.mjs"`. Node pada mesin ini `v20.20.2`, dan dukungan glob pada `--test`
> baru ada sejak Node 21, sehingga perintahnya berhenti dengan *"Could not find …"*. Ini
> **penahan lingkungan yang sudah ada sebelum task ini** dan tidak berkaitan dengannya;
> `package.json` tidak disentuh. Bentuk direktori menjalankan berkas uji yang sama persis.

**Uji unit yang ditambahkan** — tiga belas, seluruhnya atas aturan murni:

| Yang dibuktikan | Bukti |
| --- | --- |
| `VAL-45` — dua kunci berturut-turut tidak pernah sama | `dua kunci berturut-turut tidak pernah sama` |
| `VAL-45` — kunci yang sama ikut apa adanya, tidak dibuat ulang | `kunci yang sama ikut apa adanya ke muatan` |
| `VAL-43` — nama yang diketik ditolak sebagai instansi | `instansi perujuk wajib penunjuk, bukan nama yang diketik` |
| `VAL-44` — nomor surat kosong ditolak, spasi tidak dihitung | `nomor surat rujukan kosong ditolak` |
| `AC-50` — muatan tidak punya ruas nama perujuk sama sekali | `muatan rujukan membawa penunjuk perujuk, bukan nama` |
| Cara bayar Asuransi menuntut kartu; Tunai tidak mengirim kartu | tiga uji bentuk muatan |

### Verifikasi manual

`MANUAL TEST: NOT FEASIBLE`

Alasannya konkret, bukan karena tidak sempat:

1. **Data induk perujuk masih kosong.** Butir *Langkah berikutnya* pada `BE-EXT-02.md` berbunyi
   *"Mengisi daftar instansi dan dokter perujuk"*, dan pengisian itu belum dikerjakan. Tanpa
   satu pun barisnya, kontrol yang paling perlu diperiksa — pemilihan instansi, penyaringan
   dokter menurut instansi, dan pengosongan dokter saat instansi diganti — tidak dapat
   dijalankan sungguhan.
2. **Instance backend yang berjalan belum memuat endpoint baru.** Aplikasi dihentikan saat
   `BE-LAB-08` dikerjakan dan belum dijalankan kembali, sehingga ketiga endpoint pendaftaran
   maupun dua endpoint perujuk belum dapat dipanggil dari layar.

**Yang wajib diperiksa manual begitu kedua hal di atas beres**, karena tidak satu pun dapat
dijamin oleh lint, uji, maupun build:

| No | Yang diperiksa | Yang diharapkan |
| ---: | --- | --- |
| 1 | Mengetik di kotak pencarian pasien | Daftar tersaring; permintaan lama dibatalkan sehingga hasil tidak tertukar |
| 2 | Mengklik baris pasien lalu menekan salah satu tombol daftar | Formulir terbuka dengan pasien sudah terisi beserta namanya |
| 3 | Membuka kotak dokter sebelum instansi dipilih | Nonaktif, disertai keterangan "Pilih instansi perujuk lebih dulu" |
| 4 | Mengganti instansi setelah dokter dipilih | Dokter **kosong kembali** |
| 5 | Mengubah cara bayar menjadi Asuransi lalu kembali ke Tunai | Kotak kartu muncul lalu hilang, dan pilihannya ikut kosong |
| 6 | Menekan **Daftarkan** dua kali berturut-turut | **Satu** kunjungan; penekanan kedua memunculkan pesan "sudah tercatat sebelumnya" |
| 7 | Menekan **Buat Pesanan Laboratorium** pada panel hasil | Formulir pesanan terbuka dengan kunjungan **sudah terisi** beserta nomornya |
| 8 | Mendaftarkan pasien yang tidak berhak didaftarkan petugas itu | Pesan `403` dari Registrasi tampil **apa adanya** |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-44` — pendaftaran datang langsung dari layar Laboratorium | Terpenuhi di sisi layar | Formulir mengirim `POST /walk-in`; pembuktian kunjungannya ada pada `BE-LAB-08.md` |
| `AC-45` — nol penulisan ke tabel kunjungan maupun pasien | Terpenuhi | Layar hanya memanggil grup `lab-patient-registrations`; tidak ada satu pun pemanggilan endpoint kunjungan atau pasien di berkas mana pun pada layar ini |
| `AC-46` — rujukan luar menyimpan kedua perujuk dan nomor surat | Terpenuhi di sisi layar | Ketiganya wajib, dan ketiganya ikut pada muatan — dijaga uji |
| `AC-50` — perujuk dipilih dari daftar, bukan diketik | Terpenuhi | Ditegakkan **secara struktural**: tidak ada kotak isian nama perujuk, dan uji memeriksa muatan tidak punya ruas namanya |

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Tiga layar ada | Terpenuhi | Ketiga route terbentuk pada keluaran `npm run build` |
| Instansi dan dokter perujuk dipilih dari daftar | Terpenuhi | Dua endpoint bacanya dibangun pada sesi ini; keduanya `ResourceFilterSelect`, dokter tersaring menurut instansi |
| Penolakan Registrasi diteruskan apa adanya | Terpenuhi | Potongan Redux menyimpan pesan server apa adanya; layar menampilkannya tanpa mengganti kalimat |
| Alur menyambung ke pembuatan pesanan | Terpenuhi | Panel hasil membawa `encounterId` dan `encounterNumber` ke formulir pesanan, dan kunjungannya langsung terisi |

**Butir milik task lain yang ikut tertutup.** Butir Verifikasi `BE-EXT-02` — *"kedua data induk
dapat dipilih dari daftar"* — yang selama ini tidak pernah terpenuhi, kini terpenuhi.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `npm run lint:errors` bersih. `npm run build` tanpa error baru |
| Masalah yang diketahui | `npm run test:unit` tidak dapat dijalankan apa adanya pada Node `v20.20.2` karena pola glob-nya. Sudah ada sebelum task ini; `package.json` tidak disentuh |
| Risiko tersisa | **Pertama, verifikasi manual belum dijalankan** — kedelapan skenario pada bagian 5 masih menunggu data induk perujuk diisi dan backend dijalankan kembali. **Kedua**, daftar perujuk yang kosong membuat formulir rujukan luar **tidak dapat dipakai sama sekali**, walaupun layarnya sudah benar; pengisian data induknya adalah pekerjaan bagian Data Induk. **Ketiga**, `useLabOrderForm` milik `FE-LAB-06` ikut disentuh — perubahannya aditif dan seluruh uji lama tetap lolos, tetapi pemilik task itu perlu tahu |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git frontend | Enam berkas `M`, sepuluh entri `??`. **Tidak ada** `git add`, `commit`, maupun `push` |
| Status Git backend | Sembilan berkas `M`, tiga belas entri `??` — mencakup pekerjaan `BE-LAB-08` pada sesi yang sama. **Tidak ada** `git add`, `commit`, maupun `push` |
| Langkah berikutnya | 1. Menjalankan kembali backend, lalu mengisi data induk instansi dan dokter perujuk. 2. Menjalankan kedelapan skenario verifikasi manual pada bagian 5. 3. `FE-LAB-07` sampai `FE-LAB-09` — ketiganya siap dikerjakan dan berantai |
