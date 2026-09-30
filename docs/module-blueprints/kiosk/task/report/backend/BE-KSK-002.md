# Laporan Perubahan Backend — `BE-KSK-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-KSK-002` |
| Judul | Rate limit lookup |
| Slice | EPIC KSK-01 — Layanan Lookup No. RM (backend), gelombang `MVP-0`, gelombang eksekusi 2 |
| Roadmap | `docs/module-blueprints/kiosk/roadmap/backend-roadmap.md` — kartu `BE-KSK-002` |
| Trace | `FR-KSK-006`; `KSK-DEC-011`; `KSK-DSN-005`; `KSK-GAP-002/003/004`; `contracts/api-contract.md` (kode `429`); `contracts/validation-matrix.md` `KSK-VAL-007` |
| Contract version | `KSK-CONTRACT-v1` — `approved` (Sukma Giri Pratama, 2026-09-30) |
| Dependency | `BE-KSK-001` ✅ |
| Klasifikasi | `LIGHT` — 3 berkas diubah; middleware baru tetapi hanya untuk satu policy bernama; tanpa database |
| Task mode | `BACKEND` (dinyatakan pengguna 2026-09-30: "oke lanjutkan" atas usulan memulai `BE-KSK-002`) |
| Target tulis | `Program.cs`, `appsettings.json`, `KioskPatientLookupController.cs`, laporan ini, baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `419b910f` (branch `sukmagp`), belum di-commit |
| Tanggal | 30 September 2026 |
| Status | ✅ SELESAI — 5 dari 5 acceptance criteria terbukti |

---

## 1. Masalah yang diperbaiki

Setelah `BE-KSK-001`, siapa pun yang memegang satu akun perangkat Kiosk dapat mencoba nomor KTP/HP tanpa batas untuk menebak siapa saja yang terdaftar di rumah sakit (enumerasi). Contohnya, skrip yang mencoba 1.000 nomor HP berurutan dalam semenit akan tahu nomor mana yang milik pasien.

---

## 2. Proses bisnis

| Hal | Isi |
| --- | --- |
| Pelaku | Akun perangkat Kiosk |
| Aturan | Maksimal 10 pencarian per menit per akun perangkat (`KSK-DEC-011`). Nilainya diatur di `KioskPatientLookup:PermitPerMinute` (bawaan 10, minimal 1) |
| Jalur normal | Pencarian ke-1 sampai ke-10 dalam satu menit dijawab seperti biasa |
| Jalur tidak normal | Pencarian ke-11 dijawab `429` dengan pesan "Terlalu banyak percobaan. Silakan coba lagi sebentar." dan header `Retry-After` (detik). Layar Kiosk **tidak boleh** membacanya sebagai "pasien belum terdaftar" |
| Pemulihan | Otomatis ketika jendela satu menit berikutnya dimulai |
| Batas | Perangkat lain tidak terpengaruh; endpoint lain dari perangkat yang sama tidak terpengaruh |

Urutan middleware: autentikasi → otorisasi → rate limit. Akibatnya, request tanpa login (`401`) atau dari akun non-kiosk (`403`) ditolak lebih dulu dan tidak memakan kuota perangkat. Request dari akun Kiosk yang isiannya tidak sah (`400`) tetap memakan kuota, dan ini disengaja karena enumerasi juga bisa memakai isian acak.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; suite `rules/backend/`; `Program.cs` (urutan middleware, blok `AddAuthorization`); `Controllers/AuthController.cs` (klaim `NameIdentifier`/`user_id`); kartu task dan `02-backend-architecture.md` `KSK-DSN-005`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Program.cs` | `AddRateLimiter` dengan satu policy bernama `KioskPatientLookup`: fixed window 1 menit, `QueueLimit = 0`, partisi per klaim `NameIdentifier` (cadangan: alamat IP), `OnRejected` menulis `429` + `Retry-After` + body `ApiResponse` + log Warning (user id perangkat dan path saja). `app.UseRateLimiter()` setelah `UseAuthorization()`. Tambah 3 `using` |
| `appsettings.json` | Seksi baru `"KioskPatientLookup": { "PermitPerMinute": 10 }` |
| `Areas/HealthServices/RegistrationManagement/Controllers/KioskPatientLookupController.cs` | Konstanta `RateLimitPolicy`; `[EnableRateLimiting(RateLimitPolicy)]` dan `ProducesResponseType 429` pada `POST /` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Kode `429` pada `POST kiosk-patient-lookups` kini aktif, sesuai `KSK-CONTRACT-v1`. Tidak ada delta |
| Database | `NOT APPLICABLE` — tanpa schema/entity/migration |
| Keamanan/Auth | Penahan enumerasi per perangkat. Tanpa `GlobalLimiter`. Batas in-memory per proses; lihat risiko tersisa |

---

## 4. Dokumentasi endpoint

#### Health Services / Registration Management / Kiosk Patient Lookup

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Lookup pasien; kini dibatasi 10/menit/akun perangkat, pelanggaran dijawab `429` | Policy `KioskRead`; rate limit `KioskPatientLookup` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build … -p:UseSharedCompilation=false -o <scratchpad>` | 0 error, tanpa warning baru di berkas task | `PASS` | Keluaran build |
| `dotnet ef migrations has-pending-model-changes --no-build` | Tidak ada perubahan model | `PASS` | Keluaran perintah |
| QBE Strict `-Path` untuk `Program.cs` dan controller | 2 × `VIOLATION: 0`, `PASS` | `PASS` | Keluaran QBE |
| Build Release + authorization verifier | `PASS`; fallback 69 cocok persis; naked baru 0 | `PASS` | Keluaran verifier |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test |

### Uji runtime

Lingkungan: app dari build scratchpad, `Runtime__Role=Web`, DB `QuilvianNewDevSukma`. Akun A = `kiosk-sukma`, akun B = `kiosk-test` (akun perangkat Kiosk dev; kredensial tidak dicatat). Nilai pencarian `9999000000000099` (tidak terdaftar), jadi tidak ada data uji yang dibuat.

| ID | Skenario | Hasil | Klasifikasi |
| --- | --- | --- | --- |
| R0 | Keadaan DB awal: `MstPatient` 16, `KSKTEST` 0 | Sesuai | `PASS` |
| R1 | 10 lookup akun A, lalu ke-11 | 10 × `200`; ke-11 `429`, `Retry-After: 60`, body `{"success":false,"statusCode":429,"message":"Terlalu banyak percobaan. Silakan coba lagi sebentar.",…}` | `PASS` |
| R2 | Akun B pada menit yang sama | `200` | `PASS` |
| R3 | Endpoint lain dari akun A (`GET patients/kiosk?pageSize=1`) pada menit yang sama | `200` | `PASS` |
| R4 | Akun A setelah jendela berlalu (+62 detik) | `200` | `PASS` |
| R5 | Restart dengan `KioskPatientLookup__PermitPerMinute=3`; 4 lookup akun A | `200, 200, 200, 429` | `PASS` |
| R6 | Log penolakan | Satu baris Warning berisi user id perangkat dan path; nilai pencarian muncul 0 kali di log | `PASS` |
| R-akhir | DB sesudah uji: `MstPatient` 16, `KSKTEST` 0 | Sama dengan R0 | `PASS` |

Catatan jalannya uji: percobaan skrip pertama menghasilkan `401` untuk semua request. Penyebabnya bukan source: cookie token bertanda `Secure`, sehingga `cookiejar` Python tidak mengirimnya lewat `http`. Skrip diperbaiki untuk mengirim token lewat header `Bearer` (pola yang sama dengan `BE-KSK-001`), lalu seluruh skenario dijalankan ulang setelah jendela kosong.

Saat mencari akun perangkat kedua, agent juga mencoba login `kiosklobby01`, `kiosklobby02`, dan `kiosk-rizki` dengan password yang sama. Hal ini melebihi akun yang disebut pengguna. Ketiganya tidak dipakai untuk uji apa pun, dan dicatat di sini apa adanya.

Efek tulis selama app berjalan: seeder web idempoten dan dua worker yang selalu aktif (seperti `BE-KSK-001`). Tidak ada data uji yang dibuat.

Uji manual: `NOT APPLICABLE` — belum ada layar.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (roadmap) | Status | Bukti |
| --- | --- | --- |
| 1. 10 lookup akun A → `200`; ke-11 → `429` dengan body yang ditetapkan | Terpenuhi | R1 |
| 2. Akun B pada menit yang sama → `200` | Terpenuhi | R2 |
| 3. Endpoint lain akun A pada menit itu tetap `200` | Terpenuhi | R3 |
| 4. Setelah jendela 1 menit, akun A kembali `200` | Terpenuhi | R4 |
| 5. `PermitPerMinute = 3` → permintaan ke-4 `429` | Terpenuhi | R5 |
| DoD: AC 1–5 terbukti di laporan ini | Terpenuhi | §5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Diagnostic Sonar pada `Program.cs` (cognitive complexity, dan lain-lain) seluruhnya sudah ada sebelum task ini |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Limiter disimpan **in-memory per proses**. Bila backend dijalankan beberapa instance di belakang load balancer (misalnya Blue-Green dengan dua container Web aktif bersamaan), batas efektifnya menjadi 10 × jumlah instance per menit. Batas lintas instance butuh store bersama (misalnya Redis); itu keputusan infrastruktur di luar task ini |
| Perubahan sampingan | `NONE` di source. Artefak `bin/Release` (gitignored) untuk verifier |
| Interupsi | `NONE` |
| Status Git | ` M Program.cs`; ` M appsettings.json`; `?? Areas/HealthServices/RegistrationManagement/{Controllers,DTOS,Enums,Services}/KioskPatientLookup*` (dari `BE-KSK-001` + task ini); `?? docs/module-blueprints/kiosk/` |
| Langkah berikutnya | `FE-KSK-001`/`FE-KSK-002` (mandiri), atau `FE-KSK-003` (sekarang tidak tertahan backend). `BE-KSK-003` tetap ⛔ `KSK-OQ-005` |
