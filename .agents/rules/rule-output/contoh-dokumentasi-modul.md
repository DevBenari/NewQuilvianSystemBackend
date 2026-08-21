# Contoh Penerapan Aturan Output Dokumentasi

Dokumen ini adalah **contoh bentuk**, bukan dokumentasi resmi modul. Gunakan sebagai acuan
susunan dan gaya penulisan ketika mengisi artefak pada `docs/module-blueprints/<module>/`.

Contoh diambil dari modul Allowance Type yang sudah ada di backend, agar terlihat bagaimana
kode nyata diterjemahkan menjadi dokumen yang dapat dibaca orang non-teknis.

---

## 1. Ringkasan modul

Allowance Type adalah data induk (*master data*) yang menyimpan daftar jenis tunjangan yang
boleh diberikan perusahaan kepada pegawai. Contohnya tunjangan jabatan, tunjangan transport,
dan tunjangan jaga malam.

Modul ini tidak menghitung gaji. Modul ini hanya menetapkan aturan main setiap jenis
tunjangan, lalu dipakai oleh proses payroll.

| Field | Nilai |
| --- | --- |
| Pemilik proses | Human Resource |
| Jenis data | Master data |
| Dipakai oleh | Payroll Management |
| Dampak bila salah | Perhitungan gaji pegawai ikut salah |

---

## 2. Proses bisnis

### 2.1 Tujuan

Memastikan setiap jenis tunjangan yang dipakai payroll sudah terdaftar, punya cara hitung
yang jelas, dan sudah disetujui pihak berwenang.

### 2.2 Pelaku

| Pelaku | Kewenangan |
| --- | --- |
| Staf HR | Mengusulkan dan mengisi data jenis tunjangan |
| Admin HR | Membuat, mengubah, dan menonaktifkan jenis tunjangan |
| Manajer HR | Menyetujui jenis tunjangan yang berdampak ke biaya |
| Staf Payroll | Memakai data ini saat menjalankan penggajian |

### 2.3 Pemicu

Perusahaan menetapkan jenis tunjangan baru, atau mengubah aturan tunjangan yang sudah ada.

### 2.4 Prasyarat

Komponen payroll (`PayrollComponent`) yang menaungi tunjangan tersebut sudah terdaftar.

### 2.5 Langkah utama

1. Admin HR membuka daftar jenis tunjangan.
2. Admin HR menekan tombol Tambah, lalu mengisi nama tunjangan, kategori, cara perhitungan,
   mata uang, dan nilai bawaan.
3. Sistem membuat kode tunjangan secara otomatis. Admin tidak mengetik kode sendiri agar
   tidak bentrok.
4. Sistem memeriksa kelengkapan dan kewajaran isian.
5. Bila lolos pemeriksaan, data tersimpan dengan status aktif.
6. Staf Payroll memakai jenis tunjangan tersebut pada periode penggajian berikutnya.

### 2.6 Aturan bisnis

**Aturan A — Cara perhitungan menentukan isian wajib.**

Jika cara perhitungan `Percentage`, maka persentase wajib diisi. Jika `Fixed`, maka nominal
tetap yang wajib diisi.

> **Contoh:** Tunjangan jabatan memakai `Percentage` sebesar 10%. Kolom nominal tetap boleh
> dikosongkan. Sebaliknya, tunjangan transport memakai `Fixed` sebesar Rp 500.000 per bulan,
> sehingga kolom persentase dikosongkan.

**Aturan B — Batas maksimum mengalahkan hasil perhitungan.**

> **Contoh:** Gaji pokok Rp 8.000.000, tunjangan jabatan 10%, hasil hitungnya Rp 800.000.
> Karena batas maksimum disetel Rp 600.000, yang dibayarkan adalah Rp 600.000.

**Aturan C — Tunjangan yang mensyaratkan kehadiran akan dipotong bila pegawai tidak masuk.**

> **Contoh:** Tunjangan transport Rp 500.000 untuk 20 hari kerja berarti Rp 25.000 per hari.
> Pegawai yang masuk 18 hari menerima 18 x Rp 25.000 = Rp 450.000.

**Aturan D — Data tidak pernah dihapus permanen.**

Jenis tunjangan yang sudah tidak dipakai hanya ditandai tidak aktif. Riwayat penggajian lama
tetap dapat ditelusuri.

### 2.7 Perubahan status

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| — | Buat baru | `Aktif` | Admin HR | Isian wajib lengkap dan lolos validasi |
| `Aktif` | Nonaktifkan | `Nonaktif` | Admin HR | Tidak sedang dipakai periode payroll berjalan |
| `Nonaktif` | Aktifkan kembali | `Aktif` | Admin HR | Masa berlaku belum berakhir |
| `Aktif` | Hapus | `Terhapus (ditandai)` | Admin HR | Belum pernah dipakai transaksi payroll |

### 2.8 Jalur tidak normal

| Kejadian | Yang terjadi | Yang dilihat pengguna |
| --- | --- | --- |
| Nama tunjangan sudah dipakai | Data ditolak | "Nama tunjangan sudah terdaftar. Gunakan nama lain." |
| Isian wajib kosong | Data ditolak | Kolom yang salah ditandai merah beserta alasannya |
| Pengguna tidak berhak | Tindakan dibatalkan | "Anda tidak memiliki hak akses untuk tindakan ini." |
| Tunjangan sedang dipakai payroll berjalan | Penonaktifan ditolak | "Jenis tunjangan sedang dipakai penggajian periode ini." |
| Tombol Simpan ditekan dua kali | Hanya satu data tersimpan | Tombol dinonaktifkan sementara saat proses menyimpan |

### 2.9 Hasil akhir

Jenis tunjangan tercatat, dapat dipilih pada proses payroll, dan seluruh perubahannya
terekam beserta nama pengubah dan waktunya.

---

## 3. Dokumentasi API

### Corporate / Human Resource / Master Data / Allowance Type

Base URL: `api/v1/corporate/human-resource/master-data/allowance-types`

Seluruh endpoint memerlukan pengguna yang sudah masuk (*authenticated*).

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Mengambil daftar pilihan filter untuk halaman daftar | `AllowanceType : Read` | — | `AllowanceTypeFilterMetadataResponse` |
| `GET` | `/summary` | Menampilkan ringkasan jumlah tunjangan aktif, nonaktif, kena pajak, dan sejenisnya | `AllowanceType : Read` | — | `AllowanceTypeSummaryResponse` |
| `GET` | `/` | Menampilkan daftar tunjangan dengan penyaringan dan halaman | `AllowanceType : Read` | Query: `startDate`, `endDate`, `allowanceCategory`, `calculationMethod`, `isRecurring`, `isTaxable`, dan lainnya | Daftar `AllowanceTypeResponse` berhalaman |
| `GET` | `/options` | Menyediakan pilihan tunjangan untuk dropdown di layar lain | `AllowanceType : Read` | Query pencarian | Daftar ringkas `id` dan nama |
| `GET` | `/{id}` | Menampilkan detail satu jenis tunjangan | `AllowanceType : Read` | Path: `id` (GUID) | `AllowanceTypeDetailResponse` |
| `POST` | `/` | Membuat jenis tunjangan baru | `AllowanceType : Create` | Body: `CreateAllowanceTypeRequest` | Data tunjangan yang baru dibuat |
| `PUT` | `/{id}` | Mengubah seluruh data jenis tunjangan | `AllowanceType : Update` | Path `id` + body `UpdateAllowanceTypeRequest` | Pesan berhasil |
| `PATCH` | `/{id}/status` | Mengaktifkan atau menonaktifkan tanpa mengubah data lain | `AllowanceType : Update` | Path `id` + body `UpdateAllowanceTypeStatusRequest` | Pesan berhasil |
| `DELETE` | `/{id}` | Menandai jenis tunjangan sebagai terhapus | `AllowanceType : Delete` | Path: `id` (GUID) | Pesan berhasil |

#### Kode status dan artinya

| Kode | Arti teknis | Arti bagi pengguna |
| --- | --- | --- |
| `200` | Berhasil | Permintaan diproses dan datanya tersedia |
| `400` | Permintaan tidak valid | Isian tidak lengkap, formatnya salah, atau melanggar aturan bisnis |
| `401` | Belum masuk | Sesi habis; pengguna perlu masuk ulang |
| `403` | Tidak berwenang | Pengguna sudah masuk tetapi tidak punya hak untuk tindakan ini |
| `404` | Tidak ditemukan | Data yang dibuka sudah dihapus atau tidak pernah ada |
| `409` | Bentrok | Data yang sama sedang diubah pihak lain, atau melanggar keunikan |

#### Bentuk balasan

Seluruh endpoint membungkus hasilnya di dalam `ApiResponse<T>`, sehingga bentuknya seragam:

```json
{
  "statusCode": 200,
  "success": true,
  "message": "Ringkasan allowance type berhasil diambil.",
  "data": {
    "totalAllowanceType": 24,
    "activeAllowanceType": 21,
    "inactiveAllowanceType": 3
  }
}
```

Contoh balasan gagal:

```json
{
  "statusCode": 400,
  "success": false,
  "message": "Nama tunjangan sudah terdaftar. Gunakan nama lain.",
  "data": null
}
```

Catatan: seluruh data pada contoh di atas adalah data samaran.

---

## 4. Bukti penelusuran

Setiap klaim yang menyangkut kode ditulis dengan format
`repository + path + line/symbol + commit SHA`.

| Klaim | Bukti |
| --- | --- |
| Grup Swagger dan base URL | `NewQuilvianSystemBackend` + `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Controllers/AllowanceTypeController.cs` + baris 15-26 + `<commit-sha>` |
| Hak akses per endpoint | file yang sama + atribut `AccessPermission` + `<commit-sha>` |
| Bentuk balasan seragam | file yang sama + pemanggilan `ApiResponse<T>.Ok` + `<commit-sha>` |

Isi `<commit-sha>` dengan SHA yang benar-benar diaudit. Jangan mengosongkannya.

