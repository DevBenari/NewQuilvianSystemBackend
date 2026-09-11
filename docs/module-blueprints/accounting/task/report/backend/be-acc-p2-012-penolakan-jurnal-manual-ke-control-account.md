# Laporan Perubahan Backend — `BE-ACC-P2-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-012` |
| Judul | Penolakan jurnal manual ke control account |
| Slice | Gelombang `P2-CTRL` — control account |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-012` |
| Trace | `ACC-DEC-064`; diperluas `ACC-DEC-072` (koreksi) dan `ACC-DEC-073` (template), keduanya 11 September 2026; `FR-P2-036`, `FR-P2-037` |
| Contract version | `ACC-VALIDATION-0.6` bagian 3b (`approved`), diamandemen **usulan `ACC-VALIDATION-0.7`**; **usulan `ACC-API-0.11`** untuk `IsControlAccount` pada `/options`, kode `422`, dan penegasan `200` pada `reverse` — menunggu ratifikasi |
| Dependency | `BE-ACC-P2-004` ✅ (kolom sudah ada di database), `BE-ACC-P2-011` ✅ |
| Klasifikasi | `MEDIUM` — skor 5: repository 0, berkas diperiksa 1, berkas diubah 1 (5 source + 1 uji), logika bisnis 1, kontrak API 1, database 0, keamanan 1 (larangan kewenangan pencatatan), UI 0 |
| Task mode | `CROSS-REPO MODE` — backend `rizkiG` sebagai target tulis; frontend tidak disentuh pada task ini |
| Target tulis | `NewQuilvianSystemBackend` — source `Areas/Corporate/AccountingManagement/**`, project `UnitTests.Sqlite`, `docs/module-blueprints/accounting/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `968841e`; source dan laporan awal di-commit Rizki sebagai **`cca0957`** |
| Tanggal | 11 September 2026 — diperbarui hari yang sama dengan uji manual Rizki (S1–S8) dan uji manual tambahan lewat API (A–D) |
| Status | **🟡 `SEBAGIAN`** — **status dipisah atas keputusan owner 11 September 2026:** Implementation `COMPLETE` sesuai cakupan · Developer Manual Test `COMPLETED` · Automated Verification `DEFERRED` · UAT `HANDOFF TO UAT TEAM`; lihat bagian 8. Uji manual terhadap PostgreSQL dev sudah dijalankan: S1–S8 oleh Rizki, A–D lewat API. Acceptance (1), (3), dan (4) **terbukti di runtime**; (2) terbukti untuk pembalikan penuh dan draft template, **belum** untuk jurnal penutup tahun. Build `Release`, 15 uji SQLite, dan regresi SQLite **belum diverifikasi**. Lihat bagian 5 dan 6 |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AccountingManagement` (Accounting) / `JournalManagement`, `RecurringJournal`, `MasterData/ChartOfAccount` |
| Registry | `Acc` — `ACTIVE`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 31 pada repository ini |
| Selisih registry | Salinan suite skill `1.17.1` (`rules/backend/engineering/`) **tidak memuat** baris `Acc`. Menurut `AGENTS.md` backend, kontrak dan registry repository ini yang berlaku; selisihnya sudah tercatat sebagai `ACC-DEP-007`, milik lead. Tidak disunting |
| Keberlakuan | `NEW CODE` — seluruh berkas yang disentuh adalah kode modul Accounting 2026 |
| QBE yang berlaku | `QBE-MOD-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-PERM-001`, `QBE-TXN-001` |
| QBE yang tidak berlaku | `QBE-ENT-*`, `QBE-NAM-*`, `QBE-CFG-*`, `QBE-CODE-*`, `QBE-DB-*` — tidak ada entity, configuration, nomor bisnis, maupun migration baru |

---

## 1. Masalah yang diperbaiki

`ACC-DEC-064` menetapkan bahwa Kas Kasir, Kas Kecil, Piutang, dan Utang hanya boleh dicatat lewat
kejadian akuntansi atau subledger, bukan lewat layar Jurnal Manual. `BE-ACC-P2-011` sudah
menyediakan penandanya, dan per 11 September 2026 enam akun di basis data dev sudah bertanda.
Tetapi **belum ada satu pun jalur yang menolaknya.** Petugas masih dapat membuat jurnal
"debit Kas Kasir Rp 5.000.000 lawan Pendapatan", mengajukannya, lalu mengesahkannya.

Akibatnya tidak terlihat hari itu juga. Saldo Kas Kasir di buku besar bertambah Rp 5.000.000 yang
tidak pernah ada di catatan kasir, dan selisihnya baru ketahuan saat rekonsiliasi — tanpa jejak
bahwa sebabnya adalah jurnal manual.

Dua jalan memutar juga terbuka, dan keduanya ditutup keputusan owner hari ini:

- **Penyesuaian `JP`.** Barisnya diketik bebas dan boleh dibuat atas jurnal Disahkan mana pun.
  Jurnal Disahkan apa saja dapat "disesuaikan" dengan baris debit Kas Kasir (`ACC-DEC-072`).
- **Template berulang.** Siapa pun yang berhak membuat template dapat menjurnal Kas Kasir setiap
  bulan tanpa kejadian apa pun (`ACC-DEC-073`).

### Kesulitan utamanya: jurnal tidak menyimpan asal-usulnya

Acceptance (2) menuntut jurnal dari tutup tahun, template, dan kejadian akuntansi **tidak**
terkena larangan. Tetapi ketiganya dibuat lewat `AccJournalService.CreateAsync` yang sama dengan
Form Jurnal, dan diajukan lewat endpoint `submit` yang sama. `AccJournal` tidak punya kolom asal;
menambahkannya menuntut migration, yang di luar wewenang task ini.

Kode jenis jurnal juga **tidak** dapat dipakai sebagai pembeda. Form Jurnal menerima jenis apa
pun: satu-satunya jurnal di basis data dev, `JB/2026/09/00001`, berjenis Jurnal Pembalik tetapi
`ReversalOfJournalId`-nya kosong — dibuat lewat Form Jurnal. Pengecualian menurut kode jenis dapat
diakali siapa pun yang memilih jenis `JB` atau `JT` di layar.

---

## 2. Proses bisnis

### 2.1 Jalur yang terkena dan yang tidak

| Jalur | Terkena? | Kapan diperiksa | Cara mengenalinya |
| --- | :---: | --- | --- |
| Form Jurnal — simpan | **Ya** | Saat disimpan | Pintu masuk `CreateManualAsync`, hanya dipanggil `JournalController` |
| Form Jurnal — ubah | **Ya** | Saat diubah | Setiap penggantian baris adalah susunan manusia |
| Form Jurnal — ajukan | **Ya**, kecuali tiga jalur di bawah | Saat diajukan | Menangkap draft yang tersimpan sebelum akunnya ditandai |
| Penyesuaian `JP` | **Ya** | Saat dibuat | Baris penyesuaiannya sendiri, apa pun isi jurnal asal |
| Template berulang | **Ya** | Saat disimpan, diubah, dan diaktifkan | — |
| Draft hasil template, belum diubah | Tidak | — | Ada `AccRecurringJournalRun` yang menunjuk jurnal itu |
| Pembalikan penuh `JB` | Tidak | — | `ReversalOfJournalId` terisi, `FullReversal`, barisnya persis cermin jurnal asal |
| Jurnal penutup tahun | Tidak | — | Jenis `JT` **dan** setiap barisnya berakun Pendapatan, Beban, atau Ekuitas |
| Kejadian akuntansi (`P2-1`) | Tidak | — | Belum ada di kode; kelak cukup tidak melewati jalur manual |

### 2.2 Kenapa tutup tahun dikenali dari bentuk barisnya

Alur `AccYearEndClosingService` diverifikasi sebelum pengecualian ini ditulis, sesuai instruksi
owner. Hasilnya:

1. Jurnal penutup dibuat lewat `_journalService.CreateAsync(...)` — jalur yang sama dengan Form
   Jurnal. **Tidak ada penanda asal apa pun yang disimpan.**
2. Penjaga jurnal penutup ganda (`CariJurnalPenutupAsync`) pun hanya mencari jenis `JT` pada
   periode tahun itu — jadi kode sendiri tidak punya pembeda yang lebih kuat.
3. Barisnya disusun `SusunRencanaAsync` hanya dari **akun `Revenue` dan `Expense`** (lewat
   `HitungSaldoTahunAsync`) ditambah **satu akun laba ditahan yang wajib `Equity`**
   (`AlasanAkunLabaDitahanTidakLayak`).

Karena itu penandanya adalah **jenis `JT` dan bentuk barisnya sekaligus**. Jurnal berjenis `JT`
yang menyentuh Kas, Piutang, atau Utang pasti bukan hasil tutup tahun, apa pun jenisnya.

**Contoh.** Jurnal penutup 2026 berisi "debit Pendapatan Rawat Jalan Rp 1.300.000.000, kredit
Beban Rp 900.000.000, kredit Laba Ditahan Rp 400.000.000". Seandainya seseorang keliru menandai
Pendapatan Rawat Jalan sebagai control account, jurnal ini **tetap lolos** diajukan — itulah
acceptance (2). Sebaliknya, draft berjenis `JT` berisi "debit Kas Kasir Rp 300.000 lawan
Pendapatan" **ditolak**, walau jenisnya sama.

### 2.3 Kenapa pembalikan penuh diperiksa cerminnya

Pembalikan penuh lahir `PendingApproval`. Bila penyetuju menolaknya, ia kembali `Rejected` dan
dapat diajukan ulang. Pada saat itu barisnya bisa saja sudah diubah lewat Form Jurnal — tetapi
setiap perubahan lewat Form Jurnal sudah ditolak bila menyentuh control account. Pemeriksaan
cermin menambah pagar kedua: begitu barisnya tidak lagi persis kebalikan jurnal asal, ia bukan
lagi pembalikan, dan diperiksa seperti jurnal manual.

### 2.4 Jalur tidak normal

| Keadaan | Hasil |
| --- | --- |
| Akun ditandai control **sesudah** draft manual tersimpan | Draft ditolak `422` saat diajukan; tetap `Draft` — **terbukti di runtime, langkah C** |
| Akun ditandai control **sesudah** template disimpan, template belum aktif | Aktivasi ditolak `422`; template tetap tidak aktif |
| Akun ditandai control **sesudah** template aktif | Template **tetap terbit** dan draftnya lolos diajukan — sisa risiko yang diterima `ACC-DEC-073`; **terbukti di runtime, langkah D2** |
| Beberapa baris menunjuk akun control | Pesan menyebut seluruh akunnya sekaligus, berurut kode |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` backend; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` dan
`MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; suite skill `rules/backend/` (`REPORT_TEMPLATE`) dan
`rules/rule-output/status-task-roadmap.md`; `AccJournalService`, `JournalController`,
`AccRecurringJournalService`, `AccYearEndClosingService`, `AccAccountingPeriodService`,
`AccChartOfAccountService`, `ReconciliationController`; enam berkas uji
`UnitTests.Sqlite/AccountingManagement/`; kontrak `validation-matrix.md`, `api-contract.md`,
`permission-audit-matrix.md`; keputusan `ACC-DEC-064`, `072`, `073`. Untuk analisis Skenario 6:
`AccountingServiceResult.cs`, laporan `be-acc-013-pembalikan-dan-jurnal-penyesuaian.md`,
`state-transition-matrix.md`, dan slice frontend `accounting-journal-slice.jsx`. Untuk uji A–D:
`AccPeriodClosingService` (status yang menahan penutupan), `PeriksaBukanJurnalSendiri`, dan
`RecurringFrequency`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `JournalManagement/Services/AccJournalService.cs` | `CreateManualAsync` baru (jalur Form Jurnal); pemeriksaan pada `UpdateAsync`, `SubmitAsync`, dan cabang penyesuaian `ReverseAsync`; bagian baru *Control account — BE-ACC-P2-012* berisi `AlasanControlAccountAsync` (`public static`, dipakai ulang template), `PeriksaControlAccountSaatDiajukanAsync`, `BerasalDariJalurOtomatisAsync`, `MencerminkanJurnalAsalAsync`; tiga konstanta sebutan jalur. `CreateAsync` **tidak diubah**, hanya diberi catatan |
| `JournalManagement/Controllers/JournalController.cs` | `POST /` memanggil `CreateManualAsync` alih-alih `CreateAsync` |
| `RecurringJournal/Services/AccRecurringJournalService.cs` | Pemeriksaan pada `SiapkanAsync` (dipakai simpan dan ubah) serta `ActivateAsync`. **Sengaja tidak** dimasukkan ke `AlasanTidakLayakTerbitAsync`, karena method itu juga dipakai penerbitan |
| `MasterData/ChartOfAccount/DTOs/ChartOfAccountDtos.cs` | `ChartOfAccountOptionResponse.IsControlAccount` |
| `MasterData/ChartOfAccount/Services/AccChartOfAccountService.cs` | `GetOptionsAsync` mengisi bidang itu; **tidak menyaring** akun control |
| `Tests/.../AccControlAccountJournalGuardTests.cs` (baru) | 15 uji SQLite — lihat bagian 5 |

Dokumen yang diperbarui sebagai **amandemen owner pada langkah 2**, bukan wewenang skill build:
`00-interview-decisions.md` (revisi 8), `validation-matrix.md`, `api-contract.md`,
`permission-audit-matrix.md`, serta baris Trace/Kontrak/Perluasan cakupan kartu `BE-ACC-P2-012`,
`BE-ACC-P2-013`, `FE-ACC-P2-007`, dan `FE-ACC-P2-008`. Atas keputusan Rizki pada Skenario 6,
`api-contract.md` grup Journal juga mendapat **penegasan `200` untuk `reverse`** sebagai butir (5)
usulan `ACC-API-0.11` — dokumentasi saja, kode tidak diubah.

Pembaruan laporan ini sesudah uji manual **tidak mengubah source apa pun.**

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Nol endpoint baru. `422` baru pada `POST /journals`, `PUT /journals/{id}`, `POST /journals/{id}/submit`, `POST /journals/{id}/reverse` (penyesuaian), `POST`/`PUT /recurring-journals`, `PATCH /recurring-journals/{id}/activate`. Bidang baru `isControlAccount` pada respons `/chart-of-accounts/options`. Seluruhnya tercatat pada usulan `ACC-API-0.11` dan `ACC-VALIDATION-0.7` |
| Database | Nol migration, nol perubahan schema. Tidak ada eksekusi SQL langsung; data uji dibuat dan dibereskan **lewat API** (bagian 5.2 dan 5.4) |
| Keamanan/Auth | Nol `[AccessPermission]` baru atau berubah. Penolakan bersifat aturan bisnis pada data, bukan hardcode peran. Pemisahan tugas `ACC-DEC-016` tetap tegak — dibuktikan langkah D1 |

---

## 4. Dokumentasi endpoint

Tidak ada endpoint baru. Endpoint yang perilakunya berubah:

#### Corporate / Accounting / Journal Management / Journal

| Method | Path | Perubahan bagi pengguna | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | `422` bila ada baris ke akun control | `Journal : Create` |
| `PUT` | `/{id}` | `422` bila ada baris ke akun control, termasuk atas draft template dan jurnal pembalik | `Journal : Update` |
| `POST` | `/{id}/submit` | `422` bila menyentuh akun control dan bukan hasil jalur otomatis | `Journal : Submit` |
| `POST` | `/{id}/reverse` | `422` bila **penyesuaian** menyentuh akun control; pembalikan penuh tetap boleh. Sukses menjawab **`200`** | `Journal : Reverse` |

#### Corporate / Accounting / Recurring Journal

| Method | Path | Perubahan bagi pengguna | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | `422` bila baris template menunjuk akun control | `RecurringJournal : Create` |
| `PUT` | `/{id}` | Sama | `RecurringJournal : Update` |
| `PATCH` | `/{id}/activate` | `422` bila baris template menunjuk akun control | `RecurringJournal : Activate` |

#### Corporate / Accounting / Master Data / Chart of Account

| Method | Path | Perubahan bagi pengguna | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/options` | Bidang `isControlAccount` ditambahkan; daftar akun tidak berubah | `ChartOfAccount : Read` |

Pesan penolakan berbentuk:
*"Akun 1-1002 Kas Kasir hanya dapat dicatat lewat kejadian akuntansi, bukan jurnal manual."*
Kata terakhir menjadi *jurnal penyesuaian* atau *template jurnal berulang* sesuai jalurnya. Pada
aktivasi template, pesannya diawali *"Template {kode} tidak dapat diaktifkan."*

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| **Uji manual Swagger — S1–S8** | Dijalankan Rizki, 11 September 2026 | S1–S8 `PASS`; S3 dilengkapi langkah A; S6 fungsional `PASS`, **`200` sesuai kontrak/implementasi** | Bagian 5.2 dan 5.3 |
| **Uji manual tambahan lewat API — A–D** | Dijalankan agent lewat API yang sedang berjalan, 11 September 2026, masuk sebagai SuperAdmin | A, B, C, D1, D2 `PASS`; D-akhir (setujui dan sahkan `JB`) **menunggu pengguna kedua**; D3 (`JT`) tidak dijalankan | Bagian 5.4 |
| `dotnet build -c Release` | **Belum diverifikasi** | `NOT RUN` | Aplikasi yang diuji menjawab dengan kalimat penolakan yang hanya ada di kode task ini, jadi **source aplikasi terkompilasi dan berjalan** — tetapi konfigurasinya tidak tercatat, dan build `Debug` tidak membangun project test |
| 15 uji `AccControlAccountJournalGuardTests` | **Belum diverifikasi** — belum pernah dikompilasi maupun dijalankan | `NOT RUN` | — |
| Seluruh project `UnitTests.Sqlite` (regresi) | **Belum diverifikasi** | `NOT RUN` | Diperiksa secara statis saja: tidak ada uji lama yang menandai akun control lalu mengajukan, mengubah, atau menyesuaikan jurnal |

Uji manual: `PASS` — seluruh skenario yang dijalankan lulus. Satu langkah menunggu pengguna kedua.

**Kolom Verifikasi kartu** menuntut *"test integrasi PostgreSQL untuk keempat acceptance,
terutama (2)"*. Uji manual terhadap PostgreSQL sungguhan kini mencakup keempat acceptance, dengan
satu celah pada (2): pengecualian **jurnal penutup tahun** belum pernah dijalankan di runtime.
Uji otomatisnya belum pernah dijalankan sama sekali.

**Kelima belas uji yang ditulis**, dipetakan ke acceptance:

| Uji | Membuktikan |
| --- | --- |
| `JurnalManual_KeControlAccount_Ditolak422_DanMenyebutAkunnya` | (1); nol jurnal tersimpan |
| `JurnalManual_DraftSebelumAkunDitandai_DitolakSaatDiajukan` | (1) pada jalur ajukan |
| `JurnalManual_KeAkunBiasa_TetapDiterimaDanDapatDiajukan` | (3) |
| `UbahDraft_TanpaControlAccount_TetapDiterima` | (4) |
| `UbahDraft_MenambahControlAccount_Ditolak422_BarisLamaUtuh` | (4); baris lama tidak hilang |
| `JurnalPenutupTahun_TidakTerkenaLarangan` | (2) lewat `AccYearEndClosingService.GenerateAsync` sungguhan |
| `JenisJT_YangMenyentuhKas_BukanTutupTahun_DitolakSaatDiajukan` | Pengecualian bukan dari kode jenis |
| `TemplateAktif_AkunnyaBaruDitandai_TetapTerbitDanDraftnyaLolosDiajukan` | (2) template; `ACC-DEC-073` butir (3) |
| `PembalikanPenuh_JurnalControl_DiterimaDanLolosDiajukanUlang` | `ACC-DEC-072` butir (1) dan (3) |
| `PembalikYangBarisnyaDiubah_DiperiksaSepertiJurnalManual` | `ACC-DEC-072` butir (3) |
| `Penyesuaian_BarisKeControlAccount_Ditolak422` | `ACC-DEC-072` butir (2) |
| `Penyesuaian_AtasJurnalControl_BarisnyaNonControl_Diterima` | `ACC-DEC-072` butir (2), sisi yang boleh |
| `Template_KeControlAccount_Ditolak422SaatDisimpan` | `ACC-DEC-073` butir (1) |
| `Template_AkunnyaBaruDitandai_Ditolak422SaatDiaktifkan` | `ACC-DEC-073` butir (2) |
| `Options_MembawaPenandaControlAccount_TanpaMenyaringnya` | Bidang `/options` |

Jalur **ubah template** tidak diuji tersendiri: ia melewati `SiapkanAsync` yang sama dengan
simpan template.

### 5.1 Rencana uji manual yang diserahkan kepada Rizki

Rencana ini dipertahankan sebagai catatan; hasilnya ada di 5.2. Kode akun pada data dev berbeda
dari rencana (Kas Besar ternyata `1002`, pendapatan `4001`). Baris 6 **dikoreksi** atas keputusan
Rizki 11 September 2026 — lihat 5.3.

| No | Langkah | Hasil yang diharapkan | Acceptance |
| ---: | --- | --- | --- |
| 1 | `POST /journals` — debit `1-1002`, kredit akun pendapatan | `422`, pesan menyebut "1-1002 Kas Kasir" dan "jurnal manual"; tidak ada nomor jurnal terpakai | (1) |
| 2 | `POST /journals` — debit `1-1001` Kas Besar, kredit pendapatan | `201` draft | (3) |
| 3 | `PUT /journals/{id draft no. 2}` — ganti Kas Besar menjadi `1-1002` | `422`; baris lama tetap | (4) |
| 4 | Lepas penanda `1-1003` (`PUT` akun, `isControlAccount: false`), buat jurnal Kas Kecil lawan pendapatan, ajukan, setujui (pengguna kedua), sahkan. Tandai lagi `1-1003` | Jurnal Disahkan yang menyentuh akun control | Penyiapan |
| 5 | `POST /journals/{id no. 4}/reverse` — `correctionType: 2` (penyesuaian) dengan satu baris ke `1-1003` | `422`, "jurnal penyesuaian" | `ACC-DEC-072` |
| 6 | `POST /journals/{id no. 4}/reverse` — `correctionType: 1` (pembalikan penuh) | **`200`** (semula tertulis `201` — keliru), status menunggu persetujuan | `ACC-DEC-072` |
| 7 | `POST /recurring-journals` — baris ke `1-1002` | `422`, "template jurnal berulang" | `ACC-DEC-073` |
| 8 | `GET /chart-of-accounts/options?legalEntityId=…` | `1-1002` **tetap ada** dengan `isControlAccount: true`; `1-1001` bernilai `false` | `/options` |

### 5.2 Hasil uji manual — Rizki, 11 September 2026

**Lingkungan.** Basis data dev `QuilvianNewDevRizki`, lewat Swagger. Badan hukum
PT Metropolitan Medical Centre (`3bf63974-a754-4b20-81ee-70894f6fb058`). Jenis jurnal `JU`
(`87796dbd-edf2-46c1-8f7c-0f6ff17ab2ae`).

| Akun | Id | `IsControlAccount` |
| --- | --- | --- |
| `1002` Kas Besar | `00abca16-2c8b-4d09-8add-ef7ad0917cad` | `false` |
| `1-1002` Kas Kasir | `dadfb05e-5841-4bc5-9061-8443c795ff88` | `true` |
| `1-1003` Kas Kecil | `91faf71e-97bc-451f-b1c1-4bc22c77cd9f` | `true` (sempat `false` selama penyiapan S4, C, dan D2) |
| `4001` Pendapatan Rawat Jalan | `a89b3949-2e79-4ce8-8e41-dc1370680d03` | `false` |

| No | Yang dijalankan | Hasil aktual | Klasifikasi | Menjawab |
| ---: | --- | --- | --- | --- |
| S1 | `POST /journals` — debit `1-1002` Kas Kasir Rp 1.000.000, kredit akun lawan non-control | `422` — "Akun 1-1002 Kas Kasir hanya dapat dicatat lewat kejadian akuntansi, bukan jurnal manual." | `PASS` | (1) jalur Simpan |
| S2 | `POST /journals` — debit `1002` Kas Besar Rp 1.000.000, kredit `4001` Pendapatan Rawat Jalan | `201`, draft `a41b9ca2-2217-4c9e-8786-cbc6691592f1` (`JU/2026/09/00001`) | `PASS` | (3) |
| S3 | `PUT /journals/a41b9ca2…` — Kas Besar diganti `1-1002` Kas Kasir | `422` — kalimat yang sama dengan S1 | `PASS` — preservasi baris dibuktikan langkah A | (4) sisi tolak; (1) jalur Ubah |
| S4 | Penyiapan: `1-1003` sementara `false`; jurnal `JU/2026/09/00002` (`UJI-012-S4-001`), debit Kas Kecil lawan `4001` Rp 1.000.000; Create → Submit → Approve (Narendra Kusuma) → Post; lalu `1-1003` dikembalikan `true` | Seluruh alur berhasil; jurnal `1f49c14a-d820-4682-8f28-b2ff446b18c6` Disahkan | `PASS` (penyiapan) | (3) — jurnal non-control menempuh Simpan → Ajukan → Setujui → Sahkan dengan kode baru |
| S5 | `POST /journals/1f49c14a…/reverse`, `correctionType = 2`, baris penyesuaian ke `1-1003` | `422` — "Akun 1-1003 Kas Kecil hanya dapat dicatat lewat kejadian akuntansi, bukan jurnal penyesuaian." | `PASS` | `ACC-DEC-072` butir (2) |
| S6 | `POST /journals/1f49c14a…/reverse`, `correctionType = 1` | **`200`**; `JB/2026/09/00002` (`b08b00da-718b-4038-a886-b842b532fef4`), `journalTypeCode = JB`, `correctionType = 1`, `reversalOfJournalId = 1f49c14a…`, total debit = kredit = 1.000.000, `isBalanced = true`, status `PendingApproval` | **Fungsional `PASS`. HTTP `200` = sesuai kontrak dan implementasi — bukan selisih kontrak** (keputusan Rizki, 5.3) | `ACC-DEC-072` butir (1); (2) pembalikan penuh saat dibuat |
| S7 | `POST /recurring-journals` — baris ke `1-1002` Kas Kasir | `422` — "Akun 1-1002 Kas Kasir hanya dapat dicatat lewat kejadian akuntansi, bukan template jurnal berulang." | `PASS` | `ACC-DEC-073` butir (1) |
| S8 | `GET /master-data/chart-of-accounts/options` | `1002` Kas Besar `isControlAccount = false`; `1-1002` Kas Kasir `isControlAccount = true` — akun control **tetap dikirim** | `PASS` | Bidang `/options`; `/options` tidak menyaring |

**Yang dibuktikan kode, bukan uji.** Pada S1, S5, dan S7 penolakan terjadi **sebelum** apa pun
ditulis: `CreateManualAsync` menolak sebelum `CreateAsync` (belum ada alokasi nomor), penyesuaian
ditolak sebelum jurnal koreksi dibentuk, dan template ditolak di `SiapkanAsync` sebelum `Add`. Itu
pembacaan kode. Ketiadaan template sesudah S7 tidak dicatat sebagai bukti terpisah. Untuk S3,
buktinya kini **runtime** — langkah A.

### 5.3 Selisih `200` vs `201` pada Skenario 6 — **diputuskan, bukan selisih kontrak**

**Keputusan Rizki, 11 September 2026: Rekomendasi A — koreksi dokumentasi saja.**
`POST /journals/{id}/reverse` yang berhasil menjawab **`200`**. `ReverseAsync` **tidak diubah.**
Skenario 6 tercatat *Fungsional `PASS`; HTTP `200` sesuai kontrak dan implementasi*.

Dasar keputusannya, hasil penelusuran sebelum diputuskan:

| Pertanyaan | Jawaban | Bukti |
| --- | --- | --- |
| Apakah kontrak menetapkan `201`? | **Tidak** | `api-contract.md` grup Journal hanya menyebut "`200` — permintaan berhasil"; `state-transition-matrix.md` dan `validation-matrix.md` bagian 5 tidak menyebut kode sukses |
| Apakah task pemilik endpoint mencatat kode suksesnya? | **Ya, `200`** | Laporan `be-acc-013` baris 121 (pembalikan penuh) dan 127 (penyesuaian) |
| Apakah service menghasilkan `200`? | **Ya, sejak `BE-ACC-013`** | `ReverseAsync` → `MuatDanPetakanAsync` → `AccountingServiceResult<T>.Ok(data, pesan)` tanpa kode status; bawaannya `StatusCodes.Status200OK` (`AccountingServiceResult.cs` baris 27) |
| Dari mana `201` berasal? | **Rencana uji laporan ini**, baris 6 bagian 5.1 | Satu-satunya sumber; kini dikoreksi |
| Dampak ke layar? | Nol | Thunk `reverseJournal` hanya membaca kode status saat galat |

**Yang dikerjakan atas keputusan itu:**

1. Baris 6 bagian 5.1 dikoreksi menjadi `200`.
2. `api-contract.md` grup Journal mendapat penegasan eksplisit bahwa `reverse` menjawab `200`,
   sebagai **butir (5) usulan `ACC-API-0.11`** — diratifikasi bersama perubahan kontrak lain yang
   masih tertunda. Status `approved` kontrak tidak diubah.

**Catatan konvensi yang tetap terbuka, di luar task ini.** Enam jalur lain yang membuat data baru
di Accounting menjawab `201`, termasuk penerbitan template dan jurnal penutup tahun. Bila kelak
ingin diseragamkan, itu keputusan tersendiri yang menuntut amandemen kontrak — bukan cacat task
ini.

### 5.4 Uji manual tambahan A–D — lewat API, 11 September 2026

Dijalankan agent terhadap API yang sedang berjalan (`https://localhost:7184`), masuk sebagai
**SuperAdmin** (`0ba84a1a-2559-49ba-a320-10fb1f399d70`). Kredensial dibaca dari
`appsettings.Development.json` tanpa ditampilkan. Setiap request dicatat sebagai log bukti di
scratchpad sesi (tidak dilacak git). Nol eksekusi SQL langsung; seluruh perubahan data lewat API.

**Temuan sebelum langkah B.** Draft S2 ternyata **tidak lagi `Draft`**: berstatus
`PendingApproval`, diajukan oleh **Narendra Kusuma** sesudah S3. Itu sekaligus bukti runtime
tambahan untuk (3) — jurnal manual non-control lolos Ajukan. Karena `PUT` atas jurnal yang menunggu
persetujuan ditolak penjaga status, langkah B memakai jalur yang disediakan `ACC-STATE-0.1`: tolak
dahulu (Tolak tidak dibatasi pada pelakunya), lalu ubah — perubahan mengembalikannya ke `Draft`.

#### A. Preservasi baris sesudah `PUT` S3 yang gagal

| Request | Diharapkan | Aktual | Klasifikasi |
| --- | --- | --- | --- |
| `GET /journals/a41b9ca2-2217-4c9e-8786-cbc6691592f1` | Baris masih `1002` Kas Besar, bukan `1-1002` Kas Kasir | `200` — "Rincian jurnal berhasil diambil." Baris 1: `1002` Kas Besar (`00abca16…`) D 1.000.000; baris 2: `4001` (`a89b3949…`) K 1.000.000. Memuat Kas Kasir: **tidak** | `PASS` |

`PUT` yang ditolak `422` tidak memodifikasi baris yang tersimpan.

#### B. Ubah jurnal lama dengan akun non-control

| Langkah | Request | Diharapkan | Aktual | Klasifikasi |
| --- | --- | --- | --- | --- |
| B-1 | `PUT /journals/a41b9ca2…` saat masih `PendingApproval` | Ditolak penjaga status | `409` — "Jurnal sedang menunggu persetujuan dan tidak dapat diubah." | Sesuai (dokumentasi) |
| B-2 | `POST /journals/a41b9ca2…/reject`, alasan tertulis | Ditolak | `200` — "Jurnal ditolak." | Sesuai |
| B-3 | `PUT /journals/a41b9ca2…` — `1002` Kas Besar D 1.500.000 / `4001` K 1.500.000, keterangan diubah | Berhasil; tidak terkena penjaga control account | `200` — "Jurnal berhasil diperbarui dan kembali menjadi draft." Status `Draft`, total 1.500.000, `isBalanced = true` | **`PASS`** |
| B-4 | `GET /journals/a41b9ca2…` | Ubahan tersimpan | `200` — baris dan keterangan baru tersimpan | `PASS` |
| B-5 | `DELETE /journals/a41b9ca2…` | Draft terhapus | `200` — "Jurnal draft berhasil dihapus." | Sesuai |
| B-6 | `GET /journals/a41b9ca2…` | Tidak ditemukan | `404` — "Jurnal tidak ditemukan." | Sesuai |

#### C. Penjaga saat Ajukan

Penyiapan meniru S4, dengan pengembalian penanda di blok `finally` supaya `1-1003` tidak pernah
tertinggal non-control.

| Langkah | Request | Diharapkan | Aktual | Klasifikasi |
| --- | --- | --- | --- | --- |
| C-1 | `PUT /master-data/chart-of-accounts/91faf71e…` — `isControlAccount: false`, bidang lain disalin dari rincian | Berhasil | `200` — "Akun berhasil diperbarui." | Penyiapan |
| C-2 | `POST /journals` — `UJI-012-C-001`, `1-1003` Kas Kecil D 250.000 / `4001` K 250.000 | Draft tersimpan | `201` — `JU/2026/09/00003` (`e2cbdda1-f6ca-4304-b6ad-e13c4b1a2b03`) | Penyiapan |
| C-3 | `PUT` akun — `isControlAccount: true`, lalu `GET` | Penanda kembali | `200`; `isControlAccount = true` | Penyiapan |
| C-4 | `POST /journals/e2cbdda1…/submit` | `422`, "jurnal manual" | **`422` — "Akun 1-1003 Kas Kecil hanya dapat dicatat lewat kejadian akuntansi, bukan jurnal manual."** | **`PASS`** |
| C-5 | `GET /journals/e2cbdda1…` | Tetap `Draft` | `200` — status `Draft`, `submittedBy` kosong | `PASS` |
| C-6 | `DELETE /journals/e2cbdda1…` lalu `GET` | Terhapus | `200` — "Jurnal draft berhasil dihapus."; `GET` `404` | Sesuai |

#### D. Jalur otomatis tidak salah diblokir saat Ajukan

**D1 — pembalikan penuh `JB/2026/09/00002` diajukan ulang tanpa diubah**

| Langkah | Request | Diharapkan | Aktual | Klasifikasi |
| --- | --- | --- | --- | --- |
| D1-0 | `GET /journals/b08b00da…` | — | `PendingApproval`; dibuat dan diajukan SuperAdmin; baris `1-1003` K 1.000.000 / `4001` D 1.000.000 — cermin tepat `JU/2026/09/00002` | — |
| D1-1 | `POST /journals/b08b00da…/reject`, alasan tertulis | Ditolak | `200` — "Jurnal ditolak." | Sesuai |
| D1-2 | `POST /journals/b08b00da…/submit` — **tanpa diubah**, baris menyentuh `1-1003` (control) | `200` — pengecualian cermin | **`200` — "Jurnal berhasil diajukan."**; status `PendingApproval` | **`PASS`** |
| D1-3 | `POST /journals/b08b00da…/approve` oleh SuperAdmin, pembuatnya | `403` — pemisahan tugas | `403` — "Anda tidak dapat menyetujui jurnal yang Anda buat sendiri." | `PASS` (`ACC-DEC-016`) |

**D-akhir — setujui dan sahkan `JB/2026/09/00002`, lalu buktikan penetralan S4: MENUNGGU.**
Persetujuan menuntut pengguna selain pembuatnya (`ApproveAsync` membandingkan dengan `CreateBy`),
dan agent tidak memegang kredensial pengguna kedua. Setelah Narendra Kusuma — atau pengguna
berhak lain — menyetujuinya, pengesahan dan pembuktian saldo dapat dijalankan. Saldo sebelum `JB`
disahkan, dicatat 11 September 2026:

| Akun | Periode | Sebelum | Diharapkan sesudah `JB` disahkan |
| --- | --- | --- | --- |
| `1-1003` Kas Kecil (rekonsiliasi dan buku besar) | 2026-09 | D 1.000.000, saldo **+1.000.000**, 1 baris | D 1.000.000, K 1.000.000, saldo **0**, 2 baris |
| `4001` Pendapatan Rawat Jalan (buku besar) | 2026-09 | K 2.000.000, saldo **−2.000.000** | D 1.000.000, K 2.000.000, saldo **−1.000.000** — sisa Rp 1.000.000 berasal dari `JB/2026/09/00001` lama |

**D2 — draft hasil template yang akunnya baru ditandai control**

| Langkah | Request | Diharapkan | Aktual | Klasifikasi |
| --- | --- | --- | --- | --- |
| D2-1 | `PUT` akun `1-1003` — `isControlAccount: false` | Berhasil | `200` | Penyiapan |
| D2-2 | `POST /recurring-journals` — `UJI-012-D2`, bulanan tanggal 25 mulai 2026-09-01, `1-1003` D 100.000 / `4001` K 100.000 | Tersimpan tidak aktif | `201` — template `c55a7312-9fef-4dd1-882f-c7e7535b2542` | Penyiapan |
| D2-3 | `PATCH /recurring-journals/c55a7312…/activate` | Aktif | `200` — "… berhasil diaktifkan …" | Penyiapan |
| D2-4 | `PUT` akun — `isControlAccount: true`, lalu `GET` | Penanda kembali | `200`; `isControlAccount = true` | Penyiapan |
| D2-5 | `POST /recurring-journals/c55a7312…/generate` — `accountingDate` 2026-09-25 | Terbit tanpa pemeriksaan ulang (`ACC-DEC-073` butir 3) | `201` — `JU/2026/09/00004` (`94102209-569f-4abc-9bda-48a868bd91e6`) berstatus draft | **`PASS`** |
| D2-6 | `POST /journals/94102209…/submit` — baris menyentuh `1-1003` (control) | `200` — pengecualian draft template | **`200` — "Jurnal berhasil diajukan."**; status `PendingApproval` | **`PASS`** |
| D2-7 | `POST /journals/94102209…/reject` | Dibereskan | `200` — status `Rejected` | Pembersihan |
| D2-8 | `PATCH /recurring-journals/c55a7312…/deactivate` | Dinonaktifkan | `200` — "… berhasil dinonaktifkan …" | Pembersihan |

**D3 — jurnal penutup tahun `JT`: tidak dijalankan.** Penyusunannya menuntut seluruh periode satu
tahun buku tertutup. Di basis data dev itu berarti menutup dua belas periode lengkap dengan
persetujuan penutupan — bukan penyiapan yang aman. Buktinya tetap hanya uji SQLite
`JurnalPenutupTahun_TidakTerkenaLarangan`, yang belum dijalankan.

**Jalur kejadian akuntansi (`P2-1`): belum dapat diuji** — kodenya belum ada.

#### Data uji — keadaan sesudah A–D

| Data | Keadaan | Akibat |
| --- | --- | --- |
| `JU/2026/09/00001` (`a41b9ca2…`, S2) | Dihapus (lunak) pada B-5 | — |
| `JU/2026/09/00003` (`e2cbdda1…`, C) | Dihapus (lunak) pada C-6 | — |
| `JU/2026/09/00002` (`1f49c14a…`, S4) | `Posted`, permanen | Kas Kecil dan `4001` +1.000.000 sampai `JB/2026/09/00002` disahkan |
| `JB/2026/09/00002` (`b08b00da…`, S6) | `PendingApproval` | **Menahan penutupan September 2026** sampai disetujui dan disahkan — yang sekaligus menetralkan S4 |
| `JU/2026/09/00004` (`94102209…`, D2) | `Rejected`, permanen | Bukan penghalang penutupan (`AccPeriodClosingService` hanya menghitung `Draft`, `PendingApproval`, `Approved`); tanpa dampak buku besar |
| Template `UJI-012-D2` (`c55a7312…`) | Tidak aktif, permanen — tidak ada endpoint hapus template | Tidak terbit lagi |
| Akun `1-1003` Kas Kecil | `isControlAccount = true` | Kembali seperti semula; `UpdateBy`/`UpdateDateTime` akunnya berubah |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti runtime | Yang masih kurang |
| --- | --- | --- | --- |
| (1) Baris jurnal **manual** ke akun ber-`IsControlAccount = true` ditolak `422`, dan pesannya menyebut akun mana | **Terbukti di runtime** — Simpan, Ubah, dan Ajukan | S1 (Simpan), S3 (Ubah), C-4 (Ajukan); pesan menyebut kode dan nama akun | Uji SQLite belum dijalankan |
| (2) Jurnal dari kejadian akuntansi, dari template berulang, dan jurnal penutup tahun **tidak** terkena aturan ini | **Sebagian** — dua dari tiga jalur yang ada di kode | Pembalikan penuh saat dibuat (S6) dan saat diajukan ulang (D1-2); draft template saat terbit dan diajukan (D2-5, D2-6) | **Jurnal penutup tahun `JT`** belum di runtime (D3 tidak aman); jalur kejadian akuntansi **belum ada di kode**; uji SQLite belum dijalankan |
| (3) Akun non-control tetap dapat dijurnal manual | **Terbukti di runtime** | S2 (Simpan), S4 (Simpan → Ajukan → Setujui → Sahkan), pengajuan S2 oleh Narendra Kusuma, B-3 (Ubah) | — |
| (4) Jurnal manual yang sudah ada tidak ikut ditolak saat diubah, kecuali barisnya menyentuh control account | **Terbukti di runtime** | S3 (ubah ke Kas Kasir `422`), A (baris tidak berubah sesudah `PUT` gagal), B-3 (ubah dengan akun non-control `200`) | — |
| `ACC-DEC-072` (1) pembalikan penuh boleh | Terbukti di runtime | S6 (`200` sesuai kontrak), D1-2 | Pengesahan `JB` menunggu pengguna kedua |
| `ACC-DEC-072` (2) penyesuaian ke control ditolak | Terbukti di runtime | S5 | — |
| `ACC-DEC-072` (3) pembalik yang diubah diperiksa seperti manual | Belum di runtime | — | Uji SQLite `PembalikYangBarisnyaDiubah_…` |
| `ACC-DEC-073` (1) template ke control ditolak saat disimpan | Terbukti di runtime (respons) | S7 | Ketiadaan template sesudahnya belum dicatat terpisah |
| `ACC-DEC-073` (2) ditolak saat diaktifkan | Belum di runtime | — | Uji SQLite `Template_AkunnyaBaruDitandai_Ditolak422SaatDiaktifkan` |
| `ACC-DEC-073` (3) terbit tidak memeriksa ulang | Terbukti di runtime | D2-5 | — |
| Bidang `IsControlAccount` pada `/options` | Terbukti di runtime | S8 | — |
| DoD — endpoint berjalan | **Terpenuhi** | Seluruh endpoint di atas menjawab dengan perilaku kode baru pada 11 September 2026 | — |
| DoD — test hijau | **Belum terpenuhi** | — | Build `Release`, 15 uji SQLite, dan regresi `UnitTests.Sqlite` belum diverifikasi |
| DoD — laporan task tertulis | Terpenuhi | Berkas ini | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Uji otomatis belum pernah dikompilasi maupun dijalankan.** Aplikasi terbukti berjalan, tetapi project test hanya dibangun pada konfigurasi `Release` — kesalahan ketik atau nama bidang di berkas uji baru ketahuan saat `dotnet build QuilvianSystemBackend.sln -c Release -p:RunAnalyzers=false` dijalankan |
| Peringatan governance | Pada langkah 1 sesi ini, dua kueri **read-only** dijalankan ke `QuilvianNewDevRizki` **sebelum** larangan eksekusi database langsung pada `CLAUDE.md` backend terbaca. Nol tulisan. Uji A–D sesudahnya berjalan **lewat API** atas instruksi eksplisit Rizki, bukan lewat SQL |
| Keputusan yang sudah diambil | Skenario 6: `reverse` sukses = `200` — keputusan Rizki 11 September 2026, dokumentasi saja |
| Masalah yang diketahui | Pengecualian tutup tahun bergantung pada bentuk baris. Jurnal `JT` yang **dibuat lewat Form Jurnal** dengan baris Pendapatan/Beban/Ekuitas saja juga akan lolos saat diajukan — tetapi jurnal semacam itu tidak menyentuh Kas, Piutang, atau Utang, dan penyimpanannya sendiri sudah melewati pemeriksaan jalur manual |
| Risiko tersisa | (1) Template yang sudah aktif lalu akunnya baru ditandai control tetap terbit — diterima `ACC-DEC-073`, dan kini terbukti di runtime (D2). (2) Jalur baru yang disusun manusia dan memanggil `CreateAsync` langsung akan lolos tanpa pemeriksaan; catatan pada `CreateAsync` memperingatkannya. (3) Penutupnya yang bersih adalah kolom asal-usul pada `AccJournal`, yang menuntut migration. (4) `JB/2026/09/00002` menahan penutupan September 2026 sampai disahkan |
| Perubahan sampingan | `NONE` pada source. Data uji dev: lihat tabel akhir 5.4 |
| Interupsi | `NONE` |
| Nama berkas | Mengikuti pola laporan Phase 2 lain (`be-acc-p2-0xx-<judul>.md`), bukan `BE-ACC-P2-012.md` persis seperti aturan suite, supaya satu folder tidak memakai dua pola |
| Status Git | Source dan laporan awal sudah di-commit Rizki sebagai `cca0957`. Pembaruan sesudahnya — laporan ini, `api-contract.md`, roadmap backend, dan traceability — belum di-commit. Perubahan `FE-ACC-P2-007` pada roadmap frontend, traceability, dan laporan frontend juga masih belum di-commit |
| Langkah berikutnya | (1) Pengguna kedua menyetujui `JB/2026/09/00002`; lalu sahkan dan buktikan penetralan S4 (D-akhir). (2) Build `Release` lalu `UnitTests.Sqlite` — 15 uji baru dan regresi — atas konfirmasi Rizki. (3) Keputusan owner atas celah acceptance (2) jalur `JT`: diterima lewat uji SQLite, atau dijadwalkan uji runtime pada tahun buku uji. (4) Ratifikasi `ACC-VALIDATION-0.7`, `ACC-API-0.11`, `ACC-PERMISSION-0.6` |

---

## 8. Keputusan status owner — 11 September 2026

Owner menetapkan pemisahan status development dan UAT. Tim UAT terpisah menjalankan UAT,
regression acceptance, dan validasi bisnis; kekurangan UAT bukan penghalang development.

| Status | Nilai | Keterangan |
| --- | --- | --- |
| Implementation | `COMPLETE` sesuai cakupan yang dibangun | Bagian 3 dan 6 |
| Developer Manual Test | `COMPLETED` | S1–S8 oleh Rizki lewat Swagger, A–D lewat API; bagian 5 |
| Automated Verification | `DEFERRED` | Build `Release`, 15 `AccControlAccountJournalGuardTests`, dan regresi `UnitTests.Sqlite` **tidak dijalankan untuk mengejar closure** — dijadwalkan pada pipeline quality/UAT terpisah |
| UAT | `HANDOFF TO UAT TEAM` | `UAT-P2-24`, `UAT-P2-25` |

**Tanda roadmap tetap 🟡.** DoD formal kartu ini menuntut "test hijau", dan butir itu belum
terpenuhi — status `DONE`/✅ tidak dipalsukan. Tanda 🟡 ini **tidak** menahan `FE-ACC-P2-007`
maupun `FE-ACC-P2-008`: keduanya dikerjakan dan diverifikasi terhadap perilaku runtime yang
sudah terbukti di bagian 5.

**Skenario S6** — `POST /journals/{id}/reverse` sukses `200` adalah perilaku yang diterima;
`ReverseAsync` tidak diubah.

**UAT follow-up.** Skenario berikut diserahkan ke tim UAT/test environment. Yang menuntut setup
database invasif **tidak** disiapkan lewat `UPDATE`/`INSERT`/`DELETE` SQL langsung:

| No | Skenario | Kenapa diserahkan |
| ---: | --- | --- |
| 1 | Jurnal penutup tahun `JT` yang menyentuh control account lolos saat diajukan — acceptance (2) | Menuntut satu tahun buku dengan seluruh periode tertutup |
| 2 | Pengesahan `JB/2026/09/00002` dan penetralan saldo Kas Kecil (S4, D-akhir) | Menunggu persetujuan pengguna kedua; `ApproveAsync` menolak pembuat menyetujui jurnalnya sendiri |
| 3 | Template yang akunnya baru ditandai control ditolak `422` saat diaktifkan — `ACC-DEC-073` (2) | Belum diuji runtime; datanya dapat disiapkan lewat layar |
| 4 | Pembalik yang barisnya diubah diperiksa seperti jurnal manual — `ACC-DEC-072` (3) | Belum diuji runtime; datanya dapat disiapkan lewat layar |

Pekerjaan tambahan pada task ini dihentikan atas instruksi owner; pengembangan berlanjut ke
`FE-ACC-P2-008`.
