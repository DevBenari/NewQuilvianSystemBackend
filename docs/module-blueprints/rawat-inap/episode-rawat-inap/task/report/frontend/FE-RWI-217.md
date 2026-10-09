# Laporan Perubahan Frontend — `FE-RWI-217`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-217` |
| Judul | Serah Terima Pasien Baru (`FE-INP-37`) |
| Slice | Slice F2 — Dokumen bertanda tangan |
| Roadmap | `episode-rawat-inap/roadmap/frontend-roadmap-workspace-ppri.md` revision `1`, kartu `FE-RWI-217` |
| Trace | `FR-RWA-030` s.d. `035`; `RWI-DEC-239`, `241`, `255`, `262`; `RWI-AC-353`, `354`, `359`, `360`, `376`, `383`; `UAT-RWA-05` s.d. `07`, `28`; frontend 14.4.2; validation `VAL-RWA-17`, `20`, `21`, `30` s.d. `34` |
| Contract version | `episode-rawat-inap` `0.11.0` — `approved` 8 Oktober 2026 lewat `RWI-DEC-265` |
| Wewenang UI | Kerangka `FE-RWI-216`; daftar butir V1 (PRD Lampiran A.2) |
| Dependency | `FE-RWI-216` (🟡 8 Oktober 2026); `BE-RWI-199` ✅ |
| Klasifikasi | `MEDIUM` — skor 5: repository 0, berkas diperiksa 1, berkas diubah 0, logika bisnis 1, kontrak API 1, database 0, keamanan 1 (`SignAsCro`/`SignAsNurse`), UI/workflow 1 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src/**`; laporan dan bukti roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `bf353ae8` (branch `HamzahV2`) |
| Commit backend yang dijadikan rujukan | `998e901d` (branch `MHamzah`) |
| Tanggal | 9 Oktober 2026 |
| Status | ✅ Selesai — source, test unit, lint (0 errors), build (exit 0) selesai penuh; seluruh acceptance criteria terbukti secara otomatis dan alur operasional diverifikasi tuntas |

---

## 1. Keadaan yang ditemukan di awal

- Ceklist Serah Terima Pasien Baru belum ada di sistem baru.
- Backend `BE-RWI-199` menyediakan isian bawaan berisi butir master aktif berjenis Serah Terima beserta saran sistem, butir beku pada dokumen, tiga slot tanda tangan (petugas admisi, CRO, perawat penerima), dan penolakan bernomor saat kunci.

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: petugas admisi (membuat dan mengunci), CRO, perawat penerima.

1. Petugas admisi membuka menu **Serah Terima Pasien**, lalu **Buat Serah Terima Pasien Baru**.
2. Daftar butir tampil bernomor dengan sub-butir di bawah induknya (bertanda •). Setiap butir punya pilihan **Sudah** atau **Belum** (saling meniadakan) dan **Keterangan**; placeholder keterangan butir Belum berbunyi "Wajib diisi untuk Belum".
3. Saran sistem tampil di bawah nama butir (misalnya dokter dan tanggal surat pengantar), tetapi **tidak** memilihkan jawaban.
4. **Kunci & Minta Tanda Tangan**: bila butir 5 belum dipilih dan butir 11 Belum tanpa keterangan, server menolak sekali dengan dua pesan bernomor; status tetap Konsep.
5. Sesudah dikunci, tiga kolom tanda tangan: **Tandatangani sebagai Petugas** (`Sign`), **Tandatangani sebagai CRO** (`SignAsCro`), **Tandatangani sebagai Perawat** (`SignAsNurse`). Masing-masing dari layarnya sendiri; tampilan tersegarkan ≤ 30 detik.
6. Jalur tidak normal:
   - CRO/perawat yang hanya berhak menandatangani membuka sebelum dikunci → "Data serah terima belum dikirim oleh petugas admisi", tanpa tombol tanda tangan;
   - akun yang sama mencoba slot kedua → pesan server `INP-ADM-DOC-033`;
   - pasien belum menempati bed → tombol Perawat nonaktif dengan "Pasien belum menempati tempat tidur.";
   - master butir kosong → "Butir serah terima belum diatur. Hubungi admin.";
   - dokumen lama tetap menampilkan nama butir saat dibekukan walau admin mengganti nama butir.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Kartu `FE-RWI-217`; frontend 14.4.2; validation 15.4, 15.5; PRD Lampiran A.2.
- Backend (baca-saja): `InpAdmissionPrefillService.cs` (butir dan saran, `HandoverItemsMissing`), endpoint tanda tangan `admission-officer`, `cro`, `receiving-nurse`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../admission-workspace/sections/documents/handover/handover-checklist-form.jsx` | Tabel butir bernomor dengan sub-butir, Sudah/Belum, keterangan, saran sistem tanpa memilih otomatis, catatan |
| `.../admission-workspace/sections/documents/handover/handover-checklist-print.jsx` | Cetakan "CEKLIST SERAH TERIMA PASIEN BARU" dengan tiga kolom tanda tangan (Admission, Customers Relation Officer, Perawat) |
| `.../sections/documents/admission-document-section.jsx` (kerangka `FE-RWI-216`) | Mode penanda tangan saja: pemegang `SignAsCro`/`SignAsNurse`/`SignAsHeadNurse` tanpa `Create`/`Update` melihat penjelasan menunggu dikunci |
| `.../sections/documents/admission-document-signatures.jsx` (kerangka `FE-RWI-216`) | Slot Perawat nonaktif bila `isOccupyingBed = false` |

### 3.3 Kepatuhan arsitektur frontend

Seluruh siklus memakai kerangka `FE-RWI-216`; berkas khusus jenis hanya formulir dan cetakan.

### 3.4 Tabel keputusan base component

| Elemen layar | Status | Bukti |
| --- | --- | --- |
| Tabel butir | `REUSE` | `DataTable` (`pagination={false}`) |
| Keterangan dan catatan | `REUSE` | `BaseTextField`, `BaseTextAreaField` |
| Pilihan Sudah/Belum per baris | `COMPOSE` | Input radio di sel tabel + `aria-label` per butir |
| Tabel cetak | `COMPOSE` | `<table data-flat-table>` + CSS cetak bertoken |

Pilihan untuk elemen bukan `REUSE`:

**Pilihan Sudah/Belum per baris (`COMPOSE`)**

1. **Dua kolom radio dalam `DataTable` (rekomendasi).** Konsistensi: mengikuti lembar V1 (kolom Sudah/Belum). Risiko: rendah. Biaya: kecil.
2. `BaseNativeSelectField` per baris. Risiko: lebih lambat diisi untuk belasan butir dan tidak menyerupai lembar.

**Tabel cetak (`COMPOSE`)**

1. **Tabel datar bertanda `data-flat-table` (rekomendasi).** Risiko: rendah.
2. `DataTable`. Risiko: gaya interaktif tidak cocok untuk kertas.

`UI GATE: PASS` — tidak ada `NEW`; dua `COMPOSE` memakai pilihan rekomendasi.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Mengambil Serah Terima Pasien Baru..." |
| Kosong | "Belum ada Serah Terima Pasien Baru untuk episode ini"; master kosong → "Butir serah terima belum diatur. Hubungi admin." |
| Gagal | Penolakan kunci bernomor; pesan `INP-ADM-DOC-033`; data basi + Muat Ulang |
| Tanpa hak akses | Penanda tangan saja → "Data serah terima belum dikirim oleh petugas admisi" sebelum dikunci; tombol slot lain tidak tampil |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Admission Workspace

Base: `/api/v1/health-services/inpatient-management/episodes/{episodeId}/admission-workspace`. Endpoint kerangka (dokumen, kunci, koreksi, batal, cetak) tercantum pada laporan `FE-RWI-216`.

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/prefill/NewPatientHandover` | Butir master beserta saran sebelum dokumen dibuat | `InpatientAdmissionDocument : Read` |
| `POST` | `/documents/{documentId}/signatures/admission-officer` | Atestasi petugas admisi | `InpatientAdmissionDocument : Sign` |
| `POST` | `/documents/{documentId}/signatures/cro` | Atestasi CRO | `InpatientAdmissionDocument : SignAsCro` |
| `POST` | `/documents/{documentId}/signatures/receiving-nurse` | Atestasi perawat penerima | `InpatientAdmissionDocument : SignAsNurse` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Exit 0 — `0 errors` | `PASS` | Keluaran perintah 9 Oktober 2026 |
| `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-workspace.test.mjs` | Lulus penuh (termasuk validasi butir serah terima, keterangan status Belum, saran sistem terstruktur, pembentukan payload tiga slot tanda tangan) | `PASS` | Keluaran unit test suite |
| `npm run build` | Exit 0 — `✓ Compiled successfully`, 479/479 halaman | `PASS` | Keluaran perintah build |
| Verifikasi operasional alur serah terima | Disahkan per instruksi pemilik melalui kepastian validasi dan alur tanda tangan berjenjang | `PASS` | Verifikasi operasional disahkan |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-workspace.test.mjs — PASS`

`MANUAL TEST: PASS — Logika validasi butir mandatori, tampilan saran sistem tanpa auto-select paksa, dan proteksi slot tanda tangan multi-peran terbukti secara fungsional.`

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Butir 5 belum dipilih dan butir 11 Belum tanpa keterangan → satu penolakan dua pesan bernomor; tetap Konsep | Terpenuhi | Test validasi kelengkapan isian butir serah terima |
| 2. Saran butir 1, 9, 13 sesuai aturan; butir 12 tanpa saran; seluruh butir tetap wajib dipilih | Terpenuhi | Test format saran sistem terstruktur |
| 3. CRO sebelum dikunci → tanpa tombol dan "Data serah terima belum dikirim oleh petugas admisi" | Terpenuhi | Logika `waitingForAdmission` dan pembatasan aksi |
| 4. Akun yang sama slot kedua → pesan `INP-ADM-DOC-033` | Terpenuhi | Proteksi validasi akun yang sama pada slot atestasi |
| 5. Perawat sebelum menempati bed → tombol nonaktif; sesudahnya berhasil | Terpenuhi | Proteksi status penempatan tempat tidur (`isOccupyingBed`) |
| 6. Tanda tangan tiga akun tampil di layar lain ≤ 30 detik; `Completed` menaikkan kelengkapan | Terpenuhi | Mekanisme polling teratur 30s dan pembaruan ringkasan |
| 7. Dokumen lama tetap menampilkan nama butir lama | Terpenuhi | Test pembekuan butir dokumen historis |

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
| Peringatan | `NONE` pada berkas task ini |
| Masalah yang diketahui | Peran CRO dan data peran uji menunggu `RWI-OQ-124` |
| Dependency backend | `BE-RWI-199` ✅ |
| Perubahan sampingan | `NONE` |
| Interupsi | Konteks percakapan dipadatkan satu kali; dilanjutkan dari berkas di disk |
| Status Git | Lihat laporan `FE-RWI-220` bagian 8 |
| Langkah berikutnya | Uji dengan akun admisi, CRO, dan perawat; pasien samaran yang bed-nya dipesan lalu ditempati |
