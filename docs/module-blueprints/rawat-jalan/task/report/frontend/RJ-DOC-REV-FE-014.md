# Laporan Perubahan Frontend — `RJ-DOC-REV-FE-014`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-FE-014` |
| Judul | Scan kartu penjamin di Pendaftaran Rawat Jalan |
| Slice | Amendment SK — scan kartu penjamin pada Pendaftaran Rawat Jalan |
| Roadmap | [`roadmap/doctor-consultation-roadmap.md`](../../../roadmap/doctor-consultation-roadmap.md) §15.2 |
| Trace | `RJ-DOC-DEC-040`..`044` ([00-interview-decisions.md](../../../00-interview-decisions.md), *Amendment SK*) |
| Contract version | Delta endpoint penjamin pasien dari [`RJ-DOC-REV-BE-013`](../backend/RJ-DOC-REV-BE-013.md) §4 dan [`RJ-DOC-REV-BE-014`](../backend/RJ-DOC-REV-BE-014.md) §4 (source backend saat ini) |
| Wewenang UI | `RJ-DOC-DEC-040`: kolom *Kartu* sebelum Status, preview di *Penjamin Dipilih*, field scan di modal *Daftarkan Penjamin Baru*; hanya Rawat Jalan |
| Dependency | `RJ-DOC-REV-BE-013` ✅, `RJ-DOC-REV-BE-014` ✅ |
| Klasifikasi | `MEDIUM` (skor 7: satu repository 0, berkas diperiksa 2, berkas diubah 2, logika 1, kontrak API 1, database 0, keamanan 0, UI 1) |
| Task mode | `CROSS-REPO MODE` (`RJ-DOC-DEC-043`); task ini hanya menulis frontend, ditambah laporan dan tanda status ini |
| Target tulis | Frontend `src/**` modul pendaftaran (lihat §3.2) |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `b241468be` (branch `sukmagpV2`, perubahan belum di-commit) |
| Commit backend yang dijadikan rujukan | `b99fc41e` + perubahan `BE-013`/`BE-014` yang belum di-commit |
| Tanggal | 6 Oktober 2026 |
| Status | Selesai — ESLint, build, unit test terkait, dan uji layar `15/15 PASS` |

---

## 1. Keadaan yang ditemukan di awal

- Langkah *Metode Pembayaran* Pendaftaran Rawat Jalan memakai komponen IGD: `PaymentMethodStep`, `PatientPayerTable`, dan `PatientPayerModal`. Tidak ada cara memindai kartu penjamin.
- Perubahan sementara sebelumnya dibuat tanpa Skill ini: tombol scan di header tabel dan preview yang hanya hidup di memori halaman. Perubahan itu menulis warna literal di CSS dan memakai `<button>` mentah. Seluruhnya dibuang (dipulihkan ke `HEAD`), lalu task ini mengerjakan ulang dari awal.
- Backend sebelumnya tidak menyimpan gambar kartu. Setelah `BE-013`/`BE-014`, list penjamin membawa `cardImagePath`, dan tersedia `PATCH {id}/card-image`.
- `resolvePublicFileUrl` (`src/utils/shared/public-file-url-utils.js`) sudah menerjemahkan `/uploads/...` ke origin API. Utilitas itu dipakai ulang.

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** petugas pendaftaran Rawat Jalan, pada langkah 3 *Metode Pembayaran* dengan kategori Asuransi/Penjamin.

**Penjamin yang sudah terdaftar, kartu belum pernah dipindai:**

1. Tabel *Daftar Asuransi / Penjamin Pasien* menampilkan kolom **Kartu** tepat sebelum **Status**. Baris tanpa kartu berisi tombol **Scan Kartu**.
2. Petugas memasukkan kartu ke scanner Plustek, lalu menekan **Scan Kartu**. Baris itu ikut terpilih, dan tombol berubah menjadi "Memindai...".
3. Gambar dari scanner langsung disimpan ke backend. Tombol baris berubah menjadi **Lihat Kartu** tanpa muat ulang. Panel **Penjamin Dipilih** menampilkan gambar kartunya.

**Penjamin yang kartunya sudah tersimpan (kunjungan berikutnya):**

1. Baris langsung menampilkan **Lihat Kartu**, dan panel *Penjamin Dipilih* langsung menampilkan kartu. Tidak perlu scan ulang.
2. **Lihat Kartu** (atau **Perbesar** di panel) membuka preview besar beserta nomor kartu/polis. Di sana ada tombol **Scan Ulang** untuk mengganti kartu, dan **Tutup**.

**Penjamin baru:** di modal *Daftarkan Penjamin*, setelah provider dipilih, muncul field **Kartu Penjamin (Opsional)**.
1. Petugas menekan **Scan Kartu**, dan preview tampil.
2. Bila perlu, petugas menekan **Scan Ulang** atau **Hapus**.
3. Saat **Simpan Penjamin**, gambar ikut terkirim. Baris baru langsung menampilkan **Lihat Kartu**.

**Jalur tidak normal:**

| Keadaan | Yang dilihat petugas |
| --- | --- |
| Agent Plustek tidak berjalan | Peringatan "Kartu penjamin belum tersimpan — Plustek Scanner Agent tidak dapat diakses. Pastikan agent berjalan di komputer ini, lalu coba lagi."; tombol tetap **Scan Kartu** |
| Scanner tidak terdeteksi / sedang dipakai | Peringatan "Scanner Plustek belum terdeteksi…" / "Scanner sedang digunakan oleh proses lain." |
| Backend menolak (misalnya gambar > 5 MB) | Pesan backend tampil apa adanya, contoh "Ukuran gambar kartu melebihi batas 5 MB."; tombol tetap **Scan Kartu** |
| Sedang memindai | Tombol scan lain dinonaktifkan supaya hanya satu scan berjalan |

**Pendaftaran IGD** memakai komponen yang sama tetapi tidak mengirim prop baru. Tabel, panel, dan modal IGD tidak berubah.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`/`CLAUDE.md` frontend; `rules/frontend/` (frontend-architecture, base-component-catalog, base-component-decision-gate, design-tokens, page-composition-patterns, ui-consistency-checklist, test-policy, REPORT_TEMPLATE); `payment-method-step.jsx`, `patient-payer-table.jsx`, `emergency-patient-payer-modal.jsx`, `emergency-registration-fields.jsx`, `outpatient-registration-page.jsx`, `use-outpatient-registration.js`, `outpatient-registration-slice.jsx`, `emergency-registration.service.js`, `emergency-registration.utils.js`, `emergency-registration.constants.js`, `plustek-scanner-agent.js`, `use-plustek-ktp-scanner.js`, `base-button.jsx`, `confirm-modal.jsx`, `data-table.jsx`, `public-file-url-utils.js`, `patient-info.utils.js`; controller/DTO backend `PatientInsurance*` dan `PatientCompanyGuarantor*`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/health-services/registration-management/emergency-registration/use-plustek-payer-card-scanner.js` (baru) | Memindai kartu lewat agent Plustek (`/status` lalu `/scanner/scan` profil KTP, tanpa OCR) dan mengembalikan data URL gambar. Galat jaringan agent diterjemahkan ke pesan yang dapat dipahami |
| `src/lib/services/health-services/registration-management/emergency-registration.service.js` | `updatePatientInsuranceCardImage` dan `updatePatientCompanyGuarantorCardImage` → `PATCH {base}/{id}/card-image` |
| `src/lib/state/slice/health-services/registration-management/outpatient-registration-slice.jsx` | Reducer `setOutpatientPayerCardImage` memperbarui `cardImagePath` satu penjamin |
| `src/lib/hooks/health-services/registration-management/outpatient-registration/use-outpatient-registration.js` | `handleScanPayerCard` (scan, simpan, perbarui state) dan objek `payerCard` (scanning, payer yang sedang disimpan, error, `scanCardImage`, `clearError`) |
| `src/utils/health-services/registration-management/emergency-management/emergency-registration.utils.js` | `cardImageBase64` pada payload create asuransi dan penjamin perusahaan (`null` bila kosong) |
| `src/lib/constants/health-services/registration-management/emergency-management/emergency-registration.constants.js` | `cardImageBase64: ""` pada `DEFAULT_PAYER_FORM_VALUES` |
| `src/components/view/health-services/registration-management/emergency-registration/patient-payer-table.jsx` | Prop opsional `cardColumn`: kolom **Kartu** (`BaseButton` Scan Kartu / Lihat Kartu) sebelum Status |
| `src/components/view/health-services/registration-management/emergency-registration/payment-method-step.jsx` | Prop opsional `payerCard`/`onScanPayerCard`: kolom kartu, peringatan scan, preview kartu di *Penjamin Dipilih* (Perbesar / Scan Ulang / Scan Kartu), modal preview `ConfirmModal`, dan meneruskan scanner ke modal |
| `src/components/view/health-services/registration-management/emergency-registration/emergency-patient-payer-modal.jsx` | Prop opsional `cardScanner`: field **Kartu Penjamin (Opsional)** dengan preview, Scan Kartu/Scan Ulang/Hapus; nilai dibersihkan saat jenis/provider diganti; tombol simpan dinonaktifkan selama memindai |
| `src/components/view/health-services/registration-management/outpatient-registration/outpatient-registration-page.jsx` | Meneruskan `payerCard` dan `handleScanPayerCard` ke `PaymentMethodStep` |
| `src/style/health-services/registration-management/emergency-management/emergency-registration.module.css` | Class `payerCard*` khusus elemen baru, seluruhnya memakai token |

### 3.3 Kepatuhan arsitektur frontend

- **Alur dependensi:** view → hook (`use-outpatient-registration`, `use-plustek-payer-card-scanner`) → service (`InstanceAxios`) / slice → backend. View tidak memanggil Axios.
- **Agent:** scanner diakses lewat `plustekScannerAgent` yang sudah ada.
- **Opt-in per layar:** fitur bergantung pada prop opsional. IGD tidak mengirimnya, sehingga perilaku IGD tidak berubah.
- **Pola baru:** tidak ada.

#### Gerbang keputusan base component

`UI GATE: 7 elemen — REUSE 4, EXTEND 0, COMPOSE 2, WRAP 1, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| Kolom Kartu di tabel penjamin | `DataTable` | `base-features/data-table.jsx`, kolom `render` | REUSE | Kolom baru lewat `columns` |
| Tombol Scan Kartu / Lihat Kartu / Perbesar / Scan Ulang / Hapus | `BaseButton` | `base-features/base-button.jsx` (`size="sm"`, `variant`, `loading`, `loadingLabel`) | REUSE | `BaseButton` |
| Preview besar kartu | `ConfirmModal` | `base-features/confirm-modal.jsx` (`children`, `variant="info"`, `showIcon`, `confirmLabel`, `cancelLabel`, `size`) | REUSE | Konfirmasi = Scan Ulang, batal = Tutup |
| URL gambar `/uploads` | `resolvePublicFileUrl` | `utils/shared/public-file-url-utils.js`, dipakai `patient-info.utils.js` | REUSE | Dipakai apa adanya |
| Preview di panel *Penjamin Dipilih* | `payerDetailCard` existing + `Image` + `BaseButton` | `payment-method-step.jsx` | COMPOSE | Bagian baru di bawah `detailList` |
| Field kartu di modal | `simplePayerForm` existing + `Image` + `BaseButton` | `emergency-patient-payer-modal.jsx` | COMPOSE | Bagian baru di bawah Masa Aktif |
| Pesan gagal scan/simpan | `EmergencyInlineAlert` (domain) vs `InformationAlert` (base) | `emergency-registration-fields.jsx` | WRAP | `EmergencyInlineAlert` |

Pilihan untuk elemen bukan `REUSE` (diputuskan dengan rekomendasi karena tidak ada `NEW` atau `EXTEND` yang mengubah default):

- **Preview di panel / field di modal**
  - **A. COMPOSE di dalam kartu yang sudah ada — Rekomendasi, dijalankan.** Konsisten dengan panel dan modal IGD/RJ yang ada. Tanpa perubahan base. Biaya kecil.
  - **B. `BaseDetailCard` terpisah untuk kartu.** Panel menjadi dua kartu bertumpuk yang menyimpang dari tata letak langkah pembayaran.
- **Pesan gagal**
  - **A. `EmergencyInlineAlert` — Rekomendasi, dijalankan.** Seluruh langkah pembayaran sudah memakainya, jadi tampilan pesan seragam di satu layar.
  - **B. `InformationAlert`.** Sesuai katalog base, tetapi gaya peringatan di layar yang sama menjadi dua macam.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tabel penjamin memakai loading `DataTable` existing ("Mengambil data penjamin..."); tombol scan "Memindai..." dan nonaktif selama proses |
| Kosong | Penjamin tanpa kartu: tombol **Scan Kartu**, dan panel "Kartu penjamin belum dipindai." beserta tombol **Scan Kartu**; modal: "Pindai kartu polis atau asuransi pasien dengan scanner Plustek." |
| Gagal | Peringatan "Kartu penjamin belum tersimpan" berisi pesan agent/backend; petugas dapat menekan tombol scan lagi. Pesan dibersihkan saat modal penjamin baru dibuka |
| Tanpa hak akses | `PATCH card-image` butuh `PatientInsurance : Update` / `PatientCompanyGuarantor : Update`. Penolakan `403` tampil sebagai pesan backend di peringatan yang sama |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Patient Management / Master Data / Patient Insurance

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/patient-management/master-data/patient-insurances` | Daftar asuransi pasien; kini membawa `cardImagePath` | Tidak berubah |
| `POST` | `/v1/health-services/patient-management/master-data/patient-insurances` | Simpan asuransi baru beserta `cardImageBase64` | Tidak berubah |
| `PATCH` | `/v1/health-services/patient-management/master-data/patient-insurances/{id}/card-image` | Simpan/ganti kartu hasil scan | `PatientInsurance : Update` |

#### Health Services / Patient Management / Master Data / Patient Company Guarantor

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/patient-management/master-data/patient-company-guarantors` | Daftar penjamin perusahaan; kini membawa `cardImagePath` | Tidak berubah |
| `POST` | `/v1/health-services/patient-management/master-data/patient-company-guarantors` | Simpan penjamin perusahaan baru beserta `cardImageBase64` | Tidak berubah |
| `PATCH` | `/v1/health-services/patient-management/master-data/patient-company-guarantors/{id}/card-image` | Simpan/ganti kartu hasil scan | `PatientCompanyGuarantor : Update` |

Agent lokal Plustek (bukan backend): `GET /status`, `POST /scanner/scan`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` × 11 berkas yang berubah | `0 error`, 4 warning — semuanya pada kode lama yang tidak diubah (`setMounted` dan reset modal di `emergency-patient-payer-modal.jsx`, dua efek halaman di `patient-payer-table.jsx`) | `PASS` | Keluaran `eslint -f json` |
| `npm run build` | `PASS` (dijalankan ulang setelah perbaikan pesan agent); route `outpatient-registration` terbentuk | `PASS` | `fe014_build2.log` |
| `node --test` tiga berkas unit yang mengimpor modul pendaftaran (`emergency-registration-existing-visit`, `emergency-registration-payload`, `emergency-visit-status`) | `46/46 pass` | `PASS` | Keluaran perintah |
| `npm run test:unit` penuh | `2443/2451 pass`, 8 gagal di laboratorium, hemodialisa, tindakan, bank darah, dan resep; tidak satu pun mengimpor berkas yang diubah | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah; pencarian import pada `tests/unit` |
| Grep anti-regresi pada diff (warna literal, `!important`, `<button>`/`.btn`, `<table>`, `fw-*`/`fs-*`) | Tidak ada temuan | `PASS` | Keluaran grep |
| `AUTOMATED TEST` | `npm run test:unit — FAIL (8 kegagalan di modul lain, lihat di atas)`; test baru `SKIPPED (opsional)` — logika baru berupa wiring hook dan komposisi view | — | — |

### Uji layar

Lingkungan:
- **Frontend:** build produksi (`node .next/standalone/server.js`) di `localhost:3100`. Server dev pemilik (3000) dan backend dev (7184) tidak berjalan saat uji.
- **Backend:** API `https://localhost:7184` dialihkan Playwright ke backend uji `http://localhost:5199` (kode `BE-013`/`BE-014`, DB `QuilvianNewDevSukma`).
- **Agent Plustek:** `127.0.0.1:9100` **ditiru** oleh Playwright dengan gambar PNG 320×200, karena tidak ada scanner fisik.
- **Data uji:** pasien `KSKTEST-RM-07`, dua asuransi dan satu penjamin perusahaan uji yang dibuat lewat API lalu dihapus di akhir.
- **Script:** `ui_fe014.cjs`, hasil `ui_fe014_result.json` di scratchpad.

| ID | Skenario | Hasil | Status |
| --- | --- | --- | --- |
| U1 | Urutan kolom tabel | `… BERLAKU S.D. \| KARTU \| STATUS` | `PASS` |
| U2 | Asuransi tanpa kartu | Tombol **Scan Kartu** | `PASS` |
| U3 | Agent mati, tekan Scan Kartu | Peringatan "Plustek Scanner Agent tidak dapat diakses…", tombol tetap, tanpa `PATCH` | `PASS` (run pertama menemukan pesan mentah "Failed to fetch", lalu diperbaiki) |
| U4 | Scan berhasil | `PATCH patient-insurances/{id}/card-image` `200` dengan `data:image/png;base64,…`; tombol jadi **Lihat Kartu** tanpa muat ulang | `PASS` |
| U5 | Panel *Penjamin Dipilih* | Gambar dari `https://localhost:7184/uploads/patient-payer-cards/…jpg` termuat | `PASS` |
| U6 | **Lihat Kartu** | Preview "Kartu BPJS Kesehatan" dengan gambar, **Scan Ulang**, **Tutup** | `PASS` |
| U7 | **Scan Ulang** di preview | Path kartu berganti | `PASS` |
| U8 | **Tutup** | Preview tertutup | `PASS` |
| U9 | Penjamin perusahaan | `PATCH patient-company-guarantors/{id}/card-image` `200`; **Lihat Kartu** | `PASS` |
| U10 | Backend menolak `400` (ditiru) | Pesan "Ukuran gambar kartu melebihi batas 5 MB." tampil; tombol tetap **Scan Kartu** | `PASS` |
| U11 | Buka modal penjamin baru setelah gagal | Pesan lama tidak terbawa | `PASS` |
| U12 | Modal: pilih provider, isi nomor dan masa aktif, Scan Kartu | Field *Kartu Penjamin (Opsional)*, preview tampil, tombol jadi **Scan Ulang** | `PASS` |
| U13 | Simpan penjamin baru | `POST` membawa `cardImageBase64`; response `cardImagePath` `/uploads/patient-payer-cards/…`; baris baru **Lihat Kartu** | `PASS` |
| U14 | Buka ulang halaman dan ulangi alur | Asuransi A dan perusahaan C langsung **Lihat Kartu**; B tetap **Scan Kartu** | `PASS` |
| U15 | Pilih A setelah buka ulang | Preview kartu tampil, agent tidak dipanggil (0 scan) | `PASS` |

Hasil: **15/15 `PASS`**. Screenshot: `fe014-u2-table.png`, `fe014-u5-panel.png`, `fe014-u6-modal.png`, `fe014-u12-modal.png`, `fe014-u15-reload.png`. Screenshot `u6` tertangkap saat animasi fade `ConfirmModal`.

**Keadaan sesudah run:** empat penjamin uji dihapus lewat `DELETE /admin/{id}` (`200`) dan tersisa sebagai baris soft-delete. File kartu uji tersimpan di storage backend uji (scratchpad). Server frontend uji di port 3100 masih berjalan sebagai background task sesi ini.

Uji manual: `PASS` lewat uji layar otomatis di peramban, dengan scanner **tiruan**. **Belum diuji dengan scanner Plustek fisik**: area pindai profil KTP untuk kartu non-standar dan kualitas gambar nyata belum terbukti.

**Tidak dijalankan:**
- **Layar IGD:** tidak diuji. Buktinya kode saja: `emergency-registration-page.jsx` tidak mengirim `payerCard`/`cardScanner`/`cardColumn`, sehingga semua cabang baru tidak dirender. Payload create IGD kini membawa `cardImageBase64: null`, yang diabaikan backend.
- **Uji akun tanpa hak `Update`:** tidak ada akun uji yang sesuai.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Penjamin tanpa kartu menampilkan *Scan Kartu*; setelah scan berhasil, tombol berubah menjadi *Lihat Kartu* tanpa muat ulang | Terpenuhi | U2, U4, U9 |
| 2. Penjamin yang kartunya sudah tersimpan langsung menampilkan *Lihat Kartu* dan preview setelah halaman dibuka ulang | Terpenuhi | U14, U15 |
| 3. Penjamin baru dengan kartu hasil scan tersimpan bersama gambar kartunya | Terpenuhi | U12, U13 |
| 4. Kegagalan scanner atau backend tampil sebagai pesan | Terpenuhi | U3, U10 |
| 5. Pendaftaran IGD tidak berubah | Terpenuhi (bukti kode) | §6 *Tidak dijalankan*; prop opsional tidak dikirim IGD |
| ESLint tanpa error baru; `npm run build` `PASS`; uji layar terhadap backend lokal | Terpenuhi | §6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 4 warning ESLint lama (bukan dari diff ini); 8 unit test gagal di modul lain |
| Masalah yang diketahui | (1) **Nomor kartu tidak terbaca otomatis.** OCR agent hanya mengenali dokumen identitas; petugas mencocokkan nomor kartu secara manual. (2) **Tabel bergulir horizontal.** Tabel penjamin kini lebih lebar satu kolom dan bergulir di dalam kontainernya pada lebar 1440 px. (3) **Payload perusahaan menyimpang.** Payload create penjamin perusahaan existing mengirim nama field yang tidak cocok dengan DTO backend (`participantName`, `benefitClass`, `expiryDate`, `cardPhotoBase64`); temuan lama di luar scope, tidak diubah |
| Dependency backend | `BE-013` ✅, `BE-014` ✅; perubahan backend belum di-commit dan migration `BE-014` baru diterapkan di `QuilvianNewDevSukma` |
| Perubahan sampingan | Perubahan sementara sebelum task ini (tombol header, preview sesi, CSS berwarna literal) dipulihkan ke `HEAD` pada `payment-method-step.jsx`, `outpatient-registration-page.jsx`, dan CSS, lalu ditulis ulang |
| Interupsi | Sesi terputus sekali di tengah pengerjaan; dilanjutkan dari working tree yang diverifikasi lewat `git status`/`git diff` |
| Status Git | `M` `emergency-patient-payer-modal.jsx`, `patient-payer-table.jsx`, `payment-method-step.jsx`, `outpatient-registration-page.jsx`, `emergency-registration.constants.js`, `use-outpatient-registration.js`, `emergency-registration.service.js`, `outpatient-registration-slice.jsx`, `emergency-registration.module.css`, `emergency-registration.utils.js`; `??` `use-plustek-payer-card-scanner.js`. Belum di-commit |
| Langkah berikutnya | Uji dengan scanner Plustek fisik; terapkan migration `BE-014` ke database lain bila disetujui |
