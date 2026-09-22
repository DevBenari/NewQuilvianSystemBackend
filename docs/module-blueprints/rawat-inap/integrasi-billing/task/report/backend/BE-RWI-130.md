# Laporan Perubahan Backend — `BE-RWI-130`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-130` |
| Judul | Event Admisi & Room Charge Fisik |
| Slice | Gelombang INT-BE-3 — `INP-S22` |
| Roadmap | [`docs/module-blueprints/rawat-inap/integrasi-billing/roadmap/backend-roadmap.md`](../../roadmap/backend-roadmap.md) — kartu `BE-RWI-130` |
| Trace | `FR-INT-001`, `FR-INT-004`; `RWI-DEC-156`, `RWI-AC-236`; Integration Contract §1.2 |
| Contract version | `1.0.0` |
| Dependency | `BE-RWI-128` |
| Klasifikasi | `MEDIUM` — Integrasi event admisi dan hunian awal tempat tidur |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs`, `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs` |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — Hooking event ADMISSION_CONFIRMED dan BED_OCCUPIED terpasang secara transaksional. |

---

## 1. Masalah yang Diselesaikan
Sebelumnya modul kasir tidak menerima sinyal pembukaan tagihan rawat inap saat pasien dinyatakan diterima rawat inap (`Admitted`), dan sewa kamar (*room charge*) tidak memiliki pemicu berbasis waktu hunian fisik riil saat perawat menempatkan pasien di tempat tidur.

---

## 2. Proses Bisnis & Perubahan yang Dikerjakan

1. **Injeksi `IInpIntegrationOutboxService`**:
   - Diinjeksikan ke dalam `InpEpisodeService` dan `InpBedOccupancyService`.
2. **Hooking Event `ADMISSION_CONFIRMED`**:
   - Pada `InpEpisodeService.ApplyStatusChangeAsync`, ketika status episode berubah menjadi `InpEpisodeStatus.Admitted`:
   - Mendaftarkan event outbox `ADMISSION_CONFIRMED` dengan kunci idempoten `INPATIENT:ADMISSION:{episode.Id}:1`.
   - Membawa data lengkap (`EpisodeId`, `EncounterId`, `PatientId`, `ServiceUnitId`, `PatientClassId`, `AdmissionDateTime`, `GuarantorId`).
3. **Hooking Event `BED_OCCUPIED`**:
   - Pada `InpBedOccupancyService.PlacePatientAsync`, ketika perawat bangsal menempatkan pasien secara fisik ke tempat tidur (`InpBedPlacement` aktif):
   - Mendaftarkan event outbox `BED_OCCUPIED` dengan kunci idempoten `INPATIENT:ROOM_STAY:{placement.Id}:1`.
   - Membawa data lengkap (`EpisodeId`, `EncounterId`, `PlacementId`, `BedId`, `RoomId`, `RoomClassId`, `OccupancyStartAt`, `PlacedByUserId`).
   - Sinyal ini menjadi dasar bagi modul Kasir untuk memulai perhitungan tarif kamar secara akurat tanpa membuat tagihan prematur saat pasien masih berupa draft/booking.

---

## 3. Berkas yang Berubah / Dibuat

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs` | Diperbarui — Injeksi outbox service & penerbitan event ADMISSION_CONFIRMED |
| `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs` | Diperbarui — Injeksi outbox service & penerbitan event BED_OCCUPIED |

---

## 4. Verifikasi dan Kepatuhan Kriteria Penerimaan

- **AC-1 (Event Admisi):** Episode yang sah menjadi `Admitted` menerbitkan event `ADMISSION_CONFIRMED` ke antrean outbox.
- **AC-2 (Event Bed Occupied):** Penempatan fisik tempat tidur menerbitkan event `BED_OCCUPIED` disertai `OccupancyStartAt` presisi.
- **AC-3 (Pencegahan Tagihan Prematur):** Status `Draft` atau `Reservation/Booking` tidak menerbitkan event penagihan.
