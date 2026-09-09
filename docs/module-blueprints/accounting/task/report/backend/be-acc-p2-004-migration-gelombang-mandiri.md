# Laporan Perubahan Backend — `BE-ACC-P2-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-004` |
| Judul | Migration gelombang mandiri |
| Slice | Menutup `P2-0a`, `P2-3`, `P2-4`, dan bagian `P2-CTRL` |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-004` |
| Trace | `02-backend-architecture.md` bagian 18, rencana migration urutan 1, 3, 4; kamus data bagian 13–19 dan bagian 1 |
| Contract version | Blueprint `ACC-BP-001` revisi 11; roadmap revisi 2 `APPROVED` 9 September 2026 |
| Dependency | `BE-ACC-P2-001` ✅, `002` ✅, `003` ✅, `011` ✅ — **keempatnya selesai sebelum migration dibuat** |
| Klasifikasi | `MEDIUM` — skor 4: repository 0, berkas diperiksa 1, berkas diubah 0 (3 berkas, seluruhnya hasil generator), logika bisnis 0, kontrak API 0, database 2, keamanan 0, UI 0 |
| Task mode | `BACKEND` |
| **Pelaksana** | **Rizki (owner), bukan agent** — lihat bagian Pembagian wewenang |
| Model | Claude Opus 5 — hanya untuk gate, verifikasi, dan laporan |
| Commit backend saat dikerjakan | `cc59164` |
| Tanggal | 9 September 2026 |
| Status | **`DONE`** — migration dibuat **dan diterapkan** ke database dev owner |

## Pembagian wewenang

Kartu ini berstatus **`GATED`**, dan instruksi owner pada 9 September 2026 tegas:
*"untuk migration saya yang lakukan sendiri"*. Pembagiannya:

| Yang dikerjakan | Oleh |
| --- | --- |
| Menyelesaikan `011` sebagai prasyarat, lalu **berhenti** | Agent |
| Menjalankan Migration Coordination Gate dan menyajikan bukti pertanyaan 1–4, 6, 7 | Agent |
| Menjawab pertanyaan 5 (keadaan database) | **Rizki** |
| `dotnet ef migrations add` | **Rizki** |
| Memeriksa hasilnya sebelum diterapkan | Agent, atas permintaan Rizki |
| `dotnet ef database update` | **Rizki** |
| Laporan, penandaan roadmap, dan traceability | Agent |

**Agent tidak pernah menjalankan satu pun perintah `dotnet ef` maupun perintah database pada
task ini.**

---

## 1. Masalah yang diperbaiki

Sesudah `BE-ACC-P2-001`, `002`, `003`, dan `011` selesai, seluruh bentuk Phase 2 gelombang mandiri
sudah berdiri **di model EF** — tetapi tidak satu pun ada di database. Akibatnya dua hal:

1. Tidak ada satu pun fitur Phase 2 yang dapat dijalankan. `005`, `007`, `009`, `012`, dan `013`
   semuanya menunggu tabelnya benar-benar ada.
2. **Lebih mendesak:** kolom `IsControlAccount` ditambahkan ke `AccChartOfAccount`, tabel Phase 1
   yang sudah dipakai. Selama migration belum diterapkan, model EF lebih maju daripada skema,
   sehingga setiap query ke daftar akun menghasilkan `42703: column "IsControlAccount" does not
   exist`. Endpoint daftar akun yang sebelumnya berjalan menjadi `500`.

Migration ini menutup keduanya sekaligus.

---

## 2. Proses bisnis

Migration tidak punya proses bisnis sendiri; ia membuat tempat bagi empat proses yang sudah
dirancang task lain. Yang berubah bagi pengguna: sesudah migration diterapkan, daftar akun kembali
dapat dibuka, dan keempat kemampuan berikut punya tempat penyimpanan yang siap dipakai.

| Kemampuan | Tabel yang berdiri | Task pemakainya |
| --- | --- | --- |
| Penutupan periode empat mata | `AccPeriodClosingApproval` + 2 kolom pada `AccAccountingPeriod` | `005`, `006` |
| Jurnal berulang | `AccRecurringJournalTemplate`, `...Line`, `...Run` | `007`, `008` |
| Tutup tahun | `AccAccountingConfiguration` | `009`, `010` |
| Control account | Kolom `IsControlAccount` pada `AccChartOfAccount` | `012`, `013` |

### Jalur tidak normal yang sengaja dijaga

| Keadaan | Yang terjadi |
| --- | --- |
| Migration dijalankan saat layanan hidup | **Aman.** Seluruhnya tabel baru; kedua kolom periode `nullable`, kolom control account ber-`defaultValue: false` |
| Akun yang sudah ada di daftar akun | Terisi `false` **sendiri** oleh nilai bawaan; nol pengisian data, nol perubahan perilaku |
| Periode yang sudah `SoftClosed` sebelum Phase 2 | Tetap sah tanpa riwayat persetujuan — kedua kolom baru boleh kosong |
| Perlu dibatalkan | `Down` mengembalikan keadaan semula seluruhnya |

---

## 3. Perubahan yang dikerjakan

### 3.1 Migration Coordination Gate

Dijalankan **sebelum** `dotnet ef migrations add`, sesuai
[`06-shared-migration-coordination-rule.md`](../../../06-shared-migration-coordination-rule.md).

| # | Pertanyaan | Jawaban | Bukti |
| ---: | --- | --- | --- |
| 1 | Modul paralel sudah membuat migration? | **Tidak.** Nol migration Finance maupun ARAP | Penelusuran `Migrations/` |
| 2 | Bila sudah, apa namanya? | Migration terakhir dari modul mana pun: `20260907062238_AddTablePettyCashModule` | Nama berkas |
| 3 | Sudah commit dan push? | **Ya** | Ada pada `origin/QuilvianIntegrationBackend` |
| 4 | Sudah merge ke canonical integration baseline? | **Ya** | `rizkiG` **0 commit di belakang** `origin/QuilvianIntegrationBackend` |
| 5 | Sudah diterapkan ke database development? | **Ya** | Dikonfirmasi Rizki, 9 September 2026 |
| 6 | Snapshot lokal berasal dari baseline terbaru? | **Ya** | **0 berkas** pada `Migrations/` berbeda antara `HEAD` dan `origin/QuilvianIntegrationBackend` |
| 7 | SHA baseline sumber migration ini? | `deb089d` (integration); HEAD lokal `cc59164` | `git rev-parse` |

**Gate lulus — tujuh dari tujuh terjawab.**

### 3.2 Berkas yang berubah

Tiga berkas, **seluruhnya hasil generator EF**. Nol berkas disunting tangan.

| Berkas | Perubahan |
| --- | --- |
| `Migrations/20260909060515_AddAccountingPhase2Independent.cs` | **Baru.** 5 `CreateTable`, 3 `AddColumn`, index dan check constraint |
| `Migrations/20260909060515_AddAccountingPhase2Independent.Designer.cs` | **Baru.** Model terkait migration |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Diperbarui — **penambahan saja, nol baris terhapus** |

### 3.3 Isi migration

**Lima tabel baru:**

`AccAccountingConfiguration`, `AccPeriodClosingApproval`, `AccRecurringJournalRun`,
`AccRecurringJournalTemplate`, `AccRecurringJournalTemplateLine`.

**Tiga kolom baru:**

| Kolom | Tabel | Sifat |
| --- | --- | --- |
| `IsControlAccount` | `AccChartOfAccount` | `bool`, `defaultValue: false` |
| `ClosingSubmittedAt` | `AccAccountingPeriod` | `timestamptz`, `nullable: true` |
| `ClosingSubmittedBy` | `AccAccountingPeriod` | `uuid`, `nullable: true` |

**Index unik yang terbawa apa adanya:**

| Index | Filter | Kenapa penting |
| --- | --- | --- |
| `AccAccountingConfiguration (LegalEntityId)` | `"IsDelete" = false` | Satu pengaturan hidup per badan hukum |
| `AccPeriodClosingApproval (AccountingPeriodId, ActionSequence)` | `"IsDelete" = false` | Dua pengajuan bersamaan tidak saling menimpa |
| **`AccRecurringJournalRun (TemplateId, AccountingPeriodId)`** | **TANPA filter** | **Penjaga terbit ganda.** Sengaja berbeda dari yang lain |
| `AccRecurringJournalTemplate (LegalEntityId, TemplateCode)` | `"IsDelete" = false` | Kode template unik per badan hukum |
| `AccRecurringJournalTemplateLine (TemplateId, LineNumber)` | `"IsDelete" = false` | Nomor baris unik per template |

Ditambah index non-unik `IX_AccChartOfAccount_IsControlAccount`, dan dua check constraint:
`CK_AccRecurringJournalTemplate_DayOfMonth_1_28` serta
`CK_AccRecurringJournalTemplateLine_TepatSatuSisiTerisi`.

Perbedaan filter pada penjaga terbit ganda **disengaja** dan sudah dijelaskan pada laporan
`be-acc-p2-002`: menghapus lunak catatan penerbitan lalu menerbitkan ulang akan menghasilkan jurnal
kedua untuk bulan yang sama.

### 3.4 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — nol endpoint, DTO, atau route disentuh |
| Database | **Diterapkan.** 5 tabel dan 3 kolom kini ada di database development owner |
| Keamanan/Auth | `NOT APPLICABLE` |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak membuat maupun mengubah endpoint. Yang berubah adalah
**pemulihan** endpoint daftar akun yang sempat `500` selama model EF lebih maju daripada skema.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Snapshot bertambah tanpa deletion | **0 baris terhapus** | `PASS` | `git diff -- ApplicationDbContextModelSnapshot.cs \| grep -c "^-[^-]"` → `0` |
| Jumlah entity snapshot | **565 → 570**, tepat `+5` | `PASS` | `grep -c 'b.ToTable('` sebelum dan sesudah |
| Tabel `Acc*` pada snapshot | **7 → 12** | `PASS` | Penelusuran `b.ToTable("Acc...")` |
| Isi migration | **5 `CreateTable`, 3 `AddColumn`** | `PASS` | Dicocokkan dengan angka yang diperkirakan sebelum migration dibuat |
| `Down` mengembalikan keadaan semula | 5 `DropTable` + 1 `DropIndex` + 3 `DropColumn`, urutan anak sebelum induk | `PASS` | Pembacaan berkas migration |
| Kolom baru tidak mematikan layanan | Kedua kolom periode `nullable: true`; `IsControlAccount` `defaultValue: false` | `PASS` | Pembacaan berkas migration |
| `dotnet ef database update` | **Berhasil**, dikonfirmasi owner | `PASS` | Dijalankan Rizki, 9 September 2026 |

### `CONTAMINATION GUARD` — `CLEAN`

Jumlah entity snapshot naik **tepat 5**, sama dengan jumlah tabel baru, dan **nol baris terhapus**.
Artinya tidak ada satu pun blok modul lain yang hilang — pola kerusakan `ACC-DEP-001` yang menjadi
baris Risiko pada kartu ini **tidak terjadi**.

### Kegagalan yang terjadi di tengah jalan, dan sebabnya

Percobaan `dotnet ef database update --no-build` yang pertama **gagal**:

```
PendingModelChangesWarning: The model for context 'ApplicationDbContext' has pending changes.
```

**Sebabnya instruksi agent yang keliru, bukan cacat migration.** `dotnet ef migrations add
--no-build` menulis berkas `.cs` tetapi **tidak mengompilasinya**, sehingga
`bin/Debug/net9.0/QuilvianSystemBackend.dll` masih memuat snapshot versi lama. Terbukti dari
stempel waktu: DLL `13:04:35`, berkas migration `13:05:15` — DLL **40 detik lebih tua**.

Perbaikannya membangun ulang lebih dahulu, lalu `database update` berhasil. **Nol perubahan pada
berkas migration.** Dicatat di sini supaya kekeliruan urutan perintah yang sama tidak berulang:
`--no-build` aman pada `migrations add`, tetapi **tidak** pada perintah sesudahnya.

**Tidak dijalankan:**

- Uji integrasi PostgreSQL terhadap tabel baru — belum ada perilaku yang diuji; pengujiannya
  melekat pada `005` sampai `013`.
- Penerapan ke database selain milik owner — di luar wewenang; deployment tetap terpisah.
- Pengisian jenis jurnal `JT` lewat endpoint `seed` — langkah data tersendiri, belum dijalankan.

Uji manual: `NOT APPLICABLE`.

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria persis seperti roadmap | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Snapshot bertambah **tanpa satu pun deletion** | **Terpenuhi** | 0 baris terhapus; 565 → 570 entity |
| 2 | Dapat dijalankan tanpa mematikan layanan — seluruhnya tabel baru dan kolom nullable | **Terpenuhi** | 5 tabel baru; 2 kolom `nullable: true`; 1 kolom `defaultValue: false` |
| 3 | `Down` mengembalikan keadaan semula | **Terpenuhi** | 5 `DropTable`, 1 `DropIndex`, 3 `DropColumn` |
| 4 | `CONTAMINATION GUARD` `CLEAN` | **Terpenuhi** | Kenaikan entity tepat `+5`, nol modul lain tersentuh |

**Empat dari empat terpenuhi.**

### Definition of Done

| Butir | Hasil |
| --- | --- |
| Migration dibuat **dan diterapkan owner** | **Ya** — keduanya oleh Rizki, 9 September 2026 |
| Snapshot 0 deletion | **Ya** |
| Laporan tracked ada | **Ya** — berkas ini |
| Roadmap ditandai | **Ya** |
| `requirement-traceability-phase2.md` diperbarui | **Ya** |

**Nol butir DoD dikecualikan.**

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `HostAbortedException: The host was aborted` muncul pada setiap perintah `dotnet ef`. **Normal**, bukan kegagalan — tool sengaja membangun service provider lalu membatalkan host-nya |
| Perubahan sampingan | `NONE` — nol berkas di luar `Migrations/` tersentuh |
| Interupsi | Satu percobaan `database update` gagal karena urutan perintah; dipulihkan dengan membangun ulang. Nol berkas migration diubah |

### Masalah yang diketahui

| # | Isu | Pemilik |
| ---: | --- | --- |
| 1 | **Jenis jurnal `JT` belum ada di database.** Seeder belum dijalankan; `BE-ACC-P2-010` menuntut barisnya benar-benar ada | Rizki |
| 2 | **Penanda control account belum berakibat apa pun.** Kolomnya sudah ada di database, tetapi jurnal manual ke control account **masih diterima** sampai `BE-ACC-P2-012` berdiri | Rizki |
| 3 | Delta `Cascade` lawan `Restrict` dari `BE-ACC-P2-001` — **kini sudah terlanjur diterapkan sebagai `Restrict`**. Mengubahnya sesudah ini menuntut migration kedua | Rizki |
| 4 | `SwaggerDocumentationTests` tidak dapat lulus pada Release | Owner Backend |
| 5 | Selisih dua salinan registry (`ACC-DEP-007`) | Lead |

### Risiko tersisa

| Risiko | Penjelasan |
| --- | --- |
| **Migration belum di-commit** | Ketiga berkas masih di working tree. Selama belum masuk `origin/QuilvianIntegrationBackend`, developer modul lain yang membuat migration berikutnya akan bekerja dari baseline yang tidak memuat 5 tabel ini — dan snapshot-nya berpeluang berselisih saat merge. **Ini jendela risiko `ACC-DEP-001`, dan ia terbuka sekarang** |
| **Database dev lebih maju daripada baseline** | `__EFMigrationsHistory` owner kini memuat migration yang belum ada di integration. Wajar, tetapi perlu diingat saat menggabungkan cabang |
| Butir 3 di atas | Keputusan `Restrict` kini melekat di skema, bukan lagi hanya di source |

### Status Git

```text
 M Migrations/ApplicationDbContextModelSnapshot.cs
?? Migrations/20260909060515_AddAccountingPhase2Independent.cs
?? Migrations/20260909060515_AddAccountingPhase2Independent.Designer.cs
```

Ditambah berkas `BE-ACC-P2-003` dan `011` yang juga belum di-commit.
**Nol commit, push, stage, merge, atau rebase dilakukan agent.**

### Langkah berikutnya

Migration ini membuka **lima task sekaligus**:

| Task | Judul | Dependency |
| --- | --- | --- |
| `BE-ACC-P2-005` | Daftar periksa penutupan | `004` ✅ |
| `BE-ACC-P2-007` | CRUD template jurnal berulang | `004` ✅ |
| `BE-ACC-P2-009` | Endpoint pengaturan akuntansi | `004` ✅ |
| `BE-ACC-P2-012` | Penolakan jurnal manual ke control account | `004` ✅, `011` ✅ |
| `BE-ACC-P2-013` | Saldo control account dari buku besar | `011` ✅ |

Dua hal yang disarankan didahulukan, dengan alasannya:

1. **`BE-ACC-P2-012`** — penanda control account sekarang berada dalam keadaan setengah jalan:
   akun dapat ditandai, tetapi penandanya belum menolak apa pun. Menandai akun sekarang memberi
   rasa aman yang belum ada dasarnya.
2. **Commit ketiga berkas migration**, supaya jendela risiko snapshot pada baris Risiko di atas
   segera tertutup.
