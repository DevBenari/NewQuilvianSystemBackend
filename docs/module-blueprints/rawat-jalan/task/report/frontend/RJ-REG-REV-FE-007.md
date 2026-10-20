# Laporan Perubahan Frontend — `RJ-REG-REV-FE-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-REG-REV-FE-007` |
| Judul | Kartu asuransi utuh dan Edit Penjamin |
| Requirement | `RJ-REG-REV-009` (REV-04), `RJ-REG-REV-010` (REV-07) |
| Roadmap | [registration-revision-roadmap.md](../../../roadmap/registration-revision-roadmap.md) bagian `4` |
| Trace | `RJ-REG-DEC-004` |
| Dependency | — |
| Task mode | `CROSS-REPO MODE` — frontend (`RJ-REG-DEC-001`) |
| Commit frontend | `70b3735d` (`sukmagpV2`), belum di-commit |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 14 Oktober 2026 |
| Status | ✅ Selesai teknis (`TESTED`); UAT pengguna belum |

## 1. Proses dari sisi pengguna

**Edit Penjamin (REV-07).** Di langkah Pembayaran, penjamin **asuransi** terpilih menampilkan tombol
**Edit Penjamin** (hanya pemegang `PatientInsurance : Update`). Modal memuat data terkini lewat
`GET patient-insurances/admin/{id}`, menampilkan penyedia (baca saja) dan No. polis*, No. kartu, No. peserta,
pemegang polis, hubungan, plan, kelas, tanggal berlaku. Validasi: No. polis wajib; tanggal berakhir ≥ mulai.
Simpan memakai `PUT admin/{id}` dengan **seluruh** ruas lama dikirim ulang (eligibility, limit, co-payment,
kartu, catatan tidak berubah) karena PUT bersifat full replace. Sesudah sukses daftar penjamin dimuat ulang
dan penjamin yang sama dipilih ulang. Penjamin kunjungan yang sudah terdaftar tidak tersentuh (`RJ-REG-DEC-004`).
Eligibility tetap divalidasi backend saat kunjungan didaftarkan.

**Kartu asuransi (REV-04).** Pratinjau kartu sudah `object-fit: contain` (`RJ-DOC-REV-FE-014`); uji layar tidak
menemukan pemotongan di panel. Tidak ada perubahan CSS kartu penjamin — lihat catatan.

## 2. Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/…/outpatient-registration/outpatient-payer-edit-modal.jsx` | Baru |
| `src/components/view/…/emergency-registration/payment-method-step.jsx` | Prop opt-in `onEditPayer` |
| `src/components/view/…/outpatient-registration/outpatient-registration-page.jsx` | Izin, state modal, toast |
| `src/lib/hooks/…/use-outpatient-registration.js` | `handlePayerUpdated` |
| `src/lib/services/…/emergency-registration.service.js` | `getPatientInsuranceDetail`, `updatePatientInsurance` |
| `src/style/…/outpatient-registration.module.css` | `.payerEditGrid`, `.modalFormBody` |

## 3. Keputusan base component

`UI GATE: 4 elemen — REUSE 3, EXTEND 1, NEW 0` — `ConfirmModal`, `BaseTextField`, `BaseDateField` REUSE;
`PaymentMethodStep` EXTEND lewat prop opt-in (IGD tidak berubah).

## 4. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `npm run lint:errors` (repo) | exit 0 | `PASS` |
| ESLint berkas tersentuh | 0 error; 5 warning lama di `patient-selection-step.jsx` (effect lama, ada di `HEAD`) | `PASS` / `EXISTING` |
| `npm run build` | exit 0, 493 halaman | `PASS` |
| `node --test tests/unit/*.test.mjs` | 2823 pass / 30 fail; 30 kegagalan identik dengan salinan `HEAD` (35 fail, 5 karena salinan tak lengkap) — modul lain (Rawat Inap, Bank Darah, Hemodialisa, menu RWI) | `EXISTING / ENVIRONMENT ISSUE` |
| Grep anti-regresi CSS baru (warna literal, typography, `!important`) | kosong | `PASS` |
| U17 Edit Penjamin tampil untuk asuransi terpilih | lulus | `PASS` |
| U18 form berisi nilai saat ini | lulus | `PASS` |
| U19 No. polis kosong ditolak | lulus | `PASS` |
| U20 `PUT 200`, panel menampilkan `CRD-EDIT-UI` | lulus | `PASS` |
| `GET` sesudah edit: `isEligible=true`, tanggal berlaku & `isPrimary` tetap | sesuai | `PASS` |

Uji layar Playwright: FE dev `localhost:3000` (source terkini) dengan API `7184` dialihkan ke backend uji `https://localhost:7185` (build Release `RJ-REG-REV-BE-*`), DB `QuilvianNewDevSukma`, akun SuperAdmin. Build standalone 3100 tidak dipakai karena CORS backend hanya mengizinkan origin 3000.

## 5. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Kartu tidak terpotong | Terpenuhi untuk pratinjau existing (`contain`); kartu hasil scan fisik `NOT FEASIBLE` | tangkapan layar, kode |
| 2. Data hasil scan tetap | Terpenuhi — alur scan tidak diubah | kode |
| 3. Edit Penjamin hanya pada Asuransi | Terpenuhi | U17, kode |
| 4. Data valid tersimpan, panel sinkron | Terpenuhi | U20 |
| 5. Tanpa izin tombol tidak tampil | Terpenuhi lewat kode (`usePermission`) | kode |

## Catatan penutup

| Hal | Isi |
| --- | --- |
| AUTOMATED TEST | `SKIPPED (opsional)` — tidak ada test baru; suite unit dijalankan sebagai regresi |
| Risiko tersisa | Pemotongan kartu yang dilaporkan pengguna tidak tereproduksi dengan gambar uji; perlu konfirmasi dengan kartu hasil scan nyata saat UAT |
| Perubahan sampingan | `NONE` |
| Status Git | Berkas di atas `M`/`??` pada `V2QuilvianSystemFrontendDev`, belum di-commit |
| Langkah berikutnya | UAT dengan scanner kartu asuransi nyata |

## Revisi 2026-10-15 — tombol Edit di header kartu penjamin, form ringkas, modal lebih lebar

Masukan user: tombol *Edit Penjamin* dipindah ke posisi badge *Aktif*. Badge dihapus dan label tombol menjadi *Edit*. Form tidak lagi memuat No. Peserta, Hubungan dengan Pasien, Nama Plan, Kelas, Berlaku Mulai, dan Berlaku Sampai. Modal diperlebar, dan tombol hijau diganti gradient biru yang sudah dipakai di halaman ini.

| Hal | Isi |
| --- | --- |
| Berkas | `emergency-registration/payment-method-step.jsx`, `outpatient-registration/outpatient-payer-edit-modal.jsx`, `outpatient-registration.module.css` |
| Header kartu | Bila `onEditPayer` tersedia dan penjamin bertipe asuransi, tombol `BaseButton` *Edit* (sm, secondary, `aria-label` "Edit penjamin …") menggantikan badge *Aktif*. IGD dan penjamin non-asuransi tetap menampilkan badge *Aktif*, karena komponen ini dipakai bersama IGD |
| Form | Hanya Penyedia Asuransi (baca saja), No. Polis (wajib), No. Kartu, dan Nama Pemegang Polis. Ruas yang disembunyikan tetap dikirim ulang dengan nilai lama pada `PUT` (full replace), sehingga tidak terhapus. Validasi tanggal berlaku dihapus karena ruasnya tidak lagi diisi di sini |
| Lebar modal | `dialogClassName` `.payerEditDialog`: `min(100% - 32px, 760px)`, dengan selektor tiga kelas tanpa `!important`. `size="xl"` tidak dipakai karena CSS bawaan membatasi `.region-modal .modal-dialog` 430 px |
| Tombol | `confirmButtonClassName = region-btn + primaryButton` (gradient `#0b8492 → #13a6b6`, sama dengan *Lanjut ke Pembayaran*) |
| Keputusan base component | `BaseButton`, `ConfirmModal`, `BaseTextField` `REUSE`; tombol gradient `REUSE` kelas `primaryButton`. `UI GATE: PASS` |
| ESLint berkas tersentuh | exit 0 — `PASS` |
| `npm run build` | "Compiled successfully" — `PASS` |
| Grep anti-regresi CSS baru | kosong — `PASS` |

Uji layar Playwright (FE dev 3000 → backend uji 7185, pasien `REGREV LAYAR 2508`). `PUT` dicegat dengan `route.fulfill`, jadi data asuransi tidak berubah.

| ID | Skenario | Hasil |
| --- | --- | --- |
| P1–P3 | Tombol *Edit* tampil di header kartu; label "Edit"; tombol lama *Edit Penjamin* tidak ada | `PASS` |
| P4 | Badge *Aktif* tidak tampil di header kartu asuransi | `PASS` |
| P5 | Form hanya `insuranceProviderName`, `policyNumber`, `cardNumber`, `holderName` | `PASS` |
| P6 | Lebar modal 760 px | `PASS` |
| P7 | Tombol *Simpan Penjamin* `linear-gradient(135deg, rgb(11,132,146), rgb(19,166,182))` | `PASS` |
| P8 | `PUT` mengirim nilai lama `memberNumber` (MBR-2508), `holderRelationship` (SELF), `planName`, `className`, dan masa berlaku 2026-01-01 s.d. 2027-12-31, sama dengan data `GET` | `PASS` |
| P9 | Tidak ada error halaman | `PASS` |

Total 9/9 `PASS`. Status task tetap ✅ `TESTED` / `UAT_PENDING`.

### Revisi lanjutan 2026-10-15 — Nama Pemegang Polis selebar form, teks tombol putih saat hover

| Hal | Isi |
| --- | --- |
| Perubahan | Ruas Nama Pemegang Polis memakai `.payerEditFull` (`grid-column: 1 / -1`) sehingga mengisi satu baris penuh. Tombol *Simpan Penjamin* mendapat `.payerEditSaveButton`, yang menjaga teks `--color-surface` (putih) saat hover dan fokus. Sebelumnya hover `.btn-link` Bootstrap (variant tombol `ConfirmModal`) mengubah teks menjadi biru |
| ESLint | exit 0 — `PASS` |
| Uji layar | P10: input 702 px = grid 702 px; P11: warna teks saat hover `rgb(255, 255, 255)`. Seluruh skenario P1–P11 11/11 `PASS` |

### Revisi lanjutan 2026-10-15 — tombol Scan Ulang bergradient biru

| Hal | Isi |
| --- | --- |
| Perubahan | Modal *Kartu …* (Perbesar) di `payment-method-step.jsx` memakai `confirmButtonClassName = region-btn + primaryButton + confirmPrimaryButton`. Aturan teks putih saat hover dipindah ke kelas bersama `.confirmPrimaryButton` di `emergency-registration.module.css`, dan dipakai juga oleh *Simpan Penjamin*. Kelas `.payerEditSaveButton` dihapus |
| Dampak | Komponen ini dipakai bersama Pendaftaran IGD, jadi tombol *Scan Ulang* di IGD ikut bergradient biru |
| ESLint | exit 0 — `PASS` |
| Uji layar | Pasien `AGNES YULIANI RAJA GUK GUK` (kartu Allianz). P12: *Scan Ulang* `linear-gradient(135deg, rgb(11,132,146), rgb(19,166,182))`, teks saat hover `rgb(255,255,255)`. P1–P12 12/12 `PASS`. `PUT` tetap dicegat; data tidak berubah |
