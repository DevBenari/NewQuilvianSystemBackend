# Kiosk — Kontrak Integrasi

| Field | Nilai |
| --- | --- |
| Set kontrak | `KSK-CONTRACT-v1` |
| `last_changed_in` | `v1` |
| Status | `approved` |
| Owner | Sukma Giri Pratama |
| `approved_by` / `approved_at` | Sukma Giri Pratama / 2026-09-30 |
| `input_revision` | `00-interview-decisions.md` r2; `01-existing-capability-map.md` r1 |
| Traceability | `KSK-DEC-002/013/014`, `KSK-OQ-004/005`, `RJ-BIL-DEC-015`, `RWI-ENC-PAYER-001` |

Kiosk **tidak memanggil sistem luar** (tidak ada Dukcapil, BPJS, OTP, atau pihak ketiga — PRD §46). Kontrak ini hanya mengatur titik sentuh **antar-modul internal**.

## 1. Arah baca dan tulis

| Modul lain | Arah | Apa | Lewat | Sinkron? | Kalau modul lain gagal |
| --- | --- | --- | --- | --- | --- |
| PatientManagement (`Pat`/`Mst`) | Kiosk **membaca** | Pasien, dokumen identitas, kartu asuransi, member, relasi perusahaan | `KioskPatientLookupService` (query baca-saja); endpoint `patients/kiosk/{id}`, `*-kiosk/options` existing | Ya | Lookup menjawab `500` → layar `ERROR` + Coba Lagi. Tidak ada data yang ditulis. |
| RegistrationManagement — sesi kiosk | Kiosk **menulis** | Satu sesi per perjalanan Pasien Lama, dibentuk di Step 3 | `POST kiosk-scan-sessions/kiosk/scan-result` (existing) | Ya | Tetap di Step 3 + `KSK-VAL-012`; tidak lanjut tanpa sesi |
| RegistrationManagement — kunjungan | Kiosk **menulis** | Kunjungan + satu sumber pembayaran | `POST patient-encounters/kiosk` (existing, aturan dilonggarkan) | Ya | Tetap di Konfirmasi + pesan; transaksi existing menjamin tidak ada yang tersimpan separuh |
| Laboratorium | Laboratorium **membaca** sesi kiosk | Sesi dengan `targetService = 2` | Panel lab existing | — | Di luar Kiosk |
| `KioskEncounterClosureService` | Membaca kunjungan + sesi | Menutup kunjungan kiosk yang tidak dilanjutkan | Hosted service existing | Terjadwal | Tidak berubah; sesi poliklinik tetap tanpa target (`KSK-DSN-007`) |
| Billing-kasir | Billing **membaca** sumber pembayaran kunjungan | Penanggung (`RegPatientEncounterGuarantor`) | Existing | — | Tidak berubah. Kunjungan Penjamin Perusahaan dari Kiosk diperlakukan sama dengan dari route admin (`RWI-ENC-PAYER-001`) |

## 2. Idempotensi, timeout, retry

| Titik | Aturan |
| --- | --- |
| Lookup | Aman diulang (tidak menulis). Satu request aktif per layar; request lama dibatalkan (`AbortController`) bila pasien mengubah isian. Timeout layar 20 detik → `ERROR`. |
| Pembentukan sesi di Step 3 | Satu kali per perjalanan. Tombol terkunci selama request. Bila jawaban hilang (timeout), layar **tidak** otomatis mengulang; pasien diberi Coba Lagi. Satu kali ulang yang berhasil setelah timeout dapat menghasilkan sesi kedua — risiko ini sama dengan perilaku hari ini dan dicatat sebagai risiko sisa (`KSK-RISK-001`). |
| Create encounter | Mengikuti `RegistrationIdempotencyKey` existing bila dipakai layar; tidak berubah oleh revisi ini. |

## 3. Dependency lintas blueprint

| ID | Blueprint lain | Isi | Memblokir |
| --- | --- | --- | --- |
| `KSK-OQ-004` | `laboratorium` | Amendment `FE-LAB-13` / `AC-93`: Tujuan Layanan pindah ke setelah Review; sesi dibentuk di Step 3 | Task FE urutan step |
| `KSK-OQ-005` | `rawat-inap` (`episode-rawat-inap/contracts/encounter-company-guarantor-contract.md`) | Amendment `RWI-ENC-PAYER-001` v1.0.0 → v1.1.0: route kiosk menerima `CompanyGuarantor` (disetujui Muhammad Hamzah, `KSK-DEC-013`) | **Ditutup 30 Sep 2026** — v1.1.0 tercatat (§7, §11) |
