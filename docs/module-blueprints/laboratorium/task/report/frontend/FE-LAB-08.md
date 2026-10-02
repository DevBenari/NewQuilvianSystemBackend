# Laporan Perubahan Frontend — `FE-LAB-08`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-08` |
| Judul | Layar daftar kerja dan pantau keterlambatan |
| Slice | `S7` — daftar kerja dan pemantauan keterlambatan (`roadmap/frontend-roadmap.md` bagian 6, gelombang `MVP-3`) |
| Roadmap | `docs/module-blueprints/laboratorium/roadmap/frontend-roadmap.md` bagian 6 |
| Trace | `FR-07.3`, `FR-04.1` .. `FR-04.3`; `LAB-FE-006`; `VAL-39`; `AC-10`, `AC-17`, `AC-39` |
| Contract version | `LAB-API-v1` r3 grup Lab Worklist — `approved`, dikunci 2026-09-02 |
| Wewenang UI | `LAB-FE-006` adalah **invariant keselamatan**: bentuk visualnya `DEV_DISCRETION`, **keberadaannya tidak** (`roadmap/frontend-roadmap.md` bagian 2.3) |
| Dependency | `FE-LAB-07` **selesai**; endpoint dari `BE-LAB-14` **selesai**, diverifikasi langsung pada source backend |
| Klasifikasi | `MEDIUM` — skor 9: repository 1, berkas diperiksa 2, berkas diubah 1, logika bisnis 2, kontrak API 1, database 0, keamanan 0, UI/workflow 2 |
| Task mode | `FRONTEND` — frontend target tulis, backend strict read-only sebagai sumber kebenaran kontrak |
| Target tulis | `QuilvianSystemFrontendDev` — lapis modul `laboratory-management`, store Redux, dan satu berkas uji. `NewQuilvianSystemBackend` — **hanya** laporan ini beserta tautan buktinya pada `roadmap/` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit frontend saat dikerjakan | `c71c02a07`, branch `YogaV2` |
| Commit backend yang dijadikan rujukan | `8e48841`, branch `yoga` |
| Tanggal | 2026-09-07 |
| Status | **Selesai.** Ketiga butir DoD terpenuhi; lint, uji, dan build seluruhnya `PASS`. Verifikasi manual tertunda menunggu backend dan data — bagian 5 |

---

## 1. Masalah yang diperbaiki

Petugas laboratorium tidak punya satu tempat untuk menjawab pertanyaan paling sederhana dalam
pekerjaannya: **apa yang harus saya kerjakan sekarang?**

> Yang tersedia hanya daftar pesanan. Padahal sejak `LAB-DEC-026` kesegeraan melekat pada
> **baris pemeriksaan**, bukan pada pesanan — satu pesanan dapat memuat Kalium cito dan
> Kolesterol biasa sekaligus. Daftar pesanan tidak dapat menunjukkan bahwa hanya Kalium yang
> mendesak.

Masalah kedua milik kepala instalasi: **tidak ada cara mengetahui cito mana yang sudah lewat
batas waktunya.** Batas waktu itu sudah tersimpan sejak `BE-LAB-02` dan sudah dihitung sejak
`BE-LAB-14`, tetapi belum pernah sampai ke layar siapa pun.

---

## 2. Proses bisnis

### 2.1 Daftar kerja — `AC-10`, `AC-39`

Satuannya **pemeriksaan**, bukan pesanan.

| Yang dilihat petugas | Kenapa begitu |
| --- | --- |
| Cito selalu di baris teratas | Urutan menentukan pekerjaan mana dikerjakan lebih dulu (`LAB-FE-006`) |
| Di dalam tiap tingkat kesegeraan, yang paling lama menunggu di atas | Antrean yang adil di antara pekerjaan yang sama mendesaknya |
| Barcode wadah pada tiap baris | Petugas mencocokkan baris dengan tabung yang ada di tangannya |
| Status pemeriksaan **dan** status wadah | Keduanya berbeda: pemeriksaan dapat masih `Ordered` sementara wadahnya sudah `Rejected` |

> **Contoh `AC-39` yang sebenarnya terjadi.** Kalium cito diminta pukul 08.00, Kolesterol biasa
> diminta pukul 10.00. Kolesterol lebih baru, tetapi Kalium tetap di atas. Bila petugas memilih
> pengurutan "Paling baru diminta", Kolesterol naik ke atas **di antara sesama pekerjaan
> biasa** — dan Kalium tetap di baris pertama.

### 2.2 Pantau keterlambatan cito — `AC-17`, `FR-04.3`

| Butir | Aturannya |
| --- | --- |
| Kapan mulai dihitung | **Sejak wadah dinyatakan layak**, bukan sejak pesanan dibuat. Sebelum bahannya layak, laboratorium belum punya apa pun untuk dikerjakan dan tidak adil dihitung terlambat |
| Kelebihan waktu | Ditulis "2 jam 15 menit", bukan "135" |
| Jenis pemeriksaan yang belum punya batas waktu | **Tetap ditampilkan**, ditandai *Belum ada batas* beserta keterangannya, dan **tidak** dihitung terlambat (`VAL-39`) |

Kenapa baris tanpa batas waktu tidak disembunyikan: yang belum lengkap adalah **data induknya**,
bukan pekerjaannya. Menyembunyikannya membuat kepala instalasi tidak pernah tahu ada jenis
pemeriksaan yang batas waktunya belum pernah diatur.

### 2.3 Jalur tidak normal

| Keadaan | Yang terlihat |
| --- | --- |
| Tidak ada pekerjaan menunggu | Keterangan yang menjelaskan **kenapa** kosong, bukan sekadar "tidak ada data" |
| Tidak ada cito terlambat | Dinyatakan sebagai **keadaan yang diharapkan**, bukan tanda data gagal dimuat |
| Permintaan gagal | Pesan dari server ditampilkan apa adanya |
| Penyaring diubah | Halaman kembali ke satu, supaya petugas tidak mendarat di halaman kosong |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk menetapkan |
| --- | --- |
| `roadmap/frontend-roadmap.md` bagian 2.3 dan 6 | Invariant `LAB-FE-006`, cakupan, dan DoD |
| `Areas/.../LabWorklistController.cs` beserta `LabWorklistDtos.cs` | Kontrak sebenarnya: dua endpoint, bentuk penyaring, dan seluruh ruas jawabannya |
| `components/features/base-features/data-table.jsx` | **Perilaku pengurutan bawaannya** — lihat bagian 3.4 butir 1 |
| `lab-order-list-view.jsx`, `use-lab-order-list.jsx` | Pola daftar, penyaring, dan pagination terdekat |
| `lab-specimen-rules.js` beserta ujinya | Pola aturan murni yang dipakai `FE-LAB-07` |
| `lab-specimen-constants.jsx` | Label dan warna status wadah, dipakai ulang di sini |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../constants/.../lab-worklist-constants.jsx` | **Baru.** Route, pilihan pengurutan, penyaring, kolom, dan salinan teks kedua layar |
| `.../hooks/.../lab-worklist-rules.js` | **Baru.** Aturan murni: `enforceCitoFirst`, `sortWorklistRows`, `isOverdueRow`, `formatOverdueMinutes`, `buildWorklistParams` |
| `.../services/.../lab-worklist.service.js` | **Baru.** Dua pemanggilan endpoint, keduanya baca saja |
| `.../state/slice/.../lab-worklist-slice.jsx` | **Baru.** Dua daftar terpisah beserta penanda muat masing-masing |
| `.../hooks/.../use-lab-worklist.jsx` | **Baru.** Satu controller melayani kedua layar |
| `.../view/.../lab-worklists/lab-worklist-view.jsx` | **Baru.** Kedua layar |
| `.../view/.../lab-worklists/lab-worklist-table-columns.jsx` | **Baru.** Dua susunan kolom |
| `src/app/.../lab-worklists/page.jsx` dan `.../cito-overdue/page.jsx` | **Baru.** Dua route |
| `src/style/.../lab-worklists/lab-worklist.module.css` | **Baru.** Tiga kelas |
| `src/lib/state/store.jsx` | Satu potongan Redux didaftarkan |
| `tests/unit/lab-worklist-rules.test.mjs` | **Baru.** Enam belas uji |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Tidak ada perubahan.** Layar mengonsumsi `LAB-API-v1` r3 apa adanya |
| Database | **Tidak ada dampak sama sekali** |
| Keamanan/Auth | Kedua endpoint memakai `LabWorklist : Read`. Kedua layar **baca saja** — tidak ada satu pun tindakan yang mengubah data |

### 3.4 Keputusan dan selisih yang perlu diketahui

| No | Butir | Penjelasan |
| ---: | --- | --- |
| 1 | **`DataTable` mengurutkan ulang datanya sendiri secara bawaan, dan itu ancaman langsung bagi `LAB-FE-006`** | Propertinya `sortLatestFirst` bernilai `true` bila tidak dimatikan, sehingga baris bertanggal terbaru dinaikkan ke atas. Pada daftar ini, pemeriksaan **biasa** yang diminta belakangan akan melompati **cito** yang diminta lebih dulu — invariantnya batal tanpa satu baris kode pun terlihat salah. Ditemukan saat membaca `data-table.jsx`, bukan saat menguji |
| 2 | **Penegakan invariant berlapis dua, dan lapis keduanya yang sebenarnya menjaga** | Lapis pertama mematikan pengurutan bawaan tabel. Lapis kedua melewatkan seluruh baris ke `sortWorklistRows`, yang menegakkan kembali urutan cito **sesudah** pengurutan pilihan petugas. Lapis pertama saja tidak cukup: ia hanya menutup satu jalan yang kebetulan aktif secara bawaan, dan tidak menjaga apa pun terhadap pengurutan yang ditambahkan kemudian |
| 3 | **Urutannya: mengurutkan dulu, menegakkan cito belakangan** | Membalik keduanya membuat pengurutan membatalkan invariantnya. Ini dicatat sebagai komentar pada `sortWorklistRows` karena kekeliruannya tidak akan terlihat pada tinjauan sekilas |
| 4 | **Uji menelusuri seluruh pilihan pengurutan yang ditawarkan layar** | Bukan satu contoh yang dipilih tangan. Uji membaca `LAB_WORKLIST_SORT_OPTIONS` dan memeriksa setiap nilainya, sehingga menambah pilihan baru tanpa melewatkannya lewat `enforceCitoFirst` akan **membuat uji gagal** |
| 5 | **Pengurutan berlaku pada halaman yang terbuka saja** | `LabWorklistPagedQuery` tidak punya ruas pengurutan, sehingga pengurutan pilihan petugas hanya dapat dijalankan di sisi layar. Batas ini dinyatakan terbuka pada layar lewat keterangan, bukan disembunyikan — petugas berhak tahu sebelum menyimpulkan daftarnya salah urut |
| 6 | **`onlyCito` hanya dikirim ketika benar-benar dipilih** | Mengirim `false` menyaring habis pekerjaan biasa pada sebagian pemanggilan, bukan menampilkan seluruhnya. Dijaga dua uji |
| 7 | **Nilai penyaring yang tidak masuk akal dikembalikan ke bawaannya, bukan dijepit** | `pageSize` bernilai `-5` sempat menghasilkan **satu baris per halaman** — daftar yang tampak nyaris kosong tanpa sebab yang terlihat. **Cacat ini ditemukan oleh uji, bukan oleh tinjauan**, lalu implementasinya yang diperbaiki |
| 8 | **Penyaring kesegeraan dan pengurutan tidak ada pada layar keterlambatan** | Daftar itu seluruhnya memang cito, sehingga menyaring kesegeraan tidak berarti apa-apa, dan urutannya sudah ditentukan besaran keterlambatan |
| 9 | **Kedua layar memakai satu hook dan satu view** | Penyaring, pagination, dan penanganan state keduanya sama persis; yang berbeda hanya sumber data dan dua kontrol. Memisahnya akan menyalin seluruh perilaku itu dua kali |

---

## 4. Dokumentasi endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Worklist

| Method | Path | Dipakai layar | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/pending` | Daftar kerja; cito di urutan atas | `LabWorklist : Read` |
| `GET` | `/cito-overdue` | Pantau keterlambatan cito | `LabWorklist : Read` |

Keduanya menerima penyaring yang sama: `pageNumber`, `pageSize`, `discipline`, `onlyCito`, dan
`search`; keduanya mengembalikan `PagedResult<T>`.

---

## 5. Verifikasi

| Perintah atau skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| `tests/unit/lab-worklist-rules.test.mjs` | `pass 16, fail 0` | `PASS` |
| Seluruh uji unit (`--test tests/unit/`) | `pass 519, fail 0` | `PASS` |
| `npm run lint:errors` | Bersih, tanpa keluaran | `PASS` |
| `npm run build` | `Compiled successfully`; kedua route baru terbentuk | `PASS` |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/ — PASS`
(`npm run test:unit` tetap tidak dapat dipakai apa adanya pada Node `v20.20.2`; alasannya sama
dengan yang dicatat `FE-LAB-05.md`, dan `package.json` tidak disentuh).

**Uji yang membuktikan invariant keselamatan:**

| Yang dibuktikan | Uji |
| --- | --- |
| Cito naik ke atas walau diminta belakangan | `cito naik ke atas walau diminta belakangan` |
| **Tidak satu pun** pilihan pengurutan dapat menggeser cito ke bawah | `TIDAK SATU PUN pilihan pengurutan dapat menggeser cito ke bawah` — menelusuri seluruh isi `LAB_WORKLIST_SORT_OPTIONS` |
| Pengurutan tetap berlaku di dalam kelompok kesegeraan | `pengurutan tetap berlaku DI DALAM kelompok kesegeraan` |
| Urutan relatif dipertahankan, sehingga aman dirangkai | `urutan relatif di dalam tiap kelompok dipertahankan` |
| Pengurutan yang tidak dikenal pun tetap aman | `pengurutan yang tidak dikenal tetap menegakkan urutan cito` |
| `VAL-39` — baris tanpa batas waktu tidak dihitung terlambat | `baris tanpa batas waktu TIDAK dihitung terlambat` |
| Kelebihan waktu dalam satuan yang dipahami | tiga uji `formatOverdueMinutes` |

### Verifikasi manual

`MANUAL TEST: NOT FEASIBLE`

Alasannya konkret: kedua layar menuntut data yang belum ada — pemeriksaan yang belum selesai
pada beberapa tingkat kesegeraan, dan sekurang-kurangnya satu cito yang sudah melewati batas
waktunya. Backend yang dihentikan pada sesi sebelumnya juga belum dijalankan kembali.

**Yang wajib diperiksa manual begitu backend berjalan dan datanya tersedia:**

| No | Yang diperiksa | Yang diharapkan |
| ---: | --- | --- |
| 1 | Membuka daftar kerja berisi cito dan pekerjaan biasa | Seluruh cito di atas, tanpa terkecuali |
| 2 | Mengubah pengurutan ke **Paling baru diminta** | Urutan berubah **di dalam** tiap kelompok; cito tetap di atas |
| 3 | Mengubah pengurutan ke **Nama pemeriksaan Z-A** | Sama seperti di atas |
| 4 | Menyaring **Hanya Cito**, lalu mengembalikannya ke Semua | Daftar menyempit lalu kembali utuh; halaman kembali ke satu |
| 5 | Menyaring per disiplin digabung dengan pencarian | Keduanya berlaku bersamaan, bukan saling menimpa |
| 6 | Berpindah halaman lalu mengubah penyaring | Halaman kembali ke satu, bukan tertinggal di halaman kosong |
| 7 | Membuka pantau keterlambatan | Kelebihan waktu terbaca sebagai jam dan menit |
| 8 | Baris yang batas waktunya belum diatur | Ditandai *Belum ada batas* beserta keterangannya, **tidak** ditandai terlambat |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-10` — pekerjaan belum selesai terlihat dengan cito di urutan atas | Terpenuhi di sisi layar | Enam uji urutan; sisi backend sudah terbukti pada `BE-LAB-14.md` |
| `AC-17` — keterlambatan cito terpantau | Terpenuhi di sisi layar | Layar keterlambatan beserta kolom kelebihan waktu; `VAL-39` dijaga uji |
| `AC-39` — kesegeraan per pemeriksaan, bukan per pesanan | Terpenuhi | Satuan baris adalah pemeriksaan; satu pesanan bercampur cito dan biasa hanya menaikkan yang cito |

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Dua layar ada | Terpenuhi | Kedua route terbentuk pada keluaran `npm run build` |
| Cito selalu di atas **dalam keadaan apa pun** | Terpenuhi | Ditegakkan dua lapis; uji menelusuri **seluruh** pilihan pengurutan yang ditawarkan layar |
| Kelebihan waktu tampil dalam satuan yang dipahami petugas | Terpenuhi | "2 jam 15 menit", termasuk satuan hari untuk keterlambatan panjang — tiga uji |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `npm run lint:errors` bersih. `npm run build` tanpa error |
| Masalah yang diketahui | Pengurutan pilihan petugas berlaku pada halaman yang terbuka saja, karena `LabWorklistPagedQuery` tidak punya ruas pengurutan. Dinyatakan terbuka pada layar. Bila kelak diinginkan berlaku menyeluruh, ruas pengurutan perlu ditambahkan pada kontrak — **perubahan kontrak, bukan pekerjaan layar** |
| Risiko tersisa | **Pertama, verifikasi manual belum dijalankan** — delapan skenario pada bagian 5; invariant `LAB-FE-006` baru terbukti pada tingkat aturan dan uji, belum pada layar sungguhan. **Kedua**, `DataTable` tetap mengurutkan ulang secara bawaan; layar mana pun berikutnya yang menampilkan data berurut wajib mematikannya sendiri — perilaku ini tidak dapat dimatikan sekali untuk seluruh modul |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Satu berkas `M`, enam entri `??`. **Tidak ada** `git add`, `commit`, maupun `push` |
| Langkah berikutnya | 1. Menjalankan backend, lalu menjalankan delapan skenario verifikasi manual. 2. **`FE-LAB-09`** — tiga layar monitoring per disiplin, task frontend terakhir modul ini; pasangan backendnya `BE-LAB-15` sudah selesai |
