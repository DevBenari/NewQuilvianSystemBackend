# Laporan Task `RJ-REG-REV-FE-009` — Kartu jadwal dokter penuh dinonaktifkan

| Field | Isi |
| --- | --- |
| Roadmap | [registration-revision-roadmap.md](../../../roadmap/registration-revision-roadmap.md) |
| Requirement | `RJ-REG-REV-013` |
| Keputusan | `RJ-REG-DEC-010`, wewenang `RJ-REG-DEC-001` |
| Dependency | `RJ-REG-REV-BE-006` ✅ ([laporan](../backend/RJ-REG-REV-BE-006.md)) |
| Tanggal | 15 Oktober 2026 |
| Status | ✅ `TESTED` / `UAT_PENDING` |

## 1. Hasil untuk pengguna

Pada step Data Kunjungan, dan step Data Rujukan yang memakai kartu yang sama, jadwal dokter yang kuotanya sudah penuh pada tanggal kunjungan tampil abu-abu. Tombolnya bertuliskan **Penuh** dan kartunya tidak bisa dipilih. Badge kuota tidak lagi tampil.

Contoh: petugas memilih Poli Bedah untuk hari ini. Kuota dr. Arif Lesmana sudah habis, jadi kartunya tampil nonaktif dengan tombol merah "Penuh". Petugas memilih dokter lain atau tanggal lain.

Aturan yang dipakai layar:

| Tanggal kunjungan | Jenis | Field yang dibaca |
| --- | --- | --- |
| Hari ini | Walk-in (sama dengan payload `isWalkIn: !isAppointment`) | `isFullForWalkIn` |
| Mendatang | Appointment | `isFullForAppointment` |

Bila jadwal yang sudah dipilih ternyata penuh (misalnya sesudah tanggal diganti), pilihannya dikosongkan. Bila kuota gagal dimuat, tidak ada kartu yang dinonaktifkan; validasi simpan backend tetap menolak jadwal yang penuh.

## 2. Berkas

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/.../outpatient-registration.constants.js` | URL `doctorScheduleQuotas` |
| `src/lib/services/.../outpatient-registration.service.js` | `fetchOutpatientDoctorScheduleQuotas({ visitDate, clinicId, signal })` |
| `src/lib/hooks/.../outpatient-registration/use-outpatient-schedule-quotas.js` | Baru. Memuat kuota per tanggal + poli, dengan request yang dibatalkan saat tanggal/poli berganti; mengembalikan `fullScheduleIds` |
| `src/components/view/.../outpatient-visit-step.jsx` | `ScheduleSelect` memakai hook; `DoctorScheduleCard` mendapat prop `full` (`disabled`, `aria-disabled`, `title`, label *Penuh*); pilihan yang penuh dikosongkan |
| `src/style/.../outpatient-registration.module.css` | `.doctorCardFull`, `.doctorCardActionFull` (token `--color-surface-soft`, `--color-border-strong`, `--color-danger`) |

## 3. Keputusan base component

| Elemen | Status | Bukti |
| --- | --- | --- |
| Kartu jadwal penuh | `COMPOSE` | `DoctorScheduleCard` milik fitur ini (FE-005) mendapat keadaan `disabled` + kelas CSS; base component tidak disentuh |
| Keterangan *Penuh* | `COMPOSE` | Memakai tombol aksi kartu yang sudah ada; bentuk visualnya `DEV_DISCRETION` sesuai kartu task |

`UI GATE: PASS` — tidak ada komponen baru dan tidak ada perubahan perilaku default base component.

## 4. Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| ESLint 4 berkas JS/JSX tersentuh | exit 0 — `PASS` |
| `npm run build` | "Compiled successfully", tanpa `Failed` — `PASS` |
| Grep anti-regresi CSS baru (warna literal, `!important`, typography) | kosong; token `--color-danger-soft` yang tidak ada diganti `--color-surface` — `PASS` |
| AUTOMATED TEST | `SKIPPED (opsional)` — perilaku diuji di layar |

Uji layar Playwright: FE dev `localhost:3000` (source terkini), API `7184` dialihkan ke backend uji `https://localhost:7185` (build `BE-006`), DB `QuilvianNewDevSukma`, akun SuperAdmin, pasien `REGREV LAYAR 2508`. Respons kuota diatur lewat `route.fulfill`: galat `500` untuk F1–F2, dan respons asli backend dengan semua `isFull*` diubah menjadi `true` untuk F4–F6. Endpoint aslinya sudah diuji pada BE-006 (R0–R12).

| ID | Skenario | Hasil |
| --- | --- | --- |
| F1 | Kuota gagal dimuat → 4 kartu tetap aktif | `PASS` |
| F2 | Kuota gagal → kartu tetap bisa dipilih | `PASS` |
| F3 | Request membawa `visitDate=2026-10-15&clinicId=0614bbe8-…` | `PASS` |
| F4 | Semua jadwal penuh → kartu dokter `disabled` dengan label *Penuh* | `PASS` |
| F5 | Klik kartu penuh tidak memilihnya | `PASS` |
| F6 | Badge kuota tidak tampil | `PASS` |

Total 6/6 `PASS`. Tangkapan layar: kartu dr. Arif Lesmana (Poli Bedah) tampil abu-abu dengan tombol merah *Penuh*.

## 5. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Jadwal penuh nonaktif dengan *Penuh*, tidak bisa dipilih | Terpenuhi | F4, F5 |
| 2. Jadwal belum penuh tetap bisa dipilih | Terpenuhi | F1, F2 (jalur tanpa jadwal penuh) |
| 3. Ganti tanggal memperbarui status penuh | Terpenuhi lewat kode: kunci request = `visitDate\|clinicId`, dan F3 membuktikan `visitDate` terkirim. Ganti tanggal tidak diklik di layar | kode, F3 |
| 4. Gagal memuat kuota tidak menonaktifkan kartu | Terpenuhi | F1, F2 |
| 5. Badge kuota tidak tampil | Terpenuhi | F6 |

## Catatan penutup

| Hal | Isi |
| --- | --- |
| Wewenang UI | `DEV_DISCRETION` untuk bentuk keterangan *Penuh* (kartu task) |
| Risiko tersisa | Status penuh dibaca saat tanggal/poli dipilih. Bila kuota habis sesudahnya, layar tidak diperbarui otomatis, tetapi simpan tetap ditolak backend dengan pesan "Kuota ... sudah penuh". Uji dengan data penuh asli (bukan mock) masuk UAT |
| Perubahan sampingan | `NONE` |
| Status Git | Berkas di atas `M`/`??`, belum di-commit |
| Langkah berikutnya | UAT petugas pendaftaran |
