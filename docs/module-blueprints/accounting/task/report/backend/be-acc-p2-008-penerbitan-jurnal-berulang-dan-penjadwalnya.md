# Laporan Perubahan Backend — `BE-ACC-P2-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-008` |
| Judul | Penerbitan jurnal berulang dan penjadwalnya |
| Slice | `P2-3` — jurnal berulang (`ACC-P2-S2`) |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-008` |
| Trace | `ACC-DEC-050`; `FR-P2-019`, `FR-P2-020`, `FR-P2-021` |
| Contract version | `ACC-API-0.8` `POST /{id}/generate`, `ACC-VALIDATION-0.6` bagian 3, `ACC-STATE-0.3` bagian 3, `ACC-PERMISSION-0.4` — seluruhnya `approved` |
| Dependency | `BE-ACC-P2-007` ✅ (selesai pada sesi yang sama) |
| Klasifikasi | `HEAVY` — skor 9: repository 1 (0), berkas diperiksa > 20 (2), berkas diubah 4–8 (1), logika bisnis kompleks (2), memakai kontrak yang sudah ada (1), perilaku persistence yang sudah ada (1), keamanan berkaitan tetapi bukan intinya (1), workflow terbatas (1) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi, `UnitTests.Sqlite`, dan `docs/module-blueprints/accounting/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `3e2fb76ee8482faacc3f4b3c6ad0b226bee960cf`, branch `rizkiG` |
| Tanggal | 10 September 2026 |
| Status | **✅ `DONE`** — empat dari empat acceptance terbukti; **acceptance (1) dibuktikan pada PostgreSQL sungguhan** dengan dua koneksi benar-benar bersamaan; nol migration |

---

## 1. Masalah yang diperbaiki

`BE-ACC-P2-007` membuat template dapat disimpan, tetapi belum ada yang menerbitkannya. Tanpa
task ini template hanyalah catatan yang tidak pernah menjadi jurnal.

**Bahaya yang sesungguhnya bukan lupa menerbitkan, melainkan menerbitkan dua kali.** Penyusutan
Rp 5.000.000 yang tercatat dua kali untuk bulan yang sama membuat beban bulan itu Rp 10.000.000.
Jurnalnya **tetap seimbang**, keduanya sah, dan tidak ada satu pun error. Yang ketahuan hanya
labanya lebih kecil daripada seharusnya — dan itu baru terlihat, kalau terlihat, saat audit.

Keadaan yang memicunya sangat biasa: layanan dimuat ulang di tengah siklus penjadwal, atau
aplikasi berjalan lebih dari satu instance dan keduanya sampai pada template yang sama pada
detik yang sama.

---

## 2. Proses bisnis

**Tujuan.** Template aktif menerbitkan jurnal `Draft` sendiri setiap periode, tepat satu kali.

**Pelaku.** Penjadwal, tanpa manusia. Penerbitan manual di luar jadwal dilakukan Staf Akuntansi.

### Langkah normal

1. **Penjadwal berjalan** pada jeda yang ditentukan konfigurasi, bawaannya sekali sejam.
2. **Menunggu jam jatuh tempo.** Bawaannya pukul 02.00 waktu Jakarta — sebelum jam kerja,
   supaya draft-nya sudah menunggu saat petugas membuka layar.
3. **Mengambil template aktif yang jatuh tempo**: `DayOfMonth` sudah lewat atau sama dengan
   hari ini, dan tanggal hari ini masih di dalam masa berlakunya.
4. **Untuk setiap template, sistem memeriksa** apakah barisnya masih layak, periode tujuannya
   menerima pencatatan, dan template itu belum pernah terbit untuk periode itu.
5. **Menerbitkan jurnal `Draft`** yang menyalin baris template apa adanya, **beserta unit
   biayanya**, lalu mencatat baris penerbitan sebagai bukti. Keduanya dalam satu transaction.
6. **Jurnalnya menempuh daur hidup biasa** — diajukan, disetujui, disahkan manusia. Sistem tidak
   pernah mengesahkan jurnal berulang sendiri (`ACC-DEC-050`).

### Contoh berangka

Template `SUSUT-ALKES` terbit tanggal 25, dua baris: `5-2003 Beban Penyusutan` didebit
Rp 5.000.000 dengan unit biaya Umum, dan `1-4001 Akumulasi Penyusutan` dikredit Rp 5.000.000.

Penjadwal berjalan 27 September. Yang terjadi: jurnal `JP/2026/09/00001` terbit **bertanggal
25 September**, bukan 27. Ini disengaja — bila tanggalnya ikut bergeser, jurnal penyusutan
September dapat mendarat di periode Oktober ketika penjadwal sempat mati beberapa hari, dan
tidak ada error apa pun yang menandainya.

Penjadwal berjalan lagi 28 September. Template itu **dilewati tanpa suara**: ia memang sudah
terbit. Yang bertambah nol.

### Siapa yang benar-benar mencegah penerbitan ganda

Ada dua lapisan, dan membedakannya menentukan:

| Lapisan | Apa yang dilakukannya | Apa yang **tidak** dilakukannya |
| --- | --- | --- |
| Pemeriksaan di kode | Memberi pesan yang enak dibaca, dan menghemat pekerjaan sia-sia | **Tidak mencegah apa pun** saat dua proses berjalan bersamaan — keduanya dapat sama-sama lolos memeriksa sebelum salah satunya menyimpan |
| Unique index `(TemplateId, AccountingPeriodId)` | **Mencegah sungguhan.** Dua proses dapat sama-sama lolos pemeriksaan, tetapi tidak dapat sama-sama lolos dari database | — |

Karena itu `DbUpdateException` pada jalur penerbitan **tidak dilemparkan ulang**, melainkan
diterjemahkan menjadi `409`. Ia bukan kegagalan sistem; ia penjaga yang bekerja persis
sebagaimana mestinya.

### Jalur tidak normal

| Keadaan | Penerbitan manual | Penjadwal |
| --- | --- | --- |
| Template tidak aktif | `409` "sedang tidak aktif" | Tidak dilihat sama sekali — bukan kandidat |
| Sudah terbit untuk periode itu | `409` beserta nomor jurnalnya | **Dilewati tanpa peringatan** — ini keadaan normal |
| Periode tujuan tidak menerima pencatatan | `422` | **Dilewati dan diperingatkan** — ini keadaan bermasalah |
| Belum ada periode untuk tanggal itu | `422` | Dilewati dan diperingatkan |
| Barisnya tidak lagi layak | `409` beserta nomor barisnya | Dilewati dan diperingatkan |
| Template sudah berakhir masa berlakunya | `409` | Bukan kandidat |
| Template belum mulai berlaku | `409` | Bukan kandidat |
| Siklus penjadwal gagal seluruhnya | — | Dicatat sebagai error; **penjadwal tetap berjalan**, tidak berhenti sampai aplikasi dimuat ulang |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`roadmap/backend-roadmap-phase2.md` kartu `008`; `contracts/api-contract.md`;
`contracts/validation-matrix.md` Phase 2 bagian 3; `contracts/permission-audit-matrix.md`;
`LeaveAccrualSchedulerHostedService.cs` dan `LeaveAccrualSchedulerOptions.cs` sebagai pola yang
ditunjuk kartu; `AccJournalService.cs` (`CreateAsync`, `AlokasikanNomorJurnalAsync`);
`AccAccountingPeriodService.AlasanPenolakanJenisJurnal`; `AccRecurringJournalRunConfiguration.cs`;
`Program.cs`; serta governance yang sama seperti `007`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `RecurringJournal/Services/AccRecurringJournalService.cs` | **Diperluas.** `GenerateAsync`, `TerbitkanYangJatuhTempoAsync`, `TerbitkanAsync`, `SiapkanPenerbitanAsync`, `CariPenerbitanAsync`. Konstruktor bertambah `AccJournalService` |
| `RecurringJournal/Services/AccRecurringJournalSchedulerOptions.cs` | **Baru.** Termasuk tombol `Enabled` yang **berbawaan mati** |
| `RecurringJournal/Services/AccRecurringJournalSchedulerHostedService.cs` | **Baru.** Meniru `LeaveAccrualSchedulerHostedService` |
| `RecurringJournal/DTOs/RecurringJournalCycleDtos.cs` | **Baru.** Hasil satu siklus penjadwal; bukan kontrak API |
| `RecurringJournal/DTOs/RecurringJournalDtos.cs` | **Diubah.** `GenerateRecurringJournalRequest` |
| `RecurringJournal/Controllers/RecurringJournalController.cs` | **Diubah.** Endpoint `POST /{id}/generate` |
| `Program.cs` | **Diubah.** Tiga baris: `Configure<AccRecurringJournalSchedulerOptions>` dan `AddHostedService` |
| `Tests/…/AccRecurringJournalGenerateTests.cs` | **Baru.** 14 uji |
| `Tests/…/AccRecurringJournalTemplateTests.cs` | **Diubah.** Menyesuaikan konstruktor dan jumlah endpoint |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Satu endpoint baru**, `POST /{id}/generate`. Nol endpoint yang sudah ada berubah |
| Database | **Nol migration, nol entity baru, snapshot tidak disentuh.** Unique index penjaganya sudah berdiri sejak `BE-ACC-P2-002` dan diterapkan `BE-ACC-P2-004`; keberadaannya **diverifikasi langsung** ke `QuilvianNewDevRizki` — lihat bagian 5 |
| Keamanan/Auth | Satu hak akses baru `RecurringJournal : Generate`. **Nol pemeriksaan peran yang dihardcode.** Nominal jurnal tidak masuk log |
| Runtime | **Satu hosted service baru.** Bawaannya **MATI** — berbeda dari penjadwal cuti yang bawaannya hidup, penjadwal ini menulis ke buku besar |

---

## 4. Dokumentasi endpoint

#### Corporate / Accounting / Recurring Journal

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id}/generate` | Menerbitkan jurnal template untuk satu periode secara manual, di luar jadwal | `RecurringJournal : Generate` |

Badan permintaannya `GenerateRecurringJournalRequest` berisi `accountingDate` yang tidak wajib;
dikosongkan berarti memakai `DayOfMonth` template pada bulan berjalan. **Periodenya diturunkan
dari tanggal**, tidak pernah dikirim sebagai `accountingPeriodId` — alasannya sama dengan jurnal
manual: menerimanya langsung membuka jalan jurnal tercatat di periode yang tidak sesuai
tanggalnya.

### Konfigurasi penjadwal

Bagian `Accounting:RecurringJournalScheduler` pada `appsettings`:

| Key | Bawaan | Kegunaan |
| --- | --- | --- |
| `Enabled` | **`false`** | Tombol utama. Selama `false`, penjadwal berhenti sebelum timer dibuat |
| `PollIntervalSeconds` | `3600` | Jeda antar-siklus; dibatasi minimum 60 detik |
| `TimeZoneId` | `Asia/Jakarta` | Zona waktu penentuan jam jatuh tempo |
| `DailyRunHour` / `DailyRunMinute` | `2` / `0` | Jam lokal paling awal boleh menerbitkan |
| `SystemActorUserId` | kosong | Pelaku yang tercatat sebagai pembuat jurnal terbitan |
| `WorkerInstanceName` | `quilvian-recurring-journal-scheduler` | Penanda pada log |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build ./QuilvianSystemBackend.sln -c Release -p:RunAnalyzers=false --no-incremental` | Berhasil | `PASS` | `0 Error(s)`, `200 Warning(s)` — **nol warning dari berkas task ini** |
| `dotnet test … --filter "FullyQualifiedName~AccRecurringJournal"` | Berhasil | `PASS` | `Failed: 0, Passed: 46` (32 milik `007`, 14 milik `008`) |
| `dotnet test … UnitTests.Sqlite` (seluruh project) | Berhasil | `PASS` | `Failed: 0, Passed: 604` — **nol regresi** |
| `./tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | Berhasil | `PASS` | `VIOLATION: 0`, `Final result: PASS` |
| **Konkurensi dua koneksi terpisah pada PostgreSQL sungguhan** | Berhasil | `PASS` | Lihat di bawah |
| Verifikasi unique index terpasang di `QuilvianNewDevRizki` | Berhasil | `PASS` | `pg_indexes` melaporkan `CREATE UNIQUE INDEX "IX_AccRecurringJournalRun_TemplateId_AccountingPeriodId" … ("TemplateId", "AccountingPeriodId")` |

Uji manual: `NOT APPLICABLE` — endpoint belum punya layar.

### Uji konkurensi PostgreSQL — inti acceptance (1)

Kartu menuntut pembuktian bahwa penerbitan kedua ditolak **oleh database**, dan menegaskan
bahwa menguji secara berurutan *"tidak membuktikan apa pun"*. Karena itu pengujiannya dijalankan
langsung terhadap `QuilvianNewDevRizki` dengan **dua koneksi PostgreSQL terpisah yang berangkat
bersamaan**, disinkronkan lewat barrier supaya keduanya benar-benar berlomba.

| Langkah | Hasil |
| --- | --- |
| Koneksi A dan B berangkat bersamaan, menyisipkan pasangan `(TemplateId, AccountingPeriodId)` yang sama | — |
| Koneksi A | **BERHASIL** |
| Koneksi B | **DITOLAK** — constraint `IX_AccRecurringJournalRun_TemplateId_AccountingPeriodId` |
| Baris penerbitan tersimpan | **1** |

Percobaan pertama sempat memakai `lock_timeout` dan berujung `LockNotAvailable` — itu **timeout
menunggu**, bukan penolakan index, dan tidak membuktikan apa yang dituntut. Percobaan diulang
tanpa timeout sehingga koneksi B benar-benar menunggu A selesai, lalu ditolak unique index
dengan nama constraint yang tercatat.

**Data uji yang dibuat dan dihapus.** Satu baris `AccRecurringJournalTemplate` berkode
`UJI-KONKURENSI-P2-008`, dan satu baris `AccRecurringJournalRun`. **Keduanya dihapus** pada
akhir pengujian; hitungan akhir `AccRecurringJournalTemplate` = 0 baris dan
`AccRecurringJournalRun` = 0 baris. Badan hukum, jenis jurnal, periode, dan jurnal yang dipakai
sebagai induk **sudah ada sebelumnya** dan tidak disentuh. **Nol DDL, nol migration.**

### Yang diuji SQLite, dan batasnya

SQLite membuktikan jalur kodenya: penerbitan kedua berurutan ditolak `409` dan tetap
menghasilkan satu jurnal; unique index menolak baris kedua dengan `DbUpdateException`; dan
service menerjemahkannya menjadi `409`, bukan meledak menjadi `500`. Yang **tidak** dapat
dibuktikan di sana adalah konkurensi sungguhan — SQLite dalam memori memakai satu koneksi
bersama. Itulah yang ditutup uji PostgreSQL di atas.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Penerbitan dua kali untuk periode yang sama hanya menghasilkan **satu** jurnal, dan yang kedua ditolak **oleh database** | **Terpenuhi** | Dua lapis. SQLite: `PenerbitanKedua_DitolakDanTetapSatuJurnal` dan `UniqueIndexDatabase_MenolakBarisPenerbitanKedua`. **PostgreSQL sungguhan**: dua koneksi bersamaan, A berhasil, B ditolak constraint `IX_AccRecurringJournalRun_TemplateId_AccountingPeriodId`, tersimpan tepat 1 baris |
| (2) Periode tidak menerima pencatatan ⇒ **dilewati, bukan gagal** | **Terpenuhi** | `Penjadwal_MelewatiYangBermasalahDanTetapMenerbitkanSisanya` — satu template terbit, satu dilewati, siklusnya tetap selesai. `Penjadwal_PeriodeTutupDihitungSebagaiBermasalahBukanSudahTerbit` membuktikan keduanya dibedakan. `PeriodeTutupPermanen_PenerbitanDitolak422` untuk jalur manual |
| (3) Template nonaktif tidak menerbitkan apa pun | **Terpenuhi** | `TemplateNonaktif_TidakMenerbitkanApaPun` — `409` pada jalur manual, dan penjadwal tidak melihatnya sama sekali (`Considered = 0`); nol jurnal dan nol baris penerbitan tersimpan |
| (4) Penjadwal dapat dimatikan lewat konfigurasi | **Terpenuhi** | `PenjadwalDimatikan_SelesaiTanpaMenyentuhDatabase` memakai `IServiceScopeFactory` yang **melemparkan pengecualian bila dipanggil** — selesai tanpa melempar berarti penjadwal benar-benar tidak berjalan. `PenjadwalBawaannya_Mati` menjaga bawaannya |

### Definition of Done

| Butir | Status |
| --- | --- |
| Penjadwal berjalan | **Terpenuhi** — terdaftar `AddHostedService`, bawaannya mati |
| Test konkurensi hijau | **Terpenuhi** — PostgreSQL sungguhan, dua koneksi terpisah |
| Laporan task tertulis | **Terpenuhi** — berkas ini |
| Nol migration | **Terpenuhi** |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 200 warning pada build `Release`, seluruhnya sudah ada sebelumnya; nol dari berkas task ini |
| Masalah yang diketahui | Penjaga `_tanggalTerakhirDiproses` pada hosted service **hanya berlaku dalam satu proses**. Aplikasi yang berjalan lebih dari satu instance akan menjalankan siklusnya masing-masing. Itu **tidak berbahaya** — unique index tetap menjaga terbit ganda — tetapi berarti pekerjaan yang sama dikerjakan berulang. Penyelesaiannya adalah kunci terdistribusi, di luar cakupan task ini |
| Risiko tersisa | Penjadwal bawaannya **mati**, jadi jurnal berulang tidak akan terbit sampai `Accounting:RecurringJournalScheduler:Enabled` diisi `true` pada `appsettings`. Ini disengaja, tetapi perlu diketahui supaya tidak disangka cacat |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Langkah berikutnya | Menyalakan penjadwal setelah daftar akun dan template sungguhan siap. `FE-ACC-P2-003`/`004` layar template |

### Delta kontrak

| Delta | Isi | Kenapa |
| --- | --- | --- |
| **`GenerateRecurringJournalRequest` membawa `accountingDate`, bukan `accountingPeriodId`** | Kontrak menyebut "menerbitkan jurnal draft untuk satu periode" tanpa merinci bentuknya | Periode adalah **turunan** tanggal akuntansi di seluruh modul ini. Menerima `accountingPeriodId` langsung membuka jalan jurnal tercatat di periode yang tidak sesuai tanggalnya — dan jurnalnya tetap seimbang, jadi tidak ada yang menandainya |
| **Bawaan penjadwal `Enabled = false`** | `LeaveAccrualSchedulerOptions` yang ditiru berbawaan `true` | Penjadwal cuti menghitung hak cuti; penjadwal ini **menulis ke buku besar**. Sebuah pemasangan yang daftar akunnya belum siap akan menerbitkan jurnal ke buku besar sungguhan sejak hari pertama |
| **Hasil siklus dikembalikan sebagai objek, bukan hanya dicatat log** | Tidak diatur kontrak | Supaya perilaku "dilewati, bukan gagal" pada acceptance (2) dapat diuji tanpa membaca log, dan supaya "sudah terbit" dapat dibedakan dari "bermasalah" — hanya yang kedua yang layak diperingatkan |
