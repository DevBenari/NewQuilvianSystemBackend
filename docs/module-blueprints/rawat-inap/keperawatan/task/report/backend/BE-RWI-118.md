# Laporan Perubahan Backend — `BE-RWI-118`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-118` |
| Judul | Penghentian butir dan penutupan episode membatalkan dosis |
| Slice | Gelombang 3 — `KEP-V2-2` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-118` |
| Trace | `FR-KEP-069`; `INT-KEP-09`, `INT-KEP-15`; `INT-DOK-16`; state matrix 5.5; `VAL-INP-16` |
| Contract version | `0.5.0` |
| Dependency | `BE-RWI-114` ✅ 17 September 2026 |
| Klasifikasi | `HEAVY` — penulisan di dalam transaksi pemicu milik sub-modul lain |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/PharmacyManagement`; titik pasang langkah 6 `Areas/HealthServices/InPatientManagement/Services/InpDischargeService*.cs` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `fe7e60d4` (branch `MHamzah`) |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — empat kriteria terpetakan sebagai **sisi keperawatan**; jalur penutupan terpasang; pemanggilan dari aksi penghentian butir milik `BE-RWI-100` [BE-DOK]; `dotnet build` dan uji galat buatan **NOT RUN** atas keputusan pemilik |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices / PharmacyManagement`; pemanggil `HealthServices / InPatientManagement` |
| Registry / prefix | `Phm` — `ACTIVE`; `Inp` — `ACTIVE` |
| Keberlakuan | `NEW CODE` (metode mesin) + `TOUCHED LEGACY` (`InpDischargeService`) |
| QBE relevan | `QBE-SVC-001`, `QBE-DEL-001` (pembatalan beralasan, bukan hapus), `QBE-TXN-001` |
| Database | Nol migration |

## 1. Masalah yang diperbaiki

Dosis `Due` yang sudah terbentuk tetap tampil setelah dokter menghentikan obat atau setelah pasien pulang. Perawat dapat
mencatat obat yang sudah dihentikan — atau memberi obat kepada pasien yang sudah tidak ada di ruangan.

## 2. Proses bisnis

1. **Penutupan episode (`INT-KEP-15`).** Joko ditutup 14.00 → di dalam transaksi `CloseEpisodeInternalAsync`, langkah 6
   memanggil `CancelFutureDosesForEpisodeAsync(episodeId, closedAt)`: dosis 20.00 `Due` → `Cancelled` "perawatan ditutup".
   08.00 `Administered` dan 12.00 `Missed` tidak disentuh; 13.00 yang belum dicatat tetap `Due`, hanya-baca, tampil
   "Tidak dicatat sebelum perawatan ditutup". `sideEffects.cancelledFutureDoseCount` memuat angka yang benar-benar
   tersimpan; `notYetWiredSteps` tidak lagi memuat langkah 6.
2. Peringatan sebelum menutup (`VAL-INP-16`): `UNRECORDED_PAST_DOSES` kini terukur (`isMeasured = true`) dari
   `CountUnrecordedPastDosesAsync` — tetap peringatan, tidak menahan (`RWI-DEC-129`).
3. **Penghentian butir (`INT-KEP-09`).** `CancelDueDosesForItemAsync(itemId, stoppedAt)`: Ceftriaxone dihentikan 09.10 →
   dosis `Due` 20.00, termasuk yang `Pending` cek ganda, → `Cancelled` "resep dihentikan". Metode ini disiapkan untuk
   dipanggil `PATCH prescriptions/items/{itemId}/stop` milik `BE-RWI-100` di dalam transaksinya.
4. **Jaring pengaman.** Selama `BE-RWI-100` belum mendarat — atau bila butir dihentikan lewat jalur lain — setiap
   pembentukan dosis (MAR dibuka, hosted service 15 menit) membatalkan dosis `Due` butir terhenti yang jadwalnya ≥ waktu
   henti; pencatatan atas dosis itu juga ditolak `409` (`VAL-KEP-30e`).
5. **Galat buatan:** kedua metode tidak membuka transaksi dan tidak memanggil `SaveChanges`; galat pada langkah mana pun
   di transaksi pemicu → seluruhnya batal, nol dosis berubah. Menjalankan ulang tidak mengubah apa pun.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`InpDischargeService.cs`, `InpDischargeService.Closure.cs`, `PatientProcedureOrderService.CancelPendingOrdersForClosureAsync`
(pola langkah 5), integration contract 8.3 dan 8.9, `dokter-rawat-inap` integration 12.6, kartu `BE-RWI-087` dan `BE-RWI-100`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PharmacyManagement/Services/MedicationAdministrationService.Cancellation.cs` | Baru — `CancelDueDosesForItemAsync`, `CancelFutureDosesForEpisodeAsync`, `CountUnrecordedPastDosesAsync`, jaring pengaman |
| `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.cs` | Injeksi `MedicationAdministrationService` beserta alasannya |
| `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs` | Langkah 6 terpasang di dalam transaksi; `CancelledFutureDoseCount` dari hasil nyata; peringatan `UnrecordedPastDoses` terukur |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Kontrak `episode-rawat-inap` tidak berubah bentuk; nilai `cancelledFutureDoseCount` dan peringatan dosis kini terisi |
| Database | `NOT APPLICABLE` |
| Keamanan/Auth | `NOT APPLICABLE` |

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — nol endpoint baru. Perilaku endpoint milik `episode-rawat-inap` pada
`api/v1/health-services/inpatient-management/discharges` berubah: `POST /{episodeId}/close` dan
`POST /{episodeId}/close-with-override` bertambah langkah 6; `GET /{episodeId}/closure-readiness` membawa peringatan dosis
yang kini terukur.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Review diff `InpDischargeService*` | Langkah 6 di antara langkah 5 dan perubahan status, sebelum `SaveChanges`/`Commit` | `PASS` | Diff |
| Pemeriksaan siklus dependency injection | `InpDischargeService` → `MedicationAdministrationService` → (penjaga, konteks klinis, Pengawasan Harian); tidak ada yang menunjuk balik | `PASS` | Review konstruktor |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Keputusan pemilik 17 September 2026 |
| Verifikasi proses bisnis dan uji galat buatan | Tidak dijalankan | `NOT RUN` | Menunggu build dan Postgres sekali pakai |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Penghentian butir membatalkan dosis `Due` sesudahnya | Terpenuhi — sisi keperawatan | `CancelDueDosesForItemAsync` + jaring pengaman pembentukan dosis + `VAL-KEP-30e`. Pemanggilan dari aksi henti = `BE-RWI-100` [BE-DOK] kriteria 2 |
| 2. Penutupan episode membatalkan dosis `Due` masa depan | Terpenuhi | Langkah 6 `CloseEpisodeInternalAsync` |
| 3. `Administered`/`Held`/`Refused`/`Missed` tidak disentuh | Terpenuhi | Filter `DoseStatus == Due` pada ketiga jalur |
| 4. Di dalam transaksi pemicu; galat → nol perubahan | Terpenuhi pada jalur penutupan; metode henti tanpa transaksi sendiri | Nol `SaveChanges`/`BeginTransaction` pada metode mesin |
| DoD: `dotnet build`, verifikasi proses bisnis, uji galat buatan | **Dikecualikan atas keputusan pemilik 17 September 2026** | `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Task ini menyentuh berkas `episode-rawat-inap` — titik pasang langkah 6 yang disiapkan `BE-RWI-087`. Status `BE-RWI-087` dan `BE-RWI-084` (peringatan dan angka akibat) pada roadmap `episode-rawat-inap` **tidak** diubah di sini; pemiliknya perlu memverifikasi dan memperbarui status keduanya |
| Masalah yang diketahui | Jaring pengaman berjalan di luar transaksi penghentian; jalur satu-transaksi baru lengkap setelah `BE-RWI-100` memanggil `CancelDueDosesForItemAsync` |
| Risiko tersisa | Build belum dijalankan |
| Perubahan sampingan | Konstanta `LangkahEnamBelumTerpasang` dibiarkan untuk kompatibilitas, tidak lagi dipakai |
| Interupsi | `NONE` |
| Status Git | `M` `InpDischargeService.cs`, `InpDischargeService.Closure.cs`; berkas baru `??` |
| Langkah berikutnya | `BE-RWI-100` [BE-DOK], verifikasi ulang `BE-RWI-087` dan `BE-RWI-084` [BE-INP] |
