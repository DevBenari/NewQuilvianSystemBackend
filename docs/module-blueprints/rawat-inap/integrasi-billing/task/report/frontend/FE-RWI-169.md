# Laporan Perubahan Frontend — `FE-RWI-169`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-169` |
| Judul | Daftar "Pulang sebelum izin kasir" |
| Slice | `MVP-1` / `RWF-W1` — gelombang eksekusi 1 roadmap frontend Finishing |
| Roadmap | [`frontend-roadmap-finishing.md`](../../../roadmap/frontend-roadmap-finishing.md) — kartu `FE-RWI-169` |
| Trace | `FR-RWF-007`; `RWI-DEC-186` |
| Contract version | `integrasi-billing` `1.1.0` `approved` 2 Oktober 2026 (`RWI-DEC-221`); frontend 6.3 `FE-INT-03`; API 3.5 (`departures-before-clearance`, `closures-without-financial-clearance` + `ClosureClearanceObserved`) |
| Wewenang UI | Posisi daftar di dalam kelompok `episode-rawat-inap` `DEV_DISCRETION`; urutan antarkelompok mengikat (`02-module-map.md` 3.5) |
| Dependency | `BE-RWI-153` [BE] 🟡 — endpoint dan kolom jejak tersedia, alur pulang lengkap belum UAT |
| Klasifikasi | `MEDIUM` — skor 6: berkas diperiksa 9–20 (1), berkas diubah > 8 (2), logika sedang (1), memakai kontrak yang ada (1), satu halaman (1) |
| Task mode | `FRONTEND` — backend strict read-only |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `tests/unit/**`; laporan, roadmap, traceability `integrasi-billing` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | Mulai di `8740efa02`; implementasi ikut commit pengguna `f6eca4ef7`, lalu merge pengguna `eb0a0a790` |
| Commit backend yang dijadikan rujukan | `10d3e7ee` |
| Tanggal | 6 Oktober 2026 |
| Status | ✅ **SELESAI.** Kelima acceptance criteria terpetakan ke source. Butir DoD uji manual **dikecualikan atas keputusan pengguna 6 Oktober 2026** |

**Otorisasi eksekusi.** Roadmap berstatus `DRAFT`; pengguna memerintahkan seluruh task dikerjakan
sampai selesai pada 5 Oktober 2026.

---

## 1. Keadaan yang ditemukan di awal

- Daftar Pantau `FE-INP-09` belum punya daftar pasien yang keluar ruangan sebelum izin kasir;
  endpoint `GET monitoring/departures-before-clearance` sudah ada di backend tetapi belum dipanggil.
- Daftar "Menembus Gerbang Keuangan" belum menampilkan status kasir saat penutupan, padahal
  `OverrideClosureItemResponse` kini membawa `ClosureClearanceObserved`.
- Backend mengirim jejak status kasir sebagai **angka** `InpClearanceObservation`
  (`1` disetujui, `2` menunggu, `3` terkendala, `4` dicabut, `9` tidak terbaca), karena tidak ada
  `JsonStringEnumConverter`; status kasir sekarang dikirim sebagai teks Billing beserta `IsReadable`.
- Tes `inpatient-monitoring.test.mjs` "keempat daftar pantau" sudah gagal sebelum task ini dimulai,
  karena sesi lain menambah dua daftar (`FE-RWI-199`).

---

## 2. Proses bisnis dari sisi pengguna

**Tujuan.** Kasir, admisi, dan kepala ruangan tahu pasien mana yang sudah pulang padahal kasir belum
memberi izin, supaya tagihannya dikejar.

**Pelaku.** Pemegang `InpatientMonitoring : Read`.

**Langkah.**

1. Pengguna membuka Daftar Pantau Rawat Inap dan memilih tab **Pulang Sebelum Izin Kasir**.
2. Saringan tersedia: Unit Layanan, **Keluar dari tanggal**, **Keluar sampai tanggal**, dan
   **Cakupan episode** (bawaan "Belum ditutup saja", atau "Termasuk episode sudah ditutup").
3. Setiap baris memuat episode beserta waktu keluar, pasien, unit, dicatat oleh, **Kasir saat
   keluar**, **Kasir sekarang**, dan tombol **Detail Episode**.
4. Saringan klien "status kasir" membantu menyempitkan baris menurut kolom Kasir sekarang.

**Contoh.** Tn. Contoh A keluar 4 Oktober pukul 09.05 saat kasir masih "Menunggu kasir". Pukul 11.00
kasir menyetujui. Kolom **Kasir saat keluar** tetap "Menunggu kasir", sedangkan **Kasir sekarang**
berubah menjadi "Disetujui kasir" pada pembacaan berikutnya.

**Daftar penutupan tanpa izin kasir.** Tab yang dulu bernama "Menembus Gerbang Keuangan" kini
"Ditutup Tanpa Izin Kasir" dan memuat kolom **Kasir Saat Penutupan**. Penutupan sebelum kontrak
`1.1.0` tidak punya jejak itu dan tampil "-".

**Jalur tidak normal.**

| Keadaan | Yang dilihat pengguna |
| --- | --- |
| Status kasir tidak terbaca | "Status kasir tidak dapat dibaca" |
| Gagal memuat | "Data gagal dimuat." beserta tombol **Coba lagi** |
| Tanpa `InpatientMonitoring : Read` | Halaman ditutup pesan akses ditolak |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`InpatientMonitoringController.cs`, `InpDischargeService.Departure.cs`
(`GetDeparturesBeforeClearanceAsync`), `InpatientDepartureDtos.cs`, `InpatientMonitoringDtos.cs`,
`InpCensusQueryService.cs`; frontend `inpatient-monitoring-constants.jsx`,
`inpatient-monitoring-utils.jsx`, `use-inpatient-monitoring.jsx`, `inpatient-monitoring-view.jsx`,
`inpatient-monitoring-table-columns.jsx`, `monitoring/components/*`, `filter-date-picker.jsx`,
`02-module-map.md` 3.5.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/inpatient-management/inpatient-monitoring-constants.jsx` | Kunci `departures-before-clearance` dan entri daftarnya; saringan bawaan Periode dan cakupan; istilah "Ditutup Tanpa Izin Kasir" |
| `src/utils/health-services/inpatient-management/inpatient-monitoring-utils.jsx` | `normalizeDepartureBeforeClearanceItem/List`; `buildMonitoringQuery` mengirim `from`, `to`, `includeClosed` hanya ke daftar ini; `closureClearanceStatusKey` |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-monitoring.jsx` | Penormal, pesan gagal "Data gagal dimuat.", query per daftar, jumlah tab mengikuti saringan baru |
| `src/components/view/health-services/inpatient-management/inpatient-monitoring-table-columns.jsx` | Kolom daftar baru; kolom "Kasir Saat Penutupan"; label "Tanpa Izin Kasir" |
| `src/components/view/health-services/inpatient-management/inpatient-monitoring-view.jsx` | Saringan daftar baru; saringan klien status kasir; tombol **Coba lagi** pada keadaan gagal |
| `monitoring/components/monitoring-filter.jsx` | Dua `FilterDatePicker` dan satu `FilterSelect` khusus daftar baru |
| `monitoring/components/monitoring-tabs.jsx`, `monitoring-status-badge.jsx`, `monitoring-summary-cards.jsx` | Label tab dan lencana |
| `tests/unit/inpatient-monitoring.test.mjs` | Daftar kunci dan pemeriksaan keterlambatan diperbarui; tes yang gagal sejak garis dasar kini lulus |
| `tests/unit/inpatient-integration-billing-finishing.test.mjs` | Tiga tes `FE-RWI-169` |

### 3.3 Kepatuhan arsitektur frontend

Daftar baru memakai jalur yang sama dengan daftar lain: konstanta → hook → `inpatientMonitoringService`
→ penormal di utils → kolom di berkas kolom terpisah → `MonitoringTable`. Badge kolom kasir memakai
badge `FE-INT-06`.

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Daftar baru | tab, `MonitoringTable`, berkas kolom | `monitoring-tabs.jsx`, `inpatient-monitoring-table-columns.jsx` | REUSE | — |
| Saringan Periode | `FilterDatePicker` | katalog | REUSE | — |
| "Termasuk episode sudah ditutup" | `FilterSelect` dua opsi | katalog | REUSE | Seragam dengan bar filter; sketsa memakai checkbox, bentuknya `DEV_DISCRETION` |

`UI GATE: 3 elemen — REUSE 3, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Mengambil daftar pasien yang pulang sebelum izin kasir..." |
| Kosong | "Tidak ada pasien yang pulang sebelum izin kasir pada saringan ini." |
| Gagal | "Data gagal dimuat." beserta tombol **Coba lagi** |
| Tanpa hak akses | Pesan akses ditolak dari `AccessDeniedGate` |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Monitoring

Base URL: `api/v1/health-services/inpatient-management/monitoring`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/departures-before-clearance` | Daftar pulang sebelum izin kasir; query `ServiceUnitId`, `From`, `To`, `IncludeClosed`, paginasi | `InpatientMonitoring : Read` |
| `GET` | `/closures-without-financial-clearance` | Daftar penutupan tanpa izin kasir beserta `ClosureClearanceObserved` | `InpatientMonitoring : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run test:unit` sesudah implementasi | 2174 tes, 2167 lulus, 7 gagal — seluruhnya garis dasar di luar task | `PASS` | Kegagalan "keempat daftar pantau" dari garis dasar ikut lulus |
| `node --import ./tests/helpers/register.mjs --test` tujuh berkas tes task | 113 dari 113 lulus | `PASS` | Termasuk "kolom kasir saat keluar dan kasir sekarang dibaca terpisah" |
| `npm run lint:errors` sesudah implementasi | exit 0, nol error | `PASS` | Keluaran perintah |
| `npm run build` final | `✓ Compiled successfully in 64s`, exit 0 | `PASS` | Keluaran perintah |
| Uji manual dengan data episode uji | Tidak dijalankan | `NOT RUN` | Dikecualikan |

`AUTOMATED TEST: npm run test:unit — PASS untuk tes task (7 kegagalan tersisa = garis dasar di luar task); tujuh berkas tes task 113/113 PASS`

`MANUAL TEST: NOT FEASIBLE — butuh episode uji yang keluar dengan status kasir belum disetujui, lalu disetujui kasir; tidak tersedia alat peramban maupun akun uji di sesi ini. Dikecualikan atas keputusan pengguna 6 Oktober 2026.`

Uji manual: `NOT FEASIBLE`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Episode keluar dengan `PENDING` muncul dengan "Menunggu kasir" pada kolom saat keluar | Terpenuhi | `departureStatusKey` dari angka `2`; kolom "Kasir Saat Keluar" |
| 2. Sesudah kasir menyetujui, "Kasir sekarang" berubah dan kolom saat keluar tetap | Terpenuhi | Dua kunci dibaca dari field berbeda; tes "dibaca terpisah" |
| 3. Keadaan memuat, kosong, dan gagal sesuai `FE-INT-03` | Terpenuhi | Konstanta daftar dan tombol **Coba lagi** |
| 4. Tanpa `InpatientMonitoring : Read` daftar tidak tampil | Terpenuhi | `AccessDeniedGate` menutup halaman pada 403 |
| 5. Tidak ada rupiah | Terpenuhi | Penormal hanya membaca field tanpa nominal |
| DoD: lint dan build lulus; laporan tracked | Terpenuhi | Bagian 6 |
| DoD: uji manual | Dikecualikan | Keputusan pengguna 6 Oktober 2026 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kontrak API 3.5 mencantumkan `ServiceUnitName` dan `RecordedByUserName`, tetapi DTO backend saat ini belum mengirim keduanya; kolom **Unit** dan **Dicatat Oleh** tampil "-" sampai backend menambahkannya. Field jejak bernama `DepartureClearanceObserved` di DTO, berbeda dari `ClearanceObservedAtDeparture` di kontrak; penormal membaca keduanya |
| Masalah yang diketahui | Delta DTO di atas perlu ditindaklanjuti backend; tidak menahan kriteria |
| Dependency backend | `BE-RWI-153` 🟡 |
| Perubahan sampingan | Tombol **Coba lagi** berlaku untuk semua daftar di tab Daftar Pantau, bukan hanya daftar baru |
| Interupsi | Pengguna meng-commit dan me-merge branch di tengah pekerjaan |
| Status Git | Implementasi di commit pengguna `f6eca4ef7`; tidak ada perubahan berkas task sesudahnya |
| Langkah berikutnya | Backend menambah `ServiceUnitName` dan `RecordedByUserName` pada `DepartureBeforeClearanceItem` sesuai API 3.5 |
