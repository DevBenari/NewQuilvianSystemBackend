# PLAN-REPAIR-022 — Perbaikan Menyeluruh Alur Admisi Bayi

```yaml
plan_id: PLAN-REPAIR-EPS-022
issue: ../issue/issue-022-perbaikan-alur-admisi-bayi.md
status_rencana: SUPERSEDED_RWI_BAYI_022_IMPLEMENTED_SOURCE
tanggal_rencana: "2026-10-15"
tanggal_selesai_source: "2026-10-16"
diputuskan_oleh: "Pemilik Sistem (User)"
tanggal_keputusan: "2026-10-15"
superseded_by: "RWI-BAYI-022-2.0.0 (2026-10-16)"
basis_source_backend: "ca09cae909597442385ce4513be9c803549be10e (MHamzah)"
basis_source_frontend: "4d9bf608dbbaddc96a2b4acfc8723753b5dd5028 (HamzahV2)"
```

---

> **Status final.** Kontrak `RWI-BAYI-022-2.0.0` menggantikan bagian rencana ini yang masih
> memakai step deposit dan perubahan pembatalan. Implementasi final menghapus step deposit bayi
> dan tidak mengubah perilaku pembatalan existing. Rincian final ada pada
> [`planning-perbaikan-admisi-bayi-baru-lahir.md`](../planning/planning-perbaikan-admisi-bayi-baru-lahir.md).

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti Implementasi |
| :--- | :--- | :--- | :--- | :---: | :---: | :---: | :--- |
| `FIX-EPS-022-01` | `ISS-EPS-022-01` | Jadikan pemilihan cabang bayi dan navigasi ke `newborn-context` sebagai satu transisi atomik | Frontend | 1 | `FIX-EPS-022-01` | SELESAI SOURCE | [Laporan](../../task/report/frontend/FIX-EPS-022-01.md) |
| `FIX-EPS-022-02` | `ISS-EPS-022-02` | Hapus ringkasan global; tampilkan ringkasan ringkas hanya pada Tinjau dan Aktifkan | Frontend/UI | 2 | `FIX-EPS-022-02` | SELESAI SOURCE | [Laporan](../../task/report/frontend/FIX-EPS-022-02.md) |
| `FIX-EPS-022-03` | `ISS-EPS-022-03`, `ISS-EPS-022-04` | Hilangkan input rujukan berulang dan perjelas/derivasi admisi ulang | Frontend + kontrak | 2 | `FIX-EPS-022-03` | SELESAI SOURCE | [Laporan](../../task/report/frontend/FIX-EPS-022-03.md) |
| `FIX-EPS-022-04` | `ISS-EPS-022-05` | Tata ulang layar identitas menjadi bagian Ibu Bayi dan Identitas Bayi yang responsif | Frontend/UI | 2 | `FIX-EPS-022-04` | SELESAI SOURCE | [Laporan](../../task/report/frontend/FIX-EPS-022-04.md) |
| `FIX-EPS-022-05` | `ISS-EPS-022-06` | Gunakan ulang komponen deposit admisi umum untuk bayi | Frontend | 2 | `FIX-EPS-022-05` | SUPERSEDED — step deposit dihapus | `RWI-BAYI-022-2.0.0` |
| `FIX-EPS-022-06` | `ISS-EPS-022-07` | Samakan predikat kelayakan unit antara daftar bed dan pembentukan encounter; tambahkan pemulihan UI | Backend + Frontend | 1 | `FIX-EPS-022-06` | SELESAI SOURCE | [Frontend](../../task/report/frontend/FIX-EPS-022-06.md) · [Backend](../../task/report/backend/FIX-EPS-022-06.md) |
| `FIX-EPS-022-07` | `ISS-EPS-022-08` | Tambahkan pembatalan sadar-lifecycle pada semua langkah bayi | Frontend + API episode | 1 | `FIX-EPS-022-07` | DIBATALKAN PEMILIK — tidak dikerjakan | Di luar scope final |

**Status final rencana lama:** lima perbaikan dipertahankan, satu (`FIX-EPS-022-05`) digantikan
oleh penghapusan step deposit, dan satu (`FIX-EPS-022-07`) ditarik pemilik. Implementasi kontrak
022 selesai pada source; pengujian live browser menunggu migration backend diterapkan.

---

## 2. Solusi Terpilih Per Temuan

### 2.1 Navigasi ke Cabang Bayi

| Opsi | Pendekatan | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| A | Menambahkan kondisi khusus agar fallback memilih indeks pertama daftar bayi | Perubahan kecil | Fallback tetap rapuh dan bisa rusak jika urutan slug berubah lagi |
| **B — terpilih** | Simpan pilihan sebagai state sementara; saat **Lanjut**, commit mode bayi dan navigasikan eksplisit ke `newborn-context` dalam satu aksi | Deterministik, mudah diuji, tidak bergantung slug bersama | Memerlukan pemisahan state pilihan dan state cabang aktif |

### 2.2 Ringkasan Bayi–Ibu

| Opsi | Pendekatan | Keputusan |
| :---: | :--- | :--- |
| A | Pertahankan kartu pada semua langkah tetapi diperkecil | Ditolak; tetap berulang dan memenuhi layar |
| **B — terpilih** | Hapus kartu global; tampilkan satu ringkasan pada Langkah 8 Tinjau dan Aktifkan | Sesuai instruksi pelapor dan menjaga fokus setiap langkah |

### 2.3 Konteks Admisi dan Admisi Ulang

| Opsi | Pendekatan | Keputusan |
| :---: | :--- | :--- |
| A | Pertahankan tiga kartu `HospitalBirth`, `External`, `Readmission` | Ditolak; mengulang kategori rujukan dan mencampur asal dengan lifecycle |
| **B — terpilih** | Jenis kunjungan ditampilkan baca-saja dan dipetakan ke `HospitalBirth/External`; admisi ulang hanya untuk bayi lama dengan episode sebelumnya selesai | Menghilangkan input ganda tanpa harus langsung memigrasi enum backend |

Adapter kompatibilitas menjaga payload lama: kunjungan rujukan menghasilkan `source = External`, kelahiran di RS menghasilkan `source = HospitalBirth`, dan bayi lama yang memenuhi aturan admisi ulang menghasilkan `source = Readmission`. Pemisahan skema database menjadi dua field tidak termasuk perbaikan ini kecuali audit implementasi menemukan kontrak enum tidak lagi memadai.

### 2.4 Identitas Bayi dan Ibu

Gunakan komponen form dan token yang ada. Tidak membuat base component baru. Penyusunan ulang difokuskan pada hierarki, pengurangan duplikasi, responsivitas, validasi dekat field, dan footer aksi yang konsisten.

### 2.5 Deposit

Gunakan `InpatientAdmissionDepositStep`. Tambahkan prop konfigurasi minimum (`nextLabel`, teks konteks opsional, dan handler batal) agar alur bayi menuju Penempatan, bukan Dokter. Aturan, format nominal, state loading/error, dan desain visual tetap satu sumber dengan admisi umum.

### 2.6 Bed dan Service Unit

Jangan melemahkan validasi endpoint encounter. Sebaliknya, keluarkan bed dari daftar jika unitnya tidak memenuhi seluruh syarat registrasi. Endpoint encounter tetap memvalidasi ulang untuk menangani perubahan master data setelah daftar dibuka. Frontend menangani penolakan stale secara ramah dan memuat ulang bed.

### 2.7 Pembatalan

Gunakan dua aksi yang sudah menjadi standar admisi umum:

- **Batal Pendaftaran** untuk state lokal sebelum episode terbentuk.
- **Batalkan Admisi** untuk episode `Draft` yang sudah tersimpan dan mungkin telah memesan bed.

Label tombol pada footer mengikuti alur umum (`Batal Pendaftaran`), sedangkan judul dialog boleh berbentuk pertanyaan `Batalkan pendaftaran ini?`.

---

## 3. Skema Tampilan Sebelum → Sesudah

### 3.1 Pemilihan Bayi dan Konteks Admisi

**SEBELUM**

```text
Langkah pasien umum: [Jenis Kunjungan] → [Jenis Pasien]
                                            │ pilih Bayi
                                            ▼
                          daftar langkah langsung diganti
                                            │ fallback slug bersama
                                            ▼
                              [Langkah 4: Penjamin]

Konteks bayi:
[ Episode kelahiran di RS ] [ Datang dari luar RS / rujukan ] [ Kembali dirawat setelah pulang ]
```

**SESUDAH**

```text
Langkah pasien umum: [Jenis Kunjungan] → [Jenis Pasien: Bayi] → klik [Lanjut]
                                                               │ transisi atomik
                                                               ▼
                                             [Langkah 1: Konteks Admisi Bayi]

┌─ Konteks Admisi Bayi ──────────────────────────────────────────────────────┐
│ Jenis kunjungan : Rujukan                         [dibaca dari langkah awal]│
│ Asal fasilitas  : Fasilitas perujuk               [baca-saja bila tersedia]│
│                                                                             │
│ Admisi ulang bayi setelah pernah pulang                                  ○ │
│ Gunakan bila bayi sudah memiliki RM dan episode sebelumnya sudah ditutup. │
│ Sistem membuat episode baru; episode lama tidak dibuka kembali.           │
│                                                                             │
│ [ Batal Pendaftaran ]                                [ Kembali ] [ Lanjut ]│
└─────────────────────────────────────────────────────────────────────────────┘
```

| Wilayah | Komponen | Keadaan/Perilaku |
| :--- | :--- | :--- |
| Stepper | `InpatientAdmissionStepper` | Selalu mulai pada Langkah 1 bayi; tidak pernah berkedip atau singgah di `payment`. |
| Ringkasan kunjungan | detail baca-saja | Menggunakan nilai yang sudah dipilih pada awal alur. |
| Admisi ulang | pilihan kondisional + helper | Hanya tersedia untuk pasien bayi lama dengan episode selesai. |
| Footer | aksi standar | Batal di kiri, Kembali dan Lanjut di kanan. |

### 3.2 Identitas serta Ringkasan Akhir

**SEBELUM**

```text
Pada setiap langkah:
┌─ Bayi dan ibu ──────────────────────────────────────────────────────────────┐
│ Bayi ... | No. RM ... | Ibu ... | Perawatan ... | Episode ...             │
└─────────────────────────────────────────────────────────────────────────────┘

Pada Langkah Identitas:
[Dropdown ibu sepanjang layar]
[Daftarkan Identitas Ibu sepanjang layar]
[Keterangan]
[Kartu ibu terverifikasi]
[Informasi Utama: Nama | No. RM]
[Form bayi]
```

**SESUDAH — LANGKAH 2 IDENTITAS**

```text
┌─ Identitas Bayi dan Ibu ────────────────────────────────────────────────────┐
│ Ibu bayi                                                                    │
│ [ Cari nama / No. RM ibu............................. ] [ Daftarkan Ibu ]   │
│ ┌─────────────────────────────────────────────────────────────────────────┐ │
│ │ IA  Ibu A                   RM ••••001          [ Terverifikasi ]       │ │
│ └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
│ Identitas bayi                                                              │
│ [Nama bayi*              ] [Tanggal lahir*] [Jam lahir ] [Jenis kelamin*]  │
│ [Tempat lahir            ] [NIK bila ada  ]                                 │
│ (• Kelahiran tunggal) (○ Kelahiran kembar)                                 │
│                                                                             │
│ [ Batal Pendaftaran ]                      [ Kembali ] [ Simpan & Lanjut ]  │
└─────────────────────────────────────────────────────────────────────────────┘
```

**SESUDAH — LANGKAH 8 TINJAU DAN AKTIFKAN**

```text
┌─ Ringkasan Identitas ────────────────────────────────────────────────────────┐
│ Bayi A · RM-DEMO-001 · Perempuan · lahir 15-10-2026 08:30                  │
│ Ibu A  · RM tersamar · Terverifikasi                                       │
└─────────────────────────────────────────────────────────────────────────────┘
┌─ Ringkasan Admisi ──────────────────────────────────────────────────────────┐
│ DPJP · Penjamin · Deposit · Unit/Kamar/Bed · Persetujuan wali              │
└─────────────────────────────────────────────────────────────────────────────┘
│ [ Batalkan Admisi ]                     [ Kembali ] [ Aktifkan Admisi ]     │
```

| Keadaan | Tampilan |
| :--- | :--- |
| Ibu belum dipilih | Search aktif; detail ibu tidak tampil; lanjut dinonaktifkan dan alasan terlihat. |
| Ibu dipilih | Satu baris ringkas, tidak ada kartu `Informasi Utama` duplikat. |
| Layar kecil | Seluruh field turun menjadi satu kolom; aksi utama tetap mudah dijangkau. |
| Langkah selain 2 dan 8 | Tidak menampilkan kartu ringkasan bayi–ibu. |

### 3.3 Deposit Bayi

**SEBELUM**

```text
┌─ Rencana Deposit Bayi ──────────────────────────────────────────────────────┐
│ [banner informasi khusus]                                                   │
│ Nominal rencana deposit [.................................................] │
│ [banner status pembayaran kasir]                                           │
│                                      [ Kembali ] [ Simpan Rencana & Lanjut ]│
└─────────────────────────────────────────────────────────────────────────────┘
```

**SESUDAH — pola yang sama dengan admisi umum**

```text
┌─ Rencana Deposit ────────────────────────────────────────────────────────────┐
│ [status kebijakan deposit / error policy / informasi minimum]              │
│ Nominal rencana deposit                                                     │
│ Rp [ 0 / nominal terformat............................................... ] │
│ [status cukup / kurang / tidak diwajibkan sesuai policy]                   │
│                                                                             │
│ [ Batal Pendaftaran ]                [ Kembali ] [ Lanjut ke Penempatan ]   │
└─────────────────────────────────────────────────────────────────────────────┘
```

| Wilayah | Sumber komponen | Catatan |
| :--- | :--- | :--- |
| Seluruh body deposit | `InpatientAdmissionDepositStep` | Dipakai bersama admisi umum; tidak ada salinan khusus bayi. |
| Tombol lanjut | prop `nextLabel` | Untuk bayi berlabel `Lanjut ke Penempatan`. |
| Pembatalan | handler lifecycle | Pada Langkah 5 episode belum terbentuk, sehingga memakai reset lokal. |

### 3.4 Penempatan Bed, Galat, dan Pembatalan

**SEBELUM**

```text
[Bed tampak tersedia] → pengguna memilih → POST encounter
                                      └── HTTP 400:
                                          Service unit tidak valid, tidak aktif,
                                          atau tidak tersedia untuk registrasi.
```

**SESUDAH**

```text
Query bed
  └─ unit aktif + tidak dihapus + availableForRegistration + tipe Inpatient
       └─ bed ditampilkan
            └─ POST encounter memvalidasi ulang predikat yang sama
                 ├─ sukses → episode Draft/reservasi terbentuk
                 └─ unit berubah/stale → alert ramah + refresh bed + tetap di langkah 6
```

```text
┌─ Tempat tidur perlu dipilih kembali ────────────────────────────────────────┐
│ Tempat tidur ini tidak lagi dapat dipakai karena unit layanannya belum     │
│ tersedia untuk registrasi. Daftar tempat tidur sudah diperbarui. Pilih     │
│ tempat lain atau minta admin mengaktifkan unit tersebut.                   │
└─────────────────────────────────────────────────────────────────────────────┘
│ [ Batal Pendaftaran / Batalkan Admisi* ]     [ Kembali ] [ Coba Lagi ]     │

* Ditentukan dari keberadaan episode Draft, bukan semata-mata nomor langkah.
```

| Keadaan lifecycle | Aksi kiri | Efek |
| :--- | :--- | :--- |
| Belum ada `episode.id` | `Batal Pendaftaran` | Reset form/session setelah konfirmasi. |
| Sudah ada episode `Draft` | `Batalkan Admisi` | Alasan wajib, cancel server, lepaskan reservasi, audit tersimpan. |
| Episode aktif | Tidak ada reset lokal | Pembatalan hanya melalui aksi episode sesuai kewenangan. |

---

## 4. Rincian Perbaikan

### FIX-EPS-022-01 — Transisi Atomik ke Langkah Pertama Bayi

| Parameter | Keterangan |
| :--- | :--- |
| Menutup | `ISS-EPS-022-01` |
| Area | Frontend |
| Berkas utama | `use-inpatient-admission-flow.jsx`, `inpatient-admission-view.jsx`, dan test alur admisi |
| Radius dampak | Pemilihan tipe pasien baru/lama dan sinkronisasi URL step |
| Bergantung pada | Tidak ada |

**Langkah pengerjaan:**

1. Pisahkan pilihan tipe pasien yang sedang diedit dari tipe pasien yang menentukan daftar langkah aktif, atau sediakan aksi `enterNewbornFlow()`.
2. Pada tombol **Lanjut**, set mode bayi dan `currentStep = newborn-context` secara atomik.
3. Jangan menjalankan fallback pencarian slug bersama ketika sebuah cabang baru baru saja dimasuki.
4. Pertahankan deep-link valid, tetapi redirect slug tidak valid ke langkah pertama cabang yang sesuai.
5. Tambahkan unit test untuk bayi pasien baru dan pasien lama, termasuk URL query `step`.

**Acceptance criteria:**

1. Memilih bayi tidak langsung memindahkan langkah sebelum pengguna menekan Lanjut.
2. Setelah Lanjut, layar dan URL berada di `newborn-context` (Langkah 1 dari 9).
3. Tidak pernah ada render perantara di `payment`.
4. Pilihan pasien umum dan alur pasien lama non-bayi tetap berjalan seperti sebelumnya.

**Verifikasi:** unit test reducer/hook, integration test view, dan live browser dengan inspeksi perubahan URL.

---

### FIX-EPS-022-02 — Ringkasan Hanya pada Konfirmasi

| Parameter | Keterangan |
| :--- | :--- |
| Menutup | `ISS-EPS-022-02` |
| Area | Frontend/UI |
| Berkas utama | `inpatient-admission-view.jsx`, `inpatient-newborn-admission-steps.jsx` |
| Radius dampak | Semua sembilan langkah admisi bayi |
| Bergantung pada | `FIX-EPS-022-01` untuk pengujian urutan yang stabil |

**Langkah pengerjaan:**

1. Hapus `NewbornAdmissionHeader` dari render global di induk view.
2. Buat ringkasan ringkas memakai base component yang ada di `NewbornReviewStep`/langkah Tinjau dan Aktifkan.
3. Tampilkan hanya data yang telah tersedia; gunakan status `Belum tersedia` tanpa ruang kosong besar.
4. Pastikan ringkasan tidak muncul di Langkah 1–7 dan Langkah 9.

**Acceptance criteria:** tidak ada kartu `Bayi dan ibu` berulang; Langkah 8 menampilkan ringkasan bayi, ibu, RM, DPJP, penjamin, deposit, bed, dan status persetujuan secara ringkas.

---

### FIX-EPS-022-03 — Konteks Tanpa Duplikasi dan Admisi Ulang yang Jelas

| Parameter | Keterangan |
| :--- | :--- |
| Menutup | `ISS-EPS-022-03`, `ISS-EPS-022-04` |
| Area | Frontend + adapter kontrak; backend bila diperlukan untuk riwayat episode |
| Berkas utama | `inpatient-newborn-admission-constants.js`, `inpatient-newborn-admission-steps.jsx`, hook newborn, mapper payload, service riwayat episode terkait |
| Radius dampak | Langkah 1 bayi dan nilai `newbornAdmission.source` |
| Bergantung pada | Keputusan `K-01`; `FIX-EPS-022-01` |

**Langkah pengerjaan:**

1. Hapus kartu input `Datang dari luar RS / rujukan`.
2. Baca jenis kunjungan dari state langkah awal dan tampilkan sebagai informasi baca-saja.
3. Petakan kategori awal ke enum backend melalui satu fungsi adapter yang diuji.
4. Ubah label menjadi `Admisi ulang bayi setelah pernah pulang` dan tampilkan helper yang menyebut RM, episode selesai, dan episode baru.
5. Sembunyikan/nonaktifkan admisi ulang jika pasien belum mempunyai RM atau episode sebelumnya belum selesai.
6. Jika data riwayat belum tersedia pada kontrak frontend, tambahkan kebutuhan endpoint/read model setelah keputusan owner, tanpa mengarang status dari UI.

**Acceptance criteria:**

1. Pengguna rujukan tidak diminta memilih rujukan dua kali.
2. Nilai payload `External/HospitalBirth/Readmission` tetap konsisten dan teruji.
3. Admisi ulang tidak dapat dipakai untuk pasien baru tanpa RM.
4. Helper menjelaskan bahwa episode lama tidak dibuka kembali.

---

### FIX-EPS-022-04 — Penyusunan Ulang Identitas Bayi dan Ibu

| Parameter | Keterangan |
| :--- | :--- |
| Menutup | `ISS-EPS-022-05` |
| Area | Frontend/UI |
| Berkas utama | `inpatient-newborn-admission-steps.jsx` dan stylesheet/module terkait bila ada |
| Radius dampak | Langkah 2 identitas bayi |
| Bergantung pada | Revisi skema tampilan hulu |

**Langkah pengerjaan:**

1. Kelompokkan layar menjadi `Ibu bayi` dan `Identitas bayi`.
2. Letakkan search ibu dan tombol `Daftarkan Ibu` dalam satu area responsif.
3. Ganti kartu informasi generik berulang dengan satu baris identitas ibu terpilih dan badge verifikasi.
4. Susun field bayi dalam grid desktop dan satu kolom mobile.
5. Pertahankan field wajib, helper klinis, pilihan tunggal/kembar, dan validasi yang sudah ada.
6. Pasang footer konsisten: Batal, Kembali, Simpan & Lanjut.

**Acceptance criteria:**

1. Tidak ada pengulangan nama/RM ibu dalam dua kartu besar.
2. Semua field yang semula tersedia tetap dapat diisi.
3. Kesalahan validasi tampil di dekat field terkait.
4. Pada lebar mobile tidak ada overflow horizontal dan urutan tab logis.
5. Identitas sensitif ditampilkan seperlunya dan RM dapat dimasking pada ringkasan.

---

### FIX-EPS-022-05 — Reuse Komponen Deposit Admisi Umum

| Parameter | Keterangan |
| :--- | :--- |
| Menutup | `ISS-EPS-022-06` |
| Area | Frontend |
| Berkas utama | `inpatient-admission-deposit-step.jsx`, `inpatient-newborn-admission-steps.jsx`, `inpatient-admission-view.jsx` |
| Radius dampak | Deposit admisi umum dan bayi |
| Bergantung pada | `FIX-EPS-022-07` untuk handler batal yang seragam |

**Langkah pengerjaan:**

1. Tambahkan props sempit pada komponen umum bila perlu: label lanjut, teks konteks, dan callback.
2. Ganti render `NewbornDepositStep` dengan komponen umum.
3. Pastikan langkah bayi menuju Penempatan, bukan kembali ke Dokter.
4. Hapus implementasi khusus bayi setelah tidak mempunyai pemanggil.
5. Jalankan regresi pada admisi umum agar label dan perilakunya tidak berubah.

**Acceptance criteria:** tampilan/state deposit umum dan bayi berasal dari komponen yang sama; policy loading/error, format rupiah, status nominal, navigasi, dan pembatalan berfungsi pada kedua alur.

---

### FIX-EPS-022-06 — Predikat Tunggal Kelayakan Service Unit

| Parameter | Keterangan |
| :--- | :--- |
| Menutup | `ISS-EPS-022-07` |
| Area | Backend + Frontend |
| Berkas backend | `InpBedOccupancyService.cs`, helper/specification kelayakan unit baru atau yang sudah ada, `PatientEncounterController.cs`/service validasi terkait, test backend |
| Berkas frontend | `use-inpatient-admission-doctor.jsx`, service encounter, controller/list bed, komponen alert penempatan, test frontend |
| Radius dampak | Daftar bed rawat inap dan semua encounter admin yang memakai service unit rawat inap |
| Bergantung pada | Backend lebih dahulu, lalu adaptasi frontend |

**Langkah pengerjaan backend:**

1. Definisikan satu ekspresi/specification kelayakan unit registrasi rawat inap: aktif, tidak dihapus, tersedia untuk registrasi, dan bertipe `Inpatient`.
2. Terapkan pada query daftar bed dan evaluasi eligibility, termasuk relasi ruangan–unit.
3. Pakai aturan yang sama pada validasi pembentukan encounter atau uji ekuivalensinya bila controller belum dapat memakai expression secara langsung.
4. Pertahankan revalidasi dalam transaksi tulis untuk menangani stale data.
5. Tambahkan test bahwa unit invalid tidak pernah ditawarkan dan tetap ditolak bila statusnya berubah sesaat sebelum submit.

**Langkah pengerjaan frontend:**

1. Kenali error kelayakan service unit melalui code terstruktur bila backend menyediakannya; hindari ketergantungan permanen pada pencocokan teks.
2. Tampilkan pesan ramah yang telah ditetapkan, tetap pada Langkah 6, pertahankan semua input selain pilihan bed, kosongkan bed invalid, lalu refresh daftar.
3. Sediakan `Coba Lagi` dan jangan melakukan submit otomatis tanpa pilihan pengguna.

**Acceptance criteria:**

1. Bed dari unit nonaktif, dihapus, tidak tersedia untuk registrasi, atau bukan Rawat Inap tidak muncul sebagai tersedia.
2. Endpoint encounter tetap menolak perubahan unit yang terjadi setelah daftar dimuat.
3. Pada penolakan stale, tidak ada pesan teknis mentah; form tidak hilang dan daftar bed diperbarui.
4. Tidak ada episode setengah jadi atau reservasi yatim bila pembentukan encounter gagal.
5. Test backend dan frontend mencakup keempat flag kelayakan serta race condition.

**Risiko:** predikat yang terlalu ketat dapat menghilangkan bed yang master datanya belum lengkap. Mitigasi: keluarkan laporan diagnostik admin tanpa memperlihatkan detail teknis tersebut kepada petugas admisi, lalu perbaiki master data—bukan melonggarkan keselamatan validasi.

---

### FIX-EPS-022-07 — Pembatalan Sadar-Lifecycle pada Alur Bayi

| Parameter | Keterangan |
| :--- | :--- |
| Menutup | `ISS-EPS-022-08` |
| Area | Frontend; memakai API episode yang sudah ada |
| Berkas utama | `inpatient-newborn-admission-steps.jsx`, `inpatient-admission-view.jsx`, helper footer aksi bila dipisah, test pembatalan |
| Radius dampak | Semua langkah bayi, reset session, reservasi bed, dan audit pembatalan episode |
| Bergantung pada | `FIX-EPS-022-01`; reuse handler pembatalan admisi umum |

**Langkah pengerjaan:**

1. Perluas footer `Actions` agar menerima `onResetAdmission`, `onCancelAdmission`, `episodeId`, status episode, loading, dan hak akses yang diperlukan.
2. Tentukan jenis pembatalan dari keberadaan episode/status transaksi, bukan hanya nomor langkah.
3. Langkah 1–5 dan Langkah 6 sebelum episode: tampilkan `Batal Pendaftaran` dengan konfirmasi sederhana; bersihkan state dan session storage lalu kembali ke tipe pendaftaran.
4. Setelah episode `Draft` ada pada Langkah 6, serta Langkah 7–8: tampilkan `Batalkan Admisi`; alasan wajib; panggil endpoint cancel; lepaskan reservasi; lalu bersihkan alur.
5. Resume episode `Draft`: gunakan `Batalkan Admisi` di semua langkah yang dapat dibuka.
6. Langkah 9: jangan tampilkan reset pendaftaran. Arahkan operasi episode aktif ke Daftar Kerja/Detail Episode dan terapkan otorisasi server.
7. Cegah double-submit dan tampilkan loading/error yang dapat dipulihkan.

**Acceptance criteria:**

1. Tombol berada di sisi kiri footer dan konsisten dengan admisi umum.
2. Reset sebelum episode tidak memanggil endpoint cancel dan tidak menyisakan state lokal.
3. Pembatalan setelah episode selalu meminta alasan, tercatat `Cancelled`, dan membebaskan bed.
4. Petugas tanpa wewenang tidak dapat membatalkan episode aktif melalui manipulasi UI/API.
5. Jika API cancel gagal, episode/bed tidak disamarkan sebagai berhasil dibatalkan dan pengguna tetap melihat alasan serta pesan pemulihan.

---

## 5. Urutan dan Dependency Pengerjaan

```text
Revisi dokumen hulu dan persetujuan PLAN-REPAIR-022
├── FIX-EPS-022-01  Navigasi cabang bayi
│   ├── FIX-EPS-022-02  Ringkasan hanya di konfirmasi
│   ├── FIX-EPS-022-03  Konteks dan admisi ulang
│   ├── FIX-EPS-022-04  Identitas bayi–ibu
│   └── FIX-EPS-022-07  Pembatalan sadar-lifecycle
│       └── FIX-EPS-022-05  Reuse deposit + aksi batal
└── FIX-EPS-022-06  Predikat unit backend
    └── Penanganan error dan refresh bed frontend

Semua FIX selesai
└── pengujian unit/integrasi
    └── live browser end-to-end
        └── audit kesiapan/sign-off
```

Urutan prioritas eksekusi yang disarankan:

1. `FIX-EPS-022-01` agar seluruh test memasuki langkah bayi yang benar.
2. Backend `FIX-EPS-022-06` agar sumber data bed aman, lalu frontend recovery-nya.
3. `FIX-EPS-022-07` agar tidak ada episode/reservasi yatim selama pengujian.
4. `FIX-EPS-022-03`, `04`, `05`, lalu `02`.
5. Regresi admisi umum dan pengujian end-to-end alur bayi.

---

## 6. Keputusan Pemilik yang Dibutuhkan

| ID | Keputusan | Pilihan | Rekomendasi | Menahan |
| :--- | :--- | :--- | :--- | :--- |
| `K-01` | Penentuan `Admisi ulang bayi setelah pernah pulang` | (A) pengguna memilih bebas; (B) hanya tersedia bila server menemukan bayi lama dengan episode selesai | **B — DISETUJUI 2026-10-15**, agar tidak terjadi episode ulang palsu dan makna status konsisten | Bagian admisi ulang pada `FIX-EPS-022-03` |

Keputusan lain telah diberikan langsung oleh pelapor dan diperlakukan sebagai scope rencana: kartu ringkasan hanya di konfirmasi, tidak ada input rujukan ganda, deposit memakai tampilan admisi umum, serta tombol pembatalan mengikuti contoh admisi umum.

---

## 7. Dokumen Hulu yang Harus Direvisi Setelah Rencana Disetujui

| Dokumen | Bagian | Perubahan yang Diperlukan |
| :--- | :--- | :--- |
| `05-skema-tampilan.md` | Amendemen alur bayi | Hapus header global, ubah skema konteks dan identitas, gunakan deposit umum, tambahkan matriks tombol batal. |
| `contracts/newborn-admission-repair-016.md` | Step contract dan `newbornAdmission.source` | Dokumentasikan derivasi sumber dari kategori awal dan aturan admisi ulang; tegaskan ringkasan hanya di Langkah 8. |
| `03-frontend-architecture.md` | State/navigation admisi bayi | Tetapkan transisi atomik cabang dan reuse komponen deposit/action footer. |
| `contracts/api-contract.md` | Bed availability dan encounter admin | Samakan predikat unit registrasi rawat inap dan tambahkan code error terstruktur untuk stale unit. |
| `contracts/validation-matrix.md` | Validasi bed/unit dan pembatalan | Tambahkan matriks empat syarat unit, race condition, reset lokal, serta cancel episode Draft. |

Revisi dokumen hulu dilakukan oleh tahap desain/delivery berikutnya; dokumen diagnosis ini tidak mengubah kontrak yang telah disetujui.

---

## 8. Verifikasi Menyeluruh

### 8.1 Test Otomatis Frontend

1. Pasien baru → jenis pasien bayi → Lanjut menghasilkan `step=newborn-context`.
2. Pasien lama bayi → alur kategori → `newborn-context` tanpa singgah di `payment`.
3. Stepper browser back/forward dan refresh mempertahankan langkah valid.
4. `NewbornAdmissionHeader` tidak dirender pada Langkah 1–7 dan 9; ringkasan ada pada Langkah 8.
5. Kategori rujukan tampil baca-saja dan payload adapter menghasilkan `External`.
6. Admisi ulang tersembunyi/nonaktif tanpa bukti episode selesai.
7. Deposit bayi dan umum memakai state behavior yang sama, tetapi label tujuan bayi benar.
8. Error unit stale menghapus hanya pilihan bed, mempertahankan isian lain, memuat ulang daftar, dan menampilkan salinan ramah.
9. Reset lokal dan cancel server dipanggil sesuai keberadaan `episode.id`.

### 8.2 Test Otomatis Backend

1. Daftar bed mengecualikan unit `IsActive = false`.
2. Daftar bed mengecualikan unit terhapus.
3. Daftar bed mengecualikan unit `IsAvailableForRegistration = false`.
4. Daftar bed mengecualikan unit bukan tipe `Inpatient`.
5. Encounter berhasil untuk unit yang memenuhi seluruh predikat.
6. Perubahan unit menjadi invalid setelah list dimuat tetap ditolak secara atomik dan tidak membuat encounter/episode parsial.
7. Cancel episode `Draft` oleh petugas admisi melepaskan reservasi dan menyimpan alasan/audit.

### 8.3 Live Browser End-to-End

Gunakan hanya data uji non-pribadi.

| Skenario | Langkah Ringkas | Hasil Diharapkan |
| :--- | :--- | :--- |
| Bayi baru lahir di RS | Pilih pasien baru, kategori umum, bayi, lanjut hingga konfirmasi | Mulai di Langkah 1, tidak ada field rujukan ulang, identitas rapi, ringkasan hanya Langkah 8. |
| Bayi rujukan | Pilih kategori rujukan dan bayi | Data rujukan awal tampil baca-saja; tidak diminta memilih `luar RS/rujukan` lagi. |
| Admisi ulang valid | Pilih bayi lama dengan episode selesai | Label/helper jelas dan episode baru dibuat tanpa membuka episode lama. |
| Deposit | Isi kosong, nol, minimum, dan di bawah policy | Tampilan dan validasi sama dengan admisi umum; tujuan lanjut ke Penempatan. |
| Bed unit valid | Pilih bed pada unit siap registrasi | Encounter dan episode Draft berhasil terbentuk. |
| Bed unit berubah/stale | Nonaktifkan eligibility unit melalui fixture/test setup setelah list dimuat, lalu submit | Tetap di Langkah 6, pesan ramah, bed di-refresh, tidak ada transaksi parsial. |
| Batal sebelum episode | Klik Batal Pendaftaran pada Langkah 1–5/awal Langkah 6 | Konfirmasi, state bersih, kembali ke awal, tanpa API cancel. |
| Batal setelah episode | Setelah Draft/reservasi terbentuk, klik Batalkan Admisi dan isi alasan | Episode Cancelled, bed bebas, audit ada, alur reset. |
| Langkah 9 | Aktifkan admisi lalu buka dokumen bayi | Tidak ada tombol reset pendaftaran; tindakan episode mengikuti kewenangan resmi. |
| Responsif | Uji desktop dan viewport mobile | Tidak ada overflow; field satu kolom pada mobile; footer tetap terbaca. |

### 8.4 Definition of Done

Rencana dianggap selesai hanya jika:

1. seluruh acceptance criteria tujuh FIX terpenuhi;
2. test frontend/backend dan regresi admisi umum lulus;
3. bukti live browser tersimpan sesuai standar pengujian modul;
4. dokumen hulu telah diselaraskan;
5. tidak ada data pasien nyata pada fixture/screenshot; dan
6. register status di dokumen ini diperbarui dengan Task ID serta tautan bukti implementasi.

---

## 9. Risiko dan Rollback

| Risiko | Mitigasi | Rollback |
| :--- | :--- | :--- |
| Perubahan navigasi memengaruhi pasien umum | Test matriks semua tipe pasien dan URL deep-link | Kembalikan hanya aksi transisi cabang, bukan data pasien |
| Filter unit mengurangi jumlah bed yang tampak | Sediakan laporan master data untuk admin; jangan longgarkan validasi registrasi | Nonaktifkan deployment filter hanya setelah data dikoreksi dan dengan tetap menjaga validasi encounter |
| Reuse deposit mengubah label umum | Props memiliki default sama dengan perilaku existing | Lepas props pada pemanggil bayi dan pulihkan render lama sementara |
| Tombol reset salah dipakai setelah episode terbentuk | Keputusan berdasarkan `episode.id/status`; test resume Draft | Sembunyikan tombol bila lifecycle tidak dapat ditentukan, jangan reset lokal |
| Riwayat episode untuk admisi ulang belum tersedia | Tahan hanya pilihan admisi ulang, lanjutkan FIX lain | Pertahankan source enum lama tanpa menampilkan pilihan bebas |

---

## 10. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :--- | :--- | :--- |
| 2026-10-15 | Rencana dibuat berdasarkan analisis source frontend/backend, blueprint admisi bayi sebelumnya, enam lampiran, dan tambahan permintaan tombol pembatalan. | Codex |
| 2026-10-15 | Rencana dan rekomendasi `K-01` disetujui pemilik sistem melalui instruksi untuk mengimplementasikan seluruh perbaikan. | Codex |
| 2026-10-16 | Tujuh perbaikan selesai pada source frontend/backend; laporan task dan bukti validasi ditautkan pada register. Validasi live browser dengan API/database nyata masih menjadi handoff. | Codex |

