# Laporan Perubahan Frontend — `FE-MCU-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-MCU-007` |
| Judul | Daftar Pasien MCU |
| Slice | `MVP-1` (pasangan `BE-MCU-010` ✅) |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian *`FE-MCU-007`* |
| Trace | `FR-MCU-037` (tampilan); `MCU-FE-003`; brief layar [`05-screen-brief.md`](../../../05-screen-brief.md) bagian 2, 3, 5.1, 5.5 |
| Contract version | `MCU-API-v1` grup `Mcu Order` (GET: `filters/metadata`, `summary`, `/`, `/{id}`, `/{id}/history`) — `approved` 2026-10-21; dilayani `McuOrderController`/`McuOrderQueryService` |
| Wewenang UI | Brief layar `FE-MCU-000` ✅ (keputusan pemilik 2026-10-21) |
| Dependency | `FE-MCU-000` ✅, `FE-MCU-001` ✅, `FE-MCU-006` ✅, `BE-MCU-010` ✅ |
| Klasifikasi | `LARGE` — satu repository, ±12 berkas baru, dua halaman (daftar + kerangka detail), aturan privasi (NIK) dan aksi per status/izin, kontrak yang sudah ada, tanpa database |
| Task mode | `FRONTEND` |
| Target tulis | Frontend: route, view, hook, utilitas, konstanta Daftar Pasien MCU dan detail order, menu, uji unit + E2E; satu perbaikan ketahanan spec paket. Backend: laporan ini, revisi kecil laporan `FE-MCU-006`, tanda roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `f7f60e34c` (branch `YogaV2`) |
| Commit backend yang dijadikan rujukan | `a3b63b50` (branch `yoga`) |
| Tanggal | 2026-10-27 |
| Status | Selesai — ketiga kriteria terbukti di E2E API-tiruan; UAT baca terhadap backend lokal lulus |

---

## 1. Keadaan yang ditemukan di awal

- Lapisan data `FE-MCU-006` ✅ (slice `mcuOrder`) dan aturan status → aksi `FE-MCU-001` ✅ tersedia.
- Backend (`McuOrderQueryService`):
  - `GET orders` **tanpa `date` mengembalikan semua tanggal**; hanya `GET orders/summary` yang memakai hari ini
    sebagai bawaan. Komentar konstanta `FE-MCU-006` sempat menyebut sebaliknya dan diperbaiki (dicatat di laporan
    `FE-MCU-006`).
  - Opsi status/sumber di `filters/metadata` bernilai **nama enum** (`Scheduled`, `DirectBooking`), bukan angka.
  - `maskedNik` disamarkan server (`McuLabels.MaskNik`).
- Peta route brief menempatkan `…/mcu-patients/[orderId]` (detail order) pada `FE-MCU-007` bersama `011`..`014`,
  dan *klik baris* harus membuka detail. Karena itu task ini memasang **kerangka detail**: header + tab *Ringkasan*
  dan *Riwayat*.
- Layar Booking, *Walk-in*, Check-in, Jadwal Ulang, dan Batal belum ada (`FE-MCU-008`, `009`).

---

## 2. Proses bisnis dari sisi pengguna

1. **Menu** *Medical Check-Up › Daftar Pasien MCU* (`McuOrder : Read`), paling atas di grup.
2. **Tanggal** — bawaan hari ini (WIB) dan selalu dikirim. Menghapus tanggal mengembalikannya ke hari ini, dan
   mengganti tanggal memuat ulang ringkasan dan tabel.
3. **Sembilan kartu status** dari `summary` (Terjadwal … Dibatalkan). Klik kartu menyaring status itu; klik lagi
   melepas saringan.
4. **Penyaring** — Status, Sumber, Perusahaan, Dokter MCU (opsi dari `filters/metadata`), cari (nama/No. RM/No.
   Order), jumlah baris, dan reset.
5. **Tabel** — No, No. Order, No. RM (*Belum terdaftar* bila identitas sementara), Nama Peserta, **NIK tersamar**,
   Paket MCU, Perusahaan (Penjamin), Dokter MCU, Kelengkapan (*n dari m*), dan Status.
   - Kosong: *"Belum ada peserta MCU pada tanggal ini."*
   - Gagal muat: pesan server + *Coba lagi*.
   - `403`: halaman akses ditolak.
6. **Menu ⋮** menurut `FE-MCU-001`: *Buka Detail* selalu ada. *Terjadwal* hari ini menampilkan Check-in, Jadwal
   Ulang, dan Batal; *Tidak Hadir* menampilkan Jadwal Ulang; selebihnya hanya Buka Detail. Aksi yang layarnya belum
   ada tampil **nonaktif dengan keterangan *Fitur belum tersedia***; `FE-MCU-008`/`009` cukup memasang fungsinya
   (`actionHandlers`).
7. **Detail order** (klik dua kali baris atau *Buka Detail*; URL bertoken, tanpa UUID):
   - **Header** memuat No. Order dan nama, RM, NIK utuh (detail berizin), paket, penjamin (*Perorangan* atau
     perusahaan), dokter, tanggal, sumber, lencana status, satu aksi utama, dan menu *Aksi lain*, semuanya menurut
     `FE-MCU-001`. *Buka Pemeriksaan*/*Buka Resume* memindah tab; aksi lain *Fitur belum tersedia*.
   - **Ringkasan**: peserta, paket dan penjamin (tarif dan harga terpakai, dasar mulai, kelengkapan), kunjungan
     (No. kunjungan, check-in, dokter, mulai pemeriksaan, alasan batal), dan tabel penandaan persyaratan saat
     check-in.
   - **Riwayat**: waktu (WIB), perpindahan status atau penggantian dokter, pelaku, dan alasan.
   - **Pemeriksaan / Resume / Penyerahan**: keterangan *Fitur belum tersedia* sampai `FE-MCU-011`..`013`.
   - Gagal muat → *Coba lagi*; `403` → akses ditolak; tombol *← Daftar Pasien MCU*.

Tombol *+ Booking MCU* dan *+ Walk-in* dipasang bersama layarnya (`FE-MCU-008`, `009`).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/health-services/medical-checkup-management/mcu-patients/page.jsx`, `mcu-patients-client.jsx`, `[orderId]/page.jsx` (baru) | Route daftar dan detail; token `booking`/`walk-in` dicadangkan untuk `FE-MCU-008`/`009` |
| `src/components/view/…/mcu-patients/mcu-patient-list-view.jsx` (baru) | Daftar Pasien MCU |
| `src/components/view/…/mcu-patients/detail/mcu-order-detail-view.jsx` (baru) | Kerangka detail order |
| `src/lib/hooks/…/mcu-patients/use-mcu-patient-list.jsx`, `use-mcu-order-detail.jsx` (baru) | Logika daftar dan detail |
| `src/utils/…/mcu-patients/mcu-patient-utils.js` (baru) | Kartu ringkasan, teks sel (NIK tersamar idempoten), query bertanggal, opsi metadata, aksi daftar/header, waktu WIB, riwayat |
| `src/lib/constants/…/mcu-patients/mcu-patient-constants.js` (baru) | Config, kolom, tab, kalimat brief |
| `src/lib/constants/…/medical-checkup-constants.js` | Koreksi komentar `MCU_ORDER_DEFAULT_FILTERS` (revisi `FE-MCU-006`) |
| `src/utils/menu-sidebar/menu-items.jsx` | Butir *Daftar Pasien MCU* (`RiFolderUserLine` yang sudah diimpor) |
| `tests/unit/mcu-patient-list-rules.test.mjs` (baru) | 7 uji |
| `tests/e2e/mcu-patient-list-screen.spec.mjs` (baru) | 6 skenario API-tiruan |
| `tests/e2e/mcu-package-screen.spec.mjs` | Ketahanan: klik *Perbarui Paket MCU* memakai `click()` Playwright (lihat §6) |

### 3.2 Kepatuhan arsitektur frontend

- Alur `view → hook → slice (FE-MCU-006) → service → InstanceAxios`.
- Komponen yang sudah ada: `Hero`, `SummaryGrid` (kartu ber-`onClick`/`active`), `DataFilter` (juga sebagai strip
  tab detail lewat `tabs`, dengan bilah kontrol kosong disembunyikan `controlsClassName="d-none"`), `FilterDatePicker`,
  `FilterSelect`, `DataTable`, `RowActionMenu` (item nonaktif + keterangan), `StatusBadge`, `BaseDetailSection`, dan
  `InformationAlert`.
- Baris info detail memakai grid Bootstrap `dl`. Tidak ada CSS Module, komponen, atau dependency baru.
- Izin memakai `selectHasPermission` (perilaku sama dengan `usePermission` layar MCU lain) sebagai `hasPermission`
  aturan `FE-MCU-001`.

---

## 4. State yang ditangani di layar

| State | Perilaku |
| --- | --- |
| Memuat | Kerangka kartu ringkasan, penanda muat tabel, *Mengambil detail order MCU...* |
| Kosong | *"Belum ada peserta MCU pada tanggal ini."*; detail: *Belum ada penandaan persyaratan.* / *Belum ada riwayat.* |
| Gagal muat | Pesan server + *Coba lagi* (daftar dan detail); ringkasan gagal tampil sebagai peringatan tanpa menutup tabel |
| Tanpa hak akses | `403` → halaman akses ditolak; aksi menurut izin (`FE-MCU-001`) |
| Privasi | NIK daftar selalu lewat `maskMcuNik` (idempoten), walau server mengirim NIK utuh; NIK utuh hanya di detail berizin |
| Aksi belum tersedia | Item/ tombol nonaktif dengan keterangan *Fitur belum tersedia* |

---

## 5. Endpoint yang dikonsumsi

Base URL: `/v1/health-services/medical-checkup-management/orders` — `GET /filters/metadata`, `GET /summary?date`,
`GET /?date&status&source&companyGuarantorId&mcuDoctorId&search&pageNumber&pageSize`, `GET /{id}`, `GET /{id}/history`;
semuanya `McuOrder : Read` (rincian di [laporan `FE-MCU-006`](FE-MCU-006.md) §5).

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `tests/unit/mcu-patient-list-rules.test.mjs` | 7/7 lulus | `PASS` | `# pass 7`, `# fail 0` |
| Uji mutasi NIK | Sel NIK diganti `maskedNik` mentah → uji NIK **gagal**; dipulihkan identik (`cmp`) | `PASS` | Keluaran `node --test` |
| E2E API-tiruan `tests/e2e/mcu-patient-list-screen.spec.mjs` | 6/6 lulus (diulang sesudah build kedua: 6/6) | `PASS` | `6 passed` |
| **Kriteria 1** — kosong | *"Belum ada peserta MCU pada tanggal ini."* tampil | `PASS` | Skenario *kosong…* |
| **Kriteria 2** — gagal muat | `500` → pesan server + *Daftar Pasien MCU belum dapat ditampilkan.*; *Coba lagi* memulihkan tabel; `403` → akses ditolak (daftar dan detail) | `PASS` | Skenario *kosong…*, *detail: 403…* |
| **Kriteria 3** — NIK tidak pernah utuh di daftar | Server tiruan **sengaja mengirim NIK utuh** `3202230101900007` → sel `3202********0007`; teks NIK utuh tidak ada di seluruh halaman | `PASS` | Skenario *daftar…* |
| Daftar | Permintaan awal `date=hari ini`; ringkasan `date=hari ini`; baris persis sesuai kolom brief; kartu 12/0/5… dan klik *Hadir* → `status=CheckedIn` + kartu aktif; penyaring gabungan (status + sumber + perusahaan + dokter + cari) terkirim persis; reset kembali ke hari ini; ganti tanggal memuat ulang ringkasan dan tabel | `PASS` | Skenario *daftar…* |
| Menu ⋮ | *Terjadwal* hari ini: Buka Detail + Check-in/Jadwal Ulang/Batal nonaktif *Fitur belum tersedia*; *Dalam Pemeriksaan*: Buka Detail saja; izin baca saja: Buka Detail saja | `PASS` | Skenario *menu ⋮…* |
| Detail | URL bertoken; header dengan NIK utuh, *Perorangan*, dokter; Ringkasan (Lunas, Rp 350.000, kunjungan, check-in WIB, catatan persyaratan); aksi lain *Batal Penuh*/*Ganti Dokter MCU* nonaktif; *Buka Pemeriksaan* memindah tab; Riwayat (perpindahan status, penggantian dokter, waktu WIB); kembali ke daftar | `PASS` | Skenario *detail…* |
| Regresi seluruh spec MCU (paket, referensi, jadwal, daftar) | 33/33 lulus, dua kali berturut-turut | `PASS` | `33 passed` ×2 |
| UAT baca, backend lokal + devYoga (semua tulis diblokir) | `orders?date=hari ini`, `summary`, `filters/metadata` → `200`; opsi asli (9 status, 7 sumber, PT PLN Persero; dokter kosong karena belum ada order); devYoga belum punya order MCU → kalimat kosong tampil; nol tulis, nol GET gagal | `PASS` | Log skrip + tangkapan layar |
| Perbaikan tampilan dari UAT | Label kartu terpotong (*"DALAM PEMERIKSAA N"*) pada lebar minimum 140 px → 180 px; kartu tersusun 6 + 3 dengan label utuh (dibuktikan tangkapan layar sesudah build ulang) | `PASS` | Tangkapan layar |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 3.882 uji: 3.840 lulus, 42 gagal — daftar nama **identik** dengan baseline `FE-MCU-006` (semua di luar MCU) | `EXISTING / ENVIRONMENT ISSUE` | `diff` kosong |
| `npm run lint:errors` | Exit 0 | `PASS` | Keluaran perintah |
| `npm run build` | Exit 0: *Compiled successfully*, kedua route terbentuk | `PASS` | Keluaran build |

**Ketahanan spec paket.** Pada regresi pertama, uji *ubah paket* (`FE-MCU-003`) gagal sekali: klik koordinat
(`klikTengah`) pada *Perbarui Paket MCU* meleset karena tata letak bergeser di bawah beban dua worker. Uji itu lulus
4/4 bila dijalankan sendiri. Langkahnya diganti `click()` Playwright (menunggu tombol stabil dan aktif) sesudah
tombol digulir ke tengah. Seluruh 33 uji MCU kemudian lulus dua kali berturut-turut. Kode layar paket tidak diubah.

`MANUAL TEST: PASS` — tanggal, kartu status, keempat penyaring + cari + reset, paginasi/jumlah baris, menu ⋮, klik
dua kali, tab detail, dan aksi utama dijalankan lewat peramban (Playwright) pada E2E dan UAT baca.
`AUTOMATED TEST: npm run test:unit — FAIL (42 kegagalan existing di luar MCU; 7/7 uji baru PASS)`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Kosong menampilkan *"Belum ada peserta MCU pada tanggal ini."* | Terpenuhi | §6 Kriteria 1 |
| Gagal muat menampilkan *Coba lagi* | Terpenuhi | §6 Kriteria 2 |
| NIK tidak pernah tampil utuh di daftar | Terpenuhi | §6 Kriteria 3 |
| DoD: kriteria terbukti | Terpenuhi | §6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Aksi khusus dokter yang ditetapkan (mis. *Buka Ulang*) belum ditampilkan di header detail, karena pemetaan akun ↔ dokter MCU (`isAssignedDoctor`) datang bersama `FE-MCU-011` |
| Masalah yang diketahui | Pilihan *Dokter MCU* di penyaring kosong bila belum ada order yang berdokter (metadata server diturunkan dari order). Bukti dengan order sungguhan menunggu layar booking/check-in (`FE-MCU-008`, `009`) |
| Dependency backend | Tidak ada; `BE-MCU-010` ✅ |
| Perubahan sampingan | Komentar konstanta `FE-MCU-006` dikoreksi; langkah klik di spec paket dibuat tahan beban |
| Interupsi | `NONE` |
| Status Git (frontend) | ` M medical-checkup-constants.js`, ` M store.jsx`, ` M mcu-request-rules.js` (dari `FE-MCU-006`), ` M menu-items.jsx`, ` M tests/e2e/mcu-package-screen.spec.mjs`; `??` folder route/view/hook/konstanta/utilitas `mcu-patients`, service/slice order, dan uji baru |
| Langkah berikutnya | `FE-MCU-008` (Booking, jadwal ulang, batal) — siap |
