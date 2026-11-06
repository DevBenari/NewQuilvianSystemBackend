# Integration Contract — Nurse View

| Field | Nilai |
|---|---|
| `last_changed_in` | `0.1.2` · status `approved` — Rizki Gunawan, 2026-11-03 (`NV-DEC-033`). Sebelumnya `0.1.0`, 2026-10-28 (`NV-DEC-028`) |
| Owner | Rizki Gunawan · `approved_by` Rizki Gunawan · `approved_at` 2026-10-28 |
| Traceability | `NV-DEC-006`, `011`, `012`, `021`, `023`, `026`; `TLM-NV-001` keperawatan & episode |

Nurse View **tidak memanggil sistem luar**. Integrasi alat monitor pasien ditunda
(`NV-DEC-011`). Yang diatur di sini adalah arah baca dan tulis antarmodul internal.

## 1. Arah baca

| Dari modul | Data | Cara | Bila data tidak ada | Pemilik yang dikabari |
|---|---|---|---|---|
| Rawat Inap | Status bed, penempatan, reservasi | `InpBedManagementQueryService.ProjectBatchAsync` (`internal`), disetujui Hamzah 2026-10-28 | Bed tetap tampil dengan status dari proyeksi | Muhammad Hamzah, `TLM-NV-001` episode |
| Rawat Inap | Perawat PJ | Query `InpNurseAssignment` aktif | "Belum ada PJ" | Idem |
| Rawat Inap | Kebutuhan isolasi | `InpEpisode.RequiresIsolation` vs `MstBed.IsIsolationBed` | — | Idem |
| Master Data | Kamar per lantai, bangsal | Query `MstRoom`, `MstServiceUnit` | Kamar tanpa `FloorCode` tidak tampil | Admin master data |
| Clinical | TTV dan EWS | Query `TrxPatientVitalSign` per `InpEpisodeId` | "Belum ada TTV" | Muhammad Hamzah, `TLM-NV-001` keperawatan |
| Clinical | Risiko jatuh | Query hasil instrumen `FallRiskScale` | Tanpa ikon | Idem |
| Clinical | Alergi | Query `TrxPatientAllergy` aktif | Tanpa ikon | — |
| HR | Perawat jaga | Query `TrxShiftAssignment` `Published`; unit dan profesi cadangan dari periode roster dan data pegawai (`NV-DEC-033`) | "Roster shift ini belum tersedia" | — |
| Patient | Nama, jenis kelamin, tanggal lahir | Query `MstPatient`, langsung disamarkan | "Umur –" | — |

## 2. Arah tulis

| Ke modul | Yang ditulis | Catatan |
|---|---|---|
| Platform Identity | Satu `ApplicationUser` per perangkat lewat `UserManager` | Pola `QueueDisplayDeviceController` |
| Platform Number Series | Alokasi deret `NVW_DISPLAY_DEVICE` | `NumberSeriesAllocator` |
| Rawat Inap, Clinical, HR, Patient | **Tidak ada** | — |

## 3. Titik sentuh kode di modul lain

| Modul | Berkas | Perubahan | Status |
|---|---|---|---|
| Clinical / keperawatan | `PatientVitalSignController.cs` | Satu panggilan `NvwRealtimeService.NotifyVitalSignChangedAsync` sesudah commit | Disetujui Hamzah 2026-10-28 (`TLM-NV-001` keperawatan, `NV-OQ-016` ditutup) |
| Clinical / keperawatan | `InpatientClinicalContextService.MapCpptProfessionType` (`private static`) | Dijadikan `internal static` agar penyaringan profesi perawat/bidan di header memakai aturan yang sama, tanpa perubahan perilaku | Disetujui Hamzah 2026-10-28 (`TLM-NV-001` keperawatan permintaan 2) |
| Platform | `AuthController`, `Program.cs`, `AuthorizationPolicies.cs`, `QueueHub.cs` | Lihat `02-backend-architecture.md` §4.10 | Direstui lead dengan syarat (`NV-DEC-030`) |
| Frontend platform | `proxy.js`, `lib/hooks/auth/use-base-login.jsx` | Redirect akun perangkat ke halaman papan | Direstui lead dengan syarat (`NV-DEC-030`) |

## 4. Realtime

| Hal | Ketentuan |
|---|---|
| Sinyal | `NurseViewBoardChanged`, payload tanpa identitas |
| Pengiriman | Best-effort sesudah commit; gagal → dicatat log, simpan TTV tetap berhasil |
| Idempotensi | Penerima selalu memuat ulang papan penuh; sinyal ganda hanya menambah satu muat ulang |
| Tanpa sinyal | Muat ulang penuh setiap 60 detik tetap berjalan |
| Grup terlarang | `inpatient-bed-selection:{unit}` — akun perangkat ditolak oleh pemeriksaan izin yang sudah ada |

## Revision 2 — Tablet bed dan panggilan pasien

| Field | Nilai |
|---|---|
| `last_changed_in` | `0.2.0` · status `approved` — Rizki Gunawan, 2026-11-05 (`NV-DEC-055`) |
| Owner | Rizki Gunawan · `approved_by` Rizki Gunawan · `approved_at` 2026-11-05 |
| Traceability | `NV-DEC-035`, `037`, `043`, `046`, `050`, `052`; `NV-CAP-17`–`23` |

Tetap **tanpa sistem luar**. Bel kabel tidak diintegrasikan (`NV-DEC-037`).

### 5. Arah baca — tambahan

| Dari modul | Data | Cara | Bila data tidak ada | Pemilik yang dikabari |
|---|---|---|---|---|
| Rawat Inap | Penempatan aktif satu bed | Query `InpBedPlacement` dengan `BedId` tablet dan `EndDateTime IS NULL` (unik per bed, `INV-INP-02`) | Tablet `BedKosong`; panggilan baru ditolak `VAL-NV-28` | Muhammad Hamzah — **pemberitahuan ketergantungan baca baru**, bukan permintaan perubahan kode (`TLM-NV-002` episode, dibuat di `plan-module-delivery`) |
| Rawat Inap | Akhir penempatan (`EndDateTime`, `EndReason`) untuk panggilan aktif | Query berkala tiap 30 detik oleh `NvwPatientCallReconciliationService` | Panggilan tetap aktif sampai perawat menutup | Idem |
| Master Data | Bed, kamar, lantai | Query `MstBed`, `MstRoom` | Bed tanpa lantai tidak bisa dipasangi tablet (`VAL-NV-18`) | Admin master data |
| Patient | Nama, jenis kelamin, tanggal lahir | Query `MstPatient` lewat `InpEpisodeId`, langsung disamarkan, hanya untuk konsol | "Umur –" | — |
| Platform Identity | `DisplayName` pengklaim | Query `ApplicationUser` | Nama pengguna | — |

**Contoh ketergantungan.** Pukul 10.05 petugas admisi memindahkan pasien 331-C ke 402-B lewat
layar Rawat Inap. Rawat Inap mengakhiri penempatan 331-C dengan `EndReason = Transfer`, seperti
biasa, tanpa tahu Nurse View ada. Paling lambat 10.06 pemeriksaan berkala Nurse View membaca
akhir penempatan itu dan menutup panggilan 331-C. Bila kelak Rawat Inap mengganti arti
`EndReason`, Nurse View terdampak; karena itu Hamzah dikabari.

### 6. Arah tulis — tambahan

| Ke modul | Yang ditulis | Catatan |
|---|---|---|
| Platform Identity | Satu `ApplicationUser` per tablet lewat `UserManager` | Pola `NvwDisplayDeviceService` |
| Platform Number Series | Alokasi deret `NVW_BED_TABLET` | `NumberSeriesAllocator` |
| Rawat Inap, Clinical, Patient, Master Data | **Tidak ada** | — |

### 7. Titik sentuh kode di luar modul — tambahan

| Modul | Berkas | Perubahan | Status |
|---|---|---|---|
| Platform | `AuthController`, `AuthorizationPolicies.cs`, `Program.cs`, `QueueHub.cs` | `02-backend-architecture.md` R2.5 | **Menunggu restu lead `NV-OQ-028`** |
| Frontend platform | `proxy.js`, `use-base-login.jsx`, `root-layout-shell.jsx`, `auth-wrapper.jsx`, `session-activity-manager.jsx`, `queue-realtime-client.js`, `menu-items.jsx` | `03-frontend-architecture.md` R2.7 | **Menunggu restu lead `NV-OQ-028`**; butir menu di grup Rawat Inap juga dikabarkan ke Hamzah (`NV-FE-008`) |
| Rawat Inap, Clinical | — | **Tidak ada** perubahan kode | — |

### 8. Realtime — tambahan

| Hal | Ketentuan |
|---|---|
| Sinyal | `NurseViewPatientCallChanged`, payload tanpa identitas pasien dan tanpa nama perawat |
| Pengiriman | Sesudah commit, best-effort; gagal → dicatat log, perintah panggilan tetap berhasil |
| Penerima | TV (grup lantai yang sudah ada), konsol (grup konsol lantai), tablet (grup tablet) |
| Idempotensi | Penerima selalu memuat ulang bagiannya; sinyal ganda hanya menambah satu muat ulang |
| Tanpa sinyal | TV: strip dimuat ulang bersama papan tiap 60 detik. Konsol: daftar dimuat ulang tiap 15 detik. Tablet: keadaan dimuat ulang tiap 15 detik selama ada panggilan aktif (`NV-FE-009`) |
| Pemeriksaan berkala | `NvwPatientCallReconciliationHostedService` tiap 30 detik; galat dicatat log dan putaran berikutnya tetap berjalan |
