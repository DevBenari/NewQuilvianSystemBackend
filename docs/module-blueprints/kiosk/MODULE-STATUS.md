# Kiosk — Status Modul

| Field | Nilai |
| --- | --- |
| Blueprint ID | `KSK-BP-001` |
| Nama modul | Kiosk (Revisi Module Kiosk & Cek Nomor Rekam Medis) |
| Revision | `1` |
| Status modul | `IN_PROGRESS` — blueprint & roadmap approved (Sukma, 30 Sep 2026) |
| Fase saat ini | `KSK-PH-003` — Perencanaan delivery |
| Terakhir diverifikasi | belum diverifikasi |
| SHA source backend | `419b910fca5188285946850a95976ebff83ae8ce` |
| SHA source frontend | `4ec51b0bf5e724e899b95f118e273351437b71e7` |

## Keadaan fase

| Fase selesai | Fase aktif | Fase terblokir |
| --- | --- | --- |
| `KSK-PH-001` Discovery, `KSK-PH-002` Desain | `KSK-PH-003` Perencanaan delivery | Tidak ada |

## Keadaan delivery

| Backend | Frontend | Integrasi | Verifikasi |
| --- | --- | --- | --- |
| `DONE` — `BE-KSK-001` ✅, `BE-KSK-002` ✅, `BE-KSK-003` ✅ | `DONE` — `FE-KSK-001` ✅ … `FE-KSK-008` ✅ | `NOT_STARTED` | `NOT_STARTED` |

## Keputusan

Seluruh keputusan tercatat di [00-interview-decisions.md](00-interview-decisions.md) r2 (`KSK-DEC-001..019`, status `approved`). Keputusan terbuka PRD dan seluruh conflict/unknown capability map r1 sudah ditutup.

## Blocker dan pemiliknya

Tidak ada blocker desain. Dua tindak lanjut lintas blueprint berada di luar wewenang tulis blueprint Kiosk dan baru memblokir **implementasi**:

| ID | Ringkasan | Pemilik | Fase terdampak | Bisa jalan terpisah |
| --- | --- | --- | --- | --- |
| ~~`KSK-OQ-004`~~ | **Ditutup 30 Sep 2026** — amendment `FE-LAB-13` / `AC-93` tercatat di blueprint Laboratorium (Amendment Pass putaran 20, revisi 82); pemilik modul Laboratorium perlu diberi tahu | — | — | — |
| ~~`KSK-OQ-005`~~ | **Ditutup 30 Sep 2026** — `RWI-ENC-PAYER-001` `1.1.0` tercatat di blueprint rawat-inap | — | — | — |

## Bukti usang

| Artefak/bukti | SHA tercatat | SHA saat ini | Impact review |
| --- | --- | --- | --- |
| — | — | — | — |

## Tindakan berikutnya yang disarankan

1. Roadmap `KSK-RM-BE-001` r1 dan `KSK-RM-FE-001` r1 **approved** (Sukma, 30 Sep 2026).
2. Task yang boleh jalan setelah approval (gelombang 1): `BE-KSK-001` (`TASK MODE: BACKEND`), `FE-KSK-001` dan `FE-KSK-002` (`TASK MODE: FRONTEND`).
3. Tutup `KSK-OQ-004` (membuka `FE-KSK-007`) dan `KSK-OQ-005` (membuka `BE-KSK-003` → `FE-KSK-008`).

## Progres delivery

`11 / 11` task approved selesai (3 BE, 8 FE): `BE-KSK-001` ✅, `BE-KSK-002` ✅, `BE-KSK-003` ✅, `FE-KSK-001` ✅, `FE-KSK-002` ✅, `FE-KSK-003` ✅, `FE-KSK-004` ✅, `FE-KSK-005` ✅, `FE-KSK-006` ✅, `FE-KSK-007` ✅, `FE-KSK-008` ✅ — 30 Sep – 1 Okt 2026 ([laporan 001](task/report/backend/BE-KSK-001.md), [laporan 002](task/report/backend/BE-KSK-002.md), [laporan 003](task/report/backend/BE-KSK-003.md), [laporan FE-KSK-003](task/report/frontend/FE-KSK-003.md), [laporan FE-KSK-004](task/report/frontend/FE-KSK-004.md), [laporan FE-KSK-005](task/report/frontend/FE-KSK-005.md), [laporan FE-KSK-006](task/report/frontend/FE-KSK-006.md), [laporan FE-KSK-001](task/report/frontend/FE-KSK-001.md), [laporan FE-KSK-002](task/report/frontend/FE-KSK-002.md), [laporan FE-KSK-007](task/report/frontend/FE-KSK-007.md), [laporan FE-KSK-008](task/report/frontend/FE-KSK-008.md)). Seluruh task backend dan frontend selesai; tidak ada yang tertahan. Tersisa: UAT di perangkat Kiosk fisik oleh pemilik, dan pemberitahuan amendment `KSK-OQ-004` kepada pemilik modul Laboratorium.

Amandemen 1 Okt 2026 (`KSK-DEC-020/021`): `FE-KSK-009` 🟡, `FE-KSK-010` 🟡 (keduanya menunggu uji browser dengan akun perangkat Kiosk), `FE-KSK-011` ⛔ (Cek No. RM kosong belum dapat direproduksi). Penjaga satu kunjungan aktif per pasien berlaku juga di Kiosk lewat `RJ-DOC-REV-BE-007` ✅ (blueprint Rawat Jalan). Lihat [roadmap](roadmap/frontend-roadmap.md#amandemen-1-oktober-2026--revisi-minor-kiosk).

Amandemen 8 Okt 2026 (`KSK-DEC-022..024`, PDF "Pendaftaran pasien manual"): `FE-KSK-012` ✅ (uji browser akun Kiosk 11/11), `FE-KSK-013` dibatalkan (`KSK-DEC-025`). Sisi petugas: `RJ-DOC-REV-FE-017` ✅ (blueprint Rawat Jalan).

Amandemen 8 Okt 2026 (B) — Rujukan Kiosk: `FE-KSK-014` ✅ (step Data Rujukan, uji browser 14/14), `FE-KSK-015` ✅ (popup jadwal praktik, 5/5), `FE-KSK-016` ✅ (pencocokan scan kartu asuransi, 7/7), semua `2026-10-09`. Backend dari blueprint Rawat Jalan bagian 19.

Amandemen 19 Nov 2026 — Amendment PO (Pendaftaran Pasien Online dan Check-in Kiosk, `KSK-DEC-026` s.d. `053`): desain revisi 2 `approved` (Sukma, 19 Nov 2026, `KSK-DEC-054`; `KSK-CONTRACT-v2`). Roadmap r1 `draft`: 10 task backend (`BE-KSK-PO-001` s.d. `010`) dan 8 task frontend (`FE-KSK-PO-001` s.d. `008`), `approved` (`KSK-DEC-055`). Gelombang 1 selesai 2026-11-19: `BE-KSK-PO-001` ✅ (migration `AddRegOnlineRegistration` diterapkan ke `QuilvianNewDevSukma`), `BE-KSK-PO-003` ✅, `FE-KSK-PO-004` ✅. `BE-KSK-PO-002` ✅ 2026-11-20 (kunci kuota; AC 2 runtime terbukti lewat `BE-KSK-PO-005`). `BE-KSK-PO-004` ✅ 2026-11-19 (endpoint publik opsi + pencocokan pasien). `BE-KSK-PO-005` ✅ 2026-11-20 (kirim pendaftaran online; nomor `PO-yyyyMMdd-nnnn`, delta format dicatat di laporan). `BE-KSK-PO-006` 🟡 2026-11-20 (verifikasi check-in kiosk; penolakan terbukti di runtime, `200` di harness, runtime `200` pada 2026-11-21). `BE-KSK-PO-009` ✅ 2026-11-20 (layar petugas baca). `BE-KSK-PO-010` ✅ 2026-11-20 (ubah jadwal, ubah kontak, batal). `BE-KSK-PO-008` 🟡 2026-11-20 (Jalur Lain terbukti di runtime; Tidak Datang di harness, runtime pada 2026-11-22). `FE-KSK-PO-001` ✅ 2026-11-20 (fondasi halaman publik `/online-registration`, CSP + widget Turnstile). `FE-KSK-PO-002` ✅ 2026-11-20 (langkah Data Pasien, Jenis Pasien, Data Kunjungan, Penjamin; komponen RJ dipakai ulang lewat prop opsional, regresi menu RJ petugas 13/13). `FE-KSK-PO-003` ✅ 2026-11-20 (Data Rujukan, General Consent, Verifikasi + Kirim multipart, Selesai dengan kode, email, dan tombol WhatsApp; halaman online lengkap `MVP-1`). `FE-KSK-PO-007` ✅ 2026-11-20 (Daftar Pendaftaran Online petugas dan butir menu di bawah Reservasi Prioritas). `FE-KSK-PO-008` ✅ 2026-11-20 (Detail, Ubah Jadwal, Ubah Kontak, Batalkan; layar petugas `MVP-3` lengkap). Berikutnya: 2026-11-21 runtime `200` `BE-KSK-PO-006` lalu `BE-KSK-PO-007`. Go-live menunggu `KSK-PO-OQ-010`, `015`, `016` (IT). Lihat [blueprint-manifest.md](blueprint-manifest.md) bagian Revisi 2.
