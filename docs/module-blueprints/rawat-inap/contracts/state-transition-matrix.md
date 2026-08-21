# State Transition Matrix — Modul Rawat Inap

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| `contract_version` | `0.1.0` |
| Status | `draft` |
| Owner | Product/Domain Owner sementara sesuai `RWI-DEC-006` |
| `input_revision` | `00-interview-decisions.md` revision `2`; `evidence/03-hospital-domain-architecture.md` revision `0.1` |
| Dampak kompatibilitas | Seluruhnya baru. Tidak ada state machine existing yang berubah |

Dokumen ini memuat perpindahan yang **sah** dan perpindahan yang **tidak sah**. Keduanya sama
pentingnya: yang tidak sah adalah yang paling sering dicoba petugas ketika sedang terburu-buru.

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
| — | Pasien menempati | `Aktif` | Penempatan atau perpindahan | `MstBed.BedStatus` menjadi `Occupied` |
| `Aktif` | Pasien pindah | `Berakhir`, `EndReason = Transfer` | Perpindahan | Penempatan baru dibuka; tempat tidur lama menjadi `Available` |
| `Aktif` | Episode ditutup | `Berakhir`, `EndReason = EpisodeClosed` | Penutupan | Tempat tidur menjadi `Available` |
| `Aktif` | Admisi dibatalkan | `Berakhir`, `EndReason = AdmissionCancelled` | Pembatalan | Tempat tidur menjadi `Available` |

**Tidak ada satu pun jalur** yang menutup penempatan tanpa menutup atau memindahkan episodenya.
Inilah bentuk nyata `INV-INP-01` dan `INV-INP-07`.

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
| Tertandatangani | Ubah isi setelah episode `Closed` | Tertandatangani | Supervisor | **Hanya** bila ada sesi koreksi terbuka |

### 5.1 Perpindahan yang **tidak sah**

| Tindakan yang dicoba | Kenapa ditolak | Kode |
| --- | --- | ---: |
| Dokter yang bukan DPJP aktif menandatangani | `RWI-RULE-032` aturan 4 | 403 |
| Membuat resume kedua untuk episode yang sama | `INV-INP-05` | 409 |
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

## 7. Traceability

| Bagian | Requirement dan decision asal |
| --- | --- |
| Episode | `RWI-RULE-003`, `RWI-RULE-004`, `RWI-RULE-010`, `RWI-RULE-011`, `RWI-RULE-022`, `RWI-DEC-009` |
| Pemesanan | `RWI-RULE-001`, `RWI-RULE-002`, `RWI-RULE-015`, `RWI-DEC-007`, `RWI-DEC-008` |
| Penempatan | `RWI-RULE-008`, `RWI-RULE-027`, `RWI-DEC-014`, `RWI-DEC-039` |
| Kelayakan keuangan | `RWI-RULE-009`, `RWI-RULE-028`, `RWI-DEC-015`, `RWI-DEC-040` |
| Resume pulang | `RWI-RULE-032`, `RWI-DEC-045` |
| Sesi koreksi | `RWI-RULE-020`, `RWI-DEC-028`, arsitektur domain bagian G.4 |
| Kewenangan DPJP | `RWI-RULE-030`, `RWI-DEC-042` |
