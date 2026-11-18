# Register Aset — Matriks Hak Akses dan Audit

| Field | Value |
|---|---|
| `last_changed_in` | `AST-REG-1.0.0` — `draft` |
| Owner | Rizki · `approved_by`: — · `approved_at`: — |
| Traceability | `AST-DEC-011`, `017`, `025`, `027`, `029`, `030`, `043`, `044`, `050`; `AST-DES-016` |

Pemetaan endpoint → hak akses hanya ada di kolom `Hak akses` pada [`api-contract.md`](api-contract.md).
String atribut diturunkan: `[AccessPermission("<Resource>", "<Action>")]`. Pencatatan logger mengikuti
konvensi: `GET` tidak dicatat, selain `GET` dicatat.

**Pengecualian bernama:** `POST /register/assets/labels` dan `POST /register/reports/*/export` hanya
membaca, sehingga **tidak** dicatat logger walaupun bermetode `POST`/`GET` ekspor.

## 1. Cara kerja hak akses di repository ini

| Unsur | Rujukan source |
|---|---|
| Penanda controller | `Attributes/AccessControllerAttribute.cs` |
| Penanda endpoint | `Attributes/AccessActionAttribute.cs` + `Attributes/AccessPermissionAttribute.cs` (keduanya wajib; tanpa `AccessPermission` gagal verifier CI) |
| Hak penanda tanpa endpoint | `Attributes/AccessExplicitPermissionAttribute.cs` — dipakai untuk `AssetRegister : ReadAllUnits` |
| Pemeriksaan di service | `HasAccessAsync(User, Resource, Action)` (pola `PayrollCompensationsController` baris 42) |
| Akses role | `Constants/AccessTypes.cs`; perhatian: pengaturan Akses Role menimpa seluruh policy role |

## 2. Peta peran ke butir hak akses

`R` = Read. Kolom kosong = tidak diberi.

| Resource : Action | Petugas Aset | Kepala Bagian Aset | Kepala Unit | Penanggung Jawab | Kepala Akuntansi | Direktur | Auditor |
|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| `AssetCategory`, `AssetClassification`, `AssetGroup`, `AssetOwnershipType`, `AssetDocumentType`, `AssetLocation` : R | ✓ | ✓ | ✓ | | ✓ | ✓ | ✓ |
| … : `Create`, `Update` | | ✓ | | | | | |
| `AssetSetting` : R | ✓ | ✓ | | | ✓ | | ✓ |
| `AssetSetting` : `Update` | | ✓ | | | | | |
| `AssetRegister` : R | ✓ | ✓ | ✓ (unit sendiri) | ✓ (unit sendiri) | ✓ | ✓ | ✓ |
| `AssetRegister` : `ReadAllUnits` (penanda) | ✓ | ✓ | | | ✓ | ✓ | ✓ |
| `AssetRegister` : `Create`, `Update`, `Submit` | ✓ | ✓ | | | | | |
| `AssetRegister` : `Approve` | | ✓ | | | | | |
| `AssetRegister` : `ViewAmount` | ✓ | ✓ | | | ✓ | ✓ | |
| `AssetPlacement` : R | ✓ | ✓ | ✓ | ✓ | | ✓ | ✓ |
| `AssetPlacement` : `Create`, `Cancel` | ✓ | ✓ | | | | | |
| `AssetPlacement` : `Confirm` | | | ✓ | ✓ | | | |
| `AssetCondition` : R | ✓ | ✓ | ✓ | ✓ | | ✓ | ✓ |
| `AssetCondition` : `Report` | ✓ | ✓ | ✓ | ✓ | | | |
| `AssetCondition` : `Decide` | ✓ | ✓ | | | | | |
| `AssetCondition` : `ApproveLoss` | | ✓ | | | | | |
| `AssetDocument` : R | ✓ | ✓ | | | ✓ | ✓ | ✓ |
| `AssetDocument` : `Create`, `Update`, `Delete` | ✓ | ✓ | | | | | |
| `AssetUtilization` : R | ✓ | ✓ | | | ✓ | ✓ | ✓ |
| `AssetUtilization` : `Create`, `Submit` | ✓ | ✓ | | | | | |
| `AssetUtilization` : `Approve`, `End` | | ✓ | | | | | |
| `AssetUtilization` : `ApproveDirector` | | | | | | ✓ | |
| `AssetImport` : R | ✓ | ✓ | | | ✓ | | ✓ |
| `AssetImport` : `Upload` | ✓ | | | | | | |
| `AssetImport` : `ApproveAsset` | | ✓ | | | | | |
| `AssetImport` : `ApproveValue` | | | | | ✓ | | |
| `AssetDashboard` : R | ✓ | ✓ | ✓ (unit sendiri) | | ✓ | ✓ | ✓ |
| `AssetReport` : R | ✓ | ✓ | ✓ (unit sendiri) | | ✓ | ✓ | ✓ |

Auditor tidak diberi `ViewAmount` karena keputusan hanya menyebut Bagian Aset, Akuntansi, dan
Direktur (`AST-DEC-025`). Bila auditor butuh rupiah, itu keputusan baru.

## 3. Kewenangan yang tidak dapat dijaga mesin hak akses

Mesin hak akses hanya menjawab "boleh memanggil endpoint ini". Aturan berikut dijaga di service:

| Aturan | Penjaga | Yang **tidak** dijaganya | Risiko |
|---|---|---|---|
| Kepala Unit hanya bertindak atas unitnya | `AssetActorResolver` dari penugasan manajerial aktif | Penugasan HR yang salah atau terlambat diperbarui | Kepala Unit lama masih bisa mengonfirmasi sampai HR menutup penugasannya (`AST-UNK-004`) |
| Penerima penyerahan yang dituju | `AssetPlacementService` membandingkan pengguna dengan Kepala Unit tujuan / pegawai penanggung jawab | Pengguna yang meminjamkan akunnya | Pemakaian akun bersama |
| Pengaju tidak menyetujui sendiri | Pemeriksaan `CreateBy`/`RequestedBy` ≠ pengguna pada setiap persetujuan | Dua akun milik orang yang sama | Diterima sebagai risiko organisasi |
| Penyetuju pemanfaatan sesuai batas | `AssetUtilizationService` menghitung tingkat penyetuju saat diajukan dan mengunci nilainya | Perubahan batas sesudah diajukan (sengaja: batas yang dipakai = batas saat diajukan) | — |
| Rupiah tidak terkirim ke peran tanpa `ViewAmount` | Rupiah hanya ada pada `/amounts` dan endpoint `nilai` | Ekspor laporan register (sengaja tanpa kolom rupiah) | — |

## 4. Audit

| Lapisan | Isi |
|---|---|
| `IdentityModel` | Pembuat, pengubah, penghapus, pembatal beserta waktunya pada setiap tabel |
| Kolom langkah dokumen | Pengaju, penyetuju, penolak, alasan, waktu — pada aset, penyerahan, laporan kondisi, pemanfaatan, batch impor |
| `AstAssetHistory` | Linimasa tahan lama per aset: pelaku, waktu, nilai sebelum-sesudah, dokumen sumber; tidak pernah diubah/dihapus |
| Logger | `EntityId`, controller, action, status hasil. **Tanpa** isi berkas, tanpa nilai rupiah, tanpa nomor identitas pegawai. Ingat: argumen `data` pada `LoggerService` tidak tertulis, sehingga jejak yang dibutuhkan ditulis di `message` |

Kejadian yang wajib meninggalkan jejak di `AstAssetHistory`: didaftarkan, diimpor, ditempatkan, pindah
dalam unit, kondisi berubah, dimanfaatkan, pemanfaatan berakhir (dan kejadian sub-modul lain yang
memanggil `AssetHistoryService`).

## 5. Kolom sensitif dan masa simpan

| Kolom | Tabel | Perlakuan |
|---|---|---|
| `RegisteredAcquisitionCost` | `AstAsset` | Hanya `/amounts`; tidak masuk logger |
| `AgreementValue`, `PartnerName` | `AstUtilization` | Tidak masuk logger |
| `StoredPath` | `AstAttachment` | Tidak pernah dikirim ke frontend; berkas diambil lewat endpoint unduh |
| `RawJson` | `AstImportBatchRow` | Memuat harga dan nama pegawai; hanya peran `AssetImport : Read` |

Masa simpan: data aset dan linimasa disimpan selama aset ada ditambah masa simpan dokumen akuntansi
yang berlaku di RS. Angka masa simpan belum diputuskan dan **tidak** dikarang di sini; penghapusan
fisik data tidak dirancang (seluruh penghapusan memakai `IsDelete`).
