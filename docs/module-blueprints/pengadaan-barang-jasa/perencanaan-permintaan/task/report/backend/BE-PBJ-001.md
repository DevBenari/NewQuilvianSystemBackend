# Laporan Perubahan Backend — `BE-PBJ-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PBJ-001` |
| Judul | Perluasan mesin approval `Wfl` |
| Slice | `MVP-0` — fondasi (`EPIC PBJ-01`) |
| Roadmap | `perencanaan-permintaan/roadmap/backend-roadmap.md` revisi 1, kartu `BE-PBJ-001` |
| Trace | `PBJ-DEC-019`, `PBJ-DEC-026`, `PBJ-DEC-030`; FR-PBJ-001..003; `02-backend-architecture.md` §4.2, §8; `PBJ-PP-INT` 1.0 §1; kamus data §6 |
| Contract version | Set `1.0` (`approved`, `PBJ-DEC-034`). Task ini tidak menambah atau mengubah endpoint |
| Dependency | Tidak ada |
| Klasifikasi | `MEDIUM` (skor 7: berkas diperiksa 1, berkas diubah 1, logika 1, database 2, keamanan/auth 1, workflow 1) |
| Task mode | `BACKEND` (diberikan pemilik 12 November 2026) |
| Target tulis | `NewQuilvianSystemBackend`: berkas `Wfl` pada kartu task + migration `AddWorkflowStepActivationCondition` (+ Designer, snapshot); laporan dan status pada blueprint |
| Model | Claude Opus 5.5 |
| Branch / commit saat dikerjakan | `sukmagp` @ `100c011d` (working tree belum di-commit; tanpa stage/commit/push) |
| Tanggal | 12 November 2026 |
| Status | 🟡 **SEBAGIAN.** 4 dari 5 acceptance criteria terbukti (statis/EF); migration **sudah diterapkan** ke `QuilvianNewDevSukma` (12 November 2026, atas izin pemilik). Uji regresi HR di runtime (`AT-PP-35`) `NOT FEASIBLE`: aplikasi tidak dapat start di basis data dev karena migration rekan `20261110044224_AddHrdDurableAuditAndPinLockouts` belum diterapkan di sana, dan basis data dev belum punya definisi workflow sama sekali |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate/HumanResource` / `WorkflowManagement` (+ `MasterData/Workflow` untuk `MstWorkflowStep`) |
| Registry | Baris 36: `Corporate/HumanResource | WorkflowManagement / Workflow | SHARED PLATFORM CAPABILITY | Wfl | ACTIVE / LEGACY`. **Tidak ada entity baru**, sehingga `QBE-MOD-002/003` tidak memblokir |
| Keberlakuan | `TOUCHED LEGACY` (`MstWorkflowStep`, `WorkflowService`, `WorkflowReferenceLifecycleService`); `NEW CODE` untuk `IWorkflowReferenceLifecycleHandler` |
| QBE yang berlaku | `QBE-ENT-002` (kolom nullable sesuai semantik), `QBE-CFG-002` (konfigurasi diperbarui dalam cakupan), `QBE-SVC-001` (tidak ada controller baru), `QBE-VAL-001` (validasi konteks tetap di service), `QBE-AUD-001` (tidak menyentuh audit). `QBE-NAM-*` tidak berlaku (tidak ada nama entity baru) |
| Pengecualian pola | Interface `IWorkflowReferenceLifecycleHandler` menyimpang dari konvensi "service tanpa interface". Ini disengaja: interface adalah titik perluasan agar `Wfl` dapat memanggil penangan modul lain tanpa merujuk modul itu (`02-backend-architecture.md` §4.2) |

## 1. Masalah yang diperbaiki

Mesin approval bersama `Wfl` hanya meneruskan hasil persetujuan ke jenis dokumen HR, karena daftar penangannya
tertulis langsung di `WorkflowReferenceLifecycleService`. Akibatnya, bila modul Pengadaan memakai `Wfl`,
permintaan pengadaan yang sudah disetujui tidak pernah ikut berubah status (`IsHandled = false`). Selain itu,
setiap langkah definisi workflow selalu dibuat, sehingga langkah tambahan seperti "Direktur Keuangan bila
melebihi pagu" (`PBJ-DEC-030`) tidak bisa dinyatakan tanpa menggandakan definisi.

## 2. Proses bisnis

**Tujuan:** mesin approval yang sama dapat melayani dokumen modul mana pun, dengan jenjang yang bisa muncul
atau tidak sesuai kondisi dokumen.

**Langkah setelah perubahan:**
1. Modul pemilik dokumen membuat instance `Wfl` dan menyertakan `RequestContext`, misalnya
   `{ "amount": 45000000, "conditionCodes": ["PBJ_OVER_BUDGET"] }`.
2. `WorkflowService.CreateAsync` memuat langkah aktif definisi, lalu **melewati** langkah yang
   `ActivationConditionCode`-nya tidak ada pada `conditionCodes`. Langkah tanpa kode tetap dibuat.
3. Penyetuju bertindak seperti biasa. Perpindahan ke langkah berikutnya memakai nomor urut terkecil berikutnya
   (`AdvanceToNextStepGroupAsync`, `Min()`), sehingga langkah yang dilewati tidak meninggalkan celah.
4. Setelah keputusan commit, `WorkflowReferenceLifecycleService` menangani jenis dokumen HR seperti sebelumnya.
   Bila tidak ada yang cocok, ia menanyai penangan terdaftar (`CanHandle`), dan penangan pertama yang
   mengklaim jenis dokumen itu menyinkronkan status dokumen asalnya.

**Contoh:** definisi `PBJ-PR` punya langkah 1 Kepala Unit (tanpa kode), 2 Manajer Pengadaan (tanpa kode), dan
3 Direktur Keuangan (`PBJ_OVER_BUDGET`). PR yang melebihi pagu mendapat tiga langkah; PR yang tidak melebihi
pagu hanya dua.

**Jalur tidak normal:**
- Bila semua langkah terlewati, `CreateAsync` mengembalikan galat yang sudah ada: "Workflow definition belum
  memiliki step aktif."
- Bila tidak ada penangan terdaftar untuk sebuah jenis dokumen, hasilnya tetap `IsHandled = false` tanpa efek,
  sama seperti sebelum perubahan.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`WorkflowService.cs` (`CreateAsync`, `SelectApprovalMatrixPair`, `AdvanceToNextStepGroupAsync`,
`WorkflowRequestContext`), `WorkflowReferenceLifecycleService.cs`, `IdempotentDownstreamExecutionService.cs`,
`WorkflowInstanceCreationService.cs`, `WorkflowRoutingMatrixService.cs`, `MstWorkflowStep.cs`,
`MstWorkflowStepConfiguration.cs`, `WorkflowStepController.cs` (pemakai `MstWorkflowStep`), `Program.cs`
(registrasi DI), `Migrations/ApplicationDbContextModelSnapshot.cs`, migration dan Designer terbaru,
`Migrations/MigrationMetadata.g.*`, registry prefix.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkflowManagement/Services/IWorkflowReferenceLifecycleHandler.cs` | **Baru.** Interface `CanHandle(string)` + `SynchronizeAsync(TrxWorkflowInstance, Guid, bool, CancellationToken)` |
| `Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowReferenceLifecycleService.cs` | Menerima `IEnumerable<IWorkflowReferenceLifecycleHandler>` dan memanggil penangan pertama yang mengklaim jenis dokumen, **sesudah** seluruh cabang HR dan sebelum fallback `IsHandled = false`. +24/−1 baris. CR ganda (`\r\r\n`) yang sudah ada di berkas asli dipertahankan agar diff tidak membengkak |
| `Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowService.cs` | Konteks permohonan dibaca sebelum langkah dipilih (pindah dari dalam blok transaksi, isi sama); langkah disaring `IsStepActivated`; `WorkflowRequestContext` mendapat `ConditionCodes` (tanpa beda huruf besar/kecil) dari `conditionCodes` beserta pembacanya `ReadStringSet` |
| `Areas/Corporate/HumanResource/MasterData/Workflow/Models/MstWorkflowStep.cs` | Kolom `ActivationConditionCode` (`string?`, `[MaxLength(50)]`) |
| `Repositories/Configurations/Corporate/HumanResource/MasterData/Workflow/MstWorkflowStepConfiguration.cs` | `HasMaxLength(50).IsRequired(false)` |
| `Migrations/20261112120000_AddWorkflowStepActivationCondition.cs` | **Baru.** `AddColumn` nullable, `Down` = `DropColumn` |
| `Migrations/20261112120000_AddWorkflowStepActivationCondition.Designer.cs` | **Baru.** `BuildTargetModel` = isi snapshot sesudah perubahan (snapshot adalah gabungan seluruh migration) |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | +4 baris properti `ActivationConditionCode` |

`MigrationMetadata.g.cs` tidak disentuh, karena migration baru tidak diarsipkan (pola migration November).

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE`: tidak ada endpoint yang berubah. `RequestContext` menerima field opsional baru `conditionCodes`; konteks lama tetap valid. Endpoint pengaturan langkah (`WorkflowStepController`) **belum** menampilkan dan menerima `ActivationConditionCode`. Langkah ber-kode diatur lewat data awal (`BE-PBJ-005`), sesuai catatan `PBJ-PP-API` §perubahan internal |
| Database | Satu kolom nullable `public."MstWorkflowStep"."ActivationConditionCode" varchar(50)`. Migration **dibuat, tidak diterapkan**. Urutan rilis: database dulu, lalu aplikasi, karena model baru ikut memilih kolom ini pada setiap query langkah workflow (termasuk HR) |
| Keamanan/Auth | Tidak mengubah izin. Penjaga anti-setujui-sendiri (`AllowSelfApproval`, cek `RequestedByUserId`) tidak disentuh. Penangan terdaftar baru dipanggil untuk jenis dokumen yang tidak dikenal HR, sehingga tidak dapat mengambil alih dokumen HR |

## 4. Dokumentasi endpoint

`NOT APPLICABLE`: task tidak menyentuh endpoint.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build .\QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false` | 0 error, 608 warning; 3 menit 16 detik; tidak ada warning dari berkas yang berubah | `PASS` | Log build di scratchpad sesi |
| EF `migrations script 20261111100000_… 20261112120000_AddWorkflowStepActivationCondition` | Tepat satu `ALTER TABLE public."MstWorkflowStep" ADD "ActivationConditionCode" character varying(50);` + baris riwayat | `PASS` | Keluaran skrip (kriteria 5) |
| EF `migrations has-pending-model-changes` | Melaporkan perubahan tertunda | `EXISTING / ENVIRONMENT ISSUE` | Migration pemeriksa sementara (dibuat lalu dihapus, snapshot dikembalikan; `git diff --stat` snapshot tetap +4) berisi 200 `AlterColumn`, 46 FK, dan 14 index pada **21 tabel HR lain** (mis. `trx_fppe_case_logs`, `TrxClinicalPrivilegeEmergencyOverrideLog`, `HrdEarlyLeavePermission`, `MstWorkforceProfile`), dan **tidak** memuat `MstWorkflowStep`/`ActivationConditionCode`. Selisih ini bukan dari task ini; kolom baru sudah selaras antara model, snapshot, dan migration |
| Kriteria 1: HR ditangani penangan yang sama | Cabang HR tidak berubah; penangan terdaftar hanya dicapai bila semua cabang HR tidak cocok; belum ada penangan terdaftar | `PASS` (statis) | Review diff `WorkflowReferenceLifecycleService.cs` |
| Kriteria 2: penangan terdaftar dipanggil untuk tipe yang ia klaim | Jalur kode ada; belum ada penangan nyata (yang pertama dibuat `BE-PBJ-005`) | `NOT RUN` (runtime) | Dibuktikan bersama `BE-PBJ-005`/`009` |
| Kriteria 3–4: langkah ber-kode hanya dibuat bila kodenya ada; langkah `null` selalu dibuat | `IsStepActivated` + filter sebelum instance dibuat | `PASS` (statis) | Review diff `WorkflowService.cs` |
| Kriteria 5: migration hanya satu kolom nullable | — | `PASS` | Skrip EF |
| Penerapan migration ke `QuilvianNewDevSukma` (skrip idempoten berisi migration ini saja, eksekutor scratchpad yang menolak nama DB lain) | Kolom `ActivationConditionCode` `character varying(50)`, nullable; `__EFMigrationsHistory` bertambah tepat satu baris `20261112120000_AddWorkflowStepActivationCondition`; migration rekan yang tertunda (`20261110044224`, `20261110090000`, `20261111100000`) tidak ikut | `PASS` | Query `information_schema.columns` dan `__EFMigrationsHistory` dalam transaksi `READ ONLY` |
| Start aplikasi lokal (`bin/Debug`, `https://localhost:7184`) untuk `AT-PP-35` | Aplikasi berhenti (exit 82) di `OnboardingRuntimeDevSeeder` dengan galat `42703: column h.ContractEscalationDays does not exist`. Kolom itu milik migration rekan `20261110044224_AddHrdDurableAuditAndPinLockouts` yang belum diterapkan di basis data dev. Tidak berhubungan dengan task ini | `EXISTING / ENVIRONMENT ISSUE` | Log startup di scratchpad sesi |
| `AT-PP-35` regresi cuti/lembur di runtime | Tidak dapat dijalankan: aplikasi tidak start (baris di atas), dan `MstWorkflowStep` di basis data dev berisi **0** baris, sehingga belum ada definisi workflow untuk diuji | `NOT RUN` (`NOT FEASIBLE` di lingkungan ini) | Query `count(*)` = 0 |

- **Uji manual:** `NOT FEASIBLE` di basis data dev saat ini (lihat dua baris terakhir tabel).
- **AUTOMATED TEST:** `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Status migration dan basis data

| Butir | Status |
| --- | --- |
| Berkas migration, Designer, snapshot | Dibuat |
| Diterapkan ke basis data | **Ya**, hanya ke `QuilvianNewDevSukma`, 12 November 2026, atas instruksi pemilik ("Ya, terapkan hanya migration ini"). Lingkungan lain belum |
| Catatan basis data dev `QuilvianNewDevSukma` | Menurut catatan sesi sebelumnya, ada migration modul lain yang tertunda di sana. Cara aman: terapkan **hanya** migration ini lewat skrip idempoten (`migrations script 20261111100000_… 20261112120000_… --idempotent`), bukan `database update` biasa |

## 7. Risiko yang tersisa

| Risiko | Penanganan |
| --- | --- |
| Aplikasi dijalankan dengan model baru sebelum kolom ada → semua query `MstWorkflowStep` (termasuk HR) gagal | Database dulu, lalu aplikasi |
| `OnApproveNextStepCode` pada definisi lain menunjuk langkah ber-kode yang dilewati | Kolom ini tidak dipakai untuk perpindahan langkah di `WorkflowService` (hanya snapshot); risiko rendah. Definisi `PBJ-*` tidak memakainya |
| Penyunting langkah workflow di UI belum bisa mengisi kode kondisi | Data awal `BE-PBJ-005`; bila perlu UI, jadi task tersendiri pada `Wfl` |
| Jalur V2 `WorkflowInstanceCreationService` (fallback pegawai sembarang) dan `WorkflowRoutingMatrixService` (jenjang di kode) | Tidak disentuh; temuan untuk pemilik `Wfl`. Pengadaan tidak memakainya |

## 8. Task berikutnya

1. Untuk menaikkan task ini ke ✅: basis data dev perlu menerapkan migration rekan yang tertunda (minimal `20261110044224`) agar aplikasi bisa start, ditambah satu definisi workflow uji. Kedua tindakan itu butuh izin pemilik. Alternatifnya, `AT-PP-35` dibuktikan bersama `BE-PBJ-009`, yang men-seed definisi `PBJ-*` dan menjalankan instance pertama.
2. `BE-PBJ-002` **dibuka**: registry `Prc` disetujui 12 November 2026 (`PBJ-DEC-035`).
