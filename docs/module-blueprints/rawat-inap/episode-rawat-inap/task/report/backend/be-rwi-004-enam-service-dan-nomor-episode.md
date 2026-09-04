# Laporan Perubahan Backend — `BE-RWI-004`

> **Pembaruan 26 Agustus 2026 — validasi sudah dijalankan.** Field `Status` di bawah beserta
> setiap baris `NOT RUN` untuk `dotnet build` dan `dotnet test` pada laporan ini **sudah tidak
> berlaku**. Kedua perintah dijalankan 26 Agustus 2026 atas seluruh solution: **build 0 error**,
> dan **255 test hijau, 0 gagal**. Perinciannya ada pada
> [laporan validasi](be-rwi-validasi-build-dan-test.md).
>
> Yang **belum** berubah: acceptance criteria dan DoD task ini tetap belum terbukti penuh —
> build hijau bukan tanda selesai — sehingga tandanya pada roadmap tetap 🟡.

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-004` |
| Judul | Enam service terdaftar dan angka pengaturan terbaca dari master |
| Slice | S0 — Modul benar-benar berdiri |
| Roadmap | `docs/module-blueprints/rawat-inap/roadmap/backend-roadmap.md` bagian 4 |
| Trace | `02-backend-architecture.md` §4.14 s.d. §4.19, §7.1 langkah 6; `RWI-DEC-008`; `RWI-AC-003`, `RWI-AC-110` |
| Contract version | API `0.4.0` — **tidak berubah**. Task ini tidak menyentuh satu pun endpoint |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `0869775` |
| Tanggal pengerjaan | 24 Agustus 2026 |
| Jenis perubahan | Penambahan enam service baru beserta pendaftarannya. Tidak ada tabel, endpoint, maupun perilaku modul lain yang tersentuh |
| Status | **IMPLEMENTASI SELESAI — VALIDASI BELUM DIJALANKAN.** Lihat bagian 5.1 |

> **Peringatan yang tidak boleh dilewat.** Pemilik pekerjaan meminta pengerjaan dilakukan
> **tanpa menjalankan build**. `dotnet build` dan `dotnet test` **tidak dijalankan** pada sesi
> ini, sehingga acceptance criteria nomor 1 — "aplikasi menyala tanpa galat" — **belum
> terbukti sama sekali**. Task ini belum boleh ditandai selesai. Lihat bagian 5.1 dan 8.

---

## 1. Apa yang dibangun, dan kenapa

### 1.1 Keadaan sebelum perubahan

Modul Rawat Inap sudah punya 13 tabel (`BE-RWI-001` dan `BE-RWI-003`) dan tujuh enum, tetapi
**tidak punya satu baris pun kode yang menjalankan aturan bisnis**. Folder
`Areas/HealthServices/InPatientManagement/` hanya berisi `Enums/` dan `Models/`.

### 1.2 Kenapa pendaftaran service dikerjakan sebelum controller

Controller di project ini menerima service lewat *dependency injection* — mekanisme yang
membentuk objek yang dibutuhkan sebuah kelas secara otomatis. Kalau service-nya tidak
didaftarkan, kegagalannya **tidak muncul saat aplikasi dibangun**.

> **Contoh.** Seorang pengembang membuat `InpatientEpisodeController` yang meminta
> `InpEpisodeService`, tetapi lupa menambahkan satu baris pendaftaran di `Program.cs`.
> Aplikasi tetap dibangun tanpa galat, tetap menyala, dan Swagger tetap menampilkan
> endpoint-nya. Kegagalannya baru muncul ketika petugas admisi menekan tombol Simpan: balasan
> yang diterima adalah `500 Internal Server Error` tanpa penjelasan apa pun, karena aplikasi
> gagal membentuk controller-nya bahkan sebelum satu baris kode modul sempat dijalankan.
>
> Task ini memindahkan kegagalan seperti itu ke waktu build, lewat satu test yang meminta
> keenam service dari container.

### 1.3 Kenapa angka batas waktu dibaca dari master, bukan ditanam di kode

Ini keputusan `RWI-DEC-008`, dan alasannya bukan soal kerapian.

> **Contoh.** Rumah sakit memutuskan pemesanan tempat tidur cukup mengunci 90 menit, bukan
> 120. Bila angka `120` ditulis langsung di dalam kode service, perubahan itu berarti mengubah
> kode, membangun ulang, dan menurunkan aplikasi sebentar — pekerjaan satu hari yang melibatkan
> tiga orang. Bila angka itu satu baris di `MstInpatientSetting`, admin mengubahnya sendiri
> dari layar pengaturan pada pukul 09:00, dan pemesanan yang dibuat pukul 09:05 sudah memakai
> 90 menit.

---

## 2. Proses bisnis

### 2.1 Tujuan

Controller yang dibuat task berikutnya benar-benar dapat dijalankan, dan seluruh angka batas
waktu modul Rawat Inap dibaca dari master — bukan ditanam di kode.

### 2.2 Pelaku

| Pelaku | Perannya |
| --- | --- |
| Tim Backend/API | Pemilik perubahan ini |
| Admin Rawat Inap | Merasakan akibatnya lewat layar pengaturan: angka yang ia ubah langsung dipakai |
| Petugas admisi | Merasakan akibatnya lewat nomor episode yang lahir bernomor unik |

### 2.3 Pemicu

Aplikasi dinyalakan. Keenam service didaftarkan ke container pada tahap penyusunan aplikasi.

### 2.4 Prasyarat

| Prasyarat | Alasan |
| --- | --- |
| 13 tabel Rawat Inap sudah ada | `BE-RWI-001` dan `BE-RWI-003` |
| Baris pengaturan `DEFAULT` sudah terisi | `BE-RWI-002`. **Bukan** prasyarat keras: bila belum terisi, modul memakai nilai bawaan dan mencatat peringatan |

### 2.5 Langkah utama — membaca angka pengaturan

1. Sebuah service Rawat Inap membutuhkan angka batas waktu, misalnya lama pemesanan mengunci.
2. Service itu memanggil `InpSettingService`.
3. `InpSettingService` membaca database, mencari baris pengaturan yang aktif. Baris bertanda
   default didahulukan; bila ada beberapa baris aktif, yang paling baru dibuat yang dipakai.
4. Bila baris ditemukan, angkanya dikembalikan apa adanya.
5. Bila **tidak** ada satu pun baris aktif, nilai bawaan dikembalikan **dan** satu peringatan
   ditulis ke log, menyebut angka apa saja yang sedang dipakai dan cara memperbaikinya.
6. Pemanggil menerima angkanya dan menjalankan aturannya.

Tidak ada penyimpanan sementara di langkah mana pun. Setiap pemanggilan membaca ulang.

### 2.6 Langkah utama — membentuk nomor episode

1. `InpEpisodeService` membutuhkan nomor untuk episode yang baru lahir.
2. Ia memanggil `InpEpisodeNumberService`.
3. `InpEpisodeNumberService` menanyakan awalan nomor kepada `InpSettingService`.
4. Nomor dibentuk dari tiga bagian: awalan, waktu sampai detik, dan enam huruf/angka acak.
5. Nomor dikembalikan. Service ini tidak menulis apa pun ke database dan tidak membuka
   transaksi sendiri.

### 2.7 Aturan bisnis yang melekat

| Aturan | Diwujudkan sebagai | Dasar |
| --- | --- | --- |
| Angka batas waktu dibaca dari master | `InpSettingService` membaca `MstInpatientSetting` | `RWI-DEC-008` |
| Nilai baru berlaku pada pembacaan berikutnya | Tidak ada penyimpanan sementara | `RWI-AC-003`, `RWI-AC-110` |
| Master kosong tidak mematikan modul | Nilai bawaan dikembalikan | Acceptance criteria 3 |
| Pemakaian nilai bawaan selalu terlihat | Satu peringatan pada log setiap pembacaan | Acceptance criteria 3 |
| Awalan nomor tidak ditanam di kode | Dibaca dari `EpisodeNumberPrefix` | Acceptance criteria 4 |
| Dua permintaan bersamaan tidak menghasilkan nomor kembar | Enam huruf/angka acak + unique index `IX_InpEpisode_EpisodeNumber` | Acceptance criteria 5, QBE-CODE-003 |

### 2.8 Jalur tidak normal

| Keadaan | Yang terjadi |
| --- | --- |
| Master pengaturan kosong | Nilai bawaan dipakai; satu peringatan ditulis pada log |
| Satu-satunya baris pengaturan dinonaktifkan | Sama seperti di atas — baris nonaktif tidak dianggap ada. `BE-RWI-005` menolak penonaktifan itu di layar admin |
| `EpisodeNumberPrefix` kosong atau hanya spasi | Awalan bawaan `RI` dipakai, sehingga nomor tidak pernah lahir tanpa awalan |
| Ada beberapa baris pengaturan aktif | Yang bertanda default didahulukan; bila seri, yang paling baru dibuat |

### 2.9 Hasil akhir

Enam service Rawat Inap dan dua service master Rawat Inap terdaftar dan dapat dibentuk. Dua di
antaranya sudah berisi perilaku penuh; empat sisanya baru kerangkanya. Tidak ada satu pun
perilaku aplikasi yang berubah bagi pengguna hari ini.

---

## 3. File yang diubah

| Berkas | Jenis | Isi |
| --- | --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpSettingService.cs` | Baru | **Terisi penuh.** Pembacaan pengaturan, nilai bawaan, peringatan, dan bentuk `InpatientSettingValues` |
| `Areas/HealthServices/InPatientManagement/Services/InpEpisodeNumberService.cs` | Baru | **Terisi penuh.** Pembentukan nomor episode |
| `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs` | Baru | Kerangka |
| `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs` | Baru | Kerangka |
| `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.cs` | Baru | Kerangka |
| `Areas/HealthServices/InPatientManagement/Services/InpCensusQueryService.cs` | Baru | Kerangka |
| `Program.cs` | Disunting | Enam pendaftaran service Rawat Inap, dua pendaftaran service master (`BE-RWI-005`), dan `using` |
| `QuilvianSystemBackend.Tests/InPatientManagement/InpSettingServiceTests.cs` | Baru | 6 test |
| `QuilvianSystemBackend.Tests/InPatientManagement/InpEpisodeNumberServiceTests.cs` | Baru | 6 test |
| `QuilvianSystemBackend.Tests/InPatientManagement/InpatientServiceRegistrationTests.cs` | Baru | 3 test |
| `QuilvianSystemBackend.Tests/InPatientManagement/RecordingLogger.cs` | Baru | Logger perekam, dipakai membuktikan peringatan benar-benar ditulis |

Pola yang diikuti: `builder.Services.AddScoped<TService>()` dengan kelas konkret tanpa
interface, sama seperti puluhan service lain. `InpEpisodeNumberService` mengikuti
`EmergencyDocumentNumberService` yang sudah ada.

### 3.1 Isi keenam service

| Service | Keadaannya | Tanggung jawab | Diisi task |
| --- | --- | --- | --- |
| `InpSettingService` | **Penuh** | Membaca pengaturan aktif, menyediakan nilai bawaan, mencatat peringatan | — |
| `InpEpisodeNumberService` | **Penuh** | Membentuk nomor episode unik dan terbaca manusia | — |
| `InpEpisodeService` | Kerangka | Satu-satunya pintu perubahan status episode, penugasan DPJP dan perawat | `BE-RWI-007`, `008`, `012`, `014`, `017`, `018`, `025`, `026` |
| `InpBedOccupancyService` | Kerangka | Memesan, menempatkan, memindahkan, melepas tempat tidur | `BE-RWI-010`, `011`, `013` s.d. `015`, `019`, `027` |
| `InpDischargeService` | Kerangka | Keputusan pulang, resume, daftar periksa, lima syarat penutupan | `BE-RWI-020` s.d. `027` |
| `InpCensusQueryService` | Kerangka | Census, lama dirawat, papan tempat tidur, daftar pantau | `BE-RWI-009`, `015`, `016`, `029` |

Kerangka berarti: kelas ada, dependency-nya sudah ditetapkan sesuai `02-backend-architecture.md`
§3.4, dan batas desain yang sudah terkunci ditulis sebagai catatan pada kelasnya — tetapi belum
ada method bisnis. Catatan itu bukan hiasan: ia mencegah task berikutnya diam-diam memecah
perpindahan pasien menjadi dua transaksi, atau menyetel status episode dari controller.

### 3.2 Bentuk nomor episode

```text
RI-260824153012-A1B2C3
│  │            └── enam huruf/angka acak
│  └── waktu pembuatan: 24 Agustus 2026, 15:30:12
└── awalan dari MstInpatientSetting.EpisodeNumberPrefix
```

Panjangnya 22 karakter untuk awalan dua huruf — jauh di bawah batas kolom `EpisodeNumber` yang
50 karakter.

> **Kenapa bukan nomor urut.** Nomor urut terlihat lebih rapi bagi manusia, tetapi
> pembentukannya menuntut membaca nomor terakhir lalu menambah satu.
>
> **Contoh kegagalannya.** Dua petugas admisi menekan Simpan pada pukul 15:30:12. Keduanya
> membaca nomor terakhir `RI-000123` pada saat yang hampir sama, keduanya menghitung
> `RI-000124`, dan keduanya menyimpannya. Salah satu gagal karena database menolak nomor
> kembar — dan petugas yang gagal melihat galat tanpa penjelasan setelah selesai mengisi
> seluruh formulir admisi.
>
> Cara yang dipakai di sini menghindarinya: bagian acak sepanjang enam huruf/angka membuat
> kedua nomor berbeda sejak lahir. `QBE-CODE-003` memang melarang `Count + 1` dan `Max + 1`
> sebagai satu-satunya pembentuk nomor, tepat karena kegagalan di atas.

### 3.3 Yang sengaja tidak dibuat

| Yang tidak dibuat | Alasan |
| --- | --- |
| Method bisnis pada empat service kerangka | Masing-masing punya task sendiri. Menulisnya sekarang berarti menulis kode yang belum ada acceptance criteria-nya |
| Penyimpanan sementara nilai pengaturan | Melanggar `RWI-AC-003` — nilai baru wajib berlaku pada pembacaan berikutnya |
| Interface untuk keenam service | Project ini memakai kelas konkret. Menambah interface hanya untuk modul ini berarti dua konvensi berjalan sejajar |
| Penyedia nomor seri bersama yang atomik | `QBE-CODE-006` menyebutnya untuk penyedia bersama; project ini belum punya satu pun. Roadmap secara tegas menunjuk `EmergencyDocumentNumberService` sebagai pola. Bila kelak rumah sakit menuntut nomor tanpa lompatan, penyedia seperti itu menjadi task tersendiri |
| Controller apa pun | `BE-RWI-005` dan seterusnya |

---

## 4. Grup endpoint yang terdampak

**Tidak ada.** Task ini tidak menambah, mengubah, maupun menghapus satu pun endpoint. Seluruh
endpoint modul Rawat Inap pada `contracts/api-contract.md` tetap berstatus
**Rencana (belum tersedia)** setelah task ini.

---

## 5. Verifikasi

### 5.1 Yang **belum** dijalankan, dan kenapa

| Pemeriksaan | Perintah | Hasil |
| --- | --- | --- |
| Build Release | `dotnet build -c Release` | ✅ **PASS** — Build succeeded, 0 Error(s), 26 Agustus 2026 |
| Test | `dotnet test` | ✅ **PASS** — Passed! Failed 0, Passed 255, Total 255 |
| Aplikasi menyala | `dotnet run` | **BELUM DIJALANKAN** — di luar wewenang task, dan memerlukan basis data |

Acceptance criteria nomor 1 berbunyi "aplikasi menyala tanpa galat". Kriteria itu **belum
terbukti sama sekali** dan tidak boleh dianggap terpenuhi hanya karena kodenya sudah ditulis.

### 5.2 Yang diperiksa lewat pembacaan kode

| Pemeriksaan | Cara | Hasil |
| --- | --- | --- |
| Rantai dependency tidak melingkar | Penelusuran ctor keenam service | `InpSettingService` → tidak bergantung service lain; `InpEpisodeNumberService` → `InpSettingService`; `InpBedOccupancyService` → `InpSettingService`; `InpEpisodeService` → tiga di atas; `InpDischargeService` → `InpEpisodeService`; `InpCensusQueryService` → `InpSettingService`. Tidak ada lingkaran |
| Tidak ada nomor dibentuk dari hitungan baris | Pembacaan `InpEpisodeNumberService` | Tidak ada `Count`, `Max`, `Last`, maupun penghitung statis |
| Tidak ada penyimpanan sementara pengaturan | Pembacaan `InpSettingService` | Tidak ada field penampung; setiap pemanggilan menjalankan query |
| Diff hanya berisi penambahan | `git diff --stat` | `Program.cs` +114, tanpa satu pun baris terhapus |
| Tidak ada nilai rahasia | Pembacaan diff | Tidak ada |

### 5.3 Acceptance criteria

| # | Kriteria | Status | Bukti yang disiapkan |
| :---: | --- | :---: | --- |
| 1 | Aplikasi menyala tanpa galat | ⏳ **Belum diuji sama sekali** | Perlu `dotnet build` dan menjalankan aplikasi. Lihat bagian 8 |
| 2 | Keenam service dapat diminta dari container | ⏳ Belum diuji | `InpatientServiceRegistrationTests.KeenamServiceRawatInap_DapatDimintaDariContainer`, dengan `ValidateOnBuild` dan `ValidateScopes` menyala |
| 3 | `InpSettingService` membaca baris `DEFAULT`; bila tidak ada, mengembalikan nilai bawaan **dan** mencatat peringatan | ⏳ Belum diuji | `BarisAda_...`, `BarisTidakAda_NilaiBawaanDipakaiDanPeringatanDicatat` (memeriksa isi peringatannya, bukan hanya jumlahnya), `BarisNonaktif_...` |
| 4 | Nomor episode memakai awalan dari master, bukan huruf yang ditanam di kode | ⏳ Belum diuji | `Awalan_DibacaDariMasterBukanDitanamDiKode`, `AwalanDiubahAdmin_NomorBerikutnyaMemakaiAwalanBaru` |
| 5 | Dua permintaan nomor bersamaan tidak menghasilkan nomor kembar | ⏳ Belum diuji | `DuaPermintaanBersamaan_TidakMenghasilkanNomorKembar` — 20 nomor dibentuk pada detik yang sama persis, lalu dihitung berapa yang unik. Jaminan sebenarnya adalah unique index di database, bukan peluang |
| — | Tambahan: nilai diubah admin, pembacaan berikutnya memakai nilai baru | ⏳ Belum diuji | `NilaiDiubahAdmin_PembacaanBerikutnyaMemakaiNilaiBaru`. Ini `RWI-AC-003` |

### 5.4 Definition of Done

| Butir DoD | Status |
| --- | :---: |
| Enam service terdaftar | ✅ Ditulis pada `Program.cs` baris 316–321 |
| Dua terisi penuh | ✅ `InpSettingService`, `InpEpisodeNumberService` |
| Test aktivasi dan tiga unit test lulus | ⏳ **Belum dijalankan** |
| Build lulus | ⏳ **Belum dijalankan** |

---

## 6. Penyimpangan terhadap roadmap dan blueprint

| Yang tertulis | Yang dikerjakan | Alasan |
| --- | --- | --- |
| Roadmap scope: "pendaftaran pada `Program.cs`" untuk enam service | Delapan service didaftarkan | Dua tambahannya milik `BE-RWI-005` — `InpatientSettingService` dan `InpatientClearanceItemService`. Keduanya ditulis pada blok terpisah dengan komentar yang menyebut pemiliknya, supaya tidak terbaca sebagai bagian dari keenam service modul |
| Arsitektur §4.19: `InpSettingService` "membaca pengaturan aktif" | Sesuai | — |
| Arsitektur §3.4: `InpEpisodeService --> InpSettingService` | Sesuai, ditambah `InpEpisodeNumberService` sebagai dependency | §3.4 menggambarkan `InpatientEpisodeController --> InpEpisodeNumberService`. Nomor episode dibentuk saat episode lahir, yaitu di dalam `InpEpisodeService` — bukan di controller, karena `QBE-CODE-002` melarang controller membangkitkan nomor bisnis. Pemasangannya di service adalah penerapan aturan itu, dan perlu disinkronkan pada §3.4 oleh pemilik arsitektur |

---

## 7. Kesesuaian QBE

| ID | Berlaku pada | Kepatuhan |
| --- | --- | --- |
| `QBE-MOD-001` | Seluruh berkas | Keenam service berada di bawah `Areas/HealthServices/InPatientManagement/Services/`, modul pemiliknya |
| `QBE-MOD-002` | Seluruh berkas | `InPatientManagement` / `Inp` berstatus `ACTIVE` sejak `RWI-DEC-068`, 24 Agustus 2026 |
| `QBE-NAM-001` | Seluruh berkas | Tidak ada satu pun nama berawalan `Trx` |
| `QBE-NAM-002` | Seluruh berkas | Awalan `Inp` sesuai registry |
| `QBE-SVC-001` | Seluruh service | Service memegang orkestrasi domain. Tidak ada controller pada task ini |
| `QBE-CODE-001` | `InpEpisodeNumberService` | Service yang memiliki bentuk nomor; pembentukannya deterministik dan aman terhadap dua permintaan bersamaan |
| `QBE-CODE-002` | `InpEpisodeNumberService` | Tidak ada controller yang membangkitkan nomor. Nomor dibentuk di dalam service |
| `QBE-CODE-003` | `InpEpisodeNumberService` | Tidak memakai `Count + 1`, `Max + 1`, penghitung statis, maupun kunci proses lokal |
| `QBE-CODE-004` | `InpEpisode.EpisodeNumber` | Unique index `IX_InpEpisode_EpisodeNumber` sudah dibuat `BE-RWI-003` |
| `QBE-CODE-005` | `InpEpisodeNumberService` | Awalan dan bentuknya dimiliki modul, dibaca dari master modul |
| `QBE-CODE-006` | — | **Tidak berlaku sepenuhnya.** Project ini belum punya penyedia nomor seri bersama. Lihat bagian 3.3 dan 9 |
| `QBE-ENUM-001` | — | Tidak ada enum baru |
| `QBE-DTO-001` | `InpatientSettingValues` | Bukan entity EF; bentuk terpisah yang tidak dapat dipakai mengubah baris master |
| `QBE-LOG-001` | — | Tidak ada perubahan status pada task ini |

---

## 8. Langkah berikutnya

| Urutan | Langkah | Perintah atau keterangan |
| :---: | --- | --- |
| 1 | Jalankan build | `dotnet build -c Release` |
| 2 | Jalankan test | `dotnet test --filter FullyQualifiedName~InPatientManagement` |
| 3 | Nyalakan aplikasi sekali dan pastikan tidak ada galat | Membuktikan acceptance criteria 1 |
| 4 | Perbarui bagian 5.3 dan 5.4 laporan ini dengan hasil sebenarnya | Wajib sebelum task ditandai selesai |
| 5 | Tandai `BE-RWI-004` selesai pada roadmap dan `requirement-traceability.md` | Hanya bila langkah 1–3 lulus |
| 6 | `add`, `commit`, `push` | Dijalankan sendiri oleh pemilik pekerjaan |

---

## 9. Risiko tersisa

| Risiko | Sifat | Pemilik |
| --- | --- | --- |
| Build, test, dan penyalaan aplikasi belum dijalankan | Kelima acceptance criteria belum terbukti | Backend/API |
| Peringatan nilai bawaan ditulis pada **setiap** pembacaan | Bila master pengaturan kosong di lingkungan yang sibuk, log akan penuh peringatan yang sama. Ini disengaja — peringatan yang muncul sekali lalu diam akan terlewat — tetapi perlu diawasi bila log dikirim ke sistem berbayar per baris | Backend/API |
| Nomor episode tidak berurut dan dapat melompat | Bila kelak ada tuntutan hukum atau regulasi yang mewajibkan nomor tanpa lompatan, bentuk nomor ini tidak memenuhinya dan harus diganti. `QBE-CODE-006` menyebut kebutuhan itu perlu disetujui terpisah. Belum ada tuntutan seperti itu pada blueprint modul ini | Product/Domain |
| Empat service masih kerangka | Wajar dan disengaja. Yang perlu dijaga: catatan batas desain pada tiap kelas jangan dihapus saat method-nya diisi | Backend/API |
| Provider InMemory tidak menegakkan index unik | Test nomor kembar membuktikan bahwa **kodenya** tidak menghasilkan nomor kembar, bukan bahwa database menolaknya. Penjagaan database sudah terbukti pada `BE-RWI-003` | Backend/API |
