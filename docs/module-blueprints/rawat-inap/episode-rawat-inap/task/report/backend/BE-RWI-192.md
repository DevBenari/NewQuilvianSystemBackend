# Laporan Perubahan Backend — `BE-RWI-192`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-192` |
| Judul | Tabel dokumen admisi dan log cetak (`E10`) |
| Slice | Slice A; gelombang `RWA-MVP-0` |
| Roadmap | [`../../../roadmap/backend-roadmap-workspace-ppri.md`](../../../roadmap/backend-roadmap-workspace-ppri.md) — kartu `BE-RWI-192` |
| Trace | `FR-RWA-120` s.d. `128`; `RWI-DEC-228`, `229`, `263`; `INV-RWA-01`, `03`, `04`, `09`, `10`; backend 13.3, 13.9, 13.11, 13.12; data 20.2 s.d. 20.10, 20.13, 20.18 |
| Contract version | `0.11.0` **`approved`** (`RWI-DEC-265`) |
| Dependency | `BE-RWI-185` ✅ |
| Klasifikasi | `HEAVY` — sepuluh tabel, constraint penegak invariant, migration |
| Task mode | `BACKEND` |
| Target tulis | `InPatientManagement/{Enums,Models}`, `Repositories/Configurations/HealthServices/InPatientManagement/`, `ApplicationDbContext.cs`, migration |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `fdf85a07` (branch `MHamzah`), perubahan kerja belum di-commit |
| Tanggal | 8 Oktober 2026 |
| Status | ✅ Selesai. Build pengguna `PASS`; migration `20261008055604_AddWorkspacePpriAdmissionDocuments` diterapkan (`Done.`) dan ditinjau terhadap DDL 20.18 ([bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026)). Rollback **dikecualikan atas keputusan pengguna 8 Oktober 2026** |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement` |
| Prefix registry | `Inp` — `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-ENT-001` (`IdentityModel`), `QBE-NAM-001`, `QBE-NAM-002`, `QBE-NAM-004`, `QBE-CFG-001`, `QBE-MOD-001`, `QBE-MOD-002`, `QBE-DEL-001`, `QBE-TXN-001` |
| Wewenang | Source: ya. Migration dijalankan pengguna |

---

## 1. Kebutuhan

Dokumen admisi butuh tabel sendiri yang menegakkan invariant di basis data, bukan hanya di kode:
- satu dokumen aktif per jenis per episode (`INV-RWA-01`);
- satu akun satu slot petugas (`INV-RWA-04`);
- cetak ulang beralasan (`INV-RWA-09`);
- keadaan wajib per status (`INV-RWA-10`).

## 2. Proses bisnis

1. `InpAdmissionDocument` menyimpan satu baris per versi, dengan:
   - `Status` (Konsep, Menunggu tanda tangan, Lengkap, Digantikan, Dibatalkan);
   - `VersionNo`, `PreviousVersionId`;
   - salinan beku `SnapshotJson` (`jsonb`);
   - `RowVersion` untuk konkurensi;
   - `IdempotencyKey`.
2. Anak satu-ke-satu: pihak penanda tangan, permintaan privasi, pernyataan selisih biaya, pernyataan
   deposit. Anak satu-ke-banyak: tanda tangan, butir serah terima, baris privasi, butir keyakinan.
3. `InpAdmissionPrintLog` mencatat setiap cetak (jenis, status dokumen saat cetak, salinan 1–10, alasan
   cetak ulang, pencetak, waktu).
4. Tidak ada hapus permanen; seluruh FK `Restrict`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Pola `InpAdmissionReferral` (`Guid RowVersion`, `CHECK`, unique bersaring), `CliNursingIntervention` (`IdempotencyKey`),
`CliTransferHandover` (`jsonb`); kamus data 20.2–20.10, 20.13, DDL 20.18; backend 13.9.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `InPatientManagement/Enums/` — 12 berkas | Baru:<br>• `InpAdmissionDocumentType`, `InpAdmissionDocumentStatus`<br>• `InpAdmissionSignatureSlot`, `InpAdmissionSignatureMethod`<br>• `InpAdmissionPartySource`, `InpAdmissionPartyRelationship`, `InpAdmissionPartyIdentityType`<br>• `InpHandoverItemChoice`, `InpAdmissionPrivacyEntryType`, `InpCostDifferenceSubject`<br>• `InpAdmissionPrintKind`, `InpReprintReason` |
| `InPatientManagement/Models/` — 10 berkas | Baru: `InpAdmissionDocument`, `…Signature`, `…Party`, `InpAdmissionHandoverItem`, `InpAdmissionPrivacyRequest`, `…PrivacyEntry`, `InpAdmissionBeliefItem`, `InpAdmissionCostDifferenceStatement`, `InpAdmissionDepositStatement`, `InpAdmissionPrintLog` |
| `Repositories/Configurations/HealthServices/InPatientManagement/` — 10 berkas | Baru:<br>• enam `CHECK`;<br>• tujuh index unik bersaring (`UX_…`);<br>• index biasa per kamus data;<br>• empat relasi satu-ke-satu dengan `HasForeignKey<T>`;<br>• FK `Restrict`;<br>• `date`, `jsonb`, `numeric(18,2)` |
| `Repositories/ApplicationDbContext.cs` | Sepuluh `DbSet` jamak |
| `Migrations/20261008055604_AddWorkspacePpriAdmissionDocuments.cs` (+ `.Designer.cs`, snapshot) | Bagian `E10` (dibuat pengguna) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada (tabel dipakai `BE-RWI-193` s.d. `202`) |
| Database | 10 tabel baru, diterapkan pengguna 8 Oktober 2026. Rincian: 6 `CHECK`, 7 index unik bersaring, 4 index unik satu-ke-satu, 4 index unik komposit, 5 index biasa, 14 FK |
| Keamanan/Auth | Kolom bertanda **SENSITIF** di kamus data tidak pernah masuk logger (lihat `BE-RWI-193`) |

## 4. Dokumentasi endpoint

Tidak ada endpoint pada task ini.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight | Prefix `Inp`; `IdentityModel`; tidak ada `Trx*`/`SortOrder` baru | `PASS` | Review source |
| Tinjauan migration terhadap DDL 20.18 | Kolom, tipe, nullability, bawaan (`Status` 1, `VersionNo` 1, `SourceType` 4, `Copies` 1, `IsReprint`/`IsTransportPrivacyRequested` false), `CHECK`, filter index, FK `Restrict` sama | `PASS` | `Migrations/20261008055604_…cs` |
| `dotnet build` (pengguna) | 0 error | `PASS` | [Bukti bersama](BE-RWI-185.md#51-bukti-build-dan-migration-bersama-8-oktober-2026) |
| `dotnet ef database update` (pengguna) | `Done.` | `PASS` (maju) | Output pengguna |
| Rollback `Down()` | — | Dikecualikan atas keputusan pengguna 8 Oktober 2026 | `Down()` simetris menurut tinjauan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual/runtime: dikecualikan atas keputusan pengguna 8 Oktober 2026.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Kolom, tipe, nullability, bawaan, FK `Restrict`, `CHECK` sama dengan kamus data | Terpenuhi | Tinjauan migration; delta nama FK terpotong 63 karakter (batas PostgreSQL) |
| 2. `UX_InpAdmissionDocument_Episode_Type_Active`, `UX_…_PreviousVersionId`, `UX_…Signature_Document_Slot`, `UX_…_Document_Signer`, unique `IdempotencyKey` | Terpenuhi | `CreateIndex` dengan `filter` sesuai DDL |
| 3. `Down()` menghapus bersih | Terpenuhi (review) | `DropTable` sepuluh tabel; dijalankan: dikecualikan |
| 4. Tidak ada `Trx*` maupun `SortOrder` baru | Terpenuhi | Migration |
| DoD build | Terpenuhi | Build pengguna 0 error; migration diterapkan |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `E9` dan `E10` digabung dalam satu migration atas instruksi pengguna; roadmap merencanakan dua. Urutan `E9` → `E10` tetap di dalam `Up()` |
| Masalah yang diketahui | Anak berbaris (butir privasi/keyakinan/serah terima) pada dokumen `Draft` disinkronkan dengan menghapus fisik baris yang dibuang; dokumen terkunci tidak pernah diubah barisnya |
| Risiko tersisa | Rollback belum dijalankan |
| Perubahan sampingan | — |
| Interupsi | Sesi agent terputus saat build; build/migration dijalankan pengguna |
| Status Git | `??` 12 enum, 10 model, 10 configuration, migration; `M` `ApplicationDbContext.cs`, snapshot |
| Langkah berikutnya | — |
