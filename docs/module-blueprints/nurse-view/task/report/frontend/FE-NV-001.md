# Laporan Perubahan Frontend — `FE-NV-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-NV-001` |
| Judul | Daftar perangkat dan menu |
| Slice | `MVP-1` — pengelolaan perangkat display |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/frontend-roadmap.md` — kartu `FE-NV-001` |
| Trace | `FR-NV-005`; `NV-DEC-016`; layar `FE-NVW-01` (`03-frontend-architecture.md` §2, §3.1); `contracts/api-contract.md` §1 |
| Contract version | `0.1.0` — `approved` |
| Wewenang UI | Isi layar dan sumber data dikunci skema §3.1; nama butir menu, urutan, ikon, warna `DEV_DISCRETION` |
| Dependency | `BE-NV-002` 🟡 — source ada di working tree backend, belum di-build. Dikerjakan lebih dulu atas permintaan Rizki agar uji cukup sekali dari layar |
| Klasifikasi | `MEDIUM` — skor 6: satu repository untuk source (0), berkas diperiksa > 20 (2), berkas diubah 4–8 (1), logika sederhana (0), memakai kontrak yang ada (1), persistence tidak ada (0), auth tidak ada (0), satu halaman + menu (1) |
| Task mode | `FRONTEND` (laporan dan status roadmap di repository backend sesuai wewenang sempit skill) |
| Target tulis | `QuilvianSystemFrontendDev` (branch `RizkiV2`) |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `fc5af984f` — berbeda dari snapshot desain `76b0c6e4b`; berkas yang disentuh (`menu-items.jsx`, `store.jsx`) hanya ditambah baris |
| Commit backend yang dijadikan rujukan | `4af76bcb` + working tree `BE-NV-002` |
| Tanggal | 2026-10-29 |
| Status | ✅ **SELESAI** — source, lint, dan build selesai; seluruh skenario daftar/menu di FE-NV-002 §6 (no. 1, 2, 3, 17, 18, 19) `PASS` per 29 Okt. Ini verifikasi developer, bukan UAT |

---

## 1. Keadaan yang ditemukan di awal

Belum ada layar apa pun untuk perangkat Nurse View, dan sidebar Administrator › Master Data
belum memuat butirnya. Admin IT tidak punya jalan masuk ke fitur ini.

## 2. Proses bisnis dari sisi pengguna

1. Admin IT membuka **Administrator › Master Data › Display Nurse View**.
2. Ia melihat daftar perangkat: kode, nama, lantai, lokasi, status, dan online/offline.
3. Ia menyaring dengan kata kunci, lantai, atau status. Setiap perubahan saringan mengembalikan
   tabel ke halaman 1.
4. Ia menekan **+ Tambah Perangkat Nurse View** untuk membuka form (`FE-NV-002`), atau klik dua
   kali sebuah baris untuk membuka detail.

**Contoh.** Lantai 3 punya `NVW-000001` (Aktif, Online) dan `NVW-000004` (Nonaktif, "Offline
sejak 28 Okt 2026 21.00"). Saringan "Lantai 3 · 20 bed" menampilkan keduanya; menambah saringan
status "Aktif" menyisakan `NVW-000001`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` dan `CLAUDE.md` frontend; suite skill `master-data-feature-standard.md`,
`ui-consistency-checklist.md`, `REPORT_TEMPLATE.md`; rujukan `hr/master-data/job-level` (constants,
slice, utils, tiga hook, tiga view, lima route); pembanding `administrator/master-data/queue-display-device`;
`base-features/confirm-modal.jsx`, `base-detail-view.jsx`, `base-editor-form.jsx`,
`base-editor-field.jsx`, `status-badge.jsx`, `filter-select.jsx`; `store.jsx`; `menu-items.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/administrator/master-data/nurse-view-device/nurse-view-device-constants.jsx` | **Baru.** `NURSE_VIEW_DEVICE_CONFIG` tunggal: endpoint, route, token scope, salinan teks, filter, kolom, field form, baris detail, pesan |
| `src/lib/state/slice/administrator/master-data/master-data-nurse-view-device-slice.jsx` | **Baru.** Delapan thunk untuk delapan endpoint kontrak (dipakai bersama `FE-NV-002`); kata sandi **tidak** pernah ditulis ke state |
| `src/utils/administrator/master-data/nurse-view-device/nurse-view-device-utils.jsx` | **Baru.** Fungsi murni: baca field, status/badge, online, versi, opsi lantai, form, validasi, payload, baris detail |
| `src/lib/hooks/administrator/master-data/nurse-view-device/use-master-data-nurse-view-device.jsx` | **Baru.** Controller halaman daftar |
| `src/components/view/administrator/master-data/nurse-view-device/master-data-nurse-view-device-view.jsx` | **Baru.** Halaman daftar |
| `src/app/administrator/master-data/nurse-view-devices/page.jsx`, `nurse-view-device-client.jsx` | **Baru.** Route tipis |
| `src/lib/state/store.jsx` | +2 baris: import dan `masterDataNurseViewDevice` |
| `src/utils/menu-sidebar/menu-items.jsx` | +5 baris: butir "Display Nurse View" di bawah "Display Antrian" |

### 3.3 Kepatuhan arsitektur frontend

| Aturan | Keadaan |
| --- | --- |
| Bentuk berkas standar master data (rujukan `job-level`) | Diikuti: constants, slice, utils, hook, view, route tipis, registrasi store dan menu |
| Sembilan thunk master data | **Delta disengaja:** kontrak `0.1.0` tidak punya `filters/metadata`, `summary`, `options`, `PATCH status`, `DELETE`. Slice memetakan tepat delapan endpoint yang ada. Akibatnya tidak ada kartu ringkasan dan tidak ada hidrasi metadata |
| Filter tanggal dan periode wajib tampil | **Tidak dirender:** skema `FE-NVW-01` yang disetujui hanya memuat kata kunci, lantai, status. Backend tidak menerimanya |
| Komentar alasan di constants | Tidak ditulis — ketentuan proyek "nol komentar baris" yang lebih tinggi |
| `InstanceAxios`, `signal`, `abort` | Dipakai di setiap thunk dan effect |
| Token route privat, tanpa UUID di layar | `registerPrivateRouteToken`; `safeString` mengganti UUID dengan "-" |
| Tanpa CSS Module baru | 0 berkas CSS; hanya `base-data-components.module.css` |

**Tabel keputusan base component (UI GATE):**

| Elemen | Base component | Status |
| --- | --- | --- |
| Kepala halaman | `Hero` | `REUSE` |
| Baris saringan + pencarian | `DataFilter` + `FilterSelect` ×3 | `REUSE` |
| Tabel + paginasi | `DataTable` + `RegionPagination` | `REUSE` |
| Status & online | `StatusBadge` (`active`/`inactive`/`pending`) | `REUSE` |
| Tombol tambah | `BaseButton` | `REUSE` |
| Akses ditolak | `AccessDeniedGate` | `REUSE` |

`UI GATE: PASS — seluruh elemen REUSE, tidak ada keputusan user yang dibutuhkan.`

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Mengambil data perangkat Nurse View..." (kerangka `DataTable`) |
| Kosong | "Belum ada perangkat Nurse View." · "Coba gunakan filter lain atau tambahkan perangkat baru." |
| Gagal | Pesan backend di alert, misalnya "Gagal mengambil daftar perangkat Nurse View." |
| Tanpa hak akses | `AccessDeniedGate` dengan pesan 403 backend |
| Lantai gagal dimuat | Pilihan lantai hanya "Semua lantai" |

## 5. Endpoint yang dikonsumsi

#### Health Services / Nurse View Management / Display Device

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Tabel, saringan, paginasi | `NurseViewDisplayDevice : Read` |
| `GET` | `/floors` | Pilihan saringan lantai | `NurseViewDisplayDevice : Read` |

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` atas 8 lokasi berkas baru/berubah | 0 error, 2 warning `react-hooks/set-state-in-effect` (di hook detail dan editor `FE-NV-002`) | `EXISTING WARNING` | Rujukan `job-level` menghasilkan 2 warning yang sama persis (`npx eslint src/lib/hooks/hr/master-data/job-level`) |
| Grep anti-regresi UI (`<button`, `<table`, `style={`, warna literal, CSS baru) | 0 / 0 / 0 / 0 / 0 | `PASS` | Keluaran grep |
| Nol komentar | Nol hasil | `PASS` | Grep |
| `npm run build` | Lulus | `PASS (pernyataan owner)` | `.next/BUILD_ID` `7H3ZrbK00TFmEZdHVvLuE` 2026-10-29 10:26, lebih baru dari source terakhir (09:57) |
| Uji layar — menu (FE-NV-002 §6 no. 1) | "Display Nurse View" tepat di bawah "Display Antrian" | `PASS` | `run-2026-10-29T03-51-41-193Z/S01.json` + `.png` |
| Uji layar — daftar kosong (no. 2) | Route benar lewat klik menu, `GET /` `200` `totalData` 0, teks "Belum ada perangkat Nurse View." | `PASS` | `S02` |
| Uji layar — saringan Lantai (no. 3) | Putaran 2: enam lantai berbentuk "Lantai N · M bed", sama jumlahnya dengan `GET /floors` `200`. Putaran 1 `BLOCKED (data)` | `PASS` | `run-lanjutan-2026-10-29T04-47-18-042Z/S03` |
| Uji layar — akun tanpa hak (no. 19) | LOKET: `403` → "Ups! Akses Ditolak" | `PASS` | `S19` |
| Uji layar — kolom Lantai berubah sesudah update (no. 17) | Sel LANTAI baris `NVW-000001` = "Lantai 4" | `PASS` | `S17-ulang-2026-10-29T04-50-08-805Z` |
| Uji layar — saringan Status (no. 18) | Nonaktif: `status=3&pageNumber=1`, 0 baris + teks kosong; Aktif: `status=2&pageNumber=1`, baris Aktif. Keduanya GET `200` | `PASS` | `S18a`, `S18b` |
| Kolom Online | "Belum Pernah Online" untuk perangkat yang belum pernah memanggil runtime | Teramati | `S17-ulang` (pengisian `LastOnlineAt` ada di `BE-NV-003`) |

Bukti: `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-nv-mvp1-20261029/` (di-ignore git).
Uraian lengkap, aktor, dan run yang digantikan ada di `FE-NV-002` §6.

`AUTOMATED TEST: SKIPPED (opsional) — ketentuan proyek: agent tidak menulis/menjalankan unit test.`
`MANUAL TEST: DONE — Playwright 2026-10-29, dua putaran (putaran 2 sesudah data lantai diisi Rizki).`

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Skema `FE-NVW-01` lengkap (saringan, kolom, tombol tambah, empat keadaan) | Terpetakan ke source | §3.2, §4 |
| `FR-NV-005` online/offline dari `IsOnline`/`LastOnlineAt` | Terpetakan ke source | `getOnlineLabel` |
| Butir menu terjangkau | Terpenuhi | §6, `S01`/`S02` |
| DoD — lint | Terpenuhi | §6 |
| DoD — tangkapan layar | Terpenuhi | Menu, daftar kosong, saringan lantai, daftar berisi data, saringan status, akses ditolak |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Menu tampil bagi siapa pun yang membuka sidebar Administrator; isi tabel tetap dijaga `NurseViewDisplayDevice : Read` di backend |
| Masalah yang diketahui | `NONE` |
| Dependency backend | `BE-NV-002` ✅ |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M src/lib/state/store.jsx`, `M src/utils/menu-sidebar/menu-items.jsx`, `?? src/app/administrator/master-data/nurse-view-devices/`, `?? src/components/view/administrator/master-data/nurse-view-device/`, `?? src/lib/constants/.../nurse-view-device/`, `?? src/lib/hooks/.../nurse-view-device/`, `?? src/lib/state/slice/.../master-data-nurse-view-device-slice.jsx`, `?? src/utils/administrator/master-data/nurse-view-device/` |
| Langkah berikutnya | `FE-NV-002` tinggal no. 5 (toast sesudah simpan); lihat laporannya |
