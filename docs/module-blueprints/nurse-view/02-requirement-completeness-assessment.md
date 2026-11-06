# Penilaian Kelengkapan Requirement — Nurse View

| Field | Nilai |
|---|---|
| Blueprint ID | `nurse-view` |
| Revision | `2` — ditambah slice `NV-S8` (tablet bed dan panggilan pasien, Amendment revision 5), 2026-11-05; lihat [Revision 2](#revision-2--slice-nv-s8-tablet-bed-dan-panggilan-pasien). Sebelumnya `1` — `NV-S1` s.d. `S7` |
| Status | `draft` |
| Owner | Rizki Gunawan |
| Tanggal | 2026-10-28; revision 2 2026-11-05 |
| Masukan | `00-interview-decisions.md` revision `2` (`NV-DEC-004` s.d. `026`, `NV-AC-001` s.d. `022`); `01-existing-capability-map.md` revision `1`; `rawat-inap/keperawatan/docs/task-lintas-modul/TLM-NV-001`; `rawat-inap/episode-rawat-inap/docs/task-lintas-modul/TLM-NV-001` |
| Backend SHA | `a6f977a3` (branch `rizkiG`) |
| Frontend SHA | `76b0c6e4b` (branch `RizkiV2`) |
| Baseline rujukan | `indonesia-hospital-domain-reference` **tidak dipakai**. Seluruh keputusan inti sudah dikonfirmasi owner; pembanding praktik rumah sakit lain dicatat di decision log sebagai pembanding, bukan kebijakan |
| Hasil | **`READY_FOR_DOMAIN_DESIGN`** untuk ketujuh slice; nol butir `BLOCKING` bagi desain. Revision 2: `NV-S8` awalnya **`PARTIALLY_READY`**; sesudah `NV-OQ-034`, `035` ditutup (`NV-DEC-053`, `054`) seluruh `NV-S8` **`READY_FOR_DOMAIN_DESIGN`** (R2.10) |

Gerbang ini hanya menilai apakah requirement sudah cukup lengkap untuk dirancang. Ia tidak
merancang entity, endpoint, maupun layar.

## 1. Scope penilaian

Modul `nurse-view`, satu blueprint (`SINGLE`, `NV-DEC-005`). Dinilai per slice:

| Slice | Isi | Keputusan |
|---|---|---|
| `NV-S1` | Papan bed per lantai: status bed, penghuni samaran, perawat PJ, bed kosong | `NV-DEC-006`, `007`, `008`, `012`, `015`, `017`, `025` |
| `NV-S2` | TTV dan EWS terakhir, peringatan kritis visual | `NV-DEC-007`, `011`, `013` |
| `NV-S3` | Tanda "TTV lewat jadwal" dan pengaturan intervalnya | `NV-DEC-010` |
| `NV-S4` | Ikon keselamatan: risiko jatuh tinggi, alergi, isolasi + tidak cocok | `NV-DEC-018`, `019`, `020` |
| `NV-S5` | Header perawat jaga dari roster | `NV-DEC-012`, `021` |
| `NV-S6` | Master perangkat display, login perangkat, TV pasif | `NV-DEC-009`, `016`, `022`, `024` |
| `NV-S7` | Pembaruan data: muat ulang 60 detik, push TTV, pita data basi | `NV-DEC-014`, `023`, `026` |

## 2. Bukti yang dipakai

| Wewenang | Bukti |
|---|---|
| 1 — Requirement eksplisit owner | Permintaan Rizki Gunawan 2026-10-28; 26 keputusan `NV-DEC` hasil scope pass dan closure pass |
| 3 — Keputusan pemilik modul tetangga yang dikonfirmasi | `RWI-DEC-100` (Muhammad Hamzah, 2026-09-11): shift perawat belum dijalankan; `GIZ-DEC-013`: menu di luar scope Gizi; aturan dokumen lintas modul dari Muhammad Hamzah 2026-10-28 |
| 6 — Implementasi V2 terverifikasi | `01-existing-capability-map.md` (`NV-CAP-01` s.d. `16`) |
| 2 — SOP/kebijakan RS yang disahkan | **Tidak ada** di bukti. Lihat butir `PROPOSED` pada bagian 4 |

## 3. Temuan per dimensi

`C` = `CONFIRMED`, `P` = `PROPOSED`, `M` = `MISSING`, `—` = tidak berlaku secara material.

| ID | Dimensi | S1 | S2 | S3 | S4 | S5 | S6 | S7 | Catatan |
|---|---|:-:|:-:|:-:|:-:|:-:|:-:|:-:|---|
| 01 | Tujuan | C | C | C | C | C | C | C | Perawat memantau seluruh pasien lantai dari jauh tanpa membuka rekam medis satu per satu (`NV-DEC-004`) |
| 02 | Aktor | C | C | C | C | C | C | C | Admin IT, perawat/kepala ruang (penonton), Komite Keperawatan (angka interval), akun perangkat |
| 03 | Pemicu/prasyarat | C | C | C | C | C | C | C | Prasyarat data: `FloorCode` kamar, versi instrumen aktif, `OrganizationUnitId` bangsal — runtime, lihat `G-06` |
| 04 | Alur utama | C | C | C | C | C | C | C | Skenario normal dan exception sudah ada di decision log |
| 05 | Exception | C | C | C | C | C | C | C | Data basi, perangkat dinonaktifkan, roster kosong, kamar tanpa `FloorCode`, TTV dibatalkan |
| 06 | Data minimum | C | C | C | C | C | C | C | Hampir seluruhnya dibaca dari modul lain; data milik sendiri hanya perangkat dan interval |
| 07 | Aturan bisnis | C | C | P | C | C | C | C | S3: bentuk aturan `C`, angka interval `M` → `G-01` |
| 08 | Status | — | — | C | — | — | C | C | Hanya perangkat yang punya status sendiri (`BelumAdaLogin`, `Aktif`, `Nonaktif`); status bed diturunkan |
| 09 | Authorization | C | C | C | C | C | C | C | Perangkat hanya membaca lantainya; admin IT pengelola tunggal; tidak ada aksi perawat di TV |
| 10 | Dependency antarmodul | C | C | C | C | C | C | C | Rawat Inap, Clinical, HR Scheduling, Patient Management — baca saja |
| 11 | Integrasi | — | — | — | — | — | C | C | Hanya SignalR internal. Tidak ada sistem luar; alat monitor ditunda (`NV-DEC-011`) |
| 12 | Hasil akhir | C | C | C | C | C | C | C | Papan menampilkan keadaan terkini lantai, atau pita data basi |
| 13 | Pembatalan/koreksi | C | C | — | C | — | C | — | TTV batal → kembali ke TTV sah sebelumnya; perangkat nonaktif/atur ulang login |
| 14 | Audit/histori | — | — | P | — | — | P | — | Siapa mengubah interval dan siapa membuat/mengatur ulang login belum disebut → `G-03` |
| 15 | Notifikasi | C | C | — | — | — | — | — | Sengaja tanpa suara (`NV-DEC-013`); jalur `notify-doctor` yang ada tetap berlaku |
| 16 | Billing | — | — | — | — | — | — | — | Tidak ada tarif atau tagihan: modul hanya membaca dan menampilkan |
| 17 | Keselamatan klinis | C | C | C | P | C | — | C | Ditangani: data basi, identitas samaran + nomor bed, aman-gagal interval. Celah kecil `G-02`, `G-04`, `G-05` |
| 18 | Pelaporan | — | — | — | — | — | P | — | Laporan waktu tanggap gugur bersama panggilan pasien. Status online perangkat untuk admin → `G-07` |

## 4. Gap dan dampaknya

| ID | Butir | Status bukti | Dampak | Alasan klasifikasi |
|---|---|---|---|---|
| `G-01` | Angka interval ukur ulang TTV per tingkat EWS | `MISSING` (`NV-OQ-010`, Komite Keperawatan) | `CONFIGURABLE_DEFAULT` | Bentuk aturan, pemilik angka, dan perilaku aman-gagal sudah dikonfirmasi. Angka memang wajar berbeda antar rumah sakit. **Syarat:** desain dilarang mengisi angka bawaan; tabel dibuat kosong dan tanda tidak tampil sampai angka disahkan |
| `G-02` | Papan adalah alat bantu pantau, bukan sumber keputusan klinis; perawat tetap memeriksa rekam medis sebelum bertindak | `PROPOSED` | `NON_BLOCKING_STANDARD` | Tersirat dari `NV-DEC-009` (tanpa detail di TV), tetapi belum dinyatakan. Cukup dicatat di PRD dan materi UAT |
| `G-03` | Jejak siapa yang membuat/mengubah perangkat, mengatur ulang login, dan mengubah angka interval | `PROPOSED` | `NON_BLOCKING_STANDARD` | Ikuti pola existing: kolom audit `IdentityModel` + `LoggerService` seperti `QueueDisplayDeviceController`. Khusus angka interval: simpan siapa dan kapan mengesahkannya, karena itu keputusan klinis |
| `G-04` | Bed bayi (boks) dan umur bayi pada identitas samaran | `PROPOSED` | `NON_BLOCKING_STANDARD` | `NV-DEC-006` = semua bed, jadi boks ikut. Usulan: umur < 1 tahun tampil bulan/hari, misalnya "B.N.R. · P · 5 hr". EWS bayi bisa `Unknown` → kartu "EWS –" |
| `G-05` | Risiko jatuh "Tinggi" memakai pita `HIGH` instrumen keperawatan; dasar SOP gelang kuning belum ada sebagai dokumen | `PROPOSED` (ambang milik instrumen keperawatan) | `NON_BLOCKING_STANDARD` | Nurse View tidak menetapkan ambang sendiri; ia membaca pita yang dimiliki `rawat-inap/keperawatan`. Makna klinis tidak bergeser |
| `G-06` | Prasyarat data runtime: `FloorCode`, versi instrumen aktif, isi roster, `OrganizationUnitId` (`NV-UNK-001` s.d. `004`) | `MISSING` | `NON_BLOCKING_STANDARD` | Tidak mengubah desain; semuanya sudah punya perilaku aman-gagal. Wajib menjadi daftar periksa go-live |
| `G-07` | Admin melihat status online/offline perangkat | `PROPOSED` | `NON_BLOCKING_STANDARD` | Pola `LastOnlineDateTime` sudah ada di display antrean; berguna untuk mendeteksi TV mati |
| `G-08` | Waktu "shift berjalan" dan "umur pengukuran" memakai jam operasional rumah sakit | `PROPOSED` | `NON_BLOCKING_STANDARD` | Ikuti pola jam operasional yang dipakai IGD (`GetOperationalNow`) |

**Tidak ada butir `BLOCKING`, dan tidak ada `CONFLICT` yang tersisa.** Kedua conflict capability
map (`NV-CON-001`, `002`) sudah ditutup owner lewat `NV-DEC-021` dan `022`.

## 5. Gerbang implementasi yang dibawa

Butir berikut **tidak** menghalangi desain, tetapi menghalangi task implementasi yang
bergantung padanya. Butir-butir ini wajib terbawa ke `plan-module-delivery`.

| ID | Yang ditunggu | Pemilik | Slice yang tertahan |
|---|---|---|---|
| `NV-OQ-010` | Angka interval | Komite Keperawatan | S3: hanya runtime tanda lewat jadwal |
| `NV-OQ-015` | Restu berkas bersama untuk login perangkat | Lead — **ditutup** `NV-DEC-030` dengan syarat | Tidak lagi menahan |
| `NV-OQ-016` | Titik terbit TTV (`TLM-NV-001` keperawatan) | Muhammad Hamzah | S7: hanya push TTV |
| `NV-OQ-017` | Registry `HealthServices / NurseViewManagement / Nvw` | Rizki Gunawan — **ditutup** `NV-DEC-029` (2026-10-28) | Tidak lagi menahan |
| `TLM-NV-001` episode | Boleh/tidaknya memanggil `ProjectBatchAsync` yang `internal` | Muhammad Hamzah | S1: bentuk adapter, bukan isi desain |

## 6. Kesiapan per slice

| Slice | Kesiapan | Catatan |
|---|---|---|
| `NV-S1` | `READY_FOR_DOMAIN_DESIGN` | Desain perlu dua bentuk adapter bila jawaban `TLM-NV-001` episode belum masuk: memanggil metode `internal` atau metode baca publik |
| `NV-S2` | `READY_FOR_DOMAIN_DESIGN` | — |
| `NV-S3` | `READY_FOR_DOMAIN_DESIGN` | Dengan syarat `G-01`: tanpa angka bawaan |
| `NV-S4` | `READY_FOR_DOMAIN_DESIGN` | — |
| `NV-S5` | `READY_FOR_DOMAIN_DESIGN` | Aman-gagal sudah diputuskan |
| `NV-S6` | `READY_FOR_DOMAIN_DESIGN` | `NV-OQ-015`, `017` tertutup (`NV-DEC-029`, `030`) |
| `NV-S7` | `READY_FOR_DOMAIN_DESIGN` | Push TTV tertahan `NV-OQ-016`; muat ulang 60 detik tidak tertahan |

## 7. Yang boleh berjalan dan yang harus berhenti

- **Boleh berjalan:** `design-business-module` untuk ketujuh slice. `hospital-domain-architect`
  dilewati (`DOMAIN_ARCHITECTURE_NOT_RUN`). Alasannya: modul hanya membaca data, tidak memiliki
  master bersama, tidak berdampak billing, dan batas keselamatan klinisnya sudah diputuskan
  owner.
- **Harus berhenti:** tidak ada pada tahap desain. Pada tahap implementasi, berhenti mengikuti
  bagian 5.

## 8. Keputusan pemilik yang dibutuhkan

Tidak ada keputusan baru yang dibutuhkan sebelum desain. Butir `PROPOSED` `G-02` s.d. `G-08`
akan dituliskan **sebagai usulan** di desain, dengan tanda yang jelas, agar bisa disahkan atau
ditolak owner saat approval blueprint.

## 9. Handoff

| Field | Nilai |
|---|---|
| Skill berikutnya | `design-business-module` |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` — alasan di bagian 7 |
| Slice dikirim | `NV-S1` s.d. `NV-S7` |
| Decision ID dibawa | `NV-DEC-004` s.d. `026` |
| Gap dibawa | `G-01` s.d. `G-08` |
| Gerbang implementasi dibawa | `NV-OQ-010`, `015`, `016`, `017`, `TLM-NV-001` episode |

## Revision 2 — slice NV-S8: tablet bed dan panggilan pasien

| Field | Nilai |
|---|---|
| Tanggal | 2026-11-05 |
| Masukan | `00-interview-decisions.md` revision `5` draft (`NV-DEC-034` s.d. `052`, `NV-AC-028` s.d. `043`, `NV-OQ-022`, `028`, `NV-UNK-005` s.d. `007`); `01-existing-capability-map.md` revision `2` (`NV-CAP-17` s.d. `29`) |
| Backend SHA | `0c6eab34` (branch `rizkiG`) |
| Frontend SHA | `91a11b08f` (branch `RizkiV2`) |
| Baseline rujukan | `indonesia-hospital-domain-reference` **tidak dipakai**. Pembanding sistem panggil perawat (UL 1069, DIN VDE 0834) dan ECRI tentang kelelahan alarm dicatat di decision log sebagai pembanding, bukan kebijakan |
| Hasil | **`PARTIALLY_READY`** saat dinilai (R2.6); **`READY_FOR_DOMAIN_DESIGN`** sesudah closure 2026-11-05 (R2.10) |

Bagian 1 s.d. 9 di atas (`NV-S1` s.d. `S7`) tidak disunting dan tetap berlaku.

### R2.1 Scope penilaian

Satu slice baru, `NV-S8`, dipecah menjadi empat bagian supaya bagian yang siap tidak ikut
tertahan:

| Bagian | Isi | Keputusan |
|---|---|---|
| `NV-S8a` | Perangkat tablet bed: didaftarkan admin IT, login per perangkat, terikat ke satu bed, nonaktifkan | `NV-DEC-035`, `041`, `046` (kunci saat bed kosong) |
| `NV-S8b` | Siklus panggilan: tekan, naik Darurat, klaim, ambil alih, lepas, hadir, ditutup sistem; data waktu tanggap | `NV-DEC-038` s.d. `042`, `046`, `049`, `050`, `052` |
| `NV-S8c` | Strip Panggilan Aktif di TV | `NV-DEC-044`, `047`, `048` |
| `NV-S8d` | Konsol Panggilan di PC nurse station | `NV-DEC-034`, `043`, `044`, `045` |

### R2.2 Bukti yang dipakai

| Wewenang | Bukti |
|---|---|
| 1 — Requirement eksplisit owner | Permintaan Rizki Gunawan 2026-11-05; 19 keputusan `NV-DEC-034` s.d. `052` hasil Amendment dan closure pass |
| 3 — Keputusan pemilik modul tetangga | `INV-INP-02` Rawat Inap (satu bed paling banyak satu penempatan aktif), dibaca dari source, bukan keputusan baru |
| 6 — Implementasi V2 terverifikasi | `01-existing-capability-map.md` revision 2 (`NV-CAP-17` s.d. `29`) |
| 2 — SOP/kebijakan RS yang disahkan | **Tidak ada** di bukti: tidak ada SOP sistem panggil perawat, masa simpan data panggilan, maupun angka ambang tanggap. Lihat R2.4 |

### R2.3 Temuan per dimensi

`C` = `CONFIRMED`, `P` = `PROPOSED`, `M` = `MISSING`, `—` = tidak berlaku secara material.

| ID | Dimensi | S8a | S8b | S8c | S8d | Catatan |
|---|---|:-:|:-:|:-:|:-:|---|
| 01 | Tujuan | C | C | C | C | Pasien memanggil perawat dari tablet; perawat tahu siapa yang memanggil, siapa yang menangani, dan urutannya. Tablet melengkapi bel kabel (`NV-DEC-035`, `037`) |
| 02 | Aktor | C | C | C | C | Pasien atau keluarga di tablet; perawat di konsol dan di tablet; admin IT untuk tablet dan izin; Komite Keperawatan untuk angka ambang; lead untuk berkas bersama |
| 03 | Pemicu/prasyarat | C | C | C | C | Tablet aktif dan bed berpenghuni; konsol dibuka pemegang izin. Prasyarat lapangan `NV-UNK-005` s.d. `007` → `G-16` |
| 04 | Alur utama | C | C | C | C | Tekan → strip dan konsol → klaim → hadir (`NV-DEC-038` s.d. `041`, `045`, `047`) |
| 05 | Exception | C | P | C | P | Tercakup: gagal kirim, tekan ulang, pindah/pulang, koreksi penempatan, TV putus, bed kosong. **Belum tercakup:** panggilan yang tidak bisa ditutup karena tablet rusak, mati, nonaktif, atau "Perawat hadir" lupa ditekan → `NV-OQ-034` |
| 06 | Data minimum | C | C | C | C | Data yang dicatat dikunci `NV-DEC-049`. Isi tablet tentang identitas pasien belum dinyatakan → `G-09`. Masa simpan → `G-10` |
| 07 | Aturan bisnis | C | C | P | C | S8c: bentuk aturan warna `C`, angka `M` → `G-15` |
| 08 | Status | C | **M** | C | C | S8b: pembatalan oleh pasien belum punya keputusan sendiri → `NV-OQ-035`. Penutupan cadangan → `NV-OQ-034`. Darurat tidak bisa diturunkan → `G-13` |
| 09 | Authorization | C | C | C | C | Izin konsol lewat Akses Role (`NV-DEC-043`); lepas hanya oleh pengklaim, ambil alih oleh pemegang izin lain (`NV-DEC-040`); tablet hanya akun perangkat bed-nya. Restu berkas bersama → gerbang implementasi `NV-OQ-028` |
| 10 | Dependency antarmodul | C | C | C | C | Rawat Inap (penempatan, baca saja), Master Data (bed, kamar, lantai), Patient Management (identitas untuk inisial), platform Auth dan Hub |
| 11 | Integrasi | — | — | C | C | Hanya SignalR internal. Bel kabel tanpa integrasi (`NV-DEC-037`). Kemampuan tablet → `NV-UNK-007` |
| 12 | Hasil akhir | C | C | C | C | Panggilan berakhir `Ditangani` atau `DitutupSistem` dengan jam-jam yang tercatat |
| 13 | Pembatalan/koreksi | C | **M** | — | P | Lepas klaim `C`; pembatalan oleh pasien `M` (`NV-OQ-035`); penutupan dari konsol `M` (`NV-OQ-034`); panggilan selesai tidak dibuka ulang → `G-12` |
| 14 | Audit/histori | P | C | — | C | S8b dikunci `NV-DEC-049`; S8a jejak admin perangkat tablet → `G-17` |
| 15 | Notifikasi | — | C | C | C | Bunyi hanya di konsol dengan pengingat (`NV-DEC-045`); TV visual; tanpa HP. Tanpa eskalasi ke pihak lain → `G-11`. Bila browser memblokir bunyi → `G-14` |
| 16 | Billing | — | — | — | — | Panggilan perawat tidak menimbulkan tarif atau tagihan |
| 17 | Keselamatan klinis | C | **M** | C | C | Tertangani: bel kabel tetap jalur utama, Darurat bukan Code Blue, kehadiran tak terverifikasi diterima owner. **Belum:** panggilan yang tertinggal bisa menutupi panggilan baru di strip dan membuat pengingat konsol berbunyi tanpa akhir → `NV-OQ-034` |
| 18 | Pelaporan | — | C | — | — | Data dicatat sekarang, layar laporan slice berikutnya; batas verifikasi wajib disebut (`NV-DEC-049`) |

### R2.4 Gap dan dampaknya

| ID | Butir | Status bukti | Dampak | Alasan klasifikasi |
|---|---|---|---|---|
| `NV-OQ-034` | **Penutupan cadangan.** Bagaimana panggilan ditutup bila "Perawat hadir" tidak bisa ditekan: tablet rusak, mati, tidak tersambung, atau dinonaktifkan; atau perawat lupa menekannya. **Contoh:** Sari menangani 305-A pukul 10.02 tetapi lupa menahan "Perawat hadir"; pukul 13.00 strip masih menampilkan "305-A · Sari menuju · menunggu 3:00:00", dan pukul 13.05 panggilan baru 318-B tergeser ke "+1 panggilan lain" | `MISSING` | **`BLOCKING`** | Menambah atau tidak menambah perpindahan status, menentukan siapa yang boleh menutup, dan apa yang dicatat sebagai waktu tanggap. Menyentuh siklus, authorization, audit, dan keselamatan |
| `NV-OQ-035` | **Pembatalan oleh pasien.** Bolehkah pasien atau keluarga membatalkan panggilannya dari tablet? Baris "Pasien membatalkan" pada tabel siklus berasal dari usulan analisa, belum dari keputusan. **Contoh:** pasien 318-B memanggil karena infus habis, lalu perawat lain kebetulan lewat dan menggantinya; apakah pasien boleh menekan "Batal"? | `PROPOSED` | **`BLOCKING`** | Menentukan ada tidaknya status `Dibatalkan` dan siapa pemicunya. Sistem panggil perawat umumnya hanya bisa dihentikan petugas di bed, sedangkan aplikasi tablet modern sering mengizinkan pasien membatalkan. Pilihannya mengubah siklus |
| `G-09` | Tablet hanya menampilkan nomor bed dan status panggilan, tanpa nama, inisial, atau No. RM pasien | `PROPOSED` | `NON_BLOCKING_STANDARD` | Sejalan dengan prinsip data minimal (Business rule 3) dan strip TV (`NV-DEC-044`). Tablet berada di samping bed dan terlihat pengunjung |
| `G-10` | Masa simpan data panggilan | `MISSING` | `NON_BLOCKING_STANDARD` | Usulan: tidak ada penghapusan otomatis, sama seperti data operasional lain. Masa simpan resmi mengikuti kebijakan rumah sakit bila kelak ada, tanpa mengubah desain |
| `G-11` | Tidak ada eskalasi ke pihak lain di MVP amendment; yang ada hanya warna lama menunggu dan pengingat konsol | `PROPOSED` | `NON_BLOCKING_STANDARD` | Turunan `NV-DEC-035` (notifikasi HP di luar scope) dan `NV-DEC-037` (bel kabel jalur utama). Wajib disebut eksplisit di PRD dan UAT supaya tidak disangka ada |
| `G-12` | Panggilan yang sudah selesai tidak dibuka ulang; bila masih perlu, pasien memanggil lagi dan lahir panggilan baru | `PROPOSED` | `NON_BLOCKING_STANDARD` | Menjaga waktu tanggap tetap jujur dan siklus sederhana |
| `G-13` | Pasien tidak bisa menurunkan Darurat menjadi panggilan biasa | `PROPOSED` | `NON_BLOCKING_STANDARD` | `NV-DEC-039` hanya mengatur kenaikan. Menurunkan prioritas dari tablet berisiko menutupi kegawatan |
| `G-14` | Konsol menampilkan tombol "Aktifkan suara" bila browser menahan bunyi | `PROPOSED` | `NON_BLOCKING_STANDARD` | Pola layar antrean yang sudah ada (`NV-CAP-27`) |
| `G-15` | Angka ambang warna lama menunggu (`NV-OQ-022`) | `MISSING` (Komite Keperawatan) | `CONFIGURABLE_DEFAULT` | Bentuk aturan dan aman-gagal sudah dikonfirmasi (`NV-DEC-048`). **Syarat:** desain dilarang mengisi angka bawaan, sama seperti `G-01` |
| `G-16` | Prasyarat lapangan: bel kabel berfungsi, praktik pencatatan transfer, uji tablet contoh (`NV-UNK-005` s.d. `007`) | `MISSING` | `NON_BLOCKING_STANDARD` | Tidak mengubah desain; wajib masuk daftar periksa go-live |
| `G-17` | Jejak admin untuk perangkat tablet: siapa membuat, mengatur ulang login, dan menonaktifkan | `PROPOSED` | `NON_BLOCKING_STANDARD` | Ikuti `G-03` dan pola perangkat display yang sudah berjalan |

Tidak ada `CONFLICT`. `NV-CON-003` sudah ditutup `NV-DEC-051`.

### R2.5 Gerbang implementasi yang dibawa

| ID | Yang ditunggu | Pemilik | Bagian yang tertahan |
|---|---|---|---|
| `NV-OQ-028` | Restu berkas bersama: akun perangkat tablet dan policy-nya, route tablet di lima berkas frontend, method hub konsol dan tablet | Lead | Implementasi `NV-S8a`, `S8b` (endpoint tablet), `S8c`, `S8d` (method hub). **Ditutup 2026-11-05 — `NV-DEC-056`:** lead merestui sebelas berkas bersama sesuai `02-backend-architecture.md` R2.5 dan `03-frontend-architecture.md` R2.7, dengan syarat `NV-DEC-030`; tidak ada bagian yang tertahan lagi oleh gerbang ini |
| `NV-OQ-022` | Angka ambang warna | Komite Keperawatan | Runtime warna `NV-S8c`/`S8d` saja |

### R2.6 Kesiapan per bagian

| Bagian | Kesiapan | Catatan |
|---|---|---|
| `NV-S8a` | `READY_FOR_DOMAIN_DESIGN` | Tidak bergantung pada `NV-OQ-034`/`035`: pengelolaan perangkat dan penguncian saat bed kosong sudah diputuskan |
| `NV-S8b` | `BUSINESS_DECISION_REQUIRED` | Menunggu `NV-OQ-034`, `035` — keduanya mengubah siklus status |
| `NV-S8c` | `BUSINESS_DECISION_REQUIRED` | Isi strip bergantung pada status yang ada di `NV-S8b` |
| `NV-S8d` | `BUSINESS_DECISION_REQUIRED` | Tombol konsol bergantung pada `NV-OQ-034` |

### R2.7 Yang boleh berjalan dan yang harus berhenti

- **Boleh berjalan:** desain `NV-S8a` (perangkat tablet bed). `hospital-domain-architect`
  dilewati (`DOMAIN_ARCHITECTURE_NOT_RUN`): seluruh slice berada di satu bounded context Nurse
  View, tidak memiliki master bersama, tidak berdampak billing, dan batas keselamatan klinisnya
  diputuskan owner.
- **Harus berhenti:** desain `NV-S8b` s.d. `S8d` sampai `NV-OQ-034` dan `035` diputuskan.
  Karena keempat bagian dirancang dalam satu blueprint revision 2 (`SINGLE`), yang paling
  praktis adalah menutup kedua keputusan dulu, lalu mendesain seluruh `NV-S8` sekaligus.

### R2.8 Keputusan pemilik yang dibutuhkan

| ID | Pertanyaan | Pemilik | Skill |
|---|---|---|---|
| `NV-OQ-034` | Penutupan cadangan bila "Perawat hadir" tidak bisa ditekan | Rizki Gunawan | `grill-me` |
| `NV-OQ-035` | Boleh tidaknya pasien membatalkan panggilan | Rizki Gunawan | `grill-me` |

Butir `PROPOSED` `G-09` s.d. `G-17` dituliskan **sebagai usulan** di desain, dengan tanda yang
jelas, agar bisa disahkan atau ditolak owner saat approval blueprint revision 2.

### R2.9 Handoff

| Field | Nilai |
|---|---|
| Skill berikutnya | `grill-me` untuk `NV-OQ-034`, `035`; sesudahnya `design-business-module` |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` — alasan di R2.7 |
| Bagian siap dikirim | `NV-S8a` |
| Bagian tertahan | `NV-S8b`, `S8c`, `S8d` |
| Decision ID dibawa | `NV-DEC-034` s.d. `052` |
| Gap dibawa | `G-09` s.d. `G-17` |
| Gerbang implementasi dibawa | `NV-OQ-028`, `NV-OQ-022` |

### R2.10 Pembaruan sesudah closure (2026-11-05)

Kedua keputusan pemblokir ditutup owner lewat `grill-me`:

| ID | Keputusan | Dampak pada penilaian |
|---|---|---|
| `NV-OQ-034` | `NV-DEC-053` — pemegang izin boleh "Tutup panggilan" dari konsol dengan alasan wajib; tercatat "tidak dikonfirmasi di bed"; tanpa kedaluwarsa otomatis | Dimensi 05, 08, 13, 17 untuk `NV-S8b`/`S8d` menjadi `C` |
| `NV-OQ-035` | `NV-DEC-054` — pasien boleh membatalkan panggilan biasa yang masih `Menunggu`; Darurat dan yang sudah diklaim tidak bisa | Dimensi 08, 13 untuk `NV-S8b` menjadi `C` |

`G-13` (Darurat tidak bisa diturunkan pasien) **tetap `PROPOSED`**. Pertanyaan `NV-OQ-035`
sempat menyebutnya sebagai sudah diputuskan; itu kekeliruan penanya, bukan keputusan owner,
sehingga statusnya tidak dinaikkan.

| Bagian | Kesiapan sesudah closure |
|---|---|
| `NV-S8a` | `READY_FOR_DOMAIN_DESIGN` |
| `NV-S8b` | `READY_FOR_DOMAIN_DESIGN` |
| `NV-S8c` | `READY_FOR_DOMAIN_DESIGN` |
| `NV-S8d` | `READY_FOR_DOMAIN_DESIGN` |

**Handoff diperbarui:** skill berikutnya `design-business-module` untuk seluruh `NV-S8`;
`DOMAIN_ARCHITECTURE_NOT_RUN`; Decision ID dibawa `NV-DEC-034` s.d. `054`; gap `G-09` s.d.
`G-17` sebagai usulan bertanda; gerbang implementasi `NV-OQ-028` (lead) dan `NV-OQ-022`
(Komite Keperawatan, runtime).

**Approval 2026-11-05.** Usulan `G-09`–`G-14`, `G-16`, `G-17` disahkan owner bersama blueprint
revision 2 (`NV-DEC-055`), sehingga statusnya kini `CONFIRMED`. `G-15` tetap `CONFIGURABLE_DEFAULT`
sampai Komite Keperawatan mengesahkan angka ambang.
