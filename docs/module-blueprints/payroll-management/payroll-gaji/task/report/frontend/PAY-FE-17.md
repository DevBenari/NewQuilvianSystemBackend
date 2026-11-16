# Laporan Perubahan Frontend — `PAY-FE-17`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-17` |
| Judul | Penghasilan Variabel per run |
| Slice | `MVP-1` — `EPIC PGJ-04`, layar `FE-PGJ-02` dan `FE-PGJ-03` |
| Roadmap | `docs/module-blueprints/payroll-management/payroll-gaji/roadmap/frontend-roadmap.md`, kartu `PAY-FE-17` |
| Trace | FR-PGJ-030–036; PRL-DEC-07, 10, 31, 38, 43; `../../../03-frontend-architecture.md` bagian 1, 3.2, 4, 5, 6; `../../../contracts/api-contract.md` grup Payroll Variable Input; `../../../contracts/validation-matrix.md` bagian 3 (`VI-01`–`VI-12`); `../../../contracts/state-transition-matrix.md` bagian 1 |
| Contract version | `PGJ-C1` — `approved` (Yasmin, 2026-11-12) |
| Wewenang UI | Isi layar, sumber data, hak akses per tombol, serta kalimat kosong dan gagal dikunci `03-frontend-architecture.md` bagian 3.2 dan 6. Bentuk form (modal), tata letak, dan komponen `DEV_DISCRETION` lewat gerbang keputusan base component. Format berkas impor tidak ditetapkan skema (dipilih CSV, lihat bagian 8) |
| Dependency | `PAY-BE-19` 🟡 dan `PAY-BE-20` 🟡 (di atasnya `PAY-BE-14`, `PAY-BE-18` 🟡) — source backend sudah ada dan dibaca langsung dari working tree backend, tetapi `dotnet build` dan uji API belum dijalankan pemilik. Aturan roadmap meminta prasyarat `[BE]` berstatus ✅ sebelum task dimulai; task ini dikerjakan atas instruksi pemilik pada 2026-11-13, sama seperti `PAY-FE-16` |
| Klasifikasi | `HEAVY` — layar baru dengan lima alur interaktif (daftar dan saringan, form, impor, aksi per baris, aksi massal), sepuluh endpoint, 11 berkas source baru, 2 berkas yang sudah ada diubah, dan satu berkas test |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` branch `yasmina`; laporan dan penandaan status di repository backend |
| Model | Claude Sonnet 5.5 |
| Commit frontend saat dikerjakan | `ff46d5db1` (branch `yasmina`); perubahan task ini dan `PAY-FE-16` belum di-commit |
| Commit backend yang dijadikan rujukan | `726234df` (branch `Yasmina`). Source yang dibaca untuk task ini (controller, service, DTO, konstanta variabel input, token versi, penjaga `VI-10`/`VI-11`) tidak berubah dari commit itu; di akhir pekerjaan `PayrollSimulationService.cs` tampak berubah oleh pihak lain (bukan task ini, tidak berkaitan dengan layar ini) |
| Tanggal | 2026-11-13 |
| Status | ✅ — source selesai dan 6 dari 6 kriteria terpetakan ke source; ESLint 0 galat; 34 uji unit baru lulus; suite penuh tanpa kegagalan baru; agen tidak membuka layar (`VISUAL CHECK: NOT FEASIBLE`, `MANUAL TEST: NOT FEASIBLE` oleh agen); pada 2026-11-13 pemilik melaporkan `npm run dev` berhasil dan seluruh uji lulus (laporan lisan, tanpa rincian per langkah, tanpa screenshot tersimpan; bukan bukti agen) lalu menginstruksikan status naik ke ✅; **`npm run build` tetap `NOT RUN`**; backend `PAY-BE-19` dan `PAY-BE-20` belum terbukti build dan uji API (masih 🟡) |

---

## 1. Keadaan yang ditemukan di awal

- **Backend** sudah menyediakan sepuluh endpoint di `PayrollVariableInputsController` (daftar, buat, impor, ubah isi,
  ajukan, setujui, tolak, batalkan, ajukan massal, setujui massal) dengan aturan `VI-01`–`VI-09` dan `VI-12`. Penjaga
  pengajuan run `VI-10` dan `VI-11` ada di `PayrollRunService` (`PAY-BE-20`). Tidak ada `DELETE`: pembatalan lewat aksi.
- **Frontend** belum punya layarnya. Detail Payroll Run (`/payroll/payroll-runs/[id]`) hanya punya tombol Kembali di
  header, dan tidak ada jalan masuk ke penghasilan variabel.
- **Perilaku backend yang menentukan rancangan:**
  - Pesan penolakan dikirim sebagai teks bebas di `message`, tanpa kode aturan dan tanpa nama isian.
  - Respons input **tidak** membawa waktu perubahan. Penjaga `VI-11` membandingkan
    `UpdateDateTime ?? CreateDateTime` dengan `CalculatedAt`, sedangkan token versi (`rowVersion`) dibentuk dari nilai
    yang sama (ticks .NET).
  - Impor menerima JSON `rows` (nomor pegawai, kode komponen, jenis, nominal, catatan), bukan berkas. Penolakan `VI-08`
    datang sebagai 400 dengan daftar baris di `errors.failedRows`, dan `RowNumber` dimulai dari 1 pada kiriman.
  - Pembuatan memakai `payrollRunEmployeeId` (id baris pegawai di run), bukan nomor pegawai. Daftar pegawai di run
    tersedia pada detail run (`employees`).
  - `PUT`/`PATCH /{id}/content` mengganti `attachmentPath` dengan isi permintaan. Tanpa membawa nilai lama, lampiran
    yang sudah ada terhapus saat ubah.
  - Aksi massal memproses tiap input sendiri-sendiri dan melaporkan hasil per input.

## 2. Proses bisnis dari sisi pengguna

**Pengguna.** Petugas Payroll yang memegang hak `PayrollVariableInput` di Akses Role: mengisi dan mengajukan (`Create`,
`Import`, `Update`, `Submit`) dan, bila ia juga memegang `Approve`, menyetujui. Pengaju boleh menyetujui inputnya sendiri
(PRL-DEC-10 dan 43).

**Langkah:**

1. Petugas membuka detail Payroll Run. Pemegang `PayrollVariableInput : Read` melihat tombol **Penghasilan Variabel**
   di header.
2. Layar menampilkan nomor run, saringan (cari nama atau nomor pegawai atau nomor input, Status, Jenis, jumlah baris), dan
   tabel: nomor input, pegawai, komponen, jenis, nominal, status beserta siapa yang mengajukan atau memutuskan, dan menu aksi.
3. **Tambah satu input** (hak `Create`): pilih Pegawai dari pegawai yang terdaftar di run, pilih Komponen (hanya
   komponen penghasilan aktif, daftar bercari), pilih Jenis Input (Insentif, Bonus, Honor Tugas Tambahan), isi Nominal dan
   Catatan. Input tersimpan sebagai **Draf** dengan nomor `PVI-...`. Untuk Honor Tugas Tambahan, Catatan berubah menjadi
   "Tugas yang Dihonori" dan wajib (`VI-06`).
4. **Impor** (hak `Import`): petugas memilih berkas CSV. Layar membaca berkas di peramban dan menyatakan jumlah baris
   yang siap diimpor, atau menahan pengiriman bila judul kolom hilang atau ada nominal yang bukan angka. Bila server
   menolak satu baris atau lebih (`VI-08`), **tidak ada baris yang tersimpan**; layar menampilkan daftar "Baris 17: ..."
   beserta sebabnya.
5. **Ajukan**, **Setujui**, **Tolak**, dan **Batalkan** per baris lewat menu aksi, hanya yang ada pada `availableActions`
   dari server dan yang dipegang haknya. Tolak dan Batalkan meminta alasan. Alasan ditampilkan pada baris berstatus Ditolak
   dan Dibatalkan.
6. **Aksi massal**: petugas mencentang beberapa baris, lalu menekan **Ajukan terpilih (n)** atau **Setujui terpilih (n)**.
   Hanya baris terpilih yang memenuhi aksi yang dikirim. Layar menampilkan hasil per baris: berapa berhasil dan, untuk yang
   gagal, nomor input, pegawai, dan sebabnya.
7. Bila ada input yang dibuat atau diubah sesudah perhitungan terakhir run, layar menampilkan peringatan "Ada n input
   berubah setelah perhitungan terakhir. Hitung ulang run." Petugas kembali ke detail run, menghitung ulang, lalu mengajukan
   run.

**Contoh** (data samaran, `UAT-11`): petugas mengimpor 120 insentif Mei (120 baris Draf), memilih semuanya dan menekan
Ajukan terpilih, lalu menyetujui 118 dan menolak 2 dengan alasan "Nominal salah". Run dihitung ulang; yang masuk gaji hanya
118 baris yang Disetujui. Dua baris Ditolak tetap tampil dengan alasannya sebagai jejak.

**Jalur tidak normal:**

| Keadaan | Yang terjadi |
| --- | --- |
| Run belum punya input | Tabel menampilkan persis: "Belum ada penghasilan variabel di run ini." |
| Daftar gagal dimuat | Kotak galat (pesan server, atau "Data gagal dimuat.") dan tombol **Coba lagi** |
| Server menjawab 403 saat memuat | Kotak galat "Anda tidak punya hak untuk tindakan ini." tanpa tombol coba lagi |
| Tanpa `PayrollVariableInput : Read` | Layar tidak ditampilkan; tombol masuk di detail run tidak tampil |
| Hanya `Read` | Daftar tampil tanpa Tambah, Impor, kolom pilih, dan menu aksi |
| Run berstatus Diajukan atau lebih jauh (tidak menerima input, `VI-01`) | Layar baca-saja: tanpa Tambah, Impor, kolom pilih, dan menu aksi; ada keterangan "Run ini sudah tidak menerima penghasilan variabel. Daftar hanya dapat dibaca." |
| Run berubah status saat petugas bekerja | Server menjawab `VI-01`; pesan tampil, run dan daftar dimuat ulang sehingga layar menjadi baca-saja |
| Honor tugas tambahan tanpa tugas | Pesan `VI-06` di bawah isian Catatan, sebelum dikirim dan, bila lolos, dari server |
| Nominal nol atau bukan angka | Pesan `VI-04` di bawah isian Nominal |
| Pegawai atau komponen ditolak server (`VI-02`, `VI-03`) | Pesan server di bawah isian terkait; id apa pun dalam pesan diganti kata "terpilih" |
| Baris diubah petugas lain (`VI-12`, 409) atau statusnya sudah berubah (`VI-09`, 409) | Form atau dialog ditutup, toast merah memuat pesan server, daftar dimuat ulang |
| Berkas impor tanpa kolom wajib | Pesan "Kolom tidak ditemukan: ..." dan impor tertahan |
| Berkas impor berisi nominal "seratus" | Daftar "Baris 2: Nominal "seratus" bukan angka." dan impor tertahan |
| Berkas lebih dari 1 MB atau lebih dari 2.000 baris | Ditolak sebelum dikirim (batas teknis, bagian 8) |
| Klik berulang saat permintaan berjalan | Semua tombol aksi, kolom pilih, dan tombol simpan/impor dinonaktifkan sampai respons diterima; satu permintaan tulis pada satu waktu |
| Peran tanpa `PayrollComponent : Read` | Daftar komponen tidak termuat; modal menampilkan peringatan |
| Peran tanpa `PayrollRun : ReadDetail` | Nomor run tidak tampil, status run tidak dikenal (layar dianggap menerima input; server tetap menegakkan `VI-01`), daftar pegawai untuk form kosong dan modal memberi peringatan |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- **Governance:** `AGENTS.md` frontend; `rules/frontend/*` (arsitektur, katalog dan gerbang keputusan base component,
  pola komposisi halaman, checklist UI, template laporan); `rules/rule-output/status-task-roadmap.md`.
- **Blueprint:** `03-frontend-architecture.md`, `contracts/api-contract.md`, `contracts/validation-matrix.md`,
  `contracts/state-transition-matrix.md`, `contracts/permission-audit-matrix.md`, `testing/acceptance-test-matrix.md`
  (`UAT-11`–`UAT-15b`), `roadmap/frontend-roadmap.md`, `roadmap/requirement-traceability.md`, laporan `PAY-BE-19`,
  `PAY-BE-20`, `PAY-FE-15`, dan `PAY-FE-16`.
- **Backend (baca-saja):** `PayrollVariableInputsController.cs`, `PayrollVariableInputService.cs`,
  `PayrollVariableInputDtos.cs`, `PayrollManualInputTypes.cs`, `PayrollRunStatuses.cs`, `PayrollVersionToken.cs`,
  `PayrollRunService.cs` (penjaga `VI-10` dan `VI-11`), `PayrollRunsController.cs`, `PayrollControllerSupport.cs`,
  `ApiResponse.cs`, `PayrollComponentController.cs` dan DTO-nya.
- **Rujukan frontend:** `payroll-transport-allowance-*` dan `salary-structure-allowance-*` (pola daftar + modal),
  `payroll-run-detail-view.jsx`, `use-payroll-run-detail.js`, `payroll-run-service.js`, `payroll-run-utils.js`,
  `payroll-run-constants.js`, `payroll-constants.js`, `payroll-section.jsx`, `payroll-run.module.css`,
  `receivable-invoice-batch-form-view.jsx` (kolom pilih), `row-action-menu.jsx`, `confirm-modal.jsx`,
  `base-form-control.jsx`, `data-table.jsx`, `use-select-resource.jsx`,
  `use-hemodialysis-schedule-dialog.jsx` (`NURSE_SELECT_RESOURCE`, pola konfigurasi pilihan lepas),
  `use-permission.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/payroll/payroll-variable-input-constants.js` | Baru. Endpoint, rute, hak akses, status dan jenis, lencana, nama aksi, kolom dan alias berkas impor, batas, konfigurasi pilihan Komponen, seluruh salinan teks termasuk kalimat kosong, gagal, dan peringatan dari skema |
| `src/utils/payroll/payroll-variable-input-utils.js` | Baru. Normalisasi respons, validasi `VI-03`–`VI-06`, payload, peringatan hitung ulang, pemetaan pesan ke isian, penguraian CSV, nominal berkas, pembacaan baris gagal impor, hasil massal |
| `src/lib/services/payroll/payroll-variable-input-service.js` | Baru. Sepuluh panggilan Axios lewat `InstanceAxios`; galat dilempar apa adanya supaya hook membaca `errors.failedRows` |
| `src/lib/hooks/payroll/use-payroll-variable-inputs.js` | Baru. Daftar, saringan, pemindaian peringatan hitung ulang, pilihan massal, aksi per baris dan massal, form, dialog alasan, kunci tulis tunggal, penanganan 409 |
| `src/lib/hooks/payroll/use-payroll-variable-input-import.js` | Baru. Pembacaan berkas, pengiriman impor, daftar baris gagal |
| `src/components/view/payroll/variable-input/payroll-variable-input-view.jsx` | Baru. Halaman: header, peringatan, saringan, bilah aksi massal, tabel, dialog alasan |
| `src/components/view/payroll/variable-input/payroll-variable-input-table-columns.jsx` | Baru. Kolom tabel, kolom pilih, menu aksi per baris |
| `src/components/view/payroll/variable-input/payroll-variable-input-form-modal.jsx` | Baru. Modal tambah/ubah |
| `src/components/view/payroll/variable-input/payroll-variable-input-import-modal.jsx` | Baru. Modal impor |
| `src/components/view/payroll/variable-input/payroll-variable-input-bulk-result-modal.jsx` | Baru. Hasil aksi massal per baris |
| `src/app/payroll/payroll-runs/[id]/variable-inputs/page.jsx` | Baru. Route tipis |
| `src/lib/hooks/payroll/use-payroll-run-detail.js` | Diubah. `canOpenVariableInputs` (`PayrollVariableInput : Read`) |
| `src/components/view/payroll/run/payroll-run-detail-view.jsx` | Diubah. Tombol **Penghasilan Variabel** di header, di samping Kembali |
| `tests/unit/payroll-variable-input.test.mjs` | Baru. 34 uji, termasuk pencocokan dengan source backend |

### 3.3 Kepatuhan arsitektur frontend

- **Pola:** sub-proses transaksi ter-scope ke run induk, bukan master data. Karena itu mengikuti pola Payroll V2 yang
  sama dengan `PAY-FE-15` dan `PAY-FE-16` (konstanta → utils → service → hook dengan state lokal → view), bukan standar
  tujuh berkas Redux master data. Tidak ada slice, butir menu, atau CSS Module baru.
- **Alur dependensi:** route → view → hook → service/utils/constants. View tidak memanggil Axios; utils murni.
- **Hak akses:** `usePermission` dengan resource `PayrollVariableInput`; tidak ada nama peran di kode.
- **Route anak tanpa butir menu:** sesuai skema bagian 2 (layar anak dari detail run).
- **Tanpa abstraksi baru:** tidak ada komponen shared, instance Axios, atau registry pilihan bersama yang diubah. Pilihan
  Komponen memakai konfigurasi pilihan lepas (pola `NURSE_SELECT_RESOURCE`).
- **Pola yang perlu disebut:** pembacaan waktu perubahan input dari token versi (lihat bagian 8).

### 3.4 Gerbang tampilan

**Modul referensi visual:** `payroll-transport-allowance-view.jsx` (daftar dan modal), `payroll-run-detail-view.jsx`
(susunan layar run), `receivable-invoice-batch-form-view.jsx` (kolom pilih untuk aksi massal).

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Header halaman | `Hero` | `REUSE` | — |
| Saringan, pencarian, tombol Tambah dan Impor | `DataFilter`, `FilterSelect`, `BaseButton` | `REUSE` | Pola daftar baku |
| Tabel berhalaman server | `DataTable`, `Pagination` — `tableStyles.regionDataTable` | `REUSE` | Pola daftar baku |
| Kolom pilih | `Form.Check` (react-bootstrap) | `REUSE` | Pola yang sama di tabel invoice batch |
| Menu aksi per baris | `RowActionMenu` | `REUSE` | Dipakai tabel keuangan; satu baris bisa memuat tiga aksi |
| Lencana status | `StatusBadge` dengan konfigurasi baru (data, bukan komponen) | `REUSE` | Pola `TRANSPORT_ALLOWANCE_BADGE_CONFIG` |
| Peringatan, galat, galat dalam modal | `InformationAlert`, `PayrollActionError` | `REUSE` | Dipakai di seluruh layar Payroll |
| Bilah aksi massal | `BaseButton` — `payroll-run.module.css` (`actionBar`, `actionNote`, `actionButtons`) | `REUSE` | CSS Module Payroll yang sudah ada |
| Form tambah/ubah, pilihan Pegawai dari pegawai di run | `ConfirmModal size="lg"` + `BaseSelectField`, `BaseTextField`, `BaseTextAreaField` | `REUSE` | `PAY-FE-15`, `PAY-FE-16` |
| Dialog Tolak dan Batalkan | `ConfirmModal` dengan `requireReason` dan `maxReasonLength` | `REUSE` | Pola baku alasan wajib |
| Hasil aksi massal dan daftar baris gagal impor | `ConfirmModal` + `PayrollNoticeList` | `REUSE` | Dipakai di detail run |
| Pilihan Komponen penghasilan | `BaseSelectField` + `useSelectResource` dengan konfigurasi lepas | `COMPOSE` | Tidak ada resource registry untuk komponen bertipe Earning; konfigurasi lepas di atas `payroll-components/options?componentType=Earning`, pola `NURSE_SELECT_RESOURCE`. Cari dan muat bertahap dari server |
| Pilih berkas impor | `BaseFormControl` membungkus input berkas | `COMPOSE` | Tidak ada base component unggah dokumen (`UploadPhotoField` khusus gambar) |
| Tombol masuk di detail run | `BaseButton` pada `actions` `Hero` | `REUSE` | Pola tombol masuk `PAY-FE-15` |

**Pilihan yang dicatat (`COMPOSE`, bukan `NEW`):** (1) Komponen — A (dipakai) konfigurasi lepas tanpa menyentuh registry
bersama; B menambah entri registry bersama, lebih rapi bila dipakai ulang tetapi mengubah berkas bersama untuk satu layar.
(2) Pilih berkas — A (dipakai) `BaseFormControl` dengan input berkas asli; B komponen unggah baru, membutuhkan persetujuan
dan berlebihan untuk satu layar. (3) Aksi per baris — A (dipakai) `RowActionMenu`; B tombol bertumpuk seperti `PAY-FE-15`,
tabel menjadi tinggi.

`UI GATE: PASS — 14 elemen: REUSE 12, EXTEND 0, COMPOSE 2, WRAP 0, NEW 0`

Hasil pemeriksa dan grep anti-regresi, ditempel apa adanya:

```text
$ node ../QuilvianEngineeringSkills/Claude/.claude/rules/frontend/tools/check-css-usage.mjs \
    .../variable-input/payroll-variable-input-view.jsx \
    .../variable-input/payroll-variable-input-table-columns.jsx \
    .../variable-input/payroll-variable-input-form-modal.jsx \
    .../variable-input/payroll-variable-input-import-modal.jsx \
    .../variable-input/payroll-variable-input-bulk-result-modal.jsx \
    .../run/payroll-run-detail-view.jsx
OK — 6 berkas diperiksa, 0 temuan.

$ grep -nE "<button|className=\"btn|btn-primary|btn-secondary" <5 berkas baru>           (kosong)
$ grep -n "<table" <5 berkas baru>                                                       (kosong)
$ grep -nE "fw-(bold|semibold|light)|fs-[0-9]" <5 berkas baru>                           (kosong)
$ grep -nE "<pola #7 class Tailwind>" <5 berkas baru>                                    (kosong)
$ grep -nE "fixed inset-0|position: ?fixed" <5 berkas baru>                              (kosong)
$ grep -nE "<(input|select|textarea)\b|UUID|GUID" <5 berkas baru>
payroll-variable-input-import-modal.jsx:58:          <input
$ grep -nE "variant=\"(outline|danger-outline|default|link|light|dark)\"|<StatusBadge[^>]*(status=\{[^}]*label|variant=)" <5 berkas baru>   (kosong)
$ grep -n "style={{" <5 berkas baru>                                                     (kosong)
$ grep -nEi "#[0-9a-f]{3,8}\b|rgba?\(" <5 berkas baru>                                   (kosong)
```

Satu temuan grep nomor 9 **dipertahankan dengan alasan**: input berkas asli di dalam `BaseFormControl` karena belum ada
base component unggah dokumen (checklist UI bagian G). Tidak ada stylesheet baru, sehingga grep warna literal, typography,
dan `!important` pada berkas gaya tidak berlaku (`NOT APPLICABLE`).

| Layar | Screenshot | Catatan perbandingan dengan referensi |
| --- | --- | --- |
| `/payroll/payroll-runs/[id]/variable-inputs` | `VISUAL CHECK: NOT FEASIBLE` | Dev server, build, dan skrip peramban tidak dijalankan: pemilik meminta agen tidak menyalakannya otomatis, dan agen tidak memegang sesi login. Susunan mengikuti layar daftar `PAY-FE-15`, tetapi **tampilannya belum pernah dilihat**. Yang paling perlu dilihat: tabel di dalam bungkus `regionTheme`, menu aksi baris, kolom pilih, dan input berkas di modal impor |
| `/payroll/payroll-runs/[id]` (tombol masuk) | `VISUAL CHECK: NOT FEASIBLE` | Alasan sama |

Catatan 2026-11-13: pemilik menguji layar di peramban lewat `npm run dev` dan melaporkan lulus; tidak ada screenshot yang disimpan, sehingga `VISUAL CHECK: NOT FEASIBLE` di atas tetap berlaku bagi agen.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tabel "Mengambil penghasilan variabel..."; tombol Tambah, Impor, kolom pilih, dan menu aksi dinonaktifkan |
| Kosong | "Belum ada penghasilan variabel di run ini." dan "Tambahkan satu per satu atau impor dari berkas." |
| Gagal | Kotak galat (pesan server atau "Data gagal dimuat.") dengan tombol **Coba lagi**; bila 403: "Anda tidak punya hak untuk tindakan ini." |
| Tanpa hak akses | Layar tidak tampil tanpa `Read`; Tambah, Impor, kolom pilih, dan aksi baris tidak tampil tanpa hak masing-masing |
| Run tidak menerima input | Keterangan baca-saja; tombol tulis, kolom pilih, dan menu aksi disembunyikan |
| Berisi | Tabel berhalaman (nomor, pegawai, komponen, jenis, nominal, status, aksi); alasan pada baris Ditolak dan Dibatalkan |
| Peringatan hitung ulang | Kotak kuning "Ada n input berubah setelah perhitungan terakhir. Hitung ulang run." |
| Permintaan berjalan | Seluruh tombol aksi dinonaktifkan; tombol simpan dan impor menampilkan "Memproses..." |
| Data basi (409) | Toast merah dengan pesan server dan daftar dimuat ulang |

## 5. Endpoint yang dikonsumsi

#### Corporate / Payroll Management / Payroll Variable Input

Base URL: `/v1/corporate/payroll-management/payroll-runs/{runId}/variable-inputs`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar berhalaman menurut saringan, dan pemindaian seluruh input run untuk peringatan hitung ulang | `PayrollVariableInput : Read` |
| `POST` | `/` | Tambah satu input Draf | `PayrollVariableInput : Create` |
| `POST` | `/import` | Impor semua atau tidak sama sekali | `PayrollVariableInput : Import` |
| `PATCH` | `/{id}/content` | Ubah isi input Draf; membawa `expectedRowVersion` | `PayrollVariableInput : Update` |
| `POST` | `/{id}/submit` | Ajukan satu input | `PayrollVariableInput : Submit` |
| `POST` | `/{id}/approve` | Setujui satu input | `PayrollVariableInput : Approve` |
| `POST` | `/{id}/reject` | Tolak dengan alasan | `PayrollVariableInput : Reject` |
| `POST` | `/{id}/cancel` | Batalkan dengan alasan | `PayrollVariableInput : Cancel` |
| `POST` | `/bulk-submit` | Ajukan banyak input | `PayrollVariableInput : Submit` |
| `POST` | `/bulk-approve` | Setujui banyak input | `PayrollVariableInput : Approve` |

`GET /{id}` tidak dipakai: respons aksi dan ubah sudah memuat baris terbaru.

#### Corporate / Payroll Management / Payroll Run

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/payroll-management/payroll-runs/{id}` | Nomor dan status run, `calculatedAt`, dan pegawai di run (pilihan Pegawai di form) | `PayrollRun : ReadDetail` |

#### Corporate / Payroll Management / Master Data / Payroll Component

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/payroll-management/master-data/payroll-components/options?componentType=Earning&onlyActive=true` | Pilihan Komponen di form | `PayrollComponent : Read` |

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada 13 berkas source baru atau berubah dan berkas test | 0 galat; 3 peringatan `react-hooks/set-state-in-effect`: 2 pada baris warisan `use-payroll-run-detail.js` (82 dan 104), 1 pada hook baru dengan pola yang sama seperti hook Payroll lain | `PASS` (peringatan: `EXISTING WARNING` 2, pola yang sama 1) | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/payroll-variable-input.test.mjs` | 34 uji, 34 lulus, 0 dilewati (source backend ada, jadi pencocokan ke backend ikut berjalan) | `PASS` | Keluaran perintah |
| Pencocokan alamat dengan `[Route]` controller backend; sub-alamat dan metode HTTP; tidak ada `DELETE` | Cocok | `PASS` | Uji "alamat endpoint", "sub-alamat dan metode HTTP" |
| Pencocokan hak akses dengan `ControllerName` dan seluruh `[AccessPermission]` | Delapan aksi sama | `PASS` | Uji "hak akses cocok" |
| Pencocokan nilai status dan jenis, aksi tersedia, status run penerima input, nama properti baris impor dan hasil, awal kalimat pesan `VI`, endpoint opsi Komponen dengan source backend | Cocok | `PASS` | Lima uji pada berkas yang sama |
| Kopling token versi: format `ticks` pada `PayrollVersionToken`, proyeksi `RowVersion`, dan penjaga `VI-11` pada `PayrollRunService` | Cocok; uji akan gagal bila salah satunya berubah | `PASS` | Uji "token versi adalah ticks .NET" |
| Peringatan hitung ulang: input sebelum, tepat sama, dan sesudah `calculatedAt`; status Dibatalkan; token tak terbaca; waktu tanpa zona | 2 dari 5 dihitung; sama dengan semantik `>` penjaga `VI-11` | `PASS` | Tiga uji peringatan |
| Validasi `VI-03`–`VI-06`, payload tambah dan ubah (token versi, lampiran dibawa), pemetaan pesan ke isian, id disamarkan | Sesuai | `PASS` | Uji validasi, payload, pemetaan |
| Aksi massal hanya mengirim baris yang memenuhi; hasil per baris menyebut nomor input dan pegawai | Sesuai | `PASS` | Dua uji aksi massal |
| Penguraian CSV (koma, titik koma, kutip, BOM, CRLF), nominal gaya Indonesia dan internasional, jenis input, judul kolom Indonesia dan Inggris, nomor baris, galat nominal | Sesuai | `PASS` | Lima uji impor |
| Pembacaan baris gagal impor dari respons 400 (`errors.failedRows`) | Terurut; id disamarkan; bentuk lain tidak meledak | `PASS` | Uji "baris gagal impor" |
| Pemeriksa statis sekali pakai (di luar repo): setiap simbol impor di 13 berkas terpetakan ke ekspor modul tujuannya | `OK — 13 berkas, semua impor terpetakan.` | `PASS` | Keluaran skrip |
| Suite unit penuh (`node --import ./tests/helpers/register.mjs --test tests/unit/*.test.mjs`) | 4.861 uji, 4.810 lulus, 51 gagal pada 35 berkas | `EXISTING / ENVIRONMENT ISSUE` (51), `PASS` (34 uji baru) | Keluaran perintah |
| Pembanding: himpunan (berkas, nama uji) yang gagal pada suite sebelum task ini (HEAD sama, `4.827` uji) | Identik: 0 kegagalan hanya sesudah perubahan, 0 hanya sebelum; seluruhnya juga gagal pada `HEAD` murni (dibuktikan pada `PAY-FE-16`) | `PASS` (tanpa kegagalan baru) | Selisih himpunan nama uji |
| Grep anti-regresi UI dan `check-css-usage.mjs` | 0 temuan; 1 `<input type="file">` dipertahankan di `BaseFormControl` | `PASS` | Bagian 3.4 |
| `npm run build` | Tidak dijalankan | `NOT RUN` | — |
| `npm run dev` dan uji manual oleh pemilik (2026-11-13) | Dilaporkan berhasil dan seluruh uji lulus. Tidak dirinci: layar dan akun yang dipakai, hasil per langkah uji; tidak ada screenshot tersimpan | `OWNER-REPORTED` — tidak diverifikasi agen; bukan pengganti `npm run build` | Pesan pemilik |
| Pembukaan layar di peramban dan screenshot | Tidak dilakukan | `NOT FEASIBLE` | Bagian 3.4 |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/payroll-variable-input.test.mjs — PASS (34/34)`

Uji manual: `NOT FEASIBLE` — agen tidak menyalakan `npm run dev` atau skrip peramban, dan tidak memegang sesi login.

**Langkah uji untuk pemilik** (setelah `npm run dev`; akun dengan `PayrollVariableInput : *`, `PayrollRun : ReadDetail`,
`PayrollComponent : Read`; run berstatus `Draft` atau `Calculated` dengan beberapa pegawai):

1. Buka detail Payroll Run. Harapan: tombol **Penghasilan Variabel** muncul. Tanpa `PayrollVariableInput : Read`, tombol tidak
   tampil.
2. Buka layar pada run tanpa input. Harapan: kalimat kosong persis dari skema; nomor run tampil di judul kecil.
3. **Tambah** satu Insentif: pilih pegawai, komponen penghasilan, nominal 1.000.000. Harapan: tersimpan sebagai Draf dengan
   nomor `PVI-...`; komponen bertipe potongan tidak muncul di pilihan.
4. Tambah Honor Tugas Tambahan tanpa catatan. Harapan: pesan `VI-06` di bawah isian "Tugas yang Dihonori" (`UAT-15`).
5. **Impor** berkas CSV tiga baris sah (judul: Nomor Pegawai, Kode Komponen, Jenis Input, Nominal, Catatan). Harapan:
   "3 baris data siap diimpor sebagai draf", lalu tiga Draf muncul. Coba juga berkas titik koma dari Excel Indonesia dengan
   nominal "1.250.000".
6. Impor berkas dengan dua baris salah (nomor pegawai tidak ada, nominal 0). Harapan: daftar "Baris n: ..." beserta sebab,
   tidak ada baris yang tersimpan (`UAT-12`). Impor berkas dengan nominal "seratus": daftar galat nominal tanpa pengiriman.
7. Centang beberapa Draf lalu **Ajukan terpilih**. Harapan: hasil per baris. Centang campuran Draf dan Diajukan: hanya
   yang memenuhi aksi dikirim, dan jumlah di tombol sesuai.
8. **Setujui** satu baris, **Tolak** satu baris tanpa lalu dengan alasan, **Batalkan** satu baris. Harapan: alasan wajib;
   status dan alasan tampil pada baris Ditolak dan Dibatalkan.
9. Pada run `Calculated`, setujui atau ubah satu input, lalu kembali ke detail run dan ajukan run. Harapan: layar input
   menampilkan peringatan hitung ulang; pengajuan run ditolak `VI-11`; setelah hitung ulang peringatan hilang dan pengajuan
   diterima (`UAT-14`). Biarkan satu input Diajukan lalu ajukan run: ditolak `VI-10` (`UAT-13`).
10. Ajukan run lalu buka layar input. Harapan: baca-saja, keterangan tampil, tanpa Tambah, Impor, kolom pilih, dan menu aksi.
11. Buka dua tab pada baris yang sama; ubah dan simpan di tab A, ubah dan simpan di tab B. Harapan: toast "Data ini baru
    saja diubah petugas lain. Muat ulang lalu coba lagi.", form tertutup, daftar dimuat ulang.
12. Akun yang hanya punya `Read`: daftar tampil tanpa tombol tulis, kolom pilih, dan menu aksi.
13. Matikan backend lalu muat ulang. Harapan: "Data gagal dimuat." dan tombol Coba lagi.

Uji manual oleh pemilik (2026-11-13): `OWNER-REPORTED`, lulus. Pemilik menjalankan `npm run dev` dan melaporkan seluruh uji lulus; hasil per langkah tidak dirinci dan tidak diverifikasi agen. `npm run build` tetap `NOT RUN`.

**Tidak dijalankan oleh agen:** `npm run build`, `npm run dev`, uji e2e, uji manual dan pemeriksaan visual di peramban, `dotnet build`
backend.

## 7. Acceptance criteria dan Definition of Done

Ringkasan: **6 dari 6** kriteria terpetakan ke source dan bukti statis; **6 dari 6** dinyatakan lulus oleh pemilik di peramban (2026-11-13, tanpa rincian per langkah; bukan bukti agen).

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Tombol aksi mengikuti `availableActions` dan hak akses | Terpenuhi di source; pengujian pemilik: lulus (2026-11-13, tanpa rincian) | `buildVariableInputColumns`: tiap aksi memerlukan hak (`Update`, `Submit`, `Approve`, `Reject`, `Cancel`) **dan** muncul pada `availableActions`; Tambah dan Impor memerlukan `Create` dan `Import`; uji kecocokan aksi dan hak dengan backend |
| 2. Impor dengan baris salah menampilkan daftar baris dan sebabnya; tidak ada baris tersimpan | Terpenuhi di source; pengujian pemilik: lulus (2026-11-13, tanpa rincian) | `readImportFailedRows` dan modal impor; "tidak ada baris tersimpan" dijamin backend (`VI-08`, satu `SaveChanges`) dan dikunci hanya oleh kontrak, bukan diuji di layar |
| 3. Aksi massal menampilkan hasil per baris | Terpenuhi di source | `PayrollVariableInputBulkResultModal`; uji `describeBulkResult` |
| 4. Peringatan hitung ulang tampil bila input berubah setelah `calculatedAt` | Terpenuhi di source dan uji logika; pengujian pemilik: lulus (2026-11-13, tanpa rincian) | `countInputsChangedAfterCalculation`, pemindaian seluruh input run, uji kopling token versi dengan source backend |
| 5. Run yang tidak menerima input menjadikan layar baca-saja | Terpenuhi di source | `runAcceptsVariableInputs` (sama dengan `AcceptsInputs` backend, diuji); tombol tulis, kolom pilih, dan menu aksi disembunyikan; `availableActions` dari server juga kosong pada run tertutup |
| 6. Tombol dinonaktifkan selama permintaan berjalan | Terpenuhi di source | Kunci tulis tunggal `actionLockRef` dan `busy` pada semua tombol dan kolom pilih; tombol simpan dan impor memuat |
| DoD: kriteria terbukti | Terpenuhi menurut pernyataan pemilik (2026-11-13); build produksi belum (`NOT RUN`) | `OWNER-REPORTED`; bagian 6 |
| DoD: jalan masuk ada | Terpenuhi | Tombol **Penghasilan Variabel** di detail run |
| DoD: laporan ditulis | Terpenuhi | Berkas ini |
| Verifikasi roadmap: lint | Terpenuhi (0 galat) | Bagian 6 |
| Verifikasi roadmap: build | **Belum** | `NOT RUN` |
| Verifikasi roadmap: uji manual `UAT-11`–`UAT-14` | Terpenuhi menurut pernyataan pemilik (2026-11-13), tanpa rincian per langkah | `OWNER-REPORTED` |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 3 peringatan ESLint `react-hooks/set-state-in-effect`; 2 pada baris warisan, 1 pada hook baru dengan pola yang sama seperti hook Payroll V2 lain. Tidak diperbaiki di luar cakupan |
| Selisih desain dengan backend: waktu perubahan input | Skema meminta peringatan dari "pembandingan `calculatedAt` run dengan waktu perubahan input", tetapi respons input tidak memuat waktu perubahan. Layar membacanya dari token versi (`rowVersion`), yang adalah ticks .NET dari `UpdateDateTime ?? CreateDateTime`, nilai yang sama dengan yang dibandingkan penjaga `VI-11`. Ini mengikat layar pada format token. Uji unit mengunci tiga hal pada source backend (format token, proyeksi `RowVersion`, penjaga `VI-11`) dan gagal bila berubah; token yang tidak terbaca diabaikan, dan server tetap menolak pengajuan run (`VI-11`). Solusi tahan lama: backend menambah `updatedAt` pada `PayrollVariableInputResponse`. Perlu keputusan pemilik |
| Peringatan mencakup seluruh input run | Pemindaian memuat paling banyak 10 halaman x 200 = 2.000 input per pemuatan. Run dengan input lebih banyak dapat menampilkan jumlah yang kurang; penjaga `VI-11` di server tetap menahan pengajuan |
| Peringatan mengikuti semantik `VI-11` | Input berstatus apa pun dihitung, termasuk yang ditolak atau dibatalkan sesudah perhitungan, sesuai tafsiran `PAY-BE-20`. Peringatan bisa terasa ketat, tetapi sama dengan penolakan server |
| Impor memakai berkas CSV | Skema berbunyi "unggah baris" tanpa menetapkan format. Dipilih CSV (koma atau titik koma, judul kolom Indonesia atau Inggris) dengan penguraian di peramban, karena tidak ada pustaka spreadsheet di proyek dan tidak boleh ditambah di task ini. Berkas Excel (`.xlsx`) belum didukung; petugas menyimpannya sebagai CSV. Tidak ada tombol unduh templat; kolom dijelaskan di modal. Nominal "1.500" dibaca 1.500 (gaya Indonesia), bukan 1,5 |
| Batas teknis impor | Berkas paling besar 1 MB dan paling banyak 2.000 baris, sebagai pelindung peramban dan ukuran kiriman (`DEV_DISCRETION`). Kontrak dan backend tidak membatasi baris, dan batas bisnis tidak dikarang di sini. Backend menerbitkan nomor satu per satu (`PAY-BE-19`), sehingga impor besar bisa lambat; umpan balik progres berupa tombol "Memproses..." selama satu permintaan, bukan persentase |
| Lampiran | `attachmentPath` ada pada kontrak, tetapi tidak ada endpoint unggah dan skema tidak menyebutnya. Layar tidak menyediakan isian lampiran; nilai yang sudah ada dibawa apa adanya saat ubah supaya tidak terhapus |
| Daftar pegawai untuk form | Diambil dari `employees` pada detail run, sehingga membutuhkan `PayrollRun : ReadDetail`. Tanpa hak itu, form Tambah tidak dapat memilih pegawai dan menampilkan peringatan. Impor tidak terpengaruh (memakai nomor pegawai) |
| Hak akses yang harus diberikan admin | `PayrollVariableInput : Read/Create/Import/Update/Submit/Approve/Reject/Cancel`, `PayrollComponent : Read` (pilihan Komponen), dan `PayrollRun : ReadDetail`. Usulan peran Staf Payroll di `permission-audit-matrix.md` bagian 3 memuat yang pertama, tetapi **tidak** memuat `PayrollComponent : Read` dan `PayrollRun : ReadDetail`. Perlu ditambahkan pada usulan peran |
| Draf yang tertinggal | `PAY-BE-20` mencatat input Draf yang tertinggal saat run disahkan tetap Draf di run terkunci, dan menyarankan layar mengingatkan petugas. Peringatan itu belum dibuat di sini karena di luar kriteria kartu. Usulan task kecil berikutnya |
| Pesan penolakan dipasangkan lewat awal kalimat | Sama seperti `PAY-FE-16`: backend mengirim `VI-xx` sebagai teks bebas. Uji unit mengunci awal kalimat terhadap source backend. Bila backend mengubah kalimatnya, pesan tetap tampil sebagai galat umum; yang hilang hanya penempelan ke isian |
| Kosong: tombol Tambah | Skema menaruh tombol "Tambah" di dalam keadaan kosong. Di sini tombol Tambah dan Impor ada di bilah saringan di atas tabel, tidak di dalam keadaan kosong, karena `DataTable` hanya menerima teks pada keadaan kosong |
| Masalah yang diketahui | (1) Pilihan massal dibersihkan saat saringan atau halaman berganti; pilihan lintas halaman tidak didukung. (2) Setelah aksi, daftar dan run dimuat ulang (dua sampai tiga permintaan) selain pembaruan lokal baris. (3) Toast layar ini berdiri sendiri. (4) Layar sudah diuji pemilik (2026-11-13, lulus, tanpa rincian), tetapi agen belum pernah melihatnya; `Form.Check` pada header tabel dan menu aksi `RowActionMenu` di dalam tabel belum diverifikasi tampilannya |
| Dependency backend | `PAY-BE-19` dan `PAY-BE-20` 🟡: source dibaca dan dicocokkan, tetapi `dotnet build` dan uji API belum dijalankan pemilik. Seluruh perilaku layar bergantung pada backend itu berjalan |
| Perubahan sampingan | `NONE`. Berkas scratch (pemeriksa statis, ringkasan lint, perbandingan kegagalan) ada di folder scratch di luar repo |
| Interupsi | `NONE` |
| Status Git | Frontend, task ini: ` M` `payroll-run-detail-view.jsx`, ` M` `use-payroll-run-detail.js`; `??` route `[id]/variable-inputs/`, folder `view/payroll/variable-input/` (5 berkas), konstanta, 2 hook, service, utils, dan `tests/unit/payroll-variable-input.test.mjs`. Perubahan `PAY-FE-16` yang belum di-commit masih ada. Tidak ada yang di-stage atau di-commit. Backend, dari task ini: `??` laporan ini; ` M` `frontend-roadmap.md` dan `requirement-traceability.md` (tanda status). `??` `PAY-FE-16.md` berasal dari task sebelumnya; ` M` `PayrollSimulationService.cs` bukan dari task ini |
| Langkah berikutnya | Status ✅ atas instruksi pemilik (2026-11-13). Sisa: `npm run build` produksi (`NOT RUN`) dan pembuktian build serta uji API backend `PAY-BE-19` dan `PAY-BE-20` (masih 🟡). |
