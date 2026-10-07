# Laporan Perubahan Frontend — `FE-LAB-47`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-47` |
| Judul | Terima Sampling dan Proses Pemeriksaan pada ketiga daftar |
| Slice | Susulan `S15` (daftar pantau) — amendment pass putaran 22 |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian *Susulan putaran 22* |
| Trace | BR-136 butir 1, 2, 4, 5, 6; `LAB-DEC-189`, `LAB-DEC-191`, `LAB-DEC-190` (Tahan/Lanjutkan); `LAB-FE-030`; `Q-P22-02` (2026-10-07); capability map rev 7 `CAP-P22-01`, `-04`, `-05`, `-06`, `-09`, `-12`, `-22`, `-23` |
| Contract version | `LAB-API-v1` `r39` bagian 7 (`PUT /lab-orders/{id}/start-process`); `LAB-STATE-v1` `r7` bagian 1 — `approved`, tidak diamandemen |
| Wewenang UI | `LAB-FE-030` `DEV_DISCRETION` dibatasi BR-136 — yang dipakai: ikon, urutan butir, bunyi petunjuk nonaktif, bentuk dialog, **dinonaktifkan beralasan** (bukan disembunyikan) bagi status dan izin yang tidak sah, *Terima Sampling* nonaktif hanya pada pesanan selesai/batal |
| Dependency | `FE-LAB-46` ⚠ (layar Wadah menerjemahkan token) — terpenuhi. Backend: nol |
| Klasifikasi | `MEDIUM` — 6 berkas diubah + 2 baru; satu thunk dan keadaan Redux baru; satu dialog baru; nol endpoint baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`; laporan ini beserta status roadmap dan traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `5427ddbe4` (branch `YogaV2`), di atas perubahan `FE-LAB-46` yang belum di-commit |
| Commit backend yang dijadikan rujukan | `171dc314` (branch `yoga`) |
| Tanggal | 2026-10-07 |
| Status | ✅ **`SELESAI`** — keempat AC dan dua AC tambahan terbukti di peramban; `AC-276` terbukti **ujung ke ujung** dengan satu tulis sungguhan yang disetujui pemilik modul |

---

## 1. Keadaan yang ditemukan di awal

| Bukti | Isi |
| --- | --- |
| `lab-order.service.js` | Nol fungsi `start-process` — nol pemanggil di seluruh frontend (`CAP-P22-04`) |
| `lab-monitoring-table-columns.jsx` | Aksi baris: Buka Hasil, Konfirmasi, Batalkan, Nota, Label Lab, Label Goldar |
| `lab-monitoring-slice.jsx#readServerFailure` | Hanya membawa `message`; status HTTP tidak pernah dikirim, sehingga `confirmErrorStatus`/`cancelErrorStatus` selalu kosong (`CAP-P22-05`) |
| `use-lab-monitoring.jsx` | Nol pembacaan izin (`CAP-P22-12`) |
| Backend `LabOrderService.StartProcessAsync` | `Accepted` → `InProcess`; status lain → `400` hari ini, `409` menurut kontrak (`CAP-P22-03`); tidak membaca pembayaran |

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** analis laboratorium, pemegang `LabOrder : Process` dan seluruh aksi wadah.

1. Analis membuka salah satu dari tiga daftar pasien lab dan menekan tombol aksi (⋮) sebuah baris.
2. **Terima Sampling** membuka layar *Wadah dan Pemeriksaan* pesanan itu. Daftar tidak mengirim apa pun
   ke backend; perencanaan, pengambilan, penerimaan, dan keputusan layak dijalankan di layar Wadah.
3. Wadah pertama yang dinyatakan layak memindahkan pesanan ke *Diterima*.
4. Kembali ke daftar, **Proses Pemeriksaan** kini dapat ditekan. Dialog menyebut pasien, nomor pesanan,
   dan pemeriksaannya, serta mengingatkan agar pembayaran pasien Tunai sudah dipastikan (`LAB-OPEN-051`).
5. *Proses Sekarang* mengirim **satu** permintaan. Sesudah backend menjawab berhasil, baris menjadi
   *Sedang Dikerjakan* dan Proses Pemeriksaan tidak dapat ditekan lagi.

**Contoh.** Pesanan Hemoglobin berstatus *Diterima* → analis menekan Proses Pemeriksaan → *Proses
Sekarang* → baris berbunyi *Sedang Dikerjakan* dan butir Proses Pemeriksaan kini redup dengan petunjuk
*"Proses Pemeriksaan hanya sah pada pesanan berstatus Diterima…"*.

**Jalur tidak normal.**

| Keadaan | Yang dilihat petugas |
| --- | --- |
| Pesanan belum *Diterima* (wadah belum layak) | Proses Pemeriksaan redup, petunjuk *"…hanya sah pada pesanan berstatus Diterima, yaitu sesudah wadahnya dinyatakan layak."* |
| Petugas lain memproses lebih dulu (`400` hari ini, `409` sesudah `BE-LAB-89`) | Dialog tetap terbuka: *"Pesanan berstatus InProcess tidak dapat dipindahkan ke InProcess. Muat ulang daftar untuk melihat status terbaru."*; baris **tidak** berubah |
| Pengguna tanpa `LabOrder : Process` | Proses Pemeriksaan redup: *"Anda tidak memiliki izin memproses pesanan laboratorium."* Terima Sampling tetap dapat dipakai |
| Pesanan selesai atau dibatalkan | Terima Sampling redup: *"Pesanan sudah selesai atau dibatalkan."* |
| Tahan/Lanjutkan | Tidak ada — sengaja (`LAB-DEC-190`) |

**Perubahan status.**

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| `Accepted` | Proses Pemeriksaan | `InProcess` | `LabOrder : Process` | Tidak membaca status bayar (`LAB-DEC-189`) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Roadmap `FE-LAB-47`; capability map rev 7; `lab-monitoring-*` (view, kolom, konstanta, slice, hook);
`lab-cancellation-rules.js` dan `lab-confirmation-rules.js` beserta ujinya (pola aturan murni);
`row-action-menu.jsx`; `confirm-modal`; `use-permission.jsx`; `permission-slice.jsx`;
`lab-specimen-constants.jsx`; `private-route-token-utils.js`; backend `LabOrderController.cs`,
`LabOrderService.cs` (`StartProcessAsync`, `MoveOrderStatusAsync`, `ExecuteAsync`), `LabOrderDtos.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/health-services/laboratory-management/lab-order-process-rules.js` | **Baru.** Aturan murni: `resolveStartProcessAction` (hanya `Accepted`; izin dibaca lebih dulu), `resolveReceiveSamplingAction` (nonaktif pada `Completed`/`Cancelled`), `buildStartProcessFailureMessage` (`400`/`409` dibaca sama), `buildStartProcessSummary`. Nol pembacaan status bayar |
| `tests/unit/lab-order-process-rules.test.mjs` | **Baru.** 8 uji |
| `src/lib/services/.../lab-order.service.js` | `startLabOrderProcess(labOrderId)` — `PUT …/start-process` tanpa badan |
| `src/lib/state/slice/.../lab-monitoring-slice.jsx` | `readServerFailure` ikut membawa `statusCode` dan `errors` (**bunyi pesan tidak berubah**); thunk `submitLabOrderStartProcess`; keadaan `processLoading`/`processError`/`processErrorStatus` per disiplin; `clearLabOrderProcessError`; `mergeOrderIntoRow` memperbarui baris **hanya dari jawaban `200`**, dicocokkan dengan `id` jawaban |
| `src/lib/hooks/.../use-lab-monitoring.jsx` | `usePermission("LabOrder", "Process")`; dialog Proses (`openProcess`, `closeProcess`, `submitProcess` — diabaikan selama permintaan berjalan); `openSpecimenWorkspace` — token pesanan lalu `buildLabSpecimenRoute` |
| `src/components/view/.../lab-monitoring-table-columns.jsx` | Butir *Terima Sampling* dan *Proses Pemeriksaan* pada ketiga daftar, nonaktif beralasan |
| `src/components/view/.../lab-monitoring-view.jsx` | Dialog *Proses Pemeriksaan* — pengantar, ringkasan pasien, pesan penolakan; tombol nonaktif selama mengirim |
| `src/lib/hooks/.../use-lab-specimen-workspace.jsx` | **Milik `FE-LAB-46`**, masih belum di-commit |

### 3.3 Kepatuhan arsitektur frontend

Alur `view → hook → slice → service → InstanceAxios` dipertahankan; pola thunk, keadaan per disiplin,
dan pembaruan baris di tempat meniru Konfirmasi (`FE-LAB-15`) dan Batalkan (`FE-LAB-16`). Aturan
keadaan aksi diletakkan pada berkas aturan murni bersebelahan dengan `lab-cancellation-rules.js`.
Nol komponen dasar baru; `RowActionMenu`, `ConfirmModal`, `InformationAlert`, dan `usePermission` dipakai
ulang. Pengantar dialog dirender sebagai **anak**, sebab `ConfirmModal` menuliskan `{children ?? message}`.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tombol *Proses Sekarang* berputar dan nonaktif; klik kedua tidak mengirim apa pun |
| Kosong | Daftar kosong seperti sebelumnya — tidak ada butir aksi |
| Gagal | Pesan di dalam dialog; `400`/`409` disertai *"Muat ulang daftar untuk melihat status terbaru."* |
| Tanpa hak akses | Butir Proses redup berpetunjuk izin; bila izin belum termuat, butir tetap aktif dan backend menjawab `403` (konvensi `usePermission`) |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `/v1/health-services/laboratory-management/lab-orders/{id}/start-process` | Proses Pemeriksaan | `LabOrder : Process` |

*Terima Sampling* tidak memanggil endpoint apa pun; layar Wadah yang memuat datanya (`FE-LAB-46`).

**Kode status yang ditangani:** `200` — baris diperbarui dari `LabOrderDetailResponse`; `400`/`409` —
status sudah berubah, muat ulang; `403` — tanpa izin; selain itu pesan backend apa adanya.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| ESLint 7 berkas yang disentuh | 0 error, 0 warning | `PASS` | Keluaran kosong |
| `npm run lint:errors` | Nol error | `PASS` | Keluaran kosong |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 2483 uji: 2475 lulus, 8 gagal | `PASS` (nol kegagalan baru) | 8 uji baru lulus; 8 kegagalan identik dengan baseline 2026-10-06, seluruhnya di luar Laboratorium |
| `npm run build` | `Compiled successfully in 56s` | `PASS` | Dijalankan saat nol server lokal hidup |
| S1 — ketiga daftar | *Terima Sampling* dan *Proses Pemeriksaan* ada; nol Tahan/Lanjutkan (`AC-281`) | `PASS` | Isi `RowActionMenu` PK, PA, Mikro |
| S2 — keadaan Proses per baris PK | Aktif **tepat** pada baris berstatus *Diterima*; baris lain redup berpetunjuk | `PASS` | 6 baris diperiksa |
| S3 — *Terima Sampling* | Layar Wadah pesanan itu terbuka; **nol** permintaan `/lab-specimens/**` antara klik dan pindah halaman; layar Wadah memuat dengan GUID (`AC-280`) | `PASS` | Pencatat jaringan berstempel waktu |
| S4 — dialog lalu *Batal* | Pengantar dan ringkasan tampil; **nol** `PUT` | `PASS` | Hitungan permintaan |
| S5/S6 — jawaban `400` dan `409` (tiruan) | Satu `PUT`; dialog tetap; pesan muat ulang; baris tetap *Diterima* | `PASS` | Jawaban disuapkan Playwright, tidak diteruskan |
| S7 — jawaban `200` (tiruan berbentuk detail asli) dengan klik ganda | **Tepat satu** `PUT`; dialog tertutup; baris *Sedang Dikerjakan*; Proses kini redup | `PASS` | Detail asli diambil lalu `orderStatus` diubah; muat ulang → tetap *Diterima* (nol tulis) |
| S8 — izin `LabOrder : Process` dicabut | Proses redup *"Anda tidak memiliki izin…"*; Terima Sampling tetap aktif | `PASS` | `/v1/auth/permissions` disuapkan: `isSuperAdmin=false`, `LabOrder:Process` dibuang dari 1624 izin |
| Penjaga tulis dan galat halaman | Nol tulis lain; nol galat JavaScript | `PASS` | Log skrip |
| **S9 — tulis sungguhan** (izin pemilik modul 2026-10-07) | `LAB-RSMMC-000001` *Diterima* → `PUT` diteruskan **sekali** → `200` *"Order laboratorium mulai dikerjakan."*, `orderStatus=InProcess`; dialog tertutup; baris *Sedang Dikerjakan*; **tetap** sesudah muat ulang; Proses redup | `PASS` | Skrip hanya meneruskan `PUT …/5afcc717…/start-process` satu kali |

**Lingkungan:** backend lokal Development (https 7184, biner berisi source `171dc314`), `next dev`
port 3000 diarahkan `.env.local` ke backend lokal, basis data dev bersama. **Akun:** superadmin sebagai
pengganti analis; skenario tanpa izin dibuat dengan menyuapkan daftar izin, karena akun *Dokter Umum*
dan analis tidak tersedia di sesi ini. Kedua server dimatikan sesudah uji; port 3000, 7184, 5107 bebas.

Uji manual: `PASS`.

AUTOMATED TEST: `node --import ./tests/helpers/register.mjs --test tests/unit/` — PASS (8 uji baru lulus; nol kegagalan baru).

**Tidak dijalankan:** akun analis dan *Dokter Umum* asli (sandi tidak tersedia); `AC-277` di peramban
dengan tiga penjamin berbeda — dibuktikan uji unit, karena layar sama sekali tidak membaca penjamin.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-276` — aksi pada ketiga daftar; dapat ditekan hanya pada `Accepted`; satu tekanan terkonfirmasi = tepat satu `PUT`; status baris berubah hanya sesudah `200` | Terpenuhi | S1, S2, S4, S7, **S9 tulis sungguhan** |
| `AC-277` — Tunai, Asuransi, Perusahaan sama-sama dapat ditekan; layar tidak membaca status lunas | Terpenuhi | Uji unit (penjamin 1/2/3, nama, kosong); `lab-order-process-rules.js` nol pembacaan pembayaran |
| `AC-280` — *Terima Sampling* membuka layar Wadah pesanan itu; nol permintaan `/lab-specimens/**` dari daftar | Terpenuhi | S3 |
| `AC-281` — nol Tahan maupun Lanjutkan | Terpenuhi | S1 |
| Tambahan (a) — tanpa `LabOrder : Process` (sesudah izin termuat) tidak dapat menekan Proses | Terpenuhi | S8, uji unit |
| Tambahan (b) — `400` dan `409` meninggalkan baris apa adanya dan menampilkan pesan muat ulang | Terpenuhi | S5, S6, uji unit |
| DoD — uji unit hijau tanpa kegagalan baru; lint dan build hijau; laporan ini | Terpenuhi | Bagian 6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol pada berkas yang disentuh |
| Masalah yang diketahui | (1) Dialog **Konfirmasi** dan **Batalkan** yang sudah ada mengirim prop `message` sekaligus anak, sehingga kalimat pengantarnya tidak pernah tampil (`ConfirmModal` menuliskan `{children ?? message}`) — di luar cakupan, tidak diubah. (2) Status `Confirmed` tampil sebagai *Confirmed* karena `LAB_ORDER_STATUS_LABEL` belum punya labelnya — di luar cakupan. (3) `usePermission` memulangkan *boleh* selama daftar izin belum termuat; backend tetap penjaga terakhir |
| Dependency backend | `BE-LAB-89` (penolakan ke `409`) — **tidak memblokir**; layar sudah membaca `400` dan `409` sama |
| Perubahan sampingan | `readServerFailure` kini ikut membawa status HTTP, sehingga `confirmErrorStatus`/`cancelErrorStatus` mulai terisi. Nol pembaca lain atas kedua ruas itu; bunyi pesan tidak berubah |
| Data dev yang berubah | **`LAB-RSMMC-000001` kini `InProcess`** (2026-10-07, satu `PUT` atas izin pemilik modul). Pesanan ini siap dipakai menguji *Selesaikan* `FE-LAB-48` — masih ada pemeriksaan yang belum dirilis |
| Interupsi | `NONE` |
| Status Git | Frontend: enam berkas `M` (lima milik task ini + `use-lab-specimen-workspace.jsx` milik `FE-LAB-46`), dua berkas baru `??` |
| Langkah berikutnya | `FE-LAB-48` — *Selesaikan* pada daftar PK dan Mikro |
