# Validation Matrix — Modul Radiologi

| Field | Value |
|---|---|
| Contract version | `RAD-VAL-001` |
| Revision | `1` |
| Status | `draft` |
| Backend SHA | `64da911` |
| Input | `RAD-ARCH-BE-001`, `RAD-STATE-001` |

Pesan ditulis dalam bahasa yang dipahami pengguna, bukan istilah teknis. Pesan yang hanya
berbunyi "tidak valid" tidak memenuhi kontrak ini — pengguna harus tahu apa yang harus
diperbaiki.

---

## 1. Hasil Bacaan

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| Kesimpulan wajib diisi | Draf bacaan | `Impression` kosong | "Kesimpulan bacaan wajib diisi." | `400` |
| Panjang kesimpulan | Draf bacaan | Lebih dari 4.000 huruf | "Kesimpulan bacaan terlalu panjang, maksimal 4.000 huruf." | `400` |
| Panjang temuan | Draf bacaan | Lebih dari 8.000 huruf | "Uraian temuan terlalu panjang, maksimal 8.000 huruf." | `400` |
| Study harus layak | Buat draf | `IsUsable` bernilai `false` | "Citra pemeriksaan ini dinyatakan tidak layak dibaca, sehingga bacaan tidak dapat dibuat. Buat pemeriksaan ulang lebih dulu." | `422` |
| Mutu harus sudah dinilai | Buat draf | `IsUsable` masih `null` | "Mutu citra belum dinilai. Nilai mutu citra lebih dulu sebelum menulis bacaan." | `422` |
| Satu bacaan per study | Buat draf | Sudah ada bacaan untuk study itu | "Pemeriksaan ini sudah memiliki bacaan. Gunakan koreksi bila ingin mengubahnya." | `409` |
| Pengesahan sendiri | Sahkan | Penulis bukan radiolog dan mengesahkan sendiri | "Draf yang Anda tulis harus disahkan dokter radiolog." | `403` |
| Pengesah wajib radiolog | Sahkan | Pengesah bukan dokter radiolog | "Hanya dokter radiolog yang boleh mengesahkan hasil bacaan." | `403` |
| Pengesah wajib manusia | Sahkan | Pengesah adalah bantuan AI | "Hasil bacaan wajib disahkan dokter radiolog, tidak dapat disahkan sistem." | `403` |
| Ubah draf oleh orang lain | Ubah draf | Bukan penulis draf | "Hanya penulis draf yang dapat mengubahnya sebelum disahkan." | `403` |
| Draf beku setelah rilis | Ubah draf | Versi sudah `Released` atau `Superseded` | "Bacaan yang sudah dirilis tidak dapat diubah. Buat koreksi bila ada yang perlu diperbaiki." | `403` |
| Alasan koreksi wajib | Draf koreksi | `AmendmentReason` kosong | "Alasan koreksi wajib diisi." | `400` |
| Koreksi hanya setelah rilis | Draf koreksi | Bacaan belum pernah `Released` | "Bacaan ini belum pernah dirilis, sehingga belum ada yang perlu dikoreksi." | `409` |
| Rilis setelah disahkan | Rilis | Belum berstatus `Validated` | "Bacaan harus disahkan lebih dulu sebelum dirilis." | `409` |

> **Contoh pesan yang benar dan yang salah.**
>
> Benar: "Draf yang Anda tulis harus disahkan dokter radiolog."
> Salah: "Validation failed: AuthorRoleSnapshot != Radiologist."
>
> Pesan pertama memberi tahu petugas apa yang harus dilakukan. Pesan kedua hanya berguna bagi
> programmer.

---

## 2. Aturan Keselamatan

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| Alat wajib dipilih | Susun draf | `ModalityId` kosong | "Alat pencitraan wajib dipilih." | `400` |
| Butir wajib dipilih | Susun draf | `SafetyRequirementId` kosong | "Butir keselamatan wajib dipilih." | `400` |
| Alat harus aktif | Susun draf | Alat berstatus tidak aktif | "Alat pencitraan yang dipilih sudah tidak aktif." | `422` |
| Butir harus aktif | Susun draf | Butir berstatus tidak aktif | "Butir keselamatan yang dipilih sudah tidak aktif." | `422` |
| Masa berlaku | Susun draf | `EffectiveTo` lebih awal dari `EffectiveFrom` | "Tanggal akhir berlaku tidak boleh lebih awal dari tanggal mulai berlaku." | `400` |
| Tidak boleh bertabrakan | Sahkan | Sudah ada aturan `Active` untuk kombinasi alat, pemeriksaan, dan butir yang sama | "Sudah ada aturan aktif untuk alat, pemeriksaan, dan butir keselamatan yang sama. Nonaktifkan aturan lama lebih dulu." | `409` |
| Pengesah berwenang | Sahkan | Bukan penanggung jawab klinis | "Hanya penanggung jawab klinis yang boleh mengesahkan aturan keselamatan." | `403` |
| Alasan penolakan wajib | Tolak | `RejectionReason` kosong | "Alasan penolakan wajib diisi." | `400` |
| Aturan aktif tidak dapat diubah | Ubah | `RuleStatus` bernilai `Active` | "Aturan yang sedang berlaku tidak dapat diubah. Susun draf baru lalu ajukan pengesahan." | `403` |

---

## 3. Data Induk Alat dan Butir Keselamatan

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| Kode alat wajib | Tambah atau ubah alat | `ModalityCode` kosong | "Kode alat wajib diisi." | `400` |
| Kode alat unik | Tambah atau ubah alat | Kode sudah dipakai alat lain | "Kode alat sudah dipakai. Gunakan kode lain." | `409` |
| Nama alat wajib | Tambah atau ubah alat | `ModalityName` kosong | "Nama alat wajib diisi." | `400` |
| Kode butir wajib | Tambah atau ubah butir | `RequirementCode` kosong | "Kode butir keselamatan wajib diisi." | `400` |
| Kode butir unik | Tambah atau ubah butir | Kode sudah dipakai | "Kode butir keselamatan sudah dipakai." | `409` |
| Alat masih dipakai | Nonaktifkan alat | Masih ada aturan `Active` yang memakai alat itu | "Alat ini masih dipakai aturan keselamatan yang berlaku. Nonaktifkan aturannya lebih dulu." | `409` |
| Butir masih dipakai | Nonaktifkan butir | Masih ada aturan `Active` yang memakai butir itu | "Butir ini masih dipakai aturan keselamatan yang berlaku. Nonaktifkan aturannya lebih dulu." | `409` |

---

## 4. Validasi yang Sudah Berjalan

Berlaku pada endpoint yang sudah tersedia. Direkam di sini supaya kontrak lengkap.

| Aturan | Berlaku pada | Pesan bagi pengguna | Kode |
|---|---|---|---|
| Aturan keselamatan belum ada | Nyatakan lolos keselamatan | "Aturan keselamatan untuk modalitas ini belum ditetapkan, sehingga acquisition tidak dapat dijalankan. Hubungi admin Radiologi untuk menetapkan aturannya lebih dulu." | `409` |
| Butir wajib belum dijawab | Nyatakan lolos keselamatan | "Gerbang keselamatan wajib belum dijawab: `<kode butir>`." | `409` |
| Butir wajib dinyatakan tidak aman | Nyatakan lolos keselamatan | "Gerbang keselamatan wajib dinyatakan tidak aman: `<kode butir>`." | `409` |
| Mutu dinilai sebelum citra diambil | Nilai mutu | "Kualitas hanya dapat dinilai untuk study berstatus Acquired." | `409` |
| Identitas pelaku tidak diketahui | Semua tindakan | "Identitas petugas tidak dapat ditentukan dari sesi yang sedang berjalan. Tindakan radiologi tidak dijalankan." | `401` |

---

## 5. Penanganan Dua Orang Bekerja Bersamaan

Seluruh perubahan status memakai token konkurensi. Bila dua petugas mengubah data yang sama
pada waktu hampir bersamaan, satu berhasil dan satu ditolak.

| Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|
| Token konkurensi tidak cocok | "Data ini baru saja diubah petugas lain. Muat ulang halaman lalu ulangi tindakan Anda." | `409` |

> **Contoh.** dr. Sinta dan dr. Bagas sama-sama membuka draf bacaan Tn. B dan menekan Sahkan
> pada detik yang hampir sama. Satu pengesahan berhasil. Yang kedua ditolak dengan pesan di
> atas, dan **tidak ada** dua baris pengesahan yang tersimpan.

---

## 6. Aturan Umum Penulisan Pesan

| Aturan | Contoh benar | Contoh salah |
|---|---|---|
| Sebut apa yang harus diperbaiki | "Kesimpulan bacaan wajib diisi." | "Field required." |
| Sebut butir yang menahan, bukan sekadar gagal | "Gerbang keselamatan wajib belum dijawab: SKRINING-HAMIL." | "Safety gate failed." |
| Jangan menyebut nama kolom database | "Alasan koreksi wajib diisi." | "AmendmentReason cannot be null." |
| Jangan memuat data pasien dalam pesan | "Pemeriksaan ini sudah memiliki bacaan." | "Bacaan untuk pasien Budi Santoso sudah ada." |

Alasan aturan terakhir: pesan kesalahan sering ikut tercatat di log dan tampil di layar yang
mungkin dilihat orang lain. Data pasien tidak boleh ikut di dalamnya.
