# Roadmap Delivery Backend V2 — Sub-modul Episode Rawat Inap

> ## Berkas ini **baru**, dan **tidak menggantikan** `backend-roadmap.md`
>
> | Hal | `backend-roadmap.md` (lama) | **`backend-roadmap-v2.md` (berkas ini)** |
> | --- | --- | --- |
> | Isinya | 39 task revision `4` s.d. `6`, sebagian besar sudah `✅` | **Hanya** task penyelarasan `PRD-RWI-V2-001` revision `7` |
> | Rentang task ID | `BE-RWI-001` s.d. `BE-RWI-078` | **`BE-RWI-079` s.d. `BE-RWI-087`** |
> | Statusnya | **Tetap berlaku** sebagai register task lama | Register task baru |
> | Kenapa dipisah | Permintaan pemilik 16 September 2026: berkas lama sudah terlalu panjang untuk dibaca | — |
>
> **Nomor task tidak pernah dipakai ulang.** Deret `BE-RWI-###` berjalan lurus melintasi kedua
> berkas dan melintasi ketiga sub-modul. Sebelum menambah task baru, baca **kedua** berkas ini
> beserta roadmap sub-modul lain untuk menemukan nomor bebas berikutnya.
>
> Berkas lama: [`backend-roadmap.md`](./backend-roadmap.md) — jangan menambah task baru di sana.

## Metadata

```yaml
module_id: rawat-inap
module_name: InPatientManagement
entity_prefix: Inp
blueprint_id: RWI-BP-001
blueprint_revision: 7
blueprint_shape: COMPOSITE
submodule: episode-rawat-inap
blueprint_root: docs/module-blueprints/rawat-inap/episode-rawat-inap/
roadmap_file: roadmap/backend-roadmap-v2.md
roadmap_revision: 1
status: APPROVED
roadmap_mode: DELIVERY
realignment_phase: RLN-PH-07
approval_gate: BLUEPRINT_APPROVED
approved_by: "Muhammad Hamzah — Product/Domain owner (RWI-DEC-061)"
approved_at: "2026-09-16"
approval_decision: RWI-DEC-150
gate_closure_decision: RWI-DEC-151   # {GATE-YOGA} tertutup 2026-09-16
upstream_input: "PRD-RWI-V2-001 v2.0 — docs/Modul-RS/Rawat-Inap/04-prd-to-mvp-final.md"
input_revision_hash: sha256:2b3b2f29c9e547f448f186d7ac990e33dc3bdede8043a9b4bebfad6fbe0a679f
contract_version: 0.9.0
decision_source: "00-interview-decisions.md revision 23; RWI-DEC terakhir 151"
backend_source_sha: df3679c0d5b2f08106702153eb242d3a6cb2929b
frontend_source_sha: 1ce219b40f8e411f3c4e66975626ab33ae81616a
task_id_range: BE-RWI-079..BE-RWI-087
task_id_next_free: BE-RWI-127
waves: [RI-V2-1, RI-V2-2, RI-V2-3]
migration_steps: [E1, E2, E3]
test_policy: "rules/backend/TEST_POLICY.md — backend tidak memelihara project automated test"
write_authority: "TIDAK diberikan di sini. Wewenang tulis source, migration, database, dan deployment dinyatakan terpisah per task"
```

## Legenda tanda status

| Tanda | Arti |
| :---: | --- |
| ✅ | Acceptance criteria dan Definition of Done sudah terbukti, laporan tracked-nya ada |
| 🟡 | Source-nya sudah ada tetapi kriterianya belum terbukti penuh |
| ⛔ | Prasyaratnya belum terpenuhi; nama blocker-nya disebut |
| tanpa tanda | Belum disentuh sama sekali |

Status per 16 September 2026, sesudah gelombang eksekusi pertama dan verifikasinya:
empat task `✅`, tiga task `🟡`, satu task `⛔`, dan tidak ada lagi task tanpa tanda.
Rinciannya ada pada kolom `Task ID` tabel task dan pada baris `Status` masing-masing kartu.

---

## Grafik Urutan Dependency

```text
BE-RWI-079 ✅ ─┬─> BE-RWI-080 ✅
               │
               └─> BE-RWI-081 ✅

BE-RWI-091 [BE-DOK] ─> BE-RWI-082 ✅ ─┬─> BE-RWI-084 🟡 ─┬─> BE-RWI-085 ✅ ─> BE-RWI-086 🟡
                                      │                  │
BE-RWI-097 [BE-DOK] ─> BE-RWI-083 🟡 ─┘                  └─┬─> BE-RWI-087 ⛔
                                                           │
BE-RWI-114 [BE-KEP] ───────────────────────────────────────┘
```

`[BE-DOK]` = task backend pada `dokter-rawat-inap/roadmap/backend-roadmap-v2.md`, cermin baca-saja.
`[BE-KEP]` = task backend pada `keperawatan/roadmap/backend-roadmap-v2.md`, cermin baca-saja.

Jumlah pasangan prasyarat→task pada grafik: **10**. Jumlah entri kolom `Dependency` pada tabel
task: **10**. Keduanya cocok.

### Tabel gelombang eksekusi

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — | `BE-RWI-079` |
| 2 | `BE-RWI-079` | `BE-RWI-080`, `BE-RWI-081` — boleh paralel |
| 2 | `BE-RWI-091` [BE-DOK] mendarat | `BE-RWI-082` |
| 2 | `BE-RWI-097` [BE-DOK] mendarat | `BE-RWI-083` |
| 3 | `BE-RWI-082`, `BE-RWI-083` | `BE-RWI-084` |
| 4 | `BE-RWI-084` | `BE-RWI-085` |
| 4 | `BE-RWI-084` **dan** `BE-RWI-114` [BE-KEP] mendarat | `BE-RWI-087` — dirilis bersama `KEP-V2-2` |
| 5 | `BE-RWI-085` | `BE-RWI-086` |

Pemetaan gelombang PRD ke gelombang eksekusi di atas:

| Gelombang PRD | Isinya | Task |
| --- | --- | --- |
| `RI-V2-1` | `EPIC RI-38`, `EPIC RI-39` (migration E1), `FR-RI-198`, `199`, `201` | `BE-RWI-079` s.d. `BE-RWI-084` |
| `RI-V2-2` | `EPIC RI-40` (migration E2) | `BE-RWI-085`, `BE-RWI-086` |
| `RI-V2-3` | `FR-RI-200` | `BE-RWI-087` |

---

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| ✅ `BE-RWI-079` | Kolom tujuan penugasan ada di database beserta penjaganya | `FR-RI-193`, `FR-RI-194`; `RWI-DEC-099`, `RWI-DEC-130` | `0.9.0` — `data-dictionary` 18.1 | `InpDoctorAssignment` sudah ada | Migration E1: `AssignmentPurpose integer NOT NULL DEFAULT 0` + check constraint; baris lama menjadi `Regular` | — | AC-1 s.d. AC-4 pada kartu | `dotnet build`; verifikasi skema pada Postgres sekali pakai | Mundur dilarang bila sudah ada baris `LateDocumentation` / Muhammad Hamzah | Kartu `BE-RWI-079` |
| ✅ `BE-RWI-080` | Kepala ruangan melibatkan konsulen, dokter jaga, dan penugasan singkat | `FR-RI-193`, `194`, `195`; `EPIC RI-39` | `0.9.0` — API 10.2 | `InpatientDoctorAssignmentService` | Endpoint buat/akhiri penugasan pendukung; validasi `VAL-INP-01`, `08`, `09`; invariant `INV-INP-12` | `BE-RWI-079` | AC-1 s.d. AC-6 | `dotnet build`; verifikasi kontrak API; verifikasi proses bisnis `UAT-46` s.d. `UAT-48` | DPJP tidak boleh tergusur / Muhammad Hamzah | Kartu `BE-RWI-080` |
| ✅ `BE-RWI-081` | Dokter hanya melihat pasien penugasannya | `FR-RI-191`, `FR-RI-192`; `EPIC RI-38` | `0.9.0` — API 10.1 | Census `InpatientCensus` sudah ada | `assignedToMe=true` menyaring dari penugasan aktif; dokter dari akun login; ringkasan dari daftar yang sama; akun tanpa data dokter mendapat daftar kosong berpesan | `BE-RWI-079` | AC-1 s.d. AC-5 | `dotnet build`; verifikasi kontrak API; rencana eksekusi memakai index (`NFR-026`) | Kebocoran daftar pasien bila filter salah / Muhammad Hamzah | Kartu `BE-RWI-081` |
| ✅ `BE-RWI-082` | Penutupan episode mengunci konsep catatan dokter | `FR-RI-198`; `INT-INP-08`; `RWI-DEC-138` | `0.9.0` — integrasi `INT-INP-08` | Mesin keutuhan `MedicalRecordManagement` | Langkah 4 penutupan: konsep encounter menjadi `LockedUnsigned` di dalam transaksi penutupan | `BE-RWI-091` [BE-DOK] | AC-1 s.d. AC-4 | `dotnet build`; verifikasi proses bisnis `UAT-50`, `UAT-51`; galat buatan → nol perubahan (`NFR-025`) | ~~Pemberitahuan kepada Yoga Aji Pratama wajib tercatat~~ **terpenuhi 2026-09-16 lewat `RWI-DEC-151`** / Yoga Aji Pratama ✅ | Kartu `BE-RWI-082` |
| 🟡 `BE-RWI-083` | Penutupan membatalkan pesanan tertunda yang belum ditagih | `FR-RI-199`; `INT-INP-09`; `RWI-DEC-143` | `0.9.0` — API 10.4 | `PatientProcedureOrderService` dari `BE-RWI-097` | Langkah 5 penutupan; pesanan **tertagih** dibiarkan dan masuk daftar pantau | `BE-RWI-097` [BE-DOK] | AC-1 s.d. AC-5 | `dotnet build`; verifikasi kontrak API; verifikasi proses bisnis `UAT-50` | Nasib pesanan tertagih masih pertanyaan terbuka 22.7 nomor 1 / Muhammad Hamzah + pemilik Billing | Kartu `BE-RWI-083` |
| 🟡 `BE-RWI-084` | Petugas melihat apa yang akan terkunci sebelum menutup, dan akibatnya sesudah menutup | `FR-RI-201`; `VAL-INP-13` s.d. `17` | `0.9.0` — validation matrix | Endpoint kesiapan penutupan sudah ada | Peringatan yang **tidak menahan**; ringkasan akibat setelah penutupan | `BE-RWI-082`, `BE-RWI-083` | AC-1 s.d. AC-4 | `dotnet build`; verifikasi kontrak API; verifikasi proses bisnis | Peringatan tidak boleh berubah menjadi penghalang / Muhammad Hamzah | Kartu `BE-RWI-084` |
| ✅ `BE-RWI-085` | Resume memuat delapan bagian | `FR-RI-196`; `EPIC RI-40`; `RWI-DEC-112` | `0.9.0` — data 18.2–18.3 | `InpDischargeSummary` + tabel revisinya | Migration E2: tiga kolom nullable pada dua tabel; baca/simpan/tanda tangan delapan bagian | `BE-RWI-084` | AC-1 s.d. AC-4 | `dotnet build`; verifikasi skema; verifikasi kontrak API | Kolom nullable — resume lama tetap terbaca / Muhammad Hamzah | Kartu `BE-RWI-085` |
| 🟡 `BE-RWI-086` | Dokter menekan "Isi dari data klinis" dan melihat usulan bersumber | `FR-RI-197`; `NFR-027` | `0.9.0` — API 10.3 | Sumber klinis yang sudah ada | Usulan isian beserta **label sumber**; tidak tersimpan tanpa tindakan dokter; tahan terhadap sumber gagal | `BE-RWI-085` | AC-1 s.d. AC-5 | `dotnet build`; verifikasi kontrak API; pengukuran waktu per sumber dicatat pada laporan | Batas 5 detik per sumber adalah **angka usulan desain** / Muhammad Hamzah | Kartu `BE-RWI-086` |
| ⛔ `BE-RWI-087` | Penutupan membatalkan dosis obat berjadwal setelah waktu tutup | `FR-RI-200`; `INT-INP-10`, `INT-KEP-15` | `0.9.0` — integrasi | Mesin MAR dari `BE-RWI-114` | Langkah 6 penutupan; dosis `Due` setelah waktu tutup menjadi `Cancelled` dalam transaksi yang sama | `BE-RWI-084`, `BE-RWI-114` [BE-KEP] | AC-1 s.d. AC-4 | `dotnet build`; verifikasi proses bisnis; galat buatan → nol perubahan | Dosis `Administered` adalah rekam medis dan tidak boleh tersentuh / Muhammad Hamzah | Kartu `BE-RWI-087` |

---

## Kartu task

### ✅ `BE-RWI-079` — Kolom tujuan penugasan dokter (migration E1)

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 16 September 2026.** Keempat acceptance criteria terpetakan ke source **dan terbukti pada database.** `dotnet build` `0 Error(s)`, `211 Warning(s)`, `00:04:31`. Migration `E1` dijalankan pada container Postgres 16 sekali pakai: kolom `AssignmentPurpose integer NOT NULL DEFAULT 0`, check constraint `CK_InpDoctorAssignment_LateDocumentation`, dan index `IX_InpDoctorAssignment_DoctorId_Active` dibaca dari katalog dan **sama persis** dengan DDL `data-dictionary` 18.4. Uji perilaku 6 kasus: 2 diterima, 4 ditolak constraint. Mundur **ditolak** saat ada baris `LateDocumentation` (`P0001: BE-RWI-079: rollback ditolak. 1 baris …`), mundur **berhasil** sesudah baris itu dihapus, lalu **maju lagi berhasil**. Container dibuang; migration **belum** diterapkan ke database dev. Bukti: [laporan](../task/report/backend/BE-RWI-079.md) |
| **Gelombang** | 1 — `RI-V2-1` |
| **Outcome** | Database membedakan penugasan biasa dari penugasan konsulen, dokter jaga, dan penugasan singkat penulisan catatan terlambat |
| **Migration** | `E1` — `docs/module-blueprints/rawat-inap/episode-rawat-inap/02-backend-architecture.md` 11.8 |

**Bisnis prosesnya.** Hari ini `InpDoctorAssignment` hanya tahu "siapa dokter pasien ini". Ia tidak
tahu **kenapa** dokter itu ditugaskan. Padahal tiga hal berikut berbeda kewenangannya: DPJP yang
bertanggung jawab penuh, konsulen yang dimintai pendapat, dan dokter jaga yang diberi jendela
singkat hanya untuk menulis catatan yang tertinggal. Kolom `AssignmentPurpose` inilah yang
membedakan ketiganya.

**Contoh.** dr. Yoga lupa menandatangani SOAP shift malam. Kepala ruangan membuatkan penugasan
singkat 10.00–11.00 dengan tujuan `LateDocumentation`. Baris itu tidak menggeser dr. Ahmad sebagai
DPJP, dan jendela satu jam itulah yang nanti dibaca penjaga penulis klinis.

**Cakupan yang diharapkan.**

- Migration menambah `AssignmentPurpose integer NOT NULL DEFAULT 0` pada `InpDoctorAssignment`.
- Check constraint menegakkan nilai yang sah saja.
- Seluruh baris lama bernilai `Regular`, tanpa tebakan dan tanpa pengisian ulang.

**Acceptance criteria.**

1. Kolom `AssignmentPurpose` ada, `NOT NULL`, bawaan `0` = `Regular`.
2. Check constraint menolak nilai di luar enum yang tercatat pada `data-dictionary` 18.1.
3. Seluruh baris yang sudah ada bernilai `Regular` sesudah migration dijalankan.
4. Migration mundur menghapus kolom **hanya** selama belum ada baris `LateDocumentation`; bila sudah ada, mundur berhenti dengan pesan yang menyebut alasannya.

**Bukti verifikasi.** `dotnet restore` dan `dotnet build` pada project aplikasi; verifikasi skema
pada container Postgres sekali pakai; keluaran perintahnya ditempel apa adanya ke laporan task.

**Definition of Done.** Migration maju dan mundur dijalankan pada Postgres sekali pakai; laporan
tracked ada di `task/report/backend/BE-RWI-079.md`; baris status pada roadmap ini diperbarui;
`requirement-traceability-v2.md` membawa buktinya.

**Wewenang yang belum diberikan.** Menjalankan migration ini pada database mana pun di luar
container sekali pakai adalah wewenang terpisah dan **tidak** tercakup task ini.

---

### ✅ `BE-RWI-080` — Penugasan konsulen, dokter jaga, dan penugasan singkat

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 16 September 2026.** Keenam acceptance criteria terpetakan ke source. `dotnet build` `0 Error(s)`, `211 Warning(s)`, `00:04:31`. Penjaga database `INV-INP-12` **diuji perilakunya**: penugasan singkat tanpa waktu selesai, berperan DPJP, beralasan kosong, dan berwaktu selesai mendahului waktu mulai seluruhnya **ditolak** `CK_InpDoctorAssignment_LateDocumentation` — butir DoD "check constraint ikut diuji" terpenuhi. Kode status mengikuti kontrak `0.9.0` 10.2 (`VAL-INP-08` → `400`, `VAL-INP-09` → `409`), berbeda dari blok Swagger ilustratif kartu ini; selisihnya dicatat pada laporan. `Idempotency-Key` **belum dipasang**. `UAT-46` s.d. `UAT-48` `NOT RUN`, **dikecualikan atas keputusan pemilik pekerjaan 16 September 2026**. Bukti: [laporan](../task/report/backend/BE-RWI-080.md) |
| **Gelombang** | 2 — `RI-V2-1` |
| **Outcome** | Kepala ruangan dapat melibatkan dokter lain tanpa mengganggu DPJP |

**Bisnis prosesnya.** Pasien rawat inap sering ditangani lebih dari satu dokter. Yang boleh
menambahkan mereka bukan dokternya sendiri, melainkan **kepala ruangan atau supervisor** — supaya
tidak ada dokter yang memberi dirinya sendiri akses ke rekam medis pasien yang bukan tanggung
jawabnya. Setiap penambahan wajib beralasan, dan penugasan singkat wajib punya waktu selesai.

**Contoh.** Budi dirawat dr. Ahmad. Kondisi jantungnya memburuk, sehingga kepala ruangan melibatkan
dr. Sari sebagai konsulen dengan alasan "konsultasi kardiologi". Besoknya konsultasi selesai dan
kepala ruangan menekan "Akhiri". dr. Ahmad tetap DPJP sepanjang episode.

**Cakupan yang diharapkan.**

- Endpoint pembuatan penugasan pendukung beserta alasannya.
- Endpoint pengakhiran penugasan konsulen dan dokter jaga.
- Penugasan singkat selalu berperan dokter jaga, berwaktu mulai "sekarang", berwaktu selesai wajib.
- Validasi `VAL-INP-01`, `VAL-INP-08`, `VAL-INP-09`; invariant `INV-INP-12`.

**Endpoint bergaya Swagger.**

```yaml
POST /api/inpatient-management/episodes/{episodeId}/doctor-assignments:
  summary: Membuat penugasan dokter pendukung pada satu episode
  security: [ { bearer: [] } ]
  x-permission: "InpatientEpisode : Update — hanya kepala ruangan atau supervisor"
  requestBody:
    required: true
    content:
      application/json:
        schema:
          type: object
          required: [doctorId, assignmentPurpose, reason]
          properties:
            doctorId:        { type: string, format: uuid }
            assignmentPurpose: { type: string, enum: [Consultant, WardDoctor, LateDocumentation] }
            reason:          { type: string, minLength: 1, maxLength: 500 }
            startedAt:       { type: string, format: date-time, description: "Diabaikan untuk LateDocumentation; selalu waktu sekarang" }
            endedAt:         { type: string, format: date-time, description: "WAJIB untuk LateDocumentation" }
  responses:
    "201": { description: Penugasan dibuat }
    "403": { description: Pemanggil bukan kepala ruangan atau supervisor }
    "422": { description: "VAL-INP-08 — penugasan singkat tanpa waktu selesai; atau percobaan mengganti DPJP" }

DELETE /api/inpatient-management/episodes/{episodeId}/doctor-assignments/{assignmentId}:
  summary: Mengakhiri penugasan konsulen atau dokter jaga
  x-permission: "InpatientEpisode : Update"
  responses:
    "204": { description: Penugasan diakhiri }
    "422": { description: "VAL-INP-09 — penugasan DPJP tidak dapat diakhiri lewat jalur ini" }
```

**Acceptance criteria.**

1. Kepala ruangan membuat penugasan konsulen beralasan → `201`, dan baris penugasan bertujuan `Consultant`.
2. Penugasan singkat tanpa `endedAt` → `422` menyebut `VAL-INP-08`.
3. Penugasan singkat selalu berperan dokter jaga dan berwaktu mulai "sekarang", walau pemanggil mengirim `startedAt` lain.
4. Penugasan baru apa pun **tidak** mengubah siapa DPJP episode itu — `INV-INP-12`.
5. Perawat pelaksana memanggil endpoint ini → `403`.
6. Penugasan DPJP dicoba diakhiri lewat `DELETE` → `422` menyebut `VAL-INP-09`.

**Bukti verifikasi.** `dotnet build`; verifikasi kontrak API terhadap `contracts/api-contract.md`
`0.9.0` bagian 10.2; verifikasi proses bisnis menjalankan `UAT-46`, `UAT-47`, dan `UAT-48`.

**Definition of Done.** Penjagaan penugasan singkat ada di **service dan database** — check
constraint dari `BE-RWI-079` ikut diuji; laporan tracked ada; roadmap dan traceability diperbarui.

---

### ✅ `BE-RWI-081` — Census dokter dari penugasan aktif

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 16 September 2026.** Kelima acceptance criteria terpetakan ke source. `dotnet build` `0 Error(s)`, `211 Warning(s)`, `00:04:31`. Index `IX_InpDoctorAssignment_DoctorId_Active` terbukti lahir di database. `NeedsReviewCount` baru mencakup entri CPPT; bagian pesanan tindakan menunggu `BE-RWI-097` [BE-DOK] — di luar acceptance criteria kartu ini. **Rencana eksekusi query `NFR-026` `NOT RUN`**: pada tabel kosong PostgreSQL memilih `Seq Scan` apa pun index-nya, sehingga `EXPLAIN` di sana tidak membuktikan apa pun. Uji batas 06.59/07.01 `NOT RUN`, dikecualikan atas keputusan pemilik pekerjaan 16 September 2026. Bukti: [laporan](../task/report/backend/BE-RWI-081.md) |
| **Gelombang** | 2 — `RI-V2-1` |
| **Outcome** | Daftar pasien dokter memuat persis pasien yang boleh ia tulis, tidak lebih |

**Bisnis prosesnya.** Rumah sakit punya 122 pasien rawat inap. dr. Ahmad hanya bertanggung jawab
atas dua di antaranya. Daftar yang menampilkan 122 pasien bukan sekadar merepotkan — ia membuka
data pasien yang bukan urusannya. Karena itu daftarnya diambil dari **penugasan aktif**, dan
identitas dokternya diambil dari **akun login**, bukan dari parameter yang dikirim peramban.

**Contoh.** Akun dr. Ahmad mengirim `doctorId` milik dr. Rina bersama `assignedToMe=true`. Yang
kembali tetap dua pasien dr. Ahmad. Parameter `doctorId` diabaikan, bukan dipakai.

**Endpoint bergaya Swagger.**

```yaml
GET /api/inpatient-management/census:
  summary: Daftar pasien rawat inap, disaring penugasan dokter login
  x-permission: "InpatientCensus : Read"
  parameters:
    - { name: assignedToMe, in: query, schema: { type: boolean, default: false } }
    - { name: doctorId,     in: query, schema: { type: string, format: uuid },
        description: "DIABAIKAN bila assignedToMe=true — FR-DOK-070" }
    - { name: page,     in: query, schema: { type: integer, default: 1 } }
    - { name: pageSize, in: query, schema: { type: integer, default: 20 } }
  responses:
    "200":
      description: Daftar pasien beserta ringkasan yang dihitung dari daftar yang sama
      content:
        application/json:
          schema:
            type: object
            properties:
              items: { type: array, items: { $ref: "#/components/schemas/InpatientCensusRow" } }
              summary:
                type: object
                properties:
                  totalPatients: { type: integer }
              emptyReason:
                type: string
                nullable: true
                description: "Terisi bila akun tidak punya data dokter — FR-RI-192"
```

**Acceptance criteria.**

1. `assignedToMe=true` hanya mengembalikan pasien yang punya penugasan **aktif** milik dokter login.
2. `doctorId` yang dikirim bersama `assignedToMe=true` diabaikan seluruhnya.
3. Ringkasan `totalPatients` dihitung dari daftar yang sama, bukan dari query terpisah.
4. Akun tanpa data dokter menerima `200` dengan daftar kosong dan `emptyReason` terisi — **bukan** `403`.
5. Keaktifan penugasan dinilai saat query dijalankan, tanpa proses latar — uji batas 06.59 dan 07.01 membedakan hasilnya.

**Bukti verifikasi.** `dotnet build`; verifikasi kontrak API bagian 10.1; rencana eksekusi query
menunjukkan pemakaian index penugasan per dokter sesuai `NFR-026`, dan keluarannya ditempel ke
laporan.

**Definition of Done.** Regresi census unit — daftar per unit tanpa `assignedToMe` — tetap sama
seperti sebelumnya; laporan tracked ada; roadmap dan traceability diperbarui.

---

### ✅ `BE-RWI-082` — Penutupan episode mengunci konsep catatan dokter

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 16 September 2026.** Keempat acceptance criteria terpetakan ke source. `dotnet build` `0 Error(s)`, `211 Warning(s)`, `00:04:31`, termasuk pembentukan host penuh yang membuktikan dependency baru `ClinicalDocumentIntegrityService` pada `InpDischargeService` dapat di-resolve. Langkah 4 berada di dalam transaksi `CloseEpisodeInternalAsync` dan memanggil service pemiliknya, bukan menulis `MrcClinicalDocumentIntegrity` langsung. Persetujuan `INT-INP-08` dirujuk dari `RWI-DEC-151`. **Cakupan dokumen yang terkunci bertambah sendiri ketika `BE-RWI-091` [BE-DOK] mendarat**, tanpa perubahan source di sini. `UAT-50`, `UAT-51`, dan uji galat buatan `NFR-025` `NOT RUN`, **dikecualikan atas keputusan pemilik pekerjaan 16 September 2026**. Bukti: [laporan](../task/report/backend/BE-RWI-082.md) |
| **Gelombang** | 2 — `RI-V2-1`, menunggu `BE-RWI-091` [BE-DOK] |
| **Outcome** | Konsep yang tidak sempat ditandatangani tetap terbaca sebagai konsep yang terkunci, bukan hilang dan bukan tertanda tangan |

**Bisnis prosesnya.** Saat episode ditutup, kadang masih ada catatan yang berstatus konsep. Dua
jalan keluar yang salah: menghapusnya — riwayat klinis hilang; atau menandatanganinya otomatis —
sistem memalsukan tanda tangan dokter. `RWI-DEC-138` memilih jalan ketiga, mengikuti `RM-DEC-003`:
konsep itu **dikunci apa adanya** dan ditandai "Tidak Ditandatangani".

**Contoh.** Joko pulang Jumat 14.00. Ada satu SOAP konsep dari Kamis malam yang belum
ditandatangani. Setelah penutupan, SOAP itu tetap ada, berstatus `LockedUnsigned`, terbaca siapa
penulisnya dan kapan ditulis, dan tidak bisa lagi disunting siapa pun.

**Cakupan yang diharapkan.**

- Langkah 4 pada urutan penutupan episode (`E3`).
- Seluruh registrasi keutuhan `Draft` milik encounter itu menjadi `LockedUnsigned`.
- Dijalankan **di dalam transaksi penutupan**, bukan sesudahnya.

**Acceptance criteria.**

1. Penutupan episode mengubah seluruh konsep encounter itu menjadi `LockedUnsigned`.
2. `LockedUnsigned` tidak pernah kembali menjadi `Draft` — `INV-DOK-18`.
3. Galat buatan pada langkah mana pun membuat **nol** perubahan tersimpan, termasuk penguncian ini — `NFR-025`.
4. Episode yang ditutup tanpa satu pun konsep tetap tertutup normal, tanpa galat.

**Bukti verifikasi.** `dotnet build`; verifikasi proses bisnis menjalankan `UAT-50` dan `UAT-51`;
uji galat buatan pada Postgres sekali pakai dengan keluaran ditempel ke laporan.

**Definition of Done.** ~~Pemberitahuan kepada Yoga Aji Pratama atas pemanggilan mesin penguncian
dari `InPatientManagement` (`INT-INP-08`) tercatat pada laporan task~~ — **butir ini sudah terpenuhi
lebih awal**: Yoga Aji Pratama menyetujui pemanggilan itu pada 16 September 2026, tercatat
`RWI-DEC-151`. Laporan task cukup **merujuk `RWI-DEC-151`**, tanpa pemberitahuan terpisah. Laporan
tracked ada; roadmap dan traceability diperbarui.

**Yang persetujuan itu tidak berikan.** Wewenang menulis source pada mesin penguncian tetap
dinyatakan terpisah saat task ini dikirim ke builder.

---

### 🟡 `BE-RWI-083` — Penutupan membatalkan pesanan tindakan tertunda

| Field | Isi |
| --- | --- |
| **Status** | 🟡 **SEBAGIAN.** `dotnet build` `0 Error(s)`, `211 Warning(s)`, `00:04:31`. Tiga dari lima acceptance criteria terpetakan ke source — AC-2, AC-3, dan AC-4. **AC-1 dan AC-5 belum terpenuhi:** langkah 5 penutupan belum dipasang karena `PatientProcedureOrderService` milik `ClinicalManagement` dibuat `BE-RWI-097` [BE-DOK] dan belum ada di repository; menulis `TrxPatientProcedure` langsung dari `InPatientManagement` ditolak `02-backend-architecture.md` 11.3. `UAT-50` `NOT RUN`, dan menjalankannya sekarang pun belum dapat lulus penuh karena langkah yang diujinya memang belum ada. Bukti sejauh ini: [laporan](../task/report/backend/BE-RWI-083.md) |
| **Gelombang** | 2 — `RI-V2-1`, menunggu `BE-RWI-097` [BE-DOK] |
| **Outcome** | Pesanan yang tidak akan pernah dikerjakan ditutup rapi; pesanan yang sudah ditagih tidak dihapus diam-diam |

**Bisnis prosesnya.** Pasien pulang dengan satu pesanan tindakan yang belum dikerjakan. Kalau
dibiarkan, pesanan itu menggantung selamanya di antrean tindakan. Kalau dibatalkan semuanya,
pesanan yang **sudah ditagih ke pasien** ikut hilang padahal uangnya sudah masuk. `RWI-DEC-143`
memisahkan keduanya: yang belum ditagih dibatalkan beralasan tetap, yang sudah ditagih dibiarkan
dan dimunculkan di daftar pantau supaya ada orang yang menindaklanjutinya.

**Contoh.** Joko punya dua pesanan tertunda: fisioterapi yang belum ditagih, dan EKG yang sudah
masuk tagihan. Setelah penutupan, fisioterapi berstatus `Cancelled` beralasan "Episode ditutup",
sedangkan EKG tetap dan muncul pada daftar pantau pesanan tertagih.

**Endpoint bergaya Swagger.**

```yaml
GET /api/inpatient-management/monitoring/billed-pending-procedure-orders:
  summary: Daftar pantau pesanan tindakan tertagih yang tidak dibatalkan saat penutupan
  x-permission: "InpatientMonitoring : Read"
  parameters:
    - { name: serviceUnitId, in: query, schema: { type: string, format: uuid } }
  responses:
    "200": { description: Daftar pesanan beserta episode, pasien, waktu tutup, dan status tagihannya }
```

**Acceptance criteria.**

1. Pesanan tertunda yang **belum ditagih** menjadi `Cancelled` dengan alasan tetap saat penutupan.
2. Pesanan tertunda yang **sudah ditagih** tidak disentuh sama sekali.
3. Pesanan yang sudah `Completed` atau `Cancelled` sebelumnya tidak disentuh.
4. Daftar pantau memuat persis pesanan pada kriteria 2.
5. Galat buatan → nol perubahan tersimpan.

**Bukti verifikasi.** `dotnet build`; verifikasi kontrak API bagian 10.4; verifikasi proses bisnis
`UAT-50`.

**Risiko yang terbuka.** Apa yang **selanjutnya** dilakukan terhadap pesanan tertagih itu belum
diputuskan — `04-prd-to-mvp.md` 22.7 nomor 1, menunggu Muhammad Hamzah bersama pemilik Billing.
Task ini hanya wajib **memunculkannya**, bukan menyelesaikannya. Kalau keputusan itu turun sebelum
task dikerjakan, cakupannya dinilai ulang lebih dulu.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### 🟡 `BE-RWI-084` — Kesiapan dan akibat penutupan episode

| Field | Isi |
| --- | --- |
| **Status** | 🟡 **SEBAGIAN.** `dotnet build` `0 Error(s)`, `211 Warning(s)`, `00:04:31`. AC-2 dan AC-4 terpetakan penuh ke source: `isReady` dan `isReadyWithOverride` dihitung dari `conditions` saja sehingga peringatan tidak pernah menahan. **AC-1 baru tiga dari empat peringatan** — `UNRECORDED_PAST_DOSES` belum dapat dibaca karena tabel MAR `BE-RWI-114` [BE-KEP] belum ada, dan ditandai `isMeasured = false`, bukan dilaporkan nol. **AC-3 baru dua dari empat angka akibat** — `cancelledProcedureOrderCount` menunggu `BE-RWI-097`, `cancelledFutureDoseCount` menunggu `BE-RWI-114`, keduanya disertai `notYetWiredSteps`. `UAT-50` dan `UAT-51` `NOT RUN`, dikecualikan atas keputusan pemilik pekerjaan 16 September 2026. Bukti sejauh ini: [laporan](../task/report/backend/BE-RWI-084.md) |
| **Gelombang** | 3 — `RI-V2-1` |
| **Outcome** | Petugas tahu apa yang akan terjadi sebelum menekan tutup, dan tahu apa yang sudah terjadi sesudahnya |

**Bisnis prosesnya.** Penutupan episode punya akibat yang tidak bisa dibatalkan: konsep terkunci,
pesanan batal, dosis batal. Petugas admisi yang menekan tombolnya bukan orang klinis, dan sering
tidak tahu ada konsep dokter yang belum ditandatangani. Karena itu sistem memberi **peringatan**
lebih dulu. Peringatan itu sengaja **tidak menahan**: menahan penutupan berarti pasien yang sudah
pulang tetap tercatat dirawat, dan itu lebih berbahaya.

**Contoh.** Sebelum menutup episode Joko, layar menampilkan "1 konsep akan terkunci, 1 pesanan akan
dibatalkan, 1 dosis akan dibatalkan". Petugas tetap boleh melanjutkan. Sesudah menutup, layar
menampilkan ringkasan apa yang benar-benar terjadi.

**Acceptance criteria.**

1. Endpoint kesiapan menyebut jumlah konsep, pesanan, dan dosis yang akan terdampak.
2. Peringatan **tidak** menahan penutupan — `VAL-INP-13` s.d. `VAL-INP-17` seluruhnya bersifat peringatan.
3. Hasil penutupan mengembalikan ringkasan akibat yang benar-benar tersimpan, bukan yang diperkirakan sebelumnya.
4. Penutupan yang gagal menampilkan "Penutupan gagal disimpan, coba lagi" dan episode tetap `DischargePending` — `UAT-51`.

**Bukti verifikasi.** `dotnet build`; verifikasi kontrak API; verifikasi proses bisnis `UAT-50` dan
`UAT-51`.

**Definition of Done.** Regresi penutupan **tanpa** konsep, pesanan, maupun dosis tetap hijau;
laporan tracked ada; roadmap dan traceability diperbarui.

---

### ✅ `BE-RWI-085` — Resume pulang delapan bagian (migration E2)

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 16 September 2026.** Keempat acceptance criteria terpetakan ke source **dan terbukti pada database.** `dotnet build` `0 Error(s)`, `211 Warning(s)`, `00:04:31`. Migration `E2` dijalankan pada container Postgres 16 sekali pakai: keenam kolom dibaca dari `information_schema.columns` — `ImportantFindingsSummary varchar(4000)`, `DischargeConditionNote varchar(2000)`, `EducationSummary varchar(2000)`, seluruhnya nullable, pada kedua tabel. Mundur **ditolak** saat ada isian terisi (`P0001: BE-RWI-085: rollback ditolak. 1 baris …`), mundur **berhasil** sesudah dikosongkan, lalu **maju lagi berhasil**. Container dibuang; migration **belum** diterapkan ke database dev. Bukti: [laporan](../task/report/backend/BE-RWI-085.md) |
| **Gelombang** | 4 — `RI-V2-2` |
| **Outcome** | Resume pulang memuat Pemeriksaan Penting, Kondisi Saat Pulang, dan Edukasi |

**Bisnis prosesnya.** Resume pulang adalah dokumen yang dibawa pasien ke fasilitas kesehatan
berikutnya. Tiga isian yang selama ini tidak ada — hasil pemeriksaan penting, kondisi pasien saat
pulang, dan edukasi yang sudah diberikan — justru bagian yang paling dibaca dokter penerima.

**Cakupan yang diharapkan.**

- Migration `E2`: `ImportantFindingsSummary varchar(4000)`, `DischargeConditionNote varchar(2000)`, `EducationSummary varchar(2000)`, seluruhnya nullable.
- Ketiga kolom yang sama pada `InpDischargeSummaryRevision`.
- Endpoint baca, simpan, dan tanda tangan resume membawa ketiga isian itu.

**Acceptance criteria.**

1. Ketiga kolom ada pada `InpDischargeSummary` dan `InpDischargeSummaryRevision`, seluruhnya nullable.
2. Resume lama tanpa ketiga isian tetap terbaca dan tetap dapat ditandatangani.
3. Perubahan pada resume tersimpan bervers sebagai revisi, termasuk ketiga isian baru.
4. Migration mundur menghapus kolom selama kosong.

**Bukti verifikasi.** `dotnet build`; verifikasi skema pada Postgres sekali pakai; verifikasi
kontrak API.

**Catatan lintas sub-modul.** Kontrak yang sama dipakai tab Resume Medis `FE-DOK-12` milik
`dokter-rawat-inap` — `FR-DOK-107`. Bentuk payload-nya tidak boleh berbeda antara dua permukaan itu.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### 🟡 `BE-RWI-086` — Usulan isian resume dari data klinis

| Field | Isi |
| --- | --- |
| **Status** | 🟡 **SEBAGIAN.** `dotnet build` `0 Error(s)`, `211 Warning(s)`, `00:04:31`, termasuk pembentukan host penuh yang membuktikan `InpDischargeSummaryPrefillService` terdaftar dan dapat di-resolve; berkas baru terbesar pada rangkaian ini **nol warning**. AC-1 s.d. AC-4 terpetakan ke source: usulan berlabel sumber, nol `SaveChanges`, tahan sumber gagal per bagian, dan menolak membentuk usulan untuk resume yang sudah ditandatangani. **AC-5 baru sebagian:** pengukuran waktu terpasang dan keluar pada `sourceTimings`, tetapi **angka dari data nyata `NOT RUN`** karena menuntut lingkungan berisi data klinis — sehingga bukti `NFR-027` belum ada. Sumber laboratorium dilaporkan belum tersedia pada setiap balasan: `LabExamination` belum menyimpan nilai hasil maupun penandaan kritis. Bukti sejauh ini: [laporan](../task/report/backend/BE-RWI-086.md) |
| **Gelombang** | 5 — `RI-V2-2` |
| **Outcome** | Dokter mendapat usulan isian bersumber, dan tetap dialah yang memutuskan |

**Bisnis prosesnya.** Menulis resume dari nol memakan waktu, sehingga sering ditunda sampai pasien
sudah pulang. Sistem boleh membantu dengan mengambil bahan dari data klinis yang sudah ada — hasil
lab terakhir, diagnosis, obat pulang. Yang **tidak boleh** adalah menyimpannya sebagai resume tanpa
dokter membacanya: resume adalah dokumen bertanda tangan, dan tanda tangan itu berarti dokter
menyatakan isinya benar.

**Contoh.** dr. Rina menekan "Isi dari data klinis". Kotak Pemeriksaan Penting terisi usulan
bertanda "Sumber: Hasil Laboratorium 14 Sep 2026". dr. Rina menghapus dua baris, menambah satu, lalu
menandatangani. Yang tersimpan adalah teks final dr. Rina.

**Acceptance criteria.**

1. Endpoint usulan mengembalikan isian beserta **label sumber** untuk setiap bagian.
2. Memanggil endpoint usulan **tidak** menyimpan apa pun ke resume.
3. Satu sumber yang gagal dibaca tidak menggagalkan seluruh usulan; bagian itu kembali kosong berketerangan.
4. Usulan hanya dibentuk untuk resume yang belum ditandatangani.
5. Waktu penyelesaian per sumber diukur dan dicatat pada laporan task.

**Bukti verifikasi.** `dotnet build`; verifikasi kontrak API bagian 10.3; pengukuran waktu per
sumber ditempel apa adanya.

**Risiko yang terbuka.** `NFR-027` menyebut batas 5 detik per sumber sebagai **angka usulan desain**
yang dikonfirmasi saat approval. `RWI-DEC-150` menyetujui desainnya; bila pengukuran nyata jauh
melampaui 5 detik, angka itu dilaporkan apa adanya dan dibawa kembali ke pemilik, **bukan** diam-diam
diubah di roadmap.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### ⛔ `BE-RWI-087` — Penutupan membatalkan dosis obat berjadwal

| Field | Isi |
| --- | --- |
| **Status** | ⛔ **TERBLOKIR `BE-RWI-114`** [BE-KEP] pada roadmap `keperawatan` — "MAR dan pembentukan dosis (migration `K4`)". Per 16 September 2026, `PhmMedicationAdministration` dan `MedicationAdministrationService` **tidak ada sama sekali** di repository; pencarian nama berkas dan nama kelas pada seluruh `Areas/**` mengembalikan nol hasil. `02-backend-architecture.md` 11.8 memerintahkannya apa adanya: "Selama tabel dosis belum ada, langkah 6 tidak dipasang." **Nol berkas source diubah**, sehingga `dotnet build` `0 Error(s)`, `211 Warning(s)`, `00:04:31` pada rangkaian ini tidak memuat satu baris pun dari task ini. Titik pemasangan langkah 6 sudah ditandai di dalam `CloseEpisodeInternalAsync`, dan `sideEffects.cancelledFutureDoseCount` sudah ada bentuknya bernilai `0` beserta `notYetWiredSteps`. Pemilik blocker: roadmap `keperawatan`. Bukti: [laporan](../task/report/backend/BE-RWI-087.md) |
| **Gelombang** | 4 — `RI-V2-3`, dirilis bersama `KEP-V2-2` |
| **Outcome** | Tidak ada dosis obat yang menunggu diberikan kepada pasien yang sudah pulang |

**Bisnis prosesnya.** MAR membentuk dosis `Due` ke depan berdasarkan jadwal. Kalau pasien pulang
pukul 14.00, dosis pukul 20.00 tetap ada di daftar perawat dan bisa tercatat diberikan kepada
pasien yang tidak ada di ruangan. Karena itu penutupan membatalkan dosis berjadwal **setelah waktu
tutup** — dan hanya itu. Dosis yang sudah `Administered` adalah rekam medis pemberian obat dan tidak
pernah disentuh.

**Contoh.** Joko pulang 14.00. Dosis 08.00 sudah `Administered` → tetap. Dosis 20.00 masih `Due` →
`Cancelled` beralasan "Episode ditutup". Dosis 12.00 yang `Missed` → tetap `Missed`, karena itu
fakta yang sudah terjadi.

**Acceptance criteria.**

1. Dosis `Due` berjadwal **setelah** waktu tutup menjadi `Cancelled` beralasan tetap.
2. Dosis `Administered`, `Held`, `Refused`, dan `Missed` tidak disentuh sama sekali.
3. Pembatalan terjadi di dalam transaksi penutupan yang sama — `INT-KEP-15`.
4. Galat buatan → nol perubahan tersimpan.

**Bukti verifikasi.** `dotnet build`; verifikasi proses bisnis `UAT-50`; uji galat buatan.

**Definition of Done.** Task ini **tidak boleh** dimulai sebelum `BE-RWI-114` [BE-KEP] mendarat,
karena tabel MAR-nya belum ada. Laporan tracked ada; roadmap dan traceability diperbarui.

---

## Kebijakan verifikasi backend

Mengikuti [`rules/backend/TEST_POLICY.md`](../../../../../../QuilvianEngineeringSkills/.claude/rules/backend/TEST_POLICY.md)
sebagaimana berlaku pada repository ini:

- Backend **tidak memelihara project automated test**. Tidak adanya automated test **bukan**
  coverage gap dan tidak pernah menahan `✅`.
- Roadmap ini karena itu **tidak memuat** satu pun task "write unit tests", "integration tests",
  maupun pembuatan test project.
- Bukti verifikasi yang dipetakan adalah: QBE preflight dan conformance, review diff dan scope,
  `dotnet restore` serta `dotnet build` project aplikasi, verifikasi kontrak API, verifikasi proses
  bisnis, dan verifikasi runtime pada Postgres sekali pakai bila task menyentuh skema.
- Kesesuaian QBE dan aturan engineering diselesaikan **pada waktu eksekusi** dari `AGENTS.md`
  repository backend target beserta dokumen engineering canonical-nya, bukan disalin ulang di sini.

---

## Gerbang yang masih terbuka

| Gerbang | Menahan | Siapa yang membukanya |
| --- | --- | --- |
| ~~Pemberitahuan `INT-INP-08` kepada Yoga Aji Pratama~~ | **TERTUTUP 2026-09-16** lewat `RWI-DEC-151` — DoD `BE-RWI-082` cukup merujuk keputusan itu | Yoga Aji Pratama ✅ |
| Nasib pesanan tertagih setelah penutupan — 22.7 nomor 1 | Tidak menahan; menilai ulang cakupan `BE-RWI-083` bila keputusannya turun | Muhammad Hamzah + pemilik Billing |
| Isi minimal resume, termasuk kewajiban tiga isian baru sebelum tanda tangan — 22.7 nomor 3 | Gerbang **produksi**, bukan gerbang task | Pemilik klinis, belum ditunjuk |
| `BE-RWI-091` [BE-DOK] | ~~`BE-RWI-082`~~ — **tidak lagi menahan.** `BE-RWI-082` ✅ 16 September 2026: langkah 4 terpasang memakai mesin penguncian yang sudah ada. Yang bertambah ketika `BE-RWI-091` mendarat adalah **cakupan dokumen** yang punya registrasi `Draft`, tanpa perubahan source | Roadmap `dokter-rawat-inap` |
| `BE-RWI-097` [BE-DOK] | `BE-RWI-083` langkah 5 — **masih menahan**, task 🟡 3 dari 5 kriteria; juga menahan `cancelledProcedureOrderCount` pada `BE-RWI-084` dan bagian pesanan `NeedsReviewCount` pada `BE-RWI-081` | Roadmap `dokter-rawat-inap` |
| `BE-RWI-114` [BE-KEP] | `BE-RWI-087` — **masih menahan**, task ⛔ dan nol berkas source diubah; juga menahan peringatan `UNRECORDED_PAST_DOSES` dan `cancelledFutureDoseCount` pada `BE-RWI-084` | Roadmap `keperawatan` |

---

## Traceability

Tabel penuh ada di [`requirement-traceability-v2.md`](./requirement-traceability-v2.md).
Ringkasannya:

| FR | Epic | Task backend | Task frontend | Kontrak | Bukti acceptance |
| --- | --- | --- | --- | --- | --- |
| `FR-RI-191`, `FR-RI-192` | `RI-38` | `BE-RWI-081` | `FE-RWI-067` [FE-DOK] | `0.9.0` API 10.1 | Acceptance 18.1 |
| `FR-RI-193`, `194`, `195` | `RI-39` | `BE-RWI-079`, `BE-RWI-080` | `FE-RWI-063` | `0.9.0` API 10.2 | Acceptance 18.2 |
| `FR-RI-196` | `RI-40` | `BE-RWI-085` | `FE-RWI-064` | `0.9.0` data 18.2–18.3 | Acceptance 18.3 |
| `FR-RI-197` | `RI-40` | `BE-RWI-086` | `FE-RWI-064` | `0.9.0` API 10.3 | Acceptance 18.3 |
| `FR-RI-198` | `RI-41` | `BE-RWI-082` | `FE-RWI-065` | `INT-INP-08` | Acceptance 18.4 |
| `FR-RI-199` | `RI-41` | `BE-RWI-083` | `FE-RWI-065`, `FE-RWI-066` | `INT-INP-09`, API 10.4 | Acceptance 18.4 |
| `FR-RI-200` | `RI-41` | `BE-RWI-087` | `FE-RWI-065` | `INT-INP-10`, `INT-KEP-15` | Acceptance 18.4 |
| `FR-RI-201` | `RI-41` | `BE-RWI-084` | `FE-RWI-065` | `VAL-INP-13` s.d. `17` | Acceptance 18.4 |

**Gap keterkaitan requirement ke bukti verifikasi:** nol. Kedelapan FR pada amandemen terbatas
revision `7` punya task backend dan bukti acceptance-nya.
