# Laporan Perubahan Backend — `BE-PBJ-VD-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PBJ-VD-006` |
| Judul | Status Dibatasi |
| Slice | `MVP-V2` — status vendor (`04-prd-to-mvp.md` bagian 20) |
| Roadmap | `manajemen-vendor/roadmap/backend-roadmap.md` revisi 1, kartu `BE-PBJ-VD-006` |
| Trace | `PBJ-DEC-047`; FR-VD-030; `PBJ-VAL-090`, `093`, `104`, `105`; `AT-VD-20`, `21` |
| Contract version | `PBJ-VD-API` 1.0 §Vendor (restriction), `PBJ-VD-STATE` 1.0 bagian 1, `PBJ-VD-VAL` 1.0, `PBJ-VD-PERM` 1.0 — semua `approved` (`PBJ-DEC-062`) |
| Dependency | `BE-PBJ-VD-003` ✅ |
| Klasifikasi | `LOW` (logika 1, kontrak API 1, database 0, keamanan 1) |
| Task mode | `BACKEND` (pemilik: "oke lanjutkan", 17 November 2026) |
| Target tulis | `VendorsController`, `ProcurementVendorProfileService`, `VendorDtos`, konstanta aksi riwayat |
| Model | Claude Opus 5.5 |
| Branch / commit | Backend `sukmagp` @ `a6df5619`, frontend `b1b360efa` (belum di-commit) |
| Tanggal | 17 November 2026 |
| Status | ✅ **SELESAI.** Dua endpoint pembatasan berjalan; uji runtime 19/19 `PASS` dengan aktor non-SuperAdmin; tanpa migration |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `Vendor` |
| Registry | `Corporate / Procurement · ProcurementManagement / Vendor · Prc · ACTIVE` (`PBJ-DEC-063`) |
| Keberlakuan | `NEW CODE` (berkas vendor dari `VD-003`); `TOUCHED LEGACY` untuk `ProcurementDocumentHistoryActions.cs` |
| Arketipe endpoint | Aksi status pada aggregate vendor: `POST /{supplierId}/restriction`, `POST /{supplierId}/restriction/lift` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-TXN-001`, `QBE-LOG-001`, `QBE-AUD-001` |
| Hasil checker | QBE Strict `PASS` (4 berkas, 0 temuan) |

## 1. Masalah yang diselesaikan

Manajer Pengadaan perlu menahan vendor yang bermasalah tanpa memblokirnya. Contohnya vendor yang terlambat kirim tiga
kali: masih boleh dipilih, tetapi pemilih wajib memberi alasan (`PBJ-DEC-047`). Task ini membuka aksi **Batasi** dan
**Cabut**. Status Dibatasi berakhir sendiri saat tanggal berakhirnya lewat, tanpa job terjadwal.

## 2. Proses bisnis

| Langkah | Pelaku | Endpoint | Contoh |
| --- | --- | --- | --- |
| Membatasi vendor | Manajer Pengadaan (pemegang `ProcurementVendor : Restrict`) | `POST /vendors/{supplierId}/restriction` | "Terlambat kirim tiga kali berturut-turut", berakhir 30-06-2027 → Dibatasi |
| Membatasi sampai dicabut | Manajer Pengadaan | idem, tanpa `restrictedUntil` | "Kualitas kemasan buruk berulang" |
| Mencabut lebih awal | Manajer Pengadaan | `POST /vendors/{supplierId}/restriction/lift` | "Pengiriman sudah tepat waktu dua bulan." → Aktif |
| Berakhir sendiri | Sistem (dihitung saat dibaca) | — | Mulai 01-07-2027 vendor tampil Aktif |

Setiap aksi menulis riwayat vendor (`RESTRICT`, `RESTRICTION_LIFT`) beserta alasan dan status asal → tujuan.

**Jalur gagal:**

| Kejadian | Respons |
| --- | --- |
| Alasan kosong | 422 `PBJ-VAL-105` "Alasan pembatasan wajib diisi." |
| Tanggal berakhir sebelum hari ini | 422 `PBJ-VAL-105` "Tanggal berakhir tidak boleh sebelum hari ini." |
| Vendor Diblokir | 409 `PBJ-VAL-093` "Vendor yang Diblokir tidak dapat dibatasi." |
| Vendor sudah Dibatasi | 409 `PBJ-VAL-093` (cabut dulu bila ingin mengubah) |
| Cabut vendor yang tidak Dibatasi | 409 `PBJ-VAL-093` |
| Cabut dengan alasan < 10 karakter | 422 `PBJ-VAL-104` |
| Versi basi | 409 `PBJ-VAL-090` |
| Tanpa izin `Restrict` | 403 |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Areas/Corporate/ProcurementManagement/Vendor/Controllers/VendorsController.cs` | Diperbarui | Dua action dengan `[AccessAction("Restrict")]`/`[AccessPermission(ProcurementVendor, Restrict)]`, `Idempotency-Key` pada Batasi |
| `.../Vendor/Services/ProcurementVendorProfileService.cs` | Diperbarui | `RestrictAsync`, `LiftRestrictionAsync`, penjaga status, riwayat |
| `.../Vendor/DTOs/VendorDtos.cs` | Diperbarui | `RestrictVendorRequest`, `LiftRestrictionRequest` |
| `.../Common/Constants/ProcurementDocumentHistoryActions.cs` | Diperbarui | `RESTRICT`, `RESTRICTION_LIFT` |

### 3.2 Dampak kontrak API, database, dan keamanan

**Database.** Tidak ada perubahan skema. Penulisan ke `PrcVendorProfile` (kolom pembatasan, versi) dan
`PrcDocumentHistory`. `MstSupplier` hanya dibaca.

**Keamanan.** Aksi `Restrict` kini muncul di layar Akses Role. Peta peran awal (PERM bagian 3) memberikannya kepada
Manajer Pengadaan; Tim Pengadaan tanpa `Restrict` mendapat 403. Data vendor tidak dibatasi per unit.

**Selisih terhadap kontrak (delta), dicatat apa adanya.**

| # | Kontrak / desain | Yang dibangun | Alasan |
| ---: | --- | --- | --- |
| D-1 | STATE hanya punya Aktif → Dibatasi | Membatasi vendor yang **sedang** Dibatasi → 409 `PBJ-VAL-093`; harus dicabut dulu | Mengubah alasan atau tanggal berakhir tanpa jejak cabut tidak diputuskan; jalur aman memaksa dua langkah yang tercatat |
| D-2 | `RestrictVendorRequest.expectedRowVersion` | Wajib bila profil sudah ada; diabaikan saat profil pertama dibuat oleh aksi ini | Sama dengan atur kategori (`VD-003`) |
| D-3 | — | Cabut mengosongkan `RestrictedUntil`, `RestrictionReason`, `RestrictedAt`, `RestrictedByUserId`; alasan dan pelaku tetap di riwayat | Kolom profil menggambarkan keadaan sekarang, riwayat menyimpan jejak |
| D-4 | — | Vendor nonaktif di Master Data tetap boleh dibatasi; statusnya tetap tampil Nonaktif | Tidak ada aturan yang melarang; Nonaktif lebih kuat dari Dibatasi (STATE bagian 1) |
| D-5 | `AT-VD-20`: "kelayakan `isRestricted = true`" | Dibuktikan lewat detail vendor (`isRestricted = true`, status Dibatasi). Endpoint kelayakan milik `BE-PBJ-VD-005` | Endpoint kelayakan belum ada di task ini |

## 4. Dokumentasi endpoint

#### Corporate / Procurement Management / Vendor / Vendor

Base URL: `api/v1/corporate/procurement-management/vendors`.

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/{supplierId}/restriction` | Tetapkan Dibatasi | `ProcurementVendor : Restrict` | `{ reason, restrictedUntil?, expectedRowVersion? }` + `Idempotency-Key` | `VendorDetailResponse` |
| `POST` | `/{supplierId}/restriction/lift` | Cabut Dibatasi | `ProcurementVendor : Restrict` | `{ reason, expectedRowVersion }` | `VendorDetailResponse` |

Kode status: 200; 400 (validasi model); 401; 403; 404; 409 (`PBJ-VAL-090`, `093`); 422 (`PBJ-VAL-104`, `105`).

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -c Release` dan `--no-incremental` | 0 error; 0 warning dari berkas yang berubah | `PASS` | Keluaran build |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 4 berkas | VIOLATION 0, REVIEW 0, INFO 0 | `PASS` | Laporan QBE |
| `dotnet test` | Tidak ada project test (kebijakan repository) | `NOT RUN` | — |
| Migration | Tidak ada perubahan skema | `NOT APPLICABLE` | — |
| Uji runtime HTTP ke build baru di `https://localhost:7185` terhadap `QuilvianNewDevSukma` | 19 skenario, 19 `PASS` | `PASS` | `vd006.results.json` scratchpad sesi + query DB |

**Fixture uji (`TEST-PBJ VD006 20261117160618`, lewat API SuperAdmin).** Dua akun `Employee` **tanpa role**:
- `mgr`: `ProcurementVendor : Read, Update, Restrict`.
- `staff`: `ProcurementVendor : Read, Update`.

Ada juga tiga supplier uji:
- S1 aktif;
- S2 diblokir lewat Master Data;
- S3 untuk uji tanggal berakhir.

**Data uji `AT-VD-21`.** API menolak tanggal berakhir yang sudah lewat, jadi tanggal berakhir profil S3 dimundurkan
langsung di `QuilvianNewDevSukma` (`RestrictedUntil` = kemarin). Ini sesuai bunyi skenario ("data uji").

| ID | Skenario | Hasil | Bukti |
| --- | --- | --- | --- |
| `AU-00` | Butir Akses Role `ProcurementVendor` | `PASS` | `Read`, `Restrict`, `Update` |
| `R-01` | Tanpa `Restrict` membatasi | `PASS` | 403 |
| `AT-VD-20a` | Batasi tanpa alasan | `PASS` | 422 `PBJ-VAL-105` |
| `R-02` | Tanggal berakhir kemarin | `PASS` | 422 `PBJ-VAL-105` |
| `AT-VD-20b` | Batasi dengan alasan, berakhir besok | `PASS` | Status Dibatasi, `isRestricted = true`, `restrictedUntil` besok, pelaku tercatat; profil lahir; riwayat `CREATE`, `RESTRICT` |
| `R-03` | Kunci idempoten sama | `PASS` | 200; `RESTRICT` sekali |
| `R-04` | Batasi vendor yang sudah Dibatasi | `PASS` | 409 `PBJ-VAL-093` |
| `R-05` | Saring status Dibatasi | `PASS` | Hanya S1 (bukti cabang `VD-003`) |
| `R-06` | Batasi vendor Diblokir | `PASS` | 409 `PBJ-VAL-093` |
| `R-07` | Cabut dengan alasan pendek | `PASS` | 422 `PBJ-VAL-104` |
| `R-08` | Cabut dengan versi basi | `PASS` | 409 `PBJ-VAL-090` |
| `R-09` | Cabut tanpa `Restrict` | `PASS` | 403 |
| `R-10` | Cabut | `PASS` | Aktif; kolom pembatasan kosong di DB; riwayat `RESTRICTION_LIFT` |
| `R-11` | Cabut vendor yang tidak Dibatasi | `PASS` | 409 `PBJ-VAL-093` |
| `R-12` | Batasi tanpa tanggal berakhir | `PASS` | Dibatasi, `restrictedUntil` kosong |
| `R-13` | Batasi dengan berakhir hari ini | `PASS` | Diterima, Dibatasi |
| `AT-VD-21` | Tanggal berakhir dimundurkan ke kemarin (data uji) | `PASS` | Detail Aktif, `isRestricted = false`, `restrictedUntil` tidak ditampilkan; tersaring di status Aktif |
| `R-14` | Batasi lagi vendor yang masa Dibatasinya lewat | `PASS` | 200, Dibatasi |
| `R-15` | Vendor tak dikenal | `PASS` | 404 |

19 skenario, 19 `PASS`.

**Sesudah uji.** Supplier uji dihapus lunak lewat API. Sisa fixture dibersihkan dengan
`cleanup_vendor_fixture.py vd006.fixture.json` atas izin pemilik (17 November 2026, "lanjutkan" atas tawaran
pembersihan). Dry run lebih dulu, lalu apply dalam satu transaksi:
- dihapus lunak: 2 profil vendor, 5 policy, 2 Employee, 2 jabatan, dan 1 unit (kode diberi akhiran `-DEL-20261117160618`);
- dinonaktifkan dan dikunci: 2 akun.

Sesudahnya `PrcVendorProfile` aktif = 0. Riwayat tidak disentuh.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-VD-20` 422 `PBJ-VAL-105`, lalu Dibatasi dengan `isRestricted = true` | Terpenuhi (detail vendor; kelayakan menyusul `VD-005`, delta D-5) | `AT-VD-20a`, `AT-VD-20b` |
| `AT-VD-21` tampil Aktif tanpa job saat tanggal berakhir lewat | Terpenuhi | `AT-VD-21` |
| Tolak bila vendor Diblokir | Terpenuhi | `R-06` |
| Riwayat vendor | Terpenuhi | `AT-VD-20b`, `R-10` |
| Laporan | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Delta D-1 (harus cabut dulu untuk mengubah pembatasan) dapat dilonggarkan bila pemilik ingin aksi "ubah pembatasan" |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | `NONE` di luar data uji dev |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Belum di-commit. Diubah: `VendorsController.cs`, `ProcurementVendorProfileService.cs`, `VendorDtos.cs`, `ProcurementDocumentHistoryActions.cs`, laporan ini, roadmap, traceability, `MODULE-STATUS.md` |
| Langkah berikutnya | `BE-PBJ-VD-005` (kelayakan vendor; `VD-004` dan `VD-006` sudah ✅) |
