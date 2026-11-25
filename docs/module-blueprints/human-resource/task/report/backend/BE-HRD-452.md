# Laporan Perubahan Backend — `BE-HRD-452`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-452` |
| Judul | Cakupan unit pada Perubahan Shift dan Tukar Dinas sisi HR |
| Slice | `A52-B` / `MVP-74` · `EPIC HRD-129` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 30, kartu `BE-HRD-452` |
| Trace | `FR-PK-038`; `HRD-DEC-242`; fakta `HRD-TF-090`, `HRD-TF-095` |
| Contract version | `v30` — API 36.7; Validation `VR-PK-35`; Permission 34.2; desain `02` 38.4, 38.9.5, 38.10 — approved 24 November 2026 |
| Dependency | Kontrak `v30` ✅; memakai `HrdUnitScopeService` (`BE-HRD-336`) |
| Klasifikasi | `STANDARD` — saringan baca dan penjaga Terapkan di service, pengalihan action controller HR; tanpa perubahan skema maupun hak akses |
| Task mode | `BACKEND` (instruksi pengguna 25 November 2026: "lanjutkan BE-HRD-452") |
| Model | Claude Opus 5.5 |
| Source acuan | Backend `4d2c0162` (branch `yoga`). Working tree sebelum task memuat `BE-HRD-450`, `BE-HRD-451` (belum di-commit) dan dokumen amandemen `A52-B`; tidak disentuh task ini |
| Tanggal | 25 November 2026 |
| Status | ✅ Selesai — 4/4 acceptance criteria terpenuhi di harness; bukti API data asli devYoga ada di `BE-HRD-455` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `SchedulingManagement` (submodule legacy tanpa baris pemilik sendiri, catatan registry baris 121; task tidak menambah entity sehingga tidak butuh keputusan prefix) |
| Keberlakuan | `TOUCHED LEGACY` — `ScheduleChangeService`, `ShiftSwapService` (konstruktor, method bercakupan, saringan), `ScheduleChangeController`, `ShiftSwapController` (action HR dialihkan) |
| QBE yang berlaku | `QBE-SVC-001` (aturan cakupan dan penjaga Terapkan di service; controller hanya memilih method dan meneruskan hasil), `QBE-API-001` (`ApiResponse`, kode status kontrak `403`/`404`), `QBE-PERM-001` (atribut `[AccessPermission]` tidak berubah). Tanpa entity/tabel baru, sehingga `QBE-NAM-001`, `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-002` tidak tersentuh. Tidak ada `<summary>` pada action controller; `<summary>` hanya pada method service |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `CLAUDE.md` backend |
| Wewenang terpisah | Implementasi source ✅; migration ✖ (tidak diperlukan); eksekusi database ✖; deployment ✖ |

---

## 1. Cara kerja sesudah perubahan

Contoh: Sari adalah Kepala Unit ICU (penempatan berjabatan di ICU). ICU punya subunit ICU Isolasi. IGD adalah unit lain di bawah rumah sakit.

| Langkah | Yang terjadi |
| --- | --- |
| Sari membuka daftar atau ringkasan Perubahan Shift (`GET schedule-change-requests`, `/summary`) | Hanya pengajuan yang shift-nya berada di ICU atau ICU Isolasi. Unit shift diambil dari periode rosternya, atau dari shift itu sendiri bila periode tidak bertautan unit. Pengajuan IGD tidak tampil dan tidak terhitung |
| Sari membuka detail atau workflow pengajuan IGD | `404` "Pengajuan perubahan jadwal tidak ditemukan." |
| Sari menekan Terapkan atau Sinkronkan pada pengajuan IGD | `403` "Anda tidak berwenang mengubah roster unit ini." Pengajuan dan shift tidak berubah |
| Sari menekan Terapkan pada pengajuan ICU Isolasi | Terlihat (cakupan baca mencakup subunit), tetapi `403`: Terapkan butuh cakupan kelola, dan cakupan kelola Sari hanya ICU |
| Sari menekan Terapkan pada pengajuan ICU yang sudah disetujui | Aturan Terapkan yang ada berjalan: `200`, shift berubah, pengajuan `Applied` |
| Sari membuka daftar Tukar Dinas | Tukar dinas ICU–ICU dan lintas ICU–IGD tampil (salah satu shift ada di ICU); IGD–IGD tidak |
| Sari menerapkan tukar dinas lintas ICU–IGD | `403`: kedua unit harus dalam cakupan kelola. Tidak ada shift yang tertukar |
| Petugas HR Scheduling (`RosterPeriod : ManageAllUnits`) | Melihat dan menerapkan semua unit, termasuk pengajuan jadwal tetap tanpa shift |
| Pegawai tanpa penempatan berjabatan yang punya hak `ScheduleChangeRequest : Read` | Daftar kosong, detail `404`, Terapkan `403` (gagal tertutup) |
| Pegawai mengajukan atau membatalkan lewat ESS; engine menyetujui lalu menerapkan otomatis | Tidak berubah: ESS dan penerapan otomatis workflow memakai method lama tanpa saringan unit |

---

## 2. Perubahan

| Berkas | Isi |
| --- | --- |
| `Areas/Corporate/HumanResource/SchedulingManagement/Services/ScheduleChangeService.cs` | Konstruktor menerima `HrdUnitScopeService`. Method baru untuk sisi HR: `GetSummaryInScopeAsync`, `GetPagedInScopeAsync`, `GetByIdInScopeAsync`, `CheckReadScopeAsync` (`404` di luar cakupan baca), `CheckManageScopeAsync` (`404` tidak ada; `403` di luar cakupan kelola), `ApplyInScopeAsync` (penjaga lalu `ApplyAsync`). Isi ringkasan dan daftar dipindah ke `BuildSummaryAsync`/`BuildPagedAsync` dengan parameter cakupan; `GetSummaryAsync`/`GetPagedAsync` lama memanggilnya tanpa cakupan (perilaku sama). Saringan `ApplyReadScope`: unit `CurrentShiftAssignment` = unit periode roster ?? unit shift |
| `…/Services/ShiftSwapService.cs` | Pola yang sama. Terbaca bila unit shift pemohon **atau** tujuan dalam cakupan baca; `CheckManageScopeAsync` mensyaratkan **kedua** unit dalam cakupan kelola |
| `…/Controllers/ScheduleChangeController.cs`, `…/Controllers/ShiftSwapController.cs` | Ringkasan, daftar, detail, dan Terapkan memanggil method bercakupan. `GET {id}/workflow` didahului `CheckReadScopeAsync`, `POST {id}/workflow/synchronize` didahului `CheckManageScopeAsync`. Atribut rute dan hak akses tidak berubah |
| `Tests/QuilvianSystemBackend.HumanResourceTests/ScheduleChangeScopeTests.cs` (baru), `AcceptanceTestIndex.md` | 7 uji lewat controller HR dan service nyata; baris `AT-HRD-PK-52` dan baris kelas di indeks |

Controller ESS (`ScheduleChangeSelfServiceController`, `ShiftSwapSelfServiceController`), layanan integrasi workflow, dan layanan siklus hidup workflow tidak diubah.

### Delta terhadap kartu roadmap dan kontrak

| Hal | Kartu/kontrak | Pelaksanaan | Alasan |
| --- | --- | --- | --- |
| Sumber unit shift | Desain 38.4: `TrxShiftAssignment.OrganizationUnitId` | Unit periode roster shift itu, atau `TrxShiftAssignment.OrganizationUnitId` bila periode tidak bertautan unit | Aturan yang sama dipakai cakupan Ganti Petugas (`ShiftReplacementService`) dan roster. Dengan begitu satu shift selalu jatuh di unit yang sama untuk setiap layar. Shift bertanda unit pada periode tanpa unit tetap terbaca (diuji) |
| Bentuk perubahan | "Daftar, ringkasan, detail, dan workflow hanya memuat…" | Method bercakupan baru; method lama tetap tanpa saringan | Method lama juga dipakai ESS dan penerapan otomatis workflow (`…WorkflowLifecycleService.ApplyAsync`). Menyaring di sana akan mengubah alur ESS dan persetujuan engine (AC4) |
| Pengajuan di luar cakupan baca saat Terapkan | Tidak disebut | `403` (bukan `404`) | AC2 meminta `403` untuk Terapkan pengajuan IGD; penjaga kelola dinilai langsung |
| Status kontrak API 36.7 | Tabel masih "**Rencana (belum tersedia)**" | Tidak disunting | Artefak kontrak terkunci hash. Penyegaran status dicatat untuk pass blueprint |

---

## 3. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `ScheduleChangeScopeTests` | 7/7 lulus | `PASS` (AC1–AC4) | Keluaran `dotnet test` |
| `UnitScopeTests` (`BE-HRD-336`) bersama uji baru | 22/22 lulus | `PASS` | Keluaran `dotnet test` |
| Kontrol negatif: 20 mutasi — service Perubahan Shift: (1) saringan baca dimatikan; (2) tanpa cadangan unit shift; (3) Terapkan tanpa cakupan kelola; (4) cakupan baca dipakai untuk kelola; (5) dan (6) ringkasan/daftar jalur ESS ikut disaring. Controller Perubahan Shift: (7) daftar, (8) ringkasan, (9) detail, (11) Terapkan memanggil method lama; (10) penjaga workflow dan (12) penjaga sinkronisasi dibuang. Service Tukar Dinas: (13) saringan baca dimatikan; (14) butuh kedua shift terbaca; (15) Terapkan cukup satu unit. Controller Tukar Dinas: (16) daftar, (17) detail, (18) Terapkan memanggil method lama; (19) penjaga sinkronisasi dan (20) penjaga workflow dibuang | 20/20 tertangkap tanpa galat kompilasi. Setiap mutasi menggagalkan uji AC-nya: AC1 oleh (1), (2), (7)–(10), (13), (14), (16), (17), (20); AC2 oleh (3), (4) (kasus subunit ICU Isolasi), (11), (12), (15), (18), (19); AC4 oleh (5), (6). Uji gagal tertutup ikut menangkap (1), (3), (7)–(9), (11), (13), (16), (18). Keempat berkas dipulihkan dengan `writeFileSync` (mtime baru), hash sama dengan salinan asli | `PASS` | `mut452.cjs`, `mut452-hasil.log` di scratchpad sesi |
| Regresi penuh proyek uji HR | 437/437 lulus (12 menit 58 detik) sesudah build ulang dari source asli, dengan env `QUILVIAN_BACKEND_ROOT`. Dijalankan dari salinan tanpa empat uji payroll yang tidak terkompilasi sejak merge `815ce085`; salinan selain itu identik dengan folder uji repo | `PASS` | `test452-full.log` |
| Build proyek utama | 0 galat (`-p:RunAnalyzers=False`) | `PASS` | `build452-final.log` |
| QBE `Strict` atas 5 berkas | 0 violation, 0 review. Probe `TrxZzzQbeProbe452` (ber-`[Table]`) terdeteksi 4 violation lalu dihapus | `PASS` | Keluaran checker |
| Regresi uji perubahan jadwal / tukar dinas | Tidak ada uji sebelumnya untuk kedua service (pencarian `ScheduleChange`/`ShiftSwap` di folder `Tests` kosong) | `N/A` | Pencarian source |
| Bukti API data asli devYoga | Tidak dijalankan di task ini | `NOT RUN` | Kartu menempatkannya di `BE-HRD-455` |

### Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Kepala Unit ICU: daftar dan ringkasan hanya pengajuan ICU (dan tukar dinas lintas ICU–IGD); detail pengajuan IGD `404` | Terpenuhi | `Kepala_ICU_hanya_melihat_perubahan_shift_ICU`, `Kepala_ICU_melihat_tukar_dinas_ICU_dan_lintas_ICU_IGD` |
| 2. Terapkan atau sinkronisasi pengajuan IGD → `403`, tanpa perubahan; tukar dinas ICU–IGD tanpa cakupan IGD → `403` | Terpenuhi | `Terapkan_dan_sinkronisasi_perubahan_shift_di_luar_cakupan_kelola_ditolak`, `Terapkan_tukar_dinas_butuh_kedua_unit_dalam_cakupan_kelola`; gagal tertutup: `Pengguna_tanpa_cakupan_tidak_melihat_dan_tidak_menerapkan` |
| 3. `ManageAllUnits` melihat dan menerapkan semua | Terpenuhi | `ManageAllUnits_melihat_dan_menerapkan_semua` |
| 4. Alur ESS Perubahan Shift/Tukar Dinas dan persetujuan lewat engine tidak berubah | Terpenuhi | `Jalur_ESS_dan_penerapan_otomatis_workflow_tidak_berubah` (method bersama tanpa saringan; `ApplyAsync` yang dipanggil siklus hidup workflow tetap berhasil untuk pengguna tanpa cakupan) |

---

## 4. Temuan

| Temuan | Dampak | Tindakan |
| --- | --- | --- |
| As-is: daftar, ringkasan, detail, workflow, dan Terapkan sisi HR tidak dibatasi unit (`HRD-TF-095`) | Siapa pun yang memegang hak `ScheduleChangeRequest`/`ShiftSwapRequest` melihat dan menerapkan pengajuan semua unit | Diperbaiki (inti task) |
| **Pengajuan Perubahan Shift tanpa shift** (jadwal tetap, `CurrentShiftAssignmentId` kosong) tidak punya unit shift | Hanya terlihat oleh pemegang cakupan semua unit (`ManageAllUnits`/`ReadAllUnits`). Kepala Unit tidak melihatnya di layar HR. Ini konsekuensi definisi kontrak ("unit shift `CurrentShiftAssignmentId`") dan aturan gagal tertutup `HrdUnitScope` | **Perlu keputusan pemilik modul**: tetap seperti ini, atau memakai unit penempatan jadwal pegawai (`WfpWorkScheduleAssignment.OrganizationUnitId`) sebagai cadangan lewat amandemen kontrak |
| Pengguna HR tanpa cakupan yang sebelumnya melihat semua kini melihat lebih sedikit | Risiko kartu: perubahan perilaku yang disetujui | Petugas HR lintas unit perlu penanda `RosterPeriod : ManageAllUnits` lewat Akses Role |
| Penjaga Terapkan dinilai sebelum transaksi `ApplyAsync` | Cakupan bisa berubah di antara keduanya (misalnya penempatan berakhir tepat saat itu) | Diterima: cakupan berdasar penempatan hari ini dan tidak berubah dalam satu permintaan |

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Migration / database | Tidak ada migration. Tidak ada eksekusi database |
| Git | Tidak ada stage, commit, push, pull, merge, rebase, atau deploy. Berkas baru: `ScheduleChangeScopeTests.cs` (cukup `git add`) |
| Dampak frontend | Kontrak respons tidak berubah. Layar HR (`FE-HRD-404`, `FE-HRD-405`) menerima `403` "Anda tidak berwenang mengubah roster unit ini." pada Terapkan di luar cakupan, dan `404` pada detail di luar cakupan |
| Bukti data asli | Akun Kepala Unit dan pengajuan lintas unit di devYoga dikerjakan di `BE-HRD-455` |
| Task berikutnya | `BE-HRD-359` (daftar penempatan sementara lintas pegawai, dibuka kembali) atau `BE-HRD-453`, sesuai urutan bagian 30 |
