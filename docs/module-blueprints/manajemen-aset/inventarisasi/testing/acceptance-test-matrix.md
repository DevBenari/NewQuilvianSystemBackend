# Inventarisasi Fisik — Matriks Uji Penerimaan

| Field | Value |
|---|---|
| `last_changed_in` | `AST-INV-1.0.0` — `draft` |

## 1. Jalur berhasil

| ID | Requirement | Skenario | Jenis | Bukti |
|---|---|---|---|---|
| `AT-INV-01` | `AST-RULE-030` | Jadwalkan IGD dengan tim 2 orang | INTEG | `Dijadwalkan`, satu ketua |
| `AT-INV-02` | `AST-RULE-031` | Mulai; snapshot 312 aset | INTEG | 312 baris diharapkan; perubahan aset sesudahnya tidak mengubah snapshot |
| `AT-INV-03` | `AST-RULE-023` | Selama `Berjalan`, ajukan mutasi aset IGD | INTEG | 409 `AST-VAL-MOV-006` |
| `AT-INV-04` | `AST-RULE-031` | Pindai dan tutup dengan data contoh `01-pemeriksaan-dan-pencocokan.md` | INTEG | Jumlah per kategori sama dengan contoh |
| `AT-INV-05` | `AST-DES-027` | Pompa dipinjam ICU dipindai di ICU | INTEG | `SesuaiSedangDipinjam` |
| `AT-INV-06` | `AST-DES-029` | Pindai ulang aset dengan kondisi berbeda | INTEG | Pindaian lama `IsLatest=false`; hasil memakai yang baru |
| `AT-INV-07` | `AST-DEC-019` | Verifikasi | INTEG | 7 usulan; **nol** perubahan lokasi/kondisi aset |
| `AT-INV-08` | `AST-DES-007` | Proses usulan beda unit | INTEG | Mutasi `KoreksiOpname` `Diajukan` |
| `AT-INV-09` | `AST-DES-007` | Proses usulan beda ruangan | INTEG | Penyerahan `KoreksiOpname` `Diserahkan` |
| `AT-INV-10` | `AST-RCG-G10` | Proses temuan | INTEG | Draft aset dengan foto temuan |
| `AT-INV-11` | `AST-DEC-031` | Sesi lewat tanggal belum diverifikasi | INTEG | Kartu 1.5 bertambah |
| `AT-INV-12` | `CAP-07.6` | Cetak berita acara | INTEG | PDF memuat ringkasan per kategori dan nama/waktu verifikator |
| `AT-INV-13` | `AST-FE-003` | Pindai dari HP lewat kamera | UI | Nomor terbaca dan tersimpan |

## 2. Jalur gagal

| ID | Requirement | Skenario | Jenis | Bukti |
|---|---|---|---|---|
| `AT-INV-30` | `AST-VAL-INV-006` | Mulai saat ada mutasi belum diterima | INTEG | 409 berisi nomor mutasi |
| `AT-INV-31` | `AST-VAL-INV-007` | Sesi kedua untuk unit yang sama | INTEG | 409 |
| `AT-INV-32` | `AST-VAL-INV-010` | Bukan anggota tim memindai | INTEG | 403 |
| `AT-INV-33` | `AST-VAL-INV-011` | Pindai sesudah ditutup | INTEG | 409 |
| `AT-INV-34` | `AST-VAL-INV-021` | Ajukan dengan selisih tanpa catatan | INTEG | 400 |
| `AT-INV-35` | `AST-VAL-INV-022` | Kepala Bagian Aset anggota tim memverifikasi | INTEG | 403 |
| `AT-INV-36` | `AST-VAL-INV-031` | Proses usulan atas aset terkunci | INTEG | 409; usulan tetap `Usulan` |
| `AT-INV-37` | Status tidak sah | Ubah hasil sesi `Diverifikasi` | INTEG | 409 |
| `AT-INV-38` | Kamera ditolak | Izin kamera ditolak | UI | Pesan dan input manual tetap bekerja |
