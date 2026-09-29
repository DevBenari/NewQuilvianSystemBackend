# Laporan Perubahan Backend — `BE-ACC-P2-026`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-026` |
| Judul | Penghalang tutup bulan dari kejadian, dan penolakan penonaktifan aturan yang ditunggu |
| Slice | `P2-2` — Wave B kotak masuk kejadian |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-026` (revisi 4, `APPROVED`) |
| Trace | `ACC-DEC-051`, `ACC-DEC-070` (`HELD_EVENTS` peringatan keenam); kekurangan terencana `BE-ACC-P2-018` |
| Contract version | `ACC-VALIDATION` Phase 2 bagian 2 baris "Aturan yang masih dipakai tidak boleh dihapus" dan bagian 4 baris "Tidak boleh ada kejadian gagal" (`ACC-VALIDATION-0.8`, approved 24 September 2026); `ACC-STATE-0.4` baris `Open` → `PendingClosingApproval` |
| Dependency | `BE-ACC-P2-021` ✅ |
| Klasifikasi | `MEDIUM` — skor 4: berkas diperiksa 9–20 (1), logika sedang (1), perilaku endpoint yang ada bertambah `409` (1), query baca baru (1); 2 berkas diubah, tanpa migration, keamanan, atau UI |
| Task mode | `BACKEND` |
| Target tulis | `AccountingPeriod/Services/AccPeriodClosingService.cs`, `MasterData/PostingRule/Services/AccPostingRuleService.cs`; laporan ini; baris status roadmap dan traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d62a084e` (branch `rizkiG`), belum di-commit — bertumpuk dengan `BE-ACC-P2-023` yang juga belum di-commit |
| Tanggal | 24 September 2026 |
| Status | **✅ SELESAI** — 3 dari 3 acceptance terpetakan ke source dan terbukti dari **response Swagger mentah Rizki, 25 September 2026** (bagian 8): Tertahan tampil sebagai peringatan di daftar periksa September 2026, penonaktifan aturan `UJI-026-A` yang ditunggu `409`, kejadian Gagal `EVT-UJI-112` menahan daftar periksa dan pengajuan Januari 2031 (`409`). Build Rizki **berhasil, 222 warning**. Bukti 24 September 2026 dari laporan agen AI dicabut (bagian 8.3). UAT belum dijalankan — diserahkan ke tim UAT |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `AccountingPeriod` dan `MasterData/PostingRule` |
| Registry | `Acc` — `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` atas kode `BE-ACC-P2-005`/`006`/`018`; bagian baru ditulis sebagai `NEW CODE` |
| QBE yang berlaku | `QBE-SVC-001` (aturan di service), `QBE-API-001` (bentuk respons daftar periksa tetap), `QBE-VAL-001` (penghalang dan penolakan `409`) |
| Standar endpoint | Nol endpoint baru. `GET /periods/{id}/closing-checklist`, `POST /periods/{id}/submit-closing`, dan `PATCH /posting-rules/{id}/deactivate` yang sudah ada berubah perilaku |
| Hak akses | Tidak berubah |

---

## 1. Masalah yang diperbaiki

| Sebelum | Akibat |
| --- | --- |
| Butir `FAILED_EVENTS` dan `HELD_EVENTS` pada daftar periksa penutupan selalu "Belum dapat diperiksa: kotak masuk kejadian keuangan belum berdiri" | Kotak masuk sudah berdiri sejak `021`, tetapi periode tetap dapat diajukan tutup walaupun ada kejadian Gagal — pendapatan atau piutang yang tidak pernah sampai ke buku besar ikut terkunci di bulan yang sudah ditutup |
| `PATCH /posting-rules/{id}/deactivate` tidak memeriksa kejadian Tertahan | Aturan dapat dimatikan padahal ada kejadian yang menunggu diproses olehnya; begitu dicoba ulang, kejadian itu tertahan lagi dengan alasan lain |

---

## 2. Proses bisnis

**Daftar periksa dan pengajuan penutupan.**

| Langkah | Isi |
| --- | --- |
| Pelaku | Accounting Manager (`AccountingPeriod : Close`) |
| 1 | Membuka daftar periksa satu periode. Kejadian dihitung **saat itu juga**, tidak disimpan |
| 2 | Kejadian "pada periode itu" = badan hukum sama dengan periode dan **tanggal akuntansi kejadian** jatuh di antara tanggal awal dan akhir periode |
| 3 | `FAILED_EVENTS` (penghalang): jumlah kejadian **Gagal**. Lebih dari nol → tombol Ajukan mati, pesan "Masih ada N kejadian keuangan yang gagal diproses. Coba ulang atau abaikan dengan alasan dari Kotak Masuk Kejadian." |
| 4 | `HELD_EVENTS` (peringatan): jumlah kejadian **Tertahan**. Tampil, tetapi **tidak** menahan (`ACC-DEC-070`) |
| 5 | Menekan Ajukan: penghalang dihitung **ulang** di server dengan fungsi yang sama. Ada kejadian Gagal → `409` dengan pesan yang sama |
| Jalan keluar | Coba Ulang kejadian Gagal sampai Terjurnal, atau Abaikan dengan alasan tertulis. Kejadian Diabaikan tidak dihitung |

**Penonaktifan aturan posting.**

| Langkah | Isi |
| --- | --- |
| 1 | Petugas menekan Nonaktifkan pada aturan posting |
| 2 | Dihitung kejadian **Tertahan** yang badan hukum dan jenis kejadiannya sama dengan aturan itu — yaitu kejadian yang akan diproses aturan ini begitu dicoba ulang |
| 3 | Lebih dari nol → `409` "Masih ada N kejadian yang menunggu aturan ini. Perbaiki aturannya lalu Coba Ulang kejadian itu dari Kotak Masuk Kejadian." |
| Catatan | Kejadian Tertahan karena jenisnya belum terdaftar tidak punya jenis, sehingga tidak menahan aturan mana pun |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Alasan |
| --- | --- |
| `AccountingPeriod/Services/AccPeriodClosingService.cs` | Daftar periksa, pengajuan, `HitungJurnalBelumDisahkanAsync` sebagai pola hitung bersama |
| `AccountingPeriod/DTOs/PeriodClosingDtos.cs` | `PeriodClosingBlockerResponse` — bentuk respons tidak berubah |
| `AccountingPeriod/Models/AccAccountingPeriod.cs` | `LegalEntityId`, `StartDate`, `EndDate` |
| `AccountingEvent/Models/AccAccountingEvent.cs`, `Enums/AccountingEventStatus.cs` | `LegalEntityId`, `AccountingDate`, `EventTypeId`, status |
| `MasterData/PostingRule/Services/AccPostingRuleService.cs`, `Models/AccPostingRule.cs` | `DeactivateAsync`, catatan "ditunda" |
| `contracts/validation-matrix.md` bagian 2 dan 4 | Teks aturan dan pesan |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/AccountingManagement/AccountingPeriod/Services/AccPeriodClosingService.cs` | + `HitungKejadianAsync` (`public static`, dipakai daftar periksa dan pengajuan — pola `HitungJurnalBelumDisahkanAsync`); `FAILED_EVENTS` dan `HELD_EVENTS` kini `Evaluated`; `SubmitClosingAsync` menolak `409` bila ada kejadian Gagal; alasan "belum tersedia" `OPEN_CASH_SHIFTS` dan `INTEGRATION_MISMATCH` ditulis ulang karena alasan lamanya ("kotak masuk belum berdiri") kini tidak benar |
| `Areas/Corporate/AccountingManagement/MasterData/PostingRule/Services/AccPostingRuleService.cs` | `DeactivateAsync` menolak `409` bila ada kejadian Tertahan yang menunggu |

**Komentar lama yang dihapus** karena kini keliru, tanpa menambah komentar baru: "Penghalang 1 — satu-satunya yang sudah dapat diperiksa sepenuhnya" (daftar periksa), paragraf catatan kelas "Yang sengaja belum ditegakkan" dan komentar `DITUNDA` di `DeactivateAsync` (aturan posting). Nol baris `//` ditambahkan.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Bentuk respons tidak berubah. Nilai berubah: `FAILED_EVENTS`/`HELD_EVENTS` ber-`State = Evaluated` dengan `Count` sungguhan; `NotYetAvailableCount` turun 7 → 5; `CanSubmitClosing` kini ikut `false` bila ada kejadian Gagal. `409` baru pada pengajuan dan penonaktifan aturan, sesuai `ACC-VALIDATION`. Layar Daftar Periksa (`FE-ACC-P2-001`) sudah menangani `Evaluated` — tanpa perubahan frontend |
| Database | Nol migration. Dua query hitung baru atas `AccAccountingEvent` |
| Keamanan/Auth | `NOT APPLICABLE` |

---

## 4. Dokumentasi endpoint

Nol endpoint baru. Perilaku yang berubah:

| Method | Path | Hak akses | Yang berubah |
| --- | --- | --- | --- |
| `GET` | `api/v1/corporate/accounting/periods/{id}/closing-checklist` | `AccountingPeriod : Read` | `FAILED_EVENTS` (penghalang) dan `HELD_EVENTS` (peringatan) dihitung sungguhan |
| `POST` | `api/v1/corporate/accounting/periods/{id}/submit-closing` | `AccountingPeriod : Close` | `409` bila ada kejadian Gagal pada periode itu |
| `PATCH` | `api/v1/corporate/accounting/posting-rules/{id}/deactivate` | `PostingRule : Update` | `409` bila ada kejadian Tertahan yang menunggu aturan itu |

Rute persis mengikuti controller `BE-ACC-P2-005`/`006` dan `BE-ACC-P2-018`; tidak diubah.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Pemeriksaan source 3 acceptance | Terpetakan semua | `PASS` | Bagian 6 |
| Nol `//` baru | Baris tambahan berisi `//`: 0 | `PASS` | `git diff -U0 \| grep "^+" \| grep -c "//"` |
| `dotnet build` | Berhasil — Rizki, 24 September 2026: `Build succeeded with 222 warning(s) in 448,7s`. Sama dengan baseline 222 build `024`/`025`; nol warning baru | `PASS` | Tangkapan layar keluaran build |
| Uji | Lulus — skenario A dan B lewat Swagger, response mentah Rizki 25 September 2026 | `PASS` | Bagian 8 |

Uji manual: `PASS` — Rizki, 25 September 2026. Skenario A dijalankan ulang dengan data baru (`UJI-026-A`, `EVT-UJI-113`) dan skenario B dengan tahun 2031 (`EVT-UJI-112`) — lihat bagian 8.

```powershell
cd C:\Users\BenariDev03\QuilvianV2\NewQuilvianSystemBackend
dotnet build .\QuilvianSystemBackend.csproj -p:RunAnalyzers=false
```

**Skenario A — Tertahan: peringatan dan penolakan penonaktifan (acceptance 2 dan 3).** Memakai
jenis uji tersendiri supaya tidak mengunci aturan `PATIENT_PAYMENT` yang menurut `ACC-DEC-083`
harus dinonaktifkan kelak. Diakhiri dengan membereskan data ujinya.

| # | Langkah | Diharapkan |
| ---: | --- | --- |
| A1 | Layar Jenis Kejadian: tambah `UJI-026` "Uji Penghalang 026", modul `Finance`, Transaksi. **Jangan** buat aturan posting dulu | Tersimpan |
| A2 | Swagger `POST /accounting-events`: salin Isi Pesan Asli `EVT-UJI-002`, ganti `EventNumber` = `EVT-UJI-026A`, `EventTypeCode` = `UJI-026`, `SourceTransactionId` = nilai baru, `AccountingDate` = `2026-09-20` | `422`, Tertahan `POSTING_RULE_MISSING` |
| A3 | Layar Periode Akuntansi → September 2026 → Daftar Periksa | Butir **Kejadian keuangan tertahan** berangka ≥ 1 di bagian peringatan; tombol Ajukan **tidak** dimatikan olehnya (acceptance 2) |
| A4 | Layar Aturan Posting: buat aturan untuk `UJI-026` — salin baris aturan `PATIENT_PAYMENT`, perlakuan **Langsung Disahkan** | Tersimpan, aktif |
| A5 | Nonaktifkan aturan `UJI-026` | `409` "Masih ada 1 kejadian yang menunggu aturan ini…" (acceptance 3) |
| A6 | Rincian `EVT-UJI-026A` → **Coba Ulang** | Terjurnal (jurnal Posted di September 2026) |
| A7 | Nonaktifkan aturan `UJI-026`, lalu jenis `UJI-026` | Keduanya berhasil — tidak ada lagi yang menunggu. Data uji bersih |

**Skenario B — Gagal menahan pengajuan (acceptance 1).** Kejadian Gagal hanya lahir dari penjadwal
`BE-ACC-P2-023`, jadi skenario ini menyambung uji `023`: kejadian `EVT-UJI-023B` (Gagal,
tanggal 2030-01-15). Ia baru dapat dilihat pada daftar periksa bila periode 2030 **ada**.

| # | Langkah | Diharapkan |
| ---: | --- | --- |
| B1 | *(hanya bila Rizki rela periode 2030 ada di database dev)* Layar Periode Akuntansi: bangkitkan tahun buku 2030 | Periode 2030-01 … 2030-12 Open |
| B2 | Daftar Periksa Januari 2030 | **Kejadian keuangan gagal** = 1, bertanda menahan; tombol Ajukan mati |
| B3 | Swagger `POST /periods/{id Januari 2030}/submit-closing` | `409` "Masih ada 1 kejadian keuangan yang gagal diproses…" (acceptance 1) |
| B4 | Rincian `EVT-UJI-023B` → **Coba Ulang** | Terjurnal di Januari 2030 (sekaligus `BE-ACC-P2-023` skenario 9) |
| B5 | Daftar Periksa Januari 2030 lagi | Kejadian keuangan gagal = 0 |

Bila Rizki tidak ingin periode 2030 di database dev, acceptance (1) dibuktikan lewat source seperti
jalur Gagal pada `BE-ACC-P2-025` — keputusan Rizki.

**Tidak dijalankan:** automated test — dilarang `ACC-DEC-081`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Satu kejadian Gagal pada periode itu menahan pengajuan penutupan | Terpenuhi | Daftar periksa: `Butir(FAILED_EVENTS, …, kejadianGagal, menahan: true)` → `IsBlocking`, `CanSubmitClosing = false`. Pengajuan: `SubmitClosingAsync` → `HitungKejadianAsync(Gagal) > 0` → `409`. Uji: Januari 2031 `FAILED_EVENTS` `count: 1`, `isBlocking: true`, `canSubmitClosing: false`; `submit-closing` → `409` (bagian 8.2 B3–B4) |
| (2) Kejadian Tertahan hanya peringatan | Terpenuhi | `Butir(HELD_EVENTS, …, menahan: false)` di daftar `peringatan`; `SubmitClosingAsync` tidak menghitung Tertahan. Uji: September 2026 `HELD_EVENTS` `count: 3`, `isBlocking: false`, sedangkan `blockingCount: 1` hanya dari `UNPOSTED_JOURNALS` (bagian 8.1 A3) |
| (3) Penonaktifan aturan yang ditunggu → `409` | Terpenuhi | `DeactivateAsync`: hitung Tertahan dengan `LegalEntityId` dan `EventTypeId` sama → `409`. Uji: `PATCH /posting-rules/1d03f254-…/deactivate` → `409` "Masih ada 1 kejadian yang menunggu aturan ini…"; sesudah kejadian terjurnal → `200` (bagian 8.1 A5, A7) |
| DoD: source berubah | Terpenuhi | Bagian 3.2 |
| DoD: build owner 0 error | Terpenuhi | Build Rizki berhasil, 222 warning |
| DoD: laporan task tertulis | Terpenuhi | Berkas ini |
| Verifikasi kartu: uji panggil daftar periksa | Terpenuhi | September 2026 (A3) dan Januari 2031 (B3, B5) |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Keputusan terbuka — **pemilik Rizki** | Kejadian Gagal bertanggal di periode yang **sudah ditutup** tidak menahan periode mana pun: periodenya sendiri sudah tertutup, dan periode sesudahnya tidak menghitungnya karena tanggalnya di luar rentang. Padahal begitu dicoba ulang, jurnalnya jatuh ke periode terbuka berikutnya (`ACC-DEC-047`). Apakah kejadian seperti itu harus menahan periode terbuka paling awal? Kode mengikuti bunyi kontrak "pada periode itu" apa adanya |
| Temuan di luar cakupan | Alasan "belum tersedia" butir `DEPRECIATION_NOT_RUN` masih menyebut "Penjadwal jurnal berulang belum berdiri (`BE-ACC-P2-008`)", padahal `008` sudah ✅. Tidak diubah |
| Risiko tersisa | Penutupan periode yang sudah **diajukan** sebelum kejadian Gagal muncul tidak diperiksa ulang saat disetujui — kontrak hanya menaruh penghalang pada Ajukan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Source di-commit Rizki `1ea09d66`; `rizkiG` sudah di-merge dengan integration (`68ebc667`). Pembaruan laporan ini belum di-commit |
| Langkah berikutnya | Keputusan terbuka di atas tetap menunggu Rizki; `027`/`028`/`014` tetap ⛔ `GATE-FIN-087` |

---

## 8. Bukti uji — response Swagger mentah, Rizki, 25 September 2026

Seluruh langkah lewat Swagger dengan akun SuperAdmin; nol SQL langsung. Badan hukum
`3bf63974-a754-4b20-81ee-70894f6fb058` (PT Metropolitan Medical Centre).

### 8.1 Skenario A — Tertahan hanya peringatan, penonaktifan aturan ditolak

| Langkah | Panggilan | Hasil sebenarnya | Hasil |
| --- | --- | --- | :---: |
| A0 | `GET /event-types?Search=PATIENT_PAYMENT`, `GET /posting-rules?EventTypeId=…`, `GET /posting-rules/ca655eaa-…` | Bahan aturan: jenis jurnal `JU`, dua baris komponen `TOTAL` | ✅ |
| A1 | `POST /event-types` `UJI-026-A`, `eventKind: 1` | `201`; `id c5d7a5a3-…`, `accountingEventCount: 0` | ✅ |
| A2 | `POST /accounting-events` `EVT-UJI-113` (`UJI-026-A`, 2026-09-25, Rp 400.000) | `422` Tertahan `POSTING_RULE_MISSING`; `accountingEventId 9754dbc5-…` | ✅ |
| A3 | `GET /periods/94b6592f-…/closing-checklist` (September 2026) | `HELD_EVENTS` `count: 3`, `isBlocking: false`, `state: 1`. `FAILED_EVENTS` `count: 0`, `state: 1`. `blockingCount: 1` hanya dari `UNPOSTED_JOURNALS` (3 jurnal draft) — acceptance (2) | ✅ |
| A4 | `POST /posting-rules` untuk `UJI-026-A`, `treatment: 1` | `201`; aturan `1d03f254-…` aktif | ✅ |
| A5 | `PATCH /posting-rules/1d03f254-…/deactivate` | `409` "Masih ada 1 kejadian yang menunggu aturan ini. Perbaiki aturannya lalu Coba Ulang kejadian itu dari Kotak Masuk Kejadian." — acceptance (3) | ✅ |
| A6 | `POST /accounting-events/9754dbc5-…/retry` | `200` "Kejadian EVT-UJI-113 berhasil dijurnal sebagai JU/2026/09/00007", `journalStatus: Posted` | ✅ |
| A7 | `PATCH /posting-rules/1d03f254-…/deactivate`, lalu `PATCH /event-types/c5d7a5a3-…/deactivate` | Keduanya `200`; jenis `isActive: false`, `accountingEventCount: 1` | ✅ |

### 8.2 Skenario B — kejadian Gagal menahan tutup bulan

| Langkah | Panggilan | Hasil sebenarnya | Hasil |
| --- | --- | --- | :---: |
| B0 | `GET /periods?…&FiscalYear=2031` | `totalData: 0` — tahun 2031 dipakai | ✅ |
| B1 | `POST /accounting-events` `EVT-UJI-112` (PATIENT_PAYMENT, 2031-01-15) | `201` "…Penjurnalan tertunda dan akan dicoba ulang otomatis." Menjadi Gagal lewat penjadwal (percobaan #1–#4) | ✅ |
| B2 | `POST /periods/generate` `fiscalYear: 2031` pukul 13.36 | `201` "Dua belas periode tahun buku 2031 berhasil dibangkitkan"; Januari 2031 `e300b651-…` | ✅ |
| B3 | `GET /periods/e300b651-…/closing-checklist` pukul 13.43 | `FAILED_EVENTS` `count: 1`, `isBlocking: true`, `state: 1`; `canSubmitClosing: false`; `blockingCount: 1` | ✅ |
| B4 | `POST /periods/e300b651-…/submit-closing` pukul 13.48 | `409` "Masih ada 1 kejadian keuangan yang gagal diproses. Coba ulang atau abaikan dengan alasan dari Kotak Masuk Kejadian." — acceptance (1) | ✅ |
| B5 | `POST /accounting-events/cda4d52c-…/retry` pukul 13.50, lalu daftar periksa lagi | `200` "…berhasil dijurnal sebagai JU/2031/01/00001" (percobaan #5), `journalStatus: Draft`; `FAILED_EVENTS` `count: 0` | ✅ |

Pada B5, butir `UNPOSTED_JOURNALS` Januari 2031 menjadi `1`: aturan `PATIENT_PAYMENT` berperlakuan
Buat Draft (`treatment: 2`), sehingga jurnal hasil Coba Ulang masih draft. Itu perilaku aturan posting,
bukan cacat task ini.

UAT belum dijalankan — diserahkan ke tim UAT.

### 8.3 Bukti yang dicabut

Bukti versi 24 September 2026 berasal dari laporan agen AI penguji dan menyebut kejadian `EVT-UJI-026A`,
`EVT-UJI-023B`, serta periode 2030. Kotak Masuk pada 25 September 2026 tidak memuat kedua kejadian itu,
sehingga bukti tersebut **tidak dipakai**. Status ✅ kini bersandar hanya pada bagian 8.1 dan 8.2.

### 8.4 Data uji yang tertinggal di database dev

| Data | Keadaan |
| --- | --- |
| Periode tahun buku 2031 | Dua belas periode Open |
| Jurnal `JU/2026/09/00007` (`EVT-UJI-113`) | Posted, September 2026, akun uji aturan `UJI-026-A` |
| Jurnal `JU/2031/01/00001` (`EVT-UJI-112`) | Draft, Januari 2031 |
| Jenis dan aturan `UJI-026-A` | Nonaktif |
| `EVT-UJI-001` (`PENGAKUAN-PIUTANG`) dan `EVT-UJI-104` (`UJI-BELUM-TERDAFTAR`) | Masih Tertahan — ikut dihitung peringatan `HELD_EVENTS` September 2026 |
