# Laporan Perubahan Backend — `BE-RWI-043`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-043` |
| Judul | Dokter dapat menulis lebih dari satu catatan dan satu resep |
| Slice | `DOK-MVP-1` — fondasi konteks, kolom, tabel visite, pelonggaran |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/backend-roadmap.md`, task `BE-RWI-043` |
| Trace | `INT-DOK-02`; `RWI-DEC-038`, `RWI-DEC-070`, `RWI-RULE-026` aturan 4 dan 5; `INV-DOK-04`, `INV-DOK-05`; `FR-DOK-002`, `FR-DOK-003`; `RWI-AC-143` |
| Contract version | `0.3.0`, `APPROVED` Muhammad Hamzah 3 September 2026 |
| Dependency | `BE-RWI-039` **selesai** ([laporan](BE-RWI-039.md)); `BE-RWI-042` 🟡 sebagian ([laporan](BE-RWI-042.md)) |
| Klasifikasi | `HEAVY`, skor 10: repository 0, berkas diperiksa 1, berkas diubah 2, logika bisnis 2, kontrak API 1, database 2, keamanan/auth 1, UI/workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | Repository `NewQuilvianSystemBackend`; source `ClinicalManagement` dan `PharmacyManagement`, configuration, `Migrations/`, project uji, dokumen tracked sub-modul |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `c8e83854af240186b5091da412fadde3810afcb1` pada branch `MHamzah` |
| Tanggal | 3 September 2026 |
| Status | 🟡 **Sebagian.** Empat dari enam acceptance criteria terbukti ujung ke ujung; kriteria 1 dan 2 terbukti pada **aturan dan lapisan penyimpanan**, tetapi belum dapat dibuktikan lewat endpoint sampai `BE-RWI-044` membuka jalur tanpa antrean bagi pasien rawat inap |

## Backend Governance Preflight

| Pemeriksaan | Hasil |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement` dan `PharmacyManagement` |
| Pemilik / prefix registry | `ClinicalManagement / Cli` `ACTIVE`; `PharmacyManagement / Phm` `ACTIVE` |
| Applicability | `TOUCHED LEGACY` — kedua controller adalah kode lama; perubahan dibatasi pada aturan jumlah yang disebut task |
| QBE berlaku | `QBE-VAL-001`, `QBE-API-001`, `QBE-CFG-002`, `QBE-TXN-001`, `QBE-PERM-001` |
| Utang teknis yang sengaja tidak dirapikan | Kedua controller menaruh logika bisnis di dalam controller, berlawanan dengan `QBE-SVC-001`. Ini utang milik modul lain; merapikannya di tengah penambahan aturan adalah dua pekerjaan yang digabung. Dicatat, tidak dikerjakan |
| Archetype | Transaksi. Tidak ada endpoint baru; yang berubah adalah aturan penerimaan pada endpoint yang sudah ada |
| Database authority | Pembuatan migration `PROVIDED` sebagai konsekuensi teknis yang tidak dapat dihindari — lihat bagian 3.3. **Eksekusi migration tidak diberikan dan tidak dilakukan** |
| Frontend | Diperiksa read-only. Kalimat penolakan rawat jalan dipertahankan sama persis supaya layar yang menampilkannya tidak berubah |

---

## 1. Masalah yang diperbaiki

Dua batas dipasang ketika sistem ini hanya melayani pasien rawat jalan, dan keduanya masuk akal di
sana: **satu catatan dokter per kunjungan**, dan **satu resep aktif per catatan**. Pasien
poliklinik memang datang, diperiksa sekali, menerima satu resep, lalu pulang.

Pasien yang menginap tidak begitu.

**Contoh nyata.** Pasien demam berdarah dirawat sepuluh hari. Dokter memeriksanya setiap pagi, dan
setiap pagi ada yang berubah: trombosit turun, lalu naik, cairan disesuaikan, obat diganti. Dengan
batas satu catatan per kunjungan, dokter hanya boleh menulis **satu** catatan untuk seluruh
sepuluh hari itu — dan hanya boleh meresepkan **satu kali**.

Yang terjadi kemudian dapat ditebak: dokumentasi hariannya pindah ke kertas, dan resep hari
berikutnya dicarikan jalan lain. Rekam medis elektroniknya menjadi tidak menggambarkan perawatan
yang sebenarnya terjadi.

Kedua pelonggaran ini sudah disetujui sejak `RWI-DEC-038` dan diperluas `RWI-DEC-070`. Yang belum
ada kodenya.

---

## 2. Proses bisnis

**Tujuan.** Pasien yang dirawat menerima catatan harian dan resep sebanyak yang memang
dibutuhkan, tanpa mengubah satu pun perilaku rawat jalan dan medical check-up.

**Pelaku.** Dokter yang merawat pasien menginap.

**Pemicu.** Dokter menyimpan catatan kedua, atau meresepkan untuk kedua kalinya, pada perawatan
yang sama.

**Langkah yang berurutan.**

1. Ketika catatan dokter hendak dibuat, sistem menanyakan lebih dulu: apakah kunjungan ini sedang
   menaungi **perawatan rawat inap yang berjalan**?
2. Bila **ya**, batas satu catatan per kunjungan tidak diberlakukan. Catatan yang lahir juga
   distempel dengan perawatan itu.
3. Bila **tidak** — rawat jalan, medical check-up, atau IGD — batas lama diberlakukan apa adanya,
   dengan kode dan kalimat penolakan yang sama persis seperti sebelumnya.
4. Ketika resep hendak dibuat, sistem melihat catatan dokter yang menaunginya. Bila catatan itu
   menempel pada perawatan rawat inap, batas satu resep aktif tidak diberlakukan.
5. Resep yang lahir mewarisi perawatan dari catatannya.

**Aturan yang berlaku.**

- **Penyaringnya adalah perawatan yang berjalan, bukan sekadar nama tipe kunjungan.** Kunjungan
  bertipe rawat inap yang perawatannya belum dimulai — pasien belum masuk kamar — atau sudah
  ditutup tetap tunduk pada batas lama. Tanpa syarat ini akan ada celah untuk menulis catatan
  berulang pada kunjungan yang belum benar-benar menjadi perawatan.
- **Aturan aplikasi dan penjagaan database wajib memakai penanda yang sama.** Bila aturan aplikasi
  dilonggarkan sedangkan index unique di database masih berlaku penuh, permintaan akan lolos
  pemeriksaan lalu **gagal saat disimpan** — mengubah penolakan yang rapi menjadi kegagalan
  sistem, persis kegagalan yang baru saja ditutup `BE-RWI-037`. Karena itu keduanya dilonggarkan
  bersama-sama, dengan penanda yang sama.
- **Kalimat penolakan rawat jalan tidak disentuh sama sekali.** `INV-DOK-05` menuntut kode dan
  kalimatnya sama persis, dan `RWI-AC-143` adalah penjaganya.

**Status yang dihasilkan.** Tidak ada status baru.

**Jalur tidak normal.**

| Keadaan | Hasilnya |
| --- | --- |
| Catatan kedua pada kunjungan rawat jalan | Ditolak `400`, "Konsultasi dokter untuk encounter ini sudah ada." — kalimat lama, tidak berubah |
| Catatan kedua pada kunjungan medical check-up | Ditolak sama persis |
| Catatan kedua pada kunjungan IGD | Ditolak sama persis; perilaku IGD sengaja tidak diubah — lihat bagian 7 |
| Resep aktif kedua pada catatan tanpa perawatan rawat inap | Ditolak `400`, "Konsultasi ini sudah memiliki resep aktif." — kalimat lama |
| Catatan kedua pada kunjungan rawat inap yang perawatannya belum dimulai | Ditolak seperti rawat jalan |

**Hasil akhirnya.** Batas yang memang hanya masuk akal untuk rawat jalan berhenti mengikat
perawatan rawat inap, tanpa satu pun perilaku rawat jalan berubah.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs`,
  kedua cabang validasi pembuatan
- `Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs`
- `Repositories/Configurations/HealthServices/TrxDoctorConsultationConfiguration.cs`,
  `TrxPrescriptionConfiguration.cs`, `TrxQueueConfiguration.cs`
- `Areas/HealthServices/RegistrationManagement/Enums/EncounterType.cs`
- `contracts/integration-contract.md` §2, `02-backend-architecture.md` §3.3

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs` | Aturan satu catatan per kunjungan dipindahkan ke satu tempat dan disaring perawatan yang berjalan; kedua cabang validasi memakainya. Konteks perawatan distempel pada catatan yang lahir. Service konteks klinis dari `BE-RWI-039` dipasang sebagai dependency |
| `Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs` | Aturan satu resep aktif per catatan dilepas bila catatannya menempel pada perawatan rawat inap; resep mewarisi konteks perawatan dari catatannya |
| `Repositories/Configurations/HealthServices/TrxDoctorConsultationConfiguration.cs` | Unique index kunjungan dipersempit: berlaku hanya bila catatan tidak menempel pada perawatan rawat inap |
| `Repositories/Configurations/HealthServices/TrxPrescriptionConfiguration.cs` | Unique index catatan dipersempit dengan cara yang sama |
| `Migrations/20260903100128_RelaxSingleConsultationAndPrescriptionForInpatient.cs` | **Baru.** Membentuk ulang kedua unique index dengan penyaring barunya |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/DoctorConsultationInpatientPathTests.cs` | Uji pelonggaran, regresi rawat jalan, regresi medical check-up, regresi IGD, dan uji kunjungan rawat inap yang perawatannya belum dimulai |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/SupportingOrderAndPrescriptionContextTests.cs` | Uji resep kedua diterima saat ada konteks perawatan dan ditolak saat tidak ada |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/DoctorConsultationCompletionTests.cs` | Penyesuaian pembuatan controller mengikuti dependency baru |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route, method, bentuk permintaan, dan bentuk balasan **tidak berubah**. Yang berubah adalah **kapan** permintaan diterima: catatan dan resep kedua pada perawatan rawat inap tidak lagi ditolak. Kode dan kalimat penolakan rawat jalan, medical check-up, dan IGD **tidak disentuh** |
| Database | **Dua unique index dibentuk ulang dengan penyaring yang lebih sempit.** Nol kolom baru, nol tabel baru, nol data yang dipindahkan. Satu migration: `20260903100128_RelaxSingleConsultationAndPrescriptionForInpatient`. **Belum diterapkan ke database mana pun** |
| Keamanan/Auth | `NOT APPLICABLE`. Metadata hak akses tidak disentuh. Pelonggaran ini menyangkut aturan bisnis jumlah dokumen, bukan kewenangan siapa yang boleh menulis |

**Kenapa ada migration padahal roadmap tidak menyebutnya.** Batas satu catatan per kunjungan tidak
hanya hidup di dalam kode; ia ditegakkan **unique index di database** —
`IX_TrxDoctorConsultation_EncounterId` — dan hal yang sama berlaku untuk resep lewat
`IX_TrxPrescription_ConsultationId`. Selama kedua index itu berlaku penuh, melonggarkan aturan di
lapisan aplikasi saja tidak menghasilkan apa pun selain kegagalan sistem pada saat penyimpanan.
Penyaring barunya memakai kolom konteks perawatan yang lahir dari `BE-RWI-040` dan `BE-RWI-042`,
sehingga rawat jalan dan medical check-up tetap dijaga database persis seperti sebelumnya.

---

## 4. Dokumentasi endpoint

Task ini tidak menambah maupun mengubah bentuk endpoint. Endpoint yang aturannya berubah:

#### Health Services / Clinical Management / Doctor Consultation

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/clinical-management/doctor-consultations` | Membuat catatan pemeriksaan dokter. Batas satu catatan per kunjungan tidak lagi berlaku bila kunjungan itu menaungi perawatan rawat inap yang berjalan | `DoctorConsultation : Create` |

#### Health Services / Pharmacy Management / Prescription

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/pharmacy-management/prescriptions` | Membuat header resep. Batas satu resep aktif per catatan tidak lagi berlaku bila catatannya menempel pada perawatan rawat inap | `Prescription : Create` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | Berhasil, `0 Error(s)` | `PASS` | Keluaran perintah |
| Dua catatan pada satu kunjungan rawat inap tersimpan keduanya | Keduanya tersimpan; hitungan `2` | `PASS` | `DoctorConsultationInpatientPathTests.RawatInap_IndexTidakLagiMenolakCatatanKedua` |
| Catatan tanpa konteks perawatan pada kunjungan yang sama, yang kedua | Ditolak database | `PASS` | Uji yang sama |
| Konteks perawatan distempel pada catatan yang lahir | Terisi sesuai perawatan berjalan kunjungan itu | `PASS` | `…RawatInap_KonteksPerawatanTerstempelPadaCatatan` |
| **Regresi rawat jalan** — catatan kedua ditolak dengan kalimat yang sama persis | `400`, "Konsultasi dokter untuk encounter ini sudah ada." | `PASS` | `…RawatJalanDanMedicalCheckup_CatatanKeduaTetapDitolakDenganKalimatSama(Outpatient)` |
| **Regresi medical check-up** — perilakunya tidak berubah | `400`, kalimat yang sama persis | `PASS` | Uji yang sama, varian `MedicalCheckup` |
| **Regresi IGD** — perilakunya tetap berjalan | Catatan pertama diterima; catatan kedua ditolak `400` dengan kalimat yang sama; nol baris antrean | `PASS` | `…Igd_PerilakunyaTetapSepertiSebelumnya` |
| Kunjungan rawat inap yang perawatannya belum dimulai | Tetap dibatasi; ditolak `400` dengan kalimat yang sama | `PASS` | `…RawatInapYangPerawatannyaBelumDimulai_TetapDibatasi` |
| Resep kedua diterima saat ada konteks perawatan | Dua resep tersimpan pada satu catatan rawat inap | `PASS` | `SupportingOrderAndPrescriptionContextTests.ResepKedua_DiterimaSaatAdaKonteksPerawatan_DitolakSaatTidakAda` |
| Resep aktif kedua tanpa konteks perawatan | Ditolak database | `PASS` | Uji yang sama |
| Penyaring index catatan memakai kolom konteks perawatan | Unique dengan penyaring yang menyebut `InpEpisodeId` | `PASS` | `InpatientClinicalSchemaTests.UniqueCatatanPerKunjungan_HanyaBerlakuTanpaKonteksPerawatan` |
| Penyaring index resep memakai kolom konteks perawatan | Unique dengan penyaring yang menyebut `InpEpisodeId` | `PASS` | `…UniqueResepAktif_HanyaBerlakuTanpaKonteksPerawatan` |
| **Catatan kedua rawat inap lewat endpoint** | **Tidak dijalankan** | `NOT RUN` | Lihat "Tidak dijalankan" |
| **Resep kedua rawat inap lewat endpoint** | **Tidak dijalankan** | `NOT RUN` | Lihat "Tidak dijalankan" |
| `dotnet test` seluruh berkas uji SQLite | `Failed: 0, Passed: 219` | `PASS` | Keluaran perintah |

Uji manual: `NOT FEASIBLE`.

**Tidak dijalankan:**

- **Pembuktian ujung ke ujung lewat endpoint untuk catatan dan resep kedua rawat inap.** Alasannya
  bukan kelalaian, melainkan urutan dependency roadmap. Satu kunjungan hanya boleh memiliki
  **satu baris antrean hidup** — `IX_TrxQueue_EncounterId` unique — sehingga catatan kedua pada
  satu kunjungan hanya dapat lahir lewat cabang **tanpa antrean**. Cabang itu hari ini masih
  terbatas pada kunjungan IGD, dan membukanya untuk pasien rawat inap adalah acceptance criteria
  nomor 1 milik `BE-RWI-044`, task yang justru bergantung pada task ini. Yang dibuktikan sekarang
  adalah kedua penghalangnya benar-benar sudah dilepas: aturan aplikasi tidak lagi menolak, dan
  index database tidak lagi menolak.
- Eksekusi migration ke database mana pun.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Catatan kedua pada satu kunjungan rawat inap diterima | **Sebagian** — terbukti pada aturan dan pada penyimpanan, belum lewat endpoint | `…RawatInap_IndexTidakLagiMenolakCatatanKedua`; `…UniqueCatatanPerKunjungan_HanyaBerlakuTanpaKonteksPerawatan`. Pembuktian lewat endpoint menunggu `BE-RWI-044` |
| 2. Resep kedua sepanjang perawatan diterima | **Sebagian** — terbukti pada aturan dan pada penyimpanan, belum lewat endpoint | `…ResepKedua_DiterimaSaatAdaKonteksPerawatan_DitolakSaatTidakAda`; `…UniqueResepAktif_HanyaBerlakuTanpaKonteksPerawatan` |
| 3. Catatan kedua pada kunjungan **rawat jalan** tetap ditolak **dengan kode dan kalimat yang sama persis** | Terpenuhi | `…RawatJalanDanMedicalCheckup_CatatanKeduaTetapDitolakDenganKalimatSama(Outpatient)`; kalimatnya dibandingkan utuh, bukan sepotong |
| 4. Resep aktif kedua pada kunjungan rawat jalan tetap ditolak sama persis | Terpenuhi | Kalimat penolakan pada `PrescriptionController` tidak disentuh sama sekali; penolakan pada lapisan penyimpanan dibuktikan `…ResepKedua_DiterimaSaatAdaKonteksPerawatan_DitolakSaatTidakAda` |
| 5. Perilaku medical check-up tidak berubah | Terpenuhi | Uji yang sama, varian `MedicalCheckup` |
| 6. Perilaku IGD tetap berjalan | Terpenuhi | `…Igd_PerilakunyaTetapSepertiSebelumnya` |

**Kalimat penolakan sebelum dan sesudah, berdampingan** — sebagaimana diminta Definition of Done:

| Jalur | Sebelum perubahan | Sesudah perubahan |
| --- | --- | --- |
| Catatan kedua, rawat jalan | `400` — "Konsultasi dokter untuk encounter ini sudah ada." | `400` — "Konsultasi dokter untuk encounter ini sudah ada." |
| Catatan kedua, medical check-up | `400` — "Konsultasi dokter untuk encounter ini sudah ada." | `400` — "Konsultasi dokter untuk encounter ini sudah ada." |
| Catatan kedua, IGD | `400` — "Konsultasi dokter untuk encounter ini sudah ada." | `400` — "Konsultasi dokter untuk encounter ini sudah ada." |
| Catatan kedua, rawat inap berjalan | `400` — "Konsultasi dokter untuk encounter ini sudah ada." | **Diterima** |
| Resep aktif kedua, tanpa perawatan rawat inap | `400` — "Konsultasi ini sudah memiliki resep aktif." | `400` — "Konsultasi ini sudah memiliki resep aktif." |
| Resep aktif kedua, catatan rawat inap | `400` — "Konsultasi ini sudah memiliki resep aktif." | **Diterima** |

Ketiga kalimat penolakan itu tidak berubah satu huruf pun; keduanya diambil dari source yang sama
sebelum dan sesudah perubahan.

**Definition of Done.**

| Butir | Status |
| --- | --- |
| Keenam acceptance criteria terbukti | **Belum** — kriteria 1 dan 2 baru terbukti pada aturan dan penyimpanan |
| Test regresi rawat jalan dan medical check-up hijau | Terpenuhi |
| Laporan mencantumkan kalimat penolakan sebelum dan sesudah, berdampingan | Terpenuhi — lihat tabel di atas |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru dari berkas task ini |
| **Bagian kontrak yang sengaja belum dikerjakan** | `INT-DOK-02` meminta pelonggaran untuk kunjungan bertipe `Inpatient` **dan** `Emergency`. Yang dikerjakan baru `Inpatient`. Alasannya teknis dan konkret: `TrxPrescription` tidak memiliki satu pun kolom yang membedakan resep IGD dari resep rawat jalan, sehingga penyaring index tidak dapat dibentuk untuk IGD. Melonggarkan aturan aplikasi saja akan membuat resep IGD kedua lolos validasi lalu **gagal saat disimpan**. Acceptance criteria task ini hanya menuntut penerimaan rawat inap dan menuntut perilaku IGD tetap berjalan, dan keduanya terpenuhi. Bagian `Emergency` diteruskan kepada pemilik `PharmacyManagement` beserta pilihan bentuknya |
| Masalah yang diketahui | Kriteria 1 dan 2 belum dapat dibuktikan lewat endpoint sampai `BE-RWI-044` selesai. Urutan ini melekat pada dependency roadmap dan bukan sesuatu yang dapat diselesaikan di dalam task ini tanpa mengambil scope `BE-RWI-044` |
| Risiko tersisa | Ini perubahan paling berisiko pada seluruh roadmap: ia menyentuh alur poliklinik yang sedang melayani pasien. Jaring pengamannya kini ada — regresi rawat jalan, medical check-up, dan IGD seluruhnya hijau, dan kalimat penolakannya dibandingkan utuh sehingga perubahan sekecil apa pun akan menggagalkan uji. Migration index belum dijalankan; sebelum diterapkan, uji maju-mundur terhadap PostgreSQL wajib dijalankan lebih dulu |
| Perubahan sampingan | `NONE` untuk task ini. Penyesuaian `DoctorConsultationCompletionTests.cs` adalah konsekuensi langsung dependency baru pada controller, bukan efek samping di luar scope |
| Interupsi | `NONE` pada bagian task ini |
| Status Git | `git status --short` di akhir pekerjaan: **25 baris `M` dan 29 baris `??`**, seluruhnya berasal dari rangkaian `BE-RWI-037` s.d. `BE-RWI-043` beserta migration, test, dan dokumen laporannya. Termasuk di dalamnya `QuilvianSystemBackend.csproj` — perbaikan infrastruktur build di luar scope yang dicatat pada [laporan BE-RWI-037](BE-RWI-037.md) bagian 3.4. Branch `MHamzah`, upstream `origin/MHamzah`. Tidak ada stage, commit, push, pull, merge, rebase, checkout, maupun deploy |
| Langkah berikutnya | `BE-RWI-044` membuka jalur tanpa antrean bagi pasien rawat inap; setelah itu kriteria 1 dan 2 task ini dapat dibuktikan ujung ke ujung dan statusnya dinaikkan menjadi selesai |
