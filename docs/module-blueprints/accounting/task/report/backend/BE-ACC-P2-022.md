# Laporan Perubahan Backend — `BE-ACC-P2-022`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-022` |
| Judul | `EventKind` pada API jenis kejadian |
| Slice | `P2-1` — Wave B kotak masuk kejadian |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-022` (revisi 4, `APPROVED`) |
| Trace | `ACC-DEC-087`; `02-backend-architecture.md` bagian 22.6 dan tabel "Diperbarui" bagian 22; kamus data bagian 11 baris `EventKind`. **Belum ada FR** — coverage gap yang sudah tercatat |
| Contract version | `ACC-API-0.12` grup Event Type — approved Rizki 24 September 2026 (`GATE-DESAIN-0924`) |
| Dependency | `BE-ACC-P2-019` ✅ (kolom `EventKind` di entity), `BE-ACC-P2-020` ✅ (kolom ada di database), `BE-ACC-P2-017` ✅ (grup endpoint sudah berdiri) |
| Klasifikasi | `MEDIUM` — skor 5: berkas diperiksa 9–20 (1), logika sedang (1), kontrak API berubah secara kompatibel (2), perilaku query yang ada (1); satu repository, 2 berkas diubah, tanpa dampak keamanan atau UI |
| Task mode | `BACKEND` (disusul `FE-ACC-P2-013` pada `FRONTEND`, berurutan) |
| Target tulis | `Areas/Corporate/AccountingManagement/MasterData/EventType/**`; laporan ini; baris status roadmap dan traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `8535dd56` (branch `rizkiG`), belum di-commit — bertumpuk dengan `021`, `024`, `025` yang juga belum di-commit |
| Tanggal | 24 September 2026 |
| Status | **✅ SELESAI** — 24 September 2026. 4 dari 4 acceptance terpetakan ke source; uji Rizki lulus 10 dari 10 (7 skenario layar `FE-ACC-P2-013` + 3 Swagger, bagian 5). Build terbukti **tidak langsung**: backend yang diuji sudah menjawab `eventKind` dan `accountingEventCount`, bidang yang hanya ada di kode task ini — jumlah warning build tidak dilaporkan. UAT belum dijalankan — diserahkan ke tim UAT. Riwayat: 🟡 pada hari yang sama |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `MasterData/EventType` |
| Registry | `Acc` — `ACTIVE` (`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 38) |
| Keberlakuan | `TOUCHED LEGACY` atas kode `BE-ACC-P2-017` yang masih muda; perubahan baru ditulis sebagai `NEW CODE` |
| QBE yang berlaku | `QBE-SVC-001` (aturan di service, controller tidak berubah), `QBE-API-001`, `QBE-VAL-001` (nilai enum dan larangan ubah), `QBE-DTO-001`, `QBE-LOG-001` (pencatatan lewat controller yang ada), `QBE-ENUM-001` (`EventTypeKind` milik modul) |
| Standar endpoint | **Master data.** Nol endpoint baru; enam endpoint `017` ditambah `activate` tetap. Kekurangan baseline sembilan endpoint (`GET /filters/metadata`, `GET /summary`, `DELETE`) sudah tercatat pada laporan `BE-ACC-P2-017` dan tidak diubah di sini |
| Hak akses | Tidak berubah. `PUT` tetap `[AccessAction("Update", …)]` + `[AccessPermission("EventType", "Update")]` |

---

## 1. Masalah yang diperbaiki

Sejak `BE-ACC-P2-019`, setiap jenis kejadian punya kolom `EventKind` di database: **Transaksi**
(kejadian yang dijurnal) atau **Saldo Subledger** (pernyataan saldo akun kontrol yang disimpan tanpa
jurnal, `ACC-DEC-087`). Kotak masuk sudah membaca kolom itu saat menerima pesan (`BE-ACC-P2-021`).
Tetapi API jenis kejadian belum mengenalnya. Akibatnya:

- Administrator akuntansi tidak dapat menandai jenis mana pun sebagai Saldo Subledger. Seluruh
  jenis selamanya Transaksi, termasuk kode saldo yang kelak disepakati Finance.
- Daftar dan rincian jenis kejadian tidak menunjukkan perlakuannya, sehingga petugas tidak tahu
  kenapa sebuah pesan diperlakukan sebagai saldo.

---

## 2. Proses bisnis

| Langkah | Isi |
| --- | --- |
| Pelaku | Administrator akuntansi yang punya hak `EventType : Create` / `Update` |
| Pemicu | Menambah jenis kejadian baru, atau memperbaiki jenis yang sudah ada |
| 1 | Saat menambah, petugas memilih Jenis Perlakuan. Bila tidak memilih — termasuk pemanggil lama yang belum mengenal bidang ini — jenis tersimpan sebagai **Transaksi** |
| 2 | Saat mengubah, perlakuan boleh diganti **selama belum ada satu kejadian pun yang tercatat atas jenis itu** |
| 3 | Bila sudah ada kejadian, penggantian ditolak `409`: "Jenis perlakuan tidak dapat diubah karena jenis ini sudah dipakai kejadian." Nama dan modul asal tetap boleh diubah asal perlakuannya tidak diganti |
| 4 | Penggantian yang berhasil disebut dalam pesan balasan — "Jenis perlakuan berubah dari Transaksi menjadi Saldo Subledger" — dan pesan itu ikut tercatat di log aplikasi |
| Jalur tidak normal | Angka perlakuan selain `1`/`2` → `400`. Teks selain angka → `400` dari pengikatan model ASP.NET |
| Hasil akhir | Daftar dan rincian memuat `eventKind`; rincian juga memuat `accountingEventCount` sehingga layar dapat mengunci isian sebelum petugas mencoba |

**Kenapa larangan ubah dihitung dari kejadian yang terhubung, bukan dari kodenya.** Kejadian yang
tertahan karena jenisnya belum terdaftar (`EVENT_TYPE_NOT_REGISTERED`) menyimpan kode tetapi
`EventTypeId`-nya kosong — ia belum pernah diproses dengan perlakuan apa pun. Bila kejadian seperti itu
ikut dihitung, petugas yang salah memilih perlakuan saat mendaftarkan jenisnya tidak dapat lagi
membetulkannya, padahal jenis tidak dapat dihapus. Begitu kejadian itu diproses ulang dan
dipasangkan ke jenisnya (`ACC-DEC-075`), ia terhitung dan perlakuan terkunci.

**Kenapa `PUT` tanpa `EventKind` berarti "tidak berubah", bukan "Transaksi".** Kontrak menjamin
permintaan tanpa `EventKind` tetap diterima. Pada `POST` artinya Transaksi. Pada `PUT`, membacanya
sebagai Transaksi akan diam-diam mengembalikan jenis Saldo Subledger menjadi Transaksi setiap kali
pemanggil lama menyimpan nama — atau menolaknya `409` bila jenis itu sudah punya kejadian. Membiarkan
nilai tersimpan adalah satu-satunya bacaan yang tidak merusak pemanggil lama.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Alasan |
| --- | --- |
| `MasterData/EventType/Controllers/EventTypeController.cs` | Hak akses dan pencatat log `PUT`/`POST` |
| `MasterData/EventType/Services/AccEventTypeService.cs` | Seluruh aturan jenis kejadian |
| `MasterData/EventType/DTOs/EventTypeDtos.cs` | Bentuk request dan respons |
| `MasterData/EventType/Enums/EventTypeKind.cs`, `Models/AccEventType.cs` | Nilai enum `1`/`2`, bawaan `Transaksi` |
| `AccountingEvent/Models/AccAccountingEvent.cs` | `EventTypeId` nullable — dasar penghitungan "sudah dipakai kejadian" |
| `AccountingEvent/Services/AccAccountingEventService.cs` | Cara kotak masuk mencocokkan kode dan membaca `EventKind` |
| `MasterData/PostingRule/Services/AccPostingRuleService.cs` | Pola `Enum.IsDefined` → `400` yang ditiru |
| `contracts/api-contract.md` grup Event Type, `02-backend-architecture.md` bagian 22.6, `erd/data-dictionary.md` baris `EventKind` | Kontrak target |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/AccountingManagement/MasterData/EventType/DTOs/EventTypeDtos.cs` | + `EventKind` pada `EventTypeListResponse` (ikut ke rincian lewat pewarisan); + `AccountingEventCount` pada `EventTypeDetailResponse`; + `EventTypeKind? EventKind` pada `CreateEventTypeRequest` dan `UpdateEventTypeRequest` |
| `Areas/Corporate/AccountingManagement/MasterData/EventType/Services/AccEventTypeService.cs` | Daftar dan rincian memetakan `EventKind`; rincian menghitung kejadian terhubung; `CreateAsync` memakai bawaan Transaksi dan menolak nilai di luar enum; `UpdateAsync` menolak nilai di luar enum dan menolak penggantian perlakuan `409` bila sudah ada kejadian, lalu menyebut penggantian di pesan balasan; dua pembantu `PerlakuanTidakSah` dan `NamaPerlakuan` |

Controller **tidak** berubah: payload log `POST` (`request`) dan `PUT` (`EntityId` + `request`) sudah
membawa `EventKind`, dan pesan balasan yang dicatat kini menyebut perlakuan lama dan baru.

Nol baris komentar `//` ditambahkan.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Bertambah, kompatibel ke belakang. Bidang baru pada respons; bidang opsional pada request. `EventKind` dikirim sebagai **angka** (`1` Transaksi, `2` Saldo Subledger), seperti seluruh enum Accounting lain. **Delta kontrak:** `AccountingEventCount` pada rincian tidak disebut `ACC-API-0.12` — ditambahkan supaya layar dapat mengunci isian sebelum petugas mencoba (`FE-ACC-P2-013` acceptance 2), meniru `ActivePostingRuleCount` yang sudah ada |
| Database | **Nol migration.** Kolom `AccEventType.EventKind` sudah dibuat `20260924042630_AddAccountingEventInbox` (`BE-ACC-P2-020`, diterapkan). Hanya query baca baru: hitung `AccAccountingEvent` per `EventTypeId` |
| Keamanan/Auth | `NOT APPLICABLE` — hak akses tidak berubah; `accountingEventCount` hanya angka, tanpa isi kejadian |

---

## 4. Dokumentasi endpoint

#### Corporate / Accounting / Master Data / Event Type

Base URL: `api/v1/corporate/accounting/event-types`

| Method | Path | Kegunaan | Hak akses | Yang berubah |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar jenis kejadian | `EventType : Read` | + `eventKind` per baris |
| `GET` | `/{id}` | Rincian jenis kejadian | `EventType : Read` | + `eventKind`, `accountingEventCount` |
| `POST` | `/` | Menambah jenis kejadian | `EventType : Create` | + `eventKind` opsional, bawaan `1`; `400` bila bukan `1`/`2` |
| `PUT` | `/{id}` | Mengubah nama, modul asal, dan perlakuan | `EventType : Update` | + `eventKind` opsional, kosong = tidak berubah; `400` bila bukan `1`/`2`; `409` bila berubah dan sudah ada kejadian |
| `GET` | `/options` | Pilihan untuk form aturan posting | `EventType : Read` | Tidak berubah |
| `PATCH` | `/{id}/deactivate`, `/{id}/activate` | Keaktifan | `EventType : Update` | Tidak berubah |

Contoh `PUT` yang ditolak:

```json
{ "eventTypeName": "Pembayaran Pasien", "sourceModule": "CASHIER", "eventKind": 2 }
```

→ `409` `"Jenis perlakuan tidak dapat diubah karena jenis ini sudah dipakai kejadian."`

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Pemeriksaan source 4 acceptance | Terpetakan semua | `PASS` | Bagian 6 |
| Nol `//` baru | `git diff` baris tambahan berisi `//`: 0 | `PASS` | Perintah `git diff … \| grep "^+" \| grep -c "//"` |
| Konsumen DTO lain | Hanya `EventTypeController` dan `AccEventTypeService` memakai keempat DTO | `PASS` | Pencarian nama DTO di seluruh `*.cs` |
| `dotnet build` | Berhasil — dibuktikan tidak langsung: backend yang dijalankan Rizki 24 September 2026 melayani `eventKind` dan `accountingEventCount`. Jumlah warning tidak dilaporkan | `PASS` | Hasil uji skenario 1–2 |
| Uji panggil | Lulus 7 dari 7 skenario di bawah (1–4 lewat layar, 5–7 lewat Swagger) | `PASS` | Laporan uji Rizki, 24 September 2026 |

Uji manual: `PASS` — Rizki, 24 September 2026.

| # | Hasil uji Rizki |
| ---: | --- |
| 1 | Kolom Jenis Perlakuan tampil; "Pembayaran Pasien" = Transaksi ✅ |
| 2 | Form "Pembayaran Pasien": isian terkunci, keterangan "sudah dipakai N kejadian" ✅ — `accountingEventCount` terisi |
| 3 | `UJI-SALDO-022` Saldo Subledger dibuat, tampil di daftar ✅ |
| 4 | `UJI-SALDO-022` diganti ke Transaksi, berhasil ✅ |
| 5 | Swagger `PUT` "Pembayaran Pasien" `eventKind = 2` → `409` ✅ |
| 6 | Swagger `POST` `eventKind = 3` → `400` ✅ |
| 7 | Swagger `POST` tanpa `eventKind` → `201`, `eventKind = 1` ✅ |

Jenis uji `UJI-SALDO-022` sudah dinonaktifkan Rizki lewat layar.

**Perintah build untuk Rizki** (hentikan backend yang sedang berjalan lebih dahulu):

```powershell
cd C:\Users\BenariDev03\QuilvianV2\NewQuilvianSystemBackend
dotnet build .\QuilvianSystemBackend.csproj -p:RunAnalyzers=false
```

**Skenario uji.** Data yang dipakai benar-benar ada: jenis `PATIENT_PAYMENT` "Pembayaran Pasien"
(dipakai `EVT-UJI-002` yang Terjurnal `JU/2026/09/00005`). Skenario 1–5 dapat dijalankan lewat layar
Jenis Kejadian sesudah `FE-ACC-P2-013`; skenario 6–7 lewat Swagger karena layar tidak menawarkan
nilai itu. Jenis uji yang dibuat di skenario 3 dan 7 tidak dapat dihapus — nonaktifkan sesudah uji.

| # | Langkah | Hasil yang diharapkan |
| ---: | --- | --- |
| 1 | `GET /event-types` | Setiap baris membawa `eventKind: 1` — baris lama bernilai bawaan migration `020` |
| 2 | `GET /event-types/{id}` untuk `PATIENT_PAYMENT` | `eventKind: 1`, `accountingEventCount` ≥ 1 |
| 3 | `POST` `{ "eventTypeCode": "UJI-SALDO-022", "eventTypeName": "Uji Saldo 022", "sourceModule": "Finance", "eventKind": 2 }` | `201`, `eventKind: 2`, `accountingEventCount: 0` |
| 4 | `PUT` jenis `UJI-SALDO-022` dengan `eventKind: 1` | `200`, pesan "… Jenis perlakuan berubah dari Saldo Subledger menjadi Transaksi." |
| 5 | `PUT` `PATIENT_PAYMENT` dengan `eventKind: 2`, nama dan modul tetap | `409` "Jenis perlakuan tidak dapat diubah karena jenis ini sudah dipakai kejadian."; `eventKind` tetap `1` |
| 6 | `POST` dengan `"eventKind": 3` (kode lain) | `400` "Jenis perlakuan harus Transaksi (1) atau Saldo Subledger (2)."; tidak tersimpan |
| 7 | `POST` `{ "eventTypeCode": "UJI-TANPA-PERLAKUAN-022", "eventTypeName": "Uji tanpa perlakuan", "sourceModule": "Finance" }` tanpa `eventKind` | `201`, `eventKind: 1` |

**Tidak dijalankan:** automated test — dilarang `ACC-DEC-081`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Permintaan tanpa `EventKind` tersimpan sebagai `Transaksi` — pemanggil lama tidak rusak | Terpenuhi | `CreateAsync`: `request.EventKind ?? EventTypeKind.Transaksi`. `UpdateAsync`: kosong = nilai tersimpan (bagian 2). Uji: skenario 7 ✅ |
| (2) Nilai di luar enum → `400` | Terpenuhi | `Enum.IsDefined(perlakuan)` → `PerlakuanTidakSah` pada `CreateAsync` dan `UpdateAsync`. Uji: skenario 6 ✅ |
| (3) Mengubah `EventKind` pada jenis yang sudah punya kejadian → `409` | Terpenuhi | `UpdateAsync`: `perlakuanBerubah && AnyAsync(EventTypeId == jenis.Id)` → `409` dengan teks kontrak. Uji: skenario 5 ✅ |
| (4) Perubahan tercatat `LoggerService` | Terpenuhi di source | `EventTypeController.Update` → `CatatAsync("EventType.Update", hasil, new { EntityId = id, request })`; `request` memuat `EventKind`, pesan memuat perlakuan lama dan baru; penolakan `409` tercatat sebagai `Warning`. Isi tabel log tidak diperiksa langsung — agent tidak mengeksekusi database |
| DoD: source berubah | Terpenuhi | Bagian 3.2 |
| DoD: build owner 0 error | Terpenuhi — bukti tidak langsung | Kode task ini berjalan di backend yang diuji Rizki; jumlah warning tidak dilaporkan |
| DoD: laporan task tertulis | Terpenuhi | Berkas ini |
| Verifikasi kartu: `GET /event-types` menampilkan `EventKind` | Terpenuhi | Skenario 1 ✅ |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Menandai sebuah jenis sebagai Saldo Subledger berakibat nyata: `POST /accounting-events` dengan kode itu kini ditolak `409` "Pesan saldo subledger belum dapat diterima" sampai `BE-ACC-P2-028` dibangun (perilaku `021`). Jangan menandai jenis produksi sebagai saldo sebelum `028` |
| Masalah yang diketahui | Pemeriksaan "sudah ada kejadian" dan penyimpanan tidak dalam satu transaksi. Bila kejadian pertama tiba tepat di antara keduanya, perlakuan dapat berubah sesudah satu kejadian terhubung. Peluangnya sangat kecil (penggantian perlakuan adalah tindakan administrasi yang jarang), dan dibiarkan agar tidak menambah penguncian pada tabel kejadian |
| Risiko tersisa | `GET /options` tidak membedakan perlakuan, sehingga form aturan posting tetap menawarkan jenis Saldo Subledger walaupun jenis itu tidak pernah memakai aturan posting (`02-backend-architecture.md` bagian 22.6). Menyaringnya adalah keputusan tampilan yang belum diambil — **terbuka, pemilik Rizki** |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat laporan `FE-ACC-P2-013` bagian penutup untuk keadaan akhir kedua repository; di backend task ini hanya menambah dua berkas `M` di `MasterData/EventType/` dan berkas laporan ini |
| Langkah berikutnya | `BE-ACC-P2-023` menunggu perintah Rizki. Commit `021`–`025` + dokumen masih milik Rizki |
