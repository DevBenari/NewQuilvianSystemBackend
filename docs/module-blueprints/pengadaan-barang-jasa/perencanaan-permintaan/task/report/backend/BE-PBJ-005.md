# Laporan Perubahan Backend — `BE-PBJ-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PBJ-005` |
| Judul | Gerbang approval Pengadaan |
| Slice | `MVP-0` — fondasi (`EPIC PBJ-01`) |
| Roadmap | `perencanaan-permintaan/roadmap/backend-roadmap.md` revisi 1, kartu `BE-PBJ-005` |
| Trace | `PBJ-DEC-007`, `016`, `019`, `027`, **`038`** (baru); FR-PBJ-001, FR-PBJ-044; INT §1; arsitektur §8, §10 |
| Contract version | `PBJ-PP-API` 1.1, `PBJ-PP-PERM` 1.2 (task ini tidak membuat endpoint) |
| Dependency | `BE-PBJ-001` 🟡, `BE-PBJ-003` 🟡 |
| Klasifikasi | `HIGH` (logika 2, kontrak antarmodul 2, keamanan 2 — menyentuh mesin approval bersama milik HR) |
| Task mode | `BACKEND` (diberikan pemilik 12 November 2026, bersama `BE-PBJ-006`) |
| Target tulis | `Areas/Corporate/ProcurementManagement/Common/**`, `WorkflowService.cs` (Wfl), `Program.cs` (urutan seeder), extension DI, `.gitignore` (izin terpisah); basis data dev tidak disentuh |
| Model | Claude Opus 5.5 |
| Branch / commit | `sukmagp` @ `100c011d` (belum di-commit) |
| Tanggal | 12 November 2026 |
| Status | ✅ **SELESAI — diperbarui 16 November 2026.** Kriteria 1–4 terbukti runtime; instance nyata `PBJ-RKB`, `PBJ-PR`, `PBJ-RPK`, `PBJ-PAGU` dibuat atas nama pemohon dan disetujui end-to-end; seeder idempoten (`AT-PP-64`) |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `Common`; plus `Corporate` / `HumanResource` / `WorkflowManagement` (Wfl, perluasan) |
| Registry | `Corporate / Procurement | ProcurementManagement / Common | Prc | ACTIVE` (`PBJ-DEC-035`); Wfl milik HR, diperluas atas keputusan pemilik `PBJ-DEC-019` dan `PBJ-DEC-038` |
| Keberlakuan | `NEW CODE` untuk Pengadaan; `TOUCHED LEGACY` untuk `WorkflowService.cs` dan `Program.cs` |
| QBE yang berlaku | `QBE-MOD-001..003`, `QBE-NAM-002`, `QBE-SVC-001`, `QBE-TXN-001` (Wfl dulu, idempoten), `QBE-LOG-001`, `QBE-AUD-001`, `QBE-PERM-001` (tanpa hardcode role: penyetuju ditentukan matriks) |
| Hasil checker | QBE `PASS` (15 berkas bersama `BE-PBJ-006`, 0 temuan) |

## 1. Masalah yang diperbaiki

Dokumen Pengadaan (PR, rencana kebutuhan, rencana paket, pagu) butuh persetujuan berjenjang. Mesin approval
bersama (Wfl) sudah ada, tetapi:

1. Wfl selalu mencatat **pengguna yang sedang login** sebagai pengaju. PR baru masuk approval ketika
   **verifikator** menyelesaikan verifikasi (`PBJ-DEC-027`), sehingga yang tercatat sebagai pengaju justru
   verifikator, dan jenjang "atasan pemohon" menunjuk atasan yang salah.
2. Belum ada pintu tunggal dari Pengadaan ke Wfl, belum ada penangan hasil approval untuk dokumen `PBJ_*`,
   dan belum ada definisi alur `PBJ-*`.

## 2. Proses bisnis

1. Modul pemilik dokumen (mis. `BE-PBJ-008` saat verifikasi selesai) memanggil gerbang dengan: jenis dokumen,
   ID, nomor, kode alur, **pemohon asli**, nilai, kode kondisi, dan "pengajuan ke-berapa".
2. Gerbang menyusun kunci idempoten `<jenis>:<id>:<ke-berapa>` dan konteks `{amount, conditionCodes}`.
3. Wfl membuat instance **atas nama pemohon** lalu langsung mengajukannya. Pemanggilan ulang dengan kunci sama
   mengembalikan instance yang sama.
4. Langkah ber-kode kondisi hanya aktif bila kodenya dikirim. Contoh: langkah "Direktur Keuangan" hanya
   muncul bila PR membawa `PBJ_OVER_BUDGET`.
5. Saat Wfl menyelesaikan approval, penangan lifecycle Pengadaan menerima instance `PBJ_*`, memeriksa ulang
   bahwa penyetuju bukan pemohon, lalu meneruskan ke penerap status milik jenis dokumennya.

**Contoh:** Rina (IGD) membuat PR Rp 12 juta; Budi (Tim Pengadaan) menyelesaikan verifikasi. Instance
tercatat diajukan oleh Rina; langkah pertama menunggu atasan Rina, bukan atasan Budi.

**Jalur tidak normal:**
- Definisi `PBJ-*` belum diaktifkan admin (404) atau tanpa langkah aktif → hasil `ConfigurationMissing = true`,
  yang dipetakan pemanggil ke `PBJ-VAL-091` "Alur persetujuan belum dikonfigurasi". Status dokumen tidak berubah.
- Wfl gagal di tengah → dokumen belum diubah (urutan "Wfl dulu"); pengulangan aman berkat kunci idempoten.
- Pemohon mencoba menyetujui pengajuannya sendiri → ditolak Wfl (`AllowSelfApproval = false`) dan diperiksa
  ulang oleh penangan lifecycle.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`WorkflowService.cs` (`CreateAsync`, `SubmitAsync`, resolusi pemohon/atasan, idempotensi),
`HumanResourceContextService`, `WorkflowReferenceLifecycleService.cs`, model `MstWorkflowDefinition`,
`MstWorkflowStep`, `MstApprovalMatrix`, seeder workflow HR.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `HumanResource/WorkflowManagement/Services/WorkflowService.cs` | **Diperbarui.** `CreateAsync`/`SubmitAsync` menjadi pembungkus `CreateCoreAsync`/`SubmitCoreAsync` (perilaku lama tidak berubah). Metode baru `CreateAndSubmitOnBehalfAsync(request, requesterUserId, ct)`: kunci idempoten wajib, konteks pemohon dari `GetByUserIdAsync`, ajukan hanya bila masih `Draft` dengan kunci `+":submit"` |
| `Common/Services/ProcurementApprovalGateway.cs` | **Baru.** `ProcurementApprovalRequest`/`Result`, `BuildIdempotencyKey`, `SubmitAsync` |
| `Common/Services/ProcurementWorkflowLifecycleHandler.cs` | **Baru.** Penangan `PBJ_*`, interface `IProcurementWorkflowStatusApplier`, `ProcurementWorkflowStatusMap` |
| `Common/Seeders/ProcurementWorkflowDefinitionSeeder.cs` | **Baru.** 5 definisi `PBJ-PR`, `PBJ-PR-MENDESAK`, `PBJ-RKB`, `PBJ-RPK`, `PBJ-PAGU`; kategori `Procurement`; `WorkflowStatus = "Draft"`, `IsActive = false`; langkah atasan pemohon + jabatan dengan kode kondisi `PBJ_OUT_OF_PLAN`/`PBJ_OVER_BUDGET`; idempoten |
| `ProcurementManagementServiceCollectionExtensions.cs` | +gateway, +penangan lifecycle |
| `Program.cs` | Seeder definisi dijalankan sesudah `ProcurementMasterDataSeeder` |
| `.gitignore` | `/App_Data/` (risiko `BE-PBJ-004`, izin pemilik terpisah) |

### 3.3 Dampak kontrak, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada endpoint baru. Kontrak antarmodul Wfl bertambah satu metode (delta `PBJ-DEC-038`) |
| Database | Tidak ada migration. Seeder menulis definisi/langkah ke tabel Wfl yang sudah ada **saat aplikasi start** |
| Keamanan/Auth | Penyetuju ditentukan matriks yang diisi admin, tanpa nama peran di kode. Definisi di-seed **tidak aktif tanpa baris matriks** (fail-closed) — delta dari kartu yang menyebut "matriks fallback [NILAI AWAL]": nilai matriks belum diputuskan (`PBJ-OQ-019`), jadi tidak ada penyetuju yang dikarang |

## 4. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` (perintah baku pemilik, bersama `BE-PBJ-006`) | 0 error, 608 warning (= baseline), 2 m 03 s | `PASS` | Log build |
| QBE checker (15 berkas) | 0 pelanggaran, 0 review | `PASS` | Laporan checker |
| Kriteria 1 — satu instance per kunci idempoten | `CreateCoreAsync` memakai pencarian `IdempotencyKey` milik Wfl; submit hanya dari `Draft` dengan kunci turunan | `PASS` (statis) | Review source |
| Kriteria 2 — definisi tidak aktif → `PBJ-VAL-091` | 404 / "belum memiliki step aktif" → `ConfigurationMissing` | `PASS` (statis) | Review source |
| Kriteria 3 — penangan terdaftar dan mengklaim `PBJ_*` | DI + `CanHandle` prefix `PBJ_` | `PASS` (statis) | Review source |
| Kriteria 4 — seeder idempoten | Cari per `WorkflowCode`/`StepCode` sebelum menambah | `PASS` (statis) | Review source; belum dijalankan |
| Perilaku Wfl lama tidak berubah | `CreateAsync`/`SubmitAsync` meneruskan `null` = jalur lama | `PASS` (statis) | Review diff |
| Instance approval nyata, DB definisi/langkah | Tidak dijalankan | `NOT RUN` | Aplikasi tidak dapat start di dev DB (migration rekan tertunda, kolom `ContractEscalationDays`); instance pertama direncanakan di `BE-PBJ-009` |

- **AUTOMATED TEST:** `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 5. Status migration dan basis data

Tidak ada migration. Basis data dev tidak disentuh.

## 6. Risiko yang tersisa

| Risiko | Penanganan |
| --- | --- |
| Wfl menolak instance baru bila instance lama untuk dokumen yang sama masih `Returned` | Jalur ajukan ulang dirancang di `BE-PBJ-008` (kunci idempoten memakai `SubmissionCount`) |
| Penerap status tidak boleh bergantung pada `WorkflowService`/gateway (siklus DI) | Dicatat pada XML doc interface; diperiksa saat `BE-PBJ-008`/`009` mendaftarkan penerap |
| Approval tidak dapat dipakai sampai admin mengisi matriks dan mengaktifkan definisi | Disengaja; nilai menunggu SOP (`PBJ-OQ-019`) |

## 7. Task berikutnya

`BE-PBJ-009` (rencana kebutuhan) dan `BE-PBJ-010` (pagu) memakai gerbang ini dan membuktikan instance nyata pertama.

## Verifikasi lanjutan — 16 November 2026

Dijalankan bersama `BE-PBJ-007`..`011`: build Release `PASS` (0 error, 601 warning = baseline), QBE Strict `PASS` (53
berkas), `has-pending-model-changes` bersih, seluruh migration Pengadaan diterapkan ke `QuilvianNewDevSukma` atas izin
pemilik, dan uji runtime HTTP ke `https://localhost:7185` dengan aktor non-SuperAdmin `TEST-PBJ`.

**Fixture uji (`TEST-PBJ-20261116110649`, dibuat lewat API sebagai SuperAdmin, belum dibersihkan).** Lima unit uji
(`TEST-PBJ IGD/Radiologi/Pengadaan/Manajemen/Keuangan Uji`), tujuh jabatan, delapan akun `Employee` **tanpa role**
(`reqA`, `reqA2`, `headA`, `reqB`, `proc`, `mgr`, `dirk`, `keu`), dan policy Akses Role per unit × jabatan sesuai peta
peran `permission-audit-matrix.md` bagian 3. Konfigurasi approval uji (izin pemilik "nilai uji di dev"): definisi
`PBJ-*` diaktifkan, satu baris `MstApprovalMatrix` fallback `SpecificUser` per langkah, dan empat
`MstRejectionReason` — semuanya bertanda `DATA UJI PBJ (bukan nilai SOP)`. Komitmen pagu uji Rp10.000.000
(`SourceDocumentType = UJI_PBJ`) disisipkan lewat DB sesuai catatan `AT-PP-45`. Sisa fixture percobaan pertama
(`TEST-PBJ-20261116105253`) juga masih ada.

**Cacat yang ditemukan dan diperbaiki di `BE-PBJ-009`.** Pada uji runtime pertama, `CreateAndSubmitOnBehalfAsync`
menjawab 409 "Submit workflow gagal karena terjadi konflik data". Penyebabnya bukan task ini secara langsung:
`WorkflowService.AddStatusHistory` menambahkan riwayat ber-`Id` terisi lewat navigasi instance yang sedang dilacak,
sehingga EF Core menjalankan `UPDATE` (0 baris). Setelah diperbaiki, gateway ini terbukti di runtime.

| ID | Skenario | Hasil | Bukti |
| --- | --- | --- | --- |
| `AT-PP-62` | Skema: 14 tabel Pengadaan, index unik parsial, CHECK kuantitas/nominal, kolom ActivationConditionCode varchar(50) NULL | `PASS` | tables=14; uniquePartial={'IX_PrcNeedPlan_CurrentVersion': True, 'IX_PrcBudget_CurrentVersion': True, 'IX_PrcBudgetCommitment_ActiveSource': True, 'IX_PrcPurchaseRequest_RequestNumber': True, 'IX_PrcPackagePlan_PackagePlanNumber': True, 'IX_PrcNeedPlan_PlanNum |
| `AT-PP-64` | Seeder master + definisi PBJ-* dijalankan ulang (server start ulang 3×) → jumlah baris sama | `PASS` | before={'types': 4, 'umum': 1, 'funds': 2, 'wfl_defs': 5, 'wfl_steps': 11}; after={'types': 4, 'umum': 1, 'funds': 2, 'wfl_defs': 5, 'wfl_steps': 11} |
| `B-11` | Definisi PBJ-RKB tidak aktif → 409 PBJ-VAL-091, rencana tetap DRAFT, tanpa instance | `PASS` | http=409; code=PBJ-VAL-091; message=Alur persetujuan pengadaan belum dikonfigurasi. Hubungi administrator. |
| `B-15` | Instance PBJ-RKB atas nama penyusun, kunci idempoten <type>:<id>:1 | `PASS` | requestedBy=reqA; key=PBJ_NEED_PLAN:fc151009fecb460ab0dc6f8d9036ebf6:1; steps=['PBJ-RKB-01', 'PBJ-RKB-02'] |
| `B-16` | Disetujui kedua jenjang di kotak masuk → APPROVED, versi berlaku, WF_APPROVED sekali | `PASS` | approvals=[['PBJ-RKB-01', 'headA', 200], ['PBJ-RKB-02', 'mgr', 200]]; status=APPROVED; wfApproved=1 |
| `F-05` | Instance atas nama pengusul (bukan verifikator), kunci idempoten :1 | `PASS` | key=PBJ_PR:4164a87d97994fd9b239c54a2b76f196:1 |
| `F-09` | Dikembalikan Wfl → RETURNED; ajukan ulang menutup instance lama (Cancelled) dan mengosongkan cek stok; verifikasi ulang → instance baru :2 | `PASS` | oldInstance=Cancelled; newKey=PBJ_PR:9e5a8601510741209d4b80cfc756e2cc:2; message=Verifikasi berhasil diselesaikan. |
| `F-DB-1` | 5 definisi PBJ-* kategori Procurement, 11 langkah, AllowSelfApproval=false, 4 langkah ber-kode kondisi (PR-03/04, PRM-02/03) | `PASS` | conditional=['PBJ-PR-03', 'PBJ-PR-04', 'PBJ-PRM-02', 'PBJ-PRM-03'] |

8 skenario, 8 `PASS`.

