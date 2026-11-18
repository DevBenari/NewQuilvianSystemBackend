# Laporan Perubahan Backend — `BE-HRD-336`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-336` |
| Judul | `HrdUnitScopeService` dan penerapan cakupan pada endpoint roster |
| Slice | Amandemen `A52` atas `S-B4` Penjadwalan — `EPIC HRD-109`, gelombang `MVP-66` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 24, Grafik B84 |
| Trace | `FR-PK-002`; `HRD-DEC-194`, `206`, `208`; `INV-A52-08`; `02-backend-architecture.md` 35.4.2, 35.9.1; `contracts/api-contract.md` 32 (Roster Period "semua tulis"); `validation-matrix.md` `VR-PK-06`, `VR-PK-27` |
| Contract version | `v26` — `approved` (pengguna, 12 November 2026) |
| Dependency | `BE-HRD-334` 🟡 (tidak dibutuhkan secara teknis — task ini tidak memakai tabel baru); `BE-HRD-335` ✅ (penanda `ManageAllUnits`) |
| Klasifikasi | `MEDIUM` — skor 5: satu repository (0), sekitar 15 berkas diperiksa (1), 4 berkas source (1), logika cakupan bertingkat (1), perilaku 403 baru pada endpoint lama (1), tanpa schema (0), keamanan/otorisasi (1), tanpa UI (0) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/Corporate/HumanResource/WorkforceCore/Services/`, `Areas/Corporate/HumanResource/SchedulingManagement/Services/`, `Program.cs`; dokumen roadmap/traceability/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `9cc8097c` (branch `yoga`) + perubahan `BE-HRD-335` yang belum di-commit |
| Tanggal | 12 November 2026 |
| Status | 🟡 Sebagian — kode selesai dan terbukti lewat harness. Migration `BE-HRD-334` sudah diterapkan ke devYoga (13 November 2026); uji dengan akun non-superadmin masih menunggu akun berpenempatan berjabatan (Kepala Unit/Kepala Bidang) beserta sandinya |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `WorkforceCore`, `SchedulingManagement` |
| Pemilik / prefix registry | `Human Resource` / `Hrd` (`ACTIVE / LEGACY`) |
| Keberlakuan | `NEW CODE` untuk `HrdUnitScopeService`; `TOUCHED LEGACY` untuk `RosterPeriodService`, `ShiftReplacementService`, `Program.cs` |
| QBE yang berlaku | `QBE-NAM-001`, `QBE-SVC-001` (aturan bisnis cakupan berada di service, bukan controller; controller tidak disentuh) |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; registry; `rules/backend/` |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, siapa pun yang memegang hak `RosterPeriod : Update` dapat mengubah roster
**unit mana pun**. Contohnya, Kepala Ruang ICU dapat menerbitkan atau menutup roster HCU. Selain
itu, daftar periode roster menampilkan seluruh unit kepada semua pengguna. `HRD-DEC-194`
menetapkan bahwa seseorang hanya boleh mengelola roster unit yang ia pimpin.

---

## 2. Proses bisnis

| Pengguna | Penempatan berjabatan | Boleh mengubah roster | Boleh membaca roster |
| --- | --- | --- | --- |
| Sari, Kepala Ruang ICU | ICU | ICU | ICU |
| Pak Dodi, Kepala Bidang Keperawatan | Bidang Keperawatan (induk ICU, HCU, Bangsal Anak) | Bidang Keperawatan saja (tidak punya roster) | Bidang beserta seluruh unit di bawahnya, termasuk cucu unit |
| Petugas HR Scheduling | — + penanda `RosterPeriod : ManageAllUnits` | Semua unit | Semua unit |
| Pegawai tanpa penempatan berjabatan | — | Tidak ada | Tidak ada |

Aturan yang dipasang:

1. **Penempatan berjabatan** adalah `WfpOrganizationAssignment` aktif dengan
   `IsManagerialAssignment = true` yang berlaku pada tanggal hari ini (WIB).
2. **Cakupan baca** adalah cakupan kelola ditambah seluruh unit turunannya, ditelusuri lewat
   `ParentOrganizationUnitId` sampai habis.
3. **Data tanpa unit** (periode roster dengan `OrganizationUnitId` kosong) hanya boleh dikelola
   dan dibaca oleh pemegang cakupan semua unit. Ini pilihan gagal tertutup: tanpa unit, sistem
   tidak dapat membuktikan siapa kepala unitnya.
4. Unit sebuah shift adalah unit periode roster-nya, atau unit shift itu sendiri bila periode
   tidak bertautan unit.

Contoh: Sari menekan **Terbitkan** pada roster HCU, lalu server menjawab 403 "Anda tidak
berwenang mengubah roster unit ini.". Pak Dodi membuka detail roster ICU dan berhasil, tetapi
saat menekan **Tutup** server menjawab 403.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `02-backend-architecture.md` 35.4.2, 35.9.1; `api-contract.md` 32; `validation-matrix.md` `VR-PK-06`, `VR-PK-27`
- `Services/Security/AccessPermissionService.cs` (perilaku superadmin dan penanda)
- `WfpOrganizationAssignment.cs`, `MstOrganizationUnit.cs`, `TrxRosterPeriod.cs`, `TrxShiftAssignment.cs`, `ApplicationUser.cs`
- `RosterPeriodController.cs`, `ShiftReplacementController.cs`, `ShiftAssignmentService.cs` (pemanggil)
- `Helpers/AppDateTimeHelper.cs`; memori proyek "tanggal HR berlabel UTC"
- devYoga baca-saja (`default_transaction_read_only=on`): jumlah penempatan berjabatan, kedalaman hierarki unit, periode roster

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkforceCore/Services/HrdUnitScopeService.cs` | Baru — `GetManageScopeAsync`, `GetReadScopeAsync` (hasil disimpan per request), kelas `HrdUnitScope` dengan `Contains(Guid?)` |
| `Areas/Corporate/HumanResource/SchedulingManagement/Services/RosterPeriodService.cs` | Pemeriksaan cakupan kelola pada `Create`, `AddAssignment`, `PopulateEligibleStaff`, `CopyPreviousPeriod` (target), `ApplyShiftPattern`, `ValidateRosterPeriod`, `PublishRosterPeriod`, `CloseRosterPeriod`; cakupan baca pada `GetSummary`, `GetPaged`, `GetById`, dan periode sumber `CopyPreviousPeriod` |
| `Areas/Corporate/HumanResource/SchedulingManagement/Services/ShiftReplacementService.cs` | Cakupan kelola pada `CreateReplacement`, `ApproveReplacement`, `ExecuteDirectReplacement`, `RetroactiveCorrection`; cakupan baca pada `GetPaged` |
| `Program.cs` | `AddScoped<HrdUnitScopeService>()` |

Urutan pemeriksaan di setiap aksi: data tidak ditemukan (404) → di luar cakupan (403) → periode
terkunci (403) → aturan lain. Jadi pengguna tidak mendapat petunjuk isi periode unit lain
selain keberadaannya.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Bentuk URL dan respons tidak berubah. Perilaku baru sesuai kontrak `v26`: 403 pada tulis di luar cakupan kelola (`VR-PK-06`) dan baca detail di luar cakupan baca (`VR-PK-27`); daftar dan ringkasan periode serta daftar penggantian shift kini tersaring cakupan baca |
| Database | `NOT APPLICABLE` — tanpa migration. Hanya kueri baca tambahan (penempatan, unit) |
| Keamanan/Auth | Memperketat. Superadmin tetap melihat semua unit (perilaku `HasAccessAsync`). Penanda `AttendanceReport : ReadAllUnits` sudah dibaca kode, tetapi baru dapat diberikan sesudah `BE-HRD-349` mendaftarkannya |

---

## 4. Dokumentasi endpoint

### HR Scheduling - Roster Period

Base URL: `api/v1/corporate/human-resource/scheduling-management/roster-periods`

| Method | Path | Hak akses | Perubahan |
| --- | --- | --- | --- |
| `GET` | `/summary`, `/` | `RosterPeriod : Read` | Hanya menghitung/menampilkan periode dalam cakupan baca |
| `GET` | `/{id}` | `RosterPeriod : Read` | 403 "Anda tidak berwenang melihat data unit ini." bila di luar cakupan baca |
| `POST` | `/`, `/{id}/assignments`, `/{id}/populate-staff`, `/{id}/copy-previous`, `/{id}/apply-pattern`, `/{id}/validate`, `/{id}/publish`, `/{id}/close` | Tetap | 403 "Anda tidak berwenang mengubah roster unit ini." bila di luar cakupan kelola; `copy-previous` juga 403 bila periode sumber di luar cakupan baca |

### HR Scheduling - Shift Assignment & Replacement

Base URL: `api/v1/corporate/human-resource/scheduling-management/shift-replacements`

| Method | Path | Hak akses | Perubahan |
| --- | --- | --- | --- |
| `GET` | `/` | `ShiftReplacement : Read` | Tersaring cakupan baca |
| `POST` | `/`, `/{id}/approve`, `/{id}/replace`, `/{id}/retroactive-correct` | Tetap | 403 bila unit shift di luar cakupan kelola |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 galat, 603 peringatan (sama dengan sebelumnya). Tiga peringatan XML di `RosterPeriodService.cs:463` dan `ShiftReplacementService.cs:485` berasal dari baris lama (`f2100b2c9`) | `PASS` | Keluaran build, 59 detik |
| Harness xUnit SQLite di memori atas DLL hasil build (15 uji): `AccessPermissionService` asli, data uji di memori | 15/15 lulus | `PASS` | `dotnet test` di `C:\Users\Admin\AppData\Local\Temp\claude\d823ad68bb5a8f68\ef24ad71-9cc8-48c3-b37f-ddd8b9cac1bd\scratchpad\scopetest` |
| — Sari: kelola dan baca hanya ICU; tambah penugasan HCU 403, ICU lolos cakupan (berhenti 404 karena pegawai fiktif) | Sesuai | `PASS` | Uji `Sari_tambah_penugasan_HCU...`, `Sari_kelola_hanya_ICU...` |
| — Pak Dodi: kelola = Bidang; baca = Bidang + ICU + HCU + Bangsal Anak + Anak Isolasi (tiga tingkat), bukan Farmasi/RS; detail ICU 200, tutup ICU 403 | Sesuai | `PASS` | Uji `Dodi_*` |
| — Gagal tertutup: tanpa penempatan berjabatan, penempatan berakhir kemarin/baru mulai besok, tanpa login → cakupan kosong | Sesuai | `PASS` | Uji `Cakupan_kosong_gagal_tertutup` (3 kasus) |
| — Superadmin dan petugas HR berpenanda `ManageAllUnits` (lewat `SysAccessPolicy` departemen+jabatan) → semua unit, termasuk periode tanpa unit | Sesuai | `PASS` | Uji `Superadmin_dan_penanda...`, `Buat_periode...` |
| — Daftar/ringkasan tersaring; buat periode tanpa unit oleh Sari 403; salin dari sumber Farmasi 403; Ganti Petugas dan koreksi mundur shift HCU 403, ICU lolos cakupan (berhenti 400) | Sesuai | `PASS` | Uji sisanya |
| `AT-HRD-PK-09`, `AT-HRD-PK-10` dengan akun non-superadmin di devYoga | — | `NOT RUN` | devYoga tidak punya satu pun penempatan berjabatan aktif (0 dari 57), sehingga semua akun non-superadmin akan mendapat cakupan kosong. Menyiapkan data uji berarti menulis ke database (butuh instruksi). Backend juga belum dapat dijalankan dengan aman sebelum migration `BE-HRD-334` diterapkan |

Uji kontrol pada harness memastikan 403 memang berasal dari cakupan, bukan dari hal lain: aksi
yang sama di unit dalam cakupan lolos dan berhenti di kode lain (404/400/200/201).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Contoh 35.9.1 terpenuhi (Sari ICU, Pak Dodi bidang) | Terpenuhi di harness | Bagian 5 |
| Hierarki unit bertingkat tiga terbaca | Terpenuhi di harness | Bidang → Bangsal Anak → Anak Isolasi |
| Pengguna tanpa penempatan berjabatan dan tanpa penanda mendapat cakupan kosong | Terpenuhi di harness | Uji gagal tertutup |
| `AT-HRD-PK-09`/`-10` dengan akun non-superadmin | **Belum** | Migration sudah diterapkan (13 November 2026). Masih menunggu akun non-superadmin yang diberi penempatan berjabatan beserta hak akses `RosterPeriod`/`ShiftReplacement`; roster uji ICU `ROST-202611-0002` sudah tersedia (lihat `BE-HRD-337`) |
| DoD umum: build lulus, tanpa angka tertanam, tanpa kolom sensitif di log | Terpenuhi | Tidak ada log baru |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tiga peringatan XML lama di berkas yang disentuh; tidak dirapikan (bukan cakupan task) |
| Masalah yang diketahui | **Dampak operasional besar.** Sesudah perubahan ini dipasang, setiap pengguna non-superadmin yang belum punya penempatan berjabatan (`IsManagerialAssignment = true`) **tidak lagi dapat melihat maupun mengubah roster apa pun**. Di devYoga jumlahnya 0 dari 57 penempatan aktif. Sebelum dipakai pengguna, Admin HRD wajib menandai penempatan Kepala Unit sebagai berjabatan, dan petugas HR Scheduling wajib diberi penanda `RosterPeriod : ManageAllUnits` |
| Risiko tersisa | (1) Periode roster lama tanpa `OrganizationUnitId` (devYoga: 1 dari 1) hanya terlihat oleh superadmin/`ManageAllUnits`; bila perlu dibuka untuk Kepala Unit, unitnya harus diisi. (2) Rentang `IsManagerialAssignment` dibandingkan dengan tanggal WIB hari ini; tanggal yang tersimpan bukan tengah malam UTC dapat bergeser satu hari di batas akhir. (3) Hierarki unit dibaca utuh sekali per request; wajar untuk ratusan unit. (4) Frontend (`FE-HRD-306`) perlu menampilkan pesan 403 dan daftar kosong dengan benar |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 4 berkas source task ini (1 baru, 3 diubah) di atas perubahan `BE-HRD-335` yang belum di-commit; 2 dokumen roadmap/traceability; laporan ini. Harness uji di scratchpad sesi (folder `Tests/` diabaikan Git). Tidak ada stage atau commit |
| Langkah berikutnya | (a) Bila diizinkan: terapkan migration `BE-HRD-334`, siapkan data uji penempatan berjabatan (Sari/Pak Dodi) di devYoga, lalu jalankan `AT-HRD-PK-09`/`-10` dengan akun non-superadmin. (b) `BE-HRD-337` — `RosterValidationService` (`B3`, `B8`, `W6`, `W7`, `ValidateSingleShiftAsync`) |
