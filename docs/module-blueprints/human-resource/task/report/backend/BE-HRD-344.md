# Laporan Perubahan Backend — `BE-HRD-344`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-344` |
| Judul | Pratinjau tutup berisi daftar dan konversi tanpa syarat setelan |
| Slice | Amandemen `A52` atas `S-B1` Kehadiran — `EPIC HRD-111`, gelombang `MVP-67` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 24, Grafik B86 |
| Trace | `FR-PK-012`; `HRD-DEC-197` (butir 3–4), `HRD-DEC-207` (butir 5), `HRD-DEC-216`, `HRD-ASM-81`; `02-backend-architecture.md` 35.4.2, 35.9.4, 35.9.6; `api-contract.md` Period Management (**Diubah**); `state-transition-matrix.md` 30.6; `AT-HRD-PK-22`, `-23`, `-31` |
| Contract version | `v26` — `approved` (pengguna, 12 November 2026) |
| Dependency | `BE-HRD-343` ✅ (status `NoScan` sudah dihasilkan pemrosesan) |
| Klasifikasi | `STANDARD` — skor 5: satu repository (0), sekitar 10 berkas diperiksa (1), 2 berkas source (0), perubahan aturan tutup periode (2), perubahan bentuk respons aditif (1), tanpa schema (0), tanpa keamanan (0), tanpa UI (0), ditambah 1 untuk transaksi tutup |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/Corporate/HumanResource/AttendanceManagement/{Services/AttendancePeriodService.cs,DTOs/AttendancePeriodDtos.cs}`; dokumen roadmap/traceability/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `13b5d76b` (branch `yoga`), worktree bersih sebelum task |
| Tanggal | 16 November 2026 |
| Status | ✅ Selesai — harness 84/84 (termasuk kontrol negatif); `AT-HRD-PK-22`, `-23`, `-31` lulus lewat API devYoga |

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
| Pratinjau tutup hanya memberi **jumlah** `IncompletePunch` | HRD tidak tahu siapa, di unit mana, dan tanggal berapa yang akan menjadi Alpa sebelum menekan **Tutup** (melanggar `HRD-DEC-197` butir 3) |
| Tutup hanya mengonversi `IncompletePunch` | Hari "Tidak Ada Scan" (`NoScan`, sejak `BE-HRD-343`) dan nilai lama `Incomplete` (`HRD-TF-071`) terkunci tanpa menjadi `Absent` — pegawai yang tidak datang sama sekali tidak tercatat Alpa ke payroll (melanggar `HRD-DEC-207` butir 5, `HRD-DEC-216`) |
| Scan dari mesin yang belum terdaftar tidak tampak di pratinjau | HRD menutup periode tanpa tahu ada scan yang belum ikut dihitung (`HRD-ASM-81`) |

Kolom `AutoConvertIncompleteToAbsent` memang sudah tidak dibaca oleh `AttendancePeriodService` sebelum task ini; task ini menegaskannya dengan uji (`AT-HRD-PK-23`).

---

## 2. Proses bisnis

**Pratinjau (`HRD-DEC-197` butir 3).** Sebelum menutup periode, HRD membuka pratinjau dan melihat daftar hari
yang akan menjadi `Absent`: "Bagas Saputra — Unit Perawatan Intensif (ICU) — 2 Okt 2026 — Belum Lengkap",
"Raka Firmansyah — ICU — 3 Okt 2026 — Tidak Ada Scan", dan seterusnya. Daftar urut tanggal, unit, lalu nama.
Yang masuk daftar: `IncompletePunch` ("Belum Lengkap"), `Incomplete` (nilai lama), dan `NoScan` ("Tidak Ada Scan")
pada tanggal dan cakupan periode (rumah sakit/unit/departemen). Pratinjau juga menampilkan jumlah scan
"Menunggu Pendaftaran Mesin" pada tanggal periode sebagai peringatan, bukan penghalang.

**Tutup (`HRD-DEC-216`).** Saat HRD menekan **Tutup**, setiap hari di daftar itu selalu menjadi `Absent`
(`IsAbsent` benar, `IsPresent` salah, menit kerja 0) dengan keterangan "Konversi otomatis penutupan periode
(HRD-DEC-216: NoScan diubah menjadi Absent)". Tidak ada setelan untuk menundanya. Status lain (`Present`, cuti,
`NotYetPresent`) tidak berubah. Seluruh konversi dan penguncian terjadi dalam satu transaksi; bila gagal,
tidak ada yang berubah dan periode tetap terbuka.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `00-interview-decisions.md` `HRD-DEC-197`, `HRD-DEC-207`, `HRD-DEC-216`, `HRD-ASM-81`; `02-backend-architecture.md` 35.4.2, 35.9.4, 35.9.6; `api-contract.md` Period Management; `state-transition-matrix.md` 30.6, 30.7; `acceptance-test-matrix.md` `AT-HRD-PK-22`, `-23`, `-31`; `frontend-roadmap.md` `FE-HRD-311`
- `AttendancePeriodService.cs` (`PreviewCloseAsync`, `CloseAsync`, `BuildClosePreviewAsync`, `ApplyScope`, `AddIssue`), `AttendancePeriodController.cs`, `AttendancePeriodDtos.cs`
- `AttendanceValueConstants.cs` (`Incomplete`, `IncompletePunch`, `NoScan`, `AwaitingDeviceRegistration`), `HrdAttendanceDaily.cs`, `HrdAttendanceRawLog.cs`, `HrdAttendanceSetting.cs`, `AttendanceDailyQueryService.cs` (pola proyeksi nama pegawai)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/AttendanceManagement/Services/AttendancePeriodService.cs` | Baru: `CloseConversionStatuses` (`IncompletePunch`, `Incomplete`, `NoScan`), batas daftar `ConversionCandidatePreviewLimit` (500), `WibOffset`. `BuildClosePreviewAsync`: kueri daftar kandidat terproyeksi beserta jumlah total, hitung scan tertahan pada tanggal WIB periode (scan tanpa rumah sakit ikut dihitung untuk periode berlingkup rumah sakit), isu `HELD_DEVICE_SCAN` (peringatan, tidak memblokir), isu `INCOMPLETE_PUNCH_ATTENDANCE` kini menghitung seluruh kandidat. `CloseAsync`: konversi memakai `CloseConversionStatuses`, keterangan menyebut status asal, pesan respons menyebut "Belum Lengkap/Tidak Ada Scan" |
| `Areas/Corporate/HumanResource/AttendanceManagement/DTOs/AttendancePeriodDtos.cs` | `AttendancePeriodClosePreviewResponse`: tambah `ConversionCandidateCount`, `IsConversionCandidateListTruncated`, `ConversionCandidates`, `HeldDeviceScanCount` (dengan `<summary>` properti untuk schema). Kelas baru `AttendancePeriodConversionCandidateResponse` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `GET /{id}/close-preview`: respons diperluas secara aditif sesuai `v26`; field lama tetap (`IncompletePunchDailyCount` tetap menghitung `IncompletePunch` saja). `POST /{id}/close`: request dan bentuk respons tetap; `ConvertedIncompleteToAbsentCount` kini mencakup ketiga status |
| Database | Tanpa migration, tanpa perubahan schema. Kolom `AutoConvertIncompleteToAbsent` tetap ada dan tidak dibaca (penghapusan = task perapian, 35.4.1) |
| Keamanan/Auth | Tidak berubah: `close-preview` tetap `AttendancePeriod : Read`, `close` tetap `AttendancePeriod : Close` |
| Delta terhadap kontrak | Kontrak menyebut `ConversionCandidates[]` dan `HeldDeviceScanCount`; dua field pendamping `ConversionCandidateCount` dan `IsConversionCandidateListTruncated` ditambahkan untuk Risiko kartu ("dibatasi dengan jumlah total"). Isu baru `HELD_DEVICE_SCAN` mewujudkan 35.9.6 butir 3 ("sebagai peringatan") |

---

## 4. Dokumentasi endpoint

Base URL: `api/v1/corporate/human-resource/attendance/periods`

| Method | Path | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/{id}/close-preview` | `AttendancePeriod : Read` | — | `ApiResponse<AttendancePeriodClosePreviewResponse>` | `200`, `401`, `403`, `404` |
| `POST` | `/{id}/close` | `AttendancePeriod : Close` | `CloseAttendancePeriodRequest` (`Reason`) | `ApiResponse<AttendancePeriodActionResponse>` | `200`, `400`, `401`, `403`, `404`, `409`, `500` |

Field baru pada `AttendancePeriodClosePreviewResponse`:

| Field | Tipe | Isi |
| --- | --- | --- |
| `conversionCandidateCount` | `int` | Jumlah seluruh hari yang akan menjadi `Absent` |
| `isConversionCandidateListTruncated` | `bool` | Benar bila daftar dipotong (lebih dari 500) |
| `conversionCandidates[]` | daftar | `attendanceDailyId`, `workforceProfileId`, `profileCode`, `employeeName`, `organizationUnitId`, `organizationUnitName`, `attendanceDate`, `attendanceStatus` (status asal; label dari `attendance/dailies/filters/metadata`) |
| `heldDeviceScanCount` | `int` | Jumlah scan `AwaitingDeviceRegistration` pada tanggal periode |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 galat, 603 peringatan (tetap) | `PASS` | Keluaran build |
| Harness xUnit SQLite di memori (84 uji: 78 sebelumnya + 6 baru di `ClosePeriodTests.cs`) | 84/84 lulus | `PASS` | `dotnet test` di `C:\Users\Admin\AppData\Local\Temp\claude\d823ad68bb5a8f68\4222eeca-aa89-4652-b697-eb38df816c85\scratchpad\scopetest` |
| — `AT-HRD-PK-22`: periode ICU dengan `IncompletePunch`, `NoScan`, `Incomplete`, `Present`, `Leave`, `NotYetPresent`, ditambah `NoScan` unit lain dan `IncompletePunch` di luar tanggal | Pratinjau: 3 kandidat (nama, kode, unit, tanggal, status asal; urut tanggal), isu tidak memblokir. Tutup: ketiganya `Absent`, terkunci, menit 0; status lain tetap; unit lain dan tanggal lain tidak tersentuh; tutup ulang 409 | `PASS` | `Tutup_mengonversi_Belum_Lengkap_Tidak_Ada_Scan_dan_nilai_lama` |
| — `AT-HRD-PK-23`: `AutoConvertIncompleteToAbsent = false` di basis data uji | Konversi tetap terjadi (2/2) | `PASS` | `Setelan_lama_mati_tetap_dikonversi` |
| — `AT-HRD-PK-31`: empat scan tertahan di sekitar batas periode + satu scan terproses | `HeldDeviceScanCount` 2 (batas hari WIB benar), isu `HELD_DEVICE_SCAN` tidak memblokir, tutup berhasil | `PASS` | `Scan_tertahan_dihitung_tanpa_menghalangi_tutup` |
| — Periode berlingkup rumah sakit | Scan tanpa rumah sakit dan rumah sakit periode dihitung; rumah sakit lain tidak | `PASS` | `Scan_tertahan_mengikuti_rumah_sakit_periode` |
| — Risiko kartu: 501 kandidat | Daftar 500, jumlah 501, `IsConversionCandidateListTruncated` benar; tutup mengonversi 501 | `PASS` | `Daftar_panjang_dibatasi_dengan_jumlah_total` |
| — Jalur gagal: galat pada simpan kedua di dalam transaksi tutup | 500; tidak ada status yang berubah, rekap tidak terkunci, periode tetap `Open` | `PASS` | `Galat_saat_tutup_membatalkan_seluruh_konversi` |
| Kontrol negatif: `CloseConversionStatuses` dikembalikan ke `IncompletePunch` saja, backend dibangun ulang | Tepat tiga uji konversi gagal (`-22`, `-23`, daftar panjang). Kode dipulihkan (dibandingkan byte demi byte), dibangun ulang, 84/84 lagi | `PASS` | Keluaran `dotnet test` |
| Temuan selama uji: proyeksi nama memakai navigasi wajib `User` → INNER JOIN menyaring baris sehingga daftar kosong padahal jumlah 3 | Fallback `User` dibuang; nama dari profil SDM, pegawai, atau dokter | `PASS` sesudah perbaikan | Keluaran `dotnet test` pertama |
| API devYoga (16 November 2026), periode uji `ATP-202610-0001` "UJI-A52 BE-HRD-344 ICU 1-7 Okt 2026" dibuat lewat API; empat rekap ICU (Bagas Saputra `IncompletePunch` 2 Okt, Raka Firmansyah `NoScan` 3 Okt, Arif Maulana `Incomplete` 4 Okt, Indra Setiawan `Present` 5 Okt) dan satu scan tertahan `UJI-A52-MESIN-99` (3 Okt 08.00 WIB) disisipkan ke basis data | Pratinjau `200`: `canClose` benar, 3 kandidat dengan nama, kode profil, "Unit Perawatan Intensif (ICU)", tanggal, status asal; `heldDeviceScanCount` 1; isu `INCOMPLETE_PUNCH_ATTENDANCE` 3 dan `HELD_DEVICE_SCAN` 1, keduanya tidak memblokir | `PASS` | `at-log.jsonl` (entri "344 …") di `C:\Users\Admin\AppData\Local\Temp\claude\d823ad68bb5a8f68\5d4a0fb6-9e9b-47eb-9814-7cfdf33f8519\scratchpad\mig` |
| — Pratinjau tanpa sesi | `401` | `PASS` | `at-log.jsonl` |
| — Tutup dengan `AutoConvertIncompleteToAbsent = false` di devYoga | `200`, "dikonversi 3 hari Belum Lengkap/Tidak Ada Scan menjadi Absent"; basis data: ketiganya `Absent`, `IsAbsent`, terkunci, menit 0, keterangan menyebut status asal; Indra tetap `Present` (terkunci); setelan dikembalikan ke `true` | `PASS` | `at-log.jsonl` |
| — Tutup ulang | `409` | `PASS` | `at-log.jsonl` |
| Uji 403 dengan akun non-superadmin | Tidak dijalankan; atribut hak akses tidak diubah task ini | `NOT RUN` | — |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `IncompletePunch`, `Incomplete` lama, dan `NoScan` dikonversi | Terpenuhi | `AT-HRD-PK-22` harness dan devYoga |
| Kolom setelan lama tidak dibaca | Terpenuhi | `AT-HRD-PK-23` harness dan devYoga; tidak ada pembacaan `AutoConvertIncompleteToAbsent` di source selain properti model |
| Outcome: HRD melihat daftar dan jumlah scan tertahan; tutup selalu mengonversi | Terpenuhi | Bagian 5 |
| Risiko kartu: daftar panjang dibatasi dengan jumlah total | Terpenuhi | Uji 501 kandidat |
| DoD umum: build lulus, uji lulus, laporan ditulis, tanpa angka bisnis tertanam, tanpa kolom sensitif di log | Terpenuhi — angka 500 adalah batas teknis tampilan, bukan aturan bisnis | Bagian 3–5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan build baru |
| Masalah yang diketahui | (1) Rekap yang masih `NotYetPresent` saat tutup (sapuan `BE-HRD-343` belum sempat menjalankannya, misalnya karena `AttendanceScheduler:SystemActorUserId` belum dikonfigurasi) tidak dikonversi dan ikut terkunci sebagai "Belum Hadir"; state matrix 30.6 hanya mengizinkan tiga status. Perlu keputusan pengguna apakah sisa `NotYetPresent` yang shift-nya sudah lewat diperlakukan sebagai `NoScan` saat tutup, atau dijadikan penghalang. (2) Field `severity` pada `issues` tidak muncul di respons API walaupun ada di DTO (perilaku lama, di luar cakupan) |
| Risiko tersisa | (1) Daftar dibatasi 500; layar `FE-HRD-311` perlu menampilkan "500 dari N" dan merujuk ke daftar rekap harian bila terpotong. (2) Snapshot validasi (`ValidationSnapshotJson`) kini ikut menyimpan daftar kandidat (paling banyak 500 baris). (3) Batas hari scan tertahan memakai WIB (+07.00) tetap |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 2 berkas source diubah; laporan ini (baru); 2 dokumen roadmap/traceability. Tidak ada commit |
| Langkah berikutnya | `BE-HRD-345` — scan mesin tak terdaftar ditahan dan dilepas sesudah pendaftaran |

Data uji devYoga baru (bertanda `UJI-A52`): periode kehadiran `ATP-202610-0001` (ICU, 1–7 Okt 2026, kini `Closed`);
empat rekap harian ICU berketerangan `UJI-A52 BE-HRD-344 …` (tiga kini `Absent`); satu log mesin tertahan
`UJI-A52-RAW-HELD-344` (`AwaitingDeviceRegistration`, mesin `UJI-A52-MESIN-99`) — akan ikut dilepas bila mesin itu
didaftarkan pada uji `BE-HRD-345`/`346`.
