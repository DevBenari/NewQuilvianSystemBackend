# Laporan Perubahan Frontend — `FE-BKC-014`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BKC-014` |
| Judul | Dropdown tarif dan harga read-only pada form testing (pasien tunai) |
| Slice | Entri manual katalog tarif + coverage per item (`BKC-DEC-059`–`062`, amendment 2 September 2026) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` § `FE-BKC-014` (baris 193–207) |
| Trace | `BKC-DEC-059`, `061`; `FR-BKC-001`, `002`, `004`; `UAT-01`, `02` (`04-prd-to-mvp.md`) |
| Contract version | `POST catalog-charges` (`BIL-API-0.4` amendment, sudah diimplementasikan backend `BE-BKC-019`); `GET tariffs/options` (existing, reuse penuh, tidak ada perubahan kontrak) |
| Wewenang UI | Ganti field teks bebas "Nama Item/Layanan" jadi dropdown searchable katalog tarif; ganti field "Harga (Rp)" jadi read-only; ganti thunk submit. Tidak ada wewenang untuk mengubah komponen dasar (base component) — seluruhnya `REUSE` (lihat § 3.3) |
| Dependency | `BE-BKC-018` (konteks `ServiceUnitId`/`ClinicId`/`PatientClassId` pada `ActiveEncounterOptionResponse`) — selesai, terkonfirmasi lulus build. `BE-BKC-019` (`POST catalog-charges`) — selesai, build belum diverifikasi ulang pengguna (lihat `task/report/backend/BE-BKC-019.md`). `BKC-BLK-FE-001` — diverifikasi ulang sesi ini: `QuilvianSystemFrontendDev/AGENTS.md` ada dan terbaca, blocker resolved |
| Klasifikasi | `MEDIUM` — skor 5 (repo 0, berkas diperiksa 1, berkas diubah 1, logika bisnis 1, kontrak API 1, database 0, keamanan 0, UI/workflow 1) |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — `src/components/view/health-services/billing-management/billing-invoices/create-manual/`, `src/lib/hooks/health-services/billing-management/billing-invoices/`, `src/lib/state/slice/health-services/billing-management/`, `src/lib/hooks/select/health-service/` |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | (working tree belum di-commit — lihat § 8 Status Git) |
| Commit backend yang dijadikan rujukan | `fec3579` (`NewQuilvianSystemBackend`) |
| Tanggal | 3 September 2026 |
| Status | Source lengkap. **Terverifikasi hidup** (login sungguhan, data dev nyata) sampai menemukan bug backend pra-eksisting yang memblokir dropdown tarif selalu kosong — bug itu **sudah diperbaiki** pada task terpisah `BE-BKC-FIX-001` (source saja, belum di-build/restart). Verifikasi ulang end-to-end penuh (pilih tarif → harga terisi → submit → invoice tersimpan) **tertunda** sampai backend di-build ulang dan proses yang berjalan di-restart — lihat § 6 dan § 8 |

---

## 1. Keadaan yang ditemukan di awal

Form "Buat Invoice Manual" (alat bantu testing, `create-manual-invoice-view.jsx`) sudah ada dan
berfungsi, tapi field "Nama Item/Layanan" adalah teks bebas dan field "Harga (Rp)" bisa diketik
manual — submit-nya memakai jalur `ADHOC` (`addAdhocBillingCharge`, `POST from-source`) yang
menerima harga apa pun dari client. Ini cocok untuk data uji cepat, tapi tidak membuktikan alur
katalog tarif resmi (`BE-BKC-018`/`019`) yang baru selesai dikerjakan di backend — harga di sana
seharusnya **tidak bisa** diketik client sama sekali (`BIL-VAL-026`).

Dropdown tarif (resource `"tariffs"`) **sudah terdaftar** di `select-resource-registry.js` sebelum
task ini — sudah lengkap dengan endpoint, `labelKeys`, `priceKeys` — tapi belum pernah dipakai di
layar mana pun, dan `filterKeys`-nya belum mencakup `clinicId`/`patientClassId` (baru
`tariffCategoryId`/`serviceUnitId`), serta belum ada mekanisme disambiguasi label untuk tarif nama
sama berscope berbeda (`BKC-DEC-061`).

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna**: kasir atau penguji, pada halaman testing "Buat Invoice Manual" (bukan alur produksi
Rajal → Billing yang belum tersambung).

**Pemicu**: pengguna ingin membuat invoice pertama untuk sebuah kunjungan pasien tunai, memakai
harga resmi dari katalog tarif rumah sakit alih-alih mengetik manual.

**Langkah**:

1. Pengguna memilih kunjungan pasien di dropdown "Pasien / Kunjungan" (sudah ada, tidak berubah) —
   kartu "Konteks Kunjungan" di atas form menampilkan asal kunjungan, cara bayar, dan status
   invoice-nya.
2. **(Opsional)** Pengguna memilih "Kategori Tarif" untuk mempersempit pilihan di bawah — field ini
   murni penyaring lokal, nilainya tidak pernah dikirim ke server.
3. Pengguna membuka dropdown "Tarif Layanan" dan mengetik kata kunci (nama atau kode tarif). Hasil
   yang muncul sudah disaring server berdasarkan kategori yang dipilih **dan** konteks kunjungan
   (`ServiceUnitId`/`ClinicId`/`PatientClassId` milik kunjungan yang dipilih di langkah 1) — hanya
   tarif yang relevan untuk kunjungan itu yang tampil. Tarif dengan nama sama tapi scope berbeda
   dibedakan lewat label tambahan (mis. "Konsultasi · Poli Anak · Kelas 1").
4. Begitu satu tarif dipilih, field "Harga (Rp)" **otomatis terisi** dari `NormalPrice` tarif itu
   dan **tidak bisa diketik** — field ini murni tampilan, bukan input.
5. Mengganti kunjungan atau kategori setelah tarif terpilih **mengosongkan kembali** pilihan tarif
   dan harga — mencegah kasir submit tarif yang scope-nya sudah tidak relevan dengan konteks baru.
6. Pengguna mengisi Qty, menekan "Simpan Invoice" — request yang dikirim ke server **hanya**
   berisi `encounterId`, `tariffId`, dan `quantity`; tidak ada field harga sama sekali.
7. Setelah tersimpan, muncul dialog konfirmasi dan form dikosongkan untuk invoice berikutnya (sudah
   ada, tidak berubah).

**Jalur tidak normal**:
- Kunjungan/kategori belum dipilih → dropdown tarif tetap bisa dibuka dan dicari (kategori memang
  opsional), tapi hasilnya tidak tersaring konteks kunjungan bila kunjungan belum dipilih.
- Tidak ada tarif yang cocok dengan pencarian/filter → pesan bawaan komponen "Data tidak
  ditemukan" (belum dikustomisasi khusus tarif — lihat § 8 *Masalah yang diketahui*).
- Submit tanpa memilih tarif → validasi field "Tarif wajib dipilih." (sama pola dengan validasi
  field lain di form ini).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` § `FE-BKC-014`;
`QuilvianSystemFrontendDev/AGENTS.md` (verifikasi ulang `BKC-BLK-FE-001`);
`src/components/view/.../create-manual/create-manual-invoice-view.jsx`;
`src/lib/hooks/.../use-create-manual-invoice.js`;
`src/lib/state/slice/.../billing-invoice-slice.jsx` (pola `addBillingOtherCharge` sebagai
referensi struktur thunk baru); `src/components/features/base-features/
{base-editor-view.jsx, base-editor-form.jsx, base-editor-field.jsx, base-form-control.jsx,
filter-select.jsx}`; `src/lib/hooks/select/{use-select-resource.jsx, select-resource-registry.js,
health-service/health-service-select-resources.js}`; `Areas/HealthServices/MasterData/Controllers/
TariffController.cs` dan `Areas/HealthServices/MasterData/DTOs/TariffDtos.cs` (backend, dibaca
untuk memastikan bentuk response `/tariffs/options` — read-only, tidak diubah oleh task ini).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/select/health-service/health-service-select-resources.js` | Resource `tariffs` (sudah ada) diperluas: `filterKeys` menambah `clinicId`/`patientClassId`; `extendOption` baru menambahkan label scope (unit layanan/klinik/kelas pasien) ke label tarif untuk disambiguasi (`BKC-DEC-061`) |
| `src/lib/state/slice/health-services/billing-management/billing-invoice-slice.jsx` | Thunk baru `addCatalogCharge` (`POST catalog-charges`) — pola identik `addBillingOtherCharge`, request hanya `encounterId`/`tariffId`/`quantity`/`correlationId`/`causationId`, tanpa field harga |
| `src/lib/hooks/health-services/billing-management/billing-invoices/use-create-manual-invoice.js` | Form field `description`+`categoryId` (wajib) diganti `tariffId` (wajib)+`categoryId` (opsional, murni filter); `handleChange` diperluas membaca opsi tarif yang dipilih untuk mengisi `unitPrice`, dan mengosongkan `tariffId`/`unitPrice` saat kunjungan/kategori berganti; `handleSubmit` memanggil `addCatalogCharge` |
| `src/components/view/.../create-manual/create-manual-invoice-view.jsx` | Field "Nama Item/Layanan" diganti dropdown `tariffId` (`optionResource: "tariffs"`, `filters` dinamis dari kategori+konteks kunjungan); field "Harga (Rp)" jadi `disabled: true` tanpa `normalizeValue`; salinan teks header/deskripsi diperbarui mengikuti jalur katalog tarif |

### 3.3 Kepatuhan arsitektur frontend

Alur dependensi mengikuti pola yang sudah ada: View → Hook (`useCreateManualInvoice`) → Redux
thunk (`addCatalogCharge`) → `InstanceAxios`. Tidak ada state/HTTP/komponen paralel baru.

**Tabel keputusan base component:**

| Elemen | Keputusan | Alasan |
| --- | --- | --- |
| Dropdown "Tarif Layanan" | `REUSE` | `BaseSelectField` lewat `optionResource: "tariffs"` — mekanisme `useSelectResource`/registry yang sudah ada dan sudah dipakai modul lain (mis. `procedures`, `drugs`, `clinics`); entry `"tariffs"` sendiri sudah terdaftar sebelum task ini, hanya diperluas `filterKeys`+`extendOption` (konfigurasi data, bukan komponen baru) |
| Field "Harga (Rp)" read-only | `REUSE` | `BaseTextField` (number) yang sama persis dipakai field ini sebelumnya, hanya ditambah `disabled: true` dan dilepas `normalizeValue` — tidak ada perilaku default komponen yang berubah |
| Field "Kategori Tarif" (filter) | `REUSE` | Select yang sudah ada (`master-data-tariff-category-slice`), tidak diubah komponennya — hanya `required` dilepas dan salinan `description` diperbarui |

**`UI GATE`**: seluruh elemen berstatus `REUSE` murni — tidak ada `NEW` maupun `EXTEND` yang
mengubah perilaku default base component. Tidak ada keputusan pengguna yang perlu ditunggu sebelum
implementasi dimulai.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Teks "Menyiapkan pilihan data..." di bawah form (perilaku bawaan `BaseEditorForm` saat salah satu field sedang memuat opsi — tidak dikustomisasi khusus task ini) |
| Kosong | Pesan bawaan komponen "Data tidak ditemukan" pada dropdown tarif bila pencarian/filter tidak menghasilkan apa pun — **belum dikustomisasi** menjadi pesan yang lebih spesifik konteks tarif (lihat § 8) |
| Gagal | Bila endpoint `/tariffs/options` gagal, pesan errornya tampil langsung di area pilihan dropdown (`remote.error`, perilaku bawaan `useSelectResource` — sama dengan resource lain yang memakai mekanisme ini) |
| Tanpa hak akses | `NOT APPLICABLE` — mengandalkan penanganan 401/403 global yang sudah ada di seluruh aplikasi (interceptor `InstanceAxios`/route guard), tidak ada penanganan khusus per-field yang dibangun task ini |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Billing Management / Billing / Invoices

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/catalog-charges` | Submit form — menambah baris tagihan dari tarif yang dipilih | `BillingInvoice : Create` |

#### Health Services / Master Data / Tariff

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/options` | Isi dropdown "Tarif Layanan" — pencarian server-side, difilter kategori+konteks kunjungan | `Tariff : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` (4 berkas yang diubah) | Berhasil tanpa error | `PASS` | Keluaran perintah kosong (`--quiet`, tidak ada error) |
| Grep import-vs-pemakaian (`addAdhocBillingCharge` dihapus bersih, `addCatalogCharge` terpakai penuh) | Tidak ada referensi tertinggal | `PASS` | Pencarian menyeluruh pada berkas yang berubah |
| Render halaman langsung (login sungguhan `superadmin@admin.com`, data dev nyata, Playwright headless) | Halaman render sempurna: hero, kartu konteks kunjungan, seluruh field baru dengan salinan teks yang benar | `PASS` | Screenshot `01-loaded.png` |
| Pilih kunjungan pasien (dropdown server-side search) | Pencarian dan pemilihan berfungsi; kartu "Konteks Kunjungan" ter-update benar (pasien, RM, RAJAL, Tunai, tanggal kunjungan) | `PASS` | Screenshot `03-encounter-selected.png` |
| Field "Harga (Rp)" sebelum tarif dipilih | Kosong, `disabled: true` terkonfirmasi lewat DOM (`isDisabled()`) | `PASS` | Log `price field value: "" disabled: true` |
| Buka dan cari dropdown "Tarif Layanan" | **Selalu kosong** — bug backend pra-eksisting (filter scope strict-equality menyaring habis tarif berscope null) | `NEW ERROR` (backend, di luar scope frontend) | Panggilan API langsung: `search=a` tanpa filter → 351.749 hasil; dengan `serviceUnitId` kunjungan sungguhan → 0 hasil. Root cause dan perbaikannya didokumentasikan di `task/report/backend/BE-BKC-FIX-001.md` |
| Pilih tarif → cek harga otomatis terisi → submit → invoice tersimpan | **Belum bisa diverifikasi** — terhalang bug di atas; perbaikannya (`BE-BKC-FIX-001`) sudah ditulis tapi backend belum di-build ulang/restart | `NOT RUN` | Lihat `BE-BKC-FIX-001.md` § 5 dan § 8 |

Uji manual: `PARTIAL` — struktur layar, salinan teks, dropdown kunjungan, dan field harga read-only
sudah diklik-coba langsung dan lulus. Jalur inti (pilih tarif → harga otomatis → submit) terhalang
bug backend yang ditemukan saat verifikasi ini sendiri, sudah diperbaiki sumbernya tapi belum bisa
diklik-coba ulang sampai backend di-build ulang.

**Tidak dijalankan:** `npm run build`/`next build` penuh (lint per-berkas + rendering langsung
sudah menutup risiko syntax/import error yang selama ini jadi perhatian utama di modul ini, lihat
memori sesi tentang keterbatasan `no-undef` ESLint); submit sungguhan (terhalang bug backend).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `UAT-01`, `02` — kasir memilih item dari katalog tarif resmi, harga terisi otomatis, tidak bisa diketik manual | Source terpenuhi, **verifikasi interaktif penuh tertunda** oleh bug backend (lihat § 6) | Field `disabled: true` terkonfirmasi; pengisian harga dari `option.price` terkonfirmasi lewat pembacaan kode `handleChange`, belum lewat klik-coba ujung-ke-ujung |
| Disambiguasi multi-baris tarif nama sama tampil berlabel scope (`BKC-DEC-061`) | Terpenuhi (source) | `extendOption` pada resource `tariffs` — belum bisa dibuktikan visual lewat klik-coba karena dropdown masih kosong akibat bug backend |
| DoD: Field harga tidak punya `onChange`; tests/lint/build lulus; tidak ada field harga di request payload (`BIL-VAL-026`) | Field harga: terpenuhi (tidak ada `onChange`/`normalizeValue` terpasang). Lint: terpenuhi. Request payload `addCatalogChargeAsync`: terpenuhi (hanya `encounterId`/`tariffId`/`quantity`/`correlationId`/`causationId`). Build penuh (`next build`): **belum dijalankan** | Lihat § 3.2 dan § 6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Verifikasi manual awalnya terhalang total: dropdown "Tarif Layanan" tidak pernah menampilkan hasil apa pun. Ditelusuri sampai ke akar masalah backend (bukan bug frontend) — lihat baris *Dependency backend* |
| Masalah yang diketahui | 1) Pesan "kosong" pada dropdown tarif masih memakai default komponen ("Data tidak ditemukan"), belum dikustomisasi jadi pesan yang menyebutkan kemungkinan filter kategori/kunjungan terlalu sempit — perbaikan kosmetik, bukan penghalang fungsi. 2) Field "Kategori Tarif" terlihat sedikit tertutup footer sticky pada resolusi tertentu (terlihat di screenshot) — kemungkinan quirk layout pra-eksisting `BaseEditorView`, bukan sesuatu yang diperkenalkan task ini (field lain di layar yang sama tidak menunjukkan pola ini secara konsisten), belum diverifikasi lebih lanjut |
| Dependency backend | **`BE-BKC-FIX-001`** (perbaikan filter scope tarif) — source sudah ditulis, **backend belum di-build ulang/restart**. Sampai itu terjadi, dropdown "Tarif Layanan" akan tetap kosong pada lingkungan manapun yang memakai binary backend saat ini. Ini SATU-SATUNYA penghalang tersisa untuk verifikasi ujung-ke-ujung task ini |
| Perubahan sampingan | `NONE` pada source. Skrip Playwright sementara yang dipakai untuk verifikasi (`__verify_fe_bkc_014_tmp.mjs`, `__probe_tariff_filter.mjs`) sudah dihapus dari working tree setelah dipakai — dikonfirmasi lewat `git status --short` bersih dari berkas untracked selain source task ini |
| Interupsi | Verifikasi manual menemukan bug backend yang tidak terduga (lihat § 1 dan *Dependency backend*) — dikonfirmasi eksplisit ke pengguna lewat pertanyaan sebelum melanjutkan perbaikannya sebagai task terpisah (`BE-BKC-FIX-001`); pengguna memilih memperbaikinya sekarang |
| Status Git | `On branch (frontend), working tree bersih dari berkas sementara`. Modified: `src/components/view/.../create-manual/create-manual-invoice-view.jsx`, `src/lib/hooks/.../use-create-manual-invoice.js`, `src/lib/hooks/select/health-service/health-service-select-resources.js`, `src/lib/state/slice/.../billing-invoice-slice.jsx`. Belum staged/commit |
| Langkah berikutnya | 1) **Build ulang dan restart backend** (lihat `BE-BKC-FIX-001.md` § 7). 2) Verifikasi ulang jalur inti: pilih tarif → cek harga terisi otomatis dan disambiguasi label scope tampil → submit → invoice tersimpan dengan `SourceDomain=ADHOC_CATALOG` dan `TariffId` terisi (`BIL-AT-025`, sudah dites di sisi backend pada `BE-BKC-019`). 3) Setelah terverifikasi, lanjut `FE-BKC-015` (badge coverage pasien asuransi, bergantung `BE-BKC-020`+`FE-BKC-014` ini) atau `FE-BKC-016` (subtotal terpisah, tidak bergantung apa pun, boleh paralel) |
