# Laporan Analisis & Rencana Kerja Modernisasi Menu: Monitoring Nyeri (Pain Monitoring & Management)

## 1. Ringkasan Eksekutif & Identitas Dokumen

| Atribut | Keterangan |
| :--- | :--- |
| **Area / Modul** | Pelayanan Kesehatan (*Health Services*) — Rawat Inap Keperawatan (*Inpatient Nursing Workspace*) |
| **Menu Sasaran** | Asuhan Keperawatan → Pengkajian Pasien → Sub-Menu: **Monitoring Nyeri (*Pain Monitoring & Management*)** |
| **Jalur Dokumen** | `docs/module-blueprints/rawat-inap/keperawatan/roadmap/rencana-kerja/asuhan-keperawatan/monitoring-nyeri/monitoring-nyeri.md` |
| **Dasar Penyelarasan** | Mengadopsi parameter operasional teruji lapangan dari **QuilvianV1** (sesuai capture formulir operasional `01-form-monitoring-nyeri.png` dan `02-riwayat-monitoring-nyeri.png`), menyelaraskan dengan arsitektur modern **QuilvianFinal**, serta menambahkan inovasi cerdas keselamatan klinis standar akreditasi rumah sakit. |
| **Standar Keselamatan & Regulasi** | **KARS / STARKES (Standar Akreditasi Rumah Sakit)**: Bab **HPK (Hak Pasien dan Keluarga)** & **PAP (Pelayanan dan Asuhan Pasien)** tentang Manajemen Nyeri Terintegrasi, serta **SKP (Sasaran Keselamatan Pasien)** pencegahan komplikasi overdosis sedasi opioid (*Pasero Opioid-Induced Sedation Scale* / POSS). |
| **Prinsip Data** | **Zero Breaking Migration** — Memanfaatkan instrumen klinis berversi `CliAssessmentInstrumentResponse` (`PAIN_MONITORING`), sinkronisasi otomatis ke kolom ringkasan `TrxPatientAssessment` (`HasPain`, `PainAssessmentState`, `PainScale`, `PainLocation`, `PainTrigger`, `PainFrequency`, `PainManagement`, `PainNote`, `PainReassessmentDueAt`), dan integrasi data tanda vital rujukan. |

---

## 2. Analisis Mendalam: Audit Kesenjangan V1 vs Final (Status Implementasi)

Berdasarkan perbandingan langsung antara formulir operasional **QuilvianV1** (dari berkas tangkapan layar `01-form-monitoring-nyeri.png` dan `02-riwayat-monitoring-nyeri.png` serta model `MonitoringNyeri.cs`) terhadap kondisi sistem saat ini di **QuilvianFinal**:

### 2.1. Matriks Status Implementasi Fitur & Parameter

| No | Komponen / Parameter | Kondisi di QuilvianV1 (Operasional) | Kondisi Saat Ini di QuilvianFinal | Status Implementasi | Catatan & Kesenjangan |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | **Status Evaluasi Nyeri Cepat (Triase Nyeri)** | Belum ada pemisahan awal; perawat langsung memilih dropdown skor nyeri | Sudah ada pilihan 3 status cepat: `Tidak Nyeri`, `Ada Nyeri`, `Tidak Dapat Dinilai` (`PAIN_STATE` / `VAL-KEP-22a`) | **SUDAH DITERAPKAN (BACKEND & SEEDER)** | Pilihan cepat memudahkan perawat triase awal tanpa harus mengisi seluruh parameter jika pasien tidak nyeri |
| 2 | **Metode / Alat Ukur Klinis Sesuai Usia & Kondisi** | Hanya dropdown skala pain tunggal umum tanpa pengkategorian alat ukur | Memiliki 4 alat ukur klinis terstandar: `NRS` (dewasa sadar), `Wong-Baker FACES` (anak & geriatri), `CPOT` (ICU/koma/ventilator), `FLACC` (bayi/anak < 3 th) | **SUDAH TERSEDIA (SEEDER)** | Perlu selektor visual interaktif (slider & visual wajah Wong-Baker) di antarmuka |
| 3 | **Skala Intensitas Nyeri (Skor 0–10)** | Dropdown teks biasa `0` s.d. `10` | Input angka `PAIN_SCALE` di seeder, namun di UI masih berupa kontrol dasar | **SEBAGIAN DITERAPKAN** | Tampilan perlu diubah menjadi visual skala warna gradasi (Hijau 0, Kuning 1–3, Oranye 4–6, Merah 7–10) |
| 4 | **Skor Sedasi POSS (Pasero Opioid-Induced Sedation Scale)** | Input angka polos `Skor Sedasi: 0` tanpa interpretasi keselamatan | Menggunakan skala POSS terstandar (POSS 0 s.d. POSS 4) lengkap dengan deskripsi klinis dan peringatan depresi napas | **SUDAH TERSEDIA (SEEDER)** | Banner keselamatan klinis untuk skor POSS 3 dan 4 sudah ada, perlu penataan kartu visual yang lebih kontras |
| 5 | **Pengkajian Karakteristik Nyeri (PQRST)** | Hanya ada catatan `Keterangan` bebas tanpa panduan PQRST terstruktur | Struktur PQRST lengkap di seeder: Lokasi (`PAIN_LOCATION`), Penjalaran (`PAIN_RADIATION`), Kualitas (`PAIN_QUALITY_SEL`), Pemicu (`PAIN_TRIGGER_SEL`), Frekuensi (`PAIN_FREQUENCY`) | **SUDAH TERSEDIA (SEEDER)** | Form input di UI perlu dikemas dalam kartu 2-kolom terorganisir agar cepat diisi |
| 6 | **Integrasi Tanda-Tanda Vital (TTV)** | V1 meminta input manual ulang seluruh TTV (Sistolik, Diastolik, Nadi, Respirasi, Suhu) yang rawan redundan | Final memiliki arsitektur integrasi rujukan tanda vital (`VitalSignReferenceCard` / `referencedVitalSign`) | **SUDAH DITERAPKAN (BACKEND & UTILS)** | Komponen rujukan tanda vital perlu dihadirkan berdampingan pada formulir monitoring nyeri |
| 7 | **Intervensi Farmakologi (Terapi Analgetik)** | Input manual: Waktu, Sumber Obat (Resep/Master), Nama Obat, Dosis, Rute | Kolom di seeder: `PAIN_PHARM_GIVEN`, `PAIN_MED_NAME`, `PAIN_MED_DOSE`, `PAIN_MED_ROUTE` (Oral, IV, IM, SC, Topikal, Rektal, Inhalasi) | **SEBAGIAN DITERAPKAN** | Perlu opsi autocomplete dari resep aktif dokter atau master obat rumah sakit |
| 8 | **Intervensi Non-Farmakologi** | Dropdown pilihan: Dingin, Panas, Posisi, Pijat, Musik, TENS, Relaksasi | Multi-pilihan di seeder: Relaksasi nafas dalam, Kompres hangat, Kompres dingin, Posisi semifowler, Masase, Musik, TENS, Edukasi | **SUDAH TERSEDIA (SEEDER)** | Perlu chip interaktif (*multi-select chips*) yang mudah diklik perawat bangsal |
| 9 | **Interval Kajian Ulang Otomatis (*Reassessment Due*)** | Input tanggal manual oleh perawat (`Waktu Kajian Ulang`) | Sistem server menghitung otomatis `ReassessmentMinutes = 60` dan menyetel `PainReassessmentDueAt` | **SUDAH TERSEDIA (BACKEND ENGINE)** | Tampilkan badge countdown / tenggat waktu evaluasi ulang di bagian atas formulir |
| 10 | **Siklus Hidup Dokumen: Draf vs Terkunci vs Riwayat Berseri** | Toggle statis V1 `[Form Assement Nyeri]` vs `[Riwayat Assement Nyeri]` | Arsitektur V2: `Draft` $\rightarrow$ `Complete/Locked` (EMR Sah) $\rightarrow$ `Addendum` + Timeline Pemantauan Serial Nyeri (*Serial Monitoring Timeline*) | **SUDAH DITERAPKAN (BACKEND & HOOKS)** | Gantikan toggle naif V1 dengan **State-Driven EMR View** + Bilah Riwayat Pemantauan Multi-Waktu |

---

## 3. Laporan Spesifikasi Endpoint Swagger API

Modul Monitoring Nyeri terintegrasi pada controller:
- `PatientAssessmentController` (`Health Services / Clinical Management / Patient Assessment`)
- `ClinicalInstrumentController` (`Health Services / Clinical Management / Clinical Instrument`)

### 3.1. Tabel Spesifikasi Endpoint API

| No | Method | Route Path | Deskripsi Fungsi | Otorisasi / Hak Akses | Request Body (DTO) | Response DTO | HTTP Code |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | `GET` | `/api/v1/health-services/clinical-management/patient-assessments/instruments/resolve` | Mengambil definisi formulir monitoring nyeri terpadu (`PAIN_MONITORING`) untuk episode perawatan aktif | `PatientAssessment : Read` | *Query Params*: `instrumentKind=3`, `episodeId={guid}` | `ApiResponse<ResolvedInstrumentResponse>` | `200 OK`<br/>`404 Not Found` |
| 2 | `GET` | `/api/v1/health-services/clinical-management/patient-assessments` | Mengambil daftar riwayat dokumen monitoring nyeri untuk episode rawat inap aktif | `PatientAssessment : Read` | *Query Params*: `inpEpisodeId={guid}`, `assessmentType=7` | `ApiResponse<PagedResult<PatientAssessmentResponse>>` | `200 OK`<br/>`400 Bad Request` |
| 3 | `GET` | `/api/v1/health-services/clinical-management/patient-assessments/{id}` | Mengambil detail lengkap dokumen monitoring nyeri termasuk isian instrumen dan catatan intervensi | `PatientAssessment : Read` | *None (Route Param: id)* | `ApiResponse<PatientAssessmentResponse>` | `200 OK`<br/>`404 Not Found` |
| 4 | `POST` | `/api/v1/health-services/clinical-management/patient-assessments` | Menyimpan dokumen draf monitoring nyeri baru untuk episode rawat inap | `PatientAssessment : Create` | `CreatePatientAssessmentRequest` (`AssessmentType: 7`) | `ApiResponse<PatientAssessmentResponse>` | `201 Created`<br/>`400 Bad Request` |
| 5 | `PUT` | `/api/v1/health-services/clinical-management/patient-assessments/{id}` | Memperbarui isian skala nyeri, PQRST, TTV rujukan, dan intervensi pada dokumen draf | `PatientAssessment : Update` | `UpdatePatientAssessmentRequest` | `ApiResponse<PatientAssessmentResponse>` | `200 OK`<br/>`404 Not Found` |
| 6 | `POST` | `/api/v1/health-services/clinical-management/patient-assessments/{id}/complete` | Menyelesaikan dan mengunci dokumen monitoring nyeri ke rekam medis elektronik (menghitung `PainReassessmentDueAt`) | `PatientAssessment : Complete` | `CompletePatientAssessmentRequest` (catatan perawat) | `ApiResponse<PatientAssessmentResponse>` | `200 OK`<br/>`422 Unprocessable` |
| 7 | `POST` | `/api/v1/health-services/clinical-management/patient-assessments/{id}/corrections` | Menambahkan catatan koreksi klinis formal (*addendum*) bila terdapat ralat pasca dokumen dikunci | `PatientAssessment : AddCorrection` | `AddAssessmentCorrectionRequest` (alasan & pembetulan) | `ApiResponse<AssessmentAddendumResponse>` | `201 Created`<br/>`403 Forbidden` |

### 3.2. Contoh Kontrak Payload Data

#### Contoh Payload Simpan Respons Formulir (`ResponsesJson`):
```json
{
  "PAIN_STATE": "HasPain",
  "PAIN_TOOL": "NRS",
  "PAIN_SCALE": 7,
  "PAIN_SEDATION": "POSS_2",
  "PAIN_LOCATION": "Abdomen kuadran kanan bawah pasca laparatomi",
  "PAIN_RADIATION": "NO",
  "PAIN_QUALITY_SEL": "MENUSUK",
  "PAIN_TRIGGER_SEL": "GERAK",
  "PAIN_FREQUENCY": "TERUS_MENERUS",
  "PAIN_NON_PHARM": ["RELAKSASI", "POSISI"],
  "PAIN_PHARM_GIVEN": true,
  "PAIN_MED_NAME": "Ketorolac Tromethamine",
  "PAIN_MED_DOSE": "30 mg",
  "PAIN_MED_ROUTE": "IV",
  "PAIN_INTERVENTION": "Injeksi Ketorolac 30mg IV pelan, atur posisi semifowler, edukasi tarik nafas dalam.",
  "PAIN_NOTE": "Pasien tampak meringis berkurang 15 menit pasca injeksi."
}
```

#### Contoh Sinkronisasi ke Kolom Ringkasan `TrxPatientAssessment`:
```json
{
  "assessmentType": 7,
  "hasPain": true,
  "painAssessmentState": 2,
  "painScale": 7,
  "painLocation": "Abdomen kuadran kanan bawah pasca laparatomi",
  "painQuality": "MENUSUK",
  "painTrigger": "GERAK",
  "painFrequency": "TERUS_MENERUS",
  "painManagement": "Injeksi Ketorolac 30mg IV pelan, atur posisi semifowler, edukasi tarik nafas dalam.",
  "painNote": "Pasien tampak meringis berkurang 15 menit pasca injeksi.",
  "painReassessmentDueAt": "2026-09-29T13:00:00Z"
}
```

---

## 4. Alur Proses Bisnis & Keselamatan Pasien (KARS / PAP / SKP)

### 4.1. Diagram Alur Proses Bisnis (Mermaid Flowchart)

```mermaid
flowchart TD
    A["1. Pasien Masuk Rawat Inap / Evaluasi Rutin Shift Perawat"] --> B["2. Perawat Membuka Sub-Menu 'Monitoring Nyeri'"]
    B --> C{"3. Evaluasi Awal Status Nyeri Pasien (PAIN_STATE)"}
    
    C -- "Tidak Nyeri" --> D1["Set Skala Nyeri = 0<br/>Simpan Draf / Selesaikan Dokumen"]
    C -- "Tidak Dapat Dinilai" --> D2["Catat Alasan (Koma / Sedasi Berat / Bayi)<br/>Gunakan Alat Observasi Khusus (CPOT / FLACC)"]
    C -- "Ada Nyeri" --> E["4. Pilih Metode / Alat Ukur Klinis Sesuai Usia & Kondisi"]
    
    E --> F1{"Pilihan Alat Ukur"}
    F1 -- "Dewasa Kooperatif" --> G1["NRS (Numeric Rating Scale 0–10)"]
    F1 -- "Anak > 3 Thn / Geriatri" --> G2["Wong-Baker FACES Pain Scale"]
    F1 -- "Pasien ICU / Ventilator" --> G3["CPOT (Critical-Care Pain Observation Tool)"]
    F1 -- "Bayi / Balita < 3 Thn" --> G4["FLACC Behavioral Scale"]
    
    G1 --> H["5. Tentukan Derajat Intensitas Nyeri (Visual Scale 0–10)"]
    G2 --> H
    G3 --> H
    G4 --> H
    
    H --> I{"Tingkat Keparahan Nyeri"}
    I -- "Nyeri Ringan (1–3)" --> J1["Intervensi Non-Farmakologi (Relaksasi, Posisi, Kompres)"]
    I -- "Nyeri Sedang (4–6)" --> J2["Kolaborasi Analgetik Ringan/Sedang + Non-Farmakologi"]
    I -- "Nyeri Berat (7–10)" --> J3["ALERT KARS: Wajib Lapor DPJP + Analgetik Kuat/Opioid + Re-evaluasi Intensif"]
    
    J1 --> K["6. Pengkajian Lanjutan Karakteristik Nyeri (PQRST) & Skor Sedasi POSS"]
    J2 --> K
    J3 --> K
    
    K --> L{"Skor Sedasi POSS Pasien"}
    L -- "POSS 0 s.d. 2" --> M1["Sedasi Normal / Terkendali"]
    L -- "POSS 3 atau 4" --> M2["SAFETY ALERT: Depresi Pernapasan! Lapor DPJP Segera & Pantau TTV Ketat"]
    
    M1 --> N["7. Dokumentasikan Tindakan Intervensi & Hubungkan Tanda Vital"]
    M2 --> N
    
    N --> O["8. Selesaikan Dokumen (Kunci EMR) -> Server Set Countdown Re-evaluasi 60 Menit"]
    O --> P["9. Evaluasi Ulang Nyeri Pasca Tindakan (Shift Berikutnya / Reassessment)"]
```

### 4.2. Skenario Nyata di Rumah Sakit

#### Skenario 1: Pasien Anak — Bangsal Rawat Inap Anak
- **Pasien**: An. Kevin (6 tahun), pasca tonsilektomi hari ke-0.
- **Tindakan Perawat**: Membuka Monitoring Nyeri, memilih status *Ada Nyeri*, alat ukur **Wong-Baker FACES Pain Scale**.
- **Pengisian**: Anak menunjuk wajah nomor 3 (wajah menangis kecil), skor **6** (*Nyeri Sedang*). Lokasi: Tenggorokan, terasa perih saat menelan. Sedasi POSS 1 (sadar penuh).
- **Intervensi**: Kolaborasi pemberian Paracetamol drip 15 mg/kgBB, anjurkan minum air dingin perlahan, dan teknik distraksi dengan mendengarkan dongeng.
- **Tindak Lanjut**: Dokumen dikunci, sistem menyetel alarm evaluasi ulang 60 menit kemudian.

#### Skenario 2: Pasien Dewasa Pasca Bedah — Bangsal Bedah Orthopedi
- **Pasien**: Tn. Joko (48 tahun), pasca operasi ORIF femur kanan hari ke-1.
- **Tindakan Perawat**: Status *Ada Nyeri*, alat ukur **NRS**, skala nyeri **8** (**Nyeri Berat / Severe Pain**).
- **Respon Klinis**: Sistem memunculkan banner merah: *"PERHATIAN KLINIS: Nyeri Berat (Skor ≥ 7). Wajib lapor dokter DPJP untuk tatalaksana analgetik kuat/parenteral dan evaluasi ulang dalam 30–60 menit."*
- **Tindakan**: Dokter menginstruksikan Morfin 5 mg IV. Perawat memantau sedasi POSS: skor meningkat ke POSS 3 (sering mengantuk). Sistem memicu *Safety Alert* peringatan depresi pernapasan sehingga perawat memantau saturasi O2 dan laju respirasi secara berkala.

#### Skenario 3: Pasien Ruang Rawat Intensif (ICU) — Terpasang Ventilator
- **Pasien**: Ny. Sumarni (68 tahun), syok sepsis terintubasi.
- **Tindakan Perawat**: Status *Ada Nyeri*, alat ukur **CPOT**. Pasien gelisah, ekspresi wajah tegang (1), batuk melawan ventilator (1), tahanan otot positif (1), total skor 3.
- **Tindakan**: Dilakukan titrasi analgetik sedasi Fentanyl sesuai protokol ICU.

### 4.3. Analisis Dampak Perubahan Bisnis
1. **Pencegahan Komplikasi Fatal (Depresi Pernapasan)**:
   - Penggunaan skala sedasi POSS yang terhubung dengan alert visual menghilangkan risiko henti napas akibat overdosis opioid pada pasien rawat inap.
2. **Kepatuhan Standar KARS / STARKES (Sasaran HPK & PAP)**:
   - Standar akreditasi mewajibkan seluruh pasien rumah sakit diskrining nyeri dan dievaluasi ulang secara berkala (re-assessment 60 menit pasca analgetik).
3. **Efisiensi Kerja Perawat Multi-Shift**:
   - Timeline pemantauan serial nyeri memungkinkan perawat shift malam langsung mengetahui obat apa yang diberikan perawat shift sore dan bagaimana respons penurunan skala nyerinya.

---

## 5. Rencana Modernisasi Desain Tampilan (UI/UX) & Inovasi Sistem

### 5.1. Skema Tampilan UI/UX Modern (Wireframe & Layout Architecture)

Rancangan antarmuka memadukan kemudahan operasional V1 dengan inovasi modern V2: **State-Driven EMR View + Dual-Mode Switcher Terpadu** (menggantikan toggle statis V1 `[Form Assement Nyeri] [Riwayat Assement Nyeri]`):

#### Tampilan Mode 1: Saat Dokumen Berstatus DRAFT (Pengisian & Pemantauan Nyeri Aktif)
```text
+---------------------------------------------------------------------------------------------------------------+
| 🏷️ MONITORING & EVALUASI NYERI RAWAT INAP                  [ Status: DRAFT #ASS-2026-0089 ]  [ Tenggat: 60m ]|
| Pasien: Tn. Joko Santoso (48 Tahun)  |  Kamar: Mawar 302  |  DPJP: dr. Rahyussalim, Sp.OT (K)                  |
+---------------------------------------------------------------------------------------------------------------+
| Riwayat Dokumen: [ #ASS-0012 (✓ Selesai - Nyeri 8) ]  * [ #ASS-0089 (Draft Aktif) ]   [ + Pemantauan Baru ]   |
+---------------------------------------------------------------------------------------------------------------+
|                                                                                                               |
| 1. STATUS EVALUASI NYERI: < Wajib Dipilih Sebelum Menyelesaikan Pengkajian >                                  |
| +-----------------------------------------------------------------------------------------------------------+ |
| |  [ ( ) Tidak Nyeri (Skor 0) ]       [ (•) Ada Nyeri (Skor 1–10) ]       [ ( ) Tidak Dapat Dinilai ]        | |
| +-----------------------------------------------------------------------------------------------------------+ |
|                                                                                                               |
| 2. ALAT UKUR & SKALA INTENSITAS NYERI (VISUAL PAIN SCALE SELECTOR):                                           |
| Metode Alat Ukur: [ (•) NRS (Dewasa Sadar) ]  [ ( ) Wong-Baker FACES ]  [ ( ) CPOT (ICU) ]  [ ( ) FLACC ]    |
|                                                                                                               |
| PILIH TINGKAT INTENSITAS NYERI (0 - 10):                                                                      |
| [ 0 ]   [ 1 ]   [ 2 ]   [ 3 ]   [ 4 ]   [ 5 ]   [ 6 ]   [ 7 ]  *[ 8 ]*  [ 9 ]   [ 10 ]                        |
|  🟢      🟢      🟢      🟡      🟡      🟡      🟠      🔴     *🔴*     🔴      🔴                           |
| Bebas   --- Nyeri Ringan ---   ---- Nyeri Sedang ----   -------- NYERI BERAT / SEVERE --------                |
|                                                                                                               |
| +-----------------------------------------------------------------------------------------------------------+ |
| | ⚠️ PERHATIAN KLINIS: PASIEN TERIDENTIFIKASI NYERI BERAT (SKOR: 8 ≥ 7)                   [ Alert Merah KARS] |
| | Pasien mengalami nyeri berat! Wajib segera lapor dokter DPJP untuk analgetik parenteral kuat, posisikan   |
| | pasien senyaman mungkin, dan jadwalkan evaluasi ulang intensif dalam 30–60 menit.                         |
| +-----------------------------------------------------------------------------------------------------------+ |
|                                                                                                               |
| 3. SKOR SEDASI POSS (PASERO OPIOID-INDUCED SEDATION SCALE):                                                   |
| [ ( ) POSS 0: Tidur pulas ]  [ ( ) POSS 1: Sadar penuh ]  *[ (•) POSS 2: Mengantuk ringan, mudah bangun ]*    |
| [ ( ) POSS 3: Sering mengantuk / tertidur saat bicara ⚠️ ]  [ ( ) POSS 4: Somnolen / sulit dibangunkan 🛑 ]  |
|                                                                                                               |
| 4. KARAKTERISTIK KLINIS NYERI (PQRST):                                                                        |
| +----------------------------------------------------+------------------------------------------------------+ |
| | Lokasi Anatomis Nyeri:                             | Penjalaran Nyeri:                                    | |
| | [ Abdomen kuadran kanan bawah pasca laparatomi  ]  | (•) Tidak Menjalar          ( ) Ya, Menjalar         | |
| +----------------------------------------------------+------------------------------------------------------+ |
| | Kualitas Sensasi Nyeri:                            | Faktor Pencetus / Provokasi:                         | |
| | [ Menusuk / Tajam                               v] | [ Saat bergerak / mobilisasi                      v] | |
| +----------------------------------------------------+------------------------------------------------------+ |
| | Frekuensi & Durasi Nyeri:                          | Tanda Vital Rujukan Pasien:                          | |
| | [ Terus-menerus menetap (konstan)               v] | TD: 130/85 mmHg | Nadi: 88x | RR: 20x | Suhu: 36.8°C | |
| +----------------------------------------------------+------------------------------------------------------+ |
|                                                                                                               |
| 5. RENCANA & TINDAKAN INTERVENSI MANAJEMEN NYERI:                                                             |
| Intervensi Non-Farmakologi (Pilih satu atau lebih):                                                           |
| [✓ Relaksasi Nafas Dalam]  [✓ Pengaturan Posisi]  [ Kompres Hangat ]  [ Terapi Musik ]  [ Masase Lembut ]     |
|                                                                                                               |
| Pemberian Analgetik Farmakologi:                                                                              |
| [X] Pasien Diberikan Terapi Obat Analgetik                                                                    |
| Nama Obat: [ Ketorolac Tromethamine         ]  Dosis: [ 30 mg        ]  Rute: [ Intravena (IV)            v]  |
|                                                                                                               |
| Catatan Respon Klinis & Kajian Ulang:                                                                         |
| [ Pasien tampak rileks 20 menit pasca injeksi, keluhan nyeri berkurang menjadi skala 4...                 ] |
|                                                                                                               |
| +-----------------------------------------------------------------------------------------------------------+ |
| | [ Batal ]                                           [ 💾 Simpan Konsep (Draft) ]   [ 🔒 Selesaikan & Kunci ]| |
| +-----------------------------------------------------------------------------------------------------------+ |
+---------------------------------------------------------------------------------------------------------------+
```

#### Tampilan Mode 2: Saat Dokumen Berstatus COMPLETED (Lembar Hasil Terkunci & Sah EMR)
```text
+---------------------------------------------------------------------------------------------------------------+
| 🏷️ MONITORING & EVALUASI NYERI RAWAT INAP                  [ Status: ✓ SELESAI #ASS-2026-0012 ] [ 🔒 Dikunci ]|
| Pasien: Tn. Joko Santoso (48 Tahun)  |  Kamar: Mawar 302  |  DPJP: dr. Rahyussalim, Sp.OT (K)                  |
+---------------------------------------------------------------------------------------------------------------+
| Riwayat Dokumen: * [ #ASS-0012 (✓ Selesai - Nyeri 8) ]   [ #ASS-0089 (Draft Aktif) ]    [ + Pemantauan Baru ] |
+---------------------------------------------------------------------------------------------------------------+
|                                                                                                               |
|  ✓ DOKUMEN PENGKAJIAN SELESAI & TELAH DITANDATANGANI SECARA DIGITAL                                           |
|  Ditandatangani oleh: Ns. Dewi Sartika, S.Kep  |  Waktu: 29 September 2026, 09:15 WIB                          |
|  Batas Kajian Ulang Nyeri: 29 September 2026, 10:15 WIB (Interval 60 Menit KARS)                               |
|                                                                                                               |
|  RINGKASAN HASIL EVALUASI NYERI:                                                                              |
|  +---------------------------------------------------------------------------------------------------------+  |
|  | SKOR NYERI: 8 (NYERI BERAT) | ALAT: NRS | SEDASI: POSS 2 (RINGAN) | ANALGETIK: KETOROLAC 30MG IV DIBERIKAN|  |
|  +---------------------------------------------------------------------------------------------------------+  |
|                                                                                                               |
|  DETAIL KARAKTERISTIK PQRST:                                                                                  |
|  - Lokasi Nyeri     : Abdomen kuadran kanan bawah pasca laparatomi (Tidak menjalar)                           |
|  - Kualitas Nyeri   : Menusuk / Tajam                                                                         |
|  - Faktor Pemicu    : Saat bergerak / mobilisasi                                                              |
|  - Durasi/Frekuensi : Terus-menerus menetap (konstan)                                                         |
|                                                                                                               |
|  TINDAKAN INTERVENSI YANG DILAKUKAN:                                                                          |
|  - Non-Farmakologi  : Relaksasi Nafas Dalam, Posisi Semifowler                                                |
|  - Farmakologi      : Ketorolac 30 mg IV                                                                      |
|  - Respon Pasien    : Nyeri berkurang, ekspresi rileks                                                        |
|                                                                                                               |
|  +---------------------------------------------------------------------------------------------------------+  |
|  | [ 🖨️ Cetak Lembar Monitoring (PDF) ]                                             [ ➕ Tambah Koreksi ]  |  |
|  +---------------------------------------------------------------------------------------------------------+  |
+---------------------------------------------------------------------------------------------------------------+
```

#### Tampilan Mode 3: Bilah Riwayat Pemantauan Serial Nyeri (*Serial Pain Monitoring Timeline*)
```text
+---------------------------------------------------------------------------------------------------------------+
| 📊 TREN MONITORING SERIAL NYERI (TIMELINE ANTAR-SHIFT)                                    [ ● Real-Time Aktif]|
+---------------------------------------------------------------------------------------------------------------+
|                                                                                                               |
| [08:00 WIB] Skor: 8 (Merah)   --> [08:15 WIB] Injeksi Ketorolac 30mg IV --> [09:15 WIB] Skor: 4 (Kuning)      |
| Evaluasi Awal (Dewi Sartika)      Tindakan Kolaboratif                      Evaluasi Ulang (Dewi Sartika)     |
|                                                                                                               |
| [14:00 WIB] Skor: 3 (Kuning)  --> [14:30 WIB] Kompres & Relaksasi       --> [20:00 WIB] Skor: 1 (Hijau)       |
| Operan Shift Sore (Ahmad)         Non-Farmakologi                           Operan Shift Malam (Siti)         |
|                                                                                                               |
+---------------------------------------------------------------------------------------------------------------+
```

### 5.2. Penjelasan Inovasi Unggulan & Perbedaan dengan V1

1. **Visual Pain Scale Selector (Face Icons & Color Gradients)**:
   - Di V1, skor nyeri hanya dropdown teks biasa yang monoton.
   - Di V2, perawat disajikan selector interaktif visual dengan gradasi warna standar keselamatan pasien (0 Hijau, 1–3 Kuning, 4–6 Oranye, 7–10 Merah) dan ikon wajah Wong-Baker yang ramah untuk pasien anak dan geriatri.
2. **Evaluasi Sedasi POSS & Safety Alert Depresi Napas**:
   - Di V1, skor sedasi hanya input angka `0` tanpa penjelasan.
   - Di V2, mengadopsi standar internasional *Pasero Opioid-Induced Sedation Scale* (POSS). Jika perawat memilih POSS 3 atau POSS 4, sistem menyalakan *Safety Alert* berdenyut merah memperingatkan risiko henti napas akibat opioid.
3. **PQRST Terstruktur (Bebas Ketik Manual Monoton)**:
   - Di V1, keluhan nyeri hanya berupa satu kolom teks bebas.
   - Di V2, parameter PQRST dikemas dalam pilihan chip dan dropdown terstruktur sehingga pengisian selesai dalam hitungan detik.
4. **Integrasi Rujukan TTV Tanpa Duplikasi**:
   - Di V1, perawat terpaksa mengetik ulang seluruh angka TTV di form monitoring nyeri.
   - Di V2, data TTV langsung terhubung ke baris tanda vital aktif pasien (*single source of truth*).
5. **Countdown Timer Evaluasi Ulang 60 Menit (KARS Compliance)**:
   - Sistem secara otomatis menghitung `PainReassessmentDueAt` = 60 menit setelah dokumen diselesaikan, mengingatkan perawat bangsal untuk melakukan evaluasi ulang tepat waktu.

---

## 6. Rencana Implementasi Tuntas (*Definition of Done*)

Setelah dokumen rencana kerja ini disetujui pengguna, eksekusi implementasi dilakukan secara menyeluruh tanpa bertahap-tahap:

### 6.1. Pekerjaan Backend:
1. Memastikan definisi instrumen `PAIN_MONITORING` di `ClinicalInstrumentDraftSeeder.cs` memuat butir lengkap: `PAIN_STATE`, `PAIN_TOOL`, `PAIN_SCALE`, `PAIN_SEDATION`, PQRST (`PAIN_LOCATION`, `PAIN_RADIATION`, `PAIN_QUALITY_SEL`, `PAIN_TRIGGER_SEL`, `PAIN_FREQUENCY`), intervensi (`PAIN_NON_PHARM`, `PAIN_PHARM_GIVEN`, `PAIN_MED_NAME`, `PAIN_MED_DOSE`, `PAIN_MED_ROUTE`, `PAIN_INTERVENTION`, `PAIN_NOTE`), dan `ReassessmentMinutes = 60`.
2. Memastikan endpoint `resolve` mengembalikan definisi instrumen `PAIN_MONITORING` saat `instrumentKind === 3`.
3. Memastikan sinkronisasi otomatis kolom ringkasan ke `TrxPatientAssessment` (`HasPain`, `PainAssessmentState`, `PainScale`, `PainLocation`, `PainTrigger`, `PainFrequency`, `PainManagement`, `PainNote`, `PainReassessmentDueAt`).
4. Menjalankan verifikasi kompilasi backend: `dotnet build QuilvianSystemBackend.csproj --no-incremental` bebas error (0 error).

### 6.2. Pekerjaan Frontend:
1. Membangun komponen UI modern `PainMonitoringForm.jsx`:
   - Tombol pilihan cepat `PAIN_STATE` (*Tidak Nyeri*, *Ada Nyeri*, *Tidak Dapat Dinilai*).
   - Visual Pain Scale Selector (0–10) dengan chip warna interaktif dan ikon wajah.
   - Pilihan metode alat ukur (*NRS*, *Wong-Baker*, *CPOT*, *FLACC*).
   - Selektor Skor Sedasi POSS dengan kartu peringatan keselamatan jika skor $\ge 3$.
   - Form 2-kolom terorganisir untuk karakteristik PQRST.
   - Intervensi multi-select chips non-farmakologi dan input analgetik farmakologi.
2. Mengintegrasikan komponen ke dalam `ClinicalInstrumentFormRenderer.jsx` untuk mendelegasikan secara eksklusif saat `instrumentKind === 3`.
3. Menghubungkan ke `useClinicalInstrumentForm.js` untuk auto-derivation dan sinkronisasi payload ringkasan ke database.
4. Menyediakan bilah riwayat dokumen multi-shift dan visualisasi tren serial nyeri antar-waktu.
5. Menjalankan unit test dan verifikasi kompilasi frontend (`npm.cmd run build`) bebas error (0 error).

---

## 7. Status Persetujuan & Hasil Implementasi Tuntas

> Dokumen ini berstatus: **DISETUJUI & SELESAI DIIMPLEMENTASIKAN (COMPLETED & VERIFIED)**.  
> Persetujuan pengguna: *"setuju , lakukan implementasi"*.

### Bukti Implementasi & Verifikasi:
1. **Komponen Frontend Terpadu (`PainMonitoringForm.jsx`)**:
   - Berhasil dibuat di `src/components/view/health-services/inpatient-management/nursing-workspace/sections/assessment/components/pain-monitoring/pain-monitoring-form.jsx`.
   - Menghadirkan Triase Cepat Nyeri (3 mode: *Tidak Nyeri*, *Ada Nyeri*, *Tidak Dapat Dinilai*).
   - Skala Visual Interaktif 0–10 bergradasi warna keselamatan pasien (Hijau, Kuning, Oranye, Merah) dengan ikon wajah Wong-Baker FACES.
   - Pilihan metode alat ukur klinis terstandar: NRS, Wong-Baker FACES, CPOT, dan FLACC.
   - *Safety Alert* Nyeri Berat ($\ge 7$) berdenyut merah sesuai standar KARS/IPSG.
   - Skrining Skor Sedasi Opioid POSS (*Pasero Opioid-Induced Sedation Scale*) dengan alert depresi napas untuk POSS 3 dan POSS 4.
   - Format 2-kolom terorganisir untuk karakteristik PQRST dan integrasi kartu rujukan tanda vital aktif pasien (`VitalSignReferenceCard`).
   - Intervensi non-farmakologi (multi-select chip) dan farmakologi (analgetik dosis & rute).
   - *Countdown Badge* Kaji Ulang 60 Menit Pasca Intervensi.
2. **Integrasi Renderer & Hook Payload (`ClinicalInstrumentFormRenderer` & `useClinicalInstrumentForm`)**:
   - Terintegrasi penuh pada `ClinicalInstrumentFormRenderer.jsx` saat `Number(instrumentKind) === 3`.
   - Banner skor dan toolbar atas yang tidak relevan disembunyikan otomatis untuk menjaga kerapian tampilan.
   - Hook `use-clinical-instrument-form.js` memetakan seluruh kolom ringkasan (`hasPain`, `painScale`, `painLocation`, `painQuality`, `painTrigger`, `painFrequency`, `painManagement`, `painNote`) ke payload EMR `TrxPatientAssessment`.
3. **Desain Token & CSS Modern (`nursing-workspace.module.css`)**:
   - Seluruh kelas penataan gaya interaktif telah dipasang (`.painMonitoringWrapper`, `.painScalePillsGrid`, `.painTriaseCard`, `.possContainer`, `.pqrstCard`, `.painInterventionCard`, `.painAlertBanner`).
4. **Verifikasi Pengujian Otomatis (`node --test`)**:
   - 7/7 unit tests lulus 100% tanpa kegagalan:
     - `tests/unit/inpatient-pain-monitoring-operational.test.mjs` (2 pass)
     - `tests/unit/inpatient-fall-risk-assessment-operational.test.mjs` (3 pass)
     - `tests/unit/inpatient-fall-risk-instruments-and-alerts.test.mjs` (2 pass)

