# Traceability Delivery Transfer Pasien Masuk (IGD/Rajal ke Ranap) & Penugasan PPJA

| Metadata | Nilai |
| --- | --- |
| blueprint_id / planning_id | `RWI-BP-001` / `TRF-DEL-20261016-01` |
| revision / tanggal / status | 1 / 2026-10-16 Asia/Jakarta / APPROVED_DESIGN; 3 task backend dan 3 task frontend belum dikerjakan |
| approval | Desain disetujui pengguna 16 Oktober 2026 ("iya lakukan /plan-module-delivery"). Kontrak target `0.14.0` |
| kontrak | `0.14.0` (isi Transfer Pasien Masuk & Penugasan PPJA) |
| masukan canonical | Decision log 51; manifest anak 13; flowcharts/19; contracts/admission-transfer-patient-handover-contract.md |
| source backend / frontend | `ca09cae909597442385ce4513be9c803549be10e` / `4d9bf608dbbaddc96a2b4acfc8723753b5dd5028` |
| hash snapshot | [transfer-pasien-masuk-delivery-lock.json](transfer-pasien-masuk-delivery-lock.json) |
| roadmaps | [backend](backend-roadmap-transfer-pasien-masuk.md) / [frontend](frontend-roadmap-transfer-pasien-masuk.md) |

---

## Matriks Keterlacakan Requirement ke Task dan Bukti Verifikasi

Bukti backend memakai requirement-ke-bukti-verifikasi menurut `rules/backend/TEST_POLICY.md`; tidak adanya automated test backend bukan gap. Seluruh status bukti **NOT_RUN** sampai task dikerjakan lewat builder.

| Requirement | Perilaku Terukur | Desain / Kontrak | Backend Task | Frontend Task | Bukti Diharapkan / Status |
| --- | --- | --- | --- | --- | --- |
| `FR-TRF-001` | Otomasi trigger pembuatan serah terima klinis `PendingSend` saat admisi transfer disubmit | BE 16.1; Kontrak 2.1; Flowchart 19 | `BE-RWI-241` | — | TRF-AC-001, 002; UAT-TRF-01; NOT_RUN |
| `FR-TRF-002` | Tampilan banner serah terima aktif di Detail Episode dan membuka Laci Transfer | FE 17.1; Kontrak 5.A | — | `FE-RWI-254` | TRF-AC-008; UAT-TRF-01; NOT_RUN |
| `FR-TRF-003` | Verifikasi rekonsiliasi obat IGD/Poli dan alert alergi di laci serah terima | BE 16.1; FE 17.1; Kamus Data 23.2 | `BE-RWI-242` | `FE-RWI-255` | TRF-AC-003, 010; UAT-TRF-01; NOT_RUN |
| `FR-TRF-004` | Penetapan perawat PPJA wajib saat menerima pasien transfer (tombol disabled jika kosong) | BE 16.1; FE 17.1; Kontrak 4 (`CLI-TRH-001`) | `BE-RWI-242` | `FE-RWI-255` | TRF-AC-004, 011; UAT-TRF-01, 02; NOT_RUN |
| `FR-TRF-005` | Pencegahan akun penerima serah terima sama dengan perawat pengirim unit asal | BE 16.1; FE 17.1; Kontrak 4 (`CLI-TRH-002`) | `BE-RWI-242` | `FE-RWI-255` | TRF-AC-005, 012; NOT_RUN |
| `FR-TRF-006` | Pratinjau & cetak Gelang Ranap Resmi dan Checklist Serah Terima Pasien Baru di PPRI | FE 17.2; Kontrak 5.B | — | `FE-RWI-256` | TRF-AC-013, 014; UAT-TRF-01; NOT_RUN |
| `FR-TRF-007` | Alert status dokumen PPRI belum selesai pada Detail Episode bersifat non-blocking | BE 16.1; FE 17.1; Kamus Data 23.3 | `BE-RWI-243` | `FE-RWI-254` | TRF-AC-006, 007, 009; UAT-TRF-03; NOT_RUN |

---

## Matriks Kriteria Penerimaan (Acceptance Criteria Index)

| Kode AC | Pernyataan Kriteria Penerimaan | Task Pemilik |
|---|---|---|
| `TRF-AC-001` | `POST /admission-transfers/admit` otomatis menghasilkan `CliTransferHandover` dengan status `PendingSend` | `BE-RWI-241` |
| `TRF-AC-002` | Snapshot klinis handover memuat TTV, alat, dan riwayat obat terakhir dari kunjungan asal | `BE-RWI-241` |
| `TRF-AC-003` | Rekonsiliasi obat IGD/Poli tersimpan pada snapshot serah terima klinis saat diterima | `BE-RWI-242` |
| `TRF-AC-004` | `PATCH /transfer-handovers/{id}/accept` menolak request tanpa `assignedNurseId` dengan `422 Unprocessable Entity` (`CLI-TRH-001`) | `BE-RWI-242` |
| `TRF-AC-005` | `PATCH /transfer-handovers/{id}/accept` menolak request jika akun login sama dengan pengirim (`CLI-TRH-002`) | `BE-RWI-242` |
| `TRF-AC-006` | Kueri detail episode mengembalikan objek `admissionDocumentsSummary` yang akurat | `BE-RWI-243` |
| `TRF-AC-007` | `isAllCompleted` bernilai `false` jika masih ada dokumen admisi PPRI yang belum `Completed` | `BE-RWI-243` |
| `TRF-AC-008` | Detail Episode merender `TransferHandoverBanner` amber jika terdapat handover `Sent` | `FE-RWI-254` |
| `TRF-AC-009` | Detail Episode merender alert kuning ramah pengguna jika berkas PPRI belum lengkap tanpa memblokir layanan | `FE-RWI-254` |
| `TRF-AC-010` | `TransferHandoverDrawer` merender badge alergi dan tabel terapi obat dari unit asal | `FE-RWI-255` |
| `TRF-AC-011` | Tombol `[Terima Pasien]` pada drawer terkunci (disabled) sebelum perawat PPJA dipilih | `FE-RWI-255` |
| `TRF-AC-012` | Menerima serah terima berhasil menutup drawer dan memperbarui status penugasan PPJA | `FE-RWI-255` |
| `TRF-AC-013` | Gelang ranap resmi memuat No RM, Nama, Bed, Ruangan, dan Barcode/QR siap cetak | `FE-RWI-256` |
| `TRF-AC-014` | Tanda tangan perawat pada berkas serah terima PPRI aktif saat pasien menempati bed | `FE-RWI-256` |
