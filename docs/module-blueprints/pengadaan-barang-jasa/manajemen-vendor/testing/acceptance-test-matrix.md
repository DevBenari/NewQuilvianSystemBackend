# Manajemen Vendor — Matriks Uji Penerimaan

| Field | Nilai |
| --- | --- |
| Kontrak | `PBJ-VD-TEST` · `last_changed_in: 1.0` · status `approved` (`PBJ-DEC-062`) |
| Masukan | `PBJ-VD-API`, `-STATE`, `-VAL`, `-PERM`, `-INT` 1.0 |
| Kebijakan bukti | Sama dengan `perencanaan-permintaan`: tanpa proyek test; bukti berupa build, QBE, skema EF/basis data, uji API manual, query `SELECT`, dan uji layar Playwright dengan aktor non-SuperAdmin |

Jenis bukti: **API**, **DB**, **FE**, **QBE** (lihat `perencanaan-permintaan/testing/acceptance-test-matrix.md`).

## 1. Profil, kategori, rekening

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-VD-01` | `PBJ-DEC-010`, `053` | Atur kategori untuk supplier yang belum punya profil | API, DB | Profil lahir satu baris; identitas `MstSupplier` tidak berubah; riwayat memuat status blokir awal (`PBJ-DEC-060`) |
| `AT-VD-02` | `PBJ-VAL-100` | Atur kategori nonaktif | API | 422 `PBJ-VAL-100` |
| `AT-VD-03` | `PBJ-DEC-015`, `PBJ-AC-009` | Baca detail vendor yang rekeningnya `1234567890` | API | `maskedBankAccount = "****7890"`; nomor utuh tidak ada di respons mana pun |
| `AT-VD-04` | `PBJ-DEC-051`, `PBJ-AC-017` | Aktor Unit Peminta membuka daftar vendor | API, FE | 403; menu 5 dan 10 tidak tampil |

## 2. Dokumen legalitas dan kelayakan

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-VD-10` | `PBJ-VAL-101` | Catat Izin PBF tanpa tanggal berlaku; lalu tanggal berlaku sebelum terbit | API | Dua kali 422 `PBJ-VAL-101` |
| `AT-VD-11` | `PBJ-VAL-102` | Verifikasi dokumen tanpa berkas | API | 422 `PBJ-VAL-102` |
| `AT-VD-12` | `PBJ-DEC-046`, `PBJ-AC-012` | Pengunggah memverifikasi dokumennya sendiri | API, FE | 403 `PBJ-VAL-103`; tombol Verifikasi tidak tampil bagi pengunggah |
| `AT-VD-13` | `PBJ-DEC-046` | Petugas lain memverifikasi; lalu dokumen lain ditolak dengan alasan | API, DB | `VERIFIED` dengan pemeriksa dan waktu; `REJECTED` dengan alasan; riwayat vendor bertambah |
| `AT-VD-14` | `PBJ-DEC-061`, `PBJ-AC-021` | Kategori Obat tanpa kewajiban aktif; vendor Obat tanpa dokumen | API | Kelayakan `isEligible = true` |
| `AT-VD-15` | `PBJ-DEC-045`, `061`, `PBJ-AC-011` | Aktifkan kewajiban Izin PBF untuk Obat; vendor tanpa izin terverifikasi; lalu izin kedaluwarsa (tanggal berlaku kemarin) | API | `isEligible = false` dengan alasan; `MstSupplier.IsBlacklisted` tidak berubah |
| `AT-VD-16` | `PBJ-ASM-002` | Izin berlaku 20 hari lagi | API, FE | Keadaan "Segera kedaluwarsa"; masuk hitungan ringkasan |
| `AT-VD-17` | `PBJ-DEC-052` | Vendor berkategori Alat Kesehatan saja, diperiksa untuk kategori Obat | API | `isEligible = false` "Vendor tidak melayani kategori ini" |

## 3. Status vendor dan usulan blokir

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-VD-20` | `PBJ-DEC-047` | Batasi tanpa alasan; lalu dengan alasan dan berakhir besok | API | 422 `PBJ-VAL-105`; lalu status Dibatasi, kelayakan `isRestricted = true` |
| `AT-VD-21` | `PBJ-DEC-047`, `PBJ-AC-013` | Tanggal berakhir Dibatasi sudah lewat (data uji) | API | Status tampil Aktif tanpa job |
| `AT-VD-22` | `PBJ-VAL-107` | Dua usulan blokir untuk vendor yang sama | API | Yang kedua 409 `PBJ-VAL-107` |
| `AT-VD-23` | `PBJ-VAL-091` | Ajukan usulan saat definisi `PBJ-VND-BLOKIR` tidak aktif | API | 409 `PBJ-VAL-091`; usulan tetap `DRAFT` |
| `AT-VD-24` | `PBJ-DEC-021` | Usulan blokir disetujui penyetuju lain | API, DB | Usulan `APPROVED`; `MstSupplier.IsBlacklisted = true`, `BlacklistReason` = alasan; satu riwayat vendor |
| `AT-VD-25` | `PBJ-DEC-021` (pengusul) | Pengusul mencoba menyetujui usulannya sendiri | API | Ditolak `Wfl`; usulan tetap `PENDING_APPROVAL` |
| `AT-VD-26` | `PBJ-DEC-060`, `PBJ-AC-020` | `PUT` Master Data Supplier dengan `isBlacklisted = false` untuk vendor yang diblokir | API, DB | 200, tetapi `IsBlacklisted` tetap `true` |
| `AT-VD-27` | `PBJ-DEC-054`, `PBJ-AC-018` | Vendor diblokir | API | Kelayakan `isEligible = false`; tidak ada data proses lain yang dibatalkan |
| `AT-VD-28` | `PBJ-VAL-106` | Usulan `UNBLOCK` untuk vendor tidak diblokir | API | 409 `PBJ-VAL-106` |
| `AT-VD-29` | Tarik | Pengusul menarik usulan `PENDING_APPROVAL` | API, DB | `CANCELLED`; instance `Wfl` `Withdrawn` |

## 4. Penilaian berkala

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-VD-30` | `PBJ-VAL-108` | Buat penilaian saat bobot kriteria kategori 80% | API | 422 `PBJ-VAL-108` |
| `AT-VD-31` | `PBJ-DEC-059`, `PBJ-AC-019` | Dua penilaian vendor × kategori × semester sama; lalu kategori lain | API | Kedua 409 `PBJ-VAL-109`; kategori lain 201 |
| `AT-VD-32` | `PBJ-DEC-050`, `PBJ-AC-016` | Bobot 40/40/20, nilai 4/5/3; lalu bobot master diubah | API, DB | `finalScore = 4.20`; tetap 4,20 sesudah master berubah |
| `AT-VD-33` | `PBJ-DEC-049`, `PBJ-AC-015` | Penilai memfinalkan sendiri; lalu Manajer memfinalkan | API | 403 `PBJ-VAL-111`; lalu `FINAL` terkunci (`PUT` 409 `PBJ-VAL-093`) |
| `AT-VD-34` | `PBJ-VAL-110` | Finalkan dengan satu kriteria kosong | API | 422 `PBJ-VAL-110` |
| `AT-VD-35` | `PBJ-DEC-049` | Revisi penilaian final lalu finalkan revisinya | API, DB | Versi lama `SUPERSEDED`, revisi `FINAL`, keduanya terbaca |
| `AT-VD-36` | `PBJ-DEC-048`, `PBJ-AC-014` | Rekomendasi Blokir difinalkan | DB | Status vendor tidak berubah; tidak ada usulan otomatis |

## 5. Fondasi dan konformitas

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `AT-VD-60` | `QBE-MOD-002` | Registry memuat submodule `Vendor` sebelum model pertama (`PBJ-OQ-037`) | QBE | Laporan konformitas lulus |
| `AT-VD-61` | Build | Backend dan frontend terbangun | Build | Sukses |
| `AT-VD-62` | Skema | Migration `AddProcurementVendorManagement` sesuai kamus data | DB | `information_schema`/`pg_indexes` cocok, termasuk unik parsial usulan terbuka dan penilaian |
| `AT-VD-63` | `PBJ-FE-004` | Setiap layar daftar vendor | FE | Satu baris filter; hero tanpa tombol kecuali detail/form; tombol filter primary |
| `AT-VD-64` | Data master | Seeder dijalankan dua kali | DB | Jenis dokumen, kriteria awal, definisi `PBJ-VND-*`, deret `UBV` tidak berlipat; kewajiban dokumen tetap kosong |
