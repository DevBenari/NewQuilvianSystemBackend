# Permintaan Penunjukan Pemilik Modul — Blokir Modul IGD

| Field | Value |
|---|---|
| `request_id` | `IGD-REQ-001` |
| `tanggal` | 2026-08-24 |
| `pengaju` | Rizki Gunawan — Product/Domain Owner IGD (`IGD-DEC-089`) |
| `rujukan` | `blueprint-manifest.md` revision `5` bagian 4; `04-prd-to-mvp.md` bagian 7 |
| `status` | `dikirim` / `menunggu jawaban` |
| `sifat` | Operasional. **Bukan** artefak desain — tidak masuk daftar hash manifest |

Dokumen ini dapat diteruskan apa adanya. Setiap bagian berdiri sendiri: penerima cukup
membaca bagian yang menyebut namanya.

---

## 1. Satu paragraf untuk yang tidak punya waktu

Modul IGD sudah punya blueprint lengkap (revision `5`, 24 Agustus 2026) dan pemilik
Product/Domain sejak hari yang sama. **Pekerjaan tetap tidak bisa jalan penuh** karena
sebagian tabel yang harus disentuh IGD bukan milik IGD, dan modul pemiliknya belum punya
orang yang berwenang menyetujui. Yang diminta bukan persetujuan teknis, melainkan
**penunjukan nama** — satu orang per modul yang berhak berkata ya atau tidak.

---

## 2. Prioritas 1 — memblokir kemampuan inti IGD

### 2.1 Pemilik `ClinicalManagement`

| Butir | Isi |
|---|---|
| **Yang diminta** | Penunjukan satu nama sebagai Product/Domain Owner `ClinicalManagement` |
| **Yang diblokir** | `EPIC IGD-09` — pengkajian, diagnosis, tindakan, dan tanda vital pasien IGD |
| **Akibat nyata** | Hasil pengkajian pasien IGD **tidak dapat disimpan**. Layar pengkajian yang sudah jadi (`FE-IGD-011`) menulis ke tabel milik modul ini |
| **Keputusan yang menunggu** | `IGD-DEC-068`, `IGD-DEC-080` |
| **Tabel terdampak** | `TrxPatientAssessment`, `TrxPatientDiagnosis`, `TrxPatientProcedure`, `TrxPatientVitalSign`, `TrxPatientIntegratedProgressNote`, `TrxDoctorConsultation` |

### 2.2 Pemilik `PharmacyManagement`

| Butir | Isi |
|---|---|
| **Yang diminta** | Penunjukan satu nama sebagai Product/Domain Owner `PharmacyManagement` |
| **Yang diblokir** | `EPIC IGD-09` bagian resep; catatan pemberian obat pada `POST-MVP` |
| **Akibat nyata** | Resep yang ditulis dokter IGD tidak punya jalur tersimpan yang disetujui pemiliknya |
| **Keputusan yang menunggu** | `IGD-DEC-068`, `IGD-DEC-078` |
| **Tabel terdampak** | `TrxPrescription` |

> Keduanya bersama-sama memblokir **satu** epic. Selama belum ada nama, `EPIC IGD-09` tetap
> berstatus `OPEN DECISION` dan tidak masuk gelombang MVP mana pun.

---

## 3. Prioritas 2 — memblokir gelombang `MVP-5`

### 3.1 Security/Privacy owner

| Butir | Isi |
|---|---|
| **Yang diminta** | Penunjukan nama, lalu jawaban atas `IGD-OQ-071` |
| **Pertanyaannya** | Unit layanan yang belum dipetakan ke simpul organisasi: tolak semua pengguna (pelayanan berhenti) atau izinkan semua pengguna (penjagaan hilang)? |
| **Yang diblokir** | `EPIC IGD-08` — kewenangan unit; gelombang `MVP-5` |
| **Keputusan yang menunggu** | `IGD-DEC-080`, `IGD-DEC-081`, `IGD-DEC-086` |

### 3.2 Pemilik Master Data

| Butir | Isi |
|---|---|
| **Yang diminta** | Penunjukan nama; pengisian pemetaan unit layanan ke simpul organisasi |
| **Yang diblokir** | `MVP-5`. Sebagian `MVP-0` (pengisian master kelas pasien IGD) |
| **Keputusan yang menunggu** | `IGD-DEC-086` |

---

## 4. Prioritas 3 — belum memblokir gelombang aktif, tetapi akan

| Peran | Keputusan yang menunggu | Akan memblokir |
|---|---|---|
| Registration API owner | `IGD-DEC-074`, `075`, `084` | `MVP-1` |
| Pemilik `LaboratoryManagement` | `IGD-DEC-087` | `POST-MVP` |
| Finance owner | `IGD-DEC-076`, `077` | Penagihan kunjungan IGD |
| Clinical Governance | Sebelas keputusan klinis | Nilai batas waktu triase dan pengkajian ulang |
| Nursing authority | Lima keputusan keperawatan | Sama dengan di atas |
| Integration owner | `IGD-DEC-085` | `MVP-3` |
| Pemilik Corporate/HR | `IGD-DEC-081`, `086` | `MVP-5` |

Nilai batas waktu yang menunggu Clinical Governance **tidak memblokir kode**. Kode ditulis
dengan nilai yang dapat dikonfigurasi; yang menunggu adalah angkanya, bukan mekanismenya.

---

## 5. Untuk Muhammad Hamzah — Product/Domain Owner Rawat Inap

Bagian ini bersifat timbal balik: masing-masing pihak memegang jawaban yang ditunggu pihak lain.

### 5.1 Yang IGD tunggu dari Rawat Inap

| Butir | Isi |
|---|---|
| Persetujuan revisi `RWI-RULE-026` dan bagian `compatibility_impact` | Blueprint IGD revision `5` membatalkan janji "nol perubahan kolom pada tabel modul lain" lewat `IGD-DEC-075`. Rawat Inap ikut terdampak |
| Persetujuan `IGD-DEC-069`, `071`, `073`, `075`, `077`, `078` | Seluruh urusan tempat tidur berpindah ke Rawat Inap |

### 5.2 Yang Rawat Inap tunggu dari IGD — **sudah dapat dijawab**

`RWI-OQ-034` dan `DEC-INP-002` berbunyi: *"Apakah pemilik `EmergencyInstallationManagement`
menyetujui bahwa disposisi `RANAP` menutup kunjungan IGD dan membuat kunjungan rawat inap
baru, serta menyetujui penanda `ClosesEmergencyVisit` mulai benar-benar dijalankan?"*

Keduanya berstatus `OPEN` dengan keterangan "pemilik belum ditunjuk", dan slice `INP-S09`
milik Rawat Inap **berhenti** karenanya. Sejak `IGD-DEC-089` pada 24 Agustus 2026, pemiliknya
ada: **Rizki Gunawan**, dan `IGD-DEC-067` adalah jawabannya.

**Slice `INP-S09` dapat berjalan kembali begitu persetujuan `IGD-DEC-067` dicatat.**

---

## 6. Bentuk jawaban yang diterima

Satu baris per penunjukan, tertulis, dengan nama dan tanggal. Contoh:

> Pemilik `ClinicalManagement` ditunjuk: **<nama>**, berlaku sejak **<tanggal>**.

Jawaban dicatat sebagai keputusan baru pada `00-interview-decisions.md` dan mengubah baris
`owners` pada `blueprint-manifest.md`. Tanpa nama tertulis, butir 10 Definition of Done
(`04-prd-to-mvp.md` bagian 6) **tidak dapat dijawab "ya"** untuk gelombang mana pun yang
menyentuh modul tersebut.

---

## 7. Yang tetap berjalan tanpa jawaban apa pun

Agar jelas bahwa permintaan ini bukan penghentian pekerjaan:

| Berjalan sekarang | Alasan |
|---|---|
| Gelombang `MVP-0` | Tidak bergantung pada satu pun peran di atas |
| `EPIC IGD-03` — perbaikan status kunjungan yang dapat mundur | Perbaikan cacat murni pada tabel milik IGD |
| Pelayanan klinis darurat | **Tidak pernah** diblokir gerbang mana pun (`IGD-DEC-086` butir 7) |
