# Laporan Perubahan Backend — `PAY-BE-18`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-18` |
| Judul | Hitung transport bulanan tetap |
| Slice | `MVP-1` — `EPIC PGJ-03` |
| Roadmap | `docs/module-blueprints/payroll-management/payroll-gaji/roadmap/backend-roadmap.md`, kartu `PAY-BE-18` |
| Trace | FR-PGJ-022, 023, 024; PRL-DEC-46; `../../../02-backend-architecture.md` bagian 5.3; `../../../contracts/validation-matrix.md` bagian 4 (`CALC-01`, `CALC-02`) |
| Contract version | `PGJ-C1` — `approved` (Yasmin, 2026-11-12) |
| Dependency | `PAY-BE-17` 🟡 (berkas yang sama; alur tunjangan tetap sudah ditulis) |
| Klasifikasi | `MEDIUM` — skor 4: repository 0, berkas diperiksa 1, berkas diubah 0 (1 berkas), logika bisnis 2 (prorata, batas, irisan masa berlaku, galat per pegawai), kontrak API 0, database 1 (perilaku persistence yang ada), keamanan 0, UI 0 (hasil tampil di detail run dan slip yang sudah membaca baris komponen) |
| Task mode | `BACKEND` |
| Target tulis | `Areas/Corporate/PayrollManagement/Services/PayrollCalculationService.cs`; dokumen laporan dan penandaan status |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6722d7ca` (branch `Yasmina`), di atas perubahan `PAY-BE-14`–`17` yang belum di-commit |
| Tanggal | 2026-11-12 |
| Status | 🟡 — source selesai; empat kriteria dan empat kasus tepi terbukti lewat simulasi; **`dotnet build` tidak dijalankan atas instruksi pemilik**; uji proses di DB uji menunggu pemilik |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `Corporate` / `PayrollManagement` |
| Pemilik / prefix | Payroll — `Prl` (registry `ACTIVE`) |
| Keberlakuan | `TOUCHED LEGACY` — service mesin hitung yang sudah ada; tidak ada entity, controller, atau migration baru |
| QBE yang berlaku | `QBE-SVC-001` (logika di service, bukan controller), `QBE-CODE-002/003` (tidak ada alokasi nomor), `QBE-NAM-001` (nama class dan method) |
| Hasil | Tidak ada pelanggaran pada diff; checker resmi `NOT RUN` (tanpa `pwsh`) |

## 1. Masalah yang diperbaiki

Menu Tunjangan Transportasi sudah punya kebijakan (`PrlTransportAllowancePolicy`) dan tunjangan
per pegawai (`PrlTransportAllowance`), tetapi run gaji tidak membacanya sama sekali. Transport tidak
muncul di slip kecuali dimasukkan manual.

## 2. Proses bisnis

**Tujuan.** Run menghitung tunjangan transport bulanan tetap dari data yang sudah diisi di menu
Tunjangan Transportasi (PRL-DEC-46).

**Pelaku.** Petugas Payroll menekan "Hitung" pada run; sistem menghitung tiap pegawai.

**Langkah per pegawai:**

1. Sistem mengambil tunjangan transport pegawai yang aktif, berstatus `Active`, dan masa
   berlakunya beririsan dengan periode. Data dimuat sekali untuk seluruh pegawai di run.
2. Tunjangan dilewati tanpa pesan bila kebijakannya tidak `IsIncludedInPayroll`.
3. Bila kebijakannya bukan `FixedMonthly` (per kehadiran, per hari, penggantian), tunjangan tidak
   dihitung dan petugas mendapat peringatan `CALC-02`.
4. Bila kebijakannya terhapus, nonaktif, atau tidak ada, tunjangan tidak dihitung dan petugas
   mendapat peringatan dengan arahan yang sama.
5. Hari berlaku = irisan masa berlaku tunjangan dengan jendela hari dibayarkan pegawai (periode,
   tanggal bergabung/berhenti, masa berlaku profil payroll).
6. Bila dua tunjangan bulanan tetap beririsan, pegawai itu **gagal dihitung** dengan pesan
   `CALC-01`. Pegawai lain tetap terhitung.
7. Nilai sebulan = `MonthlyAmount` tunjangan pegawai; bila nol, `FixedMonthlyAmount` kebijakan.
   Bila `MonthlyLimitAmount` kebijakan lebih dari 0, nilai dipotong ke batas itu.
8. Bila kebijakan `IsProrated`: nilai × hari berlaku / hari periode. Bila tidak: nilai penuh, hanya
   untuk tunjangan yang terakhir berlaku di jendela itu.
9. Setiap tunjangan menjadi satu komponen `PAY_TRANSPORT_ALLOWANCE` bernama "Tunjangan
   Transportasi", sumber `TransportAllowance`, dan status pajak dari `IsTaxable` kebijakan.

**Contoh berangka** (kontrak bagian 5.3 dan kartu task):

| Kasus | Masukan | Hasil |
| --- | --- | --- |
| Kriteria 1 — prorata | Kebijakan Rp 400.000, diprorata; berlaku mulai 11 November (20 dari 30 hari) | Rp 266.666,67 |
| Kriteria 2 — batas | Tunjangan pegawai Rp 500.000; batas kebijakan Rp 300.000; tidak diprorata | Rp 300.000 |
| Kriteria 3 — ganda | Dua tunjangan bulanan: satu sebulan penuh, satu mulai 20 November | Pegawai gagal, `CALC-01` |
| Kriteria 4 — metode lain | Kebijakan `PerAttendance` | Tanpa baris; peringatan `CALC-02` |

**Jalur tidak normal:**

- **Dua tunjangan berurutan, tidak diprorata** (1–15 Rp 400.000, 16–30 Rp 500.000): hanya yang
  terakhir dibayar, Rp 500.000. Tidak ada bayar ganda.
- **Dua tunjangan berurutan, diprorata**: masing-masing sesuai harinya, Rp 200.000 + Rp 300.000.
- **Satu tunjangan berakhir tanggal 15, tidak diprorata**: dibayar penuh Rp 400.000, karena
  kebijakannya memang tidak memprorata.
- **Run dihitung ulang setelah tunjangan dicabut**: baris transport lama otomatis ditandai terhapus
  oleh pembersihan baris mesin yang sudah ada.
- **Peringatan `CALC-02`** tidak menahan pengajuan run. Galat `CALC-01` menahan, seperti pegawai
  bergalat lainnya.

**Hasil akhir.** Slip dan detail run menampilkan baris "Tunjangan Transportasi". Nominal transport
masuk ke `TotalRecurringEarning` dan `GrossPay`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PayrollCalculationService.cs` (alur hitung, `Add`/`UpsertComponent`, pembersihan baris mesin,
`Round`, `MaxDate`/`MinDate`), `PrlTransportAllowance.cs`, `PrlTransportAllowancePolicy.cs`,
`WfpTransportAllowanceController.cs` dan `WfpTransportAllowancePolicyController.cs` (nilai status
dan metode), `PayrollStatusCatalog.cs`, `ApplicationDbContext.cs`, `02-backend-architecture.md`
bagian 5.3, `validation-matrix.md`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/PayrollManagement/Services/PayrollCalculationService.cs` | (1) memuat tunjangan transport beserta kebijakannya, sekali per run, dikelompokkan per pegawai; (2) parameter `transportAllowances` baru di `CalculateEmployee`; (3) `BuildTransportAllowanceLines` dan record `TransportAllowanceLine` baru; (4) baris `PAY_TRANSPORT_ALLOWANCE` ditulis lewat `Add` yang sudah ada; (5) nominal transport ditambahkan ke `TotalRecurringEarning` dan `GrossPay` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Bentuk respons tidak berubah. Baris komponen run bertambah jenis `PAY_TRANSPORT_ALLOWANCE` dengan `SourceType = TransportAllowance` |
| Database | Tidak ada perubahan skema. Satu query baca tambahan per run |
| Keamanan/Auth | `NOT APPLICABLE` |

**Perubahan perilaku yang terlihat:** pegawai yang sudah punya tunjangan transport bulanan tetap
aktif langsung menerima baris transport pada hitung berikutnya, sehingga gaji brutonya naik.
Pegawai dengan data transport lama yang beririsan menjadi gagal dihitung.

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — tidak ada endpoint baru. Hasil terlihat pada endpoint yang sudah ada
(`POST /payroll-runs/{id}/calculate`, detail run, detail pegawai dalam run, slip).

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan atas instruksi pemilik | `NOT RUN` | — |
| Simulasi empat kriteria dengan langkah dan pembulatan yang sama (`Decimal`, 2 desimal, setengah ke atas) | Rp 266.666,67; Rp 300.000; `CALC-01`; `CALC-02` tanpa baris | `PASS` | Skrip simulasi |
| Simulasi empat kasus tepi | Berurutan tanpa prorata Rp 500.000 saja; berurutan prorata Rp 200.000 + Rp 300.000; berakhir tanggal 15 tanpa prorata Rp 400.000; tidak `IsIncludedInPayroll` tanpa baris dan tanpa pesan | `PASS` | Skrip simulasi |
| Bunyi pesan `CALC-01` dan `CALC-02` | Sama persis dengan `validation-matrix.md` baris 70–71 | `PASS` | `grep` |
| Keseimbangan kurung | `{}` 77/77, `()` 409/409, `[]` 14/14 | `PASS` | Skrip penghitung |
| Rujukan helper dan properti | `MaxDate`, `MinDate`, `Round`, `windowStart`/`windowEnd`, `PolicyCode`, DbSet `PrlTransportAllowances` ada | `PASS` | `grep` |
| Galat per pegawai | `errors` dibuat baru per pegawai (baris 172), jadi `CALC-01` tidak menular | `PASS` | Review source |
| QBE pada berkas ini | Service, bukan controller atau entity; tidak ada `Trx*`, alokasi nomor, atau akses data di controller | `PASS` | Review diff |
| Uji proses di DB uji | Menunggu pemilik | `NOT RUN` | Langkah di bawah |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `REQUIRED` — dijalankan pemilik setelah build.

**Langkah uji untuk pemilik** (DB uji, data samaran, periode 30 hari):

1. Buat kebijakan "Transport Bulanan" `FixedMonthly` Rp 400.000, diprorata, `IsIncludedInPayroll`.
   Beri satu pegawai tunjangan berlaku mulai tanggal 11. Hitung run. Harapan: baris "Tunjangan
   Transportasi" Rp 266.666,67.
2. Ubah kebijakan menjadi tidak diprorata dengan batas bulanan Rp 300.000, lalu isi nominal
   tunjangan pegawai Rp 500.000. Hitung ulang. Harapan: Rp 300.000.
3. Tambah tunjangan bulanan kedua yang beririsan untuk pegawai yang sama. Hitung ulang. Harapan:
   pegawai itu bergalat `CALC-01`; pegawai lain tetap terhitung.
4. Hapus tunjangan kedua. Buat kebijakan `PerAttendance` dan beri pegawai lain tunjangannya. Hitung
   ulang. Harapan: peringatan `CALC-02`, tanpa baris transport untuk pegawai itu.
5. Pastikan `GrossPay` naik sebesar baris transport, dan baris transport hilang setelah
   tunjangannya dicabut lalu run dihitung ulang.

**Tidak dijalankan:** `dotnet build`, uji proses, checker QBE resmi.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Rp 400.000 diprorata 20 dari 30 hari = Rp 266.667 | Terpenuhi (simulasi) dengan selisih bunyi: mesin membulatkan ke 2 desimal, jadi hasilnya Rp 266.666,67. Kontrak bagian 5.3 menyerahkan pembulatan ke "aturan pembulatan mesin hitung yang sudah ada"; aturan itu tidak diubah | Bagian 5 |
| 2. `MonthlyLimitAmount` membatasi | Terpenuhi (simulasi) | Bagian 5 |
| 3. Dua tunjangan bulanan beririsan: pegawai gagal `CALC-01`, pegawai lain terhitung | Terpenuhi (simulasi dan review source) | Bagian 5 |
| 4. Metode lain: peringatan `CALC-02`, tanpa baris | Terpenuhi (simulasi) | Bagian 5 |
| DoD: build hijau | **Belum terpenuhi** | `NOT RUN` atas instruksi pemilik |
| DoD: QBE bersih | Terpenuhi untuk aturan yang dicek; checker resmi `NOT RUN` | Bagian 5 |
| DoD: kriteria terbukti | Terpenuhi lewat simulasi; di DB uji **belum** | Langkah uji menunggu pemilik |
| DoD: laporan ditulis | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Keputusan teknis | (1) Tanpa prorata, hanya tunjangan yang terakhir berlaku di jendela bayar yang dibayar penuh, mengikuti pola `coveringAssignment` di `PAY-BE-17`. Ini mencegah bayar ganda saat dua tunjangan berurutan dalam sebulan. (2) Kebijakan yang hilang atau nonaktif diberi peringatan bergaya `CALC-02`, bukan dilewati diam-diam. (3) Konstanta metode `FixedMonthly` ditulis privat di service karena belum ada konstanta bersama; controller lama memakai literal |
| Masalah yang diketahui | (1) Kolom `MaximumMonthlyAmount` pada tunjangan pegawai tidak dipakai, karena kontrak hanya menyebut batas kebijakan (`MonthlyLimitAmount`). (2) Perbandingan `AllowanceStatus = "Active"` peka huruf besar-kecil di PostgreSQL; controller transport menyimpan nilai dari daftar baku, jadi aman selama data tidak diisi langsung ke database. (3) Kolom `PayrollComponentId` pada baris komponen tidak diisi, sama seperti baris mesin lain |
| Risiko tersisa | Data transport lama yang beririsan menggagalkan pegawai. Sebelum run produksi, periksa tunjangan bulanan aktif yang beririsan per pegawai |
| Perubahan sampingan | `NONE` |
| Interupsi | Konteks percakapan dipadatkan di tengah task; dilanjutkan dari isi berkas yang terverifikasi, tanpa penyuntingan ganda |
| Status Git | Task ini: `M` `PayrollCalculationService.cs` (bersama perubahan `PAY-BE-17`); ditambah laporan dan penandaan roadmap. Perubahan `PAY-BE-14`–`17` yang belum di-commit masih ada. Tidak ada yang di-stage atau di-commit |
| Langkah berikutnya | Pemilik: build dan langkah uji bagian 5. Task berikutnya: `PAY-BE-19` (input variabel manual), lalu `PAY-BE-20` |
