# Laporan Perubahan Frontend — `FE-HRD-R14`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R14` |
| Judul | Layar K3RS: sambung dan migrasi (pola rujukan) |
| Slice | `S-C6` |
| Roadmap | `roadmap/frontend-unfinished-work.md`, Gelombang 2 |
| Trace | `HRD-DEC-176` s.d. `HRD-DEC-181`; API Contract v23 bagian 29 |
| Contract version | v23, `approved` |
| Wewenang UI | Migrasi 13 berkas layar K3RS ke base component dan API nyata; tidak mengubah backend |
| Dependency | `FE-HRD-R02` ✅ |
| Klasifikasi | `EPIC` — delapan rute, 13 berkas tampilan, integrasi API dan privasi klinis |
| Task mode | `FRONTEND` dengan penulisan laporan/status lintas repository yang sempit |
| Target tulis | Source `QuilvianSystemFrontendDev`; laporan/status blueprint Human Resource |
| Model | Codex |
| Commit frontend saat dikerjakan | `ec36164052a033ee4db95fae4a962cae62c2203c` (`HamzahV2`) |
| Commit backend yang dijadikan rujukan | `7b2b15ccd54e32771bca4cf89d3f9a855c7eea1f` (`MHamzah`, read-only) |
| Tanggal | 16 November 2026 |
| Status | 🟡 **SEBAGIAN** — source dan validasi statis lulus; live browser terblokir autentikasi lokal `401` |

## 1. Keadaan yang ditemukan di awal

Audit 11 November 2026 masih menyebut 13 layar K3RS memakai data contoh atau komponen HTML langsung. Pada HEAD saat task dikerjakan, migrasi substantifnya sudah berada dalam commit `2ce7a5911`: layar memakai `OccupationalHealthService`, base component, CSS Module bertoken, dan permission backend. Roadmap serta laporan perbaikan belum diperbarui.

Tiga endpoint daftar lintas pegawai belum tersedia di backend: daftar rekam kesehatan, pembatasan kerja, dan kecelakaan kerja. Service tidak lagi mengarang data; layar menjelaskan bahwa data belum tersedia. Kekurangan endpoint tersebut tetap dimiliki `FE-HRD-R53`/`DEC-FER-05`, bukan ditutup oleh task ini.

## 2. Proses bisnis dari sisi pengguna

1. Petugas K3RS membuka dasbor dan melihat ringkasan kelaikan, pajanan aktif, booster, serta surveilans radiasi.
2. Petugas dengan `ViewSummary` dapat mencari pegawai dan melihat kesimpulan laik kerja tanpa diagnosis atau hasil laboratorium.
3. Rincian klinis hanya diminta dan ditampilkan bila pengguna mempunyai `OccupationalHealthRecord : ViewClinicalDetail`.
4. Dokter berwenang dapat menetapkan kelaikan kerja, pembatasan, return-to-work, PPP, vaksinasi, surveilans, MCU, dan laporan cedera melalui endpoint resmi.
5. Ketika endpoint daftar belum tersedia atau request gagal, layar menampilkan keadaan gagal/tidak tersedia dan tidak menggantinya dengan data contoh.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Delapan view pada `src/components/view/hr/occupational-health/ohs-*-view.jsx`.
- `components/OhsVisualComponents.jsx`, tiga modal, dan `widget/NeedleStickPanicButton.jsx`.
- `src/lib/services/hr/occupational-health-service.js`.
- `src/lib/constants/hr/occupational-health/ohs-constants.js`.
- Controller backend `OccupationalHealthManagement/Controllers/*.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `test-with-agy/scripts/fe-hrd-r14-k3rs-readonly.mjs` | Skrip Chromium headed read-only untuk delapan rute K3RS dan bukti screenshot/network |
| `docs/module-blueprints/human-resource/task/report/frontend/FE-HRD-R14.md` | Laporan bukti task |
| `docs/module-blueprints/human-resource/roadmap/frontend-unfinished-work.md` | Status `FE-HRD-R14` menjadi 🟡 |
| `docs/module-blueprints/human-resource/roadmap/requirement-traceability.md` | Catatan bukti perbaikan dan blocker live browser |

Source aplikasi tidak memerlukan perubahan tambahan karena implementasi target sudah ada pada HEAD dan lolos pemeriksaan task.

### 3.3 Kepatuhan arsitektur frontend

Alur yang dipakai adalah route App Router → view domain → hook `useHrRemoteData`/permission → `OccupationalHealthService` → `InstanceAxios`. Tidak ada Axios instance, state manager, atau arsitektur paralel baru.

### 3.4 Gerbang tampilan

**Modul referensi visual:** `src/components/view/hr/workforce-core/employee-profile-change-list-view.jsx` dan base page Region.

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Header dan aksi | `Hero`, `BaseButton` | `REUSE` | Komponen canonical tersedia |
| Ringkasan | `SummaryGrid` | `REUSE` | Mendukung loading dan empty state |
| Filter dan tabel | `DataFilter`, `DataTable` | `REUSE` | Mendukung filter, tabel, pagination, dan empty state |
| Status | `StatusBadge` | `REUSE` | Seluruh status memakai kunci prop yang sah |
| Form | `BaseFormControl`, `ResourceFilterSelect` | `REUSE` | Pengguna tidak mengetik UUID |
| Dialog | `ConfirmModal` | `REUSE` | Dialog canonical dengan backdrop |
| Pesan/notifikasi | `InformationAlert`, `ToastStack` | `REUSE` | State gagal dan hasil aksi konsisten |
| Panel dan layout form | CSS Module existing berbasis token | `COMPOSE` | Tidak membuat base component baru |

`UI GATE: PASS — 7 kelompok REUSE, 1 COMPOSE, 0 NEW.`

```text
$ node rules/frontend/tools/check-css-usage.mjs <13 berkas JSX K3RS>
OK — 13 berkas diperiksa, 0 temuan.
$ rg -n 'grid-cols-|text-slate-|border-slate-|rounded-md|w-full|items-center|space-y-|md:|hover:|focus:' <13 berkas JSX K3RS>
NO_TAILWIND_MATCHES
$ rg -n '<(button|table|input|select|textarea)(\s|>)' <13 berkas JSX K3RS>
NO_RAW_FORM_OR_TABLE_ELEMENTS
```

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| Delapan rute K3RS | `test-with-agy/screenshots/fe-hrd-r14-k3rs/01-dashboard.png` s.d. `08-injuries.png` | Chromium headed dijalankan, tetapi sesi dialihkan ke login karena `/api/v1/auth/me` menghasilkan `401` |

`VISUAL CHECK: NOT FEASIBLE — akun pengujian/helper login lokal tidak memperoleh sesi valid; seluruh request /api/v1/auth/me menghasilkan 401.`

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Teks memuat atau loading state base component |
| Kosong | Pesan tidak ada data tanpa data contoh |
| Gagal | `InformationAlert` dengan pesan gagal atau belum tersedia |
| Tanpa hak akses | Aksi disembunyikan; rincian klinis tidak diminta dan tidak ditampilkan |

## 5. Endpoint yang dikonsumsi

#### Corporate - HRD Occupational Health & Medical Privacy

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/hrd/occupational-health/records/{employeeId}/clinical` | Rincian klinis | `OccupationalHealthRecord : ViewClinicalDetail` |
| `GET` | `/api/v1/hrd/occupational-health/records/{employeeId}/fitness-summary` | Ringkasan laik kerja | `OccupationalHealthRecord : ViewSummary` |
| `PUT` | `/api/v1/hrd/occupational-health/records/{employeeId}/fitness-status` | Penetapan kelaikan | `OccupationalHealthRecord : AssessFitnessToWork` |
| `GET` | `/api/v1/hrd/occupational-health/my-health` | Kesehatan sendiri | `OccupationalHealthRecord : ViewOwn` |

#### Corporate - HRD Work Restriction & Injury BPJS JKK

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/hrd/occupational-health/restrictions` | Terbitkan pembatasan | `WorkRestrictionAndInjury : IssueWorkRestriction` |
| `GET` | `/api/v1/hrd/occupational-health/restrictions/validate-schedule/{employeeId}` | Validasi roster | `WorkRestrictionAndInjury : Read` |
| `POST` | `/api/v1/hrd/occupational-health/injuries` | Laporan cedera kerja | `WorkRestrictionAndInjury : ReportOccupationalInjury` |

#### Corporate - HRD Needle-Stick & Occupational Exposure Emergency

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/hrd/occupational-health/needle-stick` | Antrean pajanan | `NeedleStickEmergency : Read` |
| `POST` | `/api/v1/hrd/occupational-health/needle-stick/report-cito` | Laporan cito | `NeedleStickEmergency : ReportNeedleStickEmergency` |

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint` | `0 errors`, `1252 warnings` existing | `PASS` | Exit code `0` |
| `npm run build` | Compiled, 814 static pages, standalone assets siap | `PASS` | Exit code `0` |
| CSS usage checker | 13 berkas, 0 temuan | `PASS` | Keluaran bagian 3.4 |
| Audit permission klinis | Request klinis hanya aktif bila `ViewClinicalDetail` | `PASS` | `ohs-health-records-view.jsx` |
| Chromium headed delapan rute | `/api/v1/auth/me` menghasilkan `401`; layar tidak dapat diverifikasi setelah login | `EXISTING / ENVIRONMENT ISSUE` | `result.json`, `network-readonly.log`, screenshot |

Uji manual: `NOT FEASIBLE` karena autentikasi lokal gagal membentuk sesi.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Seluruh layar K3RS memakai data nyata dan komponen standar | Terpenuhi secara source | Service nyata, base component, CSS gate 0 temuan |
| Data medis hanya tampil bagi `ViewClinicalDetail` | Terpenuhi secara source | `enabled: canViewClinical` dan conditional render |
| Pola migrasi ditulis untuk `R37`–`R41` | Terpenuhi | Tabel keputusan bagian 3.4 |
| Lint dan build | Terpenuhi | Kedua command exit code `0` |
| Screenshot setelah autentikasi dan uji akun tanpa hak | Belum terpenuhi | Sesi lokal selalu `401` |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Traceability lama mengklaim seluruh UAT PASS, tetapi bukti live browser task ini belum dapat direproduksi |
| Masalah yang diketahui | Tiga endpoint daftar dimiliki `FE-HRD-R53`; autentikasi lokal `401` |
| Dependency backend | Endpoint aksi utama tersedia; endpoint daftar tertentu belum tersedia |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Source frontend aplikasi bersih; artefak test berada pada `test-with-agy`; backend hanya laporan/status yang diberi wewenang |
| Langkah berikutnya | Pulihkan akun/sesi pengujian, ulangi delapan rute dan uji akun tanpa `ViewClinicalDetail` sebelum menaikkan ke ✅ |
