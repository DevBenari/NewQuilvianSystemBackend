# Laporan Perubahan Backend — `BE-IGD-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-003` |
| Judul | Enam data master IGD terisi sehingga modul dapat dipakai |
| Slice | S0 — Modul benar-benar hidup |
| Roadmap | `docs/module-blueprints/igd/roadmap/backend-roadmap.md` bagian 4 |
| Trace | `02-backend-architecture.md` bagian 7, `IGD-DEC-047`, `IGD-DEC-048`, `AT-IGD-070` |
| Contract version | Tidak ada kontrak API yang berubah |
| Dependency | `BE-IGD-002` — sudah dikerjakan |
| Commit backend saat dikerjakan | `d2682c3ca045d95d293564dd6f4bdad9d6df8f6c` |
| Tanggal | 14 Agustus 2026 |
| Status | **Selesai sebagian.** Lima master terisi penuh, satu master bersyarat, satu baris tidak dapat dibuat karena terhalang aturan basis data |

---

## 1. Masalah yang diperbaiki

Modul IGD sudah dapat dipanggil sejak `BE-IGD-001`, tetapi seluruh daftar pilihannya kosong.
Perawat yang membuka layar triase tidak menemukan satu pun level yang bisa dipilih. Petugas
pendaftaran tidak menemukan cara kedatangan. Dokter tidak menemukan jenis tindak lanjut.

> **Contoh:** perawat menilai pasien kecelakaan pukul 21.30. Ia membuka layar triase, menekan
> daftar pilihan level, dan daftarnya kosong. Penilaian tidak dapat disimpan sama sekali —
> bukan karena salah isi, melainkan karena tidak ada pilihan yang tersedia.

Task ini mengisi data awal keenam master tersebut.

---

## 2. Proses bisnis

### 2.1 Tujuan

Menyediakan data awal yang cukup agar petugas IGD dapat memakai modul sejak hari pertama,
tanpa mengarang aturan klinis yang belum disahkan.

### 2.2 Pelaku

| Pelaku | Kewenangan |
| --- | --- |
| Tim Backend/API | Menjalankan seeder saat lingkungan disiapkan |
| Admin IGD | Menyesuaikan isi master setelah terisi, termasuk menambah dan menonaktifkan |
| Kepala Instalasi Gawat Darurat | Pemilik SOP triase; menetapkan target waktu dan indikator final |
| Manajer Sistem Informasi | Pemilik master unit pelayanan yang dibutuhkan pengaturan IGD |

### 2.3 Pemicu

Aplikasi dinyalakan **dan** tombol seeder dinyalakan pada konfigurasi.

### 2.4 Prasyarat

| Prasyarat | Alasan |
| --- | --- |
| Migration `MakeTriageMaxWaitingMinutesNullable` sudah diterapkan | Tanpa itu, level 2 sampai 5 tidak dapat disimpan dengan target kosong |
| Akun SuperAdmin sudah ada | Dipakai sebagai pencatat pembuat data, mengikuti pola seeder lain |
| Unit pelayanan bertipe Emergency sudah terdaftar | Hanya untuk pengaturan IGD; lihat bagian 4 |

### 2.5 Langkah utama

1. Petugas teknis menyalakan `Seeders:RunEmergencyMasterDataSeed` pada konfigurasi.
2. Aplikasi dijalankan.
3. Seeder memeriksa setiap kode yang akan diisi.
4. Kode yang **belum ada** ditambahkan. Kode yang **sudah ada** dilewati tanpa disentuh.
5. Seeder mencatat berapa baris yang benar-benar ditambahkan pada log.
6. Admin IGD membuka layar master dan menyesuaikan isinya bila perlu.

### 2.6 Aturan bisnis

**Aturan A — Seeder tidak pernah menimpa data yang sudah ada.**

Bila sebuah kode sudah tersimpan, seeder melewatinya sepenuhnya. Ini disengaja dan berbeda
dari beberapa seeder lain di aplikasi ini yang menimpa isi baris.

> **Contoh mengapa:** bulan depan SOP MMC terbit dan Admin IGD mengisi target level 3 menjadi
> 30 menit. Bila kemudian aplikasi dijalankan ulang dan seeder menimpa isinya, target itu
> kembali kosong tanpa ada yang menyadari. Karena itu seeder hanya menambah, tidak menimpa.

**Aturan B — Target waktu level 2 sampai 5 tetap dikosongkan.**

Hanya level 1 yang diberi target, yaitu 0 menit yang berarti dilayani seketika. Sisanya
dikosongkan sampai SOP MMC terbit, sesuai `IGD-DEC-027` dan `IGD-DEC-035`. Baris yang
targetnya kosong diberi keterangan "Target waktu respons belum ditetapkan SOP triase MMC."

**Aturan C — Seeder tidak membuat data milik modul lain.**

Pengaturan IGD wajib menunjuk satu unit pelayanan. Unit pelayanan dimiliki modul Master Data,
bukan IGD. Seeder mencari unit bertipe Emergency yang sudah terdaftar. Bila belum ada, seeder
**tidak membuatnya** dan melewati pengaturan sambil menyebutkan alasannya di log.

**Aturan D — Menjalankan dua kali tidak menghasilkan data ganda.**

Pemeriksaan dilakukan berdasarkan `Code`, yang memang sudah bersifat unik di basis data untuk
kelima master. Untuk pengaturan IGD, pemeriksaannya adalah "sudah ada baris atau belum".

### 2.7 Perubahan status

Tidak ada perubahan status transaksi. Seluruh baris baru dibuat dengan status aktif.

### 2.8 Jalur tidak normal

| Kejadian | Yang terjadi | Yang terlihat |
| --- | --- | --- |
| Seeder dijalankan dua kali | Tidak ada baris ganda | Log mencatat "Baris baru: … Total 0" |
| Belum ada akun SuperAdmin | Seeder berhenti dengan pesan jelas | "Seeder master data IGD membutuhkan akun SuperAdmin." |
| Belum ada unit pelayanan IGD | Lima master tetap terisi, pengaturan dilewati | Peringatan di log: "Belum ada unit pelayanan bertipe Emergency yang aktif…" |
| Pengaturan IGD sudah ada | Tidak ditambah lagi | Peringatan di log: "Pengaturan IGD sudah ada, tidak ditambah lagi." |
| Tombol seeder mati | Tidak terjadi apa-apa | Tidak ada tulisan apa pun di log |

### 2.9 Hasil akhir

Layar IGD memiliki daftar pilihan yang dapat dipakai, dan seluruh isinya dapat disesuaikan
Admin IGD tanpa mengubah kode.

---

## 3. Isi yang di-seed, apa adanya

### 3.1 `MstEmergencyTriageLevel` — 5 baris

Sistem triase: ATS. Warna mengikuti pengelompokan Permenkes 47/2018.

| Level | Code | Nama | Warna | Target waktu | Boleh dilayani sebelum administrasi | Urutan |
| ---: | --- | --- | --- | --- | :---: | ---: |
| 1 | `L1` | Resusitasi | Merah `#E53935` | **0 menit** | Ya | 10 |
| 2 | `L2` | Emergensi | Merah `#E53935` | *kosong* | Ya | 20 |
| 3 | `L3` | Urgen | Kuning `#FDD835` | *kosong* | Tidak | 30 |
| 4 | `L4` | Semi-urgen | Hijau `#43A047` | *kosong* | Tidak | 40 |
| 5 | `L5` | Tidak gawat darurat | Hijau `#43A047` | *kosong* | Tidak | 50 |

Kolom "boleh dilayani sebelum administrasi" disetel Ya untuk level 1 dan 2 agar cocok dengan
nilai bawaan pengaturan IGD, yaitu `ImmediateCareLevelThreshold = 2` dan
`RequireRegistrationBeforeTreatmentFromLevel = 3` yang sudah tertulis pada model.

Nama level adalah label yang dapat diubah Admin IGD. Yang tidak boleh diubah tanpa SOP adalah
angka targetnya.

### 3.2 `MstEmergencyTriageIndicator` — 25 baris

Lima kelompok ABCDE untuk masing-masing dari lima level, sehingga 5 × 5 = 25 baris.

| Kelompok | Nama yang tersimpan | Urutan |
| --- | --- | ---: |
| A | Airway — Penilaian jalan napas | 10 |
| B | Breathing — Penilaian pernapasan | 20 |
| C | Circulation — Penilaian sirkulasi | 30 |
| D | Disability — Penilaian kesadaran dan neurologis | 40 |
| E | Exposure — Penilaian paparan dan pemeriksaan menyeluruh | 50 |

Kode dibentuk `TRI-<kode level>-<huruf>`, misalnya `TRI-L1-A` sampai `TRI-L5-E`. Setiap baris
diberi keterangan "Daftar indikator final untuk level ini menunggu SOP triase MMC."

Isi ini sengaja umum, sesuai peringatan pada roadmap. Indikator yang benar-benar membedakan
level 1 dari level 5 hanya boleh ditetapkan pemilik SOP klinis.

### 3.3 `MstEmergencyArrivalMode` — 5 baris

| Code | Nama | Ambulans | Rujukan | Urutan |
| --- | --- | :---: | :---: | ---: |
| `SELF` | Datang sendiri | — | — | 10 |
| `FAMILY` | Diantar keluarga | — | — | 20 |
| `AMBULANCE` | Ambulans | **Ya** | — | 30 |
| `POLICE` | Diantar polisi | — | — | 40 |
| `REFERRAL` | Rujukan fasilitas kesehatan lain | — | **Ya** | 50 |

Penanda ambulans dan rujukan penting karena keduanya menjadi dasar pelaporan.

### 3.4 `MstEmergencyCaseType` — 8 baris

| Code | Nama | Urutan |
| --- | --- | ---: |
| `TRAUMA` | Trauma | 10 |
| `NON_TRAUMA` | Non-trauma | 20 |
| `KLL` | Kecelakaan lalu lintas | 30 |
| `KERJA` | Kecelakaan kerja | 40 |
| `KRIMINAL` | Kriminalitas | 50 |
| `OBSTETRI` | Obstetri | 60 |
| `RACUN` | Keracunan | 70 |
| `BENCANA` | Bencana | 80 |

### 3.5 `MstEmergencyDispositionType` — 7 baris

| Code | Nama | Perlu unit tujuan | Perlu fasilitas rujukan | Menutup kunjungan | Urutan |
| --- | --- | :---: | :---: | :---: | ---: |
| `PULANG` | Pulang | — | — | Ya | 10 |
| `RANAP` | Rawat inap | **Ya** | — | Ya | 20 |
| `INTENSIF` | Pindah ICU atau kamar operasi | **Ya** | — | Ya | 30 |
| `RUJUK` | Rujuk ke fasilitas kesehatan lain | — | **Ya** | Ya | 40 |
| `MENINGGAL` | Meninggal | — | — | Ya | 50 |
| `TOLAK` | Menolak perawatan | — | — | Ya | 60 |
| `APS` | Pulang atas permintaan sendiri | — | — | Ya | 70 |

> **Contoh cara kolom ini dipakai:** dokter memilih tindak lanjut "Rawat inap". Karena kolom
> "perlu unit tujuan" bernilai Ya, sistem mewajibkan pengisian ruang rawat tujuan sebelum
> keputusan dapat disimpan. Sebaliknya untuk "Pulang", tidak ada tujuan yang perlu diisi.

### 3.6 `MstEmergencySetting` — 1 baris, bersyarat

| Field | Nilai |
| --- | --- |
| `Code` | `DEFAULT` |
| `Name` | Pengaturan IGD Default |
| `DefaultEmergencyServiceUnitId` | Unit pelayanan bertipe Emergency yang aktif, urutan pertama |
| `TriageSystem` | ATS |
| Nilai lainnya | Memakai nilai bawaan yang sudah tertulis pada model |

Baris ini **hanya dibuat bila** sudah ada unit pelayanan bertipe Emergency di master unit
pelayanan. Bila belum ada, seeder melewatinya dan mencatat alasannya.

### 3.7 Ringkasan jumlah

| Master | Baris |
| --- | ---: |
| Level triase | 5 |
| Indikator triase | 25 |
| Cara kedatangan | 5 |
| Jenis kasus | 8 |
| Jenis tindak lanjut | 7 |
| Pengaturan IGD | 1 (bersyarat) |
| **Total** | **51** |

---

## 4. Blocker: baris Hitam tidak dapat dibuat

Acceptance criteria nomor 1 meminta satu baris Hitam di luar skala antrean. **Baris itu tidak
dapat dibuat tanpa mengubah skema basis data**, dan mengubah skema berada di luar scope task
ini yang berbunyi "seeder baru beserta pendaftarannya".

Penyebabnya tiga lapis pembatas yang saling mengunci:

| Lapis | Aturan | Berkas |
| --- | --- | --- |
| Basis data | `CK_MstEmergencyTriageLevel_Level`: `Level >= 1 AND Level <= 5` | `MstEmergencyTriageLevelConfiguration.cs` baris 32-34 |
| Basis data | Index unik `(TriageSystem, Level)` | berkas yang sama, baris 41 |
| Model dan DTO | `[Range(1, 5)]` pada `Level` | `MstEmergencyTriageLevel.cs` baris 17; `EmergencyTriageLevelDtos.cs` baris 28 |

Artinya: `Level` wajib bernilai 1 sampai 5, dan kelima nilai itu sudah dipakai `L1` sampai
`L5` pada sistem triase ATS. Tidak tersisa nilai yang sah untuk baris Hitam.

Tiga jalan keluar yang mungkin, seluruhnya memerlukan keputusan pemilik:

| Pilihan | Yang dibutuhkan | Akibat |
| --- | --- | --- |
| 1. Longgarkan batas menjadi 0 sampai 5, Hitam memakai `Level = 0` | Migration baru + ubah `[Range]` di model dan DTO | Skala tetap lima level; 0 dibaca sebagai "di luar skala". Paling kecil dampaknya |
| 2. Pindahkan Hitam keluar dari master level triase | Keputusan desain baru | Hitam sudah terwakili jenis tindak lanjut `MENINGGAL`, sehingga mungkin memang tidak perlu jadi level |
| 3. Biarkan tidak ada baris Hitam | Tidak ada perubahan kode | Kategori Hitam tidak dapat dicatat sebagai hasil triase |

Saya tidak memilih satu pun, karena ketiganya adalah keputusan klinis dan desain, bukan
keputusan teknis. Pilihan 2 layak dipertimbangkan lebih dulu: arsitektur sendiri menyebut
Hitam "di luar skala antrean" dan "tidak boleh ditetapkan otomatis oleh aplikasi".

---

## 5. File yang diubah

| File | Perubahan |
| --- | --- |
| `Areas/HealthServices/MasterData/Seeders/EmergencyMasterDataSeeder.cs` | **Baru** — seluruh isi seeder |
| `Program.cs` | Menambah `using`, fungsi `SeedEmergencyMasterDataAsync`, dan pemanggilannya di belakang tombol konfigurasi |
| `appsettings.json` | Menambah `Seeders:RunEmergencyMasterDataSeed` bernilai `false` |

`appsettings.json` tidak memuat kredensial apa pun, dan yang ditambahkan hanya satu tombol
bernilai benar/salah. Berkas `appsettings.Development.json` yang memuat kredensial **tidak
disentuh**.

### 5.1 Mengapa tombolnya mati secara bawaan

Seeder ini menulis data. Berkas konfigurasi pengembangan saat ini mengarah ke basis data
pengembangan **bersama**, bukan basis data lokal. Bila tombolnya menyala secara bawaan, siapa
pun yang menjalankan aplikasi akan ikut menulis 51 baris ke basis data bersama tanpa
bermaksud demikian.

Polanya sama dengan `Seeders:RunIcdSeed` yang juga bernilai `false`.

Cara menyalakannya:

```jsonc
"Seeders": {
  "RunEmergencyMasterDataSeed": true
}
```

---

## 6. Verifikasi

### 6.1 Yang sudah dijalankan

| Pemeriksaan | Cara | Hasil |
| --- | --- | --- |
| Build project | `dotnet build` | **Lulus** — 0 galat, 125 peringatan, jumlahnya sama persis dengan sebelum perubahan sehingga berkas baru tidak menambah peringatan |
| Seluruh kolom wajib terisi | Pembacaan keenam model dan konfigurasinya satu per satu | Lulus — tidak ada kolom `[Required]` yang dibiarkan kosong |
| Kode yang dipakai tidak melanggar keunikan | Pembacaan index unik pada keenam konfigurasi | Lulus — `Code` unik pada lima master; indikator juga unik pada `(TriageLevelId, Sequence)`, dan urutan 10-50 per level memenuhinya |
| Batas panjang teks | Nama terpanjang "Rujukan fasilitas kesehatan lain" 32 huruf, batas kolom 150; nama indikator terpanjang 62 huruf, batas 250 | Lulus |
| Pembatas nilai tidak dilanggar | Level 1-5 memenuhi `CK_..._Level`; target 0 dan kosong memenuhi `CK_..._MaxWaitingMinutes` | Lulus |
| Tidak membuat data milik modul lain | Pembacaan ulang seeder | Lulus — unit pelayanan hanya dicari, tidak pernah dibuat |

### 6.2 Acceptance criteria

| No | Kriteria | Status | Catatan |
| ---: | --- | --- | --- |
| 1 | Level 1-5 beserta kelompok warna Merah, Kuning, Hijau, **ditambah satu baris Hitam** | **Sebagian** | Lima level dan ketiga kelompok warna terpenuhi. Baris Hitam terhalang aturan basis data; lihat bagian 4 |
| 2 | Hanya level 1 yang punya target, yaitu 0 menit; level 2-5 dikosongkan | **Terpenuhi di kode, belum diuji berjalan** | Terlihat langsung pada daftar di bagian 3.1 |
| 3 | Lima master lain terisi sesuai daftar isi minimum arsitektur bagian 7.2 | **Terpenuhi di kode, belum diuji berjalan** | Seluruh isi minimum tercakup; lihat bagian 3.3 sampai 3.6 |
| 4 | Menjalankan seeder dua kali tidak menghasilkan data ganda | **Terpenuhi di kode, belum diuji berjalan** | Seeder hanya menambah kode yang belum ada |
| 5 | `MstEmergencySetting` memiliki tepat satu baris default | **Terpenuhi di kode, dengan syarat** | Hanya dibuat bila unit pelayanan IGD sudah terdaftar |

### 6.3 Yang belum dijalankan, beserta alasannya

| Yang belum diuji | Alasan | Cara menutupnya |
| --- | --- | --- |
| Menjalankan seeder sungguhan, termasuk uji dua kali jalan | Belum ada basis data lokal. Menjalankannya pada basis data pengembangan bersama di `160.22.250.77` berarti menulis 51 baris ke lingkungan orang lain, dan itu memerlukan izin eksplisit | Siapkan PostgreSQL lokal, terapkan seluruh migration, nyalakan tombol seeder, jalankan aplikasi dua kali, lalu hitung barisnya |
| `AT-IGD-070` dan `AT-IGD-073` | Belum ada project test, sama seperti dua task sebelumnya | Perlu keputusan pemilik tentang project test |

Perlu ditegaskan: seeder ini belum pernah menyentuh basis data mana pun. Yang terbukti adalah
kodenya benar dan project berhasil dibangun.

---

## 7. Risiko tersisa

| No | Risiko | Akibat nyata bila diabaikan |
| ---: | --- | --- |
| 1 | Baris Hitam belum ada | Pasien yang meninggal saat tiba tidak dapat ditandai lewat level triase. Sementara ini masih dapat dicatat lewat jenis tindak lanjut `MENINGGAL` |
| 2 | Indikator triase masih umum | Perawat melihat lima indikator yang sama untuk semua level, sehingga checklist belum benar-benar membantu membedakan level |
| 3 | Pengaturan IGD bergantung unit pelayanan | Bila unit IGD belum didaftarkan, pendaftaran darurat tidak punya unit tujuan bawaan |
| 4 | Seeder belum pernah dijalankan | Kesalahan yang hanya muncul saat menyentuh basis data, misalnya pelanggaran relasi, belum tentu terlihat |

---

## 8. Bukti penelusuran

| Klaim | Bukti |
| --- | --- |
| Enam master IGD memang kosong dan belum punya seeder | `NewQuilvianSystemBackend` + pencarian `*Seeder*.cs`; sebelum perubahan hanya ada enam seeder dan tidak satu pun untuk IGD + `d2682c3` |
| Baris Hitam terhalang aturan basis data | `Repositories/Configurations/HealthService/MasterData/EmergencyInstallationManagement/MstEmergencyTriageLevelConfiguration.cs` baris 32-34 dan 41 + `d2682c3` |
| Batas level juga ditegakkan di model dan DTO | `Areas/HealthServices/MasterData/Models/MstEmergencyTriageLevel.cs` baris 17; `Areas/HealthServices/MasterData/DTOs/EmergencyTriageLevelDtos.cs` baris 28 + `d2682c3` |
| Pengaturan IGD wajib menunjuk unit pelayanan | `Areas/HealthServices/MasterData/Models/MstEmergencySetting.cs`, `DefaultEmergencyServiceUnitId` bertanda `[Required]` + `d2682c3` |
| Ada tipe unit pelayanan Emergency yang dapat dicari | `Areas/HealthServices/MasterData/Enums/ServiceUnitType.cs` baris 8 + `d2682c3` |
| Pola tombol seeder sudah dipakai sebelumnya | `appsettings.json` baris 47-51 dan `Program.cs` pemanggilan `SeedPrescriptionReviewCriteriaAsync` + `d2682c3` |
