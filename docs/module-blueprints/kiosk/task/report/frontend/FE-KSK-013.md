# Laporan Perubahan Frontend — `FE-KSK-013`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-KSK-013` |
| Judul | Jenis Kunjungan dipilih di step Pilih Layanan Tujuan |
| Slice | Amandemen 8 Oktober 2026 — Revisi Pendaftaran Pasien manual |
| Roadmap | [frontend-roadmap.md](../../../roadmap/frontend-roadmap.md#amandemen-8-oktober-2026--revisi-pendaftaran-pasien-manual) |
| Trace | `KSK-DEC-023`, `KSK-DEC-024`; jalur B `RJ-DOC-DEC-067` |
| Task mode | `FRONTEND` (`KSK-DEC-024`) |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat diperiksa | `de323430` (`sukmagpV2`) |
| Tanggal | 8 Oktober 2026 |
| Status | `CANCELLED` — dibatalkan oleh `KSK-DEC-025` (8 Okt 2026); tidak ada perubahan source |

## 1. Keadaan yang ditemukan

- Pasien Lama: step *Jenis Kunjungan* (Pasien Umum / Pasien Rujukan) berada **sebelum** *Pembayaran*. *Layanan & Dokter* berada **sesudah** *Pembayaran* (`OLD_PATIENT_STEP_ITEMS`, `use-kiosk-old-patient-registration.jsx`).
- Pilihan *Pasien Rujukan* sudah terhubung ke `referralForm` (sumber rujukan) yang dikirim saat Konfirmasi. Pasien Baru punya alur serupa (`REGISTRATION_TYPES`, `use-kiosk-new-patient-registration.jsx`).
- PDF yang sama (butir pasien lama 4, berlaku juga untuk pasien baru) meminta **step baru Rujukan sesudah data kunjungan** bila pasien rujukan. Isinya: identitas rujukan, fasilitas perujuk bermitra, unit tujuan, diagnosa, alasan, dan upload surat. Bagian ini adalah jalur B (`RJ-DOC-DEC-067`) dan belum didesain.

## 2. Alasan terblokir

Memindahkan Jenis Kunjungan ke *Layanan & Dokter* mengubah urutan step dan titik masuk `referralForm`. Step Rujukan jalur B akan menentukan ulang di mana data rujukan diisi dan di mana pilihan Umum/Rujukan berada. Bila task ini dikerjakan sekarang, urutan step Kiosk berisiko dibongkar dua kali, dan bar step yang dilihat pasien berubah dua kali.

Blocker: **keputusan desain step Rujukan jalur B (`RJ-DOC-DEC-067`)**, yang dijadwalkan lewat `grill-me` sesudah jalur A.

## 3. Verifikasi

Tidak ada source yang diubah. `NOT RUN`.

## 4. Acceptance criteria

| Kriteria | Status |
| --- | --- |
| 1–5 | Belum dikerjakan — menunggu blocker |

## 5. Langkah membuka blokir

Putuskan dalam `grill-me` jalur B: (a) letak pilihan Umum/Rujukan di Kiosk, dan (b) posisi step Rujukan baru terhadap *Layanan & Dokter*. Sesudah itu task ini dikerjakan bersama step Rujukan dalam satu perubahan urutan step.

## 6. Penutupan

Blocker ditutup lewat `grill-me` jalur B (8 Okt 2026). Pemilik memilih Jenis Kunjungan Kiosk tetap step sendiri sebelum memilih layanan (`KSK-DEC-025`), karena pilihan Umum/Rujukan menentukan apakah step Rujukan muncul (`RJ-DOC-DEC-072`). Task ini dibatalkan tanpa perubahan source.
