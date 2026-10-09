# Laporan Perubahan Backend — `RJ-DOC-REV-BE-009`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-BE-009` |
| Judul | Daftar, summary, metadata bercakupan dan butir hak akses |
| Roadmap | `rawat-jalan/roadmap/doctor-consultation-roadmap.md` bagian `11.1` |
| Trace | `RJ-DOC-DEC-012`, `013`, `014`, `015` (bagian `canCancel`); desain `02-backend-architecture.md` DP.3.3, DP.5, DP.7; `contracts/permission-audit-matrix.md` *Amendment DP* |
| Kontrak | `RJ-DOC-ENCLIST-001@1.0.0` (`approved`) — `GET /`, `GET /summary`, `GET /filters/metadata` |
| Dependency | `RJ-DOC-REV-BE-008` ✅ |
| Wewenang | `RJ-DOC-DEC-025`; akun uji sementara disetujui pemilik pada sesi yang sama (2 Okt 2026) |
| Task mode | `BACKEND` |
| Baseline | BE `245f0464` (`sukmagp`) + perubahan `BE-008` yang belum di-commit |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 2 Oktober 2026 |
| Status | ✅ SELESAI — AT-DP-01..06, 20, 21 terbukti runtime |

## 1. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` (prefix `Reg`, `ACTIVE / LEGACY`) |
| Keberlakuan | `NEW CODE` untuk enam berkas baru; `TOUCHED LEGACY` untuk `Program.cs` (registrasi DI) |
| Registry | Tidak ada entity baru; `QBE-MOD-002`/`003` tidak berlaku |
| QBE yang berlaku | `QBE-MOD-001`, `QBE-SVC-001` (controller tanpa `ApplicationDbContext`), Conformance Strict |
| Arketipe endpoint | Transaksi — **worklist** baca bercakupan. Tanpa `GET /options`, `PATCH /{id}/status`, `DELETE` |
| Hak akses | `[AccessController(ControllerName = "OutpatientEncounter")]`; tiga `GET` ber-`[AccessAction("Read", …)]` + `[AccessPermission("OutpatientEncounter", "Read")]`; penanda `ReadAll` lewat `[assembly: AccessExplicitPermission]`. **Tanpa** `IsInRole`/nama role |
| Migration / DB | Tidak ada |

## 2. Perubahan

| Berkas | Status | Isi |
| --- | --- | --- |
| `Areas/HealthServices/RegistrationManagement/Controllers/OutpatientEncounterController.cs` | Baru | Tiga `GET`; memetakan `OutpatientEncounterScopeException` → `403`, `OutpatientEncounterValidationException` → `400` |
| `Areas/HealthServices/RegistrationManagement/Services/OutpatientEncounterListService.cs` | Baru | Query bercakupan, filter mode `today`/`active`/`range` + `hangingOnly`, daftar berhalaman, ringkasan lima kelompok, metadata, `GetCancelBlockedReason` (dipakai ulang `BE-010`) |
| `Areas/HealthServices/RegistrationManagement/Services/ClinicalActorScopeService.cs` | Baru | `ClinicalActorScope { CanReadAll, DoctorId, ClinicIds }`. Pengenal dokter/perawat disalin dari kedua controller antrean, termasuk mapping poli per perawat lalu fallback poli cluster; cabang SuperAdmin tidak dibawa |
| `Areas/HealthServices/RegistrationManagement/OutpatientEncounterExplicitPermissions.cs` | Baru | Penanda `OutpatientEncounter : ReadAll` |
| `Areas/HealthServices/RegistrationManagement/DTOS/OutpatientEncounterDtos.cs` | Baru | Query, item, summary, metadata |
| `Areas/HealthServices/RegistrationManagement/Services/OutpatientEncounterRules.cs` | Diperbarui (dari `BE-008`) | Tambah expression `IsBlockingState`; `WhereBlocksRegistration` memakainya. Aturan tidak berubah |
| `Program.cs` | Diperbarui | `AddScoped<ClinicalActorScopeService>()`, `AddScoped<OutpatientEncounterListService>()` |

**Delta terhadap kontrak:** tidak ada. Catatan perilaku: `canCancel` dihitung dari butir
`OutpatientEncounter : Cancel` yang baru didaftarkan `BE-010`; sebelum itu bernilai `false` bagi
pengguna biasa (terbukti T15), dan `true` bagi SuperAdmin karena `AccessPermissionService`
membebaskan SuperAdmin.

## 3. Validasi

| Perintah / bukti | Hasil | Status |
| --- | --- | --- |
| `dotnet build … -c Release` | `0 Error(s)`, `244 Warning(s)` (sama dengan baseline). Satu `CS8602` baru sempat muncul di `ClinicalActorScopeService.cs:98`, diperbaiki dengan pengecekan `Email != null` | `PASS` |
| QBE Strict × 7 berkas (6 baru + `Program.cs`) | Setiap berkas `VIOLATION 0`, `REVIEW 0`, `PASS` | `PASS` |
| `AUTOMATED TEST` | `NOT APPLICABLE` — pola Bank Darah | — |

### Uji runtime — SuperAdmin (`rt_be009.log`)

Lingkungan sama dengan `BE-008` (`localhost:5199`, `QuilvianNewDevSukma`). Pembanding dihitung
dengan query read-only memakai definisi DP.3.1/DP.3.2.

| ID | Skenario | Hasil | Status |
| --- | --- | --- | --- |
| S1 | Metadata | `canReadAll true`, label "Semua klinik", 12 status, 56 klinik, 11 dokter | `PASS` |
| S2 | `mode=active` | API `141` = DB `141` | `PASS` |
| S3 | **AT-DP-06** — tidak ada baris IGD/Rawat Inap/penunjang | 0 baris menyimpang dari 141 | `PASS` |
| S4 | **AT-DP-20** — cari `ENC-RSMMC-00146` | Ditemukan, "Menunggu Perawat", `isHanging true` | `PASS` |
| S5 | **AT-DP-21** — kartu Menggantung lintas tanggal | `mode=today` 141 = `mode=active` 141 = DB 141 | `PASS` |
| S6 | Summary aktif | `waiting 126`, `inConsultation 15`, `readyForBilling 0`, `closed 0` | `PASS` |
| S7 | `hangingOnly` | 141 | `PASS` |
| S8 | Mode bawaan hari ini | API 2 = DB 2, semua bertanggal 2 Okt | `PASS` |
| S9 | Saringan klinik | 42 baris, semuanya klinik itu | `PASS` |
| S10–S13 | Saringan tidak valid (`range` tanpa tanggal, 46 hari, `mode=zzz`, status 99) | `400` dengan pesan `RJDP-VAL-007`/`008` | `PASS` |
| S14 | `mode=range` 30 Jul | 1 baris | `PASS` |
| S15 | Tanpa autentikasi | `401` | `PASS` |
| S16 | Registry hak akses | `OutpatientEncounter : Read`, `ReadAll` terdaftar | `PASS` |

Ringkasan: `17/17 PASS`.

### Uji runtime — akun uji (`rt_be009_scope.log`)

| ID | Skenario | Hasil | Status |
| --- | --- | --- | --- |
| T0 | Kunjungan uji `ENC-RSMMC-00185` (pasien `KSKTEST-RM-07`, dokter uji, poli tugas perawat uji) | `200` | `PASS` |
| T2 | **AT-DP-01** — dokter uji | API 1 = DB 1, semua `doctorId` = dokter uji | `PASS` |
| T3 | **AT-DP-02** — dokter uji mengirim `doctorId` dokter lain | 0 baris | `PASS` |
| T4 | Metadata dokter | `canReadAll false`, "Pasien Anda", tanpa opsi dokter | `PASS` |
| T6 | **AT-DP-03** — perawat uji (mapping 2 dari 6 poli cluster) | API 34 = DB 34, lima dokter berbeda, hanya dua poli itu, termasuk `00185` | `PASS` |
| T7 | Perawat mengirim `clinicId` poli lain | 0 baris | `PASS` |
| T8 | Metadata perawat | "Klinik cluster Anda" | `PASS` |
| T10–T12 | **AT-DP-04** — akun tanpa cakupan pada tiga endpoint | `403` "Akun Anda belum terhubung ke data dokter atau cluster perawat. …" | `PASS` |
| T14 | **AT-DP-05** — akun pendaftaran ber-`ReadAll`, bukan Super Admin | API 142 = DB 142, `canReadAll true` | `PASS` |
| T15 | `canCancel` tanpa butir `Cancel` | 0 dari 142 bernilai `true` | `PASS` |

Ringkasan: `16/16 PASS` (run kedua). Run pertama `14/16`: skrip membaca id kunjungan dari field
yang salah sehingga T0/T6 gagal dan pembersihan terlewat; run kedua memakai kunjungan yang sama dan
membatalkannya. `ENC-RSMMC-00185` kini `IsCancel = true`.

## 4. Data uji yang dibuat (semua lewat API aplikasi)

| Data | Keterangan | Penanganan |
| --- | --- | --- |
| Dokter `dr. UJI-RJDP Dokter` + akun `uji.rjdp.dokter@rsmmc.local` | Departemen Medis / Dokter Umum | Dinonaktifkan setelah seluruh task DP selesai |
| Pegawai `UJI-RJDP Perawat`, `UJI-RJDP Tanpa Cakupan`, `UJI-RJDP Pendaftaran` + akun | Keperawatan / Perawat Rawat Jalan (dua akun), Pendaftaran / Petugas Pendaftaran | Sama |
| Cluster staff perawat uji | Cluster `4db661cc-…`, 2 poli | Sama |
| Bypass geolokasi akun uji | Berlaku sampai 9 Okt 2026 | Berakhir otomatis |
| Hak akses jabatan | **Digabung**, bukan ditimpa: Dokter Umum 755 → 756 (`Read`), Perawat Rawat Jalan 11 → 12 (`Read`), Petugas Pendaftaran 0 → 2 (`Read`, `ReadAll`). Daftar asli disimpan di `policy_backup.json` (scratchpad) | Dibiarkan atau dipulihkan sesuai keputusan pemilik; lihat risiko 1 |

Kredensial akun uji tidak dicetak di laporan ini.

**Pembersihan 2 Okt 2026 (atas permintaan pemilik, lewat API aplikasi):**
- Hak ketiga jabatan dipulihkan dari cadangan, dan isinya cocok dengan daftar asli:
  - Dokter Umum: 755.
  - Perawat Rawat Jalan: 11.
  - Petugas Pendaftaran: 0.
- Dokter uji, empat pegawai uji (termasuk `UJI-RJDP Tanpa Hak` dari `FE-010`), dan cluster staff perawat uji dinonaktifkan; bypass geolokasi dimatikan.
- Kelima akun kini ditolak login dengan pesan "Akun tidak aktif."
- Data master uji tidak dihapus, hanya dinonaktifkan, sehingga jejaknya tetap ada.

## 5. Risiko tersisa

1. **Hak akses jabatan nyata ikut berubah.** Semua user berjabatan Dokter Umum dan Perawat Rawat
   Jalan kini memegang `OutpatientEncounter : Read`, dan Petugas Pendaftaran memegang `Read` +
   `ReadAll`. Ini memang konfigurasi yang dituju `DP.11`, tetapi diterapkan lebih awal oleh uji.
2. Pengenal dokter/perawat sekarang ada di tiga tempat (dua controller antrean + service baru);
   perubahan aturan pengenal harus menyentuh ketiganya sampai ada task refactor.
3. Kecocokan lewat email tetap ada sebagai jalur terakhir (warisan layar antrean).

## 6. Task berikutnya

`RJ-DOC-REV-BE-010` — pembatalan kunjungan.
