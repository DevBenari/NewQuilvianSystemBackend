# Laporan Perubahan Backend — `BE-OPT-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-OPT-010` |
| Judul | Kunjungan Optik |
| Slice | `MVP-1` — `EPIC OPT-01` |
| Roadmap | `roadmap/backend-roadmap.md` (`OPT-RM-BE-001` r2) — Rincian `BE-OPT-010` |
| Trace | `FR-OPT-010`; `OPT-DEC-006`, `043`; `INT-OPT-01`; `UAT-01` |
| Contract version | `OPT-API-v1` grup Opt Visit, `OPT-INT-v1` `INT-OPT-01`, `OPT-PERM-v1` — `approved` sisi Optik; Registrasi `OPT-APR-008` ✅ (Muhammad Hamzah) |
| Dependency | `BE-OPT-004` ✅; data unit Optik `BE-OPT-001` ✅ (seeder `SU-OPT-001`) |
| Klasifikasi | `MEDIUM` — satu controller (3 endpoint), satu service, DTO; memakai ulang service Registrasi |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/OpticalManagement/{Controllers,Services,DTOs}/`, `Program.cs`, `Tests/QuilvianSystemBackend.OpticalTests/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `2a5094ca` (branch `yoga`) |
| Tanggal | 2026-11-03 |
| Status | Selesai dan tervalidasi. Belum di-commit |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `OpticalManagement` |
| Registry | `Opt` / `ACTIVE` |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` hanya `Program.cs` (DI) |
| QBE yang berlaku | `QBE-SVC-001` (controller tanpa `DbContext`), `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-AUD-001` |
| Wewenang terpisah | Source ✅ · migration tidak diperlukan · eksekusi database ✘ · deployment ✘ |

---

## 1. Masalah yang diperbaiki

Petugas Optik belum bisa mendaftarkan pasien ke unit Optik maupun melihat daftar kunjungannya. Semua dokumen
Optik (pemeriksaan, resep, pesanan) menggantung pada kunjungan, sehingga task berikutnya belum bisa dikerjakan.

---

## 2. Proses bisnis

1. **Petugas mendaftarkan pasien.** **Contoh (`UAT-01`):** pasien `RM-000123` datang tanpa rujukan dan membayar tunai. Petugas menekan Simpan, lalu Registrasi membuat kunjungan rawat jalan di unit `SU-OPT-001` "Unit Optik" tanpa dokter dan tanpa antrean. Kunjungan itu langsung muncul di daftar kunjungan Optik.
2. **Simpan tertekan dua kali.** Layar mengirim kunci yang sama pada header `Idempotency-Key`. Permintaan kedua tidak membuat kunjungan baru; jawabannya kunjungan yang sama dengan `IsReplay = true`.
3. **Pasien rujukan dokter mata.** Dokter perujuk dan instansi perujuk diisi dan diteruskan ke Registrasi. Tautan ke kunjungan Poli Mata asal **belum tersedia**: bila diisi, permintaan ditolak "Tautan ke kunjungan Poli Mata asal belum tersedia. Kosongkan isian itu; dokter perujuk tetap dapat diisi." Fitur ini datang di `BE-OPT-003`.
4. **Melihat kunjungan.** Daftar bisa disaring per tanggal WIB, kunjungan yang masih terbuka, kunjungan yang masih punya pesanan Optik belum selesai, dan kata kunci (nomor kunjungan, nomor RM, nama). Ringkasan satu kunjungan menampilkan pemeriksaan, resep, pesanan, dan klaim garansinya. Selama task berikutnya belum selesai, daftar itu kosong.

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Header `Idempotency-Key` kosong | `400` "Header Idempotency-Key wajib diisi agar Simpan dua kali tidak membuat kunjungan ganda." |
| Unit Optik belum ada atau tidak aktif | `409` `OPT-UNIT-MISSING` "Unit layanan Optik belum terdaftar atau tidak aktif. Hubungi bagian Master Data." |
| Petugas tidak punya izin Registrasi `PatientEncounter : Create` | `403` `OPT-REG-FORBIDDEN`, memakai kalimat dari Registrasi |
| Registrasi menolak isian, misalnya pasien tidak ada atau kartu asuransi tidak cocok | `400` `OPT-REG-REJECTED`, memakai kalimat dari Registrasi |
| Registrasi menolak karena bentrok, misalnya kunci sama dengan isian berbeda | `409` `OPT-REG-CONFLICT` |
| Database Registrasi tidak terjangkau | `503` "Pendaftaran gagal karena layanan registrasi sedang tidak dapat diakses. Silakan coba lagi." |
| Ringkasan untuk kunjungan unit lain | `404` "Kunjungan Optik tidak ditemukan." |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`EncounterIntakeService.cs` (jalur unit penunjang, idempotensi VAL-45, seam kewenangan, pengecualian), `EncounterIntakeRequest`, `RegPatientEncounter`, `OutpatientQueueClassificationService`, `PatientMembershipStatusResolver`, serta `OptHttp`/`OptOperationResult` dari `BE-OPT-005`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/OpticalManagement/Controllers/OptVisitController.cs` | **Baru.** Tiga endpoint (bagian 4). Tanpa `<summary>` XML pada action |
| `Areas/HealthServices/OpticalManagement/Services/OptVisitService.cs` | **Baru.** Pendaftaran lewat `EncounterIntakeService` (kunci berawalan `OPT-VISIT:`), daftar berhalaman dengan penyaring, ringkasan, dan `FindOpenOpticalEncounterAsync` untuk task berikutnya |
| `Areas/HealthServices/OpticalManagement/DTOs/OptVisitDtos.cs` | **Baru.** Permintaan pendaftaran, penyaring daftar, baris daftar, ringkasan |
| `Areas/HealthServices/OpticalManagement/Services/OptOperationResult.cs` | **Diperbarui.** Kode `OPT-UNIT-MISSING`, `OPT-REG-REJECTED`/`FORBIDDEN`/`CONFLICT`/`UNAVAILABLE` |
| `Program.cs` | **Diperbarui.** `AddScoped<OptVisitService>()` |
| `Tests/QuilvianSystemBackend.OpticalTests/Be010VisitTests.cs` | **Baru** (diabaikan Git). 9 uji |
| `Tests/QuilvianSystemBackend.OpticalTests/OpticalPermissionRegistryTests.cs` | **Diperbarui.** Hitungan endpoint Optik 5 → 8 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tiga endpoint `OPT-API-v1` grup Opt Visit tersedia. Ada tambahan di luar kontrak, tanpa mengubah kontrak: penyaring `OpenOnly`/`HasOpenOrder`/`Search` dan ruas `IsReplay` |
| Database | Tanpa migration. Optik **tidak** menulis `RegPatientEncounter`; kunjungannya ditulis `EncounterIntakeService` |
| Keamanan/Auth | `[AccessPermission("OptVisit", "Read" / "Create")]`, lolos `PermissionRegistryValidator`. Pendaftaran juga diperiksa Registrasi terhadap `PatientEncounter : Create`. Kalimat log hanya memuat nomor kunjungan |

---

## 4. Dokumentasi endpoint

#### Health Services / Optical Management / Opt Visit

Base URL: `api/v1/health-services/optical-management/visits`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar kunjungan Optik: `Date`, `OpenOnly`, `HasOpenOrder`, `Search`, `PageNumber`, `PageSize` | `OptVisit : Read` |
| `GET` | `/{encounterId}` | Ringkasan satu kunjungan beserta dokumen Optiknya | `OptVisit : Read` |
| `POST` | `/` | Daftarkan kunjungan; header `Idempotency-Key` (GUID) wajib | `OptVisit : Create` + Registrasi `PatientEncounter : Create` |

Kode status: `200` berhasil (termasuk pengulangan dengan kunci yang sama); `400` isian tidak sah atau ditolak Registrasi; `403` tanpa izin Registrasi; `404` kunjungan bukan Optik; `409` unit Optik belum ada atau bentrok di Registrasi; `503` Registrasi tidak terjangkau.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet test Tests/QuilvianSystemBackend.OpticalTests -p:RunAnalyzers=False` | 42 lulus, 0 gagal (9 baru) | `PASS` | Keluaran `dotnet test` |
| `UAT-01`: kunjungan tercatat di unit `Optical`, jenis `Outpatient`, kunci berawalan `OPT-VISIT:`, lalu muncul di daftar | Lulus | `PASS` | Uji dengan `EncounterIntakeService` asli; hanya seam kewenangan yang diganti |
| Simpan dua kali dengan kunci sama | Satu kunjungan; jawaban kedua `IsReplay = true` | `PASS` | Uji |
| Tanpa kunci; rujukan bertaut | `400`; tidak ada kunjungan dibuat | `PASS` | Uji |
| Unit Optik belum ada | `409 OPT-UNIT-MISSING` | `PASS` | Uji |
| Tanpa izin Registrasi | `403 OPT-REG-FORBIDDEN`; tidak ada kunjungan dibuat | `PASS` | Uji |
| Pasien tidak ada | `400 OPT-REG-REJECTED` dengan kalimat Registrasi | `PASS` | Uji |
| Daftar hanya unit Optik; penyaring pesanan terbuka dan kata kunci; ringkasan berlabel "Dalam Pengerjaan"; kunjungan Lab → `404` | Lulus | `PASS` | Uji |
| Controller meneruskan header dan memetakan `200/400/404` | Lulus | `PASS` | Uji controller |
| `PermissionRegistryValidator` atas 8 endpoint Optik | Nol temuan; 8 endpoint terlihat | `PASS` | Uji registri |
| Optik tidak menulis `RegPatientEncounter` (kriteria 2) | Tidak ditemukan `Add`/`Update`/`Remove` atas `RegPatientEncounter` di `OpticalManagement/` | `PASS` | `grep` |
| QBE checker atas 5 berkas + kontrol multi-baris (controller ber-`DbContext`, model `Zzz*` ber-`[Table]`) | Kontrol terkena 5 temuan; 5 berkas bersih | `PASS` | Keluaran checker; probe dihapus |

**Tidak dijalankan:** panggilan HTTP nyata ke backend yang menyala. Ringkasan kunjungan membaca tabel Optik di devYoga, sementara migration belum diterapkan dan wewenang database tidak diberikan.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Satu kunjungan per kunci idempoten | Terpenuhi | Uji Simpan dua kali dan uji controller |
| (2) Optik tidak menulis `RegPatientEncounter` langsung | Terpenuhi | `grep` dan tinjauan kode |
| (3) `UAT-01` | Terpenuhi di harness | Uji `UAT_01_…` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Akun petugas Optik juga wajib punya izin Registrasi `PatientEncounter : Create` untuk jabatannya di `SysAccessPolicies`. Tanpa itu pendaftaran ditolak `403` walaupun `OptVisit : Create` sudah diberikan |
| Masalah yang diketahui | Cara bayar penjamin perusahaan (`CompanyGuarantor`) saat ini ditolak Registrasi untuk kunjungan penunjang karena hanya MCU yang diizinkan. Pasien perusahaan di Optik menunggu Registrasi memperluas jalurnya; ini perlu diajukan ke pemilik Registrasi (Muhammad Hamzah) |
| Risiko tersisa | Tautan kunjungan Poli Mata asal (`ReferralSourceEncounterId`, `OPT-DEC-037`) baru bisa dipakai sesudah `BE-OPT-003`. Uji harness memakai SQLite; perilaku kunci unik idempoten di PostgreSQL belum dicoba di server |
| Perubahan sampingan | `NONE`. Probe QBE sementara dibuat dan dihapus |
| Interupsi | Konteks sesi dipadatkan di tengah pengerjaan uji; dilanjutkan dari berkas yang sudah tertulis tanpa penyuntingan ganda |
| Status Git | `M Program.cs`; `?? Areas/HealthServices/OpticalManagement/` bertambah `OptVisitController.cs`, `OptVisitService.cs`, `OptVisitDtos.cs` |
| Langkah berikutnya | `BE-OPT-011` (pemeriksaan refraksi). Sebelum mulai perlu keputusan `OPT-OQ-017` (STR wajib selain SIP?) dan konfirmasi isian visus/autoref. Sejajar dengan itu `BE-OPT-003` (rujukan di Registrasi) dan `BE-OPT-030` (perluasan Billing) |
