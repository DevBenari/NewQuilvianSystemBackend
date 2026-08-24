# Rekam Medis — Arsitektur Frontend

| Field | Value |
|---|---|
| Blueprint ID | `RM-BP-001` |
| Revision | `1` |
| Status | `draft` |
| Contract version | `0.1.0` |
| Frontend SHA | `c4e2ef2a6080f3ce328d2faad79be1893ac13e22` |
| Input revisions | `00-interview-decisions.md` revision `2`; `01-existing-capability-map.md` revision `1` |
| Owners | Frontend authority: `OPEN`; security/privacy authority: `OPEN` |

> **PERINGATAN DASAR DESAIN.** Disusun di atas keputusan berstatus `draft` yang belum disetujui
> owner mana pun. Lihat `RM-DEC-025`.

Dokumen ini memuat **kontrak fungsional**, bukan rancangan tampilan. Bentuk menu, rute, tab,
warna, dan tata letak **tidak** ditetapkan di sini karena belum ada brief UI yang disetujui.
Ruang yang diserahkan ke developer ditandai `DEV_DISCRETION` pada bagian 6.

---

## 1. Fondasi yang sudah terbukti dan dipakai ulang

Audit menemukan tiga hal yang tidak perlu dibangun dari nol.

| Yang sudah ada | Bukti | Dipakai untuk |
|---|---|---|
| Pemanggilan riwayat per pasien | `src/lib/hooks/health-services/clinical-management/use-doctor-cppt.js:229-232` sudah mengirim `patientId`, bukan hanya `encounterId` | Pola pemanggilan riwayat lintas kunjungan |
| Penomoran halaman riwayat per bulan | `use-doctor-cppt.js:372`, memakai `CPPT_TIMELINE_MONTH_LIMIT` | Menghindari memuat seluruh riwayat sekaligus |
| Pola service dan hook klinis | `src/lib/services/health-services/clinical-management/` berisi 6 berkas | Contoh susunan berkas untuk service rekam medis |

Yang **belum ada sama sekali**: halaman rekam medis yang berdiri sendiri, menu untuk
mencapainya, dan komponen penampil riwayat gabungan. Isi rekam medis saat ini hanya dapat
dilihat dari dalam layar antrean dokter sebagai tab.

---

## 2. Kebutuhan layar

Lima layar pada rilis pertama. Bentuknya bebas; yang mengikat adalah kebutuhan fungsionalnya.

### 2.1 Berkas Rekam Medis Pasien

Layar utama modul ini.

| Aspek | Kebutuhan |
|---|---|
| Tujuan | Menampilkan seluruh riwayat klinis seorang pasien lintas kunjungan, dalam satu tempat, urut waktu |
| Data yang dikonsumsi | `GET /medical-records/{patientId}/summary` dan `GET /medical-records/{patientId}/timeline` |
| Yang wajib terlihat | Nomor rekam medis, nama pasien, alergi aktif, diagnosis aktif, dan daftar dokumen berurut waktu |
| Yang wajib dibedakan | Status keutuhan dan status alur kerja tiap dokumen — lihat `RM-FE-008` |
| Penyaringan | Rentang tanggal, jenis dokumen, dan kunjungan tertentu |
| Pembatasan | Jumlah baris dibatasi; riwayat panjang dimuat bertahap, mencontoh pola per bulan yang sudah ada |
| Keadaan khusus | Bila `409` diterima, tampilkan pesan bahwa nomor rekam medis sudah digabungkan beserta nomor penggantinya, **jangan** menampilkan riwayat sebagian |

### 2.2 Kotak Isian Keperluan Akses

Bukan layar penuh, melainkan penghalang yang muncul sebelum isi terlihat.

| Aspek | Kebutuhan |
|---|---|
| Kapan muncul | Ketika pasien tidak punya kunjungan aktif, dan selalu ketika membuka `PrivateNote` |
| Data yang dikonsumsi | `GET /medical-record-access-purposes/options` |
| Yang wajib terlihat | Pilihan keperluan dari master, dan kotak alasan bebas bila keperluan yang dipilih menuntutnya |
| Aturan mengikat | **Isi rekam medis tidak boleh terlihat sedikit pun sebelum keperluan diisi.** Termasuk tidak boleh memuat isi di belakang layar lalu menutupinya dengan lapisan buram |
| Pesan yang jujur | Pengguna diberi tahu bahwa akses ini akan dicatat dan ditinjau. Menyembunyikan hal itu membuat pencatatan terasa seperti jebakan |

Aturan keempat perlu ditegaskan karena mudah dilanggar tanpa disadari. Memuat data lalu
menutupinya secara visual berarti isi rekam medis sudah berpindah ke perangkat pengguna dan
dapat dilihat lewat alat pengembang peramban. Penghalangnya harus terjadi sebelum permintaan
dikirim, bukan setelah jawabannya diterima.

### 2.3 Catatan Saya yang Belum Ditandatangani

| Aspek | Kebutuhan |
|---|---|
| Tujuan | Memberi dokter dan perawat cara menemukan catatannya sendiri yang belum ditandatangani |
| Data yang dikonsumsi | `GET /clinical-document-integrities/my-unsigned` |
| Yang wajib terlihat | Nama pasien, tanggal catatan, kunjungan, dan tombol menandatangani |
| Mengapa penting | Tanpa layar ini, catatan yang lupa ditandatangani tidak dapat ditemukan, dan seluruhnya akan berakhir `LockedUnsigned` saat kunjungan ditutup — hasil yang berlawanan dengan tujuan `RM-DEC-003` |

### 2.4 Tinjauan Akses

| Aspek | Kebutuhan |
|---|---|
| Tujuan | Unit rekam medis meninjau akses yang ditandai perlu ditinjau |
| Data yang dikonsumsi | `GET /medical-record-access-logs/pending-review` dan `PATCH /{id}/mark-reviewed` |
| Yang wajib terlihat | Nama pengakses, pasien, waktu, keperluan, dan alasan |
| Perhatian privasi | Layar ini memuat `AccessReason` yang bertanda sensitif. Hak aksesnya tidak boleh seluas hak baca rekam medis |

### 2.5 Master Keperluan Akses

| Aspek | Kebutuhan |
|---|---|
| Tujuan | Mengelola daftar keperluan akses |
| Data yang dikonsumsi | Endpoint master keperluan akses |
| Catatan | Mengikuti pola layar master data yang sudah ada di `src/components/view/health-services/master-data/`. Tidak perlu pola baru |

---

## 3. Aksi per peran

| Aksi | Dokter | Perawat | Kepala unit | Petugas rekam medis | Koder | Auditor |
|---|:---:|:---:|:---:|:---:|:---:|:---:|
| Membuka berkas rekam medis | Ya | Ya | Ya | Ya | Ya | Tidak |
| Membuka `PrivateNote` | Ya | Tidak | Ya | Tidak | Tidak | Tidak |
| Menandatangani catatan sendiri | Ya | Ya | Ya | Tidak | Tidak | Tidak |
| Menambah addendum pada catatan sendiri | Ya | Ya | Ya | Tidak | Tidak | Tidak |
| Menambah addendum sebagai pengganti | Tidak | Tidak | Ya | Tidak | Tidak | Tidak |
| Menetapkan penulis berhalangan | Tidak | Tidak | Ya | Tidak | Tidak | Tidak |
| Meninjau jejak akses | Tidak | Tidak | Tidak | Ya | Tidak | Ya |
| Mengelola master keperluan | Tidak | Tidak | Tidak | Ya | Tidak | Tidak |

Tabel ini **saran**, sejalan dengan permission-audit-matrix bagian 6. Pemetaan peran organisasi
ke izin adalah kewenangan security/privacy owner yang belum ditunjuk.

Aturan yang mengikat frontend: **tombol yang tidak berhak ditekan tidak ditampilkan**, bukan
ditampilkan lalu gagal saat ditekan. Untuk addendum, endpoint `GET /authority/{kind}/{id}`
disediakan khusus supaya frontend dapat mengetahuinya sebelum menggambar tombol.

---

## 4. Penanganan keadaan

| Keadaan | Yang wajib dilakukan |
|---|---|
| Sedang memuat | Tampilkan penanda memuat per bagian, bukan menutup seluruh layar. Riwayat panjang dimuat bertahap |
| Kosong | Bedakan "pasien belum punya riwayat" dari "penyaringan tidak menemukan apa pun". Keduanya berbeda maknanya bagi pengguna |
| Gagal sebagian | Bila satu sumber dokumen gagal dimuat, tampilkan bagian lain dan tandai bagian yang gagal. **Jangan** menampilkan riwayat tidak lengkap tanpa memberi tahu |
| Gagal seluruhnya | Sediakan tombol coba lagi |
| Jejak akses gagal (`503`) | Tampilkan pesan bahwa berkas tidak dapat dibuka saat ini, bukan pesan galat teknis |
| Data basi | Riwayat rekam medis boleh disimpan sementara di sisi klien **kecuali** `PrivateNote`, yang tidak boleh disimpan sama sekali |
| Kiriman ganda | Tombol menandatangani dan menambah addendum wajib dikunci setelah ditekan. Addendum ganda tidak dapat dihapus, sehingga pencegahannya harus di depan |

Baris ketiga adalah aturan yang paling mudah dilanggar dan paling berbahaya di modul ini.
Riwayat rekam medis yang tampil tidak lengkap tanpa peringatan akan dibaca sebagai riwayat
lengkap, dan keputusan klinis dapat diambil di atasnya.

Baris terakhir juga perlu perhatian: addendum tidak dapat diubah maupun dihapus. Tekan ganda
menghasilkan dua koreksi kembar yang menempel selamanya pada rekam medis.

---

## 5. Susunan berkas yang disarankan

Mengikuti pola yang sudah ada. Nama boleh berbeda selama polanya konsisten.

```text
src/app/health-services/medical-record-management/
├── medical-records/page.jsx                    # Baru
├── my-unsigned-notes/page.jsx                  # Baru
└── access-review/page.jsx                      # Baru

src/components/view/health-services/medical-record-management/
├── medical-records/                             # Baru
├── my-unsigned-notes/                           # Baru
└── access-review/                               # Baru

src/lib/services/health-services/medical-record-management/
├── medical-record.service.js                    # Baru
├── clinical-document-integrity.service.js       # Baru
├── clinical-note-addendum.service.js            # Baru
└── medical-record-access-log.service.js         # Baru

src/lib/hooks/health-services/medical-record-management/
├── use-medical-record-timeline.js               # Baru
├── use-clinical-document-signing.js             # Baru
└── use-medical-record-access-review.js          # Baru

src/utils/menu-sidebar/menu-items.jsx            # Diperbarui — entri menu baru
```

Catatan tentang menu: kunci `menuLaboratorium`, `menuRadiologi`, `menuMCU`, dan `menuOptik`
pada `src/components/features/left-sidebar/left-sidebar-menu-handle.jsx:6-19` **bukan** definisi
menu, melainkan daftar nama kelompok menu bersarang. Menu sesungguhnya didefinisikan di
`src/utils/menu-sidebar/menu-items.jsx`. Jangan tertukar.

---

## 6. Matriks kewenangan UI

Urutan kewenangan: keamanan/privasi/invariant → brief produk atau UI yang disetujui → konvensi
proyek → kebijakan developer.

| Decision ID | Area | Owner | Status | Allowed range | Evidence |
|---|---|---|---|---|---|
| `RM-FE-001` | Penanda status keutuhan wajib terlihat pada setiap dokumen | Clinical governance | `draft` | Wajib terlihat; bentuk visual bebas | `RM-DEC-003` |
| `RM-FE-002` | Addendum wajib menempel pada dokumen induknya | Clinical governance | `draft` | Wajib; tata letak bebas | `RM-DEC-004` |
| `RM-FE-003` | Isian keperluan akses wajib mendahului tampilnya isi | Security/privacy | `draft` | Wajib mendahului, termasuk mendahului permintaan ke server | `RM-DEC-005` |
| `RM-FE-006` | Keterangan bahwa label kerahasiaan belum membatasi akses | Security/privacy | `draft` | Wajib ada; susunan kalimat bebas | `RM-DEC-018` |
| `RM-FE-007` | `PrivateNote` tidak tampil pada tampilan rutin | Security/privacy | `draft` | Wajib; bentuk pembukaan bebas | `RM-DEC-022` |
| `RM-FE-008` | Status keutuhan dan status alur kerja harus dapat dibedakan | Clinical governance | `draft` | Wajib dapat dibedakan; bentuk visual bebas | `RM-DEC-013` |
| `RM-FE-009` | Keterangan bahwa baru CPPT yang tunduk aturan keutuhan | Product/domain | `draft` | Wajib ada selama cakupannya masih satu jenis dokumen | Arsitektur backend bagian 7 |
| `RM-FE-010` | Tombol yang tidak berhak ditekan tidak ditampilkan | Frontend | `draft` | Wajib | Bagian 3 |
| `RM-FE-004` | Bentuk navigasi: menu, rute, tab, modal, atau drawer | Frontend | `DEV_DISCRETION` | Mengikuti konvensi proyek | Belum ada brief UI |
| `RM-FE-005` | Tata letak, warna, ikon, komponen tabel | Frontend | `DEV_DISCRETION` | Mengikuti konvensi proyek | Belum ada brief UI |
| `RM-FE-011` | Cara memuat bertahap: gulir tanpa batas, tombol muat lagi, atau halaman | Frontend | `DEV_DISCRETION` | Bebas selama ada pembatasan jumlah | Belum ada brief UI |
| `RM-FE-012` | Bentuk penanda dokumen gagal dimuat sebagian | Frontend | `DEV_DISCRETION` | Bebas selama keberadaannya jelas terlihat | Belum ada brief UI |

Delapan butir pertama **tidak** boleh diputuskan developer, karena berasal dari keputusan
keamanan, privasi, atau invariant klinis. Empat butir terakhir memang diserahkan ke developer.

---

## 7. Yang sengaja tidak dibuat pada rilis pertama

| Yang ditolak | Alasan |
|---|---|
| Pencetakan resume medis | Cakupan 6, rilis berikutnya menurut `RM-DEC-002` |
| Layar kelengkapan berkas | Cakupan 4, rilis berikutnya |
| Layar verifikasi koding | Cakupan 5, rilis berikutnya |
| Layar peminjaman berkas | Cakupan 7, rilis berikutnya |
| Portal pasien untuk melihat rekam medis sendiri | Di luar scope. Area `SelfServices` tidak memuat kemampuan klinis apa pun saat ini |
| Penyuntingan catatan klinis dari layar rekam medis | Melanggar `RM-DEC-001`. Penulisan tetap di layar pelayanan masing-masing |
| Pengunduhan seluruh riwayat sebagai satu berkas | Menciptakan salinan rekam medis di luar sistem tanpa jejak. Perlu keputusan pelepasan informasi lebih dulu, yaitu cakupan 7 |

---

## 8. Ketergantungan pengujian

| Kebutuhan | Keadaan sekarang |
|---|---|
| Uji alur klinis di frontend | **Tidak ada.** `tests/` hanya memuat 4 berkas, tidak satu pun menyentuh alur klinis |
| Uji rute dapat dicapai | Ada, `tests/e2e/route-smoke.spec.mjs`. Rute rekam medis baru perlu ditambahkan ke sana |
| Uji regresi komponen dasar | Ada, `tests/unit/base-components-regression.test.mjs` |

Empat uji antarmuka pada acceptance test matrix — `AT-RM-39` sampai `AT-RM-42` — memerlukan
pengujian yang belum ada padanannya di repository. Menyusun urutannya adalah pekerjaan
`/plan-module-delivery`.

---

## 9. Traceability

| Kebutuhan frontend | Decision | Acceptance test |
|---|---|---|
| Layar berkas rekam medis | `RM-DEC-002` | `AT-RM-09`, `AT-RM-31` |
| Pembedaan dua status | `RM-DEC-013` | `AT-RM-39` |
| Addendum menempel pada induknya | `RM-DEC-004` | `AT-RM-40` |
| Isian keperluan mendahului isi | `RM-DEC-005` | `AT-RM-41` |
| Keterangan label kerahasiaan | `RM-DEC-018` | `AT-RM-42` |
| `PrivateNote` tersembunyi | `RM-DEC-022` | `AT-RM-37` |
| Layar catatan belum ditandatangani | `RM-DEC-003` | `AT-RM-18` |
| Layar tinjauan akses | `RM-DEC-005` | `AT-RM-29` |
| Penanganan pasien hasil penggabungan | `RM-CAP-007` | `AT-RM-22` |
