# Laporan Perubahan Frontend — `FE-RJE-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RJE-002` |
| Judul | Pemberitahuan penyerahan tagihan |
| Slice | `MVP-4` — `EPIC RJE-07` |
| Roadmap | [roadmap/e2e-frontend-roadmap.md](../../../roadmap/e2e-frontend-roadmap.md), kartu `FE-RJE-002` |
| Trace | `FR-RJE-062`; `RJ-E2E-FE-004`; `RJ-E2E-DEC-017` (kontrak `1.0.1` approved); `03-frontend-architecture.md` V2.2, V2.7; `SEC-RJ-004` |
| Contract version | `RJ-E2E-CONTRACT-001@1.0.1` — `approved` (`POST /doctor-queues/{id}/finish-consultation` + `billingHandoffIssues: string[]`, `BE-RJE-013`) |
| Wewenang UI | Bentuk pemberitahuan `DEV_DISCRETION` (`RJ-E2E-FE-004`); syaratnya terlihat oleh dokter dan tidak membuat konsultasi tampak gagal |
| Dependency | `BE-RJE-013` ✅ (di-commit pemilik) |
| Klasifikasi | `LIGHT` — satu state di hook yang sudah ada dan satu pemberitahuan di view |
| Task mode | `FRONTEND` |
| Target tulis | `V2QuilvianSystemFrontendDev` (`sukmagpV2`); laporan di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `83b8b7274` (`sukmagpV2`) + perubahan `FE-RJE-001` yang belum di-commit |
| Commit backend yang dijadikan rujukan | HEAD `sukmagp` (build `out-rje012b`) |
| Tanggal | 30 September 2026 |
| Status | ✅ **SELESAI** — ketiga acceptance criteria terbukti di Chromium terhadap backend sungguhan |

---

## 1. Keadaan yang ditemukan di awal

Tombol **Simpan Konsultasi → Ya, Simpan** memanggil `POST /doctor-queues/{id}/finish-consultation`
lewat `finishDoctorConsultation`. Sejak `BE-RJE-013`, respons itu membawa `billingHandoffIssues`.
Namun `runAction` di `use-doctor-queue.js` hanya membaca `message` dan membuang sisanya, sehingga
dokter tidak pernah tahu bila jasa konsultasi atau resepnya gagal diserahkan ke Billing.

---

## 2. Proses bisnis dari sisi pengguna

1. dr. B menekan **Simpan Konsultasi**, lalu **Ya, Simpan**.
2. Konsultasi selesai seperti biasa: pesan sukses hijau "Konsultasi dokter selesai." tampil, dan
   pasien keluar dari daftar sedang konsultasi.
3. Bila Billing melaporkan masalah, di bawah pesan sukses muncul pemberitahuan kuning:
   **"Konsultasi selesai, tetapi sebagian tagihan belum terkirim ke Billing"**, disertai "Status
   konsultasi tetap selesai. Billing akan meninjau pelayanan berikut:" dan daftar masalahnya, mis.
   "Jasa konsultasi: CLIN_FACT_RECONCILIATION_REQUIRED".
4. Pemberitahuan hilang saat dokter melakukan aksi antrean berikutnya atau mengubah saringan.

**Jalur tidak normal.** Konsultasi yang gagal diselesaikan tetap menampilkan pesan galat merah seperti
sebelumnya, tanpa pemberitahuan tagihan. Daftar kosong berarti tidak ada pemberitahuan sama sekali.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`use-doctor-queue.js` (`runAction`, `handleFinishConsultation`, titik pembersihan pesan),
`doctor-queue.service.js` (`finishDoctorConsultation`), `useDoctorConsultationWorkspace.js`
(`handleConfirmFinalizeConsultation`), `MessageBox.jsx`, `FinalizeConsultationPanel.jsx`,
`FinalizeConsultationModal.jsx`, `doctor-queue-view.jsx`, `doctor-queue-display-utils.js`,
`base-features/information-alert.jsx`; backend `DoctorQueueController.FinishConsultation` dan
`DoctorQueueActionResponse` (kontrak `1.0.1`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/health-services/registration-management/doctor-queue/doctor-queue-display-utils.js` | `getBillingHandoffIssues(result)` — fungsi murni: camelCase/PascalCase, hanya untaian berisi, selain array dianggap kosong. Berlaku juga bila kelak tombol pindah ke `PATCH /doctor-consultations/{id}/complete` (field sama) |
| `src/lib/hooks/health-services/registration-management/doctor-queue/use-doctor-queue.js` | State `billingHandoffIssues`; diisi dari hasil `handleFinishConsultation`; dikosongkan di setiap titik yang membersihkan pesan (aksi antrean, saringan, simpan skrining); diekspor ke view. Nilai kembali `handleFinishConsultation` tidak berubah |
| `src/components/view/health-services/registration-management/doctor-queues/doctor-queue-view.jsx` | `InformationAlert` variant `warning` berisi kalimat dan daftar masalah, di bawah `MessageBox` |
| `tests/unit/doctor-billing-handoff-issues.test.mjs` | **Baru** (opsional) — 3 test |
| `tests/e2e/doctor-billing-handoff-notice.spec.mjs` | **Baru** — spec runtime R1–R2, pola Bank Darah |

### 3.3 Kepatuhan arsitektur frontend

Normalisasi ada di `utils`, state ada di hook yang sudah ada, dan view hanya merender. Tidak ada
service, slice, atau komponen baru. Teks dirender sebagai teks biasa, tanpa
`dangerouslySetInnerHTML`.

`UI GATE: 2 elemen — REUSE 1, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| Pemberitahuan berwarna peringatan | `InformationAlert` | `base-features/information-alert.jsx` (`variant="warning"`, `title`, `children`) | REUSE | — |
| Kalimat + daftar masalah di dalam pemberitahuan | `InformationAlert` + `<ul>` | idem; `children` menggantikan `message` | COMPOSE | Dirangkai di view, memakai utilitas Bootstrap spasi (`mb-0 mt-2 ps-3`), tanpa CSS baru |

**Temuan pada base component.** `InformationAlert` tidak merender `message` bila `children` diisi.
Karena itu kalimat pengantar ditaruh di dalam `children`. Temuan yang sama memperbaiki satu cacat di
`FE-RJE-001` (lihat laporan itu).

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Selesai tanpa masalah | Pesan sukses hijau saja |
| Selesai dengan masalah | Pesan sukses hijau + pemberitahuan kuning berisi daftar masalah |
| Gagal menyelesaikan | Pesan galat merah seperti sebelumnya; tidak ada pemberitahuan tagihan |
| Aksi berikutnya | Pemberitahuan dibersihkan bersama pesan lain |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Registration Management / Doctor Queue

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/registration-management/doctor-queues/{id}/finish-consultation` | Menyelesaikan konsultasi; membaca `billingHandoffIssues` dari respons | `DoctorQueue : FinishConsultation` (tidak berubah) |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Kode keluar `0` | `PASS` | — |
| `npx eslint` pada berkas task | Nol error; 4 warning di berkas lain atau `useEffect` lama `use-doctor-queue.js` (baris 1686) | `EXISTING WARNING` | Tidak ada warning dari baris yang diubah |
| `npm run test:unit` | 2066 test: 2059 pass, 7 fail | `UNRELATED EXISTING ISSUE` untuk 7 kegagalan | Tujuh kegagalan lama yang sama dengan `FE-RJE-001`; tiga test baru `doctor-billing-handoff-issues.test.mjs` **pass** |
| `npm run build` | Kode keluar `0`, `Compiled successfully in 45s` | `PASS` | — |
| `npx playwright test tests/e2e/doctor-billing-summary-tab.spec.mjs tests/e2e/doctor-billing-handoff-notice.spec.mjs --workers=1` | **9 passed (33,8 detik)** — dua milik task ini, tujuh regresi `FE-RJE-001` | `PASS` | Bagian 6.1 |

`AUTOMATED TEST: node --test tests/unit/doctor-billing-handoff-issues.test.mjs — PASS (3/3)`

### 6.1 Validasi runtime — 30 September 2026

Build standalone di `http://127.0.0.1:3710`, Playwright Chromium, backend sungguhan
`http://localhost:5219` terhadap `QuilvianNewDevSukma`. Seluruh request `/v1/**` diteruskan ke
backend; tidak ada jawaban yang dikarang.

**Data uji yang disiapkan.**
- Antrean `a5a60bd7…` dan `02e83663…` (keduanya sedang konsultasi, tanpa resep) dimajukan ke hari
  ini lewat SQL, lalu tanggalnya dipulihkan sesudah uji.
- SOAP kedua konsultasi diisi, bertanda `TEST-RJE-FE002`, dan diagnosis utama ditambahkan lewat API.
- "Billing dibuat gagal": konsultasi `6d96f2e6…` diberi fakta jasa konsultasi sintetis yang belum
  pasti dan menunggu rekonsiliasi. Producer lalu menolak revisi baru lewat jalur CASE C, cara yang
  sama dengan `BE-RJE-013`.

| No | Skenario | Hasil sebenarnya | AC | Klasifikasi |
| ---: | --- | --- | --- | :---: |
| R1 | Antrean dengan masalah penyerahan → Lanjutkan → Simpan Konsultasi → Ya, Simpan | Backend `200`, `billingHandoffIssues` tidak kosong, `queueStatusName = Completed`. Layar menampilkan pesan sukses dari server **dan** pemberitahuan "Konsultasi selesai, tetapi sebagian tagihan belum terkirim ke Billing" + "Status konsultasi tetap selesai." + setiap butir masalah dari respons | 1, 3 | `PASS` |
| R2 | Antrean tanpa masalah → alur yang sama | Backend `200`, `billingHandoffIssues = []`; pesan sukses tampil; pemberitahuan **tidak** ada | 2 | `PASS` |

`MANUAL TEST: PASS` — dijalankan agent lewat Chromium sungguhan terhadap backend sungguhan.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Daftar tidak kosong → pemberitahuan tampil; status konsultasi tetap selesai (`UAT-19`) | Terpenuhi | R1 |
| 2. Daftar kosong → tidak ada pemberitahuan | Terpenuhi | R2; test unit AC 2 |
| 3. Bentuk pemberitahuan `DEV_DISCRETION` | Terpenuhi — `InformationAlert` peringatan di bawah pesan sukses | R1 |
| DoD: laporan | Terpenuhi | Berkas ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` di luar warning dan kegagalan unit lama (bagian 6) |
| Masalah yang diketahui | Masalah berjenis `OutcomeUnknown` dianggap aman secara klinis oleh backend dan **tidak** masuk daftar; fakta itu dikirim ulang otomatis oleh pekerja `BE-RJE-011`. Jadi pemberitahuan hanya muncul untuk penolakan yang butuh tinjauan |
| Dependency backend | `NONE` |
| Perubahan sampingan | Dua berkas tracked di `test-results/` yang diubah Playwright dipulihkan dengan `git restore -- test-results`. Tanggal antrean uji dipulihkan. Kedua konsultasi uji kini selesai dan satu fakta sintetis `TEST-RJE-FE002-FACT-*` tersisa di antrean rekonsiliasi (data uji) |
| Interupsi | `NONE` |
| Status Git | Perubahan `FE-RJE-001` dan `FE-RJE-002` bersama di working tree; belum di-stage atau di-commit |
| Langkah berikutnya | `FE-RJE-003` (layar Antrean Rekonsiliasi Tagihan Klinis) |
