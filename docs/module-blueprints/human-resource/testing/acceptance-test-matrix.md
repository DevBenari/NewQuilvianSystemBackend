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


---

## 21. Bagian `S-B3` — Matriks Uji Penerimaan Administrasi Lembur Pegawai Rumah Sakit (Sub-modul 09, `last_changed_in` `v13`, 4 November 2026)

Matriks uji penerimaan (*Acceptance Test Matrix*) ini mengunci 20 skenario pengujian komprehensif berbasis bukti untuk memvalidasi pemenuhan kebutuhan fungsional dan teknis **Slice `S-B3` — Administrasi Lembur Pegawai Rumah Sakit**.

| Kode Uji | Skenario Pengujian | Masukan Uji (Input) | Hasil yang Diharapkan (Expected Result) | Status Kesiapan | Rujukan Aturan / Keputusan |
| --- | --- | --- | --- | :---: | --- |
| `AT-HRD-OT-01` | SPKL pra-dinas terencana H-1 masa depan | Form SPKL H-1, jam mulai > waktu submit | Berkas tersimpan berstatus `WaitingApproval` | Siap Uji | `FR-OT-01`, `HRD-DEC-116` |
| `AT-HRD-OT-02` | Penolakan SPKL terencana masa lampau | Form SPKL terencana dengan jam mulai masa lalu | HTTP 400 Bad Request: "Waktu mulai harus di masa depan" | Siap Uji | `VR-OT-01`, `OT-B1` |
| `AT-HRD-OT-03` | Klaim darurat cito medis nomor RM valid | Form cito pasca-dinas H+1 dengan no RM `RM-089` | Berkas klaim tersimpan; nomor RM dan tindakan tercatat | Siap Uji | `FR-OT-02`, `FR-OT-03` |
| `AT-HRD-OT-04` | Penolakan klaim cito tanpa nomor RM | Form cito pasca-dinas tanpa mengisi no RM/insiden | HTTP 400 Bad Request: "Nomor RM / Insiden wajib diisi" | Siap Uji | `VR-OT-02`, `OT-B3` |
| `AT-HRD-OT-05` | Penolakan klaim cito terlambat > H+1 kerja | Lapor klaim cito hari ke-3 pasca dinas selesai | HTTP 400 Bad Request: "Batas waktu pelaporan kedaluwarsa" | Siap Uji | `FR-OT-05`, `VR-OT-06` |
| `AT-HRD-OT-06` | Penolakan lembur biasa > 4 jam/hari | Pengajuan lembur terencana durasi 5 jam sehari | HTTP 400 Bad Request: "Melebihi kuota 4 jam/hari PP 35" | Siap Uji | `FR-OT-07`, `HRD-DEC-117` |
| `AT-HRD-OT-07` | Penolakan lembur biasa > 18 jam/minggu | Pengajuan lembur menyebabkan akumulasi 20 jam | HTTP 400 Bad Request: "Melebihi kuota 18 jam/minggu PP 35" | Siap Uji | `FR-OT-07`, `HRD-DEC-117` |
| `AT-HRD-OT-08` | Eskalasi cito melebihi kuota ke Direktur Medis | Klaim cito 5 jam resusitasi diajukan | Status dialihkan ke `WaitingMedicalDirectorEscalation` | Siap Uji | `FR-OT-08`, `VR-OT-12` |
| `AT-HRD-OT-09` | Otorisasi dispensasi khusus Direktur Medis | Direktur Medis menyetujui klaim cito 5 jam | Otorisasi darurat tercatat; klaim lanjut verifikasi presensi | Siap Uji | `FR-OT-10`, `HRD-DEC-117` |
| `AT-HRD-OT-10` | Pemblokiran persetujuan diri sendiri (*Anti-Self*) | Ka. Ruangan klik approve pada SPKL miliknya sendiri | HTTP 403 Forbidden: "Dilarang menyetujui lembur sendiri" | Siap Uji | `VR-OT-03`, `HRD-DEC-120` |
| `AT-HRD-OT-11` | Pengalihan approval lembur struktural | Ka. Ruangan submit SPKL lembur pribadinya | Sistem merutekan SPKL ke kotak masuk Kepala Instalasi | Siap Uji | `HRD-DEC-120` |
| `AT-HRD-OT-12` | Min-Duration Rule SPKL 3 jam vs Tap 2.5 jam | SPKL 180 menit, log tap mesin presensi 150 menit | Durasi sah dihitung tepat 150 menit (2.5 jam) | Siap Uji | `FR-OT-12`, `HRD-DEC-119` |
| `AT-HRD-OT-13` | Pembulatan kelipatan 30 menit ke bawah | Log mesin presensi kelebihan 140 menit (2 jam 20 mnt)| Durasi sah dibulatkan ke bawah menjadi 120 menit (2 jam) | Siap Uji | `FR-OT-13`, `HRD-ASM-23` |
| `AT-HRD-OT-14` | Durasi lembur fisik < 30 menit nol jam | Log mesin presensi kelebihan 25 menit | `VerifiedPayableMinutes = 0` (tidak ada kompensasi) | Siap Uji | `VR-OT-09`, `HRD-ASM-23` |
| `AT-HRD-OT-15` | Deteksi Flagged Discrepancy deviasi > 30 menit | SPKL klaim 3 jam, tap mesin hanya lebih 1 jam | Status ditandai `FlaggedDiscrepancy: DurationMismatch` | Siap Uji | `FR-OT-14`, `VR-OT-10` |
| `AT-HRD-OT-16` | Penyesuaian manual HR dengan bukti logbook | HR input 120 menit dengan no logbook `LB-ICU-012` | Status berubah `ManuallyAdjusted`; klaim siap rekonsiliasi | Siap Uji | `FR-OT-15`, `VR-OT-11` |
| `AT-HRD-OT-17` | Kredit mutasi cuti pengganti CTO ke Sub-modul 08 | Klaim opsi CTO disetujui durasi 8 jam | `TrxLeaveBalanceLedger` (08) bertambah +1.0 hari CTO | Siap Uji | `FR-OT-18`, `INT-HR-49` |
| `AT-HRD-OT-18` | Scheduler kedaluwarsa saldo CTO 90 hari | Saldo CTO belum terpakai per H+91 hari kalender | Status tiket CTO menjadi `Expired`; kuota hangus non-tunai | Siap Uji | `FR-OT-20`, `HRD-ASM-25` |
| `AT-HRD-OT-19` | Penguncian cut-off bulanan (tgl 20) | HR Payroll kunci periode cut-off 20 November | Status `CutoffLocked`; modifikasi ditolak HTTP 409 | Siap Uji | `FR-OT-21`, `VR-OT-16` |
| `AT-HRD-OT-20` | Rapel retrospektif SPKL pending di payroll depan | SPKL pending disetujui tgl 23 Nov (pasca cut-off) | Flag `IsRetrospective = true`; masuk slip gaji Desember | Siap Uji | `FR-OT-23`, `HRD-DEC-121` |


---

## 22. Bagian S-B5 — Matriks Uji Penerimaan Penggajian Pegawai Rumah Sakit (Sub-modul 10, last_changed_in v14, 5 November 2026)

Matriks uji penerimaan (*Acceptance Test Matrix*) ini mengunci 20 skenario pengujian komprehensif berbasis bukti untuk memvalidasi pemenuhan kebutuhan fungsional, regulasi pajak, keamanan segregation of duties, dan integritas finansial **Slice `S-B5` — Penggajian Pegawai Rumah Sakit**.

| Kode Uji | Skenario Pengujian | Masukan Uji (Input) | Hasil yang Diharapkan (Expected Result) | Status Kesiapan | Rujukan Aturan / Keputusan |
| --- | --- | --- | --- | :---: | --- |
| `AT-HRD-PAY-01` | Inisiasi siklus payroll baru periode berjalan | Inisiasi payroll periode `2026-11` pasca cut-off tgl 20 | Batch tersimpan berstatus `Draft`; snapshot parameter terkunci | Siap Uji | `FR-PAY-01`, `HRD-DEC-122` |
| `AT-HRD-PAY-02` | Penolakan inisiasi jika waktu kerja belum terkunci | Inisiasi payroll saat periode presensi/lembur masih `Open` | HTTP 400 Bad Request: "Periode presensi/lembur belum terkunci" | Siap Uji | `VR-PAY-02`, `PAY-B1` |
| `AT-HRD-PAY-03` | Penolakan inisiasi payroll duplikat | Submit pembuatan payroll `2026-11` kedua kali | HTTP 409 Conflict: "Periode penggajian 2026-11 sudah ada" | Siap Uji | `VR-PAY-01`, `PAY-B2` |
| `AT-HRD-PAY-04` | Kalkulasi komponen gaji pokok & tunjangan profesi | Trigger kalkulasi pada profil Dokter Spesialis Organik | Gaji pokok, tunjangan fungsional & risiko medis terhitung tepat | Siap Uji | `FR-PAY-04`, `HRD-DEC-123` |
| `AT-HRD-PAY-05` | Penyerapan data lembur resmi terverifikasi Sub-09 | Perhitungan lembur perawat ICU (10 jam Tier 1, 15 jam Tier 2)| Nilai lembur terhitung otomatis sesuai formula PP 35/2021 | Siap Uji | `FR-PAY-05`, `INT-HR-52` |
| `AT-HRD-PAY-06` | Kalkulasi premi shift malam presensi Sub-07 | Data kehadiran 8 shift malam (21:00 - 07:00) | Premi shift malam Rp 400.000 (8 x Rp 50.000) terakumulasi | Siap Uji | `FR-PAY-06`, `INT-HR-36` |
| `AT-HRD-PAY-07` | Kalkulasi BPJS Kesehatan batas plafon gaji | Pegawai dengan gaji Rp 15.000.000 (di atas plafon Rp 12.000.000) | Iuran 1% pekerja dihitung tepat dari plafon Rp 12.000.000 (Rp 120.000) | Siap Uji | `FR-PAY-07`, `VR-PAY-08` |
| `AT-HRD-PAY-08` | Kalkulasi BPJS Ketenagakerjaan (JHT & JP) | Pegawai organik gaji Rp 12.000.000 (di atas batas JP Rp 10.042.300)| JHT 2% dihitung dari gaji penuh; JP 1% dipotong batas Rp 100.423 | Siap Uji | `FR-PAY-08`, `VR-PAY-09` |
| `AT-HRD-PAY-09` | Pemotongan PPh 21 Tarif Efektif Rata-Rata (TER) | Pegawai TK/0 bruto Rp 8.500.000 (TER A lapis tarif 1.5%) | PPh 21 TER terhitung otomatis Rp 127.500 tanpa selisih pembulatan | Siap Uji | `FR-PAY-09`, `INT-HR-56` |
| `AT-HRD-PAY-10` | Penyerapan potongan angsuran pinjaman Sub-11 | Tarik potongan cicilan pinjaman koperasi Rp 500.000 | Komponen potongan muncul di slip gaji; sisa pokok pinjaman berkurang | Siap Uji | `FR-PAY-10`, `INT-HR-55` |
| `AT-HRD-PAY-11` | Penyerapan penyesuaian retrospektif / rapel lembur | Kompensasi lembur pending bulan Oktober disetujui tgl 23 Nov | Nominal rapel masuk ke komponen `RetrospectiveAdjustment` | Siap Uji | `FR-PAY-11`, `HRD-DEC-123` |
| `AT-HRD-PAY-12` | Deteksi anomali variansi kenaikan gaji $\ge 20\%$ | Gaji bersih staf naik dari Rp 6.000.000 ke Rp 7.500.000 (+25%)| Sistem memicu `trx_payroll_variance_alerts` status `PendingJustification` | Siap Uji | `FR-PAY-12`, `HRD-DEC-124` |
| `AT-HRD-PAY-13` | Penolakan submit batch jika variansi belum dijustifikasi| Payroll Officer submit batch saat ada 2 alert pending | HTTP 422 Unprocessable Entity: "Terdapat variansi belum dijustifikasi" | Siap Uji | `VR-PAY-14`, `PAY-B5` |
| `AT-HRD-PAY-14` | Pengisian justifikasi variansi minimum 20 karakter | Officer input penjelasan 25 karakter atas lonjakan jam lembur cito | Alert terselesaikan (`Resolved`); batch diizinkan untuk di-submit | Siap Uji | `FR-PAY-14`, `VR-PAY-15` |
| `AT-HRD-PAY-15` | Otorisasi Tier 1 oleh Payroll Officer | Payroll Officer klik submit pengajuan approval | Status batch berubah `WaitingHrReview`; notifikasi ke HR Manager | Siap Uji | `FR-PAY-16`, `HRD-DEC-125` |
| `AT-HRD-PAY-16` | Pemblokiran persetujuan diri sendiri (*Anti-Self*) | HR Manager menyetujui batch yang memuat gajinya sendiri | Approval tier 2 dialihkan otomatis ke HR Director / Wadir Umum | Siap Uji | `FR-PAY-17`, `VR-PAY-17` |
| `AT-HRD-PAY-17` | Otorisasi Tier 3 Direktur & Penguncian Immutable | Direktur SDM/Keuangan menyetujui batch penggajian | Status `ApprovedFinal`; data terkunci permanen `IsImmutable = true` | Siap Uji | `FR-PAY-18`, `VR-PAY-20` |
| `AT-HRD-PAY-18` | Pembentukan paket handoff Finance & verifikasi Checksum | Sistem men-generate payload batch `INT-HR-53` | Checksum SHA-256 terbentuk; rekap bank per pegawai terbit valid | Siap Uji | `FR-PAY-20`, `INT-HR-53` |
| `AT-HRD-PAY-19` | Penolakan batch oleh Finance & Rollback Terkendali | Modul Finance mengirim callback `PaymentRejected` (rekening salah) | Status batch rollback ke `NeedCorrection`; gembok terbuka dengan audit | Siap Uji | `FR-PAY-22`, `HRD-DEC-126` |
| `AT-HRD-PAY-20` | Rilis slip gaji ESS & Autentikasi PIN Sekunder 6-Digit | Pegawai akses slip gaji di ESS pada tgl 25 dan input PIN | PIN valid membuka slip gaji PDF; 3 kali salah mengunci akses 30 menit | Siap Uji | `FR-PAY-24`, `HRD-DEC-127` |

## 23. Bagian S-B6 — Matriks Uji Penerimaan Restitusi & Pemotongan Penggajian Pegawai Rumah Sakit (Sub-modul 11, last_changed_in v15, 5 November 2026)

Bagian ini mendefinisikan 20 skenario uji penerimaan fungsional dan keselamatan finansial (**`AT-HRD-DED-01` s.d. `AT-HRD-DED-20`**) untuk **Sub-modul 11 (Restitution & Payroll Deduction Integration — Slice `S-B6`)**.

---

| ID Uji | Judul Skenario Uji | Kategori Pengujian | Langkah Pengujian (Action Steps) | Kondisi Input & Data Uji | Hasil yang Diharapkan (Expected Result) | Status |
| :--- | :--- | :--- | :--- | :--- | :--- | :---: |
| `AT-HRD-DED-01` | Penegakan Batas Maksimal 50% Gross Pay | Safety Invariant | Eksekusi perhitungan payroll bulanan untuk pegawai dengan total cicilan > 50% | Gross Pay: Rp 8.000.000, Total Tagihan: Rp 5.000.000 | Sistem membatasi pemotongan tepat maksimal Rp 4.000.000 (50%); sisa tagihan Rp 1.000.000 otomatis ditunda | Lolos Rencana |
| `AT-HRD-DED-02` | Hirarki Prioritas Berjenjang (Ganti Rugi vs Kasbon) | Business Rule | Eksekusi potongan untuk pegawai yang memiliki ganti rugi aset RS dan kasbon pinjaman | Plafon 50%: Rp 4.000.000, Ganti Rugi: Rp 2.500.000, Kasbon: Rp 2.000.000 | Ganti rugi dipotong penuh Rp 2.500.000, kasbon dipotong parsial Rp 1.500.000, sisa kasbon Rp 500.000 ditunda | Lolos Rencana |
| `AT-HRD-DED-03` | Pemotongan Parsial & Penundaan Cicilan Koperasi | Business Rule | Eksekusi potongan prioritas 3 ketika sisa plafon tinggal sebagian kecil | Sisa Plafon: Rp 300.000, Tagihan Koperasi: Rp 1.000.000 | Koperasi dipotong Rp 300.000, status `PartiallyDeducted`, sisa Rp 700.000 status `DeferredDueToSalaryCap` | Lolos Rencana |
| `AT-HRD-DED-04` | Penundaan Penuh Iuran Paguyuban / Sosial | Business Rule | Eksekusi potongan prioritas 4 saat seluruh kuota 50% telah habis terpakai | Sisa Plafon: Rp 0, Tagihan Paguyuban: Rp 150.000 | Tagihan paguyuban tidak dipotong di slip (Rp 0), status `DeferredDueToSalaryCap`, dialihkan ke bulan depan | Lolos Rencana |
| `AT-HRD-DED-05` | Tarik Snapshot Piutang Finance H-3 (tgl 17) | Integration Test | Jalankan job terjadwal penarikan data piutang Finance per tgl 17 | Koneksi Finance mTLS aktif, ada 150 kontrak aktif | 150 jadwal cicilan tersimpan di `trx_payroll_deduction_schedules`, audit log `AUDIT-DED-01` terbit | Lolos Rencana |
| `AT-HRD-DED-06` | Retry Eksponensial saat Sinkronisasi Finance Timeout | Resilience Test | Simulasi service Keuangan mengalami downtime saat job tgl 17 berjalan | Endpoint Finance melempar HTTP 504 Gateway Timeout | Sistem mencoba ulang otomatis pada menit 5, 15, dan 30, lalu menerbitkan alert peringatan jika tetap gagal | Lolos Rencana |
| `AT-HRD-DED-07` | Penguncian Jadwal Cicilan per Tgl 20 | Boundary Test | Percobaan menambahkan jadwal cicilan bulan berjalan pada tgl 21 | Timestamp transaksi: 21 November 08:00:00 WIB | Request ditolak sistem (`VR-DED-17`), data diarahkan otomatis sebagai jadwal siklus bulan Desember | Lolos Rencana |
| `AT-HRD-DED-08` | Pengiriman Webhook Callback Realisasi ke Finance | Integration Test | Sahkan slip gaji final tgl 25 dan verifikasi kiriman payload callback | Status Payroll Run: `Approved`, ada 142 potongan sah | Webhook `INT-HR-58` terkirim dengan HMAC signature, Finance membalas `200 OK` nomor jurnal akuntansi | Lolos Rencana |
| `AT-HRD-DED-09` | Pengajuan Klaim Kacamata Kerja di ESS | Fungsional ESS | Staf mengajukan klaim kacamata lengkap dengan struk optik & resep | Pengeluaran: Rp 1.200.000, Kuota Plafon: Rp 1.500.000 | Berkas terbit berstatus `Submitted`, plafon dikunci Rp 1.200.000, notifikasi approval masuk ke atasan | Lolos Rencana |
| `AT-HRD-DED-10` | Penolakan Klaim Melebihi Kuota Plafon Tahunan | Validation Rule | Staf mencoba mengajukan klaim dengan nominal melebihi sisa kuota | Sisa Kuota: Rp 500.000, Nilai Kuitansi: Rp 1.000.000 | Form menolak submit di ESS (`VR-DED-10`), menampilkan peringatan kuota tidak mencukupi | Lolos Rencana |
| `AT-HRD-DED-11` | Penolakan Kuitansi Berusia Lebih dari 60 Hari | Validation Rule | Staf menginput nota kuitansi yang bertanggal 75 hari yang lalu | Tanggal Nota: H-75 dari hari pengajuan | Validasi gagal (`VR-DED-09`), sistem menolak berkas dengan pesan kuitansi kedaluwarsa | Lolos Rencana |
| `AT-HRD-DED-12` | Uji Anti-Self-Approval Klaim Restitusi | Security / SoD | Kepala Ruangan mencoba menyetujui klaim restitusi atas dirinya sendiri | Akun login = Pemohon klaim restitusi | Tombol approve terblokir (`VR-DED-08`), sistem menugaskan persetujuan ke Kepala Instalasi / Wadir | Lolos Rencana |
| `AT-HRD-DED-13` | Alur Verifikasi Bertingkat Lengkap (3-Tier) | Workflow Test | Jalankan alur persetujuan: Atasan $
ightarrow$ HR C&B $
ightarrow$ Manajer SDM | Dokumen sah, cap stempel faskes luar valid | Status bertransisi: `Submitted` $
ightarrow$ `SupervisorApproved` $
ightarrow$ `HrReviewed` $
ightarrow$ `Approved` | Lolos Rencana |
| `AT-HRD-DED-14` | Injeksi Klaim Approved ke Penggajian Bulanan | End-to-End Test | Klaim approved per tgl 15 dimasukkan ke perhitungan payroll tgl 25 | Klaim non-cito, approved amount: Rp 1.000.000 | Komponen `TrxPayrollRestitution` masuk slip gaji sebagai tunjangan non-pajak, status klaim `Processed` | Lolos Rencana |
| `AT-HRD-DED-15` | Restitusi Koreksi Salah Potong Periode Lampau | End-to-End Test | HR mengembalikan salah potong gaji bulan lalu tanpa revisi slip lama | Slip Oktober salah potong Rp 500.000 | Terbit `TrxPayrollRestitution` linked ke slip Oktober, uang dikembalikan pada slip gaji November berjalan | Lolos Rencana |
| `AT-HRD-DED-16` | Kompensasi Salah Potong Bebas Pajak PPh 21 | Tax Compliance | Hitung pajak PPh 21 pada slip gaji yang memuat koreksi salah potong | Gaji Kotor: Rp 10jt, Koreksi Salah Potong: Rp 1jt | PPh 21 dihitung dari dasar Rp 10jt; komponen koreksi Rp 1jt tidak dikenai pajak tambahan (`VR-DED-05`) | Lolos Rencana |
| `AT-HRD-DED-17` | Penerbitan Voucher Kasir Cito Darurat Medis Nakes | Fungsional Kasir | Dokter IGD tertusuk jarum butuh obat cito, direktur setujui jalur cito | Flag `IsCitoEmergency = true`, nominal Rp 750.000 | Terbit `TrxCashierDisbursementVoucher` ber-token SHA-256 dan berlaku tepat 48 jam (`VR-DED-13`) | Lolos Rencana |
| `AT-HRD-DED-18` | Pencairan Uang Tunai di Loket Kasir RS 24/7 | Integration Test | Kasir IGD memindai QR voucher dan menyerahkan uang tunai | Token valid, kasir input nomor kuitansi kasir | Status voucher menjadi `DisbursedViaCashier`, kasir mencetak bukti tanda terima fisik | Lolos Rencana |
| `AT-HRD-DED-19` | Proteksi Nol Risiko Bayar Ganda Kasir vs Payroll | Safety Invariant | Verifikasi payroll run tgl 25 saat ada klaim yang sudah cair di kasir | Klaim telah berstatus `DisbursedViaCashier` | Mesin penggajian mengabaikan klaim tersebut; dana TIDAK ditransfer ganda pada slip gaji tgl 25 | Lolos Rencana |
| `AT-HRD-DED-20` | Pelunasan Sisa Pinjaman saat PHK (Exit Clearance) | Inter-Module Sub-20 | Pegawai resign memiliki sisa pinjaman Rp 6jt, hak pesangon Rp 10jt | Integrasi Sub-20 Exit Clearance, sisa utang Rp 6jt | Hak pesangon dipotong Rp 6jt (pengecualian batas 50%), utang lunas, Exit Clearance disahkan | Lolos Rencana |


---

## 24. Matriks Pengujian Penerimaan Sub-Modul 12 (Slice S-D3 Benefit Management, AT-HRD-BEN-01 s.d. AT-HRD-BEN-20)

Bagian ini mendefinisikan 20 skenario pengujian penerimaan (*Acceptance Tests*) untuk **Sub-Modul 12 (Benefit Management — Slice `S-D3`)**, mencakup jalur pengujian berhasil (*happy path*), skenario penolakan validasi batas kuota/usia tanggungan, pengujian koordinasi manfaat COB BPJS, transaksi billing SIMRS real-time, aktivasi bertahap, dan pengakhiran hak pada Exit Clearance.

---

### Tabel Spesifikasi Skenario Uji (`AT-HRD-BEN-01` s.d. `AT-HRD-BEN-20`)

| ID Skenario | Kategori Pengujian | Deskripsi Skenario Uji & Kondisi Awal | Tindakan Pengujian (Action Steps) | Hasil yang Diharapkan (Expected Result) | Status |
| :--- | :--- | :--- | :--- | :--- | :---: |
| `AT-HRD-BEN-01` | Fungsional | Pembuatan master paket manfaat 3-lapisan baru | HR Manager membuat paket `BEN-PVT-EXEC` dengan limit Rp 50.000.000 dan mandatory COB BPJS. | Data paket tersimpan dengan benar, status `Active`, dan tier kamar rawat inap tercatat `VVIP`. | `Rencana` |
| `AT-HRD-BEN-02` | Fungsional | Pemetaan matriks kelayakan grade staf | HR mengaitkan grade `GR-MED-SP` ke paket Executive dan grade `GR-NUR-02` ke paket Staff. | Matriks kelayakan tersimpan, endpoint inquiry mengembalikan paket yang tepat sesuai grade nakes. | `Rencana` |
| `AT-HRD-BEN-03` | Happy Path | Aktivasi cepat BPJS TK H+0 Onboarding | Pegawai baru dr. Siti menyelesaikan onboarding hari pertama kerja (Sub-04 `S-C4-ONB`). | BPJS Ketenagakerjaan (JKK & JKM) langsung berstatus `Active`, sedangkan BPJS Kes & Swasta `PendingActivation`. | `Rencana` |
| `AT-HRD-BEN-04` | Boundary / Rule | Penolakan pendaftaran pasangan kedua | Pegawai yang sudah memiliki 1 pasangan terdaftar mencoba mendaftarkan pasangan kedua di ESS. | Sistem memblokir pendaftaran dengan pesan error `VR-BEN-01` (kuota pasangan sah maksimal 1 orang). | `Rencana` |
| `AT-HRD-BEN-05` | Boundary / Rule | Penolakan pendaftaran anak keempat | Pegawai yang sudah memiliki 3 anak sah mencoba mendaftarkan anak keempat di ESS. | Sistem memblokir pendaftaran dengan pesan error `VR-BEN-02` (kuota anak sah maksimal 3 orang). | `Rencana` |
| `AT-HRD-BEN-06` | Boundary / Rule | Penolakan anak usia > 21 tahun tanpa surat kuliah | Pegawai mendaftarkan anak berusia 22 tahun tanpa melampirkan surat keterangan mahasiswa aktif. | Sistem memblokir submit dengan pesan error `VR-BEN-07` (mandatory surat keterangan dekanat kampus). | `Rencana` |
| `AT-HRD-BEN-07` | Happy Path | Penerimaan anak kuliah usia 23 tahun ber-surat kuliah | Pegawai mendaftarkan anak usia 23 tahun disertai unggahan scan surat keterangan dekanat kampus yang valid. | Berkas berhasil diunggah, status `Submitted`, masa berlaku diset hingga 31 Agustus tahun ajaran berjalan. | `Rencana` |
| `AT-HRD-BEN-08` | Fungsional | Verifikasi dokumen kependudukan oleh HR | HR Benefit Officer memeriksa scan KK dan akta nikah staf, lalu mengklik "Setujui". | Status tanggungan berubah menjadi `Verified`, hak subsidi berobat aktif bagi tanggungan tersebut. | `Rencana` |
| `AT-HRD-BEN-09` | Fungsional | Penolakan berkas buram dengan catatan perbaikan | HR Benefit Officer menolak berkas scan KK yang buram dan mengisi catatan "Foto KK tidak terbaca". | Status berubah menjadi `Rejected`, notifikasi perbaikan muncul di portal ESS pegawai. | `Rencana` |
| `AT-HRD-BEN-10` | Happy Path | Aktivasi paket lengkap pasca-lulus masa percobaan | Pegawai dinyatakan lulus masa percobaan 3 bulan (`ProbationPassed`) pada evaluasi Sub-Modul 05. | Event memicu pengubahan status enrollment menjadi `Active`, dan saldo awal plafon RS terbentuk. | `Rencana` |
| `AT-HRD-BEN-11` | Integrasi SIMRS | Inquiry eligibilitas berobat di loket admisi RS | Petugas registrasi SIMRS menginput NIP pegawai nakes yang berobat di RS Quilvian sendiri (`INT-HR-61`). | Sistem HRD merespons dengan status `Eligible`, hak diskon staf 30%, dan sisa saldo plafon berjalan. | `Rencana` |
| `AT-HRD-BEN-12` | Integrasi SIMRS | Pemotongan saldo plafon internal saat billing lunas | Kasir SIMRS menutup billing pemeriksaan rawat jalan nakes senilai Rp 800.000 (diskon menjadi Rp 560.000). | Saldo plafon berkurang tepat Rp 560.000, mutasi tercatat pada `HrdBenefitBalanceMovement` tipe `HospitalCareUsage`. | `Rencana` |
| `AT-HRD-BEN-13` | Boundary / Rule | Penolakan pemotongan saldo melebihi sisa limit | Kasir SIMRS mencoba memotong saldo Rp 2.000.000 pada pegawai yang sisa saldo plafonnya hanya Rp 500.000. | Transaksi ditolak atau dipotong maksimal Rp 500.000; sisa Rp 1.500.000 diterbitkan sebagai ekses tagihan (`VR-BEN-14`). | `Rencana` |
| `AT-HRD-BEN-14` | Integrasi Payroll| Pendelegasian ekses tagihan medis ke slip gaji | Pegawai memilih melunasi ekses tagihan rawat inap Rp 1.500.000 melalui pemotongan slip gaji bulan berjalan. | Event terkirim ke Sub-11 `S-B6` (`INT-HR-63`), terjadwal pada prioritas ke-2 pemotongan upah. | `Rencana` |
| `AT-HRD-BEN-15` | Happy Path | Pengajuan klaim COB sekunder berlampiran SEP BPJS | Nakes mengajukan klaim selisih naik kelas VIP dengan melampirkan nomor SEP BPJS valid dan rincian kuitansi. | Klaim tersimpan dengan nomor `CLM-BEN/...`, status `Submitted`, nomor SEP tercatat pada kolom `sep_bpjs_number`. | `Rencana` |
| `AT-HRD-BEN-16` | Boundary / Rule | Penolakan klaim COB tanpa nomor SEP BPJS | Nakes mencoba mengajukan klaim selisih rawat inap swasta tanpa menginput nomor SEP BPJS Kesehatan. | Pengajuan diblokir oleh validasi sistem dengan pesan error `VR-BEN-16` (Mandatory SEP BPJS Kesehatan). | `Rencana` |
| `AT-HRD-BEN-17` | Fungsional | Penyelesaian dan settlement klaim sekunder | HR Manager menyetujui klaim selisih biaya Rp 3.500.000 dan menginput referensi pencairan bank. | Status klaim berubah menjadi `Settled`, tanggal settlement terisi, notifikasi pelunasan terkirim ke ESS. | `Rencana` |
| `AT-HRD-BEN-18` | Otomasi Cron | Reset hangus saldo plafon akhir tahun | Simulasi pergantian tahun 1 Januari pukul 00:00:00 WIB dijalankan. | Sisa saldo tahun lalu di-reset menjadi Rp 0.00 (`YearEndExpiryReset`), saldo baru dialokasikan sesuai grade. | `Rencana` |
| `AT-HRD-BEN-19` | Integrasi Offb | Blokir Exit Clearance jika ada ekses tertunggak | Pegawai yang mengajukan resign masih memiliki tagihan ekses rawat inap belum lunas di kasir RS. | Checklist Exit Clearance Sub-Modul 20 menolak sign-off dengan peringatan `VR-BEN-24`. | `Rencana` |
| `AT-HRD-BEN-20` | Happy Path | Terminasi benefit otomatis & surat mutasi BPJS | Pegawai resmi keluar kerja pada `EffectiveTerminationDate`. | Status kepesertaan otomatis berubah menjadi `Terminated`, dan PDF Surat Portabilitas BPJS Mandiri terbit. | `Rencana` |


---

## 25. Matriks Pengujian Penerimaan Sub-Modul 13 (Slice S-D4 HR Service Management & Request, AT-HRD-HRS-01 s.d. AT-HRD-HRS-20)

Bagian ini mendefinisikan 20 skenario pengujian penerimaan (*Acceptance Tests*) untuk **Sub-Modul 13 (Slice `S-D4`)**, mencakup jalur berhasil (*happy path*), skenario batas (*boundary*), pengujian penegakan SLA 3-tier, penarikan gaji otomatis Sub-10, proteksi PIN finansial, verifikasi portal publik ber-QR Code, serta pencabutan keabsahan dokumen (revokasi).

### Tabel Spesifikasi Skenario Uji (`AT-HRD-HRS-01` s.d. `AT-HRD-HRS-20`)

| ID Skenario | Kategori Pengujian | Deskripsi Skenario Uji & Kondisi Awal | Tindakan Pengujian (Action Steps) | Hasil yang Diharapkan (Expected Result) | Status |
| :--- | :--- | :--- | :--- | :--- | :---: |
| `AT-HRD-HRS-01` | Happy Path | Pengajuan tiket mandiri ESS dengan berkas lengkap | Staf perawat mengajukan tiket permohonan SKK di ESS dengan melampirkan berkas persyaratan. | Tiket tersimpan dengan status `Submitted`, nomor tiket unik `TKT-202611-XXXXX` diterbitkan. | `Rencana` |
| `AT-HRD-HRS-02` | Boundary / Rule | Penolakan prioritas Cito tanpa surat disposisi medis | Staf memilih prioritas Cito (SLA 4 Jam) tetapi tidak mengunggah berkas disposisi atasan klinis. | Sistem memblokir pengajuan tiket dengan pesan error `VR-HRS-02` (mandatory surat disposisi cito). | `Rencana` |
| `AT-HRD-HRS-03` | Fungsional | Inisiasi timer SLA 3-tier berbasis jam kerja RS | Tiket kategori Standar disubmit pada hari Jumat pukul 15:00 WIB (SLA target 24 jam kerja). | `sla_deadline_utc` dihitung melompati akhir pekan, jatuh tepat pada hari Selasa minggu berikutnya pukul 14:00 WIB. | `Rencana` |
| `AT-HRD-HRS-04` | Fungsional | Penangguhan timer SLA saat minta klarifikasi staf | Analis HR mengubah status tiket menjadi `WaitingStaffResponse` untuk meminta scan KTP yang lebih jelas. | Timer SLA berstatus *Paused*, durasi jam berhenti dihitung sampai staf membalas klarifikasi. | `Rencana` |
| `AT-HRD-HRS-05` | Happy Path | Resume timer SLA saat staf membalas klarifikasi | Staf pemohon mengunggah scan KTP perbaikan melalui form klarifikasi di portal ESS. | Status tiket kembali ke `InReview`, timer SLA otomatis melanjutkan penghitungan sisa waktu (*Resumed*). | `Rencana` |
| `AT-HRD-HRS-06` | Otomasi Cron | Alert peringatan dini kuning pada 75% durasi SLA | Tiket helpdesk telah berjalan mencapai 18 jam dari target 24 jam tanpa ada penyelesaian analis. | Worker mengubah `sla_status = 'Warning75'`, notifikasi peringatan dini terkirim ke HR Supervisor. | `Rencana` |
| `AT-HRD-HRS-07` | Otomasi Cron | Auto-eskalasi merah ke Manajer SDM saat 100% overdue | Tiket helpdesk melewati batas waktu deadline `sla_deadline_utc` tanpa ada tindakan resolusi. | Status berubah menjadi `Overdue`, tiket otomatis dieskalasi ke Manajer SDM (`Escalated`) dan KPI berkurang. | `Rencana` |
| `AT-HRD-HRS-08` | Happy Path | Penarikan gaji otomatis Sub-10 pada surat gaji KPR | Analis HR membuat draf Surat Keterangan Gaji KPR untuk dokter spesialis. | Nominal gaji pokok, tunjangan, dan penerimaan bersih ditarik otomatis dari slip gaji final Sub-10 (`INT-HR-56`). | `Rencana` |
| `AT-HRD-HRS-09` | Boundary / Rule | Penolakan surat gaji jika payroll belum finalized | Staf baru mengajukan surat keterangan gaji namun slip gaji bulan berjalan masih berstatus draft di Sub-10. | Sistem menolak pembuatan draf dengan pesan error `VR-HRS-05` (data payroll belum finalized). | `Rencana` |
| `AT-HRD-HRS-10` | Security / Guard | Blokir pengetikan nominal manual pada formulir surat | Analis HR mencoba memanipulasi payload angka gaji kotor melalui intercept request API. | API menolak payload request karena nilai gaji wajib di-resolve server-side dari riwayat payroll (`VR-HRS-06`). | `Rencana` |
| `AT-HRD-HRS-11` | Fungsional | Mandatory gerbang persetujuan C&B Supervisor | Pejabat mencoba mengotorisasi surat gaji yang belum memperoleh approval Supervisor C&B. | Tindakan diblokir dengan pesan error `VR-HRS-07` (mandatory persetujuan C&B sebelum otorisasi). | `Rencana` |
| `AT-HRD-HRS-12` | Security / SoD | Pencegahan otorisasi mandiri oleh pejabat (Anti-Self-Approval)| Manajer SDM mencoba menandatangani Surat Keterangan Kerja atas nama dirinya sendiri. | Sistem menolak aksi otorisasi dengan pesan error `VR-HRS-08` (pejabat dilarang menyetujui suratnya sendiri). | `Rencana` |
| `AT-HRD-HRS-13` | Concurrency | Mesin penomoran surat resmi gapless sequence | 10 permohonan surat disahkan secara simultan dalam 1 detik yang sama. | Seluruh surat memperoleh nomor urut yang unik berurutan tanpa ada nomor lompat atau duplikasi (`HRS-B3`). | `Rencana` |
| `AT-HRD-HRS-14` | Integrasi Sub-02 | Pemutakhiran otomatis nomor rekening bank payroll | Analis HR menyetujui tiket permohonan ubah rekening payroll dari Bank BCA ke Bank Mandiri. | Kolom rekening pada `TrxEmployeeBankDetail` Sub-02 ter-update otomatis secara atomik (`INT-HR-65`). | `Rencana` |
| `AT-HRD-HRS-15` | Audit Forensik | Rekam snapshot histori data profil sebelum-sesudah | Perubahan rekening bank dieksekusi otomatis oleh sistem. | Data lama tersimpan di `HrdProfileChangeHistory` dalam format JSON, lengkap dengan ID user approver. | `Rencana` |
| `AT-HRD-HRS-16` | Integrasi Sub-02 | Pengarsipan otomatis salinan surat ke E-Dossier | Surat resmi berstatus `Issued` diterbitkan sistem. | File PDF otomatis disalin ke `MstWorkforceProfileDocument` Sub-02 dan dapat diunduh staf di menu "Dokumen Saya". | `Rencana` |
| `AT-HRD-HRS-17` | Public Portal | Scan QR Code publik menampilkan metadata legalitas tersamar | Pihak bank memindai QR Code surat keterangan penghasilan fisik melalui kamera ponsel. | Halaman `/verify/document/{hash}` terbuka: nomor surat & nama sah, namun nominal gaji tertutup `Rp ••••••••••`. | `Rencana` |
| `AT-HRD-HRS-18` | Security / PIN | Pembukaan nominal gaji sah dengan 4-digit PIN rahasia | Pihak bank memasukkan 4-digit PIN rahasia yang diberikan oleh dokter bersangkutan. | Sistem memvalidasi hash PIN (BCrypt) dan menampilkan rincian nominal gaji pokok serta tunjangan sah. | `Rencana` |
| `AT-HRD-HRS-19` | Security / Brute | Pembatasan salah tebak PIN di portal verifikasi publik | Pihak luar mencoba menebak PIN secara brute force dengan memasukkan PIN salah 3 kali berturut-turut. | Akses verifikasi PIN untuk dokumen tersebut dikunci sementara selama 30 menit (`VR-HRS-12`). | `Rencana` |
| `AT-HRD-HRS-20` | Happy Path | Protokol pencabutan surat resmi terbit (Revokasi) | Manajer SDM menginput nomor BAP dan mencabut surat keterangan kerja pegawai yang terindikasi fraud. | Status berubah menjadi `Revoked`, scan QR publik seketika memunculkan banner merah: "DOKUMEN INI TELAH DICABUT". | `Rencana` |


---

## 26. Matriks Pengujian Penerimaan Sub-Modul 14 (Slice `S-C1` Credentialing, License & Clinical Privilege, `AT-HRD-CRD-01` s.d. `AT-HRD-CRD-22`)

Bagian ini mendefinisikan 22 skenario pengujian penerimaan (*Acceptance Tests*) untuk **Sub-Modul 14 (Slice `S-C1`)**, mencakup verifikasi sumber primer (PSV) STR seumur hidup, radar kedaluwarsa SIP 5 tahun, pemisahan 3 komite mandiri, penilaian panel Mitra Bestari, otorisasi tunggal Direktur Utama, penegakan Hard-Stop di SIMRS/EMR (`INT-HR-69`), life-saving emergency override, kewenangan sementara/darurat, serta siklus rekredensialing 3 tahunan.

### Tabel Spesifikasi Skenario Uji (`AT-HRD-CRD-01` s.d. `AT-HRD-CRD-22`)

| ID Skenario | Kategori Pengujian | Deskripsi Skenario Uji | Kondisi Awal (Pre-conditions) | Langkah Eksekusi / Aksi | Hasil yang Diharapkan (Expected Result) | Status |
| :--- | :--- | :--- | :--- | :--- | :--- | :---: |
| `AT-HRD-CRD-01` | Fungsional | Pendaftaran STR seumur hidup & verifikasi PSV Kemenkes | Dokter mengunggah nomor STR ke sistem | Petugas kredensialing memasukkan nomor STR ke portal SatuSehat SDMK & klik verifikasi | `is_psv_verified = true`, URL verifikasi & stempel audit tersimpan di database lisensi. | `Rencana` |
| `AT-HRD-CRD-02` | Negatif / Validasi | Penolakan disposisi berkas jika STR belum terverifikasi PSV | Berkas kredensial baru di-submit | Petugas mencoba meneruskan berkas ke panel reviewer sebelum lolos PSV | Sistem menolak aksi dengan error `VR-CRD-02`, berkas tetap berstatus `PendingPSV`. | `Rencana` |
| `AT-HRD-CRD-03` | Fungsional | Registrasi lisensi SIP 5 tahunan faskes rumah sakit | STR dokter telah berstatus terverifikasi PSV | Input nomor SIP Dinkes, tanggal terbit, dan tanggal kedaluwarsa (tepat 5 tahun) | SIP tersimpan status `Active`, cron job pemantau radar kedaluwarsa terdaftar di scheduler. | `Rencana` |
| `AT-HRD-CRD-04` | Negatif / Boundary | Penolakan masa berlaku SIP melebihi batas 5 tahun | Input data SIP baru di portal admin | Masukkan masa berlaku 5 tahun lebih 1 hari sejak tanggal penerbitan | Sistem menolak penyimpanan dengan validasi `VR-CRD-03` (maksimal 5 tahun). | `Rencana` |
| `AT-HRD-CRD-05` | Background Worker | Eksekusi radar peringatan dini SIP H-90, H-60, H-30, H-7 | Sisa masa berlaku SIP mencapai 90 hari | Worker `SipExpirationMonitorJob` dijalankan | Email & notifikasi ESS peringatan perpanjangan terkirim ke nakes dan kepala bidang. | `Rencana` |
| `AT-HRD-CRD-06` | Fatal Safety Gate | Eksekusi fail-closed H-0: SIP kedaluwarsa | SIP mencapai jam 00:00 tanggal jatuh tempo | Scheduler mengeksekusi `OnSipExpired` event | Akun order SIMRS dinonaktifkan (`INT-HR-69`) dan jadwal roster Sub-06 terblokir (`INT-HR-71`). | `Rencana` |
| `AT-HRD-CRD-07` | Fungsional | Pengajuan berkas kredensial dengan pemisahan 3 komite | Staf perawat login ke portal ESS | Pilih permohonan kredensial dan isi checklist whitebook | Berkas otomatis diarahkan ke Komite Keperawatan, form terkunci untuk ranah keperawatan. | `Rencana` |
| `AT-HRD-CRD-08` | Keamanan & Integritas | Penugasan panel Mitra Bestari & blokir anti-self-review | Berkas permohonan dokter spesialis bedah | Ketua subkomite menunjuk panel reviewer sebidang (Reviewer 1 & Reviewer 2) | Sistem menerima penugasan min 2 reviewer; menolak jika dokter menugaskan dirinya sendiri (`VR-CRD-15`). | `Rencana` |
| `AT-HRD-CRD-09` | Fungsional | Asesmen portofolio oleh panel Mitra Bestari | Berkas berada di status `PeerReview` | Masing-masing reviewer menilai checklist tindakan (Mandiri / Supervisi / Ditolak) | Berita acara asesmen tertandatangani digital, status berkas maju ke `PlenaryReview`. | `Rencana` |
| `AT-HRD-CRD-10` | Fungsional | Sidang pleno komite menyetujui rekomendasi kewenangan | Berkas berada di status `PlenaryReview` | Pengurus komite menggelar sidang dan Ketua Komite mengesahkan risalah | Surat usulan resmi Ketua Komite terbit; draf SPK-RKK ter-generate di sistem. | `Rencana` |
| `AT-HRD-CRD-11` | Validasi | Keharusan penetapan dokter supervisor untuk Level 3 | Tindakan bedah disetujui berstatus supervisi | Sekretariat menyusun draf SPK tanpa memasukkan nama supervisor pendamping | Sistem menolak draf dengan error `VR-CRD-05`; berhasil saat nama supervisor disematkan. | `Rencana` |
| `AT-HRD-CRD-12` | Fungsional | Paraf manajerial Direktur Pelayanan Medik / Keperawatan | Draf SPK-RKK telah memuat supervisor | Direktur Pelayanan Medik mereview kesesuaian operasional dan membubuhkan paraf | Status berkas naik ke `PendingDirectorSignature`. | `Rencana` |
| `AT-HRD-CRD-13` | Otoritas Kritis | Pengesahan tunggal SPK & RKK oleh Direktur Utama RS | Draf SPK berstatus `PendingDirectorSignature` | Direktur Utama RS login dan membubuhkan e-Sign sah | Dokumen SPK-RKK resmi aktif, nomor register terbit, masa berlaku dikunci 3 tahun (`HRD-DEC-148`). | `Rencana` |
| `AT-HRD-CRD-14` | Negatif / Keamanan | Penolakan pengesahan SPK oleh selain Direktur Utama | Akun Direktur Pelayanan mencoba sign SPK | Kirim request POST ke endpoint pengesahan akhir | Sistem mengembalikan `403 Forbidden` (`VR-CRD-12`), SPK tidak sah tanpa tanda tangan Dirut. | `Rencana` |
| `AT-HRD-CRD-15` | Integrasi | Auto-arsip dokumen sah SPK ke e-Dossier Sub-02 | SPK resmi telah ditandatangani Dirut | Sistem memicu integrasi `INT-HR-72` | Berkas PDF ber-QR Code tersimpan abadi di repositori e-Dossier pegawai Sub-02. | `Rencana` |
| `AT-HRD-CRD-16` | Fungsional Publik | Verifikasi keabsahan dokumen via pemindaian QR Code | Dokumen fisik SPK dicetak ber-QR Code | Kamera smartphone memindai barcode QR dokumen | Halaman web publik menampilkan status `DOKUMEN ASLI & AKTIF`, nama nakes, dan daftar RKK. | `Rencana` |
| `AT-HRD-CRD-17` | Integrasi SIMRS | Evaluasi wewenang mandiri Level 4 di SIMRS EMR | Dokter memiliki RKK tindakan Level 4 mandiri | SIMRS memanggil `INT-HR-69` saat dokter membuat order tindakan | Gate mengembalikan `ALLOW_INDEPENDENT` (latensi < 50ms), order langsung diproses SIMRS. | `Rencana` |
| `AT-HRD-CRD-18` | Integrasi SIMRS | Evaluasi wewenang supervisi Level 3 & alur co-signature | Dokter memiliki RKK tindakan Level 3 supervisi | SIMRS memanggil `INT-HR-69` saat dokter membuat order tindakan | Gate mengembalikan `ALLOW_UNDER_SUPERVISION`; order tertahan hingga di-co-sign supervisor. | `Rencana` |
| `AT-HRD-CRD-19` | Fatal Hard-Stop | Blokir keras order tindakan klinis tidak berwenang | Dokter tidak memiliki kewenangan tindakan (Level 1-2) | Dokter mencoba menginput order prosedur di EMR | Resolusi `HRD-DEC-005` & `HRD-DEC-149`: Tombol eksekusi dinonaktifkan keras dengan pesan penolakan. | `Rencana` |
| `AT-HRD-CRD-20` | Audit Keselamatan | Bypass darurat cito penyelamatan nyawa (Life-Saving Override) | Order diblokir Hard-Stop pada kasus henti jantung | Dokter menekan tombol 'Darurat Nyawa' & isi alasan cito | Order darurat dieksekusi, log audit tersimpan, alert insiden terkirim ke KMKP dalam 24 jam. | `Rencana` |
| `AT-HRD-CRD-21` | Fungsional | Penerbitan kewenangan klinis sementara dokter konsultan tamu | Dokter tamu memiliki SIP aktif faskes asal | Pengajuan fast-track disahkan Ketua Komite Medik & Dirut | SPK sementara terbit dengan masa berlaku maksimal 3 bulan (`HRD-DEC-150`). | `Rencana` |
| `AT-HRD-CRD-22` | Kedaruratan Bencana | Penerbitan kewenangan darurat cito bencana massal | Terjadi bencana gempa / wabah KLB | Direktur Pelayanan Medik mengotorisasi relawan nakes cito | Wewenang darurat aktif seketika dengan pembatasan otomatis maksimal 72 jam (`CRD-B6`). | `Rencana` |

---

## 27. Matriks Pengujian Penerimaan Sub-Modul 15 (Slice `S-C1-OPPE` OPPE, FPPE & Recredentialing, `AT-HRD-OPP-01` s.d. `AT-HRD-OPP-22`)

Bagian ini mendefinisikan 22 skenario pengujian penerimaan (*Acceptance Tests*) untuk **Sub-Modul 15 (Slice `S-C1-OPPE`)**, mencakup evaluasi berkala OPPE dengan bobot 60% SIMRS/KMKP + 40% telaah sejawat, penegakan kerahasiaan *Peer Review Privilege* dengan perekaman audit pembacaan (*read-audit log*), deteksi ambang pemicu kritis insiden sentinel & komplikasi > 2 SD, tata laksana proctoring FPPE dokter baru & prosedur baru, kalkulator komposit 3 tahun rekredensialing berbobot 25%-35%-40%, klasifikasi tier kelayakan (Hijau/Kuning/Merah), serta sinkronisasi 4 putusan akhir ke SIMRS EMR (`INT-HR-76`).

### Tabel Spesifikasi Skenario Uji (`AT-HRD-OPP-01` s.d. `AT-HRD-OPP-22`)

| ID Skenario | Kategori Pengujian | Deskripsi Skenario Uji | Kondisi Awal (Pre-conditions) | Langkah Eksekusi / Aksi | Hasil yang Diharapkan (Expected Result) | Status |
| :--- | :--- | :--- | :--- | :--- | :--- | :---: |
| `AT-HRD-OPP-01` | Fungsional | Pembukaan siklus periode OPPE baru dengan rasio 60:40 | Admin Komite Medik login ke menu konfigurasi OPPE | Buka periode evaluasi tahun 2026 siklus Semester 1 dengan bobot default 60% kuantitatif dan 40% kualitatif | Periode tersimpan status `Active`, berkas evaluasi draf terbentuk untuk seluruh dokter aktif di unit. | `Rencana` |
| `AT-HRD-OPP-02` | Negatif / Validasi | Penolakan konfigurasi bobot OPPE jika total tidak 100% | Pembuatan siklus periode evaluasi | Masukkan pembobotan 70% kuantitatif dan 40% kualitatif | Sistem menolak dengan validasi `VR-OPP-01` (total wajib tepat 100%). | `Rencana` |
| `AT-HRD-OPP-03` | Integrasi SIMRS | Agregasi otomatis indikator kuantitatif dari SIMRS/KMKP | Periode OPPE aktif & dokter memiliki riwayat pelayanan | Memicu job ekstraksi data SIMRS (`INT-HR-73`) | Nilai volume tindakan, kepatuhan EMR 24 jam, dan kepatuhan Clinical Pathway terhitung akurat. | `Rencana` |
| `AT-HRD-OPP-04` | Fungsional | Penugasan formulir telaah sejawat ke dokter rekan KSM | Rapor evaluasi dokter berstatus `InReview` | Subkomite menugaskan formulir peer review ke dokter sejawat sekelompok KSM | Form evaluasi kualitatif muncul di dashboard portal penilai rekan sejawat. | `Rencana` |
| `AT-HRD-OPP-05` | Keamanan & Integritas | Penolakan penilaian sejawat mandiri (anti-self-review) | Dokter penilai login ke portal ESS | Mencoba memilih berkas penilaian atas nama dirinya sendiri | Sistem menolak aksi dengan kode error fatal `VR-OPP-03`. | `Rencana` |
| `AT-HRD-OPP-06` | Validasi Kualitatif | Keharusan narasi klinis jika skor telaah sejawat <= 2 | Dokter penilai mengisi form peer review | Berikan skor likert 2 pada etika profesi tanpa mengisi kolom ulasan deskriptif | Form menolak submit dengan pesan error `VR-OPP-04` (wajib minimal 25 karakter penjelasan). | `Rencana` |
| `AT-HRD-OPP-07` | Fatal Safety / Privasi | Penegakan Peer Review Privilege & perekaman read-audit | Petugas membuka detail lembar ulasan kualitatif sejawat | Buka dokumen telaah kualitatif di antarmuka web | Dialog peringatan konfidensialitas muncul & log keamanan `PeerReviewDocumentRead` otomatis tercatat. | `Rencana` |
| `AT-HRD-OPP-08` | Algoritma Kalkulasi | Perhitungan nilai komposit akhir 60% SIMRS + 40% Peer | Data kuantitatif (80) dan nilai kualitatif (90) terisi | Sistem memproses kalkulasi komposit | Nilai akhir komposit terhitung tepat: (0.60 * 80) + (0.40 * 90) = 84.00, status lulus `TRUE`. | `Rencana` |
| `AT-HRD-OPP-09` | Fungsional | Pengesahan rapor OPPE dan konfirmasi baca oleh nakes | Skor komposit terhitung lengkap | Subkomite mengesahkan rapor; nakes login ke portal mandiri untuk konfirmasi | Rapor berstatus `Acknowledged`, dokumen PDF ber-QR Code diarsipkan ke e-Dossier Sub-02. | `Rencana` |
| `AT-HRD-OPP-10` | Real-Time Trigger | Deteksi insiden sentinel memicu penugasan FPPE seketika | Modul keselamatan pasien SIMRS mencatat 1 insiden sentinel | Sinyal `INT-HR-74` diterima sistem manajemen kredensialing | Kasus FPPE terarah otomatis terinisiasi dengan flag pemicu bahaya merah (`VR-OPP-05`). | `Rencana` |
| `AT-HRD-OPP-11` | Analisis Tren | Pemicu FPPE komplikasi bedah melampaui 2 standar deviasi | Data SIMRS mencatat tren komplikasi bedah 3 bulan berturut | Worker evaluasi bulanan memindai agregat indikator | Terdeteksi nilai komplikasi > 2 SD, sistem menerbitkan peringatan pemicu FPPE investigasi. | `Rencana` |
| `AT-HRD-OPP-12` | Kepatuhan EMR | Pemicu FPPE akibat keterlambatan resume EMR > 48 jam | Laporan KMKP mendeteksi penundaan resume EMR > 20% kasus | Sistem memproses evaluasi kepatuhan dokumentasi rekam medis | Terbit rekomendasi pelaksanaan audit FPPE keterlambatan EMR (`VR-OPP-07`). | `Rencana` |
| `AT-HRD-OPP-13` | Fungsional | Inisiasi proctoring staf dokter baru (3 bulan / 10 kasus) | Dokter spesialis baru selesai kredensialing awal | Ketua Komite menunjuk Dokter Proctor senior pendamping | Penugasan FPPE berstatus `InProgress` dengan target 10 kasus dalam kurun waktu 90 hari. | `Rencana` |
| `AT-HRD-OPP-14` | Fungsional | Inisiasi proctoring penambahan prosedur klinis baru | Dokter mengajukan kewenangan tindakan baru | Terbit penugasan FPPE khusus tindakan tersebut | Target kasus ditetapkan minimal 5 kasus dengan batas pemantauan maksimal 6 bulan. | `Rencana` |
| `AT-HRD-OPP-15` | Privasi Medis | Penyamaran (pseudonimisasi) identitas RM pasien pada logbook | Dokter mengentri kasus operasi ke logbook proctoring | Masukkan data tindakan klinis dan nomor register pasien | Sistem mengenkripsi/menyamarkan nomor rekam medis menjadi pseudonim ID terproteksi (`VR-OPP-11`). | `Rencana` |
| `AT-HRD-OPP-16` | Fungsional | Verifikasi tindakan dan penilaian kemandirian oleh proctor | Kasus terproctoring diinput oleh dokter kandidat | Dokter Proctor login dan memberikan nilai kemandirian (Level 3/4) | Logbook kasus bertanda tangan digital proctor, progres penugasan bertambah (misal 8/10 kasus). | `Rencana` |
| `AT-HRD-OPP-17` | Sidang Komite | Putusan pleno FPPE memutuskan perpanjangan masa proctoring | Periode proctoring selesai namun komite butuh observasi lanjutan | Sidang pleno menetapkan putusan `ProctoringExtended` (2 bulan) | Masa observasi bertambah 60 hari, status penugasan diperbarui menjadi `Extended` (`VR-OPP-13`). | `Rencana` |
| `AT-HRD-OPP-18` | Penurunan Derajat | Putusan pleno FPPE menurunkan kewenangan ke Level 3 supervisi | Terjadi komplikasi berulang saat evaluasi kasus | Sidang pleno menetapkan vonis `SupervisedPractice` | Status kewenangan klinis diturunkan ke Level 3 supervisi, wajib didampingi supervisor senior. | `Rencana` |
| `AT-HRD-OPP-19` | Algoritma Rekredensial | Kalkulasi rapor komposit 3 tahun dengan rasio 25-35-40 | Tersedia nilai OPPE Th 1 (80), Th 2 (85), Th 3 (90) | Jalankan kalkulator komposit rekredensialing | Skor akhir terhitung tepat: (0.25 * 80) + (0.35 * 85) + (0.40 * 90) = 85.75. | `Rencana` |
| `AT-HRD-OPP-20` | Klasifikasi Kelayakan | Otomasi pengelompokan ke Tier Hijau Fast-Track | Skor komposit 85.75 dan riwayat insiden sentinel = 0 | Sistem mengevaluasi tier kelayakan berkas | Berkas otomatis diklasifikasikan ke `Tier Hijau (Fast-Track)` perpanjangan tanpa hambatan. | `Rencana` |
| `AT-HRD-OPP-21` | Otoritas Diskresi | Penyesuaian diskresi tier kelayakan oleh Ketua Komite | Berkas berada di Tier Kuning | Ketua Komite Medik mengubah tier ke Hijau dengan mengunggah nota pertimbangan | Perubahan disetujui, event audit `RecredentialingTierOverridden` mencatat alasan klinis resmi. | `Rencana` |
| `AT-HRD-OPP-22` | Fatal Hard-Stop SIMRS | Transmisi putusan Revoked memblokir hak order SIMRS | Sidang rekredensialing menjatuhkan putusan `Revoked` | Sistem mengirimkan payload pembaruan wewenang ke SIMRS (`INT-HR-76`) | Menu peresepan dan pemesanan kamar operasi di SIMRS terkunci total secara seketika (*fail-closed*). | `Rencana` |

## 28. Matriks Pengujian Penerimaan Sub-Modul 16 (Slice S-C2 Learning, Training & Competency, AT-HRD-TRN-01 s.d. AT-HRD-TRN-22)

Matriks pengujian penerimaan (*User Acceptance Testing* - UAT) ini memuat 22 skenario pengujian komprehensif (`AT-HRD-TRN-01` s.d. `AT-HRD-TRN-22`) untuk memastikan seluruh fungsi bisnis, gerbang validasi, kepatuhan akreditasi rumah sakit, dan integrasi lintas sistem berjalan tanpa cacat.

| ID UAT | Kategori | Skenario Pengujian | Kondisi Awal (Pre-condition) | Langkah Tindakan (Test Steps) | Hasil yang Diharapkan (Expected Result) | Status |
| :--- | :--- | :--- | :--- | :--- | :--- | :---: |
| `AT-HRD-TRN-01` | Akreditasi | Konfigurasi 5 master aturan pelatihan wajib STARKES KPS 8 | User login sebagai `Diklat_Manager` | Entri aturan baru untuk BHD, PPI, SKP, K3RS/Code Red, dan Komunikasi SBAR | Kelima aturan pelatihan tersimpan aktif dengan target lingkup 100% pegawai RS (`VR-TRN-01`). | `Rencana` |
| `AT-HRD-TRN-02` | Siklus | Penolakan konfigurasi masa berlaku akreditasi > 24 bulan | Form pendaftaran aturan diklat terbuka | Input masa berlaku 36 bulan untuk aturan pelatihan wajib akreditasi | Sistem menolak dengan kode HTTP `400 Bad Request` menegakkan batas maksimal 24 bulan (`VR-TRN-02`). | `Rencana` |
| `AT-HRD-TRN-03` | Radar | Deteksi otomatis sertifikat masuk radar kedaluwarsa H-90/60/30 | Terdapat pegawai dengan sertifikat expired dalam 60 hari | Worker `MandatoryTrainingExpiryRadarWorker` dijalankan | Status sertifikat berubah ke `ExpiringSoon`, log radar kedaluwarsa tercatat pada sistem. | `Rencana` |
| `AT-HRD-TRN-04` | Notifikasi | Penyiaran pesan peringatan perpanjangan massal ke pegawai | Radar mendeteksi 15 staf berisiko kedaluwarsa | Admin Diklat menekan tombol *Broadcast Expiry Alerts* | Notifikasi multi-kanal (ESS & WhatsApp) terkirim ke 15 staf dan kepala ruangannya. | `Rencana` |
| `AT-HRD-TRN-05` | Masa Tenggang| Pemberian grace period 30 hari pasca habis tempo sertifikat | Sertifikat nakes melewati tanggal kedaluwarsa | Sistem mengevaluasi status sertifikat pada H+5 hari | Status menjadi `Expired` namun grace period aktif 25 hari tersisa sebelum pemblokiran penuh (`VR-TRN-10`). | `Rencana` |
| `AT-HRD-TRN-06` | Penjadwalan | Penugasan otomatis *buddy system* jaga shift ke Sub-06 | Sertifikat BHD perawat berstatus `Expired` | Kepala Ruangan menyusun jadwal dinas jaga di Sub-06 | Sub-06 menolak jadwal mandiri dan mewajibkan perawat didampingi perawat senior kompeten (`VR-TRN-11`). | `Rencana` |
| `AT-HRD-TRN-07` | Kredensial | Pembekuan kewenangan klinis RKK Level 4 ke Level 3 di Sub-14 | Sertifikat ACLS dokter spesialis anestesi kedaluwarsa | Sistem mengirimkan event `TrainingCertificateStatusChangedEvent` | Sub-14 membekukan hak mandiri tindakan darurat menjadi kewenangan supervisi (`INT-HR-77`). | `Rencana` |
| `AT-HRD-TRN-08` | Perencanaan | Penyusunan dan pengesahan rencana kerja tahunan diklat (TNA) | Staf Diklat login ke portal TNA | Input program tahunan diklat 2026 dan submit ke Direktur SDM | Direktur menyetujui, status program menjadi `Approved` dan pagu anggaran tahunan resmi dibuka. | `Rencana` |
| `AT-HRD-TRN-09` | Penyelenggaraan| Penjadwalan batch kelas pelatihan internal dan workshop lab | Rencana tahunan berstatus aktif | Buat sesi workshop BHD (kuota 30 peserta, instruktur klinis, aula RS) | Sesi tersimpan dengan nomor `SES-BHD-2026-B1` dan status pendaftaran dibuka (`RegistrationOpen`). | `Rencana` |
| `AT-HRD-TRN-10` | Pendaftaran | Pendaftaran diklat via ESS dengan persetujuan berjenjang | Jadwal sesi BHD terbuka | Perawat mengajukan pendaftaran mandiri via portal ESS | Pendaftaran berlanjut ke `PendingSupervisor`, disetujui Karu, divalidasi Diklat, menjadi `Approved`. | `Rencana` |
| `AT-HRD-TRN-11` | Anggaran | Pemblokiran pengajuan diklat jika pagu anggaran unit defisit | Sisa pagu diklat departemen = Rp 5.000.000 | Mengajukan pendaftaran workshop spesialisasi senilai Rp 8.000.000 | Sistem menolak pendaftaran dengan error `422 Unprocessable Entity` saldo pagu defisit (`VR-TRN-14`). | `Rencana` |
| `AT-HRD-TRN-12` | Presensi | Validasi kehadiran presensi minimal 100% untuk staf klinis | Sesi workshop klinis 3 hari pelajaran | Peserta klinis hanya hadir 2 hari (66.7% jam pelajaran) | Sistem melarang peserta masuk ke tahap ujian OSCE karena absensi < 100% (`VR-TRN-03`). | `Rencana` |
| `AT-HRD-TRN-13` | Evaluasi | Penilaian ujian teori post-test memenuhi passing grade >= 75 | Peserta menyelesaikan ujian teori online | Sistem merekam skor nilai post-test = 85.00 | Peserta dinyatakan lulus ujian teori (`post_test_passed: true`), siap lanjut ke OSCE (`VR-TRN-04`). | `Rencana` |
| `AT-HRD-TRN-14` | Ujian Klinis | Pengesahan rubrik OSCE dengan tanda tangan digital assessor | Peserta menjalani simulasi resusitasi | Dokter assessor mengisi checklist rubrik dan menandatangani digital | Nilai OSCE = 90.00 tercatat sah beserta hash digital tanda tangan penguji (`VR-TRN-06`). | `Rencana` |
| `AT-HRD-TRN-15` | Remedial | Pelaksanaan remedial maksimal 2 kali untuk peserta gagal | Peserta memperoleh nilai teori 65 (gagal) | Peserta mengikuti ujian remedial ke-1 dan mendapat nilai 80 | Status hasil akhir berubah menjadi `PASSED`, batas percobaan tercatat 1 dari maksimal 2 kali (`VR-TRN-20`). | `Rencana` |
| `AT-HRD-TRN-16` | Sertifikat | Penerbitan otomatis e-Sertifikat dan arsip ke e-Dossier Sub-02 | Peserta lulus teori dan ujian OSCE klinis | Sistem memproses kelulusan akhir sesi diklat | e-Sertifikat terbit resmi dengan nomor seri baku dan berkas PDF tersimpan di e-Dossier pegawai. | `Rencana` |
| `AT-HRD-TRN-17` | Ikatan Dinas| Deteksi otomatis diklat biaya > Rp 10 Juta memicu ikatan dinas | Pengajuan kursus bedah laparoskopi Rp 15 Juta | Tim Diklat memvalidasi berkas pendaftaran | Sistem otomatis menginisiasi draf akta ikatan dinas retensi 24 bulan (`VR-TRN-16`). | `Rencana` |
| `AT-HRD-TRN-18` | Kontrak Kerja| Penandatanganan akta ikatan dinas dan aktivasi masa retensi | Draf akta ikatan dinas telah siap | Pegawai dan Direktur menandatangani akta perjanjian | Status ikatan dinas menjadi `Active`, masa retensi 24 bulan berjalan pasca diklat tuntas. | `Rencana` |
| `AT-HRD-TRN-19` | Penalti | Penagihan ganti rugi saat pegawai ikatan dinas mengundurkan diri| Pegawai ikatan dinas baru menjalani 6 dari 24 bulan | Pegawai mengajukan permohonan resign di portal HRD | Sistem menandai `BreachPenalized` dan menerbitkan tagihan denda prorata 18 bulan sisa (`VR-TRN-17`). | `Rencana` |
| `AT-HRD-TRN-20` | Portofolio | Visual tracker akumulasi 50 SKP nakes selama 5 tahun berjalan | Nakes memiliki 10 riwayat kegiatan diklat | Buka halaman Portofolio SKP nakes di ESS | Tampil diagram radial perolehan 38/50 SKP terbagi pilar Pembelajaran, Pelayanan, dan Pengabdian. | `Rencana` |
| `AT-HRD-TRN-21` | Kemenkes | Verifikasi ID sertifikat ke API Plataran Sehat Kemenkes | Staf mengentri nomor sertifikat Plataran Sehat | Klik tombol verifikasi keabsahan Kemenkes | Sistem memanggil gateway API Kemenkes, memvalidasi NIK nakes, dan menandai `VERIFIED_PLATARAN_SEHAT`. | `Rencana` |
| `AT-HRD-TRN-22` | Kinerja Klinis| Agregasi jam diklat dan SKP ke pilar evaluasi OPPE Sub-15 | Siklus evaluasi semesteran OPPE dibuka | Sub-15 meminta rekap bukti CPD dokter (`INT-HR-78`) | Data jam pelatihan dan skor OSCE terpetakan otomatis ke pilar *Medical Knowledge* rapor kinerja dokter. | `Rencana` |

## 29. Matriks Pengujian Penerimaan Sub-Modul 17 (Slice S-C3 Performance Management & Clinical Career, AT-HRD-PRF-01 s.d. AT-HRD-PRF-22)

Matriks pengujian penerimaan (*User Acceptance Testing* - UAT) ini memuat 22 skenario pengujian komprehensif (`AT-HRD-PRF-01` s.d. `AT-HRD-PRF-22`) untuk memastikan seluruh aturan bisnis kinerja rumah sakit, komposisi 60:40, kalibrasi kurva sebaran, program PIP 60 hari, clinical ladder nakes, dan sanggah banding beroperasi tanpa cacat.

| ID UAT | Kategori | Skenario Pengujian | Kondisi Awal (Pre-condition) | Langkah Tindakan (Test Steps) | Hasil yang Diharapkan (Expected Result) | Status |
| :--- | :--- | :--- | :--- | :--- | :--- | :---: |
| `AT-HRD-PRF-01` | Siklus | Pembukaan siklus kinerja tahunan dan pencegahan tumpang tindih | Login sebagai `HR_Performance_Manager` | Buat siklus kinerja baru untuk tahun 2026 saat siklus 2025 masih aktif | Sistem menolak dengan kode HTTP `409 Conflict` hingga siklus lama disahkan penutupannya (`VR-PRF-01`). | `Rencana` |
| `AT-HRD-PRF-02` | Target | Cascading sasaran target unit dan validasi bobot tepat 100% | Karu login ke konsol cascading | Entri 5 butir target sasaran kerja individu perawat dengan total bobot 90% | Sistem menolak penyimpanan target dengan error `400 Bad Request` menegakkan total bobot tepat 100% (`VR-PRF-02`). | `Rencana` |
| `AT-HRD-PRF-03` | Evaluasi | Penyerahan evaluasi mandiri (Self-Assessment) via portal ESS | Pegawai login ke menu ESS | Isi skor refleksi mandiri, ulasan kerja, dan unggah tautan bukti capaian | Lembar kerja tersimpan dengan status `SelfSubmitted`, terkunci dari perubahan mandiri lanjutan. | `Rencana` |
| `AT-HRD-PRF-04` | Penilaian | Penilaian komposit Karu menerapkan bobot 60% KPI + 40% Perilaku | Karu membuka lembar evaluasi bawahan | Input nilai capaian KPI (85) dan nilai rubrik Core Values BerAKHLAK (90) | Skor komposit terhitung tepat: `(85 * 0.6) + (90 * 0.4) = 87.00` berpredikat "Baik" (`VR-PRF-03`). | `Rencana` |
| `AT-HRD-PRF-05` | Alur | Otorisasi berkas evaluasi oleh atasan kedua (Kabid) | Status berkas `ManagerAssessed` | Kabid memeriksa konsistensi skor dan menyetujui lembar evaluasi | Status berkas berubah menjadi `SecondLineReviewed`, siap masuk antrean sidang kalibrasi (`VR-PRF-04`). | `Rencana` |
| `AT-HRD-PRF-06` | Kalibrasi | Tinjauan matriks sebaran kurva kalibrasi terhadap pedoman RS | Sidang kalibrasi dibuka komite | Buka konsol visualisasi kurva sebaran nilai komposit RS | Menampilkan grafik lonceng aktual vs pedoman guiding distribution (10-15% A, 70-80% B, 5-10% C/D). | `Rencana` |
| `AT-HRD-PRF-07` | Override | Penyesuaian skor kinerja pegawai wajib mencatat nota justifikasi | Komite menemukan bias kelonggaran nilai Karu | Ubah nilai staf dari 95 ke 88 tanpa mengisi kolom alasan pertimbangan | Sistem menolak eksekusi simpan dengan error `400 Bad Request` mewajibkan nota tertulis (`VR-PRF-05`). | `Rencana` |
| `AT-HRD-PRF-08` | Rilis Nilai | Pengesahan pleno kalibrasi dan rilis nilai massal ke portal ESS | Sidang pleno selesai diisi notulen | Direktur SDM mengesahkan penutupan sidang kalibrasi | Status lembar kerja menjadi `FinalApproved`, skor resmi rilis ke ESS pegawai, dan hitung mundur 7 hari aktif. | `Rencana` |
| `AT-HRD-PRF-09` | Pemicu PIP | Otomasi penerbitan kontrak pembinaan PIP untuk skor akhir < 65 | Skor akhir terkalibrasi staf = 62.00 | Sistem memproses rilis nilai akhir siklus | Draf kontrak PIP 60 hari kalender terbit otomatis dengan penunjukan mentor pendamping (`VR-PRF-08`). | `Rencana` |
| `AT-HRD-PRF-10` | Hak Karir | Pembekuan sementara pengajuan mutasi dan promosi selama PIP | Staf sedang menjalani kontrak PIP aktif | Staf mencoba mengajukan permohonan mutasi antar unit di ESS | Sistem menolak dengan error `422 Unprocessable Entity` hak mutasi dibekukan selama masa PIP (`VR-PRF-11`). | `Rencana` |
| `AT-HRD-PRF-11` | Monitoring | Pengisian logbook mingguan monitoring PIP oleh mentor | Masuk minggu ke-3 masa pembinaan PIP | Mentor mengentri progres perbaikan mingguan dan centang status on-track | Logbook minggu ke-3 tersimpan sah dan indikator ketercapaian target mingguan terpantau (`VR-PRF-09`). | `Rencana` |
| `AT-HRD-PRF-12` | Evaluasi PIP | Sidang evaluasi hari ke-60 PIP menetapkan putusan Lulus | 8 Logbook mingguan terpenuhi on-track | Komite menggelar sidang evaluasi hari ke-60 PIP | Putusan `PASSED_NORMAL` disahkan, status PIP menjadi `Completed`, profil pegawai pulih normal. | `Rencana` |
| `AT-HRD-PRF-13` | Perpanjangan | Sidang evaluasi PIP memberikan perpanjangan terbatas 30 hari | Progres perbaikan ada namun belum optimal | Komite menetapkan putusan perpanjangan masa pembinaan | Status kontrak menjadi `Extended`, masa pembinaan bertambah 30 hari kalender (`VR-PRF-10`). | `Rencana` |
| `AT-HRD-PRF-14` | Gagal PIP | Sidang evaluasi PIP menetapkan kegagalan dan eskalasi HR Legal | Staf tidak menunjukkan perubahan performa | Sidang evaluasi hari ke-60 menetapkan putusan gagal | Status PIP menjadi `Failed`, berkas otomatis dieskalasi ke HR Legal untuk tindak lanjut disiplin/hubungan kerja. | `Rencana` |
| `AT-HRD-PRF-15` | Gate Jenjang | Validasi syarat skor kinerja minimal 75 untuk jenjang klinis | Perawat memiliki skor kinerja tahunan 72 | Perawat mengajukan kenaikan jenjang PK I ke PK II di ESS | Sistem menolak pengajuan dengan error `422 Unprocessable Entity` skor KPI belum memenuhi ambang 75 (`VR-PRF-14`). | `Rencana` |
| `AT-HRD-PRF-16` | Kredensial | Penolakan jenjang klinis jika sertifikat kompetensi mati | Perawat skor KPI 88 namun sertifikat BHD expired | Perawat mengajukan kenaikan jenjang klinis di ESS | Sistem menolak pengajuan karena sertifikat kompetensi prasyarat di Sub-16 tidak aktif (`VR-PRF-16`). | `Rencana` |
| `AT-HRD-PRF-17` | Sentinel | Penolakan jenjang klinis jika ada insiden sentinel aktif | Nakes memiliki catatan insiden mutu di Sub-15 | Nakes mengajukan kenaikan jenjang klinis di ESS | Sistem menolak pengajuan via query `INT-HR-84` karena catatan insiden keselamatan pasien aktif (`VR-PRF-17`). | `Rencana` |
| `AT-HRD-PRF-18` | Promosi Klinis| Pengesahan SK kenaikan jenjang nakes memutakhirkan Sub-02/14 | Komite merekomendasikan kenaikan ke PK III | Direktur Utama mengesahkan SK kenaikan jenjang klinis | Profil pegawai di Sub-02 terupdate ke PK III dan paket tindakan mandiri di Sub-14 terbuka (`INT-HR-83`). | `Rencana` |
| `AT-HRD-PRF-19` | Banding Nilai | Pengajuan sanggah banding nilai kinerja dalam batas H+7 | Nilai dirilis pada hari ke-1 | Pegawai mengajukan sanggahan di ESS pada hari ke-5 dengan melampirkan bukti | Berkas sanggahan tersimpan dengan status `DisputeSubmitted`, SLA mediasi 7 hari diaktifkan (`VR-PRF-12`). | `Rencana` |
| `AT-HRD-PRF-20` | Kedaluwarsa | Penolakan otomatis pengajuan sanggah banding setelah H+7 | Nilai dirilis 10 hari yang lalu | Pegawai mencoba mengklik tombol sanggah banding di ESS | Sistem menonaktifkan form dan menolak pengajuan dengan error `400 Bad Request` batas waktu habis (`VR-PRF-12`). | `Rencana` |
| `AT-HRD-PRF-21` | Mediasi HR | Klarifikasi tertutup HRD menyepakati penyesuaian skor damai | Sengketa nilai terdaftar | HR Performance memfasilitasi pertemuan Karu dan staf | Karu mengakui kekhilafan data dan menyetujui koreksi nilai; berita acara disahkan menjadi `Resolved`. | `Rencana` |
| `AT-HRD-PRF-22` | Majelis Banding| Sidang majelis banding Direktur SDM memutus perkara dalam 14 hari| Mediasi klarifikasi tidak mencapai titik temu | Berkas dieskalasi ke Komite Majelis Banding | Majelis menetapkan putusan final mengikat, skor akhir dimutakhirkan, dan sengketa ditutup resmi (`VR-PRF-13`). | `Rencana` |


---

## 30. Matriks Pengujian Penerimaan Sub-Modul 18: Hubungan Industrial & Terminasi (Slice `S-C5`)

### 30.1 Butir Uji Kriteria Penerimaan (`AT-HRD-EMP-01` s.d. `AT-HRD-EMP-22`)

| ID Pengujian | Deskripsi Skenario Uji | Kriteria Keberhasilan (Acceptance Criteria) | Tipe Pengujian |
|---|---|---|---|
| `AT-HRD-EMP-01` | Pelaporan insiden tata tertib pegawai | Form insiden terekam dengan status `Reported` dan nomor unik `INC-...` | Integration |
| `AT-HRD-EMP-02` | Validasi Just Culture kasus insiden klinis medis | Sistem menolak laporan insiden klinis tanpa nomor audit KMKP sah (`VR-EMP-02`) | Negative |
| `AT-HRD-EMP-03` | Pemblokiran sanksi SP untuk kategori Human Error | Sistem menolak penerbitan sanksi SP jika insiden terbukti Human Error (`VR-EMP-03`) | Security / Rule |
| `AT-HRD-EMP-04` | Pelaksanaan sidang Berita Acara Pemeriksaan (BAP) | BAP tersimpan lengkap dengan daftar pemeriksa, pembelaan staf, dan temuan | Functional |
| `AT-HRD-EMP-05` | Penerbitan SP 1 dengan masa berlaku 6 bulan | `ValidUntil` otomatis tercatat tepat `IssuedDate + 6 Months` kalender | Unit / Business |
| `AT-HRD-EMP-06` | Kedaluwarsa otomatis sanksi SP pada H+180 hari | Worker `EMP-W1` mengubah status sanksi aktif menjadi `Expired` pada H+180 | Worker / Async |
| `AT-HRD-EMP-07` | Peringatan H-14 kedaluwarsa sanksi SP | Notifikasi peringatan terkirim ke staf dan atasan 14 hari sebelum sanksi gugur | Async Event |
| `AT-HRD-EMP-08` | Kenaikan berjenjang sanksi SP 1 ke SP 2 | Sistem mengizinkan penerbitan SP 2 karena pegawai masih memiliki SP 1 aktif | Business Rule |
| `AT-HRD-EMP-09` | Penerbitan langsung SP 3 untuk pelanggaran berat | SP 3 langsung terbit sukses setelah persetujuan Direktur SDM terverifikasi | Functional |
| `AT-HRD-EMP-10` | Alur sanggah banding atas sanksi SP | Tiket banding tercatat dan sanksi berubah status `RevokedOnAppeal` jika disetujui | Workflow |
| `AT-HRD-EMP-11` | Penetapan status skorsing investigasi (*paid suspension*) | Skorsing aktif tercatat, hak upah bernilai `PaidFull`, tanggal batas maks 6 bulan | Regulatory |
| `AT-HRD-EMP-12` | Pencabutan instan akses SSO SIMRS saat skorsing | Event `INT-HR-88` terbit dan sesi token Keycloak SIMRS/EMR terhapus seketika | Security Event |
| `AT-HRD-EMP-13` | Pengiriman tiket keluhan hubungan kerja via ESS | Tiket keluhan terkirim ke supervisor dengan SLA target 14 hari kerja | Functional |
| `AT-HRD-EMP-14` | Pengiriman laporan whistleblower anonim terenkripsi | Tiket whistleblower tersimpan dalam payload terenkripsi tanpa identitas terbuka | Cryptography / Privacy |
| `AT-HRD-EMP-15` | Isolasi akses baca tiket whistleblower dari atasan | Atasan langsung menerima HTTP 403 Forbidden saat mencoba membaca tiket | Security / Auth |
| `AT-HRD-EMP-16` | Dokumentasi perundingan forum Bipartit RS | Agenda perundingan, notulen risalah, dan naskah PB PDF tersimpan lengkap | Compliance |
| `AT-HRD-EMP-17` | Pengajuan terminasi resign sukarela 30 hari | Sistem memvalidasi tanggal notice minimal 30 hari kalender sebelum tanggal efektif | Validation |
| `AT-HRD-EMP-18` | Kalkulator pesangon resign (UPH + Uang Pisah) | UP dan UPMK bernilai Rp 0; UPH cuti dan Uang Pisah terhitung akurat PP 35/2021 | Calculation Engine |
| `AT-HRD-EMP-19` | Kalkulator pesangon pensiun (1.75x UP + 1x UPMK + UPH) | Formula pengali 1.75x UP dan 1x UPMK teraplikasi akurat sesuai masa kerja sah | Calculation Engine |
| `AT-HRD-EMP-20` | Persetujuan lembar pesangon oleh Direktur Keuangan | Nilai pesangon terkunci dan status perhitungan berubah menjadi `Approved` | Approval Gate |
| `AT-HRD-EMP-21` | Gerbang exit clearance 5 departemen | Checklist dari Unit, Aset, Farmasi, Keuangan, dan IT terverifikasi bertahap | Multi-Party Signoff |
| `AT-HRD-EMP-22` | Hard-block pemblokir rilis gaji akhir di Sub-10 | Sub-10 menolak proses payroll final jika clearance masih pending (`INT-HR-87`) | Inter-Module Gate |

---

### 30.2 Lima Skenario Pengujian Penerimaan Pengguna (User Acceptance Testing - UAT)

1. **UAT-EMP-01: Penerbitan Sanksi Disiplin Berjenjang Sesuai PP 35/2021 & Kedaluwarsa 6 Bulan**
   - *Aktor:* Kepala Ruangan, Staf Medis Terlapor, Manajer HR.
   - *Langkah:* Pelaporan insiden ketidakhadiran berulang -> Pemeriksaan BAP klarifikasi -> Penerbitan SP 1 bertanggal kedaluwarsa 6 bulan -> Simulasi berjalannya waktu 180 hari -> Worker `EMP-W1` mengeset status `Expired` dan mengirim notifikasi pembersihan sanksi.
   - *Ekspektasi:* SP 1 aktif selama tepat 6 bulan kalender, tercatat di e-Dossier Sub-02, dan otomatis kedaluwarsa tanpa intervensi manual jika tidak ada pelanggaran baru.

2. **UAT-EMP-02: Penanganan Insiden Pelayanan Medis Pasien dengan Filter Prinsip Just Culture**
   - *Aktor:* Perawat Pelapor, Tim KMKP Sub-15, Komite Etik Disiplin RS, Direktur SDM.
   - *Langkah:* Laporan insiden kesalahan dosis obat di bangsal rawat inap -> Validasi nomor audit KMKP -> Audit KMKP membuktikan penyebab utama adalah kelelahan akibat shift darurat dan pelabelan obat mirip (*Human Error*) -> Sistem memblokir penerbitan SP dan mengalihkan perawat ke pelatihan resertifikasi PPI/BHD Sub-16.
   - *Ekspektasi:* Tenaga medis terlindungi dari kriminalisasi kesalahan manusiawi murni, budaya keselamatan pasien terjaga, dan data terdokumentasi rapi.

3. **UAT-EMP-03: Pelaporan Kecurangan Farmasi Melalui Kanal Whistleblowing Terenkripsi & Anti-Retaliasi**
   - *Aktor:* Staf Farmasi Pelapor, Tim Kepatuhan Internal, Direktur SDM.
   - *Langkah:* Staf farmasi menemukan gratifikasi pengadaan obat -> Mengirimkan tiket via portal whistleblower dengan mode `Anonim` -> Data dienkripsi asimetris -> Tiket hanya muncul di dasbor Tim Kepatuhan -> Kepala Instalasi Farmasi tidak memiliki hak akses membuka tiket.
   - *Ekspektasi:* Identitas pelapor 100% aman terlindungi dari kebocoran internal dan intimidasi atasan langsung.

4. **UAT-EMP-04: Penetapan Skorsing Investigasi Pelanggaran Berat dengan Upah Penuh & Pencabutan Akses SIMRS**
   - *Aktor:* Pegawai Terperiksa, Direktur Utama, Administrator IT SIMRS.
   - *Langkah:* Dugaan manipulasi data rekam medis pasien -> Direktur Utama menerbitkan surat skorsing investigasi -> Sistem seketika menerbitkan sinyal `INT-HR-88` -> Sesi login SIMRS/EMR terputus dan kartu pintar akses ruang server/rekam medis nonaktif -> Hak gaji pokok dan tunjangan tetap di Sub-10 Payroll tetap terproses 100%.
   - *Ekspektasi:* Rumah sakit terlindungi dari manipulasi data lanjutan, sementara hak normatif upah pekerja tetap dipenuhi sesuai UU Ketenagakerjaan Pasal 157A.

5. **UAT-EMP-05: Pengakhiran Kerja Resign Sukarela, Kalkulasi Pesangon PP 35/2021 & Exit Clearance Hard-Block**
   - *Aktor:* Pegawai Resign, Staf Aset Umum, Farmasi, Keuangan, IT, Petugas Payroll Sub-10.
   - *Langkah:* Pegawai mengajukan resign 30 hari sebelumnya -> Sistem menghitung hak UPH cuti dan Uang Pisah -> Direktur Keuangan mengesahkan lembar kompensasi -> Pegawai mengembalikan seragam dinas, stetoskop RS, kartu akses, dan melunasi kasbon -> Bagian IT mencabut akses SSO SIMRS -> Seluruh 5 unit memberi persetujuan `Approved` -> Petugas Payroll Sub-10 dapat membuka gembok dan merilis transfer gaji terakhir serta uang pisah.
   - *Ekspektasi:* Zero aset hilang, akun sistem tertutup rapi, dan tidak ada sengketa pembayaran gaji akhir.


---

## 31. Sub-Modul 19: Occupational Health & Staff Safety (K3RS & Medical Check-Up) — Slice `S-C6`

### 31.1 Matriks Kriteria Pengujian Penerimaan Otomatis (Acceptance Test Criteria)

| Kode Pengujian | Target Komponen / Logika Bisnis | Kondisi Uji & Prasyarat | Hasil yang Diharapkan (Expected Result) | Tingkat Prioritas |
|---|---|---|---|---|
| `AT-HRD-OHS-01` | Enkripsi Rekam Medis Klinis K3RS | Simpan rekam medis staf dengan diagnosis klinis dan catatan dokter | Data tersimpan terenkripsi AES-256 di basis data; tidak terbaca plain-text | `CRITICAL` |
| `AT-HRD-OHS-02` | Pemisahan Hak Akses HR vs Dokter (HRD-DEC-176) | Request GET rincian rekam medis dilakukan memakai token akun HR Admin | Respon hanya memuat status kelayakan kerja; akses data klinis menghasilkan HTTP 403 | `CRITICAL` |
| `AT-HRD-OHS-03` | Akses Mandiri Rekam Medis Pegawai via ESS | Pegawai login ke portal ESS dan membuka riwayat MCU miliknya | Pegawai dapat mengunduh salinan rekam kesehatan pribadinya secara aman | `HIGH` |
| `AT-HRD-OHS-04` | Validasi Sertifikat Hiperkes Dokter K3RS | Dokter tanpa nomor sertifikat Hiperkes mencoba mengesahkan kelaikan kerja | Sistem menolak pengesahan dengan pesan error validasi `VR-OHS-03` | `HIGH` |
| `AT-HRD-OHS-05` | Penetapan Status 'Fit With Restriction' | Dokter menetapkan status kelaikan dengan restriksi dinas malam | Surat pembatasan tugas diterbitkan dengan tanggal kedaluwarsa maksimal 6 bulan | `HIGH` |
| `AT-HRD-OHS-06` | Roster Hard-Stop Penjadwalan Sub-06 (`INT-HR-89`) | Atasan mencoba menjadwalkan shift malam pada staf yang memiliki restriksi aktif | Sistem roster melempar eksepsi error `VR-OHS-06` dan membatalkan simpan shift | `CRITICAL` |
| `AT-HRD-OHS-07` | Pelepasan Restriksi via Asesmen Return-to-Work | Dokter K3RS menerbitkan asesmen RTW `Fit` setelah masa pemulihan selesai | Status restriksi beralih ke `CompletedCleared` dan kunci roster Sub-06 terbuka | `HIGH` |
| `AT-HRD-OHS-08` | Tombol Darurat Tertusuk Jarum ESS (< 5 Detik) | Pegawai menekan tombol panik tertusuk jarum pada portal ESS mobile | Tiket darurat terbit seketika dan sinyal notifikasi cito sampai ke ponsel K3RS/PPI | `CRITICAL` |
| `AT-HRD-OHS-09` | Batas Emas Terapi PPP / PEP < 4 Jam | Staf terpajan darah jarum risiko tinggi menjalani triase IGD/K3RS | Resep paket darurat PPP terbit dan tercatat dimulai < 4 jam dari waktu insiden | `CRITICAL` |
| `AT-HRD-OHS-10` | Otomasi Jadwal Surveilans Serial 1, 3, 6 Bulan | Insiden pajanan darah infeksius disahkan oleh dokter K3RS | Worker `OHS-W1` membentuk 3 jadwal surveilans darah otomatis di kalender staf | `HIGH` |
| `AT-HRD-OHS-11` | Penutupan Kasus Pajanan Non-Reaktif | Hasil uji serologi bulan ke-6 menunjukkan non-reaktif (negatif) | Tiket pajanan berstatus `ClosedSeronegative` dan staf dinyatakan aman | `MEDIUM` |
| `AT-HRD-OHS-12` | Penapisan Otomatis Paket MCU Berbasis Risiko | Staf radiologi didaftarkan ke jadwal pemeriksaan MCU berkala | Paket MCU otomatis menyertakan evaluasi radiasi TLD dan darah rutin perifer | `HIGH` |
| `AT-HRD-OHS-13` | Imutabilitas Hasil Kelaikan yang Telah Disahkan | Pengguna mencoba menyunting status kelayakan kerja yang sudah ditandatangani | Sistem menolak perubahan data (HTTP 400 Bad Request / Immutable Record) | `HIGH` |
| `AT-HRD-OHS-14` | Kelengkapan Data Log Imunisasi Staf | Input vaksinasi staf tanpa menyertakan nomor batch dan pabrikan vaksin | Sistem menolak input dengan pesan error validasi `VR-OHS-14` | `MEDIUM` |
| `AT-HRD-OHS-15` | Deteksi Titer Anti-HBs Non-Protektif (< 10 mIU/mL) | Hasil lab staf mencatat titer Anti-HBs sebesar 3 mIU/mL | Status imunitas otomatis `NonProtective` dan tiket rujukan booster terbit | `HIGH` |
| `AT-HRD-OHS-16` | Notifikasi Pengingat Dini Booster Vaksin H-30 | Simulasi tanggal saat ini berada pada H-30 kedaluwarsa imunitas staf | Worker `OHS-W3` mengirim pesan pengingat booster ke akun portal ESS staf | `LOW` |
| `AT-HRD-OHS-17` | Ambang Batas Kumulatif Radiasi TLD (> 20 mSv/thn) | Rekam dosis radiasi tahunan staf melampaui batas aman 20 mSv | Sistem menyalakan alarm peringatan bahaya radiasi dan rekomendasi rotasi kerja | `CRITICAL` |
| `AT-HRD-OHS-18` | Ekspor Dokumen Klaim JKK Tahap I BPJS (< 2x24 Jam) | Data kecelakaan kerja diekspor untuk pelaporan BPJS Ketenagakerjaan | Berkas elektronik Formulir JKK Tahap I siap kirim lengkap dengan kronologi TKP | `HIGH` |
| `AT-HRD-OHS-19` | Investigasi Kecelakaan Kerja & Matriks CAPA | Tim K3RS menyelesaikan formulir investigasi kecelakaan kerja | Berkas investigasi memuat minimal 2 nama investigator dan tabel rekomendasi CAPA | `MEDIUM` |
| `AT-HRD-OHS-20` | Sinkronisasi Hari Kerja Hilang (LTI) ke Sub-07 | Staf izin sakit akibat kecelakaan kerja selama 7 hari kalender | Kolom `lost_time_injury_days` otomatis terisi 7 hari dan sinkron dengan Sub-07 | `MEDIUM` |
| `AT-HRD-OHS-21` | Finalisasi Klaim JKK Tahap II dan Penggantian Biaya | Staf K3RS mengunggah resume medis dan kwitansi perawatan IGD RS | Dokumen klaim santunan Tahap II tervalidasi lengkap dan siap klaim | `HIGH` |
| `AT-HRD-OHS-22` | Ekspor Laporan Kepatuhan Akreditasi STARKES KPS | Manajemen mengunduh laporan pemenuhan kesehatan staf tahunan | Terbit berkas ringkasan STARKES KPS berisi persentase MCU dan cakupan vaksinasi | `HIGH` |

---

### 31.2 Skenario Pengujian Penerimaan Pengguna (User Acceptance Testing Scenarios)

1. **UAT-OHS-01: Penanganan Tanggap Darurat Tertusuk Jarum Cito, Pemberian PPP < 4 Jam & Pemantauan 6 Bulan**
   - *Aktor:* Perawat Bangsal Bedah (Korban), Dokter Jaga IGD, Dokter Spesialis Okupasi K3RS, Analis Lab.
   - *Langkah:* Perawat tertusuk jarum saat menjahit luka pasien terkonfirmasi Hepatitis B -> Perawat menekan tombol darurat di ESS -> Sirene berbunyi di K3RS & PPI -> Triase IGD dalam 30 menit -> Dokter K3RS meresepkan Hepatitis B Immunoglobulin & Starter Pack ARV yang diserahkan dalam waktu 2,5 jam sejak insiden -> Pengambilan darah baseline staf dan pasien -> Worker `OHS-W1` mengeset pemantauan serial bulan 1, 3, dan 6 -> Uji lab bulan ke-6 non-reaktif -> Tiket resmi ditutup aman.
   - *Ekspektasi:* Respon cito terpenuhi dalam batas emas keselamatan < 4 jam, obat profilaksis tersedia instan, dan staf terpantau secara terstruktur hingga bebas risiko penularan.

2. **UAT-OHS-02: Pelaksanaan MCU Berkala Berbasis Profil Ruangan, Pembatasan Tugas Medis & Hard-Stop Roster**
   - *Aktor:* Staf Ruang Perawatan Paru, Dokter K3RS Bersertifikat, Kepala Ruangan, Staf HR Admin.
   - *Langkah:* Staf menjalani MCU berkala tahunan -> Rontgen toraks dan spirometri terunggah aman -> Dokter K3RS mendiagnosis bronkitis akut dengan restriksi tidak boleh kontak dengan pasien infeksius menular dan tidak berdinas malam selama 30 hari -> Terbit surat restriksi `FitWithRestriction` -> Kepala Ruangan menyusun jadwal dinas malam di Sub-06 -> Sistem menolak penjadwalan (*Hard-Stop*) -> HR Admin hanya melihat status kelaikan operasional tanpa tahu rincian catatan klinis internal.
   - *Ekspektasi:* Rahasia medis staf terlindungi 100% dari HR/atasan, sementara keselamatan staf terjamin secara fisik lewat gembok penjadwalan otomatis.

3. **UAT-OHS-03: Radar Kepatuhan Imunisasi Nakes, Deteksi Titer Anti-HBs Rendah & Penjadwalan Booster Otomatis**
   - *Aktor:* Dokter Spesialis Bedah, Petugas Vaksinator K3RS, Komite Mutu K3RS.
   - *Langkah:* Hasil evaluasi lab tahunan mencatat titer Anti-HBs dokter bedah turun menjadi 4,5 mIU/mL (di bawah ambang protektif 10 mIU/mL) -> Dasbor radar imunitas mendeteksi status `NonProtective` -> Sistem menerbitkan tiket otomatis rujukan vaksinasi booster Hepatitis B -> Dokter bedah menerima notifikasi kalender vaksinasi di portal ESS -> Dokter disuntik vaksin booster dan nomor batch dicatat -> Status imunitas pulih protektif.
   - *Ekspektasi:* Seluruh tenaga medis berisiko tinggi memiliki proteksi imunologis yang terpantau aktif demi kepatuhan akreditasi STARKES dan keselamatan kerja.

4. **UAT-OHS-04: Penanganan Kecelakaan Kerja Tertimpa Alat Medis, Investigasi K3RS & Klaim JKK BPJS Tahap I-II**
   - *Aktor:* Petugas CSSD (Korban), Kepala Instalasi CSSD, Staf Klaim K3RS, Tim Keselamatan K3RS.
   - *Langkah:* Petugas CSSD mengalami cedera fraktur ringan kaki saat memindahkan autoklaf steril -> Laporan kecelakaan dibuat di sistem -> Tim K3RS olah TKP dan menyusun CAPA perbaikan lantai licin -> Staf Klaim mengekspor Formulir JKK Tahap I ke portal BPJS Ketenagakerjaan dalam 18 jam (patuh < 48 jam) -> Korban dirawat dan istirahat sakit 14 hari (LTI sinkron dengan Sub-07) -> Pengajuan Tahap II selesai dan seluruh biaya RS diganti penuh oleh BPJS.
   - *Ekspektasi:* Kepatuhan regulasi 2x24 jam terpenuhi, risiko kerja diperbaiki tuntas, dan santunan jaminan sosial tenaga kerja cair tanpa kendala.

5. **UAT-OHS-05: Asesmen Kelaikan Kembali Bekerja (Return-to-Work) & Pemulihan Kewenangan Klinis Sub-03**
   - *Aktor:* Perawat Terluka, Dokter K3RS Bersertifikat, Sub-Komite Kredensial Keperawatan Sub-03.
   - *Langkah:* Masa istirahat pemulihan cedera fraktur selesai -> Perawat mengajukan evaluasi kembali bertugas (RTW) -> Dokter K3RS melakukan uji kebugaran fisik, rentang gerak motorik, dan kekuatan beban -> Hasil evaluasi dinilai `FitForFullDuty` -> Asesmen disahkan -> Kunci gembok roster Sub-06 dilepas otomatis -> Sub-03 memulihkan status kewenangan klinis aktif perawat.
   - *Ekspektasi:* Transisi staf dari masa sakit kembali ke pelayanan pasien berjalan aman, terukur, dan sah secara hukum kedokteran okupasi.


---

## 32. Sub-Modul 21: Employee Self-Service (ESS) & Unified Approval Inbox (V2 Hardening) — Slice `S-A7-V2`

### 32.1 Matriks Kriteria Pengujian Penerimaan Otomatis (Acceptance Test Criteria)

| Kode Pengujian | Target Komponen / Logika Bisnis | Kondisi Uji & Prasyarat | Hasil yang Diharapkan (Expected Result) | Tingkat Prioritas |
|---|---|---|---|---|
| `AT-HRD-WF-01` | Inisiasi Workflow Tiket Baru dari ESS | Pegawai mengisi formulir permohonan cuti/lembur di portal ESS | `TrxWorkflowInstance` terbuat dengan nomor registrasi unik dan status `Submitted` | `HIGH` |
| `AT-HRD-WF-02` | Resolusi Dinamis Matriks Persetujuan Multi-Level | Permohonan diajukan oleh staf perawat bangsal | Sistem membentuk alur 2 tahap: Step 1 Kepala Ruangan, Step 2 Kepala Bidang Keperawatan | `HIGH` |
| `AT-HRD-WF-03` | Anti Self-Approval Pejabat Struktural (`VR-WF-02`) | Kepala Ruangan mengajukan permohonan cuti tahunan mandiri | Sistem memblokir approval mandiri dan otomatis mengalihkan tiket ke Kepala Bidang | `CRITICAL` |
| `AT-HRD-WF-04` | Penugasan Wewenang ke Pelaksana Harian (Plh) | Kepala Ruangan cuti dan menunjuk Wakil Karu sebagai Plh aktif | Tiket permohonan staf masuk ke inbox Plh dengan penanda badge "Delegated Request" | `HIGH` |
| `AT-HRD-WF-05` | Jejak Audit Ganda Keputusan Plh | Plh mengeksekusi persetujuan cuti atas nama Kepala Ruangan | `TrxApprovalAction` mencatat `action_employee_id = Plh` dan `on_behalf_of = Karu` | `CRITICAL` |
| `AT-HRD-WF-06` | Kedaluwarsa Otomatis Wewenang Delegasi Plh | Tanggal hari ini melewati `EndDate` surat kuasa Plh | Worker `WF-W2` mengeset `IsActive = false`; wewenang Plh dicabut seketika | `MEDIUM` |
| `AT-HRD-WF-07` | Penolakan Permohonan dengan Alasan Wajib | Approver menolak tiket tanpa menyertakan catatan alasan tertulis | Sistem melempar eksepsi error validasi `VR-WF-05` (alasan minimal 15 karakter) | `HIGH` |
| `AT-HRD-WF-08` | Pengembalian Berkas Revisi (Return for Revision) | Approver meminta perubahan tanggal dinas pada tiket tukar shift | Status tiket beralih ke `ReturnedForRevision` dan masuk ke tab revisi pemohon di ESS | `HIGH` |
| `AT-HRD-WF-09` | Pengajuan Ulang (Resubmit) Pasca Koreksi Data | Staf mengedit tanggal dinas dan mengirim ulang tiket | Tiket berstatus `Resubmitted` dan kembali masuk ke inbox approver terkait | `HIGH` |
| `AT-HRD-WF-10` | Pembatalan Mandiri Tiket oleh Pemohon | Staf membatalkan permohonan sebelum approver mengambil tindakan | Status tiket berubah menjadi `Cancelled` dan keluar dari antrean inbox approver | `MEDIUM` |
| `AT-HRD-WF-11` | Larangan Auto-Approve saat Timeout SLA (`VR-WF-11`) | Tiket permohonan melewati batas waktu SLA tanpa respon atasan | Sistem dilarang menyetujui tiket; status beralih ke `EscalatedDueToTimeout` | `CRITICAL` |
| `AT-HRD-WF-12` | Eskalasi Otomatis Tiket Macet via Worker `WF-W1` | Tiket berstatus overdue selama 6 jam sejak batas waktu SLA | Worker `WF-W1` menduplikasi penugasan tiket ke atasan tingkat di atasnya | `HIGH` |
| `AT-HRD-WF-13` | Peringatan Dini Batas Waktu SLA H-4 Jam | Tiket permohonan berada pada sisa waktu 4 jam sebelum SLA habis | Sistem mengirimkan notifikasi push prioritas tinggi ke ponsel approver | `MEDIUM` |
| `AT-HRD-WF-14` | Persetujuan Massal Permohonan Berisiko Rendah | Approver memilih 10 permohonan lembur rutin berindikator hijau | 10 tiket disetujui sekaligus dalam satu kali konfirmasi PIN keamanan | `HIGH` |
| `AT-HRD-WF-15` | Penolakan Persetujuan Massal untuk Tiket Warning | Approver mencoba memasukkan tiket tukar shift bentrok ke bulk approve | Sistem menolak item tersebut dari batch dan mewajibkan evaluasi lembar detail | `CRITICAL` |
| `AT-HRD-WF-16` | Verifikasi Keaslian PIN Otorisasi Approver | Approver memasukkan PIN keamanan yang salah saat menyetujui tiket | Sistem menolak eksekusi persetujuan dengan HTTP 403 Forbidden | `HIGH` |
| `AT-HRD-WF-17` | Eksekusi Idempoten Pemotongan Saldo Cuti Sub-08 | Tiket cuti disetujui final oleh approver tahapan terakhir | Sinyal `INT-HR-93` memotong saldo cuti di Sub-08 tepat 1 kali tanpa duplikasi | `CRITICAL` |
| `AT-HRD-WF-18` | Eksekusi Perubahan Jadwal Shift Sub-06 | Tiket tukar shift disetujui final oleh Kepala Ruangan | Roster dinas kedua perawat di Sub-06 otomatis bertukar posisi shift | `CRITICAL` |
| `AT-HRD-WF-19` | Pengiriman Email Ringkasan Harian Atasan | Simulasi jadwal waktu pukul 07:00 pagi WIB | Worker `WF-W3` mengirim email kompilasi daftar pending request ke atasan | `LOW` |
| `AT-HRD-WF-20` | Isolasi Catatan Internal Peninjauan Approver | Pemohon login ke ESS dan memeriksa riwayat permohonannya | Catatan bertanda `is_internal_review_only = true` tidak tampil di layar pemohon | `MEDIUM` |
| `AT-HRD-WF-21` | Validasi Ukuran & Format Berkas Lampiran | Pengguna mengunggah berkas format `.exe` atau berukuran 15 MB | Sistem menolak berkas dengan pesan validasi `VR-WF-13` dan `VR-WF-14` | `LOW` |
| `AT-HRD-WF-22` | Rekonsiliasi Otomatis Aksi Hilir yang Macet | Terjadi network glitch saat sinyal persetujuan dikirim ke Sub-08 | Worker `WF-W6` mengeksekusi ulang sinyal hingga pemotongan saldo terkonfirmasi | `HIGH` |

---

### 32.2 Skenario Pengujian Penerimaan Pengguna (User Acceptance Testing Scenarios)

1. **UAT-WF-01: Alur Pengajuan Cuti Staf, Peninjauan Atasan Langsung & Pemotongan Saldo Cuti Otomatis**
   - *Aktor:* Perawat Rawat Inap, Kepala Ruangan, Sistem Sub-08.
   - *Langkah:* Perawat mengajukan cuti tahunan 3 hari melalui portal ESS -> Tiket masuk ke Unified Approval Inbox Kepala Ruangan -> Indikator validasi menunjukkan hijau (saldo cukup 8 hari, tidak bentrok kuota shift) -> Kepala Ruangan menyetujui dengan PIN -> Sistem menerbitkan event `INT-HR-93` -> Saldo cuti berkurang menjadi 5 hari di Sub-08 dan jadwal shift di Sub-06 ditandai cuti resmi.
   - *Ekspektasi:* Transaksi persetujuan berjalan mulus dari pengajuan mandiri hingga pembaruan operasional tanpa jeda manual.

2. **UAT-WF-02: Mekanisme Anti Self-Approval Kepala Ruangan & Pengalihan Otorisasi ke Kepala Bidang**
   - *Aktor:* Kepala Ruangan Melati (Pemohon), Kepala Bidang Keperawatan (Approver).
   - *Langkah:* Kepala Ruangan mengajukan izin lembur supervisi akreditasi -> Sistem mengevaluasi aturan `VR-WF-02` dan mendeteksi pemohon adalah pejabat approver -> Sistem memblokir tombol approve diri sendiri dan secara otomatis mengarahkan tiket ke inbox Kepala Bidang Keperawatan -> Kepala Bidang menerima notifikasi dan menyetujui lembur Karu.
   - *Ekspektasi:* Zero self-approval, tata kelola wewenang rumah sakit terjaga 100% dari potensi manipulasi.

3. **UAT-WF-03: Pendelegasian Wewenang Selama Masa Cuti Atasan & Jejak Audit Pelaksana Harian (Plh)**
   - *Aktor:* Kepala Ruangan (Definitif), Wakil Kepala Ruangan (Plh), Perawat Pemohon.
   - *Langkah:* Karu mendaftarkan pendelegasian wewenang ke Wakil Karu selama 5 hari masa cuti -> Perawat mengajukan tukar shift jaga malam -> Tiket masuk ke inbox Wakil Karu bertanda "Delegated Request: On Behalf of Karu" -> Wakil Karu menyetujui -> Log audit permanen mencatat Wakil Karu sebagai eksekutor atas nama Kepala Ruangan definitif.
   - *Ekspektasi:* Tidak ada keterlambatan persetujuan shift saat kepala ruangan cuti, dan akuntabilitas audit tetap jelas.

4. **UAT-WF-04: Penanganan Permohonan Tukar Shift dengan Siklus Pengembalian Revisi (Return for Revision)**
   - *Aktor:* Perawat Pengaju, Perawat Pengganti, Kepala Ruangan.
   - *Langkah:* Perawat mengajukan tukar shift dinas malam ke dinas pagi -> Karu meninjau dan melihat perawat belum melampirkan persetujuan tertulis perawat pengganti -> Karu memilih aksi `Return for Revision` dengan catatan: "Lampirkan bukti konfirmasi perawat pengganti" -> Tiket kembali ke tab revisi ESS perawat -> Perawat mengunggah bukti dan menekan `Resubmit` -> Tiket kembali aktif di inbox Karu -> Karu menyetujui.
   - *Ekspektasi:* Kolaborasi perbaikan data berjalan efektif tanpa mengharuskan staf mengulang isi formulir dari awal.

5. **UAT-WF-05: Penegakan SLA Batas Waktu 24 Jam, Peringatan Dini & Eskalasi Otomatis Tanpa Auto-Approve**
   - *Aktor:* Perawat Pemohon, Kepala Ruangan (Approver 1), Kepala Bidang (Approver 2).
   - *Langkah:* Perawat mengajukan tukar shift mendesak (SLA 24 jam) -> Karu tidak membuka sistem selama 20 jam -> Sistem mengirim push notification peringatan merah H-4 jam -> Memasuki jam ke-25 (SLA terlewati), worker `WF-W1` menandai tiket `IsOverdue = true`, menolak auto-approve, dan otomatis mengeskalasi tiket ke inbox Kepala Bidang Keperawatan -> Kepala Bidang segera memproses persetujuan darurat.
   - *Ekspektasi:* Keselamatan pasien terjamin karena tidak ada persetujuan liar tanpa kontrol manusia, dan tiket macet segera tertangani.


---

## 33. Matriks Pengujian Penerimaan Sub-Modul 13: Perjalanan Dinas & Manajemen Biaya Operasional (Business Travel & Expense Management) — Slice `S-D5`

### 33.1 Rincian 22 Kriteria Uji Penerimaan Otomatis (`AT-HRD-TRV-01` s.d. `AT-HRD-TRV-22`)

| Kode Pengujian | Target Komponen yang Diuji | Kondisi Pengujian (Prasyarat & Masukan) | Hasil yang Diharapkan (Expected Result) |
|---|---|---|---|
| `AT-HRD-TRV-01` | Formulir Pembuatan SPPD | Pengajuan SPPD baru dengan tanggal, kota, agenda, dan moda transportasi lengkap | Tiket SPPD terdaftar berstatus `Submitted` dengan nomor registrasi unik format `SPPD/RSQ/YYYY/MM/XXX` |
| `AT-HRD-TRV-02` | Kalkulator Otomatis Lumpsum Uang Harian | Perjalanan dinas 4 hari tujuan Surabaya (Zona 2) untuk perawat pelaksana (Grade II) | Sistem otomatis menghitung `EstimatedAllowanceAmount = 4 * TarifPerDiem(Zona 2, Grade II)` dari master data |
| `AT-HRD-TRV-03` | Batas Maksimal Plafon Uang Muka 80% | Pemohon mengajukan uang muka Rp 9.000.000 pada SPPD dengan pagu disetujui Rp 10.000.000 | Sistem menolak pengajuan (`VR-TRV-02`) karena melebihi batas maksimal 80% (maks Rp 8.000.000) |
| `AT-HRD-TRV-04` | Alur Persetujuan Uang Muka 2-Pintu | Permohonan uang muka diajukan ke Atasan Langsung dan Finance Manager | Uang muka baru dapat dicairkan setelah statusnya `Approved` oleh kedua pejabat otoritas sah |
| `AT-HRD-TRV-05` | Pencairan Dana Uang Muka oleh Kasir RS | Kasir menginput nomor voucher kasir / referensi transfer bank | Status uang muka berubah `Paid`, mencatat tanggal pencairan dan audit log `HRD_TRV_ADVANCE_PAID` |
| `AT-HRD-TRV-06` | Gembok Hard-Stop Roster Medis Sub-06 | SPPD disetujui untuk dokter spesialis pada tanggal 12 s.d. 15 November | Modul Penjadwalan Roster Sub-06 menolak keras penjadwalan dinas jaga bangsal dokter pada tanggal tersebut |
| `AT-HRD-TRV-07` | Sinkronisasi Presensi Hadir Dinas Luar Sub-07 | Hari keberangkatan dinas tercapai | Modul Presensi Sub-07 otomatis menyuntikkan status log presensi "Hadir Dinas Luar" tanpa dianggap alpa |
| `AT-HRD-TRV-08` | Penyerahan Laporan Settlement Tepat Waktu | Pegawai menyerahkan laporan settlement pada H+4 kepulangan dengan bukti kuitansi riil | Berkas settlement diterima berstatus `Submitted` dalam batas SLA aman (< 7 hari) |
| `AT-HRD-TRV-09` | Notifikasi Pengingat Settlement H+3 & H+5 | Pegawai belum menyerahkan settlement pada H+3 dan H+5 kepulangan dinas | Worker `TRV-W1` menyiarkan push notification dan email peringatan batas waktu penyerahan settlement |
| `AT-HRD-TRV-10` | Pembekuan Hak SPPD Baru (H+14 Hari) | Pegawai belum menyelesaikan settlement setelah melewati 14 hari kepulangan | Sistem otomatis membekukan hak pengajuan SPPD baru (`VR-TRV-14`) dan mencatat `HRD_TRV_SPPD_FROZEN_OVERDUE` |
| `AT-HRD-TRV-11` | Otomasi Pemotongan Payroll Tunggakan > 30 Hari| Settlement menunggak melewati 30 hari hingga akhir bulan berjalan | Sisa uang muka yang belum dipertanggungjawabkan diteruskan otomatis ke slip gaji Sub-10 sebagai potongan sah |
| `AT-HRD-TRV-12` | Batas Maksimal Deklarasi Biaya Tanpa Nota 10% | Pegawai mengisi formulir deklarasi biaya darurat tanpa nota Rp 200.000 pada klaim Rp 1.000.000 | Sistem menolak klaim (`VR-TRV-08`) karena nominal deklarasi melebihi Rp 150.000 dan melebihi 10% total klaim |
| `AT-HRD-TRV-13` | Validasi Alasan Darurat Deklarasi Tanpa Nota | Pegawai mengisi formulir deklarasi darurat dengan alasan teks < 15 karakter | Sistem menolak pengajuan (`VR-TRV-09`) dan meminta penjelasan kronologi kedaruratan lebih rinci |
| `AT-HRD-TRV-14` | Kewajiban Invoice Resmi untuk Tiket & Hotel | Pegawai melampirkan kwitansi biasa tanpa NPWP/boarding pass untuk klaim tiket pesawat | Verifikator Keuangan menandai berkas tidak lolos audit dan mengembalikan ke pemohon untuk revisi |
| `AT-HRD-TRV-15` | Rekonsiliasi Settlement Selisih Kurang Bayar | Uang muka Rp 3.000.000, pengeluaran riil sah Rp 3.500.000 | Sistem menghitung `DifferenceType = Underpaid` Rp 500.000 dan menerbitkan voucher pencairan kasir ke staf |
| `AT-HRD-TRV-16` | Rekonsiliasi Settlement Selisih Lebih Bayar | Uang muka Rp 4.000.000, pengeluaran riil sah Rp 3.200.000 | Sistem menghitung `DifferenceType = Overpaid` Rp 800.000 dan menerbitkan tagihan setor balik ke Kasir RS |
| `AT-HRD-TRV-17` | Pengajuan Klaim Operasional Non-Trip | Staf farmasi mengajukan ganti biaya pembelian kasa steril cito di apotek luar Rp 250.000 | Klaim non-trip terdaftar pada `TrxExpenseClaim` tanpa mewajibkan pembuatan surat tugas perjalanan luar kota |
| `AT-HRD-TRV-18` | Verifikasi Struk Pembelian Non-Trip oleh Kasir | Staf Finance memeriksa foto struk apotek dan mencocokkan tanggal transaksi | Status klaim berubah `Verified` dan siap untuk dicairkan oleh Kasir RS |
| `AT-HRD-TRV-19` | Pembayaran Reimbursement Non-Trip oleh Kasir | Kasir mencairkan uang tunai / transfer ke staf pemohon | Status klaim berubah `Paid`, mencatat nomor referensi kasir dan audit log `HRD_EXP_CLAIM_PAID` |
| `AT-HRD-TRV-20` | Pemblokiran Bentrok Tanggal SPPD & Cuti | Staf mengajukan SPPD pada rentang tanggal di mana ia memiliki cuti tahunan disetujui | Sistem menolak pengajuan SPPD (`VR-TRV-11`) karena bentrok jadwal ketidakhadiran cuti |
| `AT-HRD-TRV-21` | Validasi Ketersediaan Anggaran Unit Cost Center| Pengajuan SPPD pada unit kerja yang pagu anggaran dinas tahunannya telah habis | Sistem menolak pengajuan (`VR-TRV-12`) dengan pesan "Pagu anggaran perjalanan dinas unit kerja habis" |
| `AT-HRD-TRV-22` | Kepatuhan Kontrak API v25 5 Tag Swagger | Audit verifikasi seluruh 24 endpoint API pada Swagger UI | Seluruh endpoint terdaftar rapi, otorisasi token JWT dan model DTO tervalidasi 100% |

---

### 33.2 Lima Skenario Uji Penerimaan Pengguna Terpadu (UAT Scenarios)

1. **UAT-TRV-01: Alur Ujung-ke-Ujung Pengajuan SPPD, Uang Muka & Pelaksanaan Tugas Dinas Medis:**  
   Dokter spesialis bedah anak diajukan dinas luar kota 3 hari untuk workshop teknik bedah minimal invasif. Sistem menghitung estimasi biaya tiket, hotel, dan lumpsum per diem otomatis. Pemohon mengajukan uang muka 80%, disetujui atasan dan Finance, lalu dicairkan kasir. Selama tanggal dinas, presensi tercatat "Hadir Dinas Luar" dan jadwal jaga bangsal dokter diblokir otomatis dari roster Sub-06.
2. **UAT-TRV-02: Alur Rekonsiliasi Settlement Selisih Kurang Bayar (*Underpaid Reimbursement*):**  
   Pasca kepulangan dinas, staf menyerahkan settlement pada H+4 dengan melampirkan boarding pass asli dan invoice hotel. Total riil melampaui uang muka karena ada kenaikan tarif hotel resmi. Verifikator Finance memeriksa bukti dan mengesahkan selisih kurang bayar. Kasir mentransfer selisih uang ke rekening staf dalam 2 hari kerja. SPPD ditutup berstatus `Completed`.
3. **UAT-TRV-03: Penegakan Disiplin Settlement: Pembekuan SPPD & Pemotongan Otomatis Slip Gaji:**  
   Staf administrasi menerima uang muka dinas Rp 2.500.000 namun tidak menyerahkan settlement hingga H+14. Sistem membekukan hak pembuatan SPPD baru. Pada H+30, worker otomatis mengirimkan instruksi pemotongan sisa uang muka ke modul payroll Sub-10. Gaji staf bulan berjalan terpotong resmi sebesar saldo yang belum dipertanggungjawabkan dengan batas aman 50% upah.
4. **UAT-TRV-04: Penanganan Kedaruratan Evakuasi Pasien & Deklarasi Biaya Tanpa Kuitansi:**  
   Perawat ICU mendampingi evakuasi ambulans pasien rujukan ke pulau seberang. Terjadi kebutuhan sewa perahu penyeberangan darurat malam hari Rp 120.000 tanpa kuitansi. Staf mengisi formulir *Declaration of Missing Receipts* disertai kronologi medis darurat. Atasan langsung mengesahkan deklarasi karena nominal < Rp 150.000 dan < 10% total klaim. Biaya diganti penuh oleh rumah sakit.
5. **UAT-TRV-05: Penggantian Biaya Operasional Non-Trip Lapangan (*Direct Expense Claim*):**  
   Analis laboratorium membeli reagen golongan darah langka secara cito di apotek rekanan Rp 350.000 menggunakan uang pribadi karena stok gudang kosong saat operasi darurat. Staf membuat klaim non-trip di portal ESS dengan foto struk resmi. Kepala Unit Lab menyetujui, Finance memverifikasi, dan Kasir mencairkan uang tunai reimbursement dalam waktu < 24 jam.
