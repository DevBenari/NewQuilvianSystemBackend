# Roadmap Delivery Backend — Sub-modul `dokter-rawat-inap` (Rawat Inap)

## Metadata

```yaml
module_id: rawat-inap
module_name: InPatientManagement
entity_prefix: Inp
roadmap_revision: 1
status: APPROVED
approval_gate: BLUEPRINT_APPROVED
blueprint_shape: COMPOSITE
submodule: dokter-rawat-inap
blueprint_root: docs/module-blueprints/rawat-inap/dokter-rawat-inap/
capability_scope: [CAP-015, CAP-020, CAP-021, CAP-022, CAP-023, CAP-024, CAP-025]
owners:
  - "Product/Domain: Muhammad Hamzah (RWI-DEC-061)"
  - "Pemilik ClinicalManagement, PharmacyManagement, MasterData: Muhammad Hamzah (RWI-DEC-062)"
  - "Clinical governance: sebagian terisi (RWI-DEC-064); pemilik SLA klinis BELUM ditunjuk"
  - "Security/Privacy: OPEN"
approved_by:
  - "Muhammad Hamzah — Product/Domain owner (RWI-DEC-061), approval desain 2026-09-03"
approved_at: "2026-09-03"
source_sha:
  backend: "93b3227c431401d8f586dec4e1fb25fbf41766e3"
  frontend: "863f24b0d1617069310c04e5770b47fd1b518b5b"
contract_versions: "0.3.0"
input_revisions:
  blueprint-manifest.md (tingkat modul): 5
  blueprint-manifest.md (sub-modul): 5
  00-interview-decisions.md: 10
  01-existing-capability-map.md: 1.3
  02-module-map.md: 1
  evidence/03-hospital-domain-architecture.md: 0.2
  02-backend-architecture.md: 0.3
  04-prd-to-mvp.md: 0.3
input_hashes:
  00-interview-decisions.md: "de786bebc169636c0d7bd254d429a0209809890d78a7f1dcd8220d303fcbecc0"
  01-existing-capability-map.md: "0155b345abea61f1b69e6adaf48ee91056b5efaf7fa672ea6300e0546bf4db03"
  02-module-map.md: "29c761eed6a3fdc3a4d76c2803fde6e956a19784c4b3a14fc27d30e81e5a5d08"
  evidence/03-hospital-domain-architecture.md: "226c6ef1e4bfec544c366b265fe1e4530e80c510da33c1a9eaf2e62161d0b717"
  02-backend-architecture.md: "70b8d7e424e2e95518640ed82168c979badd6e83c6ec25bbcc96b7ab921079c3"
  04-prd-to-mvp.md: "a0d5cc0c998fea5d7c23c587eeec1718e456320c6191eb5ffb752e0f3d79f9cb"
artifact_hashes:
  contracts/api-contract.md: "bbfa035a6607710f1b2bf30f50b7d8899adcc4b214b28734bc04dba19124bbc3"
  contracts/state-transition-matrix.md: "024c330d0ccf5acf4a94ec5c87e7cde6c92626f8b8fdcd0a86dc086aa8a14802"
  contracts/validation-matrix.md: "cf8033eb2634ef63441d2c157546794dc6c0e716f913928d9ff349ef625ccb57"
  contracts/integration-contract.md: "b53f73fc6fc40cd6ed8a265564b9c4f37572aa60396dfc43415fe9042e6e2c5b"
  contracts/permission-audit-matrix.md: "7790bbc230e3a39bdfda93a0862cd81004bb035e0614077a48710cc9f99db5b2"
  testing/acceptance-test-matrix.md: "8c4d0d00d50fa92c690ae2dd23c08ce8a5ea813307a3f2c7f98fb8481ff8c169"
  data/data-dictionary.md: "dff9655378cb9a15a8513154e219df7fec1a84da7c441101194b3373a722086b"
task_id_series: "BE-RWI-037 s.d. BE-RWI-053 — deret bersama seluruh modul, dilanjutkan dari BE-RWI-036"
```

---

## 0. Empat peringatan yang tidak boleh dilewati

> **Pertama: sub-modul ini tidak memiliki satu tabel pun.** Seluruh perubahan di bawah terjadi di
> dalam modul milik orang lain — `ClinicalManagement`, `PharmacyManagement`,
> `LaboratoryManagement`, `RadiologyManagement`. Persetujuan lintas modul sudah ada lewat
> `RWI-DEC-062` untuk tiga modul pertama. **Penjadwalannya tetap wewenang pemilik modul
> masing-masing**, dan roadmap ini tidak menggantikannya.

> **Kedua: dua task pertama tidak menambah satu kemampuan pun, dan keduanya tetap paling depan.**
> `BE-RWI-037` memperbaiki jalur yang hari ini berujung kegagalan sistem; `BE-RWI-038` menutup
> keadaan di mana catatan yang sudah diselesaikan tidak dapat disunting **maupun** dikoreksi.
> Mengerjakan kemampuan baru di atas keduanya berarti membangun di atas lantai yang bolong.

> **Ketiga: setiap task yang menyentuh mesin klinis membawa test regresi poliklinik dan IGD.**
> `RWI-DEC-051` mewajibkannya, dan `RWI-AC-143` mengujinya. Bukti `DOK-TRC-VER-01` menyatakan
> **tidak ditemukan satu pun** test untuk konsultasi, pengkajian, CPPT, tindakan, resep, maupun
> radiologi rawat inap — jadi jaring pengamannya memang belum ada dan wajib dibuat sambil jalan.

> **Keempat: approval desain bukan izin menulis source.** Setiap task tetap menunggu approval task
> tersendiri. QBE preflight dan kesesuaian engineering diselesaikan **pada waktu eksekusi** dari
> `AGENTS.md` repository backend target beserta dokumen engineering canonical — bukan dari roadmap
> ini.

---

## 1. Cara membaca roadmap ini

| Kolom | Artinya |
| --- | --- |
| **Outcome** | Apa yang bisa dilakukan pengguna setelah task selesai, ditulis dari sudut pandang orang yang memakainya |
| **Trace** | Requirement, decision, dan bagian arsitektur yang menjadi asalnya |
| **Kontrak** | Versi kontrak yang mengikat task ini |
| **Reuse** | Kemampuan yang sudah ada dan dipakai ulang, supaya pelaksana tidak menulis ulang yang sudah jalan |
| **Scope** | Berkas dan perubahan yang diharapkan. Bukan daftar tertutup, tetapi batas yang wajar |
| **Dependency** | Task lain yang harus selesai lebih dulu |
| **Acceptance criteria** | Bernomor, dapat diuji, dan tidak memakai kata "dengan benar" |
| **Verification** | Bukti yang diharapkan, termasuk jenis test-nya |
| **Risk/blocker** | Risiko nyata beserta pemiliknya |
| **DoD** | Daftar yang jawabannya hanya "ya" atau "belum" |

Status setiap task ditulis pada kartunya masing-masing memakai empat tanda: ✅ selesai, 🟡 sebagian,
⛔ terblokir, dan tanpa tanda untuk yang belum dikerjakan. Tanda hanya boleh dinaikkan setelah
laporan tracked task itu ada di `../task/report/backend/`.

Per 4 September 2026: `BE-RWI-037`, `BE-RWI-039`, `BE-RWI-044`, dan `BE-RWI-046` ✅ selesai;
`BE-RWI-040`, `BE-RWI-041`, `BE-RWI-042`, `BE-RWI-043`, dan `BE-RWI-045` 🟡 sebagian. Sisanya
belum dikerjakan.

---

## 2. Keadaan awal yang menentukan urutan

| Keadaan | Buktinya | Akibatnya bagi urutan |
| --- | --- | --- |
| Fondasi episode, DPJP berperiode, dan census **sudah ada dan sudah diuji** | `DOK-TRC-CTX-01`; `BE-RWI-001` s.d. `BE-RWI-036` sebagian besar selesai | Tidak ada task fondasi episode di roadmap ini |
| Jalur tanpa antrean **gagal** | `DOK-TRC-DEF-01` | `BE-RWI-037` menjadi task pertama |
| Hanya catatan terpadu yang terdaftar pada mesin keutuhan | `RWI-FACT-014` | `BE-RWI-038` menjadi task kedua |
| Konteks klinis episode belum ada | `DOK-TRC-INT-01` `Missing` | `BE-RWI-039` mendahului seluruh kemampuan klinis |
| Batas satu catatan dan satu resep masih dipaksakan | `DOK-TRC-INT-02` `Extend` | `BE-RWI-043` mendahului catatan harian dan resep |
| Modul Radiologi **sudah ada** | Migration `20260828093000_AddRadiologyManagement` | Radiologi masuk MVP, bukan ditunda |
| Mesin resep, tindakan, lab, radiologi, integritas, dan fakta Billing sudah berjalan | Bagian 4 arsitektur | Sebagian besar task berbentuk `Extend`, bukan `New` |

---

## 3. Slice dan milestone

| Gelombang | Isi | Task | Yang dapat diverifikasi bisnis setelahnya |
| --- | --- | --- | --- |
| **`DOK-MVP-0`** ✅ | Perbaikan jalur tanpa antrean | `BE-RWI-037` ✅ | ✅ selesai 3 September 2026. Catatan untuk pasien tanpa antrean tersimpan, tidak lagi gagal |
| **`DOK-MVP-0b`** | Pendaftaran dokumen ke mesin keutuhan | `BE-RWI-038` | Catatan yang sudah diselesaikan dapat dikoreksi |
| **`DOK-MVP-1`** 🟡 | Fondasi konteks, kolom, tabel visite, pelonggaran | `BE-RWI-039` ✅, `BE-RWI-040` 🟡, `BE-RWI-041` 🟡, `BE-RWI-042` 🟡, `BE-RWI-043` 🟡 | 🟡 satu task ✅ selesai; empat task 🟡 sebagian — seluruhnya menunggu verifikasi PostgreSQL. Penghalang `BE-RWI-043` yang menunggu `BE-RWI-044` **sudah lepas** 4 September 2026: catatan kedua kini terbukti lewat endpoint |
| **`DOK-MVP-2`** 🟡 | Pintu masuk dan kajian medis | `BE-RWI-044` ✅, `BE-RWI-045` 🟡 | 🟡 4 September 2026. Dokter **sudah** menulis catatan dan kajian medis awal tanpa nomor antrean; `BE-RWI-045` 🟡 sebagian karena diagnosis kajian medis belum punya tempat penyimpanan |
| **`DOK-MVP-3`** 🟡 | Catatan harian | `BE-RWI-046` ✅, `BE-RWI-047` | 🟡 4 September 2026. Dokter **sudah** menulis catatan setiap hari dan membacanya sebagai lini masa menurut waktu pemeriksaan; koreksi menunggu `BE-RWI-047` beserta `BE-RWI-038` |
| **`DOK-MVP-4`** | Visite | `BE-RWI-048`, `BE-RWI-049` | Kunjungan dokter tercatat, terhitung, dan dapat dikoreksi |
| **`DOK-MVP-5`** | Resep, tindakan, penunjang | `BE-RWI-050` s.d. `BE-RWI-052` | Dokter meresepkan berulang, mencatat tindakan, memesan lab dan radiologi |
| **`DOK-MVP-6`** | Catatan terpadu dan verifikasi | `BE-RWI-053` | DPJP memverifikasi catatan profesi lain; keterlambatan terpantau |

**Nol gelombang memuat epic `OPEN DECISION`**, karena sub-modul ini memang tidak punya satu pun.

### Urutan dependency

Grafik di bawah adalah **tampilan lain dari kolom `Dependency` pada bagian 4**, bukan sumber
kebenaran baru. Bila keduanya berbeda, kolom `Dependency` pada tabel task yang berlaku.

```text
BE-RWI-037 (perbaikan jalur tanpa antrean)                        ✅ SELESAI
   └── BE-RWI-039 (service konteks klinis)                        ✅ SELESAI
          ├── BE-RWI-040 (kolom konteks pada empat tabel klinis)  🟡 SEBAGIAN ← uji migration PostgreSQL
          │      └── BE-RWI-041 (tabel visite)                    🟡 SEBAGIAN ← dua test PostgreSQL
          └── BE-RWI-042 (konteks pada resep dan pesanan penunjang) 🟡 SEBAGIAN ← uji migration PostgreSQL
                 └── BE-RWI-043 (pelonggaran satu catatan dan satu resep) 🟡 SEBAGIAN ← resep kedua menunggu BE-RWI-050
                        └── BE-RWI-044 (pintu masuk dokter)                 ✅ SELESAI
                               ├── BE-RWI-045 (kajian medis awal)           🟡 SEBAGIAN ← diagnosis belum punya kolom
                               ├── BE-RWI-046 (catatan harian)              ✅ SELESAI
                               │      ├── BE-RWI-047 (koreksi catatan lama) + BE-RWI-038
                               │      └── BE-RWI-053 (verifikasi DPJP)      + BE-RWI-040
                               ├── BE-RWI-048 (visite sebagai kejadian)     + BE-RWI-041
                               │      ├── BE-RWI-049 (pembatalan visite)
                               │      └── BE-RWI-051 (tindakan dokter)      + BE-RWI-040
                               ├── BE-RWI-050 (resep berulang dan obat pulang)
                               └── BE-RWI-052 (lab dan radiologi)

BE-RWI-038 (pendaftaran dokumen ke mesin keutuhan) ─────> BE-RWI-047
```

Tanda `+` berarti **dependency tambahan** yang tidak terlihat dari garis induknya. Contoh:
`BE-RWI-048` digambar di bawah `BE-RWI-044`, tetapi juga menunggu `BE-RWI-041` karena tabel
visitenya lahir di sana.

**Dua akar yang tidak saling menunggu.** `BE-RWI-037` dan `BE-RWI-038` sama-sama tidak punya
dependency dan **boleh dikerjakan orang berbeda sejak hari pertama**. Keduanya baru bertemu di
`BE-RWI-047`. Tabel gelombang menempatkan keduanya berurutan (`DOK-MVP-0` lalu `DOK-MVP-0b`), tetapi
urutan itu urusan penomoran gelombang, bukan dependency teknis.

**Yang boleh paralel.**

| Setelah selesai | Yang lepas bersamaan | Kenapa tidak saling menunggu |
| --- | --- | --- |
| `BE-RWI-039` | `BE-RWI-040`, `BE-RWI-042` | Menyentuh tabel yang berbeda; keduanya hanya butuh service konteks |
| `BE-RWI-044` | `BE-RWI-045`, `BE-RWI-046`, `BE-RWI-048`, `BE-RWI-050`, `BE-RWI-052` | Lima kemampuan berbeda di atas satu pintu masuk yang sama. `BE-RWI-048` menunggu `BE-RWI-041` lebih dulu |
| `BE-RWI-046` | `BE-RWI-047`, `BE-RWI-053` | Koreksi dan verifikasi tidak saling menyentuh. `BE-RWI-047` menunggu `BE-RWI-038` |
| `BE-RWI-048` | `BE-RWI-049`, `BE-RWI-051` | Pembatalan visite dan tindakan dokter berdiri sendiri |

**Tiga task yang menahan paling banyak.** `BE-RWI-039` menahan empat belas task sesudahnya;
`BE-RWI-040` menahan empat jalur berbeda (`041`, `044`, `051`, `053`); `BE-RWI-044` menahan sembilan
task. Keterlambatan pada ketiganya berbiaya jauh lebih besar daripada keterlambatan di mana pun.

**Paralel tidak berarti bebas jadwal.** Seluruh task di atas berjalan di dalam modul milik orang
lain — peringatan pertama bagian 0. Urutan dependency menyatakan **apa yang secara teknis boleh
dimulai**, sedangkan **kapan** dikerjakan tetap wewenang pemilik `ClinicalManagement`,
`PharmacyManagement`, `LaboratoryManagement`, dan `RadiologyManagement`. `BE-RWI-039` dan
`BE-RWI-040` juga dipakai bersama sub-modul `keperawatan` lewat `INT-DOK-09`: siapa pun yang mendarat
lebih dulu membuatnya, dan yang kedua menerima baris dependency, bukan salinan task.

---

## 4. Task

### ✅ `BE-RWI-037` — Catatan dokter untuk pasien tanpa antrean tidak lagi menggagalkan sistem

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 3 September 2026.** Kelima acceptance criteria terbukti; nol perubahan bentuk data. `dotnet build` `0 Error(s)`; `dotnet test` project uji SQLite `Failed: 0, Passed: 219`, enam di antaranya uji khusus task ini termasuk regresi poliklinik, rawat jalan, dan IGD. Selisih yang dilaporkan: kode sukses endpoint adalah `200`, bukan `201` seperti tertulis pada kriteria 1 — bentuk itu sudah ada pada source sebelum task ini dan mengubahnya merusak consumer frontend. Bukti: [laporan](../task/report/backend/BE-RWI-037.md) |
| **Outcome** | Dokter dapat menyimpan catatan untuk pasien yang tidak punya nomor antrean — keadaan normal bagi pasien menginap dan pasien IGD — tanpa permintaannya berujung kegagalan sistem |
| **Trace** | `DOK-TRC-DEF-01`; `FR-DOK-037`; `02-backend-architecture.md` §3.2; `contracts/integration-contract.md` §1.1 |
| **Kontrak** | `0.3.0` |
| **Reuse** | Cabang tanpa antrean yang sudah ada pada pembuatan catatan dokter; pola penolakan yang sudah dipakai jalur IGD |
| **Scope** | `Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs` — melindungi seluruh mutasi data antrean pada cabang tanpa antrean. **Nol perubahan bentuk data** |
| **Dependency** | — |
| **Acceptance criteria** | 1. Membuat catatan untuk kunjungan tanpa baris antrean menghasilkan `201`, bukan `500`. 2. Jumlah baris antrean sebelum dan sesudah permintaan itu **identik**. 3. Jalur IGD lewat cara lamanya tetap `201`. 4. Jalur poliklinik lewat antrean tetap `201`. 5. Tidak ada kolom maupun tabel yang berubah |
| **Verification** | Integration test jalur tanpa antrean; test yang menghitung baris antrean sebelum dan sesudah; **test regresi IGD dan poliklinik** yang wajib lulus |
| **Risk/blocker** | Menyentuh alur yang sedang melayani pasien poliklinik dan IGD, dan menurut `RWI-RISK-002` belum ada jaring pengaman test sama sekali. Test regresi karena itu **bagian dari task ini**, bukan pekerjaan menyusul. Owner: `ClinicalManagement` |
| **DoD** | Kelima acceptance criteria terbukti; empat test hijau; build lulus; laporan menyebut nol perubahan bentuk data |

---

### `BE-RWI-038` — Catatan yang sudah diselesaikan dapat dikoreksi

| Field | Isi |
| --- | --- |
| **Status** | `BELUM DIKERJAKAN` |
| **Outcome** | Dokter yang salah ketik pada catatan yang sudah diselesaikan dapat membetulkannya lewat koreksi beralasan, bukan terpaksa menulis catatan baru yang membantah catatan lama |
| **Trace** | `RWI-DEC-086`, `RWI-DEC-087`, `RWI-RULE-038`, `RWI-FACT-014`; `FR-DOK-044`, `FR-DOK-045`, `FR-DOK-046`; `RWI-AC-157` s.d. `RWI-AC-162`; `02-backend-architecture.md` §4.9.2 |
| **Kontrak** | `0.3.0` |
| **Reuse** | Mesin keutuhan dokumen, addendum, dan pendelegasian penulis milik `MedicalRecordManagement` — dipakai **apa adanya**. Pola pendaftaran yang sudah dipakai catatan terpadu |
| **Scope** | Pendaftaran keutuhan pada finalisasi catatan dokter, kajian medis, dan tindakan. **Nol nilai jenis dokumen baru; nol kolom baru; nol tabel baru** |
| **Dependency** | — |
| **Acceptance criteria** | 1. Memfinalkan catatan dokter mendaftarkannya sebagai dokumen tertanda tangan, dengan penulis dokumen sebagai penanda tangan. 2. Bila pendaftaran gagal, **finalisasi ikut batal**. 3. Menyelesaikan kajian medis dan menandai tindakan dikerjakan berperilaku sama. 4. Koreksi pada dokumen yang sudah final diterima. 5. Koreksi pada dokumen yang **belum** final ditolak `400` beserta arahan menyunting langsung. 6. Catatan terpadu **tidak berubah perilakunya** |
| **Verification** | Integration test per jenis dokumen; test yang memaksa pendaftaran gagal lalu membuktikan finalisasi ikut batal; test koreksi pada dokumen konsep; test regresi catatan terpadu |
| **Risk/blocker** | Pendaftaran dan finalisasi **wajib satu transaksi**. Bila dipisah, akan lahir catatan final yang tidak dapat dikoreksi — persis keadaan yang sedang ditutup. Owner: `ClinicalManagement` |
| **DoD** | Keenam acceptance criteria terbukti; tiga jenis dokumen terdaftar; test transaksi hijau; laporan menyebut nol perubahan bentuk data |

---

### ✅ `BE-RWI-039` — Satu tempat menjawab "dokumen ini milik perawatan yang mana"

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 3 September 2026.** Ketujuh acceptance criteria terbukti; nol tabel dan nol kolom; service terdaftar pada dependency injection. `dotnet build` `0 Error(s)`; enam belas uji khusus task ini hijau, termasuk uji yang menghitung baris antrean sebelum dan sesudah tujuh pemanggilan — sebelum `0`, sesudah `0`. Bukti: [laporan](../task/report/backend/BE-RWI-039.md) |
| **Outcome** | Setiap dokumen klinis rawat inap dapat membuktikan pasien, kunjungan, perawatan, dan kewenangan dokternya — tanpa satu pun baris antrean semu dibuat |
| **Trace** | `INT-DOK-01`; `CON-INP-015`; `INV-DOK-01`, `INV-DOK-02`, `INV-DOK-03`, `INV-DOK-13`; `RWI-RULE-026`; `02-backend-architecture.md` §3.4 |
| **Kontrak** | `0.3.0` |
| **Reuse** | `InpEpisode`, `InpDoctorAssignment` berperiode, dan pemeriksaan dokter aktif per episode yang **sudah ada** dan sudah dipakai jalur perpindahan serta pemulangan |
| **Scope** | Satu service konteks klinis pada `Areas/HealthServices/ClinicalManagement/Services/`; pendaftarannya pada DI. **Nol tabel, nol kolom** |
| **Dependency** | `BE-RWI-037` |
| **Acceptance criteria** | 1. Untuk kunjungan yang punya perawatan berjalan, service mengembalikan pasien, kunjungan, perawatan, status, dan kewenangan dokter. 2. Kunjungan tanpa perawatan rawat inap ditolak `422`. 3. Perawatan berstatus `Draft` ditolak `422`. 4. Perawatan `Closed` atau `Cancelled` ditolak untuk dokumen **baru**. 5. Pasien dokumen yang tidak cocok dengan pasien perawatan ditolak `400`. 6. Dokter yang tidak berwenang atas pasien itu ditolak `403`. 7. **Nol baris antrean dibuat** pada seluruh jalur |
| **Verification** | Unit test per cabang penolakan; integration test yang menghitung baris antrean sebelum dan sesudah; test kewenangan memakai dua dokter berbeda pada satu episode |
| **Risk/blocker** | **Dipakai bersama sub-modul `keperawatan`** lewat `INT-KEP-01`. Service ini dibuat **sekali**; roadmap `keperawatan` kelak menerima baris dependency, **bukan salinan task** — `INT-DOK-09`. Bila `keperawatan` mendarat lebih dulu, task ini berubah menjadi dependency. Owner: `ClinicalManagement` |
| **DoD** | Ketujuh acceptance criteria terbukti; service terdaftar pada DI; laporan menyebut nol tabel dan nol kolom |

---

### 🟡 `BE-RWI-040` — Dokumen klinis menyimpan konteks perawatannya

| Field | Isi |
| --- | --- |
| **Status** | 🟡 **SEBAGIAN, 3 September 2026.** Lima dari enam acceptance criteria terbukti; kriteria 4 — migration maju dan mundur berhasil — **belum terbukti** karena lingkungan kerja tidak memiliki PostgreSQL dan Docker Desktop tidak berjalan. Yang terbukti: tiga belas kolom terbentuk pada empat tabel, SQL migration kedua arah dihasilkan tanpa galat (186 baris maju, 119 baris mundur). Satu migration `20260903092936_AddInpatientClinicalContextColumns`, **belum diterapkan ke database mana pun termasuk lokal**. `dotnet build` `0 Error(s)`; `dotnet test` `Failed: 0, Passed: 219`. Butir DoD "uji maju-mundur lulus" **belum terpenuhi**. Bukti: [laporan](../task/report/backend/BE-RWI-040.md) |
| **Outcome** | Pertanyaan "catatan ini milik perawatan A atau B" dapat dijawab tanpa penelusuran berlapis, dan waktu pemeriksaan terpisah dari waktu penulisan |
| **Trace** | `02-backend-architecture.md` §4.1, §4.2, §4.3, §4.4; `data/data-dictionary.md` §2 s.d. §5; `INV-DOK-01` |
| **Kontrak** | `0.3.0` |
| **Reuse** | Pola kolom nullable beserta konfigurasi EF yang sudah dipakai tabel klinis lain |
| **Scope** | Kolom pada empat tabel `ClinicalManagement`: catatan dokter (3), catatan terpadu (5), tindakan (3), ditambah nilai jenis kajian medis pada enum jenis pengkajian; enum keadaan verifikasi; satu migration |
| **Dependency** | `BE-RWI-039` |
| **Acceptance criteria** | 1. Ketiga belas kolom terbentuk sesuai kamus data, seluruhnya nullable kecuali keadaan verifikasi yang bernilai bawaan tidak-diwajibkan. 2. Index lini masa per perawatan terbentuk. 3. Enum jenis pengkajian bertambah dua nilai kajian medis **tanpa mengubah nilai lama**. 4. Migration maju dan mundur berhasil. 5. Baris lama menerima nilai bawaan dan **tidak disentuh**. 6. Nol kolom milik modul lain di luar keempat tabel ini yang berubah |
| **Dependency lintas sub-modul** | Kolom konteks pada tabel pengkajian **sudah diminta** `keperawatan`. Siapa pun yang mendarat lebih dulu membuatnya; yang kedua memakainya apa adanya — `INT-DOK-09`. Roadmap `keperawatan` belum ada, sehingga task ini **membuatnya bila belum ada**, dan berubah menjadi dependency bila `keperawatan` mendarat lebih dulu |
| **Verification** | Uji migration maju-mundur; pembandingan bentuk kolom terhadap DDL pada kamus data; test yang menghitung jumlah nilai enum |
| **Risk/blocker** | Migration **tidak boleh** diterapkan ke database mana pun selain lokal tanpa izin tertulis. Owner: `ClinicalManagement` |
| **DoD** | Keenam acceptance criteria terbukti; satu migration; uji maju-mundur lulus; laporan menyatakan migration belum diterapkan di luar lokal |

---

### 🟡 `BE-RWI-041` — Kunjungan dokter punya tempat menyimpan

| Field | Isi |
| --- | --- |
| **Status** | 🟡 **SEBAGIAN, 3 September 2026.** Lima dari enam acceptance criteria terbukti; kriteria 6 — migration maju dan mundur berhasil — **belum terbukti**. Dua test PostgreSQL yang diminta sudah **ditulis dan terkompilasi** tetapi **belum dijalankan**: fixture berhenti pada penjagaannya sendiri dengan penanda `BLOCKED_BY_TEST_DB_CONFIGURATION` karena tidak ada database uji yang tersedia. Yang terbukti: tabel bernama `CliPhysicianVisit` — bukan `Trx*`, kunci permintaan unique penuh, kedua index waktu ada, nol unique atas pasangan perawatan-dokter-tanggal, dan 200 nomor bisnis yang dibentuk pada detik yang sama seluruhnya berbeda. Satu migration `20260903093510_AddCliPhysicianVisit`, belum diterapkan. Butir DoD "dua test PostgreSQL hijau" **belum terpenuhi**. Bukti: [laporan](../task/report/backend/BE-RWI-041.md) |
| **Outcome** | Sistem punya tempat mencatat bahwa seorang dokter benar-benar mendatangi pasien, terpisah dari catatan apa pun yang ia tulis |
| **Trace** | `RWI-DEC-084`, `RWI-DEC-085`; `CON-EXT-015`; `02-backend-architecture.md` §4.6; `data/data-dictionary.md` §6 |
| **Kontrak** | `0.3.0` |
| **Reuse** | Pola entity berprefix pemilik yang sudah dipakai `CliClinicalMilestoneFact`; pola alokasi nomor bisnis lewat penyedia seri nomor; pola konfigurasi EF pada `Repositories/Configurations/HealthServices/ClinicalManagement/` |
| **Scope** | `CliPhysicianVisit.cs`; konfigurasi EF-nya; `DbSet`; tiga enum peran dan keadaan; service visite; seri nomor; satu migration |
| **Dependency** | `BE-RWI-040` |
| **Acceptance criteria** | 1. Tabel bernama `CliPhysicianVisit` terbentuk — **bukan** berawalan `Trx`. 2. Kunci permintaan **wajib terisi** dan dijaga unique penuh. 3. Index perawatan-waktu dan dokter-waktu terbentuk. 4. **Tidak ada** unique atas pasangan perawatan, dokter, dan tanggal. 5. Nomor bisnis dialokasikan service lewat penyedia seri nomor, **bukan** Count+1 atau Max+1. 6. Migration maju dan mundur berhasil |
| **Verification** | Uji migration maju-mundur; test yang menyisipkan dua baris berkunci sama dan membuktikan database menolaknya, dijalankan terhadap **PostgreSQL sungguhan**; test yang menyisipkan dua visite dokter yang sama pada tanggal yang sama dan membuktikan keduanya **diterima** |
| **Risk/blocker** | Nama `Trx*` dilarang untuk kode baru oleh `QBE-NAM-001`; revision `0.1` sempat menuliskannya dan itu keliru. Unique atas dokter-per-tanggal **dilarang** oleh `RWI-DEC-085`. Owner: `ClinicalManagement` |
| **DoD** | Keenam acceptance criteria terbukti; satu migration; dua test PostgreSQL hijau; laporan menyebut nama tabel apa adanya |

---

### 🟡 `BE-RWI-042` — Resep dan pesanan penunjang menyimpan konteks perawatan

| Field | Isi |
| --- | --- |
| **Status** | 🟡 **SEBAGIAN, 3 September 2026.** Lima dari enam acceptance criteria terbukti; kriteria 6 — migration maju dan mundur pada ketiga modul — **belum terbukti** karena tidak ada PostgreSQL yang tersedia. Yang terbukti: lima kolom pada tiga tabel milik tiga modul, jenis resep tiga nilai berbawaan `Routine`, dan daftar pesanan laboratorium dapat disaring kunjungan — tanpa penyaring `2` baris, disaring perawatan A `1` baris. Tiga migration terpisah per modul pemilik, **belum diterapkan**. Baris registry `RadiologyManagement / Rad` masih `PLANNED` dan dicatat sebagai **utang terbuka** pada laporan. `dotnet test` `Failed: 0, Passed: 219`. Bukti: [laporan](../task/report/backend/BE-RWI-042.md) |
| **Outcome** | Resep dan pesanan pemeriksaan dapat dibuktikan miliknya perawatan mana, sehingga pesanan perawatan A tidak dapat diproses sebagai milik perawatan B |
| **Trace** | `02-backend-architecture.md` §4.5, §4.7, §4.8; `AC-CAP015-01`; `INV-DOK-01` |
| **Kontrak** | `0.3.0` |
| **Reuse** | Pola kolom nullable pada tabel milik modul lain, sebagaimana `BE-RWI-040` |
| **Scope** | Satu kolom konteks pada resep, satu pada pesanan laboratorium, satu pada pesanan radiologi; jenis resep beserta enumnya; kunci permintaan pada resep; penyaring kunjungan pada daftar pesanan laboratorium; migration **per modul pemilik** |
| **Dependency** | `BE-RWI-039` |
| **Acceptance criteria** | 1. Ketiga kolom konteks terbentuk, seluruhnya nullable. 2. Jenis resep memuat rutin, harian, dan obat pulang, dengan bawaan rutin. 3. Baris resep lama menerima bawaan rutin dan **tidak disentuh**. 4. Daftar pesanan laboratorium dapat disaring kunjungan. 5. Daftar pesanan radiologi **sudah** dapat disaring kunjungan dan tidak diubah. 6. Migration maju dan mundur berhasil pada ketiga modul |
| **Verification** | Uji migration maju-mundur per modul; test penyaring kunjungan pada daftar laboratorium; test yang membuktikan pesanan perawatan A tidak terbaca dari perawatan B |
| **Risk/blocker** | **Prasyarat registry:** baris `RadiologyManagement / Rad` masih berstatus `PLANNED` padahal entity-nya sudah ada. Penambahan kolom pada entity yang sudah ada tidak terhalang, tetapi barisnya **wajib dinaikkan menjadi `ACTIVE`** oleh pemilik registry supaya registry menggambarkan keadaan sebenarnya. Owner: pemilik registry, `PharmacyManagement`, `LaboratoryManagement`, `RadiologyManagement` |
| **DoD** | Keenam acceptance criteria terbukti; tiga migration; baris registry `Rad` sudah dinaikkan atau tercatat sebagai utang terbuka pada laporan |

---

### 🟡 `BE-RWI-043` — Dokter dapat menulis lebih dari satu catatan dan satu resep

| Field | Isi |
| --- | --- |
| **Status** | 🟡 **SEBAGIAN, diperbarui 4 September 2026.** Lima dari enam acceptance criteria terbukti ujung ke ujung. **Kriteria 1 kini terbukti lewat endpoint** setelah `BE-RWI-044` membuka cabang tanpa antrean: dua catatan berturut-turut pada satu perawatan rawat inap keduanya dijawab `200` — `InpatientDoctorEntryPointTests.RawatInap_CatatanKeduaDiterimaLewatEndpoint`, lihat [laporan BE-RWI-044](../task/report/backend/BE-RWI-044.md). **Kriteria 2 masih belum**: resep kedua baru dapat dibuktikan setelah jalur pemesanan resep rawat inap dinyalakan `BE-RWI-050`; hari ini ia terbukti pada aturan aplikasi dan index database saja. Regresi rawat jalan, medical check-up, dan IGD seluruhnya hijau dengan kalimat penolakan dibandingkan **utuh**, bukan sepotong. Satu migration `20260903100128_RelaxSingleConsultationAndPrescriptionForInpatient` mempersempit dua unique index, **belum diterapkan**. Bagian `Emergency` pada `INT-DOK-02` **sengaja belum dikerjakan**; alasan teknisnya ada pada laporan. `dotnet test` `Failed: 0, Passed: 219`. Bukti: [laporan](../task/report/backend/BE-RWI-043.md) |
| **Outcome** | Pasien yang dirawat sepuluh hari menerima catatan harian dan resep sebanyak yang memang dibutuhkan, bukan satu untuk seluruh masa perawatan |
| **Trace** | `INT-DOK-02`; `RWI-DEC-038`, `RWI-DEC-070`, `RWI-RULE-026` aturan 4 dan 5; `INV-DOK-04`, `INV-DOK-05`; `FR-DOK-002`, `FR-DOK-003` |
| **Kontrak** | `0.3.0` |
| **Reuse** | Penyaring tipe kunjungan yang sudah dipakai pelonggaran IGD |
| **Scope** | Pelonggaran batas satu catatan per kunjungan pada `ClinicalManagement`; pelonggaran batas satu resep aktif pada `PharmacyManagement`. Keduanya **disaring tipe kunjungan** |
| **Dependency** | `BE-RWI-039`, `BE-RWI-042` |
| **Acceptance criteria** | 1. Catatan kedua pada satu kunjungan rawat inap diterima. 2. Resep kedua sepanjang perawatan diterima. 3. Catatan kedua pada kunjungan **rawat jalan** tetap ditolak **dengan kode dan kalimat yang sama persis** seperti sebelum perubahan. 4. Resep aktif kedua pada kunjungan rawat jalan tetap ditolak sama persis. 5. Perilaku medical check-up tidak berubah. 6. Perilaku IGD tetap berjalan |
| **Verification** | Integration test rawat inap; **test regresi rawat jalan yang membandingkan kode dan kalimat penolakan** terhadap perilaku sebelum perubahan; test regresi IGD dan medical check-up |
| **Risk/blocker** | Ini perubahan paling berisiko pada seluruh roadmap: ia menyentuh alur poliklinik yang sedang melayani pasien, dan `RWI-RISK-002` mencatat belum ada jaring pengaman. `RWI-AC-143` adalah penjaganya. Owner: `ClinicalManagement`, `PharmacyManagement` |
| **DoD** | Keenam acceptance criteria terbukti; test regresi rawat jalan dan medical check-up hijau; laporan mencantumkan kalimat penolakan sebelum dan sesudah, berdampingan |

---

### ✅ `BE-RWI-044` — Dokter membuka pasien rawat inap dan menulis tanpa nomor antrean

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 4 September 2026.** Kelima acceptance criteria terbukti. `dotnet build` `0 Error(s)`; `dotnet test` project uji SQLite `Failed: 0, Passed: 262` — 16 di antaranya uji khusus task ini termasuk regresi poliklinik, medical check-up, dan IGD; project uji InMemory menambahkan 19 uji hak akses **peran non-SuperAdmin**. **Nol migration** dan nol perubahan bentuk data. Selisih yang dilaporkan: kalimat penolakan `VAL-DOK-04` diperbarui karena kalimat lama berhenti benar begitu pintu rawat inap dibuka — kode penolakannya tetap `400`. `VAL-DOK-06` sengaja belum ditegakkan; alasannya pada laporan. Bukti: [laporan](../task/report/backend/BE-RWI-044.md) |
| **Outcome** | Dokter dapat mulai mendokumentasikan pasien menginap langsung dari konteks perawatannya, dan hak akses barunya benar-benar berfungsi bagi peran selain SuperAdmin |
| **Trace** | `FR-DOK-001`, `FR-DOK-038`; `EPIC DOK-01`; `contracts/api-contract.md` §1, §2; `permission-audit-matrix.md` §1.1 |
| **Kontrak** | `0.3.0` |
| **Reuse** | Service konteks klinis dari `BE-RWI-039`; mesin hak akses dan penyaring yang sudah ada |
| **Scope** | Memasang service konteks pada pembuatan catatan dokter dan pengkajian; penolakan penanda perawatan yang tidak cocok; pendaftaran butir hak akses baru pada seeder |
| **Dependency** | `BE-RWI-039`, `BE-RWI-040`, `BE-RWI-043` |
| **Acceptance criteria** | 1. Catatan dokter dapat dibuat untuk perawatan berjalan tanpa antrean dan tanpa kunjungan IGD. 2. Penanda perawatan yang terisi tetapi tidak cocok dengan kunjungannya ditolak `400`. 3. Catatan dapat dibuat walaupun pengkajian awal keperawatan belum selesai. 4. Seluruh endpoint yang tersentuh dapat dipanggil peran **non-SuperAdmin** yang berhak. 5. Nama pada penanda aksi dan penanda hak akses **sama persis** |
| **Verification** | Integration test per acceptance criteria; **test hak akses memakai peran non-SuperAdmin** untuk setiap endpoint baru |
| **Risk/blocker** | `BE-RWI-034` pernah mengunci sembilan endpoint karena nama pada kedua penanda berbeda, dan menahan tujuh task frontend. Owner: `ClinicalManagement`, Platform |
| **DoD** | Kelima acceptance criteria terbukti; test peran non-SuperAdmin hijau; laporan mencantumkan pasangan nama penanda apa adanya |

---

### 🟡 `BE-RWI-045` — Kajian medis awal tersimpan terpisah dari catatan harian

| Field | Isi |
| --- | --- |
| **Status** | 🟡 **SEBAGIAN, 4 September 2026.** Lima dari enam acceptance criteria terbukti. Kriteria 4 — penyelesaian tanpa diagnosis ditolak `400` beserta daftar bagian yang kosong — **belum terpenuhi seluruhnya**: mekanismenya berjalan dan daftar bagian benar-benar dikembalikan, tetapi **diagnosis, pemeriksaan fisik, dan rencana terapi tidak punya kolom** pada `TrxPatientAssessment`, sedangkan `data/data-dictionary.md` bagian 3 menyatakan sub-modul ini menambahkan **nol** kolom pada tabel itu. Tiga pilihan keputusan struktur diajukan pada laporan dan menunggu Product/Domain bersama `ClinicalManagement`. `dotnet test` project uji SQLite `Failed: 0, Passed: 262`, 15 di antaranya uji khusus task ini. **Nol migration.** Butir DoD "keenam acceptance criteria terbukti" **belum terpenuhi**. Bukti: [laporan](../task/report/backend/BE-RWI-045.md) |
| **Outcome** | DPJP mengisi pemeriksaan menyeluruh pertama sebagai dokumen tersendiri, dan catatan harian berikutnya tidak pernah menimpanya |
| **Trace** | `EPIC DOK-02`; `FR-DOK-006` s.d. `FR-DOK-011`; `AC-CAP022-02`; `02-backend-architecture.md` §4.2 |
| **Kontrak** | `0.3.0` |
| **Reuse** | Tabel pengkajian yang sudah ada beserta mesin statusnya; jenis kajian dari `BE-RWI-040` |
| **Scope** | Pembuatan dan penyelesaian kajian medis memakai jenis kajian medis; pembacaan kajian per perawatan; kewenangan menulis bercabang menurut jenis |
| **Dependency** | `BE-RWI-044` |
| **Acceptance criteria** | 1. Kajian medis dan catatan harian tersimpan sebagai record berbeda dengan mesin status yang berjalan sendiri. 2. Menulis tiga catatan harian **tidak mengubah satu huruf pun** isi kajian medis. 3. Satu perawatan memiliki paling banyak satu kajian medis yang berlaku. 4. Menyelesaikan kajian tanpa diagnosis ditolak `400` beserta daftar bagian yang kosong. 5. Perawat ditolak `403` saat mencoba membuat kajian medis. 6. Kajian yang selesai terdaftar pada mesin keutuhan |
| **Verification** | Integration test per acceptance criteria; test yang membandingkan isi kajian sebelum dan sesudah tiga catatan harian ditulis |
| **Risk/blocker** | Berbagi satu tabel dengan pengkajian keperawatan berarti mesin hak akses melihat **satu** sumber daya untuk dua jenis dokumen; pembedaannya dijaga aturan bisnis, bukan hak akses. Bila pemilik kelak memilih bentuk penyimpanan terpisah, task ini berubah. Owner: Product/Domain bersama `ClinicalManagement` |
| **DoD** | Keenam acceptance criteria terbukti; test pemisahan isi hijau; laporan menyebut jenis kajian yang dipakai |

---

### ✅ `BE-RWI-046` — Catatan harian terbaca menurut waktu pemeriksaan yang sebenarnya

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 4 September 2026.** Kelima acceptance criteria terbukti. `dotnet test` project uji SQLite `Failed: 0, Passed: 262`, 12 di antaranya uji khusus task ini. **Nol migration** — kolom dan index-nya sudah dibuat `BE-RWI-040`. Selisih yang dilaporkan: syarat penyelesaian catatan yang berkonteks perawatan dilonggarkan menjadi `VAL-DOK-12`, yaitu cukup satu bagian S/O/A/P terisi dan diagnosis utama tidak diwajibkan; catatan poliklinik tetap menuntut keempat bagian dan diagnosis, dijaga test regresi `CatatanTanpaKonteksPerawatan_TetapMenuntutSoapLengkapDanDiagnosis`. Bukti: [laporan](../task/report/backend/BE-RWI-046.md) |
| **Outcome** | Lini masa perkembangan pasien menggambarkan urutan pemeriksaan yang sungguh terjadi, bukan urutan kapan dokter sempat mengetik |
| **Trace** | `EPIC DOK-03`; `FR-DOK-012`, `FR-DOK-013`, `FR-DOK-014`; `contracts/api-contract.md` §1 |
| **Kontrak** | `0.3.0` |
| **Reuse** | Isi SOAP yang **sudah ada** di dalam catatan dokter; penyimpanan otomatis yang sudah berjalan |
| **Scope** | Pengisian waktu pemeriksaan; pembacaan lini masa per perawatan terurut waktu pemeriksaan; validasi batas waktu |
| **Dependency** | `BE-RWI-044` |
| **Acceptance criteria** | 1. Catatan yang ditulis pukul 11.00 untuk pemeriksaan pukul 07.40 menempati urutan pukul 07.40. 2. Waktu pemeriksaan melewati waktu sekarang ditolak `400`. 3. Waktu pemeriksaan sebelum pasien masuk kamar ditolak `400`. 4. Beberapa catatan sepanjang perawatan terbaca sebagai lini masa terurut. 5. Menyelesaikan catatan dengan keempat bagian kosong ditolak `400` |
| **Verification** | Integration test urutan lini masa memakai tiga catatan berwaktu berbeda; test batas waktu; test bagian kosong |
| **Risk/blocker** | Waktu pemeriksaan **wajib** boleh diisi mundur; memaksanya sama dengan waktu penulisan membuat lini masa menyesatkan. Owner: `ClinicalManagement` |
| **DoD** | Kelima acceptance criteria terbukti; test urutan hijau |

---

### `BE-RWI-047` — Catatan lama tetap dapat dibetulkan, termasuk setelah pasien pulang

| Field | Isi |
| --- | --- |
| **Status** | `BELUM DIKERJAKAN` |
| **Outcome** | Kesalahan tulis dapat dibetulkan kapan pun tanpa mengubah isi aslinya, dan tanpa membuka kembali perawatan yang sudah ditutup |
| **Trace** | `FR-DOK-015`, `FR-DOK-016`, `FR-DOK-047`, `FR-DOK-048`; `RWI-DEC-088`; `RWI-AC-161`, `RWI-AC-163` s.d. `RWI-AC-167`; `permission-audit-matrix.md` §3 |
| **Kontrak** | `0.3.0` |
| **Reuse** | Mesin addendum dan penetapan penulis pengganti milik `MedicalRecordManagement` — **nol perubahan model** |
| **Scope** | Penjaga kewenangan per pasien pada jalur koreksi atas nama penulis lain; penolakan dokumen baru pada perawatan tertutup; pendaftaran butir hak akses koreksi dan penetapan |
| **Dependency** | `BE-RWI-038`, `BE-RWI-046` |
| **Acceptance criteria** | 1. Perawatan tertutup menolak catatan **baru** `422`. 2. Perawatan tertutup **menerima** koreksi, dan statusnya tetap tertutup, tempat tidurnya tidak berubah, lama dirawatnya tidak bergeser. 3. Setelah penetapan berhalangan berlaku, DPJP aktif perawatan itu dapat mengoreksi catatan dokter yang berhalangan. 4. Koreksi atas nama dokter lain **tidak mengubah penulis catatan aslinya**. 5. Dokter yang **bukan** DPJP aktif perawatan itu ditolak `403` walaupun butir hak akses penggantinya ada dan penetapannya berlaku. 6. Penetapan tanpa masa berlaku ditolak `400` |
| **Verification** | Integration test per acceptance criteria; **test khusus untuk nomor 5** yang membuktikan seluruh pemeriksaan hak akses lolos dan penolakannya datang dari aturan bisnis |
| **Risk/blocker** | Penetapan berhalangan bersifat **milik penulis**, tidak menyebut penggantinya. Pembatasan "hanya DPJP aktif" karena itu **tidak dapat** dijaga mesin hak akses dan wajib berada di dalam perintah bisnis. Test yang hanya menguji hak akses **tidak akan menangkapnya**. Owner: `ClinicalManagement` |
| **DoD** | Keenam acceptance criteria terbukti; test nomor 5 hijau beserta catatan bahwa hak aksesnya lolos; laporan menyebut nol perubahan pada `MedicalRecordManagement` |

---

### `BE-RWI-048` — Kunjungan dokter tercatat sebagai kejadian tersendiri

| Field | Isi |
| --- | --- |
| **Status** | `BELUM DIKERJAKAN` |
| **Outcome** | Dokter yang mendatangi pasien dapat mencatat kunjungannya walaupun belum sempat menulis apa pun, dan tombol yang tertekan dua kali tidak melahirkan dua kunjungan |
| **Trace** | `EPIC DOK-05`; `FR-DOK-023` s.d. `FR-DOK-028`, `FR-DOK-039`; `RWI-AC-150` s.d. `RWI-AC-155`; `contracts/api-contract.md` §4 |
| **Kontrak** | `0.3.0` |
| **Reuse** | Tabel dan service visite dari `BE-RWI-041`; pola kunci permintaan yang sudah dipakai penerbitan fakta klinis |
| **Scope** | Pencatatan visite; pembacaan riwayat per perawatan; penautan dokumen opsional; butir hak akses visite |
| **Dependency** | `BE-RWI-041`, `BE-RWI-044` |
| **Acceptance criteria** | 1. Visite pukul 07.40 muncul pada riwayat walaupun catatannya baru ditulis pukul 07.52 **atau tidak ditulis sama sekali**. 2. Tiga catatan tanpa satu pun kejadian visite menghasilkan hitungan **nol**. 3. Dua pengiriman berkunci sama menghasilkan **satu** kejadian dengan identitas sama, kode `200` pada yang kedua. 4. Dua visite nyata pada tanggal yang sama menghasilkan **dua** baris dan hitungan **dua**. 5. Kunci permintaan kosong ditolak `400`. 6. Perawat ditolak `403`. 7. Riwayat menampilkan perawatan, dokter, peran, waktu, pencatat, dan tautan bila ada |
| **Verification** | Integration test per acceptance criteria; **dua permintaan bersamaan berkunci sama terhadap PostgreSQL sungguhan** — provider InMemory tidak dapat membuktikan unique index |
| **Risk/blocker** | Menghitung visite dari catatan **dilarang** `INV-DOK-07`. Menolak visite kedua pada hari yang sama **dilarang** `RWI-DEC-085`. Owner: `ClinicalManagement` |
| **DoD** | Ketujuh acceptance criteria terbukti; test concurrency PostgreSQL hijau; laporan mencantumkan hitungan pada keenam keadaan pada matriks status §5.3 |

---

### `BE-RWI-049` — Kunjungan yang salah catat dapat dibatalkan tanpa menghilangkan jejaknya

| Field | Isi |
| --- | --- |
| **Status** | `BELUM DIKERJAKAN` |
| **Outcome** | Dokter yang salah mengisi jam dapat membetulkannya, dan auditor tetap dapat melihat bahwa pernah ada catatan yang dibatalkan beserta alasannya |
| **Trace** | `FR-DOK-040`, `FR-DOK-041`; `INV-DOK-08`, `INV-DOK-09`; `RWI-AC-156`; `contracts/state-transition-matrix.md` §5.2 |
| **Kontrak** | `0.3.0` |
| **Reuse** | Kolom pembatalan dan penunjuk kejadian pengganti dari `BE-RWI-041` |
| **Scope** | Pembatalan kejadian beserta alasan; penunjuk kejadian yang digantikan; penyaringan hitungan; butir hak akses pembatalan |
| **Dependency** | `BE-RWI-048` |
| **Acceptance criteria** | 1. Pembatalan tanpa alasan ditolak `400`. 2. Kejadian yang dibatalkan **tetap tersimpan** dan tetap tampil pada riwayat beserta alasannya. 3. Hitungan hanya menghitung kejadian yang tidak dibatalkan. 4. Membatalkan kejadian yang sudah batal ditolak `409`. 5. Pencatatan ulang setelah pembatalan menunjuk kejadian yang digantikannya. 6. **Tidak ada** jalur yang menyunting waktu atau peran kejadian. 7. Agregasi tagihan tidak mengubah, menggabungkan, maupun menghapus kejadian klinis |
| **Verification** | Integration test per acceptance criteria; **architecture test** yang membuktikan tidak ada endpoint penyuntingan waktu maupun peran visite |
| **Risk/blocker** | Penyuntingan di tempat **dilarang** `RWI-DEC-085`; menyediakannya berarti membatalkan keputusan pemilik. Owner: `ClinicalManagement` |
| **DoD** | Ketujuh acceptance criteria terbukti; architecture test hijau; laporan menunjukkan riwayat berisi baris batal beserta alasannya |

---

### `BE-RWI-050` — Resep berulang dan obat pulang sepanjang perawatan

| Field | Isi |
| --- | --- |
| **Status** | `BELUM DIKERJAKAN` |
| **Outcome** | Dokter meresepkan setiap hari sesuai kebutuhan, dan resep yang dibawa pulang dikenali petugas farmasi di layar mereka sendiri |
| **Trace** | `EPIC DOK-06`; `FR-DOK-029` s.d. `FR-DOK-032`; `RWI-RULE-024`, `RWI-DEC-046`; `AC-CAP023-03` |
| **Kontrak** | `0.3.0` |
| **Reuse** | Mesin resep `PharmacyManagement` yang **sudah lengkap** — resep, item, racikan, review, penyiapan, ruang kerja farmasi |
| **Scope** | Pembuatan resep dari konteks perawatan; jenis obat pulang; kunci permintaan; pembacaan status pemenuhan |
| **Dependency** | `BE-RWI-042`, `BE-RWI-043`, `BE-RWI-044` |
| **Acceptance criteria** | 1. Lima resep pada satu perawatan lima hari tersimpan seluruhnya. 2. Resep obat pulang tersaring tersendiri menurut jenisnya. 3. Pengiriman berulang berkunci sama tidak melahirkan resep ganda. 4. Status pemenuhan dapat **dibaca** kembali. 5. Percobaan menandai obat sudah diserahkan dari sub-modul ini ditolak `403`. 6. **Nol jalur tulis** menuju status pemenuhan |
| **Verification** | Integration test per acceptance criteria; **architecture test** yang memindai endpoint dan service dan menemukan nol penulisan status pemenuhan |
| **Risk/blocker** | Menandai obat diserahkan **dilarang** `RUL-DOK-01`; menambahkannya kelak berarti melanggar batas kepemilikan, bukan melengkapi fitur. Owner: `PharmacyManagement` |
| **DoD** | Keenam acceptance criteria terbukti; architecture test hijau |

---

### `BE-RWI-051` — Tindakan dokter tercatat dan tagihannya tidak pernah ganda

| Field | Isi |
| --- | --- |
| **Status** | `BELUM DIKERJAKAN` |
| **Outcome** | Tindakan yang dikerjakan tercatat pada rekam medis pasien, dan kegagalan sistem keuangan tidak pernah menghapus bukti bahwa tindakan itu terjadi |
| **Trace** | `FR-DOK-033`, `FR-DOK-034`; `INV-DOK-09`; `AC-CAP024-01`, `AC-CAP024-02`; `contracts/integration-contract.md` §3 |
| **Kontrak** | `0.3.0` |
| **Reuse** | Mesin tindakan beserta status, tarif, dan penerbitan fakta klinis ke Billing yang **sudah berjalan** |
| **Scope** | Konteks perawatan pada tindakan; tautan visite opsional; kunci permintaan; urutan simpan-lalu-terbitkan |
| **Dependency** | `BE-RWI-040`, `BE-RWI-044`, `BE-RWI-048` |
| **Acceptance criteria** | 1. Tindakan untuk pasangan pasien dan kunjungan yang tidak cocok ditolak `400`. 2. Percobaan ulang tidak menghasilkan tindakan maupun fakta klinis ganda. 3. Saat Billing gagal dihubungi, catatan tindakan **tetap tersimpan** dan hasil penerbitannya tercatat gagal. 4. Kedua jalur pencatatan dipertahankan — direncanakan lebih dulu, atau langsung dicatat dikerjakan. 5. Tautan ke kejadian visite bersifat opsional |
| **Verification** | Integration test terhadap PostgreSQL untuk percobaan ulang; test yang mematikan jalur Billing lalu membuktikan catatan klinis tetap ada |
| **Risk/blocker** | Urutan **tidak boleh dibalik**: catatan klinis disimpan lebih dulu, fakta diterbitkan sesudahnya. Owner: `ClinicalManagement` |
| **DoD** | Kelima acceptance criteria terbukti; test kegagalan Billing hijau |

---

### `BE-RWI-052` — Pemeriksaan laboratorium dan radiologi dipesan dan hasilnya dibaca

| Field | Isi |
| --- | --- |
| **Status** | `BELUM DIKERJAKAN` |
| **Outcome** | Dokter memesan pemeriksaan dari konteks pasiennya dan membaca hasil yang sudah disahkan, tanpa satu baris salinan hasil pun disimpan Rawat Inap |
| **Trace** | `CAP-015`; `FR-DOK-035`, `FR-DOK-036`, `FR-DOK-042`, `FR-DOK-043`; `INV-DOK-12`; `AC-CAP015-01`, `AC-CAP015-02` |
| **Kontrak** | `0.3.0` |
| **Reuse** | Mesin pesanan laboratorium dan radiologi yang **sudah berjalan**, termasuk studi, modalitas, dan lifecycle pesanan |
| **Scope** | Pemesanan membawa konteks perawatan; pembacaan pesanan dan hasil final per perawatan; penanda hasil belum final |
| **Dependency** | `BE-RWI-042`, `BE-RWI-044` |
| **Acceptance criteria** | 1. Pesanan laboratorium perawatan A tidak dapat diproses sebagai milik perawatan B — ditolak `400`. 2. Hal yang sama berlaku untuk pesanan radiologi. 3. Hasil final terbaca dari konteks pasien **tanpa tabel salinan**. 4. Hasil yang belum final ditampilkan dengan penanda dan tidak disajikan sebagai hasil sah. 5. Hasil milik kunjungan di luar perawatan yang dibuka **tidak ikut tampil**. 6. Percobaan menulis hasil dari sub-modul ini ditolak `403` |
| **Verification** | Integration test per acceptance criteria; **architecture test** yang membuktikan nol tabel hasil baru dan nol jalur tulis hasil |
| **Risk/blocker** | Menyalin hasil **dilarang** `RUL-DOK-02`. Hasil yang basi di layar dokter adalah risiko keselamatan, bukan masalah tampilan. Owner: `LaboratoryManagement`, `RadiologyManagement` |
| **DoD** | Keenam acceptance criteria terbukti; architecture test hijau |

---

### `BE-RWI-053` — DPJP memverifikasi catatan profesi lain, dan keterlambatannya terpantau

| Field | Isi |
| --- | --- |
| **Status** | `BELUM DIKERJAKAN` |
| **Outcome** | DPJP dapat menyatakan sudah membaca catatan perawat dan profesi lain, tanpa verifikasi itu menahan satu pun pelayanan |
| **Trace** | `EPIC DOK-04`; `FR-DOK-017` s.d. `FR-DOK-022`; `INV-DOK-11`; `AC-CAP021-03`; `contracts/state-transition-matrix.md` §3 |
| **Kontrak** | `0.3.0` |
| **Reuse** | Catatan terpadu yang sudah ada beserta profesi dan penulisnya; kolom verifikasi dari `BE-RWI-040` |
| **Scope** | Verifikasi oleh DPJP aktif; keadaan menunggu, terverifikasi, dan lewat batas; daftar catatan yang menunggu; kebijakan kosong |
| **Dependency** | `BE-RWI-040`, `BE-RWI-046` |
| **Acceptance criteria** | 1. Verifikasi **tidak mengubah** penulis asli; verifikator tersimpan terpisah. 2. Verifikator wajib DPJP yang **aktif saat verifikasi**, bukan yang aktif saat catatan ditulis. 3. Dokter jaga yang bukan DPJP ditolak `403`. 4. Kebijakan verifikasi kosong berarti seluruh catatan tidak diwajibkan dan daftar pantau kosong, sedangkan pencatatan berjalan penuh. 5. Catatan yang lewat batas muncul pada daftar pantau dan **tidak menahan** penulisan catatan berikutnya. 6. Koreksi catatan terverifikasi mengembalikannya ke menunggu verifikasi |
| **Verification** | Integration test per acceptance criteria; test pergantian DPJP yang membuktikan DPJP lama ditolak dan DPJP baru diterima; test kebijakan kosong |
| **Risk/blocker** | Nilai batas waktunya **belum disahkan** — `RWI-RULE-021` menunggu pemilik klinis yang belum ditunjuk. Mekanismenya dibangun dengan kebijakan kosong; **jangan menanam angka apa pun**. Owner: Clinical Governance |
| **DoD** | Keenam acceptance criteria terbukti; test kebijakan kosong hijau; laporan menyatakan nol angka batas waktu ditanam di kode |

---

## 4.1 Register status task

Tabel ini adalah ringkasan status seluruh task pada bagian 4, beserta tautan laporan tracked-nya.
Bila tabel ini berbeda dari baris `Status` di dalam kartu task, **kartu task yang berlaku**.

| Task | Judul singkat | Status | Laporan |
| --- | --- | :---: | --- |
| `BE-RWI-037` | Perbaikan jalur tanpa antrean | ✅ | [BE-RWI-037](../task/report/backend/BE-RWI-037.md) |
| `BE-RWI-038` | Pendaftaran dokumen ke mesin keutuhan | | — |
| `BE-RWI-039` | Service konteks klinis | ✅ | [BE-RWI-039](../task/report/backend/BE-RWI-039.md) |
| `BE-RWI-040` | Kolom konteks pada empat tabel klinis | 🟡 5 dari 6 kriteria | [BE-RWI-040](../task/report/backend/BE-RWI-040.md) |
| `BE-RWI-041` | Tabel kejadian visite | 🟡 5 dari 6 kriteria | [BE-RWI-041](../task/report/backend/BE-RWI-041.md) |
| `BE-RWI-042` | Konteks pada resep dan pesanan penunjang | 🟡 5 dari 6 kriteria | [BE-RWI-042](../task/report/backend/BE-RWI-042.md) |
| `BE-RWI-043` | Pelonggaran satu catatan dan satu resep | 🟡 5 dari 6 kriteria | [BE-RWI-043](../task/report/backend/BE-RWI-043.md) |
| `BE-RWI-044` | Pintu masuk dokter rawat inap | ✅ | [BE-RWI-044](../task/report/backend/BE-RWI-044.md) |
| `BE-RWI-045` | Kajian medis awal | 🟡 5 dari 6 kriteria | [BE-RWI-045](../task/report/backend/BE-RWI-045.md) |
| `BE-RWI-046` | Catatan harian menurut waktu pemeriksaan | ✅ | [BE-RWI-046](../task/report/backend/BE-RWI-046.md) |
| `BE-RWI-047` s.d. `BE-RWI-053` | Tujuh task gelombang berikutnya | | — |

---

## 5. Gerbang yang masih terbuka

Tidak satu pun menahan roadmap ini disusun, tetapi seluruhnya menahan **rilis**.

| Gerbang | Sifat | Pemilik | Menahan apa |
| --- | --- | --- | --- |
| Nilai batas waktu kajian medis dan verifikasi catatan terpadu — `RWI-RULE-021` | Belum disahkan | Pemilik klinis, **belum ditunjuk** | Rilis produksi. Mekanismenya tetap dibangun `BE-RWI-053` |
| Baris registry `RadiologyManagement / Rad` masih `PLANNED` | Selisih registry terhadap source | Pemilik registry | Kerapian registry; **tidak** menahan `BE-RWI-042`. **Masih terbuka** per 3 September 2026: `BE-RWI-042` selesai dikerjakan tanpa menaikkannya, dan utangnya tercatat pada [laporan BE-RWI-042](../task/report/backend/BE-RWI-042.md) |
| Rework ruang kerja dokter | `Conflict` | Frontend authority | **Rilis apa pun** — lihat roadmap frontend |
| Kebijakan pencatatan visite atas nama dokter | Belum ada | Clinical Governance | Kemampuan itu saja; bawaan aman sudah berlaku |
| Kebijakan agregasi tarif visite | Belum ada | Pemilik Billing | Penagihan visite; **tidak** menahan pencatatan klinis |
| Penjadwalan pekerjaan di modul milik orang lain | Belum dijadwalkan | Pemilik masing-masing modul | Waktu mulai setiap task; persetujuannya sudah ada lewat `RWI-DEC-062` untuk tiga modul |
| **Verifikasi PostgreSQL bagi lima migration `DOK-MVP-1`** ★ baru 3 September 2026 | Lingkungan kerja tanpa PostgreSQL | Pemilik modul yang menerapkan migration | Menaikkan `BE-RWI-040`, `BE-RWI-041`, dan `BE-RWI-042` menjadi ✅. Kelima migration sudah ada dan SQL kedua arahnya dihasilkan tanpa galat, tetapi belum pernah dijalankan |
| **Tempat menyimpan diagnosis, pemeriksaan fisik, dan rencana terapi kajian medis** ★ baru 4 September 2026 | Pertentangan antar dokumen kontrak yang sudah disetujui | Product/Domain bersama `ClinicalManagement` | Menaikkan `BE-RWI-045` menjadi ✅. `VAL-DOK-10` dan `VAL-DOK-11` menuntut ketiga bagian itu, sedangkan `data/data-dictionary.md` bagian 3 menyatakan **nol** kolom baru pada `TrxPatientAssessment`. Tiga pilihan penyelesaiannya ada pada [laporan BE-RWI-045](../task/report/backend/BE-RWI-045.md) bagian 6.1 |
| **Pelonggaran `Emergency` pada `INT-DOK-02`** ★ baru 3 September 2026 | Belum dikerjakan; alasan teknis tercatat | Pemilik `PharmacyManagement` | Kelengkapan `INT-DOK-02`. `BE-RWI-043` melonggarkan `Inpatient` saja karena `TrxPrescription` tidak memiliki kolom pembeda resep IGD; rinciannya pada [laporan BE-RWI-043](../task/report/backend/BE-RWI-043.md) |

---

## 6. Yang sengaja tidak ada di roadmap ini

| Yang tidak ada | Alasan |
| --- | --- |
| Task pembuatan tabel milik Rawat Inap | Sub-modul ini **nol tabel** — `RWI-DEC-081` |
| Task untuk sub-modul `keperawatan` | Statusnya masih `draft`; sub-modul yang belum disetujui tidak masuk roadmap mana pun |
| Salinan task service konteks klinis untuk `keperawatan` | Service dibuat **sekali** pada `BE-RWI-039`; roadmap `keperawatan` menerima baris dependency — `INT-DOK-09` |
| Task penagihan visite | Kebijakan agregasi milik Billing belum ada |
| Task pencatatan visite atas nama dokter | Kebijakannya belum ada; bawaan aman berlaku |
| Task pembacaan balik penyerahan obat pulang | Kontrak status final Farmasi belum disetujui pemiliknya |
| Task resume pulang | `CAP-026` milik `episode-rawat-inap` |
| Task perapian entity legacy berawalan `Trx` | Utang teknis milik modul lain; **task tersendiri** dengan approval pemilik arsitektur backend, bukan diselipkan ke sini |
