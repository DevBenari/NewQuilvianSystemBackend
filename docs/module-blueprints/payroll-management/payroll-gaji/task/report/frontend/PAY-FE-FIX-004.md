# Laporan Perubahan Frontend — `PAY-FE-FIX-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-FIX-004` (ad-hoc, bukan kartu roadmap) |
| Judul | Konfigurasi PPh dan Iuran oleh HRD: form Profil Pajak dan Profil Iuran, indikator kelengkapan terpisah, Rincian Penghasilan Payroll Run memuat bagian Pajak dan Iuran dari Backend |
| Roadmap | **NOT APPLICABLE.** Task ad-hoc atas perintah pemilik; roadmap dan traceability tidak disentuh (preseden `PAY-FE-FIX-001..003`) |
| Trace | Perintah pemilik 2026-11-18 (master prompt "Finishing Konfigurasi PPh dan Iuran HRD"). Prompt menulis branch `yasmin`; branch itu **tidak ada** (lokal maupun origin). Pemilik memilih `yasmina` lewat pertanyaan pada sesi yang sama, sehingga checkout lokal `yasmina` dipakai dan tidak berpindah branch |
| Dependency | Backend `PAY-BE-FIX-004` (source selesai, **belum dibuild**) **dan 10 kolom baru pada `PrlTax`/`PrlInsurance` (DDL pada laporan Backend bagian 7, belum diterapkan)**. Urutan rilis: DB → Backend → Frontend. Backend lama tidak mengirim field baru: layar tidak menurunkan status sendiri dan menampilkan "Belum ditetapkan" (bukan Rp 0), tetapi form baru tidak berfungsi tanpa metadata Backend |
| Task mode | `FRONTEND` |
| Model | Claude Sonnet 5.5 |
| Tanggal | 2026-11-18 |
| Status | 🟡 **Source selesai; ESLint, pemeriksa CSS, dan uji unit lulus. `npm run build`, `npm run dev`, dan uji peramban NOT RUN** (dilarang). `VISUAL CHECK: NOT FEASIBLE` — server dev tidak boleh dijalankan, jadi tampilan form dan dialog belum pernah dilihat. Fungsi end-to-end menunggu DDL dan build Backend |

---

## 1. Yang dikerjakan

- **FE-TAX-001 Form "Konfigurasi PPh"** (`workforce-tax-config-editor.jsx`, menggantikan form generik tab Pajak). Urutan isian: jenis PPh → kategori penerima → metode pajak → skema perhitungan → isian yang bergantung pada skema. Semua pilihan datang dari metadata Backend (`filters/metadata`), tidak ada daftar hardcoded di layar. Opsi `isSupported = false` (contoh: `AUTO_REGULATORY`, metode `GrossUp`) **tampil tetapi dinonaktifkan** dengan alasan dari Backend. Skema `APPROVED_MANUAL_AMOUNT`: nominal + dasar penetapan wajib; `EXEMPT_OR_NOT_APPLICABLE`: alasan + dasar aturan wajib, tanpa nominal. **Nominal manual hanya tampil/dapat diubah bila izin `ViewAmount`**; tanpa izin, nilai tersimpan dipertahankan (payload tidak mengirim `approvedManualAmount`) dan layar menyatakan nominal disembunyikan, bukan Rp 0. Status subjek pajak, PTKP (hanya bila `isPtkpRelevant`), NPWP, dan tanggal efektif mengikuti form lama.
- **FE-TAX-002 Ringkasan profil pajak.** Header status (`workforce-config-status-header.jsx`) di atas form: status konfigurasi (Belum dikonfigurasi / Dikonfigurasi / Belum berlaku / Bermasalah) dengan daftar masalah spesifik dari `configurationIssues`, bukan satu pesan umum. Kolom daftar tab Pajak menjadi "Konfigurasi PPh".
- **FE-TAX-003 Form "Konfigurasi Iuran"** (`workforce-insurance-config-editor.jsx`). Status kepesertaan (Aktif / Tidak mengikuti / Tidak berlaku); dasar wajib selain Aktif; pilihan program BPJS Ketenagakerjaan (JHT, JP, JKK, JKM) dari metadata; BPJS Kesehatan dan asuransi swasta mengikuti flag lama. Tarif per pegawai **tidak** dapat diubah di sini (Backend mengabaikannya dan memberi peringatan); tarif/batas berasal dari Pengaturan Penggajian. Kolom daftar menjadi "Konfigurasi Iuran".
- **FE-TAX-004 Indikator kelengkapan terpisah.** Kelengkapan Data Payroll dan Profil Penggajian menampilkan tiga indikator: **PPh**, **Iuran**, **Kesiapan**, masing-masing dengan masalah spesifik. Banner lama "kedua nominal masih nol" (`noticeTaxNotSet`) **dihapus**; peringatan kini turun dari status konfigurasi eksplisit Backend. Nominal nol tidak pernah dibaca sebagai "belum lengkap".
- **FE-TAX-005 Integrasi Payroll Run.** Status hitung dan peringatan pada dialog mengikuti `taxConfigurationStatus`/`insuranceConfigurationStatus` dari Backend. Penahan konfigurasi Backend tampil apa adanya; tombol Hitung Payroll Run tetap memakai satu jalur aksi halaman yang sudah ada.
- **FE-TAX-006 Rincian Penghasilan: Pajak dan Iuran** (`payroll-run-employee-detail-sections.jsx`). Section Pajak: jenis PPh, kategori, metode, skema, perlakuan (Potongan pegawai / Ditanggung pemberi kerja / Tidak ada), sumber nilai, status hasil, nominal potongan pegawai, nominal ditanggung pemberi kerja, masalah. Section Iuran: status kepesertaan, program, versi kebijakan, dasar upah, total pegawai, total pemberi kerja, dan satu baris per program (program, pihak penanggung, dasar, tarif, nominal). Aturan tampilan: `CALCULATED`/`VALID_ZERO`/`EXEMPT`/`NOT_APPLICABLE` menampilkan nominal sah (Rp 0 hanya untuk nol yang sah); `NOT_CONFIGURED` tampil **"Belum dikonfigurasi"**; `ERROR` tampil alasan dari Backend; run tanpa snapshot tampil "Hitung ulang untuk mendapatkan status konfigurasi". **Kontribusi pemberi kerja dan PPh yang ditanggung pemberi kerja bukan potongan dan tidak mengurangi gaji bersih**; baris itu diberi label tersendiri.
- **FE-TAX-007 Komponen dasar.** Seluruhnya memakai base component yang ada (tabel keputusan bagian 3). Tidak ada komponen base baru.
- **FE-TAX-008 Integrasi API.** `payroll-tax-config-service.js` (metadata dan `GET {id}/amount`), `use-payroll-tax-config-metadata.js` (pola permintaan terjaga: kunci selesai, tanpa setState sinkron di efek, galat 403/404 dibedakan), dan `payroll-tax-config-utils.js` (normalisasi, validasi form sisi klien yang **hanya** mencerminkan aturan Backend, pembentuk payload). **Layar tidak menghitung PPh, iuran, bruto, atau potongan**; tanpa data dummy; tanpa nominal hardcoded; tanpa data keuangan di `localStorage`/`sessionStorage`/konsol (dijaga uji).

## 2. File

Baru (FE): `src/components/view/payroll/workforce-config/workforce-tax-config-editor.jsx`, `workforce-insurance-config-editor.jsx`, `workforce-config-status-header.jsx`; `src/components/view/payroll/shared/payroll-configuration-status-lines.jsx`; `src/lib/constants/payroll/payroll-tax-config-constants.js`; `src/lib/hooks/payroll/use-payroll-tax-config-metadata.js`; `src/lib/services/payroll/payroll-tax-config-service.js`; `src/utils/payroll/payroll-tax-config-utils.js`; `src/style/payroll/payroll-workforce-config-editor.module.css`; `tests/unit/payroll-tax-config.test.mjs`.

Diubah (FE): `src/components/features/workforce/workforce-profile-dashboard.jsx` (editor khusus untuk tab `taxes`/`insurances`; tab lain tetap memakai editor generik), `src/components/view/payroll/data-readiness/payroll-data-readiness-table-columns.jsx`, `src/components/view/payroll/compensation/payroll-compensation-table-columns.jsx`, `payroll-compensation-detail-view.jsx`, `src/components/view/payroll/run/payroll-run-employee-detail-sections.jsx`, `payroll-run-employee-detail-modal.jsx`, `src/lib/constants/payroll/payroll-data-readiness-constants.js`, `payroll-run-constants.js`, `src/utils/payroll/payroll-data-readiness-utils.js`, `payroll-compensation-utils.js`, `payroll-run-employee-detail-utils.js`, `payroll-run-utils.js`, `tests/unit/payroll-data-readiness.test.mjs`, `tests/unit/workforce-tax-profile-tab.test.mjs`.

Diubah di Backend untuk task ini: lihat `PAY-BE-FIX-004`. Tidak ada berkas Backend lain disentuh dari sisi Frontend.

## 3. Tabel keputusan base component (`UI GATE`)

`UI GATE: 11 elemen — REUSE 11, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`.

| Kebutuhan UI | Base | Status | Sumber gaya |
| --- | --- | --- | --- |
| Pilihan jenis/kategori/metode/skema/status/kepesertaan | `BaseSelectField` (dengan `menuPortal`) | REUSE | bawaan |
| Isian teks dan nominal (NPWP, nominal manual, nomor, versi) | `BaseTextField` | REUSE | bawaan |
| Dasar/alasan | `BaseTextAreaField` | REUSE | bawaan |
| Status subjek pajak, pilihan program, flag | `BaseCheckboxField` | REUSE | bawaan |
| Tanggal efektif/berakhir | `BaseDateField` | REUSE | bawaan |
| Simpan, Batal | `BaseButton` | REUSE | bawaan |
| Peringatan, masalah konfigurasi, catatan nominal tersembunyi | `InformationAlert` | REUSE | bawaan |
| Badge status konfigurasi/hasil | `StatusBadge` + konfigurasi badge | REUSE | bawaan |
| Tabel baris iuran per program | `DataTable` | REUSE | `administrator-region-table.module.css` |
| Bagian bertajuk, grid label-nilai pada dialog | `PayrollSection`, `PayrollInfoGrid` | REUSE | `payroll-run.module.css` |
| Susunan form dan header status | — (tata letak) | REUSE | CSS Module domain (urutan 3) `payroll-workforce-config-editor.module.css`, seluruh nilai `var(--…)`; tidak ada CSS Module tetangga untuk form konfigurasi dua tahap ini |

Tidak ada keputusan `NEW`/`EXTEND` yang menunggu persetujuan pemilik.

## 4. Bagian 3.4 — pemeriksaan UI

| Pemeriksaan | Hasil |
| --- | --- |
| `check-css-usage.mjs` pada berkas JSX yang diubah | 0 temuan |
| Grep anti-regresi (Tailwind, `!important`, `style={{`, literal warna hex/rgb pada CSS Module dan JSX baru) | 0 temuan (grep manual 2026-11-18; **tidak** dijaga uji otomatis). Larangan `localStorage`/`sessionStorage`/`console.` pada utils dan editor dijaga uji `payroll-tax-config` |
| ESLint pada berkas yang diubah/baru | 0 galat |
| Screenshot | **Tidak ada.** `VISUAL CHECK: NOT FEASIBLE` — `npm run dev` dan server setara dilarang pada task ini; port 3000 adalah build produksi lama tanpa perubahan ini |
| `MANUAL TEST` | **NOT FEASIBLE** — alasan sama |

## 5. Validasi

| Command | Hasil |
| --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/payroll-tax-config.test.mjs tests/unit/payroll-data-readiness.test.mjs tests/unit/workforce-tax-profile-tab.test.mjs tests/unit/payroll-run-employee-detail.test.mjs tests/unit/payroll-run-screens.test.mjs` | **PASS** — 77 dari 77 (26 pada berkas baru `payroll-tax-config`, 17 kelengkapan data, 10 tab profil pajak). Uji berkas baru mencocokkan kosakata, DTO, route, izin, dan penjaga "satu mesin" dengan **source Backend**, bukan dengan service Frontend (empat uji kontrak itu otomatis dilewati bila repository Backend tidak ada di samping Frontend; pada run ini repository Backend ada dan keempatnya dijalankan) |
| Suite unit penuh (`tests/unit/*.test.mjs` lewat Git Bash; `npm run test:unit` gagal pada glob di Windows) | 5.107 uji, 5.055 lulus, **52 gagal** — jumlah dan sifat sama dengan baseline sebelum task (modul lain: Billing, RWI, IGD, HMD, HRD, menu); **0 kegagalan baru pada Payroll**. Percobaan pertama 54 gagal: 2 uji kontrak lama mengunci bentuk form generik dan teks banner "keduanya nol"; diperbarui (kontrak kini satu arah + pemeriksaan payload editor khusus) lalu dijalankan ulang |
| `npm run build` / `npm run dev` / uji peramban | **NOT RUN** |

TC-01..TC-25 diperiksa **statis** (kode dan uji unit Frontend–Backend). **Tidak ada yang dibuktikan saat dijalankan**, dan tidak ada yang boleh disebut PASS pada runtime. Yang BLOCKED oleh Backend: tampilan `AUTO_REGULATORY` dan `GrossUp` tetap terkunci (nonaktif dengan alasan) karena kebijakan tarif PPh berversi belum ada (laporan Backend bagian 4).

## 6. Risiko, delta, dan dependency

- **Tampilan belum pernah dilihat.** Susunan dua tahap form, header status, tampilan dinonaktifkan beserta alasan pada select, section Pajak/Iuran pada dialog, dan responsif perlu dilihat pemilik sebelum dianggap final.
- **Bergantung pada DDL dan build Backend.** Tanpa 10 kolom baru dan metadata baru, form Konfigurasi PPh/Iuran tidak dapat dimuat (galat dengan tombol Coba Lagi, bukan form kosong dengan pilihan palsu).
- **Dampak operasional**: kelengkapan data kini menandai seluruh pegawai tanpa konfigurasi HRD sebagai belum lengkap, dan Backend menahan pengajuan run bagi mereka. Jumlah baris "Belum dikonfigurasi" yang besar pada hari pertama adalah perilaku yang disengaja.
- **HRD hanya dapat memilih nominal manual atau pengecualian** untuk PPh sampai kebijakan tarif berversi disahkan (BLOCKED, keputusan pemilik pada laporan Backend bagian 6).
- **Izin `ViewAmount` pada `WorkforceTaxProfile`** harus diberikan pada peran HRD yang menetapkan nominal manual lewat Akses Role setelah sinkronisasi atribut Backend; tanpanya form hanya mengizinkan skema tanpa nominal dan nominal tersimpan tidak terlihat.
- **Tab Pajak layar pegawai HR** (`PAY-FE`/HR 17 Nov 2026, karyawan saja; dokter/eksternal belum) tidak diubah cakupannya.
- **Slip self-service lama** (label "PPh 21 TER PMK 168/2023") berada di Backend dan tidak diubah (laporan Backend bagian 6, butir 8).
- **Penjaga uji**: dua uji lama (`workforce-tax-profile-tab`, `payroll-data-readiness`) kini mengunci kontrak baru; bila form generik atau `IsTaxInsuranceUnset` dikembalikan, uji akan gagal.
- `git status`: tidak ada yang di-stage atau di-commit pada kedua repository. Perubahan tugas sebelumnya masih berada di working tree yang sama; branch aktif `yasmina`.
