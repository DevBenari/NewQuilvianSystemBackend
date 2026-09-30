# Laporan Perubahan Backend — `BE-ACC-P2-028`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-028` |
| Judul | Jalur pesan saldo subledger |
| Slice | Wave D-2 — saldo subledger |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-028` (revisi 4, `APPROVED` 24 September 2026) |
| Trace | `ACC-DEC-071`, `087`, `093` (belum ada FR — coverage gap); sisi Finance `FIN-DEC-035` |
| Contract version | `ACC-API-0.12` bidang `SubledgerBalance`; `ACC-VALIDATION-0.8` baris pesan saldo; `ACC-STATE-0.4` status `Tercatat`; `02-backend-architecture.md` bagian 22.4 langkah 1 dan 5b, 22.6, 22.7 — seluruhnya `approved` (`GATE-DESAIN-0924`) |
| Dependency | `BE-ACC-P2-021` ✅; `BE-ACC-P2-027` ✅ — tabel `AccSubledgerBalance` diterapkan 28 September 2026 |
| Klasifikasi | `MEDIUM` — skor 5: berkas diperiksa 9–20 (1), berkas diubah 4–8 termasuk dokumen (1), logika bisnis sedang (1), memakai kontrak API yang ada (1), perilaku persistence pada tabel yang sudah ada (1); repository, keamanan, dan UI masing-masing 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — service, DTO, hosted service kotak masuk kejadian; laporan ini; baris status roadmap dan traceability. **Tidak** termasuk migration (tidak dibutuhkan), build, maupun commit |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `f06d487f` (branch `rizkiG`), perubahan belum di-commit |
| Tanggal | 28 September 2026 |
| Status | **✅ SELESAI — 28 September 2026.** 6 dari 6 acceptance terpetakan ke source; uji panggil pesan tiruan oleh Rizki lewat Swagger dan layar (bagian 5.3) membuktikan (1), (2), (5), (6), serta pemrosesan ulang Tertahan → Tercatat dan Tertahan → Gagal di runtime. Bagian yang **hanya terbukti lewat source**: (3) penggantian baris menurut versi, (4) periode `Closed`, akun yang ada tetapi bukan control, dan regresi jenis Transaksi yang membawa rincian saldo — rinciannya di bagian 5.3. Build terbukti tidak langsung (backend menjalankan perilaku `028`); jumlah warning tidak dilaporkan. Skenario A1 dinyatakan lulus atas keputusan Rizki. UAT belum dijalankan. **Riwayat:** 🟡 pada hari yang sama |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `AccountingEvent`; membaca `Reconciliation` (`AccSubledgerBalance`), `AccountingPeriod`, `MasterData/ChartOfAccount` |
| Registry | `Acc` — `ACTIVE`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 39. Salinan registry suite skill v1.17.1 tidak memuat `Acc`; registry repository yang berlaku (`ACC-TD-015`) |
| Keberlakuan | `TOUCHED LEGACY` pada `AccAccountingEventService`, `AccountingEventDtos`, `AccAccountingEventSchedulerHostedService` — seluruhnya buatan Wave B (`BE-ACC-P2-021`, `023`), bukan legacy lama. Nol entity, nol configuration, nol endpoint baru |
| QBE yang berlaku | `QBE-SVC-001` (logika di service; controller tidak disentuh), `QBE-API-001` (bentuk `ApiResponse<AccountingEventReceiptDto>` dan kode status tetap), `QBE-VAL-001` (lima aturan pesan saldo), `QBE-TXN-001` (perpindahan status dan penulisan saldo dalam satu transaksi), `QBE-DTO-001`, `QBE-PERM-001` (hak `AccountingEvent : Receive` tidak berubah), `QBE-AUD-001`. Tidak berlaku: `QBE-CODE-*`, `QBE-ENT-*`, `QBE-NAM-*` |
| Governance yang dibaca | `AGENTS.md` backend; `rules/backend/` suite (`TASK_RULES`, `API_RULES`, `DATABASE_RULES`, `TASK_CLASSIFICATION`, `REVIEW_RULES`, `REPORT_TEMPLATE`); `BACKEND_ENGINEERING_CONTRACT.md`; registry |

## 1. Masalah yang diperbaiki

Sejak `BE-ACC-P2-021`, pesan berjenis **Saldo Subledger** ditolak sementara `409` "Pesan saldo
subledger belum dapat diterima. Jalurnya dibangun pada BE-ACC-P2-028." Finance karena itu tidak
dapat menyampaikan saldo akhir periode, dan tabel `AccSubledgerBalance` (`BE-ACC-P2-027`) tidak
pernah terisi. Task ini membuka jalurnya: pesan saldo yang sah **disimpan sebagai saldo
rekonsiliasi tanpa jurnal**, berstatus `Tercatat`.

## 2. Proses bisnis

1. Pada tutup periode, Finance mengirim satu pesan per akun kontrol. **Contoh:** saldo piutang
   penjamin September 2026 Rp 424.500.000, dengan `EventTypeCode` `SALDO-SUBLEDGER`,
   `SourceVersion` `2`, `AccountingDate` `2026-09-30`, dan rincian `SubledgerBalance`
   `{ AccountingPeriodCode: "2026-09", ControlAccountCode: "1-1201" }`.
2. **Sebelum** kejadian disimpan, Accounting memeriksa rinciannya: rincian wajib ada, tanpa
   `Components`, versi bilangan bulat positif, periode dikenal, dan akun adalah akun kontrol pada
   badan hukum itu. Pelanggaran dijawab `400` dan tidak ada yang tersimpan.
3. Kejadian disimpan (`Diterima`), lalu diproses: **tidak ada aturan posting yang dicari dan tidak ada
   jurnal**. Di dalam satu transaksi, status berpindah `Diterima` → `Tercatat` secara bersyarat, dan
   baris saldo ditulis:
   - belum ada baris untuk pasangan (badan hukum, periode, akun) → baris baru;
   - sudah ada dan versi pesan **lebih tinggi** → angka pada baris yang sama diganti;
   - sudah ada dan versi **lebih rendah atau sama** → baris tidak berubah;
   - periode berstatus **`Closed`** → baris tidak berubah.
4. Finance menerima `201` beserta tanda terima: `EventStatus` `Tercatat`, `JournalNumber` kosong,
   `AccountingPeriodCode` = periode rincian.
5. Pesan saldo yang **jenisnya belum terdaftar** tetap tersimpan `Tertahan`
   (`EVENT_TYPE_NOT_REGISTERED`). Begitu jenisnya didaftarkan dengan perlakuan Saldo Subledger dan
   petugas menekan Coba Ulang, rinciannya dibaca ulang dari pesan asli. Bila sah → `Tercatat`; bila
   tidak sah → `Gagal` beserta alasannya di riwayat percobaan.

**Contoh urutan versi.** Finance mengirim versi 2 (Rp 424.500.000) lebih dulu, lalu versi 1
(Rp 425.000.000) terlambat tiba. Keduanya `Tercatat`, tetapi baris saldo tetap Rp 424.500.000 dan
menunjuk kejadian versi 2. Koreksi versi 3 sebesar minus Rp 1.500.000 kemudian mengganti baris itu.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk apa |
|---|---|
| `02-backend-architecture.md` bagian 22.4–22.7 | Alur langkah 1 dan 5b, aturan penyimpanan saldo, isi tanda terima |
| `contracts/validation-matrix.md` baris 204–209 | Lima aturan pesan saldo beserta teks pesannya |
| `contracts/state-transition-matrix.md` tambahan `ACC-STATE-0.4` | Perpindahan ke `Tercatat` dan `Gagal` |
| `contracts/api-contract.md` bagian `SubledgerBalance` | Bidang dan aturan yang mengikat |
| `erd/data-dictionary.md` bagian 12d | Kolom `AccSubledgerBalance` |
| `AccAccountingEventService.cs` (seluruh berkas) | Alur penerimaan, pemrosesan bersama, penjadwal, tanda terima, validasi |
| `AccountingEventDtos.cs`, `AccAccountingEventSchedulerHostedService.cs` | Rincian request, hasil siklus penjadwal |
| `AccAccountingPeriod.cs`, `AccountingPeriodStatus.cs`, `AccChartOfAccount.cs` | Nama kolom `PeriodCode`, `PeriodStatus.Closed`, `AccountCode`, `IsControlAccount` |
| Frontend `accounting-event-constants.jsx` (read-only) | Label status `6` = "Tercatat" sudah ada |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/AccountingManagement/AccountingEvent/Services/AccAccountingEventService.cs` | **(a)** `PeriksaIsianMenurutJenis`: penolakan sementara `409` dicabut; aturan "rincian saldo hanya dan wajib ada pada pesan saldo" kini dua arah. **(b)** `TerimaAsync`: jenis Saldo Subledger menjalankan `PeriksaRincianSaldoAsync` sebelum kejadian disimpan → `400`. **(c)** `ProsesKejadianAsync`: cabang langkah 5b — jenis Saldo Subledger langsung ke `CatatSaldoAsync`, tanpa aturan posting. Karena `ProsesKejadianAsync` dipakai penerimaan, Coba Ulang manual, dan penjadwal, ketiganya mendapat jalur yang sama. **(d)** Method baru `CatatSaldoAsync`, `TandaiGagalSaldoAsync`, `PeriksaRincianSaldoAsync`, `BacaRincianSaldo`, record `RincianSaldoTerperiksa`. **(e)** `PetakanTandaTerimaAsync`: `AccountingPeriodCode` untuk kejadian `Tercatat` diambil dari rincian pesan (bagian 22.7). **(f)** `BalasKejadianBaru` dan `CobaUlangAsync`: pesan untuk `Tercatat`. **(g)** Penjadwal menghitung `Tercatat` dan `Gagal` |
| `Areas/Corporate/AccountingManagement/AccountingEvent/DTOs/AccountingEventDtos.cs` | `AccountingEventRetryCycleResult.Recorded` — jumlah pesan saldo yang tercatat pada satu siklus penjadwal |
| `Areas/Corporate/AccountingManagement/AccountingEvent/Services/AccAccountingEventSchedulerHostedService.cs` | Log siklus bertambah `Tercatat={Tercatat}` |

Total +277 / −21 baris source. Nol perubahan pada controller, `Program.cs`, entity, configuration,
dan `Migrations/`.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
|---|---|
| Kontrak API | Route dan bentuk DTO tidak berubah. **Perilaku** `POST /accounting-events` untuk jenis Saldo Subledger berubah dari `409` sementara menjadi sesuai `ACC-API-0.12`: `201` + `Tercatat`, atau `400` untuk lima pelanggaran. `AccountingEventReceiptDto.AccountingPeriodCode` kini terisi untuk pesan saldo |
| Database | Menulis dan mengganti baris `AccSubledgerBalance`; memperbarui `AccAccountingEvent` dan menambah `AccAccountingEventAttempt`. **Nol migration** |
| Keamanan/Auth | Hak akses tidak berubah (`AccountingEvent : Receive` untuk penerimaan, `AccountingEvent : Retry` untuk Coba Ulang). Nilai saldo tidak ditulis ke log |

### 3.4 Keputusan implementasi yang perlu diketahui

| Hal | Yang dipilih | Alasan |
|---|---|---|
| Letak cabang saldo | Di dalam `ProsesKejadianAsync`, tepat sesudah jenis ditemukan | Satu titik untuk tiga pemanggil: penerimaan, Coba Ulang manual, penjadwal. Mewujudkan `Tertahan → Tercatat`, `Tertahan → Gagal`, dan `Gagal → Tercatat` tanpa kode tambahan |
| Sumber rincian saat diproses | Dibaca dari `RawPayload` | Kejadian tersimpan tidak punya kolom rincian (`ACC-DEC-087`: `SubledgerBalance` tidak diulang). Validasi diulang saat diproses, sesuai 22.4 langkah 5b |
| Urutan di dalam transaksi | Status `Diterima` → `Tercatat` bersyarat **lebih dulu**, baru baris saldo | Bila kejadian sudah diproses pihak lain (penjadwal atau request lain), transaksi dibatalkan sebelum baris saldo tersentuh |
| Penggantian baris | `ExecuteUpdate ... WHERE SourceVersionNumber < @versi` | Atomik di database. Dua versi yang diproses bersamaan tidak dapat menurunkan versi yang berlaku |
| Dua pesan baru untuk pasangan yang sama tiba bersamaan | Unique index `027` menolak baris kedua; transaksi batal, percobaan gagal dicatat, status tetap `Diterima`; penjadwal mencoba ulang dan kali itu masuk cabang "sudah ada" | Memakai mesin coba ulang yang sudah ada; nol aturan baru |
| Periode `Closed` | Hanya status `Closed`, bukan `SoftClosed`/`PendingClosingApproval` | Bunyi bagian 22.6 dan `ACC-DEC-093` |
| `AccountingDate` pesan saldo terhadap periode rincian | **Tidak** diperiksa | Tidak ada aturannya di `ACC-VALIDATION-0.8`; tidak dikarang |
| Validasi `IsActive` akun kontrol | **Tidak** diperiksa | Aturan hanya menyebut "tidak ada, beda badan hukum, atau bukan control account" |
| Versi bilangan bulat | `int.TryParse` dengan `NumberStyles.None`, harus > 0 | Menolak tanda, spasi, desimal, dan nol |
| Komentar kode | Nol baris `//` | Arahan owner |

## 4. Dokumentasi endpoint

Endpoint tidak bertambah. Perilaku endpoint yang sudah ada berubah untuk pesan saldo:

#### Corporate - Accounting - Accounting Event

| Method | Path | Kegunaan | Hak akses |
|---|---|---|---|
| `POST` | `api/v1/corporate/accounting/accounting-events` | Menerima satu kejadian; pesan berjenis Saldo Subledger kini dicatat sebagai saldo rekonsiliasi | `AccountingEvent : Receive` |
| `POST` | `api/v1/corporate/accounting/accounting-events/{id}/retry` | Coba Ulang — kini juga memproses pesan saldo `Tertahan` atau `Gagal` | `AccountingEvent : Retry` |

| Kode | Kapan, untuk pesan saldo |
|---|---|
| `201` | Pesan saldo sah — `EventStatus` `Tercatat`, `JournalNumber` kosong, `AccountingPeriodCode` = periode rincian |
| `200` | Kiriman ulang pesan yang sama — tanda terima keadaan terkini |
| `400` | Rincian kosong, membawa `Components`, versi bukan bilangan bulat positif, periode tidak dikenal, atau akun bukan akun kontrol |
| `422` | Kode jenis belum terdaftar → `Tertahan` (`EVENT_TYPE_NOT_REGISTERED`) |

## 5. Verifikasi

### 5.1 Yang sudah dijalankan agent

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
|---|---|---|---|
| `git diff --stat` | Tiga berkas source, +277/−21 | `PASS` | Bagian 7 |
| `git ls-files --eol` | Ketiga berkas `i/lf w/lf` — sama dengan sebelum diubah | `PASS` | — |
| Penelusuran `//` pada baris baru | Nol | `PASS` | — |
| Penelusuran teks `409` sementara "Jalurnya dibangun pada BE-ACC-P2-028" | Nol — sudah dicabut | `PASS` | — |
| Pemetaan acceptance terhadap source | 6/6 | `PASS` | Bagian 6 |
| Label status `6` di frontend | Sudah ada: "Tercatat" | `PASS` | `accounting-event-constants.jsx` baris 9 |
| Build backend (Rizki) | Terbukti **tidak langsung**: backend yang diuji menjalankan perilaku task ini (pesan saldo → `201` `Tercatat`, sebelumnya `409`). Jumlah warning tidak dilaporkan | `PASS` | Bagian 5.3 |
| Uji panggil Swagger dan layar (Rizki) | Dijalankan 28 September 2026, 12.49–13.37 WIB | `PASS` dengan catatan | Bagian 5.3 |
| Automated test | Bukan acceptance (`ACC-DEC-081`) | `NOT RUN` | — |

### 5.2 Skenario uji untuk Rizki

**Persiapan data, lewat layar — bukan SQL.**

1. Daftarkan jenis kejadian uji **`UJI-SALDO-028`** dengan Jenis Perlakuan **Saldo Subledger**
   (layar Jenis Kejadian, `FE-ACC-P2-013`).
2. Catat satu **kode akun kontrol** (layar COA, kolom Control) dan satu **akun biasa** pada badan
   hukum utama.
3. Pastikan periode **`2026-09`** ada dan belum `Closed`.

Setiap pesan memakai amplop dua belas bidang seperti biasa, `SourceModule` `Finance`,
`SourceTransactionId` `UJI-SALDO-028-2026-09`, **tanpa** `Components`, dengan rincian:

```json
"SubledgerBalance": { "AccountingPeriodCode": "2026-09", "ControlAccountCode": "<kode akun kontrol>" }
```

| # | Kirim | Hasil yang diharapkan | Membuktikan |
|---:|---|---|---|
| 1 | `EVT-UJI-128A`, `SourceVersion` `2`, `Amount` `424500000` | `201`, `EventStatus` `Tercatat`, `JournalNumber` `null`, `AccountingPeriodCode` `2026-09` | Acceptance (1) |
| 2 | `EVT-UJI-128B`, `SourceVersion` `1`, `Amount` `425000000` | `201`, `Tercatat` — **baris tidak berubah** | Acceptance (3), versi lebih rendah |
| 3 | `EVT-UJI-128C`, `SourceVersion` `3`, `Amount` `-1500000` | `201`, `Tercatat` — **baris diganti** menjadi −1.500.000 versi 3 | Acceptance (2) dan (3) |
| 4 | `EVT-UJI-128D`, `SourceVersion` `4`, `Amount` `0` | `201`, `Tercatat` | Acceptance (2), nilai nol |
| 5 | Ulangi skenario 1 persis | `200` "sudah pernah diterima" | Kiriman ulang |
| 6 | `EVT-UJI-128E`, tanpa `SubledgerBalance` | `400` "Rincian saldo subledger hanya dan wajib ada pada pesan saldo subledger." | Acceptance (6) |
| 7 | `EVT-UJI-128F`, dengan `Components` `[{ "ComponentCode": "TOTAL", "Amount": 1 }]` | `400` "Pesan saldo subledger tidak boleh membawa rincian komponen." | Acceptance (5) |
| 8 | `EVT-UJI-128G`, `SourceVersion` `2a` | `400` "Versi pesan saldo harus bilangan bulat positif." | Acceptance (5) |
| 9 | `EVT-UJI-128H`, `AccountingPeriodCode` `2099-01` | `400` "Periode akuntansi 2099-01 tidak dikenal untuk badan hukum ini." | Acceptance (5) |
| 10 | `EVT-UJI-128I`, `ControlAccountCode` = akun **biasa** | `400` "Akun … bukan akun kontrol pada badan hukum ini." | Acceptance (5) |
| 11 | `EVT-UJI-128J`, `EventTypeCode` `UJI-SALDO-028B` (belum terdaftar), rincian sah | `422`, `Tertahan`, `EVENT_TYPE_NOT_REGISTERED`. Daftarkan `UJI-SALDO-028B` sebagai Saldo Subledger, lalu tekan **Coba Ulang** di layar Rincian → `Tercatat` | Pemrosesan ulang pesan saldo yang tertahan |
| 12 | Hanya bila di dev **sudah ada periode `Closed`**: pesan saldo untuk periode itu | `201`, `Tercatat`, dan **tidak ada** baris saldo baru untuk periode itu | Acceptance (4). Bila tidak ada periode `Closed`, (4) dibuktikan lewat source (bagian 6) — keputusan Rizki |

**Melihat isi baris saldo.** Belum ada endpoint yang menampilkan `AccSubledgerBalance`;
layarnya datang bersama `BE-ACC-P2-014`. Untuk skenario 1–4, bila Rizki ingin melihat angkanya,
pakai kueri **baca saja** berikut di alat basis data — tidak mengubah data apa pun:

```sql
SELECT b."Balance", b."SourceVersionNumber", b."AsOfDate", e."EventNumber"
FROM "AccSubledgerBalance" b
JOIN "AccAccountingEvent" e ON e."Id" = b."AccountingEventId"
WHERE b."IsDelete" = false;
```

Sesudah skenario 3 barisnya harus −1.500.000 / versi 3 / `EVT-UJI-128C`; sesudah skenario 4,
0 / versi 4 / `EVT-UJI-128D`.

**Bersih-bersih sesudah uji, lewat layar.** Nonaktifkan jenis `UJI-SALDO-028` dan `UJI-SALDO-028B`.
Baris saldo uji di periode `2026-09` tetap ada; baris itu akan terbaca penghalang rekonsiliasi saat
`BE-ACC-P2-014` berdiri, jadi catat keberadaannya.

### 5.3 Hasil uji — Rizki, 28 September 2026

Bukti berupa tangkapan layar response Swagger mentah dan layar Kotak Masuk/Rincian yang dikirim
Rizki. Jenis `UJI-SALDO-028` didaftarkan **sesudah** A1 terkirim, sehingga urutan uji bergeser dari
rencana bagian 5.2.

| # | Kejadian | Hasil sebenarnya | Klasifikasi |
|---|---|---|---|
| A1 | `EVT-UJI-128A` v2 Rp 424.500.000 | `422` Tertahan `EVENT_TYPE_NOT_REGISTERED` (12.49) — jenisnya belum didaftarkan saat itu. Kiriman ulang 13.03 → `200`, tetap Tertahan. **Dinyatakan lulus atas keputusan Rizki**: perilaku yang dituju A1 terbukti lewat A2–A4 | `PASS` atas keputusan owner |
| A2 | `EVT-UJI-128B` v1 Rp 425.000.000 | `201` Tercatat, `journalNumber` `null`, `accountingPeriodCode` `2026-09`; kiriman ulang → `200` Tercatat dengan periode `2026-09` | `PASS` |
| A3 | `EVT-UJI-128C` v3 −Rp 1.500.000 | `201` Tercatat | `PASS` |
| A4 | `EVT-UJI-128D` v4 Rp 0 | `201` Tercatat, pesan "…dicatat sebagai saldo subledger periode 2026-09. Tidak ada jurnal yang dibuat." | `PASS` |
| A6 | `EVT-UJI-128E` tanpa rincian | `400` "Rincian saldo subledger hanya dan wajib ada pada pesan saldo subledger." | `PASS` |
| A7 | `EVT-UJI-128F` dengan `Components` | `400` "Pesan saldo subledger tidak boleh membawa rincian komponen." | `PASS` |
| A8 | `EVT-UJI-128G` versi `2a` | `400` "Versi pesan saldo harus bilangan bulat positif." | `PASS` |
| A9 | `EVT-UJI-128H` periode `2099-01` | `400` "Periode akuntansi 2099-01 tidak dikenal untuk badan hukum ini." | `PASS` |
| A10a | `EVT-UJI-128I` akun `9-9999` | `400` "Akun 9-9999 bukan akun kontrol pada badan hukum ini." | `PASS` |
| A10b | `EVT-UJI-128I2` | Yang terkirim teks placeholder, bukan kode akun biasa → `400` dengan pesan yang sama. Kasus "akun ada tetapi bukan control" **tidak** teruji runtime | `NOT RUN` — terbukti lewat source |
| A11 | `EVT-UJI-128L` jenis `UJI-TRANSAKSI-028` | `422` Tertahan `EVENT_TYPE_NOT_REGISTERED` — jenis Transaksi uji tidak dibuat. Regresi "jenis Transaksi membawa rincian saldo → `400`" **tidak** teruji | `NOT RUN` — terbukti lewat source |
| A12 → B6 | `EVT-UJI-128J` jenis `UJI-SALDO-028B` | `422` Tertahan; Coba Ulang sebelum jenis didaftarkan → masih Tertahan; sesudah didaftarkan → **Tercatat**, Riwayat Percobaan 3 baris (gagal, gagal, berhasil) | `PASS` |
| A13 → B7 | `EVT-UJI-128K` jenis `UJI-SALDO-028C`, tanpa rincian | `422` Tertahan; sesudah jenis didaftarkan, Coba Ulang → **Gagal** dengan pesan percobaan "Rincian saldo subledger hanya dan wajib ada pada pesan saldo subledger."; lalu Diabaikan beralasan "Membersihkan data uji 028" | `PASS` |

**Yang hanya terbukti lewat source.** (a) Penggantian baris `AccSubledgerBalance` menurut versi —
isi baris tidak diperiksa, dan urutan yang terjadi (v1 → v3 → v4) selalu naik, sehingga kasus
"versi lebih rendah datang terlambat" tidak terjadi. (b) Periode `Closed` — tidak ada periode
tertutup yang diuji. (c) Akun yang ada tetapi bukan akun kontrol (A10b). (d) Regresi jenis
Transaksi yang membawa rincian saldo (A11). Keempatnya dijaga kode yang sama dengan jalur yang
teruji: (a) `ExecuteUpdate … WHERE SourceVersionNumber < @versi`; (b) cabang
`StatusPeriode != Closed`; (c) kueri akun dengan syarat `IsControlAccount`; (d)
`membawaSaldo != jenisSaldo`.

**Cara menutup (a) di runtime, opsional:** aktifkan kembali `UJI-SALDO-028`, lalu Coba Ulang
`EVT-UJI-128A` (versi 2). Kejadian harus menjadi Tercatat, sedangkan baris saldo akun `1-1002` tetap
versi 4 bernilai Rp 0 — dilihat lewat kueri baca bagian 5.2. Langkah ini sekaligus membersihkan
`128A` dari status Tertahan.

**Data uji yang tersisa di `QuilvianNewDevRizki`.**

| Data | Keadaan | Akibat |
|---|---|---|
| `EVT-UJI-128A`, `EVT-UJI-128L` | Tertahan | Muncul sebagai **peringatan** `HELD_EVENTS` pada daftar periksa penutupan September 2026 — tidak menahan. Kejadian Tertahan tidak dapat diabaikan (`ACC-DEC-078`) |
| Baris `AccSubledgerBalance` periode `2026-09` | Akun `1-1002` (dari `128B`–`128D`) dan `1-1003` (dari `128J`) | Terbaca penghalang rekonsiliasi saat `BE-ACC-P2-014` berdiri |
| Jenis `UJI-SALDO-028`, `028B`, `028C` | Nonaktif | — |

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
|---|---|---|
| (1) Pesan saldo sah → `201`, status `Tercatat`, nol jurnal | Terpenuhi — source dan runtime (A2–A4, B3) | `ProsesKejadianAsync` mengalihkan jenis Saldo Subledger ke `CatatSaldoAsync` **sebelum** aturan posting dicari; `CatatSaldoAsync` mengubah status ke `Tercatat` tanpa memanggil `AccJournalService`; `BalasKejadianBaru` → `201` |
| (2) `Amount` nol dan negatif diterima | Terpenuhi — source dan runtime (A3 −1.500.000, A4 0) | `PeriksaIsianMenurutJenis` hanya menolak `Amount <= 0` bila pesan **tidak** membawa rincian saldo; `Balance` ditulis apa adanya |
| (3) Versi lebih tinggi mengganti baris; lebih rendah atau sama tidak | Terpenuhi di source; runtime membuktikan setiap versi `Tercatat`, tetapi isi baris tidak diperiksa (bagian 5.3 butir a) | `ExecuteUpdate` bersyarat `SourceVersionNumber < @versi` |
| (4) Periode `Closed` → `Tercatat` tanpa mengubah baris | Terpenuhi di source; tidak diuji runtime | `CatatSaldoAsync`: penulisan baris dilewati bila `StatusPeriode == AccountingPeriodStatus.Closed`; status tetap berpindah ke `Tercatat` |
| (5) Periode atau akun kontrol tidak dikenal, `SourceVersion` bukan bilangan bulat, atau membawa `Components` → `400` | Terpenuhi — source dan runtime (A7, A8, A9, A10a); akun ada tetapi bukan control hanya lewat source | `PeriksaRincianSaldoAsync` dari `TerimaAsync` sebelum kejadian disimpan; `PeriksaIsianMenurutJenis` untuk `Components` |
| (6) `SubledgerBalance` kosong pada jenis `SaldoSubledger` → `400` | Terpenuhi — source dan runtime (A6) | `PeriksaIsianMenurutJenis`: `membawaSaldo != jenisSaldo` |
| DoD: source berubah | Terpenuhi | Bagian 3.2 |
| DoD: build owner 0 error | Terpenuhi — tidak langsung | Backend yang diuji menjalankan perilaku `028`; jumlah warning tidak dilaporkan |
| DoD: laporan task tertulis | Terpenuhi | Berkas ini |
| Verifikasi kartu: uji panggil pesan tiruan | Terpenuhi | Bagian 5.3 |

## 7. Catatan penutup

| Hal | Isi |
|---|---|
| Peringatan | Tidak ada |
| Masalah yang diketahui | (a) Pesan berbentuk saldo yang tertahan karena kodenya belum terdaftar, lalu kodenya didaftarkan sebagai **Transaksi**, akan diproses sebagai transaksi dan rinciannya diabaikan — perilaku ini tidak diatur kontrak, dan tidak ditangani task ini. (b) Pada jalur penerimaan, status `Gagal` hanya mungkin bila data periode atau akun berubah di antara validasi dan pemrosesan; balasannya tetap `201` dengan pesan yang menyebut Gagal |
| Risiko tersisa | (a) Belum ada layar atau endpoint yang menampilkan isi `AccSubledgerBalance` — datang bersama `BE-ACC-P2-014`. (b) Data uji tersisa di dev — tabel bagian 5.3. (c) T6: begitu `014` aktif, akun kontrol tanpa baris saldo menahan penutupan. (d) Empat perilaku hanya terbukti lewat source — bagian 5.3 |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` tiga berkas source di `Areas/Corporate/AccountingManagement/AccountingEvent/`; ditambah laporan ini dan baris status roadmap serta traceability |
| Langkah berikutnya | `BE-ACC-P2-014` tetap ⛔ — kontrak endpoint sisi subledger dirancang lebih dulu (`design-business-module`) dan T6 diputuskan (`grill-me`). Opsional: Coba Ulang `EVT-UJI-128A` untuk menutup bagian 5.3 butir (a) di runtime |

**Perintah build untuk Rizki** (dari folder `NewQuilvianSystemBackend`):

```bash
dotnet build ./QuilvianSystemBackend.csproj -p:RunAnalyzers=false
```
