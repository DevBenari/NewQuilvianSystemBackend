# ISSUE-007 — Tombol Unduh Dokumen Administrasi Mengarah ke Layar Coming Soon Frontend

```yaml
issue_id: ISSUE-HRD-007
module_id: human-resource
submodule: null   # blueprint berbentuk SINGLE (HRD-DEC-003); blueprint root = human-resource/
layar: "Administrasi Kepegawaian — Dokumen Administrasi & Registri Arsip Terpadu (/hr/workforce-core/administrative-documents)"
sumber_laporan: "Laporan pengguna 16-11-2026: Keluhan tombol unduh memunculkan halaman Coming Soon + 2 screenshot layar registri dan tampilan Coming Soon"
tanggal_issue: "2026-11-16"
status: SELESAI
keparahan_tertinggi: High
source_sha_backend: "80c94e8e (MHamzah)"
source_sha_frontend: "f5a0717bd (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-007-unduh-dokumen-administrasi-coming-soon.md
```

---

## 1. Ringkasan

Pengguna membuka menu **Dokumen Administrasi & Registri Arsip Terpadu** (`/hr/workforce-core/administrative-documents`), kemudian menekan tombol **Unduh** pada salah satu baris naskah dinas resmi kepegawaian (misalnya dokumen `ISS-DOC-20261116-006882` — *Surat Keterangan Bekerja Aktif*). Peramban membuka tab baru ke alamat `http://localhost:3000/api/v1/corporate/hrd/services/documents/edc5cefc-093f-49fc-b320-861faf6c8d7f/download` dan menampilkan layar **"COMING SOON! Kami sedang membangun halaman ini. Nantikan kehadirannya!"**.

Akar masalahnya adalah kesalahan mekanisme unduhan pada komponen frontend: tautan tombol menggunakan elemen tautan biasa `<a href="/api/v1/...">` dengan alamat relatif tanpa domain backend. Alih-alih menghubungi server backend ASP.NET Core (`https://localhost:7184`), peramban meminta rute tersebut ke server frontend Next.js (`http://localhost:3000`). Karena Next.js tidak memiliki rute halaman untuk URL tersebut, sistem menangkapnya sebagai rute tak dikenal (*catch-all route*) dan menyajikan halaman cadangan *Coming Soon*.

Selain itu, navigasi tautan peramban biasa (`<a href="..." target="_blank">`) tidak menyertakan kredensial otorisasi API terproteksi, sehingga unduhan berkas wajib dialihkan menggunakan fungsi unduhan berbasis `InstanceAxios` dengan penanganan berkas *blob* yang aman. Masalah ini berkategori **High Severity** karena dokumen arsip resmi rumah sakit tidak dapat diunduh oleh pengguna.

---

## 2. Laporan Asli

| No. laporan | Keluhan (kata-kata pelapor, diringkas) | Lampiran |
| :---: | :--- | :--- |
| 1 | "Ketika saya melakukan klik unduh pada menu Dokumen Administrasi & Registri Arsip Terpadu terjadi coming soon seperti pada gambar yang saya berikan" | Tangkapan layar 1: Tab peramban `http://localhost:3000/api/v1/corporate/hrd/services/documents/edc5cefc-093f-49fc-b320-861faf6c8d7f/download` menampilkan halaman besar berlatar hijau toska bertuliskan "COMING SOON!". |
| 2 | "Coba anda lakukan /diagnose-module-issue untuk mengatasi masalah tersebut" | Tangkapan layar 2: Menu `Dokumen Administrasi & Registri Arsip Terpadu` memperlihatkan baris data `ISS-DOC-20261116-006882` dengan tombol aksi `Rincian` dan `Unduh` pada tab *Network* DevTools. |

---

## 3. Ringkasan Temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| :--- | :---: | :--- | :---: | :---: | :---: | :---: | :--- |
| `ISS-HRD-007-01` | 1 | Tombol unduh menggunakan tag anchor relatif yang mengarah ke port Frontend (localhost:3000) | `BUG` | Frontend View | High | SUDAH-VERIFIKASI | `FIX-HRD-007-01` |
| `ISS-HRD-007-02` | 1 | Ketiadaan fungsi pengunduh berkas terotentikasi (*Blob Downloader*) pada berkas layanan HR | `BUG` | Frontend Service | High | SUDAH-VERIFIKASI | `FIX-HRD-007-02` |
| `ISS-HRD-007-03` | 2 | Dokumen berstatus 'Menunggu Otorisasi' menampilkan tombol unduh tanpa indikator draf/kesiapan naskah | `DESIGN_CHANGE` | Frontend View | Medium | SUDAH-VERIFIKASI | `FIX-HRD-007-03` |

---

## 4. Rincian per Temuan

### ISS-HRD-007-01 — Tombol unduh menggunakan tag anchor relatif yang mengarah ke port Frontend (localhost:3000)

| | |
| :--- | :--- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend View |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di `QuilvianSystemFrontendDev/src/components/view/hr/workforce-core/administrative-documents-view.jsx:166, 303-314, 501-510` |
| **Perbaikan** | `FIX-HRD-007-01` |

**Apa yang terjadi.** Ketika pengguna menekan tombol **Unduh** pada tabel registri atau tombol **Unduh Dokumen Asli** pada jendela rincian arsip, peramban membuka tab baru ke alamat `http://localhost:3000/api/v1/corporate/hrd/services/documents/.../download`. Halaman yang muncul bukan berkas PDF surat dinas, melainkan halaman bertuliskan *"COMING SOON! Kami sedang membangun halaman ini. Nantikan kehadirannya!"*.

**Kenapa terjadi.** Pada berkas `administrative-documents-view.jsx`:
```jsx
// Baris 166:
downloadUrl: `/api/v1/corporate/hrd/services/documents/${doc.id}/download`,

// Baris 302-314:
{row.downloadUrl ? (
  <BaseButton
    as="a"
    size="sm"
    variant="secondary"
    href={row.downloadUrl}
    target="_blank"
    rel="noreferrer"
    title="Unduh Salinan Dokumen"
  >
    <RiDownload2Line className="me-1" /> Unduh
  </BaseButton>
) : null}
```
Properti `as="a"` merender komponen `BaseButton` sebagai elemen HTML `<a>` biasa. Nilai `href` yang diisi adalah path relatif berawalan `/api/v1/...`. Ketika diklik oleh pengguna, peramban otomatis melengkapi URL tersebut dengan origin tab yang sedang aktif, yaitu `http://localhost:3000` (port frontend Next.js), bukan `https://localhost:7184` (port backend ASP.NET Core). 

Karena Next.js App Router tidak memiliki halaman atau route handler pada jalur `/api/v1/...`, Next.js mencocokkannya ke halaman *catch-all* cadangan (`[...catchAll]/page.jsx`) yang menampilkan tulisan "COMING SOON!".

**Apakah ini menyimpang dari desain?** `BUG`. Tombol unduh dokumen dinas resmi harus mengunduh berkas fisik asli (PDF) dari server backend, bukan membuka halaman web kosong frontend.

**Dampak nyata.** Petugas administrasi rumah sakit dan pegawai tidak dapat mengunduh surat naskah dinas resmi yang telah tersimpan di sistem.

**Rekomendasi.** Ganti navigasi `<a>` dengan tombol klik biasa (`onClick={handleDownload}`) yang memanggil fungsi service pengunduh berkas terotentikasi, lalu memicu unduhan berkas secara dinamis ke peramban pengguna.

---

### ISS-HRD-007-02 — Ketiadaan fungsi pengunduh berkas terotentikasi (*Blob Downloader*) pada berkas layanan HR

| | |
| :--- | :--- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend Service |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI di `QuilvianSystemFrontendDev/src/lib/services/hr/hr-service.js` |
| **Perbaikan** | `FIX-HRD-007-02` |

**Apa yang terjadi.** Berkas `hr-service.js` belum menyediakan fungsi pembungkus (*wrapper function*) untuk mengunduh naskah dinas kepegawaian melalui Axios instance. 

**Kenapa terjadi.** Endpoint unduhan backend di `EmployeeDocumentIssuanceController.cs`:
```csharp
[HttpGet("{id:guid}/download")]
[Authorize]
public async Task<IActionResult> DownloadIssuedDocument(Guid id, CancellationToken cancellationToken = default)
```
dilindungi oleh atribut `[Authorize]`. Mengakses URL backend secara langsung lewat tautan peramban baru (*direct URL in new tab*) berisiko gagal otentikasi karena cookie sesi peramban antar port (`3000` vs `7184`) dapat diblokir oleh kebijakan SameSite peramban modern.

**Apakah ini menyimpang dari desain?** `BUG`. Seluruh pertukaran data API terproteksi wajib melalui `InstanceAxios` yang telah dikonfigurasi dengan `withCredentials: true` dan penanganan token terpusat.

**Dampak nyata.** Ketiadaan helper unduhan membuat antarmuka berspekulasi menggunakan tag anchor manual yang salah sasaran.

**Rekomendasi.** Tambahkan metode `downloadIssuedDocument(id)` pada `hrService` yang memanggil `InstanceAxios.get(...)` dengan opsi `{ responseType: 'blob' }`, mengekstrak nama berkas dari header `Content-Disposition`, dan memicu unduhan berkas melalui objek Blob URL di browser.

---

### ISS-HRD-007-03 — Dokumen berstatus 'Menunggu Otorisasi' menampilkan tombol unduh tanpa indikator draf/kesiapan naskah

| | |
| :--- | :--- |
| **No. laporan** | 2 |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend View |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di `administrative-documents-view.jsx:153-166` dan tangkapan layar pelapor |
| **Perbaikan** | `FIX-HRD-007-03` |

**Apa yang terjadi.** Pada tangkapan layar pelapor, dokumen `ISS-DOC-20261116-006882` berstatus **Menunggu Otorisasi** (*Pending*), namun tombol **Unduh** sudah aktif dan dapat diklik. 

**Kenapa terjadi.** Logika pembentukan data registri pada `administrative-documents-view.jsx` selalu mengisi `downloadUrl` untuk seluruh dokumen surat keterangan tanpa memeriksa apakah status dokumen tersebut sudah sah (*Active/Authorized*) atau masih draf dalam antrean (*Pending*). 

Di sisi backend (`EmployeeDocumentIssuanceService.cs:598-608`), jika berkas PDF resmi belum selesai diotorisasi, backend mengembalikan fallback berupa dokumen draf HTML (`Naskah Dinas Belum Siap`). Hal ini dapat menimbulkan salah paham bagi petugas yang mengira naskah tersebut sudah sah ditandatangani.

**Apakah ini menyimpang dari desain?** `DESIGN_CHANGE`. Dokumen dinas resmi rumah sakit hanya boleh diunduh sebagai naskah sah setelah melewati tahap otorisasi digital (*Active*). Untuk dokumen yang masih berstatus antrean (*Pending*), tombol unduh sebaiknya dinonaktifkan (*disabled*) dengan tooltip: *"Dokumen masih menunggu otorisasi digital pejabat berwenang"*, atau mengizinkan unduhan pratinjau draf dengan penanda yang jelas.

**Dampak nyata.** Petugas berpotensi mencetak atau mengunduh draf naskah yang belum disahkan oleh direktur atau manajer SDM.

**Rekomendasi.** Atur ketersediaan tombol unduh pada registri agar aktif hanya untuk dokumen yang berstatus sah (`Active` / `Authorized`), atau tampilkan konfirmasi pratinjau jika dokumen masih dalam status `Pending`.

---

## 5. Tanya-Jawab Pelapor

> **T:** Mengapa ketika saya menekan tombol "Unduh" di menu Dokumen Administrasi muncul halaman bertuliskan "COMING SOON!"?
>
> **J:** Karena tombol unduh pada antarmuka saat ini dibuat sebagai tautan web biasa yang mengarah ke port aplikasi web antarmuka Anda (`http://localhost:3000`), bukan ke server backend pengelola dokumen (`https://localhost:7184`). Karena port 3000 tidak memiliki halaman web untuk jalur tersebut, sistem otomatis menampilkan layar bawaan *"COMING SOON!"*. Tombol ini perlu diperbaiki agar memanggil API pengunduh berkas fisik PDF langsung ke server backend dengan otentikasi login yang sah.

---

## 6. Temuan Tambahan

### ISS-HRD-007-T1 — Unduhan lampiran SK juga menggunakan jalur relatif tanpa prefix API backend

| | |
| :--- | :--- |
| **Kenapa dicantumkan** | Masalah yang sama persis terjadi pada dokumen Surat Keputusan (SK) |
| **Area** | Frontend View |
| **Keparahan** | High |
| **Status bukti** | `SUDAH-VERIFIKASI` di `administrative-documents-view.jsx:104` |
| **Perbaikan** | `FIX-HRD-007-01` |

Pada baris 104 `administrative-documents-view.jsx`:
```javascript
downloadUrl: sk.hasAttachment
  ? `/api/v1/corporate/human-resource/personnel-action-orders/${sk.id}/attachments/${sk.attachmentId}`
  : null,
```
Path ini juga merupakan path relatif yang jika diklik akan membuka `http://localhost:3000` dan memicu halaman *Coming Soon*. Penanganan unduhan harus mencakup berkas lampiran SK secara seragam.

---

## 7. Pertanyaan dan Keputusan yang Dibutuhkan

### Untuk Pelapor
Tidak ada pertanyaan penahan; tangkapan layar peramban dan penelusuran kode telah membuktikan akar masalah 100%.

### Untuk Pemilik Sistem

| No | Keputusan | Pilihan | Rekomendasi | Menahan Perbaikan |
| :---: | :--- | :--- | :--- | :---: |
| K-01 | Apakah dokumen yang masih berstatus *Menunggu Otorisasi* (Pending) boleh diunduh oleh staf admin? | **A.** Boleh, diunduh sebagai salinan draf pratinjau (*Draft Preview*).<br>**B.** Tidak boleh, tombol Unduh hanya aktif jika dokumen berstatus sah (*Active/Terbit Sah*). | **Pilihan B**: Menjaga integritas hukum persuratan rumah sakit agar draf naskah yang belum disahkan tidak beredar di luar. | `FIX-HRD-007-03` |

---

## 8. Catatan Pola

Anti-pattern: Menggunakan elemen `<BaseButton as="a" href="/api/v1/..." target="_blank">` untuk mengunduh dokumen dari backend.
Pencegahan:
1. Setiap aksi unduh berkas pada aplikasi Quilvian wajib menggunakan fungsi helper unduhan terpusat (`downloadBlobFile`) yang memanfaatkan `InstanceAxios`.
2. Dilarang memasang path API relatif pada atribut `href` elemen `<a>` di sisi frontend.

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :---: | :--- | :---: |
| 2026-11-16 | Dokumen issue dibuat dari laporan tombol unduh memunculkan layar Coming Soon pada menu Dokumen Administrasi. Status: TERBUKA. | `diagnose-module-issue` |
| 2026-11-16 | Rencana perbaikan disetujui dan seluruh perbaikan (FIX-01, FIX-02, FIX-03) telah selesai diimplementasikan. Status: SELESAI. | `diagnose-module-issue` |
