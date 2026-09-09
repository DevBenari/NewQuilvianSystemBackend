# Zona Konflik

Diaudit pada backend `f2c5090` dan frontend `847be1fc0`.

Berkas ini menjawab: **di mana saja titik yang berpotensi membuat dua modul bertabrakan.**

## Jenis konflik

| Kode | Jenis | Cara mendeteksi |
| --- | --- | --- |
| `KF-1` | Nama kembar | Dua entity bernama sangat mirip di area berbeda |
| `KF-2` | Kandidat duplikasi konsep | Dua entity berbeda nama tetapi menyimpan konsep yang sama |
| `KF-3` | Entity tanpa pemilik | Tidak jelas modul mana yang berwenang menulis |
| `KF-4` | Skema tidak lengkap | Ada `DbSet` tanpa configuration, atau ada API tanpa migration |
| `KF-5` | Alamat endpoint bentrok | Dua controller memakai grup `[Tags(...)]` atau base URL yang sama |
| `KF-6` | Enum ganda | Enum dengan makna sama didefinisikan di dua area |
| `KF-7` | Prefix tidak sesuai | Data induk diberi prefix `Trx`, atau sebaliknya |

## Daftar temuan

| ID | Jenis | Temuan | Modul terdampak | Risiko nyata bila diabaikan | Status |
| --- | --- | --- | --- | --- | --- |
| `KF-001` | `KF-3` | Tidak ada satu pun modul yang tercatat pemilik proses bisnisnya. Seluruh kolom pemilik pada berkas 01 berisi `Belum ditentukan`, kecuali Human Resource | Seluruh sistem | Ketika dua modul ingin mengubah aturan pada data yang sama, tidak ada yang berwenang memutuskan. Perselisihan baru ketahuan saat integrasi, ketika perbaikannya sudah mahal | Terbuka |
| `KF-002` | `KF-4` | 36 entity sudah dipakai controller atau service tetapi tidak punya berkas `IEntityTypeConfiguration`. Terbesar di Billing Management | Billing Management, Master Data HealthServices | Relasi, index, dan perilaku hapus tidak ditetapkan eksplisit. Tabel bisa terbentuk tanpa index yang dibutuhkan, sehingga laporan melambat seiring data bertambah, dan penghapusan data induk dapat menyeret data transaksi | Terbuka |
| `KF-003` | `KF-2` | Konsep konsultasi muncul sebagai `TrxDoctorConsultation` di Clinical Management. Modul baru yang membutuhkan permintaan konsultasi ke tenaga lain berpotensi membuat konsultasi versi sendiri | Clinical Management, modul baru mana pun | Dua modul menghitung status konsultasi dengan cara berbeda, sehingga riwayat pasien terbelah dan laporan jumlah konsultasi tidak pernah cocok | Terbuka |
| `KF-004` | `KF-1` | `MstPerformanceCycle` dan `TrxPerformanceCycle` sama-sama ada di area Corporate | HR Master Data, HR Performance Management | Developer berikutnya menebak mana yang benar ketika menulis kode siklus penilaian. Salah pilih berarti data penilaian masuk ke tabel yang tidak dibaca laporan | Terbuka |
| `KF-005` | `KF-7` | Empat model diletakkan di dalam pohon EF configuration, bukan di `Areas/`: `MstDisciplinaryActionType`, `MstViolationType`, `MstSanctionType`, `MstEmployeeRelationCaseType` | HR Employee Relation Management | Model tidak ditemukan saat orang mencari di `Areas/`, sehingga berpotensi dibuat ulang dengan nama lain. Pola ini juga menyebar bila ditiru | Terbuka |
| `KF-006` | `KF-5` | Dua pasang controller memakai base URL sama tanpa keterangan apakah disengaja: `billing/patient-funds` dan `workflow-instances` | Billing Management, HR Workflow Management | Frontend dapat memanggil endpoint yang salah karena dua controller menjawab alamat yang sama | Terbuka |
| `KF-007` | `KF-5` | `AuthController` dan `VersionController` memakai `Route("api/v1/[controller]")` dengan token yang belum diganti | Global | Base URL-nya baru terbentuk saat runtime, sehingga tidak dapat diperiksa terhadap pemakaian frontend maupun terhadap kavling alamat modul lain | Terbuka |

## Temuan yang diperiksa dan ternyata bukan konflik

Bagian ini sengaja ditulis agar tidak ditemukan ulang sebagai masalah baru enam bulan lagi.

| Yang diperiksa | Hasil |
| --- | --- |
| Enum bermakna sama di dua area | Tidak ditemukan. Tidak ada nama enum yang muncul dua kali |
| Entity tanpa migration | Tidak ada. Seluruh 516 `DbSet` punya tabel yang dibuat atau diganti nama migration |
| Entity berhenti di `L1` | Tidak ada |
| Tiga entity bernama `*StatusHistory` | Bukan konflik. `TrxWorkflowStatusHistory`, `InpStatusHistory`, dan `OprStatusHistory` memang riwayat status milik tiga aggregate berbeda, dan prefix modulnya membedakannya dengan jelas |
| Base URL berbagi pada modul Operasi | Disengaja. Beberapa controller memakai base sama karena hak aksesnya berbeda per kelompok aksi |

## Status temuan

| Status | Arti |
| --- | --- |
| Terbuka | Belum dibahas siapa pun |
| Dibahas | Sudah masuk wawancara suatu modul, keputusan belum ada |
| Diputuskan | Sudah ada keputusan owner, lengkap dengan `decision_id` |
| Selesai | Sudah ditutup di kode, dibuktikan pada commit tertentu |

Temuan tidak boleh dihapus. Temuan yang sudah selesai tetap tinggal beserta bukti penutupnya.

## Yang paling menghalangi modul baru

Bila modul baru akan dibahas dalam waktu dekat, dua temuan ini paling menentukan:

**`KF-001` kepemilikan proses bisnis.** Tanpa pemilik yang jelas, wawancara modul baru akan
menghasilkan keputusan yang tidak dapat disahkan siapa pun.

**`KF-003` konsep konsultasi.** Modul apa pun yang alurnya dimulai dari "order konsultasi ke
tenaga lain" akan langsung bersentuhan dengan `TrxDoctorConsultation`. Keputusan memakai ulang
atau membuat baru harus diambil sebelum desain, bukan sesudahnya.
