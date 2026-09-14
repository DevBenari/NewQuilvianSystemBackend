# Roadmap Delivery — Billing dan Kasir

Blueprint `BIL-CASH-001 revision 0.4` telah disetujui pada 20 Agustus 2026; revision `0.5` (amendment `BKC-DEC-059`–`062`, form "Buat Invoice Manual (Testing)" berbasis katalog tarif + coverage per item) disetujui 2 September 2026. **Revision `0.8` (mencakup amendment 3 dan 4 September 2026, sampai `BKC-DES-025`) disetujui 4 September 2026** — Product/Domain Owner mengunci keenam dokumen kontrak turunannya. Hanya revision `0.9` (`BKC-DES-026`/`027`, perluasan perutean jalur `NotCovered`) yang masih `draft`.

Roadmap ini berada pada **revision `2`** (4 September 2026) dan berstatus `DRAFT_FORWARD_TEST`: urutan dan task sudah dapat ditinjau, tetapi **belum memberi wewenang menulis source**. Setiap builder hanya boleh menjalankan satu task yang kemudian disetujui secara eksplisit. Revision `1` memuat `BKC-PH-001` sampai `BKC-PH-008`; revision `2` menambahkan `BKC-PH-009` sampai `BKC-PH-015`.

## Fase

| Phase | Outcome | Backend | Frontend | Dependency | Status |
| --- | --- | --- | --- | --- | --- |
| `BKC-PH-001` | Fondasi dapat diuji | `BE-BKC-001` | — | Blueprint approved | `READY_FOR_TASK_APPROVAL` |
| `BKC-PH-002` | Policy finansial dapat dikelola | `BE-BKC-002`–`004` | `FE-BKC-002` | PH-001 | `PLANNED` |
| `BKC-PH-003` | Charge menjadi running invoice yang benar | `BE-BKC-005`–`008` | `FE-BKC-001`,`003`,`004` | PH-001/002 | `PLANNED` |
| `BKC-PH-004` | Deposit dan split payment berjalan | `BE-BKC-009`–`011` | `FE-BKC-005`,`006` | PH-003 | `PLANNED` |
| `BKC-PH-005` | Shift dan exception finansial terkontrol | `BE-BKC-012`–`014` | `FE-BKC-007`,`008` | PH-004 | `PLANNED` |
| `BKC-PH-006` | Finalisasi menghasilkan AR/AP idempotent | `BE-BKC-015`,`016` | `FE-BKC-009` | PH-003–005 | `PLANNED` |
| `BKC-PH-007` | Bukti lintas-slice dan hardening lengkap | `BE-BKC-017` | `FE-BKC-010` | Semua slice | `PLANNED` |
| `BKC-PH-008` | Entri manual katalog tarif + coverage per item (form "Buat Invoice Manual (Testing)") — `BKC-DEC-059`–`062` | `BE-BKC-018`–`021` | `FE-BKC-014`–`016` | Blueprint `0.5 approved` (2 Sep 2026) | `READY_FOR_TASK_APPROVAL` |
| `BKC-PH-009` | Rupiah tanggungan penjamin per baris biaya (`MVP-4`, `EPIC BKC-04`) | `BE-BKC-022` | — | Tidak ada | **`READY_FOR_TASK_APPROVAL`** |
| `BKC-PH-010` | Lembar "Invoice Asuransi" — endpoint dan tab (`MVP-5`/`MVP-6`, `EPIC BKC-05`) | `BE-BKC-023` | `FE-BKC-018` | `BKC-PH-009`; `BKC-GATE-03` | `BLOCKED` |
| `BKC-PH-011` | Tanggungan tanpa nominal menggantung dan anomali data penjamin (`MVP-7`, `EPIC BKC-06`/`BKC-07`) | `BE-BKC-024`, `BE-BKC-025` | — | Tidak ada | **`READY_FOR_TASK_APPROVAL`** |
| `BKC-PH-012` | Verifikasi gerbang PPN rawat inap versus rawat jalan (`MVP-8`, `EPIC BKC-08`) | `BE-BKC-026` | — | Tidak ada | **`READY`** |
| `BKC-PH-013` | Menu Pembayaran menjumlah dan menampilkan anomali (`MVP-9`) | — | `FE-BKC-019`, `FE-BKC-020` | `BKC-PH-011`, `BKC-PH-014` terverifikasi hidup | `BLOCKED` |
| `BKC-PH-014` | Penanggungan selisih yang tidak dapat ditagihkan (`MVP-11`/`MVP-12`, `EPIC BKC-09`) | `BE-BKC-027`–`029` | `FE-BKC-021` | `BKC-PH-011` **selesai lebih dulu** (sequencing, bukan gerbang) | `BLOCKED` |
| `BKC-PH-015` | Perluasan perutean jalur `NotCovered`, data induk PPN, dan regresi penutup | `BE-BKC-030`–`032` | — | `BKC-PH-014`; `BKC-GATE-06` | `BLOCKED`, kecuali `BE-BKC-031` yang **`READY`** — **status usang, lihat § Amendment 7 September 2026: `BE-BKC-030`–`032` sudah `DONE` per `backend-roadmap.md` § 3** |
| `BKC-PH-016` | Dokumen Kasir: tab "Invoice Asuransi" dibangun ulang dari nol (laporan lama tidak dapat dipercaya), dan penyelarasan Struk Pasien terhadap referensi staging (`BLOCKED` penuh) | `BE-BKC-023` (sudah ada di source, verifikasi ulang saja) | `FE-BKC-018` (dibuka ulang), `FE-BKC-022` (baru, `BLOCKED` penuh) | `BKC-PH-010` (superseded) | `READY_FOR_TASK_APPROVAL` untuk `FE-BKC-018`; `FE-BKC-022` **`BLOCKED` penuh** — lihat `frontend-roadmap.md` § Amendment 7 September 2026 |
| `BKC-PH-017` | **Rumpun baru — Petty Cash.** Fondasi skema, penomoran, dan data induk kategori (`MVP-13`) | `BE-BKC-033`–`035` | — | Blueprint revisi `1.0 approved`; `PC-OQ-003` non-blocking (lihat catatan task) | **`READY_FOR_TASK_APPROVAL`** |
| `BKC-PH-018` | **Rumpun baru — Petty Cash.** Kolam anggaran, siklus hidup voucher penuh, dan hardening lintas-slice (`MVP-14`) | `BE-BKC-036`–`038` | — | `BKC-PH-017` selesai dan terverifikasi | **`READY_FOR_TASK_APPROVAL`** |
| `BKC-PH-019` | **Rumpun baru — Petty Cash.** Layar monitoring, Buat Voucher, Bukti Nota, detail voucher, Anggaran Kas Kecil, dan Kategori Petty Cash (`MVP-15`) | — | `FE-BKC-023`–`027` | `BKC-PH-018` selesai dan terverifikasi (sequencing, bukan gerbang) | **`READY_FOR_TASK_APPROVAL`** |
| `BKC-PH-020` | Deposit rawat inap terikat episode — permintaan `RWI-BP-001` lewat `RWI-DEC-093`–`096`. Semula `BKC-PH-009`/`BE-BKC-022`,`023`, dinomori ulang 9 September 2026 karena bentrok dengan gelombang 4 September | `BE-BKC-039` (✅ Selesai), `BE-BKC-040` (✅ Selesai) | — (layar ada di Rawat Inap) | `BKC-PH-004` | ✅ `Selesai` (`BE-BKC-039` dan `BE-BKC-040` selesai 2026-09-09) |

## Amendment 7 September 2026 — Koreksi revisi blueprint, verifikasi ulang FE-BKC-018, dan cakupan Struk Pasien

```yaml
roadmap_revision: 3
roadmap_status: DRAFT_FORWARD_TEST
blueprint_revision_dibaca_top_level: 0.8 (field `revision` pada blueprint-manifest.md)
blueprint_revision_dibaca_narasi: 0.9 (BKC-DES-026/027, BKC-GATE-06 ditutup 5 September 2026 — lihat isi manifest sendiri § "Catatan revisi 0.7" dan `contract_versions.api`/`testing`)
frontend_branch_diperiksa: QuilvianIntegrationFrontend (checked out saat sesi ini; HEAD `12f9242ce`)
frontend_branch_disebut_laporan_lama: yasmina (tidak checked out saat sesi ini; tip `699935230`, sudah ter-merge ke `QuilvianIntegrationFrontend`)
```

**Ketidaksesuaian revisi manifest — dikonfirmasi nyata, BUKAN temuan baru.** `blueprint-manifest.md`
field `revision: 0.8` tidak pernah dinaikkan menjadi `0.9`, padahal badan dokumen yang sama
(`contract_versions.api`/`testing`, `artifact_hashes_note`, dan bagian "Catatan revisi 0.7") sudah
menyatakan `BKC-DES-026`–`027` **approved** 5 September 2026 dan `BKC-GATE-06` **tertutup**. Ini
sudah dicatat sebelumnya sebagai `BKC-GAP-03` pada `requirement-traceability.md` § 3 (koreksi:
"Perbarui manifest ke revisi 0.9 beserta hash artefaknya") — **masih terbuka**, belum ada perbaikan
sejak dicatat. Isi revisi `0.9` (perluasan perutean jalur `NotCovered` ke mekanisme write-off,
`BE-BKC-030`) **tidak bersinggungan** dengan tab "Invoice Asuransi" maupun Struk Pasien — keduanya
domain terpisah (residual/write-off vs dokumen cetak). Karena itu `BKC-GAP-03` **tidak memblokir**
perencanaan pada fase ini; ia tetap dicatat sebagai utang dokumentasi milik `/manage-module-blueprint`.

**Temuan baru: laporan tracked `FE-BKC-018` tidak dapat dipercaya.** Laporan
`task/report/frontend/FE-BKC-018.md` menyatakan tab "Invoice Asuransi" **source selesai**,
`lint:errors`/`test:unit`/`build` **lulus** 6 September 2026, dengan delapan berkas
diubah/ditambah. Verifikasi langsung pada repository frontend sesi ini (branch `QuilvianIntegrationFrontend`,
juga diperiksa di branch `yasmina` yang disebut laporan) menemukan **nol** jejak: tidak ada berkas
`invoice-asuransi-document.jsx`, tidak ada string `INVOICE_ASURANSI`/`insuranceInvoiceDocument` di
mana pun pada `src/`, `git log`/`git reflog`/`git stash list` tidak menemukan commit maupun stash
yang berisi perubahan ini. Laporan itu sendiri mencatat status "**Belum di-commit**" — kombinasi
"belum di-commit" dan "tidak ada di working tree maupun reflog/stash mana pun" berarti pekerjaan itu
**hilang** (working tree tempat ia ditulis sudah tidak dapat ditemukan), bukan sekadar belum
disinkronkan. Detail lengkap dan bukti grep ada di `frontend-roadmap.md` § Amendment 7 September
2026 dan `requirement-traceability.md` § Amendment 7 September 2026 (`BKC-GAP-08`).

Backend pasangannya, `BE-BKC-023` (endpoint `GET .../invoices/{id}/insurance-invoice-document`),
**terkonfirmasi ADA** pada source backend saat ini (`BillingInvoicesController.cs` baris 303) —
tidak terdampak masalah yang sama. Registrasi DI (`BillingInsuranceInvoiceDocumentService`) yang
sempat hilang dan menyebabkan `InvalidOperationException` pada seluruh `BillingInvoicesController`
sudah diperbaiki di luar sesi perencanaan ini (dilaporkan pemilik task, tidak didiagnosis ulang di
sini).

**Keputusan scope untuk gelombang ini**:

1. `FE-BKC-018` **dibuka ulang** dengan ID yang sama (bukan ID baru) — statusnya diturunkan dari
   "selesai" menjadi belum dikerjakan sama sekali untuk tujuan perencanaan, sesuai
   `status-task-roadmap.md` § 5 ("menurunkan status juga wajib"). Acceptance criteria dan kontrak
   pada roadmap ini **tidak berubah** — backend (`BE-BKC-023`) sudah siap, sehingga task ini
   `READY_FOR_TASK_APPROVAL` untuk dieksekusi dari nol oleh `build-module-frontend`.
2. `FE-BKC-022` (**baru**) menampung permintaan penyelarasan Struk Pasien terhadap referensi
   staging (`staging.quilvian-mmchospital.com`). Setelah dicocokkan satu per satu terhadap
   `00-interview-decisions.md`, **tidak satu pun** dari empat elemen visual staging punya
   keputusan bisnis yang mengikat penempatannya di Struk Pasien — task ini karena itu **`BLOCKED`
   secara penuh**, bukan sebagian, sampai `/grill-me` menutup gapnya. Rinciannya ada di
   `frontend-roadmap.md` § Amendment 7 September 2026.

## Roadmap revision `2` — gelombang `MVP-4` sampai `MVP-12`

Revision `2` (4 September 2026) menambahkan `BKC-PH-009` sampai `BKC-PH-015` di atas, yaitu
sebelas task backend (`BE-BKC-022`–`032`) dan empat task frontend (`FE-BKC-018`–`021`). Isinya
menurunkan amendment blueprint 3 dan 4 September 2026 — dokumen "Invoice Asuransi", pembagian
tanggungan penjamin, anomali data pendaftaran, gerbang PPN, dan penanggungan selisih yang tidak
dapat ditagihkan.

**Diperbarui 4 September 2026 — kontrak dikunci.** Semula sepuluh dari sebelas task backend dan
keempat task frontend berstatus `BLOCKED`. Urutan yang terjadi: `/qv-trace` dijalankan terhadap
`HEAD` `fd4a605` dan menemukan sebagian besar pekerjaan sudah selesai lewat task ad-hoc di luar
roadmap; pemilik menjawab tiga pertanyaan penutup soal PPN rawat inap dan limit bulanan; lalu
**Product/Domain Owner (wewenang ganda Finance/AR) mengunci keenam dokumen kontrak**, menutup
`BKC-GATE-01`. Hasilnya: **empat task backend kini `READY_FOR_TASK_APPROVAL` tanpa satu gerbang
pun** (`BE-BKC-022`, `024`, `026`, `031`); lima task lain hanya menunggu task pendahulunya selesai
(sequencing, bukan gerbang governance).

Yang masih tertahan gerbang sungguhan tinggal dua: `BKC-GATE-03` (Security, hanya
`BE-BKC-023`/`FE-BKC-018`) dan `BKC-GATE-06` (revisi `0.9`, hanya `BE-BKC-030`). `BKC-GATE-05`
(MCU/telemedicine/OTC) turun menjadi syarat aktivasi, bukan syarat roadmap.

**Kontrak dikunci bukan wewenang tulis.** Setiap task tetap menunggu approval task tersendiri dan
konfirmasi `TASK MODE: BACKEND`/`FRONTEND` beserta cabang kerja (`BKC-GATE-09`) sebelum satu baris
source pun ditulis — sesuai § Aturan eksekusi di bawah.

**Riwayat koreksi.** Tiga putaran koreksi berturut-turut pada roadmap ini — penutupan
`BKC-GATE-07` (working tree ternyata sudah ter-commit), penutupan `BKC-GATE-02` (`/qv-trace`
dijalankan), jawaban pemilik yang menutup `BKC-GATE-04`/menurunkan `BKC-GATE-05`, dan akhirnya
penutupan `BKC-GATE-01` (kontrak dikunci) — ada di [backend-roadmap.md](./backend-roadmap.md)
§ 4 dan § 5, serta [01-existing-capability-map.md](../01-existing-capability-map.md) § 17.

Rincian gerbang, urutan gelombang, dan alasan tiap dependency ada di
[backend-roadmap.md](./backend-roadmap.md) § Amendment 4 September 2026 dan
[requirement-traceability.md](./requirement-traceability.md) § Amendment 4 September 2026.

## Aturan eksekusi

1. Task backend dan frontend tetap terpisah.
2. Migration **boleh digenerasikan** hanya bila disebut dalam scope task backend yang disetujui. Migration tidak boleh dijalankan ke database tanpa otorisasi terpisah.
3. Setiap backend task menjalankan QBE preflight dari `AGENTS.md`, engineering contract, registry prefix, dan aturan `.codex` yang berlaku saat eksekusi.
4. Frontend task menunggu governance frontend tersedia; pilihan visual yang tidak mengubah kontrak tetap `DEV_DISCRETION`.
5. Task berstatus `DONE` hanya setelah bukti acceptance yang ditetapkan benar-benar tersedia.

Dokumen: [backend](./backend-roadmap.md), [frontend](./frontend-roadmap.md), dan [traceability](./requirement-traceability.md).

## Amendment 7 September 2026 (kedua) — Rumpun baru: Petty Cash (Voucher Kas Kecil)

Blueprint `BIL-CASH-001` naik ke **revisi `1.0`** (7 September 2026), disetujui `Product/Domain
Owner` lewat `PC-DEC-001`–`015` (`00-interview-decisions.md`) dan `PC-DES-001`–`014`
(`02-backend-architecture.md`/`03-frontend-architecture.md`). Revisi `1.0` adalah pass desain
**penuh untuk satu rumpun baru** — Petty Cash — bukan amendment atas rumpun yang sudah ada, dan
**tidak menyentuh** satu pun file, tabel, atau kontrak milik rumpun `billing-kasir` sebelumnya
(revisi 0.6–0.9: Invoice Asuransi, PPN/coverage subtotal, write-off routing, Struk Pasien).

**Cakupan pass perencanaan ini dibatasi khusus pada Petty Cash.** Rumpun-rumpun lain memiliki
roadmap revisi `3` yang sudah ada di atas (`FE-BKC-018` dibuka ulang, `FE-BKC-022` `BLOCKED`
penuh, dst.) dan **tidak diubah maupun dinilai ulang** oleh pass ini — `blueprint-manifest.md`
menandai bukti as-is rumpun tersebut sebagai stale terhadap SHA `dd31bc9` dan menuntut
`trace-existing-capabilities` impact scan penuh sebelum `plan-module-delivery` dapat aman
merencanakan ulang mereka. Petty Cash **tidak menunggu** scan itu — manifest mengonfirmasi
eksplisit bahwa rumpun ini tidak bersinggungan dengan satu pun berkas yang dirancang
rumpun-rumpun tersebut.

**Enam task backend baru** (`BE-BKC-033`–`038`, `backend-roadmap.md` § Amendment 7 September 2026
kedua) dan **lima task frontend baru** (`FE-BKC-023`–`027`, `frontend-roadmap.md` § Amendment yang
sama) — seluruhnya **`READY_FOR_TASK_APPROVAL`**, dua gelombang backend (`MVP-13` fondasi,
`MVP-14` alur pertama) diikuti satu gelombang frontend (`MVP-15`). Tidak ada satu pun task yang
`BLOCKED` — blueprint ini tidak melahirkan satu pun pertanyaan terbuka bertanda memblokir.

Satu prasyarat non-blocking tersisa: **`PC-OQ-003`** (baris registry kepemilikan modul untuk
folder `PettyCash/`, `QBE-MOD-003`) dicatat sebagai dependency tingkat task pada `BE-BKC-033` —
task yang menulis berkas model persisted pertama — bukan sebagai roadmap blocker. Ia menahan
langkah pertama `build-module-backend`, bukan approval perencanaan ini.

Rincian lengkap: [backend § Amendment 7 September 2026 (kedua)](./backend-roadmap.md), [frontend §
Amendment 7 September 2026 (kedua)](./frontend-roadmap.md), dan [traceability § Amendment 7
September 2026 (kedua)](./requirement-traceability.md).

