# Laporan Perubahan Frontend — `FE-RAD-06`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RAD-06` |
| Judul | Antrian pesanan masuk |
| Epic | `EPIC RAD-05` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 4, gelombang `MVP-3` |
| Contract version | `RAD-API-001` grup *Rad Order*; `RAD-STATE-001` bagian 1 |
| Acceptance criteria | Transisi sesuai `RAD-STATE-001` bagian 1 |
| Test yang diminta roadmap | Tindakan yang tidak sah tidak ditawarkan di layar |
| Wewenang UI | Seluruhnya `DEV_DISCRETION` sepanjang mengikuti pola yang ada |
| Dependency | `FE-RAD-01` **selesai** |
| Task mode | `FRONTEND` — frontend target tulis; backend strict read-only kecuali laporan ini |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Branch frontend | `YogaV2`, upstream `origin/YogaV2` |
| Tanggal | 2026-09-11 |
| Status | **Selesai.** 15 unit test baru lulus; 820 unit test repository lulus, 0 gagal; lint 0 error |

---

## 1. Dua delta antara kontrak dan backend — **temuan yang perlu keputusan**

Aturan aksi disusun dari penjaga `RadOrderService` yang sebenarnya, bukan dari tabel kontrak.
Keduanya **tidak sama**, dan bedanya bukan penulisan.

### 1.1 `Mulai` diterima dari `Accepted` — benign, layar mengikuti backend

| Sumber | Status asal yang diterima |
| --- | --- |
| `RAD-STATE-001` bagian 1 | `Scheduled` saja |
| `RadOrderService.StartAsync` | `Accepted` **dan** `Scheduled` |

Layar mengikuti backend. Memesan lalu langsung mengerjakan tanpa penjadwalan adalah alur nyata
pasien yang datang langsung, dan menyembunyikannya berarti menyembunyikan aksi yang sah.

### 1.2 `Tahan` diterima dari status terminal — **cacat, layar sengaja lebih ketat**

`RadOrderService.HoldAsync` hanya menolak ketika pesanan **sudah** `OnHold`. Ia tidak memeriksa
status asalnya sama sekali:

```csharp
if (order.OrderStatus == RadOrderStatus.OnHold) { ...tolak... }

var from = order.OrderStatus;
order.StatusBeforeHold = from;
order.OrderStatus = RadOrderStatus.OnHold;
```

Akibatnya backend hari ini **menerima penahanan pesanan `Completed`, `Cancelled`, dan
`Rejected`** — padahal `RAD-STATE-001` bagian 1 menyebut `Tahan` hanya sah dari `Requested`,
`Accepted`, `Scheduled`, dan `InProgress`, dan menyebut ketiganya status terminal.

**Mengapa ini bukan sekadar kerapian.** Menahan pesanan `Completed` menyimpan `Completed` pada
`StatusBeforeHold`. `Lanjutkan` kemudian mengembalikan pesanan ke `Completed` — status terminal
dimasuki ulang lewat pintu samping, lengkap dengan dua baris `RadTransitionHistory` yang mengaku
terjadi transisi yang dilarang kontrak. Riwayat yang seharusnya menjadi jejak audit justru
mencatat perpindahan yang tidak pernah boleh ada.

**Yang dikerjakan layar:** `Tahan` tidak ditawarkan pada ketiga status terminal. Layar lebih
ketat daripada backend, dan itu disengaja — dicatat pada `HOLD_LEBIH_KETAT_DARIPADA_BACKEND`
beserta alasannya, dan dikunci unit test `S8`.

**Yang perlu Anda putuskan:** apakah `HoldAsync` diperbaiki agar menolak status terminal.
Frontend sudah aman; jalur API-nya belum. Selama tidak diperbaiki, siapa pun yang memanggil
`PUT /rad-orders/{id}/hold` langsung tetap dapat merusak riwayat sebuah pesanan yang sudah
selesai.

---

## 2. Aksi yang ditawarkan per status

| Status | Aksi |
| --- | --- |
| `Requested` | Terima, Tolak *(alasan wajib)*, Tahan, Batalkan *(alasan wajib)* |
| `Accepted` | Jadwalkan, Mulai, Tahan, Batalkan |
| `Scheduled` | Mulai, Tahan, Batalkan |
| `InProgress` | Selesaikan, Tahan |
| `OnHold` | Lanjutkan, Batalkan |
| `CancelRequested` | Batalkan |
| `Completed`, `Cancelled`, `Rejected` | **Tidak ada** |

Dua hal yang sengaja begitu:

- **`InProgress` tidak menawarkan Batalkan.** Backend memang tidak menerimanya. Pesanan yang
  study-nya sudah berjalan dihentikan pada tingkat study, supaya paparan yang sudah terjadi
  tetap tercatat.
- **Status terminal menampilkan kalimat, bukan barisan tombol mati.** Tombol yang dinonaktifkan
  berjajar hanya membuat petugas mencari-cari sebabnya.

---

## 3. Perubahan yang dikerjakan

| Berkas | Keadaan |
| --- | --- |
| `src/lib/hooks/health-services/radiology-management/rad-order-transition-rules.js` | Baru — fungsi murni; aturan aksi, alasan wajib, dan kesegeraan |
| `src/lib/hooks/health-services/radiology-management/use-rad-order-queue.jsx` | Baru — controller antrian |
| `src/lib/constants/health-services/radiology-management/rad-order-constants.jsx` | Diubah — meta 10 status, salinan teks antrian, 6 kartu ringkasan |
| `src/components/view/health-services/radiology-management/rad-orders/rad-order-queue-view.jsx` | Baru — layar antrian |
| `src/style/health-services/radiology-management/rad-orders/rad-order-queue.module.css` | Baru — 6 kelas, design token |
| `src/app/health-services/radiology-management/rad-orders/…` | Baru — 2 berkas route |
| `src/utils/menu-sidebar/menu-items.jsx` | Diubah — grup **Radiologi** baru, satu entri |
| `tests/unit/rad-order-transition-rules.test.mjs` | Baru — 15 test |

`store.jsx` **tidak disentuh** — `radOrder` sudah terdaftar sejak `FE-RAD-01`.

---

## 4. Yang datang gratis dari `RAD-CONF-001` bagian 8

Layar ini adalah pemakai pertama dari empat penghambat yang dibereskan pada hari yang sama:

| Perubahan backend | Yang dipakai layar |
| --- | --- |
| `orderNumber` | Kolom pertama tabel, dan penanda pesanan pada setiap modal konfirmasi |
| Objek `patient` pada setiap baris | Kolom Pasien dan Kunjungan — **tanpa satu pun panggilan tambahan ke registrasi** |
| 19 penyaring | Status, kesegeraan, kategori kunjungan, dan pencarian bebas, **seluruhnya dikerjakan server** |
| `limit` | Daftar dibatasi 200 baris terbaru, dan layar menyatakannya apa adanya pada keterangan daftar |

Tanpa perubahan itu, kolom Pasien menuntut 50 panggilan registrasi untuk 50 baris, dan nama
pasien berisiko menempel pada baris milik pasien lain.

---

## 5. Validasi

| Yang dijalankan | Hasil |
| --- | --- |
| `node --test tests/unit/rad-order-transition-rules.test.mjs` | **15 lulus, 0 gagal** |
| `node --test tests/unit/` (seluruh repository) | **820 lulus, 0 gagal** |
| `npx eslint` atas seluruh berkas yang disentuh | **0 error, 1 warning** |
| `npm run build` | **Compiled successfully in 33.7s.** Route terdaftar: `/health-services/radiology-management/rad-orders` (static) |

Satu warning `react-hooks/set-state-in-effect` berasal dari `use-rad-order-form.jsx`
(`FE-RAD-05`), bukan dari berkas task ini.

**`MANUAL TEST: NOT FEASIBLE`.** Menjalankan layar ini menuntut pesanan radiologi yang sudah ada
pada lingkungan pengembangan beserta sesi petugas yang memegang `RadOrder : Read` dan
`: Process`; sesi login tidak tersedia dari sini. Perilaku yang diminta roadmap — tindakan tidak
sah tidak ditawarkan — dikunci 15 unit test, termasuk keenam transisi terlarang pada `S9` dan
penahanan status terminal pada `S8`.

---

## 6. Yang sengaja **tidak** dikerjakan

| Butir | Alasan |
| --- | --- |
| Halaman rincian pesanan | Roadmap menempatkannya bersama layar verifikasi dan gerbang keselamatan, `FE-RAD-08` dan seterusnya. Antrian ini bekerja penuh tanpanya |
| Entri menu untuk formulir pemesanan | Formulir `FE-RAD-05` dibuka dari konteks klinis dengan `?encounterId=`. Menu yang membukanya tanpa kunjungan hanya menyuguhkan formulir yang dokternya harus mencari pasiennya sendiri |
| Kategori "Pasien Persiapan" | Backend belum punya indikator persiapan — `RAD-CONF-DEC-05` masih terbuka. Empat kategori lain tersedia |
| Paginasi | `GET /rad-orders` belum memakai `PagedResult`; yang ada batas 200 baris. Perpindahannya task tersendiri pada `RAD-API-001` |
| Base component baru | Dilarang `AGENTS.md` |

---

## 7. Risiko dan dependency yang masih terbuka

| Hal | Keadaan |
| --- | --- |
| **`HoldAsync` menerima status terminal** | Temuan task ini, bagian 1.2. Frontend sudah aman; jalur API belum. **Perlu keputusan pemilik modul** |
| Daftar dibatasi 200 baris tanpa paginasi | Petugas pada unit sibuk dapat kehilangan baris terlama. Keterangan daftar menyatakannya, tetapi itu penanganan sementara |
| Tidak ada aturan keselamatan `Active` | `RAD-OPEN-011` terbuka. Pesanan dapat diterima dan dijadwalkan dari layar ini, tetapi **pemeriksaannya tetap ditolak gerbang** sampai Komite Medis menunjuk pemegang akun pengesah |

---

## 8. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-11 | Laporan dibuat. 6 berkas baru, 2 diubah. 15 test baru lulus; 820 test repository lulus; lint 0 error. Dua delta kontrak–backend dicatat; satu di antaranya cacat yang perlu keputusan. | `draft` |
