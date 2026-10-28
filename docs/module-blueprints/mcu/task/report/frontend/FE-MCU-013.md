# Laporan Perubahan Frontend — `FE-MCU-013`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-MCU-013` |
| Judul | Verifikasi, cetak, persetujuan, penyerahan |
| Slice | `MVP-3` (pasangan `BE-MCU-030`, `033`, `034`) |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian *`FE-MCU-013`* |
| Trace | `FR-MCU-042`, `043`, `050`..`053`; `UAT-15`, `UAT-18`, `UAT-19`; brief layar `05-screen-brief.md` §5.5, §5.7, §5.8 |
| Contract version | `MCU-API-v1` grup `Mcu Resume` (`current/verify`, `current/return`) dan `Mcu Delivery` (7 endpoint) — `approved` 2026-10-21. Kalimat `VAL-MCU-31`..`33`, `39`, `40`, `46` dibaca dari `McuDeliveryService` dan `McuResumeService` |
| Wewenang UI | Brief layar `MCU-FE-002` (approved): tombol *Verifikasi*/*Kembalikan ke Dokter* di tab Resume, tab Penyerahan (Cetak, persetujuan pelepasan, riwayat penyerahan), modal *Catat Penyerahan*, *Catat/Cabut Persetujuan*, *Catat* pemberitahuan koreksi. Tata cetak memakai lembar cetak Lab yang ada (brief 5.8) |
| Dependency | `FE-MCU-012` ✅, `FE-MCU-010` ✅ (lapisan data); backend `BE-MCU-030` ✅, `033` ✅, `034` ✅ |
| Klasifikasi | `HIGH` — berkas diubah > 10, logika sedang–tinggi (hak menurut status + izin + dokter pemberi approve, persetujuan, versi), privasi tinggi (`MCU-FE-003`), UI baru termasuk dokumen cetak |
| Task mode | `FRONTEND` |
| Target tulis | Frontend: konstanta, aturan, hook, view, dokumen cetak, uji. Backend: laporan ini; tanda roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `f7f60e34c` (branch `YogaV2`) |
| Commit backend yang dijadikan rujukan | `1b7ff563` (branch `yoga`) |
| Tanggal | 2026-10-28 |
| Status | Selesai — `UAT-15`, `18`, `19` terbukti di E2E API-tiruan; sisi server dan payload dibuktikan terhadap backend lokal tanpa tulis. Uji dengan data sungguhan menjadi batas verifikasi (devYoga belum punya order *Disetujui Dokter* ke atas) |

---

## 1. Keadaan yang ditemukan di awal

- Thunk verifikasi, kembalikan, dokumen cetak, penyerahan, pemberitahuan koreksi, dan persetujuan sudah ada sejak
  `FE-MCU-010`. Reducer `clearMcuResultDocuments` juga sudah ada.
- Header detail menampilkan *Verifikasi*, *Kembalikan ke Dokter*, dan *Catat Penyerahan* nonaktif *Fitur belum tersedia*.
  Tab Penyerahan masih kosong.
- Aturan server yang menentukan layar:

  | Aksi | Syarat server (urut) |
  | --- | --- |
  | Verifikasi | Versi → order *Disetujui Dokter* dengan resume *Disetujui* → bukan dokter pemberi approve (`VAL-MCU-39`, `403`) → hasil Lab/Rad komponen selesai masih tersedia di modulnya |
  | Kembalikan | Alasan wajib (`VAL-MCU-40`) → versi → resume *Disetujui*. **Tanpa** larangan `VAL-MCU-39` |
  | Data cetak | Order belum dibatalkan dan resume *Disetujui*/*Terverifikasi*. Ringkasan hanya 12 ruas (`AT-MCU-42`) |
  | Catat penyerahan | Penerima/isi/saluran → tujuan (`VAL-MCU-33`) → catatan ≤ 500 → versi → resume terverifikasi (`VAL-MCU-32`) → penerima perusahaan hanya order perusahaan → isi lengkap ke perusahaan butuh persetujuan berlaku (`VAL-MCU-31`). Penyerahan pertama memindah order ke *Diserahkan* |
  | Persetujuan | Bentuk bukti → formulir kertas wajib bernomor → waktu tidak di masa depan → versi → order perusahaan → belum ada persetujuan berlaku |
  | Pemberitahuan koreksi | Waktu tidak di masa depan → hanya penyerahan bertanda perlu, sekali |

- Respons resume hanya memuat **nama** dokter pemberi approve, tanpa id. `isApprovingDoctor` di aturan header belum
  pernah diisi.

---

## 2. Proses bisnis dari sisi pengguna

1. **Verifikasi (`UAT-15`).** Pada order *Disetujui Dokter*, petugas MCU melihat *Verifikasi* sebagai aksi utama header,
   serta *Kembalikan ke Dokter* di menu ⋮. Keduanya juga tersedia di tab Resume.
   - Isi resume tetap baca-saja bagi petugas.
   - Modal Verifikasi menyebut versi dan dokter pemberi approve, dan mengingatkan bahwa isi klinis tidak dapat diubah.
   - Penolakan server tampil apa adanya, mis. *"Hasil Thorax PA belum tersedia di modulnya."*
   - Sesudah berhasil: order *Terverifikasi*, jejak *Diverifikasi Dewi*, dan aksi utama header menjadi *Catat
     Penyerahan*.
2. **Kembalikan ke Dokter** — alasan wajib, mis. *Kategori kelayakan tidak sesuai hasil Thorax*. Resume kembali *Draf*,
   order *Menunggu Resume*, dan alasan tampil sebagai *Dikembalikan petugas* di atas draf.
3. **Dokter pemberi approve** tidak melihat *Verifikasi* (`VAL-MCU-39`), tetapi tetap melihat *Kembalikan ke Dokter*
   karena server tidak membatasinya.
4. **Tab Penyerahan.**
   - Keterangan menurut status: sebelum resume disetujui, *"Hasil dapat dicetak sesudah resume disetujui dokter, dan
     diserahkan sesudah diverifikasi petugas."*; pada *Disetujui Dokter*, cetakan hanya untuk pemeriksaan internal.
   - **Cetak Hasil Lengkap** / **Cetak Ringkasan Kelayakan** membuka pratinjau data server di lembar cetak Lab, lalu
     *Cetak* lewat `react-to-print`.
     - Hasil lengkap memuat identitas, kesimpulan dokter, nilai Lab bertanda `*` bila di luar rujukan, laporan Rad,
       isian Pemeriksaan MCU, dan tanda tangan.
     - Ringkasan hanya memuat kategori kelayakan dan rekomendasi kerja, dengan catatan bahwa nilai dan diagnosis tidak
       dicantumkan; tanpa No. RM dan tanggal lahir.
     - Resume yang belum terverifikasi dicetak bertanda *BELUM DIVERIFIKASI — tidak untuk diserahkan*.
   - **Persetujuan pelepasan** (hanya order perusahaan): *"Persetujuan pelepasan ke PT Contoh: Belum ada"* atau
     *"Berlaku sejak … · Formulir kertas FRM-… · dicatat Dewi"*, dengan *Catat Persetujuan* / *Cabut Persetujuan* dan
     jejak pencabutan. Ada keterangan bahwa bentuk bukti final menunggu `MCU-OQ-003`.
   - **Riwayat penyerahan**: Waktu · Penerima · Isi · Saluran · Tujuan · Versi · Petugas · Pemberitahuan Koreksi
     (*⚠ Perlu* + *Catat*, *Dicatat …*, atau —). Tidak ada tombol Kirim otomatis (`MCU-DEC-027`).
5. **Catat Penyerahan (`UAT-18`).**
   - Isian: Penerima (Peserta; Perusahaan hanya untuk order perusahaan), Isi (Hasil lengkap/Ringkasan kelayakan),
     Saluran, Tujuan, dan Catatan.
   - Tujuan berlabel sesuai saluran (*Nomor WhatsApp*, *Nomor Telepon*, *Alamat Email*) dan tersembunyi untuk *Ambil
     langsung*. Kalimat validasinya sama dengan server, mis. *"Isi nomor WhatsApp tujuan."*
6. **Isi lengkap ke perusahaan tanpa persetujuan (`UAT-19`).** Layar tidak mencegah; server menolak dan pesannya tampil
   apa adanya: *"Peserta belum menyetujui pelepasan hasil lengkap kepada PT Contoh. Serahkan ringkasan kelayakan."* Tombol
   *Ganti ke Ringkasan Kelayakan* mengubah isi dengan satu klik.
7. **Pemberitahuan koreksi.** Sesudah resume dibuka ulang, baris versi lama bertanda *⚠ Perlu*. *Catat* membuka modal
   yang menyebut penerima, saluran, tujuan, dan versi, dengan waktu opsional (kosong = sekarang).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/.../mcu-patients/mcu-delivery-constants.js` (baru) | Batas panjang, toleransi waktu server 5 menit, varian cetak, teks keterangan, pesan cadangan |
| `src/utils/.../mcu-patients/mcu-delivery-utils.js` (baru) | `resolveMcuDeliveryAccess`, `describeMcuDeliveryState`, `isMcuCompanyOrder`, waktu WIB → UTC, baris penyerahan, persetujuan berlaku, validasi + body penyerahan/persetujuan/pencabutan/pemberitahuan, `isMcuConsentRequiredFailure`, daftar putih ringkasan (`pickMcuFitnessSummary`), baris hasil cetak, judul berkas cetak |
| `src/utils/.../mcu-patients/mcu-resume-utils.js` | `resolveMcuResumeAccess` + `returnToDoctor` dan `verify` |
| `src/lib/hooks/.../mcu-patients/use-mcu-resume.jsx` | Modal *Verifikasi* dan *Kembalikan ke Dokter* |
| `src/lib/hooks/.../mcu-patients/use-mcu-delivery.jsx` (baru) | Muat riwayat penyerahan dan persetujuan saat tab aktif (persetujuan hanya order perusahaan), pratinjau cetak (dokumen dibuang dari store saat ditutup/ditinggal), empat modal |
| `src/lib/hooks/.../mcu-patients/use-mcu-order-detail.jsx` | `isApprovingDoctor` dalam konteks aksi; handler header `verifyResume`, `returnResume`, `recordDelivery` (pindah tab + buka modal) |
| `src/components/view/.../mcu-patients/delivery/mcu-delivery-tab.jsx` (baru) | Tab Penyerahan |
| `src/components/view/.../mcu-patients/delivery/mcu-delivery-modals.jsx` (baru) | Pratinjau cetak + `useReactToPrint`, *Catat Penyerahan*, *Catat/Cabut Persetujuan*, *Catat* pemberitahuan koreksi |
| `src/components/view/.../mcu-patients/delivery/mcu-result-print-document.jsx` (baru) | `McuResultPrintDocument` dan `McuFitnessSummaryPrintDocument` memakai `lab-order-print.module.css` (tanpa CSS baru) |
| `src/components/view/.../mcu-patients/detail/mcu-resume-tab.jsx`, `mcu-resume-modals.jsx` | Tombol dan modal *Kembalikan ke Dokter* / *Verifikasi* |
| `src/components/view/.../mcu-patients/detail/mcu-order-detail-view.jsx` | Tab Penyerahan dan modalnya dipasang; penolong *Fitur belum tersedia* untuk tab dihapus |
| `tests/unit/mcu-delivery-screen-rules.test.mjs` (baru) | 7 uji |
| `tests/unit/mcu-resume-screen-rules.test.mjs` | Pembanding hak resume dibatasi ke tiga hak `FE-MCU-012` (perubahan disengaja: dua hak baru); satu argumen salah tempat diperbaiki |
| `tests/e2e/mcu-delivery-screen.spec.mjs` (baru) | 5 skenario API-tiruan |

### 3.2 Kepatuhan arsitektur frontend

- Alur `view → hook → slice → service` dengan thunk `FE-MCU-010`, tanpa thunk atau service baru.
- **Ringkasan kelayakan tidak pernah disusun di browser** (`MCU-FE-003`):
  - Sumbernya hanya `variant=fitness-summary`.
  - Dokumennya merender ruas daftar putih (`pickMcuFitnessSummary`). Ruas lain dari server dibuang.
  - Membuka ringkasan tidak meminta `variant=full` (E2E).
- Cetak memakai pola Lab yang sudah ada: pratinjau di `ConfirmModal` dengan `previewModal`/`previewBody`, `react-to-print`
  v3 (`contentRef`), dan logo `react-bootstrap` `Image`.
- Aturan murni terpisah dan diuji unit; aksi header tetap lewat `resolveMcuOrderActions` (`FE-MCU-001`).
- Tidak ada dependency, pola state, komponen dasar, atau CSS baru.

---

## 4. State yang ditangani

| State | Perilaku |
| --- | --- |
| Memuat | Tabel *"Mengambil riwayat penyerahan..."*; persetujuan *"Mengambil persetujuan..."*; pratinjau *"Mengambil data cetak dari server..."* dengan *Cetak* nonaktif sampai data tiba |
| Kosong | *"Belum ada penyerahan hasil."* |
| Gagal | Riwayat/persetujuan: pesan + *Coba lagi*; data cetak: pesan server di pratinjau; aksi: pesan server di modal |
| Kiriman ganda | Tombol modal *Menyimpan…*/*Memproses…* nonaktif selama permintaan |
| Validasi | Kalimat server sebelum kirim: `VAL-MCU-33`, `40`, bentuk bukti, nomor formulir, panjang, waktu tidak di masa depan |
| Tanpa hak akses | Tanpa `McuDelivery : Read` → keterangan dan nol permintaan riwayat (E2E); tombol per izin (`Create`, `ManageConsent`, `McuResume : Verify`); `403` server tampil apa adanya |
| Data basi | `409` memuat ulang detail + data grup (`FE-MCU-010`); kiriman berikutnya memakai versi terbaru |
| Privasi | Dokumen cetak tidak bertahan di store; ringkasan tanpa nilai/diagnosis/No. RM/tanggal lahir; tujuan penyerahan hanya di detail berizin; judul berkas cetak tanpa nama peserta |
| Aksesibilitas | Label setiap isian; tombol *Catat* per baris ber-`aria-label` (penerima + waktu); ringkasan persetujuan `aria-live` |

---

## 5. Endpoint yang dikonsumsi

| Method | Path | Dipakai untuk |
| --- | --- | --- |
| `POST` | `orders/{id}/resumes/current/verify`, `/current/return` | Verifikasi, kembalikan |
| `GET` | `orders/{id}/result-document?variant=full` / `fitness-summary` | Pratinjau cetak |
| `GET` / `POST` | `orders/{id}/deliveries` | Riwayat, catat penyerahan |
| `PATCH` | `orders/{id}/deliveries/{deliveryId}/correction-notice` | Pemberitahuan koreksi |
| `GET` / `POST` | `orders/{id}/release-consents` | Persetujuan pelepasan |
| `PATCH` | `orders/{id}/release-consents/{consentId}/revoke` | Cabut persetujuan |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `tests/unit/mcu-delivery-screen-rules.test.mjs` + `mcu-resume-screen-rules.test.mjs` | 16/16 lulus. Cakupan: hak verifikasi/kembalikan + header, hak tab per status/izin/perusahaan, daftar putih ringkasan (12 ruas, `HBsAg`/No. RM dibuang), `VAL-MCU-33` per saluran, body penyerahan persis DTO, `VAL-MCU-31` dikenali, persetujuan (WIB → UTC, masa depan, ringkasan berlaku), pemberitahuan koreksi | `PASS` | `# pass 16` |
| Kontrol negatif | (1) Ringkasan meneruskan seluruh ruas → uji 3 gagal. (2) Verifikasi tanpa syarat `isApprovingDoctor` → uji 1 gagal. Berkas dipulihkan (`cmp` sama) → 16/16 | `PASS` | Keluaran perintah |
| **`UAT-15`** E2E | Dewi: (1) tab Resume menampilkan *Kembalikan ke Dokter*; isian nonaktif. (2) Header *Verifikasi* → modal *"disetujui dr. Rina"*. (3) Kiriman pertama → pesan server *"Hasil Thorax PA belum tersedia di modulnya."*; kiriman ulang → *Terverifikasi*, *Diverifikasi Dewi*, aksi utama *Catat Penyerahan*. Dua `POST verify` `{ version: 5 }` | `PASS` (tiruan) | Spec skenario 1 |
| Kembalikan + `VAL-MCU-39` E2E | (1) Alasan kosong → *"Alasan wajib diisi."*, nol tulis. (2) Dengan alasan → `POST return` `{ reason, version: 5 }`; *Menunggu Resume*; *Dikembalikan petugas* + alasan. (3) dr. Rina (pemberi approve) atas resume *Disetujui*: *Kembalikan* tampil, *Verifikasi* tidak | `PASS` (tiruan) | Spec skenario 2 |
| **`UAT-18`** E2E | (1) Ringkasan: hanya `variant=fitness-summary` diminta; tampil *Unfit sementara* + rekomendasi kerja; **tanpa** *HBsAg* dan No. RM walau tiruan menyisipkannya. (2) Hasil lengkap: *Reaktif \**, laporan Rad, No. RM; *Cetak* menyalin lembar ke iframe cetak. (3) Penyerahan ke Siti via WhatsApp: tujuan kosong ditolak layar → `{1,1,2,"0812 3456 7890",null,null,5}`, lalu *Diserahkan*. (4) Ringkasan ke PT Contoh via email: format salah ditolak → `{2,2,3,"hrd@contoh.co.id",null,null,6}`. Tabel berisi dua baris | `PASS` (tiruan) | Spec skenario 3 |
| **`UAT-19`** E2E | (1) Lengkap ke PT Contoh → pesan `VAL-MCU-31` apa adanya. (2) *Ganti ke Ringkasan Kelayakan* → isi berganti, pesan hilang, kirim `contentType: 2`. (3) *Catat Persetujuan* kertas tanpa nomor → *"Isi nomor formulir persetujuan."*; dengan nomor → `{1,"FRM-MCU-0045",null,6}`, *Berlaku sejak*. (4) Cabut: alasan kosong ditolak → `{ reason, version: 7 }` → *Belum ada* + jejak pencabutan | `PASS` (tiruan) | Spec skenario 4 |
| Pemberitahuan koreksi + keadaan E2E | (1) Order *Menunggu Resume*: keterangan; *Cetak Hasil Lengkap* nonaktif; tanpa *Catat Penyerahan*. (2) Baris v1 *⚠ Perlu* → *Catat* → `PATCH correction-notice` `{ notifiedAt: null, version: 5 }` → *Dicatat*. (3) Tanpa `McuDelivery : Read` → keterangan, nol `GET deliveries` | `PASS` (tiruan) | Spec skenario 5 |
| Backend lokal + devYoga (tanpa tulis) | Tidak ada order *Disetujui Dokter*/*Terverifikasi*/*Diserahkan* (0/0/0). Pada `MCU-RSMMC-000004` (*Hadir*, perorangan): (1) `GET` riwayat dan persetujuan `200`; `result-document` kedua varian `409` *"Resume belum disetujui dokter."* (2) `POST deliveries` tanpa saluran `400`; WA tanpa tujuan `400` `VAL-MCU-33`; **body layar lengkap** `409` `VAL-MCU-32` *"Hasil belum diverifikasi."* — bentuk payload diterima. (3) Persetujuan: bukti kosong `400`, kertas tanpa nomor `400`. (4) `verify` `409`; `return` alasan kosong `400` `VAL-MCU-40`. (5) Layar tab Penyerahan: keterangan, cetak nonaktif, tanpa *Catat Penyerahan*; tab Resume tanpa *Verifikasi*/*Kembalikan*. (6) Sesudahnya order versi 1, penyerahan 0, persetujuan 0; layar nol non-GET | `PASS` | Keluaran `uat-013.mjs`, `uat-013-penyerahan.png` (scratchpad sesi `cbe51345`) |
| Temuan UAT → diperbaiki | Server menerima `hrd@contoh` (`MailAddress` .NET tidak menuntut titik pada domain), sedangkan layar semula menolaknya. Aturan email layar disamakan dengan server, uji unit diberi kasus itu, spec memakai contoh yang memang tidak sah (`hrd.contoh.co.id`). Build ulang → E2E penyerahan + resume 11/11 | `PASS` | Keluaran perintah |
| Seluruh spec E2E MCU (9 berkas, 64 skenario) | 63 lulus, 1 gagal: `mcu-check-in` *409 basi* — tombol *Check-in* di dasar halaman tertutup footer melayang (tangkapan layar). Ulang `mcu-check-in` sendirian: dua kali gagal pada skenario yang mengklik tombol di dasar halaman (1 lalu 2 skenario). Rantai impor halaman check-in tidak memuat satu pun berkas yang diubah task ini; kegagalan yang sama tercatat di `FE-MCU-011` | `FLAKY EXISTING` | Keluaran Playwright + `grep` impor |
| `npx eslint` (berkas layar pasien MCU + uji) | Exit 0 | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 3.930 uji: 3.888 lulus, 42 gagal; daftar nama **identik** baseline, semuanya di luar MCU | `EXISTING ISSUE` | Keluaran + `diff` |
| `npm run build` (dua kali) | Exit 0 | `PASS` | Keluaran build |

`MANUAL TEST`: kontrol interaktif diuji dengan Playwright pada build standalone. Cakupannya:

- Pilihan penerima/isi/saluran, isian tujuan yang berganti per saluran, dan tombol beralih ke ringkasan.
- Pilihan bentuk bukti dan nomor formulir wajib, serta waktu opsional.
- Modal cabut dan pemberitahuan koreksi.
- Pratinjau cetak beserta salinan ke iframe cetak.
- Modal verifikasi dan kembalikan.

Layar sungguhan terhadap backend lokal dibuka untuk tab Penyerahan dan Resume pada order *Hadir*.

`UAT-15`, `18`, `19` dengan data sungguhan **tidak dijalankan**. Alasannya: devYoga belum punya order yang resumenya
disetujui. Untuk mencapainya dibutuhkan pembayaran, Lab/Rad selesai, skrining, dan resume disetujui dr. Bima. Order
perusahaan untuk `UAT-19` juga dibutuhkan. Sesi ini tidak menulis data uji.

`AUTOMATED TEST: npm run test:unit — FAIL (42 kegagalan existing di luar MCU, identik baseline; uji MCU PASS)`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `UAT-15` | Terpenuhi di E2E; payload dan penolakan server terbukti terhadap backend lokal | §6 |
| `UAT-18` | Terpenuhi di E2E (dua catatan penyerahan) | §6 |
| `UAT-19` | Terpenuhi di E2E (ditolak, diarahkan ke ringkasan) | §6 |
| Cetak ringkasan tidak memuat nilai maupun diagnosis | Terpenuhi: hanya `variant=fitness-summary` + daftar putih 12 ruas, dibuktikan unit (kontrol negatif) dan E2E | §6 |
| DoD: kriteria terbukti | Terpenuhi dengan batas verifikasi di §6 | §6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Delta brief | Brief 5.8 menulis modal pemberitahuan koreksi berisi *waktu + catatan*, sedangkan `RecordCorrectionNoticeRequest` hanya memuat `notifiedAt` dan `version`. Modal mengikuti kontrak (waktu saja); catatan butuh perubahan backend dan kontrak bila diinginkan |
| Keputusan pengembang | (1) `isApprovingDoctor` = `isAssignedDoctor`. Approve hanya dapat dilakukan dokter yang ditetapkan, dan respons resume tidak memuat id pemberi approve. Bila dokter diganti **sesudah** approve, dokter lama dapat melihat *Verifikasi* lalu ditolak server `403`. (2) Kolom *Waktu penyerahan* tidak ditampilkan (brief tidak memintanya); server mengisi *sekarang*. (3) *Catat Penyerahan* dari header membuka tab Penyerahan sekaligus modalnya |
| Batas verifikasi | `UAT-15`/`18`/`19` dengan data sungguhan. Untuk menutupnya dibutuhkan: order MCU yang resumenya disetujui di devYoga (alur penuh sampai approve dr. Bima), order perusahaan untuk `UAT-19`, izin tulis devYoga, dan akun petugas MCU ber-`McuResume : Verify` + `McuDelivery` |
| Risiko | Bentuk bukti persetujuan final menunggu `MCU-OQ-003` dan menahan go-live penyerahan ke perusahaan (sama dengan roadmap) |
| Flaky existing | Dua skenario `mcu-check-in-screen.spec.mjs` yang mengklik *Check-in* di dasar halaman makin sering gagal karena footer melayang menutupi tombol. Perlu perbaikan spec, atau tata letak footer, di task terpisah |
| Dependency backend | Tidak ada |
| Perubahan sampingan | `NONE`. Output Playwright ke scratchpad; `test-results/` ter-track tidak tersentuh. Server BE 7184 dan FE 3710 dimatikan sebelum build, port bebas |
| Status Git (frontend) | `??` berkas baru di §3.1 (di dalam folder `mcu-patients/` yang belum ter-track) serta berkas uji baru |
| Langkah berikutnya | Seluruh task layar MCU `MVP-3` selesai. Tersisa `FE-MCU-015` (label *Dicakup paket MCU* di Lab/Rad; `BE-MCU-020` ✅). Lalu verifikasi kesiapan modul dan penutupan batas verifikasi data sungguhan |
