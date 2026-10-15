# Laporan Perubahan Frontend — `FE-CRM-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-CRM-007` |
| Judul | Formulir pasien berhenti mengirim status member |
| Slice | `MVP-2` |
| Roadmap | `docs/module-blueprints/crm-membership/roadmap/frontend-roadmap.md` § `FE-CRM-007` |
| Trace | `FR-CRM-041`; `CRM-CAP-004`; `UAT-11` |
| Contract version | API `v2` (`PatientController` mengabaikan `IsMember`, `DefaultMembershipTierId`, `ActivePatientMembershipId`), `approved` (Yoga Aji Pratama, 2026-10-13) |
| Wewenang UI | Roadmap: isian "dihapus atau dijadikan tampilan saja". Dipilih **dihapus** di formulir; status member tetap tampil baca-saja di detail dan daftar pasien. Tata letak `DEV_DISCRETION` (`CRM-FE-003`) |
| Dependency | `BE-CRM-015` 🟡 (backend selesai, rilis bersama `FE-CRM-001`) |
| Klasifikasi | `MEDIUM`: satu komponen formulir bersama, empat berkas konstanta/util, satu uji lama disesuaikan, uji unit dan E2E baru. Menyentuh layar IGD, Rawat Jalan, dan Rawat Inap karena formulir pasien barunya satu komponen |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`. Backend hanya laporan ini serta roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `f1dfd3edd` (branch `YogaV2`) + perubahan `FE-CRM-001` s.d. `006` yang belum di-commit |
| Commit backend yang dijadikan rujukan | `83ef27bd` (branch `yoga`) + perubahan `BE-CRM-012` s.d. `015` yang belum di-commit |
| Tanggal | 2026-10-14 |
| Status | Selesai. 4 skenario E2E lulus dengan API tiruan, dan `UAT-11` (master pasien dan pasien baru IGD) lulus terhadap backend lokal 2026-10-14 |

## 1. Keadaan yang ditemukan di awal

- **Master pasien** (`patient-constants.jsx`, `patient-editor-utils.jsx`): grup **03 Membership** berisi centang "Member", pilihan "Membership Tier Default", dan isian teks "Membership Aktif" (mengetik ID). Ketiganya dikirim pada `POST`/`PUT` pasien.
- **Pasien baru IGD** (`new-patient-form.jsx`): kartu "Pasien Member" di "Data tambahan pasien" beserta pilihan "Tier Membership" (wajib bila dicentang). `buildPatientPayload` mengirim `isMember`, `defaultMembershipTierId`, dan `activePatientMembershipId`.
- Komponen dan payload yang sama dipakai **Pendaftaran Rawat Jalan** (`use-outpatient-registration.js`) dan **Admisi Rawat Inap** (`inpatient-admission-registration-step.jsx`, hanya kartu member).
- Backend `BE-CRM-015` sudah mengabaikan ketiga isian dan menyimpan pasien baru sebagai bukan member. Dengan begitu centang di formulir **menyesatkan**: petugas mencentang, tetapi tidak ada efeknya.
- **Bug lama ditemukan** di `new-patient-form.jsx`: merge `3f8cb2fac` (2026-10-12) menghapus baris `useFormContext()` yang memuat `setValue`/`clearErrors`, dan peta `SPECIAL_FLAG_DEPENDENT_FIELDS` (dari `1f889d67c`) ikut hilang. Akibatnya, melepas centang kartu "Bayi Baru Lahir" atau "Meninggal" memicu `ReferenceError` di peramban.

## 2. Proses bisnis dari sisi pengguna

**Master Pasien (tambah/ubah):**
1. Petugas membuka formulir tambah atau ubah pasien.
2. Grup isian kini tiga: **01 Identitas Pasien**, **02 Kontak dan Alamat**, **03 Status Khusus**. Grup Membership tidak ada lagi.
3. Simpan tidak mengirim status member. Contoh `UAT-11`: pasien Budi sudah member Gold. Petugas admisi membuka formulir ubah Budi: tidak ada centang Member yang bisa diubah. Detail pasien tetap menampilkan "Member: Ya" dari data membership.

**Pasien baru IGD / Rawat Jalan:**
1. Petugas memilih Pasien Baru → Input Manual, lalu mengisi identitas, kontak, dan alamat.
2. "Data tambahan pasien (opsional)" kini hanya memuat **Pasien Bayi Baru Lahir** dan **Pasien Meninggal**.
3. Mencentang Bayi menampilkan isian ibu dan data kelahiran. Melepas centangnya menyembunyikan dan mengosongkan isian itu tanpa galat.
4. **Simpan Pasien & Lanjut** menyimpan pasien tanpa status member, lalu lanjut ke langkah 2 Emergency Visit.

**Admisi Rawat Inap (pasien baru):** bagian "Data tambahan pasien" tidak lagi memuat kartu apa pun (sebelumnya hanya kartu member). Yang tersisa catatan pasien dan petunjuk "Bayi baru lahir dipilih pada Langkah 1".

**Jadi member hanya lewat CRM:** pendaftaran membership di menu CRM → iuran lunas → backend mengisi status member pasien.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Roadmap § `FE-CRM-007`; `04-prd-to-mvp.md` (`FR-CRM-041`, `UAT-11`); `01-existing-capability-map.md` (`CRM-CAP-004`); laporan `BE-CRM-015`; `PatientDtos.cs` (isian request masih ada tetapi diabaikan). Frontend: `patient-constants.jsx`, `patient-editor-utils.jsx`, `use-master-data-patient-editor.jsx`, `patient-display-utils.jsx`, `new-patient-form.jsx` (dan riwayat Git-nya), `emergency-registration.constants.js`, `emergency-registration.utils.js`, `emergency-registration.service.js`, `use-emergency-registration.js`, `inpatient-admission-flow-constants.jsx`, `inpatient-admission-registration-step.jsx`, `inpatient-admission-patient-utils.jsx`, helper kiosk (konstanta sendiri, tidak terdampak), uji `inpatient-admission-registration-issue-003.test.mjs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/.../emergency-registration/new-patient-form.jsx` | Kartu "Pasien Member" dan pilihan "Tier Membership" dihapus. **Penangan lepas-centang dipulihkan**: `setValue`/`clearErrors` diambil lagi dari `useFormContext`, dan `SPECIAL_FLAG_DEPENDENT_FIELDS` didefinisikan lagi (bayi → ibu, meninggal → tanggal) |
| `src/lib/constants/.../emergency-registration.constants.js` | Tiga isian member dihapus dari nilai bawaan. `defaultMembershipTierId` dikeluarkan dari isian kondisional. Resource dan filter pilihan tier dihapus. `NEW_PATIENT_SPECIAL_FLAG.MEMBER` dihapus; bawaan = bayi, meninggal |
| `src/utils/.../emergency-registration.utils.js` | `buildPatientPayload` tidak lagi mengirim `isMember`, `defaultMembershipTierId`, `activePatientMembershipId`. Dipakai IGD, Rawat Jalan, dan Rawat Inap |
| `src/lib/constants/.../inpatient-admission-flow-constants.jsx` | `INPATIENT_NEW_PATIENT_SPECIAL_FLAGS` menjadi kosong. Baris "Tier Membership" dihapus dari urutan isian. Impor yang tidak terpakai dibuang |
| `src/lib/constants/.../patient-constants.jsx` | Tiga isian member dihapus dari nilai bawaan dan isian formulir. Grup Membership dihapus, Status Khusus menjadi nomor 03. Seksi detail "Membership" (baca-saja) tetap |
| `src/utils/.../patient/patient-editor-utils.jsx` | `mapPatientToForm` dan `buildPatientPayload` master pasien tidak lagi membawa tiga isian member |
| `tests/unit/inpatient-admission-registration-issue-003.test.mjs` | Uji `FE-RWI-205` "tier hanya terkirim bila member dicentang" diganti: payload admisi tidak membawa ketiga isian walau nilainya ada |
| `tests/unit/crm-membership-patient-form.test.mjs` | Baru, 5 uji |
| `tests/e2e/crm-membership-patient-form.spec.mjs` | Baru, 4 skenario dengan API tiruan |

Berkas sumber tetap CRLF.

### 3.3 Kepatuhan arsitektur frontend

Tidak ada komponen, hook, atau pola baru. Perubahan berupa penghapusan isian di konstanta dan util yang sudah ada. Rantai `view → hook → util payload` tetap. Daftar pasien (kolom "Anggota", filter tier) dan detail pasien (tab/seksi Membership) tidak diubah, karena hanya membaca proyeksi.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tidak berubah dari perilaku formulir sebelumnya |
| Kosong | `NOT APPLICABLE`: formulir isian |
| Gagal simpan | Tidak berubah: pesan "Pendaftaran belum dapat dilanjutkan" dengan pesan backend |
| Tanpa hak akses | Tidak berubah. Status member memang tidak lagi bisa diubah siapa pun lewat formulir pasien |
| Lepas centang kartu tambahan | Isian turunan hilang dan dikosongkan, tanpa galat (sebelumnya `ReferenceError`) |

## 5. Endpoint yang dikonsumsi

#### Health Services / Patient Management / Master Data / Patient

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/patient-management/master-data/patients` | Simpan pasien baru (master pasien, IGD, Rawat Jalan, Rawat Inap). Body tanpa `isMember`, `defaultMembershipTierId`, `activePatientMembershipId` | `Patient : Create` |
| `PUT` | `/v1/health-services/patient-management/master-data/patients/{id}` | Ubah pasien. Body tanpa tiga isian yang sama | `Patient : Update` |

#### Administrator / Master Data / Membership Tier

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/administrator/master-data/membership-tiers/options` | **Berhenti dipakai** oleh formulir pasien baru (pilihan tier). Masih dipakai filter daftar pasien dan layar CRM | `MembershipTier : Read` |

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Uji unit `crm-membership-patient-form.test.mjs`: payload pasien baru dan master (buat/ubah) tanpa tiga isian walau nilainya ada (`UAT-11`); konstanta formulir tanpa isian/grup Membership, nomor grup 01–03; flag kartu = bayi, meninggal; Rawat Inap tanpa kartu; penangan lepas-centang dipulihkan | 5 lulus | `PASS` | Keluaran `node --test` |
| Uji `FE-RWI-205` yang disesuaikan | Lulus | `PASS` | idem |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 2.853 uji, 2.824 lulus, 29 gagal | `EXISTING / ENVIRONMENT ISSUE` (29) | Daftar 29 nama gagal identik (`diff`) dengan sebelum task ini, termasuk `FE-RWI-204` scan Bekasi |
| `npx eslint` enam berkas sumber | Bersih | `PASS` | Keluaran perintah |
| `npm run lint:errors` | Bersih | `PASS` | Keluaran perintah |
| `npm run build` | "Compiled successfully in 59s" | `PASS` | Log build |
| E2E tambah master pasien: tanpa grup/isian Membership, Member, Membership Tier Default, Membership Aktif | Lulus | `PASS` | `crm-membership-patient-form.spec.mjs` + tangkapan layar |
| E2E `UAT-11`: ubah pasien member (Budi, Gold) → formulir terisi tanpa isian member; detail pasien tetap menampilkan Member | Lulus | `PASS` | idem |
| E2E IGD pasien baru: hanya kartu Bayi dan Meninggal; centang Bayi → isian Pasien Ibu tampil; lepas → hilang; nol galat peramban | Lulus | `PASS` | idem |
| E2E IGD simpan pasien baru: identitas, tanggal lahir (kalender), jenis kelamin, NIK, telepon, alamat, wilayah berjenjang → **Simpan Pasien & Lanjut** → satu `POST /patients` tanpa tiga isian member → layar pindah ke langkah 2 Emergency Visit; nol galat peramban | Lulus | `PASS` | idem |
| Regresi `inpatient-admission-flow.spec.mjs` (API tiruan) | 1 lulus, 3 gagal | `EXISTING / ENVIRONMENT ISSUE` | Spec bertanggal 2026-09-02 mencari `data-testid` `inpatient-admission-step-doctor`/`-registration` yang **tidak ada lagi di source** sejak UI admisi berubah 2026-10-12. Spec juga butuh `SESSION_SIGNING_SECRET` dari lingkungan. Bukan akibat task ini |

Uji manual: `PASS`. Kartu tambahan (centang/lepas), formulir master tambah/ubah, dan alur simpan pasien baru IGD dijalankan lewat Playwright di server standalone, lalu diperiksa pada tangkapan layar.

Server standalone 3710 dinyalakan untuk E2E lalu dimatikan. `test-results/` tidak berubah.

**UAT terhadap backend lokal (2026-10-14).** Rincian lingkungan ada di
[`FE-CRM-005.md` §6.1](FE-CRM-005.md#61-uat-terhadap-backend-lokal-2026-10-14).

| UAT | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `UAT-11` (a) simpan ubah pasien IKBAL YULIYANTO di master pasien | Tanpa isian member di layar; `PUT` tanpa `isMember`/tier/membership aktif; DB `IsMember=false` | `PASS` | `PUT /patients/{id}` 200 + kueri baca-saja |
| `UAT-11` (b) pasien baru lewat IGD (`UAT FECRM PASIEN IGD`, RM 00-00-00-17, wilayah dari master sungguhan) | Tanpa kartu member; tersimpan dan lanjut ke langkah 2 Emergency Visit; nol galat peramban; DB `IsMember=false` | `PASS` | `POST /patients` 200 + kueri baca-saja |

**Tidak dijalankan:**
- Simpan pasien baru dari layar Rawat Jalan dan Rawat Inap secara terpisah. Keduanya memakai `buildPatientPayload` yang sama dan sudah diuji unit; formulir Rawat Inap ikut tercakup uji konstanta.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Centang "Member" dan pilihan tingkat di formulir pasien (master dan pasien baru IGD) dihapus atau dijadikan tampilan saja | Terpenuhi (dihapus; status member tetap tampil baca-saja di detail pasien) | Uji unit + E2E |
| Status member dibaca dari proyeksi | Terpenuhi: detail, daftar, dan tab Membership tetap membaca proyeksi | E2E detail pasien |
| `UAT-11` lulus | Terpenuhi: lulus dengan API tiruan dan terhadap backend lokal 2026-10-14 | E2E + uji unit payload + UAT §6 |
| Pendaftaran pasien baru IGD tetap berhasil | Terpenuhi dengan API tiruan: pasien tersimpan dan alur lanjut ke langkah 2 | E2E |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Perubahan ikut tampil di **Pendaftaran Rawat Jalan** dan **Admisi Rawat Inap**, karena formulir pasien barunya satu komponen. Roadmap mensyaratkan pemberitahuan ke owner IGD; owner Rawat Jalan dan Rawat Inap juga perlu diberi tahu |
| Masalah yang diketahui | (1) **Bug lama** pada `patient-display-utils.jsx`: `UUID_PATTERN` kurang satu kelompok, sehingga UUID asli tidak dikenali. Halaman ubah/detail pasien hanya terbuka bila ID sudah ada di cache `patient-route-map` (dibuka dari tabel). Tautan langsung berisi UUID gagal memuat data. Di luar cakupan, tidak diperbaiki. (2) `inpatient-admission-flow.spec.mjs` usang terhadap UI admisi sekarang (lihat §6) |
| Perbaikan di luar outcome | Penangan lepas-centang `new-patient-form.jsx` dipulihkan (regresi merge `3f8cb2fac`). Dikerjakan karena berada di penangan yang sama dengan kartu member yang dihapus, dan tanpanya melepas centang Bayi/Meninggal membuat galat |
| Dependency backend | `BE-CRM-015` 🟡: backend sudah mengabaikan ketiga isian. Frontend kini juga tidak mengirimnya, jadi urutan rilis keduanya aman |
| Perubahan sampingan | `NONE`. Build menulis `.next/standalone` |
| Interupsi | `NONE` |
| Status Git | ` M` `new-patient-form.jsx`, `emergency-registration.constants.js`, `emergency-registration.utils.js`, `inpatient-admission-flow-constants.jsx`, `patient-constants.jsx`, `patient-editor-utils.jsx`, `inpatient-admission-registration-issue-003.test.mjs`. `??` `crm-membership-patient-form.test.mjs`, `crm-membership-patient-form.spec.mjs`. Belum di-commit |
| Langkah berikutnya | UAT `01`, `03`–`05`, `08`, `09`, `11`, `15` sudah lulus terhadap backend lokal 2026-10-14 (`FE-CRM-005` §6.1). Tersisa: rilis bersama `BE-CRM-015` + `FE-CRM-001`, dan `AT-24` dengan akun petugas CRM non-administrator |
