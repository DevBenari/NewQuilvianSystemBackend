# Laporan Perubahan Frontend — `FE-RWI-170`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-170` |
| Judul | Koreksi penempatan |
| Slice | `MVP-1` / `RWF-W1` — gelombang eksekusi 1 roadmap frontend Finishing |
| Roadmap | [`frontend-roadmap-finishing.md`](../../../roadmap/frontend-roadmap-finishing.md) — kartu `FE-RWI-170` |
| Trace | `FR-RWF-019`; `RWI-DEC-157`, `RWI-DEC-192` butir (g); `UAT-RWF-25` |
| Contract version | `integrasi-billing` `1.1.0` `approved` 2 Oktober 2026 (`RWI-DEC-221`); frontend 6.3 `FE-INT-04`; API 3.4 (`CorrectPlacementRequest`, kode `INP-COR-001` s.d. `004`) |
| Wewenang UI | Bentuk dialog `DEV_DISCRETION`; teks dialog wajib menjelaskan beda koreksi dan perpindahan (risiko roadmap) |
| Dependency | `BE-RWI-154` [BE] 🟡 — endpoint koreksi tersedia, uji `IsSuperseded` dengan Billing nyata belum UAT |
| Klasifikasi | `MEDIUM` — skor 6: berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika sedang (1), memakai kontrak yang ada (1), hak akses terkait (1), satu workflow (1) |
| Task mode | `FRONTEND` — backend strict read-only |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `tests/unit/**`; laporan, roadmap, traceability `integrasi-billing` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | Mulai di `8740efa02`; implementasi ikut commit pengguna `f6eca4ef7`, lalu merge pengguna `eb0a0a790` |
| Commit backend yang dijadikan rujukan | `10d3e7ee` |
| Tanggal | 6 Oktober 2026 |
| Status | 🟡 **SEBAGIAN.** Kriteria 1, 2, 4, dan 5 terpetakan ke source; kriteria 3 baru sebagian — baris koreksi dan baris lama tercoret sudah tampil, tetapi **alasan koreksi tidak dapat ditampilkan di riwayat** karena `GET placements/by-episode` tidak mengirimnya. Menunggu backend menambah alasan koreksi ke `BedPlacementResponse` (keputusan pengguna 6 Oktober 2026) |

**Otorisasi eksekusi.** Roadmap berstatus `DRAFT`; pengguna memerintahkan seluruh task dikerjakan
sampai selesai pada 5 Oktober 2026. Pada 6 Oktober 2026 pengguna memutuskan task ini ditandai 🟡 dan
celah backend dicatat, alih-alih memberi wewenang tulis backend.

---

## 1. Keadaan yang ditemukan di awal

- Riwayat penempatan di Detail Episode belum membedakan transfer dari koreksi, dan belum punya tombol
  Koreksi.
- `normalizePlacements` belum membaca `Version`, `CorrectsPlacementId`, `SupersededByCorrectionId`,
  dan `IsCorrection` yang sudah dikirim backend.
- `resolveCurrentLocation` dapat menganggap baris yang sudah dikoreksi sebagai lokasi sekarang.
- **Celah kontrak.** Frontend 6.3 `FE-INT-04` meminta baris lama tercoret "beserta alasan", tetapi
  API 3.4 dan `BedPlacementResponse` tidak membawa alasan koreksi. Backend menyimpannya di kolom
  `InpBedPlacement.ChangeReason` dan hanya membukanya lewat Laporan Transfer Ruangan, yang tidak dapat
  disaring per episode dan dijaga hak akses lain (`InpatientReport : ReadRoomTransfer`). Layar tidak
  menebak field yang belum ada di kontrak.

---

## 2. Proses bisnis dari sisi pengguna

**Tujuan.** Salah catat kamar, bed, kelas, atau waktu dibetulkan tanpa menagih tarif kamar dua kali.

**Pelaku.** Kepala ruangan dan petugas admisi — pemegang `InpatientBedOccupancy : Correct`.

**Langkah.**

1. Petugas membuka Detail Episode, bagian Riwayat, kelompok **Riwayat Penempatan**.
2. Pada baris yang masih berlaku, petugas menekan **Koreksi**.
3. Dialog menjelaskan bahwa koreksi bukan perpindahan pasien, lalu menyediakan isian **Bed yang Benar**,
   **Kelas yang Benar**, **Waktu Mulai yang Benar**, **Waktu Selesai yang Benar** (hanya untuk penempatan
   yang sudah berakhir), dan **Alasan Koreksi** yang wajib.
4. Petugas menekan **Simpan Koreksi**. Layar memeriksa isian lebih dulu, lalu mengirim koreksi bersama
   versi baris yang terbaca.
5. Riwayat dimuat ulang: baris baru bertanda **Koreksi**, baris lama tercoret bertanda **Dikoreksi**
   dengan keterangan "Baris ini sudah dikoreksi dan tidak lagi ikut tarif kamar."

**Contoh.** Pasien tercatat masuk kelas 1 sejak 1 Oktober pukul 08.00, padahal sejak awal menempati VIP.
Kepala ruangan memilih kelas VIP dan menulis alasan "Salah pilih kelas saat admisi". Baris kelas 1
tetap tersimpan tetapi tercoret, dan Billing menghitung seluruh periode sebagai VIP.

**Aturan isian (diperiksa sebelum dikirim).**

| Isian | Aturan | Contoh ditolak |
| --- | --- | --- |
| Alasan | Wajib, memuat huruf atau angka | "..." |
| Isian koreksi | Minimal satu dari bed, kelas, waktu mulai, waktu selesai | Hanya alasan, tanpa isian lain |
| Waktu | Tidak boleh melewati sekarang | Mulai 1 Desember 2026 saat ini 6 Oktober |
| Waktu selesai | Hanya untuk penempatan yang sudah berakhir, dan harus sesudah waktu mulai | Selesai 30 September untuk penempatan mulai 1 Oktober |

**Jalur tidak normal.**

| Keadaan | Yang dilihat petugas |
| --- | --- |
| Invoice rawat inap sudah final (422 `INP-COR-001`) | "Tagihan sudah difinalkan, hubungi kasir." |
| Status tagihan tidak terbaca (422 `INP-COR-002`) | "Status tagihan tidak dapat dibaca, sehingga koreksi belum dapat disimpan. Coba lagi beberapa saat." |
| Dua petugas mengoreksi baris yang sama (409 `INP-COR-003`) | Yang kalah melihat pesan "Data penempatan sudah diubah pengguna lain…", dialog ditutup, riwayat dimuat ulang |
| Bed tujuan tidak layak (422 `INP-COR-004`) | "Tempat tidur yang dipilih tidak lolos kelayakan penempatan." |
| Tanpa `Correct` | Tombol Koreksi tidak dirender |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`InpatientBedOccupancyController.cs`, `InpPlacementCorrectionService.cs`,
`InpBedOccupancyService.cs` (`ApplyPlacementCorrectionAsync`, `GetPlacementsByEpisodeAsync`),
`InpatientBedOccupancyDtos.cs`, `InpatientCorrectionDtos.cs`, `InpatientReportController.cs`,
`InpatientReportDtos.cs`; frontend `inpatient-episode-detail-view.jsx`,
`use-inpatient-episode-detail.jsx`, `inpatient-episode-utils.jsx`, `bed-occupancy.service.js`,
`health-service-select-resources.js` (`beds`, `patientClasses`), `confirm-modal.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/inpatient-management/inpatient-placement-correction-constants.js` | **Baru.** Batas alasan, kode `INP-COR-*`, kalimat baku, penjelasan beda koreksi dan perpindahan |
| `src/utils/health-services/inpatient-management/inpatient-placement-correction-utils.js` | **Baru.** Validasi, payload `CorrectPlacementRequest`, pemetaan penolakan |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-placement-correction.jsx` | **Baru.** Editor koreksi; `usePermission("InpatientBedOccupancy", "Correct")` bentuk ketat; penjaga pengiriman ganda |
| `src/utils/health-services/inpatient-management/inpatient-episode-utils.jsx` | `normalizePlacements` membaca empat field koreksi; `resolveCurrentLocation` melewati baris yang sudah dikoreksi |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-episode-detail.jsx` | Mengekspos `addToast` untuk editor koreksi |
| `src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx` | Tanda Koreksi/Dikoreksi, baris tercoret, tombol Koreksi, dialog koreksi |
| `src/style/health-services/inpatient-management/inpatient-episode-detail.module.css` | Kelas `timelineSuperseded` dan `placementActions`, memakai token |
| `tests/unit/inpatient-integration-billing-finishing.test.mjs` | Enam tes `FE-RWI-170` |

### 3.3 Kepatuhan arsitektur frontend

Hook editor terpisah dari hook detail supaya fokusnya jelas; validasi dan payload di utils murni;
view merangkai `ConfirmModal`, `ResourceFilterSelect`, `BaseTextField`, `BaseTextAreaField`,
`InformationAlert`, dan `StatusBadge`.

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Tombol Koreksi | `BaseButton` | katalog | REUSE | — |
| Dialog koreksi | `ConfirmModal` + `ResourceFilterSelect` + field form | registry select memuat `beds` dan `patientClasses` | COMPOSE | — |
| Baris koreksi dan baris lama tercoret | `StatusBadge` + kelas CSS Module | — | COMPOSE | — |

`UI GATE: 3 elemen — REUSE 1, EXTEND 0, COMPOSE 2, WRAP 0, NEW 0`

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tombol Simpan Koreksi dalam keadaan memproses; isian dinonaktifkan |
| Kosong | "Belum ada penempatan yang tercatat." |
| Gagal | Kalimat penolakan di dalam dialog; 409 menutup dialog dan memuat ulang riwayat |
| Tanpa hak akses | Tombol Koreksi tidak dirender |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Bed Occupancy

Base URL: `api/v1/health-services/inpatient-management/bed-occupancies`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/placements/{placementId}/corrections` | Menyimpan koreksi dengan `ExpectedVersion` | `InpatientBedOccupancy : Correct` |
| `GET` | `/placements/by-episode/{episodeId}` | Riwayat penempatan beserta `IsCorrection` dan `SupersededByCorrectionId` | `InpatientBedOccupancy : Read` |

Pilihan bed dan kelas memakai endpoint `options` master data yang sudah ada (`beds`,
`patientClasses`); kelayakan bed tetap diperiksa server (`INP-COR-004`).

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run test:unit` sesudah implementasi | 2174 tes, 2167 lulus, 7 gagal — seluruhnya garis dasar di luar task | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test` tujuh berkas tes task | 113 dari 113 lulus | `PASS` | Termasuk validasi isian, payload, pemetaan penolakan, baris tercoret |
| `npm run lint:errors` sesudah implementasi | exit 0, nol error | `PASS` | Keluaran perintah |
| `npm run build` final | `✓ Compiled successfully in 64s`, exit 0 | `PASS` | Keluaran perintah |
| Uji manual skenario `UAT-RWF-25` | Tidak dijalankan | `NOT RUN` | Dikecualikan |

`AUTOMATED TEST: npm run test:unit — PASS untuk tes task (7 kegagalan tersisa = garis dasar di luar task); tujuh berkas tes task 113/113 PASS`

`MANUAL TEST: NOT FEASIBLE — butuh episode uji dengan invoice OPEN dan FINAL serta dua akun bersamaan; tidak tersedia di sesi ini. Dikecualikan atas keputusan pengguna 6 Oktober 2026.`

Uji manual: `NOT FEASIBLE`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Tombol hanya tampil bagi `InpatientBedOccupancy : Correct` | Terpenuhi | `canCorrect = loaded && allowed`; tes "dijaga butir Correct" |
| 2. Alasan kosong atau tanpa satu pun field koreksi ditolak di layar sebelum dikirim | Terpenuhi | `validatePlacementCorrectionForm`; tes kriteria 2 |
| 3. Sesudah berhasil, riwayat menampilkan baris koreksi dan baris lama tercoret beserta alasan | **Belum terpenuhi penuh** | Baris koreksi dan baris lama tercoret sudah tampil. **Alasan** hanya tampil pada notifikasi saat koreksi disimpan; riwayat tidak dapat menampilkannya karena `BedPlacementResponse` tidak membawa `ChangeReason` |
| 4. Invoice final → "Tagihan sudah difinalkan, hubungi kasir" | Terpenuhi | Pemetaan `INP-COR-001` |
| 5. Dua pengguna bersamaan → yang kalah melihat pesan dan riwayat dimuat ulang | Terpenuhi | `shouldReloadAfterCorrectionFailure`; dialog ditutup dan `onCorrected` memuat ulang |
| DoD: lint dan build lulus; laporan tracked | Terpenuhi | Bagian 6 |
| DoD: uji manual `UAT-RWF-25` | Dikecualikan | Keputusan pengguna 6 Oktober 2026 |

**4 dari 5 kriteria terpenuhi.** Kriteria 3 adalah kekurangan source di backend dan tidak dapat
dikecualikan.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Daftar bed pada dialog memuat semua bed aktif; kelayakan penempatan dinilai server saat simpan |
| Masalah yang diketahui | Alasan koreksi tidak terbaca di riwayat penempatan (kriteria 3) |
| Dependency backend | Butuh perubahan backend: `ChangeReason` ditambahkan ke `BedPlacementResponse` dan ke query `GetPlacementsByEpisodeAsync`, lalu disebut pada kontrak API 3.4. Sesudah itu frontend cukup membaca field tersebut di `normalizePlacements` dan menampilkannya di baris koreksi. `BE-RWI-154` sendiri masih 🟡 |
| Perubahan sampingan | `NONE` |
| Interupsi | Pengguna meng-commit dan me-merge branch di tengah pekerjaan |
| Status Git | Implementasi di commit pengguna `f6eca4ef7`; tidak ada perubahan berkas task sesudahnya selain tes gabungan |
| Langkah berikutnya | `plan-module-delivery` menambah task backend kecil untuk membuka `ChangeReason` pada riwayat penempatan, lalu task ini diselesaikan dengan satu perubahan baca di frontend |
