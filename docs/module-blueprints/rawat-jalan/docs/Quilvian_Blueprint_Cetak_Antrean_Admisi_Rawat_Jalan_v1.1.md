# BLUEPRINT CETAK ANTREAN KE ADMISI RAWAT JALAN - QUILVIAN

**Versi 1.1 (revisi cakupan)** | **28 Oktober 2026** | **Status:** baseline proses bisnis sampai registrasi kunjungan dan pencatatan nomor antrean poli internal; bukan audit implementasi final.

## 1. Ringkasan keputusan yang disepakati (revisi)

- Fitur **Cetak Antrean** pada kiosk hanya untuk pasien **reguler** yang datang langsung dan membutuhkan pelayanan **Admisi Rawat Jalan**.
- Nomor yang **dicetak dan diserahkan kepada pasien** adalah **nomor antrean admisi** (contoh `A-015`), bukan antrean poliklinik.
- MVP hanya menggunakan **tiket kertas thermal**; pengiriman melalui WhatsApp tidak masuk cakupan.
- Pasien tidak perlu menginput NIK, No. RM, nama, nomor telepon, jenis penjamin, poliklinik, atau dokter saat mengambil tiket admisi.
- **Daftar Antrean Admisi** berbeda dari **Daftar Pasien Rawat Jalan** dan antrean internal pada **Skrining Pasien**.
- Petugas admisi memanggil tiket, mengidentifikasi pasien, memeriksa penjamin dan persyaratan, kemudian **menyimpan registrasi kunjungan rawat jalan**.
- **Sesudah registrasi kunjungan berhasil, sistem membentuk/mencatat nomor antrean poliklinik internal** sesuai proses antrean skrining existing. Contoh `PD-012` adalah **contoh ilustratif**, bukan format wajib.
- **Nomor antrean poliklinik tidak dicetak, tidak diserahkan, dan tidak perlu ditunjukkan kepada pasien**. Nomor tersebut tercatat pada sistem sebagai dasar urutan daftar kerja Skrining Pasien/poliklinik.
- Pasien baru pada poli yang sama masuk ke **posisi paling bawah dalam daftar pasien yang masih menunggu**, berdasarkan urutan antrean internal/waktu siap skrining menurut aturan existing; pasien yang sudah lebih dahulu menunggu tetap di atas.
- **Batas akhir blueprint** adalah keadaan di mana transaksi kunjungan berhasil tersimpan, antrean poliklinik internal berhasil terbentuk/tercatat, dan data siap dibaca oleh proses Skrining Pasien existing. **Pemanggilan skrining, pengisian tanda vital, asesmen, hingga pelayanan dokter berada di luar cakupan**.
- Membership dan jalur pelayanan prioritas tidak menggunakan kiosk reguler ini. Kegawatan klinis ditangani langsung mengikuti SOP rumah sakit.
- Rumah sakit boleh menggabungkan atau memisahkan kelompok loket admisi berdasarkan penjamin; **default satu kelompok antrean admisi reguler**.

## 2. Tujuan, aktor, dan batasan

**Tujuan:** memfasilitasi pasien walk-in reguler mengambil nomor antrean admisi melalui tiket kertas; memungkinkan petugas admisi memanggil nomor dan mendaftarkan kunjungan; lalu menghubungkan kunjungan tersebut dengan **nomor antrean poliklinik internal yang sudah digunakan oleh sistem Skrining Pasien**.

**Termasuk:** penerbitan tiket admisi anonim, cetak thermal, daftar antrean loket, alokasi dan pemanggilan oleh petugas, display antrean admisi, registrasi kunjungan rawat jalan, hubungan tiket admisi ke kunjungan, dan **pencatatan/penyediaan nomor antrean poliklinik internal sebagai output akhir**.

**Tidak termasuk:** registrasi lengkap pada menu Cetak Antrean, pencetakan/penyerahan tiket atau nomor poli kepada pasien, pengiriman WhatsApp, jalur membership/prioritas, reservasi online, seluruh aktivitas operasional petugas skrining sesudah daftar menunggu terbentuk, asesmen/tanda vital, pemanggilan dokter, dan penyelesaian pemeriksaan.

**Aktor dalam cakupan:** pasien reguler, perangkat kiosk, backend penomoran admisi, printer thermal, petugas/loket admisi, display antrean admisi, layanan registrasi kunjungan, dan **layanan antrean poliklinik existing sebagai penerima handoff**. Petugas skrining adalah pengguna sistem downstream, bukan aktor pelaksana dalam blueprint ini.

**Definisi dua nomor berbeda:**

| Jenis | Contoh | Dibuat saat | Ditunjukkan/diberikan kepada pasien? |
|---|---|---|---|
| Antrean **admisi** | `A-015` | Pasien menekan Cetak Antrean di kiosk | **Ya**, melalui tiket kertas. |
| Antrean **poliklinik/skrining internal** | `PD-012` | Registrasi kunjungan rawat jalan berhasil dan proses antrean poli existing dijalankan | **Tidak**. Nomor hanya disimpan/digunakan di sistem. |

Kedua nomor tidak boleh diperlakukan sebagai satu sequence dan tidak boleh tertukar dalam pencetakan atau display admisi.

## 3. Alur end-to-end sampai batas admisi (berhenti pada nomor antrean poli internal)

1. **Datang:** pasien reguler tiba di rumah sakit dan menuju kiosk. Kondisi gawat mengikuti jalur penanganan klinis, bukan antrean admisi rutin.
2. **Pilih menu:** pasien menekan **Cetak Antrean**. Jika hanya satu kelompok admisi, tidak ada pilihan penjamin atau loket.
3. **Buat nomor admisi:** backend mengalokasikan nomor antrean admisi berikutnya secara atomik, misalnya `A-015`, menyimpan ID tiket, waktu penerbitan, tanggal layanan, serta kelompok antrean.
4. **Cetak:** printer thermal mencetak **tiket antrean admisi A-015**. Pasien diarahkan untuk menunggu di area Admisi Rawat Jalan.
5. **Masuk daftar admisi:** tiket berstatus **MENUNGGU** pada halaman Daftar Antrean Admisi.
6. **Panggil berikutnya:** petugas loket menekan **Panggil Berikutnya**; sistem mengambil tiket terlama yang berstatus menunggu pada kelompok antrean tersebut, menetapkan loket tujuan, dan menampilkan nomor admisi pada display.
7. **Pelayanan loket:** pasien menuju loket; petugas mengidentifikasi pasien baru/lama, memeriksa penjamin serta dokumen, memilih poliklinik dan dokter sesuai ketentuan rumah sakit, kemudian mengisi registrasi kunjungan.
8. **Simpan kunjungan:** petugas menekan simpan. Jika valid dan berhasil, backend menyimpan **kunjungan rawat jalan** dengan identitas pasien, poli, dokter, penjamin, serta identitas transaksi. Tiket `A-015` ditautkan ke kunjungan sebagai audit asal pendaftaran; antrean admisi ditandai selesai sesuai status pelayanan.
9. **Bentuk antrean poli internal:** melalui mekanisme antrean rawat jalan/skrining **yang sudah ada**, sistem mengalokasikan dan menyimpan **nomor antrean poliklinik internal** untuk kunjungan tersebut (misalnya `PD-012`) serta status awal **menunggu skrining**, jika kunjungan memenuhi prasyarat skrining.
10. **Serah-terima data otomatis:** kunjungan dan antrean internal sudah tersedia pada sumber data/daftar kerja **Skrining Pasien** sesuai poli tujuan. Urutannya mengikuti mekanisme existing: pasien yang lebih dulu siap menunggu di atas, pasien baru di bawah. **Tidak ada tiket poli kedua yang dicetak atau diberikan kepada pasien.**

**TITIK SELESAI BLUEPRINT:** petugas admisi berhasil mendaftarkan kunjungan dan sistem memastikan satu nomor antrean poliklinik internal tercatat serta terhubung dengan pasien, kunjungan, dan poli tujuan. Seluruh proses setelah titik ini mengikuti modul **Skrining Pasien existing** dan tidak dirancang ulang di dokumen ini.

## 4. Alur antarunit dan tanggung jawab sampai handoff

| Tahap | Pasien | Kiosk / sistem | Petugas admisi |
|---|---|---|---|
| Ambil antrean admisi | Menekan Cetak Antrean | Terbitkan dan cetak tiket `A-015` | - |
| Menunggu loket | Menunggu panggilan | Menyimpan status MENUNGGU | Memantau daftar antrean admisi |
| Pemanggilan | Menuju loket yang ditampilkan | Mengalokasikan tiket ke loket dan menampilkan `A-015` | Menekan Panggil Berikutnya |
| Registrasi | Menyampaikan identitas dan dokumen | Mengelola data pasien, penjamin, poli dan dokter | Memverifikasi lalu menyimpan kunjungan |
| Hasil registrasi | Diarahkan menuju area poliklinik sesuai petunjuk | Menyimpan kunjungan dan hubungan dengan tiket admisi | Menyelesaikan pelayanan nomor admisi |
| Handoff internal (akhir blueprint) | **Tidak menerima nomor poli/tiket baru** | Membentuk nomor antrean poli internal dan menyediakan data kunjungan untuk Skrining Pasien existing | Memastikan transaksi berhasil; tidak membuat antrean manual kedua |

Catatan: **admisi tidak menjalankan skrining klinis** dalam dokumen ini. Ketersediaan data pada daftar antrean skrining merupakan keluaran integrasi, bukan tahapan layanan perawat yang dibahas lebih lanjut.

## 5. Penjamin, kelompok antrean, dan loket

**Default:** satu kelompok **Admisi Rawat Jalan Reguler**, kode `A`, antrean A-001 dan seterusnya, dilayani satu atau beberapa loket. Jenis pembayaran ditentukan saat registrasi, bukan ketika tiket dicetak.

| Kondisi RS | Konfigurasi antrean |
|---|---|
| Tidak melayani BPJS; tunai dan asuransi satu loket/area | Satu antrean gabungan; kiosk langsung cetak. |
| Melayani BPJS; semua penjamin satu kelompok loket | Tetap satu antrean gabungan. |
| Loket BPJS terpisah dari umum/asuransi | Tampilkan dua kelompok pilihan di kiosk. |
| Loket umum, BPJS, asuransi benar-benar terpisah | Tampilkan tiga pilihan sesuai konfigurasi. |
| RS tidak punya fasilitas BPJS | Jangan tampilkan BPJS. |

Beberapa loket dapat mengambil nomor berikutnya dari **satu antrean bersama**; tidak ada nomor berbeda per petugas kecuali kelompok antrean operasional memang dipisah. Pemanggilan nomor harus diklaim secara atomik untuk mencegah dua loket mengambil tiket yang sama.

Karena pencetakan tiket tidak memerlukan identitas, kiosk **tidak bisa mendeteksi status membership secara otomatis** pada tahap ini. Pembatasan pasien reguler ditegakkan melalui arahan di layar, petugas penerima, dan penanganan khusus sesuai SOP.

## 6. Aturan nomor antrean

- Nomor urut dialokasikan oleh **backend bersama**, bukan dihitung oleh setiap kiosk.
- Unik pada kombinasi **rumah sakit/fasilitas + tanggal pelayanan + kelompok antrean**.
- Contoh format `A-001` sampai `A-999` dan berlanjut sesuai kapasitas; nomor reset harian merupakan **usulan konfigurasi awal**, bukan ketentuan nasional.
- Setiap tiket tetap mempunyai `ticket_id` unik permanen sekalipun nomor display berulang di tanggal lain.
- Alokasi harus aman saat dua kiosk digunakan serentak. Tombol cetak juga harus idempotent untuk mencegah dua tiket karena ketukan ganda.
- Tiket yang sudah tercatat tetapi gagal dicetak harus bisa **dicetak ulang menggunakan ID/nomor yang sama**, bukan menerbitkan nomor berikutnya.

## 7. Siklus status antrean admisi

| Status | Makna | Tindakan |
|---|---|---|
| **MENUNGGU** | Nomor baru diterbitkan dan masuk daftar. | Dapat dipanggil oleh loket. |
| **DIPANGGIL** | Nomor telah dialokasikan dan diumumkan pada loket. | Panggil ulang, mulai pelayanan, atau lewati. |
| **DILAYANI** | Pasien sudah hadir dan sedang ditangani admisi. | Lanjut registrasi / catat kendala. |
| **SELESAI** | Pelayanan di loket sudah ditutup. | Tautkan ke kunjungan jika registrasi berhasil. |
| **DILEWATI** | Pasien tidak hadir saat dipanggil sesuai SOP. | Recall atau perlakuan lain sesuai kewenangan. |
| **DIBATALKAN** | Tiket dinyatakan tidak berlaku dengan alasan. | Audit wajib; tidak masuk skrining. |

**Selesai antrean admisi tidak sama dengan selesai kunjungan rawat jalan.** Jika registrasi tidak berhasil, status/alasannya dicatat dan sistem tidak boleh membuat antrean poliklinik internal untuk kunjungan yang tidak valid.

Status cetak (`BERHASIL`, `GAGAL`, `BELUM_DIKONFIRMASI`) adalah **status terpisah** dari status proses pelayanan.

## 8. Halaman dan titik integrasi yang dibutuhkan

### 8.1 Kiosk - Cetak Antrean

- Satu menu **Cetak Antrean** pada beranda kiosk reguler.
- Langsung mencetak jika rumah sakit hanya menggunakan satu kelompok admisi; pilihan kelompok ditampilkan hanya jika jalur loket memang dibedakan.
- Tidak meminta NIK, No. RM, penjamin, poli atau dokter.
- Menyediakan petunjuk tempat menunggu, penanganan printer gagal, dan proteksi ketukan ganda.
- Hanya menampilkan atau mencetak **nomor admisi**, tidak pernah nomor poliklinik internal.

### 8.2 Halaman petugas - Daftar Antrean Admisi

- **Halaman tersendiri**, bukan halaman **Daftar Pasien Rawat Jalan**.
- Informasi minimum: nomor admisi, waktu ambil, status, kelompok antrean, loket, waktu panggil, dan hubungan ke kunjungan setelah valid.
- Aksi petugas: **Panggil Berikutnya**, Panggil Ulang, Mulai Pelayanan, Lewati, Buka Registrasi Kunjungan, dan Selesaikan.
- Dua petugas tidak boleh mengklaim nomor menunggu yang sama; filter berdasarkan tanggal, kelompok dan status tersedia.

### 8.3 Halaman petugas - Pendaftaran Pasien / Daftar Pasien Rawat Jalan

- Dari antrean berstatus DILAYANI, petugas mengakses proses registrasi kunjungan rawat jalan existing.
- Petugas menentukan pasien baru/lama, penjamin, poliklinik, dokter, serta kelengkapan kunjungan.
- Simpan kunjungan menghasilkan `encounter_id`/identitas registrasi dan menghubungkannya ke tiket admisi.
- **Hasil simpan yang berhasil harus memicu atau menggunakan proses pembentukan antrean poliklinik internal yang sudah ada**; tidak membuat tiket cetak atau form pilih antrean baru.
- Daftar Pasien Rawat Jalan menampilkan hasil registrasi yang valid sebagai data kunjungan, bukan sekadar tiket admisi.

### 8.4 Titik integrasi ke halaman Skrining Pasien existing (hanya handoff)

- Setelah simpan kunjungan valid, **satu nomor antrean poliklinik internal** tercatat pada sistem berdasarkan aturan penomoran existing dan poli tujuan.
- Kunjungan yang baru siap skrining masuk **bagian paling bawah antrean pasien menunggu** dalam cakupan poli/cluster yang sama sebagaimana ketentuan proses skrining existing.
- Status awal antrean internal disesuaikan dengan kontrak existing (secara bisnis: **Menunggu Skrining**).
- Data minimum dapat diakses oleh halaman **Skrining Pasien** yang sudah dibuat.
- **Di luar scope:** mengubah desain halaman, tombol panggil perawat, pelaksanaan skrining, vital sign, asesmen, status selesai skrining, dan antrean dokter.

### 8.5 Display Antrean Admisi

- Menampilkan **nomor admisi yang sedang dipanggil** dan **loket tujuan**, serta suara panggilan bila digunakan.
- Tidak menampilkan identitas pasien, No. RM, NIK, data medis, ataupun **nomor antrean poli internal**.

## 9. Rancangan tiket thermal 80 mm

```text
            RUMAH SAKIT [NAMA]
       ANTREAN ADMISI RAWAT JALAN
------------------------------------
              NOMOR ANTREAN
                  A-015

         Tanggal: 28-10-2026
         Waktu  : 09:15 WIB
------------------------------------
 Silakan tunggu nomor Anda dipanggil
 di area Admisi Rawat Jalan.
 Simpan tiket hingga selesai dilayani.
```

Ukuran 80 mm adalah rekomendasi rancangan awal; harus menyesuaikan printer yang dipakai rumah sakit. Tiket tidak memuat nama/NIK karena belum ada identifikasi pasien.

## 10. Kontrak integrasi registrasi kunjungan -> antrean poliklinik internal

**Kontrak ini adalah batas handoff, bukan rancangan ulang proses Skrining Pasien.**

1. **Pemicu:** kunjungan rawat jalan berhasil disimpan dan memenuhi syarat untuk masuk antrean skrining/poliklinik. Penerbitan tiket admisi ataupun pemanggilan loket **tidak** membuat antrean poli internal.
2. **Relasi wajib:** `admission_ticket_id` -> `encounter_id` (audit asal registrasi); antrean poli internal mengacu ke `encounter_id`, `patient_id`, `clinic_id`/unit layanan dan aturan penomoran existing.
3. **Nomor internal wajib ada:** setelah registrasi valid, sistem memakai mekanisme existing untuk menghasilkan/mendapatkan nomor antrean poli (`QueueNumber`/`QueueCode` atau padanan kontrak yang berlaku). Ini **nomor nyata pada data internal**, bukan sekadar nomor urut visual baris.
4. **Tidak ada distribusi nomor internal kepada pasien:** nomor poli tidak dicetak pada tiket kedua, tidak dicantumkan pada tiket admisi, tidak dikirim WhatsApp, dan tidak diminta untuk pasien baca/masukkan kembali.
5. **Urutan:** mekanisme penyusunan daftar pasien menunggu pada **Skrining Pasien** existing tetap digunakan. Untuk poli yang sama, pasien baru ditempatkan setelah pasien yang sudah lebih dahulu siap menunggu; bila aturan penomoran memakai scope unit/cluster, ikuti scope existing tersebut.
6. **Integritas:** untuk satu kunjungan dan kebutuhan antrean aktif yang sama tidak boleh lahir antrean ganda karena retry, refresh, atau tombol Simpan ditekan ulang.
7. **Konsistensi keberhasilan:** kegagalan pencatatan antrean internal setelah kunjungan tersimpan **tidak boleh diam-diam dianggap handoff berhasil**. Sistem mencatat status sinkronisasi dan menyiapkan retry idempotent terhadap kunjungan yang sama; jangan membentuk kunjungan kedua.
8. **Koreksi sebelum skrining:** bila poli/dokter dalam kunjungan dikoreksi, antrean internal dan daftar kerja tujuan harus konsisten melalui aturan perubahan existing, tanpa menggandakan antrean aktif.
9. **Bukti akhir:** dapat ditelusuri satu tiket admisi, satu registrasi kunjungan valid, serta satu entri antrean poli internal yang sesuai. Halaman skrining dapat membacanya; **pekerjaan blueprint berhenti di sini**.

**Contoh ilustrasi internal** (bukan payload API nyata dan bukan format wajib):

| Objek | Identitas | Visibilitas |
|---|---|---|
| Tiket Admisi | `A-015` | Dicetak untuk pasien dan tampil pada display admisi. |
| Kunjungan Rawat Jalan | `RJ-20261028-0015` | Disimpan pada SIMRS, dipakai petugas. |
| Antrean Poli / Skrining | `PD-012` | **Hanya data internal SIMRS**, tampil pada daftar kerja petugas yang berwenang. |

## 11. Data minimal (konseptual, bukan nama tabel baru yang dipastikan ada)

| Data bisnis | Kebutuhan minimum |
|---|---|
| Tiket antrean admisi anonim | `ticket_id`, `facility_id`, `service_date`, `queue_group_id`, `sequence_number`, `display_number`, `issued_at`, `kiosk_device_id`, `status` |
| Pemanggilan/pelayanan admisi | `counter_id`, `called_at`, `called_by`, `call_attempt_count`, `service_started_at`, `service_ended_at`, `skip_reason`, `cancel_reason` |
| Pencetakan admisi | `printer_device_id`, `print_status`, `print_attempt_count`, `last_printed_at` |
| Registrasi kunjungan | `patient_id`, `encounter_id`, `service_unit_id`, `clinic_id`, `doctor_id`, `payment/guarantor`, `registration_completed_at` |
| **Antrean poli internal** | **`queue_id`, `encounter_id`, `patient_id`, `clinic_id`/scope, `queue_number`, `queue_code`, `queue_status`, `queue_date`, `ready_for_screening_at` atau field existing ekuivalen** |
| Hubungan & audit handoff | `admission_ticket_id`, `encounter_id`, referensi antrean poli internal, status/hasil sinkronisasi, `correlation_id` |
| Konfigurasi antrean admisi | `queue_group`, `prefix`, `reset_policy`, `active_counter`, `kiosk_assignment` |
| Audit perubahan | `actor_id`, `event`, `event_at`, `old_status`, `new_status`, `reason` |

Semua nama field di atas bersifat **kebutuhan data konseptual** kecuali yang dinyatakan telah ditemukan pada source. Jangan membuat tabel antrean poli baru atau menambah nomor antrean poli baru jika **`RegQueue`/mekanisme existing** telah menangani kebutuhan tersebut. Tiket antrean admisi anonim adalah domain berbeda karena dibuat sebelum `patient_id` dan `encounter_id` diketahui.

## 12. Penanganan kondisi alternatif dan gagal

| Kondisi | Respons yang diwajibkan |
|---|---|
| Printer kosong/offline sebelum nomor dibuat | Bila terdeteksi, blokir penerbitan dan arahkan ke Bantuan Petugas. |
| Nomor admisi sudah tersimpan tetapi gagal dicetak | **Cetak ulang nomor admisi yang sama**, bukan antrean baru. |
| Tombol kiosk ditekan dua kali / request diulang | Gunakan idempotency untuk mencegah tiket ganda. |
| Dua kiosk menerbitkan bersamaan | Alokasi nomor admisi atomik; tidak ada nomor identik pada tanggal dan kelompok sama. |
| Dua loket memanggil berikutnya bersamaan | Satu tiket admisi hanya diklaim satu loket. |
| Pasien tidak hadir saat dipanggil | DILEWATI dan recall dicatat sesuai SOP; tanpa membuat antrean poli internal. |
| Identitas/berkas tidak valid sehingga registrasi gagal | Antrean admisi ditangani sesuai SOP; **jangan bentuk kunjungan atau antrean poli fiktif**. |
| Kunjungan tersimpan tetapi respons API hilang | Cari transaksi berdasarkan identitas/correlation; retry aman tanpa membuat kunjungan baru. |
| Kunjungan tersimpan tetapi alokasi antrean poli internal gagal | Tandai handoff pending/gagal, lakukan pemulihan idempotent pada kunjungan yang sama, dan tampilkan kendala kepada petugas; jangan menganggap selesai. |
| Retry antrean poli internal | Pastikan antrean untuk kunjungan yang sama tidak terduplikasi. |
| Pasien sebenarnya sudah mendaftar/check-in dari jalur lain | Cegah registrasi kunjungan ganda; arahkan ke proses yang sesuai. |
| Pasien memerlukan penanganan gawat | Alihkan ke pelayanan klinis segera, tidak mengikuti antrean reguler. |

**Batas skenario:** dokumen ini tidak mengatur apa yang dilakukan perawat setelah antrean internal muncul pada halaman Skrining Pasien.

## 13. Skenario UAT penerimaan MVP (berakhir saat handoff internal)

1. **UAT-01** - Satu sentuhan pada kiosk kelompok tunggal menerbitkan tiket admisi tanpa meminta identitas.
2. **UAT-02** - Dua pasien berurutan mendapatkan `A-001` lalu `A-002` pada kelompok yang sama.
3. **UAT-03** - Dua kiosk menerbitkan tiket secara bersamaan tanpa nomor duplikat.
4. **UAT-04** - Gagal cetak setelah transaksi tiket tersimpan: cetak ulang menggunakan tiket/nomor yang sama.
5. **UAT-05** - Dua loket memanggil antrean berikutnya secara bersamaan tanpa klaim tiket yang sama.
6. **UAT-06** - Panggil Ulang dan Lewati dapat dicatat tanpa mengubah nomor antrean admisi.
7. **UAT-07** - Registrasi kunjungan gagal: tidak ada antrean poli internal yang dibuat dari transaksi gagal.
8. **UAT-08** - Registrasi kunjungan berhasil: pasien dan poli tujuan tertaut ke kunjungan valid; tiket admisi tertaut ke kunjungan tersebut.
9. **UAT-09** - Sesudah registrasi berhasil, **sistem memiliki satu nomor antrean poliklinik internal** terkait kunjungan, pasien, dan poli tujuan.
10. **UAT-10** - Pasien yang baru diregistrasi berada di **bagian bawah daftar menunggu Skrining Pasien** yang sudah ada untuk poli/scope terkait, tanpa mengubah alur kerja existing.
11. **UAT-11** - **Nomor antrean poli internal tidak dicetak, tidak diberikan kepada pasien, dan tidak ditampilkan di display antrean admisi**.
12. **UAT-12** - Dua kali penyimpanan/retry tidak menambah kunjungan maupun antrean poli aktif ganda.
13. **UAT-13** - Jika alokasi antrean poli gagal, status handoff tidak dinyatakan sukses; pemulihan tidak membuat kunjungan kedua.
14. **UAT-14** - RS tanpa BPJS tidak menampilkan pilihan BPJS; RS satu kelompok antrean langsung mencetak.
15. **UAT-15** - Display antrean admisi hanya menampilkan nomor admisi/loket, tidak menampilkan identitas pasien maupun antrean poli internal.

**Tidak diuji dalam blueprint ini:** panggil perawat, mulai/selesai skrining, tanda vital, asesmen, dokter, atau tahap lain setelah handoff.

## 14. Draft Step Pembuatan untuk tim implementasi (bukan write ClickUp)

| Task bisnis | Native Subtask yang direncanakan |
|---|---|
| **T01 - Pengaturan Kelompok Antrean dan Loket Admisi** | 01 - Definisi kelompok dan aturan loket; 02 - Prefix/nomor/reset; 03 - Penugasan loket dan kiosk; 04 - Validasi konfigurasi BPJS/non-BPJS. |
| **T02 - Penerbitan dan Cetak Tiket Admisi** | 01 - Alokasi nomor admisi atomik; 02 - Persistensi tiket dan audit; 03 - Cetak kertas thermal; 04 - Idempotency dan cetak ulang tiket yang sama. |
| **T03 - Daftar Antrean dan Pemanggilan Admisi** | 01 - Daftar/status/filter tiket; 02 - Panggil Berikutnya multi-loket; 03 - Panggil Ulang/Lewati/Mulai/Selesai; 04 - Riwayat pemanggilan. |
| **T04 - Registrasi Kunjungan dari Tiket Admisi** | 01 - Membuka pendaftaran dari tiket; 02 - Verifikasi pasien/penjamin/poli/dokter; 03 - Simpan kunjungan dan tautkan tiket; 04 - Pencegahan registrasi ganda. |
| **T05 - Pencatatan Nomor Antrean Poliklinik Internal** | 01 - Pakai ulang mekanisme antrean poli existing; 02 - Bentuk nomor internal setelah registrasi valid; 03 - Hubungkan nomor ke pasien/kunjungan/poli; 04 - Pastikan hanya tersimpan di sistem dan masuk urutan daftar menunggu existing; 05 - Uji idempotency handoff. |
| **T06 - Display Antrean Admisi dan UAT** | 01 - Display nomor admisi/loket; 02 - Suara panggilan admisi; 03 - Proteksi data pasien dan pemisahan dua nomor; 04 - Uji alur ujung-ke-ujung hanya sampai handoff. |

**Pekerjaan yang sengaja tidak dibentuk di blueprint ini:** perubahan UI/UX Skrining Pasien, fitur panggilan perawat, tindakan asesmen, vital sign, pemanggilan dokter, atau layanan setelah registrasi/handoff.

Semua baris adalah **rencana review**, bukan Task/Subtask ClickUp yang telah dibuat. Ketersediaan komponen existing harus diverifikasi agar pekerjaan yang sudah ada digunakan kembali, bukan diimplementasikan ulang.

## 15. Pemetaan awal terhadap source Quilvian (inspeksi read-only terbatas)

**Frontend V2:** repository `DevBenari/QuilvianSystemFrontendDev`, branch `QuilvianDevV2`, memperlihatkan Menu induk **Rawat Jalan** dengan label Submenu existing tepat **Pendaftaran Pasien**, **Skrining Pasien**, dan **Daftar Pasien Rawat Jalan**. Route **Skrining Pasien** mengarah ke halaman `nurse-station-queue`. Pada beranda kiosk `kiosk-home-view.jsx` yang diperiksa dalam baseline sebelumnya, item aktif **Cetak Antrean** belum terlihat; ini bukan audit exhaustif seluruh implementasi.

**Backend legacy:** `DevBenari/QuilvianSystemBackendDev`, branch `master`, mempunyai model `Kunjungan` yang dipetakan ke `public.MstKunjungan`, termasuk `Antrian` dan `IsScreening`. Ini bukti persistensi kunjungan serta sebagian metadata antrean lama, **bukan** bukti tiket admisi anonim sudah tersedia.

**Backend baru:** `DevBenari/NewQuilvianSystemBackend`, branch `QuilvianIntegrationBackend`, mempunyai model `RegPatientEncounter` untuk kunjungan dan model `RegQueue` yang dipetakan ke `public.RegQueue`. `RegQueue` menyimpan **`EncounterId`, `PatientId`, `ClinicId`, `QueueNumber`, `QueueCode`, `QueueStatus`** serta hubungan ke kunjungan dan pasien. Ini konsisten dengan kebutuhan **nomor antrean poli/skrining internal setelah registrasi**. Mekanisme otomatis saat admisi menyimpan kunjungan dan kesesuaian urutan tampil harus diuji secara end-to-end; keberadaan model saja tidak membuktikan semua integrasi sudah berjalan.

**Pemisahan model:** tiket admisi anonim diterbitkan **sebelum** pasien/kunjungan teridentifikasi, sehingga **tidak boleh langsung direpresentasikan sebagai `RegQueue` yang memerlukan `EncounterId` dan `PatientId`**. Sebaliknya, kebutuhan antrean poli internal harus **memprioritaskan reuse `RegQueue` dan allocator existing** bila sesuai kontrak, bukan membuat tabel/penomoran kedua.

| Kebutuhan data / persistensi | BE legacy (`master`) | BE baru (`QuilvianIntegrationBackend`) | Gap / langkah verifikasi |
|---|---|---|---|
| Registrasi kunjungan pasien | `TERSEDIA`: `Kunjungan` / `public.MstKunjungan` | `TERSEDIA`: `RegPatientEncounter` / `public.RegPatientEncounter` | Reuse proses registrasi existing; uji hubungan ke tiket admisi. |
| **Nomor antrean poli internal** | `PARTIAL`: `MstKunjungan.Antrian`, tetapi kontrak worklist dan penomoran skrining belum diaudit | `TERSEDIA` untuk struktur data: `RegQueue` / `public.RegQueue` memiliki `QueueNumber`, `QueueCode`, `EncounterId`, `PatientId`, `QueueStatus` | Verifikasi pembuatan nomor otomatis sesudah registrasi, satu antrean per kunjungan yang berlaku, urutan worklist dan tidak dicetak. |
| Tiket admisi anonim, loket dan printer | `BELUM_TERVERIFIKASI` | `BELUM_TERVERIFIKASI` | Audit model, service, controller, migration, printer, dan display admisi; jangan menyimpulkan tidak tersedia. |

**Klasifikasi keseluruhan implementasi target: UNVERIFIED / BELUM TERVERIFIKASI.** Pemetaan ini **belum merupakan audit seluruh route/service atau paket backlog siap import**; tidak ada perubahan source repository.

**Empat kategori availability (batas bukti yang tersedia):**

- **Menu Tersedia:** `Rawat Jalan` (label existing FE V2).
- **Submenu Tersedia di bawah Rawat Jalan:** `Pendaftaran Pasien`; `Skrining Pasien`; `Daftar Pasien Rawat Jalan`.
- **Menu Belum Tersedia / Usulan:** tidak dinyatakan sebagai fakta sebelum audit menyeluruh; **Cetak Antrean** adalah kemampuan target di beranda Kiosk, bukan otomatis Menu sidebar baru.
- **Submenu Belum Tersedia / Usulan:** **Daftar Antrean Admisi** adalah usulan halaman/kapabilitas petugas; klasifikasi perlu audit menu & route existing agar tidak menduplikasi navigasi.

**Sumber pemeriksaan terbatas:**

- FE: https://github.com/DevBenari/QuilvianSystemFrontendDev/blob/QuilvianDevV2/src/utils/menu-sidebar/menu-items.jsx
- FE: https://github.com/DevBenari/QuilvianSystemFrontendDev/blob/QuilvianDevV2/src/components/view/kiosk/kiosk-home-view.jsx
- FE: https://github.com/DevBenari/QuilvianSystemFrontendDev/blob/QuilvianDevV2/src/app/health-services/registration-management/nurse-station-queue/page.jsx
- Legacy BE: https://github.com/DevBenari/QuilvianSystemBackendDev/blob/master/Areas/ManajemenKesehatan/Pendaftaran/Models/Kunjungan.cs
- New BE: https://github.com/DevBenari/NewQuilvianSystemBackend/blob/QuilvianIntegrationBackend/Areas/HealthServices/RegistrationManagement/Models/RegQueue.cs
- New BE: https://github.com/DevBenari/NewQuilvianSystemBackend/blob/QuilvianIntegrationBackend/Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounter.cs

## 16. Referensi rumah sakit dan batas bukti

**RSUD dr. M. Ashari Pemalang:** standar pelayanan menyebut pasien mengambil antrean lewat mesin, dipanggil di loket, menyerahkan berkas, lalu didaftarkan sesuai penjamin dan poli. Standar rawat jalan menyebut perawat memanggil pasien sesuai urutan SIMRS dan melakukan tanda vital. Ini referensi proses **nomor antrean admisi -> loket registrasi -> pengelolaan urutan di sistem poli**; dokumen publik tidak membuktikan detail nomor internal yang tidak dicetak.
- https://ppidrsud.pemalangkab.go.id/laman/pelayanan-pendaftaran-rawat-jalan
- https://ppidrsud.pemalangkab.go.id/laman/pelayanan-rawat-jalan

**RSUD Bali Mandara:** standar rawat jalan mendokumentasikan mesin antrean -> loket pendaftaran -> nurse station -> skrining/asesmen keperawatan -> dokter. Standar tersebut juga menyebut pemanggilan memakai nomor antrean pada nurse station, sehingga **bukan bukti bahwa semua rumah sakit tidak menggunakan nomor di poli**.
- https://rsbm.baliprov.go.id/standar-pelayanan-poliklinik

**RSUD dr. Darsono Pacitan:** memisahkan pasien yang harus bertemu petugas admisi dari pasien yang sudah bisa check-in/pendaftaran mandiri untuk menuju poli. Ini mendukung pemisahan menu Cetak Antrean, Pendaftaran Mandiri, dan Check-In dalam Quilvian.
- https://rsud.pacitankab.go.id/perubahan-alur-dan-lokasi-pendaftaran-rawat-jalan/

**Klasifikasi evidence:** alur kiosk/loket/poli didukung dokumen rumah sakit; **nomor poli internal yang tidak diberikan kepada pasien**, penempatan pasien baru di bawah daftar menunggu, dan konfigurasi kelompok antrean merupakan **keputusan target Quilvian** dan integrasi dengan mekanisme skrining existing, bukan klaim alur identik pada rumah sakit referensi.

## 17. Validasi sebelum mulai implementasi (hanya scope admisi dan handoff)

1. Jumlah loket dan kelompok antrean admisi serta kategori penjamin aktif di rumah sakit target.
2. Jenis printer thermal, format kertas, dan kemampuan perangkat mendeteksi gagal cetak.
3. SOP panggil ulang, lewati, batal, dan penutupan antrean admisi saat registrasi gagal.
4. Kontrak kapan registrasi dianggap berhasil, kapan nomor antrean poli internal dibentuk, serta apakah proses existing sudah menjamin idempotency.
5. Scope dan aturan pengurutan internal **Skrining Pasien** yang sudah dibuat (poli/cluster, urutan masuk, field `QueueNumber` dan `QueueCode`); pastikan pasien baru menempati urutan setelah pasien yang sudah lebih dulu menunggu dalam scope yang sama.
6. Verifikasi bahwa **nomor antrean poli internal tidak muncul pada tiket kiosk, tidak dicetak setelah admisi, dan tidak diberikan kepada pasien**.
7. Audit lebih dalam apakah entitas/endpoint tiket admisi anonim sudah tersedia serta bagaimana registrasi terhubung ke `RegQueue` existing. Kebutuhan baru tidak boleh menduplikasi persistence/logic antrean poli.

---

**KEPUTUSAN AKHIR REVISI V1.1:** kiosk hanya mencetak **nomor antrean Admisi Rawat Jalan** untuk pasien reguler. Petugas admisi memanggil dan menyimpan **kunjungan rawat jalan**. Setelah berhasil, sistem mencatat **nomor antrean poliklinik/skrining internal** pada proses yang sudah ada, menempatkan pasien pada daftar menunggu yang tepat, dan **tidak memberikan nomor tersebut kepada pasien**. **Blueprint berhenti ketika kunjungan dan antrean internal telah tercatat/siap dipakai oleh Skrining Pasien.**
