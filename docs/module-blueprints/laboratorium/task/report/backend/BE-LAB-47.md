# Laporan Perubahan Backend — `BE-LAB-47`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-47` (backend) |
| Judul | Dua tabel Mikrobiologi beserta index parsialnya — gelombang `MVP-6c`, slice `S4b` |
| Trace | `FR-13.2`, `FR-13.3`; `LAB-DC-036`, `LAB-DC-037`; `INV-27`, `INV-30`, `INV-31`; **dan** `LAB-DEC-115`, `122`, `123`, `126` |
| Kontrak | `erd/laboratory-operations.md` amandemen 2026-09-21 (pertama dan kedua); `LAB-API-v1` `r27` bagian 22.2 |
| Klasifikasi | `MEDIUM` — dua tabel baru, nol endpoint |
| Model | Claude Opus 5 |
| Tanggal | 2026-09-21 |
| Status | ✅ **`SELESAI`** — dua tabel berdiri; **`AC-108`, `AC-109`, `AC-110` terbukti terhadap database sungguhan** |

---

## 1. Cakupan task ini BASI, dan bentuknya diperbarui

Roadmap 6i.4 menulis kontraknya sebagai `02-backend-architecture.md` bagian 14.7/14.8/14.11 —
rancangan `r24` dari 2026-09-18. Bagian itu **sudah tersusul** oleh empat keputusan yang
disetujui 2026-09-21:

| Keputusan | Yang ditambahkan |
|---|---|
| `LAB-DEC-115` | `ConcentrationUnitId` — MIC wajib membawa satuannya |
| `LAB-DEC-122` | `DiscContentUgSnapshot`, `BreakpointLowerMmSnapshot`, `BreakpointUpperMmSnapshot` |
| `LAB-DEC-123` | `ComputedResult`, `IsResultOverridden`, `ResultOverrideReason` |
| `LAB-DEC-126` | `IsSusceptibilityTested` pada isolat |

**Tujuh kolom lebih banyak daripada bentuk `r24`.** Kontrak yang lebih baru dan sudah disetujui
menang, sehingga tabel dibangun mengikuti ERD amandemen 2026-09-21 — bukan bagian 14.

---

## 2. Yang dibangun

| Berkas | Isi |
|---|---|
| `Models/LabMicrobiologyIsolate.cs` | 6 properti + 2 navigasi + koleksi |
| `Models/LabIsolateSusceptibility.cs` | 14 properti + 3 navigasi |
| `Repositories/Configurations/.../LabMicrobiologyIsolateConfiguration.cs` | FK, index, index unik parsial |
| `Repositories/Configurations/.../LabIsolateSusceptibilityConfiguration.cs` | FK, presisi, index unik parsial |
| `Repositories/ApplicationDbContext.cs` | 2 `DbSet` |
| `Migrations/20260921041046_AddLabMicrobiologyIsolateAndSusceptibility.cs` | Dibangkitkan |

**Nol endpoint**, sesuai cakupan task.

### Kenapa hampir seluruh kolom kepekaan nullable

Bukan karena longgar. **Dua metode uji tinggal pada tabel yang sama**, dan separuh kolomnya
memang tidak berlaku pada metode yang lain (`LAB-DEC-124`): difusi cakram memakai kandungan
cakram, rentang breakpoint, dan zona; dilusi memakai MIC beserta satuannya.

### Satu Cascade di antara empat Restrict

`LabIsolateSusceptibility` → `LabMicrobiologyIsolate` memakai **Cascade**, dan hanya itu.
Baris kepekaan nol punya umur sendiri — ia hasil pengujian **terhadap** isolat itu.

Empat FK lain **Restrict**: isolat ke pemeriksaan dan ke organisme, baris kepekaan ke
antibiotik dan ke satuan. Ketiganya data induk atau bukti kejadian; menghapusnya dari bawah
temuan pasien akan membuat temuan itu kehilangan artinya (`INV-31`).

---

## 3. Verifikasi — terhadap database sungguhan

Migration diterapkan ke `QuilvianNewDevYoga`. Pemeriksaan **terhadap definisi di database**,
bukan terhadap kode configuration-nya — sesuai yang diminta roadmap.

### `AC-108` — seluruh FK ke data induk ber-`Restrict`

| Constraint | `delete_rule` |
|---|---|
| `FK_LabMicrobiologyIsolate_LabExamination_LabExaminationId` | **RESTRICT** |
| `FK_LabMicrobiologyIsolate_LabOrganism_LabOrganismId` | **RESTRICT** |
| `FK_LabIsolateSusceptibility_LabAntibiotic_LabAntibioticId` | **RESTRICT** |
| `FK_LabIsolateSusceptibility_MstMeasurement_ConcentrationUnitId` | **RESTRICT** |
| `FK_LabIsolateSusceptibility_LabMicrobiologyIsolate_...` | **CASCADE** — disengaja |

✅ Terbukti.

### `AC-109` — index unik berbentuk PARSIAL

```sql
CREATE UNIQUE INDEX "IX_LabMicrobiologyIsolate_LabExaminationId_LabOrganismId"
  ON public."LabMicrobiologyIsolate" USING btree ("LabExaminationId", "LabOrganismId")
  WHERE ("IsDelete" = false);

CREATE UNIQUE INDEX "IX_LabIsolateSusceptibility_IsolateId_AntibioticId"
  ON public."LabIsolateSusceptibility" USING btree ("LabMicrobiologyIsolateId", "LabAntibioticId")
  WHERE ("IsDelete" = false);
```

✅ Terbukti. **Kedua-duanya membawa pembatas `IsDelete = false`.**

> **Ini butir yang paling mudah dilewatkan pada gelombang ini, dan akibatnya halus.** Tanpa
> pembatas itu, analis yang salah memilih antibiotik, menghapusnya, lalu memilih antibiotik
> yang benar akan **ditolak database tanpa sebab yang dapat ia pahami** — sebab baris yang
> sudah ditandai hapus masih menempati kuncinya.

### `AC-110` — snapshot nama wajib terisi

| Kolom | `is_nullable` | Panjang |
|---|---|---|
| `LabMicrobiologyIsolate.OrganismNameSnapshot` | **NO** | 200 |
| `LabIsolateSusceptibility.AntibioticNameSnapshot` | **NO** | 200 |

✅ Terbukti.

### Bentuk kolom lain yang diperiksa

`Concentration` `numeric(18,4)` — presisi eksplisit, mengikuti `ResultNumeric`.
`Result` `integer NOT NULL`; `ComputedResult` `integer` nullable — kosong ketika breakpoint
belum tersedia. `IsResultOverridden` dan `IsSusceptibilityTested` keduanya `boolean NOT NULL`.
Enam kolom metode-spesifik seluruhnya nullable.

---

## 4. Yang sengaja TIDAK dikerjakan

| Hal | Alasan |
|---|---|
| Endpoint pengisian isolat dan antibiogram | `BE-LAB-48` |
| Tabel breakpoint | `BE-LAB-60` |
| Penghitung interpretasi | `BE-LAB-61` |
| `git add`, `commit`, `push` | Nol diminta |
