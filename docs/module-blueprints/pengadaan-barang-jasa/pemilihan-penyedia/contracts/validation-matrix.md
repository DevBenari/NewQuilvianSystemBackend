# Pemilihan Penyedia — Matriks Validasi

| Field | Nilai |
| --- | --- |
| Kontrak | `PBJ-PS-VAL` · `last_changed_in: 1.0` · status `approved` (`PBJ-DEC-095`) |
| Pemilik | Pengguna sesi · `approved_by: Pengguna sesi` · `approved_at: 2026-11-26` |
| Masukan | `PBJ-DEC-066`..`085`; `PBJ-ASM-005`..`009`; `PBJ-AC-022`..`036`; `PBJ-PS-STATE` 1.0 |
| Kompatibilitas | Kode baru `PBJ-VAL-120`..`149`. Kode bersama yang dipakai ulang: `PBJ-VAL-071` (hapus master terpakai, `PBJ-DEC-041`), `PBJ-VAL-090` (data basi), `PBJ-VAL-091` (alur persetujuan belum dikonfigurasi) |

Galat anotasi DTO (`[Required]`, `[MaxLength]`, `[Range]`) dijawab 400 `ValidationProblemDetails` sebelum aturan bisnis,
mengikuti konvensi `PBJ-VD-API` 1.1. Aturan bisnis dijawab dengan `errors.code` di bawah.

## 1. Kode aturan

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Baris PR belum siap | Buat paket, tambah baris | PR bukan `APPROVED`, atau baris tidak punya jumlah beli | "Baris permintaan ini belum disetujui atau tidak perlu dibeli." | 409 `PBJ-VAL-120` |
| Baris PR sudah di paket lain | Buat paket, tambah baris | Baris sudah punya paket hidup | "Baris permintaan ini sudah masuk paket {nomor}." | 409 `PBJ-VAL-121` |
| Jenis pengadaan berbeda (`PBJ-DEC-090`) | Buat paket, tambah baris | Jenis barang berbeda dengan paket | "Satu paket hanya untuk satu jenis pengadaan." | 422 `PBJ-VAL-122` |
| Tahun anggaran berbeda (`PBJ-DEC-091`) | Buat paket, tambah baris | Tahun PR berbeda dengan paket | "Satu paket hanya untuk satu tahun anggaran." | 422 `PBJ-VAL-123` |
| HPS belum lengkap | Ajukan persiapan | Ada barang dengan HPS satuan kosong atau 0 | "Lengkapi HPS setiap barang sebelum mengajukan." | 422 `PBJ-VAL-124` |
| Metode belum dipilih | Ajukan persiapan | Metode kosong atau tidak aktif | "Pilih metode pengadaan yang aktif." | 422 `PBJ-VAL-125` |
| Metode di luar rentang | Ajukan persiapan, ubah metode | HPS di luar rentang metode tanpa alasan | "Nilai HPS di luar rentang metode ini. Tuliskan alasannya." | 422 `PBJ-VAL-126` |
| Checklist belum lengkap | Ajukan persiapan | Dokumen wajib belum dilampirkan | "Dokumen wajib belum lengkap: {daftar}." | 422 `PBJ-VAL-127` |
| Status tidak mengizinkan | Semua aksi status | Transisi tidak tercantum di `PBJ-PS-STATE` | "Tindakan ini tidak dapat dilakukan pada status {status}." | 409 `PBJ-VAL-128` |
| RFQ sebelum persiapan disetujui | Buat/kirim RFQ | Paket bukan `READY_FOR_RFQ` | "RFQ baru dapat dikirim setelah persiapan disetujui." | 409 `PBJ-VAL-129` |
| Vendor tidak layak | Undang vendor | Tidak layak untuk salah satu kategori paket (`PBJ-ASM-006`) | "{vendor} tidak layak untuk kategori {kategori}: {sebab}." | 422 `PBJ-VAL-130` |
| Vendor Dibatasi tanpa alasan | Undang vendor, ajukan penetapan | Vendor Dibatasi dan alasan kosong (`PBJ-DEC-047`) | "{vendor} sedang dibatasi. Tuliskan alasan memilihnya." | 422 `PBJ-VAL-131` |
| Batas waktu tidak sah | Kirim RFQ, perpanjang | Batas waktu tidak di masa depan, atau perpanjangan setelah batas lewat (`PBJ-DEC-081`) | "Batas waktu harus di masa depan dan hanya dapat diperpanjang sebelum lewat." | 422 `PBJ-VAL-132` |
| Vendor tidak diundang | Catat penawaran | Vendor tidak ada di undangan RFQ | "Vendor ini tidak diundang pada RFQ {nomor}." | 422 `PBJ-VAL-133` |
| Penawaran ganda | Catat penawaran | Vendor sudah punya penawaran tercatat di RFQ itu | "Penawaran {vendor} sudah dicatat. Ubah catatan yang ada." | 409 `PBJ-VAL-134` |
| Pencatat mengevaluasi | Isi hasil evaluasi | Evaluator = pencatat penawaran (`PBJ-DEC-074`) | "Anda mencatat penawaran ini, sehingga tidak dapat mengevaluasinya." | 403 `PBJ-VAL-135` |
| Alasan tidak lulus | Isi hasil evaluasi | `FAIL` tanpa alasan | "Tuliskan alasan penawaran tidak lulus." | 422 `PBJ-VAL-136` |
| Harga negosiasi naik | Catat negosiasi | Harga baru > harga sebelumnya (`PBJ-ASM-005`) | "Harga hasil negosiasi tidak boleh lebih tinggi dari harga sebelumnya." | 422 `PBJ-VAL-137` |
| Negosiasi tidak sah | Catat negosiasi | Penawaran terlambat, tidak lengkap, batal, atau belum *Lulus* | "Negosiasi hanya dengan penawaran yang sah dan lulus evaluasi." | 422 `PBJ-VAL-138` |
| Penawaran tidak dapat ditetapkan | Ajukan penetapan | Penawaran tidak sah atau belum *Lulus* dari semua evaluator | "Penawaran ini belum memenuhi syarat untuk ditetapkan: {sebab}." | 422 `PBJ-VAL-139` |
| Penawaran kurang dari minimum | Ajukan penetapan | Penawaran sah < minimum metode (`PBJ-DEC-075`) | "Penawaran sah baru {n} dari minimum {m}. Kirim RFQ ulang atau ganti metode." | 422 `PBJ-VAL-140` |
| Melebihi HPS tanpa alasan | Ajukan penetapan | Harga akhir > HPS dan alasan kosong (`PBJ-DEC-073`) | "Harga akhir melebihi HPS. Negosiasikan atau tuliskan alasannya." | 422 `PBJ-VAL-141` |
| Bukan terendah tanpa alasan | Ajukan penetapan | Bukan harga akhir terendah dan alasan kosong (`PBJ-DEC-085`) | "Vendor ini bukan penawar terendah. Tuliskan alasan memilihnya." | 422 `PBJ-VAL-142` |
| Vendor tidak layak saat penetapan | Ajukan penetapan | Diblokir, legalitas tidak lengkap, atau nonaktif (`PBJ-DEC-076`) | "{vendor} tidak dapat ditetapkan: {sebab}." | 422 `PBJ-VAL-143` |
| Pecah tidak diizinkan | Pecah paket | Status tidak sah, persetujuan berjalan, penetapan diajukan, atau semua item dipilih (`PBJ-DEC-083`) | "Paket ini tidak dapat dipecah sekarang: {sebab}." | 409 `PBJ-VAL-144` |
| Nomor PO ganda | Catat nomor PO | Nomor sudah tercatat di paket lain (`PBJ-DEC-011`) | "Nomor PO {nomor} sudah tercatat pada paket {paket}." | 409 `PBJ-VAL-145` |
| Nomor PO terlalu dini / cabut setelah PO | Catat nomor PO, cabut penetapan | Penetapan belum disetujui, atau PO sudah dicatat | "Nomor PO hanya dicatat setelah penetapan disetujui, dan penetapan tidak dapat dicabut setelah PO dicatat." | 409 `PBJ-VAL-146` |
| Batal setelah PO | Batalkan paket | Paket `ORDERED` (`PBJ-DEC-077`) | "Paket yang sudah dipesan tidak dapat dibatalkan di Pengadaan." | 409 `PBJ-VAL-147` |
| Alasan wajib | Batal, pecah, buka ulang, RFQ gagal, perpanjang, cabut, batalkan catatan penawaran | Alasan kosong | "Tuliskan alasannya." | 422 `PBJ-VAL-148` |
| HPS terkunci | Ubah HPS/metode | Paket sudah `READY_FOR_RFQ` atau sesudahnya (`PBJ-DEC-073`) | "HPS sudah disetujui. Buka ulang persiapan untuk mengubahnya." | 409 `PBJ-VAL-149` |

**Contoh:** HPS PKT-000012 Rp 42.750.000; penawaran terendah sah Rp 44.000.000 (termasuk PPN). Andi mengajukan penetapan tanpa
alasan → 422 `PBJ-VAL-141`. Setelah negosiasi turun ke Rp 41.250.000 → pengajuan diterima.

## 2. Penolakan tanpa kode khusus

| Keadaan | Jawaban |
| --- | --- |
| Paket di luar cakupan unit pengguna, atau evaluator membuka paket yang tidak ditugaskan | 404 (tidak membocorkan keberadaan paket) |
| Tanpa izin endpoint | 403 dari mesin hak akses |
| `Idempotency-Key` sama dengan isi berbeda | 409 sesuai konvensi `PBJ-PP-API` |
