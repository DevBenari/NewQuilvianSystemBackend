# `BE-OPR-011` — verifikasi kontrak otorisasi Operasi

| Hal | Isi |
|---|---|
| Status | **Implemented / Contract Verified / Runtime Acceptance Blocked by Environment** |
| Tanggal | 5 Oktober 2026 |
| Basis kode | `QuilvianIntegrationBackend` `7787318d` (merge PR #234), `Ikbal` disinkronkan penuh |
| Build | 0 error |
| Uji Operasi | 117 / 117 |
| Uji khusus izin | 41 / 41 (`PermissionMatrixTests`, `ReportPermissionTests`) |

Dokumen ini mencatat apa yang **berhasil** dibuktikan tanpa runtime HTTP, dan apa yang
**tidak dapat** dibuktikan tanpa lingkungan yang hidup. Dibuat karena runtime proof `403`
terhalang blocker startup di luar modul Operasi —
[`blocker-startup-seeder-tabel-hilang.md`](../../engineering/blocker-startup-seeder-tabel-hilang.md).

## 1. Cakupan penjagaan: 36 dari 36 endpoint

Sepuluh berkas controller, sembilan di antaranya memuat endpoint
(`OperatingRoomControllerResults.cs` hanya penolong balasan).

| Controller | Endpoint | `[AccessPermission]` | `[Authorize]` kelas | `AllowAnonymous` |
|---|---|---|---|---|
| `OperatingRoomCaseController` | 6 | 6 | ya | 0 |
| `OperatingRoomExecutionController` | 3 | 3 | ya | 0 |
| `OperatingRoomIntegrationController` | 5 | 5 | ya | 0 |
| `OperatingRoomMaterialController` | 2 | 2 | ya | 0 |
| `OperatingRoomPreparationController` | 4 | 4 | ya | 0 |
| `OperatingRoomRecoveryController` | 7 | 7 | ya | 0 |
| `OperatingRoomReportController` | 3 | 3 | ya | 0 |
| `OperatingRoomScheduleController` | 4 | 4 | ya | 0 |
| `OperatingRoomStockSourceController` | 2 | 2 | ya | 0 |
| **Total** | **36** | **36** | **9 / 9** | **0** |

Pemeriksaan dilakukan dengan menyusuri setiap atribut `[HttpGet|Post|Put|Patch|Delete]` dan
menuntut adanya `[AccessPermission]` sebelum tanda tangan metodenya. **Tidak ada satu pun
endpoint tanpa izin**, dan pencarian `AllowAnonymous` pada seluruh direktori Operasi bernilai
nol.

## 2. Izin yang diminta tiap endpoint

19 pasangan `resource:action` unik dipakai oleh 36 atribut.

| Endpoint | Izin yang diminta |
|---|---|
| `GET cases`, `GET cases/{id}` | `OperatingRoomCase:Read` |
| `POST cases` | `OperatingRoomCase:Create` |
| `PUT cases/{id}` | `OperatingRoomCase:Update` |
| `PATCH cases/{id}/cancel` | `OperatingRoomCase:Cancel` |
| `PATCH cases/{id}/start` | `OperatingRoomExecution:Update` |
| `GET .../execution/operation-record` | `OperatingRoomCase:Read` |
| `PUT .../execution/operation-record`, `POST .../addenda` | `OperatingRoomExecution:Update` |
| `GET .../execution/materials` | `OperatingRoomMaterial:Read` |
| `POST .../execution/materials` | `OperatingRoomMaterial:Update` |
| `GET .../execution/anesthesia-record`, `GET .../recovery` | `OperatingRoomAnesthesia:Read` |
| `PUT .../execution/anesthesia-record`, `PUT .../recovery` | `OperatingRoomAnesthesia:Update` |
| `GET .../execution/handovers` | `OperatingRoomHandover:Read` |
| `POST .../handovers`, `PATCH .../handovers/{id}/accept` | `OperatingRoomHandover:Update` |
| `GET .../preparation` | `OperatingRoomPreparation:Read` |
| `PUT .../checklists/{phase}`, `POST .../sign-offs`, `POST .../emergency-bypass` | `OperatingRoomPreparation:Update` |
| `POST .../integration/inventory/dispatch` | `OperatingRoomIntegration:Dispatch` |
| `GET .../integration/reconciliation`, `GET .../deliveries/{id}/event` | `OperatingRoomIntegration:Read` |
| `PATCH .../deliveries/{id}/attempts`, `PATCH .../retry` | `OperatingRoomIntegration:Update` |
| `GET cases/{id}/schedule`, `GET .../schedule/history` | `OperatingRoomCase:Read` |
| `PATCH cases/{id}/schedule`, `PATCH cases/{id}/postpone` | `OperatingRoomSchedule:Update` |
| `GET stock-sources` | `OperatingRoomStockSource:Read` |
| `PUT stock-sources` | `OperatingRoomStockSource:Update` |
| `GET reports/operations`, `GET reports/utilization` | `OperatingRoomCase:Read` |
| `GET reports/materials` | `OperatingRoomMaterial:Read` |

Dua pilihan yang **sengaja** menyilang nama controllernya, dan keduanya benar secara domain:

- `PATCH cases/{id}/start` menuntut `OperatingRoomExecution:Update`, bukan
  `OperatingRoomCase:Update` — memulai operasi adalah tindakan pelaksanaan, bukan penyuntingan
  data kasus;
- `GET .../execution/operation-record` menuntut `OperatingRoomCase:Read` — membaca laporan
  operasi setara dengan membaca kasusnya.

## 3. Laporan material memang menuntut izin material

Tuntutan khusus yang diminta pemilik kebutuhan, dan terbukti:

```csharp
// OperatingRoomReportController.cs
[AccessPermission("OperatingRoomCase", "Read")]      // baris 29  — reports/operations
[AccessPermission("OperatingRoomCase", "Read")]      // baris 47  — reports/utilization
[AccessPermission("OperatingRoomMaterial", "Read")]  // baris 63  — reports/materials
```

Laporan material **tidak** memakai `OperatingRoomCase`. Dijaga uji
`Laporan_material_memakai_izin_material_bukan_izin_kasus`, sementara
`Setiap_laporan_hanya_menuntut_satu_izin` mencegahnya diperlebar diam-diam.

## 4. Kecocokan terhadap registry izin di basis data

Dibaca **read-only** dari `localhost / QuilvianNewDevIkbalFr`. Tidak ada satu pun perintah
tulis.

```
pasangan resource:action diminta atribut     : 19
pasangan aktif di registry (SysActionAccess) : 19
cocok dua arah                               : 19
diminta atribut tapi tidak aktif di registry : 0   <- yang penting
aktif di registry tapi tak dipakai endpoint  : 0
```

Nol pada baris keempat adalah intinya: bila satu pasangan saja tidak terdaftar aktif,
`HasAccessAsync` mengembalikan `false` bagi **semua** orang dan endpointnya mati total walaupun
pemetaan perannya benar. Tidak ada yang seperti itu.

Empat entri registry lama bertanda `IsDelete = true` — `OperatingRoomRecovery:Read/Update`,
`OperatingRoomReport:Read`, `OperatingRoomExecution:Read`, `OperatingRoomSchedule:Read` —
**tidak dipakai atribut mana pun**, jadi sisa itu tidak berbahaya. Nama controller
`OperatingRoomRecovery` dan `OperatingRoomReport` juga tidak dipakai: `RecoveryController`
memakai `OperatingRoomAnesthesia` dan `OperatingRoomHandover`, `ReportController` memakai
`OperatingRoomCase` dan `OperatingRoomMaterial`.

## 5. Jalur keputusan penolakan

| Lapis | Berkas | Perilaku |
|---|---|---|
| Autentikasi | `AccessPermissionFilter.cs:47` | belum login → `401` |
| Otorisasi | `AccessPermissionFilter.cs:98,104` | login tetapi tidak berizin → `403` |
| Saklar | `AccessPermissionService.cs:37` | `Security:Authorization:Enabled=false` hanya berlaku di luar produksi |
| Pesan | `AccessPermissionAttribute` | `DeniedCode`/`DeniedMessage` hanya mengganti isi balasan, **tidak** ikut memutuskan boleh atau tidak |

Lapis terakhir layak dicatat: karena kode dan kalimat penolakan tidak pernah ikut menentukan
keputusan, tidak ada pemeriksaan hak akses kedua yang dapat menyimpang dari yang pertama.

## 6. Yang tidak dapat dibuktikan, dan mengapa

Startup mati sebelum Kestrel mengikat port, pada `MstNursingDiagnosisSeeder`
(`42P01 MstNursingDiagnosisGroup does not exist`).

Di luar itu, pembacaan read-only menunjukkan hambatan kedua yang berdiri sendiri:
`SysAccessPolicy` **0 baris** dan `AspNetUserOrganization` **0 baris**, sementara
`SysControllerAccess` 451 dan `SysActionAccess` 1716. Artinya seandainya startup diperbaiki
hari ini, setiap pengguna non-SuperAdmin akan tetap ditolak `403` di **seluruh** sistem — sisi
`403` terpicu secara hampa, dan sisi `200` tidak mungkin dibuktikan. Acceptance menuntut
keduanya.

Akun dan role uji sengaja **tidak** dibuat: mekanisme sahnya adalah endpoint administrator
`role-access/policies`, dan endpoint itu ikut mati bersama aplikasinya. Menyuntikkannya lewat
SQL akan menghasilkan data yang tidak melewati pipeline yang justru sedang diuji.

## 7. Yang diperlukan dari pemilik lingkungan

1. aplikasi dapat dijalankan pada basis data dev — Jalur A (jalankan migration tertinggal) atau
   Jalur B (buat seeder startup tidak fatal) pada dokumen blocker;
2. baris `AspNetUserOrganization` bagi akun uji, agar tiap akun punya `DepartmentId` +
   `PositionId`;
3. baris `SysAccessPolicy` yang **memberi** izin pada sebagian pasangan Operasi dan **menahan**
   sisanya — tanpa kontras itu `403` tidak membedakan apa pun;
4. jabatan dan departemen klinis pada master data; empat role yang ada (`SuperAdmin`, `User`,
   `Manajer Finance`, `Supervisor Finance`) tidak memuat peran bedah, anestesi, atau perawat.

Begitu keempatnya tersedia, yang tersisa hanyalah menjalankan matriks `200 / 403 / 401` dengan
akun non-SuperAdmin. Kontraknya sendiri sudah tidak menyisakan pertanyaan.
