# Rekam Medis — Arsitektur Frontend

| Field | Value |
|---|---|
| Blueprint ID | `RM-BP-001` |
| Revision | `2` |
| Status | `approved` — disahkan Yoga Aji Pratama 27 Agustus 2026 (`RM-DEC-028`) |
| Contract version | `0.1.0` — `approved` 27 Agustus 2026 |
| Frontend SHA | `c4e2ef2a6080f3ce328d2faad79be1893ac13e22` |
| Input revisions | `00-interview-decisions.md` revision `4`; `01-existing-capability-map.md` revision `2` |
| Owners | Frontend authority: **Yoga Aji Pratama** (`RM-DEC-028`); security/privacy authority: **Yoga Aji Pratama** (`RM-DEC-027`) |

> **DASAR DESAIN.** Revisi 1 disusun di atas keputusan berstatus `draft` tanpa owner. Keadaan
> itu sudah berubah: `RM-DEC-027` (26 Agustus 2026) dan `RM-DEC-028` (27 Agustus 2026)
> menetapkan Yoga Aji Pratama sebagai pemilik proses, tata kelola klinis, keamanan/privasi, API,
> dan frontend. **Batas yang tetap berlaku:** pengesahan ini tidak menggantikan tinjauan komite
> medik maupun pihak perlindungan data bila kelak keduanya ditunjuk.

Bagian 1 sampai 9 memuat **kontrak fungsional** modul. Bagian 10 dan 11 memuat **brief UI** —
entri menu, rute, dan skema tampilan per layar — yang ditambahkan pada revisi 2 dan disahkan
pemilik frontend.

Pembagian kewenangannya tetap tegas. Bagian 1 sampai 9 mengikat karena berasal dari keputusan
klinis dan privasi. Bagian 10 dan 11 mengikat karena disahkan pemilik frontend, tetapi **hanya**
pada susunan navigasi dan penempatan wilayah layar. Warna, jarak, tipografi, dan pemilihan
komponen tetap `DEV_DISCRETION` — lihat bagian 6.

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

Lima layar pada rilis pertama. Bagian ini menetapkan **kebutuhan fungsionalnya**; skema
tampilan tiap layar ada pada **bagian 11**, dan entri menu yang mencapainya pada **bagian 10**.

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

Isi entri menu yang harus ditambahkan ke berkas itu ditetapkan pada **bagian 10**, lengkap
dengan kunci, rute, dan izin yang menyertainya.

---

## 6. Matriks kewenangan UI

Urutan kewenangan: keamanan/privasi/invariant → brief produk atau UI yang disetujui → konvensi
proyek → kebijakan developer.

| Decision ID | Area | Owner | Status | Allowed range | Evidence |
|---|---|---|---|---|---|
| `RM-FE-001` | Penanda status keutuhan wajib terlihat pada setiap dokumen | Clinical governance | `approved` | Wajib terlihat; bentuk visual bebas | `RM-DEC-003` |
| `RM-FE-002` | Addendum wajib menempel pada dokumen induknya | Clinical governance | `approved` | Wajib; tata letak bebas | `RM-DEC-004` |
| `RM-FE-003` | Isian keperluan akses wajib mendahului tampilnya isi | Security/privacy | `approved` | Wajib mendahului, termasuk mendahului permintaan ke server | `RM-DEC-005` |
| `RM-FE-006` | Keterangan bahwa label kerahasiaan belum membatasi akses | Security/privacy | `approved` | Wajib ada; susunan kalimat bebas | `RM-DEC-018` |
| `RM-FE-007` | `PrivateNote` tidak tampil pada tampilan rutin | Security/privacy | `approved` | Wajib; bentuk pembukaan bebas | `RM-DEC-022` |
| `RM-FE-008` | Status keutuhan dan status alur kerja harus dapat dibedakan | Clinical governance | `approved` | Wajib dapat dibedakan; bentuk visual bebas | `RM-DEC-013` |
| `RM-FE-009` | Keterangan bahwa baru CPPT yang tunduk aturan keutuhan | Product/domain | `approved` | Wajib ada selama cakupannya masih satu jenis dokumen | Arsitektur backend bagian 7 |
| `RM-FE-010` | Tombol yang tidak berhak ditekan tidak ditampilkan | Frontend | `approved` | Wajib | Bagian 3 |
| `RM-FE-004` | Susunan navigasi: satu menu induk `Rekam Medis` berisi tiga entri, ditambah satu entri pada menu Master Data yang sudah ada | Frontend | `approved` | Susunan dan rute mengikat; ikon, urutan visual, dan penamaan kelompok bebas | `RM-DEC-028`; bagian 10 |
| `RM-FE-013` | Menu `Tinjauan Akses` hanya tampil bagi pemegang `MedicalRecordAccessLog : Read` | Security/privacy | `approved`, **belum ditegakkan** | Wajib sebagai aturan. Penegakannya **ditunda** 27 Agustus 2026 karena penyaringan menu per izin belum ada di frontend; tercatat sebagai utang terbuka. Isi tetap ditahan `403`. Menambal dengan pencocokan nama peran **dilarang** — lihat bagian 10.5 | permission-audit-matrix bagian 6; `RM-DEC-005` |
| `RM-FE-014` | Berkas rekam medis dicapai lewat pencarian pasien, bukan lewat daftar seluruh pasien | Security/privacy | `approved` | Wajib; bentuk pencarian bebas | Bagian 11.1; kontrak hanya menyediakan endpoint per `patientId` |
| `RM-FE-015` | Isi dokumen, addendum, dan `PrivateNote` dibuka dari panel detail, bukan langsung dari baris riwayat | Frontend | `approved` | Wajib; bentuk panel bebas | Bagian 11.1; `RM-FE-002`, `RM-FE-007` |
| `RM-FE-016` | Layar master keperluan akses ditempatkan di bawah menu `Master Data` yang sudah ada, bukan di bawah menu `Rekam Medis` | Frontend | `approved` | Wajib; mengikuti pengelompokan API | api-contract bagian 7 |
| `RM-FE-005` | Tata letak, warna, ikon, komponen tabel | Frontend | `DEV_DISCRETION` **terbatas** | Penempatan wilayah layar ditetapkan bagian 11. Warna, jarak, tipografi, dan pemilihan komponen tetap bebas | Bagian 11 |
| `RM-FE-011` | Cara memuat bertahap: gulir tanpa batas, tombol muat lagi, atau halaman | Frontend | `DEV_DISCRETION` | Bebas selama ada pembatasan jumlah | Belum ada brief UI |
| `RM-FE-012` | Bentuk penanda dokumen gagal dimuat sebagian | Frontend | `DEV_DISCRETION` | Bebas selama keberadaannya jelas terlihat | Belum ada brief UI |

Tiga belas butir pertama **tidak** boleh diputuskan developer. Delapan di antaranya berasal dari
keputusan keamanan, privasi, atau invariant klinis; lima sisanya — `RM-FE-004` dan `RM-FE-013`
sampai `RM-FE-016` — ditetapkan pemilik frontend pada revisi 2 dan dirinci pada bagian 10 dan 11.

Tiga butir terakhir tetap diserahkan ke developer. `RM-FE-005` menyusut menjadi terbatas: apa
yang tampil dan di wilayah mana sudah ditetapkan, yang bebas tinggal bagaimana ia digambar.

Seluruh status `draft` pada revisi 1 dinaikkan menjadi `approved` pada revisi 2, mengikuti
`RM-DEC-027` dan `RM-DEC-028`. Tidak ada isi butir yang berubah; yang berubah hanya statusnya.

---

## 7. Yang sengaja tidak dibuat pada rilis pertama

| Yang ditolak | Alasan |
|---|---|
| Pencetakan resume medis | Cakupan 6, rilis berikutnya menurut `RM-DEC-002` |
| Layar kelengkapan berkas | Cakupan 4, rilis berikutnya |
| Layar verifikasi koding | Cakupan 5, rilis berikutnya |
| Layar peminjaman berkas | Cakupan 7, rilis berikutnya |
| Portal pasien untuk melihat rekam medis sendiri | Di luar scope. Area `SelfServices` tidak memuat kemampuan klinis apa pun saat ini |
| Layar penetapan penulis berhalangan | **Ditunda 27 Agustus 2026** oleh pemilik frontend. Ketiga endpointnya masih `Rencana`, sehingga tidak ada yang tertahan. Akibatnya: `UnitHeadGrant` hanya dapat dibuat lewat API langsung. Rinciannya pada bagian 10.6 |
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

---

## 10. Navigasi dan entri menu

Ditambahkan pada revisi 2. Menetapkan `RM-FE-004`, `RM-FE-013`, dan `RM-FE-016`.

### 10.1 Tempat modul ini di sidebar

Menu rekam medis masuk ke kelompok **Pelayanan Kesehatan** yang sudah ada, sejajar dengan
`Farmasi`, `Manajemen Pasien`, `Dokter`, `Instalasi Gawat Darurat`, dan `Rawat Jalan`. Bukan
kelompok baru — modul ini melayani proses klinis yang sama, hanya memandangnya per pasien
alih-alih per kunjungan.

Susunan yang diikuti, dengan contoh yang sudah berjalan di `src/utils/menu-sidebar/menu-items.jsx`:

| Tingkat | Field yang dipakai | Contoh yang sudah ada |
|---|---|---|
| 1 — menu induk | `subMenu` | `healthServicesPatientManagement`, baris 811 |
| 2 — entri menu | `pathname` | `healthServicesPharmacyManagementPrescriptions`, baris 804 |
| 3 — entri bersarang | `subItems` | `healthServicesPatientManagementMasterData`, baris 818 |

Kedalaman tiga tingkat sudah didukung **tanpa mengubah komponen sidebar mana pun**: `subItems`
sudah terdaftar pada `NESTED_MENU_KEYS` di `left-sidebar-menu-handle.jsx:3-34`, dan
`getChildrenFromItem` pada `left-sidebar-menu-handle.jsx:41-50` membaca `subMenu` di tingkat 0
lalu menyerahkan sisanya ke `getNestedMenuFromSubItem` pada baris `164-174`. Breadcrumb dan
penanda menu aktif ikut bekerja sendiri dari susunan yang sama.

### 10.2 Entri menu yang ditambahkan

Dua tambahan pada satu berkas, `src/utils/menu-sidebar/menu-items.jsx`.

**Tambahan pertama — menu induk baru.** Disisipkan setelah blok `healthServicesPatientManagement`
dan sebelum blok `healthServicesDoctorQueue`, mengikuti urutan alami pekerjaan: pasien terdaftar,
berkasnya ditelusuri, lalu pelayanannya berjalan.

```jsx
{
  label: "Rekam Medis",
  key: "healthServicesMedicalRecordManagement",
  icon: <RiFolderUserLine className="fs-4" />,
  subMenu: [
    {
      label: "Berkas Rekam Medis",
      key: "healthServicesMedicalRecords",
      icon: <RiFileSearchLine className="fs-4" />,
      pathname: "/health-services/medical-record-management/medical-records",
    },
    {
      label: "Catatan Belum Ditandatangani",
      key: "healthServicesMyUnsignedNotes",
      icon: <RiQuillPenLine className="fs-4" />,
      pathname: "/health-services/medical-record-management/my-unsigned-notes",
    },
    {
      label: "Tinjauan Akses",
      key: "healthServicesMedicalRecordAccessReview",
      icon: <RiShieldKeyholeLine className="fs-4" />,
      pathname: "/health-services/medical-record-management/access-review",
    },
  ],
},
```

**Tambahan kedua — satu entri pada menu Master Data yang sudah ada.** Disisipkan ke dalam
`subItems` milik `healthServicesMasterData`, **bukan** ke dalam menu `Rekam Medis`. Alasannya
pada `RM-FE-016`: kontrak menempatkannya di bawah `api/v1/health-services/master-data/`
(api-contract bagian 7), dan 24 layar master lain sudah berkumpul di sana. Memisahkannya berarti
petugas harus mencari master di dua tempat.

```jsx
{
  label: "Keperluan Akses Rekam Medis",
  key: "healthServicesMedicalRecordAccessPurpose",
  icon: <RiShieldKeyholeLine className="fs-4" />,
  pathname: "/health-services/master-data/medical-record-access-purposes",
},
```

Empat ikon yang dipakai — `RiFolderUserLine`, `RiFileSearchLine`, `RiQuillPenLine`,
`RiShieldKeyholeLine` — sudah tersedia pada `react-icons/ri` yang terpasang, dan perlu
ditambahkan ke blok `import` di baris 1-13. **Pilihan ikonnya sendiri `DEV_DISCRETION`**; yang
mengikat hanya label, kunci, dan rutenya.

### 10.3 Daftar lengkap entri menu

| Menu | Kunci | Rute | Berkas halaman | Task | Izin yang menentukan | Peran yang diusulkan melihatnya |
|---|---|---|---|---|---|---|
| Rekam Medis *(induk)* | `healthServicesMedicalRecordManagement` | — | — | `FE-07` | — | Tampil bila salah satu anaknya tampil |
| Berkas Rekam Medis | `healthServicesMedicalRecords` | `/health-services/medical-record-management/medical-records` | `src/app/health-services/medical-record-management/medical-records/page.jsx` | `FE-01`, `FE-02`, `FE-04` | `MedicalRecord : Read` | Dokter, perawat, kepala unit, petugas rekam medis, koder |
| Catatan Belum Ditandatangani | `healthServicesMyUnsignedNotes` | `/health-services/medical-record-management/my-unsigned-notes` | `.../my-unsigned-notes/page.jsx` | `FE-03` | `ClinicalDocumentIntegrity : Read` dan `: Update` | Dokter, perawat, kepala unit |
| Tinjauan Akses | `healthServicesMedicalRecordAccessReview` | `/health-services/medical-record-management/access-review` | `.../access-review/page.jsx` | `FE-05` | `MedicalRecordAccessLog : Read` | Petugas rekam medis, auditor internal |
| Keperluan Akses Rekam Medis | `healthServicesMedicalRecordAccessPurpose` | `/health-services/master-data/medical-record-access-purposes` | `src/app/health-services/master-data/medical-record-access-purposes/page.jsx` | `FE-06` | `MedicalRecordAccessPurpose : Read` | Petugas rekam medis |

Kolom peran mengikuti saran permission-audit-matrix bagian 6. Ia **saran**, bukan ketetapan —
pemetaan peran ke izin dilakukan manusia lewat layar pengaturan hak akses.

Satu koreksi terhadap acceptance criteria `FE-07` nomor 3, yang berbunyi "rute baru terdaftar
pada `tests/e2e/route-smoke.spec.mjs`": **tidak ada langkah pendaftaran.** Berkas uji itu
menelusuri sendiri `src/app` mencari setiap `page.jsx` dan menyusun daftar rutenya
(`route-smoke.spec.mjs:11-54`). Membuat berkas halamannya sudah cukup; yang perlu dilakukan
hanyalah memastikan uji itu dijalankan ulang.

### 10.4 Rute yang sengaja tidak diberi entri menu

| Rute | Cara mencapainya | Alasan |
|---|---|---|
| Berkas satu pasien | Dari hasil pencarian pada layar `Berkas Rekam Medis` | `RM-FE-014`. Menu yang langsung membuka daftar seluruh pasien akan menjadi daftar pasien rumah sakit yang dapat diramban bebas |
| Detail satu dokumen | Dari panel detail di dalam layar berkas | `RM-FE-015`. Dokumen hanya bermakna dalam konteks berkasnya |
| Kotak isian keperluan akses | Muncul sendiri saat dibutuhkan | Ia penghalang, bukan tujuan |
| Penetapan penulis berhalangan | **Tidak ada layarnya pada rilis pertama** | Ditunda 27 Agustus 2026; tercatat pada bagian 7. Rinciannya pada 10.6 |

### 10.5 Yang menahan `RM-FE-013`: penyaringan menu per izin belum ada

Acceptance criteria `FE-07` nomor 2 berbunyi "menu hanya muncul bagi peran yang berhak".
**Mekanisme itu tidak ada di frontend saat ini.** Buktinya berlapis:

| Bukti | Isinya |
|---|---|
| `left-sidebar-items-virtualized.jsx:237` | Peran dibaca dari cookie `role` — satu string, bukan daftar izin |
| `left-sidebar-items-virtualized.jsx:211` | Seluruh penyaringan menu hanya memanggil `filterMenuItemsByRole(userRole, menuItems)` |
| `filter-menu-items-by-role.jsx:5-7` | `Admin` dan `Manajer` menerima seluruh menu tanpa penyaringan |
| `filter-menu-items-by-role.jsx:13-15` | Peran lain disaring lewat kunci `ManajemenKesehatan`, yang **sudah tidak ada** di `menu-items.jsx` |
| `filter-menu-items-by-role.jsx:22-37` | Satu-satunya aturan penyaringan nyata masih dalam bentuk komentar |

Kesimpulannya: fungsi itu **tidak menyaring apa pun**. Peran apa pun melihat seluruh menu. Daftar
izin pengguna juga tidak pernah sampai ke sisi klien — tidak ada endpoint maupun state yang
membawanya.

Dampaknya tidak seragam pada empat entri di atas:

| Entri | Dampak bila tetap tampil bagi yang tidak berhak |
|---|---|
| Berkas Rekam Medis | Ringan. Halaman terbuka, permintaan dijawab `403`, isi tidak keluar |
| Catatan Belum Ditandatangani | Ringan. Daftarnya kosong |
| **Tinjauan Akses** | **Perlu perhatian.** Bukan kebocoran isi — `403` tetap menahannya — tetapi menu tinjauan jejak akses yang terlihat seluruh pengguna bertentangan dengan `RM-FE-013` |
| Keperluan Akses Rekam Medis | Ringan |

Yang terekspos bukan datanya, melainkan **kesan bahwa layar itu tersedia**. Backend tetap
menahan seluruh endpoint dengan `403`. Ini perlu dinyatakan tegas supaya tidak dibaca lebih
menakutkan daripada keadaannya, dan juga tidak dibaca lebih ringan.

Dua jalan keluar dipertimbangkan:

| Pilihan | Yang dikerjakan | Harganya | Keputusan |
|---|---|---|---|
| **A — tunda** | Empat menu tampil bagi semua peran; `403` yang menahan isinya | `FE-07` nomor 2 tidak dapat dibuktikan lulus. `RM-FE-013` tercatat sebagai utang terbuka | **Diambil** — pemilik frontend, 27 Agustus 2026 |
| B — perbaiki mekanismenya | Tambahkan field `permission` pada entri menu, sediakan daftar izin pengguna di sisi klien, saring pohon menu berdasarkan itu | Menyentuh berkas di luar modul rekam medis: `menu-items.jsx`, `filter-menu-items-by-role.jsx`, dan lapisan sesi. Perlu endpoint daftar izin pengguna dari backend, yang belum ada | Tidak diambil pada rilis pertama |

Pilihan B memperbaiki seluruh aplikasi, bukan hanya modul ini — 24 layar master dan seluruh menu
lain menanggung keadaan yang sama. Justru karena itu ia **tidak layak diselundupkan ke dalam
`FE-07`** sebagai pekerjaan sampingan; ia task tersendiri, dengan pemilik dan uji sendiri. Itu
pula alasan pilihan A diambil: bukan karena keadaan sekarang memadai, melainkan karena
perbaikannya bukan milik modul ini.

Yang mengikat setelah keputusan A:

| Butir | Keadaan |
|---|---|
| `FE-07` acceptance criteria nomor 2 | **Dinyatakan tidak dapat dibuktikan pada rilis pertama.** Bukan lulus, bukan gagal — tidak dapat diuji dengan mekanisme yang ada. Menyatakannya lulus berarti mencatat sesuatu yang tidak benar |
| `RM-FE-013` | Tetap `approved` sebagai aturan, **belum ditegakkan**. Tercatat sebagai utang terbuka, bukan butir yang selesai |
| Penahan sebenarnya | `403` dari backend pada seluruh endpoint. Ini yang menjaga isinya, dan ia sudah berjalan |
| Yang **tidak** boleh dilakukan | Menyembunyikan menu dengan pemeriksaan nama peran di sisi klien sebagai pengganti sementara. Itu menambah tempat kedua yang menyimpan aturan izin, dan akan berselisih dengan mekanisme sebenarnya saat kelak dibangun |

Baris terakhir perlu ditegaskan. Menambal dengan pencocokan nama peran terasa murah dan
menghasilkan layar yang terlihat benar, tetapi ia memindahkan aturan izin ke tempat yang tidak
punya kewenangan menyimpannya. Utang yang tercatat terbuka lebih murah daripada tambalan yang
harus dibongkar.

### 10.6 Satu layar yang belum punya tempat

Bagian 3 mencantumkan aksi **"Menetapkan penulis berhalangan"** sebagai kewenangan kepala unit,
dan kontrak menyediakan tiga endpoint untuknya (api-contract bagian 5). Tetapi bagian 2 tidak
memuat layarnya, roadmap frontend tidak memuat task-nya, dan bagian 7 tidak menyatakannya
ditunda. Ia jatuh di antara ketiganya.

Tanpa layar, penetapan bertipe `UnitHeadGrant` hanya dapat dibuat lewat pemanggilan API
langsung. Penetapan bertipe `InactiveAccount` tidak terpengaruh — sistem menyimpulkannya sendiri
dari keadaan akun dan memang tidak memerlukan layar apa pun.

Karena ketiga endpoint itu sendiri masih berstatus `Rencana (belum tersedia)`, celah ini **tidak
memblokir** rilis pertama.

**Keputusan pemilik frontend, 27 Agustus 2026: ditunda, dan penundaannya dinyatakan terbuka.**
Layar ini tidak masuk rilis pertama, tidak diberi entri menu, dan dicatat pada bagian 7 bersama
enam hal lain yang sengaja tidak dibuat. Celah pada revisi 1 dengan demikian tertutup — bukan
karena layarnya dibuat, melainkan karena keadaannya sekarang dinyatakan.

Akibat yang harus diketahui sampai layar itu ada:

| Hal | Keadaan pada rilis pertama |
|---|---|
| `UnitHeadGrant` | Hanya dapat dibuat lewat pemanggilan API langsung. Tidak ada jalur lewat antarmuka |
| `InactiveAccount` | Tidak terpengaruh. Sistem menyimpulkannya sendiri dari keadaan akun |
| Addendum sebagai pengganti | Tetap berjalan bagi delegasi yang sudah ada. Yang belum ada hanyalah cara membuat delegasi barunya |
| Aksi "Menetapkan penulis berhalangan" pada bagian 3 | Tetap tercantum sebagai kewenangan kepala unit, tetapi **belum punya layar**. Ini disebut terbuka supaya tabel itu tidak terbaca sebagai janji |

---

## 11. Skema tampilan per menu

Ditambahkan pada revisi 2. Menetapkan `RM-FE-014`, `RM-FE-015`, dan mempersempit `RM-FE-005`.

Skema di bawah menetapkan **wilayah layar dan isinya**, bukan gambarnya. Kotak-kotaknya denah,
bukan mockup: yang mengikat adalah wilayah mana memuat apa, dan aturan yang menyertainya. Warna,
jarak, tipografi, komponen tabel, dan bentuk penanda tetap `DEV_DISCRETION`.

### 11.1 Berkas Rekam Medis

Satu rute, dua keadaan berurutan: mencari pasien, lalu membuka berkasnya.

#### Keadaan A — pencarian pasien

```text
┌ Rekam Medis › Berkas Rekam Medis ───────────────────────────────────────────┐
│                                                                              │
│  ┌ Cari pasien ────────────────────────────────────────────────────────────┐ │
│  │  [ No. RM / NIK / Nama pasien                          ]   [ Cari ]     │ │
│  └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                              │
│  Hasil pencarian                                                             │
│  ┌──────────┬─────────────────┬────────────┬───────────────┬──────────────┐  │
│  │ No. RM   │ Nama pasien     │ Tgl lahir  │ Kunjungan     │              │  │
│  ├──────────┼─────────────────┼────────────┼───────────────┼──────────────┤  │
│  │ 00012345 │ ...             │ 12-03-1980 │ Aktif         │ [Buka berkas]│  │
│  │ 00012781 │ ...             │ 04-11-1962 │ Tidak ada     │ [Buka berkas]│  │
│  └──────────┴─────────────────┴────────────┴───────────────┴──────────────┘  │
│                                                                              │
│  ⓘ Membuka berkas rekam medis dicatat atas nama Anda. Bila pasien tidak      │
│    sedang Anda rawat, Anda akan diminta menyatakan keperluan lebih dulu.     │
└──────────────────────────────────────────────────────────────────────────────┘
```

Aturan yang mengikat keadaan A:

| Aturan | Alasan |
|---|---|
| Layar ini **tidak** memanggil `/summary` maupun `/timeline` | Kedua endpoint itu menghasilkan jejak akses. Jejak hanya boleh tercatat saat berkas benar-benar dibuka, bukan saat pengguna mengetik di kotak pencarian |
| Hanya identitas pasien yang tampil — tidak ada diagnosis, alergi, maupun catatan | `RM-FE-014`. Hasil pencarian bukan pratinjau rekam medis |
| Kolom `Kunjungan` menandai ada tidaknya kunjungan aktif | Memberi tahu lebih dulu bahwa berkas ini akan meminta keperluan akses, sebelum tombolnya ditekan |
| Satu-satunya panggilan yang diizinkan sebelum berkas dibuka adalah `GET /medical-records/filters/metadata` | Kontrak menyatakan endpoint itu **tidak** menghasilkan jejak akses. Ia sumber daftar keperluan dan penanda master kosong |
| Pemberitahuan pencatatan tampil di layar ini, bukan hanya di kotak keperluan | Pengguna tahu sebelum menekan, bukan sesudah |

#### Keadaan B — berkas terbuka

```text
┌ Rekam Medis › Berkas Rekam Medis › 00012345 ─────────────────────────────────────┐
│ ┌ A. Kepala berkas ────────────────────────────────────────────────────────────┐ │
│ │ 00012345 · NAMA PASIEN · L · 12 Mar 1980 (46 th)        Akses: Rawatan       │ │
│ │ ⚠ Alergi aktif: Amoksisilin · Seafood                                        │ │
│ │ Diagnosis aktif: J18.9 Pneumonia · E11 DM tipe 2                             │ │
│ └──────────────────────────────────────────────────────────────────────────────┘ │
│ ┌ B. Pemberitahuan cakupan ────────────────────────────────────────────────────┐ │
│ │ ⓘ Aturan keutuhan dokumen baru berlaku pada CPPT.                            │ │
│ │ ⓘ Label kerahasiaan dokumen belum membatasi siapa yang dapat membukanya.     │ │
│ └──────────────────────────────────────────────────────────────────────────────┘ │
│ ┌ C. Penyaring ────────────────────────────────────────────────────────────────┐ │
│ │ [Rentang tanggal ▾] [Jenis dokumen ▾] [Kunjungan ▾]  ☐ Termasuk dibatalkan   │ │
│ └──────────────────────────────────────────────────────────────────────────────┘ │
│ ┌ D. Peringatan kelengkapan — hanya bila isComplete = false ───────────────────┐ │
│ │ ⚠ RIWAYAT INI TIDAK LENGKAP. 2 sumber gagal dimuat: Hasil Lab, Radiologi.    │ │
│ │   Jangan mengambil keputusan klinis dari daftar ini.        [ Coba lagi ]    │ │
│ └──────────────────────────────────────────────────────────────────────────────┘ │
│ ┌ E. Riwayat ─────────────────────────┬ F. Detail dokumen ─────────────────────┐ │
│ │ ── 12 Agu 2026 ─────────────────    │ CPPT · 12 Agu 2026 10:14               │ │
│ │ ▸ 10:14  CPPT · dr. A               │ Penulis   : dr. A                      │ │
│ │          Keutuhan: Ditandatangani   │ Keutuhan  : Ditandatangani (terkunci)  │ │
│ │          Alur    : Selesai          │ Alur kerja: Selesai                    │ │
│ │          ↳ 2 addendum               │ ────────────────────────────────────── │ │
│ │ ▸ 09:40  Asesmen Awal · Ns. B       │ S : ...                                │ │
│ │          Keutuhan: belum tunduk     │ O : ...                                │ │
│ │          Alur    : Selesai          │ A : ...                                │ │
│ │ ── 03 Jul 2026 ─────────────────    │ P : ...                                │ │
│ │ ▸ 14:02  CPPT · dr. C               │ ─── Addendum (menempel pada dokumen) ─ │ │
│ │          Keutuhan: TidakDitandatangani│ #1 13 Agu · dr. A — koreksi dosis    │ │
│ │          Alur    : Selesai          │ #2 14 Agu · dr. D (pengganti) — ...    │ │
│ │                                     │ ────────────────────────────────────── │ │
│ │ [ Muat 1 bulan sebelumnya ]         │ [+ Addendum]   [🔒 Catatan pribadi]    │ │
│ └─────────────────────────────────────┴────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────────────────────────┘
```

| Wilayah | Isi | Sumber data | Aturan yang mengikat |
|---|---|---|---|
| A. Kepala berkas | No. RM, nama, jenis kelamin, tanggal lahir, alergi aktif, diagnosis aktif, penanda jenis akses | `GET /{patientId}/summary` → `data`, `data.access` | Alergi aktif **wajib** terlihat tanpa perlu digulir. `data.access` menyatakan akses rawatan atau beralasan, dan apakah pembukaan ini akan ditelaah — dan itu **wajib** disampaikan sekarang, bukan saat ditanya unit rekam medis |
| B. Pemberitahuan cakupan | Dua kalimat tetap | — | `RM-FE-009` dan `RM-FE-006`. Boleh diringkas kalimatnya, **tidak** boleh dihilangkan atau disembunyikan di balik tooltip |
| C. Penyaring | Rentang tanggal, jenis dokumen, kunjungan, termasuk dibatalkan, urutan | Query `startDate`, `endDate`, `documentKinds`, `encounterId`, `includeCancelled`, `newestFirst` | Pilihan jenis dokumen diambil dari `/filters/metadata`, **tidak** ditulis tetap di kode |
| D. Peringatan kelengkapan | Muncul **hanya** bila `isComplete = false` | `data.failedSources`, `data.isTruncated`, `data.isComplete` | `RM-FE-012`. Wajib menyebut sumber mana yang gagal, dan wajib menyatakan larangan mengambil keputusan klinis dari daftar itu. **Tidak** boleh berbentuk ikon kecil atau tooltip |
| E. Riwayat | Baris dokumen urut waktu, dikelompokkan per tanggal | `data.page.items` | Dibaca dari **`data.page.items`**, bukan `data.items` — lihat delta kontrak `BE-14`. Tiap baris membawa dua penanda status terpisah dan jumlah addendum |
| F. Detail dokumen | Isi dokumen, addendum menempel di bawahnya, tombol aksi | `GET /{patientId}/documents/{documentKind}/{documentId}` | `RM-FE-002` dan `RM-FE-015`. Addendum digambar **di dalam** kartu dokumen induknya, bukan sebagai entri terpisah di wilayah E |

**Dua penanda status pada wilayah E harus terbaca sebagai dua hal berbeda**, bukan satu label
gabungan (`RM-FE-008`):

| Penanda | Nilai yang mungkin | Asal |
|---|---|---|
| Status keutuhan | `Draft`, `Signed`, `LockedUnsigned`, `Cancelled` | `ClinicalDocumentIntegrityStatus` |
| Status alur kerja | Status milik dokumen itu sendiri — misalnya CPPT sudah selesai atau masih berjalan | Model dokumen masing-masing |

`LockedUnsigned` adalah nilai yang paling mudah disalahbaca. Ia berarti "terkunci karena
kunjungan ditutup, penulisnya belum sempat menandatangani" — catatannya **tetap sah dibaca**,
tetapi ditandai kurang lengkap. Menggambarnya dengan bentuk yang sama seperti `Signed`
menghapus perbedaan yang justru menjadi alasan `RM-DEC-003` ada.

Dokumen selain CPPT belum tunduk aturan keutuhan sama sekali. Penandanya **tidak** boleh
dikosongkan begitu saja — kolom kosong terbaca sebagai "belum ditandatangani". Ia harus
menyatakan bahwa jenis dokumen itu memang belum tercakup.

Aksi pada wilayah F:

| Aksi | Digambar hanya bila | Endpoint |
|---|---|---|
| Tambah addendum | `GET /addendums/authority/{kind}/{id}` menyatakan berhak | `POST /addendums/by-document/{kind}/{id}` |
| Tambah addendum sebagai pengganti | Endpoint yang sama menyatakan berhak atas dasar delegasi | `POST /addendums/by-document/{kind}/{id}/as-substitute` |
| Buka catatan pribadi | Dokumen `ProgressNote`, `hasPrivateNote = true`, dan pengguna memegang `MedicalRecord : ReadPrivateNote` | `GET /{patientId}/documents/{kind}/{id}/private-note` |
| Tandatangani | Dokumen milik pengguna dan berstatus `Draft` | `POST /clinical-document-integrities/by-document/{kind}/{id}/sign` |

Tiga hal yang paling mudah keliru di wilayah ini:

**Tombol addendum wajib terkunci setelah ditekan.** Addendum tidak dapat diubah maupun dihapus.
Tekan ganda menghasilkan dua koreksi kembar yang menempel selamanya pada rekam medis, dan tidak
ada cara membersihkannya.

**Membuka catatan pribadi selalu melewati kotak keperluan akses**, bahkan untuk pasien yang
sedang dirawat pengguna. Ini berbeda dari isi rekam medis lain, dan bukan kelalaian — `RM-DEC-022`
menetapkannya begitu.

**Isi catatan pribadi tidak boleh disimpan sementara di sisi klien** — tidak di state yang
bertahan, tidak di `localStorage`, tidak di cache permintaan. Ia dibaca, ditampilkan, lalu
hilang saat panel ditutup.

Keadaan gagal pada layar ini:

| Kode | Yang tampil |
|---|---|
| `409` | Satu pesan: nomor rekam medis ini sudah digabungkan, beserta nomor penggantinya dan tautan ke sana. **Tidak ada** wilayah A sampai F yang digambar — riwayat sebagian lebih berbahaya daripada tidak ada riwayat |
| `503` | "Berkas tidak dapat dibuka saat ini." Tombol coba lagi. Bukan pesan galat teknis |
| `403` | "Anda tidak punya hak membuka berkas rekam medis." |
| `404` | "Pasien tidak ditemukan." |

### 11.2 Kotak Isian Keperluan Akses

Bukan menu, dan bukan rute. Ia penghalang yang muncul di atas layar 11.1, dan di atas setiap
pembukaan catatan pribadi.

```text
┌ Keperluan membuka rekam medis ─────────────────────────────┐
│                                                             │
│  Pasien : 00012345 · NAMA PASIEN                            │
│  Pasien ini tidak sedang dalam rawatan Anda.                │
│                                                             │
│  Keperluan akses *   [ pilih keperluan            ▾ ]       │
│                                                             │
│  Alasan *            [ ................................ ]   │
│                      (muncul hanya bila keperluan yang      │
│                       dipilih menuntut alasan bebas)        │
│                                                             │
│  ⓘ Pembukaan ini dicatat atas nama Anda dan dapat ditinjau  │
│    unit rekam medis.                                        │
│                                                             │
│                        [ Batal ]   [ Buka berkas ]          │
└─────────────────────────────────────────────────────────────┘
```

| Wilayah | Isi | Sumber | Aturan |
|---|---|---|---|
| Kepala | Nomor RM dan nama pasien | Hasil pencarian, sudah ada di sisi klien | **Hanya identitas.** Tidak ada satu pun isi klinis di kotak ini |
| Sebab | Kalimat mengapa keperluan diminta | Penanda kunjungan aktif dari hasil pencarian | Membedakan "pasien di luar rawatan Anda" dari "pembukaan catatan pribadi selalu meminta keperluan" |
| Pilihan keperluan | Daftar dari master | `GET /medical-record-access-purposes/options` | Wajib dari master; **tidak** ditulis tetap di kode |
| Alasan bebas | Kotak teks, maksimum 500 karakter | — | Muncul hanya bila keperluan terpilih bertanda `IsFreeTextRequired` |
| Pemberitahuan | Bahwa pembukaan ini dicatat dan dapat ditinjau | — | Wajib. Menyembunyikannya membuat pencatatan terasa seperti jebakan |

Aturan yang paling mengikat, dan paling mudah dilanggar tanpa disadari:

**Tidak boleh ada satu pun permintaan isi rekam medis dikirim sebelum kotak ini dijawab.** Bukan
"isi dimuat lalu ditutupi lapisan buram" — itu berarti isinya sudah berpindah ke perangkat
pengguna dan terbaca lewat alat pengembang peramban. `RM-FE-003` mengikat pada **lalu lintas
jaringan**, bukan pada tampilan. Pembuktiannya juga di sana: pemeriksaan tab jaringan, bukan
pemeriksaan layar.

Keadaan khusus — master keperluan masih kosong:

```text
┌ Keperluan membuka rekam medis ─────────────────────────────┐
│  Pasien : 00012781 · NAMA PASIEN                            │
│                                                             │
│  ⚠ Daftar keperluan akses belum diisi unit rekam medis.     │
│    Selama daftar itu kosong, berkas pasien di luar rawatan  │
│    Anda tidak dapat dibuka. Hubungi unit rekam medis.       │
│                                                             │
│                        [ Tutup ]   [ Buka berkas ]·nonaktif │
└─────────────────────────────────────────────────────────────┘
```

`GET /medical-records/filters/metadata` mengembalikan `isAccessPurposeMasterEmpty`. Bila benar,
kotak ini **wajib** menyatakan keadaannya. Menampilkan daftar pilihan kosong tanpa penjelasan
akan dibaca sebagai sistem rusak — dan pengguna yang mengira sistemnya rusak akan mencari jalan
lain untuk sampai ke isi rekam medis.

Keadaan ini bukan kemungkinan yang jauh: master itu **memang masih kosong** sampai SOP rekam
medis rumah sakit tersedia. Pada hari `FE-02` selesai, inilah tampilan yang akan terlihat.

### 11.3 Catatan Belum Ditandatangani

```text
┌ Rekam Medis › Catatan Belum Ditandatangani ─────────────────────────────────┐
│                                                                              │
│  ⓘ 7 catatan menunggu tanda tangan Anda. Catatan yang belum ditandatangani   │
│    saat kunjungan ditutup akan terkunci bertanda "TidakDitandatangani".      │
│                                                                              │
│  ┌────────────────┬──────────┬────────────┬─────────────┬────────┬────────┐  │
│  │ Waktu catatan  │ No. RM   │ Pasien     │ Kunjungan   │ Jenis  │        │  │
│  ├────────────────┼──────────┼────────────┼─────────────┼────────┼────────┤  │
│  │ 12 Agu 10:14   │ 00012345 │ ...        │ Rajal #771  │ CPPT   │[Lihat] │  │
│  │                │          │            │             │        │[Tanda  │  │
│  │                │          │            │             │        │ tangani]│ │
│  └────────────────┴──────────┴────────────┴─────────────┴────────┴────────┘  │
│                                                                              │
│  Kosong: "Tidak ada catatan yang menunggu tanda tangan Anda."                │
└──────────────────────────────────────────────────────────────────────────────┘
```

| Wilayah | Isi | Sumber | Aturan |
|---|---|---|---|
| Pemberitahuan atas | Jumlah catatan dan akibat bila dibiarkan | Jumlah baris dari `GET /clinical-document-integrities/my-unsigned` | Menyebut akibatnya, bukan hanya jumlahnya. Inilah yang membuat layar ini dipakai |
| Daftar | Waktu catatan, no. RM, nama pasien, kunjungan, jenis dokumen | Endpoint yang sama | Daftar **hanya** memuat catatan milik pengguna. Bukan penyaring di sisi klien — backend yang membatasinya |
| Aksi per baris | Lihat isi, tandatangani | `POST /clinical-document-integrities/by-document/{kind}/{id}/sign` | Tombol **terkunci setelah ditekan**. Setelah berhasil, baris hilang dari daftar |
| Keadaan kosong | Kalimat yang menyatakan tidak ada yang menunggu | — | Dibedakan dari "penyaringan tidak menemukan apa pun" dan dari galat pemuatan |

Menandatangani **tidak** meminta kata sandi ulang maupun sidik jari. Ini keputusan yang sudah
diambil, bukan kelalaian: menandatangani adalah pernyataan tanggung jawab, dan sesi yang sudah
sah dianggap cukup membuktikan siapa yang menyatakannya.

Layar inilah yang membuat `RM-DEC-003` bekerja sebagaimana dimaksud. Tanpa ia, catatan yang lupa
ditandatangani tidak dapat ditemukan kembali, dan seluruhnya akan berakhir `LockedUnsigned` saat
kunjungan ditutup — hasil yang berlawanan dengan maksud keputusan itu.

### 11.4 Tinjauan Akses

```text
┌ Rekam Medis › Tinjauan Akses ───────────────────────────────────────────────┐
│  🔒 Layar ini memuat alasan akses yang bersifat sensitif.                    │
│                                                                              │
│  [ Perlu ditinjau ] [ Seluruh jejak ]                                        │
│  [Rentang tanggal ▾] [Jenis akses ▾] [Cakupan ▾] [Pengakses ▾] [Pasien ▾]    │
│                                                                              │
│  ┌──────────────┬───────────┬──────────┬──────────────┬───────────┬───────┐  │
│  │ Waktu        │ Pengakses │ No. RM   │ Jenis akses  │ Keperluan │       │  │
│  ├──────────────┼───────────┼──────────┼──────────────┼───────────┼───────┤  │
│  │ 12 Agu 10:14 │ dr. A     │ 00012345 │ Beralasan    │ Rujukan   │[Tinjau]│ │
│  │              │           │          │ · CatatanPribadi│         │       │ │
│  └──────────────┴───────────┴──────────┴──────────────┴───────────┴───────┘  │
│                                                                              │
│  ┌ Panel tinjauan ────────────────────────────────────────────────────────┐  │
│  │ Alasan yang dituliskan : "..."                                         │  │
│  │ Kunjungan aktif saat itu: tidak                                        │  │
│  │ Catatan tinjauan *      [ ............................................ ]│  │
│  │                                          [ Tandai sudah ditinjau ]     │  │
│  └────────────────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────────────────┘
```

| Wilayah | Isi | Sumber | Aturan |
|---|---|---|---|
| Peringatan sensitivitas | Satu kalimat | — | Wajib. Layar ini memuat `AccessReason`, yang dapat mengungkap keadaan pasien |
| Dua tab | Antrean perlu ditinjau; seluruh jejak | `GET /medical-record-access-logs/pending-review`; `GET /medical-record-access-logs/` | Tab antrean **hanya** memuat baris bertanda perlu ditinjau |
| Penyaring | Tanggal, jenis akses, cakupan, pengakses, pasien | Query `accessType`, `startDate`, `endDate`, `userId`, `patientId`, `isFlaggedForReview` | — |
| Daftar | Waktu, nama pengakses, no. RM, jenis akses, cakupan, nama keperluan | Endpoint yang sama | **`AccessReason` tidak ditampilkan di baris daftar** |
| Panel tinjauan | Alasan lengkap, keadaan kunjungan saat itu, kotak catatan tinjauan | `PATCH /{id}/mark-reviewed` | Menandai sudah ditinjau **wajib** disertai catatan tinjauan |

Satu keputusan tampilan yang perlu dijelaskan: **alasan akses hanya terbaca di panel tinjauan,
tidak di baris daftar.** Acceptance criteria `FE-05` nomor 2 menuntut alasan terlihat, dan ia
terpenuhi — tetapi terlihat saat satu baris dibuka, bukan terhampar puluhan sekaligus di layar.
Alasan akses dapat mengungkap keadaan pasien; menghamparkannya dalam satu tabel mengubah layar
pengawasan menjadi ringkasan kondisi pasien yang dapat dibaca sekilas dari belakang punggung.

Nama pengakses diambil dari salinan yang tersimpan pada baris jejak, bukan dari akun pengguna
saat ini. Jejak akses harus tetap terbaca puluhan tahun kemudian, sementara akun bisa berganti
nama atau dihapus.

Baris jejak **tidak dapat diubah maupun dihapus** dari layar ini. Satu-satunya perubahan yang
mungkin adalah menandainya sudah ditinjau — yang menambah keterangan, bukan mengubah isi jejak.
Karena itu tidak ada tombol ubah maupun hapus di mana pun pada layar ini.

### 11.5 Keperluan Akses Rekam Medis (master)

Mengikuti pola layar master data yang sudah ada di
`src/components/view/health-services/master-data/`. **Tidak ada pola baru** — daftar, tambah,
detail, dan pengaktifan, persis seperti 24 layar master lainnya.

Kolom yang ditampilkan dan dapat disunting:

| Kolom | Bentuk isian | Aturan |
|---|---|---|
| `PurposeCode` | Teks, maks 50, unik | Kode keperluan, misalnya `CROSS_UNIT` |
| `PurposeName` | Teks, maks 150 | Nama yang dibaca pengguna pada kotak keperluan akses |
| `IsFreeTextRequired` | Saklar | Bila menyala, pengguna wajib menuliskan alasan sendiri. **Inilah yang memunculkan kotak alasan bebas pada 11.2** |
| `RequiresReview` | Saklar, bawaan menyala | Bila menyala, akses dengan keperluan ini masuk antrean tinjauan pada 11.4 |
| `SortOrder` | Angka | Urutan tampil pada kotak keperluan |
| `Description` | Teks, maks 250 | Keterangan tambahan |
| `IsActive` | Saklar | Mengikuti konvensi project |

Dua saklar di tengah tabel itu menentukan perilaku dua layar lain. Perlu dinyatakan di layar ini
— sebaris keterangan di dekat saklarnya — supaya petugas rekam medis tahu bahwa ia sedang
mengatur alur kerja, bukan sekadar mengisi daftar.

Selama master ini kosong, **pembukaan berkas pasien di luar rawatan selalu ditolak**. Layar ini
karenanya bukan pelengkap; ia pintu yang harus dibuka lebih dulu agar 11.1 dan 11.2 berguna.

### 11.6 Yang berlaku pada seluruh layar

| Aturan | Bunyinya di layar |
|---|---|
| Sedang memuat | Penanda per wilayah. Wilayah A boleh sudah tampil sementara wilayah E masih memuat |
| Kosong | "Pasien belum punya riwayat" dibedakan dari "penyaringan tidak menemukan apa pun" |
| Gagal sebagian | Ditandai jelas di wilayah D, tidak disembunyikan |
| Gagal seluruhnya | Tombol coba lagi |
| Kiriman ganda | Tombol tandatangan dan addendum terkunci setelah ditekan |
| Catatan pribadi | Tidak pernah disimpan sementara di sisi klien |
| Tombol tanpa hak | Tidak digambar sama sekali, bukan digambar lalu gagal saat ditekan |

Bagian ini sengaja mengulang bagian 4. Bagian 4 menyatakan aturannya; bagian ini menyatakan di
wilayah mana aturan itu terlihat. Aturan yang tidak punya tempat di layar cenderung tidak
dikerjakan.

---

## 12. Catatan revisi

| Revisi | Tanggal | Yang berubah |
|---|---|---|
| 1 | 24 Agustus 2026 | Susunan awal. Kontrak fungsional saja; bentuk tampilan diserahkan ke developer karena belum ada brief UI yang disetujui |
| 2 | 27 Agustus 2026 | Owner dan status disesuaikan dengan `RM-DEC-027` dan `RM-DEC-028`. Bagian 10 (navigasi dan entri menu) dan bagian 11 (skema tampilan per menu) ditambahkan. `RM-FE-004` naik dari `DEV_DISCRETION` menjadi `approved`; `RM-FE-005` menyusut menjadi terbatas; `RM-FE-013` sampai `RM-FE-016` ditambahkan |

Tiga keputusan pemilik frontend diambil bersama revisi ini:

| Keputusan | Isinya | Tercatat di |
|---|---|---|
| Penyaringan menu per izin | **Ditunda.** Mekanismenya belum ada di frontend, dan perbaikannya menyentuh seluruh aplikasi sehingga menjadi task tersendiri. `RM-FE-013` tetap berlaku sebagai aturan tetapi belum ditegakkan; `FE-07` nomor 2 dinyatakan tidak dapat dibuktikan pada rilis pertama | Bagian 10.5, matriks `RM-FE-013` |
| Layar penetapan penulis berhalangan | **Ditunda, penundaannya dinyatakan terbuka.** Tidak diberi entri menu; `UnitHeadGrant` sementara hanya lewat API langsung | Bagian 7, bagian 10.4, bagian 10.6 |
| Sinkronisasi artefak turunan | **Dikerjakan 27 Agustus 2026.** Hash pada manifest, `input_revisions` dan hash pada roadmap frontend, daftar `DEV_DISCRETION` bagian 9 roadmap, serta acceptance criteria `FE-07` nomor 2 dan 3 disesuaikan | `blueprint-manifest.md`, `roadmap/frontend-roadmap.md` |

Bagian 1 sampai 9 **tidak** dinomori ulang, dan bagian baru diletakkan di belakang justru karena
itu. Roadmap frontend merujuk "arsitektur frontend bagian 4" dan "bagian 5" secara langsung;
menomori ulang akan memutus rujukan itu tanpa ada yang menyadarinya.

Satu koreksi ikut dibawa revisi ini: acceptance criteria `FE-07` nomor 3 berbunyi rute baru
perlu didaftarkan pada `tests/e2e/route-smoke.spec.mjs`. **Tidak ada langkah pendaftaran** —
berkas uji itu menelusuri `src/app` sendiri. Kriteria itu disesuaikan pada roadmap.
