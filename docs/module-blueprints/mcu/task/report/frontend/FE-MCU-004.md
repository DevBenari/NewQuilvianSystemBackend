# Laporan Perubahan Frontend — `FE-MCU-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-MCU-004` |
| Judul | Layar jenis Pemeriksaan MCU dan kategori kelayakan |
| Slice | `MVP-0` (pasangan `BE-MCU-006` ✅) |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian *`FE-MCU-004`* |
| Trace | `FR-MCU-004`; brief layar [`05-screen-brief.md`](../../../05-screen-brief.md) bagian 2, 3, 5.10 (`MCU-FE-002`); `MCU-OQ-002`, `003` (nilai master usulan) |
| Contract version | `MCU-API-v1` grup `Mcu Reference Data` — `approved` 2026-10-21; `VAL-MCU-09`. Tabel kontrak masih bertanda *Rencana*, tetapi ketujuh endpoint sudah dilayani `McuReferenceDataController` (`BE-MCU-006` ✅) dengan route, method, dan bentuk DTO yang sama |
| Wewenang UI | Brief layar `FE-MCU-000` ✅ (keputusan pemilik 2026-10-21) |
| Dependency | `FE-MCU-000` ✅, `FE-MCU-002` ✅, `BE-MCU-006` ✅ |
| Klasifikasi | `LARGE` — satu repository, ±16 berkas baru (route, view, hook, utilitas, konstanta, uji), tabel isian bertingkat dan modal, kontrak yang sudah ada, tanpa database, keamanan berkaitan (izin per aksi `McuReferenceData`), UI sedang–besar |
| Task mode | `FRONTEND` |
| Target tulis | Frontend: route, view, hook, utilitas, konstanta jenis pemeriksaan dan kategori kelayakan, menu, uji unit + E2E. Backend: laporan ini; tanda roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `d0888bf6f` (branch `YogaV2`) |
| Commit backend yang dijadikan rujukan | `a3b63b50` (branch `yoga`) |
| Tanggal | 2026-10-27 |
| Status | Selesai — kedua kriteria terbukti di E2E API-tiruan; UAT baca terhadap backend lokal lulus |

---

## 1. Keadaan yang ditemukan di awal

- Lapisan data `FE-MCU-002` ✅ sudah punya 7 thunk grup `Mcu Reference Data` di slice `mcuReferenceData`.
- Belum ada route, menu, maupun layar jenis Pemeriksaan MCU dan kategori kelayakan.
- Backend (`McuReferenceDataService`):
  - jenis isian adalah enum angka `Text = 1`, `Number = 2`, `Option = 3`, `YesNo = 4`, berlabel *Teks*, *Angka*,
    *Pilihan*, *Ya/Tidak*;
  - daftar pilihan disimpan sebagai teks dipisah `|`;
  - isian Pilihan tanpa daftar ditolak dengan `VAL-MCU-09` *"Isian pilihan wajib berisi daftar pilihan."*;
  - isian disimpan dan dikembalikan urut nomor butir;
  - mengganti jenis isian yang sudah dipakai hasil pemeriksaan ditolak `409`;
  - `UpdateMcuExaminationTypeRequest.IsActive` bawaannya `true`.
- Grup ini **tidak** punya `/filters/metadata`, `/summary`, `PATCH status`, maupun `DELETE`.
- `GET fitness-categories` mengembalikan `List` utuh (bukan halaman) dan hanya menerima `isActive`.
- devYoga sudah berisi master usulan: jenis `FISIK` (1 isian) dan `TANDA_VITAL` (5 isian), serta kategori `FIT`,
  `FIT_DENGAN_CATATAN`, `UNFIT`, `UNFIT_SEMENTARA`.

---

## 2. Proses bisnis dari sisi pengguna

1. **Menu** *Medical Check-Up › Master Data* kini memuat *Jenis Pemeriksaan MCU* dan *Kategori Kelayakan*. Keduanya
   tampil bagi pemegang `McuReferenceData : Read`.
2. **Daftar Jenis Pemeriksaan MCU** — tabel kode, nama, jumlah isian, dan status.
   - Penyaring yang dikirim ke server: status dan kata kunci.
   - Tanggal mulai/akhir dan periode tetap tampil sesuai standar master data, tetapi tidak dikirim.
   - Catatan *usulan* untuk master bawaan ditampilkan.
   - Klik dua kali (atau aksi *Ubah Jenis*) membuka halaman ubah.
3. **Buat Jenis Pemeriksaan MCU** — data jenis (kode, nama, keterangan) dan tabel **Isian Formulir**:
   - kolom: nomor butir, kode, label, jenis, satuan, pilihan, wajib;
   - isian baru otomatis dinomori satu di atas nomor terbesar;
   - kolom *Pilihan* hanya dapat diisi untuk isian Pilihan, dengan petunjuk jumlah pilihan (*"3 pilihan, dipisah
     tanda |"*).
4. **Simpan ditolak** — kalimat server tampil apa adanya di *Jenis Pemeriksaan MCU belum disimpan*, misalnya
   `VAL-MCU-09` atau `409` isian yang sudah dipakai. Semua isian tetap.
5. **Simpan berhasil** — halaman pindah ke **Ubah Jenis Pemeriksaan MCU** (URL bertoken, tanpa UUID). Isian tampil
   urut nomor butir dan membawa `id` tersimpan.
6. **Ubah** memuat sakelar **Status Aktif** (dikirim sebagai `isActive`). Menurut aturan master data, halaman ini
   tidak memakai tombol Aktifkan/Nonaktifkan. Mengganti jenis isian tidak membuang daftar pilihannya, sehingga
   pilihan kembali utuh bila jenisnya dikembalikan.
7. **Kategori Kelayakan** — tabel kode, nama, keterangan, dan status, dengan modal **Buat** (kode, nama, keterangan)
   dan modal **Ubah** (ditambah *Status Aktif*).
   - Isian wajib diperiksa di layar.
   - Penolakan server tampil di dalam modal, dan isian modal tetap.
   - Sesudah simpan, daftar dimuat ulang.
   - Kata kunci menyaring daftar yang sudah termuat utuh.
8. **Tanpa izin:**
   - tanpa `Create` → tidak ada tombol buat, dan halaman Buat menampilkan akses ditolak;
   - tanpa `Update` → halaman Ubah jenis baca-saja, dan kategori tanpa aksi ubah;
   - `403` pada daftar → halaman akses ditolak.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah (frontend)

| Berkas | Perubahan |
| --- | --- |
| `src/app/health-services/medical-checkup-management/master-data/examination-types/page.jsx`, `examination-types-client.jsx`, `create/page.jsx`, `[examinationTypeId]/page.jsx` (baru) | Route daftar, buat, ubah (token → id; token kosong/cadangan/>180 → `notFound`) |
| `src/app/…/master-data/fitness-categories/page.jsx`, `fitness-categories-client.jsx` (baru) | Route kategori kelayakan |
| `src/components/view/…/master-data/mcu-examination-types/master-data-mcu-examination-type-view.jsx` (baru) | Daftar jenis |
| `…/mcu-examination-types/add/mcu-examination-type-form-view.jsx`, `mcu-examination-field-table.jsx` (baru) | Form jenis dan tabel isian |
| `src/components/view/…/master-data/mcu-fitness-categories/master-data-mcu-fitness-category-view.jsx` (baru) | Daftar kategori + modal buat/ubah |
| `src/lib/hooks/…/mcu-examination-types/use-master-data-mcu-examination-type.jsx`, `…-editor.jsx` (baru) | Logika daftar dan editor jenis |
| `src/lib/hooks/…/mcu-fitness-categories/use-master-data-mcu-fitness-category.jsx` (baru) | Logika daftar dan modal kategori |
| `src/utils/…/mcu-examination-types/mcu-examination-type-utils.js` (baru) | Detail ↔ form ↔ payload, urut nomor butir, nomor butir berikutnya, jumlah pilihan, validasi data jenis |
| `src/utils/…/mcu-fitness-categories/mcu-fitness-category-utils.js` (baru) | Form, validasi, payload, saringan kata kunci |
| `src/lib/constants/…/mcu-examination-types/mcu-examination-type-constants.js`, `…/mcu-fitness-categories/mcu-fitness-category-constants.js` (baru) | Config, jenis isian, batas panjang (sama dengan backend), catatan usulan |
| `src/utils/menu-sidebar/menu-items.jsx` | Dua butir menu di bawah *Medical Check-Up › Master Data* (ikon `RiStethoscopeLine`, `RiShieldCheckLine` yang sudah diimpor) |
| `tests/unit/mcu-reference-data-screen-rules.test.mjs` (baru) | 8 uji aturan murni |
| `tests/e2e/mcu-reference-data-screen.spec.mjs` (baru) | 9 skenario API-tiruan |

Slice, service, konstanta API, dan store **tidak** diubah, karena lapisan data `FE-MCU-002` sudah cukup.

### 3.2 Kepatuhan arsitektur frontend

- Alur `view → hook → slice (FE-MCU-002) → service → InstanceAxios`; tidak ada Axios di view/page.
- Pola diambil dari layar paket `FE-MCU-003` dan standar master data:
  - `BaseEditorView` untuk form;
  - `DataFilter` + `DataTable` + `RegionPagination` untuk daftar;
  - `ConfirmModal` berisi `BaseTextField`/`BaseTextAreaField`/`BaseSwitchField` untuk modal kategori;
  - token route privat.
- Tidak ada komponen, CSS Module, dependency, abstraksi HTTP, atau pola state baru.

### 3.3 Selisih terhadap standar master data (dicatat, bukan disamarkan)

| Standar | Di task ini | Alasan |
| --- | --- | --- |
| Sembilan endpoint baseline (`summary`, `filters/metadata`, `DELETE`, `PATCH status`, …) | Tidak ada | Kontrak `MCU-API-v1` grup ini hanya punya 7 endpoint; status diubah lewat `PUT` |
| Halaman detail terpisah | Tidak ada | Brief layar `MCU-FE-002` §3 menetapkan halaman ubah saja (sama dengan paket) |
| Payload buat tanpa kode bisnis | Kode jenis/kategori diisi admin | `CreateMcuExaminationTypeRequest`/`CreateMcuFitnessCategoryRequest` mewajibkan kode (kontrak terkunci) |
| Pilihan jumlah baris dari metadata | Ditetapkan di config (`10/25/50/100`) | Tidak ada `/filters/metadata` |
| Kategori: paginasi + select jumlah baris | Tanpa paginasi dan tanpa select jumlah baris | `GET fitness-categories` mengembalikan seluruh daftar (`List`) |
| Kategori: kata kunci dikirim ke server | Disaring di layar | `McuFitnessCategoryQuery` hanya menerima `isActive` |

---

## 4. State yang ditangani di layar

| State | Perilaku |
| --- | --- |
| Memuat | Penanda muat daftar, detail, dan tindakan; tombol simpan terkunci saat mengirim |
| Kosong | *Data jenis Pemeriksaan MCU tidak ditemukan…* / *Data kategori kelayakan tidak ditemukan…*; tabel isian kosong menyebut syarat minimal satu isian |
| Gagal muat | Pesan server + *Coba lagi* |
| Ditolak server | Kalimat server apa adanya (form: di atas tombol simpan; kategori: di dalam modal); isian tetap |
| Tanpa hak akses | Halaman akses ditolak (`403`/tanpa `Create`); Ubah jenis baca-saja tanpa `Update`; kategori tanpa aksi ubah |

---

## 5. Endpoint yang dikonsumsi

Base URL: `/v1/health-services/medical-checkup-management` — `GET/POST /examination-types`,
`GET/PUT /examination-types/{id}`, `GET/POST /fitness-categories`, `PUT /fitness-categories/{id}` (rincian di
[laporan `FE-MCU-002`](FE-MCU-002.md) §5).

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `tests/unit/mcu-reference-data-screen-rules.test.mjs` | 8/8 lulus | `PASS` | `# pass 8`, `# fail 0` |
| Uji mutasi: pengurutan nomor butir dihapus dan pilihan kosong dikirim sebagai string | 4 uji **gagal**; berkas dipulihkan identik (`cmp`), kembali 8/8 | `PASS` | Keluaran `node --test` |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 3.622 uji: 3.576 lulus, 46 gagal — daftar nama kegagalan **identik** dengan baseline (semua di luar MCU) | `EXISTING / ENVIRONMENT ISSUE` | `diff` daftar kegagalan kosong |
| `npm run lint:errors` | Exit 0 (berkas task juga tanpa peringatan) | `PASS` | Keluaran perintah |
| `npm run build` | Exit 0: *Compiled successfully in 60s*, 522 halaman; empat route baru terbentuk | `PASS` | Keluaran build |
| E2E API-tiruan `tests/e2e/mcu-reference-data-screen.spec.mjs` | 9/9 lulus | `PASS` | `9 passed (1.2m)` |
| **Kriteria 1** — isian Pilihan tanpa daftar pilihan | `POST` tetap dikirim (`fieldType: 3`, `optionValues: null`); `400` `VAL-MCU-09` tampil apa adanya; isian tetap; sesudah daftar diisi (`Normal|Parsial|Total`) tersimpan dan pindah ke halaman Ubah tanpa UUID | `PASS` | Skenario *buat jenis* |
| **Kriteria 2** — urutan isian mengikuti nomor butir | Detail dikirim tidak urut (10, 2, 5) → tampil 2, 5, 10. Isian baru bernomor 11 diubah ke 1 → `PUT` dikirim urut 1, 2, 5, 10 dengan `id` tersimpan; sesudah simpan baris pertama = isian nomor 1 | `PASS` | Skenario *ubah jenis* |
| Ubah: `isActive`, `409`, pilihan tetap utuh | `PUT` membawa `isActive: false`; `409` *"Isian Visus sudah dipakai…"* tampil apa adanya; Pilihan → Teks → Pilihan memulihkan `Normal|Kurang` | `PASS` | Skenario *ubah jenis* |
| Kategori | Wajib isi di layar tanpa permintaan; `400` tampil di modal dengan isian tetap; buat → body persis; ubah → `isActive: false`; penyaring status terkirim; kata kunci tidak terkirim | `PASS` | Skenario *kategori* |
| Hak akses dan gagal muat | Menu `Read`; tanpa `Create`/`Update`; `403` → akses ditolak; `500` → *Coba lagi* | `PASS` | 5 skenario |
| Regresi `tests/e2e/mcu-package-screen.spec.mjs` (menu berubah) | 12/12 lulus | `PASS` | `12 passed` |
| UAT baca, backend lokal + devYoga (semua tulis diblokir) | Daftar jenis (`FISIK` 1 isian, `TANDA_VITAL` 5 isian) dan kategori (4 usulan) termuat. Halaman Ubah `TANDA_VITAL` menampilkan butir 1–5 **sama persis** dengan urutan API; URL bertoken; modal ubah kategori terisi. Nol tulis, nol GET gagal | `PASS` | Log skrip + tangkapan layar |

`MANUAL TEST: PASS` — select jenis isian, sakelar status, modal, penyaring status/kata kunci, paginasi daftar jenis,
dan reset dijalankan lewat peramban (Playwright) pada E2E dan UAT baca.
`AUTOMATED TEST: npm run test:unit — FAIL (46 kegagalan existing di luar MCU, identik dengan baseline; 8/8 uji baru PASS)`.

UAT **tulis** ke devYoga tidak dijalankan: roadmap menetapkan verifikasi E2E API-tiruan, dan menulis master referensi
bersama butuh izin terpisah.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Isian pilihan tanpa daftar pilihan ditolak dengan pesan server | Terpenuhi | §6 Kriteria 1 |
| Urutan isian mengikuti nomor butir | Terpenuhi | §6 Kriteria 2 + UAT baca |
| DoD: kriteria terbukti | Terpenuhi | §6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko | Isi master bawaan belum diratifikasi dokter MCU (`MCU-OQ-002`/`003`). Layar menampilkan catatan usulan; isi master sendiri tidak diubah |
| Peringatan | Data bawaan devYoga sudah memuat teks *USULAN…* pada keterangan, sehingga catatan usulan tampil dua kali di daftar kategori. Ini tidak menyesatkan, tetapi dapat dirapikan saat master diratifikasi |
| Masalah yang diketahui | Tabel isian tidak mengurutkan ulang baris selagi nomor butir diketik (agar baris tidak melompat). Urutan dirapikan saat dikirim dan saat form dibentuk ulang dari jawaban server |
| Dependency backend | Tidak ada; `BE-MCU-006` ✅ |
| Perubahan sampingan | `NONE` |
| Revisi 2026-10-27 (saat `FE-MCU-005`) | `ConfirmModal` merender `children` menggantikan `message`, sehingga keterangan modal kategori (*"Kategori baru langsung aktif…"* / *"Perubahan berlaku untuk pilihan kesimpulan berikutnya."*) tidak pernah tampil. Keterangan dipindah ke dalam isi modal; spec E2E referensi 9/9 tetap lulus |
| Interupsi | `NONE` |
| Status Git (frontend) | ` M menu-items.jsx` (ditambah butir task ini di atas perubahan `FE-MCU-003`); `??` folder route/view/hook/konstanta/utilitas jenis pemeriksaan dan kategori, uji unit dan E2E. Berkas Lab/Radiologi lain yang ` M` bukan bagian task ini |
| Langkah berikutnya | `FE-MCU-005` (jadwal, kuota, dokter jaga) — siap; lalu lapisan data order `FE-MCU-006` |
