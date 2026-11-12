# Integration Contract — Sub-modul Payroll Gaji

| Field | Nilai |
|---|---|
| Blueprint | `PRL-BP-001` / `payroll-gaji`, revision `1` |
| Status | `approved` |
| `last_changed_in` | `PGJ-C1` |
| Owner | Yasmin; `approved_by`: Yasmin; `approved_at`: 2026-11-12 |
| `input_revision` | `02-backend-architecture.md` r1, `../02-module-map.md` r1 |
| Traceability | PRL-DEC-36, 39, 42, 44, 48; DEC-PRL-001, 005 |

Sub-modul ini tidak memanggil sistem luar rumah sakit. Seluruh integrasi bersifat internal,
di dalam satu aplikasi dan satu basis data.

## 1. Arah baca dan tulis

| Mitra | Arah | Data | Cara | Sinkron / asinkron | Kegagalan dan pemulihan | Status |
|---|---|---|---|---|---|---|
| HRD — penetapan gaji (`WfpSalaryAssignment`) | Payroll **membaca** | Gaji pokok, struktur, grade, masa berlaku, hanya yang disetujui | Query langsung saat impor dan hitung | Sinkron | Pegawai tanpa penetapan disetujui menjadi galat "belum siap payroll" pada run | Sudah ada |
| HR — kehadiran dan lembur terverifikasi | Payroll **membaca** (snapshot) | Menit kerja, cuti, mangkir, lembur terverifikasi | `PayrollHrdImportService` menyalin ke run | Sinkron, saat aksi impor | Impor gagal = run tetap di status sebelumnya; petugas mengulang impor | Sudah ada |
| HR — tarif lembur (`MstOvertimeRate`) | Layar Payroll **membaca** endpoint HR | Tarif lembur | Frontend memanggil `GET .../overtime-rates` | Sinkron | Gagal muat = pesan dan tombol coba lagi; tidak memblokir proses gaji | Sudah ada (endpoint) |
| HR — Cost Center (`MstCostCenter`) | Payroll **membaca** | Kode dan nama cost center | Snapshot di baris run | Sinkron | Pegawai tanpa cost center masuk kelompok "Tanpa Cost Center" di laporan | Sudah ada |
| Platform — NumberSeriesManagement | Payroll **meminta nomor** | Nomor input variabel seri `PVI` | `NumberSeriesAllocator.AllocateAsync` | Sinkron, `DbContext` terpisah | Alokasi gagal = pembuatan input gagal (400/500) tanpa data setengah jadi; nomor yang terlanjur dialokasikan boleh tidak terpakai (celah nomor diterima) | Baru dipakai |
| Finance — handoff pembayaran | Payroll **menyerahkan** | Hasil run yang disahkan | `PayrollPaymentsController` `finance-handoff` | Sesuai implementasi yang ada | Sesuai implementasi yang ada | Sudah ada, tidak berubah |
| Medical Fee — jasa dokter bergaji tetap | Payroll **membaca** | Jasa kotor yang disetujui | `PayrollMedicalFeeIntakeService` | Sesuai implementasi yang ada | Sesuai implementasi yang ada | Sudah ada, tidak berubah |
| Scheduling — rekap aktivitas | Payroll **akan membaca** | Shift, siaga, panggilan, hari hadir | Belum dirancang | — | — | `BLOCKED` — DEC-PRL-005 |
| Mesin Payroll Modern | Tidak ada integrasi baru | — | — | — | Dibekukan sampai pensiun (PRL-DEC-44) | `BLOCKED` — DEC-PRL-001 |
| Sub-modul `remunerasi-jasa` | `remunerasi-jasa` **akan menulis** lewat input variabel | Hasil distribusi yang dibayar lewat gaji | Belum dirancang | — | — | `BLOCKED` |

## 2. Idempotensi dan konsistensi

| Kejadian | Jaminan |
|---|---|
| Aksi status input yang diulang | Input yang sudah di status tujuan dikembalikan apa adanya |
| Run disahkan dua kali | Pengesahan sudah idempoten di `PayrollRunService` (`89b9fd69` baris 756); input manual hanya berpindah ke `Applied` sekali |
| Impor input | Satu transaksi untuk seluruh baris; tidak ada impor setengah jadi |
| Simulasi | Transaksi selalu dibatalkan; tidak ada efek samping di basis data utama |

## 3. Yang **MUST NOT** dilakukan sub-modul ini

| Larangan | Sebab |
|---|---|
| Menulis ke tabel HR, Scheduling, Medical Fee, atau Finance | Batas kepemilikan data (`../02-module-map.md` bagian 2) |
| Membuat jurnal atau memanggil Accounting | PRL-DEC-27 |
| Mengirim instruksi ke bank | PRL-DEC-39, keputusan 11 November 2026 |
| Memanggil endpoint mesin Modern | PRL-DEC-44 |
