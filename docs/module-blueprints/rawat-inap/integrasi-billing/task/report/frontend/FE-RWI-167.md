# Laporan Perubahan Frontend — `FE-RWI-167`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-167` |
| Judul | Keluar ruangan dengan peringatan kasir |
| Slice | `MVP-0` / `RWF-W0` — gelombang eksekusi 2 roadmap frontend Finishing |
| Roadmap | [`frontend-roadmap-finishing.md`](../../../roadmap/frontend-roadmap-finishing.md) — kartu `FE-RWI-167` |
| Trace | `FR-RWF-006`, `FR-RWF-007`; `RWI-DEC-186`, `RWI-DEC-187`; `AC-RWF-006`, `AC-RWF-007` |
| Contract version | `integrasi-billing` `1.1.0` `approved` 2 Oktober 2026 (`RWI-DEC-221`); frontend 6.3 `FE-INT-01`; API 3.2 (`record-departure`, `RecordDepartureRequest`, `InpatientDepartureResponse`, 409 `INP-DEP-001`) |
| Wewenang UI | Bentuk dialog dan warna badge `DEV_DISCRETION`. Mengikat: peringatan kasir dengan pengakuan sekali klik, tanpa PIN, tanpa isian alasan |
| Dependency | `FE-RWI-166` ✅ (sesi yang sama); `BE-RWI-153` [BE] 🟡 — source selesai dan build PASS, alur klinis lengkap belum UAT |
| Klasifikasi | `MEDIUM` — skor 6: berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika sedang (1), memakai kontrak yang ada (1), hak akses terkait (1), satu dialog (1) |
| Task mode | `FRONTEND` — backend strict read-only |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `tests/unit/**`; laporan, roadmap, traceability `integrasi-billing` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | Mulai di `8740efa02`; implementasi ikut commit pengguna `f6eca4ef7` (6 Oktober 2026 10.48), lalu merge pengguna `eb0a0a790` |
| Commit backend yang dijadikan rujukan | `10d3e7ee` |
| Tanggal | 6 Oktober 2026 |
| Status | ✅ **SELESAI.** Ketujuh acceptance criteria terpetakan ke source. Butir DoD uji manual **dikecualikan atas keputusan pengguna 6 Oktober 2026** |

**Otorisasi eksekusi.** Roadmap berstatus `DRAFT`; pengguna memerintahkan seluruh task dikerjakan
sampai selesai pada 5 Oktober 2026. Task ini berjalan walau `BE-RWI-153` masih 🟡, karena source dan
endpoint backend-nya sudah ada; yang belum hanya UAT klinis backend.

---

## 1. Keadaan yang ditemukan di awal

| Temuan | Bukti | Akibat |
| --- | --- | --- |
| Pencatatan kepergian mengirim `{ departedAt, note }` tanpa `ClearanceWarningAcknowledged` | `buildDeparturePayload` lama | Setiap pasien yang belum diizinkan kasir ditolak 409 tanpa jalan lanjut dari layar |
| Jawaban `record-departure` diperlakukan sebagai detail episode | `applyEpisode(data)` di `use-inpatient-episode-detail.jsx` | Jawaban sebenarnya `InpatientDepartureResponse`; detail episode di layar sempat tertimpa bentuk yang salah |
| Ada isian "Catatan Kepergian" | `DEPARTURE_NOTE_FIELD` di Detail Episode | `RecordDepartureRequest` kontrak `1.1.0` tidak punya catatan, dan `RWI-DEC-187` melarang isian alasan |
| Kartu gerbang lama dan dua modal memanggil endpoint yang dihapus | `discharge-clearance-gate-card.jsx`, `supervisor-override-modal.jsx`, `physical-discharge-confirm-modal.jsx` | Tombol pulang fisik dan override darurat gagal 404 |
| Tombol kepergian hanya disembunyikan dari dokter berdasarkan nama peran | `resolveDepartureAuthority` | Kriteria 6 meminta penjaga `InpatientDischarge : RecordDeparture` |

---

## 2. Proses bisnis dari sisi pengguna

**Tujuan.** Pasien yang sudah meninggalkan ruangan langsung tercatat dan bednya kosong, walau
kasir belum memberi izin. Kasir tidak menahan kepergian (`RWI-DEC-186`); yang dijamin hanyalah
petugas sadar akan peringatan kasir.

**Pelaku.** Petugas admisi, perawat pelaksana, kepala ruangan, supervisor — pemegang
`InpatientDischarge : RecordDeparture`.

**Prasyarat.** DPJP sudah menyatakan pasien boleh pulang (episode `DischargePending`).

**Langkah.**

1. Petugas membuka Detail Episode, bagian **Kepergian Pasien**.
2. Waktu keluar boleh dikosongkan (berarti sekarang) atau diisi.
3. Petugas menekan **Catat pasien meninggalkan ruangan**. Layar membaca ulang detail episode.
4. Dialog tampil: nama pasien, lokasi, waktu boleh pulang, waktu keluar, dan badge status kasir yang
   dibaca langsung dari Billing. Bila status bukan "Disetujui kasir", peringatan dan daftar kendala
   ikut tampil.
5. Petugas menekan **Catat keluar ruangan**. Kiriman pertama selalu tanpa pengakuan.
6. Bila kasir sudah memberi izin, kepergian langsung tercatat.
7. Bila belum, server menjawab 409 `INP-DEP-001` tanpa mengubah apa pun. Dialog tetap terbuka,
   peringatan server tampil, dan tombol berganti menjadi **Tetap catat keluar ruangan**.
8. Satu klik pada tombol itu mengirim ulang dengan pengakuan. Tidak ada PIN, password, maupun alasan.

**Hasil.** Contoh: "Pasien tercatat keluar pukul 09.05. Bed sudah kosong." ditambah jumlah pemakaian
alat yang ikut ditutup dan peringatan tindak lanjut bila ada. Episode tetap `DischargePending` dan
masih perlu ditutup.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Billing mati | Dialog berbunyi "Status kasir tidak dapat dibaca"; kepergian tetap dapat dicatat dengan pengakuan, dan episode masuk daftar "Pulang sebelum izin kasir" |
| Petugas lain sudah mencatat kepergian | 409 tanpa kode `INP-DEP-001`; pesan server tampil apa adanya dan detail dimuat ulang |
| Klik ganda | Pengiriman kedua tertahan penjaga `departureInFlight`; tombol nonaktif selama permintaan berjalan |
| Tanpa `RecordDeparture` | Bagian kepergian tidak dirender |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`InpatientDischargeController.cs`, `InpDischargeService.Departure.cs`, `InpatientClosureDtos.cs`
(`RecordDepartureRequest`), `InpatientDepartureDtos.cs`, `InpClearanceObservation.cs`,
`ApiResponse.cs`; frontend `use-inpatient-episode-detail.jsx`, `inpatient-episode-detail-view.jsx`,
`inpatient-departure-utils.jsx`, `inpatient-departure-constants.jsx`, `confirm-modal.jsx`,
`inpatient-setting-utils.jsx`, `02-module-map.md` (`FE-INP-14` adalah bagian Detail Episode).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/health-services/inpatient-management/inpatient-departure-utils.jsx` | Payload menjadi `{ departedAt?, clearanceWarningAcknowledged }`; `resolveDepartureAuthority` menerima `canRecordDeparture` |
| `src/lib/constants/health-services/inpatient-management/inpatient-departure-constants.jsx` | Batas isian catatan dicabut |
| `src/utils/health-services/inpatient-management/inpatient-billing-status-utils.js` | `isDepartureClearanceWarning`, `readApiErrorCode`, `describeDepartureResult`, `requiresDepartureWarning` |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-episode-detail.jsx` | Alur dua langkah dengan `departureWarning`; jawaban dibaca sebagai `InpatientDepartureResponse`; penjaga `usePermission("InpatientDischarge", "RecordDeparture")` |
| `src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx` | Isian catatan dicabut; dialog `FE-INT-01` dengan badge, kendala, peringatan, dan label tombol yang berganti; hasil keluar ruangan tampil di bagian kepergian |
| `src/style/health-services/inpatient-management/inpatient-episode-detail.module.css` | Tiga kelas konteks dialog, memakai token |
| `src/lib/services/health-services/inpatient-management/inpatient-billing.service.js` | `submitSupervisorOverride` dan `confirmPhysicalDischarge` dicabut (bersama `FE-RWI-166`) |
| **Dihapus** | `supervisor-override-modal.jsx`, `physical-discharge-confirm-modal.jsx`, alias PascalCase-nya, dan CSS Module-nya |
| `tests/unit/inpatient-departure.test.mjs` | Diperbarui ke payload dan dialog kontrak `1.1.0` |
| `tests/unit/inpatient-integration-billing-finishing.test.mjs` | **Baru**; enam tes `FE-RWI-167` |

### 3.3 Kepatuhan arsitektur frontend

Hook tetap satu-satunya pemanggil service; view hanya merangkai `ConfirmModal`, `BaseTextField`,
`InformationAlert`, dan badge status kasir. Pengenalan kode 409 berada di utils murni.

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Dialog keluar ruangan | `ConfirmModal` + badge `FE-INT-06` + `InformationAlert` | `ConfirmModal` sudah dipakai dialog lain di layar yang sama | COMPOSE | Satu dialog; label tombol berganti sesudah 409 |
| Tombol catat | `BaseButton` | katalog | REUSE | — |

`UI GATE: 2 elemen — REUSE 1, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0`

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tombol "Memeriksa..." saat detail dibaca ulang; badge "Memeriksa status kasir..." di dialog |
| Kosong | Tidak berlaku — dialog selalu menampilkan pasien yang sedang dibuka |
| Gagal | Pesan server di dialog atau di bagian kepergian; detail dimuat ulang |
| Tanpa hak akses | Bagian kepergian tidak dirender bagi pengguna tanpa `RecordDeparture` maupun dokter non-supervisor |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Discharge

Base URL: `api/v1/health-services/inpatient-management/discharges`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{episodeId}/record-departure` | Mencatat pasien meninggalkan ruangan; 409 `INP-DEP-001` memicu peringatan | `InpatientDischarge : RecordDeparture` |

#### Inpatient Billing Operational

Base URL: `api/v1/health-services/inpatient-management/episodes`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/{episodeId}/billing-status` | Badge dan kendala di dialog | `InpatientBillingOperational : Read` |

#### Health Services / Inpatient Management / Inpatient Episode

Base URL: `api/v1/health-services/inpatient-management/episodes`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/{episodeId}` | Membaca ulang detail sebelum dialog dibuka | `InpatientEpisode : Read` |

Kode status yang ditangani: `200` tercatat; `400` waktu tidak sah; `409` `INP-DEP-001` peringatan
kasir; `409` tanpa kode berarti sudah dicatat; `422` episode belum `DischargePending`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run test:unit` sesudah implementasi | 2174 tes, 2167 lulus, 7 gagal — seluruhnya garis dasar di luar task | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test` tujuh berkas tes task | 113 dari 113 lulus | `PASS` | Termasuk "kiriman pertama tanpa pengakuan", "hanya 409 INP-DEP-001 yang meminta pengakuan", "nol pemanggil supervisor-override" |
| `npm run lint:errors` sesudah implementasi | exit 0, nol error | `PASS` | Keluaran perintah |
| `npm run build` final | `✓ Compiled successfully in 64s`, exit 0 | `PASS` | Keluaran perintah |
| Grep anti-regresi checklist UI | Nol temuan | `PASS` | Keluaran perintah |
| Uji manual `PENDING`, `CLEARED`, Billing mati | Tidak dijalankan | `NOT RUN` | Dikecualikan |

`AUTOMATED TEST: npm run test:unit — PASS untuk tes task (7 kegagalan tersisa = garis dasar di luar task); tujuh berkas tes task 113/113 PASS`

`MANUAL TEST: NOT FEASIBLE — tidak ada alat kendali peramban dan akun uji di sesi ini; skenario butuh status kasir nyata dan Billing yang dimatikan. Dikecualikan atas keputusan pengguna 6 Oktober 2026.`

Uji manual: `NOT FEASIBLE`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `PENDING`: klik pertama menampilkan peringatan dan kendala tanpa rupiah; "Tetap catat keluar ruangan" berhasil, bed kosong | Terpenuhi | Dialog `departure-dialog`; `departureWarning` mengganti label tombol; pengiriman kedua `clearanceWarningAcknowledged: true` |
| 2. `CLEARED`: tercatat tanpa peringatan | Terpenuhi | `requiresDepartureWarning(CLEARED) = false`; server menjawab 200 pada kiriman pertama |
| 3. Billing mati: peringatan "Status kasir tidak dapat dibaca", tetap dapat dicatat dengan pengakuan | Terpenuhi | Judul peringatan `UNREADABLE_WARNING`; alur pengakuan yang sama |
| 4. Tidak ada isian PIN, password, maupun alasan | Terpenuhi | Isian catatan dicabut; tes payload tanpa `pin`, `password`, `reason`, `note` |
| 5. Tombol nonaktif selama permintaan berjalan; klik ganda tidak mengirim dua kali; 409 "sudah dicatat" tampil apa adanya | Terpenuhi | `departureInFlight`, `ConfirmModal loading`; `isDepartureClearanceWarning` hanya untuk `INP-DEP-001` |
| 6. Tombol hanya tampil bagi `InpatientDischarge : RecordDeparture` | Terpenuhi | `canRecordDeparture` bentuk ketat; tes "dijaga butir RecordDeparture" |
| 7. Pencarian kode: nol pemanggil `supervisor-override` dan `confirm-physical-discharge` | Terpenuhi | Tes "nol pemanggil" atas service, hook, dan kedua view |
| DoD: lint dan build lulus; laporan tracked | Terpenuhi | Bagian 6 |
| DoD: uji manual | Dikecualikan | Keputusan pengguna 6 Oktober 2026 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `createToast` modul rawat inap menulis `type`, sedangkan `ToastStack` membaca `variant`; semua toast modul ini tampil bernada info. Perilaku bawaan, di luar cakupan |
| Masalah yang diketahui | Backend masih menerima `Note` pada `RecordDepartureRequest`; layar mengikuti kontrak dan tidak mengirimnya |
| Dependency backend | `BE-RWI-153` 🟡 — alur keluar ruangan backend belum UAT. Gerbang "satu jendela rilis" bersama `BE-RWI-146`, `152`, `153` tetap berlaku |
| Perubahan sampingan | `NONE` di luar pencabutan yang diminta kartu |
| Interupsi | Pengguna meng-commit dan me-merge branch di tengah pekerjaan; pekerjaan dilanjutkan dari keadaan terverifikasi |
| Status Git | Implementasi berada di commit pengguna `f6eca4ef7`; berkas task tidak berubah lagi sesudahnya kecuali tes `inpatient-integration-billing-finishing.test.mjs` (` M`) |
| Langkah berikutnya | Uji di peramban dengan tiga keadaan kasir: `PENDING`, `CLEARED`, dan Billing dimatikan |
