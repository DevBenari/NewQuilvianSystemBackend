# Laporan Perubahan Backend — `BE-HRD-342`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-342` |
| Judul | Setelan kehadiran sebagai satu-satunya sumber toleransi dan jendela |
| Slice | Amandemen `A52` atas `S-B1` Kehadiran — `EPIC HRD-111`, gelombang `MVP-67` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 24, Grafik B86 |
| Trace | `FR-PK-010`; `HRD-DEC-196`, `211`, `215`, `216`; `HRD-ASM-83`; `INV-A52-05`; `02-backend-architecture.md` 35.4.2 (`AttendanceSettingService`, `AttendanceProcessingService`, `AttendanceScheduleResolverService`); `api-contract.md` 32 Settings (**Diubah**); `VR-PK-23`, `VR-PK-30`; `AT-HRD-PK-24`, `-25`, `-27` |
| Contract version | `v26` — `approved` (pengguna, 12 November 2026) |
| Dependency | `BE-HRD-335` ✅; kolom jendela dari migration `BE-HRD-334` ✅ (diterapkan di devYoga) |
| Klasifikasi | `MEDIUM` — skor 6: satu repository (0), sekitar 12 berkas diperiksa (1), 4 berkas source (1), logika pemrosesan inti (2), perubahan kontrak endpoint setelan (1), tanpa schema (0), tanpa keamanan baru (0), tanpa UI (0), ditambah 1 untuk kompatibilitas layar lama |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/Corporate/HumanResource/AttendanceManagement/{Services,DTOs}/`; dokumen roadmap/traceability/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `400a9678` (branch `yoga`) + perubahan `BE-HRD-339`–`341` dan migration `BE-HRD-FIX-001` yang belum di-commit |
| Tanggal | 13 November 2026 |
| Status | ✅ Selesai — harness 72/72 (termasuk kontrol negatif); kontrak Settings dan pemrosesan diperiksa lewat API devYoga. `AT-HRD-PK-24`, `-25`, `-27` bermetode unit service menurut matriks |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `AttendanceManagement` |
| Pemilik / prefix registry | `Human Resource` / `Hrd` (`ACTIVE / LEGACY`) |
| Keberlakuan | `TOUCHED LEGACY` — empat berkas yang sudah ada |
| QBE yang berlaku | `QBE-SVC-001` (aturan di service) |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; registry; `rules/backend/` |

---

## 1. Masalah yang diperbaiki

| Cacat sebelum task ini | Akibat |
| --- | --- |
| Toleransi terlambat dibaca dari `MstGracePeriodPolicy` lewat resolver, dan bila nol diganti angka tertanam **10** | Setelan kehadiran yang diubah HRD tidak berpengaruh; toleransi 0 (tanpa toleransi) diam-diam menjadi 10 menit |
| Jendela scan memakai konstanta **240/480** atau angka kebijakan per jadwal, mana yang lebih besar, ditambah perpanjangan dari `MaximumWorkMinutes` | Kolom jendela di setelan (`BE-HRD-334`) tidak dipakai; HRD tidak dapat mengatur scan mana yang dipasangkan |
| Pemrosesan tetap berjalan dengan angka cadangan bila setelan tidak ada | Rekap dihitung dengan angka yang tidak ditetapkan siapa pun (melanggar `HRD-ASM-83`) |
| GET/PUT setelan tidak memuat jendela scan dan masih memuat `AutoConvertIncompleteToAbsent` | Kontrak `v26` tidak terpenuhi; pilihan yang sudah dicabut `HRD-DEC-216` masih bisa diubah |

---

## 2. Proses bisnis

**Contoh (`HRD-DEC-215`).** Admin HRD mengubah toleransi dari 10 menjadi 15 menit. Rina scan 07.12
untuk shift 07.00. Pemrosesan berikutnya mencatat Rina tepat waktu. Sebelum perubahan, Rina tercatat
terlambat 12 menit. Kebijakan per jadwal yang berisi 30 menit tidak berpengaruh.

**Jendela (`HRD-DEC-211`).** Dengan jendela 240 menit, scan 02.30 untuk shift 07.00 tidak dipasangkan,
sehingga scan masuk yang dipakai adalah 07.12. Bila HRD melebarkan jendela menjadi 300 menit, scan
02.30 ikut terbaca.

**Setelan tidak terbaca (`HRD-ASM-83`, `VR-PK-30`).** Bila setelan tidak ada, terhapus, gagal dibaca,
atau nilainya di luar batas `VR-PK-23`, tanggal yang diproses dicatat `Error` dengan pesan
"Setelan kehadiran tidak terbaca": status kehadiran `Unprocessed`, payroll diblokir, log mesin tidak
disentuh. Tanggal itu dapat diproses ulang sesudah setelan diperbaiki.

**Layar setelan.** GET dan PUT memuat `ScanWindowBeforeMinutes` dan `ScanWindowAfterMinutes`; pilihan
konversi otomatis tidak lagi ditampilkan maupun disimpan (`HRD-DEC-216`). Batas `VR-PK-23`: toleransi
0–60, jendela sebelum 0–720, sesudah 0–960 menit.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `00-interview-decisions.md` `HRD-DEC-196`, `211`, `215`, `216`, `HRD-ASM-83`; `api-contract.md` 32 Settings; `validation-matrix.md` `VR-PK-23`, `-30`; `acceptance-test-matrix.md` `AT-HRD-PK-24`–`27`
- `AttendanceSettingService.cs`, `AttendanceSettingDtos.cs`, `HrdAttendanceSetting.cs`, `AttendanceSettingController.cs`
- `AttendanceProcessingService.cs` (alur `ProcessOneCoreAsync`, `BuildCaptureWindow`, `ApplyPunchResult`, deteksi pengecualian), `AttendanceScheduleResolverService.cs` (`ResolveAttendancePolicyAsync`, `CalculateAttendanceWindows`)
- `AttendanceSelfServiceCaptureService.cs` (konsumen `EarliestCheckInAt` dari resolver)
- Frontend `attendance-settings-view.jsx`: PUT saat ini mengirim `gracePeriodMinutes`, `maxBackdatedCorrectionDays`, `requireAttachmentForCorrection`, `autoConvertIncompleteToAbsent`, tanpa kolom jendela

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/AttendanceManagement/Services/AttendanceSettingService.cs` | Baru: `GetRequiredSettingAsync` (mengembalikan `AttendanceRuleSetting` atau gagal dengan "Setelan kehadiran tidak terbaca"; tanpa angka cadangan). GET/PUT memuat jendela; PUT memvalidasi `VR-PK-23` dan tidak lagi menyimpan `AutoConvertIncompleteToAbsent`; jendela yang tidak dikirim mempertahankan nilai tersimpan. Penyediaan otomatis setelan memakai nilai bawaan model, bukan angka di service. Dihapus: `GetEffectiveGracePeriodMinutesAsync` (cadangan 10, tanpa pemanggil). Record baru `AttendanceRuleSetting` |
| `Areas/Corporate/HumanResource/AttendanceManagement/DTOs/AttendanceSettingDtos.cs` | Respons: `+ScanWindowBeforeMinutes`, `+ScanWindowAfterMinutes`, `−AutoConvertIncompleteToAbsent`. Request: `+ScanWindowBeforeMinutes?` (0–720), `+ScanWindowAfterMinutes?` (0–960), `−AutoConvertIncompleteToAbsent`; pesan batas memakai teks `VR-PK-23`; bawaan `GracePeriodMinutes = 10` di DTO dihapus |
| `Areas/Corporate/HumanResource/AttendanceManagement/Services/AttendanceProcessingService.cs` | Konstanta 240/480 dan cadangan 10 dihapus. Setelan dibaca sebelum resolver; tak terbaca → `RecordSettingUnreadable`. Jendela = mulai shift − X s.d. selesai shift + Y dari setelan; toleransi = `GracePeriodMinutes` setelan (0 berarti tanpa toleransi). `DetectionRule` pengecualian terlambat menjadi `AttendanceSetting.GracePeriodMinutes` |
| `Areas/Corporate/HumanResource/AttendanceManagement/Services/AttendanceScheduleResolverService.cs` | Konstruktor menerima `AttendanceSettingService`. `EarlyCheckInMinutes`, `LateCheckInGraceMinutes`, `LateCheckOutMinutes` diisi dari setelan (bukan `MstGracePeriodPolicy`); bila tak terbaca, nol + peringatan. `EarlyCheckOutGraceMinutes` tetap dari kebijakan (di luar `HRD-DEC-215`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sesuai `v26` Settings (**Diubah**). Delta yang dicatat: kolom jendela pada request opsional (kosong = tidak diubah) agar layar lama `FE-HRD-099` yang belum mengirimnya tidak mengosongkan jendela; `autoConvertIncompleteToAbsent` yang masih dikirim layar lama diabaikan. 400 dari validasi model berbentuk ProblemDetails dengan teks `VR-PK-23` |
| Database | Tanpa migration. Kolom `AutoConvertIncompleteToAbsent` dibiarkan (tidak dibaca, tidak ditulis); `MstGracePeriodPolicy`/`MstAttendancePolicy` dibiarkan |
| Keamanan/Auth | Tidak berubah |
| Perilaku | (1) Jendela tidak lagi diperlebar `MaximumWorkMinutes` kebijakan (`HRD-DEC-215` butir 1: jendela hanya dari setelan); devYoga tidak berisi kebijakan (`HRD-TF-068`), jadi tanpa perubahan hasil di sana. (2) Toleransi 0 kini benar-benar 0 |

---

## 4. Dokumentasi endpoint

### Setelan kehadiran

Base URL: `api/v1/corporate/human-resource/attendance-management/settings`

| Method | Path | Hak akses | Request | Response | Kode status |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | `AttendanceSetting : Read` | — | `AttendanceSettingResponse` (+ jendela, − `AutoConvertIncompleteToAbsent`) | 200 |
| `PUT` | `/` | `AttendanceSetting : Update` | `UpdateAttendanceSettingRequest` | `AttendanceSettingResponse` | 200, 400 |

Contoh PUT:

```json
{ "gracePeriodMinutes": 15, "maxBackdatedCorrectionDays": 3, "requireAttachmentForCorrection": true,
  "scanWindowBeforeMinutes": 240, "scanWindowAfterMinutes": 480 }
```

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 galat, 603 peringatan (tetap) | `PASS` | Keluaran build |
| Pencarian literal di `AttendanceProcessingService.cs` dan `AttendanceScheduleResolverService.cs` | Tidak ada 240, 480, atau cadangan 10 untuk toleransi/jendela | `PASS` | `grep` |
| Harness xUnit SQLite di memori, pemrosesan penuh (`ProcessSingleAsync` → resolver → rekap) (72 uji: 65 sebelumnya + 7 baru) | 72/72 lulus | `PASS` | `dotnet test` di `C:\Users\Admin\AppData\Local\Temp\claude\d823ad68bb5a8f68\5d4a0fb6-9e9b-47eb-9814-7cfdf33f8519\scratchpad\scopetest` |
| — `AT-HRD-PK-24`: toleransi 10 → terlambat 12 menit; ubah ke 15 → tepat waktu; toleransi 0 → terlambat 12 | Sesuai | `PASS` | Dua uji |
| — `AT-HRD-PK-25`: kebijakan global 30 menit (EarlyCheckIn 600, LateCheckOut 900) → tetap terlambat 12, scan 02.30 tetap di luar jendela; resolver melaporkan 10/240/480 dan `EarliestCheckInAt` 03.00 | Sesuai | `PASS` | Satu uji |
| — `HRD-DEC-215` uji (b): jendela 240 → scan masuk 07.12; jendela 300 → scan masuk 02.30 | Sesuai | `PASS` | Satu uji |
| — `AT-HRD-PK-27`: setelan dihapus → rekap `Error` "Setelan kehadiran tidak terbaca", `Unprocessed`, payroll diblokir, log mesin tetap 3; setelan toleransi 90 (di luar batas) → `Error` | Sesuai | `PASS` | Dua uji |
| — Kontrak Settings: tanpa `AutoConvertIncompleteToAbsent`; GET memuat 240/480; PUT jendela 721 → 400 dengan teks `VR-PK-23`; PUT tanpa jendela → nilai tersimpan dipertahankan; PUT 120/240 tersimpan, `UpdatedBy` tercatat | Sesuai | `PASS` | Satu uji |
| Kontrol negatif: toleransi pemrosesan dikembalikan ke angka 10 tertanam, backend dibangun ulang | Tepat uji `AT-HRD-PK-24` yang gagal. Kode dipulihkan (dibandingkan byte demi byte), dibangun ulang, 72/72 lagi | `PASS` | Keluaran `dotnet test` |
| API devYoga (13 November 2026): GET setelan | 200; 10/240/480; tanpa `autoConvertIncompleteToAbsent` | `PASS` | `at-log.jsonl` |
| — PUT payload layar lama (tanpa jendela, dengan `autoConvert…`) | 200; jendela tetap 240/480; kolom `AutoConvertIncompleteToAbsent` di basis data tidak tersentuh | `PASS` | `at-log.jsonl` |
| — PUT jendela sebelum 721 / toleransi 61 | 400 / 400 (ProblemDetails, teks `VR-PK-23`) | `PASS` | `at-log.jsonl` |
| — PUT jendela 120/240, lalu kembalikan 240/480 | 200 / 200; setelan akhir devYoga 10/240/480 (sama dengan awal) | `PASS` | `at-log.jsonl` |
| — Proses kehadiran Nadia 11 November 2026 dan resolver | `Processed`; resolver 10/240/480, `EarliestCheckInAt` 03.00Z (Siang 14.00 WIB − 240 menit) | `PASS` | `at-log.jsonl` |
| `AT-HRD-PK-27` lewat API devYoga | — | `NOT RUN` | Menghapus setelan devYoga akan menghentikan pemrosesan semua pegawai; matriks menetapkan metode unit service |
| Log backend | Tanpa galat selain `AttendanceScheduler:SystemActorUserId` yang sudah ada | `PASS` | `be.log` sesi |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tidak ada literal 10, 240, 480 untuk toleransi/jendela di kedua service | Terpenuhi | Bagian 5 (`grep`, kontrol negatif) |
| Data `MstGracePeriodPolicy` tidak memengaruhi hasil | Terpenuhi | `AT-HRD-PK-25` |
| Setelan tak terbaca → pemrosesan `Error` | Terpenuhi | `AT-HRD-PK-27` |
| `AT-HRD-PK-24`, `-25`, `-27` | Terpenuhi (unit service) | Bagian 5 |
| DoD umum | Terpenuhi | Batas 60/720/960 adalah batas validasi kontrak `VR-PK-23` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan baru |
| Masalah yang diketahui | (1) Layar `FE-HRD-099` masih menampilkan dan mengirim pilihan konversi otomatis serta belum memuat jendela scan; backend mengabaikan yang pertama dan mempertahankan jendela tersimpan. Penyesuaian layar ada di `FE-HRD-309`. (2) Pengecualian `OutsideScanWindow` untuk scan di luar jendela dan status `NotYetPresent`/`NoScan` adalah cakupan `BE-HRD-343`; pada task ini scan di luar jendela hanya tidak dipasangkan |
| Risiko tersisa | (1) Server dev bersama belum diperiksa isi `MstAttendancePolicy`/`MstGracePeriodPolicy`; bila berisi, hasil pemrosesan di sana akan berubah sesuai `HRD-DEC-215`. (2) Pemrosesan tanpa jadwal (rentang 00.00 s.d. 06.00 hari berikutnya) tidak disentuh |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 4 berkas source task ini (diubah) di atas perubahan `BE-HRD-339`–`341`; laporan ini; 2 dokumen roadmap/traceability. Tidak ada stage atau commit |
| Langkah berikutnya | `BE-HRD-343` — status harian `NotYetPresent`/`NoScan`, hari libur hanya jadwal tetap, scan di luar jendela |
