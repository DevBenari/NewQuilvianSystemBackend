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
| Dependency | `BE-NV-005` ✅ |
| Klasifikasi | `HEAVY` — mengubah satu baris di modul Clinical milik tim keperawatan (restu `NV-DEC-027`) dan membaca roster HR, tanpa migration. 4 Nov ditambah perbaikan dua nilai di `RosterPeriodService` milik HR (restu Rizki) |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/NurseViewManagement/Services/NvwFloorBoardQueryService.cs`; satu kata di `Areas/HealthServices/ClinicalManagement/Services/InpatientClinicalContextService.cs`; dokumen delta kontrak; `TLM-NV-001`; laporan; status roadmap. 4 Nov: `Areas/Corporate/HumanResource/SchedulingManagement/Services/RosterPeriodService.cs` (lintas modul HR, restu Rizki) |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6489c32c` (branch `rizkiG`); perbaikan HR di `30f1396d` |
| Tanggal | 2026-11-03 sampai 2026-11-04 |
| Status | ✅ **SELESAI 4 November 2026** — QBE Strict `PASS` (2 berkas pada 3 Nov; `RosterPeriodService.cs` pada 4 Nov), build Rizki lulus (3 Nov 10.53; 4 Nov 10.39 dengan perbaikan HR), uji papan 3/3 `PASS` (16 cek): roster kosong dan bangsal tanpa unit menjawab `RosterAvailable = false` tanpa nama, papan `BE-NV-005` tidak berubah. **`NV-AC-009` terbukti dengan data nyata** (AC009 `PASS`, 8 cek): roster `Published` Instalasi Rawat Inap berisi tiga perawat, satu pegawai tanpa profesi, dan satu non-perawat; kepala bangsal Rawat Inap menampilkan tepat tiga perawat itu. Penugasan shift bisa disimpan sesudah perbaikan HR lintas modul (§3.2, §5). Verifikasi developer, bukan UAT. Riwayat: 🟡 SEBAGIAN 3 November 2026 karena HR gagal menyimpan penugasan shift dan DB dev hanya punya satu pegawai Perawat |

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
| `Areas/Corporate/HumanResource/SchedulingManagement/Services/RosterPeriodService.cs` | **Lintas modul HR, 4 Nov, restu Rizki.** Di `AddAssignmentAsync`, `ScheduledStartAt` dan `ScheduledEndAt` dibungkus `AppDateTimeHelper.ToUtc`, sehingga jam shift Asia/Jakarta dikonversi ke UTC sebelum disimpan. Ditambah `using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers`. Sebelumnya setiap penugasan shift gagal `500` (§5) |
| `00-interview-decisions.md`, `02-backend-architecture.md`, `data/data-dictionary.md`, `contracts/api-contract.md`, `contracts/integration-contract.md`, `blueprint-manifest.md` | Delta kontrak `0.1.2`: `NV-DEC-033` |
| `rawat-inap/keperawatan/docs/task-lintas-modul/TLM-NV-001-…md` | Status permintaan 2 selesai, dengan tautan ke laporan ini |

**Perubahan di luar teks kartu:** urutan kartu menurut `BedName`. Ini menutup masalah yang tercatat di `BE-NV-005` §7.
Sebelumnya API mengurutkan menurut kode internal `BD-RSMMC-xxxxx`. Tampilan TV tidak terdampak, karena frontend
sudah mengurutkan ulang menurut nama.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Bentuk field tidak berubah. `RosterAvailable` dan `OnDutyNurses` kini benar-benar diisi. Arti `RosterAvailable` dipertegas di `0.1.2`: `true` hanya bila ada minimal satu perawat/bidan jaga |
| Database | Tanpa migration. Nurse View hanya membaca; perbaikan HR hanya mengubah nilai yang ditulis `AddAssignmentAsync` menjadi UTC yang benar |
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

**Uji ulang 4 November 2026, data nyata.** Dijalankan sesudah perbaikan HR dan build Rizki:

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Checker QBE Strict, `-Path` `RosterPeriodService.cs` | `Files evaluated: 1`, 0 violation, 0 review | `PASS` | Keluaran perintah, 4 Nov |
| `dotnet build` oleh Rizki | DLL 4 Nov 10.39.55, lebih baru dari perubahan `RosterPeriodService.cs` (09.47); proses backend mulai 10.42.39; `/current` menjawab `401`. Jumlah warning belum dinyatakan Rizki | `PASS` | Waktu DLL dan proses |
| Persiapan profesi lewat aplikasi, sesi SuperAdmin atas perintah Rizki | `PUT employees` untuk R. dan Y. menjawab `200`: profesi menjadi Perawat dan kolom `MstEmployee` lain tidak berubah. Efek bawaan aplikasi: baris `AspNetUserOrganization` warisan diadopsi (`SourceAssignmentId` diisi), dan `Description` penempatan primer diganti. S. menjawab `500` (temuan 1 di bawah) tanpa perubahan data | Persiapan data | `uji-nv-006-ac009-20261104/profesi-2026-11-04T03-00-52-577Z/profesi.json`; log 03.01.02Z |
| AC009 roster shift berjalan | Periode "Uji Nurse View NV-AC-009" `201`; 5 penugasan `201` (D., S., R., Y., satu non-perawat); validasi `200`, terbitkan `200`. Di DB 5 shift `Published`, 06.00–23.00 WIB, sedang berjalan. `/board` `200`: Rawat Inap `RosterAvailable = true`, `OnDutyNurses` tepat 3 nama yang sama dengan D., R., dan Y.; S. (tanpa profesi) dan non-perawat tidak tampil; `ShiftLabel` "Uji Nurse View Siang". Kepala bangsal di TV `NVW-000003` menampilkan "Perawat jaga: …" dengan ketiga nama. 8 cek | `PASS` | `roster-2026-11-04T03-44-59-282Z/AC009.json`, `ringkasan.json`, `AC009-kepala-bangsal.png`, `AC009-papan.png` |
| Log backend selama AC009 | 61 baris sejak 03.44Z, 0 `Error` | `PASS` | `Logs/quilvian-backend-20261104.json` |
| Pencarian kredensial pada bukti 4 Nov | Nol kecocokan dengan 7 nilai sandi env, nol field `password` | `PASS` | Pencarian nilai env |

Bukti 4 Nov berada di `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-nv-006-ac009-20261104/`. Jam 06.00–23.00 WIB
di DB sekaligus menunjukkan perbaikan HR tidak menggeser shift: jam Asia/Jakarta dikonversi ke UTC, bukan dicap UTC.

**Catatan waktu respons.** Median `/board` pada 3 November berubah-ubah: 121 ms (`BE-NV-005`, 09.15), lalu sesudah
build ini 341 ms, 1.152 ms, dan 410 ms dalam tiga pengukuran berjarak beberapa menit. Pada saat yang sama, `/current`
hanya 28 ms. Tabel roster di DB dev kosong, sehingga query roster yang baru hanya menambah satu perjalanan ke database.
Karena itu kecil kemungkinan query ini penyebab utamanya; dugaannya beban mesin dev (RAM bebas 1,9 GB). Penyebabnya
belum dipisahkan dan perlu diukur ulang di server dev. Semua angka di bawah batas `NFR-002` (2 detik).

**Riwayat putaran.** Putaran `04-06-12` S4 `FAIL`, karena skrip mengira 14 nama akan terpotong padahal muat; skrip
diubah menjadi 30 nama. Putaran `04-07-31` lulus, tetapi sempat berjalan bersamaan dengan regresi `FE-NV-005` yang
mereset login perangkat yang sama. Bukti yang dipakai adalah putaran tunggal `04-13-05`.

**Temuan HR (3 Nov) dan perbaikannya (4 Nov).** `RosterPeriodService.AddAssignmentAsync` membuat `ScheduledStartAt`
dan `ScheduledEndAt` dari `ShiftDate.ToDateTime(shift.StartTime)` dengan `Kind=Unspecified`. Npgsql 9 menolak nilai itu
untuk kolom `timestamptz`, sehingga penugasan roster dengan shift tidak bisa disimpan sama sekali lewat aplikasi. Pada
3 November kode HR tidak disentuh karena di luar wewenang task. Pada 4 November Rizki merestui perbaikan lintas modul:
kedua nilai dibungkus `AppDateTimeHelper.ToUtc` (§3.2), sesuai arah perbaikan yang dicatat di §7 versi 3 Nov.

**Temuan lintas modul 4 November yang tidak diperbaiki (dicatat saja).**

1. **Platform: `PUT employees` gagal `500` untuk sebagian pegawai berakun login.** `EmployeeController.UpdateEmployee`
   (baris 1420) memanggil `OrganizationAuthorizationProjectionService.ReconcileWorkforceProfileAsync`. Pada S., baris
   `AspNetUserOrganization` yang aktif menunjuk `SourceAssignmentId` penempatan lama (`885df232…`), sedangkan penempatan
   yang berlaku `16505636…`. Service tidak menemukan baris untuk penempatan yang berlaku, lalu menyisipkan baris baru
   dengan kunci yang sama. Akibatnya ditolak unique `IX_AspNetUserOrganization_UserId_DepartmentId_PositionId_Effec~`
   (`23505`). Baris warisan tanpa `SourceAssignmentId` dengan satu kandidat (R. dan Y.) diadopsi tanpa galat. Jadi galat
   ini tidak mengenai semua pegawai berakun login, melainkan setiap pegawai yang baris proyeksinya menunjuk penempatan
   lama, dan data pegawai itu tidak bisa disunting sama sekali. Milik platform (berkas terakhir diubah devbenari,
   `e1d11214`). Akibat bagi task ini: S. tetap tanpa profesi dan ikut roster sebagai bukan perawat.
2. **HR: konvensi waktu `ScheduleChangeService`.** `ApplyShiftToAssignment` menyimpan `ScheduledStartAt` dan
   `ScheduledEndAt` dengan `DateTime.SpecifyKind(…, DateTimeKind.Utc)`, sehingga jam dinding WIB dicap UTC tanpa konversi.
   Konvensi ini bertentangan dengan perbaikan di atas. Shift 06.00–23.00 yang diubah lewat perubahan jadwal akan
   tersimpan 06.00–23.00 UTC (13.00–06.00 WIB), sehingga kepala bangsal menampilkan perawat jaga bergeser 7 jam. Keputusan
   konvensinya ada pada pemilik HR. Tidak diuji.

Uji manual: `PASS` untuk R1–R3 (3 Nov) dan AC009 (4 Nov).

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

**Tidak dijalankan:** `tools/authorization-verifier/verify-authorization.sh` (`NOT RUN`; tidak ada atribut akses yang
berubah, dan skrip itu memanggil `dotnet build` milik Rizki).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `NV-AC-009` roster pagi berisi 3 perawat → header menampilkan ketiganya | Terpenuhi, data nyata (4 Nov): roster `Published` dengan shift berjalan berisi D., R., dan Y. (Perawat) → kepala bangsal Rawat Inap menampilkan ketiganya | AC009 |
| `NV-AC-017` roster kosong atau `OrganizationUnitId` kosong → "Roster shift ini belum tersedia", tanpa nama ditebak | Terpenuhi dari sisi data | R1, R3 |
| Verifikasi kartu: roster kosong → `RosterAvailable = false` | Terpenuhi | R1 |
| Verifikasi kartu: 3 perawat + 1 petugas administrasi → hanya 3 perawat | Terpenuhi (4 Nov): 3 perawat, 1 non-perawat, dan 1 pegawai tanpa profesi → hanya 3 perawat yang tampil | AC009 |
| Prasyarat kerja: konfirmasi ke Hamzah bahwa Rizki yang menulis baris itu | Terpenuhi. Dijalankan dengan asumsi kartu; Hamzah mengonfirmasi 2026-11-03 bahwa Rizki menulis perubahan di task Nurse View dan menyetujui perubahan `internal static` | `TLM-NV-001` §7 |
| DoD — laporan dirujuk dari `rawat-inap/keperawatan/docs/task-lintas-modul/` | Terpenuhi | `TLM-NV-001` §7 baris 2026-11-03 |

**Backend Governance Preflight:** Area `HealthServices` · Module `NurseViewManagement` · prefix `Nvw` `ACTIVE` ·
applicability `NEW CODE`. `InpatientClinicalContextService.cs` (ClinicalManagement) = `TOUCHED LEGACY`, satu kata,
restu `NV-DEC-027`. Entity HR (`TrxShiftAssignment`, `TrxRosterAssignment`, `TrxRosterPeriod`) hanya dibaca oleh Nurse View.
`RosterPeriodService.cs` (Corporate/HumanResource) = `TOUCHED LEGACY` pada 4 Nov: dua nilai dibungkus `ToUtc` dan satu
`using`, restu Rizki.

**QBE yang berlaku dan dipatuhi:** `QBE-SVC-001` (query di service), `QBE-DTO-001` (entity tidak dikembalikan
langsung).

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Status migration/database | Tanpa migration |
| Data uji di DB dev | Shift master "Uji Nurse View Siang" (06.00–23.00) dan periode roster "Uji Nurse View BE-NV-006" untuk Instalasi Rawat Inap, tanggal 3 Nov, `Published` tanpa penugasan. 4 Nov: periode roster "Uji Nurse View NV-AC-009" (Instalasi Rawat Inap, tanggal 4 Nov, `Published`, 5 penugasan shift "Uji Nurse View Siang"), sehingga kepala bangsal Rawat Inap lantai 3 menampilkan tiga nama sampai 23.00 WIB 4 Nov. Profesi Perawat pada R. dan Y. diisi lewat aplikasi; S. masih tanpa profesi. Sandi `NVW-000003` dan `NVW-000004` direset lagi oleh skrip |
| Peringatan | `NOT RUN`: `authorization-verifier`. Jumlah warning build 3 dan 4 Nov belum dinyatakan Rizki |
| Masalah yang diketahui | Temuan (1) platform dan (2) HR (§5) belum diperbaiki; S. belum berprofesi karena temuan (1); ICU dan Ruang Bayi belum punya `OrganizationUnitId` |
| Risiko tersisa | Temuan (2): shift yang diubah lewat perubahan jadwal akan bergeser 7 jam di papan sampai pemilik HR menyeragamkan konvensi waktu |
| Perubahan sampingan | Urutan kartu menurut `BedName` (§3.2); perbaikan HR `RosterPeriodService.cs` (lintas modul, restu Rizki 4 Nov) |
| Status Git | Dua berkas source dan enam dokumen kontrak di `7771096f` (3 Nov 10.18, sebelum build 10.53); laporan versi 3 Nov di `55335caf`; perbaikan HR `RosterPeriodService.cs` di `30f1396d` (4 Nov 10.08). Pembaruan 4 Nov pada laporan ini belum di-commit |
| Langkah berikutnya | Commit oleh Rizki. Teruskan temuan (1) ke pemilik platform dan temuan (2) ke pemilik HR |
