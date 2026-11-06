# Laporan Perubahan Frontend — `FE-NV-015`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-NV-015` |
| Judul | Layar ambang lama menunggu beserta butir menu |
| Slice | `MVP-9` — `EPIC NV-09` (gelombang frontend 1, revision 2) |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/frontend-roadmap.md` — bagian Revision 2, kartu `FE-NV-015` |
| Trace | `FR-NV-053`; `NV-DEC-048`; `G-15`; `NV-OQ-022`; `VAL-NV-40`; `NV-AC-040`; `UAT-25`; layar `FE-NVW-09` (`03-frontend-architecture.md` R2.2, R2.3) |
| Contract version | `0.2.0` — `approved` (Rizki Gunawan, 2026-11-05, `NV-DEC-055`), api §8 |
| Wewenang UI | Isi layar dikunci skema `FE-NVW-09`; tata letak form, nama dan urutan butir menu `DEV_DISCRETION` |
| Dependency | `BE-NV-016` ✅ (6 Nov 2026, uji HTTP 14/14) |
| Klasifikasi | `LIGHT` — skor 3: berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika sederhana (0), kontrak yang ada (1), auth tidak ada (0), satu halaman (0) |
| Task mode | `FRONTEND` — perintah Rizki 6 Nov 2026 |
| Target tulis | `QuilvianSystemFrontendDev` (branch `RizkiV2`); laporan ini dan baris status roadmap/traceability di backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `3ab8e3215` (branch `RizkiV2`); perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `13fa1c4d` |
| Tanggal | 2026-11-06 |
| Status | ✅ **SELESAI 6 November 2026** — AC 5/5 terpetakan ke source dan terbukti di uji layar (6 cek `FE-NV-015` + 2 cek menu bersama, seluruhnya `PASS`); `npx eslint` 0 error; tangkapan layar kosong dan terisi tersimpan. Ambang dicabut lagi sesudah uji. Verifikasi developer, bukan UAT |

---

## 1. Keadaan yang ditemukan di awal

API ambang (`BE-NV-016`) sudah ada, tetapi belum ada layar. Angka yang disahkan Komite Keperawatan
hanya bisa dimasukkan lewat Swagger.

## 2. Proses bisnis dari sisi pengguna

1. Admin IT membuka **Administrator › Master Data › Ambang Panggilan Nurse View** (butir hanya bagi
   pemegang `NurseViewPatientCallWaitThreshold : Read`).
2. Selama belum ada pengesahan, status tampil "Belum dikonfigurasi — panggilan tampil tanpa warna." dan
   seluruh isian kosong — **tanpa angka bawaan** (`G-15`).
3. Admin mengisi ambang kuning dan merah (menit), nomor SK/notulen Komite, dan tanggal pengesahan, lalu
   **Simpan Ambang**. Status berubah menjadi "Dikonfigurasi — kuning 3 menit, merah 5 menit; <SK>,
   disahkan <tanggal>." dan jejak "Terakhir diubah: <nama>, <waktu>" tampil di bawah form.
4. Untuk mencabut, admin mengosongkan kedua angka, mengisi rujukan pencabutan, menyimpan, lalu
   mengonfirmasi "Cabut Ambang Panggilan?".
5. Pengguna yang hanya punya izin `Read` (misalnya kepala ruang) melihat layar yang sama dalam mode baca:
   isian terkunci, tanpa tombol simpan, dengan keterangan "Mode baca saja…".

**Contoh pesan.** Kuning 3 dengan merah 2, atau merah 500: di bawah isian tampil "Ambang kuning 1–240
menit dan ambang merah harus lebih besar, paling banyak 480 menit." dan tidak ada request yang terkirim.
Angka 500 tetap 500 di layar — tidak diubah diam-diam menjadi 480 — karena isian sengaja tanpa
`min`/`max` (pelajaran `FE-NV-004`).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Sama dengan `FE-NV-008` §3.1, ditambah modul rujukan **Interval TTV Nurse View** (`FE-NV-004`:
constants, slice, utils, hook, view, route) dan `filter-date-picker.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/administrator/master-data/nurse-view-call-wait-threshold/nurse-view-call-wait-threshold-constants.jsx` | **Baru.** Batas (1–240, ≤ 480, rujukan ≤ 200), pesan persis matriks `VAL-NV-40`/`12`/`13`, empat isian (angka tanpa `min`/`max`, `maxLength` 4), config resource `NurseViewPatientCallWaitThreshold` |
| `src/lib/state/slice/administrator/master-data/master-data-nurse-view-call-wait-threshold-slice.jsx` | **Baru.** Dua thunk (`GET /`, `PUT /`); payload angka kosong dikirim `null` |
| `src/utils/administrator/master-data/nurse-view-call-wait-threshold/nurse-view-call-wait-threshold-utils.jsx` | **Baru.** Form, validasi `VAL-NV-40` (dua angka berpasangan, urutan, batas), payload, status, jejak |
| `src/lib/hooks/administrator/master-data/nurse-view-call-wait-threshold/use-master-data-nurse-view-call-wait-threshold.jsx` | **Baru.** Pola hook interval TTV: `usePermission(…, Update)` untuk mode baca, konfirmasi cabut, "Tidak Ada Perubahan" |
| `src/components/view/administrator/master-data/nurse-view-call-wait-threshold/master-data-nurse-view-call-wait-threshold-view.jsx` | **Baru.** `BaseEditorView` + `InformationAlert` (status, mode baca, gagal muat + Coba lagi) + `ConfirmModal` cabut |
| `src/app/administrator/master-data/nurse-view-call-wait-thresholds/page.jsx` | **Baru.** Route tipis |
| `src/lib/state/store.jsx` | +2 baris: import dan `masterDataNurseViewCallWaitThreshold` |
| `src/utils/menu-sidebar/menu-items.jsx` | +6 baris: butir "Ambang Panggilan Nurse View" sesudah "Tablet Bed Nurse View", `requiredPermission: { resource: "NurseViewPatientCallWaitThreshold", action: "Read" }` (berkas bersama, restu `NV-DEC-056`; diff gabungan di laporan `FE-NV-008` §3.2) |

Nol komentar, nol CSS baru, seluruh berkas CRLF.

**Tabel keputusan base component (UI GATE):**

| Elemen | Base component | Status |
| --- | --- | --- |
| Halaman + form | `BaseEditorView` (`mode="update"`, `afterHero`, `afterForm`) | `REUSE` |
| Isian angka, teks, tanggal | `BaseTextField` (number), `BaseDateField` (`FilterDatePicker`, `max` hari ini) | `REUSE` |
| Status, mode baca, gagal muat, jejak | `InformationAlert` | `REUSE` |
| Coba lagi | `BaseButton` lewat `formProps.renderActions` | `REUSE` |
| Konfirmasi cabut | `ConfirmModal` `warning` | `REUSE` |
| Akses ditolak | `AccessDeniedGate` | `REUSE` |

`UI GATE: PASS — seluruh elemen REUSE, sama persis dengan modul rujukan Interval TTV.`

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Kosong | "Belum dikonfigurasi — panggilan tampil tanpa warna." |
| Terisi | "Dikonfigurasi — kuning N menit, merah M menit; <SK>, disahkan <tanggal>." + jejak pengubah |
| Mode baca | "Mode baca saja: akun ini tidak memiliki hak mengubah ambang."; tanpa tombol simpan |
| Gagal muat | Alert merah + Coba lagi |
| Tanpa hak akses | `AccessDeniedGate` |
| Tidak ada perubahan | Toast "Tidak Ada Perubahan" |

## 5. Endpoint yang dikonsumsi

#### Health Services / Nurse View Management / Patient Call Wait Threshold

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Ambang aktif dan status | `NurseViewPatientCallWaitThreshold : Read` |
| `PUT` | `/` | Simpan atau cabut ambang | `NurseViewPatientCallWaitThreshold : Update` |

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` 6 berkas baru + `store.jsx` + `menu-items.jsx` | 0 error, 1 warning `set-state-in-effect` pada pemetaan respons → form, pola yang sama dengan hook interval TTV | `PASS` (0 error); warning `EXISTING WARNING` | Keluaran perintah 6 Nov |
| Grep anti-regresi + tanpa `min`/`max` pada isian angka | Nol temuan | `PASS` | Keluaran perintah |
| `npm run build` | Tidak dijalankan | `NOT RUN` | Alasan sama dengan `FE-NV-008` §6 (`next dev` Rizki di port 3000) |
| Uji layar (run gabungan dengan `FE-NV-008`) | Seluruh cek `FE-NV-015` `PASS` | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-fe-nv-008-015-20261106/run-2026-11-06T07-52-06-902Z/` (`11-ambang-kosong.png`, `12-ditolak-urutan.png`, `14-ambang-terisi.png`, `16-baca-saja.png`) |

`AUTOMATED TEST: SKIPPED (opsional) — ketentuan owner tanpa unit test`.

`MANUAL TEST: PASS` — lewat uji layar otomatis (bukan UAT).

| No | Langkah | Hasil sebenarnya | Status |
| ---: | --- | --- | --- |
| L1 | Sidebar aktor berhak | Butir "Ambang Panggilan Nurse View" tampil | `PASS` |
| L11 | Buka layar saat ambang kosong | `GET` 200; status "Belum dikonfigurasi — panggilan tampil tanpa warna."; kedua isian kosong | `PASS` |
| L12 | Kuning 3, merah 2, rujukan, hari ini → Simpan | Pesan `VAL-NV-40` tampil; nol `PUT` | `PASS` |
| L13 | Ketik merah 500 → Simpan | Nilai isian tetap "500"; pesan tampil; nol `PUT` | `PASS` |
| L14 | Kuning 3, merah 5 → Simpan | `PUT` 200 payload `{3, 5, rujukan, 2026-11-06}`; toast; status "Dikonfigurasi — kuning 3 menit, merah 5 menit…" | `PASS` |
| L15 | Kosongkan kedua angka + rujukan cabut → konfirmasi | `PUT` 200 payload `null`/`null`; status kembali "Belum dikonfigurasi" | `PASS` |
| L16 | Aktor dengan izin `Read` saja | "Mode baca saja…" tampil; nol tombol simpan; isian terkunci; butir menu ambang tampil | `PASS` |
| L18 | LOKET | Butir menu ambang tersembunyi | `PASS` |

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 ambang kosong → status "Belum dikonfigurasi — panggilan tampil tanpa warna" (`NV-AC-040`) | Terpenuhi | L11 |
| AC-2 `UAT-25`: merah 2 dengan kuning 3 → ditolak dengan pesan; kuning 3, merah 5, SK, tanggal → tersimpan | Terpenuhi | L12, L14 |
| AC-3 angka yang diketik tidak berubah sendiri: 500 tetap 500 lalu ditolak | Terpenuhi | L13 |
| AC-4 izin `Read` saja → mode baca | Terpenuhi | L16 |
| AC-5 butir menu terjangkau bagi pemegang izin | Terpenuhi | L1, L16 (tampil), L18 (tersembunyi bagi tanpa izin) |
| DoD — lint `0 errors` | Terpenuhi | §6 |
| DoD — tangkapan layar kosong dan terisi | Terpenuhi | `11-ambang-kosong.png`, `14-ambang-terisi.png` |
| DoD — laporan | Terpenuhi | Berkas ini |

Tampilnya warna di konsol dan strip adalah AC `FE-NV-012` dan `FE-NV-014`.

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Angka 3/5 hanya angka uji; angka sah menunggu Komite Keperawatan (`NV-OQ-022`). DB dev kembali tanpa ambang aktif |
| Masalah yang diketahui | Rujukan pencabutan hanya tercatat di log backend (desain `BE-NV-016`) |
| Dependency backend | `BE-NV-016` ✅ |
| Data uji di DB dev | Baris ambang uji bertanda hapus; nol baris aktif |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Frontend task ini: `?? src/app/administrator/master-data/nurse-view-call-wait-thresholds/`, `?? src/components/view/administrator/master-data/nurse-view-call-wait-threshold/`, `?? src/lib/constants/administrator/master-data/nurse-view-call-wait-threshold/`, `?? src/lib/hooks/administrator/master-data/nurse-view-call-wait-threshold/`, `?? src/lib/state/slice/administrator/master-data/master-data-nurse-view-call-wait-threshold-slice.jsx`, `?? src/utils/administrator/master-data/nurse-view-call-wait-threshold/`; `M store.jsx` dan `menu-items.jsx` bersama `FE-NV-008`. Belum di-commit |
| Langkah berikutnya | Warna ambang dipakai `FE-NV-012` (konsol) dan `FE-NV-014` (strip TV) |
