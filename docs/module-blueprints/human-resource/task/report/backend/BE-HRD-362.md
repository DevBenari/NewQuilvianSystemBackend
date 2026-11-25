# Laporan Perubahan Backend — `BE-HRD-362`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-362` |
| Judul | Layanan simpan penetapan gaji utama dan rekening payroll satu orang (`WorkforcePayrollDataService.SaveAsync`) |
| Slice | `S-A8` / `MVP-69` — gelombang 2 |
| Roadmap | `roadmap/backend-roadmap.md` bagian Slice S-A8, kartu `BE-HRD-362` |
| Trace | `PUT api/v1/corporate/human-resource/workforce-core/payroll-data/{workforceProfileId}`; `HRD-DEC-218` (keputusan 1: perubahan mencabut persetujuan/verifikasi), Aturan C–E; Validation §32 `VR-WPD-01`..`16`; State §31; Acceptance §35 `AT-HRD-WPD-*` |
| Contract version | `v27` bagian 33 — approved 16 November 2026 (`HRD-DEC-015`); tidak diubah |
| Dependency | `BE-HRD-361` ✅ (17 November 2026) |
| Klasifikasi | `MEDIUM` — skor 3: satu repository (0), sekitar 10 berkas diperiksa (1), 2 berkas source diubah sedikit (0), tanpa perubahan API/schema (0), menulis data sensitif gaji dan rekening (1), ditambah 1 untuk verifikasi provider-nyata yang menulis data uji |
| Task mode | `BACKEND` (instruksi pengguna 17 November 2026 "lanjut" atas urutan 361 → 360 → 362 → 363; dilanjutkan 18 November 2026 "lanjutkan task yang belum selesai") |
| Target tulis | `WorkforcePayrollDataService.cs` (teks log audit), `WorkforcePayrollDataController.cs` (hapus `<summary>` action `Save`); uji baru `Tests/QuilvianSystemBackend.HumanResourceTests/WorkforcePayrollDataSaveTests.cs`; dokumen roadmap/traceability/indeks uji/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `a17a2bc3` (branch `yoga`); perubahan `BE-HRD-353`–`358`, `361`, `FIX-002`–`004` belum di-commit |
| Tanggal | 17–18 November 2026 |
| Status | ✅ Selesai — 7/7 acceptance criteria terbukti: uji baru 8/8, proyek uji HR 260/260, kontrol negatif lima mutasi tertangkap; lewat API devYoga (PostgreSQL) seluruh jalur simpan, tolak, cabut, opsi, dan log audit sesuai kontrak. Penolakan `403` opsi setuju/verifikasi terbukti di harness lewat controller dan `AccessPermissionService` asli (tidak diulang lewat akun non-superadmin) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `WorkforceCore` |
| Pemilik / prefix registry | `Human Resource` / `Wfp` (`WfpSalaryAssignment`, `WfpBankAccount`) dan master `Mst*` HRD (`MstSalaryStructure`, `MstSalaryGrade`, `MstBank` dibaca); tanpa entity baru |
| Keberlakuan | `NEW CODE` (source slice S-A8, 16 November 2026); task ini memverifikasi dan menyunting teks log serta anotasi |
| QBE yang berlaku | `QBE-SVC-001` (akses data lewat service), `QBE-API-001` (route dan `ApiResponse`), `QBE-PERM-001` (`[AccessPermission]` = `ControllerName` + nama `[AccessAction]`), `QBE-DTO-001`, `QBE-VAL-001` (pemeriksaan isian sebelum tulis), `QBE-TXN-001` (satu `SaveChangesAsync`), `QBE-AUD-001`/`QBE-LOG-001` (log audit tanpa data sensitif) |
| Tidak berlaku | `QBE-DB-*` (tanpa schema/migration) |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

---

## 1. Kondisi as-is terhadap kontrak

| Butir kontrak v27 | Source (as-is) | Hasil |
| --- | --- | --- |
| Route `PUT /{workforceProfileId}` (GUID), hak `WorkforcePayrollData : Update` | `WorkforcePayrollDataController.Save`, `[AccessPermission("WorkforcePayrollData", "Update")]` | Sesuai |
| `VR-WPD-11` permintaan tanpa bagian dan tanpa opsi → `400` "Tidak ada perubahan untuk disimpan." | Pemeriksaan pertama `SaveAsync` | Sesuai |
| Aturan C: seluruh isian diperiksa dulu, kesalahan digabung, tidak ada yang tersimpan | `ValidateSalaryAsync` + `ValidateBankAsync` mengisi satu daftar `errors`; lempar `400` sebelum entity diubah | Sesuai |
| `VR-WPD-05`/`06` tanggal mulai vs tanggal berakhir; periode beririsan dengan struktur dan grade sama | Diperiksa di `ValidateSalaryAsync` | Sesuai |
| `VR-WPD-13` opsi tanpa data → `400` berpesan | Sesuai teks matriks validasi | Sesuai |
| `VR-WPD-14`/`15` opsi setuju tanpa `WfpSalaryAssignment : Update`, verifikasi tanpa `WorkforceBankAccount : Update` → `403` berpesan | Pemeriksaan `HasAccessAsync` di controller sebelum layanan dipanggil; teks sama persis dengan matriks | Sesuai |
| `VR-WPD-16` / Aturan D: gaji di luar rentang grade → peringatan, tetap `200` | `warnings` "Gaji pokok di luar rentang grade …" | Sesuai |
| Keputusan 1 (`HRD-DEC-218`): perubahan mencabut persetujuan/verifikasi kecuali opsi dipilih | Cabang `ApprovedAt`/`IsVerified` + peringatan pencabutan | Sesuai |
| AC6: penetapan baru utama dan rahasia; rekening baru sebagai rekening payroll | `IsPrimary = true`, `IsConfidential = true`; `IsPayrollAccount = true` | Sesuai |
| Aturan E: log memuat pelaku dan hasil, tanpa nominal dan nomor rekening | **Delta**: teks log hanya "Data penggajian pegawai diperbarui."; orang dan hasil hanya ada di objek `data`, yang **tidak ditulis** `LoggerService` | Diperbaiki (bagian 2) |

---

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkforceCore/Services/WorkforcePayrollDataService.cs` | Teks log audit `WorkforcePayrollData.Update` kini memuat `WorkforceProfileId` dan hasil kedua bagian ("penetapan gaji: …; rekening payroll: …"). Pelaku diambil `LoggerService` dari klaim sesi; payload tidak memakai ruas `Id`/`UserId`/`Name`, jadi pelaku tidak tertimpa. Tanpa nominal, nomor rekening, atau nama pemilik. CRLF dipertahankan (719 CR = 719 baris); +3 −1 |
| `Areas/Corporate/HumanResource/WorkforceCore/Controllers/WorkforcePayrollDataController.cs` | Menghapus `/// <summary>` XML pada action `Save` (preferensi pemilik produk; deskripsi baris endpoint di Swagger). Perilaku tidak berubah. CRLF dipertahankan; −4 |
| `Tests/QuilvianSystemBackend.HumanResourceTests/WorkforcePayrollDataSaveTests.cs` (baru) | 8 uji: dibuat baru + simpan ulang "Tidak diubah" tanpa log (AC6), permintaan kosong dan bagian null (AC1), satu isian ditolak → tidak ada yang tersimpan + enam pesan sekaligus (AC2), aturan periode/opsi tanpa data/404, pencabutan dan opsi (AC3), rentang grade (AC5), isi log audit (AC7), `403` lewat controller nyata dengan `AccessPermissionService` nyata (AC4) |

---

## 3. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet test --filter WorkforcePayrollDataSaveTests` | 8/8 lulus | `PASS` | Keluaran `dotnet test` |
| — Grade G-10 Rp 10–16 juta: gaji Rp 9 juta | Tersimpan, satu peringatan "Gaji pokok di luar rentang grade G-10 (Rp 10.000.000 – Rp 16.000.000)." | `PASS` (AC5) | idem |
| — Rekening "1234567890" sudah ada (non-payroll), kirim gaji Rp 12 juta + rekening "1234-567-890" | `400` "Nomor rekening sudah tercatat untuk orang ini."; penetapan gaji juga tidak tersimpan | `PASS` (AC2) | idem |
| — Penetapan disetujui + rekening terverifikasi diubah tanpa opsi | "Diperbarui"/"Diperbarui", dua peringatan pencabutan; `ApprovedAt`/`ApprovedByUserId`/`VerifiedAt` dikosongkan. Dengan opsi: "Diperbarui dan disetujui"/"Diperbarui dan terverifikasi", pelaku tercatat | `PASS` (AC3) | idem |
| — Pegawai tanpa hak setuju/verifikasi (controller nyata) | `403` dengan pesan matriks; tidak ada baris dan tidak ada log; tanpa opsi `200`; superadmin dengan opsi `200` | `PASS` (AC4) | idem |
| — Log audit | Satu baris per simpan yang mengubah data; memuat id pelaku, `WorkforceProfileId`, hasil; tidak memuat `12345678` dalam tiga format, nomor rekening, atau nama pemilik | `PASS` (AC7) | idem |
| Kontrol negatif (lima putaran; berkas dipulihkan dari salinan dan dibandingkan byte demi byte tiap putaran) | M1 pencabutan persetujuan dimatikan → AC3 gagal; M2 pemeriksaan `403` setuju dihapus → AC4 gagal; M3 batas bawah rentang dihapus → AC5 gagal; M4 nominal dan nomor ditambahkan ke log → AC7 gagal; M5 lempar galat dilewati bila gaji dikirim → AC2 dan uji periode gagal | `PASS` | Keluaran `dotnet test` per mutasi |
| Proyek uji HR (`dotnet test -p:RunAnalyzers=False`, termasuk build penuh project aplikasi) | 260/260 lulus (252 + 8 baru); build 0 galat | `PASS` | Keluaran `dotnet test` |
| Startup Development | "Permission registry valid. 2457 identitas kanonik dari 2457 kemampuan terdaftar." Satu-satunya `ERR` = penjadwal kehadiran tanpa `SystemActorUserId` (kondisi devYoga yang sudah dikenal, tidak terkait) | `PASS` | `be-362.log` |
| API devYoga — data uji (disetujui pengguna 18 November 2026) | Grade `SGR-RSMMC-00006` "Grade UJI-362" (Rp 10–16 juta) dan struktur `SST-RSMMC-00001` "Struktur UJI-362" dibuat lewat API master; sasaran karyawan Dewi Anggraeni (`EMP-RSMMC-00006`), sebelumnya 0 penetapan dan 0 rekening. Profil uji A52 tidak dipakai karena tidak tercatat di Master Karyawan | Catatan | `at-362.mjs`, `at-log.jsonl` ("362 …") |
| — Permintaan kosong | `400` "Tidak ada perubahan untuk disimpan." | `PASS` (AC1) | idem |
| — Gaji 0 tanpa tanggal + bank tak dikenal, nomor " - ", pemilik kosong | `400` dengan lima pesan sekaligus; 0 penetapan, 0 rekening | `PASS` (AC2) | idem |
| — Gaji sah Rp 12,5 juta + rekening tanpa pemilik | `400` "Nama pemilik rekening wajib diisi."; gaji juga tidak tersimpan (0/0) | `PASS` (AC2) | idem |
| — Gaji Rp 9 juta + rekening "9900-3620-01" | `200` "Dibuat"/"Dibuat", peringatan rentang grade; basis data: satu penetapan utama dan rahasia, belum disetujui; satu rekening payroll belum terverifikasi dengan nomor ternormalisasi | `PASS` (AC5, AC6) | idem |
| — Detail sesudah simpan | Gaji pokok, grade, dan rentang sama dengan basis data; nomor lengkap sama — menutup batas bukti `BE-HRD-361` (gaji pokok lewat PostgreSQL) | `PASS` | idem |
| — Opsi setuju + verifikasi saja | "Disetujui"/"Terverifikasi"; basis data disetujui dan terverifikasi | `PASS` | idem |
| — Ubah gaji (Rp 12,5 juta) dan nomor tanpa opsi | "Diperbarui"/"Diperbarui", dua peringatan pencabutan; basis data tidak disetujui dan tidak terverifikasi; tetap satu rekening | `PASS` (AC3) | idem |
| — Ubah gaji (Rp 13 juta) dan nomor dengan opsi | "Diperbarui dan disetujui"/"Diperbarui dan terverifikasi"; tanpa peringatan | `PASS` (AC3) | idem |
| — Hanya bagian rekening (pemilik berubah) | Gaji "Tidak diubah", tetap Rp 13 juta dan tetap disetujui; rekening "Diperbarui", verifikasi dicabut | `PASS` (AC1, AC3) | idem |
| — Simpan ulang isian yang sama | "Tidak diubah"/"Tidak diubah", tanpa log | `PASS` | idem |
| — GUID tak dikenal | `404` dengan pesan kontrak | `PASS` | idem |
| — Log audit backend | 6 baris `WorkforcePayrollData.Update` (6 simpan yang mengubah data); semua memuat `WorkforceProfileId` dan hasil; `UserId` = akun superadmin yang login (bukan profil sasaran); 0 kemunculan nominal (dua format), nomor rekening uji, atau nama pemilik | `PASS` (AC7) | `be-362.log`, `at-log.jsonl` ("362 AC7 log audit") |
| Keadaan akhir data uji | Dewi Anggraeni: satu penetapan Rp 13 juta disetujui, satu rekening payroll terverifikasi (dirapikan dengan opsi verifikasi) | Catatan | `at-log.jsonl` |

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Bagian null tidak disentuh; permintaan tanpa perubahan ditolak | Terpenuhi | Harness + devYoga |
| 2. Seluruh isian diperiksa dulu; satu ditolak → tidak ada yang tersimpan, pesan menyebut sebabnya | Terpenuhi | Harness (enam pesan sekaligus, M5 tertangkap) + devYoga |
| 3. Perubahan mencabut persetujuan/verifikasi kecuali opsi dipilih | Terpenuhi | Harness + devYoga |
| 4. Opsi setuju/verifikasi tanpa hak masing-masing → `403` | Terpenuhi (harness) | Controller nyata + `AccessPermissionService` nyata; tidak diulang lewat akun non-superadmin di devYoga |
| 5. Gaji di luar rentang grade → peringatan, bukan penolakan | Terpenuhi | Harness + devYoga |
| 6. Penetapan dan rekening payroll baru dibuat bila belum ada | Terpenuhi | Harness + devYoga |
| 7. Log audit memuat pelaku dan hasil, tanpa gaji pokok dan nomor rekening | Terpenuhi sesudah perbaikan teks log | Harness + log backend devYoga |
| Migration | Tidak ada migration. Eksekusi database hanya lewat API aplikasi untuk data uji yang disetujui pengguna | — |

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Data uji yang tertinggal di devYoga | Grade `SGR-RSMMC-00006` "Grade UJI-362", struktur `SST-RSMMC-00001` "Struktur UJI-362", penetapan gaji dan rekening payroll Dewi Anggraeni (`EMP-RSMMC-00006`). Slice ini tidak punya endpoint hapus; master uji bisa dinonaktifkan lewat layar master bila tidak diperlukan |
| Untuk `BE-HRD-363` | Action `Save` tidak mendeklarasikan `[ProducesResponseType]` `403`, padahal mengembalikannya (`VR-WPD-14`/`15`). Ini hanya dokumentasi Swagger, bukan perilaku; diperiksa bersama keselarasan kontrak |
| Risiko tersisa | Perubahan di tempat tanpa riwayat versi gaji (keputusan 3); tanpa kontrol konkurensi (simpan terakhir menang), sama dengan endpoint HRD lain |
| Perubahan sampingan | `NONE`. Mutasi kontrol negatif dipulihkan identik |
| Interupsi | Sesi 17 November berhenti sesudah berkas uji ditulis; dilanjutkan 18 November dari status Git (source dan uji sudah ada, belum dijalankan). Parameter tak terpakai pada pembantu uji dirapikan sebelum dijalankan |
| Status Git | 2 berkas source diubah, 1 berkas uji baru, laporan ini (baru), dokumen roadmap/traceability/indeks uji. Tidak ada commit |
| Task berikutnya | `BE-HRD-360` (daftar dan ringkasan; gerbang G3), lalu `BE-HRD-363` (verifikasi slice) |
