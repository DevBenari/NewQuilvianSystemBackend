# Roadmap Backend Pendaftaran Admisi Pasien One Day Care (ODC)

| Metadata | Nilai |
| --- | --- |
| blueprint_id / pemilik sub-modul | `RWI-BP-001` / `episode-rawat-inap` (COMPOSITE; kepemilikan data pada [module map 10.1](../../02-module-map.md)) |
| planning_id / revision / tanggal | `ODC-DEL-20261017-01` / 1 / 2026-10-17 Asia/Jakarta |
| status / approval | Desain disetujui pengguna 17 Oktober 2026 lewat `/plan-module-delivery` menyerap `RWI-DEC-327` s.d. `332`. Kontrak target `0.15.0`. Setiap task dieksekusi tepat 1 task per pemanggilan via `build-module-backend` dengan task ID-nya |
| blueprint / kontrak | [Backend 18](../02-backend-architecture.md), [Kamus Data 25](../data/data-dictionary.md), [API Contract 16](../contracts/api-contract.md), [PRD 29](../04-prd-to-mvp.md), [Flowchart 20](../flowcharts/20-alur-admisi-odc.md), [Validation Matrix](../contracts/validation-matrix.md) |
| input revisions | Decision log 53; manifest anak 14; module map 7; kontrak target `0.15.0` |
| source SHA backend / frontend | `def6cf6ab5f945cb7bf228847d90fcb10a2d2d94` / `fc9aea6d62418460493899cb669832f032e48328` |
| hash lock | [admisi-odc-delivery-lock.json](admisi-odc-delivery-lock.json) |
| scope / ID | 2 task `BE-RWI-248` s.d. `249`. Ruang ID dipindai global 2026-10-17: terpakai sampai `BE-RWI-247`; berikutnya `BE-RWI-250` |
| roadmap pasangan | [Frontend](frontend-roadmap-admisi-odc.md) · [Traceability](requirement-traceability-admisi-odc.md) |

---

## Arti Tanda Status pada Dokumen Ini

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

---

## Grafik Urutan Dependency

```text
BE-RWI-248 ✅ ──> BE-RWI-249 ✅
```

Garis selalu prasyarat → task. `BE-RWI-248` menyediakan kueri ketersediaan bed ODC pada lapisan service/DTO yang kemudian digunakan oleh logika kelayakan dan admisi pada `BE-RWI-249`.

| Gelombang | Boleh Mulai Setelah | Task ID | Keterangan |
| ---: | --- | --- | --- |
| 1 | — | `BE-RWI-248` ✅ | Ekstensi query parameter `isOdcBed` / `isOdcRoom` dan field respon pada endpoint available beds |
| 2 | `BE-RWI-248` | `BE-RWI-249` ✅ | Penegakan invariant kelayakan bed ODC, pengecualian transfer bed (> 24 jam), dan validasi DPJP |

---

## Register Task

| Task ID / Status | Outcome | Requirement / Decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance Criteria | Verifikasi | Risiko / Pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-RWI-248` ✅ | Ekstensi kueri `isOdcBed` / `isOdcRoom` dan field respon `isOdcBed` pada `GET /bed-occupancies/available-beds` | FR-ODC-007; RWI-DEC-329, 330 | 0.15.0 draft; lock ODC-DEL-20261017-01 | `InpBedOccupancyDto`, `InpBedOccupancyService`, `BedOccupancyController` | Penambahan parameter `IsOdcBed` & `IsOdcRoom` pada `AvailableBedQuery`, field `IsOdcBed` pada `AvailableBedResponse`, serta filter LINQ | — | RWI-AC-465 (bagian filter API), VAL-ADM-10 | V-BE + UAT-ODC-01 | InPatientManagement; performa query bed ketersediaan | D-BE |
| `BE-RWI-249` ✅ | Invariant kelayakan penempatan bed ODC (`ODC_BED_REQUIRED`, `ODC_BED_RESERVED`), pengecualian transfer (> 24 jam), dan validasi DPJP wajib | FR-ODC-002, FR-ODC-004, FR-ODC-005; RWI-DEC-328, 329, 330, 331 | 0.15.0 draft; lock ODC-DEL-20261017-01 | `InpPlacementEligibilityService`, `InpAdmissionService` | Validasi DPJP wajib saat admisi ODC (`VAL-ADM-13`); validasi penempatan bed ODC vs reguler (`VAL-ADM-10`, `VAL-ADM-11`); bypass validasi ODC pada transfer (`VAL-ADM-12`) | `BE-RWI-248` | RWI-AC-464, RWI-AC-465, RWI-AC-466, RWI-AC-467; VAL-ADM-10 s.d. 13 | V-BE + UAT-ODC-01, 02, 03 | InPatientManagement; integritas mutasi bed | D-BE |

---

## Batas Handoff dan Definition of Done

### D-BE — Bukti Wajib Setiap Task Backend

QBE preflight dan kesesuaian engineering diselesaikan **pada waktu eksekusi** dari `AGENTS.md` backend dan dokumen engineering canonical: owner/prefix `Inp`, wewenang tulis per task, dan hash masukan pada lock.

V-BE adalah bundel bukti menurut `rules/backend/TEST_POLICY.md`: review diff/scope, `dotnet build` project aplikasi (`dotnet restore` bila dependency berubah), verifikasi kontrak API (method, path, request/response, kode status), dan verifikasi proses bisnis untuk jalur normal dan tidak normal. Dokumen ini **tidak menuntut unit test otomatis**.

Laporan tracked disimpan di `task/report/backend/<TASK-ID>.md`.

---

## Kartu Task

### BE-RWI-248 — Ekstensi Kueri `isOdcBed` / `isOdcRoom` dan Field Respon `isOdcBed` pada Available Beds

| Field | Isi |
| --- | --- |
| Status | ✅ Selesai (17 Oktober 2026). DTO kueri/respon dan filter LINQ ketersediaan bed ODC diperbarui. Laporan: [BE-RWI-248.md](../task/report/backend/BE-RWI-248.md) |
| Outcome | Endpoint `GET /api/v1/bed-occupancies/available-beds` mendukung filter query `isOdcBed` dan `isOdcRoom`, serta mengembalikan status `isOdcBed` pada setiap item tempat tidur |
| Ukuran / Batas | Modifikasi DTO `AvailableBedQuery` / `AvailableBedResponse` dan LINQ query pada `InpBedOccupancyService.SearchAvailableBedsAsync` |
| Jejak | FR-ODC-007; RWI-DEC-329, RWI-DEC-330; RWI-AC-465 |
| Desain / Kontrak | Backend 18.1; API Contract Bagian 16 (`Inpatient Bed Occupancy`) |
| Reuse / Disposisi | `InpBedOccupancyDto.cs` (Extend), `InpBedOccupancyService.cs` (Extend), `BedOccupancyController.cs` (Extend) |
| Cakupan | Tambahkan properti `bool? IsOdcBed { get; set; }` dan `bool? IsOdcRoom { get; set; }` pada `AvailableBedQuery`. Tambahkan properti `bool IsOdcBed { get; set; }` pada `AvailableBedResponse`. Pada `SearchAvailableBedsAsync`, jika `query.IsOdcBed.HasValue`, tambahkan klausul `Where(b => b.IsOdcBed == query.IsOdcBed.Value)`. Jika `query.IsOdcRoom.HasValue`, tambahkan klausul `Where(b => b.Room.IsOdcRoom == query.IsOdcRoom.Value)`. Petakan nilai `b.IsOdcBed` ke DTO response |
| Batas Source | `Areas/HealthServices/InPatientManagement/DTOs/InpBedOccupancyDto.cs`; `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs`; `Areas/HealthServices/InPatientManagement/Controllers/BedOccupancyController.cs` |
| Dependency | — |
| Verifikasi yang Wajib | V-BE (QBE preflight saat eksekusi) + kueri HTTP lokal verifikasi filter `isOdcBed=true` dan `isOdcBed=false` |
| Definition of Done | D-BE; kriteria penerimaan terverifikasi; laporan `task/report/backend/BE-RWI-248.md` |

---

### BE-RWI-249 — Invariant Kelayakan Penempatan Bed ODC, Pengecualian Transfer Bed (> 24 Jam), dan Validasi DPJP Wajib

| Field | Isi |
| --- | --- |
| Status | ✅ Selesai (17 Oktober 2026). Invariant kelayakan ODC, validasi DPJP wajib, dan pengecualian transfer bed berhasil ditegakkan. Laporan: [BE-RWI-249.md](../task/report/backend/BE-RWI-249.md) |
| Outcome | Backend menjamin kepatuhan alokasi bed ODC pada admisi baru, mewajibkan penetapan DPJP Utama rawat inap pada pasien ODC, serta memfasilitasi mutasi ke bed reguler tanpa penolakan jika pasien dirawat melebihi 24 jam |
| Ukuran / Batas | Penambahan aturan validasi pada `InpPlacementEligibilityService.EvaluatePlacementEligibilityCore` dan `InpAdmissionService.CreateInpatientAdmissionCore` |
| Jejak | FR-ODC-002, FR-ODC-004, FR-ODC-005; RWI-DEC-328, 329, 330, 331; RWI-AC-464, 465, 466, 467; VAL-ADM-10 s.d. 13 |
| Desain / Kontrak | Backend 18.2; Validation Matrix `VAL-ADM-10` s.d. `VAL-ADM-13`; State Transition Matrix Bagian Eskalasi Transfer ODC |
| Reuse / Disposisi | `InpPlacementEligibilityService.cs` (Extend), `InpAdmissionService.cs` (Extend) |
| Cakupan | (1) Pada `CreateInpatientAdmissionCore`, jika `patientType == InpPatientType.OneDayCare` (atau nilai 4), validasi bahwa `PrimaryDoctorId` tidak boleh kosong/null; jika kosong lemparkan `ValidationException` 422 (`PRIMARY_DOCTOR_REQUIRED`). (2) Pada `EvaluatePlacementEligibilityCore`, evaluasi penempatan bed: jika `patientType == OneDayCare` dan `context == InpPlacementContext.Admission`, jika `bed.IsOdcBed == false`, tolak dengan 422 (`ODC_BED_REQUIRED`). Jika `patientType != OneDayCare` dan `bed.IsOdcBed == true`, tolak dengan 422 (`ODC_BED_RESERVED`). (3) Pengecualian Transfer: Jika `context == InpPlacementContext.Transfer`, bypass aturan `ODC_BED_REQUIRED` sehingga pasien ODC yang membutuhkan rawat inap > 24 jam dapat dipindahkan ke bed reguler via `TransferCoreAsync` |
| Batas Source | `Areas/HealthServices/InPatientManagement/Services/InpPlacementEligibilityService.cs`; `Areas/HealthServices/InPatientManagement/Services/InpAdmissionService.cs` |
| Dependency | `BE-RWI-248` |
| Verifikasi yang Wajib | V-BE (QBE preflight saat eksekusi) + uji validasi admisi ODC bed non-ODC (gagal 422), non-ODC bed ODC (gagal 422), admisi ODC bed ODC (sukses 201), dan transfer ODC bed reguler (sukses 200) |
| Definition of Done | D-BE; kriteria penerimaan terverifikasi; laporan `task/report/backend/BE-RWI-249.md` |
