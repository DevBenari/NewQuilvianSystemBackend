# Validation Matrix — Modul Rekam Medis

| Field | Value |
|---|---|
| Blueprint ID | `RM-BP-001` |
| Contract version | `0.1.0` |
| Status | `draft` |
| Owner | Product/domain authority: `OPEN` |
| `approved_by` / `approved_at` | — / — |
| Input revisions | `00-interview-decisions.md` revision `2` |
| Compatibility impact | **Aditif**, kecuali tiga aturan pada bagian 5 yang mengubah perilaku endpoint berjalan |

> **PERINGATAN DASAR DESAIN.** Disusun di atas keputusan berstatus `draft`. Lihat `RM-DEC-025`.

Pesan pada dokumen ini ditulis sebagaimana akan dibaca pengguna. Pesan yang menyebut nama kolom
atau istilah teknis dianggap belum selesai ditulis, sebab yang membacanya perawat, dokter, dan
petugas rekam medis.

---

## 1. Keutuhan dokumen

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---:|
| Dokumen terkunci tidak dapat diubah | `PUT` CPPT | Status keutuhan `Signed` atau `LockedUnsigned` | "Catatan ini sudah ditandatangani dan tidak dapat diubah. Gunakan addendum untuk membetulkan." | `400` |
| Dokumen dibatalkan tidak dapat diubah | `PUT` CPPT | Status keutuhan `Cancelled` | "Catatan ini sudah dibatalkan dan tidak dapat diubah." | `400` |
| Hanya penulis yang boleh menandatangani | `POST` sign | Pengguna bukan `AuthorUserId` | "Hanya penulis catatan yang dapat menandatanganinya." | `403` |
| Dokumen kosong tidak dapat ditandatangani | `POST` sign | Seluruh ruas isi kosong | "Catatan masih kosong, jadi belum dapat ditandatangani." | `400` |
| Dokumen terkunci tidak dapat ditandatangani ulang | `POST` sign | Status bukan `Draft` | "Catatan ini sudah terkunci. Gunakan addendum bila perlu melengkapi." | `400` |
| Dokumen wajib punya baris keutuhan | `POST` CPPT | Pendaftaran keutuhan gagal | "Catatan gagal disimpan. Silakan coba lagi." | `500` |

Baris terakhir menyembunyikan sebab teknis dengan sengaja. Pengguna tidak perlu tahu istilah
"baris keutuhan"; yang perlu ia tahu adalah catatannya belum tersimpan dan harus diulang.
Sebab teknisnya tetap masuk ke `LoggerService` untuk ditelusuri pengembang.

---

## 2. Addendum

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---:|
| Dokumen harus sudah terkunci | `POST` addendum | Status keutuhan masih `Draft` | "Catatan ini belum terkunci. Perbaiki langsung pada catatannya." | `400` |
| Alasan koreksi wajib diisi | `POST` addendum | `CorrectionReason` kosong | "Alasan koreksi wajib diisi." | `400` |
| Isi addendum wajib diisi | `POST` addendum | `AddendumText` kosong | "Isi koreksi wajib diisi." | `400` |
| Panjang isi addendum terbatas | `POST` addendum | `AddendumText` melebihi 4000 huruf | "Isi koreksi terlalu panjang. Batasnya 4000 huruf." | `400` |
| Bukan penulis dan tidak ada penetapan | `POST` addendum | Lihat pemeriksaan bertingkat pada state transition matrix | "Hanya penulis catatan yang dapat menambahkan koreksi." | `403` |
| Penetapan sudah berakhir | `POST` addendum | `ValidUntil` sudah lewat | "Penetapan kewenangan pengganti sudah berakhir. Hubungi kepala unit." | `403` |
| Pengganti bukan kepala unit atau DPJP | `POST` addendum | Penulis berhalangan tetapi pengguna tidak berwenang | "Koreksi atas catatan penulis yang berhalangan hanya dapat dilakukan kepala unit atau DPJP." | `403` |
| Addendum tidak dapat diubah | `PUT` addendum | Selalu | Endpoint tidak disediakan | — |

---

## 3. Penetapan penulis berhalangan

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---:|
| Alasan penetapan wajib diisi | `POST` delegation | `GrantReason` kosong | "Alasan penetapan wajib diisi." | `400` |
| Batas waktu wajib diisi | `POST` delegation | `ValidUntil` kosong | "Batas waktu penetapan wajib diisi. Penetapan tanpa batas waktu tidak diizinkan." | `400` |
| Batas waktu harus di masa depan | `POST` delegation | `ValidUntil` sudah lewat | "Batas waktu penetapan harus setelah hari ini." | `400` |
| Tidak boleh menetapkan diri sendiri | `POST` delegation | `OriginalAuthorUserId` sama dengan pengguna | "Anda tidak dapat menetapkan diri sendiri berhalangan." | `400` |
| Penetapan ganda ditolak | `POST` delegation | Sudah ada penetapan aktif untuk penulis yang sama | "Penulis ini sudah memiliki penetapan yang masih berlaku." | `409` |
| Akun nonaktif tidak perlu penetapan | `POST` delegation | Akun penulis sudah nonaktif | "Akun penulis sudah nonaktif, sehingga kewenangan pengganti terbuka otomatis tanpa penetapan." | `400` |

Aturan keempat perlu penjelasan. Menetapkan diri sendiri berhalangan tampak tidak masuk akal,
tetapi bila diizinkan akan menjadi cara memindahkan tanggung jawab atas catatan sendiri kepada
orang lain. Menutupnya sejak awal lebih murah daripada menjelaskannya kemudian.

---

## 4. Akses rekam medis

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---:|
| Alasan wajib bila tidak ada kunjungan aktif | Seluruh `GET` rekam medis | Pasien tidak punya kunjungan aktif dan `accessPurposeId` kosong | "Pasien ini sedang tidak dalam perawatan Anda. Pilih keperluan akses terlebih dahulu." | `400` |
| Alasan bebas wajib untuk keperluan tertentu | Seluruh `GET` rekam medis | Keperluan yang dipilih menuntut alasan bebas, tetapi `accessReason` kosong | "Keperluan yang Anda pilih mengharuskan penjelasan. Tuliskan alasannya." | `400` |
| Keperluan harus aktif | Seluruh `GET` rekam medis | `accessPurposeId` menunjuk keperluan tidak aktif | "Keperluan akses yang dipilih sudah tidak berlaku. Pilih yang lain." | `400` |
| `PrivateNote` selalu menuntut alasan | `GET` private-note | `accessPurposeId` kosong, apa pun keadaan kunjungan | "Membuka catatan pribadi selalu memerlukan keperluan akses." | `400` |
| Pasien hasil penggabungan | Seluruh `GET` rekam medis | `MergedToPatientId` terisi | "Nomor rekam medis ini sudah digabungkan. Buka nomor rekam medis penggantinya agar riwayat tampil utuh." | `409` |
| Jejak akses gagal dicatat | Seluruh `GET` rekam medis | Penulisan jejak gagal | "Berkas tidak dapat dibuka saat ini. Silakan coba lagi." | `503` |

Baris keempat adalah penerapan `RM-DEC-022` dan patut digarisbawahi: **`PrivateNote` menuntut
alasan bahkan untuk pasien yang sedang dirawat pengguna.** Ini berbeda dari isi rekam medis
lainnya. Alasannya, kolom itu ditulis dengan harapan bersifat pribadi, sehingga membukanya
selalu merupakan tindakan yang perlu dipertanggungjawabkan, bukan bagian dari pekerjaan
sehari-hari.

Baris kelima menutup keterbatasan nomor 6 pada arsitektur backend dengan cara yang jujur:
daripada menampilkan riwayat terpotong tanpa peringatan, sistem menolak dan menunjukkan nomor
penggantinya.

---

## 5. Perubahan perilaku pada endpoint berjalan

Tiga aturan berikut **bukan aditif** — keduanya mengubah cara endpoint yang sudah dipakai
bekerja. Karena itu dipisahkan dari bagian lain.

| Aturan | Berlaku pada | Perilaku baru | Dampak bagi klien yang sudah ada |
|---|---|---|---|
| `ProviderUserId` diabaikan pada permintaan ubah | `PUT` CPPT | Nilai dari klien tidak lagi ditetapkan ke entity | Klien tidak menerima galat, tetapi nilai yang dikirim tidak berpengaruh. **Wajib disebut pada catatan rilis** |
| `IsReadOnlyGenerated` diabaikan pada permintaan ubah | `PUT` CPPT | Sama seperti di atas | Sama seperti di atas |
| Pemeriksaan keutuhan sebelum mengubah | `PUT` CPPT | Menolak bila dokumen terkunci | Klien yang mencoba mengubah catatan terkunci menerima `400`. Sebelumnya berhasil |

Pilihan mengabaikan alih-alih menolak perlu dijelaskan. Menolak permintaan yang memuat
`ProviderUserId` akan memutus frontend yang sedang berjalan, sebab
`patient-integrated-progress-note.service.js` mengirim seluruh isi formulir. Mengabaikan nilai
menutup celah tanpa memutus siapa pun. Namun diam-diam mengabaikan kiriman klien juga bukan
praktik yang baik, sehingga perilaku ini **wajib** disebut pada catatan rilis dan
didokumentasikan di Swagger.

---

## 6. Aturan yang ditegakkan basis data

Sebagian aturan tidak memerlukan pemeriksaan di kode karena dijamin basis data. Dicantumkan
agar implementer tidak menulis pemeriksaan yang mubazir.

| Aturan | Ditegakkan oleh |
|---|---|
| Satu dokumen tepat satu baris keutuhan | Index unik `(DocumentKind, DocumentId)` |
| Urutan addendum tidak kembar | Index unik `(IntegrityId, Sequence)` |
| Kode keperluan akses tidak kembar | Index unik `(PurposeCode)` |
| Nomor rekam medis tidak kembar | Index unik yang sudah ada pada `MstPatient` |

Sebaliknya, tiga aturan berikut **tidak** dapat dijamin basis data dan wajib ada di service:

| Aturan | Alasan tidak dapat dijamin basis data |
|---|---|
| `DocumentId` benar-benar ada | Rujukan polimorfik tidak dapat memakai foreign key |
| `AuthorUserId` tidak pernah berubah | Basis data tidak mengenal kolom yang hanya boleh diisi sekali |
| `IsSubstituteAuthor` selaras dengan `DelegationId` | Aturan antar kolom yang bergantung pada penetapan yang masih berlaku |

---

## 7. Traceability

| Kelompok aturan | Decision | Acceptance test |
|---|---|---|
| Keutuhan dokumen | `RM-DEC-003`, `RM-DEC-019` | `AT-RM-01`, `AT-RM-02`, `AT-RM-10` |
| Addendum | `RM-DEC-004` | `AT-RM-04`, `AT-RM-05` |
| Penetapan berhalangan | `RM-DEC-020` | `AT-RM-14` |
| Akses rekam medis | `RM-DEC-005`, `RM-DEC-016` | `AT-RM-06`, `AT-RM-07` |
| `PrivateNote` | `RM-DEC-022` | `AT-RM-16` |
| Pasien hasil penggabungan | `RM-CAP-007` | `AT-RM-22` |
| Perubahan perilaku CPPT | `RM-DEC-019` | `AT-RM-19`, `AT-RM-20` |
