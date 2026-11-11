# ISSUE-044 — Temuan Pengujian Admisi Bayi Baru Lahir: Kegagalan Service Unit, Konflik Registrasi Ibu Tertentu, dan Retensi Form Antar-Pasien

```yaml
issue_id: ISSUE-EPS-044
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Admisi Rawat Inap — Admisi Bayi Baru Lahir, Langkah 1–8 (/health-services/inpatient-management/admissions/newborn)"
sumber_laporan: "Laporan hasil pengujian fungsional pengguna 02-11-2026: 8 butir temuan, gagal terhenti di langkah 4 dari 8"
tanggal_issue: "2026-11-02"
status: TERBUKA
keparahan_tertinggi: Blocker
source_sha_backend: "396c8363 (MHamzah)"
source_sha_frontend: "9c4000724 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-044-temuan-pengujian-admisi-bayi-baru-lahir.md
```

---

## 1. Ringkasan

Dokumen ini menganalisis 8 butir temuan dari pengujian antarmuka alur **Admisi Bayi Baru Lahir** (Langkah 1 hingga 8). Pengujian gagal dan terhenti di Langkah 4 (Penempatan dan Pemesanan Bayi), sehingga Langkah 5 hingga 8 (Persetujuan Wali, Tinjau dan Aktifkan, Dokumen Bayi, dan Kartu Pasien) belum teruji secara end-to-end.

Temuan paling berbahaya berstatus **Blocker**: penolakan pembentukan kunjungan rawat inap bayi (`POST /patient-encounters/admin` HTTP 400) pada saat memesan tempat tidur di kamar Perinatologi (`BAYI-1-A` dan `BAYI-2-A`) karena unit layanan `SU-BABY-001` (Ruang Bayi) berstatus `IsAvailableForRegistration = false` pada Master Data, sementara denah tetap menampilkannya sebagai "Tersedia".

Temuan keparahan **Tinggi** terjadi di Langkah 2: pendaftaran bayi Ny. Dewi Lestari (RM 00-00-00-44) ditolak dengan HTTP 409 (*DbUpdateException* yang disamarkan sebagai konflik konkurensi). Dua temuan keparahan **Sedang** meliputi kontaminasi data formulir lahir saat ibu diganti di Langkah 1–2 serta pesan palsu boks rawat gabung "sudah ditempati bayi" pada boks kamar yang sebenarnya kosong atau tidak aktif. Empat temuan keparahan **Rendah** meliputi tidak segarnya timestamp refresh denah, residu filter pencarian dropdown ibu, ketiadaan batasan waktu masa depan di picker jam lahir, dan salah ketik kata *bounding*.

---

## 2. Laporan Asli Pelapor

Laporan diterima dari sesi pengujian pengguna pada tanggal 2 November 2026. Alur terhenti di langkah 4 dari 8.

| No. Laporan | Tingkat | Lokasi | Masalah (Kata-kata Pelapor) | Lampiran / Detail Terkait |
| :---: | :--- | :--- | :--- | :--- |
| 1 | Blocker | Langkah 4 | Bed BAYI-1-A dan BAYI-2-A tampil “Tersedia”, tetapi “Lanjut ke Pemesanan” selalu ditolak. | Detail: `POST /patient-encounters/admin` 400 "Service unit tidak valid, tidak aktif, atau tidak tersedia untuk registrasi". Unit yang dikirim `SU-BABY-001` (`isAvailableForRegistration: false`). |
| 2 | Tinggi | Langkah 2 | Bayi dari Ny. Dewi Lestari (RM 00-00-00-44) tidak bisa didaftarkan. | Detail: `POST /patients/newborns` 409 "Registrasi bayi berbenturan dengan permintaan lain" (berulang kali, `registeredBabyCount: 0`). |
| 3 | Sedang | Langkah 1–2 | Saat ibu diganti, tanggal lahir, jam lahir, jenis kelamin, dan tempat lahir bayi tetap terisi dari ibu sebelumnya. Bayi bisa tersimpan dengan data lahir ibu lain. | Pengamatan alur saat mengganti kandidat ibu di Langkah 1 lalu lanjut ke Langkah 2. |
| 4 | Sedang | Langkah 4 | Pesan menyebut boks 310-BOKS “sudah ditempati oleh pasien bayi”. Menurut API boksnya Unavailable dan isOccupied: false, dan ibu tersebut belum punya bayi terdaftar saat pesan muncul. | Pengamatan pada mode Rawat Gabung di Langkah 4. |
| 5 | Rendah | Langkah 4 | Setelah ditolak, muncul pesan “Daftar bed sudah diperbarui”, tetapi jam data tetap 11.41.25 dan bed masih “Tersedia”. | Pengamatan antarmuka setelah penolakan service unit. |
| 6 | Rendah | Langkah 1 | Saat dropdown ibu dibuka lagi, kolom cari kosong tetapi daftar masih tersaring kata kunci lama. | Pengamatan pada dropdown `ResourceFilterSelect` ibu persalinan. |
| 7 | Rendah | Langkah 2 | Pilihan jam lahir menampilkan jam yang belum lewat untuk tanggal hari ini. Saya tidak mencoba menyimpannya, jadi validasi di server belum diketahui. | Pengamatan kontrol jam lahir pada tanggal hari ini. |
| 8 | Rendah | Langkah 3 | Salah ketik “bounding”, seharusnya “bonding”. | Teks bullet poin kartu pilihan rencana perawatan rawat gabung. |

---

## 3. Ringkasan Temuan

| ID | No. Laporan | Judul | Jenis | Area | Keparahan | Status Bukti | Perbaikan |
| --- | :---: | --- | --- | --- | :---: | --- | --- |
| `ISS-EPS-044-01` | 1 | Bed Perinatologi tampil "Tersedia" namun ditolak 400 saat pemesanan karena `SU-BABY-001` tidak aktif untuk registrasi | `DATA`, `CONTRACT_MISMATCH` | Backend + Master Data | Blocker | SUDAH-VERIFIKASI | `FIX-EPS-044-01` |
| `ISS-EPS-044-02` | 2 | Pendaftaran bayi Ny. Dewi Lestari gagal 409 (*DbUpdateException* umum dibungkam sebagai konflik) | `BUG`, `DATA` | Backend + Frontend | High | SUDAH-VERIFIKASI | `FIX-EPS-044-02` |
| `ISS-EPS-044-03` | 3 | Formulir identitas kelahiran tidak direset saat berganti ibu di Langkah 1 | `BUG` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-EPS-044-03` |
| `ISS-EPS-044-04` | 4 | Pesan boks rawat gabung mengklaim "sudah ditempati pasien bayi" padahal boks tidak terisi (*false assumption*) | `BUG` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-EPS-044-04` |
| `ISS-EPS-044-05` | 5 | Pesan daftar bed diperbarui tidak menyegarkan denah dan jam data denah tidak bergeser | `BUG` | Frontend | Low | SUDAH-VERIFIKASI | `FIX-EPS-044-05` |
| `ISS-EPS-044-06` | 6 | `FilterSelect` server-side tidak menyinkronkan kata kunci kosong saat menu ditutup-buka kembali | `BUG` | Frontend | Low | SUDAH-VERIFIKASI | `FIX-EPS-044-06` |
| `ISS-EPS-044-07` | 7 | Kontrol jam lahir tidak menonaktifkan jam di masa depan untuk tanggal hari ini (validasi server & klien sudah aman) | `RULE_VIOLATION` (UX) | Frontend | Low | SUDAH-VERIFIKASI | `FIX-EPS-044-07` |
| `ISS-EPS-044-08` | 8 | Saltik kata "bounding" pada kartu rencana rawat gabung | `BUG` | Frontend | Low | SUDAH-VERIFIKASI | `FIX-EPS-044-08` |

---

## 4. Rincian per Temuan

### ISS-EPS-044-01 — Bed Perinatologi tampil "Tersedia" namun ditolak 400 saat pemesanan karena `SU-BABY-001` tidak aktif untuk registrasi

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `DATA`, `CONTRACT_MISMATCH` |
| **Area** | Backend + Master Data |
| **Keparahan** | Blocker |
| **Status bukti** | SUDAH-VERIFIKASI pada source dan konfigurasi DB |
| **Perbaikan** | `FIX-EPS-044-01` |

**Apa yang terjadi.** Petugas memilih bed `BAYI-1-A` atau `BAYI-2-A` yang berlabel hijau "Tersedia" di denah lantai Langkah 4. Saat menekan tombol "Lanjut ke Pemesanan", sistem menolak dengan pesan *"Service unit tidak valid, tidak aktif, atau tidak tersedia untuk registrasi."* Alur admisi terkunci dan tidak dapat berlanjut ke Langkah 5.

**Kenapa terjadi.** 
1. Di backend `PatientEncounterController.cs:2150–2161`, pembuatan kunjungan memvalidasi:
   ```csharp
   var serviceUnitExists = await _dbContext.Set<MstServiceUnit>()
       .AsNoTracking()
       .AnyAsync(x => x.Id == request.ServiceUnitId && x.IsActive && !x.IsDelete && x.IsAvailableForRegistration);
   if (!serviceUnitExists)
       return (false, "Service unit tidak valid, tidak aktif, atau tidak tersedia untuk registrasi.");
   ```
   Pada unit `SU-BABY-001` (Ruang Bayi), bendera `IsAvailableForRegistration` bernilai `false` di database pengujian.
2. Di sisi kueri denah `InpFloorPlanQueryService.cs:117–125`, kamar dan bed dimuat hanya berdasarkan kriteria `!x.IsDelete && x.IsActive && units.Contains(x.ServiceUnitId)`. Denah tidak mengevaluasi apakah service unit pemilik kamar memenuhi `IsAvailableForRegistration`. Akibatnya, bed ditampilkan sebagai "Tersedia", padahal endpoint registrasi pasti menolaknya.

**Apakah ini menyimpang dari desain?** `BUG` dan `DATA`. Desain mengharuskan tempat tidur yang ditampilkan pada denah admisi dapat dipesan tanpa penolakan validasi statis unit layanan.

**Dampak nyata.** Alur admisi rawat inap untuk seluruh bayi di ruang Perinatologi terhenti total (Blocker). Petugas admisi tidak dapat membuat episode dan tidak dapat memproses bayi masuk ke bangsal.

**Rekomendasi.** 
1. Jalankan aktivasi `IsAvailableForRegistration = true` pada unit `SU-BABY-001` (dan `SU-NICU-001`) di Master Data / database pengujian.
2. Di backend `InpFloorPlanQueryService.cs`, tambahkan filter ketersediaan unit registrasi untuk `purpose=Admission` agar bed di bawah unit yang terkunci registrasinya tidak berstatus "Available".

---

### ISS-EPS-044-02 — Pendaftaran bayi Ny. Dewi Lestari gagal 409 (*DbUpdateException* umum dibungkam sebagai konflik)

| | |
| --- | --- |
| **No. laporan** | 2 |
| **Jenis** | `BUG`, `DATA` |
| **Area** | Backend + Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI pada source backend |
| **Perbaikan** | `FIX-EPS-044-02` |

**Apa yang terjadi.** Saat mendaftarkan bayi untuk ibu Ny. Dewi Lestari (RM 00-00-00-44), penekanan tombol di Langkah 2 gagal dengan kode HTTP 409 dan pesan *"Registrasi bayi berbenturan dengan permintaan lain. Muat ulang lalu periksa hasil registrasi."* Tidak ada data pasien atau encounter yang terbentuk. Percobaan kedua dengan data yang sama tetap gagal 409.

**Kenapa terjadi.**
1. Pada `NewbornPatientRegistrationService.cs:319–325`:
   ```csharp
   catch (DbUpdateException)
   {
       await transaction.RollbackAsync(cancellationToken);
       return new NewbornRegistrationResult(
           NewbornRegistrationStatus.Conflict,
           "Registrasi bayi berbenturan dengan permintaan lain. Muat ulang lalu periksa hasil registrasi.");
   }
   ```
   Blok `catch (DbUpdateException)` menangkap *seluruh* galat basis data saat `SaveChangesAsync()` dan membungkamnya menjadi pesan konflik konkurensi (HTTP 409).
2. Di database, tabel `MstPatient` memiliki indeks unik komposit `UX_MstPatient_DeliveryCase_BirthOrder` dengan filter `"DeliveryCaseId" IS NOT NULL AND "BirthOrder" IS NOT NULL AND "IsDelete" = false`. Jika kasus persalinan Ny. Dewi Lestari memiliki record bayi lama yang non-aktif (`IsActive = false`, `IsDelete = false`), kueri kandidat ibu (`InpEpisodeService.NewbornCandidates.cs:153`) menyaringnya keluar sehingga `registeredBabyCount` bernilai `0`. Akibatnya, `NewbornPatientRegistrationService` mencoba mengalokasikan `birthOrder = 1`, memicu benturan indeks unik fisik PostgreSQL.
3. Di frontend `useInpatientAdmissionPatient.jsx:338–344`, nilai `newbornIdempotencyKey.current` tidak dibersihkan saat request gagal. Pada percobaan kedua, request dikirim ulang menggunakan kunci idempotensi yang sama tanpa regenerasi UUID.

**Apakah ini menyimpang dari desain?** `BUG`. Galat database internal seharusnya dicatat detail di log dan ditangani secara tepat, bukan dilaporkan sebagai konflik konkurensi yang membingungkan petugas.

**Dampak nyata.** Pasien ibu tertentu tidak bisa mendaftarkan bayinya sama sekali, dan pengembang/petugas tidak mengetahui penyebab pasti kegagalan di basis data.

**Rekomendasi.**
1. Di backend `NewbornPatientRegistrationService.cs`: Tangkap rincian `PostgresException` (nama constraint unik / foreign key) dan jangan menyamarkan seluruh `DbUpdateException` sebagai konkurensi.
2. Periksa kueri alokasi nomor urut agar memperhitungkan seluruh baris `MstPatient` yang `!IsDelete` (selaras dengan indeks fisik database).
3. Di frontend: Reset `newbornIdempotencyKey.current` ketika request pendaftaran bayi gagal.

---

### ISS-EPS-044-03 — Formulir identitas kelahiran tidak direset saat berganti ibu di Langkah 1

| | |
| --- | --- |
| **No. laporan** | 3 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI pada source frontend |
| **Perbaikan** | `FIX-EPS-044-03` |

**Apa yang terjadi.** Petugas memilih seorang ibu di Langkah 1, mengisi tanggal lahir, jam lahir, jenis kelamin, dan tempat lahir di Langkah 2. Jika petugas kembali ke Langkah 1 dan mengganti pilihan ibu, isian tanggal lahir, jam lahir, jenis kelamin, dan tempat lahir tetap terisi dari bayi sebelumnya.

**Kenapa terjadi.** Di `use-inpatient-newborn-admission.js:232–252` fungsi `selectMother`:
```javascript
patient.form.setValue("motherPatientId", candidate?.motherPatientId || "");
patient.form.setValue("deliveryCaseId", candidate?.deliveryCaseId || "");
patient.form.setValue("deliveryCaseRequestedAt", candidate?.requestedAt || "");
patient.form.setValue("fullName", "");
patient.form.clearErrors(["motherPatientId", "deliveryCaseId"]);
```
Fungsi hanya mengosongkan `fullName`, namun membiarkan field `birthDate`, `birthTime`, `gender`, `birthPlace`, `birthWeightGram`, dan `birthLengthCm` tetap berisi nilai lama.

**Apakah ini menyimpang dari desain?** `BUG`. Berganti ibu persalinan adalah pergantian entitas kasus klinis yang berbeda; data kelahiran anak terdahulu tidak boleh bocor ke bayi berikutnya.

**Dampak nyata.** Risiko keselamatan rekam medis: bayi Ny. B dapat tersimpan dengan tanggal, jam, jenis kelamin, dan tempat lahir milik bayi Ny. A tanpa disadari petugas.

**Rekomendasi.** Pada fungsi `selectMother`, panggil reset komprehensif pada formulir identitas bayi (`birthDate`, `birthTime`, `gender`, `birthPlace`, `identityNumber`, dll.) dan reset `newbornIdempotencyKey.current`.

---

### ISS-EPS-044-04 — Pesan boks rawat gabung mengklaim "sudah ditempati pasien bayi" padahal boks tidak terisi (*false assumption*)

| | |
| --- | --- |
| **No. laporan** | 4 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI pada source frontend |
| **Perbaikan** | `FIX-EPS-044-04` |

**Apa yang terjadi.** Pada mode Rawat Gabung di Langkah 4, layar menampilkan peringatan bahwa boks 310-BOKS *"sudah ditempati oleh pasien bayi. Untuk bayi kembar berikutnya, silakan alihkan jenis perawatan..."* Padahal menurut API, boks tersebut berstatus `Unavailable` dengan `isOccupied: false`, dan ibu tersebut belum memiliki bayi terdaftar.

**Kenapa terjadi.** Di `inpatient-newborn-admission-utils.js:262–263` fungsi `pickRoomingInCrib`:
```javascript
if (availableCribs.length) return { status: "available", cribs, availableCribs, crib };
return { status: cribs.length ? "occupied" : "no-crib", cribs, availableCribs, crib: null };
```
Jika boks ada di kamar (`cribs.length > 0`) tetapi tidak lolos ke `availableBeds` (misalnya karena `Unavailable`, `Cleaning`, atau `UnderMaintenance`), fungsi langsung mengembalikan status `"occupied"`. Komponen `inpatient-newborn-admission-steps.jsx:481–487` kemudian menampilkan teks baku yang menuduh boks sudah terisi bayi kembar sebelumnya.

**Apakah ini menyimpang dari desain?** `BUG`. Antarmuka membuat asumsi salah atas ketiadaan ketersediaan bed.

**Dampak nyata.** Petugas admisi bingung karena sistem menuduh ada bayi lain yang menempati boks di kamar ibu, memicu kepanikan atau salah sangka tertukarnya pasien bayi.

**Rekomendasi.** Periksa status riil boks (`isOccupied`, `maintenance`, `cleaning`, `unavailable`). Tampilkan pesan netral sesuai status sebenarnya, misalnya: *"Boks bayi di kamar ibu saat ini sedang tidak tersedia untuk penempatan (Status: Tidak Tersedia / Dalam Pemeliharaan)."*

---

### ISS-EPS-044-05 — Pesan daftar bed diperbarui tidak menyegarkan denah dan jam data denah tidak bergeser

| | |
| --- | --- |
| **No. laporan** | 5 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI pada source frontend |
| **Perbaikan** | `FIX-EPS-044-05` |

**Apa yang terjadi.** Saat penolakan service unit terjadi, muncul banner pesan *"Unit layanan untuk bed ini baru saja tidak tersedia untuk registrasi rawat inap. Daftar bed sudah diperbarui; silakan pilih bed lain."* Namun jam data denah tetap menunjukkan waktu lama (11.41.25) dan bed yang bermasalah tetap berstatus "Tersedia".

**Kenapa terjadi.** Di `use-inpatient-admission-bed.jsx:971–973`:
```javascript
if (isServiceUnitRegistrationFailure(message)) {
  message = FRIENDLY_SERVICE_UNIT_MESSAGE;
  clearSelection();
  await legacyBoard.refresh();
}
```
Penyegaran hanya memanggil `legacyBoard.refresh()`. Pada mode tampilan denah (`floor-plan`), fungsi pemutakhiran denah (`floorPlan.reload()`) tidak dipanggil, sehingga data dan stempel waktu denah tidak berubah.

**Apakah ini menyimpang dari desain?** `BUG`. Pesan menyatakan daftar sudah diperbarui, tetapi tampilan denah tidak ikut disegarkan.

**Dampak nyata.** Petugas mengira sistem sudah memutakhirkan denah, lalu mengklik kembali bed yang sama dan mendapatkan error yang berulang.

**Rekomendasi.** Di `use-inpatient-admission-bed.jsx`, panggil `floorPlan.retryFloorPlanPreparation?.()` atau trigger refetch denah aktif saat kegagalan service unit terjadi.

---

### ISS-EPS-044-06 — `FilterSelect` server-side tidak menyinkronkan kata kunci kosong saat menu ditutup-buka kembali

| | |
| --- | --- |
| **No. laporan** | 6 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI pada source frontend |
| **Perbaikan** | `FIX-EPS-044-06` |

**Apa yang terjadi.** Di Langkah 1, petugas mencari ibu dengan mengetik kata kunci. Dropdown ditutup lalu dibuka kembali: kolom input pencarian terlihat kosong, namun opsi di dalam daftar masih tersaring sesuai kata kunci pencarian sebelumnya.

**Kenapa terjadi.** Di `filter-select.jsx:347–360`:
```javascript
const resetTransientState = useCallback(() => {
  setSearchKeyword("");
  ...
}, []);

const handleClose = useCallback(() => {
  setIsOpen(false);
  setFloatingLayout(null);
  resetTransientState();
}, [resetTransientState]);
```
Saat menu ditutup, `setSearchKeyword("")` hanya membersihkan state input teks lokal komponen `FilterSelect`. Komponen tidak memanggil callback `onSearchChange("")` ke hook luar (`use-inpatient-newborn-admission.js`). State `motherSearch` di luar tetap menyimpan kata kunci lama.

**Apakah ini menyimpang dari desain?** `BUG`. Ada desinkronisasi antara teks input visual (kosong) dengan data daftar pilihan (tersaring).

**Dampak nyata.** Petugas melihat kolom cari kosong tetapi opsi yang dicari tidak muncul semua, sehingga mengira pasien ibu lain tidak ada di sistem.

**Rekomendasi.** Pada `FilterSelect.jsx`, saat `resetTransientState` dijalankan pada mode `serverSide`, panggil `onSearchChangeRef.current?.("")` agar state pencarian induk tersinkronisasi menjadi kosong.

---

### ISS-EPS-044-07 — Kontrol jam lahir tidak menonaktifkan jam di masa depan untuk tanggal hari ini

| | |
| --- | --- |
| **No. laporan** | 7 |
| **Jenis** | `RULE_VIOLATION` (UX) |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI pada source frontend dan backend |
| **Perbaikan** | `FIX-EPS-044-07` |

**Apa yang terjadi.** Pada formulir Langkah 2, pilihan jam lahir pada kontrol antarmuka mengizinkan pemilihan jam yang belum lewat (misal jam 20.00 padahal waktu saat ini jam 13.00) ketika tanggal lahir adalah hari ini.

**Kenapa terjadi.** Kontrol `BaseTimeField` merupakan pemilih jam standar yang belum diikat dengan pembatas batas atas dinamis (`maxTime`) berdasarkan waktu lokal rumah sakit hari ini.

**Apakah ini menyimpang dari desain?** `RULE_VIOLATION` standar UX Quilvian. Namun secara integritas data, sistem aman:
1. Validasi klien di `inpatient-newborn-admission-utils.js:188–193` menolak waktu di masa depan saat tombol simpan ditekan:
   `errors.birthTime = "Waktu lahir belum terjadi. Sekarang [tanggal] pukul [jam] WIB."`
2. Validasi server di `NewbornPatientRegistrationService.cs:107–113` menolak dengan toleransi jam 5 menit:
   `if (birthMoment > hospitalNow + BirthClockTolerance) return Invalid("Waktu lahir belum terjadi...");`

**Dampak nyata.** Petugas baru mengetahui jam lahir tidak valid setelah menekan tombol Lanjut, bukan dicegah sejak awal saat memilih di picker.

**Rekomendasi.** Berikan prop batasan jam maksimal pada komponen picker waktu ketika tanggal yang dipilih bernilai hari ini.

---

### ISS-EPS-044-08 — Saltik kata "bounding" pada kartu rencana rawat gabung

| | |
| --- | --- |
| **No. laporan** | 8 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI pada source frontend |
| **Perbaikan** | `FIX-EPS-044-08` |

**Apa yang terjadi.** Pada kartu pilihan rencana perawatan "Rawat Gabung" di Langkah 3, bullet poin kedua tertulis *"Mendukung bounding ibu & bayi"*.

**Kenapa terjadi.** Saltik pengetikan pada berkas konstanta `src/lib/constants/health-services/inpatient-management/inpatient-newborn-admission-constants.js:25`:
```javascript
bullets: ["Di kamar ibu yang memiliki boks bayi", "Mendukung bounding ibu & bayi", "Non-intensif"],
```

**Apakah ini menyimpang dari desain?** `BUG`. Istilah medis/psikologis yang baku adalah *bonding* (ikatan kasih sayang/kelekatan ibu dan anak), bukan *bounding* (melompat/membatasi).

**Dampak nyata.** Penulisan tidak baku mengurangi kualitas profesionalitas antarmuka medis rumah sakit.

**Rekomendasi.** Ganti kata `"Mendukung bounding ibu & bayi"` menjadi `"Mendukung bonding ibu & bayi"`.

---

## 5. Tanya-Jawab Pelapor

Setiap pertanyaan eksplisit pelapor dijawab secara langsung:

> **T:** *Untuk melanjutkan langkah 5–8, SU-BABY-001 perlu diaktifkan untuk registrasi. Apakah saya boleh mengubahnya di Master Data, atau tim Anda yang akan mengubahnya?*
>
> **J:** **Penguji/Admin sangat boleh dan berwenang mengubahnya langsung di menu Master Data.** Penguji dapat membuka menu Master Data Unit Layanan (`/administrator/master-data/service-units`), mencari unit `SU-BABY-001` (Ruang Bayi), mengedit data, mencentang bendera **"Tersedia untuk Registrasi"** (`IsAvailableForRegistration = true`), lalu klik Simpan. Ini adalah jalur tata kelola standar rumah sakit. Jika penguji tidak memiliki akses ke menu tersebut, tim pengembang/DB Ops dapat mengeksekusinya via basis data dev sesuai prosedur persetujuan.

> **T:** *Setelah itu saya lanjutkan dengan bayi uji yang sama (By. Ny. Putri Ayu, RM 00-00-00-48)?*
>
> **J:** **Ya, lanjutkan dengan bayi uji yang sama.** Pasien bayi tersebut saat ini tersimpan di sistem dengan status *pending admission* (memiliki No. RM tetapi belum memiliki encounter/episode). Penguji cukup membuka Langkah 1, memilih Ny. Putri Ayu, lalu pada kartu riwayat bayi klik tombol **"Lanjutkan admisi bayi ini"**. Alur akan langsung membawa penguji ke Langkah 4 tanpa membentuk No. RM baru yang ganda, dan dapat menyelesaikan pengujian Langkah 5 hingga 8.

---

## 6. Temuan Tambahan

| | |
| --- | --- |
| **ID** | `ISS-EPS-044-T1` |
| **Judul** | Ketiadaan Log Rinci Postgres pada `NewbornPatientRegistrationService` |
| **Jenis** | `RULE_VIOLATION` (Observabilitas Backend) |
| **Area** | Backend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI pada `NewbornPatientRegistrationService.cs:319` |
| **Kenapa dicantumkan** | Akar masalah mengapa temuan nomor 2 sulit dianalisis dari luar: blok `catch (DbUpdateException)` menelan pesan asli tanpa `_logger.LogError(ex, ...)` dan tanpa membaca `ex.InnerException`. |

---

## 7. Pertanyaan dan Keputusan yang Dibutuhkan

### Untuk Penguji / Pelapor

| No | Pertanyaan | Kenapa Ditanyakan | Dampak bila Belum Dijawab |
| :---: | --- | --- | --- |
| 1 | Apakah penguji memiliki hak akses untuk mengedit Master Data Unit Layanan di antarmuka? | Untuk menentukan apakah aktivasi `SU-BABY-001` dilakukan mandiri oleh penguji atau lewat script DB Ops. | Tidak memblokir, penguji dapat langsung mencoba membuka menu Master Data Unit Layanan. |
| 2 | Apakah saat pengujian Ny. Dewi Lestari terdapat pesan error lain di console atau respons network selain 409? | Memastikan apakah ada payload DTO yang bernilai null (misal relasi wilayah). | Analisis tetap valid berdasar kode sumber `catch (DbUpdateException)`. |

### Untuk Pemilik Sistem / Arsitek Domain

| No | Keputusan yang Dibutuhkan | Rekomendasi | Dampak Keputusan |
| :---: | --- | --- | --- |
| 1 | Penanganan Service Unit Khusus (Ruang Bayi, NICU, ICU): apakah seluruh unit rawat inap wajib selalu `IsAvailableForRegistration = true` pada data seed awal? | **Ya, setujui.** Seluruh unit rawat inap fungsional wajib aktif untuk registrasi rawat inap agar admisi tidak tertolak di tengah jalan. | Mencegah bug kambuhan di environment QA/Staging baru. |

---

## 8. Catatan Pola

1. **Pola Berulang ISSUE-032**: Masalah `IsAvailableForRegistration = false` pada `SU-BABY-001` pernah terjadi pada 22 Oktober 2026 (ISSUE-032) dan sudah ditutup di DB Hamzah, namun muncul kembali di DB yang digunakan penguji saat ini. Diperlukan skrip seed/migrasi persisten agar tidak berulang saat seeding ulang.
2. **Pola False Assumption pada UI Boks**: UI cenderung menyimpulkan kegagalan ketersediaan sebagai "sudah terisi bayi" (`status: cribs.length ? "occupied" : "no-crib"`), mirip dengan issue-issue copy/teks terdahulu di modul admisi.

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :--- | :--- | :--- |
| 2026-11-02 | Dokumen issue dibuat berdasarkan laporan hasil pengujian fungsional admisi bayi (8 butir temuan). | AI Agent (`diagnose-module-issue`) |

