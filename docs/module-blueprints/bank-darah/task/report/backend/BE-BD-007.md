# Laporan Perubahan Backend — `BE-BD-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BD-007` |
| Judul | Bukti kecocokan dicatat, kantong diberikan lewat gerbang tiga syarat |
| Slice | Bukti kecocokan dan pemberian — jalur kritis `BE-BD-006` → `BE-BD-007` |
| Roadmap | [roadmap/backend-roadmap.md](../../../roadmap/backend-roadmap.md) bagian 5, revisi **11** |
| Trace | `DEC-BD-013`, `DEC-BD-027`, `DEC-BD-028`, `DEC-BD-038`, `DEC-BD-042`; `BD-AGG-03`; `ARCH-BD-POS-01/02/07`; `INV-BD-013`, `INV-BD-019`, `INV-BD-020`, `INV-BD-023`, `INV-BD-029`; api-contract `v4` baris 107-108; state-transition §3; validation-matrix §3, §4b, §7 |
| Contract version | `v4` — **`approved`** |
| Dependency | `G1` ✅, `G2b` ✅, `BE-BD-005` ✅, `BE-BD-006` ✅ — seluruhnya tertutup |
| Klasifikasi | `MEDIUM` — satu entity baru, satu enum baru, satu konfigurasi EF baru, dua endpoint baru, dua berkas shared disentuh secara aditif |
| Task mode | `BACKEND` |
| Target tulis | `DevBenari/NewQuilvianSystemBackend` cabang `sukmagp` |
| Model | `claude-opus-5` |
| Commit backend saat dikerjakan | `HEAD` `6a6ebd4846a3d2f90eb714ada085d7c8bf2beabf` cabang `sukmagp`. **Belum ter-commit** — seluruh perubahan masih di working tree atas instruksi pemilik |
| Tanggal | Validasi runtime business acceptance 14 September 2026 · perbaikan kontrak dan retest kontrak 16 September 2026 |
| Status | ✅ **SELESAI 16 September 2026.** Kedua belas business acceptance terbukti runtime (bagian 4), ketujuh kode `VAL-BD-*` terbukti terkirim dengan pesan persis kontrak (bagian 5), otorisasi terbukti dengan aktor non-SuperAdmin (bagian 6), build `0 Error(s)`, QBE Strict `PASS`. **Riwayat:** 🟡 business behavior 12/12 terbukti 14 September 2026 tetapi kontrak belum konforman — kode validasi tidak terkirim dan enam dari tujuh pesan berbeda dari `validation-matrix.md` |

---

## 1. Berkas implementasi

### 1.1 Source `BE-BD-007` (ditulis sebelum sesi validasi)

| Berkas | Sifat |
| --- | --- |
| `Areas/HealthServices/BloodBankManagement/Models/BbkCompatibilityEvidence.cs` | **BARU** — entity bukti kecocokan |
| `Areas/HealthServices/BloodBankManagement/Enums/BbkCompatibilityResult.cs` | **BARU** — `Compatible = 0`, `Incompatible = 1` |
| `Areas/HealthServices/BloodBankManagement/DTOs/CompatibilityEvidenceDtos.cs` | **BARU** — `RecordEvidenceRequest`, `CompatibilityEvidenceDto`, `IssueUnitRequest`, `BloodUnitIssuanceGateResult` |
| `Repositories/Configurations/HealthServices/BloodBankManagement/BbkCompatibilityEvidenceConfiguration.cs` | **BARU** — konfigurasi EF |
| `Areas/HealthServices/BloodBankManagement/Services/BbkBloodUnitService.cs` | **DISENTUH** — `RecordCompatibilityEvidenceAsync`, `EvaluateIssuanceGateAsync`, `IssueAsync` |
| `Areas/HealthServices/BloodBankManagement/Controllers/BbkBloodUnitController.cs` | **DISENTUH** — dua endpoint baru |
| `Areas/HealthServices/BloodBankManagement/Models/BbkBloodUnit.cs` | **DISENTUH** — `IssuedToPatientId`, `IssuedAt`, `IssuedByUserId`, `IssuedViaEmergency`, `CompatibilityEvidenceIdUsed` |
| `Areas/HealthServices/BloodBankManagement/DTOs/BloodUnitDtos.cs` · `Repositories/ApplicationDbContext.cs` · `BbkBloodUnitConfiguration.cs` | **DISENTUH** — proyeksi detail dan registrasi |

### 1.2 Perbaikan yang lahir dari sesi validasi

| Berkas | Perubahan |
| --- | --- |
| `BbkBloodUnitService.cs` | Hapus blok tanpa kondisi pada `RecordCompatibilityEvidenceAsync` (bagian 3); `BloodUnitResult` menerima `ValidationCode` opsional; `Failed()` menerima `validationCode` opsional; `IssueAsync` meneruskan `gate.ValidationCode`; lima konstanta pesan disamakan dengan kontrak |
| `BbkBloodUnitController.cs` | `MapFailure` mengisi slot `errors` bila kode ada; endpoint `compatibility-evidence` memakai opt-in `VAL-BD-078` |
| `Attributes/AccessPermissionAttribute.cs` | **SHARED, ADITIF** — properti opsional `DeniedCode` dan `DeniedMessage` |
| `Filters/AccessPermissionFilter.cs` | **SHARED, ADITIF** — dua parameter konstruktor opsional berdefault string kosong; balasan generic tidak berubah bila tidak diisi |
| `docs/.../contracts/api-contract.md` | Baris `POST /{id}/issue` melengkapi daftar penolakan dengan `VAL-BD-020b` |

**`Responses/ApiResponse.cs` tidak disentuh.** Slot `Errors` sudah ada sejak semula dan sudah dipakai modul Billing; tidak ada perubahan envelope.

---

## 2. Migration

`20260914090021_AddBloodCompatibilityEvidenceAndIssuance` — dibuat dan **sudah diterapkan** ke `QuilvianNewDevSukma` sebelum sesi ini. EF melaporkan tidak ada perubahan model sejak migration terakhir. **Nol migration baru dibuat pada sesi validasi maupun perbaikan kontrak**; nol perubahan skema database.

---

## 3. Bug source yang ditemukan dan diperbaiki

**Bukti runtime lebih dulu, bukan inspeksi kode.** Terhadap binary sebelum perbaikan:

```
POST /api/v1/health-services/blood-bank-management/blood-units/5983616c-.../compatibility-evidence
{"evidenceResult":0,"checkedAt":"2026-09-14T10:17:19.308Z","version":2}
→ HTTP 400  {"success":false,"statusCode":400,
             "message":"Hasil pemeriksaan kecocokan tidak dikenali.",...}
```

`BbkCompatibilityEvidence` tetap `0` baris dan `Version` kantong tidak naik. Nilai `0` terbukti sah: kiriman `"Compatible"` justru gagal di model binding dengan pesan berbeda, sehingga request pertama memang lolos binding lalu ditolak oleh service.

**Root cause.** `RecordCompatibilityEvidenceAsync` memuat blok tanpa kondisi tepat sesudah blok `if`:

```csharp
if (!request.EvidenceResult.HasValue || !Enum.IsDefined(request.EvidenceResult.Value))
{
    return Failed(BloodUnitOutcome.Invalid, InvalidCompatibilityResultMessage);
}
{                                                    // tanpa kondisi
    return Failed(BloodUnitOutcome.Invalid, InvalidCompatibilityResultMessage);
}
```

Setiap request yang lolos `if` pertama tetap `return`. Seluruh sisa method — validasi `CheckedAt`, pencarian alokasi aktif, penulisan evidence — tidak pernah dieksekusi. Compile tetap berhasil karena unreachable code hanya menghasilkan warning `CS0162`, bukan error, sehingga bug ini lolos dari bukti build.

**Perbaikan minimal.** Hapus kelima baris blok tanpa kondisi itu saja. Nol validasi lain diubah, nol urutan gerbang diubah. Diterapkan atas persetujuan eksplisit pemilik. Pemindaian seluruh area Bank Darah mengonfirmasi tidak ada pola serupa yang tersisa, dan build sesudahnya tidak lagi memuat `CS0162` pada berkas ini.

---

## 4. Business acceptance runtime — 12 dari 12

Seluruhnya lewat panggilan API sungguhan terhadap `QuilvianNewDevSukma`, ditambah verifikasi database.

| Acceptance | Fixture | HTTP | Bukti database | Hasil |
| --- | --- | --- | --- | --- |
| `AC-BD-018` | `-01` | `422` | status `3`, `Version` tetap, evidence `0` | ✅ |
| `AC-BD-019` | `-02` | `200` | evidence `f9cace80` tersimpan | ✅ |
| `AC-BD-039` | `-02` | `200` | status `4`, `IssuedAt` terisi, `CompatibilityEvidenceIdUsed` = `f9cace80` | ✅ |
| `VAL-BD-079` | `-03` | `422` | evidence `Incompatible` `ad112d99` tetap tersimpan | ✅ |
| `AC-BD-038` | `-04` | `422` | evidence kedaluwarsa `65b7fc8e` utuh, `IsDelete=false` | ✅ |
| `AC-BD-040` | `-05` | `422` | *fail-closed*; evidence segar dan `Compatible`, penolakan murni karena validity `NULL` | ✅ |
| `AC-BD-041` | `-06` | `422` | evidence pasien A `8c051b68` tetap tersimpan | ✅ |
| `AC-BD-042` | `-06` | `200` | `IssuedToPatientId` = pasien B, `CompatibilityEvidenceIdUsed` = `d2d8d971` milik pasien B | ✅ |
| `AC-BD-072` | `-08` | `422` | lokasi `TBD007-LOCX` `IsActive=false` | ✅ |
| `AC-BD-073` | `-08` | `200` | status `4`, **alokasi aktif dipertahankan**, evidence lama dipakai ulang | ✅ |
| `AC-BD-089` | 10 baris evidence | — | `BloodUnitId`, `PatientId`, `EvidenceResult`, `ValidatedByUserId`, `CheckedAt` seluruhnya benar | ✅ |
| `AC-BD-091` | `-01`, `-09` | `200` | `ValidatedByUserId` = `CreateBy` = aktor non-SuperAdmin | ✅ |

**`PatientId` terbukti diturunkan dari alokasi aktif, bukan dari client.** Kantong `-06` memiliki dua baris evidence dengan pasien berbeda mengikuti perpindahan alokasinya, padahal `RecordEvidenceRequest` tidak memuat field `patientId` sama sekali.

---

## 5. Contract acceptance runtime — 7 dari 7

Setiap kode diuji dengan membandingkan balasan aktual terhadap teks yang dibaca langsung dari `contracts/validation-matrix.md` secara terprogram, bukan dicocokkan dengan mata.

| Kode | HTTP | `errors.code` | Pesan persis kontrak | Fixture |
| --- | --- | --- | --- | --- |
| `VAL-BD-017` | `422` | ✅ | ✅ | `-09` berstatus `Tersedia` |
| `VAL-BD-018` | `422` | ✅ | ✅ | `-09` `Allocated`, nol evidence |
| `VAL-BD-019` | `422` | ✅ | ✅ | `-09` evidence pasien A, alokasi pasien B |
| `VAL-BD-020` | `422` | ✅ | ✅ | `-04` evidence `CheckedAt` mundur 7 hari, validity `24` |
| `VAL-BD-020b` | `422` | ✅ | ✅ | `-05` komponen `TBD007-N` validity `NULL` |
| `VAL-BD-065` | `422` | ✅ | ✅ | `-01` di lokasi nonaktif |
| `VAL-BD-078` | `403` | ✅ | ✅ | aktor non-SuperAdmin tanpa `BloodUnit : Compatibility` |
| `VAL-BD-079` | `422` | ✅ | ✅ | `-03` evidence `Incompatible` |

Bentuk balasannya memakai envelope yang sudah ada, mengikuti pola `BillingFolioController`:

```json
{ "success": false, "statusCode": 422,
  "message": "Bukti kecocokan yang ada bukan untuk pasien ini. Catat bukti kecocokan terhadap pasien tujuan.",
  "data": null, "errors": { "code": "VAL-BD-019" }, "timestamp": "..." }
```

**Operasi yang ditolak tidak mengubah apa pun.** Snapshot database diambil tepat sebelum dan sesudah enam penolakan berturut-turut; keduanya **identik** — status, `Version`, `IssuedAt`, `IssuedToPatientId`, `CompatibilityEvidenceIdUsed`, jumlah evidence, dan jumlah alokasi tidak bergeser.

---

## 6. Otorisasi runtime dengan aktor non-SuperAdmin

SuperAdmin melewati pemeriksaan hak akses, sehingga `AC-BD-090` **tidak** dapat dibuktikan dengannya. Dibuat aktor uji sungguhan: `test.bd007.validator@rsmmc.local`, `UserType` `Employee`, `roles` kosong, pada Department dan Position `TEST-BD007` tersendiri.

| Keadaan | Endpoint | Hasil |
| --- | --- | --- |
| Tanpa `Compatibility` | `GET /blood-units/{id}` (`BloodUnit : Read` dimiliki) | `200` — membuktikan akun dan token sah |
| Tanpa `Compatibility` | `POST /{id}/compatibility-evidence` | `403` + `VAL-BD-078` + pesan kontrak persis |
| Dengan `Compatibility` | `POST /{id}/compatibility-evidence` — **request identik** | `200`, evidence tersimpan dengan `ValidatedByUserId` = aktor itu sendiri |

Variabel tunggal antara ditolak dan diterima benar-benar hanya permission. Baris evidence hasil `AC-BD-091` memuat `ValidatedByUserId` = `CreateBy` = `ce41e4b5-…`, membuktikan pelaksana pemeriksaan boleh sekaligus menjadi validator ketika memang berwenang (`DEC-BD-042`).

### 6.1 Bukti backward compatibility `AccessPermissionFilter`

Perluasan filter bersifat opt-in. Dengan aktor uji yang sama:

| Endpoint | Opt-in? | Balasan |
| --- | --- | --- |
| `POST /blood-units/{id}/allocate` (`BloodUnit : Allocate`) | tidak | `403` · `"Anda tidak memiliki akses ke menu atau fitur ini."` · `errors: null` |
| `GET /master-data/blood-components/options` (`BloodComponent : Read`, modul lain) | tidak | `403` · pesan generic yang sama · `errors: null` |
| `GET /blood-units/{id}` (`BloodUnit : Read`, dimiliki) | — | `200` |

Balasan generic tidak berubah sedikit pun, di controller yang sama maupun di modul lain. **Keputusan izin tidak disentuh** — yang dikhususkan hanya isi balasan sesudah filter memutuskan menolak, sehingga tidak ada pemeriksaan hak akses kedua yang dapat menyimpang dari yang pertama.

---

## 7. Build dan QBE

| Gate | Hasil |
| --- | --- |
| `dotnet build` | **`0 Error(s)`** / `198 Warning(s)`, `Time Elapsed 00:47:51`. Seluruh warning adalah `CS1573`/`CS1574`/`CS1587`/`CS1591` dokumentasi XML yang sudah ada sebelum task ini; nol diperbaiki di scope ini |
| QBE Strict | **`PASS`** — `./tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict`, scope `WorkingTree`, checker `G6-E2C`, 15 berkas dievaluasi, `VIOLATION 0` / `REVIEW 0` / `INFO 0`, `suppressedViolationCount 0`, `blockingRuleIds []` |
| `git diff --check` | bersih |
| Migration baru | **nol** |
| Perubahan skema database | **nol** |

---

## 8. Fixture ledger `TEST-BD007` — belum dibersihkan

Seluruhnya dibuat lewat API, bukan tulis database langsung.

**Master.** Komponen `TBD007-V` `4f265dac-cd4f-4104-a69c-4a16084edd45` (validity `24`) dan `TBD007-N` `50da7c5b-0aec-4099-967a-1f908a58a71b` (validity `NULL`) · lokasi `TBD007-LOCY` `9914657d-f638-4ccd-81a8-0817c07c5ab7` (aktif) dan `TBD007-LOCX` `a64ea8a9-ca93-4c37-a42f-1ee3bae258a9` (**sengaja nonaktif** sisa `AC-BD-072`, kini kosong) · alasan `TBD007-BATALALOK` `3a9d576d-51a4-45b0-a18c-b661edd764df`.

**Identitas.** Department `710e3232-960e-4036-85b9-204bccac5604` (`DPT-RSMMC-00011`) · Position `ee587f4d-b505-449c-b8bb-f857555b30f2` · Employee `caf94515-7074-4fc3-b315-7ec5b3fd9f33` (`EMP-RSMMC-00029`) · WorkforceProfile `8c3bf902-88eb-4718-9739-3c8773cf2eab` · `AspNetUsers` `ce41e4b5-4fe9-4e18-ac3d-88fabfac6ca6` (`USR-RSMMC-00051`) beserta satu baris `AspNetUserOrganization` · **tiga baris `SysAccessPolicy`** (`Read`, `Issue`, `Compatibility`) — dikembalikan ke keadaan ledger sesudah retest `VAL-BD-078`.

**Transaksional.** Order `ORD-00000083` `80546f53-08ab-4b72-91eb-7365f032a7a3` (dua baris) · Order `ORD-00000084` `d99daa80-a58d-41dd-90a5-fcbfde7632b2` (satu baris) · Permintaan PMI `PMI-00000039` `49d498e1-9b52-45a8-91d2-0b84245a9cfd` dan `PMI-00000040` `aa2a4ad5-ae9c-478b-9daf-d4ab2a03be0b` · delapan kantong `TEST-BD007-20260914171359-01/02/03/04/05/06/08/09` beserta penempatan dan alokasinya · **sepuluh baris `BbkCompatibilityEvidence`**.

**Keadaan akhir kantong.** `-02`, `-06`, `-08` `Issued` (terminal, tidak dapat dibalik); `-01`, `-03`, `-04`, `-05`, `-09` `Allocated`. Kedelapannya berada di `TBD007-LOCY` yang aktif.

**Cleanup belum dijalankan dan belum disetujui.** Jangan menghapus berdasarkan prefix saja; kantong `Issued` bersifat terminal dan tiga baris `SysAccessPolicy` perlu dicabut terpisah.

---

## 9. Fixture `BE-BD-006` tidak tersentuh

Diverifikasi ulang pada penutupan: kedelapan kantong `TEST-BD006-20260914094559-*` identik nilainya dengan keadaan sebelum sesi — `-01` `Received`/`v0`, `-02` `Allocated`/`v3`, `-03` `Available`/`v5`, `-04` `Allocated`/`v2`, `-05` `Allocated`/`v2`, `-06` `Issued`/`v3`, `-07` `PendingReview`/`v1`, `-B1` `PendingReview`/`v3`. **Nol baris evidence menempel pada kantong `BE-BD-006`.** Komponen `TBD006-PRC` tetap validity `NULL` dan kedua lokasi `TBD006-LOC1`/`LOC2` tetap aktif — tidak ada master `BE-BD-006` yang diubah untuk memaksa satu skenario pun lolos.

---

## 10. Delta kontrak

| Butir | Tindakan |
| --- | --- |
| `api-contract.md` baris `POST /{id}/issue` tidak mencantumkan `VAL-BD-020b` padahal `validation-matrix.md` §3 mendefinisikannya dan runtime memang mengeluarkannya | **Diperbaiki** — daftar penolakan kini `VAL-BD-017/018/019/020/020b/065/079` |
| Slot `errors` pada `ApiResponse<T>` sebelumnya tidak pernah dipakai modul Bank Darah | **Diisi** mengikuti pola `BillingFolioController`; envelope tidak berubah |
| `AccessPermissionAttribute` / `AccessPermissionFilter` belum bisa membawa kode penolakan per endpoint | **Diperluas secara aditif dan opt-in**; perilaku generic seluruh pemakai lama tidak berubah (bukti bagian 6.1) |

---

## 11. Sisa pekerjaan yang bukan milik task ini

- **Cleanup fixture `TEST-BD007`** menunggu persetujuan pemilik.
- **Commit** belum dilakukan; seluruh perubahan masih di working tree atas instruksi pemilik.
- `BE-BD-008`, `BE-BD-009`, `BE-BD-010` **tidak** otomatis selesai. Penahan `BE-BD-007` gugur, sehingga ketiganya kini siap dijadwalkan — statusnya tetap milik roadmap, bukan ditetapkan laporan ini.
