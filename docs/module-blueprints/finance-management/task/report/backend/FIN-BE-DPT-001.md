# Laporan Perubahan Backend — `FIN-BE-DPT-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FIN-BE-DPT-001` |
| Judul | Batas Pembayaran Langsung: pengajuan → persetujuan → aktivasi manual → riwayat permanen; persetujuan SuperAdmin |
| Slice | Pekerjaan ad-hoc di luar penomoran roadmap (permintaan 28 Nov 2026). Tidak ada baris roadmap, jadi **tidak ada tanda status yang dipasang pada roadmap** |
| Trace | `FIN-DES-086`, `FIN-DEC-134`, `FIN-DEC-146`/`FIN-VAL-228` (penanda versi), `FIN-DEC-151` (nama pengubah), `FIN-VAL-197` (fail-closed), `FIN-PERM-1.7 G.5` (**diubah**, lihat bagian 8) |
| Contract version | Aditif + satu perubahan perilaku yang memutus: `PUT` lama dijawab 410. `contracts/api-contract.md` dan `permission-audit-matrix.md` **belum diperbarui** (kontrak terkunci; butuh amandemen, bagian 8) |
| Dependency | **DDL dua tabel baru wajib diterapkan pemilik sebelum backend dijalankan untuk fitur ini** (bagian 6). FE `FIN-FE-DPT-001` membaca kontrak ini |
| Task mode | `BACKEND`, tanpa migration, tanpa eksekusi database |
| Target tulis | `NewQuilvianSystemBackend` (source) + laporan ini |
| Branch / SHA saat dikerjakan | `Yasmina` @ `1cd7b477dc48799acc68792aea534d348b902975` |
| Model | Claude Sonnet 5.5 |
| Tanggal | 2026-11-28 |
| Status | 🟡 **SEBAGIAN** — source selesai dan lolos pemeriksaan statis; `dotnet build` **TIDAK DIJALANKAN** (dilarang), migration/DDL **belum ada**, jadi alur ini **belum operasional** sampai tabel dibuat |

---

## 1. Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `FinanceManagement` / `MasterData` |
| Pemilik / prefix registry | Folder `MasterData` dimiliki prefix **`Mst`** (ACTIVE, Category `…MASTER / REFERENCE` yang diakui checker). Entitas baru diberi nama `MstDirectPaymentThresholdProposal` dan `MstDirectPaymentThresholdEvent` — sejalan `FIN-DES-003` ("Memakai prefix `Mst`, bukan `Fin`") dan `MstDirectPaymentThreshold` yang sudah ada |
| Keputusan prefix | Percobaan pertama memakai `Fin*` menghasilkan **`QBE-MOD-002` VIOLATION ×2** ("Obtain registry decision; do not infer a prefix"). Penamaan `Mst*` menyelesaikannya tanpa mengarang prefix. Bila pemilik lebih menyukai `Fin*`, butuh keputusan registry (`NEEDS CONFIRMATION`) |
| Keberlakuan | `NEW CODE`: dua entity, dua configuration, `DirectPaymentThresholdProposalService`, DTO baru. `TOUCHED LEGACY`: controller, `DirectPaymentThresholdService`, `AccessPermissionService`, dua pesan pada AR/AP |
| QBE | Controller tidak memakai `ApplicationDbContext`; entity mewarisi `IdentityModel`; ada `IEntityTypeConfiguration`; DTO (tidak mengekspos entity); setiap action baru punya `[AccessAction]`/`[AccessPermission]` berpasangan; tanpa hardcode role; nomor pengajuan dibentuk service (bukan controller) dan dijaga indeks unik |
| Pemeriksa konformansi | `Invoke-QbeConformanceCheck.ps1 -Path` pada 11 berkas: VIOLATION 0 / REVIEW 0 / INFO 0 — **PASS** |
| Migration / database | **Tidak ada berkas migration dibuat** (`git status -- Migrations` = 0), tidak ada DDL/DML dijalankan, snapshot tidak diubah. Dua tabel baru butuh skema (bagian 6) |

---

## 2. Masalah yang diperbaiki

| No | Keadaan sebelumnya | Akibat |
| ---: | --- | --- |
| 1 | `PUT` langsung menimpa `MstDirectPaymentThreshold.Amount` | Siapa pun pemegang `Update` mengubah batas pembayaran seketika, tanpa persetujuan |
| 2 | Tidak ada pengajuan, persetujuan, atau aktivasi | Tidak ada pemisahan tugas (pengaju ≠ penyetuju) |
| 3 | Hanya "perubahan terakhir" yang tercatat (`FIN-DES-086`) | Batas lama hilang saat diganti; tidak dapat diaudit |
| 4 | Tidak ada Tanggal Berlaku | Tidak diketahui sejak kapan sebuah nominal dipakai |
| 5 | Istilah "Ambang" di menu, layar, dan pesan AR/AP | Tidak sesuai istilah bisnis "Batas Pembayaran Langsung" |

---

## 3. Proses bisnis

### 3.1 Alur

```text
Ajukan → [MENUNGGU · BELUM_AKTIF] ─┬─ Setujui → [DISETUJUI · BELUM_AKTIF] ── Aktifkan → [DISETUJUI · AKTIF] ── (diganti) → [DISETUJUI · DIGANTIKAN]
                                   └─ Tolak   → [DITOLAK]  (tidak dapat diaktifkan)
```

Dua dimensi **dipisah tegas**: *Status Pengajuan* (`MENUNGGU`/`DISETUJUI`/`DITOLAK`) dan *Status Aktivasi* (`BELUM_AKTIF`/`AKTIF`/`DIGANTIKAN`). "Disetujui" bukan sinonim "Aktif".

| Dari | Tindakan | Ke | Izin Role Management | Syarat tambahan (backend) |
| --- | --- | --- | --- | --- |
| — | Ajukan | `MENUNGGU` + `BELUM_AKTIF` | `Update` | Alasan wajib (422); nominal > 0, ≤ 2 desimal (400); nominal ≠ batas berlaku (400); **tidak ada pengajuan terbuka lain** (422) |
| `MENUNGGU` | Setujui | `DISETUJUI` | `Approve` | Versi cocok (409); pengaju non-SuperAdmin ≠ penyetuju (422) |
| `MENUNGGU` | Tolak | `DITOLAK` | `Reject` | Versi cocok (409); **alasan penolakan wajib** (422); pengaju boleh menolak pengajuannya sendiri (= menarik) |
| `DISETUJUI` + `BELUM_AKTIF` | Aktifkan | `DISETUJUI` + `AKTIF` | `Activate` | Versi cocok (409); batas lama digantikan atomik |

Tindakan di luar tabel dijawab 400 dengan pesan yang jelas: setujui ulang, tolak yang sudah disetujui/ditolak, aktifkan yang masih menunggu / ditolak / sudah aktif / sudah digantikan.

### 3.2 Yang berubah pada pembayaran

> **Contoh.** Batas berlaku Rp5.000.000. Diajukan Rp10.000.000.
>
> | Keadaan pengajuan | Batas yang dipakai AR/AP |
> | --- | --- |
> | `MENUNGGU` | Rp5.000.000 |
> | `DISETUJUI`, belum diaktifkan | Rp5.000.000 |
> | `DITOLAK` | Rp5.000.000 |
> | `AKTIF` (setelah **Aktifkan**) | Rp10.000.000 — hanya untuk pembayaran **berikutnya** |

Alasannya struktural: AR/AP membaca `MstDirectPaymentThresholds` dengan `IsActive && !IsDelete` dan **tidak** membaca tabel pengajuan. Hanya `ActivateAsync` yang menulis `master.Amount`. Tanpa baris aktif AR/AP tetap fail-closed; pengajuan pertama yang belum diaktifkan **tidak** dianggap batas.

### 3.3 Aktivasi (atomik)

Dalam satu transaksi dan satu advisory lock: (1) status & versi diperiksa; (2) baris batas aktif diperbarui di tempat (atau **dibuat** pada penetapan pertama) dengan `RowVersion` diputar; (3) pengajuan yang sebelumnya `AKTIF` menjadi `DIGANTIKAN`; (4) pengajuan ini `AKTIF` dengan `EffectiveFrom = ActivatedAt = waktu aktivasi (UTC)` dan `ActivatedPreviousAmount`; (5) kejadian `THRESHOLD_ACTIVATED` dan `THRESHOLD_SUPERSEDED` ditulis. Gagal di tengah → rollback seluruhnya.

**Tanggal Berlaku = waktu aktivasi**, bukan tanggal pengajuan atau persetujuan. **Batas lama (legacy)**: batas yang sudah aktif sebelum alur ini ada **tidak** diberi pengajuan, persetujuan, atau tanggal berlaku fiktif; saat pertama digantikan, riwayat mencatat kejadian `THRESHOLD_SUPERSEDED` bertanda `IsLegacyBaseline = true`, dan layar menampilkan "Data historis belum tersedia".

### 3.4 Maker-checker dan SuperAdmin

| Aktor | Ajukan | Setujui | Tolak | Aktifkan |
| --- | --- | --- | --- | --- |
| Pemegang izin biasa | ya | ya, **kecuali pengajuannya sendiri (422)** | ya (termasuk milik sendiri) | ya |
| SuperAdmin (dikenali backend) | ya | ya, **termasuk pengajuannya sendiri** — dicatat `IsSelfApproval = true` | ya | ya |

SuperAdmin dikenali lewat `AccessPermissionService.IsSuperAdminAsync(User)` (peran `SuperAdmin` atau `UserType` SuperAdmin; logika yang sama dengan `HasAccessAsync`), **bukan** dari nama, email, atau kiriman layar. SuperAdmin **tidak** melewati: validasi nominal, status, versi, transaksi atomik, satu batas aktif, atau pencatatan aktor dan waktu. Pengecualian itu hanya pada larangan menyetujui pengajuan sendiri, hanya untuk fitur ini, dan tercatat pada kejadian (`ActorIsSuperAdmin`, `IsSelfApproval`).

### 3.5 Jalur tidak normal

| Kejadian | Respons |
| --- | --- |
| Dua penyetuju menekan Setujui bersamaan | Yang kedua mendapat `409` (versi basi) |
| Klik ganda Aktifkan | Yang kedua `409`/`400`; batas diganti satu kali |
| Pengajuan baru saat masih ada yang `MENUNGGU` atau `DISETUJUI`-belum-aktif | `422` menyebut nomor pengajuan yang masih terbuka |
| Pengaju non-SuperAdmin menyetujui sendiri | `422` "Pengaju tidak dapat menyetujui pengajuannya sendiri." |
| Tanpa izin Role Management | `403` dari filter izin |
| `PUT` lama | `410`, **tidak mengubah apa pun dan tidak membuat pengajuan diam-diam** |

---

## 4. Perubahan yang dikerjakan

### 4.1 Berkas (diverifikasi `git status`)

| Berkas | Perubahan |
| --- | --- |
| `Models/MstDirectPaymentThresholdProposal.cs` (**baru**) | Pengajuan: nominal, nominal sebelumnya, alasan, status pengajuan & aktivasi, pengaju/penyetuju/penolak/pengaktif beserta waktunya, `EffectiveFrom`, `RowVersion`; konstanta status & jenis kejadian |
| `Models/MstDirectPaymentThresholdEvent.cs` (**baru**) | Kejadian append-only: jenis, nominal sebelum/sesudah, aktor, waktu, status sebelum/sesudah, alasan, `ActorIsSuperAdmin`, `IsSelfApproval`, `IsLegacyBaseline` |
| `Repositories/Configurations/…/MstDirectPaymentThresholdProposalConfiguration.cs` (**baru**) | Presisi, `CHECK` status, indeks unik nomor, **indeks unik parsial satu pengajuan `AKTIF`**, `CK_…ActivationRequiresApproval` |
| `Repositories/Configurations/…/MstDirectPaymentThresholdEventConfiguration.cs` (**baru**) | `CHECK` jenis kejadian, FK `Restrict` ke pengajuan, indeks |
| `Repositories/ApplicationDbContext.cs` | Dua `DbSet` baru |
| `DTOs/DirectPaymentThresholdProposalDtos.cs` (**baru**) | Request, query, respons pengajuan, ringkasan, versi aktif, riwayat |
| `Services/DirectPaymentThresholdProposalService.cs` (**baru**) | Seluruh alur, transaksi + advisory lock, nama pelaku dibaca batch dari audit |
| `Services/DirectPaymentThresholdService.cs` | **`UpdateAsync` dihapus** (akar masalah); `GetActiveAsync` tetap; istilah "Batas" |
| `Controllers/DirectPaymentThresholdController.cs` | `PUT`→410; 8 endpoint baru; izin `Approve`/`Reject`/`Activate`; pemetaan galat |
| `DTOs/DirectPaymentThresholdDtos.cs`, `Models/MstDirectPaymentThreshold.cs` | Komentar dokumentasi diperbarui (isi tidak berubah) |
| `Services/Security/AccessPermissionService.cs` | **+ `IsSuperAdminAsync(ClaimsPrincipal)`** publik (memakai `IsSuperAdminUser` yang sudah ada); tidak mengubah `HasAccessAsync` |
| `…/BillingManagementServiceCollectionExtensions.cs` | Registrasi DI `DirectPaymentThresholdProposalService` |
| `Receivable/Services/FinanceReceivableService.cs`, `Payable/Services/FinanceSupplierPayableService.cs` | **Hanya** satu literal pesan: "Ambang…" → "Batas pembayaran langsung belum ditetapkan, …". Logika tidak berubah |

Tidak diubah: `MstDirectPaymentThreshold` (skema), `MstDirectPaymentThresholdConfiguration`, nama route, nama resource izin, logika pembacaan batas AR/AP.

Catatan: `CurrenciesController`, `CurrencyDtos`, `CurrencyService` pada `git status` berasal dari tugas Mata Uang & Kurs sebelumnya.

### 4.2 Dampak

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru (bagian 5). **Memutus:** `PUT` kini 410. Klien lama yang mengubah batas lewat `PUT` tidak lagi bisa |
| Database | Dua tabel **baru**; tabel master tidak diubah. Butuh skema |
| Keamanan/Auth | Tiga izin baru di layar Akses Role (`Approve`, `Reject`, `Activate`), **tidak diberikan otomatis**. Izin `Update` berubah arti: "boleh mengajukan" |

---

## 5. Dokumentasi endpoint

#### Corporate / Finance Management / Master Data / Direct Payment Threshold

Base URL: `api/v1/corporate/finance-management/master-data/direct-payment-threshold`

| Method | Path | Kegunaan | Hak akses | Status |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Batas yang **berlaku** (404 bila belum ada) | `MstDirectPaymentThreshold : Read` | Tetap |
| `PUT` | `/` | **Dinonaktifkan** — 410, tidak mengubah data | `MstDirectPaymentThreshold : Update` | Diubah (memutus) |
| `GET` | `/proposals/summary` | Total/menunggu/disetujui-belum-aktif/ditolak + versi batas berlaku | `Read` | **Baru** |
| `GET` | `/proposals` | Daftar pengajuan (filter: `search`, `approvalStatus`, `activationStatus`, `startDate`, `endDate`, paging) | `Read` | **Baru** |
| `GET` | `/proposals/{id}` | Detail satu pengajuan | `Read` | **Baru** |
| `POST` | `/proposals` | Mengajukan (body: `amount`, `changeReason`) | `Update` | **Baru** |
| `POST` | `/proposals/{id}/approve` | Menyetujui (body: `expectedRowVersion`) | `Approve` | **Baru** |
| `POST` | `/proposals/{id}/reject` | Menolak (body: `expectedRowVersion`, `rejectionReason`) | `Reject` | **Baru** |
| `POST` | `/proposals/{id}/activate` | Mengaktifkan (body: `expectedRowVersion`) | `Activate` | **Baru** |
| `GET` | `/history` | Riwayat kejadian (filter: `search`, `eventType`, `startDate`, `endDate`, paging), terbaru dulu | `Read` | **Baru** |

Kode status: `200` berhasil; `400` tindakan tidak sah untuk status saat ini / nominal tidak sah / filter tidak dikenal; `403` tanpa izin Role Management; `404` tidak ditemukan; `409` **hanya** benturan versi (data sudah diubah pihak lain); `410` `PUT` lama; `422` aturan bisnis (alasan kosong, pengajuan terbuka lain, menyetujui sendiri).

---

## 6. Penyimpanan baru — butuh skema (`NEEDS CONFIRMATION`)

Source model dan konfigurasi sudah ada, tetapi **tidak ada migration**. Dua jalur, pilih satu:

1. **Dianjurkan:** pemilik menjalankan `dotnet ef migrations add AddDirectPaymentThresholdWorkflow` lalu menerapkannya (jalur standar repository; menghasilkan DDL yang persis cocok dengan model dan snapshot).
2. DDL referensi di bawah (idempoten, **tidak dijalankan oleh saya**). Bila dipakai, jangan lagi membuat migration yang sama karena akan bentrok.

```sql
CREATE TABLE IF NOT EXISTS public."MstDirectPaymentThresholdProposal" (
  "Id" uuid NOT NULL,
  "ProposalNumber" character varying(40) NOT NULL,
  "ProposedAmount" numeric(18,2) NOT NULL,
  "PreviousActiveAmount" numeric(18,2) NULL,
  "ChangeReason" character varying(500) NOT NULL,
  "ApprovalStatus" character varying(20) NOT NULL,
  "ActivationStatus" character varying(20) NOT NULL,
  "RequestedBy" uuid NOT NULL,
  "RequestedAt" timestamp with time zone NOT NULL,
  "ApprovedBy" uuid NULL,  "ApprovedAt" timestamp with time zone NULL,
  "RejectedBy" uuid NULL,  "RejectedAt" timestamp with time zone NULL,
  "RejectionReason" character varying(500) NULL,
  "ActivatedBy" uuid NULL, "ActivatedAt" timestamp with time zone NULL,
  "EffectiveFrom" timestamp with time zone NULL,
  "ActivatedPreviousAmount" numeric(18,2) NULL,
  "SupersededAt" timestamp with time zone NULL, "SupersededBy" uuid NULL, "SupersededByProposalId" uuid NULL,
  "RowVersion" uuid NOT NULL,
  "CreateDateTime" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP, "CreateBy" uuid NOT NULL,
  "UpdateDateTime" timestamp with time zone NULL, "UpdateBy" uuid NOT NULL,
  "DeleteDateTime" timestamp with time zone NULL, "DeleteBy" uuid NOT NULL,
  "CancelDateTime" timestamp with time zone NULL, "CancelBy" uuid NOT NULL,
  "IsCancel" boolean NOT NULL DEFAULT false, "IsDelete" boolean NOT NULL DEFAULT false,
  CONSTRAINT "PK_MstDirectPaymentThresholdProposal" PRIMARY KEY ("Id"),
  CONSTRAINT "CK_MstDirectPaymentThresholdProposal_Amount" CHECK ("ProposedAmount" > 0),
  CONSTRAINT "CK_MstDirectPaymentThresholdProposal_ApprovalStatus" CHECK ("ApprovalStatus" IN ('MENUNGGU', 'DISETUJUI', 'DITOLAK')),
  CONSTRAINT "CK_MstDirectPaymentThresholdProposal_ActivationStatus" CHECK ("ActivationStatus" IN ('BELUM_AKTIF', 'AKTIF', 'DIGANTIKAN')),
  CONSTRAINT "CK_MstDirectPaymentThresholdProposal_ActivationRequiresApproval" CHECK ("ActivationStatus" = 'BELUM_AKTIF' OR "ApprovalStatus" = 'DISETUJUI')
);
CREATE UNIQUE INDEX IF NOT EXISTS "IX_MstDirectPaymentThresholdProposal_ProposalNumber" ON public."MstDirectPaymentThresholdProposal" ("ProposalNumber");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_MstDirectPaymentThresholdProposal_Active" ON public."MstDirectPaymentThresholdProposal" ("ActivationStatus") WHERE "ActivationStatus" = 'AKTIF' AND "IsDelete" = false;
CREATE INDEX IF NOT EXISTS "IX_MstDirectPaymentThresholdProposal_RequestedAt" ON public."MstDirectPaymentThresholdProposal" ("RequestedAt");
CREATE INDEX IF NOT EXISTS "IX_MstDirectPaymentThresholdProposal_Status" ON public."MstDirectPaymentThresholdProposal" ("ApprovalStatus", "ActivationStatus");

CREATE TABLE IF NOT EXISTS public."MstDirectPaymentThresholdEvent" (
  "Id" uuid NOT NULL,
  "ProposalId" uuid NOT NULL,
  "RelatedProposalId" uuid NULL,
  "EventType" character varying(30) NOT NULL,
  "PreviousAmount" numeric(18,2) NULL, "NewAmount" numeric(18,2) NULL,
  "ActorUserId" uuid NOT NULL,
  "OccurredAt" timestamp with time zone NOT NULL,
  "StatusBefore" character varying(40) NULL, "StatusAfter" character varying(40) NOT NULL,
  "Reason" character varying(500) NULL,
  "ActorIsSuperAdmin" boolean NOT NULL DEFAULT false, "IsSelfApproval" boolean NOT NULL DEFAULT false, "IsLegacyBaseline" boolean NOT NULL DEFAULT false,
  "CreateDateTime" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP, "CreateBy" uuid NOT NULL,
  "UpdateDateTime" timestamp with time zone NULL, "UpdateBy" uuid NOT NULL,
  "DeleteDateTime" timestamp with time zone NULL, "DeleteBy" uuid NOT NULL,
  "CancelDateTime" timestamp with time zone NULL, "CancelBy" uuid NOT NULL,
  "IsCancel" boolean NOT NULL DEFAULT false, "IsDelete" boolean NOT NULL DEFAULT false,
  CONSTRAINT "PK_MstDirectPaymentThresholdEvent" PRIMARY KEY ("Id"),
  CONSTRAINT "FK_MstDirectPaymentThresholdEvent_Proposal" FOREIGN KEY ("ProposalId") REFERENCES public."MstDirectPaymentThresholdProposal" ("Id") ON DELETE RESTRICT,
  CONSTRAINT "CK_MstDirectPaymentThresholdEvent_EventType" CHECK ("EventType" IN ('PROPOSAL_CREATED', 'PROPOSAL_APPROVED', 'PROPOSAL_REJECTED', 'THRESHOLD_ACTIVATED', 'THRESHOLD_SUPERSEDED'))
);
CREATE INDEX IF NOT EXISTS "IX_MstDirectPaymentThresholdEvent_ProposalId" ON public."MstDirectPaymentThresholdEvent" ("ProposalId");
CREATE INDEX IF NOT EXISTS "IX_MstDirectPaymentThresholdEvent_OccurredAt" ON public."MstDirectPaymentThresholdEvent" ("OccurredAt");
CREATE INDEX IF NOT EXISTS "IX_MstDirectPaymentThresholdEvent_EventType" ON public."MstDirectPaymentThresholdEvent" ("EventType");
```

**Perlindungan data existing:** DDL hanya `CREATE`; `MstDirectPaymentThreshold` dan batas aktif yang ada **tidak disentuh dan tidak dihapus**. Tidak ada backfill: batas lama tetap menjadi baseline legacy tanpa persetujuan fiktif. Perubahan berikutnya wajib lewat alur baru.

Selama tabel belum ada: endpoint `/proposals*` dan `/history` gagal (500); `GET /` dan seluruh pembayaran AR/AP **tidak terpengaruh**; batas tidak dapat diubah sama sekali (aman, bukan terbuka).

---

## 7. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| QBE conformance (11 berkas) | VIOLATION 0 / REVIEW 0 | `PASS` |
| Penelusuran source (tes statis FE membaca source BE) | Hanya `ActivateAsync` menulis `master.Amount`; AR/AP tidak membaca tabel pengajuan; pasangan `AccessAction`/`AccessPermission` cocok; `PUT` tidak memanggil service | `PASS` (statis) |
| `dotnet restore` / `dotnet build` | **Tidak dijalankan** — dilarang | `NOT RUN` |
| Terjemahan LINQ→SQL (`ILike` berkarakter pelarian, `GroupBy`, join navigasi), transaksi + advisory lock | Belum terbukti | `NOT RUN` |
| Migration / DDL / DML / UAT / panggilan API | Tidak dijalankan | `NOT RUN` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

---

## 8. Keputusan terbuka dan risiko

| ID | Hal | Status |
| --- | --- | --- |
| DPT-D1 | **Penyetuju = pemegang izin `Approve`**, dipetakan admin ke jabatan Kepala Departemen/Bagian Keuangan di Akses Role. Kode **tidak** menulis nama peran/departemen/jabatan (dilarang), dan `FinanceApprovalRoles` (Supervisor/Manajer Finance) **tidak** dipakai. Identitas resmi jabatan itu belum ditetapkan | `NEEDS CONFIRMATION` |
| DPT-D2 | Satu pejabat cukup (tanpa jenjang bertingkat) | `NEEDS CONFIRMATION` |
| DPT-D3 | Pengaju non-SuperAdmin tidak boleh menyetujui sendiri; SuperAdmin boleh dan dicatat | `NEEDS CONFIRMATION` |
| DPT-D4 | Pemegang `Activate` selain SuperAdmin ditentukan admin lewat Akses Role (tidak diberikan otomatis) | `NEEDS CONFIRMATION` |
| DPT-D5 | Alasan penolakan wajib (diterapkan) | `NEEDS CONFIRMATION` |
| DPT-D6 | Pengajuan baru **ditolak** selama ada pengajuan `MENUNGGU` atau `DISETUJUI`-belum-aktif. Risiko: pengajuan disetujui yang tidak ingin diaktifkan menahan pengajuan baru (aksi Tolak hanya dari `MENUNGGU`); jalan keluar saat ini = aktifkan lalu ajukan koreksi | `NEEDS CONFIRMATION` |
| DPT-D7 | Penomoran `BPL-yyyyMMdd-<8 hex Id>` dibentuk dari Id (unik, bukan berurutan). Penyedia number-series platform tidak dipakai agar tidak butuh seed tambahan | `NEEDS CONFIRMATION` |
| DPT-D8 | **Amandemen kontrak terkunci**: `FIN-DES-086` ("riwayat sengaja tidak dibuat"), `FIN-DEC-145/153` ("berlaku seketika"), `FIN-PERM-1.7 G.5` ("tanpa jenjang persetujuan"), `BE-FIN-076/086`, `api-contract.md`, `permission-audit-matrix.md` G.2 ("nol action baru"). Dokumen **tidak saya ubah** | `NEEDS CONFIRMATION` |
| DPT-D9 | Prefix `Mst*` untuk entitas pengajuan/kejadian (alternatif `Fin*` butuh keputusan registry) | `NEEDS CONFIRMATION` |
| DPT-R1 | Penyetuju yang melihat pengajuan tidak otomatis berwenang secara organisasi: kewenangan murni izin | Diketahui |
| DPT-R2 | `PUT` lama kini 410 — klien di luar repositori ini yang masih memakainya akan gagal | Diketahui |

---

## 9. Status Git

Berkas task ini: 12 `M` + 6 `??` source (daftar bagian 4.1) + laporan ini; tidak ada stage/commit/push; **0 berkas migration**.

Langkah berikutnya: pemilik membuat migration atau menerapkan DDL; `dotnet build`; admin mencentang `Approve`/`Reject`/`Activate` pada Akses Role untuk jabatan yang berwenang; UAT dengan data uji (batas Rp5.000.000 → ajukan Rp10.000.000 → setujui → pastikan AR/AP masih Rp5.000.000 → aktifkan → Rp10.000.000); keputusan DPT-D1…D9; amandemen blueprint.
