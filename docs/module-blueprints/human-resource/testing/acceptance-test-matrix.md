# Human Resource — Matriks Acceptance Test

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Dokumen | `testing/acceptance-test-matrix.md` |
| `contract_version` | `v2` |
| `last_changed_in` | `v2` |
| Status | `draft` — **belum** `approved` |
| Owner | Technical owner (`HRD-DEC-015`), bersama pemilik proses untuk skenario UAT |
| `approved_by` / `approved_at` | **Belum ada** |
| `input_revision` | `contracts/state-transition-matrix.md` `v2`; `contracts/validation-matrix.md` `v2`; `contracts/api-contract.md` `v2`; `flowcharts/**` |
| `input_hash` — decision log | `0f4bb66d96d5fcd10a388e7b98efa08510f9edf50e3033dddf84951ad09854a3` |
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

## 12. Privasi dan jejak audit — berlaku lintas slice

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-HRD-SEC-01` | Kolom sensitif tidak masuk custom logger | **Gagal:** memicu jalur yang menyentuh kolom bertanda sensitif tidak meninggalkan isinya di log | `INTEGRATION` | Isi log yang diperiksa, dengan nilai tersamarkan |
| `AT-HRD-SEC-02` | Permintaan selain `GET` meninggalkan jejak | **Berhasil:** setiap permintaan yang mengubah data meninggalkan catatan siapa, kapan, dan apa yang diubah | `INTEGRATION` | Isi catatan audit |
| `AT-HRD-SEC-03` | Permintaan `GET` tidak dicatat | **Berhasil:** pembacaan data tidak membanjiri catatan audit | `INTEGRATION` | Isi catatan audit yang membuktikan ketiadaan baris untuk pembacaan |
| `AT-HRD-SEC-04` | Hak akses pada API dan frontend sama | **Gagal:** tidak ada tombol di layar yang dijaga hak akses berbeda dari endpoint yang dipanggilnya | `MANUAL` | Perbandingan baris pada `contracts/api-contract.md` dengan skema fitur pada `03-frontend-architecture.md` |
| `AT-HRD-SEC-05` | Penghapusan bersifat penandaan | **Gagal:** tidak ada jalur yang benar-benar menghapus baris dari basis data | `INTEGRATION` | Test yang membuktikan baris masih ada dengan penanda hapus |

---

## 13. Kemampuan yang sengaja TIDAK punya baris di matriks ini

| Kemampuan | Slice | Alasan |
| --- | --- | --- |
| Kredensial, kewenangan klinis, SPK/RKK, OPPE, FPPE | `S-C1` | `BLOCKED`. Menuliskan skenario ujinya berarti menetapkan kewenangan praktik yang belum ada — `HRD-Q-08` |
| Kesehatan dan keselamatan kerja staf | `S-C6` | `BLOCKED`. Aturan aksesnya belum disahkan K3RS — `HRD-DEC-010` masih `draft` |
| Perencanaan tenaga kerja, rekrutmen, benefit, layanan HR | `S-D1` s.d. `S-D4` | `BLOCKED` oleh `HRD-Q-05` |
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
| **Jumlah** | **96** | **5** |

Setiap kelompok yang dapat diuji memuat sekurang-kurangnya satu skenario berhasil dan satu
skenario gagal, sesuai aturan pada bagian 0.1.
