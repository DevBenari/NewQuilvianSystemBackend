# Permintaan Payroll kepada MHamzah: Satu Mesin Hitung Gaji

| Field | Nilai |
|---|---|
| Dari | Yasmin, pemilik modul Payroll |
| Untuk | MHamzah, pemilik kode Payroll Modern di tim integrasi |
| Tanggal | 12 November 2026 |
| Sifat | **Permintaan persetujuan dan enam pertanyaan.** Belum ada kode Anda yang diubah, dan tidak ada yang dimatikan sebelum Anda setuju |
| Menutup | `DEC-PRL-001` |
| Dasar | `PRL-DEC-44`, disetujui pemilik Payroll pada 12 November 2026 |
| Yang memblokir | Pensiun mesin Payroll Modern, penghapusan nominal tunjangan yang ditulis langsung di kode, dan penyatuan perhitungan PPh 21 dan BPJS |
| Bukti diambil dari | Backend `NewQuilvianSystemBackend` commit `89b9fd69` (branch `Yasmina`, sudah memuat merge `origin/QuilvianIntegrationBackend` `adfde228`); frontend `QuilvianSystemFrontendDev` commit `bfa3bae69` (branch `yasmina`) |

Berkas ini berdiri sendiri. Anda dapat membacanya tanpa membuka dokumen Payroll lain.

---

## 1. Inti permintaan dalam satu paragraf

Saat ini ada **dua mesin hitung gaji** di backend, dan keduanya menulis ke tabel run yang sama
(`PrlPayrollRuns`). Mesin pertama, yang kami sebut **mesin V2**, dipakai seluruh layar `/payroll`
dan memuat keputusan pemilik Payroll sejak 11 November 2026. Mesin kedua, **mesin Payroll
Modern**, adalah milik Anda. Pemilik Payroll sudah memutuskan bahwa **mesin V2 menjadi
satu-satunya mesin hitung gaji**, dan mesin Modern **dipensiunkan secara bertahap** dengan tiga
syarat: bagian yang berguna dipindahkan lebih dulu, hasil kedua mesin direkonsiliasi, dan Anda
sebagai pemilik teknis menyetujuinya. Kami membutuhkan persetujuan Anda dan jawaban atas enam
pertanyaan di bagian 7.

---

## 2. Mengapa dua mesin menjadi masalah

Dua mesin yang menulis ke tabel yang sama bisa menghasilkan **dua angka gaji yang berbeda untuk
pegawai dan periode yang sama**, tanpa ada yang tahu mana yang benar.

**Contoh:** untuk periode Oktober 2026, mesin V2 membuat run nomor `PRL-RUN-2026-10-0001` dan
menghitung gaji Ns. Rina (nama samaran) sebesar Rp 7.250.000. Bila mesin Modern juga membuat run
untuk periode yang sama, tunjangan risikonya dihitung dari kata "icu" pada nama departemen, yaitu
Rp 1.200.000. Mesin V2 tidak mengenal tunjangan itu. Akibatnya ada dua run Oktober dengan selisih
Rp 1.200.000 untuk orang yang sama, dan Finance bisa menerima instruksi bayar dari salah satunya.

Masalah kedua: nominal tunjangan yang ditulis langsung di kode hanya bisa diubah dengan mengubah
program, dan pegawai bisa kehilangan tunjangannya hanya karena ejaan nama jabatannya berubah.
Misalnya "Kepala Ruangan" diganti menjadi "Ka. Ruang", sehingga kata kunci "kepala ruangan" tidak
lagi cocok.

---

## 3. Yang kami temukan, beserta buktinya

Semua klaim di bawah dapat Anda periksa sendiri pada commit yang disebut.

| No | Temuan | Bukti |
|---:|---|---|
| 1 | Mesin Modern membuat baris `PrlPayrollRun` baru | `NewQuilvianSystemBackend/Areas/Corporate/PayrollManagement/Services/PayrollRunInitializationService.cs` baris 61 dan 128 @ `89b9fd69` |
| 2 | Mesin V2 membaca dan menulis tabel yang sama | `NewQuilvianSystemBackend/Areas/Corporate/PayrollManagement/Services/PayrollRunService.cs` baris 80, 110, dan 484 @ `89b9fd69` |
| 3 | Endpoint mesin Modern masih aktif di dua alamat | `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PayrollManagement/Controllers/PayrollRunModernController.cs` baris 15 dan 16 @ `89b9fd69` |
| 4 | Sepuluh layanan Modern terdaftar di aplikasi | `NewQuilvianSystemBackend/Program.cs` baris 1449 sampai 1470 @ `89b9fd69` |
| 5 | Satu-satunya pemakai layanan inti Modern di backend adalah `PayrollRunModernController` | Hasil pencarian `IPayrollEarningsCalculationService`, `IPayrollRunInitializationService`, `IPayrollTaxCalculationService`, `IPayrollBpjsCalculationService` pada folder `Areas` @ `89b9fd69` |
| 6 | Tiga layar frontend untuk mesin Modern tidak dipakai di mana pun: tidak punya rute, tidak ada di menu | `QuilvianSystemFrontendDev/src/components/view/hr/payroll/payroll-run-wizard-view.jsx`, `payroll-worksheet-view.jsx`, `payroll-approvals-view.jsx`; tidak ada berkas lain yang mengimpornya @ `bfa3bae69` |
| 7 | Tunjangan fungsional ditentukan dari kata kunci nama jabatan, dengan nominal di kode | `PayrollEarningsCalculationService.cs` baris 333 sampai 359 @ `89b9fd69` |
| 8 | Tunjangan struktural ditentukan dari kata kunci nama jabatan | `PayrollEarningsCalculationService.cs` sekitar baris 325 sampai 331 @ `89b9fd69` |
| 9 | Tunjangan risiko ditentukan dari kata kunci nama departemen dan jabatan | `PayrollEarningsCalculationService.cs` baris 361 sampai 383 @ `89b9fd69` |
| 10 | Tunjangan shift malam memakai satu nominal pengaturan, bawaan Rp 50.000 per shift | `PayrollSettingService.cs` baris 99; `PayrollOvertimeAndShiftIngestionService.cs` baris 314 @ `89b9fd69` |
| 11 | Mesin Modern punya perhitungan PPh 21 dan BPJS sendiri | `PayrollRunModernController.cs` baris 225 (`calculate-bpjs`) dan 268 (`calculate-pph21`) @ `89b9fd69` |
| 12 | Mesin V2 memakai nilai PPh dan BPJS tetapan profil pegawai, sesuai keputusan 11 November 2026 | `PayrollCalculationService.cs` baris 323 sampai 325 @ `89b9fd69` |

### Contoh nominal yang saat ini tertulis di kode

| Jika nama jabatan atau departemen mengandung | Tunjangan yang diberikan | Jenis |
|---|---:|---|
| "spesialis", "subspesialis", "konsultan" | Rp 4.500.000 | Fungsional |
| "dokter umum", "dokter gigi", "dokter ruangan" | Rp 2.500.000 | Fungsional |
| "perawat" dan "pk iv" atau "utama" | Rp 1.750.000 | Fungsional |
| "perawat" dan "pk i" atau "pertama" | Rp 600.000 | Fungsional |
| "kepala ruangan", "karu", "supervisor" | Rp 1.500.000 | Struktural |
| "isolasi", "pinere", "tb-mdr", "infeksius" | Rp 1.500.000 | Risiko |
| "icu", "iccu", "picu", "nicu" | Rp 1.200.000 | Risiko |
| "igd", "gawat darurat" | Rp 1.000.000 | Risiko |

Kami belum tahu apakah angka-angka ini berasal dari kebijakan rumah sakit yang sah. Itu salah
satu pertanyaan kami di bagian 7.

---

## 4. Keputusan pemilik Payroll yang berkaitan dengan kode Anda

| ID | Keputusan | Status |
|---|---|---|
| `PRL-DEC-44` | Mesin V2 satu-satunya mesin hitung gaji. Mesin Modern dipensiunkan bertahap dengan tiga syarat | Disetujui pemilik Payroll, **menunggu persetujuan Anda** |
| `PRL-DEC-14` | Tunjangan struktural dan fungsional disimpan sebagai master kebijakan per jabatan, profesi, dan jenjang | Disetujui pemilik Payroll |
| `PRL-DEC-15` | Setiap kebijakan tunjangan punya masa berlaku, metode hitung, persyaratan penerima, dan riwayat perubahan. Tidak ada nominal yang ditulis di kode | Disetujui pemilik Payroll |
| `PRL-DEC-33` | PPh 21 dan BPJS tetap memakai nilai tetapan profil sampai rumusnya disahkan HRD, Finance, dan bagian pajak | Disetujui pemilik Payroll |
| `PRL-DEC-48` | Data aktivitas (jumlah shift, siaga, panggilan) disediakan HR dan Scheduling, lalu diimpor mesin V2 | Disetujui pemilik Payroll |

---

## 5. Usulan pensiun bertahap

Bagian ini **usulan untuk dibahas bersama**, belum keputusan.

**Tujuan.** Hanya satu mesin yang menghitung dan menulis run gaji, tanpa ada perhitungan berguna
yang hilang.

**Pelaku.**

| Pelaku | Tugas |
|---|---|
| MHamzah | Menyetujui rencana, menunjukkan bagian Modern yang masih dipakai, dan menyetujui pematian akhir |
| Pemilik Payroll | Memutuskan bagian mana yang dipindahkan ke V2, dan menyetujui hasil rekonsiliasi |
| Tim Payroll | Memindahkan bagian yang disetujui dan menjalankan rekonsiliasi |

**Pemicu.** Anda menyetujui `PRL-DEC-44`.

**Prasyarat.** Daftar pemakai endpoint Modern sudah diketahui (pertanyaan 2), dan ada satu periode
uji yang bisa dihitung oleh kedua mesin.

**Langkah utama.**

1. Anda dan pemilik Payroll menyepakati daftar bagian Modern yang berguna (lihat tabel di bawah).
2. Mesin Modern **dibekukan**: tidak ada fitur baru, hanya perbaikan bila ada kerusakan.
3. Tim Payroll memindahkan bagian yang disepakati ke mesin V2, atau menggantinya dengan master
   kebijakan.
4. Kedua mesin menghitung periode uji yang sama, lalu hasilnya dibandingkan per pegawai.
5. Setiap selisih dijelaskan dan disetujui pemilik Payroll.
6. Anda menyetujui pematian. Endpoint Modern dinonaktifkan, lalu layanan dan layar yang tidak
   dipakai dihapus pada perubahan terpisah.

**Contoh rekonsiliasi pada langkah 4.** Periode uji Oktober 2026 dengan 120 pegawai. Mesin V2
menghasilkan total gaji kotor Rp 912.400.000, mesin Modern Rp 958.000.000. Selisih
Rp 45.600.000 harus bisa dijelaskan per pegawai. Misalnya 38 pegawai ICU masing-masing menerima
tunjangan risiko Rp 1.200.000 di Modern (38 × Rp 1.200.000 = Rp 45.600.000), sedangkan di V2
tunjangan itu baru muncul setelah master kebijakan risiko diisi. Bila seluruh selisih terjelaskan
seperti ini, langkah 5 dapat disetujui.

**Usulan nasib tiap bagian mesin Modern.**

| Bagian | Usulan |
|---|---|
| `PayrollEarningsCalculationService` (tunjangan fungsional, struktural, risiko) | Diganti master kebijakan tunjangan. Nominal di kode tidak dipindahkan apa adanya sebelum dikonfirmasi sebagai kebijakan sah |
| `PayrollOvertimeAndShiftIngestionService` (shift malam) | Diganti tarif dari master kebijakan shift, dengan data jumlah shift dari HR dan Scheduling |
| `PayrollTaxCalculationService`, `PayrollBpjsCalculationService` | Dibekukan. Menjadi bahan rujukan saat rumus PPh 21 dan BPJS disahkan, bukan dipakai langsung |
| `PayrollVarianceGuardService`, `PayrollPreconditionGateService`, `PayrollDeductionEngineService`, `PayrollFinanceHandoffService`, `PayrollApprovalWorkflowService` | Perlu ditinjau bersama: mana yang punya fungsi yang belum ada di V2 |
| `PayrollRunInitializationService`, `PayrollRunModernController` | Dinonaktifkan pada langkah 6 |
| Tiga layar frontend di `src/components/view/hr/payroll/` | Dihapus pada langkah 6, karena saat ini tidak terjangkau pengguna |

**Perubahan status mesin Modern.**

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
|---|---|---|---|---|
| Aktif | Bekukan | Dibekukan | MHamzah | `PRL-DEC-44` disetujui |
| Dibekukan | Mulai rekonsiliasi | Rekonsiliasi | Pemilik Payroll | Bagian yang berguna sudah dipindahkan |
| Rekonsiliasi | Setujui hasil | Siap dimatikan | Pemilik Payroll | Seluruh selisih per pegawai terjelaskan |
| Siap dimatikan | Matikan | Pensiun | MHamzah | Tidak ada pemakai endpoint yang tersisa |

**Jalur tidak normal.**

- **Ada selisih yang tidak terjelaskan:** status kembali ke Dibekukan, lalu sumber selisih
  diperbaiki di mesin V2 sebelum rekonsiliasi diulang.
- **Ditemukan pemakai endpoint yang belum terdaftar:** pematian ditunda sampai pemakai itu pindah
  ke endpoint V2.
- **Ada kerusakan mendesak di mesin Modern selama dibekukan:** boleh diperbaiki, dengan catatan di
  log perubahan.

**Hasil akhir.** Hanya mesin V2 yang menulis `PrlPayrollRuns`. Tidak ada nominal tunjangan di
kode, dan satu mekanisme PPh 21 dan BPJS.

---

## 6. Yang tidak kami minta, dan yang tidak berubah

| Tidak kami minta sekarang | Tetap seperti semula |
|---|---|
| Mengubah atau menghapus kode Anda | Endpoint Modern tetap berjalan sampai langkah 6 |
| Memindahkan berkas Payroll Modern ke folder lain | Berkas Payroll Modern tetap di lokasinya |
| Migration atau perubahan database | Tidak ada perubahan tabel |
| Mengubah nama entity atau tabel Anda | Entity dan tabel tim integrasi tetap bernama seperti sekarang |

---

## 7. Enam pertanyaan untuk MHamzah

| # | Pertanyaan | Mengapa penting |
|---:|---|---|
| 1 | Apakah Anda menyetujui `PRL-DEC-44` beserta tiga syaratnya dan urutan langkah di bagian 5? | Tanpa persetujuan Anda, pensiun tidak bisa dimulai |
| 2 | Siapa saja pemakai endpoint `api/v1/corporate/human-resource/payroll/runs` dan `api/payroll/runs` selain tiga layar frontend yang tidak terjangkau itu? Misalnya aplikasi lain, tugas terjadwal, atau tim lain | Pematian tidak boleh memutus pemakai yang belum kami ketahui |
| 3 | Apakah rumus PPh 21 dan BPJS di mesin Modern sudah pernah dicocokkan dengan aturan yang berlaku? Bila ya, dengan data uji apa? | Bila sudah, perhitungan itu bisa menjadi bahan rujukan saat rumus disahkan |
| 4 | Dari mana nominal tunjangan di bagian 3 berasal: SK direksi, kebijakan rumah sakit, atau angka contoh? | Menentukan apakah angka itu boleh dipindahkan ke master kebijakan apa adanya (`DEC-PRL-004`) |
| 5 | Branch `origin/MHamzah` commit `f751f8ca` berbeda 10 baris pada `PayrollEarningsCalculationService.cs` dan 21 baris pada `PayrollCalculationService.cs` dibandingkan commit `89b9fd69`. Apa isinya, dan kapan akan masuk ke `QuilvianIntegrationBackend`? | Perubahan pada `PayrollCalculationService` menyentuh mesin V2 secara langsung |
| 6 | Dua controller memakai rute yang persis sama, `api/hr-deductions`: `RestitutionClaimController.cs` baris 12 dan `PayrollDeductionScheduleController.cs` baris 12 di folder `Areas/Corporate/HumanResource/PayrollManagement/Controllers` @ `89b9fd69`. Mana yang seharusnya memakai rute itu? | Rute ganda bisa membuat salah satu endpoint tidak bisa dipanggil |

Catatan untuk pertanyaan 5: ref `origin/MHamzah` yang kami bandingkan adalah salinan lokal, belum
diperbarui dari server, sehingga isinya bisa sudah berbeda.

---

## 8. Rujukan

| Berkas | Isi |
|---|---|
| `docs/module-blueprints/payroll-management/00-interview-decisions.md` | `PRL-DEC-44` beserta alasan dan keputusan lain yang berkaitan |
| `docs/module-blueprints/payroll-management/01-existing-capability-map.md` | Peta kemampuan lengkap, termasuk baris `X-02` (dua mesin) dan `P-10` (tunjangan di kode) |
