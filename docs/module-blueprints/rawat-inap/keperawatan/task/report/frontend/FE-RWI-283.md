# Laporan Perubahan Frontend — `FE-RWI-283`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-283` |
| Judul | Tampilan EWS mengikuti server |
| Slice | Repair 015 keperawatan; Slice 1 [MVP Penuntasan Rawat Inap](../../../../MVP-Penuntasan-Rawat-Inap.md) |
| Roadmap | [roadmap-repair-015.md](../../../roadmap/roadmap-repair-015.md), kartu `FE-RWI-283` |
| Trace | `FIX-KEP-015-07` menutup `ISS-KEP-015-T2` dan `T3`; plan repair 015 bagian 3.A dan 3.B |
| Contract version | `PatientVitalSignSeriesItem` pada source backend `53418d37` (`earlyWarningScore`, `ewsRiskLevel`, `ewsMonitoringRecommendation`, `needDoctorNotification`, `isExcludedFromChart`) |
| Wewenang UI | Rangka plan repair 015 bagian 3.A/3.B yang disetujui pemilik 2026-11-02; hanya cara tampil EWS |
| Dependency | Tidak ada (field tambahan seri tanda vital ikut commit backend `da737fb9`) |
| Klasifikasi | `LIGHT` — dua layar, tanpa komponen atau stylesheet baru |
| Task mode | `FRONTEND` (laporan di repository backend sesuai aturan lokasi laporan) |
| Target tulis | `QuilvianSystemFrontendDev` branch `HamzahV2` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | Mulai di `bb7e93dc8`; sebagian perubahan sesi ini di-commit pemilik sebagai `6053e347c` (9 Nov 11.55); sisanya masih di working tree |
| Commit backend yang dijadikan rujukan | `53418d372b6e37fa76aeabea9cf3b395c5534641` |
| Tanggal | 9 November 2026 |
| Status | ✅ Selesai — keenam AC terbukti di peramban dengan data uji |

---

## 1. Keadaan yang ditemukan di awal

Util `inpatient-ews-display-utils.js` (commit `66b7e5600`) sudah membaca golongan dari server. Saat
diverifikasi ternyata masih ada lima celah:

| No | Celah | Akibat bagi perawat |
| ---: | --- | --- |
| 1 | Tabel tab Vital Sign hanya menampilkan angka skor; kata golongan cuma tooltip | AC 2 tidak terpenuhi: "Tinggi" tidak terbaca di tabel |
| 2 | Anjuran server dibungkus tanda kurung | AC 5: kalimat tidak sama persis dengan server |
| 3 | Tanda vital tanpa skor tampil "Belum dihitung" di ringkasan dan tetap berbadge "-" di tabel | AC 6 tidak terpenuhi |
| 4 | **Ditemukan saat uji langsung:** server mengirim deret urut naik, tetapi tabel menandai baris pertama sebagai "Terkini" | Label "Terkini" menempel pada pengukuran **paling lama**. Contoh: Ny. M (samaran) diukur 09.40 (EWS 6) lalu 11.00 (EWS 2); yang berlabel "Terkini" justru 09.40 |
| 5 | **Ditemukan saat uji langsung:** pengukuran yang dibatalkan (`isExcludedFromChart`) tetap dipakai sebagai "Observasi Terkini" dan tampil seperti baris sah | Ny. D (samaran): pengukuran 10.10 EWS 14 sudah dibatalkan, tetapi ringkasan tetap "EWS 14 — Kritis", padahal server menilai EWS terakhir yang sah adalah 7 |

Pengawasan Harian juga menampilkan golongan sebagai teks dalam kurung, bukan badge, dan pengukuran batal tampil seperti baris biasa.

---

## 2. Proses bisnis dari sisi pengguna

1. Perawat menyimpan tanda vital di tab Vital Sign atau Pengawasan Harian.
2. Server menghitung skor, golongan (`Low`/`Medium`/`High`/`Critical`), anjuran, dan penanda lapor dokter.
3. Ringkasan "Observasi Terkini" memakai pengukuran **sah** terakhir, menampilkan "EWS: 7 — Kritis", "⚠ Perlu lapor dokter", dan kalimat anjuran server apa adanya.
4. Tabel riwayat diurutkan dari yang terbaru; label "Terkini" menempel pada pengukuran sah terakhir; badge "7 · Kritis" berwarna sesuai golongan.
5. Pengawasan Harian menampilkan "EWS: 15" disusul badge "Kritis".

Contoh golongan dari server: skor 2 → "Rendah" (hijau), 4 → "Sedang" (kuning), 6 → "Tinggi" (oranye), 7 ke atas → "Kritis" (merah).

Jalur tidak normal: pengukuran batal tetap tampil di riwayat dengan tulisan dicoret dan label "Dibatalkan", tetapi tidak pernah menjadi "Terkini" dan tidak masuk grafik. Bila semua pengukuran batal, kartu ringkasan tidak tampil.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend, `rules/frontend/*` (arsitektur, katalog base component, gerbang keputusan, checklist UI, test policy, report template); plan repair 015; `inpatient-ews-display-utils.js`; `inpatient-progress-note-constants.jsx` (`EWS_RISK_LABEL`); `nursing-workspace.module.css` (`ewsBadge`, `ewsLow`…`ewsCritical`, `rowCancelled`, `badgeCancelledStatus`); backend `InpatientVitalSignService.GetSeriesAsync` (urut naik), `PatientVitalSignCalculation`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/health-services/inpatient-management/nursing-workspace/sections/nursing-care/tabs/nursing-vital-sign-tab.jsx` | Ringkasan tanpa skor "-"; anjuran tanpa kurung; badge tabel "skor · golongan"; tanpa badge bila skor kosong; `validVitalSigns` menyaring `isExcludedFromChart` untuk ringkasan dan grafik; tabel urut terbaru; baris batal dicoret + "Dibatalkan"; `variant="outline"` (tidak sah) → `secondary`; perbandingan mode tampilan dipindah ke variabel |
| `.../sections/daily-monitoring/daily-monitoring-section.jsx` | Golongan EWS sebagai badge `ewsBadge` + kelas warna; baris batal dicoret + "Dibatalkan"; class `styles.obsTime` yang tidak ada di module dihapus; perbandingan string dipindah ke variabel |

Bagian pertama (badge, anjuran, "-") ikut commit pemilik `6053e347c`. Urutan "Terkini", penyaringan pengukuran batal, dan perbaikan gerbang CSS masih di working tree.

### 3.3 Kepatuhan arsitektur frontend

View → util `inpatient-ews-display-utils` → konstanta `EWS_RISK_LABEL`. Tidak ada hook, slice, service, atau komponen baru. Gaya memakai kelas yang sudah ada di `nursing-workspace.module.css`.

### 3.4 Gerbang tampilan

**Modul referensi visual:** tab Vital Sign itu sendiri (kelas `ewsBadge`/`ewsLow`…`ewsCritical` dan `rowCancelled`/`badgeCancelledStatus` yang sudah dipakai di Pengawasan Harian untuk cairan dan GDS).

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Badge golongan EWS tabel Vital Sign | `styles.ewsBadge` + `ewsLow/Medium/High/Critical` | `REUSE` | Sudah dipakai kolom yang sama |
| Badge golongan EWS Pengawasan Harian | Kelas yang sama (module yang sama diimpor) | `REUSE` | Konsisten dengan tab Vital Sign |
| Baris pengukuran batal | `styles.rowCancelled` + `styles.badgeCancelledStatus` | `REUSE` | Pola yang sama dipakai baris cairan/GDS batal |
| Tombol Coba Lagi / ganti tampilan | `BaseButton variant="secondary"` | `REUSE` | Mengganti `outline` yang tidak sah |

`UI GATE: PASS — 4 elemen REUSE, 0 COMPOSE, 0 NEW`

```text
$ node <rules>/frontend/tools/check-css-usage.mjs nursing-vital-sign-tab.jsx daily-monitoring-section.jsx inpatient-monitoring-table-columns.jsx drug-return-form-view.jsx medication-administration-chart-panel.jsx inpatient-discharge-view.jsx
OK — 6 berkas diperiksa, 0 temuan.
$ git diff -U0 -- <berkas jsx> | grep "^+" | grep -nE "<pola #7 class Tailwind>"
(kosong)
$ (pola #3 tombol mentah, #9 field mentah/UUID, #10 prop tidak sah) 
(kosong)
```

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| Vital Sign Ny. D, pengukuran batal | `test-with-agy/screenshots/rawat-inap-mvp-1109/05-layar-ews/v3-dian-vital-gulir.png` | Ringkasan EWS 7 Kritis; baris 10.10 dicoret "Dibatalkan"; "Terkini" pada 09.00 |
| Vital Sign Ny. M | `.../05-layar-ews/v2-maya-vital-gulir.png` | "Terkini" pada 11.00 (2 · Rendah); 09.40 (6 · Tinggi) |
| Vital Sign Ny. P | `.../05-layar-ews/v2-putri-vital-gulir.png` | Ringkasan "EWS: -"; 4 · Sedang; 2 · Rendah; baris tanpa skor "-" |
| Pengawasan Harian Ny. D | `.../05-layar-ews/v3-dian-harian-gulir.png` | "EWS: 7" + badge Kritis; 14 "Dibatalkan" |
| Form catat pertama | `.../01-vital-sign/A1-dian-ews8-sesudah-simpan.png` | Setara referensi |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka bawaan tab (tidak berubah) |
| Kosong | "Belum Ada Rekaman Tanda Vital" (tidak berubah) |
| Gagal | Pesan galat dan tombol "Coba Lagi" (kini `secondary`) |
| Tanpa hak akses | Mengikuti aturan ruang kerja keperawatan yang ada |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Clinical Management / Patient Vital Sign

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/clinical-management/patient-vital-signs/episodes/{episodeId}` | Deret tanda vital tab Vital Sign | `PatientVitalSign : Read` |
| `POST` | `/v1/health-services/clinical-management/patient-vital-signs` | Membuat data uji lewat form | `PatientVitalSign : Create` |
| `PATCH` | `/v1/health-services/clinical-management/patient-vital-signs/{id}/cancel` | Membatalkan data uji (AC jalur batal) | `PatientVitalSign : Cancel` |

---

## 6. Verifikasi

Data uji dibuat di DB dev `QuilvianNewDevHamzah` lewat form dan endpoint yang sama: Ny. D (VIP 311) EWS 7 pukul 09.00 dan EWS 14 pukul 10.10 (lalu dibatalkan); Ny. M (VIP 309) EWS 6 pukul 09.40 dan EWS 2 pukul 11.00; Ny. P (VIP 310) EWS 2, EWS 4, dan satu pengukuran tanpa skor.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | exit 0 | `PASS` | Keluaran perintah |
| `npm run test:unit` | 4200 test: 4144 lulus, 56 gagal; ke-56 kegagalan juga gagal di worktree HEAD tanpa perubahan ini (billing, CRM, sidebar, dll.) | `EXISTING / ENVIRONMENT ISSUE` | Perbandingan daftar kegagalan dengan worktree `bb7e93dc8` |
| Test unit berkas terkait (`inpatient-daily-monitoring`, `inpatient-ews-display-utils`, `inpatient-nursing-care`, dll.) | 86/86 lulus | `PASS` | `node --test` terfokus |
| `npm run build` | exit 0 (tiga kali, terakhir sesudah semua perbaikan) | `PASS` | Keluaran perintah |
| AC 1 — Pengawasan Harian "EWS: 15 [Kritis]" | "EWS: 7" + badge "Kritis" (data uji 7) | `PASS` | `v3-dian-harian-gulir.png` |
| AC 2 — EWS 6 "Tinggi" di ringkasan dan tabel | Tabel "6 · Tinggi" | `PASS` | `v2-maya-vital-gulir.png` |
| AC 3 — EWS 2 "Rendah", tidak kuning | "2 · Rendah" hijau | `PASS` | `v2-putri-vital-gulir.png` |
| AC 4 — EWS 4 "Sedang" | "4 · Sedang" | `PASS` | `v2-putri-vital-gulir.png` |
| AC 5 — anjuran sama persis dengan server | "Pemantauan terus menerus tanda-tanda vital, pertimbangkan eskalasi klinis segera." tanpa kurung | `PASS` | `v3-dian-vital-gulir.png` |
| AC 6 — tanpa skor "EWS: -" tanpa badge | Ringkasan "EWS: -", tabel "-" | `PASS` | `v2-putri-vital-gulir.png` |
| Pengukuran batal tidak menjadi "Terkini" | Ringkasan Ny. D kembali ke EWS 7, sama dengan `ews-alerts` | `PASS` | `v3-dian-vital-gulir.png` |

`AUTOMATED TEST: npm run test:unit — FAIL (56 kegagalan lama di luar cakupan; 0 kegagalan baru)`

`MANUAL TEST: PASS`

**Tidak dijalankan:** `test:e2e` (tidak diminta task).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `FIX-KEP-015-07` AC 1–6 | Terpenuhi | Bagian 6 |
| DoD `npm run lint:errors`, `npm run test:unit`, `npm run build` | Terpenuhi (unit: kegagalan lama saja) | Bagian 6 |
| DoD laporan `task/report/frontend/FE-RWI-283.md` | Terpenuhi | Berkas ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Form "Catat Tanda Vital" mengisi bawaan waktu ukur dengan jam UTC (tampil 04.52 saat jam rumah sakit 11.52), dan kolom "Kesadaran" menampilkan angka enum mentah ("1"). Keduanya cacat lama di luar cakupan; dicatat untuk issue terpisah |
| Masalah yang diketahui | Mode grafik Pengawasan Harian masih memplot pengukuran batal (batang tekanan darah); di luar AC |
| Dependency backend | Tidak ada yang tertunda |
| Perubahan sampingan | `NONE` |
| Interupsi | Tab CDP macet setelah server frontend dinyalakan ulang; jendela Chrome profil uji dibuka ulang |
| Status Git | Berubah (belum di-commit): `nursing-vital-sign-tab.jsx`, `daily-monitoring-section.jsx` beserta berkas task lain pada MVP |
| Langkah berikutnya | `FE-RWI-284` |
