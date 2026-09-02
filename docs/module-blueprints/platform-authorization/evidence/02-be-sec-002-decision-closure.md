# BE-SEC-002 — Owner Decision Closure dan Epic Decomposition

| Field | Nilai |
| --- | --- |
| `blueprint_id` | `SEC-BP-001` — Platform Authorization & Access Control |
| Dokumen sumber | `evidence/01-be-sec-002-audit-architecture.md` |
| Task mode | `AUDIT + ARCHITECTURE / OWNER DECISION CLOSURE` |
| Status | `DECISION CLOSURE SELESAI` |
| Wewenang tulis | Hanya dokumen evidence. Tidak ada source, entity, migration, database, commit, atau push |
| Backend SHA | `e1d112142510baa86ccd89977bc7189c89ed012b` (`AndryZain`) |
| Frontend SHA | `2b9e3b074f8a3839857e123515353dd2f3233ac3` (`AgentCodexFrontend`) |
| Keputusan pemilik sistem | `DECISION 1` sampai `DECISION 7` + arahan Broken Certificate, Legacy Split Safety, dan Epic Decomposition |
| Status | `APPROVED` oleh pemilik sistem 2 September 2026 |
| Dokumen lanjutan | [`03-be-sec-003-pre-implementation-impact.md`](03-be-sec-003-pre-implementation-impact.md) — impact report `BE-SEC-003` beserta hasil query read-only database development |
| Tanggal | 2 September 2026 |

> **Satu koreksi setelah query database dan audit semantik OR.** Dokumen ini memperkirakan himpunan
> endpoint fallback warisan akan berubah dari 69 menjadi 67. Perkiraan itu **keliru**: jumlahnya
> tetap **69**, yang berubah hanya nama aksi dan klasifikasi dua endpoint audio. Sebabnya diuraikan
> pada `evidence/03` bagian 8.3 dan 16. Seluruh kalimat yang terdampak sudah diperbaiki di dokumen
> ini.

---

## A. Decision Closure Matrix — Section Y.3

Tujuh keputusan pada `Y.3` ditinjau ulang terhadap keputusan pemilik sistem dan bukti source
tambahan yang dikumpulkan pada sesi ini.

| # | Keputusan Y.3 | Status setelah closure | Dasar | Sisa aksi |
| ---: | --- | --- | --- | --- |
| 1 | Persetujuan baris registry prefix | **CLOSED — OWNER APPROVED** | `DECISION 3`. Audit collision dijalankan: `Sec` bebas (bagian G) | Registry di-update sebagai langkah pertama `BE-SEC-004` |
| 2 | Dokter rawat jalan boleh approve/execute tindakan? | **CLOSED** | `DECISION 5`. `Execute` = permission terpisah; `Approve` = sensitif terpisah, fail closed | Split teknis dikerjakan `BE-SEC-003` (bagian D) |
| 3 | Siapa berwenang menandatangani surat dokter? | **DEFERRED — tidak memblokir** | Arahan Broken Certificate. Capability diklasifikasi `BROKEN_DEPENDENCY`; mapping dilarang | Kembali menjadi keputusan owner **setelah** route diperbaiki task terpisah |
| 4 | SOAP write terpisah dari Complete Consultation? | **CLOSED — YA, TERPISAH** | `DECISION 6`. Dibuktikan: `UpdateSoap` dan `CompleteConsultation` sama-sama `DoctorConsultation.Update` | Split teknis dikerjakan `BE-SEC-003` (bagian E) |
| 5 | Klasifikasi dua endpoint audio antrean | **PARTIALLY CLOSED — arah ditetapkan, satu pertanyaan baru muncul** | `DECISION 7` menolak `AllowAnonymous` dan meminta pemisahan actor. Bukti baru: **tiga kelas actor** memanggil endpoint yang sama (bagian J) | Satu `OWNER BUSINESS DECISION` tersisa — bagian N butir 1 |
| 6 | Batas penundaan pencabutan hak (cache TTL) | **CLOSED — direklasifikasi menjadi `ARCHITECTURE RECOMMENDATION`** | Dapat diputuskan dari source; tidak memerlukan kewenangan bisnis | Rekomendasi: cache **per-request saja**, tanpa TTL. Lihat A.1 |
| 7 | Tenancy `HospitalId` pada Access Profile | **CLOSED — `AUTO-RESOLVABLE` dari source** | Bukti baru pada sesi ini. Lihat A.2 | Access Profile **tidak** membawa `HospitalId` |

### A.1 Butir 6 — mengapa direklasifikasi menjadi rekomendasi arsitektur

Pada audit awal butir ini dinaikkan menjadi keputusan owner karena menyangkut "seberapa cepat hak
harus benar-benar hilang". Setelah ditinjau ulang, jawabannya dapat ditetapkan dari source tanpa
kewenangan bisnis:

`AccessPermissionService.HasAccessAsync` hari ini sudah melakukan 3–4 perjalanan ke database untuk
**setiap** request terproteksi. Menambahkan satu sumber izin kedua menambah satu perjalanan lagi
pada urutan besaran yang sama. Ini bukan lompatan biaya yang menuntut pertukaran keamanan.

> **Rekomendasi tunggal:** pakai **cache per-request saja** — himpunan kunci izin efektif dihitung
> sekali per request HTTP, lalu dibuang. Tidak ada cache ber-TTL pada fase mana pun dalam rencana
> ini.
>
> Konsekuensinya: pencabutan hak berlaku **seketika**, persis seperti perilaku hari ini. Tidak ada
> jendela basi yang perlu dijelaskan kepada rumah sakit, dan tidak ada kelas bug "hak sudah dicabut
> tetapi masih bisa dipakai".
>
> Cache ber-TTL hanya boleh dipertimbangkan kembali bila pengukuran nyata membuktikan ada masalah
> performa. Sampai itu terjadi, ia adalah optimasi tanpa bukti.

### A.2 Butir 7 — bukti yang menutup pertanyaan tenancy

Diperiksa pada sesi ini:

| Yang diperiksa | Hasil |
| --- | --- |
| `Areas/Corporate/HumanResource/MasterData/Organization/Models/MstDepartment.cs` | Properti: `Id`, `DepartmentCode`, `DepartmentName`, `Description`, `IsActive`, `Positions`. **Tidak ada `HospitalId`** |
| `.../Models/MstPosition.cs` | **Tidak ada `HospitalId`** |
| `Models/SysAccessPolicy.cs` | **Tidak ada `HospitalId`** |
| `Models/ApplicationUserOrganization.cs` | **Tidak ada `HospitalId`** |
| `.../Models/MstHospitalSite.cs` | Ada, sebagai master data lokasi tersendiri — tidak dirujuk rantai otorisasi |

**Kesimpulan.** Seluruh rantai otorisasi yang berlaku hari ini — Departemen × Posisi → penempatan
user → policy — **tidak mengenal konsep rumah sakit sama sekali**. Menambahkan `HospitalId` hanya
pada Access Profile akan melahirkan satu dimensi tenancy yang tidak dimiliki lapisan di bawahnya,
dan hasilnya adalah izin yang berbeda-beda tergantung dari sisi mana ia dibaca.

> **Keputusan:** Access Profile **tidak** membawa `HospitalId`. Ia mewarisi cakupan organisasi dari
> pasangan Departemen × Posisi, persis seperti `SysAccessPolicy`.
>
> Bila kelak Quilvian benar-benar membutuhkan tenancy per rumah sakit pada otorisasi, itu adalah
> perubahan lintas platform yang menyentuh `MstDepartment`, `MstPosition`, `SysAccessPolicy`, dan
> `AspNetUserOrganization` sekaligus — blueprint tersendiri, bukan efek samping BE-SEC-002.

---

## B. Klasifikasi Final 19 Business Permission

### B.1 Aturan klasifikasi

Empat kelas sesuai `DECISION 2`:

| Kelas | Arti |
| --- | --- |
| **A. `SAFE_EXISTING_TECHNICAL_MAPPING`** | Technical permission yang dipetakan tidak memberi kemampuan di luar makna bisnis Business Permission-nya. Dapat diaktifkan tanpa pemecahan |
| **B. `REQUIRES_TECHNICAL_PERMISSION_SPLIT`** | Technical permission yang dipetakan membawa serta endpoint bermakna bisnis berbeda. Tidak boleh diaktifkan sebelum dipecah |
| **C. `BROKEN_DEPENDENCY`** | Endpoint yang dibutuhkan tidak ada atau tidak dapat dijangkau. Tidak boleh dipetakan sama sekali |
| **D. `NON_HTTP_RUNTIME`** | Kemampuan bergantung pada jalur non-HTTP atau runtime/perangkat yang tidak ditegakkan technical permission HTTP |

**Urutan presedensi bila satu Business Permission kena lebih dari satu kelas:**
`C` > `D` > `B` > `A`. Kelas sekunder tetap ditampilkan agar tidak hilang.

**Catatan data-scope.** Beberapa technical permission `Read` membuka daftar lintas pasien
(`PatientVitalSign.Read` mencakup `critical-alerts` dan daftar seluruh pasien). Sesuai `DECISION 6`,
kepemilikan/data-scope pasien adalah **lapisan terpisah**. Karena itu keluasan baca tidak
menurunkan kelas menjadi `B`; ia dicatat pada kolom data-scope sebagai kebutuhan lapisan berikutnya.

### B.2 Ringkasan

| Kelas | Jumlah | Kode |
| --- | ---: | --- |
| `A. SAFE_EXISTING_TECHNICAL_MAPPING` | **10** | BP-01, 06, 08, 10, 12, 13, 14, 15, 16, 17 |
| `B. REQUIRES_TECHNICAL_PERMISSION_SPLIT` | **7** | BP-03, 04, 05, 07, 09, 11, 18 |
| `C. BROKEN_DEPENDENCY` | **1** | BP-19 |
| `D. NON_HTTP_RUNTIME` | **1** | BP-02 |
| **Total** | **19** | |

Artinya: **10 dari 19 Business Permission dapat diaktifkan tanpa menyentuh satu baris controller
pun.** Sembilan sisanya tertahan, dan setiap penahanannya punya sebab yang dapat dibuktikan.

### B.3 Tabel klasifikasi lengkap

---

**BP-01 · `health.doctor.outpatient.view` · Buka Rawat Jalan Dokter**

| Field | Isi |
| --- | --- |
| Technical permission sekarang | `DoctorQueue.Read` |
| Endpoint yang dilindungi | 4: `GET /doctor-queues/filters/metadata`, `GET /summary`, `GET /doctor-queues`, `GET /call-lock` |
| **Klasifikasi** | **`A. SAFE_EXISTING_TECHNICAL_MAPPING`** |
| Alasan | Keempat endpoint adalah pembacaan papan antrean yang sama. Tidak ada kemampuan lain yang ikut terbuka |
| Usulan identitas setelah split | Tidak perlu |
| Dampak kompatibilitas | Nihil |
| Sensitivitas | Non-sensitif |
| Data-scope | `OWN` — sudah ditegakkan backend (`GetQueues` hanya mengembalikan antrean dokter yang login) |
| Kelas sekunder | `D` — penyegaran realtime memakai SignalR `/hubs/queues` yang hanya `[Authorize]`. Tidak memblokir: tanpa hub, papan tetap berfungsi lewat polling |

---

**BP-02 · `health.doctor.outpatient.queue.call` · Panggil Pasien**

| Field | Isi |
| --- | --- |
| Technical permission sekarang | `DoctorQueue.Update` (aksi panggil) + **tanpa technical permission** (pengambilan berkas audio) |
| Endpoint yang dilindungi | `POST /doctor-queues/{id}/call` → `DoctorQueue.Update`; `GET /queue-voice/audio/{dateKey}/{fileName}` dan `GET /queue-voice/download/{dateKey}/{fileName}` → hanya `[Authorize]` |
| **Klasifikasi** | **`D. NON_HTTP_RUNTIME`** |
| Alasan | Kaki audio dari kemampuan ini tidak ditegakkan technical permission apa pun, dan sesuai `DECISION 7` ia **bukan** Business Permission dokter melainkan kemampuan runtime/perangkat. Kemampuan tidak dapat dinyatakan lengkap sebelum jalur audio diputuskan |
| Usulan identitas setelah split | `DoctorQueue.Call` untuk aksi dokter; kemampuan audio dipisah — lihat bagian J |
| Dampak kompatibilitas | Memberi identitas pada dua endpoint audio **mengubah himpunan terkunci** `CompatibilityFallbackMatchesApprovedLegacySetExactly` (jumlahnya tetap 69; yang berubah nama aksi dan klasifikasinya — lihat koreksi pada `evidence/03` bagian 16). Test tersebut wajib diperbarui secara sadar |
| Sensitivitas | Non-sensitif untuk aksi panggil; kaki audio menyangkut rekaman nama pasien yang diumumkan |
| Data-scope | `OWN` |
| Kelas sekunder | `B` — `DoctorQueue.Update` juga membuka skip, no-show, requeue, start, finish |

---

**BP-03 · `health.doctor.outpatient.queue.flow` · Kelola Alur Antrean**

| Field | Isi |
| --- | --- |
| Technical permission sekarang | `DoctorQueue.Update` |
| Endpoint yang dilindungi | 6 sekaligus: `call`, `start-consultation`, `finish-consultation`, `skip`, `no-show`, `requeue` |
| **Klasifikasi** | **`B. REQUIRES_TECHNICAL_PERMISSION_SPLIT`** |
| Alasan | BP ini hanya bermaksud melewati, menandai tidak hadir, dan mengembalikan pasien. Pemetaannya ikut memberi memulai dan **menyelesaikan** konsultasi |
| Usulan identitas setelah split | `DoctorQueue.Skip`, `DoctorQueue.NoShow`, `DoctorQueue.Requeue` |
| Dampak kompatibilitas | Setiap pemegang `DoctorQueue.Update` hari ini harus menerima keenam identitas baru agar tidak kehilangan hak (bagian I) |
| Sensitivitas | Non-sensitif |
| Data-scope | `OWN` |

---

**BP-04 · `health.doctor.outpatient.consultation.start` · Mulai Konsultasi**

| Field | Isi |
| --- | --- |
| Technical permission sekarang | `DoctorQueue.Update` + `DoctorConsultation.Create` + `DoctorConsultation.Read` |
| Endpoint yang dilindungi | `POST /doctor-queues/{id}/start-consultation` (dalam bundel 6 endpoint) + `POST /doctor-consultations` (1) + 5 endpoint baca |
| **Klasifikasi** | **`B. REQUIRES_TECHNICAL_PERMISSION_SPLIT`** |
| Alasan | `DoctorConsultation.Create` sudah bersih (1 endpoint). Yang bermasalah hanya `DoctorQueue.Update` |
| Usulan identitas setelah split | `DoctorQueue.StartConsultation` |
| Dampak kompatibilitas | Sama seperti BP-03 |
| Sensitivitas | Non-sensitif |
| Data-scope | `OWN` |

---

**BP-05 · `health.doctor.outpatient.consultation.finalize` · Selesaikan Konsultasi**

| Field | Isi |
| --- | --- |
| Technical permission sekarang | `DoctorQueue.Update` |
| Endpoint yang dilindungi | `POST /doctor-queues/{id}/finish-consultation` (dalam bundel 6 endpoint) |
| **Klasifikasi** | **`B. REQUIRES_TECHNICAL_PERMISSION_SPLIT`** |
| Alasan | Menutup episode pelayanan memicu proses hilir (farmasi, kasir). Ia tidak boleh satu identitas dengan "melewati pasien" |
| Usulan identitas setelah split | `DoctorQueue.FinishConsultation` |
| Dampak kompatibilitas | Sama seperti BP-03 |
| Sensitivitas | **Sensitif** |
| Data-scope | `OWN` — sesuai `DECISION 6`, kepemilikan konsultasi adalah lapisan terpisah |

---

**BP-06 · `health.doctor.outpatient.screening.read` · Lihat Hasil Skrining**

| Field | Isi |
| --- | --- |
| Technical permission sekarang | `PatientAssessment.Read`, `PatientVitalSign.Read` |
| Endpoint yang dilindungi | 4 + 7 |
| **Klasifikasi** | **`A. SAFE_EXISTING_TECHNICAL_MAPPING`** |
| Alasan | Seluruh 11 endpoint bersifat baca pada domain yang sama |
| Usulan identitas setelah split | Tidak perlu |
| Dampak kompatibilitas | Nihil |
| Sensitivitas | Non-sensitif |
| Data-scope | **`ORGANIZATION_SCOPE` — wajib.** `PatientVitalSign.Read` mencakup `critical-alerts` dan daftar tanda vital lintas pasien. Keluasan ini adalah persoalan data-scope, bukan capability |

---

**BP-07 · `health.doctor.outpatient.screening.write` · Isi / Ulangi Skrining Dokter**

| Field | Isi |
| --- | --- |
| Technical permission sekarang | `PatientAssessment.Create`, `PatientAssessment.Update`, `PatientVitalSign.Create`, `PatientVitalSign.Update` |
| Endpoint yang dilindungi | Create: 1 + 1. Update: 3 (`PUT`, `complete`, `cancel`) + 4 (`PUT`, `verify`, `notify-doctor`, `cancel`) |
| **Klasifikasi** | **`B. REQUIRES_TECHNICAL_PERMISSION_SPLIT`** |
| Alasan | `PatientVitalSign.Update` ikut memberi **`verify`** — kendali mutu atas pencatatan orang lain — dan `cancel`. `PatientAssessment.Update` ikut memberi `cancel` |
| Usulan identitas setelah split | `PatientAssessment.Edit`, `PatientAssessment.Complete`, `PatientAssessment.Cancel`; `PatientVitalSign.Edit`, `PatientVitalSign.Verify`, `PatientVitalSign.NotifyDoctor`, `PatientVitalSign.Cancel` |
| Dampak kompatibilitas | Pemegang lama menerima seluruh identitas turunannya (bagian I) |
| Sensitivitas | Non-sensitif untuk pengisian; **`Verify` sensitif** |
| Data-scope | `ORGANIZATION_SCOPE` |

---

**BP-08 · `health.doctor.outpatient.soap.read` · Lihat SOAP**

| Field | Isi |
| --- | --- |
| Technical permission sekarang | `DoctorConsultation.Read` |
| Endpoint yang dilindungi | 5: `filters/metadata`, daftar, `{id}`, `active-by-queue/{queueId}`, `{id}/finalization-validation` |
| **Klasifikasi** | **`A. SAFE_EXISTING_TECHNICAL_MAPPING`** |
| Alasan | Kelimanya baca. `finalization-validation` hanya memvalidasi dan tidak mengubah apa pun |
| Usulan identitas setelah split | Tidak perlu |
| Dampak kompatibilitas | Nihil |
| Sensitivitas | Non-sensitif |
| Data-scope | `ORGANIZATION_SCOPE` |

---

**BP-09 · `health.doctor.outpatient.soap.write` · Tulis SOAP**

| Field | Isi |
| --- | --- |
| Technical permission sekarang | `DoctorConsultation.Update` |
| Endpoint yang dilindungi | 4: `PUT /{id}`, `PATCH /{id}/soap`, `PATCH /{id}/complete`, `PATCH /{id}/cancel` |
| **Klasifikasi** | **`B. REQUIRES_TECHNICAL_PERMISSION_SPLIT`** |
| Alasan | Persis kasus `DECISION 6`. Dibuktikan dari source: `UpdateSoap` (`DoctorConsultationController.cs:503`) dan `CompleteConsultation` (`:596`) sama-sama `[AccessPermission("DoctorConsultation", "Update")]` |
| Usulan identitas setelah split | `DoctorConsultation.WriteSoap`, `DoctorConsultation.Complete`, `DoctorConsultation.Edit`, `DoctorConsultation.Cancel` |
| Dampak kompatibilitas | Pemegang lama menerima keempatnya (bagian I) |
| Sensitivitas | **Sensitif** — isi rekam medis klinis |
| Data-scope | `ORGANIZATION_SCOPE` |

---

**BP-10 · `health.doctor.outpatient.diagnosis.read` · Lihat Diagnosis**

| Field | Isi |
| --- | --- |
| Technical permission sekarang | `PatientDiagnosis.Read` (wajib), `DiagnosisRecommendationResolver.Read` (opsional) |
| Endpoint yang dilindungi | 5 + 1 |
| **Klasifikasi** | **`A. SAFE_EXISTING_TECHNICAL_MAPPING`** |
| Alasan | Seluruhnya baca. `DiagnosisRecommendationResolver.Read` melindungi tepat satu endpoint (`POST /resolve` yang bersifat baca) |
| Usulan identitas setelah split | Tidak perlu |
| Dampak kompatibilitas | Nihil |
| Sensitivitas | Non-sensitif |
| Data-scope | `ORGANIZATION_SCOPE` |

---

**BP-11 · `health.doctor.outpatient.diagnosis.write` · Tetapkan Diagnosis**

| Field | Isi |
| --- | --- |
| Technical permission sekarang | `PatientDiagnosis.Create`, `PatientDiagnosis.Update` |
| Endpoint yang dilindungi | Create: 1. Update: 4 (`PUT`, `set-primary`, `resolve`, `cancel`) |
| **Klasifikasi** | **`B. REQUIRES_TECHNICAL_PERMISSION_SPLIT`** |
| Alasan | Frontend hanya memakai `set-primary` dan `cancel`. Pemetaannya ikut memberi `resolve` — pernyataan klinis bahwa diagnosis sudah teratasi — yang tidak ada di layar dokter rawat jalan |
| Usulan identitas setelah split | `PatientDiagnosis.Edit`, `PatientDiagnosis.SetPrimary`, `PatientDiagnosis.Resolve`, `PatientDiagnosis.Cancel` |
| Dampak kompatibilitas | Pemegang lama menerima keempatnya |
| Sensitivitas | **Sensitif** — diagnosis memengaruhi penagihan dan pelaporan |
| Data-scope | `ORGANIZATION_SCOPE` |
| Prioritas | Rendah dibanding BP-18 dan BP-09; boleh masuk gelombang kedua |

---

**BP-12 · `health.doctor.outpatient.cppt.read` · Lihat CPPT**

| Field | Isi |
| --- | --- |
| Technical permission sekarang | `PatientIntegratedProgressNote.Read` |
| Endpoint yang dilindungi | 5, termasuk `draft-from-consultation` yang membuat draft **tanpa menyimpan** |
| **Klasifikasi** | **`A. SAFE_EXISTING_TECHNICAL_MAPPING`** |
| Alasan | Seluruhnya baca. Draft tidak dipersistensi |
| Usulan identitas setelah split | Tidak perlu |
| Dampak kompatibilitas | Nihil |
| Sensitivitas | Non-sensitif |
| Data-scope | `ORGANIZATION_SCOPE` |

---

**BP-13 · `health.doctor.outpatient.cppt.write` · Tulis CPPT dari SOAP**

| Field | Isi |
| --- | --- |
| Technical permission sekarang | `PatientIntegratedProgressNote.Create` |
| Endpoint yang dilindungi | 2: `POST /patient-integrated-progress-notes`, `POST /from-consultation/{consultationId}` |
| **Klasifikasi** | **`A. SAFE_EXISTING_TECHNICAL_MAPPING`** |
| Alasan | Kedua endpoint membuat CPPT. Tidak ada kemampuan lain yang ikut terbuka. `Update` dan `Delete` CPPT **tidak** dipetakan |
| Usulan identitas setelah split | Tidak perlu |
| Dampak kompatibilitas | Nihil |
| Sensitivitas | **Sensitif** — masuk rekam medis resmi |
| Data-scope | `ORGANIZATION_SCOPE` |

---

**BP-14 · `health.doctor.outpatient.prescription.read` · Lihat Resep**

| Field | Isi |
| --- | --- |
| Technical permission sekarang | Wajib: `Prescription.Read`, `PrescriptionWorkspace.Read`, `PrescribingDrug.Read`. Opsional: `Drug.Read`, `DrugUnitConversion.Read`, `Measurement.Read` |
| Endpoint yang dilindungi | Wajib: 5 + 2 + 3. Opsional: 6 + 5 + 5 |
| **Klasifikasi** | **`A. SAFE_EXISTING_TECHNICAL_MAPPING`** |
| Alasan | Seluruh pemetaan wajib bersifat baca dalam domain resep. Yang opsional membuka katalog master data penuh, karena itu **tidak** disertakan dalam profil dasar |
| Usulan identitas setelah split | Tidak perlu untuk pemetaan wajib. `Drug.Read` (6 endpoint termasuk `summary` dan daftar penuh) adalah kandidat pemecahan gelombang kedua |
| Dampak kompatibilitas | Nihil |
| Sensitivitas | Non-sensitif |
| Data-scope | `ORGANIZATION_SCOPE` |
| Aturan wajib | Ketiga pemetaan opsional **tidak boleh** masuk `DOCTOR_OUTPATIENT_BASE` secara default. Bukti bahwa fitur tetap jalan tanpanya ada pada blok `catch` di `prescribing-drug.service.js:264` dan `:325` |

---

**BP-15 · `health.doctor.outpatient.prescription.write` · Tulis Resep**

| Field | Isi |
| --- | --- |
| Technical permission sekarang | `Prescription.Create`, `PrescriptionWorkspace.Update` |
| Endpoint yang dilindungi | 1 (`POST /prescriptions`) + 1 (`PATCH /prescription-workspaces/{id}/autosave`) |
| **Klasifikasi** | **`A. SAFE_EXISTING_TECHNICAL_MAPPING`** |
| Alasan | Kedua identitas melindungi tepat satu endpoint masing-masing. `Prescription.Update` (yang membawa `cancel`) **tidak** dipetakan karena frontend tidak memakainya |
| Usulan identitas setelah split | Tidak perlu |
| Dampak kompatibilitas | Nihil |
| Sensitivitas | **Sensitif** — resep obat |
| Data-scope | `ORGANIZATION_SCOPE` |

---

**BP-16 · `health.doctor.outpatient.prescription.template` · Gunakan dan Simpan Template Resep**

| Field | Isi |
| --- | --- |
| Technical permission sekarang | `PrescriptionTemplate.Read`, `PrescriptionTemplate.Create` |
| Endpoint yang dilindungi | Read: 3. Create: 3 (`POST`, `from-prescription`, `{id}/apply`) |
| **Klasifikasi** | **`A. SAFE_EXISTING_TECHNICAL_MAPPING`** |
| Alasan | Makna bisnis BP ini memang mencakup ketiga endpoint `Create`. Pemetaannya tidak melebihi maknanya |
| Usulan identitas setelah split | Tidak wajib. Bila kelak "memakai template" ingin dipisah dari "membuat template", identitasnya `PrescriptionTemplate.Apply` — gelombang kedua |
| Dampak kompatibilitas | Nihil |
| Sensitivitas | Non-sensitif |
| Data-scope | `ORGANIZATION_SCOPE` |
| Catatan jujur | Kelas `A` di sini tercapai karena BP-nya **diperlebar** agar cocok dengan identitas yang kasar, bukan karena identitasnya halus. Ini pertukaran yang disengaja dan dicatat |

---

**BP-17 · `health.doctor.outpatient.procedure.read` · Lihat Tindakan**

| Field | Isi |
| --- | --- |
| Technical permission sekarang | `PatientProcedure.Read` |
| Endpoint yang dilindungi | 5: `filters/metadata`, `master-options`, daftar, `options`, `{id}` |
| **Klasifikasi** | **`A. SAFE_EXISTING_TECHNICAL_MAPPING`** |
| Alasan | Seluruhnya baca |
| Usulan identitas setelah split | Tidak perlu |
| Dampak kompatibilitas | Nihil |
| Sensitivitas | Non-sensitif |
| Data-scope | `ORGANIZATION_SCOPE` |

---

**BP-18 · `health.doctor.outpatient.procedure.write` · Pilih Tindakan**

| Field | Isi |
| --- | --- |
| Technical permission sekarang | `PatientProcedure.Create`, `PatientProcedure.Update` |
| Endpoint yang dilindungi | Create: 2 (`POST /select`, `POST`). Update: 5 (`PUT`, `approve`, `execute`, `remove-draft`, `cancel`) |
| **Klasifikasi** | **`B. REQUIRES_TECHNICAL_PERMISSION_SPLIT`** — kasus terparah pada pilot |
| Alasan | Frontend hanya memakai `POST /select` dan `PATCH /remove-draft`. Pemetaannya ikut memberi `approve`, `execute`, dan `cancel`. Bukti bahwa ketiganya kewenangan berbeda ada di dalam source sendiri (bagian D.2) |
| Usulan identitas setelah split | `PatientProcedure.Select`, `PatientProcedure.Create`, `PatientProcedure.Edit`, `PatientProcedure.RemoveDraft`, `PatientProcedure.Approve`, `PatientProcedure.Execute`, `PatientProcedure.Cancel` |
| Dampak kompatibilitas | Pemegang lama menerima seluruh turunannya. **Penyempitan hak adalah tindakan owner terpisah, bukan efek migrasi** |
| Sensitivitas | **Sensitif** — tindakan menimbulkan tagihan |
| Data-scope | `ORGANIZATION_SCOPE` |
| Pemecahan BP setelah split | BP-18 dipecah menjadi `procedure.select`, `procedure.execute`, `procedure.approve`, `procedure.cancel` — lihat bagian D.4 |

---

**BP-19 · `health.doctor.outpatient.certificate.write` · Buat Surat Dokter**

| Field | Isi |
| --- | --- |
| Technical permission sekarang | **Tidak ada** |
| Endpoint yang dilindungi | Frontend memanggil `POST /clinical-management/doctor-certificates` dan `PUT /clinical-management/doctor-certificates/{id}` — **keduanya tidak terdaftar di backend** |
| **Klasifikasi** | **`C. BROKEN_DEPENDENCY`** |
| Alasan | Sesuai arahan Broken Certificate: dilarang membuat permission mapping ke endpoint yang tidak ada |
| Usulan identitas setelah split | Tidak berlaku. Kandidat target `MedicalCertificate.*`, tetapi **tidak boleh dipetakan** sebelum route diperbaiki task terpisah |
| Dampak kompatibilitas | Nihil selama tidak dipetakan |
| Sensitivitas | **Sensitif** — dokumen bertanda tangan dokter |
| Data-scope | `ORGANIZATION_SCOPE` |
| Status katalog | Kode **boleh** didaftarkan sebagai katalog dengan status `BLOCKED`; barisnya tidak punya pemetaan dan karena itu tidak pernah memberi hak (fail closed secara konstruksi) |

---

## C. Technical Permission Split Matrix

### C.1 Aturan yang mengikat seluruh pemecahan

Diturunkan dari arahan **Legacy Permission Split Safety** dan dari kontrak kanonik `BE-SEC-001`:

| Aturan | Isi |
| --- | --- |
| **Identitas tetap `(resource, action)`** | Pemecahan hanya mengganti **nama aksi**. Bentuk `[AccessPermission(resource, action)]` tidak berubah. Aturan kanonik `BE-SEC-001` tidak dilanggar |
| **`AccessType` tidak ikut berubah** | Seluruh identitas baru tetap memakai `AccessType = Update` (atau sesuai aslinya). `AccessType` adalah metadata kolom layar, bukan identitas — aturan kanonik nomor 3 dan 4 |
| **Expansion wajib terbukti** | Satu identitas lama boleh mengembang menjadi banyak identitas baru **hanya** untuk endpoint yang memang dijaga identitas lama itu. Dibuktikan dengan refleksi assembly, bukan penalaran bisnis |
| **Exact historical capability set** | Setiap baris `SysAccessPolicy` yang menunjuk identitas lama diperluas menjadi **tepat** himpunan identitas turunannya. Tidak lebih, tidak kurang |
| **Penyempitan bukan bagian migrasi** | Pemecahan **tidak** mencabut hak siapa pun. Bila owner ingin dokter berhenti bisa `approve`, itu tindakan terpisah dan sadar sesudah pemecahan selesai |
| **Nol tebakan** | Tidak ada pemetaan satu-ke-banyak berdasarkan penalaran bisnis. Sumbernya adalah daftar endpoint yang benar-benar dijaga identitas lama |

### C.2 Matriks dampak — kolom yang belum dapat diisi

Arahan meminta kolom `Existing SysAccessPolicy count`, `Department`, `Position`, dan `User impact`.
Keempatnya adalah **data database**, bukan data source.

> **Batas yang jujur.** Task ini adalah audit read-only tanpa wewenang database. Angka per-identitas
> **tidak dapat** diisi sekarang, dan menebaknya berarti mengarang. Laporan `BE-SEC-001` hanya
> memberi total pada database development: `SysAccessPolicy` 498 baris, 11 pasangan
> Departemen × Posisi, 452 proyeksi menunjuk registry hidup. Angka per-identitas tidak pernah
> dipublikasikan.

Karena itu keempat kolom ditandai `PENDING READ-ONLY QUERY` dan pengisiannya menjadi **prasyarat
masuk** `BE-SEC-003`, bukan bagian dari pelaksanaannya. Bentuk laporan yang dibutuhkan:

```
Untuk setiap identitas lama yang akan dipecah, hasilkan baris:
  ResourceName, ActionName,
  ControllerAccessId, ActionAccessId,
  jumlah SysAccessPolicy aktif yang menunjuknya,
  daftar DepartmentId + DepartmentName,
  daftar PositionId + PositionName,
  perkiraan jumlah user terdampak lewat AspNetUserOrganization yang masih sah
Sumber: SysAccessPolicy ⋈ SysActionAccess ⋈ SysControllerAccess ⋈ AspNetUserOrganization
Sifat: SELECT saja, tanpa penulisan
```

Laporan itu dijalankan pada lingkungan yang akan menerima perubahan, satu per lingkungan, dan
ditinjau manusia sebelum penulisan apa pun — persis prosedur yang dipakai `BE-SEC-001`.

### C.3 Matriks pemecahan — kolom yang sudah terbukti dari source

| # | Identitas lama | Endpoint yang dijaga | Identitas baru | Jumlah | Aksi sensitif di dalamnya | `SysAccessPolicy` | Perilaku migrasi |
| ---: | --- | ---: | --- | ---: | --- | --- | --- |
| 1 | `DoctorQueue.Update` | 6 | `Call`, `StartConsultation`, `FinishConsultation`, `Skip`, `NoShow`, `Requeue` | 6 | `FinishConsultation` | `PENDING` | 1 baris → 6 baris, exact set |
| 2 | `DoctorConsultation.Update` | 4 | `Edit`, `WriteSoap`, `Complete`, `Cancel` | 4 | `Complete`, `Cancel` | `PENDING` | 1 → 4, exact set |
| 3 | `PatientProcedure.Update` | 5 | `Edit`, `RemoveDraft`, `Approve`, `Execute`, `Cancel` | 5 | `Approve`, `Execute`, `Cancel` | `PENDING` | 1 → 5, exact set |
| 4 | `PatientProcedure.Create` | 2 | `Select`, `Create` | 2 | — | `PENDING` | 1 → 2, exact set |
| 5 | `PatientVitalSign.Update` | 4 | `Edit`, `Verify`, `NotifyDoctor`, `Cancel` | 4 | `Verify` | `PENDING` | 1 → 4, exact set |
| 6 | `PatientAssessment.Update` | 3 | `Edit`, `Complete`, `Cancel` | 3 | — | `PENDING` | 1 → 3, exact set |
| 7 | `PatientDiagnosis.Update` | 4 | `Edit`, `SetPrimary`, `Resolve`, `Cancel` | 4 | `Resolve` | `PENDING` | 1 → 4, exact set |
| 8 | *(tanpa identitas)* audio antrean | 2 | lihat bagian J | — | — | Tidak ada | Penambahan identitas baru, bukan pemecahan |

**Total pilot: 7 identitas lama → 28 identitas baru, ditambah 1 keputusan identitas audio.**

Empat identitas kasar lain **sengaja tidak** masuk `BE-SEC-003` karena tidak memblokir satu pun
dari 19 Business Permission pilot:

| Identitas | Endpoint | Mengapa ditunda |
| --- | ---: | --- |
| `PrescriptionTemplate.Create` | 3 | BP-16 diklasifikasi `A`; pemecahan hanya diperlukan bila owner ingin memisahkan "pakai template" dari "buat template" |
| `Prescription.Update` | 2 | Tidak dipetakan Business Permission pilot mana pun |
| `Drug.Read` | 6 | Pemetaan opsional; tidak masuk profil dasar |
| `MedicalCertificate.Update` | **7** | Terkait BP-19 yang berstatus `BROKEN_DEPENDENCY`. Ini identitas paling kasar di seluruh audit — memuat `issue`, `verify`, `approve`, `reject`, `revoke`, `cancel` dalam satu izin. **Wajib** dipecah sebelum surat dokter pernah diaktifkan |

---

## D. Usulan Pemecahan `PatientProcedure`

### D.1 Keadaan sekarang

`Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs`

| Endpoint | Action | Identitas sekarang |
| --- | --- | --- |
| `GET /filters/metadata` | `GetFilterMetadata` | `PatientProcedure.Read` |
| `GET /master-options` | `GetMasterProcedureOptions` | `PatientProcedure.Read` |
| `GET /` | `GetProcedures` | `PatientProcedure.Read` |
| `GET /options` | `GetProcedureOptions` | `PatientProcedure.Read` |
| `GET /{id}` | `GetById` | `PatientProcedure.Read` |
| `POST /select` | `SelectProcedure` | `PatientProcedure.Create` |
| `POST /` | `CreateProcedure` | `PatientProcedure.Create` |
| `PUT /{id}` | `UpdateProcedure` | `PatientProcedure.Update` |
| `PATCH /{id}/approve` | `ApproveProcedure` | `PatientProcedure.Update` |
| `PATCH /{id}/execute` | `ExecuteProcedure` | `PatientProcedure.Update` |
| `PATCH /{id}/remove-draft` | `RemoveDraftProcedure` | `PatientProcedure.Update` |
| `PATCH /{id}/cancel` | `CancelProcedure` | `PatientProcedure.Update` |

### D.2 Bukti dari source bahwa Approve, Execute, dan Cancel memang kewenangan berbeda

Ini bukan penalaran bisnis — ini tertulis di dalam controller.

**Approve adalah gerbang sebelum Execute.** `ExecuteProcedure` menolak bila belum disetujui:

```csharp
if (entity.IsNeedApproval && !entity.IsApproved)
{
    return BadRequest(ApiResponse<object>.Fail(
        StatusCodes.Status400BadRequest, ...));
}
```

Sebuah alur yang menempatkan approve sebagai syarat execute **tidak mungkin** dimaksudkan agar
pelakunya orang yang sama dengan izin yang sama. Bila satu izin memberi keduanya, gerbang itu
kehilangan artinya.

**Cancel dijaga oleh billing.** `CancelProcedure` menolak bila tagihan sudah terbit:

```csharp
if (entity.IsBillingGenerated)
{
    return BadRequest(ApiResponse<object>.Fail(
        StatusCodes.Status400BadRequest,
        "Tindakan yang sudah masuk billing tidak dapat dibatalkan dari modul klinis."
    ));
}
```

Cancel juga menulis `CancelledByUserId`, `CancelReason`, dan mengubah `ProcedureStatus` menjadi
`Cancelled`. Ia adalah pembatalan transaksi berkonsekuensi finansial, bukan pembersihan draft.

**Execute mengubah status menjadi selesai:**

```csharp
entity.ProcedureStatus = PatientProcedureStatus.Completed;
entity.IsExecuted = true;
entity.ExecutedAt = now;
entity.ExecutedByUserId = actorUserId;
```

**RemoveDraft berbeda dari Cancel.** `RemoveDraftProcedure` bekerja pada tindakan yang masih
menempel pada konsultasi draft. Inilah yang benar-benar dipakai dokter rawat jalan di layar.

### D.3 Identitas yang diusulkan

| Endpoint | Identitas sekarang | **Identitas baru** | `AccessType` | Sensitivitas |
| --- | --- | --- | --- | --- |
| 5 endpoint `GET` | `PatientProcedure.Read` | `PatientProcedure.Read` *(tidak berubah)* | `Read` | Non-sensitif |
| `POST /select` | `PatientProcedure.Create` | **`PatientProcedure.Select`** | `Create` | Non-sensitif |
| `POST /` | `PatientProcedure.Create` | **`PatientProcedure.Create`** | `Create` | Non-sensitif |
| `PUT /{id}` | `PatientProcedure.Update` | **`PatientProcedure.Edit`** | `Update` | Non-sensitif |
| `PATCH /{id}/remove-draft` | `PatientProcedure.Update` | **`PatientProcedure.RemoveDraft`** | `Update` | Non-sensitif |
| `PATCH /{id}/approve` | `PatientProcedure.Update` | **`PatientProcedure.Approve`** | `Update` | **Sensitif** |
| `PATCH /{id}/execute` | `PatientProcedure.Update` | **`PatientProcedure.Execute`** | `Update` | **Sensitif** |
| `PATCH /{id}/cancel` | `PatientProcedure.Update` | **`PatientProcedure.Cancel`** | `Update` | **Sensitif** |

**Catatan atas `Select` versus `Create`.** `DECISION 5` menuliskan keduanya sebagai satu butir
(`procedure.select/create`). Usulan ini memisahkannya menjadi dua identitas teknis, dengan alasan
yang dapat dibuktikan: frontend pilot **hanya** memakai `POST /select`
(`use-doctor-procedure.js` → `selectPatientProcedure`) dan tidak pernah memakai `POST /`.
Menyatukan keduanya berarti memberi dokter satu kemampuan yang tidak ia pakai. Pemisahan ini tidak
menambah beban admin karena keduanya tetap berada di bawah satu Business Permission.

**Catatan atas `Edit`.** `DECISION 5` menulis "`procedure.edit` jika source membuktikan fungsi edit
diperlukan". **Source tidak membuktikannya.** Frontend pilot tidak pernah memanggil `PUT /{id}`.
Karena itu identitas `PatientProcedure.Edit` tetap dibuat — endpoint-nya ada dan wajib punya
identitas — tetapi **tidak ada Business Permission pilot yang memetakannya**.

### D.4 Business Permission setelah pemecahan

BP-18 pecah menjadi empat, menggantikan satu baris katalog lama:

| Kode | Nama tampil | Technical permission | Masuk `DOCTOR_OUTPATIENT_BASE`? | Sensitivitas |
| --- | --- | --- | --- | --- |
| `health.doctor.outpatient.procedure.select` | Pilih Tindakan | `PatientProcedure.Select`, `PatientProcedure.RemoveDraft` | **Ya** | Non-sensitif |
| `health.doctor.outpatient.procedure.execute` | Laksanakan Tindakan | `PatientProcedure.Execute` | **Tidak** — profil terpisah | **Sensitif** |
| `health.doctor.outpatient.procedure.approve` | Setujui Tindakan | `PatientProcedure.Approve` | **Tidak — FAIL CLOSED** | **Sensitif** |
| `health.doctor.outpatient.procedure.cancel` | Batalkan Tindakan | `PatientProcedure.Cancel` | **Tidak** — menunggu keputusan | **Sensitif** |

Sesuai `DECISION 5`:

- `procedure.execute` boleh diberikan kepada Departemen × Posisi atau profil dokter yang memang
  berwenang melakukan tindakan. **Tidak** otomatis untuk seluruh dokter hanya karena `UserType`
  atau role dokter.
- `procedure.approve` **tidak pernah** masuk `DOCTOR_OUTPATIENT_BASE`. Default fail closed sampai
  Departemen × Posisi approver ditetapkan. Dokter yang membuat tindakan **tidak** otomatis menjadi
  penyetujunya.

**Lapisan clinical privilege terpisah.** `DECISION 5` meminta desain memungkinkan pemeriksaan
kewenangan/kredensial klinis pada tindakan tertentu sebagai lapisan tersendiri. Model data BE-SEC-002
mendukungnya tanpa perubahan: `SecBusinessPermission` sudah memuat kolom kebutuhan data-scope, dan
lapisan clinical privilege kelak menjadi **pemeriksaan tambahan setelah** izin lolos — bukan
pengganti izin. Repository sudah punya bahannya di
`Areas/Corporate/HumanResource/CredentialingManagement/` (`WfpCertification`). Penegakannya
**bukan** cakupan BE-SEC-002 maupun `BE-SEC-003`.

---

## E. Usulan Pemecahan `DoctorConsultation`

### E.1 Bukti yang diminta `DECISION 6`

> "Jika audit membuktikan keduanya berada di `DoctorConsultation.Update`: classification
> `REQUIRES_TECHNICAL_PERMISSION_SPLIT`."

**Terbukti.** Dari `Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs`:

| Baris | Action | Endpoint | Identitas |
| ---: | --- | --- | --- |
| 503 | `UpdateSoap` | `PATCH /{id}/soap` | `[AccessPermission("DoctorConsultation", "Update")]` |
| 596 | `CompleteConsultation` | `PATCH /{id}/complete` | `[AccessPermission("DoctorConsultation", "Update")]` |

Keduanya identik. Klasifikasi `B. REQUIRES_TECHNICAL_PERMISSION_SPLIT` ditetapkan.

### E.2 Identitas yang diusulkan

| Endpoint | Identitas sekarang | **Identitas baru** | `AccessType` | Sensitivitas |
| --- | --- | --- | --- | --- |
| `GET /filters/metadata` | `DoctorConsultation.Read` | tidak berubah | `Read` | Non-sensitif |
| `GET /` | `DoctorConsultation.Read` | tidak berubah | `Read` | Non-sensitif |
| `GET /{id}` | `DoctorConsultation.Read` | tidak berubah | `Read` | Non-sensitif |
| `GET /active-by-queue/{queueId}` | `DoctorConsultation.Read` | tidak berubah | `Read` | Non-sensitif |
| `GET /{id}/finalization-validation` | `DoctorConsultation.Read` | tidak berubah | `Read` | Non-sensitif |
| `POST /` | `DoctorConsultation.Create` | tidak berubah | `Create` | Non-sensitif |
| `PUT /{id}` | `DoctorConsultation.Update` | **`DoctorConsultation.Edit`** | `Update` | Non-sensitif |
| `PATCH /{id}/soap` | `DoctorConsultation.Update` | **`DoctorConsultation.WriteSoap`** | `Update` | **Sensitif** |
| `PATCH /{id}/complete` | `DoctorConsultation.Update` | **`DoctorConsultation.Complete`** | `Update` | **Sensitif** |
| `PATCH /{id}/cancel` | `DoctorConsultation.Update` | **`DoctorConsultation.Cancel`** | `Update` | **Sensitif** |

### E.3 Business Permission setelah pemecahan

| Kode | Nama tampil | Technical permission | Masuk `DOCTOR_OUTPATIENT_BASE`? |
| --- | --- | --- | --- |
| `health.doctor.outpatient.consultation.write` | Tulis SOAP | `DoctorConsultation.WriteSoap` | **Ya** |
| `health.doctor.outpatient.consultation.complete` | Selesaikan Konsultasi | `DoctorConsultation.Complete`, `DoctorQueue.FinishConsultation` | **Ya**, untuk dokter yang berwenang atas konsultasi tersebut |
| `health.doctor.outpatient.consultation.cancel` | Batalkan Konsultasi | `DoctorConsultation.Cancel` | **Tidak** — menunggu keputusan |

Sesuai `DECISION 6`, `consultation.complete` **boleh** masuk profil dasar, tetapi
**kepemilikan/data-scope pasien tetap lapisan terpisah**: izin menjawab "boleh menyelesaikan
konsultasi", bukan "boleh menyelesaikan konsultasi **siapa saja**".

Perhatikan bahwa `consultation.complete` memetakan **dua** identitas dari **dua** controller
berbeda. Frontend pilot hari ini memakai jalur antrean (`POST /doctor-queues/{id}/finish-consultation`),
sementara jalur klinis (`PATCH /doctor-consultations/{id}/complete`) ada tetapi belum dipakai.
Memetakan keduanya membuat Business Permission tetap benar apa pun jalur yang dipakai frontend
kelak — inilah alasan lapisan bisnis dibuat.

---

## F. Technical Permission Kasar Lain yang Ditemukan

Seluruh identitas di bawah diukur dengan refleksi atas atribut source, bukan perkiraan.

### F.1 Di dalam jangkauan pilot — masuk `BE-SEC-003`

| Identitas | Endpoint | Isi bundel | Mengapa masalah |
| --- | ---: | --- | --- |
| `DoctorQueue.Update` | 6 | `call`, `start-consultation`, `finish-consultation`, `skip`, `no-show`, `requeue` | Menyatukan alur antrean dengan lifecycle konsultasi. Membuat BP-02, 03, 04, dan 05 **tidak dapat dibedakan** |
| `PatientVitalSign.Update` | 4 | `PUT`, `verify`, `notify-doctor`, `cancel` | `verify` adalah kendali mutu atas catatan orang lain |
| `PatientAssessment.Update` | 3 | `PUT`, `complete`, `cancel` | `cancel` membatalkan dokumen pengkajian |
| `PatientDiagnosis.Update` | 4 | `PUT`, `set-primary`, `resolve`, `cancel` | `resolve` adalah pernyataan klinis yang tidak ada di layar dokter rawat jalan |
| `PatientProcedure.Create` | 2 | `select`, `POST` | Frontend hanya memakai `select` |

### F.2 Di luar jangkauan pilot — ditunda dengan alasan

| Identitas | Endpoint | Isi bundel | Alasan ditunda |
| --- | ---: | --- | --- |
| **`MedicalCertificate.Update`** | **7** | `PUT`, `issue`, `verify`, `approve`, `reject`, `revoke`, `cancel` | **Identitas paling kasar di seluruh audit.** Terkait BP-19 yang `BROKEN_DEPENDENCY`. Wajib dipecah sebelum surat dokter pernah diaktifkan, tetapi bukan sekarang |
| `PrescriptionTemplate.Create` | 3 | `POST`, `from-prescription`, `apply` | BP-16 sudah `A`; pemecahan bersifat penyempurnaan |
| `Prescription.Update` | 2 | `PUT`, `cancel` | Tidak dipetakan BP pilot |
| `Drug.Read` | 6 | `filters/metadata`, `summary`, daftar, `options`, `{id}/clinical-information`, `{id}` | Pemetaan opsional; tidak masuk profil dasar |
| `PatientIntegratedProgressNote.Update` | 2 | `PUT`, `cancel` | Tidak dipetakan BP pilot |
| `DrugUnitConversion.Read`, `Measurement.Read` | 5 masing-masing | metadata, summary, daftar, options, detail | Opsional; membuka katalog master data penuh |

### F.3 Bacaan yang melebar lintas pasien — persoalan data-scope, bukan capability

| Identitas | Endpoint yang melebar | Catatan |
| --- | --- | --- |
| `PatientVitalSign.Read` | `critical-alerts`, daftar lintas pasien | Sesuai `DECISION 6`, ditangani lapisan data-scope terpisah |
| `PatientAssessment.Read` | daftar lintas pasien | Sama |
| `PatientDiagnosis.Read`, `PatientProcedure.Read` | daftar lintas pasien | Sama |

Keempatnya **tidak** menurunkan kelas Business Permission menjadi `B`, karena yang berlebih adalah
**jangkauan data**, bukan **jenis kemampuan**.

---

## G. Usulan Final Registry Prefix

### G.1 Audit collision — hasil

Diperiksa pada SHA `e1d1121`:

| Yang diperiksa | Perintah/cakupan | Hasil |
| --- | --- | --- |
| Class/record/interface/enum berawalan `Sec` | Seluruh `*.cs` di repository | **Nol** |
| Nama tabel berawalan `Sec` | `[Table("Sec…")]` | **Nol** |
| Berkas bernama `Sec*.cs` | Seluruh repository di luar `obj/` dan `bin/` | **Nol** |
| `DbSet<Sec…>` | `Repositories/ApplicationDbContext.cs` | **Nol** |
| `Sec` pada tabel registry | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | **Belum terdaftar** |
| Prefix yang sudah dipakai | Kolom Prefix pada registry | `Bil`, `Cli`, `Emg`, `Fin`, `Hrd`, `Inp`, `Ins`, `Lab`, `Mrc`, `Mst`, `Opr`, `Out`, `Pat`, `Phm`, `Rad`, `Reg`, `Wfl`, `Wfp` — **18 baris, `Sec` tidak ada** |

> **Kesimpulan: TIDAK ADA CONFLICT.**
>
> Sesuai `DECISION 3` — *"Jika tidak ada conflict berdasarkan repository evidence, perlakukan
> proposal ini sebagai OWNER APPROVED untuk registry update"* — usulan ini berstatus
> **OWNER APPROVED**.

### G.2 Physical placement

| Yang diperiksa | Hasil |
| --- | --- |
| `Areas/Administrator/` isi saat ini | `MasterData/`, `Setting/` |
| Folder `PlatformAuthorization` | **Belum ada** — akan dibuat `BE-SEC-004` |
| Folder bernama `Security` di bawah `Areas/` | **Tidak ada** |
| Catatan | `Services/Security/` sudah ada di akar, berisi service tak-terpersistensi (`AccessPermissionService`, `PermissionRegistryDescriptor`, `PermissionRegistryValidator`, `OrganizationAuthorizationProjectionService`). Ini **bukan** folder model dan tidak bertabrakan dengan penempatan yang diusulkan |

Penempatan yang diusulkan: `Areas/Administrator/PlatformAuthorization/`, dengan submap
`Models/`, `DTOs/`, `Controllers/`, mengikuti pola Area lain.

### G.3 Baris registry final

Baris berikut **belum ditambahkan** ke `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`.
Penambahannya adalah langkah pertama `BE-SEC-004`.

| Area | Module/pemilik | Category | Prefix | Lifecycle |
|---|---|---|---|---|
| `Administrator` | `PlatformAuthorization / Platform Authorization & Access Control` | `SHARED PLATFORM CAPABILITY` | `Sec` | `ACTIVE` |

Tambahan pada tabel *Kepanjangan prefix*:

| Prefix | Kepanjangan |
|---|---|
| `Sec` | Security |

### G.4 Status `Sys*` warisan

Sesuai `DECISION 3`: `SysApplicationModule`, `SysControllerAccess`, `SysActionAccess`, dan
`SysAccessPolicy` tetap **legacy/technical system registry**, tidak dinormalisasi pada task ini,
dan **tidak** didaftarkan sebagai prefix milik `PlatformAuthorization`.

Konsekuensi yang harus diterima secara sadar: untuk sementara ada dua penampilan pada satu
kemampuan — tabel registry teknis berawalan `Sys`, tabel bisnis berawalan `Sec`. Ini pertukaran
yang disengaja: memisahkan keduanya membuat batas "mana yang warisan, mana yang baru" terbaca dari
namanya sendiri, dan menghindari menormalisasi empat tabel yang baru saja distabilkan `BE-SEC-001`.

### G.5 Nama entity — usulan berbasis model data

`DECISION 3` menyatakan nama final harus dibuktikan model data. Tujuh konsep pada bagian O audit
awal dipetakan menjadi:

| # | Konsep | Nama usulan `DECISION 3` | **Nama final yang diusulkan** | Alasan |
| ---: | --- | --- | --- | --- |
| 1 | Simpul hierarki bisnis | `SecBusinessFeature` | `SecBusinessFeature` | Diterima apa adanya |
| 2 | Business Permission | `SecPermission` | **`SecBusinessPermission`** | `SecPermission` terlalu umum dan akan tertukar dengan `SysActionAccess` yang juga "permission". Nama wajib menyebut *business* karena justru itu pembedanya |
| 3 | Pemetaan ke technical permission | *(belum disebut)* | **`SecBusinessPermissionMapping`** | Dibutuhkan model data; satu-satunya tempat nama teknis boleh muncul |
| 4 | Access Profile | `SecAccessProfile` | `SecAccessProfile` | Diterima |
| 5 | Isi profil | `SecAccessProfilePermission` | `SecAccessProfilePermission` | Diterima |
| 6 | Penetapan profil ke organisasi | `SecOrganizationAccessProfile` | `SecOrganizationAccessProfile` | Diterima |
| 7 | Override langsung aditif | *(belum disebut)* | **`SecOrganizationPermissionGrant`** | `DECISION 4` menegaskan override hanya `ADDITIVE GRANT`. Kata `Grant` membuat sifat aditifnya terbaca dari nama, sehingga penambahan kolom DENY kelak akan terasa salah sejak namanya |

Tujuh entity. Tidak ada yang dibuat sekarang.

---

## H. Aturan Komposisi Access Profile

Diturunkan dari `DECISION 4`.

### H.1 Aturan yang mengikat

| # | Aturan | Konsekuensi teknis |
| ---: | --- | --- |
| 1 | Access Profile adalah **bundel izin yang dapat dipakai ulang** | Profil tidak terikat pada satu Departemen × Posisi tertentu |
| 2 | Satu Departemen + Posisi boleh punya **lebih dari satu** profil | Relasi banyak-ke-banyak lewat `SecOrganizationAccessProfile` |
| 3 | Business Permission efektif = **UNION** seluruh profil pada seluruh penempatan organisasi yang sah | Sama persis dengan aturan gabungan `BE-SEC-001` |
| 4 | Override langsung **hanya ADDITIVE GRANT** | `SecOrganizationPermissionGrant` **tidak punya** kolom `IsAllowed`, `IsDenied`, maupun sejenisnya |
| 5 | **Tidak ada** subtractive DENY override | Ditegakkan test, bukan sekadar konvensi |
| 6 | **Tidak ada** DENY precedence | Resolusi tetap `OR` murni |
| 7 | Organisasi yang butuh lebih sedikit izin memakai **profil lebih kecil / komposisi lain / melepas profil** | Layar admin wajib menawarkan jalan ini, bukan tombol "cabut izin ini" |

### H.2 Mengapa aturan 7 harus terlihat di layar

Larangan DENY hanya berhasil bila admin punya jalan keluar yang jelas. Bila seorang admin melihat
profil `DOCTOR_OUTPATIENT_BASE` memberi satu izin yang tidak ia inginkan, dan satu-satunya tombol
yang tersedia adalah "lepas seluruh profil", ia akan meminta fitur DENY dalam waktu satu minggu.

Karena itu layar Manajemen Hak Akses **wajib**, ketika admin mencoba mengurangi satu izin dari
profil, menampilkan tiga pilihan yang sah:

1. profil lain yang lebih kecil dan tetap memenuhi kebutuhan;
2. komposisi beberapa profil kecil sebagai pengganti satu profil besar;
3. melepas profil ini dan memberikan sisanya lewat `SecOrganizationPermissionGrant`.

Ini kebutuhan UX yang lahir langsung dari `DECISION 4`, dan menjadi acceptance criteria
`FE-SEC-002`.

### H.3 Komposisi profil pilot setelah pemecahan

**`DOCTOR_OUTPATIENT_BASE`** — fungsi normal dokter rawat jalan, sesuai `DECISION 5`.

| Business Permission | Kelas | Alasan masuk |
| --- | --- | --- |
| `…outpatient.view` | `A` | Membuka halaman |
| `…outpatient.queue.call` | `D`→`A` setelah audio diputuskan | Memanggil pasien |
| `…outpatient.queue.flow` | `B`→`A` setelah split | Lewati, tidak hadir, kembalikan |
| `…outpatient.consultation.start` | `B`→`A` setelah split | Memulai konsultasi |
| `…outpatient.consultation.write` | `B`→`A` setelah split | Menulis SOAP |
| `…outpatient.consultation.complete` | `B`→`A` setelah split | Menyelesaikan konsultasi — `DECISION 6` |
| `…outpatient.screening.read` | `A` | Melihat hasil skrining perawat |
| `…outpatient.screening.write` | `B`→`A` setelah split | Mengisi ulang skrining, **tanpa** `Verify` |
| `…outpatient.soap.read` | `A` | Membaca SOAP |
| `…outpatient.diagnosis.read` | `A` | Mencari dan melihat diagnosis |
| `…outpatient.diagnosis.write` | `B`→`A` setelah split | Menetapkan diagnosis, **tanpa** `Resolve` |
| `…outpatient.cppt.read` | `A` | Membaca CPPT |
| `…outpatient.cppt.write` | `A` | Menulis CPPT dari SOAP |
| `…outpatient.prescription.read` | `A` | Melihat resep, **tanpa** pemetaan master data opsional |
| `…outpatient.prescription.write` | `A` | Menulis resep |
| `…outpatient.prescription.template` | `A` | Template resep |
| `…outpatient.procedure.read` | `A` | Melihat tindakan |
| `…outpatient.procedure.select` | `B`→`A` setelah split | Memilih tindakan dan menghapusnya dari draft |

**Yang TIDAK masuk `DOCTOR_OUTPATIENT_BASE`:**

| Business Permission | Alasan | Default |
| --- | --- | --- |
| `…outpatient.procedure.execute` | `DECISION 5` — profil terpisah untuk dokter yang berwenang melakukan tindakan | Tidak diberikan |
| `…outpatient.procedure.approve` | `DECISION 5` — sensitif terpisah | **FAIL CLOSED** |
| `…outpatient.procedure.cancel` | Berkonsekuensi finansial | Tidak diberikan |
| `…outpatient.consultation.cancel` | Membatalkan konsultasi berjalan | Tidak diberikan |
| `…outpatient.certificate.write` | `BROKEN_DEPENDENCY` | Tidak dapat dipetakan |
| Pemetaan opsional master data | Fitur tetap jalan tanpanya | Tidak diberikan |

**`DOCTOR_OUTPATIENT_PROCEDURE_EXECUTOR`** — profil tambahan, bukan pengganti.

| Isi | `…outpatient.procedure.execute` |
| --- | --- |
| Cara pakai | Dipasang **bersama** `DOCTOR_OUTPATIENT_BASE` pada Departemen × Posisi yang memang melakukan tindakan |
| Larangan | Tidak diberikan otomatis berdasarkan `UserType` atau role dokter |
| Lapisan tambahan | Clinical privilege per jenis tindakan menyusul sebagai lapisan terpisah |

**`DOCTOR_OUTPATIENT_OBSERVER`** — hanya izin berakhiran `.view` dan `.read`.

**Profil approver tindakan** — **belum dibuat.** Departemen × Posisi yang berwenang menyetujui
tindakan adalah keputusan owner yang belum diambil, dan membuat profilnya sekarang berarti
menyiapkan wadah untuk keputusan yang belum ada.

### H.4 Aturan tambahan pada `SecOrganizationPermissionGrant`

| Aturan | Alasan |
| --- | --- |
| Hanya memberi, tidak pernah mencabut | `DECISION 4` |
| Alasan wajib diisi | Override tanpa alasan tertulis adalah sumber utama privilege creep |
| Masa berlaku wajib diisi | Hak sementara harus berakhir sendiri |
| Override atas izin sensitif memerlukan persetujuan terpisah | Selaras dengan perlakuan kemampuan sensitif `BE-SEC-001` |
| Ada laporan tetap "seluruh override yang berlaku" | Syarat audit |

---

## I. Strategi Migrasi dan Kompatibilitas Legacy

### I.1 Prinsip

Tidak ada tabel yang dihapus. `SysApplicationModule`, `SysControllerAccess`, `SysActionAccess`, dan
`SysAccessPolicy` tetap hidup dan tetap menjadi sumber izin yang sah, sesuai `DECISION 1`.

### I.2 Dua jenis migrasi yang harus dibedakan

Rencana ini memuat **dua** perubahan data yang sifatnya sangat berbeda. Menyamakan keduanya adalah
kesalahan yang paling mahal, karena yang pertama menyentuh hak yang sudah berjalan.

| | **Migrasi 1 — pemecahan identitas** (`BE-SEC-003`) | **Migrasi 2 — sumber izin kedua** (`BE-SEC-007`) |
| --- | --- | --- |
| Yang berubah | Isi `SysAccessPolicy` **diperluas** | Tidak ada data lama yang disentuh |
| Risiko utama | **Silent privilege loss** bila expansion gagal | **Silent privilege broadening** bila pemetaan salah |
| Sifat | Menyentuh hak yang sedang berjalan | Aditif murni |
| Rollback | Perlu snapshot dan rencana balik | Cukup matikan sakelar |

### I.3 Migrasi 1 — pemecahan identitas, langkah demi langkah

**Tujuan.** Setiap Departemen × Posisi memiliki kemampuan yang **sama persis** sebelum dan sesudah
pemecahan.

**Prasyarat masuk:**

1. Laporan read-only per identitas lama sudah dijalankan pada lingkungan yang bersangkutan
   (bagian C.2) dan ditinjau manusia.
2. Snapshot `SysAccessPolicy` diambil.

**Langkah:**

1. **Seeder mendaftarkan identitas baru.** 28 baris `SysActionAccess` baru muncul di registry.
   Belum ada satu pun policy yang menunjuknya, sehingga **belum ada perubahan hak**.
2. **Identitas lama ditutup**, mengikuti pola `CloseRowsAbsentFromSourceAsync` yang sudah ada:
   ditandai tidak aktif, **tanpa hard delete**.
3. **Expansion.** Setiap baris `SysAccessPolicy` aktif yang menunjuk identitas lama diganti menjadi
   N baris yang menunjuk identitas turunannya, dengan `DepartmentId` dan `PositionId` yang sama
   persis. Contoh: satu baris `PatientProcedure.Update` untuk Poliklinik Umum × Dokter Umum menjadi
   lima baris: `Edit`, `RemoveDraft`, `Approve`, `Execute`, `Cancel`.
4. **Verifikasi kesetaraan.** Untuk setiap Departemen × Posisi, himpunan **endpoint** yang dapat
   dijangkau sebelum dan sesudah dibandingkan. Selisih apa pun — positif maupun negatif — adalah
   kegagalan yang menghentikan migrasi.

**Contoh berangka.** Misalkan laporan read-only menemukan 3 baris `SysAccessPolicy` menunjuk
`PatientProcedure.Update`, milik 3 pasangan Departemen × Posisi. Sesudah expansion harus ada
**15 baris** (3 × 5), dan jumlah endpoint yang dapat dijangkau setiap pasangan tetap 5. Bila
hasilnya 12 baris, ada pasangan yang kehilangan hak. Bila 18, ada yang mendapat hak baru.

**Yang secara tegas TIDAK dilakukan:**

| Larangan | Alasan |
| --- | --- |
| Memberi identitas yang sebelumnya tidak dijaga identitas lama | Arahan Legacy Split Safety |
| Menebak "dokter ini pasti tidak perlu approve" lalu tidak memberinya | Penyempitan adalah keputusan owner, bukan efek migrasi |
| Menggabungkan penyempitan hak ke dalam migrasi ini | Membuat kegagalan tidak dapat dibedakan dari kesengajaan |

**Penyempitan hak sesudahnya.** Setelah pemecahan selesai dan terbukti setara, owner dapat
memutuskan mencabut `PatientProcedure.Approve` dari Departemen × Posisi tertentu. Itu operasi
terpisah, tercatat, dan dapat dikembalikan. Inilah satu-satunya cara sah untuk sampai pada
"dokter rawat jalan tidak lagi bisa approve".

### I.4 Migrasi 2 — sumber izin kedua

```
HasAccessAsync(user, resource, action)
  1..3  tidak berubah
  4a.   SUMBER LAMA   SysAccessPolicy                        ← tetap
  4b.   SUMBER BARU   SecAccessProfile → SecBusinessPermission
                       → SecBusinessPermissionMapping → (resource, action)
  hasil = 4a ATAU 4b
```

| Jaminan | Cara |
| --- | --- |
| Dapat dimatikan | Satu sakelar konfigurasi. Dimatikan → hasil identik baseline `BE-SEC-001` |
| Tidak ada privilege loss | Aditif murni; sumber lama tidak disentuh |
| Tidak ada privilege broadening diam-diam | Mode bayangan `BE-SEC-006` melaporkan calon pelebaran sebelum diaktifkan |
| Tanpa DENY | Resolusi `OR`; ditegakkan test |
| Pencabutan seketika | Cache per-request saja, tanpa TTL (bagian A.1) |

### I.5 Dampak pada test yang sudah terkunci

| Test | Dampak | Task |
| --- | --- | --- |
| `PermissionRegistryInvariantTests` | Jumlah kunci registry bertambah | `BE-SEC-003` |
| `CanonicalSecurityContractTests` | Bila dua endpoint audio diberi identitas, isi himpunan fallback warisan berubah, jumlahnya tetap **69** — lihat koreksi pada `evidence/03` bagian 16. `CompatibilityFallbackMatchesApprovedLegacySetExactly` wajib diperbarui secara sadar | `BE-SEC-003` |
| `StaleRegistryAuthorizationTests` | Identitas lama yang ditutup harus berhenti mengotorisasi — perilaku ini justru **diuji ulang** oleh pemecahan | `BE-SEC-003` |
| `ReconcileNeverCreatesAccessPolicy` | **Harus tetap hijau.** Expansion dilakukan skrip migrasi yang diberi wewenang, **bukan** seeder | `BE-SEC-003` |
| Tiga test SuperAdmin | Harus tetap hijau | Semua task |

---

## J. Keputusan Audio Antrean

### J.1 Bukti baru yang mengubah gambaran

`DECISION 7` meminta pemisahan actor: aksi dokter di satu sisi, aksi display/runtime di sisi lain.
Penelusuran frontend pada sesi ini menemukan bahwa **tiga kelas actor memanggil dua endpoint audio
yang sama**:

| # | Actor | Berkas frontend | Jenis akun | Memenuhi `QueueDisplayRuntimeRead`? |
| ---: | --- | --- | --- | --- |
| 1 | **Dokter** | `src/lib/hooks/health-services/registration-management/doctor-queue/useDoctorCallWithVoice.js` | Pengguna biasa | **Tidak** |
| 2 | **Perawat** | `src/lib/hooks/health-services/registration-management/nurse-station-management-queue/useQueueCallWithVoice.js` | Pengguna biasa | **Tidak** |
| 3 | **Perangkat display antrean** | `src/lib/services/queue-display/queue-display-screen-service.jsx` | Akun perangkat | **Ya** |

Ketiganya bermuara pada fungsi yang sama, `fetchQueueVoiceAudioBlob`
(`src/lib/services/health-services/registration-management/queue-voice.service.js:679`), yang
mengambil berkas audio memakai **sesi pemanggilnya sendiri** lewat `InstanceAxios`.

Definisi policy `QueueDisplayRuntimeRead` (`Program.cs:571`) mensyaratkan salah satu dari:
role `SuperAdmin`, `Administrator`, `QueueDisplayDevice`, `QueueDisplay`, `DisplayQueue`; atau
claim perangkat display seperti `is_queue_display_account`, `profile_type = QueueDisplayDevice`,
`queue_display_device_id`.

> **Akibatnya, bila dua endpoint audio diberi `QueueDisplayRuntimeRead` apa adanya: browser dokter
> dan perawat akan menerima `403`, dan fitur "Panggil Pasien dengan suara" berhenti bekerja untuk
> keduanya.** Panggilannya sendiri tetap berhasil — hanya suaranya yang hilang.

Ini persis keadaan yang `DECISION 7` antisipasi: *"Jika current device flow benar-benar tidak dapat
memakai authenticated runtime policy, laporkan sebagai owner decision dengan bukti."* Keadaannya
justru kebalikannya — perangkat display **bisa**, tetapi dokter dan perawat **tidak**.

### J.2 Pemisahan actor sesuai `DECISION 7`

| Actor | Kemampuan | Identitas yang diusulkan |
| --- | --- | --- |
| Dokter | Memanggil pasien | `DoctorQueue.Call` — Business Permission `…outpatient.queue.call` |
| Perawat | Memanggil pasien | Identitas antrean perawat — di luar cakupan pilot |
| Siapa pun yang memanggil, plus perangkat display | **Memutar/mengunduh berkas audio** | **`QueueVoice.PlayAudio`** — kemampuan runtime, **bukan** Business Permission dokter |

### J.3 Rekomendasi tunggal

> **Lindungi dua endpoint audio dengan otorisasi gabungan: `QueueDisplayRuntimeRead` **ATAU**
> technical permission baru `QueueVoice.PlayAudio`.**

Bentuk konseptualnya — perangkat display lolos lewat policy runtime, petugas lolos lewat izin
teknis biasa:

```
GET /queue-voice/audio/{dateKey}/{fileName}
GET /queue-voice/download/{dateKey}/{fileName}
    → [Authorize]                                  (tetap)
    → lolos bila:  policy QueueDisplayRuntimeRead
                   ATAU  AccessPermission(QueueVoice, PlayAudio)
```

| Kriteria | Penilaian |
| --- | --- |
| Menghindari `AllowAnonymous` | **Ya** — sesuai `DECISION 7` |
| Actor terpisah | **Ya** — perangkat lewat policy runtime, petugas lewat izin teknis |
| Alur dokter tetap jalan | **Ya** — `QueueVoice.PlayAudio` masuk `DOCTOR_OUTPATIENT_BASE` |
| Alur perawat tetap jalan | **Ya** — masuk profil perawat kelak |
| Alur perangkat display tetap jalan | **Ya** — tidak disentuh |
| Dampak kompatibilitas | Isi himpunan fallback warisan `CanonicalSecurityContractTests` berubah; jumlahnya tetap **69** — lihat koreksi pada `evidence/03` bagian 16. Wajib diperbarui secara sadar |

**Mengapa bukan `QueueVoice.Read` yang sudah ada.** Identitas itu melindungi `GET /queue-voice/profiles`
— daftar profil suara, kemampuan administratif. Memakainya kembali untuk pemutaran audio berarti
memberi setiap dokter akses ke pengaturan suara antrean. Identitas baru lebih murah daripada
kekaburan itu.

**Mengapa bukan sekadar `[Authorize]` seperti sekarang.** Karena berkas audio memuat nama pasien
yang diumumkan. Setiap akun yang login — termasuk akun kiosk dan akun non-klinis — hari ini dapat
mengunduhnya bila mengetahui `dateKey` dan `fileName`. `DECISION 7` sudah menolak jalur permisif.

Bagian ini **tetap memerlukan konfirmasi owner** karena menyangkut siapa yang boleh mendengar
rekaman nama pasien. Lihat bagian N butir 1.

---

## K. Perlakuan Endpoint Rusak

### K.1 Ketetapan

Sesuai arahan **Broken Doctor Certificate**:

| Ketetapan | Isi |
| --- | --- |
| Klasifikasi | BP-19 `health.doctor.outpatient.certificate.write` = **`C. BROKEN_DEPENDENCY`** |
| Larangan mutlak | **Tidak boleh** membuat permission mapping ke endpoint yang tidak ada |
| Larangan kedua | **Tidak boleh** memperbaiki `doctor-certificates` → `medical-certificates` diam-diam di dalam task BE-SEC mana pun |
| Perbaikan | Task terpisah di repository frontend, dengan keputusan kontrak tersendiri |

### K.2 Bagaimana BP-19 diperlakukan di katalog

BP-19 **boleh** didaftarkan di `SecBusinessPermission` dengan status `BLOCKED` dan **nol** baris
pada `SecBusinessPermissionMapping`.

Konsekuensinya menyenangkan: karena resolusi bekerja dengan menerjemahkan Business Permission
menjadi technical permission, sebuah Business Permission tanpa pemetaan **tidak pernah memberi hak
apa pun**. Ia fail closed secara konstruksi, bukan karena ada aturan khusus yang menjaganya.

Manfaat mendaftarkannya: layar admin dapat menampilkannya sebagai baris nonaktif berketerangan
"belum tersedia", yang jauh lebih jujur daripada menyembunyikan fitur yang dokter lihat setiap hari
di layarnya.

### K.3 Prasyarat sebelum BP-19 boleh diaktifkan

Ketiganya harus selesai, berurutan:

1. **Perbaikan kontrak frontend** — task terpisah menentukan apakah frontend menunjuk
   `medical-certificates`, atau backend menyediakan `doctor-certificates`. Ini keputusan kontrak,
   bukan penggantian teks.
2. **Pemecahan `MedicalCertificate.Update`** — identitas itu memuat **7 endpoint** termasuk `issue`,
   `verify`, `approve`, `reject`, dan `revoke`. Memetakan BP-19 ke sana sebelum dipecah akan
   memberi setiap dokter kemampuan menyetujui dan mencabut surat medis siapa pun.
3. **Keputusan owner** tentang siapa yang berwenang menandatangani surat dokter — pertanyaan
   `Y.3` butir 3 yang ditunda.

### K.4 Temuan sampingan

`src/lib/hooks/health-services/registration-management/use-queue-voice-player.js` tidak diimpor
berkas mana pun — kode mati. Dicatat, **tidak diperbaiki**, di luar cakupan.

---

## L. Epic Decomposition Formal

### L.1 Klasifikasi induk

Menurut `rules/backend/TASK_CLASSIFICATION.md`, aturan eksekusi menyatakan: *"Setiap perancangan
ulang yang menyentuh seluruh arsitektur, implementasi lintas domain, atau scope yang tidak dapat
direview dan divalidasi dengan aman sebagai satu perubahan terbatas adalah EPIC, berapa pun
skornya."*

> **`BE-SEC-002` sebagai keseluruhan adalah `EPIC`.**
> `STOP → DECOMPOSE → klasifikasikan ulang setiap fase.` Dekomposisi di bawah adalah pelaksanaan
> aturan itu.

`BE-SEC-002` sendiri **tidak** menjadi task implementasi. Ia tetap berupa gerbang audit dan
arsitektur yang sudah selesai, dengan dua dokumen evidence sebagai keluarannya.

### L.2 Pemetaan urutan konseptual pemilik sistem ke Task ID formal

| Urutan konseptual owner | Task ID formal | Catatan pemecahan |
| --- | --- | --- |
| 1. Technical Permission Granularity Hardening — pilot Doctor Outpatient | `BE-SEC-003` | Utuh, satu task |
| 2. Business Permission Catalog + technical mapping | `BE-SEC-004` | Dipecah: skema katalog dan isinya punya batas rollback berbeda |
| | `BE-SEC-005` | Isi katalog (data), terpisah dari skema |
| 3. Access Profile + organization assignment | `BE-SEC-006` | Skema profil, migration tersendiri |
| 4. Dual-source effective permission resolver | `BE-SEC-007` | Mode bayangan, belum memutuskan izin |
| | `BE-SEC-008` | Aktivasi bersakelar — inilah satu-satunya momen perilaku otorisasi berubah |
| *(tersirat pada 8)* | `BE-SEC-009` | API admin Business Access |
| 5. `/api/access/me` | `BE-SEC-010` | Utuh, satu task |
| 6. Self Service automatic baseline | `BE-SEC-011` | **Belum didekomposisi** — lihat L.4 |
| 7. Frontend authorization | `FE-SEC-001` | State izin + perbaikan guard |
| | `FE-SEC-003` | Guard tab dan tombol pilot |
| 8. Role Management business-oriented UX | `FE-SEC-002` | Layar admin |

Tiga pemecahan tambahan dilakukan karena batas rollback-nya berbeda, bukan karena scope-nya besar:
skema versus data (`004`/`005`), bayangan versus aktivasi (`007`/`008`), dan state izin versus
guard layar (`FE-SEC-001`/`FE-SEC-003`).

### L.3 Task backend

---

#### `BE-SEC-003` — Technical Permission Granularity Hardening (pilot Dokter Rawat Jalan)

| Field | Isi |
| --- | --- |
| **Judul** | Pemecahan identitas technical permission untuk pilot Dokter Rawat Jalan |
| **Klasifikasi** | `HEAVY` — skor 11 (repo 0, diperiksa 2, diubah 2, logika 2, kontrak 1, database 2, keamanan 2, UI 0) |
| **Scope** | Memecah 7 identitas kasar menjadi 28 identitas baru pada 6 controller, ditambah keputusan identitas audio antrean; menutup identitas lama tanpa hard delete; memperluas `SysAccessPolicy` ke *exact historical capability set*; memperbarui test yang terkunci |
| **Di luar scope** | Business Permission, Access Profile, resolver, penyempitan hak siapa pun, identitas di luar daftar C.3 |
| **Dependency** | Laporan read-only per identitas lama sudah dijalankan dan ditinjau (C.2); wewenang database untuk lingkungan sasaran; keputusan audio antrean **atau** pengeluaran audio dari scope |
| **Perubahan source** | `PatientProcedureController.cs`, `DoctorConsultationController.cs`, `DoctorQueueController.cs`, `PatientVitalSignController.cs`, `PatientAssessmentController.cs`, `PatientDiagnosisController.cs`, `QueueVoiceController.cs`; `Program.cs` bila policy gabungan audio dipakai; `PermissionRegistryInvariantTests.cs`, `CanonicalSecurityContractTests.cs`, `StaleRegistryAuthorizationTests.cs`; skrip migrasi data yang diberi wewenang |
| **Database/migration** | **Tidak ada perubahan skema.** Ada **migrasi data**: `SysActionAccess` bertambah 28 baris lewat seeder; identitas lama ditutup; `SysAccessPolicy` diperluas. Snapshot wajib sebelum penulisan |
| **Risiko keamanan** | **Tertinggi di seluruh rangkaian.** Expansion yang gagal = *silent privilege loss*; expansion yang melebar = *privilege broadening*. Mitigasi: laporan sebelum tulis, verifikasi kesetaraan per Departemen × Posisi, snapshot, kemampuan balik |
| **Acceptance criteria** | 1. 28 identitas baru terdaftar dan terbaca layar Akses Role. 2. Tujuh identitas lama tertutup, **tanpa** hard delete. 3. Untuk setiap Departemen × Posisi, himpunan **endpoint** yang dapat dijangkau **identik** sebelum dan sesudah — selisih nol di kedua arah. 4. `ReconcileNeverCreatesAccessPolicy` tetap hijau (expansion oleh skrip berwenang, bukan seeder). 5. `CompatibilityFallbackMatchesApprovedLegacySetExactly` diperbarui secara sadar bila audio diberi identitas. 6. Tiga test SuperAdmin tetap hijau. 7. `dotnet build` dan seluruh test suite lulus. 8. `dotnet ef migrations has-pending-model-changes` bersih. 9. Smoke test akun non-SuperAdmin: kemampuan sebelum dan sesudah sama |
| **Rollback boundary** | Mandiri. Balikkan source ke identitas lama, jalankan skrip balik yang menciutkan N baris `SysAccessPolicy` menjadi 1, pulihkan dari snapshot bila perlu. Tidak ada task lain yang bergantung padanya saat itu |

---

#### `BE-SEC-004` — Registry prefix `Sec` dan skema katalog Business Permission

| Field | Isi |
| --- | --- |
| **Judul** | Pendaftaran prefix `Sec` dan pembuatan skema katalog Business Permission |
| **Klasifikasi** | Skor 7 = `MEDIUM`; **dinaikkan ke `HEAVY` secara konservatif** karena ini entity persisted pertama di bawah prefix baru dan membawa migration |
| **Scope** | Menambahkan baris registry `Sec`; membuat 3 model — `SecBusinessFeature`, `SecBusinessPermission`, `SecBusinessPermissionMapping`; konfigurasi EF; `DbSet`; satu migration aditif; test invarian skema |
| **Di luar scope** | Isi katalog, Access Profile, resolver, API, perubahan jalur otorisasi apa pun |
| **Dependency** | `BE-SEC-003` selesai — pemetaan harus menunjuk identitas final, bukan identitas yang akan berubah |
| **Perubahan source** | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `Areas/Administrator/PlatformAuthorization/Models/` (3 berkas baru); `Repositories/Configurations/Administrator/PlatformAuthorization/` (3 baru); `Repositories/ApplicationDbContext.cs`; `Migrations/` (1 baru); test baru |
| **Database/migration** | Satu migration **aditif**: 3 tabel baru. Tidak ada tabel lama disentuh |
| **Risiko keamanan** | **Rendah.** Tabel kosong tanpa pembaca di jalur otorisasi. Tidak ada perubahan perilaku |
| **Acceptance criteria** | 1. Baris registry ada sebelum berkas model pertama dibuat (`QBE-MOD-003`). 2. Tiga tabel terbentuk. 3. `has-pending-model-changes` bersih. 4. Test invarian: kode Business Permission unik, pemetaan menunjuk `(resource, action)` yang benar-benar ada di `PermissionRegistryDescriptor`. 5. `HasAccessAsync` **tidak** disentuh — dibuktikan dengan diff. 6. Build dan test lulus |
| **Rollback boundary** | Mandiri dan bersih. Migration `Down` menghapus 3 tabel kosong; baris registry dicabut |

---

#### `BE-SEC-005` — Isi katalog Business Permission pilot

| Field | Isi |
| --- | --- |
| **Judul** | Seeder katalog Business Feature, Business Permission, dan pemetaan teknis untuk pilot Dokter Rawat Jalan |
| **Klasifikasi** | Skor 6 = `MEDIUM` |
| **Scope** | Seeder yang mendaftarkan pohon Business Feature (Health Services → Dokter → Rawat Jalan → 11 feature), katalog Business Permission pilot hasil pemecahan, dan pemetaannya ke identitas final `BE-SEC-003` |
| **Di luar scope** | Access Profile, penetapan organisasi, resolver. **Seeder tidak pernah memberi hak kepada siapa pun** |
| **Dependency** | `BE-SEC-004` |
| **Perubahan source** | `Seeders/BusinessPermissionCatalogSeeder.cs` (baru); `Program.cs` (registrasi); test katalog |
| **Database/migration** | Tidak ada perubahan skema. Hanya penyisipan data katalog, idempoten |
| **Risiko keamanan** | **Rendah.** Katalog tanpa pembaca di jalur otorisasi. Risiko nyatanya adalah pemetaan yang salah, yang baru berakibat pada `BE-SEC-008` |
| **Acceptance criteria** | 1. Seluruh Business Permission pilot terdaftar. 2. Setiap pemetaan menunjuk identitas yang ada; nol pemetaan yatim. 3. BP-19 terdaftar berstatus `BLOCKED` dengan **nol** pemetaan. 4. Pemetaan opsional master data ditandai `OPSIONAL`. 5. Test cakupan: seluruh identitas teknis yang dipakai pilot tercakup katalog. 6. Test: seeder **tidak** menyentuh `SysAccessPolicy`, `SecAccessProfile`, maupun `SecOrganizationAccessProfile`. 7. Menjalankan seeder dua kali tidak menghasilkan duplikat |
| **Rollback boundary** | Mandiri. Hapus baris katalog; tabelnya tetap ada. Tidak ada hak yang terpengaruh karena belum ada yang membacanya |

---

#### `BE-SEC-006` — Skema Access Profile dan penetapan organisasi

| Field | Isi |
| --- | --- |
| **Judul** | Skema Access Profile, isi profil, penetapan organisasi, dan grant aditif |
| **Klasifikasi** | Skor 7 = `MEDIUM`; **dinaikkan ke `HEAVY` konservatif** karena membawa migration dan menjadi wadah pemberian hak |
| **Scope** | 4 model — `SecAccessProfile`, `SecAccessProfilePermission`, `SecOrganizationAccessProfile`, `SecOrganizationPermissionGrant`; konfigurasi EF; migration aditif; seeder **definisi** dua profil pilot; test invarian |
| **Di luar scope** | Penetapan profil ke Departemen × Posisi mana pun; resolver; API |
| **Dependency** | `BE-SEC-005` |
| **Perubahan source** | 4 model baru; 4 konfigurasi EF; `ApplicationDbContext.cs`; satu migration; `AccessProfileCatalogSeeder.cs`; test |
| **Database/migration** | Satu migration **aditif**: 4 tabel baru |
| **Risiko keamanan** | **Rendah**, dengan satu invarian yang harus dijaga ketat: seeder boleh mendefinisikan profil, tetapi **tidak boleh** menetapkannya ke organisasi mana pun |
| **Acceptance criteria** | 1. Empat tabel terbentuk. 2. `SecOrganizationPermissionGrant` **tidak punya** kolom DENY dalam bentuk apa pun — dibuktikan test. 3. Alasan dan masa berlaku wajib pada grant. 4. Definisi `DOCTOR_OUTPATIENT_BASE` dan `DOCTOR_OUTPATIENT_OBSERVER` terdaftar. 5. `procedure.approve` **tidak** ada di dalam `DOCTOR_OUTPATIENT_BASE` — dibuktikan test bernama eksplisit. 6. Test: `SecOrganizationAccessProfile` kosong setelah seeding. 7. `has-pending-model-changes` bersih |
| **Rollback boundary** | Mandiri. Migration `Down` menghapus 4 tabel. Belum ada yang membacanya |

---

#### `BE-SEC-007` — Resolver Business Permission mode bayangan

| Field | Isi |
| --- | --- |
| **Judul** | Service resolusi Business Permission ke technical permission, mode laporan saja |
| **Klasifikasi** | Skor 6 = `MEDIUM` |
| **Scope** | Service yang menghitung `TechnicalPermissions(EffectiveBusinessPermissions(user))` beserta laporan pembandingnya terhadap hak yang berlaku. **Tidak dipanggil** `HasAccessAsync` |
| **Di luar scope** | Perubahan apa pun pada keputusan izin |
| **Dependency** | `BE-SEC-006` |
| **Perubahan source** | `Services/Security/BusinessPermissionResolutionService.cs` (baru); `Program.cs` (registrasi); test resolusi |
| **Database/migration** | Tidak ada. Query baca saja |
| **Risiko keamanan** | **Sangat rendah** — tidak ada jalur yang memakainya untuk memutuskan izin. Dibuktikan dengan diff pada `AccessPermissionService.cs` yang harus kosong |
| **Acceptance criteria** | 1. Untuk satu user, service mengembalikan himpunan `(resource, action)` yang benar. 2. Aturan kelayakan penempatan `BE-SEC-001` berlaku sama: `IsCancel`, `IsActive`, `IsDelete`, effective date. 3. `IsPrimary` dan `AssignmentType` **bukan** filter — dibuktikan test. 4. Gabungan lintas penempatan benar. 5. Mode laporan menghasilkan dua daftar: calon hak hilang dan calon hak melebar. 6. `AccessPermissionService.cs` **tidak berubah** |
| **Rollback boundary** | Mandiri dan sepele. Hapus service dan registrasinya |

---

#### `BE-SEC-008` — Aktivasi sumber izin kedua

| Field | Isi |
| --- | --- |
| **Judul** | Menyalakan sumber izin kedua pada `HasAccessAsync`, aditif dan bersakelar |
| **Klasifikasi** | Skor 7 = `MEDIUM`; **dinaikkan ke `HEAVY`** karena inilah satu-satunya momen perilaku otorisasi benar-benar berubah |
| **Scope** | Menambahkan langkah `4b` pada `HasAccessAsync`; sakelar konfigurasi; cache per-request |
| **Di luar scope** | Menonaktifkan `SysAccessPolicy`; memindahkan Departemen × Posisi mana pun; menghapus tabel warisan |
| **Dependency** | `BE-SEC-007`; laporan bayangan sudah ditinjau manusia dan calon pelebarannya disetujui baris demi baris |
| **Perubahan source** | `Services/Security/AccessPermissionService.cs`; `appsettings*.json` (nama kunci saja); test gabungan |
| **Database/migration** | Tidak ada |
| **Risiko keamanan** | **Tinggi.** Pelebaran hak yang tidak disengaja. Mitigasi: aditif murni, sakelar mati secara default, laporan bayangan wajib ditinjau lebih dulu |
| **Acceptance criteria** | 1. Sakelar **mati** → hasil `HasAccessAsync` **identik** baseline `BE-SEC-001`, dibuktikan test. 2. Sakelar hidup → hasil = gabungan kedua sumber. 3. Tidak ada jalur yang dapat **menolak** apa yang sumber lain berikan — test anti-DENY. 4. Penempatan tidak sah tidak memberi hak lewat jalur baru. 5. Kemampuan sensitif yang belum ditetapkan tetap `403`. 6. Pencabutan hak berlaku pada request berikutnya — tidak ada cache lintas request. 7. Smoke test akun non-SuperAdmin dengan sakelar mati dan hidup |
| **Rollback boundary** | **Paling bersih di seluruh rangkaian**: matikan sakelar. Tidak ada data yang perlu dipulihkan |

---

#### `BE-SEC-009` — API admin Business Access

| Field | Isi |
| --- | --- |
| **Judul** | Endpoint pengelolaan Business Permission, Access Profile, dan penetapan organisasi |
| **Klasifikasi** | Skor 9 = `HEAVY` |
| **Scope** | Controller dan DTO baru di `Areas/Administrator/PlatformAuthorization/`; pembacaan pohon fitur; penetapan dan pelepasan profil; grant aditif beserta alasan dan masa berlaku; laporan asal hak |
| **Di luar scope** | Layar frontend; `RoleAccessController.cs` yang **tidak boleh disentuh** |
| **Dependency** | `BE-SEC-008` |
| **Perubahan source** | `BusinessAccessController.cs` dan DTO (baru); test kontrak |
| **Database/migration** | Tidak ada skema baru. Endpoint ini yang pertama kali **menulis** `SecOrganizationAccessProfile` |
| **Risiko keamanan** | **Tinggi** — inilah pintu pemberian hak. Endpoint-nya sendiri wajib `[AccessPermission]`, dan pemberian izin sensitif memerlukan pemeriksaan terpisah |
| **Acceptance criteria** | 1. Seluruh endpoint memakai `[AccessPermission]` dengan identitas baru milik Platform Authorization. 2. Penetapan profil tercatat beserta pelakunya. 3. Grant tanpa alasan atau tanpa masa berlaku **ditolak**. 4. Pemberian Business Permission sensitif menempuh jalur persetujuan terpisah. 5. `RoleAccessController.cs` tidak berubah — dibuktikan diff. 6. Laporan asal hak menampilkan seluruh sumber, bukan yang pertama ditemukan |
| **Rollback boundary** | Mandiri. Cabut controller; data yang sudah dibuat tetap valid dan dapat dikelola lewat `BE-SEC-008` yang dimatikan |

---

#### `BE-SEC-010` — `GET /api/v1/access/me`

| Field | Isi |
| --- | --- |
| **Judul** | Kontrak izin untuk frontend |
| **Klasifikasi** | Skor 7 = `MEDIUM` |
| **Scope** | Satu endpoint yang mengembalikan kode Business Permission efektif dan pohon fitur yang dapat dijangkau, plus penanda versi |
| **Di luar scope** | Frontend; penegakan izin apa pun berdasarkan response ini |
| **Dependency** | `BE-SEC-008` |
| **Perubahan source** | Controller baru; DTO; test kontrak |
| **Database/migration** | Tidak ada |
| **Risiko keamanan** | **Sedang.** Response ini **tidak boleh** memuat `ControllerName`, `ActionName`, `SysControllerAccessId`, atau `SysActionAccessId` — dibuktikan test. Ia juga bukan pengaman: server tetap menegakkan izin di setiap endpoint |
| **Acceptance criteria** | 1. Hanya kode bisnis di response — test menegakkannya. 2. Gabungan sudah dihitung server. 3. Pohon fitur ikut dikirim. 4. Penanda versi berubah ketika hak berubah. 5. Envelope `ApiResponse<T>`. 6. User tanpa profil menerima daftar kosong, bukan error |
| **Rollback boundary** | Mandiri. Cabut endpoint; tidak ada konsumen sampai `FE-SEC-001` |

---

#### `BE-SEC-011` — Baseline Self Service otomatis

| Field | Isi |
| --- | --- |
| **Status** | **BELUM DIDEKOMPOSISI — sengaja** |
| **Alasan** | Definisi "pegawai aktif" adalah keputusan HR, bukan keputusan teknis. Task ini juga menuntut jenis penetapan baru yang tidak melalui Departemen × Posisi, sehingga model datanya belum lengkap |
| **Prasyarat masuk** | `BE-SEC-008` selesai; keputusan HR tentang definisi pegawai aktif; gerbang requirement tersendiri |
| **Catatan** | Delapan endpoint Self Service warisan hari ini berada di dalam himpunan 69 endpoint fallback yang dikunci `CanonicalSecurityContractTests`. Menyentuhnya adalah task tersendiri lagi |

### L.4 Task frontend

> **Catatan governance.** `blueprint-manifest.md` mencatat *Frontend authority: "Belum ditetapkan —
> frontend belum masuk cakupan"*. Prefix `FE-SEC` di bawah adalah **usulan** yang mengikuti pola
> `FE-BKC` dan `FE-IGD`, dan memerlukan penetapan pemilik sistem sebelum dipakai. Lihat bagian N
> butir 4.

---

#### `FE-SEC-001` — State izin frontend dan perbaikan guard

| Field | Isi |
| --- | --- |
| **Klasifikasi** | Skor 10 = `HEAVY` |
| **Scope** | Service dan slice untuk `/api/access/me`; hook `useBusinessPermission`; perbaikan `filterMenuItemsByRole` menjadi penyaringan berbasis kode; perbaikan `route-guard-link.js`; pemasangan `RouteGuard`; penanda kode fitur pada `menu-items.jsx` |
| **Di luar scope** | Layar Manajemen Hak Akses; guard tab dan tombol halaman dokter |
| **Dependency** | `BE-SEC-010` |
| **Perubahan source** | `src/lib/services/access/access-me.service.js`, `src/lib/state/slice/access/access-slice.jsx`, `src/lib/state/store.jsx`, `src/lib/hooks/access/use-business-permission.js`, `src/utils/auth/route-guard-link.js`, `src/components/features/auth/route-guard.jsx`, `src/utils/menu-sidebar/menu-items.jsx`, `src/utils/menu-sidebar/role/filter-menu-items-by-role.jsx`, `src/components/features/left-sidebar/left-sidebar-items-virtualized.jsx` |
| **Database/migration** | Tidak ada |
| **Risiko keamanan** | **Sedang.** Menyembunyikan menu **bukan** keamanan. Risiko sebenarnya adalah kebalikannya: guard yang terlalu ketat mengunci pengguna sah keluar dari halaman yang sebenarnya boleh ia buka |
| **Acceptance criteria** | 1. Menu disaring berdasarkan kode fitur, bukan nama peran. 2. `ROLE_ROUTE_PERMISSIONS` yang menunjuk rute mati dicabut. 3. `RouteGuard` benar-benar terpasang. 4. Ketika `/api/access/me` gagal, aplikasi **fail closed** pada navigasi, bukan menampilkan seluruh menu. 5. SuperAdmin tetap melihat seluruh menu. 6. Tidak ada berkas frontend yang membaca nama teknis backend |
| **Rollback boundary** | Mandiri di repository frontend. Kembalikan berkas; backend tidak terpengaruh |

---

#### `FE-SEC-002` — Layar Manajemen Hak Akses business-oriented

| Field | Isi |
| --- | --- |
| **Klasifikasi** | Skor 10 = `HEAVY` |
| **Scope** | Layar empat panel sesuai bagian T audit awal: konteks organisasi, Access Profile, pohon fitur bisnis, rincian dan asal hak. Termasuk tiga jalan keluar pengganti DENY (bagian H.2) |
| **Di luar scope** | Menghapus layar Role Access lama, yang tetap hidup sebagai tampilan teknis |
| **Dependency** | `BE-SEC-009` dan `FE-SEC-001` |
| **Perubahan source** | `administrator-role-access-view.jsx` (atau view baru berdampingan); style; `business-access.service.js` |
| **Database/migration** | Tidak ada |
| **Risiko keamanan** | **Sedang.** Layar yang membingungkan menyebabkan pemberian hak yang salah |
| **Acceptance criteria** | 1. Admin dapat memberi hak dokter rawat jalan **tanpa** melihat satu pun nama teknis di alur utama. 2. Nama teknis hanya di panel rincian yang tertutup default. 3. Asal setiap hak terlihat. 4. Izin sensitif bertanda dan berkonfirmasi terpisah. 5. Izin `BLOCKED` tampil nonaktif berketerangan. 6. **Tidak ada tombol yang menyerupai DENY.** 7. Ketika admin ingin mengurangi satu izin dari profil, layar menawarkan tiga jalan keluar sah. 8. Layar teknis lama tetap dapat dibuka |
| **Rollback boundary** | Mandiri. Bila view baru dibuat berdampingan, rollback cukup mencabut rutenya |

---

#### `FE-SEC-003` — Guard tab dan tombol Dokter Rawat Jalan

| Field | Isi |
| --- | --- |
| **Klasifikasi** | Skor 6 = `MEDIUM` |
| **Scope** | Menyembunyikan tab dan tombol pada halaman pilot sesuai Business Permission efektif |
| **Di luar scope** | Halaman lain |
| **Dependency** | `FE-SEC-001` |
| **Perubahan source** | `doctor-queue-view.jsx`, `ConsultationTabs.jsx`, `QueuePatientCard.jsx`, `FinalizeConsultationPanel.jsx` |
| **Database/migration** | Tidak ada |
| **Risiko keamanan** | **Rendah** — kosmetik. Server tetap menegakkan izin |
| **Acceptance criteria** | 1. Tab tanpa izin tidak muncul. 2. Tombol tanpa izin tidak muncul. 3. Dokter dengan profil dasar melihat seluruh tab yang seharusnya. 4. `AccessDeniedGate` tetap menangani `403` yang lolos. 5. Tab `Penunjang Medis` dan `CDSS` tetap seperti sekarang — tidak diberi izin |
| **Rollback boundary** | Mandiri dan sepele |

### L.5 Ringkasan dependency

```
BE-SEC-003  (hardening identitas)
   └── BE-SEC-004  (registry + skema katalog)
          └── BE-SEC-005  (isi katalog)
                 └── BE-SEC-006  (skema Access Profile)
                        └── BE-SEC-007  (resolver bayangan)
                               └── BE-SEC-008  (aktivasi bersakelar)   ← perilaku berubah di sini
                                      ├── BE-SEC-009  (API admin)
                                      │      └── FE-SEC-002  (layar admin)
                                      ├── BE-SEC-010  (/api/access/me)
                                      │      └── FE-SEC-001  (state izin + guard)
                                      │             ├── FE-SEC-002
                                      │             └── FE-SEC-003  (guard pilot)
                                      └── BE-SEC-011  (Self Service — belum didekomposisi)
```

Setiap task memenuhi empat syarat yang diminta: dapat direview sendiri, dapat dites sendiri, dapat
di-rollback sendiri, dan tidak menuntut migrasi big-bang. Satu-satunya task yang menyentuh data hak
yang sedang berjalan adalah `BE-SEC-003`; satu-satunya task yang mengubah perilaku otorisasi adalah
`BE-SEC-008`, dan rollback-nya adalah mematikan satu sakelar.

---

## M. Task Implementasi Pertama yang Direkomendasikan

> **`BE-SEC-003` — Technical Permission Granularity Hardening, pilot Dokter Rawat Jalan.**

### M.1 Mengapa ini yang pertama

| Alasan | Penjelasan |
| --- | --- |
| Mencegah pekerjaan terbuang | Katalog yang dibangun sebelum pemecahan harus ditulis ulang untuk **7 dari 19** Business Permission. Membangun katalog di atas identitas yang akan berubah adalah membangun di atas pasir |
| Sesuai urutan pemilik sistem | `EPIC DECOMPOSITION` menempatkannya di urutan pertama |
| Prosedurnya sudah terbukti | `BE-SEC-001` sudah menjalankan kelas perubahan yang sama: klasifikasi → laporan → tinjauan manusia → penulisan. Tidak ada prosedur baru yang perlu diciptakan |
| Bernilai walau rangkaian berhenti | Bahkan bila Business Permission tidak pernah dilanjutkan, pemecahan ini tetap membuat `approve` dan `execute` tindakan dapat diberikan terpisah lewat layar Role Access yang **sudah ada** |
| Mandiri | Tidak bergantung pada satu pun task lain |

### M.2 Prasyarat masuk yang belum terpenuhi

| # | Prasyarat | Sifat | Status |
| ---: | --- | --- | --- |
| 1 | Wewenang menjalankan laporan read-only `SysAccessPolicy` per identitas pada lingkungan sasaran, dan menerapkan migrasi data sesudahnya | Operasional | **Belum diberikan** |
| 2 | Keputusan audio antrean, **atau** persetujuan mengeluarkan audio dari scope `BE-SEC-003` | Kewenangan | **Belum diberikan** |

Selain dua butir itu, **tidak ada pekerjaan desain yang tersisa** untuk `BE-SEC-003`. Daftar
identitas, perilaku migrasi, acceptance criteria, dan batas rollback-nya sudah lengkap di L.3.

### M.3 Yang tidak perlu diputuskan untuk memulai

Sengaja dicatat agar tidak menahan pekerjaan tanpa perlu:

| Pertanyaan | Mengapa tidak menahan |
| --- | --- |
| Departemen × Posisi mana yang boleh `approve` tindakan? | Pemecahan mempertahankan hak apa adanya. Penyempitan adalah tindakan owner **sesudahnya** |
| Departemen × Posisi mana yang boleh `execute`? | Sama |
| Siapa yang menandatangani surat dokter? | `MedicalCertificate` tidak masuk scope `BE-SEC-003` |
| Nama akhir entity `Sec*` | Baru relevan pada `BE-SEC-004` |

---

## N. Owner Business Decision yang Benar-Benar Tersisa

Disaring ketat. Butir yang dapat diselesaikan dari source sudah ditutup di bagian A dan tidak
diulang di sini.

### N.1 Empat keputusan tersisa

---

**N-1 · Siapa yang boleh mendengar dan mengunduh rekaman panggilan antrean?**
`MEMBLOKIR BE-SEC-003` (kecuali audio dikeluarkan dari scope)

| Field | Isi |
| --- | --- |
| Mengapa hanya owner | Berkas audio memuat nama pasien yang diumumkan. Menentukan siapa yang boleh mengaksesnya adalah keputusan privasi, bukan penalaran teknis |
| Bukti | Tiga kelas actor memanggil endpoint yang sama; `QueueDisplayRuntimeRead` hanya dipenuhi akun perangkat display (bagian J.1) |
| Keadaan sekarang | Hanya `[Authorize]`. Setiap akun yang login — termasuk akun kiosk — dapat mengunduh bila mengetahui `dateKey` dan `fileName` |
| Rekomendasi | Lindungi dengan **`QueueDisplayRuntimeRead` ATAU `QueueVoice.PlayAudio`**, lalu masukkan `QueueVoice.PlayAudio` ke `DOCTOR_OUTPATIENT_BASE` dan profil perawat |
| Bila memilih `QueueDisplayRuntimeRead` saja | Fitur "Panggil Pasien dengan suara" **berhenti bekerja** untuk dokter dan perawat |
| Bila dibiarkan `[Authorize]` saja | Bertentangan dengan `DECISION 7` yang menolak jalur permisif |
| Alternatif menunda | Keluarkan audio dari scope `BE-SEC-003`; enam pemecahan lain tetap jalan; audio menjadi task tersendiri |

---

**N-2 · Departemen × Posisi mana yang berwenang MENYETUJUI tindakan pasien?**
`TIDAK memblokir BE-SEC-003` — dibutuhkan sebelum `BE-SEC-006`

| Field | Isi |
| --- | --- |
| Mengapa hanya owner | Kewenangan klinis dan finansial. Tindakan menimbulkan tagihan, dan `approve` adalah gerbang sebelum `execute` |
| Bukti | `ExecuteProcedure` menolak bila `IsNeedApproval && !IsApproved` (bagian D.2) |
| Keadaan sekarang | Siapa pun yang memegang `PatientProcedure.Update` dapat menyetujui — termasuk dokter yang membuat tindakan itu sendiri |
| Default bila belum diputuskan | **FAIL CLOSED.** `procedure.approve` tidak masuk profil mana pun. Sesuai `DECISION 5` |
| Yang dibutuhkan | Daftar Departemen × Posisi approver, atau pernyataan bahwa kewenangan ini belum dipakai |

---

**N-3 · Departemen × Posisi mana yang berwenang MELAKSANAKAN tindakan pasien?**
`TIDAK memblokir BE-SEC-003` — dibutuhkan sebelum `BE-SEC-006`

| Field | Isi |
| --- | --- |
| Mengapa hanya owner | Kewenangan klinis. `DECISION 5` melarang pemberian otomatis berdasarkan `UserType` atau role dokter |
| Bukti | `ExecuteProcedure` mengubah `ProcedureStatus` menjadi `Completed` dan mencatat `ExecutedByUserId` |
| Default bila belum diputuskan | `procedure.execute` tidak masuk `DOCTOR_OUTPATIENT_BASE`. Profil `DOCTOR_OUTPATIENT_PROCEDURE_EXECUTOR` tersedia tetapi tidak dipasang ke organisasi mana pun |
| Catatan | Lapisan clinical privilege per jenis tindakan menyusul terpisah dan **bukan** pengganti keputusan ini |

---

**N-4 · Penetapan frontend authority dan prefix task frontend untuk `SEC-BP-001`**
`TIDAK memblokir BE-SEC-003 sampai BE-SEC-010`

| Field | Isi |
| --- | --- |
| Mengapa hanya owner | `blueprint-manifest.md` mencatat *Frontend authority: "Belum ditetapkan"*. Kepemilikan modul adalah kewenangan owner, bukan kesimpulan source |
| Yang dibutuhkan | Penetapan pemegang frontend dan persetujuan prefix `FE-SEC` |
| Dampak bila belum ada | Tiga task frontend tidak dapat diberi Task ID resmi |

### N.2 Satu wewenang operasional

**N-5 · Wewenang database untuk `BE-SEC-003`**

| Field | Isi |
| --- | --- |
| Yang diminta | (a) menjalankan laporan **read-only** `SysAccessPolicy` per identitas pada lingkungan sasaran; (b) menerapkan migrasi data pemecahan setelah laporannya ditinjau |
| Mengapa terpisah | `AGENTS.md` backend menetapkan eksekusi database sebagai wewenang tersendiri. `BE-SEC-001` menempuh jalur yang sama |
| Lingkup | Satu lingkungan pada satu waktu, dimulai dari development |
| Tanpa ini | `BE-SEC-003` tidak dapat dimulai, karena kolom dampak pada matriks C.2 tidak dapat diisi tanpa menebak |

### N.3 Yang ditunda dan tidak perlu diputuskan sekarang

| Butir | Kapan kembali relevan |
| --- | --- |
| Siapa yang menandatangani surat dokter | Setelah route `doctor-certificates` diperbaiki **dan** `MedicalCertificate.Update` dipecah |
| Definisi "pegawai aktif" untuk baseline Self Service | Sebelum `BE-SEC-011` |
| Apakah `procedure.cancel` dan `consultation.cancel` masuk profil rawat jalan | Sebelum `BE-SEC-006`; default tidak diberikan |
| Tenancy per rumah sakit pada otorisasi | Blueprint tersendiri (bagian A.2) |

---

## O. Disposisi Governance Dokumen Evidence

### O.1 Putusan

> **Pilihan A — dokumen evidence yang sah dan harus di-track.**

### O.2 Dasar

| Yang diperiksa | Hasil |
| --- | --- |
| Wewenang mode | `AGENTS.md` backend, `MODULE BLUEPRINT MODE`: `docs/module-blueprints/**` adalah target tulis yang sah untuk artefak blueprint berbasis bukti. Tidak ada source aplikasi yang disentuh |
| Konvensi folder | `evidence/` dipakai **4 dari 10** blueprint: `billing-kasir` (5 berkas), `human-resource` (3), `rawat-inap` (2), `platform-authorization` (1) |
| Konvensi penamaan | Berprefix nomor urut, contoh `02-requirement-completeness-gate.md`, `06-be-bkc-017-acceptance-evidence-matrix.md`. Berkas ini mengikuti pola yang sama |
| Bukan laporan task | `rules/rule-output/lokasi-laporan-task.md` menetapkan `task/report/**` khusus untuk `build-module-backend`/`build-module-frontend` setelah implementasi **dan validasinya** dijalankan. `BE-SEC-002` tidak menjalankan build maupun test — ia audit. Menaruhnya di `task/report/` justru salah arsip |
| Catatan kejujuran | `evidence/` **tidak** tercantum di `_template/` maupun di `blueprint-output-contract.md`. Statusnya konvensi de facto, bukan folder kontrak kanonik. Ini tidak membatalkan keabsahannya, tetapi layak dicatat agar kelak dapat dibakukan |

Pilihan B ditolak: tidak ada "canonical architecture document" yang lebih tepat — `02-backend-architecture.md`
adalah artefak desain modul bisnis, sedangkan ini audit lintas platform. Pilihan C ditolak: tidak
ada aturan yang dilanggar.

### O.3 Berkas yang menjadi bagian dari disposisi ini

| Berkas | Status |
| --- | --- |
| `evidence/01-be-sec-002-audit-architecture.md` | Ada, untracked |
| `evidence/02-be-sec-002-decision-closure.md` | Dokumen ini, baru, untracked |

### O.4 Yang harus dikerjakan sebelum implementasi dimulai

Arahan pemilik sistem: *"Jangan biarkan working tree untracked ketika implementation dimulai."*

Empat berkas berikut harus masuk satu commit dokumentasi, **atas perintah eksplisit terpisah** —
tidak dilakukan sekarang:

| # | Berkas | Perubahan |
| ---: | --- | --- |
| 1 | `evidence/01-be-sec-002-audit-architecture.md` | Track apa adanya |
| 2 | `evidence/02-be-sec-002-decision-closure.md` | Track apa adanya |
| 3 | `roadmap/backend-roadmap.md` | `BE-SEC-002` ditandai `AUDIT COMPLETED / DECISIONS CLOSED`; tabel "Task yang BELUM dikerjakan" diganti dengan sembilan baris `BE-SEC-003` … `BE-SEC-011`; bagian "Keputusan owner yang masih terbuka" disesuaikan dengan bagian N |
| 4 | `roadmap/requirement-traceability.md` | Requirement `SEC-REQ-013` dan seterusnya untuk Business Permission, Access Profile, resolver, dan kontrak frontend, dengan bukti menunjuk kedua dokumen evidence |
| 5 | `blueprint-manifest.md` | Bagian *Prefix entity operasional* diperbarui menjadi `Sec` yang sudah disetujui; `revision` dinaikkan; frontend authority tetap ditandai belum ditetapkan sampai N-4 dijawab |

Butir 3 sampai 5 adalah pekerjaan `MODULE BLUEPRINT MODE` yang **belum dijalankan** pada sesi ini,
karena arahan hanya mengizinkan pembaruan dokumen evidence. Keduanya perlu perintah terpisah.

---

## P. Rekomendasi

> ## `OWNER_DECISION_REQUIRED`

### P.1 Alasan, dan mengapa cakupannya sempit

Pekerjaan desain untuk task pertama **sudah selesai**. Tidak ada arsitektur, identitas, kriteria,
maupun batas rollback yang masih menggantung untuk `BE-SEC-003`. Enam dari tujuh keputusan `Y.3`
tertutup, dan seluruh temuan baru sudah diklasifikasikan.

Yang menahan hanya dua hal, dan keduanya berada di luar kewenangan teknis:

| # | Penahan | Sifat | Jalan keluar bila ingin segera mulai |
| ---: | --- | --- | --- |
| **N-5** | Wewenang menjalankan laporan read-only `SysAccessPolicy` dan menerapkan migrasi data pada lingkungan development | Operasional | Tidak ada. Ini prasyarat mutlak — mengisi kolom dampak tanpa data berarti menebak |
| **N-1** | Keputusan audio antrean | Privasi/kewenangan | **Ada**: keluarkan audio dari scope `BE-SEC-003`. Enam pemecahan lain tetap berjalan penuh |

Bila kedua butir itu dijawab, `BE-SEC-003` siap dikerjakan tanpa pekerjaan desain tambahan.

### P.2 Yang TIDAK menahan

Ditegaskan agar tidak menahan pekerjaan tanpa perlu:

- **N-2** dan **N-3** (siapa boleh approve/execute) tidak menahan `BE-SEC-003`. Pemecahan
  mempertahankan hak apa adanya; penyempitan adalah tindakan owner sesudahnya.
- **N-4** (frontend authority) tidak menahan apa pun sampai `BE-SEC-010`.
- Surat dokter tidak menahan apa pun — sudah `BROKEN_DEPENDENCY` dan dikeluarkan dari seluruh scope.

### P.3 Jalur tercepat menuju implementasi

1. Berikan wewenang **N-5** untuk lingkungan development.
2. Jawab **N-1**, atau setujui pengeluaran audio dari scope `BE-SEC-003`.
3. Perintahkan commit dokumentasi sesuai O.4.
4. `BE-SEC-003` dimulai lewat `quilvian-engineering-skills:build-module-backend`.

### P.4 Ringkasan status closure

| Ukuran | Nilai |
| --- | ---: |
| Keputusan `Y.3` yang tertutup | **6 dari 7** |
| Business Permission yang diklasifikasi | **19 dari 19** |
| Di antaranya dapat diaktifkan tanpa menyentuh controller | **10** |
| Identitas teknis yang perlu dipecah untuk pilot | **7 → 28** |
| Task formal hasil dekomposisi | **11** (8 backend + 3 frontend) |
| Di antaranya sudah lengkap desainnya | **10** — `BE-SEC-011` sengaja belum |
| Collision prefix `Sec` | **Nol** — `OWNER APPROVED` |
| Owner business decision tersisa | **4** + 1 wewenang operasional |
| Yang menahan task pertama | **2** |

---

## Pernyataan Penutup

| Batasan | Status |
| --- | --- |
| Perubahan source aplikasi | **Tidak ada** |
| Entity baru | **Tidak ada** |
| Migration | **Tidak ada** |
| Eksekusi database | **Tidak ada** |
| Perubahan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | **Tidak ada** — disetujui, dijadwalkan pada `BE-SEC-004` |
| Perubahan roadmap, traceability, manifest | **Tidak ada** — menunggu perintah terpisah (O.4) |
| `git commit` | **Tidak ada** |
| `git push` | **Tidak ada** |
| Working tree frontend | Bersih, tidak disentuh |
| Working tree backend | Hanya bertambah dua dokumen evidence, keduanya untracked |

Implementasi belum diberi izin. `BE-SEC-003` menunggu dua jawaban pada bagian P.1.
