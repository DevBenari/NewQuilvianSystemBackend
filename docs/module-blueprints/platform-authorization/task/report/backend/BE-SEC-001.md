# Laporan Perubahan Backend — `BE-SEC-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-SEC-001` |
| Judul | Authorization Integrity Foundation |
| Slice | Phase A0 — pemulihan integritas otorisasi existing sebagai baseline sebelum Business Permission Layer |
| Roadmap | `docs/module-blueprints/platform-authorization/roadmap/backend-roadmap.md`, baris `BE-SEC-001` |
| Trace | `SEC-REQ-001` sampai `SEC-REQ-012`; keputusan owner `D1`–`D4` dan `N1`–`N3` |
| Contract version | `NOT APPLICABLE` — tidak ada kontrak API bertversi yang disentuh. Kontrak terkunci `opr-permission-v1` dan kontrak Billing justru dipertahankan tanpa perubahan |
| Dependency | Audit Phase 1 dan rencana Phase A0 disetujui pemilik sistem. Wewenang migration dan database development diberikan terpisah |
| Klasifikasi | `HEAVY` — menyentuh jalur otorisasi lintas modul, menambah migration, dan mengubah data hak akses pada database development |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackendAndryZain`, branch `AndryZain` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `43ba35d934d1615edbeac9952e0f19e3cca353fd` |
| Tanggal | 1 September 2026 |
| Status | Selesai dan tervalidasi. Belum di-commit sesuai instruksi pemilik sistem |

---

## 1. Masalah yang diperbaiki

Ada **89 endpoint** yang menolak setiap pengguna non-SuperAdmin dengan `403`, dan penolakan itu
**tidak dapat diperbaiki dari layar Akses Role** karena baris untuk dicentangnya memang tidak
pernah dibuat.

Penyebabnya: sistem menyimpan daftar kemampuan memakai satu nama, tetapi memeriksanya memakai nama
lain. Contoh nyata pada pengambilan sampel laboratorium — yang tersimpan sebagai kemampuan bernama
`Update`, sementara yang dicari saat petugas menekan tombol adalah `Collect`. Karena keduanya tidak
sama, pencarian tidak menemukan apa pun dan hasilnya penolakan permanen.

Dampak nyatanya bagi pengguna:

- **Kasir tidak dapat menutup, menyerahterimakan, atau membuka ulang shift.** Begitu pula menerima
  pembayaran, mencatat deposit, memproses refund, menyetujui penghapusan piutang, dan
  memfinalisasi tagihan.
- **Petugas laboratorium terhenti setelah merencanakan sampel.** Mengambil, menerima, dan
  menyatakan sampel layak periksa seluruhnya tertolak.
- **Kamar operasi tidak dapat mencatat anestesi dan serah terima.**
- **Rawat inap tidak dapat menutup episode, menandatangani ringkasan pulang, atau memindahkan
  tempat tidur.**

Masalah ini tidak terlihat selama ini karena akun SuperAdmin melewati seluruh pemeriksaan, dan
karena suite test yang ada menanam sendiri baris registry-nya alih-alih menurunkannya dari kode.

Dua masalah lain ditemukan pada jalur yang sama:

- **Penempatan organisasi tidak pernah memengaruhi hak akses.** Menambah penempatan lewat menu HR
  resmi tidak melahirkan izin, dan menonaktifkannya tidak mencabut izin. Pegawai yang pindah
  departemen menyimpan izin departemen lamanya bersama yang baru, tanpa batas waktu.
- **Penempatan yang sudah dibatalkan tetap memberi izin**, karena pemeriksaan pembatalan memang
  belum ada.

---

## 2. Proses bisnis

**Tujuan.** Admin rumah sakit dapat memberikan setiap kemampuan aplikasi kepada pasangan
Departemen × Posisi, dan pemberian itu benar-benar berlaku saat petugas memakainya.

**Pelaku.** Admin sistem (memberi hak), HR (mengatur penempatan pegawai), petugas (memakai fitur).

**Pemicu.** Aplikasi dijalankan; atau HR mengubah penempatan seseorang; atau admin mencentang hak
pada layar Akses Role.

**Langkah berurutan:**

1. **Aplikasi dijalankan.** Sistem membaca seluruh endpoint dan mencatat kemampuan yang ada ke
   daftar kemampuan (registry). Nama yang dicatat sama persis dengan nama yang nanti dicari saat
   request masuk.
2. **Sistem memeriksa dirinya sendiri.** Bila ada endpoint terproteksi yang kemampuannya tidak akan
   muncul di layar Akses Role, startup dihentikan pada lingkungan pengembangan. Di produksi hanya
   dicatat sebagai peringatan tingkat kritis, karena rumah sakit tidak boleh gagal menyala hanya
   karena satu anotasi salah.
3. **Kemampuan yang sudah tidak ada ditutup.** Baris lama ditandai tidak aktif dan terhapus, tetapi
   **tidak dihapus fisik**, sehingga sejarahnya utuh.
4. **Admin memberi hak** lewat layar Akses Role. Sistem tidak pernah memberi hak sendiri.
5. **HR mengatur penempatan pegawai.** Setiap perubahan — menambah, menyunting, menonaktifkan,
   mengganti penempatan utama, menghapus — langsung diselaraskan ke tabel yang dipakai pemeriksaan
   izin.
6. **Petugas memakai fitur.** Sistem menggabungkan izin dari **seluruh** penempatan yang masih sah
   milik akun itu, lalu memutuskan boleh atau tidak.

**Aturan yang berlaku.** Sebuah penempatan dianggap sah hanya bila belum dihapus, belum dibatalkan,
berstatus aktif, tanggal mulainya sudah lewat, dan tanggal berakhirnya belum lewat. Penanda
"penempatan utama" dan jenis penempatan **bukan** syarat kelayakan.

**Contoh berangka.** Dr. A ditempatkan di Medical/Dokter Umum sejak 1 Januari, lalu pada 1 Juni
ditambahkan penempatan kedua di IGD/Dokter Jaga sampai 31 Desember. Pada 1 Juli ia memiliki izin
dari kedua penempatan sekaligus. Pada 1 Januari tahun berikutnya penempatan IGD sudah lewat masa
berlakunya, sehingga izin dari IGD berhenti dengan sendirinya sementara izin Medical tetap ada.

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Penempatan dibatalkan HR | Izin dari penempatan itu berhenti |
| Pegawai dinonaktifkan | Seluruh izinnya berhenti, karena akunnya sendiri sudah tidak aktif |
| Pegawai pindah departemen | Penempatan lama ditutup, izin lamanya ikut berhenti |
| Penempatan dihapus lalu pegawai disunting lagi | Penempatan yang sudah ditutup **tidak** hidup kembali; yang baru dibuat sebagai baris baru |
| Sumber penempatan tidak dapat dibuktikan | Baris dipertahankan apa adanya, tidak ditebak dan tidak ditutup, lalu dilaporkan |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` backend; `rules/backend/TASK_RULES.md`; `rules/backend/REPORT_TEMPLATE.md`;
`rules/backend/role-access-rules.md`; `rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`;
`Attributes/AccessControllerAttribute.cs`; `Attributes/AccessActionAttribute.cs`;
`Attributes/AccessPermissionAttribute.cs`; `Filters/AccessPermissionFilter.cs`;
`Services/Security/AccessPermissionService.cs`; `Seeders/AccessMenuSeeder.cs`;
`Models/SysAccessPolicy.cs`; `Models/SysControllerAccess.cs`; `Models/SysActionAccess.cs`;
`Models/ApplicationUserOrganization.cs`; `Areas/Corporate/HumanResource/WorkforceCore/Models/WfpOrganizationAssignment.cs`;
`Repositories/Configurations/Global/ApplicationUserOrganizationConfiguration.cs`;
`Areas/Administrator/Setting/Controllers/RoleAccessController.cs`;
`Shared/HumanResource/Services/HumanResourceContextService.cs`;
`QuilvianSystemBackend.Tests/BillingManagement/AccessPermissionEnforcementTests.cs`;
`QuilvianSystemBackend.Tests/HealthServices/OperatingRoomManagement/OperatingRoomHardeningTests.cs`;
`Program.cs`; serta 279 controller ber-`[AccessController]` untuk inventarisasi permission.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/Security/PermissionRegistryDescriptor.cs` | **Baru.** Satu-satunya tempat identitas permission diturunkan dari atribut. Dipakai bersama oleh seeder, validator, dan test |
| `Services/Security/PermissionRegistryValidator.cs` | **Baru.** Gerbang startup: menolak endpoint terproteksi yang kemampuannya tidak dapat diberikan, identitas resource ganda, dan `AccessType` di luar empat kolom |
| `Services/Security/OrganizationAuthorizationProjectionService.cs` | **Baru.** Menurunkan proyeksi otorisasi dari penempatan otoritatif; menutup yang sudah tidak sah; mengadopsi baris warisan hanya bila sumbernya tunggal; menyediakan mode laporan tanpa menulis |
| `Seeders/AccessMenuSeeder.cs` | Rekonsiliasi generik menggantikan tiga fungsi `Normalize…` yang ditulis tangan. Mendaftarkan identitas dari `[AccessPermission]`; menutup baris yang tidak lagi dideklarasikan source; tetap tidak pernah membuat `SysAccessPolicy` |
| `Services/Security/AccessPermissionService.cs` | Menambah pemeriksaan `!IsCancel` pada kelayakan penempatan, dan mendokumentasikan bahwa `IsPrimary` bukan syarat kelayakan |
| `Models/ApplicationUserOrganization.cs` | Menambah `SourceAssignmentId` yang menghubungkan proyeksi ke penempatan otoritatif |
| `Repositories/Configurations/Global/ApplicationUserOrganizationConfiguration.cs` | Menghapus index unik mutlak yang melarang pengulangan penempatan; menambah index unik terfilter pada `SourceAssignmentId`; menutup celah `NULL` pada index ber-effective-date |
| `Areas/Corporate/HumanResource/WorkforceCore/Controllers/WfpOrganizationAssignmentController.cs` | Kelima mutasi penempatan memanggil service proyeksi. Sebelumnya tidak satu pun menyentuh tabel otorisasi |
| `Areas/Corporate/HumanResource/MasterData/Workforce/Controllers/EmployeeController.cs` | Penulisan langsung ke tabel proyeksi diganti pemanggilan service; penghidupan kembali baris terhapus dihentikan; jalur hapus memakai service |
| `…/Workforce/Controllers/DoctorController.cs` | Sama, ditambah penghapusan `ClearUserPrimaryOrganizationAsync` yang masih menulis langsung ke tabel proyeksi |
| `…/Workforce/Controllers/ExternalUserController.cs` | Sama seperti `EmployeeController` |
| `Program.cs` | Registrasi dua service baru dan pemanggilan gerbang validator setelah seeder |
| `Migrations/20260901073655_A0AuthorizationIntegrityProjection.cs` | **Baru.** Satu migration aditif |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Regenerasi otomatis EF |
| `QuilvianSystemBackend.Tests/Security/PermissionRegistryInvariantTests.cs` | **Baru.** 8 test invarian registry |
| `QuilvianSystemBackend.Tests/Security/OrganizationAuthorizationProjectionTests.cs` | **Baru.** 20 test proyeksi dan pencabutan |
| `QuilvianSystemBackend.Tests/Security/StaleRegistryAuthorizationTests.cs` | **Baru.** 7 test keamanan registry usang |
| `QuilvianSystemBackend.Tests/Security/CanonicalSecurityContractTests.cs` | **Baru.** 5 test kontrak kanonik dan fallback kompatibilitas |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE`. Tidak ada endpoint, route, atau payload yang berubah. Kontrak terkunci `opr-permission-v1` dan kontrak Billing justru **dipertahankan tanpa perubahan** — desain akhir mengambil identitas dari `[AccessPermission]` yang memang sudah dipakai kontrak-kontrak itu, sehingga 12 test kontrak terkunci lulus tanpa satu pun diubah |
| Database | Satu migration aditif `A0AuthorizationIntegrityProjection`: menambah kolom nullable `SourceAssignmentId`, menghapus satu index unik yang melarang riwayat penempatan, menambah satu index unik terfilter, dan membuat ulang index ber-effective-date dengan `NULLS NOT DISTINCT`. Tidak ada kolom dihapus, tidak ada tipe berubah, tidak ada data hilang. **Sudah diterapkan ke database development**; lingkungan lain belum |
| Keamanan/Auth | 89 endpoint pulih dari penolakan permanen dan kini dapat diberikan admin. Penempatan yang dibatalkan berhenti memberi izin. Pencabutan hak mulai bekerja. Tabel proyeksi otorisasi kini punya tepat satu penulis. Tidak ada perluasan hak: `SysAccessPolicy` tetap 498 baris dan jumlah Departemen × Posisi tetap 11 |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menambah, menghapus, atau mengubah satu pun endpoint. Yang
berubah adalah cara kemampuan endpoint didaftarkan dan diperiksa.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | Berhasil tanpa error | `PASS` | `Build succeeded. 0 Error(s)` |
| `dotnet test QuilvianSystemBackend.Tests` | 856 lulus, 0 gagal | `PASS` | `Passed! Failed: 0, Passed: 856, Skipped: 0, Total: 856` |
| `dotnet ef migrations has-pending-model-changes` | Tidak ada perubahan tertunda | `PASS` | `No changes have been made to the model since the last migration.` |
| `git diff --check` | Bersih | `PASS` | Tidak ada keluaran |
| Gerbang integritas source: mismatch sebelum perbaikan | 89 | `PASS` | Dua parser independen menghasilkan angka identik |
| Gerbang integritas source: mismatch sesudah perbaikan | 0 | `PASS` | Validator refleksi atas assembly hasil build |
| Registry usang aktif | `59 → 0` | `PASS` | Perbandingan kunci source dan database |
| Konvergensi registry | source 1.076, database aktif 1.076, selisih `0 / 0` | `PASS` | Query perbandingan himpunan |
| Validator startup pada aplikasi sungguhan | Valid | `PASS` | Log: `Permission registry valid. KeyCount=1076, ActionCount=1076` |
| Seeder tidak membuat `SysAccessPolicy` | 498 → 498, efektif 492 → 492 | `PASS` | Hitungan sebelum dan sesudah seeding |
| Klasifikasi 68 policy inert | 51 `EXACT_EQUIVALENT`, 5 `SEMANTIC_CHANGED`, 12 `REMOVED_CAPABILITY`, 0 `AMBIGUOUS` | `PASS` | Klasifikasi deterministik berbasis kesamaan nama persis; bukti per baris tersimpan |
| Safe rebind | 28 direbind, 23 di-dedupe, 17 tidak disentuh | `PASS` | Departemen × Posisi tetap 11; total policy tetap 498 |
| Proyeksi menunjuk registry hidup | `424 → 452`, naik tepat 28 | `PASS` | Sama dengan jumlah rebind |
| Rekonsiliasi proyeksi organisasi | 0 dibuat, 0 ditutup, 47 diadopsi, 2 tak terpetakan, 0 ambigu | `PASS` | Mode laporan dijalankan lebih dulu, hasilnya sama dengan mode tulis |
| Smoke test terarah, akun non-SuperAdmin | 10 lulus, 0 gagal | `PASS` | Dijalankan pada database development memakai `AccessPermissionService` sungguhan |

**Uji manual:** `PASS` — sepuluh skenario smoke test dijalankan langsung terhadap database
development memakai akun `PermanentDoctor` nyata yang bukan SuperAdmin, mencakup permission yang
tidak terdampak, permission hasil rebind, kemampuan yang dihapus, kemampuan sensitif yang belum
diberikan, gabungan izin multi-organisasi, keselarasan proyeksi, penempatan tidak sah, perilaku
SuperAdmin, dan keterbacaan registry oleh layar Akses Role.

**Tidak dijalankan:**

- Penerapan migration dan rekonsiliasi ke lingkungan selain development — belum diberi wewenang.
- Uji perilaku index lewat test otomatis — provider `InMemory` tidak menegakkan unique index,
  sehingga index diverifikasi langsung pada database sesudah migration.
- Uji dengan data `AssignmentType` selain `Primary` pada data nyata — seluruh 54 penempatan di
  development bertipe `Primary`, sehingga keenam tipe diuji lewat data sintetis pada test otomatis.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Source mismatch `89 → 0` | Terpenuhi | Validator refleksi; `EveryProtectedEndpointIsRegisterableInRoleAccess` |
| Registry usang `59 → 0` | Terpenuhi | Perbandingan kunci source dan database |
| Drift registry `0 / 0` | Terpenuhi | source 1.076, database aktif 1.076 |
| Seeder tidak pernah membuat `SysAccessPolicy` | Terpenuhi | `ReconcileNeverCreatesAccessPolicy`; hitungan 498 → 498 |
| Tidak ada perluasan hak | Terpenuhi | Departemen × Posisi 11 → 11; rebind hanya untuk nama yang identik |
| Kemampuan sensitif tetap fail closed | Terpenuhi | Smoke test `CashierShift.Close` ditolak |
| Izin efektif adalah gabungan penempatan sah | Terpenuhi | Smoke test D; `EffectivePermissionsAreUnionOfActiveAssignments` |
| `IsPrimary` dan `AssignmentType` bukan syarat kelayakan | Terpenuhi | `NonPrimaryAssignmentStillGrantsAccess`; `EveryAssignmentTypeParticipates` |
| Penempatan tidak sah tidak memberi izin | Terpenuhi | `InvalidAssignmentNeverProjects`; `CancelledOrganizationAssignmentDeniesAccess` |
| Registry usang tidak mengotorisasi | Terpenuhi | `StaleRegistryAuthorizationTests` (4 test) |
| Skema mendukung riwayat dan rehire | Terpenuhi | Index terverifikasi pada database development |
| Proyeksi dapat ditelusuri ke sumbernya | Terpenuhi sebagian | 47 dari 49 ter-backfill; 2 baris warisan sengaja dibiarkan tanpa sumber |
| Perilaku SuperAdmin tidak berubah | Terpenuhi | Tiga test existing tetap hijau; smoke test G |
| Laporan tracked dan roadmap diperbarui | Terpenuhi | Dokumen ini beserta roadmap dan traceability |

**Butir yang belum terpenuhi:**

- **Traceability `SourceAssignmentId` belum menyeluruh.** Dua baris proyeksi tidak memiliki
  penempatan otoritatif yang cocok. Keduanya sengaja dipertahankan dengan nilai kosong; menebak
  sumbernya berarti mengarang sejarah.
- **Kemampuan sensitif belum diberikan kepada siapa pun.** Kasir, refund, penghapusan piutang,
  penerimaan sampel, dan penandatanganan ringkasan pulang kini dapat diberikan, tetapi tetap
  ditolak sampai pemilik sistem memutuskan Departemen × Posisi mana yang berhak.
- **Migration dan rekonsiliasi baru diterapkan ke development.**

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Peringatan compiler `CS8619` muncul pada `BankController`, `CompanyGuarantorController`, `IdentityScannerProfileController`, dan `InsuranceProviderController`. Keempatnya **tidak** disentuh task ini dan peringatannya sudah ada sebelumnya |
| Masalah yang diketahui | 17 policy tetap tidak berlaku: 5 karena nama aksinya sudah tidak ada, 12 karena resource-nya sudah tidak ada. Sengaja dibiarkan fail closed, tidak dipindahkan, dan tidak dihapus. Dua endpoint audio antrean masih hanya terlindungi `[Authorize]` dan klasifikasinya belum diputuskan. Layar Akses Role masih mengelompokkan kolom lewat pencocokan teks, sehingga verba bisnis seperti `Collect` dan `Accept` belum dapat diberikan satu per satu — identitasnya sudah benar di database, antarmukanya menyusul pada fase berikutnya |
| Risiko tersisa | Bila hasil ini diterapkan ke lingkungan lain, rekonsiliasi registry akan menutup baris usang di lingkungan itu dan sebagian policy dapat menjadi tidak berlaku, persis seperti yang terjadi di development. Jalankan klasifikasi dan rebind aman lebih dulu, dan tinjau hasilnya sebelum menulis |
| Perubahan sampingan | Normalisasi akhir baris dan penghapusan baris kosong di akhir berkas pada lima berkas yang memang disentuh task ini, agar `git diff --check` bersih. Tidak ada berkas lain yang disentuh |
| Interupsi | `NONE` |
| Status Git | Lihat bagian di bawah |
| Langkah berikutnya | Pemilik sistem memutuskan pemberian kemampuan sensitif dan penanganan 17 policy inert; klasifikasi dua endpoint audio antrean; penerapan ke lingkungan lain; barulah fase Business Permission dimulai |

### Status Git pada akhir pekerjaan

```text
 M Areas/Corporate/HumanResource/MasterData/Workforce/Controllers/DoctorController.cs
 M Areas/Corporate/HumanResource/MasterData/Workforce/Controllers/EmployeeController.cs
 M Areas/Corporate/HumanResource/MasterData/Workforce/Controllers/ExternalUserController.cs
 M Areas/Corporate/HumanResource/WorkforceCore/Controllers/WfpOrganizationAssignmentController.cs
 M Migrations/ApplicationDbContextModelSnapshot.cs
 M Models/ApplicationUserOrganization.cs
 M Program.cs
 M Repositories/Configurations/Global/ApplicationUserOrganizationConfiguration.cs
 M Seeders/AccessMenuSeeder.cs
 M Services/Security/AccessPermissionService.cs
?? Migrations/20260901073655_A0AuthorizationIntegrityProjection.Designer.cs
?? Migrations/20260901073655_A0AuthorizationIntegrityProjection.cs
?? QuilvianSystemBackend.Tests/Security/
?? Services/Security/OrganizationAuthorizationProjectionService.cs
?? Services/Security/PermissionRegistryDescriptor.cs
?? Services/Security/PermissionRegistryValidator.cs
?? docs/module-blueprints/platform-authorization/
```

---

## Lampiran A — Kontrak permission kanonik

Aturan yang berlaku sejak task ini, dikunci sebagai test pada `CanonicalSecurityContractTests`:

1. **Identitas otorisasi** sebuah endpoint adalah pasangan `(resource, action)` pada
   `[AccessPermission]`. Tidak ada sumber lain.
2. **Seeder mendaftarkan pasangan itu apa adanya**, sehingga kunci yang dibuat registry selalu
   identik dengan kunci yang dicari saat request masuk. Selisih di antara keduanya menjadi
   mustahil.
3. **`[AccessAction]` pada endpoint yang punya `[AccessPermission]` murni metadata tampilan** —
   nama tampil, deskripsi, urutan, dan `AccessType` yang menentukan kolom layar Akses Role.
   Argumen pertamanya tidak pernah dipakai sebagai identitas otorisasi.
4. `AccessType` wajib salah satu dari `Read`, `Create`, `Update`, `Delete`.
5. Satu nama resource tidak boleh terdaftar pada lebih dari satu modul.

### Fallback kompatibilitas

Ada **69 endpoint warisan** yang hanya memiliki `[AccessAction]` tanpa `[AccessPermission]`.
Seluruhnya tetap didaftarkan memakai nama controller-nya, persis seperti perilaku sebelum task ini.

| Kelompok | Endpoint | Terlindungi oleh |
| --- | ---: | --- |
| Kiosk pendaftaran mandiri dan master data pendukungnya | 55 | Policy `KioskRead` |
| Layar antrean | 4 | Policy `QueueDisplayRuntimeRead` |
| Self Service presensi dan konteks HR | 8 | `[Authorize]` ditambah pembatasan data milik sendiri |
| Audio panggilan antrean | 2 | `[Authorize]` saja — klasifikasinya belum diputuskan |

Fallback ini **wajib ada**: sebagian kunci didaftarkan oleh endpoint kiosk sementara endpoint
saudaranya yang menegakkan kunci yang sama. Mencabutnya akan membuat endpoint saudara itu ditolak
permanen.

Fallback ini **bukan pola untuk endpoint baru.** Endpoint terproteksi yang baru wajib memakai
`[AccessPermission]`. Daftar 69 endpoint itu dikunci sebagai **himpunan persis** pada
`CompatibilityFallbackMatchesApprovedLegacySetExactly`, bukan sekadar jumlah — sehingga endpoint
baru yang masuk fallback akan menggagalkan test, dan endpoint warisan yang diperbaiki menuntut
daftar diperbarui secara sadar.

---

## Lampiran B — Klasifikasi policy yang menjadi tidak berlaku

Setelah rekonsiliasi menutup baris registry usang, 68 baris `SysAccessPolicy` menunjuk baris yang
sudah ditutup. Seluruhnya diklasifikasi secara deterministik: dicari target aktif yang **nama
resource dan nama aksinya identik**, lalu jumlah target menentukan kelasnya. Tidak ada pencocokan
kemiripan dan tidak ada tebakan.

| Kelas | Policy | Departemen × Posisi | Perlakuan |
| --- | ---: | ---: | --- |
| `EXACT_EQUIVALENT` | 51 | 1 | 28 diarahkan ke baris kanonik, 23 kembar ditutup |
| `SEMANTIC_CHANGED` | 5 | 3 | Dibiarkan tidak berlaku |
| `REMOVED_CAPABILITY` | 12 | 4 | Dibiarkan tidak berlaku |
| `AMBIGUOUS` | 0 | 0 | — |

Rincian yang dibiarkan tidak berlaku:

| Kelas | Resource dan aksi | Policy | Alasan |
| --- | --- | ---: | --- |
| `SEMANTIC_CHANGED` | `KioskScanSession.Cancel` | 1 | Resource masih ada, aksinya tidak lagi dideklarasikan |
| `SEMANTIC_CHANGED` | `WorkSchedule.Update`, `WorkSchedule.Delete` | 4 | Resource masih ada, aksinya tidak lagi dideklarasikan |
| `REMOVED_CAPABILITY` | `Queue.Read`, `Queue.Update` | 12 | Resource `Queue` sudah tidak ada |

Pengaman yang dipasang saat pengarahan ulang:

- Perintah pembaruan menyertakan `DepartmentId` dan `PositionId` pada penyaringnya, sehingga hak
  tidak mungkin berpindah ke pasangan lain.
- Hanya kelas `EXACT_EQUIVALENT` yang diarahkan ulang; nama resource dan aksinya identik, sehingga
  maknanya tidak berubah.
- Dua baris usang yang menunjuk target yang sama dijaga: yang pertama diarahkan ulang, sisanya
  ditutup, sehingga tidak lahir hak kembar.
- Tidak ada pemekaran satu-ke-banyak. Pola seperti `Update → Collect + Accept` tidak pernah
  dilakukan, dan secara konstruksi memang tidak mungkin.

Bukti per baris — memuat `OldControllerAccessId`, `OldActionAccessId`, `DepartmentId`,
`PositionId`, kandidat target, jumlah target, dan klasifikasinya — dihasilkan ulang kapan pun
dengan menjalankan klasifikasi read-only terhadap database yang bersangkutan.
