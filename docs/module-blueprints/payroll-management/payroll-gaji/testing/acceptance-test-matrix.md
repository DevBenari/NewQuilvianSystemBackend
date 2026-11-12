# Acceptance Test Matrix — Sub-modul Payroll Gaji

| Field | Nilai |
|---|---|
| Blueprint | `PRL-BP-001` / `payroll-gaji`, revision `1` |
| Status | `approved` |
| `last_changed_in` | `PGJ-C1` |
| Owner | Yasmin; `approved_by`: Yasmin; `approved_at`: 2026-11-12 |
| `input_revision` | `04-prd-to-mvp.md` r1, seluruh `contracts/` r1 |

Kebijakan uji: backend tidak memelihara project test otomatis
(`AUTOMATED TEST: NOT APPLICABLE`). Bukti backend berupa `dotnet build`, QBE conformance, uji
endpoint manual lewat Swagger, dan UAT. Test otomatis frontend bersifat opsional. Skenario UAT
lengkap (kondisi awal, langkah, hasil) ada di `04-prd-to-mvp.md` bagian 18.

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| FR-PGJ-001, 004 | `UAT-01` Tunjangan struktur muncul per jenis di slip, dengan prorata dan batas maksimum | UAT, uji API | Baris `PAY_STRUCTURE_ALLOWANCE` per jenis; angka sesuai contoh `02-backend-architecture.md` bagian 5.1 |
| FR-PGJ-002 | `UAT-02` **Gagal:** jenis tunjangan yang sama ditambahkan dua kali | Uji API, UAT | Kode 409, pesan `SA-04`; tidak ada baris kedua |
| FR-PGJ-003 | `UAT-02b` **Gagal:** jenis tunjangan yang dipakai kebijakan Shift ditambahkan | Uji API | Kode 400, pesan `SA-06` |
| FR-PGJ-005 | `UAT-03` Struktur kosong, profil berisi: angka profil dipakai dengan peringatan | UAT | Satu baris "Tunjangan Tetap" dan peringatan `CALC-03` |
| FR-PGJ-006 | `UAT-04` Struktur berisi, profil berisi: tidak terhitung ganda | UAT | Hanya baris struktur; peringatan `CALC-04`; total sama dengan jumlah baris struktur |
| FR-PGJ-010 | `UAT-05` Struktur dengan batas di dalam rentang grade tersimpan | UAT | Kode 200 |
| FR-PGJ-010 | `UAT-06` **Gagal:** batas atas struktur melebihi grade | Uji API, UAT | Kode 400, pesan `SR-02`; data lama tidak berubah |
| FR-PGJ-011 | `UAT-07` **Gagal:** rentang grade dipersempit sehingga struktur aktif keluar batas | Uji API, UAT | Kode 409, pesan `SR-04` menyebut kode struktur |
| FR-PGJ-020, 021, 022 | `UAT-08` Transport bulanan tetap diprorata | UAT | Baris `PAY_TRANSPORT_ALLOWANCE` Rp 266.667 untuk contoh 20 dari 30 hari |
| FR-PGJ-023 | `UAT-09` **Gagal:** dua transport bulanan beririsan | UAT | Pegawai gagal dihitung dengan `CALC-01`; pegawai lain tetap terhitung |
| FR-PGJ-024 | `UAT-10` Transport metode per kehadiran | UAT | Peringatan `CALC-02`; tidak ada baris transport |
| FR-PGJ-030–033, 035, 037 | `UAT-11` Impor, ajukan, setujui insentif; hitung; sahkan run | UAT, uji API | Nomor `PVI-...`; insentif masuk gaji; status akhir `Applied` |
| FR-PGJ-031 | `UAT-12` **Gagal:** impor dengan dua baris salah | Uji API, UAT | Kode 400, `VI-08` menyebut baris 17 dan 64; tidak ada baris tersimpan |
| FR-PGJ-033, 036 | `UAT-13` **Gagal:** run diajukan saat ada input `Submitted` | UAT | Kode 409, `VI-10` |
| FR-PGJ-036 | `UAT-14` **Gagal:** input disetujui setelah run dihitung, lalu run diajukan | UAT | Kode 409, `VI-11`; setelah hitung ulang pengajuan diterima |
| FR-PGJ-034 | `UAT-15` **Gagal:** honor tugas tambahan tanpa alasan | Uji API | Kode 400, `VI-06` |
| FR-PGJ-037 | `UAT-15b` Run dibatalkan | Uji API | Input manual `Draft`, `Submitted`, `Approved` menjadi `Cancelled` beralasan "Run dibatalkan" |
| FR-PGJ-040–042 | `UAT-16` Simulasi 48 pegawai | UAT | Hasil per pegawai tampil; setelahnya tidak ada run baru, input baru, atau perubahan periode |
| FR-PGJ-040 | `UAT-17` **Gagal:** simulasi 60 pegawai | Uji API | Kode 400, `SIM-02` |
| FR-PGJ-050 | `UAT-18` Laporan beban gaji per cost center | UAT | Jumlah seluruh baris = total kotor dan potongan run yang disahkan |
| FR-PGJ-052 | `UAT-19` **Gagal:** pengguna tanpa `PayrollReport : ViewAmount` membuka laporan | UAT | Butir menu tidak tampil; panggilan langsung kode 403 |
| FR-PGJ-051 | `UAT-20` Laporan PPh dan BPJS | UAT | Nilai sama dengan tetapan profil; sumber "Tetapan profil" |
| FR-PGJ-060 | `UAT-21` Rekap lembur per run | UAT | Baris sama dengan lembur terverifikasi yang diimpor |
| FR-PGJ-061 | `UAT-22` **Gagal:** pengguna mencoba mengubah tarif lembur dari Payroll | UAT | Tidak ada tombol ubah; pengguna tanpa `OvertimeRate : Read` tidak melihat butir menu |
| Semua | Kesesuaian teknis | `dotnet build`, QBE conformance strict | Build tanpa galat; 0 pelanggaran QBE yang memblokir |
| Semua | Keterjangkauan layar | Pemeriksaan menu | Enam butir menu baru dan empat jalan masuk layar anak ada |
