# Integration Contract — Nurse View

| Field | Nilai |
|---|---|
| `last_changed_in` | `0.1.0` · status `approved` — Rizki Gunawan, 2026-10-28 (`NV-DEC-028`) |
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
| HR | Perawat jaga | Query `TrxShiftAssignment` `Published` | "Roster shift ini belum tersedia" | — |
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
