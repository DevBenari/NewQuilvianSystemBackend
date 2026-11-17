# Register Aset — Matriks Uji Penerimaan

| Field | Value |
|---|---|
| `last_changed_in` | `AST-REG-1.0.0` — `draft` |
| Owner | Rizki |
| Jenis | `UNIT` = pemeriksaan logika murni dengan contoh berangka; `INTEG` = verifikasi API + database pada lingkungan uji; `UI` = live browser; `UAT` = tim UAT. Untuk backend, seluruhnya **skenario verifikasi** (manual/runtime/HTTP), bukan kewajiban automated test — `rules/backend/TEST_POLICY.md`. Untuk frontend mengikuti `rules/frontend/test-policy.md` |

Data uji memakai nama samaran. Rupiah adalah ilustrasi.

## 1. Jalur berhasil

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
|---|---|---|---|---|
| `AT-REG-01` | `AST-AC-001`, `AST-DEC-043` | Buat 40 draft kursi, setujui 38 sekaligus | INTEG | 38 nomor `AST-MBL-######` berurutan, 2 draft tanpa nomor |
| `AT-REG-02` | `AST-AC-002` | Kursi Rp 450.000 dengan batas Rp 1.000.000 disetujui | INTEG | Buku nilai `IsCapitalized=false` |
| `AT-REG-03` | `AST-DEC-040` | Kursi roda hibah Rp 3.000.000 disetujui | INTEG | Satu baris kotak keluar `AST-PEROLEHAN-HIBAH` berstatus `Pending` |
| `AT-REG-04` | `AST-AC-005`, `AST-DEC-044` | Serahkan 5 kursi roda ke Poli Anak lalu dikonfirmasi | INTEG | Penempatan berjalan baru, `PlacementState=Ditempatkan`, 5 kejadian linimasa |
| `AT-REG-05` | `AST-DES-006` | Pindah ruangan dalam unit yang sama lewat penyerahan | INTEG | Penempatan lama ditutup, unit tidak berubah |
| `AT-REG-06` | `AST-RULE-010` | Lapor Rusak Ringan dengan foto, ditetapkan Petugas Aset | INTEG | Kondisi berubah hanya setelah ditetapkan |
| `AT-REG-07` | `AST-RULE-011` | Lapor Hilang + berita acara, disetujui Kepala Bagian Aset | INTEG | Kondisi `Hilang` |
| `AT-REG-08` | `AST-RULE-013` | Sewa ATM Rp 12 jt 12 bulan → Kepala Bagian Aset setuju; tanggal mulai tiba | INTEG | `Disetujui` → `Berjalan` oleh pekerja harian |
| `AT-REG-09` | `AST-RULE-013` | KSO 60 bulan diteruskan lalu disetujui Direktur | INTEG | `MenungguDirektur` → `Disetujui` |
| `AT-REG-10` | `AST-DEC-037`, `050` | Impor 2.400 baris benar, dua persetujuan | INTEG | 2.400 aset `Terdaftar` + buku nilai saldo awal; **nol** kotak keluar |
| `AT-REG-11` | `AST-AC-004` | Nomor tetap setelah aset dimutasi (bersama `pergerakan`) | INTEG | `InventoryNumber` tidak berubah |
| `AT-REG-12` | `AST-DEC-025` | Kepala Unit IGD membuka daftar | INTEG | Hanya aset IGD; respons tanpa kolom rupiah |
| `AT-REG-13` | `CAP-03.4` | Cetak 40 label | INTEG | PDF 40 label; QR terbaca berisi nomor inventaris saja |
| `AT-REG-14` | `CAP-08.3` | STNK habis 15 Des, hari ini 16 Nov, peringatan 30 hari | INTEG | Muncul di daftar dokumen hampir habis |
| `AT-REG-15` | `CAP-08.4` | Aset Tidak Digunakan sejak 91 hari lalu | INTEG | Muncul di pemantauan menganggur |
| `AT-REG-16` | `CAP-01.*` | Dashboard dengan data campuran | UI | Angka kartu sama dengan hitungan daftar bersaringan |

## 2. Jalur gagal

| ID | Requirement | Skenario | Jenis | Bukti yang diharapkan |
|---|---|---|---|---|
| `AT-REG-30` | `AST-AC-003`, `AST-VAL-REG-007` | Ajukan monitor tanpa izin edar | INTEG | 400; status tetap `Draft` |
| `AT-REG-31` | `AST-AC-011`, `AST-VAL-REG-012` | Pengaju menyetujui asetnya sendiri | INTEG | 403; status tetap `Diajukan` |
| `AT-REG-32` | `AST-VAL-REG-013` | Alokator gagal di tengah persetujuan | INTEG | 409; aset tetap `Diajukan`; nomor yang sempat terbit tidak dipakai ulang |
| `AT-REG-33` | `AST-VAL-REG-015` | Dua Kepala Bagian Aset menyetujui aset yang sama bersamaan | INTEG | Satu berhasil, satu 409; satu nomor saja |
| `AT-REG-34` | `AST-VAL-REG-052` | Buat kelompok berkode `MUT` | UNIT | 400 |
| `AT-REG-35` | `AST-VAL-REG-053` | Ubah kode kelompok yang sudah dipakai | INTEG | 409 |
| `AT-REG-36` | `AST-VAL-REG-024` | Serahkan aset yang sedang diusulkan dihapus | INTEG | 409 menyebut proses pemegang kunci |
| `AT-REG-37` | `AST-VAL-REG-025` | Kepala Unit lain mengonfirmasi penyerahan | INTEG | 403 |
| `AT-REG-38` | `AST-VAL-REG-026` | Serahkan ke unit yang sedang opname | INTEG | 409 |
| `AT-REG-39` | `AST-AC-008`, `AST-VAL-REG-032` | Setujui Hilang tanpa berita acara | INTEG | 400 |
| `AT-REG-40` | `AST-VAL-REG-035` | Laporan kedua saat laporan pertama terbuka | INTEG | 409 |
| `AT-REG-41` | `AST-VAL-REG-045` | Kepala Bagian Aset menyetujui KSO 60 bulan | INTEG | 403 |
| `AT-REG-42` | `AST-VAL-REG-044` | Ajukan pemanfaatan atas aset KSO milik vendor | INTEG | 409 |
| `AT-REG-43` | `AST-DEC-037` | Impor berisi 1 baris salah dari 2.400 | INTEG | `GagalValidasi`; nol aset |
| `AT-REG-44` | `AST-VAL-REG-067` | Pengunggah menyetujui batchnya sendiri | INTEG | 403 |
| `AT-REG-45` | Penerapan impor atomik | Kegagalan di baris ke-1.500 saat diterapkan | INTEG | Nol aset tersimpan; batch tetap `DisetujuiBagianAset` |
| `AT-REG-46` | `AST-VAL-REG-016` | Kepala Unit IGD membuka detail aset ICU lewat URL | INTEG | 403 |
| `AT-REG-47` | `AST-FE-004` | Kepala Unit memanggil `/amounts` | INTEG | 403 |
| `AT-REG-48` | `AST-VAL-REG-072` | Unggah foto saat pengaturan lampiran kosong | INTEG | 503 |
| `AT-REG-49` | Keterjangkauan | Seluruh 35 layar dapat dibuka dari menu atau layar induk | UI | Tidak ada layar yang hanya terbuka lewat URL |
