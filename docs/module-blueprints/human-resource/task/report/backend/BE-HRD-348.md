# Laporan Perubahan Backend — `BE-HRD-348`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-348` |
| Judul | Jadwal Saya: jadwal sendiri dan roster unit |
| Slice | Amandemen `A52` — `EPIC HRD-113`, gelombang `MVP-68` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 24, Grafik B87 |
| Trace | `FR-PK-016`; `HRD-DEC-209`, `HRD-ASM-80` (dengan `HRD-DEC-195`, `HRD-DEC-210`, `HRD-DEC-213`); `02-backend-architecture.md` 35.3.4, 35.4.2; `api-contract.md` "Self Services / Human Resource / My Schedule"; `validation-matrix.md` `VR-PK-29`; `permission-audit-matrix.md` 30.1 (`MySchedule : Read`); `AT-HRD-PK-42`, `-43`, `-44` |
| Contract version | `v26` — `approved` (pengguna, 12 November 2026) |
| Dependency | `BE-HRD-343` ✅ (status hari libur hanya jadwal tetap), `BE-HRD-338` 🟡 (riwayat revisi shift — bagian yang dipakai, `ShiftRevisionRecorder`, sudah ada) |
| Klasifikasi | `STANDARD` — skor 6: satu repository (0), sekitar 18 berkas diperiksa (2), 3 berkas source baru + `Program.cs` (1), baca lintas banyak sumber (2), hak akses baru dan privasi rekan (1), tanpa schema (0), tanpa UI (0) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/SelfServices/HumanResource/{Services/MyScheduleSelfService.cs,Controllers/MyScheduleSelfServiceController.cs,DTOs/MyScheduleSelfServiceDtos.cs}`, `Program.cs` (satu registrasi DI); dokumen roadmap/traceability/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `13b5d76b` (branch `yoga`) + perubahan `BE-HRD-344` s.d. `347` yang belum di-commit |
| Tanggal | 16 November 2026 |
| Status | ✅ Selesai — harness 107/107 (termasuk kontrol negatif); `AT-HRD-PK-42`, `-43`, `-44` lulus lewat API devYoga dengan akun pegawai sungguhan |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `SelfServices` |
| Module / Submodule | `HumanResource` (self-service) |
| Pemilik / prefix registry | `Corporate / SelfServices` — `Human Resource` / `Hrd` (`ACTIVE / LEGACY`); tidak ada entity baru |
| Keberlakuan | `NEW CODE` (service, controller, DTO baru), hanya baca |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001` |
| Tidak berlaku | `QBE-LOG-001` (tidak ada perubahan state; matriks 30.1 kolom log "Tidak"), `QBE-CODE-*`, `QBE-TXN-001` |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

---

## 1. Masalah yang diperbaiki

| Cacat sebelum task ini | Akibat |
| --- | --- |
| Tidak ada API Jadwal Saya; fungsi frontend `getMyScheduleAssignments` melempar galat (`HRD-TF-062`) | Pegawai tidak dapat melihat jadwal terbitnya sendiri maupun roster unitnya |

---

## 2. Proses bisnis

**Jadwal sendiri (`HRD-DEC-209` butir 1–3, 6–7).** Gilang membuka Jadwal Saya 17–18 Nov 2026. Server mengambil identitas dari
sesi — tidak ada parameter pegawai; parameter tambahan apa pun diabaikan. Per tanggal tampil:

| Isi | Sumber |
| --- | --- |
| Shift, jam, unit, kode roster | `TrxShiftAssignment` dari roster berstatus `Published` atau `Closed` saja; roster `Draft`/`Validation` tidak pernah dibaca |
| Tanda "Berubah" + shift dan jam sebelumnya | Revisi terakhir `HrdShiftAssignmentRevision` atas shift itu (Ganti Petugas, koreksi mundur, tambah shift) |
| Shift yang dialihkan ke petugas lain atau dihapus | Revisi dengan petugas sebelumnya = pegawai ini, yang shift-nya kini bukan miliknya |
| Siaga | `TrxOnCallAssignment` pada roster terbit |
| Jadwal tetap, hari libur, hari istirahat | `AttendanceScheduleResolverService` (sumber `FixedWorkSchedule`); pegawai jadwal tetap pada hari libur berstatus "Hari libur" (`HRD-DEC-210`) |
| Status hari Cuti / Sakit / Dinas Luar / Tugas Belajar | Cuti disetujui (kategori `Sick` → Sakit), dinas luar `Approved`, tugas belajar `Active`; jenis cutinya ditampilkan untuk diri sendiri |

**Roster unit (`HRD-DEC-209` butir 4–5).** Unit yang terlihat: unit penempatan utama ditambah unit yang roster terbitnya
memuat shift pegawai itu pada bulan tersebut (penempatan sementara, `HRD-DEC-213`). Per rekan hanya nama dan kode/nama
shift. Hari rekan yang cuti, sakit, dinas luar, atau tugas belajar **dihilangkan** dari daftar, sehingga tampil sama
dengan hari libur biasa — alasannya tidak pernah dikirim.

**Rentang (`HRD-ASM-80`, `VR-PK-29`).** 3 bulan ke belakang sampai akhir bulan depan (WIB). Di luar itu `400` "Jadwal
yang dapat dilihat: 3 bulan ke belakang sampai bulan depan". Bulan depan yang rosternya belum terbit tampil kosong.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `00-interview-decisions.md` `HRD-DEC-209`, `HRD-ASM-80`; `api-contract.md` My Schedule; `validation-matrix.md` `VR-PK-29`; `acceptance-test-matrix.md` `AT-HRD-PK-42`–`44`; `02-backend-architecture.md` 35.3.4
- `HumanResourceContextService.cs`, `HumanResourceUserContextDto`; `LeaveCalendarSelfServiceController.cs`, `ShiftSwapSelfServiceController.cs` (pola self-service, modul `HUMAN_RESOURCE_EMPLOYEE_SELF_SERVICE`)
- `AttendanceScheduleResolverService.cs` (`ResolveRangeAsync` maks. 31 hari, status shift yang diterima tanpa memeriksa status roster)
- Model `TrxShiftAssignment`, `TrxRosterAssignment`, `TrxRosterPeriod` (status `Draft`/`Validation`/`Published`/`Closed`), `HrdShiftAssignmentRevision`, `TrxOnCallAssignment`, `MstOnCallType`, `WfpLeaveRequest`, `MstLeaveType`, `HrdOfficialDutyRequest`, `HrdStudyAssignment`, `MstShift`; `SchedulingRequestValueConstants.ShiftRevisionChangeType`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/SelfServices/HumanResource/Services/MyScheduleSelfService.cs` | **Baru.** `GetMyScheduleAsync`, `GetMyUnitRosterAsync`; rentang `HRD-ASM-80`; identitas lewat `HumanResourceContextService.GetByUserIdAsync`; kueri roster terbit, revisi, shift yang dialihkan, siaga, ketidakhadiran; jadwal tetap/hari libur lewat resolver; `MyScheduleServiceResult<T>` |
| `Areas/SelfServices/HumanResource/Controllers/MyScheduleSelfServiceController.cs` | **Baru.** `GET /`, `GET /unit-roster`; `AccessController` `HUMAN_RESOURCE_EMPLOYEE_SELF_SERVICE` / `MySchedule`; `AccessPermission("MySchedule", "Read")` |
| `Areas/SelfServices/HumanResource/DTOs/MyScheduleSelfServiceDtos.cs` | **Baru.** `MyScheduleDayResponse` dan turunannya, `MyUnitRosterResponse`; query tanpa parameter pegawai |
| `Program.cs` | Satu baris `AddScoped<MyScheduleSelfService>()` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 2 endpoint sesuai kontrak |
| Database | Tanpa migration; hanya baca |
| Keamanan/Auth | Resource baru `MySchedule : Read`; belum diberikan ke jabatan mana pun di devYoga. Identitas dari sesi; rekan hanya nama + shift |
| Privasi | Alasan ketidakhadiran rekan, jenis cuti, dan data tugas belajar rekan tidak pernah dikirim (diuji dengan pencarian teks pada respons) |

### 3.4 Delta terhadap kontrak dan blueprint

| Butir | Implementasi | Alasan |
| --- | --- | --- |
| `HRD-DEC-209` butir 3: tukar shift yang disetujui bertanda "Berubah" | Belum: tanda "Berubah" berasal dari `HrdShiftAssignmentRevision`, dan persetujuan tukar shift belum menulis riwayat revisi | Di luar cakupan kartu; perlu task terpisah pada alur tukar shift |
| Kontrak: rentang dibatasi `HRD-ASM-80` | Ditambah batas teknis 31 hari per permintaan | Batas `ResolveRangeAsync`; layar bulanan cukup satu bulan per permintaan |
| `HRD-DEC-209` butir 2 menyebut "Izin" | Status hari: Cuti, Sakit, Dinas Luar, Tugas Belajar, Hari libur, Libur; jenis cuti ditampilkan untuk diri sendiri | Tidak ada kategori "Izin" yang jelas di master jenis cuti; tidak dibuat pemetaan karangan |
| "Roster unit sendiri" | Unit penempatan utama + unit roster terbit yang memuat shift pegawai | Penempatan sementara (`HRD-DEC-213`) dijadwalkan unit tujuan |

---

## 4. Dokumentasi endpoint

Base URL: `api/v1/self-services/human-resource/my-schedule` — Tag `Self Services / Human Resource / My Schedule`

| Method | Path | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | `MySchedule : Read` | Query `fromDate`, `toDate` (default bulan berjalan) | `List<MyScheduleDayResponse>` | `200`, `400`, `401`, `403`, `404` (akun tanpa profil pegawai) |
| `GET` | `/unit-roster` | `MySchedule : Read` | Query `month` (`yyyy-MM`, default bulan berjalan) | `MyUnitRosterResponse` | `200`, `400`, `401`, `403`, `404` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 galat, 603 peringatan (tetap; tidak ada peringatan dari berkas baru) | `PASS` | Keluaran build |
| Harness xUnit SQLite di memori (107 uji: 102 sebelumnya + 5 baru di `MyScheduleTests.cs`, "sekarang" = 2 Feb 2027 10.00 WIB) | 107/107 lulus | `PASS` | `dotnet test` di `C:\Users\Admin\AppData\Local\Temp\claude\d823ad68bb5a8f68\4222eeca-aa89-4652-b697-eb38df816c85\scratchpad\scopetest` |
| — `AT-HRD-PK-42` (contoh `HRD-DEC-209`): roster ICU Februari terbit, Maret Draft, roster IGD; Budi cuti 15 Feb | Unit hanya ICU; Tono "P" 15 Feb; Budi tidak muncul pada 15 Feb tetapi muncul 16 Feb; respons tidak memuat "Cuti", "Leave", alasan cuti, atau pegawai IGD; Maret kosong | `PASS` | `Roster_unit_hanya_terbit_tanpa_alasan_ketidakhadiran` |
| — Jadwal sendiri | Pagi 15 Feb (ICU, jam benar); Malam 18 Feb "Berubah — sebelumnya Siang"; 20 Feb shift dialihkan ke Tono tampil sebagai shift yang dilepas; siaga 21 Feb; shift di roster Draft 22 Feb tidak tampil; Tono hanya melihat shift miliknya | `PASS` | `Jadwal_sendiri_rinci_dengan_tanda_berubah` |
| — Status hari | Budi 15 Feb "Cuti" + "Cuti Tahunan"; Andi jadwal tetap: Jumat dinas dengan nama jadwal, 8 Feb Imlek "Hari libur" | `PASS` | `Status_hari_cuti_dan_jadwal_tetap` |
| — `AT-HRD-PK-43` | Akun tanpa profil pegawai 404; tanpa identitas 401; DTO query tidak punya properti pegawai/unit | `PASS` | `Identitas_dari_sesi` |
| — `AT-HRD-PK-44` | Enam bulan lalu, sehari sebelum batas, lewat bulan depan → 400 dengan pesan kontrak; batas tepat sah; > 31 hari dan selesai < mulai 400; bulan di luar batas dan format salah 400 | `PASS` | `Rentang_di_luar_batas_ditolak` |
| Kontrol negatif: roster `Draft` ikut dibaca dan hari ketidakhadiran rekan tidak disembunyikan, backend dibangun ulang | Tepat dua uji yang menjaganya gagal. Kode dipulihkan (dibandingkan byte demi byte), dibangun ulang, 107/107 lagi | `PASS` | Keluaran `dotnet test` |
| Startup Development dengan resource hak akses baru | Backend menyala, Swagger 200, tanpa galat registri/DI | `PASS` | `be348.log` |
| API devYoga (16 November 2026) — data uji `UJI-A52` disisip ke basis data: periode roster `UJI-A52-MYSCH` (ICU 17–18 Nov, `Published`) berisi Gilang Mahendra (Pagi 17, Malam 18) dan Raka Firmansyah (Pagi 17, Siang 18); tugas belajar Raka 17 Nov; revisi koreksi mundur shift Gilang 18 Nov (Siang → Malam) | Tersimpan | — | `at-348.mjs`, `at-state.json` di `C:\Users\Admin\AppData\Local\Temp\claude\d823ad68bb5a8f68\5d4a0fb6-9e9b-47eb-9814-7cfdf33f8519\scratchpad\mig` |
| — Akun `gilang.mahendra` sebelum izin | `GET /` dan `/unit-roster` `403` | `PASS` | `at-log.jsonl` (entri "348 …") |
| — Izin `MySchedule : Read` diberikan **sementara** ke Penunjang Medis / Analis Laboratorium lewat Role Access (set 32 butir dibaca dan disimpan, lalu dikirim ulang utuh + 1), atas persetujuan pengguna | `200` | — | `at-log.jsonl`; salinan set asal `at-348-policy-backup.json` |
| — `AT-HRD-PK-43`: `GET /?fromDate=2026-11-17&toDate=2026-11-18&workforceProfileId=<Raka>` | `200` berisi jadwal Gilang sendiri (Pagi 17 ICU; Malam 18 "Berubah", sebelumnya Siang); parameter Raka diabaikan | `PASS` | `at-log.jsonl` |
| — `AT-HRD-PK-42`: `GET /unit-roster?month=2026-11` | Unit ICU dari roster terbit `ROST-202611-0004`, `-0005`, `UJI-A52-MYSCH`; anggota Gilang (saya), Nadia, Raka; Raka 17 Nov tidak tampil (tugas belajar), 18 Nov Siang tampil; Arif yang hanya ada di roster Draft `ROST-202611-0003` tidak tampil; tidak ada kata "Tugas", "Study", nama institusi, atau nomor SK | `PASS` | `at-log.jsonl` |
| — `AT-HRD-PK-44` | Mei 2026 dan Januari 2027 → `400` dengan pesan kontrak | `PASS` | `at-log.jsonl` |
| — Superadmin (tanpa profil pegawai) / tanpa sesi | `404` "Akun ini belum terhubung ke profil pegawai…" / `401` | `PASS` | `at-log.jsonl` |
| — Izin dikembalikan | Set dikirim ulang persis; dibaca ulang identik (32 butir); Gilang kembali `403` | `PASS` | `at-log.jsonl` |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Identitas dari sesi | Terpenuhi | `AT-HRD-PK-43` harness dan devYoga |
| Roster `Draft` tidak tampil | Terpenuhi | Harness (Maret, roster IGD Draft) dan devYoga (Arif) |
| Rentang di luar `HRD-ASM-80` → 400 | Terpenuhi | `AT-HRD-PK-44` |
| Hari ketidakhadiran rekan tanpa alasan | Terpenuhi | Harness (cuti) dan devYoga (tugas belajar) |
| DoD umum | Terpenuhi | Bagian 3–5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan build baru |
| Masalah yang diketahui | (1) `MySchedule : Read` belum diberikan ke jabatan mana pun; agar seluruh pegawai bisa membuka Jadwal Saya, izin ini perlu diberikan lewat Role Access ke tiap departemen-jabatan. (2) Tukar shift yang disetujui belum bertanda "Berubah" (bagian 3.4). (3) Dugaan dari pembacaan kode, belum diuji: `AttendanceScheduleResolverService` menyaring shift hanya dari status shift (`Published`/`Confirmed`/`Completed`), tidak dari status roster, sehingga shift berstatus `Published` di roster `Draft` (seperti `ROST-202611-0003` di devYoga) bisa terbaca pemrosesan kehadiran. Jadwal Saya tidak terdampak karena memeriksa status roster sendiri |
| Risiko tersisa | Respons jadwal sendiri memanggil resolver per tanggal (paling banyak 31); untuk pegawai dengan banyak penugasan waktu respons bisa beberapa ratus milidetik |
| Perubahan sampingan | `NONE`. Izin Role Access devYoga yang diubah untuk uji sudah dikembalikan identik |
| Interupsi | `NONE` |
| Status Git | 3 berkas source baru + `Program.cs`; laporan ini (baru); 2 dokumen roadmap/traceability. Perubahan `BE-HRD-344` s.d. `347` sesi yang sama juga belum di-commit. Tidak ada commit |
| Langkah berikutnya | `BE-HRD-349` — laporan kehadiran (tujuh laporan) dengan cakupan unit |

Data uji devYoga baru (bertanda `UJI-A52`, disisip ke basis data): periode roster `UJI-A52-MYSCH` (ICU, 17–18 Nov 2026,
`Published`) dengan 2 penugasan roster dan 4 shift (Gilang Mahendra, Raka Firmansyah); tugas belajar Raka
`UJI-A52-SK-348` (17 Nov 2026, `Active`); satu revisi koreksi mundur shift Gilang 18 Nov. Catatan: tugas belajar Raka
ini juga dibaca modul lain yang memakai `HrdStudyAssignment` (misalnya pemeriksaan roster `B8`) untuk tanggal 17 Nov 2026.
