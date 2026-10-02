# Finance Management — Roadmap Pengiriman

## 1. Identitas

```yaml
roadmap_id: FIN-ROADMAP-001
roadmap_revision: 14
roadmap_status: ACTIVE
blueprint_id: FIN-BP-001
blueprint_revision: 10
blueprint_status: approved
created_at: 2026-09-20T00:00:00+07:00
planned_by: /quilvian-engineering-skills:plan-module-delivery
children:
  backend: roadmap/01-backend-roadmap.md — FIN-ROADMAP-BE-001 revisi 10
  frontend: roadmap/02-frontend-roadmap.md — FIN-ROADMAP-FE-001 revisi 8
roadmap_revision_14_note: >
  Revisi 14 (30 September 2026) MENYELARASKAN tabel ringkasan frontend (Tabel 4.2) dan tabel epic
  (Tabel 4.3): status FE-FIN-006 dimutakhirkan ke ✅ Selesai (23 September 2026), tanda blocker ⛔
  pada FE-FIN-007 dicabut menyusul penutupan FIN-OQ-017 serta selesainya prasyarat backend (BE-FIN-024 ✅
  dan BE-FIN-025 ✅ pada 29 September 2026), dan catatan EPIC FIN-14 diselaraskan dengan PRD Bagian 20.2
  (sebab OPEN DECISION kini FIN-OQ-027, 030..032, 034; task BE-FIN-022..026 dan FE-FIN-007 bebas dieksekusi
  di luar nomor gelombang).
roadmap_revision_13_note: >
  Revisi 13 (30 September 2026) MENAMBAHKAN task frontend FE-FIN-015 (Layar/Tab Pemantauan & Pemicu Snapshot
  Saldo Subledger Bulanan untuk 4 Control Account) menyusul selesainya task backend BE-FIN-048 dan BE-FIN-049,
  menjawab kebutuhan operasional staf Finance/Accounting dalam memastikan kelengkapan 4 akun kontrol
  (Kas Kasir, Kas Kecil, Piutang Pasien & Penjamin, dan Utang Supplier) dan pemicu penutupan saldo subledger
  akhir bulan secara manual bila diperlukan (ACC-DEC-108, FIN-DEC-090).
roadmap_revision_12_note: >
  Revisi 12 (30 September 2026) MENAMBAHKAN task backend BE-FIN-048 (pengetatan validasi pesan saldo subledger)
  dan BE-FIN-049 (layanan kalkulasi snapshot saldo subledger bulanan untuk 4 control account) menyusul surat
  susulan Accounting evidence/15, surat balasan resmi finance/evidence/21, dan keputusan FIN-DEC-090 s/d FIN-DEC-093.
roadmap_revision_11_note: >
  Revisi 11 (30 September 2026) MENANDAI SELESAI (✅) seluruh task backend BE-FIN-028 s/d BE-FIN-041,
  BE-FIN-043, BE-FIN-044, BE-FIN-045, BE-FIN-046, BE-FIN-047 menyusul konfirmasi berhasilnya dotnet build
  (PASS 0 error) dan eksekusi skrip migrasi database fisik (BE-FIN-041, BE-FIN-038, BE-FIN-043 via
  be-fin-041-038-043-schema-migration-dbeaver.sql) oleh pengguna. Task BE-FIN-042 berstatus 🟡 (rename 6
  controller dan skrip SQL migrasi SysAccessPolicy selesai, seeder peran payung menunggu FIN-OQ-039).
  Pekerjaan EPIC FIN-12 (worker pengiriman outbox otomatis ke Accounting) DITUNDA / TIDAK DIBUAT atas
  keputusan eksplisit pemilik produk ("karena system saya belum perlu itu").
roadmap_revision_10_note: >
  Revisi 10 (29 September 2026) MENCABUT TANDA BLOCKER ⛔ PADA BE-FIN-047 menyusul disahkannya
  FIN-DES-064 dan FIN-DES-065 via FIN-DEC-080 dan FIN-DEC-081 (Amendment Pass). Pemicu kejadian
  PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT resmi disahkan: mutasi BilDepositMovement bertipe RELEASE
  yang berpasangan dengan REVERSAL pada SettlementId yang sama. Pemicu kode pengembalian uang muka
  dipersempit murni ke BilRefundCase EXECUTED. Task BE-FIN-047 kini SIAP DIKERJAKAN pada gelombang
  R6-4 setelah BE-FIN-046. Gap coverage ditutup dan keterkaitan traceability diperbarui.
roadmap_revision_9_note: >
  Revisi 9 (29 September 2026) menurunkan desain AMENDMENT REVISI 6/7 (FIN-DES-051..060, approved
  28 September 2026) yang belum punya task, dan memperbaiki isi task lama yang acceptance
  criteria-nya bertentangan dengan desain itu.
  NOL task frontend BARU ditambahkan, tetapi roadmap frontend TETAP BERGERAK: FE-FIN-007 diperbaiki
  isinya (tanda ⛔ dicabut, outcome "tujuh jenis kejadian" menjadi katalog final, dua sebab baris
  intake ERROR ditambahkan) dan menerima satu prasyarat baru BE-FIN-025.
  Tiga task lama diperbaiki: BE-FIN-023 (daftar tertutup kode penanda bernilai nol + properti
  Components dihilangkan; acceptance criteria "selisih kas -30.000" DICABUT), BE-FIN-025 (kode
  selisih kas dipecah dua berkunci BilCashierShift.Id, refund SETTLEMENT masuk jalur yang sudah ada,
  REFERRED_OUTPATIENT_ADMIN menjadi baris intake ERROR), BE-FIN-026 (rumpun kedua: baris outbox
  bernama pendek dihitung dan dilaporkan). BE-FIN-024 diperiksa dan TIDAK perlu diubah.
  Tiga task baru: BE-FIN-045 (penanda shift tertutup dan pembaliknya, worker digerbang FIN-OQ-035),
  BE-FIN-046 (pembalikan tender top-up deposit, pemicunya sudah ada sejak Billing menutup
  FIN-OQ-034), dan BE-FIN-047 ⛔ (pembalikan pemakaian uang muka deposit).
  SATU BLOCKER BARU yang ditemukan pass ini, dan ia ada di dalam blueprint sendiri: FIN-DES-057
  menetapkan pemicu PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT sebagai mutasi REVERSAL atas ALLOCATION,
  sedangkan solusi Billing yang disahkan FIN-DEC-077 dan sudah terimplementasi memakai mutasi
  RELEASE. Satu MovementType RELEASE karena itu kini membawa DUA lawan jurnal berbeda, dan cara
  Finance membedakannya belum digambar. BE-FIN-047 ⛔ menunggu amendment /design-business-module.
  SATU HAL YANG PERLU KONFIRMASI PEMILIK: eksekusi migration BE-FIN-022. Pengguna menyatakan sebuah
  migration berhasil pada 29 September 2026, tetapi dua berkas menunggu pada hari yang sama
  (BE-FIN-022 dan BE-FIN-041); status BE-FIN-022 sengaja TIDAK dinaikkan tanpa kepastian.

backend_commit_sha: 7811c048
backend_commit_sha_previous_planning: 96bf9746fedb63119a317a53758b0f0748ec7ad1
backend_commit_sha_baseline: 09101d0581695e20345a9efa8af3fce7c38b1ae4
backend_branch: Yasmina
frontend_commit_sha: 49b59cfaa
frontend_commit_sha_previous: abed49b03
frontend_branch: yasmina

input_hashes:
  00-interview-decisions.md: 5fd9d74e22c04b0bb8fcba27a6a6f2b9432e4160eeba83db1d0a8eda697b45dc
  01-existing-capability-map.md: 42e95d720d42a288220f3919cd2e80fa2fe8361cb7362df1cbc92f4431e7e2b9
input_hashes_note_revision_5: >
  Keduanya BERGERAK lagi 25 September 2026, bukan drift: decision log menerima FIN-DEC-045..056
  dan FIN-OQ-022..026 (dibuka/ditutup pass perencanaan dan /grill-me susulan); capability map menerima
  bagian 12-14 (FIN-CAP-026..036). Nilai revisi 4 roadmap: cca250f2... dan 2674907a...
roadmap_revision_8_note: >
  Revisi 8 (28 September 2026) menambahkan empat task baru untuk AMENDMENT REVISI 6 dan 8 blueprint:
  BE-FIN-042 (penyelarasan 6 controller legacy Finance ke nama kanonikal, ekspansi payung Finance.AP/AR ke granular di seeder, dan skrip SQL idempotent migrasi hak peran SysRolePermissions — FIN-CQ-08, FIN-DES-061..063),
  BE-FIN-043 (penambahan kolom PPNAmount pada FinSupplierReturn dan kalkulasi deposit retur saat CONFIRMED membawa PPN — FIN-DES-055),
  BE-FIN-044 (penyelarasan konstanta EventTypeCode pada 4 service call points ke katalog resmi Accounting, hapus 5 alias lama — FIN-DES-058), dan
  FE-FIN-014 (penyelarasan menu sidebar navigasi Finance: submenu Pembelian, relabel Faktur Pembelian, dan butir flat Tagihan Gabungan Penjamin — FIN-DEC-060, FIN-DES-060).
  Seluruh kontrak turunan diperbarui ke FIN-PERM-1.3, FIN-VAL-1.4, FIN-INTEGRATION-1.4, FIN-MVP-1.5.
  Total task kini 44 backend (BE-FIN-001..044) dan 14 frontend (FE-FIN-001..014).
roadmap_revision_7_note: >
  Revisi 7 (26 September 2026) menurunkan AMENDMENT REVISI 5 blueprint (FIN-DES-045..050,
  approved dan kontraknya locked 26 September 2026 — dipilih owner saat memanggil pass ini).
  BE-FIN-036 dan FE-FIN-010 DIBUKA. Satu task baru: BE-FIN-041 (kolom DepositAppliedAmount pada
  FinPayment yang sudah berjalan). Enam task backend diperbarui ke bentuk revisi 5. Nol task
  REV-4 yang ⛔ selain yang bergantung pada FE-FIN-004. Satu risiko baru dicatat: layar
  pembayaran supplier yang ada memakai rute alias, bukan rute kanonik tempat endpoint deposit.
roadmap_revision_6_note: >
  Revisi 6 (25 September 2026, sesudah /grill-me closure pass FIN-DEC-057..060) menutup
  FIN-OQ-022/023/024/025. DUA task DIBUKA: BE-FIN-040 dan FE-FIN-013 (FIN-OQ-024 tertutup sisi
  Finance — kode POTONGAN-PIUTANG-NON-TUNAI diusulkan, FIN-DEC-058; ratifikasi Rizki dicatat
  terpisah FIN-OQ-026, hanya menahan worker pola FIN-DEC-056). SATU task TETAP ⛔: BE-FIN-036
  dan turunannya FE-FIN-010 — FIN-OQ-023 tertutup sisi keputusan bisnis (FIN-DEC-057: Deposit
  Retur jadi baris alokasi non-tunai di FinPayment), tetapi konsekuensi skema
  FinPaymentAllocation/FinSupplierReturnDepositUsage BELUM digambar /design-business-module.
  Endpoint GET /purchasing/reports/aging DICABUT (FIN-DEC-059). Label menu DITETAPKAN
  (FIN-DEC-060). Task lama TIDAK dinomori ulang.
input_hashes_note: >
  Kedua hash BERGERAK pada 25 September 2026 dan keduanya BUKAN drift: decision log menerima
  FIN-DEC-030..044, dan capability map menerima bagian 9.3 (impact scan d6cdfaf9) beserta
  pembaruan FIN-CAP-007/008/009 dan penambahan FIN-CAP-022..025. Nilai lama:
  74529813... dan 0576bf65...
roadmap_revision_4_note: >
  Revisi 4 (25 September 2026) menambahkan enam task untuk AMENDMENT REVISI 3 blueprint:
  BE-FIN-022..026 pada roadmap backend, dan FE-FIN-007 pada roadmap frontend. Tidak ada task
  lama yang dinomori ulang atau diturunkan statusnya. Grafik urutan dependency kedua roadmap
  anak dirapikan menjadi pohon teks sesuai rules/rule-output/grafik-dependency-roadmap.md.

approval_basis: >
  Yasmin (Product/Domain Owner Finance), 20 September 2026 — dua approval terpisah:
  (1) keputusan arsitektur FIN-DES-001..024; (2) cakupan MVP dan urutan gelombang pada
  04-prd-to-mvp.md bagian 7, 8, dan 20.1.
  Ditambah pada hari yang sama, sesudah revisi 1 roadmap ini:
  (3) FIN-DES-025..028; (4) penguncian tujuh kontrak turunan ke 1.0.
  25 September 2026: (5) FIN-DES-029..036 dan penguncian lima kontrak ke 1.1 ("Saya approve
  semua"); (6) FIN-DEC-056 — gerbang FIN-OQ-020 dipersempit; (7) FIN-DES-037..044 dan
  penguncian FIN-API-1.1, FIN-INTEGRATION-1.2, FIN-STATE-1.2, FIN-VAL-1.2, FIN-PERM-1.1,
  FIN-TEST-1.2, FIN-MVP-1.3 — dipilih owner atas pertanyaan eksplisit saat memanggil pass ini.
  Koreksi pass ini atas FIN-DES-039, C.6, dan C.7 menyelaraskan desain dengan keputusan dan
  source yang sudah berjalan; ia tidak mengubah keputusan apa pun yang disetujui.
```

Roadmap ini menurunkan cakupan yang sudah disetujui menjadi task, dan menyebut dengan tegas apa
yang tertahan beserta pemiliknya. **Pembaruan 23 September 2026**: berkas ini semula (revisi 3,
20 September 2026) tidak menyatakan satu pun pekerjaan selesai — sejak itu 14 dari 21 task
`BE-FIN-*` sudah ✅ selesai dan UI brief frontend sudah closed; lihat bagian 4 dan 5 untuk
rinciannya per task, dan laporan tracked masing-masing untuk buktinya.

Berkas ini adalah **payung**. Task-nya sendiri ada di dua berkas tersendiri:

| Berkas | Isi |
|---|---|
| `01-backend-roadmap.md` | 44 task `BE-FIN-*` (`027`..`041` revisi 4-5, `042`..`044` ditambahkan revisi 6/8), urutan eksekusi, rincian per gelombang, prasyarat, DoD backend |
| `02-frontend-roadmap.md` | 14 task `FE-FIN-*` (`008`..`013` revisi 5, `014` ditambahkan revisi 6/8), yang MUST diputuskan UI brief, `DEV_DISCRETION`, DoD frontend |

Yang tetap di sini: identitas, penguncian kontrak, gelombang, traceability lintas keduanya,
coverage gap, dan risiko.

## 2. Penguncian versi kontrak — **DISETUJUI 20 September 2026**

Owner menyetujui usulan penguncian revisi 1 apa adanya. Ketujuh kontrak kini `locked`.

| Kontrak | Dari | Terkunci pada | Cakupan penguncian |
|---|---|---|---|
| `contracts/api-contract.md` | `FIN-API-0.1` | `FIN-API-1.0` ✅ | Seluruh permukaan yang berakar `FIN-DES-001`..`028` |
| `contracts/state-transition-matrix.md` | `FIN-STATE-0.1` | `FIN-STATE-1.0` ✅ | Idem |
| `contracts/validation-matrix.md` | `FIN-VAL-0.1` | `FIN-VAL-1.0` ✅ | Idem |
| `contracts/permission-audit-matrix.md` | `FIN-PERM-0.1` | `FIN-PERM-1.0` ✅ | Idem |
| `contracts/integration-contract.md` | `FIN-INTEGRATION-0.1` | `FIN-INTEGRATION-1.0` ✅ | Intake Billing dan kotak keluar Accounting; **tidak** termasuk `BilCollectionHandoff` |
| `testing/acceptance-test-matrix.md` | `FIN-TEST-0.1` | `FIN-TEST-1.0` ✅ | Idem |
| `04-prd-to-mvp.md` | `FIN-MVP-0.1` | `FIN-MVP-1.0` ✅ | Bagian 7, 8, dan 20.1 yang sudah disetujui |

Rumpun Payable **ikut terkunci**: dasar pengecualiannya pada revisi 1 adalah status `draft`
`FIN-DES-025`..`028`, dan status itu sudah tercabut oleh approval hari ini.

**Tiga permukaan tetap tidak terkunci**, dan alasannya bukan status keputusan melainkan
ketergantungan pada pihak lain:

| Permukaan | Menunggu siapa |
|---|---|
| `BilCollectionHandoff` | Owner Billing (`FIN-DEC-005`) |
| Perluasan `BilArHandoff` untuk manfaat karyawan | Owner Billing + HR (`FIN-DEC-006`, `FIN-DEC-016`) |
| Pengiriman kejadian ke Accounting | Rizki — endpoint penerima belum ada (`FIN-CAP-018`) |

Ketiganya kelak **menambah permukaan baru** yang dikunci tersendiri; kedatangannya tidak
menaikkan versi ketujuh kontrak di atas.

**Penguncian revisi 4 — DISETUJUI 25 September 2026.** Owner mengunci kontrak AMENDMENT REVISI 4
bersama approval `FIN-DES-037`..`044`:

| Kontrak | Dari | Terkunci pada |
|---|---|---|
| `contracts/api-contract.md` | `FIN-API-1.0` | `FIN-API-1.1` ✅ |
| `contracts/integration-contract.md` | `FIN-INTEGRATION-1.1` | `FIN-INTEGRATION-1.2` ✅ |
| `contracts/state-transition-matrix.md` | `FIN-STATE-1.1` | `FIN-STATE-1.2` ✅ |
| `contracts/validation-matrix.md` | `FIN-VAL-1.1` | `FIN-VAL-1.2` ✅ |
| `contracts/permission-audit-matrix.md` | `FIN-PERM-1.0` | `FIN-PERM-1.1` ✅ |
| `testing/acceptance-test-matrix.md` | `FIN-TEST-1.1` | `FIN-TEST-1.2` ✅ — dua baris uji batas Rp 50.000.000 ditambahkan pass ini |
| `04-prd-to-mvp.md` | `FIN-MVP-1.1` | `FIN-MVP-1.3` ✅ |

**Penguncian revisi 5 — DISETUJUI 26 September 2026** bersama `FIN-DES-045`..`050`:
`FIN-API-1.2`, `FIN-INTEGRATION-1.3`, `FIN-STATE-1.3`, `FIN-VAL-1.3`, `FIN-PERM-1.2`,
`FIN-TEST-1.3`, `FIN-MVP-1.4` ✅. Baris yang dicabut di tabel versi sebelumnya diberi coretan dan
rujukan, tidak dihapus.

Satu permukaan revisi 4 tetap **tidak** dianggap final walau kontraknya terkunci: endpoint
`GET /purchasing/reports/aging` (`FIN-OQ-025`). Bila dicabut, kontraknya direvisi bernomor.

**Penguncian revisi 6 — DISETUJUI 28 September 2026** bersama ratifikasi Accounting, AMENDMENT REVISI 6 dan 8:
`FIN-PERM-1.3` (pemetaan granular FIN-CQ-08, payung `Finance.AP`/`Finance.AR` dipertahankan untuk frontend),
`FIN-VAL-1.4` (`PPNAmount >= 0` pada `FinSupplierReturn`, deposit retur membawa PPN saat `CONFIRMED`),
`FIN-INTEGRATION-1.4` (penyelarasan nama kode katalog outbox resmi Accounting dan pencabutan 5 alias lama),
`FIN-MVP-1.5` ✅.

**Konsekuensi penguncian:** kerja paralel backend–frontend kini **diizinkan** untuk seluruh task
yang kontraknya terkunci. Yang masih menahan task `FE-FIN-*` hanyalah ketiadaan UI brief —
dan itu keputusan Product Owner, bukan keterikatan kontrak.

## 3. Gelombang

Mengikuti `04-prd-to-mvp.md` bagian 20.1 apa adanya. Tidak ada epic yang dipindah, ditambah,
atau dihapus.

| Gelombang | Epic | Task | Keadaan |
|---|---|---|---|
| `MVP-0` | `FIN-01` | `BE-FIN-001`..`004` · `FE-FIN-001` | 🟡 Backend selesai 23 September 2026 (4 dari 4 task); `FE-FIN-001` source lengkap 23 September 2026, `npm run lint:errors`/`npm run build` belum dijalankan — [laporan](../task/report/frontend/FE-FIN-001.md) |
| `MVP-1` | `FIN-02`, `FIN-03` | `BE-FIN-005`..`009` · `FE-FIN-002` | ✅ Selesai 23 September 2026 — Backend (5 dari 5 task) & Frontend FE-FIN-002 selesai (lint & build PASS) |
| `MVP-5` | `FIN-11` | `BE-FIN-010`..`012` · `FE-FIN-006` | ✅ Selesai 23 September 2026 — Backend (3 dari 3 task) & Frontend FE-FIN-006 selesai (lint & build PASS) |
| `MVP-4` | `FIN-10` | `BE-FIN-013`..`015` · `FE-FIN-003` | ✅ Selesai 23 September 2026 — Backend (3 dari 3 task) & Frontend FE-FIN-003 selesai (lint & build PASS) |
| `MVP-2` | `FIN-05` | `BE-FIN-016`..`017` · `FE-FIN-004` | ✅ Backend selesai 23 September 2026 — owner Billing sudah menjawab, reversal diperbaiki; 🟡 `FE-FIN-004` sebagian 30 September 2026 — kelima layar selesai (Penerimaan, alokasi, buku register, rekonsiliasi shift), menunggu verifikasi runtime ([laporan](../task/report/frontend/FE-FIN-004.md)) |
| `MVP-3` | `FIN-06` | `BE-FIN-018` | ✅ Selesai 23 September 2026 — alokasi/pembalikan dan controller selesai |
| `POST-MVP` | `FIN-07` | `BE-FIN-019` | ✅ Selesai 23 September 2026 — entity, service, dan controller selesai |
| `POST-MVP` | `FIN-09` | `BE-FIN-020` | ✅ Selesai 23 September 2026 — entity, service, dan controller selesai; `FIN-OQ-010` (ambang nominal) tetap terbuka |
| `POST-MVP` | `FIN-08` | `BE-FIN-021` | 🟡 Sebagian 22 September 2026 — entity+configuration+migration selesai, intake BLOCKED `BE-MDF-014` |
| `POST-MVP` | `FIN-13` | `FE-FIN-005` | ✅ Selesai 23 September 2026 — Rute /finance/petty-cash-voucher, menu sidebar, & re-export bridges (lint & build PASS) |
| `POST-MVP` (`REV-4`) | `FIN-15` (menggantikan `FIN-07`) | `BE-FIN-027`..`037` · `FE-FIN-008`..`011` | ✅ Backend selesai 30 September 2026 (`BE-FIN-027`..`037`, `dotnet build` PASS); FE menunggu wewenang tulis frontend |
| `POST-MVP` (`REV-4`) | `FIN-16` | `BE-FIN-038`, `039` · `FE-FIN-012` | ✅ Backend selesai 30 September 2026 (skema migrasi DBeaver aktif, `dotnet build` PASS); 🟡 `FE-FIN-012` sebagian 30 September 2026 — source lengkap, lint/build PASS, verifikasi manual/runtime `NOT FEASIBLE` (migrasi `BE-FIN-038` belum dieksekusi ke database) |
| `POST-MVP` (`REV-4`) | `FIN-17` | `BE-FIN-040` · `FE-FIN-013` | ✅ Backend selesai 30 September 2026 (skema migrasi DBeaver aktif, `dotnet build` PASS); 🟡 `FE-FIN-013` sebagian 30 September 2026 — baris potongan ditambahkan ke modal alokasi, lint/build PASS ([laporan](../task/report/frontend/FE-FIN-013.md)) |
| `POST-MVP` (`REV-6/8`) | Penyelarasan Hak Akses (FIN-CQ-08), PPN Retur, Katalog Akuntansi, Penanda Shift, Pembalikan Deposit | `BE-FIN-042`..`047` · `FE-FIN-014` | ✅ Backend `BE-FIN-043`..`047` selesai 30 September 2026 (`dotnet build` PASS, migrasi DBeaver aktif); `BE-FIN-042` 🟡 selesai rename 6 controller dan skrip SQL peran; 🟡 `FE-FIN-014` sebagian 30 September 2026 — source selesai, lint/build PASS, verifikasi manual `NOT FEASIBLE` ([laporan](../task/report/frontend/FE-FIN-014.md)) |
| **Di luar gelombang** | `FIN-04`, `FIN-12` | — | `FIN-04` OPEN DECISION; `FIN-12` (worker pengiriman outbox otomatis ke Accounting) ditunda atas keputusan pemilik ("karena system saya belum perlu itu") |

### 3.1 Catatan urutan `MVP-4`

Bagian 20.1 menyatakan `MVP-4` dapat dikerjakan **lebih dahulu** bila `BilCollectionHandoff`
belum tersedia. Roadmap ini mengikuti itu, dengan satu batas yang MUST dihormati:

`FinanceCashManagementService` menghitung kas tersedia dari **kas kasir** (`FIN-CAP-006`,
`BilCashierShift`) — bukan dari `FinReceipt`. Selama `MVP-2` belum ada, task `BE-FIN-014`
MUST membaca kas kasir langsung dan MUST NOT membuat jalur sementara yang kelak dibongkar.
Bila implementer menemukan bahwa angka kas tersedia ternyata menuntut `FinReceipt`, itu temuan
yang MUST dilaporkan balik ke pass desain, bukan diselesaikan dengan improvisasi.

## 4. Indeks task

Rincian lengkap 11 kolom ada di berkas anak. Tabel ini hanya indeks.

### 4.1 Backend — `01-backend-roadmap.md`

| Task | Outcome ringkas | Gelombang | Keadaan |
|---|---|---|---|
| `BE-FIN-001` | Pendaftaran enam submodul ke registry | `MVP-0` | ✅ Selesai 21 September 2026 — [laporan](../task/report/backend/BE-FIN-001.md) |
| `BE-FIN-002` | Entity dan configuration data induk | `MVP-0` | ✅ Selesai 23 September 2026 — `MstBank` tidak dibuat baru (dipakai ulang dari Administrator), 3 entity lain selesai; `dotnet build` PASS — [laporan](../task/report/backend/BE-FIN-002.md) |
| `BE-FIN-003` | Migration `AddFinanceMasterData` | `MVP-0` | ✅ Selesai 23 September 2026 — file migration (3 tabel) dibuat tangan, sudah dieksekusi ke database — [laporan](../task/report/backend/BE-FIN-003.md) |
| `BE-FIN-004` | API data induk | `MVP-0` | ✅ Selesai 23 September 2026 — 2 dari 3 controller (grup Bank sengaja dilewati), diuji end-to-end — [laporan](../task/report/backend/BE-FIN-004.md) |
| `BE-FIN-005` | Pintu masuk fakta Billing | `MVP-1` | ✅ Selesai 23 September 2026 — entity+configuration selesai; service konsumen dibangun `BE-FIN-009` — [laporan](../task/report/backend/BE-FIN-005.md) |
| `BE-FIN-006` | Entity buku piutang | `MVP-1` | ✅ Selesai 23 September 2026 — 5 entity+configuration selesai, invariant seimbang sudah check constraint — [laporan](../task/report/backend/BE-FIN-006.md) |
| `BE-FIN-007` | Migration intake dan piutang | `MVP-1` | ✅ Selesai 23 September 2026 — file migration dieksekusi (6 dari 8 tabel; 2 tabel Collection dibangun terpisah di `BE-FIN-016`) — [laporan](../task/report/backend/BE-FIN-007.md) |
| `BE-FIN-008` | Layanan piutang dan umur piutang | `MVP-1` | ✅ Selesai 23 September 2026 — `FinanceReceivableService` selesai; 3 temuan kontrak tetap terbuka untuk ratifikasi (bukan blocker) — [laporan](../task/report/backend/BE-FIN-008.md) |
| `BE-FIN-009` | API intake dan piutang | `MVP-1` | ✅ Selesai 23 September 2026 — Receivable API selesai; 2 asumsi bisnis Billing Intake ditutup bukti source (`BillingArApHandoffService.cs`, `RegPatientEncounter.PatientId`) — [laporan](../task/report/backend/BE-FIN-009.md) |
| `BE-FIN-010` | Kotak keluar kejadian Accounting | `MVP-5` | ✅ Selesai 23 September 2026 — entity, configuration, dan migration `AddFinanceAccountingOutbox` dieksekusi; `FinSubledgerPeriodBalance` sengaja dilewati (bukan Cakupan, menunggu `FIN-OQ-011`) — [laporan](../task/report/backend/BE-FIN-010.md) |
| `BE-FIN-011` | Outbox ikut transaksi pemanggil | `MVP-5` | ✅ Selesai 23 September 2026 — `FinanceAccountingOutboxService` selesai, diperluas ke 3 pemanggilan nyata atas otorisasi eksplisit, QBE `PASS` (47 berkas) — [laporan](../task/report/backend/BE-FIN-011.md) |
| `BE-FIN-012` | Pantauan kejadian (baca saja) | `MVP-5` | ✅ Selesai 23 September 2026 — `FinanceAccountingEventsController` (4 endpoint `GET`) dan `FinanceAccountingEventService` selesai, QBE `PASS` (50 berkas) — [laporan](../task/report/backend/BE-FIN-012.md) |
| `BE-FIN-013` | Entity kas dan setoran | `MVP-4` | ✅ Selesai 23 September 2026 — entity `FinBankDeposit` dan `FinDailyCashSnapshot` selesai beserta EF configuration dan migration `AddFinanceCashManagement` dieksekusi, QBE `PASS` — [laporan](../task/report/backend/BE-FIN-013.md) |
| `BE-FIN-014` | Kas tersedia dan penutupan harian | `MVP-4` | ✅ Selesai 23 September 2026 — `FinanceCashManagementService` selesai beserta DTO dan registrasi DI, saldo dihitung saat posting (`Serializable` + advisory lock), pembekuan penutupan kas harian, kas kecil tidak mengganggu kas kasir, QBE `PASS` — [laporan](../task/report/backend/BE-FIN-014.md) |
| `BE-FIN-015` | API setoran dan kas harian | `MVP-4` | ✅ Selesai 23 September 2026 — `FinanceBankDepositsController` (7 endpoint) dan `FinanceDailyCashController` (4 endpoint) selesai, kepatuhan `role-access-rules.md`, QBE `PASS`, diuji end-to-end — [laporan](../task/report/backend/BE-FIN-015.md) |
| `BE-FIN-016` | Penerimaan dari tender kasir | `MVP-2` | ✅ Selesai 23 September 2026 — `FinReceipt`/`FinReceiptAllocation` selesai; `FinanceBillingIntakeService` diperluas untuk `HandoffType = COLLECTION`; jalur `REVERSED` diperbaiki (konflik constraint, bagian 1.5 laporan) — [laporan](../task/report/backend/BE-FIN-016.md) |
| `BE-FIN-017` | Pembagian bayar-vs-piutang | `MVP-2` | ✅ Selesai 23 September 2026 — `FinanceReceiptService` dibangun (refactor `BE-FIN-016` + `GetInvoiceBreakdownAsync` untuk `FR-FIN-035`); `CreateReversalReceiptAsync` diimplementasikan penuh; alokasi manual maker-checker cakupan `BE-FIN-018` — [laporan](../task/report/backend/BE-FIN-017.md) |
| `BE-FIN-018` | Alokasi, koreksi, penghapusan | `MVP-3` | ✅ Selesai 23 September 2026 — `ApplyAllocationAsync`/`ReverseAllocationAsync` (`FinanceReceivableService`) dan `AllocateAsync`/`ReverseAllocationAsync` (`FinanceReceiptService`) selesai (FR-FIN-040/041/042/045); FR-FIN-043/044/046 sudah terpenuhi sejak `BE-FIN-008`; controller `FinanceReceiptsController` dibangun — [laporan](../task/report/backend/BE-FIN-018.md) |
| `BE-FIN-019` | Utang supplier | `POST-MVP` | ✅ Selesai 23 September 2026 — `FinSupplierPayable`/`FinSupplierPayableItem`/`FinPayableAdjustment` dan `FinanceSupplierPayableService` (input manual, koreksi maker-checker, pembatalan) selesai; controller `FinanceSupplierPayablesController` dibangun — [laporan](../task/report/backend/BE-FIN-019.md) |
| `BE-FIN-020` | Pembayaran keluar dan potongan | `POST-MVP` | ✅ Selesai 23 September 2026 — `FinPayment`/`FinPaymentAllocation`/`FinPaymentDeduction` dan `FinancePaymentService` (siklus hidup lengkap, pembuktian `FR-FIN-050` & `FR-FIN-051`) selesai; controller `FinancePaymentsController` dibangun; `FIN-OQ-010` (ambang nominal) tetap terbuka — [laporan](../task/report/backend/BE-FIN-020.md) |
| `BE-FIN-021` | Utang jasa tenaga medis | `POST-MVP` | 🟡 Sebagian 22 September 2026 — `FinMedicalServicePayable`/`FinMedicalServicePayableItem` (entity+configuration+migration `AddFinanceMedicalServicePayable` ditulis tangan, belum dijalankan), navigasi polimorfik selesai, intake menunggu `BE-MDF-014`, QBE `PASS` — [laporan](../task/report/backend/BE-FIN-021.md) |
| `BE-FIN-022` | Empat `HandoffType` baru + satu migration check constraint | — (`REV-3`) | 🟡 **Sebagian 25 September 2026** — source lengkap, `dotnet build` `0 Error`; **eksekusi migration belum dijalankan**, sehingga aturan pemeriksaan nilai di database masih empat nilai — [laporan](../task/report/backend/BE-FIN-022.md) |
| `BE-FIN-023` ✅ | Rincian saldo subledger + aturan nilai dan bentuk pesan diluruskan sekali | — (`REV-3`) | ✅ **Selesai 29 September 2026** — build mandiri dikonfirmasi 0 error oleh pengguna, nol perubahan skema, rangkaian ditutup tuntas oleh `BE-FIN-024` ([laporan](../task/report/backend/BE-FIN-023.md)) |
| `BE-FIN-024` ✅ | Penerimaan pra-final terbit segera dengan jenis kejadian yang benar | — (`REV-3`) | ✅ **Selesai 29 September 2026** — build mandiri dikonfirmasi 0 error oleh pengguna, nol perubahan skema, pemilihan `EventTypeCode` dan kode pembalikan selesai ([laporan](../task/report/backend/BE-FIN-024.md)) |
| `BE-FIN-025` ✅ | Deposit, kelebihan bayar, dan selisih kas masuk kotak keluar | — (`REV-3`) | ✅ **Selesai 29 September 2026** — source 4 jalur sinkronisasi dan pengolahan intake selesai, 13 acceptance criteria terpetakan ke source, nol perubahan tabel `Bil*`, `dotnet build` dikonfirmasi 0 error oleh pengguna ([laporan](../task/report/backend/BE-FIN-025.md)) |
| `BE-FIN-026` ✅ | Dua rumpun baris warisan kotak keluar dibereskan | — (`REV-3`) | ✅ Selesai 29 September 2026 — otorisasi pembacaan database diberikan, pembacaan database fisik `QuilvianNewDevYasmina` membuktikan 0 baris `HELD_FOR_FINALIZATION` dan 0 baris nama pendek `AR_*`/`AP_*`, nol baris diubah pada database fisik ([laporan](../task/report/backend/BE-FIN-026.md)) |
| `BE-FIN-027` | Submodul `Purchasing` terdaftar di registry | `POST-MVP` (`REV-4`) | ✅ Selesai 26 September 2026 — baris registry + `Invoke-QbeConformanceCheck.ps1` `PASS`; [laporan](../task/report/backend/BE-FIN-027.md) |
| `BE-FIN-028` | Satu resolver jenjang approval, batas `>= Rp 50.000.000` | `POST-MVP` (`REV-4`) | ✅ Selesai 30 September 2026 — `dotnet build` PASS 0 error; [laporan](../task/report/backend/BE-FIN-028.md). **Mengubah perilaku pembayaran pada nilai tepat Rp 50.000.000** |
| `BE-FIN-029` | Model PO, Tanda Terima Barang, Tukar Faktur, Purchasing Invoice | `POST-MVP` (`REV-4`) | ✅ Selesai 30 September 2026 — `dotnet build` PASS 0 error; [laporan](../task/report/backend/BE-FIN-029.md) |
| `BE-FIN-030` | Model Retur, Deposit Retur, kolom asal pada utang supplier | `POST-MVP` (`REV-4`) | ✅ Selesai 30 September 2026 — `dotnet build` PASS 0 error; [laporan](../task/report/backend/BE-FIN-030.md) |
| `BE-FIN-031` | Dua migration Purchasing/AP | `POST-MVP` (`REV-4`) | ✅ Selesai 30 September 2026 — berkas migration & model snapshot siap, `dotnet build` PASS 0 error; [laporan](../task/report/backend/BE-FIN-031.md) |
| `BE-FIN-032` | PO dan Tanda Terima Barang | `POST-MVP` (`REV-4`) | ✅ Selesai 30 September 2026 — `dotnet build` PASS 0 error (menutup gap `GET /` berpaging + riwayat GR pada detail PO); [laporan](../task/report/backend/BE-FIN-032.md) |
| `BE-FIN-033` | Tukar Faktur | `POST-MVP` (`REV-4`) | ✅ Selesai 30 September 2026 — `dotnet build` PASS 0 error (menutup gap `GET /` berpaging); [laporan](../task/report/backend/BE-FIN-033.md) |
| `BE-FIN-034` | Purchasing Invoice → utang + kejadian PPN Masukan | `POST-MVP` (`REV-4`) | ✅ Selesai 30 September 2026 — `dotnet build` PASS 0 error; kejadian pengakuan utang ditulis nilai pokok (`TotalAmount − PPNAmount`), PPN terpisah; [laporan](../task/report/backend/BE-FIN-034.md) |
| `BE-FIN-035` | Retur dan penerbitan Deposit Retur | `POST-MVP` (`REV-4`) | ✅ Selesai 30 September 2026 — `dotnet build` PASS 0 error, penerbitan deposit retur dan outbox terverifikasi; [laporan](../task/report/backend/BE-FIN-035.md) |
| `BE-FIN-036` | Deposit Retur sebagai sumber dana pembayaran | `POST-MVP` (`REV-4`) | ✅ Selesai 30 September 2026 — `dotnet build` PASS 0 error, migrasi fisik aktif di DB via DBeaver; [laporan](../task/report/backend/BE-FIN-036.md) |
| `BE-FIN-037` | Empat laporan Purchasing/AP | `POST-MVP` (`REV-4`) | ✅ Selesai 30 September 2026 — `dotnet build` PASS 0 error, `/aging` dicabut (`FIN-DEC-059`); [laporan](../task/report/backend/BE-FIN-037.md) |
| `BE-FIN-038` | Skema AR Invoice Agregat + Potongan AR | `POST-MVP` (`REV-4`) | ✅ Selesai 30 September 2026 — `dotnet build` PASS 0 error, migrasi fisik dieksekusi via DBeaver (`be-fin-041-038-043-schema-migration-dbeaver.sql`); [laporan](../task/report/backend/BE-FIN-038.md) |
| `BE-FIN-039` | Batch Tagihan AR | `POST-MVP` (`REV-4`) | ✅ Selesai 30 September 2026 — `dotnet build` PASS 0 error, skema batch aktif di DB; [laporan](../task/report/backend/BE-FIN-039.md) |
| `BE-FIN-040` | Potongan AR bersama alokasinya, ikut terbalik | `POST-MVP` (`REV-4`) | ✅ Selesai 30 September 2026 — `dotnet build` PASS 0 error, skema potongan aktif di DB; [laporan](../task/report/backend/BE-FIN-040.md) |
| `BE-FIN-041` | Kolom `DepositAppliedAmount` pada `FinPayment` | `POST-MVP` (`REV-4`) | ✅ Selesai 30 September 2026 — `dotnet build` PASS 0 error, migrasi fisik dieksekusi via DBeaver (`be-fin-041-038-043-schema-migration-dbeaver.sql`); [laporan](../task/report/backend/BE-FIN-041.md) |
| `BE-FIN-042` 🟡 | Penyelarasan 6 controller legacy, seeder payung Finance.AP/AR, skrip SQL idempotent migrasi peran | `POST-MVP` (`REV-6/8`) | 🟡 Rename 6 controller + skrip SQL migrasi selesai 29 September 2026, build PASS 30 September 2026; seeder payung `Finance.AP`/`Finance.AR` menunggu platform registry `FIN-OQ-039`; [laporan](../task/report/backend/BE-FIN-042.md) |
| `BE-FIN-043` | Kolom `PPNAmount` pada `FinSupplierReturn` dan deposit retur membawa PPN saat `CONFIRMED` | `POST-MVP` (`REV-6/8`) | ✅ Selesai 30 September 2026 — `dotnet build` PASS 0 error, migrasi fisik dieksekusi via DBeaver (`be-fin-041-038-043-schema-migration-dbeaver.sql`); [laporan](../task/report/backend/BE-FIN-043.md) |
| `BE-FIN-044` | Penyelarasan 4 service call points ke katalog resmi Accounting, hapus 5 alias lama | `POST-MVP` (`REV-6/8`) | ✅ Selesai 30 September 2026 — source 4 berkas selesai, 5 alias dihapus, `dotnet build` PASS 0 error; [laporan](../task/report/backend/BE-FIN-044.md) |
| `BE-FIN-045` | Penanda shift kasir tertutup dan pembaliknya | `POST-MVP` (`REV-6/8`) | ✅ Selesai 30 September 2026 — `dotnet build` PASS 0 error; worker pengirimannya digerbang `FIN-OQ-035`; [laporan](../task/report/backend/BE-FIN-045.md) |
| `BE-FIN-046` | Pembalikan tender top-up deposit dikonsumsi | `POST-MVP` (`REV-6/8`) | ✅ Selesai 30 September 2026 — `dotnet build` PASS 0 error; pemicu mutasi Billing aktif; [laporan](../task/report/backend/BE-FIN-046.md) |
| `BE-FIN-047` | Pembalikan pemakaian uang muka deposit (mutasi `RELEASE` berpasangan) | `POST-MVP` (`REV-6/8`) | ✅ Selesai 30 September 2026 — `dotnet build` PASS 0 error; pemasangan mutasi via `CausationId` aktif; [laporan](../task/report/backend/BE-FIN-047.md) §3.3 |
| `BE-FIN-048` | Pengetatan validasi pesan saldo subledger pada kotak keluar | `POST-MVP` (`REV-10`) | ✅ Selesai 30 September 2026 — validasi nilai non-negatif dan tanggal akhir periode terpasang di `ValidateRequest`, [laporan](../task/report/backend/BE-FIN-048.md) |
| `BE-FIN-049` | Layanan kalkulasi snapshot saldo subledger bulanan untuk 4 control account | `POST-MVP` (`REV-10`) | ✅ Selesai 30 September 2026 — service agregasi 4 akun kontrol dan endpoint snapshot terpasang, penerbitan 4 event SALDO-SUBLEDGER ke outbox, [laporan](../task/report/backend/BE-FIN-049.md) |
| `BE-FIN-050` | Buku register penerimaan kasir dan rekonsiliasi shift Finance vs. `SystemCash` Billing — menutup gap yang dikecualikan `BE-FIN-018` | `POST-MVP` — task baru, diotorisasi eksplisit pemilik repository pada sesi FE-FIN-004 | ✅ Selesai 30 September 2026 — source lengkap (`GET /receipts/register`, `GET /receipts/shift-reconciliation`), `dotnet build` PASS 0 error dikonfirmasi pengguna; [laporan](../task/report/backend/BE-FIN-050.md) |
| `BE-FIN-051` | Header `Idempotency-Key` pada 14 perintah uang rumpun Purchasing — menutup gap yang dicatat `FE-FIN-008` | `POST-MVP` — task baru, diotorisasi eksplisit pemilik repository pada sesi ini | ✅ Selesai 30 September 2026 — ledger idempotensi (`FinPurchasingIdempotencyRecord`) + `PurchasingIdempotencyService` + 14 aksi lintas 5 controller Purchasing; `dotnet build` PASS dan migration dieksekusi ke database, keduanya dikonfirmasi pengguna; [laporan](../task/report/backend/BE-FIN-051.md) |

### 4.2 Frontend — `02-frontend-roadmap.md`

| Task | Outcome ringkas | Menunggu backend | Keadaan |
|---|---|---|---|
| `FE-FIN-001` | Pengelolaan data induk | `BE-FIN-004` ✅ | 🟡 Source lengkap 23 September 2026 (dua fitur master data: Rekening Bank, Mata Uang+Kurs) — `npm run lint:errors`/`npm run build`/uji manual belum dijalankan — [laporan](../task/report/frontend/FE-FIN-001.md) |
| ✅ `FE-FIN-002` | Buku piutang dan umur piutang | `BE-FIN-009` ✅ | ✅ Selesai 23 September 2026 — daftar, rincian, 4 aging bucket cards, maker-checker koreksi/write-off, telusur tagihan asal selesai; `npm run lint:errors` PASS, `npm run build` PASS ([laporan](../task/report/frontend/FE-FIN-002.md)) |
| ✅ `FE-FIN-003` | Setoran bank dan kas harian | `BE-FIN-015` ✅ | ✅ Selesai 23 September 2026 — daftar setoran bank, draf saldo kasir tersedia dari backend, detail siklus hidup setoran (posting/verifikasi/batal), riwayat kas harian, breakdown modal, penutupan kas beku; `npm run lint:errors` PASS, `npm run build` PASS ([laporan](../task/report/frontend/FE-FIN-003.md)) |
| ✅ `FE-FIN-006` | Pemantauan fakta Billing dan kejadian Accounting | `BE-FIN-009` ✅, `BE-FIN-012` ✅ | ✅ Selesai 23 September 2026 — daftar gagal olah & pengulangan, antrean kejadian outbox (HELD/FAILED/HELD_FOR_FINALIZATION), modal rincian muatan JSON & attempt; `npm run lint:errors` PASS (0 error), `npm run build` PASS (0 error) ([laporan](../task/report/frontend/FE-FIN-006.md)) |
| 🟡 `FE-FIN-004` | Penerimaan dan alokasi | `BE-FIN-018` ✅, `BE-FIN-050` ✅ | 🟡 Sebagian 30 September 2026 — Penerimaan (daftar+rincian), alokasi manual (susun+balik), buku register, dan rekonsiliasi shift kelimanya selesai, lint/build PASS; verifikasi manual/runtime `NOT FEASIBLE` ([laporan](../task/report/frontend/FE-FIN-004.md)) |
| ✅ `FE-FIN-005` | Merapikan Petty Cash ke rute Finance | — | ✅ Selesai 23 September 2026 — slices/hooks/constants dipindahkan ke finance dengan jembatan re-export tanpa regresi, halaman voucher `/finance/petty-cash-voucher` dibangun, menu sidebar terdaftar; `npm run lint:errors` PASS, `npm run build` PASS ([laporan](../task/report/frontend/FE-FIN-005.md)) |
| ✅ `FE-FIN-007` | Layar pemantauan kejadian katalog final & baris intake ERROR | `FE-FIN-006` ✅, `BE-FIN-024` ✅, `BE-FIN-025` ✅ | ✅ Selesai 30 September 2026 — seluruh jenis kejadian katalog final disajikan dinamis dari metadata, baris intake ERROR disajikan beserta sebabnya tanpa tombol ulangi untuk galat kebijakan (FIN-VAL-141/142), HELD_FOR_FINALIZATION diperlakukan tegas sebagai peninggalan kebijakan lama, baris ACKNOWLEDGED tanpa nomor jurnal tidak dianggap galat; npm run lint:errors PASS (0 error), npm run build PASS (0 error) ([laporan](../task/report/frontend/FE-FIN-007.md)) |
| 🟡 `FE-FIN-008` | PO, Tanda Terima Barang, Tukar Faktur | `BE-FIN-032`, `033` | 🟡 Sebagian, diperbarui 30 September 2026 — Create/Detail/aksi PO, catat GR (riwayat persisten), Create/Detail/Cancel Tukar Faktur, Daftar PO, Daftar Tukar Faktur, kolom Supplier pada Daftar (nol batasan jumlah), dan dropdown PO/GR pada form Tukar Faktur (bukan ID manual), seluruhnya selesai; satu-satunya gap tersisa: `Idempotency-Key` — dikonfirmasi perlu task backend tersendiri (migration lintas 5 entity), bukan pekerjaan frontend; `npm run lint:errors` PASS, `npm run build` NOT RUN — [laporan](../task/report/frontend/FE-FIN-008.md) |
| ✅ `FE-FIN-009` | Purchasing Invoice, Retur, daftar Deposit Retur | `BE-FIN-034` ✅, `035` ✅ | ✅ Selesai 30 September 2026 — penyusunan draf dari Tukar Faktur, kalkulasi total seimbang, persetujuan multi-jenjang, pencatatan retur, penerbitan & daftar deposit retur berpaging; `npm run lint:errors` PASS (0 error), `npm run build` PASS (0 error) ([laporan](../task/report/frontend/FE-FIN-009.md)) |
| ✅ `FE-FIN-010` | Pilih Deposit Retur di layar susun pembayaran | `BE-FIN-036` ✅, `FE-FIN-009` ✅ | ✅ Selesai 30 September 2026 — integrasi pemilihan Deposit Retur pada draf pembayaran AP, penyajian 4 angka resmi backend, relaksasi bukti transfer saat net transfer 0; npm run lint:errors PASS (0 error), npm run build PASS (0 error) ([laporan](../task/report/frontend/FE-FIN-010.md)) |
| ✅ `FE-FIN-011` | Empat laporan Purchasing/AP | `BE-FIN-037` ✅ | ✅ Selesai 30 September 2026 — 4 laporan analitik Purchasing AP (Rekap, Tukar Faktur, Jatuh Tempo, Rekonsiliasi Tagihan), zero client calculation, read-only; npm run lint:errors PASS (0 error), npm run build PASS (0 error) ([laporan](../task/report/frontend/FE-FIN-011.md)) |
| 🟡 `FE-FIN-012` | Batch Tagihan AR | `BE-FIN-039` | 🟡 Sebagian 30 September 2026 — source lengkap (Daftar/Buat/Detail/Terbitkan/Batal/Dokumen), dilabeli "Tagihan Gabungan Penjamin" (`FIN-DEC-060`); `npm run lint:errors` PASS, `npm run build` PASS; verifikasi manual/runtime `NOT FEASIBLE` — migrasi `BE-FIN-038` belum dieksekusi ke database ([laporan](../task/report/frontend/FE-FIN-012.md)) |
| 🟡 `FE-FIN-013` | Potongan AR di layar alokasi | `BE-FIN-040`, `FE-FIN-004` 🟡 | 🟡 Sebagian 30 September 2026 — baris potongan ditambahkan ke modal alokasi `FE-FIN-004`, lint/build PASS; verifikasi manual/runtime `NOT FEASIBLE` ([laporan](../task/report/frontend/FE-FIN-013.md)) |
| 🟡 `FE-FIN-014` | Penyelarasan menu sidebar navigasi Finance (submenu Pembelian, relabel Faktur Pembelian, butir flat Tagihan Gabungan Penjamin) | `BE-FIN-042` [BE] 🟡, `FE-FIN-008`..`012` | 🟡 Sebagian 30 September 2026 — submenu Pembelian (5 link), butir flat Tagihan Gabungan Penjamin, relabel Faktur & Tagihan Supplier, hapus 2 placeholder mati; `npm run lint:errors` PASS, `npm run build` PASS; seeder payung Finance.AP/AR (BE-FIN-042 §D.6.2) dikonfirmasi tidak menahan task ini; verifikasi manual/runtime `NOT FEASIBLE`. **Addendum 30 September 2026:** `FIN-CQ-09` diperbaiki — butir "Report AR"/"Report AP" dijaga action `Report` yang tidak pernah terdaftar backend, diganti `View` agar cocok penjaga endpoint sungguhan; `npm run lint:errors` PASS ([laporan](../task/report/frontend/FE-FIN-014.md)) |
| ✅ `FE-FIN-015` | Layar/Tab Pemantauan & Pemicu Snapshot Saldo Subledger Bulanan (4 Akun Kontrol) | `BE-FIN-048` ✅, `BE-FIN-049` ✅ | ✅ Selesai 30 September 2026 — pemilih periode akuntansi, 4 kartu ringkasan status kelengkapan, tabel 4 akun kontrol dengan nominal saldo normal & status outbox, tombol pemicu generate snapshot dengan modal konfirmasi dan proteksi RBAC FinanceAccountingEvent:Create; npm run lint:errors PASS (0 error), npm run build PASS (0 error) ([laporan](../task/report/frontend/FE-FIN-015.md)) |

`FE-FIN-006` ditambahkan saat roadmap dipecah: `03-frontend-architecture.md` bagian 3.5
menuntut dua layar pemantauan, dan keduanya sebelumnya tidak punya task frontend sama sekali.

### 4.3 Task yang sengaja tidak dibuat

| Epic | Alasan |
|---|---|
| `EPIC FIN-04` — piutang manfaat karyawan | `OPEN DECISION`; menunggu owner Billing **dan** HR (`FIN-DEC-006`, `FIN-DEC-016`, `FIN-CQ-03`) |
| `EPIC FIN-12` — pengiriman kejadian ke Accounting | `OPEN DECISION`; endpoint penerima belum dibangun (`FIN-CAP-018`) |
| `EPIC FIN-14` — uang muka, deposit, kelebihan bayar, selisih kas | `OPEN DECISION`; sebabnya kini `FIN-OQ-027`, `030`..`032`, `034` (`FIN-OQ-017` sudah `CLOSED`). **Task-nya sudah direncanakan** (`BE-FIN-022`..`026`, `FE-FIN-007`) dan blocker ⛔ dicabut — bukti teknis, kontrak, dan prasyarat backend sudah lengkap, boleh dikerjakan di luar penomoran gelombang formal |
| `FinDoctorPayable`, `FinDoctorPayableItem` | Dibatalkan pada revisi 2; digantikan `FinMedicalServicePayable` |
| Migration `AddFinanceSubledgerPeriodBalance` | `FIN-OQ-011` belum dijawab; rumpunnya `POST-MVP` |

## 5. Traceability

| Requirement | Decision | Desain | Kontrak | Task BE | Task FE | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `FR-FIN-001`..`004` | `FIN-DEC-013`, `FIN-DEC-014` | `FIN-DES-001`..`005` | `FIN-API-1.0`, `FIN-VAL-1.0` | `BE-FIN-001`..`004` | `FE-FIN-001` | `UAT-01`, `UAT-02` | 🟡 Backend selesai 23 September 2026 — `BE-FIN-001` ✅ 21 September 2026 ([laporan](../task/report/backend/BE-FIN-001.md)); `BE-FIN-002` ✅, `MstBank` dipakai ulang dari Administrator bukan dibuat baru ([laporan](../task/report/backend/BE-FIN-002.md)); `BE-FIN-003` ✅, migration dieksekusi ([laporan](../task/report/backend/BE-FIN-003.md)); `BE-FIN-004` ✅, 2 dari 3 controller, `UAT-01`/`UAT-02` diuji end-to-end ([laporan](../task/report/backend/BE-FIN-004.md)). Frontend `FE-FIN-001` 🟡 source lengkap 23 September 2026, `UAT-01`/`UAT-02` belum dibuktikan runtime, validasi `npm` belum dijalankan ([laporan](../task/report/frontend/FE-FIN-001.md)) |
| `FR-FIN-010`..`013` | `FIN-DEC-005` (konsumsi) | `FIN-DES-008`, `009` | `FIN-INTEGRATION-1.0` | `BE-FIN-005`, `009` | `FE-FIN-006` | `UAT-04` | ✅ Selesai 23 September 2026 — `BE-FIN-005` ([laporan](../task/report/backend/BE-FIN-005.md)); `FinanceBillingIntakeService` selesai dibangun `BE-FIN-009` (otorisasi eksplisit, menutup gap `BE-FIN-005`), 2 asumsi bisnis ditutup bukti source, `UAT-04` terpenuhi ([laporan](../task/report/backend/BE-FIN-009.md)) |
| `FR-FIN-020`..`024` | `FIN-DEC-010`..`012` | `FIN-DES-010`..`013` | `FIN-VAL-1.0` | `BE-FIN-006`..`009` | `FE-FIN-002` | `UAT-03` | ✅ Selesai 23 September 2026 — `BE-FIN-006` entity+configuration selesai, invariant seimbang sudah check constraint ([laporan](../task/report/backend/BE-FIN-006.md)); `BE-FIN-007` migration dieksekusi, 2 tabel Collection dibangun terpisah di `BE-FIN-016` ([laporan](../task/report/backend/BE-FIN-007.md)); `BE-FIN-008` `FinanceReceivableService` selesai, 3 temuan kontrak terbuka untuk ratifikasi (tidak menahan) ([laporan](../task/report/backend/BE-FIN-008.md)); `BE-FIN-009` `FinanceReceivablesController` selesai, `FR-FIN-024` (PatientId) kini terpenuhi ([laporan](../task/report/backend/BE-FIN-009.md)). Frontend `FE-FIN-002` ✅ selesai 23 September 2026, `npm run lint:errors` PASS (0 error), `npm run build` PASS (0 error) ([laporan](../task/report/frontend/FE-FIN-002.md)) |
| `FR-FIN-030`..`035` | `FIN-DEC-005`, `FIN-DEC-015` | `FIN-DES-014` | `FIN-INTEGRATION-1.0` — permukaan collection dikecualikan | `BE-FIN-016`, `017` | `FE-FIN-004` | `UAT-05`, `06`, `20` | ✅ Selesai 23 September 2026 — `BE-FIN-016` `FinReceipt`/`FinReceiptAllocation`+konsumsi `COLLECTION` selesai, jalur `REVERSED` diperbaiki ([laporan](../task/report/backend/BE-FIN-016.md)); `BE-FIN-017` `FinanceReceiptService`+pembuktian `FR-FIN-035`+`CreateReversalReceiptAsync` selesai ([laporan](../task/report/backend/BE-FIN-017.md)); migration `AddFinanceCollection`/`FixFinReceiptTenderRequiredForReversal` masih menunggu eksekusi |
| `FR-FIN-040`..`046` | `FIN-DEC-017` | `FIN-DES-014` | `FIN-STATE-1.0` | `BE-FIN-018` | 🟡 `FE-FIN-004` | `UAT-08`..`12` | ✅ Backend selesai 23 September 2026 — `FR-FIN-040`/`041`/`042`/`045` selesai (`AllocateAsync`/`ReverseAllocationAsync`); `FR-FIN-043`/`044`/`046` sudah terpenuhi sejak `BE-FIN-008`; controller `FinanceReceiptsController` dibangun ([laporan](../task/report/backend/BE-FIN-018.md)). 🟡 `FE-FIN-004` sebagian 30 September 2026 — Penerimaan, alokasi manual, buku register, dan rekonsiliasi shift selesai, lint/build PASS ([laporan](../task/report/frontend/FE-FIN-004.md)) |
| `FR-FIN-050`, `051` | `FIN-DEC-019` | `FIN-DES-015`, `026`..`028` | `FIN-API-1.0`, `FIN-VAL-1.0` | `BE-FIN-020` | — | — | ✅ Selesai 23 September 2026 — `FinPayment`/`FinPaymentAllocation`/`FinPaymentDeduction`+`FinancePaymentService` membuktikan uang keluar berbeda dari utang lunas (`FR-FIN-050`) dan potongan tidak menyisakan utang (`FR-FIN-051`); controller dibangun ([laporan](../task/report/backend/BE-FIN-020.md)) |
| `FR-FIN-060`..`065` | `FIN-DEC-018` | `FIN-DES-018`..`020` | `FIN-VAL-1.0` | `BE-FIN-013`..`015` | `FE-FIN-003` | `UAT-13`..`16` | ✅ Backend selesai 23 September 2026 — `BE-FIN-013` entity+configuration+migration dieksekusi ([laporan](../task/report/backend/BE-FIN-013.md)); `BE-FIN-014` service selesai, saldo dihitung saat posting ([laporan](../task/report/backend/BE-FIN-014.md)); `BE-FIN-015` 2 controller selesai, QBE `PASS` ([laporan](../task/report/backend/BE-FIN-015.md)); ketiganya diuji end-to-end dengan hasil sesuai ekspektasi |
| `FR-FIN-070`..`074` | `FIN-DEC-001`, `004` | `FIN-DES-017`, `021`..`023` | `FIN-INTEGRATION-1.0`, `ACC-XMOD-0.2` | `BE-FIN-010`..`012` | `FE-FIN-006` | `UAT-07`, `UAT-17`..`19` | ✅ Backend selesai 23 September 2026 — `BE-FIN-010` entity+configuration+migration dieksekusi ([laporan](../task/report/backend/BE-FIN-010.md)); `BE-FIN-011` service+3 pemanggilan selesai, QBE `PASS` (47 berkas) ([laporan](../task/report/backend/BE-FIN-011.md)); `BE-FIN-012` `FinanceAccountingEventsController`+`Service` selesai, QBE `PASS` (50 berkas) ([laporan](../task/report/backend/BE-FIN-012.md)); ketiganya diuji end-to-end dengan hasil sesuai ekspektasi |
| `FR-FIN-075` | `FIN-DEC-003`, `019` | `FIN-DES-025` | `FIN-API-1.0` | `BE-FIN-021` | — | — | 🟡 Sebagian 22 September 2026 — model+skema selesai, intake menunggu `BE-MDF-014` ([laporan](../task/report/backend/BE-FIN-021.md)) |
| `FR-FIN-034` **diperbarui**, `FR-FIN-076` | `FIN-DEC-030` | `FIN-DES-033`, `034` | `FIN-INTEGRATION-1.5` §5.5, `FIN-STATE-1.3` §9, `FIN-VAL-1.4` `FIN-VAL-085` | `BE-FIN-023` ✅, `024` ✅ | `FE-FIN-007` | `FIN-TEST-1.5` §8a, §D.2 | ✅ Selesai 29 September 2026 — `BE-FIN-023` mencabut penahanan pra-final ([laporan](../task/report/backend/BE-FIN-023.md)); `BE-FIN-024` menerbitkan `PENERIMAAN-UANG-MUKA` dan pembalikannya mengikuti penerimaan asli ([laporan](../task/report/backend/BE-FIN-024.md)); build mandiri dikonfirmasi 0 error oleh pengguna |
| `FR-FIN-077` | `FIN-DEC-031`, `040`, `041` | `FIN-DES-029`, `030`, `035` | `FIN-INTEGRATION-1.5` §2a, §5.4 | `BE-FIN-022` 🟡, `025` ✅ | — | `FIN-TEST-1.5` §8a | 🟡 `BE-FIN-022` source selesai 25 September 2026; `BE-FIN-025` ✅ selesai 29 September 2026, build mandiri dikonfirmasi 0 error oleh pengguna ([laporan](../task/report/backend/BE-FIN-025.md)) |
| `FR-FIN-078` | `FIN-DEC-042`, **`067`**, **`074`** | `FIN-DES-030`, `035`, **`056`** | `FIN-INTEGRATION-1.5` §2a, §5.4, §5.10; `FIN-VAL-1.4` **`FIN-VAL-141`** | `BE-FIN-025` ✅ | — | `FIN-TEST-1.5` §D.6 | ✅ `BE-FIN-025` selesai 29 September 2026, build mandiri dikonfirmasi 0 error oleh pengguna — kredit `SETTLEMENT` masuk jalur yang sudah ada, `REFERRED_OUTPATIENT_ADMIN` menjadi baris intake `ERROR` ([laporan](../task/report/backend/BE-FIN-025.md)) |
| `FR-FIN-079` | `FIN-DEC-034`, `043`, **`064`**, **`073`** | `FIN-DES-036`, **`053`** | `FIN-INTEGRATION-1.5` §5.4, §5.10; `FIN-VAL-1.4` `FIN-VAL-086`, **`140`** | `BE-FIN-025` ✅ | — | `FIN-TEST-1.5` §D.3 | ✅ `BE-FIN-025` selesai 29 September 2026, build mandiri dikonfirmasi 0 error oleh pengguna — satu kode selisih kas menjadi dua (`SELISIH-KAS-KURANG`/`LEBIH`) berkunci `BilCashierShift.Id` ([laporan](../task/report/backend/BE-FIN-025.md)) |
| `FR-FIN-080` | `FIN-DEC-035`, **`064`** | `FIN-DES-031`, `032`, **`054`**, **`058`** | `FIN-INTEGRATION-1.5` §5.2, §5.6, §5.10; `FIN-VAL-1.4` `FIN-VAL-081`..`084`, **`138`**, **`139`** | `BE-FIN-023` ✅ | — | `FIN-TEST-1.5` §D.2 | ✅ **Selesai 29 September 2026** — source selesai, build mandiri dikonfirmasi 0 error oleh pengguna, rincian saldo subledger, daftar tertutup kode penanda, dan pelurusan `Components` tuntas ([laporan](../task/report/backend/BE-FIN-023.md)) |
| `FR-FIN-108`..`110` | `FIN-DEC-070`, `072`, `073`, `075` | `FIN-DES-054`, `059` | `FIN-INTEGRATION-1.5` §5.10, §5.11; `FIN-VAL-1.4` `FIN-VAL-138` | `BE-FIN-045` | — | `FIN-TEST-1.5` §D.3, §E.1 | Direncanakan 29 September 2026 (`REV-6/8`) — kode boleh ditulis; **worker** digerbang `FIN-OQ-035` |
| Pembalikan tender top-up deposit | `FIN-DEC-040`, `044`, `077`; `BKC-DEC-128`..`131` | `FIN-DES-035`, `057` | `FIN-INTEGRATION-1.5` §5.10; `FIN-VAL-1.4` `FIN-VAL-142` | `BE-FIN-046` | — | `FIN-TEST-1.5` §D.6 | Direncanakan 29 September 2026 (`REV-6/8`) — pemicunya **sudah ada** sejak Billing menutup `FIN-OQ-034` |
| Pembalikan pemakaian uang muka deposit | `FIN-DEC-063`, `077`, `080`, `081` | `FIN-DES-064`, `065` | `FIN-INTEGRATION-1.6` §5.10 (pemicu `RELEASE` berpasangan); `FIN-VAL-1.5` `FIN-VAL-144`..`146` | `BE-FIN-047` | — | `FIN-TEST-1.6` §F.1..§F.3 | Direncanakan 29 September 2026 (`REV-6/8`, gelombang `R6-4`) — blocker dicabut via `FIN-DEC-080`/`081` |
| ~~`FR-FIN-074`~~ | ~~`FIN-DEC-004`~~ | — | — | ~~`BE-FIN-012`~~ | ~~`FE-FIN-006`~~ | — | **DICABUT 25 September 2026** (`FIN-DEC-030`). Kedua task tetap ✅ dan buktinya tetap berlaku untuk keadaan saat dikerjakan; penanganan baris warisan menjadi `BE-FIN-026` |
| Baris warisan kotak keluar — `HELD_FOR_FINALIZATION` **dan** lima nama pendek `AR_*`/`AP_*` | `FIN-DEC-030`; **`FIN-DES-058`** | `02-backend-architecture.md` B.6 **dan E.9** | `FIN-VAL-1.4` `FIN-VAL-078` | `BE-FIN-026` ✅ | `FE-FIN-007` | `FIN-TEST-1.5` §D.1 baris terakhir, §8a | ✅ **SELESAI 29 September 2026** — otorisasi pembacaan database diberikan pengguna, audit fisik membuktikan 0 baris kedua rumpun pada database pengembangan, rekomendasi penanganan diserahkan ke pemilik ([laporan](../task/report/backend/BE-FIN-026.md)) |
| Manfaat karyawan | `FIN-DEC-006`, `016` | — | — | — | — | — | `OPEN DECISION` |
| Pengiriman ke Accounting | ~~`FIN-DEC-007`~~ `FIN-DEC-036` (syarat organisasi), `FIN-OQ-016` (mekanisme) | `FIN-DES-024` | `FIN-INTEGRATION-1.1` §5.1 | — | — | — | `OPEN DECISION` — tiga syarat akun layanan kini **ditetapkan**; yang terbuka hanya bentuk kredensialnya |
| `FR-FIN-081` | `FIN-DEC-050`, `052` | `FIN-DES-039` (dikoreksi) | `FIN-API-1.1` B.1, `FIN-VAL-1.2` `100`..`103` | `BE-FIN-027` ✅, `028`, `029`, `031`, `032` | 🟡 `FE-FIN-008` | `FIN-TEST-1.2` B.1 (termasuk dua baris batas Rp 50.000.000) | 🟡 `BE-FIN-027` selesai 26 September 2026 — [laporan](../task/report/backend/BE-FIN-027.md); `FE-FIN-008` sebagian, diperbarui 30 September 2026 (Create/Detail/aksi PO, dan kini Daftar PO, selesai) — [laporan](../task/report/frontend/FE-FIN-008.md) |
| `FR-FIN-082` | `FIN-DEC-051` | `FIN-DES-037` | `FIN-API-1.1` B.3, `FIN-STATE-1.2` B.3 | `BE-FIN-033` | 🟡 `FE-FIN-008` | `FIN-TEST-1.2` B.2 | 🟡 `FE-FIN-008` sebagian, diperbarui 30 September 2026 (Create/Detail/Cancel Tukar Faktur, dan kini Daftar Tukar Faktur, selesai) — [laporan](../task/report/frontend/FE-FIN-008.md) |
| `FR-FIN-083`, `084`, `086` | `FIN-DEC-045`, `051` | `FIN-DES-037`, `040` | `FIN-API-1.1` B.4, `FIN-VAL-1.2` `105`..`109` | `BE-FIN-030` ✅, `034` ✅ | ✅ `FE-FIN-009` | `FIN-TEST-1.2` B.3 | ✅ Selesai 30 September 2026 — BE-FIN-034 selesai; FE-FIN-009 selesai ([laporan](../task/report/frontend/FE-FIN-009.md)) |
| `FR-FIN-087` | `FIN-DEC-046`, `053`, `056` | `FIN-DES-043` | `FIN-INTEGRATION-1.2` §5.8, `FIN-VAL-1.2` `122` | `BE-FIN-034` ✅; worker = `EPIC FIN-12` | ✅ `FE-FIN-009` | `FIN-TEST-1.2` B.3 baris `FIN-VAL-122` | ✅ Selesai 30 September 2026 — penulisan kejadian PPN Masukan di BE-FIN-034; tampilan FE-FIN-009 tidak menampilkan klaim berhasil prematur ([laporan](../task/report/frontend/FE-FIN-009.md)) |
| `FR-FIN-085` (penerbitan) | `FIN-DEC-047` | `FIN-DES-038` | `FIN-API-1.1` B.5, `FIN-VAL-1.2` `110`, `111` | `BE-FIN-035` ✅ | ✅ `FE-FIN-009` | `FIN-TEST-1.2` B.4 (3 baris pertama) | ✅ Selesai 30 September 2026 — BE-FIN-035 selesai; FE-FIN-009 selesai ([laporan](../task/report/frontend/FE-FIN-009.md)) |
| `FR-FIN-085` (pemakaian), `FR-FIN-096`, `097` | `FIN-DEC-047`, `057`, `061` | `FIN-DES-045`..`047` | `FIN-API-1.2` C.1-C.2, `FIN-STATE-1.3` C.1-C.3, `FIN-VAL-1.3` C.1-C.2 | `BE-FIN-030`, `031`, `036`, `041` | ✅ `FE-FIN-010` | `FIN-TEST-1.3` C.1, C.2 | ✅ Selesai 30 September 2026 — BE-FIN-036 selesai; FE-FIN-010 selesai ([laporan](../task/report/frontend/FE-FIN-010.md)) |
| `FR-FIN-098` | `FIN-DEC-061` | `FIN-DES-047` | `FIN-INTEGRATION-1.3` §5.9 kode 28 | `BE-FIN-035` | — | `FIN-TEST-1.3` C.2 | Direncanakan — worker menunggu `FIN-OQ-026` |
| `FR-FIN-088` | — | `FIN-DES-044` | `FIN-API-1.1` B.6 | `BE-FIN-037` ✅ | ✅ `FE-FIN-011` | `FIN-TEST-1.2` B.6 | ✅ Selesai 30 September 2026 — BE-FIN-037 selesai; FE-FIN-011 selesai ([laporan](../task/report/frontend/FE-FIN-011.md)) |
| `FR-FIN-089`..`092` | `FIN-DEC-048`, `054` | `FIN-DES-041` | `FIN-API-1.1` B.7, `FIN-STATE-1.2` B.7, `FIN-VAL-1.2` `114`..`117` | `BE-FIN-038`, `039` | 🟡 `FE-FIN-012` | `FIN-TEST-1.2` B.5 | 🟡 Sebagian 30 September 2026 — source frontend lengkap, lint/build PASS; verifikasi manual/runtime `NOT FEASIBLE` (migrasi `BE-FIN-038` belum dieksekusi ke database) — [laporan](../task/report/frontend/FE-FIN-012.md) |
| `FR-FIN-093`..`095`, `099` | `FIN-DEC-049`, `055`, `058`, `062` | `FIN-DES-042`, `048`..`050` | `FIN-API-1.2` C.2, `FIN-STATE-1.3` C.4, `FIN-VAL-1.3` C.1, C.3 | `BE-FIN-038` (tabel), `BE-FIN-040` | 🟡 `FE-FIN-013` | `FIN-TEST-1.3` C.3, C.4 | 🟡 Sebagian 30 September 2026 — baris potongan selesai di modal alokasi, lint/build PASS ([laporan](../task/report/frontend/FE-FIN-013.md)); worker kode 26/27 tetap menunggu `FIN-OQ-026` |
| Granularitas hak akses & migrasi peran | `FIN-DEC-061`, `077`..`079` | `FIN-DES-061`..`063` | `FIN-PERM-1.3`, `FIN-MVP-1.5` | `BE-FIN-042` | 🟡 `FE-FIN-014` | `FIN-TEST-1.4` Bagian E.1, E.2 | 🟡 Sebagian 30 September 2026 — lint/build PASS, verifikasi manual `NOT FEASIBLE` ([laporan](../task/report/frontend/FE-FIN-014.md)) |
| PPN Retur Pembelian | `FIN-DEC-047`, `063` | `FIN-DES-055` | `FIN-VAL-1.4` `FIN-VAL-135`, `FIN-MVP-1.5` | `BE-FIN-043` | — | `FIN-TEST-1.4` Bagian E.3 | Direncanakan 28 September 2026 (`REV-6/8`) |
| Penyelarasan katalog kode kejadian Accounting | `FIN-DEC-039`, `071` | `FIN-DES-058` | `FIN-INTEGRATION-1.4` §5.10, `FIN-MVP-1.5` | `BE-FIN-044` ✅ | — | `FIN-TEST-1.4` Bagian E.4 | ✅ **Selesai 30 September 2026** — `dotnet build` PASS 0 error ([laporan](../task/report/backend/BE-FIN-044.md)) |
| Penyelarasan navigasi sidebar | `FIN-DEC-060`, `076` | `FIN-DES-060` | `03-frontend-architecture.md` Bagian 15, `FIN-MVP-1.5` | — | 🟡 `FE-FIN-014` | Verifikasi visual sidebar | 🟡 Sebagian 30 September 2026 — source selesai, lint/build PASS; verifikasi visual sidebar `NOT FEASIBLE` (tidak ada dev server/peramban) ([laporan](../task/report/frontend/FE-FIN-014.md)) |


## 6. Coverage gap

| Gap | Akibat | Pemilik |
|---|---|---|
| `FR-FIN-051` (potongan tidak menyisakan utang) tanpa baris uji pada `FIN-TEST-0.1` | Aturan paling halus pada rumpun pembayaran tanpa penjaga | Pass desain revisi 2 |
| Rumpun manfaat karyawan tanpa requirement yang lengkap | Tidak dapat direncanakan; sudah dikeluarkan dari seluruh gelombang | Owner Billing + HR |
| ~~Katalog 17 jenis kejadian Accounting belum diratifikasi Rizki~~ | **TERTUTUP 24 September 2026** — ketujuh belas kode diratifikasi apa adanya (`ACC-DEC-083`, dicatat `FIN-DEC-039`) | — |
| ~~**Tujuh kode kejadian baru belum diratifikasi Rizki**~~ (`FIN-OQ-017`) | **TERTUTUP 28 September 2026** — lima kode diratifikasi apa adanya, `PENGAKUAN-KELEBIHAN-BAYAR` bersyarat dan syaratnya diterima (`FIN-DEC-067`), `SELISIH-KAS-SHIFT` diganti dua kode dan namanya diterima (`FIN-DEC-064`); balasan Finance terkirim `evidence/15`. Tanda ⛔ pada `BE-FIN-023`..`025` **dicabut** revisi roadmap 9. Yang menggantikannya: lima ratifikasi kode **baru** (`FIN-OQ-027`..`031`) yang hanya menahan **worker pengiriman**, bukan penulisan kode (pola `FIN-DEC-056`) | — |
| Lawan jurnal refund `SETTLEMENT`/`REFERRED_OUTPATIENT_ADMIN` (`FIN-OQ-018`) | Refund kategori itu tidak diterbitkan sebagai kejadian apa pun; sengaja di luar cakupan | Yasmin (Finance) — `LATER SLICE`, tidak menahan apa pun |
| Mekanisme kredensial akun layanan (`FIN-OQ-016`) | `EPIC FIN-12` dan gerbang `G3` tertahan; tiga syarat organisasinya sudah ditetapkan `FIN-DEC-036` | Platform + Rizki + Yasmin |
| Audit field-per-field rumpun AR/AP/Payable Finance | Blueprint belum dapat dipakai sebagai acuan penuh AR/AP — 59 berkas hasil `BE-FIN-001`..`021` belum diaudit sejak `09101d05` | Yasmin — jalankan `trace-existing-capabilities` |
| ~~Ambang nominal approval AP (`FIN-OQ-010`)~~ | **TERTUTUP 25 September 2026** (`FIN-DEC-052`). Batas tepat Rp 50.000.000 di kode berbeda dari keputusan — dikoreksi `BE-FIN-028`, 26 September 2026 | — |
| `dotnet build` belum dikonfirmasi untuk `BE-FIN-028`, `029`, dan `030` | Perubahan pada `FinancePaymentService.cs`/`FinPayment.cs`/`FinancePaymentsController.cs`/`FinSupplierPayable.cs`/`FinSupplierPayableConfiguration.cs` yang sudah berjalan, dan sebelas entity+configuration baru submodul Purchasing, belum terverifikasi compiler; risiko regresi rendah tapi belum nol. `BE-FIN-031`/`032`/`033`/`034` MUST menunggu konfirmasi sebelum dianggap aman membangun di atasnya | Yasmin — jalankan `dotnet build` sendiri, sesuai instruksi |
| `EPIC FIN-15`/`16`/`17` tanpa skenario UAT pada `04-prd-to-mvp.md` bagian 18 | Bukti penerimaan revisi 4 bersandar pada `FIN-TEST-1.2` B.1-B.7, bukan `UAT-*` bernomor — konsisten dengan `EPIC FIN-14` yang juga tanpa `UAT-*` | Yasmin — tambahkan `UAT-21` dst. bila revisi 4 dimasukkan ke Definition of Done rilis |
| ~~Deposit Retur tanpa mekanisme pengurang utang~~ (`FIN-OQ-023`) | **TERTUTUP PENUH** — keputusan `FIN-DEC-057` (25 September 2026), skema `FIN-DES-045`..`047` (26 September 2026) | — |
| **Layar pembayaran supplier memakai rute alias** — `/finance/payment-ap` memanggil `api/finance/payable/payment` (`FinanceApController`), sedangkan endpoint deposit revisi 5 ada di `api/v1/corporate/finance-management/payments` | `FE-FIN-010` tidak dapat memperluas layar itu tanpa lebih dulu memastikan jalur mana yang kanonik; dua jalur pembayaran dapat menghasilkan dua perilaku | Yasmin — `trace-existing-capabilities` impact scan frontend/backend **sebelum** `FE-FIN-010` |
| ~~Potongan AR tanpa kode kejadian akuntansi~~ (`FIN-OQ-024`) | **TERTUTUP sisi Finance 25 September 2026** (`FIN-DEC-058`) — kode `POTONGAN-PIUTANG-NON-TUNAI` diusulkan, `BE-FIN-040`/`FE-FIN-013` dibuka. Ratifikasi Rizki dicatat `FIN-OQ-026`, hanya menahan worker | Rizki (Accounting) — surat evidence belum dikirim |
| ~~Dua endpoint umur utang~~ (`FIN-OQ-025`) | **TERTUTUP 25 September 2026** (`FIN-DEC-059`) — `/purchasing/reports/aging` dicabut dari kontrak | — |
| ~~Label menu layar revisi 4~~ (`FIN-OQ-022`) | **TERTUTUP 25 September 2026** (`FIN-DEC-060`) | — |
| **Surat evidence `POTONGAN-PIUTANG-NON-TUNAI` belum dikirim** (`FIN-OQ-026`) | Worker pengiriman kode ini tetap dimatikan; tidak menahan `BE-FIN-040`/`FE-FIN-013` | Yasmin — menyusul sebagai instruksi terpisah, pola `evidence/06` |
| ~~Kode kejadian alias di luar katalog ratifikasi~~ | **TERTUTUP 28 September 2026** — ditangani oleh task `BE-FIN-044` yang menyelaraskan 4 service call points ke katalog resmi dan menghapus 5 alias lama (`FIN-DES-058`). | — |
| ~~Rute backend di luar kontrak~~ | **TERTUTUP 28 September 2026** — ditangani oleh task `BE-FIN-042` yang menyelaraskan 6 controller legacy ke `Finance*` dan rute kanonikal (`FIN-DES-062`). | — |
| **Frontend bergerak `abed49b03` → `49b59cfaa` tanpa verifikasi ulang** — tiga commit menambah `/finance/payable/*`, `/finance/receivable/*`, `/finance/ap-aging`, `/finance/payment-ar`, dan lainnya | Status `FE-FIN-001`..`007` (khususnya `FE-FIN-004` yang tercatat "belum dikerjakan") mungkin sudah tidak cocok dengan source | Yasmin — `trace-existing-capabilities` impact scan frontend |

| ~~**`PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` tanpa pemicu yang terdefinisi**~~ | **TERTUTUP 29 September 2026** oleh `FIN-DES-064`, `FIN-DES-065`, `FIN-DEC-080`, dan `FIN-DEC-081`. Mutasi `RELEASE` berpasangan dengan `REVERSAL` ber-`SettlementId` sama ditetapkan sebagai pemicu resmi. Task `BE-FIN-047` memiliki acceptance criteria lengkap dan teruji pada `FIN-TEST-1.6` §F | — |
| **Eksekusi migration `BE-FIN-022` belum terkonfirmasi** — pengguna menyatakan sebuah migration berhasil 29 September 2026, tetapi dua berkas menunggu pada hari yang sama (`AlterFinBillingHandoffIntakeHandoffTypeCheck` dan `AddDepositAppliedAmountToFinPayment`) | `BE-FIN-025` tidak dapat dipastikan prasyaratnya terpenuhi; status `BE-FIN-022` sengaja dibiarkan 🟡 daripada dinaikkan tanpa bukti | Pemilik repository — konfirmasi satu baris sudah cukup |

Seluruh 20 skenario UAT pada `04-prd-to-mvp.md` bagian 18 sudah tertaut ke task. Yang tersisa
pada tabel di atas adalah gap yang pemiliknya berada **di luar** roadmap ini, ditambah dua gap yang
ditemukan pass perencanaan 29 September 2026 dan pemiliknya justru **di dalam** blueprint ini
sendiri (dua baris terakhir).

## 7. Prasyarat eksekusi

Rinciannya ada di berkas anak: `01-backend-roadmap.md` bagian 6 dan `02-frontend-roadmap.md`
bagian 5. Ringkasnya:

| # | Prasyarat | Status |
|---:|---|---|
| 1 | QBE preflight diselesaikan **pada waktu eksekusi**, dari `AGENTS.md` backend target | Berlaku terus |
| 2 | `BE-FIN-001` selesai sebelum file model pertama | ✅ **Sudah** — selesai 21 September 2026. Lihat [laporan](../task/report/backend/BE-FIN-001.md) |
| 3 | Otorisasi terpisah untuk membuat **dan** menjalankan setiap migration | ✅ **Sudah** — otorisasi pembuatan file diberikan pemilik repository 21 September 2026 (lihat [laporan BE-FIN-003](../task/report/backend/BE-FIN-003.md)); otorisasi **eksekusi** (`dotnet ef database update`) diberikan dan seluruh migration Finance sudah diterapkan — dikonfirmasi pengguna 23 September 2026 |
| 4 | Implementasi lewat `quilvian-engineering-skills:build-module-backend` | Berlaku terus |
| 5 | Task `BLOCKED` MUST NOT dimulai | Berlaku terus |
| 6 | Penguncian versi kontrak | **Terpenuhi** 20 September 2026 |
| 7 | UI brief disetujui Product Owner sebelum task `FE-FIN-*` pertama | ✅ **Sudah** — closed 23 September 2026, `FIN-DEC-024`..`029` di `00-interview-decisions.md` |
| 8 | *(revisi 5)* `BE-FIN-027` sebelum model Purchasing pertama; otorisasi terpisah untuk empat migration `REV-4` | ✅ **`BE-FIN-027` selesai** 26 September 2026 — [laporan](../task/report/backend/BE-FIN-027.md). Otorisasi migration **Belum** — rincian `01-backend-roadmap.md` bagian 6 butir 10, 13 |
| 9 | ~~*(revisi 5)* `FIN-OQ-022` sebelum butir menu layar revisi 4 didaftarkan~~ | ✅ **Terpenuhi** 25 September 2026 — `FIN-DEC-060` |
| 10 | *(revisi 6)* Amendment arsitektur menggambar skema `FIN-DEC-057` sebelum `BE-FIN-036`/`FE-FIN-010` | ✅ **Terpenuhi** 26 September 2026 — AMENDMENT REVISI 5 approved |
| 11 | *(revisi 7)* Otorisasi migration terpisah untuk `BE-FIN-041` — **tabel yang sudah berjalan** | **Belum** |
| 12 | *(revisi 8)* Skrip SQL idempotent migrasi hak peran `SysRolePermissions` disiapkan sebelum `BE-FIN-042` dijalankan di staging/production | Menunggu eksekusi `BE-FIN-042` |
| 13 | *(revisi 8)* Otorisasi migration terpisah untuk `AddPPNAmountToFinSupplierReturn` (`BE-FIN-043`) — **tabel yang sudah berjalan** | **Pembuatan berkas: DIBERIKAN** 29 September 2026. **Eksekusi: BELUM** |

## 8. Risiko

| Risiko | Dampak | Mitigasi |
|---|---|---|
| ~~`BilCollectionHandoff` tidak kunjung dikonfirmasi~~ | **RISIKO GUGUR 25 September 2026** — tabelnya sudah dibangun dan sudah dikonsumsi `FinanceBillingIntakeService` (`FIN-CAP-007`, `FIN-CAP-025`) | — |
| Accounting mengoreksi nama tujuh kode setelah `BE-FIN-023`..`026` dikerjakan | Konstanta `EventTypeCode`, aturan validasi, dan skenario uji harus disesuaikan ulang | Inilah sebabnya keempat task itu ⛔ dan **tidak** diberi nomor gelombang. `FIN-DES-030` sengaja tidak memasang check constraint pada `EventTypeCode` supaya penyesuaian nama **tidak** menuntut migration |
| `BE-FIN-023` dikerjakan tanpa `BE-FIN-024` menyusul | Penerimaan pra-final terbit sebagai `PENERIMAAN-KASIR` — dibukukan sebagai penerimaan final padahal uangnya kewajiban ke pasien | `01-backend-roadmap.md` bagian 4 `REV-3` mewajibkan keduanya satu rangkaian; `BE-FIN-023` **tidak** boleh ditandai selesai sendirian |
| Kontrak terkunci ternyata keliru saat implementasi | Perubahan kontrak `1.0` kini berbiaya lebih mahal daripada saat `draft` | Temuan seperti itu MUST dinaikkan sebagai revisi kontrak bernomor, bukan diselesaikan diam-diam di kode |
| Kas tersedia ternyata menuntut `FinReceipt` | Urutan `MVP-4` sebelum `MVP-2` gugur | Catatan 3.1 mewajibkan temuan itu dilaporkan balik, bukan diakali |
| UI brief tidak kunjung ada | Seluruh `FE-FIN-*` berhenti | Backend tidak tertahan; kontrak fungsional sudah berdiri |
| Endpoint Accounting dibangun lebih cepat dari perkiraan | `EPIC FIN-12` dapat dimulai lebih awal | Kotak keluar sudah terisi sejak `MVP-5`; tinggal mengirim antrean |
| *(revisi 5)* PPN Purchasing Invoice terkredit dua kali di Utang Supplier | Buku besar mencatat utang lebih besar dari faktur sebesar nilai PPN | Acceptance criteria `BE-FIN-034` dan DoD backend #20: kejadian pengakuan utang bernilai pokok, kejadian PPN bernilai PPN, jumlahnya `TotalAmount` |
| *(revisi 5)* Pembayaran tepat Rp 50.000.000 tiba-tiba menuntut Manajer | Pengguna mengira sistem rusak | `BE-FIN-028` diumumkan sebagai perubahan perilaku yang disetujui (`FIN-DEC-052`), bukan regresi — [laporan](../task/report/backend/BE-FIN-028.md) |
| *(revisi 5)* Rizki mengoreksi nama `PPN-MASUKAN-PEMBELIAN` setelah `BE-FIN-034` berjalan | Baris outbox tersimpan dengan nama lama | `EventTypeCode` tanpa check constraint (`FIN-DES-030`) — penyesuaian nama tanpa migration; baris `PENDING` yang belum terkirim dibetulkan datanya sebelum worker aktif |
| *(revisi 5)* Potongan AR dikerjakan lewat jalur pelunasan yang ada | Buku besar mencatat kas yang tidak pernah diterima | `BE-FIN-040` MUST menulis kode `POTONGAN-PIUTANG-NON-TUNAI` (`FIN-DEC-058`), bukan `AR_PAYMENT` |
| *(revisi 7)* Perubahan `FinancePaymentService` (`BE-FIN-036`) merusak pembayaran tanpa deposit | Nilai `AP_PAYMENT` atau `NetTransferAmount` pembayaran lama berubah | Setiap perubahan bersyarat `DepositAppliedAmount > 0`; baris regresi `FIN-TEST-1.3` C.2 wajib lulus |
| *(revisi 7)* Deposit dicadangkan tapi tidak pernah dilepas (pembayaran `DRAFT` ditinggalkan) | Saldo deposit tertahan tanpa batas | Pelepasan tersedia manual selama `DRAFT`; pembatalan pembayaran melepas otomatis. Batas waktu otomatis **tidak** dirancang — bila dibutuhkan, keputusan terpisah |
| *(revisi 8)* Inkonsistensi nama permission pada database staging/production | Endpoint controller menolak akses staf lama karena permission granular belum ada | `BE-FIN-042` menyediakan skrip SQL idempotent migrasi data peran `SysRolePermissions` sebelum rilis |
| *(revisi 8)* Deposit retur tidak memperhitungkan PPN | Saldo deposit retur kurang dari uang retur riil | `BE-FIN-043` memastikan deposit retur saat `CONFIRMED` mencakup `TotalAmount + PPNAmount` per `FIN-DES-055` |

## 9. Yang roadmap ini tidak lakukan

- Tidak menandai satu pun task selesai.
- Tidak menetapkan approval sendiri: ketiga approval pada revisi 2 ini diberikan owner secara langsung dan dicatat apa adanya.
- Tidak menyembunyikan satu pun dependency eksternal: keempat pemiliknya disebut namanya.
- Tidak menjalankan builder mana pun.
- Tidak mengubah cakupan MVP yang sudah dikunci owner.

---

# AMENDMENT ROADMAP REVISI 13 — `EPIC FIN-18` dan `EPIC FIN-19`

```yaml
roadmap_revision: 13
roadmap_status: DRAFT
blueprint_revision: 13
blueprint_status: draft — FIN-DES-070..077 dan lima kontrak turunannya BELUM disetujui owner
decisions: FIN-DEC-094..FIN-DEC-106 (approved)
backend_source_sha: d6978487
frontend_source_sha: d2e8a3538
tanggal: 1 Oktober 2026
```

## Dua epic baru

| Epic | Isi | Pemicu |
|---|---|---|
| `EPIC FIN-18` | Pelacakan klaim penjamin pada tagihan gabungan, dan penyelarasan seluruh menu Transaksi A/R dan A/P ke bentuk sistem produksi V1 | `FIN-DEC-094`..`098`, `105`, `106` |
| `EPIC FIN-19` | Piutang sewa non-pasien: tagihan parkir dan tenant beserta umur piutangnya | `FIN-DEC-099`..`104` |

## Gelombang

| Gelombang | Epic | Task | Status |
|---|---|---|---|
| `REV-13A` | `FIN-18` | `FE-FIN-016` 🟡, `FE-FIN-017` 🟡, `FE-FIN-018` 🟡 | 🟡 Seluruh `REV-13A` source lengkap, lint PASS, `npm run build` NOT RUN ([016](../task/report/frontend/FE-FIN-016.md), [017](../task/report/frontend/FE-FIN-017.md), [018](../task/report/frontend/FE-FIN-018.md)) — **nol pekerjaan backend**, hasil terlihat paling cepat |
| `REV-13B` | `FIN-18` | `BE-FIN-053` ✅, `BE-FIN-054` ✅, `BE-FIN-055` ✅ · `FE-FIN-019` 🟡, `FE-FIN-021` ✅ | ✅ Backend selesai 1 Oktober 2026 ([054](../task/report/backend/BE-FIN-054.md), [055](../task/report/backend/BE-FIN-055.md)); 🟡 `FE-FIN-019` source lengkap, lint PASS, build NOT RUN ([laporan](../task/report/frontend/FE-FIN-019.md)); ✅ `FE-FIN-021` selesai 1 Oktober 2026, lint & build PASS ([laporan](../task/report/frontend/FE-FIN-021.md)) |
| `REV-13C` | `FIN-18` | `BE-FIN-052` ✅ · `FE-FIN-020` ✅ | ✅ Selesai 1 Oktober 2026 — Backend: `BE-FIN-052` `dotnet build` PASS, migration berhasil dieksekusi ([laporan](../task/report/backend/BE-FIN-052.md)); Frontend: `FE-FIN-020` `npm run lint:errors` PASS, `npm run build` PASS ([laporan](../task/report/frontend/FE-FIN-020.md)) |
| `REV-13D` | `FIN-19` | `BE-FIN-056` ✅, `BE-FIN-057` ✅ · `FE-FIN-022` ✅, `FE-FIN-023` ✅ | ✅ Backend selesai 1 Oktober 2026 ([056](../task/report/backend/BE-FIN-056.md), [057](../task/report/backend/BE-FIN-057.md)); ✅ Frontend selesai 1 Oktober 2026 — `FE-FIN-022` ([laporan](../task/report/frontend/FE-FIN-022.md)), `FE-FIN-023` ([laporan](../task/report/frontend/FE-FIN-023.md)), seluruh lint & build PASS |
| **Di luar gelombang** | `FIN-19` | Integrasi pelunasan sewa ke kas harian, setoran bank, dan kejadian akuntansi | **Tertahan `FIN-OQ-044`** — sengaja belum bernomor |

Keempat gelombang `REV-13A`..`13D` **saling bebas** di tingkat backend; yang mengikat hanya
`FE-FIN-016` sebagai prasyarat seluruh task frontend, dan `BE-FIN-056` sebelum `BE-FIN-057`.

## Register task backend

| Task ID | Outcome | Gelombang | Status |
|---|---|---|---|
| ✅ `BE-FIN-052` | Sumbu klaim penjamin pada `FinReceivableInvoiceBatch` (7 kolom, migration, 3 endpoint aksi) | `REV-13C` | ✅ Selesai 1 Oktober 2026 — source dan migration lengkap, `dotnet build` PASS, migration berhasil dieksekusi ke database (keduanya dikonfirmasi pengguna) ([laporan](../task/report/backend/BE-FIN-052.md)) |
| ✅ `BE-FIN-053` | `GET /receivables/write-offs` — daftar penghapusan piutang lintas piutang | `REV-13B` | ✅ Selesai 1 Oktober 2026 — source lengkap, `dotnet build` PASS (dikonfirmasi pengguna) ([laporan](../task/report/backend/BE-FIN-053.md)) |
| ✅ `BE-FIN-054` | `GET /receipts/reversed-allocations` — daftar alokasi penerimaan yang dibalik | `REV-13B` | ✅ Selesai 1 Oktober 2026 — source lengkap dengan satu delta kontrak dicatat (`ReversalReason` dicabut, bukan blocker), `dotnet build` PASS dikonfirmasi pengguna ([laporan](../task/report/backend/BE-FIN-054.md)) |
| ✅ `BE-FIN-055` | Saringan jenis debitur pada `GET /receivables/aging` (koreksi: bukan "segmen Kasir") | `REV-13B` | ✅ Selesai 1 Oktober 2026 — source lengkap dengan koreksi rancangan dicatat, `dotnet build` PASS dikonfirmasi pengguna ([laporan](../task/report/backend/BE-FIN-055.md)) |
| ✅ `BE-FIN-056` | Skema piutang sewa non-pasien (2 model, 2 configuration, 1 migration) | `REV-13D` | ✅ Selesai 1 Oktober 2026 — source+migration lengkap, `dotnet build` PASS dan migration berhasil dieksekusi ke database, keduanya dikonfirmasi pengguna ([laporan](../task/report/backend/BE-FIN-056.md)) |
| ✅ `BE-FIN-057` | Layanan dan 10 endpoint piutang sewa non-pasien | `REV-13D` | ✅ Selesai 1 Oktober 2026 — 1 service, 1 controller, 10 endpoint, 13 DTO lengkap; **nol** migration dibutuhkan (skema sudah di `BE-FIN-056`); `dotnet build` PASS dikonfirmasi pengguna ([laporan](../task/report/backend/BE-FIN-057.md)) |

## Register task frontend

| Task ID | Outcome | Gelombang | Status |
|---|---|---|---|
| 🟡 `FE-FIN-016` | Menu Keuangan berbentuk dua grup datar mengikuti V1; submenu "Pembelian" dicabut | `REV-13A` | 🟡 Sebagian 1 Oktober 2026 — source lengkap, `npm run lint:errors` PASS, `npm run build` NOT RUN ([laporan](../task/report/frontend/FE-FIN-016.md)) |
| 🟡 `FE-FIN-017` | Tujuh layar pandangan tersaring rumpun A/R | `REV-13A` | 🟡 Sebagian 1 Oktober 2026 — 5 hook, 7 view, 7 route lengkap; `npm run lint:errors` PASS; `npm run build` NOT RUN ([laporan](../task/report/frontend/FE-FIN-017.md)) |
| 🟡 `FE-FIN-018` | Lima layar pandangan tersaring rumpun A/P beserta daftar tanda terima barang | `REV-13A` | 🟡 Sebagian 1 Oktober 2026 — 9 hook, 9 view, 9 route lengkap; `npm run lint:errors` PASS; `npm run build` NOT RUN ([laporan](../task/report/frontend/FE-FIN-018.md)) |
| 🟡 `FE-FIN-019` | Layar alokasi yang dibalik dan penghapusan piutang (baca saja) | `REV-13B` | 🟡 Sebagian 1 Oktober 2026 — 2 hook, 2 view, 2 route lengkap; `npm run lint:errors` PASS; `npm run build` NOT RUN ([laporan](../task/report/frontend/FE-FIN-019.md)) |
| ✅ `FE-FIN-020` | Layar Manajemen Klaim — dua sumbu status berdampingan | `REV-13C` | ✅ Selesai 1 Oktober 2026 — 2 hook, 2 view, 2 route, integrasi menu dan navigasi batch; `npm run lint:errors` PASS; `npm run build` PASS ([laporan](../task/report/frontend/FE-FIN-020.md)) |
| ✅ `FE-FIN-021` | Umur piutang pasien per segmen Kasir (Umur Piutang — Kasir) | `REV-13B` | ✅ Selesai 1 Oktober 2026 — pendaftaran butir menu anak grup Umur Piutang (A/R Aging), endpoint governed, metadata & breadcrumb; `npm run lint:errors` PASS; `npm run build` PASS ([laporan](../task/report/frontend/FE-FIN-021.md)) |
| ✅ `FE-FIN-022` | Layar tagihan sewa beserta butir menu "Tagihan Sewa" (`FIN-DEC-105`) | `REV-13D` | ✅ Selesai 1 Oktober 2026 — source lengkap (2 hook, 2 view, 2 route, 1 columns, modal set), `npm run lint:errors` PASS, `npm run build` PASS ([laporan](../task/report/frontend/FE-FIN-022.md)) |
| ✅ `FE-FIN-023` | Umur piutang sewa Parkir dan Tenant | `REV-13D` | ✅ Selesai 1 Oktober 2026 — source lengkap (1 hook, 1 view, 1 route, constants, menu), `npm run lint:errors` PASS, `npm run build` PASS ([laporan](../task/report/frontend/FE-FIN-023.md)) |

## Traceability

| Kebutuhan | Keputusan | Rancangan | Kontrak | Backend | Frontend | Bukti verifikasi | Status |
|---|---|---|---|---|---|---|---|
| Menu Transaksi A/R dan A/P mengikuti bentuk V1 | `FIN-DEC-094`, `105` | `FIN-DES-073` | `03-frontend-architecture.md` 17.2 | — | `FE-FIN-016` 🟡, `017` 🟡, `018` 🟡, `022` ✅ | `FIN-TEST-1.6` G.1 | 🟡 `FE-FIN-016`/`017`/`018` source lengkap, lint PASS, build NOT RUN ([016](../task/report/frontend/FE-FIN-016.md), [017](../task/report/frontend/FE-FIN-017.md), [018](../task/report/frontend/FE-FIN-018.md)); `FE-FIN-022` ✅ selesai ([laporan](../task/report/frontend/FE-FIN-022.md)) |
| Pelacakan klaim penjamin | `FIN-DEC-095`, `097`, `098` | `FIN-DES-070`..`072` | `FIN-API-1.3` D.1, `FIN-STATE-1.4` D.1 | `BE-FIN-052` ✅ | `FE-FIN-020` ✅ | `FIN-TEST-1.6` G.1 | ✅ Selesai 1 Oktober 2026 — Backend build+migration PASS ([laporan](../task/report/backend/BE-FIN-052.md)); Frontend lint+build PASS ([laporan](../task/report/frontend/FE-FIN-020.md)) |
| Ayat Silang | `FIN-DEC-096` | — | — | **Nol task** | `FE-FIN-016` 🟡 (butir menu saja) | `FIN-TEST-1.6` G.1 | 🟡 Butir menu "Ayat Silang" terdaftar, `npm run build` belum dikonfirmasi ([laporan](../task/report/frontend/FE-FIN-016.md)) — ditutup sebagai kapabilitas yang sudah ada |
| Daftar penghapusan piutang dan alokasi yang dibalik | `FIN-DEC-094` | `FIN-DES-073` | `FIN-API-1.3` D.2 | `BE-FIN-053` ✅, `054` ✅ | `FE-FIN-019` 🟡 | `FIN-TEST-1.6` G.1 | 🟡 Backend ✅ selesai ([053](../task/report/backend/BE-FIN-053.md), [054](../task/report/backend/BE-FIN-054.md)); `FE-FIN-019` 🟡 source lengkap, lint PASS, build NOT RUN ([laporan](../task/report/frontend/FE-FIN-019.md)) |
| Umur piutang per segmen | `FIN-OQ-040` (terjawab) | — | `FIN-API-1.4` | `BE-FIN-055` ✅ | `FE-FIN-021` ✅ | `FIN-TEST-1.7` H.1 | ✅ Selesai 1 Oktober 2026 — Backend: `BE-FIN-055` selesai, build PASS ([laporan](../task/report/backend/BE-FIN-055.md)); Frontend: `FE-FIN-021` selesai, lint & build PASS ([laporan](../task/report/frontend/FE-FIN-021.md)) |
| Piutang sewa parkir dan tenant | `FIN-DEC-099`..`104` | `FIN-DES-074`..`077` | `FIN-API-1.4` E.1, `FIN-STATE-1.5` E.1 | `BE-FIN-056` ✅, `057` ✅ | `FE-FIN-022` ✅, `023` ✅ | `FIN-TEST-1.7` H.1 | ✅ Selesai 1 Oktober 2026 — Backend: `BE-FIN-056` build+migration PASS ([056](../task/report/backend/BE-FIN-056.md)), `BE-FIN-057` build PASS ([057](../task/report/backend/BE-FIN-057.md)); Frontend: `FE-FIN-022` ([022](../task/report/frontend/FE-FIN-022.md)), `FE-FIN-023` ([023](../task/report/frontend/FE-FIN-023.md)), seluruh lint & build PASS |
| Pelunasan sewa tercatat sebagai kas masuk dan kejadian akuntansi | **Belum ada** | **Belum ada** | **Belum ada** | **Belum ada** | **Belum ada** | **Belum ada** | ⛔ **GAP TERBUKA** — `FIN-OQ-044` |

Baris terakhir adalah **gap yang disengaja dan dicatat terbuka**, bukan kelalaian traceability:
bagian kode kejadiannya menuntut ratifikasi Accounting dan bukan wewenang Finance sepihak.

## Prasyarat sebelum eksekusi

| # | Prasyarat | Keadaan |
|---:|---|---|
| 1 | Approval owner atas `FIN-DES-070`..`077` dan kelima kontrak turunannya | ✅ **Diberikan** 1 Oktober 2026 ("saya setujui") |
| 2 | Izin membuat migration | `BE-FIN-052` ✅ diberikan dan dibuat 1 Oktober 2026; `BE-FIN-056` ✅ diberikan dan dibuat 1 Oktober 2026 (tanpa `dotnet build`, permintaan eksplisit pengguna) |
| 3 | Izin mengeksekusi migration ke database | `BE-FIN-052` ✅ diberikan dan **berhasil dieksekusi** 1 Oktober 2026; `BE-FIN-056` ✅ diberikan dan **berhasil dieksekusi** 1 Oktober 2026 |
| 4 | Pemilik mengetahui batas `FIN-OQ-044` sebelum `EPIC FIN-19` dipakai pada data sungguhan | **MUST** disampaikan saat approval |

---

# REV-14 — Traceability dan ringkasan gelombang

```yaml
roadmap_revision: REV-14
blueprint_id: FIN-BP-001
blueprint_revision: 14
blueprint_status: approved — FIN-DES-078..091 disetujui owner 1 Oktober 2026
decisions: FIN-DEC-111..FIN-DEC-138
contract_versions: FIN-API-1.5, FIN-INTEGRATION-1.7, FIN-STATE-1.6, FIN-VAL-1.7, FIN-PERM-1.7,
                   FIN-TEST-1.8, FIN-MVP-1.9 (seluruhnya approved 1 Oktober 2026)
backend_source_sha: 7f8c3014
frontend_source_sha: 0b54fdce6
jumlah_task: 16 backend, 5 frontend
tanggal: 1 Oktober 2026
```

## Ringkasan gelombang REV-14

| Gelombang | Epic | Task backend | Task frontend | Migration | Gerbang yang berlaku |
|---|---|---|---|---|---|
| `REV-14A` | `EPIC FIN-20` — buku mutasi dan tanggal WIB | `BE-FIN-058`..`063` (6) | `FE-FIN-025` ✅, `FE-FIN-026` ✅ (2) | `AddFinanceSubledgerMovementLedgers` | Menu tertahan `FIN-OQ-079`; eksekusi migration milik Yasmin |
| `REV-14B` | `EPIC FIN-21` — pemetaan akun control dan saldo awal | `BE-FIN-064`..`068` (5) | `FE-FIN-027`..`029` (3) | `AddFinanceSubledgerSetup` | Isi kode akun menunggu **G2**; menu tertahan `FIN-OQ-079` |
| `REV-14C` | `EPIC FIN-22` — jalur pengiriman | `BE-FIN-069`..`073` (5) | — | **nol** | **Pengaktifan** menunggu **G3** dan `FIN-OQ-047`; pembangunan tidak tertahan |
| `REV-14D` | `EPIC FIN-23` — pembayaran langsung berkontrol beserta bukti | `BE-FIN-074`..`078` (5) | `FE-FIN-030` 🟡, `FE-FIN-031` (2) | `AddFinanceTransactionProofAndDirectPaymentThreshold` | **Perubahan memutus** pada dua endpoint berjalan — urutan rilis layar **MUST** dijaga; `FIN-OQ-074`/`082` fail-closed; menu tertahan `FIN-OQ-079` |
| `REV-14E` | `EPIC FIN-24` — migrasi tagihan lama, dua format | `BE-FIN-079`..`083` (5) | `FE-FIN-032` (1) | `AddFinanceOpeningItemMigration` | `BE-FIN-083` ⛔ **BLOCKED** `FIN-OQ-081`; jalur CSV **tidak** tertahan; menu tertahan `FIN-OQ-079` |

> **Diperbarui 2 Oktober 2026 (revisi 15).** Kedua baris di atas sebelumnya berbunyi
> ~~*(di luar gelombang) — `EPIC FIN-23`/`FIN-24` — `OPEN DECISION` — `FIN-OQ-075`/`FIN-OQ-077`*~~.
> Kedua gerbang itu **sudah dijawab** (`FIN-DEC-139`, `FIN-DEC-140`), desainnya digambar dan
> **disetujui** 2 Oktober 2026, dan task-nya kini bernomor. Rincian rencananya pada
> `01-backend-roadmap.md` dan `02-frontend-roadmap.md` bagian REV-14D/14E.

**Migration berisiko tertinggi kini punya gelombang, dan itu perubahan keadaan yang paling penting
dari revisi 15.** Sebelumnya `AddFinanceOpeningItemMigration` — satu-satunya migration revisi 14 yang
menyentuh **tabel berjalan** — tidak masuk gelombang mana pun karena epic pemiliknya `OPEN DECISION`.
Ia sekarang dibawa **`BE-FIN-079`** pada `REV-14E`, lengkap dengan kewajiban mengikuti urutan
`NOT VALID`/`VALIDATE` dan `CREATE INDEX CONCURRENTLY`, serta menyatakan pada laporannya urutan DDL
mana yang dipakai. Wewenang **membuat** migration sudah ada (`FIN-DEC-138`); **menerapkannya** tetap
milik Yasmin.

Keempat migration revisi 14 karena itu kini terdistribusi: dua murni aditif pada `REV-14A`/`14B`, satu
murni aditif pada `REV-14D`, dan satu yang menyentuh tabel berjalan pada `REV-14E`.

## Traceability REV-14

| Requirement | Decision | Desain | Kontrak | Task backend | Task frontend | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `FR-FIN-130`..`132` | `FIN-DEC-123` | `FIN-DES-079` | `erd` R14.1-R14.3 | `BE-FIN-058` ✅, `BE-FIN-060` ✅, `BE-FIN-061` ✅, `BE-FIN-062` ✅ | — | `FIN-TEST-1.8` I.1 | ✅ Selesai 2 Oktober 2026. Skema subledger berdiri (`BE-FIN-058`), mutasi piutang (`BE-FIN-060`), utang supplier (`BE-FIN-061`), dan mutasi kas (`BE-FIN-062`) tersambung penuh. Bukti: [laporan BE-FIN-062](../task/report/backend/BE-FIN-062.md) |
| `FR-FIN-133` | `FIN-DEC-123` | `FIN-DES-079` | `FIN-VAL-1.7` `165`..`167` | `BE-FIN-060` ✅, `BE-FIN-061` ✅ | — | `FIN-TEST-1.8` I.1 | ✅ Selesai 2 Oktober 2026. Penegakan invariant mutasi piutang (`BE-FIN-060`) dan utang supplier (`BE-FIN-061`) selesai penuh. Bukti: [laporan BE-FIN-061](../task/report/backend/BE-FIN-061.md) |
| `FR-FIN-134`, `135` | `FIN-DEC-114` | `FIN-DES-081` | `FIN-API-1.5` F.6; `FIN-VAL-1.7` `170` | `BE-FIN-067` ✅ | `FE-FIN-029` | `FIN-TEST-1.8` I.1 | Parsial (`BE-FIN-067` selesai — service kalkulator posisi subledger murni buku mutasi dan endpoint posisi selesai; menunggu `FE-FIN-029`). Bukti: [laporan BE-FIN-067](../task/report/backend/BE-FIN-067.md) |
| `FR-FIN-136` | `FIN-DEC-116` | `FIN-DES-082` | `FIN-VAL-1.7` `210` | `BE-FIN-058` ✅, `BE-FIN-059` ✅ | — | `FIN-TEST-1.8` I.4 | ✅ Selesai 2 Oktober 2026. Helper `FinanceBusinessDate` dan penyelarasan 21+ titik selesai. Bukti: [laporan BE-FIN-059](../task/report/backend/BE-FIN-059.md) |
| `FR-FIN-137`, `138` | `FIN-DEC-124`, `125`, `127`, `132`, `133` | `FIN-DES-081` | `erd/cash-and-master-data.md` rev 14 | `BE-FIN-062` ✅ | `FE-FIN-026` ✅ | `FIN-TEST-1.8` I.2 | ✅ Selesai 2 Oktober 2026. Arus kas masuk/keluar terhubung penuh dan layar Buku Mutasi Kas selesai. Bukti: [laporan BE-FIN-062](../task/report/backend/BE-FIN-062.md), [laporan FE-FIN-026](../task/report/frontend/FE-FIN-026.md) |
| `FR-FIN-139` | `FIN-DEC-125` | `FIN-DES-081` | `FIN-API-1.5` F.6 | `BE-FIN-067` ✅ | `FE-FIN-029` | `FIN-TEST-1.8` I.2 | Parsial (`BE-FIN-067` selesai — endpoint perbandingan selisih kas harian dan mutasi penjelas selesai; menunggu `FE-FIN-029`). Bukti: [laporan BE-FIN-067](../task/report/backend/BE-FIN-067.md) |
| `FR-FIN-140` | `FIN-DEC-123` | `FIN-DES-079` | `FIN-API-1.5` F.5 | `BE-FIN-063` ✅ | `FE-FIN-025` ✅, `FE-FIN-026` ✅ | `FIN-TEST-1.8` I.1 | ✅ Selesai 2 Oktober 2026. Tiga endpoint baca mutasi dan seluruh antarmuka buku mutasi (piutang, utang, kas) selesai penuh. Bukti: [laporan BE-FIN-063](../task/report/backend/BE-FIN-063.md), [laporan FE-FIN-025](../task/report/frontend/FE-FIN-025.md), [laporan FE-FIN-026](../task/report/frontend/FE-FIN-026.md) |
| `FR-FIN-141`, `144` | `FIN-DEC-113` | `FIN-DES-080` | `FIN-API-1.5` F.1; `FIN-VAL-1.7` `172`..`179` | `BE-FIN-064` ✅, `BE-FIN-065` ✅ | `FE-FIN-027` | `FIN-TEST-1.8` I.3 | Parsial (`BE-FIN-064` dan `BE-FIN-065` selesai — skema dan endpoint pemetaan akun control subledger selesai penuh; menunggu `FE-FIN-027`). Bukti: [laporan BE-FIN-065](../task/report/backend/BE-FIN-065.md) |
| `FR-FIN-142`, `143`, `145`, `149` | `FIN-DEC-112`, `113`, `122` | `FIN-DES-080`, `091` | `FIN-INTEGRATION-1.7` 5.12.4 | `BE-FIN-068` ✅ | `FE-FIN-029` | `FIN-TEST-1.8` I.3 | Parsial (`BE-FIN-068` selesai — perombakan snapshot subledger, fail-closed gate, dan penghapusan Math.Max selesai; menunggu `FE-FIN-029`). Bukti: [laporan BE-FIN-068](../task/report/backend/BE-FIN-068.md) |
| `FR-FIN-146`..`148` | `FIN-DEC-128` | `FIN-DES-088` | `FIN-STATE-1.6` F.1; `FIN-VAL-1.7` `180`..`185` | `BE-FIN-064` ✅, `BE-FIN-066` ✅ | `FE-FIN-028` | `FIN-TEST-1.8` I.7 | Parsial (`BE-FIN-064` dan `BE-FIN-066` selesai — skema dan endpoint siklus hidup saldo awal cutover selesai penuh; menunggu `FE-FIN-028`). Bukti: [laporan BE-FIN-066](../task/report/backend/BE-FIN-066.md) |
| `FR-FIN-150`..`153` | `FIN-DEC-118`, `093` | `FIN-DES-078` | `FIN-INTEGRATION-1.7` 5.12.6 | `BE-FIN-071` ✅ | — | `FIN-TEST-1.8` I.5 | Selesai — worker dibangun mati (`Enabled = false`); 8 kode dalam `GatedEventTypeCodes`; pengaktifan menunggu G3 dan gerbang Accounting. Bukti: [laporan BE-FIN-071](../task/report/backend/BE-FIN-071.md) |
| `FR-FIN-154`, `155` | `FIN-DEC-092`, `114`, `118` | `FIN-DES-078` | `FIN-INTEGRATION-1.7` 5.12.5 | `BE-FIN-072` 🟡 | — | `FIN-TEST-1.8` I.3 | 🟡 Sebagian 2 Oktober 2026 — source selesai (penjadwal harian + endpoint restate terpasang, mati secara bawaan), `dotnet build` **NOT RUN** atas permintaan eksplisit pengguna. Bukti: [laporan BE-FIN-072](../task/report/backend/BE-FIN-072.md) |
| `FR-FIN-156` | `FIN-DEC-118` | `FIN-DES-078` | `FIN-INTEGRATION-1.7` 5.12.6 | `BE-FIN-073` 🟡 | — | `FIN-TEST-1.8` I.5 | 🟡 Sebagian 2 Oktober 2026 — source selesai (penjadwal interval terpasang, mati secara bawaan), `dotnet build` **NOT RUN** atas permintaan eksplisit pengguna. Bukti: [laporan BE-FIN-073](../task/report/backend/BE-FIN-073.md) |
| `FR-FIN-157` | `FIN-DEC-115`, `121` | `FIN-DES-084` | `FIN-INTEGRATION-1.7` 5.12.3; `FIN-STATE-1.6` F.3 | `BE-FIN-070` ✅ | — | `FIN-TEST-1.8` I.5 | Parsial (`BE-FIN-070` selesai — kode `PEMBUKAAN-SHIFT-KASIR` terbit untuk 7 status, idempotensi terjaga; **pengiriman** masih tertahan `FIN-OQ-047`; penjadwal `BE-FIN-073` belum dibangun). Bukti: [laporan BE-FIN-070](../task/report/backend/BE-FIN-070.md) |
| `FR-FIN-158`, `159` | `FIN-DEC-111`, `120` | `FIN-DES-083` | `FIN-INTEGRATION-1.7` 5.12.1-5.12.2 | `BE-FIN-069` 🟡 | — | `FIN-TEST-1.8` I.5 | 🟡 Sebagian 2 Oktober 2026 — source selesai (lima ruas terpasang dan terisi pada jalur `FinanceReceiptService`), `dotnet build` **NOT RUN** atas permintaan eksplisit pengguna. **Kontraknya** masih tertahan `FIN-OQ-045`. Bukti: [laporan BE-FIN-069](../task/report/backend/BE-FIN-069.md) |
| `FR-FIN-160`..`166` | `FIN-DEC-126`, `130`, `131`, `134`, `135`, `137` | `FIN-DES-085`..`087` | `FIN-API-1.5` F.3, F.4, F.8 | — | — | `FIN-TEST-1.8` I.6 | **Diperbarui revisi 15:** ~~`OPEN DECISION`~~ → kini `REV-14D`, dibawa `BE-FIN-074`..`078` dan `FE-FIN-030`/`031`. Traceability rincinya pada bagian REV-14D/14E di bawah |
| `FR-FIN-167`..`173` | `FIN-DEC-129`, `136` | `FIN-DES-089`, `090` | `FIN-STATE-1.6` F.2; `FIN-VAL-1.7` `186`..`196` | — | — | `FIN-TEST-1.8` I.7 | **Diperbarui revisi 15:** ~~`OPEN DECISION`~~ → kini `REV-14E`, dibawa `BE-FIN-079`..`083` dan `FE-FIN-032`. Traceability rincinya pada bagian REV-14D/14E di bawah |

## Coverage gap

Dibedakan antara gap yang **MUST** ditutup dan ketiadaan yang memang kebijakan.

| # | Gap | Sifat | Tindakan |
|---:|---|---|---|
| 1 | ~~`FR-FIN-160`..`173` (14 requirement) tidak punya task~~ | **DITUTUP 2 Oktober 2026 (revisi 15).** Keempat belas requirement itu kini punya task: `BE-FIN-074`..`083` dan `FE-FIN-030`..`032`, ditambah sepuluh requirement baru `FR-FIN-174`..`183` | Ditutup lewat `/plan-module-delivery` lanjutan sesudah `FIN-DEC-139`/`140` dijawab — persis seperti yang direncanakan |
| 2 | Buku mutasi utang jasa medis tidak punya requirement maupun task | **Gap sah** — `FinMedicalServicePayable` belum punya penulis apa pun (`BE-FIN-021` `BLOCKED`) | Kewajiban membangunnya **bersamaan** dengan `BE-FIN-021` dicatat `FIN-DES-091`. `BE-FIN-021` **MUST NOT** dikerjakan tanpanya |
| 3 | Penempatan tujuh butir menu baru tidak punya task bernomor | **Gap sah** — tertahan `FIN-OQ-079`, dan ia keputusan pemilik | Diberi nomor sesudah dijawab |
| 4 | Ketiadaan project automated test | **Bukan coverage gap** — mengikuti `rules/backend/TEST_POLICY.md`. Verifikasi memakai `dotnet build` ditambah kasus uji manual yang dilampirkan pada laporan task | Tidak ada tindakan |
| 5 | `FIN-TEST-1.8` I.8 mendaftar tujuh hal yang **sengaja tidak diuji** | **Bukan gap** — masing-masing beserta alasannya, antara lain pembacaan spreadsheet (paketnya belum ada) dan pengiriman ke lingkungan nyata (kredensial G3 belum ada) | Tidak ada tindakan; ditinjau ulang bila gerbangnya terbuka |
| 6 | Posisi saldo untuk tanggal **sebelum** cutover tidak tercakup | **Bukan gap** — penolakannya justru **diuji** (`FIN-TEST-1.8` I.1). Buku mutasi tidak diisi mundur, dan itu keputusan sadar | Tidak ada tindakan |

## Pernyataan wajib pada setiap handoff implementasi backend REV-14

| # | Pernyataan |
|---:|---|
| 1 | QBE preflight dan kesesuaian engineering diselesaikan **pada waktu eksekusi**, dari `AGENTS.md` backend target, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, dan `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` — **bukan** dari roadmap ini |
| 2 | Delapan model baru revisi 14 berstatus `NEW CODE`. Pola legacy di sekitarnya **tidak** memberi wewenang menirunya |
| 3 | **Nol** folder submodul baru, sehingga **nol** gerbang `QBE-MOD-003` baru. Prefix `Fin` dan `Mst` sudah terdaftar |
| 4 | Migration **dibuat** atas `FIN-DEC-138`, dan **MUST NOT dijalankan** agent. Setiap laporan task yang membawa migration **MUST** menyatakannya belum dieksekusi beserta langkah yang Yasmin perlu jalankan |
| 5 | Branch, commit, dan push adalah wewenang terpisah dan **MUST NOT** dijalankan tanpa instruksi eksplisit |
| 6 | Task yang menyentuh service berjalan (`BE-FIN-059`..`062`, `BE-FIN-068`, `BE-FIN-069`) **MUST** membuktikan tidak ada regresi pada jalur yang sudah berjalan, bukan hanya membuktikan jalur barunya |

## Risiko REV-14 yang perlu diketahui pemilik sebelum eksekusi

| # | Risiko | Task terdampak | Mitigasi yang sudah dirancang |
|---:|---|---|---|
| 1 | **Satu jalur pengubah saldo terlewat**, sehingga posisi saldo salah tanpa ada yang tahu | `BE-FIN-060`, `BE-FIN-061`, `BE-FIN-062` | Invariant `BalanceAfter` baris terakhir sama dengan sisa agregat (`FIN-VAL-167`), beserta test yang **sengaja** dibuat untuk menangkapnya |
| 2 | **Kas keluar dihitung dari jumlah alokasi**, bukan `NetTransferAmount`, sehingga kas melebih-hitung setiap kali ada potongan | `BE-FIN-062` | Ditulis eksplisit pada `FIN-DES-081`, pada ERD `payable.md`, dan pada acceptance criteria task |
| 3 | **Satu dari 21 titik tanggal terlewat**, sehingga dua konvensi tanggal hidup bersamaan | `BE-FIN-059` | Daftar 21 titik **MUST** dilampirkan satu per satu pada laporan task |
| 4 | **Snapshot mengirim sebagian** saat pemetaan tidak lengkap | `BE-FIN-068` | Gagal tertutup dengan nol baris outbox, diuji tersendiri untuk cakupan sebagian |
| 5 | **Kredensial ditanamkan di source** worker pengiriman | `BE-FIN-071` | Kredensial **MUST** dari konfigurasi; G3 masih terbuka dan pengaktifannya menunggu |
| 6 | **Layar snapshot mempertahankan anggapan empat baris**, sehingga akun kelima tidak tampil | `FE-FIN-029` | Ditulis pada acceptance criteria; `IsComplete` di backend juga berhenti memakai angka empat |
| 7 | **Layar menjanjikan pemisahan penyiap dan penyetuju** yang tidak dijamin mesin hak akses | `FE-FIN-028` | Dicatat `FIN-PERM-1.7` G.5 dan pada acceptance criteria task; **MUST** disampaikan saat menyerahkan modul |

---

# REV-14D dan REV-14E — Traceability dan ringkasan

```yaml
blueprint_id: FIN-BP-001
roadmap_revision: REV-14D-14E
blueprint_revision: 15
status: SIAP DIEKSEKUSI
contract_versions: [FIN-API-1.6, FIN-VAL-1.8, FIN-PERM-1.8, FIN-TEST-1.9, FIN-MVP-1.10]
contract_status: approved 2026-10-02 (Yasmin)
epics: [EPIC FIN-23, EPIC FIN-24]
tasks_backend: BE-FIN-074..BE-FIN-083
tasks_frontend: FE-FIN-030..FE-FIN-032
```

## Traceability REV-14D dan REV-14E

| Requirement | Decision | Desain | Kontrak | Task backend | Task frontend | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `FR-FIN-160`..`162` | `FIN-DEC-126`, `132`, `133` | `FIN-DES-085` | `FIN-API-1.6` F.3/F.8 | `BE-FIN-077`, `BE-FIN-078` | `FE-FIN-030` 🟡 | `FIN-TEST-1.9` J.1 | Direncanakan |
| `FR-FIN-163`..`165` | `FIN-DEC-134` | `FIN-DES-086` | `FIN-API-1.6` F.4 | `BE-FIN-076`, `BE-FIN-077`, `BE-FIN-078` | `FE-FIN-030` 🟡, `FE-FIN-031` | `FIN-TEST-1.9` J.1 | Direncanakan. Nilai ambang `FIN-OQ-074` **belum ada** — perilaku tanpa ambang memang menolak semuanya |
| `FR-FIN-166` | `FIN-DEC-124` | `FIN-DES-083` | `FIN-INTEGRATION-1.7` 5.12 | `BE-FIN-069` (REV-14C) — **dipakai, bukan dibangun ulang** | — | `FIN-TEST-1.8` I.5 | Mengikuti status `BE-FIN-069` |
| `FR-FIN-174`, `175`, `177` | `FIN-DEC-139` | `FIN-DES-092` | `FIN-API-1.6` F.3; `FIN-VAL-1.8` `214`..`221` | `BE-FIN-075` | `FE-FIN-030` 🟡 | `FIN-TEST-1.9` J.2 | Direncanakan. `FIN-OQ-082` **belum ada** — unggah ditolak `503` fail-closed |
| `FR-FIN-176` | `FIN-DEC-139` | `FIN-DES-092` | `FIN-API-1.6` F.3 | `BE-FIN-075` (**nol** `PUT`/`DELETE`) | `FE-FIN-030` 🟡 (**nol** tombol ganti) | `FIN-TEST-1.9` J.2.5 | Direncanakan |
| `FR-FIN-167`, `182` | `FIN-DEC-140` | `FIN-DES-093` | `FIN-API-1.6` F.2 | `BE-FIN-080` (CSV), `BE-FIN-083` ⛔ (XLSX) | `FE-FIN-032` | `FIN-TEST-1.9` J.3.3 | Direncanakan; bagian XLSX ⛔ `FIN-OQ-081` |
| `FR-FIN-168`, `178`, `181` | `FIN-DEC-136`, `140` | `FIN-DES-093` | `FIN-API-1.6` F.2; `FIN-VAL-1.8` `224`..`227` | `BE-FIN-081` | `FE-FIN-032` | `FIN-TEST-1.9` J.3 | Direncanakan |
| `FR-FIN-169` | `FIN-DEC-130` | `FIN-DES-090` | `FIN-API-1.6` F.2 | `BE-FIN-082` | `FE-FIN-032` | `FIN-TEST-1.9` J.3 | Direncanakan |
| `FR-FIN-170`..`172` | `FIN-DEC-129` | `FIN-DES-089` | `FIN-STATE-1.6` F | `BE-FIN-082` | — (perilaku backend, tidak terlihat di layar) | `FIN-TEST-1.9` J.3 | Direncanakan |
| `FR-FIN-173` | `FIN-DEC-129` | `FIN-DES-089` | `erd/data-dictionary.md` R14.6 | `BE-FIN-079` (check constraint) | — | `FIN-TEST-1.9` J.3 | Direncanakan |
| `FR-FIN-179` | `FIN-DEC-140` | `FIN-DES-093` | `FIN-TEST-1.9` J.3.1 | `BE-FIN-083` ⛔ | — | `FIN-TEST-1.9` J.3.1, J.4.2 | ⛔ `FIN-OQ-081` — paritas menuntut kedua pembaca ada |
| `FR-FIN-180` | `FIN-DEC-140` | `FIN-DES-093` | — | `BE-FIN-080` | — | `FIN-TEST-1.9` J.3.8 | Direncanakan |
| `FR-FIN-183` | `FIN-DEC-140` | `FIN-DES-093` | `erd/data-dictionary.md` R14.6 | `BE-FIN-079`, `BE-FIN-081` | `FE-FIN-032` | `FIN-TEST-1.9` J.3 | Direncanakan |

**Coverage gap requirement-ke-bukti: NOL.** Keenam belas functional requirement kedua epic
seluruhnya punya task pembawa **dan** bukti verifikasinya. Dua baris sengaja tanpa task frontend
(`FR-FIN-170`..`172`, `FR-FIN-173`, `FR-FIN-179`, `FR-FIN-180`) karena keduanya perilaku backend yang
tidak terlihat di layar mana pun — itu **bukan** gap, dan dicatat eksplisit supaya tidak disangka
terlupa. Mengikuti `rules/backend/TEST_POLICY.md`, ketiadaan automated backend test **bukan** coverage
gap, dan **nol** task "write unit tests" dimunculkan.

## Dua gap pada traceability modul yang DITUTUP revisi 15

| Gap sebelumnya | Keadaan sekarang |
|---|---|
| `EPIC FIN-23` tercatat tanpa task, gerbangnya `FIN-OQ-075` | **DITUTUP.** `FIN-DEC-139` menjawabnya; `BE-FIN-074`..`078` dan `FE-FIN-030`/`031` membawanya |
| `EPIC FIN-24` tercatat tanpa task, gerbangnya `FIN-OQ-077` | **DITUTUP.** `FIN-DEC-140` menjawabnya; `BE-FIN-079`..`083` dan `FE-FIN-032` membawanya |

**Satu gap modul yang TETAP terbuka, dan ia bukan milik revisi ini:** pelunasan sewa non-pasien
tercatat sebagai kas masuk dan kejadian akuntansi — ⛔ `FIN-OQ-044`, masih tanpa keputusan, desain,
maupun task. Ia **tidak** tersentuh revisi 15 dan tetap dicatat terbuka pada traceability modul.

## Pernyataan wajib pada setiap handoff implementasi backend REV-14D dan REV-14E

QBE preflight dan kesesuaian engineering contract diselesaikan **pada waktu eksekusi** dari
`AGENTS.md` backend target beserta `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` dan
`docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` — **bukan** dari roadmap ini. Tiga model baru
(`FinTransactionProof`, `MstDirectPaymentThreshold`, `FinOpeningItemBatch`) berstatus `NEW CODE`,
sehingga pola legacy di sekitarnya **tidak** memberi wewenang menirunya. Prefix `Fin` dan `Mst` sudah
terdaftar; folder `Readers/` berada di dalam submodule yang sudah terdaftar, sehingga **nol** gerbang
`QBE-MOD-003` baru.

## Risiko REV-14D dan REV-14E yang perlu diketahui pemilik sebelum eksekusi

| # | Risiko | Siapa yang menanggung | Mitigasi yang sudah direncanakan |
|---:|---|---|---|
| 1 | **Dua endpoint berjalan berubah kontraknya.** Layar lama gagal mencatat pembayaran begitu backend naik | Petugas AR/AP — ini satu-satunya jalur pembayaran langsung | Urutan rilis dikunci: `FE-FIN-030` sebelum atau bersamaan `BE-FIN-077`/`078`, dan kedua laporan **MUST** mencatatnya |
| 2 | **`AddFinanceOpeningItemMigration` menyentuh tabel berjalan** — tiga kolom menjadi nullable bersyarat, satu index diganti filternya | Data piutang yang sudah ada | `BE-FIN-079` wajib `NOT VALID`/`VALIDATE`, `CREATE INDEX CONCURRENTLY`, pemeriksaan data sebelum-sesudah, dan menyatakan urutan DDL yang dipakai. Penerapan tetap milik Yasmin |
| 3 | **Bukti pembayaran dapat dilihat staf yang bukan pemilik transaksinya** | Privasi pihak ketiga pada berkas bukti | Diterima sadar (`FIN-DEC-139`). Jalur unduh bergerbang hak akses, setiap unduhan tercatat, isi berkas tidak masuk logger. **MUST** disampaikan saat serah terima, dan pemberian `Read` **SHOULD** dibatasi |
| 4 | **Jalur XLSX jarang teruji** karena staf hampir pasti memakai satu format saja | Ketepatan data migrasi | Kasus uji paritas `J.3.1` dan `J.4.2` memaksa kedua jalur diuji dengan isi yang sama, termasuk kesamaan **nomor baris** pada galatnya |
| 5 | **Tiga nilai konfigurasi masih kosong** (`FIN-OQ-074`, `076`, `082`) | Petugas yang mencoba memakai fitur sebelum nilainya diisi | Fail-closed disengaja: tanpa ambang pembayaran ditolak, tanpa batas ukuran unggah ditolak `503` beserta arahan ke administrator. **Bukan** kelalaian |
| 6 | **`FIN-OQ-081` belum dijawab** sehingga `BE-FIN-083` terblokir | Penyiap cutover yang ingin memakai XLSX | Jalur CSV berjalan **ujung ke ujung** tanpa paket apa pun. Hanya satu task yang menunggu, bukan seluruh gelombang |
| 7 | **Penempatan menu ketiga layar baru belum diputuskan** (`FIN-OQ-079`) | Petugas yang harus menemukan layarnya | Layar tetap dibangun beserta route-nya; laporan task **MUST** menyebut cara mencapainya selama menu tertahan |
