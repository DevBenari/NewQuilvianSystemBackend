# Laporan Perubahan Backend — `BE-HRD-345`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-345` |
| Judul | Scan mesin tak terdaftar ditahan dan dilepas sesudah pendaftaran |
| Slice | Amandemen `A52` atas `S-B1` Kehadiran — `EPIC HRD-112`, gelombang `MVP-67` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 24, Grafik B86 |
| Trace | `FR-PK-013`, `FR-PK-014`; `HRD-DEC-212`; `INV-A52-06`; `INT-HR-103`; `02-backend-architecture.md` 35.4.2, 35.9.6; `api-contract.md` Raw Logs (**Diubah**); `state-transition-matrix.md` 30.7; `AT-HRD-PK-29`, `-30` |
| Contract version | `v26` — `approved` (pengguna, 12 November 2026) |
| Dependency | `BE-HRD-335` ✅ (konstanta `AwaitingDeviceRegistration`) |
| Klasifikasi | `HEAVY` — skor 7: satu repository (0), sekitar 12 berkas diperiksa (1), 2 berkas source (0), perubahan perilaku penerimaan scan (2), transaksi + penjadwalan sesudah commit (2), dedup/unique index (1), tanpa schema (0), tanpa UI (0), ditambah 1 untuk konsumen mesin yang mengandalkan 404 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/Corporate/HumanResource/AttendanceManagement/{Services/AttendanceRawLogService.cs,DTOs/AttendanceRawLogDtos.cs}`; dokumen roadmap/traceability/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `13b5d76b` (branch `yoga`) + perubahan `BE-HRD-344` yang belum di-commit |
| Tanggal | 16 November 2026 |
| Status | ✅ Selesai — harness 92/92 (termasuk kontrol negatif); `AT-HRD-PK-29` lulus lewat API devYoga; `AT-HRD-PK-30` lulus lewat API `retry` + basis data (pendaftaran mesin lewat API menunggu `BE-HRD-346`) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `AttendanceManagement` |
| Pemilik / prefix registry | `Human Resource` / `Hrd` (`ACTIVE / LEGACY`) |
| Keberlakuan | `TOUCHED LEGACY` — service dan DTO yang sudah ada |
| QBE yang berlaku | `QBE-SVC-001` (aturan di service) |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

---

## 1. Masalah yang diperbaiki

| Cacat sebelum task ini | Akibat |
| --- | --- |
| Scan dari mesin yang belum terdaftar atau nonaktif ditolak `404` dan **tidak disimpan** | Mesin baru GDB-01 yang mengirim 50 scan sebelum didaftarkan kehilangan seluruh scan; pegawai tercatat "Tidak Ada Scan" walaupun datang (melanggar `HRD-DEC-212`, `INV-A52-06`) |
| Tidak ada cara memproses scan itu sesudah mesin didaftarkan | Mesin harus mengirim ulang, yang tidak selalu bisa |

---

## 2. Proses bisnis

**Menahan scan (`HRD-DEC-212` butir 2).** Mesin "GDB-01" belum terdaftar dan mengirim 50 scan. Server menyimpan
kelimapuluhnya dengan status **"Menunggu Pendaftaran Mesin"** (`AwaitingDeviceRegistration`), tanpa mesin, dengan
identitas mesin apa adanya. Identitas pegawai tetap dicocokkan dari kode pegawai di mesin. Respons tiap baris batch
berhasil berstatus ditahan, bukan galat. Scan ini **tidak** dipakai menghitung kehadiran, sehingga pegawai tetap
"Tidak Ada Scan" sampai mesin didaftarkan. Mesin yang terdaftar tetapi nonaktif diperlakukan sama. Kiriman ganda
tetap dikenali.

**Melepas scan (`HRD-DEC-212` butir 3, `INT-HR-103`).** Sesudah GDB-01 didaftarkan dan aktif,
`ReleaseHeldLogsForDeviceAsync` mencari scan tertahan yang identitas mesinnya cocok dengan external id, kode, atau nomor
seri mesin. Dalam satu transaksi, setiap scan diperlakukan seperti scan baru dari mesin terdaftar:

| Keadaan scan | Hasil |
| --- | --- |
| Identitas pegawai cocok | `Matched`, mesin terisi — siap dihitung |
| Identitas pegawai belum ditemukan | `Pending` (menunggu pencocokan, perilaku lama) |
| Scan yang sama sudah diterima lewat mesin terdaftar | `Duplicate` — tidak dihitung dua kali |
| Rumah sakit/lokasi scan tidak sesuai mesin, atau lokasi tidak mengizinkan mesin | `Rejected` |

Sesudah commit, hitung ulang paksa dijadwalkan untuk **rekap harian yang sudah ada** dan belum terkunci pada tanggal
scan (WIB) dan sehari sebelumnya (scan pulang shift malam tercatat pada tanggal masuk shift), satu job per rekap dengan
kunci `ATT-DEVREL-<rekap>-v<versi>`. Tanggal yang belum punya rekap tidak dijadwalkan: memprosesnya untuk pegawai tanpa
jadwal hanya menghasilkan rekap `Error`; tanggal itu mengambil scan yang dilepas saat diproses job harian. Bila
pelepasan gagal, seluruh scan tetap tertahan dan dapat dipicu ulang. Bila penjadwalan gagal, scan sudah dilepas dan tetap
tercakup sapuan penjadwal (`BE-HRD-343`) serta job harian.

**Retry.** `POST /raw-logs/{id}/retry` untuk scan tertahan tidak lagi mencocokkan ulang identitas saja (yang dulu akan
meloloskannya ke pemrosesan). Selama mesin belum terdaftar, jawabannya `409` "Mesin absen … belum terdaftar atau
nonaktif; scan tetap ditahan". Sesudah mesin terdaftar, retry menjalankan pelepasan untuk seluruh scan mesin itu.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `00-interview-decisions.md` `HRD-DEC-212`; `01-existing-capability-map.md` temuan (5); `02-backend-architecture.md` 35.4.2, 35.9.6; `api-contract.md` Raw Logs; `state-transition-matrix.md` 30.7; `integration-contract.md` `INT-HR-103`; `acceptance-test-matrix.md` `AT-HRD-PK-29`, `-30`
- `AttendanceRawLogService.cs` (`CreateAsync`, `CreateBatchAsync`, `RetryAsync`, `ResolveInputAsync`, `ResolveDeviceAsync`, `ResolveIdentityAsync`, `FindDuplicateAsync`, `ComputeEventHash`), `AttendanceRawLogController.cs`, `AttendanceRawLogDtos.cs`
- `HrdAttendanceRawLogConfiguration.cs` (indeks unik `EventHash` dan `(AttendanceDeviceId, ExternalLogId)`), `MstAttendanceDevice.cs`
- `AttendanceProcessingService.cs` (hanya log `Matched`/`Processed` yang dihitung), `AttendanceSchedulerService.cs` (`EnqueueAsync`, sapuan), `AttendanceSelfServiceCaptureService.cs` (pemakai `CreateAsync`)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/AttendanceManagement/Services/AttendanceRawLogService.cs` | `ResolveDeviceAsync` mengembalikan `DeviceResolution` (mesin aktif atau identitas tertahan); mesin tak terdaftar/nonaktif → tertahan, id mesin asing tanpa identitas lain tetap 404; pemeriksaan mesin terdaftar dipindah ke `ValidateRegisteredDevice`. `ResolveInputAsync`/`CreateAsync`: status `AwaitingDeviceRegistration` dan pesannya. Baru: `ReleaseHeldLogsForDeviceAsync`, `RetryHeldLogAsync`, `ValidateHeldLogAgainstDevice`, `FindRegisteredDuplicateAsync`, `ScheduleReprocessAfterReleaseAsync`. `RetryAsync` memisahkan scan tertahan. Konstruktor kini menerima `AttendanceSchedulerService` dan `ILogger`. Metadata filter memuat status "Menunggu Pendaftaran Mesin" |
| `Areas/Corporate/HumanResource/AttendanceManagement/DTOs/AttendanceRawLogDtos.cs` | Kelas baru `AttendanceRawLogReleaseResponse` (jumlah tertahan, `Matched`, `Pending`, `Duplicate`, `Rejected`, `Error`, job hitung ulang) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `POST /raw-logs` dan `/batch`: mesin tak terdaftar/nonaktif kini `200` dengan `processingStatus = AwaitingDeviceRegistration` (sebelumnya `404`). Bentuk request/response tetap. `POST /{id}/retry`: untuk scan tertahan `409` atau pelepasan. `filters/metadata`: opsi status baru |
| Database | Tanpa migration, tanpa schema baru. Memakai indeks `(ProcessingStatus, ReceivedAt)` dan dedup `EventHash` yang ada; menulis `HrdAttendanceSchedulerJob` (`ATT-DEVREL-…`) |
| Keamanan/Auth | Tidak berubah; tidak ada endpoint baru (`ReleaseHeldLogsForDeviceAsync` dipanggil `AttendanceDeviceController` di `BE-HRD-346`) |
| DI | `AttendanceRawLogService` kini bergantung pada `AttendanceSchedulerService` (tanpa siklus; startup Development lolos) |

### 3.4 Delta terhadap blueprint

| Butir blueprint | Implementasi | Alasan |
| --- | --- | --- |
| 35.9.6 butir 2 / state 30.7: scan dilepas "ke `Pending`" | `Matched` bila identitas pegawai cocok, `Pending` bila belum | Di kode, `Pending` berarti "menunggu pencocokan identitas" dan tidak dihitung pemrosesan; `HRD-DEC-212` butir 3 meminta scan "dihitung seperti scan biasa", dan scan biasa yang identitasnya cocok berstatus `Matched` |
| Cakupan kartu hanya `ResolveDeviceAsync`, `ReleaseHeldLogsForDeviceAsync` | `RetryAsync` ikut diubah | Tanpa penjaga, retry mengubah scan tertahan menjadi `Matched` dan meloloskannya ke pemrosesan (melanggar 30.7) |
| — | Id mesin (GUID) yang sama sekali tidak dikenal dan tanpa external id tetap `404` | Scan itu tidak punya identitas mesin untuk dicocokkan, sehingga tidak akan pernah bisa dilepas |
| — | Sidik (`EventHash`) dihitung ulang dengan id mesin saat dilepas | Agar kiriman ulang sesudah pendaftaran tetap dikenali ganda (`HRD-DEC-212` butir 4) |
| 35.9.6 butir 2: "memicu pemrosesan ulang tanggal terkait" | Hanya rekap yang sudah ada (tanggal scan dan sehari sebelumnya) yang dihitung ulang | Memproses tanggal tanpa jadwal membuat rekap `Error` (terbukti di devYoga); tanggal yang belum diproses mengambil scan lewat job harian |

---

## 4. Dokumentasi endpoint

Base URL: `api/v1/corporate/human-resource/attendance/raw-logs` (tanpa endpoint baru)

| Method | Path | Hak akses | Perubahan perilaku | Status |
| --- | --- | --- | --- | --- |
| `POST` | `/` | `AttendanceRawLog : Create` | Mesin tak terdaftar/nonaktif → disimpan `AwaitingDeviceRegistration` | `200`, `400`, `401`, `403`, `404` (id mesin asing tanpa external id), `409` |
| `POST` | `/batch` | `AttendanceRawLog : Create` | Baris dari mesin tak terdaftar berhasil, berstatus ditahan | `200`, `400`, `401`, `403` |
| `POST` | `/{id}/retry` | Tetap | Scan tertahan: `409` selama mesin belum terdaftar; sesudahnya melepas seluruh scan mesin itu | `200`, `401`, `403`, `404`, `409`, `500` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 galat, 603 peringatan (tetap) | `PASS` | Keluaran build |
| Harness xUnit SQLite di memori (92 uji: 84 sebelumnya + 8 baru di `DeviceHoldTests.cs`) | 92/92 lulus | `PASS` | `dotnet test` di `C:\Users\Admin\AppData\Local\Temp\claude\d823ad68bb5a8f68\4222eeca-aa89-4652-b697-eb38df816c85\scratchpad\scopetest` |
| — `AT-HRD-PK-29`: batch 50 scan GDB-01 tak terdaftar (25 pegawai) | 50 berhasil, 0 gagal, semua `AwaitingDeviceRegistration`, tanpa mesin, identitas pegawai terisi; pemrosesan tetap `NoScan`; kiriman ulang = ganda | `PASS` | `Scan_mesin_tak_terdaftar_ditahan_bukan_404` |
| — `AT-HRD-PK-30`: daftarkan GDB-01, lepas | 50 `Matched` bermesin; tepat 2 job paksa satu hari — rekap kemarin pegawai[3] dan rekap kemarin−1 pegawai[5] (kasus shift malam); rekap terkunci pegawai[6] tidak; jumlah rekap tidak bertambah; pemrosesan → `Present` 06.53; pelepasan kedua 0; kiriman ulang sesudah pendaftaran = ganda | `PASS` | `Pendaftaran_mesin_melepas_scan_dan_menjadwalkan_hitung_ulang` |
| — Mesin nonaktif: scan lewat id mesin | Ditahan dengan external id mesin; id mesin asing 404; pelepasan saat masih nonaktif 409; sesudah diaktifkan dilepas | `PASS` | `Mesin_nonaktif_ditahan_dan_dilepas_saat_diaktifkan` |
| — Retry scan tertahan | 409 dan tetap tertahan; sesudah mesin terdaftar → `Matched` | `PASS` | `Retry_scan_tertahan_menunggu_mesin` |
| — Identitas tak dikenal dan scan ganda | 1 `Pending` (bermesin), 1 `Duplicate` (tanpa mesin, tidak melanggar indeks unik), 0 job | `PASS` | `Pelepasan_menangani_identitas_kosong_dan_ganda` |
| — Jalur gagal: galat saat menyimpan pelepasan | 500; 50 scan tetap tertahan tanpa mesin; 0 job | `PASS` | `Galat_pelepasan_membiarkan_scan_tetap_tertahan` |
| — Identitas mesin dipakai dua mesin aktif | 409; scan tetap tertahan | `PASS` | `Identitas_mesin_ganda_ditolak` |
| — Regresi mesin terdaftar | `Matched` dengan mesin | `PASS` | `Mesin_terdaftar_tetap_normal` |
| Temuan selama uji: sidik dihitung ulang dari `EventAt` ber-`Kind` Unspecified (SQLite) menggeser jam lewat `ToUniversalTime` | `EventAt` ditegaskan UTC sebelum dihitung | `PASS` sesudah perbaikan | Keluaran `dotnet test` pertama |
| Kontrol negatif: status tertahan dimatikan (scan tak terdaftar langsung `Matched`), backend dibangun ulang | 7 dari 8 uji baru gagal (yang lulus: regresi mesin terdaftar). Kode dipulihkan (dibandingkan byte demi byte), dibangun ulang, 92/92 lagi | `PASS` | Keluaran `dotnet test` |
| Startup Development dengan ketergantungan baru | Backend menyala, Swagger 200, tanpa galat DI | `PASS` | `be345.log` |
| API devYoga (16 November 2026): roster `ROST-202611-0005` (ICU, 15 Nov 2026, Nadia Putri Pagi) dibuat dan diterbitkan lewat API; pemrosesan awal `NoScan` | Sesuai | `PASS` | `at-log.jsonl` (entri "345 …") di `C:\Users\Admin\AppData\Local\Temp\claude\d823ad68bb5a8f68\5d4a0fb6-9e9b-47eb-9814-7cfdf33f8519\scratchpad\mig` |
| — `AT-HRD-PK-29`: `POST /raw-logs/batch` 50 scan dari `UJI-A52-GDB-01` (belum terdaftar) | `200`: 50 berhasil, 0 gagal, seluruh baris `AwaitingDeviceRegistration`; basis data 50 baris tanpa mesin; Nadia tetap `NoScan`; kirim ulang satu scan → ganda; retry → `409` dan tetap tertahan | `PASS` | `at-log.jsonl` |
| — `AT-HRD-PK-30`: mesin `UJI-A52-GDB-01` disisipkan ke `MstAttendanceDevice`, lalu `POST /raw-logs/{id}/retry` | `200` "Dilepas 50 dari 50 scan tertahan …"; 50 `Matched` bermesin; pemrosesan → `Present` 06.30–14.24 dari 50 scan, seluruh log `Processed`; kirim ulang sesudah pendaftaran → ganda | `PASS` | `at-log.jsonl` |
| — **Temuan pada putaran pertama:** versi awal menjadwalkan rentang "tanggal −1 s.d. tanggal scan" per pegawai. Pekerja latar menjalankannya dan membuat rekap baru 14 Nov untuk Nadia yang tidak berjadwal hari itu, berstatus `Error` "Schedule belum terselesaikan" (job `CompletedWithErrors`) | Penjadwalan diubah: hanya rekap yang sudah ada dan belum terkunci, satu job per rekap. Rekap `Error` artefak uji itu dihapus lunak beserta 3 pengecualiannya | `FIXED` | `at-log.jsonl`; uji harness diperbarui |
| — Putaran ulang dengan kode final: 2 scan dari mesin kedua `UJI-A52-GDB-02` (belum terdaftar) → ditahan; mesin disisipkan; `retry` | `200` "Dilepas 2 dari 2 …; dijadwalkan 1 hitung ulang"; job `ATT-DEVREL-<rekap 15 Nov>-v5` **dieksekusi pekerja latar sampai `Completed`**; rekap 15 Nov `Present` 06.20–14.30 dari 52 scan; jumlah rekap Nadia tetap 2 (tidak ada rekap baru) | `PASS` | `at-log.jsonl` |
| — Pendaftaran mesin lewat API | Tidak dijalankan; endpoint mesin baru dibuat di `BE-HRD-346` | `NOT RUN` | — |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Contoh GDB-01 (35.9.6) terpenuhi | Terpenuhi | `AT-HRD-PK-29`, `-30` harness dan devYoga |
| Scan tertahan tidak ikut pemrosesan harian | Terpenuhi | Rekap tetap `NoScan` selama scan tertahan (harness dan devYoga); pemrosesan hanya membaca `Matched`/`Processed` |
| Risiko kartu: pengirim batch yang mengandalkan 404 | Dicatat (bagian 7) | — |
| DoD umum: build lulus, uji lulus, laporan ditulis, tanpa angka bisnis tertanam, tanpa kolom sensitif di log | Terpenuhi — 31 hari adalah batas rentang job penjadwal yang sudah ada | Bagian 3–5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan build baru |
| Masalah yang diketahui | (1) **Perubahan kontrak bagi pengirim mesin:** integrasi yang selama ini memakai `404` untuk mendeteksi mesin salah konfigurasi kini menerima `200` dengan `processingStatus = AwaitingDeviceRegistration`. Pengirim perlu membaca status per baris. (2) Pekerja latar ternyata mengeksekusi job yang punya pemicu pengguna (`TriggeredByUserId`); yang gagal karena `AttendanceScheduler:SystemActorUserId` kosong hanya job yang dipicu sistem (job harian, sapuan `BE-HRD-343`). Job pelepasan dipicu oleh petugas yang melepas, jadi berjalan |
| Risiko tersisa | (1) Scan tertahan yang dilepas bersamaan dengan scan baru dari mesin yang sama dapat bentrok di indeks unik; pelepasan gagal utuh (`500`), scan tetap tertahan, dan dapat dipicu ulang. (2) Pencocokan identitas mesin peka huruf besar/kecil, sama dengan penerimaan scan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 2 berkas source task ini (diubah); laporan ini (baru); 2 dokumen roadmap/traceability. Perubahan `BE-HRD-344` dari sesi yang sama juga belum di-commit. Tidak ada commit |
| Langkah berikutnya | `BE-HRD-346` — API daftar mesin absen (memanggil `ReleaseHeldLogsForDeviceAsync` sesudah mesin didaftarkan/diaktifkan) |

Data uji devYoga baru (bertanda `UJI-A52`): roster `ROST-202611-0005` (ICU, 15 Nov 2026, Nadia Putri Pagi, `Published`);
50 log mesin `UJI-A52-345-IN-0..24`/`-OUT-0..24` (kini `Processed`); mesin `UJI-A52-GDB-01` di `MstAttendanceDevice`
(aktif, sisip DB); 2 log `UJI-A52-345B-IN`/`-OUT` dari mesin `UJI-A52-GDB-02` (juga disisip DB, aktif); dua job
`ATT-DEVREL-…` (putaran pertama `CompletedWithErrors`, putaran ulang `Completed`). Rekap `Error` 14 Nov 2026 milik Nadia
yang dibuat putaran pertama sudah dihapus lunak. Log `UJI-A52-RAW-HELD-344` (mesin `UJI-A52-MESIN-99`) tetap tertahan.
