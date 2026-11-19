# Laporan Perubahan Frontend — `FE-HRD-R44`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R44` |
| Judul | Verifikasi layar klinis dan keselamatan |
| Slice | `S-C6` (K3RS), `S-C5` (Hubungan Industrial), `S-C1` (Kredensial), `S-C1-OPPE`; Gelombang 4 |
| Roadmap | `roadmap/frontend-unfinished-work.md` — kartu `FE-HRD-R44`; task asal `FE-HRD-200`, `215`, `275` |
| Trace | Kontrak masing-masing slice (v18, v19, v22, v23); laporan `FE-HRD-R14`, `R15`, `R30`, `R32` |
| Contract version | Kontrak slice + source controller backend branch `MHamzah` (dibaca saja) |
| Wewenang UI | Uji browser P0/P1; perbaikan kecil penanganan galat |
| Dependency | `FE-HRD-R14` 🟡, `FE-HRD-R15` 🟡, `FE-HRD-R30` 🟡, `FE-HRD-R32` 🟡 |
| Klasifikasi | `STANDARD` — verifikasi 32 rute, 1 berkas diubah |
| Task mode | `FRONTEND` — source backend hanya dibaca |
| Target tulis | `QuilvianSystemFrontendDev/src/**`; laporan ini, laporan uji live, tautan roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `f095e2e0a` → `f7a1f0917` (perubahan R44 ikut ter-commit pengguna dalam `f7a1f0917`) |
| Commit backend yang dijadikan rujukan | `604cac6a` → `fcdd9f4d` (branch `MHamzah`) |
| Tanggal | 18 November 2026 |
| Status | 🟡 Sebagian — skenario memuat data dan kegagalan server (`500`/`403`) teruji di 32 layar; PSV yang menelan galat diperbaiki dan terverifikasi. Alur tulis klinis belum diuji karena penulisan data di luar izin. |

---

## 1. Keadaan yang ditemukan di awal

- 32 layar sudah memakai API nyata sejak `R14`, `R15`, `R18`, `R20`, `R30`, dan `R32`, tetapi belum pernah dibuka di browser karena autentikasi lokal tertahan (`401`).
- Uji live menemukan bahwa **PSV** tidak membaca `radarError` dari Redux. Saat radar lisensi gagal, kartu ringkasan menampilkan 0 dan tidak ada pesan apa pun.

## 2. Proses bisnis dari sisi pengguna

1. Petugas kredensial membuka **Verifikasi Sumber Primer (PSV)** untuk mengecek STR/SIP yang mendekati kedaluwarsa (radar H-7).
2. Bila server gagal memuat radar, ringkasan kini menampilkan "-" disertai pesan galat merah, sehingga petugas tahu datanya belum termuat. Ia tidak lagi salah menyimpulkan bahwa tidak ada lisensi yang hampir habis.
3. Petugas K3RS, petugas Hubungan Industrial, dan komite medik membuka layar masing-masing. Pegawai tanpa hak ditolak dengan pesan "belum punya hak".

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

32 rute pada `hrw4-1118-routes.json` (R44), view dan service K3RS/HI/kredensial/OPPE, `menu-items.jsx`, serta controller backend `HealthRecordsController`, `MedicalExaminationsController`, `EmployeeDisciplinaryController`, `ProfessionalLicenseController`, `FppeFocusedEvaluationController`, dan `RecredentialingQualityController`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/hr/credentialing/credentialing-psv-dashboard-view.jsx` | Membaca `error: radarError` dari `selectCredentialingState`; ringkasan radar bernilai "-" saat gagal; `InformationAlert variant="danger"` menampilkan pesan galat radar |

### 3.3 Kepatuhan arsitektur frontend

Memakai state Redux yang sudah ada (`credentialingSlice`) dan base component `InformationAlert`. Tidak ada state, HTTP, atau komponen paralel baru.

### 3.4 Gerbang tampilan

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Pesan galat radar | `InformationAlert` (`variant="danger"`) | `REUSE` | Pola galat standar layar HR |
| Kartu ringkasan radar | `SummaryGrid` | `REUSE` | Hanya nilai yang berubah ("-" saat gagal) |

`UI GATE: PASS — 2 elemen REUSE, 0 COMPOSE, 0 NEW`

```text
$ npx eslint <9 berkas Gelombang 4, termasuk credentialing-psv-dashboard-view.jsx>
✖ 4 problems (0 errors, 4 warnings)   # 4 warning lama react-hooks/set-state-in-effect di 4 view ESS (bukan berkas R44)
$ node .../check-css-usage.mjs <8 berkas JSX Gelombang 4>
OK — 8 berkas diperiksa, 0 temuan.
$ grep Tailwind <berkas>
(kosong — hanya utilitas Bootstrap yang sah)
```

Screenshot:

| Layar | Berkas |
| --- | --- |
| PSV sebelum perbaikan (500 tiruan) | `test-with-agy/screenshots/hrw4-1118/R44-kegagalan-server/500-03-psv.png` |
| PSV sesudah perbaikan (500 tiruan) | `test-with-agy/screenshots/hrw4-1118/verifikasi-perbaikan/07-psv-radar-500.png` |
| 32 layar admin / non-admin | `.../R44-admin/`, `.../R44-nonadmin/` |

`VISUAL CHECK: PASS`

## 4. State yang ditangani di layar

| State | Bukti |
| --- | --- |
| Memuat dan berhasil | 32/32 layar admin |
| Gagal server `500` | Insiden HI, Pemicu FPPE, Sinkron SIMRS, PSV (setelah perbaikan) → pesan jelas |
| Ditolak `403` | Sama dengan di atas → pesan jelas |
| Tanpa hak | 18 layar non-admin |
| Data belum tersedia di server | Rekam K3RS: info "daftar rekam kesehatan seluruh pegawai belum tersedia di server" |

## 5. Endpoint yang dikonsumsi

Tidak berubah. Endpoint yang diuji dirinci per `[Tags]` pada [laporan uji live](../../testing/test-by-agy/laporan-live-testing-gelombang4-klinis-keselamatan.md#4-endpoint-yang-diuji).

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Sapu admin 32 rute | 32/32 tanpa 5xx/crash/blank/pesan Axios | `PASS` | `R44-admin/hasil.jsonl` |
| Sapu non-admin 32 rute | Tanpa crash; 18 pesan tanpa hak | `PASS` | `R44-nonadmin/hasil.jsonl` |
| Simulasi `500`/`403` (5 layar × 2) | 6 ramah, 2 benar (tanpa panggilan saat dimuat), 2 PSV gagal → diperbaiki | `PASS` setelah perbaikan | `R44-kegagalan-server/` |
| Verifikasi ulang PSV `500` | "-" + "Simulasi galat server untuk uji." tampil | `PASS` | `verifikasi-perbaikan/07-psv-radar-500.png` |
| `npx eslint` | 0 error | `PASS` | 3.4 |
| `check-css-usage.mjs` | 0 temuan | `PASS` | 3.4 |
| `npm run build` HEAD `f7a1f0917` | **FAIL** di berkas di luar cakupan (`employee-job-descriptions-view.jsx:19`, impor `getTodayDate`); Turbopack tidak melaporkan galat lain | `FAIL (di luar cakupan)` | `scratchpad/build4.log` |
| Alur tulis klinis | Tidak dijalankan | `NOT RUN` | Bagian 8 |

`MANUAL TEST: PASS` untuk memuat data dan kegagalan server; `NOT FEASIBLE` untuk alur tulis karena penulisan data di luar izin Gelombang 4.
`AUTOMATED TEST: SKIPPED (opsional) — perubahan satu kondisi tampilan; diuji langsung di browser.`

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Skenario keberhasilan diuji | 🟡 Sebagian | Memuat data 32/32; simpan/putuskan belum |
| Skenario kegagalan server diuji | ✅ | 10 simulasi + verifikasi ulang PSV |
| Outcome "alur klinis terbukti berjalan" | 🟡 | Alur tulis belum diuji |
| Laporan dan status roadmap | ✅ | Laporan ini dan laporan uji live |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Menu K3RS (8/8) dan Hubungan Industrial (7/7) terlihat oleh pegawai biasa (LBT-W4-KL-02) |
| Masalah yang diketahui | Alur tulis klinis belum diuji; data klinis DB dev sedikit |
| Dependency backend | Tidak ada blocker backend baru dari R44 |
| Perubahan sampingan | `NONE` |
| Data uji tertinggal | Tidak ada |
| Langkah berikutnya | Uji alur tulis per peran setelah izin data uji; `requiredPermission` menu K3RS/HI |
