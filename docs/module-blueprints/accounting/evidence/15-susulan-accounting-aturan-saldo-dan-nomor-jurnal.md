# Susulan Accounting untuk Finance — Aturan Pesan Saldo dan Nomor Jurnal pada Tanda Terima

| Field | Nilai |
|---|---|
| Dari | Rizki, owner modul Accounting |
| Untuk | Yasmin, owner / penggarap modul Finance (AR/AP) |
| Tanggal | 30 September 2026 |
| Menyusul | [`evidence/14`](14-balasan-accounting-atas-kode-finance-05-06-07.md) (28 September 2026). Surat itu tetap berlaku seluruhnya; berkas ini hanya menambah dua hal yang diputuskan **sesudah** surat itu ditulis |
| Dasar keputusan | `docs/module-blueprints/accounting/00-interview-decisions.md` revision 15: `ACC-DEC-107` sampai `110` (28 September 2026, `GATE-DESAIN-0928`) dan `ACC-DEC-116` sampai `119` (29 September 2026, `GATE-DESAIN-0929`) |
| Kontrak yang berlaku | `ACC-XMOD-0.4` ([cross-module-contract.md](../contracts/cross-module-contract.md)), sudah ada di `QuilvianIntegrationBackend` |

---

## 1. Ringkasan

Dua hal menyusul surat kemarin. **Tidak ada bidang baru dan tidak ada penolakan baru** — pengirim
yang Anda bangun dari `ACC-XMOD-0.3` tetap cocok. Yang berubah adalah **isi** yang kami harapkan pada
pesan saldo subledger (bagian 2), dan **cara membaca** nomor jurnal pada tanda terima (bagian 3). Kami
butuh empat jawaban singkat (bagian 4); boleh digabung dengan jawaban bagian 7 `evidence/14`.

---

## 2. Empat aturan isi pesan saldo subledger — `ACC-XMOD-0.4` bagian 8a

Accounting kini membandingkan saldo subledger yang Anda kirim dengan buku besar setiap tutup bulan.
Toleransinya **nol** dan **tidak ada jalan pengecualian** (`ACC-DEC-076`, `113`): pengajuan tutup
bulan dan tutup permanen ditolak `409` selama saldonya belum lengkap atau belum cocok (`ACC-DEC-111`).
Karena itu isi pesan saldo menentukan apakah buku kami dapat ditutup.

| # | Aturan | Contoh | Akibat bila tidak dipenuhi | Dasar |
|---:|---|---|---|---|
| 1 | **Titik mulai.** Rekonsiliasi menahan tutup bulan Accounting sejak periode **saldo pertama** yang kami terima untuk badan hukum itu | Saldo pertama untuk `2026-11` → November dan sesudahnya ditahan sampai lengkap; Oktober tidak | Saldo uji yang terkirim ke badan hukum produksi menyalakan rekonsiliasi lebih awal. Mohon kirim saldo uji hanya ke lingkungan uji | `ACC-DEC-107` |
| 2 | **Setiap control account aktif yang menerima jurnal wajib dikirimi saldo** tiap periode — termasuk yang saldonya **nol** | Kas Kecil tanpa transaksi bulan itu tetap dikirim `Amount` `0.00` | Periode tertahan "belum menerima saldo" | `ACC-DEC-108` |
| 3 | **`Amount` menurut saldo normal akun** — positif saat akun berperilaku wajar, termasuk akun bersaldo normal kredit | Utang Supplier Rp 300.000.000 → `300000000.00`, **bukan** `-300000000.00` | Selisih dua kali lipat; periode tertahan | `ACC-DEC-109` |
| 4 | **`AccountingDate` = tanggal akhir periode** | Periode `2026-09` → `2026-09-30` | Pesan tetap diterima, tetapi akun itu dianggap "belum lengkap" sampai versi lebih tinggi bertanggal akhir periode dikirim | `ACC-DEC-110` |

**Akun mana saja yang dimaksud aturan 2.** Control account adalah empat kelompok `ACC-DEC-064`: Kas
Kasir, Kas Kecil, Piutang, dan Hutang. Daftar kode akun persisnya menunggu bagan akun rumah sakit yang
sah (gerbang G2); kami kirimkan kepada Anda begitu G2 selesai, supaya kedua pihak memakai daftar yang
sama.

**Bila ada selisih.** Nyatakan ulang saldo akun itu dengan `SourceVersion` yang lebih tinggi, atau
kirim kejadian yang tertinggal. Accounting tidak menyesuaikan buku besar untuk menutup selisih.

---

## 3. Nomor jurnal pada tanda terima dapat berganti — `ACC-DEC-116` sampai `119`

Jurnal yang lahir dari kejadian Anda **tidak pernah disunting** di Accounting, supaya selalu
mencerminkan catatan Finance apa adanya (`ACC-DEC-119`). Bila jurnal draft hasil kejadian ternyata
salah — biasanya karena aturan posting kami salah akun — petugas Accounting membetulkan aturannya,
**menghapus** draft itu, lalu menekan **Coba Ulang**. Draft baru terbit dengan **nomor baru**; nomor
lama tidak dipakai ulang (`ACC-DEC-116`).

**Contoh dari uji 29 September 2026.** Kejadian `EVT-UJI-035A` diterima dan dijurnal sebagai
`JU/2031/01/00004`. Draft itu dihapus, kejadian kembali `Gagal`, lalu dicoba ulang dan menjadi
`JU/2031/01/00005`. Tanda terima pertama tetap menulis `JU/2031/01/00004`.

Akibatnya bagi Finance:

1. **`AccountingJournalNumber` yang Anda simpan dari tanda terima pertama dapat usang.** Rujukan yang
   tetap adalah `AccountingEventId` — yang Anda simpan sebagai `AccountingReceiptNumber`. Mohon nomor
   jurnal diperlakukan sebagai informasi "nomor saat diterima", bukan kunci.
2. **Kiriman ulang dibalas dengan keadaan terkini.** Mengirim ulang kejadian yang sama (kunci
   anti-ganda sama) dijawab `200` beserta status dan nomor jurnal yang berlaku **saat itu**. Ini
   perilaku anti-ganda, bukan jalur baca resmi. Akun layanan Finance saat ini hanya berhak
   `AccountingEvent : Receive`; bila Finance butuh nomor jurnal terkini secara rutin, sampaikan, lalu
   kami putuskan mekanismenya.
3. **`EventStatus` dapat bernilai `Gagal` atau `Diabaikan`,** selain `Diterima`, `Terjurnal`,
   `Tertahan`, dan `Tercatat` yang tertulis di kontrak bagian 4b. Keduanya muncul pada balasan kiriman
   ulang; `Gagal` juga muncul pada balasan `201` pesan saldo yang rinciannya tidak dapat diproses. Mohon
   pengirim Anda tidak menganggapnya galat. Bagian 4b akan kami lengkapi pada amandemen kontrak
   berikutnya.
4. **Bila yang salah adalah angka dari Finance**, bukan aturan posting kami, jalannya tetap kejadian
   pembalik dari Finance. Accounting tidak mengubah nominal jurnal hasil kejadian.

---

## 4. Yang Accounting butuhkan dari Finance

| # | Butuh | Menahan |
|---:|---|---|
| 15.1 | Kesanggupan mengirim saldo untuk **setiap** control account aktif tiap periode, termasuk yang bersaldo `0.00` (aturan 2) | G4 |
| 15.2 | Kesanggupan mengirim `Amount` menurut saldo normal akun (aturan 3) | G4 |
| 15.3 | Kapan saldo akhir bulan terbit dibanding jadwal tutup bulan Accounting, dengan `AccountingDate` tanggal akhir periode (aturan 4) | Jadwal operasional |
| 15.4 | Konfirmasi bahwa pengirim Finance memakai `AccountingEventId` sebagai rujukan tetap dan menerima `EventStatus` `Gagal`/`Diabaikan`; sampaikan bila Finance butuh nomor jurnal terkini (bagian 3) | G4 |

---

## 5. Rujukan

| Berkas | Isi |
|---|---|
| `docs/module-blueprints/accounting/contracts/cross-module-contract.md` (`ACC-XMOD-0.4`) bagian 4b, 8a, dan 9 | Isi tanda terima, empat aturan pesan saldo, dan butir yang menunggu kesanggupan Finance |
| `docs/module-blueprints/accounting/00-interview-decisions.md` — `ACC-DEC-107` sampai `114` dan `116` sampai `121` | Keputusan rekonsiliasi saldo subledger dan perlakuan jurnal hasil kejadian, beserta alasan dan contohnya |
| `docs/module-blueprints/accounting/evidence/14-balasan-accounting-atas-kode-finance-05-06-07.md` | Surat sebelumnya; bagian 7 masih menunggu jawaban |
