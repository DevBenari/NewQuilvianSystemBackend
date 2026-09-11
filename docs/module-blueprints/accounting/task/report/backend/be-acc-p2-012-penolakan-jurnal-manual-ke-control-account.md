# Laporan Perubahan Backend — `BE-ACC-P2-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-012` |
| Judul | Penolakan jurnal manual ke control account |
| Slice | Gelombang `P2-CTRL` — control account |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-012` |
| Trace | `ACC-DEC-064`; diperluas `ACC-DEC-072` (koreksi) dan `ACC-DEC-073` (template), keduanya 11 September 2026; `FR-P2-036`, `FR-P2-037` |
| Contract version | `ACC-VALIDATION-0.6` bagian 3b (`approved`), diamandemen **usulan `ACC-VALIDATION-0.7`**; **usulan `ACC-API-0.11`** untuk `IsControlAccount` pada `/options` dan kode `422` — keduanya menunggu ratifikasi |
| Dependency | `BE-ACC-P2-004` ✅ (kolom sudah ada di database), `BE-ACC-P2-011` ✅ |
| Klasifikasi | `MEDIUM` — skor 5: repository 0, berkas diperiksa 1, berkas diubah 1 (5 source + 1 uji), logika bisnis 1, kontrak API 1, database 0, keamanan 1 (larangan kewenangan pencatatan), UI 0 |
| Task mode | `CROSS-REPO MODE` — backend `rizkiG` sebagai target tulis; frontend tidak disentuh pada task ini |
| Target tulis | `NewQuilvianSystemBackend` — source `Areas/Corporate/AccountingManagement/**`, project `UnitTests.Sqlite`, `docs/module-blueprints/accounting/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `968841e` |
| Tanggal | 11 September 2026 |
| Status | **🟡 `SEBAGIAN`** — keempat acceptance terpetakan ke source, tetapi **tidak satu pun validasi dijalankan**: build, uji, dan uji PostgreSQL dilarang/diserahkan atas instruksi owner. Lihat bagian 5 |

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
| Akun ditandai control **sesudah** draft manual tersimpan | Draft ditolak `422` saat diajukan; tetap `Draft` |
| Akun ditandai control **sesudah** template disimpan, template belum aktif | Aktivasi ditolak `422`; template tetap tidak aktif |
| Akun ditandai control **sesudah** template aktif | Template **tetap terbit** dan draftnya lolos diajukan — sisa risiko yang diterima `ACC-DEC-073` |
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
`permission-audit-matrix.md`; keputusan `ACC-DEC-064`, `072`, `073`.

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
`BE-ACC-P2-013`, `FE-ACC-P2-007`, dan `FE-ACC-P2-008`. Sesudah build, yang disentuh skill ini
hanya tanda status dan tautan bukti.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Nol endpoint baru. `422` baru pada `POST /journals`, `PUT /journals/{id}`, `POST /journals/{id}/submit`, `POST /journals/{id}/reverse` (penyesuaian), `POST`/`PUT /recurring-journals`, `PATCH /recurring-journals/{id}/activate`. Bidang baru `isControlAccount` pada respons `/chart-of-accounts/options`. Seluruhnya tercatat pada usulan `ACC-API-0.11` dan `ACC-VALIDATION-0.7` |
| Database | Nol migration, nol perubahan schema. Tidak ada eksekusi database pada task ini |
| Keamanan/Auth | Nol `[AccessPermission]` baru atau berubah. Penolakan bersifat aturan bisnis pada data, bukan hardcode peran |

---

## 4. Dokumentasi endpoint

Tidak ada endpoint baru. Endpoint yang perilakunya berubah:

#### Corporate / Accounting / Journal Management / Journal

| Method | Path | Perubahan bagi pengguna | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | `422` bila ada baris ke akun control | `Journal : Create` |
| `PUT` | `/{id}` | `422` bila ada baris ke akun control, termasuk atas draft template dan jurnal pembalik | `Journal : Update` |
| `POST` | `/{id}/submit` | `422` bila menyentuh akun control dan bukan hasil jalur otomatis | `Journal : Submit` |
| `POST` | `/{id}/reverse` | `422` bila **penyesuaian** menyentuh akun control; pembalikan penuh tetap boleh | `Journal : Reverse` |

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
| `dotnet build -c Release` | Tidak dijalankan | `NOT RUN` | Instruksi owner: *"Tanpa migration, tanpa build."* |
| 15 uji `AccControlAccountJournalGuardTests` | Ditulis, **belum pernah dikompilasi maupun dijalankan** | `NOT RUN` | Instruksi yang sama |
| Seluruh project `UnitTests.Sqlite` (regresi) | Tidak dijalankan | `NOT RUN` | Instruksi yang sama. Diperiksa secara statis: tidak ada uji lama yang menandai akun control lalu mengajukan, mengubah, atau menyesuaikan jurnal, sehingga regresi pada uji lama tidak diharapkan |
| Uji integrasi PostgreSQL (kolom Verifikasi) | Diserahkan kepada Rizki | `NOT RUN` | Rizki menguji manual lewat Swagger atau layar; skenarionya di bawah |

Uji manual: `REQUIRED`.

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

### 5.1 Skenario uji manual untuk Rizki (Swagger)

**Prasyarat.** Backend dijalankan ulang dengan kode ini. Periode September 2026 berstatus terbuka.
Enam akun control yang sudah ada: `1-1002` Kas Kasir, `1-1003` Kas Kecil, `1-2001`/`1-2002`
Piutang, `2-1001`/`2-1002` Utang. Skenario 3 dan 4 butuh **pengguna kedua** untuk menyetujui,
karena pembuat tidak boleh menyetujui jurnalnya sendiri (`ACC-DEC-016`).

| No | Langkah | Hasil yang diharapkan | Acceptance |
| ---: | --- | --- | --- |
| 1 | `POST /journals` — debit `1-1002`, kredit akun pendapatan | `422`, pesan menyebut "1-1002 Kas Kasir" dan "jurnal manual"; tidak ada nomor jurnal terpakai | (1) |
| 2 | `POST /journals` — debit `1-1001` Kas Besar, kredit pendapatan | `201` draft | (3) |
| 3 | `PUT /journals/{id draft no. 2}` — ganti Kas Besar menjadi `1-1002` | `422`; baris lama tetap | (4) |
| 4 | Lepas penanda `1-1003` (`PUT` akun, `isControlAccount: false`), buat jurnal Kas Kecil lawan pendapatan, ajukan, setujui (pengguna kedua), sahkan. Tandai lagi `1-1003` | Jurnal Disahkan yang menyentuh akun control | Penyiapan |
| 5 | `POST /journals/{id no. 4}/reverse` — `correctionType: 2` (penyesuaian) dengan satu baris ke `1-1003` | `422`, "jurnal penyesuaian" | `ACC-DEC-072` |
| 6 | `POST /journals/{id no. 4}/reverse` — `correctionType: 1` (pembalikan penuh) | `201`, status menunggu persetujuan | `ACC-DEC-072` |
| 7 | `POST /recurring-journals` — baris ke `1-1002` | `422`, "template jurnal berulang" | `ACC-DEC-073` |
| 8 | `GET /chart-of-accounts/options?legalEntityId=…` | `1-1002` **tetap ada** dengan `isControlAccount: true`; `1-1001` bernilai `false` | `/options` |

Tutup tahun tidak disarankan diuji di basis data dev: ia menuntut seluruh periode setahun
tertutup. Buktinya sementara hanya uji SQLite `JurnalPenutupTahun_TidakTerkenaLarangan`.

**Data uji yang akan tertinggal.** Jurnal Disahkan pada skenario 4 dan jurnal pembalik pada
skenario 6 **tidak dapat dihapus lewat API** — jurnal Disahkan permanen (`ACC-DEC-006`). Draft
dapat dihapus lewat `DELETE /journals/{id}`. Pakai keterangan berawalan `UJI-012` supaya mudah
dikenali. Pastikan penanda `1-1003` dikembalikan ke `true` di akhir.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Baris jurnal **manual** ke akun ber-`IsControlAccount = true` ditolak `422`, dan pesannya menyebut akun mana | **Terpetakan ke source**, belum terbukti | `CreateManualAsync`, `UpdateAsync`, `SubmitAsync` → `AlasanControlAccountAsync`; uji ditulis, `NOT RUN` |
| (2) Jurnal dari kejadian akuntansi, dari template berulang, dan jurnal penutup tahun **tidak** terkena aturan ini | **Terpetakan ke source**, belum terbukti; kejadian akuntansi belum dapat diuji | `CreateAsync` tanpa pemeriksaan; `BerasalDariJalurOtomatisAsync`. Jalur `P2-1` belum ada di kode |
| (3) Akun non-control tetap dapat dijurnal manual | **Terpetakan ke source**, belum terbukti | `AlasanControlAccountAsync` mengembalikan `null` bila tidak ada akun control |
| (4) Jurnal manual yang sudah ada tidak ikut ditolak saat diubah, kecuali barisnya menyentuh control account | **Terpetakan ke source**, belum terbukti | Pemeriksaan `UpdateAsync` hanya membaca baris yang dikirim |
| DoD — endpoint berjalan | **Belum terpenuhi** | Tidak dibangun maupun dijalankan |
| DoD — test hijau | **Belum terpenuhi** | Uji `NOT RUN` |
| DoD — laporan task tertulis | Terpenuhi | Berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Kode dan uji belum pernah dikompilasi.** Kesalahan ketik atau nama bidang yang keliru baru ketahuan saat Rizki menjalankan `dotnet build -c Release -p:RunAnalyzers=false`. Uji ditulis meniru fixture `AccYearEndClosingTests` dan `AccRecurringJournalGenerateTests`, dan setiap DTO serta namespace-nya dicocokkan ke source |
| Peringatan governance | Pada langkah 1 sesi ini, dua kueri **read-only** dijalankan ke `QuilvianNewDevRizki` (menghitung template dan jurnal yang menyentuh akun control, serta memeriksa `JB/2026/09/00001`) **sebelum** larangan eksekusi database langsung pada `CLAUDE.md` backend terbaca. Nol tulisan. Tidak diulang sesudahnya |
| Masalah yang diketahui | Pengecualian tutup tahun bergantung pada bentuk baris. Jurnal `JT` yang **dibuat lewat Form Jurnal** dengan baris Pendapatan/Beban/Ekuitas saja juga akan lolos saat diajukan — tetapi jurnal semacam itu tidak menyentuh Kas, Piutang, atau Utang, dan penyimpanannya sendiri sudah melewati pemeriksaan jalur manual |
| Risiko tersisa | (1) Template yang sudah aktif lalu akunnya baru ditandai control tetap terbit — diterima `ACC-DEC-073`. (2) Jalur baru yang disusun manusia dan memanggil `CreateAsync` langsung akan lolos tanpa pemeriksaan; catatan pada `CreateAsync` memperingatkannya. (3) Penutupnya yang bersih adalah kolom asal-usul pada `AccJournal`, yang menuntut migration |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Nama berkas | Mengikuti pola laporan Phase 2 lain (`be-acc-p2-0xx-<judul>.md`), bukan `BE-ACC-P2-012.md` persis seperti aturan suite, supaya satu folder tidak memakai dua pola |
| Status Git | 5 source dan 7 dokumen `M`; `Tests/.../AccControlAccountJournalGuardTests.cs` dan laporan ini `??`. Dua laporan frontend `fe-acc-p2-003`/`004` yang `??` berasal dari sesi sebelumnya dan tidak disentuh. Tidak ada stage maupun commit |
| Langkah berikutnya | (1) Rizki membangun dan menjalankan `UnitTests.Sqlite`, lalu skenario 5.1. (2) Ratifikasi `ACC-VALIDATION-0.7`, `ACC-API-0.11`, `ACC-PERMISSION-0.6`. (3) `FE-ACC-P2-007` memakai `isControlAccount` dari `/options` |
