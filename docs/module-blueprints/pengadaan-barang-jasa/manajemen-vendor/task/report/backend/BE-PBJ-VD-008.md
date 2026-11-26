# Laporan Perubahan Backend — `BE-PBJ-VD-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PBJ-VD-008` |
| Judul | Penilaian berkala |
| Slice | `MVP-V3` — penilaian vendor (`04-prd-to-mvp.md` bagian 20) |
| Roadmap | `manajemen-vendor/roadmap/backend-roadmap.md` revisi 1, kartu `BE-PBJ-VD-008` |
| Trace | `PBJ-DEC-044`, `048`, `049`, `050`, `059`; FR-VD-040, 041, 042; `PBJ-VAL-090`, `093`, `104`, `108`..`112`; `AT-VD-30`..`36` |
| Contract version | `PBJ-VD-API` 1.0 §Vendor Evaluation, `PBJ-VD-STATE` 1.0 bagian 4, `PBJ-VD-VAL` 1.0, `PBJ-VD-PERM` 1.0 — semua `approved` (`PBJ-DEC-062`) |
| Dependency | `BE-PBJ-VD-002` ✅, `BE-PBJ-VD-003` ✅ |
| Klasifikasi | `MEDIUM` (logika 3, kontrak API 2, database 0, keamanan 2 — pemfinal ≠ penilai) |
| Task mode | `BACKEND` (pemilik: "saya berikan izin dan lanjutkan", 18 November 2026) |
| Target tulis | `VendorEvaluationsController`, `ProcurementVendorEvaluationService`, `VendorEvaluationDtos` (baru); konstanta aksi riwayat, extension DI |
| Model | Claude Opus 5.5 |
| Branch / commit | Backend `sukmagp` @ `a6df5619`, frontend `b1b360efa` (belum di-commit) |
| Tanggal | 18 November 2026 |
| Status | ✅ **SELESAI.** Sepuluh endpoint penilaian dan penyedia akses lampiran `PBJ_VENDOR_EVALUATION` berjalan; uji runtime 32/32 `PASS` (putaran kedua; putaran pertama berhenti di fixture karena kode kategori uji bentrok); tanpa migration |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `Vendor` |
| Registry | `Corporate / Procurement · ProcurementManagement / Vendor · Prc · ACTIVE` (`PBJ-DEC-063`) |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` untuk `ProcurementDocumentHistoryActions.cs` dan extension DI |
| Arketipe endpoint | Aggregate ber-lifecycle (`DRAFT → FINAL/CANCELLED`, `FINAL → SUPERSEDED` lewat revisi) dengan aksi `POST /{id}/<aksi>`; tanpa `DELETE` dan tanpa `PATCH /status` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-TXN-001`, `QBE-LOG-001`, `QBE-AUD-001` |
| Hasil checker | QBE Strict `PASS` (5 berkas, 0 temuan) |

## 1. Masalah yang diselesaikan

Rumah sakit menilai vendor secara berkala, misalnya per semester, untuk setiap kategori yang dilayani vendor itu.
Dengan task ini penilaian bisa dicatat dengan kriteria berbobot, nilai akhirnya dihitung otomatis, lalu difinalkan
oleh orang lain. Penilaian final yang salah dikoreksi lewat revisi tanpa menghapus jejak lamanya. Rekomendasi
(Lanjutkan, Batasi, Blokir) hanya saran dan tidak mengubah status vendor.

## 2. Proses bisnis

| Langkah | Pelaku | Endpoint | Contoh |
| --- | --- | --- | --- |
| Membuat draf | Tim Pengadaan (penilai) | `POST /vendor-evaluations` | PT Sehat Medika, Alat Kesehatan, Semester I 2027 → kriteria 40/40/20 disalin |
| Mengisi nilai | Penilai | `PUT /vendor-evaluations/{id}` | Ketepatan waktu 4, Kualitas 5, Layanan purnajual 3, rekomendasi Lanjutkan → nilai akhir 4,20 |
| Memfinalkan | Manajer Pengadaan (bukan penilai) | `POST …/{id}/finalize` | Final; riwayat vendor "nilai 4.20, rekomendasi Lanjutkan" |
| Mengoreksi | Tim Pengadaan | `POST …/{id}/revisions` | "Nilai layanan purnajual salah input." → draf revisi menyalin nilai |
| Memfinalkan revisi | Manajer | `POST …/{revisi}/finalize` | Revisi Final 4,40; penilaian lama Digantikan; keduanya tetap terbaca |
| Membatalkan draf | Tim Pengadaan | `POST …/{id}/cancel` | "Salah pilih periode penilaian." |

**Kriteria disalin saat draf dibuat.** Mengubah bobot di master (mis. Kualitas 40 → 50) tidak mengubah penilaian yang
sudah ada.

**Jalur gagal:**

| Kejadian | Respons |
| --- | --- |
| Total bobot kriteria aktif kategori ≠ 100 | 422 `PBJ-VAL-108` "Total bobot kriteria … harus 100%. Saat ini 80%." |
| Vendor tidak melayani kategori | 422 `PBJ-VAL-108` |
| Penilaian vendor × kategori × periode yang sama sudah ada (termasuk revisi final) | 409 `PBJ-VAL-109` |
| Finalkan dengan nilai kosong / di luar 1–5 / tanpa rekomendasi | 422 `PBJ-VAL-110` |
| Penilai memfinalkan sendiri | 403 `PBJ-VAL-111` "Penilaian harus difinalkan orang lain." |
| Revisi dari penilaian bukan final, atau revisi kedua saat revisi masih draf | 409 `PBJ-VAL-112` |
| Alasan revisi/batal < 10 karakter | 422 `PBJ-VAL-104` |
| Mengubah atau membatalkan penilaian final | 409 `PBJ-VAL-093` |
| Versi basi | 409 `PBJ-VAL-090` |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Areas/Corporate/ProcurementManagement/Vendor/Controllers/VendorEvaluationsController.cs` | Baru | 10 endpoint, resource `ProcurementVendorEvaluation` (`Read`, `Create`, `Update`, `Finalize`, `Revise`) |
| `.../Vendor/Services/ProcurementVendorEvaluationService.cs` | Baru | Buat, ubah, finalkan, revisi, batal, daftar, ringkasan, detail, riwayat; `VendorEvaluationAttachmentAccessProvider` |
| `.../Vendor/DTOs/VendorEvaluationDtos.cs` | Baru | Query, permintaan, respons, ringkasan, metadata |
| `.../Common/Constants/ProcurementDocumentHistoryActions.cs` | Diperbarui | `FINALIZE`, `EVALUATION_FINAL`, `EVALUATION_SUPERSEDE` |
| `.../ProcurementManagementServiceCollectionExtensions.cs` | Diperbarui | Service penilaian + penyedia akses `PBJ_VENDOR_EVALUATION` |

### 3.2 Dampak kontrak API, database, dan keamanan

**Database.** Tidak ada perubahan skema. Penulisan ke `PrcVendorEvaluation`, `PrcVendorEvaluationScore`, dan
`PrcDocumentHistory`. Index unik parsial dari `BE-PBJ-VD-001` menjadi lapisan kedua untuk `PBJ-VAL-109`.

**Keamanan.**
- Pemfinal ≠ penilai dijaga di service (403 `PBJ-VAL-111`), di luar izin Akses Role.
- Peta peran awal: Tim Pengadaan mendapat `Read, Create, Update, Revise`; Manajer Pengadaan menambah `Finalize`.
- Hak lampiran `PBJ_VENDOR_EVALUATION`:
  - tanpa `Read` → tidak terlihat;
  - draf → pemegang `Update` boleh menambah berkas;
  - final/batal → hanya baca.

**Selisih terhadap kontrak (delta), dicatat apa adanya.**

| # | Kontrak / desain | Yang dibangun | Alasan |
| ---: | --- | --- | --- |
| D-1 | Satu penilaian per vendor × kategori × periode (`PBJ-DEC-059`) | Pemeriksaan service mencakup **revisi** yang `DRAFT`/`FINAL` | Index basis data hanya menjaga penilaian asal. Setelah penilaian asal `SUPERSEDED`, revisi final-nya tetap mengisi periode itu |
| D-2 | `POST /{id}/finalize` hanya `Idempotency-Key` | Body `{ expectedRowVersion? }` opsional; body kosong diterima | Konsisten dengan aksi status lain tanpa mewajibkan body |
| D-3 | — | Revisi menyalin nilai, rekomendasi, catatan, **dan snapshot kriteria** penilaian asal (bukan kriteria master terkini); pembuat revisi menjadi penilai revisi | Revisi = koreksi penilaian yang sama. Pemfinal revisi wajib orang lain lagi |
| D-4 | — | `PUT` draf boleh dilakukan pemegang `Update` mana pun, bukan hanya pembuat | Matriks hak akses tidak membatasi penyunting draf; aturan "orang lain" hanya untuk finalisasi |
| D-5 | — | Nilai akhir dihitung ulang setiap simpan bila semua nilai terisi (kosong bila belum lengkap); pembulatan dua desimal, setengah ke atas | "Nilai akhir dari server" (FE §4.5) |
| D-6 | Kamus data §7: `PBJ_VENDOR_EVALUATION` untuk lampiran pendukung | Penyedia akses lampiran ditambahkan di task ini | Tanpa penyedia, lampiran penilaian tertolak (fail-closed) |
| D-7 | Ringkasan: "draf, final per periode, sebaran rekomendasi" | `GET /summary?itemCategoryId&periodYear&periodType&periodNo` → hitungan per status, sebaran rekomendasi final, rata-rata nilai akhir | Rincian bentuk respons |
| D-8 | — | Jenis periode `SEMESTER` (1–2), `QUARTER` (1–4), `YEAR` (1); nomor di luar rentang → 422 tanpa kode | Batas basis data `PeriodNo` 1–4 terlalu longgar untuk semester/tahun |

## 4. Dokumentasi endpoint

#### Corporate / Procurement Management / Vendor / Vendor Evaluation

Base URL: `api/v1/corporate/procurement-management/vendor-evaluations`.

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Periode data, urutan, kategori, jenis periode, status, rekomendasi | `ProcurementVendorEvaluation : Read` | — | `VendorEvaluationFilterMetadataResponse` |
| `GET` | `/summary` | Hitungan per status, sebaran rekomendasi, rata-rata nilai | `… : Read` | `itemCategoryId?`, `periodYear?`, `periodType?`, `periodNo?` | `VendorEvaluationSummaryResponse` |
| `GET` | `/` | Daftar | `… : Read` | `startDate`, `endDate`, `customPeriod`, `search`, `supplierId`, `itemCategoryId`, `periodYear`, `periodType`, `periodNo`, `status`, `recommendation`, `sortBy` (`createDateTime`/`period`/`supplier`/`finalScore`), `sortDirection`, `pageNumber`, `pageSize` | `PagedResult<VendorEvaluationListItemResponse>` |
| `GET` | `/{id}` | Detail beserta nilai per kriteria | `… : Read` | — | `VendorEvaluationResponse` |
| `POST` | `/` | Buat draf | `… : Create` | `{ supplierId, itemCategoryId, periodYear, periodType?, periodNo, notes? }` | `VendorEvaluationResponse` (**201**) |
| `PUT` | `/{id}` | Isi nilai, rekomendasi, catatan | `… : Update` | `{ expectedRowVersion, scores: [{ criterionId, score?, note? }], recommendation?, notes? }` | `VendorEvaluationResponse` |
| `POST` | `/{id}/finalize` | Finalkan | `… : Finalize` | `{ expectedRowVersion? }` + `Idempotency-Key` | `VendorEvaluationResponse` |
| `POST` | `/{id}/revisions` | Revisi dari final | `… : Revise` | `{ reason }` | `VendorEvaluationResponse` (**201**) |
| `POST` | `/{id}/cancel` | Batalkan draf | `… : Update` | `{ reason, expectedRowVersion? }` | `VendorEvaluationResponse` |
| `GET` | `/{id}/history` | Riwayat penilaian | `… : Read` | — | `List<DocumentHistoryResponse>` |

Contoh potongan respons `GET /{id}`:

```json
{ "periodLabel": "Semester I 2027", "status": "FINAL", "finalScore": 4.20, "recommendation": "CONTINUE",
  "recommendationLabel": "Lanjutkan", "evaluatorName": "Rina", "finalizedByName": "Budi",
  "scores": [ { "criterionName": "Ketepatan waktu", "weightPercent": 40.00, "score": 4 },
              { "criterionName": "Kualitas", "weightPercent": 40.00, "score": 5 },
              { "criterionName": "Layanan purnajual", "weightPercent": 20.00, "score": 3 } ] }
```

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -c Release` dan `--no-incremental` | 0 error; 0 warning dari berkas baru atau berubah | `PASS` | Keluaran build |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 5 berkas | VIOLATION 0, REVIEW 0, INFO 0 | `PASS` | Laporan QBE |
| `dotnet test` | Tidak ada project test (kebijakan repository) | `NOT RUN` | — |
| Migration | Tidak ada perubahan skema | `NOT APPLICABLE` | — |
| Uji runtime HTTP ke build baru di `https://localhost:7185` terhadap `QuilvianNewDevSukma` | Putaran 1 berhenti saat menyiapkan fixture (kode kategori uji `LAB`/`LAIN` bentrok di skrip). Putaran 2: **32/32 `PASS`** | `PASS` | `vd008.results.json` scratchpad sesi + query DB |

**Fixture uji putaran 2 (`TEST-PBJ VD008 20261118091815`, lewat API SuperAdmin).** Tiga akun `Employee` **tanpa
role**:
- `staff`: penilaian `Read, Create, Update, Revise`, vendor `Read, Update`, kriteria `Read, Create, Update, Delete`, kategori `Read`, lampiran `Read, Upload`.
- `mgr`: penilaian `Read, Create, Update, Revise, Finalize`, vendor `Read`.
- `req`: hanya `ProcurementRequest : Read`.

Ada juga empat kategori uji (ALKES 40/40/20, OBAT 40/40 = 80%, LAB 100%, LAIN tanpa kriteria) dan satu supplier uji
yang melayani ALKES, OBAT, dan LAB.

| ID | Skenario | Hasil | Bukti |
| --- | --- | --- | --- |
| `AU-00` | Butir Akses Role | `PASS` | `Create`, `Finalize`, `Read`, `Revise`, `Update` |
| `E-00` | Unit Peminta membuka daftar | `PASS` | 403 |
| `AT-VD-30` | Bobot kriteria kategori 80% | `PASS` | 422 `PBJ-VAL-108` "… Saat ini 80%." |
| `E-01` | Kategori tidak dilayani vendor | `PASS` | 422 `PBJ-VAL-108` |
| `E-02` | Semester 3 | `PASS` | 422 |
| `E-03` | Buat ALKES Semester I 2027 | `PASS` | 201 `DRAFT`; 3 kriteria disalin 40/40/20 |
| `AT-VD-31` | Penilaian kedua periode sama; lalu kategori LAB | `PASS` | 409 `PBJ-VAL-109`; LAB 201 |
| `E-04` | Isi dua dari tiga nilai | `PASS` | Nilai akhir kosong |
| `AT-VD-34` | Finalkan dengan satu kriteria kosong | `PASS` | 422 `PBJ-VAL-110` |
| `E-05` | Nilai 6 | `PASS` | 422 `PBJ-VAL-110` |
| `E-06` | Versi basi | `PASS` | 409 `PBJ-VAL-090` |
| `AT-VD-32a` | 40/40/20 × 4/5/3 | `PASS` | `finalScore = 4.20` |
| `E-07` | Tim Pengadaan (tanpa `Finalize`) memfinalkan | `PASS` | 403 |
| `AT-VD-33a` | Manajer memfinalkan penilaiannya sendiri | `PASS` | 403 `PBJ-VAL-111` |
| `AT-VD-33b` | Manajer memfinalkan penilaian Tim; ulang kunci sama; `PUT` sesudahnya | `PASS` | `FINAL`, pemfinal tercatat; 200 tanpa `FINALIZE` ganda; 409 `PBJ-VAL-093` |
| `AT-VD-32b` | Bobot master diubah ke 30/50/20 | `PASS` | Nilai tetap 4.20; bobot salinan di DB tetap 40/40/20 |
| `E-08` | Hapus kriteria yang dipakai penilaian | `PASS` | 400 `PBJ-VAL-071` (bukti cabang `BE-PBJ-VD-002`) |
| `E-09` | Revisi alasan pendek | `PASS` | 422 `PBJ-VAL-104` |
| `E-10` | Revisi dari final; revisi kedua | `PASS` | 201 `DRAFT` menyalin nilai (4.20); 409 `PBJ-VAL-112` |
| `E-11` | Revisi dari draf | `PASS` | 409 `PBJ-VAL-112` |
| `AT-VD-35` | Revisi (Layanan 3 → 4) difinalkan | `PASS` | Revisi `FINAL` 4.40 (bobot salinan lama); lama `SUPERSEDED`, `latestRevisionId` terisi; riwayat `SUPERSEDE` |
| `E-12` | Penilaian asal baru untuk periode yang diisi revisi final | `PASS` | 409 `PBJ-VAL-109` (delta D-1) |
| `AT-VD-36` | Rekomendasi Blokir (LAB) difinalkan | `PASS` | `MstSupplier.IsBlacklisted` tetap `false`, `IsActive` tetap `true`; 0 usulan status; status vendor Aktif |
| `E-13` | Detail vendor: penilaian final terakhir per kategori | `PASS` | ALKES 4.40 Lanjutkan, LAB 1.00 Blokir (bukti cabang `BE-PBJ-VD-003`) |
| `E-14` | Riwayat vendor | `PASS` | `EVALUATION_FINAL` ×3, `EVALUATION_SUPERSEDE` |
| `E-15` | Batal alasan pendek | `PASS` | 422 `PBJ-VAL-104` |
| `E-16` | Batalkan draf; ulang; nilai lagi periode itu | `PASS` | `CANCELLED`; 409 `PBJ-VAL-093`; 201 |
| `E-17` | Lampiran pendukung | `PASS` | Draf 200; penilaian final 409 |
| `E-18` | Ringkasan Semester I 2027 | `PASS` | Final 2, digantikan 1, Lanjutkan 1, Blokir 1 |
| `E-19` | Daftar status FINAL urut nilai | `PASS` | Revisi ALKES 4.40, LAB 1.00 |
| `E-20` | Status tidak dikenal | `PASS` | 422 |
| `E-21` | Penilaian tak dikenal | `PASS` | 404 |

32 skenario, 32 `PASS`.

**Sesudah uji.** Supplier dan kategori uji kedua putaran dihapus lunak lewat API. Sisa fixture dibersihkan dengan
`cleanup_vendor_fixture.py vd008.fixture.run1.json vd008.fixture.json` atas izin pemilik (18 November 2026, "saya berikan
izin dan lanjutkan"). Dry run lebih dulu, lalu apply dalam satu transaksi:
- putaran 2, dihapus lunak: 1 lampiran, 13 nilai, 5 penilaian, 6 kriteria, 3 kategori vendor, dan 1 profil;
- per putaran, dihapus lunak: 20 policy, 3 Employee, 3 jabatan, dan 1 unit (kode diberi akhiran `-DEL-<run>`);
- per putaran, dinonaktifkan dan dikunci: 3 akun.

Sesudahnya profil dan penilaian aktif = 0; kriteria aktif tinggal 3 kriteria seed `UMUM`. Riwayat tidak disentuh.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-VD-30` bobot 80% → 422 `PBJ-VAL-108` | Terpenuhi | `AT-VD-30` |
| `AT-VD-31` periode sama → 409 `PBJ-VAL-109`; kategori lain → 201 | Terpenuhi | `AT-VD-31` |
| `AT-VD-32` 4,20 dan tetap 4,20 sesudah master diubah | Terpenuhi | `AT-VD-32a`, `AT-VD-32b` |
| `AT-VD-33` penilai sendiri → 403 `PBJ-VAL-111`; final terkunci → `PUT` 409 `PBJ-VAL-093` | Terpenuhi | `AT-VD-33a`, `AT-VD-33b` |
| `AT-VD-34` kriteria kosong → 422 `PBJ-VAL-110` | Terpenuhi | `AT-VD-34` |
| `AT-VD-35` lama `SUPERSEDED`, revisi `FINAL`, keduanya terbaca | Terpenuhi | `AT-VD-35` |
| `AT-VD-36` rekomendasi Blokir tidak mengubah status, tidak ada usulan otomatis | Terpenuhi | `AT-VD-36` |
| Laporan | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kriteria dan bobot masih nilai awal (`PBJ-OQ-034`). Delta D-4 (penyunting draf tidak dibatasi pada pembuatnya) dapat diperketat bila pemilik menghendaki |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | `NONE` di luar data uji dev |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu jeda batas pemakaian sesi; dilanjutkan dari kondisi terverifikasi (berkas yang tampak berubah ternyata hasil patch sendiri, dicek dari waktu ubahnya) |
| Status Git | Belum di-commit. Baru: `VendorEvaluationsController.cs`, `ProcurementVendorEvaluationService.cs`, `VendorEvaluationDtos.cs`, laporan ini. Diubah: `ProcurementDocumentHistoryActions.cs`, extension DI, roadmap, traceability, `MODULE-STATUS.md` |
| Langkah berikutnya | `BE-PBJ-VD-007` (usulan blokir dan kunci Master Data) — butuh konfigurasi approval uji `PBJ-VND-*` di dev; `FE-PBJ-VD-001`..`003`, `005` kini tidak terhalang backend |
