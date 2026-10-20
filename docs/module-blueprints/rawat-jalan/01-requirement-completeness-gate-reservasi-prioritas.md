# Requirement Completeness Gate — Reservasi Pasien Prioritas (Amendment RP)

| Field | Nilai |
|---|---|
| Blueprint | `rawat-jalan` (umbrella `RJ-BIL-BP-001`), Amendment RP, bentuk `SINGLE` (`RJ-PRI-DEC-003`) |
| Keputusan yang dinilai | `RJ-PRI-DEC-001` s.d. `025`, `RJ-PRI-FE-001` s.d. `004`, `RJ-PRI-OQ-002` s.d. `006` ([00-interview-decisions.md](00-interview-decisions.md), bagian Amendment RP) |
| Bukti source | [01-capability-impact-scan-reservasi-prioritas.md](01-capability-impact-scan-reservasi-prioritas.md) (`CAP-RP-01` s.d. `22`) |
| Backend SHA | `551e1cbb` (`sukmagp`) |
| Frontend SHA | `1fd858322` (`sukmagpV2`) |
| Batasan hulu | `crm-membership` rev 1 `approved`: `CRM-DEC-003`, `004`, `005`, `007`, `008`, `009`, `011`, `013`, `015`, `016`, `022`, `031`, `032` |
| `baseline_reference_coverage` | Tidak dipakai. Semua butir dinilai dari requirement user dan bukti source |
| Dinilai pada | 2026-10-16 |
| Assessment status | `READY_FOR_DOMAIN_DESIGN` untuk `S-RP-1` s.d. `S-RP-5` (16 Okt 2026, setelah `RJ-PRI-DEC-026` s.d. `029`). Penilaian awal `PARTIALLY_READY` dipertahankan di bagian 6 sebagai riwayat |

## 1. Batas penilaian

Yang dinilai adalah alur pasien prioritas Rawat Jalan, mulai dari reservasi lewat WA/telp sampai pasien dilayani
dokter (`RJ-PRI-DEC-002`). Penilaian dipecah menjadi enam slice supaya satu gap tidak menahan semuanya:

| Slice | Isi | Keputusan utama |
|---|---|---|
| `S-RP-1` | Nomor antrean prioritas kelipatan 5, penyembunyian dari display, suara, dan kabar realtime, tiket kiosk, serta penanda lokasi di worklist | `RJ-PRI-DEC-004`, `005`, `017`, `018`, `025` |
| `S-RP-2` | Satu aturan member di antrean RJ dan tanggal penilaian (`BE-CRM-022`) | `RJ-PRI-DEC-006`, `007`, `024` |
| `S-RP-3` | Siklus hidup Reservasi Prioritas: buat, tiba, skrining, lengkapi, batal, tidak datang | `RJ-PRI-DEC-008` s.d. `011`, `013` s.d. `016`, `019` s.d. `024` |
| `S-RP-4` | Link konfirmasi publik tanpa login | `RJ-PRI-DEC-012` |
| `S-RP-5` | Label status member di layar Pendaftaran RJ | Scope butir 2, `RJ-PRI-FE-004` |
| `S-RP-6` | Titik sentuh billing (2I kirim tagihan, 2J cara bayar, 2K closing) | `RJ-PRI-DEC-002` — di luar scope |

## 2. Bukti yang dipakai

| Bukti | Kegunaan | Status |
|---|---|---|
| PDF "Pendaftaran membership pasien prioritas" poin 2A–2K (12 Okt 2026) | Requirement asli dari rumah sakit | `CONFIRMED` sebagai sumber kebutuhan |
| `00-interview-decisions.md` Amendment RP, `RJ-PRI-DEC-001` s.d. `025` | Keputusan owner (Sukma Giri, owner RJ dan penerima handover `CRM-DEC-031`) | `CONFIRMED` |
| `01-capability-impact-scan-reservasi-prioritas.md` | Keadaan source saat ini pada kedua SHA | `CONFIRMED` sebagai bukti AS-IS |
| `crm-membership` keputusan rev 1 | Aturan member: aktif hanya setelah lunas, kedaluwarsa, manfaat sama untuk semua tingkat berbayar, status tidak mengubah urutan medis | `CONFIRMED` |
| SOP PPI dan SOP identifikasi pasien RS | Dirujuk oleh `RJ-PRI-DEC-019` dan dimensi 17 | Tidak tersedia di repository. Dirujuk, tidak dinilai |

## 3. Temuan 18 dimensi kelengkapan

| No | Dimensi | Temuan | Klasifikasi | Dampak |
|---:|---|---|---|---|
| 01 | Tujuan | Member Aktif dapat reservasi tanpa poli/dokter, dilayani di ruang tunggu prioritas, dan nomornya tidak tampil di layar publik | `CONFIRMED` (PDF 2A–2H, `RJ-PRI-DEC-002`) | — |
| 02 | Aktor | Admisi RJ, petugas skrining, perawat, dokter, pasien lewat link, dan sistem terjadwal | `CONFIRMED` (`RJ-PRI-DEC-014`, `015`, `020`, `023`) | Penerima kabar "pasien tiba" belum jelas untuk petugas skrining. Lihat `RJ-PRI-OQ-007` |
| 03 | Pemicu / Prasyarat | Pasien menghubungi RS. Pasien ber-No. RM dengan membership Aktif menurut resolver pada tanggal reservasi dibuat | `CONFIRMED` (`CRM-DEC-003`, `RJ-PRI-DEC-007`) | — |
| 04 | Alur Utama | Buat reservasi, kirim `wa.me`, pasien tiba dan konfirmasi, petugas datang, skrining, admisi lengkapi poli/dokter, kunjungan dan antrean terbit, perawat dan dokter menghampiri | `CONFIRMED` (`RJ-PRI-DEC-009` s.d. `016`, state table) | — |
| 05 | Alur Alternatif / Exception | Sepuluh skenario sudah dicatat. **Belum ada:** pasien yang punya reservasi terbuka tetapi mendaftar lewat kiosk/loket biasa pada hari yang sama | Sebagian `MISSING` | `RJ-PRI-OQ-008`, `BLOCKING` untuk `S-RP-3` |
| 06 | Data Minimum | Pasien, tanggal kunjungan, nomor reservasi, kode link (hash), status, riwayat, checklist skrining, snapshot klasifikasi. **Belum ada:** nomor WA tujuan pengiriman | Sebagian `PROPOSED` | `RJ-PRI-OQ-011`, `NON_BLOCKING_STANDARD` |
| 07 | Aturan Bisnis / Validation | Kelayakan member, satu reservasi per pasien per tanggal, jadwal dan kuota saat dilengkapi, alasan wajib saat batal | `CONFIRMED` (`RJ-PRI-DEC-016`, `021`, `022`, `024`) | Batas maksimal hari ke depan belum ditetapkan: `RJ-PRI-OQ-012`, `CONFIGURABLE_DEFAULT` |
| 08 | Status / Perubahan Status | Lima status dan delapan transisi sudah tercatat | `CONFIRMED` | Bergantung `RJ-PRI-OQ-008` (bisa menambah satu status akhir) |
| 09 | Peran / Authorization | Resource baru `Read`/`Create`/`Screen`/`Complete`/`Cancel` (`RJ-PRI-DEC-020`). **Bertentangan ringan:** `RJ-PRI-DEC-014` menyebut penerima kabar "pemegang izin pendaftaran RJ", padahal `RJ-PRI-DEC-020` memisahkan resource | `CONFLICT` | `RJ-PRI-OQ-007`, `BLOCKING` untuk `S-RP-3` (wewenang group realtime) |
| 10 | Dependency Antarmodul | `crm-membership` (resolver, status Dibatalkan), Registration (jalur pembuatan kunjungan), Queue Display, Kiosk | `CONFIRMED` (`CAP-RP-04`, `14`, `17`) | — |
| 11 | Integrasi Internal / Eksternal | WA manual `wa.me` (`RJ-PRI-DEC-011`), SignalR internal, halaman publik di domain RS | `CONFIRMED` untuk keputusan. Aksesibilitas internet dan IP asli `MISSING` (`UNK-RP-01`, `02`) | Blocker `IMPLEMENTATION` (go-live), bukan desain |
| 12 | Hasil Akhir | Reservasi berakhir sebagai Menjadi Kunjungan, Dibatalkan, atau Tidak Datang. Kunjungan prioritas dilayani tanpa dipanggil di display | `CONFIRMED` | — |
| 13 | Pembatalan / Koreksi | Batal oleh admisi dengan alasan. Tiba dapat dikembalikan ke Belum Tiba. **Belum ada:** ubah tanggal reservasi | Sebagian `PROPOSED` | `RJ-PRI-OQ-010`, `NON_BLOCKING_STANDARD` |
| 14 | Audit / Histori | Riwayat status beserta pelaku, sumber (link atau petugas), jam, dan alasan. Riwayat tidak dihapus | `CONFIRMED` (`RJ-PRI-DEC-012`, `013`, `022`, `023`). Pencatatan percobaan akses link publik `PROPOSED` | `NON_BLOCKING_STANDARD`: catat jumlah percobaan dan waktu, tanpa data pribadi |
| 15 | Notifikasi | Kabar realtime ke daftar admisi (`RJ-PRI-DEC-014`). WA ke pasien manual. **Belum ada:** kirim ulang link bila pesan hilang | Sebagian `MISSING` | `RJ-PRI-OQ-009`, `BLOCKING` untuk desain keamanan link (`S-RP-4`) |
| 16 | Dampak Billing / Charge | Reservasi tidak membuat tagihan (`RJ-PRI-DEC-009`). Tagihan kunjungan berjalan seperti biasa. Diskon dan cara bayar di luar scope (`S-RP-6`) | `CONFIRMED` | — |
| 17 | Dampak Keselamatan Klinis | Status member tidak mengubah urutan medis (`CRM-DEC-007`). Skrining hanya dicatat dan diarahkan SOP PPI (`RJ-PRI-DEC-019`). Pasien prioritas tidak dipanggil, sehingga ada risiko pasien terlewat. Mitigasinya penanda lokasi di worklist (`RJ-PRI-DEC-017`). Identifikasi dua penanda saat petugas menghampiri mengikuti SOP RS | `CONFIRMED` untuk keputusan. Peringatan "menunggu terlalu lama" `PROPOSED` | `NON_BLOCKING_STANDARD`, dapat menjadi slice berikutnya |
| 18 | Pelaporan / Traceability | Daftar reservasi dengan filter status dan tanggal cukup untuk operasional. Laporan rekap (jumlah tidak datang, lama tunggu) tidak diminta | `PROPOSED` | `NON_BLOCKING_STANDARD` |

## 4. Gap material dan Decision Log

| ID | Pertanyaan | Slice | Bukti saat ini | Usulan | Klasifikasi | Dampak | Owner | Status |
|---|---|---|---|---|---|---|---|---|
| `RJ-PRI-OQ-007` | Siapa penerima kabar realtime "pasien tiba": hanya pemegang izin pendaftaran RJ (`RJ-PRI-DEC-014`), atau semua pemegang `Read` resource Reservasi Prioritas termasuk petugas skrining (`RJ-PRI-DEC-020`)? | `S-RP-3` | Dua keputusan memakai acuan izin berbeda | Pemegang `Read` resource baru, supaya petugas skrining ikut tahu | `CONFLICT` → `CONFIRMED` | Semula `BLOCKING` | Sukma Giri | Ditutup `RJ-PRI-DEC-026` |
| `RJ-PRI-OQ-008` | Pasien punya reservasi terbuka, tetapi pada hari yang sama mendaftar lewat kiosk atau loket biasa. Apa yang terjadi pada reservasinya? | `S-RP-3` | Tidak ada keputusan. Hari ini reservasi tetap terbuka, pelengkapan ditolak `BlockActiveEncounter`, lalu menjadi "Tidak Datang" pukul 23.59 walau pasien datang | Kunjungan tetap dibuat, lalu reservasi ditutup otomatis sebagai status akhir "Didaftarkan lewat jalur lain" | `MISSING` → `CONFIRMED` | Semula `BLOCKING` | Sukma Giri | Ditutup `RJ-PRI-DEC-027` |
| `RJ-PRI-OQ-009` | Bila pasien kehilangan pesan, bolehkah admisi mengirim ulang link? | `S-RP-4` | Kode disimpan sebagai hash (`CAP-RP-09`), sehingga link lama tidak dapat ditampilkan ulang | "Kirim ulang" membuat kode baru dan mematikan kode lama, tercatat di riwayat | `MISSING` → `CONFIRMED` | Semula `BLOCKING` | Sukma Giri | Ditutup `RJ-PRI-DEC-028` |
| `RJ-PRI-OQ-010` | Bila pasien ingin mengganti tanggal, apakah reservasi diubah atau dibatalkan lalu dibuat ulang? | `S-RP-3` | Tidak ada keputusan | Batal dengan alasan "ganti tanggal" lalu buat reservasi baru, tanpa status baru | `PROPOSED` → `CONFIRMED` | `NON_BLOCKING_STANDARD` | Sukma Giri | Ditutup `RJ-PRI-DEC-029` (kelayakan dinilai ulang) |
| `RJ-PRI-OQ-011` | Nomor WA tujuan pengiriman | `S-RP-3` | Pola kasir tidak mengisi nomor (`CAP-RP-22`) | Bawaan No. HP pasien dari master. Petugas boleh mengganti per pengiriman tanpa mengubah master | `PROPOSED` | `NON_BLOCKING_STANDARD` | Desain | `PROPOSED` |
| `RJ-PRI-OQ-012` | Batas maksimal hari ke depan untuk reservasi | `S-RP-3` | `RJ-PRI-DEC-008` tidak memberi batas | Konfigurasi, bawaan 30 hari | `PROPOSED` | `CONFIGURABLE_DEFAULT` | Desain | `PROPOSED` |
| `RJ-PRI-OQ-002` | Format nomor reservasi | `S-RP-3` | `NumberSeriesAllocator` siap (`CAP-RP-07`) | `RSV-YYMMDD-nnnn`, reset harian | `PROPOSED` | `CONFIGURABLE_DEFAULT` | Desain | `PROPOSED` |
| `RJ-PRI-OQ-003` / `UNK-RP-01`, `02` | Aksesibilitas internet domain RS dan penerusan IP asli | `S-RP-4` | Domain ada di CORS (`CAP-RP-12`) | — | `MISSING` | Blocker `IMPLEMENTATION` (go-live) | IT/infrastruktur | `OPEN` |
| `RJ-PRI-OQ-006` | Isi master tingkat: `PriorityQueue = true`, iuran, diskon | `S-RP-1`, `S-RP-2` | `RJ-DOC-DEC-059`: master tidak di-seed | — | `MISSING` | Blocker `IMPLEMENTATION` (data go-live) | Yoga Aji Pratama / Marketing | `OPEN` |
| `GATE-RP-01` | Display `MemberOnly` (misalnya di ruang VIP) tidak akan menampilkan apa pun bila semua tingkat berbayar prioritas, karena `RJ-PRI-DEC-005` menyaring prioritas di semua display | `S-RP-1` | `RJ-PRI-DEC-005` menyatakan "berapa pun `QueueAudienceMode`-nya" | Diterima sebagai konsekuensi, diberitahukan ke admin perangkat display | `PROPOSED` | `NON_BLOCKING_STANDARD` | Sukma Giri | Dicatat |
| `GATE-RP-02` | Pergantian aturan kelipatan 5 di tengah hari layanan | `S-RP-1` | Nomor yang sudah terbit tetap dihormati (`CAP-RP-01`) | Rilis di luar jam layanan. Tidak ada nomor ganda karena nomor terpakai tetap dilewati | `PROPOSED` | `NON_BLOCKING_STANDARD` | Rilis | Dicatat |
| `GATE-RP-03` | Peringatan bila pasien prioritas menunggu terlalu lama tanpa dihampiri | `S-RP-1` | Tidak diminta PDF | Slice berikutnya | `PROPOSED` | `NON_BLOCKING_STANDARD` | Sukma Giri | `LATER SLICE` |

## 5. Contoh dampak gap pemblokir

> **`RJ-PRI-OQ-008`.** Ibu Sari (Gold) punya reservasi untuk 16 Okt. Ia tiba pukul 08.30 dan langsung memakai kiosk
> tanpa membuka link. Kiosk menerbitkan nomor 5 (prioritas) dan tiket mengarahkannya ke ruang tunggu prioritas.
> Reservasinya tetap "Menunggu Kedatangan", jadi admisi melihat Ibu Sari belum tiba. Bila admisi mencoba melengkapi,
> sistem menolak karena kunjungan aktif sudah ada. Pukul 23.59 reservasi tercatat "Tidak Datang" padahal Ibu Sari
> dilayani. Laporan tidak datang menjadi salah dan admisi bingung.

## 6. Kesiapan per slice

| Slice | Kesiapan | Alasan |
|---|---|---|
| `S-RP-1` | `READY_FOR_DOMAIN_DESIGN` | Semua keputusan tertutup, termasuk realtime display (`RJ-PRI-DEC-025`). Sisa gap non-blocking |
| `S-RP-2` | `READY_FOR_DOMAIN_DESIGN` | `AG-03` dijawab (`RJ-PRI-DEC-006`, `007`, `024`) |
| `S-RP-3` | `BUSINESS_DECISION_REQUIRED` | `RJ-PRI-OQ-007` dan `RJ-PRI-OQ-008` |
| `S-RP-4` | `BUSINESS_DECISION_REQUIRED` | `RJ-PRI-OQ-009`. Setelah dijawab, desain boleh jalan, sedangkan go-live tetap menunggu `UNK-RP-01`/`02` |
| `S-RP-5` | `READY_FOR_DOMAIN_DESIGN` | Pekerjaan tampilan dari data yang sudah ada (`CAP-RP-21`) |
| `S-RP-6` | Di luar scope | Milik `billing-kasir` dan `platform-notification` |

**Ringkasan penilaian awal:** `PARTIALLY_READY`.

### 6.1 Penilaian ulang setelah `grill-me` singkat (16 Okt 2026)

| Slice | Kesiapan | Dasar |
|---|---|---|
| `S-RP-1` | `READY_FOR_DOMAIN_DESIGN` | Tidak berubah |
| `S-RP-2` | `READY_FOR_DOMAIN_DESIGN` | Tidak berubah |
| `S-RP-3` | `READY_FOR_DOMAIN_DESIGN` | `RJ-PRI-DEC-026` (penerima kabar), `027` (status akhir "Didaftarkan lewat Jalur Lain"), `029` (ganti tanggal) |
| `S-RP-4` | `READY_FOR_DOMAIN_DESIGN` untuk desain. Go-live tetap menunggu `UNK-RP-01`/`02` | `RJ-PRI-DEC-028` (kirim ulang dengan kode baru) |
| `S-RP-5` | `READY_FOR_DOMAIN_DESIGN` | Tidak berubah |
| `S-RP-6` | Di luar scope | Tidak berubah |

**Ringkasan akhir:** `READY_FOR_DOMAIN_DESIGN` untuk seluruh Amendment RP. Tidak ada butir `BLOCKING` yang terbuka. Yang
tersisa hanya usulan non-blocking (`RJ-PRI-OQ-002`, `011`, `012`, `GATE-RP-01` s.d. `03`) dan blocker `IMPLEMENTATION`
(`RJ-PRI-OQ-003`, `006`).

## 7. Apa yang boleh berjalan dan apa yang harus berhenti

**Boleh berjalan ke desain:** `S-RP-1`, `S-RP-2`, `S-RP-5`. Ketiganya tidak bergantung pada wadah reservasi. Nomor
kelipatan 5, penyaringan display, dan aturan member juga berlaku untuk pasien prioritas walk-in.

**Harus berhenti sampai keputusan turun:** `S-RP-3` dan `S-RP-4`. `S-RP-4` bergantung pada `S-RP-3`, karena link
melekat pada reservasi.

**Dependency:** `S-RP-3` memakai hasil `S-RP-1` (nomor dan penyaringan) dan `S-RP-2` (snapshot klasifikasi) saat
reservasi dilengkapi.

## 8. Handoff berikutnya

1. ~~`grill-me` singkat untuk `RJ-PRI-OQ-007`, `008`, `009`, dan `010`~~ — selesai 16 Okt 2026 (`RJ-PRI-DEC-026` s.d. `029`).
2. **`design-business-module`** untuk seluruh Amendment RP. `hospital-domain-architect` **tidak** dipakai:
   `domain_architecture_readiness` = `DOMAIN_ARCHITECTURE_NOT_RUN`. Alasannya, semua slice berada di bounded context
   Registration Rawat Jalan. Titik sentuh lintas konteks sudah punya kontrak: resolver CRM (`INT-01` s.d. `03` di
   `crm-membership`) dan pemisahan Member/Prioritas/Privasi (`RJ-DOC-DEC-055`). Dampak billing nihil
   (`RJ-PRI-DEC-009`), dan dampak klinis dibatasi `CRM-DEC-007`.
3. Pertahankan pada handoff: `blueprint_id` `rawat-jalan`, Amendment RP, SHA `551e1cbb`/`1fd858322`, keputusan
   `RJ-PRI-DEC-001` s.d. `029`, impact scan `CAP-RP-01` s.d. `22`, dan blocker `IMPLEMENTATION` `RJ-PRI-OQ-003`/`006`.
