# Laporan Perubahan Backend — `BE-FIN-076`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-076` |
| Judul | Ambang pembayaran langsung dapat dibaca dan diubah, dan perubahannya selalu berjejak |
| Slice | `REV-14D` — `EPIC FIN-23`, pembayaran langsung berkontrol beserta bukti |
| Roadmap | `roadmap/01-backend-roadmap.md`, bagian "Task REV-14D — `EPIC FIN-23`" |
| Trace | `FIN-DEC-134`; `FIN-DES-086` |
| Contract version | `FIN-API-1.6` F.4 (kedua endpoint, status naik dari "Rencana" menjadi tersedia); `FIN-VAL-1.8` `FIN-VAL-205`, `FIN-VAL-206`. `contract_status: approved 2026-10-02 (Yasmin)` |
| Dependency | `BE-FIN-074` 🟡 (model `MstDirectPaymentThreshold` — source lengkap, build belum diverifikasi) |
| Klasifikasi | `MEDIUM` — repo 0 + berkas diperiksa 9-20 (1, master data Finance sudah dikenal dari task sebelumnya) + berkas diubah 4 (1) + logika sedang (1, CRUD single-row dengan validasi) + kontrak API diubah (2, dua endpoint baru) + database tidak ada (0) + auth berkaitan bukan inti (1, resource/action baru tetapi pola CRUD master data standar) + UI tidak ada (0) = 6 |
| Task mode | `BACKEND` (target tulis backend; frontend tidak disentuh) |
| Target tulis | `NewQuilvianSystemBackend` — lihat daftar lengkap bagian 3.2 |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | `0e256765` (working tree membawa perubahan tidak ter-commit dari task REV-14A/14B/14C/14D sebelumnya — lihat bagian 7) |
| Tanggal | 2 Oktober 2026 |
| Status | 🟡 **SEBAGIAN.** Source lengkap; `dotnet build` **NOT RUN** atas permintaan eksplisit pengguna; dependency `BE-FIN-074` sendiri juga belum ter-build-verifikasi — lihat bagian 5 |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | Corporate |
| Module | FinanceManagement |
| Submodule | `MasterData` |
| Owner/prefix pada registry | `Mst` — `ACTIVE` sejak 2026-09-04 (baris registry "Master / Reference / MasterData"), ditegaskan ulang 2026-09-17 bahwa `FinanceManagement/MasterData` tercakup baris `Mst` umum ini (lihat laporan `BE-FIN-074` bagian preflight) |
| Keberlakuan | `TOUCHED LEGACY`/`NEW CODE` campuran: controller dan service baru (`NEW CODE`) di atas model yang sudah ada (`BE-FIN-074`, tidak diubah) |
| Status registry | `ACTIVE`. **Nol** `QBE-MOD-002`/`003` — nol entity/model baru, nol folder submodule baru pada task ini |
| QBE ID yang berlaku | `QBE-CODE-002`/`003` (tidak relevan — tidak ada penomoran/nomor urut pada task ini); resource/action permission **persis** mengikuti `permission-audit-matrix.md` G.2 yang sudah menspesifikasikan atribut `[AccessController]` apa adanya — nol penyimpangan argumen `[AccessPermission]` vs `[AccessAction]`/`[AccessController].ControllerName` (role-access-rules.md) |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, `MstDirectPaymentThreshold` (`BE-FIN-074`) hanya berupa tabel kosong tanpa jalur
baca maupun tulis. Tidak ada cara bagi petugas berwenang untuk menetapkan atau mengubah ambang nilai
pembayaran langsung, dan tidak ada cara bagi `BE-FIN-077`/`078` (nanti) untuk membaca ambang yang
berlaku saat memvalidasi sebuah pembayaran langsung.

---

## 2. Proses bisnis

**Tujuan.** Menyediakan jalur baca ambang aktif dan jalur ubah (sekaligus tetapkan pertama kali) yang
mewajibkan alasan perubahan, tanpa tabel riwayat terpisah — kolom audit baris yang sama sudah
menjawab "siapa dan kapan mengubah terakhir".

**Pelaku.** `DirectPaymentThresholdService` (baru), dipanggil `DirectPaymentThresholdController` (baru,
dua endpoint).

**Model data: satu baris, diperbarui di tempat — bukan riwayat implisit.** `MstDirectPaymentThreshold`
dirancang `FIN-DES-086` sebagai master dengan **tepat satu baris aktif**, dijaga unique index partial
(`IX_MstDirectPaymentThreshold_Active`, `BE-FIN-074`). Desain ini SENGAJA menolak membuat tabel riwayat
terpisah: "kolom audit `IdentityModel` sudah menjawab perubahan terakhir". Konsekuensinya bagi task
ini: `PUT` **memperbarui baris yang sama di tempat** (mengubah `Amount`/`ChangeReason`/`EffectiveFrom`
beserta `UpdateBy`/`UpdateDateTime` pada baris yang sudah ada), **bukan** menonaktifkan baris lama dan
menyisipkan baris baru — pendekatan kedua itu diam-diam akan menjadi tabel riwayat implisit yang
justru ditolak desainnya.

**Pemicu dan langkah berurutan:**

1. **`GET /`** — membaca baris `IsActive = true`. Tanpa baris aktif (belum pernah ditetapkan), melempar
   `404` dengan pesan yang menyebut jalur pembayaran langsung belum dapat dipakai — **fail-closed**,
   bukan dianggap tak terbatas (`FIN-DES-086`).
2. **`PUT /`** — dua pemeriksaan independen, keduanya **wajib** sebelum baris disentuh:
   - `ChangeReason` kosong/spasi → `422` (`FIN-VAL-205`).
   - `Amount` nol atau negatif → `400` (`FIN-VAL-206`).
   Baru sesudah keduanya lolos: baris aktif dicari. Bila belum ada, baris baru dibuat (`IsActive = true`,
   `CreateBy`/`CreateDateTime` diisi). Bila sudah ada, baris itu diperbarui di tempat
   (`UpdateBy`/`UpdateDateTime` diisi). Perubahan dicatat `LoggerService.AuditAsync` — mengikuti pola
   `CurrencyService`/`MstCurrency` (master data Finance lain yang sudah mencatat audit untuk mutasinya),
   **bukan** pola `FinanceSubledgerControlAccountService`/`FinanceOpeningBalanceService` yang ternyata
   tidak memanggil `LoggerService` sama sekali — keduanya diperiksa sebagai pembanding sebelum memilih
   pola yang benar untuk rumpun master data.

**Keputusan desain penting: `[Required]` SENGAJA tidak dipasang pada `ChangeReason`.** `[ApiController]`
mengaktifkan validasi model otomatis yang memotong permintaan dengan `400` generik sebelum action
method sempat berjalan, bila `[Required]` dipasang pada field string kosong/spasi. Itu akan
mengalahkan kontrak `FIN-VAL-205` yang eksplisit meminta `422` beserta pesan spesifik. Pemeriksaan
kosong dilakukan manual di `DirectPaymentThresholdService.UpdateAsync` sebagai gantinya.

**Aturan yang berlaku.** `FIN-DEC-134` (ambang dapat diubah pejabat berwenang dengan alasan dan jejak),
`FIN-DES-086` (satu baris aktif, nol tabel riwayat, fail-closed tanpa baris aktif).

**Jalur tidak normal.** Nilai ambang itu sendiri (`FIN-OQ-074`) belum pernah ditetapkan pemilik modul —
sesuai roadmap, ini **tidak** menahan task ini; perilaku tanpa ambang (seluruh pembayaran langsung
ditolak `404`/`FIN-VAL-197` di `BE-FIN-077`/`078` nanti) memang hasil yang diinginkan.

**Hasil akhir.** Ambang dapat ditetapkan pertama kali dan diubah berikutnya lewat satu endpoint `PUT`
yang sama, selalu mewajibkan alasan, dan selalu dapat dibaca kembali besertanya siapa/kapan
mengubahnya terakhir.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/contracts/validation-matrix.md` (`F.5`, `FIN-VAL-205`, `FIN-VAL-206`, `FIN-VAL-197` sebagai konteks pemakai ambang di `BE-FIN-077`/`078`)
- `docs/module-blueprints/finance-management/contracts/api-contract.md` (`F.4`, Base URL, dua endpoint, tabel kode status)
- `docs/module-blueprints/finance-management/contracts/permission-audit-matrix.md` (`[AccessController]` persis baris 910-914, `G.1` resource/action, `G.3` baris 947-948)
- `docs/module-blueprints/finance-management/02-backend-architecture.md` (`FIN-DES-086` — dibaca ulang dari `BE-FIN-074`)
- `Areas/Corporate/FinanceManagement/MasterData/Services/CurrencyService.cs` (templat CRUD master data Finance, termasuk pola `AuditAsync` untuk mutasi — dipakai sebagai acuan utama)
- `Areas/Corporate/FinanceManagement/MasterData/Controllers/CurrenciesController.cs` (templat scaffold controller)
- `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceSubledgerControlAccountService.cs`, `FinanceOpeningBalanceService.cs` (diperiksa untuk membandingkan pola logging — ternyata **tidak** memanggil `LoggerService` sama sekali, dipakai untuk memutuskan pola mana yang relevan untuk rumpun `MasterData`, bukan rumpun `AccountingIntegration`)
- `Areas/Corporate/FinanceManagement/MasterData/Models/MstDirectPaymentThreshold.cs`, `Repositories/Configurations/.../MstDirectPaymentThresholdConfiguration.cs` (dari `BE-FIN-074`, dipakai apa adanya — **tidak diubah**)
- `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` (titik registrasi `AddScoped`)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/MasterData/DTOs/DirectPaymentThresholdDtos.cs` (baru) | `UpdateDirectPaymentThresholdRequest` (`Amount`, `ChangeReason` **tanpa** `[Required]` — lihat bagian 2, `EffectiveFrom`), `DirectPaymentThresholdResponse` (termasuk `LastChangedBy`/`LastChangedAt` turunan dari `UpdateBy`/`UpdateDateTime` atau `CreateBy`/`CreateDateTime`), dua exception: `DirectPaymentThresholdValidationException` (422), `DirectPaymentThresholdBadRequestException` (400) |
| `Areas/Corporate/FinanceManagement/MasterData/Services/DirectPaymentThresholdService.cs` (baru) | `GetActiveAsync` (404 fail-closed tanpa baris aktif), `UpdateAsync` (dua validasi + upsert baris tunggal di tempat + `AuditAsync`) |
| `Areas/Corporate/FinanceManagement/MasterData/Controllers/DirectPaymentThresholdController.cs` (baru) | Dua endpoint: `GET /` (Read), `PUT /` (Update). `[AccessController]` disalin **persis** dari `permission-audit-matrix.md` baris 910-914 |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | Menambah `services.AddScoped<DirectPaymentThresholdService>();` bersebelahan dengan `CurrencyService` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Dua endpoint baru**, status naik dari "Rencana (belum tersedia)" menjadi tersedia: `GET /master-data/direct-payment-threshold`, `PUT /master-data/direct-payment-threshold`. Bentuk, kode status, dan pesan error mengikuti `FIN-API-1.6` F.4 dan `FIN-VAL-1.8` F.5 persis |
| Database | **NOT APPLICABLE.** Nol model/migration baru pada task ini — memakai `MstDirectPaymentThreshold` dari `BE-FIN-074` apa adanya |
| Keamanan/Auth | Resource/action baru ditegakkan: `MstDirectPaymentThreshold : Read` (GET), `MstDirectPaymentThreshold : Update` (PUT) — nol hardcode role/department/UserType, murni `[AccessPermission]`, mengikuti pola CRUD master data standar yang sudah berjalan (Currency/BankAccount/PettyCashCategory) |

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / Master Data / Direct Payment Threshold

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Ambang aktif beserta alasan dan jejak perubahan terakhir. `404` bila belum pernah ditetapkan | `MstDirectPaymentThreshold : Read` |
| `PUT` | `/` | Mengubah (atau menetapkan pertama kali) ambang; `ChangeReason` wajib | `MstDirectPaymentThreshold : Update` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Permintaan eksplisit pengguna. Dependency `BE-FIN-074` juga belum ter-build-verifikasi — risiko kumulatif dicatat bagian 7 |
| Pembacaan kode: `ChangeReason` kosong ditolak | Terverifikasi — `string.IsNullOrWhiteSpace(request.ChangeReason)` melempar `DirectPaymentThresholdValidationException` (422) sebelum baris disentuh | `PASS` | `DirectPaymentThresholdService.cs`, `UpdateAsync` |
| Pembacaan kode: nilai bukan angka positif ditolak | Terverifikasi — `request.Amount <= 0` melempar `DirectPaymentThresholdBadRequestException` (400) | `PASS` | `DirectPaymentThresholdService.cs`, `UpdateAsync` |
| Pembacaan kode: tanpa baris aktif, `GET` menjawab `404` | Terverifikasi — `SingleOrDefaultAsync(...) ?? throw new KeyNotFoundException(...)`, ditangkap controller dan dipetakan `NotFound` | `PASS` | `DirectPaymentThresholdService.cs` `GetActiveAsync`; `DirectPaymentThresholdController.cs` `Get` |
| Pembacaan kode: `[Required]` sengaja dihindari untuk menjaga kode status kontrak | Terverifikasi — `UpdateDirectPaymentThresholdRequest.ChangeReason` hanya `[MaxLength(500)]`, nol `[Required]` | `PASS` | `DirectPaymentThresholdDtos.cs` |
| Pembacaan kode: perubahan tercatat logger tanpa nilai sensitif | Terverifikasi — `AuditAsync` mencatat `ThresholdId`, `Amount`, `ChangeReason`, `EffectiveFrom`, `ActorUserId`; tidak ada kolom bertanda Sensitif pada `erd/data-dictionary.md` R14.8 untuk entity ini | `PASS` | `DirectPaymentThresholdService.cs`, blok `AuditAsync` |
| Pembacaan kode: baris diperbarui di tempat, bukan riwayat implisit | Terverifikasi — `UpdateAsync` mencari baris `IsActive=true` dan memperbarui field yang sama; nol baris baru dibuat ketika baris aktif sudah ada | `PASS` | `DirectPaymentThresholdService.cs`, blok `if (entity == null) {...} else {...}` |
| Pembacaan kode: `[AccessController]` sama persis dengan kontrak | Terverifikasi karakter demi karakter terhadap `permission-audit-matrix.md` baris 910-914 | `PASS` | `DirectPaymentThresholdController.cs` |

Uji manual: `NOT FEASIBLE` — memerlukan runtime aplikasi aktif dan migration `BE-FIN-074` sudah
diterapkan; di luar cakupan perubahan source tanpa eksekusi aplikasi pada task ini.

**Tidak dijalankan:** `dotnet build` (permintaan eksplisit pengguna), uji manual end-to-end
(`NOT FEASIBLE`), `dotnet test` (backend tidak memelihara project automated test).

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `ChangeReason` kosong ditolak | Terpenuhi (source) | Bagian 2, 5 |
| Nilai bukan angka positif ditolak | Terpenuhi | Bagian 5 |
| Tanpa baris aktif, `GET` menjawab `404` | Terpenuhi | Bagian 5 |
| Perubahan tercatat logger tanpa nilai sensitif | Terpenuhi | Bagian 5 |
| `dotnet build` PASS | **Belum terpenuhi** — `NOT RUN` atas permintaan eksplisit pengguna | Bagian 5 |
| Laporan task tracked ada | Terpenuhi | Berkas ini |

Butir DoD `dotnet build PASS` **belum terpenuhi** karena sengaja tidak dijalankan atas instruksi
eksplisit pengguna. Ini **bukan** pengecualian DoD permanen (`status-task-roadmap.md` §2.2).

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Risiko kumulatif tanpa build** berlanjut dari `BE-FIN-074`/`075`: model dan migration `MstDirectPaymentThreshold` yang dipakai task ini juga belum pernah di-build. Disarankan menjalankan `dotnet build` sebelum melanjutkan ke `BE-FIN-077` |
| Masalah yang diketahui | `NONE` — tidak ditemukan gap kontrak baru pada task ini |
| Risiko tersisa | Nilai ambang (`FIN-OQ-074`) belum ditetapkan pemilik modul — endpoint `GET`/`PUT` akan berfungsi begitu `dotnet build` dan migration berhasil, tetapi **seluruh** pembayaran langsung (`BE-FIN-077`/`078`, belum dibangun) akan tetap ditolak sampai ada petugas yang memanggil `PUT` untuk pertama kali menetapkan nilainya |
| Perubahan sampingan | `NONE` — seluruh berkas yang disentuh murni penambahan untuk task ini; `BillingManagementServiceCollectionExtensions.cs` yang sudah membawa perubahan tidak ter-commit dari task sebelumnya tidak dipulihkan maupun diubah bagian lamanya |
| Interupsi | `NONE` |
| Status Git | `git status --short` menunjukkan empat berkas task ini (tiga baru, satu dimodifikasi), tercampur dengan modifikasi tidak ter-commit dari task `BE-FIN-058`..`075` yang memang belum di-commit sejak sebelum task ini dimulai. Tidak ada file di luar scope Finance Management yang tersentuh |
| Langkah berikutnya | (1) Pengguna menjalankan `dotnet build` (mencakup `BE-FIN-074`/`075`/`076` sekaligus) dan mengonfirmasi hasilnya; (2) pemilik modul memanggil `PUT /master-data/direct-payment-threshold` untuk menetapkan nilai ambang pertama kali (`FIN-OQ-074`) begitu siap; (3) `BE-FIN-077`/`078` melanjutkan dengan menyambungkan ambang dan `ProofId` ke pembayaran langsung piutang/utang |
