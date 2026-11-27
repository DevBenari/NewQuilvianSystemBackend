# MASTER INSTRUKSI AGENT — MULTI-STEP PENDAFTARAN PELAMAR QUILVIAN V2

| Informasi | Ketentuan |
|---|---|
| Tanggal baseline | 25 November 2026 |
| Modul | Sumber Daya Manusia (HRD) |
| Kelompok | Siklus Kepegawaian → Rekrutmen & Seleksi |
| Submenu target | Pelamar (Menu 03 dalam register kebutuhan bisnis lama) |
| Slice canonical existing | `S-D2` — Rekrutmen & Seleksi |
| Jenis | Instruksi implementasi langsung, bukan laporan hasil implementasi |
| Mandat | Backend readiness dan perbaikan terarah terlebih dahulu, frontend wizard, integrasi, kemudian UAT live browser |
| Status keputusan | Planning lima langkah disetujui pengguna pada percakapan 25 November 2026 |
| Status kode awal | `PARTIAL / IMPLEMENTED_NOT_VERIFIED`: kemampuan dasar ada, wizard belum tersedia; belum diuji live pada snapshot ini |

**PERINTAH UTAMA:** Implementasikan fitur Multi-Step Pendaftaran Pelamar yang benar-benar berfungsi pada workspace Quilvian V2. Jangan berhenti setelah audit, planning, mockup, atau membuat dokumen. Pertahankan menu, data, kontrak bisnis, desain existing, dan permission yang sudah berlaku. Gunakan kembali backend dan base component sejauh dapat memenuhi requirement; jangan menambah tabel atau migration tanpa keputusan dan izin terpisah.

## 1. Role

Anda adalah Senior Fullstack Engineer Quilvian V2 (ASP.NET Core/EF Core dan Next.js), Business Process Analyst HR rumah sakit, UI/UX Engineer, dan QA/UAT Engineer. Pekerjaan Anda adalah menyatukan registrasi kandidat, pemilihan lowongan, kelengkapan dokumen, dan pengiriman lamaran menjadi satu pengalaman bertahap, persisten, aman, dan mudah dipahami oleh petugas HRD.

### 1.1 Repository dan baseline sumber

| Peran | Repository | Branch wajib | HEAD yang diperiksa |
|---|---|---|---|
| Frontend | `DevBenari/QuilvianSystemFrontendDev` | `HamzahV2` | `f122c1d641f2cf47e3597762e6956fabdae37eaa` |
| Backend | `DevBenari/NewQuilvianSystemBackend` | `MHamzah` | `6945b70d0f5ff35520f1789b7d5e7acbd3ecae48` |
| Governance | `DevBenari/QuilvianEngineeringSkills` | `main` | `62b7ead4595161ec8f9bb7fc2dd96b3a7b29f2a5` |

Baseline FE dan BE sama dengan pembacaan pada percakapan sebelumnya (commit 25 November 2026, 15:53:12 dan 15:53:40 WIB). HEAD adalah referensi evidence, bukan instruksi checkout/reset. Periksa branch dan SHA lokal/remote yang dapat diakses sebelum bekerja; jika berubah, lakukan impact review terhadap feature terkait dan gunakan source terbaru pada branch yang benar. Jangan mengubah pekerjaan developer lain.

### 1.2 Wewenang dan mode

- `MODULE BLUEPRINT MODE`: jika diperlukan dan diizinkan governance, sinkronkan keputusan yang sudah disetujui pada blueprint HRD dan task terkait saja; source read-only pada fase ini.
- `BACKEND MODE`: source lokal BE `MHamzah` hanya pada kontrak dan layanan registrasi, lamaran, dokumen, lookup lowongan serta validasi integrasi terkait. FE dan Engineering Skills read-only. Jika seluruh API existing memenuhi acceptance, laporkan reuse dan hasil test; jangan memaksakan perubahan BE.
- `FRONTEND MODE`: source lokal FE `HamzahV2` hanya pada route/view/hook/service/constants/style/test yang relevan dengan submenu Pelamar dan integrasinya. BE dan Engineering Skills read-only.
- Validasi terkoordinasi FE–BE setelah kedua fase, tanpa perubahan lintas domain. Gunakan `CROSS-REPO MODE` hanya apabila diperlukan dan diizinkan sesuai aturan repository.
- Dilarang otomatis melakukan commit, push, pull, merge, rebase, reset/destructive Git, deploy, membuat/menjalankan migration, atau menjalankan SQL/database langsung. Masing-masing membutuhkan otorisasi terpisah sesuai `AGENTS.md`/`CLAUDE.md`. UAT mutasi melalui aplikasi/API hanya pada environment pengujian yang memang diizinkan, bukan produksi.
- Repo Engineering Skills adalah referensi read-only. Jangan modifikasi master data, role matrix, kebijakan klinis, payroll, atau sidebar untuk memperluas scope.

## 2. Task

### 2.1 Tujuan bisnis dan scope yang disetujui

Petugas HRD membuka **Sumber Daya Manusia → Siklus Kepegawaian → Rekrutmen & Seleksi → Pelamar**, memilih Daftarkan Pelamar, lalu menyelesaikan lima langkah tanpa bolak-balik ke halaman lain untuk registrasi identitas, unggah dokumen dan pengajuan lamaran.

```text
Pelamar
├── Daftar Pelamar (existing; pertahankan)
├── Daftarkan Pelamar (fitur full-page wizard)
│   ├── 01 Lowongan
│   ├── 02 Identitas
│   ├── 03 Kontak
│   ├── 04 Dokumen
│   └── 05 Konfirmasi
└── Detail Pelamar (existing; pertahankan dan integrasikan)
```

Cakupan pekerjaan ini sengaja **hanya satu submenu, Pelamar**, bukan mandat finishing seluruh Menu 03. Delapan submenu navigasi existing tetap dipertahankan. Permintaan Requisition dan Lowongan adalah dependency; Seleksi Administrasi menerima lamaran yang telah dikirim; Tes/Wawancara, Surat Penawaran (Offer), Pemantauan MCU Pra-Kerja, Talent Pool Cadangan serta Penerimaan & Onboarding tetap existing. Jangan menyisipkan wizard sebagai menu sidebar baru dan jangan menghapus halaman, key, route atau fungsi existing.

### 2.2 Keputusan pengguna (approved melalui chat; sinkronisasi canonical jika perlu)

| ID referensi lokal planning (bukan ID DEC canonical) | Keputusan |
|---|---|
| REG-WIZ-01 | Wizard full-page, lima langkah, mengikuti layout dan warna existing, bukan modal besar. |
| REG-WIZ-02 | Entry utama dari daftar Pelamar; sidebar dan detail existing tidak dirombak. |
| REG-WIZ-03 | Pilih lowongan Published dan masih terbuka; isi jabatan/unit otomatis sesuai sumber data, bukan input UUID atau master duplikat. |
| REG-WIZ-04 | Cari kandidat berdasarkan NIK; kandidat existing direuse setelah konfirmasi; tampilkan indikasi rehire tanpa menyimpulkan kelayakan. |
| REG-WIZ-05 | Simpan profil kandidat setelah identitas dan kontak lengkap; dokumen terhubung ke kandidat; lamaran hanya dibuat pada pengiriman final. |
| REG-WIZ-06 | Dukung pendaftaran kandidat tanpa lowongan seperti existing; jalur tersebut tidak membuat lamaran. |
| REG-WIZ-07 | Dokumen mengikuti kebutuhan lowongan; bedakan unggah dengan verifikasi; STR klinis diperiksa dan aturan screening existing dipertahankan. |
| REG-WIZ-08 | Draft/resume versi awal menggunakan data kandidat dan dokumen yang benar-benar tersimpan. Draft penuh sebelum Step 3 atau pilihan lowongan yang belum diajukan tidak dijanjikan persisten. |
| REG-WIZ-09 | Reuse API existing, base components, validasi per langkah, review, error handling, permission dan anti-duplikasi; perubahan BE minimal sesuai gap nyata. |
| REG-WIZ-10 | UAT live browser; tidak mencakup portal pelamar eksternal, Recruitment Journey lengkap, Onboarding Wizard, akun pegawai, perubahan sidebar maupun kebijakan approval. |

ID di atas digunakan sebagai trace internal instruksi. Agent wajib memetakan terhadap ID resmi canonical yang ada, tanpa mengarang bahwa ID tersebut sudah terdaftar pada roadmap/decision log.

### 2.3 Prioritas task FIN (label pekerjaan usulan; rekonsiliasi dengan register resmi)

| FIN | Fase | Hasil |
|---|---|---|
| FIN-HRD-REC-WIZ-01 | BE readiness | Verifikasi kontrak, dependency, permission, data integrity, rehire, uniqueness, storage, dan gap validasi. Perbaiki gap BE yang terbukti dalam scope. |
| FIN-HRD-REC-WIZ-02 | FE | Entry dari daftar/detail, Step 1–3, lookup exact NIK, simpan profil/resume, validasi dan jalur tanpa lowongan. |
| FIN-HRD-REC-WIZ-03 | FE + integrasi | Step 4 unggah dokumen dan metadata, reload, review Step 5, submit lamaran, recovery dan success page. |
| FIN-HRD-REC-WIZ-04 | QA | Uji API, security, regresi, UAT live browser, perbaiki dan retest, laporan. |

Pakai ID resmi bila sudah dialokasikan. Jangan menandai task roadmap lama DONE hanya karena wizard selesai, atau membuat roadmap paralel. Pertahankan trace `BE-HRD-054`, `BE-HRD-055`, `FE-HRD-048`, `FE-HRD-049`, `FE-HRD-057` serta periksa task dependency lowongan/rekrutmen relevan.

### Submenu 1 - Pelamar

**Target:** kandidat dan lamaran dicatat sekali, menggunakan satu wizard, dengan jalur existing tetap tersedia. Aktor: petugas rekrutmen/HRD yang memiliki permission sesuai aksi. Prasyarat lamaran: lowongan aktif, belum ditutup, data kandidat lengkap dan hak akses sesuai kontrak. Registrasi kandidat tanpa lowongan tetap boleh. Output: `CandidateId` dan `CandidateCode` setelah registrasi, `ApplicationId` dan `ApplicationNumber` hanya setelah pengajuan lamaran sukses. Tidak menghasilkan data karyawan, akun, kontrak, status lulus, atau status onboarding.

#### 2.4 Entry dan navigasi

1. Pertahankan halaman `/hr/recruitment/candidates`, daftar/filter/paginasi/status dan detail kandidat existing. Ganti tindakan pendaftaran utama yang sekarang membuka modal menjadi **Daftarkan Pelamar** yang menuju wizard full-page.
2. Usulan route `/hr/recruitment/candidates/create` dengan page tipis dan view/form/hook sesuai arsitektur FE; periksa collision dengan `[id]` dan aturan akses route. Untuk melanjutkan kandidat tersimpan, gunakan route/form yang sama dengan identifier opaque `candidateId` yang tervalidasi (misalnya query parameter ID), atau route detail/resume sesuai pola existing. Jangan memasukkan NIK/email/nomor telepon ke URL wizard.
3. Tambahkan tindakan **Lanjutkan Pendaftaran** pada kandidat yang relevan atau aksi **Daftarkan ke Lowongan** dari detail existing yang membawa ID kandidat, tanpa membuat kandidat baru. Kandidat bisa mempunyai lebih dari satu lamaran berbeda; jangan menyederhanakan bahwa semua kandidat Active berarti pendaftarannya selesai untuk semua lowongan.
4. Sediakan mode **Daftar kandidat tanpa lowongan** sebagai opsi yang jelas di Step 1. Mode ini tetap menggunakan wizard dan berhenti pada penyimpanan kandidat, bukan membuat application kosong.
5. Tombol kembali pada wizard tidak menghilangkan input yang masih ada dalam sesi. Keluar/refresh dengan perubahan belum disimpan menampilkan peringatan yang proporsional. Setelah profil tersimpan, tampilkan kode kandidat dan mekanisme resume. Jangan menyatakan semua pilihan/draft otomatis tersimpan.

#### 2.5 Step 01 — Lowongan

**Tujuan:** menentukan konteks lamaran dan mengurangi input ganda.

| Field/UI | Sumber dan perilaku |
|---|---|
| Mode pendaftaran | Pilih lowongan atau daftar kandidat tanpa lowongan. |
| Lowongan pekerjaan | Resource select existing dari `GET /vacancies`; filter Published dan pengecekan penutupan. |
| Nomor, judul, jabatan, unit | Read-only dari data lowongan/requisition yang berelasi. Gunakan endpoint detail existing bila perlu; tampilkan informasi yang benar-benar tersedia. |
| Kategori klinis/nonklinis | `isClinical` dari lowongan; tidak dapat diubah pengguna. |
| Batas pendaftaran dan persyaratan | Dari response lowongan/kontrak; jangan mengarang persyaratan ketika data belum ada. |

Validasi: mode lowongan wajib mempunyai `vacancyId` valid; tidak tampilkan opsi Draft/Closed sebagai pilihan aktif; waktu tutup diverifikasi server pada submit akhir. Jika lowongan tidak tersedia, tampilkan empty state dan jalur kembali ke Lowongan, tanpa membuat lowongan dari wizard. Jangan mengubah headcount atau alokasi kursi saat registrasi pelamar.

Skema UI:

```text
STEP 1 / 5 — Pilih Lowongan
(●) Daftarkan ke lowongan     ( ) Daftar kandidat tanpa lowongan
Lowongan pekerjaan * [ Cari judul / nomor lowongan         v ]
┌─ Ringkasan Lowongan ──────────────────────────────────────┐
│ Nomor        : <data backend>   Kategori : <data backend> │
│ Jabatan      : <data backend>   Unit     : <data backend> │
│ Penutupan    : <data backend>                              │
│ Persyaratan  : <data tersedia>                             │
└─────────────────────────────────────────────────────────────┘
[Batal]                                      [Selanjutnya >]
```

#### 2.6 Step 02 — Identitas

| Field | Kontrak / validasi |
|---|---|
| NIK | `nationalIdNumber`, tepat 16 digit angka, trim; unik menurut registrasi backend. |
| Nama lengkap | `fullName`, wajib, maksimal 150 karakter. |
| Jenis kelamin | `gender`, opsi existing, maksimal 10 karakter. Jangan mengunci default baru tanpa konfirmasi. |
| Tempat lahir | `birthPlace`, maksimal 100 karakter. |
| Tanggal lahir | `birthDate`, wajib di FE existing; date-only valid, jangan menerima tanggal masa depan. |

- Setelah NIK valid, gunakan pencarian `GET /candidates?searchNik=...` (server existing melakukan `Contains`). Pastikan hasil kecocokan **exact**, termasuk paginasi apabila relevan, dan jangan menganggap substring sebagai kandidat yang sama. Jangan mengekspos NIK di telemetry, link navigasi, screenshot atau laporan; batasi penggunaan query lookup pada endpoint existing yang terotorisasi.
- Kandidat ditemukan: tampilkan ringkasan minim yang diperlukan, status kandidat dan kode; petugas mengonfirmasi **Gunakan Kandidat Existing**. Ambil detail, prefill identitas/kontak, tampilkan dokumen existing di Step 4. Jangan overwrite data tanpa tindakan edit yang jelas; jika perubahan disimpan gunakan `PUT` dengan `rowVersion` terbaru. NIK existing tidak diubah melalui DTO update yang tidak mendukungnya.
- Kandidat tidak ditemukan: lanjutkan formulir baru. Hasil negatif lookup bukan jaminan unik; tangani `409` dari POST secara aman dengan lookup/refetch dan konfirmasi penggunaan kandidat existing.
- Rehire existing ditentukan BE melalui pencocokan NIK ke `MstEmployee`; tampilkan sebagai indikasi riwayat yang perlu ditinjau, bukan persetujuan penerimaan. Jangan mengubah status kandidat Hired/Rejected/Blacklisted/TalentPool menjadi Registered. Hormati aturan existing dan eskalasi apabila kelayakan pendaftaran ulang belum terdefinisi.
- Cegah stale response jika NIK diganti atau pengguna melakukan lookup cepat. Permission berlaku pada pencarian dan detail; `403` bukan berarti data tidak ada.

```text
STEP 2 / 5 — Identitas Pelamar
NIK *          [________________] [Periksa NIK]
<hasil: kandidat baru / kandidat existing / tidak berwenang / gagal>
Nama lengkap * [___________________________________________]
Jenis kelamin  [________________]  Tempat lahir [___________]
Tanggal lahir* [____/____/______]
[< Kembali]                                 [Selanjutnya >]
```

#### 2.7 Step 03 — Kontak dan checkpoint penyimpanan

| Field | Kontrak / validasi |
|---|---|
| Nomor telepon | `phoneNumber`, wajib, maksimal 30 karakter, simpan string sesuai kontrak. |
| Email | `email`, wajib, format valid, maksimal 100 karakter. |
| Alamat | `address`, maksimal 300 karakter. |

- Tombol **Simpan Profil & Lanjutkan** adalah checkpoint persisten pertama untuk kandidat baru. Jelaskan bahwa data identitas/kontak akan tersimpan. Jalankan validasi semua field Step 2–3, POST `/candidates` hanya sekali, ambil `id`, `candidateCode`, `statusName`, `rowVersion`, dan `isRehire` dari respons/refetch. Tidak membuat lamaran pada fase ini.
- Untuk kandidat existing, pakai ID yang sudah dikonfirmasi. Simpan perubahan hanya bila diperlukan dan diizinkan `Candidate:Update`, memakai rowVersion. Tangani konflik perubahan bersamaan dengan reload dan pilihan edit ulang; jangan overwrite diam-diam.
- Setelah profil tersimpan, route/resume menyertakan ID kandidat sesuai pola existing sehingga refresh dapat memuat profil dan dokumen. Step aktif dan pilihan lowongan yang belum diajukan boleh hilang saat reload; pengguna diberi tahu untuk memilih lowongan kembali. Jangan membuat draft database baru hanya untuk posisi step.
- Jika API gagal, jangan maju atau menganggap kandidat sudah tercipta. Jika timeout setelah request, refetch/lookup exact NIK dan konfirmasi hasil; jangan blind retry.
- Jalur kandidat tanpa lowongan tetap dapat menyelesaikan Step 4 dan review final **Simpan Kandidat**, tanpa POST `/apply`.

```text
STEP 3 / 5 — Informasi Kontak
Nomor telepon * [__________________________]
Email *         [__________________________]
Alamat          [__________________________]
                [__________________________]
Info: Profil akan tersimpan saat tombol di bawah dipilih.
[< Kembali]                        [Simpan Profil & Lanjut >]
```

#### 2.8 Step 04 — Dokumen

Gunakan katalog enum dokumen existing, bukan master baru. Tampilkan daftar KTP, CV, ijazah, transkrip, STR, SIP, sertifikat kompetensi, surat keterangan sehat dan Lainnya sesuai data/kebutuhan lowongan. Dokumen non-STR jangan dijadikan wajib secara baru tanpa dukungan kontrak persyaratan existing. Lowongan klinis memunculkan panduan STR sesuai `VR-REC-07/08`; nonklinis tidak diwajibkan STR (`VR-REC-09`). SIP hanya sesuai kebutuhan, bukan mandatory universal.

| Informasi dokumen | Perilaku |
|---|---|
| Jenis / judul | Enum `DocumentType` existing; judul wajib maksimal 200 karakter. |
| Nomor | `documentNumber` maksimal 100 karakter, sesuai jenis dokumen. |
| Berkas | Upload fisik melalui mekanisme storage existing, tampilkan batas/format aktual dari UI dan backend; controller mendokumentasikan maksimum 10 MB, cek policy aktual. |
| Masa berlaku | `validFrom`, `validUntil` nullable sesuai dokumen; validasi urutan tanggal bila keduanya diisi. Jangan membuat tanggal kedaluwarsa palsu untuk STR tanpa masa akhir. |
| Status | Belum diunggah / Mengunggah / Tersimpan / Gagal; status verifikasi terpisah (`isVerified` dari BE). |
| Dokumen existing | GET detail kandidat; boleh gunakan kembali dokumen yang relevan dan diizinkan. Jangan duplikasi upload atau menimpa arsip existing. |

Alur teknis setiap dokumen:

1. Kandidat telah tersimpan dan ID sah. Gunakan komponen uploader existing.
2. `POST /candidates/documents/attachments` multipart `file`; simpan `key` dan metadata respons, jangan hardcode file path/size/MIME.
3. `POST /candidates/{id}/documents` dengan `documentType`, `documentTitle`, `documentNumber`, `filePath` (key dari storage), `fileSize`, `mimeType`, `validFrom`/`validUntil` sesuai DTO. Untuk wizard sebelum lamaran dikirim, dokumen kandidat-level dapat menggunakan `ApplicationId=null` sebagaimana kontrak existing. Jangan otomatis mengambil dokumen milik kandidat lain atau memalsukan relasi ke application yang belum dibuat.
4. Hanya setelah metadata berhasil tersimpan, tandai dokumen **Tersimpan**. Refetch detail kandidat dan pastikan berkas/metadata dapat dibaca sesuai permission. Jika upload fisik sukses tetapi metadata gagal, simpan informasi keberhasilan upload selama sesi untuk retry metadata tanpa upload ulang bila key valid; refetch sebelum retry dan jangan menghasilkan dokumen duplikat. Jika terjadi orphan storage, laporkan dan gunakan mekanisme cleanup existing yang sah; jangan menghapus berkas tanpa kontrak/izin.
5. Retry hanya baris gagal; status sukses tetap. Jangan membuat endpoint delete baru hanya untuk wizard. Jika terdapat tindakan penggantian/penghapusan dokumen existing, ikuti kontraknya; jika belum ada, tampilkan keterbatasannya dan jangan lakukan overwrite diam-diam.
6. Peringatan STR hilang/kedaluwarsa untuk lowongan klinis muncul sebelum final. Jangan mengubah aturan screening agar meluluskan pelamar; dokumen unggah bukan verifikasi. Karena planning menyepakati peringatan dan screening tetap berlaku, jangan menambahkan blokir pengiriman lamaran baru di luar keputusan/kontrak. Boleh mendorong HRD melengkapi STR sebelum memulai screening.

**Risiko integrasi yang wajib diuji:** `CandidateScreeningService` saat baseline memuat seluruh dokumen kandidat dan mengambil STR pertama (`FirstOrDefault`). Kandidat dengan beberapa STR atau dokumen terkait lamaran berbeda bisa memerlukan pemilihan dokumen yang lebih tepat. Uji kandidat dengan STR lama dan baru, serta relasi application. Jika aturan canonical sudah menentukan pemilihan yang benar, lakukan perbaikan minimal dan regression test; jika tidak, catat blocker keputusan owner khusus pemilihan dokumen. Jangan diam-diam menetapkan aturan hukum/klinis baru atau mengubah screening secara luas.

```text
STEP 4 / 5 — Dokumen Pendukung
<kode kandidat> · <lowongan / kandidat tanpa lowongan>
┌─ Informasi kelengkapan ────────────────────────────────────┐
│ Dokumen tersimpan: <jumlah nyata>  Gagal: <jumlah nyata>   │
│ <peringatan klinis apabila STR belum lengkap / kedaluwarsa>│
└─────────────────────────────────────────────────────────────┘
Jenis dokumen       Nomor / Masa berlaku       Status    Aksi
KTP                 ...                        ...       Unggah
CV                  ...                        ...       Unggah
Ijazah              ...                        ...       Unggah
STR (jika klinis)   ...                        ...       Unggah
[+ Dokumen Lainnya]  <opsi jenis dokumen dari enum>
[< Kembali]                 [Simpan & Keluar] [Review >]
```

#### 2.9 Step 05 — Konfirmasi dan pengiriman

Tampilkan ringkasan lowongan (atau mode tanpa lowongan), identitas, kontak, dokumen tersimpan dan dokumen belum lengkap, serta catatan lamaran opsional (`notes`, maksimum 500 karakter). Sediakan **Ubah** per bagian yang kembali ke step relevan. Masking NIK pada ringkasan jika sesuai kebutuhan/permission; jangan mengulang data pribadi tanpa alasan. Konfirmasi petugas bahwa data sudah diperiksa.

- Refetch profil dan data lowongan seperlunya sebelum submit; revalidasi eligibility di backend. Jika perubahan data belum disimpan, simpan/konfirmasi secara eksplisit. Jangan melanjutkan bila upload masih berjalan, ada kegagalan metadata yang belum ditangani, atau data wajib tidak valid.
- Mode melamar: `POST /candidates/{id}/apply` hanya pada aksi final **Kirim Lamaran** dengan `vacancyId` dan `notes`. Existing BE memvalidasi lowongan Published, belum lewat penutupan, dan kombinasi kandidat–lowongan belum pernah dilamar (non-deleted); lamaran baru berstatus tahap/status existing. Kandidat `Registered` berubah menjadi `Active`, tidak memaksa status lain berubah.
- Mode tanpa lowongan: **Simpan Kandidat**, tidak memanggil `/apply`. Profil dan dokumen sudah tersimpan; tampilkan kode kandidat, bukan nomor lamaran palsu.
- Jika `409` duplikasi lamaran atau hasil submit tidak diketahui karena timeout, refetch detail kandidat, cari application yang cocok dengan vacancy ID, dan tampilkan lamaran existing setelah bukti valid. Jangan kirim request ulang buta. Jika tidak ditemukan, tampilkan kegagalan dan izinkan retry setelah status diperiksa. Jika lowongan ditutup, kembali ke Step 1 dan pilih lowongan lain tanpa menghapus profil/dokumen.
- Sukses hanya setelah server mengembalikan atau refetch membuktikan kandidat dan application yang dimaksud. Tampilkan nomor kandidat, nomor lamaran (jika ada), lowongan dan status server. Tindakan **Lihat Detail Pelamar**, **Kembali ke Daftar Pelamar**, serta **Lanjut ke Seleksi Administrasi** bagi petugas yang berwenang dan application tersedia, menggunakan route/filter/detail existing yang benar. Jangan otomatis menginisiasi atau meluluskan screening pada saat submit.

```text
STEP 5 / 5 — Review & Konfirmasi
Lowongan & unit            <ringkasan>                      [Ubah]
Identitas pelamar          <ringkasan>                      [Ubah]
Informasi kontak           <ringkasan>                      [Ubah]
Dokumen tersimpan          <status aktual / peringatan>     [Ubah]
Catatan lamaran            [___________________________________]
[ ] Saya telah memeriksa data pendaftaran.
[< Kembali]                    [Simpan & Keluar] [Kirim Lamaran]
```

Hasil sukses:

```text
Lamaran berhasil dikirim (hanya jika server membuktikan hasil)
Nomor kandidat : <CandidateCode aktual>
Nomor lamaran  : <ApplicationNumber aktual>
Lowongan       : <judul aktual>
Status         : <status server aktual>
[Kembali ke Daftar Pelamar] [Lihat Detail] [Seleksi Administrasi*]
* Jika hak akses dan kondisi workflow mengizinkan.
```

### 2.10 Skema halaman global dan UI/UX

```text
┌─────────────────┬───────────────────────────────────────────────────────────┐
│ Sidebar existing│ Hero existing: Pendaftaran Pelamar Baru                    │
│ (tidak diubah)  │ Rekrutmen & Seleksi / Pelamar                               │
│                 ├───────────────────────────────────────────────────────────┤
│                 │ 1 Lowongan — 2 Identitas — 3 Kontak — 4 Dokumen — 5 Review │
│                 ├────────────────────────────────────┬──────────────────────┤
│                 │ Konten langkah aktif               │ Ringkasan kontekstual│
│                 │ Field berlabel, helper/error       │ Lowongan / kandidat  │
│                 │ Hanya satu section utama           │ Status tersimpan     │
│                 │                                    │ Peringatan relevan   │
│                 ├────────────────────────────────────┴──────────────────────┤
│                 │ [Kembali]        [Simpan & Keluar*]          [Selanjutnya]  │
└─────────────────┴───────────────────────────────────────────────────────────┘
* Hanya tersedia sesuai checkpoint persisten dan hak akses.
```

- Desktop: stepper horizontal dalam content shell existing; form satu area utama; ringkasan samping bersifat ringkas, opsional ketika relevan; actions di bawah form, bukan di hero. Gunakan grid dan spacing design tokens existing, maksimum dua kolom untuk field pendek. Jangan buat rangkaian card tinggi untuk semua step sekaligus.
- Layar sempit: satu kolom, step aktif dan indikator `Langkah n dari 5`, ringkasan dapat ditampilkan inline; tidak ada horizontal overflow atau tombol terpotong. Navigasi keyboard, label/aria, focus pada error atau heading step, status tidak mengandalkan warna saja.
- Status step: belum dimulai / aktif / selesai berdasarkan validasi dan state nyata, bukan sekadar pernah diklik. Tombol Next tidak bypass prasyarat. Step lampau dapat diakses untuk koreksi. Ubah pilihan lowongan wajib menghitung ulang persyaratan, menghapus state lamaran belum dikirim yang stale tanpa menghapus dokumen persisten.
- Loading skeleton/spinner, empty state, error inline, notifikasi sukses dari API; disable tombol selama action. Jangan gunakan mock, hardcode GUID, nama/data contoh di produksi, toast sukses palsu, atau fallback yang menyamarkan API gagal.
- Jaga tampilan daftar dan detail Pelamar; jangan ubah tema global, komponen base lintas modul, header atau sidebar tanpa kebutuhan nyata dan gate yang disetujui.

## 3. Context

### 3.1 Evidence source dan peta existing

Baseline source dibaca secara read-only. Keberadaan source bukan hasil test live. Agent wajib membuka kembali file pada HEAD lokal, cocokkan simbol aktual, DTO, permission dan dependency.

| Bagian | Path/simbol sumber | Status dan tindakan |
|---|---|---|
| Sidebar | FE `src/utils/menu-sidebar/menu-items.jsx` (`corporateCandidates`, route `/hr/recruitment/candidates`) | Existing; jangan diubah. |
| Daftar | FE `src/app/hr/recruitment/candidates/page.jsx`, `src/components/view/hr/recruitment/candidates-directory-view.jsx` (`CandidatesDirectoryView`, `submitCreate`) | Existing modal POST kandidat dan redirect detail; ganti entry ke wizard, pertahankan daftar. |
| Detail | FE `src/app/hr/recruitment/candidates/[id]/page.jsx`, `src/components/view/hr/recruitment/candidate-detail-screening-view.jsx` | Existing apply, upload, screening; reuse action dan permission; tambah entry resume bila diperlukan. |
| FE service | `src/lib/services/hr/recruitment-service.js` (`RecruitmentService`) | Existing list/detail/register/update/apply/attachments/documents; reuse, lengkapi mapping hanya jika kontrak mendukung. |
| FE constants | `src/lib/constants/hr/recruitment/recruitment-constants.js` | Existing `RECRUITMENT_ROUTES`, `CANDIDATE_STATUS`, `DOCUMENT_TYPE_OPTIONS`, `VACANCY_SELECT_RESOURCE`; pertahankan enum dan route existing. |
| FE hooks | `src/lib/hooks/hr/use-hr-remote-data` | Existing `useStrictPermissions`, `useHrRemoteData`, `useHrAction`, toast/select helper; cek kontrak sebelum reuse. |
| BE registrasi | `Areas/Corporate/HumanResource/RecruitmentManagement/Controllers/CandidateController.cs`; `Services/CandidateService.cs` (`RegisterAsync`, `UpdateAsync`) | Existing POST/PUT, exact duplicate NIK check, rehire via `MstEmployee`, `Registered`, RowVersion. Verifikasi uniqueness secara concurrency. |
| BE kontrak | `.../RecruitmentManagement/DTOs/CandidateDtos.cs` | `RegisterCandidateRequest`, `UpdateCandidateRequest`, `ApplyVacancyRequest`, `UploadCandidateDocumentRequest`, `CandidateResponse`, `CandidateApplicationResponse`, `CandidateDocumentResponse`. |
| BE lowongan | `.../RecruitmentManagement/Controllers/JobVacancyController.cs`, `DTOs/JobVacancyDtos.cs` | Existing data lowongan; cek response detail/requisition untuk unit, posisi, persyaratan, published dan closing date. |
| BE apply | `.../RecruitmentManagement/Services/CandidateService.cs` (`ApplyVacancyAsync`) | Existing Published + closing date + duplicate candidate/vacancy guard; perubahan Registered → Active; submit hanya di Step 5. |
| BE dokumen | `.../RecruitmentManagement/Controllers/CandidateController.cs` (`UploadHrAttachment`, `DownloadHrAttachment`, `UploadDocument`), `Services/CandidateService.cs` (`UploadDocumentAsync`) | Upload fisik dan metadata terpisah, `ApplicationId` opsional, `IsVerified=false`; cek akses dan ownership key, idempotensi dan kegagalan parsial. |
| BE screening | `.../RecruitmentManagement/Services/CandidateScreeningService.cs` | STR klinis wajib untuk screening, missing/expired auto-reject, nonklinis tanpa STR; pemilihan STR pertama perlu test. Jangan ubah kebijakan. |
| Entitas existing | `HrdCandidate`, `HrdCandidateApplication`, `HrdCandidateDocument`, `HrdJobVacancy`, `MstEmployee` | Rujukan dari service dan roadmap; cek model/config/index/relasi nyata sebelum menyimpulkan dukungan skema. |

Tautan snapshot:

- [Frontend menu dan source HamzahV2](https://github.com/DevBenari/QuilvianSystemFrontendDev/tree/f122c1d641f2cf47e3597762e6956fabdae37eaa/src/components/view/hr/recruitment)
- [CandidateService backend](https://github.com/DevBenari/NewQuilvianSystemBackend/blob/6945b70d0f5ff35520f1789b7d5e7acbd3ecae48/Areas/Corporate/HumanResource/RecruitmentManagement/Services/CandidateService.cs)
- [CandidateController backend](https://github.com/DevBenari/NewQuilvianSystemBackend/blob/6945b70d0f5ff35520f1789b7d5e7acbd3ecae48/Areas/Corporate/HumanResource/RecruitmentManagement/Controllers/CandidateController.cs)
- [CandidateDtos backend](https://github.com/DevBenari/NewQuilvianSystemBackend/blob/6945b70d0f5ff35520f1789b7d5e7acbd3ecae48/Areas/Corporate/HumanResource/RecruitmentManagement/DTOs/CandidateDtos.cs)
- [Blueprint HRD](https://github.com/DevBenari/NewQuilvianSystemBackend/tree/6945b70d0f5ff35520f1789b7d5e7acbd3ecae48/docs/module-blueprints/human-resource)
- [Engineering governance](https://github.com/DevBenari/QuilvianEngineeringSkills/tree/62b7ead4595161ec8f9bb7fc2dd96b3a7b29f2a5/agents/rules)

### 3.2 Kontrak API actual baseline

BE prefix: `/api/v1/corporate/human-resource/recruitment`. FE `RecruitmentService` uses `/v1/corporate/human-resource/recruitment` through configured `InstanceAxios`. Jangan menggandakan prefix `/api` atau menebak base URL. Semua endpoint di bawah adalah existing; bukan usulan endpoint baru.

| Method | Relative endpoint | Input / response penting | Permission |
|---|---|---|---|
| GET | `/vacancies` dan detail existing | Filter status, list/detail lowongan, `isClinical`, closing date | Verifikasi `JobVacancy:Read` pada controller/source |
| GET | `/candidates` | `searchNik`, pagination; hasil exact disaring dengan aman | `Candidate:Read` |
| GET | `/candidates/{id}` | Detail, `rowVersion`, applications, documents | `Candidate:Read` |
| POST | `/candidates` | `RegisterCandidateRequest` → `CandidateResponse` | `Candidate:Create` |
| PUT | `/candidates/{id}` | `UpdateCandidateRequest` + `rowVersion`; NIK tidak termasuk kontrak update | `Candidate:Update` |
| POST | `/candidates/documents/attachments` | Multipart `file` → storage key dan metadata | `Candidate:UploadDocument` |
| POST | `/candidates/{id}/documents` | `UploadCandidateDocumentRequest` → document ID | `Candidate:UploadDocument` |
| GET | `/candidates/documents/attachments/download` | `key` → file sesuai kebijakan akses | `Candidate:Read` + pemeriksaan akses storage |
| POST | `/candidates/{id}/apply` | `ApplyVacancyRequest` → application ID dan nomor | `Candidate:Update` |

Jaga envelope `ApiResponse<T>` dan handling `400`, `401`, `403`, `404`, `409`, `422`, `5xx`, network timeout. `useStrictPermissions` memandu visibilitas tombol, tetapi BE tetap sumber otorisasi. Jangan memperluas role atau bypass middleware untuk meloloskan UAT. Screening mempunyai resource `CandidateScreening` dan tindakan existing terpisah.

### 3.3 State dan persistensi (bukan enum baru)

| Fase UI | State server |
|---|---|
| Step 1–3 sebelum checkpoint | Belum ada kandidat baru; state sementara di memori UI, kecuali kandidat existing. |
| Step 3 berhasil | Kandidat baru `Registered`; ID dan kode server tersedia. |
| Step 4 | Dokumen fisik + metadata tersimpan, `IsVerified=false` saat unggah baru. |
| Step 5 Kirim Lamaran berhasil | `HrdCandidateApplication` tercipta; status lamaran dari response; `Registered` → `Active` jika berlaku. |
| Step 5 Simpan Kandidat | Tidak membuat lamaran; pertahankan status kandidat sebenarnya. |
| Seleksi Administrasi | Tahap pemeriksaan terpisah oleh petugas berwenang, aturan VR existing tetap berlaku. |

**Draft yang disetujui:** resume dari kandidat dan dokumen persisten. Jangan menampilkan badge `Draft` sebagai enum server baru atau mengklaim wizard progress disimpan backend. Untuk kandidat lama/Active, hitung konteks dari lamaran yang dipilih; tidak ada single global progress yang mengabaikan banyak application. Tidak menyimpan NIK, alamat, email, dokumen, file blob, atau token pada `localStorage`/storage browser untuk draft. Gunakan state lokal selama sesi dan data server setelah checkpoint; pilihan lowongan boleh diminta ulang setelah refresh.

### 3.4 Integrity dan keamanan wajib

- NIK unik, candidate–vacancy unik, nomor kandidat/lamaran aman pada request paralel; source baseline menggunakan cek eksistensi dan terdapat numbering berbasis count. Jangan menyimpulkan jaminan race-free tanpa mengecek index, constraint, transaksi, generator nomor canonical, dan test concurrency. Bila ada gap, perbaiki secara minimal dengan pola canonical. Jangan membuat migration tanpa otorisasi; jika constraint yang diperlukan belum ada, laporkan `BLOCKED_SCHEMA` untuk acceptance tersebut dan lanjutkan bagian independen.
- Dokumen: cek key milik scope/kandidat/actor yang sah sesuai mekanisme storage, file type, size, metadata valid, key tidak berasal dari path bebas yang tidak diverifikasi, download tidak bocor lintas scope. Jangan menganggap upload permission berarti akses ke semua berkas kandidat tanpa cek aturan existing.
- Jangan membuat profil atau dokumen ganda saat retry. Pertahankan ID hasil operasi berhasil, refetch sebelum retry yang hasilnya belum diketahui, deduplikasi berbasis identitas operasi/key yang benar-benar didukung. Cegah double-click dan request tumpang tindih. Jika perlu mekanisme idempotensi server tambahan, gunakan pola existing dan dokumentasikan kontrak; jangan hanya mengandalkan FE disabled untuk klaim race-safe.
- Audit actor, timestamps, validasi dan RowVersion mengikuti BE. Jangan log data pribadi sensitif atau path/token storage. Gunakan data sintetis di test, redact screenshot dan payload report.
- Periksa state kandidat existing dan akses user; jangan auto-create akun Identity, `MstEmployee`, workforce profile, kontrak, approval, payroll, atau handoff ketika registrasi. Jangan menutup kekurangan workflow dengan data dummy.

## 4. Reasoning / Execution Workflow

### 4.1 Preflight governance dan task binding

1. Baca `AGENTS.md`, `CLAUDE.md` dari kedua workspace; bandingkan HEAD dan working tree secara read-only; cek file relevan dan jangan undo uncommitted work yang bukan milik task.
2. Baca canonical `rules/GLOBAL_RULES.md` (sumber `QuilvianEngineeringSkills/agents/rules/GLOBAL_RULES.md`) dan rule relevan. CLAUDE.md terbaru menyatakan pemanggilan Skill opsional secara umum; **prompter ini secara eksplisit meminta runtime skill** `quilvian-engineering-skills:build-module-backend` untuk perubahan BE dan `quilvian-engineering-skills:build-module-frontend` untuk perubahan FE. Gunakan mekanisme registered skill di IDE yang benar, jangan berpura-pura sudah invoke. Jika skill yang diminta tidak tersedia, laporkan blocker fase terkait dan jangan bypass. Governance canonical tetap wajib walaupun tidak ada perubahan source BE.
3. BE: baca `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `rules/backend/` yang relevan (task, API, database, cross-repo, review, test). FE: baca `frontend-architecture`, `base-component-catalog`, `base-component-decision-gate`, `page-composition-patterns`, `design-tokens`, `ui-consistency-checklist`, `test-policy`; ikuti `rule-output` dan template laporan aktual.
4. Baca bagian rekrutmen dari `docs/module-blueprints/human-resource/README.md`, `00-interview-decisions.md`, `01-existing-capability-map.md`, prerequisite, contracts/flowcharts/data, roadmap FE/BE, requirement traceability, dan laporan `FE-HRD-048/049`, `BE-HRD-054/055`, task terkait. Verifikasi keputusan canonical dengan approval chat REG-WIZ-01..10. Sinkronkan hanya bagian yang dibutuhkan jika mode/izin sesuai. Jangan membuka ulang requirement yang sudah disetujui.
5. Catat task FIN dan ID resmi, baseline status, izin environment, daftar source target. Periksa `memory.md` HRD; terakhir tidak ditemukan pada snapshot audit. Jangan menganggap seed template sebagai file remote.

### 4.2 Backend terlebih dahulu — FIN-HRD-REC-WIZ-01

- Lakukan trace route → controller → DTO → service → EF model/config → persistence dan permission. Verifikasi lookup NIK exact, daftar/detail lowongan, validasi kandidat/lamaran, storage, metadata, tanggal dokumen, response dan error mapping.
- Jalankan targeted tests sesuai policy untuk alur create/refetch, update concurrency, duplicate NIK, candidate–vacancy race, lowongan Draft/Closed/expired, dokumen, permission, dan screening. Gunakan test fixture terisolasi dan environment yang diizinkan. Jangan menyentuh DB produksi.
- Perbaiki gap yang nyata dan dibutuhkan wizard, sesuai scope; gunakan model existing, service/transaction pattern canonical dan validasi server. Jika API memenuhi semua acceptance, tulis `REUSE — NO BE SOURCE CHANGE` dengan bukti test; tidak perlu membuat endpoint/tabel baru.
- Pertahankan backward compatibility karena halaman existing masih memakai kontrak. Jangan mengganti enum status, menghapus field, atau merombak screening. Jika kebutuhan schema/keputusan klinis baru menghambat acceptance, catat blocker spesifik, alternatif dan bagian yang masih dapat dikerjakan.
- Gate untuk FE: kontrak request/response, permission, error handling dan persistence minimal untuk checkpoint disepakati berdasarkan source; tulis temuan dan perubahan nyata, bukan sekadar Swagger.

### 4.3 Frontend — FIN-HRD-REC-WIZ-02 dan 03

- Buat route tipis, view form, hook/controller wizard, constants/routes, service reuse, CSS Modules hanya jika dibutuhkan, dan test sesuai pola repository. Gunakan base component yang benar-benar tersedia. Usulan nama/path tidak mengikat jika pola existing berbeda; dokumentasikan path aktual.
- Jalankan base-component decision gate sebelum JSX/CSS. Isi tabel elemen, kandidat base/reuse, props yang benar-benar didukung, keputusan dan alasan; baca source komponen dan contoh terdekat. Kandidat awal:

| Elemen | Kandidat reuse yang perlu dikonfirmasi |
|---|---|
| Hero/page shell | Hero dan layout dari `CandidatesDirectoryView` dan form existing |
| Aksi | `BaseButton` (primary, secondary; loading/disabled sesuai props) |
| Form | `BaseInputField`, `BaseSelectField`, date/textarea/checkbox base sesuai catalog; jangan hardcode UUID |
| Lookup lowongan | `HrResourceField` / `ResourceFilterSelect` + resource config existing |
| Dokumen | Uploader/HrFileField atau mekanisme komponen yang dipakai `CandidateDetailScreeningView` |
| Status/error | `HrStatusBadge`/`StatusBadge`, `InformationAlert`, toast existing |
| Confirmation | `ConfirmModal` sesuai kebutuhan untuk konfirmasi kecil/keluar; bukan wadah seluruh wizard |
| Stepper | Tinjau `src/components/view/hr/credentialing/portal-credentialing-wizard-view.jsx` dan `src/components/view/hr/payroll/payroll-run-wizard-view.jsx` sebagai pola; `WorkflowStepperCard.jsx` adalah tabel tahap approval, bukan otomatis stepper form. Reuse pola yang sesuai atau komposisi base components; buat komponen domain baru hanya setelah gate membuktikan kebutuhan. |

- Implement state wizard, step validation, dirty state, resume, fallback/error, exact NIK lookup dengan pembatalan request stale, permission per aksi, application-bound context. Gunakan FE existing action helper/API envelope.
- Implement step 1–3 dan checkpoint server dahulu; verifikasi refresh/resume. Lanjut step 4 dengan upload nyata, retry yang aman dan re-fetch metadata, lalu Step 5 dengan review dan final apply. Integrasikan kembali detail/seleksi existing.
- Jangan mengubah list layout atau global style. Jangan menggunakan HTML/CSS ad-hoc untuk mengganti base component yang sudah ada; jangan menambah dependency baru tanpa alasan/izin. Gunakan token semantik, responsive dan a11y.

### 4.4 Test Case UAT live browser — FIN-HRD-REC-WIZ-04

UAT harus melalui browser nyata (headed bila tersedia), bukan hanya memeriksa kode, mock server, atau unit test. Gunakan aplikasi FE–BE dev/test yang diizinkan dan akun uji sesuai role. Gunakan lowongan klinis/nonklinis Published yang sah serta kandidat sintetis. Minimal skenario berhasil: satu kandidat klinis dengan dokumen sesuai persyaratan, satu kandidat nonklinis, satu kandidat tanpa lowongan yang bisa dilanjutkan. Jangan membuat data master di luar scope tanpa izin; bila fixture yang diperlukan tidak tersedia, tandai kasus terkait `BLOCKED` dengan alasan dan lanjutkan yang tersedia.

| ID | Prioritas | Skenario dan tindakan | Expected / acceptance |
|---|---|---|---|
| UAT-01 | P0 | Login HRD berizin, buka Pelamar, klik Daftarkan Pelamar | Wizard full-page dapat diakses, daftar/detail existing tetap berjalan; sidebar tidak berubah. |
| UAT-02 | P0 | Kandidat nonklinis baru, pilih lowongan aktif, isi Step 2–3, unggah dokumen, review dan kirim | Satu kandidat dan satu lamaran persisten, kode/nomor aktual, status sesuai BE; setelah reload terlihat di detail. |
| UAT-03 | P0 | Kandidat klinis baru dengan STR sesuai persyaratan, selesaikan wizard | Dokumen persisten dengan metadata benar; application tercipta, bukan status lulus. Seleksi Administrasi dapat memeriksa application sesuai permission. |
| UAT-04 | P0 | Mode tanpa lowongan, simpan profil/dokumen, konfirmasi | Kandidat tersimpan; tidak ada application kosong/palsu. |
| UAT-05 | P0 | Refresh setelah Step 3/4, lanjutkan dari daftar/detail | Profil, ID dan dokumen dimuat dari server; lowongan yang belum tersimpan dapat dipilih ulang; tidak register kandidat kedua. |
| UAT-06 | P0 | Pilih NIK yang sudah ada | Exact match, konfirmasi reuse, data prefill; tidak ada POST create kandidat duplikat. |
| UAT-07 | P0 | Kirim lamaran untuk candidate–vacancy yang sudah ada | Konflik/lamaran existing terdeteksi; tidak ada application kedua. |
| UAT-08 | P0 | Double click simpan profil dan kirim lamaran | Satu operasi logis, tidak ada duplikasi atau toast palsu. |
| UAT-09 | P0 | Dua sesi uji bersamaan membuat kandidat dengan NIK sama atau apply vacancy sama; uji concurrency terkontrol | Server mencegah duplikasi secara nyata; jika skema tidak mendukung, `BLOCKED_SCHEMA`, bukan PASS. |
| UAT-10 | P0 | Akun tanpa `Candidate:Read` atau Create/Update/UploadDocument sesuai aksi | Tombol/route terjaga; panggilan API terlarang ditolak server; tidak terjadi data leak. |
| UAT-11 | P0 | Lowongan tutup ketika wizard masih dibuka | Apply ditolak dengan pesan jelas; profil/dokumen tetap ada, pilihan ulang lowongan tersedia. |
| UAT-12 | P0 | Gangguan jaringan/timeout setelah POST kandidat atau apply | Refetch memastikan status sebenarnya sebelum retry; tidak ada duplikasi atau false success. |
| UAT-13 | P0 | Upload fisik sukses, POST metadata gagal | Status gagal tetap jelas, retry aman, tidak upload ulang berkas yang sudah berhasil jika key valid; tidak ada metadata ganda. |
| UAT-14 | P0 | Gagal upload salah satu dari beberapa dokumen | Dokumen lain tetap tersimpan, hanya dokumen gagal perlu retry. |
| UAT-15 | P0 | Coba akses dokumen/kandidat di luar scope melalui ID/key dengan akun uji terbatas | BE mengikuti policy akses existing; tidak membocorkan data; security gap terlapor/perbaiki dalam scope. |
| UAT-16 | P1 | Lowongan klinis tanpa STR atau STR kedaluwarsa | Peringatan wizard jelas; aturan screening existing tetap berlaku, tidak ada auto-lulus. |
| UAT-17 | P1 | Kandidat nonklinis tanpa STR | STR tidak dipaksakan; alur mengikuti aturan existing. |
| UAT-18 | P1 | Kandidat mempunyai beberapa STR, termasuk lama/kedaluwarsa dan terbaru | Uji pemilihan dokumen screening; lakukan fix hanya bila policy canonical jelas, jika tidak tandai keputusan owner pending. |
| UAT-19 | P1 | Kandidat NIK yang terhubung riwayat `MstEmployee` | Indikasi rehire tampil sesuai response; tidak membuat pegawai atau menjamin penerimaan. |
| UAT-20 | P1 | Kandidat existing diedit di dua sesi, submit rowVersion lama | Konflik ditangani; tidak overwrite data diam-diam. |
| UAT-21 | P1 | NIK salah format, nama kosong, tanggal tidak valid, email salah, nomor telepon kosong | Error per field; Next/checkpoint diblokir sesuai kontrak, server menolak payload invalid. |
| UAT-22 | P1 | File oversize/format tidak diizinkan; tanggal mulai > tanggal akhir | Validasi/error akurat; tidak mengaku upload sukses. |
| UAT-23 | P1 | Kembali dari Step 4/5 ke langkah sebelumnya, ubah data/lowongan | Data sesi tetap, status persyaratan dihitung ulang, perubahan persisten jelas, tidak menghapus dokumen. |
| UAT-24 | P1 | Keluar sebelum profil tersimpan / setelah profil tersimpan | Peringatan perubahan belum tersimpan; resume hanya untuk data yang memang persisten. |
| UAT-25 | P1 | Kandidat existing melamar lowongan berbeda | Reuse satu kandidat, application berbeda; status existing dihormati. |
| UAT-26 | P1 | Klik Lanjut ke Seleksi Administrasi | Route/action existing yang benar, application aktual, permission diperiksa; tidak melakukan auto approval/screening. |
| UAT-27 | P2 | GET kandidat/lowongan gagal, kosong, 403, 5xx | Empty/error/loading state benar dan retry tersedia, bukan fallback demo. |
| UAT-28 | P2 | Desktop, tablet, mobile; keyboard Tab/Enter; screen-reader label | Tidak overflow, label jelas, focus error benar, langkah aktif terbaca, tombol mudah digunakan. |
| UAT-29 | P2 | Regresi halaman daftar/detail dan menu Lowongan, Seleksi Administrasi serta route existing | Fitur di luar scope tidak rusak, menu label/key/path tetap. |
| UAT-30 | P2 | Logout/login atau reload kandidat tersimpan | Data dari server, tidak ada PII/dokumen tersimpan di browser storage; status bukan progress palsu. |

Untuk setiap test catat: environment, role uji (tanpa credential), waktu WIB, browser, data sintetis/ID yang dimasking, langkah aktual, expected, actual, hasil `PASS/FAIL/BLOCKED/NOT RUN`, bukti HTTP/status dan screenshot yang aman, bug terkait, retest. Tambahkan API/unit/integration tests untuk kondisi yang tidak dapat disimulasikan aman lewat browser (concurrency, auth bypass, response loss, invalid payload). Jangan menyatakan UAT PASS jika hanya dilakukan unit test.

### 4.5 Definition of Done

- Wizard lima langkah tersedia dari Pelamar, menggunakan base component dan data API aktual, mengikuti keputusan REG-WIZ-01..10.
- Dua jalur (melamar lowongan dan kandidat tanpa lowongan) berjalan; checkpoint profil persisten; resume dari ID; data lama tidak diduplikasi; dokumen unggah dan metadata nyata; review/submit tidak mendahului aksi final.
- NIK/lamaran duplicate, file error, request timeout, lowongan tutup, conflict, permission dan concurrency ditangani sesuai acceptance. Jika ada blocker schema/keputusan, tidak boleh diberi PASS.
- Data hasil akhir terlihat di detail dan setelah refresh; aplikasi tidak mengubah candidate menjadi pegawai atau auto-lulus screening.
- Tidak mengubah sidebar, menu di luar scope, payroll, master data, role policy atau desain global. Tidak ada migration/deploy/commit/push otomatis.
- Jalankan lint/build/test sesuai kebijakan dan batas otorisasi environment; catat command aktual, exit code, warnings dan PASS/FAIL/NOT RUN. Jika BE tidak berubah, jelaskan hasil gate reuse dan uji relevan. Jika build/test dibatasi, laporkan `NOT RUN` dan alasannya, bukan melewati tanpa catatan.
- UAT P0/P1 yang relevan lulus dan regresi terverifikasi sebelum menyatakan siap UAT accepted. Perbaiki bug dalam scope dan jalankan retest. Tidak mengklaim siap produksi tanpa bukti yang diwajibkan dan persetujuan yang sah.

## 5. Stop conditions

Jangan berhenti setelah membaca source, menyusun rencana, menemukan existing API, menulis dokumentasi, membuat mockup, atau mencapai lint/build PASS. Setelah preflight sah, lakukan perubahan source dan test dalam scope.

Tahan hanya bagian terdampak jika:

- Branch salah, governance canonical tidak terbaca, runtime skill yang diminta tidak tersedia, atau target tulis tidak diizinkan. Laporkan blocker yang tepat (`BLOCKED — canonical governance unavailable` atau `BLOCKED — Quilvian runtime skill unavailable` sesuai kondisi; jangan mengarang keberhasilan invokasi).
- Constraint/relasi/generator nomor membutuhkan schema/migration yang belum diizinkan; laporkan `BLOCKED_SCHEMA` dengan bukti serta usulan perubahan minimal.
- Aturan bisnis material, misalnya pemilihan STR yang sah dari beberapa dokumen atau kelayakan rehire, belum jelas pada canonical. Jangan menetapkan kebijakan baru. Kerjakan bagian lain yang tidak bergantung pada keputusan tersebut.
- Environment, fixture, user role, akun uji, atau izin mutasi aplikasi tidak tersedia. Lakukan pemeriksaan/tes yang aman dan laporkan UAT terkait `BLOCKED/NOT RUN`.
- Menemukan risiko data sensitif, potensi duplikasi/penghilangan data, atau destructive action; jangan bypass guard demi hasil lulus.

Tidak ada izin otomatis commit, push, pull, merge, rebase, deploy, migration (create maupun execute), direct DB mutation, atau cleanup destruktif. Jangan menyentuh repository governance atau memperluas task menjadi seluruh rekrutmen/onboarding.

## 6. Output

Agent wajib menghasilkan hasil pekerjaan yang dapat ditinjau pengguna, bukan hanya dokumen ini:

1. Ringkasan implementasi FIN-HRD-REC-WIZ-01..04, mapping task resmi, status `IMPLEMENTED / PARTIAL / REPAIR / BLOCKED / NOT VERIFIED`, dan alasan. Untuk BE, jelaskan perubahan nyata atau reuse tanpa perubahan dengan bukti.
2. Daftar file berubah aktual per repository, path route/view/hook/service/test, ringkasan diff, SHA awal dan status working tree akhir. Jangan mengklaim commit/push jika tidak dilakukan.
3. Matriks lima step dengan kondisi selesai, endpoint/permission, validasi, status dan efek persistence. Sertakan skema UI aktual dan bukti screenshot wizard langkah 1–5 serta hasil akhir yang sudah dimasking.
4. Hasil uji FE/BE, API, keamanan, regression dan UAT live dengan actual vs expected, jumlah PASS/FAIL/BLOCKED/NOT RUN, bug dan retest. Lampirkan bukti yang benar-benar ada.
5. Laporan tracked sesuai `docs/module-blueprints/human-resource/task/report/backend/` dan `task/report/frontend/` serta satu laporan UAT feature/submenu Pelamar sesuai rule-output. Update roadmap/traceability hanya pada mode dan kewenangan sah; jangan membuat task canonical fiktif atau status DONE palsu.
6. Sampaikan blocker, keputusan yang belum terkunci, dampak dan next action. Catat checkpoint memory HRD hanya jika penyimpanan diizinkan; jika tidak `MEMORY_PENDING_SYNC`. Tidak membuat master instruksi kedua.

### Checkpoint handoff (bukan laporan pelaksanaan)

- Tanggal: 25 November 2026; snapshot FE/BE/Skills pada §1.1.
- Approved: lima langkah, reuse-first, checkpoint profil Step 3, dokumen persisten, final apply Step 5, jalur kandidat tanpa lowongan, tidak mengubah sidebar.
- Risiko: draft penuh sebelum profil tersimpan tidak tersedia pada kontrak yang diperiksa; concurrency/uniqueness dan idempotensi dokumen perlu pembuktian; pemilihan STR oleh screening perlu uji dan keputusan bila kontrak tidak jelas.
- Status awal: master instruksi disiapkan; source belum diubah, build/test/UAT belum dijalankan oleh penyusun dokumen. `MEMORY_PENDING_SYNC` — tidak ada write ke GitHub.
- Next action: Agent melakukan preflight, menguji kontrak BE, lalu implementasi FE dan UAT live sesuai scope.

**PERINTAH FINAL AGENT: MULAI LANGSUNG IMPLEMENTASI MULTI-STEP PENDAFTARAN PELAMAR SESUAI REQUIREMENT YANG TELAH DISETUJUI. SELESAIKAN SOURCE BACKEND YANG BENAR-BENAR DIBUTUHKAN, BANGUN WIZARD FRONTEND DENGAN BASE COMPONENT EXISTING, HUBUNGKAN API NYATA, LAKUKAN UAT LIVE DAN RETEST. JANGAN BERHENTI PADA PLANNING ATAU AUDIT-ONLY.**
