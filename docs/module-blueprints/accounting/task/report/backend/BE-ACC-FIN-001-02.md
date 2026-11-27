# Laporan Perubahan Backend — `BE-ACC-FIN-001-02`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-FIN-001-02` |
| Judul | Kolom klasifikasi dan pewarisannya |
| Slice | Gelombang pengiriman `FIN-3` |
| Roadmap | `roadmap/backend-roadmap-finishing.md` revisi 2, kartu `BE-ACC-FIN-001-02` |
| Trace | ACC-FIN-001 + rasio ACC-FIN-008 (`ACC-DEC-161` butir 10); `FR-FIN-054`, `055`, `056`, `058`; `ACC-DEC-167`, `168`, `169`, `173` |
| Contract version | `ACC-API-0.18` (`GET /`, `/{id}`, `/tree`, `/next-code`, `POST`, `PUT`); `ACC-VALIDATION-0.13` `VAL-COA-07`..`12`; `ACC-PERMISSION-0.9`; kamus data bagian 21 — approved Rizki 25 November 2026 (`GATE-DESAIN-1125`) |
| Dependency | `BE-ACC-FIN-001-01` ✅ |
| Izin migration | `OQ-167-1` diberikan Rizki 25 November 2026; migration **dibuat dan diterapkan Rizki sendiri** |
| Klasifikasi | `STANDARD` + perubahan schema (dua kolom nullable) |
| Task mode | `BACKEND` — perintah Rizki 25 November 2026 "lanjut langkah berikutnya" |
| Target tulis | `MasterData/ChartOfAccount/` (enum, model, service, DTO, controller), `AccChartOfAccountConfiguration.cs`; laporan ini; baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `0f67c5c2` (branch `rizkiG`) + perubahan `BE-ACC-FIN-001-01` yang belum di-commit |
| Tanggal | 25 November 2026 |
| Status | ✅ **SELESAI (development)** — AC-1..AC-10 `RUNTIME VERIFIED` 26 November 2026: uji API K0–K12 12/13 + uji ulang log K11b 1/1 sesudah perbaikan (K11 awal FAIL, lihat bagian 5). Migration diterapkan Rizki sesudah koreksi sapuan HR; build `PASS` (DLL 26 November 2026 09:30:58 WIB); QBE Strict `PASS`. UAT belum dijalankan — milik tim UAT |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `MasterData/ChartOfAccount` |
| Pemilik / prefix | Rizki / `Acc` — registry backend baris 42, `ACTIVE` |
| Keberlakuan | `NEW CODE` (tiga enum, `AccChartOfAccountClassification`, `AccChartOfAccountService.Classification.cs`, DTO baru) dan `TOUCHED LEGACY` (`AccChartOfAccount`, konfigurasinya, `GetPagedAsync`, `GetTreeAsync`, `CreateAsync`, `UpdateAsync`, `PetakanRincianAsync`, controller) |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-CFG-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-TXN-001`, `QBE-LOG-001`, `QBE-AUD-001` |
| Tidak berlaku | `QBE-MOD-002/003` (prefix `Acc` sudah `ACTIVE`, tanpa entity baru), `QBE-PAGE-001` (bentuk `PagedResult` lama) |
| Role access | Nol action baru. `PUT` tetap `[AccessPermission("ChartOfAccount", "Update")]`, `POST` `Create`. Nol hardcode peran |

## 1. Masalah yang diperbaiki

Bagan akun tidak membedakan aset/liabilitas lancar dan tidak lancar, maupun komponen kas, piutang, dan
persediaan. Akibatnya rasio likuiditas (`ACC-DEC-161`) tidak dapat dihitung tanpa menebak dari nama akun,
dan cara menebak itu dilarang (`ACC-DEC-168`).

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Setiap akun Aset/Liabilitas punya klasifikasi laporan, cukup diisi di induknya dan diwariskan ke turunan |
| Pelaku | Pemegang `ChartOfAccount : Create` (isi saat tambah) dan `ChartOfAccount : Update` (ubah) |
| Pemicu | Form Tambah/Perbarui Akun (`FE-ACC-FIN-001-02`); atau saringan "Belum lengkap" di Daftar COA |
| Langkah | 1) Kirim kelompok `classification` pada `POST`/`PUT`. 2) Backend memeriksa isian (jenis berlaku, nilai dikenal). 3) Di dalam kunci klasifikasi badan hukum, backend memeriksa konsistensi turunan. 4) Simpan; log nilai lama → baru |
| Jalur tidak normal | Klasifikasi pada Ekuitas/Pendapatan/Beban, sub-klasifikasi pada non-Aset, sub-klasifikasi pada akun tidak Lancar → `400`; perubahan atau pindah induk yang membuat turunan bersub-klasifikasi kehilangan Lancar → `409` menyebut kodenya |
| Hasil | Respons daftar, rincian, pohon, dan pratinjau membawa klasifikasi milik akun dan klasifikasi efektifnya |

**Contoh.** 1-1000 diisi Lancar · Kas dan Setara Kas → 1-1004 Bank Operasional (kosong) efektif Lancar · Kas,
"sumber 1-1000". 1-2001 Piutang Pasien Umum dengan 1-2000 kosong → `ClassificationGap = 1` (belum lengkap).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Enums/AccountReportClassification.cs`, `CurrentAssetCategory.cs`, `ClassificationGapKind.cs` | **Baru** |
| `Models/AccChartOfAccount.cs` | + `ReportClassification`, `CurrentAssetCategory` (nullable) |
| `Repositories/Configurations/.../AccChartOfAccountConfiguration.cs` | + dua properti `HasConversion<int>()`, `IsRequired(false)`, tanpa default, tanpa index |
| `Services/AccChartOfAccountClassification.cs` | **Baru**, static: `MuatAsync`, `Hitung`, `HitungSatu`, `BerlakuUntuk`; kelas `AkunKlasifikasi`, `KlasifikasiEfektif` |
| `Services/AccChartOfAccountService.Classification.cs` | **Baru**: `PeriksaIsianKlasifikasi` (aturan 1–3), `PeriksaKonsistensiKlasifikasiAsync` (aturan 4–5), `KunciKlasifikasiAsync`, pengisi field efektif daftar/rincian/pohon, `PerubahanKlasifikasi` + `PerubahanKlasifikasiTerakhir` |
| `Services/AccChartOfAccountService.cs` | `GetPagedAsync`: proyeksi + field efektif, saringan `UnclassifiedOnly` (wajib badan hukum, disaring dan dihalamankan di memori). `GetTreeAsync`: field klasifikasi per simpul. `CreateAsync`: periksa isian sebelum transaksi; kunci + konsistensi di dalam transaksi; simpan kolom. `UpdateAsync`: bila `Classification` dikirim atau induk berganti → transaksi (`await using`), kunci, konsistensi; kolom ganti penuh hanya bila dikirim. `PetakanRincianAsync`: field milik + efektif + akun sumber |
| `Services/AccChartOfAccountService.Code.cs` | Pratinjau `next-code` + `InheritedReportClassification`, `InheritedCurrentAssetCategory`, `InheritedFromAccountCode` (menutup delta `001-01`) |
| `DTOs/ChartOfAccountDtos.cs` | + `ChartOfAccountClassificationRequest`; `Classification` pada `Create`/`UpdateChartOfAccountRequest`; `UnclassifiedOnly`; field klasifikasi pada `List`/`Detail`/`TreeResponse`; field warisan pada `NextCodeResponse` |
| `Controllers/ChartOfAccountController.cs` | `Create`/`Update` menulis log `ChartOfAccount.Classification` (Information bila berubah; Warning bila ditolak sesudah klasifikasi dievaluasi) |

Nol baris komentar baru. Nol action, hak, atau baris `Program.cs` baru.

### 3.2 Delta terhadap desain

| Hal | Desain | Implementasi | Alasan |
| --- | --- | --- | --- |
| Pembawa nilai sebelum/sesudah ke controller | "Hasil service membawa nilai sebelum dan sesudah" | Properti `PerubahanKlasifikasiTerakhir` pada service (scoped per permintaan) | Mengubah tipe kembalian `CreateAsync`/`UpdateAsync` menuntut membungkus belasan `return` lama; service hanya dipanggil controller ini |
| Log klasifikasi pada `POST` | Hanya `PUT` disebut eksplisit di kontrak permission | Juga `POST` yang membawa klasifikasi tidak kosong | Sesuai `02-backend-architecture.md` bagian 26.10 |

### 3.3 Migration — belum dibuat (wewenang Rizki)

| Butir | Isi |
| --- | --- |
| Nama | `AddChartOfAccountReportClassification` |
| Isi `Up` yang diharapkan | **Hanya** dua `AddColumn<int>` nullable pada `AccChartOfAccount`: `ReportClassification`, `CurrentAssetCategory`; tanpa `defaultValue`, tanpa index |
| Isi `Down` | Dua `DropColumn` |
| Bahaya bila model modul lain tersapu | Catatan 19 November 2026 (tabel HR ikut `AddAssetMovement`). Bila berkas migration memuat apa pun selain dua kolom ini, **jangan diterapkan**; laporkan |
| Bahaya bila DLL baru jalan tanpa migration | Setiap query `AccChartOfAccount` gagal `42703 column ... does not exist` — layar Accounting, jurnal, dan laporan ikut gagal |

**Kejadian 25 November 2026 ~16.15 WIB.** `migrations add` (Rizki) menghasilkan `20261125091331_AddChartOfAccountReportClassification` yang memuat **lima operasi HR** selain dua kolom kita: kolom `HrdEmploymentLifecycleSetting.NormalRetirementAge`, empat kolom `HrdContractDecision.NoticeEvidence*`, dan tabel `HrdAssignmentLetter` + `HrdAssignmentLetterParticipant` beserta index-nya. `database update` gagal `42701 column "NormalRetirementAge" ... already exists`, karena satu migration dijalankan dalam satu transaksi.

| Butir | Temuan |
| --- | --- |
| Sebab | Tiga migration HR `20261120090000_AddHrdRetirementPolicyAge`, `20261120092000_AddHrdContractDecisionNoticeEvidence`, `20261120093000_AddHrdAssignmentLetter` (masuk lewat commit `b9a9ae08`) ditulis tangan sebagai SQL mentah `IF NOT EXISTS`, **tanpa** berkas `Designer` dan **tanpa** memperbarui `ApplicationDbContextModelSnapshot.cs`. DB sudah memuat perubahannya, tetapi snapshot tidak, sehingga EF menganggapnya baru |
| Tindakan agent | `Up` dan `Down` migration kita dipangkas menjadi dua kolom `AccChartOfAccount` saja. `Down` lama juga akan **menghapus tabel dan kolom HR** bila di-rollback — ikut dibuang. Snapshot hasil regenerasi dipertahankan: diff-nya hanya tambahan (342 baris, nol hapus) yang memuat blok HR yang selama ini hilang plus dua kolom kita, sehingga migration berikutnya tidak menyapu HR lagi |
| Yang tidak disentuh | Ketiga migration HR, model HR, dan DB |
| Temuan untuk pemilik HR (Hamzah) | Migration SQL mentah tanpa pembaruan snapshot akan terus muncul di migration modul lain sampai snapshot diperbarui; kini sudah terbarui lewat migration ini |

## 4. Dokumentasi endpoint

| Method | Path | Perubahan | Hak |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/accounting/master-data/chart-of-accounts?LegalEntityId=&UnclassifiedOnly=true` | + saringan dan field klasifikasi | `ChartOfAccount : Read` |
| `GET` | `.../chart-of-accounts/{id}` | + field milik, efektif, akun sumber | `ChartOfAccount : Read` |
| `GET` | `.../chart-of-accounts/tree` | + field klasifikasi per simpul | `ChartOfAccount : Read` |
| `GET` | `.../chart-of-accounts/next-code` | + klasifikasi warisan | `ChartOfAccount : Create` |
| `POST` | `.../chart-of-accounts` | + `classification` | `ChartOfAccount : Create` |
| `PUT` | `.../chart-of-accounts/{id}` | + `classification` (tidak dikirim = tidak berubah) | `ChartOfAccount : Update` |

## 5. Verifikasi

| Perintah atau skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` (11 berkas) | `Files evaluated: 11`, `VIOLATION: 0`, `REVIEW: 0`, `Final result: PASS` | `PASS` |
| Pemindaian baris komentar baru | 0 (tiga baris lama dari `001-01` hanya bergeser) | `PASS` |
| `dotnet ef migrations add` | Dijalankan Rizki; hasil awal tersapu lima operasi HR, dipangkas agent (bagian 3.3) | `PASS` sesudah koreksi |
| `dotnet ef database update` | Percobaan 1 gagal `42701` (dibatalkan transaksi). Percobaan 2 sesudah koreksi dan build berhasil (Rizki, 25 November 2026); terbukti K0: daftar akun membawa `reportClassification`, `classificationGap` | `PASS` |
| `dotnet build -p:RunAnalyzers=false` | Rizki; DLL 25 November 2026 16:21:10 WIB, sesudah koreksi migration 16:18:04 | `PASS` |
| `fin-001-02-20261125/01-uji-api.mjs` K0–K12 (26 November 2026 09.18:41–09.18:57 WIB) | 12 PASS, 1 FAIL (K11) | Lihat bawah |
| Perbaikan log (controller) | QBE Strict 1 berkas `PASS`; build Rizki DLL 26 November 2026 09:30:58 WIB, sesudah suntingan 09:19:44 | `PASS` |
| `fin-001-02-20261125/02-uji-log.mjs` K11b (26 November 2026 ±09.32 WIB) | 1/1 — tiga baris log persis: INF "Klasifikasi laporan akun diubah. Akun 1-5000: klasifikasi kosong -> Lancar, sub-klasifikasi kosong -> Kas dan Setara Kas."; WRN "… Ditolak: Akun UJI-PANJANG-12345678: klasifikasi kosong -> Tidak Lancar, sub-klasifikasi kosong -> Persediaan."; INF pengembalian Lancar -> kosong. `PUT` dengan nilai sama tidak menulis log. Klasifikasi 1-5000 kembali kosong | `PASS` |
**Hasil uji utama (26 November 2026).**

| Uji | Hasil |
| --- | --- |
| K0 | Migration terbaca: 47 akun, field klasifikasi ada di respons |
| K1 | `UnclassifiedOnly` hanya akun penerima transaksi Aset/Liabilitas ber-`ClassificationGap = 1`, memuat 1-5001, tanpa 1-5000 (induk) maupun 4-1002 (Pendapatan); tanpa badan hukum `400` dengan pesan kontrak |
| K2 | `PUT` 1-5000 {1,1} → 9901.01 (dua tingkat di bawah) efektif [1,1] bersumber 1-5000, gap `null` |
| K3 | Sesudah K2, 1-5001 dan 9901.0x keluar dari saringan |
| K4 | Empat penolakan `400` dengan pesan kontrak; nol tulis pada akun asli (4-1002, 2-1001) |
| K5 | 9901.01 menyimpan Kas; 1-5000 → Tidak Lancar `409` "… turunan 9901.01 memakai sub-klasifikasi aset lancar …"; pindah 9901 ke induk Tidak Lancar `409`; 1-5000 dan induk 9901 tidak berubah |
| K6 | 1-5001 Tidak Lancar → efektif [2,null], sumber dirinya; 9901.01 tetap [1,1] |
| K7 | `PUT` tanpa `classification` tidak mengubah; berisi `null` mengosongkan → ikut 1-5000 |
| K8 | Pratinjau di bawah 1-5000: warisan [1,1] dari 1-5000; simpul pohon 9901.01 membawa klasifikasi efektif |
| K9 | Akun penyetuju uji → `403` |
| K10 | Draft `JU/2026/11/00001` ke akun Aset belum lengkap → `201`, lalu dihapus `200` |
| K11 | **FAIL** — 7 baris log `ChartOfAccount.Classification` tercatat (INF saat berubah, WRN saat ditolak), tetapi tanpa kode akun dan nilai lama → baru |
| K12 | Klasifikasi akun uji dikosongkan kembali; tetap nonaktif |

**Temuan K11.** `Services/Logging/LoggerService.WriteAsync` (platform) tidak menulis objek muatan ke log; ia hanya memungut `Path`, `Method`, `Ip`, `UserId`/`Id`, `UserName`, `Email` darinya. Akibatnya log `ChartOfAccount.Create`/`Update` lama pun tidak pernah membawa muatan. Perbaikan dalam lingkup task: controller memasukkan rincian ke teks pesan — "Klasifikasi laporan akun diubah. Akun 1-5000: klasifikasi kosong -> Lancar, sub-klasifikasi kosong -> Kas dan Setara Kas." — dan untuk penolakan "{alasan} Ditolak: Akun …". `LoggerService` tidak disentuh.

**Rencana uji.** Penulisan klasifikasi hanya pada akun uji nonaktif di grup `1-5000` (dari `001-01`); akun asli
hanya menerima permintaan yang pasti ditolak `400` (nol tulis); satu draft jurnal uji dibuat lalu dihapus
(`F41`); klasifikasi akun uji dikosongkan kembali di akhir (K12).

## 6. Acceptance criteria

| Kriteria | Status | Rencana bukti |
| --- | --- | --- |
| AC-1 Migration hanya dua kolom nullable | `RUNTIME VERIFIED` | Berkas sesudah koreksi hanya dua `AddColumn<int>`/`DropColumn`; K0 |
| AC-2 Pewarisan 1-5000 → 9901.01 (`F34`) | `RUNTIME VERIFIED` | K2 |
| AC-3 `VAL-COA-08`..`10` → `400` (`F35`) | `RUNTIME VERIFIED` | K4 |
| AC-4 `VAL-COA-11` → `409` pada ubah klasifikasi dan pindah induk (`F36`) | `RUNTIME VERIFIED` | K5 |
| AC-5 Anak menimpa (`F37`) | `RUNTIME VERIFIED` | K6 |
| AC-6 Tanpa `classification` tidak berubah; berisi `null` mengosongkan | `RUNTIME VERIFIED` | K7 |
| AC-7 Log nilai lama → baru hanya bila berubah (`F39`) | `RUNTIME VERIFIED` sesudah perbaikan (K11 awal FAIL) | K11b |
| AC-8 `UnclassifiedOnly`; tanpa badan hukum `400` | `RUNTIME VERIFIED` | K1, K3 |
| AC-9 Jurnal ke akun belum lengkap tetap diterima (`F41`) | `RUNTIME VERIFIED` | K10 |
| AC-10 `PUT` klasifikasi tanpa `Update` → `403` (`F40`) | `RUNTIME VERIFIED` | K9 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko | (1) Migration tersapu model lain — diperiksa sebelum diterapkan. (2) `UnclassifiedOnly` memuat seluruh akun badan hukum ke memori — wajar untuk ratusan akun. (3) Field efektif pada daftar berhalaman menambah satu query ringkasan akun per badan hukum yang tampil |
| Status Git | `M` model, konfigurasi, service, DTO, controller; `??` tiga enum, `AccChartOfAccountClassification.cs`, `AccChartOfAccountService.Classification.cs`; laporan ini |
| Langkah berikutnya | `FE-ACC-FIN-001-02` (klasifikasi di form, rincian, daftar) |
| Temuan platform, tidak diubah | `LoggerService.WriteAsync` tidak menulis objek muatan; properti `Id`/`Name` pada muatan menimpa identitas pengguna login. Log lama yang mengandalkan muatan (mis. `ChartOfAccount.Create/Update`) tidak membawa isinya |
