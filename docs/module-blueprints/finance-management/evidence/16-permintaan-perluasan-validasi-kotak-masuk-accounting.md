# Permintaan Perluasan Validasi Kotak Masuk Accounting — Kejadian Penanda Bernilai Nol

| Field | Nilai |
|---|---|
| Dari | Yasmin, owner / penggarap modul Finance (AR/AP) |
| Untuk | Rizki, owner modul Accounting |
| Tanggal | 28 September 2026 |
| Sifat | **Permintaan perubahan, bukan pertanyaan.** Sengaja dipisah dari `evidence/15` karena yang diminta bukan ratifikasi nama kode, melainkan perubahan aturan validasi di kotak masuk Anda |
| Menyangkut | Dua kode penanda yang diusulkan `evidence/15` bagian 5: `PENUTUPAN-SHIFT-KASIR` dan `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR` |
| Dasar keputusan | `FIN-DEC-075` (`approved` 28 September 2026); arsitekturnya `FIN-DES-059` |
| Dicatat sebagai | `FIN-OQ-035` pada decision log Finance |

---

## 1. Ringkasan satu paragraf

Anda meminta Finance menerbitkan penanda shift tertutup supaya `ACC-DEC-065` dapat ditegakkan
(`evidence/14` bagian 4.1 dan 7.4). Kami menyanggupinya, dan bentuk yang benar untuk penanda itu
adalah kejadian **bernilai nol** — ia menyatakan status, tidak memindahkan uang, dan tidak punya
lawan jurnal. Masalahnya: kotak masuk Anda sebagaimana dibangun **menolak nilai nol lewat kedua
jalur yang tersedia**. Kami sudah memeriksanya langsung ke source, bukan menduga. Karena itu kami
meminta satu dari dua perubahan di bagian 4, dan menjelaskan di bagian 5 mengapa kami **tidak**
mengambil jalan pintas yang tersedia.

---

## 2. Apa yang kami temukan di kotak masuk Anda

Kami membaca `AccAccountingEventService.cs` untuk menyiapkan worker pengiriman kami. Dua aturan
berikut bersama-sama menutup seluruh jalan bagi kejadian bernilai nol:

| # | Aturan | Perilaku |
|---:|---|---|
| 1 | Pesan **bukan** pesan saldo dan `Amount <= 0` | Ditolak **`400`** — *"Nilai kejadian harus lebih besar dari nol."* |
| 2 | Jenis kejadian terdaftar sebagai **pesan saldo subledger** | Ditolak **`409`** — *"Pesan saldo subledger belum dapat diterima. Jalurnya dibangun pada `BE-ACC-P2-028`."* |

Aturan 1 menutup jalur kejadian biasa. Aturan 2 menutup jalur yang justru dirancang untuk nilai nol
dan negatif, karena jalur itu belum dibangun. **Tidak ada celah yang lolos.**

Ini bukan keluhan — aturan 1 memang aturan yang benar, dan kami sendiri memasang aturan serupa di
sisi kami. Yang kami minta hanyalah satu pengecualian yang sempit dan bernama.

---

## 3. Kenapa penanda itu harus bernilai nol

Penanda shift tertutup **bukan transaksi**. Ia tidak memindahkan uang, tidak menyentuh akun mana
pun, dan tidak menghasilkan jurnal. Fungsinya satu: memberi tahu Anda bahwa sebuah shift sudah
mencapai keadaan tertutup final, sehingga Anda boleh memasukkan periodenya ke tutup bulan.

Memberinya nilai selain nol berarti menuliskan angka yang tidak punya arti akuntansi ke dalam aliran
kejadian yang seluruh isinya bermakna. Siapa pun yang membaca laporan kejadian tanpa mengetahui
konteks kode ini akan menganggap angka itu nyata.

Nilai nol juga bukan hal baru bagi kontrak kita: `ACC-XMOD-0.3` sudah mengizinkan pesan saldo
bernilai nol atau negatif, dan Anda sendiri menegaskannya di `evidence/14` bagian 5 butir 2. Yang
kami minta adalah memperlakukan penanda ini **sekelas pesan saldo** — bukan membuat kelonggaran baru
yang belum ada padanannya.

---

## 4. Dua pilihan yang kami ajukan

Keduanya menyelesaikan masalah yang sama. Kami tidak punya preferensi teknis; yang kami butuhkan
adalah kepastian jalur mana yang akan tersedia.

### Pilihan A — Izinkan `Amount = 0` untuk daftar kode penanda yang tertutup

Kotak masuk menerima `Amount = 0` **hanya** untuk kode yang Anda daftarkan sebagai penanda, dan
menolaknya untuk semua kode lain persis seperti sekarang.

| Hal | Isi |
|---|---|
| Cakupan | Dua kode: `PENUTUPAN-SHIFT-KASIR`, `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR` |
| Bentuk daftarnya | Sebaiknya **daftar tertutup yang bernama**, bukan pemeriksaan longgar "kalau nilainya nol maka lolos" — kalau longgar, setiap kejadian transaksi yang kebetulan bernilai nol akan ikut lolos tanpa ada yang memergokinya |
| Yang perlu Anda putuskan | Apakah kedua kode ini terdaftar sebagai jenis kejadian yang **tidak punya aturan posting** sama sekali, sehingga tidak tertahan `POSTING_RULE_MISSING` |
| Keuntungannya | Tidak bergantung pada `BE-ACC-P2-028`; dapat berjalan lebih cepat |

### Pilihan B — Aktifkan jalur pesan saldo (`BE-ACC-P2-028`), lalu daftarkan kedua kode di jalur itu

| Hal | Isi |
|---|---|
| Cakupan | Kedua kode diperlakukan sebagai pesan saldo, memakai jalur yang memang dirancang untuk nilai nol/negatif |
| Keuntungannya | Tidak menambah pengecualian baru; memakai mekanisme yang sudah Anda rancang |
| Konsekuensinya | Bergantung pada selesainya `BE-ACC-P2-028`, yang jadwalnya ada di tangan Anda, bukan kami |

---

## 5. Jalan pintas yang tersedia, dan kenapa kami tidak mengambilnya

Kami dapat membuat penanda ini lolos hari ini tanpa meminta apa pun kepada Anda: cukup memberinya
nilai kecil non-nol, misalnya Rp 1. Pilihan itu **ditolak owner Finance** (`FIN-DEC-075`), dengan
alasan yang kami anggap layak Anda ketahui:

| Yang terjadi bila kami pakai nilai simbolis | Akibatnya bagi Anda |
|---|---|
| Setiap shift tertutup mengirim kejadian Rp 1 | Aliran kejadian Anda terisi ribuan baris bernilai Rp 1 per bulan yang tidak berarti apa pun |
| Angka itu masuk laporan kejadian | Siapa pun yang membacanya tanpa konteks akan menganggapnya transaksi nyata |
| Bila kelak ada aturan posting yang keliru menyambarnya | Rp 1 itu **terjurnal**, dan mencari sebab selisih Rp 1 di buku besar jauh lebih mahal daripada memperbaiki satu aturan validasi sekarang |

Kami memilih menunggu jawaban Anda daripada menitipkan angka palsu ke buku besar Anda.

---

## 6. Yang kami lakukan selama menunggu

Kami **tidak** menghentikan pembangunan. Pola yang kami pakai sama dengan yang sudah berjalan untuk
`PPN-MASUKAN-PEMBELIAN`:

| Hal | Keadaan selama menunggu |
|---|---|
| Penulisan baris kotak keluar | **Berjalan** sejak shift mencapai keadaan tertutup final. Baris ditulis berstatus belum terkirim |
| Worker pengiriman kedua kode | **Tidak diaktifkan.** Anda tidak akan menerima kejadian bernilai nol sebelum menjawab surat ini |
| Bila jawabannya turun | Seluruh baris yang menumpuk terkirim berurutan. Baris itu justru menjadi catatan lengkap shift mana saja yang sudah tertutup sejak fitur dibangun |
| Bila kedua pilihan ditolak | Bentuk penandanya kami rancang ulang lewat amendment tersendiri. Kami **tidak** menyiapkan jalur cadangan sekarang, karena cadangan yang dipilih sepihak justru mengunci bentuk yang belum Anda setujui |

---

## 7. Yang kami butuhkan, dan apa yang tertahan karenanya

| Butuh | Menahan apa | Tidak menahan |
|---|---|---|
| Pilihan **A** atau **B** pada bagian 4, atau penolakan beserta alasannya | Aktivasi worker pengiriman `PENUTUPAN-SHIFT-KASIR` dan `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR`. Selama tertahan, **penegakan `ACC-DEC-065` tidak dapat berjalan** — Anda tidak akan pernah menerima penanda shift tertutup | Pembangunan fitur di sisi Finance, penulisan baris kotak keluar, maupun ratifikasi empat kode pada `evidence/15` yang dapat Anda jawab terpisah |

Satu catatan agar prioritasnya jelas: permintaan ini **tidak** menahan sisa pekerjaan Finance mana
pun. Yang tertahan justru aturan yang **Anda** minta ditegakkan. Kami menyampaikannya sekarang,
sebelum kodenya dibangun, supaya tidak ada yang dibongkar belakangan.

---

## 8. Rujukan

| Berkas | Isi |
|---|---|
| `docs/module-blueprints/finance-management/evidence/15-balasan-finance-atas-ratifikasi-accounting.md` | Surat utama — jawaban tujuh pertanyaan, nama final, dan empat kode baru termasuk kedua penanda |
| `docs/module-blueprints/finance-management/01-existing-capability-map.md` bagian 15.3 | Kontrak as-is kotak masuk Anda sebagaimana kami baca, beserta dua belas aturan penolakannya |
| `docs/module-blueprints/finance-management/02-backend-architecture.md` `FIN-DES-059` | Gerbang worker yang kami pasang selama menunggu jawaban surat ini |
