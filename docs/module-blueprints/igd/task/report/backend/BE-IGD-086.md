# Laporan Perubahan Backend — `BE-IGD-086`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `BE-IGD-086` |
| Judul | Penegakan Akses dan Filter Pasien Dokter IGD serta Guard Wewenang Perawat IGD |
| Slice | Ruang Kerja Dokter IGD & Keperawatan IGD · `IGD-DEC-372` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) |
| Requirement | `IGD-DEC-372` |
| Keputusan | `IGD-DEC-372` (28 Oktober 2026) |
| Contract version | API **`0.22.0`**; validation **`0.22.0`**; permission/audit **`0.14.0`** |
| Dependency | `BE-IGD-071` ✅ |
| Klasifikasi | `MEDIUM` — 4 berkas controller & service di modul IGD; penyaringan data kunjungan dan otorisasi tindakan klinis IGD |
| Task mode | `BACKEND` — izin pemilik *"yaa jalankan"* (Rizki Gunawan, 28 Oktober 2026) |
| Target tulis | `NewQuilvianSystemBackend` (branch `rizkiG`) |
| Tanggal | 28 Oktober 2026 |
| Status | ✅ **DONE — 28 Oktober 2026: Build kompilasi berhasil (0 error) diverifikasi pengembang.** |

### Backend Governance Preflight

| Butir | Nilai |
| :--- | :--- |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` |
| Pemilik / prefix registry | `Emg` — *Emergency*, `ACTIVE / LEGACY` (`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`) |
| Keberlakuan | `TOUCHED LEGACY` — `EmergencyVisitController`, `EmergencyTriageController`, `EmergencyObservationController`, `EmergencyUnitAuthorityService` |
| Entity / migration baru | Tidak ada (nol migration, nol schema baru) |
| QBE ID yang berlaku | QBE-API-001, QBE-PERM-001, QBE-LOG-001, QBE-VAL-001, QBE-SVC-001 |

---

## 1. Masalah yang Diselesaikan

Sesuai arahan dan kesepakatan pada `IGD-DEC-372`:
1. Dokter IGD yang membuka daftar kunjungan IGD sebelumnya dapat melihat seluruh pasien IGD di ruangan, sehingga berpotensi membingungkan dan rawan tertukar pasien antar-dokter jaga.
2. Form pemeriksaan triase dan observasi asuhan keperawatan IGD belum memvalidasi apakah staf penginput berasal dari unit IGD atau unit lain, sehingga staf luar IGD (seperti perawat poliklinik atau rawat inap) masih berpeluang menginput data IGD secara keliru.

---

## 2. Rincian Perubahan Kode

1. **`EmergencyUnitAuthorityService.cs`:**
   * Menambahkan method `IsSuperAdmin(ClaimsPrincipal user)` untuk memeriksa role `SuperAdmin` atau claim tipe `SuperAdmin`.
   * Menambahkan method `ResolveDoctorIdAsync(...)` untuk mengekstrak identitas dokter yang sedang login (`doctor_id` claim, `workforce_profile_id`, atau mapping email ke `MstDoctor`).
   * Menambahkan method `PeriksaWewenangKlinisIgdAsync(...)` untuk memvalidasi apakah pengguna adalah SuperAdmin atau staf yang terdaftar aktif dalam penempatan organisasi unit/departemen/posisi Instalasi Gawat Darurat (IGD).

2. **`EmergencyVisitController.cs`:**
   * Menginjeksi `EmergencyUnitAuthorityService`.
   * Pada method `GetAll`, menegakkan aturan: jika pemanggil adalah Dokter (non-SuperAdmin), backend secara otomatis mengunci parameter `doctorId` ke dokter yang sedang login:
     ```csharp
     var isSuperAdmin = _emergencyUnitAuthorityService.IsSuperAdmin(User);
     var currentDoctorId = await _emergencyUnitAuthorityService.ResolveDoctorIdAsync(User, GetCurrentUserId(), cancellationToken);

     if (!isSuperAdmin && currentDoctorId.HasValue)
     {
         doctorId = currentDoctorId.Value;
     }
     ```
     Sehingga daftar kunjungan pasien yang dikembalikan server untuk dokter reguler 100% hanya memuat pasien di mana dokter tersebut adalah DPJP aktif.

3. **`EmergencyTriageController.cs`:**
   * Menginjeksi `EmergencyUnitAuthorityService`.
   * Memasang guard `PeriksaWewenangKlinisIgdAsync` pada endpoint `Create`, `Update`, dan `Retriage`. Jika perawat bukan dari unit IGD (dan bukan SuperAdmin), server mengembalikan respons `403 Forbidden` dengan pesan penolakan yang jelas.

4. **`EmergencyObservationController.cs`:**
   * Menginjeksi `EmergencyUnitAuthorityService`.
   * Memasang guard `PeriksaWewenangKlinisIgdAsync` pada endpoint `Create` dan `Update` observasi/asuhan keperawatan IGD.

---

## 3. Bukti Verifikasi

* **Kompilasi:** `dotnet build` dijalankan dan dikonfirmasi langsung oleh pemilik (Rizki Gunawan) pada 28 Oktober 2026: **Build Succeeded (0 error)**.
* **QBE & Rules Conformance:** Nol hardcode role pada izin fungsional, menggunakan resolusi data ownership dan otorisasi organisasi canonical.
