# Laporan Perubahan Frontend — `FE-HRD-338`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-338` |
| Judul | Tombol Ajukan, label status, dan batal yang menarik pengajuan |
| Slice | `S-D5` amandemen / `MVP-71` — gelombang 2 · `EPIC HRD-123` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 28, kartu `FE-HRD-338` |
| Trace | `FR-TRV-34` (juga layar `FR-TRV-26`, `FR-TRV-27`, `FR-TRV-29`, `FR-TRV-30`); `HRD-DEC-224`, `HRD-DEC-225` |
| Contract version | `v28` 34.1, 34.4, 34.5 — approved 19 November 2026 |
| Wewenang UI | Desain frontend 32.3 (peta status, tombol per status) + `DEV_DISCRETION` untuk posisi tombol, varian badge, dan mode ubah (lihat bagian 3) |
| Dependency | `FE-HRD-337` ✅, `BE-HRD-375` ✅, `BE-HRD-377` ✅ |
| Klasifikasi | `STANDARD` — perubahan view, konstanta, service, dan halaman; uji unit + uji browser dengan data asli |
| Task mode | `FRONTEND` (instruksi pengguna 19 November 2026: "lanjut") |
| Target tulis | Frontend `QuilvianSystemFrontendDev` (6 berkas diubah, 1 berkas uji baru); laporan, roadmap, dan traceability di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `564365595` (branch `YogaV2`) + perubahan working tree task ini |
| Commit backend yang dijadikan rujukan | `815ce085` (branch `yoga`; memuat `BE-HRD-374` s.d. `380`) |
| Tanggal | 19 November 2026 |
| Status | ✅ Selesai — 4/4 acceptance criteria terpenuhi, dibuktikan di browser sebagai pemohon (Dewi) dan atasan (dr. Bima) terhadap backend lokal dan devYoga |

---

## 1. Berkas yang diubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/hr/travel/travel-constants.js` | `TRAVEL_STATUS` menambah `Draft` ("Draf"), `NeedRevision` ("Dikembalikan untuk Revisi"), dan mengganti label `Submitted` menjadi "Menunggu Persetujuan". Ditambah `TRAVEL_EDITABLE_STATUSES`, `canSubmitTravel` (`canSubmit` dari server didahulukan), `travelCancelNote`, serta hak `updateRequest` dan `submitRequest` (= hak membuat, §34.1) |
| `src/lib/services/hr/business-travel-service.js` | `updateTravelRequest` (`PUT /requests/{id}`) dan `submitTravelRequest` (`POST /requests/{id}/submit`). Ini mengganti penolakan "tidak diperlukan; SPPD langsung diajukan" |
| `src/components/view/hr/travel/unified-travel-requests-view.jsx` | Baris daftar: tombol Ajukan (hak membuat dan `canSubmitTravel`) dan Ubah (hak ubah, status Draf/Revisi). Ditambah `ConfirmModal` "Ajukan SPPD ke Atasan" (tombol nonaktif selama permintaan berjalan, galat tampil di modal). Detail: Dibuat/Diajukan/Tahap/Disetujui/Ditolak dan tombol "Ajukan ke Atasan". Konfirmasi batal memuat catatan penarikan/penutupan/pelepasan gembok. Kolom nomor menulis "Dibuat {tanggal}" |
| `src/components/view/hr/travel/unified-travel-create-view.jsx` | Mode ubah lewat `?id=`. Isian dimuat dari detail satu kali; zona, pusat biaya, luar negeri, dan mendesak dikunci. Tombol "Simpan Draf"/"Simpan Perubahan". Layar sukses "SPPD Disimpan sebagai Draf" dengan tombol "Ajukan ke Atasan". Ditambah peringatan saat status Dikembalikan untuk Revisi dan keterangan bila SPPD tidak dapat diubah |
| `src/app/(dashboard)/hrd/travel/requests/create/page.jsx`, `src/app/(dashboard)/corporate/hrd/travel/requests/create/page.jsx` | View dibungkus `<Suspense>` karena `useSearchParams` |
| `tests/unit/business-travel-v28-status.test.mjs` (baru) | 6 uji: label status, aturan Ajukan/Ubah, `canSubmit` dari server, catatan batal, hak, endpoint service |

`unified-travel-dashboard-view.jsx` tidak diubah. Dasbor memakai `TRAVEL_STATUS` yang sama, sehingga label Draf/Revisi ikut berlaku. Isi `AWAITING_SETTLEMENT` (masih memuat `Submitted`) termasuk gerbang `FE-HRD-339`.

---

## 2. Verifikasi

### 2.1 Validasi otomatis

| Perintah | Hasil |
| --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/business-travel-v28-status.test.mjs` | 6/6 lulus |
| `npm run lint:errors` | Exit 0 |
| `npm run build` | "Compiled successfully", 890/890 halaman, termasuk `/hrd/travel/requests/create` dan `/corporate/hrd/travel/requests/create` |

### 2.2 Uji browser (manual terotomasi)

Skrip Playwright di scratchpad menjalankan `next dev` 3000 → backend lokal 7184 → devYoga. Pemohon login sebagai Dewi Anggraeni, atasan sebagai dr. Bima; sandi diberikan pengguna dan tidak disimpan. Hak `BusinessTravelRequest` diberikan sementara, lalu dikembalikan identik di blok `finally` (Dewi 11 izin, Bima 45 izin). Zona tujuan di devYoga kosong dan belum ada API untuk mengisinya, sehingga `GET …/travel/zones` ditiru dan `destinationZoneId` dikosongkan sebelum `POST`.

| Skenario | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Buat SPPD, klik "Simpan Draf" dua kali cepat | Satu `POST` saja; layar sukses "SPPD Disimpan sebagai Draf"; baris `SPPD/RSQ/2026/11/0011` berlabel "Draf" dengan tombol Ajukan dan Ubah | `PASS` (AC1, AC2) | `338-1-draf.png` |
| Ubah dari daftar (`?id=`) | Isian terisi; zona/pusat biaya/luar negeri/mendesak terkunci; `PUT` berhasil; tujuan menjadi "Surakarta" | `PASS` | `338-3-ubah.png` |
| Ajukan saat server menjawab `409` (pencegat) | Pesan server tampil apa adanya di modal; modal tetap terbuka | `PASS` (AC3) | `338-4a-409.png` |
| Ajukan sungguhan | `POST …/submit` dijawab 200; daftar dimuat ulang; badge "Menunggu Persetujuan"; tombol Ajukan/Ubah hilang; tombol nonaktif selama permintaan | `PASS` (AC1, AC2) | Keluaran skrip |
| Atasan (dr. Bima) mengembalikan dari kotak masuk | Status pemohon "Dikembalikan untuk Revisi"; formulir menampilkan peringatan revisi | `PASS` | `338-5-revisi.png` |
| Ajukan ulang dari Detail | Workflow yang sama dilanjutkan; status kembali "Menunggu Persetujuan" | `PASS` | Keluaran skrip |
| Batal saat menunggu persetujuan | Konfirmasi memuat "Pengajuan yang sedang menunggu persetujuan atasan akan ditarik."; SPPD Dibatalkan, workflow Withdrawn/Cancelled | `PASS` | `338-7-batal.png` |
| Tampilan ponsel (390 px) | Daftar dan tombol tetap terbaca | `PASS` | `338-8-ponsel.png` |
| Tombol Setujui/Tolak atau isian anggaran disetujui di layar pemohon | Tidak ada. "Anggaran Disetujui" di detail hanya teks baca | `PASS` (AC4) | Source + browser |

Data uji yang tersisa di devYoga: `SPPD/RSQ/2026/11/0011` dan `0012` (UJI-338), keduanya Dibatalkan.

---

## 3. Wewenang UI yang dipakai (`DEV_DISCRETION`)

| Pilihan | Alasan |
| --- | --- |
| Badge Draf memakai varian `pending`, bukan `inactive` | `inactive` dipakai Dibatalkan; Draf tidak boleh terbaca sebagai SPPD mati |
| Ubah lewat rute formulir yang sama dengan `?id=` | Memakai ulang formulir bertahap tanpa view baru |
| Tombol Ajukan di baris daftar, di Detail, dan di layar sukses formulir | Ketiganya memanggil satu `ConfirmModal`; Detail ditutup dulu agar tidak ada modal bertumpuk |

---

## 4. Delta kontrak dan temuan

| Temuan | Dampak | Tindak lanjut |
| --- | --- | --- |
| `UpdateBusinessTravelRequestDto` tidak memuat zona, pusat biaya, luar negeri, dan mendesak; server tidak menghitung ulang uang harian | Isian itu dikunci di mode ubah. Uang harian dikirim dari estimasi layar bila tarif pasti, selain itu memakai nilai lama | Dicatat untuk pass blueprint (API 34.1). Bila pengguna ingin zona dapat diubah, butuh task backend |
| Zona tujuan dan tarif di devYoga kosong, tanpa API isi | Uji browser meniru daftar zona | Sama dengan temuan `BE-HRD-375` (master data) |
| Kolom tabel daftar tidak membawa `canSubmit` | Baris dinilai dari status; Detail memakai `canSubmit` server (SPPD lama `Submitted` tanpa workflow, `HRD-DEC-227`) | Tidak perlu tindakan |

---

## 5. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Label dan tombol per status sesuai 32.3; status tak dikenal tampil apa adanya | Terpenuhi | Uji unit + browser (`resolveTravelInfo` mengembalikan kode mentah) |
| 2. Ajukan memuat ulang baris dari server; tombol nonaktif selama permintaan | Terpenuhi | Browser |
| 3. Galat `409`/`400` tampil apa adanya | Terpenuhi | `338-4a-409.png` |
| 4. Tidak ada tombol Setujui/Tolak dan tidak ada isian anggaran disetujui | Terpenuhi | Source + browser |
| Build dan lint | Lulus | Bagian 2.1 |

---

## 6. Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko tersisa | Isian mode ubah terbatas pada DTO ubah (bagian 4) |
| Penyegaran blueprint | Desain frontend 32.1 (`travel-display-utils.js`, data contoh) usang; peta status kini di `travel-constants.js` |
| Git | Tidak ada stage, commit, push, pull, merge, rebase, atau deploy |
| Task berikutnya | `FE-HRD-339` (gerbang uang muka/settlement, label "Perjalanan Dinas" di kotak masuk) |
