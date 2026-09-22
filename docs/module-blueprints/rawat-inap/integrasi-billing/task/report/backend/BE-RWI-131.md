# Laporan Perubahan Backend — `BE-RWI-131`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-131` |
| Judul | Mutasi & Koreksi saat Billing OPEN |
| Slice | Gelombang INT-BE-4 — `INP-S22` |
| Roadmap | [`docs/module-blueprints/rawat-inap/integrasi-billing/roadmap/backend-roadmap.md`](../../roadmap/backend-roadmap.md) — kartu `BE-RWI-131` |
| Trace | `FR-INT-007` s.d. `010`; `RWI-DEC-157`, `RWI-AC-237`; Validation `VAL-INT-002`, `003` |
| Contract version | `1.0.0` |
| Dependency | `BE-RWI-130` |
| Klasifikasi | `MEDIUM` — Validasi mutasi kamar terhadap status kasir, versioning placement, dan event koreksi hunian |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs` |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — Validasi status billing OPEN/CLOSED, penandaan is_superseded, dan event OCCUPANCY_CORRECTED terpasang. |

---

## 1. Masalah yang Diselesaikan
Jika pasien dipindahkan kamar atau kelas perawatannya saat kasir sudah menutup tagihan (`CLOSED`), mutasi kamar dapat merusak pembukuan keuangan rumah sakit. Selain itu, riwayat penempatan lama tidak boleh terhapus (*hard delete*), melainkan harus ditandai sebagai data yang digantikan (*superseded*) dengan nomor versi baru dan alasan perpindahan yang jelas.

---

## 2. Proses Bisnis & Perubahan yang Dikerjakan

1. **Validasi Alasan Mutasi (`VAL-INT-003`)**:
   - Pada `InpBedOccupancyService.TransferAsync`, memvalidasi bahwa kolom alasan mutasi (`request.Reason`) wajib diisi minimal **10 karakter**.
   - Jika kurang, sistem mengembalikan penolakan: *"Alasan perubahan kamar wajib diisi minimal 10 karakter untuk keperluan jejak rekam audit."*.
2. **Validasi Status Tagihan Kasir (`VAL-INT-002`)**:
   - Sistem memeriksa status tagihan kasir (`BilFolio`) pasien yang terkait dengan kunjungan (`EncounterId`).
   - Jika folio kasir berstatus `BillingFolioStatus.Closed`, mutasi kamar **ditolak seketika** dengan kode HTTP 422: *"Mutasi kamar ditolak: Tagihan kasir pasien sudah berstatus CLOSED. Data hunian kamar tidak dapat diubah kembali. Hubungi bagian Kasir/Keuangan bila diperlukan pembukaan kembali tagihan."*.
3. **Pemberian Versi Baru & Penandaan `IsSuperseded`**:
   - Baris penempatan lama ditandai `IsSuperseded = true` dan `SupersededAtUtc = DateTime.UtcNow` tanpa menghapus data fisik (menjaga integritas audit log).
   - Baris penempatan tempat tidur baru dibuat dengan nomor versi increment: `Version = currentPlacement.Version + 1`, dan menyimpan `ChangeReason = request.Reason`.
4. **Penerbitan Event Outbox `OCCUPANCY_CORRECTED`**:
   - Mendaftarkan event outbox `OCCUPANCY_CORRECTED` dengan kunci idempoten `INPATIENT:ROOM_STAY:{newPlacement.Id}:{newPlacement.Version}`.
   - Mengirimkan detail mutasi kamar (`PreviousBedId`, `NewBedId`, `PreviousRoomClassId`, `NewRoomClassId`, `EffectiveTransferTime`, `ChangeReason`) agar modul kasir dapat melakukan perhitungan ulang tarif (*repricing*).

---

## 3. Berkas yang Berubah / Dibuat

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs` | Diperbarui — Logika validasi CLOSED, syarat 10 karakter, superseded tracking, dan event OCCUPANCY_CORRECTED |

---

## 4. Verifikasi dan Kepatuhan Kriteria Penerimaan

- **AC-1 (Penolakan saat CLOSED):** Sistem menolak aksi transfer kamar bila folio kasir berstatus CLOSED (`VAL-INT-002`).
- **AC-2 (Minimal 10 Karakter):** Sistem mewajibkan alasan mutasi minimal 10 karakter (`VAL-INT-003`).
- **AC-3 (Non-Destructive Audit):** Baris penempatan lama tetap utuh dengan tanda `IsSuperseded = true` dan nomor versi naik pada baris baru.
- **AC-4 (Event Koreksi Terbit):** Event outbox `OCCUPANCY_CORRECTED` tercatat secara atomik di database.
