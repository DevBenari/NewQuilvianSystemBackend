# Panduan Menjalankan `BE-08` — Pengisian Status Keutuhan Catatan Lama

| Field | Value |
|---|---|
| Task | `BE-08` |
| Blueprint | `RM-BP-001` revisi 5 |
| Keputusan yang mendasari | `RM-DEC-014`, disahkan Yoga Aji Pratama 26 Agustus 2026 |
| Tanggal panduan | 26 Agustus 2026 |
| Sifat | **Menyentuh data pasien yang sudah tersimpan** |

> **Baca seluruh panduan ini sebelum menjalankan langkah mana pun.** Ini satu-satunya bagian
> modul rekam medis yang menyentuh data pasien yang sudah ada.

---

## 0. Hasil penelaahan yang sudah dijalankan

Penelaahan dijalankan pada 26 Agustus 2026. Hasilnya di bawah, dan hasilnya **jauh lebih
ringan daripada yang diperkirakan**.

| Yang ditelaah | Hasil |
|---|---:|
| Total CPPT | **10** |
| Penulis tidak tercatat | **0** |
| Penulis tercatat | 10 |
| Kunjungan sudah selesai | 10 |
| Kunjungan masih berjalan | 0 |
| Akan menjadi terkunci tanpa tanda tangan | **10** |
| Akan tetap draf | 0 |
| Tanpa kunjungan (tidak dapat didaftarkan) | **0** |

### Apa artinya angka ini

| Kekhawatiran semula | Keadaan sebenarnya |
|---|---|
| Jumlah barisnya tidak diketahui, mungkin sangat banyak | **10 baris.** Selesai dalam hitungan milidetik, satu potongan cukup |
| Akan ada catatan tanpa penulis | **Tidak ada.** Seluruh catatan mencantumkan penulisnya |
| Akan ada catatan yang tidak melekat ke kunjungan | **Tidak ada.** Seluruhnya berpasangan |
| Laporan kelengkapan akan menampilkan angka besar | **10 catatan.** Bukan angka yang mengejutkan |
| Pasien yang sedang dirawat dapat terganggu | **Tidak ada kunjungan berjalan.** Nol risiko gangguan pelayanan |

**Penjalanan bertahap tidak diperlukan** pada data sebesar ini. Satu potongan sudah mencakup
seluruhnya.

### Peringatan yang tetap berlaku

> **Basis data yang ditelaah hampir pasti bukan basis data produksi.**
>
> Sepuluh CPPT untuk sebuah sistem rumah sakit adalah jumlah lingkungan pengembangan, bukan
> jumlah pelayanan nyata. Koneksi `Development` pada project ini juga menunjuk server bersama,
> bukan server produksi.
>
> **Sebelum modul ini dipasang di produksi, penelaahan wajib dijalankan ulang di sana.** Angka
> produksi akan berbeda jauh, dan seluruh pertimbangan pada panduan ini — penjalanan bertahap,
> pemberitahuan ke unit rekam medis, pemilihan waktu — kembali berlaku penuh.
>
> Panduan ini karena itu tetap ditulis lengkap, bukan disederhanakan mengikuti angka 10.

### Satu hal yang perlu diperiksa saat percobaan

Penelaahan yang Anda jalankan tidak menyebut berapa CPPT yang berstatus **dibatalkan**
(`IsCancel`). Aturan `RM-DEC-014` memperlakukan catatan yang sudah dibatalkan berbeda: ia
ditandai **dibatalkan**, bukan **terkunci tanpa tanda tangan**.

Bila ada di antara 10 catatan itu yang sudah dibatalkan, hasil percobaan akan menampilkan
`jumlahDitandaiDibatalkan` lebih dari nol, dan `jumlahTerkunciTanpaTandaTangan` kurang dari 10.
**Itu benar, bukan kekeliruan.** Periksa saja agar tidak menimbulkan pertanyaan.

---

## 1. Apa yang dikerjakan pengisian ini

Modul rekam medis menyimpan status keutuhan setiap catatan klinis pada satu daftar tersendiri.
Catatan yang dibuat **setelah** modul ini aktif otomatis terdaftar. Catatan yang sudah ada
**sebelumnya** belum punya baris keutuhan sama sekali.

Selama belum diisi, catatan lama diperlakukan sebagai masih boleh diubah. Artinya seluruh
catatan yang dibuat sebelum modul ini aktif **tidak terlindungi aturan penguncian**.

Pengisian ini menutup kekosongan tersebut, dengan aturan berikut:

| Keadaan catatan lama | Status yang diberikan |
|---|---|
| Kunjungannya sudah selesai, batal, atau pasien tidak hadir | **Terkunci, tidak ditandatangani** |
| Kunjungannya masih berjalan | **Draf** — tetap dapat diselesaikan |
| Catatannya memang sudah dibatalkan | **Dibatalkan** |
| Tidak melekat ke kunjungan mana pun | **Dilewati** — tidak dapat didaftarkan |

---

## 2. Perubahan cara dari rancangan semula

Arsitektur bagian 8.2 menyebut pengisian ini dikerjakan sebagai **migration**. Cara itu diganti
menjadi **service yang dijalankan terkendali**, dengan tiga alasan:

| Alasan | Penjelasan |
|---|---|
| Jumlah barisnya tidak diketahui | Data produksi belum pernah ditelaah. Migration langsung berjalan tanpa dapat ditelaah lebih dulu |
| Waktunya tidak dapat dipilih | Migration berjalan otomatis saat aplikasi naik. Pengisian ini sebaiknya dijalankan ketika unit rekam medis sudah siap menerima angka besar pada laporan kelengkapan |
| Sulit dilanjutkan bila terhenti | Migration bersifat sekali jalan. Service dapat dijalankan bertahap dan dilanjutkan dari sisa yang belum diproses |

Yang **tidak** berubah: aturan penentuan statusnya persis seperti `RM-DEC-014`.

---

## 3. Langkah menjalankan

### Langkah 1 — Telaah dulu, jangan langsung jalankan

Panggil endpoint penelaahan. **Hanya membaca, tidak mengubah apa pun**, sehingga aman
dijalankan kapan saja termasuk pada jam sibuk.

```text
GET /api/v1/health-services/medical-record-management/backfill/survey?batchSize=500
```

Hak akses yang diperlukan: `MedicalRecordBackfill : Read`

Yang akan Anda dapatkan:

| Angka | Artinya |
|---|---|
| `totalProgressNote` | Seluruh catatan CPPT yang tersimpan |
| `belumTerdaftar` | Yang akan diproses pengisian |
| `akanTerkunciTanpaTandaTangan` | **Angka yang akan muncul besar pada laporan kelengkapan** |
| `akanTetapDraf` | Yang masih dapat diselesaikan penulisnya |
| `penulisTidakDiketahui` | Catatan yang tidak mencantumkan penulisnya |
| `tanpaKunjungan` | Yang tidak dapat didaftarkan |
| `catatanTertua`, `catatanTerbaru` | Rentang waktu data yang akan diproses |
| `perkiraanJumlahPotongan` | Perkiraan lama proses |
| `peringatan` | Hal yang perlu dibaca sebelum menjalankan |

**Catat angka-angka ini.** Angka inilah yang menjadi dasar pemberitahuan pada langkah 2, dan
menjadi pembanding untuk memastikan pengisian berjalan benar pada langkah 4.

### Langkah 2 — Beri tahu unit rekam medis lebih dulu

**Jangan lewati langkah ini.** Bahan pemberitahuannya ada pada bagian 5 panduan ini.

Yang wajib disampaikan: berapa banyak catatan yang akan bertanda "tidak ditandatangani", dan
mengapa angka itu bukan kegagalan sistem baru.

### Langkah 3 — Jalankan percobaan

Percobaan menghitung seluruhnya tetapi **tidak menyimpan apa pun**.

```text
POST /api/v1/health-services/medical-record-management/backfill/run-batch?batchSize=500&isDryRun=true
```

Bandingkan hasilnya dengan angka dari langkah 1. Bila berbeda jauh, **berhenti** dan telusuri
sebabnya sebelum melanjutkan.

### Langkah 4 — Jalankan sungguhan, bertahap

```text
POST /api/v1/health-services/medical-record-management/backfill/run-batch?batchSize=500&isDryRun=false
```

Hak akses yang diperlukan: `MedicalRecordBackfill : Update`

Ulangi pemanggilan selama `masihAdaSisa` bernilai benar. Setiap pemanggilan melanjutkan dari
sisa yang belum diproses, sehingga aman bila terhenti di tengah.

Yang perlu dipantau setiap potongan:

| Yang dipantau | Yang diharapkan |
|---|---|
| `jumlahDiproses` | Sama dengan ukuran potongan, sampai potongan terakhir |
| `perkiraanSisa` | Berkurang setiap potongan |
| Waktu tiap potongan | Stabil. Bila makin lambat, hentikan dan telaah |

**Kapan berhenti:** bila satu potongan gagal, jangan langsung mengulang. Telaah dulu sebabnya.
Potongan yang sudah berhasil tetap tersimpan, sehingga pengulangan tidak akan menggandakan data.

### Langkah 5 — Periksa hasilnya

Jalankan penelaahan sekali lagi. `belumTerdaftar` seharusnya tinggal sebanyak `tanpaKunjungan` —
yaitu catatan yang memang tidak dapat didaftarkan.

---

## 4. Cara mundur bila hasilnya keliru

Pengisian ini **hanya menambah baris** pada tabel keutuhan. Ia tidak mengubah satu pun catatan
klinis. Karena itu pembatalannya sederhana dan tidak menyentuh data pasien:

```sql
-- Menghapus baris keutuhan yang dibuat pengisian data lama.
-- Catatan klinisnya sendiri TIDAK tersentuh sama sekali.
DELETE FROM public."TrxClinicalDocumentIntegrity"
WHERE "LockTrigger" = 3          -- BackfillEncounterClosed
   OR ("CreateBy" = '<user-id-pelaksana>' AND "SignedAt" IS NULL);
```

Setelah dihapus, catatan lama kembali diperlakukan sebagai masih boleh diubah — persis keadaan
sebelum pengisian dijalankan.

**Perhatian:** jangan menjalankan penghapusan ini bila sudah ada catatan yang ditandatangani
atau dikoreksi setelah pengisian. Periksa lebih dulu:

```sql
SELECT COUNT(*) FROM public."TrxClinicalDocumentIntegrity"
WHERE "SignedAt" IS NOT NULL OR "AddendumCount" > 0;
```

Bila hasilnya lebih dari nol, pembatalan menyeluruh akan menghapus jejak tanda tangan dan
koreksi yang sah. Hubungi tim pengembang lebih dulu.

---

## 5. Bahan pemberitahuan untuk unit rekam medis

Bagian ini dapat disalin apa adanya dan dikirimkan.

---

> **Perihal: Perubahan pada laporan kelengkapan berkas rekam medis**
>
> Mulai [tanggal], sistem rekam medis elektronik menambahkan penanda keutuhan pada setiap
> catatan klinis. Penanda ini menyatakan apakah sebuah catatan masih boleh diubah atau sudah
> terkunci.
>
> **Yang akan Bapak/Ibu lihat.** Laporan kelengkapan berkas akan menampilkan sejumlah besar
> catatan bertanda **"terkunci, tidak ditandatangani"** — sekitar [isi angka dari penelaahan]
> catatan.
>
> **Mengapa angkanya besar.** Sebelum ini, sistem tidak pernah meminta dokter dan perawat
> menandatangani catatannya. Jadi catatan lama memang tidak pernah ditandatangani. Angka itu
> menggambarkan keadaan yang sudah berlangsung selama ini, **bukan kegagalan sistem baru** dan
> bukan pula kelalaian yang baru terjadi.
>
> **Apakah catatan itu tidak sah?** Tetap sah dan tetap dapat dibaca sebagai bagian rekam
> medis. Penandanya hanya menyatakan bahwa penulisnya belum sempat menandatangani.
>
> **Apakah angkanya akan berkurang?** Untuk catatan lama, tidak. Catatan yang kunjungannya sudah
> selesai tidak dapat ditandatangani mundur. Yang akan berubah adalah catatan **baru**: mulai
> sekarang dokter dan perawat menandatangani catatannya, sehingga catatan baru akan bertanda
> "ditandatangani".
>
> **Yang perlu Bapak/Ibu lakukan.** Tidak ada tindakan yang diperlukan sekarang. Angka ini
> menjadi titik awal pengukuran, dan yang perlu diperhatikan ke depan adalah apakah catatan
> **baru** ditandatangani penulisnya.
>
> Bila ada pertanyaan, hubungi [nama penanggung jawab modul].

---

## 6. Prasyarat yang harus terpenuhi sebelum langkah 4

| No | Prasyarat | Cara memastikan |
|---:|---|---|
| 1 | Penelaahan sudah dijalankan dan angkanya dicatat | Langkah 1 |
| 2 | Unit rekam medis sudah diberi tahu | Langkah 2, memakai bahan bagian 5 |
| 3 | Percobaan sudah dijalankan dan hasilnya masuk akal | Langkah 3 |
| 4 | Dijalankan di luar jam sibuk | Kesepakatan dengan unit terkait |
| 5 | Ada yang memantau selama proses berjalan | Kesepakatan tim |
| 6 | Cara mundur sudah dipahami | Bagian 4 panduan ini |

---

## 7. Yang sudah terbukti lewat uji otomatis

Aturan penentuan status sudah dibuktikan pada 11 uji otomatis sebelum menyentuh data sungguhan.
Berkasnya: `tests/QuilvianSystemBackend.Tests/MedicalRecordManagement/MedicalRecordBackfillTests.cs`

| Yang dibuktikan | Uji |
|---|---|
| Tiga keadaan kunjungan menghasilkan tiga status berbeda | `PengisianDataLama_MemberiStatusSesuaiKeadaanKunjungannya` |
| Kunjungan batal dan tidak hadir ikut terkunci | `KunjunganBatalAtauTidakHadir_CatatannyaIkutTerkunci` |
| Catatan tanpa penulis tetap dibuat, dengan penandanya | `CatatanTanpaPenulis_TetapDibuatDenganPenandaPenulisTidakDiketahui` |
| Catatan tanpa kunjungan dilewati dan dihitung terbuka | `CatatanTanpaKunjungan_DilewatiDanDihitungTerbuka` |
| Percobaan melaporkan angka sama tanpa menyimpan | `Percobaan_MelaporkanAngkaYangSamaTanpaMenyimpanApaPun` |
| Catatan yang sudah terdaftar tidak diproses ulang | `CatatanYangSudahTerdaftar_TidakDiprosesUlang` |
| Pengisian bertahap menyelesaikan seluruhnya | `PengisianBertahap_MenyelesaikanSeluruhnyaBilaDijalankanBerulang` |
| Penelaahan tidak mengubah apa pun | `Penelaahan_MelaporkanKeadaanTanpaMengubahApaPun` |

**Yang belum dan tidak dapat dibuktikan uji otomatis:** lama proses dan jumlah baris pada data
sungguhan. Keduanya hanya dapat diketahui lewat penelaahan pada langkah 1.
