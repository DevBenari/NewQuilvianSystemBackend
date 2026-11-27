# Laporan Perubahan Backend — `BE-PBJ-VD-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PBJ-VD-005` |
| Judul | Kelayakan vendor |
| Slice | `MVP-V1` — dokumen legalitas dan kelayakan (`04-prd-to-mvp.md` bagian 20) |
| Roadmap | `manajemen-vendor/roadmap/backend-roadmap.md` revisi 1, kartu `BE-PBJ-VD-005` |
| Trace | `PBJ-DEC-021`, `045`, `047`, `052`, `054`, `061`; FR-VD-023; `AT-VD-14`, `15`, `17`, `27` |
| Contract version | `PBJ-VD-API` 1.0 (`GET /vendors/eligibility`), `PBJ-VD-INT` 1.0 §2 (dengan delta tambahan di bagian 3.2), `PBJ-VD-STATE` 1.0 bagian 1–2 — semua `approved` (`PBJ-DEC-062`) |
| Dependency | `BE-PBJ-VD-004` ✅, `BE-PBJ-VD-006` ✅ |
| Klasifikasi | `MEDIUM` (logika 2, kontrak API/integrasi 2, database 0, keamanan 1) |
| Task mode | `BACKEND` (pemilik: "lanjutkan", 17 November 2026) |
| Target tulis | `ProcurementVendorEligibilityService` (baru), `VendorEligibilityDtos` (baru), `VendorsController`, extension DI |
| Model | Claude Opus 5.5 |
| Branch / commit | Backend `sukmagp` @ `a6df5619`, frontend `b1b360efa` (belum di-commit) |
| Tanggal | 17 November 2026 |
| Status | ✅ **SELESAI.** `GET /vendors/eligibility` dan `ProcurementVendorEligibilityService.CheckAsync`/`GetEligibleAsync` berjalan; uji runtime 14/14 `PASS`; tanpa migration |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `Vendor` |
| Registry | `Corporate / Procurement · ProcurementManagement / Vendor · Prc · ACTIVE` (`PBJ-DEC-063`) |
| Keberlakuan | `NEW CODE` |
| Arketipe endpoint | Baca saja (`GET /vendors/eligibility`) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001` |
| Hasil checker | QBE Strict `PASS` (4 berkas, 0 temuan) |

## 1. Masalah yang diselesaikan

Sub-modul `pemilihan-penyedia` nanti harus tahu vendor mana yang boleh dipilih untuk suatu kategori. Tim Pengadaan
juga perlu melihat alasan sebuah vendor tidak layak. Task ini menyatukan seluruh aturan kelayakan dalam satu service
baca-saja dan satu endpoint. Aturan yang disatukan:
- status di Master Data dan blokir;
- kategori yang dilayani;
- dokumen wajib yang aktif;
- status Dibatasi.

## 2. Proses bisnis

| Pertanyaan | Jawaban sistem | Contoh |
| --- | --- | --- |
| Apakah PT Sehat Farma layak untuk kategori Obat? | `GET /vendors/eligibility?itemCategoryId=Obat&supplierId=…` | Belum ada kewajiban aktif untuk Obat → layak, walaupun belum ada dokumen (`PBJ-DEC-061`) |
| Admin mewajibkan Izin PBF untuk Obat | Vendor tanpa Izin PBF terverifikasi tidak layak | "Dokumen wajib Izin Pedagang Besar Farmasi belum terverifikasi." |
| Izin PBF lewat tanggal berlakunya | Tidak layak; status blokir di Master Data tidak disentuh | "Dokumen wajib Izin Pedagang Besar Farmasi kedaluwarsa sejak 16-11-2026." |
| Izin PBF baru berlaku 20 hari | Layak, dengan peringatan | "Dokumen wajib … berakhir dalam 20 hari (07-12-2026)." |
| Vendor Dibatasi | Layak, ditandai `isRestricted`; alasan pembatasan di peringatan | Pemilih wajib memberi alasan di RFQ (`PBJ-DEC-047`) |
| Vendor diblokir, nonaktif, atau tidak melayani kategori | Tidak layak | "Vendor diblokir." / "Vendor nonaktif di Master Data." / "Vendor tidak melayani kategori ini." |
| Daftar pilihan untuk RFQ Obat | `GET /vendors/eligibility?itemCategoryId=Obat&onlyEligible=true` | Hanya vendor yang layak |

Kelayakan hanya mencegah pilihan **baru**. Tidak ada proses yang dibatalkan dan tidak ada data yang ditulis
(`PBJ-DEC-054`).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Areas/Corporate/ProcurementManagement/Vendor/Services/ProcurementVendorEligibilityService.cs` | Baru | `CheckAsync(supplierId, itemCategoryId)`, `GetEligibleAsync(itemCategoryId)`, `EvaluateAsync(…)` |
| `.../Vendor/DTOs/VendorEligibilityDtos.cs` | Baru | `VendorEligibilityQuery`, `VendorEligibilityResponse` |
| `.../Vendor/Controllers/VendorsController.cs` | Diperbarui | `GET /eligibility` (`ProcurementVendor : Read`) |
| `.../ProcurementManagementServiceCollectionExtensions.cs` | Diperbarui | `ProcurementVendorEligibilityService` |

### 3.2 Dampak kontrak API, database, dan keamanan

**Database.** Tidak ada perubahan skema. Tidak ada penulisan.

**Keamanan.** Memakai izin baca vendor. Vendor tanpa izin itu (mis. Unit Peminta) mendapat 403.

**Hitungan kedaluwarsa pada `GET /vendors/summary`** (bagian kartu task) sudah dibangun di `BE-PBJ-VD-003`
(`expiredDocuments`, `expiringSoonDocuments`). Uji `E-07` membuktikan ulang bahwa izin lama yang sudah diganti tidak
dihitung kedaluwarsa.

**Selisih terhadap kontrak (delta), dicatat apa adanya.**

| # | Kontrak | Yang dibangun | Alasan / tindak lanjut |
| ---: | --- | --- | --- |
| D-1 | `VendorEligibilityResponse` = `supplierId`, `itemCategoryId`, `isEligible`, `isRestricted`, `reasons[]` | Ditambah `supplierCode`, `supplierName`, `warnings[]` (alasan Dibatasi, izin segera kedaluwarsa) | Daftar pilihan butuh nama vendor, dan `PBJ-DEC-047` menuntut pemilih tahu alasan pembatasan. Tambahan saja, bentuk lama tetap. **INT bagian 3 mewajibkan kenaikan `PBJ-VD-INT` (usul 1.1) dan pemberitahuan ke desain `pemilihan-penyedia`.** Sub-modul itu belum dimulai; revisi kontrak menjadi tugas pemilik desain |
| D-2 | `GET /eligibility` mengembalikan daftar | Parameter `onlyEligible` (bawaan `false`). Tanpa `supplierId`: semua vendor yang melayani kategori, layak atau tidak | Selaras dengan `GetEligibleAsync` di INT §2 |
| D-3 | — | Kategori nonaktif → semua vendor tidak layak untuk kategori itu | Risiko yang dicatat di `BE-PBJ-VD-002` (menonaktifkan kategori tidak memeriksa kewajiban aktif) |
| D-4 | — | `itemCategoryId` kosong → 400 (validasi model); kategori atau vendor tak dikenal → 404 | Pola endpoint vendor lain |

## 4. Dokumentasi endpoint

#### Corporate / Procurement Management / Vendor / Vendor

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/vendors/eligibility` | Kelayakan vendor untuk satu kategori | `ProcurementVendor : Read` | `itemCategoryId` (wajib), `supplierId?`, `onlyEligible?` | `List<VendorEligibilityResponse>` |

Contoh respons:

```json
[ { "supplierId": "…", "itemCategoryId": "…", "isEligible": false, "isRestricted": false,
    "reasons": ["Dokumen wajib Izin Pedagang Besar Farmasi kedaluwarsa sejak 16-11-2026."],
    "supplierCode": "SUP-RSMMC-00011", "supplierName": "PT Sehat Farma", "warnings": [] } ]
```

Kode status: 200; 400 (`itemCategoryId` kosong); 401; 403; 404.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -c Release` dan `--no-incremental` | 0 error; 0 warning dari berkas baru atau berubah | `PASS` | Keluaran build |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 4 berkas | VIOLATION 0, REVIEW 0, INFO 0 | `PASS` | Laporan QBE |
| `dotnet test` | Tidak ada project test (kebijakan repository) | `NOT RUN` | — |
| Migration | Tidak ada perubahan skema | `NOT APPLICABLE` | — |
| Uji runtime HTTP ke build baru di `https://localhost:7185` terhadap `QuilvianNewDevSukma` | 14 skenario, 14 `PASS` | `PASS` | `vd005.results.json` scratchpad sesi + query DB |

**Fixture uji (`TEST-PBJ VD005 20261117162520`, lewat API SuperAdmin).** Tiga akun `Employee` **tanpa role**:
- `proc`: vendor `Read, Update, Restrict`, dokumen `Read, Create`, lampiran `Read, Upload`, dokumen wajib `Read, Create, Update`, kategori `Read`.
- `ver`: vendor `Read`, dokumen `Read, Verify`, lampiran `Read`.
- `req`: hanya `ProcurementRequest : Read`.

Ada juga dua kategori uji (Obat, Alkes) dan lima supplier uji:
- A dan D melayani Obat;
- B hanya Alkes;
- C diblokir;
- E dinonaktifkan.

Kewajiban, dokumen, dan pembatasan semuanya dibuat lewat API fitur yang sudah ada.

| ID | Skenario | Hasil | Bukti |
| --- | --- | --- | --- |
| `E-00` | Unit Peminta memeriksa kelayakan | `PASS` | 403 |
| `E-01` | Tanpa `itemCategoryId`; kategori/vendor tak dikenal | `PASS` | 400; 404; 404 |
| `AT-VD-14` | Obat tanpa kewajiban aktif; vendor A tanpa dokumen | `PASS` | `isEligible = true`, `reasons = []` |
| `AT-VD-17` | Vendor B (Alkes saja) untuk Obat | `PASS` | `false`, "Vendor tidak melayani kategori ini." |
| `AT-VD-27` | Vendor C diblokir | `PASS` | `false`, "Vendor diblokir."; kategori vendor tetap 1 baris |
| `E-02` | Vendor E nonaktif di Master Data | `PASS` | `false`, "Vendor nonaktif di Master Data." |
| `AT-VD-15a` | Kewajiban Izin PBF diaktifkan; A tanpa izin terverifikasi | `PASS` | `false`, "Dokumen wajib Izin Pedagang Besar Farmasi belum terverifikasi." |
| `AT-VD-15b` | Izin PBF A terverifikasi, berlaku sampai kemarin | `PASS` | `false`, "… kedaluwarsa sejak 16-11-2026."; `MstSupplier.IsBlacklisted` tetap `false` |
| `E-03` | Izin PBF pengganti berlaku 20 hari | `PASS` | `true`, peringatan "berakhir dalam 20 hari (07-12-2026)" |
| `E-04` | Vendor D Dibatasi dengan izin lengkap | `PASS` | `true`, `isRestricted = true`, peringatan "Dibatasi sampai dicabut: …" (bukti kelayakan `AT-VD-20`) |
| `E-05` | Tanpa `supplierId` | `PASS` | A, C, D, E (tanpa B) dengan kelayakan masing-masing |
| `E-06` | `onlyEligible=true` | `PASS` | Hanya A dan D |
| `E-07` | Ringkasan vendor | `PASS` | `expiringSoonDocuments` ≥ 1; vendor A: segera kedaluwarsa 1, kedaluwarsa 0 (izin lama digantikan) |
| `E-08` | Kategori Obat dinonaktifkan | `PASS` | `false`, "Kategori … tidak aktif." |

14 skenario, 14 `PASS`.

**Sesudah uji.** Supplier dan kategori uji dihapus lunak lewat API. Sisa fixture dibersihkan dengan
`cleanup_vendor_fixture.py vd005.fixture.json` atas izin pemilik (18 November 2026, "saya berikan izin dan lanjutkan").
Dry run lebih dulu, lalu apply dalam satu transaksi:
- dihapus lunak: 3 lampiran, 3 dokumen, 5 kategori vendor, 5 profil, 1 kewajiban, 16 policy, 3 Employee, 3 jabatan, dan
  1 unit (kode diberi akhiran `-DEL-20261117162520`);
- dinonaktifkan dan dikunci: 3 akun.

Sesudahnya profil, dokumen, dan kewajiban yang aktif = 0. Riwayat tidak disentuh.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-VD-14` tanpa kewajiban aktif → layak | Terpenuhi | `AT-VD-14` |
| `AT-VD-15` tanpa izin terverifikasi, lalu kedaluwarsa → tidak layak dengan alasan; blokir tidak berubah | Terpenuhi | `AT-VD-15a`, `AT-VD-15b` |
| `AT-VD-17` tidak melayani kategori → tidak layak | Terpenuhi | `AT-VD-17` |
| `AT-VD-27` diblokir → tidak layak; tidak ada proses lain dibatalkan | Terpenuhi | `AT-VD-27` (service baca saja) |
| `CheckAsync`/`GetEligibleAsync` tersedia bagi `pemilihan-penyedia` | Terpenuhi | `ProcurementVendorEligibilityService` |
| Hitungan kedaluwarsa pada ringkasan vendor | Terpenuhi (dibangun `VD-003`, dibuktikan ulang) | `E-07` |
| Laporan | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Delta D-1 menambah bentuk `VendorEligibilityResponse`; pemilik desain perlu menaikkan `PBJ-VD-INT` ke 1.1 sebelum `pemilihan-penyedia` dirancang |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | `NONE` di luar data uji dev |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Belum di-commit. Baru: `ProcurementVendorEligibilityService.cs`, `VendorEligibilityDtos.cs`, laporan ini. Diubah: `VendorsController.cs`, extension DI, roadmap, traceability, `MODULE-STATUS.md` |
| Langkah berikutnya | `BE-PBJ-VD-007` (usulan blokir dan kunci Master Data) atau `BE-PBJ-VD-008` (penilaian). `FE-PBJ-VD-003` kini tidak terhalang backend |
