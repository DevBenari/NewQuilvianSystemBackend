# Laporan Perubahan Backend — `BE-RWI-152`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-152` |
| Judul | Satu pembaca status kasir |
| Slice | `MVP-1` / `RWF-W1` |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-152` |
| Trace | `FR-RWF-002`, `FR-RWF-003`, `FR-RWF-004`; `RWI-DEC-167`; `INT-RWF-02`, `INT-RWF-03` |
| Contract version | `integrasi-billing` `1.1.0` **`approved`** (`RWI-DEC-221`): API 3.1 (`billing-details`, tulis `financial-clearance` dihapus), 3.3 (`billing-status`); integrasi 4.3 |
| Dependency | — |
| Klasifikasi | `MEDIUM` — skor 8: repository 0, berkas diperiksa 1, berkas diubah 1, logika 1, kontrak API 2, database 0, keamanan 2, workflow 1 |
| Task mode | `BACKEND` (dependency `BE-RWI-153`, disetujui pengguna 5 Oktober 2026) |
| Target tulis | `Areas/HealthServices/InPatientManagement/**` |
| Model | Codex (implementasi awal); Claude Opus 5.5 (review, build akhir, laporan) |
| Commit backend saat dikerjakan | `f32b2308` (branch `MHamzah`), working tree |
| Tanggal | 5 Oktober 2026 |
| Status | ✅ Selesai. Keempat acceptance criteria terpetakan ke source; build akhir `PASS` |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement`; membaca Billing hanya lewat `IInpBillingClearanceAdapter` (`RWI-DEC-102` butir e) |
| Prefix registry | `Inp` — `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001` |
| Wewenang | Source: ya. Tidak ada perubahan skema. Deployment: tidak |
| Branch | `MHamzah`; tidak ada operasi Git tulis |

## 1. Masalah yang diperbaiki

Rawat Inap punya dua sumber status kasir: salinan lokal di episode yang diisi lewat penandaan manual (`POST financial-clearance`), dan endpoint `billing-details` yang menampilkan nominal rupiah dengan pemeriksaan nama peran di kode (`User.IsInRole("Kasir")`). Keduanya bisa berbeda dari keputusan Billing yang sebenarnya, dan perawat dapat melihat rupiah yang tidak boleh dilihatnya.

## 2. Proses bisnis

**Tujuan.** Status kasir di Rawat Inap selalu dibaca langsung dari Billing, tanpa salinan dan tanpa rupiah (`RWI-DEC-167`).

**Pelaku.** Perawat, admisi, dan petugas lain pemegang `InpatientBillingOperational : Read`.

**Langkah utama.**

1. Layar meminta `billing-status` untuk satu episode.
2. Service mengambil kunjungan episode, lalu memanggil adapter status kasir.
3. Adapter membaca keputusan terbaru Billing (`IInpatientClearanceService.GetLatestStatusAsync`): status, daftar kendala tanpa nominal, waktu evaluasi, dan status invoice.
4. Bila Billing gagal dibaca, adapter mengembalikan `IsReadable = false` tanpa status; status itu tidak pernah dianggap `CLEARED`.

**Contoh.** Billing menahan izin karena masih ada resep belum diserahkan. Layar perawat menampilkan status `BLOCKED` dengan kendala "Resep belum diserahkan", tanpa angka rupiah. Bila server Billing sedang bermasalah, layar menampilkan "tidak dapat dibaca", dan penutupan episode ikut terkunci (`BE-RWI-153`).

**Aturan bisnis.**

| Aturan | Akibat |
| --- | --- |
| Billing tidak terbaca | `IsReadable = false`, `ClearanceStatus = null`; tidak pernah `CLEARED` |
| Penandaan keuangan manual | Tidak lagi dapat ditulis; riwayat lama tetap terbaca |
| Rupiah | Tidak ada pada respons `billing-status` |

**Hasil akhir.** Satu sumber status kasir; tanda keuangan manual menjadi riwayat baca saja.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

API 3.1 dan 3.3, integrasi 4.3, `IInpBillingClearanceAdapter`/`InpBillingClearanceAdapter` (dibuat `BE-RWI-154`), `InpatientBillingQueryService`, `InpatientBillingOperationalController`, `InpatientDischargeController`, `InpDischargeService.Closure.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpatientBillingQueryService.cs` | Ditulis ulang: `GetOperationalBillingStatusAsync` membaca lewat adapter; `GetFinancialDetailsAsync` dihapus |
| `Areas/HealthServices/InPatientManagement/Services/IInpatientBillingQueryService.cs` | Kontrak `GetFinancialDetailsAsync` dihapus |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientBillingSummaryDtos.cs` | `InpatientBillingStatusResponseDto` tanpa rupiah; DTO rincian finansial dihapus |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientBillingOperationalController.cs` | `GET billing-details` (dengan `IsInRole`) dihapus |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeController.cs` | `POST financial-clearance` dihapus; `GET financial-clearance` tetap |
| `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs` | `MarkFinancialClearanceAsync` dihapus |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Breaking sesuai kontrak:** `GET billing-details` dan `POST financial-clearance` dihapus; bentuk respons `billing-status` mengikuti API 3.3 |
| Database | `NOT APPLICABLE` — kolom salinan lama tidak lagi dibaca atau ditulis; tidak ada perubahan skema |
| Keamanan/Auth | Pemeriksaan `IsInRole("SuperAdmin"/"Billing"/"Kasir")` pada `billing-details` hilang bersama endpoint-nya; kemampuan `ViewBillingDetails` dan `MarkFinancialClearance` tidak lagi dideklarasikan |

## 4. Dokumentasi endpoint

#### Inpatient Billing Operational

Base URL: `api/v1/health-services/inpatient-management/episodes`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/{episodeId}/billing-status` | Status kasir dan kendala tanpa rupiah, dibaca langsung dari Billing | `InpatientBillingOperational : Read` | — | `InpatientBillingStatusResponseDto` |

Kode status: `200` status terbaca atau "tidak dapat dibaca" (`IsReadable = false`); `403` tidak berhak; `404` episode tidak ditemukan.

#### Health Services / Inpatient Management / Inpatient Discharge

Base URL: `api/v1/health-services/inpatient-management/discharges`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/{episodeId}/financial-clearance` | Riwayat tanda keuangan manual, baca saja | `InpatientDischarge : ReadFinancialClearance` | — | `FinancialClearanceResponse` |

**Endpoint yang dihapus:** `GET …/episodes/{episodeId}/billing-details`, `POST …/discharges/{episodeId}/financial-clearance`.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -v minimal` — build akhir | `Build succeeded`, `0 Error(s)`, `233 Warning(s)` | `PASS` | Log build akhir |
| Pemeriksaan statis atribut hak akses | `GetBillingStatus` cocok `InpatientBillingOperational` ↔ `Read`; `GetFinancialClearance` cocok `InpatientDischarge` ↔ `ReadFinancialClearance` | `PASS` | Skrip pemeriksa atribut |
| Verifikasi proses bisnis — Billing gagal dibaca | Adapter menangkap pengecualian dan mengembalikan `IsReadable = false` tanpa status | `PASS` | `InpBillingClearanceAdapter.GetStatusAsync` |
| Pencarian endpoint yang dihapus | Tidak ada deklarasi `billing-details` maupun `HttpPost` `financial-clearance` | `PASS` | Pencarian source |
| Uji API runtime | Tidak dijalankan | `NOT RUN` | Dikecualikan atas instruksi pengguna 5 Oktober 2026 |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` pada sesi ini (aplikasi tidak dijalankan).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `billing-status` memuat status dan daftar kendala tanpa rupiah dari Billing | Terpenuhi | `InpatientBillingQueryService.GetOperationalBillingStatusAsync`; DTO tanpa field nominal |
| 2. Billing tidak terbaca → status "tidak dapat dibaca", tidak pernah dianggap `CLEARED` | Terpenuhi | `InpBillingClearanceAdapter` (`IsReadable = false`) |
| 3. `billing-details` dan `POST financial-clearance` tidak terdaftar | Terpenuhi | Kedua action dihapus |
| 4. Riwayat `financial-clearance` tetap terbaca | Terpenuhi | `GET {episodeId}/financial-clearance` tetap |
| DoD: build tanpa error; laporan; roadmap dan traceability | Terpenuhi | Bagian 5 |
| DoD: verifikasi API runtime jalur gagal-tertutup | **Dikecualikan atas instruksi pengguna 5 Oktober 2026** | `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Layar `FE-INP-08` lama yang masih menulis tanda keuangan akan mendapat `404`; `FE-RWI-166` menjadikannya baca saja |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Belum diuji lewat HTTP |
| Perubahan sampingan | `NONE` |
| Interupsi | Sesi Codex terhenti karena batas penggunaan penyedia; dilanjutkan Claude |
| Status Git | 151 entri `git status --short` (63 `M`, 3 `D`, 85 `??`); berkas task ini pada 3.2; tidak ada stage/commit/push |
| Langkah berikutnya | `FE-RWI-166`, `FE-RWI-168` |
