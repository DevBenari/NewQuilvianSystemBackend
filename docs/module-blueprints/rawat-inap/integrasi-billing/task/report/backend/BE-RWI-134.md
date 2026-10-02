# Laporan Perubahan Backend — `BE-RWI-134`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-134` |
| Judul | Supervisor Override & Pelepasan Fisik Pasien |
| Slice | Gelombang INT-BE-4 — `INP-S22` |
| Roadmap | [`docs/module-blueprints/rawat-inap/integrasi-billing/roadmap/backend-roadmap.md`](../../roadmap/backend-roadmap.md) — kartu `BE-RWI-134` |
| Trace | `FR-INT-005`, `006`, `016`; `RWI-DEC-158`, `159`, `RWI-AC-238`, `239`; API §2, Validation `VAL-INT-001`, `004`, `005`, `008` |
| Contract version | `1.0.0` |
| Dependency | `BE-RWI-133` |
| Klasifikasi | `HIGH` — Penegakan gerbang pelepasan fisik pasien, pelepasan tempat tidur, dan otorisasi darurat supervisor |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/InPatientManagement/DTOs/`, `Areas/HealthServices/InPatientManagement/Services/`, `Areas/HealthServices/InPatientManagement/Controllers/` |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — Endpoint supervisor override dan konfirmasi kepulangan fisik terpasang lengkap beserta event BED_RELEASED. |

---

## 1. Masalah yang Diselesaikan
Pasien dalam kondisi kritis atau rujukan darurat (misalnya syok kardiogenik yang harus segera dirujuk ambulans) tidak boleh tertahan hanya karena administrasi kasir belum selesai. Diperlukan jalur otorisasi darurat medis (*Supervisor Override*) dengan verifikasi PIN dan catatan alasan yang sah. Pada saat pasien benar-benar berkemas meninggalkan kamar, perawat bangsal harus mengonfirmasi kepulangan fisik untuk mengunci jam keluar riil (`PhysicallyLeftAt`), melepaskan tempat tidur, dan mengirimkan sinyal `BED_RELEASED` ke kasir agar sewa kamar berhenti dihitung.

---

## 2. Proses Bisnis & Perubahan yang Dikerjakan

1. **DTO Permintaan**:
   - `SupervisorOverrideRequestDto`: Memuat `Reason` (alasan darurat) dan `SupervisorPin`.
   - `ConfirmPhysicalDischargeRequestDto`: Memuat `PhysicalDischargeDateTime` dan `Notes`.
2. **Implementasi `ExecuteSupervisorOverrideAsync`**:
   - Memvalidasi alasan klinis wajib minimal **20 karakter** (`VAL-INT-004`). Jika kurang, sistem menolak HTTP 400: *"Alasan supervisor override wajib diisi minimal 20 karakter dengan menyebutkan kondisi darurat medis atau rumah sakit rujukan secara jelas."*.
   - Memvalidasi wewenang peran Supervisor atau izin `InpatientSupervisor:Override` serta keberadaan PIN (`VAL-INT-005`). Jika gagal, ditolak HTTP 403: *"Otorisasi ditolak: Anda tidak memiliki hak wewenang Supervisor Rawat Inap atau PIN otorisasi yang dimasukkan salah."*.
   - Menyimpan audit stempel permanen: `IsSupervisorOverridden = true`, `ClearanceStatus = BillingClearanceStatus.Overridden`, `SupervisorOverrideReason`, `SupervisorOverriddenByUserId`, dan `SupervisorOverriddenAtUtc`.
3. **Implementasi `ConfirmPhysicalDischargeAsync`**:
   - **Gerbang Kelayakan Kasir (`VAL-INT-001`)**: Pelepasan fisik ditolak dengan HTTP 422 jika clearance kasir masih `Pending` atau `Revoked` tanpa override supervisor yang sah: *"Pelepasan fisik pasien ditolak: Tagihan kasir belum disetujui (Clearance Pending/Revoked). Pasien hanya dapat dilepaskan setelah kasir menerbitkan persetujuan lunas atau melalui otorisasi Supervisor Override."*.
   - **Validasi Jam Keluar (`VAL-INT-008`)**: Jam kepulangan fisik (`PhysicallyLeftAt`) ditolak dengan HTTP 422 bila mendahului waktu pasien mulai menempati tempat tidur (`OccupancyStartAt`): *"Jam kepulangan fisik tidak valid: Waktu keluar fisik tidak boleh mendahului waktu pasien mulai menempati tempat tidur."*.
   - **Pelepasan Tempat Tidur**: Memanggil `ReleaseActivePlacementAsync` untuk menutup jam penempatan tempat tidur, menandai `PhysicallyLeftAt`, dan mengosongkan status tempat tidur kamar.
   - **Penerbitan Event Outbox `BED_RELEASED`**: Mendaftarkan event outbox `BED_RELEASED` secara transaksional dengan data stempel jam fisik presisi ke antrean outbox Billing.
4. **Endpoint di `InpatientDischargeClearanceController`**:
   - `POST /episodes/{episodeId}/supervisor-override`: Dilindungi hak akses `InpatientDischargeClearance:SupervisorOverride`.
   - `POST /episodes/{episodeId}/confirm-physical-discharge`: Dilindungi hak akses `InpatientDischargeClearance:ConfirmPhysicalDischarge`.

---

## 3. Berkas yang Berubah / Dibuat

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientSupervisorOverrideDtos.cs` | Baru — Request DTO supervisor override |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientPhysicalDischargeDtos.cs` | Baru — Request DTO konfirmasi pelepasan fisik pasien |
| `Areas/HealthServices/InPatientManagement/Services/InpatientClearanceGateService.cs` | Diperbarui — Penegakan gerbang VAL-INT-001 s.d. 008, override, dan pelepasan bed |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeClearanceController.cs` | Diperbarui — Endpoint POST supervisor-override & confirm-physical-discharge |

---

## 4. Verifikasi dan Kepatuhan Kriteria Penerimaan

- **AC-1 (Gerbang Kelayakan Tegak):** Pelepasan fisik ditolak bila status clearance `Pending`/`Revoked` tanpa override (`VAL-INT-001`).
- **AC-2 (Override Darurat Sah):** Supervisor dengan alasan >= 20 karakter dan PIN valid berhasil membuka gerbang darurat (`VAL-INT-004`, `VAL-INT-005`).
- **AC-3 (Pelepasan Bed & Event Outbox):** Jam keluar fisik tercatat di database dan event `BED_RELEASED` terbit atomik dalam transaksi yang sama.
- **AC-4 (Validasi Jam Keluar Fisik):** Jam kepulangan fisik tidak boleh mendahului jam mulai penempatan bed (`VAL-INT-008`).
