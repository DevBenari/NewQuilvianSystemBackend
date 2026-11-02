# Laporan Perubahan Frontend — `FE-RWI-256`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-256` |
| Judul | Verifikasi dan penyelarasan aksi pratinjau & cetak Gelang Ranap Resmi dan Checklist Serah Terima Pasien Baru di Workspace PPRI |
| Slice | Workspace PPRI — Identifikasi resmi gelang ranap dan pengesahan checklist serah terima 15 butir |
| Roadmap | [`frontend-roadmap-transfer-pasien-masuk.md`](../../../roadmap/frontend-roadmap-transfer-pasien-masuk.md) — kartu `FE-RWI-256` |
| Trace | `FR-TRF-006`; `TRF-AC-013`, `TRF-AC-014`; Kontrak 0.14.0 Transfer Handover Bagian 5.B; PRD Lampiran A.2; Keputusan `RWI-DEC-241` |
| Contract version | `0.14.0` |
| Dependency | `FE-RWI-254` ✅ |
| Klasifikasi | `HIGH` — Standar keselamatan identifikasi pasien (Gelang Pasien Ranap Resmi) dan kelengkapan akreditasi berkas admisi (Checklist 15 Butir) |
| Task mode | `FRONTEND` untuk source; wewenang lintas repository untuk dokumen laporan dan roadmap |
| Target tulis | `src/components/view/health-services/inpatient-management/admission-workspace/sections/identity-labels/wristband-print.jsx`, `src/utils/health-services/inpatient-management/inpatient-admission-print-utils.js`, `tests/unit/inpatient-admission-workspace.test.mjs` |
| Tanggal | 22 Oktober 2026 |
| Status | ✅ Selesai — Seluruh acceptance criteria terbukti, `node --test tests/unit/inpatient-admission-workspace.test.mjs` lulus 17/17, `npm run lint:errors` exit 0, `npm run build` lulus (exit 0) |

---

## 1. Keadaan yang Ditemukan di Awal

1. Pada Workspace PPRI sub-menu Gelang & Label (`IdentityLabelsSection`), pratinjau gelang ranap (`WristbandPrint`) sebelumnya belum menampilkan nomor Kamar dan Bed pasien serta catatan riwayat alergi secara eksplisit pada teks fisik gelang, meskipun data penempatan kamar sudah tersedia di backend.
2. Normalisasi data identitas cetak (`normalizeIdentityLabels`) belum mengekstrak atribut `roomBed` / `bedText` dan `allergyText` ke dalam struktur gelang.
3. Alur pengesahan tanda tangan pada Formulir Checklist Serah Terima Pasien Baru 15 Butir (`AdmissionDocumentSignatures`) memerlukan verifikasi ketat invariant bahwa slot tanda tangan Perawat Ruangan (`ReceivingNurse`) terkunci jika pasien belum berstatus menempati tempat tidur aktif (`isOccupyingBed === false`), dan hanya aktif saat pasien sudah berstatus `Occupied`.

> **Skenario Sebelum:** Petugas admisi mencetak gelang tanpa nomor kamar/bed dan riwayat alergi, serta belum ada tes unit spesifik yang menjamin bahwa perawat ruangan tidak dapat menandatangani berkas admisi sebelum pasien benar-benar tiba di tempat tidur bangsal.

> **Skenario Sesudah:** Gelang ranap resmi memuat No. RM, Nama Pasien, Tanggal Lahir / Usia, Kamar/Bed, catatan Alergi, dan Barcode/QR No. RM yang siap dicetak ke printer thermal standar (200mm x 25mm untuk dewasa, 160mm x 20mm untuk bayi/anak). Tombol tanda tangan perawat ruangan pada checklist serah terima 15 butir terkunci dengan aman sampai pasien menempati tempat tidur (`Occupied`).

---

## 2. Alur Proses Bisnis & Perilaku Pengguna

| Unsur | Penjelasan |
| --- | --- |
| **Tujuan** | Menegakkan standar identifikasi pasien rawat inap yang akurat dan memastikan serah terima berkas 15 butir disahkan sesuai tahap kehadiran fisik pasien di bangsal |
| **Pelaku** | Petugas Admisi PPRI, Petugas CRO, dan Perawat Ruangan Rawat Inap |
| **Pemicu** | Pembukaan tab Identitas & Label atau tab Berkas Admisi pada Workspace PPRI |

### Langkah dan Aturan Interaksi:

1. **Pratinjau & Cetak Gelang Ranap Resmi (`wristband-print.jsx`):**
   - Petugas PPRI membuka tab *Identitas & Label* -> sub-tab *Gelang Pasien*.
   - Layar menampilkan pratinjau fisik gelang pasien berukuran nyata:
     - Nama Lengkap Pasien & Jenis Kelamin.
     - Tanggal Lahir & Usia Pasien.
     - Nomor Rekam Medis (No. RM) & Lokasi Kamar/Bed.
     - Peringatan Alergi Pasien (bila ada).
     - QR Code / Barcode yang merepresentasikan No. RM pasien.
   - Mengklik `[Cetak Gelang]` memicu dialog cetak printer thermal dan mencatat riwayat cetakan ke log audit server.
2. **Formulir Serah Terima Pasien Baru 15 Butir (`handover-checklist-form.jsx`):**
   - Petugas PPRI memeriksa 15 butir berkas administrasi dan fasilitas rawat inap (Surat pengantar, KTP/KK, Kartu asuransi/SEP, General consent, Bukti cetak gelang, Surat selisih biaya, Bukti deposit, Edukasi, Hasil lab/radiologi, Rekam medis lama, Kartu penunggu, Inventaris barang, Informasi kepulangan, Buku panduan, Kesiapan bed).
   - Setiap butir wajib diverifikasi `Sudah` atau `Belum` (butir `Belum` wajib menyertakan keterangan).
   - Setelah 15 butir lengkap, dokumen dikunci (`AwaitingSignature`).
3. **Pengesahan Tanda Tangan 3 Pihak (`admission-document-signatures.jsx`):**
   - Slot 1: Petugas Admisi PPRI -> Tanda tangan elektronik langsung aktif.
   - Slot 2: Petugas CRO -> Tanda tangan elektronik langsung aktif.
   - Slot 3: Perawat Ruangan Rawat Inap (`ReceivingNurse`):
     - **Jika Pasien Belum di Bed (`isOccupyingBed === false`):** Tombol tanda tangan dinonaktifkan (`disabled`) dengan keterangan: *"Pasien belum menempati tempat tidur aktif di ruangan rawat inap."*
     - **Jika Pasien Sudah Occupied (`isOccupyingBed === true`):** Tombol tanda tangan aktif dan perawat ruangan dapat mengesahkan serah terima berkas.

---

## 3. Rincian Perubahan Kode

### 3.1 Berkas yang Diubah

1. [`src/components/view/health-services/inpatient-management/admission-workspace/sections/identity-labels/wristband-print.jsx`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/admission-workspace/sections/identity-labels/wristband-print.jsx)
   - Menambahkan rendering Kamar/Bed dan peringatan Alergi pada teks metadata gelang rawat inap resmi.
   - Menggunakan token CSS module murni tanpa hardcoded hex colors untuk mematuhi konstitusi Workspace PPRI.
2. [`src/utils/health-services/inpatient-management/inpatient-admission-print-utils.js`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/utils/health-services/inpatient-management/inpatient-admission-print-utils.js)
   - Memperluas fungsi `normalizeIdentityLabels` untuk mengekstrak properti `roomBed` dan `allergyText` ke dalam struktur `wristband`.
3. [`tests/unit/inpatient-admission-workspace.test.mjs`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/tests/unit/inpatient-admission-workspace.test.mjs)
   - Menambahkan test suite `FE-RWI-256` yang menguji normalisasi gelang ranap resmi (No. RM, nama, umur, bed/kamar, QR, alergi) dan guard tanda tangan perawat ruangan (`ReceivingNurse`) saat `!isOccupyingBed`.

---

## 4. Bukti Verifikasi dan Pengujian

### 4.1 Unit Testing (`node --test`)
```text
✔ FE-RWI-212: alamat Workspace PPRI dan tab Surat Persetujuan (1.7438ms)
✔ FE-RWI-212: status deposit header mengikuti hak ViewAmount (0.3292ms)
✔ FE-RWI-212: kop surat selalu dari profil rumah sakit server, tanpa logo bawaan (0.2702ms)
✔ FE-RWI-212: berkas Workspace PPRI tidak menanam identitas rumah sakit client (18.8612ms)
✔ FE-RWI-216: tombol hanya dari AvailableActions dan hak akses (0.4684ms)
✔ FE-RWI-216: payload Privasi — satu nama satu baris, tanpa identitas pasien (1.351ms)
✔ FE-RWI-219, 220: telepon maksimal 13 digit dan No. ID disamarkan (0.231ms)
✔ FE-RWI-218: butir Nilai Kepercayaan episode lalu menjadi konsep, maksimal lima (0.151ms)
✔ FE-RWI-220: perhitungan deposit dan batas jatuh tempo dari kebijakan (0.4868ms)
✔ FE-RWI-216: penanda versi cetak dan cetak ulang per status (0.2876ms)
✔ FE-RWI-212: kriteria 5, 6, 7 — batas keadaan, ketiadaan data pasien, dan kelengkapan (0.3788ms)
✔ FE-RWI-213: kriteria 1, 2, 3, 5, 6, 7 — klasifikasi gelang, QR payload, No. Kartu, dan Idempotency-Key (3.772ms)
✔ FE-RWI-214: kriteria 2, 3, 4 — hubungan calon penanda tangan dan tipe kamar (0.4305ms)
✔ FE-RWI-215: kriteria 1, 3, 4 — IPD BlankFields, tarif kamar, dan CanPrint (0.3623ms)
✔ FE-RWI-217: kriteria 1, 2, 7 — butir serah terima, saran, dan payload (0.2181ms)
✔ FE-RWI-215: printPdfBlob aman dipanggil dan menangani lingkungan non-browser tanpa galat (0.3944ms)
✔ FE-RWI-256: Verifikasi Cetak Gelang Ranap Resmi dan Checklist Serah Terima Pasien Baru PPRI (3.7523ms)
ℹ tests 17
ℹ suites 0
ℹ pass 17
ℹ fail 0
ℹ cancelled 0
ℹ skipped 0
ℹ todo 0
ℹ duration_ms 171.1052
```

### 4.2 Linter Audit (`npm run lint:errors`)
- Status: **Exit code 0 (Bersih tanpa error linter)**.

### 4.3 Kompilasi Produksi Standalone (`npm run build`)
- Status: **Exit code 0 (Kompilasi Turbopack standalone berhasil)**.

---

## 5. Matriks Pemenuhan Acceptance Criteria

| Kode AC | Kriteria Penerimaan | Status | Bukti Implementasi |
| --- | --- | :---: | --- |
| `TRF-AC-013` | Gelang ranap resmi memuat No RM, Nama Pasien, Tgl Lahir / Usia, Bed, Ruangan, dan Barcode/QR siap cetak | ✅ Terbukti | `wristband-print.jsx` & `normalizeIdentityLabels` di `inpatient-admission-print-utils.js` |
| `TRF-AC-014` | Tanda tangan perawat pada berkas serah terima PPRI aktif saat pasien menempati bed (`Occupied`) | ✅ Terbukti | Guard `config.name === "ReceivingNurse" && !isOccupyingBed` di `admission-document-signatures.jsx` |
| `TRF-AC-NEG-1` | Tanda tangan perawat ruangan terkunci jika pasien belum berstatus menempati bed | ✅ Terbukti | Tombol disabled dan menampilkan pesan peringatan `ADMISSION_MESSAGES.patientNotInBed` |
