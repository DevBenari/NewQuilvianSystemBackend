# Laporan Perubahan Frontend — `PAY-FE-FIX-014`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-FIX-014` (ad-hoc lanjutan seri `PAY-FE-FIX`); pasangan backend `PAY-BE-FIX-014` |
| Judul | Master Kebijakan Tunjangan Transportasi: Grade Gaji dan Struktur Gaji bertingkat (form, daftar, detail) |
| Slice | Corporate / Payroll Management, Data Induk → Kebijakan Tunjangan Transportasi. **Hanya F5 (layar master)** dari desain `PAY-BE-FIX-013` |
| Roadmap | Tidak ada baris roadmap untuk seri ad-hoc; tidak ada roadmap atau traceability yang diubah (`NOT APPLICABLE`) |
| Trace | Keputusan pemilik 20 Nov 2026: D9 = Paket 1, DDL diterapkan pemilik (dinyatakan sudah diterapkan). Desain `PAY-BE-FIX-013`; kontrak `PAY-BE-FIX-014` bagian 4 |
| Contract version | Kontrak additive `PAY-BE-FIX-014` pada `transport-allowance-policies` (belum dibuild; lihat bagian 8) |
| Wewenang UI | Menambah dua field, tiga kolom, dua filter, tiga baris detail, dan dua keterangan pada layar yang sudah ada. Tanpa layar, route, slice, service, atau komponen dasar baru |
| Dependency | `PAY-BE-FIX-014` (source selesai, **`dotnet build` `NOT RUN`**). DDL Paket 1 dinyatakan sudah diterapkan pemilik; **belum diverifikasi dari sisi saya** |
| Klasifikasi | `MEDIUM` — skor 7 (satu repository: 0; berkas diperiksa >20: 2; berkas diubah 9: 2; logika sedang: 1; memakai kontrak yang ada: 1; database: 0; keamanan: 0; satu fitur tiga layar: 1) |
| Task mode | `FRONTEND` (kontrak backend dibaca, tidak ditulis) |
| Target tulis | `QuilvianSystemFrontendDev` ditambah berkas laporan ini |
| Model | Claude Sonnet 5.5 |
| Commit frontend saat dikerjakan | `31df72b9d907796eb106c638a2a350405c866efa` (branch `yasmina`) |
| Commit backend yang dijadikan rujukan | `d642a9df9de35801ed8332adc0cdcd6127f96353` (working tree memuat `PAY-BE-FIX-014`) |
| Tanggal | 20 November 2026 |
| Status | 🟡 **Source selesai dan 15 dari 15 tes lulus; tampilan belum dilihat dan build belum dijalankan.** `VISUAL CHECK: NOT FEASIBLE`. Cakupan ini **belum menyaring** penetapan pegawai maupun perhitungan payroll (F4 dan F6 belum ada) |

---

## 1. Keadaan yang ditemukan di awal

Kebijakan transport hanya punya cakupan Legal Entity, Fasilitas, dan Grade Pegawai. Setelah `PAY-BE-FIX-014`, backend menerima dan mengembalikan `salaryGradeId`, `salaryStructureId`, `salaryGradeName`, `salaryStructureName`, dan `scopeStatus`, tetapi layar belum memakainya: petugas tidak bisa mengisi, melihat, atau memfilter cakupan itu.

Bukti yang menentukan rancangan:

- `BaseEditorField` mengambil sendiri opsi select dari registry `SELECT_RESOURCES`, dan registry sudah memuat `salaryGrades` dan `salaryStructures` dengan cascade `salaryStructures ← salaryGradeId` (`select-resource-registry.js` baris 463). `BaseEditorForm` sudah mendukung `dependsOn` dan `remountKey`. Jadi tidak ada komponen baru yang dibutuhkan.
- **Jebakan yang ditemukan:** registry yang sama membuat `salaryGrades` otomatis menyempit mengikuti field `employeeGradeId` di form (`select-resource-registry.js` baris 464). Di form ini ada field Grade Pegawai, sehingga Grade Gaji akan menyempit mengikuti Grade Pegawai padahal keduanya atribut terpisah. Cascade itu dimatikan khusus untuk field ini (bagian 3.2).
- `useSelectResource` sudah membuang respons basi: `requestIdRef` dibandingkan `safeRequestId` (baris 730 dan 788) dan `AbortController` dibatalkan pada cleanup (baris 900). Catatan lama "pembatalan respons basi belum diverifikasi" dengan ini **terjawab**: aman untuk berganti Grade Gaji dengan cepat.
- Hasil baca: `BaseEditorField` memaksa `keepPreviousOptions: true`. Tanpa penanganan, setelah Grade Gaji berganti, daftar Struktur Gaji milik grade sebelumnya tetap tampil sampai daftar baru tiba. Itu diatasi dengan `remountKey` (bagian 3.2).

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** petugas HR/Payroll dengan hak `TransportAllowancePolicy : Create` atau `Update`.

**Tambah atau ubah kebijakan.**

1. Petugas membuka Data Induk → Kebijakan Tunjangan Transportasi, lalu menekan **+ Tambah Kebijakan Tunjangan Transportasi** (atau membuka detail lalu **Perbarui**).
2. Di bawah Fasilitas Rumah Sakit ada **Grade Gaji**, lalu **Struktur Gaji**, lalu Grade Pegawai (atribut terpisah).
3. **Struktur Gaji terkunci** dengan placeholder "Pilih Grade Gaji terlebih dahulu" sampai Grade Gaji dipilih.
4. Setelah grade dipilih, Struktur Gaji hanya menampilkan struktur **aktif milik grade itu**. Bila grade tidak punya struktur aktif, tampil "Tidak ada Struktur Gaji aktif untuk Grade Gaji ini."
5. Mengganti Grade Gaji **mengosongkan** Struktur Gaji; memilih ulang grade yang sama tidak mengosongkannya.
6. Keduanya boleh dikosongkan saat menambah (perilaku lama dipertahankan). Grade saja berarti semua struktur grade itu.
7. Petugas menyimpan. Pasangan yang tidak cocok ditolak backend, pesannya tampil di peringatan atas halaman.

**Contoh.** Kebijakan "Transport Dokter" diisi Grade Gaji `Grade A` dan Struktur Gaji `Struktur A-Dokter`. Petugas lalu mengganti grade menjadi `Grade B`: kolom Struktur Gaji langsung kosong, dan daftarnya hanya berisi struktur milik `Grade B`.

**Mode ubah, mengikuti aturan backend.**

| Keadaan kebijakan | Yang dilihat petugas | Aturan backend yang dicerminkan |
| --- | --- | --- |
| Data lama (tanpa cakupan) | Keterangan "Kebijakan ini belum ditetapkan untuk Grade Gaji atau Struktur Gaji tertentu (data lama)." Field boleh diisi | Data lama tetap berjalan; `ScopeStatus = LegacyUnscoped` |
| Sudah punya Grade Gaji | Grade Gaji menjadi **wajib** dan pilihan kosongnya hilang, dengan keterangan "Cakupan Grade Gaji yang sudah ditetapkan tidak dapat dikosongkan." | `PUT` yang mengosongkan cakupan ditolak `400` |
| Sudah dipakai penetapan transport pegawai | Grade Gaji dan Struktur Gaji **dikunci**, dengan keterangan jumlah penetapan dan saran membuat kebijakan baru | Cakupan berubah pada kebijakan yang dirujuk ditolak `409` |
| Nama tersimpan | Nama Grade Gaji dan Struktur Gaji tampil dari `salaryGradeName` dan `salaryStructureName`, bukan placeholder | — |

**Daftar.** Dua filter baru, **Grade Gaji** lalu **Struktur Gaji**, berada sebelum Status. Struktur Gaji terkunci ("Pilih Grade Gaji dahulu") sampai grade dipilih, dan dikosongkan bila grade berganti. Keduanya punya pilihan "Semua …" di depan untuk melepas filter tanpa menekan Reset. Tiga kolom baru menyusul Nama Kebijakan: **Grade Gaji**, **Struktur Gaji** ("Semua struktur" bila hanya grade), dan **Cakupan Gaji** (lencana: *Per Struktur Gaji* / *Per Grade Gaji* biru, *Belum ditentukan (data lama)* kuning).

**Detail.** Tiga baris baru (Grade Gaji, Struktur Gaji, Cakupan Gaji) dan keterangan data lama bila berlaku.

**Jalur tidak normal.**

| Kejadian | Yang dilihat petugas |
| --- | --- |
| Pilih Struktur Gaji tanpa Grade Gaji (tidak mungkin lewat layar, tetapi dijaga) | "Pilih Grade Gaji terlebih dahulu sebelum memilih Struktur Gaji." (sama dengan pesan backend) |
| Pasangan tidak cocok, master tidak aktif, tanggal di luar masa berlaku, irisan kebijakan | Pesan backend apa adanya pada peringatan atas dan toast ("Data Tidak Valid" / "Data Sudah Ada / Konflik") |
| Akun tanpa hak `SalaryGrade : Read` atau `SalaryStructure : Read` | Daftar pilihan gagal dimuat; pesan galat dari daftar tampil di menu pilihan (lihat bagian 8) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`transport-allowance-policy-constants.jsx`, `-utils.jsx`, tiga hook (list, editor, detail), tiga view (list, form, detail), slice, `base-editor-form.jsx`, `base-editor-field.jsx`, `base-editor-view.jsx`, `base-form-control.jsx`, `filter-select.jsx`, `resource-filter-select.jsx`, `use-select-resource.jsx`, `select-resource-registry.js`, `hr-select-resources.js`, `master-data-filter-constants.jsx`, `payroll-recap-view.jsx` (pola `ResourceFilterSelect`), editor penetapan gaji (pola Grade → Struktur), `InformationAlert`, `SalaryGradeController.cs`, `SalaryStructureController.cs`, serta kontrak `PAY-BE-FIX-014`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/payroll/master-data/transport-allowance-policy/transport-allowance-policy-constants.jsx` | Konstanta status cakupan, label, kunci lencana, dan pesan. Dua field form `salaryGradeId` dan `salaryStructureId` (di bawah Fasilitas, di atas Grade Pegawai). **`filters: { employeeGradeId: "" }` pada Grade Gaji** mematikan cascade registry ke Grade Pegawai. Dua kunci relasi baru pada validasi UUID. Validasi: struktur tanpa grade ditolak. Payload: `salaryGradeId`, `salaryStructureId` (struktur tanpa grade dipaksa `null`). `transformFormChange`: ganti grade mengosongkan struktur. Dua filter domain (`type: "resource"`) disisipkan sebelum Status, `dependentFilterResets`, `filterDefaults`, `supportedFilterKeys`. Tiga kolom tabel dan tiga baris detail |
| `src/utils/payroll/master-data/transport-allowance-policy/transport-allowance-policy-utils.jsx` | Fungsi murni baru: `getTransportPolicyScopeStatus` (memakai `scopeStatus` backend, atau diturunkan dari id seperti aturan backend), `resolveTransportPolicyScopeLabel`, `resolveTransportPolicyScopeBadgeStatus`, `resolveTransportPolicyStructureLabel`, `applyTransportPolicyScopeFieldState`, `buildTransportPolicyScopeSelectHandlers`. Dua format baris detail baru. Ditambahkan ke objek util bawaan |
| `src/lib/hooks/.../use-master-data-transport-allowance-policy.jsx` | Dua `useSelectResource` (Grade Gaji, Struktur Gaji) untuk filter, dikembalikan sebagai `resourceSelects`. `updateFilter` menerapkan `dependentFilterResets` |
| `src/lib/hooks/.../use-master-data-transport-allowance-policy-editor.jsx` | `fields` kini diturunkan lewat `applyScopeFieldState`; `savedDetail` hanya dipakai bila id-nya sama dengan yang dibuka; mengembalikan `optionHandlerMap` dan `showLegacyScopeNotice` |
| `src/lib/hooks/.../use-master-data-transport-allowance-policy-detail.jsx` | Mengembalikan `showLegacyScopeNotice` |
| `src/components/view/.../master-data-transport-allowance-policy-view.jsx` | Merender filter bertipe `resource` memakai `ResourceFilterSelect`; format kolom `salaryStructureScope` dan `scopeStatus` (lencana `StatusBadge`) |
| `src/components/view/.../add/transport-allowance-policy-form-view.jsx` | Meneruskan `optionHandlerMap`; keterangan data lama di `afterHero` |
| `src/components/view/.../detail/transport-allowance-policy-detail-view.jsx` | Keterangan data lama di `afterHero` |
| `tests/unit/payroll-transport-allowance-policy.test.mjs` | Payload yang dikunci tes lama diperbarui (dua kunci baru, `null`); 9 tes baru (bagian 6). `hasBackend` kini juga memeriksa DTO dan aturan cakupan backend |

Dengan sengaja **tidak diubah**: modal "Tambah Tunjangan Pegawai", halaman penetapan transport pegawai, `payroll-transport-allowance-utils.js` (pemilih kebijakan penetapan), slice, service, store, menu, dan seluruh komponen dasar. Pemilih kebijakan pada penetapan pegawai adalah bagian F4.

**Pilihan rancangan yang perlu diketahui:**

| Pilihan | Alasan |
| --- | --- |
| Opsi Grade Gaji dan Struktur Gaji di **filter daftar** memakai `onlyActive: false` | Filter ini untuk menelusuri data; kebijakan lama bisa menunjuk grade atau struktur yang kini nonaktif. Di **form**, hanya yang aktif (bawaan `BaseEditorField`) |
| Dua filter domain ditambahkan walau standar master data menyebut "satu select filter" | Standar itu tentang filter berbasis metadata (status). Pola komposisi list memang menyediakan "filter domain" sebelum status, dan task ini memintanya. Tafsir ini dicatat agar bisa dikoreksi |
| Teks "Pilih Grade Gaji terlebih dahulu" dan "Tidak ada Struktur Gaji aktif untuk Grade Gaji ini." disalin sebagai konstanta lokal | Sama dengan editor penetapan gaji; tidak mengimpor util HR yang besar ke domain payroll |
| Label tersimpan mode ubah hanya dipakai selama pilihan masih sama dengan yang tersimpan | Menghindari opsi lama ikut tampil di daftar setelah petugas mengganti pilihan (`FilterSelect` menggabungkan `selectedOption` ke daftar) |

### 3.3 Kepatuhan arsitektur frontend

Alur `view → hook → util/constant` dipatuhi. View tidak memanggil Axios; request tetap lewat `InstanceAxios` di dalam `useSelectResource`, dengan pembatalan saat cleanup. Logika turunan (status, label, keadaan field) ada di util murni, bukan di markup. Seluruh teks, kunci, dan endpoint ada di konstanta. Tidak ada factory, hook master-data generik, atau Axios baru. Import memakai alias `@/`. Semua berkas yang diubah semula CRLF dan tetap CRLF seluruhnya (CR = LF per berkas); berkas tes semula LF dan tetap LF.

### 3.4 Gerbang tampilan

**Modul referensi visual:** `view/payroll/master-data/transport-allowance-policy` itu sendiri (pola list, form, detail yang sudah ada), ditambah `payroll-recap-view.jsx` untuk pola `ResourceFilterSelect`.

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Field Grade Gaji dan Struktur Gaji pada form | `BaseEditorField` → `BaseSelectField` lewat registry `salaryGrades` dan `salaryStructures` | `REUSE` | `base-editor-field.jsx` baris 111-162; cascade `select-resource-registry.js` baris 463 |
| Struktur terikat grade, terkunci sebelum grade dipilih | `BaseEditorForm` `dependsOn` dan `remountKey` | `REUSE` | `base-editor-form.jsx` baris 167-181; `remountKey` juga dipakai editor nurse-view-bed-tablet |
| Nama tersimpan dan pesan kosong struktur | `optionHandlerMap` (`selectedOption`, `emptyText`) | `REUSE` | `base-editor-form.jsx` baris 159-165; `base-editor-field.jsx` baris 266-270 |
| Filter Grade Gaji / Struktur Gaji pada daftar | `ResourceFilterSelect` + `useSelectResource` | `REUSE` | Pola `payroll-recap-view.jsx` |
| Kolom Grade Gaji, Struktur Gaji | `DataTable` lewat `config.listColumns` | `REUSE` | Pola kolom yang ada |
| Lencana Cakupan Gaji | `StatusBadge` (`status` info/warning/danger, `label`) | `REUSE` | Kunci status sah menurut katalog |
| Keterangan data lama | `InformationAlert` `variant="info"` `closable={false}` | `REUSE` | Dipakai `payroll-recap-view.jsx` |
| Baris detail | `BaseDetailView` `detailRows` | `REUSE` | Pola yang ada |

Sumber gaya setiap wilayah: gaya bawaan base component (urutan 1). **Tidak ada CSS Module baru dan tidak ada CSS yang ditulis.**

`UI GATE: PASS — 8 elemen REUSE, 0 EXTEND, 0 COMPOSE, 0 WRAP, 0 NEW`

Hasil pemeriksa dan grep anti-regresi, ditempel apa adanya:

```text
$ node ../QuilvianEngineeringSkills/Claude/.claude/rules/frontend/tools/check-css-usage.mjs \
    <3 view yang diubah>
OK — 3 berkas diperiksa, 0 temuan.

$ grep (#3 tombol non-base, #4 <table, #5 utility tipografi, #7 class Tailwind, #8 dialog manual,
        #9 field mentah/UUID, #10 prop tidak sah) pada 3 view yang diubah
(kosong pada ketujuh pola)

$ npx eslint <9 berkas yang diubah>
✖ 2 problems (0 errors, 2 warnings)
  use-...-detail.jsx  baris 90  react-hooks/set-state-in-effect   (ada juga di HEAD, baris 90)
  use-...-editor.jsx  baris 180 react-hooks/set-state-in-effect   (ada juga di HEAD, baris 173)
```

Kedua peringatan itu **sudah ada di `HEAD`**: versi `HEAD` kedua berkas dilint dan menghasilkan peringatan yang sama pada efek resolusi token route (baris bergeser karena sisipan saya). Bukan peringatan baru.

| Layar | Screenshot | Catatan perbandingan dengan referensi |
| --- | --- | --- |
| `/payroll/master-data/transport-allowance-policy` (daftar) | — | `VISUAL CHECK: NOT FEASIBLE` |
| `/payroll/master-data/transport-allowance-policy/create` dan `.../[slug]/update` | — | `VISUAL CHECK: NOT FEASIBLE` |
| `/payroll/master-data/transport-allowance-policy/[slug]` (detail) | — | `VISUAL CHECK: NOT FEASIBLE` |

`VISUAL CHECK: NOT FEASIBLE` — server development dan build dilarang pada task ini. Task UI dengan status itu paling tinggi `🟡`. Hal yang paling perlu dilihat saat verifikasi tampilan: (1) dua filter baru membuat baris filter lebih panjang, apakah masih rapi dan tidak terpotong; (2) lebar kolom tabel dengan tiga kolom baru; (3) lencana pada kolom Cakupan Gaji; (4) Struktur Gaji benar-benar kosong dan daftarnya benar-benar berganti saat grade berganti; (5) nama tersimpan tampil pada mode ubah.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat daftar pilihan (form dan filter) | Indikator memuat bawaan select; mode ubah menampilkan "Memuat data tersimpan..." selama daftar dimuat |
| Struktur sebelum grade dipilih | Terkunci, placeholder "Pilih Grade Gaji terlebih dahulu" (form) atau "Pilih Grade Gaji dahulu" (filter) |
| Grade tanpa struktur aktif | Form: "Tidak ada Struktur Gaji aktif untuk Grade Gaji ini."; filter: "Tidak ada Struktur Gaji untuk Grade Gaji ini." |
| Gagal memuat pilihan | Pesan galat dari permintaan opsi tampil di menu pilihan (bawaan `useSelectResource`) |
| Tanpa hak akses | Akses ke halaman tetap dijaga `AccessDeniedGate`; hak baca Grade/Struktur Gaji lihat bagian 8 |
| Penyimpanan ditolak backend | Pesan backend pada peringatan atas dan toast; tombol simpan tetap dapat dicoba ulang |
| Data lama | Lencana kuning di daftar; keterangan di detail dan form ubah |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Payroll Management / Transport Allowance Policy

Base URL: `/v1/corporate/payroll-management/transport-allowance-policies` (cocok dengan `[Route]` controller; diperiksa tes)

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar; kini mengirim `salaryGradeId` dan `salaryStructureId` bila filter dipilih | `TransportAllowancePolicy : Read` |
| `GET` | `/{id}` | Detail dan mengisi form ubah; kini membaca id, nama, dan `scopeStatus` cakupan | `TransportAllowancePolicy : Read` |
| `POST` | `/` | Menambah; body kini memuat `salaryGradeId` dan `salaryStructureId` (`null` bila kosong) | `TransportAllowancePolicy : Create` |
| `PUT` | `/{id}` | Mengubah; body kini memuat dua id yang sama | `TransportAllowancePolicy : Update` |

#### Corporate / Payroll Management / Master Data / Salary Grade

Alamat yang dipanggil registry: `/v1/corporate/human-resource/master-data/salary-grades` (rute alias pada controller yang sama)

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/options` | Pilihan Grade Gaji pada form dan filter | `SalaryGrade : Read` |

#### Corporate / Payroll Management / Master Data / Salary Structure

Alamat yang dipanggil registry: `/v1/corporate/human-resource/master-data/salary-structures` (rute alias)

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/options` | Pilihan Struktur Gaji milik grade terpilih (`salaryGradeId`, `onlyActive`) | `SalaryStructure : Read` |

Kode status penting: `400` pasangan tidak cocok atau master tidak aktif atau tanggal di luar masa berlaku; `409` irisan kebijakan atau cakupan dikunci karena sudah dipakai penetapan pegawai; `403` tanpa hak.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/payroll-transport-allowance-policy.test.mjs` | **15 lulus, 0 gagal** (6 tes lama + 9 tes baru) | `PASS` | Dieksekusi sesi ini |
| Payload kontrak create yang dikunci tes lama | Diperbarui (dua kunci `null` baru), lulus | `PASS` | Tes ke-1 |
| Urutan field, `dependsOn`, resource registry, cascade struktur ← grade, cascade Grade Pegawai dimatikan, `filterKeys` struktur memuat `salaryGradeId` | Lulus | `PASS` | Tes ke-7 |
| Payload (null bila kosong, struktur tanpa grade tidak dikirim) dan validasi (struktur tanpa grade, id bukan UUID) | Lulus | `PASS` | Tes ke-8 dan ke-9 |
| Ganti grade mengosongkan struktur; grade sama mempertahankannya | Lulus | `PASS` | Tes ke-10 |
| Keadaan field: terkunci sebelum grade, `remountKey`, wajib bila grade tersimpan, terkunci bila dipakai 3 penetapan, daftar field asli tidak dimutasi | Lulus | `PASS` | Tes ke-11 |
| Label tersimpan hanya selama pilihan sama; pesan kosong struktur | Lulus | `PASS` | Tes ke-12 |
| Status cakupan, baris detail tanpa UUID | Lulus | `PASS` | Tes ke-13 |
| Urutan filter, kolom, `dependentFilterResets` | Lulus | `PASS` | Tes ke-14 |
| Kontrak backend: `[Route]`, query `salaryGradeId` dan `salaryStructureId`, lima field respons DTO, empat kunci status, pesan validasi | Lulus | `PASS` | Tes ke-15, membaca source `PAY-BE-FIX-014` |
| `npx eslint` pada 9 berkas yang diubah | 0 error, 2 peringatan yang juga ada di `HEAD` | `PASS` (tidak ada peringatan baru) | Bagian 3.4 |
| `check-css-usage.mjs` pada 3 view | 0 temuan | `PASS` | Bagian 3.4 |
| Grep anti-regresi (7 pola) | Kosong | `PASS` | Bagian 3.4 |
| Akhir baris | 8 berkas sumber CRLF seluruhnya (CR = LF); berkas tes LF seluruhnya | `PASS` | Hitung CR dengan `tr` |
| `npm run lint:errors` seluruh repository, `npm run test:unit` seluruh suite | Tidak dijalankan | `NOT RUN` | Hanya berkas yang diubah yang di-lint dan hanya satu berkas tes dijalankan; baseline 51 gagal pada modul lain tidak disentuh |
| `npm run build` | Tidak dijalankan | `NOT RUN` | Dilarang pada task ini |
| Layar dibuka di peramban, klik-klik dropdown, screenshot | Tidak dilakukan | `NOT RUN` | `VISUAL CHECK: NOT FEASIBLE` |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/payroll-transport-allowance-policy.test.mjs — PASS (15 dari 15)`. Sebagian besar tes ini menguji **fungsi murni dan konfigurasi**, bukan perilaku komponen di layar; mereka mengunci aturan, tetapi bukan bukti tampilan.

Uji manual: `NOT FEASIBLE`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Form: Grade Gaji lalu Struktur Gaji bertingkat; struktur bergantung grade | Terpenuhi pada source | Bagian 3.2; tes ke-7 |
| Struktur dikosongkan saat grade berubah | Terpenuhi pada source | Tes ke-10 |
| Pesan "Pilih Grade Gaji terlebih dahulu" dan "Tidak ada Struktur Gaji aktif untuk Grade Gaji ini." | Terpenuhi pada source | Tes ke-11 dan ke-12; baca `base-editor-field.jsx` baris 266-270 |
| Grade Gaji tidak mengikuti Grade Pegawai | Terpenuhi pada source | Tes ke-7 |
| Kolom tabel Grade Gaji, Struktur Gaji, Cakupan Gaji | Terpenuhi pada source | Tes ke-14; view |
| Filter terikat Grade Gaji → Struktur Gaji | Terpenuhi pada source | Tes ke-14; view |
| Detail menampilkan cakupan tanpa UUID | Terpenuhi pada source | Tes ke-13 |
| Mode ubah memakai nama tersimpan; aturan kunci mengikuti backend | Terpenuhi pada source | Tes ke-11 dan ke-12 |
| `buildInitialForm` / `validateForm` / `buildPayload` / `mapToForm` mengenal cakupan | Terpenuhi | Tes ke-1, ke-8, ke-9 |
| Lencana data lama (`LegacyUnscoped`) | Terpenuhi pada source | Bagian 3.2; view |
| Alamat dan field cocok dengan backend | Terpenuhi pada source backend `PAY-BE-FIX-014` | Tes ke-15 |
| Tampilan sesuai di layar | **Belum terpenuhi** | `VISUAL CHECK: NOT FEASIBLE` |
| Build berhasil | **Belum terpenuhi** | `NOT RUN` |
| Backend yang dikonsumsi terbukti berjalan | **Belum terpenuhi** | `PAY-BE-FIX-014` belum dibuild; DDL belum saya verifikasi |
| Penetapan pegawai hanya menawarkan kebijakan yang cocok (F4) | **Di luar task ini** | Belum dikerjakan |
| Perhitungan payroll memakai cakupan (F6) | **Di luar task ini** | Belum dikerjakan; jalur uang, menunggu keputusan |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Cakupan yang diisi di layar ini belum menyaring apa pun di penetapan pegawai maupun perhitungan payroll.** Kebijakan milik struktur lain masih bisa dipilih pada penetapan transport pegawai dan ikut dibayar sampai F4 dan F6 selesai. Layar sengaja tidak menjanjikan penyaringan itu |
| Masalah yang diketahui | (1) Pemilih kebijakan pada penetapan transport pegawai belum diubah (F4). (2) Kebijakan **aktif** yang grade atau strukturnya kemudian dinonaktifkan akan ditolak backend `400` ("Grade Gaji tidak ditemukan atau tidak aktif.") walau petugas hanya mengubah keterangan, karena backend memvalidasi ulang cakupan kebijakan aktif; pesannya tampil apa adanya. Keputusan perilaku itu ada di `PAY-BE-FIX-014`. (3) Opsi kedua select dipanggil lewat rute alias `human-resource` milik controller yang sama; sudah begitu sebelum task ini |
| Dependency backend | `PAY-BE-FIX-014` harus sudah dibuild dan dijalankan **setelah** DDL Paket 1. Tanpa itu, daftar dan form kebijakan gagal. Pemilik menyatakan DDL sudah diterapkan; saya tidak memverifikasinya |
| Hak akses | Pengisian cakupan membutuhkan `SalaryGrade : Read` dan `SalaryStructure : Read` selain hak kebijakan. Petugas yang hanya punya hak kebijakan akan melihat pilihan gagal dimuat. Hak itu dikelola admin di layar Akses Role |
| Perubahan sampingan | `NONE` oleh task ini. Working tree memuat pekerjaan pihak lain (editor workforce, penetapan gaji, `payroll-run.module.css`, dan lainnya) yang tidak disentuh |
| Interupsi | `NONE` |
| Status Git | Tidak ada stage, commit, push, pull, merge, atau rebase |
| Langkah berikutnya | Pemilik menjalankan backend dan melihat tiga layar di peramban (daftar, tambah, ubah, detail); lalu F4 (pemilih kebijakan penetapan pegawai) sebelum F6 |

```text
$ git status --short -- <berkas milik task ini>   (frontend, branch yasmina, HEAD 31df72b9)
 M src/components/view/payroll/master-data/transport-allowance-policy/add/transport-allowance-policy-form-view.jsx
 M src/components/view/payroll/master-data/transport-allowance-policy/detail/transport-allowance-policy-detail-view.jsx
 M src/components/view/payroll/master-data/transport-allowance-policy/master-data-transport-allowance-policy-view.jsx
 M src/lib/constants/payroll/master-data/transport-allowance-policy/transport-allowance-policy-constants.jsx
 M src/lib/hooks/payroll/master-data/transport-allowance-policy/use-master-data-transport-allowance-policy-detail.jsx
 M src/lib/hooks/payroll/master-data/transport-allowance-policy/use-master-data-transport-allowance-policy-editor.jsx
 M src/lib/hooks/payroll/master-data/transport-allowance-policy/use-master-data-transport-allowance-policy.jsx
 M src/utils/payroll/master-data/transport-allowance-policy/transport-allowance-policy-utils.jsx
 M tests/unit/payroll-transport-allowance-policy.test.mjs
```
