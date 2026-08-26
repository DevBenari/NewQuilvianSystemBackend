# Validation Matrix — Modul IGD

| Field | Nilai |
| --- | --- |
| Blueprint | `IGD-BP-001` revision `4` |
| `contract_version` | `0.2.0` |
| Status | `approved` — disetujui Product/Domain Owner 14 Agustus 2026 sesuai `IGD-DEC-046` |
| Commit diaudit | backend `e5331a0` |

Pesan ditulis dalam bahasa yang dipahami pengguna, bukan istilah teknis. Setiap aturan
disertai contoh agar tidak multitafsir.

---

## 1. Kunjungan IGD

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Unit pelayanan wajib | `POST /emergency-visits` | `ServiceUnitId` kosong | "Unit pelayanan IGD wajib dipilih." | 400 |
| Nomor kunjungan unik | `POST /emergency-visits` | Nomor sudah dipakai | "Nomor kunjungan sudah digunakan. Muat ulang halaman lalu coba lagi." | 409 |
| Satu encounter satu kunjungan | `POST /emergency-visits` | `EncounterId` sudah dipakai kunjungan lain | "Episode pelayanan ini sudah memiliki kunjungan IGD." | 409 |
| Pasien wajib kecuali tidak dikenal | `POST /emergency-visits` | `PatientId` kosong dan `IsUnknownPatient` bernilai salah | "Pasien wajib dipilih, atau tandai sebagai pasien tidak dikenal." | 400 |

> **Contoh aturan pasien tidak dikenal:** korban kecelakaan tanpa identitas tiba pukul 02.00.
> Petugas menandai `IsUnknownPatient`, sistem menerima kunjungan tanpa `PatientId`, dan
> identitas dilengkapi kemudian tanpa membuat kunjungan baru.

---

## 2. Triage

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Level triage wajib saat menyelesaikan | `PATCH /{id}/triage-status` ke `Completed` | `TriageLevelId` kosong | "Level triage wajib ditentukan sebelum penilaian diselesaikan." | 400 |
| Ringkasan ABCDE minimal satu | `PATCH /{id}/triage-status` ke `InProgress` | Keenam ringkasan kosong | "Isi minimal satu ringkasan pemeriksaan sebelum menyimpan." | 400 |
| Target waktu dari master | `POST /emergency-triages` | — | Tidak ada pesan; sistem menghitung sendiri | — |
| Retriage hanya atas penilaian selesai | `POST /{id}/retriage` | Status bukan `Completed` | "Hanya penilaian yang sudah selesai yang dapat dinilai ulang." | 409 |
| Retriage atas penilaian batal ditolak | `POST /{id}/retriage` | Status `Cancelled` | "Penilaian triage yang sudah dibatalkan tidak dapat dinilai ulang." | 409 |
| Hitam tidak otomatis | Seluruh endpoint triage | Aplikasi mencoba menetapkan kategori Hitam sendiri | "Kategori Hitam hanya dapat ditetapkan oleh klinisi berwenang." | 403 |

> **Contoh perhitungan target waktu:** pasien dinilai level 1 pada pukul 08.00.
> `MaxWaitingMinutes` untuk level 1 adalah 0 menit, sehingga `ResponseDueAt` menjadi 08.00 —
> segera, tanpa menunggu administrasi. Nilai ini disalin ke `MaxWaitingMinutesSnapshot` agar
> perubahan master di kemudian hari tidak mengubah riwayat.

> **Contoh target yang belum dikonfigurasi:** level 3 sampai 5 belum memiliki
> `MaxWaitingMinutes` karena SOP MMC belum tersedia. Sistem tidak boleh menebak angka;
> `ResponseDueAt` dibiarkan kosong dan pasien tidak dihitung melampaui batas.

---

## 3. Penyelesaian kunjungan

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Hanya dari status `Disposed` | `PATCH /{id}/complete` | Status bukan `Disposed` | "Kunjungan hanya dapat diselesaikan setelah keputusan tindak lanjut ditetapkan." | 409 |
| Transfer wajib tuntas | `PATCH /{id}/complete` | Ada transfer belum `Completed` atau `Rejected` | "Masih ada proses perpindahan yang belum selesai." | 409 |
| Observasi wajib tuntas | `PATCH /{id}/complete` | Ada observasi berstatus `Active` | "Masih ada observasi yang belum diselesaikan." | 409 |
| Billing bukan syarat | `PATCH /{id}/complete` | Billing masih `Pending` atau `Outstanding` | Tidak menolak; kunjungan tetap dapat diselesaikan | — |

> **Contoh billing bukan syarat:** pasien pulang pukul 03.00 saat bagian keuangan tidak
> bertugas. Kunjungan tetap dapat diselesaikan secara klinis dengan catatan serah terima
> billing, sehingga pasien tidak dianggap masih aktif di IGD.

---

## 4. Disposition dan transfer

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Unit tujuan wajib bila jenis mensyaratkan | `POST /emergency-dispositions` | Jenis memerlukan unit tujuan tetapi kosong | "Unit tujuan wajib dipilih untuk jenis tindak lanjut ini." | 400 |
| Fasilitas rujukan wajib bila jenis mensyaratkan | `POST /emergency-dispositions` | Jenis memerlukan rujukan tetapi kosong | "Fasilitas rujukan wajib diisi." | 400 |
| Alasan wajib saat membatalkan | Seluruh `PATCH` status ke `Cancelled` | Alasan kosong | "Alasan pembatalan wajib diisi." | 400 |
| Pengaju bukan penerima | `PATCH /emergency-transfers/{id}/transfer-status` ke `Accepted` | Penerima sama dengan pengaju | "Perpindahan harus diterima oleh petugas unit tujuan." | 403 |

---

## 5. Master data

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Kode master unik | Seluruh `POST` master | Kode sudah dipakai | "Kode sudah terdaftar. Gunakan kode lain." | 409 |
| Satu setting default | `POST` dan `PUT` `MstEmergencySetting` | Sudah ada baris `IsDefault` lain | "Sudah ada pengaturan IGD yang aktif sebagai default." | 409 |
| Level triage dalam rentang skala | `POST` `MstEmergencyTriageLevel` | `Level` di luar 1 sampai 5 | "Level triage harus bernilai 1 sampai 5." | 400 |
| Master terpakai tidak boleh dihapus | Seluruh `DELETE` master | Sudah dipakai transaksi | "Data ini sudah dipakai dan tidak dapat dihapus. Nonaktifkan saja." | 409 |

---

## 6. Aturan lintas endpoint

| Aturan | Penjelasan |
| --- | --- |
| Data sensitif tidak masuk log | Kolom bertanda sensitif pada kamus data dilarang masuk custom logger |
| UUID bukan label pengguna | Antarmuka menampilkan nama, bukan identifier teknis |
| Penekanan tombol ganda | Permintaan `POST` yang sama dalam rentang singkat hanya menghasilkan satu baris |
| Pemeriksaan hak akses per endpoint | Seluruh endpoint memakai `[AccessPermission]`; lihat permission matrix |
