# Laporan Perubahan Backend — `BE-CRM-015`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-CRM-015` |
| Judul | Penutupan dan pensiun endpoint |
| Slice | `MVP-1`, `EPIC CRM-06` dan `EPIC CRM-05` |
| Roadmap | `docs/module-blueprints/crm-membership/roadmap/backend-roadmap.md` § `BE-CRM-015` |
| Trace | `FR-CRM-041`, `FR-CRM-050` s.d. `FR-CRM-052`; `CRM-DEC-013`, `CRM-DEC-018`, `CRM-DEC-022`; `VAL-18`, `VAL-19`; `CRM-CAP-004`, `CRM-CONF-007`, `CRM-CONF-008` |
| Contract version | API `v2` (dampak kompatibilitas breaking), `approved` |
| Dependency | `BE-CRM-011` 🟡, `BE-CRM-013` ✅. **Rilis bersama `FE-CRM-001`** |
| Klasifikasi | `MEDIUM`: pensiun empat aksi, hapus kode mati, abaikan field proyeksi di formulir pasien, perbaiki default insert `IsMember` |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/PatientManagement/MasterData/`, `Repositories/Configurations/HealthServices/MstPatientConfiguration.cs`, `Tests/...`, laporan ini |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `83ef27bd` + perubahan yang belum di-commit |
| Tanggal | 2026-10-13 |
| Status | Selesai. `AT-19` lulus lewat HTTP nyata 2026-11-11 dan `UAT-11` lulus 2026-10-14 (`FE-CRM-007`) — lihat §8. Syarat rilis bersama `FE-CRM-001` terpenuhi (`FE-CRM-001` ✅) |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `PatientManagement` / `MasterData` (`Mst`) |
| Keberlakuan | `TOUCHED LEGACY`: `PatientMembershipController`, `PatientController`, `MstPatient`, `MstPatientConfiguration` |
| QBE yang berlaku | `QBE-PERM-001` (deklarasi `[AccessAction("Create")]` dipindah ke aksi pengganti agar izin tetap muncul di Role Access), `QBE-API-001` (410 dengan `ApiResponse`), `QBE-DEL-001` (tidak ada lagi hapus/ubah yang menghilangkan riwayat) |

## 1. Masalah yang diperbaiki

| Masalah | Akibat |
| --- | --- |
| Akun kiosk bisa membuat membership aktif lewat `POST /` dan `POST /kiosk` (`CRM-CONF-007`) | Celah pemberian diskon |
| `PUT`, `PATCH status`, `DELETE` mengubah atau menghapus membership tanpa jejak (`CRM-CONF-008`) | Riwayat dan bukti bayar bisa hilang |
| Formulir pasien bisa mengisi `IsMember`, tingkat, dan penunjuk membership | Status member bisa diketik manual |
| **Akar masalah baru ditemukan:** `MstPatient.IsMember` dikonfigurasi `HasDefaultValue(true)`. EF tidak mengirim `false` saat insert, karena sama dengan nilai bawaan CLR, sehingga default database `true` selalu menang | Setiap pasien baru dari jalur mana pun (master pasien, registrasi IGD, kiosk) tersimpan sebagai member. Inilah sebab 14 pasien member palsu di devYoga |

## 2. Proses bisnis

1. Membership hanya bisa dibuat petugas berizin lewat `POST /admin` (`CRM-DEC-018`). Rute `POST /` dan `POST /kiosk` tidak ada lagi (404).
2. Klien lama yang memanggil `PUT`, `PATCH status`, atau `DELETE` menerima **410** dengan petunjuk pengganti:
   - `PUT` → "Fitur ubah membership sudah tidak tersedia. Gunakan ubah catatan, upgrade, atau pembatalan."
   - `PATCH status` → "Ubah status membership sudah tidak tersedia. Gunakan pembatalan atau verifikasi."
   - `DELETE` → "Hapus membership sudah tidak tersedia. Gunakan pembatalan dengan alasan."
   - Izin lama (`Update`, `Delete`) tetap diperiksa lebih dulu, jadi pengguna tanpa izin tetap mendapat 403.
3. Saat membuat atau mengubah pasien, `IsMember`, `DefaultMembershipTierId`, dan `ActivePatientMembershipId` dari request diabaikan. Pasien baru selalu tersimpan bukan member, sampai resolver menyatakan sebaliknya.

## 3. Perubahan yang dikerjakan

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientMembershipController.cs` | Hapus `CreatePatientMembership` (rute `/` dan `/kiosk`). `UpdatePatientMembership`, `UpdatePatientMembershipStatus`, dan `DeletePatientMembership` kini menjawab 410. Hapus kode mati: `ValidateRequestAsync`, `ClearOtherPrimaryMembershipsAsync`, `SyncPatientMembershipReferenceAsync`, `RecalculatePatientMembershipReferenceAsync`, `BuildCreateResponseAsync`, tiga fungsi normalisasi. `[AccessAction("Create")]` dipindah ke `RegisterPatientMembershipForAdmin` |
| `Areas/HealthServices/PatientManagement/MasterData/DTOs/PatientMembershipDtos.cs` | Hapus `CreatePatientMembershipRequest`, `UpdatePatientMembershipRequest`, `UpdatePatientMembershipStatusRequest`, `DeletePatientMembershipRequest`, `PatientMembershipCreateResponse` (tidak dipakai di mana pun) |
| `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientController.cs` | Create: tiga field proyeksi diisi `false`/`null`. Update: tiga field proyeksi tidak lagi diubah. Field request dibiarkan ada agar klien lama tidak galat |
| `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatient.cs` | Bawaan CLR `IsMember = false` |
| `Repositories/Configurations/HealthServices/MstPatientConfiguration.cs` | `.HasSentinel(true)` pada `IsMember`, sehingga `false` dikirim eksplisit saat insert. Skema tidak berubah (`dotnet ef migrations has-pending-model-changes`: tidak ada perubahan) |
| `Tests/.../MembershipRetireLegacyTests.cs` | Baru, 4 uji |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Breaking** sesuai API `v2`: `POST /` dan `/kiosk` hilang (404). `PUT`/`PATCH status`/`DELETE` → 410. Konsumen yang terdampak hanya halaman patient-memberships frontend (`FE-CRM-001`). Kiosk hanya memakai `GET /options`, jadi tidak terdampak |
| Database | Tanpa migration. Default kolom `IsMember` di database tetap `true` (utang teknis, lihat §7) |
| Keamanan/Auth | Menutup pembuatan membership oleh akun kiosk. Status member tidak bisa lagi diketik lewat formulir pasien |

## 4. Dokumentasi endpoint

#### Health Services / Patient Management / Master Data / Patient Membership

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/`, `/kiosk` | **Dihapus** (404) | — |
| `PUT` | `/{id}`, `/admin/{id}` | Dipensiunkan, 410 | `PatientMembership : Update` |
| `PATCH` | `/{id}/status`, `/admin/{id}/status` | Dipensiunkan, 410 | `PatientMembership : Update` |
| `DELETE` | `/{id}`, `/admin/{id}` | Dipensiunkan, 410 | `PatientMembership : Delete` |

## 5. Verifikasi

| Skenario | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Rute `POST` membership (`AT-22`) | Tidak ada `POST /` dan `/kiosk`. `POST /admin` ada. `GET /kiosk/options` tetap ada | `PASS` | Uji tabel rute MVC |
| `PUT`/`PATCH status`/`DELETE` (`AT-25`) | 410 dengan pesan pengganti | `PASS` | Uji controller |
| Insert pasien tanpa mengisi `IsMember` | Tersimpan `false` (sebelumnya `true`) | `PASS` | Uji SQLite (skema dari model EF yang sama) |
| Aksi `CreatePatientMembership` | Tidak ada lagi | `PASS` | Uji refleksi |
| `PermissionRegistryValidator` atas aksi baru | Tetap tanpa temuan setelah `[AccessAction("Create")]` dipindah | `PASS` | `MembershipPermissionRegistryTests` |
| `dotnet ef migrations has-pending-model-changes` | Tidak ada perubahan model | `PASS` | Keluaran EF |
| `dotnet test` seluruh project | 170 lulus | `PASS` | Keluaran test |
| QBE checker | 0 VIOLATION | `PASS` | Keluaran checker |
| `AT-19` (403 untuk admisi tanpa izin) / `UAT-11` lewat UI | 403 / formulir tanpa status member | `PASS` (susulan — §8) | Semula `NOT RUN` 2026-10-13: butuh pipeline otorisasi berjalan dan frontend (`FE-CRM-007`) |

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-22` | Terpenuhi | Uji rute |
| `AT-23` | Terpenuhi (di `BE-CRM-021`) | Uji kiosk |
| `AT-25` | Terpenuhi | Uji 410 |
| `AT-19` | Terpenuhi 2026-11-11 | §8 — akun asli tanpa izin mendapat 403 dari backend lokal terhadap devYoga |
| `UAT-11` | Terpenuhi 2026-10-14 | Laporan `FE-CRM-007` §6: master pasien dan pasien baru IGD terhadap backend lokal, `IsMember=false` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Jangan rilis tanpa `FE-CRM-001`.** Halaman patient-memberships frontend saat ini memanggil `POST /` (kini 404) dan `GET /` (policy `KioskRead`, sehingga petugas non-administrator mendapat 403) |
| Masalah yang diketahui | Default kolom `MstPatient.IsMember` di database masih `true`. Insert lewat EF sudah benar karena sentinel, tetapi insert SQL mentah tetap mendapat `true`. Perbaikan tuntasnya adalah migration ubah default ke `false`, yang perlu persetujuan karena di luar rencana task ini |
| Risiko tersisa | `PatientDtos` masih punya field `IsMember`/`DefaultMembershipTierId`/`ActivePatientMembershipId` di request. Sekarang diabaikan, dan dibersihkan dari frontend di `FE-CRM-007` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Belum di-commit. Berkas uji baru `MembershipRetireLegacyTests.cs` diabaikan git, perlu `git add -f` |
| Langkah berikutnya | `FE-CRM-001` di repo frontend (lapisan data ke rute `/admin`), dirilis bersama task ini |

## 8. Susulan 2026-11-11 — bukti `AT-19` lewat HTTP nyata

`AT-19` dibuktikan terhadap backend lokal (`http://localhost:5107`, Development, build hari yang sama)
yang terhubung ke devYoga, memakai pipeline otorisasi yang sebenarnya. Tidak ada data yang ditulis.

| Field | Nilai |
| --- | --- |
| Task mode | `BACKEND`, khusus verifikasi. Source aplikasi tidak berubah |
| Akun tanpa izin | `gilang.mahendra@rsmmc.local` — jabatan Analis Laboratorium, tanpa role Identity. Kueri baca-saja sebelum uji: nol kebijakan `SysAccessPolicy` pada controller `PatientMembership` untuk pasangan departemen–jabatan akun ini. Pemegang `Create` hanya Customer Service dan Dokter Umum. Sandi diberikan pemilik lewat chat, dipakai lewat variabel lingkungan, tidak disimpan |
| Pengaman tulis | Body memakai `patientId` dan `membershipTierId` acak. Bila otorisasi bocor, service menjawab 404 sebelum menulis |
| Konfigurasi start | `Seeders__ContinueOnFailure=true` (migration SDM tertunda di devYoga), `PatientMembership__Lifecycle__Enabled=false` agar proses harian tidak menulis |
| Migration / database | Nol migration. Nol tulis ke devYoga — terverifikasi kueri baca-saja sesudah uji |

### 8.1 Contoh

Gilang, analis laboratorium, membuka alamat pendaftaran membership dan menekan Simpan. Sistem
menolak dengan "Anda tidak memiliki akses ke menu atau fitur ini." Tidak ada membership yang
terbentuk. Petugas Customer Service dengan izin `Create` tetap bisa mendaftarkan pasien.

### 8.2 Hasil

| Pemeriksaan | Harapan | Hasil | Status |
| --- | --- | --- | --- |
| `POST /admin` tanpa sesi | 401 | 401 | `PASS` |
| `POST /admin` superadmin, pasien acak (kontrol: rute dan service hidup) | 404, tanpa tulis | 404 "Pasien tidak ditemukan atau tidak aktif. Daftarkan pasien lebih dulu." | `PASS` |
| Login Gilang | 200 | 200 | `PASS` |
| **`AT-19`**: `POST /admin` oleh Gilang | 403 | 403 "Anda tidak memiliki akses ke menu atau fitur ini." | `PASS` |
| `GET /admin` oleh Gilang (tanpa `Read`) | 403 | 403 | `PASS` |
| Kontrol sesi: Gilang `GET lab-orders/filters/metadata` (izin Lab yang ia pegang) | 200 | 200 | `PASS` |
| Membership baru di devYoga dalam satu jam terakhir / berkatalog uji | 0 | 0 / 0 | `PASS` |
| Baris `fail:` di log backend selama uji | 0 | 0 | `PASS` |

Backend dimatikan sesudah uji; port 5107 bebas.

### 8.3 Catatan

`AT-19` di matriks menyebut "akun admisi". devYoga belum punya akun admisi yang tidak memegang
`PatientMembership : Create`, karena jabatan Pendaftaran / Customer Service justru diberi izin itu
untuk UAT 2026-10-14. Akun yang dipakai mewakili kondisi yang diuji: jabatan tanpa kebijakan
`Create`. Penjaga izinnya sama untuk jabatan mana pun (`[AccessPermission("PatientMembership", "Create")]`).
