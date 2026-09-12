# Laporan Perubahan Frontend — `FE-RWI-039`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-039` |
| Judul | Selisih Tempat Tidur menjadi laporan diagnostik yang terbaca |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md), kartu `FE-RWI-039` |
| Trace | Bukti runtime pemilik 28 Agustus 2026; `FE-INP-10`; skema §15 dan §24.1; `FR-RI-135` s.d. `138` |
| Contract version | API monitoring bed drift `0.4.0`; route `FE-INP-02`; permission `InpatientMonitoring : Read` |
| Wewenang UI | Batas read-only dan tujuan navigasi dikunci skema §15; gaya visual `DEV_DISCRETION`. **Wewenang itu tidak dipakai** — nol baris JSX dan nol baris CSS diubah |
| Dependency | `FE-RWI-036` ✅ |
| Klasifikasi | `LIGHT` — berkas source diubah **0**, berkas test diubah 1 |
| Task mode | `FRONTEND` |
| Model | `claude-opus-5` |
| Commit frontend saat dikerjakan | `7f6b9356f6349d516570d6d603ca026f2c7f4ec2` pada branch `HamzahV2` |
| Tanggal | 12 September 2026 |
| Status | ✅ **Selesai 12 September 2026 berdasarkan bukti source, dengan nol baris source baru.** Keenam acceptance criteria terbukti sudah terpenuhi oleh source yang ada dan kini dikunci enam test baru. Satu butir verifikasi **tidak dijalankan dan ditulis apa adanya**: pembuktian manual tiga keadaan di peramban `NOT RUN`, menunggu `RWI-UI-GAP-007` |

---

## 1. Keadaan yang ditemukan di awal

Task ini berstatus ⛔ `TERBLOKIR` sejak revision 2, dengan dua alasan yang dicatat pada
`Urutan-Pekerjaan-Rawat-Inap.md` — berkas itu dipindah dan diganti nama pada hari yang sama
menjadi [`Sisa-Pekerjaan-Rawat-Inap.md`](../../../../Sisa-Pekerjaan-Rawat-Inap.md): approval
skema layar, dan `RWI-UI-GAP-007`.

**Kedua alasan itu diperiksa ulang 12 September 2026 dan hasilnya berbeda dari catatannya.**

| Alasan tercatat | Keadaan sebenarnya |
| --- | --- |
| Approval skema layar | **Sudah ada.** Baris `Wewenang UI` pada kartu roadmap berbunyi "Batas read-only dan tujuan navigasi **dikunci skema §15**". Yang belum disetujui hanya gaya visualnya, dan itu justru `DEV_DISCRETION` |
| `RWI-UI-GAP-007` | **Benar, tetapi tidak memblokir pembangunan.** Kartu roadmap sendiri menyebutnya "data runtime **untuk pembuktian**". Ia menahan bukti, bukan kode |

Pemeriksaan source berikutnya menemukan hal yang lebih menentukan: **layarnya sudah
dibangun**, lewat `FE-RWI-017` dan disempurnakan pada gelombang `FE-RWI-036`. Yang tidak
pernah terjadi hanyalah penutupan formalnya — folder laporan memuat `FE-RWI-036`, `037`,
`038`, `040`, dan `041`, tetapi tidak `039`.

---

## 2. Proses bisnis dari sisi pengguna

Supervisor membuka laporan Selisih Tempat Tidur secara berkala. `RWI-DEC-039` menurunkan
`MstBed.BedStatus` menjadi salinan; sumber kebenarannya `InpBedPlacement` dan
`InpBedReservation`. Laporan ini satu-satunya pengawas atas satu-satunya arah tulis lintas
modul yang disetujui. Bila selisihnya tidak pernah dibaca, salinan itu menyimpang diam-diam
sampai seorang pasien ditempatkan di tempat tidur yang sudah ada orangnya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas source yang berubah

**Nol.** Ini temuan utama laporan ini, dan disebut apa adanya alih-alih dibuat terlihat
seperti pekerjaan baru.

### 3.2 Berkas test yang berubah

| Berkas | Perubahan |
| --- | --- |
| `tests/unit/inpatient-monitoring.test.mjs` | Enam test baru, satu per acceptance criteria, mengunci bentuk yang sudah ada agar tidak hilang tanpa sengaja |

### 3.3 Kenapa penguncian ini bukan formalitas

Kriteria 2 adalah contohnya. Tombol **Buka Papan Tempat Tidur** tinggal di `Hero`, yaitu
**di luar** blok `{!loadError ? (<DataTable .../>) : null}`. Memindahkannya ke dalam tabel
akan terasa wajar bagi pembaca berikutnya — tombol per baris memang sudah ada di kolom
Tindak Lanjut — tetapi laporan kosong dan laporan gagal akan langsung menjadi jalan buntu.
Test kriteria 2 membandingkan posisi indeks `<Hero`, `bed-drift-bed-board-link`, dan
`<DataTable` supaya perpindahan itu gagal di CI, bukan ditemukan pemilik di produksi.

---

## 4. Verifikasi

| Butir | Hasil |
| --- | --- |
| `npm run test:unit` | **754 lulus, 0 gagal**; 30 di antaranya pada `inpatient-monitoring.test.mjs` |
| `npm run lint:errors` | **0 error** |
| `npm run build` | **✓ Compiled successfully**, `postbuild` berhasil |
| Manual state mismatch, kosong, dan gagal di peramban | **`NOT RUN`** — `RWI-UI-GAP-007`; papan menunjukkan nol bed pada lingkungan target, sehingga keadaan mismatch tidak dapat dihadirkan |

---

## 5. Acceptance criteria

| # | Kriteria | Hasil | Bukti pada source |
| --- | --- | --- | --- |
| 1 | Setiap baris memperlihatkan bed, lokasi, status salinan, status seharusnya, selisih, episode pemegang | ✅ | Keenam `header:` pada `inpatient-bed-drift-table-columns.jsx`; kedua nilai status menyebut asalnya |
| 2 | **Buka Papan Tempat Tidur** terlihat pada state berisi maupun kosong | ✅ | Tombolnya di `Hero`, di luar blok `DataTable` yang menghilang saat gagal |
| 3 | Navigasi mempertahankan konteks unit/bed bila tersedia | ✅ | Tingkat layar membawa `filters.serviceUnitId`; tingkat baris membawa `serviceUnitId`, `bedCode`, `bedName` |
| 4 | Empty state dinyatakan sebagai keadaan positif | ✅ | `emptyTitle="Tidak ada tempat tidur yang statusnya menyimpang."`; nol kata bernada galat pada blok kosongnya |
| 5 | Gagal baca menyediakan **Coba Lagi** | ✅ | `bed-drift-retry` pada `Hero`; tabel disembunyikan supaya data lama tidak terbaca masih berlaku |
| 6 | Tidak ada tombol "Perbaiki" atau request tulis | ✅ | Nol `Perbaiki`/`Rekonsiliasi`/`Sinkronkan` pada ketiga berkas; nol `.post(`/`.put(`/`.patch(`/`.delete(` pada hook |

---

## 6. Catatan penutup

**Status ini diperoleh dari pembacaan source, bukan dari penulisan source.** Siapa pun yang
membaca laporan ini perlu tahu bedanya: kalau di kemudian hari ditemukan bahwa kartu
roadmap sebenarnya menuntut sesuatu yang tidak tertulis pada keenam acceptance criteria-nya,
task ini perlu dibuka kembali, sebab tidak ada satu baris pun yang ditulis untuk menutup
tuntutan itu.

**Satu butir yang tetap terbuka.** Pembuktian manual di peramban `NOT RUN` dan menunggu
`RWI-UI-GAP-007`. Menutupnya adalah pekerjaan menyiapkan data master pada lingkungan
target, bukan menulis kode, dan menanam data tiruan di frontend dilarang.

**Satu temuan di luar task ini, dilaporkan bukan ditambal diam-diam.** Saat memeriksa
kriteria 8 milik `FE-RWI-035` atas keenam layar bukti runtime, layar Butir Administrasi
Rawat Inap milik `FE-RWI-040` ditemukan **tidak memiliki tombol Coba Lagi**, padahal
kriteria 5 task itu menuntutnya dan laporannya menyatakannya ada di `Hero`. Perbaikannya
dicatat pada laporan `FE-RWI-035`.
