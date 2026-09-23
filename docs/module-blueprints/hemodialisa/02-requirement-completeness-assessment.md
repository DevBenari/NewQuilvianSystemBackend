# Hemodialisa — Penilaian Kelengkapan Requirement

| Field | Value |
|---|---|
| Assessment ID | `HMD-RCG-001` |
| Revision | `3` |
| Blueprint ID | `HMD-BP-001` |
| Status | `draft` |
| **Kesiapan modul** | **`READY_FOR_DOMAIN_DESIGN`** — 12 dari 12 slice boleh maju (r3, setelah `HMD-DEC-012` dan `HMD-DEC-013`) |
| Masukan | `00-interview-decisions.md` revision 12; `01-existing-capability-map.md` revision 1 |
| Decision yang berlaku | `HMD-DEC-001` sampai `HMD-DEC-004`, `HMD-DEC-006` sampai `HMD-DEC-011` |
| Backend SHA | `69b256ca` — branch `MHamzah` |
| Frontend SHA | `8143874d8` — branch `HamzahV2` |
| Tanggal penilaian | 18 September 2026 |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` — `hospital-domain-architect` tidak dipakai; skill itu opsional dan bukan gerbang wajib |
| `indonesia-hospital-domain-reference` | Tidak dipakai. Bukti requirement sudah cukup tebal dari tiga PRD dan audit source; baseline rujukan tidak diperlukan untuk deteksi gap |
| Task mode | `MODULE BLUEPRINT MODE` — read-only terhadap source aplikasi |

> **Cara membaca dokumen ini.**
> Dokumen ini menjawab satu pertanyaan: **apakah requirement-nya sudah cukup lengkap untuk
> mulai dirancang.** Ia tidak merancang apa pun, tidak membuat tabel, dan tidak menjawab
> pertanyaan bisnis atas nama siapa pun.
>
> | Status bukti | Artinya |
> |---|---|
> | `CONFIRMED` | Didukung bukti yang cukup berwenang |
> | `PROPOSED` | Usulan atau dugaan masuk akal, **belum** dikonfirmasi pihak berwenang |
> | `MISSING` | Informasinya tidak ada dalam bukti yang tersedia |
> | `CONFLICT` | Dua sumber bertentangan dan tidak bisa diselesaikan dari wewenang atau urutan waktu |
>
> | Dampak gap | Artinya |
> |---|---|
> | `BLOCKING` | Desain harus berhenti untuk slice yang terdampak |
> | `NON_BLOCKING_STANDARD` | Usulan standar boleh dipakai, asal terlihat dan tidak disamarkan jadi kebijakan RS |
> | `CONFIGURABLE_DEFAULT` | Wajar berbeda antar rumah sakit; aman dimodelkan sebagai pengaturan |

---

## 1. Scope Penilaian

Modul: **Hemodialisa**, Phase 1 saja (`HMD-DEC-001`). Dua puluh enam kemampuan: 25 Feature
`MUST HAVE` ditambah `HMD-CAP-001` (permintaan HD masuk).

Kemampuan dinilai per **slice proses**, bukan per modul utuh. Satu slice adalah potongan
proses terkecil yang masih bermakna dan bisa dipikirkan sendiri — misalnya "menjadwalkan sesi"
dapat dinilai terpisah dari "mengerjakan sesi", karena keduanya dikerjakan orang berbeda pada
waktu berbeda.

| Slice | Nama | Kemampuan | Epic PRD |
|---|---|---|---|
| `S1` | Permintaan HD masuk | `HMD-CAP-001` | — (baru, `HMD-DEC-008`) |
| `S2` | Penerimaan pasien dan episode HD | `FEAT-001`, `FEAT-003` | HD-01 |
| `S3` | Kelayakan, akses vaskular, persetujuan tindakan | `FEAT-004`, `FEAT-005`, `FEAT-006` | HD-02 |
| `S4` | Resep HD dan revisinya | `FEAT-007`, `FEAT-008` | HD-03 |
| `S5` | Penjadwalan sesi | `FEAT-009` | HD-03 |
| `S6` | Kesiapan unit: mesin, air, BMHP, isolasi | `FEAT-012`, `FEAT-026`, `FEAT-027`, `FEAT-028`, `FEAT-029` | HD-04 |
| `S7` | Kompetensi dan kapasitas staf | `FEAT-030` | HD-04 |
| `S8` | Pra-HD dan memulai sesi | `FEAT-010`, `FEAT-011`, `FEAT-013` | HD-05 |
| `S9` | Pemantauan, obat, komplikasi | `FEAT-014`, `FEAT-015`, `FEAT-016` | HD-06 |
| `S10` | Pasca-HD, disposisi, finalisasi | `FEAT-017`, `FEAT-018`, `FEAT-019` | HD-07 |
| `S11` | Serah terima ke Billing | `FEAT-033` | HD-08 |
| `S12` | Audit dan koreksi rekam medis | `FEAT-036` | HD-08 |

Seluruh 26 kemampuan punya slice pemilik. Tidak ada kemampuan yatim.

---

## 2. Bukti yang Dipakai

| Sumber | Wewenang | Catatan |
|---|---|---|
| `PRD TO MVP—Hemodialisa Quilvian V2.md` | Bukti bisnis dari analis, belum disetujui | Status `DRAFT`. Baseline source-nya sudah kedaluwarsa (`896014ec`) |
| `PRD Phase 2` dan `PRD Phase 3` | Bukti bisnis, konteks saja | Di luar scope Phase 1; dipakai hanya untuk mengenali titik sentuh maju |
| `00-interview-decisions.md` revision 12 | **Keputusan pemilik modul** | Sepuluh keputusan `approved` atas nama Muhammad Hamzah |
| `01-existing-capability-map.md` revision 1 | **Bukti implementasi terverifikasi** | Audit langsung pada BE `69b256ca` + FE `8143874d8` |
| Source backend dan frontend | Bukti implementasi terkini | Dipakai untuk pertanyaan "apa yang sekarang ada" |
| SOP / kebijakan rumah sakit | **Tidak tersedia** | Tidak ada satu pun SOP Hemodialisa yang disahkan dalam bukti. Ini sebab utama beberapa butir berstatus `PROPOSED` |
| Notulen rapat / keputusan Zoom | **Tidak tersedia** | — |

**Catatan wewenang yang penting:** tidak ada satu pun SOP rumah sakit dalam bukti. Akibatnya
setiap butir yang seharusnya lahir dari SOP klinis berstatus `PROPOSED`, bukan `CONFIRMED` —
sekalipun isinya masuk akal dan sudah tertulis rapi di PRD. Ini bukan kelemahan dokumen PRD;
ini keadaan yang harus terlihat.

---

## 3. Temuan Kelengkapan per Dimensi

Tabel berikut menilai 12 slice terhadap 18 dimensi kelengkapan. Bacaannya per baris.

Keterangan: **C** = `CONFIRMED`, **P** = `PROPOSED`, **M** = `MISSING`, **X** = `CONFLICT`,
**—** = tidak berlaku secara material bagi slice itu.

| Dimensi | S1 | S2 | S3 | S4 | S5 | S6 | S7 | S8 | S9 | S10 | S11 | S12 |
|---|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|
| 01 Tujuan | C | C | C | C | C | C | C | C | C | C | C | C |
| 02 Aktor | P | C | C | C | C | C | C | C | C | P | C | C |
| 03 Pemicu / prasyarat | C | C | C | C | C | C | C | C | C | C | C | C |
| 04 Alur utama | C | C | C | C | C | C | C | C | C | C | C | C |
| 05 Alur alternatif / exception | P | C | C | C | C | P | C | P | C | C | C | C |
| 06 Data minimum | C | C | C | C | C | C | C | C | C | C | C | C |
| 07 Aturan bisnis / validation | P | P | C | C | P | P | C | P | C | P | C | C |
| 08 Status / perubahan status | C | C | C | C | C | C | — | C | C | C | C | C |
| 09 Peran / authorization | P | C | C | C | C | C | C | P | C | P | C | C |
| 10 Dependency antarmodul | C | C | C | C | C | C | C | C | C | C | C | C |
| 11 Integrasi internal / eksternal | C | C | P | — | — | P | P | — | C | — | C | C |
| 12 Hasil akhir | C | C | C | C | C | C | C | C | C | C | C | C |
| 13 Pembatalan / koreksi | P | C | C | C | C | C | — | C | C | C | C | C |
| 14 Audit / histori | C | C | C | C | C | C | C | C | C | C | C | C |
| 15 Notifikasi | P | — | — | — | P | P | — | — | — | — | — | — |
| 16 Dampak billing / charge | — | — | — | — | — | — | — | — | P | — | C | — |
| 17 Dampak keselamatan klinis | C | C | C | C | C | C | P | P | C | P | — | C |
| 18 Pelaporan / traceability | C | C | C | C | C | C | C | C | C | C | C | C |

**Tidak ada lagi `MISSING` pada tabel ini.**

Pada revision 1 ada sembilan: lima di kolom **S7** dan empat di kolom **S11**. Keduanya tertutup
pada sesi `grill-me` 18 September 2026 — `HMD-DEC-012` menetapkan sesi yang dihentikan tidak
menagih otomatis, dan `HMD-DEC-013` menetapkan kewenangan petugas ditampilkan serta ditandai
sekarang lalu ditegakkan begitu Human Resource membuka pembacaannya.

Sisa butir `PROPOSED` pada kolom `S7` bukan kekurangan yang memblokir: ia menandai bahwa
kontrak pembacaan kewenangan (`HMD-DEP-002`) memang belum tersedia, dan itu sudah ditangani
lewat nilai status `Belum dapat diverifikasi`.

### Dimensi kondisional yang dinyatakan tidak berlaku

Kontrak menuntut alasan, bukan sekadar tanda hubung.

| Dimensi | Slice | Alasan tidak berlaku |
|---|---|---|
| 15 Notifikasi | S2, S3, S4, S7, S8, S9, S10, S11, S12 | PRD Phase 1 tidak memuat satu pun kebutuhan pemberitahuan otomatis, dan tidak ada kanal notifikasi yang dipakai modul klinis lain pada `69b256ca`. Memunculkannya di sini berarti mengarang requirement |
| 16 Dampak billing | S1–S8, S10, S12 | Hanya satu kejadian pada Phase 1 yang menimbulkan konsekuensi keuangan, yaitu tindakan HD selesai dan diserahkan ke Billing. Seluruh slice lain tidak membuat, mengubah, maupun membatalkan tagihan |
| 17 Dampak keselamatan klinis | S11 | Serah terima ke Billing terjadi **setelah** catatan klinis dikunci. `NFR-008` pada PRD menegaskan kegagalan Billing tidak membuka kembali catatan final, sehingga tidak ada jalur dari Billing yang dapat mengubah keputusan klinis |
| 08 Status, 13 Koreksi | S7 | Kompetensi staf adalah data rujukan milik Human Resource. Hemodialisa tidak memiliki siklus hidupnya dan tidak berwenang mengoreksinya |
| 11 Integrasi | S4, S5, S8, S10 | Keempatnya berjalan sepenuhnya di dalam Hemodialisa dan modul yang datanya hanya dibaca, tanpa memanggil sistem lain |

---

## 4. Butir CONFIRMED, PROPOSED, MISSING, dan CONFLICT

### 4.1 Yang sudah `CONFIRMED`

Butir berikut berdiri di atas bukti yang cukup berwenang — keputusan pemilik modul yang
`approved`, atau source code yang diverifikasi langsung.

| ID | Pernyataan | Bukti |
|---|---|---|
| `RCG-C-01` | Hemodialisa tidak membuat pasien, kunjungan, hasil lab, stok obat, maupun tagihan sendiri | `HMD-DEC-003`; capability map `CAP-01` sampai `CAP-12` |
| `RCG-C-02` | Episode HD terpisah dari episode rawat inap | PRD §4; `HMD-FACT-007` |
| `RCG-C-03` | Serah terima ke Billing memakai sumber `Procedure` yang sudah terdaftar, bukan sumber baru | `BillingSourceContract.cs@69b256ca`; capability map `CAP-11` |
| `RCG-C-04` | Catatan sesi final dikunci lewat mekanisme keutuhan dokumen Rekam Medis, koreksi lewat *addendum* | capability map `CAP-07`, `CAP-08` |
| `RCG-C-05` | Permintaan HD masuk memakai bentuk `LabOrder`/`RadOrder` | `HMD-DEC-008`; `HMD-FACT-025` |
| `RCG-C-06` | `DoctorId` diisi dokter penanggung jawab sesi, `InstructingDoctorId` diisi dokter pembuat resep | `HMD-DEC-009`; `HMD-FACT-005`, `HMD-FACT-006` |
| `RCG-C-07` | Setiap sesi wajib punya dokter penanggung jawab sebelum boleh dimulai | `HMD-DEC-009` |
| `RCG-C-08` | Siklus hidup episode, resep, dan sesi sebagaimana tertulis pada PRD §11 | PRD §11; ditegaskan `HMD-DEC-001` |
| `RCG-C-09` | Mesin `Blocked`, `Maintenance`, `NotEligible` tidak boleh dipakai | PRD FR-HD-010; `HMD-AC-005` |
| `RCG-C-10` | Tidak boleh ada tabrakan jadwal pada pasien, mesin, maupun station | PRD FR-HD-008; `HMD-AC-002` sampai `HMD-AC-004` |
| `RCG-C-11` | Mulai sesi bersifat idempotent — klik dua kali tetap satu sesi dan satu tindakan | PRD FR-HD-017; `HMD-AC-006` |
| `RCG-C-12` | Riwayat pemantauan berkala tidak boleh saling menimpa | PRD FR-HD-018; `HMD-AC-007` |
| `RCG-C-13` | Kegagalan Billing tidak membuka kembali catatan yang sudah final | PRD NFR-008; `HMD-AC-009` |
| `RCG-C-14` | Hak akses ditegakkan di server, bukan sekadar tombol dimatikan | PRD §14; `HMD-AC-012` |
| `RCG-C-15` | Waktu kritis memakai waktu server | PRD NFR-003; `HMD-AC-011` |

### 4.2 Yang `PROPOSED` — masuk akal, belum dikonfirmasi pihak berwenang

| ID | Pernyataan | Dampak gap | Slice |
|---|---|---|---|
| `RCG-P-01` | Tidak ada satu pun item Pra-HD yang boleh dilewati sampai badan klinis memutuskan | `CONFIGURABLE_DEFAULT` | S8 |
| `RCG-P-02` | Finalisasi dua langkah: perawat menyelesaikan dokumentasi, dokter penanggung jawab mengesahkan | `CONFIGURABLE_DEFAULT` | S10 |
| `RCG-P-03` | Menahan permintaan boleh koordinator unit; menolak permintaan hanya dokter | `CONFIGURABLE_DEFAULT` | S1 |
| `RCG-P-04` | Satu pasien hanya boleh punya satu episode HD aktif | `CONFIGURABLE_DEFAULT` | S2 |
| `RCG-P-05` | Satu perawat menangani paling banyak tiga pasien per sesi | `CONFIGURABLE_DEFAULT` | S5, S7 |
| `RCG-P-06` | Hasil pemeriksaan air punya masa berlaku; lewat masa itu unit tidak lagi `Ready` | `CONFIGURABLE_DEFAULT` | S6 |
| `RCG-P-07` | Aturan isolasi Hepatitis B tidak otomatis berlaku untuk HCV dan HIV | `NON_BLOCKING_STANDARD` | S6 |
| `RCG-P-08` | Status serologi dicatat manual oleh petugas berwenang, merujuk hasil Laboratorium | `NON_BLOCKING_STANDARD` | S3 |
| `RCG-P-09` | Pemberitahuan permintaan HD baru ke unit HD dilakukan lewat daftar kerja, bukan notifikasi aktif | `NON_BLOCKING_STANDARD` | S1, S5 |
| `RCG-P-10` | Pembatalan permintaan HD sebelum dijadwalkan boleh dilakukan pembuatnya | `NON_BLOCKING_STANDARD` | S1 |

**Contoh mengapa `RCG-P-05` hanya `PROPOSED`:** PRD mengutip ketentuan JDIH bahwa satu perawat
dapat menangani tiga pasien per sesi. Kutipan itu benar sebagai rujukan, tetapi rumah sakit ini
belum menyatakan bahwa angka tiga diberlakukan sebagai batas keras yang menolak penjadwalan.
Bisa saja kebijakan lokalnya dua pada shift malam. Karena itu angkanya dimodelkan sebagai
pengaturan, bukan ditanam di kode.

### 4.3 Yang `MISSING` — dan material

| ID | Yang tidak ada | Dampak gap | Slice | Decision ID |
|---|---|---|---|---|
| ~~`RCG-M-01`~~ | ~~Kompetensi petugas ditegakkan atau ditampilkan~~ **TERTUTUP** oleh `HMD-DEC-013` | — | S7 | `DEC-HMD-001` `CLOSED` |
| ~~`RCG-M-02`~~ | ~~Apakah sesi yang dihentikan di tengah jalan tetap ditagihkan~~ **TERTUTUP** oleh `HMD-DEC-012` | — | S11 | `DEC-HMD-002` `CLOSED` |
| `RCG-M-03` | Daftar isian minimum yang harus lengkap sebelum sesi boleh difinalisasi | `NON_BLOCKING_STANDARD` | S10 | — |
| `RCG-M-04` | Berapa lama hasil pemeriksaan air masih dianggap berlaku | `CONFIGURABLE_DEFAULT` | S6 | — |
| `RCG-M-05` | Apa arti "program HD yang sama" pada aturan satu episode aktif | `CONFIGURABLE_DEFAULT` | S2 | — |
| `RCG-M-06` | Apakah permintaan HD yang ditolak boleh diajukan ulang, dan oleh siapa | `NON_BLOCKING_STANDARD` | S1 | — |

### 4.4 `CONFLICT`

| ID | Pertentangan | Penyelesaian |
|---|---|---|
| ~~`RCG-X-01`~~ **TERTUTUP** `HMD-DEC-013` | PRD FR-HD-013 menuntut *"MVP harus bisa memvalidasi kapasitas terhadap staf kompeten"*, sedangkan audit source menunjukkan **tidak ada** satu pun cara membaca kewenangan klinis seorang petugas dari luar modul Credentialing (`HMD-DEP-002`, capability map *Temuan Kritis 3*) | **Diselesaikan** `HMD-DEC-013`: tuntutan PRD tetap dihormati sebagai tujuan, tetapi penegakannya bertahap. Status verifikasi disimpan sejak awal dengan nilai `Belum dapat diverifikasi`, sehingga ketiadaan pemeriksaan tercatat jujur alih-alih disamarkan |

Tidak ada `CONFLICT` lain. Dua conflict yang sudah tercatat di decision log — `HMD-CONF-001`
dan `HMD-CONF-002` — masing-masing sudah ditutup `HMD-DEC-008` dan berada di luar Phase 1.

---

## 5. Decision Log

Dua keputusan memblokir. Keduanya bergantung pemilik dan **tidak** dijawab di sini.

### `DEC-HMD-001` — Kompetensi petugas: ditegakkan atau ditampilkan?

**Pertanyaan.** Ketika koordinator menugaskan seorang perawat pada sesi HD, apakah sistem harus
**menolak** penugasan bila perawat itu tidak punya kewenangan dialisis yang masih berlaku, atau
cukup **menampilkan** status kewenangannya dan membiarkan koordinator memutuskan?

**Kemampuan yang terdampak.** `S7` — `FEAT-030` kompetensi dan kapasitas staf. Menyentuh juga
butir checklist "staff availability" pada `S8`.

**Bukti saat ini.** PRD FR-HD-013 memakai kata *memvalidasi*, yang berarti menegakkan. Tetapi
audit source menunjukkan data kewenangan memang lengkap di `WfpClinicalPrivilege` — sampai
`PrivilegeCode`, `ProcedureGroup`, dan `PracticeLocation` — sementara **seluruh** pembacanya
masih berada di dalam modul Credentialing sendiri. Belum pernah ada modul klinis yang
menanyakannya.

**Usulan baseline.** Tidak ada usulan yang aman. Inilah yang membuatnya memblokir: aturan
*fail-closed* biasa — "tolak bila ragu" — justru membuat modul tidak dapat dipakai sama sekali,
karena tanpa jalan baca, **setiap** petugas akan terbaca sebagai tidak berwenang dan tidak ada
sesi yang bisa dijadwalkan.

**Dampak.** Authorization, keselamatan klinis, kontrak lintas modul dengan Human Resource, dan
apakah `HMD-DEP-002` menjadi pekerjaan wajib sebelum rilis atau tidak.

**Pemilik yang dibutuhkan.** Badan klinis (belum ditunjuk, `HMD-GATE-001`) bersama pemilik
modul Human Resource.

**Status.** `OPEN`.

**Dampak implementasi.** `S7` berhenti. Sebelas slice lain jalan. Penugasan petugas ke sesi
tetap boleh dirancang — yang berhenti hanya pemeriksaan kewenangannya.

### `DEC-HMD-002` — Sesi yang dihentikan di tengah jalan: ditagihkan atau tidak?

**Pertanyaan.** Seorang pasien mulai cuci darah, lalu 40 menit kemudian tekanan darahnya turun
tajam dan sesi dihentikan. Tindakan sudah benar-benar dikerjakan, mesin sudah dipakai, dializer
dan selang sudah terbuang. Apakah sesi itu menghasilkan tagihan? Penuh, sebagian, atau tidak
sama sekali — dan siapa yang memutuskannya?

**Kemampuan yang terdampak.** `S11` — `FEAT-033` serah terima ke Billing.

**Bukti saat ini.** PRD §9 langkah 27–29 hanya menggambarkan jalur berhasil: tindakan selesai →
fakta tagihan → Billing. PRD §11.3 mengakui sesi bisa berakhir `Stopped`, bukan hanya
`Completed`, tetapi tidak pernah menyambungkan `Stopped` ke Billing. UAT-15 juga hanya menguji
jalur berhasil: *"Session final dan procedure selesai → Billing menerima tepat satu charge"*.

**Usulan baseline.** Tidak ada yang aman. Menagih penuh berpotensi menagih pasien untuk layanan
yang tidak tuntas; tidak menagih sama sekali membuat rumah sakit menanggung bahan habis pakai
yang benar-benar terpakai. Keduanya keputusan bisnis, bukan keputusan teknis.

**Preseden yang relevan.** Radiologi sudah menghadapi pertanyaan serupa dan menjawabnya tegas
di dalam kode: `RJ-BIL-GATE-DEC-004` menyatakan `Requested`, `Accepted`, dan `Scheduled`
**bukan** pemicu tagihan — yang menagih hanyalah pemeriksaan yang benar-benar dikerjakan dan
menghasilkan citra yang dapat dipakai. Hemodialisa membutuhkan pernyataan setara, dan belum
punya.

**Dampak.** Konsekuensi tagihan, definisi kejadian yang memicu tagihan, dan jalur pembatalan
tagihan bila sesi dibatalkan setelah dimulai.

**Pemilik yang dibutuhkan.** Pemilik modul bersama pemilik Billing dan Kasir; bila menyangkut
keputusan menghentikan sesi, badan klinis ikut.

**Status.** `OPEN`.

**Dampak implementasi.** `S11` berhenti. `S10` — pasca-HD, disposisi, dan finalisasi — **tetap
jalan**, karena finalisasi catatan klinis tidak bergantung pada apakah tagihan terbit.

---

## 6. Kesiapan per Slice

| Slice | Kemampuan | Kesiapan | Catatan |
|---|---|---|---|
| `S1` | Permintaan HD masuk | `READY_FOR_DOMAIN_DESIGN` | Kewenangan menolak/menahan berjalan dengan `HMD-ASM-003` |
| `S2` | Penerimaan dan episode HD | `READY_FOR_DOMAIN_DESIGN` | Arti "program yang sama" dimodelkan sebagai pengaturan |
| `S3` | Kelayakan, akses vaskular, consent | `READY_FOR_DOMAIN_DESIGN` | Status serologi dicatat manual selama `HMD-DEP-001` belum tersedia |
| `S4` | Resep HD dan revisinya | `READY_FOR_DOMAIN_DESIGN` | Paling lengkap buktinya di antara seluruh slice |
| `S5` | Penjadwalan sesi | `READY_FOR_DOMAIN_DESIGN` | Batas rasio perawat sebagai pengaturan, bawaan tidak menolak |
| `S6` | Kesiapan unit, mesin, air, BMHP, isolasi | `READY_FOR_DOMAIN_DESIGN` | Masa berlaku hasil air sebagai pengaturan |
| `S7` | Kompetensi dan kapasitas staf | `READY_FOR_DOMAIN_DESIGN` | Dibuka `HMD-DEC-013`, 18 September 2026 |
| `S8` | Pra-HD dan memulai sesi | `READY_FOR_DOMAIN_DESIGN` | Daftar item yang boleh dilewati dibangun sebagai data dan dibiarkan kosong |
| `S9` | Pemantauan, obat, komplikasi | `READY_FOR_DOMAIN_DESIGN` | — |
| `S10` | Pasca-HD, disposisi, finalisasi | `READY_FOR_DOMAIN_DESIGN` | Dirancang untuk dua pelaku, lihat syarat di bawah |
| `S11` | Serah terima ke Billing | `READY_FOR_DOMAIN_DESIGN` | Dibuka `HMD-DEC-012`, 18 September 2026 |
| `S12` | Audit dan koreksi rekam medis | `READY_FOR_DOMAIN_DESIGN` | — |

**Kesiapan modul: `READY_FOR_DOMAIN_DESIGN`.** Dua belas slice boleh maju. Tidak ada slice yang berhenti.

### Dependency antar slice yang perlu dijaga

| Yang bergantung | Bergantung pada | Apakah menghambat? |
|---|---|---|
| `S8` butir checklist "staff availability" | `S7` | **Tidak.** Butir checklist tetap dirancang; yang tertunda hanya pemeriksaan kewenangannya |
| `S5` penugasan perawat ke sesi | `S7` | **Tidak.** Penugasan boleh dirancang; yang tertunda validasi kompetensinya |
| `S11` | `S10` | Searah. `S10` tidak bergantung pada `S11`, sehingga finalisasi boleh dirancang penuh |
| `S3` tampilan serologi | `HMD-DEP-001` | **Tidak.** Pencatatan manual adalah jalur yang sah dan sudah cukup untuk MVP |

---

## 7. Syarat yang Melekat pada Slice yang Boleh Jalan

Tiga slice boleh maju **hanya dengan syarat**. Syarat ini lahir dari prinsip yang sama:
melonggarkan aturan itu murah, mengetatkannya mahal.

| Slice | Syarat | Sebabnya |
|---|---|---|
| `S8` | Daftar item Pra-HD yang boleh dilewati **wajib** disimpan sebagai data, bukan ditanam di kode | Badan klinis nanti cukup mengisi daftarnya. Bila ditanam di kode, setiap perubahan kebijakan jadi pekerjaan pengembangan ulang |
| `S10` | Catatan sesi **wajib** menyimpan dua pelaku terpisah: siapa menyelesaikan dokumentasi dan siapa mengesahkan | `HMD-ASM-002` memilih alur dua langkah. Bila kelak badan klinis memilih satu langkah, kolom kedua tinggal dikosongkan. Bila dirancang satu kolom lalu ternyata butuh dua, itu migration |
| `S5`, `S6` | Batas rasio perawat dan masa berlaku hasil air **wajib** berupa pengaturan bernilai awal, bukan angka tetap | Keduanya wajar berbeda antar rumah sakit dan antar shift |

Syarat ini **mengikat** `design-business-module`. Slice yang dirancang tanpa memenuhinya
dihitung belum memenuhi gerbang ini.

---

## 8. Apa yang Boleh Jalan dan Apa yang Harus Berhenti

### Boleh jalan sekarang

**Seluruh dua belas slice**, mencakup 26 dari 26 kemampuan — dari permintaan HD masuk sampai
catatan dikunci, diserahkan ke Billing, dan dikoreksi lewat *addendum*.

### Harus berhenti

**Tidak ada.** Dua slice yang berhenti pada revision 1 sudah dibuka:

| Slice | Dibuka oleh | Syarat yang melekat |
|---|---|---|
| `S7` Kompetensi dan kapasitas staf | `HMD-DEC-013` | Status verifikasi kewenangan **wajib** punya tiga nilai sejak awal, dan penegakan **wajib** berupa pengaturan — bukan penulisan ulang kode |
| `S11` Serah terima ke Billing | `HMD-DEC-012` | Sesi `Stopped` **wajib** menerbitkan tindakan dengan penanda tidak dapat ditagih beserta alasan, bukan menghilangkan tindakannya |

**Yang tetap tidak boleh dilakukan:** merancang `S11` dengan mengasumsikan hanya sesi
`Completed` yang ada, lalu menambahkan penanganan `Stopped` belakangan. Pemicu tagihan adalah
bagian dari kontrak Billing, dan mengubahnya setelah ada tagihan berjalan berarti menyentuh
data keuangan yang sudah terbit — apalagi karena jalur batal normal tertutup begitu tagihan
mencapai `COMPLETED`.

---

## 9. Keputusan Pemilik yang Dibutuhkan

| Decision ID | Pertanyaan ringkas | Pemilik | Memblokir |
|---|---|---|---|
| ~~`DEC-HMD-001`~~ | **TERTUTUP** `HMD-DEC-013`, 18 September 2026 — ditampilkan dan ditandai sekarang, ditegakkan begitu `HMD-DEP-002` tersedia | — | — |
| ~~`DEC-HMD-002`~~ | **TERTUTUP** `HMD-DEC-012`, 18 September 2026 — sesi `Stopped` tidak menagih otomatis; penagihan bahan lewat `ADHOC_CATALOG` | — | — |

Keduanya diselesaikan lewat `grill-me`, bukan di sini. Gerbang ini tidak menjawab keputusan
bisnis atas nama siapa pun.

Empat keputusan yang sudah tercatat di decision log tetap berlaku dan **tidak** memblokir
desain: `HMD-OQ-000`, `HMD-OQ-003`, `HMD-OQ-004`, dan `HMD-OQ-007`. Keempatnya sudah digeser
menjadi gerbang go-live oleh `HMD-DEC-010`, dan selama belum tertutup berjalan memakai
`HMD-ASM-001` sampai `HMD-ASM-003`.

Dua dependency lintas modul juga tetap berjalan paralel: `HMD-DEP-001` ke Laboratorium dan
`HMD-DEP-002` ke Human Resource. Yang kedua berkaitan erat dengan `DEC-HMD-001` — bila
jawabannya "ditegakkan", `HMD-DEP-002` naik menjadi pekerjaan wajib sebelum rilis.

### Kontrak as-is yang menjadi pokok `HMD-DEP-001`

#### Health Services / Laboratory Management / Lab Examination

Base URL: `api/v1/health-services/laboratory-management/lab-examinations`

| Method | Path | Kegunaan | Request | Response |
|---|---|---|---|---|
| `GET` | `/by-order/{labOrderId}` | Hasil pemeriksaan untuk satu pesanan | Path | `ApiResponse<...>` |
| `GET` | `/by-specimen/{specimenId}` | Hasil pemeriksaan untuk satu spesimen | Path | `ApiResponse<...>` |

*Rencana (belum tersedia)* — pembacaan per pasien per jenis pemeriksaan, yang dibutuhkan
Hemodialisa untuk menjawab *"berapa hasil HBsAg terakhir pasien ini?"*. Selama belum ada,
`S3` berjalan dengan pencatatan manual (`RCG-P-08`).

---

## 10. Handoff Berikutnya

| Tujuan | Slice yang dikirim | Alasan |
|---|---|---|
| `design-business-module` | **Seluruh dua belas slice** | Seluruhnya `READY_FOR_DOMAIN_DESIGN`. Syarat pada bagian 7 ikut terbawa dan mengikat |
| `grill-me` | Tidak ada | `DEC-HMD-001` dan `DEC-HMD-002` sudah ditutup `HMD-DEC-013` dan `HMD-DEC-012` |
| `hospital-domain-architect` | Tidak dipakai | Opsional dan bukan gerbang wajib. Batas bounded context, ownership, dan dampak billing sudah dapat diselesaikan dari bukti yang ada — capability map sudah memetakan seluruh kepemilikan data ke modul pemiliknya |
| `trace-existing-capabilities` | Tidak perlu diulang | Capability map revision 1 dibuat hari ini pada SHA yang sama dengan HEAD |

### Yang wajib ikut pada handoff

| Hal | Nilai |
|---|---|
| Modul dan kemampuan | Hemodialisa, Phase 1, 26 kemampuan |
| Revision bukti | `00-interview-decisions.md` r9; `01-existing-capability-map.md` r1 |
| Snapshot source | BE `69b256ca`, FE `8143874d8` |
| Decision ID terbuka | Tidak ada. `DEC-HMD-001` dan `DEC-HMD-002` tertutup 18 September 2026 |
| Dependency ID | `HMD-DEP-001`, `HMD-DEP-002` |
| Gerbang go-live | `HMD-GATE-001` sampai `HMD-GATE-006` |
| Kesiapan | `READY_FOR_DOMAIN_DESIGN` |
| Arsitektur domain | `DOMAIN_ARCHITECTURE_NOT_RUN` |
| Keluaran hilir yang diharapkan | Sebelas berkas desain mencakup seluruh dua belas slice |

---

## 11. Pemicu Penilaian Ulang

Tandai penilaian ini **stale** dan ulangi untuk bagian yang terdampak bila:

| Pemicu | Yang dinilai ulang |
|---|---|
| `DEC-HMD-001` ditutup | `S7`, dan syarat pada `S5`/`S8` yang menyinggung kompetensi |
| `DEC-HMD-002` ditutup | `S11`, dan dimensi 16 pada `S9` |
| SOP Hemodialisa rumah sakit terbit | Seluruh butir `PROPOSED` pada bagian 4.2 — sebagian besar akan naik menjadi `CONFIRMED` |
| Badan klinis ditunjuk (`HMD-GATE-001`) | `HMD-ASM-001` sampai `HMD-ASM-003`, dan butir `PROPOSED` yang bergantung padanya |
| `HMD-DEP-001` tersedia | `RCG-P-08`, dimensi 11 pada `S3` dan `S6` |
| SHA backend atau frontend berubah | Butir yang bersandar pada capability map; jalankan impact scan lebih dulu |
| Phase 2 dibuka | Seluruh penilaian ini — scope-nya Phase 1 saja |
