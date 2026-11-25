# Laporan Perubahan Backend — `BE-HRD-388`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-388` |
| Judul | Tautan create Pegawai, Dokter, dan Eksternal ke profil yang sama secara atomik |
| Slice | `S-FIN-M04` / Profil (alias `FIN-HRD-04-BE-002`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.2, kartu `BE-HRD-388` |
| Trace | `HRD-DEC-051`; instruksi Menu 04 `FIN-HRD-04-BE-002`; `UAT-04-03` s.d. `06`, `38` |
| Contract version | Kontrak Data Induk existing; tidak ada endpoint baru |
| Dependency | `BE-HRD-387` ✅ (laporan [BE-HRD-387.md](BE-HRD-387.md)) |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `MasterData/Workforce/Controllers/*.cs`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpetakan ke source; build dan QBE lulus; uji API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `MasterData/Workforce` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/006 (lewat resolver `BE-HRD-387`) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Ketiga create (`CreateEmployee`, `CreateDoctorForAdmin`, `CreateExternalUser`) sudah memanggil resolver dan berjalan di dalam transaksi. Dua celah tersisa:

1. **Akun login ganda.** Satu profil hanya boleh punya satu akun login (index unik `AspNetUsers.WorkforceProfileId`). Bila dr. A sudah punya akun sebagai Pegawai lalu didaftarkan sebagai Dokter dengan centang "buat akun login", penyimpanan menabrak index unik dan petugas hanya melihat galat 500.
2. **Resolver dapat dipanggil di luar transaksi.** Bila kelak ada pemanggil yang lupa membuka transaksi, profil baru akan tersimpan walau pembuatan peran gagal — profil yatim.

## 2. Proses bisnis

1. Create membuka transaksi.
2. Resolver menautkan atau membuat profil (`BE-HRD-387`); resolver kini **menolak** dipanggil tanpa transaksi.
3. Peran (Pegawai/Dokter/Eksternal) dan penempatan awal disimpan pada transaksi yang sama.
4. Bila diminta membuat akun login: bila profil sudah punya akun lewat peran lain → `400` dengan pesan "gunakan akun yang ada"; transaksi dibatalkan, tidak ada profil, peran, atau penempatan yang tertinggal.
5. Commit.

**Jalur tidak normal.** Galat apa pun sebelum commit — termasuk `DUPLICATE_ROLE` (409) dan galat akun — membatalkan transaksi; `await using` membatalkan transaksi yang belum di-commit walau sebuah cabang `return` lupa memanggil `RollbackAsync`. Nomor profil yang sudah terbit dari alokator tetap hangus (perilaku alokator, `INV-PLT-002`), bukan profil yatim.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Tiga controller master (alur create dari validasi sampai commit), `CreateLoginAccountFor{Employee,Doctor,ExternalUser}Async`, konfigurasi `ApplicationUser` (index unik `WorkforceProfileId`, `EmployeeId`, `DoctorId`, `ExternalUserId`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `EmployeeController.cs`, `DoctorController.cs`, `ExternalUserController.cs` | Pembuatan akun login menolak profil yang sudah punya akun (`400`, rollback) |
| `WorkforceIdentityResolverService.cs` | Wajib transaksi pemanggil (`IDENTITY_RESOLUTION_REQUIRES_TRANSACTION`) — dibagi dengan `BE-HRD-387` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada perubahan route/DTO. Galat 500 pada akun ganda menjadi `400` berpesan jelas |
| Database | Tanpa migration |
| Keamanan/Auth | Tidak berubah |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Master Data / Employee

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/human-resource/master-data/employees` | Membuat Pegawai, menautkan profil, opsional akun login | `Employee : Create` |

#### Corporate / Human Resource / Master Data / Doctor

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/human-resource/master-data/doctors` (`/admin`) | Membuat Dokter dan menautkan profil | `Doctor : Create` |

#### Corporate / Human Resource / Master Data / External User

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/human-resource/master-data/external-users` | Membuat Pengguna Eksternal dan menautkan profil | `ExternalUser : Create` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` (lihat `BE-HRD-387`) | Berhasil, 0 error, 608 warning (himpunan warning identik dengan sebelum perubahan) | `PASS` | Keluaran build |
| QBE Strict berkas Menu 04 | 20 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: dr. A Pegawai berakun, didaftarkan Dokter + buat akun | Ditolak `400` sebelum `UserManager.CreateAsync`; transaksi rollback | `PASS` (penelusuran) | `CreateLoginAccountForDoctorAsync` |
| Penelusuran: Dokter tertaut tanpa akun baru | Profil sama, jumlah profil +0 | `PASS` (penelusuran) | resolver cabang `IsExistingLinked` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE`.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Create Pegawai/Dokter/Eksternal memanggil resolver | Terpenuhi | `ResolveOrCreateProfileAsync` di ketiga create |
| (2) Kegagalan di tengah tidak meninggalkan profil yatim | Terpenuhi | Satu transaksi per create; resolver menolak tanpa transaksi; akun ganda ditolak sebelum menabrak index |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah yang diketahui | Rehire sesudah soft delete Pegawai (lihat `BE-HRD-387`) |
| Risiko tersisa | Bukti runtime belum ada |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat `BE-HRD-402` |
| Langkah berikutnya | `BE-HRD-399` (profesi peran jamak) |
