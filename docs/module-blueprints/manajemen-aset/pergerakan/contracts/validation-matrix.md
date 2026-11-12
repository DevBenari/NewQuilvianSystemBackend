# Pergerakan Aset — Matriks Validasi

| Field | Value |
|---|---|
| `last_changed_in` | `AST-MOV-1.0.0` — `draft` |
| Owner | Rizki · `approved_by`: — · `approved_at`: — |

| Kode | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | HTTP |
|---|---|---|---|---|---|
| `AST-VAL-MOV-001` | Aset terdaftar dan ditempatkan | Ajukan mutasi/pinjaman | Butir bukan `Terdaftar` atau `BelumDitempatkan` | "Aset {nomor} belum terdaftar atau belum ditempatkan." | 409 |
| `AST-VAL-MOV-002` | Kondisi layak | Ajukan | Butir `Hilang` atau `RusakBerat`. Contoh: ventilator Rusak Berat | "Aset {nomor} berstatus {kondisi} dan tidak dapat dipindahkan." | 409 |
| `AST-VAL-MOV-003` | Satu proses aktif | Ajukan | Butir terkunci proses lain | "Aset {nomor} sedang dalam proses {proses}." | 409 |
| `AST-VAL-MOV-004` | Satu unit asal | Ajukan mutasi | Butir berasal dari lebih dari satu unit | "Satu mutasi hanya untuk aset dari satu unit asal." | 400 |
| `AST-VAL-MOV-005` | Unit tujuan berbeda | Ajukan | Unit tujuan/peminjam = unit asal/pemilik | "Unit tujuan harus berbeda dari unit asal. Untuk pindah ruangan gunakan penyerahan." | 400 |
| `AST-VAL-MOV-006` | Unit sedang opname (sejak `MVP-3`) | Ajukan | Unit asal atau tujuan punya sesi opname `Berjalan`, `Pencocokan`, atau `MenungguVerifikasi` | "Aset sedang dalam inventarisasi. Pengajuan dapat dilakukan setelah inventarisasi unit {unit} selesai." | 409 |
| `AST-VAL-MOV-007` | Jumlah butir | Ajukan | 0 atau > 200 butir | "Pilih 1 sampai 200 aset." | 400 |
| `AST-VAL-MOV-008` | Alasan pengajuan | Ajukan mutasi | Alasan kosong; `KoreksiMutasi` tanpa rujukan mutasi asal | "Alasan mutasi wajib diisi." | 400 |
| `AST-VAL-MOV-010` | Penyetuju yang berwenang | Setujui unit / pinjaman | Pengguna bukan Kepala Unit dari unit asal/pemilik menurut penugasan manajerial aktif | "Hanya Kepala Unit {unit} yang dapat menyetujui." | 403 |
| `AST-VAL-MOV-011` | Larangan menyetujui sendiri | Semua persetujuan | Penyetuju = pengaju (kecuali tahap 1 otomatis `AST-DEC-048`) | "Anda tidak dapat menyetujui pengajuan Anda sendiri." | 403 |
| `AST-VAL-MOV-012` | Penerima yang berwenang | Terima mutasi | Pengguna bukan Kepala Unit tujuan | "Hanya Kepala Unit {unit tujuan} yang dapat menerima mutasi ini." | 403 |
| `AST-VAL-MOV-013` | Ruangan dan penanggung jawab tujuan | Terima mutasi | Ruangan bukan milik unit tujuan, atau pegawai tidak aktif | "Pilih ruangan milik unit tujuan dan penanggung jawab yang masih aktif." | 400 |
| `AST-VAL-MOV-014` | Alasan wajib | Tolak, tolak terima, batalkan | Kosong atau > 500 karakter | "Alasan wajib diisi, paling panjang 500 karakter." | 400 |
| `AST-VAL-MOV-015` | Status mengizinkan | Semua langkah | Status dokumen tidak sesuai matriks perubahan status | "Pengajuan ini sudah {status}; tindakan tidak dapat dilakukan." | 409 |
| `AST-VAL-MOV-016` | Data sudah berubah | Semua langkah | `RowVersion` berbeda | "Data ini baru saja diubah orang lain. Muat ulang lalu coba lagi." | 409 |
| `AST-VAL-MOV-020` | Tanggal pinjaman | Ajukan pinjaman | Tanggal kembali < tanggal mulai, atau tanggal mulai di masa lalu lebih dari 1 hari | "Rencana tanggal kembali harus sama dengan atau sesudah tanggal mulai." | 400 |
| `AST-VAL-MOV-021` | Pelaku pihak peminjam | Ambil, kembalikan, minta perpanjangan | Pengguna bukan Kepala Unit peminjam | "Hanya Kepala Unit peminjam yang dapat melakukan langkah ini." | 403 |
| `AST-VAL-MOV-022` | Kondisi kembali | Konfirmasi kembali | Kondisi salah satu butir kosong | "Isi kondisi setiap aset yang dikembalikan." | 400 |
| `AST-VAL-MOV-023` | Satu perpanjangan terbuka | Minta perpanjangan | Ada perpanjangan `Diajukan` | "Masih ada permintaan perpanjangan yang belum diputuskan." | 409 |
