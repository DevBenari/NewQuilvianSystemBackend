# Roadmap — Amandemen Cetak Antrean Admisi Rawat Jalan (v1.1)

| Field | Nilai |
| --- | --- |
| Sumber | [Quilvian_Blueprint_Cetak_Antrean_Admisi_Rawat_Jalan_v1.1.md](../docs/Quilvian_Blueprint_Cetak_Antrean_Admisi_Rawat_Jalan_v1.1.md) (28 Okt 2026) |
| Wewenang | Instruksi pemilik modul (Sukma), sesi 2 Nov 2026: "revisi module rawat jalan dan module kiosk mengikuti blueprint; jangan build dan jangan test dulu" |
| Snapshot | BE `bc482d01` (`sukmagp`), FE `59984eb7a` (`sukmagpV2`) — working tree, belum di-commit |
| Batas scope | Berhenti saat kunjungan dan antrean poli internal (`RegQueue`) tercatat. Skrining, tanda vital, asesmen, dan dokter di luar scope. |

## Task

Task blueprint bagian 14 (`T01`–`T06`) dipetakan ke dua vertical slice.

| Task ID | Blueprint | Ringkasan | Status |
| --- | --- | --- | --- |
| `RJ-ADM-BE-001` 🟡 | T01–T05 | Tiket admisi anonim `RegAdmissionTicket` + riwayat, konfigurasi kelompok/loket, penerbitan atomik + idempotency + cetak ulang, Panggil Berikutnya multi-loket, transisi status, display, realtime, penautan tiket ke `POST patient-encounters/admin` dalam transaksi yang sama dengan `RegQueue` existing | 🟡 2026-11-02 — source + migration ditulis; **build, `has-pending-model-changes`, dan uji API belum dijalankan** atas instruksi pemilik. [Laporan](../task/report/backend/RJ-ADM-BE-001.md) |
| `RJ-ADM-FE-001` 🟡 | T02–T06 | Menu Cetak Antrean di kiosk + tiket thermal 80 mm, Daftar Antrean Admisi, Display Antrean Admisi, mode `?admissionTicketId=` pada Pendaftaran Pasien | 🟡 2026-11-02 — source ditulis; **lint, build, dan uji browser belum dijalankan** atas instruksi pemilik. [Laporan](../task/report/frontend/RJ-ADM-FE-001.md) |

## Grafik urutan dependency

```text
RJ-ADM-BE-001 🟡
└── RJ-ADM-FE-001 🟡
```

## Gerbang terbuka

1. `RJ-ADM-BE-001` 🟡 — build backend, `migrations has-pending-model-changes`, `database update` di DB dev pemilik, lalu bukti UAT-01..13 lewat API.
2. `RJ-ADM-FE-001` 🟡 — lint/build frontend, lalu uji browser kiosk, loket, display, dan wizard (UAT-01, 04, 06, 08, 11, 14, 15).
3. Validasi bisnis bagian 17 (jumlah loket, kelompok BPJS, printer thermal, SOP lewati/batal) masih milik pemilik; konfigurasi bawaan ada di `appsettings.json` `HealthServices:Registration:AdmissionQueue`.
