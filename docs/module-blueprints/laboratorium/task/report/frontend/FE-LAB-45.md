# Laporan Perubahan Frontend — `FE-LAB-45`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-45` |
| Judul | Unduhan laporan |
| Slice | Gelombang `MVP-11b` — `EPIC-LAB-17`, `S16a` tiga laporan operasional; penutup gelombang |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) gelombang `MVP-11`, bagian `FE-LAB-45` |
| Trace | Bagian tampilan `FR-17.7`, `FR-17.8`; `FR-17.10` (tombol); `LAB-DEC-160`; A7.9; `02-backend-architecture.md` 23.10 butir 1, 2, 7 |
| Contract version | `LAB-API-v1` **`r37`** 32.2 (tiga `GET …/export`) dan 32.3 (format CSV); `LAB-PERM-v1` **revision 12** 14.2 — **`approved` 2026-09-28** |
| Wewenang UI | Disetujui: tombol hanya bagi pemegang `Export`, berkas dari backend disimpan apa adanya, nol pembentukan berkas di layar (A7.9). `DEV_DISCRETION` yang dipakai: tombol *Unduh CSV* di kanan atas setiap bagian laporan; pembantu simpan berkas privat di dalam hook unduh (bukan pembantu bersama baru) |
| Dependency | `FE-LAB-44` ⚠ (service, berkas aturan, dan ketiga bagian); `BE-LAB-86` ✅ |
| Klasifikasi | `MEDIUM` — 3 berkas baru (hook unduh, CSS module tata letak, laporan ini), 6 berkas `FE-LAB-44` diperluas; 3 endpoint unduh; nol Redux, nol dependency baru |
| Task mode | `FRONTEND` — backend baca saja |
| Target tulis | `QuilvianSystemFrontendDev` (source); `NewQuilvianSystemBackend` hanya laporan ini serta tautan buktinya pada roadmap frontend dan `traceability.md` |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `68195b2be` (branch `YogaV2`, upstream `origin/YogaV2`), di atas perubahan `FE-LAB-44` yang belum ter-commit |
| Commit backend yang dijadikan rujukan | `7ff35b8c` (branch `yoga`) beserta `BE-LAB-86` yang belum ter-commit — `LabOperationalReportController.UnduhAsync`, `Program.cs` (kebijakan CORS) |
| Tanggal | 2026-09-30 |
| Status | ⚠ **`SELESAI DENGAN BATAS VERIFIKASI`** — tombol *Unduh CSV* per laporan, hanya bagi pemegang `Export`; berkas backend disimpan apa adanya. Uji unit **22/22**, e2e layar **13/13** (8 `FE-LAB-44` + 5 unduhan), lint dan build hijau. **Tiga berkas CSV sungguhan backend** (hasil verifikasi `BE-LAB-86`) diunduh lewat layar hasil build dan tersimpan **identik byte demi byte**, dengan nama sesuai pola backend. **Batas:** unduhan tersambung langsung dengan akun asli menunggu langkah rilis `MVP-11c` 0-1; berkas belum dibuka di Excel sungguhan. **`MVP-11b` selesai pada kode** |

---

## 1. Keadaan yang ditemukan di awal

| Yang ditemukan | Bukti |
| --- | --- |
| Layar laporan (`FE-LAB-44`) sudah ada, tanpa tombol unduh | `lab-operational-report-view.jsx` |
| Backend mengirim CSV (`text/csv`) dengan `Content-Disposition` `laporan-<jenis>_<awal>_<akhir>.csv`; galat `400`/`422` berupa JSON `ApiResponse` | `LabOperationalReportController.UnduhAsync`; header asli pada verifikasi `BE-LAB-86` |
| **`Content-Disposition` tidak terbaca browser di lingkungan saat ini** | `InstanceAxios` memanggil API lewat URL absolut lintas origin (`NEXT_PUBLIC_API_QUILVIAN`), sedangkan kebijakan CORS backend (`Program.cs` `FrontendCorsPolicy`) tidak memanggil `WithExposedHeaders` |
| Belum ada pembantu unduh berkas bersama di repository | `grep` `.download =`, `content-disposition` — nol hasil |
| Pendengar `quilvian:access-denied` (yang memanggil `alert()`) **tidak terpasang** di mana pun | `grep AccessDeniedListener` — hanya definisinya |

---

## 2. Proses bisnis dari sisi pengguna

**Siapa:** kepala instalasi dan manajemen yang memegang `LabOperationalReport : Export`.

**Alur normal:**

1. Kepala instalasi membuka *Laporan Operasional*, memilih periode dan — bila perlu — disiplin.
2. Setelah laporan tampil, tombol **Unduh CSV** di kanan atas setiap bagian menjadi aktif.
3. Ia menekan *Unduh CSV* pada *Penolakan Wadah*. Tombol berubah menjadi *Mengunduh...* dan nonaktif.
4. Berkas `laporan-penolakan-wadah_2026-09-01_2026-09-30.csv` tersimpan di folder unduhan — isinya persis
   berkas buatan backend: baris periode, judul kolom Bahasa Indonesia, pemisah titik koma, angka *20,0*.
5. Backend mencatat satu baris log unduhan (`BE-LAB-86`).

**Berkas selalu sesuai layar:** unduhan memakai parameter laporan yang **sedang tampil**, dan tombol nonaktif
selama laporannya belum tampil — pengguna tidak dapat mengunduh angka periode lain dari yang ia lihat.

**Jalur tidak normal:**

| Keadaan | Yang terjadi |
| --- | --- |
| Pemegang `Read` saja | Tombol **tidak tampil**; laporan tetap terbaca |
| Tombol diklik dua kali cepat | **Tepat satu** permintaan — backend mencatat satu unduhan |
| Periode ditolak (`400`/`422`) | Pesan backend apa adanya di bawah tombol, misalnya *"Periode laporan paling panjang 366 hari. Persempit rentang tanggalnya."* — bukan *"[object Blob]"* |
| Unduhan ditolak `403` | Pesan di bawah tombol; **halaman tidak ditutup** — laporannya tetap boleh dibaca |
| Galat lain | *"Laporan gagal diunduh. Coba lagi beberapa saat lagi."* |
| Penyaring diganti sesudah unduhan gagal | Pesan lama hilang — ia milik laporan yang diminta, bukan laporan baru |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `frontend-roadmap.md` `FE-LAB-45`; `03-frontend-architecture.md` amandemen keempat (*Unduh*, *Unduhan gagal*); `api-contract.md` 32.2-32.3 | Cakupan, verifikasi, jebakan |
| Backend `LabOperationalReportController.cs` (`UnduhAsync`), `Program.cs` (CORS) | Nama berkas, jenis isi, bentuk galat, terbaca-tidaknya header |
| `InstanceAxios.jsx` | URL absolut lintas origin; siaran `quilvian:access-denied` pada `403` |
| `app-notification-provider.jsx` | Pendengar siaran `403` — tidak terpasang |
| `base-grouped-editor-field.jsx`, `self-service-profile-page.jsx`, `queue-voice-player.js` | Pola `URL.createObjectURL`/`revokeObjectURL` |
| `lab-worklist-view.jsx` dan `lab-worklist.module.css` | Pola tata letak baris tombol (`heroActions`, token `--space-3`) |
| `base-button.jsx` | `loading`, `loadingLabel`, penerusan `aria-label` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/.../lab-operational-report-constants.jsx` | `LAB_OPERATIONAL_REPORT_PERMISSION.export`; `LAB_OPERATIONAL_REPORT_FILE_PREFIX` — awalan nama berkas **disalin dari backend**; salinan teks tombol dan pesan |
| `src/lib/services/.../lab-operational-report.service.js` | `downloadLabOperationalReport` — `responseType: "blob"`, parameter dibersihkan dengan aturan yang sama dengan baca; mengembalikan `Blob` dan `Content-Disposition` |
| `src/lib/hooks/.../lab-operational-report-rules.js` | `fileNameFromContentDisposition` (`filename*` lalu `filename`, penanda folder dibuang), `fallbackFileName`, `readErrorBodyMessage` (badan galat `Blob` dibaca sebagai teks JSON), `classifyExportError`. **Koreksi `FE-LAB-44`:** `classifyReportError` tidak lagi menampilkan pesan bawaan Axios berbahasa Inggris (*"Request failed with status code 500"*) — pesan backend atau kalimat baku |
| `src/lib/hooks/.../use-lab-operational-report.jsx` | Mengekspos `requestParams` — parameter laporan yang sedang tampil |
| `src/lib/hooks/.../use-lab-operational-report-export.jsx` | **Baru** — izin `Export`, unduhan per laporan dengan `AbortController`, penjaga satu-klik-satu-permintaan (ref, ditulis seketika), pesan gagal bertanda parameter pemintanya, penyimpan berkas privat (`createObjectURL` → tautan sementara → `revokeObjectURL`) |
| `src/components/view/.../lab-operational-report-view.jsx` | Komponen lokal `ReportDownload` di awal badan ketiga bagian: `BaseButton` sekunder kecil ber-`aria-label` *"Unduh CSV <judul laporan>"*, nonaktif tanpa data, pesan gagal lewat `InformationAlert` |
| `src/style/health-services/laboratory-management/lab-operational-reports/lab-operational-report.module.css` | **Baru** — satu kelas tata letak `sectionActions` (flex, rata kanan, `--space-3`); nol warna, nol tipografi |
| `tests/unit/lab-operational-report-rules.test.mjs` | +5 uji |
| `tests/e2e/lab-operational-report-screen.spec.mjs` | +5 skenario unduhan; mock jalur `…/export` |

**Ketiga jebakan roadmap.**

| Jebakan | Bagaimana dihindari | Bukti |
| --- | --- | --- |
| Membentuk CSV di layar — unduhan lolos tanpa tercatat | Berkas `Blob` dari backend disimpan apa adanya; nol `new Blob`/`text/csv`/penggabungan `;` di layar | Grep; tiga berkas identik byte demi byte dengan berkas backend |
| *"[object Blob]"* atau pesan umum karena badan galat tidak dibaca | `readErrorBodyMessage` membaca `Blob` sebagai teks lalu JSON | Uji unit; e2e dan respons `422` sungguhan: pesan backend tampil, nol *"[object Blob]"* |
| Melupakan `URL.revokeObjectURL` | Dilepas 1 detik sesudah klik tautan | `use-lab-operational-report-export.jsx:37` |

**Keputusan kecil.**

| Hal | Keputusan dan alasan |
| --- | --- |
| Nama berkas cadangan | Pola backend dari periode yang **dikirim** — karena `Content-Disposition` tidak terbaca lintas origin, jalur inilah yang terpakai hari ini; hasilnya sama dengan nama dari backend |
| Pembantu simpan berkas | Privat di dalam hook unduh, bukan pembantu bersama baru — `AGENTS.md` melarang abstraksi bersama tanpa permintaan; ia dapat diangkat bila modul lain membutuhkannya |
| `403` unduhan | Pesan di bagian itu, **bukan** gerbang halaman — pemegang `Read` tetap berhak membaca |
| Tombol muncul menurut `usePermission` | Mengikuti konvensi modul (`allowed` benar selama daftar kewenangan belum diketahui); penegakannya `403` backend |

### 3.3 Kepatuhan arsitektur frontend

- Alur: view → hook unduh → service → `InstanceAxios`. View nol Axios.
- Hook dengan satu fokus: unduhan dipisah dari hook laporan; view memberikan `requestParams` hook laporan kepada
  hook unduh.
- CSS module fitur hanya untuk tata letak baris tombol — pola yang sama dengan `heroActions` Daftar Kerja.

`UI GATE: 2 elemen — REUSE 1, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Tombol unduh per laporan dengan keadaan memuat | `BaseButton` (`loading`, `loadingLabel`, `disabled`, `aria-label` lewat `...props`) | `base-button.jsx:20-69` | REUSE | Varian `secondary`, ukuran `sm` |
| Baris tombol beserta pesan gagal di awal bagian | `BaseButton` + `InformationAlert` di dalam `BaseDetailSection`, kelas tata letak `sectionActions` | Pola `heroActions` Daftar Kerja | COMPOSE | Rata kanan, token `--space-3` |

Letak tombol disajikan sebagai pilihan: (A) di awal badan setiap bagian — rekomendasi, pesan gagal langsung di
dekat laporannya; (B) tiga tombol di `Hero` — tanpa CSS baru, tetapi pesan gagal jauh dari laporannya; (C) satu
tombol unduh-semua — melanggar *satu tombol per laporan*. Dijalankan A.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tombol berlabel *Mengunduh...*, nonaktif, `aria-busy`; tombol nonaktif pula selama laporannya belum tampil |
| Kosong | `NOT APPLICABLE` — laporan kosong tetap dapat diunduh sebagai berkas berisi baris periode dan judul kolom, sama dengan backend |
| Gagal | Pesan backend di bawah tombol, atau *"Laporan gagal diunduh. Coba lagi beberapa saat lagi."*; pemulihan: tekan lagi |
| Tanpa hak akses | Tanpa `Export`: tombol tidak tampil. `403` dipaksakan: pesan backend atau *"Anda tidak memiliki izin mengunduh laporan ini."* di bawah tombol |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Operational Report

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-operational-reports/examination-count/export` | Tombol unduh bagian Jumlah Pemeriksaan | `LabOperationalReport : Export` |
| `GET` | `/v1/health-services/laboratory-management/lab-operational-reports/specimen-rejection/export` | Tombol unduh bagian Penolakan Wadah | `LabOperationalReport : Export` |
| `GET` | `/v1/health-services/laboratory-management/lab-operational-reports/turnaround-time/export` | Tombol unduh bagian Waktu Penyelesaian | `LabOperationalReport : Export` |

Parameter sama persis dengan laporan yang tampil: `startDate`, `endDate`, `discipline` (kosong tidak dikirim).
Respons `200` berkas `text/csv`; galat `400`/`403`/`422` berupa JSON `ApiResponse` yang datang sebagai `Blob`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint --max-warnings=0` pada seluruh berkas yang disentuh | Nol error, nol warning | `PASS` | Keluaran perintah |
| `npm run lint:errors` (seluruh repository) | Exit 0 | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/lab-operational-report-rules.test.mjs` | **22/22** | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 2079 uji: 2072 lolos, **7 gagal — tujuh kegagalan lama yang sama persis dengan baseline `FE-LAB-44`** (akuntansi, petty cash, Bank Darah M0, empat Hemodialisa) | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah; [`FE-LAB-44.md`](FE-LAB-44.md) bagian 6 |
| `npm run build` | Exit 0 (77 detik) | `PASS` | Keluaran build |
| `npx playwright test tests/e2e/lab-operational-report-screen.spec.mjs` terhadap build standalone | **13/13** — 8 skenario `FE-LAB-44` utuh, 5 unduhan | `PASS` | Keluaran perintah |
| Tiga berkas CSV **sungguhan** backend (verifikasi `BE-LAB-86`) diunduh lewat layar hasil build, header `Content-Disposition` asli tanpa `Access-Control-Expose-Headers` | Ketiganya **identik byte demi byte** — 290/290, 297/297, 474/474 byte, SHA-256 sama; nama `laporan-jumlah-pemeriksaan_…`, `laporan-penolakan-wadah_…`, `laporan-waktu-penyelesaian_2026-09-01_2026-09-30.csv` | `PASS` | Skrip Playwright; berkas tersimpan |
| Respons `422` **sungguhan** pada unduhan | Pesan backend tampil di bawah tombol bagian Waktu Penyelesaian; laporan tetap tampil; nol *"[object Blob]"* | `PASS` | Tangkapan layar |
| Grep anti-regresi | Nol warna, tipografi, `!important` di CSS baru; nol tombol/tabel mentah; nol pembentukan CSV di layar | `PASS` | Keluaran perintah |

**Verifikasi manual** — Chromium (Playwright) terhadap build standalone, zona `Asia/Jakarta`:

| Skenario | Hasil sebenarnya |
| --- | --- |
| Pemegang `Read` saja | Nol tombol *Unduh CSV*; laporan tetap tampil |
| Pemegang `Read` + `Export` | Tiga tombol, satu per bagian |
| Dua klik cepat pada *Penolakan Wadah* | Tombol *Mengunduh...* dan nonaktif selama berjalan; **tepat satu** permintaan unduhan |
| Isi berkas | Tiga byte pertama `EF BB BF`; memuat *Patologi Klinik;400;12;3,0* — byte yang sama dengan yang dikirim |
| Nama berkas tanpa header terbaca | `laporan-penolakan-wadah_<tanggal 1 bulan ini>_<hari ini>.csv` |
| Nama berkas bila header terbaca (diekspos) | Nama dari `Content-Disposition` dipakai |
| Parameter unduhan | Sama persis dengan permintaan laporan yang tampil (`startDate`, `endDate`, `discipline`) |
| Galat `422` / `403` | Pesan backend di bagiannya; `403` tidak menutup halaman; nol *"[object Blob]"* |
| Ganti disiplin sesudah galat | Pesan lama hilang |
| Laporan belum tampil | Tombol bagian itu nonaktif; aktif begitu laporannya datang |

Uji manual: `PASS` — dengan batas di bawah.

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/lab-operational-report-rules.test.mjs — PASS`

`MANUAL TEST: PASS pada build produksi dengan berkas dan galat backend sungguhan yang dimasukkan lewat Playwright; unduhan tersambung langsung dengan akun asli dan pembukaan di Excel NOT FEASIBLE — lihat di bawah`

**Tidak dijalankan:**

- **Unduhan tersambung langsung ke backend dengan akun kepala instalasi dan pemegang `Read` saja, beserta
  pemeriksaan satu baris log per dua klik cepat di backend.** Build frontend mengarah ke API dev bersama yang
  belum menerima `MVP-11a`, dan belum ada jabatan pemegang izin laporan (langkah rilis `MVP-11c` 0-1).
  Satu-permintaan dibuktikan di sisi layar; satu-permintaan = satu-log dibuktikan `BE-LAB-86`.
- **Membuka berkas di Excel.** Tidak tersedia di lingkungan ini. Berkas terbukti identik dengan berkas backend,
  yang formatnya — BOM UTF-8, titik koma, desimal koma — dirancang `BE-LAB-86` untuk Excel berbahasa Indonesia.
- Memanggil endpoint unduh backend lagi untuk task ini — dipakai berkas yang sudah diunduh saat verifikasi
  `BE-LAB-86`, keluaran backend yang sama. *Dikoreksi 2026-09-30:* alasan semula (*"setiap unduhan menulis log
  ke database dev bersama"*) keliru — pencatat `LoggerService` hanya menulis ke log aplikasi (sink Serilog
  `Console` dan `File`, `Program.cs`), bukan ke database. Unduhan dengan akun asli dijalankan pada langkah rilis
  `MVP-11c` di dev.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Matriks layar — *Tombol unduh*: pengguna tanpa `Export`, tombol tidak tampil | Terpenuhi | e2e |
| `AC-253` bagian antarmuka — `Read` tanpa `Export` | Terpenuhi pada build; akun asli **belum** | e2e |
| Baris *Unduhan gagal* `03-frontend-architecture.md` — pesan pada tombol; ulangan tidak menggandakan unduhan yang berhasil | Terpenuhi | e2e: pesan per bagian; dua klik = satu permintaan |
| Verifikasi roadmap — uji unit nama berkas cadangan dan pembaca pesan galat dari `Blob` | Terpenuhi | 22/22 |
| Verifikasi roadmap — berkas terbuka di Excel, kolom terpisah, *3,0* | Isi berkas terbukti identik dengan berkas backend; pembukaan di Excel **belum** | Bagian 6 |
| Verifikasi roadmap — tepat satu log walau diklik dua kali cepat | Satu permintaan terbukti di layar; log live **belum** (menunggu `MVP-11c`) | e2e; `BE-LAB-86` |
| DoD — tiga unduhan berjalan; tombol mengikuti izin `Export`; nol pembentukan berkas di layar; laporan | Terpenuhi | Bagian 3 dan 6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol warning lint pada berkas yang disentuh; build tanpa error |
| Masalah yang diketahui | **1. Temuan backend, tidak diperbaiki (mode `FRONTEND`):** `FrontendCorsPolicy` tidak mengekspos `Content-Disposition`, sehingga nama dari backend tidak terbaca lintas origin. Dampak hari ini nol — nama cadangan sama persis dengan pola backend — tetapi bila awalan nama berkas backend berubah, konstanta `LAB_OPERATIONAL_REPORT_FILE_PREFIX` wajib ikut diubah. Perbaikan satu baris (`WithExposedHeaders("Content-Disposition")`) milik task backend. **2.** Tombol dapat tampil sesaat bagi pemegang `Read` saja selama daftar kewenangan sesi belum termuat — konvensi `usePermission`; backend menolak dengan `403` |
| Dependency backend | Nol task backend yang belum selesai. Pemakaian sungguhan menunggu langkah rilis `MVP-11c` 0-1 (deploy `MVP-11a`, pemberian `Read` dan `Export` kepada kepala instalasi) |
| Perubahan sampingan | Playwright kembali membersihkan `test-results/` ter-track (`.last-run.json` berubah, satu `error-context.md` Rawat Inap terhapus). **Dipulihkan** ke isi HEAD dalam CRLF; `git status` bersih dari keduanya |
| Interupsi | `NONE` |
| Status Git | Frontend: ` M` `laboratory-constants.jsx`, `menu-items.jsx`; `??` `lab-operational-reports/` (route dan view), `lab-operational-report-constants.jsx`, `lab-operational-report-rules.js`, `use-lab-operational-report.jsx`, **`use-lab-operational-report-export.jsx`**, `lab-operational-report.service.js`, **`src/style/health-services/laboratory-management/lab-operational-reports/`**, spec e2e, uji unit — seluruhnya `FE-LAB-44` dan `FE-LAB-45`, belum ter-commit. Backend: laporan ini, `frontend-roadmap.md`, `traceability.md`. **Nol operasi Git dijalankan** |
| Langkah berikutnya | `MVP-11b` selesai pada kode. Langkah rilis `MVP-11c`: deploy `MVP-11a` + `MVP-11b`, lalu admin memberi `LabOperationalReport : Read` dan `Export` kepada jabatan kepala instalasi (langkah 1) dan menjalankan pemeriksaan langkah 3 dengan akun asli |
