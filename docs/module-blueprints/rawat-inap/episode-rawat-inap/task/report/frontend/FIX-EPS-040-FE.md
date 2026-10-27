# Laporan Perubahan Frontend — `FIX-EPS-040-FE`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FIX-EPS-040-FE` (bagian frontend `PLAN-REPAIR-EPS-040`) |
| Judul | Admisi bayi: gelang dengan baris ibu, validasi waktu lahir, persetujuan wali terisi otomatis dan tervalidasi, DPJP Anak diprioritaskan, picker tanggal tidak terpotong |
| Roadmap | **Tidak ada entri roadmap.** Register status dipegang [PLAN-REPAIR-EPS-040](../../../docs/plan-repair/plan-repair-040-temuan-live-testing-admisi-bayi.md) bagian 1 |
| Trace | [ISSUE-EPS-040](../../../docs/issue/issue-040-temuan-live-testing-admisi-bayi.md); K-01–K-06 = rekomendasi (pemilik, 25-10-2026); `05-skema-tampilan.md` 3.12 |
| Kontrak | Field baru dari [`FIX-EPS-040-BE`](../backend/FIX-EPS-040-BE.md): `wristband.motherLine`, `currentBedName`/`currentPatientClassName`, `newbornAdmission.nonPediatricDoctorReason` |
| Dependency | `FIX-EPS-040-BE` (source selesai; build oleh pemilik) |
| Task mode | `FRONTEND` (backend baca-saja pada task ini) |
| Commit frontend saat dikerjakan | `9cad353ba926ca3750ca03791b2ff4d5b6fddf5a` (`HamzahV2`) |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 2026-10-25 |
| Wewenang UI | PLAN-REPAIR-EPS-040 bagian 3 (skema sebelum → sesudah) yang disetujui pemilik |
| Status | Source selesai untuk 15 perbaikan frontend; lint, unit test, build, dan uji layar belum dijalankan |

---

## 1. Masalah yang diperbaiki

Pada layar Admisi Bayi Baru Lahir:
- jam lahir yang belum terjadi dan tanggal 1990 lolos ke server;
- NIK bayi menerima huruf;
- nama ibu tidak terisi otomatis pada Persetujuan Wali karena `print-data` selalu ditolak 409 selama Draft;
- NIK wali bebas isi;
- 12 klausul tercentang sejak awal;
- DPJP bayi tidak dibedakan;
- picker tanggal terpotong;
- beberapa teks menyesatkan: Kartu Pasien "menunggu konfirmasi kedatangan", ringkasan "Neonatal terpisah", dan kode teknis bed.

---

## 2. Proses bisnis yang berubah di layar

| Langkah | Perubahan | Contoh |
| --- | --- | --- |
| 1 Pilih Ibu | Status kasus berbahasa Indonesia (dari backend); `requestedAt` kasus disimpan ke form identitas | "OPR-OBS-…-068 · Sedang berlangsung" |
| 2 Identitas | Jam lahir hari ini yang melewati jam sekarang (WIB) ditolak; tanggal lahir lebih awal dari kasus −1 hari ditolak; NIK hanya angka; nama hanya huruf dan tanda baca nama | Pukul 11.15: jam 23.50 → "Waktu lahir belum terjadi. Sekarang 25 Okt 2026 pukul 11.15 WIB." |
| 3 Perawatan & DPJP | Dokter Anak di urutan atas dengan badge "Dokter Anak · Disarankan"; memilih dokter non-Anak memunculkan isian alasan wajib | dr. Gigi tanpa alasan → "Alasan memilih dokter non-Anak sebagai DPJP bayi wajib diisi." |
| 4 Penempatan | Kartu rawat gabung menampilkan nama bed ibu; keterangan "Menyiapkan admisi…" selama tombol pesan menunggu | "303-A (Sedang Ditempati Ibu)" |
| 5 Persetujuan Wali | Nama, NIK, HP, dan alamat ibu terisi otomatis untuk "Orang Tua"; NIK/HP/hubungan wajib dan tervalidasi; klausul mulai 0/12 dengan tombol "Saya telah membaca dan menyetujui seluruh klausul"; daftar "Belum lengkap: …" di atas tombol simpan; tidak ada lagi request `print-data` selama Draft | NIK `ABCDE<script>` → tersaring menjadi huruf/angka, lalu ditolak format |
| 6 Tinjau | Perawatan "Ruang NICU"/"Ruang Perinatologi"/"Rawat Gabung dengan Ibu"; tempat "VIP 303 · 303-BOKS" | |
| 7 Dokumen | Gelang dan label kecil bayi menampilkan baris "IBU: …" | |
| 8 Kartu Pasien | Episode aktif → "Episode … aktif. Pasien sudah menempati Ruang NICU · NICU-1-B." | |
| Semua layar ber-picker tanggal | Popover hanya dibuka ke bawah bila tingginya muat atau ruang bawah lebih lapang | Layar 1920×945, pemicu di y≈683 → terbuka ke atas |

---

## 3. Gerbang keputusan base component

`UI GATE: 10 elemen — REUSE 8, EXTEND 1, COMPOSE 1, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| Baris "IBU: …" di gelang | gaya `wristbandMeta` | `wristband-print.jsx` | REUSE | Satu `<span>` |
| Pesan waktu lahir | `BaseDateField`/`BaseTimeField` prop `error` | `base-form-control.jsx` | REUSE | `form.setError` dari `validateNewbornIdentity` |
| Penyaring NIK/nama bayi | `BaseInputField` | `base-form-control.jsx` | REUSE | Transform di handler `onChange` |
| Daftar "Belum lengkap" | `InformationAlert` | `base-features/information-alert` | REUSE | Varian warning |
| Setuju seluruh klausul | tombol toggle `BaseButton` yang sudah ada | `consent-form-step.jsx` | REUSE | Keadaan awal kosong, label baru |
| Tanda wajib & pesan format wali | input persetujuan yang ada | `consent-form-step.jsx` | REUSE | `formRequiredMark`, `formInputHint` |
| DPJP Anak diprioritaskan + alasan | kartu dokter domain + `StatusBadge` + `BaseInputField` | `inpatient-doctor-selection-card.jsx`, `base-features/status-badge` | COMPOSE | Prop opsional `badgeLabel`, `sortDoctorOptions`, `getDoctorBadge`, `onDoctorSelected`, `renderAfterDoctorList`; aktif hanya di mode bayi |
| Arah popover tanggal | `FilterDatePicker` | `base-features/filter-date-picker.jsx` | EXTEND (mengubah default) | Disetujui pemilik lewat FIX-11 PLAN-REPAIR-EPS-040 |
| Keterangan persiapan pemesanan | `InformationAlert` | — | REUSE | |
| Teks Kartu Pasien / perawatan / bed | komponen yang ada | — | REUSE | |

Pilihan non-REUSE yang disajikan:
- **DPJP:** A (dipakai) urutan + badge + alasan, karena K-05 masih membolehkan dokter non-Anak; B saring dokter Anak saja, ditolak karena bertentangan dengan K-05.
- **Picker:** A (dipakai) arah buka berdasarkan tinggi popover; B batas tinggi + gulir dalam, ditolak karena kalender menjadi janggal.

---

## 4. Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/health-services/inpatient-management/inpatient-admission-print-utils.js` | Petakan `motherLine` (FIX-02) |
| `src/components/view/health-services/inpatient-management/admission-workspace/sections/identity-labels/wristband-print.jsx` | Tampilkan baris ibu di gelang dan label kecil (FIX-02) |
| `src/utils/health-services/inpatient-management/inpatient-newborn-admission-utils.js` | Validasi waktu lahir WIB (FIX-06), batas bawah K-03 (FIX-07), nama (FIX-21); `sanitizeNewbornIdentityNumber`, `isPediatricSpecialty`; konteks membawa `nonPediatricDoctorReason` |
| `src/lib/constants/health-services/inpatient-management/inpatient-newborn-admission-constants.js` | Default `nonPediatricDoctorReason` |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-newborn-admission.js` | `deliveryCaseRequestedAt` ke form; `validateCare` mewajibkan alasan DPJP non-anak |
| `src/components/view/health-services/inpatient-management/inpatient-newborn-admission-steps.jsx` | Penyaring NIK; DPJP Anak diprioritaskan + alasan; label perawatan tiga jenis (FIX-15); nama bed di tinjauan (FIX-18); nama bed ibu (FIX-12) |
| `src/components/view/health-services/inpatient-management/inpatient-admission-doctor-step.jsx` | Prop opsional untuk urutan, badge, callback pilih, dan isian di bawah daftar |
| `src/components/view/health-services/inpatient-management/inpatient-doctor-selection-card.jsx` | Prop opsional `badgeLabel` (`StatusBadge`) |
| `src/components/view/health-services/inpatient-management/inpatient-admission-consent-form-step.jsx` | Isi otomatis dari ibu, tanpa `print-data` saat Draft (FIX-20); validasi identitas wali + daftar belum lengkap (FIX-08); klausul mulai kosong (FIX-10) |
| `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx` | Teruskan `mother` ke persetujuan; `activeLocation` ke Kartu Pasien bayi |
| `src/components/view/health-services/inpatient-management/inpatient-admission-print-steps.jsx` | Pesan Kartu Pasien mengikuti status aktif (FIX-14) |
| `src/components/view/health-services/inpatient-management/inpatient-admission-bed-step.jsx` | Keterangan "Menyiapkan admisi…" (FIX-17) |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-patient.jsx` | Lewati lookup booking operasi untuk bayi (FIX-19) |
| `src/components/features/base-features/filter-date-picker.jsx` | Arah popover (FIX-11); konstanta `POPOVER_MIN_BOTTOM_SPACE` dihapus |

**Radius dampak:**
- Komponen persetujuan, langkah dokter, kartu pasien, dan langkah pemesanan dipakai semua tipe admisi. Perilaku baru di sana aktif hanya untuk bayi atau lewat prop opsional, **kecuali** dua hal:
  - validasi identitas wali dan klausul mulai kosong berlaku untuk semua admisi (sesuai skema 3.12 dan K-06);
  - picker tanggal berubah perilaku di seluruh aplikasi.

---

## 5. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint --quiet` pada 14 berkas | Tidak berjalan: pemeriksa keamanan perintah shell gagal sementara berulang kali | `NOT RUN` |
| `npm run lint:errors` / `npm run test:unit` / `npm run build` | Tidak dijalankan (alasan sama) | `NOT RUN` |
| Tinjauan baca-ulang | Import tidak terpakai dihapus (`NEWBORN_CARE_OPTIONS`, `POPOVER_MIN_BOTTOM_SPACE`); dependensi `useCallback` ditambah `missingItems`; `useState` di `NewbornCareStep`; `canSubmit` hanya dipakai tombol simpan; tes unit lama `validateNewbornIdentity` tetap valid (tanggal 2026-01-10, tanpa `deliveryCaseRequestedAt`) | `PASS` (review) |

`AUTOMATED TEST: SKIPPED (opsional) — tidak ada test baru; test lama belum dijalankan karena perintah shell terblokir sementara.`

`MANUAL TEST: NOT FEASIBLE — backend menunggu build pemilik dan belum di-restart; uji layar V-01–V-10 dijalankan setelah itu.`

---

## 6. Acceptance criteria

Semua acceptance criteria frontend sudah terpetakan ke source, tetapi **belum terbukti** lewat lint, test, build, maupun uji layar.

| Perbaikan | AC di source | Terbukti |
| --- | --- | --- |
| FIX-02 (tampilan) | 3/3 tampilan | 0 |
| FIX-06 (layar) | 2/2 | 0 |
| FIX-07 (layar) | 3/3 | 0 |
| FIX-08 (layar) | 4/4 | 0 |
| FIX-09 (layar) | 4/4 | 0 |
| FIX-10 | 3/3 | 0 |
| FIX-11 | 3/3 | 0 |
| FIX-12 (layar) | 1/1 | 0 |
| FIX-14 | 2/2 | 0 |
| FIX-15 | 3/3 | 0 |
| FIX-17 | 2/2 | 0 |
| FIX-18 | 2/2 | 0 |
| FIX-19 | 2/2 | 0 |
| FIX-20 | 3/3 | 0 |
| FIX-21 (layar) | 3/3 | 0 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Penyimpangan dari plan | (1) FIX-10 memakai tombol toggle yang sudah ada dengan label "Saya telah membaca dan menyetujui seluruh klausul", bukan kotak centang baru (REUSE). (2) Kolom NIK wali kini menyaring karakter selain huruf/angka dan dibatasi 16 karakter |
| Risiko tersisa | Picker tanggal lintas modul berubah arah buka; perlu dicek pada 3 layar lain (AC FIX-11 #3). Pengenal dokter Anak berbasis nama spesialisasi |
| Perubahan sampingan | `NONE` |
| Langkah berikutnya | `npm run lint:errors`, `npm run test:unit`, `npm run build`; setelah backend di-build dan di-restart, uji layar V-01–V-10 dengan ibu cadangan |
