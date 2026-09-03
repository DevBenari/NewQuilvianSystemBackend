# Bank Darah — Validation Matrix

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` · Contract version `v4` — **`approved`** |
| `last_changed_in` | `v4` |
| Owner | Pemilik proses BDRS · pemilik proses klinis |
| `approved_by` / `approved_at` | `Sukmagp` / `2026-09-03` |
| Sumber | `00-interview-decisions.md` revisi 4 (INV/AC) · `03-domain-architecture.md` revisi 3 |

Pesan ditulis dalam Bahasa Indonesia yang dipahami pengguna, **bukan** istilah teknis. Kolom "Kode
teknis" adalah kode respons yang muncul di log/gerbang, bukan yang dibaca pengguna. Ini **satu-satunya**
tempat kalimat pesan penolakan hidup; `flowcharts/` dan `state-transition-matrix.md` hanya merujuk
kodenya.

Konvensi kode HTTP: `400` isian tidak lengkap/format salah · `403` tidak berhak · `404` data tidak
ditemukan · `409` bentrok konkurensi atau status sudah berubah · `422` melanggar aturan bisnis.

---

## 1. Order darah

| Kode | Berlaku pada | Kondisi | Pesan bagi pengguna | HTTP |
| --- | --- | --- | --- | --- |
| `VAL-BD-001` | Buat order | Sudah ada order aktif untuk pasien + kunjungan + komponen yang sama | "Sudah ada order darah aktif untuk pasien dan komponen ini pada kunjungan yang sama. Lanjutkan hanya dengan alasan tertulis." | `422` |
| `VAL-BD-002` | Baris order | Jumlah diminta ≤ 0 | "Jumlah kantong yang diminta harus lebih dari nol." | `400` |
| `VAL-BD-003` | Baris order | Komponen tidak ada di katalog / diketik bebas | "Komponen darah harus dipilih dari katalog, tidak boleh diketik bebas." | `400` |
| `VAL-BD-004` | Order `Expired` | Percobaan mengaktifkan kembali | "Order yang sudah kedaluwarsa tidak dapat dibuka kembali. Buat order baru pada kunjungan yang berjalan." | `422` |
| `VAL-BD-010` | Order manual | Pasien / kunjungan / dokter peminta / unit asal / pelaku input kosong | "Order manual wajib mengisi pasien, kunjungan, dokter peminta, unit asal, dan petugas yang menginput." | `400` |
| `VAL-BD-011` | Order tersimpan | Jejak pelaku input tidak tercatat | "Setiap order wajib menyimpan siapa yang membuatnya." | `422` |
| `VAL-BD-012` | Keputusan klinis | `MstPatient.BloodType` dipakai untuk menilai kesesuaian darah | "Golongan darah pada data pendaftaran tidak boleh dipakai untuk menilai kesesuaian darah. Gunakan hasil pemeriksaan Bank Darah." | `422` |
| `VAL-BD-013` | Buat order | Unit pelayanan `IsAvailableForBloodOrder=false` | "Unit pelayanan ini belum diberi kewenangan memesan darah." | `403` |

## 2. Permintaan PMI & penerimaan

| Kode | Berlaku pada | Kondisi | Pesan bagi pengguna | HTTP |
| --- | --- | --- | --- | --- |
| `VAL-BD-006` | Buat permintaan | Sudah ada permintaan aktif untuk kebutuhan yang sama | "Sudah ada permintaan darah yang masih berjalan untuk kebutuhan ini. Tidak boleh dibuat permintaan baru." | `422` |
| `VAL-BD-007` | Buat permintaan | Jumlah kantong kosong | "Jumlah kantong yang diminta wajib diisi." | `400` |
| `VAL-BD-014` | Terima kantong | Kantong berlebih | (Bukan penolakan) "Kiriman melebihi permintaan. Kantong tetap dicatat diterima dan masuk daftar menunggu keputusan." | `200` |
| `VAL-BD-015` | Stok | Percobaan menambah stok tanpa penerimaan fisik | "Stok bertambah hanya setelah kantong diterima secara fisik." | `422` |

## 3. Kantong: alokasi, bukti, pemberian, koreksi

| Kode | Berlaku pada | Kondisi | Pesan bagi pengguna | HTTP |
| --- | --- | --- | --- | --- |
| `VAL-BD-017` | Berikan | Kantong belum dialokasikan | "Kantong harus dialokasikan ke order pasien sebelum diberikan." | `422` |
| `VAL-BD-018` | Berikan | Tidak ada bukti kecocokan & bukan jalur darurat | "Bukti pemeriksaan kecocokan belum tercatat. Darah tidak dapat diberikan." | `422` |
| `VAL-BD-019` | Berikan | Bukti kecocokan atas nama pasien lain | "Bukti kecocokan yang ada bukan untuk pasien ini. Catat bukti kecocokan terhadap pasien tujuan." | `422` |
| `VAL-BD-020` | Berikan | Bukti kecocokan sudah lewat masa berlaku | "Bukti kecocokan sudah lewat masa berlaku. Diperlukan bukti kecocokan yang baru." | `422` |
| `VAL-BD-020b` | Berikan | Masa berlaku komponen belum dikonfigurasi | "Masa berlaku bukti kecocokan untuk komponen ini belum ditetapkan. Pemberian ditahan sampai dikonfigurasi." | `422` |
| `VAL-BD-018c` | Alokasi | Ada alokasi aktif lain pada kantong (konkurensi) | "Kantong ini baru saja dialokasikan petugas lain. Muat ulang dan pilih kantong lain." | `409` |
| `VAL-BD-021` | Jalur darurat | Bukan peran berwenang, atau alasan kosong | "Jalur darurat hanya untuk peran berwenang dan wajib mengisi alasan." | `403` |
| `VAL-BD-023` | Batalkan alokasi | Kantong sudah `Issued` | "Kantong sudah diberikan. Pembatalan tidak dapat dilakukan; gunakan catatan koreksi bila pencatatannya keliru." | `422` |
| `VAL-BD-024` | Catat koreksi | Bukan peran berwenang | "Pencatatan koreksi hanya untuk peran berwenang." | `403` |
| `VAL-BD-025` | Hapus/anulir pemberian | Percobaan menghapus atau membalik pemberian | "Pemberian darah tidak dapat dihapus atau dibatalkan. Satu-satunya jalur perbaikan adalah catatan koreksi." | `422` |
| `VAL-BD-033` | Alokasi | Kantong `Excess`/`PendingReview` dialokasikan langsung | "Kantong ini menunggu keputusan dan tidak dapat langsung dialokasikan. Selesaikan statusnya lebih dulu." | `422` |
| `VAL-BD-016` | Pembatalan / penyelesaian | Alasan tidak dipilih dari daftar terkendali | "Alasan wajib dipilih dari daftar, tidak boleh diketik bebas." | `400` |

## 4. Golongan darah & konflik

| Kode | Berlaku pada | Kondisi | Pesan bagi pengguna | HTTP |
| --- | --- | --- | --- | --- |
| `VAL-BD-030` | Catat hasil | Pemeriksa atau waktu pemeriksaan kosong | "Hasil golongan darah wajib menyimpan pemeriksa dan waktu pemeriksaan." | `400` |
| `VAL-BD-034` | Gerbang klinis | Pasien sedang `IsConflictHeld` | "Golongan darah pasien ini sedang bertentangan dan ditahan. Selesaikan perbedaannya lebih dulu." | `422` |
| `VAL-BD-037` | Validasi / penyelesaian konflik | Bukan peran validator | "Hanya peran validator yang boleh memvalidasi atau menyelesaikan perbedaan hasil golongan darah." | `403` |
| `VAL-BD-051` | Selesaikan konflik | Tidak menunjuk pemeriksaan ulang tervalidasi | "Perbedaan hasil hanya dapat diselesaikan setelah ada pemeriksaan ulang yang tervalidasi." | `422` |
| `VAL-BD-054` | Selesaikan konflik | Percobaan menutup dengan pilihan "mayoritas" otomatis | "Sistem tidak menentukan hasil yang benar. Validator wajib menyatakan hasil yang berlaku." | `422` |

## 4b. Penyimpanan kantong dan gerbang lokasi — baru pada `v2`

Diturunkan dari `DEC-BD-035` sampai `DEC-BD-038` dan `INV-BD-025` sampai `INV-BD-030`.

| Kode | Berlaku pada | Kondisi | Pesan bagi pengguna | HTTP |
| --- | --- | --- | --- | --- |
| `VAL-BD-060` | Tetapkan / pindahkan lokasi | Lokasi tujuan sedang nonaktif | "Lokasi penyimpanan itu sudah tidak aktif dan tidak dapat dipilih. Pilih lokasi lain yang masih aktif." | `422` |
| `VAL-BD-061` | Tetapkan lokasi pertama | Kantong sudah pernah ditempatkan | "Kantong ini sudah punya lokasi penyimpanan. Gunakan perpindahan lokasi bila ingin memindahkannya." | `422` |
| `VAL-BD-062` | Pindahkan lokasi | Kantong belum pernah ditempatkan | "Kantong ini belum punya lokasi penyimpanan. Tetapkan lokasinya lebih dulu." | `422` |
| `VAL-BD-063` | Alokasi | Kantong belum melewati penyimpanan (`Received`) | "Kantong belum disimpan pada lokasi penyimpanan, sehingga belum dapat dialokasikan. Tetapkan lokasi penyimpanannya lebih dulu." | `422` |
| `VAL-BD-064` | Alokasi dan pengalihan ke pasien lain | Lokasi penempatan terakhir kantong sedang nonaktif | "Kantong ini berada di lokasi penyimpanan yang sudah tidak aktif. Pindahkan dulu ke lokasi yang aktif sebelum dialokasikan." | `422` |
| `VAL-BD-065` | Pemberian jalur normal | Lokasi penempatan terakhir kantong sedang nonaktif | "Kantong ini berada di lokasi penyimpanan yang sudah tidak aktif dan belum dapat diberikan. Pindahkan dulu ke lokasi yang aktif." | `422` |
| `VAL-BD-066` | Pemberian jalur darurat | Keterangan gerbang yang dilewati tidak diisi, atau tidak sesuai keadaan kantong | "Pemberian darurat wajib menyebutkan apa yang dilewati: bukti kecocokan, lokasi penyimpanan yang tidak aktif, atau keduanya." | `422` |
| `VAL-BD-067` | Master lokasi penyimpanan | Kode atau nama lokasi sudah dipakai lokasi lain | "Kode lokasi penyimpanan itu sudah dipakai. Gunakan kode lain." | `422` |
| `VAL-BD-068` | Menonaktifkan lokasi penyimpanan | Masih ada kantong yang tercatat di lokasi itu | **Peringatan, bukan penolakan.** "Lokasi dinonaktifkan. Ada N kantong yang masih tercatat di sana dan belum dapat dialokasikan sampai dipindahkan ke lokasi aktif." | `200` |

**Kenapa `VAL-BD-068` meloloskan, bukan menolak.** Menonaktifkan lokasi justru dilakukan ketika ada
yang salah dengan lokasi itu — rusak, tidak layak, tidak lagi dipercaya. Menolak penonaktifan karena
masih ada isinya akan memaksa petugas memindahkan darah dari kulkas yang sudah diketahui bermasalah
**sebelum** boleh menandainya bermasalah. Yang benar adalah sebaliknya: tandai dulu supaya gerbang
tertutup, baru pindahkan (`DEC-BD-037`). Peringatan berisi jumlah kantong ada supaya pekerjaan yang
menunggu itu terlihat, bukan supaya penonaktifannya dibatalkan.

**Kenapa `VAL-BD-064` dan `VAL-BD-065` dua kode berbeda padahal kondisinya sama.** Keduanya dipicu
keadaan yang sama tetapi pada langkah yang berbeda, dan petugas yang membacanya sedang mengerjakan hal
yang berbeda. Menyatukannya membuat pesan "belum dapat dialokasikan" muncul saat petugas menekan tombol
berikan — membingungkan, dan menyembunyikan bahwa gerbang pemberian memang dinilai **ulang**
(`INV-BD-029`), bukan diwarisi dari alokasi.

**Yang sengaja tidak divalidasi:** perpindahan lokasi **tidak** menuntut alasan terkendali. Bukti yang
disetujui hanya menuntut lokasi asal, lokasi tujuan, pelaku, dan waktu (`INV-BD-026`). Menambah
kewajiban alasan berarti mengarang aturan yang tidak diminta.

---

## 4c. Wewenang, jalur darurat, dan koreksi dua tahap — baru pada `v3`

Diturunkan dari `DEC-BD-039`, `DEC-BD-040`, `DEC-BD-041` dan `INV-BD-031` sampai `INV-BD-033`.

| Kode | Berlaku pada | Kondisi | Pesan bagi pengguna | HTTP |
| --- | --- | --- | --- | --- |
| `VAL-BD-069` | Penyelesaian konflik golongan darah | Pelaku bukan validator klinis yang ditunjuk | "Hanya validator klinis yang ditunjuk yang boleh menyelesaikan perbedaan hasil golongan darah." | `403` |
| `VAL-BD-070` | Pemberian jalur darurat | Keterangan kondisi kedaruratan tidak diisi | "Sebutkan keadaan yang membuat pemberian ini harus dilakukan sekarang." | `422` |
| `VAL-BD-071` | Pemberian jalur darurat | Peran penerbit otorisasi tidak dinyatakan | "Sebutkan Anda menerbitkan otorisasi ini sebagai Dokter Bank Darah atau sebagai dokter penanggung jawab pasien." | `422` |
| `VAL-BD-072` | Pemberian jalur darurat | Pelaku bukan Dokter BDRS maupun DPJP pasien | "Hanya Dokter Bank Darah atau dokter penanggung jawab pasien yang boleh menerbitkan otorisasi darurat." | `403` |
| `VAL-BD-073` | Keputusan koreksi | Pemutus adalah orang yang sama dengan peminta | "Koreksi tidak dapat disetujui oleh orang yang mengajukannya. Mintakan keputusan kepada Dokter Bank Darah lain." | `422` |
| `VAL-BD-074` | Keputusan koreksi | Pelaku tidak berwenang memutuskan koreksi | "Hanya Dokter Bank Darah yang boleh menyetujui atau menolak koreksi pencatatan." | `403` |
| `VAL-BD-075` | Keputusan koreksi | Koreksi sudah pernah disetujui atau ditolak | "Koreksi ini sudah diputuskan sebelumnya dan tidak dapat diputuskan ulang. Ajukan koreksi baru bila masih ada yang perlu diperbaiki." | `422` |
| `VAL-BD-076` | Pengajuan koreksi | Bukti pendukung tidak diisi | "Jelaskan bukti pendukung yang mendasari koreksi ini." | `422` |
| `VAL-BD-077` | Penolakan koreksi | Alasan penolakan tidak diisi | "Sebutkan alasan koreksi ini ditolak, supaya pengaju mengetahui yang perlu diperbaiki." | `422` |

**Kenapa `VAL-BD-073` bernilai `422`, bukan `403`.** Menyetujui koreksi sendiri bukan soal pelakunya
tidak berwenang — ia justru **berwenang**, dan mungkin sah memegang kedua butir hak akses sekaligus.
Yang dilanggar adalah aturan bisnisnya: gerbang dua tahap kehilangan seluruh gunanya bila satu orang
menempati kedua sisi. Karena itu penjaganya ada di lapisan aturan bisnis, bukan di mesin hak akses —
`403` akan menyesatkan pembaca log seolah hak aksesnya kurang.

**Kenapa `VAL-BD-070` dan `VAL-BD-071` terpisah dari `VAL-BD-066`.** Ketiganya sama-sama tentang
kelengkapan otorisasi darurat, tetapi menahan hal yang berbeda: `VAL-BD-066` gerbang mana yang dilewati,
`VAL-BD-070` keadaan klinis yang mendasarinya, `VAL-BD-071` dengan wewenang apa penerbit bertindak.
Menggabungkannya menjadi satu pesan "isian tidak lengkap" membuat petugas menebak bagian mana yang
kurang, pada saat yang paling tidak tepat untuk menebak.

**Yang sengaja tidak divalidasi:** sistem **tidak** memeriksa apakah penerbit otorisasi darurat benar
DPJP dari pasien yang bersangkutan. Bukti yang disetujui tidak menuntutnya, dan Bank Darah bukan pemilik
data penugasan DPJP. Yang dijaga adalah kelengkapan rekam; kebenaran penugasannya terbaca saat ditinjau.

---

## 4d. Wewenang bukti kecocokan, penyelesaian, dan pembatalan order — baru pada `v4`

Diturunkan dari `DEC-BD-042`, `DEC-BD-043`, `DEC-BD-044` dan `INV-BD-034`, `INV-BD-035`.

| Kode | Berlaku pada | Kondisi | Pesan bagi pengguna | HTTP |
| --- | --- | --- | --- | --- |
| `VAL-BD-078` | Catat bukti kecocokan | Pelaku tidak memegang kewenangan validasi | "Hanya petugas Bank Darah dengan kewenangan validasi yang boleh menyatakan hasil pemeriksaan kecocokan." | `403` |
| `VAL-BD-079` | Pemberian jalur normal | Bukti kecocokan yang berlaku menyatakan **tidak cocok** | "Hasil pemeriksaan kecocokan menyatakan kantong ini tidak cocok untuk pasien tersebut. Kantong tidak dapat diberikan." | `422` |
| `VAL-BD-080` | Pengalihan kantong ke pasien lain | Pelaku tidak memegang kewenangan klinis BDRS | "Hanya pemegang kewenangan klinis Bank Darah yang boleh mengalihkan kantong ke pasien lain." | `403` |
| `VAL-BD-081` | Pengembalian kantong ke PMI | Pelaku tidak memegang kewenangan operasional BDRS | "Hanya pemegang kewenangan operasional Bank Darah yang boleh mengembalikan kantong ke PMI." | `403` |
| `VAL-BD-082` | Penetapan kantong tidak layak | Pelaku tidak memegang kewenangan penetapan kelayakan | "Hanya pemegang kewenangan penetapan kelayakan yang boleh menyatakan kantong tidak layak." | `403` |
| `VAL-BD-083` | Pembatalan order darah | Kategori alasan tidak sesuai peran pelaku — alasan klinis dipakai petugas BDRS, atau sebaliknya | "Pilih alasan pembatalan yang sesuai: pembatalan klinis oleh dokter peminta, atau pembatalan operasional oleh petugas Bank Darah." | `422` |

**Kenapa `VAL-BD-079` bernilai `422`, bukan `403`.** Pelakunya berwenang memberikan darah; yang
menahan adalah **isi buktinya**, bukan haknya. Ini aturan bisnis, bukan hak akses.

**Kenapa `VAL-BD-079` ada sama sekali.** Sebelum `v4`, bukti hanya dicatat ketika hasilnya cocok,
sehingga keberadaan bukti sudah cukup menjadi gerbang. Sejak `DEC-BD-042` menuntut hasil keputusan
tersimpan, bukti bertanda **tidak cocok** juga ada di sistem — dan gerbang yang hanya memeriksa
keberadaan akan meloloskannya. Pengetatan ini penurunan dari `DEC-BD-042`, dan pemilik proses
**menegaskannya lewat `DEC-BD-046`** pada 3 September 2026. Arah *fail-closed* karena itu bukan lagi
pilihan sementara yang menunggu jawaban, melainkan aturan yang sudah diputuskan: gerbang pemberian
jalur normal memeriksa **isi** bukti, bukan keberadaannya.

**Kenapa `VAL-BD-080` sampai `VAL-BD-082` tiga kode terpisah.** Ketiganya menahan hal yang sama —
kewenangan kurang — tetapi pada tiga tindakan yang wewenangnya memang berbeda (`INV-BD-034`). Satu kode
untuk ketiganya akan membuat pesan menyebut "kewenangan penyelesaian" yang tidak ada, dan petugas tidak
tahu kewenangan mana yang sebenarnya kurang.

**Yang sengaja tidak divalidasi:** sistem **tidak** memeriksa apakah pembatal order benar-benar dokter
peminta yang tercatat pada order itu. `DEC-BD-044` menyebut dua peran, bukan mengikat pembatalan pada
individu tertentu. Yang dijaga adalah kesesuaian **kategori alasan** dengan peran pelaku
(`VAL-BD-083`), dan kelengkapan jejaknya (`INV-BD-035`).

---

## 5. Tindakan Bank Darah

| Kode | Berlaku pada | Kondisi | Pesan bagi pengguna | HTTP |
| --- | --- | --- | --- | --- |
| `VAL-BD-026` | Catat tindakan | Tidak menunjuk order sah | "Tindakan Bank Darah wajib menunjuk satu order yang sah." | `400` |
| `VAL-BD-027` | Tindakan | Percobaan menghitung tarif sendiri | "Tarif tidak dihitung di modul ini; dirujuk dari data tindakan bertarif." | `422` |

---

## Catatan konsistensi

- Setiap kode di sini dirujuk oleh `state-transition-matrix.md` (kolom "Bila dilanggar") dan diuji oleh
  `testing/acceptance-test-matrix.md`.
- Aturan bergerbang **fail-closed**: bila konfigurasi (mis. masa berlaku `VAL-BD-020b`) atau peran
  (`DEF-BD-004`) belum ditetapkan, gerbang menolak — bukan meloloskan dengan nilai tebakan. Hal yang
  sama berlaku bila master lokasi penyimpanan kosong: tanpa satu pun lokasi aktif, tidak ada kantong
  yang dapat melewati penyimpanan, sehingga tidak ada yang dapat dialokasikan (`VAL-BD-063`).
- Gerbang pemberian dinilai **ulang** saat pemberian dicoba, tidak pernah mewarisi hasil pemeriksaan
  saat alokasi (`INV-BD-029`). Karena itu sebuah kantong dapat lolos `VAL-BD-064` pada Senin dan tetap
  tertahan `VAL-BD-065` pada Selasa — itu perilaku yang benar, bukan ketidakkonsistenan.
- Pesan **MUST NOT** memuat data medis/pribadi pasien.
