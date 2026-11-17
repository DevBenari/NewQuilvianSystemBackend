# Laporan Perubahan Backend — `PAY-BE-17`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-17` |
| Judul | Hitung tunjangan dari struktur dan masa peralihan |
| Slice | `MVP-1` — `EPIC PGJ-01` |
| Roadmap | `docs/module-blueprints/payroll-management/payroll-gaji/roadmap/backend-roadmap.md`, kartu `PAY-BE-17` |
| Trace | FR-PGJ-004, 005, 006; PRL-DEC-02, 04, 13; `../../../02-backend-architecture.md` bagian 5.1 dan 5.2; `../../../contracts/validation-matrix.md` bagian 4 (`CALC-03`, `CALC-04`) |
| Contract version | `PGJ-C1` — `approved` (Yasmin, 2026-11-12) |
| Dependency | `PAY-BE-16` 🟡 (baris tunjangan dapat diisi lewat API) |
| Klasifikasi | `MEDIUM` — skor 5: repository 0, berkas diperiksa 1, berkas diubah 0 (1 berkas), logika bisnis 2 (kompleks: prorata per segmen, batas, peralihan), kontrak API 0, database 1 (perilaku persistence yang ada), keamanan 0, UI 1 (hasil tampil di detail run dan slip) |
| Task mode | `BACKEND` |
| Target tulis | `Areas/Corporate/PayrollManagement/Services/PayrollCalculationService.cs`; dokumen laporan dan penandaan status |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6722d7ca` (branch `Yasmina`), di atas perubahan `PAY-BE-14`–`16` yang belum di-commit |
| Tanggal | 2026-11-12 |
| Status | 🟡 — source selesai; angka contoh kontrak terbukti lewat simulasi; **`dotnet build` tidak dijalankan atas instruksi pemilik**; uji proses di DB uji menunggu pemilik |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, run gaji membaca tunjangan tetap dari satu angka `TotalAllowance` di profil
pegawai. Hasilnya satu baris "Tunjangan Tetap" di slip, selalu kena pajak, dan tidak bisa dirinci.
Tunjangan yang sudah diatur di Struktur Gaji (`PAY-BE-16`) tidak dipakai sama sekali.

## 2. Proses bisnis

**Tujuan.** Run menghitung tunjangan tetap per jenis dari Struktur Gaji penetapan gaji pegawai,
dengan masa peralihan yang aman (PRL-DEC-02, PRL-DEC-04).

**Pelaku.** Petugas Payroll menekan "Hitung" pada run; sistem menghitung tiap pegawai.

**Langkah per pegawai:**

1. Sistem mengambil segmen penetapan gaji yang disetujui di periode (sudah berjalan sebelumnya
   untuk gaji pokok).
2. Untuk tiap segmen, sistem mengambil baris tunjangan aktif milik struktur penetapan itu.
3. Nilai sebulan tiap baris: nominal tetap, atau gaji pokok penetapan × persen / 100.
4. Bila jenis tunjangan punya `MaximumAmount`, nilai sebulan dipotong ke batas itu.
5. Bila jenis tunjangan `IsProrated` dan kebijakan `ProrateFixedAllowances` menyala, nilai dikali
   hari segmen / hari periode. Bila tidak diprorata, nilai diambil penuh dari struktur penetapan
   yang menutup akhir periode.
6. Setiap baris menjadi satu komponen `PAY_STRUCTURE_ALLOWANCE` bernama jenis tunjangannya, dengan
   status pajak dari jenis tunjangan.

**Masa peralihan:**

| Keadaan | Hasil |
| --- | --- |
| Struktur berisi tunjangan, profil juga berisi angka | Baris dari struktur dipakai; angka profil diabaikan; peringatan `CALC-04` |
| Struktur kosong, profil berisi angka, saklar `AllowProfileAllowanceFallback` menyala (bawaan) | Satu baris lama "Tunjangan Tetap" (`PAY_FIXED_ALLOWANCE`) dari profil; peringatan `CALC-03` |
| Struktur kosong, saklar mati | Tanpa tunjangan tetap |

**Contoh berangka** (kontrak bagian 5.1): gaji pokok Rp 6.000.000; struktur memuat Tunjangan Jabatan
Rp 750.000 (diprorata), Tunjangan Makan Rp 500.000 (tidak diprorata), Tunjangan Keahlian 5% dengan
batas Rp 400.000 (diprorata).

| Kasus | Jabatan | Makan | Keahlian | Total |
| --- | ---: | ---: | ---: | ---: |
| Sebulan penuh (30 dari 30 hari) | Rp 750.000 | Rp 500.000 | Rp 300.000 | Rp 1.550.000 |
| Masuk tanggal 16 (15 dari 30 hari) | Rp 375.000 | Rp 500.000 | Rp 150.000 | Rp 1.025.000 |
| Gaji pokok Rp 9.000.000, sebulan penuh | Rp 750.000 | Rp 500.000 | Rp 400.000 (dari Rp 450.000) | Rp 1.650.000 |

**Jalur tidak normal:**

- **Pegawai pindah struktur di tengah bulan:** setiap segmen menghitung barisnya sendiri. Bila dua
  struktur sama-sama memuat Tunjangan Jabatan, slip menampilkan dua baris Tunjangan Jabatan
  (masing-masing sesuai harinya). Pada bulan tanpa pergantian struktur, tetap satu baris per jenis.
- **Run dihitung ulang setelah struktur diisi:** baris "Tunjangan Tetap" lama otomatis ditandai
  terhapus oleh mekanisme pembersihan baris mesin yang sudah ada, sehingga tidak ada penjumlahan
  ganda.
- **Jenis tunjangan dihapus dari master:** barisnya tidak dihitung. Jenis yang hanya dinonaktifkan
  tetap dihitung selama baris strukturnya aktif.
- **Peringatan peralihan** tidak menahan pengajuan run. Kesiapan run hanya bergantung pada blocker
  dan pegawai bergalat (`PayrollRunService.cs` baris 237).

**Hasil akhir.** Slip dan detail run menampilkan tunjangan per jenis. Total tunjangan tetap
(`TotalRecurringEarning`) adalah jumlah baris yang sudah dibulatkan, sehingga rumus "total = jumlah
komponen" tetap berlaku.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PayrollCalculationService.cs` (seluruh alur hitung, `UpsertComponent`, pembersihan baris mesin),
`PayrollRunService.cs` (pemakaian peringatan dan kesiapan run), `MstSalaryStructureAllowance.cs`,
`MstAllowanceType.cs`, `WfpSalaryAssignment` (`SalaryStructureId` wajib), `PayrollStatusCatalog.cs`,
`PayrollPolicyOptions.cs`, `02-backend-architecture.md` bagian 5.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/PayrollManagement/Services/PayrollCalculationService.cs` | (1) muat baris tunjangan aktif untuk seluruh struktur penetapan di run, sekali per run; (2) loop segmen penetapan mencatat struktur dan jumlah hari; (3) `BuildStructureAllowanceLines` baru; (4) cabang peralihan dengan `CALC-03`/`CALC-04`; (5) baris `PAY_FIXED_ALLOWANCE` hanya ditulis pada cadangan profil; (6) snapshot kebijakan run mencatat sumber tunjangan dan saklar peralihan. 140 baris tambahan, 6 baris diubah |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Bentuk respons tidak berubah. Isi baris komponen run bertambah jenis `PAY_STRUCTURE_ALLOWANCE` dengan `SourceType = SalaryStructureAllowance` |
| Database | Tidak ada perubahan skema. Run yang dihitung menulis baris komponen dengan kode baru |
| Keamanan/Auth | `NOT APPLICABLE` |

**Perubahan perilaku yang terlihat sebelum struktur diisi:** setiap pegawai dengan
`TotalAllowance` lebih dari 0 kini mendapat peringatan `CALC-03` saat run dihitung. Nominalnya sama
persis dengan sebelumnya.

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — tidak ada endpoint baru. Hasil terlihat pada endpoint yang sudah ada
(`POST /payroll-runs/{id}/calculate`, detail run, detail pegawai dalam run, slip).

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan atas instruksi pemilik | `NOT RUN` | — |
| Simulasi tiga contoh kontrak dengan langkah dan pembulatan yang sama (per segmen, batas, prorata, bulat per baris setengah ke atas) | Rp 1.550.000; Rp 1.025.000 (375.000 + 500.000 + 150.000); Rp 1.650.000 (keahlian 400.000) — sama dengan kontrak | `PASS` | Skrip simulasi `Decimal` |
| Keseimbangan kurung | Seimbang | `PASS` | Skrip penghitung |
| Rujukan `fixedAllowance`/`TotalAllowance` tersisa | Hanya di cabang peralihan, total, dan snapshot kebijakan | `PASS` | `grep` |
| Peringatan tidak menahan run | Kesiapan memakai blocker dan pegawai bergalat saja | `PASS` | `PayrollRunService.cs` baris 237 dan 639–659 |
| QBE pada berkas ini | Service, bukan controller atau entity; tidak ada `Trx*`, alokasi nomor, atau akses data di controller | `PASS` | Review diff |
| Uji proses di DB uji | Menunggu pemilik | `NOT RUN` | Langkah di bawah |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `REQUIRED` — dijalankan pemilik setelah build.

**Langkah uji untuk pemilik** (DB uji, data samaran):

1. Sebelum mengisi struktur, hitung run periode uji. Harapan: total gaji tiap pegawai sama dengan
   sebelum task ini; pegawai ber-`TotalAllowance` > 0 mendapat peringatan `CALC-03`.
2. Isi struktur salah satu pegawai sesuai contoh bagian 2 lewat `PAY-BE-16`.
3. Hitung ulang run. Harapan: tiga baris tunjangan per jenis dengan angka tabel bagian 2; baris
   "Tunjangan Tetap" lama hilang; peringatan `CALC-04` bila profil masih berisi angka.
4. Ubah satu pegawai uji agar mulai bekerja tanggal 16. Harapan: Rp 1.025.000.
5. Ubah gaji pokok penetapan menjadi Rp 9.000.000. Harapan: keahlian Rp 400.000.

**Tidak dijalankan:** `dotnet build`, uji proses, checker QBE resmi.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Contoh tabel bagian 5.1 menghasilkan angka persis | Terpenuhi (simulasi); runtime menunggu | Bagian 5 |
| 2. Struktur kosong dan saklar menyala: baris "Tunjangan Tetap" dari profil dan `CALC-03` | Terpenuhi (statis) | Cabang `useProfileAllowanceFallback` |
| 3. Struktur berisi dan profil berisi: profil diabaikan, `CALC-04`, tanpa penjumlahan ganda | Terpenuhi (statis) | Cabang struktur; `PAY_FIXED_ALLOWANCE` hanya ditulis pada cadangan; pembersihan baris mesin lama |
| 4. Pajak dan prorata mengikuti jenis tunjangan | Terpenuhi (statis) | `Taxable` dari `IsTaxable`; prorata dari `IsProrated` dan kebijakan |
| DoD: build hijau | **Belum terpenuhi** | `NOT RUN` atas instruksi pemilik |
| DoD: QBE bersih | Terpenuhi untuk aturan yang dicek; checker resmi `NOT RUN` | Bagian 5 |
| DoD: kriteria terbukti di DB uji | **Belum terpenuhi** | Langkah uji menunggu pemilik |
| DoD: laporan ditulis | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Selama struktur belum diisi, peringatan `CALC-03` muncul untuk setiap pegawai ber-`TotalAllowance` > 0. Jumlahnya bisa banyak; ini disengaja (PRL-DEC-04) |
| Masalah yang diketahui | (1) Pegawai yang pindah struktur di tengah bulan dapat menerima dua baris jenis tunjangan yang sama, masing-masing sesuai harinya. (2) Kolom `PayrollComponentId` pada baris komponen tidak diisi, sama seperti baris mesin lain yang sudah ada |
| Risiko tersisa | Gaji pegawai berubah begitu struktur diisi. Pindahkan angka lama ke struktur dan rekonsiliasi sebelum run produksi (DEC-PRL-015) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Task ini: `M` `PayrollCalculationService.cs`; ditambah laporan dan penandaan roadmap. Perubahan `PAY-BE-14`–`16` yang belum di-commit masih ada. Tidak ada yang di-stage atau di-commit |
| Langkah berikutnya | Pemilik: build dan langkah uji bagian 5. Task berikutnya: `PAY-BE-18` (transport bulanan), lalu `PAY-BE-20` |
