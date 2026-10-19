# Laporan Perubahan Backend — `BE-IGD-078`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-078` |
| Judul | Perbaikan Transisi Status Alur Non-Ranap & Sequential Promotion Disposisi IGD |
| Slice | `EPIC IGD-12` · Penanganan & Disposisi Non-Ranap IGD |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian R3.18 |
| Requirement | `FR-IGD-055`, `FR-IGD-056`; `AT-IGD-120` |
| Keputusan | `IGD-DEC-311` |
| Contract version | API **`0.19.0`** §12.1; validation **`0.18.0`**; state **`0.14.0`** §11.7; `02-backend-architecture.md` §17.15 |
| Dependency | `BE-IGD-070` ✅, `BE-IGD-074` 🟡 |
| Klasifikasi | `MEDIUM` — perbaikan alur transisi status kunjungan dan eliminasi error 409 Conflict pada disposisi pulang |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` (branch `rizkiG`): 2 berkas kode, blueprint architecture, contracts, roadmap, traceability |
| Tanggal | 16 Oktober 2026 |
| Status | ✅ **DONE — source selesai, build 0 error, QBE Strict PASS.** Implementasi pada `EmergencyDispositionController` dan `EmergencyDispositionService.AdmissionRequest` berhasil mengeliminasi penolakan HTTP 409 Conflict saat perawat menekan tombol *Jalankan* pada tindak lanjut Pulang / Non-Ranap melalui promosi berurutan (*sequential promotion*) ke `AwaitingDisposition` sebelum `Disposed`. Build backend `dotnet build -p:RunAnalyzers=false` lulus 0 error dan QBE Strict PASS (0 violation). |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` |
| Pemilik dan prefix registry | IGD, prefix **`Emg`**, `ACTIVE / LEGACY`. |
| Keberlakuan | `TOUCHED LEGACY` — kedua berkas sudah ada. Nol berkas kode baru |
| QBE ID yang berlaku | `QBE-NAM-001`, `QBE-MOD-001`, `QBE-SVC-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001` |
| `Program.cs` | Nol baris perubahan. |
| Migration | **Tidak ada migration baru.** Seluruh perubahan menggunakan state engine dan enum eksisting `EmergencyVisitStatus`. |

---

## 1. Masalah yang Diselesaikan

Pada saat pengujian alur operasional IGD non-ranap (pasien pulang / kontrol rawat jalan / rujuk balik):
1. **Penolakan HTTP 409 Conflict:** Ketika perawat menekan tombol *Jalankan* pada tindak lanjut yang berstatus Pulang, backend merespons:
   `Status 409 Conflict: "Status kunjungan tidak dapat berubah dari Triaged ke Disposed"` (atau dari `InTreatment` ke `Disposed`).
2. **Penyebab:** Matriks transisi status resmi IGD (`state-transition-matrix.md` Bagian 1) dan fungsi `EmergencyVisitService.CanTransition` hanya mengizinkan transisi ke `Disposed` bila kunjungan berasal dari status `AwaitingDisposition` (6). Pada pasien non-ranap yang tidak memerlukan observasi, tidak ada aksi yang mempromosikan status kunjungan ke `AwaitingDisposition`. Akibatnya, pasien tertahan di `Triaged` atau `InTreatment` hingga gagal dieksekusi.
3. **Penyelesaian:**
   - Saat dokter mengonfirmasi tindak lanjut (`Confirmed`), status kunjungan yang masih berada pada `Triaged` atau `InTreatment` secara otomatis dipromosikan ke `AwaitingDisposition` via `_emergencyVisitService.TryApplyVisitStatus`.
   - Sebagai jaring pengaman tambahan, saat perawat menekan *Jalankan* (`Executed`), backend melakukan *sequential promotion* bila kunjungan masih `Triaged`/`InTreatment`, memindahkannya terlebih dahulu ke `AwaitingDisposition` sebelum menerapkan `Disposed`, sehingga seluruh rantai transisi sah dan lolos validasi `CanTransition`.

---

## 2. Rincian Berkas yang Berubah

| Berkas | Perubahan Teknis |
| :--- | :--- |
| `Areas/HealthServices/EmergencyInstallationManagement/Controllers/EmergencyDispositionController.cs` | 1. Pada method `UpdateDispositionStatus`: pindahkan inisialisasi `now` dan `actorUserId` ke awal sebelum pemeriksaan status `Confirmed`.<br>2. Pada blok `request.DispositionStatus == EmergencyDispositionStatus.Confirmed`: jika `visit.VisitStatus` adalah `Triaged` atau `InTreatment`, panggil `_emergencyVisitService.TryApplyVisitStatus(visit, EmergencyVisitStatus.AwaitingDisposition, actorUserId, now, out _)`.<br>3. Pada blok `request.DispositionStatus == EmergencyDispositionStatus.Executed`: tambahkan jaring pengaman berurutan (*sequential promotion*) dari `Triaged`/`InTreatment` $\rightarrow$ `AwaitingDisposition` $\rightarrow$ `Disposed`.<br>4. Pada method `Create`: terapkan promosi serupa bila disposisi dibuat langsung berstatus `Confirmed`. |
| `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDispositionService.AdmissionRequest.cs` | 1. Tambahkan `using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;` agar enum `EncounterStatus` dikenali.<br>2. Ganti nama method duplikasi CS0111 menjadi `BacaRingkasanPermintaanRawatInapPerDisposisiAsync`.<br>3. Gunakan properti `encounter.CompletedAt = now` pada `EvaluasiPenutupanKunjunganSusulanAsync` sesuai model `RegPatientEncounter`. |

---

## 3. Bukti Verifikasi

### 3.1 Kompilasi Backend C#
```powershell
dotnet build -p:RunAnalyzers=false
```
**Hasil:**
```text
Build succeeded.
    286 Warning(s)
    0 Error(s)
Time Elapsed 00:01:51.85
```

### 3.2 QBE Strict Conformance Check
```powershell
powershell -ExecutionPolicy Bypass -File tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict
```
**Hasil:**
```text
QBE Conformance Report
Checker mode: Strict
Scope: WorkingTree
Files evaluated: 5
Generated files excluded (bin/obj): 0
Unchanged migration moves excluded (Migrations/, R100): 0
Test-scope files excluded from QBE-ENT-001/QBE-CFG-001/QBE-MOD-002: 0
VIOLATION: 0
REVIEW: 0
INFO: 0
Findings: none
Final result: PASS
```

### 3.3 Bukti Live Browser Testing Antarmuka (Playwright Headed Chromium)
- **Skrip Pengujian**: `QuilvianSystemFrontendDev/test-with-agy/scripts/test-live-non-ranap-sequential-intreatment.mjs`
- **Laporan Resmi**: [`docs/module-blueprints/igd/perawat-igd/testing/test-by-agy/laporan-live-testing-non-ranap-dan-intreatment.md`](../../perawat-igd/testing/test-by-agy/laporan-live-testing-non-ranap-dan-intreatment.md)
- **Pasien Uji Independen**: Dwi Agung Baskara (No. RM `00-00-02-26`, No. Kunjungan `IGD-261002070605-923E47`)
- **Tangkapan Layar Bukti**:
  - `07_tab_tindak_lanjut_perawat.png` (Disposisi Pulang berstatus Terkonfirmasi)
  - `08_modal_konfirmasi_jalankan.png` (Modal konfirmasi eksekusi tindak lanjut perawat)
  - `09_hasil_eksekusi_disposisi.png` (Eksekusi berhasil via `PATCH /disposition-status`, HTTP 200 OK bebas 409)
  - `10_kunjungan_selesai_completed.png` (Status kunjungan terbukti berubah menjadi `Completed`)
- **Hasil**: **100% PASS** — Sequential promotion terbukti sukses mengeliminasi penolakan HTTP 409 Conflict.

---

## 4. Kesimpulan & Status Acceptance

Seluruh acceptance criteria untuk `BE-IGD-078` terpenuhi 100%:
1. Transisi `Triaged`/`InTreatment` $\rightarrow$ `AwaitingDisposition` terpasang saat dokter mengonfirmasi tindak lanjut.
2. Jaring pengaman berurutan (*sequential promotion*) saat perawat mengeksekusi tindak lanjut berhasil mengeliminasi error 409 Conflict.
3. Build backend 0 error dan QBE Conformance PASS (0 violation).
4. Tidak ada perubahan migration/skema tabel, menjamin keselamatan data eksisting.
5. Verifikasi Live Browser Testing visual Chromium pada pasien nyata independen membuktikan keberhasilan eksekusi bebas error HTTP 409 Conflict (HTTP 200 OK).

