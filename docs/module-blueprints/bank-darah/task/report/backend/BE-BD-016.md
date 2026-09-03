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
| Status | **`SELESAI SEBAGIAN`** — 8 dari 39 butir terdaftar. Sisanya **tidak dapat didaftarkan sekarang**, dan alasannya arsitektural, bukan kelalaian. Lihat bagian 8 |

---

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
| Keamanan/Auth | **Nol butir hak akses baru dibuat task ini.** Delapan butir yang sudah ada — dari `BE-BD-001` dan `BE-BD-014` — kini dijaga pengujian kontrak. Larangan butir gabungan `BloodUnit : Resolve` ditegakkan otomatis terhadap **seluruh** source |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak membuat maupun menyentuh endpoint.

Delapan butir hak akses yang **sudah** terdaftar dan kini dijaga pengujian:

| Resource | Action | Dari task |
| --- | --- | --- |
| `BloodComponent` | `Read`, `Create`, `Update`, `Delete` | `BE-BD-001` |
| `BloodStorageLocation` | `Read`, `Create`, `Update`, `Delete` | `BE-BD-014` |

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
| DoD — seluruh resource & action hak akses Bank Darah terdaftar | **Belum terpenuhi** | **8 dari 39**. Sisanya tidak dapat didaftarkan sekarang; lihat bagian 8.1 |

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

**Cakupan pendaftaran butir kontrak `v4`: 8 dari 39.**

| Resource | Butir | Terdaftar | Task yang mendaftarkan |
| --- | ---: | :---: | --- |
| `BloodComponent` | 4 | ✅ 4 | `BE-BD-001` |
| `BloodStorageLocation` | 4 | ✅ 4 | `BE-BD-014` |
| `BloodBankReason` | 4 | ❌ 0 | sisa `BE-BD-001` |
| `BloodOrder` | 4 | ❌ 0 | `BE-BD-003` |
| `BloodProviderRequest` | 4 | ❌ 0 | `BE-BD-004` |
| `BloodUnit` | 11 | ❌ 0 | `BE-BD-004`, `006`, `007`, `008`, `009`, `010`, `015` |
| `BloodGroupExam` | 5 | ❌ 0 | `BE-BD-005`, `BE-BD-011` |
| `BloodBankProcedure` | 3 | ❌ 0 | `BE-BD-012` |
| **Total** | **39** | **8** | |

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
