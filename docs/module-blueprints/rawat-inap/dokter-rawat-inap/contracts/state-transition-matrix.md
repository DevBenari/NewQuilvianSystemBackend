# State Transition Matrix — Sub-modul `dokter-rawat-inap` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `dokter-rawat-inap` — bentuk `COMPOSITE`, `RWI-DEC-082` |
| Contract version | `0.3.0` |
| `last_changed_in` | `0.3.0` |
| Status | `draft` — belum disetujui manusia |
| Owner | Product/Domain: **Muhammad Hamzah** (`RWI-DEC-061`); pemilik tabel: `ClinicalManagement`, `PharmacyManagement`, `LaboratoryManagement`, `RadiologyManagement`, `MedicalRecordManagement` |
| `approved_by` / `approved_at` | — belum |
| `input_revision` | `02-backend-architecture.md` `0.2`; arsitektur domain `0.2` |
| `input_hash` | Arsitektur domain SHA-256 `226c6ef1e4bfec544c366b265fe1e4530e80c510da33c1a9eaf2e62161d0b717` |
| Compatibility impact | `0.3.0`: perpindahan ke `Completed` kini **sekaligus** mendaftarkan dokumen ke mesin keutuhan dan menguncinya sebagai tertanda tangan. Nol nilai status baru. Sebelumnya `0.2.0` mencabut status `Amended` dan melahirkan mesin event visite |
| Tanggal | 2 September 2026 |

---

## 0. Mesin status yang dibahas, dan yang bukan milik sub-modul ini

| Mesin | Milik | Dibahas di sini |
| --- | --- | --- |
| Catatan dokter dan SOAP | `ClinicalManagement` | Ya |
| Kajian medis | `ClinicalManagement` | Ya |
| Verifikasi CPPT | `ClinicalManagement` | Ya |
| Tindakan dokter | `ClinicalManagement` | Ya |
| **Event visite** | `ClinicalManagement` | Ya — mesin baru |
| Integritas dan koreksi dokumen | `MedicalRecordManagement` | Ya — dipakai apa adanya |
| Pemenuhan resep | `PharmacyManagement` | **Dibaca saja** — `RUL-DOK-01` |
| Pesanan laboratorium dan radiologi | `LaboratoryManagement`, `RadiologyManagement` | **Dibaca saja** — `RUL-DOK-02` |
| Status episode | `episode-rawat-inap` | **Tidak** — `RWI-DEC-009` mengunci lima nilainya |

### 0.1 Yang berubah dari `0.1.0`

| Perubahan | Alasan |
| --- | --- |
| Status `Amended` **dicabut** dari mesin catatan, kajian, dan tindakan | Koreksi dipegang mesin addendum `MedicalRecordManagement`; menambah status keenam membuat dua sumber jawaban |
| Nilai status tindakan diselaraskan dengan enum yang benar-benar ada | Enum di source berbunyi `Planned`, `Ordered`, `InProgress`, `Completed`, `Cancelled` — bukan `Ordered`/`Performed`/`Amended` seperti tertulis pada `0.1.0` |
| Mesin **event visite** ditambahkan | `RWI-DEC-084`, `RWI-DEC-085` |
| Mesin "status pengiriman tagihan" **dicabut** | Sudah dijawab hasil penerbitan fakta klinis beserta `IsBillingGenerated` |

---

## 1. Catatan dokter dan SOAP

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| *(belum ada)* | Membuat catatan | `Draft` | Dokter yang berwenang atas pasien itu | Konteks klinis sah; episode `Admitted` atau `DischargePending` | `422` |
| `Draft` | Mengisi S/O/A/P | `InProgress` | Penulisnya | — | — |
| `Draft` / `InProgress` | Menyelesaikan | `Completed` | Penulisnya | Sekurang-kurangnya satu bagian terisi | `400` |
| `Completed` | — | — | — | **Perpindahan ini sekaligus mendaftarkan catatan ke mesin keutuhan sebagai tertanda tangan, dalam transaksi yang sama.** Bila pendaftaran gagal, perpindahan ikut batal | `RWI-DEC-086`, `RWI-DEC-087` |
| `Draft` / `InProgress` | Membatalkan | `Cancelled` | Penulisnya atau supervisor klinis | Alasan wajib | `400` |
| `Completed` | **Mengoreksi** | Tetap `Completed` | Penulisnya, atau penulis pengganti yang punya pendelegasian sah | Alasan koreksi wajib; addendum bernomor urut tersimpan | `403` bagi yang tidak berwenang |

### 1.1 Transisi yang tidak sah

| Dari | Ke | Kenapa dilarang |
| --- | --- | --- |
| `Completed` | `Draft` / `InProgress` | Membuka kembali catatan final menghapus jejak bahwa ia pernah final |
| `Cancelled` | Apa pun | Status terminal |
| Apa pun | Terhapus | Penghapusan bersifat penandaan; hard delete dilarang |
| Apa pun | Status baru saat episode `Closed` | `INV-DOK-03`. **Kecuali** penambahan addendum, yang justru tidak mengaktifkan kembali episode |

> **Baris terakhir adalah pembeda halus yang penting.** Episode tertutup menolak catatan **baru**,
> tetapi menerima **koreksi** catatan lama. Menyamakan keduanya membuat kesalahan tulis pada
> episode yang sudah ditutup tidak pernah dapat dibetulkan.

---

## 2. Kajian medis

Memakai mesin status yang sama dengan pengkajian keperawatan — `Draft`, `InProgress`, `Completed`,
`Cancelled` — karena tabelnya sama. Pembedanya jenis kajian dan siapa yang boleh menulisnya.

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| *(belum ada)* | Membuat kajian medis | `Draft` | **Dokter saja** | Konteks klinis sah; belum ada kajian medis berlaku pada episode itu |
| `Draft` | Melanjutkan mengisi | `InProgress` | Penulisnya | — |
| `InProgress` | Menyelesaikan | `Completed` | Penulisnya | Isian minimum terpenuhi |
| `Draft` / `InProgress` | Membatalkan | `Cancelled` | Penulisnya atau supervisor | Alasan wajib |
| `Completed` | Mengoreksi | Tetap `Completed` | Penulisnya atau pengganti sah | Addendum, alasan wajib |

| Transisi tidak sah | Kenapa |
| --- | --- |
| Kajian medis ditimpa catatan SOAP harian | Keduanya tabel berbeda; penimpaan **tidak mungkin terjadi secara struktur** |
| Perawat membuat kajian medis | `VAL-DOK-05` |

---

## 3. Verifikasi CPPT

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| `NotRequired` | — | — | — | Kebijakan verifikasi tidak aktif | — |
| `Pending` | Memverifikasi | `Verified` | **DPJP yang aktif pada saat verifikasi** | Verifikator bukan penulis aslinya | `403` |
| `Pending` | Lewat batas waktu | `Overdue` | *(sistem)* | Kebijakan aktif punya batas | — |
| `Overdue` | Memverifikasi | `Verified` | DPJP aktif | — | — |
| `Verified` | Catatan dikoreksi lewat addendum | `Pending` | Penulis asli atau pengganti sah | Alasan wajib; verifikasi ulang dibutuhkan | `400` bila alasan kosong |

### 3.1 Aturan yang tidak boleh dilanggar

| Aturan | Sumbernya |
| --- | --- |
| **Verifikasi tidak pernah mengubah penulis asli.** Penulis tetap; verifikator disimpan terpisah | `INV-DOK-11`, `AC-CAP021-03` |
| Verifikator adalah DPJP yang aktif **saat verifikasi**, bukan yang aktif saat catatan ditulis | `RWI-RULE-030` |
| Bawaan `NotRequired`, bukan `Pending` | Menyalakan kewajiban sebagai bawaan membuat daftar pantau penuh pada rumah sakit yang tidak mewajibkannya |
| `Overdue` **tidak menahan** penulisan catatan berikutnya | Verifikasi adalah pemantauan mutu, bukan gerbang pelayanan — `RWI-RULE-021` |

---

## 4. Tindakan dokter

Nilai status diambil apa adanya dari enum yang sudah ada di source.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| *(belum ada)* | Merencanakan tindakan | `Planned` | Dokter berwenang | Konteks klinis sah |
| *(belum ada)* | Mencatat tindakan yang langsung dikerjakan | `InProgress` lalu `Completed` | Dokter pelaksana | Konteks sah; waktu tidak di masa depan |
| `Planned` | Menjadwalkan atau menyetujui | `Ordered` | Dokter berwenang | — |
| `Planned` / `Ordered` | Melaksanakan | `InProgress` | Dokter pelaksana | — |
| `InProgress` | Menyelesaikan | `Completed` | Dokter pelaksana | Waktu dan pelaksana terisi |
| `Planned` / `Ordered` / `InProgress` | Membatalkan | `Cancelled` | Dokter atau supervisor | Alasan wajib |
| `Completed` | Mengoreksi | Tetap `Completed` | Pelaksananya | Addendum, alasan wajib |

### 4.1 Penerbitan fakta klinis ke Billing — **bukan status tindakan**

| Keadaan penerbitan | Artinya | Yang terjadi pada catatan klinis |
| --- | --- | --- |
| Diterbitkan | Fakta diterima Billing | Tidak berubah |
| Diputar ulang | Fakta identik sudah pernah diterbitkan; hasil yang sama dikembalikan | Tidak berubah |
| Ditekan tanpa tagihan sebelumnya | Pembatalan klinis terjadi sebelum tagihan terbentuk | Tidak berubah |
| Perlu rekonsiliasi | Keadaan sebelumnya tidak diketahui | Tidak berubah; **wajib direkonsiliasi sebelum koreksi finansial** |
| Ditolak Billing atau hasil tidak diketahui | Pengiriman gagal | **Tetap `Completed`** — `INV-DOK-09` |

> Keadaan di atas adalah keadaan **pengiriman**, bukan status tindakan. Menyimpannya sebagai status
> tindakan akan membuat kegagalan sistem keuangan terlihat seperti tindakan medis yang batal.

---

## 5. Event visite dokter — mesin baru

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| *(belum ada)* | Mencatat visite | `Recorded` | Dokter yang berwenang atas pasien itu | Konteks klinis sah; kunci permintaan terisi; waktu tidak di masa depan | `400` atau `422` |
| *(belum ada)* | Mengirim ulang dengan kunci yang sama | **Tetap event yang sama** | Sama | Kunci permintaan identik | `200`, bukan galat |
| `Recorded` | Menautkan dokumen | `Recorded` | Pemilik event | Dokumen milik episode yang sama | `400` bila dokumen milik episode lain |
| `Recorded` | **Membatalkan karena salah catat** | `Cancelled` | Pemilik event atau supervisor | **Alasan wajib** | `400` bila alasan kosong |
| `Cancelled` | — | — | — | **Status terminal** | `409` bila dibatalkan dua kali |

### 5.1 Transisi yang tidak sah, dan kenapa

| Dari | Ke | Kenapa dilarang |
| --- | --- | --- |
| `Recorded` | `Recorded` dengan waktu atau peran berbeda | **Penyuntingan di tempat dilarang.** Event menyatakan fakta kedatangan; mengubah waktunya berarti fakta yang berbeda — `RWI-DEC-085` |
| `Cancelled` | `Recorded` | Event yang dibatalkan tidak dihidupkan kembali. Yang benar adalah mencatat event baru |
| Apa pun | Terhapus | `INV-DOK-08`: event yang dibatalkan **tetap tersimpan** dan tetap terbaca auditor |

### 5.2 Cara koreksi yang benar

1. Batalkan event yang salah beserta alasannya. Ia tetap tampil pada riwayat dengan penanda batal.
2. Catat event baru dengan kunci permintaan **baru**, menunjuk event yang digantikannya.
3. Hitungan visite hanya menghitung event berstatus `Recorded`.

> **Contoh.** dr. Andi visite pukul 07.40 tetapi mengisi 17.40. Ia membatalkan dengan alasan
> "salah ketik jam", lalu mencatat event baru pukul 07.40. Riwayat Tn. Budi menampilkan **dua
> baris**: satu batal beserta alasannya, satu berlaku. Hitungan visite hari itu tetap **1**.

### 5.3 Hitungan, dengan angka

| Keadaan pada 12 September 2026 | Baris tersimpan | Hitungan | Dasar |
| --- | ---: | ---: | --- |
| dr. Andi visite pukul 07.40 lalu kembali pukul 16.10 | 2 | **2** | `RWI-AC-154` |
| Tombol Simpan tertekan dua kali dengan kunci sama | 1 | **1** | `RWI-AC-152` |
| dr. Andi dan dr. Sinta masing-masing sekali | 2 | **2** | `RWI-RULE-017` |
| Tiga SOAP ditulis tanpa satu pun event visite | 0 | **0** | `RWI-AC-151` |
| Satu event salah catat, dibatalkan, lalu dicatat ulang | 2 | **1** | `INV-DOK-08` |
| Billing menggabungkan dua event menjadi satu tagihan harian | 2 | **2** pada riwayat klinis | `RWI-AC-156` |

---

## 6. Integritas dan koreksi dokumen — milik `MedicalRecordManagement`

Nilai status diambil apa adanya dari enum yang sudah ada.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| `Draft` | Penulis menandatangani | `Signed` | Penulis dokumen | Dokumen sudah final |
| `Draft` | Kunjungan ditutup tanpa tanda tangan | `LockedUnsigned` | *(sistem)* | Pemicu penguncian tercatat |
| `Signed` / `LockedUnsigned` | Menambah addendum | Tidak berubah | Penulis asli | **Alasan koreksi wajib** |
| `Signed` / `LockedUnsigned` | Menambah addendum **atas nama penulis** | Tidak berubah | **DPJP aktif episode itu**, bila akun penulis nonaktif atau ada penetapan berhalangan yang berlaku | Alasan koreksi wajib; penulis asli tetap tercantum sebagai penulis catatan |
| `Draft` | Menambah addendum | **Ditolak** | — | Dokumen belum terkunci; perbaiki langsung pada catatannya |
| `Draft` / `Signed` | Membatalkan dokumen | `Cancelled` | Sesuai aturan modul pemiliknya | Alasan wajib |

### 6.1 Tiga tingkat kewenangan koreksi

| Tingkat | Keadaan | Siapa yang boleh | Perlu penetapan? |
| --- | --- | --- | --- |
| 1 | Penulis masih aktif | **Penulis asli** | Tidak |
| 2 | Akun penulis sudah nonaktif | **DPJP aktif episode itu** | **Tidak** — disimpulkan sistem |
| 3 | Penulis berhalangan sementara | **DPJP aktif episode itu** | **Ya** — penetapan kepala unit, wajib berbatas waktu |

> **Pertanyaan `0.2.0` sudah terjawab.** Dokumen terkunci bukan hanya menerima koreksi — ia
> **satu-satunya** keadaan yang menerimanya. Dokumen berstatus konsep justru ditolak, dengan arahan
> memperbaiki langsung pada catatannya.
>
> **Satu batas yang bukan milik mesin ini.** Penetapan berhalangan menyatakan "dokter ini
> berhalangan" tanpa menyebut penggantinya, sehingga pembatasan pada tingkat 2 dan 3 bahwa hanya
> **DPJP aktif episode itu** yang boleh mengoreksi dijaga di sisi Rawat Inap, bukan di sini —
> `INV-DOK-13`.

---

## 7. Status milik modul lain yang hanya dibaca

| Mesin | Nilai yang dibaca | Yang **tidak boleh** dilakukan sub-modul ini |
| --- | --- | --- |
| Pemenuhan resep | Menunggu finalisasi klinis dan seterusnya, milik `PharmacyManagement` | **Menulis** status apa pun — `RUL-DOK-01` |
| Pesanan laboratorium | `Requested`, ditahan, dikerjakan, selesai, dibatalkan | Menulis status maupun hasil — `RUL-DOK-02` |
| Pesanan radiologi | `Requested`, diterima, dijadwalkan, dikerjakan, selesai | Sama |
| Status episode | `Draft`, `Admitted`, `DischargePending`, `Closed`, `Cancelled` | Mengubahnya. Sub-modul ini hanya membacanya sebagai syarat |

Menampilkannya di layar adalah membaca. Mengubahnya adalah pelanggaran batas.
