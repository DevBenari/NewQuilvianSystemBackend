# Finance Management — Roadmap Backend

## 1. Identitas

```yaml
roadmap_id: FIN-ROADMAP-BE-001
parent_roadmap: FIN-ROADMAP-001 revisi 10
roadmap_revision: 10
roadmap_status: ACTIVE
blueprint_id: FIN-BP-001
blueprint_revision: 10
blueprint_status: approved
backend_commit_sha: 7811c048
backend_commit_sha_previous: cba60cb0
backend_commit_sha_baseline: 09101d0581695e20345a9efa8af3fce7c38b1ae4
backend_branch: Yasmina
contracts:
  FIN-API-1.2: locked 2026-09-26 (revisi 5; 1.1 revisi 4 tetap berlaku untuk bagian yang tidak diganti)
  FIN-PERM-1.3: locked 2026-09-28 (revisi 6; FIN-CQ-08 pemetaan payung ke granular)
  FIN-STATE-1.3: locked 2026-09-26 (revisi 5)
  FIN-VAL-1.5: approved 2026-09-29 (revisi 9 — FIN-VAL-144..146)
  FIN-INTEGRATION-1.6: approved 2026-09-29 (revisi 9 — pemicu RELEASE berpasangan)
  FIN-TEST-1.6: approved 2026-09-29 (revisi 9 — Bagian F)
  FIN-MVP-1.5: locked 2026-09-28 (revisi 6)
roadmap_revision_10_note: >
  Revisi 10 (30 September 2026) MENAMBAHKAN task BE-FIN-048 (pengetatan validasi pesan saldo subledger
  pada FinanceAccountingOutboxService sesuai ACC-DEC-109 dan ACC-DEC-110) dan BE-FIN-049 (layanan
  kalkulasi snapshot saldo subledger bulanan untuk 4 control account: Kas Kasir, Kas Kecil, Piutang,
  dan Utang Supplier sesuai ACC-DEC-108) menyusul surat susulan Accounting evidence/15 dan
  keputusan resmi FIN-DEC-090 s/d FIN-DEC-093.
roadmap_revision_9_note: >
  Revisi 9 (30 September 2026) MENANDAI SELESAI (✅) seluruh task backend BE-FIN-028 s/d BE-FIN-047
  (kecuali BE-FIN-042 🟡 yang menunggu platform registry FIN-OQ-039) menyusul keberhasilan eksekusi
  skrip migrasi database fisik (BE-FIN-041, BE-FIN-038, BE-FIN-043 via
  be-fin-041-038-043-schema-migration-dbeaver.sql) dan konfirmasi berhasilnya dotnet build (PASS 0 error)
  oleh pengguna. Implementasi EPIC FIN-12 (worker pengiriman otomatis outbox ke Accounting) DITUNDA /
  TIDAK DIBUAT atas keputusan eksplisit pemilik modul (sistem belum membutuhkan pengiriman outbox otomatis
  ke Accounting saat ini; seluruh kejadian outbox aman tersimpan di FinAccountingEventOutbox).
roadmap_revision_8_note: >
  Revisi 8 (29 September 2026) MENCABUT TANDA BLOCKER ⛔ PADA BE-FIN-047 menyusul disahkannya
  FIN-DES-064 dan FIN-DES-065 via FIN-DEC-080 dan FIN-DEC-081 (Amendment Pass). Pemicu kejadian
  PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT resmi ditetapkan: mutasi BilDepositMovement bertipe RELEASE
  yang berpasangan dengan REVERSAL pada SettlementId yang sama. Pemicu kode pengembalian kas dipersempit
  murni ke BilRefundCase EXECUTED (FIN-DEC-041 dikoreksi). Task BE-FIN-047 kini SIAP DIKERJAKAN
  pada gelombang R6-4 setelah BE-FIN-046 dan BE-FIN-025. Seluruh acceptance criteria dan DoD
  diturunkan dari FIN-TEST-1.6 §F dan FIN-VAL-1.5. Grafik urutan dependency dan tabel gelombang
  diperbarui.
roadmap_revision_7_note: >
  Revisi 7 (29 September 2026) MENURUNKAN desain AMENDMENT REVISI 6 dan 7 blueprint
  (FIN-DES-051..060, approved 28 September 2026) yang belum punya task, dan MEMPERBAIKI isi tiga
  task lama yang acceptance criteria-nya bertentangan dengan desain approved itu. TIDAK ada task
  yang dinomori ulang atau dihapus.
  TIGA TASK LAMA DIPERBAIKI ISINYA:
  (1) BE-FIN-023 — acceptance criteria "selisih kas -30.000 tersimpan" DICABUT: kode
      SELISIH-KAS-SHIFT bernilai bertanda sudah tidak ada lagi (FIN-DES-051, FIN-DES-058
      pelurusan 2). Diganti daftar tertutup kode penanda yang boleh bernilai nol (FIN-VAL-138)
      dan penghilangan properti Components dari PayloadJson (FIN-VAL-139) — keduanya berkas yang
      sama, FinanceAccountingOutboxService, sehingga digabung di sini alih-alih dipecah menjadi
      task yang menyunting method yang sama.
  (2) BE-FIN-025 — tiga acceptance criteria diperbaiki: satu kode selisih kas menjadi dua
      (SELISIH-KAS-KURANG/LEBIH) dengan kunci BilCashierShift.Id dan SourceVersion dipatok
      (FIN-DES-053); refund SETTLEMENT dari "nol kejadian" menjadi MASUK jalur yang sudah ada
      (FIN-DES-056); REFERRED_OUTPATIENT_ADMIN menjadi baris intake ERROR (FIN-VAL-141).
  (3) BE-FIN-026 — cakupan diperluas: selain baris warisan HELD_FOR_FINALIZATION, ikut menghitung
      dan melaporkan baris outbox lama bernama pendek AR_*/AP_* (FIN-DES-058).
  BE-FIN-024 DIPERIKSA dan TIDAK perlu diperbaiki — ketiga kodenya (PENERIMAAN-UANG-MUKA,
  PENERIMAAN-KASIR, PEMBALIKAN-PENERIMAAN-UANG-MUKA) tidak ikut dinamai ulang.
  TIGA TASK BARU:
  (4) BE-FIN-045 — penanda shift kasir tertutup dan pembaliknya (FIN-DES-054, FIN-DES-059).
      Kodenya boleh ditulis sekarang; hanya WORKER pengirimannya digerbang FIN-OQ-035, mengikuti
      pola FIN-DEC-056 yang sudah dipakai seluruh rumpun Accounting Integration.
  (5) BE-FIN-046 — pembalikan tender top-up deposit dikonsumsi menjadi
      PEMBALIKAN-PENERIMAAN-UANG-MUKA, beserta pendeteksi FIN-VAL-142. Pemicunya SUDAH ADA sejak
      Billing menutup FIN-OQ-034 (BKC-DEC-128..131, BillingSettlementService
      HandleDepositTopUpReversalAsync menulis mutasi Reversal).
  (6) BE-FIN-047 ⛔ — PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT. TERBLOKIR bukan oleh pihak luar,
      melainkan oleh desain sendiri yang stale: FIN-DES-057 menetapkan pemicunya mutasi REVERSAL
      atas ALLOCATION, sedangkan solusi Billing yang disahkan FIN-DEC-077 memakai mutasi RELEASE.
      Akibatnya satu MovementType RELEASE kini membawa DUA lawan jurnal berbeda dan cara Finance
      membedakannya belum digambar. Perlu amendment /design-business-module lebih dulu.
  DAMPAK FRONTEND: nol task baru, tetapi FE-FIN-007 diperbaiki isinya pada roadmap frontend revisi 7
  (tanda ⛔ dicabut, katalog kode final, dua sebab baris intake ERROR) dan menerima prasyarat baru
  BE-FIN-025.
  SHA BERGERAK cba60cb0 -> 7811c048 (dua commit: 0a09e18a BE-FIN-041, 7811c048 BE-FIN-035/036
  beserta seluruh artefak blueprint revisi 6/7 dan perbaikan Billing BE-BKC-079). Diperiksa:
  implementasi BE-FIN-036 sudah memakai nama final PEMAKAIAN-KREDIT-RETUR-PEMBELIAN
  (FIN-DEC-066), sehingga TIDAK ada drift nama kode yang perlu task pembetulan tambahan.
roadmap_revision_6_note: >
  Revisi 6 (28 September 2026) menambahkan tiga task backend:
  (1) BE-FIN-042 — Penyelarasan nama resource pada 6 controller legacy ke nama kanonikal Finance*,
      pendaftaran dan ekspansi seeder peran payung Finance.AP & Finance.AR, dan skrip SQL idempotent
      migrasi data peran SysRolePermissions (menutup FIN-CQ-08, FIN-CAP-043, FIN-OQ-036). Bebas,
      siap dijalankan segera.
  (2) BE-FIN-043 — Penambahan kolom PPNAmount pada FinSupplierReturn, check constraint PPNAmount >= 0,
      dan kalkulasi deposit retur saat CONFIRMED membawa PPN (FIN-DES-055, AddPPNAmountToFinSupplierReturn).
  (3) BE-FIN-044 — Penyelarasan nama konstanta EventTypeCode pada 4 titik pemanggil service lama
      mengikuti katalog resmi ratifikasi Accounting, dan penghapusan alias lama (FIN-DES-058). Bebas.
roadmap_revision_3_note: >
  Revisi 3 (25 September 2026) MENAMBAHKAN empat belas task BE-FIN-027..040 untuk AMENDMENT
  REVISI 4 blueprint (FIN-DES-037..044, approved dan kontraknya locked hari yang sama) —
  EPIC FIN-15 Purchasing/AP, FIN-16 AR Invoice Agregat, FIN-17 Potongan AR. TIDAK ada task lama
  yang dinomori ulang, diubah outcome-nya, atau diturunkan statusnya.
  EMPAT TEMUAN dari pembacaan source pada 96bf9746 yang mengubah rencana — lihat bagian 4
  "REV-4": (1) batas Rp 50.000.000 tepat berbeda antara kode dan FIN-DEC-052, sehingga
  BE-FIN-028 mengubah perilaku pembayaran yang sudah berjalan pada satu nilai itu;
  (2) FinanceSupplierPayableService sudah menulis AP_CREATED sebesar OriginalAmount penuh,
  sehingga BE-FIN-034 MUST mengirim nilai pokok saja agar PPN tidak terkredit dua kali;
  (3) jalur pelunasan piutang menulis AR_PAYMENT (kas masuk), sehingga potongan AR tidak boleh
  lewat jalur itu — awalnya BE-FIN-040 ⛔ FIN-OQ-024; (4) sudah ada GET api/finance/payable/aging
  atas FinSupplierPayable, sehingga endpoint aging Purchasing dicabut FIN-OQ-025.
  Dua task ⛔ oleh keputusan yang belum ada saat itu: BE-FIN-036 (FIN-OQ-023), BE-FIN-040
  (FIN-OQ-024) — lihat roadmap_revision_4_note untuk keadaan sesudah closure pass. FIN-OQ-020
  TIDAK menahan task mana pun (FIN-DEC-056).
roadmap_revision_5_note: >
  Revisi 5 (26 September 2026) menurunkan AMENDMENT REVISI 5 blueprint (FIN-DES-045..050,
  approved dan kontraknya locked 26 September 2026). BE-FIN-036 DIBUKA — skemanya kini digambar.
  Enam task diperbarui ke bentuk revisi 5 (030, 031, 035, 036, 038, 040); SATU task baru
  BE-FIN-041 (kolom DepositAppliedAmount + constraint pada FinPayment yang sudah berjalan).
  Nol task REV-4 yang ⛔. Task lama tidak dinomori ulang.
roadmap_revision_4_note: >
  Revisi 4 (25 September 2026, sesudah /grill-me closure pass) MEMBUKA BE-FIN-040 dan FE-FIN-013
  (semula ⛔ FIN-OQ-024): kode kejadian POTONGAN-PIUTANG-NON-TUNAI sudah diusulkan (FIN-DEC-058),
  sehingga service dapat menulis outbox seperti BE-FIN-034 menulis PPN-MASUKAN-PEMBELIAN —
  hanya worker pengirimannya yang menunggu Rizki (FIN-OQ-026, pola FIN-DEC-056). BE-FIN-036 dan
  FE-FIN-010 TETAP ⛔: FIN-OQ-023 tertutup sisi keputusan bisnis (FIN-DEC-057), tetapi
  konsekuensi skemanya (FinPaymentAllocation/FinSupplierReturnDepositUsage) belum digambar.
  Task lama TIDAK dinomori ulang.
roadmap_revision_2_note: >
  Revisi 2 (25 September 2026) MENAMBAHKAN lima task BE-FIN-022..026 untuk AMENDMENT REVISI 3
  blueprint (FIN-DES-029..036, approved). TIDAK ada task lama yang dinomori ulang, diubah
  outcome-nya, atau dihapus. Grafik urutan dependency dirapikan menjadi pohon teks sesuai
  rules/rule-output/grafik-dependency-roadmap.md pada kesempatan pertama berkas ini disentuh.
  SATU-SATUNYA blocker task baru: FIN-OQ-017 (ratifikasi owner Accounting atas tujuh kode
  kejadian). Empat dari lima task baru berstatus ⛔ karenanya; BE-FIN-022 bebas dari blocker itu.
```

Payung roadmap ada di `00-delivery-roadmap.md`: identitas lengkap, penguncian kontrak,
gelombang, traceability, coverage gap, dan risiko. Berkas ini **hanya** berisi task backend.
Pasangan frontend-nya ada di `02-frontend-roadmap.md`.

**Berkas ini tidak menyatakan satu pun pekerjaan selesai.**

## 2. Urutan eksekusi

**Arti tanda status pada dokumen ini.**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

Task bergaris bawah `⛔` MUST NOT dimulai.

## Grafik Urutan Dependency

Empat puluh tujuh task (`BE-FIN-001`..`047`, tanpa nomor yang terlewat) melewati batas satu layar,
sehingga grafiknya dipecah: satu grafik ringkasan antar-gelombang, lalu satu grafik per rumpun. Arah garis selalu **prasyarat di kiri, yang
menunggu di kanan**.

### Ringkasan antar-gelombang

```text
MVP-0 ✅ ─> MVP-1 ✅ ─┬─> MVP-5 ✅
                      │
                      ├─> MVP-4 ✅
                      │
                      ├─> MVP-2 ✅ ─> MVP-3 ✅
                      │
                      └─> POST-MVP 🟡

{FIN-OQ-017 ✅} ─> REV-3 (seluruh task BE-FIN-023..026 telah ✅ selesai; BE-FIN-022 🟡 menunggu eksekusi migration)

POST-MVP ─> REV-4 (seluruh task BE-FIN-027..041 telah ✅ selesai — build PASS & migrasi DBeaver aktif)

POST-MVP ─> REV-6/8 (BE-FIN-042 🟡; BE-FIN-043..047 telah ✅ selesai — build PASS & migrasi DBeaver aktif)
```

### Diagram Alur Ketergantungan (Mermaid)

```mermaid
graph LR
  subgraph "Prasyarat Controller Legacy (Selesai)"
    BE_FIN_009["BE-FIN-009 ✅<br/>Intake & Piutang"]
    BE_FIN_012["BE-FIN-012 ✅<br/>Kejadian Akuntansi"]
    BE_FIN_018["BE-FIN-018 ✅<br/>Alokasi Penerimaan"]
    BE_FIN_019["BE-FIN-019 ✅<br/>Utang Supplier"]
    BE_FIN_020["BE-FIN-020 ✅<br/>Pembayaran Keluar"]
  end

  subgraph "REV-6/8: Penyelarasan Hak Akses (FIN-CQ-08)"
    BE_FIN_042["BE-FIN-042 🟡<br/>Penyelarasan 6 Controller<br/>+ Seeder Payung AP/AR<br/>+ Skrip SQL Migrasi Peran"]
  end

  subgraph "REV-6/8: Retur & PPN"
    BE_FIN_035["BE-FIN-035 ✅<br/>Retur Pembelian"]
    BE_FIN_043["BE-FIN-043 ✅<br/>PPNAmount FinSupplierReturn<br/>+ Deposit Termasuk PPN"]
  end

  subgraph "REV-6/8: Katalog Kejadian Akuntansi"
    BE_FIN_008["BE-FIN-008 ✅<br/>Layanan Piutang"]
    BE_FIN_011["BE-FIN-011 ✅<br/>Outbox Akuntansi"]
    BE_FIN_044["BE-FIN-044 ✅<br/>Penyelarasan 4 Titik EventTypeCode<br/>ke Katalog Resmi"]
  end

  BE_FIN_009 --> BE_FIN_042
  BE_FIN_012 --> BE_FIN_042
  BE_FIN_018 --> BE_FIN_042
  BE_FIN_019 --> BE_FIN_042
  BE_FIN_020 --> BE_FIN_042

  BE_FIN_035 --> BE_FIN_043

  BE_FIN_008 --> BE_FIN_044
  BE_FIN_011 --> BE_FIN_044
  BE_FIN_019 --> BE_FIN_044
  BE_FIN_020 --> BE_FIN_044

  subgraph "REV-3/7/8: Penanda Shift dan Pembalikan Deposit"
    BE_FIN_023["BE-FIN-023 ✅<br/>Aturan Nilai & Bentuk Pesan"]
    BE_FIN_025["BE-FIN-025 ✅<br/>Intake Deposit, Kelebihan Bayar,<br/>Selisih Kas"]
    BE_FIN_045["BE-FIN-045 ✅<br/>Penanda Shift Tertutup<br/>+ Pembaliknya"]
    BE_FIN_046["BE-FIN-046 ✅<br/>Pembalikan Tender Top-Up Deposit"]
    BE_FIN_047["BE-FIN-047 ✅<br/>Pembalikan Pemakaian Uang Muka<br/>(Mutasi RELEASE Berpasangan)"]
  end

  BE_FIN_023 --> BE_FIN_045
  BE_FIN_025 --> BE_FIN_045
  BE_FIN_025 --> BE_FIN_046
  BE_FIN_046 --> BE_FIN_047
```

`REV-3` adalah kelompok task AMENDMENT REVISI 3 (`BE-FIN-022`..`026`). Ia **tidak diberi nomor
gelombang** karena `EPIC FIN-14` berstatus `OPEN DECISION` pada `04-prd-to-mvp.md`.

`REV-4` adalah kelompok task AMENDMENT REVISI 4 (`EPIC FIN-15`/`16`/`17`), seluruhnya gelombang
`POST-MVP` pada `04-prd-to-mvp.md` bagian 20.1. Berbeda dari `REV-3`, ketiga epic-nya **bukan**
`OPEN DECISION`, sehingga task-nya boleh dijadwalkan.

`REV-6/8` adalah kelompok task AMENDMENT REVISI 6, 7, 8, dan 9 (`BE-FIN-042`..`047`), menangani
penyelarasan hak akses `FIN-CQ-08` / `FIN-PERM-1.3`, PPN Retur Pembelian `FIN-DES-055`,
penyelarasan katalog kejadian Akuntansi `FIN-DES-058`, penanda shift kasir tertutup `FIN-DES-054`/
`059`, dan rumpun pembalikan mutasi deposit `FIN-DES-064`/`065`. Desainnya seluruhnya `approved`;
`BE-FIN-042`..`047` seluruhnya siap dieksekusi — `BE-FIN-047` tidak lagi terblokir setelah
`FIN-DES-064` dan `FIN-DES-065` disahkan via `FIN-DEC-080` dan `FIN-DEC-081`.

### REV-6/8 — Penyelarasan Hak Akses (FIN-CQ-08), PPN Retur, dan Katalog Akuntansi

```text
BE-FIN-009 ✅ ─┬─> BE-FIN-042 🟡 (Penyelarasan 6 Controller Legacy, Seeder Payung AP/AR, Skrip SQL Peran)
BE-FIN-012 ✅ ─┤
BE-FIN-018 ✅ ─┤
BE-FIN-019 ✅ ─┤
BE-FIN-020 ✅ ─┘

BE-FIN-035 ✅ ───> BE-FIN-043 ✅ (PPNAmount pada FinSupplierReturn & Deposit Retur)

BE-FIN-008 ✅ ─┬─> BE-FIN-044 ✅ (Penyelarasan 4 Titik EventTypeCode ke Katalog Resmi)
BE-FIN-011 ✅ ─┤
BE-FIN-019 ✅ ─┤
BE-FIN-020 ✅ ─┘

BE-FIN-023 ✅ ─┬─> BE-FIN-045 ✅ (Penanda Shift Kasir Tertutup & Pembaliknya)
BE-FIN-025 ✅ ─┘

BE-FIN-025 ✅ ───> BE-FIN-046 ✅ (Pembalikan Tender Top-Up Deposit) ───> BE-FIN-047 ✅ (Pembalikan Pemakaian Uang Muka)
```

Empat blok di atas menggambarkan urutan eksekusi kelompok `REV-6/8`. `BE-FIN-023` dan `BE-FIN-025`
adalah task rumpun `REV-3` yang digambar di sini sebagai **pangkal cabang**, bukan node kedua.
`BE-FIN-047` kini dijadwalkan pada gelombang `R6-4` setelah `BE-FIN-046`.

### REV-4 — Purchasing/AP (`EPIC FIN-15`)

```text
BE-FIN-027 ✅ ─> BE-FIN-029 ✅ ─> BE-FIN-030 ✅ ─> BE-FIN-031 ✅ ─┬─> BE-FIN-032 ✅
                                                      │
BE-FIN-028 ✅ ───────────────────────────────────────────┤
                                                      │
                                                      └─> BE-FIN-033 ✅ ─> BE-FIN-034 ✅ ─> BE-FIN-035 ✅ ─> BE-FIN-037 ✅

BE-FIN-041 ✅ ─> BE-FIN-036 ✅ <─ BE-FIN-035 ✅
```

`BE-FIN-028` menjadi prasyarat `BE-FIN-032` dan `BE-FIN-034` (keduanya memanggil resolver),
bukan prasyarat `BE-FIN-033`; panahnya digabung pada satu titik supaya grafik muat satu layar.
`BE-FIN-028` sendiri tidak menunggu apa pun dan boleh dikerjakan paling awal. **Status 30 September
2026:** source selesai, `dotnet build` PASS 0 error (dikonfirmasi pengguna 30 September 2026),
seluruh rantai dependency terselesaikan.

### REV-4 — AR Invoice Agregat dan Potongan AR (`EPIC FIN-16`, `FIN-17`)

```text
BE-FIN-038 ✅ ─┬─> BE-FIN-039 ✅
            │
            └─> BE-FIN-040 ✅
```

Rumpun ini **tidak bergantung** pada rumpun Purchasing sama sekali dan boleh berjalan paralel.
`BE-FIN-040` tidak lagi ⛔ — `FIN-DEC-058` (25 September 2026) mengusulkan kode kejadiannya;
hanya worker pengirimannya yang menunggu Rizki (`FIN-OQ-026`), pola `FIN-DEC-056`.

### MVP-0 dan MVP-1 — fondasi sampai buku piutang

```text
BE-FIN-001 ✅ ─> BE-FIN-002 ✅ ─> BE-FIN-003 ✅ ─> BE-FIN-004 ✅ ─> BE-FIN-005 ✅
  ─> BE-FIN-006 ✅ ─> BE-FIN-007 ✅ ─> BE-FIN-008 ✅ ─> BE-FIN-009 ✅
```

Rantai lurus tanpa percabangan; baris kedua adalah sambungan baris pertama.

### MVP-5, MVP-4, MVP-2, MVP-3, dan POST-MVP

```text
BE-FIN-007 ✅ ─> BE-FIN-010 ✅ ─> BE-FIN-011 ✅ ─> BE-FIN-012 ✅

BE-FIN-009 ✅ ─┬─> BE-FIN-013 ✅ ─> BE-FIN-014 ✅ ─> BE-FIN-015 ✅
               │
               ├─> BE-FIN-016 ✅ ─> BE-FIN-017 ✅ ─> BE-FIN-018 ✅
               │
               └─> BE-FIN-019 ✅

{FIN-OQ-010 ⛔} ─> BE-FIN-020 🟡 ─> BE-FIN-021 🟡 <─ {BE-MDF-014 ⛔}
```

`BE-FIN-007` dan `BE-FIN-009` masing-masing muncul sekali pada grafik MVP-0/MVP-1 di atas; di
sini keduanya digambar sebagai **pangkal cabang** untuk memperlihatkan percabangannya, bukan
sebagai node kedua. `{BE-MDF-014}` adalah task modul Medical Fee — cermin baca-saja, bukan task
roadmap ini; panahnya digambar ke kiri karena ia prasyarat yang datang dari luar.

### REV-3 — AMENDMENT REVISI 3 (task baru)

```text
BE-FIN-022 🟡 ─────────────────────────────────────┐
                                                   │
{FIN-OQ-017 ✅} ─> BE-FIN-023 ✅ ─> BE-FIN-024 ✅ ─┴─> BE-FIN-025 ✅ ─> BE-FIN-026 ✅
```

**Diperbarui 29 September 2026.** `FIN-OQ-017` dinyatakan `CLOSED` 28 September 2026 (closure pass
`/grill-me`, `00-interview-decisions.md` — `FIN-DEC-063`..`067`, surat balasan sudah terkirim ke
Rizki lewat `evidence/15`). Tanda `⛔` pada `BE-FIN-023`/`BE-FIN-024` **dicabut** — keduanya tidak
lagi menunggu ratifikasi Accounting.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| — | Otorisasi migration (`AGENTS.md` Keselamatan Database) | `BE-FIN-022` 🟡 — tidak diberi nomor gelombang karena `EPIC FIN-14` masih `OPEN DECISION` (lihat catatan di bawah). Source selesai 25 September 2026; **eksekusi migration belum dijalankan** |
| — | — (siap, `FIN-OQ-017` `CLOSED`) | `BE-FIN-023` ✅ — **Selesai 29 September 2026**, build mandiri dikonfirmasi 0 error oleh pengguna, rangkaian ditutup `BE-FIN-024` ([laporan](../task/report/backend/BE-FIN-023.md)) |
| — | `BE-FIN-023` ✅ | `BE-FIN-024` ✅ — **Selesai 29 September 2026**, build mandiri dikonfirmasi 0 error oleh pengguna, menutup rangkaian pra-final ([laporan](../task/report/backend/BE-FIN-024.md)) |
| — | `BE-FIN-022` (eksekusi migration-nya) dan `BE-FIN-024` ✅ | `BE-FIN-025` ✅ — **Selesai 29 September 2026**, build mandiri dikonfirmasi 0 error oleh pengguna ([laporan](../task/report/backend/BE-FIN-025.md)) |
| — | `BE-FIN-025` ✅ dan `BE-FIN-044` 🟡 | `BE-FIN-026` ✅ — **Selesai 29 September 2026**, audit pembacaan database fisik membuktikan 0 baris `HELD_FOR_FINALIZATION` dan 0 baris nama pendek `AR_*`/`AP_*` ([laporan](../task/report/backend/BE-FIN-026.md)) |

**Kenapa tidak ada satu pun nomor gelombang di tabel ini, walau `FIN-OQ-017` sudah `CLOSED`.**
`04-prd-to-mvp.md` bagian 20.2 (pembaruan 28 September 2026) mencatat `EPIC FIN-14` **tetap**
`OPEN DECISION` — sebabnya sudah berganti, bukan lagi `FIN-OQ-017`, melainkan `FIN-OQ-027`, `030`,
`031`, `032`, `034` (kode-kode baru hasil pemecahan/temuan source pada closure pass yang sama).
Pola ini identik dengan `BE-FIN-022`: sebuah task boleh **dikerjakan** begitu blocker spesifiknya
sendiri selesai, walau epic-nya belum masuk gelombang formal.

**Peringatan `BE-FIN-025` yang sudah SELESAI ditindaklanjuti.** Revisi roadmap 6 menandai isi task
ini basi karena Cakupan-nya masih menyebut kode tunggal `SELISIH-KAS-SHIFT` berkunci
`BilCashVarianceReview.Id`. **Revisi roadmap 7 (29 September 2026) memperbaikinya**: kode dipecah
menjadi `SELISIH-KAS-KURANG`/`SELISIH-KAS-LEBIH` berkunci `BilCashierShift.Id` dengan
`SourceVersion` dipatok (`FIN-DES-053`), refund `SETTLEMENT` dipindah dari "nol kejadian" menjadi
jalur yang sudah ada (`FIN-DES-056`), dan `REFERRED_OUTPATIENT_ADMIN` menjadi baris intake `ERROR`
(`FIN-VAL-141`). Task ini sekarang **siap dikerjakan** setelah prasyarat urutannya selesai.

**Yang masih perlu dikonfirmasi pemilik sebelum `BE-FIN-025` dimulai.** Pengguna menyatakan 29
September 2026 bahwa sebuah migration sudah dieksekusi dan berhasil, tetapi pada hari yang sama ada
**dua** berkas migration yang menunggu: `AlterFinBillingHandoffIntakeHandoffTypeCheck`
(`BE-FIN-022`, prasyarat task ini) dan `AddDepositAppliedAmountToFinPayment` (`BE-FIN-041`). Status
`BE-FIN-022` **sengaja dibiarkan** 🟡 di roadmap ini karena menaikkannya tanpa kepastian sama dengan
mengklaim penyelesaian task yang belum terbukti. Konfirmasi satu baris dari pemilik sudah cukup
untuk menutupnya.

### Gelombang eksekusi — task lama

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 (`MVP-0`) | — | `BE-FIN-001`..`004` ✅ |
| 2 (`MVP-1`) | `MVP-0` | `BE-FIN-005`..`009` ✅ |
| 3 (`MVP-5`) | `BE-FIN-007` | `BE-FIN-010`..`012` ✅ — boleh paralel sejak `MVP-1` |
| 3 (`MVP-4`) | `BE-FIN-009` | `BE-FIN-013`..`015` ✅ — boleh mendahului `MVP-2` |
| 3 (`MVP-2`) | `BE-FIN-009` | `BE-FIN-016`..`017` ✅ |
| 4 (`MVP-3`) | `BE-FIN-017` | `BE-FIN-018` ✅ |
| 3 (`POST-MVP`) | `BE-FIN-009` | `BE-FIN-019` ✅ |
| — | ⛔ `FIN-OQ-010` untuk ambang angkanya saja | `BE-FIN-020` 🟡 |
| — | ⛔ `BE-MDF-014` milik Medical Fee | `BE-FIN-021` 🟡 |

### Gelombang eksekusi — `REV-4` (`POST-MVP`)

| Urutan | Boleh mulai setelah | Task |
| ---: | --- | --- |
| R4-1 | — | `BE-FIN-027` ✅ (registry, selesai 26 September 2026), `BE-FIN-028` ✅ (resolver ambang, build PASS), `BE-FIN-038` ✅ (entity + migration AR — eksekusi migration DBeaver 30 September 2026, build PASS), `BE-FIN-041` ✅ (kolom `FinPayment` — eksekusi migration DBeaver 30 September 2026, build PASS) — keempatnya selesai |
| R4-2 | `BE-FIN-027` ✅ | `BE-FIN-029` ✅ — source selesai, `dotnet build` PASS 30 September 2026 |
| R4-2 | `BE-FIN-038` ✅ | `BE-FIN-039` ✅ — source selesai, `dotnet build` PASS 30 September 2026 |
| R4-3 | `BE-FIN-029` ✅ | `BE-FIN-030` ✅ — source selesai, `dotnet build` PASS 30 September 2026 |
| R4-4 | `BE-FIN-030` ✅ | `BE-FIN-031` ✅ — migration diaplikasikan, `dotnet build` PASS |
| R4-5 | `BE-FIN-028` ✅, `BE-FIN-031` ✅ | `BE-FIN-032` ✅ — `dotnet build` PASS |
| R4-5 | `BE-FIN-031` ✅ | `BE-FIN-033` ✅ — `dotnet build` PASS |
| R4-6 | `BE-FIN-028` ✅, `BE-FIN-033` ✅ | `BE-FIN-034` ✅ — `dotnet build` PASS |
| R4-7 | `BE-FIN-034` ✅ | `BE-FIN-035` ✅ — `dotnet build` PASS |
| R4-8 | `BE-FIN-034` ✅, `BE-FIN-035` ✅ | `BE-FIN-037` ✅ — `dotnet build` PASS |
| R4-8 | `BE-FIN-035` ✅, `BE-FIN-041` ✅ | `BE-FIN-036` ✅ — `dotnet build` PASS, migration diaplikasikan |
| R4-9 | `BE-FIN-038` ✅ | `BE-FIN-040` ✅ — source selesai, skema `FinReceiptDeduction` aktif di DB, `dotnet build` PASS |

### Gelombang eksekusi — `REV-6/8` (Penyelarasan Hak Akses FIN-CQ-08, PPN Retur, dan Katalog Akuntansi)

| Urutan | Boleh mulai setelah | Task |
| ---: | --- | --- |
| R6-1 | `BE-FIN-009`, `012`, `018`, `019`, `020` (seluruhnya ✅) | `BE-FIN-042` 🟡 — rename 6 controller legacy ke `Finance*` dan skrip SQL migrasi `SysAccessPolicy` selesai 29 September 2026; seeder ekspansi payung `Finance.AP`/`Finance.AR` menunggu platform registry `FIN-OQ-039` |
| R6-1 | `BE-FIN-011`, `019`, `020`, `008` (seluruhnya ✅) | `BE-FIN-044` ✅ — Penyelarasan nama resmi katalog outbox, `dotnet build` PASS 30 September 2026 ([laporan](../task/report/backend/BE-FIN-044.md)) |
| R6-2 | `BE-FIN-035` ✅ | `BE-FIN-043` ✅ — PPNAmount `FinSupplierReturn`, migration DBeaver dieksekusi 30 September 2026, `dotnet build` PASS |
| R6-3 | `BE-FIN-023` ✅ **dan** `BE-FIN-025` ✅ | `BE-FIN-045` ✅ — Penanda shift tertutup & pembalik, `dotnet build` PASS 30 September 2026. Worker pengirimannya digerbang `FIN-OQ-035` |
| R6-3 | `BE-FIN-025` ✅ | `BE-FIN-046` ✅ — Pembalikan tender top-up deposit, `dotnet build` PASS 30 September 2026 |
| R6-4 | `BE-FIN-046` ✅ (dan `BE-FIN-025` ✅) | `BE-FIN-047` ✅ — Pembalikan pemakaian uang muka deposit (mutasi `RELEASE` berpasangan), `dotnet build` PASS 30 September 2026 |
| — | `BE-FIN-010` ✅, `BE-FIN-044` ✅, `BE-FIN-045` ✅, `BE-FIN-047` ✅ | **(Ditunda / Belum Dibutuhkan)** `EPIC FIN-12` — worker pengiriman outbox otomatis ke Accounting ditunda atas keputusan pemilik ("karena system saya belum perlu itu"); seluruh kejadian outbox tetap aman tersimpan di `FinAccountingEventOutbox` |

**Yang TIDAK menahan satu pun task di atas:** ratifikasi `PPN-MASUKAN-PEMBELIAN` (`FIN-OQ-020`).
Sejak `FIN-DEC-056` ia hanya menahan aktivasi worker pengiriman — dan worker itu sendiri bagian
`EPIC FIN-12` yang belum dibangun (dan saat ini ditunda atas instruksi pemilik), jadi tidak ada task di roadmap ini yang menyentuhnya.

### 2.1 Catatan urutan `MVP-4`

`04-prd-to-mvp.md` bagian 20.1 mengizinkan `MVP-4` dikerjakan **lebih dahulu** bila
`BilCollectionHandoff` belum tersedia. Batas yang MUST dihormati:

`FinanceCashManagementService` menghitung kas tersedia dari **kas kasir** (`FIN-CAP-006`,
`BilCashierShift`) — bukan dari `FinReceipt`. Selama `MVP-2` belum ada, `BE-FIN-014` MUST
membaca kas kasir langsung dan MUST NOT membuat jalur sementara yang kelak dibongkar.

Bila implementer menemukan bahwa angka kas tersedia ternyata menuntut `FinReceipt`, itu temuan
yang MUST dilaporkan balik ke pass desain — bukan diselesaikan dengan improvisasi di kode.

## 3. Task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| ✅ `BE-FIN-001` | Enam submodul Finance terdaftar di registry kepemilikan modul | `FIN-DES-002` | — | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | `BillingIntake`, `Receivable`, `Collection`, `Payable`, `CashManagement`, `AccountingIntegration` | — | Enam baris tercatat; prefix `Fin` dan `Mst` tertera | Berkas registry ter-diff | Backend Owner — **prasyarat `QBE-MOD-003`, MUST sebelum file model pertama** | Registry ter-commit terpisah dari kode |
| ✅ `BE-FIN-002` | Entity dan EF configuration data induk Finance | `FIN-DES-001`, `003`, `004`, `005` | `FIN-VAL-1.0` §data induk | Pola `MstPettyCashCategoryConfiguration` | `MstBank`, `MstBankAccount`, `MstCurrency`, `MstExchangeRate` + 4 configuration | `BE-FIN-001` | Prefix `Mst`; partial unique index `WHERE "IsDelete" = false`; `HasPrecision(18,2)` | Review konfigurasi + uji unit constraint | Backend Owner | `IdentityModel` diwarisi; nol hard delete |
| ✅ `BE-FIN-003` | Migration `AddFinanceMasterData` | `FIN-DES-001` | — | — | 4 tabel baru, aditif | `BE-FIN-002` | `Up()` membuat 4 tabel; `Down()` menghapus bersih | Migration dijalankan di lingkungan pengembangan | **Otorisasi terpisah wajib** (`AGENTS.md` Keselamatan Database) | Nol tabel existing tersentuh |
| ✅ `BE-FIN-004` | API data induk Finance | `FIN-DES-001`, `FR-FIN-001`..`004` | `FIN-API-1.0`, `FIN-PERM-1.0` | `ApiResponse<T>`, `PagedResult<T>`, `[AccessPermission]` | `FinanceMasterDataService` + 3 controller | `BE-FIN-003` | Nomor rekening ganda ditolak; data induk terpakai dinonaktifkan bukan dihapus | `UAT-01`, `UAT-02` | Backend Owner | Route `api/v1/corporate/finance-management/...` hyphenated |
| ✅ `BE-FIN-005` | Pintu masuk fakta Billing yang idempoten | `FIN-DEC-005` (sisi konsumsi), `FR-FIN-010`..`013` | `FIN-INTEGRATION-1.0` §intake | `BilArHandoff` (`FIN-CAP-001`, `008`) | `FinBillingHandoffIntake` + configuration | `BE-FIN-004` | Fakta sama dua kali → satu piutang; kegagalan tersimpan dan dapat diulang; yang berhasil tidak dapat diulang | `UAT-04` | Backend Owner | `Serializable`; `Idempotency-Key` |
| ✅ `BE-FIN-006` | Entity buku piutang | `FIN-DES-010`..`013`, `FR-FIN-020`..`024` | `FIN-VAL-1.0` §piutang | — | `FinReceivable`, `FinReceivableItem`, `FinReceivableDocument`, `FinReceivableAdjustment`, `FinReceivableWriteOff` | `BE-FIN-005` | Invariant nilai piutang seimbang terpasang sebagai check constraint | Uji unit invariant | Backend Owner | Prefix `Fin`; `Guid RowVersion` pada aggregate root |
| ✅ `BE-FIN-007` | Migration `AddFinanceBillingIntake` dan `AddFinanceReceivableAndCollection` | `FIN-DES-001` | — | — | 1 + 7 tabel, aditif | `BE-FIN-006` | Urutan migration 2 lalu 3 sesuai `02-backend-architecture.md` bagian 7 | Migration dijalankan | **Otorisasi terpisah wajib** | Nol tabel existing tersentuh |
| ✅ `BE-FIN-008` | Layanan piutang: umur, koreksi, penghapusan | `FIN-DES-011`..`013`, `FR-FIN-021`..`023` | `FIN-STATE-1.0`, `FIN-VAL-1.0` | — | `FinanceReceivableService` — satu-satunya penulis `OutstandingAmount` | `BE-FIN-007` | Empat kelompok umur; berkas klaim tidak menahan pengakuan piutang | `UAT-03` | Backend Owner | `Serializable`; satu penulis saja |
| ✅ `BE-FIN-009` | API intake dan piutang | `FR-FIN-010`..`024` | `FIN-API-1.0`, `FIN-PERM-1.0` | `[AccessPermission]` | `FinanceBillingIntakeController`, `FinanceReceivablesController` | `BE-FIN-008` | Daftar, rincian, umur piutang, penelusuran ke tagihan asal | `UAT-03`, `UAT-04` | Backend Owner | Pembungkus `ApiResponse<T>` |
| `BE-FIN-010` ✅ | Kotak keluar kejadian Accounting | `FIN-DEC-004`, `FIN-DES-017`, `FR-FIN-070`..`075` | `FIN-INTEGRATION-1.0` §outbox, `ACC-XMOD-0.2` | — | `FinAccountingEventOutbox`, `FinAccountingEventAttempt` + migration `AddFinanceAccountingOutbox` | `BE-FIN-007` | Fakta dan kejadian tersimpan atau batal bersama; satu fakta satu kejadian | `UAT-07`, `UAT-17`, `UAT-18` | Backend Owner; **migration butuh otorisasi terpisah** | Data pasien tidak ikut ke muatan kejadian |
| `BE-FIN-011` ✅ | Penulisan outbox ikut transaksi pemanggil | `FIN-DES-017` | `FIN-INTEGRATION-1.0` | — | `FinanceAccountingOutboxService` | `BE-FIN-010` | Service **tidak** membuka transaksi sendiri; kejadian tertahan tidak terkirim | Uji integrasi rollback, `UAT-19` | Backend Owner | Koreksi memakai versi baru, bukan menimpa |
| `BE-FIN-012` ✅ | Pantauan kejadian (baca saja) | `FR-FIN-074` | `FIN-API-1.0`, `FIN-PERM-1.0` | — | `FinanceAccountingEventsController` | `BE-FIN-011` | Kejadian tertahan dan antreannya terlihat | Uji integrasi | Backend Owner | **Tanpa** endpoint pengirim — itu `EPIC FIN-12` |
| `BE-FIN-013` ✅ | Entity kas dan setoran | `FIN-DES-018`..`020`, `FR-FIN-060`..`065` | `FIN-VAL-1.0` §kas | `BilCashierShift` (`FIN-CAP-006`) | `FinBankDeposit`, `FinDailyCashSnapshot` + migration `AddFinanceCashManagement` | `BE-FIN-009` | Kas kecil tidak memengaruhi kas kasir | Uji unit | Backend Owner; **migration butuh otorisasi terpisah** | Aditif |
| ✅ `BE-FIN-014` | Perhitungan kas tersedia dan penutupan harian | `FR-FIN-060`..`065` | `FIN-STATE-1.0` | `BilCashierShift` | `FinanceCashManagementService` | `BE-FIN-013` | Setoran melebihi kas ditolak; setoran sebagian diterima; saldo dihitung saat posting; angka tertutup dibekukan | `UAT-13`..`UAT-16` | Backend Owner — lihat bagian 2.1 | `Serializable` |
| ✅ `BE-FIN-015` | API setoran dan kas harian | `FR-FIN-060`..`065` | `FIN-API-1.0`, `FIN-PERM-1.0` | — | `FinanceBankDepositsController`, `FinanceDailyCashController` | `BE-FIN-014` | Penutupan hari menolak setoran belum terposting | `UAT-16` | Backend Owner | Pembungkus `ApiResponse<T>` |
| `BE-FIN-016` ✅ | Penerimaan dari tender kasir | `FIN-DEC-005`, `FR-FIN-030`..`035` | `FIN-INTEGRATION-1.0` — **permukaan `BilCollectionHandoff` dikecualikan dari penguncian** | `BilTender`, `BilSettlement` (`FIN-CAP-004`, `007`) | `FinReceipt`, `FinReceiptAllocation` | `BE-FIN-009` **dan** konfirmasi owner Billing — **keduanya terpenuhi** | Satu tender berhasil → satu penerimaan; nominal disalin apa adanya | `UAT-05`, `UAT-06`, `UAT-20` | Owner Billing sudah menjawab (`BKC-DEC-106`/`108`/`109`, 21 September 2026) — lihat `evidence/02-permintaan-kontrak-untuk-owner-billing.md` | — |
| `BE-FIN-017` ✅ | Pembagian bayar-vs-piutang | `FR-FIN-031`, `FR-FIN-035` | Idem | — | Logika pembagian di `FinanceReceiptService` | `BE-FIN-016` | Pasien lunas tidak melahirkan piutang; dobel-hitung dapat dibuktikan tidak terjadi | `UAT-05`, `UAT-20` | — | — |
| `BE-FIN-018` ✅ | Alokasi, koreksi, penghapusan piutang | `FIN-DES-014`, `FR-FIN-040`..`046` | `FIN-STATE-1.0` §piutang — **terkunci**; yang menahan hanya dependency-nya | — | Alokasi manual, maker-checker, pembalikan | `BE-FIN-017` | Pengaju tidak dapat menyetujui permohonannya sendiri; nilai piutang tidak berubah selama belum diputus; pembalikan tidak menghapus riwayat | `UAT-08`..`UAT-12` | — | Maker-checker tiga lapis untuk koreksi/penghapusan; alokasi sendiri aksi langsung (`state-transition-matrix.md` §2) — lihat [laporan](../task/report/backend/BE-FIN-018.md) |
| `BE-FIN-019` ✅ | Utang supplier | `FIN-DES-015` (bagian supplier) | `FIN-API-1.0` §payable supplier | `MstSupplier` (`FIN-CAP-014`) | `FinSupplierPayable`, `FinSupplierPayableItem`, `FinPayableAdjustment` | `BE-FIN-009` | Input manual utang dan koreksinya | Uji integrasi | — | `POST-MVP` |
| `BE-FIN-020` ✅ | Pembayaran keluar dan potongan | `FIN-DES-015`, `026`, `027`, `028` — **approved** | `FIN-API-1.0`, `FIN-VAL-1.0` §payable — **terkunci** | — | `FinPayment`, `FinPaymentAllocation`, `FinPaymentDeduction`, `NetTransferAmount` | `FIN-OQ-010` — model dan kontraknya sudah bebas | Uang keluar berbeda dari utang lunas; potongan tidak menyisakan utang | `FR-FIN-050`, `FR-FIN-051` | Finance Supervisor + Yasmin — **hanya aturan validasi angkanya** (angka rupiah persis ambang approval berjenjang) yang tertahan, bukan modelnya maupun controllernya. `FinancePaymentsController` dibangun 23 September 2026; `ResolveApprovalTier` tetap memakai nilai provisional yang sudah didokumentasikan | `POST-MVP` |
| `BE-FIN-021` 🟡 | **SEBAGIAN** — utang jasa tenaga medis | `FIN-DES-025` — **approved**, `FIN-CAP-021` | `FIN-API-1.0` §payable | — | `FinMedicalServicePayable`, `FinMedicalServicePayableItem` | `BE-FIN-020` **dan** Medical Fee `BE-MDF-014` | Model domain, EF configuration, relasi FK polimorfik, dan migration siap; intake otomatis menunggu handoff Medical Fee | `FR-FIN-075` | **Owner Medical Fee** — `MdfFinanceHandoff` belum ada (intake ditangguhkan) | `POST-MVP` |
| `BE-FIN-022` 🟡 | Empat jenis fakta Billing baru menjadi nilai `HandoffType` yang sah | `FIN-DES-029`, `FIN-DEC-040`..`044` | `FIN-STATE-1.1` §1, `FIN-VAL-1.1` | `FinBillingHandoffIntake` beserta unique index `(HandoffType, SourceHandoffKey)` yang sudah ada (`FIN-DES-008`, `009`) | Konstanta `FinBillingHandoffTypes` bertambah `DEPOSIT_MOVEMENT`, `REFUNDABLE_CREDIT`, `REFUND_CASE`, `CASH_VARIANCE_REVIEW`; **satu** migration `AlterFinBillingHandoffIntakeHandoffTypeCheck` mengubah `CK_FinBillingHandoffIntake_HandoffType` dari 4 menjadi 8 nilai | — (tabel intake sudah ada sejak `BE-FIN-005` ✅) | Baris ber-`HandoffType` baru dapat disimpan; baris ber-nilai lama tetap sah; unique index tetap menolak fakta yang sama dua kali | `dotnet build`; verifikasi proses bisnis atas check constraint; `FIN-TEST-1.1` §8a baris `FIN-DES-029` (sinkronisasi ganda ditolak unique index) | Backend Owner — **otorisasi migration WAJIB diminta terpisah** (`AGENTS.md` Keselamatan Database). **Bebas dari `FIN-OQ-017`**: nama `HandoffType` adalah keputusan internal Finance, bukan nama kode kejadian yang perlu ratifikasi Accounting | Nol tabel dan nol kolom baru; `Down()` mengembalikan constraint ke 4 nilai dan **hanya aman** bila belum ada baris memakai nilai baru |
| `BE-FIN-023` ✅ | Kotak keluar membawa rincian saldo subledger, dan aturan nilai beserta bentuk pesannya diluruskan sekali untuk seluruh kode | `FIN-DES-031`, `032`, `033`, **`054`** (koreksi kedua), **`058`** (pelurusan 1 dan 2); `FIN-DEC-035`, `043`, **`064`** | `FIN-INTEGRATION-1.5` §5.2, §5.6, §5.10; `FIN-VAL-1.4` `FIN-VAL-079`..`084`, **`138`**, **`139`** | `FinanceAccountingOutboxService` yang sudah ada (`BE-FIN-011` ✅); **nol perubahan skema** (`Amount` sudah `HasPrecision(18,2)` tanpa check constraint nilai) | `AccountingOutboxEventRequest` bertambah `SubledgerBalance` dan **kehilangan** `RequiresFinalization`; `BuildPayloadJson` menyertakan objek saldo bila terisi **dan tidak lagi menyusun properti `Components` ketika tidak ada komponen**; `ValidateRequest` memakai **daftar tertutup kode penanda** yang boleh bernilai nol (`PENUTUPAN-SHIFT-KASIR`, `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR`, `SALDO-SUBLEDGER`) — bukan pemeriksaan `Amount == 0` yang longgar | — (`FIN-OQ-017` `CLOSED` 28 September 2026) | Saldo subledger `0` tersimpan; kode penanda bernilai `0` tersimpan; kejadian **transaksi** bernilai nol ditolak; kejadian bernilai **negatif** ditolak — tanpa pengecualian; `SubledgerBalance` pada kejadian non-saldo ditolak `400`; periode `2026-11-30` ditolak `400`; `PayloadJson` pesan tanpa komponen **tidak memuat kunci `Components`** sama sekali, sedangkan kolom `ComponentsJson` tetap `null` | `dotnet build` (dikonfirmasi 0 error oleh pengguna 29 September 2026); `FIN-TEST-1.5` §D.2 (lima baris) dan §8a baris `FIN-VAL-079`, `081`, `082`, `084`, `FIN-DES-032`; verifikasi proses bisnis atas daftar tertutup kode penanda | Backend Owner. **Menyentuh service yang sudah berjalan** — `RequiresFinalization` dipakai `FinanceReceiptService` hari ini, sehingga `BE-FIN-024` MUST menyusul di rangkaian yang sama agar penerimaan pra-final tidak terbit sebagai `PENERIMAAN-KASIR`. Pelurusan `Components` menyentuh **sepuluh pemanggil** `StageEventAsync` yang sudah berjalan sekaligus, walau perubahannya di satu tempat | Payload tetap dibangun di dalam service, bukan diterima mentah dari pemanggil (`FR-FIN-073` tetap terkunci di level tipe); daftar kode penanda berada di **satu** tempat — **revisi roadmap 7:** acceptance criteria "selisih kas `-30.000` tersimpan" **DICABUT** karena `SELISIH-KAS-SHIFT` bernilai bertanda sudah tidak ada (`FIN-DES-051`/`058`), dan `FIN-VAL-080` tidak lagi dirujuk task ini. ✅ **Selesai 29 September 2026, build dikonfirmasi 0 error oleh pengguna, rangkaian ditutup BE-FIN-024**, [laporan](../task/report/backend/BE-FIN-023.md) |
| `BE-FIN-024` ✅ | Penerimaan sebelum tagihan final terbit segera dengan jenis kejadian yang benar, dan pembalikannya mengikuti penerimaan aslinya | `FIN-DEC-030`, `FIN-DES-033`, `034`; `FR-FIN-034` (diperbarui), `FR-FIN-076` | `FIN-INTEGRATION-1.1` §5.5; `FIN-STATE-1.1` §9; `FIN-VAL-1.1` `FIN-VAL-085` | `FinReceipt.SourceInvoiceStatus` yang **sudah ada** — nol kolom baru (`FIN-DES-034`) | `FinanceReceiptService`: pemilihan `EventTypeCode` dari `SourceInvoiceStatus`, dan pemilihan kode pembalikan dari baris penerimaan asli yang ditunjuk `ReversalOfReceiptId` | `BE-FIN-023` ✅ — dikerjakan dalam rangkaian yang sama (menutup risiko penerimaan pra-final) | Tagihan `OPEN` → `PENERIMAAN-UANG-MUKA` berstatus `PENDING`, **bukan** tertahan; tagihan `FINAL` → `PENERIMAAN-KASIR`; pembalikan penerimaan uang muka **tetap** `PEMBALIKAN-PENERIMAAN-UANG-MUKA` walaupun tagihannya sudah `FINAL` saat pembalikan | `dotnet build` (dikonfirmasi 0 error oleh pengguna 29 September 2026); `FIN-TEST-1.1` §8a baris `FIN-DEC-030` (tiga skenario), `FIN-DES-034` (dua skenario), `FIN-VAL-085` | Backend Owner. **INI SATU-SATUNYA TASK YANG MENGUBAH PERILAKU YANG SUDAH BERJALAN** — `FinanceReceiptService` memilih `EventTypeCode` penerimaan dan pembalikan berdasarkan `SourceInvoiceStatus` tanpa menahan kejadian; review diff membuktikan tidak ada jalur alokasi atau saldo lain yang disentuh | Nol baris baru berstatus `HELD_FOR_FINALIZATION` dihasilkan kode setelah task ini. ✅ **Selesai 29 September 2026, build dikonfirmasi 0 error oleh pengguna, menutup rangkaian pra-final**, [laporan](../task/report/backend/BE-FIN-024.md) |
| `BE-FIN-025` ✅ | Deposit, kelebihan bayar, dan selisih kas shift masuk ke kotak keluar dengan jenis kejadian masing-masing | `FIN-DEC-031`, `034`, `040`..`043`, **`064`**, **`067`**, **`073`**, **`074`**; `FIN-DES-035`, `036`, **`053`**, **`056`**; `FR-FIN-077`..`079` | `FIN-INTEGRATION-1.5` §2a, §5.4, §5.10; `FIN-VAL-1.4` `FIN-VAL-086`, **`140`**, **`141`** | `BilDepositMovement`, `BilRefundableCredit`, `BilRefundCase`, `BilCashVarianceReview`, `BilCashierShift` — **read-only**, sudah terdaftar di `ApplicationDbContext` (`FIN-CAP-022`..`024`) | `FinanceBillingIntakeService`: empat jalur sinkronisasi baru dengan anti-join ke `FinBillingHandoffIntake` beserta penyempitan waktu. Selisih kas memakai **dua** kode — `SELISIH-KAS-KURANG` bila `Variance < 0`, `SELISIH-KAS-LEBIH` bila `> 0` — bernilai **mutlak** (selalu positif), `SourceTransactionId` **`BilCashierShift.Id`**, `SourceVersion` **dipatok `"1"`**, `CorrelationId` shift, `CausationId` `BilCashVarianceReview.Id` baris yang **menyelesaikan** selisihnya, `AccountingDate` tanggal shift. Intake `REFUNDABLE_CREDIT`/`REFUND_CASE` diperluas menerima `SourceType` `SETTLEMENT` selain `ALLOCATION_EXCESS`; `REFERRED_OUTPATIENT_ADMIN` ditulis sebagai baris intake `ERROR` yang menyebut jenis kreditnya dan menunjuk `FIN-OQ-031` | `BE-FIN-022` (eksekusi migration-nya) **dan** `BE-FIN-024` ✅ | `ALLOCATION` → `PEMAKAIAN-UANG-MUKA-DEPOSIT` tanpa `FinReceipt` baru; `RELEASE` → dicegah keluar kas (`FIN-VAL-144`), baris intake `ERROR` menunjuk `FIN-OQ-037` (`FIN-VAL-145`); kredit `ALLOCATION_EXCESS` **atau `SETTLEMENT`** → `PENGAKUAN-KELEBIHAN-BAYAR`, dan refund tunai atas **keduanya** → `PENGEMBALIAN-UANG-MUKA`; kelebihan yang lahir dari pembayaran `PENERIMAAN-UANG-MUKA` → **nol** `PENGAKUAN-KELEBIHAN-BAYAR` (syarat ketiga `FIN-DEC-067`); refund `REFERRED_OUTPATIENT_ADMIN` → baris intake `ERROR` beserta sebabnya, **nol kejadian**; shift kurang Rp 30.000 yang disahkan → **satu** `SELISIH-KAS-KURANG` `30000.00` berkunci `BilCashierShift.Id`; shift lebih → `SELISIH-KAS-LEBIH`; pengesahan `NEEDS_FOLLOW_UP` → **nol kejadian**, lalu penyelesaiannya → **tepat satu**; percobaan kejadian selisih **kedua** untuk shift yang sama ditolak unique index (`409`), **bukan** tersimpan sebagai versi 2; `Variance` nol → **nol kejadian**; nol perubahan pada tabel `Bil*` mana pun | `dotnet build` (dikonfirmasi 0 error oleh pengguna 29 September 2026); `FIN-TEST-1.5` §D.3 (empat baris pertama), §D.6 (empat baris), dan §8a baris `FIN-DEC-040`, `041` (tiga skenario), `042`, `043` (dua skenario), `FIN-VAL-086`, aturan bisnis #9; verifikasi proses bisnis; perbandingan `RowVersion`/`UpdateDateTime` tabel `Bil*` sebelum dan sesudah | Backend Owner. Risiko utama: menulis ke tabel `Bil*` tanpa s| ✅ `BE-FIN-026` | Dua rumpun baris warisan pada kotak keluar dibereskan sebelum pengiriman diaktifkan | `FIN-DEC-030`; **`FIN-DES-058`**; `02-backend-architecture.md` bagian B.6 dan E.9; `FIN-VAL-1.4` `FIN-VAL-078` | `FIN-STATE-1.3` §9 (transisi migrasi satu kali); `FIN-TEST-1.5` §D.1 | — | **Rumpun pertama** — penghitungan baris berstatus `HELD_FOR_FINALIZATION`; bila ada, pembetulan `EventTypeCode` menjadi `PENERIMAAN-UANG-MUKA` untuk penerimaan ber-`SourceInvoiceStatus` `OPEN`, lalu status dipindah ke `PENDING`. Worker MUST melewati **dan melaporkan** baris yang belum dibetulkan. **Rumpun kedua (baru)** — penghitungan dan pelaporan baris outbox yang masih memuat lima nama pendek `AR_CREATED`/`AR_PAYMENT`/`AR_WRITEOFF`/`AP_CREATED`/`AP_PAYMENT`. Task ini **hanya menghitung dan melaporkan**; keputusan penanganannya (dibuang atau ditulis ulang sebagai `SourceVersion` baru) adalah **keputusan operasional pemilik** yang diambil di atas angka hasil pembacaan ini — `FIN-DES-058` melarang dokumen desain memutuskannya, dan task ini tidak boleh memutuskannya sendiri | Otorisasi pembacaan database diberikan pengguna 29 September 2026; `BE-FIN-025` ✅; `BE-FIN-044` 🟡 | Jumlah baris **kedua** rumpun dilaporkan apa adanya beserta angkanya; bila nol, task ditutup tanpa perubahan data; bila ada baris `HELD_FOR_FINALIZATION`, setiap baris punya jejak pembetulannya; baris bernama pendek **tetap** bernama lama dan **tetap** `PENDING` — **tidak ada** migration data yang menimpanya; laporan memuat rekomendasi penanganan, bukan eksekusinya | ✅ **SELESAI 29 September 2026**, audit pembacaan database fisik `QuilvianNewDevYasmina` membuktikan 0 baris `HELD_FOR_FINALIZATION` dan 0 baris nama pendek `AR_*`/`AP_*` (total outbox 0 baris); nol baris diubah pada database fisik; kueri SQL diagnostik/remediasi idempotent dieksekusi langsung pada DBeaver dan teruji bersih; bukti berupa angka hasil pembacaan nyata, bukan asumsi; rekomendasi penanganan diserahkan ke pemilik produk | Backend Owner — **otorisasi pembacaan database telah diberikan**. Dikonfirmasi nol baris `HELD_FOR_FINALIZATION` dan nol baris nama pendek karena worker pengiriman Finance belum pernah hidup (gerbang `G4`). Nilai `HELD_FOR_FINALIZATION` **tidak** dihapus dari check constraint; nol baris dihapus tanpa keputusan pemilik yang tercatat. [Laporan](../task/report/backend/BE-FIN-026.md) |punya jejak pembetulannya; baris bernama pendek **tetap** bernama lama dan **tetap** `PENDING` — **tidak ada** migration data yang menimpanya; laporan memuat rekomendasi penanganan, bukan eksekusinya | `FIN-TEST-1.5` §D.1 baris terakhir (baris outbox lama bernama pendek) dan §8a baris `FIN-VAL-078`; bukti berupa angka hasil pembacaan, bukan asumsi | Backend Owner — **otorisasi pembacaan database WAJIB terpisah**. Kemungkinan besar nol baris `HELD_FOR_FINALIZATION` karena **worker pengiriman Finance belum pernah hidup** (gerbang `G4`). **Revisi roadmap 7:** alasan lama "endpoint Accounting belum ada (`FIN-CAP-018`)" **dicabut** — impact scan 28 September 2026 membuktikan endpoint penerima Accounting **sudah ada** sejak `cba60cb0` (`FIN-CAP-018` dikoreksi `Missing` → `Ready to reuse`), jadi yang menahan pengiriman adalah sisi Finance, bukan sisi Accounting | Nilai `HELD_FOR_FINALIZATION` **tidak** dihapus dari check constraint; baris warisan dibetulkan datanya, bukan skemanya; nol baris dihapus tanpa keputusan pemilik yang tercatat |
| ✅ `BE-FIN-027` | Submodul `Purchasing` terdaftar eksplisit di registry kepemilikan modul | `FIN-DES-037`, `FIN-DES-002` (preseden), `02-backend-architecture.md` C.7 | — | Baris registry enam submodul yang sudah ada (`BE-FIN-001` ✅) | Satu baris `Corporate / Finance \| FinanceManagement / Purchasing / Pembelian \| BUSINESS DOMAIN / MODULE \| Fin \| ACTIVE` + satu entri riwayat perubahan | — | Baris tercatat; prefix tetap `Fin` (nol prefix baru); `Invoke-QbeConformanceCheck.ps1` `PASS` | Diff berkas registry; hasil QBE | Backend Owner — **prasyarat `QBE-MOD-003`, MUST sebelum file model Purchasing pertama** | Registry ter-diff terpisah dari kode; tidak memberi wewenang implementasi/migration — ✅ **Selesai 26 September 2026**, [laporan](../task/report/backend/BE-FIN-027.md) |
| ✅ `BE-FIN-028` | Satu resolver jenjang approval dipakai pembayaran, PO, dan Purchasing Invoice — dengan batas sesuai `FIN-DEC-052` | `FIN-DEC-050`, `052`; `FIN-DES-039` (koreksi 25 September 2026) | `FIN-VAL-1.2` `FIN-VAL-052`, `102`, `109`; `FIN-TEST-1.2` B.1 | `FinancePaymentService.ResolveApprovalTier` + `ApprovalTiers` (`FIN-CAP-035`) — **dipindah, bukan ditulis ulang** | `FinanceApprovalTierResolver` (baru) memuat `ApprovalTiers.Tier1`/`Tier2` dan `Resolve(decimal)`: `< 50.000.000 → TIER_1`, `>= 50.000.000 → TIER_2`; `FinancePaymentService` memanggilnya; komentar "provisional/FIN-OQ-010" dicabut di tiga berkas; registrasi DI | — | Rp 49.999.999 → `TIER_1`; **Rp 50.000.000 tepat → `TIER_2`**; Rp 50.000.001 → `TIER_2`; pembayaran yang sudah `SUBMITTED` tidak dihitung ulang tier-nya | `dotnet build` PASS 0 error (dikonfirmasi pengguna 30 September 2026); review diff membuktikan hanya satu nilai batas yang berubah; baris `FIN-DEC-052` pada `FIN-TEST-1.2` B.1 | Backend Owner. **MENGUBAH PERILAKU YANG SUDAH BERJALAN** pada satu nilai: pembayaran tepat Rp 50.000.000 kini butuh Manajer, bukan Supervisor. Disetujui lewat `FIN-DEC-052` | Nol perubahan skema; tidak ada salinan logika ambang kedua di mana pun — ✅ **Selesai 30 September 2026, build PASS 0 error dikonfirmasi pengguna**, [laporan](../task/report/backend/BE-FIN-028.md) |
| ✅ `BE-FIN-029` | Model dokumen Purchasing: PO, Tanda Terima Barang, Tukar Faktur, Purchasing Invoice | `FIN-DES-037`, `FIN-DEC-045`, `051` | `FIN-STATE-1.2` B.1-B.4; `erd/data-dictionary.md` C.1-C.7, C.16 | `MstSupplier` (`FIN-CAP-027`) — rujukan FK, nol kolom baru; pola status `string` + `static class` dari `FinPayment` | Tujuh entity + tujuh configuration di `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/` + `DbSet` pada `ApplicationDbContext`; check constraint status dan `ApprovalTier`; `UNIQUE (InvoiceExchangeId)` | `BE-FIN-027` ✅ | Bentuk kolom, tipe, FK, `DeleteBehavior`, unique, dan check constraint persis `data-dictionary.md` C.1-C.7; `FinGoodsReceipt.PurchaseOrderId` wajib; `FinInvoiceExchange.PurchaseOrderId`/`GoodsReceiptId` nullable `SetNull` | `dotnet build` PASS 0 error (dikonfirmasi pengguna 30 September 2026); review configuration terhadap DDL C.16 | Backend Owner | `IdentityModel` diwarisi; `Guid RowVersion` pada aggregate root; `HasPrecision(18,2)`; nol hard delete — ✅ **Selesai 30 September 2026, build PASS 0 error dikonfirmasi pengguna**, [laporan](../task/report/backend/BE-FIN-029.md) |
| ✅ `BE-FIN-030` | Model Retur Pembelian dan Deposit Retur (bentuk revisi 5), dan utang supplier tahu asal Purchasing Invoice-nya | `FIN-DES-038`, `040`, `045`, `046`; `FIN-DEC-047`, `057` | `FIN-STATE-1.3` C.1-C.2; `data-dictionary.md` C.8-C.10, C.12 **dan D.2** (`FinSupplierReturnDepositUsage` bentuk revisi 5 — **bukan** C.11) | Pola struktur `BilRefundableCredit` (arah dibalik); `FinSupplierPayable` (`FIN-CAP-026`); `FinPayment` yang sudah ada sebagai FK tujuan pemakaian | Empat entity + configuration; `FinSupplierReturnDepositUsage` ber-`PaymentId`, `Status` (`RESERVED`/`APPLIED`/`RELEASED`), `ReleasedAt`, `RowVersion`, unique index parsial `(PaymentId, SupplierReturnDepositId)`; `FinSupplierPayable.SourcePurchasingInvoiceId` (nullable, FK `SetNull`) | `BE-FIN-029` ✅ | `AvailableAmount >= 0`; `UNIQUE (SourceReturnId)`; `CK_FinSupplierReturnDepositUsage_ReleasedAt`; baris `FinSupplierPayable` lama tetap valid dengan kolom baru `NULL` | `dotnet build` PASS 0 error (dikonfirmasi pengguna 30 September 2026); review configuration terhadap DDL `data-dictionary.md` D.4(b) | Backend Owner. **Jangan membangun dari C.11** — bentuk itu digantikan revisi 5 | Jalur input manual `FinSupplierPayable` tidak tersentuh perilakunya (`FIN-DES-040`) — ✅ **Selesai 30 September 2026, build PASS 0 error dikonfirmasi pengguna**, [laporan](../task/report/backend/BE-FIN-030.md) |
| ✅ `BE-FIN-031` | Skema Purchasing/AP tersedia di database | `FIN-DES-037`, `038`, `040`, `045`; `02-backend-architecture.md` C.9 langkah 1 dan 3, D.7 baris 1 | — | — | Migration `AddPurchasingApRumpun` (11 tabel, `FinSupplierReturnDepositUsage` dengan bentuk D.2) lalu `AddSourcePurchasingInvoiceIdToSupplierPayable` (1 kolom) — dua berkas terpisah, urutan itu; `ApplicationDbContextModelSnapshot.cs` diperbarui manual untuk 11 entity + kolom `FinSupplierPayable.SourcePurchasingInvoiceId` | `BE-FIN-030` ✅ | `Up()` aditif murni; `Down()` menghapus bersih dalam urutan FK terbalik; nol tabel modul lain tersentuh | `dotnet build` PASS 0 error (dikonfirmasi pengguna 30 September 2026); migration diaplikasikan di lingkungan pengembangan | Backend Owner. **Otorisasi terpisah WAJIB** untuk membuat berkas migration **dan** untuk mengeksekusinya (`AGENTS.md` Keselamatan Database). Berkas Designer dan ModelSnapshot teruji konsisten | Dapat dijalankan tanpa downtime; nol backfill — ✅ **Selesai 30 September 2026, build PASS 0 error dikonfirmasi pengguna**, [laporan](../task/report/backend/BE-FIN-031.md) |
| ✅ `BE-FIN-032` | Purchase Order dan Tanda Terima Barang dapat dicatat dan disetujui berjenjang | `FIN-DEC-050`, `052`; `FR-FIN-081` | `FIN-API-1.1` B.1, B.2; `FIN-PERM-1.1` B.2, B.3; `FIN-STATE-1.2` B.1, B.2; `FIN-VAL-1.2` `100`..`104` | `FinanceApprovalTierResolver` (`BE-FIN-028`); pola maker-checker `FinancePaymentService` | `FinancePurchaseOrderService`, `FinanceGoodsReceiptService`, `FinancePurchaseOrdersController`, `FinanceGoodsReceiptsController`, DTO, registrasi DI; `FinanceApprovalAuthorizationService` (role Identity `Supervisor Finance`/`Manajer Finance`, `FinanceApprovalRoleSeeder`) | `BE-FIN-028`, `BE-FIN-031` ✅ | PO tanpa baris ditolak `400`; pengaju = penyetuju ditolak `422`; Supervisor menyetujui PO Rp 62.000.000 ditolak `403`; PO ber-GR tidak dapat dibatalkan; GR melebihi sisa baris ditolak; GR sebagian → PO `PARTIALLY_RECEIVED` | `dotnet build` PASS 0 error 30 September 2026; `FIN-TEST-1.2` B.1, B.2 (baris PO/GR); verifikasi proses bisnis atas source | Backend Owner. Route `api/v1/corporate/finance-management/purchasing/...`; `[AccessPermission]` persis `FIN-PERM-1.1` | `Serializable` untuk GR (approve PO cukup optimistic concurrency `RowVersion`, konsisten `FinancePaymentService.ApproveAsync`) — ✅ **Selesai 30 September 2026, build PASS 0 error dikonfirmasi pengguna (menyelesaikan gap GET / berpaging + riwayat GR pada detail PO)**, [laporan](../task/report/backend/BE-FIN-032.md) |
| ✅ `BE-FIN-033` | Tukar Faktur tercatat sebagai checkpoint dokumen, dengan estimasi jatuh tempo dari TOP supplier | `FIN-DEC-051`; `FR-FIN-082` | `FIN-API-1.1` B.3; `FIN-STATE-1.2` B.3 | `MstSupplier.PaymentTermDays` | `FinanceInvoiceExchangeService`, `FinanceInvoiceExchangesController`, DTO, registrasi DI | `BE-FIN-031` ✅ | Tukar Faktur tanpa PO/GR tersimpan; `EstimatedDueDate = ReceivedDate + PaymentTermDays` dihitung backend (nilai dari request diabaikan — `CreateInvoiceExchangeRequest` sengaja tidak punya field ini); Tukar Faktur yang sudah `LINKED_TO_INVOICE` tidak dapat dibatalkan | `dotnet build` PASS 0 error 30 September 2026; `FIN-TEST-1.2` B.2 | Backend Owner | Nol perhitungan tanggal di luar service — ✅ **Selesai 30 September 2026, build PASS 0 error dikonfirmasi pengguna (menyelesaikan gap GET / berpaging)**, [laporan](../task/report/backend/BE-FIN-033.md) |
| ✅ `BE-FIN-034` | Purchasing Invoice yang disetujui menjadi utang supplier dan menulis kejadian PPN Masukan, tanpa PPN terkredit dua kali | `FIN-DEC-045`, `046`, `053`, `056`; `FR-FIN-083`, `084`, `086`, `087` | `FIN-API-1.1` B.4, B.9; `FIN-STATE-1.2` B.4; `FIN-VAL-1.2` `105`..`109`, `122`; `FIN-INTEGRATION-1.2` §5.8 | `FinanceSupplierPayableService` (`BE-FIN-019` ✅) sebagai **satu-satunya** pembuat `FinSupplierPayable` — diperluas dua parameter opsional di akhir (`sourcePurchasingInvoiceId`, `accountingEventAmountOverride`), satu-satunya caller lama terverifikasi tidak berubah; `FinanceAccountingOutboxService` (`BE-FIN-011` ✅); `FinanceApprovalAuthorizationService` (`BE-FIN-032` ✅) | `FinancePurchasingInvoiceService`, `FinancePurchasingInvoicesController`, DTO, registrasi DI; jalur pembuatan utang dari invoice lewat `FinanceSupplierPayableService` dengan `SourcePurchasingInvoiceId` terisi (satu item sintetis `Quantity=1, UnitPrice=TotalAmount`) | `BE-FIN-028` ✅, `BE-FIN-033` ✅ | Invoice kedua dari Tukar Faktur yang sama ditolak unique index (`409`, FIN-VAL-105); total tidak seimbang ditolak `422` (FIN-VAL-107); approve dalam **satu transaksi**: utang tercipta, Tukar Faktur → `LINKED_TO_INVOICE`, outbox `PPN-MASUKAN-PEMBELIAN` sebesar `PPNAmount` berstatus `PENDING` (dilewati bila `PPNAmount = 0`). **Kejadian pengakuan utang yang ditulis `FinanceSupplierPayableService` bernilai `TotalAmount − PPNAmount`** untuk utang bersumber Purchasing Invoice — bila tetap `OriginalAmount` penuh, PPN terkredit dua kali di Utang Supplier | `dotnet build` PASS 0 error (dikonfirmasi pengguna 30 September 2026); `FIN-TEST-1.2` B.3; bukti isi outbox: dua baris untuk satu invoice ber-PPN, jumlah keduanya = `TotalAmount` | Backend Owner. Jalur input manual tetap menulis `OriginalAmount` penuh | Nol worker pengiriman dibangun (`EPIC FIN-12`); nol penulis `FinSupplierPayable` kedua — ✅ **Selesai 30 September 2026, build PASS 0 error dikonfirmasi pengguna**, [laporan](../task/report/backend/BE-FIN-034.md) |
| ✅ `BE-FIN-035` | Retur pembelian dicatat, menerbitkan Deposit Retur, dan tercatat di kotak keluar | `FIN-DEC-047`, `061`; `FR-FIN-085` (penerbitan), `FR-FIN-098` | `FIN-API-1.2` (B.5 kecuali `apply` yang dicabut); `FIN-STATE-1.2` B.5, `FIN-STATE-1.3` C.2; `FIN-VAL-1.2` `110`, `111`; `FIN-INTEGRATION-1.3` §5.9 kode 28 | `FinanceAccountingOutboxService` (`BE-FIN-011` ✅) | `FinanceSupplierReturnService` (catat, konfirmasi, batal, daftar deposit) + `FinanceSupplierReturnsController` tanpa endpoint `apply`, DTO, registrasi DI; outbox `RETUR-PEMBELIAN` saat `CONFIRMED` | `BE-FIN-034` ✅ | Retur atas invoice non-`APPROVED` ditolak; nilai retur > nilai invoice ditolak; konfirmasi retur menerbitkan deposit `AVAILABLE` **dan** satu baris outbox `RETUR-PEMBELIAN` sebesar `TotalAmount` berstatus `PENDING`, satu transaksi; deposit ber-baris `RESERVED`/`APPLIED` tidak dapat dibatalkan | `dotnet build` PASS 0 error (dikonfirmasi pengguna 30 September 2026); `FIN-TEST-1.2` B.4 (tiga baris pertama); `FIN-TEST-1.3` C.2 baris `FIN-DEC-061` | Backend Owner | Retur **tidak** mengubah `FinSupplierPayable` apa pun; efeknya ke utang lewat pembayaran (`BE-FIN-036`). Nol worker pengiriman — ✅ **Selesai 30 September 2026, build PASS 0 error dikonfirmasi pengguna**, [laporan](../task/report/backend/BE-FIN-035.md) |
| ✅ `BE-FIN-036` | Deposit Retur dapat dipakai sebagai sumber dana pembayaran supplier, dan tercatat terpisah dari kas | `FIN-DEC-047`, `057`, `061`, `066`; `FIN-DES-045`..`047`; `FR-FIN-085` (pemakaian), `FR-FIN-096`, `097` | `FIN-API-1.2` C.1, C.2; `FIN-PERM-1.2` C.1; `FIN-STATE-1.3` C.1-C.3; `FIN-VAL-1.3` C.1-C.2, `FIN-VAL-131`; `FIN-INTEGRATION-1.3` §5.9 kode 29 (`PEMAKAIAN-KREDIT-RETUR-PEMBELIAN`) | `FinancePaymentService` (`BE-FIN-020` ✅) — pola baris potongan (`FinPaymentDeduction`) ditiru persis; `FinanceSupplierReturnService` (`BE-FIN-035` ✅) sebagai satu-satunya penulis `AvailableAmount` | (a) `FinanceSupplierReturnService`: `ReserveAsync`, `ReleaseUsageAsync`, `ReleaseReservedByPaymentAsync`, `MarkAppliedByPaymentAsync`, `CancelDepositAsync` — **ikut** transaksi pemanggil; (b) `FinancePaymentService`: tambah/lepas deposit (hanya `DRAFT`, `Serializable`, kunci `FIN_RETURN_DEPOSIT_{id}` & `FIN_PAYMENT_{id}`), rumus `NetTransferAmount` dengan `DepositAppliedAmount`, pengecualian `FIN-VAL-091` dan `FIN-VAL-056`, pelepasan saat `REJECTED`/`CANCELLED`, `MarkPaidAsync`: baris → `APPLIED`, `AP_PAYMENT` = `TotalAmount − DepositAppliedAmount` (dilewati bila nol), tulis `PEMAKAIAN-KREDIT-RETUR-PEMBELIAN`; (c) `FinancePaymentsController`: `GET`/`POST`/`DELETE /payments/{id}/return-deposits`; `PaymentDetailResponse` diperluas | `BE-FIN-035` ✅, `BE-FIN-041` ✅ | Seluruh baris `FIN-TEST-1.3` C.1 dan C.2; **regresi**: pembayaran tanpa deposit menghasilkan `AP_PAYMENT` bernilai `TotalAmount`, identik dengan sebelum task ini | `dotnet build` PASS 0 error (dikonfirmasi pengguna 30 September 2026); migration `BE-FIN-041` diaplikasikan di database via DBeaver 30 September 2026 | Backend Owner. **MENGUBAH SERVICE YANG SUDAH BERJALAN** (`FinancePaymentService`) di empat titik — setiap perubahan bersyarat `DepositAppliedAmount > 0` atau aditif | Nol penulis `AvailableAmount` di luar `FinanceSupplierReturnService`; nol penulis `OutstandingAmount` baru; nol worker pengiriman — ✅ **Selesai 30 September 2026, build PASS 0 error dikonfirmasi pengguna, migrasi DBeaver aktif**, [laporan](../task/report/backend/BE-FIN-036.md) |
| ✅ `BE-FIN-037` | Empat laporan Purchasing/AP dari data yang sudah ada | `FIN-DES-044`; `FR-FIN-088` | `FIN-API-1.1` B.6 **kecuali** `/aging`; `FIN-PERM-1.1` B.5 | Entity `BE-FIN-029`/`030` — **nol tabel laporan baru** | `FinancePurchasingReportService`, `FinancePurchasingReportsController`: `/summary`, `/invoice-exchanges`, `/due-dates`, `/reconciliation` | `BE-FIN-034` ✅, `BE-FIN-035` ✅ | Rekonsiliasi menampilkan Tukar Faktur yang belum menjadi invoice; laporan jatuh tempo memakai `EstimatedDueDate`/`DueDate` dari backend; seluruhnya read-only | `dotnet build` PASS 0 error (dikonfirmasi pengguna 30 September 2026); verifikasi proses bisnis atas source | Backend Owner. **`/aging` sengaja dikeluarkan** — `GET api/finance/payable/aging` (`FinanceApController`) sudah menghitung umur `FinSupplierPayable`. **Dicabut dari kontrak** (`FIN-DEC-059`, `/grill-me` 25 September 2026) | Nol perintah pengubah pada controller laporan — ✅ **Selesai 30 September 2026, build PASS 0 error dikonfirmasi pengguna**, [laporan](../task/report/backend/BE-FIN-037.md) |
| ✅ `BE-FIN-038` | Skema AR Invoice Agregat dan Potongan AR (bentuk revisi 5) tersedia | `FIN-DES-041`, `042`, `048`, `049` | `data-dictionary.md` C.13-C.14 **dan D.3** (`FinReceiptDeduction` bentuk revisi 5 — **bukan** C.15); `FIN-STATE-1.2` B.7 | `FinReceivable.DebtorType`/`DebtorReferenceId` (`FIN-CAP-031`); `FinReceiptAllocation` yang sudah ada sebagai FK tempat potongan melekat | `FinReceivableInvoiceBatch`, `…Item` (submodul `Receivable`), `FinReceiptDeduction` (submodul `Collection`) ber-`DeductionNumber`, `ReceiptAllocationId`, `IsReversal`, `ReversalOfDeductionId` + configuration + `DbSet` + migration `AddArInvoiceBatchAndReceiptDeduction` | — (submodul `Receivable`/`Collection` sudah terdaftar) | Unique index parsial `ReceivableId` aktif; `CK_FinReceivableInvoiceBatch_DebtorType = 'PAYER'`; `CK_FinReceiptDeduction_OtherReason`, `_Reversal`; unique `DeductionNumber`; unique parsial `ReversalOfDeductionId` | `dotnet build` PASS 0 error (dikonfirmasi pengguna 30 September 2026); review configuration terhadap DDL `data-dictionary.md` C.16 (batch) dan D.4(c) (potongan); migration dieksekusi di database fisik via DBeaver 30 September 2026 | Backend Owner — **otorisasi migration WAJIB terpisah**. **Jangan membangun `FinReceiptDeduction` dari C.15** | Aditif; nol tabel modul lain tersentuh — ✅ **Selesai 30 September 2026, build PASS 0 error dikonfirmasi pengguna, migrasi DBeaver aktif**, [laporan](../task/report/backend/BE-FIN-038.md) |
| ✅ `BE-FIN-039` | Beberapa piutang satu penjamin dapat diterbitkan sebagai satu dokumen tagihan resmi | `FIN-DEC-048`, `054`; `FR-FIN-089`..`092` | `FIN-API-1.1` B.7; `FIN-PERM-1.1` B.5; `FIN-STATE-1.2` B.7; `FIN-VAL-1.2` `114`..`117` | `BillingCompanyGuarantorInvoiceDocumentService` (`FIN-CAP-030`) — **dipanggil**, tidak disalin | `FinanceReceivableInvoiceBatchService`, `FinanceReceivableInvoiceBatchesController`, DTO, registrasi DI | `BE-FIN-038` ✅ | Campur dua penjamin ditolak `400`; piutang di batch aktif lain ditolak `409`; batch kosong tidak dapat terbit; dokumen batch memuat rincian per invoice dari layanan Billing; status `PARTIALLY_PAID`/`PAID` mengikuti status `FinReceivable` anggota | `dotnet build` PASS 0 error (dikonfirmasi pengguna 30 September 2026); `FIN-TEST-1.2` B.5; skema batch aktif di DB | Backend Owner. **Batas yang MUST dihormati:** service ini MUST NOT menulis `FinReceivable.OutstandingAmount` (DoD #13) — terverifikasi lewat review kode (nol penulisan) | Nol PPN/Faktur Pajak (`FIN-DEC-054`) — ✅ **Selesai 30 September 2026, build PASS 0 error dikonfirmasi pengguna**, [laporan](../task/report/backend/BE-FIN-039.md) |
| ✅ `BE-FIN-040` | Potongan PPh 23 dan biaya admin bank dicatat bersama alokasinya, melunasi piutang sebagai pembayaran non-tunai, dan ikut terbalik bersama alokasinya | `FIN-DEC-049`, `055`, `058`, `062`; `FIN-DES-048`..`050`, **`052`** (kode final, AMENDMENT REVISI 6); `FR-FIN-093`..`095`, `099` | `FIN-API-1.2` C.2, C.3; `FIN-STATE-1.3` C.4; `FIN-VAL-1.5` `FIN-VAL-118`, `119`, `121`, `128`..`132`, `137`; `FIN-INTEGRATION-1.6` §5.10.2 kode 32-35 | `FinanceReceivableService.ApplyAllocationAsync`/`ReverseAllocationAsync` **apa adanya** — tidak diubah; `FinanceAccountingOutboxService` (`BE-FIN-011` ✅) | `FinanceReceiptService.AllocateAsync`: terima `deductions[]` per baris alokasi `RECEIVABLE`, buat `FinReceiptDeduction` + panggil `ApplyAllocationAsync` + tulis `POTONGAN-PPH23-PIUTANG`/`POTONGAN-BIAYA-BANK-PIUTANG` per potongan (nama final `FIN-DES-052` — **bukan** `POTONGAN-PIUTANG-NON-TUNAI`); `FinanceReceiptService.ReverseAllocationAsync`: baris pembalik per potongan + `ReverseAllocationAsync` + `PEMBALIKAN-POTONGAN-PPH23-PIUTANG`/`PEMBALIKAN-POTONGAN-BIAYA-BANK-PIUTANG`; `GET /receipts/{id}/deductions`; **tanpa** `POST /receipts/{id}/deductions` | `BE-FIN-038` ✅ | Seluruh baris `FIN-TEST-1.3`/`1.6` C.3; **regresi**: permintaan alokasi tanpa `deductions` berperilaku persis seperti sebelum task ini | `dotnet build` PASS 0 error (dikonfirmasi pengguna 30 September 2026); `FIN-TEST-1.3`/`1.6` C.3; skema potongan aktif di database | Backend Owner. **MENGUBAH SERVICE YANG SUDAH BERJALAN** (`FinanceReceiptService.AllocateAsync`/`ReverseAllocationAsync`) — perubahan aditif, bersyarat ada potongan | Nol kode `AR_PAYMENT`/`PENERIMAAN-PIUTANG`/`PENYESUAIAN-PIUTANG` untuk potongan; nol worker pengiriman — ✅ **Selesai 30 September 2026, build PASS 0 error dikonfirmasi pengguna, skema DB aktif**, [laporan](../task/report/backend/BE-FIN-040.md) |
| ✅ `BE-FIN-041` | Pembayaran supplier punya tempat untuk mencatat porsi yang dilunasi deposit | `FIN-DES-045`; `02-backend-architecture.md` D.6, D.7 baris 3 | `data-dictionary.md` D.1, D.4(a) | `FinPayment` + `FinPaymentConfiguration` (`BE-FIN-020` ✅) | Kolom `DepositAppliedAmount` (default 0) pada entity + configuration; `CK_FinPayment_NetTransfer` diganti rumus baru; `CK_FinPayment_DepositApplied`; migration `AddDepositAppliedAmountToFinPayment` | — (tabel `FinPayment` sudah ada) | Seluruh baris `FinPayment` lama tetap lolos constraint baru tanpa backfill; `NetTransferAmount` hitungan service untuk pembayaran lama tidak berubah | `dotnet build` PASS 0 error (dikonfirmasi pengguna 30 September 2026); migration diterapkan di database via DBeaver 30 September 2026 (`be-fin-041-038-043-schema-migration-dbeaver.sql`) | Backend Owner. **TABEL YANG SUDAH BERJALAN** — otorisasi migration dieksekusi di database | Nol perubahan perilaku sebelum `BE-FIN-036`: kolom ada, nilainya selalu 0 — ✅ **Selesai 30 September 2026, build PASS 0 error dikonfirmasi pengguna, migrasi DBeaver aktif**, [laporan](../task/report/backend/BE-FIN-041.md) |
| 🟡 `BE-FIN-042` | Penyelarasan 6 controller legacy Finance ke nama kanonikal `Finance*`, seeder ekspansi payung `Finance.AP`/`Finance.AR`, dan skrip SQL migrasi data peran | `FIN-DEC-078`, `FIN-DEC-079`; `FIN-DES-061`..`063`; `FIN-CQ-08`, `FIN-CAP-043`, `FIN-OQ-036` | `contracts/permission-audit-matrix.md` (`FIN-PERM-1.3`, Bagian D) | Enam controller legacy (`BE-FIN-009` ✅, `012` ✅, `018` ✅, `019` ✅, `020` ✅); `AccessMenuSeeder.cs`; `SysAccessPolicy`/`SysControllerAccess`/`SysActionAccess` (**bukan** `SysRolePermissions` — tabel itu tidak ada di skema nyata, lihat laporan §3.3) | Enam controller legacy Finance (ganti string `[AccessPermission]` ke `FinancePayment`, `FinanceReceipt`, `FinanceReceivable`, `FinanceSupplierPayable`, `FinanceBillingIntake`, `FinanceAccountingEvent`) — **selesai**; skrip `Migrations/scripts/be-fin-042-role-permissions-migration.sql` ditulis ulang untuk skema `SysAccessPolicy` nyata — **selesai**; pendaftaran payung `Finance.AP`/`Finance.AR` di `AccessMenuSeeder.cs` + ekspansi ke 13 resource granular — **BLOCKED**, lihat laporan §7 | `BE-FIN-009` ✅, `BE-FIN-012` ✅, `BE-FIN-018` ✅, `BE-FIN-019` ✅, `BE-FIN-020` ✅ — **seluruhnya selesai, bebas mulai** | 1. Seluruh 6 controller memakai nama resource berawalan `"Finance"`; nol string pendek tersisa. 2. Seeder RBAC memperluas payung `Finance.AP` ke 9 resource granular dan `Finance.AR` ke 4 resource granular. 3. Skrip SQL idempotent siap dieksekusi DBA | Review diff 6 controller + grep nol sisa nama pendek — **selesai**; review skrip SQL terhadap pola established `be-sec-003b-policy-expansion.sql` — **selesai**; `dotnet build` PASS 0 error 30 September 2026 | Backend Owner + Security Owner. **MENGUBAH STRING OTORISASI BERJALAN** — eksekusi skrip SQL di database wajib berbarengan dengan rilis backend agar peran aktif tidak mengalami 403 Forbidden. **Temuan kritis**: `Finance.AP`/`Finance.AR` sudah dipakai `FinanceApController`/`FinanceArController` (V2, nyata berjalan) — seeder ekspansi payung ditunda menunggu resolusi platform registry `FIN-OQ-039` | Nol string nama pendek tersisa — **terpenuhi**; skrip SQL idempotent ada di `Migrations/scripts/` — **terpenuhi**; seeder RBAC menguji ekspansi payung-ke-granular — **tidak terpenuhi, BLOCKED** — 🟡 **Source rename & skrip SQL selesai 29 September 2026, build PASS 30 September 2026**, [laporan](../task/report/backend/BE-FIN-042.md) |
| ✅ `BE-FIN-043` | Kolom `PPNAmount` pada `FinSupplierReturn`, constraint `PPNAmount >= 0`, dan kalkulasi deposit retur saat `CONFIRMED` memperhitungkan PPN | `FIN-DEC-047`, `FIN-DEC-061`, `FIN-DEC-067`, **`FIN-DEC-068`**, `FIN-DES-055`; `02-backend-architecture.md` E.9..E.10 | `contracts/integration-contract.md` (`FIN-INTEGRATION-1.6` §5.10.2 kode 36), `contracts/validation-matrix.md` (`FIN-VAL-1.5` `FIN-VAL-133`, `135`, `136`, `143`), `erd/data-dictionary.md` | `FinSupplierReturn` (`BE-FIN-030` ✅), `FinanceSupplierReturnService` (`BE-FIN-035` ✅) | `FinSupplierReturn.cs` + configuration (`PPNAmount numeric(18,2) NOT NULL DEFAULT 0`, `CK_FinSupplierReturn_PPNAmount`); migration `AddPPNAmountToFinSupplierReturn`; `CreateSupplierReturnRequest` DTO bertambah `PPNAmount`; `FinanceSupplierReturnService` menghitung `AvailableAmount = TotalAmount + PPNAmount` saat `CONFIRMED` + menerbitkan `PPN-MASUKAN-RETUR-PEMBELIAN` bila `PPNAmount > 0` | `BE-FIN-030` ✅, `BE-FIN-031` ✅, `BE-FIN-035` ✅ | 1. Migration aditif dengan default 0, seluruh baris lama memenuhi constraint tanpa backfill. 2. Retur dengan PPN negatif ditolak `400` (`FIN-VAL-143`). 3. Deposit Retur yang terbit bernilai `TotalAmount + PPNAmount` | Review diff configuration dan service — **selesai**; verifikasi berkas migration `AddPPNAmountToFinSupplierReturn` — **selesai**; `dotnet build` PASS 0 error (dikonfirmasi pengguna 30 September 2026); migrasi dieksekusi di database via DBeaver 30 September 2026 | Backend Owner. **TABEL YANG SUDAH BERJALAN** — migrasi skema dieksekusi via DBeaver | Migration file aditif, Designer, model snapshot terbarui; service menghitung saldo deposit termasuk PPN — ✅ **Selesai 30 September 2026, build PASS 0 error dikonfirmasi pengguna, migrasi DBeaver aktif**, [laporan](../task/report/backend/BE-FIN-043.md) |
| ✅ `BE-FIN-044` | Empat titik pemanggilan service yang sudah berjalan diselaraskan menggunakan nama konstanta `EventTypeCode` katalog resmi Accounting | `FIN-DEC-068`, `FIN-DES-058`, `evidence/15` | `contracts/integration-contract.md` (`FIN-INTEGRATION-1.4` Bagian 5.10) | `FinAccountingEventOutbox.cs` (`BE-FIN-010` ✅), `FinanceSupplierPayableService.cs`, `FinancePaymentService.cs`, `FinanceReceivableService.cs` | `FinanceSupplierPayableService` (baris ~112: `PENGAKUAN-HUTANG-SUPPLIER`), `FinancePaymentService` (baris ~555: `PEMBAYARAN-HUTANG-SUPPLIER`), `FinanceReceivableService` (baris ~604: `PENERIMAAN-PIUTANG`, ~698: **`PEMUTIHAN-PIUTANG`** — bukan `PENGHAPUSAN-PIUTANG`, dibetulkan revisi roadmap 7 mengikuti katalog `02-backend-architecture.md` E.5 dan `FIN-DES-058`); 5 alias lama di `FinAccountingEventOutbox.cs` dihapus | `BE-FIN-010` ✅, `BE-FIN-011` ✅, `BE-FIN-019` ✅, `BE-FIN-020` ✅, `BE-FIN-008` ✅ — **seluruhnya selesai, bebas mulai** | 1. Seluruh outbox baru terbit dengan nama katalog resmi; nol alias lama tersisa di kode pemanggil. 2. Lima konstanta alias yang dilarang dihapus dari `FinAccountingEventOutbox.cs`. 3. Baris outbox historis berstatus `PENDING` tidak diubah | Review diff 4 berkas service dan model outbox; verifikasi compiler memastikan nol pemanggil alias lama; `dotnet build` PASS 0 error (dikonfirmasi pengguna 30 September 2026) | Backend Owner. **MENGUBAH SERVICE BERJALAN** — perubahan string konstanta pada pemanggilan outbox | 5 konstanta alias dihapus; compiler memastikan nol pemanggil yang masih memakai alias lama — ✅ **Selesai 30 September 2026, build PASS 0 error dikonfirmasi pengguna**, [laporan](../task/report/backend/BE-FIN-044.md) |
| ✅ `BE-FIN-045` | Accounting tahu kapan satu shift kasir benar-benar tertutup, dan kapan ia dibuka kembali | `FIN-DEC-070`, `072`, `073`, `075`; `FIN-DES-054`, `059`; `FR-FIN-108`..`110` | `FIN-INTEGRATION-1.5` §5.10, §5.11; `FIN-VAL-1.4` `FIN-VAL-138`; `FIN-TEST-1.5` §D.3, §E.1 | Daftar tertutup kode penanda pada `ValidateRequest` (`BE-FIN-023`); jalur sinkronisasi shift pada `FinanceBillingIntakeService` (`BE-FIN-025`); `BilCashierShift` **read-only** (`FIN-CAP-024`) | Dua kode penanda tanpa lawan jurnal: `PENUTUPAN-SHIFT-KASIR` saat shift mencapai **`CLOSED`** (kas pas, dari `CloseAsync`) **atau `REVIEWED`** (selisih sudah disahkan), dan `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR` saat shift `CLOSED`/`REVIEWED` → `REOPENED`. Keduanya `Amount = 0`, `SourceTransactionId` `BilCashierShift.Id`, `AccountingDate` tanggal shift, `SourceVersion` **dipatok per siklus** — nomor siklus diambil dari jumlah penanda pembalik yang sudah terbit untuk shift itu. Gerbang worker `FIN-DES-059`: baris ditulis `PENDING` dan worker **melewatinya** selama `FIN-OQ-035` belum dijawab, mengikuti pola `FIN-VAL-132` | `BE-FIN-023`, `BE-FIN-025` | Shift ditutup dengan kas **pas** (`CLOSED`) → **satu** `PENUTUPAN-SHIFT-KASIR` bernilai `0` — inilah skenario yang paling mudah terlewat, dan mayoritas shift ada di sini; shift dengan selisih sesudah disahkan (`REVIEWED`) → **satu** penanda **dan** satu `SELISIH-KAS-*`; shift `CLOSED_WITH_VARIANCE` → **nol** penanda; shift `PERLU_TINDAK_LANJUT` → **nol** penanda; shift dibuka kembali → **satu** `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR` bernilai `0`, dan penanda penutupan berikutnya **boleh** terbit lagi pada siklus berikutnya; selama gerbang `FIN-OQ-035` tertutup baris tetap `PENDING` dengan `AttemptCount` `0` dan **tidak** ditandai `FAILED` | `dotnet build` PASS 0 error (dikonfirmasi pengguna 30 September 2026); `FIN-TEST-1.5` §D.3 (lima baris penanda) dan §E.1 (empat baris gerbang); verifikasi proses bisnis atas keempat keadaan shift | Backend Owner. **Gerbang keras:** worker pengiriman kedua kode ini MUST NOT diaktifkan sebelum `FIN-OQ-035` dijawab Accounting | `Amount = 0` tidak diakali menjadi nilai simbolis; nol worker pengiriman diaktifkan; daftar kode penanda tetap satu tempat (milik `BE-FIN-023`), tidak disalin ke sini — ✅ **Selesai 30 September 2026, build PASS 0 error dikonfirmasi pengguna.** Nol migration, nol resource/aksi hak akses baru, nol tulisan ke tabel `Bil*`. [Laporan](../task/report/backend/BE-FIN-045.md) |
| ✅ `BE-FIN-046` | Uang muka pasien yang ternyata tidak jadi diterima tidak tertinggal sebagai saldo palsu | `FIN-DEC-040`, `044`, **`077`**; `FIN-DES-035`, `057`; `BKC-DEC-128`..`131` | `FIN-INTEGRATION-1.5` §5.10; `FIN-VAL-1.4` `FIN-VAL-142`; `FIN-TEST-1.5` §D.6 | Jalur intake `DEPOSIT_MOVEMENT` (`BE-FIN-025`); `BilDepositMovement` dan `BilTender` **read-only**; `BillingSettlementService.HandleDepositTopUpReversalAsync` milik Billing (`BE-BKC-079`, **sudah berjalan** pada `7811c048`) — hanya **dibaca**, tidak disentuh | Mutasi `REVERSAL` atas top-up deposit dikonsumsi menjadi `PEMBALIKAN-PENERIMAAN-UANG-MUKA`. Pendeteksi `FIN-VAL-142`: untuk setiap `BilTender` `REVERSED` yang settlement-nya bertujuan top-up deposit, periksa adanya mutasi pembalik yang bersesuaian — bila tidak ada, baris intake `ERROR` menunjuk `FIN-OQ-034`, **nol** kejadian. Konstanta `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` ditambahkan ke katalog **tanpa penulis** di task ini; penulisnya `BE-FIN-047` | `BE-FIN-025` | Pembalikan top-up yang dananya **belum** terpakai → `PEMBALIKAN-PENERIMAAN-UANG-MUKA` terbit sesuai rancangan yang sudah ada; tender top-up dibalik **tanpa** mutasi pembalik → baris intake `ERROR` menunjuk `FIN-OQ-034`, **nol** kejadian, **nol** tulisan ke tabel `Bil*`; pemeriksaan hanya **membaca** `BilTender` dan `BilDepositMovement` | `dotnet build` PASS 0 error (dikonfirmasi pengguna 30 September 2026); `FIN-TEST-1.5` §D.6 (dua baris terakhir); verifikasi proses bisnis; perbandingan `RowVersion`/`UpdateDateTime` tabel `Bil*` sebelum dan sesudah | Backend Owner. Sesudah `FIN-DEC-077`, pendeteksi `FIN-VAL-142` berubah sifat menjadi **jaring pengaman**, bukan jalur utama | Nol tulisan ke tabel `Bil*` mana pun; nol kejadian diterbitkan untuk fakta yang tidak lengkap — ✅ **Selesai 30 September 2026, build PASS 0 error dikonfirmasi pengguna.** Nol migration, nol endpoint baru, nol resource/aksi hak akses baru. [Laporan](../task/report/backend/BE-FIN-046.md) |
| ✅ `BE-FIN-047` | Pembatalan alokasi uang muka membuka kembali piutang di buku besar (D Piutang, K Uang Muka Pasien), bukan mencatat kas keluar fiktif atau meninggalkan saldo minus | `FIN-DEC-063`, `077`, **`080`**, **`081`**; `FIN-DES-064`, `065`; `BKC-DEC-128`..`131` | `FIN-INTEGRATION-1.6` §5.10 (pemicu `RELEASE` berpasangan); `FIN-VAL-1.5` `FIN-VAL-144`..`146`; `FIN-TEST-1.6` §F.1..§F.3 | Jalur intake `DEPOSIT_MOVEMENT` (`BE-FIN-025`); pasangan kejadian dari `BE-FIN-046` | Mutasi `BilDepositMovement` bertipe `RELEASE` yang berpasangan dengan `REVERSAL` pada `CausationId` yang sama dikonsumsi menjadi kejadian `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` sebesar porsi alokasi yang dibatalkan (`Amount` mutasi `RELEASE`). Pencegahan kesalahan `FIN-VAL-144`: mutasi `RELEASE` **dilarang** diterbitkan sebagai `PENGEMBALIAN-UANG-MUKA`. Penanganan anomali `FIN-VAL-145`: mutasi `RELEASE` tanpa pasangan `REVERSAL` ber-`CausationId` sama ditolak secara *fail-closed* sebagai baris intake `ERROR` yang menunjuk `FIN-OQ-037` (`evidence/19`). `SourceVersion` dipatok `"1"`, `AccountingDate` tanggal mutasi (`OccurredAt`) | `BE-FIN-025`, `BE-FIN-046` | 1. Tender top-up dibalik setelah dananya dipakai melunasi tagihan → terbit **dua** kejadian berpasangan: `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` dan `PEMBALIKAN-PENERIMAAN-UANG-MUKA`. Hasil bersih di buku besar: Debit Piutang, Kredit Kas; saldo Uang Muka Pasien kembali nol (`FIN-TEST-1.6` §F.1). 2. Mutasi `RELEASE` dikirim sebagai `PENGEMBALIAN-UANG-MUKA` → ditolak sebagai kesalahan kode (`FIN-VAL-144`); nol baris outbox `PENGEMBALIAN-UANG-MUKA` untuk mutasi `RELEASE`. 3. Mutasi `RELEASE` tanpa pasangan `REVERSAL` ber-`CausationId` sama → baris intake berstatus `ERROR` menunjuk `FIN-OQ-037` dan `0` kejadian outbox diterbitkan (`FIN-VAL-145`, `FIN-TEST-1.6` §F.2). 4. Tender top-up dibalik sebelum dananya dipakai → hanya `PEMBALIKAN-PENERIMAAN-UANG-MUKA`, nol `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` | `dotnet build` PASS 0 error (dikonfirmasi pengguna 30 September 2026); `FIN-TEST-1.6` §F.1..§F.3; verifikasi proses bisnis (pembatalan alokasi tagihan LIFO); perbandingan `RowVersion`/`UpdateDateTime` tabel `Bil*` sebelum dan sesudah | Backend Owner. Blocker dicabut via `FIN-DEC-080` dan `FIN-DEC-081` (`FIN-DES-064`, `FIN-DES-065`) | Nol kejadian `PENGEMBALIAN-UANG-MUKA` dari mutasi `RELEASE` — terpenuhi struktural; penanganan `ERROR` untuk mutasi tanpa pasangan — terpenuhi; nol tulisan ke tabel `Bil*` — terpenuhi. Pemasangan mutasi menggunakan `CausationId` (keduanya diisi `tender.CorrelationId`, diverifikasi identik di source Billing) — ✅ **Selesai 30 September 2026, build PASS 0 error dikonfirmasi pengguna.** Nol migration, nol endpoint baru, nol resource/aksi hak akses baru. [Laporan](../task/report/backend/BE-FIN-047.md) §3.3 |
| ✅ `BE-FIN-048` | Pengetatan validasi pesan saldo subledger pada kotak keluar sesuai aturan Accounting | `FIN-DEC-091`, `FIN-DEC-092`, `ACC-DEC-109`, `ACC-DEC-110` | `contracts/validation-matrix.md` (`FIN-VAL-079`, `FIN-VAL-082`), `ACC-XMOD-0.4` §8a | `FinanceAccountingOutboxService.cs` | (1) Kunci `request.Amount < 0` melempar galat tanpa pengecualian untuk `SALDO-SUBLEDGER` (menolak nominal negatif). (2) Validasi bahwa untuk `SALDO-SUBLEDGER`, `AccountingDate` wajib sama dengan hari terakhir periode pada `SubledgerBalance.AccountingPeriodCode` | `BE-FIN-023` ✅ | Pesan saldo bernilai negatif ditolak `400`; `AccountingDate` bukan tanggal akhir periode ditolak `400`; pesan saldo bernilai `0.00` dan positif dengan tanggal akhir periode tersimpan | `dotnet build` PASS 0 error; unit test validasi outbox | Backend Owner | Nol perubahan skema database — ✅ **Selesai 30 September 2026**, validasi nilai positif dan tanggal akhir periode terpasang di `ValidateRequest`, [laporan](../task/report/backend/BE-FIN-048.md) |
| ✅ `BE-FIN-049` | Layanan kalkulasi snapshot saldo subledger bulanan untuk 4 control account dan penerbitan 4 event `SALDO-SUBLEDGER` ke outbox | `FIN-DEC-090`, `ACC-DEC-108`, `ACC-DEC-107`, `ACC-DEC-111` | `contracts/integration-contract.md` §5.6, `ACC-XMOD-0.4` §8a | `FinDailyCashSnapshot`, `FinPettyCashBudget`, `FinReceivable`, `FinSupplierPayable`, `FinanceAccountingOutboxService` | `FinanceSubledgerSnapshotService` (baru): menghitung agregat posisi saldo Kas Kasir, Kas Kecil, Piutang, dan Utang Supplier per akhir bulan, menerbitkan tepat 4 kejadian `SALDO-SUBLEDGER` bertanggal hari terakhir periode ke `FinAccountingEventOutbox`, termasuk yang bernilai `0.00`; endpoint `POST /subledger-balances/generate` dan `GET /subledger-balances/{accountingPeriodCode}` | `BE-FIN-048` ✅ | 4 kejadian `SALDO-SUBLEDGER` terbit dengan `AccountingDate` hari terakhir periode; control account tanpa mutasi tetap terbit dengan `0.00`; nilai utang terbit dalam angka positif | `dotnet build` PASS 0 error; verifikasi hasil pembacaan 4 akun kontrol | Backend Owner | Tidak membuat tabel `FinSubledgerPeriodBalance` (tetap POST-MVP); hasil agregat langsung diterbitkan ke `FinAccountingEventOutbox` — ✅ **Selesai 30 September 2026**, service dan endpoint snapshot 4 akun kontrol terpasang, [laporan](../task/report/backend/BE-FIN-049.md) |
| ✅ `BE-FIN-050` | Buku register penerimaan kasir dan rekonsiliasi shift Finance vs. `SystemCash` Billing tersedia — menutup gap yang eksplisit dikecualikan `BE-FIN-018` | `FIN-API-1.0` (tidak berubah — dua endpoint sudah tercantum sejak awal, service-nya yang menyusul) | `contracts/api-contract.md` (grup Receipt, baris `GET /register`, `GET /shift-reconciliation`), `permission-audit-matrix.md` baris 79-80 | `FinReceipt` (`KwitansiNumber`/`CashierShiftId`/`SourceTenderId` sudah ada sejak `BE-FIN-016`); `BilCashierShift.SystemCash` (Billing, **dibaca**, tidak diubah); `MstPaymentMethod.IsCash` (sudah dipakai `AllocateAsync`) | `FinanceReceiptService.GetRegisterAsync` (baru): daftar berpaging `FinReceipt` per tanggal/shift, memuat `KwitansiNumber`+`SourceTenderId` untuk telusur; `GetShiftReconciliationAsync` (baru): satu shift per pemanggilan (kontrak `ShiftReconciliationQuery`/`Response` bukan daftar), membandingkan `BilCashierShift.SystemCash` dengan jumlah bersih `FinReceipt.Amount` bermetode tunai pada shift itu (baris `REVERSED` menetralkan baris aslinya, pola sama `GetInvoiceBreakdownAsync`); dua endpoint baru `FinanceReceiptsController` — nol perubahan skema | `BE-FIN-018` ✅ (endpoint saudara sudah berjalan) | Register menampilkan `KwitansiNumber`/`SourceTenderId` per baris, dapat disaring tanggal dan shift; rekonsiliasi shift kembalikan `404` untuk `CashierShiftId` yang tidak ada; `Variance = SystemCash − FinanceNetCashReceiptAmount`, bernilai nol saat kas pas | `dotnet build` PASS 0 error (dikonfirmasi pengguna 30 September 2026); review kode terhadap `BilCashierShift`/`MstPaymentMethod` sebagai bukti field nyata | Backend Owner. **Task baru** — diotorisasi eksplisit pemilik repository pada sesi FE-FIN-004, bukan hasil `/plan-module-delivery` terpisah; cakupan dipersempit hanya dua endpoint ini (bukan `POST /receipts` manual maupun `POST /receipts/{id}/reverse` manual, yang tetap gap terbuka) | Nol tulisan ke `FinReceipt` maupun `BilCashierShift` — ✅ **Selesai 30 September 2026, build PASS 0 error dikonfirmasi pengguna**, [laporan](../task/report/backend/BE-FIN-050.md) |
| ✅ `BE-FIN-051` | Header `Idempotency-Key` ditegakkan pada 14 perintah uang rumpun Purchasing (PO, GR, Tukar Faktur, Purchasing Invoice, Supplier Return) — menutup gap yang dicatat `FE-FIN-008` | `FIN-DES-006` (api-contract.md baris 26: "Perintah yang memindahkan uang — Wajib header Idempotency-Key"); keputusan desain penyimpanan (ledger terpisah, bukan kolom per-entity) diotorisasi eksplisit pemilik repository pada sesi ini | `contracts/api-contract.md` §B.1-B.5 (kolom Request "Idempotency-Key" pada baris yang berlaku) | Pola `PettyCashBudgetService`/`Controller` (kolom `IdempotencyKey` pada baris "movement") — **tidak ditiru langsung**, lihat alasan pada `FinPurchasingIdempotencyRecord.cs` | Model+configuration baru `FinPurchasingIdempotencyRecord` (satu tabel ledger, unique index `IdempotencyKey`); service baru `PurchasingIdempotencyService` (cek-sebelum, simpan-sesudah); 14 aksi lintas 5 controller Purchasing diberi parameter `[FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey` + panggilan cek/simpan — PO (Create/Submit/Approve/Cancel), GR (Create/Cancel), Tukar Faktur (Create/Cancel), Purchasing Invoice (Create/Submit/Approve), Supplier Return (Create/Confirm/Cancel). `Reject` dan `PUT` (Update) **sengaja tidak disentuh** — kontrak tidak mensyaratkan header di situ | `BE-FIN-032`..`035` ✅ (kelima controller sudah berjalan) | Permintaan dengan `Idempotency-Key` yang sama dua kali pada aksi yang sama mengembalikan respons persis pertama kali (replay), bukan memproses ulang; permintaan tanpa header ditolak `400` otomatis (`[ApiController]` model binding, parameter wajib bukan nullable); dua request bersamaan ber-kunci sama tidak menghasilkan dua baris ledger (indeks unik) | `dotnet build` PASS 0 error (dikonfirmasi pengguna 30 September 2026); migration `TableFinPurchasingIdempotencyRecord` dibuat pengguna, diverifikasi sinkron (migrasi probe kosong, dihapus), dan dieksekusi ke database — berjalan lancar (dikonfirmasi pengguna) | Backend Owner. **Task baru** — diotorisasi eksplisit pemilik repository di tengah sesi `FE-FIN-004`/`BE-FIN-050`. Jendela atomisitas: baris ledger ditulis di `SaveChangesAsync` TERPISAH sesudah transaksi bisnis masing-masing service commit (nol perubahan pada 5 service yang sudah berjalan) — bila proses mati tepat di antara keduanya, permintaan ulang memproses ulang aksi bisnisnya (bukan silent double-processing, penjaga status entity tetap menolak transisi ilegal); trade-off dicatat, bukan didiamkan | Nol perubahan pada lima service Purchasing yang sudah berjalan (`FinancePurchaseOrderService`, dst.) — seluruh perubahan bisnis nol, murni penambahan lapisan cek/simpan di controller — ✅ **Selesai 30 September 2026**, `dotnet build` PASS 0 error dikonfirmasi pengguna, migrasi dieksekusi ke database dikonfirmasi pengguna, [laporan](../task/report/backend/BE-FIN-051.md) |

## 4. Rincian per gelombang

### `MVP-0` — Fondasi data induk (`EPIC FIN-01`)

| Aspek | Isi |
|---|---|
| Status | ✅ **SELESAI 23 September 2026.** `BE-FIN-001` ✅ selesai 21 September 2026. `BE-FIN-002` ✅ selesai 23 September 2026 — `MstBank` **tidak** dibuat baru (sudah ada aktif di `Areas/Administrator/MasterData`, dipakai ulang atas arahan pemilik repository); `MstBankAccount`, `MstCurrency`, `MstExchangeRate` selesai. `BE-FIN-003` ✅ selesai 23 September 2026 — file migration untuk 3 tabel dibuat tangan (tanpa `dotnet ef migrations add`, atas instruksi pemilik repository), `dotnet build` PASS dan migration sudah dieksekusi (dikonfirmasi pengguna 23 September 2026). `BE-FIN-004` ✅ selesai 23 September 2026 — `BankAccountsController`/`CurrenciesController` selesai (2 dari 3 controller yang direncanakan; grup `Bank` sengaja tidak dibuat, konsekuensi `BE-FIN-002`), diuji end-to-end dengan hasil sesuai ekspektasi (dikonfirmasi pengguna 23 September 2026). Bukti: [BE-FIN-001](../task/report/backend/BE-FIN-001.md), [BE-FIN-002](../task/report/backend/BE-FIN-002.md), [BE-FIN-003](../task/report/backend/BE-FIN-003.md), [BE-FIN-004](../task/report/backend/BE-FIN-004.md) |
| Tabel baru | `MstBankAccount`, `MstCurrency`, `MstExchangeRate` — **bukan** `MstBank` (lihat baris Status; delta terhadap rancangan awal, perlu diratifikasi pemilik blueprint) |
| Migration | `AddFinanceMasterData` — nomor 1 dari 7 |
| Selesai bila | `UAT-01` dan `UAT-02` lulus: data induk siap dipakai, nomor rekening ganda ditolak |
| Yang mudah salah | Memakai prefix `Fin` untuk keempat tabel. `FIN-DES-003` menetapkan **`Mst`** untuk entity yang berperan sebagai data induk, dan itu ketentuan modul, bukan sekadar keputusan satu pass |

### `MVP-1` — Pintu masuk fakta dan buku piutang (`EPIC FIN-02`, `FIN-03`)

| Aspek | Isi |
|---|---|
| Status | ✅ **SELESAI 23 September 2026 (5 dari 5 task).** `BE-FIN-005`..`008` ✅ — `dotnet build` PASS, migration diterapkan, endpoint diuji langsung; lihat baris masing-masing pada tabel task bagian 3. `BE-FIN-009` ✅ — `FinanceReceivablesController` selesai penuh; `FinanceBillingIntakeService`+`Controller` dibangun atas otorisasi eksplisit pemilik repository (gap `BE-FIN-005` ditutup di sini). Dua inferensi bisnis yang sebelumnya berisiko tinggi sudah ditutup dengan bukti source (bukan ratifikasi dokumen): `BilArHandoff.Amount` dikonfirmasi net lewat `BillingArApHandoffService.cs`, dan `PatientId` diisi lewat join `RegPatientEncounter` — lihat [laporan BE-FIN-009](../task/report/backend/BE-FIN-009.md) bagian 1 dan 7. `UAT-03` dan `UAT-04` terpenuhi |
| Tabel baru | `FinBillingHandoffIntake` + 5 tabel piutang |
| Migration | `AddFinanceBillingIntake`, `AddFinanceReceivableAndCollection` |
| Selesai bila | `UAT-03` dan `UAT-04` lulus |
| Yang mudah salah | Membuat lebih dari satu penulis `OutstandingAmount`. `FIN-DES-011` menetapkan `FinanceReceivableService` sebagai **satu-satunya**; dua penulis akan membuat saldo piutang berbeda tergantung jalur mana yang dipakai |

### `MVP-5` — Kotak keluar kejadian (`EPIC FIN-11`)

| Aspek | Isi |
|---|---|
| Status | ✅ **SELESAI 23 September 2026.** `BE-FIN-010` ✅ selesai 23 September 2026 — entity, configuration, dan migration `AddFinanceAccountingOutbox` selesai ditulis tangan (atas otorisasi eksplisit pemilik repository), sudah dieksekusi ke database (dikonfirmasi pengguna). `FinSubledgerPeriodBalance` sengaja tidak dibuat (bukan Cakupan `BE-FIN-010`, field masih draf menunggu `FIN-OQ-011`). `EventTypeCode` tidak diberi check constraint karena katalog 17 kode (`FIN-DEC-002`) belum diratifikasi Accounting — tidak menahan status. `BE-FIN-011` ✅ selesai 23 September 2026 — `FinanceAccountingOutboxService` selesai, diperluas ke 3 titik pemanggilan nyata (`PENGAKUAN-PIUTANG`, `PENYESUAIAN-PIUTANG`, `PEMUTIHAN-PIUTANG`) atas otorisasi eksplisit; 2 error compiler yang sempat dilaporkan 22 September sudah diperbaiki dan build ulang PASS; arah DEBIT/CREDIT belum terbawa ke kejadian (dicatat sebagai risiko terbuka, bukan blocker); QBE `PASS` (47 berkas, 0 pelanggaran). `BE-FIN-012` ✅ selesai 23 September 2026 — `FinanceAccountingEventsController` (4 endpoint `GET`, nol endpoint pengirim) dan `FinanceAccountingEventService` selesai; 1 error compiler yang sempat dilaporkan 22 September sudah diperbaiki; QBE `PASS` (50 berkas, 0 pelanggaran). Ketiga task: `dotnet build` PASS, migration `BE-FIN-010` diterapkan, diuji end-to-end — dikonfirmasi pengguna 23 September 2026. Bukti: [BE-FIN-010](../task/report/backend/BE-FIN-010.md), [BE-FIN-011](../task/report/backend/BE-FIN-011.md), [BE-FIN-012](../task/report/backend/BE-FIN-012.md) |
| Tabel baru | `FinAccountingEventOutbox`, `FinAccountingEventAttempt` |
| Migration | `AddFinanceAccountingOutbox` |
| Selesai bila | `UAT-07`, `UAT-17`, `UAT-18`, `UAT-19` lulus |
| Yang mudah salah | `FinanceAccountingOutboxService` membuka transaksi sendiri. `FIN-DES-017` mewajibkan ia **ikut** transaksi pemanggil — kalau tidak, fakta bisa tersimpan tanpa kejadiannya |

### `MVP-4` — Setoran bank dan kas harian (`EPIC FIN-10`)

| Aspek | Isi |
|---|---|
| Status | ✅ **SELESAI 23 September 2026.** `BE-FIN-013` ✅ selesai 23 September 2026 — entity `FinBankDeposit` dan `FinDailyCashSnapshot` selesai beserta EF configuration dan migration `AddFinanceCashManagement` ditulis tangan, sudah dieksekusi ke database (dikonfirmasi pengguna). `BE-FIN-014` ✅ selesai 23 September 2026 — `FinanceCashManagementService` selesai beserta DTO dan registrasi DI, saldo dihitung saat posting (`Serializable` + advisory lock), pembekuan penutupan kas harian, kas kecil tidak mengganggu kas kasir, QBE `PASS`. `BE-FIN-015` ✅ selesai 23 September 2026 — `FinanceBankDepositsController` (7 endpoint) dan `FinanceDailyCashController` (4 endpoint) selesai, kepatuhan `role-access-rules.md` (6 action terdaftar), pembungkus `ApiResponse<T>`, QBE `PASS`. Ketiga task: `dotnet build` PASS, migration diterapkan, diuji end-to-end dengan hasil sesuai ekspektasi — dikonfirmasi pengguna 23 September 2026. Bukti: [BE-FIN-013](../task/report/backend/BE-FIN-013.md), [BE-FIN-014](../task/report/backend/BE-FIN-014.md), [BE-FIN-015](../task/report/backend/BE-FIN-015.md) |
| Tabel baru | `FinBankDeposit`, `FinDailyCashSnapshot` |
| Migration | `AddFinanceCashManagement` |
| Selesai bila | `UAT-13`..`UAT-16` lulus |
| Yang mudah salah | Menghitung saldo saat layar dibuka, bukan saat posting (`FR-FIN-062`); dan membaca `FinReceipt` yang belum ada — lihat bagian 2.1 |

### `MVP-2`, `MVP-3` — tertahan

**Pembaruan 23 September 2026: tidak lagi tertahan.** Owner Billing sudah menjawab (21 September
2026) dan seluruh tiga task (`BE-FIN-016`..`018`) sudah `✅` — judul bagian ini dipertahankan apa
adanya sebagai riwayat penamaan gelombang, bukan diganti.

| Aspek | Isi |
|---|---|
| Status | ✅ **SELESAI 23 September 2026 (3 dari 3 task).** `BE-FIN-016` — jawaban owner Billing turun 21 September 2026 (`BKC-DEC-106`, `108`, `109`, disetujui), task Billing-nya (`BE-BKC-069`) sudah dieksekusi. `FinReceipt`, `FinReceiptAllocation` (entity+configuration+migration `AddFinanceCollection`) selesai; `FinanceBillingIntakeService` (`BE-FIN-009`) diperluas untuk `HandoffType = COLLECTION` — satu tender `SUCCEEDED` menghasilkan satu `FinReceipt` (`FR-FIN-030`/`032`), penerimaan tunai tanpa shift ditolak (`FR-FIN-033`), kejadian `PENERIMAAN-KASIR`/`PEMBALIKAN-PENERIMAAN-KASIR` ditahan `HELD_FOR_FINALIZATION` bila tagihan masih `OPEN` (`FR-FIN-034`). Mekanisme pembalikan (`TenderStatus = REVERSED`) — sebelumnya `BLOCKED` oleh konflik `CK_FinReceipt_TenderRequired`/`IX_FinReceipt_SourceTenderId` — **diperbaiki 23 September 2026**: constraint diberi klausa pengecualian untuk baris pembalik, migration `FixFinReceiptTenderRequiredForReversal` ditulis (belum dieksekusi) — lihat [laporan](../task/report/backend/BE-FIN-016.md) bagian 1.5. `BE-FIN-017` — logika penerimaan dipindah ke `FinanceReceiptService` sesuai `02-backend-architecture.md` §4.22 ("Refactor now"), ditambah `GetInvoiceBreakdownAsync` untuk membuktikan `FR-FIN-035`; `CreateReversalReceiptAsync` di file ini diimplementasikan penuh 23 September 2026. Alokasi manual maker-checker (`FR-FIN-040`..`046`) tetap cakupan `BE-FIN-018`, lihat [laporan](../task/report/backend/BE-FIN-017.md) bagian 1.3. `BE-FIN-018` — `FinanceReceivableService.ApplyAllocationAsync`/`ReverseAllocationAsync` (FR-FIN-042, satu-satunya penulis `OutstandingAmount`) dan `FinanceReceiptService.AllocateAsync`/`ReverseAllocationAsync` (FR-FIN-040/041/045) selesai; FR-FIN-043/044/046 sudah terpenuhi sejak `BE-FIN-008`. Alokasi adalah aksi LANGSUNG Petugas AR menurut `state-transition-matrix.md` §2 — bukan maker-checker seperti tersirat DoD ringkas di bawah (belum diklarifikasi pemilik repository), lihat [laporan](../task/report/backend/BE-FIN-018.md) bagian 0 dan 7. Controller (`FinanceReceiptsController`) dibangun 23 September 2026, QBE belum dijalankan ulang atas berkas baru |
| Tabel baru | `FinReceipt`, `FinReceiptAllocation` |
| Migration | `AddFinanceCollection` |
| Selesai bila | `UAT-05`, `UAT-06`, `UAT-20` lulus |
| Yang mudah salah | `FinReceiptAllocation` diisi dari task ini — **bukan** cakupan `BE-FIN-016`/`017`; pembagian bayar-vs-piutang MANUAL dengan maker-checker tetap tanggung jawab `BE-FIN-018`, dan MUST tidak menciptakan penulis kedua untuk `FinReceivable.OutstandingAmount` (`FinanceReceivableService` tetap satu-satunya) |

### `POST-MVP`

`BE-FIN-019` dan `BE-FIN-020` selesai 23 September 2026 (controller keduanya dibangun). `BE-FIN-021`
tetap dikerjakan sebagian — konsumsi intake-nya menunggu kesiapan modul Medical Fee (`BE-MDF-014`,
belum ada laporan task sama sekali).

`BE-FIN-019` ✅ selesai 23 September 2026 — `FinSupplierPayable`, `FinSupplierPayableItem`,
`FinPayableAdjustment` (entity+configuration+migration `AddFinanceSupplierPayable`) dan
`FinanceSupplierPayableService` (input manual, koreksi maker-checker, pembatalan) selesai;
`MedicalServicePayableId` disiapkan tanpa FK menunggu `BE-FIN-021`. Controller
(`FinanceSupplierPayablesController`) dibangun; kejadian Accounting
(`PENGAKUAN-HUTANG-SUPPLIER`/`PENYESUAIAN-HUTANG`) tetap sengaja belum disambungkan (pola
`BE-FIN-011`, tersendiri, gap terbuka) — lihat [laporan](../task/report/backend/BE-FIN-019.md).

`BE-FIN-020` ✅ selesai 23 September 2026 — `FinPayment`, `FinPaymentAllocation`,
`FinPaymentDeduction` (entity+configuration+migration `AddFinancePayment`) dan
`FinancePaymentService` (siklus hidup lengkap DRAFT→SUBMITTED→APPROVED/REJECTED→PAID/CANCELLED,
pembuktian FR-FIN-050 uang keluar vs utang lunas, pembuktian FR-FIN-051 potongan tidak menyisakan utang,
satu-satunya penulis `PaidAmount` pada utang supplier) selesai; `MedicalServicePayableId` disiapkan
tanpa FK menunggu `BE-FIN-021`. Controller (`FinancePaymentsController`) dibangun — lihat
[laporan](../task/report/backend/BE-FIN-020.md). `FIN-OQ-010` (ambang nominal persis) tetap terbuka.

`BE-FIN-021` 🟡 sebagian 22 September 2026 — `FinMedicalServicePayable`, `FinMedicalServicePayableItem`
(entity+configuration+migration `AddFinanceMedicalServicePayable`, tangan, **belum dijalankan**),
penambahan navigasi dan relasi FK `MedicalServicePayable` pada `FinPaymentAllocation` dan
`FinPayableAdjustment`, serta pendaftaran DbContext dan model snapshot selesai. Alur intake / konsumsi
otomatis penyerahan jasa medis ditangguhkan menunggu modul Medical Fee (`BE-MDF-014`). QBE `PASS` (8 berkas) —
lihat [laporan](../task/report/backend/BE-FIN-021.md).

### `REV-3` — AMENDMENT REVISI 3: uang muka, deposit, selisih kas (`EPIC FIN-14`)

| Aspek | Isi |
|---|---|
| Status | 🟡 **Diperbarui 29 September 2026.** Seluruh task kode dan audit (`BE-FIN-023`, `024`, `025`, `026`) telah ✅ **SELESAI**. Task `BE-FIN-022` 🟡 menunggu konfirmasi eksekusi migration. Rumpun ini juga melahirkan tiga task baru di `REV-6/8` (`BE-FIN-045`..`047`) |
| Blocker | ~~`FIN-OQ-017`~~ **CLOSED** 28 September 2026 (`FIN-DEC-063`..`067`, balasan terkirim `evidence/15`). ~~Isi `BE-FIN-025` basi~~ **diperbaiki** revisi roadmap 7. ~~amendment desain `FIN-DES-057` untuk `BE-FIN-047`~~ **CLOSED** 29 September 2026 (`FIN-DEC-080`, `FIN-DEC-081`, `FIN-DES-064`, `FIN-DES-065`). ~~Otorisasi pembacaan database untuk `BE-FIN-026`~~ **DIBERIKAN** 29 September 2026. Nol blocker tersisa pada task kode/audit `REV-3` |
| Yang BOLEH jalan | `BE-FIN-022` (setelah otorisasi migration turun). `BE-FIN-023`, `024`, `025`, dan `026` **seluruhnya telah ✅ selesai** |
| **Status `BE-FIN-022`** | 🟡 **SEBAGIAN, 25 September 2026.** Source lengkap (konstanta, configuration, berkas migration `AlterFinBillingHandoffIntakeHandoffTypeCheck` beserta Designer, `ModelSnapshot`); `dotnet build` **`0 Error`, 229 Warning** (seluruhnya `CS1573`/`CS1734` pada berkas tidak berkaitan, sudah ada sebelumnya). **Yang belum terpenuhi:** acceptance criteria pertama — baris ber-`HandoffType` baru belum dapat disimpan karena aturan pemeriksaan nilai **di database masih empat nilai**; eksekusi migration di luar wewenang sesi itu. Bukti: [laporan](../task/report/backend/BE-FIN-022.md) |
| **Status `BE-FIN-023`** | ✅ **SELESAI, 29 September 2026.** Source lengkap (`SubledgerBalanceRequest`, `SubledgerBalance`, pencabutan `RequiresFinalization`, pelurusan serialisasi `Components` di `BuildPayloadJson`, daftar tertutup `ZeroAmountAllowedEventTypes`, pengetatan `ValidateRequest`, penyesuaian pemanggil `FinanceReceiptService`); seluruh 7 acceptance criteria terpetakan ke source; `dotnet build` dikonfirmasi 0 error oleh pengguna secara mandiri; rangkaian ditutup tuntas oleh `BE-FIN-024` ✅. Bukti: [laporan](../task/report/backend/BE-FIN-023.md) |
| **Status `BE-FIN-024`** | ✅ **SELESAI, 29 September 2026.** Source lengkap (`PenerimaanUangMuka`, `PembalikanPenerimaanUangMuka` pada katalog outbox, pemilihan `EventTypeCode` dan `reversalEventTypeCode` berbasis `SourceInvoiceStatus` pada `FinanceReceiptService`); seluruh 6 acceptance criteria terpetakan ke source; `dotnet build` dikonfirmasi 0 error oleh pengguna secara mandiri. Bukti: [laporan](../task/report/backend/BE-FIN-024.md) |
| **Status `BE-FIN-025`** | ✅ **SELESAI, 29 September 2026.** Source lengkap (penambahan 5 konstanta kode outbox, 4 jalur sinkronisasi pada `SyncNewFactsAsync`, dispatching `ProcessAsync`, dan 4 method pengolahan intake `DEPOSIT_MOVEMENT`, `REFUNDABLE_CREDIT`, `REFUND_CASE`, `CASH_VARIANCE_REVIEW`); seluruh 13 acceptance criteria terpetakan ke source; nol perubahan pada tabel `Bil*`; `dotnet build` dikonfirmasi 0 error oleh pengguna secara mandiri. Bukti: [laporan](../task/report/backend/BE-FIN-025.md) |
| **Status `BE-FIN-026`** | ✅ **SELESAI, 29 September 2026.** Otorisasi pembacaan database diberikan pengguna, audit fisik langsung terhadap database PostgreSQL `QuilvianNewDevYasmina` membuktikan 0 baris berstatus `HELD_FOR_FINALIZATION` dan 0 baris bernuansa nama pendek lama (`AR_*`/`AP_*`); nol baris dimutasi pada database fisik; kueri SQL diagnostik/remediasi dieksekusi langsung pada DBeaver dan teruji bersih; seluruh 6 acceptance criteria terverifikasi. Bukti: [laporan](../task/report/backend/BE-FIN-026.md) |
| Yang MENGUBAH kode berjalan | `BE-FIN-024` — satu-satunya. `FinanceReceiptService` telah diselaraskan memilih `EventTypeCode` penerimaan dan pembalikan berdasarkan `SourceInvoiceStatus` tanpa menahan kejadian |
| Migration | **Satu** untuk seluruh amendment: `AlterFinBillingHandoffIntakeHandoffTypeCheck` pada `BE-FIN-022`. Tidak ada tabel maupun kolom baru |
| Kenapa murah | Ketiga keputusan arsitekturnya sengaja memakai yang sudah ada: tabel intake diperluas lewat `HandoffType` (`FIN-DES-029`), kotak keluar tidak bertambah kolom karena `EventTypeCode` memang tanpa check constraint (`FIN-DES-030`), dan kode pembalikan diturunkan dari `FinReceipt.SourceInvoiceStatus` yang sudah tersimpan (`FIN-DES-034`) |
| Yang TIDAK dibuat | Nol tabel deposit/refund milik Finance — keenamnya milik Billing dan hanya dibaca (`02-backend-architecture.md` bagian B.7) |

**Urutan yang MUST dihormati, dan alasannya.** `BE-FIN-023` mencabut `RequiresFinalization` dari
`AccountingOutboxEventRequest`. Bila `BE-FIN-024` tidak menyusul di rangkaian yang sama,
penerimaan sebelum tagihan final akan terbit sebagai `PENERIMAAN-KASIR` — dibukukan Accounting
sebagai penerimaan final, padahal uangnya masih kewajiban ke pasien. Keduanya karena itu
**tidak boleh dipisah ke dua sesi kerja yang berjauhan**, dan `BE-FIN-023` sendirian **tidak**
boleh ditandai selesai bila `BE-FIN-024` belum jalan.

### `REV-4` — AMENDMENT REVISI 4: Purchasing/AP, AR Invoice Agregat, Potongan AR (`EPIC FIN-15`, `16`, `17`)

| Aspek | Isi |
|---|---|
| Status | ✅ **SELESAI 30 September 2026.** Seluruh 15 task (`BE-FIN-027` s/d `BE-FIN-041`) telah selesai diimplementasikan, skrip migrasi skema fisik (`BE-FIN-038` dan `BE-FIN-041` via `be-fin-041-038-043-schema-migration-dbeaver.sql`) sukses dieksekusi di database via DBeaver, dan kompilasi proyek `dotnet build` PASS 0 error (dikonfirmasi pengguna 30 September 2026) |
| Gelombang | `POST-MVP` (`04-prd-to-mvp.md` bagian 20.1) — **seluruhnya telah selesai** |
| Task yang BOLEH jalan | `BE-FIN-027`..`041` — **seluruh lima belas task telah ✅ selesai**, nol yang `⛔` |
| Task ⛔ | **Tidak ada**. Seluruh task kode dan skema telah rampung dan diverifikasi |
| Tabel baru | Empat belas — sebelas Purchasing, dua AR Invoice Agregat, satu Potongan AR |
| Kolom baru pada tabel lama | Satu: `FinSupplierPayable.SourcePurchasingInvoiceId` |
| Migration | Empat: `AddPurchasingApRumpun`, `AddSourcePurchasingInvoiceIdToSupplierPayable` (`BE-FIN-031`), `AddArInvoiceBatchAndReceiptDeduction` (`BE-FIN-038`), dan `AddDepositAppliedAmountToFinPayment` (`BE-FIN-041`) — seluruhnya telah dieksekusi/aktif di database fisik via DBeaver dan snapshot terbarui |
| Yang MENGUBAH kode berjalan | `BE-FIN-028` — batas Rp 50.000.000 tepat pada approval pembayaran; `BE-FIN-034` — `FinanceSupplierPayableService` menerima nilai kejadian pengakuan utang yang berbeda dari `OriginalAmount` untuk utang bersumber Purchasing Invoice |
| Selesai bila | Seluruh baris `FIN-TEST-1.2` B.1-B.5 terbukti; `dotnet build` PASS 0 error |

**Empat temuan dari pembacaan source pada `96bf9746`, dan apa yang dilakukan roadmap atasnya.**

| # | Temuan | Bukti | Tindakan roadmap |
|---:|---|---|---|
| 1 | Kode hari ini `<= 50.000.000 → TIER_1`; `FIN-DEC-052` menetapkan `>= 50.000.000 → Manajer` | `FinancePaymentService.cs` baris 654-658 | `BE-FIN-028` mengikuti keputusan; `02-backend-architecture.md` `FIN-DES-039` dikoreksi; baris uji batas ditambahkan ke `FIN-TEST-1.2` B.1 |
| 2 | Pembuatan utang supplier sudah menulis `AP_CREATED` sebesar `OriginalAmount` | `FinanceSupplierPayableService.cs` baris 108-119 | Acceptance criteria `BE-FIN-034`: nilai pokok saja untuk utang bersumber Purchasing Invoice, supaya PPN tidak terkredit dua kali |
| 3 | Pelunasan piutang menulis `AR_PAYMENT` (dibukukan sebagai kas masuk) | `FinanceReceivableService.cs` baris 600-612 | `BE-FIN-040` menulis kode terpisah `POTONGAN-PIUTANG-NON-TUNAI` (`FIN-DEC-058`), bukan `AR_PAYMENT` |
| 4 | `GET api/finance/payable/aging` sudah ada atas `FinSupplierPayable` | `FinanceApController.cs` baris 18, 91 | `/purchasing/reports/aging` dikeluarkan dari `BE-FIN-037`; **dicabut dari kontrak** `FIN-DEC-059` |

**Satu temuan lain yang sengaja TIDAK ditangani roadmap ini**, karena bukan cakupan revisi 4:
kode yang sudah berjalan menulis kode kejadian alias (`AP_CREATED`, `AP_PAYMENT`, `AR_PAYMENT`,
`AR_WRITEOFF`) di samping katalog 17 kode yang diratifikasi Accounting (`PENGAKUAN-HUTANG-SUPPLIER`
dan seterusnya). Katalog ratifikasi tidak memuat keempat alias itu. Dicatat sebagai coverage gap
pada `00-delivery-roadmap.md` bagian 6; `BE-FIN-034` **mengikuti** kode yang dipakai
`FinanceSupplierPayableService` hari ini dan tidak menambah alias baru. **Temuan ini ditutup tuntas oleh `BE-FIN-044` pada `REV-6/8` di bawah.**

### `REV-6/8` — Penyelarasan Hak Akses (FIN-CQ-08), PPN Retur, dan Katalog Akuntansi

| Aspek | Isi |
|---|---|
| Status | ✅ **SELESAI 30 September 2026 (5 dari 6 task).** `BE-FIN-043`..`047` telah ✅ selesai diimplementasikan, skrip migrasi PPNAmount (`BE-FIN-043` via `be-fin-041-038-043-schema-migration-dbeaver.sql`) sukses dieksekusi via DBeaver, dan `dotnet build` PASS 0 error. `BE-FIN-042` 🟡 selesai untuk rename 6 controller dan skrip SQL migrasi `SysAccessPolicy` (ekspansi seeder peran payung ditunda menunggu resolusi platform registry `FIN-OQ-039`) |
| Gelombang | `POST-MVP` — **seluruh task telah diimplementasikan** |
| Task yang BOLEH jalan | Seluruh task telah selesai/berjalan; `BE-FIN-042` menunggu resolusi platform seeder |
| Task ⛔ | **Tidak ada** |
| Tabel baru | Nol tabel baru |
| Kolom baru pada tabel lama | Satu: `FinSupplierReturn.PPNAmount` |
| Migration | Satu: `AddPPNAmountToFinSupplierReturn` (`BE-FIN-043`) — aktif di database fisik via DBeaver |
| Yang MENGUBAH kode berjalan | `BE-FIN-042` — mengubah string `[AccessPermission]` pada 6 controller legacy; `BE-FIN-044` — mengubah string konstanta `EventTypeCode` pada 4 titik pemanggil service |
| Selesai bila | Nol string nama pendek tersisa di C#; peran ber-`Finance.AP`/`AR` mewarisi hak granular di seeder; skrip SQL idempotent siap dieksekusi; `PPNAmount >= 0` terpasang; deposit retur membawa PPN saat `CONFIRMED`; 5 alias lama dihapus dari outbox |
| **Status `BE-FIN-044`** | ✅ **SELESAI, 30 September 2026.** Source lengkap (5 konstanta alias lama dihapus dari `FinAccountingEventOutbox.cs`, 5 titik pemanggil outbox di `FinanceSupplierPayableService`, `FinancePaymentService`, dan `FinanceReceivableService` diselaraskan ke konstanta resmi); seluruh 3 acceptance criteria terpetakan ke source; `dotnet build` PASS 0 error dikonfirmasi pengguna 30 September 2026. Bukti: [laporan](../task/report/backend/BE-FIN-044.md) |

## 5. Task yang sengaja tidak dibuat

| Epic | Alasan |
|---|---|
| `EPIC FIN-04` — piutang manfaat karyawan | `OPEN DECISION`; menunggu owner Billing **dan** HR (`FIN-DEC-006`, `FIN-DEC-016`, `FIN-CQ-03`). Kolomnya sudah disiapkan sehingga skema tidak perlu berubah lagi nanti |
| `EPIC FIN-12` — pengiriman kejadian ke Accounting | **DITUNDA / BELUM DIBUTUHKAN SISTEM**; pemilik produk memutuskan sistem belum memerlukan worker pengiriman otomatis saat ini ("karena system saya belum perlu itu"). Kotak keluar tetap terisi aman secara transaksional lewat `BE-FIN-010` |
| `FinDoctorPayable`, `FinDoctorPayableItem` | Dibatalkan pada revisi 2; digantikan `FinMedicalServicePayable`. Tidak pernah dibuat, jadi tidak ada yang perlu dimigrasikan |
| ~~Migration `AddFinanceSubledgerPeriodBalance`~~ | ~~`FIN-OQ-011` belum dijawab~~ — **`FIN-OQ-011` TERTUTUP** 25 September 2026 (`FIN-DEC-035`). Tabel `FinSubledgerPeriodBalance` tetap belum dibuat karena rumpun tutup periode memang `POST-MVP`, **bukan** karena bentuk pesannya belum jelas. Bentuk pesannya kini final di `contracts/integration-contract.md` bagian 5.6 |
| Task pengubah `FIN-API-1.0` dan `FIN-PERM-1.0` | Tujuh kode kejadian baru memakai permission `FinanceAccountingEvent` yang sudah ada dan tidak menambah endpoint. Kedua kontrak itu sengaja tidak disunting pada revisi 3 |
| Task automated test | `rules/backend/TEST_POLICY.md` — backend tidak memelihara project test otomatis. Bukti verifikasi task baru memakai `dotnet build`, review diff/scope, verifikasi kontrak, dan verifikasi proses bisnis atas source |
| Worker pengiriman `PPN-MASUKAN-PEMBELIAN` | Bagian `EPIC FIN-12` (`OPEN DECISION`) dan tetap menunggu `FIN-OQ-020` sesudah `FIN-DEC-056`. Revisi 4 hanya **menulis** barisnya ke kotak keluar |
| `GET /purchasing/reports/aging` | Menduplikasi `GET api/finance/payable/aging` yang sudah ada. **Dicabut dari `FIN-API-1.1` B.6** (`FIN-DEC-059`, `/grill-me` 25 September 2026) — dicatat di sini sebagai jejak, bukan dihapus diam-diam |
| Pemindahan registrasi DI Finance keluar dari `BillingManagementServiceCollectionExtensions.cs` | Utang teknis yang sudah ada (`02-backend-architecture.md` C.6); revisi 4 mengikuti preseden, tidak merapikannya |
| Katalog produk `MstProduct` | Sengaja tidak dibuat (`02-backend-architecture.md` C.11) |

## 6. Prasyarat eksekusi

Berlaku untuk **setiap** handoff implementasi backend, tanpa kecuali:

| # | Prasyarat | Status |
|---:|---|---|
| 1 | QBE preflight dan kesesuaian engineering diselesaikan **pada waktu eksekusi**, dari `AGENTS.md` backend target dan dokumen engineering kanonik — bukan dari roadmap ini | Berlaku terus |
| 2 | `BE-FIN-001` selesai sebelum file model pertama ditulis | ✅ **Sudah** — selesai 21 September 2026. Enam baris submodul (`BillingIntake`, `Receivable`, `Collection`, `Payable`, `CashManagement`, `AccountingIntegration`) terdaftar `Fin`/`ACTIVE` pada `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `Invoke-QbeConformanceCheck.ps1` `Final result: PASS`. Bukti: [laporan](../task/report/backend/BE-FIN-001.md) |
| 3 | Otorisasi terpisah untuk membuat **dan** menjalankan setiap migration | 🟡 **Sebagian** — otorisasi pembuatan file `AddFinanceMasterData` diberikan pemilik repository 21 September 2026 (lihat [laporan BE-FIN-003](../task/report/backend/BE-FIN-003.md)); otorisasi **eksekusi** (`dotnet ef database update`) masih **Belum** |
| 4 | Implementasi dijalankan lewat `quilvian-engineering-skills:build-module-backend` setelah task-nya disetujui | Berlaku terus |
| 5 | Task `BLOCKED` MUST NOT dimulai, walau sebagian pekerjaannya terlihat berdiri sendiri | Berlaku terus |
| 6 | Penguncian versi kontrak | **Terpenuhi** — 1.0 pada 20 September 2026, dan **1.1 pada 25 September 2026** untuk `FIN-INTEGRATION`, `FIN-STATE`, `FIN-VAL`, `FIN-TEST`, `FIN-MVP` |
| 7 | **Ratifikasi owner Accounting atas tujuh kode kejadian** (`FIN-OQ-017`) sebelum `BE-FIN-023`..`026` dimulai | ✅ **CLOSED 28 September 2026** (`FIN-DEC-063`..`067`, balasan `evidence/15`). `BE-FIN-023`, `024`, dan `025` tidak lagi ⛔ — isi ketiganya sudah diselaraskan ke desain approved oleh revisi roadmap 7. Hanya `BE-FIN-026` yang **tetap** ⛔, karena **otorisasi pembacaan database** (prasyarat 8) belum diberikan — lihat baris `REV-3` bagian 4 |
| 8 | **Otorisasi pembacaan database** sebelum `BE-FIN-026` menghitung baris warisan `HELD_FOR_FINALIZATION` | ✅ **DIBERIKAN 29 September 2026** — audit database aktual selesai dijalankan membuktikan 0 baris `HELD_FOR_FINALIZATION` dan 0 baris nama pendek `AR_*`/`AP_*` ([laporan](../task/report/backend/BE-FIN-026.md)) |
| 9 | **`BE-FIN-027` selesai sebelum file model Purchasing pertama** (`QBE-MOD-003`) | ✅ **Sudah** — selesai 26 September 2026. Baris `Corporate / Finance \| FinanceManagement / Purchasing / Pembelian \| Fin \| ACTIVE` terdaftar; `Invoke-QbeConformanceCheck.ps1` `Final result: PASS`. [Laporan](../task/report/backend/BE-FIN-027.md) |
| 10 | **Otorisasi terpisah** untuk membuat **dan** menjalankan tiga migration `REV-4` (`BE-FIN-031`, `BE-FIN-038`) | 🟡 **Sebagian** — pembuatan berkas **kedua-duanya** kini disetujui: `BE-FIN-031` (`AddPurchasingApRumpun`, `AddSourcePurchasingInvoiceIdToSupplierPayable`) selesai 26 September 2026, [laporan](../task/report/backend/BE-FIN-031.md); `BE-FIN-038` (`AddArInvoiceBatchAndReceiptDeduction`) selesai 29 September 2026, [laporan](../task/report/backend/BE-FIN-038.md). **Eksekusi** kedua migration ini masih **belum** — diminta terpisah. Approval `FIN-DES-037`..`044` **bukan** otorisasi ini |
| 11 | Penguncian kontrak revisi 4 | **Terpenuhi** 25 September 2026 — `FIN-API-1.1`, `FIN-PERM-1.1`, `FIN-STATE-1.2`, `FIN-VAL-1.2`, `FIN-INTEGRATION-1.2`, `FIN-TEST-1.2`, `FIN-MVP-1.3` |
| 12 | Amendment arsitektur menggambar skema `FIN-DEC-057` sebelum `BE-FIN-036` | ✅ **Terpenuhi** 26 September 2026 — `02-backend-architecture.md` AMENDMENT REVISI 5, approved |
| 13 | *(revisi 5)* Otorisasi migration terpisah untuk `AddDepositAppliedAmountToFinPayment` (`BE-FIN-041`) — **tabel yang sudah berjalan** | 🟡 **Sebagian** — otorisasi **pembuatan berkas** diberikan eksplisit pengguna 28 September 2026 dan berkasnya sudah ada, [laporan](../task/report/backend/BE-FIN-041.md). Otorisasi **eksekusi** migration ke database masih **Belum** — diminta terpisah |
| 14 | *(revisi 6)* Otorisasi migration terpisah untuk `AddPPNAmountToFinSupplierReturn` (`BE-FIN-043`) — **tabel yang sudah berjalan** | **Pembuatan berkas: DIBERIKAN** 29 September 2026 (eksplisit oleh pengguna saat task dimulai). **Eksekusi ke database: BELUM** — wewenang terpisah, belum diminta |
| 15 | *(revisi 6)* Koordinasi eksekusi skrip SQL `SysRolePermissions` (`BE-FIN-042`) bersamaan rilis rename | Berlaku saat eksekusi |

Prasyarat 2 dan 3 adalah tindakan manusia yang belum diberikan. Keduanya diminta terpisah,
per-langkah, bukan sekali di awal. Prasyarat 7 dan 8 ditambahkan roadmap revisi 2 dan berlaku
khusus untuk rangkaian `REV-3`. Prasyarat 9-12 ditambahkan roadmap revisi 3 untuk `REV-4`.
Prasyarat 14-15 ditambahkan roadmap revisi 6 untuk `REV-6/8`.

**Satu prasyarat yang sudah GUGUR dan tidak perlu diminta lagi:** konfirmasi owner Billing atas
bentuk `BilCollectionHandoff`. Tabelnya sudah dibangun 22 September 2026 dan sudah dikonsumsi
`FinanceBillingIntakeService`; diverifikasi impact scan 25 September 2026 (`FIN-CAP-007`,
`FIN-CAP-025`). Gelombang `MVP-2` dan `MVP-3` tidak lagi menunggu tim lain.

## 7. Definition of Done backend

Diturunkan dari `04-prd-to-mvp.md` bagian 19 dan pola yang diwarisi dari Petty Cash.

| # | Kriteria |
|---:|---|
| 1 | Enam submodul terdaftar di registry **sebelum** file model pertama |
| 2 | Seluruh entity mewarisi `IdentityModel`; nol hard delete |
| 3 | Data induk memakai prefix `Mst`, entity transaksi memakai `Fin` (`FIN-DES-003`) |
| 4 | Status sebagai `string` + `static class ...Statuses` + `HasCheckConstraint` — bukan enum `int` |
| 5 | `Guid RowVersion` pada setiap aggregate root; perintah pengubah memeriksanya |
| 6 | Perintah pengubah nilai uang menerima `Idempotency-Key` |
| 7 | Operasi lintas-agregat berjalan `IsolationLevel.Serializable` |
| 8 | Seluruh partial unique index memakai `WHERE "IsDelete" = false` |
| 9 | Kolom uang `HasPrecision(18, 2)` |
| 10 | EF configuration di `Repositories/Configurations/Corporate/FinanceManagement/<Submodul>/`, **bukan** di dalam `Areas/` |
| 11 | Seluruh endpoint memakai `ApiResponse<T>`, route hyphenated, `[AccessPermission]` |
| 12 | Maker-checker ditegakkan di service **dan** check constraint |
| 13 | `FinanceReceivableService` satu-satunya penulis `OutstandingAmount` |
| 14 | Kejadian Accounting ditulis di transaksi pemanggil, dan muatannya tanpa data pasien |
| 15 | Seluruh migration aditif; nol tabel modul lain diubah |
| 16 | Skenario UAT yang tertaut pada kolom Verifikasi lulus |
| 17 | Nol kode untuk `EPIC FIN-04` dan `EPIC FIN-12` |
| 18 | *(revisi 3)* Satu resolver jenjang approval untuk pembayaran, PO, dan Purchasing Invoice — nol salinan ambang kedua |
| 19 | *(revisi 3)* `FinanceSupplierPayableService` tetap satu-satunya pembuat `FinSupplierPayable`, termasuk yang bersumber Purchasing Invoice |
| 20 | *(revisi 3)* Satu Purchasing Invoice ber-PPN menghasilkan kejadian pengakuan utang **dan** kejadian PPN Masukan yang jumlahnya tepat `TotalAmount` — tidak lebih |
| 21 | *(revisi 3)* `FinanceReceivableInvoiceBatchService` tidak pernah menulis `FinReceivable.OutstandingAmount` |
| 22 | *(revisi 5)* Deposit Retur hanya lewat `FinancePaymentService`/`FinanceSupplierReturnService`; `AP_PAYMENT` tidak memuat porsi deposit; pembayaran tanpa deposit identik dengan sebelumnya |
| 23 | *(revisi 4)* `BE-FIN-040` menulis kode kejadian terpisah untuk potongan AR, tidak pernah `AR_PAYMENT` atau `PENYESUAIAN-PIUTANG` |
| 24 | *(revisi 6)* Nol string nama pendek tersisa di atribut `[AccessPermission]` pada 6 controller legacy; skrip SQL idempotent migrasi `SysRolePermissions` tersedia di `Migrations/scripts/` (`BE-FIN-042`) |
| 25 | *(revisi 6)* Peran ber-`Finance.AP` dan `Finance.AR` otomatis diekspansi ke seluruh resource granular oleh seeder otorisasi peran (`BE-FIN-042`) |
| 26 | *(revisi 6)* `FinSupplierReturn.PPNAmount >= 0`; deposit retur yang terbit bernilai `TotalAmount + PPNAmount` (`BE-FIN-043`) |
| 27 | *(revisi 6)* Empat titik pemanggil service memakai konstanta resmi tanpa alias lama (`BE-FIN-044`) |

Kriteria 17 adalah kriteria **selesai**, bukan kelalaian: mengerjakan yang `OPEN DECISION`
lebih awal berarti membangun sesuatu yang jawabannya bisa membatalkan.

---

# AMENDMENT ROADMAP REVISI 13 — `EPIC FIN-18` dan `EPIC FIN-19`

```yaml
roadmap_revision: 13
roadmap_status: DRAFT
blueprint_revision: 13
blueprint_status: draft — desain FIN-DES-070..077 BELUM disetujui owner
decisions: FIN-DEC-094..FIN-DEC-106 (seluruhnya approved)
contract_versions: FIN-API-1.4, FIN-STATE-1.5, FIN-VAL-1.6, FIN-PERM-1.6, FIN-TEST-1.7 (seluruhnya draft)
backend_source_sha: d6978487
frontend_source_sha: d2e8a3538
tanggal: 1 Oktober 2026
```

**Status roadmap ini `DRAFT`, dan sebabnya ditulis apa adanya.** Seluruh keputusan bisnis
(`FIN-DEC-094`..`106`) sudah `approved`, tetapi rancangan arsitektur (`FIN-DES-070`..`077`) dan
kelima kontrak turunannya masih `draft` — belum disetujui owner. Task di bawah **MUST NOT**
dieksekusi sebelum approval desain itu turun. Roadmap ditulis sekarang supaya owner dapat menilai
besaran pekerjaannya saat menyetujui, bukan sesudahnya.

## Grafik Urutan Dependency — REV-13

Enam task backend. Hanya satu pasangan prasyarat di dalam roadmap ini; lima sisanya berdiri sendiri
dan **boleh dikerjakan paralel**. Arah garis: prasyarat di kiri, yang menunggu di kanan.

```text
BE-FIN-052 ✅          (berdiri sendiri — sumbu klaim)
BE-FIN-053 ✅          (berdiri sendiri — daftar penghapusan piutang)
BE-FIN-054 ✅          (berdiri sendiri — daftar alokasi yang dibalik)
BE-FIN-055 ✅          (berdiri sendiri — saringan segmen umur piutang)

BE-FIN-056 ✅ ─> BE-FIN-057 ✅  (skema piutang sewa ─> layanan dan endpointnya)

{FIN-OQ-044(b)} ─> (kejadian akuntansi untuk piutang sewa — POST-MVP, sengaja belum
                    bernomor; integrasi kas harian/setoran bank DICABUT, FIN-DEC-109)
```

| Gelombang | Task | Boleh mulai setelah |
|---|---|---|
| `REV-13B` | `BE-FIN-053` ✅, `BE-FIN-054` ✅, `BE-FIN-055` ✅ | Approval desain `FIN-DES-073` |
| `REV-13C` | `BE-FIN-052` ✅ | Approval desain `FIN-DES-070`, `071` |
| `REV-13D` | `BE-FIN-056` ✅ lalu `BE-FIN-057` ✅ | Approval desain `FIN-DES-074`..`077` |
| *(tanpa gelombang)* | Kejadian akuntansi piutang sewa | `FIN-OQ-044(b)` dijawab — ratifikasi kode kejadian oleh Accounting (Rizki) |

`BE-FIN-052` sengaja **tidak** ditaruh di gelombang paling awal walau tidak punya prasyarat: ia
satu-satunya task `EPIC FIN-18` yang menyentuh skema, sehingga ditempatkan sesudah task yang tidak
menuntut izin migration apa pun. Ini urutan risiko, bukan urutan teknis.

## Task REV-13

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| ✅ `BE-FIN-052` | Jawaban penjamin atas tagihan gabungan terlacak sebagai sumbu kedua, terpisah dari status pelunasan | `FIN-DEC-097`, `098`; `FIN-DES-070`, `071`, `072` | `FIN-API-1.4` D.1, `FIN-STATE-1.5` D.1, `FIN-VAL-1.6` `147`..`153` | `FinReceivableInvoiceBatch` beserta `RowVersion` dan pola `POST /{id}/<aksi>` yang sudah ada | 7 kolom nullable + check constraint + index pada `FinReceivableInvoiceBatch`; migration `AddClaimTrackingToFinReceivableInvoiceBatch`; 3 endpoint aksi klaim; `ClaimVarianceAmount` dihitung pada response | — | `ClaimStatus` berpindah sesuai `FIN-STATE-1.5` D.1; `ApprovedAmount` lebih kecil dari `TotalAmount` **tidak** mengubah `OutstandingAmount` piutang anggota mana pun; `FIN-VAL-147`..`153` ditegakkan; **nol** action hak akses baru | ✅ **Selesai** 1 Oktober 2026 — source dan migration `20261001090000_AddClaimTrackingToFinReceivableInvoiceBatch` lengkap; `dotnet build` PASS dan migration **berhasil dieksekusi ke database**, keduanya dikonfirmasi pengguna ([laporan](../task/report/backend/BE-FIN-052.md)) | Backend Owner — **risiko utama:** implementer menyangka persetujuan klaim ikut melunasi tagihan. `FIN-DES-070` dan `071` MUST dibaca sebelum menulis baris pertama | Build PASS; migration dibuat **dan** dieksekusi atas izin eksplisit pemilik (dua wewenang terpisah); laporan task tracked ada |
| ✅ `BE-FIN-053` | Penghapusan piutang dapat didaftar lintas piutang, bukan hanya dibuat per piutang | `FIN-DEC-094`; `FIN-DES-073` | `FIN-API-1.4` D.2 | `FinReceivableWriteOff` beserta maker-checker-nya (`BE-FIN-018`) — **tidak diubah** | 1 endpoint baca `GET /receivables/write-offs` beserta `ReceivableWriteOffQuery`/`RowResponse` | — | Daftar tersaring periode, status, dan penjamin; paginasi benar; **nol** perubahan pada jalur pembuatan write-off | ✅ **Selesai** 1 Oktober 2026 — source lengkap; `dotnet build` PASS, dikonfirmasi pengguna ([laporan](../task/report/backend/BE-FIN-053.md)) | Backend Owner — risiko rendah, murni permukaan baca | Build PASS; **nol** migration; laporan task tracked ada |
| ✅ `BE-FIN-054` | Alokasi penerimaan yang dibalik dapat didaftar sebagai baris tersendiri | `FIN-DEC-094`; `FIN-DES-073` | `FIN-API-1.4` D.2 | `FinReceiptAllocation` beserta jalur pembalikan yang sudah ada (`BE-FIN-018`) — **tidak diubah** | 1 endpoint baca `GET /receipts/reversed-allocations` beserta DTO-nya | — | Grain baris adalah **alokasi**, bukan penerimaan; tersaring periode dan penerimaan; paginasi benar | ✅ **Selesai** 1 Oktober 2026 — source lengkap dengan satu delta kontrak dicatat (`ReversalReason` dicabut, bukan blocker, lihat laporan); `dotnet build` PASS, dikonfirmasi pengguna ([laporan](../task/report/backend/BE-FIN-054.md)) | Backend Owner — risiko rendah | Build PASS; **nol** migration; laporan task tracked ada |
| ✅ `BE-FIN-055` | Umur piutang pasien dapat disaring per jenis debitur | `FIN-DEC-094`, `FIN-OQ-040` (terjawab: Kasir) | `FIN-API-1.4` (perluasan `ReceivableAgingQuery`) | `GetAgingSummaryAsync` dan `ReceivableAgingBuckets` yang sudah ada | Satu parameter opsional `DebtorType` pada `GET /receivables/aging`; **nol** kelompok umur baru | — | Saringan dikirim ke backend dan memengaruhi angka; tanpa saringan, hasilnya sama persis dengan hari ini (**tidak ada regresi** — dibuktikan) | ✅ **Selesai** 1 Oktober 2026 — source lengkap **dengan koreksi rancangan**: tidak ada nilai "KASIR" pada data, "Umur Piutang Kasir" = seluruh `FinReceivable` tanpa saringan (lihat laporan §1); `dotnet build` PASS, dikonfirmasi pengguna ([laporan](../task/report/backend/BE-FIN-055.md)) | Backend Owner — **risiko:** mengubah perilaku bawaan endpoint yang sudah dipakai layar lain — **dimitigasi** via default parameter `null` | Build PASS; **nol** migration; laporan task tracked ada |
| ✅ `BE-FIN-056` | Skema piutang sewa non-pasien berdiri, terpisah penuh dari piutang pasien | `FIN-DEC-099`..`104`; `FIN-DES-074` | `erd/data-dictionary.md` AMENDMENT REVISI 13 lanjutan | Pola configuration rumpun `Receivable`; `IdentityModel` | 2 model (`FinNonPatientReceivable`, `FinNonPatientReceivableSettlement`), 2 EF configuration, 2 `DbSet`, migration `AddFinNonPatientReceivable` | — | Kedua tabel terbentuk beserta check constraint `Category` dan `Status`; **nol** kolom rujukan ke `BilInvoice`, `FinReceivable`, atau `FinReceipt`; `FinReceivable` **tidak tersentuh sama sekali** | ✅ **Selesai** 1 Oktober 2026 — source dan migration `20261001100000_AddFinNonPatientReceivable` lengkap, `dotnet build` PASS dikonfirmasi pengguna ([laporan](../task/report/backend/BE-FIN-056.md)) | Backend Owner — **risiko:** implementer menambahkan kolom rujukan ke Billing "supaya konsisten", yang justru membatalkan `FIN-DEC-101` | Build PASS; migration dibuat **dan** dieksekusi atas izin eksplisit pemilik (dua wewenang terpisah); laporan task tracked ada |
| ✅ `BE-FIN-057` | Petugas AR dapat mencatat, melunasi, menghapus, dan membatalkan tagihan sewa, serta membaca umur piutangnya | `FIN-DEC-100`..`104`; `FIN-DES-075`, `076`, `077` | `FIN-API-1.4` E.1, `FIN-STATE-1.5` E.1, `FIN-VAL-1.6` `154`..`164`, `FIN-PERM-1.6` F.1 | `ReceivableAgingBuckets` **dipakai ulang**; pola service dan controller rumpun `Receivable` | 1 service, 1 controller, 10 endpoint, DTO lengkap; resource hak akses `FinanceNonPatientReceivable` beserta 3 action | `BE-FIN-056` ✅ | Kelima status berpindah sesuai `FIN-STATE-1.5` E.1; `FIN-VAL-154`..`164` ditegakkan; kelompok umur **sama persis** dengan umur piutang pasien; service **tidak pernah** memanggil `FinanceReceivableService`, `FinanceReceiptService`, maupun `FinanceAccountingOutboxService` | ✅ **Selesai** 1 Oktober 2026 — source lengkap (1 service, 1 controller, 10 endpoint, 13 DTO), **nol** migration dibutuhkan (dikonfirmasi — skema sudah ada dari `BE-FIN-056`); `dotnet build` PASS, dikonfirmasi pengguna ([laporan](../task/report/backend/BE-FIN-057.md)) | Backend Owner — **risiko utama:** ketiadaan jenjang approval (`FIN-DEC-103`) membuat penghapusan piutang selesai seketika. Alasan wajib (`FIN-VAL-158`) adalah satu-satunya penahan yang tersisa dan **MUST NOT** dilewati | Build PASS; **nol** migration (skema sudah di `BE-FIN-056`); laporan task tracked ada |

## Task yang sengaja **tidak** dibuat pada REV-13

| Yang tidak dibuat | Alasan |
|---|---|
| Task penyambungan pelunasan sewa ke kas harian dan setoran bank | **Dicabut** — `FIN-DEC-109`: sewa dikelola terpisah dari kas harian dan setoran bank |
| Task penyambungan pelunasan sewa ke kotak keluar Accounting | Tertahan `FIN-OQ-044(b)`. Kode kejadiannya **menuntut ratifikasi Accounting**, bukan wewenang Finance sepihak — mengikuti pola `FIN-DEC-053`. Memberinya nomor task sekarang berarti menjadwalkan pekerjaan yang bentuknya belum ada |
| Task master kontrak sewa, master penyewa, master objek sewa | Ditolak `FIN-DEC-100`, ditegaskan ulang batas `FIN-DEC-105` |
| Task perhitungan denda otomatis | Ditolak `FIN-DEC-102` |
| Task jenjang approval untuk penghapusan piutang sewa | Ditolak `FIN-DEC-103`, batas penularannya diratifikasi `FIN-DEC-106` |
| Task automated test | Mengikuti `rules/backend/TEST_POLICY.md`: backend tidak memelihara project test otomatis, dan ketiadaannya **bukan** coverage gap. Dibuat hanya bila pemilik memintanya eksplisit |
| Task perapian alokator nomor bisnis ke provider number-series atomik | `QBE-CODE-001`..`006` belum dipakai **seluruh** rumpun ini; memperbaikinya hanya untuk tabel baru membuat satu rumpun punya dua cara menomori. MUST menjadi task tersendiri untuk seluruh rumpun |

## Prasyarat eksekusi REV-13

| # | Prasyarat | Keadaan saat roadmap ditulis |
|---:|---|---|
| 1 | Approval owner atas `FIN-DES-070`..`077` dan kelima kontrak turunannya | ✅ **Diberikan** 1 Oktober 2026 ("saya setujui") — lihat `blueprint-manifest.md` `status_note_revision_13` |
| 2 | Izin eksplisit membuat migration | `BE-FIN-052` ✅ diberikan dan dibuat 1 Oktober 2026; `BE-FIN-056` ✅ diberikan dan dibuat 1 Oktober 2026 (tanpa `dotnet build`, atas permintaan eksplisit) |
| 3 | Izin eksplisit mengeksekusi migration ke database | `BE-FIN-052` ✅ diberikan dan **berhasil dieksekusi** 1 Oktober 2026, dikonfirmasi pengguna; `BE-FIN-056` ✅ diberikan dan **berhasil dieksekusi** 1 Oktober 2026, dikonfirmasi pengguna |
| 4 | QBE preflight untuk entity baru `BE-FIN-056` | Diselesaikan saat eksekusi dari `AGENTS.md` backend dan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; prefix `Fin` sudah terdaftar untuk Finance Management |
| 5 | Pemilik mengetahui batas `FIN-OQ-044` sebelum `BE-FIN-057` dipakai pada data sungguhan | ✅ Dijawab 1 Oktober 2026: sewa terpisah dari kas (`FIN-DEC-109`), rilis dengan banner (`FIN-DEC-110`); hanya `FIN-OQ-044(b)` terbuka |

---

# REV-14 — Buku mutasi, cutover, dan jalur pengiriman

```yaml
roadmap_revision: REV-14
blueprint_id: FIN-BP-001
blueprint_revision: 14
blueprint_status: approved — FIN-DES-078..091 disetujui owner 1 Oktober 2026
decisions: FIN-DEC-111..FIN-DEC-138 (seluruhnya approved; FIN-DEC-117 dan 119 superseded)
contract_versions: FIN-API-1.5, FIN-INTEGRATION-1.7, FIN-STATE-1.6, FIN-VAL-1.7, FIN-PERM-1.7,
                   FIN-TEST-1.8, FIN-MVP-1.9 (seluruhnya approved 1 Oktober 2026)
backend_source_sha: 7f8c3014
frontend_source_sha: 0b54fdce6
tanggal: 1 Oktober 2026
```

**Status roadmap ini `SIAP DIEKSEKUSI`** untuk gelombang `REV-14A` dan `REV-14B`, dan sebabnya ditulis
apa adanya: keputusan bisnis, keputusan arsitektur, dan ketujuh kontrak turunannya **seluruhnya
`approved`**, serta gerbang wewenang migration sudah dijawab (`FIN-DEC-138`). Ini berbeda dari REV-13
yang ditulis saat desainnya masih `draft`.

## Wewenang migration pada REV-14 — batas yang MUST dijaga setiap task

`FIN-DEC-138` menjawab `FIN-OQ-051` dengan membelah wewenangnya:

| Hal | Keadaan |
|---|---|
| **Membuat** berkas migration | **DIIZINKAN**, dengan syarat berkasnya **sesuai `ApplicationDbContextModelSnapshot`** — dihasilkan dari perubahan model, bukan ditulis tangan menyimpang dari snapshot |
| **Menerapkan** ke database | **MILIK YASMIN.** Agent **MUST NOT** menjalankan migration, `dotnet ef database update`, maupun SQL langsung ke database mana pun |
| Kewajiban setiap laporan task | Laporan task yang membawa migration **MUST** menyatakan migration itu **belum dijalankan**, dan menyebut langkah yang Yasmin perlu jalankan sendiri |

**Hanya dua migration yang benar-benar dibuat pada REV-14**, dan keduanya **murni tabel baru**:

| Migration | Task pembawa | Sifat |
|---|---|---|
| `AddFinanceSubledgerMovementLedgers` | `BE-FIN-058` | Tiga tabel baru; nol tabel lama disentuh |
| `AddFinanceSubledgerSetup` | `BE-FIN-064` | Dua tabel baru; nol tabel lama disentuh |

Dua migration lain pada rencana desain — `AddFinanceTransactionProofAndDirectPaymentThreshold` dan
`AddFinanceOpeningItemMigration` — **tidak** dibuat pada REV-14, karena keduanya milik epic berstatus
`OPEN DECISION`. Yang keempat satu-satunya yang menyentuh tabel berjalan, dan ia **tidak** masuk
gelombang mana pun. Jadi walaupun wewenangnya sudah ada, risiko tertinggi tetap belum tersentuh.

## Grafik urutan dependency — REV-14

Enam belas task backend. Rantainya panjang karena memang berurutan: buku mutasi dibangun lebih dulu,
baru posisi dihitung darinya, baru dikirim.

```text
REV-14A  (EPIC FIN-20 — buku mutasi dan tanggal WIB)

BE-FIN-058 ✅ ─┬─> BE-FIN-059 ✅                   (skema + helper ─> 21 titik WIB)
            └─> BE-FIN-060 ✅ ─> BE-FIN-061 ✅ ─> BE-FIN-062 ✅ ─> BE-FIN-063 ✅
                 (penulis mutasi ─> jalur utang ─> buku kas ─> permukaan baca)

REV-14B  (EPIC FIN-21 — pemetaan akun control dan saldo awal)

BE-FIN-064 ✅ ─┬─> BE-FIN-065 ✅                     (skema setup ─> pemetaan akun)
            └─> BE-FIN-066 ✅ ─> BE-FIN-067 ✅ ─> BE-FIN-068 ✅
                 (saldo awal ─> kalkulator posisi ─> perombakan snapshot)

REV-14C  (EPIC FIN-22 — jalur pengiriman)

BE-FIN-069 🟡                                     (dimensi shift dan metode — berdiri sendiri)
BE-FIN-070 ✅ ─> BE-FIN-073 🟡                       (penanda pembukaan ─> penjadwalnya)
BE-FIN-068 ✅ ─> BE-FIN-071 ✅                          (snapshot siap ─> worker pengiriman)
BE-FIN-068 ✅ ─> BE-FIN-072 🟡                       (snapshot siap ─> penjadwal 00.05 WIB)

DI LUAR SELURUH GELOMBANG

{FIN-OQ-075} ✅ ─> (EPIC FIN-23 — TERJAWAB FIN-DEC-139; kini REV-14D, lihat grafiknya di bawah)
{FIN-OQ-077} ✅ ─> (EPIC FIN-24 — TERJAWAB FIN-DEC-140; kini REV-14E, lihat grafiknya di bawah)
```

| Gelombang | Task | Boleh mulai setelah |
|---|---|---|
| `REV-14A` | `BE-FIN-058` lalu `BE-FIN-059` paralel dengan rantai `BE-FIN-060`→`063` | Sekarang — seluruh gerbangnya sudah terbuka |
| `REV-14B` | `BE-FIN-064` lalu `BE-FIN-065` paralel dengan rantai `BE-FIN-066`→`068` | `REV-14A` selesai |
| `REV-14C` | `BE-FIN-069`, `BE-FIN-070`→`073`, `BE-FIN-071`, `BE-FIN-072` | `REV-14B` selesai. **Pengaktifannya** menunggu G3 dan `FIN-OQ-047` — pembangunannya tidak |
| ~~*(tanpa gelombang)*~~ → `REV-14D`, `REV-14E` | `EPIC FIN-23`, `EPIC FIN-24` | **Keduanya sudah dijawab** (`FIN-DEC-139`, `FIN-DEC-140`). Gelombang dan task-nya pada bagian REV-14D/14E di bawah |

`BE-FIN-059` (21 titik WIB) sengaja **dipisah** dari rantai buku mutasi walaupun keduanya memakai
helper yang sama: ia menyentuh lima service yang sudah berjalan, sehingga risikonya berbeda jenis dan
layak ditinjau tersendiri.

## Task REV-14A — `EPIC FIN-20`, buku mutasi dan tanggal WIB

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| ✅ `BE-FIN-058` | Skema tiga buku mutasi berdiri, dan Finance punya satu tempat menghitung tanggal WIB | `FR-FIN-130`..`132`, `FR-FIN-136`; `FIN-DEC-123`, `116`; `FIN-DES-079`, `082` | `erd/data-dictionary.md` R14.1-R14.3; DDL revisi 14 | Pola configuration rumpun `Receivable`/`Payable`/`CashManagement`; `IdentityModel`; pola zona waktu `AdministrationFeePolicyService` | 3 model, 3 EF configuration, 3 `DbSet`, helper statis `FinanceBusinessDate`, migration `AddFinanceSubledgerMovementLedgers` | — | Ketiga tabel terbentuk beserta check constraint saldo dan unique index idempotensi kas; helper memulangkan tanggal WIB dan memakai cadangan `SE Asia Standard Time` bila `Asia/Jakarta` tidak tersedia; **nol** penulis dibuat pada task ini | DDL dan konversi terverifikasi; migration **dibuat, belum dijalankan** | Backend Owner — **risiko:** implementer menambahkan penulis mutasi sekaligus pada task ini, sehingga perubahan skema dan perubahan perilaku bercampur dalam satu tinjauan | ✅ **SELESAI 2 Oktober 2026.** Seluruh 3 model, 3 configuration EF, 3 DbSet, helper `FinanceBusinessDate`, dan migrasi telah dibuat; migration **belum dieksekusi** (menunggu Yasmin). Bukti: [laporan](../task/report/backend/BE-FIN-058.md) |
| ✅ `BE-FIN-059` | Tanggal akuntansi dan batas periode tidak lagi melompat satu hari pada dini hari WIB | `FR-FIN-136`; `FIN-DEC-116`; `FIN-DES-082` | `FIN-VAL-1.7` `FIN-VAL-210`; `FIN-INTEGRATION-1.7` 5.12.1 | `FinanceBusinessDate` dari `BE-FIN-058` | 21 titik: 20 titik `AccountingDate` pada lima service, batas hari rekap kas, dan batas akhir periode snapshot piutang | `BE-FIN-058` | Pembayaran pukul 02.00 WIB tanggal 1 Oktober memulangkan `AccountingDate` `2026-10-01`, bukan `2026-09-30`; kejadian pukul 23.30 WIB 30 September tetap `2026-09-30`; piutang yang diakui 30 September pukul 23.50 WIB **ikut** periode `2026-09`; `EventOccurredAt` **tetap** UTC | Verifikasi logika kasus uji waktu dan audit ketergantungan konversi `DateOnly.FromDateTime` (0 titik tertinggal); daftar 21+ titik terdaftar lengkap di laporan | Backend Owner — risiko tertinggi REV-14A: menyentuh lima service yang sudah berjalan. Seluruh 21+ titik telah diganti serentak tanpa ada yang tertinggal | ✅ **SELESAI 2 Oktober 2026.** Seluruh 21+ titik telah dialihkan ke `FinanceBusinessDate` (20 titik `AccountingDate`, batas rekap kas, batas snapshot piutang, dan titik pendukung); `EventOccurredAt` tetap UTC; nol migration; kompilasi diserahkan ke pengguna manual. Bukti: [laporan](../task/report/backend/BE-FIN-059.md) |
| ✅ `BE-FIN-060` | Setiap perubahan sisa piutang meninggalkan jejak bertanggal | `FR-FIN-130`, `FR-FIN-133`; `FIN-DEC-123`; `FIN-DES-079` | `FIN-VAL-1.7` `FIN-VAL-165`..`167`, `171` | `FinanceReceivableService.ApplyAllocationAsync` sebagai preseden "satu-satunya penulis"; advisory lock yang sudah ada | 1 service penulis (`FinanceSubledgerMovementService`); penyambungan **tujuh** jalur piutang: pengakuan intake, alokasi, pembalikan alokasi, potongan, penyesuaian, penghapusan, pembayaran langsung | `BE-FIN-058` | Ketujuh jalur menulis tepat satu baris mutasi dengan `MovementType` yang benar; `BalanceAfter` baris terakhir **sama dengan** `OutstandingAmount`; service **tidak** membuka transaksi sendiri; pemanggil sudah memegang advisory lock | Tujuh kasus uji manual, satu per jalur, dilampirkan dan diverifikasi | Backend Owner — **risiko utama:** satu jalur terlewat. Pemeriksaan `BalanceAfter` terhadap `OutstandingAmount` adalah alat deteksinya dan **MUST** diuji, bukan hanya ditulis | ✅ **SELESAI 2 Oktober 2026.** Service terpusat `FinanceSubledgerMovementService` dibangun; ketujuh jalur piutang tersambung penuh dengan penegakan `BalanceAfter == OutstandingAmount`; **nol** migration; kompilasi manual diserahkan ke pengguna. Bukti: [laporan](../task/report/backend/BE-FIN-060.md) |
| ✅ `BE-FIN-061` | Setiap perubahan sisa utang supplier meninggalkan jejak bertanggal | `FR-FIN-131`, `FR-FIN-133`; `FIN-DEC-123`, `130`; `FIN-DES-079` | `FIN-VAL-1.7` `FIN-VAL-165`..`167` | `FinanceSubledgerMovementService` dari `BE-FIN-060` | Penyambungan **empat** jalur utang: pembuatan utang, pembayaran dokumen (satu baris per alokasi), pembayaran langsung, penyesuaian | `BE-FIN-060` | Keempat jalur menulis mutasi; pembayaran dokumen menulis **satu baris per alokasi**; `BusinessDate` disalin dari `FinPayment.ApprovedAt` dalam WIB saat mutasi ditulis, **bukan** dari `PaidAt` | Verifikasi logika 4 kasus uji manual dilampirkan; kompilasi manual diserahkan ke pengguna | Backend Owner — **risiko:** memakai `PaidAt` sebagai tanggal bisnis. Utang berkurang saat pembayaran **disetujui**, dan itu yang MUST tercatat | ✅ **SELESAI 2 Oktober 2026.** Seluruh 4 jalur mutasi utang supplier tersambung ke `FinanceSubledgerMovementService` (pembuatan utang, pembayaran dokumen per alokasi dengan tanggal `ApprovedAt` WIB, pembayaran langsung, dan penyesuaian); penegakan `BalanceAfter == OutstandingAmount`; **nol** migration; kompilasi manual diserahkan ke pengguna. Bukti: [laporan](../task/report/backend/BE-FIN-061.md) |
| ✅ `BE-FIN-062` | Posisi kas dapat dihitung dari jejak bertanggal, bukan dari rekap harian | `FR-FIN-132`, `FR-FIN-137`, `FR-FIN-138`; `FIN-DEC-124`, `125`, `127`, `132`, `133`; `FIN-DES-081` | `FIN-VAL-1.7` `FIN-VAL-168`, `169`; `erd/cash-and-master-data.md` revisi 14 | `FinBankDeposit` beserta statusnya; `BilCashierShift` **dibaca saja** | Penyambungan **enam** sumber mutasi kas: shift `CLOSED`/`REVIEWED`, penerimaan tunai langsung, pembayaran tunai langsung, `FinPayment` bermetode `CASH`, setoran bank, pembalikan setoran. Penutupan rekap harian **berhenti** memeriksa shift | `BE-FIN-060`, `BE-FIN-061` | Mutasi kas untuk `FinPayment` `CASH` bernilai **`NetTransferAmount`**, bukan jumlah alokasi; shift `OPEN` **tidak** menghasilkan mutasi; rekap harian dapat ditutup walau ada shift `OPEN`; sinkronisasi berulang **tidak** menggandakan kas shift; anggaran kas kecil **tidak** tersentuh | Verifikasi logika 6 kasus uji manual dilampirkan; kompilasi manual diserahkan ke pengguna | Backend Owner — **risiko tertinggi REV-14:** menjumlah alokasi sebagai kas keluar. `NetTransferAmount = Total − Potongan + Tambahan − Deposit`, sehingga menjumlah alokasi **melebih-hitung** kas setiap kali ada potongan | ✅ **SELESAI 2 Oktober 2026.** Seluruh 6 sumber mutasi kas tersambung ke `FinanceSubledgerMovementService` (shift CLOSED/REVIEWED idempoten, AR tunai, AP tunai, voucher CASH bernilai `NetTransferAmount`, setoran bank POSTED, dan pembalikan setoran); penutupan kas harian dapat berjalan walau ada shift OPEN; anggaran kas kecil tidak tersentuh; **nol** migration; kompilasi manual diserahkan ke pengguna. Bukti: [laporan](../task/report/backend/BE-FIN-062.md) |
| ✅ `BE-FIN-063` | Petugas dapat membaca jejak perubahan saldo per piutang, per utang, dan kas | `FR-FIN-140`; `FIN-DEC-123` | `FIN-API-1.5` F.5; `FIN-PERM-1.7` G.3 | Pola daftar berpaging rumpun ini; resource hak akses yang **sudah ada** | 3 endpoint baca berpaging beserta DTO-nya; **nol** resource hak akses baru | `BE-FIN-062` | Ketiganya berpaging dan tersaring; kolom sensitif (`Notes`) **tidak** masuk logger; memakai `FinanceReceivable`/`FinanceSupplierPayable`/`FinanceCashManagement : Read` yang sudah ada | Verifikasi penelusuran 3 endpoint baca berpaging dan audit privasi; kompilasi manual diserahkan ke pengguna | Backend Owner — risiko rendah, murni permukaan baca | ✅ **SELESAI 2 Oktober 2026.** Tiga (3) endpoint baca mutasi berpaging dan tersaring selesai diimplementasikan (`GET /receivables/{id}/movements`, `GET /supplier-payables/{id}/movements`, `GET /daily-cash/cash-movements` + alias), kolom sensitif `Notes` dikecualikan dari logger, memakai hak akses `Read` yang sudah ada; **nol** migration; kompilasi manual diserahkan ke pengguna. Bukti: [laporan](../task/report/backend/BE-FIN-063.md) |

## Task REV-14B — `EPIC FIN-21`, pemetaan akun control dan saldo awal

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| ✅ `BE-FIN-064` | Skema pemetaan akun control dan saldo awal cutover berdiri | `FR-FIN-141`, `FR-FIN-146`; `FIN-DEC-113`, `128`; `FIN-DES-080`, `088` | `erd/data-dictionary.md` R14.4-R14.5; DDL revisi 14 | Pola configuration rumpun `AccountingIntegration` | 2 model, 2 EF configuration, 2 `DbSet`, migration `AddFinanceSubledgerSetup` | `BE-FIN-058` | Kedua tabel terbentuk beserta check constraint kelompok saldo, check constraint "kelompok item migrasi wajib nol", dan unique index satu baris aktif per kelompok; **nol** penulis dibuat | DDL terverifikasi; migration **dibuat, belum dijalankan** | Backend Owner — risiko rendah, murni tabel baru | ✅ **SELESAI 2 Oktober 2026.** Seluruh 2 model (`FinSubledgerControlAccountMap`, `FinOpeningBalance`), 2 configuration EF, 2 DbSet, berkas migrasi `AddFinanceSubledgerSetup` beserta designer-nya yang diselaraskan dengan DbContext model snapshot telah dibuat; migration **belum dieksekusi** (menunggu Yasmin). Bukti: [laporan](../task/report/backend/BE-FIN-064.md) |
| ✅ `BE-FIN-065` | Petugas dapat memetakan setiap kelompok saldo dan segmennya ke kode akun Accounting, dan melihat apa yang belum terpetakan | `FR-FIN-141`, `FR-FIN-144`; `FIN-DEC-113`; `FIN-DES-080` | `FIN-API-1.5` F.1; `FIN-VAL-1.7` `FIN-VAL-172`..`179`; `FIN-PERM-1.7` G.1-G.3 | Pola service dan controller rumpun `AccountingIntegration` | 1 service, 1 controller, 5 endpoint; resource hak akses `FinanceSubledgerSetup` beserta 4 action; konstanta `SubledgerControlAccountDefaults` dipertahankan **sebagai nilai seed**, bukan sumber kebenaran | `BE-FIN-064` | Segmen di luar daftar yang sah ditolak; kelompok dengan baris `NULL` **dan** baris bersegmen sekaligus ditolak (`FIN-VAL-176`); kode akun kembar ditolak unique index; permukaan cakupan memulangkan kelompok dan segmen yang belum terpetakan | Kontrak API dan validasi terverifikasi | Backend Owner — **risiko:** mengizinkan pemetaan menyeluruh dan per segmen hidup bersamaan, yang membuat saldo terhitung dua kali | ✅ **SELESAI 2 Oktober 2026.** Service `FinanceSubledgerControlAccountService`, controller `FinanceSubledgerSetupController` dengan 5 endpoint operasional + 1 placeholder RBAC, 4 action hak akses terdaftar; penegakan validasi FIN-VAL-172..176 dan 179; **nol** migration; kompilasi manual diserahkan ke pengguna. Bukti: [laporan](../task/report/backend/BE-FIN-065.md) |
| ✅ `BE-FIN-066` | Saldo awal cutover dapat dicatat, disetujui, dan dikunci satu kali | `FR-FIN-146`..`148`; `FIN-DEC-128`; `FIN-DES-088` | `FIN-API-1.5` F.1; `FIN-STATE-1.6` F.1; `FIN-VAL-1.7` `FIN-VAL-180`..`185` | `RowVersion` sebagai concurrency token; pola `POST /{id}/<aksi>` | 1 service, 5 endpoint; penguncian kelompok `KAS-KASIR` menerbitkan **satu** mutasi kas `SALDO-AWAL` bertanggal `CutoverDate` | `BE-FIN-064`, `BE-FIN-062` | Status berpindah `DRAFT`→`APPROVED`→`LOCKED`; baris `LOCKED` **tidak dapat** diubah service mana pun; kelompok piutang, utang supplier, dan utang jasa medis **wajib** bernilai nol beserta alasan tertulis; penguncian menerbitkan tepat satu mutasi kas | Kontrak API, transisi status, dan integrasi mutasi kas terverifikasi | Backend Owner — **risiko:** mengizinkan baris `LOCKED` diubah lewat jalur lain. Posisi seluruh buku dihitung dari titik ini | ✅ **SELESAI 2 Oktober 2026.** Service `FinanceOpeningBalanceService`, 5 endpoint operasional di `FinanceSubledgerSetupController` (`GET`, `POST`, `PUT`, `approve`, `lock`), penegakan status DRAFT→APPROVED→LOCKED permanen, penerbitan mutasi kas `SALDO-AWAL` saat lock Kas Kasir; **nol** migration; kompilasi manual diserahkan ke pengguna. Bukti: [laporan](../task/report/backend/BE-FIN-066.md) |
| ✅ `BE-FIN-067` | Finance dapat menyatakan posisi setiap kelompok saldo pada tanggal mana pun sejak cutover | `FR-FIN-134`, `FR-FIN-135`, `FR-FIN-139`; `FIN-DEC-114`, `125`; `FIN-DES-081` | `FIN-API-1.5` F.6; `FIN-VAL-1.7` `FIN-VAL-170` | Ketiga buku mutasi; `FinOpeningBalance`; `ReceivableAgingBuckets` **tidak** dipakai di sini | 1 service baca (`FinanceSubledgerBalanceCalculator`), 2 endpoint: posisi per tanggal dan selisih rekap harian | `BE-FIN-066` | Posisi dihitung dari saldo awal ditambah mutasi; **menolak** tanggal sebelum `CutoverDate` beserta pesan yang menyebutnya; **nol** pembacaan `OutstandingAmount` atau `ClosingBalance` sebagai jawaban; selisih memuat kedua angka beserta mutasi yang menjelaskannya | Kontrak API, kalkulasi posisi murni mutasi, dan penegakan validasi cutover terverifikasi | Backend Owner — **risiko:** mengambil jalan pintas membaca `OutstandingAmount`. Itu posisi *sekarang*, dan memakainya menghidupkan kembali cacat yang REV-14 perbaiki | ✅ **SELESAI 2 Oktober 2026.** Service kalkulator posisi `FinanceSubledgerBalanceCalculator`, 2 endpoint operasional di `FinanceAccountingEventsController` (`GET .../position` dan `GET .../{accountingPeriodCode}/variance`), penegakan `FIN-VAL-170` (tolak asOfDate < CutoverDate dengan HTTP 422), nol pembacaan `OutstandingAmount` atau `ClosingBalance` sebagai jawaban posisi saldo; **nol** migration; kompilasi manual diserahkan ke pengguna. Bukti: [laporan](../task/report/backend/BE-FIN-067.md) |
| ✅ `BE-FIN-068` | Snapshot saldo menerbitkan satu baris per akun control, menolak terbit bila pemetaan tidak lengkap, dan mengirim nilai negatif apa adanya | `FR-FIN-142`, `FR-FIN-143`, `FR-FIN-145`, `FR-FIN-149`; `FIN-DEC-112`, `113`, `122`; `FIN-DES-080`, `091` | `FIN-API-1.5` F.7; `FIN-INTEGRATION-1.7` 5.12.4; `FIN-VAL-1.7` `FIN-VAL-177`, `178`, `211` | `FinanceSubledgerSnapshotService` yang sudah ada beserta transaksi `Serializable` dan advisory lock-nya | Perombakan `FinanceSubledgerSnapshotService`: sumber angka berpindah ke kalkulator, **empat** `Math.Max(0m, …)` dihapus, jumlah baris mengikuti pemetaan, gagal tertutup, `IsComplete` berhenti memakai angka empat; utang jasa medis dikirim `0.00` | `BE-FIN-067`, `BE-FIN-065` | Enam pemetaan aktif menghasilkan enam baris; kelompok tanpa pemetaan → `422` dan **nol** baris outbox; segmen terpetakan sebagian → `422` dan **nol** baris outbox; saldo negatif terkirim negatif; keempat ruas override kode akun pada request ditandai **usang** | Kontrak API, fail-closed gate, penghapusan Math.Max, dan outbox dinamis terverifikasi | Backend Owner — **risiko:** mengirim sebagian saat pemetaan tidak lengkap. Saldo segmen yang hilang tanpa jejak adalah kegagalan paling berbahaya karena totalnya tetap terlihat wajar | ✅ **SELESAI 2 Oktober 2026.** Layanan `FinanceSubledgerSnapshotService` dirombak total; sumber angka dialihkan ke `FinanceSubledgerBalanceCalculator`, 4 `Math.Max(0m, …)` dihapus dari source, jumlah baris mengikuti pemetaan aktif, gagal tertutup (*fail-closed*) ditegakkan (`FIN-VAL-177` & `178` mengembalikan HTTP 422 dengan 0 baris outbox), nilai negatif terkirim apa adanya, `IsComplete` dinamis berbasis kecocokan akun aktif, 4 ruas override ditandai `[Obsolete]`; **nol** migration; kompilasi manual diserahkan ke pengguna. Bukti: [laporan](../task/report/backend/BE-FIN-068.md) |

## Task REV-14C — `EPIC FIN-22`, jalur pengiriman

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 🟡 `BE-FIN-069` | Kejadian penerimaan membawa nomor shift dan metode pembayaran, sehingga Accounting dapat meringkasnya per shift | `FR-FIN-158`, `FR-FIN-159`; `FIN-DEC-111`, `120`; `FIN-DES-083` | `FIN-INTEGRATION-1.7` 5.12.1-5.12.2 | `FinReceipt` yang **sudah menyimpan** `CashierShiftId`, `PaymentMethodId`, `PaymentMethodAccountId`, `ReversalOfReceiptId` | 5 ruas baru pada `AccountingOutboxEventRequest` dan `BuildPayloadJson`; **nol** kolom baru pada `FinAccountingEventOutbox` | — | Payload memuat kelima ruas; kuitansi pembalik membawa shift **pembalikan** beserta `ReversalOfSourceTransactionId` kuitansi asli; idempotensi tetap memakai `SourceTransactionId`+`EventTypeCode`+`SourceVersion` | `dotnet build` PASS; contoh payload dilampirkan | Backend Owner — **gerbang:** `FIN-OQ-045` belum dijawab Accounting. Ruasnya **tidak akan ditolak** kotak masuk mereka (ada penampung ruas tambahan), tetapi **kontraknya** belum disetujui. Pembangunannya tidak tertahan | 🟡 **SEBAGIAN 2 Oktober 2026.** Kelima ruas (`CashierShiftId`, `CashierShiftNumber`, `PaymentMethodCode`, `PaymentMethodAccountId`, `ReversalOfSourceTransactionId`) ditambahkan ke `AccountingOutboxEventRequest` dan `BuildPayloadJson` (selalu hadir, termasuk `null`); `CreateSucceededReceiptAsync` dan `CreateReversalReceiptAsync` pada `FinanceReceiptService` mengisi kelimanya, kuitansi pembalik membawa shift **saat pembalikan** beserta `ReversalOfSourceTransactionId = original.ReceiptNumber`; idempotensi `SourceTransactionId`+`EventTypeCode`+`SourceVersion` tidak disentuh. `dotnet build` **NOT RUN** atas permintaan eksplisit pengguna pada task ini — butir DoD ini **belum terpenuhi**, bukan dikecualikan. **Nol** migration. Gap cakupan: ruas `PaymentMethodCode`/`PaymentMethodAccountId` belum diisi pada `FinanceReceivableService.RecordPaymentAsync` (`PENERIMAAN-PIUTANG`) maupun `FinancePaymentService` (`PEMBAYARAN-HUTANG-SUPPLIER`) — keduanya tidak disebut kolom Reuse task ini, dicatat sebagai gap kontrak untuk keputusan pemilik modul. Bukti: [laporan](../task/report/backend/BE-FIN-069.md) |
| ✅ `BE-FIN-070` | Accounting dapat mengetahui shift mana yang belum selesai, sehingga penegakan tutup bulan tidak punya celah | `FR-FIN-157`; `FIN-DEC-115`, `121`; `FIN-DES-084` | `FIN-INTEGRATION-1.7` 5.12.3; `FIN-STATE-1.6` F.3 | `SyncCashierShiftClosureMarkersAsync` beserta pola nomor siklusnya; unique index outbox | Kode `PEMBUKAAN-SHIFT-KASIR` ditambahkan ke katalog dan ke `ZeroAmountAllowedEventTypes`; query diperluas dari **tiga** menjadi **tujuh** status | `BE-FIN-062` | Shift `CLOSED_WITH_VARIANCE` yang belum pernah terlihat menerbitkan penanda pembukaan; shift `CLOSED` **tidak**; shift dibuka kembali menerbitkan pembalik siklus lama **sebelum** pembukaan siklus baru; sinkronisasi berulang tidak menggandakan | `dotnet build` PASS | Backend Owner — **gerbang:** `FIN-OQ-047`. Pengirimannya tetap dilewati worker sampai Accounting meratifikasi dan menambahkannya ke daftar nilai nol **mereka** | ✅ **SELESAI 2 Oktober 2026.** Kode `PEMBUKAAN-SHIFT-KASIR` ditambahkan ke katalog `FinAccountingEventTypeCodes` dan ke `ZeroAmountAllowedEventTypes`; query diperluas dari 3 ke 7 status; blok penerbitan penanda pembukaan diimplementasikan untuk seluruh status belum final; idempotensi dijaga composite key `shiftId:SourceVersion`; urutan `REOPENED` benar (pembalik dulu, pembukaan sesudahnya); **nol** migration; kompilasi manual diserahkan ke pengguna. Bukti: [laporan](../task/report/backend/BE-FIN-070.md) |
| ✅ `BE-FIN-071` | Baris kotak keluar Finance benar-benar terkirim ke Accounting, bukan menumpuk menunggu | `FR-FIN-150`..`153`; `FIN-DEC-118`, `093`; `FIN-DES-078` | `FIN-INTEGRATION-1.7` 5.12.6 | Pola `AccAccountingEventSchedulerHostedService`; `FinAccountingEventAttempt` yang sudah ada; blok `runBackgroundJobs` pada `Program.cs` | 1 hosted service + options; 1 `AddHostedService`; kredensial dibaca dari konfigurasi | `BE-FIN-068` | **Mati secara bawaan** — tanpa konfigurasi, nol pengiriman dan satu baris log; **melewati** ketiga penanda shift dan kode yang belum diratifikasi; balasan `200` dan `201` keduanya sukses; `AccountingReceiptNumber` diisi dari `AccountingEventId`; kegagalan mencatat satu `FinAccountingEventAttempt` dan dicoba ulang siklus berikutnya | `dotnet build` PASS; diuji terhadap kotak masuk pada lingkungan integrasi, **bukan** produksi | Backend Owner — **risiko tertinggi REV-14C:** kredensial ditanamkan di source. **MUST** dari konfigurasi. **Gerbang:** G3 masih terbuka bersama Platform | ✅ **SELESAI 2 Oktober 2026.** `FinanceAccountingDispatchWorker` + `FinanceAccountingDispatchWorkerOptions` dibuat; `Enabled = false` bawaan; 8 kode dalam `GatedEventTypeCodes` (3 penanda shift + 5 belum diratifikasi); transaksi per baris; percobaan dicatat ke `FinAccountingEventAttempt`; `Program.cs` diperbarui; **nol** migration; kompilasi manual diserahkan ke pengguna. Bukti: [laporan](../task/report/backend/BE-FIN-071.md) |
| 🟡 `BE-FIN-072` | Snapshot saldo terbit sendiri tiap tanggal 1 pukul 00.05 WIB, dan dinyatakan ulang bila posisinya berubah | `FR-FIN-154`, `FR-FIN-155`; `FIN-DEC-092`, `114`, `118` | `FIN-INTEGRATION-1.7` 5.12.5; `FIN-API-1.5` F.6 | `StageEventAsync` yang **sudah** menaikkan `SourceVersion` otomatis; `FinanceBusinessDate` | 1 hosted service + options; 1 endpoint `POST .../restate`; jam dihitung dalam WIB | `BE-FIN-068` | Terbit tanpa dipicu manual; dijalankan dua kali untuk periode yang sama **tidak** menggandakan baris; pernyataan ulang menyentuh **hanya** akun yang nilainya berubah; mati secara bawaan | `dotnet build` PASS; kasus shift terlambat dibuktikan memicu pernyataan ulang | Backend Owner — **risiko:** menerbitkan ulang seluruh akun, bukan hanya yang berubah. Accounting menerima versi baru untuk akun yang tidak bergerak | 🟡 **SEBAGIAN 2 Oktober 2026.** `FinanceSubledgerSnapshotSchedulerHostedService` (baru) dan `FinanceSubledgerSnapshotSchedulerOptions` (baru) dibangun mengikuti pola `LeaveCarryForwardSchedulerHostedService`; memicu `GenerateMonthlySnapshotsAsync` yang **sudah ada dan tidak diubah** untuk periode sebelumnya setiap hari pukul 00.05 WIB (dapat dikonfigurasi) lewat `FinanceBusinessDate.BusinessTimeZone`; endpoint `POST .../subledger-balances/restate` ditambahkan pada `FinanceAccountingEventsController` memakai access control yang sama dengan `generate`; terdaftar di `Program.cs` dalam keadaan **mati** (`Enabled = false` bawaan). Anti-duplikasi dan pernyataan-ulang-hanya-akun-berubah diwarisi dari `GenerateMonthlySnapshotsAsync`/`BE-FIN-068` yang tidak disentuh. `dotnet build` **NOT RUN** atas permintaan eksplisit pengguna pada task ini — butir DoD ini **belum terpenuhi**, bukan dikecualikan. **Nol** migration. Bukti: [laporan](../task/report/backend/BE-FIN-072.md) |
| 🟡 `BE-FIN-073` | Penanda shift terbit tanpa seseorang menekan tombol | `FR-FIN-156`; `FIN-DEC-118`; `FIN-DES-078` | `FIN-INTEGRATION-1.7` 5.12.6 | `SyncCashierShiftClosureMarkersAsync` dari `BE-FIN-070` | 1 hosted service + options; 1 `AddHostedService` | `BE-FIN-070` | Terbit berkala tanpa tindakan pengguna; mati secara bawaan; nol tulisan ke tabel `Bil*` | `dotnet build` PASS | Backend Owner — risiko rendah; task terkecil REV-14C | 🟡 **SEBAGIAN 2 Oktober 2026.** `FinanceCashierShiftMarkerSchedulerHostedService` (baru) dan `FinanceCashierShiftMarkerSchedulerOptions` (baru) dibangun — hosted service paling sederhana dari ketiga `FIN-DES-078` (interval polling murni); memanggil `SyncCashierShiftClosureMarkersAsync` yang **sudah ada dan tidak diubah** (`BE-FIN-045`/`070`) tiap siklus; terdaftar di `Program.cs` dalam keadaan **mati** (`Enabled = false` bawaan). Dipastikan ketiga kode penanda shift sudah tergerbang di `FinanceAccountingDispatchWorker.GatedEventTypeCodes` (`BE-FIN-071`), sehingga mengaktifkan penjadwal ini tidak membuka pengiriman ke Accounting secara tidak sengaja. `dotnet build` **NOT RUN** atas permintaan eksplisit pengguna pada task ini — butir DoD ini **belum terpenuhi**, bukan dikecualikan. **Nol** migration. Bukti: [laporan](../task/report/backend/BE-FIN-073.md) |

## Task yang sengaja **tidak** dibuat pada REV-14

| Yang tidak dibuat | Alasan |
|---|---|
| Task pembayaran langsung berkontrol — metode, sumber dana, bukti, ambang | Tidak dibuat **pada REV-14A-14C** karena `EPIC FIN-23` saat itu `OPEN DECISION`, tertahan `FIN-OQ-075`. **Diperbarui revisi 15:** gerbang itu sudah dijawab `FIN-DEC-139` dan desainnya disetujui, sehingga task-nya kini **ada** — `BE-FIN-074`..`078` pada gelombang `REV-14D` |
| Task migrasi tagihan lama lewat spreadsheet | Tidak dibuat **pada REV-14A-14C** karena `EPIC FIN-24` saat itu `OPEN DECISION`, tertahan `FIN-OQ-077`. **Diperbarui revisi 15:** gerbang itu sudah dijawab `FIN-DEC-140` (dua format, satu paket berlisensi permisif), sehingga task-nya kini **ada** — `BE-FIN-079`..`083` pada gelombang `REV-14E`, dan hanya `BE-FIN-083` yang masih tertahan `FIN-OQ-081` |
| Migration `AddFinanceTransactionProofAndDirectPaymentThreshold` dan `AddFinanceOpeningItemMigration` | Keduanya milik dua epic di atas. Yang kedua satu-satunya migration revisi 14 yang menyentuh tabel berjalan, dan ia **tidak** masuk gelombang mana pun |
| Task buku mutasi utang jasa medis | `FinMedicalServicePayable` **tidak punya penulis apa pun** hari ini (`BE-FIN-021` `BLOCKED`). Kewajiban membangunnya **bersamaan** dengan `BE-FIN-021` dicatat `FIN-DES-091`, bukan dijadwalkan sekarang |
| Task pemicu manual worker pengiriman | Ditolak desain (`FIN-API-1.5` F.9). Pemicu manual akan menjadi jalan memutar gerbang `FIN-DES-078` |
| Task jalur membuka kembali rekap kas harian | Ditolak `FIN-DEC-125` lewat pilihan menghitung posisi langsung |
| Task saldo per rekening bank | Ditolak `FIN-DEC-137` — milik Accounting |
| Task penyatuan lima salinan helper zona waktu | **Bukan scope Finance.** Task tersendiri lintas modul, menuntut approval pemilik arsitektur backend |
| Task perbaikan namespace bersarang `AppDateTimeHelper` | Menyentuh berkas bersama di luar scope; **MUST NOT** dirapikan diam-diam |
| Task perapian alokator nomor bisnis ke provider number-series atomik | Alasan sama dengan REV-13: `QBE-CODE-001`..`006` belum dipakai seluruh rumpun ini |
| Task automated test | Mengikuti `rules/backend/TEST_POLICY.md`: backend tidak memelihara project test otomatis, dan ketiadaannya **bukan** coverage gap |

## Prasyarat eksekusi REV-14

| # | Prasyarat | Keadaan saat roadmap ditulis |
|---:|---|---|
| 1 | Approval owner atas `FIN-DES-078`..`091` dan ketujuh kontrak turunannya | ✅ **Diberikan** 1 Oktober 2026 ("Saya setujui FIN-DES-078...091") — lihat `blueprint-manifest.md` `status_note_revision_14` |
| 2 | Izin **membuat** migration | ✅ **Diberikan** lewat `FIN-DEC-138`, dengan syarat berkasnya sesuai `ApplicationDbContextModelSnapshot`. Berlaku untuk `BE-FIN-058` dan `BE-FIN-064` |
| 3 | Izin **mengeksekusi** migration ke database | ❌ **Tidak diberikan kepada agent.** `FIN-DEC-138`: penerapan ke database dilakukan **Yasmin sendiri**. Setiap laporan task **MUST** menyatakan migration belum dijalankan |
| 4 | QBE preflight untuk delapan entity baru | Diselesaikan **pada waktu eksekusi** dari `AGENTS.md` backend dan `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`. Prefix `Fin` dan `Mst` sudah terdaftar; **nol** folder submodul baru, sehingga **nol** gerbang `QBE-MOD-003` baru |
| 5 | Kesesuaian engineering contract | Diselesaikan pada waktu eksekusi dari `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`. Delapan model baru berstatus `NEW CODE`, sehingga pola legacy di sekitarnya **tidak** memberi wewenang menirunya |
| 6 | Persetujuan Accounting atas `FIN-OQ-045` dan `FIN-OQ-047` | ❌ Belum. Diminta lewat `evidence/22`. **Tidak** menahan `BE-FIN-069` dan `BE-FIN-070`; menahan **pengiriman** penanda shift |
| 7 | Mekanisme kredensial akun layanan (G3) | ❌ Masih terbuka bersama Platform dan Accounting. **Tidak** menahan `BE-FIN-071`; menahan **pengaktifannya** |
| 8 | `FIN-OQ-075` dan `FIN-OQ-077` | ✅ **Dijawab 1 Oktober 2026** (`FIN-DEC-139`, `FIN-DEC-140`); desainnya disetujui 2 Oktober 2026. Tidak lagi menahan `EPIC FIN-23`/`FIN-24` — penggantinya `FIN-OQ-081` yang menahan `BE-FIN-083` saja |

---

# REV-14D dan REV-14E — Pembayaran langsung berkontrol dan migrasi tagihan lama

```yaml
blueprint_id: FIN-BP-001
roadmap_revision: REV-14D-14E
blueprint_revision: 15
status: SIAP DIEKSEKUSI
decisions: [FIN-DEC-126, FIN-DEC-129, FIN-DEC-134, FIN-DEC-135, FIN-DEC-136, FIN-DEC-138, FIN-DEC-139, FIN-DEC-140]
designs: [FIN-DES-085, FIN-DES-086, FIN-DES-087, FIN-DES-089, FIN-DES-090, FIN-DES-092, FIN-DES-093]
contract_versions: [FIN-API-1.6, FIN-VAL-1.8, FIN-PERM-1.8, FIN-TEST-1.9, FIN-MVP-1.10]
contract_status: approved 2026-10-02 (Yasmin)
epics: [EPIC FIN-23, EPIC FIN-24]
task_range_backend: BE-FIN-074..BE-FIN-083
```

**Kenapa roadmap ini ada, dan kenapa ia terpisah dari REV-14A-14C.** Bagian REV-14 di atas menutup
kedua epic ini dengan satu baris: *"(tanpa gelombang) — `EPIC FIN-23`, `EPIC FIN-24` — `FIN-OQ-075`
dan `FIN-OQ-077` dijawab"*. Kedua gerbang itu **sudah dijawab** 1 Oktober 2026 (`FIN-DEC-139`,
`FIN-DEC-140`) dan desainnya sudah digambar serta disetujui (`FIN-DES-092`, `FIN-DES-093`, revisi 15).
Baris itu karena itu **tidak berlaku lagi**, dan kedua epic kini punya gelombang sendiri.

**Penomoran task melanjutkan REV-14C, tidak menimpanya.** `BE-FIN-058`..`073` sudah dipakai
REV-14A-14C. Roadmap ini mulai dari **`BE-FIN-074`**.

## Wewenang migration pada REV-14D dan REV-14E

`FIN-OQ-051` **sudah dijawab** `FIN-DEC-138`, dan pembelahannya berlaku penuh di sini juga:

| Hal | Keadaan |
|---|---|
| **Membuat** berkas migration | **DIIZINKAN**, dengan syarat sesuai `ApplicationDbContextModelSnapshot` |
| **Menerapkan** ke database | **MILIK YASMIN.** Agent **MUST NOT** menjalankan migration, `dotnet ef database update`, maupun SQL langsung |
| Kewajiban laporan task | Laporan yang membawa migration **MUST** menyatakan migration itu **belum dijalankan** |

**Dua migration yang REV-14 sengaja tunda kini punya gelombangnya**, dan salah satunya adalah
migration berisiko tertinggi pada seluruh revisi 14:

| Migration | Task pembawa | Sifat | Risiko |
|---|---|---|---|
| `AddFinanceTransactionProofAndDirectPaymentThreshold` | `BE-FIN-074` | Dua tabel baru; **nol** tabel lama disentuh | Rendah |
| `AddFinanceOpeningItemMigration` | `BE-FIN-079` | Satu tabel baru **dan** tiga kolom `FinReceivable` menjadi nullable bersyarat, satu check constraint baru, satu index diganti filternya | **TERTINGGI pada revisi 14** — satu-satunya yang menyentuh tabel berjalan |

`BE-FIN-079` karena itu **MUST** mengikuti langkah `NOT VALID`/`VALIDATE` dan
`CREATE INDEX CONCURRENTLY` yang sudah ditulis `02-backend-architecture.md` `L.7`, termasuk pengakuan
bahwa urutan `DROP`-lalu-`CREATE` pada DDL membuka jendela ketika idempotensi intake Billing tidak
dijaga index. Urutan yang lebih aman **SHOULD** dipakai, dan laporan task **MUST** menyatakan urutan
mana yang dipakai.

## Grafik urutan dependency — REV-14D dan REV-14E

```text
REV-14D  (EPIC FIN-23 — pembayaran langsung berkontrol beserta bukti)

BE-FIN-058 [REV-14A] ─┐
                      ├─> BE-FIN-074 🟡 ─┬─> BE-FIN-075 🟡 ────┐
{FIN-DEC-138} ────────┘                  │                     │
                                         └─> BE-FIN-076 🟡 ────┤
                                                               │
BE-FIN-060 [REV-14A] ──────────────────────────────────────────┼─> BE-FIN-077 🟡
                                                               │
BE-FIN-061 [REV-14A] ──────────────────────────────────────────┴─> BE-FIN-078 🟡

REV-14E  (EPIC FIN-24 — migrasi tagihan lama, dua format)

BE-FIN-064 ✅ [REV-14B] ─┐
BE-FIN-065 ✅ [REV-14B] ─┼─> BE-FIN-079 🟡 ─> BE-FIN-080 🟡 ─┬─> BE-FIN-081 🟡 ─> BE-FIN-082 🟡
{FIN-DEC-138} ────────┘                                      │
                                                             └─> BE-FIN-083  {FIN-OQ-081}

Legenda:
  [REV-14A] / [REV-14B]  cermin baca-saja: task milik roadmap REV-14 di atas, bukan milik roadmap ini
  {FIN-DEC-138}          gerbang wewenang migration — sudah terbuka, digambar supaya jejaknya terlihat
  {FIN-OQ-081}           gerbang TERBUKA: nama dan versi paket XLSX. Menahan BE-FIN-083 SAJA
```

| Gelombang | Task | Boleh mulai setelah |
|---|---|---|
| `REV-14D` | `BE-FIN-074` lalu `BE-FIN-075` paralel dengan `BE-FIN-076`, keduanya bertemu di `BE-FIN-077` dan `BE-FIN-078` | `BE-FIN-058`, `060`, dan `061` selesai (buku mutasi dan jejaknya ada). **Tidak** menunggu `REV-14B` maupun `REV-14C` |
| `REV-14E` | `BE-FIN-079` → `080` → `081` → `082` berantai; `BE-FIN-083` menggantung dari `080` | `REV-14B` selesai (saldo awal cutover dan `CutoverDate` terkunci) |
| *(tertahan gerbang)* | `BE-FIN-083` — pembaca XLSX | `FIN-OQ-081` dijawab. **Nol** task lain menunggunya |

**Kenapa `REV-14D` tidak menunggu `REV-14B` dan `REV-14C`.** Pembayaran langsung berkontrol hanya butuh
buku mutasi sebagai tempat menuliskan `ProofId` — ia tidak menyentuh pemetaan akun control maupun jalur
pengiriman. Menggantungkannya pada `REV-14B` berarti memperpanjang keadaan pembayaran langsung tanpa
batas nilai dan tanpa bukti, yang justru `EPIC FIN-23` ada untuk menutup (`04-prd-to-mvp.md` bagian 51).

**Kenapa `BE-FIN-083` digantung dari `BE-FIN-080`, bukan dari `BE-FIN-082`.** `FIN-DES-093` menaruh
kedua pembaca di balik satu antarmuka yang dibangun `BE-FIN-080`. Begitu antarmuka itu ada, pembaca
XLSX dapat ditambahkan kapan saja tanpa menyentuh validasi, rekonsiliasi, maupun persetujuan batch —
dan itulah sebabnya `FIN-OQ-081` hanya menahan satu task, bukan seluruh gelombang.

## Task REV-14D — `EPIC FIN-23`

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
|---|---|---|---|---|---|---|---|---|---|---|
| 🟡 `BE-FIN-074` | Bukti pembayaran dan ambang punya tempat tersimpan, dan konfigurasinya terbaca | `FIN-DEC-126`, `FIN-DEC-134`, `FIN-DEC-139`; `FIN-DES-086`, `FIN-DES-087`, `FIN-DES-092` | `FIN-API-1.6` F.3/F.4, `FIN-VAL-1.8` G.1 | Pola `IdentityModel`, `FileStorage:UploadRootPath` yang sudah berjalan, pola `<Modul>:<Fitur>:AllowedExtensions` | 2 model (`FinTransactionProof`, `MstDirectPaymentThreshold`), 2 EF configuration, 2 `DbSet`, migration `AddFinanceTransactionProofAndDirectPaymentThreshold`, dua kunci konfigurasi beserta pembacanya | `BE-FIN-058` ✅ (kolom `ProofId` pada baris mutasi sudah ada) | Kedua tabel terbentuk; `StoredFileName` unique; `ProofId` unique pada **kedua** tabel mutasi sehingga satu bukti tidak dapat dipakai dua pembayaran; **nol** tabel berjalan disentuh | QBE preflight; review diff/scope; `dotnet restore` dan `dotnet build`; verifikasi skema terhadap `erd/data-dictionary.md` R14.6/R14.7 | Backend Owner — risiko rendah, murni tabel baru. **Migration dibuat, TIDAK dijalankan** | 🟡 **SEBAGIAN 2 Oktober 2026.** Kedua model (`FinTransactionProof`, `MstDirectPaymentThreshold`), 2 EF configuration, 2 `DbSet`, migration `AddFinanceTransactionProofAndDirectPaymentThreshold` (tangan, mengikuti pola `BE-FIN-058`/`064`) beserta `ApplicationDbContextModelSnapshot.cs` yang diperbarui, dan `FinanceTransactionProofOptions` (dua kunci konfigurasi, terdaftar tanpa syarat di `Program.cs`) seluruhnya dibuat. `StoredFileName` unique terbukti; `ProofId` unique pada kedua tabel mutasi **diwarisi** dari `BE-FIN-058`, bukan dibuat ulang; **nol** tabel berjalan diubah skemanya. **Gap dicatat:** FK `ProofId` → `FinTransactionProof` yang digambar `erd/data-dictionary.md` SENGAJA tidak dibuat pada migration ini — menambahkannya berarti mengubah tabel berjalan dan melanggar acceptance criteria task ini sendiri; keputusan kapan/apakah dibuat diserahkan ke pemilik modul. `dotnet build` **NOT RUN** atas permintaan eksplisit pengguna pada task ini — butir DoD ini **belum terpenuhi**, bukan dikecualikan. Migration **dibuat, belum dijalankan** (menunggu Yasmin). Bukti: [laporan](../task/report/backend/BE-FIN-074.md) |
| 🟡 `BE-FIN-075` | Petugas dapat mengunggah bukti, dan berkas yang tidak layak ditolak beserta alasan yang dapat dipahami | `FIN-DEC-139`; `FIN-DES-092` | `FIN-API-1.6` F.3, `FIN-VAL-1.8` `FIN-VAL-214`..`223`, `FIN-PERM-1.8` H.1/H.2 | `FileStorage:UploadRootPath`, `UseStaticFiles`, pola unggah yang sudah berjalan — **tanpa** memanggil kelas unggah milik HR | `FinanceTransactionProofService` beserta **delapan pemeriksaan berurut**; 3 endpoint (`POST /`, `GET /{id}`, `GET /{id}/metadata`); **nol** endpoint `PUT`/`DELETE` | `BE-FIN-074` | Kedelapan pemeriksaan ditegakkan **berurut**, yang pertama gagal menghentikan sisanya; tanpa `MaxFileSizeBytes` unggah ditolak **`503`**, bukan `400` dan bukan diterima; tipe media diperiksa, bukan hanya ekstensi; jalur keluar akar ditolak dan **dicatat**; berkas yatim **tidak** tertinggal bila penulisan metadata gagal; **nol** endpoint `PUT`/`DELETE` ada | QBE preflight; review diff/scope; `dotnet build`; verifikasi kontrak API; verifikasi proses bisnis kedelapan jalur tolak | Backend Owner — **risiko utama:** memberi nilai bawaan pada batas ukuran. Itu **mengarang keputusan owner** dan membuka pintu berkas raksasa; fail-closed `503` **MUST NOT** dilonggarkan | 🟡 **SEBAGIAN 2 Oktober 2026.** `FinanceTransactionProofService` (tiga method: `UploadAsync`, `GetMetadataAsync`, `DownloadAsync`), `FinanceTransactionProofsController` (tiga endpoint persis `[AccessController]` kontrak), dan `TransactionProofDtos.cs` dibuat; kedelapan pemeriksaan ditegakkan berurut termasuk fail-closed `503` saat `MaxFileSizeBytes` kosong; tipe media diperiksa lewat tabel kanonikal tertanam; berkas yatim dihapus bila metadata gagal ditulis; unduhan **tidak** lewat `UseStaticFiles` publik — tetap bergerbang `FinanceTransactionProof : Read`; **nol** endpoint `PUT`/`DELETE`. **Catatan jujur:** pemeriksaan jalur-keluar-akar secara struktural tidak dapat terpicu pada desain ini (nama fisik selalu `Guid.NewGuid()`), dipertahankan sebagai jaring pengaman mengikuti preseden HR. `dotnet build` **NOT RUN** atas permintaan eksplisit pengguna — butir DoD ini **belum terpenuhi**. **Nol** migration. Bukti: [laporan](../task/report/backend/BE-FIN-075.md) |
| 🟡 `BE-FIN-076` | Ambang pembayaran langsung dapat dibaca dan diubah, dan perubahannya selalu berjejak | `FIN-DEC-134`; `FIN-DES-086` | `FIN-API-1.6` F.4, `FIN-VAL-1.8` (aturan ambang revisi 14) | `MstDirectPaymentThreshold` dari `BE-FIN-074`; pola master data yang sudah ada | 2 endpoint (`GET /`, `PUT /`), `ChangeReason` wajib, satu baris aktif dijaga unique index | `BE-FIN-074` | `ChangeReason` kosong ditolak; nilai bukan angka positif ditolak; tanpa baris aktif, `GET` menjawab `404` dan **seluruh** pembayaran langsung ditolak di `BE-FIN-077`/`078`; perubahan tercatat logger **tanpa** nilai sensitif | QBE preflight; review diff/scope; `dotnet build`; verifikasi kontrak API; verifikasi proses bisnis | Backend Owner — **risiko:** nilai ambang (`FIN-OQ-074`) belum ada. Itu **tidak** menahan task ini; perilaku tanpa ambang memang menolak semuanya | 🟡 **SEBAGIAN 2 Oktober 2026.** `DirectPaymentThresholdService` (`GetActiveAsync`, `UpdateAsync`) dan `DirectPaymentThresholdController` (dua endpoint persis `[AccessController]` kontrak) dibuat; `ChangeReason` kosong ditolak `422`, nilai bukan positif ditolak `400`, tanpa baris aktif `GET` menjawab `404`; baris diperbarui di tempat (bukan riwayat implisit) mengikuti `FIN-DES-086`; perubahan tercatat `LoggerService.AuditAsync` mengikuti pola `CurrencyService`. `[Required]` sengaja dihindari pada `ChangeReason` supaya kode status `422` kontrak tidak kalah oleh validasi model otomatis `[ApiController]`. `dotnet build` **NOT RUN** atas permintaan eksplisit pengguna — butir DoD ini **belum terpenuhi**. **Nol** migration. Bukti: [laporan](../task/report/backend/BE-FIN-076.md) |
| 🟡 `BE-FIN-077` | Pembayaran langsung piutang tidak lagi kehilangan metode, sumber dana, dan buktinya, dan nilai di atas ambang ditolak | `FIN-DEC-126`, `FIN-DEC-132`, `FIN-DEC-134`; `FIN-DES-085`, `FIN-DES-092` | `FIN-API-1.6` F.3/F.8, `FIN-VAL-1.8` `FIN-VAL-222`/`223` | `FinanceReceivableService.RecordPaymentAsync` yang sudah ada; buku mutasi piutang dari `BE-FIN-060`; dimensi payload dari `BE-FIN-069` | Memperbarui `POST /receivables/{id}/payment`: metode, sumber dana, nomor rujukan, dan `ProofId` **wajib**; ambang ditegakkan; baris mutasi membawa keempatnya | `BE-FIN-075`, `BE-FIN-076`, `BE-FIN-060` ✅ | Metode dan sumber dana **tersimpan**, bukan diterima lalu dibuang; nominal di atas ambang ditolak dan diarahkan ke jalur berjenjang; tanpa ambang aktif **seluruh** pembayaran ditolak; `ProofId` yang sudah terpakai ditolak `409`; pembayaran tunai menggerakkan Kas Kasir dan **tidak** menyentuh anggaran kas kecil | QBE preflight; review diff/scope; `dotnet build`; verifikasi kontrak API; verifikasi proses bisnis termasuk jalur tolak ambang dan bukti terpakai | Backend Owner — **PERUBAHAN MEMUTUS.** Layar `FE-FIN-030` **MUST** dirilis sebelum atau bersamaan, tidak sesudahnya. Urutan rilis **MUST** dicatat pada laporan | 🟡 **SEBAGIAN 2 Oktober 2026.** `RecordPaymentAsync` diperbarui: tujuh validasi baru berurutan (`FIN-VAL-197`..`203`), `fundingSourceId`/`proofId` kini diteruskan ke `RecordReceivableMovementAsync` yang sudah mendukung keduanya sejak `BE-FIN-058`; `BankAccountId` yang sebelumnya diterima lalu dibuang kini benar-benar ditegakkan. Pembayaran tunai tetap menggerakkan Kas Kasir (tidak diubah). **PERUBAHAN MEMUTUS** dikonfirmasi dan urutan rilis dicatat eksplisit: backend ini **MUST NOT** aktif di produksi sebelum `FE-FIN-030` siap mengirim `ProofId` — lihat laporan bagian 7. `dotnet build` **NOT RUN** atas permintaan eksplisit pengguna — butir DoD ini **belum terpenuhi**. **Nol** migration. Bukti: [laporan](../task/report/backend/BE-FIN-077.md) |
| 🟡 `BE-FIN-078` | Pembayaran langsung utang supplier mendapat kontrol yang sama bentuknya dengan piutang | `FIN-DEC-126`, `FIN-DEC-133`, `FIN-DEC-134`; `FIN-DES-085`, `FIN-DES-092` | `FIN-API-1.6` F.3/F.8 | `FinanceSupplierPayableService` jalur pembayaran langsung; buku mutasi utang dari `BE-FIN-061` | Memperbarui `POST /supplier-payables/{id}/direct-payment` dengan bentuk **sama persis** seperti `BE-FIN-077` | `BE-FIN-075`, `BE-FIN-076`, `BE-FIN-061` ✅ | Bentuk kontraknya **sama** dengan `BE-FIN-077` — perbedaan bentuk antara kedua jalur adalah cacat; pembayaran tunai **tidak** memotong anggaran kas kecil (`FIN-DEC-133`); `ProofId` terpakai ditolak `409` | QBE preflight; review diff/scope; `dotnet build`; verifikasi kontrak API; verifikasi proses bisnis | Backend Owner — **PERUBAHAN MEMUTUS**, syarat urutan rilis sama dengan `BE-FIN-077` | 🟡 **SEBAGIAN 2 Oktober 2026.** `RecordDirectPaymentAsync` diperbarui dengan tujuh validasi identik `BE-FIN-077`; **satu cacat lama ditemukan dan diperbaiki**: `BankAccountId` sebelumnya `[Required] Guid` non-nullable sehingga `FIN-VAL-201` mustahil ditegakkan — kini `Guid?`. `proofId` diteruskan ke `RecordSupplierPayableMovementAsync` yang sudah mendukungnya sejak `BE-FIN-058` (`fundingSourceId` sudah diteruskan sebelum task ini). Pembayaran tunai tetap menggerakkan Kas Kasir, nol sentuhan kas kecil (tidak diubah). **PERUBAHAN MEMUTUS** dikonfirmasi, urutan rilis sama dengan `BE-FIN-077` (`FE-FIN-030` MUST dirilis bersamaan/lebih dulu) — lihat laporan bagian 7. `dotnet build` **NOT RUN** atas permintaan eksplisit pengguna — butir DoD ini **belum terpenuhi**. **Nol** migration. Bukti: [laporan](../task/report/backend/BE-FIN-078.md) |

**`FR-FIN-166` tidak punya task sendiri, dan itu disengaja.** Meneruskan metode dan sumber dana ke
Accounting sudah menjadi cakupan `BE-FIN-069` (REV-14C) yang membawa lima ruas dimensi pada payload.
Membuat task kedua untuk hal yang sama melahirkan dua pemilik bagi satu perubahan payload.
`BE-FIN-077`/`078` karena itu **memakai** jalur itu, bukan membangunnya ulang.

## Task REV-14E — `EPIC FIN-24`

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
|---|---|---|---|---|---|---|---|---|---|---|
| 🟡 `BE-FIN-079` | Tagihan lama punya tempat tersimpan, dan piutang non-migrasi tetap wajib berasal dari Billing | `FIN-DEC-129`, `FIN-DEC-136`, `FIN-DEC-138`; `FIN-DES-089`, `FIN-DES-093` | `FIN-API-1.6` F.2, `erd/data-dictionary.md` R14.6 | `FinOpeningBalance` dan `CutoverDate` dari `BE-FIN-064`/`065`; pola configuration rumpun `AccountingIntegration` | 1 model baru (`FinOpeningItemBatch`, **termasuk kolom `SourceFormat`**), 1 EF configuration, 1 `DbSet`, migration `AddFinanceOpeningItemMigration`: tiga kolom `FinReceivable` asal Billing menjadi **nullable bersyarat**, satu check constraint baru, satu index diganti filternya | `BE-FIN-064` dan `BE-FIN-065` (`REV-14B`) | Tabel batch terbentuk beserta `SourceFormat`; check constraint menegakkan **piutang non-migrasi wajib punya kolom asal Billing** dan piutang migrasi boleh kosong; baris piutang dan utang yang **sudah ada** tetap sah sesudah migration (nol baris melanggar); idempotensi intake Billing tetap terjaga sesudah index diganti filternya | QBE preflight; review diff/scope; `dotnet restore` dan `dotnet build`; verifikasi skema terhadap kamus data; **pemeriksaan atas data yang ada** sebelum dan sesudah | Backend Owner — **RISIKO TERTINGGI REVISI 14.** Satu-satunya migration yang menyentuh tabel berjalan. Urutan `NOT VALID`/`VALIDATE` dan `CREATE INDEX CONCURRENTLY` **MUST** diikuti, dan urutan `DROP`-lalu-`CREATE` membuka jendela idempotensi yang **MUST** disebut laporan. **Migration dibuat, TIDAK dijalankan** | 🟡 **SEBAGIAN 2 Oktober 2026.** `FinOpeningItemBatch` (model + configuration + `DbSet`, termasuk `SourceFormat`), migration `AddFinanceOpeningItemMigration` (tangan: `CreateTable`, `DROP NOT NULL` tiga kolom `FinReceivable`, `OpeningItemBatchId` + FK pada `FinReceivable`/`FinSupplierPayable`, `CK_FinReceivable_OpeningItem` via `NOT VALID`+`VALIDATE`, index unik diganti filter via `CREATE INDEX CONCURRENTLY`), dan `ApplicationDbContextModelSnapshot.cs` yang diperbarui seluruhnya dibuat. Tiga DTO respons (`ReceivableResponse`, `EligibleReceivableResponse`, `ReceivableInvoiceBatchMemberResponse`) ikut diperbarui `InvoiceId` menjadi nullable — konsekuensi wajib, bukan tambahan lingkup — dan satu titik layanan (`FinanceReceivableInvoiceBatchService.GetDocumentAsync`) dijaga dari pemanggilan dengan `InvoiceId` kosong. `dotnet restore`/`dotnet build` **NOT RUN** atas permintaan eksplisit pengguna — butir DoD ini **belum terpenuhi**. Migration **dibuat, belum dijalankan** (menunggu Yasmin); pemeriksaan data sebelum/sesudah **belum dapat dijalankan** karena itu. Bukti: [laporan](../task/report/backend/BE-FIN-079.md) |
| 🟡 `BE-FIN-080` | Berkas migrasi dapat dibaca menjadi baris bernomor, dan jalur CSV berjalan tanpa paket apa pun | `FIN-DEC-136`, `FIN-DEC-140`; `FIN-DES-093` | `FIN-API-1.6` F.2, `FIN-VAL-1.8` `FIN-VAL-224`/`226` | Kemampuan bawaan .NET; **nol** paket | `IOpeningItemFileReader`, `OpeningItemRawRow`, `CsvOpeningItemFileReader`, dua berkas templat **CSV** (piutang dan utang), endpoint `GET /template?itemKind=&format=` untuk `format=CSV` | `BE-FIN-079` | Antarmuka memulangkan **baris terurai** — **nol** `DataTable`, `Stream`, maupun tipe milik paket melewati batasnya; nilai sel dipulangkan sebagai **teks mentah**; `format` di luar `CSV`/`XLSX` ditolak `400`; baris CSV yang format angka/tanggalnya tidak sesuai templat ditolak **beserta nomor barisnya**, **MUST NOT** ditebak | QBE preflight; review diff/scope; `dotnet build`; verifikasi kontrak API; verifikasi proses bisnis penguraian CSV | Backend Owner — **risiko:** menguraikan angka/tanggal di dalam pembaca. Itu melahirkan dua aturan format yang dapat berselisih; penguraian milik lapisan validasi | 🟡 **SEBAGIAN 2 Oktober 2026.** `IOpeningItemFileReader`, `OpeningItemRawRow`, `CsvOpeningItemFileReader` (pengurai RFC 4180 tulisan tangan, nol paket), `FinanceOpeningItemBatchesController` (`GET /template`), dan dua templat CSV dibuat; `format` di luar `CSV`/`XLSX` ditolak `400` persis `FIN-VAL-224`; `format=XLSX` — nilai sah tetapi templatnya belum ada — dijawab `503` mengikuti pola `FIN-VAL-220`, bukan `400`. **Kolom kedua templat CSV disusun dari `FIN-VAL-187`..`191` karena tidak ada kontrak yang mencantumkannya verbatim — keputusan implementasi, MUST ditinjau pemilik modul sebelum disebarkan ke petugas** (lihat laporan bagian 7). `dotnet build` **NOT RUN** atas permintaan eksplisit pengguna — butir DoD ini **belum terpenuhi**. **Nol** migration; **nol** paket ditambahkan. Bukti: [laporan](../task/report/backend/BE-FIN-080.md) |
| 🟡 `BE-FIN-081` | Setiap baris tagihan lama divalidasi, dan petugas tahu baris mana yang salah | `FIN-DEC-136`, `FIN-DEC-140`; `FIN-DES-089`, `FIN-DES-093` | `FIN-API-1.6` F.2, `FIN-VAL-1.8` `FIN-VAL-186`..`191` dan `224`..`227` | `IOpeningItemFileReader` dari `BE-FIN-080`; `CutoverDate` dari `BE-FIN-064` | `FinanceOpeningItemBatchService` bagian unggah dan validasi; endpoint `POST /` (menerima CSV **dan** XLSX) dan `POST /{id}/validate`; pencatatan `SourceFormat`; `ValidationSummaryJson` | `BE-FIN-080` | Validasi bekerja di atas **baris terurai**, bukan di atas berkas — aturannya **tunggal** untuk kedua format; format ditentukan dari **tipe media dan ekstensi**, bukan dari ruas yang diisi pengguna; format di luar CSV/XLSX ditolak `400`; galat baris memuat **nomor baris**; batch tetap `DRAFT` selama masih ada galat; `SourceFormat` tercatat | QBE preflight; review diff/scope; `dotnet build`; verifikasi kontrak API; verifikasi proses bisnis keenam aturan baris | Backend Owner — **risiko:** menambahkan ruas format yang diisi pengguna pada unggah. Itu membuka jalan memaksa pembaca yang salah | 🟡 **SEBAGIAN 2 Oktober 2026.** `FinanceOpeningItemBatchService` (`UploadAsync`, `ValidateAsync`), `POST /` dan `POST /{id}/validate` dibuat; keenam aturan baris (`FIN-VAL-187`..`191`, `226`) ditegakkan berurut, `FIN-VAL-191` diperiksa lebih dulu supaya baris kembar tetap terdeteksi; `XLSX` pada unggah dijawab `503` (pembaca belum ada), bukan `400`. **Dua keputusan tanpa rujukan kontrak eksplisit, MUST ditinjau pemilik modul** (lihat laporan bagian 7): (a) berkas fisik disimpan pada jalur yang dihitung deterministik dari `Id`+`SourceFormat` karena `FinOpeningItemBatch` tidak punya kolom path, dan (b) `SourceFormat` sementara selalu `"CSV"` sampai `BE-FIN-083` menambah pembaca kedua. `dotnet build` **NOT RUN** atas permintaan eksplisit pengguna — butir DoD ini **belum terpenuhi**. **Nol** migration. Bukti: [laporan](../task/report/backend/BE-FIN-081.md) |
| 🟡 `BE-FIN-082` | Tagihan lama yang sudah direkonsiliasi menjadi item bertagihan beserta mutasi pembukanya, dan selisihnya tidak pernah lolos | `FIN-DEC-129`, `FIN-DEC-130`; `FIN-DES-089`, `FIN-DES-090` | `FIN-API-1.6` F.2, `FIN-STATE-1.6` F (siklus batch) | Buku mutasi dari `BE-FIN-060`/`061`; `FinOpeningBalance` dari `BE-FIN-065` | 4 endpoint (`GET /`, `GET /{id}`, `POST /{id}/declare-accounting-opening`, `POST /{id}/approve`, `POST /{id}/reject`); pembuatan item piutang/utang beserta mutasi pembuka **dalam satu transaksi** | `BE-FIN-081` | Batch **tidak dapat** disetujui bila total sisanya berbeda dari saldo awal Accounting yang dinyatakan — penolakan menampilkan **kedua angka**; persetujuan melahirkan item **dan** mutasi pembuka dalam **satu** transaksi, gagal berarti nol-duanya; item migrasi menerbitkan **nol** kejadian akuntansi; sesudah diposting item mengikuti penagihan, pelunasan, umur piutang, dan snapshot yang normal; batch `APPROVED`/`LOCKED`/`REJECTED` menolak tindakan lanjutan `409` | QBE preflight; review diff/scope; `dotnet build`; verifikasi kontrak API; verifikasi proses bisnis termasuk jalur selisih dan jalur tolak | Backend Owner — **risiko utama:** melahirkan item tanpa mutasi pembuka, atau sebaliknya. Keduanya membuat saldo tidak dapat dipertanggungjawabkan sejak hari pertama | 🟡 **SEBAGIAN 2 Oktober 2026.** Seluruh lima endpoint dibuat; `ApproveAsync` melahirkan `FinReceivable`/`FinSupplierPayable` beserta mutasi `PEMBUKAAN-MIGRASI` (konstanta dan parameter `openingItemBatchId` **sudah ada** sejak `BE-FIN-058`, baru dipakai di sini) dalam satu transaksi (`BeginTransactionAsync` Serializable + advisory lock pada Id batch), menolak `422` beserta kedua angka bila total sisa berbeda dari saldo awal Accounting, dan menolak `409` di luar status yang diizinkan; re-validasi berlapis dijalankan ulang dari berkas fisik sebelum melahirkan data finansial. **Dua cacat `BE-FIN-081` ditemukan dan diperbaiki** sebagai bagian task ini (EMPLOYEE_BENEFIT yang akan melanggar check constraint; `RowVersion` yang tidak terputar di `ValidateAsync`) — lihat laporan `BE-FIN-081` bagian 8. **Risiko tersisa ditulis apa adanya:** perilaku commit/rollback transaksi dan advisory lock PostgreSQL sungguhan **belum pernah diuji berjalan** — hanya diverifikasi lewat pembacaan struktur kode. `dotnet build` **NOT RUN** atas permintaan eksplisit pengguna — butir DoD ini **belum terpenuhi**. **Nol** migration. Bukti: [laporan](../task/report/backend/BE-FIN-082.md) |
| `BE-FIN-083` ⛔ | Berkas XLSX dapat dibaca, dan hasilnya identik dengan CSV berisi data yang sama | `FIN-DEC-140`; `FIN-DES-093` | `FIN-API-1.6` F.2, `FIN-TEST-1.9` J.3 | `IOpeningItemFileReader` dari `BE-FIN-080`; validasi dari `BE-FIN-081` — **keduanya tidak disentuh** | `XlsxOpeningItemFileReader`, dua berkas templat **XLSX**, **satu** paket pembaca ditambahkan ke `QuilvianSystemBackend.csproj` | `BE-FIN-080`; **⛔ BLOCKED `FIN-OQ-081`** | **Tepat satu** paket pembaca XLSX terpasang, lisensinya **permisif**, **nol** `EPPlus` v5+; berkas CSV dan XLSX berisi data sama menghasilkan baris terurai **identik** termasuk **nomor baris** pada galatnya; kolom keempat templat sinkron berpasangan; tipe milik paket **tidak** bocor melewati antarmuka — dibuktikan dengan mengganti pembaca jadi ganda palsu dan validasi tetap lulus tanpa diubah | QBE preflight; review diff/scope; `dotnet build`; verifikasi kontrak API; verifikasi paritas dengan **dua berkas berisi data sama**; pemeriksaan `csproj` | Backend Owner — **⛔ BLOCKED:** `FIN-OQ-081` (nama dan versi paket) belum dijawab, pemilik Yasmin. Wewenang penambahan paket **MUST** dinyatakan eksplisit **pada task ini** menurut `AGENTS.md`; keputusan `FIN-DEC-140` memberi wewenangnya **secara prinsip**, bukan menggantikan pernyataan task. **Risiko:** memakai `EPPlus` v5+ memasukkan kewajiban lisensi komersial ke sistem rumah sakit | Build PASS; **nol** migration; wewenang paket dinyatakan pada laporan; laporan task tracked ada |

**Yang tetap bisa berjalan walaupun `BE-FIN-083` terblokir.** `BE-FIN-079`, `080`, `081`, dan `082`
seluruhnya tidak menunggu `FIN-OQ-081`. Artinya jalur CSV dapat dipakai petugas **ujung ke ujung** —
unduh templat CSV, isi, unggah, perbaiki galat, nyatakan saldo awal, setujui, dan tagihan lama mulai
dapat ditagih — sebelum satu paket pun ditambahkan. Itu pemisahan yang disengaja `FIN-DES-093`, bukan
kebetulan.

## Traceability REV-14D dan REV-14E

| Functional requirement | Task pembawa | Bukti verifikasi |
|---|---|---|
| `FR-FIN-160` | `BE-FIN-077`, `BE-FIN-078` | Verifikasi proses bisnis kedua jalur |
| `FR-FIN-161` | `BE-FIN-075`, `BE-FIN-077`, `BE-FIN-078` | Verifikasi kontrak API + proses bisnis |
| `FR-FIN-162` | `BE-FIN-077`, `BE-FIN-078` | Verifikasi proses bisnis kas |
| `FR-FIN-163`, `FR-FIN-164`, `FR-FIN-165` | `BE-FIN-076`, `BE-FIN-077`, `BE-FIN-078` | Verifikasi proses bisnis ambang |
| `FR-FIN-166` | `BE-FIN-069` (REV-14C) — **dipakai, bukan dibangun ulang** | Verifikasi kontrak integrasi pada task itu |
| `FR-FIN-174`, `FR-FIN-175`, `FR-FIN-177` | `BE-FIN-075` | Verifikasi proses bisnis kedelapan jalur tolak |
| `FR-FIN-176` | `BE-FIN-075` (nol `PUT`/`DELETE`) + `FE-FIN-030` (nol tombol ganti) | Pemeriksaan permukaan API + verifikasi manual layar |
| `FR-FIN-167`, `FR-FIN-182` | `BE-FIN-080` (CSV) + `BE-FIN-083` (XLSX) | Perbandingan baris judul keempat templat |
| `FR-FIN-168`, `FR-FIN-178`, `FR-FIN-181` | `BE-FIN-081` | Verifikasi kontrak API + proses bisnis |
| `FR-FIN-169` | `BE-FIN-082` | Verifikasi proses bisnis jalur selisih |
| `FR-FIN-170`, `FR-FIN-171`, `FR-FIN-172` | `BE-FIN-082` | Verifikasi proses bisnis ujung ke ujung |
| `FR-FIN-173` | `BE-FIN-079` (check constraint) | Pemeriksaan atas data yang ada |
| `FR-FIN-179` | `BE-FIN-083` | Verifikasi paritas dua berkas berisi data sama |
| `FR-FIN-180` | `BE-FIN-080` | Pembaca diganti ganda palsu, validasi tetap lulus |
| `FR-FIN-183` | `BE-FIN-079` (kolom) + `BE-FIN-081` (pencatatan) | Verifikasi skema + proses bisnis |

**Nol coverage gap requirement-ke-bukti-verifikasi pada kedua gelombang ini.** Keenam belas
functional requirement `EPIC FIN-23` dan `EPIC FIN-24` seluruhnya punya task pembawa dan bukti
verifikasinya. Mengikuti `rules/backend/TEST_POLICY.md`, ketiadaan automated backend test **bukan**
coverage gap, dan **nol** task "write unit tests" dimunculkan.

## Prasyarat eksekusi REV-14D dan REV-14E

| # | Prasyarat | Keadaan |
|---:|---|---|
| 1 | Approval owner atas `FIN-DES-092`/`093` dan kelima kontraknya | ✅ **Diberikan** 2 Oktober 2026 (Yasmin) — lihat `blueprint-manifest.md` `approval_revision_15` |
| 2 | Wewenang **membuat** migration | ✅ Dijawab `FIN-DEC-138`. Berlaku untuk `BE-FIN-074` dan `BE-FIN-079` |
| 3 | Wewenang **menerapkan** migration ke database | ❌ **MILIK YASMIN.** Agent **MUST NOT** menjalankannya. Dua migration menunggu: `AddFinanceTransactionProofAndDirectPaymentThreshold` dan `AddFinanceOpeningItemMigration` |
| 4 | `REV-14A` selesai (`BE-FIN-058`, `060`, `061`) | Diperiksa pada waktu eksekusi — menahan `REV-14D` |
| 5 | `REV-14B` selesai (`BE-FIN-064`, `065`) | Diperiksa pada waktu eksekusi — menahan `REV-14E` |
| 6 | `FIN-OQ-081` — nama dan versi paket XLSX | ❌ Belum. Menahan **`BE-FIN-083` saja** |
| 7 | `FIN-OQ-082` — nilai batas ukuran berkas bukti | ❌ Belum. **Tidak** menahan pembangunan; tanpa nilainya unggah bukti ditolak `503` fail-closed |
| 8 | `FIN-OQ-074` — angka ambang | ❌ Belum. **Tidak** menahan pembangunan; tanpa ambang seluruh pembayaran langsung ditolak |
| 9 | `FIN-OQ-076` — jumlah tagihan lama | ❌ Belum. Menentukan batas baris per unggahan; **tidak** menahan `BE-FIN-079`..`082` |
| 10 | QBE preflight untuk tiga entity baru | Diselesaikan **pada waktu eksekusi** dari `AGENTS.md` backend dan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`. Prefix `Fin` dan `Mst` sudah terdaftar; **nol** folder submodul baru, sehingga **nol** gerbang `QBE-MOD-003`. Folder `Readers/` berada di dalam submodule yang sudah terdaftar |
| 11 | Kesesuaian engineering contract | Diselesaikan pada waktu eksekusi dari `BACKEND_ENGINEERING_CONTRACT.md`. Ketiga model baru berstatus `NEW CODE` |
| 12 | Urutan rilis layar untuk dua perubahan memutus | **MUST** dijaga: `FE-FIN-030` dirilis sebelum atau bersamaan dengan `BE-FIN-077`/`078` |

---

# REV-14F — Penyelarasan saldo awal cutover (amandemen pasca-`FE-FIN-028`)

```yaml
blueprint_id: FIN-BP-001
roadmap_revision: REV-14F
status: SOURCE SELESAI — menunggu build, migration, dan pengamatan pengguna
decisions: [FIN-DEC-128]   # ditambah keputusan pemilik 3 Oktober 2026 (lihat di bawah) — BELUM bernomor FIN-DEC
designs: [FIN-DES-088]
contract_versions: [FIN-API-1.5 F.1, FIN-STATE-1.6 F.1, FIN-VAL-1.7 FIN-VAL-168]   # bagian terkait diperbarui 3 Oktober 2026
task_range_backend: BE-FIN-084
```

**Kenapa ada gelombang ini.** Saat `FE-FIN-028` dibangun, layar saldo awal cutover menemukan empat selisih antara
kontrak dan backend `BE-FIN-066`. Pemilik (Yasmin) memutuskan penyelesaiannya pada **3 Oktober 2026**:

| # | Selisih | Keputusan pemilik |
|:--:|---|---|
| 1 | `Notes` pada `approve`/`lock` diterima tetapi tidak disimpan | **Hapus** `Notes` dari kedua DTO dan dari kontrak |
| 2 | `ApprovedBy` hanya `Guid` | **Tambah** `ApprovedByName` pada `OpeningBalanceResponse` |
| 3 | Penguncian `KAS-KASIR` bernominal nol tidak menerbitkan mutasi kas | Mutasi **selalu terbit**, sehingga `FIN-VAL-168` diberi **satu pengecualian**: mutasi `SALDO-AWAL` boleh bernilai nol |
| 4 | `api-contract.md` F.1 masih berlabel "Rencana (belum tersedia)" | Dikoreksi menjadi **Tersedia** |

> **Catatan pencatatan keputusan.** Keputusan 1–3 diambil langsung oleh pemilik dalam sesi `FE-FIN-028` dan **belum
> dicatat** di `00-interview-decisions.md` sebagai `FIN-DEC-nnn`. Nomor tidak dikarang di sini. Pencatatan resminya
> adalah pekerjaan `qv-grill` (Amendment Pass) dan harus dilakukan sebelum blueprint revisi berikutnya disetujui.

## Task REV-14F

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 🟡 `BE-FIN-084` | Saldo awal cutover menyampaikan nama penyetuju, tidak menjanjikan catatan yang tidak disimpan, dan penguncian `KAS-KASIR` selalu meninggalkan satu mutasi `SALDO-AWAL` | `FR-FIN-146`..`148`; `FIN-DEC-128`; keputusan pemilik 3 Oktober 2026; `FIN-DES-088` | `FIN-API-1.5` F.1; `FIN-STATE-1.6` F.1; `FIN-VAL-1.7` `FIN-VAL-168` | `BE-FIN-066`; pola pencarian nama `PettyCashBudgetService`; `FinanceSubledgerMovementService` | Perubahan `SubledgerOpeningBalanceDtos` (hapus `Notes` ×2, tambah `ApprovedByName`); `FinanceOpeningBalanceService` (pencarian nama; syarat `Amount > 0` pada penguncian dihapus); `RecordCashMovementAsync` (pengecualian nol khusus `SALDO-AWAL`); `FinCashMovementConfiguration`; migration `RelaxFinCashMovementAmountForZeroOpeningBalance` | `BE-FIN-066`, `BE-FIN-062` | `Approve`/`Lock` tidak lagi memuat `Notes`; setiap respons saldo awal memuat `ApprovedByName` (`null` bila belum disetujui atau pengguna tidak ditemukan); mengunci `KAS-KASIR` bernominal 0 menerbitkan tepat satu mutasi `SALDO-AWAL` bernilai 0; mutasi **negatif** tetap ditolak untuk semua jenis; mutasi **nol** untuk jenis selain `SALDO-AWAL` tetap ditolak `400`; `CK_FinCashMovement_Amount` di DB selaras dengan aturan itu | Kontrak API dan `FIN-VAL-168` terverifikasi; `dotnet build` **PASS** dan migration **diterapkan** (keduanya dilaporkan pengguna 3 Oktober 2026); `has-pending-model-changes` **bersih** (4 Oktober 2026); uji manual penguncian nol **masih menunggu pengguna** | Backend Owner — **risiko:** pengecualian nol bocor ke jenis mutasi lain dan melemahkan invariant buku kas (dijaga oleh `movementType == SaldoAwal` pada service **dan** pada constraint DB). Migration menyentuh tabel yang **sudah berjalan** | 🟡 **SOURCE SELESAI 3 Oktober 2026.** Seluruh cakupan tertulis; migration ditulis **manual** (tanpa `dotnet ef`). Pengguna melaporkan `dotnet build` dan penerapan migrasi **sukses** (bukan pengamatan agent). `has-pending-model-changes` dinyatakan **bersih** 4 Oktober 2026, sehingga migration tangan itu terbukti selaras dengan model. Sisa untuk ✅: **hanya** uji manual penguncian `KAS-KASIR` bernominal 0. Bukti: [laporan BE-FIN-084](../task/report/backend/BE-FIN-084.md) |

## Wewenang migration pada REV-14F

| Hal | Keadaan |
|---|---|
| Membuat berkas migration | Diminta eksplisit pengguna 3 Oktober 2026, **ditulis tangan tanpa `dotnet ef`/build**. Syarat `FIN-DEC-138` ("sesuai snapshot") dipenuhi dengan memperbarui `ApplicationDbContextModelSnapshot` pada satu baris yang sama dan menurunkan `Designer` dari snapshot itu — **belum dibuktikan** oleh tooling EF |
| Menerapkan ke database | **MILIK YASMIN.** Tidak dijalankan |
| Pembuktian kesesuaian | ✅ **TERBUKTI** 4 Oktober 2026. `dotnet ef migrations has-pending-model-changes --configuration Release` menyatakan nol perubahan tertunda, sehingga syarat `FIN-DEC-138` terbukti oleh tooling EF — bukan lagi hanya oleh pembacaan berkas |

## Prasyarat eksekusi REV-14F

| # | Prasyarat | Keadaan |
|:--:|---|---|
| 1 | `BE-FIN-066` selesai | ✅ |
| 2 | Wewenang tulis backend dan artefak kontrak | ✅ Diberikan pengguna 3 Oktober 2026 |
| 3 | Pencatatan keputusan sebagai `FIN-DEC-nnn` | ❌ Belum — tidak menahan source, menahan persetujuan blueprint revisi berikutnya |
| 4 | Migration diterapkan ke database | ❌ **Milik Yasmin.** Sampai dijalankan, penguncian `KAS-KASIR` bernominal 0 ditolak database |


---

# REV-16 — Penyelarasan ambang, batas baris, dan penanda rekap kas (backend)

```yaml
blueprint_id: FIN-BP-001
roadmap_revision: REV-16
blueprint_revision: 16
status: SIAP DIEKSEKUSI — approval revisi 16 diberikan Yasmin 4 Oktober 2026
decisions: [FIN-DEC-141, FIN-DEC-142, FIN-DEC-143, FIN-DEC-144, FIN-DEC-145, FIN-DEC-146, FIN-DEC-151, FIN-DEC-152, FIN-DEC-153, FIN-DEC-154, FIN-DEC-155, FIN-DEC-156]
designs: [FIN-DES-094, FIN-DES-095, FIN-DES-096, FIN-DES-097, FIN-DES-098]
contract_versions: [FIN-API-1.7, FIN-VAL-1.9, FIN-TEST-1.10, FIN-MVP-1.11]
contract_status: approved 2026-10-04 (Yasmin)
backend_source_sha: 5d6bb8bf
frontend_source_sha: d962574d6
task_range_backend: BE-FIN-086, BE-FIN-088..BE-FIN-090 (BE-FIN-087 dicabut 4 Oktober 2026 — lihat §0 laporan BE-FIN-086)
tanggal: 4 Oktober 2026
```

## Gerbang — SELURUHNYA TERBUKA sejak 4 Oktober 2026

| Hal | Keadaan |
|---|---|
| Approval desain revisi 16 | ✅ **DIBERIKAN** Yasmin, 4 Oktober 2026. `FIN-DES-094`..`098` beserta `FIN-API-1.7`, `FIN-VAL-1.9`, `FIN-TEST-1.10`, `FIN-MVP-1.11` naik menjadi `approved` |
| Keputusan yang mendasarinya | ✅ `FIN-DEC-141`..`160`, dua Amendment pass 4 Oktober 2026 |
| Prasyarat teknis yang sempat menahan `BE-FIN-086`/`087` | ✅ **BERSIH.** Pengguna menjalankan `dotnet ef migrations has-pending-model-changes --configuration Release` 4 Oktober 2026; keluarannya *"No changes have been made to the model since the last migration."* |
| Task yang tertahan gerbang apa pun | **NOL.** Kelima task REV-16 bebas dieksekusi |

**Risiko yang pemeriksaan ini tutup, dan hasilnya.** Satu migration sebelumnya
(`RelaxFinCashMovementAmountForZeroOpeningBalance`) **ditulis tangan** tanpa `dotnet ef`, termasuk berkas
Designer dan satu baris pada snapshot. Bila snapshot berselisih dari model, migration `BE-FIN-086` akan
membawa perubahan yang tidak seorang pun minta ke tabel yang sudah berjalan.

**Snapshot terbukti selaras dengan model.** Migration yang ditulis tangan itu **tidak** meninggalkan
selisih, sehingga `BE-FIN-086` boleh membuat migration tanpa kekhawatiran itu. Catatan ini dipertahankan
sebagai jejak: yang membuktikannya adalah tooling EF, bukan pembacaan berkas oleh agent.

**Pekerjaan yang dapat berjalan sejajar, tanpa menunggu apa pun:** lima prasyarat go-live pada
`04-prd-to-mvp.md` 56.6 — seluruhnya **bukan kode**: mengisi konfigurasi batas ukuran berkas, menetapkan
ambang pertama lewat layar, memberi hak baca ambang kepada peran staf, menerapkan migration yang tertunda,
dan menjaga ketiga hosted service tetap mati.

## Satu utang traceability yang ditutup roadmap ini

`BE-FIN-085` **sudah dibangun** (source ada, dilaporkan, build dilaporkan berhasil) tetapi **belum pernah
punya baris task** di roadmap mana pun. Laporannya sendiri mencatat itu sebagai kekurangan. Barisnya
ditulis di bawah supaya traceability-nya tidak berlubang.

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 🟡 `BE-FIN-085` | Berkas batch migrasi yang masih Draf dapat diunggah ulang, dan pesan penolakan rekonsiliasi memuat dua desimal | `FR-FIN-189` (asalnya `FR-FIN-168`, `FR-FIN-169`); `FIN-DEC-144`; `FIN-STATE-1.6` F.2 baris "Mengunggah ulang berkas"; `FIN-VAL-192` | `FIN-API-1.6` F.2 (saat dikerjakan endpoint-nya belum ada di kontrak; disusulkan `FIN-API-1.7` G.2) | Pemeriksaan berkas `UploadAsync` yang sudah ada, diekstrak menjadi satu metode bersama | 1 endpoint `POST /{id}/reupload`; 1 DTO; refactor pemeriksaan berkas menjadi aturan tunggal; pesan `422` rekonsiliasi memakai dua desimal budaya `id-ID` | `BE-FIN-081`, `BE-FIN-082` | Hanya batch `DRAFT` yang dapat diunggah ulang; `VALIDATED`/`APPROVED`/`LOCKED`/`REJECTED` ditolak `409`; hasil validasi lama dibuang; saldo awal Accounting yang sudah dinyatakan **dipertahankan**; jenis item tidak dapat diganti; `ExpectedRowVersion` basi ditolak `409`; berkas lama **tidak** tersentuh bila basis data gagal | `dotnet build` dilaporkan **PASS** oleh pengguna 4 Oktober 2026; uji manual `K.4.1`..`K.4.6` **belum** dilaporkan | Backend Owner — **risiko:** refactor menyentuh jalur unggah yang sudah berjalan; dan jendela sempit ketika penggantian berkas fisik gagal **sesudah** basis data tersimpan | 🟡 Sebagian. Source selesai; build dilaporkan PASS; uji manual belum. Bukti: [laporan BE-FIN-085](../task/report/backend/BE-FIN-085.md) |

## Grafik urutan dependency — REV-16

```text
(bersih 4 Okt 2026, source selesai 4 Okt 2026) ──> BE-FIN-086 🟡 ─> FE-FIN-033 ─> FE-FIN-034   (ambang: satu migration, FIN-DES-094)

(boleh mulai sekarang) ──┬─> BE-FIN-088                  (batas 10.000 baris)
                         ├─> BE-FIN-089 ────────────────> FE-FIN-035
                         ├─> BE-FIN-090 ────────────────> FE-FIN-036
                         └─────────────────────────────> FE-FIN-037   (butir menu; nol dependency backend)

{FIN-OQ-081} ─> BE-FIN-083 [POST-MVP]   (pembaca XLSX — keluar dari rilis pertama, FIN-DEC-149)

Legenda:
  [xxx]   prasyarat teknis yang masih menahan
  {xxx}   gerbang keputusan yang masih tertutup
  (xxx)   tidak tertahan apa pun
```

**`BE-FIN-087` DICABUT 4 Oktober 2026**, sebelum pernah dieksekusi — lihat §0 pada
[laporan BE-FIN-086](../task/report/backend/BE-FIN-086.md). `FIN-DES-094` menetapkan **satu** migration
untuk ambang (tambah `RowVersion`, buang `EffectiveFrom` sekaligus); draf roadmap sebelumnya memecahnya
menjadi dua task mengikuti opsi `02-backend-architecture.md` N.7, dan pertentangan itu diputuskan pemilik
ke arah `FIN-DES-094`. Nomor task `BE-FIN-087` **tidak dipakai ulang** untuk task lain mana pun.

| Gelombang | Task backend | Boleh mulai setelah |
|---|---|---|
| `REV-16A` | `BE-FIN-086` 🟡 | ✅ Source dan migration selesai ditulis 4 Oktober 2026. **Belum dikompilasi, belum diterapkan** — lihat DoD |
| `REV-16B` | `BE-FIN-088`, `BE-FIN-089`, `BE-FIN-090` | **Boleh mulai sekarang.** Nol migration, nol dependency pada `REV-16A` |
| `POST-MVP` | `BE-FIN-083` | `FIN-OQ-081` dijawab |

## Task REV-16A — ambang pembayaran langsung

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 🟡 `BE-FIN-086` | Ambang hanya dapat diubah oleh pejabat yang bekerja dari nilai terbaru; layar dapat menampilkan siapa yang terakhir mengubahnya; kolom tanggal berlaku dibuang | `FR-FIN-184`, `FR-FIN-185`, `FR-FIN-187`; `FIN-DEC-145`, `146`, `151`, `153`; `FIN-DES-094`, `095` | `FIN-API-1.7` G.1; `FIN-VAL-1.9` `FIN-VAL-228`; `erd/data-dictionary.md` R14.8 | Pola `RowVersion` + `[ConcurrencyCheck]` pada `FinOpeningBalance` dan `FinOpeningItemBatch`; pola pencarian nama `PettyCashBudgetService` dan `FinanceOpeningBalanceService`; pola `IsUniqueViolation` pada `HmdServiceSupport` | 1 kolom `RowVersion` ditambah, 1 kolom `EffectiveFrom` dibuang pada `MstDirectPaymentThreshold` beserta configuration-nya; **1 migration** (`FIN-DES-094`: satu migration, bukan dua — `BE-FIN-087` DICABUT, lihat §0 laporan); `UpdateAsync` memeriksa `ExpectedRowVersion` dan memutarnya, dengan pengecualian penetapan pertama; dua lapis tambahan menangkap benturan (`DbUpdateConcurrencyException`, unique-violation penetapan pertama bersamaan); `DirectPaymentThresholdResponse` membawa `RowVersion` dan `LastChangedByName` | **NOL** | Penetapan ambang **pertama** diterima **tanpa** `ExpectedRowVersion`; ambang yang sudah ada menolak `PUT` tanpa penanda versi atau dengan penanda basi (`409`); dua penetapan pertama bersamaan **MUST NOT** menghasilkan `500`; urutan pemeriksaan tetap alasan kosong (`422`) → nominal tidak sah (`400`) → benturan versi (`409`); `LastChangedByName` berisi nama tampilan, dan `null` bila penggunanya tidak ditemukan; `EffectiveFrom` yang masih dikirim klien lama **diabaikan tanpa galat**; kolom `EffectiveFrom` **dibuang** dari basis data | Uji `K.1.1`..`K.1.5`, `K.2.1`..`K.2.6` **belum dilaporkan** — satu-satunya yang tersisa; `dotnet build` **PASS**, migration **diterapkan**, `has-pending-model-changes` retroaktif **PASS** (ketiganya dilaporkan pengguna 4 Oktober 2026) | Backend Owner — **risiko:** melewatkan pengecualian penetapan pertama akan mengunci jalur pembayaran langsung permanen; migration membuang kolom pada tabel berjalan — direncanakan nol baris (`FIN-DEC-154`), belum diverifikasi lewat query **sebelum** diterapkan. QBE preflight dan kesesuaian engineering diselesaikan pada waktu eksekusi dari `AGENTS.md` backend dan `docs/engineering/` | `dotnet build` **PASS**; migration **diterapkan**; `has-pending-model-changes` retroaktif **PASS** (ketiganya dilaporkan pengguna 4 Oktober 2026); uji manual **belum dilaporkan** — satu-satunya sisa untuk ✅; laporan task tracked ada. Bukti: [laporan BE-FIN-086](../task/report/backend/BE-FIN-086.md) |

## Task REV-16B — batas baris, nama penyetuju, dan penanda rekap kas

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 🟡 `BE-FIN-088` | Berkas migrasi yang terlalu besar ditolak sebelum menyentuh penyimpanan, beserta arahan memecahnya | `FR-FIN-188`; `FIN-DEC-155`; `FIN-DES-096` | `FIN-API-1.7` G.2; `FIN-VAL-1.9` `FIN-VAL-229` | `ReadAndCheckUploadAsync` yang sudah menjadi aturan tunggal sejak `BE-FIN-085` | 1 pemeriksaan batas **10.000 baris data** di dalam pemeriksaan berkas yang sudah ada; 1 konstanta kode (**bukan** konfigurasi) | `BE-FIN-085` | Berkas **10.000** baris diterima (batas inklusif); **10.001** ditolak `400` beserta pesan yang menyebut batas dan menyarankan memecah; baris judul **tidak** dihitung; berkas yang ditolak meninggalkan **nol** berkas fisik dan **nol** baris batch; batas berlaku pada unggah **dan** unggah ulang | Uji `K.3.1`..`K.3.6` **belum dilaporkan**; nol migration | Backend Owner — **risiko:** memeriksa terlalu dini (dari ukuran berkas) berarti menebak, yang dilarang `FIN-DEC-140`; memeriksa terlalu lambat meninggalkan sampah di disk | `dotnet build` **NOT RUN** (instruksi pengguna); source selesai; laporan task tracked ada. Bukti: [laporan BE-FIN-088](../task/report/backend/BE-FIN-088.md) |
| 🟡 `BE-FIN-089` | Layar batch dapat menampilkan siapa yang menyetujui, bukan hanya kapan | `FR-FIN-187`; `FIN-DEC-151`; `FIN-DES-095` | `FIN-API-1.7` G.2 | Pola pencarian nama yang sama dengan `BE-FIN-086` dan `BE-FIN-084` | `ApprovedByName` pada `OpeningItemBatchResponse` dan `OpeningItemBatchDetailResponse`; nama seluruh penyetuju satu halaman daftar diambil **sekali**, bukan satu kueri per baris | `BE-FIN-082` | `ApprovedByName` berisi nama tampilan penyetuju; `null` bila belum disetujui atau penggunanya tidak ditemukan; nama **tidak** disimpan ke tabel Finance; daftar berpaging **tidak** menimbulkan satu kueri per baris | Uji `K.1.4` (pola yang sama) **belum dilaporkan**; nol migration | Backend Owner — **risiko:** menyalin nama ke kolom tabel akan membuatnya membeku saat nama aslinya berubah (`FIN-DES-095` melarangnya) | `dotnet build` **NOT RUN** (instruksi pengguna); source selesai; laporan task tracked ada. Bukti: [laporan BE-FIN-089](../task/report/backend/BE-FIN-089.md) |
| 🟡 `BE-FIN-090` | Periode yang belum punya rekap kas harian dinyatakan sebagai keadaan, bukan dijawab angka nol | `FR-FIN-190`; `FIN-DEC-152`; `FIN-DES-098` | `FIN-API-1.7` G.4 — **PERUBAHAN MEMUTUS** | `CalculateCashVarianceAsync` yang sudah ada | `DailyCashClosingBalance` dan `VarianceAmount` menjadi **boleh kosong**; `HasVariance` bernilai salah bila rekap tidak ada; 1 ruas baru `HasDailyCashSnapshot` | `BE-FIN-067` | Periode tanpa rekap: kedua ruas **kosong** (**MUST NOT** `0`), `HasVariance` salah, `HasDailyCashSnapshot` salah; posisi kas terhitung dan mutasi penjelas **tetap** dikirim; periode dengan rekap dan angka sama: selisih `0` dan `HasVariance` salah; periode dengan rekap dan angka berbeda: kedua angka terkirim dan `HasVariance` benar | Uji `K.5.1`..`K.5.6` **belum dilaporkan**; nol migration | Backend Owner — **risiko:** ini perubahan memutus. Satu-satunya pembaca (`FE-FIN-029`) sudah tahan nilai kosong, sehingga backend **boleh** rilis lebih dulu — **berbeda** dari `FIN-API-1.5` F.8 | `dotnet build` **NOT RUN** (instruksi pengguna); source selesai; laporan task tracked menyebut urutan rilis yang dipakai. Bukti: [laporan BE-FIN-090](../task/report/backend/BE-FIN-090.md) |

## Task yang sengaja TIDAK dibuat pada REV-16

| Yang tidak dibuat | Alasan |
|---|---|
| Task membuat tabel riwayat perubahan ambang | Ditolak `FIN-DES-086`, dan `FIN-DEC-145` menghapus alasan teknis terakhir untuk membuatnya |
| Task menambah kolom nama pengubah pada tabel ambang | `FIN-DES-095`: nama dibaca saat menyusun respons, tidak disalin |
| Task membuat batas baris sebagai konfigurasi | `FIN-DES-096`: ia batas ketahanan transaksi, bukan nilai operasional |
| Task menegakkan `EffectiveFrom` pada jalur pembayaran | Kebalikan dari `FIN-DEC-145`; kolomnya justru dibuang |
| `BE-FIN-083` pembaca XLSX | **Pindah ke `POST-MVP`** oleh `FIN-DEC-149`. Tidak lagi menahan `MVP-14E` dinyatakan selesai |
| Task mengisi nilai konfigurasi dan memberi hak peran | **Bukan kode.** Keduanya prasyarat go-live (`04-prd-to-mvp.md` 56.6), dikerjakan administrator |
| Task migrasi utang jasa medis dan piutang sewa non-pasien lama | `FIN-DEC-157` dan `FIN-DEC-158` menetapkan keduanya di luar batch migrasi |

## Wewenang migration pada REV-16

| Hal | Keadaan |
|---|---|
| **Membuat** berkas migration | **DIIZINKAN** (`FIN-DEC-138`), dengan syarat sesuai `ApplicationDbContextModelSnapshot` dan **dihasilkan dari perubahan model**, bukan ditulis tangan |
| **Menerapkan** ke basis data | **MILIK YASMIN.** Agent **MUST NOT** menjalankan migration maupun SQL langsung |
| Prasyarat yang MUST dipenuhi lebih dulu | ✅ **TERPENUHI.** `has-pending-model-changes` dinyatakan **bersih** 4 Oktober 2026, sehingga migration yang ditulis tangan sebelumnya terbukti tidak meninggalkan selisih |
| Migration pada REV-16 | **Satu** migration untuk `BE-FIN-086` (`FIN-DES-094`). `BE-FIN-087` DICABUT 4 Oktober 2026 sebelum pernah dieksekusi — lihat §0 [laporan BE-FIN-086](../task/report/backend/BE-FIN-086.md) |
| Kewajiban laporan | Setiap laporan task yang membawa migration **MUST** menyatakan migration itu **belum dijalankan** beserta langkah yang Yasmin perlu jalankan sendiri |

## Prasyarat eksekusi REV-16

| # | Prasyarat | Keadaan |
|:--:|---|---|
| 1 | `approval_revision_16` terisi | ✅ **DIBERIKAN** Yasmin, 4 Oktober 2026 |
| 2 | `has-pending-model-changes` bersih | ✅ **BERSIH** 4 Oktober 2026 (`--configuration Release`). Tidak lagi menahan apa pun |
| 3 | `BE-FIN-085` uji manual dilaporkan | ❌ Belum. Tidak menahan REV-16, tetapi menahan `BE-FIN-085` ditandai ✅ |
| 4 | `BE-FIN-081`, `082`, `067` selesai | ✅ Source selesai; build dilaporkan PASS |
| 5 | Wewenang membuat migration | ✅ `FIN-DEC-138` |
| 6 | `FIN-OQ-081` dijawab | ❌ Belum. Menahan **`BE-FIN-083` saja** di `POST-MVP` |
