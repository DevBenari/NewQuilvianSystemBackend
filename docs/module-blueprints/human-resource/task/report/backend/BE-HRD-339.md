# Laporan Perubahan Backend — `BE-HRD-339`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-339` |
| Judul | Koreksi mundur dengan riwayat dan pemicu hitung ulang |
| Slice | Amandemen `A52` atas `S-B4` Penjadwalan — `EPIC HRD-110`, gelombang `MVP-67` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 24, Grafik B85 |
| Trace | `HRD-DEC-217`, `HRD-DEC-102`; `INT-HR-101`; `02-backend-architecture.md` 35.9.3, 35.10; `api-contract.md` 32 `POST /{id}/retroactive-correct` (**Diubah**); `state-transition-matrix.md` status pemrosesan kehadiran; `AT-HRD-PK-07` |
| Contract version | `v26` — `approved` (pengguna, 12 November 2026) |
| Dependency | `BE-HRD-338` 🟡 — `ShiftRevisionRecorder` sudah selesai dan terbukti; sisa `BE-HRD-338` hanya `AT-HRD-PK-02` lewat API, tidak memengaruhi task ini |
| Klasifikasi | `MEDIUM` — skor 6: satu repository (0), sekitar 14 berkas diperiksa (1), 2 berkas source (0), transaksi + konkurensi + jalur gagal hitung ulang (2), perubahan perilaku endpoint (1), tanpa schema (0), tanpa keamanan baru (0), tanpa UI (0), ditambah 2 untuk integrasi lintas submodul Kehadiran |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/Corporate/HumanResource/SchedulingManagement/{Services,DTOs}/`; dokumen roadmap/traceability/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `400a9678` (branch `yoga`) + migration `BE-HRD-FIX-001` yang belum di-commit |
| Tanggal | 13 November 2026 |
| Status | ✅ Selesai — terbukti lewat harness (45/45, termasuk kontrol negatif) dan lewat API devYoga (`AT-HRD-PK-07`: koreksi, riwayat, batas hari, hitung ulang berhasil, dan hitung ulang gagal) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `SchedulingManagement` (membaca dan menandai rekap `AttendanceManagement`) |
| Pemilik / prefix registry | `Human Resource` / `Hrd` (`ACTIVE / LEGACY`) |
| Keberlakuan | `TOUCHED LEGACY` — `ShiftReplacementService.RetroactiveCorrectionAsync` dan dua DTO yang sudah ada |
| QBE yang berlaku | `QBE-SVC-001` (seluruh aturan, transaksi, dan pemicu hitung ulang di service; controller tidak berubah) |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; registry; `rules/backend/` |

---

## 1. Masalah yang diperbaiki

| Cacat sebelum task ini | Akibat |
| --- | --- |
| Koreksi mundur menimpa shift dan jam tanpa jejak, hanya menulis `OverrideReason` | Tidak ada yang tahu jadwal semula; melanggar `HRD-DEC-217` |
| Jam baru dibentuk dari tanggal dan jam master tanpa konversi zona (`DateTimeKind.Unspecified`) lalu ditulis ke kolom `timestamptz` | Npgsql menolak, endpoint gagal 500. Kalaupun lolos, jamnya bergeser 7 jam karena jam dinding WIB dibaca sebagai UTC |
| Batas 3 hari ditulis sebagai angka di kode dan dihitung terhadap tanggal UTC | Setelan `RetroactiveCorrectionWindowDays` diabaikan; antara pukul 00.00 dan 07.00 WIB batasnya meleset satu hari |
| Hasil hitung ulang kehadiran diabaikan; kegagalan hanya masuk log | Rekap tanggal itu tetap memakai shift lama tanpa tanda apa pun, tetapi respons tetap berkata "evaluasi ulang dijalankan" |
| Tidak ada pemeriksaan alasan dan tidak ada perlindungan tabrakan | Alasan berisi spasi saja lolos; dua koreksi bersamaan saling menimpa diam-diam |

---

## 2. Proses bisnis

**Contoh (`HRD-DEC-217`).** Pada 16 Februari 2027 Sari mengoreksi jadwal Andi tanggal 14 Februari
dari Malam 21.00–07.00 menjadi Siang 14.00–21.00 dengan alasan "Salah catat".

| Langkah | Ditolak bila | Kode |
| --- | --- | --- |
| Cakupan unit (`BE-HRD-336`) | Unit di luar cakupan kelola | 403 |
| Periode terkunci (`HRD-DEC-102`) | Roster `Closed`/terkunci | 403 |
| Alasan (`VR-PK-07`) | Kategori atau catatan kosong; kategori lebih dari 50 karakter | 400 |
| Batas hari (`HRD-DEC-102`, tetap berlaku menurut `HRD-DEC-217` butir 4) | Tanggal shift lebih tua dari `RetroactiveCorrectionWindowDays` (setelan, devYoga = 3) terhadap tanggal operasional WIB | 422 |
| Simpan (satu transaksi) | Shift di basis data sudah berubah sesaat sebelum disimpan | 409 |

Penyimpanan dalam satu transaksi: pembaruan bersyarat shift ("hanya bila shift dan jamnya masih
Malam 21.00–07.00"), lalu satu baris Riwayat Revisi `RetroactiveCorrection` dengan shift dan jam
sebelum serta sesudah, kategori, catatan, pelaku, dan waktu. Lalu commit. Roster tetap `Published`
dengan versi yang sama.

Sesudah commit, kehadiran Andi 14 Februari dihitung ulang terhadap shift Siang (`INT-HR-101`).
Bila hitung ulang gagal, tidak tersedia, atau berakhir `Error`, koreksi tetap berlaku dan rekap
tanggal itu ditandai `ReprocessRequired` (35.10); respons menyatakannya dengan
`AttendanceReevaluationTriggered = false`. Bila periode kehadiran sudah ditutup, rekap tidak disentuh.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `00-interview-decisions.md` `HRD-DEC-217`, `HRD-DEC-102`; `02-backend-architecture.md` 35.9.3, 35.10; `api-contract.md` 32; `state-transition-matrix.md` (status pemrosesan); `acceptance-test-matrix.md` `AT-HRD-PK-07`
- `ShiftReplacementService.cs`, `ShiftRevisionRecorder.cs`, `RosterSchedulingDtos.cs`, `ShiftReplacementController.cs`
- `AttendanceScheduleMutationConsumer.cs`, `AttendanceProcessingService.cs` (konstruktor, jalur `ReprocessDailyAsync`/`ProcessSingleAsync`), `HrdAttendanceDaily.cs`, `LeaveExecutionProcessorService.cs:1146` (pola penanda `ReprocessRequired`)
- `SchedulingSettingService.cs`, `HrdSchedulingSetting.cs` (`RetroactiveCorrectionWindowDays`), `Helpers/AppDateTimeHelper.cs`, `RosterPeriodService.cs:372-375` (konversi jam saat tambah shift)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/SchedulingManagement/Services/ShiftReplacementService.cs` | `RetroactiveCorrectionAsync` ditulis ulang sesuai bagian 2. Metode baru `MarkAttendanceReprocessRequiredAsync`. Konstruktor menerima `ISchedulingSettingService` (sudah terdaftar di DI) |
| `Areas/Corporate/HumanResource/SchedulingManagement/DTOs/RosterSchedulingDtos.cs` | `RetroactiveShiftCorrectionRequest.ReasonCategory` maksimal 50 (sama dengan kolom riwayat). `RetroactiveShiftCorrectionResponse.RevisionId` (aditif) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sesuai `v26`: request tetap, respons `+ RevisionId`. Delta yang dicatat: `ReasonCategory` diperketat 100 → 50 karakter, sama seperti Ganti Petugas di `BE-HRD-338`; respons 409 baru untuk tabrakan; `AttendanceReevaluationTriggered` kini mencerminkan hasil sebenarnya (sebelumnya selalu `true`) |
| Database | Tanpa migration. Menulis `HrdShiftAssignmentRevision` (tabel `A52`, sudah diterapkan di devYoga) dan dapat menandai `HrdAttendanceDaily.ProcessingStatus = ReprocessRequired` |
| Keamanan/Auth | Tidak berubah. Catatan alasan tidak ditulis ke log; ke rekap kehadiran hanya kategori alasan yang diteruskan (sebelumnya catatan lengkap) |

---

## 4. Dokumentasi endpoint

### HR Scheduling - Shift Assignment & Replacement

Base URL: `api/v1/corporate/human-resource/scheduling-management/shift-replacements`

| Method | Path | Hak akses | Request | Response | Kode status |
| --- | --- | --- | --- | --- | --- |
| `POST` (juga `PUT`) | `/{id}/retroactive-correct` | `ShiftReplacement : Update` | `RetroactiveShiftCorrectionRequest` | `RetroactiveShiftCorrectionResponse` | 200, 400, 403, 404, 409, 422 |

Contoh request:

```json
{ "newShiftId": "…Siang…", "reasonCategory": "Salah catat", "notes": "Tertukar dengan jadwal Siang" }
```

Contoh respons 200 (hitung ulang berhasil):

```json
{
  "shiftAssignmentId": "…", "isRetroactive": true, "daysBack": 2,
  "attendanceReevaluationTriggered": true, "revisionId": "…",
  "message": "Jadwal shift tanggal 2026-11-11 berhasil dikoreksi ke 'Siang (UJI-A52)' dan kehadiran dihitung ulang."
}
```

Bila hitung ulang gagal, `attendanceReevaluationTriggered` bernilai `false` dan pesannya berakhir
"Hitung ulang kehadiran belum berhasil; rekap tanggal itu ditandai perlu diproses ulang."

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 galat, 603 peringatan (tetap) | `PASS` | Keluaran build |
| Harness xUnit SQLite di memori (45 uji: 35 sebelumnya + 10 baru) | 45/45 lulus | `PASS` | `dotnet test` di `C:\Users\Admin\AppData\Local\Temp\claude\d823ad68bb5a8f68\5d4a0fb6-9e9b-47eb-9814-7cfdf33f8519\scratchpad\scopetest` |
| — Contoh `HRD-DEC-217`: satu riwayat `RetroactiveCorrection` dengan shift dan jam lama/baru (jam dibandingkan dengan perhitungan WIB → UTC independen), alasan, pelaku; shift tersimpan Siang 420 menit; roster tetap `Published` versi 3 | Sesuai | `PASS` | `Koreksi_mundur_menulis_satu_riwayat…` |
| — Hitung ulang tidak tersedia / melempar galat → koreksi tetap tersimpan, rekap `ReprocessRequired`, pesan lama rekap dipertahankan | Sesuai | `PASS` | Dua uji |
| — Batas hari dibaca dari setelan: jendela 1 hari → 422; kontrol jendela 2 hari → lolos | Sesuai | `PASS` | `Lewat_batas_hari…` |
| — `VR-PK-07` (dua kasus), kategori 51 karakter → 400 | Sesuai | `PASS` | Tiga uji |
| — Shift berubah di antara baca dan simpan (disela lewat interceptor) → 409, tanpa riwayat | Sesuai | `PASS` | `Shift_berubah…` |
| — `INV-A52-01`: riwayat gagal ditulis → koreksi ikut batal | Sesuai | `PASS` | `Riwayat_gagal_ditulis…` |
| — Roster `Closed` → 403 | Sesuai | `PASS` | `Periode_roster_ditutup…` |
| Kontrol negatif: riwayat sengaja diisi shift baru sebagai "sebelum", backend dibangun ulang | Tepat uji riwayat yang gagal (44/45). Kode dikembalikan (dibandingkan byte demi byte), dibangun ulang, 45/45 lagi | `PASS` | Keluaran `dotnet test` |
| `AT-HRD-PK-07` lewat API devYoga (13 November 2026; roster `ROST-202611-0003`, 2 dan 5 hari ke belakang): lewat batas | 8 Nov (5 hari) → 422 "… maksimal 3 hari …", tanpa riwayat | `PASS` | `at-log.jsonl` |
| — Kategori alasan berisi spasi | 400, tanpa riwayat | `PASS` | `at-log.jsonl` |
| — Koreksi Malam → Siang (pegawai tanpa akun) | 200, `revisionId` terisi; riwayat: Malam 14.00Z–00.00Z → Siang 07.00Z–14.00Z (= 21.00–07.00 → 14.00–21.00 WIB); shift tersimpan Siang 420 menit; roster tetap `Published` versi 1. Hitung ulang ditolak layanan kehadiran (profil tanpa akun) dan tidak ada rekap yang dapat ditandai; respons menyatakannya | `PASS` | `at-log.jsonl` |
| — Jalur hitung ulang **berhasil** (Nadia Putri, berakun; roster `ROST-202611-0004` langsung terbit): rekap diproses dulu (Malam, `PublishedRoster`), lalu koreksi ke Siang | 200, `attendanceReevaluationTriggered = true`; rekap pindah ke Siang (jadwal masuk/pulang 07.00Z/14.00Z), `ProcessingVersion` 5 → 6, `Processed` | `PASS` | `at-log.jsonl` |
| — Jalur hitung ulang **gagal nyata** (shift Nadia di roster yang sudah turun ke `Draft`, jadwal tak terselesaikan) | 200, `attendanceReevaluationTriggered = false`; rekap `Error` → `ReprocessRequired`, pesan "Schedule belum terselesaikan" dipertahankan | `PASS` | `at-log.jsonl` |
| Log backend selama uji | Tanpa galat selain `AttendanceScheduler:SystemActorUserId` yang sudah ada | `PASS` | `be.log` sesi |

Catatan harness: SQLite memulangkan `DateTime` tanpa Kind, sehingga pembanding jam memakai
`SpecifyKind(Utc)`. Jalur hitung ulang yang berhasil tidak dapat dibentuk di harness (rantai
`AttendanceProcessingService` → resolver jadwal), jadi dibuktikan di devYoga.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Riwayat `RetroactiveCorrection` berisi nilai lama/baru | Terpenuhi | Harness dan API devYoga |
| Batas hari tetap | Terpenuhi | 422 di harness dan API; kini dibaca dari setelan dan tanggal WIB |
| Kegagalan hitung ulang menandai `ReprocessRequired` | Terpenuhi | Harness (tidak tersedia, galat) dan API devYoga (berakhir `Error`) |
| `AT-HRD-PK-07` | Terpenuhi | Bagian 5 |
| DoD umum: build lulus, tanpa angka tertanam, catatan alasan tidak masuk log | Terpenuhi | Angka 3 diganti setelan `RetroactiveCorrectionWindowDays` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan baru |
| Masalah yang diketahui | (1) **Terbit ulang roster gagal 500** (`IX_TrxRosterPublication_RosterPeriodId_VersionNumber`): tambah shift menurunkan roster terbit ke `Draft` tanpa menaikkan versi, lalu terbit ulang mencoba membuat publikasi kedua dengan versi yang sama. Roster `ROST-202611-0003` di devYoga tertahan `Draft` karenanya. Ini cakupan `BE-HRD-340`. (2) **Tambah penempatan kedua dengan departemen/jabatan yang sama untuk pegawai berakun gagal 500** sesudah penempatannya tersimpan: sinkronisasi proyeksi otorisasi bentrok indeks unik `AspNetUserOrganization`. Temuan WorkforceCore di luar cakupan, perlu diteruskan ke pemiliknya. (3) Koreksi mundur tidak menjalankan pemeriksaan `B1`–`B8` terhadap shift baru; kontrak dan `INV-A52-03` hanya mewajibkannya untuk Ganti Petugas dan tambah shift |
| Risiko tersisa | (1) Koreksi untuk tanggal masa depan tetap diizinkan seperti perilaku lama (batas hanya ke belakang). (2) Pegawai tanpa rekap pada tanggal itu tidak dapat ditandai bila hitung ulang gagal; respons tetap menyatakan kegagalannya. (3) Kategori alasan 51–100 karakter kini ditolak 400 |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 2 berkas source task ini (diubah); laporan ini; 2 dokumen roadmap/traceability. Migration `BE-HRD-FIX-001` dari pekerjaan sebelumnya masih untracked. Tidak ada stage atau commit |
| Langkah berikutnya | `BE-HRD-340` — tambah shift tanpa turun ke `Draft`, hapus shift terbit, endpoint Riwayat Revisi (sekaligus menutup cacat terbit ulang di butir Masalah 1) |

Data uji devYoga baru (bertanda `UJI-A52`): shift master "Siang (UJI-A52)"; roster
`ROST-202611-0003` (ICU, November 2026, kini `Draft`) dan `ROST-202611-0004` (ICU, 11 November 2026,
`Published`); penempatan `Secondary` ICU untuk Nadia Putri (sama dengan departemen dan jabatan
utamanya, tanpa proyeksi hak akses baru); rekap kehadiran Nadia 11 November 2026.
