# Human Resource — Matriks Perpindahan Status

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Dokumen | `contracts/state-transition-matrix.md` |
| `contract_version` | `v6` |
| `last_changed_in` | `v7` — bagian 11 (`S-D1`) ditambahkan 3 November 2026; bagian lain tidak berubah sejak `v6` |
| Status | `draft` — **belum** `approved` |
| Owner | Technical owner (`HRD-DEC-015`) |
| `approved_by` / `approved_at` | **Hanya bagian 11** (`S-D1` Perencanaan Tenaga Kerja): `approved` oleh Pengguna — pemilik teknis dan produk (`HRD-DEC-015`) — pada 2026-11-03. Bagian lain dokumen ini **belum** disetujui, sehingga status dokumen tetap `draft` |
| `input_revision` | `00-interview-decisions.md` revision `16` (`HRD-DEC-046` s.d. `HRD-DEC-057`); `flows/` 15 berkas |
| `input_hash` — decision log | `26da64e129b0c34a17b67b0a41f47986b1aedae92a600077d936b0741eb574ee` |
| Backend SHA | `e0ee42c752a5f92c5b1663ff88bef07a5859f79f` |
| Dampak kompatibilitas | Tidak ada nilai status yang dihapus. Satu nilai baru ditambahkan pada kosakata jenis pengecualian kehadiran |

---

## 0. Cara membaca dokumen ini

Dokumen ini adalah **satu-satunya** tempat daftar lengkap perpindahan status hidup di seluruh
blueprint HR. Flowchart pada `flowcharts/` menggambar urutan langkah petugas dan **MUST NOT**
menyalin isi dokumen ini.

### 0.1 Dua hal yang sering tertukar, dan akibatnya mahal

| Istilah | Artinya | Cara membuktikannya |
| --- | --- | --- |
| **Kosakata status** | Nilai statusnya benar-benar ada di kode | Ditemukan pada konstanta atau enum |
| **Perpindahan yang dijaga** | Ada kode yang benar-benar memeriksa status asal sebelum mengizinkan tindakan | Ditemukan guard yang membandingkan status **sekarang** |

Sebuah nilai status yang ada **bukan berarti** perpindahannya dijaga. Beberapa baris pada
dokumen ini punya kosakata yang jelas tetapi perpindahannya **tidak dijaga sama sekali** — dan
itu ditulis apa adanya, bukan disembunyikan.

### 0.2 Penanda yang dipakai

| Penanda | Artinya |
| --- | --- |
| `[EXISTING]` | Perpindahan ini benar-benar dijaga kode pada baseline saat ini |
| `[VOCAB]` | Nilai statusnya ada, tetapi **tidak ada** kode yang menjaga perpindahannya |
| `[DECISION]` | Perpindahan target yang ditetapkan `HRD-DEC-xxx`, belum tentu sudah ada di kode |
| `[DEFECT]` | Perpindahan yang **terjadi hari ini** tetapi **bertentangan** dengan keputusan target |
| `[OPEN]` | Belum ada keputusan pihak berwenang |

---

## 1. Kehadiran

### 1.1 Periode kehadiran — `AttendancePeriodStatus`

Nilai: `Open`, `Closing`, `Closed`, `Reopened`, `Cancelled`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Open` atau `Reopened` | Mulai tutup | `Closing` | Pemegang `AttendancePeriod : Close` | Tidak ada pengecualian pemblokir berstatus `Open`/`UnderReview`; tidak ada permohonan koreksi yang masih berjalan | Ditolak `409`. Petugas melihat daftar penghalangnya lewat `close-preview` `[EXISTING]` |
| `Closing` | Selesai tutup | `Closed` | Sistem, bagian dari alur tutup yang sama | Seluruh hari dalam periode sudah terproses | Periode tetap `Closing`; petugas menjalankan ulang `[EXISTING]` |
| `Closed` | Buka kembali | `Reopened` | Pemegang `AttendancePeriod : Reopen` | Statusnya **harus** `Closed`; tidak ada hari yang sudah tertaut payroll; tidak ada pekerjaan terjadwal yang berjalan | Ditolak `409` dengan pesan bahwa hanya periode `Closed` yang dapat dibuka kembali `[EXISTING]` |
| `Reopened` | Tutup lagi | `Closing` lalu `Closed` | Pemegang `AttendancePeriod : Close` | Sama dengan baris pertama | Sama `[EXISTING]` |
| `Open` atau `Closing` | Batalkan | `Cancelled` | Pemegang `AttendancePeriod : Cancel` | Periode belum `Closed` | Ditolak `409` `[EXISTING]` |

**Perpindahan yang tidak sah dan harus ditolak:**

| Dari | Ke | Alasan ditolak |
| --- | --- | --- |
| `Closed` | `Open` | Satu-satunya jalan kembali adalah `Reopened`, yang meninggalkan jejak bahwa periode pernah ditutup `[EXISTING]` |
| `Closed` | `Cancelled` | Periode yang sudah ditutup dan datanya sudah dipakai payroll tidak dapat dibatalkan begitu saja `[EXISTING]` |
| `Cancelled` | mana pun | Keadaan akhir `[EXISTING]` |
| `Open` | `Closed` langsung tanpa `Closing` | Tahap `Closing` adalah tempat pemeriksaan penghalang berjalan `[EXISTING]` |

**Siapa yang seharusnya memegang `AttendancePeriod : Reopen`** masih `[OPEN]` — `HRD-Q-23`.
Mekanismenya sudah ada; pemetaan perannya belum.

### 1.2 Pemrosesan kehadiran — `AttendanceProcessingStatus`

Nilai: `Pending`, `Processing`, `Processed`, `ReprocessRequired`, `Skipped`, `Error`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Pending` | Mulai proses | `Processing` | Sistem | — | `[EXISTING]` |
| `Processing` | Selesai | `Processed` | Sistem | Seluruh langkah pengolahan berhasil | `[EXISTING]` |
| `Processing` | Gagal | `Error` | Sistem | — | `[EXISTING]` |
| `Error` | Tandai perlu ulang | `ReprocessRequired` | Sistem atau petugas | — | `[EXISTING]` |
| `ReprocessRequired` | Proses ulang | `Processing` | Pemegang `AttendanceProcessing : Update` | Periode masih dapat disunting | `[EXISTING]` |
| mana pun | Lewati | `Skipped` | Sistem | Hari itu memang tidak perlu diproses, misalnya pegawai belum aktif | `[EXISTING]` |

### 1.3 Kehadiran harian — `AttendanceStatus`

Nilai: `Unprocessed`, `Present`, `Absent`, `Late`, `EarlyLeave`, `Incomplete`, `Holiday`,
`RestDay`, `Leave`, `BusinessTrip`, `Remote`.

**Ini bukan state machine.** Nilainya adalah **hasil hitung** pemrosesan, bukan status yang
berpindah karena tindakan orang. Satu hari dapat berpindah dari `Late` menjadi `Present` bila
koreksi disetujui — tetapi itu terjadi karena **dihitung ulang**, bukan karena ada tindakan
"ubah status".

**Konsekuensi yang mengikat desain:** tidak boleh ada endpoint yang menyunting `AttendanceStatus`
langsung. Perubahannya selalu melalui koreksi lalu pemrosesan ulang `[EXISTING]`.

### 1.4 Pengecualian kehadiran — `AttendanceExceptionStatus`

Nilai: `Open`, `UnderReview`, `Corrected`, `Waived`, `Rejected`, `Closed`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Open` | Mulai tinjau | `UnderReview` | HR atau atasan | — | `[VOCAB]` — nilai ada, guard per-edge belum diverifikasi |
| `UnderReview` | Diperbaiki lewat koreksi | `Corrected` | Sistem, saat koreksi diterapkan | Permohonan koreksi berstatus `Applied` | `[EXISTING]` |
| `Open` atau `UnderReview` | Abaikan dengan alasan tercatat | `Waived` | HR atau atasan | Alasan wajib diisi | `[VOCAB]` |
| `Open` atau `UnderReview` | Tolak | `Rejected` | HR atau atasan | Alasan wajib diisi | `[VOCAB]` |
| `Corrected`, `Waived`, atau `Rejected` | Tutup | `Closed` | Sistem | — | `[VOCAB]` |

**Aturan yang benar-benar dijaga:** pengecualian dengan penanda pemblokir payroll yang masih
berstatus `Open` atau `UnderReview` **menghalangi penutupan periode** `[EXISTING]`. Inilah yang
membuat status pengecualian penting, bukan sekadar catatan.

### 1.5 Jenis pengecualian — `AttendanceExceptionType`

Nilai yang ada: `Late`, `EarlyLeave`, `MissingCheckIn`, `MissingCheckOut`, `Absent`,
`OutsideGeofence`, `DuplicatePunch`, `ScheduleMismatch`, `ScheduleConflict`,
`ExcessiveWorkHours`, `Unknown`.

**Nilai baru yang ditambahkan desain target:** `OutOfScheduleWork` `[DECISION]` `HRD-DEC-025`.

| Aspek | Isi |
| --- | --- |
| Apa artinya | Pegawai — biasanya dokter — benar-benar bekerja di luar jadwal kerjanya yang sah |
| Apa **bukan** artinya | Bukan `ScheduleMismatch`. `ScheduleMismatch` berarti *jadwal tidak dapat diselesaikan*, bukan *bekerja di luar jendela jadwal yang sudah ada* |
| Alur setelahnya | Menunggu klasifikasi atasan. Atasan menentukan salah satu: lembur, koreksi jadwal, tercatat tanpa kompensasi, atau klasifikasi resmi lain |
| Yang dilarang | **Tidak pernah** otomatis menjadi lembur `[DECISION]` `HRD-DEC-013` |
| Keadaan hari ini | `MISSING` — nilai ini belum ada di kode, dan tidak ada jalur yang mendeteksinya |

**Perpindahan klasifikasi target:**

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Open` | Klasifikasikan sebagai lembur | `Corrected` beserta rujukan ke permohonan lembur | Pemegang `AttendanceException : Classify` | Jenis pengecualiannya `OutOfScheduleWork` | Ditolak `422` `[DECISION]` |
| `Open` | Klasifikasikan sebagai koreksi jadwal | `Corrected` beserta rujukan ke permohonan ubah jadwal | Pemegang `AttendanceException : Classify` | Sama | Ditolak `422` `[DECISION]` |
| `Open` | Klasifikasikan sebagai tercatat tanpa kompensasi | `Waived` | Pemegang `AttendanceException : Classify` | Alasan wajib diisi | Ditolak `422` `[DECISION]` |

### 1.6 Permohonan koreksi kehadiran — `CorrectionRequestStatus`

Nilai: `Draft`, `Submitted`, `UnderReview`, `NeedRevision`, `Approved`, `PartiallyApproved`,
`Rejected`, `Applied`, `Cancelled`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Draft` | Ajukan | `Submitted` | Pegawai pemilik data | Alasan wajib diisi | Ditolak `400` `[EXISTING]` |
| `Submitted` | Mulai tinjau | `UnderReview` | Penyetuju yang ditugaskan | — | `[EXISTING]` |
| `UnderReview` | Minta perbaikan | `NeedRevision` | Penyetuju yang ditugaskan | Alasan wajib diisi | `[EXISTING]` |
| `NeedRevision` | Ajukan lagi | `Submitted` | Pegawai pemilik data | — | `[EXISTING]` |
| `UnderReview` | Setujui seluruhnya | `Approved` | Penyetuju yang ditugaskan | — | `[EXISTING]` |
| `UnderReview` | Setujui sebagian | `PartiallyApproved` | Penyetuju yang ditugaskan | Sekurang-kurangnya satu rincian disetujui | `[EXISTING]` |
| `UnderReview` | Tolak | `Rejected` | Penyetuju yang ditugaskan | Alasan wajib diisi | `[EXISTING]` |
| `Approved` atau `PartiallyApproved` | Terapkan | `Applied` | Pemegang `AttendanceCorrection : Apply` | Periode kehadiran masih dapat disunting | Ditolak `409` `[EXISTING]` |
| `Draft` atau `Submitted` | Batalkan | `Cancelled` | Pegawai pemilik data | — | `[EXISTING]` |

**Perpindahan yang tidak sah dan harus ditolak** `[DECISION]` `HRD-DEC-022`:

| Dari | Ke | Alasan ditolak |
| --- | --- | --- |
| `Applied` | `Approved` | `Applied` adalah **terminal terhadap sinkronisasi workflow normal**. Menurunkannya membuat penerapan berjalan dua kali dan memutasi ulang kehadiran harian |
| `Applied` | `PartiallyApproved` atau status sebelumnya mana pun | Sama |
| `Rejected` | mana pun | Keadaan akhir. Perbaikan dilakukan lewat permohonan baru |
| `Cancelled` | mana pun | Keadaan akhir |

> **`[DEFECT]` yang harus dicatat, bukan disembunyikan.** Pada baseline saat ini, endpoint
> `POST /correction-requests/{id}/workflow/synchronize` **dapat** menurunkan permohonan
> berstatus `Applied` kembali ke `Approved`, karena ia menulis status hasil pemetaan **tanpa
> memeriksa status sekarang**. Akibatnya penerapan berjalan ulang, kehadiran harian dimutasi
> ulang, pengecualian ditutup ulang, dan nomor versi pemrosesan naik. Ini bertentangan dengan
> `HRD-DEC-022` dan tercatat sebagai `IMPLEMENTATION DEFECT / REPAIR`.

**Jalur perbaikan yang sah setelah `Applied`** `[DECISION]` `HRD-DEC-022`:

1. Permohonan koreksi **baru** terhadap hari yang sama — sudah tersedia `[EXISTING]`;
2. Aksi perbaikan eksplisit yang terotorisasi dan punya jejak audit tersendiri — `MISSING`.

### 1.7 Rekaman mentah kehadiran — `RawLogProcessingStatus`

Nilai: `Pending`, `Matched`, `Processed`, `Duplicate`, `Rejected`, `Error`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Pending` | Cocokkan dengan pegawai dan jadwal | `Matched` | Sistem | Pegawai dan tanggalnya dikenali | `[EXISTING]` |
| `Matched` | Masukkan ke hasil olahan | `Processed` | Sistem | — | `[EXISTING]` |
| `Pending` | Deteksi rekaman kembar | `Duplicate` | Sistem | Ada rekaman lain dengan pegawai, waktu, dan jenis yang sama | `[EXISTING]` |
| `Pending` | Tolak | `Rejected` | Sistem | Pegawai tidak dikenali atau waktunya tidak masuk akal | `[EXISTING]` |
| `Pending` atau `Matched` | Gagal | `Error` | Sistem | — | `[EXISTING]` |
| `Error` atau `Rejected` | Coba lagi | `Pending` | Pemegang `AttendanceRawLog : Update` | — | `[EXISTING]` |

**Invariant yang paling penting di seluruh modul ini:** **isi rekaman mentah tidak pernah
berubah.** Yang berubah hanya status pemrosesannya. Koreksi kehadiran memutasi hasil olahan,
**bukan** rekaman mentah `[EXISTING]`.

---

## 2. Cuti

### 2.1 Permohonan cuti — `LeaveRequestValueConstants.Status`

Nilai: `Draft`, `Submitted`, `WaitingApproval`, `NeedRevision`, `Approved`, `Rejected`,
`Cancelled`, `Taken`, `Completed`, `Recalled`, `Expired`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Draft` | Ajukan | `Submitted` | Pegawai pemohon | Saldo mencukupi; jenis cuti berlaku; tanggal tidak bertabrakan | Ditolak `422` beserta alasannya `[EXISTING]` |
| `Submitted` | Masuk antrean persetujuan | `WaitingApproval` | Sistem | Jalur persetujuan berhasil dibentuk | Ditolak; pengajuan kembali `Draft` `[EXISTING]` |
| `WaitingApproval` | Setujui | `Approved` | Penyetuju yang ditugaskan | Pelakunya adalah penyetuju yang ditugaskan | Ditolak `403` `[EXISTING]` |
| `WaitingApproval` | Tolak | `Rejected` | Penyetuju yang ditugaskan | Alasan wajib diisi | Ditolak `400` `[EXISTING]` |
| `WaitingApproval` | Minta perbaikan | `NeedRevision` | Penyetuju yang ditugaskan | Alasan wajib diisi | Ditolak `400` `[EXISTING]` |
| `NeedRevision` | Ajukan lagi | `Submitted` | Pegawai pemohon | — | `[EXISTING]` |
| `WaitingApproval` | Lewat batas waktu | `Expired` | Sistem | Batas waktu terlampaui | `[EXISTING]` — nilainya ada; **berapa lama** batas waktunya `[OPEN]` `HRD-Q-26` |
| `Approved` | Cuti mulai berjalan | `Taken` | Sistem | Tanggal mulai tiba; eksekusi berhasil memotong saldo | Eksekusi ditandai gagal dan diulang `[EXISTING]` |
| `Taken` | Cuti selesai | `Completed` | Sistem | Tanggal selesai lewat | `[EXISTING]` |
| `Taken` | Dipanggil kembali | `Recalled` | HR Manager lewat alur pemanggilan kembali | Pemanggilan kembali sudah disetujui | `[EXISTING]` |
| `Draft`, `Submitted`, `WaitingApproval`, `Approved` | Batalkan | `Cancelled` | Pegawai pemohon, lewat alur pembatalan | — | `[EXISTING]` |

**Catatan penting tentang tingkatan persetujuan.** Status domain adalah **`WaitingApproval`
tunggal** sepanjang berapa pun tingkat persetujuan yang dikonfigurasi. Rantai bertingkat hidup di
lapisan langkah workflow, **bukan** sebagai status bernama. Komentar pada model yang menyebut
`WaitingSupervisorApproval`, `WaitingManagerApproval`, dan `WaitingHrVerification` adalah
**template yang disalin**, bukan implementasi — komentar identik ditemukan pada entity klaim
biaya yang sama sekali tidak berhubungan. `HRD-Q-44` tertutup.

**Perpindahan yang tidak sah dan harus ditolak:**

| Dari | Ke | Alasan ditolak |
| --- | --- | --- |
| `Rejected` | mana pun | Keadaan akhir. Pegawai mengajukan permohonan baru |
| `Expired` | mana pun | Keadaan akhir |
| `Draft` | `Approved` langsung | Melewati antrean persetujuan berarti tidak ada penyetuju yang pernah memutuskan |
| `Completed` | `Cancelled` atau `Taken` **lewat operasi normal** | `[DECISION]` `HRD-DEC-023` — `Completed` adalah business-final untuk operasi normal |

> **`[DEFECT]` yang harus dicatat.** Pada baseline saat ini, `POST /leave/executions/{id}/reverse`
> **tidak punya guard status apa pun** di controller. Satu-satunya yang memblokir adalah eksekusi
> yang sudah berstatus `Reversed`. Akibatnya cuti berstatus `Completed` **dapat** dibalik menjadi
> `Cancelled` (pembalikan penuh) atau kembali ke `Taken` (pembalikan sebagian). Ini jalur kode
> nyata yang dapat dijangkau, bukan dugaan.

**Pembalikan terkendali yang sah** `[DECISION]` `HRD-DEC-023` — enam syarat yang **wajib**
dipenuhi:

| No | Syarat | Keadaan hari ini |
| ---: | --- | --- |
| 1 | Permission khusus untuk membalikkan | `MISSING` |
| 2 | Alasan wajib diisi | `MISSING` |
| 3 | Pelaku dan waktu tercatat | `MISSING` |
| 4 | Rekonsiliasi kehadiran dijalankan | Sebagian `[EXISTING]` |
| 5 | Saldo dibalikkan atau dihitung ulang | `[EXISTING]` |
| 6 | Periode payroll diperiksa; bila terkunci, histori `Completed` **MUST NOT** dimutasi langsung — pakai transaksi penyesuaian terpisah | `MISSING` |

### 2.2 Eksekusi cuti — `LeaveExecutionValueConstants.ExecutionStatus`

Nilai: `Scheduled`, `Active`, `Completed`, `Failed`, `Cancelled`, `Reversed`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| — | Buat jadwal eksekusi | `Scheduled` | Sistem | Permohonan cuti `Approved` | `[EXISTING]` |
| `Scheduled` | Jalankan pada tanggal mulai | `Active` | Sistem | Saldo berhasil dipotong; hari-hari kehadiran ditandai cuti | Menjadi `Failed` `[EXISTING]` |
| `Active` | Selesaikan pada tanggal akhir | `Completed` | Sistem | — | `[EXISTING]` |
| `Scheduled` atau `Active` | Gagal | `Failed` | Sistem | — | `[EXISTING]` |
| `Failed` | Ulangi | `Scheduled` | Pemegang `LeaveExecution : Retry` | — | `[EXISTING]` |
| `Active` atau `Completed` | Balikkan terkendali | `Reversed` | Pemegang `LeaveExecution : Reverse` | Enam syarat `HRD-DEC-023` terpenuhi | Ditolak `422` `[DECISION]` — keadaan hari ini tidak menegakkannya |
| `Scheduled` atau `Active` | Batalkan | `Cancelled` | Sistem, lewat alur pembatalan cuti | Pembatalan sudah disetujui | `[EXISTING]` |

**Perpindahan yang tidak sah:** `Reversed` adalah keadaan akhir. Eksekusi yang sudah dibalik
tidak dapat dijalankan ulang; yang sah adalah permohonan cuti baru `[EXISTING]`.

### 2.3 Buku besar saldo cuti — `TransactionStatus`

Nilai: `Draft`, `Posted`, `Reversed`, `Cancelled`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Draft` | Masukkan ke saldo | `Posted` | Sistem | Berada di dalam transaksi database yang sama dengan perubahan saldo | `[EXISTING]` |
| `Posted` | Balikkan | `Reversed` | Sistem | Baris pembalik dibuat; baris asli **tidak** disunting | `[EXISTING]` |
| `Draft` | Batalkan | `Cancelled` | Sistem | — | `[EXISTING]` |

**Invariant yang mengikat seluruh domain cuti:** baris buku besar yang sudah `Posted`
**MUST NOT** diubah. Koreksi dilakukan dengan **menulis baris pembalik**, bukan menyunting baris
lama. Ini yang membuat saldo dapat diaudit — setiap selisih punya penjelasan berupa baris.

**Contoh berangka supaya jelas.** Seorang pegawai punya saldo 12 hari. Ia mengambil cuti 3 hari,
sehingga tertulis satu baris `Deduction` sebesar 3 hari dan saldo menjadi 9. Ternyata cuti itu
dibatalkan. Yang **benar** adalah menulis baris baru bertipe `CancellationRestore` sebesar 3
hari, sehingga saldo kembali 12 dan buku besarnya memuat tiga baris: pemberian hak 12, potongan
3, pengembalian 3. Yang **salah** adalah menghapus baris potongan itu — saldo memang kembali 12,
tetapi tidak ada lagi jejak bahwa cuti itu pernah diambil dan dibatalkan.

### 2.4 Pembatalan cuti — `CancellationStatus`

Nilai: `Draft`, `Submitted`, `WaitingApproval`, `NeedRevision`, `Approved`, `Rejected`,
`Cancelled`, `Applied`, `Failed`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Draft` | Ajukan | `Submitted` | Pegawai pemilik cuti | Cuti belum `Completed` | `[EXISTING]` |
| `Submitted` | Masuk antrean persetujuan | `WaitingApproval` | Sistem | — | `[EXISTING]` |
| `WaitingApproval` | Setujui | `Approved` | Penyetuju yang ditugaskan | — | `[EXISTING]` |
| `WaitingApproval` | Tolak | `Rejected` | Penyetuju yang ditugaskan | Alasan wajib diisi | `[EXISTING]` |
| `WaitingApproval` | Minta perbaikan | `NeedRevision` | Penyetuju yang ditugaskan | Alasan wajib diisi | `[EXISTING]` |
| `Approved` | Terapkan | `Applied` | Pemegang `LeaveCancellation : Apply` | Saldo berhasil dikembalikan | Menjadi `Failed` `[EXISTING]` |
| `Approved` | Gagal diterapkan | `Failed` | Sistem | — | `[EXISTING]` |

**Aturan pengembalian saldo yang perlu diketahui pengguna.** Bila pembatalan terjadi **setelah**
tanggal mulai cuti, saldo **tidak** dikembalikan penuh — yang dikembalikan hanya sisa hari yang
belum terlewat, dihitung per hari kalender `[EXISTING]`.

**Contoh berangka.** Seorang pegawai mengambil cuti 5 hari mulai 1 September. Pada 3 September ia
membatalkan sisanya. Dua hari sudah terlewat, sehingga yang dikembalikan ke saldo adalah 3 hari,
bukan 5.

### 2.5 Pemanggilan kembali dari cuti — `RecallStatus`

Nilai: `Draft`, `Submitted`, `WaitingApproval`, `NeedRevision`, `Acknowledged`, `Approved`,
`Rejected`, `Applied`, `Cancelled`, `Failed`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Draft` | Ajukan | `Submitted` | HR atau atasan | Alasan wajib diisi | `[EXISTING]` |
| `Submitted` | Masuk antrean persetujuan | `WaitingApproval` | Sistem | — | `[EXISTING]` |
| `WaitingApproval` | Setujui | `Approved` | Penyetuju yang ditugaskan | **Tidak** mensyaratkan `Acknowledged` lebih dulu | `[DECISION]` `HRD-DEC-024` — sudah sejalan dengan kode |
| `WaitingApproval` | Tolak | `Rejected` | Penyetuju yang ditugaskan | Alasan wajib diisi | `[EXISTING]` |
| `Approved` | Pegawai mengonfirmasi pemberitahuan | `Acknowledged` | Pegawai yang dipanggil | Notifikasi sudah terkirim | `[DECISION]` — mekanisme notifikasinya `[OPEN]`/belum diverifikasi |
| `Approved` | HR Manager menandai pemberitahuan tersampaikan | `Acknowledged` | Pemegang `LeaveRecall : OverrideAcknowledgement` | Alasan, pelaku, waktu, dan jejak audit **wajib** | `[DECISION]` `HRD-DEC-024` — `MISSING` di kode |
| `Acknowledged` | Terapkan | `Applied` | Pemegang `LeaveRecall : Apply` | Cuti dipotong sesuai hari yang batal diambil | Menjadi `Failed` `[EXISTING]` |

**Aturan yang mengikat** `[DECISION]` `HRD-DEC-024`: `Acknowledged` adalah **bukti pegawai
menerima pemberitahuan**, bukan syarat sebelum organisasi memutuskan. **Pegawai MUST NOT dapat
memblokir keputusan pemanggilan kembali selamanya hanya dengan tidak mengonfirmasi.**

**Perpindahan yang tidak sah:** `WaitingApproval` → `Acknowledged` langsung. Konfirmasi hanya
bermakna setelah ada keputusan yang dikonfirmasi.

### 2.6 Penyesuaian saldo cuti — `AdjustmentStatus`

Nilai: `Draft`, `Submitted`, `UnderReview`, `NeedRevision`, `Approved`, `Rejected`, `Posted`,
`Reversed`, `Cancelled`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Draft` | Ajukan | `Submitted` | HR | Alasan dan jumlah wajib diisi | `[EXISTING]` |
| `Submitted` | Tinjau | `UnderReview` | Penyetuju yang ditugaskan | — | `[EXISTING]` |
| `UnderReview` | Setujui | `Approved` | Penyetuju yang ditugaskan | — | `[EXISTING]` |
| `UnderReview` | Tolak | `Rejected` | Penyetuju yang ditugaskan | Alasan wajib diisi | `[EXISTING]` |
| `UnderReview` | Minta perbaikan | `NeedRevision` | Penyetuju yang ditugaskan | Alasan wajib diisi | `[EXISTING]` |
| `Approved` | Masukkan ke buku besar | `Posted` | Pemegang `LeaveAdjustment : Post` | Saldo tujuan tidak terkunci | Ditolak `409` `[EXISTING]` |
| `Posted` | Balikkan | `Reversed` | Pemegang `LeaveAdjustment : Reverse` | Baris pembalik dibuat | `[EXISTING]` |
| `Draft` atau `Submitted` | Batalkan | `Cancelled` | HR pembuat | — | `[EXISTING]` |

**Apakah penyesuaian saldo wajib melewati persetujuan** masih `[OPEN]` — `HRD-Q-27`. Jalur
`submit` dan `prepare-workflow` sudah ada, tetapi kewajiban memakainya belum diputuskan pemilik
produk. Ini penting karena penyesuaian saldo **mengubah hak pegawai**.

### 2.7 Proses berkala cuti — `BatchRunStatus`

Berlaku untuk akrual dan sisa cuti yang dibawa ke periode berikutnya.

Nilai: `Draft`, `Queued`, `Running`, `Completed`, `CompletedWithErrors`, `Failed`, `Cancelled`,
`Reversed`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Draft` | Antrikan | `Queued` | Pemegang `<Run> : Create` | Pratinjau sudah dijalankan | `[EXISTING]` |
| `Queued` | Jalankan | `Running` | Sistem | — | `[EXISTING]` |
| `Running` | Selesai tanpa kesalahan | `Completed` | Sistem | — | `[EXISTING]` |
| `Running` | Selesai dengan sebagian kesalahan | `CompletedWithErrors` | Sistem | — | `[EXISTING]` |
| `Running` | Gagal seluruhnya | `Failed` | Sistem | — | `[EXISTING]` |
| `Failed` atau `CompletedWithErrors` | Ulangi | `Queued` | Pemegang `<Run> : Retry` | — | `[EXISTING]` |
| `Completed` atau `CompletedWithErrors` | Balikkan | `Reversed` | Pemegang `<Run> : Reverse` | Baris pembalik dibuat untuk setiap baris buku besar | `[EXISTING]` |
| `Draft` atau `Queued` | Batalkan | `Cancelled` | Pemegang `<Run> : Cancel` | Proses belum berjalan | `[EXISTING]` |

---

## 3. Lembur

### 3.1 Rencana lembur — `PlanStatus`

Nilai: `Draft`, `Validated`, `Published`, `PartiallyConverted`, `Converted`, `Cancelled`,
`Closed`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Draft` | Validasi | `Validated` | Pemegang `OvertimePlan : Validate` | Seluruh baris rencana lolos pemeriksaan | Ditolak beserta daftar masalahnya `[EXISTING]` |
| `Validated` | Terbitkan | `Published` | Pemegang `OvertimePlan : Publish` | — | `[EXISTING]` |
| `Published` | Turunkan sebagian menjadi permohonan | `PartiallyConverted` | Pemegang `OvertimePlan : GenerateRequest` | Sekurang-kurangnya satu baris berhasil diturunkan | `[EXISTING]` |
| `Published` atau `PartiallyConverted` | Turunkan seluruhnya | `Converted` | Pemegang `OvertimePlan : GenerateRequest` | Seluruh baris berhasil diturunkan | `[EXISTING]` |
| `Converted` | Tutup | `Closed` | Sistem atau petugas | — | `[EXISTING]` |
| Mana pun sebelum `Converted` | Batalkan | `Cancelled` | Pemegang `OvertimePlan : Cancel` | — | `[EXISTING]` |

### 3.2 Permohonan lembur — `RequestStatus`

Nilai: `Draft`, `Submitted`, `NeedRevision`, `ApprovedForWork`, `Rejected`, `InProgress`,
`WaitingRealization`, `WaitingVerification`, `Realized`, `PostedToPayroll`, `Cancelled`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Draft` | Ajukan | `Submitted` | Pegawai atau sistem dari rencana | Rentang waktu **tidak** bertumpuk dengan permohonan lain | Ditolak `409` dengan penanda `REQUEST_OVERLAP` `[EXISTING]` |
| `Submitted` | Setujui untuk dikerjakan | `ApprovedForWork` | Penyetuju yang ditugaskan | — | `[EXISTING]` |
| `Submitted` | Tolak | `Rejected` | Penyetuju yang ditugaskan | Alasan wajib diisi | `[EXISTING]` |
| `Submitted` | Minta perbaikan | `NeedRevision` | Penyetuju yang ditugaskan | Alasan wajib diisi | `[EXISTING]` |
| `NeedRevision` | Ajukan lagi | `Submitted` | Pegawai pemohon | — | `[EXISTING]` |
| `ApprovedForWork` | Lembur berjalan | `InProgress` | Sistem | Waktu mulai tiba | `[EXISTING]` |
| `InProgress` | Selesai dikerjakan | `WaitingRealization` | Sistem | Waktu selesai lewat | `[EXISTING]` |
| `WaitingRealization` | Realisasi dihitung | `WaitingVerification` | Pemegang `OvertimeRealization : Calculate` | Kehadiran hari itu tersedia dan cocok | Ditolak beserta penanda alasannya `[EXISTING]` |
| `WaitingVerification` | Verifikasi disetujui | `Realized` | Pemegang `OvertimeVerification : Approve` | — | `[EXISTING]` |
| `Realized` | Serahkan ke payroll | `PostedToPayroll` | Pemegang `OvertimePayrollHandoff : Post` | Realisasi berstatus `Verified` **dan** verifikasi aktif terbaru `Approved` | Ditolak `409` `[EXISTING]` |
| Sebelum `Realized` | Batalkan | `Cancelled` | Pegawai atau atasan | — | `[EXISTING]` |

**Perpindahan yang tidak sah dan harus ditolak:**

| Dari | Ke | Alasan ditolak |
| --- | --- | --- |
| `WaitingRealization` | `PostedToPayroll` | Melewati verifikasi. Lembur yang belum diverifikasi **MUST NOT** ikut serah terima payroll `[EXISTING]` |
| `Rejected` | mana pun | Keadaan akhir |
| `PostedToPayroll` | `Realized` **lewat operasi biasa** | Hanya sah lewat `rollback` yang terotorisasi `[EXISTING]` |

**Jalur koreksi setelah diserahkan ke payroll.** Berbeda dari dugaan awal, koreksi **tidak**
selalu memerlukan pembukaan periode penuh. Tersedia `POST .../realizations/{id}/rollback` yang
mengembalikan realisasi ke `Verified` dan permohonan ke `Realized`, dijaga pemeriksaan kunci
payroll dan keterbukaan periode `[EXISTING]`.

### 3.3 Realisasi lembur — `RealizationStatus`

Nilai: `Draft`, `WaitingVerification`, `NeedRevision`, `Verified`, `Rejected`, `PostedToPayroll`,
`Cancelled`.

Urutan pokoknya `Draft` → `WaitingVerification` → `Verified` → `PostedToPayroll`. Dapat menjadi
`NeedRevision`, `Rejected`, atau `Cancelled` `[EXISTING]`.

### 3.4 Verifikasi lembur — `VerificationStatus`

Nilai: `NotStarted`, `Pending`, `Approved`, `Rejected`, `NeedRevision`, `Skipped`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `NotStarted` | Mulai | `Pending` | Pemegang `OvertimeVerification : Start` | — | `[EXISTING]` |
| `Pending` | Setujui | `Approved` | Pemegang `OvertimeVerification : Approve` | — | `[EXISTING]` |
| `Pending` | Tolak | `Rejected` | Pemegang `OvertimeVerification : Reject` | Alasan wajib diisi | `[EXISTING]` |
| `Pending` | Minta perbaikan | `NeedRevision` | Pemegang `OvertimeVerification : NeedRevision` | Alasan wajib diisi | `[EXISTING]` |
| `NotStarted` atau `Pending` | Lewati | `Skipped` | — | **`[OPEN]`** — dalam keadaan apa verifikasi boleh dilewati belum diputuskan | `HRD-Q-30` |

### 3.5 Periode lembur — `PeriodStatus`

Nilai: `Open`, `Closing`, `Closed`, `Reopened`, `Cancelled`. Strukturnya **sama persis** dengan
periode kehadiran.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Closed` atau `Closing` | Buka kembali | `Reopened` | Pemegang `OvertimePeriod : Reopen` | Statusnya **harus** `Closed` atau `Closing` | Ditolak `409` `[EXISTING]` |

**Perpindahan yang tidak sah:** `Open`, `Reopened`, dan `Cancelled` **tidak** dapat dibuka
kembali `[EXISTING]`.

**Siapa yang seharusnya memegang `OvertimePeriod : Reopen`** masih `[OPEN]` — `HRD-Q-32`.

### 3.6 Cuti pengganti dari lembur — `CompensatoryStatus`

Nilai: `Pending`, `Available`, `PartiallyUsed`, `Used`, `Expired`, `Cancelled`.

Urutan pokoknya `Pending` → `Available` → `PartiallyUsed` → `Used`. Dapat menjadi `Expired` atau
`Cancelled` `[EXISTING]`.

**Berapa lama cuti pengganti berlaku sebelum kedaluwarsa, dan apakah dapat diperpanjang,** masih
`[OPEN]` — `HRD-Q-31`.

---

## 4. Penjadwalan Kerja

### 4.1 Penempatan jadwal kerja — `WfpWorkScheduleAssignment`

**Tidak ada state machine.** Satu-satunya yang berubah adalah penanda aktif atau tidak aktif
lewat `PATCH {id}/status` `[EXISTING]`.

**Guard target yang belum ada** `[DECISION]` `HRD-DEC-027`:

| Keadaan | Perlakuan target | Keadaan hari ini |
| --- | --- | --- |
| Penempatan untuk tanggal sekarang atau yang akan datang, pada periode yang masih dapat disunting | **Tidak** memerlukan persetujuan tambahan; audit trail tetap wajib | Sudah sejalan `[EXISTING]` |
| Perubahan yang berlaku surut | **Wajib** lewat koreksi terkendali, tidak boleh disunting langsung | **`MISSING`** — tidak ada guard yang mendeteksinya |
| Perubahan yang menyentuh periode kehadiran atau payroll yang sudah diproses atau terkunci | **Wajib** lewat koreksi terkendali | **`MISSING`** |

**Larangan yang mengikat:** jangan membuat persetujuan untuk setiap suntingan kecil. Itu akan
membebani pekerjaan HR sehari-hari tanpa menambah pengendalian yang berarti.

### 4.2 Permohonan ubah jadwal — `ScheduleChangeStatus`

Nilai: `Draft`, `Submitted`, `UnderReview`, `NeedRevision`, `Approved`, `Rejected`, `Cancelled`,
`Applied`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Draft` | Ajukan | `Submitted` | Pegawai pemohon | Pratinjau lolos pemeriksaan | Ditolak `[EXISTING]` |
| `Submitted` atau `UnderReview` | Sinkronkan dari mesin persetujuan | `Approved`, `Rejected`, atau `NeedRevision` | Sistem | Mengikuti keputusan penyetuju | `[EXISTING]` |
| `Approved` | Terapkan | `Applied` | Sistem, otomatis saat persetujuan selesai | Jadwal tujuan tersedia | `[EXISTING]` |
| `Draft` atau `Submitted` | Batalkan | `Cancelled` | Pegawai pemohon | — | `[EXISTING]` |

### 4.3 Permohonan tukar shift — `ShiftSwapStatus`

Nilai: `Draft`, `PendingTarget`, `TargetAccepted`, `TargetRejected`, `PendingApproval`,
`NeedRevision`, `Approved`, `Rejected`, `Cancelled`, `Applied`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Draft` | Kirim ke rekan yang dituju | `PendingTarget` | Pegawai pemohon | Pratinjau lolos pemeriksaan aturan istirahat | Ditolak `[EXISTING]` |
| `PendingTarget` | Rekan menerima | `TargetAccepted` | **Pegawai yang dituju**, bukan pemohon | Statusnya **harus** `PendingTarget` | Ditolak `409` `[EXISTING]` |
| `PendingTarget` | Rekan menolak | `TargetRejected` | Pegawai yang dituju | Sama | `[EXISTING]` |
| `TargetAccepted` | Teruskan ke persetujuan atasan | `PendingApproval` | Sistem, atas pemicu pemohon | Penanda diterima rekan **harus** bernilai benar | Ditolak `409` — **tidak dapat dilewati** `[EXISTING]` |
| `PendingApproval` | Sinkronkan dari mesin persetujuan | `Approved`, `Rejected`, atau `NeedRevision` | Sistem | — | `[EXISTING]` |
| `Approved` | Terapkan | `Applied` | Sistem | Kedua baris penugasan shift benar-benar tertukar di dalam transaksi yang sama | Seluruhnya dibatalkan `[EXISTING]` |

**Perpindahan yang tidak sah dan harus ditolak:**

| Dari | Ke | Alasan ditolak |
| --- | --- | --- |
| `PendingTarget` | `PendingApproval` | Melewati persetujuan rekan. Guard eksplisit menolaknya `[EXISTING]` |
| `TargetRejected` | mana pun | **Keadaan akhir yang tidak pernah mencapai persetujuan atasan.** Ini bukan asumsi — dibuktikan guard `[EXISTING]` |
| `Draft` | `Approved` | Melewati kedua tahap |

**Apa yang benar-benar terjadi saat `Applied`.** Sistem memuat kedua baris penugasan shift —
milik pemohon dan milik rekan — lalu saling menukar tanggal, shift, jadwal kerja, jam terjadwal,
dan menit kerja terencana; menandai keduanya bersumber tukar shift dan merupakan penimpaan
manual; baru kemudian mengubah status permohonan menjadi `Applied`. Seluruhnya di dalam **satu
transaksi database** `[EXISTING]`.

Pemroses kehadiran **benar-benar membaca hasil pertukaran itu**, sehingga hari yang ditukar
dihitung sesuai jadwal barunya `[EXISTING]` — dibuktikan pada `PHASE 2B.1`.

### 4.4 Roster, shift harian, dan siaga

Ketiganya punya kosakata status yang lengkap di model, tetapi **tidak satu pun perpindahannya
dijaga kode** karena tidak ada controller maupun service yang mengoperasikannya.

| Entity | Kosakata status | Perpindahan |
| --- | --- | --- |
| Periode roster | `Draft`, `Validation`, `Submitted`, `Approved`, `Published`, `Locked`, `Closed`, `Cancelled` | `[VOCAB]` — **tidak ada** |
| Penugasan roster per pegawai | `Draft`, `Validated`, `Approved`, `Published`, `Cancelled` | `[VOCAB]` — **tidak ada** |
| Penugasan shift harian | `Draft`, `Validated`, `Published`, `Confirmed`, `Completed`, `Cancelled`, `Replaced` | `[VOCAB]` — **tidak ada** |
| Penugasan siaga | `Scheduled`, `Confirmed`, `Activated`, `Completed`, `Cancelled` | `[VOCAB]` — **tidak ada** |

**Perpindahan target** `[DECISION]` `HRD-DEC-026` — inilah yang dibangun oleh `EXTEND`:

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| `Draft` | Periksa kecukupan tenaga dan bentrok | `Validation` | Pemegang `RosterPeriod : Validate` | — |
| `Validation` | Ajukan | `Submitted` | Pemegang `RosterPeriod : Submit` | Tidak ada bentrok yang belum diselesaikan |
| `Submitted` | Setujui | `Approved` | Penyetuju yang ditugaskan | — |
| `Approved` | Terbitkan | `Published` | Pemegang `RosterPeriod : Publish` | Penugasan shift harian terbentuk untuk seluruh pegawai |
| `Published` | Kunci | `Locked` | Pemegang `RosterPeriod : Lock` | Periode kehadiran terkait sudah dibuka |
| `Locked` | Tutup | `Closed` | Pemegang `RosterPeriod : Close` | Periode kehadiran terkait sudah ditutup |
| Mana pun sebelum `Published` | Batalkan | `Cancelled` | Pemegang `RosterPeriod : Cancel` | — |

**Perpindahan yang tidak sah pada rancangan target:**

| Dari | Ke | Alasan ditolak |
| --- | --- | --- |
| `Draft` | `Published` | Melewati pemeriksaan kecukupan tenaga. Roster yang terbit dengan hari kosong berarti ada shift tanpa petugas |
| `Published` | `Draft` | Roster yang sudah terbit sudah menjadi jadwal yang berlaku dan sudah dibaca kehadiran. Perubahan sesudahnya lewat penggantian shift atau ubah jadwal, bukan menarik kembali roster |
| `Closed` | mana pun | Keadaan akhir |

---

## 5. Persetujuan Bersama

### 5.1 Instance persetujuan — `WorkflowStatus`

Nilai: `Draft`, `Submitted`, `InProgress`, `RevisionRequested`, `Returned`, `Approved`,
`Rejected`, `Cancelled`, `Withdrawn`, `Completed`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Draft` | Ajukan | `Submitted` | Pemohon transaksi | Jalur persetujuan berhasil dibentuk dari definisi workflow | Ditolak `[EXISTING]` |
| `Submitted` | Langkah pertama menjadi aktif | `InProgress` | Sistem | Sekurang-kurangnya satu penyetuju berhasil ditentukan | Ditolak; instance kembali `Draft` `[EXISTING]` |
| `InProgress` | Seluruh langkah disetujui | `Approved` lalu `Completed` | Sistem | — | `[EXISTING]` |
| `InProgress` | Satu langkah menolak | `Rejected` | Sistem | Alasan wajib diisi | `[EXISTING]` |
| `InProgress` | Satu langkah meminta perbaikan | `RevisionRequested` | Sistem | Alasan wajib diisi | `[EXISTING]` |
| `InProgress` | Satu langkah mengembalikan | `Returned` | Sistem | Alasan wajib diisi | `[EXISTING]` |
| `Draft` atau `Submitted` | Tarik kembali | `Withdrawn` | Pemohon transaksi | Belum ada keputusan | `[EXISTING]` |
| Mana pun sebelum `Completed` | Batalkan | `Cancelled` | Pemegang `WorkflowInstance : Cancel` | — | `[EXISTING]` |

### 5.2 Tugas persetujuan per orang — `AssignmentStatus`

Nilai: `Pending`, `Available`, `InProgress`, `Approved`, `Rejected`, `RevisionRequested`,
`Returned`, `Delegated`, `Skipped`, `Cancelled`, `Completed`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Pending` | Langkahnya menjadi giliran | `Available` | Sistem | Langkah sebelumnya selesai | `[EXISTING]` |
| `Available` | Setujui | `Approved` | **Hanya** penyetuju yang ditugaskan | Pelakunya sama dengan penyetuju yang ditugaskan | Ditolak `403` `[EXISTING]` |
| `Available` | Tolak | `Rejected` | Hanya penyetuju yang ditugaskan | Alasan wajib diisi | `[EXISTING]` |
| `Available` | Minta perbaikan | `RevisionRequested` | Hanya penyetuju yang ditugaskan | Alasan wajib diisi | `[EXISTING]` |
| `Available` | Kembalikan ke langkah sebelumnya | `Returned` | Hanya penyetuju yang ditugaskan | Alasan wajib diisi | `[EXISTING]` |
| `Available` | Dialihkan ke penerima delegasi | `Delegated` | Sistem | Ada delegasi aktif dari penyetuju itu | `[EXISTING]` |
| `Available` | Lewati | `Skipped` | Sistem | Aturan langkah mengizinkan | `[EXISTING]` |
| `Available` | Setujui otomatis karena batas waktu | `Approved` | Sistem | **Hanya** bila definisi workflow transaksi itu secara eksplisit mengizinkan | `[DECISION]` `HRD-DEC-030` — **default mati**, dan `MISSING` di kode |

**Gate kewenangan yang benar-benar berlaku:** setiap aksi memeriksa bahwa pelakunya adalah
penyetuju yang ditugaskan pada baris itu `[EXISTING]`. Memiliki butir hak akses **tidak** cukup.

**Perpindahan yang tidak sah:**

| Dari | Ke | Alasan ditolak |
| --- | --- | --- |
| `Pending` | `Approved` | Belum gilirannya. Persetujuan berurutan akan kehilangan artinya |
| `Approved` | `Available` | Keputusan yang sudah diambil tidak dicabut dengan mengembalikan tugasnya; yang sah adalah pembatalan instance atau permohonan baru |

### 5.3 Delegasi persetujuan — `DelegationStatus`

Nilai: `Draft`, `Submitted`, `Approved`, `Active`, `Expired`, `Revoked`, `Cancelled`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Draft` | Ajukan | `Submitted` | Pemberi delegasi | Rentang tanggal dan penerima wajib diisi | `[EXISTING]` |
| `Submitted` | Setujui | `Approved` lalu `Active` | **Bukan** pemberi delegasi, **bukan** pula penerimanya | Guard eksplisit melarang keduanya menyetujui delegasinya sendiri | Ditolak `[EXISTING]` |
| `Active` | Alihkan tugas terbuka | tugas berpindah penyetuju | Sistem | — | `[EXISTING]` |
| `Active` | Cabut | `Revoked` | Pemberi delegasi | Tugas terbuka dikembalikan ke penyetuju semula | `[EXISTING]` |
| `Active` | Lewat masa berlaku | `Expired` | Sistem | Tanggal akhir terlewat | `[VOCAB]` — nilainya ada; **siapa atau apa yang menjalankan pemeriksaan kedaluwarsa** belum diverifikasi |
| `Draft` atau `Submitted` | Batalkan | `Cancelled` | Pemberi delegasi | — | `[EXISTING]` |

**Pola yang layak ditiru.** Larangan menyetujui delegasi sendiri adalah **pemisahan peran yang
benar**, dan sudah ada di kode. Bandingkan dengan tindakan disiplin pada bagian 7.1, yang justru
memperbolehkan swa-setuju.

---

## 5a. Sesi Gaji Sensitif — `SALARY_SENSITIVE_SESSION` `HRD-DEC-038`

Nilai: `None`, `Active`, `Expired`, `Revoked`.

Ini **bukan** status yang tersimpan pada tabel domain. Ia keadaan otorisasi berumur pendek yang
menjaga pembacaan data gaji. Dicatat di sini karena flowchart dan kontrak lain merujuknya.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `None` | Konfirmasi kata sandi | `Active` | Pengguna yang sedang masuk | Kata sandi terverifikasi lewat mekanisme Identity **canonical** | Ditolak `401`; sesi tetap `None` `[DECISION]` |
| `Active` | Batas waktu terlampaui | `Expired` | Sistem | Bawaan **5 menit** sejak diterbitkan | Data gaji tidak lagi dapat dibaca `[DECISION]` |
| `Active` | Pengguna keluar | `Revoked` | Sistem | — | `[DECISION]` |
| `Active` | Sesi otentikasi utama tidak sah | `Revoked` | Sistem | — | `[DECISION]` |
| `Active` | Akun dinonaktifkan | `Revoked` | Sistem | — | `[DECISION]` |
| `Active` | Keadaan kata sandi atau keamanan berubah | `Revoked` | Sistem | — | `[DECISION]` |
| `Expired` atau `Revoked` | Konfirmasi kata sandi lagi | `Active` | Pengguna yang sedang masuk | Sama seperti baris pertama | `[DECISION]` |

**Perpindahan yang tidak sah dan harus ditolak:**

| Dari | Ke | Alasan ditolak |
| --- | --- | --- |
| `None` | `Active` tanpa verifikasi kata sandi | Meniadakan seluruh gunanya otentikasi bertingkat |
| `Expired` | `Active` dengan memperpanjang otomatis | Perpanjangan otomatis membuat batas lima menit tidak berarti |
| `Revoked` | `Active` tanpa otentikasi utama yang sah kembali | Sesi sensitif **MUST NOT** hidup lebih lama daripada sesi utamanya |

**Keadaan hari ini: `MISSING`.** Kosakata ini belum ada di kode, dan tidak ada satu pun jalur yang
menegakkannya. Ini `IMPLEMENTATION_WORK` turunan `HRD-DEC-038`.

---

## 6. Administrasi Kepegawaian

### 6.0 Dokumen Peristiwa Kepegawaian (SK Mutasi/Promosi/Rotasi/Demosi) — `HrdPersonnelActionOrder` (`HRD-DEC-048`, `HRD-DEC-056`, `HRD-DEC-057`)

Nilai status: `Draft`, `PendingApproval` (Menunggu persetujuan), `Scheduled` (Dijadwalkan), `Applied` (Berlaku), `Rejected` (Ditolak), `Cancelled` (Dibatalkan).

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Draft` | Ajukan untuk persetujuan | `PendingApproval` | HR Admin pembuat | Nomor SK dan berkas scan SK resmi bertandatangan wajib terunggah (`HRD-DEC-056`); tanggal efektif memenuhi aturan periode terbuka (`HRD-DEC-050`) | Ditolak 400 jika scan SK belum ada; ditolak 409 jika tanggal efektif pada periode tertutup `[DECISION]` |
| `PendingApproval` | Setujui (tanggal di masa depan) | `Scheduled` | HR Manager / `CorporateHr` | **Penyetuju MUST berbeda dari pembuat SK** (`HRD-DEC-031`, `HRD-DEC-056`); periode kehadiran masih terbuka saat persetujuan | Ditolak 403 bila pembuat menyetujui sendiri; ditolak 409 bila periode kehadiran sudah tertutup `[DECISION]` |
| `PendingApproval` | Setujui (berlaku surut) | `Applied` | HR Manager / `CorporateHr` | Tanggal efektif sudah lewat tetapi masih berada dalam periode kehadiran yang terbuka (`HRD-DEC-050`); alasan surut terisi | Ditolak 409 bila periode tertutup `[DECISION]` |
| `PendingApproval` | Tolak | `Rejected` | HR Manager / `CorporateHr` | Penyetuju berbeda dari pembuat; alasan penolakan wajib diisi | Ditolak 400 bila alasan kosong; ditolak 403 bila swa-tolak |
| `Scheduled` | Tanggal efektif tiba | `Applied` | Sistem (job terjadwal) | Tanggal server mencapai `EffectiveDate`. Sistem menutup penempatan lama per H-1 dan membuka penempatan baru (`HRD-DEC-048`, `HRD-DEC-049`) | Tercatat di log eksekusi sistem `[DECISION]` |
| `Scheduled` | Ajukan pembatalan | `Cancelled` | Diajukan HR Admin; disetujui HR Manager | Hanya boleh dibatalkan sebelum tanggal berlaku tiba (`HRD-DEC-057`); alasan pembatalan wajib diisi; penyetuju batal != pengaju batal | Ditolak bila tanggal berlaku sudah tiba `[DECISION]` |
| `Applied` | Batalkan | — **DITOLAK KERAS** | — | Dokumen yang sudah berlaku **MUST NOT** dibatalkan (`HRD-DEC-057`). Pemulihan keadaan wajib menerbitkan Dokumen SK baru | Ditolak 409 `[DECISION]` |

**Perpindahan yang tidak sah dan harus ditolak (`HRD-DEC-048`, `HRD-DEC-056`, `HRD-DEC-057`):**

| Dari | Ke | Alasan ditolak |
| --- | --- | --- |
| `Draft` | `Scheduled` atau `Applied` langsung | Melewati persetujuan pejabat berwenang meniadakan pemisahan peran |
| `PendingApproval` | `Scheduled` oleh pembuat sendiri | Pelanggaran fatal pemisahan peran (`APPROVER_MUST_DIFFER_FROM_CREATOR`) |
| `Draft` | `PendingApproval` tanpa lampiran scan SK | Scan SK resmi bertandatangan adalah syarat legal mutasi/promosi (`HRD-DEC-056`) |
| `Applied` | `Cancelled` | Pembatalan hanya sah sebelum tanggal berlaku. Setelah berlaku, buat SK baru (`HRD-DEC-057`) |
| `Rejected` atau `Cancelled` | mana pun | Keadaan akhir. Perubahan baru memerlukan pembuatan dokumen SK baru |

### 6.1 Permohonan perubahan data pegawai — `EmployeeProfileChangeService.RequestStatuses`

Nilai: `Draft`, `Submitted`, `UnderVerification`, `NeedRevision`, `Approved`, `Rejected`,
`Cancelled`, `Applied`.

**Peringatan yang menghindarkan kesalahan mahal.** Kosakata ini **berbeda tipe** dari kosakata
cuti, walau sebagian nama nilainya kebetulan sama. Ia adalah field teks biasa yang divalidasi
daftar tertutup di dalam service, **bukan** konstanta cuti. Perbedaannya: kosakata ini punya
`UnderVerification` dan `Applied`, dan **tidak punya** `WaitingApproval`, `Taken`, `Recalled`,
maupun `Expired`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Draft` | Ajukan mandiri | `Submitted` | Pegawai pemilik data | Sekurang-kurangnya satu rincian perubahan diisi; bukti wajib untuk data sensitif (`HRD-DEC-052`) | Ditolak 400 bila bukti kosong pada data sensitif `[DECISION]` |
| `Draft` | Ajukan atas nama pegawai | `Submitted` | HR Admin atas nama pegawai | Alasan pengajuan atas nama wajib diisi; berkas bukti wajib untuk data sensitif (`HRD-DEC-052`) | Ditolak 400 bila alasan atas nama atau bukti kosong `[DECISION]` |
| `Submitted` | Mulai verifikasi | `UnderVerification` | Pemegang `EmployeeProfileChange : Update` | — | `[VOCAB]` |
| `UnderVerification` | Setujui | `Approved` | Pemegang `EmployeeProfileChange : Update` | Seluruh verifikasi yang wajib sudah diputuskan | `[VOCAB]` |
| `UnderVerification` | Minta perbaikan | `NeedRevision` | Pemegang `EmployeeProfileChange : Update` | Alasan wajib diisi | `[VOCAB]` |
| `UnderVerification` | Tolak | `Rejected` | Pemegang `EmployeeProfileChange : Update` | Alasan wajib diisi | `[VOCAB]` |
| `Approved` | Terapkan ke profil | `Applied` | Pemegang `EmployeeProfileChange : Update` | — | `[EXISTING]` — nilai `Applied` terbukti terpisah dari `Approved`, menunjukkan penerapan adalah langkah tersendiri |

**Kewenangan "atasan atau HR" pada baris di atas belum diverifikasi.** Jangan menyimpulkannya
dari pola domain lain.

### 6.1a Perubahan penempatan dan remunerasi — `ApprovalStatus` `HRD-DEC-031`

Nilai: `Draft`, `Submitted`, `UnderReview`, `NeedRevision`, `Approved`, `Rejected`, `Cancelled`.

Berlaku pada empat jenis transaksi yang **terpisah** `[DECISION]` `HRD-DEC-036`:

| Jenis transaksi | Entity | Definisi alur |
| --- | --- | --- |
| Perubahan penetapan gaji | `WfpSalaryAssignment` | Sendiri |
| Perubahan penempatan organisasi | `WfpOrganizationAssignment` | Sendiri |
| Perubahan penempatan jabatan | `WfpPositionAssignment` | Sendiri |
| Perubahan penetapan atasan | `WfpManagerAssignment` | Sendiri |

**Keempatnya memakai kosakata status yang sama dan pola persetujuan awal yang sama, tetapi
MUST NOT memakai satu definisi alur bersama.** Perubahan kebijakan pada satu jenis transaksi
**MUST NOT** menyeret ketiga jenis lainnya. Penggunaan ulang konfigurasi langkah atau template
diperbolehkan bila mesin workflow memang memungkinkannya tanpa membuat definisi yang sama.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Draft` | Ajukan | `Submitted` | HR Admin pembuat | Isian lengkap; tanggal berlaku terisi | Ditolak `400` `[DECISION]` |
| `Submitted` | Mulai tinjau | `UnderReview` | Pemegang butir `: Approve` | **Peninjau MUST berbeda dari pembuat** | Ditolak `403` `[DECISION]` |
| `UnderReview` | Minta perbaikan | `NeedRevision` | Pemegang butir `: Approve` | Alasan wajib diisi | Ditolak `400` `[DECISION]` |
| `NeedRevision` | Ajukan lagi | `Submitted` | HR Admin pembuat | — | `[DECISION]` |
| `UnderReview` | Setujui | `Approved` | Pemegang butir `: Approve` | **Penyetuju MUST berbeda dari pembuat** | Ditolak `403` `[DECISION]` |
| `Submitted` | Selesaikan penyetuju saat pemrakarsa sama dengan calon penyetuju | tetap `Submitted`, ditugaskan ulang | Sistem | Penyelesaian ke penyetuju tingkat lebih tinggi yang berwenang sesuai konfigurasi | Tugas tertahan dan muncul di daftar pengawasan HR. **MUST NOT** menjadi swa-setuju `[DECISION]` `HRD-DEC-036` |
| `UnderReview` | Tolak | `Rejected` | Pemegang butir `: Approve` | Alasan wajib diisi | Ditolak `400` `[DECISION]` |
| `Draft` atau `Submitted` | Batalkan | `Cancelled` | HR Admin pembuat | — | `[DECISION]` |

**Perpindahan yang tidak sah dan harus ditolak** `[DECISION]` `HRD-DEC-031`:

| Dari | Ke | Alasan ditolak |
| --- | --- | --- |
| `Draft` | `Approved` langsung | Melewati pengajuan dan peninjauan berarti meniadakan pemisahan peran |
| mana pun | `Approved` oleh pembuatnya sendiri | Inti `APPROVER_MUST_DIFFER_FROM_CREATOR`. **Tidak ada pengecualian**, termasuk ketika unit hanya punya satu petugas |
| `Rejected` | mana pun | Keadaan akhir. Perbaikan lewat pengajuan baru |
| `Cancelled` | mana pun | Keadaan akhir |

**Gerbang efektivitas yang mengikat.** Penempatan **MUST NOT** berlaku efektif selama
`ApprovalStatus` belum bernilai `Approved`. Menyimpan barisnya saja tidak membuatnya berlaku.

**Keadaan hari ini:** seluruh perpindahan di atas berstatus **`MISSING`**. Kosakata statusnya
belum ada, dan tidak ada satu pun penjaga yang menegakkannya. Ini `IMPLEMENTATION_WORK` turunan
`HRD-DEC-031`, bukan perilaku yang sudah berjalan.

### 6.1b Versi kebijakan gaji — `SalaryPolicyStatus` `HRD-DEC-043`

Nilai: `Draft`, `Scheduled`, `Active`, `Superseded`, `Cancelled`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Draft` | Jadwalkan | `Scheduled` | `HR Manager` | Tanggal berlaku terisi dan belum lewat | Ditolak `400` `[DECISION]` |
| `Scheduled` | Tanggal berlaku tiba | `Active` | Sistem | Tidak ada versi lain yang aktif pada cakupan dan tanggal yang sama | `[DECISION]` |
| `Active` | Versi baru berlaku | `Superseded` | Sistem | Versi penerus sudah `Active` | `[DECISION]` |
| `Draft` atau `Scheduled` | Batalkan | `Cancelled` | `HR Manager` | Belum pernah `Active` | Ditolak `409` `[DECISION]` |

**Perpindahan yang tidak sah dan harus ditolak:**

| Dari | Ke | Alasan ditolak |
| --- | --- | --- |
| `Active` | `Draft` | Kebijakan yang pernah berlaku **MUST NOT** disunting menjadi draf. Perbaikan lewat versi baru |
| `Superseded` | mana pun | Keadaan akhir. Riwayat **MUST NOT** dihidupkan ulang |
| `Active` | dihapus | Riwayat versi **MUST NOT** dihapus; tanpa itu, pertanyaan "kenapa gaji berubah saat itu" tidak terjawab |

### 6.1c Calon penyesuaian gaji — `SalaryAdjustmentCandidateStatus` `HRD-DEC-043`

Nilai: `Detected`, `UnderReview`, `Accepted`, `Dismissed`, `Superseded`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| — | Perubahan faktor terdeteksi | `Detected` | Sistem | Faktor pegawai berubah — golongan, level, status kerja, atau **jenjang pendidikan terverifikasi** — dan kebijakan yang berlaku menghasilkan rekomendasi. **Masa kerja bukan pemicu** pada MVP saat ini, `HRD-DEC-045` | `[DECISION]` |
| `Detected` | Tinjau | `UnderReview` | HR Admin atau `HR Manager` | — | `[DECISION]` |
| `UnderReview` | Terima menjadi usulan penetapan gaji | `Accepted` | HR Admin atau `HR Manager` | Membentuk pengajuan `T8`, **bukan** mengubah gaji | Ditolak `409` bila mencoba mengubah gaji langsung `[DECISION]` |
| `UnderReview` | Abaikan | `Dismissed` | HR Admin atau `HR Manager` | Alasan wajib diisi | Ditolak `400` `[DECISION]` |
| `Detected` atau `UnderReview` | Faktor berubah lagi | `Superseded` | Sistem | Calon baru terbentuk | `[DECISION]` |

**Invariant yang paling penting pada kedua tabel ini:** calon penyesuaian **MUST NOT** mengubah
gaji efektif. Ia hanya dapat membentuk pengajuan `T8`, yang tetap tunduk pada
`APPROVER_MUST_DIFFER_FROM_CREATOR`. Pihak yang menerima calon **MUST NOT** menjadi penyetuju
akhir penetapan gaji itu ketika pemisahan peran menuntutnya.

**Keadaan hari ini: `MISSING`.** Kedua kosakata belum ada di kode. `IMPLEMENTATION_WORK` turunan
`HRD-DEC-043`.

### 6.2 Verifikasi perubahan data — `VerificationStatuses`

Nilai: `Pending`, `Verified`, `Rejected`, `NeedRevision`.

### 6.3 Penempatan dan penetapan gaji

Catatan penempatan dan penetapan gaji berlaku per tanggal efektif (`HRD-DEC-047`) setelah status persetujuannya disetujui (`HRD-DEC-031`).

- **Jalur persetujuan** (`HRD-Q-19` tertutup lewat `HRD-DEC-031`, `HRD-DEC-048`, `HRD-DEC-056`): wajib mengikuti rantai persetujuan pola T8; pembuat dilarang menyetujui pengajuannya sendiri.
- **Berlaku surut** (`HRD-Q-18` tertutup lewat `HRD-DEC-050`): tanggal efektif berlaku surut hanya diterima jika masih berada di dalam periode kehadiran yang masih terbuka. Pengajuan ditolak sistem jika periode kehadiran sudah ditutup.

### 6.4 Batch Data Awal Penempatan Kepegawaian — `HrdInitialPlacementBatch` (`HRD-DEC-054`)

Nilai status: `Draft`, `Validated`, `Submitted`, `Approved`, `Rejected`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Draft` | Validasi berkas baris | `Validated` | Sistem / HR Admin | Seluruh baris lolos aturan: tepat 1 unit utama, tanpa duplikat, tanpa lubang, pegawai belum pernah masuk batch (`ONCE_PER_EMPLOYEE`) | Status tetap `Draft`; daftar baris salah ditampilkan untuk diperbaiki |
| `Validated` | Ajukan batch | `Submitted` | HR Admin pengunggah | `FailedRows == 0` | Ditolak 400 bila masih ada baris gagal validasi |
| `Submitted` | Setujui sekali | `Approved` | HR Manager / `CorporateHr` | **Penyetuju MUST berbeda dari pengunggah** (`HRD-DEC-031`). Persetujuan berlaku untuk seluruh baris | Ditolak 403 bila disetujui pembuatnya sendiri |
| `Submitted` | Tolak batch | `Rejected` | HR Manager / `CorporateHr` | Alasan penolakan wajib diisi | Ditolak 400 bila alasan kosong |

---

## 7. Pengembangan Orang dan Lifecycle

### 7.1 Tindakan disiplin — `WfpDisciplinaryAction.ActionStatus`

Nilai: `Draft`, `Issued`, `UnderReview`, `Approved`, `Rejected`, `Effective`, `Completed`,
`Cancelled`.

| Aspek | Keadaan sebenarnya |
| --- | --- |
| Kosakata | `[EXISTING]` — himpunan tertutup di dalam controller, **bukan** enum resmi |
| Perpindahan | **`[VOCAB]` — hanya keanggotaan himpunan yang diperiksa, bukan urutan.** Status apa pun dalam himpunan dapat berpindah ke status lain dalam himpunan yang sama, tanpa guard urutan |
| Penilaian | Ini **transisi lemah**, didokumentasikan apa adanya. Ia **bukan** state machine penuh, dan tidak boleh dianggap begitu |

**Urutan yang seharusnya berlaku, sebagai usulan target — belum diputuskan:**

`Draft` → `Issued` → `UnderReview` → `Approved` → `Effective` → `Completed`, dengan `Rejected`
dan `Cancelled` sebagai keadaan akhir.

**Temuan yang harus diselesaikan sebelum kemampuan ini diperluas:**

| Temuan | Dampak | Pemilik |
| --- | --- | --- |
| **Pembuat tindakan dapat menyetujui tindakannya sendiri** | Tidak ada pemisahan peran pada keputusan yang menyangkut nasib seorang pegawai | `[OPEN]` `HRD-Q-51` — pemilik proses |
| Data bertanda paling rahasia **tidak** punya tingkatan izin khusus | Kasus kedisiplinan dapat dibaca siapa pun yang memegang izin baca umum | `[OPEN]` `HRD-Q-52` — pemilik keamanan |

Status banding: `Submitted`, `UnderReview`, `Accepted`, `Rejected`, `Withdrawn`. Perpindahannya
juga `[VOCAB]`.

### 7.2 Kasus, keputusan, dan investigasi kedisiplinan

Kosakata statusnya ada di model, tetapi **lebih lemah lagi**: field teks bebas **tanpa daftar
tertutup sama sekali**. Perpindahan: **tidak ada** — tidak ada controller `[VOCAB]`.

### 7.3 Penilaian kinerja — `WfpPerformanceReview`

Kosakata tahap: `Draft`, `SelfAssessment`, `ManagerAssessment`, `Calibration`, `Finalized`,
`Acknowledged`, `Cancelled`.

| Dari keadaan | Tindakan | Ke keadaan | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| Skor belum lengkap | Finalkan | Ditandai final | Pemegang `PerformanceReview : Update` | **Seluruh rincian sudah berskor** | Ditolak `[EXISTING]` — guard nyata |
| Ditandai final | Ubah, ubah tahap, atau hapus | — | — | **Ditolak**, termasuk pada rinciannya | Ditolak `[EXISTING]` |
| Ditandai final | Akui | Ditandai diakui | Pegawai yang dinilai | **Harus** sudah final lebih dulu | Ditolak `[EXISTING]` |
| Ditandai diakui | mana pun | — | — | **Tidak ada** perpindahan lanjutan yang ditemukan | `[OPEN]`/`MISSING` |

**Pola yang layak ditiru.** Guard finalkan dan akui pada domain ini **benar-benar dijaga kode** —
berbeda dari `Acknowledged` pada pemanggilan kembali cuti yang ternyata tidak dijaga. Domain lain
yang butuh pola serupa sebaiknya meniru bentuk ini.

**Catatan tentang tahap siklus penilaian.** Siklus punya kosakata `Draft`, `Open`, `GoalSetting`,
`MidReview`, `FinalReview`, `Calibration`, `Completed`, `Closed`, `Cancelled`, tetapi
**tidak ada guard urutan** — endpoint ubah tahap menerima nilai apa pun. Satu-satunya pemeriksaan
nyata pada jalur penilaian adalah penanda aktif, bukan tahapnya `[VOCAB]`.

### 7.4 Rekaman pelatihan dan asesmen kompetensi

| Entity | Bentuk status | Perpindahan |
| --- | --- | --- |
| Rekaman pelatihan | Penanda **sudah diverifikasi** bernilai benar atau salah | `[EXISTING]` — dari belum menjadi sudah; **tidak ditemukan** jalur mundur |
| Asesmen kompetensi | Hasil asesmen: `Unknown`, `NotAssessed`, `Passed`, `Failed`, `NeedTraining`, `Expired`, `Waived`, ditambah penanda sudah diverifikasi | `[EXISTING]` untuk penanda verifikasi; hasil asesmen dapat disunting |

**Yang perlu diketahui.** Keduanya adalah **pencatatan pasca-kejadian**, bukan siklus pendaftaran
sampai kelulusan. Sebelas entity rencana pelatihan formal punya kosakata status di model tetapi
**tidak ada controller** yang mengoperasikannya `[VOCAB]`.

**Apakah verifikasi bespoke ini seharusnya disatukan dengan mesin persetujuan generik** masih
`[OPEN]` — `HRD-Q-53`.

### 7.5 Pengunduran diri — `ResignationValueConstants.Status`

Nilai: `Draft`, `Submitted`, `UnderReview`, `NeedRevision`, `Approved`, `Rejected`, `Cancelled`,
`HandoffCompleted`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Draft` | Ajukan | `Submitted` | Pegawai pemohon | Tanggal terakhir bekerja dan alasan wajib diisi | Ditolak `[EXISTING]` — guard nyata |
| `Submitted` | Tinjau | `UnderReview` | Sistem, lewat mesin persetujuan | — | `[EXISTING]` |
| `UnderReview` | Setujui, tolak, atau minta perbaikan | `Approved`, `Rejected`, `NeedRevision` | Penyetuju yang ditugaskan | Mengikuti keputusan pada mesin persetujuan | `[EXISTING]` |
| `Approved` | Jalankan serah terima | `HandoffCompleted` | Pemegang `ResignationRequest : Update` | Statusnya **harus** `Approved`; idempoten | Ditolak `409` `[EXISTING]` |
| `Draft`, `Submitted`, atau `UnderReview` | Batalkan | `Cancelled` | Pegawai pemohon | Belum disetujui | Ditolak `[EXISTING]` |

**Ini satu-satunya alur lifecycle yang benar-benar matang** — 1 dari 21 model yang operasional.

**Yang perlu diketahui setelah `HandoffCompleted`:**

| Hal | Keadaan |
| --- | --- |
| Daftar periksa offboarding dibuat | `[EXISTING]` — dibuat **satu kali** |
| Status tugas offboarding dimutakhirkan sesudahnya | **`MISSING`** — tidak ada kode yang memutakhirkannya |
| Akun aplikasi dicabut | **`MISSING`** — tidak otomatis. Source sendiri memuat peringatan eksplisit soal ini. Kontrak ke Identity `[OPEN]` `HRD-DEP-003` |
| Tanggal terakhir bekerja diteruskan ke kehadiran dan payroll | **`MISSING`** `[OPEN]` `HRD-Q-50` |

### 7.6 Onboarding, masa percobaan, pemberhentian, pensiun, non-perpanjangan kontrak

Kosakata statusnya ada di model untuk seluruhnya. **Perpindahan: tidak ada** untuk semuanya —
tidak ada controller maupun service yang mengoperasikan salah satu pun `[VOCAB]`.

---

## 8. Payroll sisi HR

### 8.1 Putaran payroll — `TrxPayrollRun.RunStatus`

Nilai: `Draft`, `CollectingInput`, `Calculating`, `Review`, `WaitingApproval`, `Approved`,
`PaymentProcessing`, `Paid`, `Posted`, `Closed`, `Cancelled`, `Reversed`.

**Kenyataan yang harus dibaca sebelum apa pun dirancang di atasnya:**

| Dari status | Ke status | Keadaan |
| --- | --- | --- |
| `Draft` | `CollectingInput` | `[VOCAB]` — **tidak ada kode yang menuliskannya** |
| `CollectingInput` | `Calculating` | `[VOCAB]` |
| `Calculating` | `Review` | `[VOCAB]` — dan **tidak ada service kalkulasi** sama sekali |
| `Review` | `WaitingApproval` | `[VOCAB]` |
| `WaitingApproval` | `Approved` | `[VOCAB]` — dan **tidak ada service persetujuan** |
| `Approved`, `Paid`, `Posted`, `Closed`, `Cancelled` | — | **`[EXISTING]`** — status-status ini **dibaca** untuk **memblokir** penulisan snapshot baru. Inilah satu-satunya bagian yang benar-benar ditegakkan kode |

**Kesimpulan yang tidak boleh dilewat: `Payroll Executed` bukan `Employee Paid`.** Yang benar-benar
berjalan hari ini hanyalah pengumpulan snapshot masukan dari tiga domain. Kalkulasi lintas domain
menjadi angka gaji `MISSING`; persetujuan tingkat putaran `MISSING`; dan **bagaimana putaran
payroll benar-benar dimulai** masih `[OPEN]` — `HRD-Q-49`.

### 8.2 Snapshot masukan payroll — status yang benar-benar ditulis

| Snapshot | Status yang ditulis | Provenance |
| --- | --- | --- |
| Kehadiran harian | Ditandai terkunci; status masukan payroll menjadi `Processed` | `[EXISTING]` |
| Realisasi lembur | `RealizationStatus` menjadi `PostedToPayroll` | `[EXISTING]` |
| Cuti | Baris masukan variabel dibuat atau dimutakhirkan per permohonan | `[EXISTING]` |

**Ketiganya memeriksa status putaran payroll sebelum menulis, dan menolak bila sudah terminal**
`[EXISTING]`.

**Idempotensi.** Menjalankan serah terima dua kali untuk data yang sama **tidak** menghasilkan
dua snapshot: kehadiran memakai penanda hasil idempoten, lembur memakai kunci idempotensi, dan
cuti memeriksa baris yang sudah ada `[EXISTING]`.

### 8.3 Batas yang tidak boleh dilewati

`[DECISION]` `HRD-DEC-009`: setelah serah terima dijalankan, **tanggung jawab HR selesai**.
Bentuk data yang diterima Finance `[OPEN]` `HRD-Q-10`; perilaku bila Finance menolak satu batch
`[OPEN]` `HRD-Q-11`. Tidak ada perpindahan status yang boleh dirancang melewati batas ini.

---

## 9. Ringkasan perpindahan yang **tidak** dijaga hari ini

Tabel ini adalah daftar pekerjaan pengerasan, disusun berdasarkan besar akibatnya.

| No | Perpindahan | Keadaan | Akibat bila dibiarkan | Sumber |
| ---: | --- | --- | --- | --- |
| 1 | Koreksi kehadiran `Applied` → `Approved` lewat sinkronisasi | `[DEFECT]` | Kehadiran harian dimutasi ulang; angka yang sudah diserahkan ke payroll berubah tanpa jejak yang jelas | `HRD-DEC-022` |
| 2 | Cuti `Completed` → `Cancelled` atau `Taken` lewat pembalikan tanpa syarat | `[DEFECT]` | Saldo dan kehadiran berubah tanpa alasan tercatat, bahkan setelah payroll terkunci | `HRD-DEC-023` |
| 3 | Jadwal kerja berlaku surut disunting langsung | `MISSING` | Kehadiran pada periode yang sudah diproses dihitung ulang dengan jadwal yang berbeda | `HRD-DEC-027` |
| 4 | Tindakan disiplin berpindah status tanpa urutan, dan dapat disetujui pembuatnya | `[VOCAB]` + `[OPEN]` | Keputusan yang menyangkut nasib pegawai tidak punya pemisahan peran | `HRD-Q-51` |
| 5 | Tahap siklus penilaian kinerja berpindah tanpa urutan | `[VOCAB]` | Siklus dapat melompat ke tahap akhir tanpa melewati penilaian | — |
| 6 | Batas waktu dan eskalasi tidak pernah dijalankan | `MISSING` | Pengajuan menggantung tanpa batas; tidak ada yang mengingatkan penyetuju | `HRD-DEC-030` |
| 7 | Roster, shift harian, penggantian, tenaga darurat, dan siaga | `[VOCAB]` | Tidak ada mesin penjadwalan operasional sama sekali untuk rumah sakit 24 jam | `HRD-DEC-026` |
| 8 | Onboarding, masa percobaan, pemberhentian, pensiun | `[VOCAB]` | Hanya pengunduran diri yang punya alur; keluar-masuk pegawai lainnya dikerjakan di luar sistem | — |
| 9 | Kedaluwarsa delegasi persetujuan | `[VOCAB]` | Delegasi yang sudah lewat masa berlakunya mungkin masih aktif | — |
| 10 | Persetujuan perubahan penempatan dan remunerasi | `MISSING` | Perubahan gaji dan penempatan berlaku tanpa persetujuan, dan pembuatnya dapat menyetujui sendiri | `HRD-DEC-031` |
| 11 | Nominal gaji terbuka pada daftar lintas pegawai | `MISSING` | Butir sensitif nominal belum ada, sehingga butir baca umum sudah cukup untuk melihat nominal | `HRD-DEC-033` |

---

## 10. Traceability

| Kelompok status | Decision ID | Flow |
| --- | --- | --- |
| Periode kehadiran, pengecualian, koreksi | `HRD-DEC-022`, `HRD-DEC-025`, `HRD-DEC-028` | `flows/02-attendance.md`, `flows/07-attendance-correction.md` |
| Permohonan cuti, eksekusi, pembatalan, pemanggilan kembali | `HRD-DEC-023`, `HRD-DEC-024` | `flows/03-leave.md` |
| Rencana, permohonan, realisasi, verifikasi, periode lembur | — | `flows/04-overtime.md` |
| Penempatan jadwal, roster, shift harian | `HRD-DEC-026`, `HRD-DEC-027` | `flows/05-work-scheduling.md` |
| Ubah jadwal dan tukar shift | — | `flows/06-shift-change-swap.md` |
| Instance, tugas, dan delegasi persetujuan | `HRD-DEC-011`, `HRD-DEC-018`, `HRD-DEC-030` | `flows/09-unified-approval.md` |
| Perubahan data pegawai, penempatan, penetapan gaji | `HRD-DEC-012` | `flows/01-employee-administration.md` |
| Putaran payroll dan snapshot masukan | `HRD-DEC-009` | `flows/10-payroll-processing-handoff.md` |
| Pengunduran diri dan offboarding | — | `flows/11-lifecycle-offboarding.md` |
| Pelatihan dan kompetensi | — | `flows/12-competency-training.md` |
| Penilaian kinerja | — | `flows/13-performance-management.md` |
| Tindakan disiplin | — | `flows/14-employee-relations-discipline.md` |

Status untuk kredensial, kewenangan klinis, kesehatan kerja staf, perencanaan tenaga kerja,
rekrutmen, benefit, tiket HR, perjalanan dinas, dan reimbursement **tidak** ditulis di sini.
Seluruhnya `BLOCKED` atau `DEFERRED`.

---

## 11. Perencanaan Tenaga Kerja — `S-D1` (`last_changed_in` `v7`, 3 November 2026)

Seluruh transisi di bawah **belum ada di kode**. Nama status sama persis dengan enum pada
`02-backend-architecture.md` 16.4.11 dan dipakai apa adanya pada `flowcharts/`. Keputusan dari
kotak masuk persetujuan diteruskan ke domain oleh `HrdWorkforcePlanningWorkflowIntegrationService`;
setiap langkah `AllowSelfApproval = false`, sehingga pembuat tidak pernah muncul sebagai penyetuju
(`WorkflowService.cs:543-556`).

### 11.1 Standar kebutuhan tenaga — `HrdStaffingStandard` (`T9`)

Nilai status: `Draft`, `Submitted`, `Returned`, `Rejected`, `Scheduled`, `Active`, `Superseded`, `Cancelled`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| — | Buat draf versi baru | `Draft` | HR Admin atau Kepala Unit | Tidak ada draf lain yang terbuka untuk unit itu | `409` |
| `Draft`, `Returned` | Ajukan | `Submitted` | Pembuat | Minimal satu baris; setiap baris lengkap untuk metodenya; faktor cadangan dan pembulatan dipilih; tanggal berlaku ≥ hari ini; penyetuju langkah 2 dapat ditemukan | `422`; `400` bila penyetuju tidak ditemukan |
| `Submitted` | Setujui langkah terakhir, tanggal berlaku di masa depan | `Scheduled` | Direktur yang membawahi unit (`HRD-DEC-072`) | Langkah 1 HR Manager sudah disetujui | — |
| `Submitted` | Setujui langkah terakhir, tanggal berlaku hari ini | `Active` | Sama | Sama; versi aktif sebelumnya menjadi `Superseded` | — |
| `Scheduled` | Tanggal berlaku tiba | `Active` | Sistem | Versi aktif sebelumnya menjadi `Superseded` dengan `EffectiveEndDate` H-1 | Dicatat log scheduler |
| `Active` | Versi penggantinya aktif | `Superseded` | Sistem | — | — |
| `Submitted` | Kembalikan | `Returned` | Penyetuju langkah mana pun | Alasan wajib | `400` |
| `Submitted` | Tolak | `Rejected` | Penyetuju langkah mana pun | Alasan wajib | `400` |
| `Draft`, `Returned` | Batalkan | `Cancelled` | Pembuat atau HR Manager | — | — |

**Perpindahan yang tidak sah:**

| Dari | Ke | Alasan ditolak |
| --- | --- | --- |
| `Draft` | `Active` | Standar tanpa pengesahan `T9` **MUST NOT** dipakai hitungan maupun Penjadwalan |
| `Active`, `Scheduled`, `Superseded` | diubah isinya | Versi yang sudah disahkan dikunci; perubahan = versi baru |
| `Superseded` | `Active` | Versi lama tidak dihidupkan kembali; buat versi baru |
| `Submitted` | `Scheduled` oleh pembuatnya sendiri | `APPROVER_MUST_DIFFER_FROM_CREATOR` |

### 11.2 Batch formasi awal — `HrdInitialFormationBatch` (`T10`)

Nilai status: `Draft`, `Submitted`, `Returned`, `Rejected`, `Applied`, `Cancelled`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Draft`, `Returned` | Ajukan | `Submitted` | HR Admin pembuat | `InvalidRows = 0`; scan dokumen sumber terunggah; tidak ada baris unit × jabatan yang sudah punya formasi atau sudah ada di batch lain yang belum `Rejected`/`Cancelled` | `422` |
| `Submitted` | Setujui langkah terakhir | `Applied` | Direktur Utama | HR Manager dan Finance sudah menyetujui; versi 1 formasi lahir untuk setiap baris dalam satu transaksi | Seluruhnya batal bila satu baris gagal |
| `Submitted` | Kembalikan / Tolak | `Returned` / `Rejected` | Penyetuju langkah mana pun | Alasan wajib | `400` |
| `Draft`, `Returned` | Batalkan | `Cancelled` | Pembuat | — | — |

**Tidak sah:** `Draft` → `Applied` tanpa `T10` (cacat `HRD-TF-033` yang tidak boleh ditiru);
`Submitted` dengan baris tidak valid (cacat `HRD-TF-034`); `Applied` → apa pun — koreksi lewat addendum.

### 11.3 Rencana tahunan dan addendum — `HrdManpowerPlan` (`T10`)

Nilai status: `Draft`, `Submitted`, `Returned`, `Rejected`, `Approved`, `Cancelled`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| — | Buat rencana tahunan | `Draft` | HR Admin | Belum ada rencana tahunan aktif untuk rumah sakit dan tahun yang sama | `409` |
| — | Buat addendum | `Draft` | HR Admin | Rencana induk berstatus `Approved` | `422` |
| `Draft`, `Returned` | Hitung ulang | tetap | HR Admin | — | — |
| `Draft`, `Returned` | Ajukan | `Submitted` | HR Admin pembuat | Setiap baris punya grade; baris yang usulannya berbeda dari rekomendasi punya alasan; addendum memuat sekurang-kurangnya satu baris; potret gap tersimpan | `422` |
| `Submitted` | Koreksi Finance | tetap `Submitted` | Penyetuju langkah Finance yang sedang aktif | Alasan wajib; angka awal tetap tersimpan | `403` bila bukan penyetuju langkah aktif |
| `Submitted` | Setujui langkah Finance | tetap `Submitted` | Finance | Tidak ada baris "tarif belum tersedia" tanpa koreksi Finance (`FINANCE_ADJUSTMENT`) | `422` |
| `Submitted` | Setujui langkah terakhir | `Approved` | Direktur Utama | Versi formasi baru per baris; permintaan berjalan yang kini melebihi formasi ditandai | Seluruhnya batal bila satu baris gagal |
| `Submitted` | Kembalikan / Tolak | `Returned` / `Rejected` | Penyetuju langkah mana pun | Alasan wajib | `400` |
| `Draft`, `Returned` | Batalkan | `Cancelled` | Pembuat | — | — |

**Tidak sah:** `Approved` → diubah atau dibuka kembali (`APPROVED_PLAN_LOCKED`); addendum dari
rencana yang belum `Approved`; `Approved` → `Cancelled`.

### 11.4 Permintaan headcount — `HrdHeadcountRequest` (`T11`, `T12`)

Nilai status: `Draft`, `Submitted`, `Returned`, `Rejected`, `Withdrawn`, `ReadyForRecruitment`, `PartiallyFulfilled`, `Fulfilled`, `Closed`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Draft`, `Returned` | Ajukan | `Submitted` | Kepala Unit pembuat | Jumlah kursi luar + internal = jumlah kursi; kursi pengganti merujuk orang berstatus akan kosong atau sudah keluar; jenis dalam formasi tidak melebihi sisa kursi; jenis sementara punya alasan dan tanggal akhir; jenis di luar formasi punya alasan; potret gap tersimpan. Rantai `T11` untuk `Replacement`, `AdditionalWithinFormation`, `Temporary`; `T12` untuk `AdditionalBeyondFormation` | `422` |
| `Submitted` | Setujui langkah terakhir `T11` | `ReadyForRecruitment` | HR Manager | Sisa kursi diperiksa ulang (`RECHECK_ON_FINAL_APPROVAL`) | `409`; permintaan otomatis menjadi `Returned` agar Kepala Unit menyesuaikan jumlah (`VR-WFP-23`) |
| `Submitted` | Setujui langkah terakhir `T12` | `ReadyForRecruitment` | Direktur yang membawahi unit | Versi formasi baru lahir dalam transaksi yang sama | Seluruhnya batal bila gagal |
| `Submitted` | Kembalikan / Tolak | `Returned` / `Rejected` | Penyetuju langkah mana pun | Alasan wajib | `400` |
| `Draft`, `Submitted`, `Returned` | Tarik kembali | `Withdrawn` | Pembuat | Belum ada persetujuan terakhir | `409` bila sudah `ReadyForRecruitment` |
| `ReadyForRecruitment` | Kursi pertama terpenuhi, masih ada kursi `Open`/`Linked`/`Frozen` | `PartiallyFulfilled` | Sistem | — | — |
| `ReadyForRecruitment`, `PartiallyFulfilled` | Seluruh kursi `Fulfilled` | `Fulfilled` | Sistem | — | — |
| `ReadyForRecruitment`, `PartiallyFulfilled` | Seluruh kursi `Fulfilled` atau `Cancelled`, sekurang-kurangnya satu `Cancelled` | `Closed` | Sistem | — | — |

**Tidak sah:** `Draft` → `ReadyForRecruitment` tanpa persetujuan; `ReadyForRecruitment` →
`Withdrawn` (gunakan pembatalan kursi `T13`); `Fulfilled`/`Closed` → apa pun; jenis permintaan
diubah sesudah diajukan.

### 11.5 Kursi permintaan — `HrdHeadcountRequestSeat`

Nilai status: `Open`, `Linked`, `Fulfilled`, `Frozen`, `Cancelled`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `Open` | Tautkan penempatan atau SK | `Linked` | HR Admin atau Recruiter | Permintaan `ReadyForRecruitment`/`PartiallyFulfilled`; unit dan jabatan cocok; penempatan utama; penempatan/SK belum menautkan kursi lain; kursi internal hanya ke SK, kursi luar ke penempatan | `422`; `409` bila sudah dipakai kursi lain |
| `Linked` | Tanggal mulai penempatan tiba | `Fulfilled` | Sistem | Penempatan aktif pada tanggal itu | — |
| `Linked` | Lepas tautan | `Open` | HR Admin atau Recruiter | Penempatan belum berlaku | `409` |
| `Linked` | SK yang ditautkan dibatalkan | `Open` | Sistem | SK berstatus `Cancelled` | — |
| `Open`, `Linked` | Permintaan menjadi melebihi formasi setelah addendum | `Frozen` | Sistem | Kursi itu di luar formasi baru | — |
| `Frozen` | Persetujuan `T12` baru menambah formasi | `Open` | Sistem | — | — |
| `Open`, `Frozen` | Pembatalan `T13` disetujui | `Cancelled` | HR Manager lewat kotak masuk | Tidak ada lowongan terbuka untuk kursi itu (`CLOSE_VACANCY_FIRST`) | `409` |

**Tidak sah:** `Fulfilled` → `Cancelled` atau `Open` (`ONLY_UNFILLED_SEATS`); `Fulfilled` → ubah
sumber pemenuhan (`PLAN_CHANGE_RESPECTS_FULFILLED`); `Frozen` muncul di antrean Recruiter.

### 11.6 Pembatalan kursi — `HrdHeadcountRequestCancellation` (`T13`)

Nilai status: `Submitted`, `Approved`, `Rejected`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| — | Ajukan pembatalan | `Submitted` | Kepala Unit atau HR | Kursi yang dipilih `Open` atau `Frozen`; alasan wajib | `422` |
| `Submitted` | Setujui | `Approved` | HR Manager, bukan pengaju | Kursi → `Cancelled`; bila permintaan berasal dari `T12`, versi formasi turun sebanyak porsi kursi yang dibatalkan (`T12_CANCEL_REDUCES_FORMATION`) | Seluruhnya batal bila gagal |
| `Submitted` | Tolak | `Rejected` | HR Manager | Alasan wajib; kursi kembali tanpa tanda pembatalan | `400` |

### 11.7 Formasi, potret, dan pengaturan

| Entity | Transisi yang sah | Yang tidak sah |
| --- | --- | --- |
| `HrdPositionFormation` | Ditulis sekali oleh sistem; `EffectiveEndDate` diisi saat versi baru lahir | Setiap pengubahan nilai lain, penghapusan, atau penulisan oleh pengguna (`NO_DIRECT_FORMATION_EDIT`) |
| `HrdStaffingGapSnapshot` | Ditulis sekali | Setiap pengubahan atau penghapusan (`SNAPSHOT_IMMUTABLE`); menulis potret `Monthly` untuk bulan lampau — wajib `Reconstructed` (`MISSED_SNAPSHOT_NOT_FAKED`) |
| `HrdWorkforcePlanningSetting` | Diubah HR Manager dengan alasan | Diubah peran lain |

### 11.8 Penandaan otomatis yang tidak mengubah status

| Penanda | Menyala ketika | Padam ketika |
| --- | --- | --- |
| `IsOverFormationAfterAddendum` | Addendum atau rencana disetujui dan kursi permintaan berjalan kini melebihi formasi | Kursi berlebih dibatalkan atau `T12` baru disetujui |
| `IsOverdue` | Permintaan `ReadyForRecruitment`/`PartiallyFulfilled` melewati `OverdueRequestThresholdDays` sejak siap direkrut | Permintaan `Fulfilled` atau `Closed`. **Tidak** membatalkan apa pun (`NO_AUTO_EXPIRY`) |
