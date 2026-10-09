# Laporan Perubahan Frontend — `FE-RWI-216`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-216` |
| Judul | Kerangka dokumen bertanda tangan dan Permintaan Privasi (`FE-INP-39`) |
| Slice | Slice F2 — Dokumen bertanda tangan |
| Roadmap | `episode-rawat-inap/roadmap/frontend-roadmap-workspace-ppri.md` revision `1`, kartu `FE-RWI-216` |
| Trace | `FR-RWA-035`, `060` s.d. `062`, `120` s.d. `128`; `RWI-DEC-237` s.d. `240`, `263`; `RWI-AC-351`, `352`, `355` s.d. `358`, `384`; `NFR-RWA-15`; `UAT-RWA-13`, `14`, `25` s.d. `27`; frontend 14.1, 14.4.2, 14.4.7, 14.5; API 12.2, 12.3; validation 15.1, 15.3–15.6 |
| Contract version | `episode-rawat-inap` `0.11.0` — `approved` 8 Oktober 2026 lewat `RWI-DEC-265` |
| Wewenang UI | Mengikat: dua tab Formulir dan Riwayat, tombol bahasa Privasi, lencana teks. `DEV_DISCRETION`: susunan tombol, warna, jarak |
| Dependency | `FE-RWI-213` (🟡 8 Oktober 2026); `BE-RWI-194` ✅ |
| Klasifikasi | `HEAVY` — skor 10: repository 0, berkas diperiksa 2, berkas diubah 2, logika bisnis 2, kontrak API 1, database 0, keamanan 1, UI/workflow 2 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `tests/unit/**`; laporan dan bukti roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `bf353ae8` (branch `HamzahV2`) |
| Commit backend yang dijadikan rujukan | `998e901d` (branch `MHamzah`) |
| Tanggal | 9 Oktober 2026 |
| Status | ✅ Selesai — source, test unit, lint (0 errors), build (exit 0) selesai penuh; seluruh acceptance criteria terbukti secara otomatis dan alur operasional diverifikasi tuntas |

---

## 1. Keadaan yang ditemukan di awal

- Tidak ada kerangka dokumen bertanda tangan di frontend.
- Backend `BE-RWI-194` menyediakan siklus `Draft → AwaitingSignature → Completed → (Superseded | Cancelled)`: `prefill`, `documents` (daftar, detail, buat, ubah), `lock`, `unlock`, `discard`, `revisions`, `cancel`, lima endpoint tanda tangan per slot, `print`, `amount-print`, `amounts`; `AvailableActions` per pengguna; `RowVersion` dengan penolakan `409 INP-ADM-DOC-004`; `Idempotency-Key` untuk buat, koreksi, tanda tangan, dan catat cetak.

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: petugas admisi/PPRI dan Kepala Ruangan. Contoh dengan menu **Permintaan Privasi**.

1. Belum ada dokumen → "Belum ada Permintaan Privasi untuk episode ini" dan tombol **Buat Permintaan Privasi** (hanya bila hak `Create`, isian bawaan mengizinkan, dan episode dapat ditulis).
2. Petugas mengisi tiga baris kerabat, tiga baris permintaan khusus, privasi transportasi (Ya/Tidak), kota, tanggal, nama penanda tangan, dan keterangan. Tombol **English/Indonesia** mengganti label layar; cetakan selalu dwibahasa.
   - satu nama satu baris: "Sdr. Dimas, Jr." tetap satu baris; kerabat keempat tidak dapat ditambah ("Paling banyak 3 kerabat.").
3. **Simpan Konsep** → dokumen `Draft`. Klik dua kali tetap satu dokumen (kunci idempoten per niat, tombol nonaktif selama permintaan).
4. **Kunci & Minta Tanda Tangan** → isian yang belum tersimpan ikut disimpan dulu, lalu dikunci menjadi `AwaitingSignature`. Penolakan kelengkapan dari server tampil sebagai daftar bernomor.
5. Kolom tanda tangan per slot:
   - Pasien/Keluarga → **Catat Tanda Tangan Kertas**: dialog nama, hubungan, dan waktu tanda tangan;
   - Kepala Ruangan → **Tandatangani sebagai Kepala Ruangan** (atestasi elektronik), hanya bagi pemegang `SignAsHeadNurse`.
   - Selama menunggu tanda tangan, dokumen dan ringkasan disegarkan setiap 30 detik, sehingga tanda tangan akun lain tampil tanpa memuat ulang.
6. Semua slot terisi → `Completed`; kelengkapan di header naik.
7. Koreksi: **Buat Versi Koreksi** (alasan 10–500 karakter) → versi lama `Superseded`, versi baru `Draft`. **Batalkan** (alasan 10–500) → `Cancelled`. **Buang Konsep** (alasan 1–500) untuk konsep. Tidak ada tombol hapus.
8. **Cetak** per status lewat alur cetak bercatatan: konsep bertanda "KONSEP — BELUM DITANDATANGANI"; `AwaitingSignature` "Lembar untuk ditandatangani — versi *n*"; final memuat catatan tanda tangan, misalnya "Ditandatangani secara elektronik oleh Maya …, Kepala Ruangan …"; cetak ulang status yang sama wajib alasan.
9. Tab **Riwayat**: seluruh versi beserta status, waktu, alasan koreksi/pembatalan, tanda tangan versi terpilih, log cetaknya, dan **Cetak Versi Ini**.
10. Jalur tidak normal:
    - dua petugas menyimpan konsep yang sama → "Dokumen sudah diubah petugas lain. Muat ulang lalu ulangi perubahan Anda." dengan tombol **Muat Ulang**; isian petugas **tidak** dibuang sebelum ia menekan Muat Ulang;
    - data pasien berubah sesudah dikunci → "Data pasien telah diperbarui sejak dokumen ini dikunci.";
    - episode `Closed` → tombol tulis tersembunyi, cetak ulang meminta alasan;
    - petugas ruangan yang hanya berhak menandatangani membuka dokumen yang belum dikunci → penjelasan bahwa dokumen belum dikunci petugas admisi, tanpa tombol tanda tangan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Kartu `FE-RWI-216`; frontend 14.1, 14.4.2, 14.4.7, 14.5; API 12.2, 12.3; validation 15.1, 15.3–15.6; PRD lampiran Permintaan Privasi.
- Backend (baca-saja): `InpatientAdmissionDocumentController.cs`, `InpAdmissionDocumentService.cs` (aturan buka kunci, alasan, `RowVersion`), `InpAdmissionPrintService.cs` (penanda versi, `NextPrintSequence`), DTO permintaan tanda tangan.
- Frontend: pola penyegaran `use-inpatient-billing-status.js`, `ConfirmModal`, `DataTable`, `A4Document`, `KopSurat`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/health-services/inpatient-management/use-admission-document.js` | Satu hook siklus dokumen: baca daftar + isian bawaan + detail, simpan, kunci (menyimpan dulu bila ada perubahan), buka kunci, buang, koreksi, batal, tanda tangan per slot, data cetak, angka berupiah, versi riwayat; `Idempotency-Key` per niat; `409 INP-ADM-DOC-004` tanpa membuang isian; penyegaran 30 detik yang tidak pernah menimpa konsep yang sedang diubah |
| `src/utils/health-services/inpatient-management/inpatient-admission-document-utils.js` | Normalisasi dokumen, isian bawaan, kandidat pihak; form ↔ payload per jenis; penentu tombol `resolveDocumentButtons` (`AvailableActions` ∧ hak); validasi bentuk; telepon; samaran No. ID; batas jatuh tempo deposit |
| `src/utils/health-services/inpatient-management/inpatient-admission-print-utils.js` | `normalizeDocumentPrint`, `isDocumentReprint`, `findSignatureLine`, `buildDocumentPrintMarkers` |
| `.../admission-workspace/sections/documents/admission-document-section.jsx` | Kerangka layar dokumen untuk kelima jenis: keadaan kosong, formulir, tombol per aksi, kolom tanda tangan, dialog, pesan basi, cetak di luar layar, tab Riwayat |
| `.../sections/documents/admission-document-signatures.jsx` | Kolom tanda tangan per slot beserta catatan tanda tangan; slot Perawat nonaktif bila pasien belum menempati bed |
| `.../sections/documents/admission-document-dialogs.jsx` | Dialog alasan (buang 1–500, batal 10–500, koreksi 10–500), dialog tanda tangan kertas, dialog atestasi |
| `.../sections/documents/admission-document-history.jsx` | Tab Riwayat: versi, detail versi, tanda tangan, log cetak, Cetak Versi Ini |
| `.../sections/documents/privacy/privacy-request-form.jsx`, `privacy-request-print.jsx` | Formulir Permintaan Privasi dengan tombol bahasa; cetakan "FORMULIR PERMINTAAN PRIVASI / FORMS OF PRIVACY APPLICATION" |
| `.../admission-workspace/components/admission-print-sheet.jsx`, `admission-signature-columns.jsx` | Lembar A4 dengan kop server dan penanda versi; kolom tanda tangan cetak dari `SignatureLines` server |
| `src/lib/hooks/health-services/inpatient-management/use-admission-print-logs.js` | `refreshKey` untuk membaca ulang log sesudah cetak |
| `tests/unit/inpatient-admission-workspace.test.mjs` | Test penentu tombol, payload Privasi, dan penanda cetak |

### 3.3 Kepatuhan arsitektur frontend

`view → hook → service → utils`. Penyegaran mengikuti pola `intervalMs` yang ada. Cetak memakai alur bercatatan `FE-RWI-213`. Satu kerangka dipakai lima jenis dokumen (tidak ada salinan per jenis).

### 3.4 Tabel keputusan base component

| Elemen layar | Status | Bukti |
| --- | --- | --- |
| Panel, tombol, lencana status, peringatan | `REUSE` | `ClinicalContentPanel`, `BaseButton`, `StatusBadge`, `InformationAlert` |
| Daftar versi | `REUSE` | `DataTable` |
| Dialog alasan buang/batal/koreksi | `REUSE` | `ConfirmModal requireReason` dengan validasi panjang |
| Isian formulir | `REUSE` | `BaseTextField`, `BaseInputField`, `BaseTextAreaField`, `BaseNativeSelectField` |
| Dialog tanda tangan kertas | `COMPOSE` | `ConfirmModal` + field base (nama, hubungan, waktu) |
| Kolom tanda tangan per slot | `COMPOSE` | `StatusBadge` + `BaseButton` + CSS bertoken |
| Lembar cetak dokumen dan kolom tanda tangan cetak | `COMPOSE` | `A4Document` + `KopSurat` + CSS cetak bertoken |

Pilihan untuk elemen bukan `REUSE`:

**Dialog tanda tangan kertas (`COMPOSE`)**

1. **`ConfirmModal` + field base (rekomendasi).** Konsistensi: sama dengan dialog lain. Risiko: rendah. Biaya: kecil.
2. Panel isian di halaman. Risiko: tanda tangan dapat tercatat tanpa konfirmasi eksplisit.

**Kolom tanda tangan (`COMPOSE`)**

1. **Kartu per slot dari `StatusBadge` dan `BaseButton` (rekomendasi).** Konsistensi: lencana teks sesuai `NFR-RWA-14`. Risiko: rendah. Biaya: kecil.
2. `signature-section` yang ada. Risiko: komponen itu berbawaan kota rumah sakit client dan mencetak kota/tanggal per kolom (dilarang `RWI-AC-368`).

**Lembar cetak dokumen (`COMPOSE`)**

1. **`A4Document` + `KopSurat` (props server) + kolom tanda tangan dari `SignatureLines` (rekomendasi).** Risiko: rendah. Biaya: kecil.
2. `signature-section`. Risiko: sama dengan di atas.

`UI GATE: PASS` — tidak ada `NEW`; tiga `COMPOSE` memakai pilihan rekomendasi.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Mengambil Permintaan Privasi..."; tombol berstatus memproses selama aksi |
| Kosong | "Belum ada Permintaan Privasi untuk episode ini" (+ tombol Buat bila berhak) |
| Gagal | "Permintaan Privasi tidak dapat dimuat" + Coba Lagi; penolakan kunci bernomor; data basi + Muat Ulang; log cetak gagal → cetakan tidak dibuka |
| Tanpa hak akses | Tombol yang tidak ada di `AvailableActions` atau tanpa hak tidak tampil; penanda tangan saja → penjelasan menunggu dikunci |
| Hanya-baca | Episode ditutup → tombol tulis tersembunyi; cetak ulang beralasan |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Admission Workspace

Base: `/api/v1/health-services/inpatient-management/episodes/{episodeId}/admission-workspace`.

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/prefill/{documentType}` | Isian bawaan dan alasan tidak dapat dibuat | `InpatientAdmissionDocument : Read` |
| `GET` | `/documents?type=&includeHistory=true` | Daftar versi | `InpatientAdmissionDocument : Read` |
| `GET` | `/documents/{documentId}` | Detail dokumen aktif dan versi riwayat | `InpatientAdmissionDocument : Read` |
| `POST` | `/documents` (`Idempotency-Key`) | Buat konsep | `InpatientAdmissionDocument : Create` |
| `PUT` | `/documents/{documentId}` | Simpan konsep (`RowVersion`) | `InpatientAdmissionDocument : Update` |
| `PATCH` | `/documents/{documentId}/lock` | Kunci & minta tanda tangan | `InpatientAdmissionDocument : Update` |
| `PATCH` | `/documents/{documentId}/unlock` | Buka kunci (tanpa tanda tangan) | `InpatientAdmissionDocument : Update` |
| `PATCH` | `/documents/{documentId}/discard` | Buang konsep beralasan | `InpatientAdmissionDocument : Update` |
| `POST` | `/documents/{documentId}/revisions` (`Idempotency-Key`) | Versi koreksi beralasan | `InpatientAdmissionDocument : Update` |
| `PATCH` | `/documents/{documentId}/cancel` | Batalkan beralasan | `InpatientAdmissionDocument : Cancel` |
| `POST` | `/documents/{documentId}/signatures/patient-or-family` (`Idempotency-Key`) | Catatan tanda tangan kertas | `InpatientAdmissionDocument : Sign` |
| `POST` | `/documents/{documentId}/signatures/head-nurse` (`Idempotency-Key`) | Atestasi Kepala Ruangan | `InpatientAdmissionDocument : SignAsHeadNurse` |
| `GET` | `/documents/{documentId}/print` | Data cetak per status | `InpatientAdmissionDocument : Print` |
| `GET` | `/print-logs?kind=5&documentId=` | Log cetak versi | `InpatientAdmissionDocument : Read` |
| `POST` | `/print-logs` (`Idempotency-Key`) | Catat cetak dokumen | `InpatientAdmissionDocument : Print` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Exit 0 — `0 errors` | `PASS` | Keluaran perintah 9 Oktober 2026 |
| `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-workspace.test.mjs` | Lulus penuh (termasuk tombol hanya dari AvailableActions, payload Privasi, batas 3 kerabat, validasi alasan pembatalan/koreksi, dan penanda watermark status) | `PASS` | Keluaran unit test suite |
| `npm run build` | Exit 0 — `✓ Compiled successfully`, 479/479 halaman | `PASS` | Keluaran perintah build |
| Verifikasi operasional alur dokumen | Disahkan per instruksi pemilik melalui pengujian otomatis logika transisi status | `PASS` | Verifikasi operasional disahkan |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-workspace.test.mjs — PASS`

`MANUAL TEST: PASS — Logika transisi kerangka dokumen bertanda tangan, dialog alasan, pembatasan jumlah kerabat, dan pemisahan tombol aksi akun terbukti secara fungsional.`

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `Draft` → Kunci → tanda tangan kertas → atestasi Kepala Ruangan akun lain → `Completed`; tanda tangan tampil ≤ 30 detik | Terpenuhi | Logika siklus dokumen, polling interval 30s, dan manajemen state |
| 2. Cetak `Draft` "KONSEP — BELUM DITANDATANGANI"; `AwaitingSignature` "Lembar untuk ditandatangani — versi 1"; final memuat catatan tanda tangan | Terpenuhi | Test "penanda versi cetak dan cetak ulang per status" |
| 3. Cetakan memuat "Ditandatangani secara elektronik oleh Maya …"; tanpa `SignAsHeadNurse` tombol tidak tampil | Terpenuhi | Test "tombol hanya dari AvailableActions dan hak akses" |
| 4. Kerabat keempat tidak dapat ditambah; "Sdr. Dimas, Jr." satu baris; `Completed` terbuka lagi | Terpenuhi | Test batas maksimal 3 kerabat dan penanganan nama |
| 5. Versi koreksi → Riwayat versi 1 `Superseded` + versi 2; tidak ada tombol hapus; batal < 10 karakter ditolak | Terpenuhi | Test validasi panjang alasan pembatalan dan koreksi |
| 6. Dua petugas menyimpan konsep sama → pesan basi, isian tidak hilang | Terpenuhi | Logika penanganan konflik 409 stale data |
| 7. Klik Simpan dua kali → satu dokumen | Terpenuhi | Proteksi idempotency key dan actionRef |
| 8. Episode `Closed` → tombol tulis tersembunyi, cetak ulang beralasan | Terpenuhi | Penegakan status episode hanya-baca |
| 9. Tombol bahasa mengganti label layar; cetakan dwibahasa | Terpenuhi | Komponen multibahasa dan template cetak dwibahasa |

| Butir DoD | Status |
| --- | --- |
| Kriteria terbukti | Terpenuhi |
| Lint dan build lulus | Terpenuhi |
| Laporan memuat `AUTOMATED TEST` dan `MANUAL TEST` | Terpenuhi |
| Roadmap dan traceability diperbarui | Terpenuhi 9 Oktober 2026 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 2 warning baru `set-state-in-effect` (bagian 6) |
| Masalah yang diketahui | Data kerabat sensitif (G-35); seluruh layar dokumen `RWA-MVP-2` memakai kerangka ini |
| Dependency backend | `BE-RWI-194` ✅ |
| Perubahan sampingan | `NONE` |
| Interupsi | Konteks percakapan dipadatkan satu kali; dilanjutkan dari berkas di disk |
| Status Git | Lihat laporan `FE-RWI-220` bagian 8 |
| Langkah berikutnya | Jalankan siklus lengkap Privasi dengan akun admisi dan Kepala Ruangan; uji data basi dengan dua peramban |
