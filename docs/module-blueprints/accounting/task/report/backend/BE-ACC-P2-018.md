# Laporan Perubahan Backend — `BE-ACC-P2-018`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-018` |
| Judul | API Aturan Posting |
| Slice | `P2-0b` — Wave A, batch 14 September 2026 |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-018` (revisi 3) |
| Trace | `FR-P2-007`, `FR-P2-008`; `ACC-DEC-045`, `ACC-DEC-058`, `ACC-DEC-064`, `ACC-DEC-074` |
| Contract version | `ACC-API-0.10` grup Posting Rule termasuk penyesuaian `JournalTypeId` 14 Sep 2026; `ACC-VALIDATION-0.6` Phase 2 bagian 2; `ACC-PERMISSION-0.5` `PostingRule : Read/Create/Update` |
| Dependency | `BE-ACC-P2-015` ✅ |
| Klasifikasi | `HEAVY` — skor 9: repository 0, berkas diperiksa 2, berkas diubah 1 (4 berkas), logika bisnis 2 (validasi berbaris lintas akun, cost center, badan hukum), kontrak API 2 (grup baru), database 1, keamanan 1, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — controller, service, DTO aturan posting, satu registrasi `Program.cs`; laporan ini, baris status roadmap, traceability |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `b3ab542e` (branch `rizkiG`), perubahan belum di-commit |
| Tanggal | 14 September 2026 |
| Status | **✅ SELESAI** — 10 dari 10 acceptance terpetakan ke source; build owner `0 error`. Endpoint baru dapat dipanggil sesudah `BE-ACC-P2-016` diterapkan |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `MasterData/PostingRule` |
| Registry | `Acc` — `ACTIVE`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 31 (salinan suite 1.17.1 tidak memuatnya — `ACC-TD-015`) |
| Keberlakuan | `NEW CODE` |
| Jenis capability | **Master data** berbaris — standar master data berlaku; selisihnya di bagian 4 |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001` (ganti baris dalam satu transaction), `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-DEL-001` (nonaktif, bukan hapus) |

---

## 1. Masalah yang diperbaiki

Tabel aturan posting (`BE-ACC-P2-015`) belum punya jalan masuk. Akuntansi belum dapat menyusun
"kejadian jenis X pada badan hukum Y dibukukan ke akun-akun ini", padahal mesin posting gelombang
`P2-1` kelak hanya membaca aturan yang sudah tersusun. Menyusunnya **lebih dulu** berarti begitu
Finance mulai menerbitkan kejadian, pemetaan akunnya sudah diperiksa manusia.

---

## 2. Proses bisnis

### 2.1 Alur normal

| Langkah | Pelaku | Yang terjadi |
| ---: | --- | --- |
| 1 | Akuntansi | Memilih badan hukum, jenis kejadian aktif, jenis jurnal yang dihasilkan, perlakuan |
| 2 | Akuntansi | Menyusun baris: nomor, komponen (kosong = `TOTAL`), akun, cost center, sisi |
| 3 | Sistem | Memeriksa badan hukum utama, jenis kejadian, jenis jurnal, perlakuan, lalu setiap baris berurutan |
| 4 | Sistem | Memeriksa "satu aturan aktif per jenis per badan hukum", menyimpan aturan **aktif** → `201` |
| 5 | Akuntansi | Mengubah jenis jurnal, perlakuan, dan seluruh baris sekaligus (`PUT`, baris dikirim utuh) |
| 6 | Akuntansi | Menonaktifkan aturan; slot "satu aturan aktif" untuk jenis itu terbuka lagi |

### 2.2 Contoh berangka — aturan yang diterima

Jenis `PENGAKUAN-PIUTANG`, PT Metropolitan Medical Centre, jenis jurnal `JU`, perlakuan Buat Draft:

| Baris | Komponen | Akun | Cost center | Sisi |
| ---: | --- | --- | --- | --- |
| 1 | `TOTAL` | `1-1201 Piutang Penjamin` (control account) | — | Debit |
| 2 | `TOTAL` | `4-1001 Pendapatan Rawat Jalan` | — | Kredit |
| 3 | `JASA_MEDIS` | `5-3001 Beban Jasa Medis` (`Expense`) | Poli Umum | Debit |
| 4 | `JASA_MEDIS` | `2-1301 Utang Jasa Medis Dokter` | — | Kredit |

Diterima `201`. Baris 1 menunjuk control account **dan itu sah** — jalur otomatis adalah jalur yang
benar menuju Piutang (`ACC-DEC-064`), berbeda dari jurnal manual yang ditolak `BE-ACC-P2-012`.

### 2.3 Jalur tidak normal — urutan pemeriksaan

Penolakan pertama yang ditemukan dikembalikan, dan kalimatnya menyebut **nomor baris**.

| # | Keadaan | Hasil |
| ---: | --- | --- |
| 1 | Badan hukum utama tidak tunggal | `409` penjaga badan hukum |
| 2 | Badan hukum kosong / tidak aktif | `400` / `422` |
| 3 | Jenis kejadian kosong atau nonaktif | `400` "Jenis kejadian wajib dipilih dan harus aktif." |
| 4 | Jenis jurnal kosong atau nonaktif | `400` "Jenis jurnal wajib dipilih dan harus aktif." |
| 5 | Perlakuan bukan 1 atau 2 | `400` |
| 6 | Kurang dari 2 baris | `400` "Aturan posting minimal memiliki 2 baris." |
| 7 | Nomor baris kembar, atau < 1 | `400` |
| 8 | Sisi bukan 1 atau 2 | `400` "Baris ke-3: sisi harus debit atau kredit." |
| 9 | Kode komponen > 50 karakter | `400` |
| 10 | Akun tidak ada atau nonaktif | `400` "Baris ke-2: akun tidak ditemukan atau sudah tidak aktif." |
| 11 | Akun induk | `422` "Baris ke-2: akun 4-1000 adalah akun induk dan tidak dapat menerima transaksi." |
| 12 | Akun badan hukum lain | `409` "Baris ke-2: seluruh akun pada aturan posting harus berasal dari badan hukum yang sama; akun 4-1001 milik badan hukum lain." |
| 13 | Akun `Expense` tanpa cost center | `400` "Baris ke-3: baris akun beban 5-3001 wajib mencantumkan cost center." |
| 14 | Cost center nonaktif atau badan hukum lain | `409` |
| 15 | Seluruh baris di satu sisi | `400` "Aturan posting ini tidak akan pernah menghasilkan jurnal yang seimbang: aturan wajib memiliki sekurang-kurangnya satu baris debit dan satu baris kredit." |
| 16 | Jenis sudah punya aturan aktif pada badan hukum itu | `409` "Jenis kejadian PENGAKUAN-PIUTANG sudah punya aturan posting aktif pada badan hukum ini. Nonaktifkan aturan lama lebih dahulu, atau ubah aturan yang sudah ada." |
| 17 | Dua penyimpanan bersamaan lolos pemeriksaan 16 | Unique index `IX_AccPostingRule_LegalEntityId_EventTypeId` menolak; `23505` diterjemahkan `409` dengan kalimat yang sama |
| 18 | Menonaktifkan aturan yang sudah nonaktif | `409` |

**Contoh pemeriksaan 15.** Baris 1 `TOTAL` debit, baris 2 `JASA_MEDIS` debit, tanpa kredit. Nilai
berapa pun tidak akan pernah seimbang → `400`.

### 2.4 Ubah aturan — jebakan EF yang dijaga

`PUT` menghapus seluruh baris lama, **menyimpannya lebih dahulu**, lalu menambah baris baru lewat
`DbSet.AddRange` — bukan lewat navigasi yang terlacak — dalam satu transaction. Tanpa urutan itu EF
mencocokkan baris baru bernomor sama dengan baris lama dan mengirim `UPDATE`, lalu gagal. Jebakan
yang sama ditutup `BE-ACC-P2-007`.

### 2.5 Untuk mesin posting kelak

`AccPostingRuleService.CariAturanAktifAsync(db, legalEntityId, eventTypeId)` — `public static`,
tanpa registrasi DI — mengembalikan aturan aktif beserta barisnya berurutan, atau `null` bila belum
ada (kejadian akan berstatus Tertahan, `ACC-DEC-046`). Belum dipanggil siapa pun.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu roadmap `015`, `017`, `018`; `ACC-API-0.10` grup Posting Rule (api-contract baris 489–511);
`validation-matrix.md` bagian 2 (baris 211–223); `permission-audit-matrix.md` baris 202–204;
kamus data bagian 12, 12b; `AccRecurringJournalService` (validasi akun dan cost center, jebakan EF);
`AccChartOfAccount.cs`, `AccountType.cs`, `MstCostCenter.cs`, `MstLegalEntity.cs`,
`AccJournalType.cs`; `LoggerService.cs`; `Program.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/AccountingManagement/MasterData/PostingRule/Controllers/PostingRuleController.cs` | **Baru.** 5 action; pencatat log bermuatan `EntityId`, kode jenis, badan hukum, keaktifan, status — tanpa isi baris, tanpa nominal |
| `Areas/Corporate/AccountingManagement/MasterData/PostingRule/Services/AccPostingRuleService.cs` | **Baru.** Baca, tambah, ubah, nonaktifkan; `CariAturanAktifAsync`; penerjemahan `23505` |
| `Areas/Corporate/AccountingManagement/MasterData/PostingRule/DTOs/PostingRuleDtos.cs` | **Baru.** `PostingRulePagedQuery`, `PostingRuleListResponse`, `PostingRuleDetailResponse`, `PostingRuleLineResponse`, `CreatePostingRuleRequest`, `UpdatePostingRuleRequest`, `PostingRuleLineRequest` |
| `Program.cs` | `builder.Services.AddScoped<AccPostingRuleService>();` dan satu `using`, di blok registrasi Accounting |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Grup baru `api/v1/corporate/accounting/posting-rules`, 5 endpoint sesuai kontrak. Enum dikirim sebagai **angka**: `Treatment` 1 Langsung Disahkan / 2 Buat Draft; `Side` 1 Debit / 2 Kredit |
| Database | Menulis `AccPostingRule` dan `AccPostingRuleLine`; membaca akun, cost center, badan hukum, jenis kejadian, jenis jurnal. **Tabel belum ada sampai `BE-ACC-P2-016` diterapkan** |
| Keamanan/Auth | `ControllerName = "PostingRule"`, modul `ACCOUNTING_MASTER_DATA`, urutan 5. Hak baru `PostingRule : Read/Create/Update` — belum diberikan ke peran mana pun. Nol hardcode peran |

---

## 4. Dokumentasi endpoint

#### Corporate / Accounting / Master Data / Posting Rule

Base URL `api/v1/corporate/accounting/posting-rules`.

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar berhalaman; saring `legalEntityId`, `eventTypeId`, `journalTypeId`, `treatment`, `isActive`, `search` (kode/nama jenis kejadian); urut `eventTypeCode` (bawaan), `journalTypeCode`, `createDateTime` | `PostingRule : Read` |
| `GET` | `/{id}` | Rincian beserta seluruh baris — kode dan nama akun, penanda `IsControlAccount`, nama cost center | `PostingRule : Read` |
| `POST` | `/` | Menambah aturan; lahir aktif → `201` | `PostingRule : Create` |
| `PUT` | `/{id}` | Mengubah jenis jurnal, perlakuan, dan seluruh baris; badan hukum dan jenis kejadian tetap | `PostingRule : Update` |
| `PATCH` | `/{id}/deactivate` | Menonaktifkan aturan; disimpan sebagai riwayat | `PostingRule : Update` |

### Delta kontrak dan standar master data

| # | Delta | Alasan |
| ---: | --- | --- |
| 1 | Standar master data: `GET /filters/metadata`, `GET /summary`, `GET /options`, `PATCH /{id}/status`, `DELETE /{id}` **tidak** dibuat; tidak ada `activate` | Kontrak hanya 5 endpoint. Aturan nonaktif adalah riwayat; pemetaan baru dibuat sebagai aturan baru (kamus data bagian 12). **Kekurangan terhadap standar, dilaporkan** |
| 2 | Kontrak API menulis `409` untuk "aturan tidak akan pernah seimbang"; source menjawab **`400`** | Mengikuti `ACC-VALIDATION-0.6` bagian 2 baris 217 dan acceptance (3) kartu — keduanya `400`. Kontrak API perlu diselaraskan |
| 3 | "Tidak akan pernah seimbang" ditegakkan sebagai **wajib ada baris debit dan baris kredit** | Sesuai rumusan acceptance (3). Matriks validasi baris 217 berbunyi lebih luas — lihat bagian 7 |
| 4 | Cost center nonaktif atau milik badan hukum lain ditolak `409` | Tidak disebut kartu maupun matriks; pola `AccRecurringJournalService` |
| 5 | Kalimat penolakan menyebut nomor baris dan kode akun | Pola baris jurnal dan template; matriks menulis kalimat tanpa nomor |
| 6 | `[Tags]` bergaris miring; DTO berakhiran `Response` | Konvensi source Accounting (`ACC-GAP-004`) |
| 7 | **Ditunda:** "aturan yang masih ditunggu kejadian tertahan tidak boleh dinonaktifkan" (matriks baris 223) | Tabel kejadian belum ada — kekurangan terencana untuk `P2-1`, sesuai kartu |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=false` oleh Rizki, 14 Sep 2026 | `0 error`, 189 warning | `PASS` | Tangkapan layar owner. Nol perubahan pada berkas task ini sesudah build itu |
| Pemeriksaan source — hak akses | 5 action: `PostingRule` = `ControllerName`; argumen ke-2 = argumen ke-1 `[AccessAction]`; `AccessType` dari `AccessTypes` | `PASS` | `PostingRuleController.cs` |
| Pemeriksaan source — hardcode peran dan lapisan | Nol `IsInRole`/`UserType`; nol `ApplicationDbContext` di controller | `PASS` | Grep, 0 kecocokan |
| Pemeriksaan source — pelaku log | Muatan memakai `EntityId`, tanpa properti `Id`/`UserId`/`Name` yang akan menimpa pelaku di `LoggerService` | `PASS` | `CatatAsync` |
| Pemeriksaan kontrak | 5 endpoint `ACC-API-0.10` hadir; `JournalTypeId` wajib pada kedua request dan dikembalikan rincian | `PASS` | Bagian 4 |
| Pemanggilan endpoint | Tidak diminta kolom Verifikasi; tabel belum ada | `NOT RUN` | Menunggu `BE-ACC-P2-016` |

Uji manual: `NOT FEASIBLE` sampai migration `016` diterapkan.

**Tidak dijalankan:** automated test (`ACC-DEC-081`).

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria persis seperti roadmap | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Lima endpoint kontrak tersedia dengan `[AccessAction]` dan `[AccessPermission]` `PostingRule` | **Terpenuhi** | Bagian 4 |
| 2 | Kurang dari dua baris ditolak `400` | **Terpenuhi** | `SusunBarisAsync` — `BarisMinimum = 2` |
| 3 | Aturan yang **tidak akan pernah seimbang** ditolak `400` — yaitu aturan yang tidak punya sekurang-kurangnya satu baris Debit **dan** satu baris Kredit | **Terpenuhi** | Pemeriksaan terakhir `SusunBarisAsync` |
| 4 | Akun induk ditolak `422` | **Terpenuhi** | `!akun.IsPostable` → `422` |
| 5 | Akun milik badan hukum lain ditolak `409` | **Terpenuhi** | `akun.LegalEntityId != legalEntityId` → `409` |
| 6 | Baris akun beban tanpa cost center ditolak `400` | **Terpenuhi** | `AccountType.Expense && !CostCenterId.HasValue` → `400` |
| 7 | Jenis kejadian yang sudah punya aturan aktif pada badan hukum yang sama ditolak `409` | **Terpenuhi** | `AdaAturanAktifLainAsync` + penerjemahan `23505` pada `IX_AccPostingRule_LegalEntityId_EventTypeId` |
| 8 | `JournalTypeId` wajib dan menunjuk jenis jurnal yang ada | **Terpenuhi** | `SiapkanAsync` — kosong atau tidak aktif → `400` |
| 9 | Akun control account **boleh** dipakai | **Terpenuhi** | Tidak ada pemeriksaan `IsControlAccount`; respons membawanya sebagai penanda saja |
| 10 | Penjaga badan hukum utama dipanggil; tambah, ubah, dan nonaktifkan tercatat `LoggerService` tanpa nominal | **Terpenuhi** | `PeriksaAsync` di awal kelima method publik; `CatatAsync` + `TanpaNominal` |

| Butir DoD | Hasil |
| --- | --- |
| Source berubah | **Ya** |
| Laporan task tertulis | **Ya** — berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 189 warning solution — lihat `BE-ACC-P2-033` bagian 5 |
| Masalah yang diketahui | Endpoint menjawab `500` (`42P01`) sampai `BE-ACC-P2-016` diterapkan. Hak `PostingRule : *` belum diberikan ke peran mana pun |
| Perubahan sampingan | `NONE` |
| Interupsi | Source ditulis sesi 14 September 2026 sebelumnya; sesi ini memverifikasi tanpa menyunting ulang |
| Status Git | Lihat [`BE-ACC-P2-033`](BE-ACC-P2-033.md) bagian 7 — batch yang sama. Berkas milik task ini: `MasterData/PostingRule/{Controllers,Services,DTOs}`, `Program.cs` (bersama `017`). **Nol commit, push, stage, merge, rebase, atau migration** |

### Temuan untuk owner — perlu diputuskan sebelum mesin posting dibangun

| # | Temuan | Contoh | Pilihan |
| ---: | --- | --- | --- |
| 1 | **`PUT` mengubah aturan di tempat**, padahal kamus data bagian 12 menyebut aturan lama "disimpan nonaktif sebagai riwayat". `PUT` juga diterima untuk aturan yang sudah **nonaktif** | September: `PENGAKUAN-PIUTANG` mengkredit `4-1001`. Oktober aturan diubah menjadi `4-1002`. Jurnal otomatis September kelak tidak dapat ditelusuri ke pemetaan yang menghasilkannya — riwayatnya tertimpa | (a) `PUT` hanya untuk aturan yang belum pernah dipakai; perubahan sesudah dipakai = nonaktifkan + buat baru. (b) `PUT` menyimpan salinan lama nonaktif lalu membuat versi baru. (c) Terima perilaku sekarang dan ubah kamus data |
| 2 | **"Tidak akan pernah seimbang" lebih sempit daripada matriks validasi** | Baris: `TOTAL` debit, `JASA_MEDIS` debit, `TOTAL` kredit. Lolos pemeriksaan (ada debit dan kredit), tetapi hanya seimbang bila jasa medis = 0 | Perketat menjadi "setiap komponen seimbang terhadap dirinya sendiri", atau selaraskan matriks dengan rumusan kartu |
| 3 | Kontrak API menulis `409` untuk aturan tak seimbang; matriks dan source `400` | — | Selaraskan kontrak API |

### Langkah berikutnya

1. **Rizki:** buat dan terapkan migration `BE-ACC-P2-016`.
2. **Rizki:** putuskan temuan 1–3 di atas.
3. Frontend `FE-ACC-P2-009` (jenis kejadian) dan `FE-ACC-P2-010` (aturan posting) memakai endpoint ini.
