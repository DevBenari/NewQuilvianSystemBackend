# Acceptance Test Matrix — Modul Rekam Medis

| Field | Value |
|---|---|
| Blueprint ID | `RM-BP-001` |
| Contract version | `0.1.0` |
| Status | `draft` |
| Owner | Product/domain dan clinical governance authority: `OPEN` |
| Input revisions | `00-interview-decisions.md` revision `2`; `01-existing-capability-map.md` revision `1` |

> **PERINGATAN DASAR DESAIN.** Disusun di atas keputusan berstatus `draft`. Lihat `RM-DEC-025`.

---

## 1. Keadaan pengujian saat ini

Ini harus dinyatakan lebih dulu karena mengubah arti seluruh tabel di bawah.

| Kenyataan | Bukti |
|---|---|
| **Backend tidak memiliki project test sama sekali** | Penelusuran seluruh repository tidak menemukan project test apa pun |
| Frontend hanya memiliki 4 berkas test | `tests/e2e/auth-security.spec.mjs`, `tests/e2e/route-smoke.spec.mjs`, `tests/unit/auth-security.test.mjs`, `tests/unit/base-components-regression.test.mjs` |
| Tidak satu pun test menyentuh alur klinis | Penelusuran `tests/` tidak menemukan berkas bernama klinis |

Akibatnya: **membuat project test backend adalah prasyarat, bukan pelengkap.** Modul ini
menutup tiga celah pada kode yang sedang dipakai IGD dan antrean dokter (`RM-CAP-011`, `012`,
`013`). Mengubah kode berjalan tanpa jaring pengaman otomatis adalah risiko yang tidak
sebanding dengan penghematannya.

Apakah project test dibuat lebih dulu atau berjalan bersamaan adalah pertanyaan penyusunan
urutan kerja yang **belum diputuskan** — tercatat sebagai open question nomor 11 pada decision
log dan diteruskan ke `/plan-module-delivery`.

---

## 2. Matriks uji penerimaan

Kolom "Jenis test" memakai istilah: **Unit** untuk pengujian satu bagian kode, **Integrasi**
untuk pengujian yang menyentuh basis data, **E2E** untuk pengujian lewat antarmuka, dan
**Manual** untuk yang dibuktikan pengamatan orang.

### 2.1 Keutuhan dokumen

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|---|
| `AT-RM-01` | `RM-DEC-003` | Mengubah CPPT berstatus `Signed` | Integrasi | Ditolak `400` dengan pesan yang mengarahkan ke addendum. Isi CPPT di basis data tidak berubah sedikit pun |
| `AT-RM-02` | `RM-DEC-003`, `RM-DEC-021` | Penulis menandatangani CPPT miliknya | Integrasi | Status menjadi `Signed`; `SignedAt`, `SignedByUserId`, dan `SignatureDeviceInfo` terisi; tidak ada permintaan kata sandi |
| `AT-RM-03` | `RM-DEC-003` lapis kedua | Kunjungan ditutup sementara ada dua CPPT `Draft` | Integrasi | Kedua CPPT menjadi `LockedUnsigned` dengan `LockTrigger = EncounterClosed`, dalam transaksi yang sama |
| `AT-RM-10` | `RM-DEC-003` | Mencoba mengembalikan dokumen `Signed` ke `Draft` | Integrasi | Ditolak. Tidak ada endpoint yang menyediakannya, dan pemanggilan langsung service ditolak |
| `AT-RM-11` | `RM-DEC-006` | Mencoba menandatangani dokumen `LockedUnsigned` | Integrasi | Ditolak `400` dengan pesan mengarahkan ke addendum |
| `AT-RM-18` | `RM-DEC-003` | Dokter membuka daftar catatan miliknya yang belum ditandatangani | Integrasi | Daftar hanya memuat dokumen `Draft` milik pengguna, bukan milik orang lain |
| `AT-RM-24` | `RM-DEC-013` | CPPT dibuat lewat endpoint yang sudah ada | Integrasi | Satu baris keutuhan berstatus `Draft` ikut tercipta; `AuthorUserId` terisi dari `ProviderUserId` |

### 2.2 Addendum dan kewenangan penulis

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|---|
| `AT-RM-04` | `RM-DEC-004` | Penulis menambah addendum pada catatannya yang sudah `Signed` | Integrasi | Addendum tersimpan dengan `Sequence = 1`; isi CPPT asli **tidak berubah**; status dokumen tetap `Signed` |
| `AT-RM-05` | `RM-DEC-004` | Perawat mencoba menambah addendum pada catatan dokter yang akunnya masih aktif | Integrasi | Ditolak `403` dengan pesan bahwa hanya penulis yang dapat menambahkan koreksi |
| `AT-RM-14` | `RM-DEC-020` | Kepala unit menambah addendum pada catatan dokter yang akunnya sudah nonaktif | Integrasi | Diterima; `IsSubstituteAuthor` bernilai benar; `AuthorUserId` berisi kepala unit, **bukan** dokter yang nonaktif |
| `AT-RM-17` | `RM-DEC-004` | Menambah tiga addendum berturut-turut pada satu dokumen | Integrasi | `Sequence` berurut 1, 2, 3; status dokumen tetap `Signed` sepanjang ketiganya |
| `AT-RM-26` | `RM-DEC-020` | Kepala unit membuat penetapan berhalangan tanpa mengisi batas waktu | Integrasi | Ditolak `400`. Penetapan tanpa batas waktu tidak boleh tersimpan |
| `AT-RM-27` | `RM-DEC-020` | Menambah addendum memakai penetapan yang `ValidUntil`-nya sudah lewat | Integrasi | Ditolak `403` dengan pesan bahwa penetapan sudah berakhir |
| `AT-RM-28` | `RM-DEC-004` | Mencoba mengubah atau menghapus addendum yang sudah ada | Integrasi | Tidak ada endpoint yang menyediakannya |

### 2.3 Jejak dan kewenangan akses

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|---|
| `AT-RM-06` | `RM-DEC-016` | Membuka rekam medis pasien yang punya kunjungan aktif | Integrasi | Isi dikembalikan tanpa diminta alasan; satu baris jejak `AccessType = RoutineCare` tercatat |
| `AT-RM-07` | `RM-DEC-005`, `RM-DEC-016` | Membuka rekam medis pasien tanpa kunjungan aktif, tanpa mengisi alasan | Integrasi | Ditolak `400`; **isi rekam medis tidak dikembalikan sama sekali**; tidak ada kebocoran sebagian |
| `AT-RM-08` | `RM-DEC-015` | Mencoba mengubah atau menghapus baris jejak akses | Integrasi | Tidak ada endpoint yang menyediakannya |
| `AT-RM-12` | `RM-DEC-015` | Membuka rekam medis sepuluh kali berturut-turut | Integrasi | Sepuluh baris jejak tercatat, tidak sembilan dan tidak sebelas |
| `AT-RM-13` | `RM-DEC-017` | Pengguna ber-role `SuperAdmin` membuka rekam medis pasien tanpa kunjungan aktif | Integrasi | Tetap diminta alasan; aksesnya tercatat dan ditandai perlu ditinjau |
| `AT-RM-16` | `RM-DEC-022` | Membuka `PrivateNote` pada pasien yang **punya** kunjungan aktif | Integrasi | Tetap diminta alasan; jejak tercatat dengan `AccessScope = PrivateNote` |
| `AT-RM-25` | `RM-DEC-016` | Penilaian kunjungan aktif gagal karena gangguan basis data | Integrasi | Akses diperlakukan sebagai beralasan, bukan sebagai rawatan. Kegagalan teknis tidak melonggarkan kewenangan |
| `AT-RM-29` | `RM-DEC-005` | Petugas rekam medis membuka antrean tinjauan | Integrasi | Hanya baris `IsFlaggedForReview` bernilai benar yang muncul |
| `AT-RM-30` | `RM-DEC-015` | Penulisan jejak akses gagal | Integrasi | Permintaan dijawab `503`; **isi rekam medis tidak dikembalikan** |

### 2.4 Penelusuran berkas

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|---|
| `AT-RM-09` | `RM-DEC-002` | Membuka riwayat pasien yang punya tiga kunjungan berbeda | Integrasi | Dokumen dari ketiga kunjungan tampil dalam satu daftar berurut waktu, tanpa perlu membuka kunjungan satu per satu |
| `AT-RM-22` | `RM-CAP-007` | Membuka rekam medis pasien yang `MergedToPatientId`-nya terisi | Integrasi | Ditolak `409` disertai nomor rekam medis pengganti. **Tidak** menampilkan riwayat yang terpotong |
| `AT-RM-31` | `RM-DEC-002` | Membuka riwayat pasien dengan sangat banyak dokumen | Integrasi | Jumlah baris dibatasi; penyaringan rentang tanggal berfungsi; tidak ada permintaan yang berjalan tanpa batas |
| `AT-RM-32` | Bagian 7 arsitektur backend | Membuka riwayat yang memuat jenis dokumen selain CPPT | E2E | Dokumen selain CPPT tampil **tanpa** status keutuhan, disertai keterangan bahwa jenis itu belum tunduk aturan keutuhan |

### 2.5 Perubahan perilaku pada kode berjalan

Kelompok ini yang paling penting, karena menyentuh alur yang sedang dipakai.

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|---|
| `AT-RM-19` | `RM-CAP-012` | Mengirim `ProviderUserId` orang lain pada permintaan ubah CPPT | Integrasi | Permintaan tidak gagal, tetapi `ProviderUserId` di basis data **tidak berubah**, dan `AuthorUserId` pada baris keutuhan juga tidak |
| `AT-RM-20` | `RM-CAP-013` | Mengirim `IsReadOnlyGenerated = false` pada CPPT yang read-only | Integrasi | Nilai di basis data **tidak berubah** |
| `AT-RM-21` | `RM-DEC-014` | Menjalankan migration pengisian data lama | Integrasi | CPPT pada kunjungan selesai bernilai `LockedUnsigned`; pada kunjungan berjalan bernilai `Draft`; yang dibatalkan bernilai `Cancelled` |
| `AT-RM-33` | `RM-DEC-014` | Migration pengisian data lama pada CPPT tanpa `ProviderUserId` | Integrasi | Baris tetap dibuat dengan `IsAuthorKnown = false`, tidak dilewati diam-diam |
| `AT-RM-34` | `RM-DEC-019` | Alur antrean dokter dijalankan penuh setelah perubahan | E2E | Menulis, menyimpan, dan menandatangani CPPT berjalan; alur IGD tidak terganggu |
| `AT-RM-35` | Integration contract 2.1 | Pendaftaran keutuhan gagal saat CPPT dibuat | Integrasi | Pembuatan CPPT ikut dibatalkan; tidak ada CPPT tanpa baris keutuhan |
| `AT-RM-36` | Integration contract 2.2 | Penguncian gagal saat kunjungan ditutup | Integrasi | Penutupan kunjungan ikut dibatalkan; kunjungan tetap terbuka |

### 2.6 Privasi dan pencatatan

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|---|
| `AT-RM-23` | Aturan output dokumentasi | Membuat addendum lalu memeriksa isi log | Integrasi | `AddendumText`, `CorrectionReason`, dan `AccessReason` **tidak muncul** di log. Hanya `EntityId`, controller, action, dan status |
| `AT-RM-37` | `RM-DEC-022` | Memeriksa respons riwayat biasa | Integrasi | `PrivateNote` **tidak ada** di respons mana pun selain endpoint khususnya |
| `AT-RM-38` | Kamus data bagian 4 | Memeriksa isi tabel jejak akses | Integrasi | Tidak ada kolom yang memuat isi klinis |

### 2.7 Antarmuka

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|---|
| `AT-RM-39` | `RM-FE-008` | Melihat satu dokumen pada layar rekam medis | E2E | Status keutuhan dan status alur kerja terlihat berbeda, tidak menyatu jadi satu penanda |
| `AT-RM-40` | `RM-FE-002` | Melihat dokumen yang punya addendum | E2E | Addendum tampil menempel pada dokumen induknya, bukan sebagai entri terpisah |
| `AT-RM-41` | `RM-FE-003` | Membuka pasien tanpa kunjungan aktif | E2E | Kotak isian alasan muncul **sebelum** isi rekam medis terlihat, bukan sesudah |
| `AT-RM-42` | `RM-FE-006` | Melihat dokumen bertanda `VeryConfidential` | E2E | Keterangan bahwa label kerahasiaan belum membatasi akses terlihat jelas |
| `AT-RM-43` | `RM-DEC-014` | Petugas rekam medis melihat laporan kelengkapan setelah migration | Manual | Banyaknya catatan bertanda tidak ditandatangani terlihat, dan sudah dijelaskan lebih dulu kepada petugas |

---

## 3. Jalur gagal yang wajib diuji

Diringkas terpisah karena inilah yang paling sering terlewat. Seluruhnya sudah ada di tabel
atas; daftar ini memastikan tidak ada yang luput saat penyusunan urutan kerja.

| Jalur gagal | ID |
|---|---|
| Mengubah dokumen terkunci | `AT-RM-01` |
| Menandatangani catatan orang lain | `AT-RM-02` sisi negatifnya |
| Menandatangani dokumen yang sudah terkunci | `AT-RM-11` |
| Addendum oleh yang tidak berwenang | `AT-RM-05` |
| Addendum memakai penetapan kedaluwarsa | `AT-RM-27` |
| Penetapan tanpa batas waktu | `AT-RM-26` |
| Akses tanpa alasan pada pasien tanpa kunjungan aktif | `AT-RM-07` |
| `SuperAdmin` tanpa alasan | `AT-RM-13` |
| Penilaian kunjungan gagal | `AT-RM-25` |
| Pencatatan jejak gagal | `AT-RM-30` |
| Pendaftaran keutuhan gagal | `AT-RM-35` |
| Penguncian saat penutupan gagal | `AT-RM-36` |
| Pasien hasil penggabungan | `AT-RM-22` |
| Data lama tanpa penulis | `AT-RM-33` |

---

## 4. Yang tidak dapat diuji otomatis

Dinyatakan terbuka supaya tidak dianggap sudah terbukti padahal belum.

| Hal | Alasan | Cara membuktikannya |
|---|---|---|
| Jumlah catatan lama yang akan berstatus `LockedUnsigned` | Data produksi tidak diaudit (batas audit nomor 3) | Dijalankan pada salinan data nyata sebelum rilis |
| Lama waktu migration pengisian data lama | Jumlah barisnya belum diketahui | Sama seperti di atas |
| Apakah dokter benar-benar menandatangani catatannya | Perilaku manusia | Diamati dari perbandingan `Signed` dan `LockedUnsigned` setelah beberapa pekan |
| Apakah alasan akses diisi bermakna atau asal | Perilaku manusia | Tinjauan berkala unit rekam medis |
| Apakah pasien dengan dua nomor rekam medis benar ada | `RM-CAP-007` masih `Unknown` | Penelusuran data sebelum rilis |

Baris ketiga dan keempat pantas diperhatikan. Keduanya adalah cara mengetahui apakah desain ini
benar-benar bekerja atau hanya tampak bekerja. Bila setelah sebulan hampir seluruh catatan
berstatus `LockedUnsigned`, berarti alur penandatanganan terlalu merepotkan dan perlu ditinjau
ulang — bukan berarti dokternya lalai.

---

## 5. Traceability ringkas

| Decision | Uji yang membuktikannya |
|---|---|
| `RM-DEC-003` | `AT-RM-01`, `02`, `03`, `10`, `11` |
| `RM-DEC-004` | `AT-RM-04`, `05`, `17`, `28` |
| `RM-DEC-005` | `AT-RM-07`, `29` |
| `RM-DEC-006` | `AT-RM-11` |
| `RM-DEC-013` | `AT-RM-24` |
| `RM-DEC-014` | `AT-RM-21`, `33`, `43` |
| `RM-DEC-015` | `AT-RM-08`, `12`, `30` |
| `RM-DEC-016` | `AT-RM-06`, `07`, `25` |
| `RM-DEC-017` | `AT-RM-13` |
| `RM-DEC-018` | `AT-RM-42` |
| `RM-DEC-019` | `AT-RM-19`, `20`, `34` |
| `RM-DEC-020` | `AT-RM-14`, `26`, `27` |
| `RM-DEC-021` | `AT-RM-02` |
| `RM-DEC-022` | `AT-RM-16`, `37` |
