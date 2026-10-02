# Laporan Perubahan Backend — `BE-FIN-042`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-042` |
| Judul | Penyelarasan 6 controller legacy Finance ke nama kanonikal `Finance*`, seeder ekspansi payung `Finance.AP`/`Finance.AR`, dan skrip SQL migrasi data peran |
| Slice | `POST-MVP` (`REV-6/8`) — Governance & Keamanan |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task) |
| Trace | `FIN-DEC-078`, `FIN-DEC-079`; `FIN-DES-061`..`063`; `FIN-CQ-08`, `FIN-CAP-043`, `FIN-OQ-036` |
| Contract version | `permission-audit-matrix.md` `FIN-PERM-1.3` §D (AMENDMENT REVISI 6, `approved` 28 September 2026) |
| Dependency | `BE-FIN-009` ✅, `BE-FIN-012` ✅, `BE-FIN-018` ✅, `BE-FIN-019` ✅, `BE-FIN-020` ✅ — seluruhnya selesai |
| Klasifikasi | `MEDIUM` (untuk bagian yang dikerjakan — §D.5 dan §D.6.1) — satu repository (skor 0); 8 berkas diperiksa domain terdekat (skor 0), namun >8 total dibaca lintas kontrak dan skema otorisasi nyata (skor 1); 6 berkas diubah + 1 skrip SQL baru (skor 1); logika bisnis sederhana — string rename murni, nol logika baru (skor 0); kontrak API — nol endpoint berubah bentuk, hanya nama resource (skor 0); database — nol migration EF Core, satu skrip SQL data terpisah (skor 1); keamanan/auth — **mengubah string otorisasi berjalan** pada 6 controller (skor 2); UI/workflow — nol dampak (skor 0). Total 5 → `MEDIUM`. **§D.6.2 (seeder payung) TIDAK dikerjakan — lihat bagian 7** |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — 6 controller `Areas/Corporate/FinanceManagement/**/Controllers/*.cs`, `Migrations/scripts/be-fin-042-role-permissions-migration.sql` (baru), `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` (tanda status) |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Belum di-commit — working tree `Yasmina` |
| Tanggal | 29 September 2026 |
| Status | 🟡 **SEBAGIAN — DENGAN TEMUAN BLOCKER.** §D.5 (rename 6 controller) dan §D.6.1 (skrip migrasi data) selesai, dengan **koreksi teknis** atas isi skrip kontrak yang tidak dapat dieksekusi terhadap skema nyata (lihat bagian 7). §D.6.2 (seeder ekspansi payung `Finance.AP`/`Finance.AR`) **BLOCKED** — bentrok nama dengan controller nyata yang sudah berjalan, dan menuntut arsitektur otorisasi baru yang belum ada preseden di codebase ini. `dotnet build` **belum dijalankan** untuk perubahan task ini secara spesifik |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, 6 controller Finance yang paling awal dibangun (`FinancePaymentsController`, dkk.) memakai nama resource pendek (`"Payment"`, `"Receipt"`, dst.) pada atribut `[AccessPermission]`, berbeda dari 7 controller Purchasing yang lebih baru yang sudah memakai nama kanonikal berawalan `"Finance"` (`"FinancePurchaseOrder"`, dkk.). Ketidakkonsistenan ini menyulitkan admin membaca layar Akses Role (dua gaya penamaan berdampingan untuk modul yang sama) dan berpotensi membingungkan integrasi/dokumentasi eksternal yang mengasumsikan kontrak `permission-audit-matrix.md` sebagai kebenaran.

**Contoh konkret.** Admin yang mencari "siapa yang boleh mengalokasikan penerimaan" akan menemukan resource bernama `Receipt` di layar Akses Role, padahal dokumentasi API dan controller Purchasing lain memakai pola `Finance<Domain>`. Sesudah task ini, keenam controller itu konsisten dengan pola yang sama: `FinancePayment`, `FinanceReceipt`, `FinanceReceivable`, `FinanceSupplierPayable`, `FinanceBillingIntake`, `FinanceAccountingEvent`.

---

## 2. Proses bisnis

Task ini murni **penyelarasan nama** (rename), bukan perubahan proses bisnis. Tidak ada langkah pengguna baru; tidak ada endpoint baru; tidak ada validasi baru. Efek yang terlihat pengguna:

- Sebelum migrasi data dijalankan: **tidak ada** — kode C# saja belum mengubah apa pun di database.
- Sesudah aplikasi di-deploy dan `AccessMenuSeeder` berjalan (otomatis saat startup, `Program.cs` baris 1448): resource lama tertutup di layar Akses Role, resource baru muncul. **Sampai skrip SQL migrasi data dijalankan**, staf yang sebelumnya diberi hak pada resource lama akan mendapati `403 Forbidden` pada endpoint terkait — inilah alasan skrip §D.6.1 (dikoreksi, lihat bagian 3.3) wajib dijalankan **dalam jendela pemeliharaan yang sama** dengan deployment.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/contracts/permission-audit-matrix.md` §D (AMENDMENT REVISI 6, seluruh D.1–D.6)
- Enam controller sasaran (lihat 3.2), termasuk seluruh atribut `[AccessAction]`/`[AccessPermission]`-nya
- `Attributes/AccessControllerAttribute.cs` — konstruktor `(moduleCode, moduleName, displayName)` + properti bernama `ControllerName` (bukan argumen posisi ke-3, yang sebenarnya `displayName`)
- `Services/Security/PermissionRegistryDescriptor.cs` — `ResolveControllerName` membaca properti `ControllerName`, bukan nama kelas C#; ada validasi startup yang mencocokkan setiap `[AccessPermission]` terhadap `[AccessAction]` yang dideklarasikan
- `Seeders/AccessMenuSeeder.cs` — **hanya** mengelola registry (`SysApplicationModule`/`SysControllerAccess`/`SysActionAccess`); **tidak pernah** menulis `SysAccessPolicy` (baris komentar kelasnya eksplisit menyatakan ini)
- `Models/SysAccessPolicy.cs`, `Services/Security/AccessPermissionService.cs` — mekanisme otorisasi nyata: **Departemen+Posisi**, bukan "Role"; grant disimpan sebagai FK `(ControllerAccessId, ActionAccessId)`, bukan string nama resource
- `Areas/Corporate/FinanceManagement/Payable/Controllers/FinanceApController.cs`,
  `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceArController.cs` — controller **V2 nyata yang sudah berjalan**, memakai persis nama resource `Finance.AP`/`Finance.AR` yang kontrak ingin jadikan payung baru
- `Migrations/scripts/be-sec-003b-policy-expansion.sql`, `be-sec-014-pre-seeder-revoke-finance-workschedule.sql` — preseden pola migrasi data rename/pencabutan hak di modul `platform-authorization` (BE-SEC-003B/012/013/014), dipakai sebagai acuan struktur skrip §3.3

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Payable/Controllers/FinancePaymentsController.cs` | `ControllerName`: `Payment` → `FinancePayment`; 12 atribut `[AccessPermission]` diselaraskan; komentar kelas diperbarui |
| `Areas/Corporate/FinanceManagement/Collection/Controllers/FinanceReceiptsController.cs` | `ControllerName`: `Receipt` → `FinanceReceipt`; 5 atribut `[AccessPermission]` diselaraskan; komentar kelas diperbarui |
| `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceReceivablesController.cs` | `ControllerName`: `Receivable` → `FinanceReceivable`; 10 atribut `[AccessPermission]` diselaraskan |
| `Areas/Corporate/FinanceManagement/Payable/Controllers/FinanceSupplierPayablesController.cs` | `ControllerName`: `SupplierPayable` → `FinanceSupplierPayable`; 6 atribut `[AccessPermission]` diselaraskan; komentar kelas diperbarui |
| `Areas/Corporate/FinanceManagement/BillingIntake/Controllers/FinanceBillingIntakeController.cs` | `ControllerName`: `BillingIntake` → `FinanceBillingIntake`; 6 atribut `[AccessPermission]` diselaraskan |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Controllers/FinanceAccountingEventsController.cs` | `ControllerName`: `AccountingEvents` (jamak) → `FinanceAccountingEvent` (tunggal); 4 atribut `[AccessPermission]` diselaraskan |
| `Migrations/scripts/be-fin-042-role-permissions-migration.sql` | **Baru** — skrip migrasi data (bukan migration EF Core) untuk melestarikan `SysAccessPolicy` lintas rename, ditulis ulang dari nol karena isi kontrak tidak dapat dieksekusi (lihat 3.3) |

**Catatan cakupan rename**: hanya string RESOURCE (argumen ke-1 `[AccessPermission]` dan properti `ControllerName`) yang diubah. Nama ACTION (argumen ke-2) **tidak disentuh** — beberapa di antaranya sudah menyimpang dari daftar aksi granular §D.3 kontrak (mis. `Receipt` memakai `Allocate` untuk aksi buat **dan** balik, bukan `Create`/`Reverse` terpisah; `BillingIntake` memakai `Sync`/`Process`, bukan `Consume`). Ini gap pra-ada dari task pembangunan controllernya masing-masing (`BE-FIN-018`/`BE-FIN-016`), **bukan** bagian literal `FIN-DEC-078` yang hanya menyebut penyelarasan **nama resource**, dan tidak diperbaiki di sini — mengubah nama aksi berarti mengubah kontrak `[AccessAction]`/`[AccessPermission]` di layar Akses Role secara lebih luas daripada wewenang task ini.

### 3.3 Koreksi teknis atas skrip migrasi kontrak (§D.6.1)

Kontrak `permission-audit-matrix.md` §D.6.1 menuliskan skrip:

```sql
UPDATE "SysRolePermissions" SET "ResourceName" = 'FinancePayment' WHERE "ResourceName" = 'Payment';
-- ...dst enam baris serupa
```

**Skrip ini tidak dapat dieksekusi terhadap database backend ini** — tabel `SysRolePermissions` dan kolom `ResourceName` **tidak ada** pada skema nyata (`relation "SysRolePermissions" does not exist`). Mekanisme hak akses nyata:

| Asumsi kontrak (§D.2, §D.6) | Kenyataan di source |
| --- | --- |
| Hak akses disimpan per "Role" pada tabel `SysRolePermissions` dengan kolom string `ResourceName` | Hak akses disimpan pada `SysAccessPolicy` (`Models/SysAccessPolicy.cs`) sebagai **FK** `(DepartmentId, PositionId, ControllerAccessId, ActionAccessId)` — berbasis **Departemen + Posisi**, bukan Role, dan menunjuk registry lewat Id, bukan string |
| Rename `ControllerName` di kode otomatis "berpindah" karena stringnya sama | `AccessMenuSeeder` **membuat baris registry BARU** (Id baru) untuk nama baru dan **menutup** baris lama (`IsActive=false`) — ia tidak pernah mengganti nilai `ControllerName` pada baris yang sudah ada. `SysAccessPolicy` yang sudah ada tetap menunjuk Id LAMA sampai dimigrasikan secara eksplisit |

Skrip pengganti yang benar (`Migrations/scripts/be-fin-042-role-permissions-migration.sql`) mengikuti pola yang **sudah established** di modul `platform-authorization` (`be-sec-003b-policy-expansion.sql`, seri BE-SEC-003B/012/013/014): dry-run (Bagian 1, baca saja), Tahap 1 melestarikan `SysAccessPolicy` ke Id registry baru (INSERT idempotent, digerbang prasyarat registry baru harus sudah aktif), verifikasi parity (Bagian 3), Tahap 2 menonaktifkan `SysAccessPolicy` yang menunjuk Id lama (**setelah** parity terbukti manusia), dan bagian rollback. Dua tahap sengaja dua transaksi terpisah — kegagalan pembuktian parity di Tahap 1 tidak boleh ikut mencabut hak lama di Tahap 2.

### 3.4 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Nol perubahan bentuk endpoint — hanya string resource hak akses |
| Database | Nol migration EF Core. Satu skrip SQL **data** terpisah (`Migrations/scripts/be-fin-042-role-permissions-migration.sql`), dijalankan manual, terpisah dari `dotnet ef` |
| Keamanan/Auth | **MENGUBAH STRING OTORISASI BERJALAN** pada 6 controller. Wajib dieksekusi sebagai satu paket rilis: (1) deploy source, (2) start aplikasi sekali dalam jendela pemeliharaan agar seeder membuat registry baru, (3) jalankan skrip migrasi data Tahap 1, (4) verifikasi parity, (5) Tahap 2. Melewatkan langkah 3-5 membuat pemegang hak lama tertolak `403` begitu registry lama ditutup seeder pada langkah 2 |

---

## 4. Dokumentasi endpoint

Tidak ada endpoint baru atau berubah bentuk. Tabel berikut merangkum resource yang berganti nama (bukan kontrak endpoint baru):

| Controller | Resource lama | Resource baru |
| --- | --- | --- |
| `FinancePaymentsController` | `Payment` | `FinancePayment` |
| `FinanceReceiptsController` | `Receipt` | `FinanceReceipt` |
| `FinanceReceivablesController` | `Receivable` | `FinanceReceivable` |
| `FinanceSupplierPayablesController` | `SupplierPayable` | `FinanceSupplierPayable` |
| `FinanceBillingIntakeController` | `BillingIntake` | `FinanceBillingIntake` |
| `FinanceAccountingEventsController` | `AccountingEvents` | `FinanceAccountingEvent` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | — | `NOT RUN` | Belum diminta secara spesifik untuk task ini pada giliran ini |
| Setiap `[AccessPermission]` argumen ke-1 = `ControllerName` pada `[AccessController]` yang sama, per controller | Cocok pada keenam controller (review manual, dipandu grep) | `NOT RUN` (review kode) | Grep `AccessPermission\("(Payment\|Receipt\|Receivable\|SupplierPayable\|BillingIntake\|AccountingEvents)"` di seluruh source menghasilkan **nol** sisa selain dokumentasi historis (laporan task lama) |
| Startup validation `PermissionRegistryDescriptor` (mencocokkan `[AccessPermission]` vs `[AccessAction]`) | Diperkirakan lolos — tidak ada `[AccessAction]` yang diubah, hanya string resource | `NOT RUN` (perlu `dotnet run` untuk membuktikan) | — |
| Skrip SQL — sintaks dan struktur dry-run | Ditinjau ulang mengikuti pola `be-sec-003b-policy-expansion.sql` yang sudah terbukti dipakai | `NOT RUN` terhadap database sungguhan | Review kode skrip |

**AUTOMATED TEST:** `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

**Tidak dijalankan:** `dotnet build`, eksekusi skrip SQL terhadap database mana pun, dan `Invoke-QbeConformanceCheck.ps1` — ketiganya menunggu instruksi eksplisit terpisah.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (dari kartu roadmap) | Status | Bukti |
| --- | --- | --- |
| 1. Seluruh 6 controller memakai nama resource berawalan `"Finance"`; nol string pendek tersisa | **Terpenuhi** | Diff 6 controller; grep membuktikan nol sisa di source |
| 2. Seeder RBAC memperluas payung `Finance.AP` ke 9 resource granular dan `Finance.AR` ke 4 resource granular | **TIDAK terpenuhi — BLOCKED** | Lihat bagian 7. Arsitektur ini tidak dibangun |
| 3. Skrip SQL idempotent siap dieksekusi DBA | **Terpenuhi, dengan koreksi isi** | `Migrations/scripts/be-fin-042-role-permissions-migration.sql` — isi ditulis ulang agar sesuai skema nyata (lihat 3.3), tetap idempotent dan mengikuti pola established `be-sec-003b` |

Task ini **belum** dapat ditandai `✅` — kriteria #2 belum terpenuhi sama sekali, dan kriteria #1/#3 belum diverifikasi lewat `dotnet build`/eksekusi nyata.

---

## 7. Catatan penutup — TEMUAN KRITIS: §D.6.2 BLOCKED, dikembalikan ke pass desain

**Ini bukan gap implementasi biasa — ini benturan arsitektur yang tidak dapat diselesaikan sepihak oleh task build.**

### 7.1 Bentrok nama: `Finance.AP`/`Finance.AR` sudah dipakai controller nyata

Kontrak §D.2 menetapkan `Finance.AP` dan `Finance.AR` sebagai **resource payung baru** yang akan diberi mekanisme ekspansi otomatis ke 9/4 resource granular. **Kedua nama itu sudah dipakai** oleh controller yang benar-benar berjalan hari ini:

- `FinanceApController` (`Areas/Corporate/FinanceManagement/Payable/Controllers/FinanceApController.cs`, route `api/finance/payable`) — `[AccessController(..., ControllerName = "Finance.AP", ...)]`, aksi nyata `View`, `Payment`, dst. — "Manajemen Utang Finance V2".
- `FinanceArController` (`Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceArController.cs`, route `api/finance/receivable`) — `[AccessController(..., ControllerName = "Finance.AR", ...)]`, aksi nyata `View`, `Payment`, dst. — "Manajemen Piutang Finance V2".

Kontrak tidak menyebut kedua controller ini sama sekali — indikasi kuat bahwa audit yang mendasari `FIN-DEC-079` tidak menemukan keduanya. Membangun "payung `Finance.AP`/`Finance.AR`" seperti dirancang kontrak berarti menimpa arti resource yang **sudah dipegang** endpoint V2 nyata — staf yang hari ini diberi hak `Finance.AP : View` (untuk memakai `FinanceApController`) akan tiba-tiba **juga** memegang 9 resource granular Purchasing/AP bila mekanisme ekspansi dibangun sesuai kontrak, tanpa keputusan admin mana pun.

### 7.2 Mekanisme ekspansi yang diminta tidak punya tempat di arsitektur berjalan

`AccessMenuSeeder.cs` (baris 8-14, komentar kelas) menyatakan eksplisit: seeder ini **hanya** mengelola tiga tabel registry (`SysApplicationModule`/`SysControllerAccess`/`SysActionAccess`) dan **tidak pernah** membuat `SysAccessPolicy` — "Kemampuan yang baru terdaftar tetap ditolak untuk semua orang sampai admin memberikannya lewat layar Akses Role." §D.6.2 kontrak meminta hal yang bertentangan langsung dengan invariant ini: "seeder secara otomatis mendistribusikan... ke dalam tabel izin peran" — yaitu MEMBUAT `SysAccessPolicy` baru secara implisit dari satu grant payung. Ini genuinely **arsitektur otorisasi baru** (materialized permission expansion), bukan pekerjaan pemetaan nama:

- Tidak ada preseden apa pun untuk "grant payung yang mengembang ke banyak grant granular" di seluruh codebase — modul `platform-authorization` (BE-SEC series) yang paling dekat menanganinya justru melakukan sebaliknya: memecah SATU identitas granular jadi BANYAK identitas granular baru (bukan payung→granular), dan seluruhnya lewat migrasi data yang ditinjau manusia per pasangan Departemen+Posisi — bukan lewat mekanisme otomatis yang berjalan diam-diam di seeder atau runtime.
- `AccessPermissionService.HasAccessAsync` (jalur pemeriksaan hak akses nyata) melakukan pencarian **langsung**: apakah ada baris `SysAccessPolicy` persis untuk `(DepartmentId, PositionId, ControllerAccessId, ActionAccessId)` yang diminta. Tidak ada logika "bila punya payung X maka anggap juga punya granular Y" di jalur ini sama sekali.

### 7.3 Kenapa ini dikembalikan, bukan diselesaikan sendiri

Build-module-backend **eksplisit melarang** mengarang aturan bisnis/kewenangan yang belum diputuskan (instruksi skill, langkah 4 dan 6). Membangun mekanisme ekspansi payung berarti:
1. Memutuskan sendiri bagaimana konflik nama `Finance.AP`/`Finance.AR` diselesaikan (mengganti nama controller V2, atau menggabungkan artinya) — **keputusan produk**, bukan teknis.
2. Merancang mekanisme otorisasi baru (kapan ekspansi terjadi — saat seeder, saat grant diberikan admin, atau saat runtime request — dan apakah ia reversibel) — **keputusan arsitektur keamanan**, persis kategori yang menuntut `design-business-module`, bukan `build-module-backend`.

### 7.4 Rekomendasi

Kembalikan §D.6.2 ke pass desain (`design-business-module` atau `grill-me` Amendment) dengan pertanyaan konkret: (a) apakah `Finance.AP`/`Finance.AR` V2 controller di-retire/di-rename lebih dulu, atau payung dipetakan ke nama lain (mis. `Finance.AP.Umbrella`); (b) di titik mana ekspansi terjadi — seeder, event saat admin memberi grant di layar Akses Role, atau runtime; (c) apakah ekspansi bersifat snapshot (materialized, seperti BE-SEC-003B) atau live (dihitung setiap request). Bagian §D.5 dan §D.6.1 task ini (rename + migrasi data) **berdiri sendiri** dan aman dirilis tanpa menunggu keputusan ini.

| Hal | Isi |
| --- | --- |
| Risiko tersisa | Nol untuk bagian yang dikerjakan (§D.5/§D.6.1) — rename string murni, migrasi data mengikuti pola established. Risiko §D.6.2 didokumentasikan di atas, bukan dieksekusi |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 6 controller `M`, 1 skrip SQL `??` pada `git status --short` |
| Langkah berikutnya | (1) `dotnet build`; (2) deploy + jalankan skrip migrasi mengikuti urutan wajib di kepala skrip (jendela pemeliharaan); (3) bawa temuan §7 ke pemilik produk/keamanan untuk memutuskan resolusi `Finance.AP`/`Finance.AR` sebelum §D.6.2 dikerjakan ulang sebagai task/desain terpisah |
