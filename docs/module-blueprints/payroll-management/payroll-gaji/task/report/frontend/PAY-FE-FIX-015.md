# Laporan Perubahan Frontend — `PAY-FE-FIX-015`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-FIX-015` (ad-hoc lanjutan seri `PAY-FE-FIX`); pasangan `PAY-BE-FIX-015` |
| Judul | Rincian PPh Payroll Run dan Simulasi: makna "bulan hilang" menurut konteks TER vs masa pajak terakhir |
| Slice | Payroll Management → Payroll Run (detail penerima) dan Simulasi Penggajian |
| Roadmap | Tidak ada baris roadmap untuk seri ad-hoc (`NOT APPLICABLE`) |
| Contract version | Mengonsumsi field aditif `PAY-BE-FIX-015`; field lama tidak berubah |
| Wewenang UI | Menambah baris teks pada rincian PPh yang sudah ada. **Tanpa JSX, CSS, komponen, route, atau slice baru** |
| Dependency | `PAY-BE-FIX-015` (belum dibuild). FE tetap benar terhadap backend lama: field yang tidak dikirim diperlakukan kosong |
| Klasifikasi | `MEDIUM` — skor 5 (berkas diperiksa >20: 2; berkas diubah 4: 1; logika sedang: 1; memakai kontrak existing: 1) |
| Task mode | `FRONTEND` |
| Commit frontend saat dikerjakan | `ab5843be747a32b04c90ac1a63236835234149d6` (branch `yasmina`) |
| Commit backend rujukan | `ab340bc7de4e9a041ca3a8e0289dc910f417ec82` |
| Tanggal | 23 November 2026 |
| Status | 🟡 **Source dan tes selesai; tampilan belum dilihat.** `VISUAL CHECK: NOT FEASIBLE` (server dev dan build dilarang) |

---

## 1. Keadaan yang ditemukan di awal

- `buildTaxCoverageRows` menampilkan bulan hilang dengan petunjuk "perhitungan masa pajak terakhir ditahan" **tanpa melihat konteks**. Selama ini aman hanya karena detail TER tidak membawa bulan hilang; begitu backend menambah info kesiapan pada TER (`PAY-BE-FIX-015`), petunjuk itu akan salah muncul pada TER yang sebenarnya berhasil.
- Layar tidak menyebut **masa pajak** (biasa atau terakhir) maupun **jenis perhitungan** dalam bahasa petugas, walau `taxYear`, `taxMonth`, `isLastTaxPeriod` sudah dinormalisasi di `payroll-run-utils.js`. DTO simulasi bahkan tidak memuatnya.
- Tidak ada keterangan bahwa TER berhasil tanpa riwayat.

## 2. Proses bisnis dari sisi pengguna

1. Petugas membuka detail penerima pada Payroll Run (atau hasil Simulasi), bagian PPh.
2. **TER bulanan, riwayat tidak lengkap:** tampil Masa Pajak ("November 2026 (masa pajak biasa)"), Jenis Perhitungan "TER Bulanan", Kebutuhan Riwayat "Tidak diperlukan pada masa pajak biasa", dan keterangan: *"PPh periode ini berhasil dihitung menggunakan TER bulanan. Riwayat bulan sebelumnya tidak diperlukan untuk perhitungan periode ini."* Bulan yang belum tercatat tampil sebagai **informasi** ("Informasi kesiapan akhir tahun. Tidak memengaruhi dan tidak menahan PPh periode ini.") plus *"Riwayat tahun berjalan perlu dilengkapi sebelum rekonsiliasi masa pajak terakhir."*
3. **Masa pajak terakhir, riwayat tidak lengkap:** Jenis "Rekonsiliasi Masa Pajak Terakhir", Kebutuhan Riwayat "Diperlukan untuk rekonsiliasi — belum lengkap", daftar bulan yang belum terbukti, dan petunjuk jalur pemulihan, termasuk kalimat bahwa **Hitung Ulang tidak melengkapi riwayat yang tidak ada**. Nominal tetap tampil sebagai "tidak ada hasil", bukan Rp0.
4. **Simulasi:** sama, ditambah "Keterbatasan Simulasi" (tidak disimpan, bukan hasil pajak final, tidak dapat dipakai untuk pengajuan atau pengesahan). Untuk masa pajak terakhir yang tertahan tampil "Catatan Masa Pajak Terakhir": *bukan PPh nol dan bukan estimasi*.

Keterangan "berhasil dihitung" hanya muncul bila hasil sah (`CALCULATED` atau `VALID_ZERO`) dan nominal final. Status `ERROR`, `NOT_CONFIGURED`, `EXEMPT`, `NOT_APPLICABLE` tidak memunculkannya.

## 3. Perubahan yang dikerjakan

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/payroll/payroll-tax-detail-utils.js` | `buildTaxCoverageRows` sadar-konteks (informasi bila `ytdRequired` false; masalah rekonsiliasi bila true). Fungsi baru `buildTaxPeriodRows` dan `resolveTaxCalculationKind`. Teks baru di `TAX_DETAIL_TEXT` |
| `src/utils/payroll/payroll-run-employee-detail-utils.js` | Rincian PPh pegawai menyisipkan baris masa pajak, jenis perhitungan, kebutuhan riwayat, dan keterangan riwayat (hanya bila nominal final) |
| `src/utils/payroll/payroll-simulation-utils.js` | Menormalkan `taxYear`, `taxMonth`, `isLastTaxPeriod`; baris yang sama pada simulasi; catatan keterbatasan |
| `tests/unit/payroll-simulation.test.mjs` | Penjaga kontrak DTO↔normalizer diperbarui untuk tiga field baru (dua numerik, satu boolean) |
| `tests/unit/payroll-tax-history-context.test.mjs` | **Baru**, 12 tes |

Frontend **tidak** menentukan nominal, kategori, atau tarif pajak; seluruh nilai datang dari backend dan layar hanya memilih teks.

### 3.4 Gerbang tampilan

`UI GATE: N/A — tidak ada JSX atau CSS yang berubah; teks baru mengalir lewat penyaji existing (`AmountList`, `PayrollInfoGrid`)`. `check-css-usage.mjs`: `NOT APPLICABLE` (tidak ada berkas JSX berubah). Catatan: item simulasi hanya membawa `label` dan `value`, sehingga `hint` tidak tampil di grid simulasi; kalimat penjelasnya dimuat pada `value` baris keterangan.

`VISUAL CHECK: NOT FEASIBLE` — server dev dan build dilarang. Perlu dilihat: baris baru pada modal rincian pegawai (panjang teks keterangan), dan grid rincian PPh pada modal simulasi.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| TER sah, riwayat tidak lengkap | Hasil PPh, keterangan berhasil, informasi kesiapan (bukan galat) |
| TER sah, riwayat lengkap | Hasil PPh, keterangan berhasil, tanpa informasi kesiapan |
| Masa pajak terakhir, riwayat tidak lengkap | "Tidak ada hasil", daftar bulan, jalur pemulihan, peringatan Hitung Ulang |
| Nominal manual disetujui / dikecualikan | Jenis Perhitungan sesuai; tanpa keterangan TER |
| Penerima belum dihitung | Tanpa baris baru (nominal belum final) |
| Backend lama (tanpa field baru) | Baris masa pajak tidak tampil; sisanya sama seperti sebelumnya |

## 6. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `payroll-tax-history-context.test.mjs` | **12 lulus, 0 gagal** (9 menjalankan fungsi FE nyata, 3 mencocokkan source backend) | `PASS` |
| `payroll-simulation.test.mjs` | 19 lulus (sebelumnya 1 gagal akibat DTO baru; test diperbarui, bukan dilonggarkan) | `PASS` |
| `payroll-run-employee-detail`, `payroll-tax-config`, `payroll-tax-pph21-ytd-engine`, `payroll-tax-policy-auto-effective` | 16, 30, 20, 8 lulus, 0 gagal | `PASS` |
| `payroll-run-screens.test.mjs` | 10 lulus, **1 gagal** (#9, menguji `menu-items.jsx`) | `EXISTING / ENVIRONMENT ISSUE` — berkas yang diuji tidak saya ubah (`git diff` kosong); pembuktian terhadap `HEAD` bersih tidak dilakukan, jadi klasifikasi ini berdasarkan bahwa berkas itu tidak tersentuh |
| `eslint` pada 5 berkas | 0 error, 0 warning | `PASS` |
| `npm run build`, `npm run test:unit` seluruh suite | Tidak dijalankan | `NOT RUN` |
| Layar dibuka di peramban | Tidak dilakukan | `NOT RUN` (`VISUAL CHECK: NOT FEASIBLE`) |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test <7 berkas> — PASS pada 6 berkas; 1 kegagalan lama pada payroll-run-screens`.

## 7. Acceptance criteria

| Kriteria | Status |
| --- | --- |
| TER tanpa riwayat tidak diberi pesan penahan atau label galat | Terpenuhi pada source (tes 1) |
| Pesan informatif TER sesuai teks yang diminta | Terpenuhi (tes 5) |
| Masa pajak terakhir menampilkan bulan hilang, jalur pemulihan, dan peringatan Hitung Ulang | Terpenuhi pada source (tes 3, 7) |
| Rp0 tidak tampil sebagai hasil pada penahan | Terpenuhi (tes 9) |
| Jenis perhitungan dan kebutuhan riwayat tampil | Terpenuhi pada source (tes 5, 7, 8) |
| Simulasi: masa pajak, masa pajak terakhir, keterbatasan | Terpenuhi pada source (tes 9) |
| **Navigasi ke Konfigurasi PPh pegawai** | **Belum**. Layarnya ada di modul HR (`/hr/workforce-core/payroll-data`); tautan dengan token route privat dan izin belum dapat dipastikan tanpa menjalankan layar |
| **Estimasi masa pajak terakhir pada simulasi** | **Tidak dibuat**: butuh dasar asumsi tertulis dari pemilik pajak dan dukungan backend; layar menyatakan bahwa ini bukan PPh nol dan bukan estimasi |
| **Status "NOT_APPLICABLE/EXEMPT" tampil dengan penjelasan** | Label sudah ada sebelumnya; tidak diubah |
| Tampilan di layar | **Belum terpenuhi** (`VISUAL CHECK: NOT FEASIBLE`) |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Keterangan "tidak diperlukan" hanya benar bila backend memang mengirim `ytdRequired = false` untuk TER; itu sudah demikian pada source backend |
| Dependency backend | Baris masa pajak pada simulasi baru muncul setelah `PAY-BE-FIX-015` berjalan |
| Perubahan sampingan | `NONE` |
| Status Git | Tidak ada stage, commit, push, pull, merge, atau rebase |
| Langkah berikutnya | Lihat modal rincian pegawai dan modal simulasi di peramban; putuskan tautan Konfigurasi PPh |

```text
$ git status --short   (frontend, branch yasmina, HEAD ab5843be7)
 M src/utils/payroll/payroll-run-employee-detail-utils.js
 M src/utils/payroll/payroll-simulation-utils.js
 M src/utils/payroll/payroll-tax-detail-utils.js
 M tests/unit/payroll-simulation.test.mjs
?? tests/unit/payroll-tax-history-context.test.mjs
```
