# Laporan Perubahan Backend — `BE-LAB-08`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-08` |
| Judul | Endpoint pendaftaran pasien laboratorium |
| Slice | `S13a`, `S13b` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 4, gelombang `MVP-1` |
| Trace | `FR-08.1` .. `FR-08.5`; `LAB-DEC-032`, `LAB-DEC-035`; `AC-44`, `AC-45`, `AC-46`, `AC-50`; `VAL-40` .. `VAL-45` |
| Contract version | `LAB-API-v1` r3 grup Lab Patient Registration; `LAB-INT-v1` r3 `INT-05` — `approved`, dikunci 2026-09-02 |
| Dependency | `BE-EXT-02` **`SELESAI`**, `BE-EXT-03` **`SELESAI` untuk kolom dan kontrak**. Pelaksana `INT-05` yang selama ini belum ada **dibangun pada sesi ini** |
| Klasifikasi | `HEAVY` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source Laboratorium, source Registrasi, configuration, migration, project test, artefak blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `302ff7b`, branch `yoga` |
| Tanggal | 2026-09-07 |
| Status | **`SELESAI`.** Keempat butir DoD terpenuhi |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` (utama) dan `RegistrationManagement / Registration` (pelaksana `INT-05`) |
| Pemilik dan prefix registry | Prefix `Lab`, lifecycle `ACTIVE`. Prefix `Reg`, lifecycle `ACTIVE / LEGACY`. Persetujuan menyentuh Registrasi diberikan `andryzainhome` dan `sukmagp` pada 2026-09-01 lewat `LAB-REQ-001`, dirinci `LAB-REQ-003` bagian 3b |
| Keberlakuan | `NEW CODE` untuk grup Lab Patient Registration dan `EncounterIntakeService`; `TOUCHED LEGACY` untuk `TrxPatientEncounter` yang bertambah satu kolom |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-MOD-001`, `QBE-ENT-002`, `QBE-CFG-002`, `QBE-CODE-003`, `QBE-CODE-004`, `QBE-LOG-001`, `QBE-AUD-001` |
| QBE ID yang **tidak** berlaku | `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-002`, `QBE-MOD-003` — tidak ada entity baru; yang ada hanya satu kolom pada entity yang sudah ada. `QBE-NAM-001` dan `QBE-NAM-003` — nama `TrxPatientEncounter` adalah legacy milik Registrasi dan **tidak** dinamai ulang oleh task ini. `QBE-DB-001`, `QBE-DB-002` — bukan `LEGACY MIGRATION`. `QBE-PAGE-001` — pencarian pasien memakai batas baris, bukan pagination bernomor |
| Gerbang `BLOCKED — canonical governance unavailable` | Tidak aktif |

---

## 1. Masalah yang diperbaiki

Pasien yang hanya perlu satu pemeriksaan darah harus mengantre dua kali.

> Ia datang ke laboratorium, lalu diberi tahu bahwa ia belum punya kunjungan. Ia turun ke loket
> pendaftaran, mengantre di sana, mendaftar, lalu naik lagi ke laboratorium. Antrean pertama itu
> tidak menghasilkan apa pun selain satu baris kunjungan.

Petugas laboratorium sebenarnya sudah tahu semua yang diperlukan untuk mendaftarkannya. Yang
tidak ia miliki adalah **jalur yang sah** untuk melakukannya — dan itu bukan kelalaian,
melainkan batas yang disengaja: tabel kunjungan milik Registrasi, dan Laboratorium tidak boleh
menulis ke sana.

Persis di situlah letak bahayanya. Batas yang tidak punya jalur sah adalah batas yang cepat atau
lambat akan ditembus, biasanya lewat satu baris kode yang tampak wajar. Karena itu task ini
bukan sekadar menambah tiga endpoint; ia **membangun jalur sahnya** supaya jalur yang tidak sah
tidak pernah perlu ditempuh.

Ada juga masalah kedua yang lebih sunyi. Sebelum ini, menekan tombol Simpan dua kali pada
pendaftaran menghasilkan **dua kunjungan** untuk satu pasien pada hari yang sama — jalur loket
pun tidak punya penjagaan untuk itu. Akibatnya pesanan lab terbelah, hasil tersebar di dua
kunjungan, dan Billing menerima dua konteks tagihan.

---

## 2. Proses bisnis

### 2.1 Alur normal — pasien datang langsung (`AC-44`)

| Langkah | Pelaku | Yang terjadi |
| ---: | --- | --- |
| 1 | Petugas laboratorium | Membuka layar pendaftaran. Layar membuat **kunci idempotensi** saat itu juga |
| 2 | Petugas | Mencari pasien lewat `GET /patient-search` memakai nomor rekam medis atau nama |
| 3 | Petugas | Memilih pasien, unit layanan, dan cara bayar, lalu menekan Simpan |
| 4 | Laboratorium | Menyusun isian, lalu **menyerahkannya ke Registrasi**. Tidak menulis apa pun |
| 5 | Registrasi | Memeriksa kewenangan, memvalidasi isian, membentuk kunjungan beserta satu sumber pembayaran, dalam satu transaksi |
| 6 | Laboratorium | Menerima penunjuk kunjungan, membacakan identitas pasien, mengembalikannya ke layar |
| 7 | Petugas | Langsung membuat pesanan lab memakai penunjuk kunjungan itu |

Kunjungan yang terbentuk bertanda `IsWalkIn` benar, bersumber pendaftaran `WalkIn`, berstatus
`Registered`, dan **tidak** membawa antrean dokter — pasiennya memang tidak menuju poliklinik.

### 2.2 Alur normal — pasien rujukan luar (`AC-46`)

Sama seperti di atas, dengan tiga isian tambahan yang seluruhnya **wajib**: nomor surat rujukan,
instansi perujuk, dan dokter perujuk. Dua yang terakhir dikirim sebagai **penunjuk ke data
induk**, bukan sebagai nama.

> Petugas memilih "Klinik Sehat Sentosa" dari daftar, lalu memilih "dr. Andi Wijaya" dari daftar
> dokter klinik itu. Yang tersimpan pada kunjungan adalah dua penunjuk, bukan dua potong teks.

Kenapa ini penting sampai perlu diatur sekeras itu: sebagai teks bebas, "Klinik Sehat Sentosa",
"Kl. Sehat Sentosa", dan "sehat sentosa" terhitung **tiga institusi berbeda**, dan laporan asal
rujukan tidak akan pernah dapat dipercaya.

### 2.3 Menekan Simpan dua kali (`VAL-45`)

> Petugas menekan Simpan. Jaringan lambat, layar belum berubah. Ia menekan Simpan lagi.

| Percobaan | Kunci yang dikirim | Yang terjadi | Yang dikembalikan |
| ---: | --- | --- | --- |
| 1 | `a1b2c3` | Kunjungan `ENC-RSMMC-00001` terbentuk | `isReplay` = salah |
| 2 | `a1b2c3` — **sama** | **Tidak ada** yang terbentuk | Kunjungan `ENC-RSMMC-00001` yang sama, `isReplay` = benar |

Hasil akhirnya satu kunjungan, bukan dua. Bagi petugas, percobaan kedua tampak berhasil — dan
memang berhasil, karena yang ia inginkan sudah tercapai.

Sebaliknya, kunci yang **berbeda** berarti percobaan pendaftaran yang berbeda, dan itu memang
boleh menghasilkan kunjungan kedua. Idempotensi menjaga pengiriman ulang; ia tidak melarang
pasien datang dua kali dalam sehari.

**Kenapa kuncinya harus dibuat saat formulir dibuka, bukan saat tombol ditekan.** Bila kunci
dibuat pada penekanan tombol, penekanan kedua membawa kunci baru, dan seluruh perlindungan ini
tidak berlaku sama sekali. Syarat ini dicatat pada kartu `FE-LAB-05`.

### 2.4 Jalur tidak normal

| Keadaan | Jawaban | Aturan |
| --- | --- | --- |
| Pasien tidak dikenal atau tidak aktif | `422` — diarahkan mendaftarkan pasien di modul Pasien lebih dulu | `VAL-40` |
| Petugas tidak berhak membuat kunjungan | `403` — *"Anda tidak berhak membuat kunjungan baru. Hubungi bagian pendaftaran."* | `VAL-41` |
| Penyimpanan Registrasi tidak dapat dicapai | `503` — *"Pendaftaran gagal karena layanan registrasi sedang tidak dapat diakses. Silakan coba lagi."* | `VAL-42` |
| Instansi perujuk tidak dipilih, atau penunjuknya tidak terdaftar | `422` — *"Pilih instansi perujuk dari daftar…"* | `VAL-43`, `AC-50` |
| Nomor surat rujukan kosong | `422` — *"Nomor surat rujukan wajib diisi untuk pasien rujukan."* | `VAL-44` |
| Dokter perujuk bukan dokter instansi itu | `422` | Turunan `LAB-DEC-035` |
| Penjamin perusahaan dipilih | `422` — hanya dapat lewat pendaftaran loket | Mengikuti batas jalur kiosk |
| Kunci idempotensi tidak dikirim | `422` | Syarat pelaksanaan `VAL-45` |

Pada **seluruh** jalur gagal di atas, tidak ada satu baris pun yang tersimpan — termasuk sumber
pembayaran. Tidak ada kunjungan setengah jadi, dan tidak ada pesanan yatim.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk menetapkan |
| --- | --- |
| `roadmap/backend-roadmap.md` bagian 4 dan 8 | Cakupan, AC, DoD, dan pemetaan `VAL-40` .. `VAL-45` |
| `contracts/api-contract.md` bagian Lab Patient Registration | Route, verb, hak akses, dan nama DTO |
| `contracts/integration-contract.md` bagian 2b | Bentuk `INT-05`, pemetaan ruas, dan perilaku idempotensi |
| `contracts/validation-matrix.md` | Bunyi pesan dan status `VAL-40` .. `VAL-45` |
| `contracts/permission-audit-matrix.md` | Hak akses per endpoint dan kewajiban audit |
| `testing/acceptance-test-matrix.md` | Bentuk bukti yang diminta `AC-44` .. `AC-50` |
| `Areas/.../LaboratoryManagement/Controllers/LabCatalogController.cs` | Pola controller Laboratorium terdekat |
| `Areas/.../LaboratoryManagement/Services/LabExaminationService.cs` | Pola service tulis, exception, aktor, dan retry unique violation |
| `Areas/.../RegistrationManagement/Controllers/PatientEncounterController.cs` | Perilaku pembuatan kunjungan as-is, format nomor, dan bentuk sumber pembayaran |
| `Repositories/Configurations/HealthServices/TrxPatientEncounterConfiguration.cs` | Pola configuration dan index, termasuk filtered index milik `BE-EXT-03` |
| `Tests/.../LabScopeBoundaryTests.cs` | Pola uji batas modul berbasis telusur |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../RegistrationManagement/Models/TrxPatientEncounter.cs` | Satu kolom `RegistrationIdempotencyKey`, boleh kosong, panjang 100 |
| `Repositories/Configurations/.../TrxPatientEncounterConfiguration.cs` | Mapping kolom itu, ditambah **unique index tersaring** `"RegistrationIdempotencyKey" IS NOT NULL` |
| `.../RegistrationManagement/DTOS/EncounterIntakeDtos.cs` | **Baru.** Bentuk permintaan dan jawaban `INT-05` |
| `.../RegistrationManagement/Services/EncounterIntakeService.cs` | **Baru.** Pelaksana `INT-05` — idempotensi, kewenangan, validasi, dan pembentukan kunjungan beserta sumber pembayaran dalam satu transaksi |
| `.../LaboratoryManagement/DTOs/LabPatientRegistrationDtos.cs` | **Baru.** Lima DTO grup Lab Patient Registration |
| `.../LaboratoryManagement/Services/LabPatientRegistrationService.cs` | **Baru.** Pencarian pasien baca-saja, dan dua jalur pendaftaran yang **meneruskan** ke Registrasi |
| `.../LaboratoryManagement/Controllers/LabPatientRegistrationController.cs` | **Baru.** Tiga endpoint beserta pemetaan penolakan ke `403`, `409`, `422`, dan `503` |
| `Program.cs` | Dua baris pendaftaran service |
| `Migrations/20260907072413_AddRegistrationIdempotencyKeyToPatientEncounter.cs` | **Baru.** Satu kolom dan satu index; `Down` membalik keduanya |
| `Tests/.../LabPatientRegistrationTests.cs` | **Baru.** Delapan belas uji |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif sepenuhnya.** Tiga endpoint yang sudah tertulis sebagai `Rencana (belum tersedia)` kini tersedia. Tidak ada endpoint, ruas, pembungkus, maupun nilai status yang berubah bagi konsumen lama |
| Kontrak integrasi | `INT-05` kini punya pelaksana. Bentuk permintaan, jawaban, dan pemetaan ruasnya mengikuti `contracts/integration-contract.md` bagian 2b tanpa perubahan sepihak; yang ditambahkan pada kontrak hanya **catatan pelaksanaan**, bukan perubahan bentuk |
| Database | **Satu kolom dan satu index** pada `TrxPatientEncounter`. Aditif dan nullable, sehingga nol baris lama menjadi tidak sah. Migration `20260907072413_AddRegistrationIdempotencyKeyToPatientEncounter` **sudah diterapkan** ke `QuilvianNewDevYoga` dan **terbukti jalan dua arah** |
| Keamanan/Auth | Ketiga endpoint memakai `LabPatientRegistration : Read` dan `: Create` sesuai `permission-audit-matrix.md`. **Ditambah satu lapis:** Registrasi memeriksa sendiri `PatientEncounter : Create` atas pemanggilnya, sehingga hak akses layar Laboratorium **tidak** dengan sendirinya memberi hak membuat kunjungan (`VAL-41`). Setiap kunjungan yang terbentuk menghasilkan satu baris audit beserta aktornya |

### 3.4 Keputusan dan selisih yang perlu diketahui

| No | Butir | Penjelasan |
| ---: | --- | --- |
| 1 | **Pelaksana `INT-05` dibangun di sisi Registrasi, bukan di Laboratorium** | Ini keputusan terpenting pada task ini. Kartu `BE-LAB-08` mencakup tiga endpoint Laboratorium saja, sementara pelaksananya milik `registration-management` dan belum pernah dibangun. Membangunnya di Laboratorium akan melanggar `AC-45` secara langsung; tidak membangunnya sama sekali membuat ketiga endpoint tidak punya apa pun untuk dipanggil. Yang dipilih: membangunnya **di folder milik Registrasi**, di bawah persetujuan `LAB-REQ-003` bagian 3b yang memang sudah mencantumkan *"idempotensi terbukti lewat uji"* sebagai bukti selesai, dan atas instruksi eksplisit pemilik modul pada sesi ini. Presedennya `BE-EXT-03`, yang menempuh jalur yang sama |
| 2 | **Pemanggilannya dalam proses, bukan HTTP** | Kontrak berbunyi "meneruskan isian ke Registrasi, menunggu jawabannya". Pada monolit ini, bentuk sejalur-proses adalah yang sudah dipakai seluruh integrasi Laboratorium lain: `INT-02` membaca kunjungan langsung, `INT-04` memanggil `AccessPermissionService`, `INT-06` membaca data induk. Memasang HTTP internal justru akan menjadi pola tandingan. Batas ownership tetap utuh karena yang dipanggil adalah service milik Registrasi |
| 3 | **Kunci idempotensi disimpan pada kunjungan, bukan pada tabel tersendiri** | Yang perlu dikenali saat permintaan yang sama datang dua kali adalah *kunjungan mana* yang sudah terbentuk. Menyimpannya di sana membuat pengenalan itu satu pembacaan index, dan membuat **basis data sendiri** yang menolak kunjungan kedua lewat unique index tersaring. Alternatifnya — tabel kunci tersendiri — menambah satu tabel dan satu jalur pembersihan tanpa menambah jaminan apa pun |
| 4 | **Idempotensi dua lapis, dan lapisan keduanya bukan kode** | Pembacaan lebih dulu menangani pengiriman ulang yang **berurutan**. Untuk dua permintaan yang tiba **bersamaan**, keduanya akan sama-sama membaca "belum ada" — dan di situ unique index yang memutuskan. Yang kalah menangkap pelanggaran unique, membaca ulang milik pemenang, lalu mengembalikannya. Tanpa lapisan kedua ini, idempotensi hanya berlaku selama tidak ada balapan, yang berarti tidak berlaku pada keadaan yang justru paling sering memicunya |
| 5 | **Format nomor kunjungan sengaja disamakan dengan jalur loket** | Kedua jalur menulis ke tabel dan unique index yang sama, sehingga penomorannya tidak boleh bercabang. Alokatornya memang bergaya pencarian nomor terkecil yang belum terpakai — pola legacy yang `QBE-CODE-003` larang dipakai **tanpa proteksi** untuk kode baru. Proteksinya ditambahkan di sini: unique index pada `EncounterNumber` (`QBE-CODE-004`, sudah ada) ditambah percobaan ulang saat tabrakan, sampai lima kali, lalu menyerah dengan `409`. Membangun penomoran baru yang berbeda akan jauh lebih buruk daripada memakai format yang sama dengan perlindungan yang ditambahkan |
| 6 | **`VAL-42` berubah makna, dan itu dicatat sebagai selisih** | Aturannya berbunyi "Registrasi tidak dapat dihubungi" dan ditulis untuk pemanggilan jarak jauh. Karena pemanggilannya sejalur proses, keadaan itu tidak dapat terjadi sebagaimana dibayangkan. Yang dipetakan menjadi `503` beserta pesan yang sama adalah kegagalan basis data — yaitu ketika penyimpanan Registrasi benar-benar tidak dapat dicapai. Ini penafsiran, bukan pelaksanaan harfiah, dan pemilik blueprint perlu mengesahkannya |
| 7 | **Kunjungan dari jalur ini tidak membawa snapshot kategori umur** | Jalur loket mengisi sembilan ruas `Age*` pada kunjungan. Logikanya berupa kurang lebih 120 baris privat di dalam `PatientEncounterController` dan tidak dapat dipakai ulang tanpa memindahkannya keluar — pemindahan yang berarti membongkar controller legacy sepanjang 130 KB, jauh di luar cakupan task ini, dan dilarang aturan legacy ratchet. Menyalinnya akan melahirkan sumber kebenaran kedua yang pasti bercabang. Karena itu ruas-ruas itu **dibiarkan kosong**, dan kolomnya memang boleh kosong. Lihat Risiko tersisa |
| 8 | **Tidak ada antrean dokter yang dibuat** | Pasien penunjang tidak menuju poliklinik; daftar kerjanya ada di modul penunjang itu sendiri. Karena itu `IsQueueRequired`, `IsDoctorRequired`, dan `IsScreeningRequired` disetel salah, dan tidak ada `TrxQueue` yang terbentuk. Membuat antrean dokter untuk pasien yang tidak akan menemui dokter akan mengotori papan antrean poliklinik |
| 9 | **Penjamin perusahaan ditolak dari jalur ini** | Mengikuti batas yang sudah berlaku pada jalur kiosk (`RWI-ENC-PAYER-001` bagian 7): hanya petugas admisi yang menerimanya. Jalur laboratorium menerima Tunai dan Asuransi |
| 10 | **Kunci idempotensi diwajibkan, dan itu menambah syarat bagi frontend** | Kontrak menyebut kuncinya "ditetapkan Laboratorium per percobaan pendaftaran, dikirim bersama permintaan", tetapi tidak menyebut kewajibannya. Dibuat **wajib** karena tanpa itu `VAL-45` tidak dapat ditegakkan sama sekali. `LAB-API-v1` hanya mengunci nama DTO tanpa daftar ruasnya, sehingga ini bukan perubahan kontrak; syaratnya dicatat pada kartu `FE-LAB-05` |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Patient Registration

Base URL: `api/v1/health-services/laboratory-management/lab-patient-registrations`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/patient-search` | Mencari pasien terdaftar sebelum mendaftarkan kunjungan baru, agar pasien lama tidak berakhir punya dua nomor rekam medis | `LabPatientRegistration : Read` |
| `POST` | `/walk-in` | Mendaftarkan pasien yang datang langsung ke laboratorium; kunjungannya dibuat Registrasi | `LabPatientRegistration : Create` |
| `POST` | `/external-referral` | Mendaftarkan pasien rujukan luar beserta instansi dan dokter perujuknya, keduanya dipilih dari daftar | `LabPatientRegistration : Create` |

Ketiganya mengembalikan `ApiResponse<T>`. Kedua endpoint `POST` mengembalikan
`LabRegistrationResultResponse` berisi penunjuk kunjungan, nomor kunjungan, tanggal, status,
identitas pasien seadanya, dan penanda `isReplay`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet msbuild -t:Compile` | Berhasil; nol error dan nol warning pada berkas baru | `PASS` | Keluaran perintah |
| `dotnet build QuilvianSystemBackend.csproj` | `0 Error(s)` | `PASS` | Keluaran perintah |
| `dotnet test` — `LabPatientRegistrationTests` | `Failed: 0, Passed: 18` | `PASS` | Keluaran perintah |
| `dotnet test` — seluruh `QuilvianSystemBackend.Tests` | `Failed: 0, Passed: 306` | `PASS` | Keluaran perintah |
| `dotnet test` — `UnitTests.Sqlite` | `Failed: 0, Passed: 176` | `PASS` | Keluaran perintah |
| `dotnet test` — `UnitTests.InMemory` | `Failed: 1, Passed: 889` | `EXISTING / ENVIRONMENT ISSUE` | Lihat catatan di bawah |
| `AC-44` — pendaftaran datang langsung | Kunjungan terbentuk ber-`IsWalkIn` benar, sumber `WalkIn`, nomor berawalan `ENC-RSMMC-`, dan tepat satu sumber pembayaran | `PASS` | `AC44_PendaftaranDatangLangsung_MembentukKunjunganWalkInLewatRegistrasi` |
| `AC-45` — telusur source | Nol pembentukan maupun pengubahan kunjungan dan data induk pasien pada seluruh berkas modul Laboratorium | `PASS` | `AC45_TidakSatuPunBerkasLaboratorium_MenulisKeKunjunganAtauDataIndukPasien` |
| `AC-45` — telusur perilaku | Dengan dua penyimpanan terpisah, kunjungan hanya muncul pada penyimpanan milik Registrasi; penyimpanan Laboratorium tetap kosong | `PASS` | `AC45_PenulisanKunjungan_DatangDariRegistrasiBukanDariLaboratorium` |
| `AC-46` — rujukan luar | Kunjungan menyimpan penunjuk instansi, penunjuk dokter, dan nomor surat `RJK/2026/00123` | `PASS` | `AC46_PendaftaranRujukanLuar_MenyimpanPenunjukPerujukDanNomorSurat` |
| `AC-50` — bentuk permintaan | Permintaan rujukan tidak punya satu pun ruas teks bernama perujuk; keduanya bertipe `Guid?` | `PASS` | `AC50_PermintaanRujukan_TidakPunyaRuasNamaPerujukSamaSekali` |
| `VAL-43` — instansi tidak terdaftar | Ditolak dengan pesan yang disepakati; nol kunjungan terbentuk | `PASS` | `VAL43_InstansiPerujukTidakTerdaftar_Ditolak` |
| `VAL-44` — nomor surat kosong | Ditolak; nol kunjungan terbentuk | `PASS` | `VAL44_NomorSuratRujukanKosong_Ditolak` |
| `VAL-41` — tanpa kewenangan | Ditolak dengan pesan yang disepakati; nol kunjungan dan nol sumber pembayaran | `PASS` | `VAL41_TanpaKewenanganRegistrasi_PendaftaranDitolakDanTidakMenyimpanApaPun` |
| `VAL-45` — Simpan dua kali | Satu kunjungan, penunjuk dan nomor identik, `isReplay` salah lalu benar | `PASS` | `VAL45_MenekanSimpanDuaKali_MenghasilkanSatuKunjunganYangSama` |
| Dua percobaan berbeda | Dua kunjungan — idempotensi tidak melarang pasien datang dua kali | `PASS` | `DuaPercobaanBerbeda_MenghasilkanDuaKunjungan` |
| Penolakan tanpa data setengah jadi | Nol kunjungan dan nol sumber pembayaran tersisa | `PASS` | `PenolakanRegistrasi_TidakMeninggalkanDataSetengahJadi` |
| Bentuk penyimpanan idempotensi | Kolom nullable panjang 100, index unik dan tersaring `IS NOT NULL` | `PASS` | `KunciIdempotensi_PunyaUniqueIndexTersaringPadaTabelKunjungan` |
| Bentuk kontrak endpoint | Route, verb, dan `[AccessPermission]` cocok satu per satu dengan `LAB-API-v1` r3; grup hanya punya tiga endpoint | `PASS` | `KetigaEndpoint_CocokDenganKontrakLabApiV1`, `GrupPendaftaran_HanyaPunyaTigaEndpoint` |
| `dotnet ef database update` — `Up` | `Done.` terhadap `QuilvianNewDevYoga` | `PASS` | Keluaran perintah; migration muncul pada `migrations list` |
| `dotnet ef database update <sebelumnya>` — `Down` | `Done.` | `PASS` | Keluaran perintah |
| `dotnet ef database update` — `Up` kembali | `Done.` | `PASS` | Keluaran perintah; migration muncul kembali pada `migrations list` |

Uji manual: `NOT FEASIBLE`. Menjalankan aplikasi dan menekan tombolnya membutuhkan frontend
`FE-LAB-05` yang belum ada. Seluruh perilaku yang dapat diperiksa tanpa layar sudah dijaga uji
otomatis di atas.

**Catatan satu uji yang gagal.**
`BillingFinalizationServiceTests.NormalFinalizationRequiresFullySettledOutstandingAndSetsInvoiceDate`
gagal dengan status faktur `CLOSED` padahal diharapkan `FINAL`. Kegagalan ini **sudah ada
sebelum task ini** dan tidak berkaitan dengannya. Buktinya diambil dengan menjalankan uji yang
sama pada worktree bersih di commit `302ff7b` tanpa satu pun perubahan task ini: hasilnya
`Failed: 1, Passed: 11` — sama persis. Task ini tidak menyentuh Billing.

**Tidak dijalankan:**

| Pemeriksaan | Alasan |
| --- | --- |
| `IntegrationTests.Postgres` | Menunggu database test tersendiri; penahan lingkungan yang sudah tercatat pada roadmap dan berlaku untuk seluruh repository, bukan akibat task ini |
| Deployment | Wewenang terpisah dan tidak diberikan |
| Eksekusi migration di luar dev pemilik | Wewenang terpisah dan tidak diberikan. Yang dijalankan hanya `QuilvianNewDevYoga` |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-44` — pendaftaran datang langsung menghasilkan kunjungan yang dibuat Registrasi, bertanda datang langsung, dan pesanan lab menempel padanya | Terpenuhi | Kunjungan terbentuk ber-`IsWalkIn` dan sumber `WalkIn`; jawaban membawa penunjuk kunjungan yang siap dipakai membuat pesanan. Bagian 5 |
| `AC-45` — tidak ada satu pun kode Laboratorium yang menulis ke tabel kunjungan maupun tabel pasien | Terpenuhi | Dibuktikan **dua kali dengan cara berbeda**: telusur seluruh source modul, dan uji perilaku dengan dua penyimpanan terpisah. Bagian 5 |
| `AC-46` — pendaftaran rujukan luar menyimpan dokter perujuk, instansi perujuk, dan nomor surat rujukan pada kunjungan yang dibuat Registrasi | Terpenuhi | Ketiganya terbaca pada kunjungan setelah pendaftaran. Bagian 5 |
| `AC-50` — nama instansi dan dokter perujuk dipilih dari daftar terkendali, bukan diketik bebas; kunjungan menyimpan penunjuk, bukan teks | Terpenuhi | Ditegakkan **secara struktural**: permintaannya tidak punya ruas nama sama sekali. Penunjuk yang tidak terdaftar ditolak `422`. Bagian 5 |

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Tiga endpoint tersedia | Terpenuhi | Route, verb, dan hak aksesnya cocok satu per satu dengan `LAB-API-v1` r3, dan grupnya memang hanya tiga |
| `AC-45` terbukti lewat uji unit | Terpenuhi | Dua uji terpisah, keduanya `PASS` |
| Idempotensi terbukti | Terpenuhi | Kunci sama dua kali menghasilkan satu kunjungan; ditegakkan unique index tersaring, bukan kode aplikasi |
| Penolakan Registrasi diteruskan tanpa menyimpan data setengah jadi | Terpenuhi | Empat uji jalur gagal, seluruhnya memeriksa nol kunjungan **dan** nol sumber pembayaran |

**Butir DoD milik task lain yang ikut tertutup.** `BE-EXT-03` butir *"idempotensi terbukti lewat
uji"* yang sejak 2026-09-04 tercatat **belum terpenuhi** kini terpenuhi, karena endpoint yang
dituntutnya sudah ada dan buktinya ada pada laporan ini bagian 5.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build menghasilkan 196 warning, seluruhnya `CS1573`/`CS1574`/`CS1587` pada berkas modul lain yang tidak disentuh task ini. Berkas baru task ini menghasilkan **nol** warning |
| Masalah yang diketahui | Satu uji Billing gagal, dan kegagalannya **sudah ada sebelum task ini** — dibuktikan pada worktree bersih di commit `302ff7b`. Bukan cakupan task ini dan tidak diperbaiki di sini |
| Risiko tersisa | **Pertama, snapshot kategori umur.** Kunjungan dari jalur ini tidak membawa sembilan ruas `Age*` yang diisi jalur loket. Kolomnya boleh kosong sehingga tidak ada baris yang menjadi tidak sah, tetapi laporan atau pemilihan tarif yang kelak menyaring berdasarkan kategori umur akan **melewatkan** kunjungan laboratorium. Perbaikan yang benar adalah memindahkan logika umur keluar dari `PatientEncounterController` menjadi milik bersama — pekerjaan tersendiri yang belum berpemilik task. **Kedua, `VAL-42` ditafsirkan**, bukan dilaksanakan harfiah; lihat bagian 3.4 butir 6. **Ketiga**, alokator nomor kunjungan memindai seluruh nomor berawalan yang sama setiap kali — biaya yang tumbuh seiring jumlah kunjungan, diwarisi dari jalur loket dan sengaja tidak diubah agar penomoran kedua jalur tidak bercabang |
| Perubahan sampingan | `NONE`. `Migrations/ApplicationDbContextModelSnapshot.cs` berubah, dan itu memang keluaran wajib `dotnet ef migrations add` |
| Interupsi | Satu, bersifat lingkungan. Aplikasi backend sedang berjalan dan mengunci `bin/Debug/net9.0/QuilvianSystemBackend.exe`, sehingga build, test, dan `dotnet ef` tidak dapat jalan. Atas persetujuan eksplisit pemilik modul, proses itu dihentikan, lalu seluruh verifikasi dijalankan. **Aplikasinya belum dijalankan kembali** — pemilik modul yang akan me-restart sendiri. Satu migration kosong sempat terbentuk lebih dulu karena `--no-build` memakai assembly lama; berkasnya dihapus dan migration dibuat ulang setelah build bersih |
| Status Git | `M` pada `TrxPatientEncounter.cs`, `ApplicationDbContextModelSnapshot.cs`, `Program.cs`, `TrxPatientEncounterConfiguration.cs`, serta lima artefak blueprint. `??` pada lima berkas source baru, dua berkas migration, dan satu berkas uji. **Tidak ada** `git add`, `commit`, maupun `push` |
| Langkah berikutnya | 1. Menjalankan kembali aplikasi backend. 2. **`FE-LAB-05`** — layar pendaftaran pasien laboratorium; penahannya sudah dicabut, dan tiga syarat pelaksanaannya dicatat pada kartu task. 3. Memutuskan nasib snapshot kategori umur bagi kunjungan jalur penunjang. 4. Mengesahkan penafsiran `VAL-42` pada matriks validasi |
