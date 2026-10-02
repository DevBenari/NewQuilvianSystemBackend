# Laporan Perubahan Frontend — `FE-RAD-01`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RAD-01` |
| Judul | Fondasi modul Radiologi |
| Epic | `EPIC RAD-06` |
| Slice | — (task kerangka; menopang seluruh `FE-RAD-02` sampai `FE-RAD-13`) |
| Roadmap | `docs/module-blueprints/radiologi/roadmap/frontend-roadmap.md` bagian 3, gelombang `MVP-1` |
| Trace | `RAD-ARCH-FE-001` bagian 2 dan 5; `RAD-API-001`; `RAD-CONF-001` bagian 8 |
| Contract version | `RAD-API-001` — `approved`; memuat perubahan `RAD-CONF-001` bagian 8 yang berjalan 2026-09-11 |
| Wewenang UI | Seluruhnya `DEV_DISCRETION` sepanjang mengikuti pola `laboratory-management`. Task ini **tidak** membuat satu pun layar, sehingga tidak menyentuh sembilan ketentuan mengikat `RAD-ARCH-FE-001` bagian 5 — tetapi tiga di antaranya (butir 4, 6, dan 9) sudah **disiapkan tempatnya** di lapisan state dan konstanta, lihat bagian 3 |
| Dependency | `BE-RAD-03` — **terpenuhi**, laporannya berstatus `Selesai` |
| Task mode | `FRONTEND` — frontend target tulis; backend strict read-only sebagai sumber kebenaran kontrak, kecuali berkas laporan ini |
| Target tulis | `QuilvianSystemFrontendDev` — lapisan konstanta, service, hook rules, Redux slice, store, dan test; serta `NewQuilvianSystemBackend` — **hanya** berkas laporan ini |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Branch frontend | `YogaV2`, upstream `origin/YogaV2`, working tree bersih saat mulai |
| Tanggal | 2026-09-11 |
| Status | **Selesai untuk lingkupnya.** 9 unit test baru lulus; 753 unit test repository lulus, 0 gagal. Belum dapat dipakai orang — layar pertama milik `FE-RAD-02`. Satu butir Definition of Done sengaja tidak dikerjakan dan disebut apa adanya pada bagian 6 |

---

## 1. Keadaan yang ditemukan di awal

Frontend Radiologi hampir seluruhnya belum ada, sesuai `RAD-CAP-029`. Yang **sudah** ada
hanya satu berkas: `rad-order.service.js`, berisi tiga fungsi yang dipakai layar dokter rawat
inap lewat `use-inpatient-supporting-service` (`CAP-015`).

Berkas itu menulis alamat endpointnya sebagai teks lepas:

```js
const RAD_ORDER_BASE_URL = "/v1/health-services/radiology-management/rad-orders";
```

Tidak ada konstanta modul, tidak ada Redux slice, tidak ada route, dan tidak ada satu pun test.

---

## 2. Yang berubah di backend sebelum task ini, dan mengapa penting

`RAD-CONF-001` bagian 8 mengubah tiga hal pada `GET /rad-orders` dan `GET /rad-orders/{id}`
pada 2026-09-11:

| Perubahan | Akibat bagi frontend |
| --- | --- |
| Balasan memuat objek `patient` berisi konteks pasien | Layar daftar **tidak boleh lagi** memanggil registrasi sekali per baris |
| Daftar menerima 19 penyaring + `limit` | Penyaringan **tidak boleh** dikerjakan di browser |
| Rincian memuat `confirmedByUserId` dan `confirmedAt` | Konfirmator tidak perlu diambil dari endpoint riwayat terpisah |

Ketiganya sudah ditampung lapisan yang dibangun task ini.

---

## 3. Perubahan yang dikerjakan

| Berkas | Keadaan | Isi |
| --- | --- | --- |
| `src/lib/constants/health-services/radiology-management/radiology-constants.jsx` | Baru | Alamat 6 grup endpoint, route, salinan teks state, batas baris, penyaring bawaan, kategori daftar pasien |
| `src/lib/hooks/health-services/radiology-management/rad-order-query-rules.js` | Baru | Fungsi murni: jepit batas baris, bersihkan penyaring, bentuk query, terapkan kategori, **bedakan gangguan dari kekosongan** |
| `src/lib/services/health-services/radiology-management/rad-order.service.js` | Diubah | Alamat pindah ke konstanta; 13 fungsi baru; **ketiga fungsi lama tidak berubah nama maupun bentuknya** |
| `.../rad-study.service.js` | Baru | 16 fungsi grup *Rad Study* |
| `.../rad-report.service.js` | Baru | 12 fungsi grup *Rad Report* |
| `.../rad-modality.service.js` | Baru | 9 fungsi master data alat |
| `.../rad-safety-requirement.service.js` | Baru | 9 fungsi master data butir keselamatan |
| `.../rad-safety-rule.service.js` | Baru | 11 fungsi master data aturan keselamatan, termasuk `GET /coverage` |
| `src/lib/state/slice/health-services/radiology-management/rad-order-slice.jsx` | Baru | 7 thunk, 4 reducer, penanda muat/gagal/basi per sumber data |
| `src/lib/state/store.jsx` | Diubah | Dua baris: import dan pendaftaran `radOrder` |
| `tests/unit/rad-order-query-rules.test.mjs` | Baru | 9 test |

### Tiga ketentuan mengikat yang sudah disiapkan tempatnya

Task ini tidak membuat layar, tetapi tiga ketentuan `RAD-ARCH-FE-001` bagian 5 sudah punya
tempat di lapisan bawah supaya layar berikutnya tidak perlu menciptakannya sendiri:

- **Butir 4 — gangguan wajib dibedakan dari kekosongan.** `resolveRadOrderListState` memisahkan
  `loading`, `error`, `never-loaded`, `empty`, dan `ready`. Slice juga **tidak mengosongkan
  daftar lama ketika permintaan gagal**: mengosongkannya membuat kegagalan terlihat persis
  seperti "tidak ada pesanan".
- **Butir 6 — isi bacaan tidak boleh disimpan di penyimpanan browser.** `rad-report.service.js`
  tidak menyimpan apa pun; ia hanya meneruskan permintaan. Tidak ada slice hasil bacaan pada
  task ini, dan itu disengaja.
- **Butir 9 — penanda cito.** `setRadOrderUrgency` menerima nilainya apa adanya; komentarnya
  menyatakan konfirmasi `BR-RAD-006` adalah kewajiban layar, bukan service.

---

## 4. Satu kekeliruan yang tertangkap sebelum sempat berjalan

Saat menulis kategori daftar pasien, nilai `EncounterType` ditulis berurutan menurut dugaan:
rawat jalan 1, rawat inap 2, gawat darurat 3. Enum backend ternyata berbeda:

```
Unknown = 0, Outpatient = 1, Emergency = 2, Inpatient = 3, MedicalCheckup = 4, Telemedicine = 5
```

Rawat inap bernilai **3**, bukan 2. Kalau tidak diperiksa, tab "Rawat Inap" akan menampilkan
pasien gawat darurat — dan tidak ada yang terlihat rusak, karena daftarnya tetap terisi.
Nilainya kini ditulis eksplisit pada `RAD_ORDER_ENCOUNTER_TYPE` dan dikunci satu test.

---

## 5. Validasi

| Yang dijalankan | Hasil |
| --- | --- |
| `node --test tests/unit/rad-order-query-rules.test.mjs` | **9 lulus, 0 gagal** |
| `node --test tests/unit/` (seluruh repository) | **753 lulus, 0 gagal** |
| `npx eslint` atas seluruh berkas yang disentuh | **Bersih** — tanpa error, tanpa warning |

> **Catatan tentang `npm run test:unit`.** Script itu memakai glob `tests/unit/**/*.test.mjs`
> yang **tidak mengembang** pada Git Bash di Windows; perintahnya keluar dengan kode 0 tanpa
> menjalankan satu test pun — lulus semu. Validasi di atas karena itu memanggil `node --test`
> langsung dengan berkas dan direktori eksplisit. Keadaan ini sudah ada sebelum task ini dan
> tidak diubah di sini.

**`MANUAL TEST: NOT FEASIBLE`** — task ini tidak menghasilkan satu pun kontrol interaktif
maupun halaman yang dapat dibuka. Tidak ada yang dapat diklik, disaring, atau dikirim.
Verifikasi manual mulai berlaku pada `FE-RAD-02`, layar pertama yang punya isi.

---

## 6. Yang sengaja **tidak** dikerjakan

| Butir | Alasan |
| --- | --- |
| Berkas `page.jsx` di `src/app/health-services/radiology-management/` | Setiap halaman pada repository ini adalah pembungkus tipis atas component view. Membuat halaman berarti membuat view — dan roadmap menyatakan `FE-RAD-01` "belum ada layar berisi". Folder kosong juga tidak dilacak Git. Kerangka route-nya diwakili `RADIOLOGY_ROUTES`; halaman pertama lahir bersama `FE-RAD-02` |
| Entri menu sidebar | Menu yang mengarah ke halaman yang belum ada lebih buruk daripada tidak ada menu. Menyusul bersama layar pertama |
| Slice untuk study, bacaan, dan master data | Roadmap `FE-RAD-01` menyebut "Redux slice", dan pesanan adalah satu-satunya yang punya konsumen pada gelombang berikutnya. Slice lain dibuat oleh task yang benar-benar memakainya, bukan disiapkan kosong |
| Base component atau hook generik baru | Dilarang `AGENTS.md` frontend dan Definition of Done task ini |
| Axios instance baru | Dilarang. Seluruh service memakai `InstanceAxios` yang sudah ada |

---

## 7. Risiko dan dependency yang masih terbuka

| Hal | Keadaan |
| --- | --- |
| `RadReport : ActAsRadiologist` belum dapat diberikan kepada peran mana pun | Penghalang dari `BE-RAD-08`, **masih terbuka**. Menahan `FE-RAD-11` pengesahan bacaan, bukan task ini |
| Tidak ada alat yang punya aturan keselamatan aktif | `BE-RAD-15` selesai sebagian; `DEC-RAD-005` masih `OPEN`. Gerbang keselamatan fail-closed, sehingga tidak satu pun pemeriksaan dapat berjalan sampai aturan disahkan. Menahan `FE-RAD-08`, bukan task ini |
| Daftar pesanan belum memakai `PagedResult` | `RAD-API-001` menargetkannya; sementara ini dijaga `limit`. Layar daftar pada `FE-RAD-06` perlu menyatakan batas itu kepada petugas, bukan menyembunyikannya |
| Tujuh keputusan `RAD-CONF-DEC-01` sampai `07` | Masih terbuka. Yang paling menyentuh frontend: `RAD-CONF-DEC-05` persiapan pasien — karena itu kategori "Pasien Persiapan" sengaja belum ada |

---

## 8. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-11 | Laporan dibuat. Sebelas berkas: sembilan baru, dua diubah. 9 test baru lulus; 753 test repository lulus. | `draft` |
