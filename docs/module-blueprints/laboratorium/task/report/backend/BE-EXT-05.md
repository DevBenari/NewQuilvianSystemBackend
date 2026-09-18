# Laporan Perubahan Backend — `BE-EXT-05`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-EXT-05` |
| Judul | [Registrasi] Kunjungan dari kiosk dan penutupan otomatisnya |
| Slice | `EPIC-LAB-11`, gelombang `MVP-5b` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6c |
| Trace | `LAB-DEC-053`, `LAB-DEC-054`, `LAB-DEC-058`, `LAB-DEC-059`; `BR-46` butir 3 dan 4 |
| Kontrak | Dikontrakkan di sisi `registration-management` |
| Dependency | `BE-EXT-04` ✅, `BE-EXT-04b` ✅ |
| Klasifikasi | `HIGH` — bukan karena rumit, melainkan karena kesalahannya tidak muncul sebagai galat |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source `RegistrationManagement` dan `LaboratoryManagement`, artefak blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `13665452`, branch `yoga` |
| Tanggal | 2026-09-17 |
| Wewenang lintas modul | Pemilik modul `registration-management` — **Andry Zain**, disampaikan pemilik modul Laboratorium 2026-09-17, pilihan **A** pada `LAB-REQ-012` bagian 3.3 |
| Status | **✅ `SELESAI`** — seluruh butir DoD terpenuhi. Empat cabang dibuktikan terhadap `QuilvianNewDevYoga` lewat aplikasi yang benar-benar berjalan; **satu cacat ditemukan uji itu dan diperbaiki**; nol baris uji tertinggal |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `RegistrationManagement / Registration`, dengan satu berkas di `LaboratoryManagement` |
| Pemilik dan prefix registry | Prefix **`Reg`**, lifecycle **`ACTIVE / LEGACY`** |
| Keberlakuan | `NEW` untuk empat berkas baru; `TOUCHED LEGACY` untuk `Program.cs` dan `appsettings.json` |
| Status gerbang | Tidak menahan — **nol entity baru, nol kolom baru, nol migration** |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-CODE-002` |

---

## 1. Temuan yang membalik dasar task ini

**Kedua penahan `BE-EXT-05` ternyata tidak seperti yang tercatat.** Keduanya diperiksa ulang
terhadap source dan database sebelum satu baris pun ditulis, dan keduanya **terbantah**.

### 1.1 `LAB-OPEN-025` menyebut jalur yang salah

Catatan lama berbunyi: *"Kiosk berjalan di bawah `KioskReadPolicy`; `EncounterIntakeService.RegisterAsync`
menuntut `PatientEncounter:Create`."* Kedua pernyataannya benar — tetapi **keduanya tidak
berbicara tentang jalur yang sama**.

| Jalur | Siapa yang memakainya | Penjaganya |
|---|---|---|
| `EncounterIntakeService.RegisterAsync` | Layar pendaftaran pasien lab (`BE-LAB-08`, `INT-05`) | `PatientEncounter:Create` — `EncounterIntakeService.cs:160` |
| `POST /patient-encounters/kiosk` | **Kiosk** | **Hanya** `[Authorize(Policy = KioskReadPolicy)]` — `PatientEncounterController.cs:406-417` |

Route kiosk itu **sudah ada**, dan sudah mengerjakan hampir seluruh butir 3 `BR-46`:

| Yang sudah dikerjakannya | Baris |
|---|---|
| Menerima `KioskScanSessionId` dan menyimpannya pada kunjungan | `PatientEncounterController.cs:579` |
| Menandai `IsFromKiosk` | `:604` |
| Membentuk nomor antrean | `:638` |
| Menandai sesi kiosk terpakai (`IsUsedForRegistration = true`) | `:686` |
| Menolak Penjamin Perusahaan bagi kiosk | `allowCompanyGuarantor: false`, `:415` |

Ia **tidak** memikul `[AccessPermission("PatientEncounter", "Create")]`, tidak seperti
`POST /admin` di atasnya. Diperiksa bahwa ketiadaan itu memang berarti tidak ada pemeriksaan
izin: `AccessPermissionAttribute` adalah `TypeFilterAttribute` yang menjalankan
`AccessPermissionFilter`, sedangkan `AccessActionAttribute` — yang memang dipikul route kiosk —
`Attribute` biasa tanpa filter, murni metadata katalog peran. `Program.cs` juga **nol**
mendaftarkan filter global (`options.Filters.Add` nol kemunculan).

> **Artinya pilihan A yang disetujui Andry sudah berdiri di source sejak sebelum task ini.**
> Bukan sebagian: jalur pembentukan kunjungan tersendiri bagi kiosk, berwewenang sempit,
> dan `POST /admin` yang menerima Penjamin Perusahaan tidak ikut terbuka — persis rumusan
> pilihan A pada `LAB-REQ-012` bagian 3.3.

### 1.2 `LAB-OPEN-026` menyebut kolom yang tidak dibaca jalur itu

Catatan lama menyatakan kunjungan lab dari kiosk *"tidak punya unit layanan yang sah"* selama
`SU-LAB-001.IsAvailableForKiosk = false`.

Penelusuran seluruh source: **`IsAvailableForKiosk` nol disebut oleh `PatientEncounterController`
maupun `EncounterIntakeService`.** Ia hanya hidup pada CRUD dan penyaring daftar milik
`master-data` (`ServiceUnitController`, `ClinicController`). Yang benar-benar dituntut jalur
pembentukan kunjungan adalah **`IsAvailableForRegistration`** —
`PatientEncounterController.cs:1336-1345`.

Dibaca dari `QuilvianNewDevYoga` pada 2026-09-17:

| `ServiceUnitCode` | `IsAvailableForRegistration` | `IsAvailableForKiosk` | `IsQueueRequired` | `IsDoctorRequired` | `IsScreeningRequired` |
|---|:---:|:---:|:---:|:---:|:---:|
| `SU-LAB-001` | **`true`** | `false` | `true` | `false` | `false` |

**Kunjungan laboratorium dari kiosk karena itu sah hari ini juga.** `IsAvailableForKiosk` tetap
berpengaruh pada satu hal — apakah unit ini ikut tampil pada **daftar pilihan** yang disaring
`?isAvailableForKiosk=true` (`ServiceUnitController.cs:780`) — dan itu perkara layar kiosk milik
`registration-management`, bukan penahan pembentukan kunjungan. Lihat bagian 6.2.

### 1.3 Yang benar-benar belum ada

Sesudah kedua penahan terbantah, sisa `BE-EXT-05` mengecil menjadi **satu hal**: butir 4 `BR-46`
— **penutupan otomatis kunjungan kiosk yang tidak dilanjutkan pada akhir hari layanan.** Nol
baris yang mengerjakannya; nol `BackgroundService` di seluruh repository yang menyentuh
kunjungan.

---

## 2. Perubahan yang dikerjakan

Empat berkas baru, dua berkas disunting. **Nol migration, nol kolom baru, nol endpoint baru.**

| # | Berkas | Sifat | Isi |
|---:|---|---|---|
| 1 | `RegistrationManagement/Services/IEncounterContinuationProbe.cs` | **baru** | Kontrak yang ditetapkan Registrasi: unit layanan menjawab kunjungan mana yang benar-benar dilanjutkan |
| 2 | `RegistrationManagement/Services/KioskEncounterClosureOptions.cs` | **baru** | `LAB-DEC-059` sebagai konfigurasi — pukul 21:00 WIB, dapat diubah tanpa rilis ulang |
| 3 | `RegistrationManagement/Services/KioskEncounterClosureService.cs` | **baru** | Penutupannya sendiri |
| 4 | `RegistrationManagement/Services/KioskEncounterClosureHostedService.cs` | **baru** | Penjadwal harian |
| 5 | `LaboratoryManagement/Services/LabEncounterContinuationProbe.cs` | **baru** | Jawaban Laboratorium — "dilanjutkan" berarti ada `LabOrder` atas kunjungan itu |
| 6 | `Program.cs` | sisipan | Empat pendaftaran DI, satu `Configure`, satu `AddHostedService` |
| 7 | `appsettings.json` | sisipan | Bagian `HealthServices:KioskEncounterClosure` |

### 2.1 Kenapa ada antarmuka, bukan pembacaan langsung

Penutupan otomatis adalah wewenang **Registrasi** (`BR-46`: *"Kunjungan dari kiosk yang tidak
pernah dilanjutkan ditutup otomatis oleh Registrasi"*), tetapi **apa artinya "dilanjutkan" hanya
diketahui unit yang melayani.** Bagi Laboratorium artinya ada pesanan pemeriksaan yang terbit.

Membaca `LabOrder` langsung dari Registrasi akan **membalik arah ketergantungan antar modul.**
Diperiksa dan diukur sebelum diputuskan:

| Arah | Sebelum task ini | Sesudah task ini |
|---|---:|---:|
| `LaboratoryManagement` menyebut `RegistrationManagement` | 6 berkas | 7 berkas |
| `RegistrationManagement` menyebut `LaboratoryManagement` **dalam kode** | **0** | **0** |
| `RegistrationManagement` menyebut tipe `LabOrder` | **0** | **0** |

Antarmuka menjaga arah itu tetap satu jalan: Registrasi menetapkan pertanyaannya, unit layanan
mendaftarkan penjawabnya.

> **Diukur, bukan diklaim, dan angkanya perlu dibaca tepat.** Penelusuran `LaboratoryManagement`
> pada seluruh `RegistrationManagement` menghasilkan **satu** kemunculan — dan ia berada **di
> dalam komentar XML** `IEncounterContinuationProbe.cs` yang menjelaskan justru mengapa arahnya
> dijaga. Nol `using`, nol tipe, nol pemanggilan. Perbedaan itu ditulis di sini supaya
> pemeriksaan berikutnya yang memakai `grep` polos tidak membacanya sebagai pelanggaran.

### 2.2 Penyaring dibangun supaya tidak mungkin melebar, bukan sekadar ditulis sempit

Ini butir yang paling menentukan pada task ini, dan bentuk kodenya sengaja dipilih untuk itu.

| Penjagaan | Bentuknya di kode |
|---|---|
| Sasaran tidak dapat disalahsetel | Diambil dari `IEncounterContinuationProbe.TargetService`, **bukan** dari konfigurasi maupun konstanta |
| Nol penjawab → nol penutupan | `if (_probes.Count == 0) return result;` sebelum satu query pun dijalankan |
| Unit tanpa penjawab tidak tersentuh | Tidak punya nilai `TargetService` yang dapat disaring |
| `Unknown` tidak menutup apa pun | Dilewati eksplisit |
| Kunjungan non-kiosk tidak terbaca | `x.KioskScanSessionId != null` |

Akibatnya: **mencabut satu baris `AddScoped<IEncounterContinuationProbe, ...>` membuat unit itu
berhenti ikut ditutup — bukan membuatnya ditutup membabi buta.** Kegagalan konfigurasi jatuh ke
arah aman.

### 2.3 Yang ditulis saat menutup

| Tabel | Yang berubah |
|---|---|
| `RegPatientEncounter` | `EncounterStatus = NoShow`, `NoShowAt`, `NoShowByUserId`, `NoShowReason`, `IsActive = false` |
| `TrxQueue` | `QueueStatus = NoShow`, `NoShowAt`, `NoShowByUserId`, `NoShowReason`, `IsActive = false` |

Antreannya **ditandai tidak datang, bukan dibatalkan** — keduanya berbeda sebab, dan kolomnya
memang sudah dipisah pada `TrxQueue` (`NoShowAt` berdampingan dengan `CancelledAt`). Ketiga
kolom `NoShow*` pada kedua tabel **sudah ada sejak sebelum task ini**; tidak ada satu pun kolom
yang perlu ditambahkan.

Keduanya dalam satu transaksi. Notifikasi realtime dikirim **sesudah** commit dan kegagalannya
ditelan — penutupan yang sudah tersimpan tidak boleh dibatalkan oleh SignalR yang sedang tidak
tersambung.

### 2.4 Butir "biaya pendaftaran gugur" — nol kode, dan itu ditulis di dalam source

Registrasi **nol** menerbitkan fakta kelayakan tagih; penerbitnya hanya Klinis, Laboratorium,
Farmasi, dan Radiologi. `DefaultRegistrationFee` hanya hidup sebagai data induk. **Tidak ada
tagihan yang perlu digugurkan karena tidak pernah ada yang terbit.**

Ini sudah ditemukan `BE-EXT-04b` dan dicatat pada dokumen; yang ditambahkan task ini adalah
menuliskannya **di dalam `KioskEncounterClosureService`** — supaya orang berikutnya yang membuka
berkas itu tidak menambahkan pembatalan tagihan yang tidak punya sasaran.

---

## 3. Verifikasi

### 3.1 Build

`dotnet build -p:RunAnalyzers=False` — **0 error**. Peringatan repository turun dari 211 ke 209
sesudah dua `<param>` yang kurang dilengkapi; **nol peringatan berasal dari kelima berkas baru.**

### 3.2 Penyaringnya diukur terhadap `QuilvianNewDevYoga`, bukan diperkirakan

Penyaring kandidat diterjemahkan apa adanya menjadi `SELECT` dan dijalankan terhadap database
sungguhan pada 2026-09-17. **Nol baris diubah.** Kolom "cocok" adalah banyaknya kunjungan yang
akan ditutup.

| Penyaring | Cocok |
|---|---:|
| **A. Sebagaimana ditulis** (`TargetService = Laboratory`) | **0** |
| B. Bila klausa tujuan dihapus | **14** |
| C. Bila klausa kiosk juga dihapus | **157** |
| D. Di antara C yang berstatus `WaitingForNurse` | **91** |

**Inilah angka yang menjadi alasan bentuk kode pada bagian 2.2.** Satu klausa yang hilang berarti
14 kunjungan kiosk nyata ditutup; dua klausa yang hilang berarti 157 kunjungan, **91 di antaranya
pasien poliklinik yang sedang duduk menunggu dipanggil.** Baris A membuktikan yang sebaliknya:
sebagaimana ditulis, ia **tidak menyentuh apa pun hari ini** — karena memang belum ada kunjungan
laboratorium dari kiosk (0 dari 15 kunjungan kiosk; 1 sesi bertujuan Laboratorium, belum terpakai).

### 3.3 Fakta lain yang dibaca langsung dari database

| Hal | Nilai |
|---|---:|
| Sesi kiosk seluruhnya | 17 |
| Sesi kiosk bertujuan `Laboratory` | 1 |
| Kunjungan bersumber kiosk | 15 |
| Kunjungan kiosk **bertujuan Laboratorium** | **0** |
| `LabOrder` aktif | 8 |

### 3.4 Empat cabang, dijalankan lewat aplikasi yang sebenarnya

Penyaring yang cocok nol baris tidak membuktikan apa pun tentang **akibat tulisnya**. Karena itu
empat kunjungan uji dibentuk — **satu yang harus tertutup, dan tiga yang harus tetap utuh** —
lalu penutupannya dijalankan oleh `KioskEncounterClosureHostedService` di dalam aplikasi yang
benar-benar menyala, bukan oleh logikanya yang ditiru dari luar.

**Pukul batasnya diturunkan lewat variabel lingkungan**
`HealthServices__KioskEncounterClosure__ServiceDayEndHour=0`, bukan dengan menyunting berkas.
Itu sekaligus membuktikan tuntutan `LAB-DEC-059`: angkanya benar-benar dapat diubah tanpa rilis
ulang.

| Cabang | Rancangannya | Harapan | Hasil |
|---|---|---|---|
| **A** | Kiosk bertujuan Laboratorium, nol `LabOrder`, nol `CheckedInAt` | **DITUTUP** | ✅ `EncounterStatus` **5 → 11** (`NoShow`), `NoShowAt` terisi, sebab persis, `IsActive` → `false`. Antreannya **4 → 8** (`NoShow`) |
| **B** | Sama, tetapi **punya `LabOrder`** | tetap utuh | ✅ tetap `5`, nol `NoShowAt` |
| **C** | Kiosk bertujuan **poliklinik** | tetap utuh | ✅ tetap `5`; antreannya tetap `4` |
| **D** | Kiosk bertujuan Laboratorium, **sudah `CheckedInAt`** | tetap utuh | ✅ tetap `5` |

**Ketiga cabang "harus tetap utuh" sengaja diuji, bukan hanya cabang yang menutup.** Penutupan
yang terlalu rakus tidak akan pernah tertangkap oleh uji yang hanya memeriksa bahwa sesuatu
tertutup.

**Pagar terhadap data nyata diperiksa pada putaran yang sama:** 15 kunjungan kiosk nyata tetap
terbuka, dan 91 kunjungan `WaitingForNurse` tidak bergeser satu pun. Sesudah pembersihan,
penelusuran atas sebab khas task ini menghasilkan **nol** kunjungan dan **nol** antrean — tidak
ada satu pun baris nyata yang pernah tersentuh.

**Idempotensi dibuktikan terpisah:** putaran kedua menutup **0** kunjungan dan menghasilkan **0**
galat — kunjungan yang sudah `NoShow` tidak terbaca lagi sebagai kandidat.

### 3.5 Cacat yang ditemukan uji ini, dan kenapa ia pantas dicatat

**Putaran pertama gagal seluruhnya.** `NoShowByUserId` ber-**foreign key** ke `AspNetUsers`, dan
aktor sistem default — `Guid.Empty`, nilai bawaan sebuah `Guid` — menunjuk pengguna yang tidak
ada:

```
23503: insert or update on table "RegPatientEncounter"
violates foreign key constraint "FK_RegPatientEncounter_AspNetUsers_NoShowByUserId"
```

**Bentuk kegagalannya persis kelas yang paling berbahaya pada modul ini.** Transaksinya
ter-rollback dengan benar — nol baris tertulis separuh — dan penjadwalnya mencatat galat lalu
mencoba lagi. Tidak ada satu pun yang tampak rusak dari luar. Akibatnya: **penutupan otomatis
tidak akan pernah terjadi**, setiap malam, tanpa satu pun tanda kecuali baris log yang tidak
dibaca siapa pun.

**Perbaikannya bukan menambal dengan GUID karangan, melainkan menulis nilai yang jujur.** Kedua
kolom `NoShowByUserId` **nullable** — diperiksa dari `pg_constraint`, bukan diduga — dan `null`
memang artinya yang benar: penutupan ini tidak dilakukan orang. `ResolveNoShowActorAsync`
karena itu:

| Keadaan | Yang ditulis |
|---|---|
| `SystemActorUserId` tidak disetel | `null` |
| Disetel `Guid.Empty` | `null` |
| Disetel, penggunanya **ada** | penunjuk itu |
| Disetel, penggunanya **tidak ada** | `null`, **dan salah setelnya dicatat** — penutupan tetap berjalan |

Cabang terakhir disengaja: satu baris konfigurasi yang keliru tidak boleh menghentikan seluruh
penutupan setiap malam. Kolom `UpdateBy` dan `CreateBy` **tidak** ber-FK, sehingga tetap distempel
`Guid.Empty` seperti jalur lain.

Terbukti sesudah perbaikan: `NoShowByUserId` bernilai **`NULL`** pada kunjungan maupun antrean,
`UpdateBy` bernilai `Guid.Empty`, dan penutupannya berhasil — **`Laboratory=1`, nol galat.**

> **Satu hal yang perlu diketahui `registration-management`.** Cacat yang sama berpotensi ada
> pada `CancelledByUserId` — juga ber-FK dan nullable — tetapi **tidak terlihat hari ini** karena
> satu-satunya penulisnya, `CancelEncounter`, selalu berjalan di bawah pengguna yang sedang
> login. Ia akan muncul pada penulis berikutnya yang berjalan **tanpa** pengguna. Dilaporkan,
> tidak diperbaiki — di luar cakupan task ini.

### 3.6 Kebersihan

Keempat kunjungan uji, kedua antrean, keempat sesi kiosk, dan satu `LabOrder` dihapus permanen.
Hitungan diulang **dari koneksi baru**:

| Tabel | Sebelum | Sesudah |
|---|---:|---:|
| `TrxKioskScanSession` | 17 | **17** |
| Kunjungan kiosk nyata | 15 | **15** |
| `LabOrder` aktif | 8 | **8** |
| `WaitingForNurse` | 91 | **91** |
| Sisa baris uji `BEEXT05` | — | **0** |

---

## 4. Definition of Done

| Butir DoD | Keadaan |
|---|---|
| Kunjungan terbentuk dari sesi kiosk | ✅ **Sudah berdiri sebelum task ini** — `POST /patient-encounters/kiosk`, lihat bagian 1.1. Diverifikasi dari source, bukan diasumsikan |
| Penutupan otomatis hanya mengenai yang tidak pernah dilanjutkan | ✅ Ditulis, dibangun agar tidak mungkin melebar (2.2), dan **selektivitasnya diukur** terhadap database sungguhan (3.2) |
| Biaya pendaftaran gugur | ✅ Terpenuhi tanpa kode; alasannya kini tertulis di dalam source (2.4) |
| `AC-45` tetap tegak | ✅ Laboratorium nol menulis ke `RegPatientEncounter`. Berkas Laboratorium satu-satunya pada task ini **hanya membaca `LabOrder`** dan mengembalikan daftar penunjuk |
| Build | ✅ 0 error, nol peringatan baru |
| Verifikasi proses bisnis **dengan baris nyata** | ✅ **Empat cabang** dijalankan lewat aplikasi yang benar-benar menyala (3.4); pagar data nyata terbukti tidak bergeser; idempotensi terbukti; nol baris uji tertinggal (3.6) |
| Pukul penutupan dapat diubah tanpa rilis ulang (`LAB-DEC-059`) | ✅ Dibuktikan — putaran uji memakai variabel lingkungan, bukan suntingan berkas |

### 4.1 Satu catatan tentang cara butir terakhir terpenuhi

Butir ini sempat ditulis **belum terpenuhi**: penyisipan baris uji ditolak penjaga izin sesi
dengan alasan `QuilvianNewDevYoga` database bersama. **Pemilik modul mengoreksi premis itu** —
`QuilvianNewDevYoga` adalah database pengembangan miliknya sendiri, dan yang bersama adalah
database lain — sehingga pembuktiannya dijalankan.

Dicatat karena hasilnya menentukan: **uji itulah yang menemukan cacat bagian 3.5**, cacat yang
akan membuat seluruh fitur ini tidak pernah berjalan satu malam pun tanpa satu pun tanda dari
luar.

---

## 5. Yang **tidak** dikerjakan, dan alasannya

| Hal | Alasan |
|---|---|
| Mengubah `POST /patient-encounters/kiosk` | Tidak perlu — ia sudah mengerjakan butir 3 `BR-46`. Menyentuhnya berarti mengubah jalur yang sudah dipakai 15 kunjungan nyata tanpa sebab |
| Menyetel `SU-LAB-001.IsAvailableForKiosk` | **Data induk global, wewenang `master-data`.** Izin dari pemilik `registration-management` tidak mencakupnya. Lihat 6.2 |
| Endpoint untuk memicu penutupan secara manual | Belum ada yang memintanya. Menambah endpoint berarti menambah permukaan izin baru pada modul milik orang lain |
| Membetulkan `EncounterStatus` kunjungan lab dari kiosk | Perubahan perilaku pada jalur yang sudah berjalan. Lihat 6.1 |
| Memperbaiki 2 pesanan tanpa disiplin | Di luar cakupan, sudah tercatat sejak `BE-LAB-29` |

---

## 6. Yang perlu diketahui sesudah task ini

### 6.1 Kunjungan laboratorium memperoleh status `WaitingForDoctor`, dan itu keliru

`PatientEncounterController.cs:671` menetapkan status kunjungan begitu antreannya terbentuk:

```csharp
encounter.EncounterStatus = isScreeningRequired
    ? EncounterStatus.WaitingForNurse
    : EncounterStatus.WaitingForDoctor;
```

Cabangnya hanya melihat **screening**, tidak pernah melihat **`IsDoctorRequired`**. Karena
`SU-LAB-001` ber-`IsScreeningRequired = false`, setiap kunjungan laboratorium — dari kiosk maupun
dari loket — akan berstatus **"Menunggu Dokter"** walaupun unitnya ber-`IsDoctorRequired = false`
dan memang tidak ada dokter yang akan memanggilnya. Hal yang sama berlaku pada
`QueueStatus.WaitingForDoctor` di baris `:655`.

**Tidak diperbaiki pada task ini, dan itu keputusan yang disengaja.** Cabang itu berlaku bagi
**seluruh** unit ber-`IsScreeningRequired = false`, bukan hanya laboratorium — mengubahnya
menggeser status kunjungan pada unit-unit yang tidak ikut ditinjau task ini. Itu persis kelas
pengetatan diam-diam yang mahal harganya pada `BE-LAB-21`.

**Ia tidak memengaruhi penutupan otomatis** — penyaringnya nol bersandar pada `EncounterStatus`
selain untuk menolak tiga status terminal.

### 6.2 `IsAvailableForKiosk` masih perlu dijawab `master-data`, tetapi bukan sebagai penahan

`LAB-OPEN-026` terbantah sebagai penahan pembentukan kunjungan (1.2), **tetapi pertanyaannya
belum gugur.** Selama `SU-LAB-001.IsAvailableForKiosk = false`, layar kiosk yang menyaring
daftar unitnya dengan `?isAvailableForKiosk=true` tidak akan menampilkan Laboratorium.

Apakah layar kiosk memang menyaring begitu **belum diperiksa** — ia milik
`registration-management`, dan pertanyaan penandanya milik `master-data`. `LAB-REQ-012` bagian 4
tetap berlaku, dengan sifat yang berubah: **bukan penahan `BE-EXT-05`, melainkan syarat agar
pasien dapat memilih Laboratorium di layar kiosk.**

### 6.3 Pukul 21:00 masih angka yang belum dikonfirmasi

`LAB-DEC-059` menetapkannya, dan mencatat sendiri bahwa ia **belum dikonfirmasi** terhadap jam
operasional resmi rumah sakit maupun terhadap pemilik `registration-management`. Ia kini hidup
sebagai konfigurasi (`HealthServices:KioskEncounterClosure:ServiceDayEndHour`), sehingga
menyesuaikannya tidak menuntut rilis ulang — tetapi angkanya tetap perlu dikonfirmasi.

### 6.4 Pola yang berulang, kini untuk keempat kalinya dalam dua hari

Dua penahan yang menghentikan task ini selama dua hari **keduanya terbantah dalam satu jam
pemeriksaan**, dan keduanya salah dengan cara yang sama: **menyebut nama yang benar pada jalur
yang salah.** `EncounterIntakeService` memang menuntut `PatientEncounter:Create` — tetapi bukan
itu yang dipakai kiosk. `IsAvailableForKiosk` memang `false` — tetapi bukan itu yang dibaca jalur
pembentukan kunjungan.

Ini bersambung dengan tiga temuan sebelumnya pada hari yang sama (`LAB-COORD-010`,
`DATA-MST-MEASUREMENT`, `LAB-OPEN-025`/`026` yang tidak pernah diajukan): **catatan penahan pada
modul ini tidak pernah diverifikasi ulang terhadap source sebelum dipakai menghentikan
pekerjaan.** Dua hari berhenti untuk penahan yang tidak ada.

---

## 7. Langkah berikutnya

| # | Hal | Pemilik |
|---:|---|---|
| 1 | Menetapkan `SystemActorUserId` ke akun sistem yang sah, atau membiarkannya `null` dengan sadar | `registration-management` |
| 2 | Menjawab `LAB-REQ-012` bagian 4 — `IsAvailableForKiosk` pada `SU-LAB-001` | `master-data` |
| 3 | Mengonfirmasi pukul 21:00 terhadap jam operasional resmi | `registration-management` + manajemen rumah sakit |
| 4 | Memeriksa apakah layar kiosk menyaring unit dengan `isAvailableForKiosk=true` | `registration-management` |
| 5 | Meninjau `EncounterStatus` bagi unit ber-`IsDoctorRequired = false` (6.1) | `registration-management` |
| 6 | Meninjau `CancelledByUserId` terhadap penulis yang berjalan tanpa pengguna (3.5) | `registration-management` |
