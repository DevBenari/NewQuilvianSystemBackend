# Laporan Perubahan Frontend — `FE-HRD-R41`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R41` |
| Judul | Migrasi layar administrasi dan ESS lama |
| Slice | `S-A1` s.d. `S-A7`, `S-C4`, `FE-020`, Gelombang 3 |
| Roadmap | `roadmap/frontend-unfinished-work.md` — kartu `FE-HRD-R41`; task asal `FE-HRD-002`–`008`, `017`, `020` |
| Trace | API Contract v6; Lampiran B baris S-A*, S-C4, FE-020 |
| Contract version | API Contract v6 (tidak ada perubahan pemanggilan API) |
| Wewenang UI | Migrasi terarah ke base component; fungsi layar tidak berubah |
| Dependency | `FE-HRD-R14` — 🟡 |
| Klasifikasi | `MEDIUM` — 18 berkas disentuh dari 38 berkas kartu, 41 pelanggaran |
| Task mode | `FRONTEND` — source backend hanya dibaca |
| Target tulis | `QuilvianSystemFrontendDev/src/**`; laporan ini dan tautan roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `59099bcb8` → `af4da693e` (branch `HamzahV2`) |
| Commit backend yang dijadikan rujukan | `7445a8b8` → `974f7208` (branch `MHamzah`) |
| Tanggal | 16 November 2026 |
| Status | 🟡 Sebagian — elemen mentah dan prop salah pada layar sisa sudah dimigrasi; tiga elemen unik dipertahankan dengan alasan dan uji browser belum dapat dijalankan |

---

## 1. Keadaan yang ditemukan di awal

Audit ulang 16 November 2026 terhadap 38 berkas kartu:

- Layar lifecycle (`S-C4-ONB`, 12 berkas) dan approvals V2 (`S-A7-V2`, 13 berkas) sudah tanpa elemen mentah — telah dimigrasi pada Gelombang 2 (`FE-HRD-R24`/`R25`) dan `FE-HRD-R33`.
- Sisa pelanggaran ada di layar ESS lama, kotak masuk workflow lama, slip gaji ESS, exit clearance, benefit, editor profil pegawai, dan profil layanan mandiri.
- Dua salah prop berdampak nyata ditemukan:
  - Empat formulir ESS (cuti, koreksi presensi, lembur, tukar shift) dan kotak masuk workflow memakai `confirmText`/`cancelText`, yang tidak dikenal `ConfirmModal`; tombol kirim tampil dengan label bawaan **"Hapus"**.
  - Tiga modal di rincian exit clearance (checklist departemen, pengesahan, pembatalan) memakai `isOpen`/`onClose`/`confirmVariant`, yang tidak dikenal; ketiga modal **tidak pernah terbuka**.

## 2. Proses bisnis dari sisi pengguna

Alur setiap layar tidak berubah. Yang berubah bagi pengguna:

1. **Formulir ESS** (cuti, koreksi presensi, lembur, tukar shift): tombol kirim kini berlabel sesuai fungsinya ("Kirim Pengajuan", "Kirim Koreksi Presensi", dan seterusnya); pesan petunjuk tampil di atas isian.
2. **Kotak masuk workflow lama**: catatan revisi/penolakan memakai isian standar; alasan penolakan ditandai wajib.
3. **Exit clearance**: tombol Perbarui Checklist, Sahkan, dan Batalkan kini benar-benar membuka dialognya; pembatalan nonaktif sampai alasan diisi.
4. **Slip gaji ESS**: papan PIN memakai tombol standar dengan label aksesibilitas.
5. **Benefit**: tab rincian kepesertaan memakai tombol standar; formulir tanggungan memakai isian standar.
6. **Profil layanan mandiri**: tombol muat ulang, simpan/reset/hapus foto, buat sandi acak, dan simpan sandi memakai tombol standar.
7. Jalur tidak normal tetap seperti sebelumnya (pesan galat dan hak akses dari layar masing-masing).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

38 berkas kartu R41 (Lampiran B), `confirm-modal.jsx`, `base-form-control.jsx`, `base-button.jsx`, konstanta badge exit clearance dan piutang.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `self-services/human-resource/leave/leave-employee-view.jsx`, `attendance-correction/…`, `overtime/…`, `schedule/…` | Input/textarea → `BaseDateField`/`BaseTimeField`/`BaseTextAreaField`; `confirmLabel`/`cancelLabel`; pesan ke isi modal |
| `hr/workflow/approval-inbox-view.jsx` | Textarea → `BaseTextAreaField`; variant `outline-*` → `warning`/`danger`; label modal; badge "Pemohon" → `StatusBadge` |
| `features/hr/payroll/PayslipPinKeypadModal.jsx`, `hr/payroll/payslips-ess-view.jsx` | 12 tombol papan PIN → `BaseButton`; variant "outline" → "secondary" |
| `hr/exit-clearance/exit-clearance-detail-view.jsx` | Modal `show`/`onHide`/`variant`/`confirmProps`; checkbox dan 3 textarea → field base; 4 variant; badge → `status` |
| `hr/exit-clearance/exit-clearance-table-columns.jsx`, `exit-clearance-finance-status-card.jsx` | Variant dan `StatusBadge` |
| `lib/constants/hr/exit-clearance/exit-clearance-constants.js` | Kunci `status` sah pada peta badge |
| `hr/benefit/employee-benefit-enrollment-detail-view.jsx` | Tab → `BaseButton`; select → `BaseSelectField`; 5 isian → field base; 4 variant |
| `hr/benefit/dependent-table-columns.jsx`, `employee-benefit-receivable-report-view.jsx`, `employee-benefit-receivable-detail-view.jsx`, `employee-benefit-receivable-table-columns.jsx` | Variant; `StatusBadge variant` → `status` (peta lokal) |
| `features/workforce/workforce-profile-editor-view.jsx` | 4 tombol → `BaseButton`; modifier kelas tak terdefinisi dilepas |
| `features/self-service-profile/self-service-profile-page.jsx` | 7 tombol → `BaseButton`; kelas avatar tak terdefinisi dilepas |

### 3.3 Kepatuhan arsitektur frontend

Tidak ada perubahan alur data atau service. Peta badge piutang (konstanta Finance bersama) tidak diubah; pemetaan dilakukan lokal di layar benefit.

### 3.4 Gerbang tampilan

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Isian formulir | `BaseDateField`/`BaseTimeField`/`BaseInputField`/`BaseSelectField`/`BaseTextAreaField`/`BaseCheckboxField` | `REUSE` | — |
| Tombol | `BaseButton` | `REUSE` | — |
| Badge | `StatusBadge` (`status`) | `REUSE` | — |
| Input sandi + tombol tampil | Komposit khusus | `KEEP` (unik sah) | Base field belum mendukung tampil/sembunyi sandi |
| Kartu tab profil (ikon, judul, deskripsi) | Komposit khusus | `KEEP` (unik sah) | Tidak tercakup `DataFilter` tabs |
| Zona unggah foto | Input file tersembunyi | `KEEP` (unik sah) | DEC-FER-02 (base unggah) belum diputuskan |

`UI GATE: PASS — 3 elemen REUSE, 0 COMPOSE, 0 NEW, 3 KEEP (unik sah)`

```text
$ node .../check-css-usage.mjs <18 berkas R41>
OK — 1 berkas diperiksa, 0 temuan. (×18)
$ grep Tailwind <berkas R41>
(kosong)
```

`VISUAL CHECK: NOT FEASIBLE` — backend dan frontend tidak berjalan; menyalakan backend pada basis data dev bersama di luar wewenang mode `FRONTEND`.

## 4. State yang ditangani di layar

Tidak berubah dari sebelumnya. Memuat, kosong, gagal, dan tanpa hak akses tetap ditangani oleh masing-masing layar.

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — R41 tidak mengubah pemanggilan API; hanya markup, prop, dan gaya.

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` 18 berkas + konstanta exit clearance | 0 error; 10 warning lama (`react-hooks/set-state-in-effect`, `@next/next/no-img-element`) pada pola yang sudah ada sebelum R41 | `PASS` | Keluaran perintah |
| `check-css-usage.mjs` 18 berkas | 0 temuan | `PASS` | 3.4 |
| Hitung elemen mentah (`button`/`input`/`select`/`textarea`) | Hanya 3 elemen unik yang disengaja | `PASS` | grep |
| `npm run build` BUILD-2 | 832/832 halaman, `BUILD_EXIT=0` | `PASS` | Log build |
| Uji browser | Tidak dijalankan | `NOT RUN` | 3.4 |

Uji manual: `NOT FEASIBLE`.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-L` | Terpenuhi | 3.2, 3.4 |
| Fungsi layar tidak berubah | Terpenuhi | Tidak ada perubahan service; modal exit clearance dan label ESS justru kembali berfungsi sesuai maksud |
| Screenshot | Belum terpenuhi | `VISUAL CHECK: NOT FEASIBLE` |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 10 warning ESLint lama tidak diperbaiki karena di luar cakupan "fungsi tidak berubah" |
| Masalah yang diketahui | Tiga elemen unik di profil layanan mandiri menunggu DEC-FER-02 dan dukungan sandi pada base field |
| Dependency backend | `NONE` |
| Perubahan sampingan | Kunci `status` ditambahkan pada konstanta badge exit clearance |
| Interupsi | Konteks sesi dipadatkan sekali |
| Status Git | 81 entri belum di-commit di repo frontend untuk Gelombang 3 |
| Langkah berikutnya | Keputusan DEC-FER-02; uji browser modal exit clearance |
