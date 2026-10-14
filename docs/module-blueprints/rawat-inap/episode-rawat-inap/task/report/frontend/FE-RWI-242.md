# Laporan Perubahan Frontend — `FE-RWI-242`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-242` |
| Judul | Panel Riwayat Perubahan General Consent |
| Slice | Perbaikan `PLAN-REPAIR-EPS-008`, `FIX-EPS-008-03` |
| Roadmap | [`../../../roadmap/frontend-roadmap-admisi-pendaftaran.md`](../../../roadmap/frontend-roadmap-admisi-pendaftaran.md) — register dan kartu 4.7 |
| Trace | [`ISSUE-EPS-008`](../../../docs/issue/issue-008-edit-riwayat-general-consent.md) `ISS-EPS-008-02`; [`PLAN-REPAIR-EPS-008`](../../../docs/plan-repair/plan-repair-008-edit-riwayat-general-consent.md) bagian 3.2 dan `FIX-EPS-008-03`; `RWI-DEC-295`, `RWI-AC-431` |
| Contract version | `GET …/admission-workspace/general-consent/history` dari `BE-RWI-227` (kontrak `0.11.0` + delta yang dicatat di `contracts/api-contract.md` 13-10-2026) |
| Wewenang UI | Skema 3.2 rencana perbaikan (wilayah Riwayat Perubahan), disetujui pemilik |
| Dependency | `BE-RWI-227` ✅, `FE-RWI-241` ✅ |
| Klasifikasi | `LIGHT` — satu hook baru, satu panel baru, satu fungsi normalisasi, satu sambungan di section |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` branch `HamzahV2` (bagian 3.2), laporan ini, roadmap, traceability, register rencana perbaikan |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `f4495aa1` (`HamzahV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `4ec02370` (`MHamzah`) + perubahan `BE-RWI-227` yang belum di-commit |
| Tanggal | 13 Oktober 2026 |
| Status | ✅ Selesai pada tingkat source, lint, unit test, dan build. Uji layar `NOT FEASIBLE` (lihat bagian 6) |

---

## 1. Keadaan yang ditemukan di awal

Sesudah `FE-RWI-241`, koreksi persetujuan sudah tersimpan sebagai versi, tetapi layar belum
menampilkan riwayatnya. Permintaan pemilik "ketahuan riwayat siapa yang melakukan edit" belum
terlihat oleh petugas. Backend sudah menyediakan `GET …/general-consent/history` (`BE-RWI-227`).

---

## 2. Proses bisnis dari sisi pengguna

1. Petugas membuka Workspace PPRI → General Consent → tab Formulir General Consent pada episode
   yang persetujuannya sudah tersimpan.
2. Di bawah kartu formulir tampil panel **Riwayat Perubahan** dengan lencana jumlah versi.
3. Setiap versi tampil sebagai satu kotak, terbaru di atas, berisi:
   - lencana "Versi N · Aktif/Digantikan";
   - waktu, nama, dan jabatan pengubah;
   - alasan perubahan;
   - daftar "label: lama → baru".
   Versi 1 tertulis "Dibuat pada langkah admisi". Bila tanda tangan diganti, muncul lencana
   "Tanda tangan diganti".
4. Sesudah Edit Persetujuan berhasil, panel memuat ulang sendiri karena nomor versi aktif berubah.
   Baris lama tetap tampil selama pemuatan, sehingga panel tidak berkedip.

Contoh tampilan: "Versi 2 · Aktif — 13 Oktober 2026 10.15 — Dewi — Alasan: NIK penanda tangan
salah ketik saat admisi — No. Identitas: …98 → …99"; di bawahnya "Versi 1 · Digantikan — Sari —
Dibuat pada langkah admisi."

**Jalur tidak normal**

| Kejadian | Yang dilihat petugas |
| --- | --- |
| Gagal memuat riwayat | Alert "Riwayat perubahan tidak dapat dimuat" berisi pesan server, ditambah tombol Coba Lagi. Form dan cetak tetap bekerja |
| Belum ada versi | "Belum ada riwayat perubahan" |
| Pengguna tanpa hak `Update` | Panel tetap tampil, karena hanya butuh `Read` |
| Episode tanpa persetujuan tersimpan | Panel tidak tampil dan tidak ada permintaan riwayat |
| Tab Surat Persetujuan aktif | Riwayat tidak dimuat (hemat permintaan) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Berkas `FE-RWI-241`, kontrak `InpatientAdmissionConsentHistoryResponse` (`BE-RWI-227`), dan class
`lineList`, `metaBlock`, `statusLine`, `metaHint`, `metaLabel`, `messageList`, `emptyState`,
`emptyStateTitle` pada `admission-workspace.module.css` beserta pemakaiannya di
`admission-document-section.jsx` dan `belief-values-form.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/health-services/inpatient-management/inpatient-admission-workspace.service.js` | `fetchAdmissionConsentHistory` → `GET {episodeId}/admission-workspace/general-consent/history` |
| `src/utils/health-services/inpatient-management/inpatient-admission-consent-edit-utils.js` | `normalizeConsentHistory`: respons → baris tampilan, tahan respons camelCase maupun PascalCase, varian lencana per status |
| `src/lib/hooks/health-services/inpatient-management/use-admission-consent-history.js` (baru) | Memuat riwayat dengan `AbortController`; status memuat diturunkan dari kunci permintaan, tanpa `setState` sinkron di effect; `reloadKey` = nomor versi aktif |
| `src/components/view/.../sections/general-consent/general-consent-history-panel.jsx` (baru) | Panel Riwayat Perubahan |
| `src/components/view/.../sections/general-consent/general-consent-section.jsx` | Memanggil hook riwayat (hanya tab Formulir + persetujuan tersimpan) dan menampilkan panel di bawah kartu formulir |
| `src/components/view/.../sections/general-consent/general-consent-edit-form.jsx` | Perbaikan warning `react-hooks/use-memo` dari `FE-RWI-241`: `useMemo(() => readPenColor(), [])` |
| `tests/unit/inpatient-admission-consent-edit.test.mjs` | Satu test baru untuk `normalizeConsentHistory` |

### 3.3 Kepatuhan arsitektur frontend

Alurnya view → hook → service; normalisasi berada di `utils`. Tidak ada CSS baru dan tidak ada
Redux slice.

`UI GATE: 6 elemen — REUSE 5, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
|---|---|---|---|---|
| Kartu panel dan judul | `formCard`/`formCardHeader`/`formCardTitle` | CSS module Workspace PPRI | REUSE | — |
| Lencana jumlah versi, status versi, "Tanda tangan diganti" | `StatusBadge` | `base-features/status-badge.jsx` | REUSE | `success` Aktif, `neutral` Digantikan, `danger` Dibatalkan |
| Gagal memuat | `InformationAlert` + `BaseButton` | base-features | REUSE | Coba Lagi memakai `loading` |
| Kosong | `emptyState`/`emptyStateTitle` | Dipakai `admission-document-section.jsx:549` | REUSE | — |
| Baris kepala versi | `statusLine` | Dipakai `admission-document-section.jsx:524` | REUSE | — |
| Daftar versi beserta perubahannya | `lineList` + `metaBlock` + `messageList` | CSS module Workspace PPRI | COMPOSE | Lihat keputusan di bawah |

> **Keputusan: bentuk daftar versi**
>
> - **A. Kotak per versi (`lineList` + `metaBlock`) dengan daftar perubahan di dalamnya — Rekomendasi (dipakai).** Perubahan per versi bisa beberapa baris dan terbaca utuh pada tablet; tanpa CSS baru.
> - **B. `DataTable` seperti riwayat cetak (`admission-print-history-table.jsx`).** Konsisten dengan tabel riwayat cetak, tetapi daftar "lama → baru" dalam satu sel menjadi sempit dan sulit dibaca di layar 10 inci.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Memuat riwayat perubahan..." dan lencana "Memuat..." (bila belum ada baris) |
| Kosong | "Belum ada riwayat perubahan" |
| Gagal | "Riwayat perubahan tidak dapat dimuat" + pesan server + Coba Lagi |
| Tanpa hak akses | Workspace PPRI sendiri menahan pengguna tanpa `Read` ("Akses Tidak Tersedia", `RWI-AC-343`); pengguna `Read` tanpa `Update` tetap melihat panel |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Admission Workspace

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/episodes/{episodeId}/admission-workspace/general-consent/history` | Isi panel Riwayat Perubahan | `InpatientAdmissionDocument : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` (format JSON) pada 9 berkas source + test yang disentuh `FE-RWI-241`/`242` | 0 error, 1 warning lama `react-hooks/set-state-in-effect` (`use-admission-general-consent.js:61`). Warning `react-hooks/use-memo` dari `FE-RWI-241` ditemukan pada lint ini lalu diperbaiki | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test` tiga berkas (`consent-edit`, `consent`, `workspace`) | 30/31 lulus. Satu kegagalan lama "FE-RWI-212: alamat Workspace PPRI…" (10 vs 9 menu) — konstanta menu tidak disentuh, garis dasar 13-10-2026 | `PASS` untuk scope; kegagalan lain `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah |
| `npm run build` | Berhasil: exit 0, "Compiled successfully in 74s", 492/492 halaman, postbuild standalone siap | `PASS` | Log `fe-build-242.log` di scratchpad sesi; dev server pengguna tidak dihentikan |
| Grep anti-regresi (warna literal, `<button>`, `<table>`, utilitas typography Bootstrap, inline style) pada panel dan section | Kosong | `PASS` | Keluaran perintah |

AUTOMATED TEST: `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-consent-edit.test.mjs tests/unit/inpatient-admission-consent.test.mjs` — PASS (16/16)

MANUAL TEST: NOT FEASIBLE — backend lokal pengguna masih menjalankan DLL lama (`dotnet run --no-build`), sehingga endpoint riwayat belum aktif; proses pengguna tidak dihentikan.

**Tidak dijalankan:** `npm run lint:errors` (rusak oleh lingkungan; diganti ESLint per berkas) dan suite unit penuh (bukan gerbang).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria (`FIX-EPS-008-03`) | Status | Bukti |
| --- | --- | --- |
| 1. Sesudah koreksi NIK, panel menampilkan versi 2 Aktif (waktu, Dewi, alasan, "No. Identitas: …98 → …99") dan versi 1 Digantikan "Dibuat pada langkah admisi" | Terpenuhi di source + unit test normalisasi; uji layar `NOT FEASIBLE` | `GeneralConsentHistoryPanel`, `normalizeConsentHistory`, `reloadKey` = versi aktif |
| 2. Gagal memuat → pesan + Coba Lagi di panel; form dan cetak tetap bekerja | Terpenuhi di source | Cabang `error` pada panel; hook terpisah dari hook data cetak |
| 3. Pengguna tanpa `Update` tetap melihat riwayat bila punya `Read` | Terpenuhi di source | Panel tidak memeriksa `can("Update")`; endpoint dijaga `Read` |
| DoD: lint dan build | Terpenuhi | Bagian 6 |
| DoD: uji layar | Belum | `MANUAL TEST: NOT FEASIBLE`; dikecualikan sesuai keputusan pemilik bahwa uji runtime dijalankan pemilik |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Satu warning ESLint lama di hook data cetak |
| Masalah yang diketahui | Tidak ada yang baru |
| Dependency backend | `BE-RWI-227`; perlu backend dibangun ulang dan dijalankan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat laporan `FE-RWI-241`; tambahan berkas baru `use-admission-consent-history.js` dan `general-consent-history-panel.jsx` (`??`) |
| Langkah berikutnya | Pemilik menjalankan ulang backend dan menguji skenario `PLAN-REPAIR-EPS-008` bagian 8 |
