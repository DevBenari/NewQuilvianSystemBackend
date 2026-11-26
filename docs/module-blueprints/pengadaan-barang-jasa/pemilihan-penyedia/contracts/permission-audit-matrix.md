# Pemilihan Penyedia — Matriks Hak Akses dan Audit

| Field | Nilai |
| --- | --- |
| Kontrak | `PBJ-PS-PERM` · `last_changed_in: 1.0` · status `approved` (`PBJ-DEC-095`) |
| Pemilik | Pengguna sesi · keamanan: **belum ditetapkan** (sama dengan sub-modul lain, K-3/K-5) · `approved_by: Pengguna sesi` · `approved_at: 2026-11-26` |
| Masukan | `PBJ-DEC-074`, `079`, `080`, `085`; `PBJ-DEC-007`, `036`, `037`, `040`; `PBJ-PS-API` 1.0 |

Pemetaan endpoint ke hak akses hanya ada di kolom `Hak akses` pada `api-contract.md`. String atribut diturunkan
`[AccessPermission("<Resource>", "<Action>")]`; pencatatan: `GET` tidak dicatat, selain `GET` dicatat. Tidak ada pengecualian.

## 1. Cara kerja hak akses

| Lapisan | Rujukan | Isi |
| --- | --- | --- |
| Atribut controller | `[AccessController(ProcurementPermissions.SourcingModuleCode, ..., ControllerName = <Resource>)]`, `[AccessAction]`, `[AccessPermission]` — pola `Request/Controllers/PurchaseRequestsController.cs` baris 30–32 | Izin endpoint; disemai otomatis oleh `PermissionRegistryDescriptor` |
| Penanda data | `[assembly: AccessExplicitPermission(...)]` di `Sourcing/ProcurementSourcingExplicitPermissions.cs` — pola `Request/ProcurementRequestExplicitPermissions.cs` | `ProcurementPackage : ReadAllUnits` dan `ProcurementPackage : ReadPrice` |
| Penyaring service | `ProcurementDataScopeService.ResolveAsync`, `_accessPermissionService.HasAccessAsync` | Cakupan unit, harga, evaluator |

## 2. Daftar resource dan action

| Resource | Action |
| --- | --- |
| `ProcurementPackage` | `Read`, `Create`, `Update`, `Submit`, `Revise`, `Cancel`, `RecordOrder`, `ReadSummary`; penanda `ReadAllUnits`, `ReadPrice` |
| `ProcurementRfq` | `Read`, `Create`, `Update`, `Send`, `Close` |
| `ProcurementQuotation` | `Read`, `Create`, `Update`, `Cancel`, `Negotiate` |
| `ProcurementTechnicalEvaluation` | `Read`, `Evaluate` |
| `ProcurementPackageAward` | `Read`, `Submit`, `Revoke` |
| `ProcurementMethod`, `ProcurementPackageDocumentType`, `ProcurementPackageDocumentRequirement` | `Read`, `Create`, `Update`, `Delete` |

## 3. Peta peran ke izin (bundel awal `PBJ-DEC-080`)

| Peran | Izin |
| --- | --- |
| Tim Pengadaan | `ProcurementPackage : Read, Create, Update, Submit, Revise, Cancel, RecordOrder, ReadAllUnits, ReadPrice`; `ProcurementRfq : Read, Create, Update, Send, Close`; `ProcurementQuotation : Read, Create, Update, Cancel, Negotiate`; `ProcurementPackageAward : Read, Submit`; tiga master `Read, Create, Update, Delete`; `ProcurementAttachment : Read, Upload`; `ProcurementItem : Read`; `ProcurementVendor : Read` |
| Manajer Pengadaan | Semua izin Tim Pengadaan + `ProcurementPackageAward : Revoke` (`PBJ-DEC-093`) |
| Evaluator teknis | `ProcurementTechnicalEvaluation : Read, Evaluate`; `ProcurementAttachment : Read`. **Tanpa** `ProcurementPackage : Read` dan **tanpa** `ReadPrice` |
| Penyetuju (Manajer Pengadaan, Direktur, sesuai matriks) | `ProcurementPackage : Read, ReadAllUnits, ReadPrice`; `ProcurementQuotation : Read`; `ProcurementPackageAward : Read`; `ProcurementAttachment : Read`; `ApprovalInbox : Read, Approve, Reject, Return, RequestRevision` (pola `PBJ-DEC-065`) |
| Keuangan | `ProcurementPackageAward : Read`; `ProcurementPackage : Read, ReadAllUnits`. **Tanpa** `ReadPrice` dan tanpa `ProcurementQuotation : Read`, sehingga melihat nilai penetapan di daftar penetapan tetapi bukan rincian penawaran |
| Unit Peminta | `ProcurementPackage : ReadSummary` saja |

**Catatan Keuangan:** nilai penetapan (`AwardTotal`) bukan rincian penawaran, sehingga ditampilkan bagi pemegang
`ProcurementPackageAward : Read` walaupun tanpa `ReadPrice`. HPS dan harga per vendor tetap tertutup.

## 4. Kewenangan yang tidak dapat dijaga mesin izin

| Penjaga aturan bisnis | Letak | Yang tidak dijaganya | Risiko |
| --- | --- | --- | --- |
| Pencatat ≠ evaluator penawaran (`PBJ-DEC-074`) | `ProcurementTechnicalEvaluationService` (403 `PBJ-VAL-135`) | Dua orang yang bersekongkol | Ditangani audit riwayat |
| Pengaju ≠ penyetuju (`PBJ-DEC-007`) | `Wfl` `AllowSelfApproval = false` + pemeriksaan ulang di penerap status | — | — |
| Evaluator hanya paket ditugaskan | `PrcPackageEvaluator` di service evaluasi dan `ProcurementSourcingAccessProvider` (404) | Evaluator yang juga diberi `ProcurementPackage : Read` oleh admin | Admin wajib mengikuti bundel; dicek saat UAT |
| Harga tertutup | Penyaring DTO + `ProcurementAttachmentService` (`PBJ-CAP-004`) | Berkas yang diunggah dengan kategori salah | Pengunggah memilih kategori dari daftar jenis dokumen; kategori bebas tidak diizinkan untuk `PBJ_PACKAGE`/`PBJ_RFQ` (`PBJ-DEC-095`) |
| Alasan bukan terendah tampil ke penyetuju (`PBJ-DEC-085`) | `RankingSnapshotJson` + linimasa persetujuan | — | — |

## 5. Audit

| Kejadian | Jejak tahan lama |
| --- | --- |
| Buat, ubah HPS/metode, ajukan/tarik/buka ulang persiapan, pecah, batal, catat PO | `PrcDocumentHistory` `PBJ_PACKAGE` dengan nilai lama → baru dan alasan |
| Kirim RFQ, perpanjang batas waktu (lama → baru, alasan), tutup, gagal | `PrcDocumentHistory` `PBJ_RFQ` |
| Catat, ubah, batalkan penawaran; negosiasi | `PrcDocumentHistory` `PBJ_RFQ` + baris `PrcNegotiation` |
| Hasil evaluasi | Baris `PrcTechnicalEvaluationResult` (evaluator, waktu, alasan) |
| Ajukan, tarik, putusan, tidak dapat diterapkan, cabut penetapan | `PrcDocumentHistory` `PBJ_PACKAGE_AWARD` + `TrxWorkflowStatusHistory` |
| Komitmen dicatat/dilepas | `PrcBudgetCommitment` (milik `perencanaan-permintaan`) |

## 6. Kolom sensitif dan masa simpan

Kolom bertanda `S` di `data/data-dictionary.md` (HPS, harga, total, ringkasan negosiasi, peringkat) **tidak** masuk payload
logger dan tidak dipakai sebagai contoh berisi data asli. Masa simpan mengikuti dokumen Pengadaan lain: tidak dihapus fisik
(`IsDelete`); retensi formal menunggu `PBJ-OQ-015`.
