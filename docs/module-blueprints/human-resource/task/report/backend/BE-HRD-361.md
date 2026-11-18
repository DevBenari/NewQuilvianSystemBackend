# Laporan Perubahan Backend — `BE-HRD-361`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-361` |
| Judul | Layanan detail isian penggajian satu orang untuk diubah (`WorkforcePayrollDataService.GetDetailAsync`) |
| Slice | `S-A8` / `MVP-69` — gelombang 1 |
| Roadmap | `roadmap/backend-roadmap.md` bagian Slice S-A8, kartu `BE-HRD-361` |
| Trace | `GET api/v1/corporate/human-resource/workforce-core/payroll-data/{workforceProfileId}`; `HRD-DEC-219` (detail hanya untuk pemegang hak ubah), `HRD-DEC-222` (jenis orang); Acceptance §35 `AT-HRD-WPD-*` |
| Contract version | `v27` bagian 33 — approved 16 November 2026 (`HRD-DEC-015`); tidak diubah |
| Dependency | `BE-HRD-003` ✅ [B1]; gerbang G1 tertutup |
| Klasifikasi | `SMALL` — skor 2: satu repository (0), sekitar 8 berkas diperiksa (1), 1 berkas source diubah satu baris (0), tanpa perubahan API/schema (0), data sensitif dibaca tetapi tidak diubah (0), ditambah 1 untuk verifikasi provider-nyata |
| Task mode | `BACKEND` (instruksi pengguna 17 November 2026: "lanjut" atas saran mengerjakan `BE-HRD-361`) |
| Target tulis | `Areas/Corporate/HumanResource/WorkforceCore/Controllers/WorkforcePayrollDataController.cs` (satu baris); uji baru `Tests/QuilvianSystemBackend.HumanResourceTests/WorkforcePayrollDataDetailTests.cs`; dokumen roadmap/traceability/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `b29bff1d` (branch `yoga`); source slice S-A8 sudah di-commit di `dc723f0f` ("feat(payroll): add Payroll Data Readiness functionality") dan bersih sebelum task ini; perubahan `BE-HRD-357`, `353`–`356`, `FIX-002`–`004` belum di-commit |
| Tanggal | 17 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria terbukti: uji baru 6/6, proyek uji HR 252/252, kontrol negatif empat mutasi tertangkap; lewat API devYoga (PostgreSQL) pemegang Read saja `403`, pemegang Update `200`, GUID tak dikenal `404` berpesan sesuai kontrak, bagian kosong `null`, nomor rekening lengkap sama dengan basis data |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `WorkforceCore` |
| Pemilik / prefix registry | `Human Resource` / `Wfp` (`WfpSalaryAssignment`, `WfpBankAccount`) dan master `Mst*` HRD; hanya baca, tanpa entity baru |
| Keberlakuan | `NEW CODE` (source slice S-A8, 16 November 2026); task ini hanya memverifikasi dan menyunting satu baris anotasi |
| QBE yang berlaku | `QBE-SVC-001` (akses data lewat service), `QBE-API-001` (route dan `ApiResponse`), `QBE-PERM-001` (`[AccessPermission]` = `ControllerName` + nama `[AccessAction]`), `QBE-DTO-001` (DTO sesuai kontrak) |
| Tidak berlaku | `QBE-DB-*` (tanpa schema/migration), `QBE-TXN-001`/`QBE-AUD-001`/`QBE-LOG-001` (baca saja, tanpa tulis dan tanpa log), `QBE-VAL-001` (tanpa isian) |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

---

## 1. Kondisi as-is terhadap kontrak

| Butir kontrak v27 §33 | Source (as-is) | Hasil |
| --- | --- | --- |
| Route `GET /{workforceProfileId}` (GUID), `ApiResponse<WorkforcePayrollDataDetailResponse>` | `WorkforcePayrollDataController.Detail`, `[HttpGet("{workforceProfileId:guid}")]` | Sesuai |
| Hak akses `WorkforcePayrollData : Update` | `[AccessAction("Update", ...)]` + `[AccessPermission("WorkforcePayrollData", "Update")]`; modul `HUMAN_RESOURCE_WORKFORCE_CORE` | Sesuai |
| `404` "Pegawai, perawat, atau dokter tidak ditemukan atau sudah tidak aktif." | `FindPersonAsync` → `WorkforcePayrollDataException(404, …)` → controller `StatusCode(404, ApiResponse.Fail)` | Sesuai |
| DTO detail: identitas, `salary` (`null` bila belum ada; struktur, grade + rentang, `baseSalary`, `effectiveStartDate`, `isApproved`, `approvedAt`), `bankAccount` (`null` bila belum ada; bank, `accountNumber` lengkap, pemilik, `isVerified`) | `GetDetailAsync` memetakan seluruh ruas itu | Sesuai |
| Penetapan dan rekening yang dipakai sama dengan daftar (Aturan B) | Penetapan utama aktif dengan tanggal mulai terbaru; rekening payroll aktif terverifikasi → utama → terbaru — urutan sama dengan `LoadOverviewAsync` | Sesuai |
| Layanan terdaftar di DI | `Program.cs:1128` `AddScoped<WorkforcePayrollDataService>()` | Sesuai |

Tidak ada delta perilaku terhadap kontrak; source tidak perlu diubah untuk memenuhi acceptance criteria.

---

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkforceCore/Controllers/WorkforcePayrollDataController.cs` | Menghapus `/// <summary>` XML pada action `Detail` (tampil sebagai deskripsi baris endpoint di Swagger, yang tidak dikehendaki pemilik produk). Perilaku tidak berubah. CRLF dipertahankan (129 CR = 129 baris); −1. `<summary>` pada action `Save` milik `BE-HRD-362` dan belum disentuh |
| `Tests/QuilvianSystemBackend.HumanResourceTests/WorkforcePayrollDataDetailTests.cs` (baru) | 6 uji: data yang dipakai (penetapan utama terbaru, rekening payroll terverifikasi; penetapan bukan-utama, nonaktif, dan rekening non-payroll diabaikan), bagian kosong `null`, jenis orang (perawat dari profesi, bidan = karyawan, dokter, karyawan sekaligus dokter = karyawan), `404` berpesan (GUID tak dikenal, karyawan nonaktif, dokter terhapus), hak akses (`Detail` = Update, daftar/ringkasan = Read, terdaftar di registri, lolos `PermissionRegistryValidator`), dan penegakan Read-saja ≠ Update |

---

## 3. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet test --filter WorkforcePayrollDataDetailTests` | 6/6 lulus | `PASS` | Keluaran `dotnet test` |
| — Detail Sari (grade G-10 Rp 10–16 juta): dua penetapan utama disetujui (2025 Rp 11 juta, 2026 Rp 12,5 juta) + bukan-utama Rp 20 juta + nonaktif Rp 30 juta; rekening payroll terverifikasi lama, payroll belum terverifikasi baru, non-payroll terverifikasi | Gaji pokok Rp 12.500.000 (2026), SS-01/G-10 dengan rentang 10–16 juta, disetujui; rekening `1234567890` terverifikasi lengkap (tanpa samaran); departemen dan jabatan terisi | `PASS` (AC1 bagian data) | idem |
| — Budi tanpa data; Budi hanya penetapan belum disetujui; Rina hanya rekening non-payroll | `salary`/`bankAccount` `null`; `isApproved` false, `approvedAt` null | `PASS` (AC3) | idem |
| — GUID tak dikenal, karyawan nonaktif berpenetapan, dokter terhapus | `WorkforcePayrollDataException` 404 dengan pesan kontrak | `PASS` (AC2) | idem |
| — Hak akses | `Detail` = (`WorkforcePayrollData`, `Update`); `List`/`Summary` = `Read`; Read dan Update terdaftar di modul `HUMAN_RESOURCE_WORKFORCE_CORE`; validator tanpa temuan; jabatan Read-saja ditolak Update, jabatan Update lolos, superadmin lolos | `PASS` (AC1 bagian 403) | idem |
| Kontrol negatif (empat putaran terpisah; berkas dipulihkan dari salinan dan dibandingkan byte demi byte tiap putaran) | `Detail` diturunkan ke Read → uji hak akses gagal; syarat `IsPrimary` penetapan dihapus → uji data gagal; urutan rekening terverifikasi dihapus → uji data gagal; syarat `IsActive` karyawan dihapus → uji 404 gagal | `PASS` | Keluaran `dotnet test` per mutasi |
| Proyek uji HR (`dotnet test -p:RunAnalyzers=False`, termasuk build penuh project aplikasi) | 252/252 lulus (246 + 6 baru); build 0 galat | `PASS` | Keluaran `dotnet test` |
| Startup Development | "Permission registry valid. 2457 identitas kanonik dari 2457 kemampuan terdaftar." | `PASS` | `be-361.log` |
| API devYoga — populasi | 28 karyawan aktif, **0** dengan penetapan gaji atau rekening payroll; 1 dokter (bukan karyawan) dengan rekening payroll | Catatan | `at-361.mjs`, `at-log.jsonl` ("361 …") |
| — Superadmin: karyawan tanpa data | `200`, `Karyawan`, `salary` `null`, `bankAccount` `null` | `PASS` (AC3) | idem |
| — Superadmin: dokter | `200`, `Dokter`; `accountNumber`, pemilik, dan status verifikasi sama dengan baris basis data, tanpa samaran; daftar menampilkan `****` + empat karakter terakhir (nilai tidak dicetak ke bukti) | `PASS` (AC1 bagian data) | `at-361b.mjs` |
| — Superadmin: GUID tak dikenal | `404` "Pegawai, perawat, atau dokter tidak ditemukan atau sudah tidak aktif." (sama persis dengan kontrak) | `PASS` (AC2) | `at-361.mjs` |
| — gilang (non-superadmin), jabatan diberi `WorkforcePayrollData : Read` saja (izin sementara, kebijakan asli dikirim ulang utuh + tambahan) | Detail **`403`** "Anda tidak memiliki akses ke menu atau fitur ini.", tanpa data; daftar `200` | `PASS` (AC1) | idem |
| — gilang dengan Read + Update | Detail `200` | `PASS` (AC1) | idem |
| — gilang tanpa izin payroll-data | Detail `403` | `PASS` | idem |
| Pemulihan devYoga | Izin jabatan gilang dikirim ulang dari cadangan (32 butir) dan dibaca **identik**. Tidak ada data lain yang ditulis | `PASS` | `at-log.jsonl` ("361 Izin gilang dikembalikan") |

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Pemegang hak Update menerima gaji pokok dan nomor rekening lengkap; pemegang Read saja ditolak (403) | Terpenuhi | Harness (gaji pokok + nomor lengkap; penegakan izin); devYoga (403/200 akun non-superadmin; nomor rekening dokter lengkap) |
| 2. Orang yang bukan karyawan atau dokter aktif → 404 berpesan | Terpenuhi | Harness (tak dikenal, nonaktif, terhapus); devYoga (tak dikenal) |
| 3. Bagian yang belum ada dikembalikan null | Terpenuhi | Harness + devYoga |
| Migration | Tidak ada migration dan tidak ada eksekusi database selain izin sementara yang dipulihkan | — |

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Batas bukti | Gaji pokok lewat API PostgreSQL belum terlihat karena devYoga belum punya satu pun penetapan gaji; jalur itu terbukti di harness dan akan terbukti di PostgreSQL saat `BE-HRD-362` menyimpan penetapan uji |
| Gerbang G4 | Bagian backend untuk `BE-HRD-361` terpenuhi (build + uji runtime). Bagian frontend (`npm run build`, uji browser) milik task FE |
| Pengamatan (tidak diubah) | Proyeksi karyawan memakai navigasi wajib `PrimaryDepartment`/`PrimaryPosition` (INNER JOIN); karyawan aktif tanpa baris departemen/jabatan tidak akan ditemukan (`404`). Di basis data nyata FK menjamin barisnya ada; di harness SQLite (FK dimatikan) baris itu harus dibuat |
| Untuk `BE-HRD-362` | `<summary>` XML pada action `Save` sebaiknya dihapus juga (preferensi yang sama) |
| Risiko tersisa | Detail memuat data sensitif; dijaga hak Update. Tidak ada pencatatan akses baca detail (tidak diminta kontrak) |
| Perubahan sampingan | `NONE`. Mutasi kontrol negatif dipulihkan; izin devYoga dipulihkan identik |
| Interupsi | `NONE` |
| Status Git | 1 berkas source diubah (−1 baris), 1 berkas uji baru, laporan ini (baru), dokumen roadmap/traceability. Tidak ada commit |
| Task berikutnya | `BE-HRD-360` (daftar dan ringkasan; gerbang G3) atau `BE-HRD-362` (simpan; bergantung pada 361 ✅) |
