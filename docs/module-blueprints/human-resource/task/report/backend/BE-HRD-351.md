# Laporan Perubahan Backend — `BE-HRD-351`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-351` |
| Judul | Verifikasi MVP `A52` |
| Slice | Amandemen `A52` — gelombang `Verifikasi` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 24, Grafik B87 |
| Trace | Definition of Done `04-prd-to-mvp.md` 40.19; `testing/acceptance-test-matrix.md` (`AT-HRD-PK-01`–`14`, `19`–`31`, `37`–`39`, `42`–`45`); `integration-contract.md` `INT-HR-39`; `permission-audit-matrix.md` 30.1 |
| Contract version | `v26` — `approved` (pengguna, 12 November 2026) |
| Dependency | `BE-HRD-334` s.d. `BE-HRD-350` — seluruhnya ✅ kecuali `BE-HRD-338` 🟡 (`AT-HRD-PK-02` lewat API menunggu data pegawai Didampingi). `BE-HRD-336` ditutup oleh task ini (bagian 5) |
| Klasifikasi | `STANDARD` — skor 6: satu repository (0), sekitar 25 berkas diperiksa (2), proyek uji baru 17 berkas + `.gitignore` (2), hak akses dan data devYoga sementara (2), tanpa perubahan source aplikasi (0) |
| Task mode | `BACKEND` (verifikasi) |
| Target tulis | `NewQuilvianSystemBackend`: `Tests/QuilvianSystemBackend.HumanResourceTests/**` (baru), `.gitignore` (entri `!`); dokumen roadmap/traceability/laporan. **Tidak ada perubahan source aplikasi** |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `13b5d76b` (branch `yoga`) + perubahan `BE-HRD-344` s.d. `350` yang belum di-commit |
| Tanggal | 16 November 2026 |
| Status | ✅ Selesai — proyek uji 149/149 lulus; startup Development "Permission registry valid"; uji 403 dengan akun non-superadmin lulus lewat API devYoga |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` / `SelfServices` (yang diuji) |
| Module / Submodule | `HumanResource` / `AttendanceManagement`, `SchedulingManagement`, `MasterData/AttendanceAndSchedule`, `WorkforceCore`, self-service |
| Pemilik / prefix registry | `Human Resource` / `Hrd` dan `Master / Reference` / `Mst`; tidak ada entity baru |
| Keberlakuan | Proyek uji baru (bukan kode aplikasi) |
| QBE yang berlaku | `QBE-PERM-001` (diverifikasi lewat validator registri dan uji 403); selebihnya tidak berlaku karena tidak ada source aplikasi yang berubah |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

---

## 1. Masalah yang diselesaikan

| Keadaan sebelum task ini | Akibat |
| --- | --- |
| Uji A52 hanya ada di harness scratchpad sesi (`scopetest`) yang mereferensikan DLL hasil build | Bukti hilang bila scratchpad dibersihkan; aturan A52 hanya terjaga di mesin yang kebetulan pernah menjalankannya |
| `AT-HRD-PK-09`/`-10` belum pernah dijalankan dengan akun non-superadmin (`BE-HRD-336` 🟡); `AT-HRD-PK-38` hanya di harness | Superadmin menutupi kegagalan hak akses (catatan matriks uji) |
| Belum ada uji regresi `INT-HR-39` dan registri hak akses A52 | Perubahan siaga atau anotasi izin bisa merusak integrasi Lab atau startup tanpa terdeteksi |

---

## 2. Yang dibangun

**Proyek `Tests/QuilvianSystemBackend.HumanResourceTests`** (pola `OpticalTests`: `net9.0`, xUnit, SQLite di memori,
`ProjectReference` ke backend, `SkipMigrationMetadata`):

| Berkas | Isi |
| --- | --- |
| `Infrastructure/TestInfrastructure.cs` | Basis data SQLite dari model aplikasi, penyesuai model, penyimpan pengguna tiruan, `Pelaku` (sesi uji dan `AccessPermissionService`) |
| 13 kelas uji dari harness A52 | `ReplacementTests`, `PublishedRosterTests`, `RetroactiveTests`, `UnitScopeTests`, `RosterValidationTests`, `TemporaryPlacementTests`, `AttendanceSettingTests`, `AttendanceStatusTests`, `ClosePeriodTests`, `DeviceHoldTests`, `HolidayTests`, `MyScheduleTests`, `AttendanceReportTests` — namespace disesuaikan, isi tidak diubah |
| `A52PermissionRegistryTests.cs` (baru) | `PermissionRegistryValidator` untuk sembilan controller A52; 14 resource/aksi kontrak di registri (termasuk dua penanda); mekanisme 403 untuk akun non-superadmin pada setiap endpoint A52 baru (19) |
| `IntHr39RegressionTests.cs` (baru) | Dokter siaga `Active` untuk Lab tetap sama walau ada cadangan dan catatan panggilan A52 |
| `AcceptanceTestIndex.md` | Pemetaan setiap AT MVP → uji → bukti API |

**`.gitignore`**: blok `!/Tests/QuilvianSystemBackend.HumanResourceTests/` (bin/obj tetap diabaikan), mengikuti blok
Operasi/Farmasi/Gizi/Patient Management. Diperiksa: berkas uji tidak diabaikan, `bin/` diabaikan.

---

## 3. Dampak

| Aspek | Dampak |
| --- | --- |
| Source aplikasi | Tidak berubah (kontrol negatif sementara dipulihkan byte demi byte) |
| Database | Tanpa migration. Data uji devYoga lihat bagian 7 |
| Hak akses devYoga | Diubah sementara untuk uji lalu dikembalikan identik |

---

## 4. Endpoint

Tidak ada endpoint baru.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build Tests/QuilvianSystemBackend.HumanResourceTests -p:RunAnalyzers=False` | 0 galat; 604 peringatan (603 backend + peringatan kerentanan paket `SixLabors.ImageSharp` yang sudah ada di backend) | `PASS` | Keluaran build |
| `dotnet test Tests/QuilvianSystemBackend.HumanResourceTests` | **149/149 lulus** (4 m 34 d) | `PASS` | `hasil-be-hrd-351.trx` di `C:\Users\Admin\AppData\Local\Temp\claude\d823ad68bb5a8f68\4222eeca-aa89-4652-b697-eb38df816c85\scratchpad\hasil351` |
| — Cakupan AT MVP | Seluruh 34 AT dalam cakupan memiliki uji (indeks) | `PASS` | `AcceptanceTestIndex.md` |
| — Validator registri | Tidak ada temuan untuk controller A52 | `PASS` | `Controller_A52_lolos_PermissionRegistryValidator` |
| — Registri kontrak | 14/14 resource/aksi terdaftar | `PASS` | `Registri_memuat_hak_akses_A52` |
| — 403 non-superadmin | 19/19 endpoint baru: pegawai tanpa kebijakan ditolak, berkebijakan dan superadmin lolos | `PASS` | `Endpoint_A52_menolak_pegawai_tanpa_izin` |
| — Regresi `INT-HR-39` | Hanya dr. Andi (`Active`, dalam rentang); bukan penugasan `Scheduled`, bukan cadangan yang menerima panggilan | `PASS` | `IntHr39RegressionTests` |
| Kontrol negatif (a): daftar dokter siaga membaca status selain `Active` | Uji regresi `INT-HR-39` gagal; kode dipulihkan identik | `PASS` | Keluaran `dotnet test` |
| Kontrol negatif (b): `[AccessPermission("Holiday","Delete")]` dicabut | Uji validator gagal dengan temuan "Holiday.Delete"; kode dipulihkan identik. Percobaan pertama tidak gagal karena saringan uji memakai nama tipe, bukan nama resource — saringan dibetulkan lalu kontrol diulang | `PASS` | Keluaran `dotnet test` |
| Startup Development | "Permission registry valid. 2437 identitas kanonik dari 2437 kemampuan terdaftar." | `PASS` | `be351.log` |
| API devYoga (16 November 2026) — akun non-superadmin `gilang.mahendra` (sandi dari pengguna, tidak disimpan) dijadikan Kepala Bidang Instalasi Rawat Inap (induk ICU) lewat penempatan berjabatan uji `UJI-A52-KB-GILANG`; izin `RosterPeriod : Read`, `ShiftReplacement : Update`, `AttendanceReport : Read`/`Export` diberikan sementara ke jabatannya (atas persetujuan pengguna) | Disiapkan | — | `at-351.mjs` di `C:\Users\Admin\AppData\Local\Temp\claude\d823ad68bb5a8f68\5d4a0fb6-9e9b-47eb-9814-7cfdf33f8519\scratchpad\mig` |
| — `AT-HRD-PK-10` | Detail roster ICU `ROST-202611-0005` dan `UJI-A52-MYSCH` `200`; roster di luar bidang `UJI-LAB59` `403`; daftar hanya memuat roster ICU | `PASS` | `at-log.jsonl` (entri "351 …") |
| — `AT-HRD-PK-09` | Ganti Petugas shift Raka 18 Nov di roster ICU → `403` "Anda tidak berwenang mengubah roster unit ini."; sidik shift beserta jumlah riwayatnya identik sebelum/sesudah | `PASS` | `at-log.jsonl` |
| — `AT-HRD-PK-37` | Ekspor Keterlambatan ICU → `200` berkas `.xlsx`; Laporan Ketidakhadiran ICU hanya unit ICU (4 baris) | `PASS` | `at-log.jsonl` |
| — `AT-HRD-PK-38` | Ekspor IGD (di luar cakupan) → `403` "Anda tidak berwenang melihat data unit ini", bukan berkas | `PASS` | `at-log.jsonl` |
| — Pemulihan | Set izin 32 butir dikirim ulang dan dibaca identik; penempatan uji `IsActive = false`, `IsDelete = true`; gilang kembali `403` untuk roster dan ekspor | `PASS` | `at-log.jsonl` |
| Uji API per task sebelumnya | `BE-HRD-344`–`350` (termasuk 403 non-superadmin untuk mesin absen, hari libur, Jadwal Saya, laporan, ekspor) | `PASS` | Laporan masing-masing |
| `AT-HRD-PK-02` lewat API | Tidak dijalankan: devYoga tanpa pegawai berstatus Didampingi (rantai onboarding kosong); terbukti di proyek uji | `NOT RUN` | Laporan `BE-HRD-338` |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Semua uji MVP lulus | Terpenuhi — 149/149 | Bagian 5 |
| Akun non-superadmin untuk uji 403 | Terpenuhi — `AT-HRD-PK-09`, `-38` lewat API dengan `gilang.mahendra`; uji mekanisme 403 di proyek uji | Bagian 5 |
| Startup Development tanpa galat registri | Terpenuhi | `be351.log` |
| Regresi `INT-HR-39` | Terpenuhi | `IntHr39RegressionTests` |
| Laporan berisi keluaran uji | Terpenuhi | Bagian 5; berkas `.trx` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan dari kode uji |
| Masalah yang diketahui | (1) `AT-HRD-PK-02` lewat API menunggu data pegawai Didampingi (`BE-HRD-338` tetap 🟡). (2) Izin baru A52 (`AttendanceDevice`, `Holiday`, `MySchedule`, `AttendanceReport : Read/Export/ReadAllUnits`) belum diberikan ke jabatan mana pun di devYoga. (3) Daftar roster untuk Kepala Bidang memuat roster `Draft` unitnya (`ROST-202611-0003`) — perilaku layar penyusunan, bukan Jadwal Saya |
| Risiko tersisa | Batasan harness SQLite (indeks unik bertapis, desimal sebagai teks) — uji yang bergantung padanya menegakkan kunci sendiri; perilaku PostgreSQL dibuktikan lewat API di laporan task |
| Perubahan sampingan | `BE-HRD-336` ditandai ✅ di roadmap karena sisa verifikasinya ditutup di sini; tidak ada perubahan source |
| Interupsi | `NONE` |
| Status Git | Folder `Tests/QuilvianSystemBackend.HumanResourceTests/` (baru, kini tidak diabaikan), `.gitignore`; laporan ini; 2 dokumen roadmap/traceability. Perubahan `BE-HRD-344` s.d. `350` juga belum di-commit. Tidak ada commit |
| Langkah berikutnya | Gelombang `POST-MVP` (`BE-HRD-352` dst.) sesuai jadwal pengguna; frontend `FE-HRD-306` dst. |

Data uji devYoga baru (bertanda `UJI-A52`): penempatan berjabatan `UJI-A52-KB-GILANG` (Gilang Mahendra, Instalasi
Rawat Inap) — sudah dinonaktifkan dan dihapus lunak. Tidak ada perubahan lain yang tertinggal.
