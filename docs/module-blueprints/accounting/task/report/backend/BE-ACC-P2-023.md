# Laporan Perubahan Backend — `BE-ACC-P2-023`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-023` |
| Judul | Penjadwal coba ulang |
| Slice | `P2-2` — Wave B kotak masuk kejadian |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-023` (revisi 4, `APPROVED`) |
| Trace | `FR-P2-013`, `FR-P2-014`; `ACC-DEC-049`, `074`, `084` |
| Contract version | `02-backend-architecture.md` bagian 22.5 (approved Rizki 24 September 2026, `GATE-DESAIN-0924`); `ACC-STATE-0.4` |
| Dependency | `BE-ACC-P2-021` ✅ |
| Klasifikasi | `MEDIUM` — skor 5: berkas diperiksa 9–20 (1), berkas diubah 4 (1), logika penjadwalan dan persaingan (2), perilaku persistence yang ada (1); tanpa perubahan API, keamanan, atau UI |
| Task mode | `BACKEND` |
| Target tulis | `Areas/Corporate/AccountingManagement/AccountingEvent/**`; satu baris di blok Accounting `Program.cs` (**diizinkan Rizki 24 September 2026**); laporan ini; baris status roadmap dan traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d62a084e` (branch `rizkiG`), belum di-commit |
| Tanggal | 24 September 2026 |
| Status | **🟡 SEBAGIAN** — 6 dari 6 acceptance terpetakan ke source; build Rizki **berhasil, 222 warning** (nol warning baru); uji Rizki 24 September 2026 membuktikan acceptance (3), (4), (5) serta jalur Gagal di layar (bagian 8). **Belum:** bukti waktu percobaan untuk acceptance (1) tenggang dan (2) jeda 1/5/15 menit — laporan uji menyebut 3 percobaan dan jeda 120 detik, sedangkan kode menghasilkan 4 percobaan berjarak ±2, 5, 15 menit |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `AccountingEvent` |
| Registry | `Acc` — `ACTIVE` |
| Keberlakuan | `NEW CODE` (hosted service, metode siklus) di atas kode `021` yang masih muda |
| QBE yang berlaku | `QBE-SVC-001` (seluruh aturan di `AccAccountingEventService`; hosted service hanya memanggil), `QBE-TXN-001` (perpindahan status bersyarat), `QBE-LOG-001` (pelaku tercatat di baris percobaan dan `UpdateBy`), `QBE-VAL-001` |
| Standar endpoint | `NOT APPLICABLE` — nol endpoint baru |
| Hak akses | `NOT APPLICABLE` — proses latar tanpa pengguna; pelaku = `SystemActorUserId` (`ACC-DEC-074`) |

---

## 1. Masalah yang diperbaiki

Sejak `BE-ACC-P2-021`, kejadian yang gagal dijurnal karena gangguan teknis tetap berstatus
**Diterima** dan diam di sana selamanya. Contoh nyata di kode saat ini: kejadian bertanggal di tahun
yang periode akuntansinya belum dibangkitkan. Pesannya tersimpan, percobaan pertama dicatat gagal
("Belum ada periode akuntansi untuk tanggal …"), lalu tidak ada yang mencobanya lagi.

Akibat kedua: **status Gagal tidak pernah terjadi.** Tidak ada satu baris kode pun yang menulis
`Gagal`, sehingga tombol Abaikan dan Coba Ulang untuk Gagal (`BE-ACC-P2-025`, `FE-ACC-P2-012`) hanya
dapat dibuktikan lewat pembacaan source.

---

## 2. Proses bisnis

| Langkah | Isi |
| --- | --- |
| Pelaku | Sistem (`SystemActorUserId`) — tanpa petugas |
| Pemicu | Setiap 30 detik (dapat diatur) |
| 1 | Ambil kejadian **Diterima** saja, paling lama 100 per siklus, urut dari yang tertua |
| 2 | Kejadian jatuh tempo bila percobaan terakhirnya — atau waktu diterima bila belum ada percobaan — lebih tua dari **masa tenggang (120 detik)** dan dari **jeda coba ulang**: 1 menit sebelum coba ulang ke-1, 5 menit sebelum ke-2, 15 menit sebelum ke-3 |
| 3 | Kejadian "diklaim" dengan menaikkan `AttemptCount` secara bersyarat (`WHERE EventStatus = Diterima AND AttemptCount = <nilai yang dibaca>`). Bila penjadwal lain sudah mengklaimnya, kejadian dilewati |
| 4 | Diproses lewat jalur yang sama dengan penerimaan dan coba ulang manual (`ProsesKejadianAsync`) — aturan posting terkini, periode terbuka berikutnya, perlakuan Posted/Draft |
| Hasil | **Terjurnal** (berhasil), **Tertahan** (aturan/komponen belum ada — tidak diambil lagi, menunggu Coba Ulang manual), atau tetap **Diterima** (gangguan teknis lagi) |
| 5 | Sesudah coba ulang ke-3 tetap gagal → **Gagal**, perpindahan bersyarat `WHERE EventStatus = Diterima` |
| Tidak diambil | Tertahan, Gagal, Terjurnal, Tercatat, Diabaikan |

Contoh lini waktu kejadian bertanggal 2030 (periode belum ada), percobaan pertama pukul 10.00:

| Waktu kira-kira | Yang terjadi | `AttemptCount` | Status |
| --- | --- | :---: | --- |
| 10.00 | Percobaan #1 di dalam request, gagal | 0 | Diterima |
| 10.02 | Coba ulang ke-1 (tenggang 120 detik mengalahkan jeda 1 menit), gagal | 1 | Diterima |
| 10.07 | Coba ulang ke-2, gagal | 2 | Diterima |
| 10.22 | Coba ulang ke-3, gagal | 3 | **Gagal** |

Waktu sebenarnya bergeser paling lama satu jeda polling (30 detik).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Alasan |
| --- | --- |
| `AccountingEvent/Services/AccAccountingEventService.cs` | `ProsesKejadianAsync`, `TahanAsync`, `CatatPercobaanGagalAsync`, `CobaUlangAsync` — jalur yang dipakai ulang |
| `AccountingEvent/Services/AccAccountingEventSchedulerOptions.cs` | Opsi yang sudah dibuat `021` |
| `AccountingEvent/Models/AccAccountingEvent.cs`, `AccAccountingEventAttempt.cs` | `AttemptCount`, `AttemptedAt`, `AttemptNumber` |
| `RecurringJournal/Services/AccRecurringJournalSchedulerHostedService.cs`, `…Options.cs` | Pola hosted service Accounting yang ditiru |
| `JournalManagement/Services/AccJournalService.cs` | Memastikan pelaku `Guid.Empty` diterima pembuatan dan pengesahan jurnal |
| `Program.cs` baris 665–683 | Blok registrasi Accounting; opsi sudah terdaftar sejak `021` |
| `02-backend-architecture.md` bagian 22.5 | Aturan pengambilan |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/AccountingManagement/AccountingEvent/Services/AccAccountingEventSchedulerHostedService.cs` | **Baru.** `BackgroundService`: tombol `Enabled`, `PeriodicTimer`, scope per siklus, siklus gagal dicatat lalu dilanjutkan — sama dengan penjadwal jurnal berulang |
| `Areas/Corporate/AccountingManagement/AccountingEvent/Services/AccAccountingEventService.cs` | + `CobaUlangTerjadwalAsync` (pengambilan, jatuh tempo, klaim, proses, Gagal), + `TandaiGagalTerjadwalAsync`, konstanta `BatasCobaUlangTerjadwal = 3`, jeda 1/5/15 menit, gelombang 100 |
| `Areas/Corporate/AccountingManagement/AccountingEvent/Services/AccAccountingEventSchedulerOptions.cs` | + `Enabled` (bawaan `true`), + `PollIntervalSeconds` (bawaan 30, minimum 15) |
| `Areas/Corporate/AccountingManagement/AccountingEvent/DTOs/AccountingEventDtos.cs` | + `AccountingEventRetryCycleResult` — ringkasan satu siklus untuk log, bukan kontrak API |

**`Program.cs` — diizinkan Rizki 24 September 2026**, satu baris di blok Accounting sesudah registrasi `AccAccountingEventService` (baris 684):

```csharp
    builder.Services.AddHostedService<AccAccountingEventSchedulerHostedService>();
```

`using` untuk namespace-nya sudah ada di baris 11; tidak ada perubahan lain di `Program.cs`.

Nol baris komentar `//` ditambahkan.

### 3.3 Keputusan implementasi

| Hal | Pilihan | Alasan |
| --- | --- | --- |
| Letak aturan | Metode di `AccAccountingEventService`, hosted service hanya memanggil | `QBE-SVC-001`; jalur proses sama persis dengan penerimaan dan coba ulang manual |
| Klaim sebelum proses | `AttemptCount` dinaikkan bersyarat lebih dahulu | Dua instance aplikasi tidak memproses coba ulang yang sama. Bila proses terputus sesudah klaim, coba ulang itu tetap terhitung — tidak berulang tanpa batas |
| `AttemptCount` naik juga saat coba ulang berhasil | Ya | Ia menghitung coba ulang **penjadwal**, bukan kegagalan (bagian 22.5). Kejadian Terjurnal dengan `AttemptCount = 2` berarti terjurnal pada coba ulang kedua |
| Tenggang lawan jeda | Yang lebih panjang yang berlaku | Coba ulang ke-1 praktis 2 menit, bukan 1 menit — tenggang melindungi request yang masih berjalan (bagian 22.5) |
| Kejadian Diterima dengan `AttemptCount` ≥ 3 | Langsung ditandai Gagal tanpa diproses | Hanya terjadi bila aplikasi berhenti di antara coba ulang ke-3 dan penandaan Gagal |
| `Enabled` bawaan `true` | **Berbeda** dari jurnal berulang (bawaan `false`) — **dipilih Rizki 24 September 2026** | Penjadwal ini tidak menerbitkan apa pun yang baru; ia menyelesaikan kejadian yang sudah diterima dan memang seharusnya terjurnal saat itu juga (`FR-P2-013`). Dapat dimatikan lewat `Accounting:AccountingEventScheduler:Enabled` |
| Pelaku | `SystemActorUserId`, atau `Guid.Empty` bila kosong | Sama dengan jurnal berulang. Kunci `Accounting:AccountingEventScheduler:SystemActorUserId` belum ada di `appsettings`, sehingga jurnal hasil penjadwal ber-`CreateBy` kosong sampai diisi |
| Log | `ILogger` ringkasan per siklus (jumlah saja), galat per kejadian dengan nomor kejadian | Pola penjadwal jurnal berulang; nominal dan `SourceTransactionId` tidak dicatat |

### 3.4 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — nol endpoint berubah. `AttemptCount` pada daftar/rincian kini benar-benar bergerak |
| Database | Nol migration. Hanya menulis kolom yang sudah ada: `AttemptCount`, `EventStatus`, `UpdateDateTime`, `UpdateBy`, baris `AccAccountingEventAttempt`, dan jurnal lewat `AccJournalService` |
| Keamanan/Auth | `NOT APPLICABLE` — tanpa permukaan HTTP |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menambah atau mengubah endpoint.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Pemeriksaan source 6 acceptance | Terpetakan semua | `PASS` | Bagian 6 |
| Nol `//` baru | Baris tambahan berisi `//`: 0 | `PASS` | `git diff -U0 \| grep "^+" \| grep -c "//"` |
| `dotnet build` | Berhasil — Rizki, 24 September 2026: `Build succeeded with 222 warning(s) in 448,7s`; nol warning baru dibanding build `024`/`025` | `PASS` | Tangkapan layar keluaran build |
| Uji | Lulus sebagian — waktu percobaan belum dilaporkan | `PASS` untuk (3)(4)(5); belum ada bukti untuk (1)(2) | Bagian 8 |

Uji manual: `PASS` sebagian — Rizki, 24 September 2026 (bagian 8).

```powershell
cd C:\Users\BenariDev03\QuilvianV2\NewQuilvianSystemBackend
dotnet build .\QuilvianSystemBackend.csproj -p:RunAnalyzers=false
```

**Cara memicu gangguan teknis tanpa SQL dan tanpa mengubah kode.** Kirim kejadian bertanggal di
tahun yang periodenya belum dibangkitkan (cek layar Periode Akuntansi; contoh 2030). Jalur `021`
menyimpannya sebagai Diterima dengan percobaan #1 gagal "Belum ada periode akuntansi untuk tanggal …".
Penjadwal lalu mencobanya ulang, dan sampai periode itu ada, setiap coba ulang juga gagal.

**Data yang dipakai:** jenis "Pembayaran Pasien" (`PATIENT_PAYMENT`), yang punya aturan posting aktif
(dipakai `EVT-UJI-002`). Isi pesannya disalin dari Isi Pesan Asli `EVT-UJI-002` di layar Rincian.

| # | Langkah | Diharapkan |
| ---: | --- | --- |
| 1 | Swagger `POST /accounting-events`: salin pesan `EVT-UJI-002`, ganti `EventNumber` = `EVT-UJI-023A`, `SourceTransactionId` = nilai baru, `AccountingDate` = `2030-01-15` | `201`, tanpa `JournalNumber`, status Diterima. Rincian: percobaan #1 gagal "Belum ada periode akuntansi…" |
| 2 | Ulangi dengan `EVT-UJI-023B` dan `SourceTransactionId` lain | Sama seperti #1 |
| 3 | Dalam 2 menit pertama, buka rincian `EVT-UJI-023A` | Masih satu percobaan — tenggang 120 detik (acceptance 1) |
| 4 | Sekitar menit ke-2, ke-7, dan ke-22 sesudah #1, muat ulang rincian | Percobaan #2, #3, #4 muncul satu per satu, semuanya gagal; jarak antarpercobaan ≥ 5 dan ≥ 15 menit (acceptance 2) |
| 5 | Sesudah percobaan #4 | Status **Gagal**; tab Gagal di Kotak Masuk menampilkan angka 2 (acceptance 4) |
| 6 | Rincian `EVT-UJI-023A` → **Abaikan** dengan alasan | Status Diabaikan, alasan tampil — jalur yang sebelumnya hanya dibuktikan lewat source (`BE-ACC-P2-025` bagian 6.2, `FE-ACC-P2-012` bagian 8.2) |
| 7 | Rincian `EVT-UJI-023B` → **Coba Ulang** | Tombol aktif; percobaan #5 gagal; status tetap Gagal; `AttemptCount` tetap 3 — coba ulang manual tidak menambah hitungan (acceptance 3) |
| 8 | Biarkan backend berjalan beberapa menit lagi | Tidak ada percobaan baru pada kedua kejadian — Gagal dan Diabaikan tidak diambil (acceptance 5) |
| 9 | *(opsional — hanya bila Rizki rela periode 2030 ada di database dev)* Bangkitkan periode 2030 lewat layar Periode Akuntansi, lalu Coba Ulang `EVT-UJI-023B` | Terjurnal di periode 2030-01 — Coba Ulang Gagal yang berhasil |

Log backend menampilkan satu baris "Siklus coba ulang kejadian selesai…" pada setiap siklus yang
mengambil kejadian (#4).

**Tidak dijalankan:** automated test — dilarang `ACC-DEC-081`. Acceptance (6) dibuktikan lewat source.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Hanya kejadian Diterima yang percobaan terakhirnya lebih tua dari masa tenggang yang diambil | Terpenuhi di source — **bukti uji belum ada** | `Where(EventStatus == Diterima)`; jatuh tempo `dasar + max(tenggang, jeda) <= sekarang`, `dasar` = percobaan terakhir atau waktu diterima. Uji: #3 — butuh waktu percobaan #1 dan #2 (bagian 8.2) |
| (2) Jeda 1, 5, 15 menit | Terpenuhi di source — **bukti uji belum ada** | `JedaCobaUlangTerjadwal` diindeks `AttemptCount`. Uji: #4 — butuh waktu percobaan #2, #3, #4 (bagian 8.2) |
| (3) Percobaan dalam request tidak menambah `AttemptCount` | Terpenuhi | Hanya `CobaUlangTerjadwalAsync` yang menulis `AttemptCount`; `TerimaAsync`, `ProsesKejadianAsync`, `CobaUlangAsync` tidak. Uji: #1 (`AttemptCount = 0`), #7 |
| (4) Sesudah tiga coba ulang gagal → Gagal | Terpenuhi | `hitunganBaru >= BatasCobaUlangTerjadwal` → `TandaiGagalTerjadwalAsync`. Uji: #5 |
| (5) Tertahan dan Gagal tidak diambil | Terpenuhi | Filter `EventStatus == Diterima`. Uji (tidak langsung, bagian 8): `EVT-UJI-023B` tetap Gagal sesudah periode 2030 dibangkitkan sampai dicoba ulang manual — penjadwal tidak menyentuhnya walau jurnalnya kini dapat terbentuk; `EVT-UJI-026A` tetap Tertahan sampai Coba Ulang manual |
| (6) Perpindahan status bersyarat — request dan penjadwal bersamaan tetap satu jurnal | Terpenuhi di source | Klaim `AttemptCount` bersyarat; `ProsesKejadianAsync` menulis Terjurnal `WHERE EventStatus = statusAsal` di dalam transaksi dan membatalkan jurnalnya bila 0 baris; Gagal `WHERE EventStatus = Diterima`. Tidak dapat dipicu manual — dibuktikan lewat source (risiko yang sudah disebut kartu) |
| DoD: source berubah | Terpenuhi | Bagian 3.2 |
| DoD: build owner 0 error | Terpenuhi | Build Rizki berhasil, 222 warning |
| DoD: laporan task tertulis | Terpenuhi | Berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Begitu terdaftar, penjadwal langsung aktif di setiap backend yang berjalan — termasuk milik pengembang lain yang menarik branch ini — karena bawaan `Enabled = true` |
| Masalah yang diketahui | Pengambilan dibatasi 100 kejadian Diterima tertua per siklus. Lebih dari itu, sisanya menunggu siklus berikutnya. Wajar untuk status yang hanya lahir dari gangguan teknis |
| Risiko tersisa | `SystemActorUserId` belum diisi di konfigurasi, sehingga jurnal hasil penjadwal ber-`CreateBy` kosong. Kolom Jurnal "Dibuat oleh" akan kosong untuk jurnal itu |
| Frontend | Kartu ini **tidak punya pasangan frontend** di roadmap. Layar yang ada sudah menampilkan status Gagal, tab Gagal, riwayat percobaan, dan tombol Abaikan/Coba Ulang untuk Gagal (`FE-ACC-P2-011`, `012`). Uji #5–#7 sekaligus menguji ulang jalur Gagal `FE-ACC-P2-012` yang sebelumnya dibuktikan lewat source |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Backend: `M` `Program.cs` (+1 baris), `AccountingEventDtos.cs`, `AccAccountingEventSchedulerOptions.cs`, `AccAccountingEventService.cs`; `??` `AccAccountingEventSchedulerHostedService.cs`, laporan ini. Dokumen penandaan ✅ `FE-ACC-P2-011`–`013` sesi yang sama juga belum di-commit |
| Langkah berikutnya | Rizki: kirim tangkapan layar Riwayat Percobaan `EVT-UJI-023A` (bagian 8.2); sesudah itu task ini dapat dinaikkan ke ✅. Commit oleh Rizki |

---

## 8. Bukti uji — Rizki, 24 September 2026

### 8.1 Yang terbukti

| Skenario | Yang dilaporkan Rizki | Acceptance | Hasil |
| --- | --- | --- | :---: |
| 1–2 | `EVT-UJI-023A` dan `023B` bertanggal 2030-01-15 → `201`, Diterima, tanpa nomor jurnal, alasan "Belum ada periode akuntansi untuk tanggal 2030-01-15" | Prasyarat | ✅ |
| 4–5 | Penjadwal mencoba ulang otomatis, semuanya gagal, status akhir **Gagal** | (4) | ✅ |
| 6 | `023A` → Abaikan tanpa alasan ditolak; dengan alasan "Testing ignore event" → Diabaikan | Jalur Gagal `BE-ACC-P2-025` | ✅ |
| 7 | `023B` → Coba Ulang manual; "AttemptCount tetap sesuai acceptance" | (3) | ✅ |
| `026` B1–B4 | Sesudah periode 2030 dibangkitkan, `023B` tetap Gagal (daftar periksa Januari 2030 = 1) sampai dicoba ulang manual → Terjurnal | (5) — penjadwal tidak mengambil Gagal | ✅ |
| `026` A2–A6 | `EVT-UJI-026A` tetap Tertahan sampai Coba Ulang manual | (5) — penjadwal tidak mengambil Tertahan | ✅ |
| Log | Registrasi penjadwal berhasil (dilaporkan "Scheduler Registration PASS") | — | ✅ |

Nol SQL langsung. UAT belum dijalankan — diserahkan ke tim UAT.

### 8.2 Yang belum terbukti — acceptance (1) dan (2)

Laporan uji menuliskan **tiga** percobaan (#1–#3) lalu Gagal, dengan konfigurasi "Retry Delay: 120 detik".
Menurut kode, kejadian menjadi Gagal sesudah **tiga coba ulang penjadwal**, sehingga riwayatnya memuat
**empat** baris:

| Percobaan | Asal | Kapan, dihitung dari #1 |
| --- | --- | --- |
| #1 | Request `POST` | 0 |
| #2 | Penjadwal, coba ulang ke-1 | ±2 menit (tenggang 120 detik mengalahkan jeda 1 menit) |
| #3 | Penjadwal, coba ulang ke-2 | ±5 menit sesudah #2 (±7 menit dari #1) |
| #4 | Penjadwal, coba ulang ke-3 → Gagal | ±15 menit sesudah #3 (±22 menit dari #1) |

Kemungkinan laporan uji hanya menghitung percobaan penjadwal. Untuk menutupnya cukup **satu tangkapan
layar Riwayat Percobaan `EVT-UJI-023A`** yang menampilkan nomor dan waktu tiap percobaan. `023A` sudah
Diabaikan, tetapi riwayatnya tetap tersimpan dan tampil di rincian.

- Bila tampil empat baris dengan jarak seperti tabel di atas → acceptance (1) dan (2) terbukti, task ✅.
- Bila tampil tiga baris, atau jaraknya tetap 120 detik → ada selisih dengan kode yang perlu diperiksa sebelum ✅.
