# ISSUE-009 — Tampilan UI/UX Catatan Keperawatan Mengalami Tumpukan Double Header, Inkonsistensi Sub-Navigasi, dan Pelanggaran Base Component

```yaml
issue_id: ISSUE-KEP-009
module_id: rawat-inap
submodule: keperawatan
layar: "Asuhan Keperawatan — Sub-tab Catatan Keperawatan (FE-KEP-24 / FE-RWI-180)"
sumber_laporan: "Laporan pengguna 06-10-2026: 5 butir arahan perbaikan UI/UX (Posisi Rapi, User Friendly, Tampilan Menarik, Ikuti Template Sub-menu Lain, Ikuti Base Component), disertai tangkapan layar observasi WSD"
tanggal_issue: "2026-10-06"
status: SELESAI
keparahan_tertinggi: Medium
source_sha_backend: "671191eb1aff3f618456cb6c909863bb0eca82f4 (MHamzah)"
source_sha_frontend: "1f889d67cbaaa5c3df5f8d73cfe9cd0b1d434d83 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-009-tampilan-ui-ux-catatan-keperawatan.md
ditulis_dengan: "skill diagnose-module-issue"
```

---

## 1. Ringkasan

Laporan pengguna menyoroti ketidakrapian dan kekakuan tampilan antarmuka (UI/UX) pada menu **Catatan Keperawatan** di dalam ruang kerja perawat rawat inap (`nursing-workspace`). Dari hasil telaah visual dan penelusuran kode sumber, ditemukan anomali visual utama berupa **"Double Header Bar"**, di mana kartu header luar `Catatan Keperawatan (V1)` bertumpuk vertikal dengan kartu header internal milik sub-panel yang sedang aktif (misalnya `Observasi Pengeluaran Cairan WSD`). Struktur bertumpuk ini diperparah oleh penempatan banner peringatan keselamatan di tengah navigasi serta penggunaan tombol navigasi pill Bootstrap mentah (`nav-pills bg-light border`) yang tidak selaras dengan sub-menu keperawatan lain seperti *Obat & Alkes*.

Selain tata letak yang boros ruang vertikal, antarmuka ini masih menampilkan label teknis internal `(V1)` yang membingungkan staf perawat dan melanggar standar penyajian antarmuka klinis rumah sakit. Tombol aksi catatan naratif CPPT diletakkan secara dominan di kartu atas sehingga memecah fokus perawat yang sedang melakukan tindakan khusus seperti pemantauan selang WSD atau penetapan diet medis. Seluruh peringatan keselamatan dan pesan aksi juga masih menggunakan kelas Bootstrap mentah (`alert alert-info`, `alert alert-danger`) alih-alih memanfaatkan pustaka Base Component Quilvian resmi (`ClinicalSafetyAlert`, `InformationAlert`, `BaseButton`, `StatusBadge`).

Isu ini berstatus keparahan **Medium** karena tidak memblokir fungsionalitas pengiriman data ke server, namun sangat menurunkan kenyamanan kerja (*ergonomi kognitif*) perawat bangsal, memicu kebingungan hirarki navigasi, dan merusak konsistensi desain sistem rumah sakit Quilvian.

---

## 2. Laporan Asli

| No. laporan | Keluhan pelapor (kata-kata asli / ringkas) | Lampiran |
| :---: | --- | --- |
| 1 | "Perbaiki Tampilan UI/UX pada catatan keperawatan agar bisa tampilan bisa rapih dan terlihat bagus. Berikan rekomendasi UI/UX yang bagus: 1. Posisi Rapi, 2. User Friendly, 3. Tampilan Menarik Dan Bagus, 4. Ikuti Tamplet pada sub menu keperawatan lain, 5. Ikuti Base Component" | Tangkapan layar layar Asuhan Keperawatan → Catatan Keperawatan (V1) yang sedang membuka sub-menu Observasi Pengeluaran Cairan WSD, memperlihatkan tumpukan dua kartu header, banner aturan keselamatan di tengah, dan sub-navigasi pill abu-abu. |

---

## 3. Ringkasan Temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | :---: | --- | --- | --- | --- | --- | --- |
| `ISS-KEP-009-01` | 1 | Tumpukan dua kartu header besar (*Double Header Bar*) yang memboroskan ruang vertikal | `DESIGN_CHANGE` | Frontend | Medium | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-KEP-009-01` |
| `ISS-KEP-009-02` | 1 | Sub-navigasi 6 sub-menu memakai pill Bootstrap mentah dan berbeda dari template *Obat & Alkes* | `DESIGN_CHANGE` | Frontend | Medium | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-KEP-009-01` |
| `ISS-KEP-009-03` | 1 | Penggunaan alert dan tombol kelas Bootstrap mentah alih-alih Base Component Quilvian | `RULE_VIOLATION` | Frontend | Low | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-KEP-009-02` |
| `ISS-KEP-009-04` | 1 | Penempatan tombol catatan naratif CPPT memotong fokus klinis sub-menu spesifik | `DESIGN_CHANGE` | Frontend | Medium | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-KEP-009-03` |
| `ISS-KEP-009-05` | 1 | Label teknis internal rekayasa `(V1)` tercantum pada judul layar klinis | `RULE_VIOLATION` | Frontend | Low | SUDAH-VERIFIKASI + DARI-CAPTURE | `FIX-KEP-009-04` |
| `ISS-KEP-009-T1` | — | Kontrol internal sub-panel (pill selang WSD dan tab DPO) memakai button HTML native | `RULE_VIOLATION` | Frontend | Low | SUDAH-VERIFIKASI | `FIX-KEP-009-05` |

---

## 4. Rincian per Temuan

### ISS-KEP-009-01 — Tumpukan Dua Kartu Header Besar (Double Header Bar) Memboroskan Ruang Vertikal

| | |
| --- | --- |
| **No. laporan** | 1 (Butir 1: Posisi Rapi, Butir 3: Tampilan Menarik Dan Bagus) |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source code dan tangkapan layar pengguna |
| **Perbaikan** | `FIX-KEP-009-01` |

**Apa yang terjadi.**
Saat perawat membuka tab *Catatan Keperawatan*, layar menampilkan kartu putih besar berisi judul "Catatan Keperawatan (V1)", deskripsi panjang, serta tombol naratif CPPT. Tepat di bawahnya terdapat deretan tombol sub-menu. Ketika perawat memilih salah satu sub-menu (misalnya *Observasi Pengeluaran Cairan WSD*), sistem menampilkan KARTU PUTIH BESAR KEDUA yang memiliki bentuk, ketebalan border, dan bayangan yang persis sama, lengkap dengan judul sub-menu dan tombol-tombolnya sendiri. Akibatnya, separuh tinggi layar monitor komputer bangsal habis hanya untuk menampilkan dua kartu pengantar sebelum tabel data medis yang sebenarnya terlihat.

**Kenapa terjadi.**
Komponen koordinator `nursing-narrative-tab.jsx` mengadopsi styling header penuh (`styles.tabHeaderBar`) pada baris 116–149. Di saat bersamaan, sub-panel yang dirender di dalamnya (seperti `wsd-observation-panel.jsx` baris 274–307, `spooling-cairan-panel.jsx` baris 98–135, dan `inpatient-diet-panel.jsx` baris 209–241) masing-masing juga membungkus dirinya dengan kelas yang sama (`styles.tabHeaderBar`):

```jsx
// src/.../sections/nursing-care/tabs/nursing-narrative-tab.jsx:116
<div className={styles.tabHeaderBar}>
  <div className={styles.tabTitleGroup}>
    <div className={styles.tabTitleWithIcon}>
      <FaFileAlt className={styles.tabIcon} />
      <h3 className={styles.tabTitle}>Catatan Keperawatan (V1)</h3>
    </div>
...

// Lalu di dalam wsd-observation-panel.jsx:274
<div className={styles.tabHeaderBar}>
  <div className={styles.tabTitleGroup}>
    <div className={styles.tabTitleWithIcon}>
      <FaWater className={styles.tabIcon} />
      <h3 className={styles.tabTitle}>Observasi Pengeluaran Cairan WSD</h3>
    </div>
...
```

Pola ini membuat tata letak bertingkat dua tanpa pembagian tanggung jawab visual yang jelas antara navigasi level sub-modul dan kartu kerja operasional.

**Apakah ini menyimpang dari desain?**
`DESIGN_CHANGE`. Keputusan `RWI-DEC-172` menetapkan penggabungan enam sub-menu V1 ke dalam tab Catatan Keperawatan. Namun saat implementasi task `FE-RWI-180`, layout lama dari tab naratif dibiarkan utuh di atas dan sub-panel baru langsung diselipkan di bawahnya tanpa restrukturisasi layout wadah (*shell wrapper*).

**Dampak nyata.**
Perawat jaga di Ruang HCU atau Dahlia yang menggunakan layar beresolusi 1366x768 atau 1920x1080 harus terus-menerus melakukan scroll ke bawah untuk sekadar melihat riwayat pembacaan cairan selang WSD atau daftar diet pasien. Dua kartu header ini menyita sekitar 280–320 piksel ruang vertikal.

**Rekomendasi.**
Hapuskan kartu header luar yang redundan. Jadikan wadah utama Catatan Keperawatan sebagai bilah navigasi sub-menu yang ramping (*compact sub-nav bar*) yang terpadu dengan aksi cepat naratif CPPT, sehingga hanya ada SATU kartu kerja aktif yang tampil di layar.

---

### ISS-KEP-009-02 — Sub-Navigasi 6 Sub-Menu Memakai Pill Bootstrap Mentah dan Berbeda dari Template Obat & Alkes

| | |
| --- | --- |
| **No. laporan** | 1 (Butir 4: Ikuti Template pada Sub Menu Keperawatan Lain) |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source code dan tangkapan layar pengguna |
| **Perbaikan** | `FIX-KEP-009-01` |

**Apa yang terjadi.**
Navigasi enam sub-menu Catatan Keperawatan (Spooling Cairan, Observasi WSD, Sliding Scale, DPO, Pra-Operasi, Diet Medis) ditampilkan di dalam kotak abu-abu tebal (`bg-light border rounded`) dengan tombol pill Bootstrap (`nav nav-pills`). Tampilan ini terlihat asing, kaku, dan tidak serasi dengan sub-menu lain di ruang kerja rawat inap seperti tab *Obat & Alkes*.

**Kenapa terjadi.**
Pada `nursing-narrative-tab.jsx` baris 198–218, pengembang menulis struktur Bootstrap murni:

```jsx
// src/.../sections/nursing-care/tabs/nursing-narrative-tab.jsx:198-218
<div className="nav nav-pills gap-1 mb-3 p-1 bg-light rounded border" role="tablist">
  {NURSING_NOTE_SUB_MENUS.map((menu) => {
    const isActive = activeSubMenu === menu.key;
    return (
      <button
        key={menu.key}
        type="button"
        className={`nav-link btn-sm d-flex align-items-center gap-2 ${
          isActive ? "active fw-bold shadow-sm" : "text-secondary"
        }`}
...
```

Sementara itu, pada sub-menu *Obat & Alkes* (`nursing-medication-section.jsx` baris 82–97), navigasi sub-tab dirancang sangat bersih menggunakan tab underline horizontal modern:

```jsx
// src/.../sections/medication/nursing-medication-section.jsx:82-97
<div className={styles.medicationSubNavBar}>
  {MEDICATION_SUB_TABS.map((tab) => {
    const isActive = activeSubTab === tab.key;
    return (
      <button
        key={tab.key}
        type="button"
        className={`${styles.medicationSubNavBtn} ${isActive ? styles.medicationSubNavBtnActive : ""}`}
        onClick={() => setActiveSubTab(tab.key)}
...
```

CSS `.medicationSubNavBar` (`nursing-workspace.module.css:6699-6732`) menggunakan border bawah 2px halus (`#e2e8f0`), warna teks slate (`#64748b`), dan aksen biru/teal aktif (`#0284c7` dengan background `#f0f9ff` dan border-bottom aktif).

**Apakah ini menyimpang dari desain?**
`DESIGN_CHANGE`. Prinsip arsitektur frontend Quilvian pada `rules/frontend/ui-consistency-checklist.md` menuntut konsistensi komponen antar tab di dalam modul yang sama.

**Dampak nyata.**
Pengalaman pengguna terfragmentasi; perawat merasakan transisi visual yang melompat dan tidak konsisten saat berpindah dari tab *Obat & Alkes* ke *Catatan Keperawatan*.

**Rekomendasi.**
Ubah navigasi sub-menu Catatan Keperawatan agar mengadopsi struktur sub-navbar horizontal yang setara dengan `.medicationSubNavBar` atau sub-tab segmented modern yang terintegrasi dengan header, lengkap dengan ikon klinis dan indikator tab aktif yang elegan.

---

### ISS-KEP-009-03 — Penggunaan Alert dan Tombol Kelas Bootstrap Mentah alih-alih Base Component Quilvian

| | |
| --- | --- |
| **No. laporan** | 1 (Butir 5: Ikuti Base Component) |
| **Jenis** | `RULE_VIOLATION` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI di source code |
| **Perbaikan** | `FIX-KEP-009-02` |

**Apa yang terjadi.**
Peringatan aturan keselamatan klinis RWI-AC-206 ditulis menggunakan elemen HTML `<div className="alert alert-info py-2 px-3 small d-flex align-items-center mb-3">`, bukan menggunakan komponen standar `ClinicalSafetyAlert` atau `InformationAlert`. Demikian juga pada pesan sukses/gagal di sub-panel WSD dan Spooling yang masih memakai `alert alert-danger` dan `alert alert-success`.

**Kenapa terjadi.**
Pengembang mengandalkan kelas utilitas Bootstrap bawaan untuk mempercepat pengerjaan tanpa memeriksa katalog base component di `src/components/ui/clinical-workspace/ClinicalSafetyAlert.jsx` dan `src/components/features/base-features/information-alert.jsx`.

Potongan kode pelanggaran pada `nursing-narrative-tab.jsx` baris 152–158:
```jsx
// src/.../sections/nursing-care/tabs/nursing-narrative-tab.jsx:152-158
<div className="alert alert-info py-2 px-3 small d-flex align-items-center mb-3" role="status">
  <FaInfoCircle className="me-2 flex-shrink-0" />
  <div>
    <strong>Aturan Keselamatan:</strong> Angka cairan atau obat yang diketik di narasi catatan keperawatan <em>tidak mengubah total maupun MAR</em>. Pencatatan intake/output cairan dilakukan di Spooling Cairan / Pengawasan Harian, dan pemberian obat dilakukan di Daftar Pemberian Obat (DPO).
  </div>
</div>
```

**Apakah ini menyimpang dari desain?**
`RULE_VIOLATION`. Melanggar aturan tata kelola frontend `rules/frontend/base-component-catalog.md` dan `AGENTS.md` ("Jangan memakai `.btn` atau `.btn-primary` atau komponen native peramban/Bootstrap untuk fitur baru jika base component tersedia").

**Dampak nyata.**
Komponen tidak mewarisi token warna tema rumah sakit, sulit diuji dengan selektor standar testing library (`data-testid="clinical-safety-alert"`), dan merusak standarisasi aksesibilitas ARIA.

**Rekomendasi.**
Ganti seluruh wadah alert mentah dengan `ClinicalSafetyAlert` bertone `info` untuk aturan keselamatan klinis, atau `InformationAlert` untuk notifikasi umpan balik form.

---

### ISS-KEP-009-04 — Penempatan Tombol Catatan Naratif CPPT Memotong Fokus Klinis Sub-menu Terpilih

| | |
| --- | --- |
| **No. laporan** | 1 (Butir 2: User Friendly, Butir 1: Posisi Rapi) |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source code dan tangkapan layar pengguna |
| **Perbaikan** | `FIX-KEP-009-03` |

**Apa yang terjadi.**
Pada kartu paling atas, terdapat tombol utama `[+ Tulis Catatan Naratif CPPT]` dan `[Lihat Riwayat Naratif (0)]`. Ketika tombol "Lihat Riwayat Naratif" ditekan, panel accordion putih besar terbuka tepat di atas sub-menu, sehingga mendorong enam sub-menu dan area kerja operasional terlempar jauh ke bawah layar. Selain itu, perawat yang membuka sub-menu WSD atau Diet Medis merasa rancu apakah tombol di atas tersebut ditujukan untuk mencatat WSD atau CPPT umum.

**Kenapa terjadi.**
Berdasarkan keputusan `RWI-DEC-172` butir 4 dan `03-frontend-architecture.md` baris 732, catatan naratif perawat sebenarnya disimpan sebagai entri CPPT terpadu dan tempat membaca/menulis utamanya adalah pada tab *Catatan Terintegrasi* atau *SOAP* dengan saringan jenis `NursingNarrative`. Namun untuk kenyamanan transisi dari alur kerja lama, kedua tombol tersebut tetap diletakkan di tab ini tetapi diposisikan secara dominan di header statis.

**Apakah ini menyimpang dari desain?**
`DESIGN_CHANGE`. Dokumen `03-frontend-architecture.md` baris 732 secara tegas mencatat:
> *"Narasi perawat: Tidak di sini. Tautan 'Tulis catatan naratif' membuka Catatan Terintegrasi dengan saringan 'Naratif Keperawatan'"*.

Menempatkan panel riwayat naratif CPPT yang dapat dibuka-tutup di bagian atas sub-menu terbukti mengacaukan tata letak visual sub-menu yang sedang aktif.

**Dampak nyata.**
Saat perawat sedang tergesa-gesa mencatat volume selang dada WSD pasien pasca-operasi, mereka terdistraksi oleh riwayat catatan naratif atau bahkan salah menekan tombol naratif alih-alih tombol "+ Daftarkan Selang" / "+ Catat Pembacaan Shift".

**Rekomendasi.**
Kemas aksi catatan naratif CPPT sebagai tombol aksi cepat (*quick action button*) yang kompak di pojok kanan bilah navigasi sub-menu atau diintegrasikan sebagai drawer/modal terpisah tanpa merusak ruang kerja vertikal sub-panel yang sedang aktif.

---

### ISS-KEP-009-05 — Label Teknis Internal Rekayasa "(V1)" Tercantum pada Judul Layar Klinis

| | |
| --- | --- |
| **No. laporan** | 1 (Butir 3: Tampilan Menarik Dan Bagus, Butir 2: User Friendly) |
| **Jenis** | `RULE_VIOLATION` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI di source code (`nursing-narrative-tab.jsx:120`) |
| **Perbaikan** | `FIX-KEP-009-04` |

**Apa yang terjadi.**
Judul pada kartu teratas tertulis: `Catatan Keperawatan (V1)`.

**Kenapa terjadi.**
Teks `(V1)` adalah penanda internal dari roadmap finishing (`PRD-RWI-FINISHING-001`) untuk membedakan struktur enam sub-menu V1 dengan versi blueprint sebelumnya. String ini terbawa ke dalam kode JSX produksi:

```jsx
// src/.../sections/nursing-care/tabs/nursing-narrative-tab.jsx:120
<h3 className={styles.tabTitle}>Catatan Keperawatan (V1)</h3>
```

**Apakah ini menyimpang dari desain?**
`RULE_VIOLATION`. Aturan output dan panduan UI Quilvian melarang penayangan jargon internal rekayasa, nomor versi modul mentah, atau kode task di hadapan pengguna klinis.

**Dampak nyata.**
Tampilan aplikasi terkesan belum tuntas (*unfinished software*) dan menimbulkan pertanyaan dari pihak manajemen rumah sakit dan akreditasi KARS mengenai arti kode "(V1)" tersebut.

**Rekomendasi.**
Hapus string `(V1)` dari judul tampilan. Judul resmi adalah **Catatan Keperawatan**.

---

### ISS-KEP-009-T1 — Kontrol Internal Sub-Panel Memakai Button HTML Native dan Badge Bootstrap

| | |
| --- | --- |
| **No. laporan** | — (Temuan Tambahan Penelusuran Source Code) |
| **Jenis** | `RULE_VIOLATION` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI di source code |
| **Perbaikan** | `FIX-KEP-009-05` |
| **Kenapa dicantumkan** | Terlihat pada source code sub-panel yang dibuka (`wsd-observation-panel.jsx:340` dan `nursing-dpo-panel.jsx:71`) yang memicu inkonsistensi styling kontrol saat sub-menu dibuka. |

**Apa yang terjadi.**
Pada panel WSD, pemilihan selang menggunakan tag button HTML biasa dengan kelas Bootstrap:
`<button className="btn btn-sm btn-outline-secondary">` dan `<span className="badge bg-success">Aktif</span>`. Pada panel DPO, sub-tab internal juga memakai pola serupa (`btn btn-sm btn-primary`).

**Rekomendasi.**
Standardisasi tombol menggunakan `BaseButton` (`variant="outline"` / `variant="primary"`, `size="sm"`) dan status selang memakai `StatusBadge` resmi (`variant="success"` / `variant="secondary"`).

---

## 5. Tanya-Jawab Pelapor

> **T:** Mengapa tampilan catatan keperawatan saat ini terlihat bertumpuk dan tidak rapi?
>
> **J:** Karena saat ini ada dua kartu header besar yang dipasang sekaligus: kartu luar "Catatan Keperawatan" dan kartu dalam milik masing-masing sub-menu (seperti Observasi WSD). Selain itu, navigasi sub-menu dibungkus kotak abu-abu tebal terpisah yang memotong alur mata. Solusinya adalah menyatukan navigasi ke dalam bilah sub-tab horizontal modern (seperti pada sub-menu *Obat & Alkes*) dan menghapus kartu header luar yang berulang.

> **T:** Bagaimana rekomendasi UI/UX yang ideal agar memenuhi 5 kriteria yang diminta?
>
> **J:** Rekomendasi desain baru terdiri dari:
> 1. **Posisi Rapi:** Struktur satu tingkat (single-layer hierarchy). Hilangkan kartu luar; letakkan sub-navigasi tepat di bawah tab utama Asuhan Keperawatan.
> 2. **User Friendly:** Perawat langsung melihat kartu kerja dan data klinis yang relevan tanpa harus scroll melewati dua header. Hapus label teknis `(V1)`.
> 3. **Tampilan Menarik:** Gunakan sub-navbar dengan garis aksen aktif teal/cyan khas Quilvian, tipografi modern, dan spasi yang proporsional.
> 4. **Ikuti Template Sub Menu Lain:** Adopsi pola tata letak sub-navigasi dari sub-menu *Obat & Alkes* (`.medicationSubNavBar`).
> 5. **Ikuti Base Component:** Gunakan `ClinicalSafetyAlert` untuk banner aturan keselamatan, `BaseButton` untuk seluruh tombol aksi, dan `StatusBadge` untuk penanda status klinis.

---

## 6. Temuan Tambahan

Seluruh temuan tambahan (`ISS-KEP-009-T1`) telah diuraikan pada bagian 4 dan didaftarkan pada tabel perbaikan `FIX-KEP-009-05`. Tidak ada temuan yang berada di luar batas sub-modul `keperawatan`.

---

## 7. Pertanyaan dan Keputusan yang Dibutuhkan

### Untuk Pelapor / Pengguna

| No | Pertanyaan | Kenapa ditanyakan | Dampak bila belum dijawab |
| :---: | --- | --- | --- |
| 1 | Apakah tombol aksi "Tulis Catatan Naratif CPPT" tetap perlu disediakan sebagai tombol aksi cepat di pojok kanan sub-navbar Catatan Keperawatan, atau cukup diarahkan sepenuhnya melalui tab Catatan Terintegrasi / SOAP? | Dokumen arsitektur `03-frontend-architecture.md:732` menyebutkan narasi ditulis di Catatan Terintegrasi, namun tombol cepat saat ini sudah ada di antarmuka. | Bila tetap dibutuhkan, tombol diletakkan rapi di pojok kanan bilah navigasi tanpa memakan ruang kartu vertikal (Opsi Rekomendasi). Bila tidak, tombol dapat dihilangkan agar antarmuka semakin bersih. |

### Untuk Pemilik Modul (Product Owner / Domain Lead)

| No | Keputusan yang dibutuhkan | Rekomendasi | Opsi lain | Dampak bila ditolak |
| :---: | --- | --- | --- | --- |
| K-01 | Pengesahan penyatuan header dan penghapusan kartu luar redundan pada `nursing-narrative-tab.jsx` | **Setujui Opsi A**: Hapus kartu header luar, jadikan sub-navbar satu tingkat mengikuti pola `Obat & Alkes`. | Opsi B: Pertahankan kartu luar tetapi diperkecil (tetap menyisakan sedikit tumpukan). | Tampilan tetap bertumpuk dua tingkat dan memboroskan ruang monitor perawat. |

---

## 8. Catatan Pola

1. **Pola "Container Wrapper Redundant":** Ketika fitur baru digabungkan ke tab yang sudah ada (seperti saat 6 sub-menu V1 dimasukkan ke dalam tab naratif keperawatan pada task `FE-RWI-180`), pengembang cenderung membungkus komponen baru di bawah komponen lama tanpa membersihkan layout wadahnya. Pola ini serupa dengan temuan layout lama pada pendaftaran IGD dan rawat inap.
2. **Pencegahan:** Setiap pembuatan sub-menu majemuk wajib mengikuti kontrak `rules/frontend/page-composition-patterns.md` di mana sub-navigasi menjadi penentu tunggal kartu kerja di bawahnya tanpa kartu pengantar ganda.

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :---: | --- | --- |
| 2026-10-06 | Dokumen issue dibuat berdasarkan laporan pengguna; 5 temuan utama dan 1 temuan tambahan dirinci dengan status TERBUKA. | `diagnose-module-issue` (Antigravity) |
| 2026-10-06 | Seluruh 5 butir perbaikan selesai diimplementasikan dan diverifikasi (status SELESAI). | `diagnose-module-issue` (Antigravity) |
