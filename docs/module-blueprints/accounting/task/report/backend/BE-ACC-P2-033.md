# Laporan Perubahan Backend — `BE-ACC-P2-033`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-033` |
| Judul | Nominal tidak masuk log |
| Slice | `HARDENING` — batch 14 September 2026 |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-033` (revisi 3) |
| Trace | `NFR-004`; `ACC-PERMISSION` bagian 4; `ACC-TD-023` |
| Contract version | `ACC-API-0.10` — **tidak berubah**; `ACC-PERMISSION-0.5` |
| Dependency | — |
| Klasifikasi | `LIGHT` — skor 2: repository 0, berkas diperiksa 0, berkas diubah 0 (2 berkas), logika bisnis 0, kontrak API 1 (memakai kontrak yang ada), database 0, keamanan 1 (privasi log, bukan inti auth), UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — dua controller Accounting, laporan ini, baris status roadmap, traceability, register `ACC-TD-023` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `b3ab542e` (branch `rizkiG`), perubahan belum di-commit |
| Tanggal | 14 September 2026 |
| Status | **✅ SELESAI** — 4 dari 4 acceptance terpetakan ke source; build owner `0 error` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `JournalManagement` dan `MasterData/ChartOfAccount` |
| Registry | `Acc` — `ACTIVE`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 31. Salinan suite skill 1.17.1 **tidak** memuat `Acc` — selisih lama `ACC-TD-015`; source repository yang berlaku |
| Keberlakuan | `TOUCHED LEGACY` — mengubah pencatat pada controller yang sudah ada |
| QBE yang berlaku | `QBE-LOG-001` (log tetap menyertakan aktor), `QBE-AUD-001` (log aplikasi terpisah dari audit database), `QBE-API-001` (respons tidak berubah) |
| Governance terbaca | `AGENTS.md` backend, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, registry backend, `rules/backend/` suite 1.17.1 — seluruhnya terbaca |

---

## 1. Masalah yang diperbaiki

`NFR-004` melarang nilai uang masuk catatan log. Dua controller Accounting mencatat pesan hasil
layanan **apa adanya**, padahal beberapa pesan penolakan memuat nominal. Contoh nyata dari
`AccJournalService`:

> "Jurnal belum seimbang. Total debit Rp 4.500.000, total kredit Rp 4.000.000, selisih Rp 500.000."

Pesan itu benar dan perlu bagi petugas yang sedang memperbaiki jurnal. Masalahnya, kalimat yang
sama ikut tertulis ke log aplikasi, sehingga siapa pun yang dapat membaca log — tim infrastruktur,
pembaca dasbor log — melihat angka keuangan rumah sakit tanpa pernah punya hak `Journal : Read`.

---

## 2. Proses bisnis

| Langkah | Pelaku | Yang terjadi |
| ---: | --- | --- |
| 1 | Petugas akuntansi | Mengajukan jurnal yang belum seimbang |
| 2 | Sistem | Menolak `400` dengan pesan lengkap beserta angkanya |
| 3 | Sistem | Mencatat kejadian penolakan ke log, **dengan angka diganti `***`** |
| 4 | Petugas akuntansi | Membaca pesan lengkap di layar, memperbaiki selisih Rp 500.000 |

| Yang dilihat | Isi |
| --- | --- |
| Layar petugas (`ApiResponse.Message`) | "Jurnal belum seimbang. Total debit Rp 4.500.000, total kredit Rp 4.000.000, selisih Rp 500.000." |
| Log aplikasi | "Jurnal belum seimbang. Total debit Rp *** total kredit Rp *** selisih Rp ***" |

Koma setelah angka ikut terbuang karena pola `Rp\s?[\d.,]+` menangkap titik dan koma. Kalimatnya
tetap dapat dibaca dan sebab kegagalannya tetap dapat ditelusuri; yang hilang hanya nilainya.
Pola ini sama persis dengan `RecurringJournalController` dan `YearEndClosingController`, jadi
seluruh pencatat Accounting kini berperilaku seragam.

**Jalur tidak normal.** Pesan tanpa kata `Rp` tidak berubah sama sekali. Angka yang ditulis dengan
format budaya berbeda — `4,500,000` atau `4.500.000` — tetap tertangkap, karena pola menerima titik
maupun koma.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu roadmap `BE-ACC-P2-033`; `UTANG-TEKNIS.md` `ACC-TD-023`; `JournalController.cs`;
`ChartOfAccountController.cs`; `RecurringJournalController.cs` dan `YearEndClosingController.cs`
sebagai pola; `AccJournalService.cs` dan `AccChartOfAccountService.cs` untuk seluruh pesan
bernominal (`Rp {…:N0}` di baris 911, 1186–1187, dan 433); `Services/Logging/LoggerService.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/AccountingManagement/JournalManagement/Controllers/JournalController.cs` | `CatatAsync` meneruskan `TanpaNominal(hasil.Message)` ke `LoggerService`; method `private static TanpaNominal` baru. `ToActionResult` tidak disentuh |
| `Areas/Corporate/AccountingManagement/MasterData/ChartOfAccount/Controllers/ChartOfAccountController.cs` | Perubahan yang sama pada `CatatAsync` pencatat daftar akun |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — `ToActionResult` tetap memakai `result.Message` utuh; status kode dan isi respons tidak berubah |
| Database | `NOT APPLICABLE` — nol entity, nol configuration, nol migration |
| Keamanan/Auth | Privasi log membaik: nominal tidak lagi tertulis ke log aplikasi. Hak akses endpoint tidak berubah |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — tidak ada endpoint yang ditambah atau berubah perilakunya. Seluruh endpoint grup
`Corporate / Accounting / Journal Management / Journal` dan daftar akun (`ChartOfAccountController`)
menjawab persis sama seperti sebelumnya.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=false` oleh Rizki, 14 Sep 2026 | `Build succeeded with 189 warning(s) in 150,5s`, `0 error` | `PASS` | Tangkapan layar terminal owner |
| Pemeriksaan source — respons pengguna | `ToActionResult` pada kedua controller memakai `result.Message`, bukan `pesan` tersaring | `PASS` | `JournalController.cs`, `ChartOfAccountController.cs` |
| Pemeriksaan source — seluruh jalur log | `_loggerService.` hanya dipanggil dari `CatatAsync` pada kedua controller; tidak ada jalur log lain yang melewati penyaring | `PASS` | Grep `_loggerService\.` pada `JournalManagement/**` dan `MasterData/ChartOfAccount/**` |
| Pemeriksaan source — sumber nominal | Tiga pesan bernominal (`AccJournalService` 911, 1186–1187; `AccChartOfAccountService` 433) memakai format `Rp {angka:N0}` — seluruhnya cocok pola | `PASS` | Grep `Rp` |
| Warning 189 terhadap garis dasar 145 | Garis dasar 145 diukur 9 Sep 2026, **sebelum** merge `b7ea1ae7` (PR #131, 385 berkas). Asal +44 **belum dapat dipastikan** tanpa daftar warning | `NOT RUN` | Kedua berkas task ini hanya menambah satu method `static` berketik penuh; tidak ada sumber warning nullable yang terlihat |

Uji manual: `NOT APPLICABLE` — perilakunya hanya terlihat di berkas log; pembuktian runtime
diserahkan ke tim UAT bila diperlukan.

**Tidak dijalankan:** automated test — dilarang `ACC-DEC-081`; `dotnet build` oleh agent —
dijalankan owner sendiri.

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria persis seperti roadmap | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Pesan yang diteruskan ke `LoggerService` dari kedua pencatat tidak memuat angka rupiah | **Terpenuhi** | `CatatAsync` kedua controller meneruskan `TanpaNominal(hasil.Message)` |
| 2 | `ApiResponse` kepada pengguna tetap memuat pesan lengkap beserta angkanya | **Terpenuhi** | `ToActionResult` tidak berubah |
| 3 | Status kode dan isi respons endpoint tidak berubah | **Terpenuhi** | Diff hanya menyentuh `CatatAsync` dan menambah `TanpaNominal` |
| 4 | Nol migration | **Terpenuhi** | `git status` — `Migrations/` tidak berubah |

| Butir DoD | Hasil |
| --- | --- |
| Source berubah | **Ya** |
| Laporan task tertulis | **Ya** — berkas ini |
| Register `ACC-TD-023` diperbarui | **Ya** — ditandai `CLOSED` 14 September 2026 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 189 warning solution; asal kenaikan dari 145 belum dipastikan — lihat bagian 5 |
| Masalah yang diketahui | Lihat temuan di bawah |
| Risiko tersisa | Pesan bernominal **baru** yang kelak ditulis dengan format selain `Rp <angka>` — misalnya `IDR 4.500.000` — tidak akan tertangkap pola ini |
| Perubahan sampingan | `NONE` |
| Interupsi | Source ditulis sesi 14 September 2026 sebelumnya; sesi ini memverifikasi diff yang ada tanpa menyunting ulang |
| Status Git | Lihat di bawah |

### Temuan di luar cakupan — dilaporkan, tidak diperbaiki

**Pelaku pada log `ChartOfAccount` dan `JournalType` tercatat keliru.** `LoggerService` baris 76
membaca properti `UserId` **atau `Id`** dari muatan dan memakainya menggantikan pengguna yang
login. `ChartOfAccountController` mengirim muatan `new { id, request }` pada Update dan Deactivate,
serta `new { id }` pada Activate; `JournalTypeController` mengirim `new { id, request }` pada Update.
Akibatnya kolom `UserId` pada log ketiga tindakan itu berisi **id akun atau id jenis jurnal**,
bukan id petugas yang melakukannya — melanggar `QBE-LOG-001`.

Contoh: Budi (`UserId` `7c1e…`) menonaktifkan akun `1-1101 Kas Kasir` (`Id` `a93f…`). Log mencatat
`UserId="a93f…"`, sehingga pertanyaan audit "siapa yang menonaktifkan Kas Kasir" tidak dapat
dijawab dari log. Perbaikannya satu kata per baris: ganti `id` menjadi `EntityId = id`, pola yang
sudah dipakai `JournalController` dan `RecurringJournalController`. Tidak dikerjakan di sini karena
di luar cakupan kartu (hanya `CatatAsync`). **Pemilik: owner modul.**

### Status Git

```text
 M Areas/Corporate/AccountingManagement/AccountingPeriod/Services/AccYearEndClosingService.cs
 M Areas/Corporate/AccountingManagement/JournalManagement/Controllers/JournalController.cs
 M Areas/Corporate/AccountingManagement/JournalManagement/Services/AccJournalService.cs
 M Areas/Corporate/AccountingManagement/MasterData/ChartOfAccount/Controllers/ChartOfAccountController.cs
 M Program.cs
 M Repositories/ApplicationDbContext.cs
 M Repositories/Configurations/Corporate/AccountingManagement/JournalManagement/AccJournalConfiguration.cs
 M docs/module-blueprints/accounting/** (dokumen batch 14 September 2026)
?? Areas/Corporate/AccountingManagement/MasterData/EventType/
?? Areas/Corporate/AccountingManagement/MasterData/PostingRule/
?? Repositories/Configurations/Corporate/AccountingManagement/MasterData/Acc{EventType,PostingRule,PostingRuleLine}Configuration.cs
?? docs/module-blueprints/accounting/task/report/backend/BE-ACC-P2-0{15,17,18,31,32,33}.md
```

Berkas milik task ini: dua controller di baris ke-2 dan ke-4. Sisanya milik `031`, `032`, `015`,
`017`, `018` pada batch yang sama. **Nol commit, push, stage, merge, rebase, atau migration.**

### Langkah berikutnya

1. Owner membaca daftar warning build untuk memastikan asal kenaikan 145 → 189.
2. Owner memutuskan perbaikan pelaku log `ChartOfAccount`/`JournalType` di atas.
