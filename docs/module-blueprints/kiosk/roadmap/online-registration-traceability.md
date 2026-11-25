# Traceability — Pendaftaran Pasien Online dan Check-in Kiosk (Amendment PO)

| Field | Nilai |
| --- | --- |
| Traceability ID | `KSK-PO-TRACE-001` |
| Revision | `1.1` — `KSK-DEC-057` (`BE-KSK-PO-011`, `FE-KSK-PO-009`) |
| Status | `approved` — mengikuti roadmap `KSK-PO-RM-BE-001` r1 dan `KSK-PO-RM-FE-001` r1 (`KSK-DEC-055`); r1.1 (`KSK-DEC-058`) |
| Blueprint | `KSK-BP-001` revisi `2`, `approved` (`KSK-DEC-054`) |
| Kontrak | `KSK-CONTRACT-v2` `approved`; `KSK-CONTRACT-v2.1` `approved` (`KSK-DEC-058`) |
| SHA baseline | BE `5e777833`, FE `5f18e9f04` |
| Roadmap | [online-registration-backend-roadmap.md](online-registration-backend-roadmap.md), [online-registration-frontend-roadmap.md](online-registration-frontend-roadmap.md) |

Kolom **Status** diisi task builder sesudah laporan task ada (`✅`, `🟡`, `⛔`, atau kosong).

## 1. Acceptance criteria → desain → task → bukti

| AC (§13.9) | Keputusan | Desain/kontrak | Task backend | Task frontend | Bukti yang diharapkan | Status |
| --- | --- | --- | --- | --- | --- | --- |
| AC 1 — halaman tanpa login, urutan langkah sama dengan menu RJ | `KSK-DEC-029` | PO-FE.3 `FE-KSK-PO-01`; `KSK-PO-FE-001` | `BE-KSK-PO-004` | `FE-KSK-PO-001`, `002`, `003` | PO-T.1 baris AC 1 | ✅ `BE-KSK-PO-004` ✅ 2026-11-19, `FE-KSK-PO-001` ✅ 2026-11-20 (halaman tanpa login, urutan langkah = menu RJ; [laporan](../task/report/frontend/FE-KSK-PO-001.md)), `FE-KSK-PO-002` ✅ 2026-11-20 (urutan langkah Pribadi/Asuransi/Rujukan di layar = menu RJ; [laporan](../task/report/frontend/FE-KSK-PO-002.md)), `FE-KSK-PO-003` ✅ 2026-11-20 (alur lengkap sampai Selesai; [laporan](../task/report/frontend/FE-KSK-PO-003.md)) |
| AC 2 — pencocokan tanpa daftar, pesan umum, nama tersamar | `KSK-DEC-031`; `KSK-PO-FE-002` | PO.4.4; `KSK-VAL-PO-004` | `BE-KSK-PO-004` | `FE-KSK-PO-002` | PO-T.1 baris AC 2 | ✅ `BE-KSK-PO-004` ✅ 2026-11-19 ([laporan](../task/report/backend/BE-KSK-PO-004.md)); `FE-KSK-PO-002` ✅ 2026-11-20 (layar: hanya nama dan nomor kartu tersamar, tanggal lahir salah → pesan umum `404`; rework 2026-11-23: No. RM terformat `00-00-00-01` dan tetap cocok; [laporan](../task/report/frontend/FE-KSK-PO-002.md)) |
| AC 3 — Menunggu Check-in, satu kuota, tidak muncul di layanan | `KSK-DEC-030` | `INV-PO-01`, `02`; PO.4.3 | `BE-KSK-PO-002`, `005` | — | PO-T.1 baris AC 3 | ✅ 2026-11-20 — `BE-KSK-PO-002` ✅ ([laporan](../task/report/backend/BE-KSK-PO-002.md)), `BE-KSK-PO-005` ✅ ([laporan](../task/report/backend/BE-KSK-PO-005.md), T1, T1d, T1e) |
| AC 4 — pasien baru tanpa master sebelum check-in | `KSK-DEC-032` | `INV-PO-06` | `BE-KSK-PO-005` | — | PO-T.1 baris AC 4 | ✅ 2026-11-20 — `BE-KSK-PO-005` ✅ ([laporan](../task/report/backend/BE-KSK-PO-005.md), T4b, DB2) |
| AC 5 — kode, email, tombol WhatsApp | `KSK-DEC-041`, `042` | `INT-PO-03`, `04`; `KSK-PO-FE-003` | `BE-KSK-PO-003`, `005` | `FE-KSK-PO-003` | PO-T.1 baris AC 5 | ✅ `BE-KSK-PO-003` ✅ 2026-11-19 ([laporan](../task/report/backend/BE-KSK-PO-003.md)); `BE-KSK-PO-005` ✅ 2026-11-20 (email terkirim; SMTP mati → `emailStatus = 2`; [laporan](../task/report/backend/BE-KSK-PO-005.md)); `FE-KSK-PO-003` ✅ 2026-11-20 (email SMTP uji berisi kode tanpa NIK; tombol `wa.me/62…` berisi kode, tanggal, poli; [laporan](../task/report/frontend/FE-KSK-PO-003.md)) |
| AC 6 — penolakan kontak kosong, captcha, ganda, > 30 hari, batas laju | `KSK-DEC-038`, `043`, `044` | `KSK-VAL-PO-007`, `010`, `020`, `023`, `033` | `BE-KSK-PO-003`, `004`, `005` | `FE-KSK-PO-002`, `003` | PO-T.1 baris AC 6 | ✅ `BE-KSK-PO-003` ✅, `BE-KSK-PO-004` ✅ 2026-11-19 (captcha, batas laju pencocokan), `BE-KSK-PO-005` ✅ 2026-11-20 ([laporan](../task/report/backend/BE-KSK-PO-005.md)); `FE-KSK-PO-002` ✅ 2026-11-20 (kontak kosong/tidak sah, captcha pencocokan, tanggal > 30 hari di layar; [laporan](../task/report/frontend/FE-KSK-PO-002.md)); `FE-KSK-PO-003` ✅ 2026-11-20 (captcha wajib sebelum Kirim, `409` ganda nyata kembali ke Data Kunjungan dengan pesan; [laporan](../task/report/frontend/FE-KSK-PO-003.md)) |
| AC 7 — check-in kode + tanggal lahir, tiket, kode sekali pakai | `KSK-DEC-033`, `036` | PO.4.6 | `BE-KSK-PO-006`, `007` | `FE-KSK-PO-005` | PO-T.2 baris AC 7 | ✅ `BE-KSK-PO-006` ✅ 2026-11-25 (kode + tanggal lahir `200` hari H; [laporan](../task/report/backend/BE-KSK-PO-006.md)); `BE-KSK-PO-007` ✅ 2026-11-25 (kunjungan appointment sumber 7, status 2, token dan kode sekali pakai, runtime hari H; [laporan](../task/report/backend/BE-KSK-PO-007.md)); `FE-KSK-PO-005` ✅ 2026-11-25 (kode sampai tiket, nyata; [laporan](../task/report/frontend/FE-KSK-PO-005.md)) |
| AC 8 — sebelum hari H, tanggal lahir salah | `KSK-DEC-033` | `KSK-VAL-PO-041`, `042` | `BE-KSK-PO-006` | `FE-KSK-PO-005` | PO-T.2 baris AC 8 | ✅ `BE-KSK-PO-006` ✅ 2026-11-25 (`409` sebelum hari H, `404` tanggal lahir salah sebelum dan pada hari H; [laporan](../task/report/backend/BE-KSK-PO-006.md)); `FE-KSK-PO-005` ✅ 2026-11-25 (`404` seragam dan `409` dengan tanggal tampil di kiosk, nyata; [laporan](../task/report/frontend/FE-KSK-PO-005.md)) |
| AC 9 — pasien baru: master setelah pindai; NIK lama memakai No. RM lama | `KSK-DEC-032`, `039` | PO.4.6 | `BE-KSK-PO-007` | `FE-KSK-PO-006` | PO-T.2 baris AC 9 | ✅ `BE-KSK-PO-007` ✅ 2026-11-25 (pasien baru: master dari `POST patients/kiosk`, check-in `200`, runtime hari H; [laporan](../task/report/backend/BE-KSK-PO-007.md)); `FE-KSK-PO-006` ✅ 2026-11-25 (pindai KTP/KIA → master baru dari kartu, NIK lama memakai master yang ada, coba ulang tanpa master ganda; [laporan](../task/report/frontend/FE-KSK-PO-006.md)) |
| AC 10 — Lupa Kode | `KSK-DEC-040` | `KSK-VAL-PO-041`, `043` | `BE-KSK-PO-006` | `FE-KSK-PO-005` | PO-T.2 baris AC 10 | ✅ `BE-KSK-PO-006` ✅ 2026-11-25 (Lupa Kode NIK dan No. RM `200` hari H; [laporan](../task/report/backend/BE-KSK-PO-006.md)); `FE-KSK-PO-005` ✅ 2026-11-25 (Lupa Kode sampai tiket, nyata; [laporan](../task/report/frontend/FE-KSK-PO-005.md)) |
| AC 11 — Tidak Datang 23.59, idempoten | `KSK-DEC-037` | PO.4.7 | `BE-KSK-PO-008` | — | PO-T.3 baris AC 11 | ✅ 2026-11-23 — `BE-KSK-PO-008` (harness: Tidak Datang, idempoten, hari tertinggal; runtime 2026-11-23: 3 pendaftaran 21 Nov status 4, satu riwayat `NO_SHOW` Sistem, kode `404`, kuota 0, menyala ulang tidak menambah riwayat; [laporan](../task/report/backend/BE-KSK-PO-008.md)) |
| AC 12 — Didaftarkan lewat Jalur Lain | `KSK-DEC-045` | PO.4.7 | `BE-KSK-PO-008` | — | PO-T.3 baris AC 12 | ✅ 2026-11-20 — `BE-KSK-PO-008` (runtime: pasien lama dan pasien baru lewat NIK master status 3, kode mati, kuota tidak ganda; [laporan](../task/report/backend/BE-KSK-PO-008.md)) |
| AC 13 — jadwal tidak berlaku, arahkan ke loket | `KSK-DEC-046` | `KSK-VAL-PO-053` | `BE-KSK-PO-007` | `FE-KSK-PO-005` | PO-T.2 baris AC 13 | ✅ `BE-KSK-PO-007` ✅ 2026-11-25 (`409` `053` jadwal dinonaktifkan dan `054` kuota penuh, runtime hari H; [laporan](../task/report/backend/BE-KSK-PO-007.md)); `FE-KSK-PO-005` ✅ 2026-11-25 (pesan `053`, Check-in nonaktif, arahan loket; [laporan](../task/report/frontend/FE-KSK-PO-005.md)) |
| AC 14 — consent online tersimpan dan melekat saat check-in | `KSK-DEC-034` | PO.4.6 | `BE-KSK-PO-005`, `007` | `FE-KSK-PO-003` | PO-T.1 dan PO-T.2 baris AC 14 | ✅ `BE-KSK-PO-005` ✅ 2026-11-20 (consent tersimpan di pendaftaran; [laporan](../task/report/backend/BE-KSK-PO-005.md)); `FE-KSK-PO-003` ✅ 2026-11-20 (consent ditandatangani di HP; [laporan](../task/report/frontend/FE-KSK-PO-003.md)); `BE-KSK-PO-007` ✅ 2026-11-25 (`RegPatientGeneralConsent` dengan `SignedAt` asli melekat pada kunjungan check-in, runtime hari H; [laporan](../task/report/backend/BE-KSK-PO-007.md)) |
| AC 15 — berkas > 5 MB ditolak | `KSK-DEC-035` | `KSK-VAL-PO-029`, `030` | `BE-KSK-PO-005` | `FE-KSK-PO-003` | PO-T.1 baris AC 15 | ✅ `BE-KSK-PO-005` ✅ 2026-11-20 ([laporan](../task/report/backend/BE-KSK-PO-005.md), T5p, T5q); `FE-KSK-PO-003` ✅ 2026-11-20 (6 MB dan `.exe` ditolak di layar, > 3 berkas ditolak, 2 berkas sah tersimpan; [laporan](../task/report/frontend/FE-KSK-PO-003.md)) |
| AC 16 — hari ini ditolak, H+1 s.d. H+30 diterima | `KSK-DEC-047` | `KSK-VAL-PO-020` | `BE-KSK-PO-005` | `FE-KSK-PO-002` | PO-T.1 baris AC 6/16 | ✅ `BE-KSK-PO-005` ✅ 2026-11-20 ([laporan](../task/report/backend/BE-KSK-PO-005.md), T5f, T5g); `FE-KSK-PO-002` ✅ 2026-11-20 (kalender: hari ini dan H+31 nonaktif, H+1 s.d. H+30 aktif; [laporan](../task/report/frontend/FE-KSK-PO-002.md)) |
| AC 17 — NIK KTP/KIA wajib sama; SIM ditolak; master dari pindai | `KSK-DEC-048`, `053` | `KSK-VAL-PO-050` s.d. `052`; `CAP-PO-37` | `BE-KSK-PO-007` | `FE-KSK-PO-004`, `006` | PO-T.2 baris AC 17 | ✅ `FE-KSK-PO-004` ✅ 2026-11-19 ([laporan](../task/report/frontend/FE-KSK-PO-004.md)); `BE-KSK-PO-007` ✅ 2026-11-25 (NIK master berbeda `400` `KSK-VAL-PO-052`; [laporan](../task/report/backend/BE-KSK-PO-007.md)); `FE-KSK-PO-006` ✅ 2026-11-25 (hanya KTP/KIA, NIK terkunci, `051`/`052` di kiosk, master dari pindai; [laporan](../task/report/frontend/FE-KSK-PO-006.md)) |
| AC 18 — daftar petugas dengan hak akses | `KSK-DEC-050`; `KSK-PO-AS-002` | PO-FE.3 `FE-KSK-PO-03`; PO-P.2 | `BE-KSK-PO-009` | `FE-KSK-PO-007` | PO-T.4 baris AC 18 | ✅ `BE-KSK-PO-009` ✅ 2026-11-20 ([laporan](../task/report/backend/BE-KSK-PO-009.md); aksi Akses Role lengkap lewat `BE-KSK-PO-010`); `FE-KSK-PO-007` ✅ 2026-11-20 (daftar, filter, cari, kode tidak tampil; tanpa hak: menu tersembunyi + `AccessDeniedGate`; [laporan](../task/report/frontend/FE-KSK-PO-007.md)) |
| AC 19 — ubah jadwal/kontak, pertukaran kuota, riwayat | `KSK-DEC-051`; `KSK-DSN-PO-016` | `KSK-VAL-PO-060` s.d. `067` | `BE-KSK-PO-010` | `FE-KSK-PO-008` | PO-T.4 baris AC 19 | ✅ `BE-KSK-PO-010` ✅ 2026-11-20 (pertukaran kuota, riwayat, kode tetap; [laporan](../task/report/backend/BE-KSK-PO-010.md)); `FE-KSK-PO-008` ✅ 2026-11-20 (Ubah Jadwal ke tanggal lain dengan dokter lain di poli yang sama, kuota ±1, jadwal penuh ditolak, Ubah Kontak dengan riwayat; [laporan](../task/report/frontend/FE-KSK-PO-008.md)) |
| AC 20 — batal dengan alasan, kode mati, kuota lepas | `KSK-DEC-052` | `KSK-VAL-PO-070` s.d. `072` | `BE-KSK-PO-010` | `FE-KSK-PO-008` | PO-T.4 baris AC 20 | ✅ `BE-KSK-PO-010` ✅ 2026-11-20 (status 5, kode mati di kiosk, kuota lepas; [laporan](../task/report/backend/BE-KSK-PO-010.md)); `FE-KSK-PO-008` ✅ 2026-11-20 (Batalkan dengan alasan wajib, tombol hilang pada status akhir; [laporan](../task/report/frontend/FE-KSK-PO-008.md)) |
| `KSK-DEC-057` — faskes perujuk hanya mitra pada tanggal kunjungan (r1.1) | `KSK-DEC-057`, `058` | `KSK-DSN-PO-018`; `KSK-VAL-PO-073`, `074` | `BE-KSK-PO-011` | `FE-KSK-PO-009` | PO-T.1 tiga baris `KSK-DEC-057` | ✅ `BE-KSK-PO-011` ✅ 2026-11-25 (opsi faskes hanya mitra pada `visitDate`, `074`, kirim `073`, runtime 10/10; [laporan](../task/report/backend/BE-KSK-PO-011.md)); `FE-KSK-PO-009` ✅ 2026-11-25 (pencarian membawa `visitDate`, pesan kosong faskes mitra, ganti tanggal mengosongkan faskes, `073` tampil; browser 375 px 14/14; [laporan](../task/report/frontend/FE-KSK-PO-009.md)) |

## 2. Functional requirement → task

| FR (`04-prd-to-mvp.md` PO-10) | Task |
| --- | --- |
| `FR-KSK-PO-001` | `BE-KSK-PO-001` ✅ 2026-11-19 ([laporan](../task/report/backend/BE-KSK-PO-001.md)) |
| `FR-KSK-PO-002` | `BE-KSK-PO-002` ✅ 2026-11-20 ([laporan](../task/report/backend/BE-KSK-PO-002.md)); AC 2 runtime terbukti lewat `BE-KSK-PO-005` |
| `FR-KSK-PO-003`, `004` | `BE-KSK-PO-008` ✅ 2026-11-23 ([laporan](../task/report/backend/BE-KSK-PO-008.md)) |
| `FR-KSK-PO-010`, `011`, `014` | `BE-KSK-PO-004` ✅ 2026-11-19 ([laporan](../task/report/backend/BE-KSK-PO-004.md)) |
| `FR-KSK-PO-012` | `BE-KSK-PO-005` ✅ 2026-11-20 ([laporan](../task/report/backend/BE-KSK-PO-005.md)) |
| `FR-KSK-PO-013` | `BE-KSK-PO-003` ✅ 2026-11-19 ([laporan](../task/report/backend/BE-KSK-PO-003.md)) |
| `FR-KSK-PO-015` | `BE-KSK-PO-011` ✅ 2026-11-25 ([laporan](../task/report/backend/BE-KSK-PO-011.md)) |
| `FR-KSK-PO-020` | `BE-KSK-PO-006` ✅ 2026-11-25 ([laporan](../task/report/backend/BE-KSK-PO-006.md)) |
| `FR-KSK-PO-021` | `BE-KSK-PO-007` ✅ 2026-11-25 ([laporan](../task/report/backend/BE-KSK-PO-007.md)) |
| `FR-KSK-PO-030` | `BE-KSK-PO-009` ✅ 2026-11-20 ([laporan](../task/report/backend/BE-KSK-PO-009.md)) |
| `FR-KSK-PO-031` | `BE-KSK-PO-010` ✅ 2026-11-20 ([laporan](../task/report/backend/BE-KSK-PO-010.md)) |
| `FR-KSK-PO-032` | `BE-KSK-PO-009` ✅ 2026-11-20 (Resource `Read`, `Update`, `Cancel`), `BE-KSK-PO-010` ✅ 2026-11-20 (batal) |
| `FR-KSK-PO-040` | `FE-KSK-PO-001` ✅ 2026-11-20 ([laporan](../task/report/frontend/FE-KSK-PO-001.md)) |
| `FR-KSK-PO-041`, `042` | `FE-KSK-PO-002` ✅ 2026-11-20 ([laporan](../task/report/frontend/FE-KSK-PO-002.md)), `FE-KSK-PO-003` ✅ 2026-11-20 ([laporan](../task/report/frontend/FE-KSK-PO-003.md)) |
| `FR-KSK-PO-043` | `FE-KSK-PO-009` ✅ 2026-11-25 ([laporan](../task/report/frontend/FE-KSK-PO-009.md)) |
| `FR-KSK-PO-050` | `FE-KSK-PO-005` ✅ 2026-11-25 ([laporan](../task/report/frontend/FE-KSK-PO-005.md)) |
| `FR-KSK-PO-051` | `FE-KSK-PO-004` ✅ 2026-11-19 ([laporan](../task/report/frontend/FE-KSK-PO-004.md)), `FE-KSK-PO-006` ✅ 2026-11-25 ([laporan](../task/report/frontend/FE-KSK-PO-006.md)) |
| `FR-KSK-PO-060` | `FE-KSK-PO-007` ✅ 2026-11-20 ([laporan](../task/report/frontend/FE-KSK-PO-007.md)) |
| `FR-KSK-PO-061` | `FE-KSK-PO-008` ✅ 2026-11-20 ([laporan](../task/report/frontend/FE-KSK-PO-008.md)) |

## 3. Keputusan desain → task

| Keputusan | Task |
| --- | --- |
| `KSK-DSN-PO-001`, `002`, `003`, `014` | `BE-KSK-PO-001` ✅ 2026-11-19 ([laporan](../task/report/backend/BE-KSK-PO-001.md)) (`014` juga `BE-KSK-PO-007` ✅ 2026-11-25) |
| `KSK-DSN-PO-004`, `011`, `012` | `BE-KSK-PO-005` ✅ 2026-11-20, `FE-KSK-PO-002` ✅ 2026-11-20 ([laporan](../task/report/frontend/FE-KSK-PO-002.md)), `FE-KSK-PO-003` ✅ 2026-11-20 ([laporan](../task/report/frontend/FE-KSK-PO-003.md)) |
| `KSK-DSN-PO-005`, `013` | `BE-KSK-PO-004` ✅ 2026-11-19 ([laporan](../task/report/backend/BE-KSK-PO-004.md)) |
| `KSK-DSN-PO-006`, `007` | `BE-KSK-PO-003` ✅ 2026-11-19 ([laporan](../task/report/backend/BE-KSK-PO-003.md)) |
| `KSK-DSN-PO-008`, `010` | `BE-KSK-PO-006` ✅ 2026-11-25, `007` ✅ 2026-11-25 ([laporan](../task/report/backend/BE-KSK-PO-007.md)) |
| `KSK-DSN-PO-009` | `BE-KSK-PO-002` ✅ 2026-11-20 |
| `KSK-DSN-PO-015` | `BE-KSK-PO-008` ✅ 2026-11-23 |
| `KSK-DSN-PO-016` | `BE-KSK-PO-010` ✅ 2026-11-20, `FE-KSK-PO-008` ✅ 2026-11-20 (pilihan jadwal hanya poli yang sama; [laporan](../task/report/frontend/FE-KSK-PO-008.md)) |
| `KSK-DSN-PO-017` | `BE-KSK-PO-009` ✅ 2026-11-20, `FE-KSK-PO-007` ✅ 2026-11-20 ([laporan](../task/report/frontend/FE-KSK-PO-007.md)) |
| `KSK-DSN-PO-018` | `BE-KSK-PO-011` ✅ 2026-11-25, `FE-KSK-PO-009` ✅ 2026-11-25 |
| `KSK-PO-FE-005` (letak menu) | `FE-KSK-PO-007` ✅ 2026-11-20 (butir "Pendaftaran Online" tepat di bawah "Reservasi Prioritas"; [laporan](../task/report/frontend/FE-KSK-PO-007.md)) |

## 4. Coverage gap

| Gap | Keterangan |
| --- | --- |
| — | Setiap AC 1–20, `FR-KSK-PO-*`, dan `KSK-DSN-PO-*` punya sekurang-kurangnya satu task |
| Go-live | `KSK-PO-OQ-010`, `015`, `016` bukan task build; ditutup IT. Centang Akses Role dilakukan admin |
