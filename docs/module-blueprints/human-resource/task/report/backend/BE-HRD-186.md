# Laporan Penyelesaian Backend Task: BE-HRD-186

## 1. Identitas Task
* **ID Task**: `BE-HRD-186`
* **Nama Task**: Layanan API SIMRS real-time inquiry eligibilitas & pemotongan saldo plafon loket kasir RS (`INT-HR-61`, `chk_benefit_balance_non_negative`, `BEN-B4`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 12 — Benefit Management (Manajemen Manfaat, Asuransi BPJS & Tanggungan Pegawai Rumah Sakit)
* **Slice / Milestone**: `Slice S-D3` · `Fase MVP-38: Integrasi SIMRS Kasir, Buku Besar Plafon, Klaim COB & Kliring Exit Clearance`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-09
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Tenaga medis, perawat, dan staf non-medis beserta keluarga inti sah di RS Quilvian memperoleh hak subsidi perawatan kesehatan di fasilitas rumah sakit sendiri (*In-House Healthcare Subsidy*). Agar proses admisi dan kasir SIMRS berjalan cepat tanpa menunggu konfirmasi manual HRD, sistem menyediakan integrasi online sinkronus real-time (`INT-HR-61`).

Layanan ini menegakkan aturan bisnis kritis:
1. **Inquiry Eligibilitas & Kuota Real-Time (`FR-BEN-11`, `FR-BEN-12`)**:
   - Petugas admisi loket SIMRS dapat memvalidasi apakah pasien merupakan nakes aktif atau tanggungan keluarga terverifikasi cukup dengan memasukkan NIK atau nomor identitas pegawai.
   - Sistem mengembalikan hak kelas kamar inap (VVIP, VIP, Kelas 1, Kelas 2), persentase diskon staf internal (misal 30% jasa medis), dan sisa saldo pagu berjalan.
2. **Peringatan Dini Batas Plafon Kasir SIMRS (`BEN-W2`)**:
   - Jika sisa kuota tahunan telah terpakai lebih dari 80% dari batas maksimal grade, sistem memunculkan indikator peringatan oranye pada layar kasir (`IsNearExhaustion = true`) agar kasir dapat mengomunikasikan potensi timbulnya biaya ekses mandiri kepada staf.
3. **Penegakan Saldo Plafon Non-Negatif (`BEN-B4`, `VR-BEN-14`, `VR-BEN-15`)**:
   - Saldo plafon subsidi berobat tidak boleh bernilai negatif (`balance_after >= 0.00`).
   - Setiap pemotongan di loket kasir SIMRS hanya dapat mengurangi saldo maksimal sebesar sisa limit berjalan (`balance_before`).
   - Upaya pemotongan melebihi saldo ditolak dengan pesan: *"Transaksi pemotongan saldo ditolak. Sisa saldo plafon tidak mencukupi untuk menutup pemotongan (VR-BEN-14, BEN-B4)."*
4. **Isolasi Konkurensi & Locking Transaksi Kasir (`BEN-W6`)**:
   - Mengunci transaksi kasir secara atomik untuk mencegah *race condition* (pemotongan saldo ganda) jika staf melakukan billing obat di farmasi bersamaan dengan billing tindakan di poli.

---

## 3. Komponen Teknis yang Dibuat & Dimodifikasi

### A. Model Basis Data Entity Framework Core
File: `Areas/Corporate/HumanResource/BenefitManagement/Models/HrdBenefitBalanceMovement.cs`
* Kolom pendukung: `WorkforceProfileId`, `ReferenceNumber` (Nomor Rawat SIMRS), `Description`, `Amount` (negatif untuk pemotongan), `BalanceBefore`, `BalanceAfter`.
* Check constraint: `chk_benefit_balance_non_negative` (`BalanceAfter >= 0.00`).

### B. Data Transfer Objects (DTO)
File: `Areas/Corporate/HumanResource/BenefitManagement/DTOs/BenefitInHouseAndClaimDtos.cs`
* `InHouseCoverageInquiryQuery`: Pencarian via `NationalIdNumber` (NIK) atau `EmployeeIdNumber`.
* `InHouseCoverageInquiryDto`: Mengembalikan status eligibilitas, hak kelas kamar, diskon RS (30%), pagu limit, saldo sisa, dan peringatan 80% (`BEN-W2`).
* `DeductInHouseBalanceRequestDto`: Payload kasir SIMRS memuat Nomor Rawat, total tagihan kotor, diskon, dan nominal plafon yang dipotong.
* `BenefitBalanceDeductionResultDto`: Rincian saldo sebelum, saldo sesudah, dan nominal ekses biaya yang dialihkan ke payroll.

### C. Logika Bisnis & Validasi Layanan
File: `Areas/Corporate/HumanResource/BenefitManagement/Services/BenefitInHouseService.cs`
* Metode: `InquireCoverageAsync` dan `DeductBalanceAsync`.
* Mengunci transaksi kasir, memotong saldo plafon, dan mencatat mutasi buku besar `HospitalCareUsage`.

---

## 4. Contoh Skenario Rumah Sakit Konkret
* **Skenario 1 — Pemeriksaan Poli Spesialis Dokter Staf RS**:
  - Ns. Dian berobat di Poli THT RS Quilvian.
  - Petugas admisi SIMRS memanggil `GET /coverage-inquiry?nationalIdNumber=320101...`.
  - Sistem mengembalikan: `IsEligible = true`, Diskon Jasa Medis = `30%`, Sisa Saldo = `Rp 8.500.000`.
  - Total tagihan tindakan kotor Rp 500.000, diskon staf 30% (-Rp 150.000), total bersih Rp 350.000.
  - Kasir memanggil `POST /deduct-balance` dengan nominal plafon Rp 350.000.
  - Plafon terpotong Rp 350.000, sisa saldo Dian menjadi Rp 8.150.000, dan tagihan Dian di kasir menjadi Rp 0 (tertanggung penuh).

* **Skenario 2 — Rawat Inap Melebihi Plafon (Ekses Biaya)**:
  - Anak dari staf farmasi dirawat inap di RS Quilvian dengan total tagihan bersih Rp 12.000.000.
  - Sisa saldo plafon keluarga yang tersisa hanya Rp 8.000.000.
  - Kasir memotong plafon maksimal Rp 8.000.000 (`balance_after = 0`).
  - Sisa selisih Rp 4.000.000 menjadi ekses tagihan (`out_of_pocket_excess_amount = Rp 4.000.000`) yang diterbitkan ke pemotongan payroll Sub-11 (`INT-HR-63`).

---

## 5. Verifikasi & Integritas Bukti
* **Zero Negative Balance**: Teruji tidak ada mutasi yang meloloskan saldo akhir di bawah Rp 0.00.
* **Audit Trail Kasir**: Dicatat log audit `[INT-HR-61]` memuat nomor rawat SIMRS dan breakdown potongan saldo.
* **Kepatuhan Acceptance Test**: Memenuhi kriteria penerimaan `AT-HRD-BEN-11` s.d. `AT-HRD-BEN-14`.


---

## Pengerjaan Ulang 10 November 2026 — Remediasi Gelombang 1 Audit Backend HR

| Field | Nilai |
| --- | --- |
| Dasar | [backend-unfinished-work.md](../../../roadmap/backend-unfinished-work.md) bagian 10 Gelombang 1, kelompok E — nomor dokumen dari angka acak |
| Task mode | `BACKEND` |
| Model | Claude Fable 5.1 |
| Commit backend saat dikerjakan | mulai `edfd843bac1c2860da7ae9a5ddcb68a75cf42666`; pemilik melakukan commit dan merge selama sesi (HEAD akhir `05325918`, branch `MHamzah`) |
| Status sesudah pengerjaan ulang | ✅ **SELESAI 10 November 2026** (dikerjakan ulang) |

### Masalah yang ditemukan audit

Referensi koreksi manual plafon `ADJ-{yyyyMMdd}-{acak 3 digit}` bila pemohon tidak mengisi nomor referensi.

### Perubahan yang dikerjakan

- Bila pemohon tidak mengisi referensi, nomor dialokasikan deret `HRD_BENEFIT_LEDGER_ADJUSTMENT` (harian, 4 digit): `ADJ-20261110-0001`. Referensi yang diisi pemohon tetap dipakai apa adanya.

### Berkas yang berubah

- `Areas/Corporate/HumanResource/BenefitManagement/Services/BenefitInHouseService.cs`

### Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` **0 error**, 606 warning (garis dasar sebelum sesi 578; selisih berasal dari merge cabang lain yang masuk HEAD pada sesi yang sama, bukan dari berkas task ini) | 0 error | `PASS` |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 58 berkas yang disentuh: **0 VIOLATION, 0 REVIEW, PASS** | 0 temuan | `PASS` |
| Review diff dan scope | hanya berkas di atas yang disentuh untuk task ini | `PASS` |
| Uji runtime / API | tidak dijalankan pada sesi ini | `NOT RUN` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

### Acceptance criteria yang disentuh pengerjaan ulang

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Referensi koreksi tidak lagi acak | Terpenuhi | alokasi dari provider bersama |

### Database dan migration

`NOT APPLICABLE` — tidak ada perubahan skema untuk task ini.

### Catatan penutup

- `ReferenceNumber` pada `HrdBenefitBalanceMovement` sengaja **tidak** diberi index unik: kolom itu juga menampung referensi bebas dari pemohon yang boleh berulang.
- Perubahan sampingan: migration hasil scaffold otomatis `20261110044224_AddHrdDurableAuditAndPinLockouts` (14.620 baris, 79 CreateTable/26 DropTable di luar lingkup) **dibuang** dan diganti migration SQL manual; snapshot model yang ikut ter-commit pemilik dibiarkan karena sudah bercampur dengan merge cabang lain.
- Status Git akhir sesi: berkas task sudah masuk HEAD lewat commit pemilik; sisa di working tree adalah penghapusan dua berkas scaffold dan satu berkas migration manual baru (lihat `git status --short` pada ringkasan sesi).
