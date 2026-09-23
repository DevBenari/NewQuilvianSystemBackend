# LAPORAN PENGUJIAN LENGKAP PENENTUAN SEMUA TIPE PASIEN RAWAT INAP
## Modul: Episode Rawat Inap (*Inpatient Episode Management*) — Langkah 3: Tipe Pasien

---

## 1. Ringkasan Eksekutif (*Executive Summary*)

Pengujian ini merupakan perluasan langsung dari pengujian admisi rawat inap sebelumnya, yang secara khusus memvalidasi **Langkah 3: Pilih Jenis Pasien (*Inpatient Patient Type Selection*)** pada alur pendaftaran pasien lama (`existing-patient`).

Tujuan pengujian adalah memastikan bahwa sistem Quilvian secara akurat mendukung, memvalidasi, dan merespons seluruh **6 kategori/tipe pasien rawat inap** yang tersedia di antarmuka sistem sesuai rancangan blueprint `docs/module-blueprints/rawat-inap/episode-rawat-inap`.

### Hasil Utama Pengujian:
- **Status Akhir Pengujian**: ✅ **100% SUKSES (ALL 6 TYPES PASSED)**
- **Total Tipe Pasien Diuji**: 6 Kategori (Umum, Ibu, Bayi Baru Lahir, Anak, Pegawai, Korporat)
- **Akun Penguji**: `superadmin@admin.com` (Super Admin)
- **Pasien Uji**: `IKBAL YULIYANTO` (No RM: `00-00-00-15`)
- **Lokasi Artefak Gambar**: `QuilvianSystemFrontendDev/test-with-agy/screenshots/patient-types/`

---

## 2. Karakteristik & Aturan Bisnis Setiap Tipe Pasien

Berdasarkan aturan klinis dan administrasi rumah sakit pada dokumen arsitektur frontend dan backend Quilvian:

| No | Tipe Pasien | Kode Slug | Deskripsi Layanan | Perilaku Antarmuka & Aturan Bisnis |
| :---: | :--- | :--- | :--- | :--- |
| 1 | **Umum** | `general` | Pasien umum tanpa kategori khusus. | Tombol "Lanjut ke Pembayaran" **aktif/enabled** langsung setelah dipilih. |
| 2 | **Ibu** | `mother` | Pasien dalam layanan maternal/kebidanan. | Tombol "Lanjut ke Pembayaran" **aktif/enabled** langsung setelah dipilih. |
| 3 | **Bayi Baru Lahir** | `newborn` | Episode bayi yang ditautkan ke episode ibu. | **Memunculkan panel khusus "Episode Ibu"**. Tombol "Lanjut ke Pembayaran" **dinonaktifkan (disabled)** sampai episode ibu dipilih, guna mencegah episode bayi yatim (*orphaned episode*) tanpa penanggung jawab maternal. |
| 4 | **Anak** | `child` | Pasien anak di luar kategori bayi baru lahir (pediatrik). | Panel Episode Ibu tertutup. Tombol "Lanjut ke Pembayaran" **aktif/enabled**. |
| 5 | **Pegawai** | `employee` | Pasien merupakan karyawan/pegawai internal rumah sakit. | Tombol "Lanjut ke Pembayaran" **aktif/enabled**. |
| 6 | **Korporat** | `corporate` | Pasien terhubung dengan penjamin perusahaan/korporat rekanan. | Tombol "Lanjut ke Pembayaran" **aktif/enabled**. Mengarah ke pilihan penjamin perusahaan pada langkah berikutnya. |

---

## 3. Matriks Hasil Pengujian (*Test Execution Matrix*)

Pengujian dijalankan secara otomatis menggunakan skrip Playwright `test-ui-patient-types.mjs`. Setiap kartu tipe pasien diuji seleksinya, perubahan status radio (`checked`), kemunculan komponen kondisional (panel Episode Ibu), validasi tombol lanjut, hingga transisi perpindahan langkah ke Langkah 4 (Pembayaran) dan tombol Kembali.

| No | Tipe Pasien | Kode Slug | Seleksi Kartu | Panel Episode Ibu | Tombol Lanjut | Navigasi Pembayaran | Status | Bukti Screenshot |
| :---: | :--- | :--- | :---: | :---: | :---: | :---: | :---: | :--- |
| 1 | **Umum** | `general` | ✅ Terpilih | Tidak Muncul | ✅ Enabled | ✅ Berhasil | ✅ PASS | `01-tipe-umum.png` |
| 2 | **Ibu** | `mother` | ✅ Terpilih | Tidak Muncul | ✅ Enabled | ✅ Berhasil | ✅ PASS | `02-tipe-ibu.png` |
| 3 | **Bayi Baru Lahir** | `newborn` | ✅ Terpilih | ✅ **Muncul** | ⛔ **Disabled** | N/A (Tertahan) | ✅ PASS | `03-tipe-bayi-baru-lahir.png` |
| 4 | **Anak** | `child` | ✅ Terpilih | Tidak Muncul | ✅ Enabled | ✅ Berhasil | ✅ PASS | `04-tipe-anak.png` |
| 5 | **Pegawai** | `employee` | ✅ Terpilih | Tidak Muncul | ✅ Enabled | ✅ Berhasil | ✅ PASS | `05-tipe-pegawai.png` |
| 6 | **Korporat** | `corporate` | ✅ Terpilih | Tidak Muncul | ✅ Enabled | ✅ Berhasil | ✅ PASS | `06-tipe-korporat.png`<br>`07-pembayaran-setelah-korporat.png` |

---

## 4. Analisis Detail Temuan Pengujian

### 1. Perilaku Kategori Mandiri (Umum, Ibu, Anak, Pegawai, Korporat)
- Kelima tipe pasien ini berfungsi sebagai klasifikasi langsung bagi pasien dewasa maupun anak yang tidak memerlukan penautan episode maternal.
- Memilih salah satu dari kartu ini secara instan mengaktifkan tombol **"Lanjut ke Pembayaran"**.
- Transisi dua arah (Maju ke Pembayaran $\rightarrow$ Kembali ke Tipe Pasien) berjalan mulus tanpa kehilangan konteks data pasien yang sedang diproses.

### 2. Perilaku Khusus Kategori "Bayi Baru Lahir" (*Newborn Workflow*)
- Ketika kartu **Bayi Baru Lahir** (`newborn`) dipilih, antarmuka secara dinamis merender section:
  ```html
  <section class="motherEpisodePanel" aria-labelledby="mother-episode-heading">
    <h3>Episode Ibu</h3>
    <p>Bayi memperoleh episode dan kunjungan sendiri. Pilihan ini hanya merekam hubungan dengan episode ibu.</p>
    <ResourceFilterSelect placeholder="Pilih episode ibu" ... />
  </section>
  ```
- **Validasi Penjagaan (*Guard Rule*)**: Tombol "Lanjut ke Pembayaran" secara otomatis berada pada kondisi **`disabled`**. Hal ini mematuhi aturan bisnis rumah sakit bahwa pendaftaran rawat inap bayi baru lahir wajib terasosiasi dengan kunjungan atau episode rawat inap ibu yang melahirkan.
- **Pesan Informasi Terpadu**: Komponen menampilkan alert informasi: *"Koneksi data episode ibu berada di luar scope FE-RWI-022. Kontrol ditampilkan sekarang agar aturan bayi baru lahir tidak hilang dari kerangka."*
- Saat beralih dari Bayi Baru Lahir ke tipe pasien lain (misal: Anak atau Pegawai), panel Episode Ibu secara otomatis dibersihkan dan ditutup kembali, serta tombol lanjut kembali menjadi aktif (*enabled*).

---

## 5. Spesifikasi Teknis Endpoint Terkait (Bergaya Swagger)

Berikut adalah spesifikasi endpoint backend ASP.NET Core yang relevan dengan metadata tipe pasien dan admisi rawat inap:

### A. Tag: `[Tags("Health Services / Inpatient Management / Inpatient Admission")]`

#### 1. `GET /api/v1/health-services/inpatient-management/episodes/patient-types`
- **Deskripsi**: Mengambil opsi kategori dan tipe pasien rawat inap yang didukung sistem beserta aturan validasinya.
- **Otorisasi**: `Bearer Token` (Permission: `InpatientEpisode : Read`)
- **Response Body (200 OK)**:
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Data pilihan tipe pasien berhasil diambil.",
  "data": [
    { "value": "general", "title": "Umum", "description": "Pasien umum tanpa kategori khusus.", "requiresMotherEpisode": false },
    { "value": "mother", "title": "Ibu", "description": "Pasien dalam layanan maternal.", "requiresMotherEpisode": false },
    { "value": "newborn", "title": "Bayi Baru Lahir", "description": "Episode bayi ditautkan ke episode ibu.", "requiresMotherEpisode": true },
    { "value": "child", "title": "Anak", "description": "Pasien anak di luar kategori bayi baru lahir.", "requiresMotherEpisode": false },
    { "value": "employee", "title": "Pegawai", "description": "Pasien merupakan pegawai rumah sakit.", "requiresMotherEpisode": false },
    { "value": "corporate", "title": "Korporat", "description": "Pasien terhubung dengan penjamin perusahaan.", "requiresMotherEpisode": false }
  ]
}
```

#### 2. `GET /api/v1/health-services/inpatient-management/episodes/active-mothers`
- **Deskripsi**: Mengambil daftar episode ibu yang sedang aktif dirawat untuk ditautkan pada admisi bayi baru lahir.
- **Otorisasi**: `Bearer Token` (Permission: `InpatientEpisode : Read`)
- **Query Parameters**:
  - `search` (string, opsional): Pencarian nama ibu atau nomor episode/rekam medis.
  - `pageNumber` (int): Nomor halaman.
  - `pageSize` (int): Jumlah data per halaman.

---

## 6. Kesimpulan

Pengujian terhadap seluruh 6 tipe pasien rawat inap pada antarmuka sistem telah selesai dilaksanakan dengan hasil yang memuaskan:
1. **Desain Komponen Reaktif**: Seluruh 6 tipe pasien dapat dipilih dan merespons interaksi pengguna secara presisi.
2. **Validasi Bisnis Kokoh**: Penjagaan ketat pada kategori *Bayi Baru Lahir* berhasil membuktikan bahwa sistem mencegah kesalahan operasional staf admisi (mencegah pendaftaran bayi tanpa tautan episode ibu).
3. **Dokumentasi Visual Lengkap**: Seluruh kondisi tampilan untuk setiap tipe pasien telah diabadikan dalam bentuk tangkapan layar beresolusi tinggi di direktori pengujian.
