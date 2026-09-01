# FE-IGD-022 — Layar asuhan keperawatan IGD: pengkajian lanjutan, pemantauan observasi, dan penunjang medis

- **TASK ID:** `FE-IGD-022`
- **TASK TYPE:** Implementasi frontend, vertical slice tiga bagian pada satu layar
- **COMPLEXITY:** Menengah
- **CLASSIFICATION SCORE:** Menengah — tiga bagian layar, satu slice Redux, nol endpoint baru di backend
- **MODEL:** Claude Opus 5
- **TASK MODE:** `FRONTEND MODE`
- **WRITE TARGET:** `QuilvianSystemFrontendDev` (branch `RizkiV2`, upstream `origin/RizkiV2`), ditambah laporan tracked ini di `NewQuilvianSystemBackend` (branch `rizkiG`)

---

## 1. Latar dan batas

Permintaan owner: meninjau modul IGD bagian **pengkajian** dan **transfer pasien**, lalu —
bila backend-nya sudah selesai — mengerjakan frontend asuhan keperawatan IGD berisi assesmen
awal (tanda vital, pengkajian nyeri, pemeriksaan pernapasan, pemeriksaan awal lainnya),
observasi, nosokomial, dan penunjang, dengan data yang disesuaikan pada parameter backend
`EmergencyInstallationManagement`.

**Hasil tinjauan backend: kedua bagian sudah selesai.** Rinciannya di bagian 2. Karena itu
pekerjaan frontend dijalankan, dengan batas berikut:

- **Transfer pasien hanya ditinjau, tidak diubah.** Permintaan implementasi tertuju pada layar
  pengkajian. Satu celah nyata ditemukan pada transfer dan dilaporkan di bagian 6, bukan
  ditambal diam-diam di luar cakupan yang diminta.
- **Nosokomial tidak diubah.** Ditinjau kolom per kolom terhadap
  `CreateNosocomialInfectionRequest` dan sudah lengkap; menyentuhnya berarti perubahan tanpa
  sebab.
- **Nol perubahan backend.** Empat cacat backend yang ditemukan dicatat di bagian 6.

---

## 2. Tinjauan backend — pengkajian dan transfer pasien

Diperiksa langsung pada source, bukan pada dokumen.

### 2.1 Pengkajian — selesai

| Kemampuan | Bukti di source | Keadaan |
| --- | --- | --- |
| Pengkajian pasien | `PatientAssessmentController`, `TrxPatientAssessment` — `QueueId` sudah `Guid?` | **Selesai** (`BE-IGD-026`, `BE-IGD-027`) |
| Tanda vital, diagnosis, tindakan, CPPT | Keempatnya encounter-only | **Selesai** (`BE-IGD-030`) |
| Infeksi nosokomial | `NosocomialInfectionController` + DTO lengkap | **Selesai** |
| Periode observasi | `EmergencyObservationController` — 5 route termasuk `PATCH .../observation-status` | **Selesai** |
| Pemantauan berkala | `EmergencyObservationDetailController` — CRUD penuh | **Selesai** |
| Resusitasi, tindakan, tindak lanjut | `EmergencyResuscitationController`, `EmergencyProcedureDetailController`, `EmergencyDispositionController` | **Selesai** |

Jalur simpan pengkajian dengan `queueId = null` sudah dibuktikan pada `BE-IGD-036`
(`ASM-20260827-00003`).

### 2.2 Transfer pasien — selesai

`EmergencyDepartureController` memuat 14 route: pembuatan, `submit-handover`,
`accept-handover`, `reject-handover`, `depart`, `arrive`, `cancel`, `events/{id}/amend`,
`events/{id}/reverse`, dan lima route `order-items`. Dua rangkaian status
(`PhysicalStatus`, `HandoverStatus`) beserta `EmgDepartureEvent` yang tambah-saja sudah ada.
Sesuai `BE-IGD-031`…`035` yang berstatus selesai.

### 2.3 Penunjang medis — sebagian, dan lebih maju dari catatan sebelumnya

Catatan modul sebelumnya menyebut `LabOrder` *"nol status, nol hasil, nol spesimen"*. Yang
berlaku sekarang **berbeda**: `LabOrderStatus` punya 8 nilai, `LabSpecimenStatus` punya 8
nilai, ada `TrxLabSpecimen`, `TrxLabTransitionHistory`, `MstLabRejectionReason`, dan
`LabSpecimenController`. Yang **masih** nol adalah **hasil pemeriksaan** — tidak ada satu pun
kolom hasil pada `LabOrderListResponse` maupun `LabOrderDetailResponse`.

`RadiologyManagement` tetap **nol berkas**, sesuai `IGD-DEC-099`.

---

## 3. FILES INSPECTED

**Backend (read-only, source of truth kontrak API):**

- `Areas/HealthServices/EmergencyInstallationManagement/Controllers/` — `EmergencyObservationController.cs`, `EmergencyObservationDetailController.cs`, `EmergencyDepartureController.cs`
- `Areas/HealthServices/EmergencyInstallationManagement/DTOs/` — `EmergencyObservationDtos.cs`, `EmergencyObservationDetailDtos.cs`, `EmergencyDepartureDtos.cs`, `EmergencyProcedureDetailDtos.cs`, `EmergencyResuscitationDtos.cs`
- `Areas/HealthServices/EmergencyInstallationManagement/Enums/EmergencyObservationStatus.cs`
- `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyObservationService.cs` — `CanTransition`
- `Areas/HealthServices/ClinicalManagement/DTOs/PatientAssessmentDtos.cs`, `NosocomialInfectionDtos.cs`
- `Areas/HealthServices/ClinicalManagement/Enums/` — `FallRiskStatus`, `FunctionalStatus`, `NutritionRiskStatus`, `AppetiteStatus`, `OxygenSupportType`, `ConsciousnessStatus`
- `Areas/HealthServices/LaboratoryManagement/` — `Controllers/LabOrderController.cs`, `DTOs/LabOrderDtos.cs`, `Enums/LaboratoryEnums.cs`, `Services/LabOrderService.cs`
- `Areas/HealthServices/MasterData/Controllers/ProcedureController.cs`, `DTOs/ProcedureDtos.cs`

**Frontend:**

- `emergency-assessment-view/` — seluruh 11 komponen, `emergency-assessment-detail-view.jsx`, `emergency-assessment-list-view.jsx`
- `nurse-station-management/VitalSignTab.jsx`, `AssessmentTab.jsx`
- `utils/health-services/clinical-management/patient-assessment-payload.utils.js`
- `constants/.../nurse-station-queue.constants.js`, `nurse-screening-options.constants.js`
- `utils/.../screening-validation.utils.js` — `getFallRiskSummary`

---

## 4. FILES CHANGED

| File | Perubahan |
| --- | --- |
| `src/lib/state/slice/health-services/emergency-installation-management/emergency-assessment-slice.jsx` | +137. Tiga URL baru; thunk `fetchObservationDetails`, `createObservationDetail`, `updateObservationStatus`, `fetchLabOrders`, `createLabOrder`, `fetchLabProcedureOptions`; dua bagian state baru; reducer dipasang lewat helper yang sudah ada |
| `src/lib/constants/health-services/emergency-installation-management/emergency-assessment-constant.jsx` | +179/−4. Nav `diagnostic-support` menjadi `available: true`; `OBSERVATION_STATUS` beserta label/varian/aksi; `OBSERVATION_STATUS_OPTIONS` **dikoreksi**; `FALL_RISK_STATUS_OPTIONS`, `FUNCTIONAL_STATUS_OPTIONS`, `LAB_ORDER_STATUS_LABELS`, `LAB_ORDER_STATUS_VARIANTS` |
| `.../components/emergency-assessment-initial-tab.jsx` | +105/−1. Kelompok "Pengkajian Lanjutan IGD" berisi delapan kolom yang selama ini tidak pernah punya isian |
| `.../components/emergency-assessment-observation-tab.jsx` | +554/−22. Ditulis ulang: periode + aksi status + pemantauan berkala per periode |
| `.../components/emergency-assessment-diagnostic-support-tab.jsx` | **Baru**, 211 baris. Pemesanan laboratorium dan pemantauan statusnya |
| `.../emergency-assessment-detail-view.jsx` | +14. Menyambungkan tab penunjang; meneruskan `observationDetails`; satu baris ringkasan kanan |
| `src/lib/hooks/.../use-emergency-assessment-detail.jsx` | +14/−2. Loader nav `diagnostic-support`; pemuatan master `labProcedures`; dua bagian state diekspor |
| `src/style/.../emergency-assessment.module.css` | +11. Satu aturan `.recordItem[data-active="true"]` |

Nol komponen bersama diubah. Nol modul CSS baru. Nol Axios instance baru.

---

## 5. IMPLEMENTATION

### 5.1 Assesmen Awal IGD — delapan kolom yang selama ini hilang tanpa suara

`VitalSignTab` dan `AssessmentTab` tetap dipakai **apa adanya**, sesuai arahan berulang owner
untuk memakai ulang formulir bersama. Tetapi perbandingan kolom layar terhadap
`CreatePatientAssessmentRequest` menemukan delapan kolom yang **dikirim
`buildPatientAssessmentPayload` tetapi tidak pernah punya isian di layar**, sehingga selalu
tersimpan sebagai nilai bawaan atau `null`:

`painNote`, `fallRiskStatus`, `fallRiskScore`, `nutritionRiskScore`, `functionalStatus`,
`functionalNote`, `psychosocialNote`, `educationNote`.

`fallRiskStatus` yang paling menyesatkan: `AssessmentTab` **menampilkan** ringkasan risiko
jatuh hasil `getFallRiskSummary`, tetapi ringkasan itu tidak pernah dituliskan ke form —
sehingga setiap pengkajian tersimpan sebagai `NoRisk` apa pun yang terbaca di layar.

Kedelapan kolom ditambahkan sebagai kelompok **"Pengkajian Lanjutan IGD"** di dalam tab IGD,
**bukan** di dalam `AssessmentTab`. Alasannya kepemilikan: komponen itu juga dipakai skrining
antrean perawat rawat jalan, dan menambah isian di sana berarti mengubah layar modul lain
tanpa permintaan pemiliknya.

Instrumen skor risiko jatuh dan risiko nutrisi **tidak** ditetapkan layar — backend hanya
menyimpan angka, dan alat ukur mana yang berlaku adalah kebijakan unit.

### 5.2 Observasi — pemantauan berkala yang backend punya tetapi layar tidak

Sebelumnya tab observasi hanya dapat **membuka** periode. `EmergencyObservationDetail` —
tabel yang menyimpan isi pemantauannya — punya CRUD penuh di backend dan **nol pemakai di
frontend**.

Ditambahkan:

1. **Aksi status periode** mengikuti `EmergencyObservationService.CanTransition`:
   `Active → Completed | Escalated | Cancelled`, `Escalated → Completed | Cancelled`.
2. **Pemantauan berkala per periode**: waktu, keadaan klinis, tindakan, respons pasien, dan
   lima angka keseimbangan cairan (`fluidIntakeMl`, `urineOutputMl`, `otherOutputMl`,
   `bleedingEstimatedMl`, `vomitEstimatedMl`).
3. **Riwayat pemantauan** per periode, terbaru lebih dulu.

Periode yang dipantau dipilih otomatis — yang berjalan lebih dulu — karena pencatatan ini
berulang tiap 15–30 menit dan satu langkah pemilihan pada pekerjaan sebanyak itu tidak murah.

`recordedByUserId` sengaja tidak dikirim: controller mengisinya dari token ketika
`Guid.Empty`.

Angka tanda vital tetap di tab Tanda Vital. `EmgObservationDetail.PatientVitalSignId`
memang menyediakan penautan, tetapi jalur penautannya belum ada di layar — dicatat, bukan
ditebak.

### 5.3 Koreksi cacat: `OBSERVATION_STATUS_OPTIONS` memetakan nilai yang salah

Daftar lama:

```
{ value: "1", label: "Aktif" }
{ value: "2", label: "Selesai" }
{ value: "3", label: "Dibatalkan" }   ← salah
```

Enum backend `EmergencyObservationStatus` adalah
`Active = 1, Completed = 2, Escalated = 3, Cancelled = 4`.

Akibatnya perawat yang membuka periode dengan pilihan **"Dibatalkan"** sebenarnya membuat
periode ber-status **`Escalated`** — dan `Escalated` memindahkan status kunjungan ke
`InTreatment`. Kebalikan dari yang diniatkan.

Diperbaiki dengan mempersempit daftar pembukaan menjadi `Aktif` saja: membuka periode yang
langsung selesai, tereskalasi, atau batal tidak mencatat apa pun yang dipantau. Ketiga status
lain dicapai lewat aksi pada periode yang sudah ada.

### 5.4 Penunjang Medis — tab baru, batas kewenangan sempit

Sesuai `IGD-DEC-105`, **IGD hanya memesan dan membaca**. Layar menyediakan pemesanan
laboratorium (`POST /laboratory-management/lab-orders`) dan pembacaan status; **nol tombol**
yang menyentuh alur di dalam laboratorium walau endpoint-nya (`start-process`, `complete`,
`hold`, `resume`, `cancel`) tersedia.

Pilihan pemeriksaan disaring `isLaboratory=true` karena `LabOrderService.CreateAsync` menolak
procedure lain dengan `400`.

Dua keterbatasan **ditulis apa adanya** kepada perawat sebagai keterangan tetap, bukan
disembunyikan sebagai bagian kosong:

1. **Hasil pemeriksaan belum dapat ditampilkan** — respons pesanan hanya memuat status dan
   jumlah spesimen.
2. **Radiologi belum dapat dipesan** — `RadiologyManagement` nol berkas; permintaannya
   ditempuh di luar sistem dan dicatat pada serah terima (`IGD-DEC-099`).

---

## 6. API CONTRACT IMPACT dan temuan backend

**Nol perubahan kontrak.** Seluruh endpoint yang dipakai sudah ada. Empat temuan backend
dilaporkan **tanpa diperbaiki**, sesuai batas keselamatan lintas repository:

| # | Temuan | Bukti | Akibat |
| ---: | --- | --- | --- |
| 1 | `PATCH .../observation-status` **membuang** `notes` untuk target `Completed` dan `Cancelled` | Cabang refleksi `entity.GetType().GetProperty("Notes")` — `EmgObservation` **tidak punya** properti `Notes`. Hanya `Escalated` yang menyimpannya, ke `EscalationReason` | `CompletionSummary` **tidak punya jalan tulis** dari layar; satu-satunya jalur adalah `PUT /{id}` yang menimpa seluruh periode. Layar karena itu **tidak meminta** alasan pada `Completed`/`Cancelled` — meminta teks yang dibuang adalah cacat yang sama dengan `FE-IGD-021` |
| 2 | `GET /laboratory-management/lab-orders` **nol parameter** — tanpa `encounterId`, tanpa paging | `LabOrderService.GetListAsync(CancellationToken)` mengembalikan seluruh isi tabel | Penyaringan per pasien terpaksa dikerjakan di frontend. Begitu tabelnya membesar, layar menarik seluruh tabel untuk menampilkan beberapa baris. Perbaikannya milik pemilik `LaboratoryManagement` |
| 3 | Sikap pesanan serah terima (`order-items`) **nol pemakai di frontend** | Lima route ada di `EmergencyDepartureController`; nol referensi di `src/` | `BE-IGD-035` menahan penutupan kunjungan bila ada pesanan tanpa sikap, tetapi **tidak ada layar** untuk menetapkan sikap itu. Kunjungan dapat tertahan tanpa jalan keluar lewat antarmuka |
| 4 | `EmergencyResuscitationController` **nol pemakai di frontend** | Controller + DTO lengkap; nol referensi di `src/` | Resusitasi IGD tidak dapat dicatat lewat layar |

Temuan 3 dan 4 berada di luar cakupan yang diminta owner pada task ini.

**Cacat frontend di luar cakupan, dilaporkan tanpa diperbaiki:**
`CONSCIOUSNESS_OPTIONS` pada `emergency-assessment-constant.jsx` memuat 7 nilai (0–6, dengan
`Soporocoma`) sedangkan enum backend hanya 0–5. Konstanta itu **tidak dipakai siapa pun** —
`VitalSignTab` memakai versi yang benar dari `nurse-screening-options.constants.js` — jadi
dampaknya nol saat ini, tetapi ia jebakan bagi yang mengimpornya kelak.

---

## 7. DATABASE IMPACT

**Nol.** Task frontend; nol migration, nol perintah tulis ke basis data bersama.

## 8. SECURITY IMPACT

Nol. Tidak ada endpoint, header, atau penanganan token yang berubah — seluruh permintaan lewat
`InstanceAxios` yang sudah ada. Nol nilai environment, secret, atau credential dalam diff
maupun laporan. Keadaan `forbidden` (403) ditangani bagian layar yang sudah ada.

## 9. VISUAL REFERENCE: NOT REQUIRED

Seluruh tampilan diturunkan dari referensi Quilvian yang sudah mapan: `EmergencyAssessmentFormCard`,
`EmergencyAssessmentFormSection`, `EmergencyAssessmentSection`, `ConfirmModal`, primitif
`form-pemeriksaan-ui`, dan `Badge` react-bootstrap — sama dengan tab tindak lanjut dan transfer.

## 10. VALIDATION

| Command / pemeriksaan | Hasil | Klasifikasi | Catatan |
| --- | --- | --- | --- |
| `npm run lint:errors` | Keluaran kosong, exit 0 | **PASS** | Nol error |
| `node --import ./tests/helpers/register.mjs --test tests/unit` | `# tests 119`, `# pass 119`, `# fail 0` | **PASS** | Bentuk perintah yang benar-benar menjalankan test |
| `npm run test:unit` | `Could not find '…\tests\unit\**\*.test.mjs'`, exit 1 | **EXISTING / ENVIRONMENT ISSUE** | Node v20.20.2 belum mendukung glob pada `--test`; masuk pada Node 21. **Nol test berjalan.** Cacat script `package.json`, ada sebelum task ini, dan diff task ini tidak menyentuh `package.json` maupun `tests/` |
| `npm run build` | `Compiled successfully`, `postbuild` selesai | **PASS** | Route `emergency-assessment` dan `[slug]` ikut ter-build |
| Tinjauan diff | 985 tambah / 29 hapus pada 7 berkas + 1 berkas baru | **PASS** | Seluruh 29 baris terhapus ditelusuri satu per satu dan disengaja |
| Line ending / BOM | LF, nol BOM — sama dengan `HEAD` | **PASS** | Nol perubahan sampingan |

## 11. WARNINGS

`npm run lint` penuh (bukan `lint:errors`) tetap memuat ratusan warning lama di seluruh
repository. Tidak diperiksa ulang dan tidak diperbaiki — di luar cakupan.

## 12. KNOWN ISSUES

- `completionSummary` periode observasi belum dapat diisi dari layar (temuan backend 1).
- Daftar pesanan laboratorium disaring di sisi klien (temuan backend 2).
- Riwayat Assesmen Awal menampilkan kolom dari `PatientAssessmentResponse` saja. Enam dari
  delapan kolom baru hanya ada di `PatientAssessmentDetailResponse`, sehingga tidak
  ditampilkan pada daftar — sengaja, supaya tidak ada kolom yang kosong selamanya.

## 13. MANUAL TEST: NOT FEASIBLE

Tiga penghalang konkret, seluruhnya di luar kendali task ini:

1. **Backend tidak berjalan.** Probe ke `http://localhost:5107` gagal tersambung.
2. **Nol kredensial petugas.** Layar berada di balik autentikasi JWT; utang ini berlaku
   sejak roadmap revision `1`.
3. **Basis data pengembangan dipakai bersama satu tim.** Membuka periode observasi, mencatat
   pemantauan, dan membuat pesanan laboratorium menulis baris nyata. Perlu persetujuan owner
   lebih dulu.

Yang **sudah** diverifikasi tanpa runtime: setiap field pada setiap formulir dicocokkan satu
per satu terhadap DTO backend, dan setiap aksi status dicocokkan terhadap `CanTransition`.

## 14. INCIDENTAL CHANGES: NONE

Nol perubahan line ending, nol BOM, nol format ulang. Lima berkas ter-*stage* milik pekerjaan
lain (kiosk, sidebar, Dockerfile, docker-compose, next.config) **tidak disentuh**.

## 15. INTERRUPTIONS: NONE

## 16. GIT STATUS

`QuilvianSystemFrontendDev` — branch `RizkiV2`, upstream `origin/RizkiV2`:

```
 M src/components/view/.../emergency-assessment-view/components/emergency-assessment-initial-tab.jsx
 M src/components/view/.../emergency-assessment-view/components/emergency-assessment-observation-tab.jsx
 M src/components/view/.../emergency-assessment-view/emergency-assessment-detail-view.jsx
 M src/lib/constants/health-services/emergency-installation-management/emergency-assessment-constant.jsx
 M src/lib/hooks/.../emergency-assessment/use-emergency-assessment-detail.jsx
 M src/lib/state/slice/.../emergency-assessment-slice.jsx
 M src/style/.../emergency-assessment/emergency-assessment.module.css
?? src/components/view/.../emergency-assessment-view/components/emergency-assessment-diagnostic-support-tab.jsx
```

Ditambah lima berkas ter-*stage* milik pekerjaan lain yang sudah ada sebelum task ini:
`.agent/README.md`, `Dockerfile`, `docker-compose.yml`, `next.config.js`,
`src/components/features/left-sidebar/left-sidebar.jsx`,
`src/components/view/kiosk/...`, `src/lib/hooks/kiosk/...`.

`NewQuilvianSystemBackend` — branch `rizkiG`: hanya laporan ini dan pembaruan roadmap.

**Nol `git add`, commit, push, merge, atau rebase dijalankan.**

## 17. NEXT RECOMMENDED STEP

Jalankan alur simpan sungguhan lewat layar dengan kredensial petugas, atas persetujuan owner
untuk menulis ke basis data bersama — melunasi utang yang berlaku sejak roadmap revision `1`.
Urutan yang paling murah membuktikan seluruh slice: simpan assesmen awal berisi kedelapan
kolom baru → buka periode observasi → catat dua putaran pemantauan → selesaikan periode →
pesan satu pemeriksaan laboratorium.

Sesudah itu, dua celah terbesar yang tersisa adalah **sikap pesanan serah terima**
(temuan 3 — menahan penutupan kunjungan tanpa jalan keluar lewat antarmuka) dan **resusitasi**
(temuan 4).
