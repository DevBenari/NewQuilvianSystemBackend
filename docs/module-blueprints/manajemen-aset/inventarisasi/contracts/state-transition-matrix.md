# Inventarisasi Fisik — Matriks Perubahan Status

| Field | Value |
|---|---|
| `last_changed_in` | `AST-INV-1.0.0` — `draft` |
| Owner | Rizki · `approved_by`: — · `approved_at`: — |

## 1. Sesi opname (`StockTakeStatus`)

| Dari | Tindakan | Ke | Siapa | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Jadwalkan | `Dijadwalkan` | Petugas Aset, Kepala Bagian Aset | Unit aktif; tanggal akhir ≥ mulai; tim ≥ 1 orang dengan tepat satu ketua | `AST-VAL-INV-001..004` |
| `Dijadwalkan` | Ubah jadwal | `Dijadwalkan` | Petugas Aset | — | — |
| `Dijadwalkan` | Mulai | `Berjalan` | Ketua tim atau Petugas Aset | Hari ini ≥ tanggal mulai rencana; unit tanpa penyerahan/mutasi/pinjaman yang belum selesai (`AST-DES-008`); tidak ada sesi lain unit itu yang `Berjalan`..`MenungguVerifikasi` | `AST-VAL-INV-005`, `006`, `007` |
| `Berjalan` | Pindai / catat temuan | `Berjalan` | Anggota tim | — | `AST-VAL-INV-010..014` |
| `Berjalan` | Tutup pemindaian | `Pencocokan` | Ketua tim | Hasil dihitung | — |
| `Pencocokan` | Buka kembali | `Berjalan` | Ketua tim | Alasan | `AST-VAL-INV-020` |
| `Pencocokan` | Ajukan verifikasi | `MenungguVerifikasi` | Ketua tim | Setiap hasil bukan `Sesuai` punya catatan tim | `AST-VAL-INV-021` |
| `MenungguVerifikasi` | Verifikasi (berita acara disetujui) | `Diverifikasi` | Kepala Bagian Aset, **bukan** anggota tim | — | `AST-VAL-INV-022` |
| `MenungguVerifikasi` | Kembalikan | `Pencocokan` | Kepala Bagian Aset | Alasan | `AST-VAL-INV-020` |
| `Dijadwalkan`, `Berjalan`, `Pencocokan`, `MenungguVerifikasi` | Batalkan | `Dibatalkan` | Kepala Bagian Aset | Alasan | `AST-VAL-INV-020` |

Penahanan unit (`AST-RULE-023`) aktif pada `Berjalan`, `Pencocokan`, `MenungguVerifikasi`; lepas pada
`Diverifikasi` dan `Dibatalkan`. Pada `Diverifikasi`: usulan tindak lanjut dibuat, linimasa `Diopname`
ditulis untuk setiap aset yang diharapkan, berita acara dapat dicetak.

**Tidak sah:** `Diverifikasi` → apa pun (koreksi = sesi baru); `Dibatalkan` → apa pun; memindai di luar
`Berjalan`; mengubah snapshot sesudah `Berjalan`.

## 2. Hasil pencocokan (dihitung, bukan dipilih)

| Kategori | Aturan | Usulan saat diverifikasi |
|---|---|---|
| `Sesuai` | Dipindai di ruangan yang diharapkan, kondisi sama | — |
| `SesuaiSedangDipinjam` | Aset dipinjam keluar dan dipindai di unit peminjam | — |
| `TidakDitemukan` | Diharapkan, tidak dipindai | `UsulanHilang` |
| `BedaLokasi` | Dipindai di ruangan/unit lain; atau aset unit lain ditemukan di unit sesi (`AST-DES-028`) | `KoreksiLokasi` (+ `PerubahanKondisi` bila kondisi juga beda) |
| `BedaKondisi` | Lokasi sesuai, kondisi berbeda | `PerubahanKondisi` |
| `TidakTerdaftar` | Temuan tanpa nomor atau nomor tidak dikenal | `PendaftaranTemuan` |

## 3. Usulan tindak lanjut (`StockTakeFollowUpStatus`)

| Dari | Tindakan | Ke | Siapa | Akibat |
|---|---|---|---|---|
| — | Sesi diverifikasi | `Usulan` | Sistem | — |
| `Usulan` | Proses | `Diproses` | Petugas Aset, Kepala Bagian Aset | `KoreksiLokasi` beda unit → mutasi `Diajukan` beralasan `KoreksiOpname`; beda ruangan → penyerahan `KoreksiOpname`; `UsulanHilang` → laporan kondisi Hilang (tetap butuh berita acara kehilangan); `PerubahanKondisi` → laporan kondisi; `PendaftaranTemuan` → draft aset |
| `Usulan` | Abaikan | `Diabaikan` | Kepala Bagian Aset | Alasan wajib |

**Tidak sah:** memproses usulan dua kali; mengabaikan usulan `Diproses`.
