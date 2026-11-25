# Laporan Perubahan Backend — `BE-HRD-453`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-453` |
| Judul | ESS izin pulang cepat dan larangan memutuskan pengajuan sendiri |
| Slice | `A52-B` / `MVP-74` gelombang 2 · `EPIC HRD-127` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 30, kartu `BE-HRD-453` |
| Trace | `FR-PK-035`; `HRD-DEC-241`, `HRD-DEC-245`; fakta `HRD-TF-091`, `HRD-TF-094` |
| Contract version | `v30` — API 36.4; Validation `VR-PK-32` s.d. `VR-PK-34`, `VR-PK-36`; State 34.1; Permission 34.1; Integration `INT-HR-115`; desain `02` 38.4, 38.9.3, 38.11; kamus data 31 — approved 24 November 2026 |
| Dependency | Kontrak `v30` ✅ |
| Klasifikasi | `STANDARD` — controller ESS baru, DTO ESS baru, metode service baru, satu aturan baru pada keputusan HR; tanpa entity, kolom, maupun migration |
| Task mode | `BACKEND` (instruksi pengguna 25 November 2026: "lanjutkan BE-HRD-453") |
| Model | Claude Opus 5.5 |
| Source acuan | Backend `4d2c0162` (branch `yoga`). Working tree sebelum task memuat `BE-HRD-450` s.d. `BE-HRD-452` dan `BE-HRD-359` (belum di-commit) serta dokumen amandemen `A52-B`; tidak disentuh task ini |
| Tanggal | 25 November 2026 |
| Status | ✅ Selesai — 4/4 acceptance criteria terpenuhi di harness; bukti API data asli devYoga ada di `BE-HRD-455` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `SelfServices` / `HumanResource` (controller dan DTO ESS) dan `Corporate` / `HumanResource` / `AttendanceManagement` (service). Entity `HrdEarlyLeavePermission` yang sudah ada; task tidak menambah entity sehingga tidak butuh keputusan prefix |
| Keberlakuan | `NEW CODE` — `EarlyLeavePermissionSelfServiceController`, `EarlyLeavePermissionSelfServiceDtos`; `TOUCHED LEGACY` — `EarlyLeavePermissionService` (empat method ESS, penjaga `VR-PK-36` di `ApproveAsync`/`RejectAsync`, pemetaan `CancelDateTime`), `EarlyLeavePermissionDtos` (`CancelDateTime` pada respons) |
| QBE yang berlaku | `QBE-SVC-001` (aturan ESS, pembatalan, dan larangan memutuskan sendiri di service; controller ESS hanya menentukan identitas sesi, memetakan respons, dan menulis log), `QBE-API-001` (`ApiResponse`, kode status kontrak 36.4), `QBE-PERM-001` (`[AccessController]` modul `HUMAN_RESOURCE_EMPLOYEE_SELF_SERVICE`, resource `MyEarlyLeavePermission`, `[AccessAction]` + `[AccessPermission]` Read/Create/Cancel). Tanpa entity/tabel baru, sehingga `QBE-NAM-001`, `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-002` tidak tersentuh. Tidak ada `<summary>` pada action controller; `filters/metadata` dideklarasikan tepat sesudah konstruktor |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `CLAUDE.md` backend |
| Wewenang terpisah | Implementasi source ✅; migration ✖ (tidak diperlukan); eksekusi database ✖; deployment ✖ |

---

## 1. Cara kerja sesudah perubahan

Contoh: Sari (perawat ICU) perlu pulang pukul 12.00 karena anaknya sakit. Dodi adalah Kepala Ruang ICU.

| Langkah | Yang terjadi |
| --- | --- |
| Sari membuka Layanan Mandiri → Izin Pulang Cepat dan mengajukan untuk 10 Februari pukul 12.00 (`POST self-services/human-resource/early-leave-permissions`) | Profil diambil dari akun Sari. Rekap harian Sari pada 10 Februari dicari, lalu aturan pengajuan yang sama dengan layar HR berjalan (jam tidak sebelum jam masuk, tidak ganda, alasan wajib). Izin tercatat `Submitted` dan tampil di daftar HR. Log `EarlyLeavePermission.SelfCreate` memuat Id izin, tanggal, dan jam izin, tanpa alasan |
| Klien mengirim `workforceProfileId` pegawai lain di body | Diabaikan; izin tetap atas profil Sari |
| Sari mengajukan untuk tanggal tanpa jadwal/rekap | `400` "Belum ada jadwal atau rekap kehadiran Anda pada tanggal ini." |
| Sari mengajukan untuk tanggal di periode yang sudah ditutup (atau rekapnya terkunci) | `409` "Periode kehadiran tanggal ini sudah ditutup." |
| Akun tanpa profil pegawai aktif membuka ESS | `404` "Akun ini belum terhubung ke profil pegawai." |
| Sari membatalkan izinnya yang masih `Submitted` dengan alasan | Status `Cancelled`, `IsCancel`, `CancelBy`, `CancelDateTime` terisi. Rekap harian tidak lagi menunjuk izin itu (sama dengan penolakan). Log `SelfCancel` memuat Id izin dan status sebelumnya |
| Sari membatalkan lagi, atau membatalkan izin yang sudah disetujui | `409` "Pengajuan sudah diputuskan dan tidak dapat dibatalkan." |
| Dodi mencoba membatalkan izin Sari lewat ESS | `404` (bukan miliknya) |
| Dodi mengajukan izinnya sendiri, lalu menyetujui atau menolaknya di layar HR | `403` "Anda tidak dapat memutuskan pengajuan Anda sendiri." Identitas pemutus diambil dari akun login, bukan dari `approverWorkforceProfileId` di body |
| Dodi menyetujui izin Sari | Berjalan seperti sebelumnya |

---

## 2. Perubahan

| Berkas | Isi |
| --- | --- |
| `Areas/SelfServices/HumanResource/Controllers/EarlyLeavePermissionSelfServiceController.cs` (baru) | Lima endpoint: `GET filters/metadata`, `GET /`, `GET {id}`, `POST /`, `POST {id}/cancel`. Modul `HUMAN_RESOURCE_EMPLOYEE_SELF_SERVICE`, resource `MyEarlyLeavePermission` (`Read`/`Create`/`Cancel`), `SortOrder = 13`. Profil dari `HumanResourceContextService.GetCurrentAsync` (akun tidak sah → `401`). Respons dipetakan ke `EarlyLeaveSelfResponse`. Log `AuditAsync` `EarlyLeavePermission.SelfCreate`/`SelfCancel` dengan isi di teks pesan |
| `Areas/SelfServices/HumanResource/DTOs/EarlyLeavePermissionSelfServiceDtos.cs` (baru) | `EarlyLeaveSelfQuery`, `CreateEarlyLeaveSelfRequest` (tanpa ruas pegawai/rekap), `CancelSelfRequest` (dipakai juga `BE-HRD-454`), `EarlyLeaveSelfFilterMetadataResponse`, `EarlyLeaveSelfOptionResponse`, `EarlyLeaveSelfResponse` (tanpa data pegawai lain), label status `EarlyLeaveSelfStatus` (Diajukan, Disetujui, Ditolak, Dibatalkan) |
| `Areas/Corporate/HumanResource/AttendanceManagement/Services/EarlyLeavePermissionService.cs` | Baru: `GetPagedForSelfAsync`, `GetByIdForSelfAsync`, `CreateForSelfAsync`, `CancelBySelfAsync`, `IsActiveProfileAsync`, `IsOwnRequestAsync`. `ApproveAsync`/`RejectAsync`: penjaga `VR-PK-36` sesudah pemeriksaan "tidak ditemukan", sebelum pemeriksaan status. Daftar dan detail mengisi `CancelDateTime` |
| `…/AttendanceManagement/DTOs/EarlyLeavePermissionDtos.cs` | `EarlyLeavePermissionResponse.CancelDateTime` (baru, nullable) |
| `Tests/QuilvianSystemBackend.HumanResourceTests/EarlyLeaveSelfServiceTests.cs` (baru), `AcceptanceTestIndex.md` | 6 uji lewat controller ESS dan service nyata; baris `AT-HRD-PK-49` dan baris kelas di indeks |

Tidak ada registrasi DI baru: `EarlyLeavePermissionService`, `HumanResourceContextService`, dan `LoggerService` sudah terdaftar.

### Delta terhadap kartu roadmap dan kontrak

| Hal | Kartu/kontrak | Pelaksanaan | Alasan |
| --- | --- | --- | --- |
| **Alasan batal** | `VR-PK-33`/38.9.3: alasan batal wajib (maks. 500) | Divalidasi (`400` bila kosong atau lebih dari 500), **tidak disimpan** di mana pun | Kamus data 31 menegaskan tidak ada kolom baru dan pembatalan hanya mengisi `IsCancel`/`CancelBy`/`CancelDateTime`; respons 36.4 tidak memuat alasan batal; matriks audit 34 menetapkan isi log `SelfCancel` hanya Id dan status sebelum. **Celah kontrak: perlu keputusan pass blueprint** (kolom baru dengan migration, atau mencabut kewajiban alasan) |
| Penentu pemutus `VR-PK-36` | "profil pegawai pengguna = `WorkforceProfileId` pengajuan" | Profil dari `Users.WorkforceProfileId` akun login | `approverWorkforceProfileId` di body bisa diisi sembarang; aturan pemisahan tugas tidak boleh bergantung pada isian klien. Akun tanpa profil (mis. superadmin) tidak terhalang |
| Rekap terkunci | `VR-PK-32`: "belum terkunci … atau periodenya tertutup → 409" | `IsLocked` pada rekap atau periode `Closed` → `409` dengan pesan kontrak | Sesuai kontrak. `CreateAsync` lama hanya memeriksa periode tertutup dengan pesan lain; ESS memeriksa lebih dulu agar pesannya sesuai kontrak |
| Lebih dari satu rekap pada tanggal yang sama | Tidak disebut | Diambil rekap dengan jam masuk terjadwal paling awal | Model mengizinkan lebih dari satu rekap per hari; pilihan deterministik |
| Pembatalan melepas tautan rekap | State 34.1: "tidak memengaruhi rekap harian" | `HrdAttendanceDaily.EarlyLeavePermissionId` dilepas bila menunjuk izin itu (seperti `RejectAsync`); status kehadiran tidak berubah | Tautan yang tertinggal menunjuk izin yang tidak berlaku; status dan menit kehadiran memang tidak disentuh |
| `CancelDateTime` pada respons HR | Tidak disebut | Ruas baru, nullable | Respons ESS membutuhkannya; layar HR ikut mendapatkannya tanpa perubahan lain |
| Status kontrak API 36.4 | "**Rencana (belum tersedia)**" | Tidak disunting | Artefak kontrak terkunci hash. Penyegaran status dicatat untuk pass blueprint |

---

## 3. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `EarlyLeaveSelfServiceTests` + `AttendanceDashboardTests` (pemakai izin pulang cepat) | 12/12 lulus | `PASS` (AC1–AC4) | Keluaran `dotnet test` |
| Kontrol negatif: 21 mutasi — (1) penjaga `VR-PK-36` setuju dibuang; (2) penjaga tolak dibuang; (3) pemutus dari body; (4) rekap dicari tanpa saringan pegawai; (5) tanpa rekap `404`; (6) rekap terkunci diabaikan; (7) periode tertutup diabaikan; (8) profil nonaktif diterima; (9) batal tanpa saringan pemilik; (10) batal tanpa syarat `Submitted`; (11) alasan batal tidak dicek; (12) tautan rekap tidak dilepas; (13) `IsCancel` tidak diisi; (14) detail tanpa pemeriksaan pemilik; (15) daftar tanpa saringan pegawai; (16) batas 500 karakter alasan dibuang; (17) `CancelBy` tidak diisi; (18) log `SelfCreate` memuat alasan; (19) aksi log `SelfCancel` salah; (20) endpoint ajukan memakai hak `Read`; (21) label `Cancelled` salah | 21/21 tertangkap. AC1 oleh (4), (18); AC2 oleh (4)–(8), (16); AC3 oleh (4), (9)–(13), (17), (19), (21); AC4 oleh (1)–(4); daftar/detail oleh (4), (14), (15); hak akses oleh (20). Pada putaran pertama mutasi (18), (20), (21) gagal build karena sesi lain sedang menyunting dan mem-build modul Laboratorium di working tree yang sama (`LabOrderService.cs` sementara merujuk method yang belum ada; `obj/…sourcelink.json` terkunci); ketiganya diulang sesudahnya dan tertangkap. Ketiga berkas dipulihkan dengan `writeFileSync` (mtime baru), hash sama dengan salinan asli | `PASS` | `mut453.cjs`, `mut453-hasil.log`, `mut453-ulang.log` di scratchpad sesi |
| Regresi penuh proyek uji HR | 451/451 lulus (13 menit 15 detik) sesudah build ulang dari source asli, dengan env `QUILVIAN_BACKEND_ROOT`. Dijalankan dari salinan tanpa empat uji payroll yang tidak terkompilasi sejak merge `815ce085`; salinan selain itu identik dengan folder uji repo. Build mencakup suntingan modul Laboratorium milik sesi lain yang ada di working tree saat itu | `PASS` | `test453-full.log` |
| Build proyek utama | 0 galat (`-p:RunAnalyzers=False`) | `PASS` | `build453-final.log` |
| QBE `Strict` atas 5 berkas | 0 violation, 0 review. Probe `TrxZzzQbeProbe453` (ber-`[Table]`) terdeteksi 4 violation lalu dihapus | `PASS` | Keluaran checker |
| Bukti API data asli devYoga | Tidak dijalankan di task ini | `NOT RUN` | Kartu menempatkannya di `BE-HRD-455` |

### Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Ajukan dengan `workforceProfileId` pegawai lain di body → tercatat atas profil sesi; tampil di daftar HR | Terpenuhi | `Ajukan_tercatat_atas_profil_sesi_dan_tampil_di_daftar_HR` (JSON memuat `workforceProfileId` dan `attendanceDailyId` milik Raka) |
| 2. Tanpa rekap pada tanggal itu → `400`; periode tertutup → `409`; akun tanpa profil → `404` | Terpenuhi | `Ajukan_ditolak_bila_rekap_tidak_ada_periode_tertutup_atau_tanpa_profil` (termasuk rekap terkunci, profil nonaktif, dan aturan `CreateAsync` yang tetap berlaku) |
| 3. Batal `Submitted` → `Cancelled`; batal kedua dan batal sesudah `Approved` → `409`; batal milik orang lain → `404` | Terpenuhi | `Batal_hanya_milik_sendiri_yang_masih_diajukan`; daftar/detail milik sendiri: `Daftar_dan_detail_hanya_milik_sendiri` |
| 4. Kepala Unit menyetujui atau menolak izinnya sendiri → `403`; izin pegawai lain tetap bisa diputuskan | Terpenuhi | `Pemohon_tidak_dapat_memutuskan_izinnya_sendiri`; hak akses ESS: `Hak_akses_MyEarlyLeavePermission_terdaftar_dan_ditegakkan` |

---

## 4. Temuan

| Temuan | Dampak | Tindakan |
| --- | --- | --- |
| As-is: pegawai tidak bisa mengajukan izin pulang cepat sendiri; hanya HR yang mencatat atas nama (`HRD-TF-091`) | Menu Layanan Mandiri tertahan (`FE-HRD-100`) | Diperbaiki (inti task) |
| As-is: `ApproveAsync`/`RejectAsync` hanya memeriksa status; identitas pemutus dari body | Kepala Ruang dapat menyetujui izinnya sendiri; `approvedByWorkforceId` dapat diisi siapa saja | `VR-PK-36` ditambahkan berbasis akun login. `ApprovedByWorkforceId` masih disalin dari body (perilaku lama, di luar lingkup task) |
| Alasan batal wajib tetapi tidak ada tempat menyimpannya | Alasan yang diketik pegawai hilang | Celah kontrak, lihat delta. Perlu keputusan pass blueprint |
| Hak `MyEarlyLeavePermission : Read/Create/Cancel` baru terdaftar di kode | Belum ada peran yang memegangnya | Admin Sistem memberikannya kepada Pegawai lewat Akses Role (Permission 34.3) |

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Migration / database | Tidak ada migration. Tidak ada eksekusi database |
| Git | Tidak ada stage, commit, push, pull, merge, rebase, atau deploy. Berkas baru: `EarlyLeavePermissionSelfServiceController.cs`, `EarlyLeavePermissionSelfServiceDtos.cs`, `EarlyLeaveSelfServiceTests.cs` |
| Dampak frontend | Endpoint baru untuk `FE-HRD-100` (dibuka kembali). Layar HR menerima `403` pada keputusan atas izin sendiri dan ruas baru `cancelDateTime` |
| Bukti data asli | Akun pegawai dan Kepala Ruang di devYoga dikerjakan di `BE-HRD-455` |
| Task berikutnya | `BE-HRD-454` (ESS surat tugas dinas luar) memakai pola ini dan `CancelSelfRequest` yang sama |
