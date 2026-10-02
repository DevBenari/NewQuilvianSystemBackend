# Laporan Perubahan Backend — `BE-RWI-057`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-057` |
| Judul | Pengkajian final dibetulkan lewat koreksi, bukan dengan menimpanya |
| Slice | Gelombang `KEP-MVP-1` |
| Roadmap | [`../../../roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 4 |
| Trace | `FR-KEP-008`, `FR-KEP-009`; `RWI-DEC-091`, `RWI-DEC-087`; PRD 16.2 aturan 12 dan 13, 27.3 aturan 7; `AC-CAP012-05`, `RWI-AC-175`; `VAL-KEP-12`; `RWI-FACT-013` |
| Contract version | API `0.3.0` `POST /{id}/addendums` dan `GET /{id}/addendums`; state transition `0.3.0` bagian 1; permission `PatientAssessment : Amend` |
| Dependency | `BE-RWI-065` — selesai, lihat laporannya |
| Klasifikasi | `MEDIUM` — repository 1 (0), berkas diperiksa ≤ 8 (0), berkas diubah ≤ 3 (0), logika bisnis sedang (1), memakai kontrak yang sudah ada (1), tidak ada dampak database (0), keamanan berkaitan (1), workflow terbatas (1). Total **4** |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi, uji, dan `docs/module-blueprints/rawat-inap/keperawatan/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | Garis dasar `7d4bf2b91d39265eab4453a230ba324f95866962`, branch `MHamzah`; commit `0a63358` dibuat pemilik repository di tengah pengerjaan |
| Tanggal | 6 September 2026 |
| Status | **Selesai.** Ketujuh acceptance criteria terbukti. **Nol tabel baru dan migration kosong** |

---

## 1. Masalah yang diperbaiki

Perawat salah mengisi. Itu kejadian biasa, dan sistem yang baik menyediakan jalan
membetulkannya — jalan yang **meninggalkan jejak**. Sebelum perubahan ini, pengkajian yang sudah
diselesaikan tidak punya jalan itu sama sekali: penyuntingan langsung ditolak (sejak `BE-RWI-065`),
dan tidak ada endpoint koreksi pada grup pengkajian.

Godaan terbesarnya adalah membuat penyimpanan koreksi sendiri di `ClinicalManagement` karena terasa
lebih cepat. Itu **mesin koreksi tandingan**, dilarang `RWI-DEC-087` dan justru dihindari
`RWI-DEC-091`: satu lembar rekam medis akan memuat dua bentuk koreksi yang berbeda, dan pertanyaan
"apakah dokumen ini pernah dikoreksi" akan punya dua sumber jawaban.

> **Contoh nyatanya.** Ns. Sari menyelesaikan pengkajian awal Tn. Budi pukul 09.00. Pukul 10.00 ia
> menyadari skala nyerinya tertulis 3 padahal seharusnya 7. Yang benar adalah: isi pengkajian
> **tidak berubah sedikit pun**, koreksinya tercatat sebagai catatan bernomor 1 atas namanya
> beserta alasan dan waktunya, dan status pengkajian **tetap** `Completed`.

---

## 2. Proses bisnis

**Tujuan.** Pengkajian final dapat dibetulkan lewat koreksi beralasan; isi asli tetap tersimpan
sebagai bukti klinis, dan tidak ada jalan menghapusnya diam-diam.

**Pelaku.** Penulis pengkajian.

**Langkah yang berurutan.**

1. Perawat membuka pengkajian yang sudah selesai dan menyadari ada yang keliru.
2. Ia menambahkan koreksi: apa yang seharusnya tertulis, beserta alasannya.
3. Sistem menyimpan koreksi itu sebagai **addendum bernomor urut** pada mesin keutuhan dokumen
   milik `MedicalRecordManagement` — bukan pada tabel mana pun milik `ClinicalManagement`.
4. Isi pengkajian **tidak tersentuh**. Statusnya **tetap** `Completed`.
5. Koreksi berikutnya menambah nomor urut berikutnya. Statusnya tetap `Completed` sesudah berapa
   kali pun dikoreksi.

**Jalur tidak normal.**

| Keadaan | Yang terjadi | Kode |
| --- | --- | --- |
| Alasan koreksi kosong | Ditolak: *"Alasan koreksi wajib diisi."* — `VAL-KEP-12` | `400` |
| Isi koreksi kosong | Ditolak: *"Isi koreksi wajib diisi."* | `400` |
| Koreksi pada pengkajian yang masih konsep | Ditolak: *"Catatan ini belum final. Perbaiki langsung pada catatannya."* — `RWI-FACT-013` | `400` |
| Pengguna bukan penulis pengkajian | Ditolak: *"Hanya penulis catatan yang dapat menambahkan koreksi."* | `403` |
| Pengkajian tidak ditemukan | Ditolak | `404` |

**Hasil akhir.** Rekam medis memuat isi asli **dan** koreksinya, berdampingan, lengkap dengan
siapa, kapan, dan mengapa.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa |
| --- | --- |
| `roadmap/backend-roadmap.md` bagian 2.3 dan bagian 5.2 | Butir "pintu kedua endpoint koreksi" yang wajib dibaca sebelum menulis controller |
| `Areas/HealthServices/MedicalRecordManagement/Services/ClinicalNoteAddendumService.cs` | `CreateAsync`, `ListByDocumentAsync`, dan `ResolveAuthorityAsync` |
| `Areas/HealthServices/MedicalRecordManagement/Controllers/ClinicalNoteAddendumController.cs` | Endpoint generik yang sudah ada beserta pola penerusannya |
| `Areas/HealthServices/ClinicalManagement/Services/InpatientDocumentCorrectionAuthorityService.cs` | Penjaga koreksi atas nama penulis lain milik Rawat Inap |
| `contracts/api-contract.md` bagian 1 dan `contracts/permission-audit-matrix.md` bagian 2 | Bentuk endpoint dan pemetaan hak akses |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` | Dua endpoint baru — `POST /{id}/addendums` dan `GET /{id}/addendums` — yang **meneruskan** ke `ClinicalNoteAddendumService` dengan jenis dokumen `Assessment`; hak akses `PatientAssessment : Amend`; helper penyusun balasan beserta nama penulisnya |
| `Areas/HealthServices/ClinicalManagement/DTOs/NursingAssessmentMonitoringDtos.cs` | `CreateAssessmentAddendumRequest` — `Content` dan `Reason`, keduanya wajib. Penulis, waktu, perangkat, dan alamat jaringan **tidak** diterima dari klien |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/NursingAssessmentIntegrityTests.cs` | **Berkas baru**, dipakai bersama `BE-RWI-065`. Sembilan uji di antaranya milik task ini |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Dua endpoint baru** pada grup Patient Assessment. Nol endpoint lama berubah |
| Database | **`NOT APPLICABLE`.** Nol tabel, nol kolom, nol index, nol nilai enum. **Migration task ini kosong** — koreksi seluruhnya tersimpan pada `MrcClinicalDocumentIntegrity` dan `MrcClinicalNoteAddendum` yang sudah ada |
| Keamanan/Auth | Satu action baru `Amend` pada Resource `PatientAssessment` yang sudah ada, ber-`AccessType` `Update` sehingga muncul di layar Akses Role. Kepemilikan data tetap dijaga aturan bisnis: hanya penulis pengkajian yang dapat mengoreksinya |

---

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Assessment

Base URL: `api/v1/health-services/clinical-management/patient-assessments`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id}/addendums` | Menambahkan koreksi pada pengkajian yang sudah selesai. Isi asli tidak berubah; status tetap `Completed` | `PatientAssessment : Amend` |
| `GET` | `/{id}/addendums` | Daftar koreksi satu pengkajian, terurut nomor | `PatientAssessment : Read` |

**Bentuk permintaan `POST`.**

| Field | Wajib | Keterangan |
| --- | :---: | --- |
| `Content` | Ya | Isi koreksinya: apa yang seharusnya tertulis |
| `Reason` | Ya | Alasan koreksi — `VAL-KEP-12` |

**Kode status yang perlu ditangani layar.**

| Kode | Artinya bagi pengguna |
| --- | --- |
| `201` | Koreksi tersimpan |
| `400` | Alasan atau isi kosong, atau pengkajiannya belum final |
| `403` | Anda bukan penulis pengkajian ini |
| `404` | Pengkajian tidak ditemukan |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln -m:1` | `Build succeeded. 0 Error(s)` | `PASS` | Keluaran perintah |
| `dotnet test Tests/QuilvianSystemBackend.UnitTests.Sqlite` | `Failed: 0, Passed: 394, Skipped: 0, Total: 394` | `PASS` | Keluaran perintah |
| `dotnet test Tests/QuilvianSystemBackend.Tests` | `Failed: 0, Passed: 288, Skipped: 0, Total: 288` | `PASS` | Keluaran perintah |
| `dotnet test Tests/QuilvianSystemBackend.UnitTests.InMemory` | `Failed: 9, Passed: 917, Skipped: 0, Total: 926` — kesembilan kegagalan seluruhnya pada `BillingManagement` | `EXISTING / ENVIRONMENT ISSUE` | **Direproduksi pada working tree bersih** di commit `7d4bf2b`, tanpa satu pun perubahan slice ini: `Failed: 9, Passed: 915, Total: 924` dengan nama uji yang sama persis. Nol berkas Billing disentuh slice ini |
| `Koreksi_MenyimpanJejakTanpaMengubahIsiAsli` | Nomor 1, penulis, alasan, dan waktu tersimpan; skala nyeri asli tetap 3; status tetap `Completed` | `PASS` | Uji |
| **Skenario koreksi berulang**: `KoreksiKedua_MenambahNomorUrutDanStatusTetapCompleted` | Nomor urut `[1, 2]`; status tetap `Completed` | `PASS` | Uji |
| **Skenario koreksi tanpa alasan**: `KoreksiTanpaAlasan_Ditolak400` | `400`; nol addendum terbentuk | `PASS` | Uji |
| **Skenario koreksi pada pengkajian konsep**: `KoreksiPadaPengkajianKonsep_DitolakDenganArahanSuntingLangsung` | `400`; pesannya menyebut "belum final" | `PASS` | Uji |
| **Percobaan hard-delete**: `GrupPengkajian_TidakPunyaEndpointPenghapusan` | Nol endpoint `DELETE` pada grup ini | `PASS` | Uji refleksi |
| **Percobaan membuka kembali pengkajian final**: `StatusAmended_TidakAdaPadaMesinStatusPengkajian` | `PatientAssessmentStatus` tetap empat nilai; nol `Amended`; nol endpoint yang memindahkan `Completed` ke `Draft` | `PASS` | Uji; ditambah `MenyuntingPengkajianTerkunci_Ditolak400DenganArahanKoreksi` milik `BE-RWI-065` |
| `Koreksi_TidakMenulisPadaTabelClinicalManagement` | Jumlah baris pengkajian dan catatan terpadu tidak berubah; `UpdateDateTime` pengkajian tidak bergerak; satu baris addendum terbentuk pada `MedicalRecordManagement` | `PASS` | Uji |
| `DaftarKoreksi_TerbacaTerurutNomor` | Dua koreksi terbaca lewat endpoint bacanya | `PASS` | Uji |
| `KoreksiPadaPengkajianTidakDikenal_Ditolak404` | `404` | `PASS` | Uji |
| **Pemeriksaan migration task ini kosong** | Nol berkas migration dibuat untuk task ini | `PASS` | Bagian 3.3 dan bagian 7 |
| Kontrak penamaan hak akses `Amend` | Ketiga nilai cocok huruf demi huruf; `AccessType` `Update` | `PASS` | Uji `NursingAssessmentAccessContractTests` |

Uji manual: `NOT APPLICABLE`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Koreksi menyimpan aktor, waktu, alasan, dan nomor urut; isi pengkajian asli tidak berubah sedikit pun | Terpenuhi | Uji `Koreksi_MenyimpanJejakTanpaMengubahIsiAsli` |
| 2. Alasan kosong ditolak `400` | Terpenuhi | Uji `KoreksiTanpaAlasan_Ditolak400` |
| 3. Status pengkajian tetap `Completed` sesudah koreksi, dan tetap `Completed` sesudah koreksi kedua | Terpenuhi | Uji `KoreksiKedua_MenambahNomorUrutDanStatusTetapCompleted` |
| 4. Koreksi pada pengkajian `Draft` ditolak dengan arahan membetulkan langsung | Terpenuhi | Uji `KoreksiPadaPengkajianKonsep_DitolakDenganArahanSuntingLangsung` |
| 5. Transisi `Completed → Draft` dan `Completed → Amended` ditolak | Terpenuhi | Nilai `Amended` **tidak ada** pada enum — uji `StatusAmended_TidakAdaPadaMesinStatusPengkajian`; nol endpoint memindahkan `Completed` kembali ke `Draft`, dan penyuntingan dokumen terkunci ditolak `400` |
| 6. Pengkajian final tidak dapat dihapus | Terpenuhi | Uji `GrupPengkajian_TidakPunyaEndpointPenghapusan` |
| 7. Koreksi tidak menulis baris pada tabel mana pun milik `ClinicalManagement` | Terpenuhi | Uji `Koreksi_TidakMenulisPadaTabelClinicalManagement` |

**Definition of Done.**

| Butir | Status |
| --- | --- |
| Dua endpoint yang meneruskan ke mesin yang sudah ada | Terpenuhi |
| Hak akses terpasang | Terpenuhi — `PatientAssessment : Amend` |
| Ketujuh acceptance criteria terbukti | Terpenuhi |
| Nol tabel baru dan migration kosong | Terpenuhi |
| `dotnet build` lulus | Terpenuhi |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Butir "pintu kedua" | Roadmap bagian 2.3 mengangkat pertanyaan apakah endpoint koreksi per sumber daya memang diinginkan, mengingat mesin generiknya sudah ada. Task ini mengikuti kontrak `0.3.0` apa adanya **dengan syarat penerusan**: kedua endpoint tidak menyimpan apa pun sendiri, melainkan memanggil `ClinicalNoteAddendumService`. Keputusan bentuk pintunya tetap milik pemilik kontrak lewat `/qv-design` |
| Delta kontrak | **Bentuk balasan memakai `ClinicalNoteAddendumResponse`, bukan `ClinicalDocumentAddendumResponse`.** Kontrak menyebut nama kedua; source sudah memiliki bentuk pertama dan memakainya pada endpoint generik. Membuat bentuk kedua berarti satu konsep punya dua bentuk balasan, dan layar yang berpindah antar sumber harus menulis ulang pembacanya |
| Delta kontrak | **`POST` dijawab `201`**, mengikuti endpoint addendum generik yang sudah ada. Kontrak tidak menyebut kodenya secara eksplisit untuk endpoint ini |
| Butir terbuka | **Jalur koreksi oleh kepala ruangan belum dibuka pada endpoint ini.** `VAL-KEP-07` menyebut catatan final dapat dikoreksi penulisnya **atau kepala ruangan**. Endpoint yang dibuat task ini hanya melayani jalur penulis (`actorHasSubstituteAuthority: false`), persis seperti endpoint generik `POST /clinical-note-addendums/by-document/{kind}/{id}`. Jalur pengganti sudah tersedia lewat endpoint `as-substitute` milik `MedicalRecordManagement`, dan aturannya — penetapan berhalangan, akun penulis nonaktif — **dimiliki modul itu**, bukan sub-modul ini. Membuka jalur kedua di sini berarti mengarang kebijakan kewenangan yang belum diputuskan. Tidak satu pun acceptance criteria task ini menuntutnya. **Diangkat sebagai butir terbuka** kepada pemilik kontrak |
| Risiko tersisa | Koreksi hanya dapat ditambahkan pada pengkajian yang **sudah terdaftar** pada mesin keutuhan, yaitu pengkajian rawat inap yang diselesaikan sejak `BE-RWI-065`. Pengkajian rawat inap yang sudah `Completed` sebelumnya perlu pengisian data lama sebelum dapat dikoreksi |
| Perubahan sampingan | `NONE` |
| Interupsi | Sesi sempat terputus; pemulihan mengikuti `TASK_RULES.md`. Nol penyuntingan ganda |
| Status Git | Perubahan task ini masih di working tree; **agent tidak menjalankan satu pun** `stage`, `commit`, `push`, `pull`, `merge`, `rebase`, maupun deployment |
| Langkah berikutnya | `BE-RWI-058` menampilkan nomor koreksi pada lini masa; `FE-RWI-052` membangun layarnya |
