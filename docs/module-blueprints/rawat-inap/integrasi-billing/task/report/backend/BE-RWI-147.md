# Laporan Perubahan Backend — `BE-RWI-147`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-147` |
| Judul | Tarif kamar satu jalur dan label `RANAP` seragam |
| Slice | `MVP-0` / `RWF-W0` |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-147` |
| Trace | `FR-RWF-013`, `018`; `RWI-DEC-192` butir (b) dan (e); `FIN-CON-01` |
| Contract version | `1.1.0` **`approved`** (`RWI-DEC-221`) |
| Dependency | — |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, diperiksa 1, diubah 2, logika 1, kontrak API 1, database 0, keamanan 0, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/BillingManagement/Billing/**` |
| Model | Claude Opus 5.5 |
| Commit backend | Kode task ini sudah di-commit pemilik pada `e2ded614` (branch `MHamzah`, 2 Oktober 2026) |
| Tanggal | Kode 2 Oktober 2026; laporan 5 Oktober 2026 |
| Status | ✅ Implementasi kode selesai. `dotnet build` **dikecualikan atas keputusan pengguna 2 Oktober 2026**. Tiga berkas mati belum terhapus (lihat bagian 7) |
| Catatan wewenang | Header roadmap masih `DRAFT`; task dikerjakan atas instruksi eksplisit pengguna 2 Oktober 2026 ("Kerjakan juga prasyarat IB") |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `BillingManagement` / `Billing` |
| Prefix registry | `Bil` — `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-PERM-001` (endpoint dicabut) |
| Wewenang | Source: ya. Database: tidak. Menghapus berkas: **ditolak** pengaman sesi |

---

## 1. Masalah yang diperbaiki

Tarif kamar dapat dihitung lewat dua jalur: `BillingCalculationService` dari penempatan bed, dan
`InpatientRoomChargeCalculationService` lewat `POST invoices/occupancy-charges`. Selain itu sebagian
pembaca invoice rawat inap mencari label `"INPATIENT"`, padahal invoice dibuka dengan label
`"RANAP"`, sehingga tagihan susulan sesudah izin `CLEARED` tidak menemukan invoicenya.

## 2. Proses bisnis

1. Tarif kamar hanya dihitung `BillingCalculationService` dari linimasa `InpBedPlacement` setiap
   hitung ulang invoice.
2. Contoh hitungan dari source: pasien kelas 2 menempati satu bed tiga hari → satu segmen tarif
   kamar (`ROOM_CHARGE`) sebesar 3 unit × tarif kelas 2 pada invoice `RANAP`. Jalur kedua yang dulu
   dapat menambah baris kamar tersendiri sudah tidak terdaftar, sehingga tidak ada baris kedua.
3. Seluruh pembaca invoice rawat inap membandingkan label `RANAP`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`InpatientClearanceController.cs`, `InpatientRoomChargeCalculationService.cs` dan interface-nya,
`InpatientRoomChargeDtos.cs`, `BillingCalculationService.cs`, `BillingInvoiceService.cs`
(`MapServiceType`), `InpatientClearanceService.cs`, `BilConsumerHandoffService.cs`,
`BillingManagementServiceCollectionExtensions.cs`; penelusuran seluruh pemanggil.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Billing/Controllers/InpatientClearanceController.cs` | Endpoint `POST invoices/occupancy-charges` dan dependensinya dicabut |
| `Billing/BillingManagementServiceCollectionExtensions.cs` | Registrasi `IInpatientRoomChargeCalculationService` dicabut |
| `Billing/Services/InpatientClearanceService.cs` | Label `"INPATIENT"` → `"RANAP"` |
| `Billing/Services/BilConsumerHandoffService.cs` | Label `"INPATIENT"` → `"RANAP"` |
| `Billing/Services/BillingInvoiceService.cs` | Dua pembanding label → `"RANAP"` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `POST …/invoices/occupancy-charges` dihapus (API 3.1) |
| Database | `NOT APPLICABLE`. Invoice lama berlabel `INPATIENT` (bila ada di lingkungan uji) **tidak** dimigrasi |
| Keamanan/Auth | Satu pintu tulis dicabut; tidak ada permission baru |

## 4. Dokumentasi endpoint

| Method | Path | Perubahan |
| --- | --- | --- |
| `POST` | `api/v1/health-services/billing-management/billing/invoices/occupancy-charges` | **Dihapus** |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Penelusuran pemanggil `InpatientRoomChargeCalculationService` | Tidak ada pemanggil; tidak terdaftar di DI | `PASS` | Pencarian source 5 Oktober 2026 |
| Penelusuran label `"INPATIENT"` sebagai `ServiceType` invoice | Tidak ada; sisa `"INPATIENT"` adalah nama domain sumber outbox/handoff, bukan label invoice | `PASS` | Pencarian source |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas keputusan pengguna 2 Oktober 2026 |
| Verifikasi hitungan tarif kamar dengan data | Belum dijalankan | `NOT RUN` | Butuh build dan database |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` — belum ada build dan database termigrasi pada sesi ini.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `POST …/invoices/occupancy-charges` tidak terdaftar | Terpenuhi (source) | Action dihapus dari controller |
| 2. Tidak ada pemanggil service lama | Terpenuhi (source) | Registrasi DI dicabut; tidak ada referensi selain berkasnya sendiri |
| 3. Seluruh pembaca invoice rawat inap memakai `RANAP` | Terpenuhi (source) | Empat titik pembanding diubah |
| 4. Kamar kelas 2 tiga hari muncul satu kali | Terpenuhi (source) | Satu-satunya jalur hitung `CalculateRoomChargeAsync` |
| DoD build | Dikecualikan atas keputusan pengguna 2 Oktober 2026 | — |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Tiga berkas mati masih ada** karena penghapusan berkas ditolak pengaman sesi: `Billing/Services/InpatientRoomChargeCalculationService.cs`, `Billing/Services/IInpatientRoomChargeCalculationService.cs`, `Billing/Dtos/InpatientRoomChargeDtos.cs`. Tidak terdaftar dan tidak dipanggil; **pemilik perlu menghapusnya** |
| Masalah yang diketahui | Invoice lama berlabel `INPATIENT` di lingkungan uji tidak dimigrasi (temuan, butuh wewenang tersendiri) |
| Risiko tersisa | `NONE` di luar build |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Kode di `e2ded614`; laporan ini `??` |
| Langkah berikutnya | Hapus tiga berkas mati; build |
