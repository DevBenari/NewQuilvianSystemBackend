# Hemodialisa — Matriks Validasi

| Field | Value |
|---|---|
| Blueprint ID | `HMD-BP-001` |
| Contract version | `HMD-CONTRACT-v1` — `last_changed_in: HMD-CONTRACT-v1`, status `approved` |
| Owner | Muhammad Hamzah |
| `approved_by` / `approved_at` | Muhammad Hamzah / 2026-09-18T15:40:07+07:00 |
| `input_revision` | `contracts/state-transition-matrix.md` r1 |

Dokumen ini adalah **satu-satunya** tempat kalimat pesan penolakan hidup. Flowchart dan layar
tidak menyalinnya; keduanya cukup menyebut sebab penolakannya secara singkat.

Pesan ditulis sebagaimana dibaca petugas — bukan istilah teknis. Kode HTTP disebut di kolom
terakhir, bukan di dalam kalimat.

---

## 1. Permintaan HD

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `HMD-VAL-001` | Buat permintaan | Alasan klinis kosong, atau kunjungan pasien tidak sah | "Permintaan tidak dapat dikirim karena alasan klinis belum diisi atau data kunjungan pasien tidak ditemukan." | 400 |
| `HMD-VAL-002` | Terima permintaan | Permintaan sudah tidak berstatus menunggu | "Permintaan ini sudah diproses orang lain. Muat ulang halaman untuk melihat status terbaru." | 409 |
| `HMD-VAL-003` | Tahan permintaan | Alasan penahanan kosong | "Alasan penahanan wajib diisi agar unit peminta tahu apa yang perlu dilengkapi." | 400 |
| `HMD-VAL-004` | Lepas tahanan | Permintaan tidak sedang ditahan | "Permintaan ini tidak sedang ditahan." | 409 |
| `HMD-VAL-005` | Tolak permintaan | Pengguna bukan dokter, atau alasan klinis kosong | "Penolakan permintaan hemodialisa hanya dapat dilakukan dokter, dan alasan klinisnya wajib diisi." | 403 atau 400 |
| `HMD-VAL-006` | Batalkan permintaan | Permintaan sudah diterima unit HD | "Permintaan sudah diterima unit Hemodialisa dan tidak dapat dibatalkan dari sini. Hubungi koordinator unit." | 409 |

---

## 2. Episode HD

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `HMD-VAL-010` | Buat episode | Pasien tidak ditemukan, atau dokter penanggung jawab belum dipilih | "Episode tidak dapat dibuat karena data pasien atau dokter penanggung jawab belum lengkap." | 400 |
| `HMD-VAL-011` | Aktifkan episode | Pasien sudah punya episode hemodialisa yang aktif | "Pasien ini sudah memiliki episode hemodialisa yang aktif. Tutup atau tangguhkan episode itu lebih dulu." | 409 |
| `HMD-VAL-012` | Tangguhkan episode | Alasan penangguhan kosong | "Alasan penangguhan wajib diisi." | 400 |
| `HMD-VAL-013` | Tutup episode | Masih ada sesi yang belum difinalisasi | "Episode belum dapat ditutup karena masih ada sesi yang catatannya belum disahkan." | 422 |

**Contoh `HMD-VAL-011`:** Ibu Sinta sudah menjalani program HD rutin sejak Januari dengan
episode `HD-2026-00042` berstatus aktif. Petugas administrasi membuka episode baru untuk pasien
yang sama, lalu menekan Aktifkan. Permintaan ditolak dengan pesan di atas. Episode kedua tetap
tersimpan berstatus draf sehingga tidak hilang, tetapi tidak berlaku.

---

## 3. Resep HD

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `HMD-VAL-020` | Buat resep | Episode tidak berstatus aktif | "Resep hanya dapat dibuat pada episode hemodialisa yang aktif." | 422 |
| `HMD-VAL-021` | Aktifkan resep | Ada parameter wajib yang kosong, atau akses vaskular yang dipilih sudah tidak layak | "Resep belum dapat diaktifkan. Periksa kembali frekuensi, durasi, target penarikan cairan, dan akses vaskular yang dipilih." | 422 |
| `HMD-VAL-022` | Gantikan resep | Resep pengganti gagal diaktifkan | "Penggantian resep gagal. Resep lama tetap berlaku." | 409 |
| `HMD-VAL-023` | Batalkan resep aktif | Masih ada sesi berjalan yang memakai resep ini | "Resep tidak dapat dibatalkan karena sedang dipakai sesi yang berjalan." | 409 |
| `HMD-VAL-024` | Sunting resep | Resep sudah berstatus aktif | "Resep yang sudah aktif tidak dapat diubah. Buat resep baru untuk menggantikannya." | 423 |

**Contoh `HMD-VAL-024`:** dokter ingin menaikkan target penarikan cairan dari 2.000 ml menjadi
2.500 ml pada resep yang sudah aktif. Penyuntingan langsung ditolak. Yang dilakukan adalah
membuat resep baru bertarget 2.500 ml lalu mengaktifkannya; resep lama otomatis menjadi
digantikan, dan riwayat perubahan instruksi tetap terbaca utuh.

---

## 4. Penjadwalan sesi

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `HMD-VAL-030` | Bentuk sesi | Episode tidak aktif, atau tidak ada resep aktif | "Sesi belum dapat dijadwalkan karena episode atau resep hemodialisa pasien belum aktif." | 422 |
| `HMD-VAL-031` | Tetapkan jadwal | Pasien sudah terjadwal pada waktu yang bertumpang tindih | "Pasien sudah memiliki sesi lain pada jam tersebut." | 409 |
| `HMD-VAL-032` | Tetapkan jadwal | Mesin sudah dipakai sesi lain pada waktu yang bertumpang tindih | "Mesin ini sudah dipakai pasien lain pada jam tersebut." | 409 |
| `HMD-VAL-033` | Tetapkan jadwal | Station sudah dipakai sesi lain pada waktu yang bertumpang tindih | "Station ini sudah dipakai pasien lain pada jam tersebut." | 409 |
| `HMD-VAL-034` | Tetapkan jadwal | Mesin berstatus diblokir, dalam perawatan, atau dinyatakan tidak laik | "Mesin ini sedang tidak dapat digunakan. Pilih mesin lain." | 422 |
| `HMD-VAL-035` | Tetapkan jadwal | Pasien memerlukan isolasi tetapi mesin atau station yang dipilih tidak memenuhinya | "Pasien ini memerlukan mesin atau ruang khusus. Mesin yang dipilih tidak memenuhi kebutuhan tersebut." | 422 |
| `HMD-VAL-036` | Tandai pasien datang | Kunjungan pasien belum ada atau tidak sah | "Pasien belum memiliki data kunjungan yang sah untuk hari ini." | 422 |
| `HMD-VAL-037` | Tetapkan petugas | Petugas terbaca tidak berwenang **dan** penegakan kewenangan menyala | "Petugas ini tidak memiliki kewenangan dialisis yang berlaku." | 422 |
| `HMD-VAL-038` | Tetapkan petugas | Jumlah pasien per perawat melebihi batas **dan** penegakan rasio menyala | "Perawat ini sudah menangani jumlah pasien maksimum pada shift tersebut." | 422 |

**Contoh `HMD-VAL-032`:** koordinator menjadwalkan Bapak Darma ke mesin `HD-M-03` pukul
07.00–11.00. Pada saat hampir bersamaan, koordinator lain menjadwalkan Ibu Wulan ke mesin yang
sama pukul 09.00–13.00. Rentangnya bertumpang tindih dua jam, jadi permintaan kedua ditolak.
Hanya satu jadwal yang tersimpan; tidak ada keadaan di mana dua pasien terjadwal pada mesin yang
sama.

**Peringatan tanpa penolakan.** Dua keadaan berikut **tidak** menolak, hanya memperingatkan,
karena pengaturannya bernilai awal mati:

| Keadaan | Yang ditampilkan |
|---|---|
| Petugas berstatus belum dapat diverifikasi | "Kewenangan petugas ini belum dapat diperiksa karena data kewenangan belum tersedia." |
| Rasio pasien per perawat terlampaui, penegakan mati | "Perawat ini menangani lebih dari batas yang disarankan pada shift tersebut." |

---

## 5. Pra-HD dan memulai sesi

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `HMD-VAL-040` | Nyatakan siap | Ada butir checklist wajib yang belum terpenuhi dan tidak dilewati secara sah | "Sesi belum dapat dinyatakan siap karena masih ada butir persiapan yang belum terpenuhi." | 422 |
| `HMD-VAL-041` | Nyatakan siap | Penilaian Pra-HD belum diisi | "Penilaian sebelum tindakan belum lengkap. Isi berat badan dan tanda vital pasien lebih dulu." | 422 |
| `HMD-VAL-042` | Nyatakan siap | Kesiapan unit pada tanggal dan shift itu bukan siap | "Unit hemodialisa belum dinyatakan siap untuk shift ini." | 422 |
| `HMD-VAL-043` | Nyatakan siap | Dokter penanggung jawab sesi belum ditetapkan | "Sesi belum dapat dinyatakan siap karena dokter penanggung jawab belum ditetapkan." | 422 |
| `HMD-VAL-044` | Lewati butir checklist | Butir yang dipilih tidak boleh dilewati | "Butir ini tidak dapat dilewati. Lengkapi lebih dulu sebelum sesi dimulai." | 422 |
| `HMD-VAL-045` | Tahan sesi | Alasan penahanan kosong | "Alasan penahanan wajib diisi." | 400 |
| `HMD-VAL-046` | Lewati butir checklist | Alasan pelewatan kosong | "Alasan wajib diisi ketika melewati butir persiapan." | 400 |
| `HMD-VAL-050` | Mulai sesi | Sesi tidak berstatus siap | "Sesi belum dapat dimulai. Periksa kembali status persiapan." | 422 |
| `HMD-VAL-051` | Mulai sesi | Mesin berubah status menjadi tidak siap setelah pemeriksaan tadi | "Mesin berubah status dan tidak lagi dapat digunakan. Pilih mesin lain lalu nyatakan siap kembali." | 422 |
| `HMD-VAL-052` | Mulai sesi | Kunjungan pasien tidak lagi sah | "Konteks kunjungan pasien tidak dapat diverifikasi. Sesi tidak dapat dimulai." | 422 |
| `HMD-VAL-053` | Mulai sesi | Sesi sudah berjalan atau sudah dimulai orang lain | "Sesi ini sudah dimulai." | 409 |

**Contoh `HMD-VAL-044`, dan inilah wujud `HMD-ASM-001`:** perawat membuka Pra-HD Ibu Sinta.
Sebelas butir hijau, satu butir "Akses vaskular layak dipakai" masih merah karena tangan pasien
bengkak. Dokter menekan Lewati dan mengetik alasan. Permintaan **ditolak** dengan pesan di atas,
karena seluruh butir sekarang bertanda tidak boleh dilewati. Yang dapat dilakukan adalah menahan
sesi atau memperbaiki keadaannya. Setelah badan klinis kelak menetapkan butir mana yang boleh
dilewati, tombol itu bekerja tanpa satu baris kode pun berubah.

**Contoh `HMD-VAL-053`:** perawat menekan tombol Mulai, jaringan lambat, lalu ia menekan lagi.
Permintaan kedua membawa kunci idempotency yang sama, sehingga server mengembalikan sesi yang
**sama** — bukan membuat sesi kedua dan bukan membuat tindakan kedua. Bila kuncinya berbeda
tetapi sesinya sudah berjalan, muncul pesan di atas.

---

## 6. Pemantauan, obat, dan komplikasi

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `HMD-VAL-054` | Catat pemantauan | Sesi tidak sedang berjalan | "Pemantauan hanya dapat dicatat ketika sesi sedang berlangsung." | 422 |
| `HMD-VAL-055` | Catat pemantauan | Waktu pengamatan mendahului waktu mulai sesi | "Waktu pengamatan tidak boleh lebih awal dari waktu sesi dimulai." | 400 |
| `HMD-VAL-056` | Catat pemberian obat | Obat, dosis, satuan, atau jalur pemberian kosong | "Catatan pemberian obat belum lengkap. Isi obat, dosis, satuan, dan jalur pemberiannya." | 400 |
| `HMD-VAL-057` | Catat komplikasi | Jenis kejadian atau tindakan penanganan kosong | "Catatan komplikasi belum lengkap. Isi jenis kejadian dan tindakan yang dilakukan." | 400 |

---

## 7. Pasca-HD dan finalisasi

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `HMD-VAL-060` | Selesaikan sesi | Penilaian Pasca-HD atau disposisi pasien belum diisi | "Sesi belum dapat diselesaikan karena penilaian setelah tindakan dan tujuan pasien belum diisi." | 422 |
| `HMD-VAL-061` | Hentikan sesi | Alasan penghentian kosong | "Alasan penghentian wajib diisi." | 400 |
| `HMD-VAL-070` | Selesaikan dokumentasi | Ada isian minimum yang belum lengkap | "Dokumentasi belum dapat diselesaikan. Periksa kembali waktu selesai, berat badan setelah tindakan, jumlah cairan yang ditarik, dan tanda vital akhir." | 422 |
| `HMD-VAL-071` | Kembalikan untuk dilengkapi | Alasan pengembalian kosong | "Alasan pengembalian wajib diisi agar perawat tahu bagian mana yang perlu dilengkapi." | 400 |
| `HMD-VAL-072` | Sahkan dan kunci | Pengguna bukan dokter penanggung jawab sesi | "Pengesahan catatan hemodialisa hanya dapat dilakukan dokter penanggung jawab sesi." | 403 |
| `HMD-VAL-073` | Sahkan dan kunci | Pendaftaran dokumen ke daftar keutuhan rekam medis gagal | "Catatan gagal disahkan karena pendaftaran dokumen rekam medis tidak berhasil. Coba lagi." | 422 |
| `HMD-VAL-074` | Sahkan dan kunci | Pengguna yang mengesahkan sama dengan yang menyelesaikan dokumentasi, sedangkan pengaturan menghendaki dua orang berbeda | "Pengesahan harus dilakukan orang yang berbeda dari yang menyelesaikan dokumentasi." | 422 |
| `HMD-VAL-075` | Ubah catatan sesi | Sesi sudah berstatus disahkan | "Catatan sesi ini sudah disahkan dan tidak dapat diubah. Gunakan koreksi rekam medis untuk memperbaikinya." | 423 |

**Contoh `HMD-VAL-075`:** dua hari setelah sesi disahkan, perawat menyadari berat badan setelah
tindakan salah ketik — tertulis 58,5 kg padahal seharusnya 55,8 kg. Ia membuka catatan sesi dan
menekan Simpan. Permintaan ditolak dengan pesan di atas. Perbaikannya lewat koreksi rekam medis:
catatan asli tetap tersimpan apa adanya, dan koreksi tercatat sebagai tambahan beserta alasan,
penulis, dan waktunya. Inilah sebabnya angka yang salah tidak pernah hilang begitu saja.

---

## 8. Kesiapan unit

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `HMD-VAL-100` | Bentuk lembar kesiapan | Sudah ada lembar untuk tanggal dan shift yang sama | "Lembar kesiapan untuk tanggal dan shift ini sudah ada." | 409 |
| `HMD-VAL-101` | Nyatakan unit siap | Masih ada butir wajib yang belum terpenuhi | "Unit belum dapat dinyatakan siap karena masih ada butir pemeriksaan yang belum terpenuhi." | 422 |
| `HMD-VAL-102` | Nyatakan unit siap | Hasil pemeriksaan air sudah melewati masa berlakunya | "Hasil pemeriksaan pengolahan air sudah kedaluwarsa. Perbarui hasilnya sebelum menyatakan unit siap." | 422 |
| `HMD-VAL-103` | Nyatakan unit tidak siap | Alasan kosong | "Alasan wajib diisi ketika menyatakan unit tidak siap." | 400 |

---

## 9. Mesin dan station

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `HMD-VAL-090` | Ubah status mesin atau station | Sedang dipakai sesi yang berjalan | "Mesin sedang dipakai sesi yang berlangsung dan statusnya belum dapat diubah." | 409 |
| `HMD-VAL-091` | Ubah status mesin atau station | Alasan perubahan kosong | "Alasan perubahan status wajib diisi agar riwayatnya dapat ditelusuri." | 400 |
| `HMD-VAL-092` | Daftarkan mesin atau station | Kode sudah dipakai pada unit yang sama | "Kode ini sudah dipakai. Gunakan kode lain." | 409 |

---

## 10. Aturan yang berlaku di seluruh modul

| Aturan | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|
| `HMD-VAL-900` | Konteks pasien atau sesi tidak dapat diverifikasi | "Konteks pasien tidak dapat diverifikasi. Tidak ada data klinis yang dapat ditampilkan maupun disimpan." | 422 |
| `HMD-VAL-901` | Pengguna tidak memiliki hak akses untuk tindakan ini | "Anda tidak memiliki hak akses untuk tindakan ini." | 403 |
| `HMD-VAL-902` | Data yang diubah sudah berubah di tangan orang lain | "Data ini sudah diubah pengguna lain. Muat ulang halaman untuk melihat versi terbaru." | 409 |
| `HMD-VAL-903` | Waktu yang dikirim dari perangkat pengguna berbeda jauh dari waktu server | Waktu yang tersimpan tetap waktu server; tidak ada pesan penolakan yang ditampilkan | — |

`HMD-VAL-900` adalah aturan paling keras di modul ini: ketika konteks pasien gagal
diverifikasi, **tidak satu pun** penulisan data klinis diterima, dan data pasien sebelumnya
tidak boleh tetap tampil di layar sambil menunggu.
