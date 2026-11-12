# Validation Matrix — Sub-modul Payroll Gaji

| Field | Nilai |
|---|---|
| Blueprint | `PRL-BP-001` / `payroll-gaji`, revision `1` |
| Status | `approved` |
| `last_changed_in` | `PGJ-C1` |
| Owner | Yasmin; `approved_by`: Yasmin; `approved_at`: 2026-11-12 |
| `input_revision` | `02-backend-architecture.md` r1, `contracts/state-transition-matrix.md` r1 |
| Traceability | PRL-DEC-02, 04, 07, 12, 13, 31, 32, 46; `EPIC PGJ-01` s.d. `PGJ-07` |

Pesan ditulis sebagaimana dibaca petugas. Kode di kolom terakhir adalah kode HTTP; artinya bagi
pengguna dijelaskan di bagian akhir.

## 1. Baris tunjangan struktur

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `SA-01` | Tambah, ubah baris | Metode bukan `FixedAmount` atau `PercentageOfBaseSalary` | "Cara hitung tunjangan harus nominal tetap atau persentase gaji pokok." | 400 |
| `SA-02` | Tambah, ubah baris | Metode nominal tetap dan nominal ≤ 0 | "Nominal tunjangan harus lebih dari nol." | 400 |
| `SA-03` | Tambah, ubah baris | Metode persentase dan persen ≤ 0 atau > 100 | "Persentase tunjangan harus di atas 0 dan paling tinggi 100." | 400 |
| `SA-04` | Tambah, ubah baris | Jenis tunjangan sudah ada di struktur yang sama | "Tunjangan {nama jenis} sudah ada di struktur ini. Ubah baris yang sudah ada." | 409 |
| `SA-05` | Tambah baris | Struktur atau jenis tunjangan tidak aktif | "Struktur gaji atau jenis tunjangan tidak aktif." | 400 |
| `SA-06` | Tambah, ubah baris | Jenis tunjangan dipakai kebijakan Shift, Siaga, atau Risiko yang aktif | "Tunjangan {nama jenis} dihitung dari aktivitas kerja, jadi tidak boleh dipasang sebagai tunjangan tetap struktur." | 400 |
| `SA-07` | Ubah, hapus baris | Token versi tidak cocok | "Data ini baru saja diubah petugas lain. Muat ulang lalu coba lagi." | 409 |

**Contoh `SA-03`:** petugas mengisi Tunjangan Keahlian 150%. Ditolak, karena persentase gaji
pokok paling tinggi 100.

## 2. Batas gaji struktur dan grade

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `SR-01` | Simpan Struktur Gaji | Batas bawah struktur < batas bawah grade | "Batas bawah gaji struktur (Rp {a}) lebih rendah dari batas bawah grade {kode} (Rp {b})." | 400 |
| `SR-02` | Simpan Struktur Gaji | Batas atas struktur > batas atas grade | "Batas atas gaji struktur (Rp {a}) melebihi batas atas grade {kode} (Rp {b})." | 400 |
| `SR-03` | Simpan Struktur Gaji | Gaji pokok bawaan di luar rentang grade, atau di luar batas struktur yang terisi | "Gaji pokok bawaan Rp {a} berada di luar rentang Rp {min} – Rp {maks}." | 400 |
| `SR-04` | Ubah Grade Gaji | Rentang baru membuat struktur aktif keluar batas | "Rentang baru membuat {n} struktur aktif keluar batas: {daftar kode}. Sesuaikan struktur dulu." | 409 |

**Contoh `SR-02`:** grade GR-05 berbatas Rp 6.000.000 sampai Rp 9.000.000. Struktur "GR-05
Cabang A" disimpan dengan batas atas Rp 10.000.000, maka ditolak. Batas atas Rp 9.000.000 atau
lebih rendah diterima.

## 3. Penghasilan variabel manual

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `VI-01` | Semua aksi petugas | Run tidak berstatus `Draft`, `CollectingInput`, atau `Calculated` | "Run ini sudah tidak menerima penghasilan variabel." | 400 |
| `VI-02` | Buat, impor | Pegawai tidak terdaftar di run | "Pegawai {nomor} tidak ada di run periode ini." | 400 |
| `VI-03` | Buat, ubah, impor | Komponen tidak aktif atau bukan penghasilan | "Komponen {kode} tidak aktif atau bukan jenis penghasilan." | 400 |
| `VI-04` | Buat, ubah, impor | Nominal ≤ 0 | "Nominal harus lebih dari nol." | 400 |
| `VI-05` | Buat, ubah, impor | Jenis input bukan `Incentive`, `Bonus`, atau `AdditionalDutyHonorarium` | "Jenis input harus Insentif, Bonus, atau Honor Tugas Tambahan." | 400 |
| `VI-06` | Buat, ubah, impor | Honor tugas tambahan tanpa alasan penugasan | "Honor tugas tambahan wajib menyebut tugasnya." | 400 |
| `VI-07` | Tolak, batalkan | Alasan kosong | "Alasan wajib diisi." | 400 |
| `VI-08` | Impor | Ada satu atau lebih baris gagal | "Impor dibatalkan. {n} baris bermasalah; tidak ada baris yang tersimpan." beserta daftar baris dan sebabnya | 400 |
| `VI-09` | Semua aksi | Transisi tidak sah (lihat `state-transition-matrix.md`) | "Input berstatus {status} tidak dapat {aksi}." | 409 |
| `VI-10` | Ajukan run | Masih ada input manual `Submitted` | "Masih ada {n} penghasilan variabel yang menunggu keputusan. Setujui atau tolak dulu." | 409 |
| `VI-11` | Ajukan run | Ada input manual yang berubah setelah perhitungan terakhir | "Penghasilan variabel berubah setelah perhitungan terakhir. Hitung ulang run dulu." | 409 |
| `VI-12` | Ubah, aksi status | Token versi tidak cocok | "Data ini baru saja diubah petugas lain. Muat ulang lalu coba lagi." | 409 |

**Contoh `VI-08`:** file impor berisi 120 baris. Baris 17 menyebut pegawai yang tidak ada di
run, dan baris 64 bernominal 0. Seluruh impor dibatalkan; petugas memperbaiki dua baris itu lalu
mengimpor ulang 120 baris.

## 4. Perhitungan run

Bukan penolakan permintaan, melainkan galat dan peringatan per pegawai pada hasil hitung.

| Aturan | Kondisi | Bunyi pada hasil hitung | Jenis |
|---|---|---|---|
| `CALC-01` | Dua tunjangan transport bulanan aktif beririsan untuk satu pegawai | "Pegawai punya lebih dari satu tunjangan transport bulanan yang berlaku. Perbaiki di Tunjangan Transportasi." | Galat — pegawai tidak dihitung |
| `CALC-02` | Tunjangan transport bermetode per kehadiran, per hari, atau penggantian | "Tunjangan transport metode {metode} belum dihitung otomatis. Masukkan lewat Penghasilan Variabel." | Peringatan |
| `CALC-03` | Struktur kosong, angka profil dipakai | "Tunjangan tetap memakai angka profil karena Struktur Gaji belum memuat tunjangan." | Peringatan |
| `CALC-04` | Struktur berisi, profil juga berisi | "Angka tunjangan di profil diabaikan karena Struktur Gaji sudah memuat tunjangan." | Peringatan |

## 5. Simulasi dan laporan

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `SIM-01` | Simulasi | Periode tidak ditemukan | "Periode penggajian tidak ditemukan." | 404 |
| `SIM-02` | Simulasi | Pegawai yang dipilih lebih dari batas | "Simulasi paling banyak {n} pegawai sekali jalan." | 400 |
| `SIM-03` | Simulasi | Tidak ada pegawai yang layak dihitung | "Tidak ada pegawai dengan penetapan gaji yang berlaku di periode ini." | 400 |
| `RPT-01` | Laporan beban gaji, laporan PPh dan BPJS | Belum ada run yang disahkan di periode itu | Bukan galat: hasil kosong dengan keterangan "Belum ada run yang disahkan pada periode ini." | 200 |

## 6. Arti kode bagi pengguna

| Kode | Arti |
|---|---|
| 200 | Berhasil |
| 400 | Isian atau keadaan data tidak memenuhi aturan; petugas memperbaiki isian |
| 403 | Pengguna tidak punya hak untuk tindakan ini; minta admin mengatur di Akses Role |
| 404 | Data tidak ditemukan atau sudah dihapus |
| 409 | Data berbenturan dengan keadaan terbaru (diubah petugas lain, status sudah berubah, atau ada data ganda); petugas memuat ulang |
