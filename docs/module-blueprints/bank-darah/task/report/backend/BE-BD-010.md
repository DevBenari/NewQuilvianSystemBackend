# Laporan Perubahan Backend — `BE-BD-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BD-010` |
| Judul | Koreksi pencatatan pemberian dua tahap |
| Roadmap | [roadmap/backend-roadmap.md](../../../roadmap/backend-roadmap.md) bagian 5 |
| Trace | `DEC-BD-030/034/041/051/052/053/054`; `BD-DOM-23`, `BD-DOM-17`; `INV-BD-021/024/033`; api-contract `v4` baris 110–113; state-transition §3 dan §6; validation-matrix `VAL-BD-016/024/025/049/073/074/075/076/077`; kamus data `BbkIssuanceCorrection` |
| Contract version | `v4` — **`approved`** |
| Dependency | `G1` ✅, `G2b` ✅, `BE-BD-007` ✅; `OQ-BD-014` ✅ (`DEC-BD-051`); `VAL-BD-049` ✅ (`DEC-BD-052`) |
| Klasifikasi | `MEDIUM` — satu entity baru, satu enum, satu konfigurasi EF, empat endpoint, perhitungan pemenuhan diaktifkan; nol modul lain tersentuh |
| Target tulis | `DevBenari/NewQuilvianSystemBackend` cabang `sukmagp` |
| Model | `claude-opus-5` |
| Commit backend saat dikerjakan | Mulai dari `24aa4390` (`docs(bank-darah): align roadmap and PRD before BE-BD-010`), working tree bersih. **Belum ter-commit** atas instruksi pemilik |
| Tanggal | 17 September 2026 |
| Status | ✅ **SELESAI 17 September 2026.** Ketujuh acceptance terbukti runtime lewat API sungguhan + verifikasi database; kedelapan kode wajib `VAL-BD-024/025/049/073/074/075/076/077` ditambah `VAL-BD-016` terkirim dengan HTTP, `errors.code`, dan pesan persis `validation-matrix.md`; empat aktor non-SuperAdmin; migration terterapkan `0 pending`; build `0 Error(s)` / `198 Warning(s)`; QBE Strict `PASS`. 28/28 kasus runtime lulus |

---

## 1. Berkas implementasi

| Berkas | Sifat |
| --- | --- |
| `Areas/HealthServices/BloodBankManagement/Enums/BbkCorrectionStatus.cs` | **BARU** — `Requested`/`Approved`/`Rejected` |
| `Areas/HealthServices/BloodBankManagement/Models/BbkIssuanceCorrection.cs` | **BARU** — entity `BD-DOM-23`, kolom persis kamus data |
| `Repositories/Configurations/HealthServices/BloodBankManagement/BbkIssuanceCorrectionConfiguration.cs` | **BARU** — tiga index, FK `Restrict`, `CorrectionStatus` sebagai token konkurensi |
| `Areas/HealthServices/BloodBankManagement/DTOs/IssuanceCorrectionDtos.cs` | **BARU** — `RequestIssuanceCorrectionRequest`, `DecideCorrectionRequest`, `IssuanceCorrectionDto` (nama persis kontrak) |
| `Areas/HealthServices/BloodBankManagement/Services/BbkBloodUnitService.cs` | **DISENTUH** — `RequestIssuanceCorrectionAsync`, `ApproveIssuanceCorrectionAsync`, `RejectIssuanceCorrectionAsync`, pembacaan koreksi, aksi `RequestIssuanceCorrection` pada kantong `Issued`; penolong alasan terkendali menerima pesan kategori opsional (bawaan `BE-BD-009` tidak berubah) |
| `Areas/HealthServices/BloodBankManagement/Controllers/BbkBloodUnitController.cs` | **DISENTUH** — `GET /{id}/corrections`, `POST /{id}/corrections`, `POST …/approve`, `POST …/reject` |
| `Areas/HealthServices/BloodBankManagement/DTOs/BloodUnitDtos.cs` | **DISENTUH** — `IssuanceCorrections` pada detail kantong |
| `Areas/HealthServices/BloodBankManagement/Services/BbkBloodOrderService.cs` | **DISENTUH** — ringkasan pemenuhan `BD-DOM-17` kini menghitung kantong `Issued` nyata, dikurangi kantong ber-koreksi `Approved` |
| `Repositories/ApplicationDbContext.cs` | **DISENTUH** — satu `DbSet` |
| `Migrations/20260917055332_AddBbkIssuanceCorrection*.cs` + snapshot | **BARU** |

### 1.1 Aturan yang ditegakkan

| Aturan | Penegak |
| --- | --- |
| Pemberian asal append-only; kantong tetap `Issued`; nol transisi status | Pengajuan hanya menambah satu baris koreksi; keputusan hanya mengisi kolom keputusan baris itu. `BbkBloodUnit` (status, `Version`, `IssuedAt`, `IssuedToPatientId`, alokasi aktif) dan `BbkTransitionHistory` tidak ditulis |
| `Requested → Approved` / `Requested → Rejected`, keputusan sekali | `CorrectionStatus != Requested` → `422 VAL-BD-075`; keputusan serentak dijaga token konkurensi `CorrectionStatus` → yang kalah `422 VAL-BD-075` |
| Pengaju ≠ pemutus, walau memegang kedua butir | `RequestedByUserId == pelaku` → `422 VAL-BD-073` |
| Hak akses | `BloodUnit : Correct` (`403 VAL-BD-024`), `BloodUnit : ApproveCorrection` (`403 VAL-BD-074`); keduanya di-seed dari `[AccessAction]` |
| Bukti pendukung teks, wajib | `422 VAL-BD-076` |
| Alasan penolakan wajib | `422 VAL-BD-077` |
| Alasan terkendali kategori `IssuanceCorrection` | `400 VAL-BD-016` |
| Pelaku dari akun login | Tidak ada isian pelaku, status, atau pemutus di body |
| Billing tidak disentuh | Nol kode Billing |

---

## 2. Keputusan pemilik yang diambil sebelum implementasi

Kanonik tidak menetapkan dua hal yang menentukan bentuk implementasi. Keduanya ditanyakan dan diputuskan
pemilik pada sesi ini, 17 September 2026, **sebelum** source ditulis:

| Celah kanonik | Keputusan pemilik | Implementasi |
| --- | --- | --- |
| Bagaimana `POST /corrections` mengenali percobaan menganulir pemberian (`VAL-BD-025`) atau memindahkannya ke pasien lain (`VAL-BD-049`) — kamus data hanya punya isian teks bebas | **Isian penjaga yang hanya ditolak, tidak pernah disimpan** | `RequestIssuanceCorrectionRequest.AnnulIssuance = true` → `422 VAL-BD-025`; `IssuedToPatientId` berbeda dari penerima pemberian → `422 VAL-BD-049` |
| Efek koreksi `Approved` terhadap angka pemenuhan — `BuildFulfillment` masih mengunci `IssuedQuantity = 0` | **Hitung kantong `Issued` nyata; keluarkan kantong ber-koreksi `Approved`** | Per baris order: kantong `Issued` dengan alokasi aktif ke baris itu, tanpa koreksi `Approved`. `Requested`/`Rejected` tidak mengubah angka. Status order tidak disentuh |

Kedua keputusan kini tercatat pada register: **`DEC-BD-053`** (isian penjaga request-only) dan
**`DEC-BD-054`** (koreksi `Approved` dikeluarkan dari `BD-DOM-17`) — `00-interview-decisions.md` §8.30.
**Riwayat:** belum bernomor saat laporan ini pertama ditulis.

---

## 3. Migration

`20260917055332_AddBbkIssuanceCorrection`.

| Pemeriksaan | Hasil |
| --- | --- |
| Scope `Up` | Satu `CreateTable` `public.BbkIssuanceCorrection` (22 kolom: 12 kamus data + 10 `IdentityModel`), PK, tiga index (`BloodUnitId`, `CorrectionStatus`, `RequestedByUserId`), satu FK `Restrict` ke `BbkBloodUnit`. **Nol operasi tabel lain** |
| Scope `Down` | Satu `DropTable` tabel yang sama |
| Snapshot | Hanya entity `BbkIssuanceCorrection` beserta relasinya |
| `has-pending-model-changes` | "No changes have been made to the model since the last migration." |
| `migrations list` sebelum | Tepat satu `(Pending)` — migration ini |
| `database update` | `Done.` ke `QuilvianNewDevSukma` |
| `migrations list` sesudah | **0 pending** |
| PostgreSQL | 22 kolom; `PK` + tiga index terverifikasi di `pg_indexes`; FK `confdeltype = r` (Restrict) |

---

## 4. Runtime acceptance — 7 dari 7

Seluruhnya lewat API sungguhan terhadap `QuilvianNewDevSukma` + verifikasi database. Fixture
`TEST-BD010-20260917135556`: order `ORD-00000088` (4 kantong diminta), tiga kantong `Issued` (`-01`..`-03`)
dan satu `Allocated` (`-04`). Ringkasan pemenuhan awal: diberikan **3**, sisa **1**.

| Acceptance | Kantong | Keadaan awal | Request | HTTP | `errors.code` | Keadaan database sesudah | Hasil |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `AC-BD-050` | `-01` | `Issued` | `POST /corrections` oleh aktor tanpa `Correct` (juga oleh pemegang `ApproveCorrection` saja) | `403` | `VAL-BD-024` | Kantong, koreksi (0), alokasi aktif, transisi tidak bergeser | ✅ |
| `AC-BD-048` | `-01` | `Issued` | `POST /corrections` dengan `annulIssuance: true` | `422` | `VAL-BD-025` | Nol koreksi, kantong utuh | ✅ |
| `AC-BD-049` | `-01` | `Issued` ke pasien A | `POST /corrections` dengan `issuedToPatientId` = pasien B | `422` | `VAL-BD-049` | Nol koreksi, `IssuedToPatientId` tetap pasien A | ✅ |
| `AC-BD-086` | `-01` | `Issued` | `POST /corrections` oleh petugas BDRS (`Correct`) | `200` | — | Koreksi `Requested`, `RequestedByUserId` = pengaju; kantong status 4 `Version` 4 tidak bergeser; **pemenuhan tetap (3, 1)** | ✅ |
| `AC-BD-088` | `-02` | `Issued`, koreksi `Requested` diajukan aktor pemegang **kedua** butir | `POST /approve` (dan `/reject`) oleh aktor yang sama | `422` | `VAL-BD-073` | Koreksi tetap `Requested` | ✅ |
| `AC-BD-087` | `-01` | Koreksi `Requested` | `POST /approve` oleh Dokter BDRS (`ApproveCorrection`) | `200` | — | Koreksi `Approved`, `DecidedByUserId` = pemutus, `DecidedAt` terisi; kantong tetap `Issued`, `Version` tetap, `IssuedAt` tetap, alokasi aktif tetap 1; **pemenuhan (3, 1) → (2, 2)** | ✅ |
| `AC-BD-047` | `-01` | Koreksi diajukan lalu disetujui | `GET /blood-units/{id}` | `200` | — | Detail: `unitStatus` 4, `issuedAt` dan `issuedToPatientId` pemberian asal utuh, `issuanceCorrections` = `Disetujui`; pemenuhan dihitung ulang | ✅ |

### 4.1 Siklus dua tahap dan pemenuhan

| Keadaan | Pemenuhan (diberikan, sisa) |
| --- | --- |
| Awal — 3 `Issued` | (3, 1) |
| Sesudah koreksi `-01` **Requested** | (3, 1) — tidak bergerak (`INV-BD-033`) |
| Sesudah koreksi `-01` **Approved** | (2, 2) — dihitung ulang |
| Sesudah koreksi `-02` **Rejected** (dengan alasan) | (2, 2) — tidak bergerak; koreksi tetap terbaca `Ditolak` |

---

## 5. Contract acceptance — 9 dari 9 (8 kode wajib + `VAL-BD-016`)

Pesan diambil terprogram dari tabel `validation-matrix.md` dan dibandingkan dengan `message` respons.

| Kode | Pemicu yang dibuktikan | HTTP | `errors.code` | Pesan persis |
| --- | --- | --- | --- | --- |
| `VAL-BD-016` | Alasan diketik bebas | `400` ✅ | ✅ | ✅ |
| `VAL-BD-024` | Tanpa `Correct` (dua aktor) | `403` ✅ | ✅ | ✅ |
| `VAL-BD-025` | `annulIssuance: true` | `422` ✅ | ✅ | ✅ |
| `VAL-BD-049` | `issuedToPatientId` pasien lain | `422` ✅ | ✅ | ✅ |
| `VAL-BD-073` | Pengaju menyetujui/menolak sendiri | `422` ✅ | ✅ | ✅ |
| `VAL-BD-074` | Tanpa `ApproveCorrection` (pembaca, dan pengaju yang hanya `Correct`) | `403` ✅ | ✅ | ✅ |
| `VAL-BD-075` | Setujui ulang, tolak sesudah disetujui, setujui sesudah ditolak, pemutus kalah serentak | `422` ✅ | ✅ | ✅ |
| `VAL-BD-076` | Bukti pendukung kosong dan tidak dikirim | `422` ✅ | ✅ | ✅ |
| `VAL-BD-077` | Penolakan tanpa alasan | `422` ✅ | ✅ | ✅ |

**Nol partial mutation.** Setiap penolakan dibandingkan sebelum/sesudah pada: baris kantong (status,
`Version`, `IssuedToPatientId`, `IssuedAt`, `IssuedByUserId`, `CompatibilityEvidenceIdUsed`,
`IssuedViaEmergency`), jumlah koreksi, jumlah alokasi aktif, dan jumlah baris transisi — seluruhnya sama.

---

## 6. Otorisasi

Empat aktor `Employee`, **nol role**, masing-masing pada Department/Position `TEST-BD010` tersendiri.

| Aktor | Policy `BloodUnit` | `POST /corrections` | `POST /approve` / `/reject` |
| --- | --- | --- | --- |
| `test.bd010.req` | `Read`, `Correct` | `200` | `403 VAL-BD-074` |
| `test.bd010.doc` | `Read`, `ApproveCorrection` | `403 VAL-BD-024` | `200` |
| `test.bd010.both` | `Read`, `Correct`, `ApproveCorrection` | `200` | `422 VAL-BD-073` atas koreksinya sendiri; `200` atas koreksi orang lain |
| `test.bd010.none` | `Read` | `403 VAL-BD-024` | `403 VAL-BD-074` |

---

## 7. Konkurensi dan keamanan keadaan

| Uji | Hasil |
| --- | --- |
| Dua pemutus sah (`doc`, `both`) menyetujui koreksi `-03` serentak | `200` + `422 VAL-BD-075`; tepat satu keputusan tersimpan |
| Token `Version` kantong usang pada pengajuan | `409`, nol koreksi |
| Kantong belum `Issued` (`-04`, `Allocated`) | `422`, nol koreksi |
| Isian pelaku/status/pemutus dari body | Tidak ada isian semacam itu; pelaku dari klaim akun login |

---

## 8. Regression minimal

| Uji | Hasil |
| --- | --- |
| Alur alokasi → bukti kecocokan → pemberian normal untuk tiga kantong fixture | `200` seluruhnya — alur `BE-BD-006`/`007` utuh |
| Detail kantong ber-koreksi | Pemberian asal terbaca utuh |
| `POST /cancel-allocation` atas kantong `Issued` | `422`, pesan `VAL-BD-023`; alokasi aktif tetap — pemberian tidak dibalik |
| `POST /reallocate` atas kantong `Issued` | `422` — perilaku `BE-BD-009` tidak berubah |
| Tabel Billing (`BilChargeLine`, `BilChargeComponent`, `BilAdjustment`, `BilProcessingEffect`, `BilInvoiceItem`) | Jumlah baris sama sebelum/sesudah (seluruhnya 0) |
| Fixture `TEST-BD006/007/008/009` | `UpdateDateTime` terakhir sebelum sesi ini — tidak tersentuh |

---

## 9. Build, QBE, dan fixture

| Gate | Hasil |
| --- | --- |
| `dotnet build` (source) | **`Build succeeded`, `0 Error(s)`, `198 Warning(s)`** — sama dengan baseline, nol warning baru |
| `dotnet build` (migration terkompilasi) | **`Build succeeded`, `0 Error(s)`, `198 Warning(s)`** |
| QBE Strict `WorkingTree` | **`PASS`** — 12 berkas, `VIOLATION 0` / `REVIEW 0` / `INFO 0` |
| `git diff --check` | bersih |

Build memakai `-m:1 -nodeReuse:false -p:BuildInParallel=false -p:UseSharedCompilation=false`, tanpa batas heap.

**Fixture ledger `TEST-BD010-20260917135556` — belum dibersihkan.** Seluruhnya lewat API.

- Master: komponen `TBD010-P` `bfd8be25-4925-446b-ab58-e3623567c1f4` · lokasi `TBD010-LOCA`
  `4f902c02-3355-488e-a748-dc0518c82106` · alasan `TBD010-KOREKSI` (`IssuanceCorrection`) dan `TBD010-ALIH`
  (`PendingReviewResolution`).
- Identitas (user · department · position): `req` `bd898c31-bff2-4f1b-b677-e2fdf0c09e8c` ·
  `49e062c6-c15d-4712-a918-6cd67fabcf3f` · `910ef2ff-f349-4c65-8c50-69ee43bd3573`; `doc`
  `7dd684b6-05cf-4264-9816-a04f5b4267be` · `b18bd97e-b1e0-4057-9417-1ae492336b13` ·
  `9c9c184b-de2c-4b11-b3cb-65550f6944ed`; `both` `c5e92c1d-1d65-4894-81e8-a2bd968bd535` ·
  `863308a0-d4d2-49d7-a82e-4b1e68410823` · `850ee028-f747-450b-b4e8-44cfeba0f5bd`; `none`
  `6a4b1d29-331a-47cb-b1ae-83e9287019c6` · `568278f8-9328-4c8a-b47e-05ba1a22e282` ·
  `2eed1ee7-97fe-4a22-8882-18dacc253bde`; beserta employee/workforce profile dan sembilan baris
  `SysAccessPolicy`.
- Transaksional: order `ORD-00000088` `f88aec73-d4ef-43b0-9761-4244b6de2853` (pasien dan kunjungan yang sudah
  ada dibaca, tidak diubah) · permintaan PMI `PMI-00000043` `70ae4ff0-52d5-406a-942b-580d96e6fb45`.

| Kantong | Id | Status | Koreksi |
| --- | --- | --- | --- |
| `-01` | `81af8838-d7a7-4c03-bb8f-df5344fc34f9` | `Issued` | `8082123c-27c2-4933-80bb-4c988602a3fb` **Approved** |
| `-02` | `6494d8a4-646f-44fb-a5dd-6af81d0837c0` | `Issued` | `aaeb7fa8-b97e-48f1-bcef-3232cb79a3da` **Rejected** |
| `-03` | `12288e80-609a-4171-a775-e0c4ddd000e4` | `Issued` | `d88d68d7-5c5b-4d3f-ba3c-18b632a55936` **Approved** (uji serentak) |
| `-04` | `3711631e-6e78-4add-83c3-7979597971b5` | `Allocated` | — |

---

## 10. Gap

### 10.1 Pre-existing — bukan milik task ini, tidak disentuh

- Belum ada perpindahan sesudah `Reallocated` (kontrak).
- `POST /allocate` dan `POST /cancel-allocation` tidak mengisi `errors.code` (`BE-BD-006`); terlihat lagi pada
  regresi `cancel-allocation` di atas.
- `CancelWithReasonRequest` masih memakai `[Required]` (`BE-BD-006`).
- `AC-BD-052`/`057` tanpa definisi skenario; rekonsiliasi `BE-BD-016`.

### 10.2 Tersisa

- ~~Dua keputusan pemilik pada bagian 2 belum bernomor `DEC-BD-*`~~ — **ditutup**: `DEC-BD-053` dan
  `DEC-BD-054`, tercatat pada `00-interview-decisions.md`, `api-contract.md`, dan `data-dictionary.md`.
- ~~Status endpoint pada `api-contract.md` baris 110–113 masih "Rencana"~~ — **ditutup**: kini
  "Terimplementasi `BE-BD-010`"; kode dan pesan tidak berubah.
- **Status order** (`PartiallyFulfilled`/`FullyFulfilled`) tidak diturunkan dari angka pemenuhan pada task ini;
  yang diaktifkan hanya ringkasan angka.
- **Bukti Billing bersifat lemah** — tabel Billing yang diperiksa memang kosong sebelum dan sesudah.
- **Cleanup fixture** `TEST-BD010` menunggu persetujuan pemilik. **Commit** belum dilakukan.
