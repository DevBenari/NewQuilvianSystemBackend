# Laporan Perubahan Backend — `BE-NV-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-NV-003` |
| Judul | Login dan runtime akun perangkat |
| Slice | `MVP-1` — perangkat dan login TV |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/backend-roadmap.md` — kartu `BE-NV-003` |
| Trace | `FR-NV-018`; `NV-DEC-009`, `022`, `030`, `031`; `VAL-NV-14`–`16`; `02-backend-architecture.md` §4.5, §4.10; `contracts/state-transition-matrix.md` |
| Contract version | `0.1.1` — `approved` (Rizki Gunawan, 2026-11-02, `NV-DEC-031`); sebelumnya `0.1.0` |
| Dependency | `BE-NV-002` ✅ |
| Klasifikasi | `HEAVY` — berkas bersama login seluruh sistem (`AuthController`, `Program.cs`, `AuthorizationPolicies`), keamanan, syarat lead `NV-DEC-030`, tanpa migration |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/NurseViewManagement/**`, `Controllers/AuthController.cs`, `Constants/AuthorizationPolicies.cs`, `Program.cs`, dokumen delta kontrak, laporan, status roadmap |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `80e2a284` (branch `rizkiG`); source di-commit Rizki sebagai `d936447e` |
| Tanggal | 2026-11-02 |
| Status | ✅ **SELESAI 2 November 2026** — QBE Strict `PASS` (8 berkas), build Rizki lulus, regresi login 9/9 skenario sama dengan baseline (125 cek), uji TV 9/9 `PASS` (67 cek). Verifikasi developer, bukan UAT |

---

## 1. Masalah yang diperbaiki

Akun perangkat Nurse View yang dibuat di `BE-NV-002` sudah bisa login, tetapi sistem belum mengenalinya
sebagai TV. Baseline sebelum perubahan: akun `NVW-000003` masuk ke dashboard admin dengan profil
`AccountOnly`, sesi hanya 60 menit, dan endpoint runtime belum ada (`404`).

**Contoh.** TV lantai 3 dinonaktifkan admin karena hilang. Petugas yang mencoba login di TV itu membaca
"Akun terkunci sementara karena terlalu banyak percobaan login gagal.", padahal tidak ada percobaan
gagal sama sekali. Sekarang, dengan sandi yang benar, ia membaca "Perangkat display dinonaktifkan.
Hubungi IT."

---

## 2. Proses bisnis

**Tujuan:** TV login memakai akun perangkat dan hanya mendapat akses ke runtime Nurse View, sedangkan
login akun lain tidak berubah.

**Pelaku:** TV (akun perangkat), admin IT (membuat login dan menonaktifkan).

**Langkah normal:**

1. TV login dengan akun `nvw-000003`. Sistem mengenali akun itu lewat `NvwDisplayDevice.LoginUserId`.
2. Karena akun perangkat, pemeriksaan lokasi dan masa akses dilewati, seperti Kiosk dan display antrean.
3. Token berlaku 129.600 menit (90 hari) dan membawa klaim `is_nurse_view_account=true`,
   `nurse_view_device_id`, `nurse_view_session_stamp`.
4. TV memanggil `GET /display-runtime/current`. Di setiap permintaan sistem memeriksa ulang status perangkat
   dan keabsahan sesinya.

**Jalur tidak normal:**

| Kejadian | Jawaban sistem | Kode |
| --- | --- | --- |
| Perangkat nonaktif login dengan sandi benar | "Perangkat display dinonaktifkan. Hubungi IT." | `401` (`NV-DEC-031`) |
| Perangkat nonaktif login dengan sandi salah | Pesan terkunci standar, supaya status perangkat tidak terbaca orang yang tak tahu sandinya | `401` |
| Perangkat dinonaktifkan saat TV menyala | Permintaan berikutnya ditolak, pesan yang sama, kode `VAL-NV-15` | `403` |
| TV memakai token lama sesudah Atur Ulang Login | "Sesi perangkat sudah tidak berlaku. Silakan login ulang." | `403`, `NV-SESSION-STALE` |
| Token lama mencoba `POST /auth/refresh` | Pesan nonaktif atau pesan sesi, sesi tidak bisa dihidupkan lagi | `401` |
| Akun manusia, termasuk SuperAdmin, membuka runtime | Ditolak policy, tanpa isi | `403` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AuthController.cs`, `Program.cs`, `AuthorizationPolicies.cs`, `QueueDisplayRuntimeController.cs`,
`NvwDisplayDeviceService.cs`, `NvwHttp.cs`, `LanguageService.cs`, `PermissionRegistryDescriptor.cs`,
`QueueDisplayDeviceController.cs`, `KioskDeviceController.cs`; blueprint §4.5, §4.10, kontrak, `00-interview-decisions.md`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Services/NvwDeviceContextService.cs` | **Baru.** Resolver login, pemeriksaan perangkat per permintaan, `LastOnlineAt` paling sering sekali per 60 detik, data `/current` |
| `Areas/.../Controllers/NvwDisplayRuntimeController.cs` | **Baru.** `GET /current` dengan policy, tanpa `[AccessPermission]`, pola `QueueDisplayRuntimeController` |
| `Areas/.../DTOs/NvwDisplayRuntimeDtos.cs` | **Baru.** `NvwDisplayRuntimeCurrentResponse`, `NvwDeviceLoginContext` |
| `Areas/.../Services/NvwResult.cs` | Tiga kode dan tiga pesan baru |
| `Areas/.../Constants/NurseViewPermissions.cs` | Resource `NurseViewDisplayRuntime` |
| `00-interview-decisions.md`, `contracts/validation-matrix.md`, `contracts/api-contract.md`, `02-backend-architecture.md`, `03-frontend-architecture.md`, `blueprint-manifest.md` | Delta kontrak `0.1.1`: `NV-DEC-031`, `NV-DEC-032`, `VAL-NV-16`, catatan runtime |

**Diff ringkas per berkas bersama (syarat kartu dan `NV-DEC-030`):**

| Berkas bersama | Yang ditambahkan | Yang tidak disentuh |
| --- | --- | --- |
| `Controllers/AuthController.cs` | Satu dependensi `NvwDeviceContextService`; resolver `ResolveNurseViewLoginContextAsync`; pada `Login`, `Me`, `Refresh` satu baris pemanggilan; `nurseViewContext.IsNurseViewAccount` pada `isDeviceLoginAccount`; blok tolak perangkat nonaktif; pada cabang terkunci hanya perangkat Nurse View nonaktif + sandi benar yang mendapat pesan khusus; blok klaim `is_nurse_view_account` di `GenerateJwtToken`; parameter opsional `nurseViewContext` pada lima fungsi; `profileType = NurseViewDevice` | Isi `ResolveKioskLoginContextAsync`, `ResolveQueueDisplayLoginContextAsync`, klaim Kiosk, klaim display antrean, `/fingerprint-login`, DTO `UserLoginResponse` |
| `Constants/AuthorizationPolicies.cs` | Konstanta `NurseViewDisplayRead` dan satu entri di `ApprovedAlternativeAuthorization` | Policy lain |
| `Program.cs` | Satu `AddScoped<NvwDeviceContextService>`; satu `AddPolicy(NurseViewDisplayRead)` yang wajib klaim `nurse_view_device_id`, tanpa jalan pintas SuperAdmin/Administrator | Policy `KioskRead`, `QueueDisplayRuntimeRead`, `QueueDisplayRead` |

**Dua tambahan di luar teks kartu, keduanya keputusan developer:**

1. **Klaim `nurse_view_session_stamp`.** `state-transition-matrix` dan teks layar menyatakan sesi lama tidak berlaku
   sesudah Atur Ulang Login, tetapi JWT di repository ini tidak memeriksa security stamp, sehingga tanpa ini TV lama
   tetap berjalan. Isinya diturunkan dari `LoginIssuedAt`, lalu dibandingkan di `/current` dan `/auth/refresh`.
2. **`try/catch` pada resolver di `AuthController`.** Tanpa ini, lingkungan yang belum menjalankan migration Nurse View
   membuat semua login gagal `500`. Bila gagal, akun diperlakukan sebagai akun biasa tanpa akses papan, dan
   kesalahannya tercatat di log. Ini menjaga syarat lead.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru `GET /current`. Token semua akun bertambah satu klaim `is_nurse_view_account=false`, satu-satunya beda yang terukur pada akun lain. `/auth/permissions` bertambah butir `NurseViewDisplayRuntime:Read` pada akun akses penuh |
| Database | Tanpa migration, tanpa perubahan skema |
| Keamanan/Auth | Policy baru masuk `ApprovedAlternativeAuthorization`; alasannya akun perangkat tidak punya Departemen × Posisi. Akun manusia tidak bisa membaca runtime. Perbedaan pesan sandi benar/salah hanya pada perangkat nonaktif; sandi diacak dan selalu diganti saat diaktifkan kembali, sehingga risikonya rendah |

---

## 4. Dokumentasi endpoint

#### Health Services / Nurse View Management / Display Runtime

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/nurse-view-management/display-runtime/current` | Identitas perangkat yang login dan pengaturan muat ulang | policy `NurseViewDisplayRead` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Checker QBE Strict, `-Path` 8 berkas | `Files evaluated: 8`, 0 violation, 0 review | `PASS` | Keluaran perintah |
| `dotnet build` oleh Rizki | DLL 15.08 memuat kelas baru; `/current` tanpa login `401` (sebelumnya `404`); snapshot dan atribut migration terbaru ada di DLL, artinya build penuh. Jumlah warning belum dinyatakan Rizki | `PASS` | Pemeriksaan DLL dan route |
| Regresi perawat, SuperAdmin, LOKET, Kiosk, display antrean, dibanding baseline | 5/5 akun, 21 cek per akun: status, pesan, halaman tujuan, isi user, klaim, cookie, `/me`, `/refresh`, policy | `PASS` | `perbandingan-2026-11-02T09-07-48-904Z` |
| Login gagal: sandi salah, email tak dikenal, akun terkunci sandi salah, akun terkunci sandi benar | 4/4 status dan pesan sama dengan baseline | `PASS` | Idem; akun terkunci memakai `PERAWAT_RUANGAN` dengan 5× sandi salah, seizin Rizki |
| Akun TV login, `/current` 200 dengan seluruh field kontrak | N1, 18 cek | `PASS` | `tv-2026-11-02T09-11-28-274Z` |
| Admin menonaktifkan saat TV menyala | N5: TV kembali ke login dalam 59 detik; token lama `403` `VAL-NV-15`, `/auth/refresh` `401` | `PASS` | Idem |
| Login perangkat nonaktif | N6: sandi benar pesan khusus, sandi salah pesan terkunci standar | `PASS` | Idem |
| Atur Ulang Login | N7: token lama `403` `NV-SESSION-STALE`, refresh `401`, sandi lama `401`, sandi baru masuk | `PASS` | Idem |
| Akun manusia membuka runtime | N8: LOKET dan SuperAdmin `403` | `PASS` | Idem |
| `LastOnlineAt` | Dua kali tulis berjarak tepat 60 detik pada 3 kali `GET` | `PASS` | `tambahan-2026-11-02T09-15-24-065Z` |
| Pencarian kredensial pada seluruh bukti JSON | Nol token, nol sandi | `PASS` | Pencarian pola pada folder uji |

Seluruh bukti berada di `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-nv-003-20261102/`.
Baseline direkam sebelum kode berubah: `baseline-2026-11-02T05-58-02-247Z` dan `baseline-2026-11-02T06-04-27-191Z`.

**Catatan sidik `/auth/permissions`.** Sidiknya berbeda di semua akun. Penyebabnya terukur: amplop respons memuat
`timestamp`, sehingga Kiosk dan display yang panjangnya sama (210 → 210) tetap berbeda, dan dua akun akses penuh
bertambah 54–55 karakter, sama dengan panjang satu butir `NurseViewDisplayRuntime:Read`. Isi per butir tidak direkam
di baseline, jadi perbandingan butir demi butir tidak dilakukan.

**Penyimpangan dari rencana uji.** Reset login Kiosk dan display, Atur Ulang Login, serta nonaktifkan dijalankan lewat
endpoint yang dipanggil layar admin, memakai sesi SuperAdmin, karena halaman detail admin menjawab `404` pada dev server
saat uji (rincian di `FE-NV-003.md` §8). Login, papan, dan penolakan tetap diuji dari layar. Layar admin itu sendiri
sudah diuji di `FE-NV-002`.

**Riwayat.** Putaran pertama TV (`tv-2026-11-02T09-07-58-304Z`) N7 `FAIL`, penyebabnya skrip uji: halaman TV lama sudah
logout sendiri sesuai `NV-AC-016` sehingga cookie-nya hilang. Skrip diperbaiki, produk tidak diubah.

Uji manual: `PASS`.

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

**Tidak dijalankan:** `tools/authorization-verifier/verify-authorization.sh` (`NOT RUN`, memanggil `dotnet build`, milik
Rizki).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `NV-AC-016` perangkat dinonaktifkan, permintaan berikutnya ditolak, TV ke login | Terpenuhi | N5 |
| `NV-AC-023` perawat ke dashboard, tanpa `nurse_view_device_id`, tanpa cookie `isNurseViewAccount` | Terpenuhi | Perbandingan `perawat`, N9 |
| `NV-AC-024` Kiosk ke `/kiosk`, `KioskRead` lolos | Terpenuhi | Perbandingan `kiosk`: tujuan `/kiosk`, `kioskRead` `200` |
| `NV-AC-025` display ke `/queue-display`, klaim terbit, papan tampil | Terpenuhi | Perbandingan `display`; `/queue-display-runtime/current` `200`; layar sama dengan baseline |
| `NV-AC-026` SuperAdmin tidak berubah, papan `403` | Terpenuhi | Perbandingan `superadmin`, N8 |
| `NV-AC-027` pesan dan kode login gagal sama | Terpenuhi, dengan pengecualian `NV-DEC-031` | Perbandingan `gagal-*`, `terkunci-*` |
| `VAL-NV-14` | Terpenuhi untuk status `403`; akun tanpa klaim ditolak policy tanpa isi, sama dengan display antrean. Pesannya hanya keluar bila klaim ada tetapi perangkat tidak ditemukan (catatan `api-contract` `0.1.1`) | N8 |
| `VAL-NV-15` | Terpenuhi | N5 |
| `VAL-NV-16` | Terpenuhi | N6 |
| `UAT-11` | Terverifikasi developer; UAT belum dijalankan | Perbandingan |
| DoD — regresi lulus dan direkam | Terpenuhi | §5 |
| DoD — laporan mencantumkan diff per berkas bersama | Terpenuhi | §3.2 |

**Backend Governance Preflight:** Area `HealthServices` · Module `NurseViewManagement` · prefix `Nvw` `ACTIVE` ·
applicability `NEW CODE`; `AuthController.cs`, `AuthorizationPolicies.cs`, `Program.cs` = `TOUCHED LEGACY` (tambahan saja,
restu `NV-DEC-030`).

**QBE yang berlaku dan dipatuhi:** `QBE-SVC-001` (akses data di service, bukan di `AuthController`), `QBE-API-001`,
`QBE-PERM-001` (policy terdaftar di `ApprovedAlternativeAuthorization`), `QBE-DTO-001`, `QBE-LOG-001`.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Status migration/database | Tanpa migration. Data uji di DB dev: `NVW-000003` kini Aktif (bukan lagi "Belum ada login"); sandi `KSK-RSMMC-00009` dan `QDD-RSMMC-00003` sudah diganti dan hanya pernah ada di memori skrip, sehingga perangkat itu perlu reset dari layar bila hendak dipakai; `PERAWAT_RUANGAN` sempat terkunci 60 menit |
| Peringatan | `NOT RUN`: `authorization-verifier`. Hasil warning build belum dinyatakan Rizki |
| Masalah yang diketahui | `/fingerprint-login` sengaja tidak menyertakan konteks Nurse View, karena akun perangkat tidak punya kredensial sidik jari |
| Risiko tersisa | Sesi TV 90 hari; status tetap diperiksa di setiap permintaan, sehingga penonaktifan tetap berlaku pada permintaan berikutnya. `NV-DEC-032` tentang tiga berkas frontend tambahan masih perlu disampaikan Rizki ke lead |
| Perubahan sampingan | `NONE` |
| Interupsi | Dev server menjawab `404` pada rute admin, lihat `FE-NV-003.md` §8. Permintaan menghentikan `next dev` milik Rizki ditolak pengaman sesi, sehingga tidak dilakukan |
| Status Git | Source task ini ada di commit `d936447e`. Working tree berisi kode `BE-NV-005` yang belum dibuild, serta laporan ini dan status roadmap yang belum di-commit |
| Langkah berikutnya | `BE-NV-005` + `FE-NV-005` |
