# Laporan Perubahan Backend — `BE-ACC-P2-032`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-032` |
| Judul | Penjaga penyusunan jurnal penutup tahun bersamaan |
| Slice | `HARDENING` — batch 14 September 2026 |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-032` (revisi 3) |
| Trace | `FR-P2-033`; `ACC-DEC-079`; `ACC-TD-024` |
| Contract version | `ACC-API-0.10` — **tidak berubah**; `POST /year-end-closing/generate` tetap `409` |
| Dependency | `BE-ACC-P2-010` ✅ |
| Klasifikasi | `LIGHT` — skor 3: repository 0, berkas diperiksa 0, berkas diubah 0 (1 berkas), logika bisnis 1 (konkurensi), kontrak API 1, database 1 (perilaku persistence, tanpa skema), keamanan 0, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `AccYearEndClosingService`, laporan ini, baris status roadmap, traceability, register `ACC-TD-024` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `b3ab542e` (branch `rizkiG`), perubahan belum di-commit |
| Tanggal | 14 September 2026 |
| Status | **✅ SELESAI** — 5 dari 5 acceptance terpetakan ke source; build owner `0 error` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `AccountingPeriod` |
| Registry | `Acc` — `ACTIVE`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 31 (salinan suite 1.17.1 tidak memuatnya — `ACC-TD-015`) |
| Keberlakuan | `TOUCHED LEGACY` — service `BE-ACC-P2-010` |
| QBE yang berlaku | `QBE-TXN-001` (kunci dan pembuatan jurnal dalam satu transaction), `QBE-CODE-003` (penjaga bukan lock process-local), `QBE-API-001` (`409` yang sudah ada) |

---

## 1. Masalah yang diperbaiki

`BE-ACC-P2-010` sudah menolak penyusunan jurnal penutup kedua untuk tahun buku yang sama — **bila
permintaannya berurutan**. Pemeriksaan "apakah jurnal penutup tahun ini sudah ada" tidak mengunci
apa pun, sehingga dua permintaan yang tiba bersamaan pada koneksi berbeda sama-sama melihat "belum
ada", lalu sama-sama membuat jurnal penutup.

**Akibat bila terjadi.** PT Metropolitan Medical Centre tahun buku 2026 berlaba Rp 850.000.000.
Dua jurnal penutup terbentuk; bila keduanya disahkan, laba ditahan bertambah Rp 1.700.000.000 dan
seluruh akun pendapatan dan beban bersaldo terbalik — neraca tahun berikutnya dibuka dengan angka
salah.

`ACC-DEC-079` memutuskan penjaganya **advisory transaction lock**, tanpa kolom maupun migration,
karena "satu jurnal `JT` per badan hukum per tahun buku" tidak dapat diungkapkan unique constraint
pada skema `AccJournal` yang ada.

---

## 2. Proses bisnis

| Langkah | Pelaku | Yang terjadi |
| ---: | --- | --- |
| 1 | Accounting Manager | Membuka layar Tutup Tahun, memilih badan hukum dan tahun buku, menekan Susun Jurnal Penutup |
| 2 | Sistem | Memeriksa jenis jurnal `JT` dan menyusun rencana baris penutup |
| 3 | Sistem | Membuka transaction, **mengambil kunci `ACC_YEAR_END_{badan hukum}_{tahun}`** |
| 4 | Sistem | Memeriksa jurnal penutup yang sudah ada — **sesudah** kunci didapat |
| 5 | Sistem | Membuat jurnal `Draft`, commit; kunci lepas bersama berakhirnya transaction |

**Jalur tidak normal — dua permintaan bersamaan.**

| Waktu | Permintaan A | Permintaan B |
| --- | --- | --- |
| 10:00:00.000 | Mengambil kunci `ACC_YEAR_END_…_2026` — berhasil | — |
| 10:00:00.050 | Memeriksa: belum ada jurnal penutup | Mengambil kunci yang sama — **menunggu** |
| 10:00:00.300 | Membuat `JT/2026/12/00001`, commit, kunci lepas | Masih menunggu |
| 10:00:00.301 | — | Kunci didapat; memeriksa: **sudah ada** `JT/2026/12/00001` |
| 10:00:00.310 | `201` | `409` "Jurnal penutup tahun buku 2026 sudah pernah disusun dengan nomor JT/2026/12/00001 berstatus Draft. …" |

**Yang tidak saling menunggu.** Kuncinya ber-scope badan hukum **dan** tahun buku. Penutupan 2026
PT Metropolitan Medical Centre tidak menahan penutupan 2025, maupun penutupan 2026 badan hukum lain.

**Penyedia selain PostgreSQL.** `pg_advisory_xact_lock` hanya ada di PostgreSQL, jadi langkah 3
dilewati bila `Database.IsNpgsql()` bernilai salah — pola `AccJournalService`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu roadmap `BE-ACC-P2-032`; `ACC-DEC-079`; `UTANG-TEKNIS.md` `ACC-TD-024`;
`AccYearEndClosingService.cs` (`GenerateAsync` baris 159–271); `AccJournalService.cs` baris
1333–1341 sebagai pola kunci.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/AccountingManagement/AccountingPeriod/Services/AccYearEndClosingService.cs` | Di dalam `try` sesudah `BeginTransactionAsync`, sebelum `CariJurnalPenutupAsync`: `if (_db.Database.IsNpgsql())` → `ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));", [$"ACC_YEAR_END_{LegalEntityId:N}_{FiscalYear}"])`. Komentar `remarks` diperbarui — kalimat "risiko tersisa" lama diganti penjelasan kunci |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — kalimat dan status `409` yang sudah ada dipakai apa adanya |
| Database | Nol entity, nol kolom, nol migration. Hanya advisory lock tingkat transaction yang lepas sendiri |
| Keamanan/Auth | `NOT APPLICABLE` |

---

## 4. Dokumentasi endpoint

#### Corporate / Accounting / Year End Closing

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/accounting/year-end-closing/generate` | Menyusun jurnal penutup tahun sebagai `Draft`. **Tidak berubah**; permintaan bersamaan kini hanya menghasilkan satu jurnal | `YearEndClosing : Generate` — tidak berubah |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=false` oleh Rizki, 14 Sep 2026 | `0 error`, 189 warning | `PASS` | Tangkapan layar owner. Asal kenaikan dari 145 belum dipastikan — lihat `BE-ACC-P2-033` bagian 5 |
| Pemeriksaan source — letak kunci | Baris 198–204, di dalam `try` sesudah `BeginTransactionAsync` (baris 186–189) | `PASS` | `AccYearEndClosingService.cs` |
| Pemeriksaan source — urutan | `CariJurnalPenutupAsync` di baris 206, sesudah kunci | `PASS` | Idem |
| Pemeriksaan source — satu transaction dengan pembuatan jurnal | `_journalService.CreateAsync` (baris 244) memakai transaction yang sedang terbuka; commit di baris 252 | `PASS` | Idem |
| Pemeriksaan source — kunci ter-scope | Kunci memuat `LegalEntityId:N` dan `FiscalYear` | `PASS` | Idem |
| Uji dua permintaan bersamaan pada PostgreSQL | Tidak dijalankan | `NOT RUN` | Tidak diminta kolom Verifikasi; automated test dilarang `ACC-DEC-081` |

Uji manual: `NOT APPLICABLE` — kolom Verifikasi kartu hanya meminta pemeriksaan source dan build.
Pembuktian runtime dua permintaan bersamaan diserahkan ke tim UAT bila diperlukan.

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria persis seperti roadmap | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Kunci diambil di dalam transaction yang sama dengan pembuatan jurnal | **Terpenuhi** | Baris 186–204 dan 244–252 |
| 2 | Pemeriksaan "jurnal penutup sudah ada" dijalankan **sesudah** kunci didapat | **Terpenuhi** | Baris 206 |
| 3 | Permintaan kedua menerima `409` dengan pesan yang sudah ada | **Terpenuhi** | Baris 209–219, kalimat tidak berubah dari `BE-ACC-P2-010` |
| 4 | Pada penyedia non-PostgreSQL kunci dilewati, mengikuti pola yang sudah ada | **Terpenuhi** | `if (_db.Database.IsNpgsql())` baris 198 |
| 5 | Nol kolom baru, nol migration | **Terpenuhi** | `git status` — nol berkas `Migrations/`, nol entity berubah |

| Butir DoD | Hasil |
| --- | --- |
| Source berubah | **Ya** |
| Laporan task tertulis | **Ya** — berkas ini |
| Register `ACC-TD-024` diperbarui | **Ya** — `CLOSED` 14 September 2026 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 189 warning solution — lihat `BE-ACC-P2-033` bagian 5 |
| Masalah yang diketahui | Rencana baris penutup (`SusunRencanaAsync`, baris 178) dihitung **sebelum** kunci. Permintaan kedua yang menunggu tetap ditolak `409`, jadi tidak ada jurnal ganda; kalimat ini dicatat hanya supaya tidak dikira rencana dihitung ulang di bawah kunci |
| Risiko tersisa | Penjaga hanya berlaku bagi jalur yang mengambil kunci yang sama. Jalur baru yang menyusun jurnal `JT` tanpa `GenerateAsync` tidak terjaga — sesuai batas `ACC-DEC-079` yang memilih tanpa skema |
| Perubahan sampingan | `NONE` |
| Interupsi | Source ditulis sesi 14 September 2026 sebelumnya; sesi ini memverifikasi diff tanpa menyunting ulang |
| Status Git | Lihat [`BE-ACC-P2-033`](BE-ACC-P2-033.md) bagian 7 — batch yang sama. Berkas milik task ini: `AccYearEndClosingService.cs`. **Nol commit, push, stage, merge, rebase, atau migration** |
| Langkah berikutnya | Tidak ada lanjutan wajib untuk task ini |
