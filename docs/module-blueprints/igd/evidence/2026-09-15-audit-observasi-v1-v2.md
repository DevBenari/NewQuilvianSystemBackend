# Audit dan Rekonsiliasi Workflow Observasi IGD — V1, Blueprint, V2

| Field | Nilai |
| --- | --- |
| Tanggal | 15 September 2026 |
| Jenis | Evidence audit + usulan perencanaan. **Bukan** implementasi, **bukan** perubahan roadmap |
| Skill | `plan-module-delivery` (mode rekonsiliasi) |
| Blueprint | `IGD-BP-001` revision `6` (`draft`); kontrak API/validation `0.5.0` |
| V1 (legacy reference) | Frontend `QuilvianSystemFrontendDev` ref `rizkiG` `2abf2183b` — dibaca lewat `git show`, tanpa checkout. Folder observasi identik dengan `origin/rizkiG` `a3a4fd977`. Backend V1 (`/IGDObservasi`) **tidak ada** di repository backend V2 |
| V2 backend | `NewQuilvianSystemBackend` branch `rizkiG` `0aa42668` |
| V2 frontend | `QuilvianSystemFrontendDev` branch `RizkiV2` `f37e949ea` + working tree (termasuk `FE-IGD-024`) |
| Batasan | Tanpa coding, migration, query basis data, build, test, commit |

---

## A. Kesimpulan eksekutif

**Observasi V2 = `PARTIAL`, bukan `DESIGN GAP`.**

Data model V2 sudah menyediakan hampir semua kemampuan klinis V1 — sebagian besar dengan cara yang
**lebih benar** daripada V1 — tetapi layar Observasi V2 **tidak menyambungkannya**:

1. **Tanda vital, GCS, kesadaran, dan oksigenasi** sudah ada di `TrxPatientVitalSign`
   (ClinicalManagement), sudah dapat diisi dari tab Assesmen Awal lewat `VitalSignTab`, dan
   `EmgObservationDetail.PatientVitalSignId` sudah punya FK, index, dan diterima API. **Nol**
   pemakai di frontend Observasi. Ini gap **wiring**, bukan gap data model.
2. **Keluaran cairan** (urine, muntah, perdarahan, keluaran lain, cairan masuk) sudah tampil di V2
   dan lebih baik dari V1 (angka ml, bukan teks "Ya/Tidak").
3. **ABCDE** di V2 hanya ada sebagai ringkasan teks pada penilaian triase; V1 mengulangnya pada
   setiap observasi. Pengulangannya di Observasi belum diputuskan → `IGD-OQ-085`.
4. **Alat bantu jalan napas** (OPA/NPA/ETT/LMA/stoma) **tidak punya tempat terstruktur** di V2 →
   `IGD-OQ-086`.
5. **Obat/dosis, EKG, DC Shock** milik domain lain (Farmasi/Tindakan/Resusitasi) dan tidak layak
   diduplikasi di Observasi.

`FE-IGD-024` **tetap valid**. Backend **tidak** butuh migration; yang dibutuhkan adalah penguatan
validasi tautan dan (direkomendasikan) ringkasan tanda vital pada response — lihat bagian J.

---

## B. Matriks kemampuan Observasi V1 (bukti source)

Sumber: `src/components/view/IGD/pengkajian/asuhan-keperawatan-igd/observasi/` pada `rizkiG` —
`index.jsx`, `section-form/observasi-ABC.jsx`, `observasi-gcs.jsx`, `observasi-alat-nafas.jsx`,
`_components/riwayat-observasi.jsx`, `_components/modal-detail-observasi.jsx`; slice
`Observasi-IGD/observasiIGDSlice.jsx`; hook `skrining/useVitalSignLatest.jsx`.

**Struktur V1:** satu "observasi" = satu header (ABCD, GCS, alat bantu napas, keterangan) + banyak
baris detail (waktu, obat, EKG, DC Shock, TD, RR, suhu, SpO₂, urine, muntah, perdarahan,
keterangan). Disimpan lewat `POST /IGDObservasi`. Tidak ada periode, status, indikasi, rencana,
atau penutupan.

| # | Kemampuan V1 | Bentuk input V1 | Terkirim ke backend? | Tampil kembali di riwayat/detail? | Catatan bukti |
| ---: | --- | --- | :-: | :-: | --- |
| V1-01 | Tanggal & jam observasi (header) | date + time | Ya `tglObservasi` | Ya | — |
| V1-02 | A — Airway | teks bebas | Ya | Ya | — |
| V1-03 | B — Breathing | teks bebas | Ya | Ya | — |
| V1-04 | C — Circulation | teks bebas | Ya | Ya | — |
| V1-05 | D — Disability | teks bebas | Ya | Ya | — |
| V1-06 | E — Exposure | **tidak ada isian** | Tidak | Tidak | Hanya kunci localStorage `observasi_exposure` yang dihapus saat simpan |
| V1-07 | GCS Eye / Motor / Verbal | **teks bebas** (`TextArea`), placeholder "Pilih…" | Ya `eye/motor/verbal` (string) | Ya (detail) | Wajib diisi (`rules.required`) |
| V1-08 | GCS total | dihitung di komponen | **Tidak** | **Tidak** | Variabel `total` tidak pernah dirender maupun dikirim |
| V1-09 | Alat bantu jalan napas | select: OPA, NPA, ETT, LMA, STOMA | Ya | Ya (detail) | — |
| V1-10 | Alat bantu oksigenasi | select: O2 Kanule, Simple Mask, NRM, Head Box, JR/T-Pice, Ambu Bag, Ventilator | Ya | Ya (riwayat + detail) | Peta label detail memakai kunci lain (`o2_mask`, `o2_non_rebreather`) — sebagian label tidak cocok |
| V1-11 | Keterangan tambahan (header) | textarea | Ya | Ya | — |
| V1-12 | Waktu per baris pemantauan | date + time | Ya | Tidak (detail tidak menampilkan waktu baris) | Baris tanpa tanggal/jam **dibuang** |
| V1-13 | Obat/dosis per baris | teks, placeholder "Adrenalin 1mg" | **Tidak** | Tidak | Isian terikat `obatDosis`, payload mengirim `obatId` → selalu `null` (cacat V1) |
| V1-14 | Gambaran EKG per baris | teks | Ya | **Tidak** | — |
| V1-15 | DC Shock per baris | teks, "1x" | Ya | **Tidak** | — |
| V1-16 | TD per baris | teks "120/80" → systolic/diastolic | Ya | Ya | Kosong dikirim `0` |
| V1-17 | RR, suhu, SpO₂ per baris | angka | Ya | Ya | Kosong dikirim `0` — melanggar `IGD-DEC-056`/AC 17 (kosong ≠ nol) |
| V1-18 | Urine per baris | **teks** "Normal" → `parseInt` | Ya, jadi angka/0 | Ya | "Normal" tersimpan `0` |
| V1-19 | Muntah, perdarahan per baris | **teks** "Ya/Tidak" → `parseInt` | Ya, jadi 0 | **Tidak** | Makna hilang |
| V1-20 | Keterangan per baris | teks | Ya | Ya | — |
| V1-21 | "Gunakan Vital Sign Terbaru" | tombol | — | — | **Menyalin angka** vital sign skrining terbaru ke baris pertama — **duplikasi**, bukan tautan |
| V1-22 | Dokter & perawat | dari konteks halaman | Ya `dokterId/perawatId` | Tidak (hanya `createByName`) | — |
| V1-23 | `ats` | hardcode `0` | Ya | Tidak | Tidak bermakna |
| V1-24 | Riwayat: cari, sort, halaman, hapus | tabel + modal | — | — | Tombol Edit hanya `console.log`; Hapus memakai `confirm()` |

---

## C. Pemetaan V1 → blueprint

| Kemampuan V1 | Dasar di blueprint | Kekuatan dasar |
| --- | --- | --- |
| Tanda vital serial dalam observasi (V1-16, 17, 21) | Skenario dokumen sumber *"observasi berkala mereferensikan vital sign tanpa duplikasi"* (`00-interview-decisions.md` baris ±899); scope pass perjalanan pasien butir 5 *"pemantauan berkala: tanda vital serial, GCS, nyeri, respons pasien, perburukan kondisi"* (baris ±1757); `IGD-CAP-21` Ready to reuse | **Kuat** — dan menetapkan bentuknya: **referensi, bukan salinan** |
| GCS serial (V1-07, 08) | Scope butir 5 menyebut GCS eksplisit | **Kuat** untuk kemampuan; tempat penyimpanan tidak disebut |
| ABCDE (V1-02…06) | `IGD-DEC-054`, `IGD-DEC-057` (data inti pengkajian); `IGD-GAP-027` + PRD §3: bentuk terstruktur **ditunda**, ringkasan teks triase dianggap memenuhi `IGD-DEC-057` | Kuat untuk **pengkajian**; **tidak ada** keputusan bahwa ABCDE diulang pada setiap pemantauan |
| Evaluasi ulang setelah intervensi/perubahan kondisi | `IGD-DEC-060`, `IGD-DEC-083` (daftar pantau) | Kuat untuk **pemicu**; bentuk isi evaluasi tidak dikunci |
| Oksigenasi (V1-10) | Tidak disebut eksplisit; tercakup "tanda vital" pada `TrxPatientVitalSign` | Sedang |
| Alat bantu jalan napas (V1-09) | `IGD-DEC-096` pemakaian alat untuk pengendalian infeksi/keselamatan (**belum direncanakan**, manifest `belum_direncanakan`) | Lemah / belum diputuskan |
| Obat/dosis (V1-13) | PRD §3: catatan pemberian obat **ditunda** (`IGD-CAP-32`, `IGD-GAP-025`), pengganti *"pemberian dicatat pada catatan perkembangan"* | Kuat — sudah diputuskan **bukan** di Observasi |
| EKG (V1-14) | Tidak ada keputusan; penunjang/tindakan (`IGD-DEC-095`: IGD hanya memesan dan membaca) | Lemah |
| DC Shock (V1-15) | Resusitasi `IGD-CAP-28` Ready to reuse | Sedang — domain resusitasi |
| Keluaran cairan (V1-18, 19) | Tidak eksplisit; sudah ada di `EmgObservationDetail` (`IGD-CAP-26`) | Sedang — sudah terimplementasi |
| Periode, status, indikasi, rencana, kesimpulan | `IGD-CAP-26`, `IGD-DEC-115`/`119`/`121` | Kuat — V2 lebih kaya dari V1 |

---

## D. Cakupan backend V2

### `EmgObservation` (periode)

| Field | Ada | Dipakai frontend V2 |
| --- | :-: | --- |
| `Indication`, `ObservationPlan`, `ObservationLocation`, `StartedAt` | Ya | Ya — formulir *Buka Periode Observasi* |
| `EndedAt` | Ya | Ya — tampil; diisi backend saat status berubah |
| `CompletionSummary` | Ya | Ya — `FE-IGD-021` (baca), `FE-IGD-024` (tulis) |
| `EscalationReason` | Ya | Ya — aksi Eskalasi |
| `ResponsibleDoctorId`, `ResponsibleNurseUserId` | Ya | **Tidak** dikirim maupun ditampilkan |

### `EmgObservationDetail` (pemantauan)

| Field | Ada | Dipakai frontend V2 | Catatan |
| --- | :-: | --- | --- |
| `RecordedAt` | Ya | Ya | — |
| `RecordedByUserId` | Ya | Tidak dikirim (diisi token bila kosong); **tidak ditampilkan** | Backend **menerima** nilai dari klien bila tidak kosong — lihat temuan J-3 |
| `PatientVitalSignId` | Ya — FK `TrxPatientVitalSign`, index, `Restrict`; filter `GET ?patientVitalSignId=` | **Tidak** | Validasi hanya "ada"; **tidak** memeriksa pasien/encounter yang sama — temuan J-1 |
| `ProgressNoteId` | Ya — FK `TrxPatientIntegratedProgressNote` | Tidak | Tidak dibutuhkan V1; tersedia bila kelak dibutuhkan |
| `ClinicalConditionSummary`, `InterventionSummary`, `PatientResponseSummary` | Ya (2000) | Ya | — |
| `FluidIntakeMl`, `UrineOutputMl`, `OtherOutputMl`, `BleedingEstimatedMl`, `VomitEstimatedMl` | Ya (decimal) | Ya | — |
| `Notes` | Ya (1000) | Ya | — |

**API `emergency-observation-details`:** `GET /` (filter `emergencyObservationId`,
`patientVitalSignId`, `progressNoteId`, tanggal, paging), `GET /{id}`, `POST /`, `PUT /{id}`,
`DELETE /{id}` (soft). Response hanya membawa **ID** vital sign, bukan nilainya.

### Data yang ternyata sudah ada di domain lain

| Data | Tempat di V2 | Bukti |
| --- | --- | --- |
| TD, nadi, RR, suhu, SpO₂, MAP | `TrxPatientVitalSign` | `ClinicalManagement/Models/TrxPatientVitalSign.cs` |
| **GCS Eye/Verbal/Motor/Total**, `ConsciousnessStatus`, `NeurologicalNote` | `TrxPatientVitalSign` | idem |
| **Oksigenasi**: `IsUsingOxygen`, `OxygenSupportType` (None, NasalCannula, SimpleMask, NonRebreathingMask, VenturiMask, Other), `OxygenFlowRate`, `OxygenSupportNote` | `TrxPatientVitalSign` | idem + `Enums/OxygenSupportType.cs` |
| Nyeri, EWS, penanda kritis/notifikasi dokter | `TrxPatientVitalSign` | idem |
| API vital sign | `GET /` (filter `encounterId`, `patientId`…), `GET /options`, `GET /active-by-encounter/{id}`, `POST`, `PUT`, `PATCH verify/notify-doctor/cancel`; tanpa `DELETE` | `PatientVitalSignController.cs` |
| ABCDE + red flag (teks) | `EmgTriage.AirwaySummary` … `ExposureSummary`, `RedFlagSummary` | `EmgTriage.cs` |
| Manajemen jalan napas/napas/sirkulasi/neurologis saat resusitasi, jumlah defibrilasi, RJP, ROSC | `EmgResuscitation` | `EmgResuscitation.cs` |
| Obat darurat & tindakan dalam periode observasi | `EmgProcedureDetail` punya **`EmergencyObservationId`**, `DetailType` `EmergencyMedication`, `MedicationRoute`, `MedicationDateTime`, `EmergencySpecificResult` | `EmgProcedureDetail.cs`, `EmergencyProcedureDetailType.cs` |
| Alat bantu jalan napas terstruktur (OPA/NPA/ETT/LMA/stoma) | **Tidak ada** di model mana pun | Pencarian `Models/`/`Enums/` nol hasil |
| Gambaran EKG terstruktur | **Tidak ada** | idem |
| ABCDE pada `TrxPatientAssessment` | **Tidak ada** (hanya oksigen & kesadaran) | `TrxPatientAssessment.cs` |

---

## E. Cakupan frontend V2

| Bagian | Keadaan | Bukti |
| --- | --- | --- |
| Periode: buka, daftar, status, aksi Selesaikan/Eskalasi/Batalkan, kesimpulan | **Ada** | `emergency-assessment-observation-tab.jsx`; `FE-IGD-022`, `021`, `024` |
| Pemantauan: waktu, keadaan klinis, tindakan, respons, 5 angka cairan, catatan | **Ada** | idem |
| Riwayat pemantauan per periode | **Ada**, tanpa nama pelaku | idem |
| Tanda vital di dalam pemantauan | **Tidak ada**. Komentar tab: *"penautan itu belum punya jalur di layar — dicatat, bukan ditebak"* | idem baris komentar komponen |
| GCS, kesadaran, oksigen | **Ada, tetapi di tab Assesmen Awal** (`VitalSignTab`: GCS Mata/Verbal/Motorik, Kesadaran, Menggunakan oksigen, Jenis alat oksigen, Aliran, Catatan) | `emergency-assessment-initial-tab.jsx` → `VitalSignTab.jsx` |
| ABCDE | Hanya pada layar Triase (ringkasan teks) | triage view |
| Tindakan | **Baca saja** (`EmergencyAssessmentRecordTab`) | `emergency-assessment-detail-view.jsx` case `procedure` |
| Resusitasi | **Tidak ada layar** (`IGD-EV-111`, sengaja tanpa ID task) | — |
| Penanggung jawab periode | Tidak dikirim/ditampilkan | payload `createObservation` |

---

## F. Klasifikasi gap per kemampuan V1

Setiap baris memisahkan **nasib kemampuan bisnis** dari **nasib desain legacy**.

| Kemampuan V1 | Blueprint | V2 backend | V2 frontend | Status | Disposition | Kemampuan bisnis | Desain V1 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Waktu pemantauan | `IGD-CAP-26` | `RecordedAt` | Ada | Lengkap | `REUSE EXISTING` | KEEP | — |
| TD, RR, suhu, SpO₂ (+nadi) | Skenario "mereferensikan vital sign tanpa duplikasi"; scope butir 5 | `TrxPatientVitalSign` + FK `PatientVitalSignId` | **Tidak tersambung di Observasi** | Gap wiring | `LINK / REUSE` → `EXTEND OBSERVATION` (wiring) | **KEEP** | **DEPRECATE** salinan angka per baris & tombol "salin vital sign terbaru" |
| GCS E/M/V/total | Scope butir 5 | `TrxPatientVitalSign.GcsEye/Verbal/Motor/Total` | Ada di Assesmen Awal; tidak di Observasi | Gap wiring | `CLINICAL MANAGEMENT DOMAIN` + `LINK` | **KEEP** | **DEPRECATE** GCS teks bebas di header observasi |
| Kesadaran | Scope butir 5 (perburukan) | `ConsciousnessStatus` | Ada di Assesmen Awal | Gap wiring | `CLINICAL MANAGEMENT DOMAIN` + `LINK` | KEEP | — |
| Alat bantu oksigenasi | Tercakup tanda vital | `IsUsingOxygen`, `OxygenSupportType`, `OxygenFlowRate`, `OxygenSupportNote` | Ada di Assesmen Awal | Gap wiring; 4 pilihan V1 tidak ada di enum | `CLINICAL MANAGEMENT DOMAIN` + `LINK`; pilihan hilang `NEEDS DECISION` (`IGD-OQ-087`) | KEEP | DEPRECATE daftar pilihan V1 sebagai sumber enum |
| Alat bantu jalan napas (OPA/NPA/ETT/LMA/stoma) | `IGD-DEC-096` belum direncanakan | **Tidak ada** | Tidak ada | Gap desain | `NEEDS DECISION` (`IGD-OQ-086`) | KEEP (klinis relevan) | DEPRECATE kolom header V1 |
| ABCDE (A–D) | `IGD-DEC-057`; `IGD-GAP-027` ditunda | `EmgTriage.*Summary` (teks) | Hanya di Triase | Konflik interpretasi | `NEEDS DECISION` (`IGD-OQ-085`) | KEEP sebagai data pengkajian | DEPRECATE pengisian ulang ABCD di setiap header observasi sampai diputuskan |
| Exposure | `IGD-DEC-057` | `EmgTriage.ExposureSummary` | Di Triase | V2 lebih lengkap dari V1 | `REUSE EXISTING` (triase) | KEEP | — (V1 tidak punya isian) |
| Obat/dosis | PRD §3 ditunda; pengganti catatan perkembangan | `EmgProcedureDetail` `EmergencyMedication` + `EmergencyObservationId`; resep Farmasi | Tindakan baca saja | Sudah diputuskan di luar Observasi | `CLINICAL MANAGEMENT DOMAIN` (Farmasi/Tindakan); interim `InterventionSummary` | KEEP di domain Farmasi/Tindakan | **DEPRECATE** kolom obat per baris (di V1 pun tidak pernah terkirim) |
| Gambaran EKG | Tidak ada keputusan | Tidak terstruktur; tindakan EKG lewat `TrxPatientProcedure` | — | — | `LINK` ke Tindakan/Penunjang; interim teks `ClinicalConditionSummary` | KEEP sebagai temuan klinis teks | DEPRECATE kolom per baris |
| DC Shock | `IGD-CAP-28` | `EmgResuscitation.DefibrillationCount` | Layar resusitasi tidak ada | Domain lain | `RESUSCITATION DOMAIN` | KEEP di Resusitasi | **DEPRECATE** di Observasi — DC Shock menandakan perburukan → aksi **Eskalasi** |
| Urine, muntah, perdarahan, keluaran lain, cairan masuk | `IGD-CAP-26` | 5 kolom ml | Ada | Lengkap, lebih baik dari V1 | `REUSE EXISTING` | KEEP | DEPRECATE teks "Ya/Tidak" |
| Keterangan header & per baris | `IGD-CAP-26` | `Notes`, `ClinicalConditionSummary` | Ada | Lengkap | `REUSE EXISTING` | KEEP | — |
| Respons pasien | Scope butir 5 | `PatientResponseSummary` | Ada | Lengkap (V1 tidak punya) | `REUSE EXISTING` | KEEP | — |
| Dokter penanggung jawab | `EPIC IGD-04` (`BE-IGD-044/045`) | `EmgObservation.ResponsibleDoctorId` + riwayat penugasan (rencana) | Tidak ada | Menunggu `BE-IGD-045` | `LINK` ke Emergency Doctor Assignment | KEEP | DEPRECATE pengambilan dokter dari konteks halaman |
| Perawat pencatat | `IGD-DEC-057` AC 15 (identitas dari pengguna terautentikasi) | `RecordedByUserId` | Tidak ditampilkan | Tampilan kurang | `EXTEND OBSERVATION` (tampilkan pelaku) | KEEP | — |
| `ats` hardcode 0 | — | — | — | — | `DEPRECATE LEGACY` | — | DEPRECATE |
| Hapus riwayat observasi | `IGD-DEC-080` tanpa hapus permanen | Soft delete | Tidak ada tombol | — | `DEPRECATE LEGACY` (tidak dibawa) | — | DEPRECATE |

---

## G. Target workflow Observasi V2 — USULAN

Hanya memuat bagian yang didukung bukti. Bagian bertanda *(keputusan)* menunggu owner.

```text
PERIODE OBSERVASI                                    ← sudah ada
│
├── Indikasi, Rencana, Lokasi, Waktu mulai           ← sudah ada
├── Dokter penanggung jawab                          ← baca dari Emergency Doctor Assignment
│                                                       setelah BE-IGD-045 (bukan isian baru)
├── Ringkasan ABCDE triase terakhir (baca saja)      ← (keputusan IGD-OQ-085)
│
├── CATAT PEMANTAUAN                                 ← sudah ada, diperluas
│   ├── Waktu                                        ← sudah ada
│   ├── Tanda vital TERTAUT                          ← BARU: TrxPatientVitalSign
│   │     TD, nadi, RR, suhu, SpO2, GCS E/V/M,          → PatientVitalSignId
│   │     kesadaran, oksigen (jenis/aliran)             (keputusan IGD-OQ-084: catat baru / pilih)
│   ├── Keadaan klinis (termasuk evaluasi ABCDE teks) ← sudah ada
│   ├── Tindakan/intervensi (teks)                   ← sudah ada; obat terstruktur tetap
│   │                                                   di domain Farmasi/Tindakan
│   ├── Respons pasien                               ← sudah ada
│   ├── Keluaran & cairan (ml)                       ← sudah ada
│   └── Catatan                                      ← sudah ada
│
├── RIWAYAT PEMANTAUAN                               ← sudah ada, diperluas:
│     menampilkan angka tanda vital tertaut + nama pelaku
│
├── ESKALASI → Alasan (→ Resusitasi bila perlu)      ← sudah ada; DC Shock/RJP milik Resusitasi
│
└── SELESAIKAN → Kesimpulan (opsional, ≤1000)        ← FE-IGD-024 / BE-IGD-040, tidak berubah
```

**Aturan data yang dipegang usulan ini:**

1. Angka tanda vital **tidak pernah** disimpan di tabel Emergency Installation; yang disimpan hanya
   `PatientVitalSignId`.
2. Vital sign yang ditautkan **wajib** milik pasien dan encounter yang sama dengan kunjungan IGD
   periode itu.
3. Kosong berarti tidak diukur, bukan nol (`IGD-DEC-056`, AC 17) — perilaku yang dilanggar V1.

---

## H. Dampak terhadap `FE-IGD-022`

**Rekomendasi: Pilihan B — `FE-IGD-022` tetap baseline dengan status 🟡 miliknya sendiri; kekurangan
pemantauan dikerjakan sebagai task lanjutan baru.**

Alasan berbasis bukti:

1. Kartu `FE-IGD-022` (roadmap R3.5) **tidak memuat acceptance criteria bernomor** — hanya uraian
   tiga bagian perubahan. Tidak ada kriteria penautan tanda vital yang "gagal".
2. Laporannya menyatakan penautan `PatientVitalSignId` **sengaja di luar lingkup**: *"jalur
   penautannya belum ada di layar — dicatat, bukan ditebak"* (bagian 5.2).
3. Status 🟡-nya punya alasan sendiri yang tidak berkaitan: uji layar belum pernah dijalankan; dua
   alasan lain (tab lab, teks radiologi) sudah ditangani `FE-IGD-023`.
4. Pekerjaan baru menuntut **prasyarat yang tidak pernah dimiliki `FE-IGD-022`**: validasi backend
   tautan vital sign (J-1) dan keputusan owner `IGD-OQ-084`/`085`. Memasukkannya ke kartu lama
   berarti mengubah lingkup task yang laporannya sudah ada, dan status 🟡 `FE-IGD-022` akan
   tertahan oleh keputusan yang lahir sesudahnya.

Pilihan A (memperjelas acceptance `FE-IGD-022`) ditolak karena poin 1–4. **Tidak ada ID baru
yang dialokasikan pada audit ini**; ID di bagian L hanyalah usulan.

---

## I. Dampak terhadap `FE-IGD-024`

**Kompatibel: YA.**

`FE-IGD-024` tetap `IMPLEMENTATION COMPLETE`; kemampuan penyelesaian tidak terpengaruh audit detail
pemantauan. Mekanisme *Selesaikan → Kesimpulan → `Completed`* bekerja pada `EmgObservation`
(periode), sedangkan seluruh gap audit ini berada pada `EmgObservationDetail` (pemantauan) dan
tautan ke `TrxPatientVitalSign`. Tidak ada field, endpoint, atau komponen yang sama. Yang
dipertahankan dari `FE-IGD-024`: penanda `collectsConclusion`, isian Kesimpulan opsional ≤1000,
pesan backend `400`/`409` di modal, dan Batalkan tanpa isian (`IGD-OQ-083`).

---

## J. Dampak backend

| Pertanyaan | Jawaban |
| --- | --- |
| Data-model change / migration | **Tidak** untuk inti usulan (FK sudah ada). Menjadi **ya** hanya bila owner memilih `IGD-OQ-085` B atau `IGD-OQ-086` C |
| Endpoint baru | **Tidak** |
| Hanya wiring frontend | **Cukup untuk menampilkan**, tetapi **tidak aman** tanpa J-1 |
| Perubahan validasi | **Ya — J-1 wajib** sebelum frontend menautkan |
| DTO change | **Direkomendasikan, aditif**: ringkasan tanda vital tertaut pada `EmergencyObservationDetailResponse`, supaya riwayat tidak perlu N+1 permintaan atau menggabungkan daftar vital sign berhalaman di browser |
| Kontrak | Aturan J-1 dan response aditif **belum ada** di `contracts/validation-matrix.md` maupun `api-contract.md` → perlu pass `design-business-module` (kenaikan versi) sebelum task backend |

**Temuan integritas backend** (dicatat, tidak diperbaiki):

| ID | Temuan | Risiko | Usulan |
| --- | --- | --- | --- |
| J-1 | `EmergencyObservationDetailController.ValidateRequestAsync` hanya memeriksa `PatientVitalSignId` **ada**, tidak memeriksa vital sign itu milik pasien/encounter kunjungan periode yang sama | Tanda vital pasien lain dapat ditautkan ke observasi pasien ini — keselamatan & privasi | Masuk task backend usulan |
| J-2 | Detail pemantauan dapat dibuat pada periode `Completed`/`Cancelled` | Pemantauan "setelah selesai" tanpa penanda susulan | `IGD-OQ-088` |
| J-3 | `RecordedByUserId` dari klien dipakai bila tidak kosong | Pelaku dapat dipalsukan; bertentangan dengan semangat `IGD-DEC-057` AC 15 | Masuk task backend usulan: selalu dari token |
| J-4 | `PUT /{id}` menimpa detail di tempat | Bertentangan dengan `IGD-DEC-080` (catatan klinis tidak diubah di tempat). Frontend tidak memakainya | Dicatat; di luar usulan ini |
| J-5 | `CreatePatientVitalSignRequest.PatientId` wajib, sedangkan `EmgVisit.PatientId` boleh kosong untuk pasien tanpa identitas yang masih provisional | Tanda vital — termasuk GCS — mungkin tidak dapat dicatat untuk pasien tanpa identitas provisional; bertentangan dengan `IGD-DEC-057` AC 16 | **UNKNOWN** — perlu uji runtime; berlaku juga untuk Assesmen Awal |

---

## K. Keputusan yang dibutuhkan dari owner

Hanya yang tidak dapat dijawab dari bukti.

### `IGD-OQ-084` — Cara tanda vital masuk ke pemantauan

| Pilihan | Dampak |
| --- | --- |
| A. Catat baru dari formulir pemantauan (memakai `VitalSignTab`), lalu otomatis ditautkan | Satu layar untuk perawat; dua permintaan tidak atomik — bila detail gagal, vital sign tetap tersimpan sebagai pengukuran sah |
| B. Pilih vital sign yang sudah dicatat di tab Assesmen Awal | Tanpa duplikasi input baru; perawat berpindah tab setiap 15–30 menit |
| C. **A sebagai bawaan, B sebagai pilihan** | Paling fleksibel; UI sedikit lebih rumit |

**Rekomendasi C.** Pemantauan berkala adalah saat pengukuran terjadi (A), tetapi pengukuran yang
baru dicatat di tab lain tidak boleh diketik ulang (B).

### `IGD-OQ-085` — ABCDE selama observasi

| Pilihan | Dampak |
| --- | --- |
| A. Observasi **menampilkan** ABCDE triase terakhir (baca saja); evaluasi ulang ditulis pada *Keadaan Klinis* | Tanpa perubahan backend; konsisten dengan PRD §3 dan `IGD-GAP-027` |
| B. ABCDE terstruktur pada setiap pemantauan | Migration + DTO; membuka ulang penundaan `IGD-GAP-027`; berisiko duplikasi dengan triase |
| C. Evaluasi ABCDE hanya lewat **retriase** | Tanpa perubahan; retriase membuat penilaian baru — bisa berlebihan untuk evaluasi rutin |

**Rekomendasi A.** Dokumen sudah memutuskan ringkasan teks triase memenuhi `IGD-DEC-057` untuk MVP;
perubahan besar pada ABCDE menunggu `IGD-GAP-027` dibuka resmi.

### `IGD-OQ-086` — Alat bantu jalan napas (OPA/NPA/ETT/LMA/stoma)

| Pilihan | Dampak |
| --- | --- |
| A. Teks pada *Keadaan Klinis*/*Tindakan* sementara | Tanpa perubahan; tidak terstruktur, tidak dapat dihitung untuk pengendalian infeksi |
| B. **Domain Pemakaian Alat** (`IGD-DEC-096`), direncanakan saat area itu dibuka; A sebagai pengganti sementara | Satu sumber untuk ETT/ventilator yang juga dibutuhkan `TrxNosocomialInfection` |
| C. Kolom baru pada `EmgObservationDetail` | Migration; menduplikasi data yang kelak dimiliki Pemakaian Alat |

**Rekomendasi B.** ETT, stoma, dan ventilator adalah alat yang dipantau untuk infeksi — persis
tujuan `IGD-DEC-096`.

### `IGD-OQ-087` — Jenis oksigen V1 yang tidak ada di enum V2 (Head Box, JR/T-Piece, Ambu Bag, Ventilator)

| Pilihan | Dampak |
| --- | --- |
| A. Pakai `Other` + *Catatan Oksigen* | Tanpa perubahan; tidak terstruktur |
| B. Minta pemilik `ClinicalManagement` menambah nilai enum `OxygenSupportType` | Terstruktur; berdampak ke seluruh modul pemakai vital sign (rawat jalan, rawat inap). Pemiliknya belum ditunjuk — sementara `IGD-DEC-107` |

**Rekomendasi A sekarang, B diajukan** ke pemilik `ClinicalManagement`. Tidak memblokir apa pun.

### `IGD-OQ-088` — Pemantauan pada periode yang sudah ditutup

| Pilihan | Dampak |
| --- | --- |
| A. Ditolak `409` untuk periode `Completed`/`Cancelled` | Integritas terjaga; entri susulan setelah penutupan tidak mungkin |
| B. Diizinkan bebas (perilaku sekarang) | Tanpa perubahan; pemantauan dapat tercatat setelah kesimpulan |
| C. Diizinkan hanya sebagai entri susulan beralasan | Pola `IGD-DEC-065`; butuh kolom penanda → migration |

**Rekomendasi A**, dengan waktu `RecordedAt` tetap boleh mundur selama periode masih `Active`/
`Escalated` (entri tertunda ditulis sebelum menutup periode).

---

## L. Usulan task delivery — BELUM MASUK ROADMAP

Register ID diperiksa pada seluruh `docs/module-blueprints/igd/**` (termasuk arsip): tertinggi
`BE-IGD-045`, `FE-IGD-027`, `IGD-OQ-083`, `IGD-EV-123`. ID di bawah **usulan**, dialokasikan resmi
hanya setelah keputusan K dijawab dan kontrak diperbarui.

| Urutan | Pekerjaan | Jenis | Prasyarat |
| ---: | --- | --- | --- |
| 0 | Jawaban owner `IGD-OQ-084`, `085`, `088` (`086`/`087` tidak memblokir) | Keputusan | — |
| 1 | Pass `design-business-module`: validation-matrix bagian baru untuk detail pemantauan (J-1, J-3, `IGD-OQ-088`); api-contract aditif ringkasan tanda vital pada response detail | Desain | 0 |
| 2 | **`BE-IGD-046` (usulan)** — Tautan tanda vital pada pemantauan observasi aman: tolak vital sign beda pasien/encounter; pelaku selalu dari token; perlakuan periode tertutup sesuai `IGD-OQ-088`; response aditif ringkasan tanda vital. Tanpa migration | Backend | 1 |
| 3 | **`FE-IGD-028` (usulan)** — Catat Pemantauan dengan tanda vital tertaut (sesuai `IGD-OQ-084`), riwayat menampilkan TD/nadi/RR/suhu/SpO₂/GCS/kesadaran/oksigen dan nama pelaku, ringkasan ABCDE triase terakhir baca saja (bila `IGD-OQ-085` A). Tidak mengubah `FE-IGD-024` | Frontend | 2 |
| — | Verifikasi runtime J-5 (pasien tanpa identitas provisional) | Verifikasi owner | Dapat paralel |
| Nanti | Alat bantu jalan napas → area Pemakaian Alat (`IGD-OQ-086` B) | Perencanaan area | Area `belum_direncanakan` dibuka owner |
| Nanti | Dokter penanggung jawab periode → baca dari `BE-IGD-045` | Frontend | `BE-IGD-045`, `FE-IGD-027` |

Yang **tidak** diusulkan: tabel vital sign kedua, kolom GCS/oksigen/EKG/DC Shock/obat pada
`EmgObservationDetail`, tombol salin vital sign, layar resusitasi (gap tanpa ID atas instruksi owner),
dan catatan alasan pembatalan (`IGD-OQ-083`).

---

## M. Penutupan audit — 16 September 2026

Audit ini **ditutup**. Kelima pertanyaan pada bagian K dijawab Product/Domain Owner pada
16 September 2026 dan menjadi keputusan; usulan task pada bagian L dialokasikan resmi.

| Pertanyaan bagian K | Jawaban owner | Keputusan |
| --- | --- | --- |
| `IGD-OQ-084` — cara tanda vital masuk | Keduanya; **catat baru** sebagai alur bawaan, pilih yang sudah ada sebagai pilihan. Hanya pasien dan encounter yang sama | `IGD-DEC-122` |
| `IGD-OQ-085` — ABCDE | Ditampilkan baca saja; evaluasi ditulis pada `ClinicalConditionSummary`; retriase untuk penilaian ulang | `IGD-DEC-123` |
| `IGD-OQ-086` — alat bantu jalan napas | **Bukan** otomatis milik Pemakaian Alat; teks sementara, bentuk terstruktur tetap terbuka | `IGD-DEC-124` + `IGD-OQ-089` |
| `IGD-OQ-087` — jenis oksigen | `Other` + catatan; usul penambahan enum dirutekan ke `ClinicalManagement` | `IGD-DEC-125` |
| `IGD-OQ-088` — periode tertutup | `409` untuk pembuatan detail normal; **bukan** larangan permanen atas dokumentasi susulan | `IGD-DEC-126` + `IGD-OQ-090` |

**Yang berubah sesudah audit:**

| Artefak | Perubahan |
| --- | --- |
| `contracts/api-contract.md` | `0.5.0` → `0.6.0`; bagian 7 baru untuk `Emergency Observation Detail`; bagian 5 tidak lagi memuat grup itu |
| `contracts/validation-matrix.md` | `0.5.0` → `0.6.0`; bagian 9 baru beserta urutan pemeriksaan 9.1 |
| `02-backend-architecture.md` | Bagian 12 — kepemilikan data, relasi validasi, kelas yang berubah, tanpa migration, urutan pemanggilan |
| `03-frontend-architecture.md` | Bagian 12 — peta butir menu, skema wilayah, sumber data per wilayah, sembilan aturan layar |
| `roadmap/backend-roadmap.md` | Gelombang R3.9 beserta kartu `BE-IGD-046` |
| `roadmap/frontend-roadmap.md` | Gelombang R3.7 beserta kartu `FE-IGD-028` |
| `roadmap/requirement-traceability.md` | Bagian R3.5 |
| `00-interview-decisions.md` | `IGD-DEC-122`…`126`; `IGD-OQ-089`, `IGD-OQ-090` |
| `MODULE-STATUS.md` | Baris audit ditutup; dua blocker baru yang tidak menahan; urutan pekerjaan berikutnya |

**Yang tidak berubah:** `FE-IGD-024` tetap `IMPLEMENTATION COMPLETE`, `BE-IGD-040` tetap seperti
adanya, dan `FE-IGD-022` tetap 🟡 sebagai baseline — `FE-IGD-028` adalah follow-up, bukan
penggantinya (bagian H).

**Yang masih terbuka sesudah penutupan ini:** `IGD-OQ-089` (bentuk terstruktur alat jalan napas),
`IGD-OQ-090` (entri susulan setelah periode ditutup), dan verifikasi runtime J-5 (tanda vital
untuk pasien tanpa identitas provisional). Tidak satu pun menahan `BE-IGD-046` maupun
`FE-IGD-028`.
