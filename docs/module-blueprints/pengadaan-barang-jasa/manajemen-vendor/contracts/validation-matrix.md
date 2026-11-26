# Manajemen Vendor — Matriks Validasi

| Field | Nilai |
| --- | --- |
| Kontrak | `PBJ-VD-VAL` · `last_changed_in: 1.1` (`PBJ-DEC-064`, 2026-11-19) · status `approved` (`PBJ-DEC-062`) |
| Masukan | `PBJ-DEC-045`..`050`, `059`..`061`; `PBJ-PP-VAL` 1.2 (kode umum `PBJ-VAL-090`..`094` berlaku) |

Kode galat dikirim di `errors.code`. Kode `PBJ-VAL-1xx` khusus sub-modul ini. Bagian 2 (1.1) mendaftar penolakan yang
sengaja **tanpa** kode khusus.

## 1. Kode aturan

| Kode | Aturan | Saat | Kondisi gagal | Pesan | HTTP |
| --- | --- | --- | --- | --- | --- |
| `PBJ-VAL-100` | Kategori vendor sah | Atur kategori | Kategori yang **ditambahkan** nonaktif atau tidak ada; kategori lama yang kini nonaktif boleh dipertahankan (1.1, `VD-003 D-5`) | "Kategori {nama} tidak aktif." | 422 |
| `PBJ-VAL-101` | Tanggal berlaku dokumen | Catat dokumen | Jenis mewajibkan tanggal berlaku tetapi kosong, atau tanggal berlaku sebelum tanggal terbit. **Contoh:** Izin PBF tanpa tanggal berlaku | "Tanggal berlaku wajib diisi untuk {jenis}." / "Tanggal berlaku tidak boleh sebelum tanggal terbit." | 422 |
| `PBJ-VAL-102` | Berkas ada sebelum verifikasi | Verifikasi | Dokumen belum punya berkas | "Unggah berkas dokumen sebelum diverifikasi." | 422 |
| `PBJ-VAL-103` | Pemeriksa ≠ pengunggah | Verifikasi, tolak | Pemeriksa adalah pengunggah (`PBJ-DEC-046`). Diperiksa **sebelum** `PBJ-VAL-102` | "Dokumen ini harus diperiksa petugas lain." | 403 |
| `PBJ-VAL-104` | Alasan wajib | Tolak dokumen, batal dokumen, cabut Dibatasi, batal usulan, batal/revisi penilaian | Alasan kosong atau < 10 karakter | "Alasan wajib diisi (minimal 10 karakter)." | 422 |
| `PBJ-VAL-105` | Syarat Dibatasi | Batasi | Alasan kosong, atau tanggal berakhir sebelum hari ini. **(1.1, `VD-006`)** Vendor Diblokir atau sedang Dibatasi ditolak dengan `PBJ-VAL-093`, bukan kode ini | "Alasan pembatasan wajib diisi." / "Tanggal berakhir tidak boleh sebelum hari ini." | 422 |
| `PBJ-VAL-106` | Jenis usulan sesuai status | Buat usulan | `BLOCK` untuk vendor sudah diblokir, atau `UNBLOCK` untuk vendor tidak diblokir | "Vendor sudah diblokir." / "Vendor tidak sedang diblokir." | 409 |
| `PBJ-VAL-107` | Satu usulan terbuka | Buat usulan | Ada usulan `DRAFT`/`PENDING_APPROVAL`/`RETURNED` untuk vendor itu | "Masih ada usulan {nomor} yang belum selesai untuk vendor ini." Usulan draf tanpa nomor tertulis "(draf)" (1.1) | 409 |
| `PBJ-VAL-108` | Kategori penilaian milik vendor dan kriteria lengkap | Buat penilaian | Kategori bukan kategori vendor, atau total bobot kriteria aktif ≠ 100. **Contoh:** bobot 40 + 40 = 80 | "Total bobot kriteria {kategori} harus 100%. Saat ini {total}%." | 422 |
| `PBJ-VAL-109` | Satu penilaian per vendor × kategori × periode | Buat penilaian | Sudah ada penilaian `DRAFT`/`FINAL` pada kombinasi yang sama, termasuk revisi (`PBJ-DEC-059`; 1.1 `VD-008 D-1`) | "Penilaian {vendor} {kategori} {periode} sudah ada. Buat revisi bila perlu koreksi." | 409 |
| `PBJ-VAL-110` | Nilai lengkap | Finalkan | Ada kriteria tanpa nilai, nilai di luar 1–5, atau rekomendasi kosong | "Semua kriteria wajib dinilai 1–5 dan rekomendasi wajib dipilih." | 422 |
| `PBJ-VAL-111` | Pemfinal ≠ penilai | Finalkan | Pemfinal adalah penilainya: pembuat draf, atau pembuat revisi untuk penilaian revisi (`PBJ-DEC-049`; 1.1 `VD-008 D-3`) | "Penilaian harus difinalkan orang lain." | 403 |
| `PBJ-VAL-112` | Revisi sah | Buat revisi | Penilaian bukan `FINAL`, atau sudah ada revisi `DRAFT` | "Revisi hanya dari penilaian final dan hanya satu revisi berjalan." | 409 |
| `PBJ-VAL-113` | Bobot kriteria | Simpan kriteria | Bobot ≤ 0 atau > 100 | "Bobot harus lebih dari 0 dan paling tinggi 100." | 422 |
| `PBJ-VAL-071` | Master terpakai tidak dihapus | Hapus jenis dokumen/kriteria/kewajiban | Sudah dipakai dokumen atau penilaian | "… sudah dipakai. Nonaktifkan saja." | 400 + `errors.code` (`PBJ-DEC-041`) |
| `PBJ-VAL-090` | Data tidak basi | Semua ubah | `expectedRowVersion` tidak cocok | (umum) | 409 |
| `PBJ-VAL-091` | Approval terkonfigurasi | Ajukan usulan | Definisi `PBJ-VND-*` tidak aktif | "Alur persetujuan status vendor belum dikonfigurasi. Hubungi administrator." | 409 |
| `PBJ-VAL-093` | Transisi sah | Semua aksi status | Lihat `state-transition-matrix.md`. **(1.1)** Termasuk: batasi vendor Diblokir atau sedang Dibatasi, cabut vendor yang tidak Dibatasi (`VD-006 D-1`); ubah/ajukan usulan yang sedang diproses; ubah atau batal penilaian final | (umum) | 409 |
| `PBJ-VAL-094` | Saringan tanggal sah | Baca daftar | Rentang terbalik / periode tak dikenal | (umum) | 422 |

## 2. Penolakan tanpa kode khusus (1.1, `PBJ-DEC-064`)

Penolakan berikut sengaja dijawab tanpa `errors.code`, karena layar cukup menampilkan pesannya dan tidak ada keputusan
bisnis yang bergantung pada kodenya.

| Kejadian | HTTP | Rujukan |
| --- | --- | --- |
| Isian gagal anotasi DTO (`[Required]`, `[MaxLength]`, `[Range]`) | 400 `ValidationProblemDetails` | `VD-002 D-8` |
| Nilai saringan tak dikenal, mis. `vendorStatus=XYZ` → "Status vendor 'XYZ' tidak dikenal." | 422 | `VD-003 D-10` |
| Catat dokumen dengan jenis nonaktif atau vendor tak dikenal | 422 | `VD-004 D-5` |
| Batal dokumen oleh selain pengunggah → "Hanya pengunggah yang dapat membatalkan dokumen ini." | 403 | `VD-004 D-3` |
| Ubah, ajukan, atau tarik usulan oleh selain pengusul → "Hanya pengusul yang dapat …" | 403 | `VD-007 D-2` |
| Nomor periode penilaian di luar rentang jenisnya | 422 | `VD-008 D-8` |
| Bobot kriteria lebih dari dua desimal | 422 | `VD-002 D-5` |
| Pindah kategori kriteria yang sudah dipakai penilaian; ganti pasangan kategori × jenis pada kewajiban yang pernah berlaku; nonaktifkan jenis dokumen yang masih diwajibkan kewajiban aktif | 409 | `VD-002 D-3`, `D-4`, `D-5` |
| Aktifkan kewajiban dengan kategori atau jenis dokumen nonaktif | 422 | `VD-002 D-4` |
| Kelayakan tanpa `itemCategoryId` / kategori atau vendor tak dikenal | 400 / 404 | `VD-005 D-4` |
