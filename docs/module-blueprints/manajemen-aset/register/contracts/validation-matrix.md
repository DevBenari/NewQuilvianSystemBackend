# Register Aset — Matriks Validasi

| Field | Value |
|---|---|
| `last_changed_in` | `AST-REG-1.0.0` — `draft` |
| Owner | Rizki · `approved_by`: — · `approved_at`: — |
| Traceability | `AST-RULE-001..014`, `060`; `AST-DEC-033`, `037`, `043`, `044`, `050`; `AST-DES-014..016`, `020` |

Pesan ditulis sebagaimana dibaca petugas. Kode HTTP pada kolom terakhir.

## 1. Master

| Kode | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | HTTP |
|---|---|---|---|---|---|
| `AST-VAL-REG-050` | Kode unik per master | Semua master | Kode sudah dipakai | "Kode ini sudah dipakai data lain." | 409 |
| `AST-VAL-REG-051` | Bentuk kode inventaris | Kelompok | Bukan 2–11 huruf besar/angka. Contoh sah: `ALKMED`; tidak sah: `alat-medis`, `ALATKEDOKTERAN` (14) | "Kode inventaris harus 2–11 huruf besar atau angka tanpa spasi." | 400 |
| `AST-VAL-REG-052` | Kode terlarang | Kelompok | Kode = `PNY`, `KND`, `PMF`, `IMP`, `MUT`, `PJM`, `OPN`, `NLI`, `HPS`, `PLH` | "Kode ini dipakai untuk nomor dokumen aset. Pilih kode lain." | 400 |
| `AST-VAL-REG-053` | Kode terkunci | Kelompok | Kode diubah padahal sudah ada aset bernomor di kelompok ini | "Kode inventaris tidak dapat diubah karena sudah dipakai pada nomor aset." | 409 |
| `AST-VAL-REG-055` | Lokasi milik unit aktif | Lokasi | Unit tidak ada atau nonaktif | "Unit pemilik ruangan tidak ditemukan atau sudah tidak aktif." | 400 |
| `AST-VAL-REG-056` | Rentang pengaturan | Pengaturan | Hari menganggur di luar 1–3650; peringatan dokumen di luar 1–365; masa pemanfaatan di luar 1–120 bulan | "Nilai pengaturan di luar batas yang diizinkan." | 400 |

## 2. Pencatatan aset

| Kode | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | HTTP |
|---|---|---|---|---|---|
| `AST-VAL-REG-001` | Nama wajib | Draft, ajukan | Kosong atau > 250 karakter | "Nama aset wajib diisi, paling panjang 250 karakter." | 400 |
| `AST-VAL-REG-002` | Kelompok wajib dan aktif | Draft, ajukan | Kosong atau nonaktif | "Pilih kelompok aset yang masih aktif." | 400 |
| `AST-VAL-REG-003` | Jenis kepemilikan wajib dan aktif | Draft, ajukan | Kosong atau nonaktif | "Pilih jenis kepemilikan yang masih aktif." | 400 |
| `AST-VAL-REG-004` | Jenis perolehan dan buktinya | Ajukan | `HibahMasuk` tanpa nomor berita acara hibah | "Aset hibah wajib mencantumkan nomor berita acara hibah." | 400 |
| `AST-VAL-REG-005` | Tanggal | Draft, ajukan | Tanggal perolehan kosong atau sesudah hari ini; tanggal siap pakai sebelum tanggal perolehan atau sesudah hari ini | "Tanggal siap pakai tidak boleh sebelum tanggal perolehan dan tidak boleh di masa depan." | 400 |
| `AST-VAL-REG-006` | Harga perolehan | Ajukan | < 0, lebih dari 2 desimal, atau 0 untuk `Pembelian`/`HibahMasuk` | "Harga perolehan wajib lebih dari Rp 0 dengan paling banyak 2 angka desimal." | 400 |
| `AST-VAL-REG-007` | Atribut alat medis | Ajukan | Klasifikasi kelompok `IsMedical` dan salah satu dari merek, tipe/model, nomor seri, produsen, izin edar kosong. Contoh: monitor pasien tanpa nomor izin edar ditolak | "Alat medis wajib diisi merek, tipe, nomor seri, produsen, dan nomor izin edar." | 400 |
| `AST-VAL-REG-008` | Jumlah draft | Buat draft | `Quantity` di luar 1–200 | "Jumlah unit yang dibuat sekaligus 1 sampai 200." | 400 |
| `AST-VAL-REG-009` | GR yang ditautkan | Draft, ajukan | GR tidak ditemukan atau dibatalkan | "Penerimaan barang yang dipilih tidak ditemukan atau sudah dibatalkan." | 400 |
| `AST-VAL-REG-010` | Alasan wajib | Tolak, batalkan, akhiri | Kosong atau > 500 karakter | "Alasan wajib diisi, paling panjang 500 karakter." | 400 |
| `AST-VAL-REG-011` | Kelompok aktif saat disetujui | Setujui | Kelompok sudah dinonaktifkan sejak diajukan | "Kelompok aset sudah tidak aktif. Kembalikan ke petugas untuk diganti." | 409 |
| `AST-VAL-REG-012` | Larangan menyetujui sendiri | Setujui, tolak | Penyetuju = pengaju | "Anda tidak dapat menyetujui pengajuan Anda sendiri." | 403 |
| `AST-VAL-REG-013` | Nomor gagal terbit | Setujui | Alokator Number Series gagal | "Nomor inventaris belum dapat diterbitkan. Coba lagi beberapa saat." | 409 |
| `AST-VAL-REG-014` | Hanya draft yang dapat diubah | Ubah draft | Status bukan `Draft` | "Aset ini sudah diajukan atau terdaftar, sehingga tidak dapat diubah dari formulir draft." | 409 |
| `AST-VAL-REG-015` | Data sudah berubah | Semua perubahan | `RowVersion` tidak sama | "Data ini baru saja diubah orang lain. Muat ulang lalu coba lagi." | 409 |
| `AST-VAL-REG-016` | Batas unit | Baca dan ubah | Aset bukan milik unit pengguna dan pengguna tanpa `ReadAllUnits` | "Aset ini bukan milik unit Anda." | 403 |

Contoh `AST-VAL-REG-007` pada pembuatan massal: 10 draft syringe pump dibuat sekaligus tanpa nomor
seri. Draft tersimpan, tetapi setiap draft baru bisa diajukan setelah nomor serinya diisi.

## 3. Penyerahan

| Kode | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | HTTP |
|---|---|---|---|---|---|
| `AST-VAL-REG-020` | Aset terdaftar | Serahkan | Ada butir yang bukan `Terdaftar` | "Hanya aset yang sudah terdaftar yang dapat diserahkan." | 409 |
| `AST-VAL-REG-021` | Bukan Hilang | Serahkan | Ada butir `Hilang` | "Aset yang berstatus hilang tidak dapat diserahkan." | 409 |
| `AST-VAL-REG-022` | Tujuan sesuai | Serahkan | Penempatan awal untuk aset yang sudah ditempatkan, atau `PindahDalamUnit` ke unit lain | "Aset ini sudah ditempatkan. Untuk memindahkan ke unit lain gunakan mutasi." | 409 |
| `AST-VAL-REG-023` | Lokasi dan penanggung jawab sah | Serahkan | Lokasi bukan milik unit tujuan, atau pegawai tidak aktif | "Ruangan atau penanggung jawab tidak sesuai dengan unit tujuan." | 400 |
| `AST-VAL-REG-024` | Satu proses aktif | Serahkan, ajukan pemanfaatan, dan proses sub-modul lain | Aset sedang dalam proses lain. Contoh: aset yang sedang diusulkan dihapus | "Aset {nomor} sedang dalam proses {proses}. Selesaikan proses itu terlebih dahulu." | 409 |
| `AST-VAL-REG-025` | Penerima yang dituju | Konfirmasi, tolak terima | Pengguna bukan Kepala Unit tujuan dan bukan penanggung jawab yang dituju | "Hanya penerima yang dituju yang dapat mengonfirmasi penyerahan ini." | 403 |
| `AST-VAL-REG-026` | Unit sedang opname | Serahkan | Unit asal atau tujuan punya sesi opname `Berjalan`, `Pencocokan`, atau `MenungguVerifikasi` | "Unit ini sedang dalam inventarisasi. Penyerahan dapat dilakukan setelah inventarisasi selesai." | 409 |

## 4. Kondisi

| Kode | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | HTTP |
|---|---|---|---|---|---|
| `AST-VAL-REG-030` | Foto wajib | Laporan dari unit | Tanpa foto | "Lampirkan foto kondisi aset." | 400 |
| `AST-VAL-REG-031` | Laporan masih terbuka | Tetapkan, tolak | Laporan bukan `Dilaporkan` | "Laporan ini sudah diputuskan." | 409 |
| `AST-VAL-REG-032` | Berita acara kehilangan | Setujui Hilang | Tanpa lampiran berita acara | "Status hilang wajib disertai berita acara kehilangan." | 400 |
| `AST-VAL-REG-033` | Larangan menyetujui sendiri | Setujui Hilang | Penyetuju = pelapor | "Anda tidak dapat menyetujui laporan kehilangan yang Anda buat sendiri." | 403 |
| `AST-VAL-REG-034` | Aset hilang | Laporkan | Aset `Hilang` dan laporan bukan "ditemukan kembali" | "Aset ini berstatus hilang. Laporkan sebagai ditemukan kembali." | 400 |
| `AST-VAL-REG-035` | Satu laporan terbuka | Laporkan | Sudah ada laporan `Dilaporkan` untuk aset ini | "Aset ini masih punya laporan kondisi yang belum diputuskan." | 409 |

## 5. Pemanfaatan

| Kode | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | HTTP |
|---|---|---|---|---|---|
| `AST-VAL-REG-040` | Aset layak dimanfaatkan | Ajukan | Aset bukan `Terdaftar`, berstatus `Hilang`/`RusakBerat`, atau terkunci proses lain | "Aset {nomor} tidak dapat dimanfaatkan karena {sebab}." | 409 |
| `AST-VAL-REG-041` | Rentang tanggal | Draft, ajukan | Tanggal akhir ≤ tanggal mulai | "Tanggal berakhir harus sesudah tanggal mulai." | 400 |
| `AST-VAL-REG-042` | Isian perjanjian | Ajukan | Mitra, nomor perjanjian, atau nilai kosong; nilai < 0 | "Nama mitra, nomor perjanjian, dan nilai perjanjian wajib diisi." | 400 |
| `AST-VAL-REG-043` | Jenis pemanfaatan | Draft | Kosong | "Pilih jenis pemanfaatan: sewa, kerja sama operasional, atau pinjam pakai." | 400 |
| `AST-VAL-REG-044` | Aset milik RS | Ajukan | Jenis kepemilikan bukan `IsCapitalizable` (misalnya aset KSO milik vendor) | "Aset yang bukan milik rumah sakit tidak dapat dimanfaatkan ke pihak lain." | 409 |
| `AST-VAL-REG-045` | Penyetuju sesuai tahap | Setujui, tolak | Penyetuju = pengaju; atau Kepala Bagian Aset mencoba menyetujui pemanfaatan yang wajib ke Direktur. Contoh: KSO 5 tahun hanya bisa diteruskan, tidak bisa disetujui Kepala Bagian Aset | "Pemanfaatan ini harus disetujui {penyetuju}." | 403 |

## 6. Impor data awal

| Kode | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | HTTP |
|---|---|---|---|---|---|
| `AST-VAL-REG-060` | Berkas templat | Unggah | Bukan `.xlsx`, atau kolom templat tidak lengkap | "Berkas harus memakai templat impor aset terbaru." | 400 |
| `AST-VAL-REG-061` | Batas baris | Unggah | Lebih dari 5.000 baris data (bawaan, diatur `AssetManagement:Import:MaxRows`) | "Berkas berisi lebih dari 5.000 baris. Pecah per unit atau per kelompok." | 400 |
| `AST-VAL-REG-062` | Kelompok dan jenis kepemilikan dikenal | Validasi baris | Kode tidak ditemukan atau nonaktif | Baris {n}: "Kode kelompok {kode} tidak dikenal." | baris galat |
| `AST-VAL-REG-063` | Unit, lokasi, penanggung jawab dikenal | Validasi baris | Tidak ditemukan, atau lokasi bukan milik unit | Baris {n}: "Lokasi {kode} bukan milik unit {unit}." | baris galat |
| `AST-VAL-REG-064` | Akumulasi wajar | Validasi baris | Akumulasi < 0 atau > harga − residu. Contoh: harga Rp 60.000.000, residu 0, akumulasi Rp 61.000.000 → ditolak | Baris {n}: "Akumulasi penyusutan melebihi nilai yang dapat disusutkan." | baris galat |
| `AST-VAL-REG-065` | Tanggal siap pakai | Validasi baris | Sesudah tanggal awal | Baris {n}: "Tanggal siap pakai sesudah tanggal awal." | baris galat |
| `AST-VAL-REG-066` | Nomor seri alat medis unik | Validasi baris | Nomor seri + merek kembar di dalam berkas atau dengan aset terdaftar | Baris {n}: "Nomor seri sudah dipakai aset lain." | baris galat |
| `AST-VAL-REG-067` | Larangan menyetujui sendiri | Kedua persetujuan | Penyetuju = pengunggah | "Anda tidak dapat menyetujui berkas yang Anda unggah sendiri." | 403 |
| `AST-VAL-REG-068` | Tanggal awal diisi | Unggah | `MstAssetSetting.CutoverDate` kosong | "Tanggal awal belum diatur. Isi di Pengaturan Aset sebelum mengimpor." | 409 |

Satu baris galat membuat seluruh batch `GagalValidasi` (`AST-DEC-037`).

## 7. Lampiran dan dokumen

| Kode | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | HTTP |
|---|---|---|---|---|---|
| `AST-VAL-REG-070` | Jenis berkas | Semua unggahan | Ekstensi di luar `AssetManagement:Attachment:AllowedExtensions` | "Jenis berkas tidak diizinkan." | 400 |
| `AST-VAL-REG-071` | Ukuran berkas | Semua unggahan | Melebihi `MaxFileSizeBytes` | "Berkas terlalu besar." | 413 |
| `AST-VAL-REG-072` | Pengaturan lampiran | Semua unggahan | Pengaturan kosong | "Unggahan belum dapat dipakai karena pengaturan lampiran belum diisi administrator." | 503 |
| `AST-VAL-REG-080` | Isian dokumen | Tambah, ubah | Nomor dokumen kosong; tanggal habis kosong untuk jenis yang `HasExpiry` | "Nomor dokumen wajib diisi; dokumen jenis ini wajib punya tanggal berakhir." | 400 |
