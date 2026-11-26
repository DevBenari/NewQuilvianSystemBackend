# Laporan Perubahan Backend — `BE-PBJ-VD-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PBJ-VD-004` |
| Judul | Dokumen legalitas |
| Slice | `MVP-V1` — dokumen legalitas dan kelayakan (`04-prd-to-mvp.md` bagian 20) |
| Roadmap | `manajemen-vendor/roadmap/backend-roadmap.md` revisi 1, kartu `BE-PBJ-VD-004` |
| Trace | `PBJ-DEC-045`, `046`, `055`, `060`; `PBJ-ASM-001`, `002`; FR-VD-020, 021, 022; `PBJ-VAL-090`, `093`, `094`, `101`..`104`; `AT-VD-10`..`13`, `16` |
| Contract version | `PBJ-VD-API` 1.0 §Vendor Document, `PBJ-VD-STATE` 1.0 bagian 2, `PBJ-VD-VAL` 1.0, `PBJ-VD-PERM` 1.0 — semua `approved` (`PBJ-DEC-062`) |
| Dependency | `BE-PBJ-VD-002` ✅, `BE-PBJ-VD-003` ✅ |
| Klasifikasi | `MEDIUM` (logika 2, kontrak API 2, database 0, keamanan 2 — pemisahan pengunggah/pemeriksa, hak lampiran) |
| Task mode | `BACKEND` (pemilik: "lanjutkan", 17 November 2026) |
| Target tulis | `Vendor/Controllers`, `Vendor/Services`, `Vendor/DTOs`, konstanta aksi riwayat, extension DI |
| Model | Claude Opus 5.5 |
| Branch / commit | Backend `sukmagp` @ `a6df5619`, frontend `b1b360efa` (belum di-commit) |
| Tanggal | 17 November 2026 |
| Status | ✅ **SELESAI.** Sembilan endpoint dokumen dan penyedia akses lampiran berjalan; uji runtime 33/33 `PASS` (putaran kedua, sesudah dua harapan skrip dikoreksi); tanpa migration |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `Vendor` |
| Registry | `Corporate / Procurement · ProcurementManagement / Vendor · Prc · ACTIVE` (`PBJ-DEC-063`) |
| Keberlakuan | `NEW CODE` untuk controller, service, DTO, helper keadaan berlaku; `TOUCHED LEGACY` untuk `ProcurementDocumentHistoryActions.cs`, extension DI, dan `ProcurementVendorProfileService` (pembuatan profil dipisah ke `AddProfile`, perilaku sama) |
| Arketipe endpoint | Aggregate ber-lifecycle (`PENDING → VERIFIED/REJECTED/CANCELLED`) dengan aksi `POST /{id}/<aksi>`; tanpa `DELETE` dan tanpa `PATCH /status` |
| QBE yang berlaku | `QBE-MOD-001..003`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-TXN-001`, `QBE-LOG-001`, `QBE-AUD-001` |
| Hasil checker | QBE Strict `PASS` (8 berkas, 0 temuan) |

## 1. Masalah yang diselesaikan

Vendor belum bisa dinilai legalitasnya karena belum ada tempat mencatat izin dan memeriksanya. Task ini membuka
pencatatan dokumen legalitas (NIB, NPWP, Izin PBF, Izin Distribusi Alkes, dan lain-lain). Berkasnya diunggah lewat
layanan lampiran yang sudah ada, lalu petugas **lain** memverifikasi atau menolaknya. Keadaan berlaku setiap izin
(berlaku, segera kedaluwarsa, kedaluwarsa, digantikan) dihitung otomatis setiap kali dibaca.

## 2. Proses bisnis

| Langkah | Pelaku | Endpoint | Contoh |
| --- | --- | --- | --- |
| Mencatat izin | Tim Pengadaan (Rina) | `POST /vendor-documents` | Izin PBF PT Sehat Farma, terbit 01-07-2021, berlaku sampai 07-12-2026 → `PENDING` |
| Mengunggah berkas | Rina (pengunggah) | `POST /attachments` (`PBJ_VENDOR_DOCUMENT`) | `pbf.pdf` |
| Memeriksa dan memverifikasi | Tim Pengadaan lain (Budi) | `POST /vendor-documents/{id}/verify` | `VERIFIED`, pemeriksa Budi, jam tercatat |
| Menolak | Budi | `POST …/reject` | "Masa berlaku pada berkas berbeda dengan isian." |
| Membatalkan salah catat | Rina | `POST …/cancel` | "Salah catat jenis dokumen." |
| Memantau izin yang hampir habis | Tim Pengadaan | `GET /vendor-documents?validityState=EXPIRING` | Izin PBF tinggal 20 hari → Segera kedaluwarsa |

Dokumen pengganti dicatat sebagai baris baru. Contoh: NIB baru diverifikasi, maka NIB lama terbaca **Digantikan** dan
tidak lagi ikut dihitung kedaluwarsa. Setiap langkah tercatat di riwayat dokumen dan riwayat vendor.

**Jalur gagal:**

| Kejadian | Respons | Yang dilakukan petugas |
| --- | --- | --- |
| Izin PBF tanpa tanggal berlaku | 422 `PBJ-VAL-101` "Tanggal berlaku wajib diisi untuk Izin Pedagang Besar Farmasi." | Isi tanggal berlaku |
| Tanggal berlaku sebelum tanggal terbit | 422 `PBJ-VAL-101` | Perbaiki tanggal |
| Verifikasi sebelum berkas diunggah | 422 `PBJ-VAL-102` | Pengunggah mengunggah berkas dulu |
| Pengunggah memverifikasi atau menolak dokumennya sendiri | 403 `PBJ-VAL-103` "Dokumen ini harus diperiksa petugas lain." | Minta rekan memeriksa |
| Tolak/batal tanpa alasan ≥ 10 karakter | 422 `PBJ-VAL-104` | Tulis alasan |
| Dua pemeriksa memutus dokumen yang sama | Yang kedua 409 `PBJ-VAL-090` | Muat ulang |
| Memutus dokumen yang sudah final | 409 `PBJ-VAL-093` | — |
| Menambah berkas ke dokumen yang sudah final, atau oleh bukan pengunggah | 409 "Dokumen sudah final; lampiran tidak dapat diubah." | — |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`IProcurementDocumentAccessProvider` dan penyedia PR, `ProcurementAttachmentService`, `PrcAttachment`,
`AccessPermissionService`, `ProcurementDataScopeService.GetUserId`, `ProcurementBudgetService` (pola `Idempotency-Key`),
`PurchaseRequestsController` (pola 403), model vendor, kontrak §Vendor Document, matriks status bagian 2, dan matriks
validasi `PBJ-VAL-101`..`104`.

### 3.2 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Areas/Corporate/ProcurementManagement/Vendor/Controllers/VendorDocumentsController.cs` | Baru | 9 endpoint, resource `ProcurementVendorDocument` (`Read`, `Create`, `Verify`) |
| `.../Vendor/Services/ProcurementVendorDocumentService.cs` | Baru | Catat, verifikasi, tolak, batal, daftar, ringkasan, riwayat; `VendorDocumentAttachmentAccessProvider` |
| `.../Vendor/Services/ProcurementVendorDocumentValidity.cs` | Baru | Keadaan berlaku `VALID`/`EXPIRING`/`EXPIRED`/`REPLACED`, batas dari `Procurement:Vendor:ExpiryWarningDays` |
| `.../Vendor/DTOs/VendorDocumentDtos.cs` | Baru | Query, permintaan catat/verifikasi/tolak/batal, respons daftar/detail/ringkasan/metadata |
| `.../Vendor/Services/ProcurementVendorExceptions.cs` | Diperbarui | `ProcurementVendorForbiddenException` (403 berkode) |
| `.../Vendor/Services/ProcurementVendorProfileService.cs` | Diperbarui | `AddProfile` publik (dipakai saat dokumen pertama dicatat); batas peringatan lewat helper bersama |
| `.../Common/Constants/ProcurementDocumentHistoryActions.cs` | Diperbarui | `VERIFY`, `DOCUMENT_ADD`, `DOCUMENT_VERIFY`, `DOCUMENT_REJECT`, `DOCUMENT_CANCEL` |
| `.../ProcurementManagementServiceCollectionExtensions.cs` | Diperbarui | Service dokumen + penyedia akses `PBJ_VENDOR_DOCUMENT` |

### 3.3 Dampak kontrak API, database, dan keamanan

**Database.** Tidak ada perubahan skema. Yang ditulis:
- `PrcVendorDocument`;
- `PrcVendorProfile`, bila vendor belum punya profil;
- `PrcDocumentHistory`, untuk dua jenis riwayat;
- `PrcAttachment`, lewat layanan lampiran.

**Keamanan.**
- Pemeriksa ≠ pengunggah dijaga di service (403 `PBJ-VAL-103`), di luar izin Akses Role.
- Hak lampiran `PBJ_VENDOR_DOCUMENT`:
  - tanpa `ProcurementVendorDocument : Read` → tidak terlihat;
  - pengunggah selama `PENDING` → boleh tambah atau tarik berkas;
  - selain itu → hanya baca.
- Alasan tolak/batal hanya disimpan di dokumen dan riwayat. Log audit hanya memuat id dan status.

**Selisih terhadap kontrak (delta), dicatat apa adanya.**

| # | Kontrak / desain | Yang dibangun | Alasan |
| ---: | --- | --- | --- |
| D-1 | Keadaan berlaku `VALID`/`EXPIRING`/`EXPIRED` | Ditambah `REPLACED` (Digantikan) untuk dokumen terverifikasi yang sudah ada penggantinya | Dokumen pengganti = baris baru (STATE bagian 2); tanpa nilai ini izin lama yang sudah diganti tetap terbaca kedaluwarsa |
| D-2 | `VerifyVendorDocumentRequest`/`Reject` membawa `expectedRowVersion` | Wajib di verifikasi dan tolak. Di **batal**, `expectedRowVersion` opsional | Kontrak batal hanya `{ reason }` |
| D-3 | Batal "oleh pengunggah" | Hak akses `ProcurementVendorDocument : Create` (kontrak) **dan** pelaku = pengunggah (403 tanpa kode bila bukan) | Kontrak tidak memberi kode untuk batal oleh orang lain |
| D-4 | — | Catat dokumen untuk vendor tanpa profil membuat profil sekaligus, beserta riwayat status blokir awal | Sama dengan atur kategori (`BE-PBJ-VD-003`, `PBJ-DEC-060`) |
| D-5 | — | Jenis dokumen nonaktif dan vendor tak dikenal pada catat → 422 tanpa kode | STATE: "Jenis aktif" sebagai syarat; matriks validasi tidak memberi kode |
| D-6 | — | Penolakan mencatat pemeriksa dan waktunya di `VerifiedByUserId`/`VerifiedAt`; pembatalan juga mengisi `IsCancel`/`CancelBy`/`CancelDateTime` | Kolom pemeriksa = orang yang memutus |
| D-7 | — | Ringkasan bisa disaring `supplierId`; daftar memuat `daysUntilExpiry` dan `fileCount` | Kolom layar `-04` dan tab Dokumen `-02` |
| D-8 | — | Daftar dihitung di memori setelah saringan dasar, seperti daftar vendor | Keadaan berlaku bergantung tanggal hari ini dan dokumen pengganti |

## 4. Dokumentasi endpoint

#### Corporate / Procurement Management / Vendor / Vendor Document

Base URL: `api/v1/corporate/procurement-management/vendor-documents`. Semua respons dibungkus `ApiResponse<T>`.

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Periode, urutan, jenis dokumen, 4 status verifikasi, 4 keadaan berlaku, batas peringatan | `ProcurementVendorDocument : Read` | — | `VendorDocumentFilterMetadataResponse` |
| `GET` | `/summary` | Menunggu, terverifikasi, ditolak, kedaluwarsa, segera kedaluwarsa | `… : Read` | `supplierId?` | `VendorDocumentSummaryCountsResponse` |
| `GET` | `/` | Daftar dokumen | `… : Read` | `startDate`, `endDate`, `customPeriod`, `search`, `supplierId`, `documentTypeId`, `verificationStatus`, `validityState`, `sortBy` (`createDateTime`/`expiryDate`/`supplier`/`documentType`/`verificationStatus`), `sortDirection`, `pageNumber`, `pageSize` | `PagedResult<VendorDocumentListItemResponse>` |
| `GET` | `/{id}` | Detail beserta berkas terkini | `… : Read` | — | `VendorDocumentResponse` |
| `POST` | `/` | Catat dokumen (`PENDING`) | `… : Create` | `{ supplierId, documentTypeId, documentNumber?, issuedDate?, expiryDate? }` | `VendorDocumentResponse` (**201**) |
| `POST` | `/{id}/verify` | Verifikasi | `… : Verify` | `{ expectedRowVersion }` + `Idempotency-Key` | `VendorDocumentResponse` |
| `POST` | `/{id}/reject` | Tolak | `… : Verify` | `{ expectedRowVersion, reason }` | `VendorDocumentResponse` |
| `POST` | `/{id}/cancel` | Batalkan dokumen `PENDING` (pengunggah) | `… : Create` | `{ reason, expectedRowVersion? }` | `VendorDocumentResponse` |
| `GET` | `/{id}/history` | Riwayat dokumen | `… : Read` | — | `List<DocumentHistoryResponse>` |

Contoh respons `GET /{id}` (Izin PBF yang tinggal 20 hari):

```json
{ "documentTypeCode": "IZIN_PBF", "documentNumber": "PBF/123/2021", "issuedDate": "2021-07-01", "expiryDate": "2026-12-07",
  "verificationStatus": "VERIFIED", "verificationStatusLabel": "Terverifikasi", "validityState": "EXPIRING",
  "validityStateLabel": "Segera kedaluwarsa", "daysUntilExpiry": 20, "uploadedByName": "Rina", "verifiedByName": "Budi",
  "files": [ { "originalFileName": "pbf.pdf", "contentType": "application/pdf", "versionNo": 1 } ] }
```

Kode status: 200/201; 400 (validasi model); 401; 403 (izin, atau `PBJ-VAL-103`); 404; 409 (`PBJ-VAL-090`, `093`); 422
(`PBJ-VAL-094`, `101`, `102`, `104`, saringan tidak dikenal).

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -c Release` dan `--no-incremental` | 0 error; 0 warning dari berkas baru atau berubah | `PASS` | Keluaran build |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 8 berkas `.cs` | VIOLATION 0, REVIEW 0, INFO 0 | `PASS` | Laporan QBE |
| `dotnet test` | Tidak ada project test (kebijakan repository) | `NOT RUN` | — |
| Migration | Tidak ada perubahan skema | `NOT APPLICABLE` | — |
| Uji runtime HTTP ke build baru di `https://localhost:7185` terhadap `QuilvianNewDevSukma` | Putaran 1: 31/33. Putaran 2: **33/33 `PASS`** | `PASS` | `vd004.results.run1.json`, `vd004.results.json` scratchpad sesi + query DB |

**Kenapa dua putaran.** Dua kegagalan putaran 1 berasal dari skrip uji, bukan dari kode:

| ID | Putaran 1 | Sebab | Perbaikan skrip |
| --- | --- | --- | --- |
| `AT-VD-12` | 403 umum "Anda tidak memiliki akses…" | Aktor pengunggah di skrip tidak diberi `Verify`, sehingga tertolak di lapisan Akses Role sebelum aturan `PBJ-VAL-103`. Peta peran (PERM bagian 3) memberi Tim Pengadaan `Create` **dan** `Verify` | Aktor pengunggah diberi `Read, Create, Verify` |
| `AT-VD-13a` | Riwayat dokumen `CREATE, ATTACH, VERIFY` | Layanan lampiran juga mencatat `ATTACH`; harapan skrip hanya `CREATE, VERIFY`. Verifikasi idempoten tetap tercatat sekali | Periksa `VERIFY` muncul tepat sekali |

**Fixture uji putaran 2 (`TEST-PBJ VD004 20261117155515`, lewat API SuperAdmin).** Tiga akun `Employee` **tanpa
role**:
- `up` (pengunggah): `ProcurementVendorDocument : Read, Create, Verify`, `ProcurementAttachment : Read, Upload`, `ProcurementVendor : Read`.
- `ver` (pemeriksa): `ProcurementVendorDocument : Read, Verify`, `ProcurementAttachment : Read, Upload`, `ProcurementVendor : Read`.
- `none`: hanya `ProcurementVendor : Read`.

Ada juga satu supplier uji, satu jenis dokumen uji nonaktif, dan jenis seed `NIB`, `NPWP`, `IZIN_PBF`,
`IZIN_DISTRIBUSI_ALKES`.

| ID | Skenario | Hasil | Bukti |
| --- | --- | --- | --- |
| `AU-00` | Butir Akses Role | `PASS` | `Create`, `Read`, `Verify` |
| `AU-01` | Tanpa izin dokumen: daftar dan catat | `PASS` | 403, 403 |
| `AU-02` | Tanpa login | `PASS` | 401 |
| `D-00` | Metadata | `PASS` | Jenis seed, 4 status, 4 keadaan, 30 hari |
| `AT-VD-10` | Izin PBF tanpa tanggal berlaku; berlaku sebelum terbit | `PASS` | 422 `PBJ-VAL-101` dua kali, pesan sesuai matriks |
| `D-01` | Jenis nonaktif | `PASS` | 422 |
| `D-02` | Vendor tak dikenal | `PASS` | 422 |
| `D-03` | Catat NIB untuk vendor tanpa profil | `PASS` | 201 `PENDING`; profil 1 baris; riwayat vendor `CREATE`, `DOCUMENT_ADD` |
| `AT-VD-11` | Verifikasi tanpa berkas | `PASS` | 422 `PBJ-VAL-102` |
| `D-04` | Pengunggah mengunggah PDF | `PASS` | 200 |
| `D-05` | Pemeriksa menambah berkas ke dokumen orang lain | `PASS` | 409 (lampiran hanya baca baginya) |
| `AT-VD-12` | Pengunggah (punya `Verify`) memverifikasi dokumennya | `PASS` | 403 `PBJ-VAL-103` |
| `D-06` | `expectedRowVersion` basi | `PASS` | 409 `PBJ-VAL-090` |
| `AT-VD-13a` | Pemeriksa lain memverifikasi; ulang dengan kunci idempoten sama | `PASS` | `VERIFIED`, `VerifiedByUserId` = pemeriksa, `VerifiedAt` terisi; 200 kedua tanpa `VERIFY` ganda; keadaan `VALID` |
| `D-07` | Verifikasi ulang dokumen `VERIFIED` dengan kunci baru | `PASS` | 409 `PBJ-VAL-093` |
| `D-08` | Unggah berkas ke dokumen `VERIFIED` | `PASS` | 409 |
| `D-09` | Tolak dengan alasan "kurang" | `PASS` | 422 `PBJ-VAL-104` |
| `D-10` | Pengunggah menolak dokumennya | `PASS` | 403 `PBJ-VAL-103` |
| `AT-VD-13b` | Dokumen lain ditolak dengan alasan | `PASS` | `REJECTED`, alasan tersimpan; riwayat vendor + `DOCUMENT_VERIFY`, `DOCUMENT_REJECT` |
| `AT-VD-16` | Izin berlaku 20 hari lagi | `PASS` | `EXPIRING`, `daysUntilExpiry = 20`; ringkasan dokumen `expiringSoon = 1`; detail vendor `EXPIRING_SOON`, hitungan 1 |
| `D-11` | Izin berlaku sampai kemarin | `PASS` | `EXPIRED`; daftar vendor `documentState = EXPIRED` dan tersaring `documentState=EXPIRED` (bukti cabang `BE-PBJ-VD-003`) |
| `D-12` | NIB pengganti diverifikasi | `PASS` | NIB lama `REPLACED`, baru `VALID` |
| `D-13` | Pemeriksa (tanpa `Create`) membatalkan | `PASS` | 403 |
| `D-14` | Batal dengan alasan "salah" | `PASS` | 422 `PBJ-VAL-104` |
| `D-15` | Pengunggah membatalkan; batal ulang | `PASS` | `CANCELLED`, `IsCancel`, `CancelBy` = pengunggah; 409 `PBJ-VAL-093` |
| `D-16` | Saring keadaan Segera kedaluwarsa | `PASS` | Hanya izin PBF 20 hari |
| `D-17` | Saring Terverifikasi, urut berlaku sampai | `PASS` | Alkes (kemarin), PBF (20 hari), NIB, NIB |
| `D-18` | Keadaan berlaku tidak dikenal | `PASS` | 422 |
| `D-19` | Ringkasan vendor uji | `PASS` | Menunggu 0, terverifikasi 4, ditolak 1, kedaluwarsa 1, segera kedaluwarsa 1 |
| `D-20` | Detail memuat berkas terkini | `PASS` | `nib-2.pdf`, pengunggah "TEST-PBJ up VD004" |
| `D-21` | Pemeriksa membaca daftar lampiran | `PASS` | 200 |
| `D-22` | Dokumen tak dikenal | `PASS` | 404 detail dan riwayat |
| `D-23` | Riwayat vendor | `PASS` | Memuat `DOCUMENT_ADD`, `DOCUMENT_VERIFY`, `DOCUMENT_REJECT`, `DOCUMENT_CANCEL` |

33 skenario, 33 `PASS`.

**Bukti tambahan untuk `BE-PBJ-VD-003`.** `D-11` dan `AT-VD-16` membuktikan cabang keadaan dokumen yang sebelumnya
belum bisa diuji di daftar, ringkasan, dan detail vendor. Cabang hapus jenis dokumen yang dipakai dokumen
(`BE-PBJ-VD-002`) belum dijalankan ulang di task ini.

**Sesudah uji.** Supplier dan jenis dokumen uji kedua putaran dihapus lunak lewat API. Sisa fixture kedua putaran
masih ada di `QuilvianNewDevSukma`:
- profil, dokumen, dan lampiran vendor;
- unit, jabatan, akun, dan policy.

Pembersihan dijalankan dengan `cleanup_vd004.py` atas izin pemilik (17 November 2026, "oke lanjutkan" atas tawaran
pembersihan). Dry run lebih dulu, lalu apply dalam satu transaksi. Yang ditangani per putaran:
- dihapus lunak: 5 lampiran, 6 dokumen, 1 profil, 11–12 policy, 3 Employee, 3 jabatan, dan 1 unit (kode diberi akhiran
  `-DEL-<run>`);
- dinonaktifkan dan dikunci: 3 akun.

Sesudahnya profil, dokumen, dan lampiran `PBJ_VENDOR_DOCUMENT` yang aktif = 0. Riwayat tidak disentuh. Berkas PDF uji
tetap di penyimpanan lampiran.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-VD-10` dua kali 422 `PBJ-VAL-101` | Terpenuhi | `AT-VD-10` |
| `AT-VD-11` 422 `PBJ-VAL-102` | Terpenuhi | `AT-VD-11` |
| `AT-VD-12` 403 `PBJ-VAL-103` (bagian API; tombol tersembunyi adalah bagian `FE-PBJ-VD-003`) | Terpenuhi (API) | `AT-VD-12`, `D-10` |
| `AT-VD-13` `VERIFIED` dengan pemeriksa dan waktu; `REJECTED` dengan alasan; riwayat vendor bertambah | Terpenuhi | `AT-VD-13a`, `AT-VD-13b`, `D-23` |
| `AT-VD-16` Segera kedaluwarsa dan masuk hitungan ringkasan (bagian API) | Terpenuhi (API) | `AT-VD-16`, `D-19` |
| Penyedia akses lampiran `PBJ_VENDOR_DOCUMENT` | Terpenuhi | `D-04`, `D-05`, `D-08`, `D-21` |
| Laporan | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Batas peringatan 30 hari masih nilai awal (`PBJ-ASM-002`). Kelayakan vendor per kategori (`AT-VD-14`, `15`, `17`) milik `BE-PBJ-VD-005` |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Perhitungan daftar di memori (delta D-8). Berkas PDF uji (45 byte) tetap di penyimpanan lampiran sesudah pembersihan basis data |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Belum di-commit. Baru: `VendorDocumentsController.cs`, `ProcurementVendorDocumentService.cs`, `ProcurementVendorDocumentValidity.cs`, `VendorDocumentDtos.cs`, laporan ini. Diubah: `ProcurementVendorExceptions.cs`, `ProcurementVendorProfileService.cs`, `ProcurementDocumentHistoryActions.cs`, extension DI, roadmap, traceability, `MODULE-STATUS.md` |
| Langkah berikutnya | `BE-PBJ-VD-006` (Dibatasi) lalu `BE-PBJ-VD-005` (kelayakan, butuh `004` + `006`); paralel `BE-PBJ-VD-007`, `BE-PBJ-VD-008`. `FE-PBJ-VD-003` menunggu `VD-005` |
