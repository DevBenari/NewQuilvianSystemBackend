# Arsitektur Frontend — Sub-modul Payroll Gaji

| Field | Nilai |
|---|---|
| Blueprint | `PRL-BP-001` / `payroll-gaji`, revision `1` |
| Status | `approved` — `approved_by`: Yasmin; `approved_at`: 2026-11-12 |
| `last_changed_in` | `PGJ-C1` |
| Owner | Yasmin; frontend authority: Product Owner — **belum ada UI brief yang disetujui** |
| `input_revision` | `contracts/api-contract.md` r1, `contracts/permission-audit-matrix.md` r1, `../02-module-map.md` r1 |
| Repository | `QuilvianSystemFrontendDev` @ `bfa3bae69` |

Dokumen ini mengunci isi layar dan sumber datanya. Warna, jarak, ikon, component library, serta
pilihan tab, modal, atau drawer tetap `DEV_DISCRETION` dan diputuskan lewat gerbang keputusan
base component pada `build-module-frontend`.

## 1. Kebutuhan layar

| ID | Layar | Status | Route | Epic |
|---|---|---|---|---|
| `FE-PGJ-01` | Detail Struktur Gaji — bagian Tunjangan Tetap | Diperluas | `/payroll/master-data/salary-structure/[slug]` (sudah ada) | `PGJ-01`, `PGJ-02` |
| `FE-PGJ-02` | Penghasilan Variabel per run | Baru | `/payroll/payroll-runs/[id]/variable-inputs` | `PGJ-04` |
| `FE-PGJ-03` | Impor Penghasilan Variabel | Baru, bagian dari `FE-PGJ-02` | — | `PGJ-04` |
| `FE-PGJ-04` | Simulasi Penggajian | Baru | `/payroll/simulations` | `PGJ-05` |
| `FE-PGJ-05` | Rekap Lembur per run | Baru | `/payroll/payroll-runs/[id]/overtime` | `PGJ-07` |
| `FE-PGJ-06` | Tarif Lembur (baca-saja) | Baru | `/payroll/overtime-rates` | `PGJ-07` |
| `FE-PGJ-07` | Laporan Beban Gaji per Cost Center | Baru | `/payroll/reports/expense-summary` | `PGJ-06` |
| `FE-PGJ-08` | Laporan PPh dan BPJS | Baru | `/payroll/reports/tax-contribution` | `PGJ-06` |
| `FE-PGJ-09` | Kebijakan Tunjangan Transportasi | Baru (backend sudah ada) | `/payroll/master-data/transport-allowance-policy` | `PGJ-03` |
| `FE-PGJ-10` | Tunjangan Transportasi pegawai | Baru (backend sudah ada) | `/payroll/workforce-compensations/[id]/transport-allowance` | `PGJ-03` |

Layar yang sudah ada dan ikut menampilkan hasil baru tanpa perubahan struktur: detail run
(`/payroll/payroll-runs/[id]`) dan slip menampilkan baris komponen baru (tunjangan per jenis,
transport, penghasilan variabel) karena daftarnya dibaca dari baris komponen hasil hitung.
Detail run **mendapat dua tombol baru** sebagai jalan masuk `FE-PGJ-02` dan `FE-PGJ-05`.

## 2. Peta butir menu

Peta seluruh modul ada di `../02-module-map.md` bagian 3.1, karena sidebar hanya satu. Butir
milik sub-modul ini:

| Butir menu | Tingkat | Induk | `pathname` | Layar | Butir hak akses | Status |
|---|:---:|---|---|---|---|---|
| Tunjangan Transportasi | 3 | Kebijakan Tunjangan | `/payroll/master-data/transport-allowance-policy` | `FE-PGJ-09` | `TransportAllowancePolicy : Read` | Baru |
| Simulasi Penggajian | 2 | Proses Penggajian | `/payroll/simulations` | `FE-PGJ-04` | `PayrollRun : Simulate` | Baru |
| Lembur | 1 | Payroll | — | — | — | Baru (grup) |
| Tarif Lembur | 2 | Lembur | `/payroll/overtime-rates` | `FE-PGJ-06` | `OvertimeRate : Read` | Baru |
| Beban Gaji per Cost Center | 2 | Laporan | `/payroll/reports/expense-summary` | `FE-PGJ-07` | `PayrollReport : ViewAmount` | Baru |
| PPh dan BPJS | 2 | Laporan | `/payroll/reports/tax-contribution` | `FE-PGJ-08` | `PayrollReport : ViewAmount` | Baru |

Layar anak: `FE-PGJ-01` (dari detail Struktur Gaji), `FE-PGJ-02` dan `FE-PGJ-05` (dari detail
run), `FE-PGJ-03` (dari `FE-PGJ-02`), `FE-PGJ-10` (dari detail Profil Penggajian).

Pendaftaran butir menu **MUST** menjadi acceptance criteria task layar masing-masing.

## 3. Skema fitur per layar

### 3.1 `FE-PGJ-01` — bagian Tunjangan Tetap pada detail Struktur Gaji

```text
+- Struktur Gaji GR-05 Perawat Pelaksana ------------------ FE-PGJ-01 -+
| Grade GR-05 (Rp 6.000.000 – Rp 9.000.000)   Aktif                    |
| Batas struktur Rp 6.500.000 – Rp 8.500.000  Gaji bawaan Rp 7.000.000 |
+- Tunjangan Tetap -------------------------------- [Tambah Tunjangan] +
| Jenis tunjangan   | Cara hitung       | Nilai     | Pajak | Prorata  |
| Tunjangan Jabatan | Nominal tetap     | 750.000   | Ya    | Ya  [Ubah][Hapus] |
| Tunjangan Keahlian| % gaji pokok      | 5% (maks 400.000) | Ya | Ya [Ubah][Hapus] |
+----------------------------------------------------------------------+
| kosong -> "Struktur ini belum memuat tunjangan tetap. Selama kosong,  |
|           run memakai angka tunjangan di profil pegawai."            |
| gagal  -> "Tunjangan gagal dimuat."                   [Coba lagi]    |
+----------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Kepala | Grade, rentang grade, batas struktur, gaji bawaan | Detail struktur yang sudah ada (`GET .../salary-structures/{id}`) | `SalaryStructure : Read` | Gagal: seluruh layar diganti pesan dan tombol coba lagi |
| Daftar tunjangan | Jenis, cara hitung, nilai, batas maksimum, pajak, prorata | `GET .../salary-structures/{salaryStructureId}/allowances` | `SalaryStructureAllowance : Read` | Kosong dan gagal seperti gambar |
| Tombol Tambah, Ubah, Hapus | Form tunjangan | `POST`, `PUT`, `DELETE` baris | `SalaryStructureAllowance : Create`, `Update`, `Delete` | Tombol tanpa hak disembunyikan |
| Form simpan struktur | Pesan penolakan batas grade | Respons `POST`/`PUT` struktur (`SR-01`–`SR-03`) | `SalaryStructure : Create`, `Update` | Pesan tampil di dekat isian batas |

### 3.2 `FE-PGJ-02` — Penghasilan Variabel per run (termasuk `FE-PGJ-03` Impor)

```text
+- Penghasilan Variabel — Run PRL-RUN-2026-05-0001 (Calculated) - FE-PGJ-02 -+
| ! Ada 2 input berubah setelah perhitungan terakhir. Hitung ulang run.      |
| [cari nama/nomor pegawai] [Status v] [Jenis v]     [Tambah] [Impor]        |
+---------------------------------------------------------------------------+
| [ ] No. Input | Pegawai | Komponen | Jenis | Nominal | Status | Aksi        |
| [ ] PVI-...   | ...     | Insentif | Insentif | 1.000.000 | Submitted | [Setujui][Tolak][Batal] |
+---------------------------------------------------------------------------+
| [Ajukan terpilih] [Setujui terpilih]                                       |
| memuat -> kerangka baris                                                   |
| kosong -> "Belum ada penghasilan variabel di run ini."   [Tambah]          |
| gagal  -> "Data gagal dimuat."                           [Coba lagi]       |
| run tidak menerima input -> daftar baca-saja, tombol tulis disembunyikan   |
+- Halaman 1 dari n -------------------------- [< Sebelumnya] [Berikutnya >]+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Kepala | Nomor dan status run, peringatan hitung ulang | Detail run yang sudah ada; peringatan dari pembandingan `calculatedAt` run dengan waktu perubahan input pada daftar | `PayrollRun : Read` | Gagal: pesan dan coba lagi |
| Saringan dan daftar | Nomor, pegawai, komponen, jenis, nominal, status, penyetuju | `GET .../variable-inputs` | `PayrollVariableInput : Read` | Kosong dan gagal seperti gambar |
| Tambah dan ubah | Form satu input | `POST /`, `PATCH /{id}/content` | `PayrollVariableInput : Create`, `Update` | Pesan penolakan per isian |
| Impor (`FE-PGJ-03`) | Unggah baris; tampilkan baris gagal bila ditolak | `POST /import` | `PayrollVariableInput : Import` | Tidak ada baris tersimpan bila satu baris gagal; tampilkan daftar baris dan sebabnya |
| Aksi per baris | Ajukan, Setujui, Tolak (alasan), Batalkan (alasan) | `POST /{id}/submit`, `/approve`, `/reject`, `/cancel` | `Submit`, `Approve`, `Reject`, `Cancel` | Tombol mengikuti `availableActions` dari respons dan hak akses |
| Aksi massal | Ajukan dan setujui banyak baris | `POST /bulk-submit`, `/bulk-approve` | `Submit`, `Approve` | Tampilkan hasil per baris: berhasil dan gagal beserta sebab |

### 3.3 `FE-PGJ-04` — Simulasi Penggajian

```text
+- Simulasi Penggajian ------------------------------------- FE-PGJ-04 -+
| Hasil simulasi tidak tersimpan dan tidak mempengaruhi run resmi.      |
| [Periode v] [Pilih pegawai (opsional, maks 50)]        [Jalankan]     |
+- Ringkasan ----------------------------------------------------------+
| Pegawai 48 | Kotor Rp ... | Potongan Rp ... | Bersih Rp ...          |
+- Per pegawai --------------------------------------------------------+
| Pegawai | Kotor | Potongan | Bersih | Peringatan | [Rincian]          |
+----------------------------------------------------------------------+
| belum dijalankan -> "Pilih periode lalu jalankan simulasi."          |
| memuat -> "Menghitung..."  (tombol Jalankan dinonaktifkan)           |
| gagal  -> pesan penolakan dari server               [Coba lagi]      |
+----------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Pilihan | Periode, pegawai | Opsi periode yang sudah ada (`GET /payroll-runs/period-options`) | `PayrollRun : ReadPeriods` (sudah ada) — pemegang `PayrollRun : Simulate` perlu hak ini juga | Gagal muat opsi: pesan dan coba lagi |
| Ringkasan dan daftar | Total dan hasil per pegawai | `POST /payroll-runs/simulations` | `PayrollRun : Simulate` | Penolakan `SIM-01`–`SIM-03` tampil apa adanya |
| Rincian pegawai | Baris komponen, peringatan, galat | Respons yang sama | `PayrollRun : Simulate` | — |

### 3.4 `FE-PGJ-05` Rekap Lembur dan `FE-PGJ-06` Tarif Lembur

Keduanya layar daftar baca-saja dengan bentuk seperti `FE-PGJ-02` tanpa tombol tulis.

| Layar | Kolom | Sumber data | Butir hak akses | Kosong |
|---|---|---|---|---|
| `FE-PGJ-05` | Pegawai, tanggal lembur, menit terverifikasi, pengali, tarif per jam, nominal | `GET /payroll-runs/{id}/overtime-inputs` | `PayrollRun : Read` | "Belum ada lembur terverifikasi yang diimpor ke run ini." |
| `FE-PGJ-06` | Tarif lembur sesuai endpoint HR | `GET api/v1/corporate/human-resource/master-data/overtime-rates` | `OvertimeRate : Read` | "Belum ada tarif lembur." — tarif diubah di modul HR, bukan di sini |

### 3.5 `FE-PGJ-07` Beban Gaji dan `FE-PGJ-08` PPh dan BPJS

| Layar | Saringan | Kolom | Sumber data | Butir hak akses | Kosong |
|---|---|---|---|---|---|
| `FE-PGJ-07` | Periode atau run, Cost Center | Cost Center, komponen, jenis, jumlah pegawai, nominal; total penghasilan dan potongan | `GET /reports/expense-summary` | `PayrollReport : ViewAmount` | "Belum ada run yang disahkan pada periode ini." |
| `FE-PGJ-08` | Periode atau run, cari pegawai | Pegawai, Cost Center, PPh, BPJS pegawai, sumber "Tetapan profil", versi kebijakan | `GET /reports/tax-contribution` | `PayrollReport : ViewAmount` | Sama |

### 3.6 `FE-PGJ-09` Kebijakan Tunjangan Transportasi dan `FE-PGJ-10` Tunjangan Transportasi pegawai

`FE-PGJ-09` adalah layar master data dan **MUST** mengikuti standar fitur master data frontend
yang berlaku (daftar, detail, tambah, ubah, status, hapus) memakai sembilan endpoint
`TransportAllowancePolicy` yang sudah ada. `FE-PGJ-10` adalah daftar dan form per pegawai memakai
endpoint `WorkforceTransportAllowance` yang sudah ada.

| Layar | Catatan isi wajib |
|---|---|
| `FE-PGJ-09` | Form menampilkan cara hitung. Bila dipilih selain "Bulanan tetap", tampil keterangan: "Metode ini belum dihitung otomatis di run. Gunakan Penghasilan Variabel." |
| `FE-PGJ-10` | Sebelum menyimpan, tampil peringatan bila pegawai sudah punya tunjangan transport bulanan aktif yang masa berlakunya beririsan, karena run akan menggagalkan pegawai itu (`CALC-01`) |

## 4. Aksi per peran

Diturunkan dari `contracts/permission-audit-matrix.md` bagian 3. Layar menampilkan tombol hanya
bila pengguna memegang hak yang tercantum di tabel wilayah, dan untuk input variabel juga hanya
bila aksinya ada di `availableActions`.

## 5. Data dan status yang dikonsumsi

| Status | Tampilan |
|---|---|
| Input `Draft` | Bisa diubah, diajukan, dibatalkan |
| Input `Submitted` | Bisa disetujui, ditolak, dibatalkan |
| Input `Approved` | Bisa dibatalkan selama run menerima input |
| Input `Applied`, `Rejected`, `Cancelled` | Baca-saja; alasan ditampilkan untuk `Rejected` dan `Cancelled` |
| Peringatan hitung | `CALC-02`–`CALC-04` tampil di detail pegawai pada run dan pada simulasi |

## 6. Penanganan keadaan

| Keadaan | Perilaku |
|---|---|
| Memuat | Kerangka baris pada daftar; tombol aksi dinonaktifkan selama permintaan berjalan |
| Kosong | Kalimat pada skema masing-masing |
| Gagal | Pesan dan tombol coba lagi; pesan penolakan dari server ditampilkan apa adanya |
| Data basi (409) | "Data ini baru saja diubah petugas lain. Muat ulang lalu coba lagi." lalu daftar dimuat ulang |
| Pengiriman ganda | Tombol simpan dan aksi dinonaktifkan sampai respons diterima; aksi status di server idempoten |
| Tanpa hak (403) | Tombol tidak ditampilkan; bila tetap terjadi, tampil "Anda tidak punya hak untuk tindakan ini." |
| Privasi | Nominal hanya tampil di layar yang dijaga hak yang tercantum; tidak ditulis ke log atau penyimpanan browser |

## 7. Kewenangan UI

| Hal | Kewenangan |
|---|---|
| Keberadaan layar, sumber data, hak akses per tombol, kalimat kosong dan gagal | Dikunci dokumen ini |
| Letak butir menu baru di kelompok yang ada | Dikunci `../02-module-map.md`; penataan ulang ke 13 menu menunggu DEC-PRL-017 |
| Nama butir menu, urutan, ikon | `DEV_DISCRETION` |
| Bentuk form (halaman, modal, drawer), tata letak, komponen | `DEV_DISCRETION`, lewat gerbang keputusan base component |
| Warna, jarak, tipografi | Design token yang berlaku; tidak ada nilai literal |
