# Perencanaan & Permintaan — Matriks Validasi

| Field | Nilai |
| --- | --- |
| Kontrak | `PBJ-PP-VAL` · `last_changed_in: 1.0` · status `approved` (`PBJ-DEC-034`) |
| Pemilik | Pengguna sesi · `approved_by: Pengguna sesi` · `approved_at: 2026-11-12` |
| Masukan | `PBJ-DEC-007`..`030`; state matrix `PBJ-PP-STATE` 1.0 |

Semua validasi dijalankan di **server**. Validasi di frontend hanya membantu pengguna, bukan penjaga.
Kode HTTP: 422 untuk isian tidak memenuhi aturan, 409 untuk bentrok status atau data bersamaan, 404 untuk
data di luar cakupan pengguna, 403 untuk izin yang tidak dimiliki.

## 1. Permintaan Pengadaan

| Kode | Aturan | Berlaku pada | Kondisi ditolak | Pesan bagi pengguna | HTTP |
| --- | --- | --- | --- | --- | --- |
| `PBJ-VAL-001` | Minimal satu baris | Ajukan | Tidak ada baris aktif | "Pengajuan harus berisi minimal satu barang atau jasa." | 422 |
| `PBJ-VAL-002` | Kuantitas positif | Simpan, Ajukan | Kuantitas ≤ 0. **Contoh:** 0 kursi roda | "Jumlah pada baris {n} harus lebih dari 0." | 422 |
| `PBJ-VAL-003` | Desimal sesuai satuan | Simpan | Kuantitas pecahan untuk satuan `IsDecimalAllowed = false`. **Contoh:** 1,5 unit | "Satuan {satuan} tidak boleh pecahan." | 422 |
| `PBJ-VAL-004` | Estimasi harga tidak negatif | Simpan | Harga < 0 | "Perkiraan harga tidak boleh negatif." | 422 |
| `PBJ-VAL-005` | Barang aktif di katalog | Simpan, Ajukan | Barang nonaktif atau tidak ada | "Barang {kode} tidak aktif di katalog. Pilih barang lain atau minta Tim Pengadaan mengaktifkannya." | 422 |
| `PBJ-VAL-006` | Justifikasi wajib | Ajukan | Kosong atau < 10 karakter | "Alasan kebutuhan wajib diisi (minimal 10 karakter)." | 422 |
| `PBJ-VAL-007` | Tanggal kebutuhan | Ajukan | Tanggal dibutuhkan sebelum hari ini | "Tanggal dibutuhkan tidak boleh sebelum hari ini." | 422 |
| `PBJ-VAL-008` | Alasan mendesak (`PBJ-DEC-016`) | Ajukan | Mendesak tanpa alasan atau tanpa batas waktu | "Permintaan mendesak wajib berisi alasan dan batas waktu." | 422 |
| `PBJ-VAL-009` | Alasan di luar rencana (`PBJ-DEC-014`, `029`) | Ajukan | Ada kuantitas di luar rencana tanpa alasan pada baris itu. **Contoh:** sisa rencana 4, diminta 6, alasan kosong | "Baris {n}: {jumlah} {satuan} di luar rencana. Isi alasannya." | 422 |
| `PBJ-VAL-010` | Unit dalam cakupan (`PBJ-DEC-020`) | Buat, Ajukan | Pengguna tidak punya penempatan aktif di unit PR | "Anda tidak terdaftar di unit {unit}." | 404 (cakupan) / 422 (isian) |
| `PBJ-VAL-011` | Sumber dana aktif | Ajukan | Sumber dana nonaktif atau kosong | "Pilih sumber dana yang masih aktif." | 422 |
| `PBJ-VAL-012` | Baris rencana cocok | Ajukan | `NeedPlanLineKey` dipilih tetapi barang, unit, atau tahun berbeda dengan baris rencana | "Baris {n} tidak sesuai dengan rencana yang dipilih." | 422 |
| `PBJ-VAL-020` | Cek stok masuk akal | Catat cek stok | Dipenuhi stok > tersedia, dipenuhi stok > diminta, atau nilai negatif. **Contoh:** diminta 20, tersedia 5, dipenuhi 8 | "Jumlah dipenuhi dari stok tidak boleh melebihi stok tersedia ({tersedia}) maupun jumlah diminta ({diminta})." | 422 |
| `PBJ-VAL-021` | Semua baris sudah dicek | Selesaikan verifikasi | Ada baris tanpa hasil cek stok | "Masih ada {k} baris yang belum dicek stoknya." | 422 |
| `PBJ-VAL-030` | Alasan tindakan | Kembalikan, Tolak, Batal (bernomor) | Kosong | "Alasan wajib diisi." | 422 |
| `PBJ-VAL-090` | Data tidak basi | Ubah | `ExpectedRowVersion` ≠ data saat ini | "Data sudah diubah orang lain. Muat ulang lalu ulangi perubahan Anda." | 409 |
| `PBJ-VAL-091` | Konfigurasi approval ada | Selesaikan verifikasi | Definisi `Wfl` tidak aktif, atau tidak ada penyetuju sama sekali | "Alur persetujuan pengadaan belum dikonfigurasi. Hubungi administrator." | 409 |
| `PBJ-VAL-092` | Belum masuk paket | Batal setelah disetujui | PR sudah `CONVERTED_TO_PACKAGE` | "Pengajuan sudah masuk paket dan tidak dapat dibatalkan dari sini." | 409 |
| `PBJ-VAL-093` | Transisi sah | Semua aksi status | Lihat transisi tidak sah di `state-transition-matrix.md` | "Status pengajuan tidak memungkinkan tindakan ini." | 409 |

**Rumus server (bukan validasi, tetapi menentukan hasil):**
- Kebutuhan beli per baris = diminta − dipenuhi stok.
- Nilai beli per baris = kebutuhan beli × perkiraan harga satuan.
- `PurchaseTotal` = Σ nilai beli.

**Contoh:** 20 kursi roda @Rp 3.000.000, dengan 5 dipenuhi stok → 15 × Rp 3.000.000 = Rp 45.000.000.

**Rumus lewat pagu (`PBJ-DEC-030`):**
- Untuk setiap kategori barang di PR: kelebihan = max(0, nilai beli kategori − sisa pagu(tahun, unit, kategori, sumber dana)).
- Pagu yang tidak ada dianggap sisa 0.
- `IsOverBudget` = ada kelebihan > 0; `OverBudgetAmount` = Σ kelebihan.

**Contoh:** sisa Rp 30 juta, nilai beli Rp 45 juta → kelebihan Rp 15 juta. Ini **bukan** penolakan; PR mendapat
tanda dan kode kondisi `PBJ_OVER_BUDGET`.

## 2. Rencana kebutuhan, rencana paket, pagu

| Kode | Aturan | Berlaku pada | Kondisi ditolak | Pesan bagi pengguna | HTTP |
| --- | --- | --- | --- | --- | --- |
| `PBJ-VAL-040` | Satu rencana aktif per unit-tahun | Buat | Sudah ada rencana unit-tahun yang belum `CANCELLED`/`REJECTED` | "Unit {unit} sudah memiliki rencana {tahun}. Buka rencana itu atau buat revisi." | 409 |
| `PBJ-VAL-041` | Satu revisi berjalan | Buat revisi | Ada versi lain `DRAFT`/`PENDING_APPROVAL`/`RETURNED` | "Masih ada revisi yang belum selesai." | 409 |
| `PBJ-VAL-042` | Revisi tidak di bawah pemakaian | Ajukan revisi | Kuantitas baris < kuantitas yang dipakai PR aktif, atau < yang dialokasikan ke rencana paket aktif | "Baris {barang}: sudah dipakai {terpakai} {satuan}, tidak bisa diturunkan ke {baru}." | 422 |
| `PBJ-VAL-043` | Tahun anggaran wajar | Buat | Tahun < tahun berjalan − 1 atau > tahun berjalan + 2 | "Tahun rencana harus antara {min} dan {maks}." | 422 |
| `PBJ-VAL-044` | Barang unik per rencana | Simpan | Barang yang sama muncul dua kali | "Barang {kode} sudah ada di rencana ini. Gabungkan jumlahnya." | 422 |
| `PBJ-VAL-050` | Alokasi tidak melebihi rencana | Simpan, Ajukan rencana paket | Σ alokasi untuk satu baris rencana (semua rencana paket aktif) > kuantitas baris. **Contoh:** baris 10, paket A 6, paket B 5 | "Alokasi {barang} unit {unit} melebihi rencana ({rencana}), sudah dialokasikan {terpakai}." | 422 |
| `PBJ-VAL-051` | Satu jenis per rencana paket | Simpan | Baris dengan jenis pengadaan berbeda dari kepala paket | "Semua baris paket harus berjenis {jenis}." | 422 |
| `PBJ-VAL-060` | Pagu unik per kunci | Buat | Sudah ada pagu aktif untuk tahun × unit × kategori × sumber dana yang sama | "Pagu untuk kombinasi ini sudah ada. Buat revisi." | 409 |
| `PBJ-VAL-061` | Nilai pagu positif | Simpan | ≤ 0 | "Nilai pagu harus lebih dari 0." | 422 |
| `PBJ-VAL-062` | Revisi pagu ≥ komitmen | Ajukan revisi | Nilai baru < komitmen aktif | "Pagu tidak boleh lebih kecil dari komitmen yang sudah berjalan ({komitmen})." | 422 |

## 3. Katalog dan lampiran

| Kode | Aturan | Kondisi ditolak | Pesan bagi pengguna | HTTP |
| --- | --- | --- | --- | --- |
| `PBJ-VAL-070` | Nama barang unik per jenis-kategori-satuan | Kombinasi nama (tanpa beda huruf besar/kecil), jenis, kategori, dan satuan sudah ada | "Barang serupa sudah ada: {kode} {nama}." | 409 |
| `PBJ-VAL-071` | Barang terpakai tidak dihapus | Hapus barang yang dirujuk dokumen | "Barang sudah dipakai dan hanya dapat dinonaktifkan." | 409 |
| `PBJ-VAL-072` | Rujukan obat sesuai jenis | `DrugId` diisi untuk jenis `SERVICE` | "Rujukan obat hanya untuk barang, bukan jasa." | 422 |
| `PBJ-VAL-080` | Ekstensi diizinkan | Ekstensi di luar daftar | "Jenis berkas tidak diizinkan. Gunakan PDF, JPG, PNG, DOCX, atau XLSX." | 422 |
| `PBJ-VAL-081` | Isi sesuai ekstensi | Tanda tangan berkas tidak cocok. **Contoh:** `penawaran.pdf` yang isinya HTML | "Isi berkas tidak sesuai dengan jenisnya." | 422 |
| `PBJ-VAL-082` | Ukuran | > batas (bawaan 10 MB) | "Ukuran berkas maksimal {batas} MB." | 422 |
| `PBJ-VAL-083` | Lampiran hanya pada dokumen yang masih bisa diubah | Unggah saat dokumen final | "Dokumen sudah final; lampiran tidak dapat ditambah." | 409 |
