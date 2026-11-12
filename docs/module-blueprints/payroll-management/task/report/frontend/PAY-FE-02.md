# Laporan Perubahan Frontend — `PAY-FE-02`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-02` |
| Judul | Pindah halaman master data Payroll |
| Slice | Payroll mandiri — data induk |
| Roadmap | Tidak ada roadmap Payroll terdaftar di `docs/module-blueprints/`; register status ada di `PAYROLL_V2_04_KONTRAK_DAN_TASK_IMPLEMENTASI.md` bagian 2 |
| Trace | Prompt tabel ownership; `PAYROLL_V2_01` bagian 5.3 |
| Contract version | Controller Backend Payroll pada SHA `b5fb6c251eafcd680ec35483732e3e72226e19f3` + perubahan lokal (kontrak dibaca dari source, tidak ditebak) |
| Wewenang UI | Prompt `MASTER_PROMPT_MIGRASI_MODUL_PAYROLL_QUILVIAN_V2.md`: pakai komponen, tema, filter, tabel, status, pagination, navigasi, hak akses yang sudah ada; label Bahasa Indonesia |
| Dependency | PAY-BE-02 |
| Klasifikasi | `MEDIUM` — 10 entitas, perubahan mekanis |
| Task mode | `CROSS-REPO` (BE dan FE sama-sama diberi wewenang tulis oleh prompt) |
| Target tulis | `QuilvianSystemFrontendDev` branch `yasmina`; laporan ini di repository backend |
| Model | Claude (`claude-sonnet-5-5`) |
| Commit frontend saat dikerjakan | `6a980d8c9dee0cae92c616fcd74f903ec0c67d73` + perubahan lokal belum di-commit |
| Commit backend yang dijadikan rujukan | `b5fb6c251eafcd680ec35483732e3e72226e19f3` + perubahan lokal belum di-commit |
| Tanggal | 2026-11-04 |
| Status | ✅ Selesai (statis) — rute dan endpoint dipindah; implementasi view/hook tetap di folder lama (fase 2). Pembaruan 2026-11-05 (bagian 9): cacat lama "Simpan tidak mengirim apa pun" diperbaiki pada lima form (Jenis Tunjangan, Jenis Potongan, dan tiga Kebijakan Tunjangan); belum diuji di browser |

## 1. Keadaan yang ditemukan di awal

Sepuluh halaman master penggajian berada di `/hr/master-data/...` dan memanggil endpoint HRD.

## 2. Proses bisnis dari sisi pengguna

Pengguna membuka master data dari menu Payroll > Data Induk. CRUD, filter, form, detail (`Kembali`/`Perbarui`/`Hapus`) bekerja seperti sebelumnya; hanya alamat halaman, endpoint, dan teks "Payroll > Data Induk" yang berubah.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`/`CLAUDE.md` frontend, `rules/frontend/*` (arsitektur, katalog base component, gerbang keputusan, token desain, pola halaman, checklist UI, kebijakan tes), modul referensi visual terdekat (`hr/payroll/run`, `hr/master-data/job-level`), `menu-items.jsx`, `InstanceAxios`, controller/DTO Payroll di backend.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/payroll/master-data/<10 entitas>/**` | 50 berkas rute dipindah dengan `mv` dari `src/app/hr/master-data/<entitas>/` |
| `src/lib/constants/hr/master-data/<10 entitas>/*-constants.jsx` | `routeBase` → `/payroll/master-data/<entitas>`; `endpoint` → `/v1/corporate/payroll-management/master-data/...`; `routeTokenScope` tidak diubah |
| `src/components/view/hr/master-data/<10 entitas>/**` | 30 berkas: eyebrow "Payroll > Data Induk", deskripsi "data induk penggajian" (hanya teks) |

### 3.3 Kepatuhan arsitektur frontend

Alur `route tipis → view → hook → service → InstanceAxios`; halaman di `src/app/payroll/**` hanya memuat metadata dan satu view; tidak ada `InstanceAxios` di view atau halaman; alias `@/`; konstanta endpoint/rute/izin dipusatkan di `src/lib/constants/payroll/`; tidak ada Axios instance baru, tidak ada hook/view master-data generik baru, dan tidak ada Redux slice baru (data operasional mengikuti pola hook + service payroll run yang sudah ada).

**Keputusan komponen (UI GATE):** `UI GATE: N/A — tidak ada elemen UI baru; hanya rute, endpoint, dan teks`

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tidak berubah (pola master data existing) |
| Kosong | Tidak berubah |
| Gagal | Tidak berubah |
| Tanpa hak akses | Halaman diganti peringatan "Ups! Akses Ditolak" (`AccessDeniedGate`); menu tidak tampil bila izin menu tidak ada |

## 5. Endpoint yang dikonsumsi

Grup **Corporate / Payroll Management / Master Data / ...** (10 grup; baris di bawah mewakili kesepuluhnya).

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/payroll-management/master-data/{salary-grades|salary-structures|payroll-periods|payroll-components|payroll-component-categories|allowance-types|deduction-types|shift-allowance-policies|on-call-allowance-policies|hazard-allowance-policies}` | Daftar, detail, buat, ubah, hapus data induk | ``<Entitas> : Read/Create/Update/Delete`` |

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `eslint` pada cakupan yang disentuh | 0 error; 27 peringatan `react-hooks/set-state-in-effect` pada hook baru (pola yang sama ada di hook payroll lama) | `EXISTING WARNING` | keluaran eslint |
| `node --test tests/unit/payroll-module-contract.test.mjs` | 12 lulus | `PASS` | berkas tes |
| `node --test tests/unit/payroll-run-screens.test.mjs`, `payroll-installment-deduction-guards`, `hr-master-data-sidebar-grouping` | 11 + 6 + 6 lulus | `PASS` | berkas tes |
| Resolver impor statis (`verify_fe.py`) | Tidak ada impor baru yang tak terselesaikan dibanding SHA dasar | `PASS` | keluaran skrip |
| Grep anti-regresi (tombol/tabel mentah, warna literal, `!important`, tipografi Bootstrap, inline style) | Kosong pada berkas baru | `PASS` | tes kontrak + grep |
| `npm run build` | Tidak dijalankan (dilarang pemilik) | `NOT RUN` | - |
| Uji manual kontrol interaktif (filter, cari, reset, paginasi, kirim, unduh) | Tidak ada server yang boleh dijalankan | `NOT FEASIBLE` | - |

AUTOMATED TEST: `node --test` pada berkas tes di atas — PASS. Seluruh suite unit memiliki kegagalan lama yang tidak terkait (dibandingkan dengan ekspor bersih SHA dasar).

**Tidak dijalankan:** build, server pengembangan, e2e. Perbandingan visual berdampingan dengan modul referensi belum dilakukan.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| CRUD/filter/form identik atau lebih baik | Terpenuhi (statis): identik dengan SHA dasar, **termasuk cacat lamanya** | Tidak ada perubahan perilaku; hanya konstanta rute/endpoint dan teks. Pemeriksaan statis awal tidak mendeteksi bahwa form simpan lima entitas sudah gagal di SHA dasar; ditemukan 2026-11-05 dari tangkapan layar pemilik (bagian 9) |
| Endpoint baru sesuai BE | Terpenuhi | Tes kontrak: setiap konstanta memakai alias `payroll-management` yang ada di controller |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kompilasi/terjemahan LINQ/perilaku layar belum dibuktikan karena `dotnet build`, `npm run build`, dan server dilarang pemilik. |
| Masalah yang diketahui | Berkas view/hook/slice/konstanta master tetap di `hr/master-data` (fase 2, kosmetik). |
| Dependency backend | Alias rute BE tersedia (PAY-BE-02). |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 45 file `M`, 65 file `D` (dipindah/diganti), 133 file `??` — tidak ada stage/commit |
| Langkah berikutnya | Fase 2: pindahkan implementasi 10 entitas ke `payroll/master-data`. |

## 9. Pembaruan 2026-11-05 — tombol Simpan Jenis Tunjangan tidak mengirim apa pun

**Gejala (laporan pemilik):** di `/payroll/master-data/allowance-type/create`, form terisi lalu **Simpan Jenis Tunjangan** tidak menyimpan data.

**Cara penelusuran:** log backend hari itu (`Logs/quilvian-backend-20261105.json`) hanya diperiksa pada kolom tanggal, metode, path, dan status. Hasilnya: `POST .../payroll-management/master-data/salary-structures` berhasil **200** lewat alias baru, tetapi untuk `allowance-types` **tidak ada `OPTIONS` maupun `POST`**. Jadi permintaan tidak pernah keluar dari browser dan alias rute BE bukan penyebab. Fungsi `validateForm` dan `buildPayload` yang asli lalu dijalankan dengan Node (tanpa server, tanpa build) memakai isian lengkap.

**Penyebab:** `ALLOWANCE_TYPE_CONFIG.validateFormExtra` mewajibkan `form.currencyCode` berisi 3 karakter, padahal form tidak punya field mata uang dan state form tidak pernah memuat `currencyCode`. Validasi selalu gagal dengan galat yang tidak tampil di layar (tidak ada kolom untuk menampilkannya), sehingga hanya toast "Form Belum Lengkap" yang muncul sekilas. Cacat ini **sudah ada di SHA dasar** (baris yang sama di `HEAD`), bukan akibat pemindahan.

| Contoh | Sebelum | Sesudah |
| --- | --- | --- |
| Tambah: nama "Tunjangan Uji Coba", tanggal 02 Jul–28 Nov 2026, deskripsi terisi | Galat `currencyCode: Kode mata uang harus terdiri dari 3 karakter.`; tidak ada permintaan | Lolos; payload membawa `currencyCode: "IDR"` |
| Tambah dengan nama kosong | Dua galat, satu tidak terlihat | Satu galat terlihat: "Nama Jenis Tunjangan wajib diisi." |
| Ubah jenis tunjangan bermata uang `USD` | Tidak bisa disimpan; seandainya lolos, backend menimpa ke `IDR` | Lolos; `USD` dipertahankan |

**Perbaikan:** `currencyCode` dibawa di state form seperti `sortOrder` yang sudah ada: `buildInitialForm` mengisinya dari detail (atau `IDR` saat tambah) dan `buildPayload` mengirimnya kembali. Validator tidak dilonggarkan; aturan tanggal, persentase, dan nominal maksimum tetap berlaku.

| Berkas | Perubahan |
| --- | --- |
| `src/utils/hr/master-data/allowance-type/allowance-type-utils.jsx` | `buildInitialForm` dan `buildPayload` membawa `currencyCode` (permintaan pertama pemilik) |
| `src/utils/hr/master-data/deduction-type/deduction-type-utils.jsx` | Perubahan yang sama (permintaan lanjutan pemilik) |
| `src/utils/hr/master-data/{hazard,on-call,shift}-allowance-policy/*-utils.jsx` | Perubahan yang sama; `buildInitialForm` diubah dari `return fields.reduce(...)` menjadi variabel `form` agar ada tempat membawa `currencyCode` |
| `tests/unit/payroll-master-allowance-type-form.test.mjs` | Baru: 5 uji regresi yang menjalankan util asli |
| `tests/unit/payroll-master-hidden-currency-code.test.mjs` | Baru: 15 uji berbasis tabel untuk kelima entitas (form minimal lolos dan membawa IDR; tidak ada galat pada field yang tidak dirender; mata uang tersimpan dipertahankan saat ubah) |

**Verifikasi:**

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test` pada `payroll-master-allowance-type-form` dan `payroll-master-hidden-currency-code` | 5 + 15 lulus | `PASS` |
| Uji Payroll yang ada (`payroll-module-contract`, `payroll-run-screens`, `hr-master-data-sidebar-grouping`) | 12 + 11 + 6 lulus (total gabungan 49 lulus, 0 gagal) | `PASS` |
| Pemindaian ulang kesepuluh form master dengan util asli (Node, tanpa server) | 0 galat pada field yang tidak dirender; select wajib yang diisi UUID tidak memunculkan penghalang kedua pada tiga kebijakan dan Jenis Potongan | `PASS` |
| Penelusuran konsumen state form (`Object.keys/entries(form)`, `JSON.stringify(form)`) di hook editor dan base editor | Tidak ada; key `currencyCode` baru tidak terbaca di tempat lain | `PASS` |
| `eslint` pada seluruh berkas yang disentuh (5 util + 2 berkas uji) | 0 temuan | `PASS` |
| Klik Simpan di browser sampai backend menjawab | Belum dicoba: server tidak boleh dijalankan agen | `NOT FEASIBLE` |
| Payload memuat `defaultAmount`/`defaultPercentage` sebagai teks (`"0"`) | Diterima backend menurut konfigurasi JSON bawaan MVC (`AddJsonOptions` hanya menambah converter) dan sejalan dengan POST `salary-structures` yang berhasil; **belum terbukti pada endpoint ini** | `NEEDS CONFIRMATION` |

**Cakupan perbaikan:** pemindaian dengan harness yang sama menemukan galat `currencyCode` tak terlihat yang identik pada empat form lain: `deduction-type`, `hazard-allowance-policy`, `on-call-allowance-policy`, dan `shift-allowance-policy`. Atas permintaan lanjutan pemilik keempatnya diperbaiki dengan pola yang sama. Backend keempatnya juga mewajibkan `CurrencyCode` (3 karakter, bawaan `IDR`) dan menimpanya pada PUT, sehingga nilai tersimpan memang harus dibawa saat ubah. Lima entitas lain (`payroll-component`, `payroll-component-category`, `payroll-period`, `salary-grade`, `salary-structure`) tidak memiliki galat tersembunyi pada pemindaian ini (`salary-structure` terbukti menyimpan lewat log).

**Yang belum terbukti:** kelima form belum dicoba di browser sampai backend menjawab (`NOT FEASIBLE` bagi agen). Penghalang lain pada sisi backend (aturan bisnis, data induk yang belum ada) baru akan terlihat saat pengguna mencoba menyimpan.

`UI GATE: N/A — tidak ada perubahan JSX/CSS; hanya logika state form dan payload.` Backend tidak diubah. Migration/database: tidak tersentuh.
