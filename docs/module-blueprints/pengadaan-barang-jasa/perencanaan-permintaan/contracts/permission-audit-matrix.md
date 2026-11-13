# Perencanaan & Permintaan — Matriks Hak Akses dan Audit

| Field | Nilai |
| --- | --- |
| Kontrak | `PBJ-PP-PERM` · `last_changed_in: 1.2` (`PBJ-DEC-036`, `PBJ-DEC-037`, 2026-11-12) · status `approved` (`PBJ-DEC-034`) |
| Pemilik | Pengguna sesi · `approved_by: Pengguna sesi` · `approved_at: 2026-11-12` |
| Masukan | `PBJ-DEC-005`, `007`, `017`, `020`, `023`, `026`; `PBJ-PP-API` 1.0 |

Pemetaan endpoint ke izin hanya ada di [`api-contract.md`](api-contract.md) kolom **Hak akses**. String atribut
diturunkan dari kolom itu: `[AccessPermission("<Resource>", "<Action>")]`. Pencatatan logger mengikuti
konvensi (`GET` tidak dicatat, selain itu dicatat), tanpa pengecualian.

## 1. Cara kerja hak akses di repository ini

| Unsur | Sumber | Peran |
| --- | --- | --- |
| `[AccessController]` | `BE/Attributes/AccessControllerAttribute.cs` | Mendaftarkan controller ke layar Akses Role |
| `[AccessAction]` | `BE/Attributes/AccessActionAttribute.cs` | Nama tampil dan urutan aksi |
| `[AccessPermission]` | `BE/Attributes/AccessPermissionAttribute.cs`, `BE/Filters/AccessPermissionFilter.cs` | Penjaga runtime per pasangan resource + action |
| Seeder registry | `BE/Services/Security/PermissionRegistryDescriptor.cs` | Membuat baris izin dari `[AccessPermission]` apa adanya |
| Cakupan unit | `ProcurementDataScopeService` (baru) memakai `AspNetUserOrganization` | Penyaring data, **bukan** izin aksi |

## 2. Daftar resource dan action

| Resource | Action |
| --- | --- |
| `ProcurementRequest` | `Read`, `Create`, `Update`, `Submit`, `Cancel`; penanda `ReadAllUnits` |
| `ProcurementRequestVerification` | `Read`, `Verify`, `CancelApproved` |
| `ProcurementNeedPlan` | `Read`, `Create`, `Update`, `Submit`, `Revise`; penanda `ReadAllUnits` |
| `ProcurementPackagePlan` | `Read`, `Create`, `Update`, `Submit`, `Cancel`; penanda `ReadAllUnits` |
| `ProcurementBudget` | `Read`, `Create`, `Update`, `Submit`, `Revise`; penanda `ReadAllUnits` |
| `ProcurementItem` | `Read`, `Create`, `Update`, `Delete` |
| `ProcurementItemCategory` | `Read`, `Create`, `Update`, `Delete` (1.2) |
| `ProcurementFundSource` | `Read`, `Create`, `Update`, `Delete` (1.2) |
| `ProcurementType` | `Read` (1.2) |
| ~~`ProcurementMasterData`~~ | Dicabut pada 1.2 (`PBJ-DEC-037`) |
| `ProcurementAttachment` | `Read`, `Upload` |
| ~~`ProcurementCrossUnit`~~ | **Dicabut pada 1.1** (`PBJ-DEC-036`). Diganti penanda `ReadAllUnits` per resource dokumen, didaftarkan `[assembly: AccessExplicitPermission]` oleh task controller dokumen itu. `ProcurementDataScopeService` menanyakan `HasAccessAsync(user, <resource dokumen>, "ReadAllUnits")`; aksi yang belum terdaftar menghasilkan *tidak berhak* (fail-closed) |

## 3. Peta peran ke izin **[DIKONFIRMASI `PBJ-DEC-033` (`G-08`) — peran awal, dapat diatur di layar Akses Role]**

| Peran | Izin |
| --- | --- |
| Unit Peminta | `ProcurementRequest : Read, Create, Update, Submit, Cancel`; `ProcurementNeedPlan : Read, Create, Update, Submit, Revise`; `ProcurementItem : Read`; `ProcurementAttachment : Read, Upload` |
| Kepala Unit | Seperti Unit Peminta, ditambah `ApprovalInbox : Read, Approve, Reject, Return` (`Wfl`) |
| Tim Pengadaan | `ProcurementRequest : Read`; `ProcurementRequestVerification : Read, Verify, CancelApproved`; `ProcurementPackagePlan : *`; `ProcurementNeedPlan : Read, Revise`; `ProcurementItem`, `ProcurementItemCategory`, `ProcurementFundSource` : `Read, Create, Update, Delete`; `ProcurementType : Read`; `ProcurementAttachment : Read, Upload`; `<resource dokumen> : ReadAllUnits` |
| Manajer Pengadaan / Direktur (penyetuju) | `ApprovalInbox : Read, Approve, Reject, Return`; `ProcurementRequest : Read`; `ProcurementNeedPlan : Read`; `ProcurementPackagePlan : Read`; `ProcurementBudget : Read`; `<resource dokumen> : ReadAllUnits` |
| Keuangan / Anggaran | `ProcurementBudget : Read, Create, Update, Submit, Revise, ReadAllUnits`; `ProcurementItemCategory : Read`; `ProcurementFundSource : Read` |
| Auditor | Semua `Read` di atas + `<resource dokumen> : ReadAllUnits` |
| Administrator | Konfigurasi definisi dan matriks `Wfl` (izin `Wfl` existing) |

## 4. Kewenangan yang tidak dapat dijaga mesin izin

| Aturan | Penjaga | Yang **tidak** dijaganya | Risiko |
| --- | --- | --- | --- |
| Pengusul tidak menyetujui PR-nya sendiri (`PBJ-AC-002`) | `Wfl`: `AllowSelfApproval = false` + cek `RequestedByUserId`; diperiksa ulang `ProcurementWorkflowLifecycleHandler` | Pengusul yang memakai akun orang lain | Kebijakan akun dan sesi |
| Hanya data unit sendiri (`PBJ-DEC-020`) | `ProcurementDataScopeService` di setiap query | Data yang diekspor lalu dibagikan di luar sistem | Ekspor belum ada di MVP |
| Pengubah PR draft = pembuatnya | Service PR membandingkan `RequesterUserId` | Rekan satu unit yang ingin membantu | Disetujui bersama desain (`PBJ-DEC-034`): hanya pembuat; dapat dilonggarkan kelak |
| Verifikator bukan pengusul | Service verifikasi menolak bila `RequesterUserId = pengguna` | — | — |
| Status `APPROVED` hanya dari `Wfl` | Tidak ada endpoint Pengadaan yang menulisnya | Akses langsung ke basis data | Kendali basis data |

## 5. Audit

| Lapisan | Isi | Tahan lama |
| --- | --- | --- |
| `IdentityModel` | Pelaku dan waktu buat/ubah/hapus/batal pada setiap baris | Ya |
| `PrcDocumentHistory` | Setiap aksi: buat, ubah, ajukan, cek stok, verifikasi, kembalikan, tolak, batal, revisi, hasil approval; berisi status sebelum–sesudah, alasan, dan cuplikan nominal (`SnapshotJson`) | Ya, hanya ditambah |
| `Wfl` (`TrxWorkflowStatusHistory`, `TrxApprovalAction`) | Siapa menyetujui pada langkah apa, jenjang dan nominal saat itu (`PBJ-AC-003`) | Ya |
| Logger aplikasi | `EntityId`, controller, action, status | Mengikuti masa simpan log |

**Kejadian yang wajib meninggalkan jejak:** ajukan, selesaikan verifikasi (dengan nilai beli dan tanda pagu),
setiap perubahan cek stok, kembalikan, tolak, batal, revisi rencana/pagu, unggah dan tarik lampiran, serta
perubahan katalog.

## 6. Kolom sensitif dan masa simpan

| Kolom | Alasan | Perlakuan |
| --- | --- | --- |
| `PrcPurchaseRequest.Justification`, `UrgencyReason`, alasan pada riwayat | Bisa memuat keterangan internal | Tidak masuk logger |
| `PrcAttachment.OriginalFileName`, berkasnya | Isi dokumen | Tidak masuk logger; unduh hanya lewat endpoint terotorisasi |
| Nominal pagu dan PR | Rahasia bisnis | Tidak muncul di isi notifikasi (`G-11`) |

Masa simpan dokumen dan berkas: **belum ditetapkan** (`PBJ-OQ-015`). Sampai ditetapkan, tidak ada penghapusan
otomatis.
