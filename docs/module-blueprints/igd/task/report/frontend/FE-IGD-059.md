# Laporan Perubahan Frontend — `FE-IGD-059`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-059` |
| Judul | Form Permintaan Rawat Inap di layar perawat: draf, minta TTD, batal sebelum kirim |
| Slice | `EPIC IGD-15` · `MVP-11` · `SCR-IGD-P01` tab *Tindak Lanjut* (panel) + `SCR-IGD-P02` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian R3.16 |
| Trace | `FR-IGD-109`, `110`, `111`, `112` (sisi perawat), `115` (sebelum kirim); `AT-IGD-221`…`224`, `227`; `IGD-DEC-268`, `271`, `273`, `278`, `282`…`290`, `294` (U1), `300` |
| Contract version | API **`0.17.1`** §12.2–12.3 (`approved` `IGD-DEC-294`, metode aksi `POST` `IGD-DEC-300`); validation `0.16.0` §14; state `0.11.0` §11.1–11.2; `03-frontend-architecture.md` §17 |
| Wewenang UI | U1 disetujui: panel penuh di dalam tab *Tindak Lanjut*. Rupa tampilan `DEV_DISCRETION` dalam konvensi proyek |
| Dependency | `BE-IGD-072` 🟡 — source, migration, dan build ada; tanda `RANAP` belum terisi (lihat 8) |
| Klasifikasi | `MEDIUM` — 6 berkas baru, 1 berkas disentuh, 7 endpoint dikonsumsi, nol route, nol menu, nol CSS baru |
| Task mode | `FRONTEND` — izin pemilik *"lanjut ke langkah selanjutnya"* (12 Oktober 2026) sesudah migration dan build `BE-IGD-072` |
| Target tulis | `QuilvianSystemFrontendDev` (branch `RizkiV2`): berkas pada 3.2; laporan ini, panduan uji, roadmap, dan traceability di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `701ddfb5d` (`RizkiV2`) + working tree |
| Commit backend yang dijadikan rujukan | `6a1c6cf9` (`rizkiG`) + working tree `BE-IGD-072` |
| Tanggal | 12 Oktober 2026 |
| Status | 🟡 **SEBAGIAN — kode lengkap, ESLint 0 error 0 warning; putaran A dinilai dari bukti mentah: 12 dari 14 terbukti, A-01 sebagian, A-11 tidak terbukti ([penilaian putaran A](../../../testing/2026-10-12-laporan-uji-form-permintaan-rawat-inap-putaran-a.md#7-penilaian-ulang-dari-bukti-mentah--agent-pengembang-12-oktober-2026)); `npm run build` belum (server dev pemilik aktif)** |

---

## 1. Keadaan yang ditemukan di awal

- Tab *Tindak Lanjut* layar perawat (`emergency-assessment-disposition-tab.jsx`) hanya menampilkan riwayat tindak lanjut dan
  tombol *Jalankan*/*Batalkan*. Tidak ada tempat untuk Form Permintaan Rawat Inap.
- Backend `BE-IGD-072` sudah menyediakan sembilan endpoint `emergency-admission-requests` dan ruas
  `requiresAdmissionRequest` pada respons tindak lanjut (laporan `BE-IGD-072` bagian 3.4).
- Pilihan unit rawat inap dan kelas perawatan sudah punya pemanggil di layar admisi Rawat Inap:
  `fetchInpatientServiceUnitOptions` dan `fetchInpatientPatientClassOptions`.
- Working tree frontend memuat pekerjaan sesi lain yang belum di-commit di layar perawat yang sama (antara lain
  `emergency-assessment-detail-view.jsx`, `emergency-assessment-form-card.jsx`, slice, dan CSS module). Berkas-berkas itu
  **tidak** disentuh; komponen yang dipakai hanya bagian yang sudah ada di `HEAD`.

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: perawat IGD. Layar: Pengkajian Pasien IGD → tab *Tindak Lanjut*.

1. Dokter mengonfirmasi tindak lanjut rawat inap. Pada baris itu muncul kotak *Form Permintaan Rawat Inap* bertuliskan
   *"Belum ada Form Permintaan Rawat Inap."* dan tombol *Buat Form*.
2. Perawat menekan *Buat Form*. Form terbuka sebagai panel penuh di bawah daftar tindak lanjut, berlencana *Draf*. Unit
   layanan, indikasi klinis, dan rencana perawatan sudah terisi dari tindak lanjut; lama rawat 3 hari.
3. Bagian *Identitas* dan *Diagnosis terakhir* hanya dibaca: nama, No. RM, jenis kelamin, umur, asal IGD, alergi
   (*"Belum ada data alergi"* bila kosong), penjamin beserta hak kelas, diagnosis ICD-10 dengan diagnosis utama paling atas.
4. Perawat memilih kelas perawatan dan DPJP rawat inap usulan. Daftar DPJP datang dari server: bila unit punya jadwal
   dokter, hanya dokter berjadwal; bila tidak, semua dokter aktif dengan keterangan *"Unit ini belum punya jadwal dokter;
   menampilkan semua dokter aktif."* Mengganti unit mengosongkan DPJP usulan.
5. Perawat menandai CITO dan kebutuhan khusus bila perlu, lalu *Simpan Draf* (boleh berkali-kali).
6. Perawat menekan *Minta TTD dr. …*. Dialog konfirmasi menyebut penanda tangan; bila ada isian yang belum disimpan,
   isian disimpan lebih dulu. Sesudah berhasil: lencana *Menunggu TTD*, isian menjadi baca-saja, dan kotak di baris tindak
   lanjut menulis *"Tanda tangan diminta pukul …"*.
7. Bila dokter mengembalikan form, form kembali *Draf*; catatan dokter tampil sebagai kotak kuning di atas isian dan di
   *Riwayat*.
8. Selama form belum ditandatangani perawat dapat *Batalkan Form*; alasan wajib. Sesudah dibatalkan kotak menampilkan
   alasan, *Lihat Form*, dan *Buat Form* lagi.

**Jalur tidak normal.**

| Keadaan | Yang dilihat perawat |
| --- | --- |
| Tindak lanjut rawat inap belum dikonfirmasi | Kotak tampil tanpa *Buat Form*: *"Form dapat dibuat sesudah tindak lanjut dikonfirmasi dokter."* |
| Pasien belum punya DPJP IGD, atau DPJP tanpa akun | Kotak peringatan dengan pesan aturan 12; tombol *Minta TTD* nonaktif |
| Penolakan `400`/`409` dari server | Pesan server apa adanya, di tempat aksinya (bawah form atau di dalam dialog); isian tidak dikosongkan |
| Form diubah orang lain | *"Form sudah diubah orang lain. Muat ulang lalu coba lagi."* dan tombol *Muat ulang form* |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`, `CLAUDE.md` frontend; `rules/frontend/` (arsitektur, katalog, gerbang, token, checklist, template); kartu
`FE-IGD-059`; `03-frontend-architecture.md` §17; API §12; validation §14; `emergency-assessment-disposition-tab.jsx`,
`emergency-assessment-transfer-tab.jsx` (rujukan visual), `emergency-assessment-section.jsx`,
`emergency-assessment-form-card.jsx`, `emergency-assessment-detail-view.jsx`, `use-emergency-assessment-detail.jsx`,
`emergency-physician.service.js`, `use-emergency-progress-note.jsx` (pola hook non-Redux), `base-form-control.jsx`,
`filter-select.jsx`, `status-badge.jsx`, `inpatient-admission-encounter.service.js`, `inpatient-admission-payment.service.js`,
`inpatient-physician-workspace-utils.jsx`; backend `EmergencyAdmissionRequestController.cs`, DTO, dan service form.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/emergency-installation-management/emergency-admission-request-constant.jsx` | **Baru** — URL, nilai enum (angka, sesuai respons backend), label dan nada lencana, konfigurasi isian, teks baku §17.4 |
| `src/lib/services/health-services/emergency-management/emergency-admission-request.service.js` | **Baru** — tujuh fungsi `InstanceAxios`: daftar per kunjungan, rincian, buat, simpan draf, minta TTD (`POST`), batal (`POST`), pilihan DPJP |
| `src/utils/health-services/emergency-installation-management/emergency-admission-request.utils.js` | **Baru** — normalisasi respons, bentuk form dan payload `PUT`, pengelompokan per tindak lanjut, teks tampilan. *Perbaikan sesudah putaran A:* penjamin tidak lagi tampil ganda (*"Tunai — Tunai"*) bila jenis bayar sama dengan nama penjamin |
| `src/lib/hooks/health-services/emergency-installation-management/emergency-assessment/use-emergency-admission-request.jsx` | **Baru** — hook non-Redux: muat daftar dan rincian (dengan `AbortController`), pilihan unit/kelas/DPJP, aksi buat, simpan, minta TTD, batal; status sibuk dan galat per tempat aksi |
| `src/components/view/…/emergency-assessment-view/components/emergency-assessment-admission-request-panel.jsx` | **Baru** — kotak keadaan form pada baris tindak lanjut |
| `src/components/view/…/emergency-assessment-view/components/emergency-assessment-admission-request-form.jsx` | **Baru** — layar `SCR-IGD-P02` sebagai panel penuh di tab |
| `src/components/view/…/emergency-assessment-view/components/emergency-assessment-disposition-tab.jsx` | Memanggil hook; kotak hanya pada baris `requiresAdmissionRequest`; panel form di bawah daftar; daftar form ikut dimuat ulang sesudah aksi tindak lanjut |

### 3.3 Kepatuhan arsitektur frontend

- Alur: view → hook → service → `InstanceAxios`; view tidak memanggil Axios; normalisasi di `utils`; endpoint di constants.
- Hook non-Redux mengikuti pola Ruang Kerja Dokter IGD (`use-emergency-progress-note.jsx`), sesuai kartu. Tidak ada slice
  baru; slice `emergency-assessment-slice.jsx` (sedang diubah sesi lain) tidak disentuh.
- Pakai ulang lintas modul tanpa mengubahnya: `fetchInpatientServiceUnitOptions`, `fetchInpatientPatientClassOptions`
  (layar admisi Rawat Inap), `ALLERGY_SEVERITY_LABEL`, `ALLERGY_CATEGORY_LABEL`, `readEmergencyPhysicianErrorMessage`,
  `isAccessDeniedError`, `isUuidValue`.
- Nol CSS baru: kelas `recordItem`, `recordGrid`, `formSection`, `formFieldFull`, `recordActions`, `formHint`, `mutedText`
  dari `emergency-assessment.module.css` yang sudah ada di `HEAD`.

#### Gerbang keputusan base component

`UI GATE: 11 elemen — REUSE 9, EXTEND 0, COMPOSE 2, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| Kotak keadaan form pada baris tindak lanjut | `StatusBadge`, `BaseButton`, `InformationAlert`, kelas `recordItem` | dipakai `emergency-assessment-transfer-tab.jsx` | COMPOSE | Dirangkai di view |
| Wadah form beserta muat/gagal/tanpa akses | `EmergencyAssessmentSection` (slot `actions`), `EmergencyAssessmentFormSection` | komponen tab perawat yang ada | COMPOSE | Dirangkai, tanpa komponen baru |
| Identitas, alergi, penjamin, diagnosis baca-saja | kelas `recordGrid` | sama dengan baris tindak lanjut | REUSE | `dl` berkelas `recordGrid` |
| Unit, kelas, DPJP usulan | `BaseSelectField` (`serverSide`, `selectedOption`) | `base-form-control.jsx:593` | REUSE | DPJP dicari di server |
| Lama rawat | `BaseTextField` `type="number"` | `base-form-control.jsx:233` | REUSE | `min` 1, `max` 90 |
| CITO dan empat kebutuhan khusus | `BaseCheckboxField` | `base-form-control.jsx:771` | REUSE | — |
| Indikasi klinis, rencana perawatan | `BaseTextAreaField` + `formFieldFull` | dipakai tab dokter tindak lanjut | REUSE | `maxLength` 2000 |
| Tombol aksi | `BaseButton` | katalog | REUSE | `loading` pada aksi ber-API |
| Konfirmasi *Minta TTD* dan *Batalkan Form* beralasan | `ConfirmModal` (`requireReason`, `maxReasonLength`) | katalog | REUSE | Alasan 1–500 |
| Pesan galat dan berhasil | `InformationAlert` | katalog | REUSE | Di tempat aksi |
| Lencana keadaan | `StatusBadge` | katalog | REUSE | Nada `info`/`pending`/`active`/`inactive`/`warning` |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kotak: *"Memuat Form Permintaan Rawat Inap..."*; panel form: kerangka `EmergencyAssessmentSection`; pilihan: teks muat `FilterSelect` |
| Kosong | *"Belum ada Form Permintaan Rawat Inap."*; alergi *"Belum ada data alergi"*; penjamin *"Tanpa penjamin (umum)"*; diagnosis *"Belum ada diagnosis ICD-10 pada kunjungan ini."* |
| Gagal | Kotak: *"Form Permintaan Rawat Inap tidak dapat dimuat."* + *Coba lagi*; panel form: pesan server + *Coba lagi*; aksi: pesan server di tempat aksi |
| Tanpa hak akses | Kotak: *"Anda tidak memiliki hak akses Form Permintaan Rawat Inap."*; panel form: pemberitahuan hak akses baku |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Emergency Installation Management / Emergency Admission Request

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/emergency-installation-management/emergency-admission-requests?emergencyVisitId=` | Kotak keadaan per tindak lanjut | `EmergencyAdmissionRequest : Read` |
| `GET` | `…/emergency-admission-requests/{id}` | Isi panel form | `EmergencyAdmissionRequest : Read` |
| `POST` | `…/emergency-admission-requests` | *Buat Form* | `EmergencyAdmissionRequest : Create` |
| `PUT` | `…/emergency-admission-requests/{id}` | *Simpan Draf* | `EmergencyAdmissionRequest : Update` |
| `POST` | `…/emergency-admission-requests/{id}/request-signature` | *Minta TTD* | `EmergencyAdmissionRequest : Update` |
| `POST` | `…/emergency-admission-requests/{id}/cancel` | *Batalkan Form* | `EmergencyAdmissionRequest : Cancel` |
| `GET` | `…/emergency-admission-requests/inpatient-doctor-options?serviceUnitId=&search=` | Pilihan DPJP usulan | `EmergencyAdmissionRequest : Read` |

#### Health Services / Master Data (sudah ada, dipakai lewat service layar admisi Rawat Inap)

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/master-data/service-units/options?serviceUnitType=2` | Pilihan unit layanan | `ServiceUnit : Read` |
| `GET` | `/v1/health-services/master-data/patient-classes/options?isForInpatient=true` | Pilihan kelas perawatan | Hak baca kelas perawatan |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` atas 7 berkas yang dibuat/diubah | 0 error, 0 warning | `PASS` | Keluaran perintah, 12 Oktober 2026 |
| Grep komentar (`//`, `/* */`, `{/* */}`) pada 6 berkas baru dan baris tambahan diff | Nihil | `PASS` | `grep` |
| Grep anti-regresi: warna literal, `style={{`, `<button`, `<table`, `fw-*`/`fs-*` | Nihil | `PASS` | `grep` |
| Grep `console.`, `localStorage`, `sessionStorage` (aturan 17.6 nomor 5) | Nihil | `PASS` | `grep` |
| Teks *"Tidak ada alergi"* pada berkas task ini | Nihil | `PASS` | `grep` |
| Akhiran baris | 7 berkas CRLF, tanpa BOM, nol LF tunggal (hitungan byte Node) | `PASS` | Skrip hitung byte |
| `npm run build` | Belum dijalankan — port 3000 dipakai `next dev` milik pemilik; pemilik memilih menguji dulu di server dev | `NOT RUN` | Keputusan pemilik 12 Oktober 2026 |
| Uji layar putaran A (A-01…A-14), 16.12–16.43 | 12 terbukti, 1 sebagian (A-01), 1 tidak terbukti (A-11); satu cacat tampilan kecil ditemukan dan diperbaiki (*"Tunai — Tunai"*) | `PASS` sebagian | [penilaian putaran A](../../../testing/2026-10-12-laporan-uji-form-permintaan-rawat-inap-putaran-a.md#7-penilaian-ulang-dari-bukti-mentah--agent-pengembang-12-oktober-2026) |

Uji manual: `PASS` sebagian — A-11 dan keadaan awal A-01 diuji ulang (laporan uji 7.4). ESLint diulang atas `emergency-admission-request.utils.js` sesudah perbaikan: 0 error.

`AUTOMATED TEST: SKIPPED (opsional) — kebijakan tim: unit test tidak ditulis atau dijalankan untuk task ini.`

**Tidak dijalankan:** `npm run build` (keputusan pemilik: port 3000 dipakai server uji dev), unit test (kebijakan tim).

---

## 7. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Panel form tampil hanya pada tindak lanjut bertanda `requiresAdmissionRequest`; *Buat Form* hanya pada tindak lanjut terkonfirmasi | **Sebagian** | Kotak pada baris rawat inap terbukti (`A-01.png`, `A-13.png`); keadaan *"Belum ada Form…"* + *Buat Form* tidak terekam (bukti 16.12 tertimpa); baris jenis lain dan tindak lanjut *Draf* tidak diuji |
| 2 | Identitas, alergi (*"Belum ada data alergi"* bila kosong, tidak pernah *"Tidak ada alergi"*), penjamin dan hak kelas, diagnosis utama paling atas — baca-saja | Terpenuhi | `A-01.png`…`A-13.png`: alergi *"Belum ada data alergi"*, diagnosis *(utama)*, keadaan diagnosis kosong (`A-13.png`); hak kelas tidak teruji (pasien tunai) |
| 3 | Pilihan DPJP dari endpoint server; keterangan unit tanpa jadwal tampil | Terpenuhi | `A-04-1.png`, `A-04-2.png` |
| 4 | Penolakan `400`/`409` tampil apa adanya di tempat aksi; isian tidak dikosongkan | **Sebagian** | `400` di bawah form (`A-05.png`) dan di dialog (`A-06.png`) terbukti, isian tetap. `409` konkurensi di layar (A-11) **tidak terbukti** |
| 5 | *Minta TTD* mengubah lencana dan menampilkan nama DPJP aktif; catatan *Kembalikan* tampil di riwayat dan form kembali dapat diubah | Terpenuhi | `A-07.png`, `A-10.png` |
| 6 | *Batalkan Form* meminta alasan; form batal dan *Buat Form* tersedia lagi | Terpenuhi | `A-12.png` |
| 7 | Diff hanya berkas Cakupan; nol komentar baru; `eslint` 0 error; `npm run build` lulus | **Sebagian** | Diff, komentar, ESLint terpenuhi; build `NOT RUN` (ditunda atas persetujuan pemilik karena server dev aktif) |

**Definition of Done belum terpenuhi:** acceptance 1 dan 4 sebagian, acceptance 7 (build) belum. Penilaian: [penilaian putaran A](../../../testing/2026-10-12-laporan-uji-form-permintaan-rawat-inap-putaran-a.md#7-penilaian-ulang-dari-bukti-mentah--agent-pengembang-12-oktober-2026). Koreksi 12 Oktober 2026: status *🟢 SELESAI* yang ditulis agen uji dikembalikan ke 🟡 — tidak didukung bukti.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Sampai jenis tindak lanjut `RANAP` diberi tanda `requiresAdmissionRequest = true` (prasyarat P1, diisi pemilik lewat API master), kotak form **tidak muncul** di baris rawat inap dan pembuatan form akan ditolak `409` aturan 1 |
| Masalah yang diketahui | Notifikasi hasil aksi memakai `InformationAlert` di dalam panel, bukan `ToastStack` — mengikuti pola tab perawat IGD yang lain. Tombol *Jalankan* belum ditahan di layar (`FE-IGD-061`). Teks *"Tidak ada alergi aktif"* yang sudah ada di `emergency-assessment-patient-card.jsx` (di luar task ini) berbenturan dengan `IGD-FACT-117`; dicatat, tidak diubah |
| Dependency backend | `BE-IGD-072` 🟡 (dipakai); *Kembalikan* dan *Perlu TTD* dari layar dokter menunggu `FE-IGD-060`; tanda tangan dan cetak menunggu `BE-IGD-074` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 1 `M` (`emergency-assessment-disposition-tab.jsx`) dan 6 `??` berkas baru milik task ini; berkas `M`/`??` lain di working tree milik sesi lain dan tidak disentuh. Nol stage, nol commit |
| Langkah berikutnya | Pemilik mengisi tanda `RANAP` dan butir hak akses, lalu menjalankan putaran A dari [panduan](../../../testing/2026-10-12-panduan-uji-form-permintaan-rawat-inap-putaran-a.md); sesudah itu agent menjalankan `npm run build` atas izin menghentikan server dev |
