# Manajemen Vendor — Matriks Hak Akses dan Audit

| Field | Nilai |
| --- | --- |
| Kontrak | `PBJ-VD-PERM` · `last_changed_in: 1.1` (`PBJ-DEC-065`, `PBJ-DEC-064`, 2026-11-19) · status `approved` (`PBJ-DEC-062`) |
| Masukan | `PBJ-DEC-046`, `047`, `049`, `051`, `060`, `064`, `065`; `PBJ-ASM-003`; `PBJ-PP-API` 1.4 |

## 1. Cara kerja hak akses

Sama dengan sub-modul `perencanaan-permintaan`: setiap action punya `[AccessAction]` dan `[AccessPermission(Resource,
Action)]`; argumen ke-1 = `ControllerName` pada `[AccessController]`. Peran diatur admin di Pengaturan → Manajemen Role
→ Akses Role. Tidak ada `IsInRole` atau nama peran di kode. Data vendor **tidak** dibatasi per unit.

## 2. Daftar resource dan action

| Resource | Action | Endpoint |
| --- | --- | --- |
| `ProcurementVendor` | `Read` | `GET /vendors/*`, `GET /vendors/eligibility` |
| `ProcurementVendor` | `Update` | `PUT /vendors/{id}/categories` |
| `ProcurementVendor` | `Restrict` | `POST /vendors/{id}/restriction`, `/restriction/lift` |
| `ProcurementVendorDocument` | `Read`, `Create`, `Verify` | Daftar/detail; catat dan batal; verifikasi dan tolak |
| `ProcurementVendorStatusRequest` | `Read`, `Create`, `Cancel` | Daftar/detail/approval; buat, ubah, ajukan; batal/tarik |
| `ProcurementVendorEvaluation` | `Read`, `Create`, `Update`, `Finalize`, `Revise` | Sesuai kontrak API |
| `ProcurementVendorDocumentType`, `ProcurementCategoryDocumentRequirement`, `ProcurementEvaluationCriterion` | `Read`, `Create`, `Update`, `Delete` | Sembilan endpoint master |
| `ProcurementAttachment` (sudah ada) | `Read`, `Upload` | Berkas dokumen legalitas, lampiran usulan status (`PBJ_VENDOR_STATUS`), lampiran penilaian (`PBJ_VENDOR_EVALUATION`) (1.1) |
| `ApprovalInbox` (sudah ada) | `Read`, `Approve`, `Reject`, `Return`, `RequestRevision` | Keputusan usulan blokir; "Minta revisi" di Antrean Persetujuan Pengadaan memakai `RequestRevision` (1.1, `PBJ-DEC-065`) |

## 3. Peta peran ke izin (bundel awal `PBJ-DEC-051`)

| Peran | Izin |
| --- | --- |
| Tim Pengadaan | `ProcurementVendor : Read, Update`; `ProcurementVendorDocument : Read, Create, Verify`; `ProcurementVendorStatusRequest : Read`; `ProcurementVendorEvaluation : Read, Create, Update, Revise`; tiga master vendor `Read, Create, Update, Delete`; `ProcurementAttachment : Read, Upload`; `ProcurementItemCategory : Read` |
| Manajer Pengadaan | Semua izin Tim Pengadaan + `ProcurementVendor : Restrict` (`PBJ-DEC-047`); `ProcurementVendorStatusRequest : Create, Cancel` (`PBJ-ASM-003`); `ProcurementVendorEvaluation : Finalize` (`PBJ-DEC-049`) |
| Direktur (penyetuju) | `ProcurementVendor : Read`; `ProcurementVendorDocument : Read`; `ProcurementVendorStatusRequest : Read`; `ProcurementVendorEvaluation : Read`; `ApprovalInbox : Read, Approve, Reject, Return`, **`RequestRevision`** (1.1); **`ProcurementAttachment : Read`** (1.1) — supaya penyetuju bisa membuka lampiran bukti usulan di layar `-07` lalu menyetujui, menolak, atau meminta revisi (`PBJ-DEC-065`) |
| Unit Peminta, Keuangan | Tidak ada izin vendor Pengadaan (`PBJ-DEC-051`) |

## 4. Kewenangan yang tidak dapat dijaga mesin izin

| Aturan | Penjaga |
| --- | --- |
| Pemeriksa dokumen ≠ pengunggah | Service: `VerifiedByUserId ≠ CreateBy` → 403 `PBJ-VAL-103` |
| Pemfinal penilaian ≠ penilai | Service: pemfinal ≠ `EvaluatorUserId` (pembuat draf, atau pembuat revisi untuk penilaian revisi; 1.1 `VD-008 D-3`) → 403 `PBJ-VAL-111` |
| Batal dokumen hanya oleh pengunggah | Service: pelaku = `CreateBy` dokumen, selain itu 403 tanpa kode (1.1, `VD-004 D-3`) |
| Ubah, ajukan, tarik usulan hanya oleh pengusul | Service: pelaku = `RequesterUserId`, selain itu 403 tanpa kode; batal draf boleh pemegang `Cancel` mana pun (1.1, `VD-007 D-2`) |
| Lampiran usulan dan penilaian | Penyedia akses `PBJ_VENDOR_STATUS` dan `PBJ_VENDOR_EVALUATION`: baca butuh izin `Read` dokumen induk; tambah hanya saat dokumen masih bisa diubah (1.1, `VD-007 D-6`, `VD-008 D-6`) |
| Pengusul blokir tidak menyetujui sendiri | `Wfl` `AllowSelfApproval = false` + pemeriksaan ulang penerap |
| Master Data tidak mengubah blokir | `SupplierController` mengabaikan field blokir (`PBJ-DEC-060`) |

## 5. Audit

| Kejadian | Dicatat ke | Isi |
| --- | --- | --- |
| Profil dibuat | Riwayat `PBJ_VENDOR` | Status blokir awal dari Master Data (`PBJ-DEC-060`) |
| Kategori ditambah/dilepas | Riwayat `PBJ_VENDOR` | Kategori, pelaku, waktu |
| Dokumen dicatat, diverifikasi, ditolak, dibatalkan | Riwayat dokumen + `PBJ_VENDOR` | Status, pelaku, alasan |
| Dibatasi/dicabut | Riwayat `PBJ_VENDOR` | Alasan, tanggal berakhir |
| Usulan diajukan/diputuskan | Riwayat usulan + `PBJ_VENDOR` | Nomor, keputusan, penyetuju (dari `Wfl`). **(1.1, `VD-007 D-7`)** Riwayat vendor mencatat pengajuan (`STATUS_REQUEST_SUBMIT`) dan keputusan (`VENDOR_BLOCK`, `VENDOR_UNBLOCK`, `STATUS_REQUEST_REJECT`); dikembalikan dan ditarik hanya di riwayat usulan |
| Penilaian final/digantikan | Riwayat penilaian + `PBJ_VENDOR` | Nilai akhir, rekomendasi, pemfinal |

Riwayat hanya ditambah, tidak diubah.

## 6. Kolom sensitif dan masa simpan

| Data | Perlakuan |
| --- | --- |
| `MstSupplier.BankAccountNumber` | **Sensitif.** Hanya `****` + 4 digit terakhir keluar dari endpoint Pengadaan; tidak dicatat logger |
| Berkas dokumen legalitas | Diunduh lewat endpoint lampiran yang memeriksa hak atas dokumen induk |
| Alasan blokir, alasan tolak, catatan penilaian | Tidak masuk logger; tampil hanya bagi pemegang izin baca |
| Masa simpan | Mengikuti `PBJ-OQ-015`; tidak ada penghapusan otomatis |
