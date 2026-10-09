# Temuan — modul izin Farmasi terbelah menjadi dua kode

| Hal | Isi |
|---|---|
| Ditemukan | 5 Oktober 2026, saat menyiapkan matriks izin Farmasi tingkat HTTP |
| **Status** | ✅ **Selesai 5 Oktober 2026** — source dikonsolidasikan, data-fix registry tersedia dan teruji |
| Keputusan | kode kanonik **`HEALTH_SERVICE_PHARMACY_MANAGEMENT`**; `HEALTH_SERVICE_PHARMACY` menjadi legacy |
| Sifat | ketidakrapian konfigurasi dengan akibat operasional, bukan cacat kode |
| Dampak saat ditemukan | nol endpoint rusak; seluruh 137 endpoint dijaga dan seluruh izinnya terdaftar aktif |

## Apa yang ditemukan

23 controller Farmasi memasang `[AccessController(...)]`, tetapi menyebut **dua kode modul yang
berbeda**: `HEALTH_SERVICE_PHARMACY` (12 controller) dan `HEALTH_SERVICE_PHARMACY_MANAGEMENT`
(11). Pembelahannya tidak mengikuti pola apa pun yang dapat dibaca, sehingga satu resep melewati
**dua modul izin** dalam satu alur: dibuat di bawah `HEALTH_SERVICE_PHARMACY`, ditelaah dan
disiapkan di bawah `HEALTH_SERVICE_PHARMACY_MANAGEMENT`, diracik kembali di bawah yang pertama,
lalu diserahkan di bawah yang kedua.

Operasi sebagai pembanding memakai satu kode untuk seluruh modulnya:
`HEALTH_SERVICE_OPERATING_ROOM_MANAGEMENT`.

**Akibatnya jatuh pada orang yang memberi izin, bukan pada kode.** Administrator yang diminta
"beri peran ini akses Farmasi" melihat dua modul bernama mirip dan wajar menganggap salah satunya
cukup. Bila yang dipilih `HEALTH_SERVICE_PHARMACY`, peran itu dapat membuat resep tetapi tidak
dapat menelaah, menyiapkan, atau menyerahkannya; bila yang dipilih yang satunya, peran itu dapat
menyerahkan obat tetapi tidak dapat membuat resepnya. Kegagalannya muncul sebagai `403` di tengah
alur klinis, dan dugaan pertama orang hampir pasti tertuju pada kode, bukan pada modul mana yang
tercentang.

## Yang dikerjakan

### 1. Source — 12 controller dipindahkan

| Controller |
|---|
| `MedicationAdministrationController` |
| `MedicationReconciliationController` |
| `MedicationScheduleSettingController` |
| `PrescriptionCompoundController` |
| `PrescriptionCompoundItemController` |
| `PrescriptionController` |
| `PrescriptionItemController` |
| `PrescriptionTemplateController` |
| `PrescriptionWorkspaceController` |
| `SlidingScaleExecutionController` |
| `SlidingScaleOrderController` |
| `SlidingScaleTemplateController` |

Yang diubah **hanya** `moduleCode` dan `moduleName` — 24 baris pada 12 berkas, dua baris per
berkas. Route, resource code, action code, metode HTTP, dan business logic **nol** tersentuh;
diff-nya diaudit untuk memastikan itu.

Sesudahnya: **23 / 23** controller pada satu kode kanonik, legacy dipakai **0** controller.

### 2. Data — `PharmacyModuleCodeConsolidationSeeder`

Startup registration **tidak boleh** diandalkan sendirian, dan ini sebabnya.

`AccessMenuSeeder` mengenali satu baris `SysControllerAccess` dari pasangan
`(ModuleId, ControllerName)` — ada indeks unik pada pasangan itu. Begitu kode modulnya berganti,
pasangan itu berubah, sehingga seeder **tidak menemukan** baris lama dan membuat baris baru
dengan `Id = Guid.NewGuid()`, lalu menutup baris lama (`IsActive = false`, `IsDelete = true`).

Akibatnya fatal dan senyap:

```
SysAccessPolicy.ControllerAccessId -> SysControllerAccess.Id   (RESTRICT)
SysAccessPolicy.ActionAccessId     -> SysActionAccess.Id       (RESTRICT)
SysActionAccess.ControllerAccessId -> SysControllerAccess.Id   (RESTRICT)
SysControllerAccess.ModuleId       -> SysApplicationModule.Id  (RESTRICT)
```

Policy tetap menunjuk baris yang sudah ditutup, dan `AccessPermissionService` menyaring
`IsActive && !IsDelete`. Jadi **setiap izin Farmasi yang pernah diberikan administrator akan
berhenti berlaku tanpa satu pun galat.**

Karena itu seeder konsolidasi berjalan **tepat sebelum** `AccessMenuSeeder`
([`Program.cs`](../../../Program.cs) baris 1610) dan **hanya memindahkan relasi modulnya**:

```
UPDATE SysControllerAccess SET ModuleId = <kanonik> WHERE ModuleId = <legacy>
```

`Id` dipertahankan, sehingga `SysActionAccess.ControllerAccessId` dan kedua kolom pada
`SysAccessPolicy` tetap sah **tanpa disentuh sama sekali**. `AccessMenuSeeder` sesudahnya
menemukan barisnya lewat kunci baru dan memperbaruinya di tempat.

**Mengapa seeder, bukan migration.** Registry ini memang data yang dikelola seeder, bukan schema
— `SysApplicationModule`, `SysControllerAccess`, dan `SysActionAccess` seluruhnya diisi
`AccessMenuSeeder`. Selain itu repositori ini tidak menjalankan migration otomatis saat startup,
sehingga sebuah migration menuntut `dotnet ef database update` — operasi yang pada lingkungan ini
justru terhalang drift 199 migration. Seeder berjalan sendiri pada setiap lingkungan, idempoten,
dan tidak menyentuh metadata migration apa pun.

### 3. Pengaman yang terpasang

| Keadaan | Perilaku |
|---|---|
| Lingkungan tanpa modul legacy | nol pekerjaan |
| Modul kanonik belum ada | dibuat lebih dulu, karena `AccessMenuSeeder` berjalan sesudahnya |
| Baris sudah pernah dipindahkan | nol pekerjaan — idempoten |
| Baris sudah ditutup | ikut dipindahkan, `Id` dipertahankan, keadaan tertutupnya tidak diubah |
| Nama controller kembar di kedua modul | digabungkan: aksi dan policy dialihkan ke baris kanonik, baris lama ditutup |
| Izin kembar pada peran yang sama | yang sudah ada dibiarkan, bukan ditimpa — keduanya menyatakan izin yang sama |
| Modul legacy masih punya controller aktif | **tidak** dinonaktifkan |
| Masih ada policy menggantung pada isi modul legacy | **tidak** dinonaktifkan |
| Modul legacy sudah bersih | dinonaktifkan (`IsActive = false`), **tidak** dihapus |

**Nol policy dihapus pada jalur mana pun.**

## Bukti

| Pemeriksaan | Hasil |
|---|---|
| Controller pada kode kanonik | **23 / 23** |
| Controller pada kode legacy | **0** |
| Endpoint tetap punya izin | **137 / 137** |
| Pasangan `resource:action` unik | **85** — tidak berubah |
| Cocok dua arah dengan registry basis data | **85 / 85** |
| Yatim di kedua sisi | **0** |
| Nama controller kembar sesudah konsolidasi | **0** |
| Uji `PermissionMatrixTests` | **260** lulus |
| Uji `ModuleCodeConsolidationTests` | **16** lulus |

`ModuleCodeConsolidationTests` menguji perilaku seeder-nya, bukan hanya bahwa ia berjalan —
termasuk satu uji yang meniru bentuk sebenarnya (12 berpindah + 11 sudah benar, masing-masing
dengan satu izin) dan membuktikan setiap izin masih menunjuk `ControllerAccessId` dan
`ActionAccessId` yang sama persis seperti sebelum perpindahan, serta satu pembuktian negatif
bahwa nol izin menjadi yatim.

## Keadaan lingkungan dev saat ini

Dibaca **read-only** dari `localhost / QuilvianNewDevIkbalFr`; nol perintah tulis.

| Hal | Jumlah |
|---|---|
| Controller di modul legacy | 12 — belum dipindahkan |
| Controller di modul kanonik | 11 |
| `SysAccessPolicy` menunjuk Farmasi | **0** |
| `SysAccessPolicy` seluruh sistem | **0** |

Jadi pada lingkungan ini **nol izin yang terdampak**, dan perpindahan akan berjalan tanpa risiko
apa pun. Registry-nya belum dipindahkan karena seeder berjalan saat startup, dan startup pada
lingkungan ini masih mati di `MstNursingDiagnosisSeeder` —
[`blocker-startup-seeder-tabel-hilang.md`](../../engineering/blocker-startup-seeder-tabel-hilang.md).
Begitu startup hidup, perpindahannya terjadi sendiri pada percobaan pertama.

Lingkungan lain yang **sudah** memiliki `SysAccessPolicy` Farmasi akan ikut terlayani seeder yang
sama, dan itulah sebabnya ia ditulis memindahkan relasi alih-alih membuat ulang baris.

## Backward compatibility

Tidak ada runtime alias untuk `HEALTH_SERVICE_PHARMACY`, dan itu disengaja: nol controller
mengirimkannya lagi, nol pembaca yang mencarinya, sehingga alias hanya akan menjadi jalur kedua
yang harus dijaga tanpa ada yang memakainya. Prioritasnya seperti yang ditetapkan — migrasikan
data existing, satu kode kanonik, legacy tidak dipakai lagi.

Modul legacy **dinonaktifkan, bukan dihapus**, sehingga jejaknya tetap dapat ditelusuri bila ada
yang perlu memahami mengapa sebuah izin lama menunjuk ke sana.
