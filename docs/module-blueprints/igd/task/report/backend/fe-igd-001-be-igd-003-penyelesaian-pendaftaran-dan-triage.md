# Laporan Perubahan — `FE-IGD-001`, `BE-IGD-003`, `FE-IGD-003`, `FE-IGD-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-001`, `BE-IGD-003`, `FE-IGD-003`, `FE-IGD-004` |
| Slice | F0 — Pendaftaran; S0 — Data master; F2 — Antrean dan penilaian triage |
| Repository | `NewQuilvianSystemBackend`, `QuilvianSystemFrontendDev` |
| Contract version | API `0.2.0`, State `0.2.0`, Validation `0.2.0` — tidak ada kontrak yang berubah |
| Tanggal | 21 Agustus 2026 |
| Batasan | Tampilan layar pendaftaran pasien IGD **tidak diubah** sesuai permintaan |
| **Status** | **Kode selesai, lint bersih, build lulus, 33 unit test lulus. Alur simpan belum dijalankan sungguhan** |

---

## 1. Bug paling serius yang ditemukan: status pendaftaran salah kirim

Ini bukan penyempurnaan, melainkan perbaikan data yang salah tersimpan sejak awal.

Frontend memiliki `EMERGENCY_VISIT_REGISTRATION_STATUS` yang hanya berisi satu baris,
`REGISTERED: 1`. Enum backend `EmergencyRegistrationStatus` berbunyi lain:

| Nilai | Arti di backend | Yang dikirim frontend |
| ---: | --- | --- |
| 1 | `Pending` | dikirim, dan disebut "Registered" |
| 2 | `Provisional` | — |
| 3 | `Registered` | tidak pernah dikirim |
| 4 | `Completed` | — |
| 5 | `Cancelled` | — |

Akibatnya setiap pendaftaran IGD yang sudah tuntas tersimpan sebagai `Pending`. Petugas
menyelesaikan pendaftaran, layar menyatakan berhasil, dan basis data mencatat bahwa
pendaftaran itu belum diproses siapa pun.

Peta lengkap kelima nilai kini tersedia beserta label dan varian warnanya, dan payload
kunjungan mengirim `3`.

> **Data lama tidak diperbaiki.** Kunjungan yang terlanjur tersimpan sebagai `Pending`
> tetap `Pending`. Membedakan "benar-benar belum didaftarkan" dari "korban bug ini" tidak
> dapat dilakukan dari data itu sendiri, dan menebaknya berarti memalsukan riwayat.
> Keputusan koreksi data ada pada Product/Domain owner.

---

## 2. `FE-IGD-001` — Pendaftaran berhenti bertentangan dengan kontrak

### 2.1 Bukti terhadap acceptance criteria

| No | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Payload encounter mengirim `Outpatient` | **Terpenuhi** | Sudah benar sebelum sesi ini; kini terkunci oleh test |
| 2 | Pemetaan status registrasi mengikuti backend | **Terpenuhi** | Lihat bagian 1; 5 test |
| 3 | Unit IGD dari pengaturan IGD, bukan tebakan kode | **Terpenuhi** | Lihat 2.2 |
| 4 | Kegagalan panggilan kedua memberi cara melanjutkan | **Terpenuhi** | Lihat 2.3 |

### 2.2 Unit IGD tidak lagi ditebak dari nama

Sebelumnya `isEmergencyServiceUnit` mencocokkan `"SU-ER-001"`, `"IGD"`, `"GAWAT DARURAT"`,
dan `"EMERGENCY"`. Rumah sakit yang menamai unitnya berbeda tidak akan terdeteksi, dan unit
lain yang kebetulan bernama mirip bisa ikut terpilih.

Urutan pemilihan sekarang:

1. Unit yang ditunjuk `MstEmergencySetting.DefaultEmergencyServiceUnitId`, dibaca lewat
   `EmergencySettingController`. Ini keputusan yang sudah diambil rumah sakit.
2. Satu-satunya unit bertipe gawat darurat, bila memang hanya ada satu. Tidak ada yang
   perlu ditebak ketika pilihannya tunggal.

Bila keduanya gagal, pendaftaran berhenti dengan pesan yang menyebut cara memperbaikinya.
Ini disengaja: pasien yang terdaftar di unit yang salah tidak akan muncul pada antrean
triage mana pun, dan itu lebih berbahaya daripada berhenti dengan pesan yang jelas.

Pencocokan nama dihapus seluruhnya. Yang tersisa hanya atribut bertipe — `serviceUnitType`,
`isActive`, `isAvailableForRegistration`.

### 2.3 Kegagalan panggilan kedua

Pengulangan yang aman sebenarnya sudah ada: `completedEncounter` disimpan di Redux, sehingga
percobaan berikutnya memakai encounter yang sama dan tidak membuat duplikat. Yang belum ada
adalah **petugas mengetahui hal itu**.

Kegagalan pembuatan kunjungan kini dijawab dengan pesan backend apa adanya, ditambah:

> Data pasien dan encounter sudah tersimpan, jadi jangan mengulang pendaftaran dari awal.
> Tekan simpan sekali lagi untuk melanjutkan dari langkah yang gagal ini.

Tanpa kalimat itu petugas cenderung mengulang dari awal — dan mengulang dari awal adalah
persis yang menghasilkan encounter menggantung.

### 2.4 Tampilan tidak disentuh

Seluruh perubahan pada bagian ini berada di constants, utils, service, dan hook. Tidak ada
satu pun berkas komponen atau CSS pendaftaran yang diubah.

---

## 3. `BE-IGD-003` — Baris Hitam akhirnya dapat dibuat

### 3.1 Keputusan yang diambil

Laporan `BE-IGD-003` menyajikan tiga jalan keluar dan tidak memilih satu pun. Yang dipakai
sekarang adalah **pilihan 1**, yang di laporan itu sendiri disebut "paling kecil dampaknya":
batas dilonggarkan menjadi 0-5, dan Hitam memakai `Level = 0`.

Alasannya: hanya pilihan 1 yang memenuhi acceptance criteria nomor 1 apa adanya, dan angka 0
membaca persis seperti yang dimaksud arsitektur — "di luar skala antrean". Lima nilai skala
antrean tidak bergeser sedikit pun, sehingga data lama tidak berubah arti.

> Ini keputusan desain yang sebelumnya sengaja ditahan. Bila Product/Domain owner lebih
> memilih pilihan 2 (Hitam diwakili jenis tindak lanjut `MENINGGAL`), perubahan ini dapat
> dimundurkan — lihat catatan rollback di 3.3.

### 3.2 Yang diubah

| Berkas | Perubahan |
| --- | --- |
| `MstEmergencyTriageLevel.cs` | `[Range(1,5)]` menjadi `[Range(0,5)]`; const `OutOfQueueScaleLevel = 0` |
| `EmergencyTriageLevelDtos.cs` | `[Range(1,5)]` menjadi `[Range(0,5)]` |
| `MstEmergencyTriageLevelConfiguration.cs` | Check constraint `Level >= 0 AND Level <= 5` |
| `Migrations/…_AllowOutOfQueueScaleTriageLevel.cs` | Drop lalu add check constraint, dengan `Down` yang benar |
| `EmergencyMasterDataSeeder.cs` | Baris `L0` "Meninggal saat tiba", Hitam `#212121`, tanpa target waktu |

Baris Hitam sengaja **tidak** memiliki target waktu respons: pasien yang meninggal saat tiba
tidak sedang menunggu dilayani, sehingga menghitung keterlambatan untuknya tidak punya arti.

`ImmediateCareLevelThreshold` dan `RequireRegistrationBeforeTreatmentFromLevel` tetap
divalidasi 1-5. Keduanya adalah ambang pada skala antrean, dan Hitam memang tidak boleh
menjadi ambang.

### 3.3 Catatan rollback

`Down` mengembalikan batas menjadi 1-5, sehingga **akan gagal bila baris Level 0 sudah ada**.
Hapus baris kategori Hitam lebih dulu sebelum memundurkan migrasi. Catatan ini juga ditulis
di dalam berkas migrasinya.

Migrasi belum diterapkan ke basis data mana pun.

---

## 4. `FE-IGD-003` — Warna kategori sepenuhnya dari master

Berkas konstanta triage menyimpan `TRIAGE_LEVEL_FALLBACK_COLORS` dan
`TRIAGE_LEVEL_TEXT_FALLBACK`: peta warna per nama kategori, dipakai ketika master belum
mengisi `ColorHex`. Cadangan itu efektif menjadi kebijakan kedua yang diam-diam menang, dan
roadmap bagian 4.2 melarangnya.

Keduanya dihapus. Perilaku sekarang:

| Keadaan | Yang tampil |
| --- | --- |
| Master mengisi `ColorHex` | Warna master apa adanya, termasuk bentuk tiga digit |
| Master belum mengisi | Abu-abu netral `#6b7280` |

Abu-abu dipilih justru supaya **tidak** dapat disalahartikan sebagai Merah, Kuning, atau
Hijau. Level yang belum dikonfigurasi tidak boleh tampak punya arti klinis yang belum
pernah ditetapkan rumah sakit.

Warna teks tidak lagi dipetakan per kategori, melainkan **dihitung** dari luminansi relatif
warna latar mengikuti WCAG. Karena dihitung, warna master apa pun yang dipilih rumah sakit
tetap terbaca — termasuk warna yang belum terpikirkan saat kode ini ditulis.

---

## 5. `FE-IGD-004` — Penilaian ulang dan kategori Hitam

### 5.1 Bukti terhadap acceptance criteria

| No | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Level dipilih dari master | **Terpenuhi** | Sudah ada sebelumnya |
| 2 | Tanpa level ditolak beserta alasannya | **Terpenuhi** | Tombol simpan mati; kartu hasil menyebut "Pilih indikator pengkajian untuk mendapatkan hasil prioritas triage." |
| 3 | Riwayat penilaian terlihat saat menilai ulang | **Terpenuhi** | Panel riwayat + dialog menyebut nomor urut penilaian yang akan digantikan |
| 4 | Penilaian yang digantikan tampil sebagai riwayat, tidak dapat diubah | **Terpenuhi** | Layar hanya membuat penilaian baru; backend yang menjadikan yang lama `Superseded` |
| 5 | Hitam tidak dapat dipilih pada skala antrean biasa | **Terpenuhi** | Lihat 5.3; 4 test |
| 6 | Klik simpan dua kali menghasilkan satu penilaian | **Terpenuhi** | Lihat 5.4 |

### 5.2 Penilaian ulang

Berkas baru:

| Berkas | Isi |
| --- | --- |
| `use-emergency-triage-retriage.jsx` | Logika penilaian ulang |
| `emergency-triage-retriage-dialog.jsx` | Dialog pemilihan level baru, alasan, dan catatan |

Ditambah thunk `retriageEmergencyTriage`, state `retriage`, helper `findRetriageableTriage`
dan `buildEmergencyTriageRetriagePayload`, serta tombol "Nilai Ulang Pasien" pada panel
riwayat.

Layar **tidak menyentuh penilaian lama sama sekali**. Yang dikirim hanyalah penilaian baru;
backend yang menjadikan penilaian sebelumnya `Superseded` dan memberi nomor urut berikutnya.
Sifat append-only riwayat klinis hanya aman bila dijaga di satu tempat.

Tombol hanya muncul bila ada penilaian berstatus `Completed`, karena backend menolak 409
untuk status lain. Menyembunyikannya **bukan** pengaman — penolakan 409 tetap ditampilkan apa
adanya bila keadaan berubah di antara pemuatan dan penekanan tombol.

Payload retriage sengaja tidak memuat `emergencyVisitId` maupun `triageStatus`: keduanya
ditentukan backend dari penilaian yang sedang dinilai ulang, dan mengirimnya sendiri hanya
membuka peluang keduanya berbeda dari kenyataan.

### 5.3 Kategori Hitam disaring di satu tempat

`getQueueScaleTriageLevels` menyaring level 0, dan hook formulir memakainya sebagai sumber
`levels`. Karena itu pemilih level, matriks indikator, kartu hasil, dan dialog penilaian
ulang ikut terlindungi tanpa perlu masing-masing mengingatnya.

`resolveRecommendedTriageLevel` juga dijaga: sekalipun ada indikator yang menunjuk ke level
Hitam, rekomendasi tetap kosong. Aplikasi tidak pernah menetapkan kategori Hitam sendiri.

### 5.4 Penjaga klik ganda

`form.handleSubmit(...)` kini dipanggil di dalam event, bukan saat render, dan dijaga satu
ref. Tombol memang sudah dinonaktifkan lewat `saveLoading` dari Redux, tetapi klik kedua yang
terjadi sebelum Redux sempat memperbarui state masih dapat lolos — dan hasilnya dua penilaian
untuk satu pasien.

---

## 6. Harness test akhirnya dapat menguji kode sungguhan

Sebelumnya `node --test` tidak mengenal alias `@/`, import tanpa ekstensi, maupun berkas
`.jsx`. Akibatnya satu-satunya berkas yang dapat diuji adalah berkas yang kebetulan tidak
mengimpor apa pun — dan itulah sebabnya uji pembentukan payload tidak pernah dapat ditulis.

| Berkas | Isi |
| --- | --- |
| `tests/helpers/alias-resolver.mjs` | Resolver alias, ekstensi, dan `.jsx` |
| `tests/helpers/register.mjs` | Pendaftaran hook |

Resolver hanya menerjemahkan alamat modul dan tidak mengubah isi berkas, sehingga yang diuji
tetap kode yang sama persis dengan yang dijalankan aplikasi.

Dua perbaikan yang ikut terbawa:

- `npm run test:unit` memakai glob `tests/unit/*.test.mjs` yang tidak pernah diperluas di
  Windows, sehingga perintahnya selalu berhenti dengan "Could not find". Kini menunjuk
  direktori.
- `tests/unit/auth-security.test.mjs` mengimpor tiga berkas dengan ekstensi `.js` padahal
  aslinya `.jsx`. Kegagalan ini sudah ada sebelum sesi ini dan tidak terkait IGD. Sekarang
  lulus, dan empat test di dalamnya berjalan untuk pertama kalinya.

---

## 7. Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| `dotnet build` backend | **Lulus — 0 error**, 125 warning gaya yang sudah ada sebelumnya |
| ESLint seluruh berkas tersentuh | **Bersih — 0 error, 0 warning baru** |
| `npm run test:unit` | **33 lulus, 0 gagal** (sebelumnya 10 lulus, 1 gagal) |
| `npm run build` | **Lulus** — "Compiled successfully in 30.3s" |
| **Alur simpan dijalankan sungguhan** | **Belum** |
| **Seeder dijalankan pada basis data** | **Belum** |
| **Migrasi diterapkan** | **Belum** |

Perlu ditegaskan: yang terbukti adalah kode terkompilasi, lolos lint, lulus unit test, dan
halaman terbangun. Bahwa data benar-benar tersimpan di basis data **belum dibuktikan**.

### 7.1 Catatan lingkungan

Backend sedang berjalan (PID 10608) dan mengunci `bin/Debug/net9.0/QuilvianSystemBackend.exe`,
sehingga `dotnet build` biasa gagal menyalin apphost. Kompilasi sendiri tidak terpengaruh:
build dijalankan ke direktori keluaran terpisah lewat `BaseOutputPath`, dengan hasil 0 error.
Direktori sementara itu sudah dihapus.

---

## 8. Yang sengaja tidak dikerjakan

| No | Hal | Alasan |
| ---: | --- | --- |
| 1 | Riwayat penyakit dan alergi ke `TrxPatientMedicalHistory` / `TrxPatientAllergy` | Lihat 8.1 |
| 2 | Koreksi data kunjungan lama berstatus `Pending` | Butuh keputusan owner; lihat bagian 1 |
| 3 | Proyek test backend (`BE-IGD-014`) | Di luar scope triage; seluruh `AT-IGD-*` tetap tidak dapat dijalankan |
| 4 | `BE-IGD-010`, `011`, `012` | Terhalang relasi pengguna ke unit pelayanan; butuh `/design-business-module` |

### 8.1 Mengapa riwayat penyakit tetap di `Notes`

Pada review 20 Agustus hal ini tercatat sebagai "belum disambungkan". Setelah ditelusuri,
**datanya tidak hilang**: `buildTriageNotes` sudah menyimpan riwayat penyakit sekarang,
riwayat penyakit dahulu, riwayat pengobatan, riwayat alergi, dan kesimpulan pengkajian ke
`Notes` penilaian triage, masing-masing dengan label.

Yang belum ada adalah penyimpanan **terstruktur**, dan itu sengaja tidak dikerjakan di sini.
`TrxPatientAllergy` mewajibkan `AllergenName` dan menyalakan `IsAlertEnabled` secara bawaan.
Memindahkan teks bebas ke sana berarti perawat yang mengetik "tidak ada" pada kolom alergi
akan membuat baris alergi permanen bernama "tidak ada", lengkap dengan peringatan klinis yang
menyala. Itu lebih berbahaya daripada keadaan sekarang.

Pemetaan field dan aturan pengisiannya adalah keputusan Clinical Management sebagai pemilik
data, bukan keputusan teknis.

---

## 9. Berkas yang diubah

### Backend

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MasterData/Models/MstEmergencyTriageLevel.cs` | Range 0-5, const `OutOfQueueScaleLevel` |
| `Areas/HealthServices/MasterData/DTOs/EmergencyTriageLevelDtos.cs` | Range 0-5 |
| `Repositories/Configurations/…/MstEmergencyTriageLevelConfiguration.cs` | Check constraint |
| `Areas/HealthServices/MasterData/Seeders/EmergencyMasterDataSeeder.cs` | Baris Hitam |
| `Migrations/20260821020839_AllowOutOfQueueScaleTriageLevel.cs` | **Baru** |

### Frontend

| Berkas | Perubahan |
| --- | --- |
| `…/emergency-registration.constants.js` | Peta status registrasi lengkap + label + varian |
| `…/emergency-registration.utils.js` | Resolver status registrasi yang aman |
| `…/emergency-registration.service.js` | Unit IGD dari pengaturan IGD |
| `…/use-emergency-registration.js` | Pesan pemulihan kegagalan panggilan kedua |
| `…/emergency-management-triage-constant.jsx` | Peta warna tebakan dihapus |
| `…/emergency-management-triage-utils.jsx` | Warna dari master, kontras dihitung, helper skala antrean, payload retriage |
| `…/emergency-management-triage-slice.jsx` | Thunk + state `retriage` |
| `…/use-emergency-management-triage-form.jsx` | Skala antrean, penjaga klik ganda |
| `…/use-emergency-triage-retriage.jsx` | **Baru** |
| `…/emergency-triage-retriage-dialog.jsx` | **Baru** |
| `…/emergency-triage-history-panel.jsx` | Tombol dan dialog penilaian ulang |
| `…/emergency-triage.module.css` | Gaya dialog dan aksi judul |
| `tests/helpers/alias-resolver.mjs`, `register.mjs` | **Baru** |
| `tests/unit/emergency-registration-payload.test.mjs` | **Baru** — 6 test |
| `tests/unit/emergency-triage-utils.test.mjs` | **Baru** — 12 test |
| `tests/unit/auth-security.test.mjs` | Perbaikan ekstensi import |
| `package.json` | `test:unit` memakai resolver dan direktori |

---

## 10. Penerapan ke basis data — 21 Agustus 2026

Migrasi dan seeder **sudah dijalankan** pada `QuilvianNewDevTim01 @ 160.22.250.77`, basis data
pengembangan bersama. Bagian 3.3 dan bagian 7 di atas yang menyatakan "belum diterapkan" kini
tidak berlaku lagi.

### 10.1 Migrasi

```
dotnet ef database update
```

Tercatat pada `__EFMigrationsHistory` sebagai `20260821020839_AllowOutOfQueueScaleTriageLevel`.
Check constraint diverifikasi langsung ke katalog PostgreSQL:

```
CK_MstEmergencyTriageLevel_Level | CHECK ((("Level" >= 0) AND ("Level" <= 5)))
```

### 10.2 Cacat seeder yang ditemukan sebelum dijalankan

Pemeriksaan sebelum menjalankan seeder menemukan bahwa **master IGD di basis data ini sudah
terisi sumber lain**, dengan kode yang sama sekali berbeda dari daftar seeder:

| Master | Isi yang sudah ada | Daftar seeder |
| --- | --- | --- |
| Level triase | `ATS-L1` … `ATS-L5` | `L1` … `L5`, `L0` |
| Cara kedatangan | `WALK_IN`, `HOSPITAL_AMBULANCE`, `EXTERNAL_AMBULANCE`, … | `SELF`, `AMBULANCE`, … |
| Indikator | 17 baris berkode `ATS-L*-*` | dibentuk sebagai `TRI-*` |
| Jenis kasus / tindak lanjut | 11 dan 9 baris | daftar berbeda |

Seeder memeriksa idempotensi **hanya lewat `Code`**, padahal `MstEmergencyTriageLevel` juga
punya index unik `(TriageSystem, Level)`. Menjalankannya apa adanya akan:

1. Menyisipkan level 1-5 sekali lagi dengan `TriageSystem = ATS`, menabrak index unik itu.
2. Gagal pada `SaveChangesAsync`, dan karena `SeedEmergencyMasterDataAsync` dipanggil tanpa
   `try`/`catch`, **aplikasi berhenti sebelum sempat melayani permintaan**. Backend
   pengembangan bersama akan mati bagi seluruh tim.
3. Karena seluruh batch gagal, baris Hitam yang sebenarnya sah pun ikut tidak tersimpan.
4. Pada empat master lain tidak ada tabrakan, tetapi hasilnya lebih halus dan lebih buruk:
   dua baris yang artinya sama dengan kode berbeda, misalnya `WALK_IN` dan `SELF`. Laporan
   yang mengelompokkan menurut master itu menjadi salah tanpa ada yang menyadarinya.

Seeder karena itu **tidak dijalankan apa adanya**. Cacatnya diperbaiki lebih dulu.

### 10.3 Perbaikan seeder

| Perubahan | Isi |
| --- | --- |
| Level triase | Idempotensi diperiksa lewat dua kunci: `Code` **dan** `(TriageSystem, Level)`. Slot yang sudah terpakai dilewati, tidak pernah ditimpa |
| Empat master lain | Helper `IsOwnedByAnotherSource` melewati seluruh bagian bila di tabel ada kode yang tidak dikenal daftar seeder |
| `Program.cs` | Seluruh alasan pelewatan ditulis ke log sebagai `Warning`, bukan hanya dua seperti sebelumnya |

Menjalankan seeder dua kali atas datanya sendiri tetap aman: seluruh kode yang ada dikenal,
sehingga tidak ada yang dianggap asing.

### 10.4 Hasil menjalankan seeder

```
Seeder master data IGD selesai. Baris baru: level triase 1, indikator 0,
cara kedatangan 0, jenis kasus 0, jenis tindak lanjut 0, pengaturan 0. Total 1.

Warning  5 level triase dilewati karena slot (sistem triase, level)-nya sudah dipakai baris lain.
Warning  Indikator triase dilewati: sudah diisi sumber lain …
Warning  Cara kedatangan dilewati: sudah diisi sumber lain …
Warning  Jenis kasus dilewati: sudah diisi sumber lain …
Warning  Jenis tindak lanjut dilewati: sudah diisi sumber lain …
Warning  Pengaturan IGD dilewati: Pengaturan IGD sudah ada, tidak ditambah lagi.
```

Tepat **satu baris** ditambahkan, yaitu yang memang hilang:

| Level | Code | Name | ColorName | ColorHex | MaxWaitingMinutes |
| ---: | --- | --- | --- | --- | --- |
| 0 | `L0` | Meninggal saat tiba | Hitam | `#212121` | `NULL` |

Jumlah baris master lain tidak berubah sama sekali: indikator 17, cara kedatangan 6, jenis
kasus 11, jenis tindak lanjut 9, pengaturan 1. `TrxEmergencyTriage` tetap 2 baris — tidak ada
data transaksi yang tersentuh.

Tombol `Seeders:RunEmergencyMasterDataSeed` sudah dikembalikan ke `false`.

### 10.5 Dua temuan yang perlu keputusan pemilik

**Pertama, target waktu level 2-5 sudah terisi dan itu bertentangan dengan keputusan yang
sudah diambil.** Isi master di basis data ini:

| Level | MaxWaitingMinutes |
| ---: | ---: |
| 1 | 0 |
| 2 | 10 |
| 3 | 30 |
| 4 | 60 |
| 5 | 120 |

`IGD-DEC-027` dan `IGD-DEC-035` menyatakan level 2 sampai 5 berstatus `TargetUnconfigured`
sampai SOP triase MMC tersedia — itulah alasan `MaxWaitingMinutes` dibuat nullable oleh
`BE-IGD-002`. Angka-angka di atas berasal dari sumber lain, bukan dari SOP.

Akibatnya nyata: pemantau SLA (`BE-IGD-006`) akan menghitung `ResponseDueAt` untuk keempat
level itu dan menandai pasien terlambat berdasarkan angka yang belum disahkan siapa pun.
Saya **tidak** mengubahnya — sudah ada 2 penilaian triase yang merujuk master ini, dan
mengubah target mengubah perilaku SLA atas data yang sudah berjalan. Owner: Product/Domain
bersama clinical governance.

**Kedua, kode baris Hitam tidak seragam dengan tetangganya.** Baris baru berkode `L0`,
sedangkan lima baris lain berkode `ATS-L1` sampai `ATS-L5`. Ini konsekuensi seeder memakai
daftar kodenya sendiri. Bila tim menginginkan keseragaman, ubah kodenya menjadi `ATS-L0`
lewat master unit — bukan lewat seeder, karena seeder tidak menimpa baris yang sudah ada.

Perlu dicatat juga: master jenis tindak lanjut sudah memuat `DECEASED`. Lihat bagian 11 untuk
alasan mengapa itu **tidak** menggantikan kategori Hitam.

---

## 11. Kategori Hitam: mengapa tetap dibutuhkan, dan jalurnya di layar

### 11.1 `DECEASED` bukan pengganti Hitam

Sempat muncul dugaan bahwa keberadaan jenis tindak lanjut `DECEASED` membuat level Hitam tidak
perlu. Setelah ditelusuri, keduanya mencatat fakta yang berbeda pada waktu yang berbeda:

| | Level triase Hitam | Tindak lanjut `DECEASED` |
| --- | --- | --- |
| Kapan dicatat | Saat triase, ketika pasien tiba | Saat keputusan akhir, di ujung kunjungan |
| Menjawab | Pasien tiba dalam keadaan apa | Bagaimana kunjungan ini berakhir |
| Entitas | `TrxEmergencyTriage.TriageLevelId` | `TrxEmergencyDisposition` |

Dua jalur yang berbeda sama sekali:

- Pasien tiba **sudah meninggal** — ditriase Hitam, lalu tindak lanjutnya `DECEASED`.
- Pasien tiba **hidup** dan ditriase Merah, memburuk saat dirawat, lalu meninggal — triasenya
  tetap Merah, tindak lanjutnya `DECEASED`.

Bila Hitam dihapus dan hanya `DECEASED` yang dipakai, kedua kasus itu menjadi tidak dapat
dibedakan. Laporan "berapa pasien tiba dalam keadaan meninggal" — angka mutu IGD yang lazim
diminta — tidak dapat dihitung lagi. Karena itu baris Hitam dipertahankan.

### 11.2 Celah yang ditemukan pada pekerjaan sendiri

`FE-IGD-004` kriteria 5 berbunyi "Kategori Hitam tidak dapat dipilih **sebagai bagian skala
antrean biasa**". Pada bagian 5.3 di atas, kriteria itu dipenuhi dengan menyaring level 0 dari
`levels` — dan penyaringan itu membuatnya **tidak dapat dipilih sama sekali**.

Itu salah membaca kriterianya. Yang dilarang adalah kategori Hitam muncul sebagai pilihan
keenam sederet dengan L1 sampai L5, dan ditetapkan otomatis oleh aplikasi. Yang dibutuhkan
adalah jalur yang sengaja bagi manusia untuk menyatakannya.

### 11.3 Jalur penetapan

| Berkas | Isi |
| --- | --- |
| `emergency-triage-deceased-section.jsx` | **Baru** — bagian terpisah di bawah formulir |
| `emergency-management-triage-utils.jsx` | `findOutOfQueueScaleTriageLevel` |
| `emergency-management-triage-slice.jsx` | Parameter `overrideLevel` pada thunk simpan |
| `use-emergency-management-triage-form.jsx` | State deklarasi dan alasannya |
| `emergency-triage-form-view.jsx` | Penyambungan; tombol simpan dan kartu hasil menyesuaikan |
| `use-emergency-triage-retriage.jsx`, `emergency-triage-retriage-dialog.jsx` | Kategori Hitam sebagai `optgroup` terpisah |
| `emergency-triage.module.css` | Gaya bagian penetapan |

Rancangannya menahan penetapan yang tidak disengaja:

1. **Terpisah secara fisik.** Bagiannya berada di bawah matriks indikator, berbingkai putus-putus,
   bukan pilihan keenam pada deret L1 sampai L5.
2. **Dua langkah.** Centang pernyataan lebih dulu, baru kolom alasan muncul.
3. **Alasan wajib.** Tombol simpan tetap mati sampai alasannya diisi, dan alasan itu tersimpan
   sebagai sebab penilaian sehingga penetapannya dapat ditinjau ulang.
4. **Indikator diabaikan.** Saat ditetapkan, tidak ada baris detail indikator yang dibuat —
   penetapan ini datang dari pernyataan perawat, bukan dari pencentangan.
5. **Dapat dibatalkan** sebelum disimpan, lewat tombol tersendiri.
6. **Tetap tidak pernah otomatis.** `resolveRecommendedTriageLevel` tetap tidak akan pernah
   mengembalikan kategori Hitam, sekalipun ada indikator yang menunjuk ke sana.

Pada penilaian ulang, kategori Hitam ditaruh di `optgroup` "Di luar skala antrean" — terpisah
dari daftar biasa — dengan alasan yang tetap wajib. Ini menutup kasus pasien yang memburuk
saat dirawat.

### 11.4 Backend tidak perlu diubah

Diperiksa dan terbukti sudah benar:

| Perilaku | Bukti |
| --- | --- |
| Level mana pun yang sah dapat ditetapkan | `EmergencyTriageController.cs` baris 511 hanya memeriksa keberadaan baris |
| Hitam tidak pernah memperoleh batas waktu respons | Baris 227-228 hanya menghitung `ResponseDueAt` bila `MaxWaitingMinutes` terisi, dan Hitam bernilai `NULL` |
| Pemantau SLA tidak pernah menandainya | `BE-IGD-006` kriteria 2: `ResponseDueAt` kosong tidak pernah ditandai |

### 11.5 Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| `npm run test:unit` | **38 lulus, 0 gagal** (5 test baru untuk jalur ini) |
| ESLint | Bersih |
| `npm run build` | Lulus — "Compiled successfully in 34.3s" |
| `dotnet build` | **0 error** |

### 11.6 Satu langkah yang belum dilakukan

Penyeragaman kode `L0` menjadi `ATS-L0` **belum dijalankan** karena penulisan ke basis data
bersama diblokir guardrail lingkungan. Tidak ada yang merujuk baris itu — nol penilaian triase
dan nol indikator — sehingga penggantian kodenya aman:

```sql
UPDATE public."MstEmergencyTriageLevel"
SET "Code" = 'ATS-L0', "UpdateDateTime" = NOW() AT TIME ZONE 'UTC'
WHERE "Code" = 'L0' AND "Level" = 0;
```

Menjalankan seeder lagi sesudahnya tetap aman: slot `(ATS, 0)` sudah terpakai, sehingga
penjaga pada 10.3 melewatinya.
