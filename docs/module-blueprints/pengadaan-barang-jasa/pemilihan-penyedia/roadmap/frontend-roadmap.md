# Pemilihan Penyedia — Roadmap Frontend

| Field | Nilai |
| --- | --- |
| Blueprint | `PBJ-BP-001` revisi 2 · sub-modul `pemilihan-penyedia` (`approved`, `PBJ-DEC-095`) |
| Revisi roadmap | `1` · status `draft` |
| Tanggal | 26 November 2026 |
| Kontrak | set `1.0` (`PBJ-PS-API`, `-PERM`, `-STATE`, `-VAL`); `03-frontend-architecture.md` |
| Kewenangan UI | `PBJ-FE-001` (nama menu SOURCE; **nama 11 submenu menu 4 dan 6 menunggu `PBJ-OQ-038`**), `PBJ-FE-004` (standar layar daftar), `PBJ-FE-003` (invariant: harga tertutup, tombol tersembunyi bukan pengganti cek server); rupa lain `PBJ-FE-002` `DEV_DISCRETION` |
| SHA | FE `3e5386679` (branch `sukmagpV2`), BE `45b22777` |
| Repository target | `V2QuilvianSystemFrontendDev` |

**Kebijakan uji frontend:** bukti utama verifikasi manual kontrol interaktif (Playwright dari scratchpad, aktor
non-SuperAdmin), lint, dan build. Test unit opsional. Daftar `-01`, `-04`, `-06`, `-09` wajib lolos pemeriksaan baris filter
satu baris (`AT-PS-65`). Warna memakai token `--domain-*` di `globals.css`, bukan nilai literal.

**Kontrak dikunci:** frontend boleh mulai paralel dengan backend hanya untuk kontrak set `1.0`. Bila respons nyata berbeda,
kontrak yang direvisi, bukan frontend yang menyesuaikan diam-diam.

**Label menu sementara.** Sampai `PBJ-OQ-038` dijawab, butir menu memakai label **usulan** pada
`03-frontend-architecture.md` bagian 3. Pemetaan final 11 submenu SOURCE adalah task tersendiri (`FE-PBJ-PS-008`), sehingga
task lain tidak tertahan oleh label.

## Legenda tanda status

| Tanda | Arti |
| :---: | --- |
| ✅ | Selesai; terbukti di laporan task |
| 🟡 | Sebagian |
| ⛔ | Terblokir; nama blocker disebut |
| tanpa tanda | Belum disentuh |

## Grafik Urutan Dependency

Empat blok. Node yang muncul lagi di blok berikutnya (`FE-PBJ-PS-001`, `004`, `005`, `007`) adalah node yang sama
(sambungan), bukan task baru.

Blok 1 — menu, Pengaturan, evaluasi teknis:

```text
BE-PBJ-PS-002 ✅ [BE] ─> FE-PBJ-PS-001 ─┬─> FE-PBJ-PS-005
                                        │
BE-PBJ-PS-011 [BE] ─────────────────────┘
```

Blok 2 — paket, persiapan, RFQ dan penawaran:

```text
FE-PBJ-PS-001 ─────────┐
                       │
BE-PBJ-PS-005 ✅ [BE] ─┴─> FE-PBJ-PS-002 ─┐
                                          │
BE-PBJ-PS-008 ✅ [BE] ────────────────────┴─> FE-PBJ-PS-003 ─┐
                                                             │
BE-PBJ-PS-010 [BE] ──────────────────────────────────────────┴─> FE-PBJ-PS-004
```

Blok 3 — perbandingan, penetapan, nomor PO:

```text
FE-PBJ-PS-004 ──────┐
                    │
BE-PBJ-PS-013 [BE] ─┴─> FE-PBJ-PS-006 ─┐
                                       │
BE-PBJ-PS-015 [BE] ────────────────────┴─> FE-PBJ-PS-007
```

Blok 4 — pemetaan final submenu:

```text
FE-PBJ-PS-005 ─┐
               │
FE-PBJ-PS-007 ─┴─┐
                 │
{PBJ-OQ-038 ⛔} ─┴─> FE-PBJ-PS-008 ⛔
```

`[BE]` = task backend pada `backend-roadmap.md`, cermin baca-saja. `{PBJ-OQ-038}` = keputusan nama 11 submenu SOURCE menu 4
dan 6 (BRD B10), pemilik pengguna sesi.

Jumlah pasangan prasyarat → task: **16** (blok 1: 3, blok 2: 6, blok 3: 4, blok 4: 3), sama dengan isi kolom `Dependency`.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `BE-PBJ-PS-002` ✅ | `FE-PBJ-PS-001` |
| 2 | `FE-PBJ-PS-001`, `BE-PBJ-PS-005` ✅ | `FE-PBJ-PS-002` |
| 2 | `FE-PBJ-PS-001`, `BE-PBJ-PS-011` | `FE-PBJ-PS-005` — menunggu `BE-PBJ-PS-011` ✅ |
| 3 | `FE-PBJ-PS-002`, `BE-PBJ-PS-008` ✅ | `FE-PBJ-PS-003` |
| 4 | `FE-PBJ-PS-003`, `BE-PBJ-PS-010` | `FE-PBJ-PS-004` |
| 5 | `FE-PBJ-PS-004`, `BE-PBJ-PS-013` | `FE-PBJ-PS-006` |
| 6 | `FE-PBJ-PS-006`, `BE-PBJ-PS-015` | `FE-PBJ-PS-007` |
| — | ⛔ menunggu keputusan `PBJ-OQ-038` (pengguna sesi), `FE-PBJ-PS-005`, `FE-PBJ-PS-007` | `FE-PBJ-PS-008` |

Gelombang dihitung dari prasyarat frontend; prasyarat `[BE]` menahan mulai sampai task backend-nya ✅.

## Tabel task

| Task ID | Outcome | Layar | Kontrak / izin | Reuse | Dependency | Acceptance criteria | Verifikasi | Status |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-PBJ-PS-001` | Butir menu sub-modul, layanan API, tiga layar Pengaturan | `-12`, `-13`, `-14`; menu | API §Master Data (3 grup); `ProcurementMethod`, `ProcurementPackageDocumentType`, `ProcurementPackageDocumentRequirement` | Pola master `settings/*` (`FE-PBJ-VD-001`); `procurement-list-filter` | `BE-PBJ-PS-002` | Menu tampil sesuai izin (label usulan); tiga master CRUD; `UAT-PS-01`, `02` | Lint, build, Playwright | — |
| `FE-PBJ-PS-002` | Daftar, pembuatan, dan kerangka detail paket | `-01`, `-02`, `-03` (kepala, tab Persiapan baca, Riwayat) | API §Procurement Package (daftar, buat, baris, detail, riwayat); `ProcurementPackage : Read, Create, Update` | `DataTable`, `procurement-list-filter`, `procurement-document-tabs`, `procurement-history-panel` | `FE-PBJ-PS-001`, `BE-PBJ-PS-005` | `AT-PS-01`, `02` (layar), `65` untuk `-01`; HPS "—" tanpa `ReadPrice` | Playwright Tim Pengadaan, Keuangan | — |
| `FE-PBJ-PS-003` | Persiapan dapat disusun, diajukan, dan diputus | Tab Persiapan, Lampiran, Persetujuan `-03`; `-11` | API `PUT /{id}`, `method-suggestion`, `criteria`, `evaluators`, `preparation/*`, `approval`; izin `Submit`, `Revise`; `ApprovalInbox : *` | `procurement-attachment-panel`, `procurement-approval-panel`, Antrean Persetujuan (`PBJ-DEC-058`) | `FE-PBJ-PS-002`, `BE-PBJ-PS-008` | `AT-PS-11`, `12`, `13`, `15`, `16` (layar); lampiran harga tidak tampil tanpa `ReadPrice` | Playwright pengaju dan penyetuju | — |
| `FE-PBJ-PS-004` | RFQ dikirim dan penawaran dicatat | `-04`, `-05`; tab RFQ dan Penawaran `-03` | API §Procurement RFQ, §Procurement Quotation (catat); `ProcurementRfq : *`, `ProcurementQuotation : Create, Update, Cancel` | Pola detail + form baris | `FE-PBJ-PS-003`, `BE-PBJ-PS-010` | `AT-PS-21`, `22`, `23`, `24`, `25` (layar), `65` untuk `-04` | Playwright Tim Pengadaan | — |
| `FE-PBJ-PS-005` | Evaluator menilai tanpa melihat harga | `-06`, `-07` | API §Technical Evaluation; `ProcurementTechnicalEvaluation : Read, Evaluate` | `procurement-attachment-panel` (baca) | `FE-PBJ-PS-001`, `BE-PBJ-PS-011` | `AT-PS-30` (layar), `31`, `33`; `65` untuk `-06`; `UAT-PS-07`, `08` | Playwright evaluator dan pencatat; respons jaringan diperiksa | — |
| `FE-PBJ-PS-006` | Perbandingan, negosiasi, dan penetapan | `-08`, `-09`, `-10`; tab Penetapan `-03`; jenis penetapan di `-11` | API `comparison`, `negotiations`, §Package Award; `ProcurementQuotation : Negotiate`; `ProcurementPackageAward : *` | `procurement-approval-panel` | `FE-PBJ-PS-004`, `BE-PBJ-PS-013` | `AT-PS-34`, `40`, `41`, `55` (layar), `65` untuk `-09`; `UAT-PS-09`, `10` | Playwright Tim Pengadaan, Manajer, penyetuju, Keuangan | — |
| `FE-PBJ-PS-007` | Nomor PO, pecah, batal; unit peminta melihat paketnya | Aksi `-03`; panel `-15` di Detail PR; label "termasuk PPN" di layar PR dan pagu | API `purchase-order`, `split`, `cancel`, `request-summary`; `ProcurementPackage : RecordOrder, Cancel, ReadSummary` | Layar PR dan pagu `perencanaan-permintaan` | `FE-PBJ-PS-006`, `BE-PBJ-PS-015` | `AT-PS-50`, `52`, `53`, `54` (layar); `UAT-PS-11`..`14`; regresi layar PR | Playwright Tim Pengadaan, Unit Peminta | — |
| `FE-PBJ-PS-008` ⛔ | Pemetaan final 11 submenu SOURCE menu 4 dan 6 | Menu | `PBJ-FE-001` | `menu-items.jsx` | `{PBJ-OQ-038}`, `FE-PBJ-PS-005`, `FE-PBJ-PS-007` | `AT-PS-63` (setiap layar terjangkau dari menu atau layar induk dengan label final) | Playwright per butir menu | ⛔ menunggu keputusan `PBJ-OQ-038` |

## Kartu task

### `FE-PBJ-PS-001` — Menu dan Pengaturan pemilihan

| Aspek | Isi |
| --- | --- |
| Cakupan | Butir menu "4. Persiapan Pengadaan" dan "6. Penawaran & Pemilihan Penyedia" dengan label submenu **usulan** (`03-frontend-architecture.md` bagian 3), butir "Persetujuan Paket" di menu 11, tiga butir Pengaturan; service `package-service.js`, `rfq-service.js`, `quotation-service.js`, `technical-evaluation-service.js`, `award-service.js`, `sourcing-master-data-service.js`; konstanta izin, status paket/RFQ/penetapan, jenis dokumen `PBJ_PACKAGE`/`PBJ_PACKAGE_AWARD`/`PBJ_RFQ`; `procurement-status-badge` ditambah `kind` paket, RFQ, penetapan; layar `-12`..`-14` (kartu editor di atas daftar) |
| Acceptance criteria | (1) Butir menu tampil hanya bagi pemegang izin baca masing-masing. (2) Tiga master CRUD dengan baris filter `PBJ-FE-004`. (3) `-12` menolak rentang tumpang tindih dengan pesan dari server. (4) `-14` memaksa pilih metode **atau** jenis pengadaan. (5) Hapus master terpakai menampilkan "sudah dipakai, nonaktifkan saja" (`UAT-PS-02`) |
| DoD | Kriteria + laporan `task/report/frontend/FE-PBJ-PS-001.md` |
| **Status** | Belum dikerjakan |

### `FE-PBJ-PS-002` — Daftar, buat, dan detail paket

| Aspek | Isi |
| --- | --- |
| Cakupan | `-01` daftar (hero tanpa tombol, baris filter: tanggal awal, tanggal akhir, periode, jenis pengadaan, tahun anggaran, status, cari, jumlah baris, muat ulang, "+ Buat Paket"); `-02` pilih baris PR siap paket dan rencana paket rujukan; `-03` kepala paket, tab Persiapan (baca: barang, jumlah, asal PR, HPS), tab Riwayat; tambah dan lepas baris PR di `DRAFT` |
| Acceptance criteria | (1) `AT-PS-01` paket kursi roda 15 unit dari dua PR terbentuk dari layar. (2) 409 `PBJ-VAL-121` tampil "Baris … sudah masuk paket lain" lalu daftar dimuat ulang (`AT-PS-02`). (3) Pengguna tanpa `ReadPrice` melihat HPS "—", bukan 0. (4) `AT-PS-65` untuk `-01` |
| DoD | Kriteria + laporan |
| **Status** | Belum dikerjakan |

### `FE-PBJ-PS-003` — Persiapan dan persetujuannya

| Aspek | Isi |
| --- | --- |
| Cakupan | Tab Persiapan `-03` dapat diubah (spesifikasi, HPS satuan, metode + saran + alasan di luar rentang, checklist ✓/✗, evaluator, kriteria teknis); tab Lampiran (`PBJ_PACKAGE`, kategori dari daftar jenis dokumen); tombol Ajukan, Tarik, Buka ulang persiapan; tab Persetujuan (linimasa); `-11` Persetujuan Paket di Antrean Persetujuan (jenis `PBJ_PACKAGE`, siap menerima `PBJ_PACKAGE_AWARD`); sinkron ulang |
| Acceptance criteria | (1) Saran metode tampil dari HPS (`AT-PS-11`). (2) Metode di luar rentang tanpa alasan → galat di isian (`AT-PS-12`). (3) Jasa tanpa KAK/TOR → daftar dokumen kurang (`AT-PS-13`, `UAT-PS-04`). (4) Disetujui, dikembalikan, ditolak terbaca di layar (`AT-PS-15`, `UAT-PS-03`). (5) Pengaju tidak mendapat tugas persetujuan (`AT-PS-16`). (6) Lampiran Dasar HPS tidak tampil bagi pengguna tanpa `ReadPrice` |
| Verifikasi | Playwright pengaju dan penyetuju dengan matriks `Wfl` uji yang dicabut sesudahnya |
| DoD | Kriteria + laporan |
| **Status** | Belum dikerjakan |

### `FE-PBJ-PS-004` — RFQ dan pencatatan penawaran

| Aspek | Isi |
| --- | --- |
| Cakupan | `-04` daftar RFQ (saringan status, paket); `-05` undangan dari vendor layak (alasan wajib untuk Dibatasi), Kirim, Perpanjang batas waktu, Tutup, Nyatakan gagal, Unduh RFQ; form catat penawaran (vendor, waktu diterima, PPN%, harga per barang, lampiran); Batalkan catatan; tab RFQ dan Penawaran di `-03` |
| Acceptance criteria | (1) Vendor tidak layak tidak muncul di pilihan (`AT-PS-21`). (2) Dibatasi tanpa alasan → galat (`AT-PS-22`). (3) Penawaran terlambat bertanda "Terlambat — tidak sah" (`AT-PS-23`, `UAT-PS-06`). (4) Perpanjang setelah lewat → galat `PBJ-VAL-132` (`AT-PS-24`). (5) Harga akhir termasuk PPN tampil dari server (`AT-PS-25`). (6) Tiga penawaran sah tercatat (`UAT-PS-05`). (7) `AT-PS-65` untuk `-04` |
| DoD | Kriteria + laporan |
| **Status** | Belum dikerjakan |

### `FE-PBJ-PS-005` — Evaluasi teknis

| Aspek | Isi |
| --- | --- |
| Cakupan | `-06` antrean paket yang ditugaskan (tanda "ditugaskan sebagai evaluator", `PBJ-DEC-088`); `-07` per barang: spesifikasi, kriteria, per vendor lampiran teknis, Lulus/Tidak lulus + alasan; **tanpa kolom harga sama sekali** |
| Acceptance criteria | (1) Tidak ada angka harga di layar maupun di respons jaringan (`AT-PS-31`, `UAT-PS-07`). (2) Pencatat tidak melihat tombol menilai penawarannya; dipaksa lewat API → 403 `PBJ-VAL-135` tampil sebagai pesan (`AT-PS-30`, `UAT-PS-08`). (3) Tidak lulus tanpa alasan → galat (`AT-PS-33`). (4) `AT-PS-65` untuk `-06` |
| DoD | Kriteria + laporan |
| **Status** | Belum dikerjakan |

### `FE-PBJ-PS-006` — Perbandingan, negosiasi, penetapan

| Aspek | Isi |
| --- | --- |
| Cakupan | `-08` perbandingan (peringkat, sah, lulus, layak, total akhir, selisih terhadap HPS, ★ terendah), negosiasi per putaran dengan berita acara; `-10` ajukan penetapan (alasan bukan terendah, di atas HPS, vendor Dibatasi), detail penetapan (status, peringkat tersimpan, linimasa, Tarik, Cabut bagi Manajer); `-09` daftar penetapan (saringan status, tahun); tab Penetapan `-03`; jenis penetapan di `-11` |
| Acceptance criteria | (1) Harga negosiasi naik → galat (`AT-PS-34`). (2) Di atas HPS tanpa alasan → galat; dengan alasan → langkah tambahan tampil di linimasa (`AT-PS-40`). (3) Bukan terendah tanpa alasan → galat (`AT-PS-41`, `UAT-PS-10`). (4) Disetujui → paket "Ditetapkan" (`UAT-PS-09`). (5) Cabut hanya tampil bagi pemegang `Revoke` (`AT-PS-55`). (6) Keuangan melihat nilai penetapan di `-09` tanpa rincian penawaran. (7) `AT-PS-65` untuk `-09` |
| DoD | Kriteria + laporan |
| **Status** | Belum dikerjakan |

### `FE-PBJ-PS-007` — Nomor PO, pecah, batal, dan panel di Detail PR

| Aspek | Isi |
| --- | --- |
| Cakupan | Aksi `-03`: Catat No. PO, Pecah paket (pilih item, judul baru, alasan), Batalkan; panel `-15` di Detail PR (`perencanaan-permintaan`): nomor paket, status, vendor terpilih per baris, tanpa harga; label "termasuk PPN" pada harga perkiraan PR dan nilai pagu (`PBJ-DEC-084`) |
| Acceptance criteria | (1) Nomor PO ganda → pesan menyebut paket pemakainya (`AT-PS-50`, `UAT-PS-12`). (2) Paket "Sudah dipesan" dan unit peminta melihat nomor paket dan vendor (`UAT-PS-11`). (3) Batal `AWARDED` → PR kembali dapat dipaketkan (`AT-PS-52`, `UAT-PS-14`). (4) Pecah memindahkan tongkat beserta penawarannya (`AT-PS-53`, `UAT-PS-13`). (5) Pecah saat penetapan diajukan → pesan `PBJ-VAL-144` (`AT-PS-54`). (6) Regresi layar PR dan pagu |
| Risiko / pemilik | Menyentuh layar `perencanaan-permintaan` (`PBJ-DEC-095`) |
| DoD | Kriteria + regresi + laporan |
| **Status** | Belum dikerjakan |

### ⛔ `FE-PBJ-PS-008` — Pemetaan final submenu

| Aspek | Isi |
| --- | --- |
| Cakupan | Mengganti label usulan dengan nama 11 submenu SOURCE menu 4 dan 6, dan memetakan tiap butir ke layar `-01`, `-03`, `-04`, `-06`, `-08`, atau `-09` dengan saringan atau tab awal yang sesuai |
| Acceptance criteria | `AT-PS-63`: setiap layar `-01`..`-15` terjangkau dari butir menu berlabel final atau dari layar induknya; tangkapan layar per butir |
| DoD | Kriteria + laporan |
| **Status** | ⛔ **TERBLOKIR.** Menunggu keputusan `PBJ-OQ-038` dari pengguna sesi (nama 11 submenu, BRD B10), serta `FE-PBJ-PS-005` dan `FE-PBJ-PS-007` |

## Tidak masuk roadmap ini

- Laporan "direncanakan vs dipaketkan" — `POST-MVP`.
- Notifikasi aktif (email) — menunggu `PBJ-OQ-015`; selama MVP cukup tugas `Wfl` dan tanda di daftar (`PBJ-DEC-088`).
- Portal vendor — tidak dibuat (`PBJ-DEC-067`).
