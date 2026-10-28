# Penilaian Kelengkapan Requirement — Nurse View

| Field | Nilai |
|---|---|
| Blueprint ID | `nurse-view` |
| Revision | `1` |
| Status | `draft` |
| Owner | Rizki Gunawan |
| Tanggal | 2026-10-28 |
| Masukan | `00-interview-decisions.md` revision `2` (`NV-DEC-004` s.d. `026`, `NV-AC-001` s.d. `022`); `01-existing-capability-map.md` revision `1`; `rawat-inap/keperawatan/docs/task-lintas-modul/TLM-NV-001`; `rawat-inap/episode-rawat-inap/docs/task-lintas-modul/TLM-NV-001` |
| Backend SHA | `a6f977a3` (branch `rizkiG`) |
| Frontend SHA | `76b0c6e4b` (branch `RizkiV2`) |
| Baseline rujukan | `indonesia-hospital-domain-reference` **tidak dipakai**. Seluruh keputusan inti sudah dikonfirmasi owner; pembanding praktik rumah sakit lain dicatat di decision log sebagai pembanding, bukan kebijakan |
| Hasil | **`READY_FOR_DOMAIN_DESIGN`** untuk ketujuh slice; nol butir `BLOCKING` bagi desain |

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
