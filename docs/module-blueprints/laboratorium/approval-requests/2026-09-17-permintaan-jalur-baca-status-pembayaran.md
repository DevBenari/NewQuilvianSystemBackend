# Permintaan Persetujuan — Jalur Baca Status Pembayaran untuk Layar Laboratorium

| Field | Value |
|---|---|
| `request_id` | `LAB-REQ-008` |
| `tanggal` | 2026-09-17 |
| `pengaju` | Yoga Aji Pratama — Product/Domain Owner Laboratorium (`yogaaji452@gmail.com`) |
| `rujukan` | `LAB-COORD-010`; `LAB-DEC-062`; `05-evidence-reconciliation.md` revision `4` bagian 10 dan 11; `blueprint-manifest.md` revision `41` |
| `status` | `menunggu jawaban` |
| `ditujukan kepada` | Pemilik modul `billing-kasir` |
| `sifat` | Operasional. **Bukan** artefak desain — tidak masuk daftar hash manifest |

> **Koreksi pembukuan yang menyertai permintaan ini.** `LAB-REQ-007` bagian 7 menyatakan jalur
> baca ini *"sudah diajukan terpisah sebagai `LAB-COORD-010`"*. Pernyataan itu **keliru**.
> `LAB-COORD-010` selama ini hanya tercatat sebagai penahan pada `blueprint-manifest.md` — yang
> dengan benar menulis *"Perlu permintaan persetujuan ke pemilik Billing"* — dan **tidak pernah
> menjadi permintaan kepada siapa pun**. Dokumen inilah permintaan yang dimaksud, dan baris pada
> `LAB-REQ-007` sudah dikoreksi agar menunjuk ke sini.
>
> Ditulis terbuka karena akibatnya nyata: sebuah penahan disangka sedang menunggu jawaban,
> padahal belum pernah ditanyakan. Ia menggantung sejak 2026-09-15.

---

## 1. Satu paragraf untuk yang tidak punya waktu

Layar laboratorium perlu **menampilkan** status pembayaran pasien, dan pada satu jalur perlu
**mengunci tombol** sampai pasien membayar. Laboratorium **tidak menyimpan satu pun kolom
pembayaran** dan memang tidak boleh — itu ditetapkan `RJ-BIL-GATE-DEC-003` dan diperkuat
`LAB-DEC-037`. Karena itu Laboratorium memerlukan **jalur baca** milik Billing.

Yang diminta **bukan** endpoint baru yang besar. Billing sudah punya
`GET …/invoices/encounters/{encounterId}/charge-summary`. Masalahnya jawaban endpoint itu memuat
**seluruh angka uang** — bruto, diskon, pajak, bagian pasien, bagian penjamin — dan Laboratorium
**tidak boleh melihatnya**. Yang dibutuhkan hanya **satu dari tiga kata**.

Permintaan ini karena itu berisi **dua hal**: satu keputusan pemetaan, dan satu jalur baca sempit.

---

## 2. Kenapa Laboratorium membutuhkannya

Dua kebutuhan berbeda, dan keduanya sudah diputuskan pemilik modul Laboratorium.

| Kebutuhan | Keputusan | Wujudnya di layar |
|---|---|---|
| **Menampilkan** status pembayaran pada daftar pesanan | `LAB-DEC-069`; requirement Menu Hasil bagian 5.3 | Satu kolom berisi `Belum ditagihkan`, `Belum Lunas`, atau `Lunas` |
| **Mengunci** tombol `Proses Pemeriksaan` bagi pasien Mandiri/tunai sampai `Lunas` | `LAB-DEC-062` | Tombol mati; petugas diberi tahu sebabnya |

Yang kedua lebih menentukan, dan pantas dibaca perlahan. **Selama jalur bacanya belum ada,
penguncian itu tidak dapat ditegakkan backend sama sekali.** Menegakkannya hanya di layar berarti
aturan uang yang dapat dilewati siapa pun yang memanggil API langsung — dan modul ini sudah
mencatat kelas kesalahan itu pada `AC-83`, ketika sebuah janji ternyata hanya ditegakkan frontend
dan baru ketahuan jauh belakangan.

Hari ini **tidak ada penguncian sama sekali**. Pasien Mandiri dapat diproses pemeriksaannya tanpa
membayar, dan sistem tidak melihat apa pun yang salah.

---

## 3. Yang sudah ada, dan kenapa belum cukup

Dibaca langsung dari source pada backend `13665452`.

| Hal | Keadaan |
|---|---|
| Endpoint | `GET /api/v1/health-services/billing-management/billing/invoices/encounters/{encounterId}/charge-summary` — **ada** |
| Hak akses | `BillingInvoice : Read` |
| Jawabannya | `EncounterChargeSummaryResponse`: `InvoiceId`, `InvoiceNumber`, `ServiceType`, `Status`, `CurrentCalculationVersion`, `InvoiceCount`, daftar kategori biaya, dan **`Totals` lengkap** |
| Nilai `Status` | `OPEN`, `FINAL`, `CLOSED`, `SETTLED_BY_WRITE_OFF` (`BillingInvoiceStatuses`) |

Dua hal membuatnya belum dapat dipakai apa adanya.

### 3.1 Ia memberi jauh lebih banyak daripada yang boleh dilihat Laboratorium

`LAB-REQ-005` bagian 5.2 sudah menuliskan batasnya, dan batas itu ditetapkan Laboratorium
**atas dirinya sendiri**:

| Laboratorium melakukan | Laboratorium **tidak** melakukan |
|---|---|
| Menampilkan metode pembayaran secara baca-saja | Memutuskan apakah pasien membayar |
| Mengirim fakta kelayakan tagih | Membentuk, mengubah, atau membatalkan tagihan |

Memberi Laboratorium `BillingInvoice : Read` berarti memberinya bruto, diskon, pajak, bagian
pasien, dan bagian penjamin — **seluruhnya**. Itu melampaui kebutuhannya dan memperluas permukaan
data keuangan ke modul klinis tanpa alasan. Laboratorium **tidak meminta** hak akses itu.

### 3.2 `Status` invoice bukan status pembayaran

Ini inti persoalannya, dan **bukan** hal yang boleh disimpulkan Laboratorium sendiri.

`OPEN`, `FINAL`, dan `CLOSED` menggambarkan **daur hidup tagihan**, bukan apakah uangnya sudah
diterima. Sebuah invoice `FINAL` belum tentu sudah dibayar; `SETTLED_BY_WRITE_OFF` diselesaikan
justru **tanpa** pembayaran pasien. Sementara ketiga kata yang dibutuhkan layar laboratorium —
`Belum ditagihkan`, `Belum Lunas`, `Lunas` — berbicara tentang **uang pasien**.

Menerjemahkan yang satu menjadi yang lain adalah **aturan bisnis milik Billing**. Bila
Laboratorium menyimpulkannya sendiri dari nilai `Status`, ia akan mengarang aturan uang — persis
yang dilarang `LAB-DEC-037`, dan kekeliruannya tidak akan terlihat sebagai galat sistem melainkan
sebagai kolom yang tampil meyakinkan dan salah.

---

## 4. Yang diminta

### 4.1 Butir 1 — keputusan pemetaan

Mohon Billing menetapkan pemetaan resmi dari keadaan tagihan yang sebenarnya menjadi tiga kata
yang dipakai layar laboratorium.

| Kata yang tampil | Artinya menurut requirement Laboratorium (`BP-003`) | Padanannya di Billing |
|---|---|---|
| `Belum ditagihkan` | Pesanan sudah dibuat, tetapi belum diverifikasi petugas | **perlu ditetapkan Billing** |
| `Belum Lunas` | Sudah diverifikasi, pasien belum membayar | **perlu ditetapkan Billing** |
| `Lunas` | Pasien sudah membayar di kasir | **perlu ditetapkan Billing** |

Tiga hal yang mohon ikut dijawab pada kesempatan yang sama, karena ketiganya akan menjadi
pertanyaan susulan bila dilewatkan:

1. **Bagaimana `SETTLED_BY_WRITE_OFF` dibaca layar laboratorium?** Ia bukan `Lunas` dalam arti
   pasien membayar, tetapi tagihannya memang sudah selesai. Bila ia diperlakukan sebagai `Lunas`,
   penguncian `LAB-DEC-062` akan terbuka untuk pasien yang tidak membayar — dan itu mungkin
   memang benar, tetapi harus **diputuskan**, bukan kebetulan.
2. **Bagaimana pembayaran sebagian dibaca?** Requirement Laboratorium hanya mengenal tiga kata dan
   tidak punya tempat untuk "sebagian". Apakah ia `Belum Lunas`?
3. **Bagaimana pasien berpenjamin dibaca?** Pasien BPJS atau asuransi tidak membayar di kasir.
   Apakah statusnya `Lunas` begitu penjaminnya sah, atau ia keadaan keempat yang belum punya nama?

### 4.2 Butir 2 — jalur baca sempit

Mohon Billing menyediakan **satu** jalur baca yang mengembalikan hasil pemetaan butir 1 saja.

| Hal | Usulan Laboratorium |
|---|---|
| Kegunaan | Membaca status pembayaran satu kunjungan, untuk ditampilkan dan untuk menegakkan `LAB-DEC-062` |
| Bentuk jawaban | Satu nilai enum — `BelumDitagihkan`, `BelumLunas`, `Lunas` — ditambah penanda boolean `bolehDiproses` |
| Yang **tidak** diminta | Nominal apa pun. Nol bruto, nol diskon, nol pajak, nol bagian pasien, nol bagian penjamin |
| Hak akses | Resource baru bersifat baca, misalnya `BillingPaymentStatus : Read`, **bukan** `BillingInvoice : Read` |

**Bentuk, nama, dan letak endpoint sepenuhnya wewenang Billing.** Usulan di atas ditulis supaya
kebutuhannya konkret, bukan untuk mendikte rancangan. Bila Billing menilai `charge-summary` yang
sudah ada lebih tepat dipersempit daripada ditambah saudaranya, Laboratorium mengikuti.

Satu hal yang mohon diperhatikan pada rancangan apa pun yang dipilih: **kuncinya sebaiknya
kunjungan (`encounterId`), bukan pesanan laboratorium.** Satu kunjungan dapat memuat beberapa
pesanan lab, dan tagihannya menyatu pada tingkat kunjungan — bukan per pesanan.

---

## 5. Yang **tidak** diminta permintaan ini

Agar tidak salah baca:

| Hal | Keadaan |
|---|---|
| Laboratorium menerima pembayaran tunai | **Ditolak sendiri** oleh `LAB-DEC-037`. Tidak diminta, dan tidak akan diminta |
| Laboratorium membentuk atau mengubah tagihan | Tidak, dan tidak pernah. `RJ-BIL-GATE-DEC-003` berlaku penuh |
| Laboratorium membaca nominal | **Tidak diminta.** Justru itu yang dihindari permintaan ini |
| Promo yang mengubah grand total | Dikeluarkan dari scope `LAB-DEC-037`. Sudah dilaporkan pada `LAB-REQ-005` bagian 5.3, belum berpemilik. Tidak diminta di sini |
| Piutang mitra (`LAB-COORD-007`) | Perkara terpisah, sudah diajukan `LAB-REQ-005` butir 4-7 dan masih menunggu jawaban |

---

## 6. Akibat bila belum dijawab

Ditulis apa adanya, bukan untuk mendesak.

| Yang tertahan | Akibat nyatanya hari ini |
|---|---|
| Kolom status pembayaran pada layar Laboratorium | Tidak dapat dibangun. Ia bagian dari `BR-50`, yang juga tertahan `LAB-SIGN-001` — jadi butir ini **bukan** penahan tercepat |
| Penguncian `LAB-DEC-062` | **Tidak ada penguncian sama sekali.** Pasien Mandiri dapat diproses tanpa membayar, dan sistem tidak melihat apa pun yang salah. Ini yang mendesak |

Perbedaan keduanya disebut supaya prioritasnya jelas: yang pertama menunggu hal lain juga, yang
kedua **hanya** menunggu permintaan ini.

---

## 7. Cara menjawab

Cukup balas dokumen ini dengan menyebut nomor butirnya.

| Butir | Yang dibutuhkan |
|---|---|
| 4.1 | Pemetaan tiga kata, beserta jawaban atas ketiga pertanyaan susulannya |
| 4.2 | Bentuk jalur bacanya, atau penolakan beserta alasannya |

Bila Billing menilai kebutuhan ini sebaiknya dilayani dengan cara lain sama sekali, jawaban itu
juga cukup — yang dibutuhkan Laboratorium adalah **keputusan**, bukan endpoint tertentu.

Jawaban akan dicatat sebagai penutupan `LAB-COORD-010` pada `blueprint-manifest.md` dan
`roadmap/traceability.md`, dan diturunkan menjadi task hanya setelah kontraknya disepakati.
