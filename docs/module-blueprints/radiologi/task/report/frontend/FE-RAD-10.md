# Laporan Perubahan Frontend — `FE-RAD-10`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RAD-10` |
| Judul | Daftar bacaan menunggu dan penulisan draf |
| Epic | `EPIC RAD-02` |
| Requirement | `FR-RAD-010` (penomoran blueprint) |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 4, gelombang `MVP-3` |
| Contract version | `RAD-API-001` grup *Rad Report* (`v1`, lengkap sejak amandemen revision 7); `RAD-VAL-001` bagian 1 |
| Acceptance criteria | Kesimpulan wajib diisi; draf hanya dapat diubah penulisnya |
| Test yang diminta roadmap | Menyimpan draf tanpa kesimpulan ditolak beserta pesannya |
| **Ketentuan mengikat** | `RAD-ARCH-FE-001` bagian 5 butir 6 — isi bacaan **tidak boleh** disimpan di penyimpanan browser |
| Dependency | `FE-RAD-01` **selesai**; `BE-RAD-09` **selesai** |
| Task mode | `FRONTEND` — frontend target tulis; backend strict read-only kecuali laporan ini dan baris status roadmap |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Branch frontend | `YogaV2`, upstream `origin/YogaV2` |
| Tanggal | 2026-09-14 |
| Status | **Selesai sebagian — lihat bagian 3.** 17 unit test baru lulus; 876 unit test repository lulus, 0 gagal. Bagian "daftar study layak yang **belum** dibaca" **tidak dapat dikerjakan**: tidak ada endpoint yang dapat mendaftarkannya |

---

## 1. Ketentuan mengikat: isi bacaan tidak menyentuh penyimpanan browser

`RAD-ARCH-FE-001` bagian 5 butir 6, dan Definition of Done menuntut **tidak ada sisa setelah
halaman ditutup**. `Findings`, `Impression`, `Recommendation`, dan `AmendmentReason` seluruhnya
ditandai **Sensitif** pada DTO backend; alasan koreksi bahkan ditandai haram masuk application
log.

Empat hal dikerjakan untuk memenuhinya:

| Yang dikerjakan | Alasannya |
| --- | --- |
| Isi draf hanya hidup di `useState` layar penulisannya | Tidak ada `localStorage`, `sessionStorage`, `indexedDB`, maupun penulisan cookie pada tujuh berkas bacaan |
| **Tidak dititipkan ke Redux** | `rad-report-slice` sengaja tidak punya ruas untuk isi bacaan dan tidak memuat `detail`. Store dapat dibaca layar mana pun dan bertahan selama navigasi klien |
| Dikosongkan saat layar dilepas | Efek pembersih `useEffect` mengosongkan isian dan bacaan yang termuat |
| **Tidak ada simpan otomatis** | Simpan otomatis menuntut tempat penampungan, dan tempat penampungan itulah yang dilarang |

### 1.1 Jebakan yang hampir kena

Repository ini punya **dua** `BaseTextField`. Yang di `base-form-control.jsx` tidak menyimpan
apa pun. Yang di `base-text-field.jsx` punya prop `persist` yang menuliskan nilainya ke
`sessionStorage` atau `localStorage`:

```js
const getBrowserStorage = (storageType) =>
  storageType === "local" ? window.localStorage : window.sessionStorage;
```

Nilai bawaannya `persist = false`, sehingga aman selama tidak dinyalakan — tetapi itu berarti
isi bacaan hanya berjarak **satu prop** dari kebocoran. Layar draf karena itu memakai varian
`base-form-control`, dan uji `G3` menolak berkas draf yang mengimpor `base-text-field.jsx`.

### 1.2 Dibuktikan, bukan dijanjikan

Ketentuan seperti ini tidak dapat dibuktikan oleh komentar. `tests/unit/rad-report-storage-guard.test.mjs`
membaca **source** ketujuh berkas bacaan lalu menolak setiap jejak penyimpanan — `localStorage`,
`sessionStorage`, `indexedDB`, `document.cookie`, `Cookies.set`, `persist`, dan `persistKey` —
setelah membuang isi komentar, supaya keterangan yang **menerangkan** larangan tidak ikut
terbaca sebagai pelanggaran.

**Penjaganya diuji dengan melanggarnya.** Satu baris `localStorage.setItem("draft", form.impression)`
disisipkan sementara ke `use-rad-report-draft.jsx`, dan `G1` gagal dengan pesan yang menyebut
berkas beserta alasannya. Baris itu dikembalikan setelahnya. Penjaga yang tidak pernah gagal
tidak membuktikan apa pun.

Lima uji penjaga: bebas penyimpanan (`G1`), isi bacaan tidak masuk Redux (`G2`), isian tanpa
kemampuan menyimpan (`G3`), pembersihan saat unmount (`G4`), dan tidak ada simpan otomatis
(`G5`).

---

## 2. Acceptance criteria

**Kesimpulan wajib diisi.** `validateDraftContent` menolak kesimpulan kosong dengan kalimat
yang **sama persis** dengan `RadReportService` — *"Kesimpulan bacaan wajib diisi."* Yang
dikerjakan layar hanya memunculkannya lebih awal; yang menolak sebenarnya tetap server.
Menuliskan kalimat sendiri akan membuat petugas membaca dua bunyi berbeda untuk satu penolakan
yang sama. Ketiga batas panjang ikut disalin, termasuk batas saran 2.000 huruf yang
**tidak tercantum pada `RAD-VAL-001`** — backend menambahkannya sendiri karena kolomnya
`varchar(2000)`.

**Draf hanya dapat diubah penulisnya.** `canEditDraft` menuntut dua hal sekaligus, menyalin
`UpdateDraftAsync`: versi kerja masih `Drafted`, **dan** penulisnya orang yang sama. Identitas
dibaca dari cookie `userId`, mengikuti pola `use-cashier-shift`. Ketika identitas tidak
diketahui hasilnya `false` — layar tidak menebak kepemilikan. Itu hanya menyembunyikan tombol;
penolakan sebenarnya tetap `403` dari server.

---

## 3. Temuan: separuh deliverable tidak punya sumber data

Roadmap meminta **"daftar study layak yang belum dibaca"**. Bagian itu tidak dapat dikerjakan,
dan sebabnya bukan di frontend.

`RadReportService` punya `EnsurePendingReportAsync` — menyiapkan wadah bacaan berstatus
`Pending` untuk study yang citranya sudah dinyatakan layak. Diperiksa di seluruh repository:

```text
$ grep -rn "EnsurePendingReport" --include=*.cs .
Areas/.../Services/RadReportService.cs:562:  public async Task<...> EnsurePendingReportAsync(
```

**Satu baris — definisinya sendiri. Tidak ada satu pun pemanggil.** Ia tidak tersambung ke
endpoint mana pun, dan `RadStudyService` tidak menyebut `RadReport` sama sekali. Akibatnya
berantai:

1. Bacaan **hanya lahir** di dalam `POST /by-study/{id}/draft`.
2. Baris `RadReport` berstatus `Pending` **tidak pernah ada**.
3. `GET /rad-reports?reportStatus=Pending` **selalu kosong**.
4. `GET /rad-reports/summary` melaporkan `MenungguDraf` dan ikut menghitungnya pada
   `BelumDirilis` — keduanya **selalu nol**, dan angka nol itu terbaca "tidak ada yang menunggu
   dibaca".
5. Tidak ada endpoint yang mendaftarkan study dengan `IsUsable == true` yang belum punya
   bacaan. `GET /rad-studies` daftar tidak ada; yang ada hanya `by-order`.

Ini persis penghalang terbuka yang dicatat `BE-RAD-08`: *"**Belum tersambung**: `RadStudyService`
belum memanggil kelahiran bacaan saat study menjadi `QualityAccepted`."* Ia masih terbuka.

### Yang dikerjakan sebagai gantinya

**Tidak mengarang daftar dari sisi klien.** Menyusunnya berarti menelusuri seluruh pesanan lalu
seluruh study-nya satu per satu — pola N+1 yang baru saja dihapus `RAD-CONF-001`, dan hasilnya
tetap tidak lengkap karena tidak ada daftar pesanan se-rumah-sakit yang dijamin memuat semuanya.

**Tidak menampilkan daftar kosong yang terbaca "tidak ada yang perlu dibaca".** Layar daftar
menyatakan keterbatasannya di atas tabel, dengan kalimat yang menyebut sebab dan jalan
keluarnya: daftar ini hanya memuat bacaan yang **sudah pernah ditulis**; pemeriksaan yang belum
ada drafnya belum dapat didaftarkan; daftar yang kosong di sini **bukan** berarti tidak ada yang
perlu dibaca.

**Jalan masuknya disediakan dari tempat yang memang memegang penandanya.** Konsol study
`FE-RAD-08`/`FE-RAD-09` memegang `radStudyId` sebuah pemeriksaan yang mutunya baru dinyatakan
layak, dan di situlah tombol **Tulis Bacaan** diletakkan. Alurnya utuh — hanya tidak lewat
daftar.

**Perbaikannya kecil dan sudah ada kodenya**: memanggil `EnsurePendingReportAsync` ketika study
menjadi `QualityAccepted`. Setelah itu `GET /rad-reports?reportStatus=Pending` langsung berisi,
dan daftar bacaan menunggu dapat dibuat tanpa mengubah satu baris pun di frontend ini.
**Perlu keputusan pemilik modul.**

---

## 4. Satu alamat, dua keadaan

Layar draf beralamat pada **pemeriksaan** —
`/rad-reports/study/[radStudyId]` — bukan pada bacaan. Bacaan baru lahir ketika drafnya ditulis,
sehingga alamat berbasis `radReportId` tidak dapat dipakai untuk menulis yang pertama.

`GET /by-study/{id}` menjawab `404` dengan *"Pemeriksaan ini belum memiliki bacaan."* ketika
bacaannya belum ada. Hook memperlakukan `404` itu sebagai **tanda mode tulis baru**, bukan
kegagalan — menampilkannya sebagai galat akan membuat petugas mengira ada yang rusak. Satu
alamat karena itu melayani tulis baru dan sunting sekaligus, dan baris daftar bacaan pun
menautkannya lewat `radStudyId` yang memang sudah dibawa `RadReportListResponse`.

---

## 5. Perubahan yang dikerjakan

| Berkas | Keadaan |
| --- | --- |
| `src/lib/hooks/health-services/radiology-management/rad-report-rules.js` | Baru — fungsi murni; validasi isi, kewenangan ubah, versi berlaku vs versi kerja, pembentuk muatan |
| `src/lib/hooks/health-services/radiology-management/use-rad-report-draft.jsx` | Baru — controller penulisan draf |
| `src/lib/hooks/health-services/radiology-management/use-rad-report-list.jsx` | Baru — controller daftar |
| `src/lib/state/slice/health-services/radiology-management/rad-report-slice.jsx` | Baru — daftar, rekap, metadata. **Tanpa isi bacaan** |
| `src/lib/constants/health-services/radiology-management/rad-report-constants.jsx` | Baru — label keadaan, pilihan peran penulis, salinan teks |
| `src/components/view/…/rad-reports/rad-report-list-view.jsx` | Baru — daftar berhalaman |
| `src/components/view/…/rad-reports/rad-report-draft-view.jsx` | Baru — layar tulis dan ubah draf |
| `src/style/health-services/radiology-management/rad-reports/rad-report.module.css` | Baru — 22 kelas, seluruhnya design token |
| `src/app/…/rad-reports/page.jsx` dan `…/rad-reports/study/[radStudyId]/page.jsx` | Baru — 2 route |
| `tests/unit/rad-report-rules.test.mjs` | Baru — 12 test |
| `tests/unit/rad-report-storage-guard.test.mjs` | Baru — 5 test penjaga ketentuan mengikat |
| `src/lib/state/store.jsx` | Diubah — satu reducer `radReport` |
| `src/lib/constants/…/radiology-constants.jsx` | Diubah — `RADIOLOGY_ROUTES` menambah `reports` dan `reportDraft()` |
| `src/utils/menu-sidebar/menu-items.jsx` | Diubah — satu entri pada grup Radiologi |
| `src/components/view/…/rad-safety-gate/rad-acquisition-panel.jsx` | Diubah — tombol **Tulis Bacaan**; lihat bagian 3 |

Tidak ada base component baru, tidak ada pemanggilan Axios baru — `rad-report.service.js` sudah
lengkap sejak `FE-RAD-01` dan dipakai apa adanya.

---

## 6. Validasi

| Yang dijalankan | Hasil |
| --- | --- |
| `node --test tests/unit/rad-report-rules.test.mjs` dan `rad-report-storage-guard.test.mjs` | **17 lulus, 0 gagal** |
| Penjaga diuji dengan melanggarnya | `G1` **gagal** saat `localStorage.setItem` disisipkan, lalu lulus lagi setelah dikembalikan. Lihat bagian 1.2 |
| `node --test tests/unit/` (seluruh repository) | **876 lulus, 0 gagal** |
| `npx eslint` atas seluruh berkas yang disentuh | **0 error, 1 warning baru** — `react-hooks/set-state-in-effect` pada pemuatan bacaan, jenis dan bentuk yang sama dengan `use-rad-worklist` dan `use-rad-order-form` yang sudah ada. Dipertahankan demi keseragaman; menghindarinya menuntut membuang penanda muat saat menyegarkan |
| `npm run build` | **Compiled successfully in 38.5s.** Route terdaftar: `/health-services/radiology-management/rad-reports` (static) dan `…/rad-reports/study/[radStudyId]` (dynamic) |

**`MANUAL TEST: NOT FEASIBLE`.** Menulis draf menuntut study berstatus `QualityAccepted` dengan
`IsUsable == true`. Status itu **tidak dapat dicapai di lingkungan mana pun**: gerbang
keselamatan fail-closed dan belum ada satu pun aturan berstatus `Active` (`RAD-OPEN-011`), jadi
tidak satu pun study melewati `SafetyCleared`, apalagi sampai dinilai mutunya. Sesi petugas
pemegang `RadReport : Create` dan `RadReport : Update` juga tidak dapat dibuat dari sini.
Seluruh jalur dikunci unit test terhadap bentuk balasan yang diverifikasi langsung dari DTO dan
service backend.

---

## 7. Yang sengaja **tidak** dikerjakan

| Butir | Alasan |
| --- | --- |
| Daftar study layak yang belum dibaca | Tidak ada sumber datanya. Lihat bagian 3 |
| Menyusun daftar itu dari sisi klien | Pola N+1 yang baru dihapus `RAD-CONF-001`, dan hasilnya tetap tidak lengkap |
| Tombol Sahkan dan Rilis | Milik `FE-RAD-11` |
| Layar koreksi dan riwayat versi lengkap | Milik `FE-RAD-12`. Riwayat versi di sini hanya penyerta layar draf |
| Menampilkan isi bacaan pada daftar | Balasan daftar memang tidak membawanya — dan itu disengaja backend |
| Simpan otomatis draf | Menuntut tempat penampungan yang dilarang ketentuan mengikat |

---

## 8. Risiko dan dependency yang masih terbuka

| Hal | Keadaan |
| --- | --- |
| **`EnsurePendingReportAsync` tanpa pemanggil** | Temuan bagian 3. Separuh deliverable task ini tertahan karenanya. **Perlu keputusan pemilik modul** |
| **`MenungguDraf` pada rekap selalu nol** | Akibat langsung temuan bagian 3. Angka nol terbaca "tidak ada yang menunggu dibaca" |
| **`RadReport : ActAsRadiologist` belum dapat diberikan** | Penghalang `BE-RAD-08` dan `BE-RAD-09`, masih terbuka. Tidak ada yang dapat menulis draf sebagai radiolog maupun mengesahkannya — berdampak langsung pada `FE-RAD-11` |
| **Tidak ada aturan keselamatan `Active`** | `RAD-OPEN-011` terbuka. Tidak satu pun study mencapai `QualityAccepted`, sehingga layar ini belum dapat dipakai siapa pun |
| **`base-text-field.jsx` punya prop `persist`** | Temuan bagian 1.1. Aman selama tidak dinyalakan, dan uji `G3` menjaganya untuk layar bacaan — tetapi komponennya tetap ada bagi layar lain |
| **Empat transisi study tanpa endpoint** | Temuan `FE-RAD-09`, masih terbuka |
| **Tiga enum tidak terbit pada metadata study** | Temuan `FE-RAD-09`, masih terbuka |
| **Study terkunci saat aturan keselamatan berubah** | Temuan `FE-RAD-08`, masih terbuka |
| **Kontrak `409` vs jalur API `422`** | Temuan `FE-RAD-08`, masih terbuka |
| **`HoldAsync` menerima status terminal** | Temuan `FE-RAD-06`, masih terbuka |
| **Daftar kerja tanpa identitas pasien** | Temuan `FE-RAD-07`, masih terbuka |

---

## 9. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-14 | Laporan dibuat. 11 berkas baru, 4 diubah. 17 test baru lulus; 876 test repository lulus; lint 0 error; build lulus. Ketentuan mengikat bagian 5 butir 6 dipenuhi dan **dibuktikan lewat uji penjaga yang diuji dengan melanggarnya**. Temuan utama: `EnsurePendingReportAsync` tidak punya satu pun pemanggil, sehingga bacaan berstatus `Pending` tidak pernah ada dan "daftar study layak yang belum dibaca" tidak punya sumber data. | `draft` |
