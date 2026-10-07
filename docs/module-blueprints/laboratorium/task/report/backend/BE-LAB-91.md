# Laporan Perubahan Backend — `BE-LAB-91`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-91` |
| Judul | Daftar dokter pemeriksa bagi pengonfirmasi dan jejak konfirmasi pada rincian pesanan |
| Slice | Gelombang `MVP-12a` — `EPIC-LAB-18` diperluas (BR-139) |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian **6ar.4** |
| Trace | `LAB-DEC-200`, `LAB-DEC-201`, `LAB-DEC-203`; `FR-18.8`, `FR-18.9`, `FR-18.10`; `AC-289`, `AC-290`, `AC-292`; temuan T1/T2 [`FE-LAB-50.md`](../frontend/FE-LAB-50.md) |
| Contract version | `LAB-API-v1` **`r41`** bagian 36; `LAB-PERM-v1` **revision 14** bagian 16 — **`approved` 2026-10-07** lewat `LAB-REQ-017`. `LAB-STATE-v1` `r8` dan `LAB-VAL-v1` `r17` tidak berubah |
| Dependency | `BE-LAB-90` ✅ — dikerjakan di atas working tree-nya (berkas yang sama, belum di-commit) |
| Klasifikasi | `MEDIUM` — repository 0, berkas diperiksa 1 (±12), berkas diubah 1 (3), logika bisnis 1 (predikat bersama), kontrak API 2 (endpoint baru, ruas terisi), database 0 (nol schema, baca saja), keamanan/auth 1 (endpoint baru dijaga aksi yang ada), UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement/Controllers/LabOrderController.cs`, `Services/LabOrderService.cs`, `DTOs/LabOrderDtos.cs`; laporan ini beserta status roadmap, traceability, dan kolom *Status* `r41` 36.2 |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `f17cb984` (branch `yoga`) + working tree `BE-LAB-90` |
| Tanggal | 2026-10-07 |
| Status | ⚠ **`SELESAI DENGAN BATAS VERIFIKASI`** — harness InMemory **47/47** (17 uji baru + 30 non-regresi `BE-LAB-90`) dan HTTP baca-saja **10/10** ke DB dev. **Batas:** penolakan `403` endpoint baru hanya dibuktikan lewat metadata atribut dan registrasi — belum diamati di runtime karena belum ada akun non-pemegang `Confirm` yang sandinya tersedia di sesi ini |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix registry | `Lab` — `ACTIVE` (`MODULE_OWNERSHIP_PREFIX_REGISTRY.md`) |
| Keberlakuan | `NEW CODE` — action `GetExaminerDoctorOptions`, `LabOrderService.GetExaminerDoctorOptionsAsync`, dua DTO baru. `TOUCHED LEGACY` — `GetDetailAsync` (proyeksi), `ConfirmAsync` (`VAL-73` memakai predikat bersama). Nol `LEGACY MIGRATION`, nol entity baru |
| QBE yang berlaku | `QBE-SVC-001` (kueri di service; controller hanya memetakan), `QBE-DTO-001` (entity `MstDoctor` tidak diekspos — proyeksi ke `LabExaminerDoctorOptionResponse`), `QBE-API-001` (`ApiResponse<PagedResult<T>>` seperti endpoint `options` Lab lain), `QBE-PAGE-001` (halaman + pencarian `ILike`), `QBE-OPT-001` (dikonsumsi `FE-LAB-51`), `QBE-PERM-001` (`[AccessAction]` berpasangan `[AccessPermission]`, aksi yang sudah terdaftar) |
| Governance yang dibaca | `AGENTS.md` backend; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/` dari suite Skill Quilvian (repository ini tidak memuat `.codex`; `AGENTS.md` menunjuk suite itu sebagai sumber aturan operasional) |
| Pengecualian | Nol |

---

## 1. Masalah yang diperbaiki

| Sebelum | Akibat |
| --- | --- |
| Pemilih dokter dialog Konfirmasi memuat `GET …/human-resource/master-data/doctors/options` milik SDM, dijaga kebijakan `KioskRead` | Analis pemegang `LabOrder : Confirm` mendapat `403` dan tidak dapat memilih dokter pemeriksa — Konfirmasi lewat layar mustahil (T2) |
| `GetDetailAsync` tidak memetakan lima ruas jejak konfirmasi yang dijanjikan `r13` | Jawaban `confirm` membalas `confirmedAt`, nama konfirmator, dan dokter pemeriksa kosong; sejak `r40` status *Diterima* tidak berubah, jadi layar baru benar sesudah muat ulang (T1) |
| Syarat dokter-dapat-dipilih hanya tertulis sebaris di `ConfirmAsync` | Daftar baru berisiko memakai syarat yang berbeda dari `VAL-73` |

**Contoh:** analis membuka Konfirmasi pesanan Ureum *Diterima*. Kemarin kotak *Dokter Pemeriksa* kosong dan
backend menjawab `403`. Kini daftar 16 dokter aktif muncul dan dapat dicari ("arif" → satu dokter); sesudah Simpan,
jawaban server langsung membawa nama analis, waktu, dan nama dokter.

---

## 2. Proses bisnis

**Pengguna:** analis laboratorium (pemegang `LabOrder : Confirm`).

1. Analis membuka dialog Konfirmasi dan membuka kotak *Dokter Pemeriksa*.
2. Layar memuat `GET /lab-orders/examiner-doctor-options` — dokter aktif, 25 per halaman, urut nama.
3. Analis mengetik sebagian nama, kode, atau spesialisasi; daftar tersaring tanpa membedakan huruf besar/kecil.
4. Analis memilih dokter dan Simpan → `POST /{id}/confirm` → jawaban membawa jejak konfirmasi lengkap.

**Jalur tidak normal.**

| Keadaan | Jawaban |
| --- | --- |
| Pengguna tanpa `LabOrder : Confirm` memuat daftar | `403` |
| Ukuran halaman di luar 1–50 atau nomor halaman < 1 | Dijepit — tetap `200` |
| Pencarian lebih dari 100 karakter | Dipotong 100 — tetap `200` |
| Dokter dinonaktifkan sesudah daftar dimuat, lalu Simpan | `422` `VAL-73` (tidak berubah) |

**Perubahan status:** tidak ada.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`LabOrderController.cs`, `LabOrderService.cs` (`ConfirmAsync`, `GetDetailAsync`, `MapDetailResponse`, pemanggil
`GetDetailOrThrowAsync`), `LabOrderDtos.cs`, `LabConfirmingDoctorResolver.cs` (pola kueri `MstDoctor`),
`LabMicrobiologyMasterDataService.GetOptionsAsync` (pola `options`), `LabMonitoringService.cs` (sub-kueri nama),
`DoctorController.cs` (SDM, pembanding), `MstDoctor.cs`, `PagedResult.cs`, `PermissionRegistryValidator.cs`.

### 3.2 Berkas yang berubah

Angka diff di bawah hanya bagian `BE-LAB-91`; berkas yang sama juga memuat perubahan `BE-LAB-90` yang belum di-commit.

| Berkas | Perubahan |
| --- | --- |
| `DTOs/LabOrderDtos.cs` | **Baru:** `LabExaminerDoctorOptionQuery` (`Search`, `PageNumber` 1, `PageSize` 25) dan `LabExaminerDoctorOptionResponse` (`Id`, `DoctorCode`, `FullName`, `SpecialistName` — tepat empat ruas). +45 |
| `Services/LabOrderService.cs` | `using System.Linq.Expressions`; predikat statis privat `SelectableExaminerDoctor` (`!IsDelete && IsActive`) kini dipakai `VAL-73` di `ConfirmAsync`; `GetExaminerDoctorOptionsAsync` (baca, tanpa transaksi, `ILike` nama/kode/spesialisasi, urut nama lalu kode, jepit halaman, proyeksi empat ruas di dalam kueri); `GetDetailAsync` mengisi `ConfirmedAt`, `ConfirmedByUserId`, `ConfirmedByName`, `ExaminerDoctorId`, `ExaminerDoctorName` — ekspresi sama dengan `LabMonitoringService`. `MapDetailResponse` tidak diubah |
| `Controllers/LabOrderController.cs` | Action `GetExaminerDoctorOptions` — `[HttpGet("examiner-doctor-options")]`, `[AccessAction("Confirm", "Confirm Lab Order", …, AccessType = AccessTypes.Update, SortOrder = 3)]`, `[AccessPermission("LabOrder", "Confirm")]`, `ProducesResponseType` `200`/`403`; komentar `//`, tanpa `<summary>` XML pada action |

### 3.3 Dampak kontrak API, database, dan keamanan

| Hal | Isi |
| --- | --- |
| API | Satu endpoint baca baru; lima ruas `LabOrderDetailResponse` kini terisi pada `GET /{id}`, `confirm`, `verify-instruction`, `start-process`, `complete`, `hold`, `resume`, `cancel`. Bentuk DTO rincian tidak berubah — **aditif** |
| Database | Nol migration, nol tulis, nol tabel/kolom/index. Hanya membaca `MstDoctor` dan `AspNetUsers` |
| Izin | Nol aksi baru — `SysActionAccess` `LabOrder` tetap tujuh aksi (diperiksa baca-saja sesudah aplikasi menyala) |
| Privasi | Telepon, WhatsApp, surel, alamat, dan data pribadi dokter tidak pernah dibaca kueri daftar (harness memastikan nilai kontak semaian tidak terserialisasi) |
| Logging | Nol — baca murni |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/laboratory-management/lab-orders/examiner-doctor-options` | Pilihan dokter pemeriksa saat Konfirmasi | `LabOrder : Confirm` |

**Query:** `search` (opsional, maks 100), `pageNumber` (bawaan 1), `pageSize` (bawaan 25, 1–50).
**Response:** `ApiResponse<PagedResult<LabExaminerDoctorOptionResponse>>` — `{ pageNumber, pageSize, totalData, totalPage, items: [{ id, doctorCode, fullName, specialistName }] }`.
**Kode status:** `200`; `401`; `403`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` (server lokal mati) | 0 error, 242 warning — jumlah sama dengan build `BE-LAB-90`; nol warning pada tiga berkas yang disentuh | `PASS` | Log build, 1 m 43 s |
| Harness InMemory (`ApplicationDbContext` asli, nol tulis ke DB bersama) | **47/47** | `PASS` | Rincian di bawah |
| — `AC-289` tanpa pencarian | Hanya dokter aktif dan tidak terhapus (nonaktif dan terhapus tidak muncul); urut nama lalu kode (dua dokter bernama sama → kode); bawaan 1/25; jepit 500→50, 0→1, 0→1, −3→1; halaman 2 ukuran 2; query `null` dan pencarian spasi = tanpa pencarian | `PASS` | |
| — `AC-290` | Tepat empat ruas (refleksi); nilai kontak semaian (`0800-RAHASIA`, surel, alamat) tidak ada di JSON; atribut `LabOrder:Confirm` + `Confirm|Confirm Lab Order|Update`; rute `examiner-doctor-options`; snapshot aksi `LabOrder` tetap tujuh | `PASS` | |
| — Predikat bersama | Setiap dokter di daftar lolos `confirm` `200`; dokter terhapus (tidak di daftar) → `422` `VAL-73` | `PASS` | |
| — `AC-292` | Rincian belum dikonfirmasi → lima ruas `null`; jawaban `confirm` membawa lima ruas (nama konfirmator dari `AspNetUsers`, nama dokter), status tetap `Accepted`; `GET` rincian sesudahnya sama; jawaban `start-process` ikut membawa jejak | `PASS` | |
| — Non-regresi `BE-LAB-90` | 30 uji (`AC-283`..`AC-286`, `AC-288` metadata, `hold`, validator) | `PASS` | Validator 1625 aksi |
| HTTP baca-saja — backend lokal Development + DB dev, superadmin | **10/10**: `200` berhalaman (16 dokter); empat ruas; pencarian `ILike` asli: "arif" (huruf kecil) 1 hasil, kode dokter, "PATOLO" (huruf besar) 4 hasil; jepit 500/0 dan `onlyActive=false` diabaikan (total sama); pencarian 150 karakter → `200`; rincian `LAB-RSMMC-000003` lima ruas terisi; `LAB-RSMMC-000002` lima ruas `null` | `PASS` | Pemeriksaan "urut nama" pada skrip HTTP hanya pengamatan (kondisinya selalu benar) — bukti urutan dari harness |
| Pemeriksaan DB baca-saja | Dokter aktif tidak terhapus = **16** (sama dengan `totalData`); aksi `LabOrder` = 7, nol baru; versi `000002` (v4) dan `000003` (v3) tidak berubah | `PASS` | Npgsql `default_transaction_read_only=on` |
| `403` endpoint baru di runtime | — | `NOT RUN` | Superadmin melewati izin; akun analis memegang `Confirm`; tidak ada sandi akun non-pemegang di sesi ini. Mekanisme `403` atribut yang sama terbukti di runtime pada `BE-LAB-90` (`cancel`, `pathology-context`) |

Satu kegagalan harness pada putaran pertama adalah **harapan uji yang salah**, bukan kode: dokter semaian lama
ber-`DoctorCode` kosong (bawaan `string.Empty`), bukan `"X"`. Harapan dibetulkan; urutan yang dihasilkan kode tidak
berubah.

Uji manual: `NOT APPLICABLE` — task backend; layar dikerjakan `FE-LAB-51`.

**Tidak dijalankan:** `403` runtime (di atas); uji otomatis repository — `Tests/` dikecualikan `.gitignore`
(`LAB-RDY-C04`), harness tinggal di scratchpad sesi.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-289` — dokter aktif saja, dapat dicari, berhalaman, dijaga `Confirm` | Terpenuhi (harness + HTTP) | Bagian 5 |
| `AC-290` — `403` tanpa `Confirm`; tepat empat ruas | **Sebagian** — empat ruas dan privasi terbukti; `403` dari metadata dan registrasi, belum runtime | Bagian 5 |
| `AC-292` sisi backend — lima ruas pada `confirm` dan `GET /{id}` | Terpenuhi (harness + HTTP rincian asli) | Bagian 5 |
| Predikat bersama `VAL-73` | Terpenuhi | Bagian 5 |
| DoD — build hijau, validator lolos, laporan, kolom *Status* `r41` 36.2 | Terpenuhi | Status 36.2 diperbarui bersama laporan ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol baru |
| Masalah yang diketahui | `LabOrderService` makin besar; memecahnya bukan bagian task ini (`02-backend-architecture.md` 25.7) |
| Risiko tersisa | **Rilis serempak wajib** (`LAB-DEC-202`): frontend lama yang belum memakai daftar Lab tetap memanggil daftar SDM; frontend baru tanpa backend ini mendapat `404`. Deploy `BE-LAB-90`+`BE-LAB-91` dan `FE-LAB-50`+`FE-LAB-51` dalam satu jendela |
| Perubahan sampingan | Nol. Sesudah uji, server kompilator .NET (`VBCSCompiler`, ±10 GB) dimatikan lewat `dotnet build-server shutdown` |
| Data dev | Nol perubahan — seluruh panggilan HTTP `GET` |
| Interupsi | `NONE` |
| Status Git | Backend source: ` M` tiga berkas (`LabOrderController.cs`, `LabOrderService.cs`, `LabOrderDtos.cs`) — gabungan `BE-LAB-90` dan `BE-LAB-91`, belum di-commit |
| Langkah berikutnya | `FE-LAB-51` — pemilih dokter memakai endpoint ini; verifikasinya membutuhkan pesanan *Diterima* belum dikonfirmasi (izin tulis per sesi). `403` runtime dapat ditutup bila tersedia akun non-pemegang `Confirm` |
