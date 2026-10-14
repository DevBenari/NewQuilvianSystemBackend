# Laporan Perubahan Frontend — `RJ-REG-REV-FE-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-REG-REV-FE-003` |
| Judul | Kewarganegaraan pada pasien baru RJ |
| Requirement | `RJ-REG-REV-003` (REV-09) |
| Roadmap | [registration-revision-roadmap.md](../../../roadmap/registration-revision-roadmap.md) bagian `4` |
| Trace | `RJ-REG-DEC-006` |
| Dependency | `RJ-REG-REV-BE-002` ✅ |
| Task mode | `CROSS-REPO MODE` — frontend (`RJ-REG-DEC-001`) |
| Commit frontend | `70b3735d` (`sukmagpV2`), belum di-commit |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 14 Oktober 2026 |
| Status | ✅ Selesai teknis (`TESTED`); UAT pengguna belum |

## 1. Proses dari sisi pengguna

Form pasien baru RJ (bagian *Dokumen dan Kontak*) punya **Kewarganegaraan** (bawaan WNI). Memilih WNA
memunculkan **Negara Asal** (dari master negara, tanpa ID ter-hardcode) yang wajib diisi. Payload create pasien
mengirim `citizenship` dan `nationalityCountryId` (null untuk WNI). Kewarganegaraan tampil kembali di Detail dan
Edit Daftar Pasien RJ (`FE-002`). Pendaftaran IGD tidak menampilkan field ini (prop opt-in `citizenshipOptions`).

## 2. Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/…/emergency-registration/new-patient-form.jsx` | Prop opt-in `citizenshipOptions`; field Kewarganegaraan + Negara Asal |
| `src/components/view/…/emergency-registration/patient-selection-step.jsx` | Meneruskan prop |
| `src/components/view/…/outpatient-registration/outpatient-registration-page.jsx` | Mengirim opsi WNI/WNA |
| `src/lib/constants/…/outpatient-registration.constants.js` | `PATIENT_CITIZENSHIP`, opsi, nilai bawaan |
| `src/utils/…/outpatient-registration.utils.js` | `buildOutpatientCitizenshipPayload` |
| `src/lib/hooks/…/use-outpatient-registration.js` | Validasi + payload kewarganegaraan |

## 3. Keputusan base component

`UI GATE: 2 elemen — REUSE 2, NEW 0` — `EmergencySelectField` existing. Delta: Negara Asal **tidak** memakai
mode `searchable` (opsi tidak terbuka pada mode itu di uji layar); sama dengan field Negara alamat.

## 4. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `npm run lint:errors` (repo) | exit 0 | `PASS` |
| ESLint berkas tersentuh | 0 error; 5 warning lama di `patient-selection-step.jsx` (effect lama, ada di `HEAD`) | `PASS` / `EXISTING` |
| `npm run build` | exit 0, 493 halaman | `PASS` |
| `node --test tests/unit/*.test.mjs` | 2823 pass / 30 fail; 30 kegagalan identik dengan salinan `HEAD` (35 fail, 5 karena salinan tak lengkap) — modul lain (Rawat Inap, Bank Darah, Hemodialisa, menu RWI) | `EXISTING / ENVIRONMENT ISSUE` |
| Grep anti-regresi CSS baru (warna literal, typography, `!important`) | kosong | `PASS` |
| N4 bawaan WNI | lulus | `PASS` |
| N5 WNA memunculkan Negara Asal | lulus | `PASS` |
| N6 WNA tanpa negara ditolak di layar | lulus | `PASS` |
| N7 pasien tersimpan `citizenship=2` + negara, terbaca ulang via `GET` (`WNA`) | lulus | `PASS` |

Uji layar Playwright: FE dev `localhost:3000` (source terkini) dengan API `7184` dialihkan ke backend uji `https://localhost:7185` (build Release `RJ-REG-REV-BE-*`), DB `QuilvianNewDevSukma`, akun SuperAdmin. Build standalone 3100 tidak dipakai karena CORS backend hanya mengizinkan origin 3000.

## 5. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. WNI bawaan | Terpenuhi | N4 |
| 2. WNA tanpa negara ditolak | Terpenuhi | N6 |
| 3. Tersimpan dan terbaca ulang | Terpenuhi | N7, L2 |
| 4. IGD tidak berubah | Terpenuhi lewat kode (prop default `null`) | kode |

## Catatan penutup

| Hal | Isi |
| --- | --- |
| AUTOMATED TEST | `SKIPPED (opsional)` — tidak ada test baru; suite unit dijalankan sebagai regresi |
| Risiko tersisa | Master negara DB uji hanya berisi Indonesia — negara asing harus diisi di Master Wilayah |
| Perubahan sampingan | `NONE` |
| Status Git | Berkas di atas `M`/`??` pada `V2QuilvianSystemFrontendDev`, belum di-commit |
| Langkah berikutnya | Isi master negara; UAT |
