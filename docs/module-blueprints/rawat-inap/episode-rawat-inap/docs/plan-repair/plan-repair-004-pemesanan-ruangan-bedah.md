# PLAN-REPAIR-004 — Perbaikan Tampilan, Kontrol Input, dan Sinkronisasi Tab Menu Pemesanan Ruangan Bedah (Bedah Operasi & Obgyn)

```yaml
plan_id: PLAN-REPAIR-EPS-004
issue: ../issue/issue-004-pemesanan-ruangan-bedah.md
status_rencana: DISETUJUI
tanggal_rencana: "2026-10-06"
diputuskan_oleh: "Pemilik Sistem / User (07-10-2026)"
tanggal_keputusan: "2026-10-07"
basis_source_backend: "671191eb1aff3f618456cb6c909863bb0eca82f4 (MHamzah)"
basis_source_frontend: "1f889d67cbaaa5c3df5f8d73cfe9cd0b1d434d83 (HamzahV2)"
```

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | :---: | :---: | :---: | --- | --- |
| `FIX-EPS-004-01` | `ISS-EPS-004-01` | Satukan kendali tab pada Tab Bar ruang kerja, hapus tombol ganda di dalam kartu, dan sinkronkan state URL | FE | 1 | `FE-RWI-208` | ✅ SELESAI 2026-10-07 | [Laporan FE-RWI-208](../../task/report/frontend/FE-RWI-208.md), unit test `surgery-booking.test.mjs` (4/4 PASS) |
| `FIX-EPS-004-02` | `ISS-EPS-004-02` | Tata ulang kontrol form memakai token UI Quilvian: radio pill lega, input tanggal-waktu terstandar, dan textarea proporsional | FE | 1 | `FE-RWI-208` | ✅ SELESAI 2026-10-07 | [Laporan FE-RWI-208](../../task/report/frontend/FE-RWI-208.md), `surgery-booking-form.jsx` |
| `FIX-EPS-004-03` | `ISS-EPS-004-03` | Sediakan panel protektif saat order operasi belum dipesan dokter beserta tombol segarkan order | FE | 1 | `FE-RWI-208` | ✅ SELESAI 2026-10-07 | [Laporan FE-RWI-208](../../task/report/frontend/FE-RWI-208.md), guardrail klinis `procedures.length === 0` |
| `FIX-EPS-004-04` | `ISS-EPS-004-04` | Tombol pesan selalu aktif dengan validasi inline dan fokus otomatis ke isian wajib yang kosong | FE | 2 | `FE-RWI-208` | ✅ SELESAI 2026-10-07 | [Laporan FE-RWI-208](../../task/report/frontend/FE-RWI-208.md), validasi interaktif inline |
| `FIX-EPS-004-05` | `ISS-EPS-004-05` | Tambahkan safe padding bawah pada kontainer form pemesanan untuk mencegah tumpang-tindih footer | FE | 1 | `FE-RWI-208` | ✅ SELESAI 2026-10-07 | [Laporan FE-RWI-208](../../task/report/frontend/FE-RWI-208.md), kelas `pb-28` pada section |
| `FIX-EPS-004-06` | `ISS-EPS-004-T1`, `ISS-EPS-004-T2` | Rapikan spasi label dokter operator dan integrasikan tabel riwayat pesanan dengan `BaseButton` | FE | 3 | `FE-RWI-208` | ✅ SELESAI 2026-10-07 | [Laporan FE-RWI-208](../../task/report/frontend/FE-RWI-208.md), `patient-surgery-cases-table.jsx` |

**Ringkasan: 6 dari 6 perbaikan selesai.**

---

## 2. Solusi terpilih per temuan

### ISS-EPS-004-01 — Tab ganda bertumpuk dan desinkronisasi tab antara ruang kerja dan form

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A (Terpilih)** | Jadikan `NursingSecondaryTabBar` sebagai pengendali tunggal, hapus tombol tab manual di header kartu form, lalu tambahkan badge penjelas mode di header kartu | Visual bersih tanpa redundansi, konsisten dengan menu ruang kerja lain (Pengkajian, Asuhan, Prosedur), sinkron dua arah dengan parameter URL `?tab=` | Memerlukan penyelarasan prop dan callback tab di `SurgeryBookingSection` |
| B | Pertahankan kedua set tab dan lakukan sinkronisasi dua arah via state effect | Tab tetap dapat diklik dari dua tempat | Tampilan tetap terlihat bertumpuk dan membingungkan perawat |
| C | Sembunyikan tab sekunder atas khusus untuk menu pemesanan bedah | Hanya ada 1 set tab di dalam kartu | Menyimpang dari pola tata letak seluruh menu ruang kerja keperawatan lainnya |

**Solusi terpilih: Opsi A.**
1. Mengembalikan konsistensi navigasi ruang kerja keperawatan: seluruh sub-tab dikendalikan oleh bilah tab sekunder standar (`NursingSecondaryTabBar`).
2. Menghilangkan redundansi visual tombol kotak bergaris di dalam kartu yang memakan ruang vertikal.
3. Menjamin parameter URL `?section=surgery-booking&tab=general-surgery` atau `tab=obgyn-surgery` selalu akurat dan dapat dibagikan (*shareable link*).

**Kenapa bukan opsi lain:** Opsi B mempertahankan kebingungan visual tab ganda yang dikeluhkan pelapor. Opsi C merusak keseragaman arsitektur shell ruang kerja.

---

### ISS-EPS-004-02 — Kontrol formulir memakai elemen HTML native yang berdesakan dan menyimpang dari katalog komponen

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A (Terpilih)** | Restrukturisasi form ke dalam kelompok kartu logis, ubah pilihan radio bulat menjadi *Radio Pill / Segmented Card* berjarak aman, rapikan input waktu dan textarea dengan styling token Quilvian | Bebas salah klik klinis, tampilan modern dan nyaman untuk perawat di PC bangsal maupun layar sentuh, sesuai token desain | Sedikit menambah struktur markup JSX |
| B | Pertahankan radio button HTML native dan perbesar margin secara manual dengan utility Tailwind | Perubahan kode minimal | Kontrol native tetap terlihat kaku dan tidak konsisten antar browser peramban |

**Solusi terpilih: Opsi A.**
1. Memenuhi prinsip desain keselamatan pasien (*patient safety*): pemilihan Sisi Tubuh (Laterality) dan Jenis Kasus tidak boleh memakai tombol kecil yang berdesakan karena rawan salah klik.
2. Menggunakan radio pill / kartu bersekat dengan status aktif yang tegas (warna biru indigo lembut dan border kontras).
3. Memberikan area ketik textarea minimal 3-4 baris yang nyaman untuk deskripsi indikasi klinis tindakan bedah.

**Kenapa bukan opsi lain:** Opsi B hanya menambal jarak sementara tanpa menyelesaikan masalah inkonsistensi desain platform Quilvian.

---

### ISS-EPS-004-03 — Formulir tetap aktif terbuka meski tindakan operasi belum dipesan dokter

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A (Terpilih)** | Tampilkan **Panel Pemandu Alur Klinis** yang ramah dan sembunyikan isian form pemesanan ketika order dokter belum ada, disertai tombol `[ Segarkan Daftar Order ]` | Perawat tidak bingung mengisi form sia-sia; alur kerja teredukasi bahwa dokter DPJP harus order tindakan di CPPT terlebih dahulu | Form tersembunyi hingga order tersedia |
| B | Form tetap tampil tetapi seluruh input dinonaktifkan (`disabled`) dengan overlay gembok | Perawat tetap bisa melihat struktur isian form | Mengotori layar dengan kolom abu-abu panjang yang tidak dapat diisi |
| C | Izinkan simpan tanpa order dokter | Perawat bisa memesan lebih dulu | **Dilarang keras**: Melanggar invariant domain klinis `INV-RWF-25` dan `RWI-DEC-176` |

**Solusi terpilih: Opsi A.**
1. Mencegah perawat membuang waktu mengetik data operasi sebelum order tindakan dokter terbit di rekam medis elektronik.
2. Memberikan tombol aksi langsung bagi perawat untuk menyegarkan data seketika setelah dokter selesai memesan tindakan di CPPT bangsal.

**Kenapa bukan opsi lain:** Opsi B tetap menyajikan formulir panjang yang buntu dan menabrak footer. Opsi C melanggar integritas klinis rumah sakit.

---

### ISS-EPS-004-04 — Tombol "Pesan Ruang Bedah" terkunci mati tanpa indikasi isian yang kurang

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A (Terpilih)** | Tombol selalu aktif (kecuali saat pengiriman jaringan), lakukan validasi interaktif saat ditekan, tampilkan pesan error merah di bawah isian yang kurang, dan fokuskan kursor ke isian tersebut | Pengguna langsung paham apa yang kurang; mematuhi aturan platform UI Quilvian butir 7.3 | Perlu menambahkan state objek penampung error validasi form |
| B | Tetap mematikan tombol, tetapi beri tooltip teks saat kursor diarahkan ke tombol | Tidak mengubah logika tombol | Tooltip tidak terbaca di perangkat layar sentuh dan menyulitkan perawat |

**Solusi terpilih: Opsi A.**
1. Mengikuti aturan wajib `rules/frontend/ui-consistency-checklist.md`: tombol aksi utama tidak boleh mati secara membisu.
2. Memberikan panduan visual langsung pada kolom yang belum valid (misal: "Indikasi klinis operasi wajib diisi").

---

### ISS-EPS-004-05 — Bilah footer aplikasi menabrak dan menutupi bagian bawah formulir pemesanan

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A (Terpilih)** | Tambahkan padding pengaman bawah (`pb-32` / ruang bebas footer) pada pembungkus kartu pemesanan dan kontainer section | Form dapat digulir penuh, tombol simpan dan identitas penginput selalu berada di atas bilah footer | Menambah sedikit jarak kosong di akhir halaman |
| B | Sembunyikan footer aplikasi pada halaman ruang kerja keperawatan | Footer tidak akan pernah menutupi konten | Menghilangkan informasi versi sistem dan tautan hak cipta aplikasi |

**Solusi terpilih: Opsi A.**
1. Menjaga integritas layout global aplikasi sekaligus memastikan konten formulir tidak terpotong atau tertabrak footer.

---

### ISS-EPS-004-06 — Perbaikan tipografi dokter operator dan tabel riwayat kasus

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A (Terpilih)** | Berikan spasi pemisah dan format pill pada info dokter operator, serta integrasikan tombol `BaseButton` pada tabel riwayat pesanan | Visual rapi, bebas teks cacat, konsisten dengan base component | Perubahan kecil di dua file |

---

## 3. Skema tampilan sebelum → sesudah

### SEBELUM (Keadaan saat ini sesuai tangkapan layar pelapor)

```text
┌────────────────────────────────────────────────────────────────────────┐
│ [ Bedah Operasi (aktif) ]    [ Bedah Obgyn ]                           │  ← Tab sekunder atas
├────────────────────────────────────────────────────────────────────────┤
│ Pemesanan Ruangan Bedah                                                │
│ Budi Santoso · RM 00-00-00-17 · Rawat Inap                             │
│                                                                        │
│ [ Bedah Operasi ] [ Bedah Obgyn (SC/Caesar) ]                          │  ← Tombol tab ganda bertumpuk!
│ ────────────────────────────────────────────────────────────────────── │
│ Tindakan Operasi *                                                     │
│ ┌────────────────────────────────────────────────────────────────────┐ │
│ │ ℹ Tindakan operasi belum dipesan dokter.                           │ │  ← Order belum ada, tapi form
│ │   Minta dokter memesan tindakan lebih dulu melalui CPPT / order.   │ │    di bawahnya tetap aktif!
│ └────────────────────────────────────────────────────────────────────┘ │
│ Dokter Operator                                                        │
│ 👤 -(dari order, tidak dapat diubah)                                   │  ← Teks berdempetan tanpa spasi
│ Perkiraan Tarif Tindakan                                               │
│ Pilih tindakan operasi untuk melihat tarif                             │
│                                                                        │
│ Tanggal & Jam Operasi *   [ 06/10/2026 19:00 📅 ]                      │  ← Native datetime-local
│ Rencana Anestesi *        [ Umum (General) ▾ ]                         │  ← Native select
│ Prioritas *               [ Rutin ▾ ]                                  │  ← Native select
│ Jenis Kasus *             (•)Elektif ( )Darurat                        │  ← Radio rapat berdesakan
│ Sisi Tubuh (Laterality)   (•)Tidak berlaku ( )Kiri ( )Kanan ( )Bilateral │  ← Radio sangat rapat
│ Perkiraan Durasi Operasi  [ 60 ] menit (1-1440)                        │  ← Input terpisah kaku
│ Indikasi Operasi *        [ textarea rows=2                          ] │  ← Sangat sempit
│ Keterangan / Catatan      [ textarea rows=2                          ] │
│ ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │  ← Bilah footer menabrak &
│ ░ [Terms of Use]      Quilvian System Version 2.2.0    [Pesan Ruang] ░ │    menutupi tombol submit!
└────────────────────────────────────────────────────────────────────────┘
```

---

### SESUDAH — Skenario 1: Belum Ada Order Tindakan Dokter (Keadaan pada Screenshot)

```text
┌────────────────────────────────────────────────────────────────────────┐
│ [ Bedah Operasi (aktif) ]    [ Bedah Obgyn ]                           │  ← Pengendali tunggal tab
├────────────────────────────────────────────────────────────────────────┤
│ 🏥 Formulir Pemesanan Kamar Operasi Bedah                              │
│ Budi Santoso · RM 00-00-00-17 · Melati 302/2 (Rawat Inap)              │
│ Mode Aktif: [ Badge: Bedah Operasi Umum ]                              │  ← Indikator mode elegan
│ ────────────────────────────────────────────────────────────────────── │
│                                                                        │
│ ┌────────────────────────────────────────────────────────────────────┐ │
│ │ ℹ BELUM ADA ORDER TINDAKAN OPERASI DARI DOKTER DPJP                │ │
│ │                                                                    │ │
│ │ Pasien ini belum memiliki order tindakan bedah aktif yang dipesan  │ │
│ │ oleh dokter penanggung jawab pelayanan (DPJP).                     │ │
│ │                                                                    │ │
│ │ Alur Klinis:                                                       │ │
│ │ 1. Dokter DPJP/Operator membuat order tindakan klinis via CPPT     │ │
│ │    atau Formulir Permintaan Prosedur Bedah.                        │ │
│ │ 2. Setelah order diterbitkan, klik tombol di bawah untuk membuka    │ │
│ │    pengisian jadwal dan rincian pemesanan kamar operasi.           │ │
│ │                                                                    │ │
│ │ [ 🔄 Segarkan Daftar Order Tindakan ]                              │ │
│ └────────────────────────────────────────────────────────────────────┘ │
│                                                                        │
│ ────────────────────────────────────────────────────────────────────── │
│ 📋 Pesanan Ruang Bedah Pasien Ini               [ 🔄 Muat Ulang Data ] │
│ ┌──────────────┬──────────────────┬──────────────┬───────────────────┐ │
│ │ No. Kasus    │ Tindakan         │ Status       │ Keterangan / Aksi │ │
│ ├──────────────┼──────────────────┼──────────────┼───────────────────┤ │
│ │ (Belum ada riwayat pemesanan kamar bedah untuk kunjungan ini)      │ │
│ └──────────────┴──────────────────┴──────────────┴───────────────────┘ │
│                                                                        │
│                                                                        │  ← Safe padding bawah luas
├────────────────────────────────────────────────────────────────────────┤
│ Terms of Use · Privacy Policy      Quilvian System v2.2.0 © 2026       │  ← Footer bebas tabrakan
└────────────────────────────────────────────────────────────────────────┘
```

---

### SESUDAH — Skenario 2: Order Tindakan Dokter Tersedia (Formulir Terbuka Lengkap)

```text
┌────────────────────────────────────────────────────────────────────────┐
│ [ Bedah Operasi (aktif) ]    [ Bedah Obgyn ]                           │  ← Tab sekunder ruang kerja
├────────────────────────────────────────────────────────────────────────┤
│ 🏥 Pemesanan Ruangan Bedah                                             │
│ Budi Santoso · RM 00-00-00-17 · Melati 302/2                           │
│ Mode Layanan: [ Badge: Bedah Operasi Umum ]                            │
│ ────────────────────────────────────────────────────────────────────── │
│                                                                        │
│ 1. RUJUKAN ORDER KLINIS & DOKTER OPERATOR                              │
│ Tindakan Operasi Terjadwal *                                           │
│ [ Appendektomi — dr. Hendra, Sp.B (Order: 06/10/2026)                ▾ ]│
│                                                                        │
│ ┌──────────────────────────────────┬─────────────────────────────────┐ │
│ │ Dokter Operator                  │ Perkiraan Biaya Tindakan        │ │
│ │ 👨‍⚕️ dr. Hendra, Sp.B              │ 🏷️ Ditanggung BPJS              │ │
│ │ (Ditentukan dari order klinis)   │ Rp 8.500.000 (estimasi tarif)   │ │
│ └──────────────────────────────────┴─────────────────────────────────┘ │
│                                                                        │
│ 2. JADWAL PELAKSANAAN & SPESIFIKASI PROSEDUR                           │
│ ┌──────────────────────────────────┬─────────────────────────────────┐ │
│ │ Tanggal & Jam Rencana Operasi *  │ Rencana Anestesi *              │ │
│ │ [ 06/10/2026 19:00             ] │ [ Umum (General Anesthesia)   ▾]│ │
│ ├──────────────────────────────────┼─────────────────────────────────┤ │
│ │ Prioritas Prosedur *             │ Perkiraan Durasi Tindakan *     │ │
│ │ [ Rutin (Terjadwal)            ▾]│ [ 60             ] menit        │ │
│ └──────────────────────────────────┴─────────────────────────────────┘ │
│                                                                        │
│ 3. KARAKTERISTIK KLINIS TINDAKAN                                       │
│ Jenis Kasus *                                                          │
│ [• Elektif (Terencana)   ]   [  Darurat (Emergency) ]                  │  ← Radio Pill lega berjarak
│ *(Pada tab Bedah Obgyn, otomatis terkunci ke "Obstetri (SC/Caesar)")   │
│                                                                        │
│ Sisi Tubuh / Laterality *                                              │
│ [• Tidak Berlaku ]  [  Sisi Kiri ]  [  Sisi Kanan ]  [  Bilateral ]    │  ← Radio Pill aman salah klik
│                                                                        │
│ 4. INDIKASI KLINIS & CATATAN PERSIAPAN                                 │
│ Indikasi Operasi Bedah *                                               │
│ ┌────────────────────────────────────────────────────────────────────┐ │
│ │ Appendicitis akut perforasi dengan tanda peritonitis lokal...      │ │  ← Textarea 3 baris lega
│ └────────────────────────────────────────────────────────────────────┘ │
│ Catatan Khusus / Permintaan Alat                                       │
│ ┌────────────────────────────────────────────────────────────────────┐ │
│ │ Pasien telah puasa sejak pukul 12.00, siapkan set laparoskopi...   │ │
│ └────────────────────────────────────────────────────────────────────┘ │
│                                                                        │
│ ────────────────────────────────────────────────────────────────────── │
│ 👤 Penginput: Ns. Siti Aminah (Perawat Bangsal Melati)                 │
│                                            [ 📤 Pesan Kamar Bedah ]    │  ← Selalu aktif & interaktif
│                                                                        │
│                                                                        │  ← Safe padding bawah luas
├────────────────────────────────────────────────────────────────────────┤
│ Terms of Use · Privacy Policy      Quilvian System v2.2.0 © 2026       │  ← Footer bebas tabrakan
└────────────────────────────────────────────────────────────────────────┘
```

---

### Tabel Spesifikasi Wilayah Antarmuka

| Wilayah | Elemen yang Ditampilkan | Sumber Data & Komponen | Perilaku Responsif |
| --- | --- | --- | --- |
| **Bilah Tab Utama** | Sub-tab Bedah Operasi dan Bedah Obgyn | `NursingSecondaryTabBar` via URL query | Otomatis menyesuaikan tab aktif dan merubah context form |
| **Header Formulir** | Judul, nama pasien, RM, kamar/bed, badge mode aktif | Data episode rawat inap | Tersemat rapi di bagian atas kartu |
| **Panel Prasyarat Order** | Banner edukasi klinis jika order dokter belum ada, tombol segarkan | `getPatientProcedures` (panjang list = 0) | Menggantikan formulir bila belum ada order aktif |
| **Kartu Order & Operator** | Dropdown tindakan, nama dokter operator, estimasi biaya tindakan | `getPatientProcedures` | Grid 2-kolom pada desktop, 1-kolom pada tablet/ponsel |
| **Kartu Jadwal & Anestesi** | Pemilih tanggal-jam, dropdown anestesi, prioritas, durasi | State lokal formulir | Input terformat rapi dengan placeholder jelas |
| **Kartu Karakteristik** | Pilihan Jenis Kasus dan Sisi Tubuh (Laterality) | State lokal pilihan radio pill | Radio pill dengan padding lega dan indikator aktif warna indigo |
| **Kartu Indikasi & Catatan** | Textarea indikasi wajib dan catatan persiapan | Form state | Tinggi 3 baris dengan penghitung sisa karakter |
| **Bilah Aksi & Penginput** | Nama pengguna login, tombol submit pemesanan | Auth slice & `BaseButton` | Posisi di ujung kanan bawah dengan bantalan margin bebas footer |
| **Tabel Riwayat Kasus** | Daftar kasus terdaftar, status, tombol aksi | `PatientSurgeryCasesTable` | Terletak di bawah kartu form dengan pemisah yang proporsional |

---

### Tabel Tombol dan Aksi

| Label Tombol | Varian / Jenis | Kapan Aktif | Aksi yang Terjadi Saat Diklik |
| --- | --- | --- | --- |
| **Segarkan Daftar Order** | `secondary` / outline | Saat order dokter masih kosong | Memanggil ulang `loadProcedures()` untuk memeriksa apakah order sudah terbit di CPPT |
| **Pesan Kamar Bedah** | `primary` (indigo) | Selalu aktif (kecuali sedang memproses jaringan) | Jika data lengkap: kirim `POST /surgery-bookings`. Jika data belum lengkap: tampilkan peringatan merah dan scroll ke isian kosong |
| **Muat Ulang Data (Tabel)** | `secondary` kecil | Selalu aktif | Memperbarui daftar riwayat kasus kamar bedah pasien |

---

### Tabel Keadaan Formulir (States)

| Keadaan | Tampilan di Layar | Aksi Pengguna |
| --- | --- | --- |
| **Memuat Order (Loading)** | Skeleton baris dan teks "Memeriksa order tindakan operasi dari dokter..." | Menunggu proses pembacaan selesai |
| **Order Belum Ada (Empty)** | Banner informatif alur klinis + tombol Segarkan Daftar Order | Menghubungi dokter DPJP atau klik segarkan jika order sudah dibuat |
| **Formulir Siap Diisi** | Seluruh kartu isian terbuka dengan nilai default cerdas | Mengisi jadwal, anestesi, laterality, durasi, dan indikasi |
| **Validasi Belum Lengkap** | Border isian yang kosong berubah merah disertai pesan di bawah ruas | Memperbaiki isian yang ditandai merah |
| **Memproses (Submitting)** | Tombol bertuliskan "Memproses Pemesanan..." dengan spinner dan status nonaktif | Menunggu respon konfirmasi nomor kasus dari server |
| **Berhasil Tersimpan** | Banner hijau sukses dengan nomor kasus OK (misal OK-2026-0012) dan form tereset | Tabel riwayat kasus di bawah otomatis tersegarkan |
| **Pasien Discharge Pending** | Spanduk peringatan kuning: pemesanan hanya untuk pasien yang sedang dirawat aktif | Formulir terkunci sesuai aturan keselamatan pasien |

---

## 4. Rincian perbaikan

### FIX-EPS-004-01 — Sinkronisasi Tab Navigasi Bedah Operasi & Obgyn dan Eliminasi Tab Ganda Bertumpuk

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-004-01` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (`surgery-booking-form.jsx`, `surgery-booking-section.jsx`) |
| **Berkas yang diubah** | `src/components/view/health-services/inpatient-management/nursing-workspace/sections/surgery-booking/surgery-booking-form.jsx`<br>`src/components/view/health-services/inpatient-management/nursing-workspace/sections/surgery-booking/surgery-booking-section.jsx` |
| **Radius dampak** | Menu Pemesanan Ruangan Bedah rawat inap saja |
| **Bergantung pada** | Tidak ada |

**Langkah.**
1. Di `surgery-booking-section.jsx`, tambahkan sinkronisasi `useEffect` agar state tab mengikuti perubahan prop `activeTab` dari bilah tab sekunder:
```jsx
useEffect(() => {
  if (activeTab) {
    setCurrentTab(activeTab);
  }
}, [activeTab]);
```
2. Di `surgery-booking-form.jsx`, hapus blok tombol tab manual (baris 260-283).
3. Gantikan area bekas tombol tab dengan **Badge Mode Layanan** yang informatif:
   - Jika tab `general-surgery`: Tampilkan badge biru `Layanan: Bedah Operasi Umum`.
   - Jika tab `obgyn-surgery`: Tampilkan badge ungu `Layanan: Bedah Obgyn (Obstetri)`.
4. Pastikan ketika `handleRebook` dipanggil pada tabel kasus, tab diarahkan melalui pembaruan router/state yang konsisten.

**Acceptance criteria.**
1. Pada menu Pemesanan Ruangan Bedah, hanya ada **satu baris pemilih tab** yaitu tab sekunder di atas kartu (`Bedah Operasi` dan `Bedah Obgyn`).
2. Mengklik tab "Bedah Obgyn" di bilah atas seketika mengubah mode formulir menjadi Bedah Obgyn (jenis kasus otomatis Obstetri dan badge header berubah menjadi Bedah Obgyn).
3. Mengklik tab "Bedah Operasi" di bilah atas mengembalikan formulir ke mode Bedah Operasi (pilihan jenis kasus Elektif/Darurat aktif).

**Verifikasi.** Jalankan pengujian interaksi pergantian tab di peramban dan pastikan URL query `?tab=` selaras dengan mode formulir yang aktif.

**Risiko.** Tombol Pesan Ulang (`handleRebook`) pada tabel riwayat perlu dipastikan tetap dapat memicu peralihan tab ke obgyn jika kasus yang dipesan ulang berjenis Obstetri.

---

### FIX-EPS-004-02 — Tata Ulang Kontrol Form Pemesanan Ruang Bedah Menggunakan Quilvian UI & Base Component

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-004-02` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (`surgery-booking-form.jsx`) |
| **Berkas yang diubah** | `src/components/view/health-services/inpatient-management/nursing-workspace/sections/surgery-booking/surgery-booking-form.jsx` |
| **Radius dampak** | Tampilan formulir pemesanan kamar bedah |
| **Bergantung pada** | `FIX-EPS-004-01` |

**Langkah.**
1. Ganti pilihan radio native pada **Jenis Kasus** (`caseType`) dan **Sisi Tubuh** (`laterality`) menjadi tombol pill/card tersegmen dengan padding minimum 10px dan border yang kontras saat terpilih.
2. Tata field dalam grid 2-kolom responsif dengan judul segmen yang jelas:
   - Baris 1: Rujukan Order Tindakan Dokter & Dokter Operator / Tarif
   - Baris 2: Tanggal & Waktu Operasi dan Rencana Anestesi
   - Baris 3: Prioritas Kasus dan Perkiraan Durasi Operasi
   - Baris 4: Jenis Kasus dan Sisi Tubuh (Laterality)
   - Baris 5: Indikasi Operasi (textarea minimal `rows={3}`)
   - Baris 6: Keterangan / Catatan Persiapan (textarea `rows={2}`)
3. Pada tab `obgyn-surgery`, kunci `caseType` ke nilai `Obstetric` dengan visual kartu informatif yang elegan, bukan kotak abu-abu kaku.

**Acceptance criteria.**
1. Pilihan radio Sisi Tubuh (Tidak berlaku, Kiri, Kanan, Bilateral) tampil sebagai tombol pill lebar yang mudah diklik tanpa saling bertumpukan.
2. Pilihan Jenis Kasus (Elektif vs Darurat) pada tab Bedah Operasi memiliki ukuran sentuh yang lega dan nyaman.
3. Input perkiraan durasi operasi tampil menyatu dengan label "menit" di dalam satu grup kontrol yang rapi.
4. Textarea indikasi operasi memiliki tinggi yang memadai (minimal 3 baris) dan tidak berdesakan dengan textarea catatan khusus.

**Verifikasi.** Uji tampilan form pada berbagai ukuran resolusi layar (desktop 1920x1080 dan laptop 1366x768).

---

### FIX-EPS-004-03 — Tampilan Protektif Empty State Saat Tindakan Operasi Belum Dipesan Dokter

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-004-03` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (`surgery-booking-form.jsx`) |
| **Berkas yang diubah** | `src/components/view/health-services/inpatient-management/nursing-workspace/sections/surgery-booking/surgery-booking-form.jsx` |
| **Radius dampak** | Penanganan state ketika pasien belum punya order tindakan bedah aktif |
| **Bergantung pada** | `FIX-EPS-004-01` |

**Langkah.**
1. Ketika `procedures.length === 0` dan tidak sedang loading:
   - Sembunyikan elemen isian formulir di bawahnya (Dokter, Jadwal, Anestesi, Durasi, Indikasi, Tombol Pesan).
   - Tampilkan **Kartu Panduan Alur Klinis** dengan ikon informasi, penjelasan alur pembuatan order oleh DPJP, dan tombol aksi `[ 🔄 Segarkan Daftar Order ]`.
2. Ketika tombol `[ 🔄 Segarkan Daftar Order ]` ditekan, picu fungsi `loadProcedures()` dan tampilkan indikator loading yang mulus.
3. Jika dokter telah selesai membuat order di CPPT, formulir otomatis terbuka dengan dropdown order terisi dan ter-auto-select bila hanya ada 1 order.

**Acceptance criteria.**
1. Saat pasien belum memiliki order tindakan operasi aktif, layar tidak menampilkan kolom-kolom isian kosong yang buntu.
2. Tampil panduan edukatif yang menjelaskan bahwa dokter DPJP harus memesan tindakan terlebih dahulu di CPPT.
3. Tombol "Segarkan Daftar Order" berfungsi mengambil data terbaru dari server tanpa perlu me-refresh seluruh halaman peramban.

**Verifikasi.** Buka pasien rawat inap yang belum memiliki order bedah, amati tampilan bersih yang memandu pengguna.

---

### FIX-EPS-004-04 — Validasi Interaktif Tombol Pesan Ruang Bedah Tanpa Mematikan Tombol Secara Bisu

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-004-04` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (`surgery-booking-form.jsx`) |
| **Berkas yang diubah** | `src/components/view/health-services/inpatient-management/nursing-workspace/sections/surgery-booking/surgery-booking-form.jsx` |
| **Radius dampak** | Alur interaksi pengiriman formulir pemesanan |
| **Bergantung pada** | `FIX-EPS-004-02` |

**Langkah.**
1. Ubah properti tombol `disabled={submitting}` (hanya mati ketika permintaan jaringan sedang berlangsung).
2. Di dalam handler `handleSubmit`:
   - Lakukan pemeriksaan kelengkapan ruas wajib (`selectedProcedureId`, `preferredAt`, `plannedAnesthesiaType`, `priority`, `caseType`, `indication.trim()`, `estimatedMinutes`).
   - Simpan galat pada state `formErrors` (misal: `{ indication: "Indikasi operasi wajib diisi", preferredAt: "Waktu operasi wajib diisi" }`).
   - Tampilkan pesan error berwarna merah tepat di bawah masing-masing ruas yang bermasalah.
   - Gulirkan layar ke ruas pertama yang belum lengkap.

**Acceptance criteria.**
1. Tombol "Pesan Ruang Bedah" selalu dapat diklik oleh pengguna yang memiliki wewenang `OperatingRoomCase:Create`.
2. Jika ada ruas wajib yang terlewat (misal Indikasi Operasi kosong), menekan tombol akan memunculkan tulisan peringatan merah di bawah ruas yang bersangkutan dan tidak mengirim data ke server.
3. Tombol hanya dalam kondisi loading (`aria-busy="true"`) saat request API sedang berjalan.

**Verifikasi.** Kosongkan indikasi operasi, klik tombol "Pesan Ruang Bedah", pastikan muncul pesan validasi inline dan kursor fokus ke kolom indikasi.

---

### FIX-EPS-004-05 — Penyesuaian Bottom Safe-Padding Kontainer Form Terhadap Footer Aplikasi

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-004-05` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (`surgery-booking-section.jsx`, `surgery-booking-form.jsx`) |
| **Berkas yang diubah** | `src/components/view/health-services/inpatient-management/nursing-workspace/sections/surgery-booking/surgery-booking-section.jsx` |
| **Radius dampak** | Tata letak vertikal menu pemesanan ruang bedah |
| **Bergantung pada** | `FIX-EPS-004-01` |

**Langkah.**
1. Tambahkan margin / padding bawah yang aman pada pembungkus utama `SurgeryBookingSection`:
   `className="space-y-6 pb-28"` atau manfaatkan variabel CSS `--app-footer-safe-space`.
2. Pastikan footer form pemesanan dan tabel riwayat kasus memiliki jarak pandang minimum 80px dari batas bawah jendela peramban saat digulir maksimal.

**Acceptance criteria.**
1. Ketika formulir digulir sampai paling bawah, tombol "Pesan Ruang Bedah" dan teks penginput berada sepenuhnya di atas bilah footer aplikasi tanpa ada teks atau tombol yang terpotong.
2. Tidak ada elemen antarmuka yang tertutup oleh banner toska footer.

**Verifikasi.** Buka menu pada resolusi laptop (1366x768), scroll ke bawah, pastikan footer melayang tidak menimpa elemen form apa pun.

---

### FIX-EPS-004-06 — Perbaikan Tipografi Dokter Operator dan Integrasi Visual Tabel Riwayat Kasus

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-004-T1`, `ISS-EPS-004-T2` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (`surgery-booking-form.jsx`, `patient-surgery-cases-table.jsx`) |
| **Berkas yang diubah** | `src/components/view/health-services/inpatient-management/nursing-workspace/sections/surgery-booking/surgery-booking-form.jsx`<br>`src/components/view/health-services/inpatient-management/nursing-workspace/sections/surgery-booking/patient-surgery-cases-table.jsx` |
| **Radius dampak** | Komponen dokter operator dan tabel riwayat kasus |
| **Bergantung pada** | `FIX-EPS-004-02` |

**Langkah.**
1. Di `surgery-booking-form.jsx:362-364`, pisahkan tanda minus dan teks bantuan:
```jsx
<span>{operatorDoctorName}</span>
<span className="text-[11px] text-slate-400 font-normal italic ml-1">
  (dari order klinis, tidak dapat diubah)
</span>
```
2. Di `patient-surgery-cases-table.jsx`, perbarui tombol "Muat ulang" menggunakan komponen standar `BaseButton` (`variant="secondary"`, `size="sm"`).

**Acceptance criteria.**
1. Ketika dokter operator belum dipilih, tampil `- (dari order klinis, tidak dapat diubah)` dengan spasi yang benar.
2. Tombol muat ulang pada tabel riwayat kasus menggunakan `BaseButton` standar Quilvian.

---

## 5. Urutan pengerjaan

```text
FIX-EPS-004-01 (Sinkronisasi Tab Ruang Kerja & Eliminasi Tab Ganda)
├── FIX-EPS-004-02 (Tata Ulang Kontrol Form & Radio Pills)
│   ├── FIX-EPS-004-04 (Validasi Interaktif Tombol Submit)
│   └── FIX-EPS-004-06 (Perbaikan Tipografi Operator & Tabel Riwayat)
├── FIX-EPS-004-03 (Protektif Empty State Saat Belum Ada Order Dokter)
└── FIX-EPS-004-05 (Bottom Safe Padding Terhadap Footer)
```

Urutan eksekusi implementasi:
1. Gelombang 1: `FIX-EPS-004-01`, `FIX-EPS-004-05` (Perbaikan struktur navigasi tab dan tata letak footer)
2. Gelombang 2: `FIX-EPS-004-02`, `FIX-EPS-004-03` (Penyempurnaan kontrol formulir dan penanganan kondisi order kosong)
3. Gelombang 3: `FIX-EPS-004-04`, `FIX-EPS-004-06` (Validasi interaktif tombol pesan dan pemolesan komponen tabel riwayat)

---

## 6. Keputusan pemilik yang dibutuhkan

| No | Keputusan | Pilihan | Rekomendasi | Status Keputusan |
| ---: | --- | --- | --- | --- |
| K-01 | Pengendalian Tab: Apakah tab "Bedah Operasi" dan "Bedah Obgyn" cukup dikendalikan oleh Secondary Tab Bar ruang kerja, atau tetap butuh penanda di dalam kartu? | (A) Kendali tunggal di Tab Bar atas + badge penjelas mode di dalam kartu / (B) Pertahankan tombol tab di dalam kartu dan sembunyikan tab bar atas | **Pilihan A**: Sesuai konsistensi navigasi ruang kerja keperawatan Quilvian | ✅ **DISETUJUI (Pilihan A)** oleh pemilik pada 07-10-2026 |
| K-02 | Perilaku Saat Order Operasi Belum Dipesan Dokter: Apakah formulir disembunyikan sepenuhnya atau ditampilkan dalam keadaan terkunci (read-only)? | (A) Tampilkan panel panduan alur klinis dan sembunyikan formulir isian / (B) Tetap tampilkan isian tetapi seluruhnya dinonaktifkan (disabled) | **Pilihan A**: Lebih bersih, mencegah perawat salah paham, dan langsung mengarahkan perawat untuk mengingatkan dokter DPJP | ✅ **DISETUJUI (Pilihan A)** oleh pemilik pada 07-10-2026 |

---

## 7. Dokumen hulu yang ikut direvisi

| Dokumen | Bagian | Sekarang | Menjadi |
| --- | --- | --- | --- |
| `03-frontend-architecture.md` | Bagian 13.4.1 Rangka ASCII `FE-INP-25` | Rangka menampilkan dua tombol tab di dalam kartu di bawah judul | Rangka menampilkan kendali tab pada secondary tab bar ruang kerja dan badge mode aktif di header kartu |
| `03-frontend-architecture.md` | Bagian 13.4.1 Tabel Wilayah | "Kosong → Tindakan operasi belum dipesan dokter. Minta dokter memesan tindakan lebih dulu. dan tombol Pesan nonaktif" | "Kosong → Tampilkan panel panduan alur klinis dan sembunyikan form isian hingga order tindakan terbit; tombol Segarkan Order aktif" |

---

## 8. Verifikasi menyeluruh

Setelah seluruh perbaikan selesai diimplementasikan oleh builder, jalankan pengujian berikut:

1. **Uji Kasus 1: Navigasi dan Alih Tab**
   - Buka menu Pemesanan Ruangan Bedah pada ruang kerja keperawatan.
   - Klik tab **Bedah Obgyn** pada bilah tab atas → Pastikan badge mode berubah menjadi Bedah Obgyn, isian Jenis Kasus terkunci ke "Obstetri (SC / Caesar)".
   - Klik tab **Bedah Operasi** pada bilah tab atas → Pastikan badge mode berubah menjadi Bedah Operasi, isian Jenis Kasus menawarkan pilihan Elektif dan Darurat.
2. **Uji Kasus 2: Pasien Tanpa Order Tindakan Dokter**
   - Buka pasien yang belum memiliki order tindakan bedah dari DPJP.
   - Pastikan layar menyajikan panel panduan alur klinis yang ramah dan tidak menampilkan formulir kosong yang membingungkan.
   - Klik tombol "Segarkan Daftar Order" → Pastikan fungsi pemuatan ulang terpanggil dengan lancar.
3. **Uji Kasus 3: Pasien Dengan Order Tindakan Dokter**
   - Buka pasien yang memiliki order tindakan bedah terjadwal.
   - Pastikan form terbuka lengkap dengan dropdown tindakan otomatis memilih order tersebut.
   - Pastikan radio pill Jenis Kasus dan Sisi Tubuh (Laterality) lega dan dapat dipilih dengan lancar.
   - Pastikan textarea indikasi dan catatan luas dan mudah diketik.
4. **Uji Kasus 4: Validasi Interaktif**
   - Kosongkan isian indikasi operasi, lalu tekan tombol "Pesan Ruang Bedah".
   - Pastikan muncul pesan error merah di bawah kolom indikasi dan halaman tidak mengirim request invalid ke server.
5. **Uji Kasus 5: Bebas Tabrakan Footer**
   - Gulirkan halaman hingga bagian paling bawah.
   - Pastikan tombol simpan dan keterangan penginput terlihat jelas dan tidak tertutup bilah footer aplikasi pada semua ukuran layar peramban.

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-06 | Rencana perbaikan dibuat dengan 6 butir perbaikan terstruktur, menunggu persetujuan pemilik | `diagnose-module-issue` |
| 2026-10-07 | Seluruh rekomendasi K-01 dan K-02 disetujui pemilik (Pilihan A); status rencana menjadi `DISETUJUI`; didaftarkan sebagai task `FE-RWI-208` | Pemilik sistem / User |
