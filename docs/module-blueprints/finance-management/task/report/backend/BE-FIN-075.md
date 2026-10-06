# Laporan Perubahan Backend — `BE-FIN-075`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-075` |
| Judul | Petugas dapat mengunggah bukti, dan berkas yang tidak layak ditolak beserta alasan yang dapat dipahami |
| Slice | `REV-14D` — `EPIC FIN-23`, pembayaran langsung berkontrol beserta bukti |
| Roadmap | `roadmap/01-backend-roadmap.md`, bagian "Task REV-14D — `EPIC FIN-23`" |
| Trace | `FIN-DEC-139`; `FIN-DES-092` |
| Contract version | `FIN-API-1.6` F.3 (ketiga endpoint, status naik dari "Rencana" menjadi tersedia); `FIN-VAL-1.8` `FIN-VAL-214`..`221`; `FIN-PERM-1.8` H.1/H.2/H.3. `contract_status: approved 2026-10-02 (Yasmin)` |
| Dependency | `BE-FIN-074` 🟡 (model `FinTransactionProof`, `FinanceTransactionProofOptions`, migration — source lengkap, build belum diverifikasi) |
| Klasifikasi | `HEAVY` — repo 0 + berkas diperiksa >20 (2) + berkas diubah 4 (1) + logika kompleks (2, delapan pemeriksaan berurut fail-closed) + kontrak API diubah (2, tiga endpoint baru) + database tidak ada (0) + auth dampak inti (2, resource/action baru + keamanan jalur berkas) + UI tidak ada (0) = 9 |
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
| Submodule | `Collection` (controller, service, DTO baru; model `FinTransactionProof` sudah ada dari `BE-FIN-074`) |
| Owner/prefix pada registry | `Fin` — `ACTIVE` sejak 2026-09-21 (lihat laporan `BE-FIN-074` bagian preflight) |
| Keberlakuan | `TOUCHED LEGACY`/`NEW CODE` campuran: service dan controller baru (`NEW CODE`) di atas model yang sudah ada (`BE-FIN-074`, tidak diubah) |
| Status registry | `ACTIVE`. **Nol** `QBE-MOD-002`/`003` — nol entity/model baru, nol folder submodule baru pada task ini |
| QBE ID yang berlaku | `QBE-CODE-002`/`003` (lolos — `StoredFileName` dari `Guid.NewGuid():N}`, bukan Count/Max/Last+1); resource/action permission **persis** mengikuti `permission-audit-matrix.md` G.1/G.2 yang sudah menspesifikasikan atribut `[AccessController]` apa adanya — nol penyimpangan argumen `[AccessPermission]` vs `[AccessAction]`/`[AccessController].ControllerName` (role-access-rules.md) |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, `FinTransactionProof` (`BE-FIN-074`) hanya berupa tabel kosong — tidak ada jalur
untuk mengisinya. Petugas tidak dapat mengunggah bukti pembayaran langsung sama sekali, dan tidak ada
mekanisme yang menolak berkas tidak layak (ukuran raksasa, jenis berbahaya, isi yang tidak sesuai
klaim ekstensinya) sebelum sampai ke penyimpanan.

---

## 2. Proses bisnis

**Tujuan.** Menyediakan jalur unggah bukti pembayaran yang aman — delapan pemeriksaan berurut yang
menolak berkas tidak layak dengan pesan yang dapat dipahami — beserta jalur baca metadata dan unduh
berkasnya, tanpa pernah menyediakan jalur ganti atau hapus.

**Pelaku.** `FinanceTransactionProofService` (baru) sebagai pelaksana, dipanggil
`FinanceTransactionProofsController` (baru, tiga endpoint).

**Pemicu dan langkah berurutan (unggah, `POST /transaction-proofs`):**

1. Petugas mengunggah berkas lewat `multipart/form-data` beserta `ProofType` (mis. `BUKTI-TRANSFER`).
2. Delapan pemeriksaan berjalan **berurut**, yang pertama gagal menghentikan sisanya:
   1. Berkas ada dan tidak kosong (`FIN-VAL-214`, `400`).
   2. Nama berkas ≤255 karakter (`FIN-VAL-215`, `400`).
   3. Ekstensi wajib ada (`FIN-VAL-216`, `400`).
   4. Ekstensi tidak ada pada daftar terlarang — 18 ekstensi berbahaya, ditulis ulang dari preseden
      `WorkflowFileStorageService` milik HR, **tanpa memanggilnya** (`FIN-VAL-216`, `400`).
   5. Ekstensi ada pada `FinanceManagement:TransactionProof:AllowedExtensions` (`FIN-VAL-217`, `400`).
   6. Tipe media **cocok** dengan ekstensinya — dipetakan dari tabel kanonikal tertanam
      (`.pdf→application/pdf`, `.jpg`/`.jpeg→image/jpeg`, `.png→image/png`); ekstensi yang lolos
      butir 5 tetapi tidak ada di tabel kanonikal **selalu dianggap tidak cocok** (fail-closed)
      (`FIN-VAL-218`, `400`).
   7. `MaxFileSizeBytes` **MUST** sudah dikonfigurasi — `null` berarti unggah ditolak seluruhnya
      (`FIN-VAL-220`, `503`, fail-closed, bukan tak terbatas), baru kemudian ukuran dibandingkan
      terhadap batas itu (`FIN-VAL-219`, `413`).
   8. Jalur simpan hasil penormalan (folder `finance/transaction-proofs/{tahun}/{bulan}/{guid}{ekstensi}`)
      **MUST** berada di bawah `FileStorage:UploadRootPath` (`FIN-VAL-221`, `400`, percobaan dicatat).
3. Berkas ditulis ke disk dengan nama hasil generate (`Guid.NewGuid():N` + ekstensi) — nama asli
   pengguna **tidak pernah** dipakai sebagai nama fisik, mencegah tabrakan dan menutup jalur
   manipulasi nama berkas sejak awal.
4. Baris metadata ditulis ke `FinTransactionProof`. Bila penulisan metadata gagal, berkas yang **sudah**
   tersimpan di disk dihapus (satu-satunya penghapusan berkas yang diizinkan di luar jalur manual).
5. Keberhasilan dicatat `LoggerService.AuditAsync` — `ProofId`, `ProofType`, `SizeBytes`, `MediaType`,
   pengunggah, waktu; `OriginalFileName` **dipotong** 40 karakter sebelum dicatat (H.2: tidak pernah
   dicatat utuh karena berpotensi memuat nama orang).

**Unduh (`GET /transaction-proofs/{id}`).** Membaca metadata, memeriksa berkas ada di disk, mencatat
`ProofId` + pengunduh + waktu (`AuditAsync`), lalu mengalirkan berkas lewat `FileStreamResult` —
**bukan** lewat `UseStaticFiles` publik yang sudah ada di `Program.cs`. Ini sengaja: endpoint publik
itu tidak bergerbang hak akses, sedangkan bukti pembayaran **MUST** tetap bergerbang
`FinanceTransactionProof : Read` (H.3).

**Metadata (`GET /transaction-proofs/{id}/metadata`).** Mengembalikan keterangan berkas tanpa
mengunduh isinya — tidak mencatat audit (hanya `GET .../{id}` download yang dicatat, sesuai H.2).

**Aturan yang berlaku.** `FIN-DEC-139` (daftar jenis berkas turun dari preseden repository, bukan
diciptakan baru), `FIN-DES-092` (urutan delapan pemeriksaan dan perilaku kosong dua kunci konfigurasi
yang berbeda — lihat laporan `BE-FIN-074`).

**Jalur tidak normal.** Permintaan unggah dengan `MaxFileSizeBytes` belum dikonfigurasi ditolak **503**
untuk SETIAP percobaan sampai admin mengisi konfigurasi — bukan diam-diam dianggap tak terbatas.
Percobaan jalur keluar akar penyimpanan dicatat `WarningAsync` beserta jalur mentah yang diminta —
satu-satunya tempat jalur mentah memang dicatat, karena ia bukti percobaan (H.2). **Catatan jujur:**
karena nama fisik berkas SELALU di-generate server (`Guid.NewGuid()`, bukan dari nama pengguna),
percobaan keluar-akar secara struktural **tidak dapat terjadi** pada implementasi ini — pemeriksaan
#8 tetap dipertahankan sebagai jaring pengaman invarian (pola yang sama dengan preseden HR,
`WorkflowFileStorageService`/`WfpDocumentController`, yang punya kondisi serupa), bukan dihapus
dengan alasan "tidak mungkin terjadi".

**Hasil akhir.** Bukti pembayaran dapat diunggah, dibaca metadatanya, dan diunduh — seluruhnya
bergerbang hak akses, nol jalur ganti/hapus.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/contracts/validation-matrix.md` (`G.1`, `FIN-VAL-214`..`221`)
- `docs/module-blueprints/finance-management/contracts/api-contract.md` (`F.3`, Base URL, tiga endpoint, tabel kode status, "Aturan berkas bukti")
- `docs/module-blueprints/finance-management/contracts/permission-audit-matrix.md` (`G.1`, `[AccessController]` persis, `G.2` tabel audit wajib, `H.1`/`H.2`/`H.3`)
- `docs/module-blueprints/finance-management/02-backend-architecture.md` (`FIN-DES-092` delapan pemeriksaan dan dua kunci konfigurasi — dibaca ulang dari `BE-FIN-074`)
- `Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowFileStorageService.cs` (pola `ValidateFile`, `SaveAsync`, `ResolvePhysicalPath` path-safety — **ditulis ulang**, tidak dipanggil)
- `Areas/Corporate/HumanResource/WorkforceCore/Controllers/WfpDocumentController.cs` (pola endpoint unduh bergerbang hak akses yang membaca `FileStorage:UploadRootPath` dan mengalirkan `File(stream, contentType, fileName)` — **bukan** lewat `UseStaticFiles` publik; dipakai sebagai templat persis untuk `Download`)
- `Program.cs` baris 1423-1456 (`FileStorage:UploadRootPath`, `FileStorage:PublicRequestPath`, `UseStaticFiles` — dipastikan bukti pembayaran **tidak** didaftarkan ke jalur publik ini)
- `Services/Logging/LoggerService.cs` (penuh — `AuditAsync`/`WarningAsync`, dipakai ulang untuk H.2)
- `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` (pola pemakaian `LoggerService`/`AuditAsync`/`Truncate`)
- `Areas/Corporate/FinanceManagement/Collection/Controllers/FinanceReceiptsController.cs` (templat scaffold controller Collection)
- `Areas/Corporate/FinanceManagement/Collection/Models/FinTransactionProof.cs`, `Services/FinanceTransactionProofOptions.cs` (dari `BE-FIN-074`, dipakai apa adanya — **tidak diubah**)
- `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` (titik registrasi `AddScoped`)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Collection/Dtos/TransactionProofDtos.cs` (baru) | `UploadTransactionProofRequest` (`IFormFile File`, `string ProofType`), `TransactionProofResponse` (tanpa `StoredFileName`/`RelativePath` — detail internal), tiga exception: `FinanceTransactionProofValidationException` (400), `FinanceTransactionProofTooLargeException` (413), `FinanceTransactionProofNotConfiguredException` (503) |
| `Areas/Corporate/FinanceManagement/Collection/Services/FinanceTransactionProofService.cs` (baru) | `UploadAsync` (delapan pemeriksaan berurut + simpan berkas + tulis metadata + audit log), `GetMetadataAsync`, `DownloadAsync` (baca berkas + audit log). Daftar ekstensi terlarang dan tabel kanonikal ekstensi→tipe-media ditulis ulang di sini — **nol** panggilan ke `WorkflowFileStorageService` milik HR |
| `Areas/Corporate/FinanceManagement/Collection/Controllers/FinanceTransactionProofsController.cs` (baru) | Tiga endpoint: `POST /` (Create), `GET /{id:guid}` (Read, download berkas), `GET /{id:guid}/metadata` (Read, JSON). `[AccessController]` disalin **persis** dari `permission-audit-matrix.md` G.1. **Nol** endpoint `PUT`/`DELETE` |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | Menambah `services.AddScoped<FinanceTransactionProofService>();` bersebelahan dengan `FinanceReceiptService` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Tiga endpoint baru**, status naik dari "Rencana (belum tersedia)" menjadi tersedia: `POST /transaction-proofs`, `GET /transaction-proofs/{id}`, `GET /transaction-proofs/{id}/metadata`. Bentuk, kode status, dan pesan error mengikuti `FIN-API-1.6` F.3 dan `FIN-VAL-1.8` G.1 persis |
| Database | **NOT APPLICABLE.** Nol model/migration baru pada task ini — memakai `FinTransactionProof` dari `BE-FIN-074` apa adanya |
| Keamanan/Auth | **ADA, dampak inti.** Resource/action baru ditegakkan: `FinanceTransactionProof : Create` (upload), `FinanceTransactionProof : Read` (download + metadata) — nol hardcode role/department/UserType, murni `[AccessPermission]`. Jalur unduh **sengaja tidak** lewat `UseStaticFiles` publik supaya tetap bergerbang hak akses (H.3). Pembatasan per pemilik transaksi **belum ada** — batas yang diterima sadar sesuai `permission-audit-matrix.md` H.3, **bukan** ditemukan sendiri di sini |

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / Transaction Proof

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Mengunggah bukti pembayaran lewat delapan pemeriksaan berurut; mengembalikan `ProofId` untuk dipakai pada pembayaran langsung piutang/utang | `FinanceTransactionProof : Create` |
| `GET` | `/{id:guid}` | Mengunduh berkas bukti (bergerbang hak akses, bukan lewat jalur statis publik) | `FinanceTransactionProof : Read` |
| `GET` | `/{id:guid}/metadata` | Keterangan berkas bukti tanpa mengunduh isinya | `FinanceTransactionProof : Read` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Permintaan eksplisit pengguna. Catatan tambahan: dependency `BE-FIN-074` (model `FinTransactionProof`, `ApplicationDbContextModelSnapshot.cs` hand-written) juga **belum** diverifikasi build — risiko kumulatif dicatat bagian 7 |
| Pembacaan kode: kedelapan pemeriksaan berurut, yang pertama gagal menghentikan sisanya | Terverifikasi — setiap pemeriksaan adalah blok `if` berurutan yang melempar exception dan menghentikan eksekusi method, persis urutan `FIN-DES-092` | `PASS` | `FinanceTransactionProofService.cs`, method `UploadAsync` |
| Pembacaan kode: tanpa `MaxFileSizeBytes` unggah ditolak `503`, bukan `400` dan bukan diterima | Terverifikasi — pemeriksaan `_options.MaxFileSizeBytes is not { } maxFileSizeBytes` melempar `FinanceTransactionProofNotConfiguredException` sebelum pemeriksaan ukuran apa pun dijalankan | `PASS` | `FinanceTransactionProofService.cs` baris ~138-142 |
| Pembacaan kode: tipe media diperiksa, bukan hanya ekstensi | Terverifikasi — `CanonicalMediaTypeByExtension.TryGetValue` dibandingkan terhadap `file.ContentType`; ekstensi lolos tetapi tipe media tidak cocok (atau ekstensi tidak ada di tabel kanonikal) ditolak | `PASS` | `FinanceTransactionProofService.cs` baris ~122-128 |
| Pembacaan kode: jalur keluar akar ditolak dan dicatat | Terverifikasi secara struktural — `ResolvePhysicalPath` melempar bila hasil `Path.GetFullPath` tidak diawali akar; blok catch memanggil `WarningAsync` sebelum melempar exception 400. **Catatan:** secara praktik tidak dapat dipicu pada implementasi ini karena nama fisik selalu di-generate server — lihat bagian 2 dan 7 | `PASS` (struktural; jalur gagal tidak dapat diuji manual tanpa memodifikasi kode sementara, yang dilarang kebijakan test) | `FinanceTransactionProofService.cs`, method `ResolvePhysicalPath` |
| Pembacaan kode: berkas yatim tidak tertinggal bila penulisan metadata gagal | Terverifikasi — `try/catch` di sekitar `SaveChangesAsync` memanggil `DeleteIfExists(physicalPath)` sebelum `throw` ulang | `PASS` | `FinanceTransactionProofService.cs`, blok `try { _dbContext.Set<FinTransactionProof>().Add... } catch { DeleteIfExists...; throw; }` |
| Pembacaan kode: nol endpoint `PUT`/`DELETE` | Terverifikasi — controller hanya memuat tiga action (`Upload`, `Download`, `GetMetadata`), nol `[HttpPut]`/`[HttpDelete]` | `PASS` | `FinanceTransactionProofsController.cs` |
| Pembacaan kode: unduhan tidak lewat jalur statis publik | Terverifikasi — `Program.cs` baris 1423-1456 (`UseStaticFiles`) tidak disentuh maupun direferensikan; `Download` membaca `FileStorage:UploadRootPath` sendiri lalu mengalirkan lewat `File(...)` yang bergerbang `[AccessPermission]` | `PASS` | `FinanceTransactionProofService.cs`, `FinanceTransactionProofsController.cs` |
| Pembacaan kode: `[AccessController]` sama persis dengan kontrak | Terverifikasi karakter demi karakter terhadap `permission-audit-matrix.md` baris 904-907 | `PASS` | `FinanceTransactionProofsController.cs` |
| Pemeriksaan keamanan tambahan: `[DisableRequestSizeLimit]` SENGAJA tidak dipasang | Diputuskan eksplisit — memasangnya akan membiarkan request raksasa diterima penuh oleh Kestrel sebelum pemeriksaan `MaxFileSizeBytes` sempat berjalan, bertentangan langsung dengan risiko utama task ini sendiri ("membuka pintu berkas raksasa") | `PASS` (keputusan desain, bukan hasil perintah) | `FinanceTransactionProofsController.cs`, lihat bagian 7 |

Uji manual: `NOT FEASIBLE` — memerlukan runtime aplikasi aktif, database dengan migration `BE-FIN-074`
sudah diterapkan, dan konfigurasi `FinanceManagement:TransactionProof:*` terisi; semuanya di luar
cakupan perubahan source tanpa eksekusi aplikasi pada task ini.

**Tidak dijalankan:** `dotnet build` (permintaan eksplisit pengguna), uji manual kedelapan jalur tolak
(`NOT FEASIBLE`, memerlukan runtime), `dotnet test` (backend tidak memelihara project automated test).

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Kedelapan pemeriksaan ditegakkan berurut, yang pertama gagal menghentikan sisanya | Terpenuhi (source) | Bagian 2, 5 |
| Tanpa `MaxFileSizeBytes` unggah ditolak `503`, bukan `400` dan bukan diterima | Terpenuhi | Bagian 5 |
| Tipe media diperiksa, bukan hanya ekstensi | Terpenuhi | Bagian 5 |
| Jalur keluar akar ditolak dan dicatat | Terpenuhi (struktural; tidak dapat dipicu praktik pada desain ini — lihat catatan) | Bagian 2, 5, 7 |
| Berkas yatim tidak tertinggal bila penulisan metadata gagal | Terpenuhi | Bagian 5 |
| Nol endpoint `PUT`/`DELETE` ada | Terpenuhi | Bagian 4, 5 |
| `dotnet build` PASS | **Belum terpenuhi** — `NOT RUN` atas permintaan eksplisit pengguna | Bagian 5 |
| Laporan task tracked ada | Terpenuhi | Berkas ini |

Butir DoD `dotnet build PASS` **belum terpenuhi** karena sengaja tidak dijalankan atas instruksi
eksplisit pengguna. Ini **bukan** pengecualian DoD permanen (`status-task-roadmap.md` §2.2).

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Risiko kumulatif tanpa build:** task ini dibangun di atas `FinTransactionProof`/`FinanceTransactionProofOptions` dari `BE-FIN-074`, yang migration dan `ApplicationDbContextModelSnapshot.cs`-nya juga ditulis tangan dan **belum pernah di-build**. Kesalahan kecil pada salah satu dari kedua task ini (nama kolom, tipe, namespace) baru akan terlihat saat `dotnet build` dijalankan — disarankan menjalankannya sebelum melanjutkan ke `BE-FIN-076` |
| Masalah yang diketahui | **Pemeriksaan #8 (jalur keluar akar) secara praktik tidak dapat terpicu** pada implementasi ini — nama fisik berkas selalu `Guid.NewGuid()`, bukan diturunkan dari input pengguna, sehingga tidak ada jalur bagi penyerang memengaruhi `relativePath`. Dipertahankan sebagai jaring pengaman invarian mengikuti pola preseden HR (`WorkflowFileStorageService`/`WfpDocumentController`) yang punya kondisi serupa — **bukan** dihapus dengan alasan "tidak mungkin terjadi", karena refactor di masa depan yang mengubah skema penamaan bisa saja membuatnya reachable kembali |
| Risiko tersisa | (1) Pembatasan per pemilik transaksi pada unduhan **belum ada** — diterima sadar sesuai `permission-audit-matrix.md` H.3, bukan ditemukan sendiri di sini; **MUST** disampaikan eksplisit saat pemberian hak `FinanceTransactionProof : Read`. (2) `FinanceManagement:TransactionProof:MaxFileSizeBytes` dan `FinanceManagement:TransactionProof:AllowedExtensions` (kustom) belum diisi di `appsettings.json` mana pun — endpoint upload akan **selalu** menjawab `503` sampai admin mengisinya, sesuai desain fail-closed |
| Perubahan sampingan | `NONE` — seluruh berkas yang disentuh murni penambahan untuk task ini; `BillingManagementServiceCollectionExtensions.cs` yang sudah membawa perubahan tidak ter-commit dari task sebelumnya tidak dipulihkan maupun diubah bagian lamanya |
| Interupsi | `NONE` |
| Status Git | `git status --short` menunjukkan empat berkas task ini (tiga baru, satu dimodifikasi), tercampur dengan modifikasi tidak ter-commit dari task `BE-FIN-058`..`074` yang memang belum di-commit sejak sebelum task ini dimulai. Tidak ada file di luar scope Finance Management yang tersentuh |
| Langkah berikutnya | (1) Pengguna menjalankan `dotnet build` (mencakup `BE-FIN-074` dan `BE-FIN-075` sekaligus) dan mengonfirmasi hasilnya; (2) pemilik modul mengisi `FinanceManagement:TransactionProof:MaxFileSizeBytes` (`FIN-OQ-082`) pada konfigurasi lingkungan sebelum unggah dapat dipakai produksi; (3) `BE-FIN-076` melanjutkan dengan CRUD ambang (`MstDirectPaymentThreshold`), lalu `BE-FIN-077`/`078` menyambungkan `ProofId` ke pembayaran langsung |
