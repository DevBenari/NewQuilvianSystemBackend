# Traceability — Pendaftaran Pasien Online dan Check-in Kiosk (Amendment PO)

| Field | Nilai |
| --- | --- |
| Traceability ID | `KSK-PO-TRACE-001` |
| Revision | `1` |
| Status | `draft` — mengikuti status roadmap `KSK-PO-RM-BE-001` r1 dan `KSK-PO-RM-FE-001` r1 |
| Blueprint | `KSK-BP-001` revisi `2`, `approved` (`KSK-DEC-054`) |
| Kontrak | `KSK-CONTRACT-v2` `approved` |
| SHA baseline | BE `5e777833`, FE `5f18e9f04` |
| Roadmap | [online-registration-backend-roadmap.md](online-registration-backend-roadmap.md), [online-registration-frontend-roadmap.md](online-registration-frontend-roadmap.md) |

Kolom **Status** diisi task builder sesudah laporan task ada (`✅`, `🟡`, `⛔`, atau kosong).

## 1. Acceptance criteria → desain → task → bukti

| AC (§13.9) | Keputusan | Desain/kontrak | Task backend | Task frontend | Bukti yang diharapkan | Status |
| --- | --- | --- | --- | --- | --- | --- |
| AC 1 — halaman tanpa login, urutan langkah sama dengan menu RJ | `KSK-DEC-029` | PO-FE.3 `FE-KSK-PO-01`; `KSK-PO-FE-001` | `BE-KSK-PO-004` | `FE-KSK-PO-001`, `002`, `003` | PO-T.1 baris AC 1 | |
| AC 2 — pencocokan tanpa daftar, pesan umum, nama tersamar | `KSK-DEC-031`; `KSK-PO-FE-002` | PO.4.4; `KSK-VAL-PO-004` | `BE-KSK-PO-004` | `FE-KSK-PO-002` | PO-T.1 baris AC 2 | |
| AC 3 — Menunggu Check-in, satu kuota, tidak muncul di layanan | `KSK-DEC-030` | `INV-PO-01`, `02`; PO.4.3 | `BE-KSK-PO-002`, `005` | — | PO-T.1 baris AC 3 | |
| AC 4 — pasien baru tanpa master sebelum check-in | `KSK-DEC-032` | `INV-PO-06` | `BE-KSK-PO-005` | — | PO-T.1 baris AC 4 | |
| AC 5 — kode, email, tombol WhatsApp | `KSK-DEC-041`, `042` | `INT-PO-03`, `04`; `KSK-PO-FE-003` | `BE-KSK-PO-003`, `005` | `FE-KSK-PO-003` | PO-T.1 baris AC 5 | |
| AC 6 — penolakan kontak kosong, captcha, ganda, > 30 hari, batas laju | `KSK-DEC-038`, `043`, `044` | `KSK-VAL-PO-007`, `010`, `020`, `023`, `033` | `BE-KSK-PO-003`, `004`, `005` | `FE-KSK-PO-002`, `003` | PO-T.1 baris AC 6 | |
| AC 7 — check-in kode + tanggal lahir, tiket, kode sekali pakai | `KSK-DEC-033`, `036` | PO.4.6 | `BE-KSK-PO-006`, `007` | `FE-KSK-PO-005` | PO-T.2 baris AC 7 | |
| AC 8 — sebelum hari H, tanggal lahir salah | `KSK-DEC-033` | `KSK-VAL-PO-041`, `042` | `BE-KSK-PO-006` | `FE-KSK-PO-005` | PO-T.2 baris AC 8 | |
| AC 9 — pasien baru: master setelah pindai; NIK lama memakai No. RM lama | `KSK-DEC-032`, `039` | PO.4.6 | `BE-KSK-PO-007` | `FE-KSK-PO-006` | PO-T.2 baris AC 9 | |
| AC 10 — Lupa Kode | `KSK-DEC-040` | `KSK-VAL-PO-041`, `043` | `BE-KSK-PO-006` | `FE-KSK-PO-005` | PO-T.2 baris AC 10 | |
| AC 11 — Tidak Datang 23.59, idempoten | `KSK-DEC-037` | PO.4.7 | `BE-KSK-PO-008` | — | PO-T.3 baris AC 11 | |
| AC 12 — Didaftarkan lewat Jalur Lain | `KSK-DEC-045` | PO.4.7 | `BE-KSK-PO-008` | — | PO-T.3 baris AC 12 | |
| AC 13 — jadwal tidak berlaku, arahkan ke loket | `KSK-DEC-046` | `KSK-VAL-PO-053` | `BE-KSK-PO-007` | `FE-KSK-PO-005` | PO-T.2 baris AC 13 | |
| AC 14 — consent online tersimpan dan melekat saat check-in | `KSK-DEC-034` | PO.4.6 | `BE-KSK-PO-005`, `007` | `FE-KSK-PO-003` | PO-T.1 dan PO-T.2 baris AC 14 | |
| AC 15 — berkas > 5 MB ditolak | `KSK-DEC-035` | `KSK-VAL-PO-029`, `030` | `BE-KSK-PO-005` | `FE-KSK-PO-003` | PO-T.1 baris AC 15 | |
| AC 16 — hari ini ditolak, H+1 s.d. H+30 diterima | `KSK-DEC-047` | `KSK-VAL-PO-020` | `BE-KSK-PO-005` | `FE-KSK-PO-002` | PO-T.1 baris AC 6/16 | |
| AC 17 — NIK KTP/KIA wajib sama; SIM ditolak; master dari pindai | `KSK-DEC-048`, `053` | `KSK-VAL-PO-050` s.d. `052`; `CAP-PO-37` | `BE-KSK-PO-007` | `FE-KSK-PO-004`, `006` | PO-T.2 baris AC 17 | |
| AC 18 — daftar petugas dengan hak akses | `KSK-DEC-050`; `KSK-PO-AS-002` | PO-FE.3 `FE-KSK-PO-03`; PO-P.2 | `BE-KSK-PO-009` | `FE-KSK-PO-007` | PO-T.4 baris AC 18 | |
| AC 19 — ubah jadwal/kontak, pertukaran kuota, riwayat | `KSK-DEC-051`; `KSK-DSN-PO-016` | `KSK-VAL-PO-060` s.d. `067` | `BE-KSK-PO-010` | `FE-KSK-PO-008` | PO-T.4 baris AC 19 | |
| AC 20 — batal dengan alasan, kode mati, kuota lepas | `KSK-DEC-052` | `KSK-VAL-PO-070` s.d. `072` | `BE-KSK-PO-010` | `FE-KSK-PO-008` | PO-T.4 baris AC 20 | |

## 2. Functional requirement → task

| FR (`04-prd-to-mvp.md` PO-10) | Task |
| --- | --- |
| `FR-KSK-PO-001` | `BE-KSK-PO-001` |
| `FR-KSK-PO-002` | `BE-KSK-PO-002` |
| `FR-KSK-PO-003`, `004` | `BE-KSK-PO-008` |
| `FR-KSK-PO-010`, `011`, `014` | `BE-KSK-PO-004` |
| `FR-KSK-PO-012` | `BE-KSK-PO-005` |
| `FR-KSK-PO-013` | `BE-KSK-PO-003` |
| `FR-KSK-PO-020` | `BE-KSK-PO-006` |
| `FR-KSK-PO-021` | `BE-KSK-PO-007` |
| `FR-KSK-PO-030` | `BE-KSK-PO-009` |
| `FR-KSK-PO-031` | `BE-KSK-PO-010` |
| `FR-KSK-PO-032` | `BE-KSK-PO-009` (Resource), `BE-KSK-PO-010` (batal) |
| `FR-KSK-PO-040` | `FE-KSK-PO-001` |
| `FR-KSK-PO-041`, `042` | `FE-KSK-PO-002`, `FE-KSK-PO-003` |
| `FR-KSK-PO-050` | `FE-KSK-PO-005` |
| `FR-KSK-PO-051` | `FE-KSK-PO-004`, `FE-KSK-PO-006` |
| `FR-KSK-PO-060` | `FE-KSK-PO-007` |
| `FR-KSK-PO-061` | `FE-KSK-PO-008` |

## 3. Keputusan desain → task

| Keputusan | Task |
| --- | --- |
| `KSK-DSN-PO-001`, `002`, `003`, `014` | `BE-KSK-PO-001` (`014` juga `BE-KSK-PO-007`) |
| `KSK-DSN-PO-004`, `011`, `012` | `BE-KSK-PO-005`, `FE-KSK-PO-002`, `003` |
| `KSK-DSN-PO-005`, `013` | `BE-KSK-PO-004` |
| `KSK-DSN-PO-006`, `007` | `BE-KSK-PO-003` |
| `KSK-DSN-PO-008`, `010` | `BE-KSK-PO-006`, `007` |
| `KSK-DSN-PO-009` | `BE-KSK-PO-002` |
| `KSK-DSN-PO-015` | `BE-KSK-PO-008` |
| `KSK-DSN-PO-016` | `BE-KSK-PO-010`, `FE-KSK-PO-008` |
| `KSK-DSN-PO-017` | `BE-KSK-PO-009`, `FE-KSK-PO-007` |
| `KSK-PO-FE-005` (letak menu) | `FE-KSK-PO-007` |

## 4. Coverage gap

| Gap | Keterangan |
| --- | --- |
| — | Setiap AC 1–20, `FR-KSK-PO-*`, dan `KSK-DSN-PO-*` punya sekurang-kurangnya satu task |
| Go-live | `KSK-PO-OQ-010`, `015`, `016` bukan task build; ditutup IT. Centang Akses Role dilakukan admin |
