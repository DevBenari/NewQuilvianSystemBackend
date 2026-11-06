# Laporan Live Browser Testing — SOAP Dokter IGD: Isi Otomatis dari Perawat dan Simpan Ulang Draf (ISSUE-IGD-004)

| Metadata | Nilai |
| :--- | :--- |
| **Tanggal pengujian** | 4 November 2026 |
| **Menu** | Dokter › Instalasi Gawat Darurat › Ruang Kerja Dokter IGD › tab **Catatan Dokter** |
| **Yang diuji** | `FIX-IGD-004-01` (simpan ulang draf tidak mengosongkan S/O/A/P) dan `FIX-IGD-004-02` (S/O draf baru terisi dari nyeri dan tanda vital perawat) — [plan repair 004](../../docs/plan-repair/plan-repair-004-data-perawat-tidak-sampai-dokter-igd.md) |
| **Source yang diuji** | Frontend `RizkiV2` @ `739e05f68` + perubahan belum di-commit pada `use-emergency-progress-note.jsx`; backend `rizkiG` @ `30f1396d` |
| **Lingkungan** | Frontend `http://localhost:3000` (`next dev`); backend `https://localhost:7184`; database pengembang bersama |
| **Peramban** | Chromium build 1234, mode tampak (*headed*), `slowMo` 300 ms, layar penuh |
| **Akun** | Peran **Dokter IGD** (kredensial dari variabel lingkungan) — bukan SuperAdmin |
| **Pasien uji** | A: `IGD-261027031823-B9CDD2` — pasien uji pengembang dengan pengkajian keperawatan bernyeri (skala 4, lengan kanan bawah); encounter **tanpa** dokter. B: `IGD-261018123311-11F483` ("Pasien Uji K1-C") — encounter **dengan** dokter, tanpa data perawat |
| **Skrip** | `QuilvianSystemFrontendDev/test-with-agy/igd/uji-issue-004-soap-dokter-20261104/uji-soap-dokter.mjs` |
| **Bukti** | Folder yang sama: `screenshots/`, `hasil-*.json`, `jaringan-*.json` |
| **Hasil akhir** | **Isi otomatis S/O: 4/4 lulus** (pasien A, tiga putaran konsisten). **Simpan ulang draf: 3/3 lulus** (pasien B). Penyelesaian catatan **tidak dapat diuji** karena layar tidak punya tombol Selesaikan (temuan T-01) |

---

## 1. Ringkasan

Laporan pelapor (ISSUE-IGD-004) berisi dua keluhan pada SOAP dokter IGD. Pertama, Subjective tidak
terisi pengkajian nyeri dari perawat. Kedua, Objective tidak terisi tanda vital, dan isinya tidak
tersimpan.

Perbaikan pertama mengubah cara layar membentuk isian saat menyimpan ulang draf. Sebelumnya isian
terkirim kosong, dan server menghapus S/O/A/P. Perbaikan kedua menambahkan pengambil data perawat
per kunjungan: tanda vital terbaru, serta detail pengkajian keperawatan terbaru untuk keluhan dan
nyeri.

Hasil uji: draf SOAP baru langsung terisi keluhan, rincian nyeri, dan tanda vital pasien. Saat draf
diubah lalu disimpan ulang, keempat kolom tetap utuh di server.

---

## 2. Alur proses yang diuji

1. Dokter IGD masuk aplikasi dan membuka Ruang Kerja Dokter IGD untuk pasien uji (`?visitId=…`).
2. Dokter membuka tab **Catatan Dokter**. Layar membuat draf baru dan mengambil:
   - daftar tanda vital kunjungan (`GET /patient-vital-signs?encounterId=…`);
   - daftar pengkajian kunjungan (`GET /patient-assessments?encounterId=…`), lalu detail pengkajian
     keperawatan terbaru (`GET /patient-assessments/{id}`).
3. Subjective dan Objective draf baru terisi otomatis bila masih kosong.
4. Dokter mengisi Assessment dan Plan, lalu menekan **Simpan Catatan CPPT** (`POST /doctor-consultations`).
5. Dokter menambah teks di Objective dan menekan **Simpan Catatan CPPT** lagi
   (`PATCH /doctor-consultations/{id}/soap`).
6. Isi catatan di server diperiksa lewat query baca-saja.

---

## 3. Endpoint yang terlibat

**[Tags("Health Services / Clinical Management / Doctor Consultation")]**

| Method | Path | Deskripsi | Auth | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/clinical-management/doctor-consultations` | Membuat draf catatan dokter | Bearer; `DoctorConsultation : Create` | `encounterId`, S/O/A/P, tanda vital | `200` + id catatan |
| `PATCH` | `/api/v1/health-services/clinical-management/doctor-consultations/{id}/soap` | Menyimpan ulang S/O/A/P draf | Bearer; `DoctorConsultation : WriteSoap` | S/O/A/P; teks kosong berarti **kosongkan**, `null` berarti biarkan | `200` |
| `GET` | `/api/v1/health-services/clinical-management/doctor-consultations/encounters/{encounterId}/soap-timeline` | Riwayat catatan dokter per encounter | Bearer | — | `200` |

**[Tags("Health Services / Clinical Management / Patient Assessment")]** dan **Patient Vital Sign**

| Method | Path | Deskripsi | Auth | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/clinical-management/patient-vital-signs` | Tanda vital terbaru kunjungan | Bearer; `PatientVitalSign : Read` | `encounterId`, `sortBy=observationDateTime`, `sortDirection=desc`, `pageSize=5` | `200` + daftar |
| `GET` | `/api/v1/health-services/clinical-management/patient-assessments` | Pengkajian kunjungan | Bearer; `PatientAssessment : Read` | `encounterId`, urut terbaru, `pageSize=20` | `200` + daftar |
| `GET` | `/api/v1/health-services/clinical-management/patient-assessments/{id}` | Detail pengkajian keperawatan (rincian nyeri) | Bearer; `PatientAssessment : Read` | — | `200` |

---

## 4. Hasil per skenario

| ID | Prioritas | Skenario | Diharapkan | Hasil | Status | Bukti |
| --- | :---: | --- | --- | --- | :---: | --- |
| S1 | P1 | Pengambil data perawat berjalan | Permintaan tanda vital dan pengkajian per encounter, lalu detail pengkajian | Pasien A: ketiganya terkirim dengan `encounterId` yang benar | LULUS | `jaringan-*.json` |
| S2 | P1 | Subjective terisi keluhan dan nyeri | Memuat "Keluhan Utama", "Skala 4/10", "Lokasi: Lengan kanan bawah" | Memuat keluhan dan **enam** rincian nyeri (lokasi, kualitas, pencetus, durasi, upaya meredakan, catatan) | LULUS | `screenshots/lengkap-01-draf-baru-terisi-otomatis.png` |
| S3 | P1 | Objective terisi tanda vital | "Tekanan Darah: 112/72 mmHg", "Nadi: 94 x/menit"; kesadaran berupa label, bukan angka | TD, nadi, RR, suhu, SpO₂, "Tingkat Kesadaran: Sadar penuh" | LULUS | screenshot yang sama |
| S4 | P2 | Tombol sisip nyeri | "+ Sisipkan dari catatan perawat: Nyeri 4/10 …" tampil | Tampil | LULUS | screenshot yang sama |
| S5 | P0 | Simpan draf pertama | `POST` berhasil | Pasien B: `200` | LULUS | `screenshots/putaran-simpan-awal/02-draf-tersimpan.png` |
| S6 | P0 | Simpan ulang membawa isi | `PATCH …/soap` membawa S/O/A/P tidak kosong, termasuk teks tambahan | Panjang S 37, O 76, A 37, P 31 karakter; O memuat teks tambahan | LULUS | `jaringan-2026-11-04T06-35-08-671Z.json` |
| S7 | P0 | Isi di server utuh | Keempat kolom tidak kosong; Objective memuat teks tambahan | S 37, O 76, A 37, P 31; teks tambahan ada | LULUS | DB baca-saja |
| S8 | P1 | Pasien tanpa data perawat | S/O kosong, tanpa galat | Pasien B: S/O kosong, tanpa galat, layar tetap dapat dipakai | LULUS | `hasil-2026-11-04T06-35-08-671Z.json` |

**Catatan per skenario.**

- **S5–S7 pada pasien A selalu gagal dengan `400`**: "Dokter pemeriksa wajib diisi untuk konsultasi
  tanpa antrean." Encounter pasien A belum punya dokter. Ini aturan backend lama yang tidak disentuh
  perbaikan ini (lihat T-02). Karena itu jalur simpan diuji pada pasien B.
- **S1 pada pasien B tercatat "GAGAL" di `hasil-*.json`**, tetapi itu perilaku yang benar. Pasien B
  tidak punya dokumen keperawatan, sehingga permintaan detail pengkajian memang tidak dikirim.
  Kriteria S1 hanya berlaku untuk pasien yang punya data perawat.
- **Sumber tanda vital yang teruji adalah jalur cadangan**: pasien A tidak punya catatan tanda vital
  terpisah, sehingga Objective diambil dari tanda vital pada pengkajian keperawatan terbaru. Jalur
  utama (catatan tanda vital dari triase/observasi lebih diutamakan) belum teramati di browser.
- Pasien A diuji tiga kali dengan hasil S1–S4 identik.

---

## 5. Temuan sampingan

| No | Temuan | Prioritas | Bukti | Saran |
| ---: | --- | :---: | --- | --- |
| T-01 | **Tidak ada tombol Selesaikan** pada SOAP dokter. `SoapEditor` menerima `onComplete` tetapi tidak memasangnya ke tombol mana pun. Jalur penyelesaian (yang juga diperbaiki `FIX-01`) tidak dapat dijangkau dari layar. Berlaku juga untuk Rawat Inap yang memakai editor yang sama | P1 | `soap-editor.jsx:86`: `onComplete` hanya didestrukturisasi; `grep` tidak menemukan pemakaiannya | Issue baru; tentukan letak tombol Selesaikan bersama pemilik Rawat Inap |
| T-02 | Dokter **tidak dapat membuat SOAP** untuk pasien IGD yang encounter-nya belum punya dokter. Backend memakai `request.DoctorId ?? encounter.DoctorId`, sedangkan layar sengaja tidak mengirim `doctorId`. Komentar frontend menyebut backend menentukan dokter dari pengguna yang masuk, padahal backend tidak melakukannya | P1 | `DoctorConsultationController.cs:1478-1481`; `inpatient-progress-note-utils.jsx` komentar di atas `buildProgressNoteCreatePayload` | Issue baru; pilihan: backend memakai dokter dari sesi bila keduanya kosong, atau layar mengarahkan dokter menetapkan DPJP lebih dulu |
| T-03 | Label tombol sisip menampilkan jam saja ("Monitoring Nyeri 10.40") padahal pengkajiannya dari tanggal lain (27 Okt) | P5 | `screenshots/lengkap-01-…png` | Tampilkan tanggal bila bukan hari ini |

---

## 6. Data uji yang tercipta

Seluruh data dibuat lewat aplikasi dengan akun Dokter IGD. Query database hanya untuk membaca.

| Data | Pasien | Isi penanda |
| --- | --- | --- |
| 1 draf catatan dokter (`TrxDoctorConsultation`, status 1) | B — `IGD-261018123311-11F483` | S/O/A/P "Uji ISSUE-IGD-004 … 13:35:08" + "Uji simpan ulang ISSUE-IGD-004 …" |

Percobaan simpan pada pasien A ditolak server dan tidak membuat data.

---

## 7. Kesimpulan

- `FIX-IGD-004-02` bekerja: draf SOAP baru terisi keluhan, rincian nyeri, dan tanda vital perawat,
  dan pasien tanpa data perawat tidak menimbulkan galat.
- `FIX-IGD-004-01` bekerja untuk jalur **simpan ulang draf**: isi S/O/A/P tidak lagi terhapus. Jalur
  **Selesaikan** sudah diperbaiki di source, tetapi belum dapat diuji karena tidak ada tombolnya (T-01).
- Laporan ini adalah bukti uji pengembang, **bukan** UAT.

| Berkas bukti | Isi |
| --- | --- |
| `screenshots/lengkap-00-ruang-kerja-dokter.png` | Ruang kerja dokter, pasien A |
| `screenshots/lengkap-01-draf-baru-terisi-otomatis.png` | S/O terisi otomatis + tombol sisip nyeri |
| `screenshots/lengkap-02-draf-tersimpan.png` | Penolakan `400` pasien A (dokter encounter kosong) |
| `screenshots/putaran-simpan-awal/02-draf-tersimpan.png`, `03-simpan-ulang.png` | Pasien B: simpan dan simpan ulang |

---

## 8. Putaran ISSUE-IGD-005 — Assessment terisi dari diagnosis ICD-10 (4 November 2026, siang)

| Metadata | Nilai |
| :--- | :--- |
| **Yang diuji** | `FIX-IGD-005-01` (penanda blok Assessment aktif sejak draf awal) dan `FIX-IGD-005-02` (`assessmentOutOfSync` → tombol *Susun ulang blok diagnosa*) — [plan repair 005](../../docs/plan-repair/plan-repair-005-assessment-soap-tidak-terisi-diagnosis.md) |
| **Pasien uji** | A — `IGD-261027031823-B9CDD2`, dibuka sebagai **pasien pertama** setelah halaman dimuat (keadaan yang dilaporkan pelapor) |
| **Skrip** | `QuilvianSystemFrontendDev/test-with-agy/igd/uji-issue-005-assessment-icd-20261104/uji-assessment-icd.mjs` |
| **Data yang ditulis** | **Tidak ada.** Diagnosis pada draf hanya tertunda di layar; draf tidak disimpan |
| **Hasil** | **7 dari 7 lulus** (S7 lulus setelah pemeriksaan ulang — lihat catatan) |

### 8.1 Alur

1. Dokter membuka pasien A, lalu tab **Catatan Dokter**; draf baru langsung terbuka.
2. Dokter mencari dan menambahkan **A02.1** lalu **A20.1** lewat kotak *Cari cepat ICD-10*.
3. Dokter mengetik penilaian klinis di bawah blok, menghapus diagnosis, mengganti diagnosis utama,
   lalu menulis ulang Assessment secara manual dan memakai tombol *Susun ulang blok diagnosa*.

### 8.2 Hasil per skenario

| ID | Prioritas | Skenario | Hasil | Status | Bukti |
| --- | :---: | --- | --- | :---: | --- |
| S1 | P1 | Tambah A02.1 | Assessment: "1. A02.1 - Salmonella septicaemia (Diagnosa Utama)" | LULUS | `screenshots/01-satu-diagnosis.png` |
| S2 | P1 | Tambah A20.1 | Baris kedua "2. A20.1 - Cellulocutaneous plague (Diagnosa Tambahan)" | LULUS | `screenshots/02-dua-diagnosis.png` |
| S3 | P1 | Hapus A20.1 setelah dokter mengetik teks | Blok tinggal satu baris; teks dokter "Uji ISSUE-IGD-005: klinis sepsis…" tetap | LULUS | `hasil-*.json` |
| S4 | P2 | Jadikan A20.1 Utama | Urutan blok berubah: A20.1 Utama nomor 1, A02.1 Tambahan nomor 2; teks dokter tetap | LULUS | `screenshots/03-ganti-utama.png` |
| S5 | P2 | Assessment ditulis ulang manual | Petunjuk "belum sesuai" **tidak** tampil sebelum edit, **tampil** setelah edit, **hilang** setelah *Susun ulang*; blok kembali di awal dan teks manual dipertahankan | LULUS | `screenshots/04-petunjuk-tidak-sinkron.png`, `05-sesudah-susun-ulang.png` |
| S6 | P1 | Isi otomatis S/O perawat (`FIX-IGD-004-02`) tetap jalan | S diawali "Keluhan Utama: …", O diawali "Tekanan Darah: 112/72 mmHg" | LULUS | `hasil-*.json` |
| S7 | P0 | Tidak ada data yang ditulis | Terekam 4 × `POST …/diagnosis-recommendations/resolve` | LULUS | lihat catatan |

**Catatan S7.** Skrip menandai S7 "GAGAL" karena menangkap permintaan `POST`. Pemeriksaan source
menunjukkan endpoint itu **hanya membaca**: `DiagnosisRecommendationResolverController` bertanda
`AccessType = Read`, memakai `AsNoTracking`, dan tidak memanggil `SaveChanges`. Endpoint ini mengambil
rekomendasi planning untuk ICD-10 terpilih. Tidak ada permintaan tulis lain, jadi uji ini tidak
mengubah data.

### 8.3 Kesimpulan putaran

`FIX-IGD-005-01` dan `FIX-IGD-005-02` bekerja pada keadaan yang dilaporkan: pasien pertama yang
dibuka setelah halaman dimuat.

---

## 9. Putaran ISSUE-IGD-006 — Buka/Reset, Selesaikan Catatan, dan SOAP ke CPPT (4 November 2026, sore)

| Metadata | Nilai |
| :--- | :--- |
| **Yang diuji** | `FIX-IGD-006-01` (Buka/Reset tanpa galat), `FIX-IGD-006-02` (tombol Selesaikan Catatan, props modal, `persistPending`), `FIX-IGD-006-03` (salin SOAP ke CPPT saat selesai), perluasan `FIX-IGD-005-01` (blok ICD-10 pada draf yang dibuka ulang) — [plan repair 006](../../docs/plan-repair/plan-repair-006-buka-soap-galat-cppt-dokter-kosong.md) |
| **Pasien uji** | B — `IGD-261018123311-11F483` ("Pasien Uji K1-C"), encounter berdokter |
| **Skrip** | `QuilvianSystemFrontendDev/test-with-agy/igd/uji-issue-006-buka-selesaikan-cppt-20261104/uji-buka-selesaikan-cppt.mjs` dan `uji-buka-ulang-icd.mjs` |
| **Data yang ditulis** | Draf uji `ISSUE-IGD-004` **diselesaikan** (status 2) beserta 1 diagnosis A02.0 dan **1 entri CPPT**; **1 draf baru** "Uji ISSUE-IGD-006 … 14:29:23" berdiagnosis A02.0, dibiarkan draf. Seluruhnya lewat aplikasi, disetujui pemilik |
| **Hasil** | Putaran 1 berhenti di S3 karena modal Selesaikan tidak terbuka (ditemukan `ISS-IGD-006-T1`). Setelah diperbaiki: **11 skenario, seluruhnya sesuai harapan** (S3 dan S4 dinilai ulang — lihat catatan) |

### 9.1 Hasil per skenario

| ID | Prioritas | Skenario | Hasil | Status |
| --- | :---: | --- | --- | :---: |
| S1 | P0 | **Reset** pada draf baru | Tanpa galat; editor tetap tampil | LULUS |
| S2 | P0 | Riwayat → **Buka** draf tersimpan | Tanpa galat; Form SOAP terbuka berisi "Uji ISSUE-IGD-004 subjective 13:35:08" | LULUS |
| S3 | P1 | Selesaikan tanpa diagnosis | Tombol tampil; modal terbuka; server menolak (`PATCH …/complete` **400**); catatan tetap draf; isi S/O utuh (37/76 karakter) | LULUS (dinilai ulang) |
| S4 | P1 | Lanjutkan draf: tambah A02.0, ubah Plan, Simpan | `POST /patient-diagnoses` 200; `PATCH …/soap` 200. Putaran awal: blok ICD-10 **tidak** masuk Assessment draf lama (`ISS-IGD-006-T3`) | DIPERBAIKI → lihat S9 |
| S5 | P0 | **Selesaikan Catatan** dengan diagnosis | `PATCH …/complete` **200**; status catatan 1 → 2 | LULUS |
| S6 | P1 | Salin ke CPPT | `POST …/from-consultation/{id}` **200**; notifikasi "…dan tampil di CPPT." | LULUS |
| S7 | P1 | CPPT Pasien | SOAP dokter tampil di layar; baris CPPT untuk konsultasi ini 0 → 1 | LULUS |
| S8 | P0 | Galat halaman selama putaran | 0 | LULUS |
| S9 | P1 | Draf baru → simpan → **Buka** → tambah A02.0 | Assessment: "1. A02.0 - Salmonella enteritis (Diagnosa Utama)" di atas "Uji ISSUE-IGD-006 penilaian awal 14:29:23" | LULUS |
| S10 | P1 | Simpan draf yang dibuka ulang | `PATCH …/soap` 200; Assessment di DB diawali blok; 1 diagnosis tersimpan untuk konsultasi itu | LULUS |
| S11 | P0 | Galat halaman | 0 | LULUS |

**Catatan S3.** Skrip mengharapkan penolakan terjadi di layar tanpa permintaan ke server. Ternyata
layar meneruskan permintaan, lalu server menolaknya dengan `400`. Hasil yang penting tetap terpenuhi:
catatan tidak selesai dan isinya tidak hilang.

### 9.2 Temuan sampingan

| No | Temuan | Prioritas | Bukti |
| ---: | --- | :---: | --- |
| T-04 | Modal Koreksi (addendum) dan modal Salin dari catatan sebelumnya tidak dapat terbuka (props salah); payload addendum IGD `{ notes }` tidak sesuai kontrak `{ addendumText, correctionReason }` | P1 | `ISS-IGD-006-T4` |
| T-05 | Kepala CPPT dokter IGD menampilkan "0 catatan" dan "No. RM -" walau CPPT berisi (props `DoctorCpptHeader` salah) | P4 | `screenshots/06-cppt-pasien.png`; `ISS-IGD-006-T5` |

### 9.3 Satu tombol simpan (`FIX-IGD-006-04`)

Pemilik memutuskan IGD hanya memakai **satu** tombol. "Simpan Catatan CPPT" kini langsung menyimpan,
menandatangani, dan memasukkan catatan ke CPPT setelah konfirmasi. Skrip:
`uji-simpan-selesaikan-satu-klik.mjs`.

| ID | Skenario | Hasil | Status |
| --- | --- | --- | :---: |
| B1 | Hanya satu tombol simpan | Label "Simpan Catatan CPPT"; bilah kedua tidak ada; 1 tombol "Simpan" tampil | LULUS |
| B2 | Konfirmasi menjelaskan akibatnya | "Catatan akan disimpan, ditandatangani, dan langsung tampil di CPPT pasien. Menekan Selesaikan menandatangani…" | LULUS |
| B3 | Draf **baru** → satu tombol, tanpa simpan sebelumnya | `POST /doctor-consultations` 200 → `POST /patient-diagnoses` 200 → `PATCH …/complete` 200 → `POST …/from-consultation` 200; status 2; 1 diagnosis; 1 baris CPPT | LULUS |
| B4 | CPPT Pasien | Catatan tampil | LULUS |
| B5 | Galat halaman | 0 | LULUS |

Pada putaran opsi B sebelumnya (tombol terpisah "Simpan & Selesaikan"), jalur **draf lama** lewat
Buka juga lulus: `PATCH …/soap` 200 → `PATCH …/complete` 200 → `POST …/from-consultation` 200,
perubahan Plan tersimpan, dan 1 baris CPPT. Jalur ini memakai fungsi yang sama dengan tombol tunggal.

Percobaan pertama opsi B ditolak server (`400` "Masih ada draf catatan dokter yang belum diselesaikan
untuk kunjungan IGD ini.") karena pasien uji masih punya draf terbuka dari uji sebelumnya. Ini aturan
server, bukan cacat perbaikan, tetapi menunjukkan `ISS-IGD-006-T6`.

## 10. Putaran ISSUE-IGD-007 — Tanpa draf, detail catatan selesai, dan pindah segmen (4 November 2026, sore)

| Metadata | Nilai |
| :--- | :--- |
| **Yang diuji** | `FIX-IGD-007-01`, `FIX-IGD-007-04` — [plan repair 007](../../docs/plan-repair/plan-repair-007-detail-soap-kosong-penanda-draf.md) |
| **Skrip** | `QuilvianSystemFrontendDev/test-with-agy/igd/uji-issue-007-detail-tanpa-draf-20261104/uji-detail-tanpa-draf.mjs` |
| **Pasien uji** | K1-C; catatan selesai yang dibuka: `14c912b8…` |
| **Data yang ditulis** | **Tidak ada** (jumlah catatan dokter 5 → 5). Regresi jalur sukses satu tombol (`uji-simpan-selesaikan-satu-klik.mjs`) menambah 1 catatan selesai + 1 CPPT |
| **Hasil** | **7/7 lulus**; regresi **5/5 lulus** |

| ID | Skenario | Hasil | Status |
| --- | --- | --- | :---: |
| T1 | S/O/A/P terisi tanpa diagnosis → Simpan Catatan CPPT | Konfirmasi **tidak** muncul; **0** permintaan tulis; pesan merah "Catatan belum disimpan. Lengkapi dulu: Silakan pilih minimal satu diagnosa ICD-10…"; isian tetap | LULUS |
| T2 | Riwayat → Buka catatan selesai | "Dokumen asli" (diagnosis A02.0, S/O/A/P) + Riwayat Koreksi + bilah "Catatan sudah tersimpan di CPPT dan terkunci sejak 04 Nov 2026, 13.35." dengan **Kembali ke Riwayat** dan **Catatan Baru** | LULUS |
| T3 | Kembali ke Riwayat | Segmen Riwayat aktif; daftar dimuat ulang (1 × `GET …/soap-timeline`) | LULUS |
| T4 | Buka catatan selesai → segmen Riwayat → segmen Form SOAP | Form baru tampil; detail lama tidak lagi tampil | LULUS |
| T5 | Detail → Catatan Baru | Form baru tampil | LULUS |
| T6 | Riwayat → Refresh, lalu Buat SOAP Baru | Refresh memuat ulang daftar; Buat SOAP Baru membuka form baru | LULUS |
| T7 | Galat halaman dan data baru | 0 galat; 0 catatan baru | LULUS |

Bukti: `screenshots/01-tanpa-draf-ditahan.png`, `02-detail-catatan-selesai.png`,
`03-pindah-segmen-form-baru.png`, `04-buat-soap-baru-dari-riwayat.png`.

| Berkas bukti (folder skrip §9) | Isi |
| --- | --- |
| `screenshots/02-draf-terbuka.png` | Draf terbuka lewat Buka |
| `screenshots/03-tombol-selesaikan.png` | Bilah **Selesaikan Catatan** |
| `screenshots/05-sesudah-selesaikan.png` | Sesudah selesai |
| `screenshots/06-cppt-pasien.png` | Notifikasi + CPPT |
| `screenshots/07-buka-ulang-icd-masuk-assessment.png` | Blok ICD-10 pada draf yang dibuka ulang |
| `screenshots/12-satu-tombol.png` | Satu tombol "Simpan Catatan CPPT" |
| `screenshots/13-modal-satu-tombol.png` | Konfirmasi simpan + tanda tangan + CPPT |
| `screenshots/15-cppt-satu-tombol.png` | CPPT setelah satu tombol | Penyimpanan draf berisi blok diagnosis tidak diulang di putaran ini.
Jalur simpan ulang sudah terbukti pada putaran `ISSUE-IGD-004` (S5–S7), dan diagnosis draf
tersimpan bersama catatan lewat jalur yang sama.

## 11. Putaran K-02 dan K-01 — Koreksi/Salin SOAP Dokter dan Tanda Vital Pengkajian Medis (5 November 2026)

| Metadata | Nilai |
| :--- | :--- |
| **Yang diuji** | `FIX-IGD-007-03` (Koreksi/addendum dan Salin pada SOAP dokter IGD — [plan repair 007](../../docs/plan-repair/plan-repair-007-detail-soap-kosong-penanda-draf.md)) dan `FIX-IGD-004-03` (tanda vital Pengkajian Medis tersimpan dan terbaca — [plan repair 004](../../docs/plan-repair/plan-repair-004-data-perawat-tidak-sampai-dokter-igd.md)) |
| **Keputusan pemilik** | K-02 (plan 007) dan K-01 (plan 004) disetujui lisan Rizki Gunawan, 5 Nov 2026 |
| **Source yang diuji** | Frontend `RizkiV2` @ `91a11b08f` + perubahan belum di-commit: `emergency-doctor-progress-tab.jsx`, `use-emergency-progress-note.jsx`, `inpatient-medical-assessment-utils.jsx` (komponen bersama Rawat Inap). Backend `rizkiG` @ `30f1396d`, berjalan apa adanya (tidak dibangun ulang) |
| **Akun** | Peran **Dokter IGD** (dr. Jenah Ali, kredensial dari variabel lingkungan) — bukan SuperAdmin |
| **Skrip** | `QuilvianSystemFrontendDev/test-with-agy/igd/uji-fix-007-03-koreksi-salin-20261105/uji-koreksi-salin.mjs` dan `…/uji-fix-004-03-tanda-vital-kajian-medis-20261105/uji-tanda-vital-kajian-medis.mjs` |
| **Bukti** | Folder skrip masing-masing: `screenshots/`, `hasil-*.json`; untuk V1–V5 juga `log-konsol-putaran-1-igd.txt` (lihat catatan di §11.4) |
| **Validasi statis** | `npx eslint` ketiga berkas source: 0 masalah. Unit test: berkas baru `tests/unit/emergency-medical-assessment-vitals.test.mjs` 9/9, ditambah uji Kajian Medis Rawat Inap yang sudah ada 29/29 (total 38/38 lulus) |
| **Hasil** | **Koreksi/Salin: 7/7 lulus.** **Tanda vital IGD: 5/5 lulus.** **Rawat Inap di browser: tidak diuji** — akun Dokter IGD tidak punya konteks dokter Rawat Inap, dan pemilik memutuskan uji Rawat Inap di luar ranah IGD (§11.5, T-11c) |

### 11.1 Alur yang diuji

**Koreksi catatan dokter yang sudah selesai (addendum).** Contoh: dr. Jenah Ali menyadari dosis pada
Plan catatan SOAP kemarin salah ketik.

1. Catatan Dokter → Riwayat SOAP → **Buka** catatan selesai `CON-20261104-00001`.
2. Layar bertanya ke server apakah dokter ini boleh mengoreksi. Karena ia penulis catatan itu, tombol
   **Koreksi** tampil di bilah bawah, di samping **Kembali ke Riwayat** dan **Catatan Baru**.
3. **Koreksi** → jendela "Koreksi catatan SOAP" terbuka. **Simpan Koreksi** baru aktif bila *Isi
   Koreksi* dan *Alasan Koreksi* keduanya terisi.
4. **Simpan Koreksi** → koreksi tersimpan sebagai catatan tambahan bernomor urut. Isi asli catatan tidak
   berubah. Riwayat Koreksi berubah dari "0 Koreksi" menjadi "1 Koreksi" dan teks koreksi tampil.

**Salin dari catatan sebelumnya.** Contoh: pada catatan baru, dokter ingin memakai lagi Assessment
dan Plan dari catatan final terakhir.

1. Catatan Baru → tombol **Salin A & P dari SOAP 04/11, 15.03** tampil.
2. Assessment/Plan sudah diketik → jendela "Timpa Assessment dan Plan?" menyebut sumbernya
   (`CON-20261104-00006`). **Batal** tidak mengubah apa pun.
3. **Salin dan Timpa** → Assessment, Plan, dan diagnosis sumber tersalin. Penanda "Disalin dari SOAP"
   tampil. Belum ada yang disimpan ke server.

**Tanda vital Pengkajian Medis.** Contoh: dr. Jenah Ali mengisi kajian medis pasien uji K1-A.

1. Pengkajian Medis → **Tambah Kajian** → bagian "2. Tanda Vital & Kondisi Umum" berisi TD, Nadi, RR,
   Suhu, dan SpO₂.
2. TD ditulis "120" (tanpa diastolik) → **Simpan Draft** → layar menolak dengan pesan jelas. Tidak ada
   yang dikirim ke server.
3. **Sinkronkan Tanda Vital dari Perawat** → kelima isian terisi dari pengkajian perawat → **Simpan
   Draft** → kajian baru tersimpan beserta tanda vitalnya → dibuka ulang, angkanya tetap ada.
4. TD diubah manual menjadi 118/76, suhu ditulis "37,2" (koma), SpO₂ dikosongkan → **Simpan Draft** →
   dibuka ulang: 118/76, 37.2, SpO₂ kosong (tersimpan `null`, bukan 0). Nadi dan RR yang tidak disunting
   tetap.

### 11.2 Endpoint yang terlibat

`[Tags("Health Services / Medical Record Management / Clinical Note Addendum")]` — basis
`/api/v1/health-services/medical-record-management/clinical-note-addendums`

| Method | Path | Deskripsi | Auth | Request / Response |
| --- | --- | --- | --- | --- |
| GET | `/authority/{documentKind}/{documentId}` | Apakah pengguna boleh mengoreksi catatan ini (penulis asli atau pengganti sah) | Bearer; `ClinicalNoteAddendum:Read` | — / `{ isAllowed, isOriginalAuthor, isSubstituteAuthor, explanation }` |
| GET | `/by-document/{documentKind}/{documentId}` | Daftar koreksi pada satu catatan | Bearer; `ClinicalNoteAddendum:Read` | — / daftar `{ sequence, authorName, addendumText, correctionReason, signedAt }` |
| POST | `/by-document/{documentKind}/{documentId}` | Tambah koreksi (addendum) | Bearer; `ClinicalNoteAddendum:Create` | `{ addendumText, correctionReason }` / 201 + addendum tersimpan; 400 "Isi koreksi wajib diisi." bila `addendumText` kosong |

`[Tags("Health Services / Clinical Management / Patient Assessment")]` — basis
`/api/v1/health-services/clinical-management/patient-assessments`

| Method | Path | Deskripsi | Auth | Request / Response |
| --- | --- | --- | --- | --- |
| POST | `/` | Buat kajian medis | Bearer | `{ encounterId, assessmentType, …teks…, bloodPressureSystolic, bloodPressureDiastolic, pulseRate, respiratoryRate, temperature, oxygenSaturation }` / 201 |
| PUT | `/{id}` | Simpan ulang draf (mengganti seluruh baris) | Bearer | Sama dengan POST + kolom tanda vital lain dari detail / 200 |
| GET | `/{id}` | Detail kajian, termasuk kolom tanda vital | Bearer | — / detail |

### 11.3 Hasil — Koreksi dan Salin (pasien uji K1-C)

| ID | Skenario | Hasil | Status |
| --- | --- | --- | :---: |
| K1 | Buka catatan selesai milik dokter | Tombol **Koreksi** tampil; Riwayat Koreksi "0 Koreksi" | LULUS |
| K2 | Koreksi → jendela terbuka; isi kosong; Batal | Judul "Koreksi catatan SOAP"; Simpan Koreksi nonaktif saat kosong; Batal menutup jendela; 0 permintaan tulis | LULUS |
| K3 | Isi koreksi tanpa alasan | Simpan Koreksi tetap nonaktif | LULUS |
| K4 | Isi + alasan → Simpan Koreksi | `POST …/by-document/…` **201** dengan isi `{ addendumText, correctionReason }`; jendela tertutup; "0 Koreksi" → **"1 Koreksi"**; teks koreksi tampil; database 0 → 1 addendum, `AddendumCount` = 1 | LULUS |
| S1 | Catatan baru, A/P terisi → Salin → Batal | Tombol "Salin A & P dari SOAP 04/11, 15.03"; jendela "Timpa Assessment dan Plan?" menyebut sumber `CON-20261104-00006`; Batal tidak mengubah isian | LULUS |
| S2 | Salin → **Salin dan Timpa** | Assessment dan Plan sama dengan catatan sumber; penanda "Disalin dari SOAP" tampil; tidak ada permintaan simpan (hanya `POST …/diagnosis-recommendations/resolve`, penghitung rekomendasi berizin Read) | LULUS |
| Z | Galat halaman dan data SOAP baru | 0 galat; 0 respons 5xx; catatan dokter 6 → 6 | LULUS |

Bukti: `screenshots/01-tombol-koreksi-tampil.png`, `02-modal-koreksi-terbuka.png`,
`03-modal-koreksi-terisi.png`, `04-riwayat-koreksi-bertambah.png`, `05-modal-salin-terbuka.png`,
`06-salin-dan-timpa-berhasil.png`. K1–K4 dari `hasil-2026-11-05T02-55-30-567Z.json`; S1–S2 dari
`hasil-2026-11-05T02-57-27-463Z.json` (putaran Salin-saja, supaya tidak menambah addendum kedua). Pada
putaran pertama S2 tercatat GAGAL karena pembanding skrip terlalu ketat terhadap baris kosong (§11.5,
T-11b); isinya sendiri sama.

### 11.4 Hasil — Tanda vital Pengkajian Medis

| ID | Skenario | Hasil | Status |
| --- | --- | --- | :---: |
| V1 | IGD (K1-A): TD "120" → Simpan Draft | Pesan "Tanda vital belum dapat disimpan. Tekanan Darah (TD) "120" tidak sah. Tulis sistolik/diastolik, misalnya 120/80."; 0 permintaan ke `patient-assessments` | LULUS |
| V2 | Nadi "9O" (huruf O) | Pesan "Frekuensi Nadi (HR) "9O" tidak sah. Isi dengan bilangan bulat, misalnya 84."; 0 permintaan | LULUS |
| V3 | Sinkron dari perawat → Simpan Draft → buka ulang | Isian 120/80, 82, 20, 36.6, 98; `POST` **201** dengan angka yang sama; database `ASM-20261105-00001` 120/80/82/20/36.60/98.00; dibuka ulang utuh | LULUS |
| V4 | TD manual 118/76, suhu "37,2", SpO₂ dikosongkan → Simpan → buka ulang | `PUT` **200** dengan 118/76, 37.2, `oxygenSaturation: null`; database 118/76/82/20/37.20/NULL; dibuka ulang "118/76", "37.2", SpO₂ kosong; nadi 82 tetap | LULUS |
| V5 | Kajian lama tanpa tanda vital (`ASM-20261020-00001`, Pasien Uji Jenah Ali) | Terbuka baca-saja, TD "- mmHg", 0 galat | LULUS |
| R0 | Ruang Kerja Dokter Rawat Inap (Pasien Uji P1-A 7049) | Panel kiri: "Konteks dokter tidak tersedia — Akun ini tidak memiliki identitas dokter yang terdaftar atau izin membaca census"; tab Kajian Pasien tidak dapat dibuka | TIDAK DAPAT DIUJI |

Bukti: `screenshots/01-igd-tab-pengkajian-medis.png` sampai `07-igd-kajian-lama-tanpa-tanda-vital.png`,
`08-rwi-workspace-tidak-terbuka.png`. **Catatan bukti V1–V5:** skrip putaran pertama berhenti di bagian
Rawat Inap sebelum menulis `hasil-*.json`, sehingga keluaran konsol V1–V5 disalin apa adanya ke
`log-konsol-putaran-1-igd.txt` bersama potret database (SELECT). `hasil-2026-11-05T03-03-46-665Z.json`
memuat R0.

**Rawat Inap (AC 4) tidak diuji di browser** — di luar ranah IGD (keputusan Rizki Gunawan, 5 Nov 2026). Yang sudah ada: berkas yang diubah dipakai bersama
oleh kedua ruang kerja lewat hook yang sama (`use-inpatient-medical-assessment.jsx`), uji unit Kajian
Medis Rawat Inap tetap 29/29 lulus, dan uji unit baru membuktikan pemetaan buat/ubah/baca. Verifikasi
layar Rawat Inap menjadi urusan pemilik Rawat Inap.

### 11.5 Temuan sampingan

| ID | Temuan | Keparahan | Tindak lanjut |
| --- | --- | --- | --- |
| T-11a | Pada jendela Koreksi, **Batal** (dan tutup lewat Esc) ikut nonaktif selama *Isi Koreksi* kosong. Sebabnya `CorrectionModal` meneruskan `disabled={!addendumText.trim()}` ke `ConfirmModal`, yang memakai `disabled` juga untuk keadaan sibuk (`isBusy`). Dokter yang membuka Koreksi lalu berubah pikiran harus mengetik sesuatu dulu agar bisa menutup jendela. Berlaku juga di Rawat Inap (komponen yang sama) | Low | Tidak diperbaiki oleh IGD — `correction-modal.jsx` milik Rawat Inap; keputusan pemilik: ranah IGD saja. Dicatat sebagai `ISS-IGD-006-T7` untuk informasi pemilik Rawat Inap |
| T-11b | Setelah **Salin dan Timpa**, Assessment memuat baris kosong antara blok ICD-10 dan teks, sedangkan catatan sumber tersimpan dengan satu baris baru. Penyusunan ulang blok ICD dilakukan util bersama `replaceGeneratedPrefix` (`blok + "\n\n" + teks`), sama dengan Rawat Inap | Info | Bukan cacat; isi tersalin utuh |
| T-11c | Akun Dokter IGD tidak dapat membuka Ruang Kerja Dokter Rawat Inap (tidak terdaftar di sensus), sehingga AC 4 `FIX-IGD-004-03` tidak dapat diuji di browser | — | Tidak dikejar — uji Rawat Inap di luar ranah IGD (keputusan pemilik) |
| T-11d | Suite unit IGD `tests/unit/emergency-*.test.mjs`: 143/146 lulus. Tiga yang gagal (`buildSubjectivePrefill …`, dua `FE-IGD-017 K1 …`) menguji `emergency-assessment-soap.utils.js` dan `emergency-assessment-transfer-tab.jsx`, yang tidak tersentuh putaran ini | — | `UNRELATED EXISTING ISSUE` |

### 11.6 Data uji yang tercipta

| Data | Pasien | Isi |
| --- | --- | --- |
| 1 addendum (Koreksi ke-1) | K1-C, catatan `CON-20261104-00001` (`14c912b8…`) | "Uji FIX-IGD-007-03: koreksi dosis pada Plan (2026-11-05T02-55-30-567Z)."; alasan "Uji otomatis FIX-IGD-007-03 — salah ketik dosis."; penulis dr. Jenah Ali |
| 1 kajian medis (draf, status *In progress*) | K1-A (`IGD-261018123252-672191`) | `ASM-20261105-00001`; TD 118/76, nadi 82, RR 20, suhu 37,2, SpO₂ kosong; keadaan umum "Sedang", kesadaran "Compos Mentis" (diisi tombol sinkron) |

Tidak ada catatan SOAP atau CPPT baru. Bagian Rawat Inap tidak menulis apa pun.

### 11.7 Kesimpulan putaran

- `FIX-IGD-007-03`: AC 1 (Koreksi tersimpan, "1 Koreksi") dan AC 2 (Salin dan Timpa) **terverifikasi
  di browser**. Koreksi addendum dokter IGD kini benar-benar sampai ke server dengan isi dan alasannya.
- `FIX-IGD-004-03`: AC 1, 2, 3, dan 5 **terverifikasi di browser IGD**. AC 4 (Rawat Inap) hanya
  lewat unit test; uji browser Rawat Inap di luar ranah IGD (keputusan pemilik).
- Laporan task tracked belum ada, sehingga status kedua perbaikan tetap 🟡 SEBAGIAN.
