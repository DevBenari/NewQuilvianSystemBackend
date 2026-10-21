# Roadmap Backend Peta Bangsal Denah

| Metadata | Nilai |
|---|---|
| blueprint_id / owner | `RWI-BP-001` / `episode-rawat-inap` |
| planning_id / revision / tanggal | `PETA-DEL-20261020-01` / 1 / 2026-10-20 Asia/Jakarta |
| status / approval | `APPROVED_DESIGN_PLANNED`; `RWI-DEC-352` |
| kontrak terkunci | `0.18.0`; isi kontrak backend tidak bergerak ([API contract](../contracts/api-contract.md) 14.2, [backend architecture](../02-backend-architecture.md)) |
| source snapshot | Backend `6e0f2b1b58a2192a0f54f0fdff718bc77670e3bd`; frontend `a491b05afcb6b3ed70218ccaa557007719d45ac3` |
| hash lock | [peta-bangsal-denah-delivery-lock.json](peta-bangsal-denah-delivery-lock.json) |
| scope / ID | **0 task.** ID backend berikutnya tetap `BE-RWI-254` |
| roadmap pasangan | [Frontend](frontend-roadmap-peta-bangsal-denah.md) · [Traceability](requirement-traceability-peta-bangsal-denah.md) |

## Arti Tanda Status pada Dokumen Ini

| Tanda | Artinya |
|:---:|---|
| ✅ | Selesai; seluruh acceptance criteria dan DoD terbukti pada laporan task |
| 🟡 | Source ada, tetapi bukti acceptance belum lengkap; belum selesai |
| ⛔ | Prasyarat belum terpenuhi; task tidak boleh dimulai |
| tanpa tanda | Belum dikerjakan |

## Grafik Urutan Dependency

```text
(tidak ada task backend pada planning PETA-DEL-20261020-01)
```

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| — | — | Tidak ada task |

## Mengapa tidak ada task backend

`RWI-DEC-343` menetapkan bahwa amandemen ini hanya mengubah rupa denah: tidak ada endpoint, DTO,
tabel, kolom, migration, maupun permission baru. Semua data yang dipakai frontend sudah dikirim
endpoint yang ada:

| Kebutuhan frontend | Sumber backend yang sudah ada | Bukti |
|---|---|---|
| Kamar, status kamar, badge, bed, pesanan | `GET api/v1/health-services/inpatient-management/bed-management/floor-plan` dan `GET .../bed-occupancies/floor-plan` | `InpatientBedManagementController.cs:26`, `InpatientBedOccupancyController.cs:76` |
| Label HD bila sekurang-kurangnya satu penghuni HD | `DependencySummary.IsHighDependency` | `InpFloorPlanQueryService.cs:260` |
| Angka bed kosong per tab lantai | `Floors[].AvailableBedCount` | `InpFloorPlanQueryService.cs:139`; `InpatientFloorPlanDtos.cs:42-47` |
| Privasi profil Housekeeping | `HoldingContext` dan `DependencySummary` dihapus sebelum JSON | `InpFloorPlanQueryService.cs:199`; `GUARD-DNH-01` |
| Susunan lantai untuk Master Bed | `GET api/v1/health-services/master-data/rooms/floor-plan-layout` | `RoomController.cs:63` |

Akibatnya QBE preflight, `dotnet build`, dan verifikasi kontrak backend tidak berlaku pada planning
ini. Bila task frontend menemukan kebutuhan data yang tidak ada di tabel di atas, pekerjaan berhenti
dan kembali ke `design-business-module`; task backend tidak dibuat diam-diam dari roadmap ini.
