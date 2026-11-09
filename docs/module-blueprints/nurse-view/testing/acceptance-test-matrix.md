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

## Revision 2 — Tablet bed dan panggilan pasien

| Field | Nilai |
|---|---|
| `last_changed_in` | `0.2.0` · status `approved` — Rizki Gunawan, 2026-11-05 (`NV-DEC-055`) |
| Owner | Rizki Gunawan; UAT oleh tim terpisah |
| Catatan | Ketentuan revision 1 tetap: status Implementation / DevVerif / UAT dipisah; agent tidak menulis unit test; dilarang SQL langsung untuk membuat skenario lulus. Data pasien di skenario memakai pasien uji, bukan pasien asli |

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `NV-AC-028` | Tablet 305-A menekan "Panggil perawat" → strip TV dan konsol lantai 3 menampilkan 305-A tanpa menunggu muat ulang 60 detik | UAT dua layar | Rekaman layar berjam TV + konsol |
| `NV-AC-029` | Tekan biasa 10.00.00, tekan lagi, lalu Darurat 10.00.50 → satu item, Darurat, menunggu sejak 10.00.00 | HTTP + layar | Response `POST /calls` ketiga + `GET /active` |
| `NV-AC-030` | 305-A biasa, 318-B biasa, 331-C Darurat → urutan 331-C, 305-A, 318-B | UAT layar | Tangkapan konsol dan strip |
| `NV-AC-031` | TV di halaman 1 dari 3, 331-C di halaman 3 memanggil → strip langsung menampilkan 331-C dan tidak ikut berganti halaman | UAT layar | Rekaman TV 30 detik |
| `NV-AC-032` | Sari mengklaim, Dewi hanya bisa "Ambil alih"; sesudah ambil alih strip "Dewi menuju"; jejak mencatat keduanya | HTTP + layar | Response klaim kedua `409` `VAL-NV-33`; response ambil alih; jejak panggilan |
| `NV-AC-033` | Sari melepas → `Waiting`, jam awal tetap | HTTP | Response `release` |
| `NV-AC-034` | Sentuh "Perawat hadir" < 3 detik → tetap aktif; tahan 3 detik → `Handled`, "tidak terverifikasi identitas" tercatat | UAT tablet | Rekaman tablet + jejak `NurseArrived` |
| `NV-AC-035` | Dua panggilan belum diklaim → pengingat berbunyi; semua diklaim → senyap; pengingat ditunda tetapi panggilan baru tetap berbunyi | UAT konsol (audio) | Rekaman video berbunyi |
| `NV-AC-036` | **Gagal:** pengguna tanpa izin membuka konsol/klaim → `403`; Sari berizin dengan roster kosong mengklaim lantai 4 → diterima | HTTP + layar | Dua response |
| `NV-AC-037` | Response strip TV tanpa identitas; response konsol hanya inisial · jenis kelamin · umur; tidak ada nama lengkap/No. RM di keduanya | HTTP | JSON tersimpan; pencarian nama lengkap pasien uji → nol hasil |
| `NV-AC-038` | 331-C dipindah ke 402-B 10.05.00 → paling lambat 10.06.00 `ClosedBySystem` "pasien pindah"; tablet 331-C "Bed belum berpenghuni"; lantai 4 tanpa panggilan | UAT + HTTP | Rekaman berjam + jejak `ClosedBySystem` |
| `NV-AC-039` | **Gagal:** tablet terputus → "Panggilan belum terkirim — tekan bel di dinding"; tersambung → terkirim tepat satu kali | UAT tablet | Rekaman + jejak hanya satu `Pressed` |
| `NV-AC-040` | Ambang kosong → item menunggu 10 menit tanpa warna | UAT layar | Tangkapan + `GET /patient-call-wait-thresholds` `IsConfigured = false` |
| `NV-AC-041` | **Regresi:** perawat, Kiosk, display antrean, TV Nurse View login sesudah tablet ditambahkan → perilaku sama dengan `NV-AC-023`–`027` | UAT login | Rekaman login keempat akun |
| `NV-AC-042` | Penempatan 331-C dikoreksi salah catat di bed yang sama → panggilan tetap aktif; "Perawat hadir" tetap bisa. Batal admisi dan koreksi ke bed lain → `ClosedBySystem` (`NV-DEC-057`) | UAT + HTTP | Rekaman + response `nurse-arrived`; jejak `ClosedBySystem` |
| `NV-AC-043` | Berkas `nurseCallContext.jsx` tidak ada dan tidak dirujuk | Pemeriksaan source | Hasil pencarian kosong |
| `NV-AC-044` | **Gagal lalu berhasil:** tutup dari konsol tanpa alasan → `400`; dengan "Sudah ditangani, lupa konfirmasi di tablet" → selesai, "tidak dikonfirmasi di bed" | HTTP + layar | Dua response + jejak `ClosedFromConsole` |
| `NV-AC-045` | **Gagal:** Batal pada panggilan diklaim atau Darurat → tombol tidak ada, permintaan langsung ditolak `409`; Batal panggilan biasa menunggu → `Cancelled` | HTTP + layar | Response `409` + response berhasil |
| `VAL-NV-14` × tablet | **Gagal (privasi):** akun tablet membuka `GET /display-runtime/board` dan `GET /active-calls` → `403` | HTTP | Dua response `403` |
| `VAL-NV-26` × TV | **Gagal:** akun TV dan akun Administrator membuka `GET /bed-tablet-runtime/current` → `403` | HTTP | Response |
| `VAL-NV-19` | **Gagal:** daftarkan tablet kedua untuk bed yang sama → `409` | HTTP | Response |
| `VAL-NV-28` | **Gagal:** tablet di bed kosong memanggil → `409`; layar tanpa tombol | HTTP + layar | Response + tangkapan |
| `VAL-NV-29` | **Gagal:** `POST /calls` tanpa `Idempotency-Key` → `400` | HTTP | Response |
| `VAL-NV-38` | **Gagal:** Sari dan Dewi mengklaim bersamaan → satu berhasil, satu `409` | HTTP paralel | Dua response |
| `VAL-NV-41` | **Gagal:** akun TV atau tablet memanggil `JoinNurseViewCallConsole` → `HubException` | Hub | Log hub |
| Idempotensi | Kirim `POST /calls` dua kali dengan kunci sama → satu jejak | HTTP | Response + jejak |
| Logging | `CloseNote` dan kata sandi tablet tidak muncul di log | Pemeriksaan log | Potongan log |
