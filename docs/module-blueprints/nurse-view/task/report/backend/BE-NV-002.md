# Laporan Perubahan Backend — `BE-NV-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-NV-002` |
| Judul | API admin perangkat display |
| Slice | `MVP-1` — pengelolaan perangkat display |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/backend-roadmap.md` — kartu `BE-NV-002` |
| Trace | `FR-NV-001`–`005`; `NV-DEC-016`, `022`, `030`; `contracts/api-contract.md` §1; `contracts/state-transition-matrix.md` §1; `VAL-NV-01`–`08` |
| Contract version | `0.1.0` — `approved` (Rizki Gunawan, 2026-10-28) |
| Dependency | `BE-NV-001` ✅ (commit `00cbca33`) |
| Klasifikasi | `MEDIUM` — skor 7: satu repository (0), berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika sedang (1), endpoint baru sesuai kontrak (1), persistence existing (1), auth berkaitan — akun login perangkat (1), UI tidak ada (0), dinaikkan konservatif karena membuat akun Identity |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/NurseViewManagement/{Constants,Services,Controllers,DTOs}`, `Program.cs` (registrasi service, restu `NV-DEC-030`), laporan dan status roadmap |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `4af76bcb` (branch `rizkiG`), perubahan belum di-commit |
| Tanggal | 2026-10-29 |
| Status | ✅ **SELESAI** — 8 endpoint sesuai kontrak, QBE Strict `PASS`, build Rizki `PASS`, uji HTTP 29 Okt 12/12 `PASS` (dua putaran; putaran 2 sesudah data lantai diisi). Ini verifikasi developer, bukan UAT |

---

## 1. Masalah yang diperbaiki

Tabel perangkat sudah ada (`BE-NV-001`), tetapi belum ada cara apa pun untuk mengisinya. Admin
IT belum bisa mendaftarkan TV, membuat login untuk TV itu, maupun mematikannya bila TV hilang.

**Contoh.** TV di nurse station lantai 4 dicuri malam hari. Tanpa API ini, tidak ada cara
mencabut akses TV itu selain menghapus data langsung di database, yang dilarang.

---

## 2. Proses bisnis

**Tujuan:** admin IT mengelola TV display Nurse View dari awal pemasangan sampai dinonaktifkan.

**Pelaku:** Admin IT dengan hak `NurseViewDisplayDevice : Read/Create/Update`.

**Langkah normal:**

1. Admin membuka daftar lantai yang dikenal sistem, diambil dari `FloorCode` kamar aktif.
   Contoh: lantai `3` dengan 6 kamar dan 20 bed.
2. Admin mendaftarkan perangkat "TV Nurse Station Lantai 3" untuk lantai `3`. Sistem menerbitkan
   kode dari number series, misalnya `NVW-000001`, dengan status `PendingLogin` (Belum ada
   login) dan `Version = 0`.
3. Admin menekan Buat Login dan mengirim `version = 0`. Sistem membuat akun `nvw-000001`
   dengan kata sandi acak, misalnya `Nvw@3F9A…aA1!`, yang **hanya tampil di response ini**.
   Status berubah menjadi `Active` dan `Version` menjadi `1`.
4. Bila TV hilang, admin menonaktifkan dengan alasan "TV hilang, laporan satpam no. 45". Status
   berubah menjadi `Inactive`, akun dikunci 100 tahun, dan security stamp diganti.
5. Saat TV pengganti datang, admin menekan Atur Ulang Login. Sistem memberi kata sandi baru,
   membuka kunci akun, dan status kembali `Active`.

**Jalur tidak normal:**

| Kejadian | Hasil | Kode |
| --- | --- | --- |
| Lantai tidak ada di data kamar, misalnya `99` | "Lantai tidak dikenal…" | `422` |
| Dua admin mengubah perangkat yang sama; yang kedua mengirim versi lama | "Data perangkat sudah diubah orang lain…" | `409` |
| Buat Login pada perangkat yang sudah punya login | "Perangkat ini sudah punya login. Gunakan Atur Ulang Login." | `409` |
| Atur Ulang Login pada perangkat `PendingLogin` | "Perangkat belum punya login. Gunakan Buat Login." | `409` |
| Nonaktifkan perangkat yang sudah nonaktif | "Perangkat sudah nonaktif." | `409` |
| Alasan nonaktif kurang dari 5 karakter | "Alasan menonaktifkan wajib diisi (5–500 karakter)." | `400` |
| Akun dengan nama `nvw-000001` sudah ada di Identity | "Akun login untuk kode perangkat ini sudah ada…" | `409` |
| Number series gagal menerbitkan kode | "Kode perangkat gagal diterbitkan. Coba lagi." | `409` |

**Transaksi.** Buat login, atur ulang login, dan nonaktifkan dijalankan dalam satu transaksi
database bersama perubahan akun Identity. Bila salah satu gagal, akun maupun perangkat tidak
berubah.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Governance: `AGENTS.md`, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, registry; suite
  skill `API_RULES.md`, `role-access-rules.md`, `TASK_RULES.md`, `REVIEW_RULES.md`
- Pola `NEW CODE`: `Areas/HealthServices/HemodialysisManagement/Controllers/HmdMachineController.cs`,
  `HmdHttp.cs`, `Services/HmdOperationResult.cs`, `HmdResourceService.cs`,
  `HmdServiceSupport.cs`, `Constants/HemodialysisPermissions.cs`, `DTOs/HmdResourceDtos.cs`
- Number series: `Areas/HealthServices/MedicalCheckupManagement/Services/McuOrderNumberService.cs`,
  `Areas/Platform/NumberSeriesManagement/Services/NumberSeriesAllocator.cs`
- Alur login (pola saja): `Areas/Administrator/MasterData/Controllers/QueueDisplayDeviceController.cs`
  (buat login, reset login, generator kata sandi)
- `Program.cs` (registrasi service Hmd/Mcu, opsi Identity), `Responses/PagedResult.cs`,
  `Responses/ApiResponse.cs`, `Areas/HealthServices/MasterData/Services/FloorPlanRoomNaming.cs`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/NurseViewManagement/Constants/NurseViewPermissions.cs` | **Baru.** Kode modul `HEALTH_SERVICE_NURSE_VIEW`, resource `NurseViewDisplayDevice`, `NurseViewVitalSignInterval` |
| `Areas/HealthServices/NurseViewManagement/Services/NvwResult.cs` | **Baru.** Hasil operasi modul (pola `HmdResult`), kode galat `VAL-NV-*`, dan pesan persis dari validation matrix |
| `Areas/HealthServices/NurseViewManagement/Services/NvwDisplayDeviceService.cs` | **Baru.** Daftar, lantai, detail, buat, ubah, buat login, atur ulang login, nonaktifkan. Kode dari `NumberSeriesAllocator` (`NVW_DISPLAY_DEVICE`, awalan `NVW`, 6 digit, `NEVER`). Cek `Version` request + token konkurensi EF |
| `Areas/HealthServices/NurseViewManagement/Controllers/NvwHttp.cs` | **Baru.** Pemetaan hasil ke `ApiResponse<T>` dan logging aksi (pola `HmdHttp`) |
| `Areas/HealthServices/NurseViewManagement/Controllers/NvwDisplayDeviceController.cs` | **Baru.** 8 endpoint api §1; tanpa akses `ApplicationDbContext` langsung |
| `Areas/HealthServices/NurseViewManagement/DTOs/NvwDisplayDeviceDtos.cs` | **Baru.** Query, request, response sesuai api §1 |
| `Program.cs` | +3 baris: `using` modul dan `AddScoped<NvwDisplayDeviceService>()` sesudah blok service Mcu. Tidak ada baris lain berubah |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 8 endpoint baru persis seperti `api-contract.md` §1. **Delta:** tidak ada endpoint baseline master data (`filters/metadata`, `summary`, `options`, `PATCH status`, `DELETE`) — sengaja, karena kontrak yang disetujui memakai aksi bernama dan melarang hapus (state matrix §1 "Hapus perangkat: tidak tersedia di MVP") |
| Database | Tidak ada schema baru. Menulis `NvwDisplayDevice`, `AspNetUsers` (lewat `UserManager`), dan number series `NVW_DISPLAY_DEVICE` (lahir saat alokasi pertama). Tidak ada migration |
| Keamanan/Auth | Membuat akun Identity per perangkat. Kata sandi hanya di `NvwDisplayDeviceLoginResponse`, tidak disimpan di tabel modul, dan tidak masuk payload log (dicatat hanya `DeviceId`, `DeviceCode`, `LoginUserName`). **Sampai `BE-NV-003` selesai,** akun perangkat login sebagai akun biasa tanpa hak akses apa pun dan belum dialihkan ke papan |

---

## 4. Dokumentasi endpoint

#### Health Services / Nurse View Management / Display Device

Base URL: `api/v1/health-services/nurse-view-management/display-devices`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar perangkat; saring `floorCode`, `status`, `search`; urut `sortBy`/`sortDirection` | `NurseViewDisplayDevice : Read` |
| `GET` | `/floors` | Pilihan lantai dari `FloorCode` kamar aktif, beserta jumlah kamar dan bed | `NurseViewDisplayDevice : Read` |
| `GET` | `/{id}` | Detail, jejak login/nonaktif, `Version`, `AvailableActions` | `NurseViewDisplayDevice : Read` |
| `POST` | `/` | Daftarkan perangkat (`201`) | `NurseViewDisplayDevice : Create` |
| `PUT` | `/{id}` | Ubah nama, lantai, lokasi, keterangan | `NurseViewDisplayDevice : Update` |
| `POST` | `/{id}/login` | Buat login; kata sandi tampil sekali | `NurseViewDisplayDevice : Update` |
| `POST` | `/{id}/login/reset` | Atur ulang login; mengaktifkan kembali perangkat nonaktif | `NurseViewDisplayDevice : Update` |
| `PATCH` | `/{id}/deactivate` | Nonaktifkan; akun dikunci | `NurseViewDisplayDevice : Update` |

Kode status: `200`/`201` berhasil · `400` isian salah · `403` tanpa hak akses · `404` perangkat
tidak ada · `409` aksi tidak sah untuk status sekarang atau versi kedaluwarsa · `422` lantai
tidak dikenal.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 7 berkas (6 baru + `Program.cs`) | VIOLATION 0 · REVIEW 0 · `Final result: PASS` | `PASS` | Keluaran checker 2026-10-29; diulang untuk service sesudah perbaikan kecil: `PASS` |
| Pasangan `[AccessAction]` arg-1 ↔ `[AccessPermission]` arg-2 ↔ `ControllerName` | 8/8 cocok (`Read`, `Create`, `Update` × `NurseViewDisplayDevice`) | `PASS` | Pemeriksaan manual atas controller |
| Kata sandi tidak masuk log | Payload log tanpa `Password` | `PASS` | Pemeriksaan `NvwDisplayDeviceController.cs` |
| Nol komentar baris | Nol hasil | `PASS` | `grep` |
| `dotnet build ./QuilvianSystemBackend.sln -p:RunAnalyzers=false` | Lulus | `PASS (pernyataan owner)` | Rizki menyatakan sudah build; `bin/Debug/net9.0/QuilvianSystemBackend.dll` 2026-10-29 10:14 lebih baru dari source terakhir (`NvwDisplayDeviceService.cs` 09:34); proses backend mulai 10:31 dan kedelapan route menjawab |
| `bash tools/authorization-verifier/verify-authorization.sh` | Belum | `NOT RUN` | Dijalankan terpisah sesudah build, di luar sesi uji ini |
| Uji HTTP alur penuh | 12/12 | `PASS` | Hasil per skenario di bawah |

Uji manual: `REQUIRED`.

### Skenario uji (HTTP, akun dengan hak `NurseViewDisplayDevice`) — hasil 2026-10-29

Uji dijalankan agen dengan skrip Node/Playwright, tanpa SQL tulis. Bukti ada di
`QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-nv-mvp1-20261029/`; folder ini di-ignore
git, jadi tidak ikut commit. Aktor berhak: `QUILVIAN_SUPERADMIN_*`, karena kartu uji FE-NV-002 §6
mengizinkan SuperAdmin. Akibatnya uji ini **tidak membuktikan** konfigurasi Akses Role peran Admin
IT; itu langkah operasional tersendiri (traceability baris "Pemberian hak akses peran Admin IT").
Aktor tanpa hak: `QUILVIAN_LOKET_*`. Akun `QUILVIAN_PERAWAT_*` tidak bisa dipakai sebagai aktor
tanpa hak: per 29 Okt akun itu memegang tiga entri `NurseViewDisplayDevice` dan dijawab `200`.

**Putaran 1 (siang, 10.50–10.53 WIB) — tertahan data.** `GET /floors` dijawab `200` dengan `[]`.
Ke-68 kamar aktif saat itu ber-`floorCode = null` (`00-cek-kamar.json`). Kode service benar membaca
`MstRoom.FloorCode`, jadi ini bukan cacat `BE-NV-002`. Yang bisa dijalankan saat itu hanya no. 3 dan
11.

**Putaran 2 (11.47–11.51 WIB) — sesudah Rizki mengisi data lantai.** `GET /floors` kini memuat
lantai 2, 3, 4, 5, 8, dan 9 (contoh: Lantai 3 = 29 kamar, 66 bed). Urutan uji: layar FE-NV-002 §6
dulu (menerbitkan `NVW-000001`), lalu skrip HTTP ini (`NVW-000002`). Kata sandi hanya dipegang di
memori skrip. Di JSON, setiap ruas `password` ditulis `<disamarkan>`; skrip juga membatalkan
penulisan bila menemukan kata sandi di bukti. Pencarian `Nvw@<hex>` atas seluruh folder bukti: nol.

| No | Langkah | Hasil yang diharapkan | Hasil 29 Okt | Bukti |
| ---: | --- | --- | --- | --- |
| 1 | `GET /floors` | Lantai dari data kamar, misalnya `3` | `PASS` (putaran 2): `200`, lantai `2`,`3`,`4`,`5`,`8`,`9`. Putaran 1: `[]` karena data | `BE-lanjutan-2026-10-29T04-50-19-415Z.json` |
| 2 | `POST /` `{ "deviceName": "TV NS Lantai 3", "floorCode": "3" }` | `201`, `deviceCode` `NVW-000001`, status `PendingLogin`, `version 0` | `PASS`: `201`, `NVW-000002`, `deviceStatus` 1 (`PendingLogin`), `version` 0. Kode `000002` karena `000001` sudah diterbitkan uji layar beberapa menit sebelumnya; nomornya berurutan dan request `422` no. 3 tidak memakan nomor | idem |
| 3 | `POST /` dengan `floorCode` `"99"` | `422` VAL-NV-02 | `PASS`: `422`, "Lantai tidak dikenal. Pilih lantai yang terdaftar pada data kamar."; `totalData` 0 → 0 (GET sebelum/sesudah `200`) | `BE-03-floor-99-2026-10-29T03-52-39-321Z.json` |
| 4 | `POST /{id}/login` `{ "version": 0 }` | `200`, `password` terisi, `version 1` | `PASS`: `200`, kata sandi terisi, `version` 1, `loginUserName` `nvw-000002` | `BE-lanjutan-…json` |
| 5 | Ulangi langkah 4 | `409` VAL-NV-05 | `PASS`: `409`, "Perangkat ini sudah punya login. Gunakan Atur Ulang Login." | idem |
| 6 | `GET /{id}` | Tanpa `password`; status `Active`; `availableActions` berisi `ResetLogin` | `PASS`: `200`, nol ruas `password`, `deviceStatus` 2, `availableActions` memuat `ResetLogin` | idem |
| 7 | `PUT /{id}` dengan `version 0` | `409` VAL-NV-04 | `PASS`: `409`, "Data perangkat sudah diubah orang lain. Muat ulang lalu coba lagi." | idem |
| 8 | `PATCH /{id}/deactivate` `{ "reason": "TV", "version": 1 }` | `400` (alasan < 5 karakter) | `PASS`: `400` | idem |
| 9 | `PATCH /{id}/deactivate` `{ "reason": "TV hilang", "version": 1 }` | `200`, status `Inactive`; login `nvw-000002` ditolak (terkunci) | `PASS`: `200`, `deviceStatus` 3; login `nvw-000002` → `401` "Akun terkunci sementara karena terlalu banyak percobaan login gagal.", tanpa cookie | idem |
| 10 | `POST /{id}/login/reset` `{ "version": 2 }` | `200`, kata sandi baru, status `Active`; login dengan sandi lama ditolak, sandi baru diterima | `PASS`: `200`, sandi baru berbeda; sandi lama `401` "Email atau password salah."; sandi baru `200` "Login berhasil." + cookie. Respons reset tidak memuat ruas status (bentuknya `deviceId`, `deviceCode`, `loginUserName`, `password`, `version`), jadi status dibaca lewat `GET /{id}`: `deviceStatus` 2 "Aktif", `version` 3, jejak nonaktif tetap tersimpan | idem; `BE-10-status-2026-10-29T04-50-42-882Z.json` |
| 11 | Akun tanpa hak `NurseViewDisplayDevice` memanggil `GET /` | `403` | `PASS`: LOKET `403` "Anda tidak memiliki akses ke menu atau fitur ini." pada `GET /` dan `GET /floors`; ADMISI, PENERIMA, DOKTER, DOKTER2 juga `403`. Akun perangkat `nvw-000001` yang aktif pun `403` (FE §6 no. 16) | `00-cek-akun.json`; `run-…03-51-41-193Z/S19.json` |
| 12 | Periksa log aplikasi | Tidak ada kata sandi | `PASS`: `Logs/quilvian-backend-20261029.json` dipindai skrip dengan kata sandi di memori. Keempat kata sandi (2 uji layar + 2 uji HTTP) tidak ditemukan, pola `Nvw@<hex>` nol, ruas `"password":"…"` nol; selama uji tertulis 91+ baris log baru, jadi lognya memang aktif | `BE-lanjutan-…json` (`pindaiLog`); `run-lanjutan-…/S20-pindai-log-layar.json` |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `FR-NV-001` daftarkan, kode number series, lantai dikenal | Terpenuhi | §5 no. 1–3 |
| `FR-NV-002` / `NV-AC-018` buat login sekali, sandi tampil sekali | Terpenuhi | §5 no. 4–6 |
| `FR-NV-003` atur ulang login | Terpenuhi | §5 no. 10 |
| `FR-NV-004` nonaktifkan beralasan, akun dikunci | Terpenuhi untuk bagian admin | §5 no. 8–9. Penolakan runtime papan baru lengkap di `BE-NV-003` |
| `FR-NV-005` online/offline | Terpetakan ke source | `IsOnline` (≤ 3 menit); `LastOnlineAt` baru diisi di `BE-NV-003` |
| `VAL-NV-01`–`08` | Terpenuhi untuk yang diuji | `02`, `04`, `05`, `07` terbukti di §5 (pesan persis); sisanya terpetakan ke source |
| `UAT-02`, `UAT-03` | Bergantung | `UAT-02` butuh `BE-NV-003` + `FE-NV-003`; `UAT-03` dapat diuji lewat Swagger |
| DoD — build Rizki lulus | Terpenuhi | §5, pernyataan owner + waktu DLL |
| DoD — uji HTTP | Terpenuhi | §5: 12/12 `PASS` |
| DoD — laporan ada | Terpenuhi | Berkas ini |

**Backend Governance Preflight:** Area `HealthServices` · Module `NurseViewManagement` · prefix
`Nvw` `ACTIVE` · applicability `NEW CODE`; `Program.cs` = `TOUCHED LEGACY` (hanya tambahan,
restu `NV-DEC-030`).

**QBE yang berlaku dan dipatuhi:** `QBE-SVC-001` (controller → service), `QBE-API-001`,
`QBE-PERM-001`, `QBE-LOG-001` (log aksi + aktor), `QBE-DTO-001`, `QBE-VAL-001`, `QBE-TXN-001`
(transaksi akun + perangkat), `QBE-CODE-001`/`002`/`003`/`006` (kode dari provider bersama,
bukan controller, bukan `Max+1`), `QBE-PAGE-001`, `QBE-DEL-001` (tanpa hapus; nonaktif beraudit).

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Sampai `BE-NV-003` selesai, akun perangkat yang dibuat di sini bisa login ke aplikasi sebagai akun biasa (tanpa hak akses, tanpa redirect ke papan). Jangan bagikan kredensialnya di luar uji |
| Masalah yang diketahui | Login perangkat yang dinonaktifkan ditolak dengan pesan "Akun terkunci sementara karena terlalu banyak percobaan login gagal." Pesan ini menyesatkan, karena penyebabnya penonaktifan (lockout 100 tahun), bukan percobaan gagal. Pesan itu berasal dari `AuthController` bersama, jadi pertimbangkan untuk `BE-NV-003`, yang memang menyentuh login perangkat. Bukan skenario yang gagal |
| Risiko tersisa | Opsi password Identity (`RequireDigit`, panjang ≥ 8) dipenuhi pola `Nvw@<16 hex>aA1!`; bila kebijakan password berubah, pembuatan login bisa gagal dengan pesan Identity |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M Program.cs`; `?? Areas/HealthServices/NurseViewManagement/{Constants,Controllers,DTOs,Services}/`; laporan dan roadmap di `docs/module-blueprints/nurse-view/` |
| Data uji yang tertinggal di DB dev | `NVW-000001` "TV NS Lantai 3 (tab 1)", Lantai 4, Aktif; `NVW-000002` "TV NS Lantai 3 (uji HTTP)", Lantai 3, Aktif. Akun Identity `nvw-000001`/`nvw-000002` aktif, dan kata sandinya tidak disimpan di mana pun. Hapus perangkat tidak tersedia; bila perlu, nonaktifkan dari layar |
| Langkah berikutnya | `authorization-verifier` sesudah build (terpisah). `BE-NV-004` (interval) atau `BE-NV-003` (login perangkat) |
