# Urutan Pekerjaan Modul Rawat Inap

## 1. Identitas dokumen

| Atribut | Nilai |
|---|---|
| Tujuan | Menjabarkan seluruh task Rawat Inap yang belum selesai beserta urutan pengerjaannya |
| Tanggal | 11 September 2026, **diperbarui 12 September 2026** |
| Blueprint | `RWI-BP-001` revision `6`, bentuk `COMPOSITE`, status `approved` |
| Kontrak berlaku | `episode-rawat-inap` `0.8.0`, `dokter-rawat-inap` `0.5.0`, `keperawatan` `0.4.0` — ketiganya `approved` 11 September 2026 lewat `RWI-DEC-105` |
| Backend | `NewQuilvianSystemBackend`, branch `MHamzah`, `HEAD` `d6858a9d` |
| Frontend | `QuilvianSystemFrontendDev`, branch `HamzahV2`, `HEAD` `7f6b9356` |
| Sifat dokumen | Catatan kerja. Ia **menurunkan** dari roadmap dan kontrak, tidak menciptakan task baru |

Bila dokumen ini berbeda dari roadmap sub-modul, **roadmap yang berlaku**. Letaknya:
`docs/module-blueprints/rawat-inap/<sub-modul>/roadmap/`.

---

## 2. Di mana posisi modul ini sekarang

Rawat Inap **bukan** modul kosong. Sebagian besar sudah terbangun dan terbukti.

| Sub-modul | Task backend | Task frontend |
|---|---|---|
| `episode-rawat-inap` | 40 dari 42 aktif selesai | 24 dari 28 selesai |
| `keperawatan` | 14 dari 14 selesai | 6 dari 6 selesai |
| `dokter-rawat-inap` | **22 dari 22 selesai** | **9 dari 9 selesai** |

Empat nomor `BE-RWI-037` s.d. `BE-RWI-040` pada sisi `episode-rawat-inap` sudah dibatalkan atau
dipindah ke Billing, sehingga tidak ikut dihitung.

Yang tersisa terbagi tiga kelompok, dan **hanya kelompok pertama yang benar-benar siap dikerjakan
hari ini**.

| Kelompok | Isi | Keadaan |
|---|---|---|
| A. Gelombang 1A | 7 task koreksi keselamatan | ✅ **TUNTAS 7 dari 7**, 11 s.d. 12 September 2026 |
| B. Sisa pekerjaan lama | 12 task pada 9 baris | ✅ **6 selesai**, 🟡 1 sebagian, 🟡/⛔ 5 tertahan endpoint Billing |
| C. Gelombang 2 ke atas | 9 kemampuan yang belum ada sama sekali | **Belum boleh direncanakan.** Menunggu keputusan |

---

## 3. Kelompok A — Gelombang 1A, ✅ TUNTAS 7 dari 7

Seluruhnya menutup empat penyimpangan `P0` yang ditemukan `PRD-to-MVP-Rawat-Inap-V2`.
**Ketujuhnya selesai** — enam pada 11 September 2026, dan `FE-RWI-062` pada 12 September 2026.

### 3.1 Urutan yang disarankan

| Urut | Task | Sub-modul | Repo | Status |
|---:|---|---|---|---|
| 1 | ✅ `BE-RWI-074` | `episode-rawat-inap` | Backend | Selesai 11 September 2026 |
| 2 | ✅ `BE-RWI-073` | `episode-rawat-inap` | Backend | Selesai 11 September 2026 |
| 3 | ✅ `BE-RWI-075` | `dokter-rawat-inap` | Backend | Selesai 11 September 2026 |
| 4 | ✅ `BE-RWI-077` | `keperawatan` | Backend | Selesai 11 September 2026 |
| 5 | ✅ `BE-RWI-078` | `keperawatan` | Backend | Selesai 11 September 2026 |
| 6 | ✅ `BE-RWI-076` | `dokter-rawat-inap` | Backend | Selesai 11 September 2026 |
| 7 | ✅ `FE-RWI-062` | `episode-rawat-inap` | **Frontend** | Selesai 12 September 2026 |

Urutan 1 sampai 5 sebenarnya **boleh paralel** karena nol prasyarat. Urutan di atas dipilih supaya
risiko terbesar diselesaikan lebih dulu dan jalur kritis tidak menunggu.

**Satu hal yang belum tertutup dan bukan pekerjaan koding.** `BE-RWI-073` dan `FE-RWI-062` masih
**lokal, belum di-commit**, sehingga butir DoD "dirilis pada gelombang yang sama" belum dapat
dinyatakan. Bila backend naik lebih dulu tanpa frontend, tiga berkas test frontend akan gagal.
Commit dan push dijalankan pemilik, bukan agent.

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

#### ✅ `BE-RWI-077` — Deret waktu tanda vital tidak dapat diputus diam-diam

| Field | Isi |
|---|---|
| Outcome | Tanda vital yang sudah tercatat tidak dapat dihilangkan, dan penanda pemberitahuan dokter tidak dapat dimatikan lewat penghapusan |
| Yang dikerjakan | Hapus route `HttpDelete`; sesuaikan test |
| Letak | `Areas/HealthServices/ClinicalManagement/Controllers/PatientVitalSignController.cs` |
| Prasyarat | Nol |
| Status | ✅ **Selesai 11 September 2026.** Nol atribut `HttpDelete` tersisa. `dotnet build` dan uji integrasi `NOT RUN` atas permintaan pemilik. Bukti: [BE-RWI-077](../../module-blueprints/rawat-inap/keperawatan/task/report/backend/BE-RWI-077.md) |

**Kenapa tanda vital berbeda dari dokumen lain.** Ia deret waktu, bukan dokumen tunggal. Menghapus
satu baris tidak menyisakan lubang yang terlihat: grafik tetap tersambung dan tetap tampak wajar.

**Satu butir DoD pasti tidak terpenuhi, dan itu disengaja.** `ClinicalDocumentKind.VitalSign` belum
termasuk jenis yang ditegakkan mesin keutuhan dokumen, sehingga pembatalan dokumen final belum
dapat diuji. Tulis **`NOT RUN` apa adanya** pada laporan, jangan dihilangkan dari daftar. Dilacak
`V2-UNK-01`, pemiliknya `MedicalRecordManagement`.

**Regresi Rawat Jalan dan IGD wajib.**

---

#### ✅ `BE-RWI-078` — Perawat hanya menulis untuk pasien di unit tempat ia bertugas

| Field | Isi |
|---|---|
| Outcome | Dokumentasi keperawatan hanya dapat ditulis perawat yang bertugas di unit tempat pasien dirawat |
| Yang dikerjakan | Tambah kemampuan pada resolver untuk menilai unit perawat; ambil penulis dari `ApplicationUser.EmployeeId`; tolak `nurseId` payload yang berbeda |
| Prasyarat | Nol |
| Status | ✅ **Selesai 11 September 2026.** Sumber data unit ditetapkan lewat `WfpOrganizationAssignment` berperiode, bukan kolom baru pada `InpNurseAssignment`. `AC-KEP-050`, uji integrasi, dan `dotnet build` `NOT RUN` atas permintaan pemilik. Bukti: [BE-RWI-078](../../module-blueprints/rawat-inap/keperawatan/task/report/backend/BE-RWI-078.md) |

**Kemampuannya belum ada sama sekali.** `InpatientClinicalContextService` nol menyebut perawat;
satu-satunya pemeriksaan yang tersedia adalah `IsDoctorAssignedAsync`.

**Sumber data unit tempat perawat bertugas sudah ditetapkan pada pelaksanaan task ini**: rantai
`MstServiceUnit.OrganizationUnitId` di sisi pasien dan `WfpOrganizationAssignment` berperiode di
sisi perawat. **Satu jalan dilarang:** menambahkan kolom unit ke `InpNurseAssignment`, karena unit episode
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

## 4. Kelompok B — sisa pekerjaan lama, dikerjakan 12 September 2026

Dua belas task pada sembilan baris. **Empat selesai, dua naik sebagian, enam tertahan hal yang
bukan koding.**

### 4.1 Yang selesai

| Task | Sub-modul | Hasil |
|---|---|---|
| ✅ `BE-RWI-068` | `dokter` | Coding sudah lengkap sejak 9 September 2026 dan `dotnet build` `0 Error(s)`. Ditandai selesai atas instruksi pemilik bahwa implementasi backend cukup ditandai selesai begitu coding-nya ada. `dotnet test` dan uji migration tetap `NOT RUN` |
| ✅ `FE-RWI-044` | `dokter` | Elemen terakhir dibangun: tombol **+ Tambah Diagnosis**. Penghalangnya gugur karena `BE-RWI-068` sudah melonggarkan `ConsultationId` menjadi `Guid?` — diverifikasi langsung pada source |
| ✅ `FE-RWI-057` | `episode` | Kartu tempat tidur menyebut aturan yang menolaknya. Cacat pemilik 9 September 2026 ditutup: alasan server kini mendahului kalimat status master |
| ✅ `FE-RWI-039` | `episode` | Keenam kriteria terbukti **sudah** terpenuhi pada source dan dikunci enam test baru. **Nol baris source baru.** Blokirnya ternyata tidak berdiri — skema §15 sudah mengunci batasnya, dan `RWI-UI-GAP-007` menahan pembuktian, bukan pembangunan |
| ✅ `FE-RWI-050` | `dokter` | Kriteria 5 ditutup sesudah pemilik peta modul **menetapkan urutan daftar** `FE-INP-09`: episode, lalu dokter, lalu keperawatan. Source ternyata merender keperawatan mendahului dokter, sehingga urutannya ditukar dan dikunci test |
| ✅ `FE-RWI-058` | `episode` | `RWI-UI-GAP-008` **ditutup lebih dulu** dengan menulis skema `FE-INP-20` pada `05-skema-tampilan.md` bagian 3.5A, revision naik `0.4` → `0.5`. Langkah Deposit lalu dibangun pada kedua jalur dengan **nol endpoint dipanggil** |

### 4.2 Yang naik tetapi belum ✅

| Task | Sub-modul | Keadaan | Yang menahan, dan kenapa bukan koding |
|---|---|---|---|
| 🟡 `FE-RWI-035` | `episode` | **6 dari 8**, naik dari 5 | Kriteria 8 tertutup karena `FE-RWI-039` selesai. Kriteria 1 menunggu `RWI-UI-GAP-007`; kriteria 5 menunggu **katalog hak akses per butir** dikirim backend ke frontend |

### 4.3 Yang tertahan dan nol barisnya dapat ditulis

| Task | Yang menahan | Bukti |
|---|---|---|
| 🟡 `FE-RWI-059` | `BE-BKC-039` | Pencarian `deposit-policies` pada seluruh `Areas/` mengembalikan **nol hasil**. Kriteria 1 melarang angka minimum ditulis di kode layar |
| 🟡 `FE-RWI-060` | **Temuan baru** — `DepositTopUpRequest` | Route `top-ups` memang ada, tetapi badan permintaannya menuntut `PaymentMethodId` (`Guid` non-nullable) dan `Reason` (`[Required]`). Alur admisi **nol** mengenal `paymentMethodId`, dan baik task ini maupun `FE-RWI-058` tidak menyediakan cara menangkapnya |
| 🟡 `FE-RWI-061` | `BE-BKC-040` | Pencarian `deposits/episodes` pada seluruh `Areas/` mengembalikan **nol hasil** |
| ⛔ `BE-RWI-071` | `BE-BKC-040` | Sama. Empat dari lima kriteria tidak punya angka untuk diuji |
| ⛔ `BE-RWI-072` | `BE-BKC-040` | Sama. Kriteria 4 menuntut sistem membedakan "tidak terbaca" dari "tidak ada kekurangan"; gerbang di atas sumber yang belum ada hanya punya dua kemungkinan dan keduanya merugikan |

`BE-BKC-039` dan `BE-BKC-040` berada pada roadmap `billing-kasir` dengan status
`BLOCKED_PENDING_OWNER_APPROVAL`, menunggu `RWI-OQ-053` — pemilik `BillingManagement` belum bernama.

**Dua gap lingkungan yang menahan banyak hal sekaligus.**

| Gap | Isinya |
|---|---|
| `RWI-UI-GAP-007` | Data master rawat inap pada lingkungan target belum layak: pengaturan `DEFAULT` tidak ada, butir administrasi kosong, papan menunjukkan nol bed. Menahan **pembuktian runtime**, bukan pembangunan layar |
| ~~`RWI-UI-GAP-008`~~ | ✅ **DITUTUP 12 September 2026.** Skema `FE-INP-20` ditulis pada `05-skema-tampilan.md` bagian 3.5A, revision naik `0.4` → `0.5`. Statusnya `draft` dan **belum disetujui pemilik**. Penutupannya membuka `FE-RWI-058`, tetapi **tidak** membuka ketiga task deposit lainnya — ketiganya ternyata tertahan endpoint Billing, bukan skema |

`RWI-UI-GAP-007` tetap terbuka dan bukan pekerjaan koding: ia diselesaikan dengan menyiapkan data
pada lingkungan target, dan menanam data tiruan di frontend **dilarang**.

**Satu pernyataan roadmap terbukti tidak akurat.** Kartu `FE-RWI-060` berbunyi "Backend-nya sudah
siap apa adanya — lihat `RWI-FACT-018`". Route-nya memang ada, tetapi `DepositTopUpRequest`
menuntut `PaymentMethodId` dan `Reason` yang tidak dapat disediakan alur admisi. Mengirim
`Guid.Empty` atau memilih metode pembayaran secara otomatis berarti mencatat uang atas metode yang
tidak pernah dipilih petugas, jadi **nol baris ditulis** dan temuannya dilaporkan.

### 4.4 Satu cacat ditemukan dan diperbaiki di luar daftar ini

Saat memeriksa kriteria 8 milik `FE-RWI-035` atas keenam layar bukti runtime, layar **Butir
Administrasi Rawat Inap** milik `FE-RWI-040` ditemukan **tidak memiliki tombol Coba Lagi sama
sekali**, padahal kriteria 5 task itu menuntutnya dan laporannya menyatakan tombol itu ada di
`Hero`. Tombolnya dipasang memakai `refreshData` yang sudah diekspor hook-nya sejak semula.

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
| Skill yang dipakai | `build-module-backend` untuk enam task backend Gelombang 1A; `build-module-frontend` untuk `FE-RWI-062` |
| Butir verifikasi backend | **Instruksi pemilik 12 September 2026:** implementasi backend tidak perlu menunggu `dotnet build`; begitu coding-nya ada, task ditandai selesai. Butir yang tidak dijalankan tetap ditulis `NOT RUN` apa adanya |
| Migration yang belum diterapkan | `20260909065125_AddInpatientEpisodeContextToPatientDiagnosis` milik `BE-RWI-068` **belum diterapkan ke database mana pun**, dan langkah mundurnya tidak simetris. Penerapannya menunggu wewenang terpisah |
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
