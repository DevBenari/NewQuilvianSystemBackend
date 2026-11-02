# Laporan Perubahan Frontend — `FE-HRD-022`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-022` |
| Judul | Service API Axios dan Redux Slice untuk Dokumen SK Kepegawaian (Personnel Action Order) |
| Slice | `S-A1` (Workforce Core Target State) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 4 |
| Trace | `HRD-DEC-048`, `HRD-DEC-050`, `HRD-DEC-056`, `HRD-DEC-057`, `BE-HRD-024`, `BE-HRD-025` |
| Contract version | Personnel Action Order API v1 (`HrdPersonnelActionOrderController.cs`) |
| Dependency | `BE-HRD-025` (✅ Selesai) |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `src/lib/services/hr/personnel-action-order-service.js`, `src/lib/state/slice/hr/workforce-core/personnel-action-order-slice.js`, `src/lib/state/store.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 02 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Untuk memfasilitasi antarmuka Surat Keputusan (SK) Kepegawaian pada rumah sakit, modul frontend membutuhkan jembatan komunikasi data terstruktur (Service API) dan manajemen state terpusat (Redux Toolkit).

Task ini mengimplementasikan:
1. **`PersonnelActionOrderService`**: Service Axios yang terhubung langsung ke controller backend `/v1/corporate/human-resource/personnel-action-orders` untuk operasi pengambilan daftar SK, pembacaan detail, pembuatan draf, pembaruan, pengajuan persetujuan, persetujuan, penolakan, pembatalan, dan unggah berkas fisik hasil pindai (*scan*).
2. **`personnelActionOrderSlice`**: Redux slice dengan thunk asinkron terkelola yang menangani status *loading*, pesan *error* ramah pengguna, data *caching*, dan didaftarkan resmi pada root Redux store.

---

## 1. Berkas yang Dibuat dan Diperbarui

1. `src/lib/services/hr/personnel-action-order-service.js` — Service Axios lengkap dengan 9 fungsi API.
2. `src/lib/state/slice/hr/workforce-core/personnel-action-order-slice.js` — Redux slice dan 9 thunk terstruktur.
3. `src/lib/state/store.jsx` — Pendaftaran reducer `personnelActionOrder` ke store utama.

---

## 2. Bukti Pengujian Unit

```text
✔ FE-HRD-022: PersonnelActionOrderService memanggil endpoint canonical backend tanpa /v1/hr/ (1.1391ms)
✔ FE-HRD-022: Redux slice SK Kepegawaian terdaftar di store (0.43ms)
```
Status: **PASS (100%)**
