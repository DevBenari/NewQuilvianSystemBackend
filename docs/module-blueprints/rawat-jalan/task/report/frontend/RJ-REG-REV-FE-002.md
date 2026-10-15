# Laporan Perubahan Frontend — `RJ-REG-REV-FE-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-REG-REV-FE-002` |
| Judul | Detail dan Edit di Daftar Pasien RJ |
| Requirement | `RJ-REG-REV-002` (REV-08) |
| Roadmap | [registration-revision-roadmap.md](../../../roadmap/registration-revision-roadmap.md) bagian `4` |
| Trace | `RJ-REG-DEC-005` |
| Dependency | `RJ-REG-REV-BE-003` ✅ |
| Task mode | `CROSS-REPO MODE` — frontend (`RJ-REG-DEC-001`) |
| Commit frontend | `70b3735d` (`sukmagpV2`), belum di-commit |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 14 Oktober 2026 |
| Status | ✅ Selesai teknis (`TESTED`); UAT pengguna belum |

## 1. Proses dari sisi pengguna

Setiap baris Daftar Pasien Rawat Jalan punya aksi **Detail** (selalu) dan **Edit Data Pendaftaran** (hanya
pemegang `OutpatientEncounter : Update`, kunjungan belum batal dan sebelum Sedang Konsultasi).
- Detail (`/outpatient-encounters/[id]`, `BaseDetailView`): kunjungan, poli, dokter, jadwal, antrean, pembayaran,
  rujukan, data pasien termasuk kewarganegaraan. Tombol **Perbarui** hanya bila server menyatakan `canEdit`;
  panel samping menjelaskan alasan bila edit ditutup.
- Edit (`/outpatient-encounters/[id]/edit`, `BaseEditorView`): No. RM dan Nama baca saja; No. HP, WhatsApp,
  email, kewarganegaraan (+ negara asal bila WNA), alamat, catatan. Validasi layar sama dengan server; galat
  server tampil di atas form. Sukses kembali ke Detail.
Kunjungan di luar cakupan dijawab `404` → pesan "tidak ditemukan atau berada di luar cakupan Anda".

## 2. Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/…/outpatient-encounters/[id]/page.jsx`, `[id]/edit/page.jsx` | Route baru |
| `src/components/view/…/outpatient-encounters/outpatient-encounter-detail-view.jsx` | Baru |
| `src/components/view/…/outpatient-encounters/outpatient-encounter-edit-view.jsx` | Baru |
| `src/lib/hooks/…/outpatient-encounters/use-outpatient-encounter-detail.jsx` | Baru — loader, editor, negara |
| `src/utils/…/outpatient-encounter/outpatient-encounter-detail-utils.jsx` | Baru — baris detail, field, validasi, payload, tanggal date-only |
| `src/lib/services/…/outpatient-encounter.service.js` | `getOutpatientEncounterDetail`, `updateOutpatientEncounterRegistrationData` |
| `src/components/view/…/outpatient-encounters/outpatient-encounter-table-columns.jsx`, `…-list-view.jsx` | Aksi Detail/Edit, izin `Update` |

## 3. Keputusan base component

`UI GATE: 5 elemen — REUSE 5, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

`BaseDetailView`, `BaseEditorView`, `RowActionMenu`, `StatusBadge`, `AccessDeniedGate` dipakai apa adanya.

## 4. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `npm run lint:errors` (repo) | exit 0 | `PASS` |
| ESLint berkas tersentuh | 0 error; 5 warning lama di `patient-selection-step.jsx` (effect lama, ada di `HEAD`) | `PASS` / `EXISTING` |
| `npm run build` | exit 0, 493 halaman | `PASS` |
| `node --test tests/unit/*.test.mjs` | 2823 pass / 30 fail; 30 kegagalan identik dengan salinan `HEAD` (35 fail, 5 karena salinan tak lengkap) — modul lain (Rawat Inap, Bank Darah, Hemodialisa, menu RWI) | `EXISTING / ENVIRONMENT ISSUE` |
| Grep anti-regresi CSS baru (warna literal, typography, `!important`) | kosong | `PASS` |
| L1 aksi Detail + Edit Data Pendaftaran di baris | lulus | `PASS` |
| L2 detail terbuka, WNA tampil | lulus | `PASS` |
| L3 No. RM & Nama baca saja | lulus | `PASS` |
| L4 email salah ditolak di layar | lulus | `PASS` |
| L5 simpan `PATCH 200`, detail menampilkan nilai baru | lulus | `PASS` |

Uji layar Playwright: FE dev `localhost:3000` (source terkini) dengan API `7184` dialihkan ke backend uji `https://localhost:7185` (build Release `RJ-REG-REV-BE-*`), DB `QuilvianNewDevSukma`, akun SuperAdmin. Build standalone 3100 tidak dipakai karena CORS backend hanya mengizinkan origin 3000.

## 5. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Detail terbuka untuk kunjungan dalam cakupan | Terpenuhi | L2 |
| 2. Edit hanya bila `canEdit` | Terpenuhi (status ≥ 6 dibuktikan BE D8; tanpa izin lewat kode — akun uji SuperAdmin) | L1, kode |
| 3. Simpan memperbarui detail; galat tampil | Terpenuhi | L4, L5 |
| 4. Field identitas baca saja | Terpenuhi | L3 |

## Catatan penutup

| Hal | Isi |
| --- | --- |
| AUTOMATED TEST | `SKIPPED (opsional)` — tidak ada test baru; suite unit dijalankan sebagai regresi |
| Risiko tersisa | — |
| Perubahan sampingan | `NONE` |
| Status Git | Berkas di atas `M`/`??` pada `V2QuilvianSystemFrontendDev`, belum di-commit |
| Langkah berikutnya | Admin mencentang `OutpatientEncounter : Update`; UAT |
