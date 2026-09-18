# Laporan Perubahan Backend — `BE-RWI-128`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-128` |
| Judul | Service Outbox & Enqueue Helper |
| Slice | Gelombang INT-BE-2 — `INP-S22` |
| Roadmap | [`docs/module-blueprints/rawat-inap/integrasi-billing/roadmap/backend-roadmap.md`](../../roadmap/backend-roadmap.md) — kartu `BE-RWI-128` |
| Trace | `FR-INT-002`, `FR-INT-017`; `RWI-DEC-161`; Backend Architecture §3 & §7 |
| Contract version | `1.0.0` |
| Dependency | `BE-RWI-127` |
| Klasifikasi | `MEDIUM` — Service pendaftaran event transaksional outbox |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/InPatientManagement/Services/`, `Program.cs` |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — Interface dan implementasi service terpasang dan terdaftar di Dependency Injection. |

---

## 1. Masalah yang Diselesaikan
Agar penerbitan event ke Billing tidak terpisah dari penyimpanan status klinis/operasional, dibutuhkan service transaksional yang mendaftarkan baris outbox ke dalam `DbContext` yang sama dengan transaksi bisnis, sehingga event hanya tersimpan bila transaksi bisnis berhasil di-commit (mencegah *phantom event*).

---

## 2. Proses Bisnis & Perubahan yang Dikerjakan

1. **Antarmuka `IInpIntegrationOutboxService`**:
   - Dibuat di `Areas/HealthServices/InPatientManagement/Services/IInpIntegrationOutboxService.cs`.
   - Menyediakan metode `EnqueueEventAsync(string eventType, string idempotencyKey, string sourceDomain, string sourceType, string sourceDetailId, object payload, CancellationToken cancellationToken)`.
2. **Implementasi `InpIntegrationOutboxService`**:
   - Dibuat di `Areas/HealthServices/InPatientManagement/Services/InpIntegrationOutboxService.cs`.
   - Melakukan validasi compound key idempotensi sesuai `VAL-INT-007` (`SourceDomain:SourceType:SourceDetailId:Version`).
   - Melakukan serialisasi `payload` menjadi format JSON string terstruktur.
   - Menambahkan objek `InpIntegrationOutbox` baru berstatus `OutboxStatus.Pending` ke `_dbContext.InpIntegrationOutboxes`.
   - Penulisan bersifat atomik: record ikut tersimpan saat pemanggil mengeksekusi `await _dbContext.SaveChangesAsync()`.
3. **Pendaftaran Dependency Injection**:
   - Mendaftarkan `IInpIntegrationOutboxService` dan `InpIntegrationOutboxService` sebagai `Scoped` di `Program.cs`.

---

## 3. Berkas yang Berubah / Dibuat

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/IInpIntegrationOutboxService.cs` | Baru — Kontrak interface outbox service |
| `Areas/HealthServices/InPatientManagement/Services/InpIntegrationOutboxService.cs` | Baru — Implementasi pendaftaran event atomik outbox |
| `Program.cs` | Diperbarui — Registrasi DI service outbox |

---

## 4. Verifikasi dan Kepatuhan Kriteria Penerimaan

- **AC-1 (Atomisitas Transaksional):** Penambahan entitas outbox dilakukan pada DbContext yang sama tanpa membuka koneksi atau commit terpisah.
- **AC-2 (Pencegahan Phantom Event):** Jika transaksi utama di-rollback, pesan outbox ikut terbatalkan otomatis.
- **AC-3 (Validasi Format Kunci Idempotensi):** Validasi format compound key sesuai `VAL-INT-007` diterapkan secara ketat.
