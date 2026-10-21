# ISSUE-031 — Penerapan 3 Kartu Pilihan Perawatan Bayi (Rawat Gabung, Perinatologi, NICU), Konfigurasi Boks Kamar VIP Lantai 3, dan Perencanaan Unit NICU

```yaml
issue_id: ISSUE-EPS-031
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Admisi Rawat Inap — Langkah 3 Perawatan & DPJP Bayi dan Langkah 4 Penempatan (FE-INP-03 / FE-INP-026)"
sumber_laporan: "Arahan dan persetujuan pemilik sistem 21 Oktober 2026: penerapan 3 kartu pilihan jenis perawatan bayi (Rawat Gabung, Perinatologi, NICU), pendaftaran boks bayi pada sebagian kamar ibu (Kamar VIP 301 s.d. 305 di Lantai 3), pembersihan data penempatan uji coba, dan perencanaan kamar/bed fisik NICU"
tanggal_issue: "2026-10-21"
status: TERBUKA
keparahan_tertinggi: Mayor
source_sha_backend: "6406fc76 (MHamzah)"
source_sha_frontend: "381ec25fa (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-031-tiga-tipe-kamar-bayi-rawat-gabung-perinatologi-nicu.md
```

---

## 1. Ringkasan

Laporan ini menganalisis kebutuhan penyempurnaan alur pendaftaran admisi bayi baru lahir (`entry=newborn`) terkait penyajian pilihan jenis perawatan pada Langkah 3 ("Perawatan dan DPJP Bayi") serta kesiapan master data tempat tidur pendukung pada Langkah 4 ("Penempatan dan Pemesanan").

Berdasarkan hasil wawancara dan persetujuan pemilik sistem pada tanggal 21 Oktober 2026:
1. **Redesain Antarmuka Langkah 3**: Mengubah model pemilihan lama yang terdiri dari 2 kartu utama ("Rawat gabung dengan ibu" dan "Neonatal terpisah") ditambah kotak centang tersembunyi *"Membutuhkan perawatan neonatal intensif"* menjadi **3 kartu pilihan langsung yang eksplisit**:
   - **Kartu 1: Rawat Gabung dengan Ibu** (*Rooming-In*)
   - **Kartu 2: Ruang Perinatologi** (Neonatal Observasi / Reguler Non-Intensif)
   - **Kartu 3: Ruang NICU** (Neonatal Perawatan Intensif Kritis)
2. **Ketersediaan Boks Bayi Rawat Gabung pada Kamar Ibu**: Sesuai audit master data riil Lantai 3 (kamar `VIP 301` s.d. `317`, `VIP DELUXE 318`, `KELAS II 325` s.d. `329`, dan `KELAS I 330`), kamar-kamar tersebut saat ini hanya memiliki tempat tidur pasien dewasa (`IsForNewborn = false`). Pemilik sistem menetapkan bahwa **hanya sebagian kamar saja yang mendukung boks bayi**, yaitu kamar privat **`VIP 301` sampai `VIP 305` di Lantai 3**.
3. **Penyediaan Unit & Tempat Tidur NICU**: Master kelas `NICU` (`CLS-149`) telah tersedia di database, namun kamar fisik dan tempat tidur inkubator intensif belum didaftarkan di master data. Perlu perencanaan pendaftaran 1 kamar NICU percontohan di Lantai 2 dengan 2 bed inkubator intensif (`IsIntensiveCareBed = true`, `IsForNewborn = true`).
4. **Pembersihan Data Penempatan Uji Coba**: Terdapat sisa transaksi penempatan dan reservasi uji coba sebelumnya yang mengunci kamar menjadi `Occupied` / `Reserved`. Data tersebut perlu dibersihkan (*reset testing state*) sebelum data boks dan NICU baru dimasukkan.

---

## 2. Laporan Asli Pelapor

| No. Laporan | Ringkasan Kalimat Pelapor | Lampiran Terkait |
| :---: | :--- | :--- |
| **1** | PILIH JENIS PERAWATAN BAYI: [ Kartu 1: Rawat Gabung ] [ Kartu 2: Ruang Perinatologi ] [ Kartu 3: Ruang NICU ]. Iya saya setuju dengan tampilan ini. | Diskusi chat 21 Oktober 2026 menyepakati struktur 3 kartu langsung. |
| **2** | Tapi dari pilihan tipe tersebut, apakah kamar bayi sudah tersedia? | Pertanyaan eksplisit verifikasi ketersediaan kamar di database. |
| **3** | Sebentar informasi mengenai kamar mawar dimana ya? Setahu ku tidak ada, hanya lantai 3 berisi informasi ini. | Lampiran: Screenshot denah resmi Lantai 3 RSMMC (berisi kamar `VIP 301` s.d. `317`, `VIP DELUXE 318`, `ODC 319` s.d. `322`, `KELAS II 325` s.d. `329`, `KELAS I 330`). |
| **4** | Tetap di daftarkan di database, tapi sebagian kamar saja yang mendukung support bayi tersebut. | Arahan pembatasan boks rawat gabung hanya pada kamar ibu terpilih (VIP 301 s.d. 305). |
| **5** | Sebelum itu anda clear dulu data yang sudah penempatan kelas kamar, tapi dimasukkan ke dalam planning dulu ya. | Arahan pembersihan data penempatan uji coba via planning gate sebelum penambahan data baru. |

---

## 3. Analisis Per Temuan

### ISS-EPS-031-01 — Struktur Pemilihan Langkah 3 Membingungkan Petugas & Memicu Kesalahan Centang Intensif
- **Akar Masalah**: Pada kode `inpatient-newborn-admission-steps.jsx`, `NEWBORN_CARE_OPTIONS` hanya menyediakan 2 nilai: `RoomingIn` dan `Neonatal`. Untuk membedakan apakah bayi dirawat di Perinatologi umum atau NICU, sistem mengandalkan checkbox terpisah `requiresIntensiveCare`. Petugas admisi sering kali mencentang kotak tersebut secara tidak sengaja karena mengira "neonatal" identik dengan "neonatal intensif", yang berujung pada pemblokiran kamar Perinatologi (`BAYI-1` dan `BAYI-2`) di Langkah 4.
- **Rekomendasi**: Mengganti pilihan radio menjadi 3 kartu pilihan langsung:
  1. `careMode = "RoomingIn"` $\rightarrow$ Rawat gabung di kamar ibu.
  2. `careMode = "Neonatal"`, `requiresIntensiveCare = false` $\rightarrow$ Ruang Perinatologi reguler.
  3. `careMode = "Neonatal"`, `requiresIntensiveCare = true` $\rightarrow$ Ruang NICU intensif.

### ISS-EPS-031-02 — Kamar Ibu di Lantai 3 Belum Memiliki Boks Bayi di Master Data
- **Akar Masalah**: Tabel `public."MstBed"` saat ini hanya memiliki 8 boks bayi bertanda `IsForNewborn = true`, dan seluruhnya berada di kamar `BAYI-1` dan `BAYI-2` (Lantai 2). Kamar rawat inap di Lantai 3 hanya memiliki tempat tidur dewasa (`IsForNewborn = false`). Ketika petugas memilih *Rawat Gabung*, aturan invariant backend `InpNewbornAdmissionRules` menolak penempatan bayi di ranjang dewasa (`NEWBORN_BED_REQUIRED`).
- **Rekomendasi**: Menambahkan 1 boks bayi berstatus `Available` (`BD-RSMMC-BOKS-301` s.d. `305`) pada sebagian kamar ibu terpilih di Lantai 3, yaitu kamar privat **`VIP 301` sampai `VIP 305`**. Kamar lainnya tetap murni untuk pasien dewasa.

### ISS-EPS-031-03 — Ketiadaan Ruangan Fisik dan Bed Inkubator NICU di Master Data
- **Akar Masalah**: Kelas `NICU` (`CLS-149`, ID: `97ff6d1f-fd0d-4a4a-bb3c-ca23add7a0d5`) sudah ada di tabel `public."MstPatientClass"`, tetapi belum ada baris data kamar fisik di `public."MstRoom"` dan bed di `public."MstBed"` yang bertanda `IsIntensiveCareBed = true` dan `IsForNewborn = true`. Jika petugas memilih opsi NICU, pencarian tempat tidur intensif akan selalu kosong.
- **Rekomendasi**: Mendaftarkan 1 kamar percontohan `Ruang NICU` (`RM-RSMMC-NICU01`) di Lantai 2 dengan 2 bed inkubator intensif (`BD-RSMMC-NICU-01` dan `BD-RSMMC-NICU-02`).

### ISS-EPS-031-04 — Kontaminasi Data Penempatan Uji Coba pada Status Bed Master
- **Akar Masalah**: Transaksi pengujian sebelumnya menyisakan catatan penempatan aktif pada `InpBedPlacement` (tanpa `EndDateTime`) dan reservasi sementara di `InpBedReservation`, sehingga beberapa bed di Lantai 2 dan Lantai 3 berstatus `Occupied` (2) atau `Reserved` (3).
- **Rekomendasi**: Menyusun query pembersihan yang aman (*safe state reset*): menutup seluruh penempatan uji (`EndDateTime = NOW()`), menghapus reservasi gantung, dan mengembalikan status tempat tidur master menjadi `1` (`Available`).

---

## 4. Hubungan dengan Dokumen Blueprint

| Dokumen | Bagian Terkait | Keterangan |
| :--- | :--- | :--- |
| `01-business-process.md` | Bagian Alur Admisi Bayi Baru Lahir | Menegaskan pemisahan 3 mode perawatan: Rawat Gabung, Perinatologi, dan NICU. |
| `02-backend-architecture.md` | `InpNewbornAdmissionRules` & `InpBedOccupancyService` | Validasi penempatan boks bayi `IsForNewborn` dan aturan intensif `RequiresNeonatalIntensiveCare`. |
| `03-frontend-architecture.md` | `NewbornCareStep` & `inpatient-newborn-admission-steps.jsx` | Redesain selektor jenis perawatan menjadi 3 kartu pilihan mandiri. |
| `05-skema-tampilan.md` | Langkah 3 Formulir Admisi Bayi | Skema visual 3 kartu pilihan perawatan bayi. |

---

## 5. Tanya-Jawab Pelapor

> **T:** Dari pilihan tipe tersebut, apakah kamar bayi sudah tersedia?  
> **J:** 
> 1. **Ruang Perinatologi:** **Sudah tersedia 100%** di Lantai 2 (Kamar `BAYI-1` dan `BAYI-2` berisi total 8 boks bayi berstatus *Available*).
> 2. **Rawat Gabung:** Kamar ibu di Lantai 3 sudah tersedia, namun boks bayi fisik di dalam kamar ibu belum didaftarkan di master data. Solusinya, kita akan daftarkan 1 boks bayi di kamar `VIP 301` sampai `VIP 305`.
> 3. **Ruang NICU:** Kelasnya sudah ada, namun kamar fisik dan inkubator intensif belum didaftarkan di database. Solusinya, kita akan daftarkan 1 kamar percontohan NICU dengan 2 inkubator di Lantai 2.

> **T:** Informasi mengenai kamar mawar dimana ya? Setahu ku tidak ada, hanya lantai 3 berisi informasi ini (kamar 301 s.d. 330).  
> **J:** Betul sekali. Nama "Mawar" hanyalah nama perumpamaan pada penjelasan sebelumnya. Di master data asli RSMMC Lantai 3, kamar menggunakan penamaan resmi berdasarkan nomor: **`VIP 301` s.d. `317`**, **`VIP DELUXE 318`**, **`ODC 319` s.d. `322`**, **`KELAS II 325` s.d. `329`**, dan **`KELAS I 330`**. Penambahan boks bayi akan dialokasikan khusus pada kamar **`VIP 301` sampai `VIP 305`**.

---

## 6. Temuan Tambahan

- **Integritas Penomoran Bed (`BedNumber`)**: Di kamar yang mendukung rawat gabung (misal `VIP 301`), ranjang dewasa memiliki `BedNumber = 'A'`, sedangkan boks bayi diberi `BedNumber = 'BOKS'`. Ini memudahkan perawat dan sistem denah membedakan mana tempat tidur ibu dan mana boks bayi di kamar yang sama.
- **Isolasi Neonatus**: Kebutuhan isolasi tetap dipertahankan sebagai opsi ortogonal di bawah pemilihan DPJP, sehingga bayi rawat gabung, perinatologi, maupun NICU yang memiliki infeksi menular tetap dapat disaring ke tempat tidur isolasi.

---

## 7. Pertanyaan dan Keputusan yang Dibutuhkan

### Untuk Pemilik Sistem
Seluruh keputusan telah disetujui pada wawancara 21 Oktober 2026:
1. Pemilik menyetujui tampilan 3 kartu pilihan langsung pada Langkah 3.
2. Pemilik menyetujui pendaftaran boks bayi dibatasi pada sebagian kamar saja (`VIP 301` s.d. `VIP 305` di Lantai 3).
3. Pemilik menginstruksikan pembersihan data penempatan uji coba dimasukkan ke dalam perencanaan sebelum eksekusi database.

---

## 8. Catatan Pola

1. **Pemisahan Opsi Eksplisit Mencegah Kesalahan Petugas**: Mengganti kotak centang boolean tersembunyi dengan kartu pilihan radio eksplisit adalah pola terbaik untuk alur klinis yang berdampak langsung pada penempatan pasien (*human-error prevention by design*).
2. **Kepatuhan Planning Gate Database**: Setiap operasi tulis/ubah/hapus data pada database `QuilvianNewDevHamzah` wajib melalui gerbang persetujuan eksplisit agar integritas data referensi tetap terjaga.

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :--- | :--- | :--- |
| 2026-10-21 | Dokumen issue dibuat berdasarkan arahan pemilik sistem terkait penerapan 3 kartu pilihan perawatan bayi, boks sebagian kamar VIP Lantai 3, pembersihan data penempatan uji, dan perencanaan NICU. | `diagnose-module-issue` |
