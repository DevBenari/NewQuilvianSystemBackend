# Laporan Perubahan Frontend — `FE-KSK-016`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-KSK-016` |
| Judul | Pencocokan scan kartu asuransi di Kiosk |
| Roadmap | [frontend-roadmap.md](../../../roadmap/frontend-roadmap.md) — *Amandemen 8 Oktober 2026 (B)* |
| Trace | `RJ-DOC-DEC-068`..`070`; `RJ-AC-PM-01`..`03` (blueprint Rawat Jalan) |
| Contract version | `RJ-DOC-REFERRAL-001@1.0.0` (**approved**) — `cardScan` pada `POST /patient-insurances` jalur Kiosk |
| Dependency | `RJ-DOC-REV-BE-020` ✅ |
| Task mode | `TASK MODE: FRONTEND` (`RJ-DOC-DEC-083`) |
| Commit frontend | `de323430` (`sukmagpV2`), belum di-commit |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 9 Oktober 2026 |
| Status | ✅ `COMPLETE` |

## 1. Keadaan awal

Form asuransi baru di Kiosk (Pasien Lama dan Pasien Baru) hanya berisi asuransi, No. polis, dan tanggal berlaku (`FE-KSK-010`). Kartu tidak pernah di-scan.

## 2. Proses dari sisi pengguna

Form asuransi baru (bukan penjamin perusahaan) mendapat tombol opsional **Scan Kartu Asuransi**. Scanner dan aturan pencocokannya sama dengan layar petugas (`RJ-DOC-REV-FE-021`):

| Hasil baca agent | Yang tampil | Akibat |
| --- | --- | --- |
| Tanpa nama/No. polis (keadaan agent sekarang) | Tidak ada alert | Perilaku `FE-KSK-010` identik |
| Nama atau No. polis berbeda | Alert merah **"Data tidak match"** + isi kartu | Pasien Lama: *Simpan Asuransi Baru* nonaktif. Pasien Baru: tidak dapat lanjut ke Layanan |
| Hanya salah satu terbaca | **"Kartu tidak terbaca lengkap"** | Sama |
| Cocok | Alert hijau **"Kartu cocok"** | `cardScan` ikut terkirim saat asuransi disimpan; backend memeriksa ulang |

Mengganti penyedia asuransi mengosongkan hasil scan. Penjamin yang sudah tersimpan tidak menampilkan tombol scan.

## 3. Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/kiosk/registration/shared/kiosk-insurance-card-scan.jsx` | **Baru** — tombol scan + alert status; memakai `usePlustekPayerCardScanner` dan `insurance-card-scan-utils` (`FE-021`) |
| `src/style/kiosk/registration/kiosk-insurance-card-scan.module.css` | **Baru** — token global saja |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-payment.jsx` | Status pencocokan, kunci Simpan, `cardScan` pada `createPatientInsuranceFromKiosk` |
| `src/components/view/kiosk/registration/new-patient/kiosk-new-patient-step-payment.jsx` | `cardScan` disimpan pada item asuransi, validasi lanjut, reset saat ganti penyedia |
| `src/lib/helpers/kiosk/registration/kiosk-new-patient-submit.helpers.jsx` | `buildPatientInsuranceRequests` mengirim `cardScan` bila terbaca |

**UI GATE: 2 elemen — REUSE 1, COMPOSE 1, NEW 0**

| Kebutuhan UI | Keputusan | Status |
| --- | --- | --- |
| Scanner kartu + aturan cocok | `usePlustekPayerCardScanner`, `matchInsuranceCardScan` (`FE-021`) | REUSE |
| Tombol + alert ukuran Kiosk | Komponen Kiosk bertoken (pola tombol/alert Kiosk) | COMPOSE |

## 4. Verifikasi

| Skenario / perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` berkas yang berubah | 0 error. Tidak ada warning baru dibanding `HEAD` | `PASS` |
| `npm run build` | exit 0 | `PASS` |
| Uji browser akun perangkat Kiosk (build 3100 → backend uji 7185, agent **tiruan**) | **7/7 PASS** | `PASS` |

| ID | Skenario | Hasil |
| --- | --- | --- |
| C1 | Pasien Lama, agent tanpa nilai → tanpa alert, Simpan aktif | PASS |
| C2 | No. polis kartu beda → "Data tidak match", Simpan nonaktif | PASS |
| C3 | Kartu cocok (polis beda format) → "Kartu cocok", Simpan aktif | PASS |
| C4 | Simpan → `POST /patient-insurances` (jalur Kiosk) membawa `cardScan`, backend `200` | PASS |
| C5 | Penjamin tersimpan terpilih tanpa tombol scan | PASS |
| C6 | Pasien Baru, kartu tidak cocok → "Data tidak match", tidak dapat lanjut ke Layanan (request tulis diblok) | PASS |
| C7 | Pasien Baru, kartu cocok → "Kartu cocok" | PASS |

Penjamin uji dari C4 dihapus (`DELETE …/patient-insurances/admin/{id}`, `200`). Kredensial akun Kiosk lewat variabel lingkungan, tidak ditulis ke berkas.

`AUTOMATED TEST: SKIPPED (opsional) — tidak ada test baru di repository.`

## 5. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `RJ-AC-PM-01`/`02` dengan agent tiruan | Terpenuhi | C2–C4, C6, C7 |
| 2. Penjamin tersimpan tidak diminta scan | Terpenuhi | C5 |
| 3. Agent tanpa nilai → perilaku `FE-KSK-010` identik | Terpenuhi | C1 |
| 4. Lint, build, uji browser akun Kiosk | Terpenuhi | Bagian 4 |

## 6. Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko | Pencocokan sungguhan menunggu kontrak OCR agent (`RJ-DOC-OQ-PM-01`) |
| Belum diuji | Simpan asuransi Pasien Baru sampai submit pendaftaran (submit membuat pasien baru); payload `cardScan` dibuktikan lewat source `buildPatientInsuranceRequests` |
| Perubahan sampingan | `NONE` |
