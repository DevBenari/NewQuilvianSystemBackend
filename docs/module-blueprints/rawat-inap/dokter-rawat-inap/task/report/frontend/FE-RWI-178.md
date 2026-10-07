# Laporan Perubahan Frontend - FE-RWI-178

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID / Judul | FE-RWI-178 - Penanda Pasca operasi di Konteks pasien |
| Slice / Roadmap | D2 / frontend-roadmap-finishing.md revision 1 |
| Trace | FR-RWF-081/082; RWI-DEC-213; RWI-AC-339; UAT-RWF-38 |
| Dependency | FE-RWI-196 milik episode-rawat-inap (selesai & terintegrasi) |
| Status | ✅ Selesai — Seluruh acceptance criteria terbukti, build lulus, unit test lulus, Playwright E2E lulus |

| Contract version | 0.7.0 approved, RWI-BP-001 revision 8 |
| Wewenang UI | DEV_DISCRETION dalam batas kartu task; tidak mengubah keputusan klinis |
| Task mode / Target tulis | FRONTEND; laporan/roadmap/traceability di backend; source backend read-only |
| Branch | HamzahV2 / origin/HamzahV2, ditetapkan pemilik |
| Tanggal | 6 Oktober 2026 |

---

## 1. Outcome yang Dipasang

1. Dokter melihat tombol penanda **"Pasca operasi"** untuk setiap kasus bedah berstatus `Completed` (`OprCaseStatus = 5` / `"Completed"`) pada panel Konteks Pasien (`InpatientPhysicianContextHeader` dan `InpatientEpisodeHeader`), diurutkan dari kasus terbaru ke terlama (`actualEndTime` / `scheduledStartTime` / `createdAt`).
2. Mengklik tombol penanda membuka laci ringkasan pasca operasi `PostOpSummaryDrawer` (`FE-INP-28` dari `FE-RWI-196`) dengan mode baca-saja (`readOnly={true}`).
3. Pada mode baca-saja, tombol serah terima **Terima** dan **Tolak** tidak ditampilkan sama sekali (mematuhi `RWI-DEC-213`).
4. Laporan operasi yang belum final menampilkan banner peringatan `"Laporan operasi belum final"` dan rincian klinis tetap terlindungi.
5. Bila pasien tidak memiliki kasus bedah selesai, bila izin `OperatingRoomCase : Read` tidak dimiliki, atau bila pemanggilan API kasus operasi gagal, penanda tidak ditampilkan dan sama sekali tidak menghalangi informasi klinis lainnya.
6. Urutan dan jumlah delapan tab Ruang Kerja Dokter rawat inap (`assessment`, `cppt`, `screening`, `visit`, `procedure`, `prescription`, `supporting`, `resume`) tetap utuh tanpa perubahan (`RWI-AC-339`).

---

## 2. Keputusan Desain & UI Gate

UI GATE: 4 elemen — REUSE 4, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0.

| Elemen | Bukti Source / Props | Status | Keputusan & Rekomendasi |
| --- | --- | --- | --- |
| Penanda klik | `src/components/features/base-features/base-button.jsx` (`variant="secondary"`, `size="sm"`, `aria-haspopup="dialog"`) | REUSE | Tombol sekunder ukuran kecil per kasus bedah `Completed` |
| Konteks pasien | `InpatientPhysicianContextHeader.jsx` & `InpatientEpisodeHeader.jsx` (`ClinicalContextBar`) | REUSE | Wadah badge/tombol konteks keselamatan existing tanpa primitive baru |
| Laci ringkasan | `PostOpSummaryDrawer.jsx` (`isOpen`, `caseId`, `caseNumber`, `readOnly={true}`, `onClose`) | REUSE | Laci dari `FE-RWI-196` dipakai ulang secara bersih dengan prop `readOnly={true}` |
| Peringatan draft/error | `InformationAlert.jsx` & `report-not-final-banner` pada laci existing | REUSE | Menggunakan komponen alert existing pada laci |

---

## 3. Berkas yang Diubah / Dibuat

1. `src/components/features/health-services/operating-room-management/post-op-summary/post-op-summary-drawer.jsx`:
   - Memperbaiki import autentikasi ke `@/lib/state/slice/auth/permission-slice` dan `@/lib/state/slice/auth/login-slice`.
   - Menyesuaikan signature selector curried: `useSelector(selectHasPermission("OperatingRoomHandover", "Receive"))`.
   - Menambahkan guard render `if (!isOpen) return null;`.
2. `src/components/view/health-services/inpatient-management/doctor-inpatient/inpatient-physician-context-header.jsx`:
   - Mengambil daftar kasus operasi via `getOperatingRoomCases({ encounterId })` yang dijaga permission `OperatingRoomCase : Read`.
   - Menyaring kasus berstatus `Completed`, mengurutkan terbaru di depan, dan merender tombol penanda `"Pasca operasi"`.
   - Membuka `PostOpSummaryDrawer` dengan `readOnly={true}`.
3. `src/components/view/health-services/inpatient-management/physician-workspace/components/inpatient-episode-header.jsx`:
   - Mengintegrasikan penanda `"Pasca operasi"` dan `PostOpSummaryDrawer` pada header ruang kerja dokter alternatif.
4. `src/lib/constants/health-services/inpatient-management/inpatient-departure-constants.jsx`:
   - Memulihkan export `INPATIENT_DEPARTURE_LIMITS` yang dibutuhkan oleh `inpatient-episode-detail-view.jsx`.
5. `tests/unit/inpatient-physician-post-op-marker.test.mjs`:
   - Unit test verifikasi integrasi penanda pasca operasi, mode `readOnly`, dan invariant delapan tab dokter (`RWI-AC-339`).
6. `tests/e2e/inpatient-doctor-finishing.spec.mjs`:
   - Menambahkan skenario E2E penanda pasca operasi dan laci baca-saja tanpa tombol Terima/Tolak.

---

## 4. Bukti Verifikasi & Pengujian

### A. Automated Tests
1. **Lint Errors:**
   - Command: `npm.cmd run lint:errors`
   - Hasil: **PASS (exit 0)** — 0 error.
2. **Production Build:**
   - Command: `npm.cmd run build`
   - Hasil: **PASS (exit 0)** — Build Next.js Turbopack sukses penuh, standalone output siap dijalankan.
3. **Unit Tests Terkait Dokter & Pasca Operasi:**
   - Command: `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-physician-post-op-marker.test.mjs tests/unit/post-op-summary.test.mjs tests/unit/inpatient-ancillary-order.test.mjs tests/unit/inpatient-physician-needs-review.test.mjs tests/unit/inpatient-procedure-utils.test.mjs tests/unit/inpatient-prescription-builder-utils.test.mjs tests/unit/inpatient-supporting-service-modernisasi.test.mjs tests/unit/inpatient-physician-clinical-tabs.test.mjs tests/unit/inpatient-physician-workspace.test.mjs`
   - Hasil: **PASS 88/88 tests (exit 0)**.
4. **Playwright E2E Finishing Test:**
   - Command: `npx.cmd --no-install playwright test tests/e2e/inpatient-doctor-finishing.spec.mjs --workers=1 --reporter=line`
   - Hasil: **PASS 11/11 tests (exit 0)**:
     - `Rehab tetap placeholder dan pilihan Rehab tidak membuat request jaringan` (PASS)
     - `Lab tanpa tarif tetap dapat dikirim dan klik ganda mengirim satu pesanan` (PASS)
     - `Katalog kosong dan gagal tidak menampilkan contoh; retry memulihkan katalog` (PASS)
     - `Katalog benar-benar kosong tidak mengisi pemeriksaan contoh` (PASS)
     - `Kegagalan resolver terlihat dan tidak menahan kirim Lab` (PASS)
     - `Harga Lab berasal dari resolver dan memiliki label perkiraan` (PASS)
     - `Harga resep berlabel perkiraan; tanpa tarif tidak menjadi Rp 0 dan tanggungan yang tidak applicable disembunyikan` (PASS)
     - `Enam sumber terpisah: diet gagal; verifikasi darah 403` (PASS)
     - `Enam sumber terpisah: diet gagal; verifikasi darah 409` (PASS)
     - `FE-RWI-178: Pasien tanpa kasus Completed tidak memunculkan penanda Pasca operasi` (PASS)
     - `FE-RWI-178: Pasien dengan kasus Completed memunculkan penanda Pasca operasi dan membuka laci baca-saja tanpa tombol Terima/Tolak` (PASS)

---

## 5. Pemenuhan Acceptance Criteria

| No | Kriteria Acceptance | Status | Bukti |
|---|---|:---:|---|
| 1 | Pasien tanpa kasus `Completed` → penanda tidak tampil | ✅ | Playwright E2E test `[10/11]` PASS (`expect(post-op-marker).toHaveCount(0)`) |
| 2 | Pasien dengan kasus `Completed` → penanda membuka ringkasan baca-saja tanpa Terima/Tolak (`UAT-RWF-38`) | ✅ | Playwright E2E test `[11/11]` PASS (`badge (baca saja)` tampil, tombol Terima & Tolak count 0) |
| 3 | Laporan draft → `"Laporan operasi belum final"` | ✅ | Playwright E2E test `[11/11]` PASS (`data-testid="report-not-final-banner"` tampil) |
| 4 | Jumlah dan urutan delapan tab tidak berubah | ✅ | Unit test `inpatient-physician-post-op-marker.test.mjs` PASS (`RWI-AC-339`) |
| 5 | Tanpa `OperatingRoomCase : Read` → penanda tidak tampil | ✅ | Guard `canReadOprCase` pada effect & unit test PASS |
| 6 | Bebas nominal rupiah | ✅ | Unit test `post-op-summary.test.mjs` PASS (0 nominal Rupiah) |
