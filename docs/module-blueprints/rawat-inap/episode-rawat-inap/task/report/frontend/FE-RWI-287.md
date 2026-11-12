# Laporan Perubahan Frontend — `FE-RWI-287`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-287` |
| Judul | Pemulihan residu formulir, sinkronisasi pencarian, batas jam lahir, saltik istilah bonding, dan reset kunci idempotensi |
| Slice | Admisi Rawat Inap (`FE-INP-03`) — Alur Admisi Bayi Baru Lahir (Langkah 1, 2, 3) |
| Roadmap | [roadmap-repair-044.md](../../roadmap/roadmap-repair-044.md) |
| Trace | [ISSUE-EPS-044](../../docs/issue/issue-044-temuan-pengujian-admisi-bayi-baru-lahir.md), [PLAN-REPAIR-EPS-044](../../docs/plan-repair/plan-repair-044-temuan-pengujian-admisi-bayi-baru-lahir.md) (`FIX-EPS-044-02` (FE), `FIX-EPS-044-03`, `FIX-EPS-044-06`, `FIX-EPS-044-07`, `FIX-EPS-044-08`) |
| Contract version | Selaras alur 8 langkah admisi bayi baru lahir |
| Dependency | `BE-RWI-258` |
| Klasifikasi | `MEDIUM` — 6 berkas frontend |
| Task mode | `FRONTEND` |
| Target tulis | `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-patient.jsx`, `src/lib/hooks/health-services/inpatient-management/use-inpatient-newborn-admission.js`, `src/components/features/base-features/filter-select.jsx`, `src/lib/constants/health-services/inpatient-management/inpatient-newborn-admission-constants.js`, `src/utils/health-services/inpatient-management/inpatient-newborn-admission-utils.js`, `src/components/view/health-services/inpatient-management/inpatient-newborn-admission-steps.jsx` |
| Branch | `HamzahV2` (SHA basis `9c4000724`) |
| Tanggal | 2026-11-02 |
| Status | ✅ SELESAI (Unit test PASS 24/24, ESLint PASS 0 error) |

---

## 1. Ringkasan Perubahan

1. **Reset Kunci Idempotensi Pasca Galat Simpan (`use-inpatient-admission-patient.jsx`):**
   - Mengosongkan `newbornIdempotencyKey.current = ""` pada blok `catch` metode `saveNewbornDraftPatient`.
   - Mengizinkan penguji/pengguna mengirim ulang registrasi bayi setelah terjadi penolakan tanpa terkunci oleh hash idempotensi lama yang berbenturan (HTTP 409).
2. **Pembersihan Residu Formulir Identitas Bayi (`use-inpatient-newborn-admission.js`):**
   - Pada `selectMother`, ketika pengguna beralih ke ibu persalinan lain di Langkah 1, seluruh bidang identitas bayi di Langkah 2 (`fullName`, `birthDate`, `birthTime`, `gender`, `birthPlace`, `identityNumber`, `birthWeightGram`, `birthLengthCm`) direset menjadi kosong serta membersihkan pesan galat validasi.
   - Mencegah bayi tersimpan dengan tanggal, jam, atau tempat lahir dari persalinan ibu sebelumnya.
3. **Pembersihan Riwayat Kata Kunci Server-Side Dropdown (`filter-select.jsx`):**
   - Pada metode penutupan menu `handleClose`, jika mode `serverSide` aktif dan terdapat kata kunci pencarian sebelumnya, pengendali memanggil `onSearchChangeRef.current?.("")` untuk menyelaraskan parameter kueri dengan tampilan input pencarian yang kosong saat dropdown dibuka kembali.
4. **Pembatasan Jam Lahir Masa Depan di Picker (`inpatient-newborn-admission-utils.js` & `inpatient-newborn-admission-steps.jsx`):**
   - Menambahkan utilitas `buildNewbornBirthTimeLimits(birthDate, now = new Date())`. Jika tanggal lahir yang dipilih adalah hari ini, properti `maxTime` diatur ke jam saat ini (`HH:mm`), sehingga picker waktu native menonaktifkan jam yang belum terlewati.
   - Mengintegrasikan properti `maxTime` ke komponen `BaseTimeField` pada Langkah 2 (Identitas Bayi).
5. **Koreksi Saltik Istilah Bonding (`inpatient-newborn-admission-constants.js`):**
   - Memperbaiki salah ketik `"Mendukung bounding ibu & bayi"` menjadi `"Mendukung bonding ibu & bayi"` pada kartu pilihan perawatan rawat gabung.

---

## 2. Berkas yang Diubah

1. `QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-patient.jsx`
2. `QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-newborn-admission.js`
3. `QuilvianSystemFrontendDev/src/components/features/base-features/filter-select.jsx`
4. `QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-newborn-admission-constants.js`
5. `QuilvianSystemFrontendDev/src/utils/health-services/inpatient-management/inpatient-newborn-admission-utils.js`
6. `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-newborn-admission-steps.jsx`
7. `QuilvianSystemFrontendDev/tests/unit/inpatient-newborn-admission.test.mjs`

---

## 3. Bukti Verifikasi

1. **Unit Testing:**
   - Menjalankan `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-newborn-admission.test.mjs`:
     - Seluruh 24 test case (termasuk 7 sub-pengujian baru untuk `FE-RWI-287`) lulus 100% (`pass 24, fail 0`).
2. **Linting:**
   - Menjalankan `npx eslint` pada seluruh berkas terdampak:
     - Hasil: `0 errors, 17 warnings` (tidak ada error linter yang memblokir build).

---

## 4. Kesimpulan & Status

Task `FE-RWI-287` berstatus **✅ SELESAI**.
Seluruh temuan 2 (reset idempotensi), temuan 3 (residu form saat ganti ibu), temuan 6 (sinkronisasi filter select), temuan 7 (batas jam hari ini), dan temuan 8 (saltik bonding) telah selesai diperbaiki dan diverifikasi.

