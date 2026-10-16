# Laporan Perubahan Backend — `BE-RAD-19`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RAD-19` |
| Judul | Rekap Beranda Radiologi |
| Slice | Paritas v1, Fase 3 — `RAD-EVD-001` bagian 2.1 dan 6 |
| Roadmap | `roadmap/backend-roadmap.md` bagian 6c |
| Trace | `RAD-DEC-018`, `RAD-DEC-020` |
| Contract version | `RAD-API-001` revision 14 — amandemen ditulis task ini |
| Dependency | `BE-RAD-18` (aturan baca pembayaran pada daftar) |
| Task mode | `BACKEND` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `49c6a6920`, branch `yoga` |
| Tanggal | 2026-10-15 |
| Status | **Selesai — batas verifikasi.** 46/46 uji Radiologi lulus. Belum dijalankan terhadap devYoga |

---

## 1. Yang dibuat

`GET /api/v1/health-services/radiology-management/rad-orders/dashboard` — `RadOrder : Read`, baca saja, tanpa
schema. `RadOrderService.GetDashboardAsync(DateTime? sekarangUtc)`; DTO `RadDashboardResponse` dan tiga DTO anak di
`RadiologyFilterAndSummaryDtos.cs`.

| Bagian v1 | Isi response |
| --- | --- |
| Kartu | `TotalOrderCount`, `OutpatientOrderCount`, `InpatientOrderCount`, `EmergencyOrderCount`, `PaidOrderCount` |
| Status dipantau | `WaitingOrderCount` (Requested/Accepted/Scheduled), `InProgressOrderCount` (InProgress/OnHold/CancelRequested), `CompletedOrderCount`, `CancelledOrderCount` (Cancelled/Rejected), `UrgentOrderCount` |
| Tren 30 hari | `DailyOrders` — tepat 30 butir `yyyy-MM-dd` WIB, termasuk hari kosong |
| Komposisi | `Composition` — Rujukan (`RegPatientEncounter.IsReferral`) lebih dulu seperti v1, lalu RJ/RI/IGD |
| Booking terbaru | `RecentOrders` — lima terakhir tanpa batas cakupan, dengan nama dan No. RM |

Keputusan teknis:

- **Cakupan 30 hari WIB** menurut `RequestedAt ?? CreateDateTime`. v1 menghitung hingga 2.000 booking yang
  kebetulan dimuat browser; angka tanpa batas waktu tidak bermakna dan membengkak.
- **`Draft` tidak dihitung** — belum dikirim ke Radiologi (`LabelStatusPesanan`: "Draf — tidak dipakai").
- **Lunas** memakai `LabPaymentClearanceRules` yang sama dengan kolom Pembayaran Daftar Pasien.
- Identitas pasien hanya dibaca untuk lima baris terbaru.
- Diletakkan di `RadOrderService` (bukan service baru) supaya tidak menambah registrasi DI dan memakai ulang
  `HariIniWibDalamUtc`.

## 2. Verifikasi

| Bukti | Hasil |
| --- | --- |
| `dotnet test` RadiologyTests | **46/46 lulus** — 5 baru di `RadDashboardTests.cs` |
| Uji baru | kartu + status 30 hari (di luar cakupan dan draf tidak dihitung); tren 30 butir dengan batas tengah malam WIB; rujukan lebih dulu; lima terbaru urut tanpa draf; nol data |

"Sekarang" pada uji dipatok 2026-10-15 03:00 UTC, sehingga hasilnya tidak bergantung pada jam uji dijalankan.

## 3. Batas verifikasi

Belum dipanggil terhadap backend yang tersambung ke devYoga. Penahannya sama dengan `BE-RAD-18`: migration
`AddRadOrderRequiresPreparation` belum diterapkan, dan penerapannya ditolak pengaman izin sesi ini (perubahan sumber
daya bersama).
