# Penghapusan & Pelepasan Aset — Matriks Validasi

| Field | Value |
|---|---|
| `last_changed_in` | `AST-HPS-1.0.0` — `draft` |
| Owner | Rizki · `approved_by`: — · `approved_at`: — |

| Kode | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | HTTP |
|---|---|---|---|---|---|
| `AST-VAL-HPS-001` | Aset terdaftar | Usulkan | Butir bukan `Terdaftar` | "Aset {nomor} tidak terdaftar atau sudah dihapus." | 409 |
| `AST-VAL-HPS-002` | Tanpa proses aktif | Usulkan | Butir terkunci | "Aset {nomor} sedang dalam proses {proses}." | 409 |
| `AST-VAL-HPS-003` | Syarat alasan | Usulkan | `RusakBerat` pada aset bukan `RusakBerat`; `Hilang` pada aset bukan `Hilang`; `Usang` pada aset `Hilang`. Contoh: mengusulkan monitor Aktif dengan alasan Rusak Berat | "Alasan {alasan} tidak sesuai kondisi aset {nomor} ({kondisi}). Tetapkan kondisinya lebih dulu." | 409 |
| `AST-VAL-HPS-004` | Unit pengusul | Usulkan oleh Kepala Unit | Aset bukan milik unitnya | "Anda hanya dapat mengusulkan aset unit Anda." | 403 |
| `AST-VAL-HPS-005` | Isian | Usulkan | Tanpa butir, > 200 butir, atau uraian kosong | "Pilih 1–200 aset dan isi uraian usulan." | 400 |
| `AST-VAL-HPS-010` | Tim | Tunjuk tim | Tanpa anggota atau pegawai nonaktif | "Tunjuk minimal satu pegawai aktif sebagai pemeriksa." | 400 |
| `AST-VAL-HPS-011` | Anggota tim | Catat kelayakan | Pengguna bukan anggota tim | "Anda bukan anggota tim pemeriksa usulan ini." | 403 |
| `AST-VAL-HPS-012` | Semua butir dinilai | Catat kelayakan | Ada butir tanpa hasil atau tanpa catatan | "Nilai kelayakan dan beri catatan pada setiap aset." | 400 |
| `AST-VAL-HPS-020` | Penyetuju independen | Setujui (Kepala Bagian Aset) | Pengguna pengusul atau anggota tim | "Persetujuan harus oleh Kepala Bagian Aset yang bukan pengusul dan bukan pemeriksa." | 403 |
| `AST-VAL-HPS-021` | Tahap Direktur | Setujui Direktur | Status bukan `MenungguPersetujuanDirektur` | "Usulan ini tidak memerlukan persetujuan Direktur." | 409 |
| `AST-VAL-HPS-030` | Metode | Catat pemindahtanganan | Metode kosong | "Pilih cara pelepasan: penjualan, hibah keluar, tukar tambah, atau pemusnahan." | 400 |
| `AST-VAL-HPS-031` | Nilai pelepasan | Catat | Penjualan/tukar tambah dengan nilai ≤ 0; hibah/pemusnahan dengan nilai ≠ 0 | "Penjualan dan tukar tambah wajib bernilai; hibah keluar dan pemusnahan bernilai Rp 0." | 400 |
| `AST-VAL-HPS-032` | Pihak dan bukti | Catat | Penjualan/hibah/tukar tambah tanpa nama pihak; tanpa lampiran berita acara serah terima/pemusnahan | "Isi pihak penerima dan lampirkan berita acara pelaksanaan." | 400 |
| `AST-VAL-HPS-033` | Tanggal | Catat | Tanggal pelepasan sebelum tanggal disetujui atau di masa depan | "Tanggal pelepasan harus antara tanggal persetujuan dan hari ini." | 400 |
| `AST-VAL-HPS-034` | Alasan Hilang | Catat | Usulan beralasan Hilang | "Aset hilang tidak dicatat pemindahtanganannya; langsung finalisasi." | 409 |
| `AST-VAL-HPS-040` | Penyusutan mutakhir | Finalisasi | Penyusutan bulan sebelum hari ini belum diproses. Contoh: final 10 Agustus, Juli belum diproses | "Proses penyusutan {bulan} terlebih dahulu agar nilai buku saat penghapusan benar." | 409 |
| `AST-VAL-HPS-041` | Semua butir dicatat | Finalisasi | Ada butir aktif tanpa pemindahtanganan (alasan bukan Hilang) | "Masih ada {jumlah} aset yang belum dicatat pelepasannya." | 409 |
| `AST-VAL-HPS-050` | Pemulihan dari final | Ajukan pemulihan | Butir bukan dari usulan `Final`, atau sudah dipulihkan | "Hanya aset yang sudah dihapus final yang dapat dipulihkan." | 409 |
| `AST-VAL-HPS-051` | Bukti pemulihan | Ajukan pemulihan | Butir dijual/dihibahkan/ditukar tanpa berita acara pembatalan (`AST-DES-037`) | "Lampirkan berita acara pembatalan pelepasan untuk aset yang sudah dijual, dihibahkan, atau ditukar." | 400 |
| `AST-VAL-HPS-052` | Larangan menyetujui sendiri | Putuskan pemulihan | Penyetuju = pengaju | "Anda tidak dapat menyetujui pengajuan Anda sendiri." | 403 |
| `AST-VAL-HPS-090` | Alasan wajib | Tolak, batalkan | Kosong/ > 500 | "Alasan wajib diisi, paling panjang 500 karakter." | 400 |
| `AST-VAL-HPS-091` | Batas pembatalan | Batalkan | Ada pemindahtanganan tercatat, atau pengusul membatalkan sesudah `DalamPemeriksaan` | "Usulan tidak dapat dibatalkan pada tahap ini." | 409 |
| `AST-VAL-HPS-092` | Status | Semua langkah | Tidak sesuai matriks | "Usulan ini sudah {status}." | 409 |
| `AST-VAL-HPS-093` | Data berubah | Semua langkah | `RowVersion` berbeda | "Data ini baru saja diubah orang lain. Muat ulang lalu coba lagi." | 409 |
