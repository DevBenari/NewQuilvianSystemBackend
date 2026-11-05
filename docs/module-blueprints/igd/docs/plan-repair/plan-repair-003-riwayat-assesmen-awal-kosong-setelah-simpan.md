# PLAN-REPAIR-003 — Muat Ulang Riwayat Assesmen Awal IGD Setelah Pengkajian Berhasil Disimpan

```yaml
plan_id: PLAN-REPAIR-IGD-003
issue: ../issue/issue-003-riwayat-assesmen-awal-kosong-setelah-simpan.md
status_rencana: DISETUJUI
tanggal_rencana: "2026-11-04"
diputuskan_oleh: "Rizki Gunawan (persetujuan lisan di sesi kerja)"
tanggal_keputusan: "2026-11-04"
basis_source_backend: "30f1396d (rizkiG)"
basis_source_frontend: "739e05f68 (RizkiV2)"
```

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-IGD-003-01` | `ISS-IGD-003-01` | Panggil kembali `reload()` setelah pengkajian berhasil disimpan, mengikuti pola tab SOAP | FE | 1 | — | 🟡 SEBAGIAN — AC 1–6 terverifikasi di browser; AC 7 terbatas (pasien uji sudah `InTreatment`); laporan task tracked belum ada | [laporan uji](../../perawat-igd/testing/test-by-agy/laporan-live-testing-riwayat-assesmen-awal.md) |
| `FIX-IGD-003-02` | `ISS-IGD-003-T1` | Ganti keterangan segmen Riwayat dengan kalimat petugas, tanpa kata "backend" | FE | 2 | — | 🟡 SEBAGIAN — AC 2 dari 2 terverifikasi di browser; laporan task tracked belum ada | [laporan uji](../../perawat-igd/testing/test-by-agy/laporan-live-testing-riwayat-assesmen-awal.md) |
| `FIX-IGD-003-03` | Pencegahan (catatan pola issue) | Tambah satu kalimat pada `03-frontend-architecture.md` §17.13.1: `onVisitUpdated` tidak menggantikan muatan ulang daftar tab | Dokumen | 3 | — | BELUM DIKERJAKAN | — |

**Ringkasan: 0 dari 3 perbaikan selesai; 2 sebagian (lolos uji browser 4 Nov 2026, menunggu laporan task tracked dan commit).**
`✅` baru sah setelah acceptance criteria diuji dan laporan task tracked ditautkan.

---

## 2. Solusi terpilih per temuan

### ISS-IGD-003-01 — Daftar riwayat tidak dimuat ulang sesudah pengkajian berhasil disimpan

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Panggil `reload()` di `submit` setelah simpan berhasil, sebelum layar pindah ke Riwayat — sama seperti tab SOAP | Satu berkas, beberapa baris; memulihkan perilaku sebelum regresi; pola sudah terbukti di tab SOAP, Observasi, Nosokomial, Tindak Lanjut, Resep | Setiap tab tetap harus "ingat" memanggil `reload()` sendiri |
| B | Ubah hook halaman agar memuat ulang otomatis setiap daftar yang ditandai `loaded = false` oleh slice | Berlaku untuk semua tab sekaligus | Radius dampak seluruh ruang kerja; tab yang sudah memanggil `reload()` sendiri akan meminta dua kali; perlu uji ulang semua tab |
| C | Sisipkan hasil `POST` langsung ke daftar tanpa bertanya ke server | Tanpa permintaan tambahan | Respons simpan (`PatientAssessmentCreateResponse`) tidak memuat semua kolom riwayat; menyalahi `IGD-DEC-133` yang meminta daftar **dimuat ulang** |

**Solusi terpilih: Opsi A.**

1. Persis yang diminta `IGD-DEC-133` (`00-interview-decisions.md:4286`): pindah ke Riwayat **dan**
   daftar dimuat ulang.
2. Radius dampak terkecil — hanya `emergency-assessment-initial-tab.jsx`, berkas milik modul IGD.
3. Memakai ulang fungsi `reload` yang sudah ada di berkas itu (`:660-663`) dan pola tab SOAP
   (`emergency-assessment-soap-tab.jsx:274-278`).
4. Tidak menyentuh `IGD-DEC-311`: transisi `Triaged → InTreatment` dan `onVisitUpdated` tetap ada.

**Kenapa bukan opsi lain.** Opsi B memperbaiki satu tab dengan mengubah perilaku semua tab — risiko
lebih besar dari masalahnya. Opsi C bertentangan dengan keputusan yang disetujui dan menampilkan
data yang tidak lengkap.

### ISS-IGD-003-T1 — Keterangan segmen Riwayat memakai istilah teknis

Hanya ada satu cara yang wajar: ganti teksnya. Usulan: *"Daftar diperbarui otomatis setiap kali
pengkajian berhasil disimpan."* Teks keterangan ini wewenang pengembang (`DEV_DISCRETION`), jadi
tidak butuh keputusan pemilik.

---

## 3. Skema tampilan sebelum → sesudah

Susunan wilayah tidak berubah. Yang berubah hanya isi segmen Riwayat sesudah menyimpan, badge
jumlah, baris "Terakhir dikaji", dan teks keterangan.

**SEBELUM** (sesuai lampiran 1, sesaat setelah *Simpan Pengkajian* berhasil):

```text
┌───────────────────────────────────────────────────────────────────────┐
│ [Assesmen Awal IGD] [Observasi] [SOAP] [Resep] [Catatan Terintegrasi] │
│ [Nosokomial] [Tindak Lanjut]                                          │
├───────────────────────────────────────────────────────────────────────┤
│ [ Formulir Pengkajian Aktif ]   (•) Riwayat Dokumen Pengkajian        │  ← tanpa badge jumlah
│                                                  [ Pengkajian baru ]  │
│ Riwayat Assesmen Awal                                                 │
│ Dimuat ulang dari backend setiap kali penyimpanan berhasil.           │
│ ┌───────────────────────────────────────────────────────────────────┐ │
│ │        Belum ada assesmen awal untuk pasien ini.                  │ │  ← padahal sudah tersimpan
│ └───────────────────────────────────────────────────────────────────┘ │
└───────────────────────────────────────────────────────────────────────┘
```

**SESUDAH** (contoh pasien pseudonim "Tn. Pseudonim B", satu pengkajian baru tersimpan):

```text
┌───────────────────────────────────────────────────────────────────────┐
│ [Assesmen Awal IGD] [Observasi] [SOAP] [Resep] [Catatan Terintegrasi] │
│ [Nosokomial] [Tindak Lanjut]                                          │
├───────────────────────────────────────────────────────────────────────┤
│ [ Formulir Pengkajian Aktif ]   (•) Riwayat Dokumen Pengkajian (1)    │
│                                                  [ Pengkajian baru ]  │
│ Riwayat Assesmen Awal                                                 │
│ Daftar diperbarui otomatis setiap kali pengkajian berhasil disimpan.  │
│ ┌───────────────────────────────────────────────────────────────────┐ │
│ │ PA-20261104-0007                                                  │ │
│ │ Status: Completed        Waktu pengkajian: 4 Nov 2026 09.10       │ │
│ │ Keluhan utama: Nyeri dada kiri sejak 2 jam                        │ │
│ │ Skala nyeri: 6           Catatan nyeri: -                         │ │
│ │ Status lokalis: …        …                                        │ │
│ └───────────────────────────────────────────────────────────────────┘ │
└───────────────────────────────────────────────────────────────────────┘
```

Nomor dokumen dan isi di atas hanya contoh. Format nomor mengikuti yang dibuat server.

**Wilayah**

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Segmen | Formulir / Riwayat, badge jumlah pada Riwayat | `section.items.length` | `EmergencyAssessmentWorkPanel` (tidak berubah) |
| Keterangan | Kalimat petugas baru (`FIX-IGD-003-02`) | Teks tetap | `EmergencyAssessmentRecordTab` prop `description` |
| Daftar riwayat | Satu kartu per pengkajian, terbaru di atas | `GET /patient-assessments?patientId=&encounterId=&sortDirection=desc` | `EmergencyAssessmentRecordTab` (tidak berubah) |
| Baris "Terakhir dikaji" (segmen Formulir) | Waktu, pencatat, keluhan, skala nyeri pengkajian terbaru | Daftar riwayat yang sama | `EmergencyAssessmentLastStrip` (tidak berubah) |

**Tombol**

| Label | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Simpan Pengkajian | Utama | Sesuai aturan validasi sekarang (tidak berubah) | Simpan → bila berhasil: kosongkan formulir, transisi status (`IGD-DEC-311`), segarkan kunjungan, **muat ulang daftar riwayat**, pindah ke segmen Riwayat |
| Pengkajian baru | Sekunder | Selalu, di segmen Riwayat | Kembali ke segmen Formulir (tidak berubah) |
| Coba lagi | Sekunder | Saat daftar gagal dimuat | Memuat ulang daftar (tidak berubah) |

**Keadaan**

| Keadaan | Tampilan |
| --- | --- |
| Memuat | Penanda memuat pada segmen Riwayat selama daftar diminta ulang |
| Kosong | "Belum ada assesmen awal untuk pasien ini." — hanya bila server memang tidak punya data |
| Gagal | Pesan galat + tombol Coba lagi; pengkajian yang tersimpan tetap aman di server |
| Tidak berhak | Pesan hak akses baku (tidak berubah) |
| Simpan gagal | Tetap di segmen Formulir, isian tidak hilang, daftar **tidak** dimuat ulang (tidak berubah) |

---

## 4. Rincian perbaikan

### FIX-IGD-003-01 — Muat ulang daftar riwayat setelah simpan berhasil

| | |
| --- | --- |
| **Menutup** | `ISS-IGD-003-01` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-initial-tab.jsx` — fungsi `submit` (`:665-723`) |
| **Radius dampak** | Hanya tab Assesmen Awal IGD pada Ruang Kerja Pemeriksaan IGD. Komponen ini dipakai satu tempat saja (`emergency-assessment-detail-view.jsx:233`). Tab SOAP ikut diuntungkan karena membaca `sections.assessments` yang sama |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Di dalam blok `if (createPatientAssessment.fulfilled.match(result))`, panggil `reload()` setelah
   `onVisitUpdated?.()` dan sebelum `setSavedSignal(...)` — urutan yang sama dengan tab SOAP.
2. Tambahkan `reload` ke daftar dependency `useCallback` milik `submit`.
3. Jangan mengubah blok transisi status `IGD-DEC-311` maupun pemanggilan `onVisitUpdated`.

**Acceptance criteria.**

1. Pasien uji "Tn. Pseudonim B" belum punya pengkajian. Perawat mengisi Assesmen Awal IGD lalu
   menekan *Simpan Pengkajian*. Layar pindah ke segmen Riwayat dan menampilkan **1 kartu** dengan
   nomor dokumen, status, waktu pengkajian, dan keluhan utama yang baru diisi — tanpa pindah tab
   dan tanpa F5.
2. Segmen Riwayat menampilkan badge jumlah **1**.
3. Setelah menekan *Pengkajian baru*, baris **"Terakhir dikaji"** di atas formulir tampil dengan
   waktu dan nama pencatat pengkajian tadi.
4. Pengkajian kedua untuk pasien yang sama menghasilkan **2 kartu**, terbaru di atas, badge **2**.
5. Pada tab Network peramban, setelah `POST /patient-assessments` berbalas `201`, ada satu
   `GET /patient-assessments` dengan `patientId` dan `encounterId` pasien tersebut.
6. Bila penyimpanan ditolak server (contoh: isian wajib dikosongkan sehingga `400`), layar tetap
   di segmen Formulir, isian tidak hilang, dan tidak ada `GET` daftar tambahan.
7. Pasien berstatus `Triaged` tetap berpindah ke "Sedang ditangani" setelah pengkajian disimpan
   (`IGD-DEC-311` tidak rusak).

**Verifikasi.**

- `npx eslint` pada berkas yang diubah.
- Build frontend sesuai kebiasaan repository (cek port 3000 lebih dulu).
- Uji manual atau `live-browser-testing` terhadap kriteria 1–7 dengan akun peran Perawat IGD.

**Risiko.**

- Daftar `assessments` di slice tidak punya penjaga "balasan permintaan terakhir" (`attachSection`,
  `emergency-assessment-slice.jsx:1203-1222`). Bila penyimpanan data kedatangan sebelumnya
  mengubah keluhan utama, hook halaman bisa ikut meminta daftar sesaat sebelum `POST` selesai.
  Permintaan itu hampir pasti berbalas lebih dulu daripada `reload()` sesudah `POST`, jadi hasil
  akhirnya tetap benar. Deteksi: kriteria 5 — `GET` terakhir di tab Network harus sesudah `POST`.
  (DUGAAN, risiko rendah; tidak perlu diperbaiki pada rencana ini.)

### FIX-IGD-003-02 — Ganti keterangan segmen Riwayat

| | |
| --- | --- |
| **Menutup** | `ISS-IGD-003-T1` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend (teks) |
| **Berkas yang diubah** | `emergency-assessment-initial-tab.jsx:1069` — prop `description` |
| **Radius dampak** | Hanya teks di segmen Riwayat Assesmen Awal IGD |
| **Bergantung pada** | `FIX-IGD-003-01` (kalimat baru baru benar setelah daftar benar-benar dimuat ulang) |

**Langkah.**

1. Ganti `"Dimuat ulang dari backend setiap kali penyimpanan berhasil."` menjadi
   `"Daftar diperbarui otomatis setiap kali pengkajian berhasil disimpan."`

**Acceptance criteria.**

1. Segmen Riwayat Assesmen Awal tidak lagi memuat kata "backend".
2. Kalimat yang tampil persis: "Daftar diperbarui otomatis setiap kali pengkajian berhasil
   disimpan."

**Verifikasi.** Lihat langsung di layar; ikut diuji bersama `FIX-IGD-003-01`.

**Risiko.** Tidak ada — perubahan teks saja.

### FIX-IGD-003-03 — Catatan pencegahan pada arsitektur frontend

| | |
| --- | --- |
| **Menutup** | Pola regresi pada issue bagian 8 |
| **Area** | Dokumen |
| **Jenis perubahan** | Revisi dokumen blueprint lewat jalur yang berwenang |
| **Berkas yang diubah** | `03-frontend-architecture.md` §17.13.1 butir 1 |
| **Radius dampak** | Tidak ada pada source |
| **Bergantung pada** | Tidak ada; tidak menahan `FIX-01` dan `FIX-02` |

**Langkah.** Lihat bagian 7.

**Acceptance criteria.**

1. §17.13.1 butir 1 memuat kalimat bahwa `onVisitUpdated` hanya menyegarkan data kunjungan dan
   tab tetap wajib memuat ulang daftarnya sendiri setelah simpan berhasil (`IGD-DEC-133`).

**Verifikasi.** Baca ulang bagian tersebut.

**Risiko.** Tidak ada.

---

## 5. Urutan pengerjaan

```text
FIX-IGD-003-01   tanpa dependency — boleh dikerjakan sekarang
└── FIX-IGD-003-02   teks baru benar setelah FIX-01
FIX-IGD-003-03   tanpa dependency — dokumen, boleh kapan saja
```

`FIX-01` dan `FIX-02` berada di berkas yang sama dan muat dalam satu task builder.

---

## 6. Keputusan pemilik yang dibutuhkan

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Bila ternyata ada dokumen pengkajian ganda (pertanyaan P-02 pada issue), bagaimana menanganinya? | Batalkan dokumen berlebih lewat aksi Batal pengkajian di aplikasi / biarkan | Batalkan lewat aplikasi dengan alasan "terisi ganda karena riwayat tidak tampil" | Tidak ada |

Selain K-01, yang dibutuhkan hanya persetujuan rencana ini.

---

## 7. Dokumen hulu yang ikut direvisi

`ISS-IGD-003-01` adalah `BUG`, bukan `DESIGN_CHANGE` — desain `IGD-DEC-133` sudah benar dan tidak
perlu diubah. Satu penambahan pencegahan diusulkan:

| Dokumen | Bagian | Sekarang | Menjadi |
| --- | --- | --- | --- |
| `03-frontend-architecture.md` | §17.13.1 butir 1, poin ketiga | "Callback `onVisitUpdated?.()` dipanggil seketika untuk me-refresh data kunjungan, sehingga lencana status di kartu pasien berubah …" | Kalimat lama **ditambah**: "`onVisitUpdated` hanya menyegarkan data kunjungan; tab tetap wajib memuat ulang daftar riwayatnya sendiri setelah simpan berhasil (`IGD-DEC-133`)." |

---

## 8. Verifikasi menyeluruh

1. Login sebagai Perawat IGD. Buka pasien uji berstatus `Triaged` yang belum punya pengkajian awal.
2. Buka tab Assesmen Awal IGD → segmen Riwayat kosong, tanpa badge.
3. Isi formulir minimal (keluhan, tanda vital) → *Simpan Pengkajian*.
4. Pastikan: pindah ke Riwayat, 1 kartu tampil, badge "1", keterangan baru tampil, kartu pasien
   berubah ke "Sedang ditangani".
5. *Pengkajian baru* → baris "Terakhir dikaji" tampil.
6. Simpan pengkajian kedua → 2 kartu, terbaru di atas.
7. Buka tab SOAP → isian yang diambil dari pengkajian terbaru tetap terbaca.
8. Kosongkan isian wajib lalu simpan → tetap di Formulir, isian tidak hilang.

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-11-04 | Rencana dibuat, 3 perbaikan, menunggu persetujuan | `diagnose-module-issue` |
| 2026-11-04 | Rencana disetujui Rizki Gunawan (lisan, sesi kerja). `FIX-01` dan `FIX-02` dikerjakan pada `emergency-assessment-initial-tab.jsx` di atas basis `739e05f68`; menunggu uji browser kriteria 1–7. `FIX-03` belum dikerjakan | Claude Code |
| 2026-11-04 | Live browser testing Chromium, akun Perawat IGD: 7/7 skenario lulus pada putaran 2 (putaran 1 gagal 1 karena kesalahan skrip, bukan aplikasi). AC 7 hanya teruji terbatas. Temuan sampingan: kolom STATUS menampilkan angka mentah. Laporan: `perawat-igd/testing/test-by-agy/laporan-live-testing-riwayat-assesmen-awal.md` | Claude Code |
