# Catatan Rilis — Modul Rekam Medis

> Berkas ini memuat empat rilis, terbaru di atas: **rilis amandemen `0.4.0`**, **rilis amandemen
> `0.3.0`**, **rilis amandemen `0.2.0`**, dan **rilis pertama `0.1.x`**. Rilis lama tetap utuh karena
> perilakunya masih berlaku.

# Rilis amandemen `0.4.0` — Indikator ringkasan pulang dan rencana kontrol

| Field | Isi |
|---|---|
| Tanggal disusun | 11 November 2026 |
| Disusun pada task | `BE-51` |
| Kontrak | API, state, validation, integration, permission/audit **`0.4.0`** — disahkan 11 November 2026 (`RM-DEC-108`); melebur penyegaran `0.3.1` (`RM-DEC-095`) |
| Task yang dirilis | `BE-47`, `BE-48`, `BE-49`, `BE-50` — seluruhnya `SELESAI`; laporan per task di `task/report/backend/` |
| Persetujuan modul lain | Perubahan di folder Rawat Inap (`BE-47`, `BE-48`, `BE-50`): Muhammad Hamzah, disampaikan Yoga Aji Pratama (`RM-DEC-109`) |
| Status catatan ini | **Disahkan** pemilik API, Yoga Aji Pratama, 11 November 2026 (`RM-DEC-110`) — lihat bagian C9 |
| Dampak skema | **Nol.** Tanpa migration, tanpa tabel atau kolom baru |
| Dampak perilaku | **Satu penolakan baru di Rawat Inap:** tanda tangan ulang ringkasan pulang (C1). Satu bagian baru di rincian ringkasan pulang (C2). Satu grup endpoint dan satu izin baru (C3, C5) |
| Prasyarat penerapan | Tidak ada migration. Rilis `0.3.0` sebaiknya sudah terpasang lebih dulu |

> **BACA BAGIAN C1 LEBIH DULU.** Ia satu-satunya perubahan yang dapat membuat permintaan yang
> kemarin berhasil kini ditolak.

## C1. Perubahan perilaku pada endpoint yang sudah berjalan

### C1.1 Ringkasan pulang yang sudah ditandatangani tidak dapat ditandatangani ulang di luar sesi koreksi

`PATCH api/v1/health-services/inpatient-management/discharges/{episodeId}/summary/sign` (`BE-48`)

| | |
|---|---|
| **Sebelumnya** | Ringkasan yang sudah ditandatangani dapat ditandatangani lagi kapan saja. Waktu dan dokter penanda tangan lama ditimpa tanpa salinan |
| **Sekarang** | Bila ringkasan sudah ditandatangani **dan** tidak ada sesi koreksi yang terbuka → **`409`** "Resume sudah ditandatangani; ubah lewat sesi koreksi." Waktu tanda tangan tidak berubah |
| **Yang tidak berubah** | Tanda tangan pertama kali; tanda tangan ulang **di dalam** sesi koreksi supervisor; penutupan episode; jalur supervisor |
| **Urutan pemeriksaan** | Pemeriksaan ini mendahului pemeriksaan DPJP. Dokter yang bukan DPJP pada ringkasan yang sudah final menerima `409`, bukan `403` |
| **Alasan** | `RM-DEC-105`: waktu tanda tangan pertama adalah dasar indikator ketepatan ringkasan pulang (C3) dan bukti klinis yang tidak boleh hilang |
| **Yang harus dilakukan klien** | Layar Rawat Inap sudah menyembunyikan tombol tanda tangan untuk ringkasan yang bertanda tangan (`resolveDischargeSummaryAuthority`), sehingga pemakaian normal tidak terdampak. Klien lain cukup menampilkan pesan `409` apa adanya. Untuk mengubah ringkasan yang sudah final: minta supervisor membuka sesi koreksi |

**Contoh.** dr. Bima menandatangani ringkasan Pak Andi 4 Oktober 10.00 WIB. Dua hari kemudian
permintaan tanda tangan terkirim lagi dari tab lama → `409`; waktu tanda tangan tetap 4 Oktober 10.00.

## C2. Rincian ringkasan pulang di berkas memuat rencana kontrol

`GET api/v1/health-services/medical-record-management/medical-records/{patientId}/documents/DischargeSummary/{documentId}` (`BE-47`)

Bentuk respons tidak berubah; `sections[]` bertambah satu butir **"Rencana kontrol"** tepat sesudah
"Instruksi tindak lanjut", bila DPJP mengisi tanggal kontrol (`RM-DEC-104`):

| Isian di ringkasan | Teks bagian |
|---|---|
| Tanggal + poli + dokter | `10-10-2026 — Poli Penyakit Dalam — dr. Maya` |
| Tanggal + poli | `10-10-2026 — Poli Penyakit Dalam` |
| Tanpa tanggal kontrol | Bagian tidak muncul |

Penilaian kelengkapan berkas **tidak** berubah: rencana kontrol bukan dokumen wajib. Layar berkas
menampilkannya tanpa perubahan frontend karena panel menggambar `sections` apa adanya.

## C3. Endpoint baru

### C3.1 Health Services / Medical Record Management / Discharge Summary Timeliness

Base URL: `api/v1/health-services/medical-record-management/discharge-summary-timeliness`
(`BE-49`, `BE-50`)

| Method | Path | Kegunaan | Hak akses |
|---|---|---|---|
| `GET` | `/filters/metadata` | Definisi kolom indikator dan rentang periode maksimal (366 hari) | `DischargeSummaryTimeliness : Read` |
| `GET` | `/` | Angka ketepatan tanda tangan ringkasan pulang Rawat Inap per dokter per periode, untuk OPPE/FPPE HR | `DischargeSummaryTimeliness : Read` |

Query `GET /`: `periodFrom`, `periodTo` (wajib, `yyyy-MM-dd`, kalender WIB, inklusif), `doctorId`,
`workforceProfileId`, `page` (bawaan 1), `pageSize` (bawaan 20, maks 100).

Kode status: `200`; `400` (periode kosong, terbalik, lebih dari 366 hari, ukuran halaman di luar 1–100);
`401`; `403`; `503` "Data Rawat Inap tidak dapat dibaca. Coba lagi." — **tidak pernah** angka sebagian.

**Definisi yang dipakai** (`RM-DEC-102`, `105`–`107`):

| Kolom | Arti |
|---|---|
| `within24h` | Ditandatangani paling lama 24 jam sesudah pasien keluar ruangan (cadangan: waktu tutup episode). Tanda tangan sebelum pasien keluar dihitung 0 jam |
| `between24And48h` | Lebih dari 24 sampai 48 jam, **termasuk** ringkasan yang belum ditandatangani dan sudah tertunda selama itu |
| `over48h` | Lebih dari 48 jam, termasuk yang belum ditandatangani |
| `unsignedBetween24And48h`, `unsignedOver48h` | Bagian dari dua kolom di atas yang belum ditandatangani saat ditarik |
| `notYetDueCount` | Belum ditandatangani dan baru tertunda 24 jam atau kurang — **tidak** masuk penyebut `assessedCount` |
| Dokter | Penanda tangan **pertama**; ringkasan yang belum ditandatangani dibebankan ke DPJP aktif. Tanpa DPJP aktif → baris `doctorId = null` "Tanpa DPJP aktif", selalu terakhir |

**Contoh** (ditarik 1 November 12.00 WIB). Ibu Sari keluar 30 Oktober 09.00, ringkasannya belum
ditandatangani → `over48h` dan `unsignedOver48h` atas DPJP-nya. Pak Andi keluar 1 November 08.00, belum
ditandatangani → `notYetDueCount`, tidak masuk penyebut.

**Paging.** Respons memakai bentuk `PagedResult` project: `pageNumber`, `pageSize`, `totalData`,
`totalPage`, `items`. Contoh di kontrak API `0.4.0` C1 masih menulis `page`/`totalCount`/`totalPages`;
selisih ini dicatat sebagai **penyegaran kontrak `0.4.1`** tanpa perubahan perilaku, untuk pass blueprint
berikutnya (artefak kontrak terkunci hash tidak disunting selama delivery).

**Yang tidak pernah ada di respons maupun log:** identitas pasien, nomor rekam medis, kunjungan,
episode, isi ringkasan (`RM-DEC-101`).

## C4. Keterangan mutu data pada setiap tarikan

| Field | Isi |
|---|---|
| `generatedAt` | Waktu tarik (UTC). Angka periode yang masih berjalan dapat berubah sampai seluruh ringkasan ditandatangani |
| `dataNotes.periodStillChanging` | `true` bila masih ada ringkasan belum ditandatangani pada periode itu |
| `dataNotes.closedAtFallbackCount` | Jumlah ringkasan yang jam mulainya memakai waktu tutup episode karena waktu keluar ruangan kosong |
| `dataNotes.signedBeforeDepartureCount` | Jumlah ringkasan yang ditandatangani sebelum pasien keluar (dihitung 0 jam) |
| `dataNotes.resignTrackingNote` | "Tanda tangan ulang tanpa sesi koreksi sebelum perbaikan Rawat Inap tidak terlacak; waktu tanda tangan pertama dapat tampak lebih lambat." — data sebelum C1 berlaku tidak dapat dipulihkan (`RM-DEC-105`) |

## C5. Hak akses baru

| Izin | Untuk | Keterangan |
|---|---|---|
| `DischargeSummaryTimeliness : Read` | Kedua endpoint C3.1 | Terdaftar otomatis saat aplikasi mulai. **Tidak** dipasangkan ke jabatan mana pun (`DS-R6-05`). Usulan pemegang: Komite Medik dan Subkomite Mutu Profesi — dipasang admin **sesudah** pemilik HR menerima kontrak `INT-RM-20` |

Izin ini **tidak sama** dengan `MedicalRecordCompleteness : Read`: memegang salah satu tidak memberi
yang lain.

## C6. Penyegaran `0.3.1` yang dilebur

Profil dokumen wajib berjenis poli `0` (`Unknown`, "Tidak diketahui") ditolak `400` "Jenis poli tidak
dikenal." dan tidak lagi ditawarkan di `clinicTypeOptions` (`RM-DEC-095`). Saringan daftar dengan
`clinicType=0` tetap sah.

## C7. Yang WAJIB disiapkan sebelum rilis dipakai

| No | Persiapan | Pemilik |
|---:|---|---|
| 1 | Beri tahu pengguna dan pengembang layar Rawat Inap tentang C1 | Muhammad Hamzah (pemilik Rawat Inap) |
| 2 | Kirim kontrak `INT-RM-20` ke pemilik HR; pasang izin C5 ke jabatan hanya sesudah HR menerima | Yoga Aji Pratama; admin hak akses |
| 3 | Uji tarikan indikator periode panjang di lingkungan dengan data nyata; bila lambat, ajukan index ke pemilik Rawat Inap lewat task terpisah | Rekam medis bersama Rawat Inap |

## C8. Yang TIDAK berubah

- Skema database, seluruh tabel rekam medis dan Rawat Inap.
- Penilaian kelengkapan berkas, linimasa, jejak akses, dan addendum.
- Tanda tangan ringkasan pulang pertama kali, sesi koreksi, dan penutupan episode Rawat Inap.
- Gerbang kewenangan klinis HR, log override darurat, dan catatan K3RS **tidak** dibaca rekam medis
  (`RM-DEC-098`–`100`).

## C9. Keterangan di Swagger dan status persetujuan

Keterangan rilis ini diletakkan pada **schema** (properti DTO indikator), mengikuti A8. Tidak ada
`<summary>` baru pada action controller. Keterangan itu dibuktikan terbit lewat `ReleaseContractTests`.

**Catatan rilis amandemen `0.4.0` disahkan** pemilik API, 11 November 2026 (`RM-DEC-110`): C1 dan C7 diterima
untuk diumumkan kepada pemakai. Kontraknya disahkan lebih dulu (`RM-DEC-108`). Pernyataan pemilik Rawat Inap atas
C1 dan C2 masih ditunggu; persetujuan folder Rawat Inap untuk perubahan kodenya sudah ada (`RM-DEC-109`).

| Peran | Nama | Tanggal setuju |
|---|---|---|
| Pemilik API | Yoga Aji Pratama | 11 November 2026 (`RM-DEC-110`) |
| Pemilik Rawat Inap (untuk C1, C2) | Muhammad Hamzah | — |

---

# Rilis amandemen `0.3.0` — Optik dan Rehab Medik

| Field | Isi |
|---|---|
| Tanggal disusun | 10 November 2026 |
| Disusun pada task | `BE-46` |
| Kontrak | API, state, validation, integration, permission/audit **`0.3.0`** — disahkan 10 November 2026 (`RM-DEC-088`) |
| Task yang dirilis | `BE-42`, `BE-43`, `BE-44`, `BE-45` — seluruhnya `SELESAI`; laporan per task di `task/report/backend/` |
| Status catatan ini | **Disahkan** pemilik API, Yoga Aji Pratama, 11 November 2026 (`RM-DEC-110`) — lihat bagian B8 |
| Dampak skema | **Aditif.** Satu migration `20261110043756_AddClinicTypeToMedicalRecordCompletenessProfile`: satu kolom boleh kosong, index unik profil diganti empat kolom, satu CHECK. **Nol** perubahan pada tabel modul lain |
| Dampak perilaku | Kunjungan unit Optik dan poli Rehabilitasi Medik dinilai dengan profilnya sendiri. **Satu penolakan baru:** addendum koreksi pada dokumen Optik (B1) |
| Prasyarat penerapan | Migration amandemen `0.2.0` (`20261028070658_…`) **wajib** sudah diterapkan lebih dulu. Per 10 November 2026 baru devYoga yang menerimanya |

> **BACA BAGIAN B1 LEBIH DULU.** Ia satu-satunya perubahan yang dapat membuat permintaan yang
> kemarin berhasil kini ditolak.

## B1. Perubahan perilaku pada endpoint yang sudah berjalan

### B1.1 Addendum koreksi pada dokumen Optik ditolak

`POST .../clinical-note-addendums/by-document/{documentKind}/{documentId}` dan `.../as-substitute`

| | |
|---|---|
| **Sebelumnya** | Addendum koreksi diterima untuk jenis dokumen apa pun, termasuk Pemeriksaan Refraksi (`45`) dan Resep Optik (`46`). Addendum itu tersimpan tetapi dokumen **tidak** bertanda "pernah diralat" |
| **Sekarang** | `purpose` koreksi — atau `purpose` kosong, yang bermakna koreksi — untuk jenis `45`/`46` ditolak **`400`**: "Koreksi dokumen Optik dibuat sebagai revisi di layar Optik." Nol baris ditulis |
| **Yang tidak berubah** | Addendum **pengesahan** (`purpose = 2`) atas dokumen Optik yang terkunci tanpa tanda tangan tetap diterima. Koreksi CPPT, asesmen, dan dokumen Rehab tidak berubah |
| **Alasan** | `RM-DEC-079`, `RM-DEC-084`: dokumen Optik dikoreksi lewat versi baru di layar Optik, supaya hanya ada satu jalur koreksi |
| **Yang harus dilakukan klien** | Baca `isCorrectionAllowed` dari `GET .../authority/{documentKind}/{documentId}`; bila `false`, jangan tampilkan tombol koreksi. Layar berkas rekam medis menyesuaikan pada `FE-16` |

**Contoh.** Rina (RO) mencoba menambah koreksi pada refraksi Ibu Sari yang sudah ditandatangani
lewat layar berkas rekam medis → `400` berisi arahan. Rina membuat revisi 2 di layar Optik; berkas
menampilkan versi 2 bertanda "pernah diralat".

### B1.2 Penanda "pernah diralat" dokumen Rehab tidak lagi menghitung pengesahan

Linimasa dan rincian dokumen jenis `47`–`52`. Sebelumnya catatan rehab yang hanya **disahkan**
(addendum pengesahan) bertanda "pernah diralat". Sekarang hanya addendum **koreksi** yang menandai
(`DS-R5-04`). Bagian "Riwayat addendum" pada rincian tetap menyebut jumlah seluruh addendum.

### B1.3 Penanda "pernah diralat" dokumen Optik ikut menghitung addendum koreksi lama

Dokumen Optik versi 1 yang sempat diberi addendum koreksi sebelum B1.1 berlaku kini bertanda
"pernah diralat" (`BE-44`). Sebelumnya hanya nomor versi di atas 1 yang menandai.

## B2. Yang baru di kontrak

### B2.1 Health Services / Medical Record Management / Medical Record — jenis dokumen

Tidak ada endpoint baru. Delapan nilai `ClinicalDocumentKind` yang sudah tampil sejak modul Optik
dan Rehab Medik dipasang kini tercatat di kontrak:

| Nilai | Nama | Modul | Cara koreksi |
|---:|---|---|---|
| 45 | `OpticalRefraction` — Pemeriksaan Refraksi | Optik | Versi baru di layar Optik |
| 46 | `OpticalPrescription` — Resep Optik | Optik | Versi baru di layar Optik |
| 47–52 | `RehabAssessment`, `RehabProgram`, `RehabSession`, `RehabProgressReview`, `RehabDischargeSummary`, `RehabEducation` | Rehab Medik | Addendum |

### B2.2 Health Services / Master Data / Medical Record Completeness Profile

Base URL: `api/v1/health-services/master-data/medical-record-completeness-profiles`

| Method | Path | Yang bertambah | Hak akses |
|---|---|---|---|
| `GET` | `/filters/metadata` | `clinicTypeOptions[]`; isian formulir `clinicType`; parameter query `clinicType` | `MedicalRecordCompletenessProfile : Read` |
| `GET` | `/` | Query `clinicType`; baris memuat `clinicType`, `clinicTypeName` | `MedicalRecordCompletenessProfile : Read` |
| `GET` | `/criteria/options` | Opsi `17` `RehabClinicalDocument` — "Dokumen Rehab Medik" | `MedicalRecordCompletenessProfile : Read` |
| `GET` | `/{id}` | `clinicType`, `clinicTypeName` | `MedicalRecordCompletenessProfile : Read` |
| `POST` | `/` | Request `clinicType` (opsional) | `MedicalRecordCompletenessProfile : Create` |
| `PUT` | `/{id}` | Request `clinicType` (opsional) | `MedicalRecordCompletenessProfile : Update` |

Kode status baru: `400` "Jenis poli hanya dapat dipakai pada profil tanpa kondisi." (`VAL-RM-B02`);
`400` "Jenis poli tidak dikenal." (`VAL-RM-B03`); `409` "Profil untuk kombinasi ini sudah ada: {kode}."
(`VAL-RM-B04`).

**Perubahan bunyi pesan.** Kunci kembar sebelumnya berbunyi "Profil untuk jenis kunjungan dan unit
ini sudah ada." Kode statusnya tetap `409`; klien yang mencocokkan bunyi pesan perlu menyesuaikan.

### B2.3 Health Services / Medical Record Management / Clinical Note Addendum

| Method | Path | Yang bertambah |
|---|---|---|
| `GET` | `/authority/{documentKind}/{documentId}` | `isCorrectionAllowed` (`boolean`, bawaan `true`) |

## B3. Kelengkapan berkas

Urutan pemilihan profil dasar kini empat langkah: **poli + unit → poli → unit → umum**
(`RM-DEC-085`). Profil lama tidak berjenis poli, sehingga hasil penilaian kunjungan di luar unit
Optik dan poli Rehabilitasi Medik **tidak berubah**.

| Profil baru (seeder) | Berlaku untuk | Dokumen wajib |
|---|---|---|
| `RJ-OPT` | Rawat Jalan di unit berjenis Optik | Tidak ada (`RM-DEC-080`) — lengkap bila seluruh dokumen Optik ditandatangani |
| `RJ-REHAB` | Rawat Jalan di poli berjenis Rehabilitasi Medik | Satu asesmen, sesi terapi, reasesmen, atau ringkasan akhir Rehab yang final (`RM-DEC-081`, `086`) |

Optik dan Rehab Medik **tidak** pernah membuat berkas `Menunggu hasil`: kacamata yang belum
diserahkan bukan hasil klinis, dan sesi berikutnya terjadi di kunjungan lain (`RM-DEC-082`).

## B4. Hak akses

**Tidak ada izin baru.** Seluruh perubahan memakai izin yang sudah ada.

## B5. Yang WAJIB disiapkan sebelum rilis dipakai

| No | Persiapan | Pemilik |
|---:|---|---|
| 1 | Terapkan migration `20261110043756_AddClinicTypeToMedicalRecordCompletenessProfile` sesudah migration amandemen `0.2.0` | User, dengan instruksi terpisah |
| 2 | Pastikan poli Rehabilitasi Medik ada di master poli dengan jenis **Rehabilitasi** (`ClinicType = 6`). Tanpa itu `RJ-REHAB` tidak pernah terpilih | Admin data |
| 3 | Bila kunjungan Optik/Rehab yang sudah dinilai perlu dinilai ulang seketika, sentuh profil `RJ-OPT`/`RJ-REHAB` lewat layar master (simpan ulang). Tanpa itu penilaian ulang menyusul paling lambat 24 jam | Unit rekam medis |

## B6. Yang TIDAK berubah

- Profil dokumen wajib lama, kriteria 1–16, dan hasil penilaian kunjungan di luar unit Optik dan
  poli Rehabilitasi Medik.
- Aturan addendum untuk CPPT, asesmen, dan dokumen Rehab.
- Seluruh endpoint linimasa, kelengkapan, dan jejak akses — bentuknya hanya bertambah.

## B7. Keterangan di Swagger

Keterangan rilis ini diletakkan pada **schema**, mengikuti A8: `clinicType` pada request dan respons
profil, `clinicTypeOptions` pada metadata, dan `isCorrectionAllowed` pada respons kewenangan addendum.
Tidak ada `<summary>` baru pada action controller. Keterangan itu dibuktikan terbit lewat uji
`ReleaseContractTests`.

## B8. Status persetujuan

**Catatan rilis amandemen `0.3.0` disahkan** pemilik API, 11 November 2026 (`RM-DEC-110`): B1 dan B5 diterima
untuk diumumkan kepada pemakai. Kontraknya disahkan lebih dulu (`RM-DEC-088`).

| Peran | Nama | Tanggal setuju |
|---|---|---|
| Pemilik API | Yoga Aji Pratama | 11 November 2026 (`RM-DEC-110`) |
| Pemilik frontend | Yoga Aji Pratama | 11 November 2026 (`RM-DEC-110`) |

---

# Rilis amandemen `0.2.0` — berkas lintas modul dan kelengkapan berkas

| Field | Isi |
|---|---|
| Tanggal disusun | 2 November 2026 |
| Disusun pada task | `BE-41` |
| Kontrak | API, state, validation, integration, permission/audit **`0.2.0`** — disahkan 28 Oktober 2026 (`RM-DEC-062`) |
| Task yang dirilis | `BE-21`–`BE-40`, seluruhnya `SELESAI`; laporan per task di `task/report/backend/` |
| Status catatan ini | **Menunggu pengesahan** pemilik API (Yoga Aji Pratama) — lihat bagian A9 |
| Dampak skema | **Aditif.** Empat tabel baru dan satu kolom ber-bawaan dalam satu migration `20261028070658_AddMedicalRecordCompletenessFoundation`; **nol** perubahan kolom pada tabel modul lain |
| Dampak perilaku | Linimasa kini membaca seluruh modul; penutupan kunjungan mencatat waktu tutup klinis; berkas dinilai kelengkapannya. **Bentuk respons hanya bertambah** — klien `0.1.1` tetap berjalan |

> **BACA BAGIAN A1 DAN A7 LEBIH DULU.** A1 memuat perubahan perilaku pada endpoint yang sudah
> dipakai. A7 memuat keterbatasan yang wajib tampil di layar supaya pembaca tidak salah membaca
> berkas.

## A1. Perubahan perilaku pada endpoint yang sudah berjalan

### A1.1 Jenis 14 dan 15 tidak lagi kosong diam-diam

`GET .../medical-records/{patientId}/timeline` dan `.../summary`

| | |
|---|---|
| **Sebelumnya** | Jenis `14` (sesi hemodialisa) dan `15` (hasil laboratorium) sudah terdaftar, tetapi tidak punya pembaca. Linimasa menjawabnya **kosong**, seolah pasien tidak punya dokumen itu |
| **Sekarang** | Keduanya terbaca dari modulnya: sesi hemodialisa yang sudah disahkan dan hasil laboratorium PK/Mikrobiologi yang sudah dirilis |
| **Jenis tanpa pembaca** | Tidak lagi dijawab kosong. Jenis itu dilaporkan lewat field baru `unavailableKinds`, lengkap dengan kalimat untuk pembaca. Saat ini hanya satu: hasil Patologi Anatomi (`44`, `RM-DEC-058`) |

**Yang harus dilakukan klien:** menampilkan isi `unavailableKinds` sebagai "belum tersedia",
**bukan** "tidak ada dokumen". `isComplete` tidak dipengaruhi `unavailableKinds`; ia tetap hanya
menyatakan sumber gagal dan pemotongan.

> **Perbaikan pada `BE-41`.** Field `unavailableKinds` sudah dihitung sejak `BE-21`, tetapi baru
> sampai ke selubung respons `/timeline` (`MedicalRecordTimelineResponse`) pada `BE-41`. Sebelum
> itu ia hanya ada di respons `/summary`. Uji baru memastikan setiap field hasil linimasa sampai
> ke klien.

### A1.2 Satu sumber lambat tidak lagi membatalkan berkas

| | |
|---|---|
| **Sebelumnya** | Seluruh sumber dibaca tanpa batas waktu; satu sumber yang macet menahan seluruh linimasa |
| **Sekarang** | Setiap sumber dibatasi **3 detik**, seluruh berkas **20 detik**. Sumber yang melewati batas masuk `failedSources`, dan respons tetap `200` |
| **Pembatalan oleh pengguna** | Tetap membatalkan permintaan, sama seperti sebelumnya |
| **Konfigurasi** | `MedicalRecord:SourceTimeoutSeconds`, `MedicalRecord:FileTimeoutSeconds` |

### A1.3 Linimasa kini membaca seluruh modul

Jenis baru yang tampil, masing-masing hanya bila sudah **final menurut modul pemiliknya**:

| Modul | Jenis | Final bila | Task |
|---|---|---|---|
| Laboratorium | `15` Hasil laboratorium (PK, Mikrobiologi) | Dirilis | `BE-23` |
| Radiologi | `16` Laporan radiologi | Dirilis | `BE-24` |
| IGD | `17`–`20` Triase, observasi, resusitasi, disposisi | Selesai; disposisi `Confirmed` | `BE-29` |
| Rawat Inap | `21` Ringkasan pulang, `22` Dokumen admisi | Ditandatangani / `Completed` | `BE-30` |
| Kamar Operasi | `23`–`26` Laporan operasi, catatan anestesi, checklist keselamatan, catatan kelahiran | `Final` / `Completed` / saat tersimpan | `BE-31` |
| Hemodialisa | `14` Sesi hemodialisa | Disahkan (`Finalized`) | `BE-32` |
| Gizi | `27` Asuhan gizi | Saat tersimpan, **dapat berubah** | `BE-33` |
| Bank Darah | `28` Golongan darah, `29` Reaksi transfusi | `Validated` / `Acknowledged` | `BE-34` |
| Farmasi | `30`–`32` Resep, pemberian obat, rekonsiliasi obat | `Submitted` / `Administered` / ada keputusan | `BE-35` |
| MCU | `33` Resume, `34` Hasil pemeriksaan | Versi yang pernah diverifikasi | `BE-36` |
| Rekam medis (`ClinicalManagement`) | `35`–`42` Rencana asuhan, visite, intervensi, monitoring transfusi, surat dokter, evaluasi manajemen kasus, infeksi nosokomial, serah terima | Final menurut modelnya | `BE-22` |
| Rekam medis (`ClinicalManagement`) | `43` Ringkasan pencatatan berkala | Satu baris per jenis per hari (WIB) | `BE-22` |

Pembaca tidak memeriksa izin modul asal (`RM-DEC-040`): pengguna dengan `MedicalRecord : Read`
melihat dokumen final semua modul. Jejak aksesnya tetap dicatat berkas rekam medis.

Field tambahan pada setiap baris (`MedicalRecordTimelineItemResponse`):

| Field | Arti | Yang harus dilakukan klien |
|---|---|---|
| `sourceModule` | Modul pemilik dokumen | Tampilkan sebagai asal dokumen |
| `finalizedAt` | Waktu final menurut modul pemilik | — |
| `isCorrected` | Pernah diralat lewat mekanisme modul pemilik; yang tampil versi berlaku | Tampilkan tanda "pernah diralat" |
| `isProvisional` | Isinya dapat berubah tanpa riwayat — saat ini hanya asuhan gizi | **Wajib** berlabel "Dapat berubah" |
| `isLateEntry` | Dokumen final **sesudah** tutup klinis kunjungannya | Tampilkan tanda "Susulan" |
| `clinicalClosedAt`, `isClinicalClosureDerived` | Waktu tutup klinis pembanding; `true` bila diturunkan dari data lama | — |
| `periodicSummary` | Hanya pada jenis `43`: `kind`, `date`, `count` | Buka rinciannya lewat endpoint A2.1 |

`GET .../documents/{documentKind}/{documentId}` meneruskan jenis baru ke modul pemiliknya. Isinya
dibaca saat diminta dan **tidak** disimpan rekam medis.

### A1.4 Penutupan kunjungan mencatat waktu tutup klinis

Keenam jalur penutupan (PATCH status kunjungan, antrean dokter, nurse station, penutupan IGD,
pemulangan rawat inap, finalisasi tagihan) kini mencatat **satu** waktu tutup klinis per kunjungan,
yaitu waktu penutupan pertama. Jalur berikutnya tidak menggesernya (`RM-DEC-051`).

| Hal | Keadaan |
|---|---|
| Disposisi IGD ke rawat inap | **Kini mengunci** catatan IGD yang masih konsep dan mencatat tutup klinis dalam transaksi yang sama (`BE-27`) |
| Gagal mencatat tutup klinis | Penutupan pada jalur bertransaksi ikut gagal, sama seperti aturan penguncian rilis pertama (bagian 1.4) |
| Dua penutupan serentak pada PATCH status atau antrean dokter | Yang kedua dapat gagal dengan galat index unik lalu berhasil bila diulang. Kedua jalur itu tidak bertransaksi; menambahkannya butuh persetujuan Registrasi tersendiri |

**Yang harus dilakukan klien:** tidak ada perubahan bentuk. Tanda "Susulan" (`isLateEntry`)
dihitung terhadap waktu tutup klinis ini.

### A1.5 Addendum menerima `purpose`

`POST .../clinical-note-addendums/by-document/{documentKind}/{documentId}` dan `.../as-substitute`

| | |
|---|---|
| **Field baru** | `purpose` opsional: `Correction` (bawaan) atau `Attestation` |
| **`Attestation`** | Penulis mengesahkan isi catatan `LockedUnsigned` tanpa mengubah statusnya (`RM-DEC-049`) |
| **Kode status baru** | `409` bila pengesahan atas catatan yang bukan `LockedUnsigned`, atau catatan sudah disahkan |
| **Klien lama** | Tanpa `purpose` tetap membuat addendum koreksi |

Keterbatasan: addendum yang dibuat lewat `PatientAssessmentController` (`ClinicalManagement`)
selalu bertanda `Correction`, karena controller itu menyusun responsnya sendiri.

## A2. Endpoint baru

### A2.1 Health Services / Medical Record Management / Medical Record — tambahan

| Method | Path | Kegunaan | Hak akses |
|---|---|---|---|
| `GET` | `/{patientId}/periodic-observations` | Rincian satu ringkasan pencatatan berkala. Query `kind` dan `date` (tanggal WIB); nilai tidak sah → `400` | `MedicalRecord : Read` |

### A2.2 Health Services / Medical Record Management / Medical Record Completeness

Base URL: `api/v1/health-services/medical-record-management/medical-record-completeness`

| Method | Path | Kegunaan | Hak akses |
|---|---|---|---|
| `GET` | `/filters/metadata` | Pilihan saringan: keadaan, jenis kunjungan, jenis kekurangan | `MedicalRecordCompleteness : Read` |
| `GET` | `/summary` | Jumlah kunjungan per keadaan | `MedicalRecordCompleteness : Read` |
| `GET` | `/` | Daftar kerja; bawaannya kunjungan yang belum `Complete` | `MedicalRecordCompleteness : Read` |
| `GET` | `/encounters/{encounterId}` | Penilaian tersimpan satu kunjungan beserta seluruh kekurangannya. `404` bila belum pernah dinilai | `MedicalRecordCompleteness : Read` |
| `POST` | `/encounters/{encounterId}/evaluate` | Menilai ulang sekarang. Kunjungan yang tutup sebelum rilis dinilai **tanpa disimpan** (`isPersisted = false`). `409` bila kunjungan masih berjalan, batal, atau `NoShow` | `MedicalRecordCompleteness : Evaluate` |

### A2.3 Health Services / Master Data / Medical Record Completeness Profile

Base URL: `api/v1/health-services/master-data/medical-record-completeness-profiles`

| Method | Path | Kegunaan | Hak akses |
|---|---|---|---|
| `GET` | `/filters/metadata` | Pilihan jenis kunjungan, jenis unit, kondisi | `MedicalRecordCompletenessProfile : Read` |
| `GET` | `/summary` | Jumlah profil aktif dan nonaktif | `MedicalRecordCompletenessProfile : Read` |
| `GET` | `/` | Daftar profil | `MedicalRecordCompletenessProfile : Read` |
| `GET` | `/criteria/options` | Katalog kriteria beserta keterangannya | `MedicalRecordCompletenessProfile : Read` |
| `GET` | `/{id}` | Satu profil beserta butirnya | `MedicalRecordCompletenessProfile : Read` |
| `POST` | `/` | Membuat profil | `MedicalRecordCompletenessProfile : Create` |
| `PUT` | `/{id}` | Mengubah profil dan mengganti butirnya | `MedicalRecordCompletenessProfile : Update` |
| `PATCH` | `/{id}/status` | Aktif atau nonaktif. Profil tidak pernah dihapus | `MedicalRecordCompletenessProfile : Update` |

**Tiga selisih terhadap kontrak `0.2.0` A3** (`BE-37`), seluruhnya ke arah yang lebih sempit:

1. `items[].sortOrder` tidak diterima. Isiannya diabaikan, tidak ditolak; kriteria diurutkan menurut nilai katalog.
2. `/criteria/options` membalas `value`, `name`, `label`, `description`, bukan `OptionResponse` generik.
3. Kode profil kembar dijawab "Kode profil X sudah dipakai profil lain."

## A3. Kelengkapan berkas

| Keadaan | Arti |
|---|---|
| `Complete` | Seluruh syarat terpenuhi |
| `Incomplete` | Ada dokumen wajib yang belum ada, atau catatan yang belum ditandatangani dan belum disahkan |
| `AwaitingResult` | Ada hasil penunjang yang belum dirilis (laboratorium termasuk PA, radiologi) |
| `Undetermined` | Ada sumber yang gagal dibaca atau profilnya tidak ditemukan. **Tidak pernah berarti lengkap** (`RM-DEC-044`) |

| Hal | Aturan |
|---|---|
| Dokumen wajib | Dari master profil: profil dasar menurut jenis kunjungan dan jenis unit, ditambah profil kondisi (`K-OK` operasi, `K-HD` sesi hemodialisa, `K-TRF` transfusi) bila kondisinya ada |
| Kondisi operasi | Ada kasus `Completed`. **Setiap** kasus selesai wajib punya laporan operasi, catatan anestesi, dan checklist Sign In, Time Out, Sign Out (`RM-DEC-076`) |
| Kondisi sesi hemodialisa | Ada sesi yang berakhir (`Stopped`, `Completed`, `AwaitingFinalization`, `Finalized`). **Setiap** sesi itu wajib disahkan |
| Kunjungan `Unknown` | Tidak punya profil → kekurangan `RuleNotFound`, keadaan `Undetermined` |
| Penilaian pertama | Sesudah tutup klinis, lewat antrean di memori, di luar transaksi penutupan |
| Penilaian berkala | Setiap 5 menit, paling banyak 200 kunjungan per putaran. Giliran per kunjungan: 60 menit untuk yang tutup ≤ 30 hari, 24 jam untuk yang lebih lama. Yang sudah `Complete` tidak diperiksa berkala |
| Pemicu penilaian ulang | Tanda tangan dokumen, addendum pengesahan, perubahan profil |
| Konfigurasi | `MedicalRecordCompleteness:RecheckEnabled`, `MedicalRecordCompleteness:RecheckIntervalSeconds` |
| Dampak pada alur lain | **Tidak ada.** Penutupan, pemulangan, dan tagihan tidak pernah tertahan berkas yang belum lengkap (`RM-DEC-046`) |

## A4. Hak akses baru

Terdaftar otomatis. Peran yang belum diberi izin tidak melihat perubahan apa pun.

| Izin | Untuk |
|---|---|
| `MedicalRecordCompleteness : Read` | Daftar kerja dan rincian kelengkapan berkas |
| `MedicalRecordCompleteness : Evaluate` | Menilai ulang satu kunjungan |
| `MedicalRecordCompletenessProfile : Read` / `Create` / `Update` | Master profil dokumen wajib |

`MedicalRecord : Read` **tidak berubah**, tetapi isinya kini lebih luas: dokumen final seluruh
modul ikut terbaca. Tinjau ulang siapa yang memegangnya.

## A5. Yang WAJIB disiapkan sebelum rilis dipakai

| Butir | Keadaan | Akibat bila belum siap |
|---|---|---|
| Migration `20261028070658_AddMedicalRecordCompletenessFoundation` | **Sudah diterapkan di devYoga** 28 Oktober 2026. Lingkungan lain belum; penerapannya butuh instruksi terpisah | Penutupan kunjungan pada jalur bertransaksi gagal karena tabel tutup klinis tidak ada |
| Sebelas profil dokumen wajib awal | Diisi seeder `MedicalRecordCompletenessProfileSeeder` saat aplikasi mulai; idempoten | Tanpa profil, setiap kunjungan `Undetermined` (`RuleNotFound`) |
| Penyelarasan profil dengan SOP rumah sakit | **Belum.** Isi awal mengikuti `RM-DEC-047`, `053`–`056`, `060`, `061`, **bukan** SOP | Daftar kerja menuntut dokumen menurut rancangan, belum menurut kebijakan rumah sakit |
| Pemberian izin A4 | Belum | Menu kelengkapan dan master profil tidak terlihat |
| Penilaian ulang manual kunjungan beroperasi atau ber-HD | Disarankan, lihat A7 butir 3 | Kunjungan yang dulu `Complete` karena asumsi sementara tetap `Complete` |
| Layar `FE-10`–`FE-14` | Direncanakan, berjalan paralel dengan API tiruan | Penanda baru (A1.3) tidak tampil sampai layarnya dirilis |

## A6. Yang TIDAK berubah

| Hal | Keadaan |
|---|---|
| Tabel modul sumber | **Nol** kolom baru. Setiap pembaca hanya satu berkas service di modul pemiliknya, ditambah pendaftaran DI, dan hanya membaca |
| Alur modul sumber | Tidak berubah, kecuali satu jalur IGD (`BE-27`, A1.4) |
| Bentuk respons yang sudah ada | Hanya bertambah field |
| Data klinis | Tidak disalin ke rekam medis; dibaca saat diminta |

## A7. Keterbatasan yang wajib diketahui

| No | Keterbatasan | Penanganan |
|---:|---|---|
| 1 | **Hasil Patologi Anatomi belum tersedia** di berkas (`RM-DEC-058`). Pemeriksaan PA yang belum dirilis tetap dihitung "menunggu hasil" | Tampil di `unavailableKinds` |
| 2 | **Asuhan gizi dapat berubah tanpa riwayat** (`RM-DEC-073`); waktu finalnya waktu simpan pertama, sehingga perubahan sesudah tutup klinis tidak bertanda susulan | `isProvisional = true`, wajib berlabel "Dapat berubah" |
| 3 | **Kunjungan yang dulu `Complete` karena asumsi sementara tidak dinilai ulang otomatis.** Sebelum `BE-31`/`BE-32`, kondisi operasi dan sesi hemodialisa dianggap tidak ada | Nilai ulang lewat `POST .../encounters/{encounterId}/evaluate`, atau terima apa adanya |
| 4 | **Golongan darah tidak terikat kunjungan.** Ia tidak muncul bila linimasa disaring per kunjungan | Tampilkan golongan darah di luar penyaring kunjungan, mis. di kepala berkas |
| 5 | **Golongan darah tervalidasi belum tentu berlaku.** Ringkasannya membawa keterangan "berlaku", "ditahan", atau "tidak berlaku lagi" dari Bank Darah. Berkas **bukan** sumber golongan darah untuk transfusi | Tampilkan keterangannya utuh |
| 6 | Antrean penilaian pertama hilang saat aplikasi restart | Disusul putaran berkala |
| 7 | Dokumen yang didaftarkan langsung bertanda tangan (sesi HD, hasil lab) tidak memicu penilaian ulang seketika | Disusul putaran berkala, paling lambat 60 menit |
| 8 | Label kekurangan tidak merinci bagian: checklist tanpa nama fase; sesi HD berbunyi "Catatan sesi hemodialisa ditandatangani belum ada." | Rincian ada di linimasa; perbaikan teks milik master profil |
| 9 | Resume MCU yang sedang dikoreksi hanya ditandai lewat teks ringkasan, belum lewat field | Calon field `isUnderCorrection` pada kontrak berikutnya |
| 10 | Kunjungan `NoShow` diperlakukan sama dengan batal (`409` saat dinilai) | Belum tercatat di validation matrix |

## A8. Keterangan di Swagger

Proyek ini tidak memuat keterangan tingkat controller ke Swagger (`includeControllerXmlComments`
dimatikan, `Program.cs`), dan action tidak memakai `<summary>` XML. Karena itu keterangan amandemen
diletakkan pada **schema** — field `unavailableKinds`, `encounterId`, `isProvisional`, `isCorrected`,
`finalizedAt`, `sourceModule` pada linimasa, dan `state` pada kelengkapan. Keterangan itu dibuktikan
terbit lewat uji `ReleaseContractTests`.

## A9. Status persetujuan

**Catatan rilis amandemen ini belum disahkan.** Kontraknya sudah (`RM-DEC-062`); yang belum adalah
pernyataan bahwa butir A1, A5, dan A7 diterima untuk diumumkan kepada pemakai.

| Peran | Nama | Tanggal setuju |
|---|---|---|
| Pemilik API | Yoga Aji Pratama | — |
| Pemilik frontend | Yoga Aji Pratama | — |

---

# Rilis pertama `0.1.x` — 27 Agustus 2026

| Field | Isi |
|---|---|
| Modul | Rekam Medis (`RM-BP-001`) |
| Tanggal disusun | 27 Agustus 2026 |
| Disusun pada task | `BE-18` |
| Status persetujuan | **Disetujui** 27 Agustus 2026 — Yoga Aji Pratama selaku pemilik API (`RM-DEC-028`) |
| Dampak skema | **Aditif.** Lima tabel baru; **nol perubahan kolom** pada tabel yang sedang dipakai |
| Dampak perilaku | **Empat perubahan** pada endpoint yang sudah berjalan — lihat bagian 1 |

> **BACA BAGIAN 1 LEBIH DULU.** Bagian itu memuat perubahan yang **tidak terlihat** dari bentuk
> permintaan maupun responsnya. Klien yang tidak membacanya dapat mengira permintaannya berhasil
> padahal sebagian nilainya diabaikan.

---

## 1. Perubahan perilaku pada endpoint yang sudah berjalan

Empat perubahan berikut menyentuh endpoint yang sudah dipakai. **Tidak ada perubahan pada bentuk
permintaan maupun respons** — yang berubah hanya perilakunya.

### 1.1 `PUT` CPPT menolak catatan yang sudah terkunci

`PUT api/v1/health-services/clinical-management/patient-integrated-progress-notes/{id}`

| | |
|---|---|
| **Sebelumnya** | Catatan dapat diubah kapan saja, tanpa batas |
| **Sekarang** | Catatan yang sudah ditandatangani, atau terkunci karena kunjungannya ditutup, dijawab `400` |
| **Pesan** | *"Catatan ini sudah ditandatangani dan tidak dapat diubah. Gunakan addendum untuk membetulkan."* |

**Yang harus dilakukan klien:** menangani kode `400` baru ini, dan mengarahkan pengguna ke jalur
addendum. Pembetulan catatan terkunci memang dilakukan lewat addendum, bukan dengan mengubah
isinya — riwayat koreksi harus tetap terbaca.

### 1.2 `ProviderUserId` diabaikan pada permintaan ubah CPPT

| | |
|---|---|
| **Sebelumnya** | Nilai dari permintaan menimpa penulis catatan |
| **Sekarang** | **Diabaikan.** Penulis ditetapkan sekali saat catatan dibuat |
| **Apakah ditolak?** | **Tidak.** Permintaan tetap berhasil, nilainya sekadar tidak berpengaruh |

### 1.3 `IsReadOnlyGenerated` diabaikan pada permintaan ubah CPPT

| | |
|---|---|
| **Sebelumnya** | Nilai dari permintaan dapat melepas penanda hanya-baca |
| **Sekarang** | **Diabaikan.** Penanda hanya-baca tidak dapat dilepas lewat permintaan ubah |
| **Apakah ditolak?** | **Tidak.** Permintaan tetap berhasil, nilainya sekadar tidak berpengaruh |

**Kenapa diabaikan, bukan ditolak.** Menolak permintaan akan memutus frontend yang sedang
berjalan. Mengabaikan nilai menutup celahnya tanpa memutus siapa pun.

**Tetapi mengabaikan kiriman klien tanpa pemberitahuan bukan praktik yang baik.** Itulah sebabnya
kedua perubahan ini dinyatakan di sini dan pada halaman Swagger endpoint-nya — bukan didiamkan.

**Yang harus dilakukan klien:** berhenti mengandalkan kedua kolom itu pada permintaan ubah. Bila
frontend menampilkan nilainya sebagai kolom yang dapat disunting, kolom itu perlu dijadikan
hanya-baca supaya pengguna tidak mengira suntingannya tersimpan.

### 1.4 `PATCH` status kunjungan mengunci dokumen yang masih terbuka

`PATCH api/v1/health-services/registration-management/patient-encounters/{id}/status`

| | |
|---|---|
| **Sebelumnya** | Status kunjungan berubah, dokumen klinisnya tidak tersentuh |
| **Sekarang** | Ketika status berpindah **menuju** `Completed`, seluruh catatan berstatus draf pada kunjungan itu dikunci |
| **Akibat baru** | **Penutupan kunjungan kini dapat gagal**, yaitu bila penguncian gagal. Bila itu terjadi, status kunjungan **tidak** ikut berubah |

**Yang harus dilakukan klien:** menangani kemungkinan penutupan kunjungan gagal, dan tidak
menganggap keberhasilannya sudah pasti.

Kenapa keduanya harus gagal bersamaan: kunjungan yang tertutup sementara dokumennya masih terbuka
adalah keadaan yang dilarang. Catatan yang tertinggal terbuka tidak akan pernah terkunci lagi,
karena pemicunya — penutupan kunjungan — sudah lewat.

Perlu diketahui pula: endpoint ini **tidak** memvalidasi perpindahan status, sehingga status
dapat melompat dari nilai mana pun ke `Completed`. Penguncian karena itu dipicu oleh perpindahan
**menuju** `Completed`, bukan oleh urutan tertentu. Ini bukan perbaikan atas celah tersebut —
celah itu tetap terbuka.

---

## 2. Cakupan aturan keutuhan pada rilis ini

**Baru CPPT yang tunduk aturan keutuhan rekam medis.**

Dua belas jenis dokumen klinis lain — konsultasi, asesmen, diagnosis, tindakan, tanda vital,
alergi, riwayat penyakit, riwayat keluarga, dokumen klinis, lampiran, surat keterangan, dan
persetujuan tindakan — sudah punya nomor jenisnya, tetapi aturannya **belum ditegakkan** pada
rilis ini.

Akibatnya untuk kedua belas jenis itu:

| Hal | Keadaan |
|---|---|
| Penandatanganan dan penguncian | Belum berlaku |
| Pemeriksaan sebelum perubahan | Belum berlaku — dokumen masih dapat diubah bebas |
| Status keutuhan pada balasan API | Kosong, disertai penanda bahwa jenis itu belum tunduk aturan |

**Yang harus dilakukan klien:** setiap baris riwayat dan detail dokumen membawa penanda apakah
jenisnya sudah tunduk aturan keutuhan. **Penanda itu wajib ditampilkan.** Menampilkan alergi
seolah-olah sudah terlindungi aturan keutuhan akan membuat pembacanya mempercayai dokumen yang
sebenarnya masih dapat diubah bebas.

---

## 3. Yang baru: endpoint berkas rekam medis

Seluruhnya endpoint baru; tidak menyentuh endpoint yang sudah ada.

### Health Services / Medical Record Management / Medical Record

Base URL: `api/v1/health-services/medical-record-management/medical-records`

| Method | Path | Kegunaan | Hak akses |
|---|---|---|---|
| `GET` | `/{patientId}/summary` | Ringkasan berkas: identitas, alergi aktif, diagnosis aktif, jumlah dokumen per jenis | `MedicalRecord : Read` |
| `GET` | `/{patientId}/timeline` | Riwayat gabungan tiga belas sumber, urut waktu | `MedicalRecord : Read` |
| `GET` | `/{patientId}/documents/{documentKind}/{documentId}` | Detail dokumen beserta addendum | `MedicalRecord : Read` |
| `GET` | `/{patientId}/documents/{documentKind}/{documentId}/private-note` | Membuka catatan pribadi klinisi | `MedicalRecord : ReadPrivateNote` |
| `GET` | `/filters/metadata` | Daftar pilihan penyaring dan keperluan akses | `MedicalRecord : Read` |

Grup lain yang bertambah: **Clinical Document Integrity**, **Clinical Note Addendum**,
**Clinical Note Author Delegation**, **Medical Record Access Log**, dan **Medical Record Access
Purpose**.

### Tiga aturan yang mengikat seluruh endpoint berkas rekam medis

**1. Setiap pembukaan dicatat lebih dulu.** Jejak akses ditulis **sebelum** isi dikembalikan.
Bila pencatatan gagal, permintaan dijawab `503` dan isi **tidak** dikembalikan. Ini pilihan yang
menutup rapat: membaca diam-diam dinilai lebih berbahaya daripada tidak bisa membaca.

**2. Pasien di luar rawatan menuntut keperluan akses.** Bila pasien tidak sedang punya kunjungan
berjalan, `accessPurposeId` wajib diisi. Tanpa itu, permintaan dijawab `400`.

**3. Catatan pribadi selalu menuntut keperluan akses** — bahkan untuk pasien yang sedang dirawat
pengguna, dan dengan izin yang terpisah dari izin baca biasa.

### Kode status yang perlu ditangani klien

| Kode | Arti |
|---|---|
| `400` | Keperluan akses belum dipilih, atau penjelasannya belum diisi |
| `403` | Tidak punya hak akses ke menu rekam medis |
| `404` | Pasien tidak ditemukan, atau dokumen bukan milik pasien itu |
| `409` | Pasien hasil penggabungan nomor rekam medis — buka nomor penggantinya |
| `503` | Jejak akses gagal dicatat, sehingga isi tidak dikembalikan. Coba lagi |

### Perubahan bentuk balasan riwayat

Balasan `/timeline` **tidak** berbentuk `PagedResult` langsung. Ia membungkusnya:

```
data.page.items      ← isi halaman
data.page.totalData  ← jumlah seluruhnya
data.access          ← keterangan pembukaan yang baru saja terjadi
data.failedSources   ← sumber yang gagal dibaca. Kosong berarti lengkap
data.isTruncated     ← ada sumber yang datanya melampaui batas
data.isComplete      ← ringkasan dua penanda di atas
```

Alasannya: riwayat digabung dari tiga belas sumber, dan bila satu sumber gagal dibaca,
kekurangannya **wajib dinyatakan**. Bentuk `PagedResult` tidak punya tempat untuk itu — dan daftar
yang kurang satu jenis dokumen yang terbaca sebagai daftar lengkap adalah kekeliruan paling
berbahaya pada berkas rekam medis.

**Yang harus dilakukan klien:** membaca `data.page.items`, dan menampilkan peringatan bila
`data.isComplete` bernilai `false`.

---

## 4. Hak akses baru

Terdaftar otomatis. Peran yang belum diberi izin tidak melihat perubahan apa pun.

| Izin | Untuk |
|---|---|
| `MedicalRecord : Read` | Membuka berkas rekam medis |
| `MedicalRecord : ReadPrivateNote` | Membuka catatan pribadi klinisi |
| `ClinicalDocumentIntegrity : Read` / `Update` | Melihat dan menandatangani keutuhan dokumen |
| `ClinicalNoteAddendum : Read` / `Create` / `CreateAsSubstitute` | Koreksi catatan terkunci |
| `ClinicalNoteAuthorDelegation : Read` / `Create` / `Update` | Penetapan penulis berhalangan |
| `MedicalRecordAccessLog : Read` / `Update` | Tinjauan jejak akses |

**Dua izin yang menuntut kehati-hatian khusus:**

`MedicalRecord : ReadPrivateNote` **tidak boleh** diberikan seluas hak baca rekam medis. Bila
diberikan seluas itu, seluruh pengaman catatan pribadi kehilangan artinya.

`MedicalRecordAccessLog : Read` menampilkan alasan akses, yang dapat mengungkap keadaan pasien —
misalnya *"konsultasi kejiwaan"*. Haknya harus lebih sempit daripada hak baca rekam medis.

---

## 5. Yang WAJIB disiapkan sebelum modul dipakai

**Skema database sudah diterapkan 27 Agustus 2026** pada `QuilvianNewDevTim01`: lima tabel baru,
nol perubahan kolom pada tabel yang sedang dipakai. Rinciannya pada `roadmap/backend-roadmap.md`
bagian "Keadaan database". Yang berikut ini adalah hal-hal yang **belum** siap.

| Butir | Akibat bila belum siap |
|---|---|
| **Isi master keperluan akses** (`MstMedicalRecordAccessPurpose`) | **Seluruh** pembukaan rekam medis pasien di luar rawatan akan ditolak — bukan karena kesalahan pengguna, melainkan karena tidak ada keperluan yang dapat dipilih. **Keadaan ini berlaku sekarang:** tabelnya sudah ada, isinya masih nol |
| **Pemberitahuan kepada penulis CPPT** | Penulis CPPT selama ini menganggap kolom Catatan Pribadi sepenuhnya pribadi. Mereka berhak tahu bahwa kolom itu dapat dibuka lewat jalur sah. Bahannya pada `roadmap/BE-15-pemberitahuan-penulis-cppt.md` |
| **Penetapan siapa yang berhak `ReadPrivateNote`** | Lihat bagian 4 |
| **Pemeriksaan jumlah pasien bernomor rekam medis ganda** | Bila ada, berkas mereka tidak dapat dibuka lewat nomor lama sejak modul dipakai. Jumlahnya perlu diketahui agar unit rekam medis tidak terkejut |

Endpoint `/filters/metadata` mengembalikan penanda `isAccessPurposeMasterEmpty` beserta
peringatan tegas bila master keperluan akses masih kosong, supaya pengguna tidak mengira
penolakan akses adalah kesalahannya sendiri.

---

## 6. Yang TIDAK berubah

Dinyatakan supaya tidak ada yang mengira harus melakukan sesuatu.

| Hal | Keadaan |
|---|---|
| Kolom pada tiga belas tabel klinis | **Nol perubahan.** Alur IGD, antrean dokter, dan farmasi tidak tersentuh |
| Bentuk permintaan dan respons endpoint yang sudah ada | Tidak berubah |
| Empat model status lama pada dokumen klinis | Tetap berlaku. Status keutuhan berjalan berdampingan, bukan menggantikan |
| Isi data klinis | Tidak dipindahkan, tidak disalin, tidak diubah |

---

## 7. Status persetujuan

**Catatan rilis ini sudah disetujui**, beserta kontrak API `0.1.0` yang naik dari `draft` menjadi
`approved`. Dicatat pada `RM-DEC-028`.

Dua hal yang secara khusus ikut disahkan:

1. Perubahan bentuk balasan `/timeline` pada bagian 3.
2. Field `access` yang ditambahkan pada seluruh balasan endpoint berkas rekam medis.

| Peran | Nama | Tanggal setuju |
|---|---|---|
| Pemilik API | Yoga Aji Pratama | 27 Agustus 2026 |
| Pemilik frontend | Yoga Aji Pratama | 27 Agustus 2026 |
| Pemilik proses / clinical governance / keamanan | Yoga Aji Pratama | 26 Agustus 2026 (`RM-DEC-027`) |

**Batas yang tetap berlaku.** Sama seperti `RM-DEC-027`: tinjauan komite medik atas aturan
penguncian, addendum, dan penetapan berhalangan, serta tinjauan pihak perlindungan data atas
kewenangan `SuperAdmin`, tanda tangan elektronik, catatan pribadi, dan masa simpan jejak akses
**belum dilakukan**. Bila kedua pihak itu kelak ditunjuk dan menghasilkan keputusan berbeda,
bagian yang bergantung padanya wajib dirombak. Risiko ini diterima secara sadar.

**Akibat langsung pengesahan ini:** gerbang paralel frontend terbuka. Sepuluh task `FE-00`
sampai `FE-09` tidak lagi tertahan kontrak.
