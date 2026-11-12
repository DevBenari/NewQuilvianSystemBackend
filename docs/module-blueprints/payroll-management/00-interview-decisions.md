# Payroll Management — Keputusan Wawancara

| Field | Nilai |
|---|---|
| Blueprint ID | `PRL-BP-001` |
| Revision | `1` |
| Status | `draft` — keputusan dalam wewenang pemilik Payroll sudah disetujui; keputusan milik pihak lain menunggu pengesahan mereka (kolom "Menunggu" di bagian 4) |
| Pemilik produk/domain | Yasmin — pemilik modul Payroll, juga pemilik modul Medical Fee dan Finance |
| Backend SHA | `89b9fd69` (branch `Yasmina`) |
| Frontend SHA | `bfa3bae69` (branch `yasmina`) |
| Sumber | Wawancara `grill-me` 11–12 November 2026 (scope pass, perluasan scope, closure pass), dokumen jawaban OQ-1 s.d. OQ-7 dan rekap sembilan butir dari pemilik, serta hasil `requirement-completeness-gate` 12 November 2026 |
| Persetujuan pemilik | "Saya setujui semuanya" — Yasmin, 12 November 2026 |
| Catatan penulisan | Wawancara berjalan saat repository masih read-only. Berkas ini ditulis 12 November 2026 setelah `MODULE BLUEPRINT MODE` aktif; isinya salinan keputusan yang sudah terjadi, tanpa keputusan baru |

---

## 1. Scope dan hasil yang dikejar

**Batas scope:** seluruh 13 menu Payroll (perluasan scope disetujui pemilik pada wawancara 11
November 2026), termasuk master data gaji: grade, struktur, tunjangan, komponen, bonus, dan THR,
beserta cara semuanya menentukan angka gaji saat run dihitung.

**Di dalam scope:** 13 menu dan 56 submenu berikut, ditambah keputusan master data gaji.

| No | Menu | Submenu |
|---:|---|---|
| 1 | Dashboard Payroll | Ringkasan Penggajian, Total Remunerasi, Proses Tertunda, Tren Biaya Pegawai |
| 2 | Master Komponen Penghasilan | Jenis Penghasilan, Jenis Potongan, Sumber Dana, Aturan Komponen |
| 3 | Struktur Gaji & Tunjangan | Golongan/Grade Gaji, Gaji Pokok, Tunjangan Jabatan, Tunjangan Profesi, Tunjangan Tetap |
| 4 | Penggajian Pegawai | Periode Payroll, Data Penggajian, Simulasi, Perhitungan, Koreksi |
| 5 | Lembur & Harian | Rekap Lembur Disetujui, Tarif Lembur, Honorium Tugas Tambahan |
| 6 | Potongan, Pajak & Iuran | PPH21, BPJS Kesehatan, BPJS Ketenagakerjaan, Potongan Lainnya |
| 7 | Master Skema Remunerasi | Grade/Indeks Remunerasi, Bobot Jabatan, Indikator Kinerja, Aturan Pembagian, Versi Kebijakan |
| 8 | Jasa Pelayanan Medis | Rekap Tindakan/Layanan, Jasa Dokter, Jasa Tenaga Kesehatan, Verifikasi Kontribusi Pelayanan |
| 9 | Distribusi Jasa Pelayanan | Pool Jasa Pelayanan, Pembagian Langsung & Tidak Langsung, Alokasi Per-unit, Distribusi Per-pegawai |
| 10 | Insentif & Bonus Kinerja | Indikator Kinerja, Insentif Jabatan, Bonus Prestasi, Penyesuaian Remunerasi |
| 11 | Verifikasi & Approval | Validasi Hasil, Koreksi, Persetujuan Payroll, Persetujuan Remunerasi |
| 12 | Pelayanan & Slip | Daftar Pembayaran, Instruksi Transfer, Status Pembayaran, Slip Gaji, Rincian Remunerasi |
| 13 | Laporan & Rekonsiliasi | Laporan Payroll, Distribusi Jasa, Biaya Per-Unit, Jurnal, History Pembayaran |

**Di luar scope — hanya titik sentuhnya yang dibahas:**

| Hal | Pemilik |
|---|---|
| Perhitungan jasa kotor, tarif sharing, penyerahan jasa ke Finance | Medical Fee (`MF-BP-001`) |
| Pembayaran, potongan di sisi Finance, pembukuan | Finance Management dan Accounting |
| Master KPI, penilaian kinerja, tarif lembur, jadwal dan penugasan | Human Resource dan Scheduling |
| Aturan internal penetapan gaji | HRD |

## 2. Bentuk blueprint

| Field | Nilai |
|---|---|
| `blueprint_shape` | `COMPOSITE` |
| `shape_decided_by` | `USER_CONFIRMED` (`PRL-DEC-21`, menggantikan `PRL-DEC-01`) |
| Sub-modul `payroll-gaji` | Menu 1–6, 11.1–11.3, 12.1–12.4, 13.1, 13.4 (diganti laporan beban gaji, `PRL-DEC-27`), 13.5 |
| Sub-modul `remunerasi-jasa` | Menu 7–10, 11.4, 12.5, 13.2, 13.3 |
| Kemampuan tanpa rumpun | Tidak ada. Dashboard ditempatkan di `payroll-gaji` atas persetujuan pemilik |

Hasil uji pemecahan: kedua rumpun memenuhi lima dari lima syarat — bounded context sendiri,
kosakata status sendiri (status run dan slip lawan versi kebijakan dan periode distribusi),
Resource hak akses sendiri, dapat dirilis terpisah, master dan pemilik approval sendiri. Bukti
untuk Remunerasi baru berupa nama menu, belum aturan bisnis.

## 3. Fakta dari source (dikutip apa adanya)

| ID | Fakta | Bukti @ `89b9fd69` |
|---|---|---|
| F-01 | Tunjangan tetap dihitung dari satu angka di profil (`TotalAllowance`), tampil sebagai satu baris "Tunjangan Tetap", selalu kena pajak | `PayrollCalculationService.cs` baris 276 dan 344 |
| F-02 | Master Jenis Tunjangan, Struktur Gaji (`ComponentConfigurationJson`, batas min/maks), kebijakan Shift/Siaga/Risiko, dan Tunjangan Transportasi tidak dibaca perhitungan gaji | Pencarian pada `Areas` |
| F-03 | Penetapan gaji hanya memeriksa aktif dan periode tidak beririsan; tidak ada cek rentang grade | `WfpSalaryAssignmentController.cs` baris 379–396 |
| F-04 | Rekrutmen sudah memakai `MaximumSalary` grade untuk menentukan tingkat persetujuan (`Tier14Exceptional`) | `JobOfferService.cs` baris 197; `TalentPoolService.cs` baris 221 |
| F-05 | Dua mesin hitung (V2 dan Modern) menulis tabel `PrlPayrollRuns` yang sama | `PayrollRunService.cs`; `PayrollRunInitializationService.cs` baris 128 |
| F-06 | Mesin Modern menentukan tunjangan fungsional, struktural, dan risiko dari kata kunci nama jabatan dengan nominal di kode | `PayrollEarningsCalculationService.cs` baris 325–383 |
| F-07 | Hanya jenis run `Regular` yang didukung V2 | `PayrollRunService.cs` baris 304 |
| F-08 | HR sudah menyimpan penilaian kinerja per pegawai (`FinalScore`, `FinalRating`, `IsFinalized`) | `WfpPerformanceReview.cs` |
| F-09 | Medical Fee menyimpan jasa kotor per penerima; detail jasa hanya punya asal domain, tanpa unit atau cost center | `MdfServiceFee.cs`, `MdfServiceFeeDetail.cs` |
| F-10 | `AllowProfileSalaryFallback` bawaan `false`; pegawai tanpa penetapan gaji disetujui dihentikan dengan pesan, bukan dihitung nol | `PayrollPolicyOptions.cs` baris 20; `PayrollCalculationService.cs` baris 256–261 |
| F-11 | Dokumen `PAYROLL_V2_02` bagian 2 menyatakan batas grade ditegakkan layar penetapan gaji HRD; kode tidak memeriksanya | Bertentangan dengan F-03 — **Conflict** dokumen, perlu koreksi dokumen |

## 4. Decision Log

Kolom **Status** memakai `approved` bila keputusan berada dalam wewenang pemilik Payroll dan
sudah disetujuinya pada 12 November 2026. Kolom **Menunggu** menyebut pihak lain yang masih
harus mengesahkan; selama kolom itu berisi, keputusan belum boleh dianggap final untuk bagian
yang menjadi wewenang pihak tersebut.

| ID | Type | Keputusan | Status | Menunggu |
|---|---|---|---|---|
| PRL-DEC-01 | Decision | Bentuk SINGLE untuk scope sempit | `superseded` oleh PRL-DEC-21 | — |
| PRL-DEC-02 | Decision | Tunjangan tetap berasal dari Struktur Gaji per grade: daftar Jenis Tunjangan beserta nominalnya | `approved` | — |
| PRL-DEC-03 | Decision | Penyimpangan tunjangan satu pegawai dari struktur boleh, lewat pengajuan dengan alasan, masa berlaku, dan persetujuan | `approved` | HRD (DEC-PRL-002) |
| PRL-DEC-04 | Decision | Masa peralihan: struktur menang; angka profil dipakai bila struktur kosong, dengan peringatan dan saklar kebijakan | `approved` | — |
| PRL-DEC-05 | Decision | Insentif bulanan ikut run bulanan; THR dan bonus tahunan lewat run sendiri | `approved` | — |
| PRL-DEC-06 | Decision | THR memakai Master Kebijakan THR ditambah penyesuaian per pegawai dengan alasan dan persetujuan | `approved` | — |
| PRL-DEC-07 | Decision | Insentif dan bonus: nominal diinput atau diimpor per pegawai, lalu disetujui sebelum dihitung | `approved` | — |
| PRL-DEC-08 | Decision | Gaji pokok di luar rentang grade boleh dengan persetujuan tingkat pengecualian | `approved` | HRD |
| PRL-DEC-09 | Decision | Tunjangan berbasis nama jabatan di mesin Modern dihapus bertahap menjadi data master | `approved` | MHamzah |
| PRL-DEC-10 | Decision | Pengecualian grade dan penyimpangan tunjangan disetujui orang lain yang bertingkat lebih tinggi; THR, insentif, bonus satu langkah (pengaju boleh = pengesah) | `approved` | Direksi |
| PRL-DEC-11 | Decision | Run THR dan bonus tahunan tidak memotong cicilan piutang | `approved` | — |
| PRL-DEC-12 | Decision | Batas gaji Struktur wajib berada di dalam batas Grade; keduanya berlaku | `approved` | — |
| PRL-DEC-13 | Decision | Tiap tunjangan punya satu metode utama: nominal tetap, persentase gaji pokok, atau berbasis aktivitas; campuran hanya bila kebijakan mengaturnya eksplisit | `approved` | HRD |
| PRL-DEC-14 | Decision | Tunjangan Struktural dan Fungsional memakai master kebijakan baru per jabatan, profesi, dan jenjang | `approved` | HRD |
| PRL-DEC-15 | Decision | Kebijakan tunjangan memuat periode efektif, metode, persyaratan penerima, dan riwayat; tanpa nominal di kode | `approved` | MHamzah |
| PRL-DEC-16 | Decision | Pengesah pengecualian bukan pengaju; satu hak `ApproveException` untuk gaji di luar rentang dan penyimpangan tunjangan; verifikasi Kepala HRD hanya untuk pengecualian; riwayat dan audit wajib | `approved` | HRD, Direksi |
| PRL-DEC-17 | Decision | Mesin V2 sebagai jalur utama (rekomendasi OQ-4) | `superseded` oleh PRL-DEC-44 | — |
| PRL-DEC-18 | Fact/Decision | Fallback gaji pokok dari profil mati; pegawai tanpa penetapan disetujui ditandai belum siap payroll | `approved` (sudah berlaku di kode, F-10) | — |
| PRL-DEC-19 | Decision | THR: ≥ 12 bulan = 1 bulan upah, 1–<12 bulan proporsional; dasar upah dipilih per kebijakan (nilai awal gaji pokok + tunjangan tetap); di bawah rumus legal ditolak kecuali kasus HRD/Legal dengan `ApproveException` | `approved` | HRD, Legal (verifikasi regulasi) |
| PRL-DEC-20 | Decision | THR, bonus tahunan, dan integrasi PPh-nya ditunda ke tahap berikutnya | `approved` | — |
| PRL-DEC-21 | Decision | COMPOSITE: `payroll-gaji` dan `remunerasi-jasa` (pemetaan di bagian 2) | `approved` | — |
| PRL-DEC-22 | Decision | Medical Fee tetap menghitung jasa langsung; Remunerasi mengurus jasa tidak langsung | `approved` | — |
| PRL-DEC-23 | Decision | Pool ditetapkan per periode oleh pejabat berwenang dengan dasar penetapan, lalu disetujui | `approved` | — |
| PRL-DEC-24 | Decision | Distribusi dua tahap: ke unit, lalu ke pegawai menurut poin (indeks grade × bobot jabatan × skor kinerja) | `approved` | — |
| PRL-DEC-25 | Decision | Skor kinerja diinput atau diimpor | `superseded` oleh PRL-DEC-45 | — |
| PRL-DEC-26 | Decision | Tunjangan Jabatan tetap bulanan lewat run gaji; Insentif Jabatan variabel per periode lewat distribusi | `approved` | — |
| PRL-DEC-27 | Decision | Menu Jurnal dikeluarkan dari Payroll; diganti laporan ringkasan beban gaji untuk Finance | `approved` | — |
| PRL-DEC-28 | Decision | Sumber Dana = asal pembiayaan beban gaji dan tunjangan; alokasi per pegawai, unit, komponen, periode; boleh dibagi ke beberapa sumber | `approved` | HRD, Finance |
| PRL-DEC-29 | Decision | Persetujuan remunerasi oleh pengesah bukan pengaju dan bertingkat lebih tinggi, untuk versi skema dan hasil distribusi | `approved` | Direksi |
| PRL-DEC-30 | Decision | Versi kebijakan berlaku periode berikutnya; periode lama tidak berubah; koreksi lewat Penyesuaian Remunerasi | `approved` | — |
| PRL-DEC-31 | Decision | Honor tugas tambahan diinput per tugas per pegawai, disetujui, ikut run bulanan | `approved` | — |
| PRL-DEC-32 | Decision | Simulasi = hitung percobaan satu periode, tidak disimpan sebagai run, tidak mengunci periode | `approved` | — |
| PRL-DEC-33 | Decision | Menu 6: PPh dan BPJS tetap tetapan profil; isinya pengaturan dan laporan hasil | `approved` | — |
| PRL-DEC-34 | Decision | Dashboard: akses nominal dibatasi; definisi empat metrik masih terbuka | `approved` (sebagian) | HRD, Finance |
| PRL-DEC-35 | Decision | Aturan Komponen = aturan kelayakan, kalkulasi, pajak, dan efektivitas komponen | `approved` | HRD |
| PRL-DEC-36 | Decision | HRD pemilik penetapan gaji pokok; Payroll hanya memakai data sah | `approved` | HRD |
| PRL-DEC-37 | Decision | Kontribusi pelayanan diverifikasi di pelayanan/Medical Fee; Payroll mengonsumsi hasilnya | `approved` | Pelayanan |
| PRL-DEC-38 | Decision | Bonus masuk sebagai input variabel yang disetujui; koreksi lewat alur penyesuaian terkontrol | `approved` | HRD |
| PRL-DEC-39 | Decision | Instruksi Transfer = handoff ke Finance, bukan eksekusi transfer bank | `approved` | Finance |
| PRL-DEC-40 | Decision | Rincian Remunerasi hanya untuk pemilik data dan petugas berwenang | `approved` | HRD |
| PRL-DEC-41 | Decision | Biaya Per-Unit berdasarkan Cost Center; Sumber Dana dimensi terpisah | `approved` | Finance, HRD |
| PRL-DEC-42 | Decision | Rekap dan tarif lembur dibaca-saja dari HRD | `approved` | HRD |
| PRL-DEC-43 | Decision | Insentif dan Bonus Prestasi satu langkah; hanya Penyesuaian Remunerasi yang pengesahnya bukan pengaju dan bertingkat lebih tinggi | `approved` | — |
| PRL-DEC-44 | Decision | Mesin V2 satu-satunya mesin; Modern dipensiunkan bertahap setelah bagian berguna dipindah, hasil direkonsiliasi, dan pemilik teknis setuju | `approved` | MHamzah (`evidence/01-permintaan-untuk-mhamzah-mesin-payroll.md`) |
| PRL-DEC-45 | Decision | Skor kinerja dari penilaian HR yang final; bila tidak ada, input manual dengan alasan, disetujui bersama distribusi | `approved` | HRD |
| PRL-DEC-46 | Decision | Tunjangan Transportasi masuk keluarga tunjangan berbasis aktivitas dan dihitung di run | `approved` | — |
| PRL-DEC-47 | Decision | Medical Fee diamandemen agar jasa membawa unit atau cost center | `approved` | Revisi `MF-BP-001` |
| PRL-DEC-48 | Decision | HR dan Scheduling menyediakan rekap aktivitas per pegawai per periode; Payroll mengimpornya | `approved` | HR, Scheduling |

### Keputusan 11 November 2026 yang tetap berlaku

Dicatat di `PAYROLL_V2_01_AUDIT_DAN_PEMINDAHAN_HRD.md` bagian 7.3 dan 8.1: approval run satu
langkah dan pengaju boleh sama dengan pengesah; pengakuan piutang saat run disahkan beserta
penanda status bayar; batas potong 50%; honor medik lewat Finance AP, dokter bergaji tetap lewat
Payroll; tanpa integrasi bank dan tanpa jurnal; slip tetap `PaymentPaid` dengan Konfirmasi
Pembayaran Manual; PPh/BPJS dari profil; prorata dan cuti tetap.

## 5. Pertanyaan terbuka dan blocker

| ID | Pertanyaan | Kemampuan terdampak | Usulan baseline | Dampak | Pemilik | Status |
|---|---|---|---|---|---|---|
| DEC-PRL-001 | Persetujuan pensiun Modern, daftar pemakai endpoint Modern, nasib kode PPh/BPJS Modern, panjang rekonsiliasi | Pensiun mesin Modern | PRL-DEC-44 | `BLOCKING` | MHamzah, pemilik | `OPEN` |
| DEC-PRL-002 | Persetujuan HRD atas perubahan penetapan gaji: cek rentang, verifikasi Kepala HRD, `ApproveException`, riwayat | Pengecualian gaji pokok, penyimpangan tunjangan | PRL-DEC-08, 16, 36 | `BLOCKING` | Pemilik HRD | `OPEN` |
| DEC-PRL-003 | Data jabatan, profesi, dan jenjang pegawai di HR untuk persyaratan penerima | Kebijakan Struktural/Fungsional | Perlu impact scan | `BLOCKING` | HR, pemilik | `OPEN` |
| DEC-PRL-004 | Apakah nominal hardcode Modern adalah kebijakan sah | Isi master tunjangan | — | `NON_BLOCKING_STANDARD` | HRD, Finance, Direksi | `OPEN` |
| DEC-PRL-005 | Kontrak rekap aktivitas HR/Scheduling ke Payroll (shift, siaga, panggilan, hari hadir per pegawai) | Shift, Siaga, Risiko; Transport per kehadiran | PRL-DEC-48 | `BLOCKING` | HR, Scheduling | `OPEN` |
| DEC-PRL-006 | Parameter THR: pegawai keluar, cuti di luar tanggungan, tanggal acuan, cut-off; verifikasi hukum | Run THR | PRL-DEC-19 | `BLOCKING` | HRD, Legal | `OPEN` |
| DEC-PRL-007 | PPh 21 gabungan untuk run terpisah dan pencegahan pajak ganda | Run THR dan bonus tahunan | Ditunda (PRL-DEC-20) | `BLOCKING` | Pajak, Finance | `OPEN` |
| DEC-PRL-008 | Sumber Dana: pengesahan, tingkat penautan, prioritas, pembagian | Sumber Dana | PRL-DEC-28 | `BLOCKING` | HRD, Finance | `OPEN` |
| DEC-PRL-009 | Definisi empat metrik Dashboard dan hak lihat nominal | Dashboard | PRL-DEC-34 | `BLOCKING` | Pemilik menu, HRD, Finance | `OPEN` |
| DEC-PRL-010 | Dimensi kelayakan Aturan Komponen | Aturan Komponen | PRL-DEC-35 | `BLOCKING` | HRD, Payroll | `OPEN` |
| DEC-PRL-011 | Revisi `MF-BP-001` (unit/cost center), kontrak ke Remunerasi (`MF-CQ-06`), apakah "unit" = Cost Center | Seluruh Remunerasi | PRL-DEC-47, 41 | `BLOCKING` | Pemilik Medical Fee, HR | `OPEN` |
| DEC-PRL-012 | Rumus alokasi pool ke unit, penilaian tahunan mana untuk periode distribusi, definisi indeks/bobot, kasus tepi | Skema remunerasi, distribusi | PRL-DEC-24, 45 | `BLOCKING` | Pemilik, Direksi, HR | `OPEN` |
| DEC-PRL-013 | Pejabat penyetuju `ApproveException` dan Persetujuan Remunerasi | Persetujuan | Direktur SDM atau ditunjuk Direksi | `NON_BLOCKING_STANDARD` untuk desain; wajib sebelum go-live | Direksi | `OPEN` |
| DEC-PRL-014 | Entri registry dan prefix untuk master baru di luar `Mst`/`Prl` yang sudah ada (Kebijakan THR, kebijakan tunjangan jabatan/profesi/aktivitas, skema remunerasi, Sumber Dana, pool, distribusi) | Seluruh master baru | — | `BLOCKING` untuk entity baru tersebut | Pemilik repository | `OPEN` |
| DEC-PRL-015 | Tanggal batas migrasi data tunjangan lama, strategi, rekonsiliasi | Peralihan tunjangan | Saklar `AllowProfileAllowanceFallback` | `NON_BLOCKING_STANDARD` (`CONFIGURABLE_DEFAULT` untuk tanggal) | Pemilik bisnis | `OPEN` |
| DEC-PRL-016 | Isi "pengaturan" menu PPH21/BPJS selama perhitungan otomatis ditunda | Menu 6 | Tampilan baca-saja | `NON_BLOCKING_STANDARD` | Pemilik, Finance | `OPEN` |
| DEC-PRL-017 | Apakah sidebar Payroll ditata ulang mengikuti 13 menu, atau butir baru ditambahkan ke kelompok yang sudah ada | Navigasi | Tambahkan ke kelompok yang ada pada MVP | `NON_BLOCKING_STANDARD` | Pemilik | `OPEN` |

`DEC-PRL-017` lahir pada pass desain 12 November 2026, karena daftar 13 menu berbeda susunan
dengan sidebar yang sudah berjalan.

## 6. Hasil requirement-completeness-gate (12 November 2026)

| Slice | Kesiapan | Penghalang |
|---|---|---|
| Input variabel: insentif, bonus, honor tugas | `READY_FOR_DOMAIN_DESIGN` | — |
| Simulasi | `READY_FOR_DOMAIN_DESIGN` | — |
| Lembur baca-saja | `READY_FOR_DOMAIN_DESIGN` | — |
| Laporan beban gaji per cost center | `READY_FOR_DOMAIN_DESIGN` | — |
| Tunjangan tetap dari Struktur Gaji | `PARTIALLY_READY` | Penyimpangan per pegawai (DEC-PRL-002, 013); migrasi (DEC-PRL-015) |
| Batas grade | `PARTIALLY_READY` | Alur pengecualian penetapan gaji (DEC-PRL-002) |
| Tunjangan berbasis aktivitas | `PARTIALLY_READY` | Shift, Siaga, Risiko (DEC-PRL-005) |
| Menu 6 PPh/BPJS | `PARTIALLY_READY` | "Pengaturan" (DEC-PRL-016) |
| Kebijakan Struktural/Fungsional | `BUSINESS_DECISION_REQUIRED` | DEC-PRL-003, 004, 014 |
| Pensiun mesin Modern | `BUSINESS_DECISION_REQUIRED` | DEC-PRL-001 |
| Run THR dan bonus tahunan | `BUSINESS_DECISION_REQUIRED` | DEC-PRL-006, 007 |
| Sumber Dana | `BUSINESS_DECISION_REQUIRED` | DEC-PRL-008, 014 |
| Dashboard | `BUSINESS_DECISION_REQUIRED` | DEC-PRL-009 |
| Aturan Komponen | `BUSINESS_DECISION_REQUIRED` | DEC-PRL-010 |
| Seluruh Remunerasi | `BUSINESS_DECISION_REQUIRED` | DEC-PRL-011, 012, 013, 014 |

Kutipan regulasi pada dokumen jawaban (Permenaker 6/2016, PP 58/2023, PMK 168/2023) berstatus
`PROPOSED` dan belum diverifikasi; verifikasinya wewenang HRD dan Legal.

## 7. Kriteria penerimaan yang sudah dapat diuji

1. Pegawai dengan struktur terisi menampilkan baris tunjangan per jenis di slip.
2. Pegawai dengan struktur kosong dan angka profil terisi memakai angka profil dengan peringatan.
3. Struktur Gaji dengan batas di luar rentang grade ditolak saat disimpan.
4. Input insentif, bonus, atau honor yang belum disetujui tidak ikut dihitung.
5. Simulasi tidak membuat run dan tidak mengunci periode.
6. Payroll tidak membuat jurnal.
7. Menu PPH21 dan BPJS menampilkan nilai tetapan profil, tanpa perhitungan otomatis.
8. Setelah pensiun Modern, hanya mesin V2 yang menulis run.
9. Tunjangan transport yang berlaku muncul sebagai baris penghasilan di slip dan tidak terbayar dua kali.
