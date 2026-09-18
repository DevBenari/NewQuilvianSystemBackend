# Laporan Perubahan Backend — `BE-BD-016`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BD-016` |
| Judul | Seluruh resource & action hak akses Bank Darah terdaftar |
| Slice | `MVP-0` — fondasi master Bank Darah |
| Roadmap | `docs/module-blueprints/bank-darah/roadmap/00-delivery-plan.md` §E |
| Trace | `DEC-BD-039`..`DEC-BD-045` · `INV-BD-034` · `contracts/permission-audit-matrix.md` · `AC-BD-078`, `AC-BD-090`, `AC-BD-093` |
| Contract version | `v4` — **`approved`** (`Sukmagp` / `2026-09-03`) |
| Dependency | `G1` approval ✅ · bebas `G2b` |
| Klasifikasi | `MEDIUM` — nol entity, nol endpoint, nol migration; satu berkas pengujian kontrak beserta penelusuran mekanisme hak akses |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `QuilvianSystemBackend.Tests/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `ec2bcac` cabang `sukmagp` |
| Tanggal | `2026-09-03` |
| Status | ✅ **SELESAI 17 September 2026** — inventaris current **38 butir**, identik pada source, `api-contract.md`, dan database hasil `AccessMenuSeeder`; `BloodUnit : Resolve` dan `BloodOrder : Update` absen; pemisahan wewenang dibuktikan runtime dengan aktor non-SuperAdmin (bagian 10). **Riwayat:** 🟡 **SELESAI SEBAGIAN.** Per audit source pemilik 14 September 2026: **30 deklarasi unik terhadap baseline roadmap 39**; pendaftaran DB dan penegakan akses non-SuperAdmin belum diverifikasi pada review ini. Lihat bagian 9. **Riwayat 3 September 2026:** **`SELESAI SEBAGIAN`** — **12 dari 39** butir terdaftar per 3 September 2026, naik dari 8 setelah `MstBloodBankReason` selesai. Sisanya **tidak dapat didaftarkan sekarang**, dan alasannya arsitektural, bukan kelalaian. Lihat bagian 8 |

---

> **Penutupan current ada pada bagian 10 (17 September 2026).** Bagian 1–9 adalah HISTORY.
>
> **Cara membaca laporan:** bagian 1-8 dan preflight di bawah dipertahankan sebagai histori pengerjaan awal 3 September 2026. Angka 8/12/39 serta test historis di dalamnya bukan inventaris source terkini. Pembaruan terbatas yang memiliki bukti output pemilik ada pada bagian 9.

## 1. Masalah yang diperbaiki

Sebuah kemampuan di Quilvian hanya dapat diberikan kepada seseorang bila ia **muncul sebagai baris
yang dapat dicentang** di layar Pengaturan → Manajemen Role → Akses Role. Endpoint yang memeriksa
pasangan hak akses yang tidak pernah terdaftar tidak menghasilkan galat yang terlihat — ia
menghasilkan **403 permanen yang tidak dapat diperbaiki dari layar mana pun**, karena baris untuk
dicentangnya memang tidak pernah dibuat.

Cacat semacam ini pernah benar-benar terjadi. Sembilan endpoint modul Rawat Inap memeriksa pasangan
yang tidak pernah didaftarkan, dan lolos berbulan-bulan karena seluruh pengujiannya dilakukan
memakai akun SuperAdmin — dan `AccessPermissionService.HasAccessAsync` memulangkan `true` untuk
SuperAdmin **sebelum satu baris hak akses pun dibaca**.

Bank Darah memikul risiko yang sama, ditambah satu risiko khasnya sendiri: `DEC-BD-043` memecah
`BloodUnit : Resolve` menjadi tiga butir terpisah justru karena arah risikonya berlawanan. Bila butir
gabungan lama diam-diam ikut terdaftar, siapa pun yang boleh membuang kantong rusak otomatis boleh
**mengalihkan darah ke pasien lain** — tindakan paling berisiko di antara ketiganya.

---

## 2. Proses bisnis

**Tujuan.** Memastikan setiap kewenangan Bank Darah dapat diberikan admin lewat layar Akses Role,
dan tidak ada kewenangan yang diam-diam menyatu.

**Pelaku.** Admin sistem, lewat layar Akses Role. **Bukan** kode — kode hanya mendeklarasikan
kemampuan apa yang ada; kode tidak pernah memutuskan siapa yang boleh memakainya.

**Cara kerja yang berlaku di repository ini:**

```text
[AccessController] + [AccessAction]  ──(refleksi saat startup)──►  SysControllerAccess / SysActionAccess
                                                                        │
                                        layar Akses Role membaca daftar ini
                                                                        ▼
                                          admin mencentang  ──►  SysAccessPolicy (Departemen × Posisi)
                                                                        │
[AccessPermission] ──► AccessPermissionFilter ──► AccessPermissionService.HasAccessAsync
```

**Contoh nyata.** Admin membuka layar Akses Role dan mencari kemampuan "Menghapus komponen darah".
Kemampuan itu muncul karena `BloodComponentController.Delete` membawa
`[AccessAction("Delete", ...)]`. Admin mencentangnya untuk departemen Bank Darah posisi Kepala Unit.
Sejak saat itu petugas pada posisi tersebut dapat memanggil endpoint-nya. Tidak ada nama peran yang
ditulis di kode.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Alasan diperiksa |
| --- | --- |
| `rules/backend/role-access-rules.md` | Kontrak penamaan `[AccessController]`/`[AccessAction]`/`[AccessPermission]` dan larangan hardcode |
| `Seeders/AccessMenuSeeder.cs` | **Penentu bentuk task ini.** Menetapkan bagaimana butir hak akses sesungguhnya lahir |
| `Services/Security/AccessPermissionService.cs` | Apa yang dicari filter saat request masuk |
| `Attributes/AccessPermissionAttribute.cs`, `AccessActionAttribute.cs` | Bentuk argumen yang harus dicocokkan |
| `Constants/AccessTypes.cs` | Empat nilai `AccessType` yang ditampilkan layar Akses Role |
| `QuilvianSystemBackend.Tests/InPatientManagement/InpatientRoleAccessContractTests.cs` | **Preseden rumah** untuk pengujian kontrak hak akses (`BE-RWI-034`) |
| `docs/module-blueprints/bank-darah/contracts/permission-audit-matrix.md` | Daftar butir dan pemetaan peran |
| `docs/module-blueprints/bank-darah/contracts/api-contract.md` | 39 pasangan hak akses kontrak `v4` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `QuilvianSystemBackend.Tests/HealthServices/MasterData/BloodBankRoleAccessContractTests.cs` | **Baru.** 12 pengujian kontrak hak akses |

**Nol berkas source aplikasi berubah.** Alasannya di bagian 8.1.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — nol endpoint dibuat atau disentuh |
| Database | `NOT APPLICABLE` — nol entity, nol migration. `SysControllerAccess` dan `SysActionAccess` diisi `AccessMenuSeeder` saat startup, bukan oleh migration |
| Keamanan/Auth | **Nol butir hak akses baru dibuat task ini.** Butir yang sudah ada — dari `BE-BD-001` dan `BE-BD-014` — kini dijaga pengujian kontrak; jumlahnya 8 saat task ini ditulis, menjadi **12** setelah `MstBloodBankReason` selesai pada hari yang sama. Larangan butir gabungan `BloodUnit : Resolve` ditegakkan otomatis terhadap **seluruh** source |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak membuat maupun menyentuh endpoint.

Butir hak akses yang **sudah** terdaftar dan kini dijaga pengujian:

| Resource | Action | Dari task |
| --- | --- | --- |
| `BloodComponent` | `Read`, `Create`, `Update`, `Delete` | `BE-BD-001` |
| `BloodStorageLocation` | `Read`, `Create`, `Update`, `Delete` | `BE-BD-014` |
| `BloodBankReason` | `Read`, `Create`, `Update`, `Delete` | `BE-BD-001` pass kedua — ditambahkan setelah task ini ditulis |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | Berhasil — `0 Error(s)`, `186 Warning(s)` | `PASS` | Jumlah warning identik dengan sebelum task ini |
| 12 pengujian `BloodBankRoleAccessContractTests` | `Failed: 0, Passed: 12` | `PASS` | Dijalankan bersama 59 pengujian task sebelumnya; total **71 lulus** |
| Penelusuran penulis `SysActionAccess`/`SysControllerAccess` di production | Hanya `AccessMenuSeeder.cs` | `PASS` | `grep` seluruh source: dua penulis lain berada di berkas pengujian, bukan production |
| `dotnet test QuilvianSystemBackend.Tests` | **Tidak dapat dijalankan** | `EXISTING / ENVIRONMENT ISSUE` | Kerusakan pre-existing `PatientEncounterTestWorld.cs`, sama seperti dicatat `BE-BD-001.md`. **Masih belum diperbaiki pemiliknya** |

**Rincian 12 pengujian:**

| Kelompok | Jumlah | Yang dibuktikan |
| --- | ---: | --- |
| Kontrak penamaan | 5 | Setiap pasangan yang diperiksa ada sebagai baris yang dapat dicentang; resource sama persis dengan `ControllerName`; aksi sama persis dengan `[AccessAction]` argumen pertama; `AccessType` termasuk empat kolom yang ditampilkan; nol endpoint yang hanya terlindungi `[Authorize]` |
| Penjaga pemisahan butir | 4 | `BloodUnit : Resolve` gabungan **tidak pernah** terdaftar di seluruh source (`DEC-BD-043`, `INV-BD-034`); pembatalan order tidak dipetakan ke `Update` (`DEC-BD-044`); kontrak memuat tiga butir penyelesaian terpisah; validasi rutin terpisah dari penyelesaian konflik (`DEC-BD-039`) |
| Cakupan dan gap | 3 | Butir milik task yang sudah selesai seluruhnya terdaftar; butir milik task yang belum dikerjakan **belum** terdaftar — penjaga terhadap baris yatim; angka cakupan dikunci pada 8 dari 39 |

Uji manual: `NOT FEASIBLE` — memverifikasi kemunculan di layar Akses Role menuntut aplikasi berjalan
dengan database yang sudah dimigrasikan beserta akun admin. Pengujian kontrak di atas memeriksa
sumber yang sama yang dipakai `AccessMenuSeeder`, sehingga hasilnya setara tanpa database.

**Tidak dijalankan:**

| Pemeriksaan | Alasan |
| --- | --- |
| Menjalankan `AccessMenuSeeder` terhadap database | Menuntut eksekusi database, wewenang terpisah |
| Pemeriksaan jalur otorisasi memakai pengguna non-SuperAdmin | Preseden `BE-RWI-034` melakukannya, tetapi menuntut penyusunan `SysAccessPolicy` beserta `ApplicationUserOrganization`. Untuk Bank Darah, seluruh butir yang ada berasal dari master data biasa yang bentuknya sudah terbukti; nilainya belum sepadan sebelum controller operasional lahir. Dicatat sebagai pekerjaan `BE-BD-009` |
| `dotnet test` seluruh solusi | Terhalang kerusakan pre-existing |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| DoD — `BloodUnit : Resolve` lama **MUST NOT** didaftarkan | **Terpenuhi** | `ButirGabunganBloodUnitResolve_TidakPernahDidaftarkanDiSeluruhSource`, menyapu seluruh assembly aplikasi |
| `AC-BD-078` — pemisahan wewenang validasi golongan darah | **Terpenuhi pada tingkat kontrak** | `KontrakMemisahkanValidasiRutinDariPenyelesaianKonflik`. Penegakan runtime menunggu `BE-BD-005` dan `BE-BD-011` |
| `AC-BD-093` — pemegang kewenangan operasional ditolak saat mengalihkan kantong | **Belum terpenuhi — bukan milik task ini** | Menuntut `BloodUnitController` yang belum ada. Menjadi acceptance `BE-BD-009` |
| `AC-BD-090` — petugas tanpa kewenangan validasi ditolak menyatakan bukti kecocokan | **Belum terpenuhi — bukan milik task ini** | Menuntut endpoint bukti kecocokan. Menjadi acceptance `BE-BD-007` |
| DoD — seluruh resource & action hak akses Bank Darah terdaftar | **Belum terpenuhi** | **12 dari 39**. Sisanya tidak dapat didaftarkan sekarang; lihat bagian 8.1 |

### Kenapa task ini tidak boleh ditandai selesai

Judul task berbunyi "**Seluruh** resource & action hak akses Bank Darah terdaftar". Yang terdaftar
baru delapan, dan tiga puluh satu sisanya menunggu controller pemakainya lahir. Menandainya selesai
akan menyembunyikan kekurangan itu di balik pengujian yang semuanya hijau.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 186 warning build, jumlahnya identik dengan sebelum task ini. Nol warning baru |
| Masalah yang diketahui | Tiga hal pada bagian 8: mekanisme pendaftaran yang berbeda dari asumsi roadmap, satu pertentangan di dalam matriks hak akses, dan hardcode role pada modul lain |
| Risiko tersisa | Selama 31 butir belum lahir, layar Akses Role belum dapat memberikan kewenangan Bank Darah operasional kepada siapa pun. Itu **benar dan aman** untuk keadaan sekarang — endpoint-nya memang belum ada — tetapi menjadi pemblokir go-live bila terlupakan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat di bawah |
| Langkah berikutnya | 1. Pemilik proses memutuskan pertentangan `BloodUnit : Compatibility` pada bagian 8.2. 2. Selesaikan sisa `BE-BD-001` bagian `MstBloodBankReason` — menambah 4 butir. 3. Setiap task `BE-BD-003` dan seterusnya menambahkan `ModuleControllers` pada berkas pengujian ini, sehingga cakupannya naik otomatis. 4. Jalankan ketiga migration `MVP-0` lewat wewenang eksekusi database terpisah |

```text
 M docs/module-blueprints/bank-darah/roadmap/00-delivery-plan.md
?? QuilvianSystemBackend.Tests/HealthServices/MasterData/BloodBankRoleAccessContractTests.cs
?? docs/module-blueprints/bank-darah/task/report/backend/BE-BD-016.md
```

---

## 8. Gap yang ditemukan dan sengaja tidak diimplementasikan

### 8.1 ⚠️ Tidak ada seeder permission untuk ditulis — dan menulisnya akan merusak

Roadmap menuliskan `BE-BD-016` sebagai "**Seeder resource + action**". Source bekerja dengan
mekanisme yang berbeda, dan selisih ini menentukan seluruh bentuk task.

**Yang sebenarnya berlaku.** `Seeders/AccessMenuSeeder.cs` adalah **satu-satunya** penulis
`SysControllerAccess` dan `SysActionAccess` di production. Ia bekerja murni lewat refleksi:

```csharp
var controllerActions = actionDescriptorProvider
    .ActionDescriptors
    .Items
    .OfType<ControllerActionDescriptor>()
    .ToList();
```

Artinya butir hak akses **lahir dari controller yang benar-benar ada dan ter-routing**, bukan dari
daftar yang ditulis tangan. Tidak ada daftar permission di mana pun dalam source.

**Kenapa menulis seeder tandingan akan merusak.** Menyisipkan baris untuk controller yang belum
dibuat menghasilkan empat akibat, dan ketiga yang pertama tidak dapat diperbaiki tanpa membongkarnya
lagi:

1. **Dua sumber kebenaran.** Satu berbasis refleksi, satu berbasis daftar tangan. Keduanya akan
   menyimpang, dan yang menang bergantung pada urutan eksekusi.
2. **Baris yatim yang menipu.** Admin melihat kemampuan itu di layar Akses Role dan mencentangnya
   untuk sebuah posisi. Yang dicentang tidak menjaga endpoint mana pun, karena endpoint-nya belum
   ada. Admin percaya kewenangan sudah diatur padahal belum.
3. **Bertentangan dengan aturan hak akses.** Aturannya berbunyi "kode mendeklarasikan kemampuan apa
   yang ada". Mendeklarasikan kemampuan yang tidak ada membalik kalimat itu.
4. Ketika controller-nya akhirnya dibuat, refleksi akan menemukan baris yang sudah ada dengan
   `ControllerAccessId` yang mungkin berbeda, dan hasilnya duplikat atau tabrakan kunci.

**Yang dikerjakan sebagai gantinya.** Berkas pengujian kontrak yang mengikuti preseden rumah
`InpatientRoleAccessContractTests` (`BE-RWI-034`), berisi tiga lapis penjagaan: kontrak penamaan
untuk controller yang ada, penjaga pemisahan butir `DEC-BD-043`/`DEC-BD-044` yang menyapu **seluruh**
source, dan pengunci angka cakupan yang membuat gap terbaca sebagai angka.

**Cakupan pendaftaran butir kontrak `v4`: 12 dari 39.** Angka ini dikunci pengujian, sehingga naik hanya ketika sebuah task benar-benar melahirkan controller-nya.

| Resource | Butir | Terdaftar | Task yang mendaftarkan |
| --- | ---: | :---: | --- |
| `BloodComponent` | 4 | ✅ 4 | `BE-BD-001` |
| `BloodStorageLocation` | 4 | ✅ 4 | `BE-BD-014` |
| `BloodBankReason` | 4 | ✅ 4 | `BE-BD-001` |
| `BloodOrder` | 4 | ❌ 0 | `BE-BD-003` |
| `BloodProviderRequest` | 4 | ❌ 0 | `BE-BD-004` |
| `BloodUnit` | 11 | ❌ 0 | `BE-BD-004`, `006`, `007`, `008`, `009`, `010`, `015` |
| `BloodGroupExam` | 5 | ❌ 0 | `BE-BD-005`, `BE-BD-011` |
| `BloodBankProcedure` | 3 | ❌ 0 | `BE-BD-012` |
| **Total** | **39** | **12** | |

**Untuk pemilik roadmap:** deskripsi `BE-BD-016` sebaiknya diubah dari "seeder resource + action"
menjadi penjagaan kontrak hak akses yang cakupannya tumbuh mengikuti task pembuat controller. Itu
perubahan roadmap, bukan perubahan source, dan berada di luar wewenang task ini.

### 8.2 ⚠️ Pertentangan di dalam matriks hak akses — `BloodUnit : Compatibility`

Ditemukan saat menyandingkan matriks hak akses dengan matriks validasi. **Butuh keputusan pemilik
proses; saya tidak memilih salah satunya.**

| Sumber | Isinya |
| --- | --- |
| `permission-audit-matrix.md` baris 116 | Peran **Petugas Bank Darah / BDRS** memperoleh `BloodUnit : Read/Store/Allocate/**Compatibility**/Issue` |
| `permission-audit-matrix.md` baris 121 | Peran **Petugas BDRS berwenang validasi** memperoleh `BloodUnit : Compatibility`, "Ditetapkan `DEC-BD-042`" |
| `validation-matrix.md` `VAL-BD-078` | Menolak **403** ketika "pelaku tidak memegang kewenangan validasi" |

Ketiganya tidak dapat benar bersamaan. Bila butir `Compatibility` diberikan kepada seluruh petugas
BDRS lewat baris pertama, maka pembatasan yang dibuat `DEC-BD-042` batal di tingkat seeder, dan
`VAL-BD-078` tidak akan pernah menyala sebagaimana dirancang — karena hak aksesnya sudah dimiliki
semua orang sebelum aturan bisnisnya sempat memeriksa.

Akibatnya nyata: bukti kecocokan darah dapat dinyatakan sah oleh petugas yang tidak ditunjuk
memvalidasi.

**Dua kemungkinan, dan hanya pemilik yang boleh memilih:**

| Pilihan | Tindak lanjut |
| --- | --- |
| `DEC-BD-042` berlaku apa adanya — hanya petugas berwenang validasi | Cabut `Compatibility` dari baris peran Petugas BDRS umum lewat `design-business-module`. Nol perubahan source |
| Seluruh petugas BDRS memang boleh menyatakan bukti kecocokan | Itu perubahan keputusan bisnis; jalurnya `grill-me`, dan `VAL-BD-078` beserta `AC-BD-090` ikut dicabut |

Tidak diselesaikan di task ini karena keduanya menyentuh keselamatan klinis, dan mengarang salah
satunya berarti memutuskan siapa yang boleh menyatakan darah cocok untuk pasien.

### 8.3 Pemetaan pemilik — lengkap, tanpa gap

Diperiksa satu per satu terhadap `permission-audit-matrix.md` §2: **seluruh 39 butir memiliki
pemetaan peran**, termasuk `BloodUnit : ResolveNotUsable` yang pemiliknya baru ditetapkan
`DEC-BD-045` (kewenangan operasional BDRS). Tidak ada butir yang perlu dikarang pemiliknya.

Satu-satunya persoalan pemetaan adalah pertentangan pada 8.2 — dan itu soal butir yang dipetakan ke
**dua** peran, bukan butir yang tidak dipetakan ke peran mana pun.

### 8.4 Hardcode role pada modul lain — temuan, bukan perbaikan

`Areas/HealthServices/InPatientManagement/Helpers/InpatientActorClaims.cs` menyimpan
`SupervisorOrWardHeadRoles`, `SupervisorRoles`, dan `CashierOrBillingRoles` sebagai daftar nama peran
tetap — anti-pola yang dilarang aturan hak akses, dan komentarnya sendiri sudah mengakuinya sebagai
risiko terbuka.

**Tidak diperbaiki**: milik modul Rawat Inap, di luar wewenang tulis task ini, dan menggantinya
mengubah siapa yang dapat memakai fitur — keputusan pemilik proses. Nol berkas Bank Darah memakai
pola itu, dan pengujian kontrak task ini menjaganya tetap begitu.

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `MasterData` (controller yang diuji) · penjagaan menyapu seluruh assembly |
| Submodule | Tidak berlaku |
| Pemilik/prefix registry | `Administrator / HealthServices` · `Master / Reference` · prefix **`Mst`** · Lifecycle **`ACTIVE`** |
| Keberlakuan | `NEW CODE` — berkas pengujian baru. Nol source aplikasi disentuh |
| Status registry | Terdaftar dan `ACTIVE`. Nol entri registry baru dibutuhkan |

**QBE ID yang berlaku dan cara pemenuhannya:**

| QBE ID | Pemenuhan |
| --- | --- |
| `QBE-PERM-001` | **Inti task ini.** Metadata Access yang berlaku diperiksa huruf demi huruf pada seluruh endpoint Bank Darah yang ada |
| `QBE-API-001` | Boundary API tidak disentuh; pengujian hanya membaca metadata |

**QBE ID yang TIDAK berlaku, beserta alasannya:**

| QBE ID | Alasan tidak berlaku |
| --- | --- |
| `QBE-ENT-001`..`003`, `QBE-CFG-001`/`002`, `QBE-DTO-001`, `QBE-ENUM-001` | Nol entity, configuration, DTO, dan enum dibuat maupun disentuh |
| `QBE-NAM-001`..`004`, `QBE-MOD-001`..`003` | Nol model persisted baru; nol prefix dan folder baru |
| `QBE-SVC-001`, `QBE-PAGE-001`, `QBE-OPT-001`, `QBE-DEL-001` | Nol controller, service, maupun endpoint dibuat |
| `QBE-CODE-001`..`006` | Nol nomor bisnis |
| `QBE-VAL-001`, `QBE-TXN-001` | Nol jalur request dan nol transaksi |
| `QBE-LOG-001`, `QBE-AUD-001` | Nol perubahan state yang dihasilkan task ini |
| `QBE-DB-001`, `QBE-DB-002` | Bukan `LEGACY MIGRATION` |


---

## 9. Adendum audit deklarasi source - 14 September 2026

**Asal bukti:** output PowerShell yang dikirim pemilik, bukan pembacaan database atau eksekusi seeder oleh penyusun dokumen. Perintah memeriksa file tracked pada `Areas/HealthServices/BloodBankManagement/Controllers/*.cs` dan `Areas/HealthServices/MasterData/Controllers/*Blood*.cs`, lalu mengambil pasangan literal `[AccessPermission("Resource", "Action")]` dengan regex dan menghitung nilai unik. Output akhirnya `TOTAL UNIQUE = 30`.

| Resource | Action yang ditemukan | Jumlah | Task asal sesuai roadmap |
| --- | --- | ---: | --- |
| `BloodBankProcedure` | `Create`, `Read`, `Update` | 3 | `BE-BD-012` |
| `BloodBankReason` | `Create`, `Delete`, `Read`, `Update` | 4 | `BE-BD-001` |
| `BloodComponent` | `Create`, `Delete`, `Read`, `Update` | 4 | `BE-BD-001` |
| `BloodGroupExam` | `Create`, `Read`, `ResolveConflict`, `Update`, `Validate` | 5 | `BE-BD-005`, `BE-BD-011` |
| `BloodOrder` | `Cancel`, `Create`, `Read` | 3 | `BE-BD-003` |
| `BloodProviderRequest` | `Create`, `Process`, `Read`, `Update` | 4 | `BE-BD-004` |
| `BloodStorageLocation` | `Create`, `Delete`, `Read`, `Update` | 4 | `BE-BD-014` |
| `BloodUnit` | `Allocate`, `Read`, `Store` | 3 | `BE-BD-006`, `BE-BD-004`, `BE-BD-015` |
| **Total deklarasi unik** | | **30** | |

**Rekonsiliasi angka:** inventaris current diperbarui dari 29 menjadi 30 karena `BloodUnit : Allocate` dari BE-BD-006 belum ikut hitungan lama. Permission tersebut tidak baru dibuat pada sesi review dokumentasi. Angka **29 pada riwayat BE-BD-015 tetap dipertahankan**.

**Baseline 39 belum diaudit ulang sebagai daftar endpoint yang semuanya sah.** Tabel historis 8.1 memuat 39 butir; selisih aritmetisnya kini 9. Selisih ini bukan bukti bahwa sembilan endpoint baru wajib dibuat. Roadmap mencatat `BloodOrder : Update` tidak memiliki endpoint pada kontrak v4. Keputusan atas butir itu tetap milik pemilik kontrak. Delapan selisih lain tidak diberi nama baru hanya berdasarkan angka; cocokkan dengan kontrak dan endpoint aktual sebelum final closure.

**Batas bukti dan pekerjaan tersisa:**

1. Cocokkan setiap deklarasi dengan `ControllerName` di `[AccessController]` dan action di `[AccessAction]`, serta endpoint pemakainya. Regex yang dipakai pemilik bukan validasi kompilasi atau routing.
2. Periksa hasil `AccessMenuSeeder` pada database dev dan kemunculan butir di Akses Role.
3. Buktikan allow/deny melalui akun non-SuperAdmin dengan permission berbeda. Sesi runtime BE-BD-006 memakai SuperAdmin, sehingga tidak membuktikan akses per peran.
4. Saat BE-BD-007/008/009/010 melahirkan pemakai sah, perbarui inventaris secara incremental dan rekonsiliasi baseline kontrak. Jangan membuat permission, controller, endpoint, atau seeder tandingan palsu untuk mencapai 39/39.

Status tetap **SELESAI SEBAGIAN**. Adendum ini tidak menjalankan test otomatis, tidak membuat folder `Tests/`, tidak menambah permission, tidak menulis database, dan tidak menutup keputusan kontrak.

---

## 10. Penutupan — rekonsiliasi inventaris current, 17 September 2026

Dikerjakan dari HEAD `f3aedffa` cabang `sukmagp`, working tree bersih. **Status: ✅ SELESAI.**

### 10.1 Inventaris source

Dibaca dari **8 controller**: lima pada `Areas/HealthServices/BloodBankManagement/Controllers` ditambah tiga
master Bank Darah pada `Areas/HealthServices/MasterData/Controllers` (`BloodComponent`, `BloodBankReason`,
`BloodStorageLocation`) — ketiganya bagian scope task ini sejak awal (§8.1, §9).

| Pemeriksaan | Hasil |
| --- | --- |
| Endpoint ber-routing | **78** (4 + 9 + 10 + 20 + 8 + 9 + 9 + 9), dicocokkan dengan hitungan `[Http*]` per berkas |
| Endpoint tanpa `[AccessAction]` | 0 |
| Endpoint tanpa `[AccessPermission]` | 0 |
| Resource ≠ `AccessController.ControllerName` | 0 |
| Action ≠ `AccessAction.ActionName` | 0 |
| Butir unik | **38** |
| Deklarasi ganda yang sah | Beberapa endpoint berbagi satu butir: `Read` pada setiap resource (daftar, detail, ringkasan, metadata, riwayat, koreksi), `BloodOrder : Create` (elektronik, manual, konfirmasi ganda), `BloodUnit : Store` (tetapkan dan pindahkan lokasi), `BloodUnit : Allocate` (alokasi dan pembatalan), `BloodUnit : ApproveCorrection` (setujui dan tolak), `Update` master (`PUT` dan `PATCH /status`) |

### 10.2 Source vs kontrak vs database

| Sisi | Butir unik | Selisih |
| --- | ---: | --- |
| Source | 38 | — |
| `api-contract.md` | 38 | Semula 37: baris `DELETE /{id}` master lokasi penyimpanan (`BloodStorageLocation : Delete`, dibuat `BE-BD-014`) tidak tertulis. Baris ditambahkan — sinkronisasi dokumen atas endpoint yang sudah ada, bukan endpoint baru |
| Database (`AccessMenuSeeder`) | 38 aktif, 0 terhapus | 0 ganda, 0 yatim, 0 hilang; 8 baris controller aktif, 0 ganda |

`BloodUnit : Resolve`: **tidak ada** di source, kontrak, maupun database (nol baris dalam keadaan apa pun,
nol policy). `BloodOrder : Update`: **tidak ada** di ketiganya; tidak ada endpoint `v4` yang memakainya, sehingga
dikeluarkan dari hitungan kanonik. Baseline historis **39** = 38 + `BloodOrder : Update`.

Seeder yang dipakai adalah `AccessMenuSeeder` yang sudah ada, dijalankan saat aplikasi start pada build ini
(`[StartupSeed] AccessMenuSeeder completed`). Tidak ada seeder baru dan tidak ada SQL yang menulis tabel hak akses.

### 10.3 Perbaikan source minimal — kode penolakan kontrak

`api-contract.md` menetapkan `403 VAL-BD-037` pada `POST /blood-group-exams/{id}/validate` dan `403 VAL-BD-069`
pada `POST /blood-group-exams/conflict-resolution` (dituntut `AC-BD-078`), tetapi kedua endpoint memulangkan 403
generik. Diperbaiki dengan `DeniedCode`/`DeniedMessage` pada `[AccessPermission]` — mekanisme opt-in yang sama
dengan `BE-BD-007`..`010`. **Keputusan izin tidak berubah**; yang berubah hanya isi balasan penolakan. Satu
berkas: `BbkBloodGroupExamController.cs`. Nol perubahan model, nol migration.

### 10.4 Verifikasi kontrak terhadap assembly terkompilasi

`BloodBankRoleAccessContractTests.cs` **tidak lagi ada di repository**: dihapus pada `cefd927d` ("Remove backend
test projects and simplify Integration gate", 11 September 2026) dan `/Tests/` di-gitignore pada `fcabdff9`.
Proyek uji itu tidak dipulihkan. Sebagai gantinya, sepuluh pernyataan kontrak dijalankan sebagai pemeriksa
reflection **di luar repository** terhadap `QuilvianSystemBackend.dll` build ini:

| # | Pernyataan | Hasil |
| --- | --- | --- |
| 1 | Setiap endpoint ber-routing punya `AccessAction` | PASS |
| 2 | Setiap endpoint punya `AccessPermission` | PASS |
| 3 | Resource == `AccessController.ControllerName` | PASS |
| 4 | Action == `AccessAction` | PASS |
| 5 | Set source == set kanonik kontrak (38) | PASS |
| 6 | `BloodUnit : Resolve` absen | PASS |
| 7 | `ResolveReallocate`/`ResolveReturn`/`ResolveNotUsable` terpisah, satu endpoint masing-masing | PASS |
| 8 | `Correct` (pengajuan) terpisah dari `ApproveCorrection` (setujui, tolak) | PASS |
| 9 | `Validate` dan `ResolveConflict` terpisah, dengan `VAL-BD-037`/`VAL-BD-069` | PASS |
| 10 | `BloodOrder : Cancel` berdiri sendiri; `BloodOrder : Update` absen | PASS |

Tidak ada angka yang dikunci secara manual: himpunan kanonik dibaca dari `api-contract.md`.

### 10.5 Otorisasi non-SuperAdmin

Bukti runtime yang sudah ada dipakai ulang karena eksplisit dan masih berlaku:

| Butir | Bukti | Sumber |
| --- | --- | --- |
| `BloodUnit : Compatibility` | `AC-BD-090` `403 VAL-BD-078`, `AC-BD-091` `200` | `BE-BD-007` §6 |
| `BloodUnit : EmergencyIssue` | `403 VAL-BD-072` lalu `200` untuk request identik | `BE-BD-008` §6 |
| `ResolveReallocate`/`ResolveReturn`/`ResolveNotUsable` | `AC-BD-093` `403 VAL-BD-080`; `403 VAL-BD-081/082`; ketiga jalur `200` | `BE-BD-009` §6 |
| `Correct`/`ApproveCorrection` | `403 VAL-BD-024/074`; `422 VAL-BD-073` pemegang dua butir | `BE-BD-010` §6 |

Yang sebelumnya hanya terbukti pada tingkat atribut (`BE-BD-003`/`005`/`011`) dibuktikan runtime pada sesi ini.
Lima aktor `Employee`, **nol role**: `validate`, `conflict`, `cancel`, `issue`, `none`. Panggilan memakai id acak,
sehingga lolos filter terlihat sebagai `404`/`400` dari service dan **nol baris domain ditulis** (jumlah
`BbkBloodGroupExam`, `BbkBloodGroupConflictResolution`, `BbkBloodOrder`, `BbkBloodUnit`, `BbkTransitionHistory`
sama sebelum/sesudah).

| Kasus | Aktor | HTTP | `errors.code` / pesan |
| --- | --- | --- | --- |
| `POST /blood-group-exams/{id}/validate` | `validate` | `404` | lolos filter |
| **`AC-BD-078`** `POST /blood-group-exams/conflict-resolution` | `validate` | **`403`** | **`VAL-BD-069`**, pesan persis |
| `POST /blood-group-exams/conflict-resolution` | `conflict` | `400` | lolos filter |
| `POST /blood-group-exams/{id}/validate` | `conflict`, `none` | `403` | `VAL-BD-037`, pesan persis |
| `POST /blood-group-exams/conflict-resolution` | `none` | `403` | `VAL-BD-069`, pesan persis |
| `POST /blood-orders/{id}/cancel` | `cancel` | `404` | lolos filter |
| `POST /blood-orders/{id}/cancel` | `none`, `validate` | `403` | generik — kontrak tidak menetapkan kode `403` |
| `POST /blood-units/{id}/issue` | `issue` | `404` | lolos filter |
| `POST /blood-units/{id}/issue` | `none`, `cancel` | `403` | generik — kontrak tidak menetapkan kode `403` |

13/13 lulus. `AC-BD-090` dan `AC-BD-093` tetap pada bukti `BE-BD-007`/`BE-BD-009`.

### 10.6 Build dan QBE

| Gate | Hasil |
| --- | --- |
| `dotnet build` (memory-safe) | `Build succeeded`, **`0 Error(s)`**, `198 Warning(s)` — sama dengan baseline |
| EF | Nol perubahan model, nol migration |
| QBE Strict | Lihat laporan penutupan sesi |

### 10.7 Fixture `TEST-BD016` — belum dibersihkan

Hanya identitas; nol baris domain. Lima Department/Position `TEST-BD016 …`, lima akun
`test.bd016.{validate,conflict,cancel,issue,none}@rsmmc.local` (user `69c0a86f-04e9-43b0-b99e-9185d7723352`,
`ab147ba1-a6f6-463e-bf41-3ad253a801c7`, `179d8112-31bf-49c5-9396-d38a069cd0bb`,
`c77decff-1051-4b49-af6f-ca69bd397873`, `721d9cd3-3b70-4b48-b9da-640a53db610a`) beserta employee/workforce
profile, dan sebelas baris `SysAccessPolicy`. Fixture `TEST-BD006`..`TEST-BD010` tidak disentuh.
