# Nilai Aset — Matriks Validasi

| Field | Value |
|---|---|
| `last_changed_in` | `AST-NLI-1.0.0` — `draft` |
| Owner | Rizki · `approved_by`: — · `approved_at`: — |

## 1. Parameter dan kebijakan

| Kode | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | HTTP |
|---|---|---|---|---|---|
| `AST-VAL-NLI-001` | Angka parameter | Tetapkan parameter | Nilai < 0 atau > 2 desimal | "Batas wajib angka rupiah 0 atau lebih." | 400 |
| `AST-VAL-NLI-002` | Berlaku ke depan | Tetapkan parameter/kebijakan | Tanggal berlaku bukan awal bulan berikutnya (kecuali baris pertama, `AST-DES-032`) | "Parameter baru berlaku mulai {bulan berikutnya}." | 400 |
| `AST-VAL-NLI-003` | Satu baris per periode | Tetapkan | Sudah ada baris untuk periode berlaku yang sama | "Sudah ada parameter yang akan berlaku pada {bulan}. Ubah bulan depan atau hubungi administrator." | 409 |
| `AST-VAL-NLI-004` | Umur dan residu | Tetapkan kebijakan | Umur di luar 1–600 bulan; residu di luar 0–100% | "Umur manfaat 1–600 bulan dan nilai residu 0–100%." | 400 |
| `AST-VAL-NLI-005` | Kelompok dapat disusutkan | Tetapkan kebijakan | Kategori kelompok `IsDepreciable=false` (misalnya Tanah) | "Kelompok ini tidak disusutkan." | 400 |
| `AST-VAL-NLI-006` | Kode akun kontrol | Tetapkan akun kontrol | Kode tidak ada atau nonaktif di daftar akun Akuntansi | "Kode akun {kode} tidak ditemukan di daftar akun Akuntansi." | 400 |
| `AST-VAL-NLI-007` | Pusat biaya | Pemetaan | Pusat biaya nonaktif | "Pilih pusat biaya yang masih aktif." | 400 |
| `AST-VAL-NLI-008` | Parameter tersedia | Aset disetujui (dipanggil `register`) | Belum ada parameter berlaku | "Parameter nilai belum ditetapkan Akuntansi; aset belum dapat disetujui." | 409 |

## 2. Proses penyusutan

| Kode | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | HTTP |
|---|---|---|---|---|---|
| `AST-VAL-NLI-010` | Bukan masa depan | Pratinjau, proses | Periode > bulan berjalan | "Penyusutan {bulan} belum dapat diproses." | 409 |
| `AST-VAL-NLI-011` | Belum diproses | Proses | Proses periode itu sudah ada | "Penyusutan {bulan} sudah diproses pada {tanggal} oleh {nama}." | 409 |
| `AST-VAL-NLI-012` | Berurutan | Proses | Periode sebelumnya belum diproses | "Proses penyusutan {bulan sebelumnya} terlebih dahulu." | 409 |
| `AST-VAL-NLI-013` | Pratinjau bersih | Proses | Ada galat pratinjau | "Masih ada {jumlah} galat pada pratinjau. Selesaikan sebelum memproses." | 409 |
| `AST-VAL-NLI-014` | Pusat biaya jelas (galat pratinjau) | Pratinjau | Unit aset tanpa pemetaan dan punya 0 atau > 1 pusat biaya aktif. Contoh: unit Rawat Inap punya pusat biaya "RI Lt 2" dan "RI Lt 3" | "Unit {unit} belum jelas pusat biayanya. Petakan di Parameter Nilai." | — |
| `AST-VAL-NLI-015` | Kebijakan kelompok (galat pratinjau) | Pratinjau | Aset dikapitalisasi tanpa kebijakan berlaku untuk kelompoknya | "Kelompok {kode} belum punya umur manfaat." | — |
| `AST-VAL-NLI-016` | Akun kontrol (peringatan pratinjau) | Pratinjau | Kelompok tanpa akun kontrol | "Saldo kelompok {kode} tidak akan dikirim untuk rekonsiliasi." | — |

## 3. Perubahan nilai

| Kode | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | HTTP |
|---|---|---|---|---|---|
| `AST-VAL-NLI-020` | Aset bernilai | Ajukan | Buku tidak dikapitalisasi atau `Dihentikan` | "Aset ini tidak punya nilai buku yang dapat diubah." | 409 |
| `AST-VAL-NLI-021` | Satu pengajuan terbuka | Ajukan | Ada pengajuan `Diajukan` untuk aset ini | "Aset ini masih punya pengajuan perubahan nilai yang belum diputuskan." | 409 |
| `AST-VAL-NLI-022` | Bukti | Ajukan | Tanpa lampiran; `PerbaikanBesar` tanpa nomor faktur | "Lampirkan bukti; perbaikan besar wajib mencantumkan nomor faktur." | 400 |
| `AST-VAL-NLI-023` | Nilai positif | Ajukan | Nilai ≤ 0 untuk perbaikan besar/penurunan nilai; harga koreksi ≤ 0 | "Nilai harus lebih dari Rp 0." | 400 |
| `AST-VAL-NLI-024` | Batas penurunan nilai | Ajukan, setujui | Penurunan > nilai buku − residu. Contoh: nilai buku Rp 18 jt, residu 0, penurunan Rp 20 jt | "Penurunan nilai melebihi nilai buku yang tersisa ({nilai})." | 400 |
| `AST-VAL-NLI-025` | Estimasi baru wajar | Ajukan perubahan estimasi | Umur baru ≤ bulan yang sudah disusutkan; residu baru > biaya tercatat | "Umur baru harus lebih panjang dari {bulan} bulan yang sudah disusutkan." | 400 |
| `AST-VAL-NLI-026` | Larangan menyetujui sendiri | Setujui, tolak | Penyetuju = pengaju | "Anda tidak dapat menyetujui pengajuan Anda sendiri." | 403 |
| `AST-VAL-NLI-027` | Status | Semua langkah | Tidak sesuai matriks | "Pengajuan ini sudah {status}." | 409 |
| `AST-VAL-NLI-028` | Data berubah | Semua langkah | `RowVersion` berbeda | "Data ini baru saja diubah orang lain. Muat ulang lalu coba lagi." | 409 |

## 4. Saldo awal dan kotak keluar

| Kode | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | HTTP |
|---|---|---|---|---|---|
| `AST-VAL-NLI-040` | Saldo awal konsisten (dipanggil validasi impor) | Validasi baris impor | Sisa umur ≤ 0 tetapi nilai buku > residu | Baris {n}: "Umur sudah habis tetapi nilai buku masih tersisa; periksa akumulasi." | baris galat |
| `AST-VAL-NLI-050` | Coba ulang | Coba ulang kotak keluar | Status bukan `Failed` | "Hanya kiriman yang gagal yang dapat dicoba ulang." | 409 |
| `AST-VAL-NLI-051` | Kirim ulang saldo | Kirim ulang | Periode belum diproses | "Penyusutan {bulan} belum diproses." | 409 |
