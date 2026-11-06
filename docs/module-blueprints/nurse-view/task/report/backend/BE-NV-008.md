# Laporan Perubahan Backend — `BE-NV-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-NV-008` |
| Judul | Fondasi data tablet bed dan panggilan pasien |
| Slice | `MVP-5` — fondasi data revision 2 (gelombang backend 1) |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/backend-roadmap.md` — bagian Revision 2, kartu `BE-NV-008` |
| Trace | `NV-DEC-035`, `039`, `040`, `048`, `049`, `050`, `053`, `055`; `AD-01`, `AD-03`; `G-10`, `G-15`, `G-17`; `NFR-009`, `011`, `014`; `02-backend-architecture.md` R2.3, R2.4.1–R2.4.4, R2.4.7, R2.7, R2.8; `data/data-dictionary.md` R2.2, R2.4 |
| Contract version | `0.2.0` — `approved` (Rizki Gunawan, 2026-11-05, `NV-DEC-055`) |
| Dependency | Tidak ada. Revision 1 (`BE-NV-001`–`007`, seluruhnya ✅) menjadi titik awal |
| Klasifikasi | `MEDIUM` — skor 6: satu repository (0), berkas diperiksa > 20 termasuk dokumen governance dan blueprint (2), berkas diubah > 8 (2), logika sederhana (0), kontrak API tidak ada (0), dampak schema/entity (2), auth tidak ada (0), UI tidak ada (0) |
| Task mode | `BACKEND` — atas persetujuan Rizki Gunawan 2026-11-05 ("lanjutkan saya setujui") sesudah roadmap revision 2 disetujui |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/NurseViewManagement/{Models,Enums}`, `Repositories/Configurations/HealthServices/NurseViewManagement/`, `Repositories/ApplicationDbContext.cs`, laporan ini, serta baris status pada roadmap dan traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `0c6eab34` (branch `rizkiG`, upstream `origin/rizkiG`); perubahan belum di-commit |
| Tanggal | 2026-11-05; diperbarui 2026-11-06 sesudah migration diterapkan |
| Status | ✅ **SELESAI** — 6 November 2026. Source dan QBE Strict `PASS` (5 Nov); build, migration `20261106015006_AddNurseViewPatientCall`, dan `database update` dijalankan Rizki (6 Nov); migration diperiksa bersih dan hasilnya di DB dev diverifikasi lewat `SELECT` read-only. Riwayat: 🟡 SEBAGIAN 5 November 2026 karena build dan migration belum dijalankan |

---

## 1. Masalah yang diperbaiki

Revision 2 Nurse View menambah tablet di setiap bed agar pasien bisa memanggil perawat. Sebelum
task ini, sistem belum punya tempat menyimpan empat hal yang dibutuhkan:

1. **tablet bed**: tablet mana terpasang di bed mana, dan akun login-nya;
2. **panggilan pasien**: siapa yang memanggil, kapan, apakah Darurat, siapa perawat yang menuju;
3. **jejak panggilan**: setiap kejadian beserta pelaku dan jamnya, sebagai dasar waktu tanggap;
4. **ambang lama menunggu**: angka menit dari Komite Keperawatan untuk warna kuning dan merah.

Contoh akibatnya: tanpa tabel panggilan, tombol "Panggil perawat" di tablet bed 305-A tidak punya
tujuan penyimpanan. Strip TV dan Konsol Panggilan pun tidak punya data untuk ditampilkan.

---

## 2. Proses bisnis

Task ini hanya membangun fondasi data; belum ada layar atau endpoint. Aturan berikut sudah dijaga
di tingkat **database**, sehingga tetap berlaku walau ada kode yang keliru di task berikutnya.

| Aturan | Penjaga | Contoh |
| --- | --- | --- |
| Kode tablet unik | Unique `IX_NvwBedTablet_DeviceCode` | Dua tablet `NVT-000001` ditolak database |
| Satu akun login paling banyak satu tablet | Unique berfilter `LoginUserId IS NOT NULL` | Akun `nvt-000001` tidak bisa dipasang ke dua tablet |
| Satu bed paling banyak satu tablet yang belum nonaktif (`AD-01`) | Unique berfilter `IX_NvwBedTablet_BedId_InUse` (`"DeviceStatus" <> 3 AND "IsDelete" = false`) | Bed 305-A sudah punya `NVT-000001` yang `Active`; tablet kedua untuk 305-A ditolak. Setelah `NVT-000001` dinonaktifkan (`3`), tablet pengganti boleh didaftarkan |
| Satu bed paling banyak satu panggilan aktif (`NV-DEC-039`) | Unique berfilter `IX_NvwPatientCall_BedId_Active` (`"CallStatus" IN (1, 2) AND "IsDelete" = false`) | 305-A sedang `Waiting` (`1`); baris `Waiting` kedua ditolak. Begitu panggilan itu `Handled` (`3`), panggilan baru boleh lahir |
| Permintaan tablet yang terkirim ulang tidak membuat jejak kedua (`NV-AC-039`) | Unique berfilter `IdempotencyKey IS NOT NULL` | Wi-Fi tersendat, permintaan dengan kunci yang sama terkirim dua kali; jejak kedua ditolak |
| Dua perubahan bersamaan pada tablet atau panggilan | Concurrency token `Version` | Sari dan Dewi mengklaim 305-A pada detik yang sama; yang kalah balapan ditolak saat disimpan |
| Ambang kuning 1–240 menit, merah lebih besar dan paling banyak 480 | Check constraint `CK_NvwPatientCallWaitThreshold_Order` | Kuning 3, merah 2 ditolak; kuning 3, merah 5 diterima |
| Satu ambang aktif per lingkup | Unique berfilter `ScopeCode` (`"IsDelete" = false`) | Dua ambang `GLOBAL` aktif ditolak; baris lama yang ditandai hapus tidak dihitung |
| Tidak ada angka ambang bawaan (`G-15`) | Tabel lahir kosong; tidak ada seed | Selama Komite belum mengesahkan, panggilan tampil tanpa warna |
| Data panggilan tidak terhapus berantai | Seluruh FK `Restrict` | Bed, tablet, atau penempatan yang masih dirujuk panggilan tidak bisa dihapus fisik |

Status yang disiapkan (nilai angka sama persis dengan `02-backend-architecture.md` R2.7):

| Data | Status | Arti |
| --- | --- | --- |
| Tablet | `PendingLogin = 1` → `Active = 2` ↔ `Inactive = 3` | Sama dengan TV Nurse View |
| Panggilan | `Waiting = 1`, `Claimed = 2` (aktif); `Handled = 3`, `Cancelled = 4`, `ClosedBySystem = 5` (akhir) | Status akhir tidak dibuka ulang (`G-12`) |
| Prioritas | `Normal = 1`, `Emergency = 2` | Hanya boleh naik (`G-13`), ditegakkan service di `BE-NV-011` |

Jalur tidak normal — kirim ganda, klaim bersamaan, bed kosong, pasien pindah — baru punya pesan dan
perilaku di `BE-NV-009` s.d. `BE-NV-016`. Yang disiapkan di sini adalah penjaganya di database.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Governance: `NewQuilvianSystemBackend/AGENTS.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`
  (baris 45 `Nvw` `ACTIVE`); suite skill `rules/backend/engineering/BACKEND_ENGINEERING_CONTRACT.md`,
  `TASK_RULES.md`, `TASK_CLASSIFICATION.md`, `DATABASE_RULES.md`, `REVIEW_RULES.md`,
  `REPORT_TEMPLATE.md`, `TEST_POLICY.md`; `tooling/qbe/README.md`
- Blueprint: `02-backend-architecture.md` Revision 2, `data/data-dictionary.md` R2,
  `roadmap/backend-roadmap.md` kartu `BE-NV-008`, laporan `BE-NV-001.md` (pola dan konvensi)
- Pola terdekat: `Areas/HealthServices/NurseViewManagement/Models/NvwDisplayDevice.cs`,
  `NvwVitalSignInterval.cs`, `Enums/NvwDisplayDeviceStatus.cs`;
  `Repositories/Configurations/HealthServices/NurseViewManagement/NvwDisplayDeviceConfiguration.cs`,
  `NvwVitalSignIntervalConfiguration.cs`;
  `Repositories/Configurations/HealthServices/InPatientManagement/InpBedPlacementConfiguration.cs`
  (index unik berfilter `IX_InpBedPlacement_BedId_Active`)
- Entity yang dirujuk FK: `Areas/HealthServices/InPatientManagement/Models/InpBedPlacement.cs`,
  `Areas/HealthServices/MasterData/Models/MstBed.cs` (+ `MstBedConfiguration` tabel `MstBed`),
  `Models/ApplicationUser.cs` (`IdentityUser<Guid>`), `Models/IdentityModel.cs`
- `Repositories/ApplicationDbContext.cs` (region `NURSE VIEW MANAGEMENT`, `ApplyConfigurationsFromAssembly`)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/NurseViewManagement/Enums/NvwBedTabletStatus.cs` | **Baru.** `PendingLogin = 1`, `Active = 2`, `Inactive = 3` |
| `…/Enums/NvwPatientCallPriority.cs` | **Baru.** `Normal = 1`, `Emergency = 2` |
| `…/Enums/NvwPatientCallStatus.cs` | **Baru.** `Waiting = 1` s.d. `ClosedBySystem = 5` |
| `…/Enums/NvwPatientCallCloseReason.cs` | **Baru.** `NurseArrived = 1` s.d. `PatientDischarged = 7` |
| `…/Enums/NvwPatientCallEventType.cs` | **Baru.** `Pressed = 1` s.d. `ClosedBySystem = 10` |
| `…/Enums/NvwPatientCallActorKind.cs` | **Baru.** `BedTablet = 1`, `Nurse = 2`, `System = 3` |
| `…/Models/NvwBedTablet.cs` | **Baru.** 15 kolom kamus data R2.2, mewarisi `IdentityModel`; navigation `Bed` → `MstBed`, `LoginUser` → `ApplicationUser` |
| `…/Models/NvwPatientCall.cs` | **Baru.** 20 kolom kamus data R2.2; navigation `Bed`, `BedTablet`, `BedPlacement` → `InpBedPlacement`, `Events`. `InpEpisodeId` dan `ClaimedByUserId` tanpa FK, sesuai kamus data |
| `…/Models/NvwPatientCallEvent.cs` | **Baru.** 10 kolom kamus data R2.2; navigation `PatientCall` |
| `…/Models/NvwPatientCallWaitThreshold.cs` | **Baru.** 6 kolom kamus data R2.2; konstanta `GlobalScopeCode = "GLOBAL"` sebagai bawaan `ScopeCode` |
| `Repositories/Configurations/HealthServices/NurseViewManagement/NvwBedTabletConfiguration.cs` | **Baru.** Panjang kolom; `DeviceStatus` integer bawaan `1`; `Version` concurrency bawaan `0`; FK `BedId`, `LoginUserId` `Restrict`; 4 index |
| `…/NvwPatientCallConfiguration.cs` | **Baru.** `CallStatus` bawaan `1`, `PressCount` bawaan `1`, `IsBedsideConfirmed` bawaan `false`, `Version` concurrency; 3 FK `Restrict`; 5 index |
| `…/NvwPatientCallEventConfiguration.cs` | **Baru.** FK `PatientCallId` `Restrict` (relasi ke `Events`); 2 index |
| `…/NvwPatientCallWaitThresholdConfiguration.cs` | **Baru.** Check constraint urutan ambang; `ScopeCode` bawaan `GLOBAL`; unique berfilter |
| `Repositories/ApplicationDbContext.cs` | 4 baris `DbSet` di region `NURSE VIEW MANAGEMENT`: `NvwBedTablets`, `NvwPatientCalls`, `NvwPatientCallEvents`, `NvwPatientCallWaitThresholds`. Tidak ada baris lain yang berubah |

Tidak ada baris komentar pada berkas kode baru, sesuai konvensi modul (`BE-NV-001`, `002`, `004`, `007`).

**Index dan constraint yang akan dihasilkan migration** (12 index, 6 FK, 1 check constraint):

| Tabel | Index | FK |
| --- | --- | --- |
| `NvwBedTablet` | `IX_NvwBedTablet_DeviceCode` (unik); `IX_NvwBedTablet_LoginUserId` (unik, `"LoginUserId" IS NOT NULL`); `IX_NvwBedTablet_BedId_InUse` (unik, `"DeviceStatus" <> 3 AND "IsDelete" = false`); `IX_NvwBedTablet_BedId_DeviceStatus` | `MstBed` (`BedId`), `AspNetUsers` (`LoginUserId`) |
| `NvwPatientCall` | `IX_NvwPatientCall_BedId_Active` (unik, `"CallStatus" IN (1, 2) AND "IsDelete" = false`); `IX_NvwPatientCall_FloorCode_CallStatus`; `IX_NvwPatientCall_BedTabletId`; `IX_NvwPatientCall_InpBedPlacementId`; `IX_NvwPatientCall_FirstPressedAt` | `MstBed` (`BedId`), `NvwBedTablet` (`BedTabletId`), `InpBedPlacement` (`InpBedPlacementId`) |
| `NvwPatientCallEvent` | `IX_NvwPatientCallEvent_PatientCallId_OccurredAt`; `IX_NvwPatientCallEvent_IdempotencyKey` (unik, `"IdempotencyKey" IS NOT NULL`) | `NvwPatientCall` (`PatientCallId`) |
| `NvwPatientCallWaitThreshold` | `IX_NvwPatientCallWaitThreshold_ScopeCode_Active` (unik, `"IsDelete" = false`); check `CK_NvwPatientCallWaitThreshold_Order` | — |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — belum ada endpoint; dimulai `BE-NV-009` |
| Database | Empat tabel baru `public."NvwBedTablet"`, `"NvwPatientCall"`, `"NvwPatientCallEvent"`, `"NvwPatientCallWaitThreshold"`. Migration `Migrations/20261106015006_AddNurseViewPatientCall.cs` (+ Designer, snapshot +435/−0) dibuat dan diterapkan Rizki ke DB dev bersama, 6 November 2026. Tidak ada tabel yang sudah ada yang diubah: FK ke `MstBed`, `InpBedPlacement`, dan `AspNetUsers` hanya menambah constraint di tabel baru. Deployment: belum, wewenang terpisah |
| Keamanan/Auth | `NOT APPLICABLE` untuk task ini. Kolom sensitif `CloseNote` dan `Note` disiapkan sebagai teks bebas ≤ 250 karakter; larangan masuk log dan sinyal ditegakkan di `BE-NV-012`/`014`. Tidak ada kata sandi yang disimpan |

**Delta kecil terhadap kamus data (bukan perubahan kontrak).** Blok DDL kamus data R2.4 tidak
menuliskan `IX_NvwBedTablet_BedId_DeviceStatus` dan `IX_NvwPatientCall_BedTabletId`, padahal tabel
kolom di R2.2 menyebut keduanya ("index `(BedId, DeviceStatus)`" dan "`BedTabletId` · Index").
Configuration mengikuti tabel kolom. Blok DDL itu sendiri menyatakan bukan sumber kebenaran.

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menyentuh endpoint.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path <15 berkas>` (dipanggil `& .\tooling\qbe\…` dengan array path) | Files evaluated **15** (= 15 berkas) · VIOLATION 0 · REVIEW 0 · INFO 0 · `Final result: PASS` | `PASS` | Keluaran checker 2026-11-05, dijalankan ulang sesudah komentar dihapus |
| Pemeriksaan komentar `grep "//\|/*"` pada 14 berkas baru | Nol hasil | `PASS` | Keluaran perintah |
| Pemeriksaan nama `NurseCall` (`NV-FCT-004`) | Nol hasil | `PASS` | Keluaran perintah |
| Review diff `ApplicationDbContext.cs` | Hanya +4 baris `DbSet`, −0 | `PASS` | `git diff -U0` |
| Akhiran baris 15 berkas (hitung byte via Node) | Seluruhnya CRLF, nol LF tunggal | `PASS` | Keluaran skrip |
| `SELECT` read-only DB dev (`default_transaction_read_only=on`) | `MstBed`, `InpBedPlacement`, `AspNetUsers`, `NvwDisplayDevice` ada; keempat tabel baru **belum** ada; migration Nurse View terakhir `20261028082048_AddNurseViewManagement` | `PASS` | Keluaran `psql`; tidak ada penulisan |
| `dotnet build` (sebelum dan sesudah migration) | Lulus | `PASS` | Dinyatakan Rizki Gunawan, 6 November 2026; DLL `bin/Debug/net9.0/QuilvianSystemBackend.dll` bertanggal 6 Nov 09.39, sesudah berkas migration (08.50). Jumlah warning tidak dinyatakan |
| `dotnet ef migrations add AddNurseViewPatientCall` | `Migrations/20261106015006_AddNurseViewPatientCall.cs` + Designer terbentuk | `PASS` | Diperiksa agent: `Up()` = 4 `CreateTable` Nurse View, 12 `CreateIndex` (5 unik, 4 di antaranya berfilter persis kamus data), 6 FK `ReferentialAction.Restrict`, 1 check `CK_NvwPatientCallWaitThreshold_Order`; nol `AddColumn`/`AlterColumn`/`DropColumn`/`RenameColumn`. `Down()` = `DropTable` `NvwPatientCallEvent` → `NvwPatientCallWaitThreshold` → `NvwPatientCall` → `NvwBedTablet`. Snapshot +435/−0, seluruh blok tambahan milik empat entity `Nvw*` |
| `dotnet ef database update` | Diterapkan | `PASS` | Dinyatakan Rizki, 6 Nov; dibuktikan `SELECT` read-only: `__EFMigrationsHistory` memuat `20261106015006_AddNurseViewPatientCall` sebagai migration terakhir |
| `SELECT` read-only `pg_indexes` dan `pg_constraint` sesudah migration | 12 index + 4 PK; filter `("DeviceStatus" <> 3) AND ("IsDelete" = false)`, `("CallStatus" = ANY (ARRAY[1, 2])) AND ("IsDelete" = false)`, `("IdempotencyKey" IS NOT NULL)`, `("LoginUserId" IS NOT NULL)`, `("IsDelete" = false)`; 6 FK; 1 check; keempat tabel berisi 0 baris | `PASS` | Keluaran `psql`, 6 Nov 2026 |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

Uji manual: `NOT APPLICABLE` — belum ada layar maupun endpoint.

**Tidak dijalankan:** tidak ada. Build, migration, dan penerapan database dijalankan Rizki sesuai ketentuan task.

### Perintah untuk Rizki

Dijalankan dari folder `NewQuilvianSystemBackend`, berurutan. Di laptop, tambahkan
`-m:1 -nodeReuse:false` pada `dotnet build`. **Jangan** memakai `-p:SkipMigrationMetadata=true`
pada task ini, karena DLL hasil flag itu tidak bisa dipakai `dotnet ef`.

```bash
dotnet build ./QuilvianSystemBackend.sln -p:RunAnalyzers=false
dotnet ef migrations add AddNurseViewPatientCall --no-build
# periksa berkas migration (daftar di bawah) sebelum lanjut
dotnet build ./QuilvianSystemBackend.sln -p:RunAnalyzers=false
dotnet ef database update --no-build
```

Build kedua diperlukan supaya migration baru ikut terkompilasi sebelum `database update --no-build`.

**Periksa migration sebelum `database update`:**

1. `Up()` hanya berisi empat `CreateTable`: `NvwBedTablet`, `NvwPatientCallWaitThreshold`,
   `NvwPatientCall`, `NvwPatientCallEvent`.
2. Ada 12 `CreateIndex` sesuai tabel di §3.2, termasuk tiga filter `"DeviceStatus" <> 3 AND "IsDelete" = false`,
   `"CallStatus" IN (1, 2) AND "IsDelete" = false`, dan `"IdempotencyKey" IS NOT NULL`.
3. Ada 6 FK dengan `ReferentialAction.Restrict` dan satu check constraint `CK_NvwPatientCallWaitThreshold_Order`.
4. Tidak ada `AddColumn`, `AlterColumn`, `DropColumn`, maupun `CreateTable` milik modul lain.
   Bila ada, itu tanda snapshot EF tertinggal (pernah terjadi lewat resolusi merge): hentikan dan
   jangan terapkan.
5. `Down()` menghapus `NvwPatientCallEvent` lebih dulu, lalu `NvwPatientCall`, lalu `NvwBedTablet`
   dan `NvwPatientCallWaitThreshold`.
6. Snapshot `ApplicationDbContextModelSnapshot.cs` hanya bertambah, tanpa baris yang hilang.

DB dev dipakai bersama, jadi waktu `database update` sebaiknya dikabarkan ke tim lebih dulu.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 kolom, tipe, panjang, wajib/tidak, bawaan, index, dan relasi sama dengan kamus data R2.2 dan R2.4 | Terpenuhi di source | §3.2; delta dokumentasi index dicatat di §3.3. Contoh `IX_NvwPatientCall_BedId_Active` ada di `NvwPatientCallConfiguration.cs` dengan filter persis kamus data |
| AC-2 tidak ada seed maupun angka bawaan untuk ambang dan tablet (`G-15`) | Terpenuhi | Tidak ada `HasData` maupun seeder; `WarningAfterMinutes` dan `CriticalAfterMinutes` tanpa `HasDefaultValue` |
| AC-3 tidak ada nama `NurseCall*`; tidak ada perubahan model Rawat Inap, Master Data, Patient, maupun Identity | Terpenuhi | Pencarian `NurseCall` nol hasil; diff hanya berkas di §3.2 |
| AC-4 migration hanya membuat empat tabel baru, dan `Down()` menghapusnya dengan urutan R2.8 | Terpenuhi | `20261106015006_AddNurseViewPatientCall` diperiksa 6 Nov (§5). `Down()` menghapus `NvwPatientCallEvent` → `NvwPatientCall` → `NvwBedTablet`, dengan `NvwPatientCallWaitThreshold` di antaranya; urutan FK aman karena ambang tidak punya relasi |
| DoD — QBE `PASS` | Terpenuhi | §5 |
| DoD — build Rizki lulus | Terpenuhi | Pernyataan Rizki 6 Nov; DLL 09.39 |
| DoD — migration diterapkan | Terpenuhi | `__EFMigrationsHistory` dan `pg_indexes` DB dev, 6 Nov |
| DoD — laporan ada | Terpenuhi | Berkas ini |

**QBE yang berlaku dan dipatuhi:** `QBE-ENT-001` (keempat entity mewarisi `IdentityModel`),
`QBE-ENT-002`, `QBE-ENT-003` (tidak ada kolom murni presentasi; `FloorCode` dan `InpEpisodeId`
adalah salinan domain yang disahkan desain R2.4.2), `QBE-NAM-001`, `QBE-NAM-002`, `QBE-NAM-004`,
`QBE-CFG-001` (empat configuration), `QBE-MOD-001`, `QBE-MOD-002`, `QBE-MOD-003` (prefix `Nvw` sudah
terdaftar sebelum model baru), `QBE-CODE-004` (unique `DeviceCode`), `QBE-ENUM-001` (enum di folder
modul). Tidak berlaku pada task ini: `QBE-SVC-*`, `QBE-API-*`, `QBE-PERM-*`, `QBE-CODE-001`–`003`,
`005`, `006` (alokasi kode di `BE-NV-009`), `QBE-LOG-001`, `QBE-VAL-001`, `QBE-DB-*`. Keberlakuan:
`NEW CODE`, kecuali `ApplicationDbContext.cs` = `TOUCHED LEGACY` (hanya tambahan).

**Backend Governance Preflight:** Area `HealthServices` · Module `NurseViewManagement` / Nurse View ·
Submodule — · pemilik Rizki Gunawan · prefix `Nvw` · registry `ACTIVE` (baris 45, 2026-10-28,
`NV-DEC-029`) · applicability `NEW CODE` · branch `rizkiG` sesuai blueprint · sisa folder
`agents/rules/` dan `.codex/` tidak ditemukan.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `AGENTS.md` (preferensi pemilik 2026-10-05) meminta berkas migration ikut disiapkan agent, **kecuali task melarang**. Kartu roadmap dan ketentuan sesi ini melarangnya (migration dibuat Rizki, tanpa `dotnet ef`), jadi berkas migration sengaja tidak dibuat |
| Masalah yang diketahui | Blok DDL kamus data R2.4 kurang dua index (§3.3); tidak mengubah kontrak |
| Risiko tersisa | Migration sudah ada di DB dev bersama; branch lain yang belum memuat `20261106015006_AddNurseViewPatientCall` akan melihat tabel tanpa migration-nya sampai branch ini di-merge. Jumlah warning build tidak diketahui |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan task ini: `M Repositories/ApplicationDbContext.cs`, `M Migrations/ApplicationDbContextModelSnapshot.cs` dan `?? Migrations/20261106015006_AddNurseViewPatientCall{,.Designer}.cs` (dibuat Rizki); enam berkas baru di `Areas/HealthServices/NurseViewManagement/Enums/`, empat di `…/Models/`, dan empat di `Repositories/Configurations/HealthServices/NurseViewManagement/` (tampil sebagai `??`); laporan ini. Perubahan dokumen blueprint revision 2 sesi sebelumnya dan berkas `docs/module-blueprints/igd/**` milik pengguna tidak disentuh task ini |
| Langkah berikutnya | `BE-NV-009` (admin tablet bed, jalur kritis menuju login tablet dan panggilan) dan `BE-NV-016` (ambang) — boleh paralel |
