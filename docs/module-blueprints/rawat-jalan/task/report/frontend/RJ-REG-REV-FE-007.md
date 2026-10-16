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
