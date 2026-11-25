# Laporan Perubahan Backend — `BE-HRD-359`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-359` |
| Judul | Daftar penempatan sementara lintas pegawai (dibuka kembali) |
| Slice | `A52-B` / `MVP-74` gelombang 2 · `EPIC HRD-128` (sebelumnya `A52`, `EPIC HRD-120`, tertahan `DSG-Q-02`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 30, kartu `BE-HRD-359` (kartu lama bagian 24 menunjuk ke sini) |
| Trace | `FR-PK-037` (juga dirujuk `FR-PK-032` grup menu); `HRD-DEC-240` |
| Contract version | `v30` — API 36.6; Validation `VR-PK-27`; Permission 34.1; Integration `INT-HR-117`; desain `02` 38.4, 38.9.4, 38.11 — approved 24 November 2026 |
| Dependency | Kontrak `v30` ✅; memakai `HrdUnitScopeService` (`BE-HRD-336`) |
| Klasifikasi | `STANDARD` — controller dan service baca baru, DTO baru, satu registrasi DI, satu resource hak akses baru; tanpa entity, tabel, maupun migration |
| Task mode | `BACKEND` (instruksi pengguna 25 November 2026: "lanjutkan BE-HRD-359") |
| Model | Claude Opus 5.5 |
| Source acuan | Backend `4d2c0162` (branch `yoga`). Working tree sebelum task memuat `BE-HRD-450` s.d. `BE-HRD-452` (belum di-commit) dan dokumen amandemen `A52-B`; tidak disentuh task ini |
| Tanggal | 25 November 2026 |
| Status | ✅ Selesai — 4/4 acceptance criteria terpenuhi di harness; bukti API data asli devYoga ada di `BE-HRD-455` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkforceCore` (registry: pemilik `WorkforceCore / WorkforceProfileManagement`, prefix `Wfp`, catatan registry 2026-08-28). Task hanya membaca `WfpOrganizationAssignment` yang sudah ada |
| Keberlakuan | `NEW CODE` — `TemporaryAssignmentController`, `TemporaryAssignmentQueryService`, `TemporaryAssignmentDtos`; `TOUCHED LEGACY` — `Program.cs` (satu baris registrasi) |
| QBE yang berlaku | `QBE-SVC-001` (aturan cakupan, unit asal, status waktu, dan paginasi di service; controller hanya meneruskan query dan memetakan hasil), `QBE-API-001` (`ApiResponse<PagedResult<…>>`, kode status kontrak), `QBE-PERM-001` (`[AccessController]` modul `HUMAN_RESOURCE_WORKFORCE_CORE`, `[AccessAction]` + `[AccessPermission("TemporaryAssignment", "Read")]` pada kedua endpoint). Tanpa entity/tabel baru, sehingga `QBE-NAM-001`, `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-002` tidak tersentuh. Tidak ada `<summary>` pada action controller; `filters/metadata` dideklarasikan tepat sesudah konstruktor |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `CLAUDE.md` backend |
| Wewenang terpisah | Implementasi source ✅; migration ✖ (tidak diperlukan); eksekusi database ✖; deployment ✖ |

---

## 1. Cara kerja sesudah perubahan

Contoh desain: Raka (penempatan utama ICU) ditempatkan sementara di IGD dari tiga hari lalu sampai sepuluh hari lagi, dengan surat SPT-2027-031.

| Langkah | Yang terjadi |
| --- | --- |
| Kepala Unit ICU membuka Penugasan Tambahan (`GET workforce-core/temporary-assignments`) | Raka tampil: unit asal ICU (penempatan utama yang berlaku pada tanggal mulai), unit tujuan IGD, tanggal, nomor surat, keterangan, dan status "Berlangsung". Penempatan sementara pegawai lain yang bertujuan ICU juga tampil |
| Kepala Unit IGD membuka daftar yang sama | Raka tampil karena tujuannya IGD. Penempatan pegawai IGD ke unit lain juga tampil karena unit asalnya IGD |
| Kepala Unit Farmasi membuka daftar | Raka tidak tampil. Penempatan utama Raka di Farmasi dulu sudah berakhir sebelum tanggal mulai, sehingga tidak dihitung sebagai unit asal |
| Kepala Unit ICU memfilter unit IGD | `403` "Anda tidak berwenang melihat data unit ini" (`VR-PK-27`) |
| Petugas HR berpenanda semua unit | Melihat semua penempatan sementara, dengan filter status waktu `Upcoming`/`Ongoing`/`Ended` menurut tanggal WIB hari ini |
| Pegawai tanpa penempatan utama pada tanggal mulai | Unit asal kosong; baris tetap tampil bila unit tujuan dalam cakupan |
| Pengguna tanpa `TemporaryAssignment : Read` | Ditolak filter hak akses sebelum service dipanggil |

Penempatan bukan `Temporary`, penempatan yang dihapus, dan penempatan nonaktif tidak tampil. Pencatatan dan perubahan penempatan tetap di Profil Pegawai 360.

---

## 2. Perubahan

| Berkas | Isi |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkforceCore/Controllers/TemporaryAssignmentController.cs` (baru) | `GET filters/metadata` dan `GET /`; `[AccessController]` modul `HUMAN_RESOURCE_WORKFORCE_CORE`, resource `TemporaryAssignment`, `SortOrder = 27`; kedua action `[AccessPermission("TemporaryAssignment", "Read")]`; `ProducesResponseType` 200/400/403 |
| `…/WorkforceCore/Services/TemporaryAssignmentQueryService.cs` (baru) | Membaca `WfpOrganizationAssignment` `AssignmentType = Temporary`, aktif, tidak dihapus. Unit asal dihitung di basis data sebagai subkueri berkorelasi: penempatan utama (`IsPrimary`, bukan `Temporary`, aktif, tidak dihapus, berunit) yang berlaku pada tanggal mulai; bila lebih dari satu, yang mulainya paling akhir. Saringan cakupan baca (tujuan **atau** asal), filter unit (tujuan atau asal; di luar cakupan `403`), rentang tanggal bersinggungan, status waktu WIB, cari nama/kode/nomor surat, urut tanggal mulai terbaru, paginasi di basis data (maks. 100) |
| `…/WorkforceCore/DTOs/TemporaryAssignmentDtos.cs` (baru) | `TemporaryAssignmentPagedQuery`, `TemporaryAssignmentFilterMetadataResponse`, `TemporaryAssignmentOptionResponse`, `TemporaryAssignmentResponse` (ruas sesuai kontrak 36.6), `TemporaryAssignmentQueryResult<T>` |
| `Program.cs` | `AddScoped<TemporaryAssignmentQueryService>()` |
| `Tests/QuilvianSystemBackend.HumanResourceTests/TemporaryAssignmentQueryTests.cs` (baru), `AcceptanceTestIndex.md` | 8 uji lewat controller dan service nyata; baris `AT-HRD-PK-51` dan baris kelas di indeks |

### Delta terhadap kartu roadmap dan kontrak

| Hal | Kartu/kontrak | Pelaksanaan | Alasan |
| --- | --- | --- | --- |
| Arti filter `organizationUnitId` | "filter unit" | Penempatan yang unit tujuan **atau** unit asalnya sama dengan unit itu | Mengikuti aturan cakupan (asal atau tujuan). Dengan begitu Kepala Unit yang memfilter unitnya sendiri melihat daftar yang sama dengan tanpa filter |
| `400` untuk `timeStatus` tidak dikenal dan `fromDate` > `toDate` | Kode status kontrak: `200`, `401`, `403` | Ditambah `400` dengan pesan | Tanpa penolakan, nilai salah akan diam-diam menampilkan semua data. Ruas dan perilaku sah tidak berubah |
| Penempatan utama | "unit penempatan utama pegawai pada tanggal mulai" | `IsPrimary = true`, bukan `Temporary`; bila lebih dari satu berlaku, yang mulainya paling akhir | `IsPrimary` dipakai sebagai penanda utama di `WfpOrganizationAssignmentController` dan `AttendanceDispensationService` |
| Label status waktu | Kontrak hanya menyebut "+ label" | `Akan Datang`, `Berlangsung`, `Selesai` | Label tampilan tidak ditetapkan kontrak; frontend dapat memakai label dari metadata |
| Registri hak akses A52 | Desain 38.12: `A52PermissionRegistryTests` diperluas tiga resource baru | Tidak diperluas di task ini; uji sendiri membuktikan registri, validator, dan penolakan untuk `TemporaryAssignment` | Perluasan uji registri gabungan menjadi bagian `BE-HRD-455` |
| Status kontrak API 36.6 | Tabel masih "**Rencana (belum tersedia)**" | Tidak disunting | Artefak kontrak terkunci hash. Penyegaran status dicatat untuk pass blueprint |

---

## 3. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `TemporaryAssignmentQueryTests` | 8/8 lulus | `PASS` (AC1–AC4) | Keluaran `dotnet test` |
| Kontrol negatif: 20 mutasi — (1) tanpa saringan `Temporary`; (2) tanpa saringan aktif; (3) tanpa saringan dihapus; (4) cakupan hanya unit tujuan; (5) cakupan dimatikan; (6) filter unit di luar cakupan tidak ditolak; (7) unit asal dari penempatan bukan utama; (8) unit asal tanpa syarat sudah mulai; (9) unit asal tanpa syarat belum berakhir; (10) `Ended` termasuk hari ini; (11) `Upcoming` termasuk hari ini; (12) status respons bergeser sehari; (13) rentang awal membuang penempatan tanpa akhir; (14) rentang akhir diabaikan; (15) batas 100 dibuang; (16) filter unit hanya tujuan; (17) `timeStatus` tidak dikenal tidak ditolak; (18) cari nomor surat dibuang; (19) urutan terbalik; (20) controller memakai resource lain | 20/20 tertangkap tanpa galat kompilasi. AC1 oleh (4), (7), (8); AC2 oleh (1)–(8), (16); AC3 oleh (1)–(3), (10)–(12), (17); AC4 oleh (20); unit asal oleh (7)–(9); rentang, cari, dan paginasi oleh (13)–(15), (18), (19). Kedua berkas dipulihkan dengan `writeFileSync` (mtime baru), hash sama dengan salinan asli | `PASS` | `mut359.cjs`, `mut359-hasil.log` di scratchpad sesi |
| Regresi penuh proyek uji HR | 445/445 lulus (12 menit 53 detik) sesudah build ulang dari source asli, dengan env `QUILVIAN_BACKEND_ROOT`. Dijalankan dari salinan tanpa empat uji payroll yang tidak terkompilasi sejak merge `815ce085`; salinan selain itu identik dengan folder uji repo | `PASS` | `test359-full.log` |
| Build proyek utama | 0 galat (`-p:RunAnalyzers=False`) | `PASS` | `build359-final.log` |
| QBE `Strict` atas 5 berkas (termasuk `Program.cs`) | 0 violation, 0 review. Probe `TrxZzzQbeProbe359` (ber-`[Table]`) terdeteksi 4 violation lalu dihapus | `PASS` | Keluaran checker |
| Subkueri unit asal pada PostgreSQL | Hanya terbukti di harness SQLite | `NOT RUN` | Bukti API devYoga di `BE-HRD-455` |

### Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Raka ICU → IGD tampil bagi Kepala Unit ICU dan IGD, dengan unit asal dan tujuan | Terpenuhi | `Penempatan_tampil_bagi_Kepala_Unit_asal_dan_tujuan`; ketepatan unit asal: `Unit_asal_dari_penempatan_utama_yang_berlaku_pada_tanggal_mulai` |
| 2. Penempatan unit lain tidak tampak; filter unit di luar cakupan `403` | Terpenuhi | `Penempatan_unit_lain_tidak_tampak_dan_filter_di_luar_cakupan_ditolak` |
| 3. Status waktu `Upcoming`/`Ongoing`/`Ended` sesuai tanggal WIB; penempatan bukan `Temporary` atau dihapus tidak tampil | Terpenuhi | `Status_waktu_dan_hanya_penempatan_sementara_aktif`, `Batas_hari_status_waktu` |
| 4. Tanpa `TemporaryAssignment : Read` → `403` | Terpenuhi | `Tanpa_hak_TemporaryAssignment_Read_ditolak` (anotasi kedua endpoint, registri modul Workforce Core, `PermissionRegistryValidator` bersih, pegawai tanpa kebijakan Akses Role ditolak dan superadmin lolos) |

---

## 4. Temuan

| Temuan | Dampak | Tindakan |
| --- | --- | --- |
| As-is: penempatan sementara hanya bisa dilihat per pegawai di Profil Pegawai 360 | Kepala Unit tidak punya daftar siapa yang sedang dipinjam dari atau ke unitnya | Diperbaiki (inti task) |
| Hak `TemporaryAssignment : Read` baru terdaftar di kode | Belum ada peran yang memegangnya sampai Admin Sistem memberikannya lewat Akses Role (Permission 34.3) | Butir DoD "hak akses sudah diberikan" tetap terbuka; daftar periksa: Kepala Unit, Kepala Bidang, Admin HRD |
| Pegawai tanpa penempatan utama pada tanggal mulai | Unit asal kosong; Kepala Unit asal tidak melihatnya | Risiko kartu, diterima; tetap tampil bagi unit tujuan |
| Subkueri unit asal dua kali per baris | Biaya kueri naik seiring jumlah penempatan sementara; paginasi tetap di basis data | Diterima untuk volume penempatan sementara; periksa rencana kueri PostgreSQL saat bukti `BE-HRD-455` |

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Migration / database | Tidak ada migration. Tidak ada eksekusi database |
| Git | Tidak ada stage, commit, push, pull, merge, rebase, atau deploy. Berkas baru: `TemporaryAssignmentController.cs`, `TemporaryAssignmentQueryService.cs`, `TemporaryAssignmentDtos.cs`, `TemporaryAssignmentQueryTests.cs` |
| Dampak frontend | Endpoint baru untuk layar Penugasan Tambahan (`FE-HRD-406`); label status waktu dari `filters/metadata` |
| Bukti data asli | Penempatan sementara dan akun Kepala Unit di devYoga dikerjakan di `BE-HRD-455` |
| Task berikutnya | `BE-HRD-453` (ESS izin pulang cepat), lalu `BE-HRD-454` |
