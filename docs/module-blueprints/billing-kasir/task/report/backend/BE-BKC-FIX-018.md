# Laporan Perubahan Backend — `BE-BKC-FIX-018`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-FIX-018` |
| Judul | Fakta tindakan Bank Darah diteruskan ke invoice kasir |
| Sumber | [`PLAN-REPAIR-BKC-001`](../../../docs/plan-repair/plan-repair-001-integrasi-checkout-ke-billing-kasir.md) `FIX-BKC-001-04`; temuan `ISS-BKC-001-03` |
| Keputusan | `BKC-DEC-140` (K-06; rekomendasi plan-repair atas fakta `DEC-BD-016`) |
| Target tulis | `NewQuilvianSystemBackend` (sisi Billing saja; producer Bank Darah tidak diubah) |
| Commit saat dikerjakan | `d9089882` (`MHamzah`) + working tree; belum di-commit |
| Tanggal | 2026-10-12 |
| Status | ✅ **Source selesai**, build 0 error, uji resolver lulus. Uji transaksi `NOT RUN` |

## 1. Masalah

Tindakan Bank Darah yang selesai menerbitkan satu fakta `BloodBank`/`BloodBankCharge` dan diterima folio, tetapi `IsCandidateSourceContext` tidak memuat `BloodBank`, sehingga efeknya ditandai `NotApplicable` dan tidak pernah menjadi item invoice. Petugas melihat "fakta diterima" sementara total kasir tidak bertambah.

## 2. Proses bisnis sesudah perbaikan

1. Petugas Bank Darah menyelesaikan tindakan (`Recorded` → `Completed`); producer yang sudah ada menerbitkan **satu** fakta per tindakan, berapa pun kantongnya.
2. Folio menandai efeknya `Pending` (kini kandidat), jembatan meneruskannya ke invoice kunjungan order darah sebagai `BLOOD_BANK`/`COMPLETED`, satu baris per tindakan (`SourceDetailId` = id tindakan).
3. Harga dibaca ulang dari `MstTariff` yang berlaku pada waktu selesai — tarif yang dirujuk tindakan (`TariffId`), lalu tarif tindakan rujukannya (`ProcedureRefId`) — bukan dari `TariffAmountSnapshot`, sama seperti sumber lain. Tarif tidak berlaku → `TARIFF_NOT_FOUND` di antrean rekonsiliasi.
4. Koreksi pemberian kantong tidak membalik biaya (`DEC-BD-034`); peninjauan finansialnya lewat adjustment Billing. Tidak ada void normal.

> **Contoh.** Transfusi dengan dua kantong, tarif tindakan Rp350.000 → satu item Rp350.000. Kirim ulang fakta yang sama tidak menambah item kedua (kunci deterministik per fakta-versi).

## 3. Perubahan

| Berkas | Perubahan |
| --- | --- |
| `BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.cs` | `BloodBank` menjadi kandidat; domain `BLOOD_BANK`; status tagih `COMPLETED` |
| `BillingManagement/Billing/Services/BillingSourceTariffResolver.cs` | `ResolveBloodBankAsync` (tindakan + order darah, kelas pasien tindakan, `FindTariffAsync`) |
| `BillingManagement/Billing/Services/BillingChargeSourceAdapter.cs` | Kebijakan `BLOOD_BANK`: tagih `COMPLETED`, tanpa void normal, status batal `CANCELLED`/`VOIDED` |

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak berubah (`POST /blood-bank-procedures/{id}/complete` tetap) |
| Database | Tidak ada migration |
| Keamanan | Tidak berubah |

## 4. Verifikasi

| Perintah | Hasil |
| --- | --- |
| `dotnet build` | **0 error**, warning baru 0 |
| Uji (`CheckoutBillingIntegrationTests`) | **Lulus**: `Tindakan_Bank_Darah_satu_baris_per_tindakan_dari_tarif_berlaku` (harga dari `MstTariff`, bukan salinan klinis), `Domain_baru_diterima_validator_Billing_dan_dianggap_selesai("BLOOD_BANK","COMPLETED")` |
| Uji fakta Bank Darah → invoice di PostgreSQL (skenario 8 plan-repair) | `NOT RUN` |

## 5. Acceptance criteria (`FIX-BKC-001-04`)

| AC | Bukti |
| --- | --- |
| 1. Dua kantong tetap satu fakta per tindakan; nilai sesuai tarif | Producer tidak diubah; resolver kuantitas 1 per tindakan |
| 2. Replay tidak membuat item kedua; tarif gagal tampil sebagai pengecualian | Kunci deterministik jembatan; `TARIFF_NOT_FOUND` → antrean rekonsiliasi + penahan finalisasi `BIL-FIN-021` |
| 3. "Masuk invoice" hanya bila item terbentuk | Status efek `Synced` hanya setelah upsert berhasil; `GetChargeStateAsync` membedakan `NOT_RECEIVED`/`WAITING_FOR_BILLING`/`IN_INVOICE` |

## 6. Risiko dan catatan

- **Fakta Bank Darah lama** (sebelum perubahan ini) tetap `NotApplicable`; tidak diproses ulang otomatis. Backfill memerlukan otorisasi data terpisah.
- Layar Bank Darah masih menampilkan keadaan penyerahan fakta; penanda "masuk invoice" di layar Bank Darah belum dibuat (dapat memakai `GetChargeStateAsync` bila diminta pemilik Bank Darah).
