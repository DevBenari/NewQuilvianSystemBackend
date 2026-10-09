# Laporan Perubahan Frontend — `FE-RWI-206`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-206` |
| Judul | Data tambahan pasien disusun menurut makna: admisi rawat inap hanya menawarkan Pasien Member; "Metode Persalinan" pindah ke data kelahiran |
| Slice | Perbaikan defect pasca-pengujian; bukan slice fitur baru |
| Roadmap | `docs/module-blueprints/rawat-inap/episode-rawat-inap/roadmap/frontend-roadmap-v2.md` |
| Trace | `ISSUE-EPS-003` butir `ISS-EPS-003-06`, `ISS-EPS-003-T3`; `PLAN-REPAIR-EPS-003` perbaikan `FIX-EPS-003-06`, `FIX-EPS-003-09`; `RWI-DEC-224`; `ISSUE-EPS-002` |
| Contract version | `0.10.0` — tidak disentuh |
| Dependency | `FE-RWI-205` — daftar pilihan Tier Membership. Keputusan `K-02` diambil sebagai `RWI-DEC-224` |
| Klasifikasi | `MEDIUM` — menyentuh komponen bersama IGD |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — source; laporan dan tautan bukti di `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/episode-rawat-inap/**` |
| Wewenang UI | `RWI-DEC-224`; skema tampilan 3.3 revision `0.6` wilayah "Data tambahan pasien (opsional)" |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `b010ffb9722f236160477c95c931c22467550200` (`HamzahV2`), perubahan belum di-commit |
| Tanggal | 6 Oktober 2026 |
| Status | Selesai. Verifikasi manual di peramban `NOT RUN` |

---

## 1. Masalah yang diperbaiki

Bagian "Data tambahan pasien" memuat tiga kartu centang mandiri — Pasien Member, Pasien Bayi Baru Lahir,
dan Pasien Meninggal — yang dapat dicentang sekaligus. Ketiganya tidak sejenis: member adalah atribut,
bayi baru lahir adalah kategori yang sudah dipilih di Langkah 1 dan belum masuk rilis ini
(`ISSUE-EPS-002`), sedangkan pasien meninggal tidak dirawat inap. Bagian ini juga tidak tercantum pada
skema 3.3 — ia terbawa karena formulir IGD dipakai utuh. Isian "Metode Persalinan", data kelahiran
bayi, tampil untuk setiap pasien di bagian Identitas.

## 2. Proses bisnis

**Admisi rawat inap:**

1. Bagian "Data tambahan pasien (opsional)" tertutup secara bawaan.
2. Isinya hanya kartu **Pasien Member**, Tier Membership yang muncul dan **wajib** bila kartu dicentang,
   serta Catatan Pasien. Di bawahnya: *"Bayi baru lahir dipilih pada Langkah 1 — Tipe Pasien."*
3. Melepas centang member mengosongkan tier yang sempat dipilih.
4. Apa pun isi formulirnya, payload admisi selalu `isNewborn=false`, `isDeceased=false`, tanpa pasien
   ibu maupun tanggal meninggal.

**Pendaftaran IGD (pemakai lain komponen ini):**

1. Tetap tiga kartu sampai pemilik pendaftaran IGD memutuskan lain — rekomendasinya ada pada
   `PLAN-REPAIR-EPS-003` bagian 3.3.4.
2. Aturan server kini tercermin di layar: bayi baru lahir wajib memilih Pasien Ibu; pasien meninggal wajib
   mengisi Tanggal Meninggal; member wajib memilih Tier Membership. Petugas melihat pesannya sebelum
   menyimpan, bukan penolakan server sesudahnya.
3. "Metode Persalinan" tampil bersama berat, panjang, dan jam lahir ketika Bayi Baru Lahir dicentang.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/health-services/registration-management/emergency-registration/new-patient-form.jsx` | Prop `specialRegistrationFlags` (bawaan ketiga kartu) dan `specialRegistrationHint`. Kartu tunggal memakai lebar penuh (`fieldSpan3`). Melepas centang mengosongkan isian turunannya dan galatnya. Tier, Pasien Ibu, dan Tanggal Meninggal ber-`required` — berlaku hanya ketika tampil karena react-hook-form 7.54 melewati isian yang tidak ter-mount. "Metode Persalinan" dipindah ke kelompok bayi baru lahir. Judul bagian menjadi "Data tambahan pasien (opsional)" |
| `src/lib/constants/health-services/registration-management/emergency-management/emergency-registration.constants.js` | `NEW_PATIENT_SPECIAL_FLAG`, `DEFAULT_NEW_PATIENT_SPECIAL_FLAGS`, `CONDITIONAL_NEW_PATIENT_FIELDS` |
| `src/lib/hooks/health-services/registration-management/emergency-registration/use-emergency-registration.js` | Validasi IGD sebelum simpan mencakup `CONDITIONAL_NEW_PATIENT_FIELDS` |
| `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx` | `INPATIENT_NEW_PATIENT_SPECIAL_FLAGS` (hanya member) dan `INPATIENT_NEW_PATIENT_SPECIAL_HINT` |
| `src/utils/health-services/inpatient-management/inpatient-admission-patient-utils.jsx` | `buildInpatientPatientPayload` mengunci `isNewborn`, `motherPatientId`, `isDeceased`, dan `deceasedDate` |
| `src/components/view/health-services/inpatient-management/inpatient-admission-registration-step.jsx` | Mengirim kedua prop ke `NewPatientForm` |

### 3.2 Radius dampak

| Pemakai | Dampak |
| --- | --- |
| Pendaftaran IGD | Tiga kartu tetap; **validasi baru** untuk isian bersyarat; "Metode Persalinan" pindah ke kelompok bayi. Beri tahu pemilik pendaftaran IGD |
| Admisi rawat inap | Hanya kartu member; payload terkunci |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada perubahan kontrak. Payload admisi kini tidak pernah membawa kombinasi yang ditolak server |
| Database | `NOT APPLICABLE` |
| Keamanan/Auth | `NOT APPLICABLE` |

---

## 4. Tabel keputusan base component

| Elemen | Status | Bukti |
| --- | --- | --- |
| Kartu Pasien Member | `REUSE` | `BaseCheckboxCard`; atribut mandiri, semantik kotak centang tepat |
| Kartu Bayi Baru Lahir dan Pasien Meninggal | `REUSE`, ditampilkan menurut prop | Tidak dirender pada admisi |
| Tata letak kartu tunggal | `REUSE` | Kelas `fieldSpan3` yang sudah ada |
| Kalimat bantu | `REUSE` | Kelas `stepActionHint` yang sudah ada |
| Opsi "satu grup pilihan tunggal" untuk IGD | Tidak dikerjakan | Rekomendasi bagi pemilik IGD; bukan wewenang task ini |

`UI GATE: PASS` — tidak ada komponen maupun CSS baru.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| Unit test `inpatient-admission-registration-issue-003.test.mjs` | 9 dari 9 lulus; untuk task ini: payload admisi mengunci bayi baru lahir dan meninggal walau formulir berisi sebaliknya | `PASS` |
| Perilaku react-hook-form terhadap isian tersembunyi | Dibaca pada `node_modules/react-hook-form` 7.54.0: `validateField` mengembalikan kosong bila `!mount` — aturan `required` isian yang tidak tampil tidak berlaku | `PASS` (source) |
| `npx eslint` berkas yang diubah | 0 error. Tiga warning `react-hooks/preserve-manual-memoization` pada `use-emergency-registration.js` **identik** dengan versi `HEAD` | `PASS` / `EXISTING WARNING` |
| `npx eslint src --quiet` | Exit `0` | `PASS` |
| `npm run build` | Exit `0`; 474 halaman; standalone siap | `PASS` |
| Uji peramban admisi dan IGD | Belum dijalankan | `NOT RUN` |

`MANUAL TEST: NOT RUN` — dikecualikan atas keputusan pemilik 1 September 2026 dan 10 September 2026.

`AUTOMATED TEST: node --test tests/unit/inpatient-admission-registration-issue-003.test.mjs — PASS (9/9)`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Admisi: Data tambahan hanya memuat Pasien Member, Tier Membership bila dicentang, dan Catatan | Terpenuhi | `INPATIENT_NEW_PATIENT_SPECIAL_FLAGS = ["member"]` |
| Admisi: mustahil menyimpan `IsNewborn` atau `IsDeceased` bernilai benar | Terpenuhi | `buildInpatientPatientPayload`; unit test |
| Member tanpa tier → "Tier membership wajib dipilih." tanpa request | Terpenuhi pada source | `required` pada isian tier; simpan admisi lewat `handleSubmit` (`FE-RWI-207`) |
| Centang member dilepas → tier tidak terkirim | Terpenuhi | `handleSpecialFlagChange` mengosongkan; `buildPatientPayload` mengirim GUID kosong bila bukan member |
| IGD tetap tiga kartu | Terpenuhi | Bawaan `DEFAULT_NEW_PATIENT_SPECIAL_FLAGS` |
| IGD: bayi tanpa Pasien Ibu → pesan wajib di layar | Terpenuhi pada source | `required` + `CONDITIONAL_NEW_PATIENT_FIELDS` pada `trigger` IGD |
| "Metode Persalinan" tidak lagi di Identitas Pasien; tampil bersama data bayi | Terpenuhi | `new-patient-form.jsx` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Keputusan yang dipakai | `RWI-DEC-224` — pilihan (a) pada `K-02`, diambil dari perintah pemilik untuk mengimplementasikan rencana. Pemilik dapat mengoreksinya |
| Risiko tersisa | Validasi baru di IGD mengubah perilaku layar pemilik lain; perlu diberitahukan. Verifikasi peramban belum dijalankan |
| Perubahan sampingan | `NONE` |
