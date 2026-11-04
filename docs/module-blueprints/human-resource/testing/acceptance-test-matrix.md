# Human Resource — Matriks Acceptance Test

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Dokumen | `testing/acceptance-test-matrix.md` |
| `contract_version` | `v6` |
| `last_changed_in` | `v7` — bagian 15 (`S-D1`) ditambahkan 3 November 2026; bagian lain tidak berubah sejak `v6` |
| Status | `draft` — **belum** `approved` |
| Owner | Technical owner (`HRD-DEC-015`), bersama pemilik proses untuk skenario UAT |
| `approved_by` / `approved_at` | **Hanya bagian 15** (`S-D1` Perencanaan Tenaga Kerja): `approved` oleh Pengguna — pemilik teknis dan produk (`HRD-DEC-015`) — pada 2026-11-03. Bagian lain dokumen ini **belum** disetujui, sehingga status dokumen tetap `draft` |
| `input_revision` | `contracts/state-transition-matrix.md` `v6`; `contracts/validation-matrix.md` `v6`; `contracts/api-contract.md` `v6`; `00-interview-decisions.md` rev `16` (`HRD-DEC-046` s.d. `HRD-DEC-057`) |
| `input_hash` — decision log | `26da64e129b0c34a17b67b0a41f47986b1aedae92a600077d936b0741eb574ee` |
| Backend SHA | `e0ee42c752a5f92c5b1663ff88bef07a5859f79f` |
| Frontend SHA | `fff76a1b394d4b247c70a04f106c8ec098c9696e` |

---

## 0. Cara membaca dokumen ini

Dokumen ini menjawab satu pertanyaan: **apa yang harus dibuktikan sebelum sebuah kemampuan boleh
dinyatakan selesai**, dan bukti seperti apa yang diterima.

### 0.1 Aturan yang mengikat seluruh matriks

| Aturan | Sebabnya |
| --- | --- |
| Setiap requirement **MUST** punya sekurang-kurangnya satu skenario **berhasil** dan satu skenario **gagal** | Matriks yang hanya memuat jalur berhasil tidak membuktikan apa pun. Jalur gagal justru yang paling sering ditemui petugas |
| Bukti **MUST** dapat diperiksa orang lain | "Sudah dites" bukan bukti. Nama berkas test, langkah reproduksi, atau tangkapan layar adalah bukti |
| Kemampuan berstatus `BLOCKED` **MUST NOT** punya baris yang mengaku dapat diuji | Menuliskannya membuat pembaca menyangka pekerjaannya sudah boleh dimulai |
| Data contoh **MUST** memakai nama samaran | Larangan privasi. Tidak ada data pegawai asli di dokumen mana pun |

### 0.2 Jenis test yang dipakai

| Jenis | Artinya | Bukti yang diharapkan |
| --- | --- | --- |
| `UNIT` | Menguji satu aturan bisnis terpisah dari basis data | Nama berkas dan nama test yang lulus |
| `INTEGRATION` | Menguji satu alur menembus lapisan sampai basis data | Nama berkas dan nama test yang lulus |
| `CONTRACT` | Membuktikan bentuk permintaan, jawaban, dan hak akses sebuah endpoint sesuai kontrak | Nama test, ditambah rujukan baris pada `contracts/api-contract.md` |
| `E2E` | Menguji satu kasus nyata dari layar sampai basis data | Langkah reproduksi beserta hasilnya |
| `MANUAL` | Diperiksa orang, karena tidak dapat diotomatiskan dengan wajar | Langkah reproduksi, tangkapan layar, dan nama pemeriksa |

### 0.3 Penomoran

`AT-HRD-<slice>-<nn>`. Nomor yang sudah dipakai **MUST NOT** didaur ulang untuk isi yang berbeda.
Baris yang dibatalkan ditandai `Dibatalkan` beserta alasannya, bukan dihapus.

---

## 1. Fondasi — `S0-A`, `S0-B`

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-HRD-S0-01` | Route bergaya kebab-case menjadi route canonical | **Berhasil:** memanggil route canonical sebuah endpoint HR mengembalikan jawaban yang sama dengan route lamanya | `CONTRACT` | Test kontrak untuk kedua route menunjuk satu implementasi yang sama |
| `AT-HRD-S0-02` | Route lama tetap hidup sebagai alias | **Berhasil:** memanggil route lama tetap berhasil dan tidak menghasilkan peringatan kedaluwarsa yang menghentikan pemanggil | `CONTRACT` | Test kontrak untuk route lama lulus |
| `AT-HRD-S0-03` | Satu action, satu implementasi | **Gagal:** menambahkan controller kedua yang melayani route lama ditolak pada tinjauan | `MANUAL` | Catatan tinjauan yang menyebut `HRD-DEC-016` |
| `AT-HRD-S0-04` | Prefix `Wfp` terdaftar pada registry kepemilikan | **Berhasil:** registry memuat baris `Wfp` beserta pemiliknya | `MANUAL` | Kutipan baris pada berkas registry |
| `AT-HRD-S0-05` | Entity transaksional HR baru tidak memakai prefix `Trx` | **Gagal:** usulan entity baru berprefix `Trx` ditolak pada tinjauan | `MANUAL` | Catatan tinjauan yang menyebut `HRD-DEC-019` |

---

## 2. Administrasi kepegawaian — `S-A1`

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-HRD-A1-01` | Permohonan perubahan data melewati verifikasi | **Berhasil:** permohonan Ani Lestari disetujui HR lalu diterapkan; data berubah sejak tanggal berlaku | `INTEGRATION` | Test alur permohonan sampai penerapan |
| `AT-HRD-A1-01b` | Sama | **Gagal:** permohonan tanpa bukti pendukung dikembalikan; data **tidak** berubah | `INTEGRATION` | Test yang membuktikan data tidak berubah |
| `AT-HRD-A1-02` | Verifikasi wajib menyebut alasan saat menolak | **Gagal:** penolakan tanpa alasan ditolak sistem | `UNIT` | Test validasi alasan wajib |
| `AT-HRD-A1-03` | Perubahan penempatan dan remunerasi wajib disetujui — `HRD-DEC-031` | **Gagal:** pemberlakuan tanpa persetujuan ditolak `409` | `INTEGRATION` | Test yang membuktikan penerapan tertahan |
| `AT-HRD-A1-07` | Penyetuju harus berbeda dari pembuat — `HRD-DEC-031` | **Gagal:** HR Admin Sari menyetujui pengajuannya sendiri; ditolak `403`, nilai gaji tidak berubah | `INTEGRATION` | Test yang membandingkan pembuat dan penyetuju |
| `AT-HRD-A1-07b` | Sama | **Gagal:** unit dengan satu petugas **tetap** ditolak; pengajuan dieskalasi, bukan disetujui otomatis | `INTEGRATION` | Test yang membuktikan tidak ada jalur pengecualian |
| `AT-HRD-A1-07c` | Sama | **Berhasil:** HR Manager Dewi menyetujui pengajuan Sari; nilai gaji berubah sejak tanggal berlaku | `INTEGRATION` | Test alur lengkap |
| `AT-HRD-A1-08` | Nominal gaji tidak ada pada daftar lintas pegawai — `HRD-DEC-033` | **Gagal:** isi jawaban daftar **tidak memuat** nominal, bukan sekadar tersamarkan di layar | `CONTRACT` | Isi jawaban jaringan yang diperiksa |
| `AT-HRD-A1-08b` | Sama | **Berhasil:** pemegang `WfpSalaryAssignment : ViewAmount` membaca nominal lewat endpoint detail terpisah | `CONTRACT` | Test kontrak endpoint nominal |
| `AT-HRD-A1-08c` | Butir baca umum tidak membuka nominal | **Gagal:** pemegang `: Read`/`: ReadAll` saja tidak memperoleh nominal | `INTEGRATION` | Test otorisasi |
| `AT-HRD-A1-09` | Empat transaksi penempatan berdiri sendiri — `HRD-DEC-036` | **Berhasil:** mengubah konfigurasi alur penetapan gaji **tidak** mengubah alur penempatan organisasi, jabatan, maupun atasan | `INTEGRATION` | Test yang membandingkan keempat definisi sesudah satu diubah |
| `AT-HRD-A1-09b` | Sama | **Gagal:** usulan satu definisi alur bersama untuk keempatnya ditolak pada tinjauan | `MANUAL` | Catatan tinjauan yang menyebut `HRD-DEC-036` |
| `AT-HRD-A1-10` | Persetujuan penetapan gaji | **Berhasil:** HR Manager menyetujui pengajuan HR Admin; gaji berlaku sejak tanggal berlaku. **Gagal:** pemrakarsa menyetujui sendiri, ditolak `403` | `INTEGRATION` | Test per transaksi, jejak audit menyebut jenis transaksinya |
| `AT-HRD-A1-11` | Persetujuan penempatan organisasi | Sama bentuknya, **entity dan jejak audit berbeda** | `INTEGRATION` | Sama |
| `AT-HRD-A1-12` | Persetujuan penempatan jabatan | Sama bentuknya, entity dan jejak audit berbeda | `INTEGRATION` | Sama |
| `AT-HRD-A1-13` | Persetujuan penetapan atasan | Sama bentuknya, entity dan jejak audit berbeda | `INTEGRATION` | Sama |
| `AT-HRD-A1-14` | Penyelesaian penyetuju saat pemrakarsa satu-satunya pemegang butir | **Berhasil:** tugas ditugaskan ulang ke penyetuju tingkat lebih tinggi. **Gagal:** tidak pernah menjadi swa-setuju | `INTEGRATION` | Test yang membuktikan tidak ada jalur swa-setuju |
| `AT-HRD-A1-03b` | Sama | **Berhasil:** setelah pejabat menyetujui, penerapan berhasil dan nilai gaji berubah | `INTEGRATION` | Test alur lengkap |
| `AT-HRD-A1-04` | HR tidak membuat atau mencabut akun aplikasi sendiri | **Gagal:** tidak ada satu pun endpoint HR yang membuat atau menghapus akun aplikasi | `MANUAL` | Hasil penelusuran endpoint beserta kesimpulannya |
| `AT-HRD-A1-05` | Nilai gaji tidak masuk log | **Gagal:** memicu jalur yang menyentuh nilai gaji tidak meninggalkan nominal di log | `INTEGRATION` | Isi log yang diperiksa, dengan nominal tersamarkan |
| `AT-HRD-A1-06` | Perubahan berlaku surut ke periode tertutup ditolak | **Gagal:** penerapan dengan tanggal berlaku pada periode tertutup ditolak beserta pesan yang terbaca pengguna | `INTEGRATION` | Test penolakan beserta kodenya |
| `AT-HRD-A1-15` | Layar Penempatan Organisasi memuat 55 baris (`AC-WFC-01`) | **Berhasil:** pada database dev saat ini, tabel menampilkan 55 baris data, bukan tabel kosong | `E2E` | Screenshot tampilan dan verifikasi 55 baris data |
| `AT-HRD-A1-16` | Informasi kesalahan jaringan ramah pengguna (`AC-WFC-02`) | **Gagal:** matikan backend; tabel menampilkan pesan "Data gagal dimuat" dan tombol coba lagi, bukan tabel kosong | `E2E` | Uji keadaan backend offline dan verifikasi InformationAlert |
| `AT-HRD-A1-17` | Form Master Data mengunci unit & jabatan (`AC-WFC-03`) | **Gagal:** HR membuka form edit pegawai yang ada; dropdown unit & jabatan disable dan API menolak pengubahan langsung dengan kode 422 | `INTEGRATION` | Test validasi controller master pegawai |
| `AT-HRD-A1-18` | Tepat satu unit utama per tanggal (`AC-WFC-04`) | **Gagal:** ajukan penempatan unit utama kedua yang tanggalnya beririsan tanpa menutup penempatan lama; ditolak 409 | `UNIT` | Test invariant penempatan utama tunggal |
| `AT-HRD-A1-19` | Penolakan SK berlaku surut ke periode tertutup (`AC-WFC-05`) | **Gagal:** tutup periode kehadiran November; ajukan SK berlaku efektif 15 November; ditolak 409 | `INTEGRATION` | Test integrasi penjaga periode kehadiran |
| `AT-HRD-A1-20` | Nomor identitas unik lintas jenis tenaga (`AC-WFC-06`) | **Berhasil:** daftarkan Pegawai baru dengan NIK milik Dokter yang sudah ada; jumlah profil workforce tidak bertambah dan peran ditautkan | `INTEGRATION` | Test pembuatan profil dengan NIK kembar |
| `AT-HRD-A1-21` | Rekening sensitif dilarang diedit langsung (`AC-WFC-07`) | **Gagal:** HR mengedit rekening via master ditolak; lewat permohonan atas nama, pembuat dilarang memverifikasi sendiri (ditolak 403) | `INTEGRATION` | Test otorisasi verifikasi permohonan |
| `AT-HRD-A1-22` | Kontrol akses Employee 360 atasan (`AC-WFC-08`) | **Berhasil:** login atasan ICU; profil bawahan tampil tanpa nominal gaji/rekening/NIK. Buka profil perawat IGD ditolak 403 Forbidden | `CONTRACT` | Test otorisasi dan serialisasi DTO Employee 360 |
| `AT-HRD-A1-23` | Batch data awal sekali seumur hidup (`AC-WFC-09`) | **Gagal:** masukkan pegawai yang sudah pernah masuk batch yang disetujui ke batch baru; validasi baris menolak baris tersebut | `INTEGRATION` | Test validasi batch initial placement |
| `AT-HRD-A1-24` | Tanpa edit/hapus inline di tabel penempatan (`AC-WFC-10`) | **Berhasil:** periksa keenam layar Administrasi Kepegawaian; tidak ada tombol edit atau hapus pada baris tabel | `E2E` | Audit tampilan antarmuka frontend |
| `AT-HRD-A1-25` | Eksekusi SK utuh pada tanggal berlaku (`AC-WFC-11`) | **Berhasil:** SK berisi unit, jabatan, atasan disetujui; pada tanggal berlaku ketiganya berubah bersamaan dan unit lama ditutup H-1 | `INTEGRATION` | Test job terjadwal eksekusi penempatan SK |
| `AT-HRD-A1-26` | Wajib scan SK dan larangan swa-setuju (`AC-WFC-12`) | **Gagal:** ajukan SK tanpa scan SK resmi ditolak 400; pembuat SK mencoba menyetujui sendiri ditolak 403 | `INTEGRATION` | Test validasi pengajuan dan approval SK |
| `AT-HRD-A1-27` | SK hanya batal sebelum tanggal berlaku (`AC-WFC-13`) | **Berhasil:** batalkan SK Dijadwalkan dengan persetujuan; penempatan tidak berubah. **Gagal:** batalkan SK Berlaku ditolak 409 | `INTEGRATION` | Test state transition pembatalan SK |

---

## 3. Layanan mandiri pegawai — `S-A2` s.d. `S-A6`

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-HRD-A2-01` | Pegawai hanya melihat datanya sendiri | **Gagal:** Budi Santoso membuka data cuti Ani Lestari; permintaan ditolak | `INTEGRATION` | Test otorisasi yang membuktikan penolakan |
| `AT-HRD-A2-02` | Angka saldo berasal dari backend | **Berhasil:** angka sisa cuti di layar sama persis dengan yang dikembalikan backend | `E2E` | Langkah reproduksi beserta perbandingan angkanya |
| `AT-HRD-A2-03` | Layar tidak menghitung sendiri sisa cuti | **Gagal:** tidak ada perhitungan sisa cuti di sisi frontend | `MANUAL` | Hasil penelusuran kode frontend beserta kesimpulannya |
| `AT-HRD-A5-01` | Pencatatan kehadiran di luar jendela waktu ditolak | **Gagal:** pencatatan masuk sebelum jendela dibuka ditolak beserta pesan yang terbaca pengguna | `INTEGRATION` | Test penolakan beserta kodenya |
| `AT-HRD-A5-02` | Ambang waktu pulang berasal dari backend | **Berhasil:** tombol catat pulang baru dapat dipakai setelah ambang waktu yang dikembalikan backend terlewati | `E2E` | Langkah reproduksi |
| `AT-HRD-A6-01` | Izin pulang cepat terpisah dari cuti per jam | **Berhasil:** izin pulang cepat tercatat tanpa menyentuh saldo cuti per jam | `INTEGRATION` | Test yang membuktikan saldo cuti tidak bergerak |
| `AT-HRD-A6-02` | Izin pulang cepat melewati persetujuan | **Gagal:** pulang lebih awal tanpa izin yang disetujui menghasilkan pengecualian kehadiran | `INTEGRATION` | Test yang membuktikan pengecualian terbentuk |
| `AT-HRD-A6-03` | Dampak izin terhadap saldo dan pembayaran | — | — | **Belum dapat diuji.** `BLOCKED` oleh `HRD-Q-47` |

---

## 4. Kotak masuk persetujuan terpadu — `S-A7`

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-HRD-A7-01` | Satu kotak masuk melayani seluruh jenis pengajuan | **Berhasil:** satu penyetuju melihat cuti, lembur, koreksi kehadiran, ubah jadwal, dan perubahan profil dalam satu daftar | `INTEGRATION` | Test yang mengembalikan lebih dari satu jenis pengajuan dalam satu jawaban |
| `AT-HRD-A7-01b` | Sama | **Gagal:** penyetuju tidak melihat pengajuan yang tidak ditugaskan kepadanya | `INTEGRATION` | Test otorisasi |
| `AT-HRD-A7-02` | Aturan tiap jenis pengajuan tetap berbeda | **Berhasil:** menolak cuti dan menolak lembur menghasilkan perpindahan status yang berbeda sesuai jenisnya | `INTEGRATION` | Test yang membandingkan status akhir kedua jenis |
| `AT-HRD-A7-03` | Pengingat terkirim saat batas waktu terlampaui | **Berhasil:** tugas yang melewati batas waktu memicu pengingat dan menaikkan hitungan pengingat | `INTEGRATION` | Test pemroses pengingat |
| `AT-HRD-A7-03b` | Sama | **Gagal:** tugas yang belum melewati batas waktu **tidak** memicu pengingat | `INTEGRATION` | Test yang membuktikan hitungan pengingat tetap nol |
| `AT-HRD-A7-04` | Delegasi mengalihkan tugas | **Berhasil:** tugas yang jatuh saat delegasi berlaku muncul di kotak masuk penerima delegasi | `INTEGRATION` | Test penugasan |
| `AT-HRD-A7-04b` | Sama | **Gagal:** delegasi yang sudah lewat masa berlakunya **tidak** mengalihkan tugas | `INTEGRATION` | Test yang membuktikan tugas tetap pada penyetuju asal |
| `AT-HRD-A7-05` | Penyetuju tidak dapat ditentukan menahan pengajuan | **Gagal:** pengajuan dari unit tanpa matriks persetujuan tertahan dan muncul di daftar pengawasan HR | `INTEGRATION` | Test yang membuktikan pengajuan tidak hilang |

---

## 5. Kehadiran — `S-B1`

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-HRD-B1-01` | Rekaman mentah menjadi kehadiran harian | **Berhasil:** satu rekaman masuk dan satu rekaman pulang menghasilkan satu baris kehadiran harian berstatus hasil hitung | `INTEGRATION` | Test pengolahan |
| `AT-HRD-B1-02` | Isi rekaman mentah tidak pernah berubah | **Gagal:** tidak ada satu pun jalur yang menyunting isi rekaman mentah; koreksi hanya memutasi hasil olahan | `INTEGRATION` | Test yang membandingkan isi rekaman sebelum dan sesudah koreksi diterapkan |
| `AT-HRD-B1-03` | Status kehadiran harian tidak dapat disunting langsung | **Gagal:** tidak ada endpoint yang menyunting status kehadiran harian | `CONTRACT` | Hasil penelusuran endpoint beserta kesimpulannya |
| `AT-HRD-B1-04` | Rekaman kembar ditandai, bukan diolah dua kali | **Berhasil:** dua rekaman identik menghasilkan satu hasil olahan; rekaman kedua ditandai kembar | `INTEGRATION` | Test yang menghitung jumlah baris hasil olahan |
| `AT-HRD-B1-05` | Pengecualian pemblokir menahan penutupan periode | **Gagal:** menutup periode yang masih punya pengecualian pemblokir terbuka ditolak beserta daftar penghalangnya | `INTEGRATION` | Test penolakan beserta isi daftar penghalang |
| `AT-HRD-B1-05b` | Sama | **Berhasil:** setelah seluruh penghalang diselesaikan, penutupan berhasil | `INTEGRATION` | Test alur lengkap |
| `AT-HRD-B1-06` | Bekerja di luar jadwal terdeteksi sebagai jenis pengecualian tersendiri | **Berhasil:** kehadiran dokter di luar jendela jadwalnya menghasilkan pengecualian berjenis bekerja di luar jadwal | `INTEGRATION` | Test pendeteksian |
| `AT-HRD-B1-06b` | Sama | **Gagal:** jadwal yang tidak dapat diselesaikan menghasilkan jenis pengecualian yang **berbeda**, bukan jenis bekerja di luar jadwal | `INTEGRATION` | Test yang membandingkan kedua jenis |
| `AT-HRD-B1-07` | Bekerja di luar jadwal tidak otomatis menjadi lembur | **Gagal:** pengecualian bekerja di luar jadwal **tidak** membentuk permohonan maupun realisasi lembur sampai atasan mengklasifikasikannya | `INTEGRATION` | Test yang membuktikan tidak ada lembur yang terbentuk |
| `AT-HRD-B1-08` | Alur persetujuan koreksi kehadiran | **Berhasil:** koreksi Ani Lestari disetujui lalu diterapkan; kehadiran harinya berubah | `INTEGRATION` | Test alur lengkap |
| `AT-HRD-B1-09` | Permohonan `Applied` tidak dapat turun statusnya | **Gagal:** sinkronisasi terhadap permohonan berstatus `Applied` **tidak** menurunkannya ke `Approved`; kehadiran harian **tidak** dimutasi ulang | `INTEGRATION` | Test yang membandingkan status dan isi kehadiran sebelum dan sesudah sinkronisasi. **Ini menutup cacat yang tercatat pada `HRD-DEC-022`** |
| `AT-HRD-B1-10` | Pengajuan HR atas nama pegawai menyimpan pengetiknya | **Berhasil:** permohonan yang dibuat HR atas nama Budi Santoso menyimpan akun HR sebagai pengetik, dan Budi sebagai pemilik data | `INTEGRATION` | Test yang memeriksa kedua kolom |
| `AT-HRD-B1-10b` | Sama | **Gagal:** pengajuan atas nama tanpa alasan ditolak | `UNIT` | Test validasi alasan wajib |
| `AT-HRD-B1-11` | Penerapan koreksi ditolak bila periode tertutup | **Gagal:** penerapan pada periode tertutup ditolak beserta pesan yang terbaca pengguna | `INTEGRATION` | Test penolakan beserta kodenya |

---

## 6. Cuti — `S-B2`

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-HRD-B2-01` | Alur permohonan cuti | **Berhasil:** permohonan cuti Ani Lestari disetujui, cuti berjalan, saldo terpotong, hari kehadiran ditandai cuti | `INTEGRATION` | Test alur lengkap |
| `AT-HRD-B2-01b` | Sama | **Gagal:** permohonan melebihi sisa saldo ditolak sebelum sampai ke penyetuju | `INTEGRATION` | Test penolakan beserta kodenya |
| `AT-HRD-B2-02` | Setiap pergerakan saldo meninggalkan baris buku besar | **Berhasil:** pemotongan, pengembalian, dan penyesuaian saldo masing-masing membentuk satu baris buku besar | `INTEGRATION` | Test yang menghitung baris buku besar |
| `AT-HRD-B2-02b` | Sama | **Gagal:** tidak ada jalur yang mengubah angka saldo tanpa membentuk baris buku besar | `INTEGRATION` | Test yang membandingkan angka saldo dengan jumlah buku besarnya |
| `AT-HRD-B2-03` | Pembalikan pelaksanaan cuti wajib beralasan | **Gagal:** pembalikan tanpa alasan ditolak | `UNIT` | Test validasi alasan wajib |
| `AT-HRD-B2-03b` | Sama | **Berhasil:** pembalikan beralasan tercatat beserta siapa yang membalik dan kapan | `INTEGRATION` | Test yang memeriksa ketiga kolom pembalikan |
| `AT-HRD-B2-04` | Pengakuan penarikan bukan penghalang | **Berhasil:** penarikan tetap dapat diterapkan meski pegawai belum mengakui, dengan alasan pelewatan tercatat | `INTEGRATION` | Test alur penarikan |
| `AT-HRD-B2-04b` | Sama | **Gagal:** pelewatan pengakuan tanpa alasan ditolak | `UNIT` | Test validasi alasan wajib |
| `AT-HRD-B2-05` | Backend otoritatif atas angka saldo | **Berhasil:** angka di layar sama persis dengan yang dikembalikan backend | `E2E` | Langkah reproduksi beserta perbandingan angkanya |
| `AT-HRD-B2-06` | Pembatalan cuti mengembalikan saldo | **Berhasil:** pembatalan yang disetujui mengembalikan saldo penuh atau sebagian sesuai kebijakan | `INTEGRATION` | Test yang memeriksa angka saldo sesudahnya |

---

## 7. Lembur — `S-B3`

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-HRD-B3-01` | Alur permohonan lembur | **Berhasil:** permohonan disetujui, lembur dikerjakan, realisasi diverifikasi, lalu diteruskan ke payroll | `INTEGRATION` | Test alur lengkap |
| `AT-HRD-B3-01b` | Sama | **Gagal:** permohonan melebihi batas kebijakan ditolak | `INTEGRATION` | Test penolakan beserta kodenya |
| `AT-HRD-B3-02` | Realisasi dibuktikan data kehadiran | **Gagal:** realisasi **tidak** terbentuk untuk hari yang tidak punya kehadiran tercatat | `INTEGRATION` | Test yang membuktikan realisasi tidak terbentuk |
| `AT-HRD-B3-02b` | Sama | **Berhasil:** setelah kehadiran hari itu dikoreksi, realisasi terbentuk | `INTEGRATION` | Test alur lengkap |
| `AT-HRD-B3-03` | Jam yang dibayar tidak melebihi yang tercatat | **Gagal:** permohonan empat jam dengan kehadiran tercatat tiga jam menghasilkan realisasi **tiga** jam | `INTEGRATION` | Test yang memeriksa angka realisasi |
| `AT-HRD-B3-04` | Cuti pengganti terbit dari lembur terverifikasi | **Berhasil:** realisasi terverifikasi menghasilkan hak cuti pengganti dengan masa berlaku | `INTEGRATION` | Test penerbitan hak cuti pengganti |
| `AT-HRD-B3-04b` | Sama | **Gagal:** realisasi yang belum terverifikasi **tidak** menerbitkan hak cuti pengganti | `INTEGRATION` | Test yang membuktikan tidak ada hak yang terbit |

---

## 8. Penjadwalan kerja — `S-B4`

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-HRD-B4-01` | Roster disusun, diperiksa, lalu diterbitkan | **Berhasil:** roster satu unit lolos pemeriksaan bentrok lalu terbit; pegawai melihat jadwalnya | `INTEGRATION` | Test alur lengkap |
| `AT-HRD-B4-01b` | Sama | **Gagal:** penerbitan roster yang masih punya bentrok penghalang ditolak beserta daftar bentroknya | `INTEGRATION` | Test penolakan beserta isi daftar bentrok |
| `AT-HRD-B4-02` | Jadwal berlaku surut tidak dapat disunting langsung | **Gagal:** penyuntingan jadwal pada tanggal yang kehadirannya sudah diproses ditolak | `INTEGRATION` | Test penolakan beserta kodenya |
| `AT-HRD-B4-03` | Jadwal kerja bukan sumber jadwal praktik dokter | **Gagal:** tidak ada endpoint HR yang menjadi sumber jadwal praktik untuk pendaftaran pasien | `MANUAL` | Hasil penelusuran endpoint beserta kesimpulannya |
| `AT-HRD-B4-04` | Alur permohonan ubah jadwal | **Berhasil:** permohonan disetujui lalu diterapkan; jadwal berubah | `INTEGRATION` | Test alur lengkap |
| `AT-HRD-B4-04b` | Sama | **Gagal:** penerapan yang menimbulkan bentrok ditolak; jadwal **tidak** berubah | `INTEGRATION` | Test yang membuktikan jadwal tidak berubah |
| `AT-HRD-B4-05` | Alur permohonan tukar shift | **Berhasil:** rekan menyetujui, atasan menyetujui, kedua jadwal bertukar dalam satu tindakan | `INTEGRATION` | Test yang memeriksa kedua jadwal sesudahnya |
| `AT-HRD-B4-05b` | Sama | **Gagal:** bila salah satu sisi bentrok, **tidak ada** jadwal yang berubah — bukan sebagian | `INTEGRATION` | Test yang membuktikan kedua jadwal utuh seperti semula |
| `AT-HRD-B4-06` | Tukar shift memerlukan persetujuan rekan | **Gagal:** permohonan yang belum dijawab rekan **tidak** sampai ke atasan | `INTEGRATION` | Test yang membuktikan tidak ada tugas persetujuan yang terbentuk |
| `AT-HRD-B4-07` | Resolusi jadwal harian dipakai pengolahan kehadiran | **Berhasil:** kehadiran pada tanggal berjadwal diolah memakai shift yang terbit untuk pegawai itu | `INTEGRATION` | Test pengolahan yang memeriksa shift acuannya |

---

## 9. Payroll sisi HR — `S-B5`

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-HRD-B5-01` | Masukan HR siap payroll — `HRD-DEC-035` | **Berhasil:** kesiapan kehadiran, cuti, dan lembur terkumpul dan tervalidasi sebagai masukan yang siap diserahkan | `INTEGRATION` | Test kesiapan masukan. **Orkestrasi putaran payroll di luar cakupan MVP** |
| `AT-HRD-B5-01b` | Sama | **Gagal:** putaran payroll ditolak berjalan bila periode kehadiran belum ditutup | `INTEGRATION` | Test penolakan beserta kodenya |
| `AT-HRD-B5-02` | Tanggung jawab HR berhenti setelah serah terima | **Gagal:** tidak ada endpoint HR yang menyimpan hasil pembayaran, jurnal akuntansi, atau perhitungan pajak | `MANUAL` | Hasil penelusuran endpoint beserta kesimpulannya |
| `AT-HRD-B5-03` | Payroll memakai kehadiran final | **Berhasil:** angka masukan payroll sama persis dengan kehadiran harian pada periode yang sudah ditutup | `INTEGRATION` | Test yang membandingkan kedua angka |
| `AT-HRD-B5-04` | Serah terima yang diulang tidak menghasilkan pengiriman ganda | **Berhasil:** menjalankan serah terima dua kali menghasilkan satu pengiriman | `INTEGRATION` | Test yang menghitung jumlah pengiriman |
| `AT-HRD-B5-05` | Bentuk data yang diterima Finance | — | — | **Di luar cakupan MVP** sejak `HRD-DEC-035`. Tetap `BLOCKED` oleh `HRD-Q-10`, tetapi **tidak** memblokir MVP administratif |
| `AT-HRD-B5-06` | Perilaku bila Finance menolak batch | — | — | **Di luar cakupan MVP** sejak `HRD-DEC-035`. Tetap `BLOCKED` oleh `HRD-Q-11` |
| `AT-HRD-B5-07` | Orkestrasi putaran payroll | — | — | **`POST-MVP`** sesuai `HRD-DEC-035`. Tidak ada jalur yang membuat putaran payroll hari ini |

---

## 10. Pengembangan orang dan lifecycle — `S-C2` s.d. `S-C5`

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-HRD-C2-01` | Pelatihan wajib sampai sertifikat terbit | **Berhasil:** peserta yang lulus asesmen mendapat sertifikat dengan masa berlaku | `INTEGRATION` | Test alur lengkap |
| `AT-HRD-C2-01b` | Sama | **Gagal:** peserta yang tidak memenuhi syarat kehadiran **tidak** mendapat sertifikat | `INTEGRATION` | Test yang membuktikan sertifikat tidak terbit |
| `AT-HRD-C2-02` | Sertifikat kedaluwarsa memberi peringatan, bukan penghentian | **Berhasil:** sertifikat yang lewat masa berlakunya menghasilkan peringatan tercatat | `INTEGRATION` | Test yang memeriksa peringatan |
| `AT-HRD-C2-02b` | Sama | **Gagal:** kedaluwarsanya sertifikat **tidak** menghentikan akses pegawai ke pekerjaannya | `INTEGRATION` | Test yang membuktikan tidak ada penghentian |
| `AT-HRD-C3-01` | Siklus penilaian kinerja | **Berhasil:** sasaran disepakati, penilaian diisi, hasil disampaikan, siklus selesai | `INTEGRATION` | Test alur lengkap |
| `AT-HRD-C3-01b` | Sama | **Gagal:** penilaian tanpa butir wajib ditolak | `UNIT` | Test validasi |
| `AT-HRD-C3-02` | Perubahan hasil wajib beralasan | **Gagal:** penyesuaian hasil tanpa alasan ditolak | `UNIT` | Test validasi alasan wajib |
| `AT-HRD-C3-03` | Isi penilaian tidak terbaca pihak yang tidak berwenang | **Gagal:** rekan sejawat membuka isi penilaian orang lain; permintaan ditolak | `INTEGRATION` | Test otorisasi |
| `AT-HRD-C3-04` | Tahap siklus mengikuti urutan | **Gagal:** melompat ke tahap akhir tanpa melewati penilaian ditolak | `INTEGRATION` | Test penolakan. **Ini menutup temuan urutan tahap yang belum dijaga** |
| `AT-HRD-C4-01` | Alur pengunduran diri | **Berhasil:** permohonan disetujui, serah terima selesai, hak tuntas, penutupan berhasil | `INTEGRATION` | Test alur lengkap |
| `AT-HRD-C4-01b` | Sama | **Gagal:** penutupan ditolak bila masih ada butir serah terima yang belum selesai | `INTEGRATION` | Test penolakan beserta daftar butir yang tertinggal |
| `AT-HRD-C4-02` | Hak yang tersisa diselesaikan sebelum penutupan | **Gagal:** penutupan ditolak bila masih ada saldo cuti atau realisasi lembur yang menggantung | `INTEGRATION` | Test penolakan |
| `AT-HRD-C4-03` | Pencabutan akses lewat Identity | **Berhasil:** penutupan menghasilkan permintaan pencabutan akses yang tercatat | `INTEGRATION` | Test yang memeriksa permintaan tercatat |
| `AT-HRD-C5-01` | Alur tindakan kedisiplinan | **Berhasil:** laporan ditelaah, kasus diselidiki, tindakan disetujui lalu berlaku | `INTEGRATION` | Test alur lengkap |
| `AT-HRD-C5-01b` | Sama | **Gagal:** kasus tanpa bukti mencukupi ditutup tanpa tindakan; laporannya **tetap tercatat** | `INTEGRATION` | Test yang membuktikan laporan tidak terhapus |
| `AT-HRD-C5-02` | Laporan dan sanggahan tidak dapat dihapus | **Gagal:** tidak ada jalur yang menghapus laporan maupun sanggahan secara permanen | `INTEGRATION` | Test yang membuktikan baris tetap ada setelah penutupan |
| `AT-HRD-C5-03` | Pegawai diberi kesempatan menjelaskan | **Gagal:** tindakan yang diputuskan tanpa penjelasan pegawai tercatat ditolak | `INTEGRATION` | Test penolakan |
| `AT-HRD-C5-04` | Pemisahan peran pengusul dan penyetuju | — | — | **Belum dapat diuji.** `BLOCKED` oleh `HRD-Q-51` |
| `AT-HRD-C5-05` | Tingkatan izin data paling terbatas | — | — | **Belum dapat diuji.** `BLOCKED` oleh `HRD-Q-52` |

---

## 11. Aturan lintas-slice — `S-E`

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-HRD-E-01` | Ratchet penamaan hanya saat entity benar-benar disentuh | **Berhasil:** task yang mengubah skema sebuah entity berprefix `Trx` menaikkan namanya menjadi berprefix `Hrd` | `MANUAL` | Catatan task beserta rujukan `HRD-DEC-019` |
| `AT-HRD-E-02` | Ratchet tidak dipicu pekerjaan yang tidak menyentuh skema | **Gagal:** task frontend, pembacaan data, dokumentasi, atau perbaikan bug yang tidak mengubah kontrak persistence **tidak** memicu penggantian nama | `MANUAL` | Catatan tinjauan |
| `AT-HRD-E-03` | Prefix `Wfp` dan `Mst` tidak diubah | **Gagal:** usulan mengganti `Wfp` atau `Mst` menjadi `Hrd` ditolak pada tinjauan | `MANUAL` | Catatan tinjauan yang menyebut `HRD-DEC-019` |
| `AT-HRD-E-04` | Tidak ada kampanye penggantian nama massal | **Gagal:** tidak ada satu pun task yang menargetkan seluruh entity berprefix `Trx` sekaligus | `MANUAL` | Hasil penelusuran roadmap beserta kesimpulannya |

---

## 11.1 Keamanan data gaji dan slip gaji — `HRD-DEC-037` s.d. `HRD-DEC-040`

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-HRD-SAL-01` | Slip gaji hanya milik sendiri | **Gagal:** Budi meminta slip gaji Ani memakai pengenal yang benar; ditolak `403`, isi tidak terkirim, dan jawaban **tidak** membocorkan apakah slip itu ada | `INTEGRATION` | Test otorisasi beserta isi jawaban yang diperiksa |
| `AT-HRD-SAL-01b` | Kepemilikan diturunkan backend | **Gagal:** permintaan menyertakan pengenal profil workforce milik orang lain; pengenal itu **diabaikan**, yang dikembalikan tetap milik pengguna yang masuk | `INTEGRATION` | Test yang membuktikan pengenal dari layar tidak dipakai |
| `AT-HRD-SAL-02` | Otentikasi bertingkat wajib | **Gagal:** data gaji diminta tanpa sesi gaji sensitif; ditolak `401` | `INTEGRATION` | Test penolakan |
| `AT-HRD-SAL-02b` | Sama | **Berhasil:** setelah kata sandi dikonfirmasi, data gaji dapat dibaca | `INTEGRATION` | Test alur lengkap |
| `AT-HRD-SAL-03` | Sesi sensitif kedaluwarsa | **Gagal:** setelah lima menit, permintaan berikutnya ditolak `401` dan meminta kata sandi lagi | `INTEGRATION` | Test batas waktu |
| `AT-HRD-SAL-03b` | Sesi batal saat keadaan akun berubah | **Gagal:** setelah pengguna keluar atau akun dinonaktifkan, sesi sensitif tidak lagi berlaku | `INTEGRATION` | Test pembatalan |
| `AT-HRD-SAL-04` | Memakai Identity canonical | **Gagal:** tidak ada pemverifikasi kata sandi kedua di dalam modul HR | `MANUAL` | Hasil penelusuran source beserta kesimpulannya |
| `AT-HRD-SAL-05` | Kata sandi tidak disimpan | **Gagal:** setelah konfirmasi, kata sandi tidak ditemukan di `localStorage`, `sessionStorage`, Redux yang dipersistkan, log, maupun analytics | `E2E` | Isi penyimpanan peramban dan log yang diperiksa |
| `AT-HRD-SAL-06` | Konfigurasi kebijakan gaji hanya HR Manager | **Gagal:** peran selain HR Manager membaca atau mengubah master gaji; ditolak `403` | `INTEGRATION` | Test otorisasi |
| `AT-HRD-SAL-07` | Riwayat kebijakan tidak dihapus | **Berhasil:** kebijakan gaji diubah; versi lama **tetap ada** dan dapat dirujuk beserta tanggal berlakunya | `INTEGRATION` | Test yang membuktikan versi lama masih terbaca |
| `AT-HRD-SAL-08` | Audit `GET` sensitif | **Berhasil:** membaca nominal gaji meninggalkan catatan audit berisi pelaku, sasaran, aksi, waktu, dan hasil | `INTEGRATION` | Isi catatan audit |
| `AT-HRD-SAL-08b` | Audit tidak memuat nominal | **Gagal:** catatan audit **tidak** memuat nominal gaji, kata sandi, nomor rekening, nomor pajak, isi response, maupun token | `INTEGRATION` | Isi catatan audit yang diperiksa kolom per kolom |
| `AT-HRD-SAL-08c` | Percobaan yang ditolak ikut dicatat | **Berhasil:** permintaan slip gaji milik orang lain meninggalkan catatan audit bertanda gagal | `INTEGRATION` | Isi catatan audit |
| `AT-HRD-SAL-09` | Response gaji tidak disimpan cache | **Gagal:** response gaji sensitif membawa `Cache-Control: no-store`, dan tidak ditemukan pada cache peramban maupun Redux yang dipersistkan | `E2E` | Header response dan isi penyimpanan peramban |
| `AT-HRD-SAL-10` | Unduhan slip gaji terautentikasi | **Gagal:** berkas slip gaji tidak dapat diambil lewat URL statis yang ditebak tanpa otentikasi, kepemilikan, dan sesi sensitif | `INTEGRATION` | Test yang mencoba URL langsung |
| `AT-HRD-SAL-11` | Keterlihatan massal tidak tersedia | **Gagal:** tidak ada endpoint yang mengembalikan nominal gaji banyak pegawai sekaligus, dan `ViewAmountBulk` tidak dipegang peran mana pun | `MANUAL` | Hasil penelusuran endpoint dan konfigurasi peran |
| `AT-HRD-SAL-12` | HR Manager tetap tidak dapat swa-setuju | **Gagal:** HR Manager yang membuat penetapan gaji tidak dapat menyetujuinya sendiri; ditolak `403` | `INTEGRATION` | Test yang membuktikan `HRD-DEC-031` tidak dilonggarkan `HRD-DEC-037` |
| `AT-HRD-SAL-13` | Payroll Officer tidak memegang `: ViewAmount` — `HRD-DEC-044` | **Gagal:** Payroll Officer meminta nominal gaji; ditolak. Kesiapan payroll tetap dapat dikerjakannya | `INTEGRATION` | Test otorisasi beserta bukti kesiapan payroll tetap berjalan |
| `AT-HRD-SAL-14` | Pendidikan belum terverifikasi tidak dipakai kebijakan gaji — `HRD-DEC-041` | **Gagal:** jenjang yang dilaporkan tetapi belum diverifikasi tidak menghasilkan calon penyesuaian | `INTEGRATION` | Test yang membuktikan tidak ada calon terbentuk |
| `AT-HRD-SAL-14b` | Sama | **Berhasil:** setelah HR memverifikasi, evaluasi berjalan dan calon penyesuaian terbentuk | `INTEGRATION` | Test alur lengkap |
| `AT-HRD-SAL-15` | Verifikasi pendidikan bukan oleh pencatatnya | **Gagal:** pihak yang mencatat pendidikan tidak dapat memverifikasinya sendiri | `INTEGRATION` | Test pemisahan peran. **Menutup celah butir `WorkforceEducation : Update` yang sekarang dipakai bersama** |
| `AT-HRD-SAL-16` | Perubahan faktor tidak mengubah gaji langsung — `HRD-DEC-043` | **Gagal:** pendidikan terverifikasi naik **tidak** mengubah gaji efektif; hanya membentuk calon | `INTEGRATION` | Test yang membandingkan nilai gaji sebelum dan sesudah |
| `AT-HRD-SAL-16b` | Sama | **Berhasil:** calon diterima → pengajuan `T8` → disetujui pihak berbeda → gaji berubah sejak tanggal berlaku | `INTEGRATION` | Test alur lengkap |
| `AT-HRD-SAL-17` | Kebijakan gaji berversi | **Berhasil:** kebijakan diubah; versi lama tetap ada dan penetapan gaji lama tetap menunjuk versi yang menjadi dasarnya | `INTEGRATION` | Test yang membuktikan riwayat utuh |
| `AT-HRD-SAL-17b` | Pertanyaan audit terjawab | **Berhasil:** untuk satu perubahan gaji, sistem menampilkan rekaman faktor, versi kebijakan, tanggal berlaku, penyesuaian, pelaku dan penyetuju | `INTEGRATION` | Isi jejak audit |
| `AT-HRD-SAL-18` | Masa kerja bukan faktor gaji MVP — `HRD-DEC-045` | **Gagal:** tidak ada kriteria masa kerja, pita masa kerja, maupun penyesuaian gaji yang dipicu masa kerja pada kontrak sasaran MVP | `MANUAL` | Hasil penelusuran kontrak dan tinjauan desain |
| `AT-HRD-SAL-19` | Aturan gaji tidak ditulis di source | **Gagal:** tidak ada persentase maupun nominal penyesuaian yang ditulis sebagai percabangan di kode | `MANUAL` | Hasil penelusuran source |

---

## 12. Privasi dan jejak audit — berlaku lintas slice

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-HRD-SEC-01` | Kolom sensitif tidak masuk custom logger | **Gagal:** memicu jalur yang menyentuh kolom bertanda sensitif tidak meninggalkan isinya di log | `INTEGRATION` | Isi log yang diperiksa, dengan nilai tersamarkan |
| `AT-HRD-SEC-02` | Permintaan selain `GET` meninggalkan jejak | **Berhasil:** setiap permintaan yang mengubah data meninggalkan catatan siapa, kapan, dan apa yang diubah | `INTEGRATION` | Isi catatan audit |
| `AT-HRD-SEC-03` | Permintaan `GET` tidak dicatat, **kecuali data gaji sensitif** | **Berhasil:** pembacaan data biasa tidak membanjiri catatan audit. **Gagal:** pembacaan nominal gaji dan slip gaji **tetap** dicatat sesuai `HRD-DEC-039` | `INTEGRATION` | Isi catatan audit untuk kedua jenis pembacaan |
| `AT-HRD-SEC-04` | Hak akses pada API dan frontend sama | **Gagal:** tidak ada tombol di layar yang dijaga hak akses berbeda dari endpoint yang dipanggilnya | `MANUAL` | Perbandingan baris pada `contracts/api-contract.md` dengan skema fitur pada `03-frontend-architecture.md` |
| `AT-HRD-SEC-05` | Penghapusan bersifat penandaan | **Gagal:** tidak ada jalur yang benar-benar menghapus baris dari basis data | `INTEGRATION` | Test yang membuktikan baris masih ada dengan penanda hapus |

---

## 13. Kemampuan yang sengaja TIDAK punya baris di matriks ini

| Kemampuan | Slice | Alasan |
| --- | --- | --- |
| Kredensial, kewenangan klinis, SPK/RKK, OPPE, FPPE | `S-C1` | `BLOCKED`. Menuliskan skenario ujinya berarti menetapkan kewenangan praktik yang belum ada — `HRD-Q-08` |
| Kesehatan dan keselamatan kerja staf | `S-C6` | `BLOCKED`. Aturan aksesnya belum disahkan K3RS — `HRD-DEC-010` masih `draft` |
| Rekrutmen, benefit, layanan HR | `S-D2` s.d. `S-D4` | `BLOCKED` oleh `HRD-Q-05`. Perencanaan tenaga kerja (`S-D1`) kini punya bagian 15 |
| Perjalanan dinas dan reimbursement | `S-D5` | `DEFERRED`, dan tetap terikat `HRD-Q-05` |

**Ketiadaan baris untuk keempat kelompok di atas adalah batas yang disengaja**, bukan cakupan
pengujian yang terlewat.

---

## 14. Rekapitulasi

| Kelompok | Baris dapat diuji | Baris `BLOCKED` |
| --- | ---: | ---: |
| Fondasi `S0-A`, `S0-B` | 5 | 0 |
| Administrasi kepegawaian `S-A1` | 7 | 0 |
| Layanan mandiri `S-A2` s.d. `S-A6` | 6 | 1 |
| Kotak masuk persetujuan `S-A7` | 8 | 0 |
| Kehadiran `S-B1` | 14 | 0 |
| Cuti `S-B2` | 10 | 0 |
| Lembur `S-B3` | 7 | 0 |
| Penjadwalan kerja `S-B4` | 10 | 0 |
| Payroll sisi HR `S-B5` | 4 | 2 |
| Pengembangan orang dan lifecycle `S-C2` s.d. `S-C5` | 16 | 2 |
| Aturan lintas-slice `S-E` | 4 | 0 |
| Privasi dan audit | 5 | 0 |
| Perencanaan tenaga kerja `S-D1` — bagian 15 | 27 | 0 |
| **Jumlah** | **123** | **5** |

Setiap kelompok yang dapat diuji memuat sekurang-kurangnya satu skenario berhasil dan satu
skenario gagal, sesuai aturan pada bagian 0.1.

---

## 15. Perencanaan tenaga kerja — `S-D1` (`last_changed_in` `v7`, 3 November 2026)

Kolom "AC asal" merujuk acceptance criteria decision log bagian 32.8. Backend tidak memelihara
proyek automated test (`rules/backend/TEST_POLICY.md`); jenis `INTEGRATION` dibuktikan lewat uji API
terhadap database dev dan dicatat di laporan task.

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan | AC asal |
| --- | --- | --- | --- | --- | --- |
| `AT-HRD-D1-01` | Pengaman tabel kosong pada migration pembersihan (`HRD-DEC-059`) | **Gagal:** isi 1 baris `MstPositionHeadcountPlan` di database uji, jalankan migration pembersihan; migration berhenti dan menyebut nama tabel serta jumlah barisnya, tanpa perubahan apa pun | `MIGRATION` | Keluaran migration dan hitung baris sesudahnya | `AC-WFP-01` |
| `AT-HRD-D1-01b` | Sama | **Berhasil:** pada database tanpa isi, migration berjalan | `MIGRATION` | Keluaran migration | `AC-WFP-01` |
| `AT-HRD-D1-02` | Tiga metode dan rincian rekomendasi (`HRD-DEC-060`) | **Berhasil:** standar ICU rasio 1:1, angka beban 7, 3 shift, faktor 1,5, pembulatan ke atas → rekomendasi 32 dengan rincian "7 ÷ 1 × 1 = 7 per shift; 7 × 3 = 21; 21 × 1,5 = 31,5 → 32" | `INTEGRATION` | Response rekomendasi | `AC-WFP-02` |
| `AT-HRD-D1-02b` | Sama | **Gagal:** memilih metode Analisis Beban Kerja ditolak `VR-WFP-02`; mengajukan tanpa faktor cadangan ditolak `VR-WFP-04` | `UNIT` | Pesan penolakan | `AC-WFP-02` |
| `AT-HRD-D1-03` | Standar belum sah tidak dipakai | **Gagal:** standar versi 2 masih `Submitted`; gap dan rekomendasi tetap memakai versi 1 | `INTEGRATION` | Response gap menyebut versi standar yang dipakai | `AC-WFP-03` |
| `AT-HRD-D1-04` | Pengesahan `T9` sampai berlaku | **Berhasil:** standar disetujui HR Manager lalu Direktur; berlaku pada tanggalnya dan versi lama `Superseded` sehari sebelumnya | `INTEGRATION` | Status kedua versi sesudah scheduler berjalan | — |
| `AT-HRD-D1-05` | Rangkap/Plt tidak terhitung dua kali, porsi kerja dihitung (`HRD-DEC-061`) | **Berhasil:** Sari utama di Rawat Inap dan Plt di ICU tidak menambah terisi ICU; Budi 50% terhitung 0,5 | `INTEGRATION` | Response gap ICU | `AC-WFP-04` |
| `AT-HRD-D1-05b` | Data belum ada tidak tampil 0 | **Gagal pemalsuan:** dengan pengaturan cuti panjang kosong, lapisan "tidak tersedia sementara" bernilai `null` dan baris bertanda belum lengkap; layar menulis "data belum tersedia" | `INTEGRATION`, `UI` | Response dan tangkapan layar | `AC-WFP-05` |
| `AT-HRD-D1-06` | Formasi tidak dapat diedit langsung (`HRD-DEC-062`) | **Gagal:** `PUT` atau `DELETE` ke formasi sebagai HR Manager menjawab `405`; layar Formasi Jabatan tidak punya tombol ubah | `INTEGRATION`, `UI` | Response dan tangkapan layar | `AC-WFP-06` |
| `AT-HRD-D1-07` | Formasi tidak kosong saat pergantian tahun | **Berhasil:** tanpa Rencana 2028 yang disetujui, formasi pada 1 Januari 2028 sama dengan versi terakhir 2027 | `INTEGRATION` | Response formasi berlaku per tanggal | `AC-WFP-07` |
| `AT-HRD-D1-08` | Sisa kursi dijaga (`HRD-DEC-063`) | **Gagal:** sisa kursi 0,5, permintaan dalam formasi 1 kursi penuh ditolak `VR-WFP-22` dengan angka sisa kursi | `INTEGRATION` | Response `422` | `AC-WFP-08` |
| `AT-HRD-D1-08b` | Sama | **Berhasil:** permintaan 1 kursi berporsi 0,5 diterima | `INTEGRATION` | Response `201` lalu `Submitted` | `AC-WFP-08` |
| `AT-HRD-D1-09` | Tidak ada klaim kursi ganda | **Gagal:** dua permintaan diajukan bersamaan atas sisa kursi 1; tepat satu lolos, satu ditolak | `INTEGRATION` | Dua response; hitung kursi `Submitted` | `AC-WFP-09` |
| `AT-HRD-D1-10` | Tidak ada swa-setuju dan tidak ada langkah dilewati (`HRD-DEC-064`) | **Gagal:** Kepala ICU yang juga HR Manager tidak muncul sebagai penyetuju langkah HR Manager; dengan peran Finance belum dipetakan, permintaan `T12` ditolak `400` saat diajukan | `INTEGRATION` | Daftar penyetuju instance; response `400` | `AC-WFP-10` |
| `AT-HRD-D1-11` | Biaya dari gaji tengah grade dan dibatasi (`HRD-DEC-065`) | **Berhasil:** +2 grade III × 9 bulan = 2 × gaji tengah × 9. **Gagal:** Petugas payroll dan Recruiter tidak menerima kolom biaya; Kepala Unit lain menerima `404` | `INTEGRATION` | Response per peran | `AC-WFP-11` |
| `AT-HRD-D1-12` | Kursi terisi saat penempatan berlaku (`HRD-DEC-066`) | **Berhasil:** penempatan Wati berlaku 2 Mei; sampai 1 Mei kursi `Linked`, sesudah scheduler 2 Mei `Fulfilled` dan permintaan `PartiallyFulfilled` | `INTEGRATION` | Status kursi dan permintaan per tanggal | `AC-WFP-12` |
| `AT-HRD-D1-13` | Penempatan tidak cocok ditolak | **Gagal:** menautkan penempatan Rawat Inap ke permintaan ICU ditolak `VR-WFP-29`; menautkan penempatan yang sudah dipakai kursi lain ditolak `VR-WFP-30` | `INTEGRATION` | Response `422` dan `409` | `AC-WFP-13` |
| `AT-HRD-D1-14` | Rencana pemenuhan luar dan internal (`HRD-DEC-067`) | **Berhasil:** permintaan +6 dengan 4 luar + 2 internal; antrean Recruiter memuat 4. HR Manager mengubah satu kursi internal ke luar; antrean memuat 5. **Gagal:** HR Manager pembuat permintaan mencoba mengubah; ditolak `VR-WFP-33` | `INTEGRATION` | Isi antrean; response `403` | `AC-WFP-14` |
| `AT-HRD-D1-15` | Rencana terkunci, addendum melahirkan versi (`HRD-DEC-068`) | **Gagal:** mengubah Rencana 2027 yang `Approved` ditolak `VR-WFP-17`. **Berhasil:** Addendum ICU −2 disetujui; formasi ICU turun pada tanggal berlakunya dan permintaan berjalan yang berlebih ditandai | `INTEGRATION` | Response dan riwayat formasi | `AC-WFP-15` |
| `AT-HRD-D1-16` | Potret terkunci dan tampil berdampingan (`HRD-DEC-069`) | **Berhasil:** ajukan saat gap 3,5; satu orang pindah masuk; penyetuju melihat "saat diajukan 3,5 — sekarang 2,5". **Gagal:** endpoint ubah potret tidak ada (`405`) | `INTEGRATION`, `UI` | Response potret dan gap langsung | `AC-WFP-16` |
| `AT-HRD-D1-16b` | Potret bulanan yang terlewat tidak dipalsukan | **Gagal pemalsuan:** scheduler dimatikan sepanjang Februari; bulan itu tampil "potret tidak tersedia"; pengisian susulan berjenis `Reconstructed` | `INTEGRATION` | Daftar potret | — |
| `AT-HRD-D1-17` | Batch formasi awal berdokumen dan sekali per unit × jabatan (`HRD-DEC-070`) | **Gagal:** mengajukan tanpa scan ditolak `VR-WFP-13`; mengajukan dengan baris tidak valid ditolak `VR-WFP-12`; memasukkan unit × jabatan yang sudah berformasi ditolak `VR-WFP-11`. **Berhasil:** batch disetujui lewat `T10` melahirkan versi 1, dan penyetujunya bukan pengaju | `INTEGRATION` | Response dan formasi versi 1 | `AC-WFP-17` |
| `AT-HRD-D1-18` | Pembatalan kursi (`HRD-DEC-071`) | **Gagal:** membatalkan kursi dengan lowongan terbuka ditolak `VR-WFP-36`; membatalkan kursi `Fulfilled` ditolak `VR-WFP-35`. **Berhasil:** sesudah lowongan ditutup, pembatalan `T13` disetujui dan formasi 32 menjadi 31 | `INTEGRATION` | Response dan riwayat formasi | `AC-WFP-18` |
| `AT-HRD-D1-19` | Sub-modul 03 tidak mencatat penugasan per shift (`HRD-DEC-058`) | **Berhasil:** tidak ada endpoint `S-D1` yang menerima atau mengembalikan penugasan orang per shift; Penjadwalan membaca standar lewat `GET .../staffing-standards/effective` | `INTEGRATION` | Daftar endpoint Swagger grup Workforce Planning | `AC-WFP-19` |
| `AT-HRD-D1-20` | Direktur dari matriks persetujuan (`HRD-DEC-072`) | **Berhasil:** permintaan ICU di luar formasi sampai ke pemegang jabatan Direktur Pelayanan Medis lewat penelusuran unit induk. **Gagal tertutup:** jabatan direktur kosong → naik ke Direktur Utama; keduanya kosong → pengajuan ditolak | `INTEGRATION` | Penyetuju pada instance; response `400` | — |
| `AT-HRD-D1-21` | Koreksi biaya Finance (`HRD-DEC-065` `FINANCE_ADJUSTMENT`) | **Berhasil:** Finance mengisi koreksi pada langkahnya; angka awal dan koreksi sama-sama tersimpan. **Gagal:** HR Manager mencoba koreksi ditolak `VR-WFP-20`; Finance menyetujui padahal ada baris tanpa tarif dan tanpa koreksi ditolak `VR-WFP-21` | `INTEGRATION` | Response dan isi dokumen | — |
| `AT-HRD-D1-22` | Menu dapat dicapai dan tidak bertabrakan makna | **Berhasil:** kesembilan butir menu `S-D1` terlihat sesuai hak akses; butir "Kebutuhan Tenaga Kerja" lama sudah tidak bermakna sama dengan grup baru | `UI` | Tangkapan layar sidebar per peran | — |


---

## 16. Rekrutmen dan Hiring — `S-D2` (`last_changed_in` `v8`, 3 November 2026)

Kolom "AC Asal" merujuk acceptance criteria decision log bagian 33.6 (`HRD-DEC-073` s.d. `HRD-DEC-080`).
Uji integrasi diverifikasi terhadap endpoint API backend `RecruitmentManagement` dan database development `QuilvianNewDevHamzah`.

| ID | Requirement | Skenario | Jenis | Bukti yang Diharapkan | AC Asal |
| --- | --- | --- | --- | --- | --- |
| `AT-HRD-D2-01` | Requisition wajib berbasis formasi (`HRD-DEC-073`) | **Gagal:** Buat lowongan baru tanpa memilih `JobRequisitionId` berstatus `APPROVED` | `INTEGRATION` | Response `400 Bad Request` | `AC-REC-01` |
| `AT-HRD-D2-02` | Kuota kursi tidak melebihi sisa alokasi formasi | **Gagal:** Sisa alokasi formasi luar 2 kursi; ajukan requisition 3 kursi | `INTEGRATION` | Response `422 Unprocessable` dengan pesan kuota terlampaui | `AC-REC-01` |
| `AT-HRD-D2-03` | Pengaman tabel kosong migration (`HRD-DEC-074`) | **Gagal:** Isi 1 baris di tabel `TrxJobRequisition` dev uji, jalankan migration; migration berhenti dan membatalkan transaksi | `MIGRATION` | Pesan galat migration `EMPTY_TABLE_GUARD` | `AC-REC-02` |
| `AT-HRD-D2-03b` | Migration berhasil pada database kosong | **Berhasil:** Jalankan migration pada database kosong; 20 tabel lama dibersihkan dan 12 tabel baru terbuat | `MIGRATION` | Skema database terupdate bersih | `AC-REC-02` |
| `AT-HRD-D2-04` | Anti-duplikasi profil NIK eks-karyawan (`HRD-DEC-075`) | **Berhasil:** Pelamar dengan NIK yang sudah terdaftar di `WorkforceProfile` menerima penawaran kerja; sistem menautkan ke profil lama tanpa ID baru | `INTEGRATION` | `HrdCandidate.WorkforceProfileId` terisi ID lama; `WorkforceProfile` tidak bertambah baris orang | `AC-REC-03` |
| `AT-HRD-D2-05` | Hard gate STR aktif posisi klinis (`HRD-DEC-076`) | **Gagal:** Pelamar perawat mendaftar dengan STR yang kadaluwarsa kemarin; sistem otomatis menggugurkan di tahap screening | `INTEGRATION`, `UI` | Status lamaran `REJECTED`, jadwal wawancara terkunci | `AC-REC-04` |
| `AT-HRD-D2-06` | Hak veto klinis unit pengguna (`HRD-DEC-077`) | **Gagal:** Asesor unit klinis memilih rekomendasi `NOT_RECOMMENDED`; login akun HR Manager mencoba membuatkan Job Offer | `INTEGRATION`, `UI` | Response `403 Forbidden` - Hak veto klinis aktif | `AC-REC-05` |
| `AT-HRD-D2-07` | Masking privasi rekam medis MCU (`HRD-DEC-078`) | **Berhasil:** Dokter K3RS menginput hasil tes lab & diagnosa; akun HR membuka antarmuka pelamar; detail klinis disembunyikan | `INTEGRATION`, `UI` | Tampilan hanya memuat status kelaikan (`FIT`/`UNFIT`) | `AC-REC-06` |
| `AT-HRD-D2-08` | Penawaran bersyarat mendahului MCU (`HRD-DEC-078`) | **Gagal:** Jadwalkan MCU untuk kandidat yang belum menerima surat penawaran bersyarat | `INTEGRATION` | Response `409 Conflict` | `VR-REC-15` |
| `AT-HRD-D2-09` | Persetujuan offer melebihi batas grade (`HRD-DEC-079`) | **Berhasil:** Penawaran gaji di atas `MaximumSalary` dari `SalaryGrade` otomatis mengarahkan ke approval Direktur Keuangan & Dirut | `INTEGRATION` | Log persetujuan memuat tier `T14` | `AC-REC-07` |
| `AT-HRD-D2-10` | Promosi cepat dari Talent Pool (`HRD-DEC-080`) | **Berhasil:** Kandidat utama menolak offer; recruiter menekan tombol promosi cadangan; offer baru terbit bagi kandidat peringkat 1 | `INTEGRATION`, `UI` | Status kandidat cadangan menjadi `PROMOTED`, offer baru terbuat | `AC-REC-08` |
| `AT-HRD-D2-11` | Kadaluwarsa retensi Talent Pool (`HRD-DEC-080`) | **Gagal:** Mencoba mempromosikan kandidat yang telah berada di Talent Pool > 6 bulan | `INTEGRATION` | Response `422 Unprocessable` - Status expired | `VR-REC-19` |
| `AT-HRD-D2-12` | Serah terima onboarding aman (`HRD-DEC-075`) | **Berhasil:** Pelamar lolos MCU `FIT` diserahterimakan; draf `WorkforceProfile` berstatus `Pending Onboarding` terbentuk | `INTEGRATION` | Response `200` dan baris baru di `WfpProfile` | `AC-REC-03` |


---

## 17. Onboarding dan Kontrak Kerja — `S-C4-ONB` (`last_changed_in` `v9`, 4 November 2026)

Status `approved` — Pengguna (pemilik teknis dan produk, `HRD-DEC-015`), 2026-11-04, beserta kelima pilihan desain (`HRD-DEC-096`). Jawaban pemilik: "setujui semua ya dan kerjakan". Jenis test mengikuti bagian 0.2 dokumen ini. Backend tidak memelihara proyek automated
test; bukti berupa uji API terhadap database dev dan pemeriksaan isi tabel.

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan | Dasar |
| --- | --- | --- | --- | --- | --- |
| `AT-HRD-ONB-01` | Kasus otomatis | **Berhasil:** serah terima Rina dijalankan; kasus, butir tiga lapis, draf kontrak PKWT, dan permintaan akun terbentuk | `INTEGRATION` | Satu baris kasus; butir Umum + Klinis + ICU; kontrak `Draft` 1 Feb 2027 s.d. 31 Jan 2028 | `HRD-DEC-083`, `HRD-DEC-095`, `AC-ONB-01` |
| `AT-HRD-ONB-02` | Kasus otomatis | **Gagal:** serah terima dijalankan dua kali | `INTEGRATION` | Tetap satu kasus; response kedua mengembalikan kasus yang sama | `VR-ONB-01` |
| `AT-HRD-ONB-03` | Kasus otomatis | **Gagal:** penyimpanan kasus dipaksa gagal di tengah transaksi | `INTEGRATION` | Serah terima tetap `Generated`; tidak ada kontrak atau butir tersisa | `INT-HR-25` |
| `AT-HRD-ONB-04` | Batal bergabung | **Berhasil:** Rina tidak datang; HRD menekan Batal Bergabung dengan alasan Tidak hadir | `CONTRACT` | Kasus `Cancelled`; status `CANCELLED_JOIN`; profil nonaktif tidak terhapus; kontrak draf `Cancelled`; kursi requisition terbuka | `AC-ONB-03` |
| `AT-HRD-ONB-05` | Batal bergabung | **Gagal:** hadir hari pertama sudah dicatat | `CONTRACT` | `422` `VR-ONB-13`; kasus tetap `InProgress` | `AC-ONB-02` |
| `AT-HRD-ONB-06` | Kontrak pertama | **Gagal:** Tandai Sudah Ditandatangani tanpa scan | `CONTRACT` | `422` `VR-ONB-30` | `AC-ONB-04` |
| `AT-HRD-ONB-07` | Kontrak pertama | **Berhasil:** scan diunggah | `INTEGRATION` | Kontrak `Active`, `IsCurrent`; penempatan organisasi, jabatan, gaji awal berlaku 1 Feb 2027; `MstEmployee` berisi jenis Kontrak, tanggal masuk, tanggal akhir kontrak; butir otomatis kontrak tercentang | `HRD-DEC-084`, `INT-HR-27` |
| `AT-HRD-ONB-08` | Kontrak pertama | **Gagal:** isi gaji diubah langsung | `CONTRACT` | Tidak ada endpoint ubah isi (`405`); perubahan hanya lewat revisi tawaran | `AC-ONB-05`, `VR-ONB-31` |
| `AT-HRD-ONB-09` | Masa percobaan | **Gagal:** tanggal masa percobaan diisi pada kontrak PKWT lewat endpoint riwayat kontrak 02 dan lewat batch | `CONTRACT` | `409` `VR-ONB-32`; `422` `VR-ONB-34` | `AC-ONB-06` |
| `AT-HRD-ONB-10` | Masa percobaan | **Berhasil:** kontrak PKWTT Andi ditandatangani | `INTEGRATION` | Masa percobaan `Running` 1 Feb s.d. 30 Apr 2027; `ReminderFromDate` 16 Apr 2027 | `HRD-DEC-085` |
| `AT-HRD-ONB-11` | Masa percobaan | **Berhasil:** atasan mengajukan Lulus, HRD lain menyetujui | `INTEGRATION` | Status `Passed`; penilaian `Approved` | `HRD-DEC-085` |
| `AT-HRD-ONB-12` | Masa percobaan | **Gagal:** Tidak Lulus tanpa lampiran | `CONTRACT` | `422` `VR-ONB-57` | `AC-ONB-08` |
| `AT-HRD-ONB-13` | Masa percobaan | **Berhasil:** tidak ada keputusan sampai 30 Apr 2027 | `INTEGRATION` — scheduler | 1 Mei 2027 status `PassedAutomatically`; penilaian terbuka `ClosedOnExpiry`; persetujuan sesudahnya ditolak `VR-ONB-58` | `AC-ONB-07` |
| `AT-HRD-ONB-14` | Masa percobaan | **Berhasil:** Tidak Lulus disetujui | `INTEGRATION` | Status `Failed`; `TrxEmployeeSeparation` `ProbationFailed` dan exit clearance terbentuk | `HRD-DEC-082`, `INT-HR-29` |
| `AT-HRD-ONB-15` | Konversi | **Berhasil:** Rina diangkat PKWTT | `INTEGRATION` | Kontrak `Conversion` tanpa masa percobaan | `AC-ONB-16` |
| `AT-HRD-ONB-16` | Checklist | **Berhasil:** perawat ICU dan petugas keuangan diserahterimakan | `INTEGRATION` | Perawat tiga lapis; petugas keuangan satu lapis | `AC-ONB-09` |
| `AT-HRD-ONB-17` | Checklist | **Gagal:** butir otomatis dicentang manual; butir wajib bukti tanpa berkas; Kepala Unit mencentang butir HRD | `CONTRACT` | `422` `VR-ONB-06`, `VR-ONB-05`, `VR-ONB-03` | `AC-ONB-10` |
| `AT-HRD-ONB-18` | Checklist | **Gagal:** Kepala Farmasi membuka kasus pegawai ICU | `CONTRACT` | `404` `VR-ONB-04` | `VR-ONB-04` |
| `AT-HRD-ONB-19` | Template | **Gagal:** mengaktifkan Template Umum kedua; menghapus butir sistem | `CONTRACT` | `409` `VR-ONB-20`; `422` `VR-ONB-22` | `HRD-DEC-086` |
| `AT-HRD-ONB-20` | Template | **Berhasil:** template diubah saat kasus berjalan | `INTEGRATION` | Butir kasus yang sudah berjalan tidak berubah | `TEMPLATE_CHANGE_NOT_RETROACTIVE` |
| `AT-HRD-ONB-21` | Siap kerja | **Gagal:** satu butir wajib belum selesai | `CONTRACT` | `422` `VR-ONB-11` | `AC-ONB-11` |
| `AT-HRD-ONB-22` | Siap kerja | **Gagal:** petugas yang sama memilih atasan langsung dan menyatakan siap kerja | `CONTRACT` | `422` `VR-ONB-12` | `HRD-DEC-031` |
| `AT-HRD-ONB-23` | Siap kerja | **Berhasil:** seluruh butir wajib selesai; HRD menyatakan siap kerja | `INTEGRATION` | Status `ACTIVE`; relasi atasan; pendampingan `Accompanied`; permintaan akses kerja `Requested`; kasus `Completed` | `HRD-DEC-088` |
| `AT-HRD-ONB-24` | Kesiapan jadwal | **Gagal:** pegawai `PENDING_ONBOARDING` dijadwalkan | `INTEGRATION` — bersama 06 | Kesiapan `NotReady`; 06 menolak `VR-ONB-47` | `AC-ONB-12` |
| `AT-HRD-ONB-25` | Akun | **Berhasil:** hadir hari pertama dicatat | `INTEGRATION` | Akun aktif dengan peran layanan mandiri saja; akses klinis belum ada | `HRD-DEC-088` |
| `AT-HRD-ONB-26` | Akun | **Gagal:** email pegawai kosong | `INTEGRATION` | Kasus tetap terbentuk; permintaan akun `Failed` dengan pesan; ulang berhasil setelah email diisi | `VR-ONB-75` |
| `AT-HRD-ONB-27` | Pendampingan | **Gagal:** Siap Mandiri tanpa lampiran | `CONTRACT` | `422` `VR-ONB-60` | `AC-ONB-13` |
| `AT-HRD-ONB-28` | Pendampingan | **Berhasil:** Siap Mandiri dengan logbook | `CONTRACT` | Status `Independent`; tidak ada tugas di kotak masuk HRD; penanda Didampingi hilang dari kesiapan | `AC-ONB-13` |
| `AT-HRD-ONB-29` | Keputusan kontrak | **Gagal:** perpanjangan membuat total PKWT 5 tahun 6 bulan | `CONTRACT` | `422` `VR-ONB-36` | `AC-ONB-14` |
| `AT-HRD-ONB-30` | Keputusan kontrak | **Berhasil:** Perpanjang 12 bulan disetujui, lalu kontrak lanjutan ditandatangani | `INTEGRATION` | Kontrak lanjutan `Active` mulai 1 Feb 2028; pada tanggal itu kontrak lama `Completed` | `HRD-DEC-089` |
| `AT-HRD-ONB-31` | Keputusan kontrak | **Berhasil:** Tidak Diperpanjang disetujui dan pemberitahuan dicatat | `INTEGRATION` | Pemisahan `ContractNonRenewal` dengan tanggal terakhir = tanggal berakhir kontrak; sesudah tanggal itu kontrak `Ended` | `HRD-DEC-082` |
| `AT-HRD-ONB-32` | Penahan jadwal | **Berhasil:** PKWT lewat tanpa keputusan | `INTEGRATION` — scheduler dan 06 | Kontrak `LapsedWithoutDecision`; jadwal sesudah tanggal berakhir ditolak `VR-ONB-46`; jadwal sampai tanggal berakhir tetap dapat dibuat | `AC-ONB-15` |
| `AT-HRD-ONB-33` | Koreksi | **Gagal:** pembuat koreksi menyetujui sendiri | `INTEGRATION` | Ditolak mesin persetujuan | `HRD-DEC-089` |
| `AT-HRD-ONB-34` | Koreksi | **Berhasil:** tanggal berakhir salah ketik dikoreksi | `INTEGRATION` | Kontrak berisi nilai baru; nilai lama dan scan lama tersimpan pada baris koreksi | `HRD-DEC-089` |
| `AT-HRD-ONB-35` | Batch data awal | **Gagal:** PKWT tanpa tanggal berakhir; pegawai masuk batch kedua | `CONTRACT` | Baris tidak sah `VR-ONB-66`; `409` `VR-ONB-65` | `AC-ONB-17` |
| `AT-HRD-ONB-36` | Batch data awal | **Berhasil:** batch disetujui petugas HRD lain | `INTEGRATION` | Kontrak `InitialData` per baris; pengingat kontrak Budi muncul di daftar kerja; tanpa masa percobaan | `HRD-DEC-090` |
| `AT-HRD-ONB-37` | Pengaman migration | **Gagal:** satu baris uji di `TrxProbationReview` | `MANUAL` — migration | Migration berhenti dan menyebut nama tabel serta jumlah barisnya; tidak ada perubahan | `AC-ONB-18` |
| `AT-HRD-ONB-38` | Tawaran `S-D2` | **Gagal:** tawaran tanpa jenis kontrak; PKWT tanpa lama | `CONTRACT` | `422` `VR-ONB-50`, `VR-ONB-51` | `HRD-DEC-095` |
| `AT-HRD-ONB-39` | Riwayat kontrak 02 | **Gagal:** layar lama mencoba menyimpan kontrak | `CONTRACT` | `409` `CONTRACT_MANAGED_BY_LIFECYCLE` dengan pesan terbaca | `VR-ONB-32` |
| `AT-HRD-ONB-40` | Privasi | **Berhasil:** Kepala Unit membuka data cetak kontrak | `CONTRACT` | `basicSalary = null`, `amountVisible = false`; HR Admin dengan `ViewAmount` melihat nominal dan pembacaannya tercatat | `HRD-DEC-033`, `HRD-DEC-039` |


---

## 18. Matriks Pengujian Penerimaan Slice `S-B4` — Penjadwalan, Roster, dan On-Call (`last_changed_in` `v10`, 4 November 2026)

| Field | Value |
| --- | --- |
| Status | `draft` — Siap diajukan untuk approval pengguna |
| `contract_version` | `v10` |
| `input_revision` — decision log | `00-interview-decisions.md` revision `20`, bagian 36 (`HRD-DEC-097` s.d. `HRD-DEC-103`) |
| Tanggal | 2026-11-04 |

### 18.1 Matriks Kasus Uji Penerimaan (`AT-HRD-SCH-01` s.d. `AT-HRD-SCH-20`)

| ID Kasus Uji | Topik | Jalur Uji (Berhasil / Gagal) | Jenis Test | Hasil yang Diharapkan | Referensi Aturan / Keputusan |
| --- | --- | --- | :---: | --- | --- |
| `AT-HRD-SCH-01` | Roster | **Berhasil:** Buat draft roster unit shift bergiliran | `CONTRACT` | Periode `Draft` terbentuk, staf unit terisi otomatis dari penempatan aktif | `HRD-DEC-097` |
| `AT-HRD-SCH-02` | Roster | **Gagal:** Buat roster pada unit jam kantor biasa | `CONTRACT` | `422` unit jam kantor memakai jadwal tetap, bukan roster bulanan | `HRD-DEC-097` |
| `AT-HRD-SCH-03` | Roster | **Berhasil:** Salin seluruh jadwal dari periode bulan lalu | `CONTRACT` | 500+ kotak shift terduplikasi dalam status draft | `HRD-DEC-098` |
| `AT-HRD-SCH-04` | Roster | **Berhasil:** Terapkan pola shift berulang ke kelompok staf | `CONTRACT` | Pola shift (misal P-S-M-L-L) terpetakan ke hari-hari periode | `HRD-DEC-098` |
| `AT-HRD-SCH-05` | Validasi | **Gagal:** Staf dijadwalkan shift bertabrakan jamnya | `CONTRACT` | `422` Pelanggaran `B1` memblokir penerbitan roster | `VR-SCH-01`, `HRD-DEC-099` |
| `AT-HRD-SCH-06` | Validasi | **Gagal:** Staf dijadwalkan pada hari cuti penuh disetujui | `CONTRACT` | `422` Pelanggaran `B2` memblokir penerbitan roster | `VR-SCH-02`, `HRD-DEC-099` |
| `AT-HRD-SCH-07` | Validasi | **Gagal:** Staf belum berstatus siap kerja (`HRD-DEC-088`) | `INTEGRATION` | `422` Pelanggaran `B4` menolak penjadwalan pegawai orientasi | `VR-SCH-04`, `INT-HR-40` |
| `AT-HRD-SCH-08` | Validasi | **Gagal:** Staf dijadwalkan sesudah kontrak berakhir tanpa keputusan | `INTEGRATION` | `422` Pelanggaran `B5` menahan penjadwalan pasca kontrak | `VR-SCH-05`, `HRD-DEC-089` |
| `AT-HRD-SCH-09` | Validasi | **Gagal:** Status kesiapan profil pegawai gagal dibaca | `INTEGRATION` | `422` Pelanggaran `B6` gagal tertutup sesuai invariant sistem | `VR-SCH-06`, `INT-HR-31` |
| `AT-HRD-SCH-10` | Validasi | **Gagal:** Staf Didampingi dijadwalkan tanpa perawat Mandiri | `CONTRACT` | `422` Pelanggaran `B7` memblokir jadwal demi patient safety | `VR-SCH-07`, `HRD-DEC-100` |
| `AT-HRD-SCH-11` | Validasi | **Berhasil:** Staf Didampingi dijadwalkan bersama perawat Mandiri lain saat preceptor libur | `CONTRACT` | Jadwal shift lolos evaluasi keselamatan | `HRD-DEC-100` |
| `AT-HRD-SCH-12` | Validasi | **Berhasil:** Peringatan jeda < 11 jam (`W1`) dan kuota kurang (`W6`) diterbitkan setelah isi alasan | `CONTRACT` | Status roster berubah `Published`; alasan tersimpan di audit trail | `HRD-DEC-098`, `HRD-DEC-099` |
| `AT-HRD-SCH-13` | Hak Akses | **Berhasil & Gagal:** Kepala Bidang membuka roster (OK) dan mencoba mengedit (403) | `SECURITY` | Tampilan roster terbuka *read-only*; request edit ditolak `403` | `HRD-DEC-098` |
| `AT-HRD-SCH-14` | Override | **Berhasil:** Kepala Unit mengeksekusi Ganti Petugas langsung pasca-terbit | `INTEGRATION` | Shift berpindah ke pengganti (`IsManualOverride = true`), status roster tetap `Published` | `HRD-DEC-101` |
| `AT-HRD-SCH-15` | Override | **Gagal:** Staf pengganti yang dipilih sedang cuti atau bentrok | `CONTRACT` | `422` penunjukan pengganti ditolak sistem | `VR-SCH-16`, `HRD-DEC-101` |
| `AT-HRD-SCH-16` | Koreksi | **Berhasil:** Koreksi mundur shift 2 hari lalu dengan alasan sah | `INTEGRATION` | Shift tersimpan; event rekalkulasi kehadiran terpicu di modul 07 | `VR-SCH-17`, `HRD-DEC-102` |
| `AT-HRD-SCH-17` | Koreksi | **Gagal:** Koreksi mundur shift 5 hari lalu oleh Kepala Unit | `CONTRACT` | `422` ditolak melebihi batas toleransi mandiri 3 hari | `VR-SCH-17`, `HRD-DEC-102` |
| `AT-HRD-SCH-18` | Close Lock | **Berhasil & Gagal:** Payroll menutup periode (Closed); edit pasca-tutup ditolak | `CONTRACT` | Status periode `Closed`; seluruh mutasi shift ditolak `403` | `VR-SCH-18`, `HRD-DEC-102` |
| `AT-HRD-SCH-19` | On-Call | **Berhasil:** Pemanggilan siaga cito mencatat waktu dan evaluasi SLA | `CONTRACT` | `ResponseMinutes` dihitung, `IsSlaMet = true`, event payroll terkirim | `HRD-DEC-103` |
| `AT-HRD-SCH-20` | Integrasi Lab | **Berhasil:** Lab resolver tetap membaca dokter siaga aktif saat berstatus dipanggil | `INTEGRATION` | `LabConfirmingDoctorResolver` menemukan dokter dengan status `Active` | `LAB-DEC-111`, `HRD-DEC-103` |


---

## 19. Bagian `S-B1` — Matriks Uji Penerimaan Administrasi Kehadiran Staf Rumah Sakit (Sub-modul 07, `last_changed_in` `v11`, 4 November 2026)

| ID Uji | Skenario Pengujian | Masukan Uji | Kriteria Penerimaan yang Diharapkan | Keputusan Terkait |
| --- | --- | --- | --- | :---: |
| `AT-HRD-ATT-01` | Immutabilitas rekaman mentah biometrik | Permintaan PUT/DELETE ke raw log | Sistem menolak perubahan dan mengembalikan HTTP 405 | `ATT-B1` |
| `AT-HRD-ATT-02` | Pencegahan tap ganda biometrik | 2 tap dalam selang 60 detik | Hanya tap pertama yang diproses oleh schedule resolver | `VR-ATT-18` |
| `AT-HRD-ATT-03` | Penelusuran drawer log biometrik mentah | Buka drawer log mentah pegawai | Menampilkan seluruh riwayat tap riil dan ID perangkat | `FR-ATT-05` |
| `AT-HRD-ATT-04` | Pencocokan shift malam lintas hari | Tap masuk 20.50 (H), tap pulang 07.15 (H+1) | Record kehadiran terkunci pada tanggal H dengan jam kerja 10 jam | `HRD-DEC-104` |
| `AT-HRD-ATT-05` | Tidak ada mangkir di hari H shift malam | Shift malam 21.00 s.d. 07.00 | Hari H berstatus Present, bukan Absent | `HRD-DEC-104` |
| `AT-HRD-ATT-06` | Tidak ada dinas ganda di hari H+1 shift malam| Shift malam berakhir 07.00 hari H+1 | Hari H+1 tidak mencatat sesi dinas ganda fiktif | `HRD-DEC-104` |
| `AT-HRD-ATT-07` | Toleransi keterlambatan di bawah 10 menit | Tap masuk 07.08 pada shift 07.00 | Status Present, LateMinutes = 0 | `HRD-DEC-105` |
| `AT-HRD-ATT-08` | Keterlambatan di atas toleransi 10 menit | Tap masuk 07.14 pada shift 07.00 | Status Late, LateMinutes = 14 menit penuh | `HRD-DEC-105` |
| `AT-HRD-ATT-09` | Pengubahan setelan toleransi keterlambatan | Ubah toleransi ke 15 menit | Menit keterlambatan baru dievaluasi sesuai parameter 15 menit | `HRD-DEC-105` |
| `AT-HRD-ATT-10` | Deteksi lupa tap 1 punch | Hanya tap masuk pukul 07.00 | Status harian IncompletePunch, jam kerja belum dihitung | `HRD-DEC-106` |
| `AT-HRD-ATT-11` | Notifikasi pengingat absensi tidak lengkap | Daily berstatus IncompletePunch | Notifikasi pengingat terkirim ke ESS pegawai dan atasan | `HRD-DEC-106` |
| `AT-HRD-ATT-12` | Konversi otomatis saat cut-off periode | IncompletePunch tidak dikoreksi | Saat periode Closed, otomatis berubah menjadi Absent | `HRD-DEC-106` |
| `AT-HRD-ATT-13` | Koreksi presensi on-behalf oleh HR | HR mengajukan koreksi atas nama staf | Jejak audit on-behalf terekam; persetujuan di atasan langsung | `HRD-DEC-028` |
| `AT-HRD-ATT-14` | Pembatasan koreksi tanggal mundur > 3 hari | Koreksi shift 5 hari lalu | Ditolak validasi toleransi koreksi tanggal mundur | `VR-ATT-06` |
| `AT-HRD-ATT-15` | Izin pulang cepat non-deductible | Pulang jam 11.30 pada shift 14.00 | Status Waived; kuota saldo cuti tahunan tetap utuh | `HRD-DEC-107` |
| `AT-HRD-ATT-16` | Perhitungan jam kerja riil izin pulang cepat | Tap keluar riil pukul 11.32 | Jam kerja efektif dihitung tepat 4 jam 32 menit | `HRD-DEC-107` |
| `AT-HRD-ATT-17` | Presensi dinas luar ambulans rujukan | Surat tugas rujukan ambulans disetujui | Status kehadiran OnDuty, jam kerja sah diakui penuh | `HRD-DEC-109` |
| `AT-HRD-ATT-18` | Verifikasi GPS & foto dinas luar | Tap mobile di lokasi rujukan | Koordinat GPS dan foto selfie tersimpan pada lampiran tugas | `HRD-DEC-109` |
| `AT-HRD-ATT-19` | Penolakan tutup periode bila ada koreksi pending | Close period saat ada koreksi pending | Ditutup ditolak (HTTP 409) dengan daftar pending | `HRD-DEC-108` |
| `AT-HRD-ATT-20` | Buka kembali periode darurat rollback payroll | HR Manager mengeksekusi Reopen | Status Reopened dan serah terima payroll otomatis di-rollback | `HRD-DEC-108` |


---

## 20. Bagian `S-B2` — Matriks Uji Penerimaan Administrasi Cuti & Izin Pegawai Rumah Sakit (Sub-modul 08, `last_changed_in` `v12`, 4 November 2026)

Matriks uji penerimaan (*Acceptance Test Matrix*) ini mengunci 20 skenario pengujian komprehensif berbasis bukti untuk memvalidasi pemenuhan kebutuhan fungsional dan teknis **Slice `S-B2` — Administrasi Cuti & Izin Pegawai Rumah Sakit**.

| Kode Uji | Skenario Pengujian | Masukan Uji (Input) | Hasil yang Diharapkan (Expected Result) | Status Kesiapan | Rujukan Aturan / Keputusan |
| --- | --- | --- | --- | :---: | --- |
| `AT-HRD-LEV-01` | Alokasi hak cuti tahunan 12 hari | Pegawai aktif per 1 Januari | Ledger mencatat mutasi `Entitlement` sejumlah +12.0 hari | Siap Uji | `FR-LEV-01`, `HRD-DEC-110` |
| `AT-HRD-LEV-02` | Pencatatan mutasi atomik buku besar | Transaksi pengajuan cuti disetujui | Record baru tersimpan di `TrxLeaveBalanceLedger` dengan rujukan ID permohonan | Siap Uji | `FR-LEV-02`, `HRD-DEC-110` |
| `AT-HRD-LEV-03` | Pemotongan saldo carry-forward akhir tahun max 6 hari | Sisa cuti tahun lalu 9 hari | 6 hari dipindahkan ke tahun baru (`CarryForward`), 3 hari dicatat hangus (`Forfeiture`) | Siap Uji | `FR-LEV-03`, `LEV-B8` |
| `AT-HRD-LEV-04` | Scheduler kedaluwarsa 30 Juni | Sisa saldo carry-forward 2 hari per 30 Juni 23:59:59 | Mutasi `CarryForwardForfeiture` -2.0 hari dibukukan; per 1 Juli saldo bawaan 0 | Siap Uji | `FR-LEV-05`, `LEV-W2` |
| `AT-HRD-LEV-05` | Pengajuan cuti mandiri ESS saldo mencukupi | Pengajuan cuti tahunan 3 hari, saldo 10 hari | Status permohonan menjadi `WaitingApproval`; saldo terpesan 3 hari | Siap Uji | `FR-LEV-06` |
| `AT-HRD-LEV-06` | Penolakan cuti melebihi saldo | Pengajuan cuti 5 hari, saldo tersedia 2 hari | HTTP 400 Bad Request: "Saldo cuti Anda tidak mencukupi" | Siap Uji | `FR-LEV-08`, `LEV-B1` |
| `AT-HRD-LEV-07` | Cuti setengah hari (half-day 0.5 hari) | Cuti tahunan opsi Half-Day sesi Pagi | Durasi tepat 0.5 hari kerja; ledger mendebit 0.5 hari | Siap Uji | `FR-LEV-07`, `HRD-ASM-20` |
| `AT-HRD-LEV-08` | Penolakan half-day pada cuti non-tahunan | Pengajuan cuti melahirkan opsi half-day | HTTP 400 Bad Request: fasilitas hanya untuk Cuti Tahunan | Siap Uji | `VR-LEV-05` |
| `AT-HRD-LEV-09` | Deteksi Safe Staffing Coverage Warning | Pengajuan cuti perawat menyebabkan sisa dinas 3 orang (min 4) | Approval Inbox atasan memunculkan banner peringatan kuota layanan | Siap Uji | `FR-LEV-09`, `HRD-DEC-111` |
| `AT-HRD-LEV-10` | Persetujuan Approve with Mitigation | Atasan memilih mitigasi dan mengisi nama perawat pengganti | Cuti disetujui; audit mencatat catatan mitigasi operasional | Siap Uji | `FR-LEV-10`, `HRD-DEC-111` |
| `AT-HRD-LEV-11` | Penolakan Approve with Mitigation tanpa catatan | Atasan submit mitigasi tanpa mengisi catatan | HTTP 400 Bad Request: catatan mitigasi wajib diisi | Siap Uji | `VR-LEV-21` |
| `AT-HRD-LEV-12` | Izin sakit mandiri 1 hari | Pengajuan izin sakit durasi 1 hari kerja | Permohonan tersimpan tanpa memblokir ketiadaan surat dokter | Siap Uji | `FR-LEV-12`, `HRD-DEC-112` |
| `AT-HRD-LEV-13` | Penolakan cuti sakit $\ge 2$ hari tanpa surat dokter | Pengajuan cuti sakit 2 hari tanpa lampiran berkas | HTTP 400 Bad Request: wajib melampirkan berkas surat dokter | Siap Uji | `FR-LEV-13`, `LEV-B3` |
| `AT-HRD-LEV-14` | Masking privasi medis staf K3RS | Atasan langsung membuka rincian surat sakit bawahan | Diagnosa ICD-10 dimaskir; hanya durasi dan validitas faskes/SIP yang tampil | Siap Uji | `FR-LEV-15`, `LEV-W3` |
| `AT-HRD-LEV-15` | Pembatalan pre-leave pulih 100% | Cuti disetujui dibatalkan sebelum tanggal mulai | Status menjadi `Cancelled`; mutasi kredit `CancellationRestore` +100% dibukukan | Siap Uji | `FR-LEV-11`, `HRD-ASM-21` |
| `AT-HRD-LEV-16` | Inisiasi Leave Recall darurat oleh Kepala Ruangan | Cuti berstatus `Active`; terjadi KLB darurat | Tiket recall diterbitkan berstatus `RequestedByHeadUnit` | Siap Uji | `FR-LEV-16`, `HRD-DEC-113` |
| `AT-HRD-LEV-17` | Persetujuan Leave Recall oleh HR Manager | HR Manager approve recall dengan sisa 2 hari cuti | Ledger mencatat kredit `RecallRestore` +2.0 hari; cuti menjadi `Recalled` | Siap Uji | `FR-LEV-18`, `LEV-W4` |
| `AT-HRD-LEV-18` | Pemulihan jadwal shift pasca-recall | Modul 06 menerima event `LeaveRecallExecutedEvent` | Shift penugasan di Roster 06 berubah aktif (`Scheduled`) tanpa penalti | Siap Uji | `FR-LEV-19`, `INT-HR-46` |
| `AT-HRD-LEV-19` | Rekonsiliasi retrospektif cuti pending payroll | Cuti disetujui setelah tanggal cut-off payroll | Flag `IsPendingPayrollReconciliation = true`; diserahkan ke payroll bulan depan | Siap Uji | `FR-LEV-22`, `HRD-DEC-114` |
| `AT-HRD-LEV-20` | Guard penolakan pembalikan cuti Completed (/reverse) | Pemanggilan `POST /{id}/reverse` pada cuti status Completed | HTTP 409 Conflict: immutabilitas cuti selesai (menutup celah `HRD-Q-35`) | Siap Uji | `FR-LEV-23`, `HRD-DEC-115` |
