# Laporan Perubahan Frontend — `FE-HRD-R46`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R46` |
| Judul | Verifikasi layar pengembangan dan administrasi |
| Slice | `S-C2`, `S-C3`, `S-D3`, `S-C4-ONB`, `S-B2`, `S-B4`, `S-D2`, `S-B6`, layar lama; Gelombang 4 |
| Roadmap | `roadmap/frontend-unfinished-work.md` — kartu `FE-HRD-R46`; task asal `FE-HRD-230`, `245`, `171`, `074`, `115`, `087`, `057`, `157` |
| Trace | Kontrak masing-masing slice; laporan `FE-HRD-R16`, `R17`, `R24`, `R25`, `R37`–`R41`; HRD-DEC-127, VR-PAY-22, VR-PAY-24 (Slip Gaji) |
| Contract version | Kontrak slice + source controller backend branch `MHamzah` (dibaca saja); kontrak Slip Gaji diambil dari `PayrollRunModernController` (BE-HRD-159) |
| Wewenang UI | Uji browser P1/P2; perbaikan regresi yang ditemukan (`build-module-frontend`) |
| Dependency | `FE-HRD-R16`, `R17`, `R24`, `R25`, `R37`, `R38`, `R39`, `R40`, `R41` (semua 🟡) |
| Klasifikasi | `STANDARD` — verifikasi 66 rute; 8 berkas source diubah |
| Task mode | `FRONTEND` — source backend hanya dibaca |
| Target tulis | `QuilvianSystemFrontendDev/src/**`; laporan ini, laporan uji live, tautan roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `f095e2e0a` → `f7a1f0917` (perubahan R46 ikut ter-commit pengguna dalam `f7a1f0917`) |
| Commit backend yang dijadikan rujukan | `604cac6a` → `fcdd9f4d` (branch `MHamzah`) |
| Tanggal | 18 November 2026 |
| Status | 🟡 Sebagian — empat regresi frontend diperbaiki dan terverifikasi di browser: crash Portofolio SKP, pesan Axios di 4 layar ESS, Slip Gaji berdata contoh, dan PIN demo. Penilaian Kinerja dan Keberatan masih HTTP 500 karena backend (`my-appraisal`). Slip Gaji belum bisa diuji dengan data nyata. |

---

## 1. Keadaan yang ditemukan di awal

| Layar | Masalah | Asal |
| --- | --- | --- |
| Portofolio SKP (`/corporate/hrd/learning/skp-tracker`) | Crash `q is not defined`: baris liar `q;` di `page.jsx` | Commit `f38b84d7f` |
| ESS Lembur, Koreksi Presensi, Cuti, Jadwal (layar lama) | Menampilkan "Request failed with status code 404" | `err?.message` milik Axios |
| Slip Gaji ESS (`/ess/payslips`, `/hr/self-service/payslips`) | Daftar dan rincian dari konstanta `INITIAL_PAYSLIPS` (nama "Ns. Siti Rahmawati"); tidak ada panggilan API; gaya inline dengan warna heksadesimal | Lolos dari migrasi `R41` (laporan R41 dikoreksi) |
| Modal PIN Slip Gaji | Saat API gagal, PIN `123456`/`251126` diterima sebagai benar; rincian dan unduhan tidak mengirim token PIN | Mock lama `FE-HRD-141` |
| Penilaian Kinerja, Keberatan | HTTP 500 dari `GET appraisals/my-appraisal` | Backend (LBT-W4-PA-01) |

## 2. Proses bisnis dari sisi pengguna (Slip Gaji)

1. Pegawai membuka **Layanan Mandiri → Slip Gaji Saya**. Daftar slip terbit dibaca dari server, dengan nominal disamarkan "Rp ••••••".
2. Pegawai menekan **Lihat Rincian** atau **Buka dengan PIN**. Papan PIN muncul dengan keterangan bahwa PIN bawaan adalah tanggal lahir berformat DDMMYY.
3. Setelah 6 digit, pegawai menekan **Buka Slip Gaji**. Server memverifikasi PIN dan mengembalikan token sesi 15 menit yang disimpan di memori halaman saja.
4. Bila PIN salah, pesan server tampil ("Sisa kesempatan: 2 kali"). Setelah 3 kali salah, server menjawab `429`, dan papan terkunci dengan hitung mundur 30 menit.
5. Rincian penerimaan, potongan, pajak, dan gaji bersih tampil. Tombol **Unduh PDF** mengunduh berkas dengan token yang sama.
6. Bila sesi PIN habis (`401`), layar kembali terkunci dengan pesan server, dan pegawai tetap login.
7. **Kunci Kembali** menghapus token dari memori.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

66 rute R46, `menu-items.jsx`, `payroll-service.js`, `use-hr-remote-data.js`, `InstanceAxios.jsx` (penanganan `401`), `PayrollRunModernController.cs` (self-service), `PayrollPayslipDistributionService.cs`, DTO `PayrollReconciliationAndPayslipDtos.cs`, `PerformanceAppraisalsController.cs`, `PerformanceAppraisalService.cs`, `OvertimeSelfServiceContextService.cs`, dan kebijakan CORS `Program.cs` (`AllowAnyHeader`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/corporate/hrd/learning/skp-tracker/page.jsx` | Baris liar `q;` dihapus |
| `src/components/view/self-services/human-resource/overtime/overtime-employee-view.jsx` | Pesan galat memakai `getHrApiErrorMessage` |
| `src/components/view/self-services/human-resource/attendance-correction/attendance-correction-employee-view.jsx` | Sama |
| `src/components/view/self-services/human-resource/leave/leave-employee-view.jsx` | Sama |
| `src/components/view/self-services/human-resource/schedule/schedule-employee-view.jsx` | Sama |
| `src/lib/services/hr/payroll-service.js` | `getMyPayslips(params, signal)`; `getPayslipDetail(id, pinToken)` dan `downloadPayslipPdf(id, pinToken)` mengirim header `X-Payroll-PIN` |
| `src/components/features/hr/payroll/PayslipPinKeypadModal.jsx` | Ditulis ulang di atas `ConfirmModal`; mock PIN dihapus; pesan, sisa percobaan, dan kunci `429` mengikuti server; PIN dikirim lewat tombol, bukan otomatis |
| `src/components/view/hr/payroll/payslips-ess-view.jsx` | Ditulis ulang: data API, nominal tersamar, panel rincian, unduh PDF (blob), penanganan sesi PIN habis, tanpa data contoh dan tanpa warna literal |

### 3.3 Kepatuhan arsitektur frontend

- Data dimuat lewat `useHrRemoteData` + `selectUnwrappedList`; notifikasi lewat `useHrToasts`. Tidak ada Redux slice atau pola HTTP baru.
- Token PIN disimpan di state halaman, bukan di `localStorage`/cookie.
- Unduhan memakai `saveBlobAsFile` yang sudah ada; pesan galat dari Blob dibaca agar pesan server tetap tampil.
- **Delta kontrak dicatat:** detail dan unduhan wajib membawa header `X-Payroll-PIN` (tidak tertulis di service lama). PDF backend tidak berkata sandi (LBT-W4-PA-08), sehingga petunjuk "Password PDF DDMMYYYY" dihapus dari layar.

### 3.4 Gerbang tampilan

Modul referensi visual: `restitution-settings-view.jsx` (Gelombang 3, pola Hero + SummaryGrid + DataFilter + DataTable + ConfirmModal).

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Judul layar dan aksi kunci | `Hero` | `REUSE` | — |
| Ringkasan (jumlah slip, periode, gaji bersih) | `SummaryGrid` | `REUSE` | — |
| Judul tabel | `DataFilter` (`title`) | `REUSE` | Sama dengan layar restitusi |
| Tabel riwayat slip | `DataTable` | `REUSE` | — |
| Status slip | `StatusBadge` (`info`/`active`) | `REUSE` | — |
| Tombol | `BaseButton` | `REUSE` | — |
| Pesan galat/info | `InformationAlert` | `REUSE` | — |
| Notifikasi unduh | `ToastStack` | `REUSE` | — |
| Dialog PIN + papan angka | `ConfirmModal` + `BaseButton` + kelas `pinContainer`/`pinDisplay`/`pinDot`/`keypadGrid` di `payroll.module.css` | `COMPOSE` | Lihat pilihan A |
| Panel rincian slip | `panel`/`infoGrid` di `workforce-core.module.css` + `breakdownSection`/`breakdownRow`/`totalRow`/`amountCell` di `payroll.module.css` | `COMPOSE` | Lihat pilihan B |

**Pilihan A — dialog PIN**

1. **`ConfirmModal` + papan `BaseButton` + kelas PIN yang sudah ada (rekomendasi, dipakai).** Konsistensi: dialog sama dengan layar HR lain. Risiko regresi rendah karena hanya satu pemakai. Biaya kecil.
2. Pertahankan overlay buatan sendiri (`drawerOverlay` + gaya inline berwarna heksadesimal). Konsistensi buruk dan melanggar aturan token. Tidak ada biaya, tetapi utang visual tetap ada.

**Pilihan B — panel rincian**

1. **Rangkai kelas `panel`/`infoGrid` dan `breakdown*` yang sudah ada (rekomendasi, dipakai).** Seluruh nilai `var(--…)`. Tidak ada CSS baru.
2. Buat CSS Module `payslip.module.css` baru. Menduplikasi kelas yang sudah ada dan menambah biaya pemeliharaan.

`UI GATE: PASS — 8 elemen REUSE, 2 COMPOSE, 0 NEW`

```text
$ npx eslint <9 berkas Gelombang 4>
✖ 4 problems (0 errors, 4 warnings)
# 4 warning react-hooks/set-state-in-effect pada pola loadData lama di 4 view ESS (sudah ada sebelum R46)
$ node .../check-css-usage.mjs <8 berkas JSX Gelombang 4>
OK — 8 berkas diperiksa, 0 temuan.
$ grep -nE "style=\{\{|#[0-9a-fA-F]{3,6}\b|rgba?\(|INITIAL_PAYSLIPS|123456|251126" payslips-ess-view.jsx PayslipPinKeypadModal.jsx
(hanya baris komentar riwayat perubahan)
$ grep Tailwind <berkas>
(kosong — hanya utilitas Bootstrap yang sah, misalnya align-items-center)
```

Screenshot (folder `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw4-1118/`):

| Layar | Berkas |
| --- | --- |
| Slip Gaji, API nyata (kosong) | `R46-slip-gaji/01-api-nyata.png` |
| Slip Gaji, nominal tersamar | `R46-slip-gaji/02-daftar-tersamar.png` |
| PIN salah | `R46-slip-gaji/03-pin-salah.png` |
| Terkunci 429 | `R46-slip-gaji/04-terkunci.png` |
| Rincian terbuka | `R46-slip-gaji/05-rincian-terbuka.png` |
| Sesi PIN berakhir | `R46-slip-gaji/06-sesi-pin-berakhir.png` |
| PIN demo ditolak (500 / koneksi putus) | `R46-slip-gaji/07-pin-demo-*.png` |
| Portofolio SKP setelah perbaikan | `verifikasi-perbaikan/01-skp-tracker.png` |
| ESS Lembur/Koreksi/Cuti/Jadwal setelah perbaikan | `verifikasi-perbaikan/02` s.d. `05` |

`VISUAL CHECK: PASS`

## 4. State yang ditangani di layar (Slip Gaji)

| State | Penanganan |
| --- | --- |
| Memuat | `SummaryGrid`/`DataTable` loading |
| Kosong | "Belum ada slip gaji yang diterbitkan." + penjelasan; tombol **Buka dengan PIN** nonaktif |
| Gagal muat / tanpa hak | `InformationAlert` danger dengan pesan server |
| Terkunci (belum PIN) | Nominal "Rp ••••••" |
| PIN salah / terkunci | Pesan server; papan nonaktif dan hitung mundur saat `429` |
| Sesi PIN habis | `401` → token dihapus, pesan server tampil, nominal disamarkan lagi |
| Gagal unduh | Toast danger berisi pesan server (dibaca dari Blob) |

## 5. Endpoint yang dikonsumsi

`[Tags("HR Payroll - Self-Service Payslips")]`, `PayrollRunModernController` (`[Route("api/v1/corporate/human-resource/payroll/runs")]`):

| Method | Path | Request | Response | Permission |
| --- | --- | --- | --- | --- |
| GET | `self-service/my-payslips` | query `year?` | `ApiResponse<List<SelfServicePayslipSummaryDto>>` | `PayrollRunModern : GetMyPayslips` |
| POST | `self-service/verify-pin` | `VerifyPayslipPinRequest { pin }` | `200 ApiResponse<PayslipPinVerificationResultDto>`; `400`/`429` pesan | `PayrollRunModern : VerifyPayslipPin` |
| GET | `self-service/payslips/{payslipId}/detail` | header `X-Payroll-PIN` | `ApiResponse<SelfServicePayslipDetailDto>`; `401`/`404` | `PayrollRunModern : GetPayslipDetail` |
| GET | `self-service/payslips/{payslipId}/download-pdf` | header `X-Payroll-PIN` | `application/pdf`; `401`/`404` | `PayrollRunModern : DownloadPayslipPdf` |

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Sapu admin 66 rute | 62 ✅; SKP crash, ESS Lembur pesan Axios (keduanya diperbaiki); Penilaian & Keberatan 500 (backend) | `PARTIAL` | `R46-admin/hasil.jsonl` |
| Sapu non-admin 66 rute | 63 ✅; SKP, ESS Lembur, ESS Koreksi (diperbaiki) | `PASS` setelah perbaikan | `R46-nonadmin/hasil.jsonl` |
| Simulasi `500`/`403` (5 layar × 2) | 10/10 ramah | `PASS` | `R46-kegagalan-server/` |
| Validasi formulir tanpa simpan (6) | 6/6 sesuai (batas potongan dikoreksi otomatis ke 10%) | `PASS` | `R46-validasi/` |
| Verifikasi ulang SKP + 4 ESS | Tanpa crash, tanpa pesan Axios; pesan server tampil | `PASS` | `verifikasi-perbaikan/` |
| Slip Gaji — API nyata | `200`, 0 slip; data contoh hilang | `PASS` | `R46-slip-gaji/01` |
| Slip Gaji — alur PIN/rincian/unduh (tiruan) | 8/8 sesuai, header `X-Payroll-PIN` terkirim | `PASS` | `R46-slip-gaji/02`–`06` |
| Slip Gaji — PIN demo saat server gagal | Ditolak (500 dan koneksi putus) | `PASS` | `R46-slip-gaji/07-*` |
| `npx eslint` 9 berkas | 0 error, 4 warning lama | `PASS` | 3.4 |
| `check-css-usage.mjs` 8 JSX | 0 temuan | `PASS` | 3.4 |
| `npm run build` HEAD `f7a1f0917` | **FAIL** di berkas di luar cakupan (`employee-job-descriptions-view.jsx:19` mengimpor `getTodayDate` yang tidak ada). Turbopack tidak melaporkan galat lain, termasuk di berkas R46. | `FAIL (di luar cakupan)` | `scratchpad/build4.log` |
| Data uji tertinggal | Sesi/lockout PIN 24 jam = 0; tidak ada tulis | `PASS` | SELECT baca-saja |

`MANUAL TEST: PASS` untuk perbaikan frontend; Slip Gaji dengan PIN asli `NOT FEASIBLE`: `PrlPayrollPayslip` kosong, verifikasi PIN menulis DB, dan LBT-W4-PA-04.
`AUTOMATED TEST: SKIPPED (opsional) — perilaku diuji langsung di browser; repo tidak memakai Jest.`

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tidak ada regresi fungsi (frontend) | ✅ untuk regresi yang ditemukan | SKP, 4 ESS, Slip Gaji, PIN diperbaiki dan diverifikasi |
| Tidak ada regresi fungsi (keseluruhan) | 🟡 | Penilaian & Keberatan 500 (LBT-W4-PA-01, backend) |
| Uji P1 dan P2 | ✅ | Bagian 6 |
| `npm run build` | 🟡 | Gagal karena berkas di luar cakupan |
| Laporan dan status roadmap | ✅ | Laporan ini dan [laporan uji live](../../testing/test-by-agy/laporan-live-testing-gelombang4-pengembangan-administrasi.md) |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build HEAD gagal karena impor `getTodayDate` di berkas Menu 07; perbaiki sebelum rilis |
| Masalah yang diketahui | LBT-W4-PA-04: endpoint slip memakai ID akun login, bukan ID pegawai, sehingga daftar slip akan kosong dan PIN selalu salah setelah slip diterbitkan. LBT-W4-PA-08: PDF tanpa sandi. LBT-W4-PA-05: menu tanpa `requiredPermission`. LBT-W4-PA-07: rute lembur lama yatim. |
| Dependency backend | LBT-W4-PA-01, LBT-W4-PA-04, LBT-W4-PA-08 |
| Koreksi laporan lain | `FE-HRD-R41` menyebut Slip Gaji sudah bermigrasi, padahal masih berdata contoh; catatan koreksi ditambahkan pada laporan R41 |
| Perubahan sampingan | `NONE` |
| Langkah berikutnya | Uji Slip Gaji ujung ke ujung setelah LBT-W4-PA-04 diperbaiki dan ada slip terbit di DB dev |
