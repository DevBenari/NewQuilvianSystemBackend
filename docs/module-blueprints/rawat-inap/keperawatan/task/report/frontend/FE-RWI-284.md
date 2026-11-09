# Laporan Perubahan Frontend — `FE-RWI-284`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-284` |
| Judul | Tab Monitoring "EWS Perlu Lapor Dokter" |
| Slice | Repair 015 keperawatan; Slice 1 [MVP Penuntasan Rawat Inap](../../../../MVP-Penuntasan-Rawat-Inap.md) |
| Roadmap | [roadmap-repair-015.md](../../../roadmap/roadmap-repair-015.md), kartu `FE-RWI-284` |
| Trace | `FIX-KEP-015-04` (`ISS-KEP-015-01`); rangka plan repair 015 bagian 3.C |
| Contract version | `GET /monitoring/ews-alerts` dari `BE-RWI-255` ✅ (source backend `53418d37`) |
| Wewenang UI | Rangka 3.C plan repair 015 (disetujui 2026-11-02) |
| Dependency | `BE-RWI-255` ✅ 9 November 2026 |
| Klasifikasi | `LIGHT` — satu tab baru di halaman yang ada, memakai pola tab lain |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` branch `HamzahV2` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | Source masuk commit `66b7e5600` (2 Nov); diverifikasi pada `6053e347c` + working tree |
| Commit backend yang dijadikan rujukan | `53418d372b6e37fa76aeabea9cf3b395c5534641` |
| Tanggal | 9 November 2026 |
| Status | ✅ Selesai — AC 1, 2, 3, 5 terbukti di layar/endpoint dengan data uji; AC 4 terbukti di source |

---

## 1. Keadaan yang ditemukan di awal

Tab, kolom, penormal data, dan tautan "Buka Tanda Vital" sudah ada di commit `66b7e5600`
(`inpatient-monitoring-constants.jsx`, `inpatient-monitoring-table-columns.jsx`,
`inpatient-monitoring-utils.jsx`, `inpatient-monitoring-view.jsx`). Verifikasi tidak menemukan celah
kode; yang belum ada hanya bukti di layar, karena DB dev belum punya tanda vital rawat inap.
**Tidak ada perubahan source pada task ini.**

---

## 2. Proses bisnis dari sisi pengguna

1. Kepala ruangan membuka Rawat Inap → Daftar Pantau, lalu tab "EWS Perlu Lapor Dokter (n)".
2. Badge jumlah pada tab menunjukkan banyaknya pasien berisiko.
3. Penyaring: unit layanan, ruangan, dan golongan ("Tinggi & Kritis" bawaan, "Kritis saja", "Tinggi saja").
4. Tiap baris: pasien + No. RM, lokasi (bed, kamar, bangsal), badge "skor · golongan", penanda "Perlu lapor dokter", pengukur dan waktu ukur, anjuran server, tombol "Buka Tanda Vital".
5. "Buka Tanda Vital" membuka ruang kerja perawat pada tab Vital Sign pasien itu.

Contoh: Ny. D (samaran) VIP 311 EWS 14 tampil di atas Ny. M VIP 309 EWS 6. Saat penyaring "Kritis saja" dipilih, hanya Ny. D yang tersisa.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Berkas pada bagian 1, `use-inpatient-monitoring.jsx`, test `tests/unit/inpatient-monitoring-ews-alerts.test.mjs`, controller backend `InpatientMonitoringController.cs:183`.

### 3.2 Berkas yang berubah

`NONE` — source sudah lengkap di `66b7e5600`.

### 3.3 Kepatuhan arsitektur frontend

Kunci daftar `ews-alerts` sama dengan path endpoint (diuji `inpatient-monitoring-ews-alerts.test.mjs`); data lewat hook monitoring yang sama dengan tab lain; kolom di `inpatient-monitoring-table-columns.jsx` terpisah dari view.

### 3.4 Gerbang tampilan

**Modul referensi visual:** tab daftar pantau lain di halaman yang sama ("Pulang Sebelum Izin Kasir", "Tanpa Perawat PJ").

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Tab + badge jumlah | `monitoring-tabs.jsx` yang ada | `REUSE` | Sama dengan tab lain |
| Tabel | `DataTable` lewat `monitoring-table.jsx` | `REUSE` | Sama dengan tab lain |
| Badge golongan | `StatusBadge` (`danger`/`warning`/`info`) | `REUSE` | Nilai status sah |
| Tombol Buka Tanda Vital | `BaseButton variant="primary"` | `REUSE` | Sama dengan aksi tab lain |
| Penyaring | `monitoring-filter.jsx` | `REUSE` | Sama dengan tab lain |

`UI GATE: PASS — 5 elemen REUSE, 0 COMPOSE, 0 NEW`

```text
$ node <rules>/frontend/tools/check-css-usage.mjs ... inpatient-monitoring-table-columns.jsx ...
OK — 6 berkas diperiksa, 0 temuan.
```

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| Daftar Pantau, tab EWS (2) | `test-with-agy/screenshots/rawat-inap-mvp-1109/04-monitoring-ews/snap1-tab.png` | Setara tab lain |
| Saringan "Kritis saja" | `.../04-monitoring-ews/snap1-saring-kritis-saja.png`, `snap1-kritis-saja-tabel.png` | Hanya Ny. D "14 · Kritis" |
| Tab sesudah pembatalan/pengukuran ulang | `.../04-monitoring-ews/v3-tab.png` | Satu baris, Ny. D EWS 7 |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Mengambil daftar pasien dengan EWS tinggi atau kritis..." |
| Kosong | "Tidak ada pasien dengan EWS Tinggi atau Kritis pada saringan ini." |
| Gagal | "Gagal mengambil daftar pasien dengan EWS tinggi atau kritis." + tombol muat ulang halaman |
| Tanpa hak akses | Mengikuti `InpatientMonitoring : Read` seperti tab lain |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Monitoring

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/inpatient-management/monitoring/ews-alerts` | Isi tab dan badge jumlah | `InpatientMonitoring : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors`, `npm run build` | exit 0 | `PASS` | Sama dengan run MVP (lihat FE-RWI-283) |
| `npm run test:unit` | 4144 lulus / 56 gagal lama; `inpatient-monitoring-ews-alerts.test.mjs` lulus | `EXISTING / ENVIRONMENT ISSUE` untuk 56 kegagalan lama | Perbandingan dengan worktree HEAD |
| AC 1 — tiga pasien, tampil dua, urut skor | Ny. D (14) lalu Ny. M (6); Ny. P (2) tidak tampil | `PASS` | `snap1-tab.png`, endpoint snapshot-1 |
| AC 2 — diukur ulang membaik, keluar dari daftar | Ny. M diukur 11.00 EWS 2 → hilang | `PASS` | Endpoint snapshot-2 |
| AC 3 — pengukuran batal, pakai pengukuran sebelumnya | Ny. D 10.10 dibatalkan → tampil EWS 7 pukul 09.00 | `PASS` | Endpoint snapshot-3, `v3-tab.png` |
| AC 4 — episode `Closed` tidak tampil | DB dev tidak punya episode tutup yang memiliki tanda vital; source menyaring `Admitted`/`DischargePending` | `PASS` (penelusuran source) | Laporan `BE-RWI-255` |
| AC 5 — pasien IGD/poli tidak tampil | 48 tanda vital IGD/poli berpenanda lapor dokter tidak muncul | `PASS` | Endpoint snapshot-1 |
| Saringan "Kritis saja" | Hanya golongan Kritis | `PASS` | `snap1-saring-kritis-saja.png` |

`AUTOMATED TEST: npm run test:unit — FAIL (56 kegagalan lama di luar cakupan; 0 kegagalan baru)`

`MANUAL TEST: PASS`

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `FIX-KEP-015-04` AC 1–5 dari sisi layar | Terpenuhi | Bagian 6 |
| DoD lint, unit, build | Terpenuhi (unit: kegagalan lama saja) | Bagian 6 |
| DoD laporan | Terpenuhi | Berkas ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | — |
| Masalah yang diketahui | Revisi `05-skema-tampilan.md` dan `api-contract.md` untuk tab ini masuk Slice 6 MVP |
| Dependency backend | `BE-RWI-255` ✅ |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Tidak ada berkas task ini yang berubah |
| Langkah berikutnya | `FE-RWI-285` |
