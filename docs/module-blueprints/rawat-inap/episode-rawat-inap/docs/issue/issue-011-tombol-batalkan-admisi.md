# ISSUE-011 — Ketiadaan Tombol Batalkan Admisi pada Alur Langkah Admisi Rawat Inap

```yaml
issue_id: ISSUE-EPS-011
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Admisi Rawat Inap — Seluruh Tahapan Langkah Admisi (FE-INP-03 s/d FE-INP-10, FE-INP-17)"
sumber_laporan: "Laporan dan arahan pemilik sistem 13-10-2026: ketiadaan tombol batalkan admisi, penentuan rentang langkah, pembersihan formulir kembali ke awal, dan retensi pencatatan pada riwayat episode"
tanggal_issue: "2026-10-13"
status: TERBUKA
keparahan_tertinggi: High
source_sha_backend: "8fce8c96 (MHamzah)"
source_sha_frontend: "1b0ad5dd4 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-011-tombol-batalkan-admisi.md
```

## 1. Ringkasan

Pada antarmuka Admisi Rawat Inap (`/health-services/inpatient-management/admissions`), tidak ditemukan tombol **Batalkan Admisi** di sepanjang tahapan formulir pendaftaran. Petugas yang menghadapi situasi pasien/keluarga batal dirawat tidak memiliki jalan pintas untuk membatalkan proses secara langsung dari layar yang sedang dibuka. Saat ini, layar hanya menyediakan tombol *"Tinggalkan Admisi"* pada Langkah Konfirmasi yang sebatas mengarahkan petugas keluar ke Daftar Kerja Episode tanpa membatalkan transaksi; akibatnya, episode berstatus `Draft` terbengkalai dan tempat tidur yang telah dipesan tetap terkunci (*Reserved*) selama 60 menit sehingga tidak dapat digunakan oleh pasien lain. Temuan ini diklasifikasikan sebagai **High** karena menahan kapasitas tempat tidur rumah sakit dan memaksa petugas melakukan prosedur pembatalan manual yang berbelit dari layar lain. Berdasarkan analisis siklus data, tombol pembatalan harus dibedakan menjadi dua perilaku: tombol **"Batal Pendaftaran"** pada tahap pengisian data lokal (Langkah 1 s/d Langkah Pilih Bed sebelum lanjut) yang mereset formulir tanpa memanggil API, serta tombol resmi **"Batalkan Admisi"** pada tahap pasca-pembuatan episode (Langkah Booking Bed s/d Konfirmasi, dan seluruh langkah pelanjutan admisi) yang memanggil endpoint `PATCH /episodes/{id}/cancel`. Pembatalan resmi tersebut melepaskan reservasi tempat tidur, mereset formulir kembali bersih ke awal, dan **tetap terbaca secara permanen pada Riwayat/Daftar Kerja Episode** dengan status `Batal (Cancelled)`.

---

## 2. Laporan asli

| No. laporan | Keluhan pelapor | Lampiran |
| ---: | --- | --- |
| 1 | "di admisi rawat inap tidak ada button di batalkan. coba anda analisis tombol batalkan admisi itu harus berada di step berapa sampai dengan step berapa lalu ketika proses batalkan admisi dia clear kembali ke awal semesti nya, tapi di riwayat episode tetap terbaca tolong anda analisis terlebih dahulu" | Screenshot layar admisi: "LANGKAH 2 DARI 10 - Jenis Kunjungan & Kategori", tombol di kanan bawah hanya ada `[Kembali ke Pencarian]` dan `[Lanjut ke Pembayaran]` |

---

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | ---: | --- | --- | --- | --- | --- | --- |
| `ISS-EPS-011-01` | 1 | Ketiadaan tombol batalkan admisi pada antarmuka tahapan admisi rawat inap | `DESIGN_CHANGE` | Frontend | High | SUDAH-VERIFIKASI di source `inpatient-admission-view.jsx:963-1004` dan capture pelapor | `FIX-EPS-011-01` |
| `ISS-EPS-011-02` | 1 | Tombol keluar yang ada (*Tinggalkan Admisi*) membiarkan episode `Draft` dan reservasi bed menggantung tanpa dibatalkan | `BUG`, `RULE_VIOLATION` | Frontend | High | SUDAH-VERIFIKASI di `use-inpatient-admission-exit-guard.jsx:82-97` | `FIX-EPS-011-02` |
| `ISS-EPS-011-03` | 1 | Ketiadaan mekanisme reset/clear formulir terpadu saat petugas membatalkan admisi di tengah jalan | `BUG` | Frontend | Medium | SUDAH-VERIFIKASI di `inpatient-admission-view.jsx` | `FIX-EPS-011-03` |
| `ISS-EPS-011-T1` | — | Aksi pembatalan admisi `FE-INP-17` (`PATCH /episodes/{id}/cancel`) hanya terpasang di Worklist dan Detail Episode, belum diekspos ke wizard admisi | `DESIGN_CHANGE` | Frontend | Medium | SUDAH-VERIFIKASI di `05-skema-tampilan.md:1549` dan `FE-RWI-031.md:62` | `FIX-EPS-011-01`, `FIX-EPS-011-02` |

---

## 4. Rincian per temuan

### ISS-EPS-011-01 — Ketiadaan tombol batalkan admisi pada antarmuka tahapan admisi rawat inap

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source; DARI-CAPTURE pelapor |
| **Perbaikan** | `FIX-EPS-011-01` |

**Apa yang terjadi.** Petugas admisi rawat inap yang sedang memproses pendaftaran pasien tidak menemukan tombol untuk membatalkan proses pendaftaran di bilah navigasi langkah bawah. Bilah aksi hanya menyediakan tombol navigasi langkah seperti "Kembali" dan "Lanjut". Bila pasien tiba-tiba menolak dirawat inap (misalnya karena masalah biaya atau ingin pindah rumah sakit), petugas kebingungan bagaimana cara membatalkan pendaftaran tersebut secara langsung dari layar admisi.

**Kenapa terjadi.** Pada perancangan awal (`05-skema-tampilan.md` bagian 21), dialog pembatalan admisi `FE-INP-17` hanya dirancang sebagai aksi baris dari Daftar Kerja Episode (`FE-INP-16`) dan aksi tombol pada Detail Episode (`FE-INP-04`), dengan catatan: *"Lokasi: dialog aksi dari FE-INP-16 dan FE-INP-04; bukan menu tersendiri."* Akibatnya, pada wizard alur pendaftaran (`inpatient-admission-view.jsx`), tombol pembatalan resmi tidak pernah disematkan pada bilah aksi langkah.

```javascript
// c:\Users\Admin\Documents\Quilvian\Source Code\QuilvianFinal\QuilvianSystemFrontendDev\src\components\view\health-services\inpatient-management\inpatient-admission-view.jsx:955-988
<InpatientAdmissionStepper
  currentStep={flow.currentStep?.value}
  steps={flow.steps}
  ariaLabel="Tahapan admisi rawat inap"
/>
<section className={admissionStyles.flowPanel}>
  <AdmissionStep flow={flow} patient={patient} ... />
</section>
// Hanya merender AdmissionStep tanpa bilah pembatalan terpadu
```

**Apakah ini menyimpang dari desain?** Ini adalah `DESIGN_CHANGE`. Desain awal memang hanya menempatkan aksi pembatalan di worklist dan detail. Namun, kebutuhan operasional nyata di loket admisi menuntut petugas dapat langsung membatalkan admisi di tengah alur tanpa harus keluar dan mencari pasien di daftar kerja.

**Dampak nyata.** Petugas loket terpaksa menutup tab peramban atau keluar sembarangan, meninggalkan admisi dalam status tidak jelas, atau harus membuka tab baru ke menu Daftar Kerja untuk mencari baris pasien lalu mengklik "Batalkan Admisi".

**Rekomendasi.** Menyediakan tombol pembatalan pada bilah aksi tahapan admisi dengan dua tingkatan:
1. Tombol **"Batal Pendaftaran"** pada Langkah 1 s/d Langkah Pilih Bed (sebelum simpan titik tulis): mereset form lokal dan kembali ke menu tipe pendaftaran.
2. Tombol **"Batalkan Admisi"** pada Langkah Booking Bed s/d Konfirmasi (dan seluruh langkah alur pelanjutan): memanggil modal `FE-INP-17` beralasan dan mengeksekusi `PATCH /episodes/{id}/cancel`.

---

### ISS-EPS-011-02 — Tombol keluar yang ada (*Tinggalkan Admisi*) membiarkan episode `Draft` dan reservasi bed menggantung tanpa dibatalkan

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG`, `RULE_VIOLATION` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di source |
| **Perbaikan** | `FIX-EPS-011-02` |

**Apa yang terjadi.** Pada Langkah Konfirmasi, terdapat tombol sekunder berlabel *"Tinggalkan Admisi"*. Ketika diklik, dialog konfirmasi muncul memberitahukan bahwa episode berstatus `Draft` tersimpan di daftar kerja dan bed masih dipesan. Namun, saat ditekan "Tinggalkan", sistem hanya mengarahkan peramban ke halaman Daftar Kerja (`/health-services/inpatient-management/episodes`). Tempat tidur yang sudah dipesan tetap terkunci (*Reserved*) selama 60 menit dan episode tetap menggantung, padahal niat petugas adalah membatalkan pendaftaran.

**Kenapa terjadi.** Implementasi `useInpatientAdmissionExitGuard` (`use-inpatient-admission-exit-guard.jsx:82-97`) dirancang hanya sebagai penjaga navigasi (*exit guard*), bukan pengendali pembatalan (*cancellation handler*). Fungsi `confirmExit` hanya memanggil `router.push(INPATIENT_EPISODE_ROUTE)` tanpa pernah memanggil endpoint pembatalan.

```javascript
// c:\Users\Admin\Documents\Quilvian\Source Code\QuilvianFinal\QuilvianSystemFrontendDev\src\lib\hooks\health-services\inpatient-management\use-inpatient-admission-exit-guard.jsx:94-97
const confirmExit = useCallback(() => {
  setOpen(false);
  router.push(INPATIENT_EPISODE_ROUTE);
}, [router]);
```

**Apakah ini menyimpang dari desain?** `RULE_VIOLATION` terhadap efisiensi manajemen bed rawat inap (`RWI-RULE-004`). Menahan tempat tidur pasien yang batal dirawat merugikan pasien lain yang membutuhkan kamar.

**Dampak nyata.** Ruangan rawat inap tampak penuh (*Reserved*) di papan tempat tidur (*bed board*), padahal pasiennya sudah pulang atau menolak dirawat. Petugas ruangan tidak dapat menempatkan pasien IGD darurat ke tempat tidur tersebut sampai batas kedaluwarsa 60 menit terlampaui.

**Rekomendasi.** Tombol *"Tinggalkan Admisi"* pada Langkah Konfirmasi dan Booking Bed digantikan atau didampingi oleh tombol tegas **"Batalkan Admisi"** yang mengeksekusi pembatalan resmi via `PATCH /episodes/{id}/cancel`, melepaskan reservasi tempat tidur ke status `Available` secara seketika.

---

### ISS-EPS-011-03 — Ketiadaan mekanisme reset/clear formulir terpadu saat petugas membatalkan admisi di tengah jalan

| | |
| --- | --- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source |
| **Perbaikan** | `FIX-EPS-011-03` |

**Apa yang terjadi.** Ketika petugas membatalkan proses pendaftaran (baik di tahap awal form maupun sesudah episode dibatalkan), tidak ada prosedur terpadu yang membersihkan data penyimpanan sesi (*sessionStorage* penjamin, nominal deposit lokal, memo DPJP) dan mengembalikan tampilan secara bersih ke layar pemilihan tipe pendaftaran.

**Kenapa terjadi.** Penyimpanan sementara terdistribusi di beberapa berkas utility terpisah (`inpatient-admission-payment-storage.jsx`, `inpatient-admission-deposit-storage.jsx`, `inpatient-admission-transfer-utils.js`). Saat ini hanya ada fungsi `resetEntryMode` pada `useInpatientAdmissionFlow`, namun fungsi ini tidak membersihkan penyimpanan deposit dan penjamin sehingga bila pendaftaran baru dimulai lagi, sisa input pasien sebelumnya masih dapat terbawa.

**Apakah ini menyimpang dari desain?** `BUG`. Seluruh data pasien yang dibatalkan wajib dibersihkan agar tidak terjadi kontaminasi data (*state leak*) ke pasien berikutnya yang akan didaftarkan.

**Dampak nyata.** Petugas yang mendaftarkan pasien berikutnya berisiko melihat cara bayar atau nominal deposit dari pasien sebelumnya yang sudah dibatalkan.

**Rekomendasi.** Membuat fungsi utilitas pembersihan menyeluruh `clearInpatientAdmissionSession()` yang membersihkan `sessionStorage`, mereset seluruh state hook di `inpatient-admission-view.jsx`, dan mengarahkan rute kembali ke `/health-services/inpatient-management/admissions`.

---

### ISS-EPS-011-T1 — Aksi pembatalan admisi `FE-INP-17` (`PATCH /episodes/{id}/cancel`) hanya terpasang di Worklist dan Detail Episode, belum diekspos ke wizard admisi

| | |
| --- | --- |
| **No. laporan** | — |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source dan dokumen blueprint |
| **Perbaikan** | `FIX-EPS-011-01`, `FIX-EPS-011-02` |
| **Kenapa dicantumkan** | Menjelaskan mengapa backend sudah siap menerima `PATCH /episodes/{id}/cancel` namun antarmuka admisi belum memanfaatkannya |

**Apa yang terjadi.** Endpoint `PATCH /api/v1/health-services/inpatient-management/episodes/{id}/cancel` dan DTO `CancelAdmissionRequest` sudah terbukti berjalan stabil sejak implementasi task backend `BE-RWI-008` dan task frontend `FE-RWI-031`. Namun pemanggilannya hanya dipasang pada `use-inpatient-episode-worklist.jsx` dan `use-inpatient-episode-detail.jsx`.

**Kenapa terjadi.** Pemisahan peran layar: admisi diposisikan sebagai pembuat data (*producer*), sedangkan worklist diposisikan sebagai pengelola data (*manager*). Padahal pada alur kerja nyata, pembuat data juga membutuhkan wewenang pembatalan darurat saat pengisian form berlangsung.

**Dampak nyata.** Terjadi inkonsistensi pengalaman pengguna (UX) di mana petugas harus berpindah modul/layar hanya untuk membatalkan transaksi yang baru saja dibuatnya.

**Rekomendasi.** Menggunakan ulang (*reuse*) utilitas `buildAdmissionCancellationPayload`, `validateAdmissionCancellationReason`, dan layanan `inpatientEpisodeService.patch(`${episodeId}/cancel`, ...)` langsung di dalam alur wizard admisi.

---

## 5. Tanya-jawab pelapor

> **T:** Tombol batalkan admisi itu harus berada di step berapa sampai dengan step berapa?
>
> **J:** Berdasarkan siklus hidup data di database:
> 1. **Langkah 1 sampai Langkah 6 (Pilih Bed sebelum klik lanjut):** Belum ada episode yang tersimpan di server. Tombol yang disediakan berlabel **"Batal Pendaftaran"** (varian sekunder/ghost), yang fungsinya membersihkan formulir dan kembali ke menu awal tanpa memanggil API server.
> 2. **Langkah 7 (Booking Bed) dan Langkah 8 (Konfirmasi):** Episode sudah resmi tersimpan di server dengan status `Draft` dan kamar terkunci `Reserved`. Pada kedua langkah ini, tombol resmi **"Batalkan Admisi"** (varian merah/danger) **wajib ada**. Ketika diklik, petugas wajib mengisi alasan, memanggil API pembatalan, melepas kamar, mereset layar, dan data tercatat di riwayat.
> 3. **Langkah 9 & 10 (Form & Cetak Persetujuan):** Pasien sudah sah dirawat (*Admitted*). Sesuai aturan rumah sakit, tombol pembatalan hanya boleh muncul dan ditekan jika pengguna memiliki hak akses **Supervisor atau Kepala Ruangan**.
> 4. **Alur Pelanjutan Admisi (*Resume Draft*):** Karena episode `Draft` sudah ada di database sejak awal, tombol **"Batalkan Admisi"** aktif di **seluruh langkah** (Langkah 1 sampai akhir).

---

> **T:** Ketika proses batalkan admisi dia clear kembali ke awal semestinya, tapi di riwayat episode tetap terbaca, apakah bisa?
>
> **J:** **Ya, tentu saja dan memang demikian rancangan sistemnya.** Backend Quilvian (`InpEpisodeService.CancelAdmissionAsync`) tidak pernah menghapus (*hard delete*) data dari database. Saat admisi dibatalkan:
> 1. Status episode diubah menjadi `Cancelled` (`IsCancel = true`) lengkap dengan alasan, waktu, dan ID petugas yang membatalkan.
> 2. Dicatat riwayat status di tabel `InpStatusHistory` (`Draft -> Cancelled`).
> 3. Reservasi tempat tidur dilepaskan kembali menjadi `Available`.
> 4. Layar admisi dibersihkan (*clear*) total kembali ke tampilan awal (Pilih Tipe Pendaftaran).
> 5. Ketika petugas membuka menu **Daftar Kerja / Riwayat Episode**, transaksi tersebut **tetap ada dan terbaca** dengan status **`Batal`**. Petugas dan manajemen rumah sakit tetap memiliki bukti audit lengkap bahwa admisi pernah dibuat lalu dibatalkan.

---

## 6. Temuan tambahan

Tidak ada temuan tambahan di luar temuan terverifikasi pada tabel ringkasan bagian 3.

---

## 7. Pertanyaan dan keputusan yang dibutuhkan

### Untuk pelapor:

Tidak ada pertanyaan lanjutan. Seluruh arahan pelapor pada pesan terdahulu sudah jelas dan dapat dipetakan langsung ke arsitektur sistem.

### Untuk pemilik:

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Peletakan tombol pembatalan pada Langkah 1 s/d 6 (tahap form lokal) | A. Tombol "Batal Pendaftaran" (reset form langsung dengan konfirmasi sederhana)<br>B. Tanpa tombol batal, petugas cukup menekan tombol "Kembali" | **Opsi A**: Memudahkan petugas mereset form bila pasien batal di awal tanpa meninggalkan sampah *sessionStorage* | `FIX-EPS-011-01` |
| K-02 | Pembatalan pada Langkah 9 & 10 (Pasien sudah *Admitted*) | A. Sembunyikan tombol batal untuk petugas loket biasa; hanya tampilkan jika user adalah Supervisor/Karu<br>B. Jangan sediakan tombol batal sama sekali di Langkah 9-10 wizard; pembatalan pasien *Admitted* wajib lewat Detail Episode | **Opsi A**: Konsisten dengan matriks kewenangan `RWI-RULE-004` dan memberi jalan keluar jika ada supervisor yang mendampingi di meja admisi | `FIX-EPS-011-01` |

---

## 8. Catatan pola

Pola ketiadaan tombol batal ini serupa dengan temuan terdahulu pada `FE-RWI-031` di mana aksi pembatalan hanya dipikirkan sebagai aksi tabel (*table row action*), bukan aksi formulir (*wizard process action*). Prinsip perbaikan: **Setiap formulir multi-langkah yang mengunci sumber daya fisik rumah sakit (tempat tidur) wajib menyediakan pintu pembatalan mandiri di dalam alur itu sendiri.**

---

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-13 | Dokumen issue dibuat berdasarkan laporan ketiadaan tombol batalkan admisi dan analisis penempatan langkah | `diagnose-module-issue` |
