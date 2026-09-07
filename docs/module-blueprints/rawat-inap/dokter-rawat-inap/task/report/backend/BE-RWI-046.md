# Laporan Perubahan Backend — `BE-RWI-046`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-046` |
| Judul | Catatan harian terbaca menurut waktu pemeriksaan yang sebenarnya |
| Slice | `DOK-MVP-3` — catatan harian |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/backend-roadmap.md`, task `BE-RWI-046` |
| Trace | `EPIC DOK-03`; `FR-DOK-012`, `FR-DOK-013`, `FR-DOK-014`; `contracts/api-contract.md` §1; `VAL-DOK-12`, `VAL-DOK-13`, `VAL-DOK-14`; `AC-CAP020-01` |
| Contract version | `0.3.0`, `APPROVED` Muhammad Hamzah 3 September 2026 |
| Dependency | `BE-RWI-044` **selesai** ([laporan](BE-RWI-044.md)) |
| Klasifikasi | `MEDIUM`, skor 7: repository 0, berkas diperiksa 1, berkas diubah 1, logika bisnis 2, kontrak API 2, database 0, keamanan/auth 0, UI/workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | Repository `NewQuilvianSystemBackend`; source `ClinicalManagement` dan `PharmacyManagement`, project uji, dokumen tracked sub-modul |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `b0c1b956ae9ce221121e056b789024bdc836f1a7` pada branch `MHamzah` |
| Tanggal | 4 September 2026 |
| Status | ✅ **Selesai.** Kelima acceptance criteria terbukti. Nol migration. Satu perubahan aturan penyelesaian dilaporkan sebagai selisih kontrak, dijaga test regresi poliklinik |

## Backend Governance Preflight

| Pemeriksaan | Hasil |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement`, menyentuh satu service milik `PharmacyManagement` |
| Pemilik / prefix registry | `ClinicalManagement / Cli` — `ACTIVE / LEGACY`; `PharmacyManagement / Phm` — `ACTIVE` |
| Applicability | `TOUCHED LEGACY` — controller dan service validasi adalah kode lama |
| QBE berlaku | `QBE-VAL-001`, `QBE-API-001`, `QBE-PERM-001` |
| Entity operasional baru | `NONE`. Nol model persisted dibuat |
| Utang teknis yang sengaja tidak dirapikan | `ConsultationValidationService` berada di folder `PharmacyManagement` padahal ia memvalidasi dokumen milik `ClinicalManagement`. Penempatan itu sudah ada sebelum task ini; memindahkannya adalah pekerjaan tersendiri milik kedua pemilik modul. Dicatat, tidak dikerjakan |
| Archetype | Transaksi. Satu endpoint baca baru per arketipe sub-proses ter-scope induk: `GET /episodes/{episodeId}/soap-timeline`. Nol `GET /options`, nol `PATCH /{id}/status` generik, nol `DELETE /{id}` |
| Database authority | `NOT APPLICABLE`. Nol perubahan model, nol migration, nol eksekusi database |
| Frontend | Diperiksa read-only. Tidak ada berkas frontend yang diubah |

---

## 1. Masalah yang diperbaiki

**Lini masa perkembangan pasien menggambarkan urutan pengetikan, bukan urutan pemeriksaan.**

Visite pagi terjadi pukul 07.40. Dokter meneruskan ronde, dan baru sempat mengetiknya pukul 11.00 —
setelah ia sempat pula menuliskan catatan pemeriksaan lain pukul 10.15. Tanpa pemisahan waktu, urutan
yang terbaca menjadi:

| Yang terbaca sebelum perubahan | Yang sebenarnya terjadi |
| --- | --- |
| 10.15 — pemeriksaan siang | 07.40 — pemeriksaan pagi |
| 11.00 — pemeriksaan pagi | 10.15 — pemeriksaan siang |

Lini masa yang terbalik bukan sekadar tidak rapi. Ia adalah dasar keputusan terapi yang salah:
pembaca menyimpulkan kondisi pasien memburuk padahal ia membaik, atau sebaliknya.

`BE-RWI-040` sudah membuat kolom `ClinicalDateTime` beserta index `(InpEpisodeId, ClinicalDateTime)`.
Yang belum ada adalah jalur yang mengisinya dan pembacaan yang memakainya. Tidak ada satu pun
endpoint yang membaca catatan dokter per perawatan.

---

## 2. Proses bisnis

### 2.1 Alur normal — dokter menuliskan pemeriksaan yang sudah lewat

1. Dokter memeriksa Tn. Budi pukul 07.40.
2. Pukul 11.00 dokter membuka pasien itu dari daftar pasien rawat inap dan menuliskan catatannya.
   Pada isian waktu pemeriksaan ia mengisi **07.40**, bukan 11.00.
3. Backend memeriksa:
   1. waktu pemeriksaan **tidak boleh** melewati waktu sekarang — `VAL-DOK-13`;
   2. waktu pemeriksaan **tidak boleh** mendahului saat pasien masuk kamar — `VAL-DOK-14`.
4. Catatan tersimpan dengan dua waktu yang berbeda: waktu pemeriksaan 07.40, waktu penulisan 11.00.
5. Ketika lini masa dibaca, catatan itu menempati urutan **07.40**, dan layar dapat menandainya
   sebagai catatan yang ditulis mundur.

### 2.2 Membaca lini masa satu perawatan

`GET /doctor-consultations/episodes/{episodeId}/soap-timeline` mengembalikan seluruh catatan satu
perawatan, terurut naik menurut waktu yang dipakai lini masa.

| Aturan | Bunyinya |
| --- | --- |
| Waktu pengurutan | Waktu pemeriksaan bila ada; bila tidak, waktu penulisannya sendiri. Nilainya selalu terisi pada `timelineDateTime`, sehingga pembaca tidak perlu memilih sendiri |
| Catatan tanpa waktu pemeriksaan | **Tetap muncul.** Tidak ada catatan yang hilang dari lini masa hanya karena kolomnya kosong |
| Catatan batal | **Tetap muncul**, beserta keadaannya pada `consultationStatus`. Menghilangkannya membuat lini masa berbeda dari rekam medis yang sebenarnya |
| Penyaring `from` dan `to` | Dibandingkan terhadap waktu yang **sama** dengan yang dipakai mengurutkan. Menyaring dengan satu waktu lalu mengurutkan dengan waktu lain menghasilkan potongan yang tidak dapat dijelaskan |
| Batas perawatan | Hanya catatan milik perawatan yang diminta — `INV-DOK-12` |

### 2.3 Menyelesaikan catatan harian

Catatan harian rawat inap dinilai dengan aturan yang **berbeda** dari catatan poliklinik.

| Jenis catatan | Syarat penyelesaian |
| --- | --- |
| Catatan yang menempel pada perawatan rawat inap | **Cukup satu** dari S/O/A/P terisi — `VAL-DOK-12` |
| Catatan tanpa konteks perawatan — poliklinik, medical check-up, IGD | Keempat bagian S/O/A/P **dan** diagnosis utama, persis seperti sebelumnya |

Perbedaannya disengaja dan tertulis pada `validation-matrix.md`: menuntut keempat bagian pada setiap
catatan harian akan membuat dokter menulis kalimat kosong demi lolos validasi, dan itu menurunkan
mutu rekam medis. Diagnosis kerja pasien rawat inap hidup pada **kajian medis** — `BE-RWI-045` —
bukan diulang setiap hari pada catatan perkembangan.

### 2.4 Jalur tidak normal

| Keadaan | Jawaban backend | Kode | Aturan |
| --- | --- | --- | --- |
| Waktu pemeriksaan melewati waktu sekarang | "Waktu pemeriksaan tidak boleh melewati waktu sekarang." | `400` | `VAL-DOK-13` |
| Waktu pemeriksaan sebelum pasien masuk kamar | "Waktu pemeriksaan sebelum pasien masuk kamar. Periksa kembali." | `400` | `VAL-DOK-14` |
| Catatan harian diselesaikan dengan keempat bagian kosong | Ditolak beserta masalah `EMPTY_INPATIENT_NOTE` berbunyi "Catatan masih kosong." | `400` | `VAL-DOK-12` |
| Perawatan yang diminta tidak ada | "Perawatan rawat inap tidak ditemukan." | `404` | — |
| Batas awal penyaring melewati batas akhirnya | "Batas awal penyaring waktu melewati batas akhirnya." | `400` | — |

**Contoh berangka `VAL-DOK-14`.** Tn. Budi masuk kamar 1 September pukul 10.40. Catatan berwaktu
pemeriksaan 1 September pukul 08.00 ditolak — pada jam itu ia belum berada di kamar. Catatan
berwaktu 1 September pukul 11.00 diterima.

**Batas bawah hanya berlaku ketika saat masuk kamar diketahui.** Perawatan yang belum mencatatnya
tidak menolak apa pun; menolak berdasarkan nilai yang tidak ada akan menutup penulisan tanpa sebab
yang dapat dijelaskan kepada dokter.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa |
| --- | --- |
| `roadmap/backend-roadmap.md` | Acceptance criteria, dependency, dan DoD task |
| `contracts/api-contract.md` §1 | Bentuk `soap-timeline` dan field yang diterima `POST /` |
| `contracts/validation-matrix.md` §3 | Bunyi `VAL-DOK-12`, `VAL-DOK-13`, `VAL-DOK-14` beserta contohnya |
| `02-backend-architecture.md` §4.1 | Kolom `ClinicalDateTime` dan index lini masa |
| `Areas/HealthServices/ClinicalManagement/Models/TrxDoctorConsultation.cs` | Kolom yang tersedia dari `BE-RWI-040` |
| `Repositories/Configurations/HealthServices/TrxDoctorConsultationConfiguration.cs` | Index `(InpEpisodeId, ClinicalDateTime)` yang sudah ada |
| `Areas/HealthServices/PharmacyManagement/Services/ConsultationValidationService.cs` | Aturan kelayakan finalisasi yang berlaku hari ini |
| `Areas/HealthServices/PharmacyManagement/Services/ConsultationFinalizationService.cs` | Urutan finalisasi dan penyerahan fakta ke Billing |
| `Areas/HealthServices/InPatientManagement/Models/InpEpisode.cs` | Kolom `AdmittedAt` sebagai batas bawah waktu pemeriksaan |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/DTOs/DoctorConsultationDtos.cs` | `CreateDoctorConsultationRequest` menerima `ClinicalDateTime`; dua DTO baru `SoapTimelineItemResponse` dan `SoapTimelineResponse` |
| `Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs` | Waktu pemeriksaan disimpan dan divalidasi; endpoint `GET /episodes/{episodeId}/soap-timeline` |
| `Areas/HealthServices/ClinicalManagement/Services/InpatientClinicalContextService.cs` | Konteks membawa `AdmittedAt` sebagai batas bawah `VAL-DOK-14` |
| `Areas/HealthServices/PharmacyManagement/Services/ConsultationValidationService.cs` | Kelayakan finalisasi bercabang: catatan berkonteks perawatan memakai `VAL-DOK-12`, selain itu aturan lama apa adanya |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/InpatientDailyNoteTimelineTests.cs` | **Baru.** 12 test acceptance beserta kendali positif dan regresinya |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif.** `POST /doctor-consultations` menerima satu field opsional baru, `ClinicalDateTime`. Satu endpoint baca baru, `GET /doctor-consultations/episodes/{episodeId}/soap-timeline`, sesuai `api-contract.md` §1. Nol endpoint dihapus, nol field berganti nama. **Satu perubahan perilaku:** syarat penyelesaian catatan yang berkonteks perawatan — lihat 3.4 |
| Database | `NOT APPLICABLE`. Nol perubahan model, **nol migration**. Kolom `ClinicalDateTime` beserta index `(InpEpisodeId, ClinicalDateTime)` sudah dibuat `BE-RWI-040` dan langsung terpakai oleh pengurutan lini masa |
| Keamanan/Auth | Butir hak akses **tidak bertambah**: endpoint lini masa memakai `DoctorConsultation : Read` yang sudah ada. Nol pemeriksaan berbasis nama peran |

### 3.4 Selisih terhadap perilaku sebelumnya yang dilaporkan

| Hal | Isinya |
| --- | --- |
| **Syarat penyelesaian catatan rawat inap dilonggarkan** | Sebelumnya setiap catatan menuntut keempat bagian S/O/A/P **dan** diagnosis utama. Bagi catatan yang menempel pada perawatan rawat inap, syaratnya kini **cukup satu bagian terisi** — bunyi `VAL-DOK-12` apa adanya. **Penyaringnya adalah keberadaan `InpEpisodeId` pada catatan itu sendiri**, bukan tipe kunjungannya, sehingga catatan poliklinik tidak pernah ikut terlonggarkan. Dijaga test `CatatanTanpaKonteksPerawatan_TetapMenuntutSoapLengkapDanDiagnosis` |
| **Kenapa diagnosis tidak lagi diwajibkan pada catatan harian** | `VAL-DOK-11` menuntut diagnosis pada **kajian medis**, bukan pada catatan perkembangan. Mewajibkannya setiap hari berarti dokter mengulang diagnosis yang sama sepuluh kali pada perawatan sepuluh hari. Bila pemilik menghendaki sebaliknya, yang berubah hanya satu cabang pada `ConsultationValidationService` |
| **Zona waktu diseragamkan** | Waktu kiriman tanpa penanda zona diperlakukan sebagai UTC sebelum dibandingkan maupun disimpan, mengikuti kolom `timestamp with time zone`. Tanpa itu, perbandingan terhadap waktu sekarang membandingkan dua jam yang berbeda acuannya dan penolakannya menjadi acak menurut zona waktu pengirim |

---

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Doctor Consultation

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Membuat catatan dokter. **Kini menerima** `ClinicalDateTime`, yaitu waktu pemeriksaan yang sebenarnya. Boleh mundur, tidak boleh melewati waktu sekarang, dan tidak boleh mendahului saat pasien masuk kamar | `DoctorConsultation : Create` |
| `GET` | `/episodes/{episodeId}/soap-timeline` | **Baru.** Lini masa catatan satu perawatan rawat inap, terurut waktu pemeriksaan. Menerima penyaring `from` dan `to` | `DoctorConsultation : Read` |
| `PATCH` | `/{id}/complete` | Menyelesaikan catatan. **Untuk catatan yang berkonteks perawatan rawat inap:** cukup satu bagian S/O/A/P terisi, dan diagnosis utama tidak diwajibkan | `DoctorConsultation : Update` |

Bentuk balasan lini masa:

| Field | Isinya |
| --- | --- |
| `timelineDateTime` | Waktu yang dipakai mengurutkan. Selalu terisi |
| `clinicalDateTime` | Waktu pemeriksaan. Kosong bila tidak pernah diisi |
| `consultationDateTime` | Waktu catatan ditulis |
| `isBackdated` | Benar bila waktu pemeriksaan mendahului waktu penulisan, sehingga layar dapat menyatakannya apa adanya |
| `consultationStatus` | Keadaan catatan, termasuk yang dibatalkan |
| `subjective`, `objective`, `assessment`, `plan` | Ringkasan isi. Isi lengkap tetap diambil lewat `GET /{id}` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | Berhasil | `PASS` | `0 Error(s)`, `185 Warning(s)`, seluruhnya peringatan yang sudah ada sebelumnya |
| `dotnet test` project uji SQLite, seluruhnya | Berhasil | `PASS` | `Failed: 0, Passed: 262, Skipped: 0, Total: 262` |
| **Catatan ditulis belakangan menempati urutan waktu pemeriksaannya** | Urutan penulisan sengaja dibalik dari urutan pemeriksaan; lini masa mengembalikan "Pemeriksaan pagi" lebih dulu, dan menandainya ditulis mundur | `PASS` | `InpatientDailyNoteTimelineTests.CatatanYangDitulisBelakangan_MenempatiUrutanWaktuPemeriksaannya` |
| Tiga catatan berwaktu berbeda terbaca terurut | Urutan penulisan `-6, -30, -18` jam; lini masa mengembalikan `-30, -18, -6` | `PASS` | `...BeberapaCatatanSepanjangPerawatan_TerbacaTerurut` |
| Catatan tanpa waktu pemeriksaan | Tetap muncul; `timelineDateTime` sama dengan waktu penulisan; tidak ditandai ditulis mundur | `PASS` | `...CatatanTanpaWaktuPemeriksaan_TetapMunculMemakaiWaktuPenulisan` |
| Penyaring waktu memotong memakai waktu pemeriksaan | Dua catatan, disaring enam jam terakhir, tersisa satu | `PASS` | `...PenyaringWaktu_MemotongMemakaiWaktuPemeriksaan` |
| Lini masa tidak memuat catatan perawatan lain | Satu butir, milik pasien yang diminta | `PASS` | `...LiniMasa_TidakMemuatCatatanPerawatanLain` |
| Perawatan yang tidak ada | Dijawab `404` | `PASS` | `...LiniMasaPerawatanYangTidakAda_Dijawab404` |
| Waktu pemeriksaan di masa depan | Ditolak `400` dengan kalimat `VAL-DOK-13`; nol catatan tersimpan | `PASS` | `...WaktuPemeriksaanDiMasaDepan_Ditolak400` |
| Waktu pemeriksaan sebelum masuk kamar | Ditolak `400` dengan kalimat `VAL-DOK-14`; batasnya diambil dari `AdmittedAt` yang sebenarnya | `PASS` | `...WaktuPemeriksaanSebelumMasukKamar_Ditolak400` |
| **Kendali positif:** pengisian mundur di dalam masa perawatan | Diterima `200`; waktu pemeriksaan tersimpan lebih awal daripada waktu penulisan | `PASS` | `...WaktuPemeriksaanMundurDiDalamMasaPerawatan_Diterima` |
| Catatan harian dengan keempat bagian kosong diselesaikan | Ditolak `400` dengan masalah `EMPTY_INPATIENT_NOTE` berbunyi "Catatan masih kosong." | `PASS` | `...CatatanHarianDenganKeempatBagianKosong_DitolakSaatDiselesaikan` |
| **Kendali positif:** catatan harian dengan satu bagian terisi | Diselesaikan `200`; status menjadi `Completed` | `PASS` | `...CatatanHarianDenganSatuBagianTerisi_DapatDiselesaikan` |
| **Regresi poliklinik:** catatan tanpa konteks perawatan | Tetap ditolak; masalah `MISSING_OBJECTIVE`, `MISSING_ASSESSMENT`, `MISSING_PLAN`, dan `MISSING_PRIMARY_DIAGNOSIS` seluruhnya muncul, dan `EMPTY_INPATIENT_NOTE` **tidak** muncul | `PASS` | `...CatatanTanpaKonteksPerawatan_TetapMenuntutSoapLengkapDanDiagnosis` |
| **Regresi poliklinik, medical check-up, dan IGD** pada jalur pembuatan | Seluruhnya hijau | `PASS` | 16 test pada `InpatientDoctorEntryPointTests` dan 10 test pada `DoctorConsultationInpatientPathTests` — lihat [BE-RWI-044](BE-RWI-044.md) |

Uji manual: `NOT FEASIBLE` — tidak ada lingkungan runtime beserta database yang tersedia pada sesi
ini.

**Tidak dijalankan:**

- Uji terhadap PostgreSQL. Tidak ada database uji yang tersedia; task ini tidak mengubah schema
  sehingga tidak menambah utang verifikasi migration yang sudah tercatat.
- Uji beban pembacaan lini masa pada perawatan yang sangat panjang. Endpoint ini belum
  memberlakukan pagination — lihat bagian 7.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Catatan yang ditulis pukul 11.00 untuk pemeriksaan pukul 07.40 menempati urutan pukul 07.40 | Terpenuhi | `CatatanYangDitulisBelakangan_MenempatiUrutanWaktuPemeriksaannya` — urutan penulisan sengaja terbalik dari urutan pemeriksaan, sehingga uji gagal bila pengurutan diam-diam kembali memakai waktu penulisan |
| 2. Waktu pemeriksaan melewati waktu sekarang ditolak `400` | Terpenuhi | `WaktuPemeriksaanDiMasaDepan_Ditolak400` |
| 3. Waktu pemeriksaan sebelum pasien masuk kamar ditolak `400` | Terpenuhi | `WaktuPemeriksaanSebelumMasukKamar_Ditolak400`, beserta kendali positif `WaktuPemeriksaanMundurDiDalamMasaPerawatan_Diterima` |
| 4. Beberapa catatan sepanjang perawatan terbaca sebagai lini masa terurut | Terpenuhi | `BeberapaCatatanSepanjangPerawatan_TerbacaTerurut` |
| 5. Menyelesaikan catatan dengan keempat bagian kosong ditolak `400` | Terpenuhi | `CatatanHarianDenganKeempatBagianKosong_DitolakSaatDiselesaikan`, beserta kendali positif dan regresi poliklinik |

### 6.1 Definition of Done

| Butir DoD | Status |
| --- | --- |
| Kelima acceptance criteria terbukti | Terpenuhi |
| Test urutan hijau | Terpenuhi — dua test urutan, keduanya memakai urutan penulisan yang sengaja berbeda dari urutan pemeriksaan |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `185 Warning(s)` pada build, seluruhnya peringatan dokumentasi XML yang sudah ada sebelum task ini. Nol peringatan baru |
| Masalah yang diketahui | Satu kegagalan uji milik `BillingManagement` yang tidak berkaitan — dirinci pada [BE-RWI-044](BE-RWI-044.md) bagian 7 |
| Risiko tersisa | **Pertama, lini masa belum berpaginasi.** Perawatan yang sangat panjang mengembalikan seluruh catatannya sekali jalan. `api-contract.md` §1 memang menuliskan `SoapTimelineResponse` dan bukan `PagedResult`, sehingga bentuknya diikuti apa adanya; bila jumlah catatan per perawatan terbukti besar, pagination perlu dibahas pemilik kontrak. **Kedua, penyelesaian catatan memindahkan keadaan kunjungan** menjadi `ConsultationCompleted` pada setiap catatan harian yang difinalkan — perilaku yang sudah ada sebelum task ini, tetapi baru terasa setelah catatan harian dapat dibuat berulang. **Ketiga, `VAL-DOK-06` belum ditegakkan**, sama seperti pada `BE-RWI-044` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Sama dengan yang dirinci [BE-RWI-044](BE-RWI-044.md) bagian 7; ketiga task dikerjakan pada sesi yang sama, dan perubahan pengguna yang berjalan bersamaan **tidak disentuh**. Nol operasi Git dijalankan |
| Langkah berikutnya | `BE-RWI-047` menunggu `BE-RWI-038`. `BE-RWI-053` sudah lepas dependency-nya dan dapat dimulai. Keadaan kunjungan yang berpindah pada setiap penyelesaian catatan harian perlu ditanyakan kepada pemilik `RegistrationManagement` sebelum jumlah catatan harian bertambah banyak di produksi |
