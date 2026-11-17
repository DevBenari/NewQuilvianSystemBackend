# Inventarisasi Fisik — Matriks Validasi

| Field | Value |
|---|---|
| `last_changed_in` | `AST-INV-1.0.0` — `draft` |
| Owner | Rizki · `approved_by`: — · `approved_at`: — |

| Kode | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | HTTP |
|---|---|---|---|---|---|
| `AST-VAL-INV-001` | Unit aktif | Jadwalkan | Unit tidak ada/nonaktif | "Pilih unit yang masih aktif." | 400 |
| `AST-VAL-INV-002` | Tanggal | Jadwalkan | Tanggal akhir < mulai | "Tanggal selesai harus sama dengan atau sesudah tanggal mulai." | 400 |
| `AST-VAL-INV-003` | Tim | Jadwalkan | Tanpa anggota, tanpa ketua, atau lebih dari satu ketua | "Tim wajib punya anggota dengan tepat satu ketua." | 400 |
| `AST-VAL-INV-004` | Ruangan milik unit | Jadwalkan | Ruangan opsional bukan milik unit | "Ruangan bukan milik unit yang dipilih." | 400 |
| `AST-VAL-INV-005` | Belum waktunya | Mulai | Hari ini < tanggal mulai rencana | "Sesi baru dapat dimulai pada {tanggal}." | 409 |
| `AST-VAL-INV-006` | Proses berjalan di unit | Mulai | Unit punya penyerahan, mutasi, atau pinjaman belum selesai. Contoh: mutasi `AST-MUT-000045` menunggu diterima ICU | "Selesaikan dulu {jumlah} proses yang masih berjalan di unit ini: {daftar nomor}." | 409 |
| `AST-VAL-INV-007` | Satu sesi aktif per unit | Mulai, jadwalkan | Unit sudah punya sesi `Berjalan`..`MenungguVerifikasi` | "Unit ini sedang diinventarisasi pada sesi {nomor}." | 409 |
| `AST-VAL-INV-010` | Anggota tim | Pindai, temuan | Pengguna bukan anggota tim | "Anda bukan anggota tim inventarisasi sesi ini." | 403 |
| `AST-VAL-INV-011` | Sesi berjalan | Pindai, temuan | Status bukan `Berjalan` | "Pemindaian sudah ditutup." | 409 |
| `AST-VAL-INV-012` | Kode dikenal | Pindai | Nomor tidak ditemukan | "Nomor {kode} tidak terdaftar. Catat sebagai temuan tidak terdaftar." | 404 |
| `AST-VAL-INV-013` | Ruangan ditemukan | Pindai | Ruangan kosong | "Pilih ruangan tempat aset ditemukan." | 400 |
| `AST-VAL-INV-014` | Aset di luar lingkup | Pindai | Aset dihapus | "Aset {nomor} sudah dihapus dari register." | 409 |
| `AST-VAL-INV-015` | Kondisi diamati | Pindai | Kondisi kosong | "Pilih kondisi aset saat diperiksa." | 400 |
| `AST-VAL-INV-016` | Temuan | Temuan tidak terdaftar | Deskripsi atau foto kosong | "Temuan wajib diberi keterangan dan foto." | 400 |
| `AST-VAL-INV-020` | Alasan wajib | Buka kembali, kembalikan, batalkan, abaikan | Kosong/ > 500 | "Alasan wajib diisi, paling panjang 500 karakter." | 400 |
| `AST-VAL-INV-021` | Catatan selisih | Ajukan verifikasi | Ada hasil bukan `Sesuai`/`SesuaiSedangDipinjam` tanpa catatan | "Beri catatan pada {jumlah} selisih sebelum mengajukan verifikasi." | 400 |
| `AST-VAL-INV-022` | Verifikator independen | Verifikasi | Pengguna anggota tim, atau bukan Kepala Bagian Aset | "Verifikasi harus dilakukan Kepala Bagian Aset yang bukan anggota tim." | 403 |
| `AST-VAL-INV-023` | Status sesi | Semua langkah | Tidak sesuai matriks | "Sesi ini sudah {status}; tindakan tidak dapat dilakukan." | 409 |
| `AST-VAL-INV-024` | Data berubah | Semua langkah | `RowVersion` berbeda | "Data ini baru saja diubah orang lain. Muat ulang lalu coba lagi." | 409 |
| `AST-VAL-INV-030` | Usulan masih terbuka | Proses, abaikan | Status bukan `Usulan` | "Usulan ini sudah {status}." | 409 |
| `AST-VAL-INV-031` | Dokumen tujuan dapat dibuat | Proses | Aset tujuan terkunci proses lain atau sudah dihapus | "Usulan belum dapat diproses: aset {nomor} sedang dalam proses {proses}." | 409 |
