# Laporan Perubahan Backend — `BE-HRD-467`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-467` (sebelum penomoran ulang merge 26 November 2026 disebut pengguna sebagai `BE-HRD-454`; nomor itu kini milik evaluasi kandidat Paket B) |
| Judul | ESS surat tugas dinas luar |
| Slice | `A52-B` / `MVP-77` gelombang 3 · `EPIC HRD-133` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 32, kartu `BE-HRD-467` |
| Trace | `FR-PK-036`; `HRD-DEC-252`, `HRD-DEC-256` |
| Contract version | `v31` — API 37.5; Validation `VR-PK-33`, `VR-PK-34`, `VR-PK-36`; State 35.1; Permission 35.1, 35.4; desain `02` 39.3.3, 39.9.3, 39.10 — approved 24 November 2026 |
| Dependency | `BE-HRD-466` ✅ (pola ESS, `CancelSelfRequest`, pemeriksaan pemutus) |
| Klasifikasi | `STANDARD` — controller ESS baru, DTO ESS baru, metode service baru, satu aturan baru pada keputusan HR; tanpa entity, kolom, maupun migration |
| Task mode | `BACKEND` (instruksi pengguna 26 November 2026: "kerjakan task ini yaa lanjutkan") |
| Model | Claude Opus 5.5 |
| Source acuan | Backend `cce1dbf5` (branch `yoga`), working tree bersih sebelum task |
| Tanggal | 26 November 2026 |
| Status | ✅ Selesai — 4/4 acceptance criteria terpenuhi di harness; bukti API data asli devYoga ada di `BE-HRD-468` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `SelfServices` / `HumanResource` (controller dan DTO ESS) dan `Corporate` / `HumanResource` / `AttendanceManagement` (service, DTO, konstanta). Entity `HrdOfficialDutyRequest` yang sudah ada; task tidak menambah entity sehingga tidak butuh keputusan prefix |
| Keberlakuan | `NEW CODE` — `OfficialDutySelfServiceController`, `OfficialDutySelfServiceDtos`; `TOUCHED LEGACY` — `OfficialDutyRequestService` (enam method ESS, penjaga `VR-PK-36` di `ApproveAsync`/`RejectAsync`, pemetaan `CancelDateTime`), `OfficialDutyRequestDtos` (`CancelDateTime` pada respons), `AttendanceValueConstants` (`OfficialDutyCategory`), `HrAttachmentStorage` (`OwnerOf`) |
| QBE yang berlaku | `QBE-SVC-001` (aturan ESS, kategori, kepemilikan lampiran, pembatalan, dan larangan memutuskan sendiri di service; controller ESS hanya menentukan identitas sesi, menyimpan/membuka berkas lewat `HrAttachmentStorage`, memetakan respons, dan menulis log), `QBE-API-001` (`ApiResponse`, kode status kontrak 37.5), `QBE-PERM-001` (`[AccessController]` modul `HUMAN_RESOURCE_EMPLOYEE_SELF_SERVICE`, resource `MyOfficialDuty`, `[AccessAction]` + `[AccessPermission]` Read/Create/Cancel). Tanpa entity/tabel baru, sehingga `QBE-NAM-001`, `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-002` tidak tersentuh. Tidak ada `<summary>` pada action controller; `filters/metadata` dideklarasikan tepat sesudah konstruktor |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `CLAUDE.md` backend |
| Wewenang terpisah | Implementasi source ✅; migration ✖ (tidak diperlukan); eksekusi database ✖; deployment ✖ |

---

## 1. Cara kerja sesudah perubahan

Contoh: Sari (perawat ICU) ditugaskan mengawal pasien rujukan ke RSUP Dr. Sardjito pada 10 Februari pukul 08.00–16.00. Dodi adalah Kepala Ruang ICU.

| Langkah | Yang terjadi |
| --- | --- |
| Sari membuka Layanan Mandiri → Dinas Luar, memilih kategori dari `filters/metadata` (lima kategori dan empat status) | Kategori: Pengawalan Ambulans Rujukan, Kurir Spesimen & Laboratorium, Pelatihan/Seminar/Workshop, Pelayanan Pasien Home Care, Tugas Resmi Lainnya |
| Sari mengunggah foto surat tugas (`POST attachments`) | `201` dengan kunci `attendance-official-duty/…/{akun Sari}/…`. Jenis dan ukuran berkas mengikuti `HrAttachmentStorage` (PDF/gambar/Word/Excel, maks. 10 MB) |
| Sari mengajukan (`POST /`) dengan kunci lampiran itu | Profil diambil dari akun Sari; `workforceProfileId` di body diabaikan. Kategori wajib salah satu pilihan metadata; lampiran wajib hasil unggahan akun Sari sendiri. Lalu aturan pengajuan layar HR yang sama (jam selesai sesudah jam mulai, nomor surat/tujuan/uraian wajib, periode tertutup `409`). Surat tugas `Submitted`, tampil di daftar HR. Log `OfficialDuty.SelfCreate` memuat Id dan rentang, tanpa tujuan dan uraian |
| Sari mengunduh lampirannya (`GET attachments/download?url=…`) | `200` karena kunci itu lampiran surat tugas miliknya. Lampiran surat tugas pegawai lain, unggahan yang belum dilampirkan, atau kunci asal → `404` "Lampiran tidak ditemukan." |
| Akun tanpa profil pegawai aktif membuka endpoint ESS mana pun | `404` "Akun ini belum terhubung ke profil pegawai." (unggah pun ditolak sebelum berkas disimpan) |
| Sari membatalkan surat tugasnya yang masih `Submitted` dengan alasan | `Cancelled`, `IsCancel`, `CancelBy`, `CancelDateTime` terisi. Log `SelfCancel` memuat Id, rentang, dan status sebelumnya |
| Sari membatalkan lagi, atau membatalkan surat tugas yang sudah disetujui | `409` "Pengajuan sudah diputuskan dan tidak dapat dibatalkan." |
| Dodi mencoba membatalkan surat tugas Sari lewat ESS | `404` (bukan miliknya) |
| Dodi mengajukan surat tugasnya sendiri, lalu menyetujui atau menolaknya di layar HR | `403` "Anda tidak dapat memutuskan pengajuan Anda sendiri." Identitas pemutus dari akun login, bukan dari `approverWorkforceProfileId` di body; tidak ada rekap OnDuty yang dibuat |
| Dodi menyetujui surat tugas Sari | Berjalan seperti sebelumnya (rekap OnDuty 10 Februari) |

---

## 2. Perubahan

| Berkas | Isi |
| --- | --- |
| `Areas/SelfServices/HumanResource/Controllers/OfficialDutySelfServiceController.cs` (baru) | Tujuh endpoint: `GET filters/metadata`, `GET /`, `GET {id}`, `POST attachments`, `GET attachments/download`, `POST /`, `POST {id}/cancel`. Modul `HUMAN_RESOURCE_EMPLOYEE_SELF_SERVICE`, resource `MyOfficialDuty` (`Read`/`Create`/`Cancel`), `SortOrder = 14`. Profil dari `HumanResourceContextService.GetCurrentAsync` (akun tidak sah → `401`). Respons dipetakan ke `OfficialDutySelfResponse`. Log `AuditAsync` `OfficialDuty.SelfCreate`/`SelfCancel` dengan isi di teks pesan |
| `Areas/SelfServices/HumanResource/DTOs/OfficialDutySelfServiceDtos.cs` (baru) | `OfficialDutySelfQuery`, `CreateOfficialDutySelfRequest` (tanpa ruas pegawai), `OfficialDutySelfFilterMetadataResponse`, `OfficialDutySelfOptionResponse`, `OfficialDutySelfResponse` (tanpa data pegawai lain), label status `OfficialDutySelfStatus`, label kategori `OfficialDutySelfCategory`. `CancelSelfRequest` dipakai ulang dari `BE-HRD-466` |
| `Areas/Corporate/HumanResource/AttendanceManagement/Services/OfficialDutyRequestService.cs` | Baru: `GetPagedForSelfAsync`, `GetByIdForSelfAsync`, `CreateForSelfAsync`, `CancelBySelfAsync`, `EnsureOwnAttachmentForSelfAsync`, `EnsureSelfProfileAsync`, `IsActiveProfileAsync`, `IsOwnRequestAsync`. `ApproveAsync`/`RejectAsync`: penjaga `VR-PK-36` sesudah pemeriksaan "tidak ditemukan", sebelum pemeriksaan status. Daftar dan detail mengisi `CancelDateTime` |
| `…/AttendanceManagement/DTOs/OfficialDutyRequestDtos.cs` | `OfficialDutyResponse.CancelDateTime` (baru, nullable) |
| `…/AttendanceManagement/Constants/AttendanceValueConstants.cs` | `OfficialDutyCategory` — lima nilai yang selama ini hanya tertulis di komentar entity dan dipakai layar HR |
| `Shared/HumanResource/Helpers/HrAttachmentStorage.cs` | `OwnerOf(key)` statis (pasangan `CategoryOf`) untuk memeriksa pengunggah lampiran tanpa membuka berkas |
| `Tests/QuilvianSystemBackend.HumanResourceTests/OfficialDutySelfServiceTests.cs` (baru), `AcceptanceTestIndex.md` | 7 uji lewat controller ESS, service nyata, dan `HrAttachmentStorage` nyata di folder sementara; baris `AT-HRD-PK-50` dan baris kelas di indeks |
| `Tests/QuilvianSystemBackend.HumanResourceTests/Infrastructure/TestInfrastructure.cs` | Shim SQLite melepas CHECK yang memakai `btrim(` — lihat bagian 4 |

Tidak ada registrasi DI baru: `OfficialDutyRequestService`, `HumanResourceContextService`, `HrAttachmentStorage`, dan `LoggerService` sudah terdaftar.

### Delta terhadap kartu roadmap dan kontrak

| Hal | Kartu/kontrak | Pelaksanaan | Alasan |
| --- | --- | --- | --- |
| **Alasan batal** | `VR-PK-33`: alasan batal wajib (maks. 500) | Divalidasi (`400`), **tidak disimpan** | Celah kontrak yang sama dengan `BE-HRD-466`: tidak ada kolom, ruas respons, atau ruas log untuknya. Perlu keputusan pass blueprint |
| Parameter unduh | API 37.5: query `url` | `url` (layar HR memakai `key`) | Mengikuti kontrak ESS; endpoint HR tidak diubah |
| Unduh lampiran | "milik pengajuan sendiri; selain itu `404`" | Hanya kunci yang tercatat di `AttachmentUrl` surat tugas milik profil sesi (status apa pun, tidak dihapus). Unggahan sendiri yang belum dilampirkan → `404` | Tafsiran harfiah kontrak; pegawai tetap bisa mengunduh lampiran yang diunggah HR atas surat tugasnya |
| Lampiran saat mengajukan | "hasil unggah `/attachments` milik sendiri" | Kunci wajib kategori `attendance-official-duty` dan pengunggahnya akun sesi (dibaca dari kunci lewat `OwnerOf`); tautan `http(s)` lama ditolak `400`. Keberadaan berkas tidak diperiksa (sama dengan layar HR) | Mencegah pegawai menautkan berkas orang lain ke surat tugasnya lalu mengunduhnya |
| Kategori | "salah satu pilihan metadata" | Ditegakkan di `CreateForSelfAsync` dari `AttendanceValueConstants.OfficialDutyCategory`; layar HR tetap tanpa pemeriksaan kategori | Aturan baru hanya untuk ESS sesuai kontrak; mengubah HR di luar lingkup |
| Panjang isian | Maks. 100 (nomor), 255 (tujuan) | Juga diperiksa di service (`400`) | Atribut DTO hanya berlaku lewat model binding; service menjaga agar basis data tidak menolak dengan `500` |
| Pengajuan ganda | Kode status 37.4/37.5 menyebut `409` izin ganda | Tidak ada pemeriksaan tumpang tindih surat tugas | Kontrak 37.5 merujuk "aturan yang ada"; `CreateAsync` tidak punya aturan ganda. Bila diperlukan, keputusan pass blueprint |
| Unggah tanpa profil | `VR-PK-34` semua endpoint 37.4–37.5 | `404` sebelum berkas disimpan | Tidak menyisakan berkas yatim dari akun tanpa profil |
| `CancelDateTime` pada respons HR | Tidak disebut | Ruas baru, nullable | Respons ESS membutuhkannya; sama dengan `BE-HRD-466` |
| Rujukan kode status | 37.5: "Kode status sama dengan 36.4" | Dibaca sebagai 37.4 | 36.4 adalah evaluasi kandidat Paket B; salah rujuk di kontrak, dicatat untuk pass blueprint |
| Status kontrak API 37.5 | "**Rencana (belum tersedia)**" | Tidak disunting | Artefak kontrak terkunci hash. Penyegaran status dicatat untuk pass blueprint |

---

## 3. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `OfficialDutySelfServiceTests` + `EarlyLeaveSelfServiceTests` | 13/13 lulus | `PASS` (AC1–AC4) | Keluaran `dotnet test` (`test467.log`) |
| Kontrol negatif: 25 mutasi (service, controller, DTO) | 25/25 tertangkap oleh uji, tanpa galat build. Putaran pertama 24/25: mutasi 07 (pemeriksaan kategori lampiran dibuang) lolos karena uji kategori-lain memakai kunci milik pegawai lain sehingga sudah tertolak oleh pemeriksaan pengunggah. Uji diperkuat dengan unggahan milik sendiri dari kategori `travel-receipt`, lalu mutasi 07 diulang dan tertangkap. Hash SHA-256 ketiga berkas sumber sama dengan sebelum mutasi | `PASS` | `mut467.cjs`, `mut467-hasil-putaran1.log`, `mut467-hasil.log` di scratchpad sesi |
| Regresi penuh proyek uji HR | 458/458 lulus (baseline `BE-HRD-466` 451 + 7 uji baru), 21 menit 44 detik, dibangun ulang dari sumber yang sudah dipulihkan | `PASS` | `regresi467.log` di scratchpad sesi (salinan proyek uji tanpa empat berkas `WorkforcePayrollData*Tests.cs` yang tidak terkompilasi sejak `815ce085`) |
| Build proyek utama | 0 galat (`-p:RunAnalyzers=False`), 2 menit 45 detik | `PASS` | `build467.log` |
| QBE `Strict` atas 7 berkas | 0 violation, 0 review, `PASS`. Probe `TrxZzzQbeProbe467` (ber-`[Table]`) terdeteksi 4 violation (`QBE-NAM-001`, `-ENT-001`, `-CFG-001`, `-MOD-002`) lalu dihapus | `PASS` | Keluaran checker |
| Bukti API data asli devYoga | Tidak dijalankan di task ini | `NOT RUN` | Kartu menempatkannya di `BE-HRD-468` |

### Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Unggah `201`; ajukan `200` atas profil sesi; tampil di daftar HR | Terpenuhi | `Unggah_dan_ajukan_tercatat_atas_profil_sesi_dan_tampil_di_daftar_HR` (body JSON menyisipkan `workforceProfileId` milik Raka, tetapi surat tugas tetap tercatat atas profil Sari); validasi: `Ajukan_ditolak_bila_isian_tidak_sah_periode_tertutup_atau_tanpa_profil` |
| 2. Unduh lampiran sendiri `200`; lampiran pengajuan pegawai lain `404` | Terpenuhi | `Unduh_hanya_lampiran_pengajuan_sendiri` (termasuk unggahan belum dilampirkan, kunci traversal, kunci kosong) |
| 3. Batal mengikuti `VR-PK-33`; akun tanpa profil `404` | Terpenuhi | `Batal_hanya_milik_sendiri_yang_masih_diajukan`; `404` tanpa profil di empat endpoint pada uji validasi; daftar/detail: `Daftar_detail_dan_metadata_hanya_milik_sendiri` |
| 4. Pemutus = pemilik → `403` | Terpenuhi | `Pemohon_tidak_dapat_memutuskan_surat_tugasnya_sendiri`; hak akses ESS: `Hak_akses_MyOfficialDuty_terdaftar_dan_ditegakkan` |

---

## 4. Temuan

| Temuan | Dampak | Tindakan |
| --- | --- | --- |
| As-is: pegawai tidak bisa mengajukan surat tugas dinas luar sendiri; hanya HR yang mencatat atas nama | Menu Layanan Mandiri tertahan (`FE-HRD-100`) | Diperbaiki (inti task) |
| As-is: `ApproveAsync`/`RejectAsync` hanya memeriksa status; identitas pemutus dari body | Kepala Ruang dapat menyetujui surat tugasnya sendiri dan memicu rekap OnDuty | `VR-PK-36` ditambahkan berbasis akun login. `ApprovedByWorkforceId` masih disalin dari body (perilaku lama, di luar lingkup task) |
| Alasan batal wajib tetapi tidak ada tempat menyimpannya | Alasan yang diketik pegawai hilang | Celah kontrak yang sama dengan `BE-HRD-466` |
| Harness uji HR tidak bisa membentuk skema sejak merge `cce1dbf5`: CHECK `PrlOpeningTaxBalance`, `PrcVendorProfile`, `PrcVendorStatusRequest` memakai `btrim`, yang tidak dikenal SQLite → seluruh uji HR gagal di konstruktor | Tidak ada uji HR yang bisa dijalankan | `ModelUjiSqlite` kini juga melepas CHECK ber-`btrim(` (sama dengan `num_nonnulls`, `::`, regex). Hanya infrastruktur uji; aturan di PostgreSQL tidak berubah |
| Hak `MyOfficialDuty : Read/Create/Cancel` baru terdaftar di kode | Belum ada peran yang memegangnya | Admin Sistem memberikannya kepada Pegawai lewat Akses Role (Permission 35.3) |

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Migration / database | Tidak ada migration. Tidak ada eksekusi database |
| Git | Tidak ada stage, commit, push, pull, merge, rebase, atau deploy. Berkas baru: `OfficialDutySelfServiceController.cs`, `OfficialDutySelfServiceDtos.cs`, `OfficialDutySelfServiceTests.cs`, laporan ini |
| Perubahan sampingan | `TestInfrastructure.cs` (shim `btrim`) — perlu untuk menjalankan uji apa pun; dicatat di bagian 4 |
| Dampak frontend | Endpoint baru untuk `FE-HRD-410`/`FE-HRD-100`. Layar HR menerima `403` pada keputusan atas surat tugas sendiri dan ruas baru `cancelDateTime` |
| Bukti data asli | Akun pegawai dan Kepala Ruang di devYoga dikerjakan di `BE-HRD-468` |
| Task berikutnya | `BE-HRD-468` (verifikasi `A52-B`: registri tujuh hak, regresi HR, bukti devYoga atas izin G2) |
