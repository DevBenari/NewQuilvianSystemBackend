# State Transition Matrix — Modul Rekam Medis

| Field | Value |
|---|---|
| Blueprint ID | `RM-BP-001` |
| Contract version | `0.1.0` |
| Status | `draft` |
| Owner | Clinical governance authority: `OPEN` |
| `approved_by` / `approved_at` | — / — |
| Input revisions | `00-interview-decisions.md` revision `2` |
| Compatibility impact | **Aditif.** Status keutuhan adalah status baru yang berdampingan dengan status alur kerja yang sudah ada. Tidak ada enum berjalan yang berubah |

> **PERINGATAN DASAR DESAIN.** Disusun di atas keputusan berstatus `draft`. Lihat `RM-DEC-025`.

---

## 1. Dua status yang berjalan berdampingan

Sebelum membaca tabel, satu hal harus jelas: sebuah dokumen klinis kini punya **dua status yang
berbeda maknanya**.

| Status | Menjawab pertanyaan | Contoh nilai | Pemilik |
|---|---|---|---|
| Status alur kerja | Sudah selesai dikerjakan atau belum? | `Draft`, `InProgress`, `Completed`, `Cancelled` | `ClinicalManagement` |
| **Status keutuhan** | Masih boleh diubah atau tidak? | `Draft`, `Signed`, `LockedUnsigned`, `Cancelled` | `MedicalRecordManagement` |

Keduanya kebetulan sama-sama punya nilai bernama `Draft` dan `Cancelled`, dan itu berpotensi
membingungkan. Karena itu `RM-FE-008` mewajibkan layar membedakan keduanya secara jelas, tidak
boleh menampilkannya sebagai satu penanda tunggal.

Dokumen ini hanya membahas **status keutuhan**. Status alur kerja tidak diubah sama sekali oleh
modul ini.

---

## 2. Transisi yang sah — status keutuhan dokumen

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Dokumen klinis dibuat | `Draft` | Sistem | Dipicu otomatis saat dokumen disimpan pertama kali | Dokumen tanpa baris keutuhan luput dari seluruh aturan penguncian; pembuatan dokumen dibatalkan |
| `Draft` | Menandatangani | `Signed` | **Hanya penulis** (`AuthorUserId`) | Isi dokumen tidak kosong | `403` — "Anda bukan penulis catatan ini" |
| `Draft` | Kunjungan ditutup | `LockedUnsigned` | Sistem | Dipicu perpindahan kunjungan menuju `Completed` | Kunjungan tertutup dengan dokumen masih terbuka; penutupan kunjungan dibatalkan |
| `Draft` | Membatalkan dokumen | `Cancelled` | Penulis, atau petugas berwenang sesuai izin controller klinis | Alasan pembatalan terisi | `400` — alasan wajib diisi |
| `Signed` | Menambah addendum | **tetap** `Signed` | Penulis, atau pengganti yang berwenang | Lihat bagian 4 | Lihat bagian 4 |
| `LockedUnsigned` | Menambah addendum | **tetap** `LockedUnsigned` | Penulis, atau pengganti yang berwenang | Lihat bagian 4 | Lihat bagian 4 |

Baris kelima dan keenam penting dibaca teliti: **menambah addendum tidak mengubah status.**
Dokumen yang sudah `Signed` tetap `Signed` setelah dikoreksi sepuluh kali. Addendum adalah
lampiran, bukan perubahan keadaan. Bila addendum mengubah status, sistem akan kehilangan
kemampuan membedakan dokumen yang pernah dikoreksi dari yang belum.

---

## 3. Transisi yang TIDAK sah

Bagian ini sama pentingnya dengan bagian sebelumnya, dan sering dilupakan.

| Dari status | Ke status | Mengapa dilarang | Apa yang terjadi bila dicoba |
|---|---|---|---|
| `Signed` | `Draft` | Membuka kembali dokumen yang sudah disahkan berarti isinya dapat diubah tanpa jejak. Ini melanggar `RM-DEC-003` | `400` — "Catatan yang sudah ditandatangani tidak dapat dibuka kembali" |
| `LockedUnsigned` | `Draft` | Sama seperti di atas. Turunan langsung `RM-DEC-006` yang menyatakan kunjungan tidak pernah dibuka kembali | `400` — pesan serupa |
| `LockedUnsigned` | `Signed` | Menandatangani setelah terkunci berarti menyatakan dokumen final pada waktu yang sudah lewat. Tanda tangan yang mundur ke belakang bukan tanda tangan | `400` — "Catatan sudah terkunci. Gunakan addendum bila perlu melengkapi" |
| `Cancelled` | status apa pun | Dokumen yang dibatalkan tetap tersimpan dan terbaca sebagai dokumen yang dibatalkan. Ia tidak pernah hidup kembali | `400` — "Catatan yang sudah dibatalkan tidak dapat diubah" |
| `Signed` | `Cancelled` | Membatalkan dokumen yang sudah disahkan akan menghapus jejak pertanggungjawaban. Bila isinya keliru, koreksinya lewat addendum | `400` — "Gunakan addendum untuk membetulkan catatan yang sudah ditandatangani" |
| Status apa pun | dihapus dari tabel | Baris keutuhan tidak pernah dihapus | Permintaan hapus tidak disediakan |

Baris kelima sering diperdebatkan, jadi alasannya perlu dinyatakan. Membatalkan catatan yang
sudah ditandatangani terasa masuk akal ketika catatannya benar-benar salah, misalnya tertulis
pada pasien yang keliru. Namun pembatalan menghapus makna tanda tangan yang sudah diberikan.
Jalan yang benar adalah addendum yang menyatakan catatan tersebut keliru dan menjelaskan
sebabnya. Pembaca kemudian melihat keduanya: catatan aslinya, dan pernyataan bahwa itu keliru.

Untuk kasus salah pasien yang berat, jenis dokumen yang sudah punya nilai `EnteredInError` —
yaitu dokumen klinis dan lampiran — dapat memakainya. Catatan naratif seperti CPPT belum
punya, dan itu tercatat sebagai bahan Amendment Pass, bukan diputuskan sendiri di sini.

---

## 4. Kewenangan membuat addendum

Ini penerapan `RM-DEC-004` dan `RM-DEC-020`, dan bentuknya berupa pemeriksaan bertingkat.

```text
Apakah pengguna adalah AuthorUserId dokumen ini?
  ya    -> BOLEH, sebagai penulis asli
  tidak -> Apakah akun penulis asli nonaktif?
             ya    -> Apakah pengguna kepala unit atau DPJP?
                        ya    -> BOLEH, sebagai pengganti (Trigger = InactiveAccount)
                        tidak -> DITOLAK
             tidak -> Apakah ada penetapan berhalangan yang masih berlaku?
                        ya    -> Apakah pengguna kepala unit atau DPJP?
                                   ya    -> BOLEH, sebagai pengganti (Trigger = UnitHeadGrant)
                                   tidak -> DITOLAK
                        tidak -> DITOLAK
```

| Keadaan | Hasil | Pesan bila ditolak |
|---|---|---|
| Pengguna adalah penulis asli | Boleh | — |
| Akun penulis nonaktif, pengguna kepala unit atau DPJP | Boleh sebagai pengganti | — |
| Ada penetapan berlaku, pengguna kepala unit atau DPJP | Boleh sebagai pengganti | — |
| Penetapan sudah lewat `ValidUntil` | Ditolak | "Penetapan kewenangan pengganti sudah berakhir. Hubungi kepala unit" |
| Pengguna bukan penulis dan bukan kepala unit | Ditolak | "Hanya penulis catatan yang dapat menambahkan koreksi" |
| Dokumen masih `Draft` | Ditolak | "Catatan ini belum terkunci. Perbaiki langsung pada catatannya" |

Baris terakhir menutup kekeliruan yang mudah terjadi: addendum bukan cara mengoreksi catatan
yang masih bisa diedit. Selama dokumen masih `Draft`, penulis membetulkannya langsung.

---

## 5. Transisi status jejak akses

Jejak akses punya siklus yang jauh lebih sederhana, karena hampir seluruhnya tidak dapat diubah.

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Rekam medis dibuka | Baris jejak tercipta | Sistem | Selalu, tanpa kecuali | Bila gagal, isi rekam medis tidak dikembalikan (`503`) |
| Belum ditinjau | Menandai sudah ditinjau | Sudah ditinjau | Petugas rekam medis | Hanya untuk baris `IsFlaggedForReview` bernilai benar | `400` — "Akses ini tidak memerlukan tinjauan" |
| Sudah ditinjau | Menandai ulang | — | — | **Dilarang** | `400` — "Akses ini sudah ditinjau" |
| Status apa pun | Mengubah isi jejak | — | — | **Dilarang tanpa kecuali** | Endpoint tidak disediakan |
| Status apa pun | Menghapus | — | — | **Dilarang tanpa kecuali** | Endpoint tidak disediakan |

---

## 6. Transisi status kunjungan yang memicu penguncian

Modul ini tidak memiliki status kunjungan, tetapi bergantung padanya. Yang dipantau hanya satu
peristiwa.

| Peristiwa | Yang dilakukan modul rekam medis |
|---|---|
| Kunjungan berpindah **menuju** `Completed` | Mengunci seluruh dokumen `Draft` pada kunjungan itu menjadi `LockedUnsigned` |
| Kunjungan berpindah menuju `Cancelled` | Tidak melakukan apa pun. Dokumen tetap pada statusnya. Pembatalan kunjungan tidak sama dengan penyelesaian pelayanan |
| Perpindahan status kunjungan lainnya | Tidak melakukan apa pun |

Catatan yang harus disertakan: endpoint perubahan status kunjungan **tidak memvalidasi
perpindahan** (`RM-CAP-019`). Status dapat melompat dari `Draft` langsung ke `Completed` tanpa
melewati tahap apa pun. Karena itu penguncian dipicu oleh **tujuan** perpindahan, bukan oleh
urutan yang benar. Bila kelak `RM-CAP-019` diperbaiki, pemicu ini tidak perlu berubah.

---

## 7. Traceability

| Transisi | Decision | Acceptance test |
|---|---|---|
| `Draft` menuju `Signed` | `RM-DEC-003`, `RM-DEC-021` | `AT-RM-02` |
| `Draft` menuju `LockedUnsigned` | `RM-DEC-003` lapis kedua | `AT-RM-03` |
| Larangan kembali ke `Draft` | `RM-DEC-003`, `RM-DEC-006` | `AT-RM-10`, `AT-RM-11` |
| Kewenangan addendum | `RM-DEC-004`, `RM-DEC-020` | `AT-RM-04`, `AT-RM-05`, `AT-RM-14` |
| Addendum tidak mengubah status | `RM-DEC-004` | `AT-RM-17` |
| Jejak akses tidak dapat diubah | `RM-DEC-015` | `AT-RM-08` |
| Status awal hasil pengisian data lama | `RM-DEC-014` | `AT-RM-21` |
