# State Transition Matrix — Modul Rawat Inap

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| `contract_version` | Mengikuti set kontrak pada [manifest](../blueprint-manifest.md), amandemen Bed Management draft. Riwayat metadata sebelumnya: **`0.11.0`** — bagian 10 Workspace PPRI, `approved` 2026-10-08 (`RWI-DEC-265`). Sebelumnya `0.10.0` — bagian 9, `approved` (`RWI-DEC-221`); `0.9.0` — bagian 8 |
| `last_changed_in` | **`0.12.0`** — Bed Management. Riwayat metadata sebelumnya: **`0.11.0`** — bagian 10: siklus dokumen admisi. Sebelumnya `0.10.0` — bagian 9; `0.9.0` — bagian 8: tujuan penugasan, akibat penutupan; `0.8.0` — bagian 6A |
| Status | **`draft`** — amandemen Bed Management belum disetujui. Riwayat metadata sebelumnya: **`draft`** untuk `0.9.0`. `0.8.0` **`approved`** — disetujui **Muhammad Hamzah** 2026-09-11 lewat `RWI-DEC-105` |
| Owner | Produk/domain/API Muhammad Hamzah; keamanan/privasi OPEN; frontend sesuai DEC-292. Riwayat metadata sebelumnya: Product/Domain Owner sementara sesuai `RWI-DEC-006` |
| `input_revision` | Decision 46; gate 1.12 / BM-RCG-20261010-01; audit BM-AUD-20261010-01 rev1. Riwayat metadata sebelumnya: `00-interview-decisions.md` revision `6`; `evidence/03-hospital-domain-architecture.md` revision `0.1` |
| Dampak kompatibilitas | Bed Management: GET aditif, mutation diperketat (expected version/key/category/reason); cutover seluruh konsumen wajib. Riwayat metadata sebelumnya: Seluruhnya baru. Tidak ada state machine existing yang berubah |

Dokumen ini memuat perpindahan yang **sah** dan perpindahan yang **tidak sah**. Keduanya sama
pentingnya: yang tidak sah adalah yang paling sering dicoba petugas ketika sedang terburu-buru.

> **`0.3.0` sengaja tidak menambah satu perpindahan pun.** `RWI-DEC-065` menetapkan kebutuhan
> isolasi sebagai **atribut** episode, bukan status. Ia dapat berubah bolak-balik kapan saja selama
> episode berjalan, tidak punya urutan yang sah maupun tidak sah, dan tidak menutup satu pun
> tindakan. Karena itu ia tidak masuk ke matriks ini; yang menjaganya adalah `GUARD-INP-04` pada
> `contracts/permission-audit-matrix.md` dan aturan 7 dan 8 pada Kelayakan Penempatan.
>
> Yang naik pada revisi ini hanyalah `contract_version`, supaya seluruh kontrak tetap sebaris.

---

## 1. Episode rawat inap

Status awal: `Draft`. Status akhir: `Closed` dan `Cancelled`.

### 1.1 Perpindahan yang sah

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| — | Buka admisi | `Draft` | Petugas admisi | Pasien terdaftar; kunjungan tersedia atau dibuat; DPJP pertama dipilih | 400 bila DPJP kosong |
| `Draft` | Tempatkan pasien | `Admitted` | Petugas admisi | Kelayakan Penempatan terpenuhi | 409 bila tempat tidur sudah ditempati; 422 bila tempat tidur tidak layak |
| `Draft` | Batalkan admisi | `Cancelled` | Petugas admisi | Alasan wajib | 400 bila alasan kosong |
| `Draft` | Telantar melewati batas | `Cancelled` | **Sistem**, dihitung saat dibaca | Tidak disentuh melewati `DraftEpisodeExpiryHours` | Tidak ada; ini perhitungan, bukan permintaan |
| `Admitted` | Batalkan admisi | `Cancelled` | Supervisor atau kepala ruangan | Belum ada catatan klinis; alasan wajib | 422 bila sudah ada catatan klinis |
| `Admitted` | Pindahkan pasien | `Admitted` | Kepala ruangan, perawat pelaksana, supervisor, atau DPJP aktif | Tempat tidur tujuan lolos Kelayakan Penempatan; alasan wajib | 409 bila tempat tidur tujuan terisi; 403 bila dokter bukan DPJP aktif |
| `Admitted` | Putuskan pasien boleh pulang | `DischargePending` | **DPJP aktif** | Cara pulang dipilih | 403 bila bukan DPJP aktif; 400 bila cara pulang kosong |
| `DischargePending` | **Catat kepergian fisik pasien** | `DischargePending` — **status tidak berubah** | Petugas admisi, perawat, kepala ruangan, supervisor | Kepergian belum pernah dicatat | 409 bila sudah pernah dicatat |
| `DischargePending` | Tutup episode | `Closed` | Petugas admisi | Kelima syarat penutupan terpenuhi | 422 disertai daftar syarat yang belum terpenuhi |
| `DischargePending` | Tutup menembus gerbang keuangan | `Closed` | **Supervisor** | Empat syarat selain kelayakan keuangan terpenuhi; alasan wajib | 422 bila ada syarat lain yang belum terpenuhi; 403 bila bukan supervisor |
| `Closed` | Buka sesi koreksi | `Closed` | **Supervisor** | Alasan wajib; tidak ada sesi lain yang masih terbuka | 409 bila sudah ada sesi terbuka |

### 1.2 Perpindahan yang **tidak sah**

| Dari status | Tindakan yang dicoba | Kenapa ditolak | Kode | Pesan bagi pengguna |
| --- | --- | --- | ---: | --- |
| `Draft` | Putuskan pasien boleh pulang | Pasien belum menempati tempat tidur, jadi belum ada yang bisa dipulangkan | 422 | "Pasien belum menempati tempat tidur. Selesaikan penempatan lebih dulu." |
| `Draft` | Tutup episode | Sama seperti di atas | 422 | "Episode belum berjalan, jadi belum dapat ditutup." |
| `Admitted` | Tutup episode langsung | Keputusan pulang milik DPJP dan tidak boleh dilewati | 422 | "Episode hanya dapat ditutup setelah DPJP menyatakan pasien boleh pulang." |
| `DischargePending` | Pindahkan pasien | Pasien sudah diputuskan pulang; perpindahan akan mengaburkan lokasi terakhir | 422 | "Pasien sudah diputuskan boleh pulang, sehingga tidak dapat dipindahkan lagi." |
| `DischargePending` yang kepergiannya sudah dicatat | Catat kepergian lagi | Kepergian fisik terjadi paling banyak sekali per episode | 409 | "Kepergian pasien sudah dicatat pada pukul 10:15." |
| `DischargePending` yang kepergiannya sudah dicatat | Batalkan pencatatan kepergian | Tidak disediakan. Pasien yang ternyata belum jadi pulang menjalani admisi baru | 404 | Endpoint tidak ada |
| `Admitted` | Catat kepergian fisik | Pasien belum diputuskan boleh pulang | 422 | "Kepergian hanya dapat dicatat setelah DPJP menyatakan pasien boleh pulang." |
| `DischargePending` | Kembali ke `Admitted` | Membatalkan keputusan pulang bukan perpindahan status, melainkan koreksi | 422 | "Keputusan pulang tidak dapat dibatalkan. Hubungi supervisor untuk koreksi." |
| `DischargePending` | Batalkan admisi | Pembatalan hanya untuk admisi yang tidak jadi berjalan, bukan untuk episode yang sudah selesai dirawat | 422 | "Episode yang sudah diputuskan pulang tidak dapat dibatalkan." |
| `Closed` | Tempatkan pasien, pindahkan, atau putuskan pulang | Episode sudah berakhir. Pasien yang kembali dirawat mendapat episode baru | 409 | "Episode sudah ditutup. Pasien yang kembali dirawat memerlukan admisi baru." |
| `Closed` | Ubah data tanpa sesi koreksi terbuka | `INV-INP-06` | 409 | "Episode sudah ditutup. Buka sesi koreksi lebih dulu." |
| `Cancelled` | Tindakan apa pun | Status akhir yang tidak dapat dilanjutkan | 409 | "Admisi ini sudah dibatalkan dan tidak dapat dilanjutkan." |
| Mana pun | Menyetel status langsung ke nilai bebas | Tidak ada endpoint yang menyediakannya, sesuai `RWI-RULE-031` aturan 4 | 404 | Endpoint tidak ada |

### 1.3 Contoh berangka

> Tn. Budi, episode `RI-2026-09-000123`.
>
> **21 Sept 09:15** — Sdri. Wati membuka admisi. Episode `Draft`, DPJP dr. Andi.
> **21 Sept 10:40** — Tn. Budi berbaring di `BD-RSMMC-00042`. Episode `Admitted`.
> **23 Sept 09:30** — dr. Andi memindahkan ke `BD-RSMMC-00105`. Episode tetap `Admitted`.
> **25 Sept 09:00** — dr. Rina, dokter jaga yang bukan DPJP, mencoba menyatakan Tn. Budi boleh
> pulang. **Ditolak 403** dengan pesan "Hanya DPJP episode ini yang dapat menyatakan pasien boleh
> pulang."
> **25 Sept 09:20** — dr. Andi menyatakan boleh pulang, cara pulang "atas izin DPJP". Episode
> `DischargePending`.
> **25 Sept 10:00** — Sdri. Wati mencoba menutup. **Ditolak 422**: resume belum ditandatangani,
> kelayakan keuangan masih `Pending`.
> **25 Sept 13:10** — resume tertandatangani, tiga butir administrasi tertandai, kasir menandai
> `Cleared`. Episode `Closed`, `BD-RSMMC-00105` kembali `Available`.

---

## 2. Pemesanan tempat tidur

Status awal: `Active`. Status akhir: `Consumed`, `Expired`, `Cancelled`.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| — | Pesan tempat tidur | `Active` | Petugas admisi | Episode `Draft`; tempat tidur lolos Kelayakan Penempatan | 409 bila sudah dipesan atau ditempati pasien lain |
| `Active` | Dipakai menempatkan pasien | `Consumed` | Petugas admisi | Pemesanan milik episode yang sama dan belum lewat batas | — |
| `Active` | Lewat batas waktu | `Expired` | **Sistem**, dihitung saat dibaca | Waktu sekarang melewati `ExpiresAt` | Tidak ada; ini perhitungan |
| `Active` | Batalkan pemesanan | `Cancelled` | Petugas admisi | — | — |
| `Active` | Admisi dibatalkan | `Cancelled` | Ikut pembatalan admisi | — | — |

### 2.1 Perpindahan yang **tidak sah**

| Dari status | Tindakan yang dicoba | Kenapa ditolak | Kode |
| --- | --- | --- | ---: |
| `Consumed` | Dipakai menempatkan lagi | Pemesanan hanya berlaku sekali | 409 |
| `Expired` | Dipakai menempatkan | Sudah gugur. Petugas harus memesan ulang atau menempatkan langsung | 409 |
| `Cancelled` | Tindakan apa pun | Status akhir | 409 |

### 2.2 Yang perlu dipahami tentang `Expired`

Pemesanan yang gugur **tidak** menghalangi admisi diteruskan. `RWI-RULE-015` menetapkan: bila
tempat tidur ternyata masih kosong, penempatan diteruskan tanpa peringatan walaupun pemesanannya
sudah gugur. Yang ditolak hanya bila tempat tidur itu sudah diambil pasien lain — dan penolakannya
membiarkan episode tetap `Draft` dengan seluruh isian admisi utuh.

---

## 3. Penempatan tempat tidur

Status awal: `Aktif`, yaitu `EndDateTime` kosong. Status akhir: `Berakhir`.

| Dari | Tindakan | Ke | Pemicu | Yang terjadi bersamaan |
| --- | --- | --- | --- | --- |
| — | Pasien menempati | `Aktif` | Penempatan atau perpindahan | `MstBed.BedStatus` menjadi `Occupied`. Untuk episode asal IGD, perpindahan ke `Aktif` **tidak boleh terjadi** sebelum catatan kepergian IGD bertanda `Tiba`, dan waktu mulainya dibaca dari sana — `RWI-DEC-072` |
| `Aktif` | Pasien pindah | `Berakhir`, `EndReason = Transfer` | Perpindahan | Penempatan baru dibuka; tempat tidur lama menjadi `Available` |
| `Aktif` | Kepergian fisik pasien dicatat | `Berakhir`, `EndReason = PatientDeparted` | Pencatatan kepergian | Tempat tidur menjadi `Available`. **Status episode tidak berubah** |
| `Aktif` | Episode ditutup | `Berakhir`, `EndReason = EpisodeClosed` | Penutupan | Tempat tidur menjadi `Available` |
| `Aktif` | Admisi dibatalkan | `Berakhir`, `EndReason = AdmissionCancelled` | Pembatalan | Tempat tidur menjadi `Available` |

Sejak `contract_version` `0.2.0` ada **satu** jalur yang menutup penempatan tanpa mengubah status
episode, yaitu pencatatan kepergian fisik pasien. Ini pelonggaran `INV-INP-01` yang disengaja dan
hanya berlaku untuk episode `DischargePending`, sesuai `RWI-DEC-055`.

Di luar jalur itu, tidak ada satu pun cara menutup penempatan tanpa menutup atau memindahkan
episodenya. `INV-INP-07` tetap berlaku utuh: pasien yang masih dirawat tidak pernah tercatat tanpa
tempat tidur.

## 3A. Kehadiran pasien — `INV-INP-10`

Ini bukan status yang disimpan, melainkan keadaan yang diturunkan. Dipakai menegakkan aturan satu
pasien satu episode.

| Keadaan | Artinya | Menghalangi admisi baru pasien yang sama |
| --- | --- | :---: |
| Episode `Draft` | Admisi sedang disiapkan, pasien belum tentu ada | Tidak. Hanya memunculkan peringatan |
| Episode `Admitted` | Pasien berada di ruangan | **Ya** |
| Episode `DischargePending`, kepergian belum dicatat | Pasien masih berada di ruangan | **Ya** |
| Episode `DischargePending`, kepergian sudah dicatat | Pasien sudah pulang, tinggal urusan administrasi | Tidak |
| Episode `Closed` atau `Cancelled` | Selesai | Tidak |

Baris keempat itulah alasan batasnya kepergian fisik, bukan penutupan: pasien yang sudah pulang
pukul 10:15 dan kembali dengan keluhan baru pukul 12:00 tidak boleh tertahan hanya karena episode
lamanya baru ditutup pukul 13:10.

---

## 4. Kelayakan keuangan

Status awal: `Pending`. Tidak ada status akhir; nilainya dapat berubah berkali-kali selama episode
belum ditutup.

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| `Pending` | Tandai lunas | `Cleared` | Petugas kasir atau billing | Catatan wajib |
| `Pending` | Tandai tertahan | `Blocked` | Petugas kasir atau billing | Catatan wajib |
| `Blocked` | Tandai lunas | `Cleared` | Petugas kasir atau billing | Catatan wajib |
| `Cleared` | Tandai tertahan kembali | `Blocked` | Petugas kasir atau billing | Catatan wajib. Berlaku bila ada tagihan susulan |

### 4.1 Perpindahan yang **tidak sah**

| Tindakan yang dicoba | Kenapa ditolak | Kode |
| --- | --- | ---: |
| Petugas admisi, perawat, atau dokter menandai kelayakan keuangan | Bukan wewenangnya | 403 |
| Menandai setelah episode `Closed` tanpa sesi koreksi | Episode sudah selesai | 409 |
| Menandai tanpa catatan | `RWI-RULE-028` aturan 4 | 400 |

---

## 5. Resume pulang

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| — | Susun resume | Belum ditandatangani | DPJP aktif | Episode `DischargePending` |
| Belum ditandatangani | Ubah isi | Belum ditandatangani | DPJP aktif | Episode belum `Closed` |
| Belum ditandatangani | Tandatangani | Tertandatangani | **DPJP aktif** | Isi wajib sesuai cara pulang sudah lengkap |
| Tertandatangani | Ubah isi | Tertandatangani | DPJP aktif | Hanya selama episode belum `Closed`. Tanda tangan diperbarui |
| Tertandatangani | Ubah isi setelah episode `Closed` | Tertandatangani, **versi lama disalin** | Supervisor | **Hanya** bila ada sesi koreksi terbuka |

### 5.1 Perpindahan yang **tidak sah**

| Tindakan yang dicoba | Kenapa ditolak | Kode |
| --- | --- | ---: |
| Dokter yang bukan DPJP aktif menandatangani | `RWI-RULE-032` aturan 4 | 403 |
| Membuat resume kedua untuk episode yang sama | `INV-INP-05` | 409 |
| Mengubah atau menghapus salinan versi resume yang tersimpan | `RWI-DEC-057` — salinan versi tidak dapat diubah | 404 |
| Menandatangani sementara cara pulang `Referred` tetapi tujuan rujukan kosong | `RWI-RULE-032` aturan 5 | 400 |

---

## 6. Sesi koreksi

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| — | Buka sesi | `Terbuka` | Supervisor | Episode `Closed`; alasan wajib; tidak ada sesi lain yang terbuka |
| `Terbuka` | Tutup sesi | `Tertutup` | Supervisor | Daftar perubahan wajib diisi |

**Status episode tetap `Closed` sepanjang sesi berjalan.** Ini yang membedakannya dari status
keenam, dan yang membuat `RWI-DEC-009` serta `RWI-AC-004` tidak dilanggar.

### 6.1 Yang tetap berlaku selama sesi terbuka

| Hal | Ketetapannya |
| --- | --- |
| Tempat tidur | **Tidak** dikembalikan |
| Census | Pasien **tidak** muncul |
| Lama dirawat | **Tidak** bertambah |
| Yang boleh diubah | Cara pulang, isi resume, dan catatan episode |
| Yang **tidak** boleh diubah | Waktu admisi, waktu penutupan, riwayat penempatan, dan riwayat status |

---


## 6A. Penugasan dokter berperiode — lahir `0.8.0`

**Bagian baru 11 September 2026**, menyerap `RWI-DEC-099`. Sebelum ini penugasan dokter tidak
pernah dimuat matriks ini, karena bentuknya periode dan bukan mesin status. Peran baru mengubah
keadaan itu: sejak `AssignmentRole` lahir, satu episode dapat memiliki **beberapa** penugasan
terbuka sekaligus, dan aturan yang menjaganya berbeda per peran.

### 6A.1 Dua keadaan, bukan mesin status

Satu baris penugasan hanya punya dua keadaan, dan keduanya dibaca dari `EndDateTime`:

| Keadaan | Cara membacanya | Arti bagi kewenangan menulis |
| --- | --- | --- |
| **Aktif** | `EndDateTime` kosong | Dokter boleh menulis untuk waktu klinis di dalam periodenya |
| **Berakhir** | `EndDateTime` terisi | Dokter **masih** boleh menulis untuk waktu klinis **di dalam** periode lama, dan **tidak boleh** untuk waktu klinis di luarnya |

Baris kedua itu yang sering salah dipahami. Penugasan yang berakhir **tidak** menghapus kewenangan
atas dokumen yang waktu klinisnya berada di dalam periode itu. Dokter yang lupa mencatat visite
kemarin sore tetap boleh mencatatnya hari ini, selama waktu klinis yang ia tulis memang jatuh
ketika ia masih bertugas. Ini yang dituntut `AC-MVP-012`, dan inilah alasan penilaian memakai
waktu klinis, bukan waktu penyimpanan.

### 6A.2 Berapa banyak yang boleh aktif bersamaan

| Peran | Batas penugasan aktif per episode | Dijaga oleh |
| --- | --- | --- |
| `Dpjp` | **Tepat satu** | Index unik `IX_InpDoctorAssignment_EpisodeId_ActiveDpjp` ditambah pemeriksaan service |
| `Consultant` | Banyak, tidak dibatasi | Tidak ada batas teknis; yang membatasi adalah kewajaran klinis |
| `OnCallDoctor` | Banyak, tidak dibatasi | Sama |

**Kenapa konsulen tidak dibatasi.** Satu pasien dapat dikonsultasikan ke penyakit dalam, bedah, dan
anestesi sekaligus pada hari yang sama. Membatasinya menjadi satu berarti memaksa kepala ruangan
menutup konsultasi yang masih berjalan hanya supaya konsultasi berikutnya dapat dibuat.

### 6A.3 Perpindahan yang sah

| Dari | Ke | Pemicu | Yang wajib ada | Yang dilarang |
| --- | --- | --- | --- | --- |
| Tidak ada DPJP | `Dpjp` aktif | Admisi episode | Dokter, waktu mulai | Membuat episode `Admitted` tanpa DPJP |
| `Dpjp` aktif | `Dpjp` aktif milik dokter lain | Pengalihan tanggung jawab | **Satu transaksi**: tutup baris lama dengan `EndDateTime`, buka baris baru, `HandoverReason` wajib | Dua baris `Dpjp` terbuka pada saat yang sama, walau sekejap |
| Tidak ada | `Consultant` aktif | Kepala ruangan atau supervisor melibatkan konsulen | Alasan pelibatan pada `HandoverReason` | Konsulen membuat penugasannya sendiri |
| Tidak ada | `OnCallDoctor` aktif | Kepala ruangan atau supervisor memanggil dokter jaga | Alasan pemanggilan | Dokter jaga membuat penugasannya sendiri |
| `Consultant` atau `OnCallDoctor` aktif | Berakhir | Konsultasi selesai, atau shift jaga selesai | Waktu selesai | Menghapus barisnya |
| Peran apa pun | Peran lain pada baris yang sama | — | — | **Dilarang.** Peran tidak dapat diubah pada baris yang sudah ada; tutup baris lama, buka baris baru |

**Kenapa peran tidak boleh diubah di tempat.** Mengubah `Consultant` menjadi `Dpjp` pada baris yang
sama akan menulis ulang sejarah: dokumen yang ditulis semasa ia konsulen mendadak terbaca seakan
ditulis DPJP. Penugasan adalah catatan berperiode, dan catatan berperiode dikoreksi dengan menutup
lalu membuka, bukan dengan menimpa.

### 6A.4 Pengaruh penutupan episode

| Keadaan episode | Yang terjadi pada penugasan |
| --- | --- |
| `DischargePending` | Seluruh penugasan **tetap aktif**. Dokumen susulan masih mungkin dibuat |
| `Closed` | Seluruh penugasan yang masih terbuka ditutup pada waktu penutupan |
| `Cancelled` | Sama seperti `Closed` |
| Episode dibuka kembali lewat sesi koreksi | Penugasan **tidak** ikut dibuka kembali. Sesi koreksi tidak menerima catatan klinis baru sesuai `FR-MVP-EP-022` |

---
## 7. Traceability

| Bagian | Requirement dan decision asal |
| --- | --- |
| Episode | `RWI-RULE-003`, `RWI-RULE-004`, `RWI-RULE-010`, `RWI-RULE-011`, `RWI-RULE-022`, `RWI-DEC-009` |
| Pemesanan | `RWI-RULE-001`, `RWI-RULE-002`, `RWI-RULE-015`, `RWI-DEC-007`, `RWI-DEC-008` |
| Penempatan | `RWI-RULE-008`, `RWI-RULE-027`, `RWI-DEC-014`, `RWI-DEC-039` |
| Kelayakan keuangan | `RWI-RULE-009`, `RWI-RULE-028`, `RWI-DEC-015`, `RWI-DEC-040` |
| Resume pulang | `RWI-RULE-032`, `RWI-DEC-045` |
| Sesi koreksi | `RWI-RULE-020`, `RWI-DEC-028`, arsitektur domain bagian G.4 |
| Kepergian fisik pasien | `RWI-RULE-036`, `RWI-DEC-055` |
| Kehadiran pasien dan satu episode aktif | `RWI-RULE-035`, `RWI-DEC-054` |
| Versi resume pulang | `RWI-DEC-057` |
| Kewenangan DPJP | `RWI-RULE-030`, `RWI-DEC-042` |

---

## 8. Perubahan pada `contract_version` `0.9.0` — amandemen terbatas ★ 15 September 2026

**Status `draft`.** Dua perubahan: tujuan penugasan pada bagian 6A, dan akibat penutupan episode terhadap mesin milik
modul lain.

### 8.1 Penugasan dokter — tambahan pada 6A.3

| Dari | Ke | Pemicu | Yang wajib ada | Yang dilarang |
| --- | --- | --- | --- | --- |
| Tidak ada | `Consultant` aktif, tujuan `Regular` | Kepala ruangan atau supervisor lewat jalur tulis baru | Alasan, waktu mulai; waktu selesai opsional | Dokter membuat penugasannya sendiri |
| Tidak ada | `OnCallDoctor` aktif, tujuan `Regular` | Sama | Alasan, waktu mulai; waktu selesai opsional | Sama |
| Tidak ada | `OnCallDoctor` aktif, tujuan **`LateDocumentation`** | Sama | **Alasan, waktu mulai tidak lampau, waktu selesai** — `VAL-INP-01`, `08` | Peran selain dokter jaga; tanpa waktu selesai; menggusur DPJP |
| `Consultant`/`OnCallDoctor` aktif | Berakhir | `PATCH …/end` atau waktu selesai lewat | Waktu selesai | Mengubah tujuan atau peran pada baris yang sama |
| `LateDocumentation` aktif | Berakhir | `EndDateTime` lewat — dibaca saat query, **tanpa** proses latar | — | Memperpanjang dengan menyunting baris; buat penugasan singkat baru |

**6A.4 tetap:** penutupan episode menutup seluruh penugasan terbuka, termasuk `LateDocumentation`.

Contoh jalur gagal `RWI-DEC-130` (c): penugasan singkat dr. Rina berakhir 11.00; ia menyimpan 11.05 → `403` dari
penjaga penulis klinis, karena pukul 11.05 tidak ada penugasan aktif.

### 8.2 Akibat `DischargePending` → `Closed` pada mesin modul lain

Perpindahan status episode **tidak berubah**. Yang bertambah adalah akibatnya di dalam transaksi yang sama.

| Mesin | Pemilik | Dari | Ke | Yang tidak disentuh |
| --- | --- | --- | --- | --- |
| Keutuhan dokumen klinis | `MedicalRecordManagement` | `Draft` | `LockedUnsigned`, pemicu `EncounterClosed` | `Signed`, `Cancelled` |
| Pesanan tindakan | `ClinicalManagement` | `Planned`, `Ordered` belum dilaksanakan dan belum ditagih | `Cancelled` "episode ditutup sebelum dilaksanakan" | `InProgress`, `Completed`, pesanan tertagih |
| Dosis obat | `PharmacyManagement` | `Due` berjadwal setelah waktu tutup | `Cancelled` "perawatan ditutup" | `Due` sebelum waktu tutup, dosis yang sudah dicatat |

| Dari | Ke | Kenapa dilarang |
| --- | --- | --- |
| `Closed` | Membuka kembali lewat sesi koreksi mengembalikan konsep terkunci menjadi `Draft` | `RWI-AC-201`, `RM-DEC-003` |
| `Closed` | Membuka kembali mengembalikan pesanan atau dosis yang dibatalkan | Pembatalan tercatat sebagai akibat penutupan; pesanan baru dibuat bila perlu |
| `DischargePending` | `Closed` dengan sebagian langkah akibat tersimpan | `INV-INP-11` |

---

## 9. Perubahan pada `contract_version` `0.10.0` — Finishing Rawat Inap ★ 1 Oktober 2026

| Field | Nilai |
|---|---|
| `last_changed_in` | `0.10.0` |
| Status | **`approved`** — Muhammad Hamzah, 2026-10-02 (`RWI-DEC-221`) |
| Dampak kompatibilitas | Status `Episode` dan `BedPlacement` **tidak berubah**. Kasus OK bertambah satu status akhir; empat lifecycle baru |
| Traceability | `RWI-DEC-173`, `176`, `177`, `182`, `199`, `201`, `204`; `INV-RWF-25` s.d. `33` |

### 9.1 Kasus OK (`OprCase`) — tambahan `Rejected`

Lifecycle OK lain tetap seperti modul OK. Baris di bawah hanya yang **baru atau berubah**.

| Dari | Aksi | Ke | Pelaku | Syarat | Efek samping |
|---|---|---|---|---|---|
| — | Pesan dari bangsal | `Requested` | Perawat atau dokter bangsal | `INV-RWF-25` | — |
| `Requested` | Tolak | **`Rejected`** | Petugas penjadwalan OK (`OperatingRoomCase : Reject`) | Alasan 10–500 karakter | Pra-operasi yang ada menjadi `Superseded`; tidak ada biaya |
| `Requested` | Jadwalkan (= menyetujui) | `Scheduled` | Petugas OK | Tetap | Tetap |
| `Scheduled`, `Requested` | Tunda | `Postponed` | Petugas OK | Tetap | **Baru:** pra-operasi `Sent`/`Confirmed` → `NeedsUpdate` dalam transaksi yang sama |
| `Scheduled` | Siap | `Ready` | Petugas OK | **Baru:** ditambah `INV-RWF-26` dan `27` | Tetap |
| `InProgress` → … | Serah terima diterima, laporan final, keluar kamar pulih | `Completed` | Sistem (`OPS-DEC-025`) | Tetap | **Baru:** `OperatingRoomCompletionEffects` sesudah commit |

**Transisi terlarang tambahan**

| Dari | Aksi | Hasil |
|---|---|---|
| `Rejected` | Apa pun (ubah, jadwalkan, tunda, batal, mulai, tolak lagi) | `422` `OPR-CASE-REJ-002` |
| `Scheduled`, `Ready`, `Postponed`, `InProgress`, `Completed`, `Cancelled` | Tolak | `422` `OPR-CASE-REJ-001` |

### 9.2 Catatan Pra-Operasi (`OprWardPreOpNote`) — per versi

| Dari | Aksi | Ke | Pelaku | Syarat |
|---|---|---|---|---|
| — | Simpan draf pertama | `Draft` | Perawat bangsal (`Send`) | Kasus `Requested`, `Scheduled`, atau `Postponed` |
| `Draft` | Simpan draf | `Draft` | Pengirim | Versi sama |
| `Draft` | Kirim | `Sent` | Pengirim | Butir wajib pengirim lengkap; tanda vital tersedia; sisi penandaan cocok |
| `Sent` | Konfirmasi semua butir wajib dan penandaan | `Confirmed` | Perawat OK (`Confirm`), akun ≠ pengirim | — |
| `Sent` | Konfirmasi sebagian | `Sent` | Perawat OK | Butir yang dikonfirmasi tersimpan |
| `Sent`, `Confirmed` | Kasus ditunda | `NeedsUpdate` | Sistem | — |
| `NeedsUpdate` | Simpan draf versi baru | versi lama tetap `NeedsUpdate`; versi baru `Draft` | Pengirim | Kasus tidak `Rejected`/`Cancelled` |
| Versi baru `Sent` | — | versi lama → `Superseded` | Sistem | — |
| Apa pun selain `Superseded` | Kasus `Rejected` atau `Cancelled` | `Superseded` | Sistem | — |

Terlarang: konfirmasi oleh akun pengirim (`OPR-WPO-002`); kirim atau ubah saat kasus `InProgress`/`Completed`/`Rejected`/`Cancelled` (`OPR-WPO-004`); mengubah versi `Confirmed` tanpa penundaan.

### 9.3 Serah terima pasca operasi (`OprHandover`) — aturan baru pada transisi yang sudah ada

| Dari | Aksi | Ke | Pelaku | Syarat baru |
|---|---|---|---|---|
| — | Kirim | `Sent` | Perawat OK (`Send`) | — |
| `Sent` | Terima | `Accepted` | Perawat unit tujuan (`Receive`) | Akun ≠ pengirim; pasien menempati bed aktif di `DestinationUnitId` (`INV-RWF-28`) |
| `Sent` | Tolak beralasan | `Rejected` | Perawat unit tujuan (`Receive`) | Akun ≠ pengirim |
| `Rejected` | Kirim ulang | serah terima baru `Sent` | Perawat OK | Tetap |

Menerima **tidak pernah** memindahkan bed (`RWI-DEC-177` butir 6).

### 9.4 Permintaan admisi (`InpAdmissionReferral`)

| Dari | Aksi | Ke | Pelaku | Syarat | Efek |
|---|---|---|---|---|---|
| — | Simpan keputusan kamar pulih `Inpatient`/`Icu` | `Pending` | Sistem (dipanggil OK) | Pasien tanpa episode hadir; belum ada `Pending` untuk kasus atau pasien itu | — |
| — | Sama, pasien sudah punya episode hadir | (tidak dibuat) | Sistem | — | OK menerima `AdmissionReferralState = NotNeeded`; alur serah terima biasa |
| `Pending` | Keputusan kamar pulih berubah dari `Inpatient`/`Icu`, atau OK membatalkan beralasan | `Cancelled` | Sistem (dipanggil OK) | Alasan wajib | Hilang dari daftar |
| `Pending` | Admisi berlangkah selesai dengan `AdmissionReferralId` | `Completed` | Petugas admisi | Pasien sama | `CompletedEpisodeId` terisi dalam transaksi episode |

Terlarang: `Completed` atau `Cancelled` → status apa pun (`INP-ADM-REF-002`).

### 9.5 Serah terima transfer (`CliTransferHandover`, `P2`)

| Dari | Aksi | Ke | Pelaku | Syarat |
|---|---|---|---|---|
| — | Transfer antarunit tersimpan | `NotSent` | Sistem | Unit asal ≠ unit tujuan |
| `NotSent`, `Rejected` | Kirim | `Sent` | Perawat unit asal (`TransferHandover : Send`) | Potret klinis dibekukan |
| `Sent` | Terima | `Accepted` | Perawat unit tujuan (`: Receive`) | Akun ≠ pengirim; pasien di unit tujuan |
| `Sent` | Tolak beralasan | `Rejected` | Perawat unit tujuan | Alasan wajib |

Status apa pun **tidak** menahan transfer, keluar ruangan, atau penutupan episode (`INV-RWF-33`). Selama bukan `Accepted`, kedua unit melihat "Serah terima tertunda".

---

## 10. Perubahan pada `contract_version` `0.11.0` — Workspace PPRI ★ 7 Oktober 2026

| Field | Nilai |
|---|---|
| `last_changed_in` | `0.11.0` |
| Status | **`approved`** — Muhammad Hamzah, 2026-10-08 (`RWI-DEC-265`) |
| Dampak kompatibilitas | Status `Episode` dan `BedPlacement` **tidak berubah**. Satu lifecycle baru (dokumen admisi), satu penanda (rencana tindakan, di luar gelombang). Log cetak tidak punya status |
| Traceability | `RWI-DEC-240` (siklus), `RWI-DEC-239`, `255`, `263`; `FR-RWA-120` s.d. `125`; `INV-RWA-01` s.d. `11` |

Nama status di bawah sama persis dengan `flowcharts/05-workspace-ppri-siklus-dokumen.md`.

### 10.1 Dokumen admisi (`InpAdmissionDocument`) — per versi

Berlaku untuk Serah Terima Pasien Baru, Permintaan Privasi, Nilai Kepercayaan, Selisih Biaya, Pelunasan Deposit, dan Estimasi Biaya. General Consent, Gelang, Label, dan IPD **tidak** punya status dokumen (`RWI-DEC-233`, `240`).

| Dari | Aksi | Ke | Pelaku | Syarat | Efek |
|---|---|---|---|---|---|
| — | Simpan konsep | `Draft` | Petugas admisi (`Create`) | Episode `Admitted`/`DischargePending`; tidak ada dokumen aktif sejenis; syarat jenis (Selisih Biaya: penjamin asuransi/perusahaan; Pelunasan Deposit: kekurangan > 0 dan Billing terbaca) | Butir serah terima dibekukan namanya; kota dari pengaturan |
| `Draft` | Ubah | `Draft` | `Update` | `RowVersion` cocok | — |
| `Draft` | Kunci | `AwaitingSignature` | `Update` | Isian wajib jenis itu lengkap (`VAL-RWA-20` s.d. `27`); sumber salinan beku terbaca | Salinan beku dan angka beku dibentuk (`RWI-DEC-263`) |
| `AwaitingSignature` tanpa tanda tangan | Buka kunci | `Draft` | `Update` | Nol tanda tangan (`RWI-DEC-240` butir 2) | Salinan beku dan angka beku dikosongkan |
| `AwaitingSignature` | Tanda tangan satu slot, bukan yang terakhir | `AwaitingSignature` | Sesuai slot: `Sign`, `SignAsCro`, `SignAsNurse`, `SignAsHeadNurse` | Slot wajib untuk jenis itu dan masih kosong; akun belum mengisi slot petugas lain (`INV-RWA-04`); slot Perawat: pasien menempati bed (`INV-RWA-11`) | Baris tanda tangan |
| `AwaitingSignature` | Tanda tangan slot wajib terakhir | **`Completed`** | Sama | Sama | `CompletedAt`; kelengkapan naik; saran serah terima yang bergantung padanya muncul |
| `Completed` | Buat versi koreksi | versi lama **`Superseded`**, versi baru **`Draft`** | `Update` | Alasan 10–500 karakter | Isi disalin tanpa tanda tangan dan tanpa salinan beku; `VersionNo` + 1 |
| `Draft` | Buang konsep | **`Cancelled`** | Pembuat konsep (`Update`) | Alasan 1–500 karakter | — |
| `Draft`, `AwaitingSignature`, `Completed` | Batalkan | **`Cancelled`** | Supervisor admisi (`Cancel`) | Alasan 10–500 karakter | Dokumen tidak lagi dihitung; dokumen baru sejenis boleh dibuat (rantai versi baru mulai dari 1) |
| Apa pun | Episode `Closed` atau `Cancelled` | Tidak berubah | Sistem | — | Seluruh dokumen hanya-baca; setiap cetak beralasan; cetakan episode `Cancelled` bertanda "ADMISI DIBATALKAN" |

**Contoh.** Serah Terima Tn. Budi dibuat Sari 09.58 (`Draft`), dikunci 10.05 (`AwaitingSignature`), slot Admission ditandatangani Sari 10.05, slot CRO oleh Dewi 10.20, dan slot Perawat oleh Andi 10.40 setelah Budi menempati bed pukul 10.35. Pukul 10.40 status menjadi `Completed`.

**Transisi terlarang**

| Dari | Aksi | Hasil |
|---|---|---|
| `Completed` | Ubah langsung atau kembali ke `Draft` | `409` `INP-ADM-DOC-005` — harus lewat versi koreksi |
| `AwaitingSignature` | Ubah isi | `409` `INP-ADM-DOC-005` — buka kunci dulu bila belum ada tanda tangan |
| `AwaitingSignature` dengan tanda tangan | Buka kunci | `409` `INP-ADM-DOC-006` |
| `Draft` | Tanda tangan slot apa pun | `409` `INP-ADM-DOC-030` |
| `Superseded`, `Cancelled` | Aksi apa pun selain baca dan cetak | `409` `INP-ADM-DOC-005` |
| `Draft`, `AwaitingSignature`, `Superseded`, `Cancelled` | Buat versi koreksi | `409` `INP-ADM-DOC-005` |
| `AwaitingSignature`, `Completed` | Buang konsep | `409` `INP-ADM-DOC-005` — gunakan Batalkan |
| Slot yang sudah terisi | Tanda tangan lagi | `409` `INP-ADM-DOC-032` |
| Episode selain `Admitted`/`DischargePending` | Penulisan apa pun | `409` `INP-ADM-DOC-001` atau `002` |

### 10.2 Lencana menu dan kelengkapan — diturunkan, tidak disimpan

| Keadaan | Lencana | Dihitung lengkap |
|---|---|---|
| Ada dokumen `Completed` aktif | Lengkap | Ya |
| Dokumen aktif `AwaitingSignature` | Menunggu tanda tangan | Tidak |
| Dokumen aktif `Draft` | Konsep | Tidak |
| Tidak ada dokumen aktif, dokumen wajib | Belum dibuat | Tidak |
| Dokumen tidak wajib untuk pasien ini | Tidak diperlukan | Tidak dihitung |
| Gelang atau IPD sudah dicetak sekali | Sudah dicetak | Ya |
| General Consent | Cetak saja | Tidak dihitung (`RWI-DEC-233`) |
| Sumber aturan gagal dibaca | Tidak dapat dihitung ("?") | Tidak dihitung pada pembilang maupun penyebut |

Versi `Superseded` dan dokumen `Cancelled` tidak pernah membuat lencana Lengkap.

### 10.3 Log cetak (`InpAdmissionPrintLog`)

Tidak punya status. Setiap baris adalah satu kejadian dan **tidak pernah diubah**. "Cetakan ke-*n*" dan penanda cetak ulang diturunkan dari urutan baris (`data-dictionary.md` 20.13.1).

### 10.4 Penanda rencana tindakan (`InpAdmissionProcedurePlanMark`) — di luar gelombang

| Dari | Aksi | Ke | Pelaku | Syarat |
|---|---|---|---|---|
| Tidak ada penanda aktif | Pasang | Penanda aktif | `InpatientAdmissionDocument : Update` | Episode `Admitted`/`DischargePending` |
| Penanda aktif | Cabut | Penanda tidak aktif (`UnmarkedAt` terisi) | Sama | Sama |
| Penanda aktif | Pasang lagi | Tetap satu penanda aktif | — | `200`, tidak membuat baris kedua |

### 10.5 Butir master serah terima (`MstInpatientClearanceItem`)

Status aktif/nonaktif yang sudah ada tetap. Aturan baru: `ChecklistType` **tidak dapat diubah** setelah butir dipakai dokumen serah terima atau penandaan penutupan (`422` `MST-ICI-004`). Menonaktifkan butir tidak mengubah dokumen lama, karena nama dan kodenya sudah beku di `InpAdmissionHandoverItem`.

### 10.6 Pengaruh status episode pada Workspace PPRI

| Status episode | Workspace PPRI |
|---|---|
| `Draft` | Tidak terbuka: "Admisi belum dikonfirmasi" (G-30). Kop surat tetap terbaca untuk langkah 8 alur admisi |
| `Admitted`, `DischargePending` | Terbuka penuh menurut hak |
| `Closed`, `Cancelled` | Hanya-baca; cetak dan cetak ulang beralasan (`RWI-DEC-240` butir 7) |

## 11. Amandemen Bed Management — 10 Oktober 2026

**Status: draft — Amandemen Bed Management, 10 Oktober 2026.** Set kontrak mengikuti `blueprint-manifest.md`; `last_changed_in: 0.12.0`. Owner produk/domain/API: Muhammad Hamzah (RWI-DEC-061); frontend: pengembang dalam batas RWI-DEC-292; keamanan/privasi: OPEN. `approved_by: null`, `approved_at: null` untuk amandemen ini.

Masukan: decision log revision **46**, RWI-DEC-274–294 dan RWI-AC-396–426; gate revision **1.12**, **BM-RCG-20261010-01**, enam BM-CG siap untuk desain produk terbatas. `DOMAIN_ARCHITECTURE_NOT_RUN` untuk slice ini: ownership existing sudah diketahui dan gate mengizinkan desain langsung. Arsitektur domain lama bagi scope lain tetap berlaku. As-is bersumber audit **BM-AUD-20261010-01** revision 1 (section 7 untuk Swagger), bukan bukti runtime.

Snapshot BE `d4e1eca06fb28c05934c68c1e51a4dca01935a10`, FE `969acfcc04cdf31074a1911e9827c31d25ddadd0`. Semua nama class/field/API baru di bawah adalah **target Rencana (belum tersedia)**. Bila bagian lama bertentangan mengenai bed kembali Available, amandemen ini mengikuti RWI-DEC-281/282. Persetujuan produk bukan persetujuan desain atau SOP. Hash masukan terpusat pada manifest.

### 11.1 Enam status publik dan priority truth

| Urutan | Code / label BA | Kondisi server | Pesan/admission |
| --- | --- | --- | --- |
| 1 | Occupied / Terisi | Ada placement aktif authoritative, meski raw/admin berbeda | Tidak bookable; tampilkan conflict flag bila mismatch |
| 2 | Reserved / Dipesan | Tidak ada placement, ada reservation Active belum expired | Tidak bookable untuk pihak lain; expiry hanya server |
| 3 | Unavailable / Tidak Tersedia | Tanpa holder dan master closed/inactive/nonreservable/invalid/Unknown, root missing/Unverified | Tidak bookable; alasan operasional yang aman |
| 4 | WaitingCleaning / Menunggu Pembersihan | Kosong, tidak closed, root WaitingCleaning | Tidak bookable |
| 5 | Cleaning / Dalam Pembersihan | Root Cleaning atau AwaitingVerification; tanpa closure/holder | CleaningPhase=InProgress atau AwaitingVerification; bukan status ketujuh |
| 6 | Available / Tersedia | Root Ready + master valid/aktif/reservable + tanpa holder/closure | Bookable; eligibility tiap pasien tetap diuji |

Priority holder mencegah pasien disembunyikan menjadi bed kosong. Dual-holder berbeda episode atau raw/admin mismatch → ConflictCode dan aksi pemesanan/transfer tujuan ditutup. Raw Occupied/Reserved tanpa holder juga invalid, tidak berubah Available sendiri. Counts menggunakan hasil proyeksi yang sama dan saling eksklusif; total = penjumlahan enam status. Dalam Pembersihan/Menunggu verifikasi bukan available.

Root.Unverified saat bed ditempati adalah invalidasi bukti kesiapan lama; status publik tetap Occupied. Reservation tidak mengotori bed dan root Ready tetap dipertahankan sampai used/closure. Closure administratif tidak boleh sengaja dibuat saat occupied/reserved; jika konflik data legacy muncul, holder tetap terlihat dan perlu rekonsiliasi sah.

### 11.2 State transition sah dan atomic effects

| Sumber | Pemicu | Root/holder sesudah | Guard/actor | Status publik | Audit | Trace |
| --- | --- | --- | --- | --- | --- | --- |
| Ready; kosong, valid, tanpa closure | Reserve | Ready + reservation Active | Admisi/Create; predicate current; key/version | Reserved | Reserve | AC-408/413/419 |
| Ready + reservation Active | Cancel/expiry unused | Ready, reservation Cancelled/Expired | Reason wajib manual; expiry server; recheck closure/holder | Available hanya jika semua syarat tetap sah | Cancel/Expire | AC-408/413/418 |
| Ready; kosong/reservasi episode sendiri | Place | Unverified + placement active; verified fields dibersihkan | Eligibility/episode guard existing; consume reservation; snapshot | Occupied | Place | AC-398/419 |
| Occupied, source placement current | Transfer | Asal WaitingCleaning cycle baru; tujuan Unverified+occupied | Lock kedua bed; category match; DPJP/folio/reason existing; snapshot | Asal WaitingCleaning atau Unavailable; tujuan Occupied | TransferOut/TransferIn | AC-399–405/407/414 |
| Occupied | Departure fisik sah | WaitingCleaning cycle baru; end placement | Placement milik episode masih current; kasir bukan gate departure | WaitingCleaning atau Unavailable bila closure aktual | Release | AC-407/426 |
| WaitingCleaning | Start cleaning | Cleaning + attempt Started | HK individu, unit scope, SOP, CycleId/version | Cleaning / InProgress | StartCleaning | AC-406 |
| Cleaning / Started | Complete physical work | AwaitingVerification; attempt state2 | HK individu berwenang pada unit; cycle/version current | Cleaning / AwaitingVerification | CompleteCleaning | AC-409/410 |
| AwaitingVerification | Verify IsReady=true | Ready; attempt Accepted; verified actor/time/reference | Perawat verifier sah; empty, valid, no closure; current attempt | Available | VerifyReady | AC-409 |
| Unverified; tidak terbukti used-dirty | Initial/reopen verification true | Ready; tanpa fake cleaning attempt | Perawat verifier + SOP evidence; seluruh syarat Ready | Available | VerifyReady | AC-412/425 |
| AwaitingVerification atau Unverified | Verify IsReady=false | WaitingCleaning; attempt Rejected bila ada | Reason wajib; bukti/actor/time; riwayat tetap | WaitingCleaning | RejectReadiness | AC-411 |
| Kosong tanpa reservation aktif | Administrative close/inactive/nonreservable | Invalidasi verified fields/cycle; dirty tetap Waiting; attempt aktif Interrupted | Bed Update; alasan; guard seluruh write path | Unavailable | Close | AC-412/424 |
| Closed/Inactive; kosong | Reopen / activate / reservable | Unverified jika tidak dirty, WaitingCleaning jika dirty; tidak Ready | Bed Update; alasan; no holder; master valid | Unavailable atau WaitingCleaning sampai readiness sah | Reopen | AC-412 |
| Ended old episode placement | Close old episode | Tidak memutasi bed/root/holder baru | Periksa ownership/placement identity, bukan hanya BedId | Status pasien/siklus baru tetap | Episode closure existing, tanpa release palsu | AC-426 |
| Historical placement | Authorized correction | Versi baru/chain existing; state fisik tak berubah kecuali koreksi aktif sah | Correct + Billing OPEN + expected version + all impacted bed locks | History revised; tidak membuat transfer/cleaning palsu | Correct | AC-415/418 |

### 11.3 Transisi terlarang dan exception

| Percobaan | Hasil wajib |
| --- | --- |
| HK finish → Available tanpa verifier | 422 BED_NOT_READY; finish sah hanya AwaitingVerification |
| Verify dengan cycle/attempt/versi lama; setelah closure/new release | 409 CLEANING_CYCLE_STALE/STALE_BED_VERSION; tidak membuka siklus baru |
| Start/complete/verify pada occupied/reserved atau closed | 409/422; holder dan closure tetap |
| Close/disable/nonreservable/move location bed yang occupied/reserved lewat create/PUT/status/availability/delete | 409 BED_HOLD_CONFLICT, seluruh writer satu guard |
| Set raw Available untuk mengabaikan readiness; set raw Occupied/Reserved/Cleaning manual | 422; reopen tidak Ready; holder/status workflow hanya melalui domain operasi |
| Transfer destination stale/ineligible/reserved orang lain/class Unknown/manual mismatch | 409 atau 422 sesuai API; source/destination/history tetap utuh |
| Callback/handover gagal setelah transfer commit | Status commit tetap; event retry/outbox existing, tidak automatic reverse |
| Cancel/expire unused reservation lalu create cleaning baru | Dilarang; hanya used release membuat kebutuhan cleaning |
| Cancel committed transfer atau hard delete history | Tidak ada operasi; gunakan correction sah existing |
| Browser countdown habis lalu local marking Available | Dilarang; fetch server, no stale overwrite |

### 11.4 Proof gates dan waktu

BM-G01/02/03 tetap bukti aktivasi, bukan state/status produk baru. Eventtime dan recordedtime dipisah, koreksi versioned. Existing 120 menit lazy server expiry dipertahankan, tanpa worker/reminder/perpanjangan baru. Waktu cleanup/verification ditetapkan server saat aksi, tidak timer auto-ready. SOP waktu kejadian rekonsiliasi tidak dikarang.
