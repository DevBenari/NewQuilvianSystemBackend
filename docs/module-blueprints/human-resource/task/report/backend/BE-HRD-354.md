# Laporan Perubahan Backend — `BE-HRD-354`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-354` |
| Judul | Dispensasi pada pemrosesan harian (`POST-MVP`) |
| Slice | Amandemen `A52` — `EPIC HRD-116`, gelombang `POST-MVP` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 24, Grafik B88 |
| Trace | `FR-PK-025`; `HRD-DEC-200` (uji a–c); `INV-A52-07`; `INT-HR-102`; `02-backend-architecture.md` 35.9.4 butir 7 dan contoh dispensasi; `AT-HRD-PK-32` |
| Contract version | `v26` — `approved` (pengguna, 12 November 2026) |
| Dependency | `BE-HRD-353` ✅ |
| Klasifikasi | `HEAVY` — skor 8: satu repository (0), sekitar 15 berkas diperiksa (1), 2 berkas source diubah (0), logika inti pemrosesan kehadiran (3), pekerjaan latar (1), dampak payroll lewat menit terlambat (2), tanpa schema (0), tanpa UI (0), ditambah 1 untuk ketergantungan waktu |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/Corporate/HumanResource/AttendanceManagement/Services/{AttendanceProcessingService,AttendanceDispensationService}.cs`; proyek uji HR (`AttendanceDispensationProcessingTests.cs` baru, `AttendanceDispensationTests.cs`, `AcceptanceTestIndex.md`); dokumen roadmap/traceability/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `b29bff1d` (branch `yoga`) + perubahan `BE-HRD-357` dan `BE-HRD-353` yang belum di-commit |
| Tanggal | 17 November 2026 |
| Status | ✅ Selesai — proyek uji HR 214/214 (termasuk kontrol negatif per mutasi); `AT-HRD-PK-32` lulus lewat API devYoga dengan akun non-superadmin dan job hitung ulang yang dijalankan pekerja latar |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `AttendanceManagement` |
| Pemilik / prefix registry | `Human Resource` / `Hrd` (`ACTIVE / LEGACY`); tidak ada entity baru |
| Keberlakuan | `TOUCHED LEGACY` (`AttendanceProcessingService`) dan `NEW CODE` (`AttendanceDispensationService` dari `BE-HRD-353`) |
| QBE yang berlaku | `QBE-SVC-001` (aturan di service), `QBE-TXN-001` (penjadwalan sesudah keputusan tersimpan, kegagalannya tidak membatalkan keputusan) |
| Tidak berlaku | `QBE-API-001`/`QBE-DTO-001`/`QBE-PERM-001` (tanpa endpoint baru), `QBE-DB-*` (tanpa schema), `QBE-LOG-001` (tanpa aksi audit baru) |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

---

## 1. Masalah yang diperbaiki

| Cacat sebelum task ini | Akibat |
| --- | --- |
| Pemrosesan harian tidak membaca dispensasi (`BE-HRD-353` hanya mencatat keputusan) | Pegawai yang dispensasinya disetujui tetap tercatat terlambat dan berisiko dipotong |
| Persetujuan dispensasi tidak memicu hitung ulang | Rekap yang sudah ada tidak berubah sampai ada pemrosesan ulang lain |

---

## 2. Proses bisnis

**Perhitungan (35.9.4 butir 7, `HRD-DEC-200` butir 2).** Saat memproses satu pegawai pada satu tanggal kerja, pemroses membaca
dispensasi `Approved` pegawai itu untuk tanggal itu. Menit terlambat tetap dihitung dari jam jadwal masuk; toleransi hanya ambang
apakah pegawai dianggap terlambat. Lalu bagian rentang [jam jadwal masuk, jam scan masuk] yang tercakup dispensasi
`LateArrival` dikurangkan. Pulang cepat sama: bagian [jam scan pulang, jam jadwal pulang] yang tercakup dispensasi `EarlyDeparture`
dikurangkan. Rentang yang bertumpuk digabung, jadi tidak ada menit yang dihapus dua kali. Dispensasi berjenis lain, `Submitted`,
`Rejected`, atau `Cancelled` tidak berpengaruh.

**Contoh (`AT-HRD-PK-32`).** Shift 07.00, toleransi 10 menit, scan 07.40, dispensasi 07.00–07.40 disetujui → terlambat 0 menit,
pengecualian "Terlambat" hilang, pesan rekap "Dispensasi terlambat 40 menit". Tanpa dispensasi → 40 menit. Dispensasi 07.00–07.30 →
10 menit, dengan pengecualian "Terlambat 10 menit dari jadwal check-in. 30 menit lainnya dibebaskan dispensasi."

**Log mesin (`INV-A52-07`).** Scan tidak diubah; yang berubah hanya hasil hitung harian.

**Hitung ulang (`INT-HR-102`).** Sesudah dispensasi disetujui dan tersimpan, rekap harian pegawai pada tanggal itu dijadwalkan
dihitung ulang lewat `AttendanceSchedulerService.EnqueueAsync` (job `ATT-DISP-<id dispensasi>`, `ForceReprocess`, prioritas 100, pemicu =
penyetuju). Pekerja latar menjalankan job berpemicu pengguna. Bila rekap belum ada, tidak ada job; pemrosesan harian membaca
dispensasi saat tanggal itu diproses. Rekap terkunci (periode ditutup) tidak dijadwalkan. Kegagalan menjadwalkan dicatat sebagai
peringatan dan tidak membatalkan keputusan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `02-backend-architecture.md` 35.9.4 (urutan status, contoh dispensasi), 35.4.2 (`AttendanceProcessingService`, `AttendanceDispensationService`), keputusan desain "kolom tautan dispensasi pada `HrdAttendanceDaily`"; `00-interview-decisions.md` `HRD-DEC-200`; `integration-contract.md` `INT-HR-102`; `acceptance-test-matrix.md` `AT-HRD-PK-32`
- `AttendanceProcessingService.cs` (`ProcessSingleAsync` jalur proses, `ApplyPunchResult`, `BuildExceptions`, `BuildProcessingMessage`, `PunchResult`), `AttendanceSchedulerService.cs` (`EnqueueAsync`), `AttendanceRawLogService.cs` (pola `ATT-DEVREL`), model `HrdAttendanceSchedulerJob`, `HrdAttendanceDaily`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/AttendanceManagement/Services/AttendanceProcessingService.cs` | Memuat dispensasi `Approved` pegawai/tanggal sebelum `ApplyPunchResult`; `ApplyPunchResult` menerima rentang dispensasi dan mengurangi menit terlambat/pulang cepat sebesar bagian yang tercakup (`CoveredByDispensation`, gabungan rentang per jenis); `PunchResult` mencatat menit yang dibebaskan; pesan pengecualian dan `ProcessingMessage` menyebutnya. CRLF dipertahankan |
| `Areas/Corporate/HumanResource/AttendanceManagement/Services/AttendanceDispensationService.cs` | Dependensi `AttendanceSchedulerService`; `ScheduleReprocessAsync` dipanggil sesudah persetujuan tersimpan. **Perubahan pada aturan `BE-HRD-353`:** larangan rentang bertumpuk kini hanya untuk dispensasi **sejenis** (pesan "…dispensasi sejenis…") — lihat 3.4 |
| `Tests/QuilvianSystemBackend.HumanResourceTests/AttendanceDispensationProcessingTests.cs` | **Baru.** 5 uji pemrosesan; kelas pembantu `Penjadwal` |
| `Tests/QuilvianSystemBackend.HumanResourceTests/AttendanceDispensationTests.cs` | Konstruktor layanan memakai penjadwal; satu pengecekan "jenis lain pada jam yang sama boleh" |
| `Tests/QuilvianSystemBackend.HumanResourceTests/AcceptanceTestIndex.md` | `AT-HRD-PK-32` dipetakan ke uji |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada endpoint baru. Respons persetujuan tetap; hasilnya terlihat pada rekap harian dan laporan kehadiran |
| Database | Tanpa migration. Menulis job `HrdAttendanceSchedulerJob` sesudah persetujuan |
| Keamanan/Auth | Tidak berubah |
| Payroll | Menit terlambat/pulang cepat yang dibebaskan tidak lagi menghasilkan pengecualian `Late`/`EarlyLeave` (keduanya tidak memblokir payroll) |

### 3.4 Delta dan keputusan desain

| Butir | Implementasi | Alasan |
| --- | --- | --- |
| Penanda "tepat waktu — dispensasi" (`HRD-DEC-200` contoh) | Teks di `ProcessingMessage` ("Dispensasi terlambat N menit") dan di pesan pengecualian bila masih ada sisa | Desain sengaja tidak menambah kolom tautan pada `HrdAttendanceDaily`; status harian tetap dari aturan 35.9.4 |
| Larangan bertumpuk `BE-HRD-353` | Dipersempit ke dispensasi sejenis | Pemrosesan memisahkan jenis, sehingga `LateArrival` dan `EarlyDeparture` pada jam yang sama tidak pernah menghapus menit ganda; larangan lintas jenis hanya menghalangi pengajuan sah |
| Sisa terlambat di bawah toleransi | Tetap dihitung (mis. scan 07.45, dispensasi 07.00–07.40 → 5 menit) | 35.9.4: hitung dengan toleransi, **lalu** kurangi menit dispensasi; `HRD-DEC-200` uji (b) |
| Rekap yang sudah dikoreksi (`IsCorrected`) | Tidak dihitung ulang (pemroses melewatinya, perilaku lama) | Hasil koreksi tidak boleh tertimpa; dispensasi dan koreksi pada hari yang sama perlu ditangani lewat koreksi |
| Dispensasi `Approved` tidak dapat dicabut | Tidak ada pemicu untuk pencabutan | State 30.3: `Approved` adalah status akhir |

---

## 4. Dokumentasi endpoint

Tidak ada endpoint baru. `POST attendance/dispensations/{id}/approve` (`BE-HRD-353`) kini juga menjadwalkan hitung ulang rekap harian
(`INT-HR-102`); kontrak respons tidak berubah.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 galat; tidak ada peringatan dari berkas yang diubah | `PASS` | Keluaran build |
| Proyek uji HR (`dotnet test`, SQLite di memori) | 214/214 lulus (209 sebelumnya + 5 uji pemrosesan dispensasi) | `PASS` | Keluaran `dotnet test` |
| — `AT-HRD-PK-32` | Scan 07.40 → terlambat 40; dispensasi masih `Submitted` → tetap 40; disetujui → job `ATT-DISP-<id>` (`ForceReprocess`, pegawai, tanggal) terjadwal; diproses → `Present`, 0 menit, tanpa pengecualian `Late`, pesan "Dispensasi terlambat 40 menit"; dua log mesin tetap (07.40 utuh) | `PASS` | `AT_HRD_PK_32_…` |
| — Uji (b) | Dispensasi 07.00–07.30 → terlambat 10 menit; pesan pengecualian menyebut 30 menit dibebaskan | `PASS` | `Keterlambatan_di_luar_rentang_tetap_dihitung` |
| — Pulang lebih awal, jenis, status | Pulang 13.00 → 60 menit; dispensasi `LateArrival` 13.00–13.30 (disisip) dan `EarlyDeparture` yang ditolak tidak berpengaruh; `EarlyDeparture` 13.00–14.00 disetujui → 0, `Present` | `PASS` | `Pulang_lebih_awal_dan_jenis_yang_tidak_cocok` |
| — Gabungan rentang | 07.00–07.20 + 07.10–07.30 + 07.50–07.55 (disisip) → terlambat 10 menit (bukan 0) | `PASS` | `Rentang_bertumpuk_digabung` |
| — Pemicu hanya untuk rekap yang ada dan tidak terkunci | Tanpa rekap → tanpa job (rekap yang dibuat kemudian langsung 0 menit); rekap terkunci → tanpa job | `PASS` | `Hitung_ulang_hanya_untuk_rekap_yang_ada_dan_tidak_terkunci` |
| — Larangan bertumpuk sejenis (`BE-HRD-353`) | Sejenis `409`; `EarlyDeparture` pada jam yang sama dengan `LateArrival` aktif `200` | `PASS` | `AttendanceDispensationTests.Validasi_pengajuan` |
| Kontrol negatif, **per putaran**: (1) jenis + gabungan dimatikan; (2) pemicu dihapus; (3) filter `Approved` dihapus | (1) gagal `Pulang_…` dan `Rentang_…` saja; (2) gagal `AT_HRD_PK_32` saja; (3) gagal `AT_HRD_PK_32` dan `Pulang_…`. Putaran gabungan awal (dengan pengurangan menit dimatikan) menggagalkan kelima uji. Kode dipulihkan (dibandingkan byte demi byte) setiap putaran | `PASS` | `mut354.sh` dan keluaran `dotnet test` |
| API devYoga putaran 1 (17 November 2026): Raka (profil tanpa akun) | Pemroses menolak `404` "belum mempunyai user account aktif" — uji pemrosesan butuh pegawai berakun. Persetujuan untuk Raka berhasil dan, tanpa rekap, tidak membuat job (sesuai rancangan). Data dipulihkan | — | `at-354.mjs`, `at-log.jsonl` (entri "354 …") |
| API devYoga putaran 2: penyetuju Cahyo | Login Cahyo ditolak "Email atau password salah". Skrip putaran ini juga keliru mencoba login sebagai gilang dengan sandi Cahyo (satu percobaan gagal pada akun gilang). Data dipulihkan (izin jabatan Cahyo identik 8 butir) | — | `at-354b.mjs`, `at-log.jsonl` (entri "354b …") |
| API devYoga putaran 3 — data sementara: scan uji `UJI-A52-354C-IN` gilang 07.40; atasan langsung gilang → dr. Bima; izin `AttendanceDispensation : Read/Approve` pada jabatan dr. Bima | Tersimpan | — | `at-354c.mjs` |
| — Sebelum dispensasi | Shift Pagi uji `UJI-A52-MYSCH` 07.00–14.00: `IncompletePunch` (belum pulang), terlambat 40, pengecualian `Late` "Terlambat 40 menit…" | `PASS` | `at-log.jsonl` (entri "354c …") |
| — Superadmin mencatat atas nama gilang; diproses ulang selagi `Submitted` | Penyetuju dr. Bima (server); terlambat tetap 40 | `PASS` | `at-log.jsonl` |
| — `AT-HRD-PK-32`: dr. Bima menyetujui | `200`; job `ATJ-20261117-000002-275FC5` (`ForceReprocess`, tanggal 17 Nov, pemicu dr. Bima) dijalankan pekerja latar sampai `Completed`; rekap terlambat **0**, pengecualian `Late` hilang (tinggal `MissingCheckOut`), pesan "Dispensasi terlambat 40 menit" | `PASS` | `at-log.jsonl` |
| — `INV-A52-07` | Scan uji tetap 07.40, tidak bertambah | `PASS` | `at-log.jsonl` |
| — Pemulihan | Izin jabatan dr. Bima dikirim ulang (45 butir) dan dibaca identik; atasan sementara, dispensasi, dan scan uji dihapus lunak; rekap gilang diproses ulang → `NotYetPresent`, 0 menit | `PASS` | `at-log.jsonl`; salinan `at-354c-policy-backup.json` |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Contoh 35.9.4: 07.40, dispensasi 07.00–07.40 → 0 menit | Terpenuhi | Harness dan devYoga |
| `AT-HRD-PK-32`: terlambat 0; log mesin tetap 07.40 | Terpenuhi | Harness dan devYoga |
| `HRD-DEC-200` uji (a) hanya rentang itu, (b) di luar rentang tetap dihitung, (c) log mesin tidak berubah | Terpenuhi | Bagian 5 |
| `INT-HR-102` hitung ulang asinkron sesudah commit | Terpenuhi | Job `Completed` di devYoga |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan build baru |
| Masalah yang diketahui | (1) Rekap yang sudah dikoreksi tidak dihitung ulang oleh pemroses (perilaku lama), sehingga dispensasi tidak berlaku pada hari yang sudah dikoreksi. (2) Akun gilang mendapat satu percobaan login gagal dari skrip putaran 2. (3) Sandi akun Cahyo Pamungkas yang diberikan ditolak — perlu dicek bila akun itu dipakai uji lain. (4) Tugas belajar (`INT-HR-102` bagian kedua) milik `BE-HRD-355` |
| Risiko tersisa | Bila job hitung ulang gagal, rekap baru berubah saat pemrosesan berikutnya (sapuan penjadwal atau proses ulang manual) |
| Perubahan sampingan | `NONE`. Data uji dan izin sementara di devYoga sudah dipulihkan |
| Interupsi | `NONE` |
| Status Git | 2 berkas source diubah (salah satunya berkas baru `BE-HRD-353`); 1 uji baru + 2 berkas uji diubah; laporan ini (baru); 2 dokumen roadmap/traceability. Perubahan `BE-HRD-357` dan `BE-HRD-353` sesi yang sama juga belum di-commit. Tidak ada commit |
| Langkah berikutnya | `BE-HRD-355` (tugas belajar), `BE-HRD-356` (gangguan mesin); sesudahnya `BE-HRD-358`. Frontend: `FE-HRD-318` kini tidak lagi menunggu backend |
