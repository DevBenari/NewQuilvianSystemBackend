# Laporan Perubahan Frontend — `RJ-REG-REV-FE-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-REG-REV-FE-001` |
| Judul | Tanggal lahir pada cari pasien Pendaftaran RJ |
| Requirement | `RJ-REG-REV-001` (REV-01) |
| Roadmap | [registration-revision-roadmap.md](../../../roadmap/registration-revision-roadmap.md) bagian `4` |
| Trace | `RJ-REG-DEC-002` |
| Dependency | `RJ-REG-REV-BE-001` ✅ |
| Task mode | `CROSS-REPO MODE` — frontend (`RJ-REG-DEC-001`) |
| Commit frontend | `70b3735d` (`sukmagpV2`), belum di-commit |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 14 Oktober 2026 |
| Status | ✅ Selesai teknis (`TESTED`); UAT pengguna belum |

## 1. Proses dari sisi pengguna

Langkah 1 *Pasien Lama* kini punya kolom **Tanggal Lahir** di samping kolom cari. Petugas dapat:
- mengisi tanggal lahir saja → bila satu pasien langsung ke verifikasi; bila lebih dari satu muncul **daftar
  kandidat** (nama · No. RM · tanggal lahir · jenis kelamin) untuk dipilih;
- mengisi kata kunci + tanggal lahir → hasil dipersempit di server;
- menekan **Reset** untuk mengosongkan keduanya.

Tanggal dari input date native (`yyyy-MM-dd`) dikirim apa adanya sebagai `birthDate`, tanpa melewati `Date`
sehingga tidak bergeser zona waktu. Pendaftaran IGD tidak berubah (prop opt-in `birthDateSearch`).

## 2. Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/…/emergency-registration/patient-selection-step.jsx` | Prop opt-in `birthDateSearch`; kolom tanggal lahir; pencarian tanggal-lahir-saja; daftar kandidat; Reset |
| `src/components/view/…/outpatient-registration/outpatient-registration-page.jsx` | Mengaktifkan `birthDateSearch` |
| `src/lib/hooks/…/outpatient-registration/use-outpatient-registration.js` | `handleSearchPatients` meneruskan `birthDate` |
| `src/lib/services/…/emergency-registration.service.js` | Parameter `birthDate` |
| `src/style/…/emergency-registration.module.css` | `.patientCandidateList/.patientCandidateItem` (token) |

## 3. Keputusan base component

`UI GATE: 4 elemen — REUSE 1, EXTEND 1, COMPOSE 1, WRAP 1, NEW 0`

| Kebutuhan UI | Kandidat | Status | Keputusan |
| --- | --- | --- | --- |
| Baris cari + tanggal | `.patientNameBirthSearchRow` yang sudah ada | REUSE | Kelas grid 3 kolom existing |
| Kolom tanggal | input date native seperti kolom cari existing | EXTEND | Prop opt-in, IGD tidak terdampak |
| Daftar kandidat | tombol daftar + token | WRAP | Lokal di langkah ini (tidak ada base list pilih pasien) |
| Notifikasi | `ToastStack` via `onToast` | COMPOSE | Pola existing |

## 4. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `npm run lint:errors` (repo) | exit 0 | `PASS` |
| ESLint berkas tersentuh | 0 error; 5 warning lama di `patient-selection-step.jsx` (effect lama, ada di `HEAD`) | `PASS` / `EXISTING` |
| `npm run build` | exit 0, 493 halaman | `PASS` |
| `node --test tests/unit/*.test.mjs` | 2823 pass / 30 fail; 30 kegagalan identik dengan salinan `HEAD` (35 fail, 5 karena salinan tak lengkap) — modul lain (Rawat Inap, Bank Darah, Hemodialisa, menu RWI) | `EXISTING / ENVIRONMENT ISSUE` |
| Grep anti-regresi CSS baru (warna literal, typography, `!important`) | kosong | `PASS` |
| U1 kolom Tanggal Lahir tampil | lulus | `PASS` |
| U2 request `birthDate=1979-05-17` | lulus | `PASS` |
| U3 tiga pasien bertanggal sama → daftar kandidat, pilih → verifikasi | lulus (kandidat=3) | `PASS` |

Uji layar Playwright: FE dev `localhost:3000` (source terkini) dengan API `7184` dialihkan ke backend uji `https://localhost:7185` (build Release `RJ-REG-REV-BE-*`), DB `QuilvianNewDevSukma`, akun SuperAdmin. Build standalone 3100 tidak dipakai karena CORS backend hanya mengizinkan origin 3000.

## 5. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Cari tanggal lahir saja/gabungan menghasilkan pasien sesuai | Terpenuhi | U2, U3, BE B1–B3 |
| 2. Tanggal tidak bergeser zona waktu | Terpenuhi | U2 |
| 3. Reset mengembalikan default | Terpenuhi lewat kode (`handleResetIdentificationSearch`); tidak diklik di uji layar | kode |
| 4. Pendaftaran IGD tidak berubah | Terpenuhi lewat kode (prop default `false`) | kode |

## Catatan penutup

| Hal | Isi |
| --- | --- |
| AUTOMATED TEST | `SKIPPED (opsional)` — tidak ada test baru; suite unit dijalankan sebagai regresi |
| Risiko tersisa | — |
| Perubahan sampingan | `NONE` |
| Status Git | Berkas di atas `M`/`??` pada `V2QuilvianSystemFrontendDev`, belum di-commit |
| Langkah berikutnya | UAT petugas pendaftaran |
