# Acceptance Test Matrix — Nurse View

| Field | Nilai |
|---|---|
| `last_changed_in` | `0.1.0` · status `approved` — Rizki Gunawan, 2026-10-28 (`NV-DEC-028`) |
| Owner | Rizki Gunawan; UAT oleh tim terpisah |
| Catatan | Status dipisah: Implementation / DevVerif / UAT. Agent tidak menulis unit test; bukti DevVerif berupa uji layar/HTTP oleh Rizki dan bukti UAT dari tim UAT. Dilarang SQL langsung untuk membuat skenario lulus |

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `NV-AC-001` | TV lantai 3 menampilkan 20 bed dalam dua kelompok bangsal; kamar tanpa `FloorCode` tidak tampil | UAT layar | Tangkapan layar + daftar kamar lantai 3 dari master |
| `NV-AC-002` | Kartu menampilkan "B.S. · L · 64 th"; response `GET /board` tidak memuat nama lengkap, No. RM, tanggal lahir | HTTP + layar | JSON response disimpan; pencarian teks nama lengkap → nol hasil |
| `NV-AC-003` | TTV EWS `Critical` → kartu merah berkedip, teratas di bangsal, tanpa bunyi | UAT layar | Rekaman layar |
| `NV-AC-004` | Interval kosong → tidak ada tanda lewat jadwal, hanya "diukur 9 jam lalu" | UAT layar | Tangkapan layar + `GET /vital-sign-intervals` `IsConfigured = false` |
| `NV-AC-005` | `Medium` = 240, TTV 08.00 → pukul 12.30 "lewat jadwal 30 menit" | UAT layar | Tangkapan layar berjam |
| `NV-AC-006` | Putus jaringan > 2 menit → pita abu-abu; tersambung → pita hilang | UAT perangkat | Rekaman layar |
| `NV-AC-007` | TTV dibatalkan → kartu kembali ke TTV sah sebelumnya | HTTP + layar | Response cancel + tangkapan layar |
| `NV-AC-008` | Episode tanpa PJ → "Belum ada PJ" | UAT layar | Tangkapan layar |
| `NV-AC-009` | Roster pagi berisi 3 perawat → header menampilkan ketiganya | UAT layar | Tangkapan layar + roster `Published` |
| `NV-AC-010` | Bed dibersihkan / dipesan → kartu abu-abu berlabel benar | UAT layar | Tangkapan layar |
| `NV-AC-011` | Morse 30 tanpa ikon; Morse 50 ikon risiko jatuh | UAT layar | Tangkapan layar + hasil instrumen |
| `NV-AC-012` | Alergi Amoxicillin → ikon "Alergi" saja; nama zat tidak ada di response | HTTP + layar | JSON response; pencarian "Amoxicillin" → nol hasil |
| `NV-AC-013` | Butuh isolasi di bed biasa → ikon isolasi + "Bed tidak sesuai isolasi" | UAT layar | Tangkapan layar |
| `NV-AC-014` | Pasien pindah 301-A → 402-B → hilang di lantai 3, muncul di lantai 4 ≤ 60 detik | UAT dua TV | Rekaman dua layar |
| `NV-AC-015` | **Gagal:** perangkat lantai 3 tidak dapat membaca lantai 4 (tidak ada parameter lantai; mengubah request tidak berpengaruh) | HTTP | Response tetap lantai 3 |
| `NV-AC-016` | **Gagal:** perangkat dinonaktifkan → permintaan berikutnya `403`, TV ke layar login | HTTP + layar | Response `403` + rekaman layar |
| `NV-AC-017` | Roster kosong / `OrganizationUnitId` kosong → "Roster shift ini belum tersedia" | UAT layar | Tangkapan layar |
| `NV-AC-018` | Kata sandi tampil sekali; detail perangkat sesudahnya tanpa kata sandi | HTTP + layar | Dua response JSON |
| `NV-AC-019` | TTV `Critical` disimpan → kartu berubah tanpa menunggu 60 detik | UAT layar (setelah `NV-OQ-016`) | Rekaman berjam |
| `NV-AC-020` | Pasien pulang → kartu berubah ≤ 60 detik | UAT layar | Rekaman berjam |
| `NV-AC-021` | Kamar ber-`FloorCode` tanpa `FloorPlanRoomKey` tetap tampil | UAT layar | Tangkapan layar + data kamar |
| `NV-AC-022` | **Gagal:** akun perangkat mencoba `JoinInpatientBedSelection` → ditolak; sinyal `NurseViewBoardChanged` tanpa identitas/nilai TTV | Hub | Log hub + payload event |
| `VAL-NV-02` | **Gagal:** daftarkan perangkat dengan lantai "99" → `422` | HTTP | Response |
| `VAL-NV-04` | **Gagal:** dua admin mengubah perangkat bersamaan → yang kedua `409` | HTTP | Dua response |
| `VAL-NV-05`/`06` | **Gagal:** buat login dua kali → `409`; atur ulang tanpa login → `409` | HTTP | Response |
| `VAL-NV-09`–`13` | **Gagal:** tingkat ganda, menit 0, rujukan kosong, tanggal besok → `400` | HTTP | Response |
| `VAL-NV-14` | **Gagal:** akun manusia (termasuk Administrator) membuka `GET /board` → `403` | HTTP | Response |
| `NV-AC-023` | **Regresi:** perawat login → dashboard; tanpa klaim/cookie Nurse View | UAT login | Rekaman login + cookie |
| `NV-AC-024` | **Regresi:** akun Kiosk login → `/kiosk` | UAT login | Rekaman |
| `NV-AC-025` | **Regresi:** akun display antrean login → `/queue-display`, papan antrean tampil | UAT login | Rekaman |
| `NV-AC-026` | **Regresi:** SuperAdmin/Administrator login normal; `GET /board` tetap `403` | HTTP + UAT | Response |
| `NV-AC-027` | **Regresi:** login gagal (sandi salah/terkunci) — pesan & kode sama seperti sebelum perubahan | HTTP | Response sebelum vs sesudah |
| Logging | Kata sandi tidak muncul di log setelah buat/atur ulang login | Pemeriksaan log | Potongan log tanpa kata sandi |
