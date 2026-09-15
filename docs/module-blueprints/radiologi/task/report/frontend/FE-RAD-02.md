# Laporan Perubahan Frontend — `FE-RAD-02`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RAD-02` |
| Judul | Layar kelola alat pencitraan |
| Epic | `EPIC RAD-06` |
| Requirement | `FR-RAD-051` (penomoran blueprint, **bukan** PRD — `RAD-CONF-DEC-08`) |
| Roadmap | `docs/module-blueprints/radiologi/roadmap/frontend-roadmap.md` bagian 3, gelombang `MVP-1` |
| Contract version | `RAD-API-001` grup *Master Data / Rad Modality*; `RAD-VAL-001` bagian 3 |
| Acceptance criteria | `AC-5` — menambah alat baru lewat antarmuka, tanpa menyentuh database langsung |
| Test yang diminta roadmap | `UAT-12` — menonaktifkan alat yang masih dipakai ditolak beserta pesannya |
| Wewenang UI | Seluruhnya `DEV_DISCRETION` sepanjang mengikuti pola master data yang sudah ada. Tidak ada ketentuan mengikat `RAD-ARCH-FE-001` bagian 5 yang berlaku pada task ini |
| Dependency | `FE-RAD-01` **selesai** 2026-09-11; `BE-RAD-04` **selesai** |
| Task mode | `FRONTEND` — frontend target tulis; backend strict read-only, kecuali berkas laporan ini |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Branch frontend | `YogaV2`, upstream `origin/YogaV2` |
| Tanggal | 2026-09-11 |
| Status | **Selesai.** 13 unit test baru lulus; 766 unit test repository lulus, 0 gagal; lint 0 error. Layar dapat dibuka dari menu sidebar dan alat baru dapat didaftarkan tanpa menyentuh database |

---

## 1. Keputusan penempatan berkas, dan mengapa berbeda dari bacaan harfiah kontrak

`RAD-ARCH-FE-001` bagian 2 menyatakan route Radiologi mengikuti pola Laboratorium dengan nama
`radiology-management`. Bacaan harfiahnya menaruh layar ini di
`src/app/health-services/radiology-management/`.

**Yang dipilih: `src/app/health-services/master-data/rad-modalities/`.** Alasannya bukti, bukan
selera:

| Bukti | Isinya |
| --- | --- |
| `lab-rejection-reasons` | Master data **milik backend Laboratorium**, tetapi layarnya berada di `src/app/health-services/master-data/` |
| `lab-value-bounds` | Sama |
| `AGENTS.md` frontend | "Ikuti kode yang sudah ada. Jangan menciptakan arsitektur baru." |
| `RAD-ARCH-FE-001` bagian 2 | Menyatakan nama folder dan berkas persisnya `DEV_DISCRETION` |

Jadi preseden terdekat untuk "layar master data milik modul penunjang" adalah `master-data/`,
dan kontraknya sendiri menyerahkan penamaan kepada pelaksana. Bagian 2 `RAD-ARCH-FE-001`
berbicara tentang layar operasional; ia tidak membahas master data, dan modul Laboratorium —
pola yang secara eksplisit diminta ditiru — justru tidak menaruh master data-nya di sana.

Layar operasional Radiologi (`FE-RAD-05` dan seterusnya) tetap akan berada di
`radiology-management/`.

---

## 2. Proses bisnis dari sisi pengguna

**Yang dapat dikerjakan admin Radiologi:**

1. Membuka **Data Master → Alat Pencitraan Radiologi** dari sidebar.
2. Melihat ringkasan enam angka, termasuk **"Belum Siap"** — alat aktif yang akan menolak
   pemeriksaannya.
3. Menyaring menurut status, kesiapan gerbang, radiasi pengion, media kontras, dan kata kunci.
4. Menambah alat baru lewat formulir.
5. Memperbarui alat, termasuk kodenya.
6. Mengaktifkan, menonaktifkan, atau menandai alat terhapus.

**Keadaan yang ditangani layar:**

| Keadaan | Yang dialami pengguna |
| --- | --- |
| Ada alat aktif tanpa aturan keselamatan berlaku | Kotak peringatan kuning di atas tabel **menyebut nama alatnya satu per satu**, beserta kalimat bahwa setiap pemeriksaan pada alat itu akan ditolak dan langkah berikutnya adalah menyusun aturannya |
| Menonaktifkan alat yang masih dipakai aturan berlaku | Toast merah berisi **pesan server apa adanya**: "Alat ini masih dipakai aturan keselamatan yang berlaku. Nonaktifkan aturannya lebih dulu." Modal konfirmasi **tidak ditutup**, sehingga penolakan terbaca pada konteks alat yang sama — `UAT-12` |
| Menambah alat dengan kode yang sudah dipakai | Toast merah berisi "Kode alat sudah dipakai. Gunakan kode lain." |
| Menambah alat tanpa nama | Ditahan di sisi layar sebelum dikirim, dengan pesan pada ruasnya |
| Alat baru berhasil disimpan | Toast hijau menyatakan berhasil **dan** mengingatkan alat itu belum dapat dipakai sampai aturan keselamatannya disahkan |
| Daftar kosong setelah berhasil dimuat | "Tidak ada alat yang cocok dengan penyaring ini." |
| Daftar kosong karena **belum pernah berhasil dimuat** | "Data alat pencitraan belum dapat dimuat. Ini bukan berarti belum ada alat terdaftar." |
| Tanpa hak akses | `AccessDeniedGate` mengambil alih seluruh halaman |

---

## 3. Perubahan yang dikerjakan

| Berkas | Keadaan |
| --- | --- |
| `src/lib/constants/health-services/master-data/rad-modalities/rad-modality-constants.jsx` | Baru — konfigurasi fitur, 6 ruas formulir, 6 kartu ringkasan, 9 kolom, 5 penyaring |
| `src/utils/health-services/master-data/rad-modalities/rad-modality-utils.jsx` | Baru — fungsi murni, termasuk `getSafetyGateState` dan `getBlockingModalities` |
| `src/lib/state/slice/health-services/master-data/master-data-rad-modality-slice.jsx` | Baru — 7 thunk, 3 reducer, 17 selector |
| `src/lib/hooks/health-services/master-data/rad-modalities/use-master-data-rad-modality.jsx` | Baru — controller daftar |
| `src/lib/hooks/health-services/master-data/rad-modalities/use-master-data-rad-modality-editor.jsx` | Baru — controller formulir |
| `src/components/view/health-services/master-data/rad-modalities/master-data-rad-modality-view.jsx` | Baru — layar daftar |
| `src/components/view/health-services/master-data/rad-modalities/add/rad-modality-form-view.jsx` | Baru — formulir tambah dan ubah |
| `src/style/health-services/master-data/rad-modalities/rad-modality.module.css` | Baru — 3 kelas, seluruhnya memakai design token |
| `src/app/health-services/master-data/rad-modalities/…` | Baru — 5 berkas route |
| `src/lib/state/store.jsx` | Diubah — 2 baris |
| `src/utils/menu-sidebar/menu-items.jsx` | Diubah — 1 entri menu |
| `tests/unit/rad-modality-rules.test.mjs` | Baru — 13 test |

---

## 4. Dua keputusan teknis yang perlu diketahui

### 4.1 Slice memanggil service, bukan `InstanceAxios` langsung

Potongan master data lain di repository ini memanggil `InstanceAxios` langsung. Potongan ini
tidak — ia memakai `rad-modality.service.js` yang sudah berdiri sejak `FE-RAD-01` mengikuti
`RAD-ARCH-FE-001` bagian 2.

Alasannya: membiarkan potongan ini memanggil Axios sendiri berarti alamat endpoint radiologi
tertulis di dua tempat, dan dua sumber kebenaran untuk satu alamat cepat atau lambat
menyimpang. Tidak ada abstraksi baru yang dibuat — yang dipakai lapisan yang sudah ada.

### 4.2 `InformationAlert` mengganti `message` dengan `children`, bukan menambahkannya

Peringatan kesiapan gerbang semula ditulis dengan `message` **dan** `children` sekaligus:
kalimat penjelas pada prop, daftar nama alat sebagai children. Pembacaan komponennya
menunjukkan `{children || (safeMessage ? … : null)}` — children **menggantikan** message.

Akibatnya kalimat "setiap pemeriksaan pada alat tersebut akan ditolak gerbang keselamatan"
tidak akan pernah tampil, dan yang tersisa hanya deretan nama alat di bawah judul tanpa
keterangan apa pun — persis kebalikan dari maksud peringatan itu. Kalimatnya kini berada di
dalam children bersama daftarnya.

### 4.3 Tombol Nonaktifkan tidak dinonaktifkan berdasarkan tebakan layar

Frontend tidak tahu aturan keselamatan mana yang sedang berlaku pada sebuah alat. Menonaktifkan
tombol berdasarkan tebakan akan menyembunyikan aksi yang sebenarnya sah. Penolakan `409`
beserta langkah berikutnya datang dari server dan ditampilkan apa adanya — itulah yang diuji
`UAT-12`.

---

## 5. Validasi

| Yang dijalankan | Hasil |
| --- | --- |
| `node --test tests/unit/rad-modality-rules.test.mjs` | **13 lulus, 0 gagal** |
| `node --test tests/unit/` (seluruh repository) | **766 lulus, 0 gagal** |
| `npx eslint` atas seluruh berkas yang disentuh | **0 error, 2 warning** |
| `npm run build` | **Compiled successfully in 35.4s.** Ketiga route terdaftar: `/health-services/master-data/rad-modalities` (static), `/create` (static), `/[slug]/update` (dynamic) |

**Tentang 2 warning tersisa.** Keduanya `react-hooks/set-state-in-effect` pada controller
formulir. Hook rujukan `use-master-data-lab-rejection-reason-editor.jsx` memunculkan warning
yang **sama jenis dan sama jumlahnya**; ini pola yang sudah ada di repository, bukan
penyimpangan task ini. Satu warning ketiga — `exhaustive-deps` pada `items` — memang milik
task ini dan **sudah diperbaiki** dengan membungkusnya `useMemo`.

**`MANUAL TEST: NOT FEASIBLE`** untuk jalur `UAT-12`. Menjalankannya menuntut sebuah alat yang
sudah punya aturan keselamatan berlaku, sedangkan `BE-RAD-15` mencatat aturan keselamatan baru
tersusun sebagai **draf** dan belum disahkan — `DEC-RAD-005` masih `OPEN` dengan pemilik tata
kelola klinis. Tidak ada satu pun aturan `Active` di lingkungan mana pun, sehingga penolakan
`409` tidak dapat dipicu tanpa lebih dulu menembus keputusan klinis yang belum diambil. Jalur
sukses — menambah, memperbarui, mengaktifkan, menonaktifkan alat yang tidak dipakai — tidak
terhalang.

---

## 6. Yang sengaja **tidak** dikerjakan

| Butir | Alasan |
| --- | --- |
| Halaman detail alat | Fitur master data sejenis tidak punya halaman detail; rincian dibaca formulir ubah lewat `GET /{id}` |
| Saklar aktif/nonaktif pada formulir tambah | Alat baru lahir aktif dengan bawaan backend. Menyediakan saklarnya membuka jalan mendaftarkan alat yang langsung tidak terlihat di mana pun. Penyalaan dan pematian dikerjakan dari daftar, lewat jalur yang punya penjaga `409`-nya |
| Papan kesiapan alat yang penuh | Milik `FE-RAD-04`. Yang ada di sini peringatan dininya, dihitung dari baris yang sedang tampil |
| Base component baru | Dilarang `AGENTS.md` dan Definition of Done |

---

## 7. Risiko dan dependency yang masih terbuka

| Hal | Keadaan |
| --- | --- |
| Tidak ada alat yang punya aturan keselamatan aktif | `BE-RAD-15` selesai sebagian; `DEC-RAD-005` `OPEN`. Menahan verifikasi manual `UAT-12` dan menahan `FE-RAD-08` |
| Peringatan kesiapan dihitung dari halaman yang tampil | Angka pada kartu "Belum Siap" berasal dari backend untuk **seluruh** alat; daftar nama pada peringatan hanya dari baris yang sedang tampil. Keduanya sengaja tidak disamakan, dan perbedaannya disebut pada komentar kode |

---

## 8. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-11 | Laporan dibuat. 13 berkas baru, 2 diubah. 13 test baru lulus; 766 test repository lulus; lint 0 error. | `draft` |
