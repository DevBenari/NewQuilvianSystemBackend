# Laporan Perubahan Frontend — `PAY-FE-FIX-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-FIX-003` (ad-hoc, bukan kartu roadmap) |
| Judul | Popup "Rincian Penghasilan" Payroll Run: dialog lebar, status hitung yang jujur, gross-to-net, pajak, SEMUA cicilan piutang, komponen per kelompok |
| Roadmap | **NOT APPLICABLE.** Task ad-hoc atas perintah pemilik; roadmap dan traceability tidak disentuh (preseden `PAY-FE-FIX-001/002`) |
| Trace | Perintah pemilik 2026-11-17 (master prompt "Finishing Popup Rincian Penghasilan dan Perhitungan Payroll"); branch Frontend `yasmina`. Prompt menulis `yasmin` (404 di GitHub); pemilik sudah mengonfirmasi `yasmina` lewat pertanyaan pada sesi yang sama, jadi checkout lokal `yasmina` dipakai |
| Dependency | Backend `PAY-BE-FIX-003` (source selesai, **belum dibuild**). Tanpa itu Backend lama tidak mengirim field baru: layar menurunkan status dari `employeePayrollStatus` dan memakai `installment` tunggal (kompatibel, tanpa Rp 0 palsu), tetapi tidak menampilkan rincian baru |
| Task mode | `FRONTEND` (Backend read-only pada task ini kecuali `PAY-BE-FIX-003`) |
| Model | Claude Sonnet 5.5 |
| Tanggal | 2026-11-17 |
| Status | 🟡 **Source selesai; lint, pemeriksa CSS, dan uji unit lulus. `npm run build`, `npm run dev`, dan uji peramban NOT RUN** (dilarang). `VISUAL CHECK: NOT FEASIBLE` — server dev tidak dijalankan (port 3100 mati; port 3000 adalah build produksi lama tanpa perubahan ini), jadi tampilan, lebar dialog, dan responsif belum pernah dilihat |

---

## 1. Yang dikerjakan

- **FE-01 Dialog lebar scoped.** `modal-confirm.css` mengunci dialog 430px (`size` tidak berpengaruh). Pola `.modal.modal` (sama dengan `workforce-payroll-data-edit-modal.module.css`) menetapkan lebar `min(100% - 2rem, 1100px)` dan tinggi isi `100dvh − 48px`; seluruh selector diawali `.modal.modal` sehingga tidak ada dialog lain yang berubah (dijaga uji). Badan dialog satu-satunya area gulir; judul lekat di puncak; footer tetap terlihat.
- **FE-02/03/04 Susunan.** Identitas dan status (NIP, jabatan, departemen, periode, status penggajian, status hitung, snapshot, terakhir dihitung) → peringatan keadaan → tiga kartu bruto/total potongan/bersih → dua panel **Rincian Penghasilan** | **Rincian Potongan** (PPh 21, BPJS/Iuran pegawai, kehadiran, cuti tanpa gaji, cicilan piutang, lainnya) → **Absensi, Lembur, Cuti** | **Pajak dan Iuran** (status pajak, status konfigurasi, sumber nilai, kontribusi pemberi kerja sebagai informasi, bank dan rekening tersamar) → **Cicilan Piutang Pegawai** → **Komponen Perhitungan**.
- **FE-05 Keadaan dari Backend.** `calculationState` menentukan apa yang boleh tampil sebagai angka. `NotCalculated`/`Error`: semua nominal gross-to-net menjadi teks **"Belum dihitung"/"Tidak sah"**, bukan Rp 0, dengan pesan "Payroll Run belum dihitung. Nilai final gross-to-net belum tersedia." `DataIncomplete`: nominal tampil, PPh dan iuran tampil **"Belum ditetapkan"** dengan peringatan "Konfigurasi Pajak/Iuran Belum Lengkap". `Calculated` dengan nol sah tetap tampil Rp 0. `SnapshotFrozen`: catatan snapshot final. Alasan penerima, peringatan, dan selisih agregat dari Backend ditampilkan apa adanya. Label badge: Belum dihitung / Data belum lengkap / Hasil hitung berhasil / Hasil hitung bermasalah / Snapshot final.
- **FE-06 Cicilan Piutang Pegawai.** **Semua** angsuran tampil, berurutan menurut `allocationOrder`, satu kartu per angsuran: perjanjian + status, angsuran ke-, periode potong, sesuai perjanjian, jadwal, tunggakan terbawa, direncanakan terpotong, sisa belum terpotong, gaji bersih tersedia, batas penerima, batas tersedia untuk angsuran ini, sumber batas, urutan alokasi (hanya bila lebih dari satu), konfirmasi ke Finance, waktu persetujuan, bukti otorisasi. Kosong: "Tidak ada cicilan piutang yang berlaku pada periode ini." Status perjanjian bukan `DISETUJUI` (dan status diketahui): "Perjanjian belum disetujui HRD." Catatan batas kumulatif, nilai bawaan kebijakan yang belum disahkan, dan "saldo Finance baru berkurang saat run disahkan" ditampilkan. Bukti otorisasi hanya menyatakan "berkas tercatat; isi dan tanda tangan belum diverifikasi sistem" — tidak pernah ditampilkan sebagai terverifikasi.
- **FE-07 Tabel komponen.** Satu `DataTable` per kelompok (Penghasilan, Potongan, Pajak, Iuran; kelompok kosong dibuang), kolom Komponen/Sumber/Metode/Kuantitas/Nominal. Jumlah lebar minimum 610px dan `min-width: 840px` bawaan `DataTable` dilonggarkan di dalam dialog, jadi tidak ada gulir mendatar pada desktop; pada layar sempit gulir hanya di dalam kartu tabel.
- **FE-08 Responsif.** Dua panel berdampingan mulai 768px, satu kolom di bawahnya; mobile: dialog selebar layar dikurangi 10px, tombol footer berdampingan.
- **FE-09 Hitung Payroll Run.** Tombol hanya muncul bila Backend menyatakan penerima belum dihitung/bermasalah **dan** `actions.canCalculate` (izin + status run + tidak terkunci, aturan yang sama dengan tombol halaman). Memakai aksi hitung halaman yang sudah ada (satu jalur, bukan endpoint kedua). Klik pertama membuka konfirmasi **di dalam dialog** (tanpa dialog bertumpuk): "seluruh penerima payroll run dihitung, bukan hanya penerima ini; tidak mengubah saldo piutang". Tidak ada hitung otomatis saat dialog dibuka. Setelah berhasil, hook memuat ulang rincian penerima dari Backend.
- **FE-10 Pengambilan data.** Kunci `RunId` + `RunEmployeeId` (bukan `WorkforceProfileId`). Loading, galat dengan judul per jenis, **Coba Lagi** (tidak untuk 403/404), 403 "Akses ditolak", 404 dengan pesan milik layar ini (pesan 404 bawaan penyaring galat milik layar HRD diganti), 409 "Muat Ulang" (memuat ulang run dan rincian). Tidak ada data keuangan di `localStorage`/`sessionStorage`/konsol (dijaga uji).
- **FE-11 Pajak dari Backend saja.** Layar tidak menghitung PPh 21, BPJS, bruto, atau potongan; total ditampilkan apa adanya dari Backend. Kontribusi pemberi kerja bukan baris potongan dan nolnya dijelaskan ("Tidak dihitung pada jalur payroll ini"), bukan Rp 0.

## 2. File

Baru: `src/components/view/payroll/run/payroll-run-employee-detail-sections.jsx`, `...-employee-installments.jsx`, `src/utils/payroll/payroll-run-employee-detail-utils.js`, `src/style/payroll/payroll-run-employee-detail-modal.module.css`, `tests/unit/payroll-run-employee-detail.test.mjs`.
Diubah: `src/components/view/payroll/run/payroll-run-employee-detail-modal.jsx` (ditulis ulang, kini perakit tipis), `payroll-run-detail-view.jsx`, `payroll-run-table-columns.jsx` (`buildComponentColumns`), `src/lib/hooks/payroll/use-payroll-run-detail.js` (`reloadEmployeeDetail`, hitung memuat ulang rincian), `src/utils/payroll/payroll-run-utils.js` (`normalizeEmployeeDetail`, `normalizeInstallmentLine`), `src/lib/constants/payroll/payroll-run-constants.js`, `tests/unit/payroll-installment-deduction-guards.test.mjs` (satu asersi penjaga Backend diperbarui, lihat `PAY-BE-FIX-003` risiko 7).

## 3. Tabel keputusan base component (`UI GATE`)

`UI GATE: 10 elemen — REUSE 9, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0`. Elemen COMPOSE (daftar nominal) punya keputusan terbuka yang tidak memblokir (lihat di bawah).

| Kebutuhan UI | Base | Status | Sumber gaya |
| --- | --- | --- | --- |
| Dialog | `ConfirmModal` (`modalClassName`/`bodyClassName`/`footerClassName`) | REUSE | CSS Module domain (urutan 3): `modal-confirm.css` mengunci 430px, tidak ada CSS Module tetangga untuk dialog selebar ini |
| Bagian bertajuk | `PayrollSection` | REUSE | `payroll-run.module.css` |
| Grid label-nilai (identitas, kartu cicilan) | `PayrollInfoGrid` | REUSE | `payroll-run.module.css` |
| Kartu bruto/potongan/bersih | `SummaryCards` (`layout="stacked"`) | REUSE | bawaan |
| Tabel komponen | `DataTable` | REUSE | `administrator-region-table.module.css` + longgar `min-width` di modul baru |
| Status | `StatusBadge` + konfigurasi badge | REUSE | bawaan |
| Peringatan, galat, daftar catatan | `InformationAlert`, `PayrollNoticeList`, `PayrollActionError` | REUSE | bawaan |
| Tombol | `BaseButton` | REUSE | bawaan |
| Spinner | `react-bootstrap` `Spinner` (sudah dipakai modal ini) | REUSE | bawaan |
| **Daftar nominal** (label kiri, nominal rata kanan) | tidak ada padanan di `base-features` | **COMPOSE** | `<dl>` lokal di modul Payroll, dibatasi pada dialog ini; **tidak** menambah komponen ke `base-features` |

**Keputusan terbuka (non-blokir).** Daftar nominal dikerjakan sebagai komposisi lokal. Opsi: (1) **tetap lokal** pada dialog ini — risiko konsistensi rendah, biaya nol *(rekomendasi)*; (2) naikkan menjadi base component `AmountList` bila layar Payroll lain membutuhkannya — konsistensi lebih baik, tetapi menyentuh `base-features` dan butuh persetujuan; (3) pakai `DataTable` dua kolom — konsisten tetapi memakai tajuk tabel dan min-width 840px yang tidak cocok untuk daftar pendek. Dikerjakan dengan opsi 1; ganti bila pemilik memilih lain.

## 4. Bagian 3.4 — pemeriksaan UI

| Pemeriksaan | Hasil |
| --- | --- |
| `check-css-usage.mjs` pada 5 berkas JSX | `OK — 5 berkas diperiksa, 0 temuan.` |
| Grep anti-regresi (Tailwind, `!important`, `style={{`, literal warna, `localStorage`/`console`, UUID ditampilkan) | 0 temuan; uji menjaga CSS bebas warna literal dan setiap selector diawali `.modal.modal` |
| ESLint pada berkas yang diubah | 0 galat; 2 peringatan `react-hooks/set-state-in-effect` pada efek `use-payroll-run-detail.js` yang sudah ada sebelum task ini (tidak disentuh) |
| Screenshot | **Tidak ada.** `VISUAL CHECK: NOT FEASIBLE` — `npm run dev` dilarang pada task ini |
| `MANUAL TEST` | **NOT FEASIBLE** — alasan sama |

## 5. Validasi

| Command | Hasil |
| --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/payroll-run-employee-detail.test.mjs tests/unit/payroll-run-screens.test.mjs` | **PASS** — 24 dari 24 (13 baru + 11 lama), termasuk pencocokan route, `[HttpGet]`/`[HttpPost]`, dan seluruh field DTO yang dibaca layar dengan `PayrollRunDtos.cs` Backend |
| `node ... --test tests/unit/payroll-installment-deduction-guards.test.mjs` | **PASS** — 6 dari 6 |
| Suite unit penuh | 5.081 uji, 5.029 lulus, **52 gagal** — jumlah dan sifat sama dengan sebelum task ini (modul lain: Billing, RWI, IGD, HMD, HRD, menu); 0 kegagalan baru pada Payroll. Sempat 53 gagal pada percobaan pertama karena penjaga Backend mengunci teks yang berubah; diperbaiki dan dijalankan ulang |
| `npm run build` / `npm run dev` / uji peramban | **NOT RUN** |

Pemeriksaan statis atas TC: nominal tidak Rp 0 saat belum dihitung/gagal; nol sah tetap Rp 0; pajak belum ditetapkan ≠ Rp 0; kontribusi pemberi kerja tidak ada di daftar potongan; seluruh angsuran terurut menurut alokasi; catatan batas kumulatif dan nilai bawaan; 403/404/409/jaringan; RunEmployeeId; hitung dua langkah dan tidak otomatis; modal scoped; tanpa gulir mendatar pada tabel (jumlah lebar kolom); salinan wajib dari pemilik. **Belum dibuktikan saat dijalankan.**

## 6. Risiko, delta, dan dependency

- **Tampilan belum pernah dilihat.** Lebar dialog, judul lekat, panel dua kolom, kartu cicilan pada layar sempit, dan gulir di dalam kartu tabel perlu dilihat pemilik. Jangan menganggap layout final sebelum itu.
- **Tombol tutup (X) di kepala tidak ada.** `ConfirmModal` tidak punya tombol di kepala; penutupan lewat tombol footer "Tutup" dan tombol `Esc`. Judul (nama penerima) dibuat lekat di puncak; identitas dan status ikut digulir.
- **Dependency Backend** `PAY-BE-FIX-003` belum dibuild. Backend lama: status diturunkan dari `employeePayrollStatus`, angsuran tunggal, tanpa rincian perjanjian (alert "perjanjian belum disetujui" sengaja tidak muncul bila status perjanjian tidak dikirim).
- **Rincian tidak otomatis diperbarui bila run dihitung dari halaman, saat dialog tertutup;** dialog selalu memuat ulang saat dibuka.
- **Pesan galat Backend** ditampilkan apa adanya pada galat selain 403/404; isinya mengikuti Backend.
- `git status`: tidak ada yang di-stage atau di-commit. Perubahan tugas sebelumnya masih berada di working tree yang sama.
