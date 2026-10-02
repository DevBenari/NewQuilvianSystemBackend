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
| Commit backend saat dikerjakan | `d62a084e` (branch `rizkiG`); di-commit Rizki `1ea09d66` |
| Tanggal | 24 September 2026; bukti uji 25 September 2026 |
| Status | **✅ SELESAI** — 25 September 2026. 6 dari 6 acceptance: (1)–(5) terbukti dari response Swagger mentah Rizki (`EVT-UJI-105`: 4 percobaan berjarak 2 menit 25 detik, 5 menit 30 detik, 15 menit 30 detik, `attemptCount` 3; `EVT-UJI-112` Gagal; `EVT-UJI-001` `attemptCount` 0), (6) lewat source sesuai kartu. Build Rizki berhasil, 222 warning. UAT belum dijalankan — diserahkan ke tim UAT. Riwayat: 🟡 24 September 2026 |

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
| Uji | Lulus — response Swagger mentah Rizki, 25 September 2026 | `PASS` | Bagian 8 |

Uji manual: `PASS` — Rizki, 25 September 2026 (bagian 8). Skenario 1–9 di bawah adalah rencana awal; yang
benar-benar dijalankan memakai `EVT-UJI-105` dan `EVT-UJI-112` (bagian 8).

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
| (1) Hanya kejadian Diterima yang percobaan terakhirnya lebih tua dari masa tenggang yang diambil | Terpenuhi | `Where(EventStatus == Diterima)`; jatuh tempo `dasar + max(tenggang, jeda) <= sekarang`. Uji: `EVT-UJI-105` percobaan #1 09.13.46 → #2 09.16.11, selisih **2 menit 25 detik** ≥ tenggang 120 detik (bagian 8.1) |
| (2) Jeda 1, 5, 15 menit | Terpenuhi | `JedaCobaUlangTerjadwal` diindeks `AttemptCount`. Uji: #2 → #3 **5 menit 30 detik**, #3 → #4 **15 menit 30 detik** (bagian 8.1) |
| (3) Percobaan dalam request tidak menambah `AttemptCount` | Terpenuhi | Hanya `CobaUlangTerjadwalAsync` yang menulis `AttemptCount`. Uji: `EVT-UJI-105` punya 4 percobaan tetapi `attemptCount: 3` — percobaan #1 dari `POST` tidak dihitung |
| (4) Sesudah tiga coba ulang gagal → Gagal | Terpenuhi | `hitunganBaru >= BatasCobaUlangTerjadwal` → `TandaiGagalTerjadwalAsync`. Uji: `EVT-UJI-105` berakhir Diabaikan dengan alasan — hanya mungkin dari Gagal; `EVT-UJI-112` terhitung Gagal pada daftar periksa Januari 2031 |
| (5) Tertahan dan Gagal tidak diambil | Terpenuhi | Filter `EventStatus == Diterima`. Uji: tidak ada percobaan otomatis sesudah #4 pada `EVT-UJI-105`; `EVT-UJI-001` Tertahan sejak 24 September dengan `attemptCount: 0` — penjadwal tidak pernah mengklaimnya |
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
| Status Git | Source di-commit Rizki `1ea09d66` (24 September 2026); `rizkiG` sudah di-merge dengan integration (`68ebc667`), registrasi penjadwal tetap di `Program.cs` baris 712. Pembaruan laporan ini belum di-commit |
| Langkah berikutnya | Tidak ada untuk task ini. UAT oleh tim UAT |

---

## 8. Bukti uji

### 8.1 Bukti yang dipakai — response Swagger mentah, Rizki, 25 September 2026

**`GET /accounting-events/9fc887be-aee4-4d71-82cf-57d696803528` (`EVT-UJI-105`)**, dikirim 09.13.46 WIB
bertanggal akuntansi 2031-01-15, saat periode 2031 belum ada:

| Percobaan | `attemptedAt` | Selisih dari sebelumnya | Hasil | Acceptance |
| --- | --- | --- | --- | --- |
| #1 (`POST`) | 09.13.46,48 | — | Gagal: "Belum ada periode akuntansi untuk tanggal 2031-01-15…" | — |
| #2 (penjadwal) | 09.16.11,10 | **2 menit 24,6 detik** | Gagal | (1) ≥ tenggang 120 detik |
| #3 (penjadwal) | 09.21.40,90 | **5 menit 29,8 detik** | Gagal | (2) ≥ 5 menit |
| #4 (penjadwal) | 09.37.11,24 | **15 menit 30,3 detik** | Gagal → status Gagal | (2) ≥ 15 menit; (4) |

Bidang lain pada response yang sama: `attemptCount: 3` untuk empat percobaan — acceptance (3);
`eventStatus: 5` (Diabaikan) dengan `ignoreReason: "Membersihkan data uji"`, yang hanya mungkin dari Gagal —
acceptance (4); tidak ada percobaan otomatis sesudah #4 — acceptance (5). Kelebihan ±25–30 detik pada tiap
jarak sesuai jeda polling 30 detik.

**`EVT-UJI-112`** (dikirim untuk `BE-ACC-P2-026` skenario B, tanggal 2031-01-15): daftar periksa Januari
2031 pukul 13.43 menghitung satu kejadian Gagal, dan Coba Ulang manual pukul 13.50 tercatat sebagai
percobaan **#5** — empat percobaan sebelumnya (#1 dari `POST`, #2–#4 dari penjadwal) berakhir Gagal.
Pengulangan acceptance (4) pada kejadian kedua.

**`GET /accounting-events?Search=EVT-UJI-001`**: `eventStatus: 2` (Tertahan) sejak 2026-09-24 06.33 UTC,
`attemptCount: 0` — lebih dari sehari penjadwal tidak pernah mengklaim kejadian Tertahan. Acceptance (5).

Nol SQL langsung. UAT belum dijalankan — diserahkan ke tim UAT.

**Catatan zona waktu.** `attemptedAt` dikembalikan dalam WIB (`+07:00`), bukan UTC seperti perkiraan
sebelumnya; `receivedAt` dalam UTC (`Z`).

### 8.2 Bukti yang dicabut

Laporan uji 24 September 2026 (bagian 8 versi sebelumnya) disusun oleh agen AI penguji dan menyebut
kejadian `EVT-UJI-023A` dan `EVT-UJI-023B`. Kotak Masuk di database dev pada 25 September 2026 memuat
lima kejadian saja, tanpa keduanya, dan `EVT-UJI-001` yang dilaporkan Terjurnal masih Tertahan. Seluruh
bukti versi sebelumnya karena itu **tidak dipakai**; acceptance ditutup hanya dengan bukti bagian 8.1.
