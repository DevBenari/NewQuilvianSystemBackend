# Urutan Pekerjaan Modul Rawat Inap

## 1. Identitas dokumen

| Atribut | Nilai |
|---|---|
| Tujuan | Menjabarkan seluruh task Rawat Inap yang belum selesai beserta urutan pengerjaannya |
| Tanggal | 11 September 2026 |
| Blueprint | `RWI-BP-001` revision `6`, bentuk `COMPOSITE`, status `approved` |
| Kontrak berlaku | `episode-rawat-inap` `0.8.0`, `dokter-rawat-inap` `0.5.0`, `keperawatan` `0.4.0` — ketiganya `approved` 11 September 2026 lewat `RWI-DEC-105` |
| Backend | `NewQuilvianSystemBackend`, branch `MHamzah`, `HEAD` `201de753` |
| Frontend | `QuilvianSystemFrontendDev`, branch `HamzahV2`, `HEAD` `7f6b9356` |
| Sifat dokumen | Catatan kerja. Ia **menurunkan** dari roadmap dan kontrak, tidak menciptakan task baru |

Bila dokumen ini berbeda dari roadmap sub-modul, **roadmap yang berlaku**. Letaknya:
`docs/module-blueprints/rawat-inap/<sub-modul>/roadmap/`.

---

## 2. Di mana posisi modul ini sekarang

Rawat Inap **bukan** modul kosong. Sebagian besar sudah terbangun dan terbukti.

| Sub-modul | Task backend | Task frontend |
|---|---|---|
| `episode-rawat-inap` | 38 dari 44 selesai | 16 dari 28 selesai |
| `keperawatan` | 12 dari 12 selesai | 6 dari 6 selesai |
| `dokter-rawat-inap` | 19 dari 20 selesai | 7 dari 9 selesai |

Yang tersisa terbagi tiga kelompok, dan **hanya kelompok pertama yang benar-benar siap dikerjakan
hari ini**.

| Kelompok | Isi | Keadaan |
|---|---|---|
| A. Gelombang 1A | 7 task koreksi keselamatan | **Siap.** Kontrak sudah disetujui |
| B. Sisa pekerjaan lama | 9 task yang tertinggal sebagian atau tertahan | Sebagian siap, sebagian tertahan gap lingkungan |
| C. Gelombang 2 ke atas | 9 kemampuan yang belum ada sama sekali | **Belum boleh direncanakan.** Menunggu keputusan |

---

## 3. Kelompok A — Gelombang 1A, tujuh task yang siap

Ini yang dikerjakan lebih dulu. Seluruhnya menutup empat penyimpangan `P0` yang ditemukan
`PRD-to-MVP-Rawat-Inap-V2`.

### 3.1 Urutan yang disarankan

| Urut | Task | Sub-modul | Repo | Kenapa urutannya begini |
|---:|---|---|---|---|
| 1 | `BE-RWI-074` | `episode-rawat-inap` | Backend | Jalur kritis. Satu-satunya migration, dan satu-satunya prasyarat task lain |
| 2 | `BE-RWI-073` | `episode-rawat-inap` | Backend | Membuka pekerjaan frontend di repo sebelah |
| 3 | `BE-RWI-075` | `dokter-rawat-inap` | Backend | Membentuk pola tutup-hapus yang ditiru task berikutnya |
| 4 | `BE-RWI-077` | `keperawatan` | Backend | Menyalin pola dari `BE-RWI-075` |
| 5 | `BE-RWI-078` | `keperawatan` | Backend | Butuh keputusan teknis di dalam task |
| 6 | `BE-RWI-076` | `dokter-rawat-inap` | Backend | Menunggu `BE-RWI-074` |
| 7 | `FE-RWI-062` | `episode-rawat-inap` | **Frontend** | Menunggu `BE-RWI-073` |

Urutan 1 sampai 5 sebenarnya **boleh paralel** karena nol prasyarat. Urutan di atas dipilih supaya
risiko terbesar diselesaikan lebih dulu dan jalur kritis tidak menunggu.

### 3.2 Pengelompokan rilis — bagian yang paling mudah terlewat

`BE-RWI-073` dan `FE-RWI-062` **wajib dirilis dalam satu gelombang**.

| Bila | Akibatnya |
|---|---|
| Backend turun lebih dulu | Tiga berkas test frontend gagal, karena menguji kode penolakan yang tidak pernah terbit lagi |
| Frontend turun lebih dulu | Petugas menerima penolakan tanpa pesan yang dapat dibaca |

Lima task lainnya boleh dirilis sendiri-sendiri.

### 3.3 Rincian tiap task

#### `BE-RWI-074` — Penugasan dokter mengenal DPJP, konsulen, dan dokter jaga

| Field | Isi |
|---|---|
| Outcome | Kepala ruangan dapat melibatkan konsulen dan memanggil dokter jaga tanpa menggusur DPJP |
| Yang dikerjakan | Enum `InpDoctorAssignmentRole` dengan nilai `Dpjp=1`, `Consultant=2`, `OnCallDoctor=3`; kolom `AssignmentRole` `int` `NOT NULL DEFAULT 1` pada `InpDoctorAssignment`; ganti index unik; tambah index pendukung; satu migration |
| Prasyarat | Nol |
| Yang menunggunya | `BE-RWI-076` |
| Kontrak | `data/data-dictionary.md` bagian 2 dan 2.1; `02-backend-architecture.md` bagian 0.2 s.d. 0.4 |

**Kenapa index-nya harus berubah.** Index `IX_InpDoctorAssignment_EpisodeId_Active` hari ini
memfilter hanya `EndDateTime IS NULL`, sehingga satu episode hanya boleh punya **satu** penugasan
terbuka. Selama tabel itu cuma menyimpan DPJP, filter itu benar. Begitu konsulen masuk, baris kedua
**ditolak database** dan keputusan pemilik tidak akan pernah dapat dijalankan. Filter barunya
`EndDateTime IS NULL AND AssignmentRole = 1`, yang justru menegakkan bunyi `INV-INP-03` apa adanya.

**Urutan migration mengikat, tiga langkah.**

| Urut | Langkah |
|---:|---|
| 1 | Tambah kolom dengan `DEFAULT 1` |
| 2 | Isi baris lama menjadi `1` secara eksplisit |
| 3 | Buang index lama, buat dua index baru |

Membalik urutannya membuka jeda ketika dua DPJP aktif dapat tersimpan.

**Batas rollback.** Aman selama belum ada satu pun baris berperan `Consultant` atau `OnCallDoctor`.
Sesudah itu index lama akan menolak baris tersebut, sehingga pemulihan harus **maju**, bukan mundur.
Batas ini **wajib ditulis pada laporan task**.

**Pengisian data lama bukan tebakan.** Sebelum perubahan ini tabelnya memang hanya menyimpan DPJP,
sehingga nol baris ambigu dan nol laporan `unresolved` dibutuhkan.

---

#### `BE-RWI-073` — Kelayakan tempat tidur berhenti menilai penghuni kamar lain

| Field | Isi |
|---|---|
| Outcome | Kamar berisi pasien laki-laki dapat menerima pasien perempuan pada tempat tidur yang memang dikonfigurasi menerima keduanya |
| Yang dikerjakan | Hapus blok aturan 6 beserta kode `ROOM_GENDER_MIXED`; cabut klausa penghuni pada aturan 5; periksa `LoadRoomOccupantsAsync` apakah menjadi kode mati; sesuaikan unit test |
| Letak | `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs`, method `EvaluatePlacementEligibilityAsync` |
| Prasyarat | Nol |
| Yang menunggunya | `FE-RWI-062` |

**Risiko utamanya bukan gagal menghapus, melainkan menghapus terlalu banyak.** Aturan isolasi
berada di blok yang sama dengan aturan yang dicabut. Regresi `ISOLATION_REQUIRED` dan
`ISOLATION_BED_RESERVED` adalah pembuktian terpenting task ini.

**Yang tidak boleh ikut berubah.**

| Hal | Alasan |
|---|---|
| `BED_GENDER_MISMATCH` aturan 4 | Kelayakan tingkat tempat tidur tetap berlaku |
| Aturan isolasi 7 dan 8 | Bagian A `RWI-RULE-012` tidak tersentuh |
| Pengecualian boks bayi | Tetap berlaku bagi dua aturan yang tersisa |
| Nomor aturan 7 dan 8 | **Jangan digeser.** `ruleNumber` ikut terkirim pada response dan sudah dipakai test. Nomor 6 dibiarkan kosong dan tidak dipakai ulang |

---

#### `BE-RWI-075` — Catatan terpadu tidak dapat disembunyikan lagi

| Field | Isi |
|---|---|
| Outcome | CPPT final tidak dapat dihilangkan dari rekam medis oleh siapa pun |
| Yang dikerjakan | Hapus route `HttpDelete`; panggil `EnsureMutableAsync` pada jalur pembatalan |
| Letak | `Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs` |
| Prasyarat | Nol |

**Menutup `DELETE` saja tidak cukup.** Jalur `PATCH /{id}/cancel` hari ini tidak memanggil
pemeriksaan keutuhan dokumen. Tanpa perubahan itu, catatan final yang tadinya dapat dihapus akan
dapat dibatalkan, dan hasilnya sama saja bagi pembaca rekam medis. Kedua bagian wajib satu task.

**Regresi Rawat Jalan wajib**, karena controller ini dipakai bersama.

---

#### `BE-RWI-077` — Deret waktu tanda vital tidak dapat diputus diam-diam

| Field | Isi |
|---|---|
| Outcome | Tanda vital yang sudah tercatat tidak dapat dihilangkan, dan penanda pemberitahuan dokter tidak dapat dimatikan lewat penghapusan |
| Yang dikerjakan | Hapus route `HttpDelete`; sesuaikan test |
| Letak | `Areas/HealthServices/ClinicalManagement/Controllers/PatientVitalSignController.cs` |
| Prasyarat | Nol |

**Kenapa tanda vital berbeda dari dokumen lain.** Ia deret waktu, bukan dokumen tunggal. Menghapus
satu baris tidak menyisakan lubang yang terlihat: grafik tetap tersambung dan tetap tampak wajar.

**Satu butir DoD pasti tidak terpenuhi, dan itu disengaja.** `ClinicalDocumentKind.VitalSign` belum
termasuk jenis yang ditegakkan mesin keutuhan dokumen, sehingga pembatalan dokumen final belum
dapat diuji. Tulis **`NOT RUN` apa adanya** pada laporan, jangan dihilangkan dari daftar. Dilacak
`V2-UNK-01`, pemiliknya `MedicalRecordManagement`.

**Regresi Rawat Jalan dan IGD wajib.**

---

#### `BE-RWI-078` — Perawat hanya menulis untuk pasien di unit tempat ia bertugas

| Field | Isi |
|---|---|
| Outcome | Dokumentasi keperawatan hanya dapat ditulis perawat yang bertugas di unit tempat pasien dirawat |
| Yang dikerjakan | Tambah kemampuan pada resolver untuk menilai unit perawat; ambil penulis dari `ApplicationUser.EmployeeId`; tolak `nurseId` payload yang berbeda |
| Prasyarat | Nol |

**Kemampuannya belum ada sama sekali.** `InpatientClinicalContextService` nol menyebut perawat;
satu-satunya pemeriksaan yang tersedia adalah `IsDoctorAssignedAsync`.

**Sumber data unit tempat perawat bertugas belum ditetapkan**, dan menetapkannya bagian dari task
ini. **Satu jalan dilarang:** menambahkan kolom unit ke `InpNurseAssignment`, karena unit episode
berubah saat pasien dipindahkan dan salinannya akan berbeda sejak perpindahan pertama.

**Gerbang perawat memang lebih longgar daripada gerbang dokter, dan itu disengaja.** Perawat
berganti tiga shift sehari pada bangsal 20 sampai 30 pasien, sedangkan sistem tidak punya konsep
shift sama sekali. Dasarnya `RWI-DEC-100`.

---

#### `BE-RWI-076` — Catatan dokter selalu punya penulis yang benar-benar menulisnya

| Field | Isi |
|---|---|
| Outcome | Catatan klinis tidak dapat tersimpan atas nama dokter lain |
| Yang dikerjakan | Ambil dokter pelaku dari `ApplicationUser.DoctorId`; teruskan ke resolver pada **lima** grup jalur tulis; tolak `DoctorId` payload yang berbeda; nilai kewenangan pada waktu klinis |
| Prasyarat | **`BE-RWI-074`** |

**Lima grup, jadi lima test terpisah**, bukan satu yang mewakili semuanya: Doctor Consultation,
Patient Assessment, Patient Integrated Progress Note, Patient Diagnosis, dan Patient Procedure.

**Test hak akses lama akan tetap lulus tanpa disentuh**, karena task ini melahirkan nol Resource
dan nol Action baru. Hijau di situ **bukan bukti apa pun**. Satu-satunya bukti sah adalah skenario
negatif per pasien, dan wajib memakai peran nyata, bukan SuperAdmin.

---

#### `FE-RWI-062` — Layar berhenti menjelaskan penolakan yang tidak pernah terjadi lagi

| Field | Isi |
|---|---|
| Repo | **`QuilvianSystemFrontendDev`**, sesi terpisah |
| Yang dikerjakan | Hapus entri `ROOM_GENDER_MIXED` dari pemetaan kode; sesuaikan pesan `PATIENT_GENDER_UNKNOWN`; sesuaikan tiga assertion unit test; sesuaikan satu skenario e2e |
| Letak utama | `src/utils/health-services/inpatient-management/inpatient-placement-utils.jsx` |
| Prasyarat | **`BE-RWI-073`** |

**Satu assertion jangan sekadar dihapus.** `isIsolationFailure` memakai kode yang dicabut sebagai
contoh kode yang **bukan** kegagalan isolasi. Ia butuh contoh pengganti, bukan penghapusan, supaya
pembedaan isolasi tetap teruji.

---

## 4. Kelompok B — sisa pekerjaan lama

Sembilan task yang tertinggal dari gelombang sebelumnya. Dikerjakan **setelah** Gelombang 1A,
kecuali bila Anda memutuskan lain.

| Task | Sub-modul | Keadaan | Yang menahan |
|---|---|---|---|
| `FE-RWI-057` | `episode` | Siap dimulai | **Penghalangnya sudah gugur.** `BE-RWI-069` selesai dan kontrak `0.7.0` disetujui 10 September 2026 |
| `BE-RWI-068` | `dokter` | 🟡 sebagian | Tiga butir verifikasi `NOT RUN` atas instruksi pengguna 9 September 2026. Menjalankannya menaikkan status tanpa satu baris kode berubah |
| `FE-RWI-044` | `dokter` | 🟡 sebagian | Satu elemen layout menunggu `BE-RWI-068` |
| `FE-RWI-050` | `dokter` | 🟡 sebagian | 4 dari 5 kriteria terpenuhi |
| `FE-RWI-035` | `episode` | 🟡 sebagian | 5 dari 8 kriteria; bukti runtime menunggu `RWI-UI-GAP-007` |
| `FE-RWI-039` | `episode` | ⛔ terblokir | Approval skema layar, dan `RWI-UI-GAP-007` |
| `FE-RWI-058` s.d. `FE-RWI-061` | `episode` | 🟡 sebagian | `RWI-UI-GAP-008`. Keempatnya **tidak** menunggu backend |
| `BE-RWI-071`, `BE-RWI-072` | `episode` | ⛔ terblokir | `BE-BKC-040` pada roadmap `billing-kasir`, yang **nol barisnya ada** |

**Dua gap lingkungan yang menahan banyak hal sekaligus.**

| Gap | Isinya |
|---|---|
| `RWI-UI-GAP-007` | Data master rawat inap pada lingkungan target belum layak: pengaturan `DEFAULT` tidak ada, butir administrasi kosong, papan menunjukkan nol bed. Menahan **pembuktian runtime**, bukan pembangunan layar |
| `RWI-UI-GAP-008` | Menahan keempat task deposit frontend |

Keduanya bukan pekerjaan koding. Keduanya diselesaikan dengan menyiapkan data, bukan dengan menulis
kode, dan menanam data tiruan di frontend **dilarang**.

---

## 5. Kelompok C — Gelombang 2 ke atas, belum boleh direncanakan

Sembilan kemampuan yang **belum ada sama sekali** di source, ditemukan pada audit 11 September 2026.

| Kemampuan | Status audit | Yang menahan |
|---|---|---|
| Medication Administration Record | `Missing` | Nol model, nol service, nol endpoint |
| Observasi transfusi dan reaksi | `Missing` | Yang ada hanya jenis consent dan alasan bank darah |
| Sliding scale | `Missing` | Menuntut protokol berversi yang juga belum ada |
| Handover antar shift keperawatan | `Missing` | Yang ada hanya serah terima kasir |
| Clinical handover transfer antarunit | `Missing` | Transfer bed atomik sudah ada, artefak klinisnya belum |
| Adverse drug reaction | `Missing` | Bergantung pada MAR |
| Intake, output, drain, dan WSD | `Reuse with adapter` | Pola sudah berjalan di IGD dan layak jadi rujukan bentuk |
| Lima cara keluar | `Extend` | Enum baru punya tiga nilai; `Death` dan `Absconded` menunggu sign-off klinis |
| Bukti consent per episode | `Extend` | Lebih lengkap dari dugaan; yang kurang hanya versi template dan penegakan keutuhan |

**Kelompok ini belum boleh diturunkan menjadi task.** Sepuluh butir `OPEN-MVP-001` sampai
`OPEN-MVP-010` pada PRD V2 masih terbuka, dan `RWI-DEC-097` **tidak** membukanya. Langkah yang
tepat untuk kelompok ini bukan perencanaan, melainkan `requirement-completeness-gate` lebih dulu.

**Kelayakan keuangan berdiri sendiri.** Ia tetap `P0`, berstatus `external dependency`, dan
**bukan** `P1`. Ia menunggu `BE-BKC-040` milik roadmap Billing. Sesuai `RWI-DEC-102`, selama kontrak
authoritative Billing belum ada: `AC-MVP-021` sampai `AC-MVP-023` belum boleh dinyatakan lulus,
penandaan manual **tidak sah** sebagai dasar penutupan episode normal di produksi, dan kesiapan MVP
penuh belum boleh dinyatakan selesai.

---

## 6. Catatan operasional

| Hal | Ketentuannya |
|---|---|
| Skill yang dipakai | `build-module-backend` untuk enam task backend; `build-module-frontend` untuk `FE-RWI-062` |
| Wewenang | Setiap pemanggilan butuh **task ID** dan **wewenang tulis yang disebut eksplisit**. Approval kontrak 11 September 2026 **belum** mencakup wewenang menulis source |
| Repo frontend | Sesi terpisah di `QuilvianSystemFrontendDev`. Wewenang tulis backend tidak berlaku di sana |
| Build backend | Lambat, sekitar 25 menit, dan `bin/` dapat terkunci aplikasi yang sedang berjalan. Pakai keluaran build ke direktori scratch |
| Uji migration | Container Postgres sekali pakai. **Jangan** menerapkan migration ke environment mana pun tanpa wewenang terpisah |
| Git | Commit dan push dijalankan pemilik, bukan agent |
| Butir verifikasi yang tidak dijalankan | Ditulis **`NOT RUN` apa adanya** pada laporan task, bukan dihilangkan |
| Penutupan task | Selesai satu task berarti laporan ditulis **dan** register pada roadmap ikut diperbarui |

---

## 7. Pertanyaan terbuka yang menyertai seluruh pekerjaan ini

| ID | Isinya | Memblokir |
|---|---|---|
| `RWI-OQ-053` | Pemilik `BillingManagement` belum bernama | Kelayakan keuangan |
| `RWI-OQ-055` | Consent masih dapat dihapus, padahal PRD V2 menuntutnya menjadi bukti hukum | Penutupan bukti consent pada Gelombang 2 |
| `OPEN-MVP-004` | Kewenangan konsulen memutuskan pulang | Tidak memblokir; perilaku fail-closed berlaku, yaitu ditolak |
| `V2-UNK-01` | Sembilan jenis dokumen yang belum ditegakkan mesin keutuhan | Kelengkapan jalur pengganti `BE-RWI-077` |
| `OPEN-MVP-001` s.d. `OPEN-MVP-010` | Sepuluh butir keputusan PRD V2 | Seluruh Kelompok C |

Nomor task yang sudah dipensiunkan dan **tidak boleh dipakai ulang**: `BE-RWI-041` sampai
`BE-RWI-043`, dan `FE-RWI-042` sampai `FE-RWI-045` pada sisi `episode-rawat-inap`. ID bebas
berikutnya `BE-RWI-079` dan `FE-RWI-063`.
