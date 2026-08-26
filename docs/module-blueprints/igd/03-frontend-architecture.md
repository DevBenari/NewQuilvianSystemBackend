# Arsitektur Frontend — Modul IGD

| Field | Nilai |
| --- | --- |
| Blueprint | `IGD-BP-001` revision `5` |
| Status | `draft` |
| Commit diaudit | frontend `96a9120111f6acc6b7c0f37973ea0c717ba41f17` |
| Kontrak yang diikuti | API `0.3.0`, state `0.3.0`, validation `0.3.0`, permission/audit `0.3.0` |

---

## 0. Hierarki kewenangan yang dipakai dokumen ini

```text
keamanan / privasi / invariant
  → brief produk atau UI yang disetujui
    → konvensi dan design system proyek
      → DEV_DISCRETION
```

Dokumen ini **tidak** menetapkan sidebar, urutan menu, route final, bentuk tab/modal/drawer,
warna, layout, maupun pustaka komponen. Hal-hal itu ditandai `DEV_DISCRETION` dan diputuskan
pelaksana mengikuti konvensi yang sudah ada.

Yang **ditetapkan** dokumen ini hanyalah hal yang berasal dari dua lapis teratas: apa yang
wajib terlihat, apa yang wajib ditolak, apa yang tidak boleh ditampilkan, dan apa yang tidak
boleh menahan pekerjaan perawat.

---

## 1. Konvensi yang wajib dipakai ulang

Layar IGD **tidak** boleh membuat komponen atau gaya tandingan. Yang sudah ada dan wajib
dipakai:

| Kebutuhan | Yang sudah ada | Lokasi |
| --- | --- | --- |
| Isian formulir | `BaseTextField`, `BaseSelectField`, `BaseTextareaField`, `BaseSimpleCheckbox` | `src/components/ui/form-pemeriksaan-ui` |
| Tabel dan penyaring | `DataTable`, `DataFilter` | `base-features` |
| Gaya layar IGD | Token desain, hero gradien, blok LIST TABLE, pagination | `src/style/health-services/emergency-installation-management/emergency-triage/emergency-triage.module.css` |
| SOAP, tanda vital, pengkajian nyeri | Bentuk yang sudah dipakai antrean dokter | folder `doctor-queue` |
| Pembungkus balasan API | `unwrap`, `unwrapPaged`, `normalizeError` | `emergency-assessment-slice.jsx` |

Alasannya bukan estetika. Perawat yang sama memakai beberapa layar dalam satu shift; layar
yang tampil berbeda terbaca sebagai dua aplikasi, dan urutan isian yang berbeda memperlambat
pekerjaan yang sudah dihafal.

---

## 2. Layar yang terdampak

| Layar | Status | Route sekarang |
| --- | --- | --- |
| Pendaftaran IGD | **Diperbarui** | `/health-services/registration-management/emergency-registration` |
| Triase | **Diperbarui** | `/health-services/emergency-installation-management/emergency-triage` |
| Pengkajian IGD | **Diperbarui** | `/health-services/emergency-installation-management/emergency-assessment` |
| Daftar pantau pengkajian ulang | **Baru** | `DEV_DISCRETION` |

Route untuk layar baru **tidak ditetapkan** di sini. Yang ditetapkan hanyalah bahwa ia harus
dapat dicapai dari layar pengkajian tanpa perawat kehilangan konteks pasien.

---

## 3. Pendaftaran IGD

### 3.1 Perubahan wajib

| Perubahan | Sebab | Sifat |
| --- | --- | --- |
| Payload encounter mengirim `EncounterType.Emergency` | `IGD-DEC-074` | **Memutus** — test `FE-IGD-001 K1` wajib diperbarui dalam task yang sama |
| Tidak lagi mengirim kelas pasien | `IGD-DEC-076` — backend menetapkannya sendiri | Menghapus field dari payload |
| Menangani penolakan `409` kunjungan ganda | `IGD-DEC-084` | Baru |

### 3.2 Penolakan kunjungan ganda

Ketika backend menolak `409` karena pasien masih punya kunjungan IGD aktif, layar **wajib**:

1. menampilkan nomor kunjungan yang sudah ada beserta waktu kedatangannya;
2. menyediakan cara membuka kunjungan itu tanpa mengetik ulang apa pun;
3. **tidak** menghapus isian yang sudah diketik petugas.

Bentuk tampilannya — dialog, panel, atau baris peringatan — `DEV_DISCRETION`.

Jalan keluar beralasan disediakan, tetapi **tidak** ditampilkan sebagai tombol setara. Ia
harus menuntut tindakan sadar dan alasan tertulis, karena memakainya menghasilkan dua episode
klinis untuk satu orang.

### 3.3 Kegagalan sebagian yang sudah ada

Layar sudah menangani keadaan "encounter berhasil, kunjungan IGD gagal" dengan menahan hasil
langkah pertama. Perilaku itu **dipertahankan** dan tidak boleh dihilangkan saat menambahkan
perubahan di atas.

---

## 4. Triase

| Perubahan | Sebab |
| --- | --- |
| Penetapan dokter memakai grup `Emergency Doctor Assignment`, bukan `PATCH /patient-encounters/{id}/doctor` | `IGD-DEC-082` |
| Menampilkan riwayat penugasan dokter, bukan hanya dokter sekarang | `IGD-DEC-082` |
| Pengalihan dokter menuntut alasan | `IGD-DEC-082` |
| Menangani `409` saat kunjungan sudah tertutup | `IGD-GAP-014` |

Riwayat dokter ditampilkan sebagai daftar berurutan waktu: dokter, sejak kapan, sampai kapan,
alasan pengalihan. Baris yang sedang aktif dibedakan. Bentuknya `DEV_DISCRETION`.

---

## 5. Pengkajian IGD

### 5.1 Tab yang berubah kemampuannya

| Tab | Sekarang | Setelah revisi ini | Bergantung pada |
| --- | --- | --- | --- |
| Assesmen Awal IGD | Hanya membaca | **Dapat menyimpan** | `IGD-DEC-068` — menunggu pemilik `ClinicalManagement` |
| Resep | Hanya membaca | **Dapat menyimpan** | `IGD-DEC-068` — menunggu pemilik `PharmacyManagement` |
| Tanda Vital | Dapat menyimpan | Ditambah riwayat versi | `IGD-DEC-080` |
| SOAP, Catatan Terintegrasi | Dapat menyimpan | Ditambah riwayat versi | `IGD-DEC-080` |
| Transfer Pasien | Dapat menyimpan | **Dirombak** menjadi kepergian dua rangkaian | `IGD-DEC-069`, `070` |
| Tindak Lanjut | Dapat menyimpan | Ditambah daftar sikap pesanan | `IGD-DEC-078` |
| Observasi, Nosokomial | Dapat menyimpan | Tidak berubah | — |

> Dua baris pertama **tidak dapat dikerjakan** sebelum pemilik modulnya ditunjuk. Layar boleh
> disiapkan, tetapi tombol simpannya tidak akan berfungsi. Menyembunyikan keterbatasan ini
> dari perawat dilarang — lihat bagian 8.

### 5.2 Riwayat versi catatan klinis

Catatan yang sudah dikoreksi wajib dapat ditelusuri. Yang **wajib** terlihat:

1. nilai yang berlaku sekarang, ditandai jelas sebagai yang berlaku;
2. nilai sebelumnya beserta pelaku, waktu, dan alasan koreksi;
3. urutan koreksi menurut waktu.

Nilai lama **tidak boleh** ditampilkan berdampingan dengan nilai berlaku tanpa pembeda yang
tegas — perawat harus dapat mengetahui mana fakta klinis yang berlaku dalam sekali lihat.

Bentuk penyajiannya `DEV_DISCRETION`.

### 5.3 Daftar sikap pesanan

Muncul sebelum dokumen serah terima diajukan. Setiap pesanan menampilkan nama, jenis, dan tiga
pilihan sikap: sudah dikerjakan, dibatalkan, diteruskan.

**Wajib** ditampilkan bersamanya: keterangan bahwa **pemeriksaan penunjang belum dapat
dihitung sistem** (`IGD-DEC-087`). Keterangan ini muncul di layar, bukan hanya di dokumen.
Tanpa itu perawat akan mengira daftarnya lengkap.

Pembatalan pesanan menuntut alasan. Tombol ajukan dokumen tetap tidak aktif selama masih ada
pesanan tanpa sikap — **tetapi** tombol berangkat dan tiba **tetap aktif**.

### 5.4 Kepergian pasien — dua rangkaian

Layar menampilkan **dua** rangkaian status berdampingan, bukan satu:

```text
Fisik    :  Disiapkan  →  Berangkat  →  Tiba
Dokumen  :  Diajukan   →  Tertunda   →  Diterima / Ditolak
```

| Aturan tampilan | Sebab |
| --- | --- |
| Kombinasi fisik `Tiba` + dokumen `Tertunda` ditampilkan **normal**, bukan sebagai galat | `IGD-DEC-070` |
| Pemilik klinis pasien saat ini **selalu** terlihat | `IGD-DEC-072`, `IGD-GAP-015` |
| Tombol tindakan fisik **tidak pernah** dinonaktifkan oleh keadaan dokumen | `IGD-DEC-070`, `078` |
| Tombol catat kedatangan hanya aktif bagi petugas berwenang atas unit tujuan | `IGD-DEC-086` |
| Penolakan `403` kewenangan unit menjelaskan sebabnya, bukan sekadar "tidak berhak" | Kegunaan |

Formulir SBAR memakai empat isian. Setiap isian punya penanda "tidak dapat diisi saat ini"
beserta kolom alasan. Tiga bagian otomatis — alergi, tanda vital terakhir, tingkat kegawatan —
ditampilkan sebagai isi yang tidak dapat diketik.

---

## 6. Daftar pantau pengkajian ulang

Layar baru. Meniru daftar pelampauan batas waktu triase yang sudah ada.

| Aturan | Sebab |
| --- | --- |
| Baris dengan interval belum dikonfigurasi **tetap ditampilkan**, ditandai "interval belum ditetapkan" | `IGD-DEC-083` |
| Baris itu **tidak** dihitung sebagai terlambat maupun patuh | `IGD-DEC-083` |
| Layar **tidak pernah** menonaktifkan tindakan klinis apa pun | `IGD-DEC-060`, `083` |
| Disaring menurut unit tempat pengguna bertugas | `IGD-DEC-086` |

---

## 7. Kontrak data, muat ulang, dan kegagalan

| Aspek | Aturan |
| --- | --- |
| Kesegaran data | Daftar pasien dan daftar pantau dimuat ulang saat layar dibuka dan saat penyaring berubah. Tidak ada polling otomatis pada revisi ini — `IGD-TRQ-07` |
| Pembatalan permintaan | Permintaan yang tertinggal saat pengguna berpindah pasien wajib dibatalkan agar data pasien lain tidak muncul |
| Kirim ganda | Tombol simpan dinonaktifkan selama permintaan berjalan. Untuk tindakan yang mengubah kepemilikan pasien — catat kedatangan, terima serah terima — kirim ganda **wajib** ditolak di backend juga |
| Data basi | Bila backend menolak `409` karena status sudah berubah pihak lain, layar memuat ulang data lalu menampilkan keadaan terbaru; isian pengguna tidak dibuang |
| Sedang memuat | Kerangka isi, bukan layar kosong |
| Kosong | Menyebutkan penyaring yang sedang aktif |
| Galat | Pesan dari backend ditampilkan apa adanya, ditambah tombol coba lagi |
| `403` | Menjelaskan apakah yang kurang adalah kemampuan atau penugasan unit |

---

## 8. Privasi dan hal yang tidak boleh disembunyikan

| Aturan | Sebab |
| --- | --- |
| Nama sementara pasien tanpa identitas ditampilkan apa adanya, tidak diganti tebakan | `IGD-DEC-007` |
| Isi klinis tidak masuk ke log peramban maupun `console` | `IGD-DEC-006` |
| Bagian yang belum tersambung ditandai apa adanya, **tidak** menampilkan data contoh | Sudah menjadi konvensi layar pengkajian |
| Keterbatasan penunjang dinyatakan di layar | `IGD-DEC-087` |
| Kolom yang backend-nya belum mengirim nama **tidak** ditampilkan sebagai tanda hubung tanpa penjelasan | Pelajaran `BE-IGD-016` |

Butir terakhir berasal dari cacat nyata: layar pernah menampilkan lima kolom yang tidak pernah
mungkin terisi, dan tidak ada yang menyadarinya selama berminggu-minggu.

---

## 9. Aksesibilitas dan perilaku layar

| Aspek | Aturan | Kewenangan |
| --- | --- | --- |
| Warna kategori triase | Diambil dari `ColorHex` master, bukan dipetakan di frontend. Warna teks dihitung dari kontras | Sudah dikunci, ada test-nya |
| Warna sebagai satu-satunya pembeda | Dilarang. Status wajib punya label teks | Invariant |
| Ukuran layar | Layar IGD dipakai di komputer meja dan tablet di sisi pasien | `DEV_DISCRETION` untuk titik hentinya |
| Urutan fokus papan ketik | Mengikuti urutan kerja perawat, bukan urutan kolom di kode | Konvensi |

---

## 10. Ketergantungan test

| Test | Keadaan | Tindakan |
| --- | --- | --- |
| `tests/unit/emergency-registration-payload.test.mjs` `FE-IGD-001 K1` | **Akan gagal** setelah `IGD-DEC-074` | Diperbarui dalam task yang sama; jangan dinonaktifkan |
| `tests/unit/emergency-visit-status.test.mjs` | Tetap berlaku | — |
| `tests/unit/emergency-triage-utils.test.mjs` | Tetap berlaku | — |
| Test baru untuk dua rangkaian status | Belum ada | Wajib dibuat bersama layar kepergian |
| Test baru untuk daftar sikap pesanan | Belum ada | Wajib dibuat |

---

## 11. Yang sengaja tidak ditetapkan

| Hal | Alasan |
| --- | --- |
| Route layar daftar pantau | `DEV_DISCRETION` |
| Bentuk tab, modal, atau drawer | `DEV_DISCRETION` |
| Urutan menu dan sidebar | `DEV_DISCRETION`; tidak ada brief yang sah |
| Palet warna baru | Dilarang — salin dari `emergency-triage.module.css` |
| Pustaka komponen baru | Dilarang — pakai `form-pemeriksaan-ui` dan `base-features` |
| Pembaruan realtime | `IGD-TRQ-07`, `LATER SLICE` |
