# Laporan Perubahan Frontend — `FE-RWI-205`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-205` |
| Judul | Formulir pasien baru tanpa isian UUID: Tier Membership dan Pasien Ibu menjadi daftar pilihan; Jam Lahir memakai `FilterTimePicker` |
| Slice | Perbaikan defect pasca-pengujian; bukan slice fitur baru |
| Roadmap | `docs/module-blueprints/rawat-inap/episode-rawat-inap/roadmap/frontend-roadmap-v2.md` |
| Trace | `ISSUE-EPS-003` butir `ISS-EPS-003-04`, `ISS-EPS-003-05`; `PLAN-REPAIR-EPS-003` perbaikan `FIX-EPS-003-04`, `FIX-EPS-003-05`; aturan pemilik "UUID tidak boleh diinput user dan tidak boleh tampil di halaman frontend" |
| Contract version | `0.10.0` — tidak disentuh. Memakai endpoint yang sudah ada: `GET /api/v1/administrator/master-data/membership-tiers/options` dan `GET /api/v1/health-services/patient-management/master-data/patients/options` |
| Dependency | Tidak ada task. Izin baca tier bagi peran admisi belum diverifikasi — keputusan `K-04` |
| Klasifikasi | `MEDIUM` — menyentuh komponen bersama IGD |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — source; laporan dan tautan bukti di `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/episode-rawat-inap/**` |
| Wewenang UI | `PLAN-REPAIR-EPS-003` yang diperintahkan untuk diimplementasikan pemilik 6 Oktober 2026; `RWI-DEC-224` |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `b010ffb9722f236160477c95c931c22467550200` (`HamzahV2`), perubahan belum di-commit |
| Tanggal | 6 Oktober 2026 |
| Status | Selesai. Verifikasi manual di peramban `NOT RUN` |

---

## 1. Masalah yang diperbaiki

Formulir pasien baru meminta petugas mengetik tiga UUID: "Default Membership Tier ID", "Active Patient
Membership ID", dan "Patient ID Ibu". Isian-isian itu diturunkan langsung dari properti `Guid?` pada
`CreatePatientRequest`. "Active Patient Membership ID" bahkan **selalu** ditolak server untuk pasien
baru (`PatientController.cs:2366-2368`). Isian Jam Lahir memakai kontrol jam bawaan peramban
(`type="time"`), padahal base component `FilterTimePicker` tersedia.

## 2. Proses bisnis

1. Petugas mencentang "Pasien Member" → muncul **Tier Membership**: daftar tier yang aktif dan ditandai
   boleh dipilih saat admisi, berlabel *"Gold — MT-RSMMC-002"*. Yang dikirim ke server tetap id tier.
2. Membership aktif tidak lagi ditanyakan. Kartu membership dibuat belakangan lewat master Patient
   Membership setelah pasiennya ada.
3. Pada pendaftaran IGD, "Pasien Bayi Baru Lahir" memunculkan **Pasien Ibu**: pencarian minimal dua
   karakter, berlabel *"Sari Dewi — No. RM 00-12-34"*. NIK tidak ditampilkan.
4. Jam Lahir dipilih lewat pemilih jam 24 jam yang dapat dikosongkan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/health-services/registration-management/emergency-registration/new-patient-form.jsx` | Isian `activePatientMembershipId` dihapus. `defaultMembershipTierId` menjadi `EmergencySelectField` server-side "Tier Membership" bersumber `useSelectResource`. `motherPatientId` menjadi `EmergencySelectField` server-side "Pasien Ibu". `birthTime` memakai `EmergencyTimeField`. Tidak ada lagi placeholder atau label berbahasa teknis yang menyebut UUID/ID |
| `src/components/view/health-services/registration-management/emergency-registration/emergency-registration-fields.jsx` | `EmergencyTimeField` baru — pasangan `EmergencyDateField` yang merangkai `FilterTimePicker` 24 jam. `EmergencySelectField` menerima `searchPlaceholder`, `emptyText`, dan `helperText` |
| `src/lib/constants/health-services/registration-management/emergency-management/emergency-registration.constants.js` | `NEW_PATIENT_MEMBERSHIP_TIER_SELECT_RESOURCE` (rute administrator yang benar, label nama — kode, saringan `isSelectableInAdmission`), `NEW_PATIENT_MEMBERSHIP_TIER_FILTERS`, `NEW_PATIENT_MOTHER_SELECT_RESOURCE` (label nama — No. RM, wajib cari minimal dua karakter, tanpa NIK) |
| `src/utils/health-services/registration-management/emergency-management/emergency-registration.utils.js` | `buildPatientPayload` hanya mengirim `defaultMembershipTierId` bila member dicentang dan `motherPatientId` bila bayi baru lahir dicentang; selainnya GUID kosong, yang dibaca server sebagai `null` (`NormalizeNullableGuid`) |

### 3.2 Radius dampak

| Pemakai | Dampak |
| --- | --- |
| Pendaftaran IGD | Ikut berubah — disengaja, karena aturan UUID berlaku umum dan kedua alur sama-sama mendaftarkan pasien baru. Beri tahu pemilik pendaftaran IGD |
| Registry `select-resource-registry` | **Tidak** disentuh. Resource `membershipTiers` milik health-service menunjuk rute yang tidak ada (`ISS-EPS-003-T4`), sehingga task ini memakai konfigurasi objeknya sendiri |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada kontrak baru. Dua endpoint options yang sudah ada mulai dipanggil dari formulir ini |
| Database | `NOT APPLICABLE` |
| Keamanan/Auth | Endpoint tier mensyaratkan `MembershipTier : Read` (`MembershipTierController.cs:262`). Bila peran admisi belum memilikinya, isian menampilkan *"Daftar tier membership tidak dapat dimuat. Hubungi admin akses."* — bukan daftar kosong diam-diam. Daftar pasien ibu tidak menampilkan NIK |

---

## 4. Tabel keputusan base component

| Elemen | Status | Bukti |
| --- | --- | --- |
| Tier Membership | `REUSE` | `EmergencySelectField` → `FilterSelect` mode `serverSide`, data dari `useSelectResource` — pola yang sama dengan `ResourceFilterSelect` |
| Pasien Ibu | `REUSE` | Idem |
| Jam Lahir | `COMPOSE` | `EmergencyTimeField` merangkai `FilterTimePicker` dengan label dan galat seragam. Opsi lain — memakai `FilterTimePicker` langsung tanpa pembungkus — ditolak pada `PLAN-REPAIR-EPS-003` 2.5 karena label dan galatnya harus ditulis ulang. Pembungkus ini **tidak** mengubah perilaku bawaan `FilterTimePicker` |
| Prop `searchPlaceholder`, `emptyText`, `helperText` | `EXTEND` pembungkus fitur, bukan base component | Diteruskan apa adanya ke prop `FilterSelect` yang sudah ada; nilai bawaannya tidak berubah |

`UI GATE: PASS` — tidak ada `NEW`, dan tidak ada `EXTEND` yang mengubah perilaku bawaan base component.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| Unit test `inpatient-admission-registration-issue-003.test.mjs` | 9 dari 9 lulus; untuk task ini: tier hanya terkirim bila member, `activePatientMembershipId` selalu GUID kosong, `birthTime` "08:45" → "08:45:00" | `PASS` |
| `grep -n "UUID" new-patient-form.jsx` | Nol hasil | `PASS` |
| `npx eslint` berkas yang diubah | 0 error, 0 warning baru | `PASS` |
| `npx eslint src --quiet` | Exit `0` | `PASS` |
| `npm run build` | Exit `0`; 474 halaman; standalone siap | `PASS` |
| Simpan pasien member di peramban dan periksa payload | Belum dijalankan | `NOT RUN` |
| Izin `MembershipTier : Read` pada peran admisi | Belum diverifikasi | `NOT RUN` — `K-04` |

`MANUAL TEST: NOT RUN` — dikecualikan atas keputusan pemilik 1 September 2026 dan 10 September 2026.

`AUTOMATED TEST: node --test tests/unit/inpatient-admission-registration-issue-003.test.mjs — PASS (9/9)`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tidak ada isian yang meminta atau menampilkan UUID | Terpenuhi | Grep "UUID" nol; label isian relasi berupa nama dan kode |
| "Active Patient Membership ID" tidak ada; payload tidak membawa id membership aktif | Terpenuhi | Isian dihapus; unit test — GUID kosong, dibaca server sebagai `null` |
| Tier Membership berlabel nama — kode; simpan menyimpan id tier | Terpenuhi pada source | `NEW_PATIENT_MEMBERSHIP_TIER_SELECT_RESOURCE.getLabel`; unit test payload. Respons 200 di server belum diuji di peramban |
| Hanya tier aktif dan "Tampil di admission" | Terpenuhi | `onlyActive` bawaan `useSelectResource` + `isSelectableInAdmission=true` |
| Daftar tier gagal dimuat (termasuk 403) menampilkan pesan jelas | Terpenuhi | `helperText` saat `membershipTierSelect.error` |
| Pasien Ibu berlabel nama dan No. RM; NIK tidak tampil | Terpenuhi | `NEW_PATIENT_MOTHER_SELECT_RESOURCE` tanpa `descriptionKeys` |
| Jam Lahir memakai pemilih jam 24 jam; tidak ada lagi `type="time"` | Terpenuhi | `EmergencyTimeField`; grep `type="time"` nol |
| Jam 08:45 → `08:45:00`; dikosongkan → `null` | Terpenuhi | Unit test; `toNullableTimeSpan` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Temuan di luar cakupan | `ISS-EPS-003-T4` (registry health-service dan metadata backend menunjuk rute tier yang tidak ada) dan `ISS-EPS-003-T5` (master data pasien masih meminta UUID membership aktif) tetap terbuka untuk pemilik modul Patient Management |
| Risiko tersisa | Izin baca tier bagi peran admisi (`K-04`). Verifikasi peramban belum dijalankan |
| Perubahan sampingan | `NONE` |
