# Permintaan Persetujuan — `PHA-OQ-014`: Kegagalan Reservasi Setelah Pembayaran/Jaminan Valid

| Field | Value |
|---|---|
| `request_id` | `PHA-REQ-001` |
| `tanggal` | 2026-10-15 |
| `pengaju` | Ikbal — engineering modul Farmasi (`ikbalyuliyanto7@gmail.com`) |
| `rujukan` | `PHA-OQ-014` (`00-interview-decisions.md` baris 208); `PHA-DEC-041`; `PHA-DEC-018`; `PHA-DEC-023`; `PHA-DEC-033`; `PHA-OQ-002`; `roadmap/task-register.md` |
| `status` | `menunggu jawaban` |
| `ditujukan kepada` | Product/Domain Owner; Billing/Finance; Pharmacy/Clinical Governance |
| `sifat` | Operasional. **Bukan** artefak desain — tidak masuk daftar hash manifest |
| `menahan` | `PHA-BE-003`, `PHA-FE-001` |

> `PHA-OQ-014` berstatus `open` dan `BLOCKING` sejak **20 Agustus 2026**. Dokumen ini
> mengubahnya dari pertanyaan menggantung menjadi keputusan yang dapat diambil sekali duduk.

> **Yang TIDAK ditanyakan dokumen ini.** *Kapan* stok direservasi **sudah diputuskan** dan tidak
> dibuka ulang. `PHA-DEC-041` (`approved` oleh product owner, 20 Agustus 2026) menetapkan:
> *"Reservasi dilakukan setelah pembayaran/jaminan valid dan Farmasi mulai memproses resep;
> penentuan Depo tidak mengambil stok, sedangkan stok fisik baru berkurang saat penyerahan
> berhasil."*
>
> Jadi prinsipnya sudah berlaku. Yang belum pernah diputuskan hanya **tiga hal turunan**: berapa
> lama reservasi bertahan, apa yang terjadi saat masa itu habis, dan apa yang terjadi ketika
> reservasinya **gagal**. Hal terakhir itulah `PHA-OQ-014`.

---

## Latar Belakang

Kasus yang dibahas:

1. Dokter memfinalkan resep.
2. Sistem memeriksa stok dan stok masih tersedia.
3. Pasien menyelesaikan pembayaran / jaminan dinyatakan valid.
4. Sebelum obat diserahkan, stok digunakan transaksi lain.
5. Saat Farmasi memproses resep, reservasi stok gagal karena stok sudah habis.

Contoh:

```
10:00  Dokter finalisasi resep — stok 9
10:05  Pasien A membayar
10:07  Pasien B mengambil 9 unit — stok menjadi 0
10:15  Pasien A diproses Farmasi — stok tidak tersedia
```

`PHA-DEC-018` sudah mempersempit risiko dengan validasi stok pada finalisasi resep.

Namun masih terdapat race window antara finalisasi/pembayaran dan penyerahan.

### Bukti kode atas keadaan saat ini

Diperiksa 15 Oktober 2026 pada cabang `Ikbal`:

| Titik | Memeriksa stok? | Menahan stok? | Berkas |
|---|---|---|---|
| Dokter memilih obat di layar | tidak — stok tidak ditampilkan | tidak | `doctor-queues/tabs/` |
| Item resep ditambahkan | tidak | tidak | `PrescriptionItemService.cs` |
| **Resep difinalkan** | **ya, sejak 15 Okt** | tidak | `PrescriptionValidationService.cs` |
| Pasien membayar | tidak | tidak | — |
| Farmasi menyiapkan | ya | **ya** | `PrescriptionDispensingService.cs` |

Dari **16 service resep**, hanya `PrescriptionDispensingService` yang menyentuh stok untuk
menahannya. Gerbang finalisasi (`PHA-DEC-018`) baru dipasang 15 Oktober 2026 — sebelum itu resep
bahkan dapat difinalkan dan dibayar untuk obat yang stoknya sudah nol sejak awal, meskipun
keputusan itu sudah disahkan 19 Agustus 2026.

**Catatan penting.** `PHA-DEC-041` menempatkan reservasi *"setelah pembayaran valid **dan**
Farmasi mulai memproses resep"*. Di kode, reservasi terjadi pada **penyiapan**, yaitu bagian
akhir dari "Farmasi memproses". Jadi implementasinya tidak menyimpang dari keputusan — tetapi
inilah yang membuka race window, karena antara pembayaran dan penyiapan stok belum tertahan
sama sekali.

---

## Pertanyaan Keputusan

Apa perilaku resmi sistem ketika:

- pembayaran atau jaminan pasien sudah valid;
- resep sudah dapat diproses;
- tetapi reservasi stok gagal karena stok habis?

Keputusan ini memengaruhi:

- Farmasi
- Billing / Kasir
- Product
- pengalaman pasien
- koreksi transaksi keuangan

Keputusan tidak boleh ditentukan oleh implementasi teknis.

---

## Opsi A — Reservasi Sejak Pembayaran Valid — **REKOMENDASI**

Memajukan titik reservasi dari *penyiapan* ke *saat pembayaran/jaminan dinyatakan valid*. Ini
pengetatan dari `PHA-DEC-041`, bukan pembalikannya.

1. stok langsung direservasi untuk resep begitu pembayaran/jaminan valid;
2. stok reservasi tidak dapat digunakan resep lain;
3. Farmasi kemudian mengambil stok dari reservasi tersebut;
4. reservasi dilepas apabila:
   - resep diserahkan;
   - transaksi dibatalkan;
   - resep dibatalkan;
   - reservasi kedaluwarsa.

### Keuntungan

- pasien yang sudah membayar mendapat kepastian obat;
- mencegah stok "direbut" transaksi lain;
- menghilangkan mayoritas skenario `PHA-OQ-014`;
- workflow Billing dan Farmasi menjadi deterministik;
- tidak perlu menentukan refund setelah stok tiba-tiba habis pada proses normal.

### Konsekuensi

Diperlukan:

- reservation record / reservation state;
- mekanisme release;
- expiry;
- concurrency-safe reservation;
- audit trail.

### TTL Reservasi

Rekomendasi awal rawat jalan: **4 jam**

Nilai ini **belum** menjadi keputusan final.

Perlu disahkan Product + Farmasi berdasarkan:

- waktu tunggu Farmasi;
- waktu operasional;
- antrean pasien;
- kemungkinan pasien meninggalkan rumah sakit.

> **Mengapa TTL wajib ada.** Tanpa pelepasan otomatis, resep rawat jalan yang pasiennya pulang
> tanpa menebus akan mengunci stok selamanya. Dalam hitungan hari apotek akan "kehabisan" obat
> yang fisiknya ada di rak — masalah yang lebih buruk daripada yang sedang diperbaiki.
>
> Masa berlaku reservasi **belum pernah diputuskan**. Ia hanya pernah disebut pada
> `PHA-DEC-013`, yang sudah `superseded`.

---

## Opsi B — Tidak Memajukan Reservasi

Stok tetap bebas digunakan sampai Farmasi benar-benar menyiapkan obat — yaitu keadaan sekarang.

Jika stok habis setelah pasien membayar, sistem masuk exception flow.

Harus diputuskan:

1. Refund atau hold pembayaran?
2. Berapa lama pasien boleh menunggu?
3. Siapa yang menghubungi dokter?
4. ~~Apakah substitusi obat diperbolehkan?~~ — **sudah diputuskan, lihat catatan di bawah**
5. Jika harga obat pengganti berbeda:
   - tambah tagihan?
   - refund selisih?
   - rumah sakit menanggung?
6. Bagaimana status invoice?
7. Bagaimana status resep?
8. Siapa yang memiliki kewenangan menyelesaikan exception?

### Risiko

Opsi ini membutuhkan jauh lebih banyak keputusan bisnis dan rekonsiliasi Billing.

> **Catatan atas butir 4.** Substitusi obat **sudah diputuskan** dan tidak terbuka lagi:
> `PHA-DEC-033` menetapkan apoteker dan sistem tidak boleh melakukan substitusi; setiap
> perubahan obat hanya oleh dokter melalui perubahan resep. Model datanya mengikuti —
> `TrxPrescriptionDrugSubstitution` memisahkan `ProposedByPharmacistId` dari
> `ApprovedByDoctorId`, dan enum persetujuannya hanya mengenal `ApprovedByDoctor` /
> `RejectedByDoctor`; tidak ada `ApprovedByPharmacist` maupun `ApprovedBySupervisor`.
>
> **Catatan atas butir 5.** Sudah diputuskan untuk kasus recall/kerusakan:
> `PHA-DEC-035` menetapkan kenaikan harga tidak dibebankan kepada pasien dan menjadi beban
> rumah sakit; `PHA-DEC-036` menetapkan pasien tidak diminta kembali ke kasir. Yang belum jelas
> adalah apakah kedua aturan itu juga berlaku untuk kehabisan stok biasa — bukan recall.
>
> **Catatan atas butir 3.** Batas waktu respons dokter atas klarifikasi **belum diputuskan**;
> di kode sebuah klarifikasi dapat terbuka selamanya. Itu bagian `PHA-OQ-002`.

---

## Rekomendasi

Pilih: **Opsi A — reservasi dimajukan ke saat pembayaran/jaminan valid.**

Alasan — prinsip bisnis yang diinginkan:

> Jika pasien sudah dapat membayar resep, maka stok obat untuk pasien tersebut harus sudah
> dijamin tersedia.

Dengan model ini, pembayaran menjadi titik komitmen stok. Opsi A juga sejalan dengan arah
`PHA-DEC-041` yang sudah disahkan; yang berubah hanya *seberapa awal* dalam rentang
"Farmasi memproses" reservasinya terjadi.

---

## Keputusan yang Dibutuhkan

Mohon Product + Billing/Finance + Farmasi menetapkan:

### `DEC-1` — konfirmasi, bukan pertanyaan baru

`PHA-DEC-041` sudah menetapkan reservasi dilakukan setelah pembayaran/jaminan valid. Yang
diminta: apakah reservasi dimajukan ke **saat pembayaran dinyatakan valid**, bukan menunggu
Farmasi membuka penyiapan?

- [ ] YA — dimajukan ke saat pembayaran valid (Opsi A)
- [ ] TIDAK — tetap pada penyiapan seperti sekarang (Opsi B)

### `DEC-2`

Jika YA, TTL reservasi Rawat Jalan:

- [ ] 1 jam
- [ ] 2 jam
- [ ] 4 jam — rekomendasi awal
- [ ] sampai akhir hari
- [ ] lainnya: `__________`

Rawat inap: apakah perlu TTL sama sekali, mengingat pasiennya tidak pulang?

- [ ] tanpa TTL
- [ ] dengan TTL: `__________`

### `DEC-3`

Apa yang terjadi ketika TTL habis tetapi obat belum diserahkan?

- [ ] reservasi dilepas otomatis
- [ ] perlu konfirmasi Farmasi
- [ ] lainnya: `__________`

### `DEC-4`

Apakah resep yang pembayaran/jaminannya sudah valid tetapi reservasi kemudian gagal boleh masuk
proses Farmasi?

- [ ] TIDAK — masuk exception flow
- [ ] YA — dengan status khusus
- [ ] lainnya: `__________`

### `DEC-5` — mengeluarkan `PHA-DEC-023` dari status `draft`

`PHA-DEC-023` mengusulkan Kasir berwenang menandai pembayaran tidak dilanjutkan dan melepaskan
reservasi stok, tanpa dapat menghapus atau mengubah resep klinis. Statusnya masih `draft` sejak
19 Agustus 2026, padahal Opsi A menjadikannya jalur pelepasan utama.

- [ ] disahkan apa adanya
- [ ] disahkan dengan perubahan: `__________`
- [ ] ditolak — pelepasan dilakukan peran lain: `__________`

### `DEC-6`

Apakah `PHA-DEC-035` dan `PHA-DEC-036` — kenaikan harga obat pengganti ditanggung rumah sakit
dan pasien tidak kembali ke kasir — juga berlaku untuk **kehabisan stok biasa**, bukan hanya
recall atau kerusakan?

- [ ] YA, berlaku sama
- [ ] TIDAK, kehabisan stok biasa diperlakukan berbeda: `__________`

---

## Dampak terhadap Task

Setelah `DEC-1` sampai `DEC-6` disahkan:

| Task | Outcome | Status sekarang | Setelah keputusan |
|---|---|---|---|
| `PHA-BE-003` | Integrasikan routing depo dan reservasi stok ke workflow Farmasi | `BLOCKED BUSINESS` — `PHA-OQ-014` | `READY FOR IMPLEMENTATION` |
| `PHA-FE-001` | UI status routing/reservasi stok Farmasi | `BLOCKED BUSINESS` — `PHA-OQ-014` | `READY FOR IMPLEMENTATION` |

Kedua task ini adalah **satu-satunya** yang menahan modul Farmasi dari 100%. Rekap saat ini
17/19 = **89,5%**; tanpa kedua blocker keputusan bisnis ini **17/17 = 100%**.
Lihat `roadmap/task-register.md`.

---

## Acceptance Tingkat Bisnis yang Disarankan

Jika Opsi A dipilih:

1. pembayaran tidak boleh dianggap selesai untuk workflow Farmasi tanpa reservasi stok berhasil;
2. reservasi harus atomik terhadap stok;
3. transaksi paralel tidak boleh menggunakan quantity yang sudah direservasi;
4. pembatalan pembayaran melepaskan reservasi;
5. pembatalan resep melepaskan reservasi;
6. expiry melepaskan reservasi;
7. dispensing mengonsumsi reservasi;
8. retry harus idempotent;
9. semua perubahan reservasi memiliki audit trail;
10. kegagalan reservasi harus menghasilkan status bisnis eksplisit, bukan HTTP `500` generik.

---

## Koreksi pembukuan yang menyertai permintaan ini

Dua hal ditemukan saat menyiapkan dokumen ini, dan ditulis terbuka karena keduanya memengaruhi
cara `PHA-OQ-014` dibaca:

| Hal | Temuan |
|---|---|
| `PHA-OQ-020` | Tercatat `superseded` oleh **`PHA-DEC-052a`**, dengan keterangan *"Reservasi ditegaskan tetap berlaku, 3 September 2026"*. Namun **`PHA-DEC-052a` tidak memiliki barisnya sendiri** di `00-interview-decisions.md` — hanya `PHA-DEC-052` yang ada, dan isinya tentang urutan implementasi, bukan reservasi. Substansinya tampaknya benar (reservasi tetap berlaku, dan `PHA-DEC-041` mendukungnya), tetapi rujukannya menggantung dan perlu dirapikan pemilik dokumen. |
| `PHA-DEC-018` | Disahkan 19 Agustus 2026 tetapi gerbangnya baru terpasang 15 Oktober 2026 — hampir dua bulan keputusan yang sah tidak berlaku di sistem. Dicatat sebagai `PHA-D-11` pada register task. |

---

## Pertanyaan yang sebaiknya dijawab di pertemuan yang sama

| Pertanyaan | Isi | Mengapa bersama `PHA-OQ-014` |
|---|---|---|
| `PHA-OQ-014` | Dokumen ini | — |
| `PHA-OQ-002` | SOP substitusi: batas waktu respons dokter atas klarifikasi, dan tindakan apoteker bila dokter tidak dapat dihubungi | Menentukan jalan keluar ketika reservasi tetap gagal, pada Opsi A maupun Opsi B |

`PHA-OQ-020` **tidak** perlu dijawab lagi — ia sudah ditutup 3 September 2026 dengan kesimpulan
reservasi tetap berlaku. Hanya rujukan dokumennya yang perlu dirapikan.

---

## Owner Keputusan

- Product
- Billing / Finance
- Farmasi

Engineering hanya mengimplementasikan keputusan yang telah disahkan.
