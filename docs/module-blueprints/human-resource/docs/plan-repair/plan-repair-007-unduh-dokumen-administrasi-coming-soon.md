# PLAN-REPAIR-007 — Perbaikan Mekanisme Unduh Dokumen Registri Arsip Terpadu dari Tautan Frontend ke Unduhan Blob Terotentikasi

```yaml
plan_id: PLAN-REPAIR-HRD-007
issue: ../issue/issue-007-unduh-dokumen-administrasi-coming-soon.md
status_rencana: SELESAI
tanggal_rencana: "2026-11-16"
diputuskan_oleh: "Pemilik Sistem (User)"
tanggal_keputusan: "2026-11-16"
basis_source_backend: "80c94e8e (MHamzah)"
basis_source_frontend: "f5a0717bd (HamzahV2)"
```

---

## 1. Register Status Pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| :--- | :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| `FIX-HRD-007-01` | `ISS-HRD-007-01`, `ISS-HRD-007-T1` | Ganti tombol unduh `as="a"` relatif dengan handler klik unduhan Blob pada tabel dan modal detail | FE View | 1 | — | ✅ SELESAI | `QuilvianSystemFrontendDev/src/components/view/hr/workforce-core/administrative-documents-view.jsx` |
| `FIX-HRD-007-02` | `ISS-HRD-007-02` | Tambahkan metode `downloadIssuedDocument(id)` pada `hrService` via `InstanceAxios` berbasis Blob | FE Service | 1 | — | ✅ SELESAI | `QuilvianSystemFrontendDev/src/lib/services/hr/hr-service.js` |
| `FIX-HRD-007-03` | `ISS-HRD-007-03` | Kunci/nonaktifkan tombol unduh bila dokumen masih berstatus draf/menunggu otorisasi (`Pending`) | FE View | 2 | — | ✅ SELESAI | `QuilvianSystemFrontendDev/src/components/view/hr/workforce-core/administrative-documents-view.jsx` |

**Ringkasan: 3 dari 3 perbaikan selesai.**

---

## 2. Solusi Terpilih per Temuan

### ISS-HRD-007-01 & ISS-HRD-007-T1 — Tombol unduh menggunakan tag anchor relatif yang mengarah ke port Frontend (localhost:3000)

| Opsi | Cara | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A** | Tambahkan prefix URL backend (`https://localhost:7184`) langsung pada atribut `href` tag `<a>`. | Perubahan sangat sedikit pada JSX. | Gagal otentikasi karena cookie sesi terproteksi `[Authorize]` tidak terkirim via direct anchor lintas port, melanggar standar axios instance Quilvian. |
| **B** | Ganti tag `<a>` dengan tombol aksi klik (`onClick={handleDownload}`) yang memanggil service unduhan `blob`, lalu menyimpan berkas via utilitas `saveBlobAsFile`. | **(SOLUSI TERPILIH)** Terotentikasi penuh dengan token & cookie sesi, terproteksi CORS, nama berkas otomatis dari header atau nomor dokumen, visual indikator unduhan (loading spinner), dan menangani berkas SK maupun Surat Keterangan secara seragam. | Memerlukan fungsi handler unduhan dan state loading di komponen view. |

**Solusi terpilih: Opsi B.**
1. Memenuhi aturan otentikasi API Quilvian di mana seluruh akses endpoint terproteksi `[Authorize]` wajib melewati `InstanceAxios`.
2. Mencegah peramban membuka tab baru kosong atau terperangkap rute catch-all *Coming Soon* pada Next.js port 3000.
3. Menggunakan utilitas baku yang sudah teruji di modul workforce-core (`saveBlobAsFile`).

**Kenapa bukan opsi lain:** Opsi A tampak mudah namun pasti gagal saat diakses pengguna karena endpoint unduhan backend dilindungi otorisasi, serta melanggar tata kelola pemisahan frontend-backend.

---

### ISS-HRD-007-02 — Ketiadaan fungsi pengunduh berkas terotentikasi (*Blob Downloader*) pada berkas layanan HR

| Opsi | Cara | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A** | Menggunakan `fetch()` native peramban langsung di dalam komponen UI. | Tidak mengubah berkas service. | Duplikasi konfigurasi token/header, menyalahi arsitektur frontend Quilvian di mana akses data wajib lewat service layer. |
| **B** | Tambahkan metode `downloadIssuedDocument(id)` pada `hrService` (`hr-service.js`) menggunakan `InstanceAxios.get(..., { responseType: 'blob' })`. | **(SOLUSI TERPILIH)** Terpusat, konsisten dengan service lain seperti `PersonnelActionOrderService.downloadAttachment`, dan mudah dipelihara. | Harus memodifikasi berkas `src/lib/services/hr/hr-service.js`. |

**Solusi terpilih: Opsi B.**
1. Arsitektur modul persuratan SDM Quilvian memusatkan seluruh panggilan API katalog, permohonan, dan naskah dinas di `hr-service.js`.
2. Memungkinkan komponen menerima objek Axios utuh beserta header `content-disposition` untuk penamaan berkas otomatis.

**Kenapa bukan opsi lain:** Opsi A melanggar arsitektur service layer dan mempersulit pemeliharaan kode ke depan.

---

### ISS-HRD-007-03 — Dokumen berstatus 'Menunggu Otorisasi' menampilkan tombol unduh tanpa batas kesiapan naskah

| Opsi | Cara | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A** | Tetap izinkan unduh dokumen pending, namun beri watermark "DRAF / BELUM SAH". | Petugas dapat melihat draf cetak awal. | Backend saat ini mengembalikan fallback HTML statis sederhana, berisiko draf naskah disalahgunakan di luar RS. |
| **B** | Nonaktifkan tombol unduh (*disabled*) untuk dokumen yang belum berstatus sah (`Active`), dengan penjelasan tooltip *"Dokumen masih menunggu otorisasi pejabat berwenang"*. | **(SOLUSI TERPILIH)** Menjamin integritas hukum dan kepatuhan RS bahwa naskah dinas resmi hanya boleh diunduh setelah terbit sah dan bertandatangan digital. | Petugas tidak dapat mengunduh draf jika sekadar ingin pratinjau (pratinjau data cukup via modal Rincian). |

**Solusi terpilih: Opsi B (menunggu penegasan pemilik via K-01).**
1. Mencegah peredaran surat keterangan resmi rumah sakit bodong atau belum sah.
2. Sesuai dengan prinsip tata kelola naskah dinas bahwa nomor seri dokumen dan QR Code verifikasi baru sah setelah statusnya `Active`.

**Kenapa bukan opsi lain:** Opsi A menuntut perubahan generator PDF di backend untuk menambahkan watermark dinamis pada fase pra-otorisasi.

---

## 3. Skema Tampilan Sebelum → Sesudah

### Sebelum (Kondisi Bermasalah)

```text
+--------------------------------------------------------------------------------------------------------------------------+
| Human Resource — Administrasi Kepegawaian                                                                                |
| Dokumen Administrasi & Registri Arsip Terpadu                                                                           |
| Pusat pencarian terpadu naskah dinas kepegawaian (SK, Surat Tugas, Surat Keterangan Resmi), verifikasi keabsahan...      |
+--------------------------------------------------------------------------------------------------------------------------+
| TOTAL ARSIP: 7        | BERLAKU SAH AKTIF: 1  | DALAM PROSES: 1       | DICABUT / DIBATALKAN: 5                          |
+--------------------------------------------------------------------------------------------------------------------------+
| [ Semua Jenis Dokumen ▾ ] [ Semua Status Legal ▾ ] [ Cari nomor dokumen, nama pegawai... ] [ 25 baris ▾ ] [ Refresh ]  |
+--------------------------------------------------------------------------------------------------------------------------+
| NOMOR ARSIP DOKUMEN       | PEMILIK / PEGAWAI | URAIAN & MAKSUD DOKUMEN | TANGGAL     | STATUS LEGAL   | AKSI            |
+---------------------------+-------------------+-------------------------+-------------+----------------+-----------------+
| ISS-DOC-20261116-006882   | Andini Larasati   | Surat Keterangan Bekerja| 16 Nov 2026 | [Menunggu Otor]| [ Rincian ]     |
| Surat Keterangan Dinas    | WFP-RSMMC-00049   | Aktif                   |             |                | [ Unduh ] --->  | Mengarah ke
+---------------------------+-------------------+-------------------------+-------------+----------------+-------|---------+ port 3000
                                                                                                                 v
                                                                                           +-------------------------------+
                                                                                           | COMING SOON!                  |
                                                                                           | Kami sedang membangun...      |
                                                                                           +-------------------------------+
```

### Sesudah (Kondisi Direncanakan)

```text
+--------------------------------------------------------------------------------------------------------------------------+
| Human Resource — Administrasi Kepegawaian                                                                                |
| Dokumen Administrasi & Registri Arsip Terpadu                                                                           |
| Pusat pencarian terpadu naskah dinas kepegawaian (SK, Surat Tugas, Surat Keterangan Resmi), verifikasi keabsahan...      |
+--------------------------------------------------------------------------------------------------------------------------+
| TOTAL ARSIP: 7        | BERLAKU SAH AKTIF: 1  | DALAM PROSES: 1       | DICABUT / DIBATALKAN: 5                          |
+--------------------------------------------------------------------------------------------------------------------------+
| [ Semua Jenis Dokumen ▾ ] [ Semua Status Legal ▾ ] [ Cari nomor dokumen, nama pegawai... ] [ 25 baris ▾ ] [ Refresh ]  |
+--------------------------------------------------------------------------------------------------------------------------+
| NOMOR ARSIP DOKUMEN       | PEMILIK / PEGAWAI | URAIAN & MAKSUD DOKUMEN | TANGGAL     | STATUS LEGAL   | AKSI            |
+---------------------------+-------------------+-------------------------+-------------+----------------+-----------------+
| ISS-DOC-20261116-006882   | Andini Larasati   | Surat Keterangan Bekerja| 16 Nov 2026 | [Menunggu Otor]| [ Rincian ]     |
| Surat Keterangan Dinas    | WFP-RSMMC-00049   | Aktif                   |             |                | (× Unduh) ⚠     | Tombol disabled
+---------------------------+-------------------+-------------------------+-------------+----------------+-----------------+ dengan tooltip
| ISS-DOC-20261116-006899   | Budi Santoso      | Surat Keterangan Kerja  | 16 Nov 2026 | [Terbit Sah]   | [ Rincian ]     |
| Surat Keterangan Dinas    | WFP-RSMMC-00012   | Operasional             |             |                | [ Unduh ] --->  | Memicu unduh
+---------------------------+-------------------+-------------------------+-------------+----------------+-------|---------+ berkas PDF
                                                                                                                 v
                                                                                           [ Unduhan File: ISS-DOC-20261116-006899.pdf ]
```

### Tabel Wilayah, Tombol, dan Keadaan

#### Tabel Tombol Aksi Registri
| Tombol | Jenis | Kapan Aktif | Yang Terjadi |
| :--- | :---: | :--- | :--- |
| **Rincian** | Secondary | Selalu aktif pada setiap baris | Membuka modal dialog rincian dokumen lengkap beserta atribut legal dan audit trail. |
| **Unduh** (Tabel) | Secondary | Aktif bila dokumen memiliki berkas unduhan dan status `Active` (Sah) | Mengubah ikon tombol menjadi spinner loading, memanggil API unduhan blob via `InstanceAxios`, lalu mengunduh berkas fisik PDF di browser tanpa berpindah halaman. |
| **Unduh** (Tabel - Pending) | Secondary (Disabled) | Nonaktif jika dokumen masih berstatus `Pending` (*Menunggu Otorisasi*) | Menampilkan tooltip keterangan: *"Naskah dinas belum disahkan (hanya naskah terbit sah yang dapat diunduh)"*. |
| **Unduh Dokumen Asli** (Modal) | Primary | Aktif di footer modal jika dokumen berstatus sah | Mengunduh berkas fisik dokumen asli langsung dari modal rincian. |

#### Tabel Keadaan Komponen
| Keadaan | Tampilan |
| :--- | :--- |
| **Mengunduh Berkas** | Tombol menampilkan spinner kecil (*"Mengunduh..."*) dan dinonaktifkan sementara untuk mencegah klik ganda. |
| **Unduhan Berhasil** | Berkas PDF otomatis tersimpan di peramban pengguna; tombol kembali normal. |
| **Unduhan Gagal** | Muncul pesan error InformationAlert di atas tabel: *"Gagal mengunduh berkas naskah dinas: [Pesan Error]"*. |

---

## 4. Rincian Perbaikan

### FIX-HRD-007-02 — Tambahkan Metode Unduhan Naskah Dinas Terotentikasi pada hr-service.js

| | |
| :--- | :--- |
| **Menutup** | `ISS-HRD-007-02` |
| **Area** | Frontend Service |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `QuilvianSystemFrontendDev/src/lib/services/hr/hr-service.js` |
| **Radius dampak** | Layanan persuratan dinas (`hrService`) |
| **Bergantung pada** | Tidak ada |

**Langkah.**
1. Buka berkas `QuilvianSystemFrontendDev/src/lib/services/hr/hr-service.js`.
2. Tambahkan metode `downloadIssuedDocument` pada objek `hrService`:
   ```javascript
   async downloadIssuedDocument(id) {
     const res = await InstanceAxios.get(`${DOCUMENTS_URL}/${id}/download`, {
       responseType: "blob",
     });
     return res;
   },
   ```
3. Simpan dan pastikan ekspor service tetap utuh.

**Acceptance criteria.**
1. Pemanggilan `hrService.downloadIssuedDocument(id)` melakukan request `GET` ke endpoint `/v1/corporate/hrd/services/documents/{id}/download` dengan header otentikasi login aktif.
2. Respons yang diterima berupa respons Axios dengan payload `data` berupa tipe `Blob` (PDF binary).

**Verifikasi.**
- Jalankan pemeriksaan linter: `npx eslint --config eslint.config.mjs src/lib/services/hr/hr-service.js`.

**Risiko.**
- Sangat rendah; metode baru ini hanya menambah kapabilitas tanpa mengubah fungsi-fungsi yang telah berjalan.

---

### FIX-HRD-007-01 — Ganti Tag Anchor Relatif dengan Handler Klik Unduhan Blob

| | |
| :--- | :--- |
| **Menutup** | `ISS-HRD-007-01`, `ISS-HRD-007-T1` |
| **Area** | Frontend View |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `QuilvianSystemFrontendDev/src/components/view/hr/workforce-core/administrative-documents-view.jsx` |
| **Radius dampak** | Menu Registri Dokumen Administrasi Kepegawaian (`/hr/workforce-core/administrative-documents`) |
| **Bergantung pada** | `FIX-HRD-007-02` |

**Langkah.**
1. Di `administrative-documents-view.jsx`, import `saveBlobAsFile` dari `@/utils/hr/workforce-core/workforce-core-display-utils` dan import `PersonnelActionOrderService` dari `@/lib/services/hr/personnel-action-order-service`.
2. Tambahkan state `downloadingId` (`useState(null)`) dan `downloadError` (`useState("")`) untuk memantau progress unduhan per baris.
3. Buat fungsi helper `handleDownloadDocument(doc)`:
   - Jika `doc.docCategory === "KETERANGAN"`: panggil `hrService.downloadIssuedDocument(doc.id)`. Ekstrak nama berkas dari header `content-disposition` jika ada, atau fallback `${doc.docNumber || "naskah-dinas"}.pdf`. Panggil `saveBlobAsFile(res.data, fileName)`.
   - Jika `doc.docCategory === "SK"` dan memiliki lampiran: panggil `PersonnelActionOrderService.downloadAttachment(doc.originalData.id, doc.originalData.attachmentId)`. Panggil `saveBlobAsFile(blob, doc.originalData.attachmentFileName || "lampiran-sk.pdf")`.
   - Tangani error dengan menampilkan pesan alert informatif jika gagal.
4. Ganti elemen `<BaseButton as="a" href={row.downloadUrl} target="_blank">` di kolom aksi tabel dan footer modal rincian menjadi tombol aksi biasa:
   ```jsx
   <BaseButton
     size="sm"
     variant="secondary"
     disabled={isDownloading || isDownloadDisabled}
     onClick={() => handleDownloadDocument(row)}
     title={downloadTooltip}
   >
     {isDownloading ? (
       <Spinner size="sm" className="me-1" />
     ) : (
       <RiDownload2Line className="me-1" />
     )}
     {isDownloading ? "Mengunduh..." : "Unduh"}
   </BaseButton>
   ```

**Acceptance criteria.**
1. Mengklik tombol "Unduh" tidak lagi membuka tab baru ke port 3000 atau menampilkan layar "COMING SOON!".
2. Berkas PDF surat naskah dinas resmi terunduh langsung ke folder Download komputer pengguna dengan nama berkas yang informatif.
3. Saat unduhan berlangsung, tombol berubah menampilkan status indikator unduh (*"Mengunduh..."*) dan mencegah klik berulang.

**Verifikasi.**
- Jalankan linter: `npx eslint --config eslint.config.mjs src/components/view/hr/workforce-core/administrative-documents-view.jsx`.
- Uji klik tombol unduh pada baris dokumen di browser.

**Risiko.**
- Berkas unduhan berukuran besar memerlukan beberapa detik; diatasi dengan visual loading spinner pada tombol.

---

### FIX-HRD-007-03 — Kunci Tombol Unduh untuk Dokumen Berstatus Menunggu Otorisasi

| | |
| :--- | :--- |
| **Menutup** | `ISS-HRD-007-03` |
| **Area** | Frontend View |
| **Jenis perubahan** | Logika tampilan & UX |
| **Berkas yang diubah** | `QuilvianSystemFrontendDev/src/components/view/hr/workforce-core/administrative-documents-view.jsx` |
| **Radius dampak** | Tampilan status dokumen di tabel registri |
| **Bergantung pada** | Keputusan Pemilik `K-01`, `FIX-HRD-007-01` |

**Langkah.**
1. Pada pemetaan kolom aksi tabel dan modal detail, periksa status dokumen:
   - Jika `row.status !== "Active"` (misal masih `Pending` / *Menunggu Otorisasi*):
     - Berikan atribut `disabled={true}` pada tombol Unduh.
     - Berikan atribut `title="Naskah dinas masih dalam proses otorisasi dan belum sah diterbitkan."` agar saat kursor diarahkan ke tombol, pengguna memahami alasannya.
2. Jika dokumen berstatus `Active`: tombol unduh aktif normal.

**Acceptance criteria.**
1. Pada dokumen `ISS-DOC-20261116-006882` (berstatus *Menunggu Otorisasi*), tombol Unduh tampak nonaktif/disabled dan tidak dapat diklik.
2. Muncul tooltip penjelasan saat tombol didekati.

**Verifikasi.**
- Buka registri arsip dan periksa baris naskah dinas berstatus *Menunggu Otorisasi*.

**Risiko.**
- Rendah.

---

## 5. Urutan Pengerjaan

```text
FIX-HRD-007-02 (Tambahkan downloadIssuedDocument di hr-service.js)
└── FIX-HRD-007-01 (Ganti tombol as="a" dengan handler blob di administrative-documents-view.jsx)
    └── K-01 (Persetujuan Pemilik Sistem atas Kebijakan Unduh Naskah Draf)
        └── FIX-HRD-007-03 (Kunci tombol unduh untuk naskah Pending)
```

---

## 6. Keputusan Pemilik yang Dibutuhkan

| No | Keputusan | Pilihan | Rekomendasi | Menahan Perbaikan |
| :---: | :--- | :--- | :--- | :---: |
| **K-01** | Apakah naskah dinas kepegawaian yang masih berstatus **Menunggu Otorisasi** (*Pending*) boleh diunduh oleh staf admin? | **A.** Boleh, diunduh sebagai berkas draf pratinjau.<br>**B.** Tidak boleh, tombol Unduh dinonaktifkan (*disabled*) hingga naskah dinas resmi disahkan (*Active*). | **Pilihan B**: Menjamin keabsahan hukum persuratan rumah sakit agar naskah tanpa pengesahan resmi tidak bocor keluar. | `FIX-HRD-007-03` |

---

## 7. Dokumen Hulu yang Ikut Direvisi

Tidak ada perubahan model data backend maupun perubahan kontrak endpoint. Dokumen skema tampilan blueprint akan dicatat selaras dengan perbaikan handler unduhan ini.

---

## 8. Verifikasi Menyeluruh

1. **Uji Frontend Linting**:
   ```powershell
   npm run lint:errors
   ```
2. **Uji Alur Unduh Naskah Dinas**:
   - Masuk ke menu `Dokumen Administrasi & Registri Arsip Terpadu`.
   - Pastikan baris dokumen berstatus *Menunggu Otorisasi* memiliki tombol Unduh nonaktif dengan tooltip penjelasan.
   - Klik tombol **Unduh** pada dokumen berstatus *Terbit Sah* (Active).
   - Pastikan berkas fisik `.pdf` terunduh sempurna ke sistem operasi lokal dan tidak ada halaman "COMING SOON!" yang terbuka.
   - Buka modal **Rincian**, uji tombol **Unduh Dokumen Asli** di dalam modal dan pastikan berkas terunduh dengan baik.

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| :---: | :--- | :---: |
| 2026-11-16 | Rencana perbaikan dibuat dengan 3 butir perbaikan (FIX-01 s/d FIX-03). Status: MENUNGGU_PERSETUJUAN. | `diagnose-module-issue` |
| 2026-11-16 | Rencana disetujui penuh oleh Pemilik Sistem. Keputusan K-01 dipilih Opsi B (kunci unduh draf pending). | Pemilik Sistem (User) |
| 2026-11-16 | Implementasi selesai untuk FIX-01, FIX-02, dan FIX-03. Verifikasi ESLint lolos tanpa galat. Status: SELESAI. | `diagnose-module-issue` |
