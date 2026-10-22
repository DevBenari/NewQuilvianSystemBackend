# Roadmap Backend Prioritas Kamar Pasien Member di Antrean Transfer & Lencana Tier

| Metadata | Nilai |
|---|---|
| blueprint_id / owner | `RWI-BP-001` / `episode-rawat-inap` (bentuk `COMPOSITE`) |
| planning_id / revision / tanggal | `TRF-DEL-20261019-01` / 1 / 2026-10-19 Asia/Jakarta |
| status / approval | `APPROVED_DESIGN_PLANNED`; disetujui untuk implementasi demo Selasa 20 Oktober 2026. Eksekusi memakai `build-module-backend`, tepat satu task per pemanggilan |
| kontrak terkunci | `0.17.0`; [Backend 20](../02-backend-architecture.md), [API 18](../contracts/api-contract.md), [Integration 15](../contracts/integration-contract.md), [PRD 31](../04-prd-to-mvp.md), [Testing 25](../testing/acceptance-test-matrix.md) |
| source snapshot | Backend `MHamzah@7a4d443fcdf0c9d33cc1a9f4be81522b4fadaefb`; frontend `HamzahV2@5ad45a6944b3c320038cc48bb288471cd8f2910a` |
| hash lock | [prioritas-antrean-member-delivery-lock.json](prioritas-antrean-member-delivery-lock.json) |
| scope / ID | 2 task: `BE-RWI-252` dan `BE-RWI-253`; berikutnya `BE-RWI-254` |
| roadmap pasangan | [Frontend](frontend-roadmap-prioritas-antrean-member.md) · [Traceability](requirement-traceability-prioritas-antrean-member.md) |
| target demo | Demo Operasional Selasa, 20 Oktober 2026 (`IGD-REQ-004`) |

## Arti Tanda Status pada Dokumen Ini

| Tanda | Artinya |
|:---:|---|
| ✅ | Selesai dan seluruh acceptance criteria serta Definition of Done terbukti pada laporan task |
| 🟡 | Source ada, tetapi bukti acceptance belum lengkap; belum selesai |
| ⛔ | Prasyarat task belum terpenuhi; task tidak boleh dimulai |
| tanpa tanda | `NOT_STARTED` (Belum dikerjakan) |

## Grafik Urutan Dependency

```text
BE-RWI-252 [✅] ──> BE-RWI-253 [✅]
```

| Gelombang | Boleh Mulai Setelah | Task ID | Hasil Gelombang |
|---:|---|---|---|
| 1 | — | `BE-RWI-252` [✅] | DTO `InpatientAdmissionTransferItemResponse` diperluas dan query relasional EF Core membaca CITO dari IGD dan Membership aktif dari CRM secara fail-safe |
| 2 | `BE-RWI-252` | `BE-RWI-253` [✅] | Algoritma pengurutan multi-kriteria (CITO desc → Member Prioritas desc → PriorityLevel asc → DecidedAt asc → TransferId asc) aktif dan stabil pada endpoint antrean transfer |

Garis berarti prasyarat → task. Setiap task muncul tepat satu kali dan pasangan dependency grafik sama persis dengan kolom Dependency pada register.

## Spesifikasi Endpoint API Terkait

Swagger tag: `[Tags("Health Services / Inpatient Management / Inpatient Admission Transfer")]`. Base route: `/api/v1/health-services/inpatient-management/admission-transfers`.

| Method | Path | Permission | Request | Response Data | Perubahan pada Kontrak 0.17.0 |
|---|---|---|---|---|---|
| `GET` | `/` | `InpatientAdmissionTransfer:Read` | Query parameter standard (paging/filter) | `ApiResponse<List<InpatientAdmissionTransferItemResponse>>` | Field baru pada setiap item: `isCito: bool`, `isPriorityMember: bool`, `memberTierName: string?`, `memberPriorityLevel: int?`. Urutan array terjamin deterministik. |
| `GET` | `/{id}` | `InpatientAdmissionTransfer:Read` | `id: Guid` | `ApiResponse<InpatientAdmissionTransferItemResponse>` | Field detail respon memuat metadata CITO dan membership yang sama. |

Tidak ada endpoint baru. Tidak ada migrasi database (`0 migrasi EF Core`). Tidak ada penulisan ke modul CRM atau formulir IGD.

## Register Task

| Task ID / Status | Outcome | Requirement / Keputusan | Kontrak | Reuse | Cakupan | Dependency | Acceptance Criteria | Verifikasi | Risiko / Owner | DoD |
|---|---|---|---|---|---|---|---|---|---|---|
| `BE-RWI-252` [✅] | Ekstensi DTO `InpatientAdmissionTransferItemResponse` dan proyeksi query EF Core membaca status CITO IGD dan Membership aktif CRM secara fail-safe | FR-TRF-008, FR-TRF-012; RWI-DEC-338, 339, 341 | 0.17.0; Bab 18 API, Bab 15 Integrasi, Bab 20 Backend | `InpatientAdmissionTransferDtos.cs`, `InpAdmissionTransferService.Queries.cs`, `ApplicationDbContext` | Tambahkan properti `IsCito`, `IsPriorityMember`, `MemberTierName`, `MemberPriorityLevel` ke DTO; ubah `BuildTransferQuery` untuk left-join `EmgAdmissionRequest` (Signed) dan `MstPatientMembership` join `MstMembershipTier` (Active, valid date, `PriorityQueue == true`) | — | RWI-AC-481, RWI-AC-483, RWI-AC-484, RWI-AC-485, RWI-AC-492, RWI-AC-493; TRF-AT-01, 04, 05, 06, 07, 08, 14, 16 | `V-BE`; build lolos, verifikasi proyeksi query tanpa N+1, verifikasi fallback null | InPatientManagement; risiko query overhead relasi membership | `D-BE`; [laporan](../task/report/backend/BE-RWI-252.md) |
| `BE-RWI-253` [✅] | Algoritma pengurutan multi-kriteria deterministik (CITO → Member Prioritas → Reguler) pada antrean transfer rawat inap | FR-TRF-008; RWI-DEC-338, 339, 340 | 0.17.0; Bab 18 API, Bab 20 Backend | `InpAdmissionTransferService.cs` | Terapkan multi-level sorting: `OrderByDescending(IsCito).ThenByDescending(IsPriorityMember).ThenBy(MemberPriorityLevel ?? int.MaxValue).ThenBy(DecidedAt).ThenBy(TransferId)`; verifikasi tidak ada auto-booking | `BE-RWI-252` | RWI-AC-482, RWI-AC-489, RWI-AC-490, RWI-AC-491; TRF-AT-02, 03, 11, 12, 13 | `V-BE`; verifikasi urutan dataset campuran CITO dan Member, build aplikasi | InPatientManagement; risiko stabilitas sorting pada paging besar | `D-BE`; [laporan](../task/report/backend/BE-RWI-253.md) |

## Batas Handoff dan Definition of Done

### V-BE — Bukti Wajib Setiap Task Backend

1. Jalankan QBE preflight saat eksekusi berdasarkan `AGENTS.md`, kepemilikan modul, kontrak lock, dan file scope.
2. Review `git diff --check`, diff/scope: pastikan **nol tabel baru, nol kolom baru, nol migrasi EF Core**, dan tidak ada file di luar scope yang tersentuh.
3. Jalankan `dotnet build QuilvianSystemBackend.csproj` untuk memastikan kompilasi 100% bersih tanpa warning/error baru (dikecualikan pada eksekusi ini sesuai arahan eksplisit pengguna untuk build mandiri).
4. Verifikasi kontrak DTO dan logika bisnis query/sorting terhadap dataset uji (CITO, Member Platinum/Gold, Reguler, dan Member Expired).
5. Automated test baru **NOT APPLICABLE / tidak diwajibkan** oleh kebijakan backend `rules/backend/TEST_POLICY.md`. Bukti berbasis build bersih, conformance review, dan pengujian logis query.

### D-BE — Selesai Berarti

- Outcome, acceptance criteria, dan seluruh bukti `V-BE` task terpenuhi secara tuntas.
- Task report tracked ditulis pada `task/report/backend/<TASK-ID>.md`.
- Roadmap diberi tanda ✅ hanya setelah bukti nyata tercantum dalam laporan.

---

## Kartu Task

### BE-RWI-252 [✅] — Ekstensi DTO dan Proyeksi Query EF Core CITO & Membership Aktif

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-10-20; DTO & Query diperbarui; build mandiri oleh user; [laporan](../task/report/backend/BE-RWI-252.md)) |
| Outcome | DTO `InpatientAdmissionTransferItemResponse` memuat field `IsCito`, `IsPriorityMember`, `MemberTierName`, `MemberPriorityLevel` dan query `BuildTransferQuery` memproyeksikan data dari modul IGD dan CRM secara efisien dan fail-safe |
| File Terdampak | `Areas/HealthServices/InPatientManagement/DTOs/InpatientAdmissionTransferDtos.cs`; `Areas/HealthServices/InPatientManagement/Services/InpAdmissionTransferService.Queries.cs` |
| Jejak Requirement | `FR-TRF-008`, `FR-TRF-012`; `RWI-DEC-338`, `339`, `341`; `RWI-AC-481`, `483`, `484`, `485`, `492`, `493` |
| Kontrak Target | `0.17.0`; Bab 18 API Contract, Bab 15 Integration Contract, Bab 20 Backend Architecture |
| Reuse | `ApplicationDbContext`, entity `EmgAdmissionRequest`, `MstPatientMembership`, `MstMembershipTier` |
| Dependency | Tidak ada (dapat langsung dimulai) |
| Verifikasi yang Wajib | `V-BE`; `dotnet build` lolos, review diff DTO dan query, uji null-safety |
| Definition of Done | `D-BE`; laporan `task/report/backend/BE-RWI-252.md` |

**Kriteria Penerimaan**:
- DTO `InpatientAdmissionTransferItemResponse` memiliki properti:
  - `public bool IsCito { get; set; }`
  - `public bool IsPriorityMember { get; set; }`
  - `public string? MemberTierName { get; set; }`
  - `public int? MemberPriorityLevel { get; set; }`
- Proyeksi `BuildTransferQuery`:
  - `IsCito` bernilai `true` jika dan hanya jika terdapat `EmgAdmissionRequest` berstatus `Signed` dengan `IsCito == true` untuk encounter terkait.
  - `IsPriorityMember` bernilai `true` jika pasien memiliki keanggotaan aktif (`MembershipStatus == Active`), tanggal hari ini berada di antara `PeriodStartDate` dan `ExpiredDate`, serta tier-nya memiliki flag `PriorityQueue == true`.
  - Pasien non-member atau keanggotaan kedaluwarsa/inaktif/non-prioritas menghasilkan `IsPriorityMember = false`, `MemberTierName = null`, `MemberPriorityLevel = null`.
  - Relasi query menggunakan left join (`DefaultIfEmpty()`) sehingga ketiadaan data membership atau data IGD tidak menyebabkan crash / HTTP 500 (`RWI-AC-493`).
- Endpoint `GET /admission-transfers/{id}` mengembalikan properti yang sama pada objek detailnya (`RWI-AC-492`).

---

### BE-RWI-253 [✅] — Algoritma Pengurutan Multi-Kriteria Antrean Transfer Rawat Inap

| Field | Isi |
|---|---|
| Status | ✅ SELESAI (2026-10-20; OrderBy multi-kriteria diterapkan; build mandiri oleh user; [laporan](../task/report/backend/BE-RWI-253.md)) |
| Outcome | Antrean transfer rawat inap diurutkan secara multi-kriteria deterministik di server backend: CITO teratas, diikuti Member Prioritas sesuai level, lalu Reguler secara FIFO |
| File Terdampak | `Areas/HealthServices/InPatientManagement/Services/InpAdmissionTransferService.cs` |
| Jejak Requirement | `FR-TRF-008`; `RWI-DEC-338`, `339`, `340`; `RWI-AC-482`, `489`, `490`, `491` |
| Kontrak Target | `0.17.0`; Bab 18 API Contract, Bab 20 Backend Architecture |
| Reuse | Method `GetAdmissionTransfersAsync` di `InpAdmissionTransferService.cs` |
| Dependency | `BE-RWI-252` |
| Verifikasi yang Wajib | `V-BE`; `dotnet build` lolos, verifikasi sorting logic multi-tier, verifikasi tidak ada pemesanan bed otomatis |
| Definition of Done | `D-BE`; laporan `task/report/backend/BE-RWI-253.md` |

**Kriteria Penerimaan**:
- Pengurutan query `itemsQuery` pada `InpAdmissionTransferService.cs:48` menerapkan urutan bertingkat:
  ```csharp
  itemsQuery = itemsQuery
      .OrderByDescending(x => x.IsCito)
      .ThenByDescending(x => x.IsPriorityMember)
      .ThenBy(x => x.MemberPriorityLevel ?? int.MaxValue)
      .ThenBy(x => x.DecidedAt)
      .ThenBy(x => x.TransferId);
  ```
- Pasien CITO mutlak berada di posisi paling atas mengalahkan pasien member maupun reguler (`RWI-DEC-338`).
- Pasien Member Prioritas berada di atas Pasien Reguler; sesama member prioritas didahulukan tier tertinggi (`MemberPriorityLevel` lebih kecil didahulukan).
- Pasien dengan tingkat prioritas yang sama diurutkan secara FIFO berdasarkan `DecidedAt` paling awal, dengan `TransferId` sebagai tie-breaker penjamin kestabilan paging.
- Backend tidak melakukan pemesanan bed otomatis; alokasi kamar dan bed tetap diserahkan kepada keputusan manual petugas admisi (`RWI-AC-489`).
