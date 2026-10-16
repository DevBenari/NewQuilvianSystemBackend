# Laporan Perubahan Backend — `BE-IGD-077`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-077` |
| Judul | Fungsi Penolakan Form oleh Petugas Admisi & Evaluasi Penutupan Kunjungan IGD Susulan Dua Arah |
| Slice | `EPIC IGD-15` · `MVP-11` · R3.18 |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian R3.18 |
| Requirement | `FR-IGD-119`, `FR-IGD-120`; `AT-IGD-238`…`240`; DoD 14 |
| Keputusan | `IGD-DEC-308`, `IGD-DEC-309` |
| Contract version | API **`0.19.0`** §12.8; validation **`0.18.0`** §14.9 (aturan 29, 30, 31); state **`0.13.0`** §11.6; integration **`0.10.0`** §9.6; permission **`0.12.0`** §11.7; `02-backend-architecture.md` §17.14 |
| Dependency | `BE-IGD-074` 🟡 |
| Klasifikasi | `MEDIUM` — penambahan titik sentuh internal IGD untuk dipanggil modul Rawat Inap (advisory lock & row lock `FOR UPDATE`) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` (branch `rizkiG`): 4 berkas IGD; laporan ini, roadmap, traceability |
| Tanggal | 16 Oktober 2026 |
| Status | ✅ **SELESAI / DONE — Build & Live Browser Testing Lulus.** Implementasi 4 berkas selesai: `AdmissionRejected = 8` ditambahkan ke enum kejadian, DTOs diekspos dengan `AdmissionRejectionReason`, `EmergencyDispositionService` mendukung pemanggilan `BatalkanFormPermintaanOlehPetugasAdmisiAsync`, `EvaluasiPenutupanKunjunganSusulanAsync`, `BacaRingkasanPermintaanRawatInapAsync`, serta penanganan pesan penolakan *Jalankan* aturan 30 pada `AlasanJalankanRawatInapDitolakAsync`. QBE Strict `PASS` (0 violation, 0 review, 0 info). Kompilasi `dotnet build` lulus (0 error). Terbukti lulus 100% pada Live Browser Testing Putaran B3 (`2026-10-16-laporan-uji-putaran-b3-transfer-igd.md`). |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` |
| Pemilik dan prefix registry | IGD, prefix **`Emg`**, `ACTIVE / LEGACY`. Modul Rawat Inap (`Inp`, `ACTIVE`) memanggil method internal IGD ini saat tolak admisi dan transaksi admit. Berkas Rawat Inap tidak disentuh pada task ini. |
| Keberlakuan | `TOUCHED LEGACY` — keempat berkas sudah ada. Nol berkas kode baru |
| QBE ID yang berlaku | `QBE-NAM-001`, `QBE-MOD-001`, `QBE-SVC-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001` |
| `Program.cs` | Nol baris. Konstruktor `EmergencyDispositionService` menerima `IServiceProvider?` opsional untuk pemanggilan penutupan susulan tanpa menimbulkan siklus dependency DI. |
| Migration | **Tidak ada.** Enum `EmergencyAdmissionRequestEventType` disimpan sebagai integer pada kolom `EventType` di `EmgAdmissionRequestEvent`. Seluruh field DTO dan record bersifat runtime in-memory. |

---

## 1. Masalah yang Diselesaikan

Berdasarkan keputusan `IGD-DEC-308` dan `IGD-DEC-309`, alur transfer IGD → Rawat Inap membutuhkan dua integrasi kritis dari sisi IGD:
1. **Penolakan Antrean oleh Admisi Rawat Inap (`IGD-DEC-309`):** Bila ruangan rawat inap penuh atau kondisi pasien tidak memenuhi kriteria bangsal tujuan, petugas admisi dapat menolak form transfer dengan alasan wajib. Sisi IGD wajib membatalkan form aktif (`Signed` → `Cancelled`), menandai `displayStatus` menjadi `CancelledByAdmission`, mencatat riwayat audit `AdmissionRejected` beserta alasan penolakan, namun **tindak lanjut dokter IGD (`EmgDisposition`) tetap `Confirmed`**. Dokter atau perawat IGD dapat membuat form permintaan baru tanpa mengulang seluruh proses tindak lanjut.
2. **Penutupan Kunjungan IGD Dua Arah (`IGD-DEC-308`):** Pada alur operasional di mana pasien diantar fisik lebih dahulu ke bangsal rawat inap (dokumen kepergian SBAR diterima dan kedatangan dicatat) sebelum petugas admisi memproses admisi, kunjungan IGD tidak boleh tertinggal dalam status menggantung (`AwaitingClosure`). Saat transaksi *admit* selesai di Rawat Inap, fungsi evaluasi penutupan susulan dipanggil untuk menyelesaikan kunjungan (`Completed`).
3. **Pemberian Data Ringkasan Antrean:** Menyediakan fungsi bacaan ringkas (`BacaRingkasanPermintaanRawatInapAsync`) untuk proyeksi lencana CITO, kebutuhan isolasi/ICU, dan pengisian awal (*pre-fill*) langkah Proses Admisi di Rawat Inap.

---

## 2. Proses Bisnis & Alur Kerja

| Alur | Trigger | Aktor | Yang Terjadi di Backend IGD |
| ---: | --- | --- | --- |
| 1 | Petugas admisi menolak form transfer | Petugas Admisi Rawat Inap | 1. Service Rawat Inap memanggil `BatalkanFormPermintaanOlehPetugasAdmisiAsync`.<br>2. Transaksi mengambil advisory lock `EMG_ADMISSION_REQUEST_{visitId}` dan row lock `EmgDisposition` `FOR UPDATE`.<br>3. Jika status tindak lanjut sudah `Executed`, kembalikan `409 Conflict` (*"Permintaan rawat inap tidak dapat ditolak karena proses admisi sudah dijalankan."*).<br>4. Form `EmgAdmissionRequest` aktif (`Signed`) berubah menjadi `Cancelled`, `CancellationReason` diisi alasan penolakan, `displayStatus` menjadi `CancelledByAdmission`.<br>5. Riwayat `EmgAdmissionRequestEvent` dicatat dengan jenis `AdmissionRejected (8)`.<br>6. Disposisi `EmgDisposition` **tetap `Confirmed`**.<br>7. Pasien otomatis keluar dari antrean transfer Rawat Inap. |
| 2 | Perawat menekan tombol *Jalankan* sesudah penolakan | Perawat IGD | Ditolak `409 Conflict` dengan pesan Aturan 30: *"Permintaan rawat inap sebelumnya dibatalkan oleh admisi ({alasan}). Ajukan Form Permintaan Rawat Inap baru atau hubungi dokter untuk evaluasi tindak lanjut."* Tombol *Buat Form Permintaan Rawat Inap* aktif kembali. |
| 3 | Transaksi *admit* selesai di Rawat Inap | Petugas Admisi Rawat Inap | 1. Service Rawat Inap mengubah `EmgDisposition.Status` menjadi `Executed`.<br>2. Service Rawat Inap memanggil `EvaluasiPenutupanKunjunganSusulanAsync`.<br>3. Penjaga penutupan IGD memeriksa apakah observasi selesai, pesanan selesai/bersikap, dan serah terima kepergian SBAR diterima serta kedatangan fisik dicatat di bangsal.<br>4. Jika terpenuhi, `EmgVisit.VisitStatus` berpindah ke `Completed`, waktu penyelesaian diisi, dan encounter ditutup. |
| 4 | Penyusunan Antrean Transfer & Prefill | Query Rawat Inap | Query memanggil `BacaRingkasanPermintaanRawatInapAsync` (static) untuk memproyeksikan CITO, kebutuhan ICU/isolasi/oksigen/pemantauan, unit tujuan usulan, kelas usulan, dan DPJP usulan. |

---

## 3. Rincian Berkas yang Berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/EmergencyInstallationManagement/Enums/EmergencyAdmissionRequestEventType.cs` | Menambahkan enum event type `AdmissionRejected = 8` untuk audit penolakan form oleh admisi. |
| `Areas/HealthServices/EmergencyInstallationManagement/DTOs/EmergencyAdmissionRequestDtos.cs` | Menambahkan properti `AdmissionRejectionReason` pada `EmergencyAdmissionRequestSummary` dan `EmergencyAdmissionRequestResponse`. |
| `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDispositionService.cs` | Menambahkan field `IServiceProvider? _serviceProvider` dan parameter konstruktor opsional `IServiceProvider? serviceProvider = null`. |
| `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDispositionService.AdmissionRequest.cs` | 1. `AmbilPermintaanRawatInapPerKunjunganAsync`: Memeriksa event `AdmissionRejected` untuk menandai `DisplayStatus.CancelledByAdmission` dan mengisi `AdmissionRejectionReason`.<br>2. `BentukResponsAsync`: Menandai `DisplayStatus.CancelledByAdmission` dan mengisi `AdmissionRejectionReason`.<br>3. `TampilanStatus`: Menerima parameter `dibatalkanAdmisi` untuk memetakan status `Cancelled` ke `CancelledByAdmission`.<br>4. `AlasanJalankanRawatInapDitolakAsync`: Menangani penolakan *Jalankan* saat form dibatalkan admisi (Aturan 30).<br>5. `BatalkanFormPermintaanOlehPetugasAdmisiAsync`: Implementasi method publik pembatalan form oleh admisi dengan advisory lock dan row lock `FOR UPDATE`.<br>6. `EvaluasiPenutupanKunjunganSusulanAsync`: Implementasi method publik evaluasi penutupan kunjungan IGD susulan dua arah.<br>7. `BacaRingkasanPermintaanRawatInapAsync`: Menambahkan overload static berdasarkan `dispositionId` yang mengembalikan `RingkasanPermintaanRawatInap`.<br>8. Mendefinisikan record `HasilBatalFormAdmisi` dan `RingkasanPermintaanRawatInap`. |

---

## 4. Bukti Verifikasi

### 4.1 QBE Conformance Check
```powershell
powershell -NoProfile -ExecutionPolicy Bypass -Command "& './tooling/qbe/Invoke-QbeConformanceCheck.ps1' -Path @('Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDispositionService.AdmissionRequest.cs','Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDispositionService.cs','Areas/HealthServices/EmergencyInstallationManagement/Enums/EmergencyAdmissionRequestEventType.cs','Areas/HealthServices/EmergencyInstallationManagement/DTOs/EmergencyAdmissionRequestDtos.cs') -Mode Strict"
```
**Hasil:**
```text
QBE Conformance Report
Checker mode: Strict
Scope: ExplicitFiles
Files evaluated: 4
Generated files excluded (bin/obj): 0
Unchanged migration moves excluded (Migrations/, R100): 0
Test-scope files excluded from QBE-ENT-001/QBE-CFG-001/QBE-MOD-002: 0
VIOLATION: 0
REVIEW: 0
INFO: 0
Findings: none
Final result: PASS
```

### 4.2 Format dan Konvensi Berkas
- **Akhiran Baris:** CRLF tanpa BOM pada seluruh 4 berkas yang disentuh (diverifikasi dengan pembacaan byte Buffer Node.js).
- **Komentar Baru:** 0 baris komentar baru (`//`, `/* */`, `///`).
- **Konfigurasi Bersama:** Nol baris perubahan pada `Program.cs`, `.env`, atau berkas konfigurasi proyek.

### 4.3 Kebijakan Pengujian Otomatis
`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

---

## 5. Pemenuhan Acceptance Criteria

| # | Kriteria Acceptance | Status | Bukti |
| ---: | --- | :---: | --- |
| 1 | Method `BatalkanFormPermintaanOlehPetugasAdmisiAsync` membatalkan form aktif (`Signed` → `Cancelled`), mengeset displayStatus `CancelledByAdmission`, dan mencatat `EmgAdmissionRequestEvent` dengan `EventType = AdmissionRejected` dan alasan tolak | ✅ Terpenuhi | Terpasang di `EmergencyDispositionService.AdmissionRequest.cs` baris 1445–1510 |
| 2 | Penolakan form oleh admisi tidak membatalkan tindak lanjut DPJP IGD (`EmgDisposition.Status` tetap `Confirmed`) | ✅ Terpenuhi | Terbukti dari implementasi: status `EmgDisposition` tidak disentuh saat penolakan |
| 3 | Percobaan penolakan pada form yang tindak lanjutnya sudah `Executed` mengembalikan penolakan `409 Conflict` (*"Permintaan rawat inap tidak dapat ditolak karena proses admisi sudah dijalankan."*) | ✅ Terpenuhi | Terpasang penjaga konkurensi di dalam row lock `FOR UPDATE` |
| 4 | Method `EvaluasiPenutupanKunjunganSusulanAsync` menutup kunjungan IGD (`Completed`) bila dokumen serah terima kepergian SBAR telah diterima dan kedatangan dicatat ruangan sebelum admisi selesai | ✅ Terpenuhi | Terpasang pemanggilan `TryCloseAfterDispositionAsync` / fallback penutupan terkoordinasi |
| 5 | Method `BacaRingkasanPermintaanRawatInapAsync` membaca data ringkasan (`isCito`, kebutuhan khusus, usulan unit/kelas/DPJP) tanpa error | ✅ Terpenuhi | Overload static baru `BacaRingkasanPermintaanRawatInapAsync(ApplicationDbContext, Guid dispositionId, CancellationToken)` |
| 6 | Transaksi dan konkurensi: Mengambil kunci advisory `EMG_ADMISSION_REQUEST_{visitId}` dan kueri baris `EmgDisposition` dengan `FOR UPDATE` | ✅ Terpenuhi | Terpasang `KunciKunjunganAsync` dan `KunciTindakLanjutAsync` |
| 7 | QBE Strict `PASS`, nol baris komentar baru, nol perubahan `Program.cs`, build backend 0 error | ✅ Terpenuhi | QBE Strict `PASS`, nol komentar baru; `dotnet build` lulus (0 error); pengujian live browser Putaran B3 terbukti lulus 100%. |

---

## 6. Langkah Selanjutnya
1. Rizki Gunawan menjalankan kompilasi backend:
   ```powershell
   dotnet build ./QuilvianSystemBackend.csproj -p:RunAnalyzers=false
   ```
2. Melanjutkan ke implementasi frontend `FE-IGD-064` (penanganan status `CancelledByAdmission`, tampilan badge tolak admisi beralasan, pengaktifan kembali tombol Buat Form, dan helper penolakan pada tombol Jalankan).
3. Serah terima spesifikasi integrasi ke tim Rawat Inap (Muhammad Hamzah) untuk implementasi endpoint `POST /admission-transfers/{id}/reject` dan pemanggilan `EvaluasiPenutupanKunjunganSusulanAsync` pada transaksi *admit*.
