# Roadmap Frontend — Episode Rawat Inap, Finishing Rawat Inap

| Field | Nilai |
|---|---|
| Roadmap | `episode-rawat-inap/roadmap/frontend-roadmap-finishing.md` — revision `1` |
| Blueprint | `RWI-BP-001` revision `8`, sub-modul `episode-rawat-inap`, kontrak **`0.10.0` `approved`** 2026-10-02 lewat `RWI-DEC-221` |
| Status roadmap | **`APPROVED`** — disetujui pemilik modul 5 Oktober 2026. Task siap dikirim ke `build-module-frontend` per-slice |
| Ditulis | 2 Oktober 2026 oleh `plan-module-delivery` |
| Masukan dan hash approval | `03-frontend-architecture.md` bagian 13 (`e31154bd…`), `contracts/api-contract.md` bagian 11 (`958fcd2c…`), `testing/acceptance-test-matrix.md` bagian Finishing (`8e1e2b5d…`); peta menu `02-module-map.md` revision `4` bagian 7.3. Hash lengkap pada `../blueprint-manifest.md` bagian 11 |
| Keputusan | `RWI-DEC-173` s.d. `177`, `182`, `189`, `197`, `199`, `201`, `204`, `205`, `208`, `213` s.d. `220`, `221`; gate `1.10` |
| Source SHA | Frontend `f74758af5`; backend `bf5c6bde` |
| Deret ID | `FE-RWI-192` s.d. `FE-RWI-201` |
| Roadmap pendamping | `backend-roadmap-finishing.md`, `requirement-traceability-finishing.md` |

**Kebijakan verifikasi frontend.** Mengikuti `rules/frontend/test-policy.md`: bukti utama verifikasi manual kontrol interaktif dengan backend berjalan, ditambah `npm run lint`, `npm run test:unit` (suite yang ada), dan `npm run build`. Test unit baru opsional. Jest dan `@testing-library` tidak dipakai. Laporan task memuat baris `AUTOMATED TEST` dan `MANUAL TEST` terpisah.

**Kewenangan UI.** Mengikat: dua tab Pemesanan Ruangan Bedah seperti V1 (`RWI-DEC-172`), jalan masuk dokter lewat penanda Konteks pasien (`RWI-DEC-213`), butir menu ke-10 "Laporan Rawat Inap" sebagai wadah (`RWI-DEC-214`, `215`), urutan daftar pantau (`RWI-DEC-216`), tidak ada rupiah selain perkiraan tarif tindakan berlabel (`RWI-DEC-218`). Rupa di dalam letak itu `DEV_DISCRETION` (`RWI-FE-006`); `pathname` Laporan Rawat Inap masih usulan.

**Arti tanda status pada dokumen ini.**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

## Grafik Urutan Dependency

Roadmap ini memuat 10 task dan 11 prasyarat dari roadmap lain, melewati batas 15 node satu grafik. Grafik dipecah per slice di bawah judul slice masing-masing; grafik di bawah ini adalah ringkasan antar-slice.

```text
Slice E1 Kamar operasi dari bangsal

Slice E2 Permintaan admisi dan daftar pantau

Slice E3 Serah terima transfer dan Laporan Rawat Inap (P2)
```

Ketiga slice tidak saling menunggu. Seluruh prasyarat lintas roadmap digambar pada grafik slice. Jumlah pasangan prasyarat→task pada ketiga grafik slice: **14** (E1: 10, E2: 2, E3: 2), sama dengan isi kolom `Dependency`.

| Label | Asal |
|---|---|
| `[BE]` | Task backend sub-modul ini pada `backend-roadmap-finishing.md` |
| `[KEP]` | Task frontend sub-modul `keperawatan` pada `../../keperawatan/roadmap/frontend-roadmap-finishing.md` |

Seluruh node berlabel adalah cermin baca-saja.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `BE-RWI-173` [BE] | `FE-RWI-192` ✅ |
| 1 | `BE-RWI-175` [BE] | `FE-RWI-193` ✅ |
| 1 | `BE-RWI-174` [BE] | `FE-RWI-194` ✅, `FE-RWI-197` ✅ — boleh paralel |
| 1 | `BE-RWI-181` [BE] | `FE-RWI-198` ✅ |
| 1 | `BE-RWI-182` [BE] | `FE-RWI-199` ✅ |
| 1 | `BE-RWI-183` [BE] | `FE-RWI-200` ✅ |
| 1 | `BE-RWI-184` [BE] | `FE-RWI-201` ✅ |
| 2 | `FE-RWI-194`, `BE-RWI-176` [BE], `FE-RWI-180` [KEP] | `FE-RWI-195` ✅ |
| 2 | `FE-RWI-194`, `BE-RWI-177` [BE], `BE-RWI-180` [BE] | `FE-RWI-196` ✅ |

**Pemetaan ke gelombang `04-prd-to-mvp.md` bagian 23.20.** `MVP-1` (`RWF-W3`): `FE-RWI-192` s.d. `FE-RWI-197` (`FE-INP-25` s.d. `28`, `33`, `34`). `MVP-2` (`RWF-W7`): `FE-RWI-198`, `FE-RWI-199`; bagian ringkasan operasi pada `FE-RWI-196` boleh lebih dulu. `POST-MVP` (`P2`): `FE-RWI-200`, `FE-RWI-201`.

**Catatan jalan masuk.** Tautan dari baris daftar pantau (`FE-RWI-199`) ke laci Pasca Operasi (`FE-RWI-196`) dan ke Permintaan Admisi (`FE-RWI-198`) dipasang bila layar tujuannya sudah ada; tautan itu bukan prasyarat.

## Slice E1 — Kamar operasi dari bangsal

### Grafik dependency slice E1

```text
BE-RWI-173 [BE] ─> FE-RWI-192 ✅

BE-RWI-175 [BE] ─> FE-RWI-193

BE-RWI-174 [BE] ─┬─> FE-RWI-197
                 │
                 └─> FE-RWI-194 ─┬────────────────────────┬─> FE-RWI-195
                                 │                        │
                                 │ BE-RWI-176 [BE] ────┐  │
                                 │                     │  │
                                 │   FE-RWI-180 [KEP] ─┴──┘
                                 │
                                 └────────────────────────┬─> FE-RWI-196
                                                          │
                                   BE-RWI-177 [BE] ────┐  │
                                                       │  │
                                      BE-RWI-180 [BE] ─┴──┘
```

Pasangan: 10.

### Tabel task slice E1

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-RWI-192` | Master Butir Persiapan Bedah dapat dikelola | `FR-RWF-045`; `RWI-DEC-173` (3) | `0.10.0` FE 13.4.10 `FE-INP-34`; API 11.4 | Pola layar master data | Butir menu baru; daftar dan formulir | `BE-RWI-173` [BE] | Kartu | Kartu | Isi awal disahkan klinis / Muhammad Hamzah | [FE-RWI-192.md](../task/report/frontend/FE-RWI-192.md) ✅ |
| `FE-RWI-193` | Pemesanan Ruangan Bedah dua tab dari satu order, dengan perkiraan tarif | `FR-RWF-040` s.d. `043`; `RWI-DEC-175`, `176`, `218`; `UAT-RWF-32`, `42`, `43` | FE 13.4.1 `FE-INP-25`; API 11.1, 11.2 | Menu ke-7 ruang kerja keperawatan (*placeholder*) | Dua tab, pilihan order, isian, kirim | `BE-RWI-175` [BE] | Kartu | Kartu | — / Muhammad Hamzah | [FE-RWI-193.md](../task/report/frontend/FE-RWI-193.md) ✅ |
| `FE-RWI-194` | Daftar Pesanan Ruang Bedah pasien dengan delapan status | `FR-RWF-044`, `086`; `RWI-DEC-204`; `UAT-RWF-24` | FE 13.4.2 `FE-INP-26`; API 11.5.1 | `GET cases?encounterId=` | Tabel, aksi baris, kartu Operasi di Detail Episode | `BE-RWI-174` [BE] | Kartu | Kartu | — / Muhammad Hamzah | [FE-RWI-194.md](../task/report/frontend/FE-RWI-194.md) ✅ |
| `FE-RWI-195` | Catatan Pra-Operasi dua sisi berversi | `FR-RWF-045`, `048`, `090`; `RWI-DEC-173`, `174`, `199`; `UAT-RWF-21` | FE 13.4.3 `FE-INP-27`; API 11.3 | Wadah Catatan Keperawatan; tab Persiapan kasus OK | Mode pengirim dan penerima; penandaan; versi | `FE-RWI-194`, `BE-RWI-176` [BE], `FE-RWI-180` [KEP] | Kartu | Kartu | Dua akun wajib / Muhammad Hamzah | [FE-RWI-195.md](../task/report/frontend/FE-RWI-195.md) ✅ |
| `FE-RWI-196` | Laci Pasca Operasi: ringkasan baca-saja dan serah terima masuk | `FR-RWF-046`, `049`, `081`, `082`; `RWI-DEC-177`, `189`, `197`, `213`; `UAT-RWF-13`, `17` | FE 13.4.4 `FE-INP-28`; API 11.5.1, 11.5.2 | Aksi baris `FE-RWI-194` | Ringkasan, Terima/Tolak, mode baca-saja untuk dokter | `FE-RWI-194`, `BE-RWI-177` [BE], `BE-RWI-180` [BE] | Kartu | Kartu | Dipakai ulang `dokter-rawat-inap` `FE-RWI-178` / Muhammad Hamzah | [FE-RWI-196.md](../task/report/frontend/FE-RWI-196.md) ✅ |
| `FE-RWI-197` | Petugas OK menolak order berstatus Diminta | `FR-RWF-086`; `RWI-DEC-204`, `208`; `UAT-RWF-24` | FE 13.4.9 `FE-INP-33`; API 11.5.1, 11.9 | Detail kasus dan laporan OK | Dialog tolak; tampilan Ditolak; kolom laporan | `BE-RWI-174` [BE] | Kartu | Kartu | Layar modul OK milik Ikbal Yulianto | [FE-RWI-197.md](../task/report/frontend/FE-RWI-197.md) ✅ |

## Slice E2 — Permintaan admisi dan daftar pantau

### Grafik dependency slice E2

```text
BE-RWI-181 [BE] ─> FE-RWI-198

BE-RWI-182 [BE] ─> FE-RWI-199
```

Pasangan: 2.

### Tabel task slice E2

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-RWI-198` | Admisi dari permintaan kamar pulih yang terisi awal | `FR-RWF-080`, `089`; `RWI-DEC-201`, `220` (1); `UAT-RWF-16`, `33`, `41` | FE 13.4.5 `FE-INP-29`; API 11.6 | Layar Admisi `FE-INP-03` dan alur berlangkahnya | Daftar "Dari Kamar Pulih"; pengisian awal; penanganan 409/422 | `BE-RWI-181` [BE] | Kartu | Kartu | — / Muhammad Hamzah | [FE-RWI-198.md](../task/report/frontend/FE-RWI-198.md) ✅ |
| `FE-RWI-199` | Dua daftar pantau tertunda dan dua ambang pengaturan | `FR-RWF-088`; `RWI-DEC-201`, `216`, `220` (6) | FE 13.1, 13.4.6 `FE-INP-30`; API 11.9 | Daftar Pantau `FE-INP-09`, Pengaturan `FE-INP-12` | Dua daftar; dua isian ambang | `BE-RWI-182` [BE] | Kartu | Kartu | — / Muhammad Hamzah | [FE-RWI-199.md](../task/report/frontend/FE-RWI-199.md) ✅ |

## Slice E3 — Serah terima transfer dan Laporan Rawat Inap (`P2`)

### Grafik dependency slice E3

```text
BE-RWI-183 [BE] ─> FE-RWI-200

BE-RWI-184 [BE] ─> FE-RWI-201
```

Pasangan: 2.

### Tabel task slice E3

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-RWI-200` | Serah terima transfer antarunit lewat banner dan laci (`P2`) | `FR-RWF-071`; `RWI-DEC-182`, `189`; `UAT-RWF-14` | FE 13.4.7 `FE-INP-31`; API 11.8 | `FE-INP-04`, `FE-KEP-07` ("Integrasi belum tersedia") | Banner, laci sembilan bagian, kirim, terima/tolak | `BE-RWI-183` [BE] | Kartu | Kartu | `P2` / Muhammad Hamzah | [FE-RWI-200.md](../task/report/frontend/FE-RWI-200.md) ✅ |
| `FE-RWI-201` | Butir menu ke-10 Laporan Rawat Inap dengan Laporan Transfer Ruangan (`P2`) | `FR-RWF-087`; `RWI-DEC-205`, `214`, `215`, `220` (4); `RWI-AC-340`; `UAT-RWF-20`, `44` | FE 13.2, 13.4.8 `FE-INP-32`; API 11.7 | Peta menu `02-module-map.md` 7.3 | Butir menu bersyarat permission; halaman wadah; laporan dan ekspor | `BE-RWI-184` [BE] | Kartu | Kartu | Kuota `IA-INP-05` habis / Muhammad Hamzah | [FE-RWI-201.md](../task/report/frontend/FE-RWI-201.md) ✅ |

## Kartu task

### `FE-RWI-192` — Master Butir Persiapan Bedah

| Field | Isi |
|---|---|
| **Status** | ✅ Selesai (5 Oktober 2026, [laporan](../task/report/frontend/FE-RWI-192.md)) |
| **Outcome** | Admin Master Data mengelola butir checklist persiapan bedah lewat butir menu Master Data → Butir Persiapan Bedah |
| **Requirement/decision** | `FR-RWF-045`; `RWI-DEC-173` butir 3 |
| **Kontrak** | Frontend 13.1, 13.2, 13.4.10 (`FE-INP-34`, `pathname` `/health-services/master-data/surgical-preparation-items`, `SurgicalPreparationItem : Read`); API 11.4 |
| **Reuse** | Pola layar master data yang sudah ada |
| **Cakupan** | Butir menu baru; daftar dengan saringan kelompok dan aktif; formulir kode, kelompok, nama butir, wajib, urutan, keterangan; aktif/nonaktif; kosong → "Belum ada butir persiapan bedah." |
| **Dependency** | `BE-RWI-173` [BE] |
| **Acceptance criteria** | 1. Butir menu tampil bagi pemegang `SurgicalPreparationItem : Read`. 2. Tambah, ubah, dan aktif/nonaktif berfungsi; versi berubah → muat ulang. 3. Keadaan kosong sesuai skema. 4. Butir nonaktif tidak muncul pada versi pra-operasi baru (diperiksa bersama `FE-RWI-195`) |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual |
| **Risiko/pemilik** | Isi awal disahkan pemilik klinis sebelum produksi. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan `../task/report/frontend/FE-RWI-192.md` memuat `AUTOMATED TEST` dan `MANUAL TEST`; roadmap dan traceability diperbarui |

### `FE-RWI-193` — Pemesanan Ruangan Bedah

| Field | Isi |
|---|---|
| **Status** | ✅ Selesai (2026-10-05) |
| **Outcome** | Menu ketujuh ruang kerja keperawatan "Pemesanan Ruangan Bedah" keluar dari *placeholder* dengan tab Bedah Operasi dan Bedah Obgyn. Perawat atau dokter memesan dari satu order tindakan operasi aktif dan melihat perkiraan tarif tindakannya |
| **Requirement/decision** | `FR-RWF-040` s.d. `043`; `RWI-DEC-175`, `176`, `218`, `219`, `220` butir 5; `AC-RWF-040`, `048`; `RWI-AC-338`; `UAT-RWF-32`, `42`, `43` |
| **Kontrak** | Frontend 13.1, 13.2, 13.4.1, 13.7 (`FE-INP-25`); API 11.1 (`GET patient-procedures?encounterId=&procedureStatus=`), 11.2 (`POST episodes/{episodeId}/surgery-bookings`) |
| **Reuse** | Menu Pemesanan Ruangan Bedah di `nursing-workspace-sections.jsx` (*placeholder* bagian bedah `FE-KEP-17`) |
| **Cakupan** | Dua tab; pilihan order tindakan operasi aktif; dokter operator dari order (tidak dapat diubah); perkiraan tarif dari `UnitPrice` dan `CoverageStatus` order berlabel "perkiraan — tagihan final di kasir" beserta keterangan bahwa anestesi, sewa kamar operasi, dan bahan ditagihkan setelah operasi; isian `SurgeryBookingRequest`; `Idempotency-Key`; pesan `INP-SRG-001`/`002` |
| **Dependency** | `BE-RWI-175` [BE] |
| **Acceptance criteria** | 1. Dua tab seperti V1. 2. Tanpa order aktif → "Tindakan operasi belum dipesan dokter. Minta dokter memesan tindakan lebih dulu." dan tombol Pesan nonaktif (`UAT-RWF-32`). 3. Episode `DischargePending` → "Pemesanan hanya untuk pasien yang sedang dirawat" (`UAT-RWF-42`). 4. Perkiraan tarif dan keterangan komponen OK tampil; harga tidak ada → "tarif belum tersedia" dan tombol tetap aktif (`UAT-RWF-43`). 5. Tab Obgyn menampilkan jenis "Obstetri" yang tetap. 6. Klik ganda tidak membuat dua kasus. 7. Tombol Pesan hanya bagi `OperatingRoomCase : Create` |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual dengan order tindakan uji |
| **Risiko/pemilik** | Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

### `FE-RWI-194` — Daftar Pesanan Ruang Bedah pasien

| Field | Isi |
|---|---|
| **Status** | ✅ Selesai (2026-10-05) |
| **Outcome** | Di bawah formulir pemesanan dan pada kartu Operasi di Detail Episode, bangsal melihat kasus operasi pasien dengan delapan label status, alasan tunda, batal, atau tolak, penolak dan waktunya, serta status pra-operasi dan serah terima |
| **Requirement/decision** | `FR-RWF-044`, `FR-RWF-086`; `RWI-DEC-204`; `AC-RWF-041`; `UAT-RWF-24` |
| **Kontrak** | Frontend 13.4.2 (`FE-INP-26`); API 11.1 dan 11.5.1 (respons kasus ditambah) |
| **Reuse** | `GET operating-room-management/cases?encounterId=` |
| **Cakupan** | Tabel kasus; label Ditolak beserta alasan, penolak, dan waktu; aksi baris "Pesan ulang" (membuka `FE-INP-25` dengan order yang sama), "Pra-operasi" dan "Pasca operasi" (jalan masuk layar `FE-RWI-195` dan `FE-RWI-196`, tampil bila layar itu sudah ada); tombol Muat ulang dan penyegaran saat menu dibuka; kartu Operasi pada `FE-INP-04` |
| **Dependency** | `BE-RWI-174` [BE] |
| **Acceptance criteria** | 1. Delapan label status tampil benar. 2. Kasus Ditolak menampilkan alasan, penolak, waktu, dan tombol Pesan ulang (`UAT-RWF-24`). 3. Kosong → "Belum ada pesanan ruang bedah."; gagal → pesan dan Coba lagi. 4. Keterangan "Label Diminta tidak menjamin ruang" tampil. 5. Kartu Operasi tampil di Detail Episode. 6. Tanpa `OperatingRoomCase : Read` daftar tidak tampil |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual dengan kasus berbagai status |
| **Risiko/pemilik** | Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

### `FE-RWI-195` — Catatan Pra-Operasi

| Field | Isi |
|---|---|
| **Status** | ✅ Selesai (2026-10-05) |
| **Outcome** | Perawat bangsal mengisi dan mengirim Catatan Pra-Operasi dari Catatan Keperawatan atau dari aksi baris daftar pesanan; perawat OK mengonfirmasinya dari tab Persiapan detail kasus. Versi yang "perlu diperbarui" sesudah penundaan terlihat jelas |
| **Requirement/decision** | `FR-RWF-045`, `048`, `090`; `RWI-DEC-173`, `174`, `199`; `AC-RWF-042`, `045`, `046`, `093`, `094`; `UAT-RWF-21` |
| **Kontrak** | Frontend 13.2, 13.4.3 (`FE-INP-27`); API 11.3 |
| **Reuse** | Wadah sub-menu Catatan Pra-Operasi dari `keperawatan` `FE-RWI-180`; aksi baris `FE-RWI-194`; tab Persiapan detail kasus OK (`app/health-services/operating-room-management/cases`) |
| **Cakupan** | Mode pengirim dan penerima; potret tanda vital dan nyeri; checklist per kelompok dari master; penandaan titik pada gambar tubuh, sisi, dan keterangan lokasi tanpa unggah foto; banner "Perlu diperbarui setelah penundaan" dan tombol "Buat versi baru"; riwayat versi; akun pengirim tidak melihat tombol Konfirmasi; pesan `OPR-WPO-001` s.d. `005`; kendala pra-operasi pada kesiapan kasus |
| **Dependency** | `FE-RWI-194`, `BE-RWI-176` [BE], `FE-RWI-180` [KEP] |
| **Acceptance criteria** | 1. Belum ada tanda vital → "Catat tanda vital pasien lebih dulu" dan Kirim nonaktif. 2. Master kosong → "Butir persiapan belum diatur di Master Data". 3. Sisi berbeda → pesan `OPR-WPO-001` di bawah pilihan sisi. 4. Akun pengirim melihat "Konfirmasi harus oleh akun lain", bukan tombol Konfirmasi. 5. Kasus ditunda → banner kuning; versi baru menyalin butir lama dan memuat TD terbaru (`UAT-RWF-21`). 6. Kesiapan kasus menampilkan kendala pra-operasi. 7. Tidak ada unggah foto |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual dengan dua akun (perawat bangsal dan perawat OK) |
| **Risiko/pemilik** | Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

### `FE-RWI-196` — Laci Pasca Operasi

| Field | Isi |
|---|---|
| **Status** | ✅ Selesai (2026-10-05) |
| **Outcome** | Laci Pasca Operasi menampilkan ringkasan operasi baca-saja dan serah terima masuk dengan tombol Terima/Tolak bagi penerima sah. Laci yang sama menyediakan mode baca-saja untuk ruang kerja dokter |
| **Requirement/decision** | `FR-RWF-046`, `049`, `081`, `082`; `RWI-DEC-177`, `189`, `197`, `213`; `AC-RWF-043`, `047`, `081`, `082`; `UAT-RWF-13`, `UAT-RWF-17` |
| **Kontrak** | Frontend 13.4.4, 13.7 (`FE-INP-28`); API 11.5.1 (`post-operative-summary`), 11.5.2 (`accept`) |
| **Reuse** | Aksi baris `FE-RWI-194` |
| **Cakupan** | Ringkasan dari `PostOperativeSummaryResponse`; serah terima: status, pengirim, waktu; Terima dan Tolak beralasan bagi `OperatingRoomHandover : Receive`; `OPR-HO-001` → tombol Terima terkunci dengan pesan dan tautan ke Perpindahan Pasien `FE-INP-05`; `OPR-HO-002` → pesan; mode baca-saja tanpa Terima/Tolak (dipakai `dokter-rawat-inap` `FE-RWI-178`) |
| **Dependency** | `FE-RWI-194`, `BE-RWI-177` [BE], `BE-RWI-180` [BE] |
| **Acceptance criteria** | 1. Laporan final → ringkasan lengkap baca-saja (`UAT-RWF-17`). 2. Laporan draft → "Laporan operasi belum final". 3. Pasien belum di unit tujuan → Terima terkunci dengan pesan dan tautan transfer (`UAT-RWF-13`). 4. Pengirim mencoba menerima → pesan `OPR-HO-002`. 5. Tolak wajib alasan. 6. Mode baca-saja tidak menampilkan Terima/Tolak. 7. Tidak ada rupiah |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual skenario ICU dengan dua akun |
| **Risiko/pemilik** | Komponen dipakai ulang sub-modul dokter; antarmuka mode baca-saja dijaga stabil. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

### `FE-RWI-197` — Tolak Order Operasi di layar OK

| Field | Isi |
|---|---|
| **Status** | ✅ Selesai (2026-10-05) |
| **Outcome** | Petugas penjadwalan OK menolak kasus berstatus Diminta dengan alasan. Kasus Ditolak tampil berlabel merah dengan alasan, penolak, dan waktu, tanpa tombol ubah; laporan operasi memuat kolom Ditolak tersendiri |
| **Requirement/decision** | `FR-RWF-086`; `RWI-DEC-204`, `RWI-DEC-208`; `AC-RWF-085`, `099`; `UAT-RWF-24` |
| **Kontrak** | Frontend 13.4.9 (`FE-INP-33`); API 11.5.1 (`PATCH cases/{id}/reject`), 11.9 (`RejectedCount`) |
| **Reuse** | Detail kasus dan Laporan Operasi di `app/health-services/operating-room-management/cases` dan `…/reports` |
| **Cakupan** | Tombol Tolak pada kasus Diminta; dialog alasan 10–500 karakter; tampilan kasus Ditolak; kolom Ditolak pada laporan |
| **Dependency** | `BE-RWI-174` [BE] |
| **Acceptance criteria** | 1. Tombol Tolak hanya pada kasus Diminta dan bagi `OperatingRoomCase : Reject`. 2. Alasan divalidasi; pesan server 400 dan 422 tampil apa adanya. 3. Kasus Ditolak tanpa tombol ubah. 4. Laporan Operasi memisahkan Ditolak dari Dibatalkan |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual oleh petugas OK |
| **Risiko/pemilik** | Layar modul Kamar Operasi milik Ikbal Yulianto (persetujuan `RWI-DEC-208`) |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

### `FE-RWI-198` — Permintaan Admisi dari Kamar Pulih

| Field | Isi |
|---|---|
| **Status** | ✅ Selesai (2026-10-05) |
| **Outcome** | Petugas admisi melihat daftar "Dari Kamar Pulih" pada layar Admisi Rawat Inap dan membuka admisi berlangkah yang terisi awal dari permintaan |
| **Requirement/decision** | `FR-RWF-080`, `FR-RWF-089`; `RWI-DEC-201`, `RWI-DEC-220` butir 1; `AC-RWF-080`, `088`, `089`; `UAT-RWF-16`, `33`, `41` |
| **Kontrak** | Frontend 13.4.5 (`FE-INP-29`); API 11.6 |
| **Reuse** | Layar Admisi Rawat Inap `FE-INP-03` (`app/health-services/inpatient-management/admissions`) dan alur berlangkahnya |
| **Cakupan** | Daftar "Dari Kamar Pulih (n)" dengan lamanya menunggu (merah bila melewati ambang); tombol Admisi membuka alur berlangkah dengan pasien, kunjungan asal, dan usulan DPJP terisi — penjamin, kelas, DPJP, deposit, dan bed tetap diisi petugas; `INP-ADM-REF-002` → pesan dan muat ulang; admisi biasa yang ditolak 409 `INP-ADM-REF-001` → tawaran "Buka dari permintaan" |
| **Dependency** | `BE-RWI-181` [BE] |
| **Acceptance criteria** | 1. Daftar memuat permintaan `Pending`; kosong → "Tidak ada pasien dari kamar pulih yang menunggu admisi." 2. Admisi dari permintaan terisi awal dan berhasil; permintaan hilang dari daftar (`UAT-RWF-16`). 3. Admisi biasa untuk pasien berpermintaan ditolak dan ditawari "Buka dari permintaan" (`UAT-RWF-33`, `41`). 4. Permintaan yang sudah selesai atau batal → pesan dan muat ulang. 5. Tombol Admisi hanya bagi `InpatientEpisode : Create` |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual ujung ke ujung dengan kasus OK uji |
| **Risiko/pemilik** | Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

### `FE-RWI-199` — Dua daftar pantau tertunda dan ambang pengaturan

| Field | Isi |
|---|---|
| **Status** | ✅ Selesai (2026-10-05) |
| **Outcome** | Daftar Pantau menampilkan "Serah terima pasca operasi tertunda" dan "Permintaan admisi tertunda" di akhir kelompok episode; Pengaturan Rawat Inap memiliki dua isian ambang dalam menit |
| **Requirement/decision** | `FR-RWF-088`; `RWI-DEC-201`, `RWI-DEC-216`, `RWI-DEC-220` butir 6 |
| **Kontrak** | Frontend 13.1 (perubahan kecil `FE-INP-12`), 13.4.6, 13.7 (`FE-INP-30`); API 11.9 |
| **Reuse** | Daftar Pantau `FE-INP-09` (`app/health-services/inpatient-management/monitoring`); Pengaturan `FE-INP-12` (`…/settings`) |
| **Cakupan** | Dua daftar dengan kolom sesuai skema; tanda "pasien belum di unit tujuan"; tautan baris ke laci `FE-INP-28` dan ke `FE-INP-29` bila layar tujuan sudah ada; dua isian ambang 1–1440 |
| **Dependency** | `BE-RWI-182` [BE] |
| **Acceptance criteria** | 1. Urutan kelompok episode: sesudah `FE-INP-24`, berturut-turut `FE-INT-03`, serah terima tertunda, permintaan admisi tertunda (`RWI-DEC-216`). 2. Kosong → pesan masing-masing daftar. 3. Tanda "pasien belum di unit tujuan" tampil. 4. Ambang di luar 1–1440 ditolak; perubahan ambang mengubah isi daftar. 5. Tanpa `InpatientMonitoring : Read` daftar tidak tampil |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual dengan data melewati ambang |
| **Risiko/pemilik** | Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

### `FE-RWI-200` — Serah Terima Transfer (`P2`)

| Field | Isi |
|---|---|
| **Status** | ✅ Selesai (2026-10-05) |
| **Outcome** | Banner "Serah terima tertunda" di Detail Episode dan ruang kerja keperawatan membuka laci dokumen sembilan bagian. Perawat unit asal mengirim, perawat unit tujuan menerima atau menolak; banner tidak pernah mengunci tombol lain |
| **Requirement/decision** | `FR-RWF-071`; `RWI-DEC-182`, `RWI-DEC-189`; `AC-RWF-071`, `072`; `UAT-RWF-14` |
| **Kontrak** | Frontend 13.4.7 (`FE-INP-31`); API 11.8 |
| **Reuse** | `FE-INP-04` dan `FE-KEP-07`, menggantikan "Integrasi belum tersedia" |
| **Cakupan** | Banner; laci sembilan bagian dengan nilai klinis dari pencatatan terakhir; kirim (membekukan potret); terima atau tolak beralasan; daftar per unit di `FE-INP-09` |
| **Dependency** | `BE-RWI-183` [BE] |
| **Acceptance criteria** | 1. Pasien dipindah ke ICU → banner tampil di kedua layar sampai diterima (`UAT-RWF-14`). 2. Terima/Tolak hanya bagi `TransferHandover : Receive`; tolak wajib alasan. 3. Banner tidak mengunci tombol lain. 4. Tanpa dokumen → banner tidak tampil |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual transfer antarunit |
| **Risiko/pemilik** | `P2`. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

### `FE-RWI-201` — Laporan Rawat Inap dan Laporan Transfer Ruangan (`P2`)

| Field | Isi |
|---|---|
| **Status** | ✅ Selesai (2026-10-05) |
| **Outcome** | Butir menu tingkat dua ke-10 "Laporan Rawat Inap" membuka halaman wadah laporan; isi pertamanya Laporan Transfer Ruangan dengan saringan dan ekspor |
| **Requirement/decision** | `FR-RWF-087`; `RWI-DEC-205`, `214`, `215`, `220` butir 4; `IA-INP-05` (sepuluh butir); `AC-RWF-086`, `100`; `RWI-AC-340`; `UAT-RWF-20`, `UAT-RWF-44` |
| **Kontrak** | Frontend 13.2, 13.4.8, 13.7 (`FE-INP-32`); API 11.7; `02-module-map.md` 7.3 |
| **Reuse** | Peta menu Rawat Inap |
| **Cakupan** | Butir menu (`pathname` usulan `/health-services/inpatient-management/reports`, `DEV_DISCRETION`) yang tampil bila pengguna memegang salah satu permission laporan rawat inap — hari ini `InpatientReport : ReadRoomTransfer`; halaman wadah; laporan dengan periode wajib ≤ 31 hari, unit asal, unit tujuan, kelas; kolom jenis Transfer/Koreksi; tombol Ekspor Excel hanya bagi `InpatientReport : ExportRoomTransfer` |
| **Dependency** | `BE-RWI-184` [BE] |
| **Acceptance criteria** | 1. Butir menu hanya tampil bagi pemegang permission; sidebar Rawat Inap paling banyak sepuluh butir (`UAT-RWF-44`, `RWI-AC-340`). 2. Laporan satu minggu lengkap dan koreksi bertanda (`UAT-RWF-20`). 3. Periode lebih dari 31 hari → pesan `VAL-RWF-90`. 4. Ekspor hanya bagi `ExportRoomTransfer`. 5. Kosong → "Tidak ada transfer pada periode ini." |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual dengan akun berizin dan tanpa izin |
| **Risiko/pemilik** | Kuota butir menu Rawat Inap habis sesudah butir ini (sisa nol). Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |
