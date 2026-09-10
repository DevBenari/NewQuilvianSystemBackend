# ERD — Radiology Safety Policy

| Field | Value |
|---|---|
| Contract version | `RAD-ERD-SAF-001` |
| Revision | `1` |
| Status | `draft` |
| Konteks | `BC-RAD-04` Safety Policy |
| Backend SHA | `64da911` |
| Decision | `RAD-DEC-005`, `RJ-BIL-DEC-014` |

Konteks ini menetapkan **kebijakan**, bukan mengerjakan pasien. Ia yang menentukan pemeriksaan
boleh berjalan atau tidak.

---

## Diagram

```mermaid
erDiagram
    MstRadModality {
        uuid Id PK
        varchar ModalityCode UK
        varchar ModalityName
        varchar Description
        boolean UsesIonisingRadiation
        boolean SupportsContrast
        boolean IsActive
        int SortOrder "pola lama, jangan ditiru"
    }
    MstRadSafetyRequirement {
        uuid Id PK
        varchar RequirementCode UK
        varchar RequirementName
        varchar Description
        varchar Category
        boolean RequiresNote
        varchar SourceNote
        boolean IsActive
        int SortOrder "pola lama, jangan ditiru"
    }
    MstRadModalitySafetyRule {
        uuid Id PK
        uuid ModalityId FK
        uuid ProcedureId FK "kosong berarti berlaku untuk semua pemeriksaan alat itu"
        uuid SafetyRequirementId FK
        boolean IsMandatory
        int RuleStatus "BARU — enum RadSafetyRuleStatus"
        int RuleVersion
        timestamp EffectiveFrom
        timestamp EffectiveTo
        uuid SubmittedByUserId "BARU"
        timestamp SubmittedAt "BARU"
        uuid ApprovedByUserId
        timestamp ApprovedAt
        uuid RejectedByUserId "BARU"
        timestamp RejectedAt "BARU"
        varchar RejectionReason "BARU"
        boolean IsActive "dipertahankan untuk kompatibilitas"
        varchar Note
    }
    MstProcedure {
        uuid Id PK
        varchar ProcedureName "milik MasterData"
    }
    MstRadModality ||--o{ MstRadModalitySafetyRule : "1:N — Sudah ada"
    MstRadSafetyRequirement ||--o{ MstRadModalitySafetyRule : "1:N — Sudah ada"
    MstProcedure |o--o{ MstRadModalitySafetyRule : "0:1 — Sudah ada"
```

---

## Tabel Status Entity

| Entity | Status | Owner | Catatan |
|---|---|---|---|
| `MstRadModality` | `Sudah ada` | Radiology Management | Tidak berubah; ditambah endpoint pengelolaan |
| `MstRadSafetyRequirement` | `Sudah ada` | Radiology Management | Tidak berubah; ditambah endpoint pengelolaan |
| `MstRadModalitySafetyRule` | **`Diperbarui`** | Radiology Management | Enam kolom siklus pengesahan ditambahkan |
| `MstProcedure` | `Sudah ada` | Health Services MasterData | Direferensikan, **MUST NOT** disalin |

---

## Index dan Batasan

| Tabel | Index | Sifat | Keadaan |
|---|---|---|---|
| `MstRadModalitySafetyRule` | `ModalityId` + `ProcedureId` + `SafetyRequirementId` | **Unik**, difilter | **Filter diubah** — lihat di bawah |
| `MstRadModalitySafetyRule` | `ModalityId` + `IsActive` | Biasa | Tidak berubah |

### Perubahan filter index unik

| Keadaan | Filter |
|---|---|
| Sekarang di `64da911` | `"IsDelete" = false AND "IsActive" = true` |
| Setelah migration 1 | `"IsDelete" = false AND "RuleStatus" = 3` |

Angka `3` adalah nilai `RadSafetyRuleStatus.Active`.

**Mengapa index ini penting.** Tanpa penjaga ini, dua baris yang saling bertentangan — satu
menyatakan butir wajib, satu menyatakan tidak wajib, untuk kombinasi alat dan pemeriksaan yang
sama — dapat hidup berdampingan. Yang menang tinggal soal urutan baris, dan itu berarti
keselamatan pasien ditentukan kebetulan.

---

## Arti Kolom `ProcedureId` yang Boleh Kosong

Ini bentuk yang mudah disalahpahami.

| `ProcedureId` | Artinya |
|---|---|
| Kosong | Aturan berlaku untuk **seluruh pemeriksaan** dengan alat itu |
| Terisi | Aturan berlaku **hanya** untuk pemeriksaan tersebut |

> **Contoh.** Skrining kehamilan wajib untuk **semua** pemeriksaan CT-Scan, maka `ProcedureId`
> dikosongkan. Sedangkan pemeriksaan fungsi ginjal hanya wajib untuk **CT-Scan dengan
> kontras**, maka `ProcedureId` diisi pemeriksaan itu saja. Pasien yang menjalani CT-Scan tanpa
> kontras tidak ditanyai fungsi ginjal, tetapi tetap ditanyai kehamilan.

---

## Siklus Hidup Aturan

Rinciannya ada di [contracts/state-transition-matrix.md](../contracts/state-transition-matrix.md).
Ringkasnya:

| `RuleStatus` | Ikut dinilai gerbang keselamatan? |
|---|:---:|
| `Draft` (1) | **Tidak** |
| `PendingApproval` (2) | **Tidak** |
| `Active` (3) | **Ya** |
| `Inactive` (4) | **Tidak** |

> **Akibat yang wajib disampaikan ke pengguna.** Selama sebuah alat belum punya satu pun aturan
> berstatus `Active`, seluruh pemeriksaan dengan alat itu **akan ditolak**. Ini disengaja
> (`RJ-BIL-DEC-014`), bukan kerusakan. Layar pengelolaan wajib menampilkan peringatan itu.
