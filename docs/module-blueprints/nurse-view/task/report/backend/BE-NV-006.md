# Laporan Perubahan Backend — `BE-NV-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-NV-006` |
| Judul | Header perawat jaga dari roster |
| Slice | `MVP-2` — papan lantai |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/backend-roadmap.md` — kartu `BE-NV-006` |
| Trace | `FR-NV-017`; `NV-DEC-012`, `021`, `027`, `033`; `02-backend-architecture.md` §4.6 langkah 9; `TLM-NV-001` keperawatan permintaan 2 |
| Contract version | `0.1.2` — `approved` (Rizki Gunawan, 2026-11-03, `NV-DEC-033`); sebelumnya `0.1.1` |
| Dependency | `BE-NV-005` 🟡 (bagian papan yang dipakai task ini sudah terbukti) |
| Klasifikasi | `HEAVY` — mengubah satu baris di modul Clinical milik tim keperawatan (restu `NV-DEC-027`) dan membaca roster HR, tanpa migration |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/NurseViewManagement/Services/NvwFloorBoardQueryService.cs`; satu kata di `Areas/HealthServices/ClinicalManagement/Services/InpatientClinicalContextService.cs`; dokumen delta kontrak; `TLM-NV-001`; laporan; status roadmap |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6489c32c` (branch `rizkiG`) |
| Tanggal | 2026-11-03 |
| Status | 🟡 **SEBAGIAN 3 November 2026** — QBE Strict `PASS` (2 berkas), build Rizki lulus (10.53), uji papan 3/3 `PASS` (16 cek): roster kosong dan bangsal tanpa unit menjawab `RosterAvailable = false` tanpa nama, papan `BE-NV-005` tidak berubah. **`NV-AC-009` belum terbukti dengan data nyata**: modul HR gagal menyimpan penugasan shift (`500`, `DateTime` `Kind=Unspecified`), dan DB dev hanya punya satu pegawai berprofesi Perawat. Verifikasi developer, bukan UAT |

---

## 1. Masalah yang diselesaikan

Sejak `BE-NV-005`, setiap bangsal di papan selalu `RosterAvailable = false` dengan daftar perawat kosong, karena
langkah 9 (roster) memang ditunda ke task ini.

**Contoh.** Shift pagi Instalasi Rawat Inap dijaga Sari dan Dewi menurut roster HR. Sebelum task ini, kepala bangsal di
TV selalu "Roster shift ini belum tersedia". Sesudah task ini, begitu HR menerbitkan roster, `GET /board` mengisi
`OnDutyNurses` dengan Sari dan Dewi beserta label shiftnya, tanpa perubahan desain.

---

## 2. Proses bisnis

**Tujuan:** kepala setiap bangsal di TV menampilkan perawat dan bidan yang sedang jaga menurut roster HR, dan tidak
pernah menebak nama dari sumber lain.

**Pelaku:** TV (akun perangkat). Sumber data: roster HR (shift, penugasan, periode), data pegawai dan profesi, bangsal
Rawat Inap.

**Langkah normal:**

1. Bangsal di papan dipetakan ke unit organisasi lewat `MstServiceUnit.OrganizationUnitId`.
2. Sistem mengambil penugasan shift `Published` yang aktif, bukan libur, dan berjalan saat ini
   (`ScheduledStartAt ≤ sekarang < ScheduledEndAt`, UTC).
3. Unit penugasan diambil dari `TrxShiftAssignment.OrganizationUnitId`. Bila kosong, dipakai unit periode roster
   (`NV-DEC-033`).
4. Profesi diambil dari `TrxRosterAssignment.ProfessionId`. Bila kosong, dipakai profesi pegawai (`NV-DEC-033`).
   Hanya profesi klinis yang `MapCpptProfessionType`-nya `Nurse` atau `Midwife` yang lolos. Aturan ini sama dengan
   CPPT, karena metodenya dipakai ulang, bukan disalin.
5. Satu orang dengan dua shift berimpit hanya muncul sekali. Daftar diurutkan menurut nama. `ShiftLabel` berisi nama
   shift, atau jam "07.00–14.00" waktu Asia/Jakarta bila nama shift kosong.

**Jalur tidak normal:**

| Kejadian | Jawaban sistem |
| --- | --- |
| Bangsal tanpa `OrganizationUnitId` | `RosterAvailable = false`, daftar kosong |
| Roster terbit tetapi tidak ada shift yang berjalan | `RosterAvailable = false`, daftar kosong |
| Roster hanya berisi non-perawat, atau pegawai tanpa profesi | `RosterAvailable = false`, daftar kosong |
| Shift `Draft`, `Validated`, libur, atau sudah lewat | Tidak dihitung |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`TrxShiftAssignment.cs`, `TrxRosterAssignment.cs`, `TrxRosterPeriod.cs`, `MstWorkforceProfile.cs`, `MstEmployee.cs`,
`MstProfession.cs`, `MstShift.cs`, `RosterPeriodService.cs`, `ScheduleChangeService.cs`, `ShiftSwapService.cs`,
`InpatientClinicalContextService.cs`, `EmergencyDoctorAssignmentService.cs` (`AmbilDokterJagaAktifAsync`, pola
rujukan `NV-DEC-012`), `InpBedManagementQueryService.cs` (`BedProjectionBatch.Units`), `ApplicationDbContext.cs`;
blueprint §4.6, api §3, `data/data-dictionary.md`, `TLM-NV-001`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../NurseViewManagement/Services/NvwFloorBoardQueryService.cs` | `LoadOnDutyNursesAsync`: dua query tetap (penugasan shift berjalan, lalu profesi). `RosterAvailable` dan `OnDutyNurses` diisi per bangsal. Urutan kartu menjadi kritis dulu, lalu `BedName`, lalu `BedCode` |
| `Areas/.../ClinicalManagement/Services/InpatientClinicalContextService.cs` | `MapCpptProfessionType`: `private static` → `internal static`. Satu kata, tanpa perubahan isi (`NV-DEC-027`, `TLM-NV-001` permintaan 2) |
| `00-interview-decisions.md`, `02-backend-architecture.md`, `data/data-dictionary.md`, `contracts/api-contract.md`, `contracts/integration-contract.md`, `blueprint-manifest.md` | Delta kontrak `0.1.2`: `NV-DEC-033` |
| `rawat-inap/keperawatan/docs/task-lintas-modul/TLM-NV-001-…md` | Status permintaan 2 selesai, dengan tautan ke laporan ini |

**Perubahan di luar teks kartu:** urutan kartu menurut `BedName`. Ini menutup masalah yang tercatat di `BE-NV-005` §7.
Sebelumnya API mengurutkan menurut kode internal `BD-RSMMC-xxxxx`. Tampilan TV tidak terdampak, karena frontend
sudah mengurutkan ulang menurut nama.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Bentuk field tidak berubah. `RosterAvailable` dan `OnDutyNurses` kini benar-benar diisi. Arti `RosterAvailable` dipertegas di `0.1.2`: `true` hanya bila ada minimal satu perawat/bidan jaga |
| Database | Tanpa migration, hanya baca |
| Keamanan/Auth | Tanpa perubahan. Nama perawat jaga memang ditampilkan menurut desain (`NV-DEC-012`). ID pegawai tidak ikut dikirim |

### 3.4 Delta antara kontrak dan source (`NV-DEC-033`)

Kontrak menyebut `TrxShiftAssignment.OrganizationUnitId` dan `TrxRosterAssignment.ProfessionId`. Kedua kolom itu tidak
pernah diisi oleh `RosterPeriodService.AddAssignmentAsync` milik HR. Unit hanya tersimpan di periode roster, dan profesi
hanya ada di data pegawai. Rizki memilih memakai kolom kontrak bila terisi, dengan cadangan periode roster dan profesi
pegawai.

---

## 4. Dokumentasi endpoint

Tidak ada endpoint baru. `GET /api/v1/health-services/nurse-view-management/display-runtime/board` kini mengisi
`Wards[].RosterAvailable` dan `Wards[].OnDutyNurses[]` (`DisplayName`, `ShiftLabel`).

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Checker QBE Strict, `-Path` 2 berkas | `Files evaluated: 2`, 0 violation, 0 review | `PASS` | Keluaran perintah, 3 Nov |
| `dotnet build` oleh Rizki | DLL 3 Nov 10.53; urutan kartu `BedName` (R2) membuktikan kode baru berjalan. Jumlah warning belum dinyatakan Rizki | `PASS` | DLL, R2 |
| R1 roster terbit tanpa shift (lantai 3) | Periode roster Instalasi Rawat Inap `Published` dengan 0 shift; 0 shift `Published` berjalan di DB; Rawat Inap `RosterAvailable = false`, `OnDutyNurses` kosong. 6 cek | `PASS` | `roster-papan-2026-11-03T04-13-05-752Z/R1.json` |
| R3 bangsal tanpa unit (lantai 2) | ICU dan Ruang Bayi tanpa `OrganizationUnitId` di DB; keduanya `RosterAvailable = false` tanpa nama. 4 cek | `PASS` | `R3.json` |
| R2 regresi papan `BE-NV-005` | 66 bed sama dengan papan `BE-NV-005`, kunci kartu sama, urutan kritis lalu `BedName` (325-A, 301-A, 301-BOKS, 302-A, …), nol kebocoran dari 36 nilai identitas. 5 panggilan: 1152, 1313, 1513, 931, 1109 ms (median 1.152 ms) | `PASS` | `R2.json` |
| Pengukuran ulang waktu respons | 10 panggilan `/board`: median 410 ms (251–641 ms); 10 panggilan `/current`: median 28 ms; RAM bebas 1,9 GB | Terukur, di bawah 2 detik | `waktu-2026-11-03T04-14-40-431Z.json` |
| Persiapan roster lewat aplikasi HR, sesi SuperAdmin | Shift master `200`, periode roster `201`, **penugasan perawat dan non-perawat `500`**, validasi `200`, terbitkan `200`; 0 baris shift di DB | Temuan HR, bukan bagian task ini | `roster-2026-11-03T03-11-18-442Z/roster.json` |
| Log backend untuk `500` di atas | `DbUpdateException` dengan pesan dalam `Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone', only UTC is supported` | Temuan HR | `Logs/quilvian-backend-20261103.json`, 03.11.39Z |
| Pencarian kredensial pada bukti JSON | Nol sandi, nol token | `PASS` | Pencarian pola dan nilai env |

Bukti berada di `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-nv-006-20261103/`.

**Catatan waktu respons.** Median `/board` pada 3 November berubah-ubah: 121 ms (`BE-NV-005`, 09.15), lalu sesudah
build ini 341 ms, 1.152 ms, dan 410 ms dalam tiga pengukuran berjarak beberapa menit. Pada saat yang sama, `/current`
hanya 28 ms. Tabel roster di DB dev kosong, sehingga query roster yang baru hanya menambah satu perjalanan ke database.
Karena itu kecil kemungkinan query ini penyebab utamanya; dugaannya beban mesin dev (RAM bebas 1,9 GB). Penyebabnya
belum dipisahkan dan perlu diukur ulang di server dev. Semua angka di bawah batas `NFR-002` (2 detik).

**Riwayat putaran.** Putaran `04-06-12` S4 `FAIL`, karena skrip mengira 14 nama akan terpotong padahal muat; skrip
diubah menjadi 30 nama. Putaran `04-07-31` lulus, tetapi sempat berjalan bersamaan dengan regresi `FE-NV-005` yang
mereset login perangkat yang sama. Bukti yang dipakai adalah putaran tunggal `04-13-05`.

**Temuan HR.** `RosterPeriodService.AddAssignmentAsync` membuat `ScheduledStartAt` dan `ScheduledEndAt` dari
`ShiftDate.ToDateTime(shift.StartTime)` dengan `Kind=Unspecified`. Npgsql 9 menolak nilai itu untuk kolom `timestamptz`.
Akibatnya penugasan roster dengan shift tidak bisa disimpan sama sekali lewat aplikasi. Kode HR tidak disentuh, karena
di luar wewenang task ini. Temuan ini perlu diteruskan ke pemilik modul HR.

Uji manual: `PASS` untuk R1–R3.

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

**Tidak dijalankan:** `tools/authorization-verifier/verify-authorization.sh` (`NOT RUN`; tidak ada atribut akses yang
berubah, dan skrip itu memanggil `dotnet build` milik Rizki).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `NV-AC-009` roster pagi berisi 3 perawat → header menampilkan ketiganya | **Belum terbukti dengan data nyata.** Roster tidak bisa diisi karena temuan HR di atas, dan DB dev hanya punya satu pegawai Perawat. Jalur kode terpetakan ke source; tampilan dibuktikan `FE-NV-006` dengan jawaban tiruan | Source; `roster.json` |
| `NV-AC-017` roster kosong atau `OrganizationUnitId` kosong → "Roster shift ini belum tersedia", tanpa nama ditebak | Terpenuhi dari sisi data | R1, R3 |
| Verifikasi kartu: roster kosong → `RosterAvailable = false` | Terpenuhi | R1 |
| Verifikasi kartu: 3 perawat + 1 petugas administrasi → hanya 3 perawat | **Belum** — penugasan tidak bisa disimpan HR | `roster.json` |
| Prasyarat kerja: konfirmasi ke Hamzah bahwa Rizki yang menulis baris itu | Terpenuhi. Dijalankan dengan asumsi kartu; Hamzah mengonfirmasi 2026-11-03 bahwa Rizki menulis perubahan di task Nurse View dan menyetujui perubahan `internal static` | `TLM-NV-001` §7 |
| DoD — laporan dirujuk dari `rawat-inap/keperawatan/docs/task-lintas-modul/` | Terpenuhi | `TLM-NV-001` §7 baris 2026-11-03 |

**Backend Governance Preflight:** Area `HealthServices` · Module `NurseViewManagement` · prefix `Nvw` `ACTIVE` ·
applicability `NEW CODE`. `InpatientClinicalContextService.cs` (ClinicalManagement) = `TOUCHED LEGACY`, satu kata,
restu `NV-DEC-027`. Entity HR (`TrxShiftAssignment`, `TrxRosterAssignment`, `TrxRosterPeriod`) hanya dibaca.

**QBE yang berlaku dan dipatuhi:** `QBE-SVC-001` (query di service), `QBE-DTO-001` (entity tidak dikembalikan
langsung).

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Status migration/database | Tanpa migration |
| Data uji di DB dev | Shift master "Uji Nurse View Siang" (06.00–23.00) dan periode roster "Uji Nurse View BE-NV-006" untuk Instalasi Rawat Inap, tanggal 3 Nov, `Published` tanpa penugasan. Sandi `NVW-000003` dan `NVW-000004` direset lagi oleh skrip |
| Peringatan | `NOT RUN`: `authorization-verifier`. Jumlah warning build belum dinyatakan Rizki |
| Masalah yang diketahui | Temuan HR `Kind=Unspecified` menghalangi seluruh pengisian roster lewat aplikasi; ICU dan Ruang Bayi belum punya `OrganizationUnitId` |
| Risiko tersisa | Bila HR memperbaiki temuannya dengan menyimpan jam dinding sebagai UTC tanpa konversi, shift akan bergeser 7 jam di papan. Perbaikan yang benar mengonversi jam Asia/Jakarta ke UTC (`AppDateTimeHelper.ToUtc`) |
| Perubahan sampingan | Urutan kartu menurut `BedName` (§3.2) |
| Status Git | Dua berkas source dan enam dokumen kontrak sudah di-commit Rizki sebagai `7771096f` (10.18, sebelum build 10.53). Belum di-commit: laporan ini, `FE-NV-006.md`, status roadmap, traceability, dan `TLM-NV-001` |
| Langkah berikutnya | Teruskan temuan HR; uji ulang `NV-AC-009` sesudah HR diperbaiki; lalu `BE-NV-007` |
