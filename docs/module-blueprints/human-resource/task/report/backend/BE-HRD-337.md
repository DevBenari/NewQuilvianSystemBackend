# Laporan Perubahan Backend — `BE-HRD-337`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-337` |
| Judul | `RosterValidationService`: `B3`, `B8`, `W6`, `W7`, kode `W2`/`W3`, `ValidateSingleShiftAsync` |
| Slice | Amandemen `A52` atas `S-B4` Penjadwalan — `EPIC HRD-110`, gelombang `MVP-67` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 24, Grafik B85 |
| Trace | `FR-PK-007`; `HRD-DEC-099`, `100`, `201`, `213`; `INV-A52-03`; `02-backend-architecture.md` 35.9.2; `validation-matrix.md` `VR-PK-01`, `-02`, `-04`; `integration-contract.md` `INT-HR-106`, `-107` |
| Contract version | `v26` — `approved` (pengguna, 12 November 2026) |
| Dependency | `BE-HRD-335` ✅ (konstanta `RosterRuleCode`); `BE-HRD-334` 🟡 (tabel `HrdStudyAssignment` — lihat 7) |
| Klasifikasi | `MEDIUM` — skor 6: satu repository (0), sekitar 20 berkas diperiksa (1), 4 berkas source (1), logika aturan bertingkat (2), respons validasi diperluas aditif (1), tanpa schema (0), tanpa keamanan (0), tanpa UI (0), ditambah 1 karena seluruh evaluator ditata ulang |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/Corporate/HumanResource/SchedulingManagement/{Services,DTOs}/`; dokumen roadmap/traceability/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `9cc8097c` (branch `yoga`) + perubahan `BE-HRD-335`/`336` yang belum di-commit |
| Tanggal | 12 November 2026 |
| Status | ✅ Selesai (13 November 2026) — kode terbukti lewat harness dan lewat API devYoga (`AT-HRD-PK-11`–`14` lulus sesudah migration `BE-HRD-334` dan `BE-HRD-FIX-001` diterapkan) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `SchedulingManagement` (membaca `WorkforcePlanning`, `LearningAndDevelopment`, `AttendanceManagement`, `WorkforceCore`) |
| Pemilik / prefix registry | `Human Resource` / `Hrd` (`ACTIVE / LEGACY`) |
| Keberlakuan | `TOUCHED LEGACY` — `RosterValidationService`, `IRosterValidationService`, `RosterSchedulingDtos`, pesan di `RosterPeriodService` |
| QBE yang berlaku | `QBE-NAM-001`, `QBE-SVC-001` (aturan tetap di service). Tidak ada entity atau configuration baru |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; registry; `rules/backend/` |

---

## 1. Masalah yang diperbaiki

Pemeriksaan roster sebelumnya hanya mengenal `B1`, `B2`, `B4`–`B7`, dan `W1`–`W5`. Akibatnya:

- pegawai yang sedang **ditempatkan sementara di unit lain** atau **tidak punya penempatan** di
  unit roster tetap dapat dijadwalkan (`B3` belum ada);
- pegawai yang sedang **tugas belajar** tetap dapat dijadwalkan (`B8` belum ada);
- roster dapat terbit dengan **jumlah tenaga di bawah standar** atau **tanpa perawat
  berkompetensi** tanpa peringatan apa pun (`W6`, `W7` belum ada);
- peringatan `W2`/`W3` hanya berupa teks, tanpa kode aturan dan tanpa butir, sehingga layar
  tidak dapat menautkannya ke shift tertentu;
- Ganti Petugas tidak punya cara menjalankan pemeriksaan yang sama dengan terbit
  (`INV-A52-03`); `ValidateSingleShiftAsync` lama hanya membaca kesiapan kerja satu shift yang
  sudah tersimpan.

---

## 2. Proses bisnis

| Kode | Jenis | Contoh |
| --- | --- | --- |
| `B3` | Blokir | Tono penempatan utama ICU, sementara di HCU 1–14 Maret 2027. Shift ICU 5 Maret → "Tono ditempatkan di HCU 1–14 Maret 2027." Shift ICU 15 Maret lolos |
| `B8` | Blokir, tidak dapat diturunkan | Dewi tugas belajar 1 Agt 2027 – 31 Jul 2029. Shift 3 Agt 2027 → "Dewi sedang tugas belajar 1 Agustus 2027 – 31 Juli 2029." Tugas belajar berstatus `Cancelled` tidak dihitung |
| `W6` | Peringatan | Standar ICU aktif: 3 perawat per shift. Malam 10 Feb berisi 2 → "W6: Shift Malam 10-02-2027 di ICU berisi 2 perawat, kurang dari kuota 3 per shift." |
| `W7` | Peringatan | Aturan skill-mix "1 ICU Mahir per shift". Pagi 12 Feb hanya Ani (penilaian gagal) dan Budi → peringatan. Pagi 11 Feb ada Citra (lulus) → aman |
| Keterangan | Bukan peringatan | Unit HCU tanpa standar aktif → `InfoMessages` berisi "Kuota unit belum ditetapkan"; `W6`/`W7` tidak dievaluasi. Kompetensi tanpa satu pun data penilaian → "Data kompetensi ECMO belum tersedia; komposisi keahlian (W7) tidak diperiksa" |

Peringatan `W6`/`W7` ikut dihitung di `WarningConflicts`. Karena itu, aturan terbit yang sudah
ada otomatis meminta alasan untuk setiap peringatan (`VR-PK-04`). Keterangan tidak dihitung dan
tidak butuh alasan.

**Pemeriksaan satu shift** (`ValidateSingleShiftAsync`) menilai shift calon tanpa menyimpan
apa pun, dengan evaluator yang sama:

- shift calon dibandingkan dengan shift lain milik pegawai yang sama (`B1`, `W1` ke shift
  sebelum dan sesudahnya, `W2`, `W3`);
- komposisi slotnya dinilai dengan petugas lama dikeluarkan (`B7`, `W6`, `W7`).

Contoh: Citra (ICU Mahir) di Pagi 11 Feb diganti Eko. Jumlah slot tetap 3 orang, jadi `W6`
aman, tetapi `W7` muncul karena tidak ada lagi perawat ICU Mahir. `BE-HRD-338` (Ganti Petugas)
akan memanggil pemeriksaan ini.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `02-backend-architecture.md` 35.9.2, 35.8; `HRD-DEC-213`; `validation-matrix.md` `VR-PK-01`–`05`; `acceptance-test-matrix.md` `AT-HRD-PK-11`–`14`; `integration-contract.md` `INT-HR-106`/`107`
- `HrdStaffingStandard.cs` (beserta `Line`, `SkillMixRule`), `StaffingStandardStatus`, `WfpCompetencyAssessment.cs`, `CompetencyLevel`, `CompetencyAssessmentResultStatus`, `HrdStudyAssignment.cs`, `WfpOrganizationAssignment.cs` (`AssignmentType`), `WfpLeaveRequest.cs`, `TrxRosterAssignment.cs`, `TrxShiftAssignment.cs`
- `RosterPeriodService.PublishRosterPeriodAsync` (penghitungan alasan peringatan), `SchedulingSettingResponse`, `SchedulingEligibilityResponse`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/SchedulingManagement/Services/RosterValidationService.cs` | Ditata ulang menjadi satu evaluator: `EvaluateShiftAsync` (`B1`, `B2`, `W4`, `B3`, `B8`, `B4`–`B6`, `W5`, `W1`), `EvaluateStreaks` (`W2`, `W3`), `EvaluateSlot` (`B7`, `W6`, `W7`). Data cuti, penempatan, tugas belajar, standar, dan kompetensi dimuat **sekali per pemeriksaan**, bukan satu kueri per shift. `ValidateSingleShiftAsync` baru |
| `Areas/Corporate/HumanResource/SchedulingManagement/Services/IRosterValidationService.cs` | Tanda tangan `ValidateSingleShiftAsync(RosterSingleShiftValidationRequest)` → `RosterSingleShiftValidationResult?` (yang lama belum dipanggil siapa pun) |
| `Areas/Corporate/HumanResource/SchedulingManagement/DTOs/RosterSchedulingDtos.cs` | `RosterValidationResultResponse.InfoMessages` (aditif); `RosterSingleShiftValidationRequest`, `RosterSingleShiftValidationResult` (internal service) |
| `Areas/Corporate/HumanResource/SchedulingManagement/Services/RosterPeriodService.cs` | Teks pesan terbit "B1–B7"/"W1–W5" menjadi "B1–B8"/"W1–W7" |

Perilaku lama yang dipertahankan: teks pesan `B1`–`B7`/`W1`–`W5`, cara menghitung konflik,
penyimpanan `ValidationSummaryJson`, penandaan `IsAccompanied`, dan eskalasi `W1`/`W2`/`W3`
menjadi blokir lewat setelan.

Perubahan kecil yang disengaja:

- `W2`/`W3` kini juga muncul sebagai butir `ConflictItems` berkode;
- kegagalan pembacaan kesiapan karena galat kini juga menghasilkan butir `B6`, bukan hanya teks;
- `IsValidationPassed` per shift kini dihitung sesudah **seluruh** aturan, sehingga shift yang
  terkena eskalasi `W2`/`W3` tidak lagi tertulis lolos.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `POST .../roster-periods/{id}/validate` dan `/publish`: kode aturan baru `B3`, `B8`, `W6`, `W7` pada `conflictItems`; `W2`/`W3` kini berbutir; field baru `infoMessages`. **Selisih kontrak:** `v26` belum menetapkan bentuk "keterangan" `W6`/`W7`; `infoMessages` dipilih karena aditif. Dicatat untuk pass blueprint berikutnya |
| Database | Tanpa migration. Kueri baca baru ke `HrdStudyAssignment` (tabel `A52`), `WfpOrganizationAssignment`, `HrdStaffingStandard*`, `WfpCompetencyAssessment` |
| Keamanan/Auth | Tidak berubah |

---

## 4. Dokumentasi endpoint

### HR Scheduling - Roster Period

Base URL: `api/v1/corporate/human-resource/scheduling-management/roster-periods`

| Method | Path | Hak akses | Perubahan respons |
| --- | --- | --- | --- |
| `POST` | `/{id}/validate` | `RosterPeriod : Update` | `conflictItems[].ruleCode` dapat bernilai `B3`, `B8`, `W2`, `W3`, `W6`, `W7`. Untuk `W6`/`W7`, `workforceProfileId` bernilai Guid nol dan `staffName` kosong (temuan tingkat slot). Field baru `infoMessages: string[]` |
| `POST` | `/{id}/publish` | `RosterPeriod : Update` | Ditolak 422 bila ada `B3`/`B8`. Alasan wajib untuk setiap peringatan termasuk `W6`/`W7` |

Contoh potongan respons:

```json
{
  "isValidationPassed": false,
  "blockingConflicts": 1,
  "warningConflicts": 2,
  "infoMessages": ["Data kompetensi ECMO belum tersedia; komposisi keahlian (W7) tidak diperiksa"],
  "conflictItems": [
    { "ruleCode": "B3", "severity": "BLOCKING", "staffName": "Tono", "shiftDate": "2027-03-05",
      "message": "VR-PK-01 (B3): Tono ditempatkan di HCU 1–14 Maret 2027." },
    { "ruleCode": "W6", "severity": "WARNING", "staffName": "", "shiftDate": "2027-02-10",
      "message": "W6: Shift Malam 10-02-2027 di ICU berisi 2 perawat, kurang dari kuota 3 per shift." }
  ]
}
```

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 galat, 603 peringatan (tetap). Peringatan XML di `RosterSchedulingDtos.cs:439/459` berasal dari baris lama (`f2100b2c9`) | `PASS` | Keluaran build |
| Harness xUnit SQLite di memori (24 uji: 15 dari `BE-HRD-336` + 9 baru) | 24/24 lulus | `PASS` | `dotnet test` di `C:\Users\Admin\AppData\Local\Temp\claude\d823ad68bb5a8f68\ef24ad71-9cc8-48c3-b37f-ddd8b9cac1bd\scratchpad\scopetest` |
| — `AT-HRD-PK-11` (`B3`): Tono 5 Maret ditolak dengan pesan persis; 15 Maret lolos; status per shift tersimpan | Sesuai | `PASS` | `B3_penempatan_sementara...` |
| — `AT-HRD-PK-12` (`B8`): Dewi diblokir dengan pesan persis; tugas belajar `Cancelled` tidak dihitung | Sesuai | `PASS` | `B8_tugas_belajar...` |
| — `AT-HRD-PK-13` (`W6`/`W7`): peringatan pada slot kurang; slot lengkap aman; dihitung sebagai peringatan, bukan blokir | Sesuai | `PASS` | `W6_dan_W7...` |
| — `AT-HRD-PK-14`: unit tanpa standar aktif (standar draf diabaikan) → tanpa `W6`, keterangan "Kuota unit belum ditetapkan", roster lolos | Sesuai | `PASS` | `Tanpa_standar_aktif...` |
| — `W2` berkode; `B6` gagal tertutup tetap berlaku | Sesuai | `PASS` | Dua uji |
| — Satu shift: `B3`/`B8` sama dengan terbit; Ganti Petugas menilai slot tanpa petugas lama (`W7`); perpanjangan rangkaian → `W2` pada calon; `B1` tabrakan; periode tak ada → null; tidak menyimpan apa pun | Sesuai | `PASS` | Tiga uji |
| Kontrol negatif: logika "penempatan sementara mengalahkan utama" dirusak sementara, backend dibangun ulang, harness dijalankan | Tepat 2 uji `B3` gagal; kode dikembalikan (dibandingkan byte demi byte), dibangun ulang, 24/24 lulus lagi | `PASS` | Keluaran `dotnet test` |
| `AT-HRD-PK-14` lewat API devYoga (13 November 2026), sebelum standar ICU ada: `POST roster-periods/{id}/validate` | 200; 0 blokir, 0 peringatan, 0 `W6`; `infoMessages` = "Kuota unit belum ditetapkan" | `PASS` | `at-log.jsonl` |
| `AT-HRD-PK-13` lewat API devYoga: standar ICU 3 Perawat per shift (April 2027), Malam 10 April berisi 2 | Validasi: 1 `W6` WARNING dengan pesan "… berisi 2 Perawat Rawat Inap, kurang dari kuota 3 per shift". Terbit tanpa alasan → 422 "… seluruh 1 peringatan … Diterima 0 alasan". Terbit dengan alasan `W6` → 200, periode `Published` | `PASS` | `at-log.jsonl` |
| `AT-HRD-PK-11` lewat API devYoga: Tono `Temporary` IGD 1–14 Maret 2027 (IGD menggantikan HCU yang tidak ada di devYoga), Ganti Petugas ke Tono | 5 Maret → 422, `errors` = `B3/BLOCKING`, pesan "VR-PK-01 (B3): Raka Firmansyah ditempatkan di Instalasi Gawat Darurat 1–14 Maret 2027."; tanpa riwayat. 15 Maret → 200, satu riwayat Indra → Tono | `PASS` | `at-log.jsonl` |
| `AT-HRD-PK-12` lewat API devYoga: Dewi tugas belajar 1 Agustus – 31 Desember 2027, Ganti Petugas 3 Agustus | 422, `errors` = `B8/BLOCKING`, pesan "VR-PK-02 (B8): Putri Handayani sedang tugas belajar 1 Agustus – 31 Desember 2027."; tanpa riwayat | `PASS` | `at-log.jsonl` |

Data uji devYoga bertanda `UJI-A52`: roster `ROST-202611-0002` (ICU, 1 Februari – 31 Agustus 2027);
shift master "Pagi (UJI-A52)" dan "Malam (UJI-A52)"; penempatan `Secondary` ICU untuk lima profil
tanpa akun login (Bagas Saputra = Budi, Raka Firmansyah = Tono, Arif Maulana = Andi, Indra Setiawan,
Putri Handayani = Dewi) ditambah `Temporary` IGD untuk Tono; standar `UJI-A52-STD-ICU` versi 901;
tugas belajar `UJI-A52/SK-TB/2027`. Standar dan tugas belajar disisipkan langsung ke database karena
standar aktif hanya lewat workflow dan tugas belajar belum punya endpoint (`BE-HRD-34x`). Uji API ini
semula terhadang 500 pada tambah shift roster; penyebabnya diperbaiki di [BE-HRD-FIX-001](BE-HRD-FIX-001.md).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Penempatan `Temporary` di unit lain mengalahkan penempatan utama (`B3`) | Terpenuhi | Harness dan API devYoga (`AT-HRD-PK-11`) |
| `B8` tidak dapat diturunkan | Terpenuhi | Selalu `BLOCKING`; tidak membaca setelan eskalasi; API devYoga (`AT-HRD-PK-12`) |
| Tanpa standar aktif, `W6`/`W7` memberi keterangan, bukan peringatan | Terpenuhi | Harness dan API devYoga (`AT-HRD-PK-14`) |
| `AT-HRD-PK-11`–`14` lewat API | Terpenuhi | Bagian 5, 13 November 2026 |
| DoD umum: build lulus, tanpa angka tertanam, tanpa kolom sensitif di log | Terpenuhi | Batas kuota dibaca dari standar ketenagaan; batas `W1`–`W3` dari setelan |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan baru |
| Masalah yang diketahui | (1) **Urutan pemasangan wajib dijaga.** Validasi dan terbit roster kini membaca tabel `HrdStudyAssignment`. Di database yang belum menerapkan migration `BE-HRD-334`, keduanya gagal dengan galat 500. Perubahan ini jangan dipasang di server mana pun sebelum migration `A52` diterapkan. (2) `B3` menolak pegawai yang tidak punya penempatan aktif di unit roster. Pegawai yang masuk roster lewat jalur cadangan `PopulateEligibleStaff` (berdasarkan `PrimaryDepartmentId`, tanpa `WfpOrganizationAssignment`) kini terblokir sampai penempatannya dicatat. Periode tanpa unit tidak diperiksa `B3` |
| Risiko tersisa | (1) `W2`/`W3` lama menghitung **shift** berurutan, bukan hari kalender: celah tanggal tanpa baris libur tidak memutus rangkaian. Perilaku ini dipertahankan dan juga berlaku pada pemeriksaan satu shift. (2) `W6` memakai `StaffPerShift`, atau `MinimumHeadcount` bila kosong, sesuai desain. Slot shift yang sama sekali tidak berisi pegawai tidak terdeteksi. (3) `W7` menghitung penilaian `Passed` yang belum kedaluwarsa dan memenuhi level minimum; `Waived` tidak dihitung; filter profesi pada aturan skill-mix belum diterapkan. (4) "Data kompetensi tidak tersedia" diartikan sebagai tidak ada satu pun penilaian aktif untuk kompetensi itu |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 4 berkas source task ini (semua diubah) di atas perubahan `BE-HRD-335`/`336` yang belum di-commit; 2 dokumen roadmap/traceability; laporan ini. Tidak ada stage atau commit |
| Langkah berikutnya | (a) `AT-HRD-PK-09`/`-10` (milik `BE-HRD-336`) masih butuh akun non-superadmin berpenempatan berjabatan. (b) `BE-HRD-338` — `ShiftRevisionRecorder` dan perbaikan Ganti Petugas (memakai `ValidateSingleShiftAsync`) |
