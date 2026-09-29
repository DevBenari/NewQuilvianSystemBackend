# Laporan Perubahan Backend — `BE-ACC-P2-024`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-024` |
| Judul | Daftar, rincian, dan ringkasan kejadian |
| Slice | `P2-2` — Wave B kotak masuk kejadian |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-024` (revisi 4, `APPROVED`) |
| Trace | `FR-P2-006`, `FR-P2-015`; `ACC-DEC-057` |
| Contract version | `ACC-API-0.12` grup Accounting Event — `GET /`, `GET /{id}`, `GET /summary`; `03-frontend-architecture.md` bagian 11.1 dan 11.2 |
| Dependency | `BE-ACC-P2-019` ✅, `BE-ACC-P2-020` ✅ |
| Klasifikasi | `MEDIUM` — tiga endpoint baca, nol perubahan skema |
| Task mode | `BACKEND` |
| Target tulis | Controller, service, DTO kotak masuk; laporan ini; baris status roadmap |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `8535dd56` (branch `rizkiG`), perubahan belum di-commit — bertumpuk dengan `BE-ACC-P2-021` yang juga belum di-commit |
| Tanggal | 24 September 2026 |
| Status | **✅ SELESAI** — 24 September 2026. 4 dari 4 acceptance di source; build Rizki **berhasil, 0 error, 222 warning** (322,9 detik); uji panggil lewat layar Kotak Masuk berhasil untuk `GET /` dan `GET /summary` (bagian 7). `GET /{id}` diuji lewat layar Rincian (`FE-ACC-P2-012`). Riwayat: 🟡 pada hari yang sama |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `AccountingEvent` |
| Registry | `Acc` — `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001`, `QBE-PAGE-001` (pola paging/sort `ChartOfAccount`) |
| Standar endpoint | **Transaksi**, arketipe *worklist* read-only. Sengaja **tanpa** `GET /options`, `GET /filters/metadata`, `PATCH /status`, `DELETE` |

## 1. Endpoint

#### Corporate - Accounting - Accounting Event

Base URL: `api/v1/corporate/accounting/accounting-events`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar kejadian berhalaman | `AccountingEvent : Read` | `AccountingEventPagedQuery` | `ApiResponse<PagedResult<AccountingEventListDto>>` |
| `GET` | `/summary` | Jumlah kejadian per status | `AccountingEvent : Read` | `legalEntityId` (query, opsional) | `ApiResponse<AccountingEventSummaryDto>` |
| `GET` | `/{id}` | Rincian: komponen, riwayat percobaan, jurnal, pesan asli | `AccountingEvent : Read` | — | `ApiResponse<AccountingEventDetailDto>` |

Kode status: `200`; `400` bila `PeriodCode` bukan `YYYY-MM`; `404` kejadian tidak ditemukan;
`409` penjaga badan hukum utama.

## 2. Isi DTO — delta kontrak

Kontrak hanya menyebut nama DTO. Isinya diturunkan dari layar 11.1 dan 11.2:

| DTO | Bidang |
| --- | --- |
| `AccountingEventPagedQuery` | `PageNumber`, `PageSize` (maks 200), `LegalEntityId`, `EventStatus`, `EventTypeCode`, `PeriodCode` (`YYYY-MM`, menyaring `AccountingDate` di bulan itu), `Search` (nomor kejadian), `SortBy` (`eventNumber`, `accountingDate`, `amount`; bawaan waktu terima), `SortDirection` (bawaan `desc`) |
| `AccountingEventListDto` | `Id`, `LegalEntityId`, `EventNumber`, `EventTypeCode`, `EventTypeName`, `SourceModule`, `AccountingDate`, `Amount`, `CurrencyCode`, `EventStatus` (**angka** enum, seperti endpoint Accounting lain), `HoldReasonCode`, `JournalId`, `JournalNumber`, `AttemptCount`, `ReceivedAt` |
| `AccountingEventDetailDto` | Seluruh bidang daftar + `SourceTransactionId`, `SourceVersion`, `EventOccurredAt` (+07:00), `DocumentDate`, `CorrelationId`, `CausationId`, `IgnoreReason`, `JournalStatus`, `JournalPeriodCode`, `RawPayload`, `Components`, `Attempts` (terbaru di atas) |
| `AccountingEventSummaryDto` | `LegalEntityId`, `Total`, `Diterima`, `Tertahan`, `Gagal`, `Terjurnal`, `Diabaikan`, `Tercatat` |

`SourceTransactionId` sengaja **tidak** ada di daftar — hanya di rincian — karena bertanda sensitif
(kamus data bagian 9: penunjuk ke kunjungan pasien).

## 3. Pemetaan acceptance

| # | Acceptance kartu | Bukti | Status |
| ---: | --- | --- | :---: |
| 1 | Penyaring status, jenis, periode, badan hukum | `GetPagedAsync` | ✅ |
| 2 | Rincian memuat komponen, percobaan, `HoldReasonCode`, nomor jurnal | `GetByIdAsync` | ✅ |
| 3 | `RawPayload` tidak dikirim ke daftar; tampil di rincian | `AccountingEventListDto` tanpa `RawPayload` | ✅ |
| 4 | Ringkasan untuk penanda angka menu | `GetSummaryAsync` — hitung per status, termasuk `Tercatat` | ✅ |

## 4. Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Areas/Corporate/AccountingManagement/AccountingEvent/Controllers/AccountingEventController.cs` | Diperbarui | + `GET /`, `GET /summary`, `GET /{id:guid}` dengan `[AccessAction("Read", …)]` + `[AccessPermission("AccountingEvent", "Read")]` |
| `Areas/Corporate/AccountingManagement/AccountingEvent/Services/AccAccountingEventService.cs` | Diperbarui | + `GetPagedAsync`, `GetByIdAsync`, `GetSummaryAsync` |
| `Areas/Corporate/AccountingManagement/AccountingEvent/DTOs/AccountingEventDtos.cs` | Diperbarui | + lima DTO |

## 5. Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| Akses | `ControllerName` `AccountingEvent`; `AccessAction` `Read` = argumen 2 `AccessPermission`; `AccessTypes.Read` |
| `dotnet build` | **Berhasil** — Rizki, 24 September 2026: `Build succeeded with 222 warning(s) in 322,9s` |
| Uji panggil | **Lulus** untuk `GET /` dan `GET /summary` — bagian 7 |

## 6. Berikutnya

`FE-ACC-P2-011` memakai ketiga endpoint ini.

## 7. Bukti uji lewat layar — Rizki, 24 September 2026

| Yang terlihat di layar Kotak Masuk | Endpoint yang terbukti |
| --- | --- |
| Tab "Tertahan 1", "Terjurnal 1", "Gagal" tanpa angka | `GET /summary` — jumlah per status, nol tidak ditampilkan |
| Tab Semua: `EVT-UJI-002` Terjurnal `JU/2026/09/00005`, `EVT-UJI-001` Tertahan | `GET /` — daftar, `JournalNumber`, `EventTypeName` (Pembayaran Pasien) dan `EventTypeCode` bila nama kosong |
| Tab Tertahan + periode September 2026 → satu baris, alasan "Jenis kejadian belum terdaftar" | `GET /` penyaring `EventStatus` dan `PeriodCode`; `HoldReasonCode` |
| Rincian `EVT-UJI-002` dan `EVT-UJI-001` lewat layar Rincian (`FE-ACC-P2-012` skenario 1, 3, 6), uji Rizki kedua 24 September 2026 | `GET /{id}` — kartu jurnal, alasan tahan, riwayat percobaan, pesan asli |
