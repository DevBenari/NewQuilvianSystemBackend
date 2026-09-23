# Laporan Perubahan Backend — `BE-RWI-129`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-129` |
| Judul | Background Worker & Backoff |
| Slice | Gelombang INT-BE-3 — `INP-S22` |
| Roadmap | [`docs/module-blueprints/rawat-inap/integrasi-billing/roadmap/backend-roadmap.md`](../../roadmap/backend-roadmap.md) — kartu `BE-RWI-129` |
| Trace | `FR-INT-018`; `RWI-DEC-161`, `RWI-AC-241`; Backend Architecture §7.2 |
| Contract version | `1.0.0` |
| Dependency | `BE-RWI-128` |
| Klasifikasi | `MEDIUM` — Background service polling antrean outbox dengan exponential backoff |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/InPatientManagement/Workers/`, `Program.cs` |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — Worker background terpasang dan terdaftar di Hosted Services `Program.cs`. |

---

## 1. Masalah yang Diselesaikan
Pesan yang telah tercatat di tabel `InpIntegrationOutboxes` perlu diambil dan dikirimkan secara berkala ke modul Billing atau message broker dengan ketahanan terhadap kegagalan jaringan (*network resilience*), penghitungan jeda percobaan ulang bertahap (*exponential backoff*), dan penanganan pesan macet (*Dead-Letter Queue*).

---

## 2. Proses Bisnis & Perubahan yang Dikerjakan

1. **Kelas `InpatientIntegrationOutboxWorker`**:
   - Dibuat di `Areas/HealthServices/InPatientManagement/Workers/InpatientIntegrationOutboxWorker.cs`, mewarisi `BackgroundService`.
   - Menggunakan `PeriodicTimer` dengan polling interval **5 detik**.
   - Setiap siklus membuat scope baru via `IServiceScopeFactory` untuk mengambil batch maksimal **50 pesan** yang berstatus `Pending` atau (`Failed` dengan `NextRetryAtUtc <= UtcNow`).
2. **Siklus Hidup Status Outbox**:
   - Pesan yang diambil diubah statusnya menjadi `Processing`.
   - Jika pengiriman sukses: status diubah menjadi `Published`, `PublishedAtUtc` dicatat waktu UTC sekarang, dan `LastError` dibersihkan.
   - Jika pengiriman gagal: `RetryCount` bertambah 1, `LastError` mencatat pesan error, dan waktu retry berikutnya dihitung dengan rumus eksponensial:
     $$\text{Delay} = \min(2^{\text{RetryCount}} \times 5 \text{ detik}, 3600 \text{ detik})$$
     `NextRetryAtUtc = UtcNow + Delay`.
   - Jika `RetryCount >= 10`: status diubah menjadi `DeadLetter` untuk mencegah polling tak berujung dan memberikan tanda peringatan audit bagi tim IT.
3. **Pendaftaran di `Program.cs`**:
   - Ditambahkan `builder.Services.AddHostedService<InpatientIntegrationOutboxWorker>();`.

---

## 3. Berkas yang Berubah / Dibuat

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/Workers/InpatientIntegrationOutboxWorker.cs` | Baru — Hosted background service outbox poller |
| `Program.cs` | Diperbarui — Registrasi AddHostedService outbox worker |

---

## 4. Verifikasi dan Kepatuhan Kriteria Penerimaan

- **AC-1 (Transisi Pending → Published):** Pesan yang berhasil diproses terverifikasi beralih ke status `Published` dengan stempel waktu `PublishedAtUtc`.
- **AC-2 (Eksponensial Backoff):** Formulasi jeda percobaan ulang terimplementasi presisi dengan batas maksimum 1 jam (3600 detik).
- **AC-3 (Ambang Batas Dead-Letter):** Pesan yang gagal 10 kali secara konsisten ditandai sebagai `DeadLetter`.
