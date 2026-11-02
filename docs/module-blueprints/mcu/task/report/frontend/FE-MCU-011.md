# Laporan Perubahan Frontend — `FE-MCU-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-MCU-011` |
| Judul | Layar pemeriksaan |
| Slice | `MVP-2` (pasangan `BE-MCU-020`..`025`) |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian *`FE-MCU-011`* |
| Trace | `FR-MCU-030`..`037`; `UAT-09`..`13`; brief layar `05-screen-brief.md` §5.5–5.6, §6 |
| Contract version | `MCU-API-v1` grup `Mcu Examination` dan `PATCH orders/{id}/cancel` (batal penuh) — `approved` 2026-10-21; kalimat `VAL-MCU-06`, `34`, `36`, `38`, `40`..`44`, `46` dibaca dari `McuExaminationService` |
| Wewenang UI | Brief layar `MCU-FE-002` (approved): tab Pemeriksaan, route `…/components/[componentId]/hasil`, modal Mulai/Tambah/Batal/Jadwal Ulang; komponen dasar yang ada (`DataTable`, `ConfirmModal`, `BaseSelectField`, `RowActionMenu`) dalam rentang `DEV_DISCRETION` §7 |
| Dependency | `FE-MCU-007` ✅, `FE-MCU-010` ✅; backend `BE-MCU-021`..`025` ✅ melayani endpoint |
| Klasifikasi | `HIGH` — satu repository, berkas diperiksa > 20, berkas diubah 9–20, logika sedang–tinggi (aksi menurut status + izin + dokter yang ditetapkan, data basi), kontrak yang ada, tanpa database, keamanan berkaitan (`VAL-MCU-38`), UI baru |
| Task mode | `FRONTEND` |
| Target tulis | Frontend: konstanta, aturan, hook, view, route, uji. Backend: laporan ini; tanda roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `f7f60e34c` (branch `YogaV2`) |
| Commit backend yang dijadikan rujukan | `1b7ff563` (branch `yoga`) |
| Tanggal | 2026-10-27 |
| Status | Selesai — `UAT-09` terbukti terhadap backend lokal tanpa tulis; `UAT-10`..`13` dan isian `6.4` terbukti di E2E API-tiruan (batas verifikasi atas keputusan pemilik: UAT tanpa tulis) |

---

## 1. Keadaan yang ditemukan di awal

- Detail order (`FE-MCU-007`) sudah punya header aksi `FE-MCU-001`. Aksi *Mulai Pemeriksaan* dan *Batal Penuh* masih
  tampil nonaktif *Fitur belum tersedia*, dan tab Pemeriksaan masih berisi keterangan yang sama.
- Lapisan data `mcuExamination` (`FE-MCU-010`) sudah tersedia.
- `isAssignedDoctor` belum pernah diisi. Sesi login menyimpan `doctorId` akun (`store.auth.userInfo.doctorId`), dan
  backend membaca dokter akun dengan cara yang sama (`ResolveActorDoctorIdAsync`: `Users.DoctorId` atau klaim
  `doctor_id`).
- Aturan server yang menentukan tombol:

  | Aksi | Status order | Status komponen | Dokter yang ditetapkan? |
  | --- | --- | --- | --- |
  | Tambah komponen | *Dalam Pemeriksaan*, *Menunggu Resume* | — | Ya (`VAL-MCU-38`) |
  | Batal komponen | *Dalam Pemeriksaan*, *Menunggu Resume* | Bukan *Selesai*/*Dibatalkan* | Ya |
  | Jadwal ulang komponen | *Hadir*, *Dalam Pemeriksaan* | Bukan *Dipesan*/*Selesai*/*Dibatalkan* | Tidak (`McuExamination : Update`) |
  | Isi hasil | *Dalam Pemeriksaan*, *Menunggu Resume* | *Siap diisi*/*Selesai* | Tidak; skrining wajib selesai (`VAL-MCU-36`) |
  | Muat ulang status | *Hadir*, *Dalam Pemeriksaan* (status lain: jawaban tanpa sinkron) | — | — |

  Batal penuh memakai `PATCH orders/{id}/cancel` yang sama dengan batal sebelum datang.
- Isian Ya/Tidak diterima server sebagai teks `Ya`/`Tidak`. Angka dinormalkan `6,4` → `6.4` (`TryNormalizeNumber`).
- `McuOrderComponentResponse` **tidak** memuat harga atau nilai pengembalian per komponen (lihat §8, delta brief).

---

## 2. Proses bisnis dari sisi pengguna

1. **Mulai Pemeriksaan** — order *Hadir* menampilkan tombol utama *Mulai Pemeriksaan*. Konfirmasinya menyebut paket,
   penjamin, dan harga.
   - Bila tagihan belum lunas, server menolak dan pesannya tampil apa adanya di modal, mis. *"Tagihan paket belum
     lunas (sisa Rp 350.000)."* (`UAT-09`).
   - Bila berhasil, status menjadi *Dalam Pemeriksaan*, tab Pemeriksaan terbuka, dan nomor pesanan Lab/Rad paket
     tampil per komponen (`UAT-10`).
2. **Tab Pemeriksaan** — *Kelengkapan: n dari m tuntas* (komponen batal tidak dihitung), dan tabel Komponen · Jenis ·
   Cakupan (*Paket*/*Di luar paket*) · Status (*Dijadwal ulang (28-10)*) · Pesanan · Tanggal · Aksi. Sebelum
   pemeriksaan dimulai, tab memberi keterangan *"Pemeriksaan belum dimulai…"*.
   - *Muat ulang status komponen* dikerjakan tanpa polling.
3. **Dokter yang ditetapkan** (`doctorId` akun = dokter MCU order):
   - *+ Tambah Pemeriksaan*: jenis, lalu prosedur Lab, prosedur + modalitas Rad, atau jenis Pemeriksaan MCU, dengan
     keterangan *"Ditagih terpisah, di luar harga paket."*
   - *Batal* komponen dengan alasan wajib, mis. dr. Rina membatalkan Thorax karena peserta hamil (`UAT-11`).
   - Akun lain tidak melihat kedua tombol ini.
4. **Jadwal ulang komponen** — tanggal susulan sesudah hari ini dan MCU buka, dengan keterangan *"Tidak memakai
   kuota."*
5. **Batal Penuh** (petugas, `McuOrder : Cancel`) — modal menampilkan akibatnya: *"Pesanan Lab/Rad dibatalkan dan
   pembayaran paket Rp 350.000 dikembalikan bila sudah dibayar."* Bila spesimen sudah diterima, server menolak dengan
   *"Sudah ada pemeriksaan yang dimulai. Gunakan pembatalan per komponen."* (`UAT-12`).
6. **Isi Hasil** — tombol hanya muncul pada komponen Pemeriksaan MCU yang *Siap diisi*/*Selesai*. Halamannya:
   - Isian urut nomor butir. Angka memakai masukan desimal, sehingga `6,4` tersimpan `6.4`. Pilihan dan Ya/Tidak
     memakai `BaseSelectField`. Ada pula Kesimpulan.
   - *Simpan Draf* tidak memeriksa isian wajib. *Simpan & Selesaikan* memeriksanya dengan kalimat server *"Isi
     Merokok."*. Sesudah selesai, tombolnya menjadi *Simpan Perubahan*.
   - Bila server memberi alasan baca-saja, mis. *"Skrining awal peserta belum selesai."* (`UAT-13`) atau *"Hasil
     terkunci…"*, seluruh isian nonaktif dan tombol simpan tidak tampil.
   - Tombol kembali membuka tab Pemeriksaan (`?tab=examination`).
7. **Data basi** — `409` *diubah petugas lain* memuat ulang detail dan komponen lebih dulu (`FE-MCU-010`). Modal tetap
   terbuka dengan pesan server, dan kiriman berikutnya memakai versi baru.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/.../mcu-patients/mcu-examination-constants.js` | `MCU_EXAMINATION_LIMITS` (alasan 500, kesimpulan 2.000, teks 500), `MCU_YES_NO_OPTIONS`, `MCU_EXAMINATION_CONFIG` (token komponen, segmen `hasil`, teks keterangan, pesan cadangan) |
| `src/utils/.../mcu-patients/mcu-examination-utils.js` (baru) | `isMcuAssignedDoctor`, `resolveMcuComponentActions`, `canAddMcuComponent`, `canRefreshMcuComponents`, `describeMcuComponentRow`, `formatMcuComponentProgress`, `describeMcuCancelEffect`, validasi + body modal tambah/batal/jadwal ulang, formulir hasil (`createMcuResultForm`, `validateMcuResultForm`, `buildMcuResultPayload`) |
| `src/lib/hooks/.../mcu-patients/use-mcu-examination.jsx` (baru) | Muat komponen saat tab aktif, muat ulang status, modal Mulai/Tambah/Batal/Jadwal Ulang; `version` dari detail terbaru di store; token rute komponen untuk *Isi Hasil* |
| `src/lib/hooks/.../mcu-patients/use-mcu-examination-result.jsx` (baru) | Halaman Isi Hasil: resolusi dua token, muat detail + hasil, formulir disalin ulang tiap jawaban server berganti, baca-saja dari `readOnlyReason` atau tanpa `Update` |
| `src/lib/hooks/.../mcu-patients/use-mcu-order-detail.jsx` | `isAssignedDoctor` dari `doctorId` sesi; `initialTab`; pemasang `startExamination` dan `cancelFull`; slice `mcuExamination` dikosongkan saat masuk/keluar; pengalihan UUID → token membawa `?tab=` |
| `src/lib/hooks/.../mcu-patients/use-mcu-order-actions.jsx` | Toast batal penuh berbeda dari batal sebelum datang |
| `src/components/view/.../mcu-patients/mcu-order-action-modals.jsx` | Judul *Batal Penuh Order MCU?* dan kalimat akibat untuk order *Hadir*/*Dalam Pemeriksaan* |
| `src/components/view/.../mcu-patients/detail/mcu-examination-tab.jsx` (baru) | Tab Pemeriksaan |
| `src/components/view/.../mcu-patients/detail/mcu-examination-modals.jsx` (baru) | Empat modal |
| `src/components/view/.../mcu-patients/detail/mcu-order-detail-view.jsx` | Tab Pemeriksaan + modal dipasang; `initialTab` |
| `src/components/view/.../mcu-patients/result/mcu-examination-result-view.jsx` (baru) | Halaman Isi Pemeriksaan MCU |
| `src/app/.../mcu-patients/[orderId]/page.jsx` | Membaca `?tab=` |
| `src/app/.../mcu-patients/[orderId]/components/[componentId]/hasil/page.jsx` (baru) | Route Isi Hasil |
| `tests/unit/mcu-examination-screen-rules.test.mjs` (baru) | 7 uji aturan |
| `tests/e2e/mcu-examination-screen.spec.mjs` (baru) | 7 skenario API-tiruan |
| `tests/e2e/mcu-patient-list-screen.spec.mjs` | Harapan detail diperbarui: *Batal Penuh* kini tersedia dan tab Pemeriksaan berisi komponen |

### 3.2 Kepatuhan arsitektur frontend

- Alur `view → hook → slice → service`.
  - Thunk dan service dari `FE-MCU-010`; tidak ada panggilan Axios di view atau hook selain sumber pilihan.
  - Sumber pilihan memakai service/resource yang sudah dipakai editor paket: `procedures/options`, `rad-modalities/options`, daftar jenis Pemeriksaan MCU.
- Aturan murni terpisah dari hook dan diuji unit.
- Aksi header tetap lewat `resolveMcuOrderActions` (`FE-MCU-001`).
- Formulir hasil menyalin ulang jawaban server memakai pola "state dari render sebelumnya" (`setState` saat objek
  hasil berganti), tanpa efek. Lint React Compiler lulus.
- ID komponen tidak tampil di URL; token rute privat dipakai seperti token order.
- Tidak ada dependency, pola state, atau komponen dasar baru.

---

## 4. State yang ditangani

| State | Perilaku |
| --- | --- |
| Memuat | Tabel komponen *"Mengambil komponen pemeriksaan..."*; halaman hasil *"Mengambil isian…"*; tombol *Muat ulang* dan *Simpan* ber-`loading` |
| Kosong | *"Order ini belum punya komponen pemeriksaan."*; jenis tanpa isian: *"Jenis pemeriksaan ini belum punya isian."* |
| Gagal | Komponen dan hasil: pesan + *Coba lagi*; aksi: pesan server di modal/formulir; muat ulang status: pesan di atas tabel |
| Kiriman ganda | Tombol modal *Memproses…* nonaktif selama permintaan (`UAT-09` E2E) |
| Validasi | Kalimat server sebelum kirim: `VAL-MCU-06`, `40`, `42`, `43`, batas panjang, tanggal susulan |
| Tanpa hak akses | Tombol disembunyikan menurut izin + dokter yang ditetapkan; `403` server tampil apa adanya; halaman hasil `403` → akses ditolak; tanpa `Update` → baca-saja berketerangan |
| Data basi | `409` memuat ulang detail + komponen (+ hasil), modal tetap terbuka dengan pesan, kiriman ulang memakai versi baru (E2E) |
| Baca-saja server | `readOnlyReason` tampil sebagai peringatan; isian nonaktif |
| Privasi | Hasil hanya di halaman berizin; ID komponen tidak di URL |
| Aksesibilitas | Label tiap isian (nomor + label + satuan), formulir ber-`aria-label`, tombol *Isi hasil {komponen}* dan *Aksi komponen {komponen}* bernama, kelengkapan `aria-live` |

---

## 5. Endpoint yang dikonsumsi

| Method | Path | Dipakai untuk |
| --- | --- | --- |
| `POST` | `orders/{id}/start-examination` | Mulai Pemeriksaan |
| `GET` / `POST` | `orders/{id}/components`, `components/refresh` | Tabel, muat ulang status, tambah komponen |
| `PATCH` | `orders/{id}/components/{cid}/cancel`, `/reschedule` | Batal / jadwal ulang komponen |
| `GET` / `PUT` | `orders/{id}/components/{cid}/examination-result` | Isi Hasil |
| `PATCH` | `orders/{id}/cancel` | Batal Penuh |
| `GET` | `daily-schedules?from=&to=` | Buka/tutup tanggal susulan |
| `GET` | `/v1/health-services/master-data/procedures/options`, `rad-modalities/options`, `examination-types?isActive=true` | Pilihan modal tambah |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `tests/unit/mcu-examination-screen-rules.test.mjs` | 7/7 lulus: dokter yang ditetapkan (tak peka huruf), header Hadir/Dalam Pemeriksaan, matriks aksi komponen, tambah/muat ulang per status, teks baris + kelengkapan + akibat batal penuh, kalimat validasi modal, formulir hasil (`6,4`/`6.4`/` 6,4 ` → `"6.4"`) | `PASS` | `# pass 7` |
| **`UAT-09`** E2E | Modal *Memproses…* nonaktif selama permintaan; pesan `VAL-MCU-34` tampil apa adanya; satu `POST` ber-`{ version: 5 }`; nol tulis lain; status tetap *Hadir* | `PASS` | Spec skenario 1 |
| **`UAT-09`** backend lokal + devYoga (tanpa tulis) | `MCU-RSMMC-000002` (*Hadir*, versi 1). Pagar baca lebih dulu: ringkasan tagihan `OPEN`, `patientAmount` 350.000, `payerAmount` 0, nol pembayaran. Layar: URL bertoken membawa `?tab=examination`; tab *"Pemeriksaan belum dimulai…"*, 4 komponen *Menunggu*, *Kelengkapan: 0 dari 4*. *Mulai Pemeriksaan* → modal *"Tagihan paket belum lunas (sisa Rp 350.000)."* Sesudahnya order tetap *Hadir* versi 1, nol pesanan Lab/Rad; satu-satunya non-GET = `POST start-examination` → `409`. API: `examination-result` Pemeriksaan Fisik → `readOnlyReason` *"Pemeriksaan belum dimulai."* | `PASS` | Keluaran `uat-011.mjs` (scratchpad sesi 56020cfd) |
| **`UAT-10`** E2E | Sebelum mulai tab berketerangan; sesudah mulai *Dalam Pemeriksaan*, `LAB-RSMMC-000123`/`RAD-RSMMC-000045` + *Paket* tampil, toast *Pemeriksaan Dimulai* | `PASS` (tiruan) | Spec skenario 2 |
| **`UAT-11`** E2E | Dokter yang ditetapkan: menu Thorax (*Dipesan*) tanpa *Jadwal Ulang*; alasan kosong → *"Alasan wajib diisi."* tanpa kirim; `PATCH` `{ reason: "Peserta hamil", version: 5 }`; baris *Dibatalkan* + alasan; kelengkapan 0 dari 3; detail dimuat ulang. Akun tanpa `doctorId`: tanpa *+ Tambah* dan tanpa menu Thorax | `PASS` (tiruan) | Spec skenario 3 |
| **`UAT-12`** E2E | Menu *Batal Penuh* → judul + kalimat akibat; `PATCH cancel` `{ reason, version: 5 }`; pesan `VAL-MCU-41` tampil apa adanya | `PASS` (tiruan) | Spec skenario 4 |
| **`UAT-13`** E2E | *Isi hasil* → URL `…/components/<token>/hasil` tanpa UUID; peringatan *"Skrining awal peserta belum selesai."*; isian nonaktif; tanpa tombol simpan; nol `PUT` | `PASS` (tiruan) | Spec skenario 5 |
| **Isian `6.4`** E2E | Label urut *1. Berat badan (kg)*, *2. Buta warna*, *3. Merokok* walau server mengirim acak. *Simpan & Selesaikan* tanpa Merokok → *"Isi Merokok."*, nol `PUT`. *Simpan Draf* → `values` `[{BB,"6.4"},{Buta,null},{Rokok,null}]`, `complete:false`, isian tampil `6.4`. Selesaikan → `Normal`/`Tidak`/`Sehat`, `complete:true`, status *Selesai*, tombol *Simpan Perubahan*. Kembali → `?tab=examination` | `PASS` (tiruan) | Spec skenario 6 |
| Tambah, jadwal ulang, muat ulang, basi E2E | Kalimat `VAL-MCU-06`; `POST components` `{componentType:1, procedureId}` → baris *Di luar paket*. Jadwal ulang GDP: `409` `VAL-MCU-46` → pesan di modal, `GET` detail + komponen dimuat ulang, kirim ulang `version: 9` → *Dijadwal ulang (dd-mm)*. Muat ulang status → satu `POST refresh`, *1 dari 5 tuntas* | `PASS` (tiruan) | Spec skenario 7 |
| Seluruh spec E2E MCU (8 berkas, 53 skenario) | Run penuh: 51 lulus, 2 gagal. (a) `mcu-patient-list` — harapan lama *"Batal Penuh Fitur belum tersedia"*, perubahan disengaja → spec diperbarui. (b) `mcu-check-in` *409 basi* — klik *Check-in* di dasar halaman tidak terkirim; berkas check-in tidak diubah task ini. Ulang dua berkas itu: 13/13 lulus | `PASS` / `FLAKY EXISTING` | Keluaran Playwright |
| `npx eslint` (berkas layar pasien MCU + uji) | Exit 0 | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 3.914 uji: 3.872 lulus, 42 gagal; daftar nama **identik** baseline (`diff` kosong), semuanya di luar MCU | `EXISTING ISSUE` | Keluaran + `diff` |
| `npm run build` (dua kali, terakhir sesudah perbaikan `?tab=`) | Exit 0, *Compiled successfully in 57s* | `PASS` | Keluaran build |

`MANUAL TEST`: kontrol interaktif diuji lewat Playwright pada build standalone (E2E di atas) dan layar sungguhan
terhadap backend lokal untuk `UAT-09`, termasuk tab, tabel, modal, dan pesan server.

`UAT-10`..`13` terhadap backend lokal **tidak dijalankan**. Alasannya: keputusan pemilik pada sesi ini adalah UAT
tanpa tulis, sedangkan `UAT-10`..`13` menuntut pembayaran paket di kasir, pesanan Lab/Rad sungguhan, dan akun dokter
yang ditetapkan (dr. Bima).

`AUTOMATED TEST: npm run test:unit — FAIL (42 kegagalan existing di luar MCU, identik baseline; uji MCU PASS)`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `UAT-09` | Terpenuhi — E2E + backend lokal | §6 |
| `UAT-10`..`UAT-13` | Terpenuhi di E2E API-tiruan; uji data sungguhan menjadi batas verifikasi | §6 |
| Isian `6.4` tersimpan `6.4` | Terpenuhi di layar (body `"6.4"`); normalisasi server `TryNormalizeNumber` sudah diuji `BE-MCU-023` | §6 |
| DoD: kriteria terbukti | Terpenuhi dengan batas verifikasi di atas | §6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Delta brief (keputusan pemilik 2026-10-27) | Brief 5.6 meminta modal batal komponen menampilkan nilai pengembalian (mis. *"Rp 150.000 akan dikembalikan."*). `McuOrderComponentResponse` tidak memuat nominal, dan brief §7 melarang data di luar kontrak. Modal menulis *"Nilai pengembalian komponen ini diajukan ke Billing sesuai paket."* Nominal ditambahkan bila backend kelak menyediakan ruasnya |
| Batas verifikasi | `UAT-10`..`13` belum diuji dengan data sungguhan. Untuk menutupnya dibutuhkan: pembayaran paket `MCU-RSMMC-000002` di kasir, izin tulis devYoga, dan sandi dr. Bima (dokter yang ditetapkan). Uji API-tiruan tidak menangkap ketidakcocokan payload; payload dibandingkan langsung dengan DTO backend |
| Risiko izin | Modal *Tambah Pemeriksaan* butuh `Procedure : Read`, `RadModality : Read`, dan `McuReferenceData : Read` untuk pilihannya. Tanpa itu pilihan kosong dan pesan server tampil di modal. Matriks izin peran dokter MCU perlu dicek saat go-live |
| Peringatan | (1) Toast global duduk di atas footer dan menelan klik pada tombol di dasar halaman ±3,5 detik (perilaku tata letak yang sudah ada, tercatat sejak 2026-10-20). Spec menunggu toast hilang. (2) `anyComponentStarted` tidak diketahui layar, sehingga *Batal Penuh* selalu tampil pada *Hadir*/*Dalam Pemeriksaan* dan server penentu akhirnya (`VAL-MCU-41`) |
| Dependency backend | Tidak ada |
| Perubahan sampingan | `NONE`. Output Playwright ke scratchpad (`--output`), sehingga `test-results/` ter-track tidak tersentuh; server BE 7184 dan FE 3710 dimatikan |
| Interupsi | Pemeriksa izin otomatis berkali-kali tidak memberi putusan; pekerjaan dilanjutkan dari keadaan terverifikasi (lint, build ulang) |
| Status Git (frontend) | `??` berkas baru di §3.1 (di dalam folder `mcu-patients/` yang belum ter-track sejak `FE-MCU-007`) dan `tests/e2e/mcu-examination-screen.spec.mjs`, `tests/unit/mcu-examination-screen-rules.test.mjs` |
| Langkah berikutnya | `FE-MCU-012` (Resume dokter) — kini siap; `FE-MCU-014` (Ganti dokter) juga terbuka |
