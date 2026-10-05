# Balasan Finance atas Lima Pertanyaan Accounting, Dua Pelurusan, dan Satu Pengakuan

| Field | Nilai |
|---|---|
| Dari | Yasmin, owner / penggarap modul Finance (AR/AP) |
| Untuk | Rizki, owner modul Accounting |
| Tanggal | 1 Oktober 2026 |
| Menjawab | `docs/module-blueprints/accounting/evidence/16-balasan-accounting-atas-surat-finance-15-16-21.md` bagian 6, butir 16.1 sampai 16.5 |
| Sifat | **Jawaban, dua pelurusan atas surat kami sendiri, satu pengakuan, dan tiga permintaan** |
| Dasar keputusan | `docs/module-blueprints/finance-management/00-interview-decisions.md`: `FIN-DEC-111` sampai `FIN-DEC-138`, seluruhnya `approved` sisi Finance 1 Oktober 2026 |
| Desain yang menggambarnya | `FIN-DES-078` sampai `FIN-DES-091`, `approved` 1 Oktober 2026 |
| Kontrak yang berlaku | `ACC-XMOD-0.6` di sisi Anda; `FIN-INTEGRATION-1.7` di sisi kami |

Berkas ini berdiri sendiri, supaya dapat dibaca tanpa membuka dokumen blueprint Finance yang lain.

---

## 1. Ringkasan satu paragraf

Kelima pertanyaan Anda terjawab, dan jawabannya menuntut **tiga hal dari Anda**: menyetujui
`PENERIMAAN-KASIR` tetap per kuitansi dengan dimensi shift dan metode, meratifikasi satu kode penanda
baru, dan menerima konvensi tanggal WIB. Tetapi surat ini membawa dua hal yang **merugikan posisi
kami sendiri**, dan keduanya perlu Anda ketahui sebelum menimbang: **surat kami nomor 21 salah
membaca source kami sendiri** di satu tempat, dan **janji "G4 siap" pada surat 15 dan 21 tidak punya
kode sama sekali** — Finance belum pernah mengirim satu pun kejadian kepada Anda, karena jalur
pengirimnya memang belum pernah dibangun. Keduanya kami sampaikan di bagian 3 dan 4, sebelum
permintaan apa pun.

---

## 2. Jawaban lima pertanyaan

| # | Pertanyaan Anda | Jawaban Finance | Dasar |
|---:|---|---|---|
| 16.1 | `PENERIMAAN-KASIR` per kuitansi atau per shift? | **Tetap per kuitansi**, tetapi kejadiannya kami **tambahi nomor shift dan metode pembayaran**, sehingga Anda dapat meringkasnya menjadi satu jurnal per shift per metode lewat aturan posting. Rinciannya bagian 5 | `FIN-DEC-111` |
| 16.2 | Saldo per akun, bukan per kelompok — dan utang honor dokter | **Sanggup per akun, bahkan per segmen.** Kami membangun pemetaan "kelompok saldo + segmen → kode akun control". Piutang dapat dipecah menurut jenis debitur, utang jasa medis menurut jenis penerima. **Utang honor dokter kami kirim sendiri** sebagai kelompok `UTANG-JASA-MEDIS`, bernilai `0.00` selama tabelnya belum terisi. Rinciannya bagian 6 | `FIN-DEC-113`, `FIN-DEC-122` |
| 16.3 | Saldo negatif | **Kami kirim apa adanya, bertanda negatif.** Larangan pada surat 21 **kami cabut** — lihat pelurusan di bagian 3 | `FIN-DEC-112` |
| 16.4 | Shift yang masih terbuka saat snapshot | **Ya, kami nyatakan ulang otomatis** dengan `SourceVersion` lebih tinggi, **hanya** untuk akun yang nilainya berubah. Caranya berubah dari yang Anda bayangkan: saldo kini **dihitung sebagai posisi per tanggal** dari buku mutasi, bukan diambil dari rekap harian. Rinciannya bagian 7 | `FIN-DEC-114`, `FIN-DEC-125` |
| 16.5 | Shift yang belum pernah ditutup | **Kami terbitkan penanda pembukaan shift**, satu kode baru bernilai nol. Ini yang kami mintakan ratifikasinya. Rinciannya bagian 8 | `FIN-DEC-115`, `FIN-DEC-121` |

---

## 3. Pelurusan pertama — surat kami nomor 21 salah membaca source kami sendiri

Surat 21 bagian 3.2 menulis Kas Kasir dihitung dari **"total fisik kas dari seluruh shift kasir yang
berstatus `CLOSED` / `REVIEWED`"**. Ketika kami memeriksa source untuk menjawab pertanyaan 16.4, kode
yang sebenarnya berjalan memakai **rekap kas harian milik Finance** (`FinDailyCashSnapshot` berstatus
`CLOSED`), yang menghitung kas dari shift **tanpa memeriksa statusnya sama sekali**, dan
penutupannya pun tidak memeriksa apakah shift hari itu sudah selesai.

Jadi kalimat pada surat 21 itu menggambarkan sesuatu yang **belum dilakukan kode**. Kami menyesal
mengirimkannya sebagai pernyataan.

**Kabar baiknya:** desain baru justru membuat kalimat itu menjadi benar, dan lebih lengkap. Rumus
Kas Kasir yang kami pakai sekarang:

```text
Kas Kasir pada akhir periode
  = saldo awal cutover
  + kas dari shift berstatus CLOSED / REVIEWED
  + penerimaan tunai langsung di Finance
  − pembayaran tunai keluar
  − setoran bank
```

Seluruhnya bertanggal **WIB**, dan seluruhnya dihitung sampai tanggal akhir periode. Rekap kas harian
turun kedudukan menjadi **laporan operasional** untuk petugas kas, bukan lagi dasar saldo yang kami
kirim kepada Anda (`FIN-DEC-124`).

Satu akibat yang perlu Anda ketahui: karena rekap harian boleh ditutup walaupun masih ada shift belum
selesai, **rekap harian dan saldo yang kami kirim dapat berselisih secara sah**. Kami menampilkan
selisihnya di layar kami sendiri supaya petugas tidak bingung; Anda hanya menerima angka
snapshot-nya.

---

## 4. Pelurusan kedua dan satu pengakuan

### 4.1 Larangan nilai negatif kami cabut

Surat 21 butir 15.2 menyatakan Finance memperketat validasinya sehingga **"nominal negatif pada
`SALDO-SUBLEDGER` ditolak tanpa pengecualian"**. Pernyataan itu **tidak lagi berlaku**. Kontrak Anda
sendiri sudah mengizinkan nilai negatif untuk akun yang saldonya sedang tidak wajar, dan kami kini
mengikutinya (`FIN-DEC-112`).

**Satu kejujuran yang menyertainya.** Ketika kami mencari contoh nyata saldo negatif untuk diuji,
contoh yang kami pakai di percakapan internal — piutang yang lebih bayar — ternyata **tidak dapat
terjadi**: skema kami menjaga sisa piutang tidak pernah negatif, dan alokasi yang melebihi sisa
ditolak. Kandidat saldo negatif yang kami ketahui tinggal **Kas Kasir**. Aturannya tetap kami ambil
karena ia benar secara prinsip dan karena kontrak Anda memintanya — bukan karena contohnya terbukti.
Kami menuliskannya supaya Anda tidak menyiapkan penanganan untuk kasus yang belum ada buktinya.

### 4.2 Pengakuan — "G4 siap" belum punya kode

Ini bagian yang paling perlu Anda ketahui, dan kami menyampaikannya sekarang daripada Anda temukan
sendiri saat cutover.

Surat 15 dan 21 menyatakan G4 siap dan snapshot saldo terbit otomatis setiap tanggal 1 pukul 00.05
WIB. Pemeriksaan source kami menemukan:

| Yang dijanjikan | Keadaan sebenarnya pada tanggal surat itu |
|---|---|
| Snapshot terbit otomatis tanggal 1 pukul 00.05 WIB | **Tidak ada penjadwal.** Snapshot hanya dapat dipicu manual dari layar |
| Penanda shift terbit saat shift tertutup | **Tidak ada pemicu otomatis.** Hanya lewat tombol sinkronisasi |
| Kejadian terkirim ke kotak masuk Anda | **Tidak ada pengirim sama sekali.** Nol worker milik Finance terdaftar di aplikasi |

Akibatnya, **seluruh baris kotak keluar Finance menumpuk menunggu dan belum satu pun terkirim kepada
Anda**. Penyebabnya bukan kelalaian teknis: pembangunan worker pengiriman memang pernah **ditunda
atas keputusan owner Finance sendiri**, dan surat 15 dan 21 ditulis tanpa menyilangkan janjinya
dengan keadaan itu.

Penundaan itu **sudah kami cabut** (`FIN-DEC-118`). Kami membangun tiga bagian: worker pengiriman,
penjadwal snapshot pukul 00.05 WIB, dan pemicu otomatis penanda shift. Ketiganya dibangun dalam
keadaan **mati**, dan menyalakannya menuntut konfigurasi eksplisit.

**Karena itu kami nyatakan ulang: G4 BERSYARAT.** Ia siap sesudah ketiga bagian itu terbukti jalan
**dan** mekanisme kredensial akun layanan (G3) diputuskan bersama Platform. Mohon jangan
memperhitungkan G4 sebagai sudah selesai pada rencana cutover Anda.

---

## 5. Dimensi baru pada kejadian penerimaan — jawaban 16.1

Kami memilih **tetap satu kejadian per kuitansi**, dengan alasan yang kami harap Anda terima:

| Alasan | Isi |
|---|---|
| Jejak satu-ke-satu ke kuitansi tetap utuh | Pembalikan satu kuitansi tetap dapat ditelusuri ke kuitansinya, bukan ke ringkasan |
| Idempotensi yang sudah berjalan tidak dibongkar | Identitas kejadian tetap nomor kuitansi |
| Kejadian tidak perlu menunggu shift tutup | Bila diringkas per shift, kejadian baru terbit setelah shift selesai — dan pembalikan sesudahnya menjadi versi baru dari ringkasan |

Sebagai gantinya, kejadian kami **membawa dimensinya**, sehingga Anda dapat meringkas di sisi aturan
posting:

| Ruas baru | Isi |
|---|---|
| `CashierShiftId`, `CashierShiftNumber` | Shift tempat kuitansi itu berada |
| `PaymentMethodCode` | `CASH` atau `TRANSFER` — menentukan akun debit kas atau bank |
| `PaymentMethodAccountId` | Rekening sumber atau tujuan dana, bila non-tunai |
| `ReversalOfSourceTransactionId` | Nomor kuitansi asli, pada kejadian pembalik |

Kami memeriksa kotak masuk Anda lebih dulu: `ReceiveAccountingEventRequest` memiliki penampung ruas
tambahan, sehingga kelima ruas ini **tidak akan ditolak** walaupun Anda belum mengubah kode apa pun.
Yang kami minta adalah **persetujuan kontraknya**, bukan perubahan kode.

Contoh kejadian sesudah perubahan ini:

```json
{
  "EventTypeCode": "PENERIMAAN-KASIR",
  "SourceTransactionId": "RCP-20261002-0007",
  "SourceVersion": "1",
  "EventOccurredAt": "2026-10-01T19:10:00+00:00",
  "AccountingDate": "2026-10-02",
  "Amount": 150000.00,
  "CashierShiftNumber": "SHIFT-20261002-12",
  "PaymentMethodCode": "CASH",
  "PaymentMethodAccountId": null,
  "Components": null
}
```

### 5.1 Satu hal yang MUST Anda perhitungkan saat menyusun aturan posting

Kuitansi yang dibalik memakai **shift saat pembalikan terjadi**, bukan shift kuitansi aslinya
(`FIN-DEC-120`). Alasannya: uang fisik keluar dari laci shift itu, sehingga hitungan kas tiap shift
tetap cocok dengan lacinya, dan shift yang sudah tertutup tidak berubah angkanya.

**Akibatnya:** satu pasangan penerimaan dan pembaliknya **dapat tersebar di dua shift, bahkan dua
periode**. Penelusuran pasangan karena itu memakai `ReversalOfSourceTransactionId`, **bukan** nomor
shift. Bila aturan posting Anda mengandaikan pasangan selalu berada di satu shift, ia akan keliru.

---

## 6. Saldo per akun control — jawaban 16.2

Kami membangun pemetaan tersimpan, bukan konstanta di kode seperti sekarang:

| Kelompok saldo | Dapat dipecah menurut | Asal nilai pemecahnya |
|---|---|---|
| `KAS-KASIR` | — | — |
| `KAS-KECIL` | — | — |
| `PIUTANG` | `PAYER`, `PATIENT_GUARANTOR`, `EMPLOYEE_BENEFIT` | Jenis debitur yang sudah berjalan di Finance |
| `UTANG-SUPPLIER` | — | Belum ada sumbu pemecah yang disepakati |
| `UTANG-JASA-MEDIS` | `DOCTOR`, `NURSE`, `OTHER_PRACTITIONER` | Jenis penerima jasa yang sudah berjalan |

Satu akun boleh menanggung seluruh kelompok, atau satu akun per segmen — mengikuti bagan akun Anda,
bukan mengikuti bentuk tabel kami.

**Gagal tertutup.** Bila ada kelompok atau segmen yang belum punya kode akun, kami **tidak menerbitkan
apa pun** untuk periode itu — nol baris, bukan sebagian. Anda akan melihat periode yang belum
menerima saldo sama sekali, keadaan yang jelas, bukan periode yang menerima saldo kurang tanpa tanda.
Itu pilihan sengaja: saldo segmen yang hilang tanpa jejak adalah bentuk kegagalan paling berbahaya,
karena totalnya tetap terlihat wajar.

**Yang kami tunggu dari Anda:** daftar kode akun control definitif sesudah bagan akun disahkan (G2),
beserta keterangan akun mana yang menanggung segmen mana. Kami dapat membangun dan mengujinya lebih
dulu dengan nilai sementara.

### 6.1 Utang honor dokter

Pada surat 16 Anda menyebutnya tidak ada di daftar kami. Kami semula hendak memintanya dari modul
Medical Fee, lalu memeriksa source: **tabel utang jasa tenaga medis adalah milik Finance**, dan modul
Medical Fee sendiri belum ada. Jadi **kami yang mengirimnya** (`FIN-DEC-122`), sebagai kelompok
`UTANG-JASA-MEDIS`.

Selama tabel itu belum punya pengisi, saldonya **`0.00`** — dan kami tetap mengirimnya, mengikuti
aturan Anda bahwa setiap akun control dikirim termasuk yang bersaldo nol. Angkanya nol karena memang
belum ada utangnya, bukan karena tidak dihitung.

---

## 7. Saldo sebagai posisi per tanggal — jawaban 16.4

Pertanyaan Anda tentang shift yang tertutup sesudah snapshot membuat kami memeriksa bagaimana saldo
kami sebenarnya dihitung. Temuannya lebih besar daripada pertanyaannya: **dua jalur pembayaran di
Finance mengubah saldo tanpa meninggalkan riwayat bertanggal sama sekali**. Akibatnya saldo kami
hanya dapat menjawab "berapa sekarang", bukan "berapa pada tanggal 30 September".

Kami menutupnya dengan membangun **buku mutasi** untuk piutang, utang supplier, dan kas
(`FIN-DEC-123`). Setiap perubahan saldo menulis satu baris bertanggal WIB, dan posisi tanggal mana
pun dihitung dari sana.

| Pertanyaan Anda | Jawaban |
|---|---|
| Apakah Finance menyatakan ulang saldo bila shift selesai terlambat? | **Ya, otomatis.** Shift yang baru selesai menulis mutasi kas **bertanggal tanggal shift**. Bila tanggal itu di dalam periode yang sudah kami kirim, posisinya berubah dan kami menerbitkan ulang |
| Akun mana yang diterbitkan ulang? | **Hanya yang nilainya berubah.** Akun yang sama nilainya tidak dikirim lagi |
| Identitasnya? | `SourceTransactionId` tetap sama, `SourceVersion` naik — mekanisme yang sudah ada, tanpa kontrak baru |
| Kapan diperiksa? | Setiap hari pukul 00.05 WIB oleh penjadwal yang sama |

**Satu batas yang kami catat terbuka.** Buku mutasi **tidak diisi mundur**, sehingga posisi saldo
untuk tanggal **sebelum** buku mutasi berjalan tidak dapat dihitung. Kami menolak menerbitkan
snapshot untuk periode yang berakhir sebelum tanggal cutover, beserta pesan yang menyebutnya —
bukan mengembalikan angka yang kelihatan wajar. Mohon ini diperhitungkan saat menetapkan periode
pertama yang akan direkonsiliasi.

---

## 8. Kode baru yang kami mintakan ratifikasi — jawaban 16.5

| Field | Isi |
|---|---|
| Kode | **`PEMBUKAAN-SHIFT-KASIR`** |
| Sifat | **Penanda status**, bukan transaksi. `Amount = 0`, nol lawan jurnal, nol aturan posting — sekelas dua penanda yang sudah Anda setujui |
| Pemicu | Finance pertama kali melihat shift berstatus **belum final** |
| "Belum final" | Seluruh status selain `CLOSED` dan `REVIEWED`, yaitu `OPEN`, `HANDED_OVER`, `REOPENED`, `CLOSED_WITH_VARIANCE`, `PERLU_TINDAK_LANJUT` |
| `SourceTransactionId` | Nomor shift, sama dengan dua penanda yang sudah ada |
| `SourceVersion` | Nomor siklus, pola yang sama persis dengan penanda penutupan |
| `AccountingDate` | Tanggal shift, dalam WIB |

**Kenapa kode ini perlu, dan kenapa dua penanda yang sudah ada tidak cukup.** Anda menyatakan
`CLOSED_WITH_VARIANCE` dan `PERLU_TINDAK_LANJUT` "tetap menahan tutup bulan kami". Tetapi Anda hanya
dapat menahan sesuatu yang **Anda ketahui**. Shift yang dibuka lalu berakhir `CLOSED_WITH_VARIANCE`
di antara dua sinkronisasi kami **tidak pernah terlihat** oleh Anda sebagai shift yang terbuka —
sehingga Anda dapat menutup bulan di atas selisih kas yang belum disahkan, tanpa ada yang
memergokinya.

Dengan penanda pembukaan, satu siklus shift menghasilkan pasangan yang lengkap:

| Penanda | Terbit saat |
|---|---|
| `PEMBUKAAN-SHIFT-KASIR` | Shift terlihat belum final |
| `PENUTUPAN-SHIFT-KASIR` | Shift mencapai `CLOSED` atau `REVIEWED` |
| `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR` | Shift yang sudah tertutup dibuka kembali |

Anda menahan tutup bulan selama ada `PEMBUKAAN-` tanpa `PENUTUPAN-` pada siklus yang sama. Dengan itu
`ACC-DEC-065` dapat ditegakkan **tanpa** Anda perlu membaca tabel Billing.

**Yang kami minta:** ratifikasi nama kodenya, **dan** penambahannya ke daftar tertutup nilai nol di
kotak masuk Anda — daftar yang sama yang Anda setujui lewat `ACC-DEC-127` untuk dua penanda
sebelumnya. Selama belum turun, pengiriman **ketiga** penanda shift kami tetap kami tahan, persis
seperti sekarang.

---

## 9. Dua hal yang berubah tanpa Anda minta, dan satu yang tidak kami minta

### 9.1 Tanggal akuntansi memakai WIB

Saat menyisir source, kami menemukan `AccountingDate` dihitung dari waktu UTC pada **20 titik**.
Akibatnya pembayaran pukul 00.00–06.59 WIB tercatat sebagai **hari sebelumnya**, dan pada pergantian
bulan ia dapat jatuh ke **periode yang sudah Anda tutup**.

Kami memperbaikinya: seluruh tanggal akuntansi dan batas periode kami hitung dalam **kalender WIB**
(`FIN-DEC-116`). Waktu kejadian (`EventOccurredAt`) tetap UTC; yang berpindah hanya tanggal
akuntansinya.

Belum ada kejadian sungguhan yang terkirim kepada Anda, sehingga perbaikan ini **tidak membetulkan
data apa pun** — ia mencegah masalah sebelum kejadian pertama mengalir. Kami memintakan
**penerimaan Anda atas konvensi ini**, supaya tidak ada selisih tafsir saat rekonsiliasi.

### 9.2 Rekening bank bukan akun control kami

Kami tegaskan agar tidak Anda tunggu: Finance **tidak** memelihara saldo per rekening bank
(`FIN-DEC-137`). Kami menyimpan master rekening dan **identitas rekening** pada setiap transaksi, dan
meneruskannya kepada Anda bersama metode, nominal, tanggal, dan nomor rujukan. **Saldo rekening bank,
jurnal akun bank, dan rekonsiliasi rekening koran tetap milik Anda.** Rekening bank **tidak** akan
pernah muncul sebagai baris `SALDO-SUBLEDGER` dari kami.

### 9.3 Tagihan lama sebelum go-live — dan satu hal yang sengaja tidak kami minta

Kami akan memigrasikan piutang dan utang supplier yang sudah berjalan sebelum go-live sebagai **item
tagihan biasa** di tabel kami, berpenanda migrasi, lewat batch yang disetujui (`FIN-DEC-129`,
`FIN-DEC-136`). Dua hal yang perlu Anda ketahui:

| Hal | Isi |
|---|---|
| **Migrasi tidak menerbitkan kejadian apa pun kepada Anda** | Nilainya sudah tercakup saldo awal manual Anda (G5). Menerbitkannya akan menghitung ganda. Jadi Anda **tidak** akan menerima `PENGAKUAN-PIUTANG` atau `PENGAKUAN-HUTANG-SUPPLIER` untuk tagihan lama |
| **Total migrasi kami rekonsiliasi terhadap saldo awal Anda** | Batch tidak dapat disetujui bila totalnya berbeda |

Cara kami mengetahui angka Anda: **petugas kami mengetikkannya** dari dokumen saldo awal Anda,
beserta rujukan dokumennya, lalu sistem membandingkannya dengan total item. Kami **sengaja tidak
meminta** Anda menyediakan jalur baca saldo awal, walaupun itu akan lebih baik — surat ini sudah
memuat tiga permintaan, dan kami tidak ingin menambah yang keempat pada satu putaran. Bila kelak Anda
menilai jalur baca itu layak dibuat, kami siap memakainya.

Kelemahan cara ini kami catat apa adanya: angka yang diketik **dapat salah**, dan sistem hanya
memeriksa kedua angka itu cocok — bukan bahwa angkanya benar.

---

## 10. Yang kami butuhkan dari Anda

| # | Permintaan | Kenapa penting | Menahan apa di sisi kami |
|---:|---|---|---|
| 22.1 | **Persetujuan atas `PENERIMAAN-KASIR` tetap per kuitansi** beserta perluasan kontrak dengan dimensi shift dan metode. Ini menuntut `ACC-DEC-062` diubah, dan itu keputusan Anda, bukan kami | Tanpa persetujuan, kami mengirim bentuk yang bertentangan dengan keputusan Anda yang berlaku | Pengaktifan dimensi sebagai kontrak. Pembangunannya berjalan |
| 22.2 | **Ratifikasi `PEMBUKAAN-SHIFT-KASIR`** dan penambahannya ke daftar tertutup nilai nol di kotak masuk Anda | Tanpa itu, penegakan `ACC-DEC-065` tetap punya celah: shift yang tidak pernah terlihat terbuka | Pengiriman **ketiga** penanda shift |
| 22.3 | **Penerimaan konvensi tanggal WIB** untuk `AccountingDate` dan batas periode | Menghindari selisih tafsir satu hari, yang pada pergantian bulan berarti periode yang salah | Tidak menahan apa pun; kami memerlukan kesepakatan tafsirnya |

Jawaban boleh digabung dalam satu surat. Penolakan beserta alasannya **dapat kami terima sepenuhnya**
untuk ketiganya:

| Bila 22.1 ditolak | Kami rancang ulang menjadi ringkasan per shift lewat amandemen tersendiri. Itu membongkar jalur penerimaan yang sudah berjalan, jadi kami memilih menanyakannya lebih dulu daripada membangun lalu membongkar |
| Bila 22.2 ditolak | Bentuk penandanya kami rancang ulang. Kami **tidak** menyiapkan jalur cadangan sekarang, karena cadangan yang dipilih sepihak justru mengunci bentuk yang belum Anda setujui — pola yang sama dengan surat 16 |
| Bila 22.3 ditolak | Kami butuh tahu konvensi mana yang Anda tetapkan, karena salah satu pihak harus menyesuaikan |

---

## 11. Keadaan gerbang dari sisi kami

| Gerbang | Isi | Keadaan 1 Oktober 2026 menurut Finance |
|---|---|---|
| G2 | Bagan akun sah + aturan posting | Milik Anda. Kami menunggu daftar kode akun control definitif beserta pemetaan segmennya |
| G3 | Akun layanan aktif | **Masih terbuka** bersama Platform. Worker pengiriman kami dibangun mengambil kredensial dari konfigurasi, dan dibangun dalam keadaan mati |
| **G4** | **Pengirim Finance siap** | **BERSYARAT — bukan siap.** Lihat bagian 4.2. Tiga bagian dibangun; pengaktifannya menunggu G3 |
| G5 | Saldo awal manual | Kami membangun tempat saldo awal di sisi kami, dan **angkanya wajib sama dengan angka Anda**. Kami menunggu dokumennya |
| G6 | Deposit, kelebihan bayar, selisih kas, uang muka, refund, penegakan shift kasir | Penanda shift kami siap dibangun; pengirimannya menunggu 22.2. Refund `REFERRED_OUTPATIENT_ADMIN` tetap milik Anda bersama Billing |

Dua hal yang **tidak** kami bawa pada surat ini, supaya tidak tercampur: kode kejadian pendapatan
sewa (masih terbuka dari putaran sebelumnya) dan akun debit refund `REFERRED_OUTPATIENT_ADMIN`
(syarat G6 Anda bersama Billing).

---

## 12. Rujukan

| Berkas | Isi |
|---|---|
| `docs/module-blueprints/finance-management/00-interview-decisions.md` bagian closure pass 1 Oktober 2026 | `FIN-DEC-111` sampai `FIN-DEC-138` beserta alasan, contoh, dan koreksi atas keputusan kami sendiri |
| `docs/module-blueprints/finance-management/01-existing-capability-map.md` bagian 18 dan 19 | Hasil pemeriksaan source yang melahirkan dua pelurusan dan satu pengakuan pada surat ini |
| `docs/module-blueprints/finance-management/contracts/integration-contract.md` bagian 5.12 | Bentuk pesan sesudah perubahan, termasuk kelima ruas dimensi dan kode penanda baru |
| `docs/module-blueprints/finance-management/evidence/15`, `16`, `21` | Tiga surat kami sebelumnya, dua di antaranya diluruskan di sini |
