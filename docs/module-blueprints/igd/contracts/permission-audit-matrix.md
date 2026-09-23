# Matriks Hak Akses dan Audit — Modul IGD

| Field | Nilai |
| --- | --- |
| `contract_version` | `0.6.0` — penutupan kunjungan lewat disposisi, 23 September 2026, **Rencana (belum tersedia)**, status `draft`: bagian 8 baru (penutupan lewat disposisi, `IGD-DEC-163`…`169`). **Aditif** — nol resource dan nol aksi baru. Sebelumnya `0.5.0` — encounter-first, 22 September 2026, **Rencana (belum tersedia)**. **Aditif**: bagian 7 baru — aksi `EmergencyVisit : NoShow`, resource baru `EmergencyEncounterReconciliation` (`Read`/`Process`/`Reverse`), jejak audit override, NoShow, waktu tiba, rekonsiliasi. Sebelumnya `0.4.0` — bagian 3.1 (kewenangan atas pesanan) ditambahkan correction pass revisi 6. **Aditif** |
| Status | `draft`, **kecuali bagian 7 (encounter-first) yang `approved`** (`IGD-DEC-157`, 22 September 2026). Bagian 8 **`approved`** (`IGD-DEC-170`, 23 September 2026) |
| Owner | Product/Domain Owner IGD: **Rizki Gunawan** (`IGD-DEC-089`) |
| `approved_by` / `approved_at` | **Rizki Gunawan / 2026-09-22** — bagian 7 (encounter-first) lewat `IGD-DEC-157`; keterbatasan izin bersama pada §7.1 diterima lewat `IGD-DEC-158`. Bagian lain tetap `draft` |
| Versi sebelumnya | `0.3.0`, sebelumnya `0.2.0` |

---

## 1. Dua lapis kewenangan

`IGD-DEC-058` dan `IGD-DEC-086` menetapkan dua lapis yang **saling melengkapi dan tidak
saling menggantikan**:

| Lapis | Menjawab | Ditegakkan oleh |
| --- | --- | --- |
| Kemampuan | "Boleh melakukan tindakan jenis ini?" | `[AccessPermission("Resource", "Action")]` pada endpoint |
| Unit | "Boleh melakukannya pada unit ini?" | `EmergencyUnitAuthorityService` di dalam service IGD |

> Memiliki penugasan unit **tidak** dengan sendirinya memberi kemampuan klinis, dan memiliki
> kemampuan **tidak** melewati batas penugasan unit.

---

## 2. Resource dan aksi

| Resource | `Read` | `Create` | `Update` | `Delete` | `Approve` |
| --- | :-: | :-: | :-: | :-: | :-: |
| `EmergencyVisit` | ✓ | ✓ | ✓ | ✓ | — |
| `EmergencyTriage` | ✓ | ✓ | ✓ | ✓ | — |
| `EmergencyTriageDetail` | ✓ | ✓ | ✓ | ✓ | — |
| `EmergencyObservation` | ✓ | ✓ | ✓ | ✓ | — |
| `EmergencyObservationDetail` | ✓ | ✓ | ✓ | ✓ | — |
| `EmergencyResuscitation` | ✓ | ✓ | ✓ | ✓ | — |
| `EmergencyProcedureDetail` | ✓ | ✓ | ✓ | ✓ | — |
| `EmergencyDisposition` | ✓ | ✓ | ✓ | ✓ | — |
| `EmergencyDeparture` | ✓ | ✓ | ✓ | ✓ | **✓ baru** |
| `EmergencyDoctorAssignment` | ✓ | ✓ | ✓ | — | — |

`EmergencyDeparture : Approve` adalah aksi baru, dipakai **hanya** untuk membalik kejadian
yang salah pasien atau salah unit. Ia sengaja dipisahkan dari `Update` supaya kewenangan
membalik dapat diberikan kepada orang yang berbeda dari yang mencatat.

Nama resource lama `EmergencyTransfer` **dihapus** setelah masa peralihan. Selama peralihan,
kebijakan yang menunjuk `EmergencyTransfer` **wajib** dipetakan ke `EmergencyDeparture`;
melewatkan pemetaan ini membuat seluruh petugas kehilangan akses tanpa pesan yang menjelaskan.

---

## 3. Kewenangan unit per tindakan

| Tindakan | Wajib berwenang atas | Sebab |
| --- | --- | --- |
| Mencatat kedatangan pasien | Unit **tujuan** | `IGD-DEC-064` — hanya petugas unit penerima yang menjadi bukti kedatangan |
| Menerima serah terima | Unit **tujuan** | `IGD-DEC-061` |
| Menolak serah terima | Unit **tujuan** | `IGD-DEC-061` |
| Membuat catatan kepergian | Unit **asal** | Pengirim |
| Mencatat keberangkatan | Unit **asal** | `IGD-DEC-072` |
| Membatalkan kepergian | Unit **asal** | |
| Membalik kejadian | Unit **asal atau tujuan**, ditambah `Approve` | `IGD-DEC-066` |
| Menetapkan dan mengalihkan dokter | Unit **asal** | |
| Membaca daftar pantau | Unit mana pun tempat pengguna bertugas | Daftar disaring menurut penugasannya |
| **Menerima sebuah pesanan** | Unit **tujuan** | `IGD-DEC-102` — revisi 6 |
| **Menolak sebuah pesanan** | Unit **tujuan** | `IGD-DEC-102` — revisi 6 |
| **Menetapkan sikap pesanan** (`Continue`/`Handover`/`Cancel`) | Unit **asal** | `IGD-DEC-100`. `Cancel` menuntut klinisi berwenang, bukan sekadar petugas unit |
| **Mendaftarkan pesanan luar sistem** | Unit **asal** | `IGD-DEC-103` — revisi 6 |

### 3.1 Kewenangan atas pesanan — ditambahkan correction pass revisi 6

`IGD-DEC-102` memisahkan penerimaan pasien dari penerimaan tiap pesanan, tetapi revisi 6 belum
menetapkan **siapa** yang boleh menerima atau menolak sebuah pesanan. Tanpa itu, endpoint
`accept` dan `reject` terbuka bagi siapa pun yang dapat memanggilnya.

| Aturan | Kode | Sebab |
| --- | :-: | --- |
| `POST /order-items/{itemId}/accept` menuntut kewenangan atas `ToServiceUnitId` milik kepergiannya | `403` | Penerimaan adalah pernyataan unit penerima. Petugas unit lain tidak berada dalam posisi menyatakannya |
| `POST /order-items/{itemId}/reject` menuntut kewenangan yang sama | `403` | Sama |
| `PATCH /order-items/{itemId}/action` menuntut kewenangan atas unit **asal** | `403` | Sikap ditetapkan pengirim, bukan penerima |
| Sikap `Cancel` menuntut **klinisi berwenang**, bukan sekadar petugas unit asal | `403` | `IGD-DEC-100` butir (c). Membatalkan pesanan adalah keputusan klinis |
| Sikap pengganti setelah penolakan menuntut kewenangan unit **asal** | `403` | `IGD-DEC-102` butir (b) — pesanan kembali menjadi tanggung jawab dokter pemesan |

Penjaganya **`EmergencyUnitAuthorityService`** yang sudah dirancang pada bagian 3.5
`02-backend-architecture.md` — bukan mekanisme baru, dan bukan mesin hak akses, sesuai
`IGD-DEC-081` dan `IGD-DEC-086`.

> **Yang tetap berlaku.** `IGD-DEC-086` butir 7: penjagaan kewenangan unit **tidak pernah**
> memblokir pelayanan klinis darurat. Aturan di atas menahan pencatatan penerimaan pesanan,
> **bukan** pengerjaan pesanannya.
>
> **Yang belum terjawab.** `IGD-DEC-092` menetapkan unit yang belum dipetakan ke simpul
> organisasi bersifat fail-closed dengan jalan keluar beralasan. Sampai pemetaan terisi,
> **seluruh** aturan di atas berjalan lewat jalan keluar itu — dan itu berarti catatannya
> menjadi derau, persis yang diperingatkan `IGD-DEC-092`. Penyalaan penjagaan pesanan karena
> itu terikat pada gelombang `MVP-6`, bukan `MVP-5`.

---

## 4. Pemisahan tugas

| Aturan | Ditegakkan di | Keputusan |
| --- | --- | --- |
| Penyetuju pembalikan **wajib berbeda** dari pengaju | Service, dibandingkan `RecordedByUserId` dengan `ApprovedByUserId` | `IGD-DEC-066` |
| Pengirim serah terima tidak dapat menerima serah terimanya sendiri | Service, lewat kewenangan unit tujuan | `IGD-DEC-061` |
| Pencatat kedatangan bukan pengantar dari IGD | Service, lewat kewenangan unit tujuan | `IGD-DEC-064` |

Cacat `BE-IGD-016` — kolom penerima terisi oleh pengajunya sendiri — **tidak boleh terulang**.
Pada `0.3.0` kolom penerima hanya terisi oleh tindakan penerimaan, bukan oleh pembuatan.

---

## 5. Yang dicatat pada jejak audit

### 5.1 Catatan klinis — tambah-saja

`IGD-DEC-080` menetapkan catatan klinis tidak diubah di tempat. Yang dicatat:

| Yang disimpan | Di mana |
| --- | --- |
| Nilai asli beserta seluruh kolom klinisnya | Baris lama, `IsEffective = false` |
| Nilai hasil koreksi | Baris baru, `IsEffective = true` |
| Penunjuk ke baris yang dikoreksi | `Amends…Id` |
| Alasan koreksi | `AmendmentReason` |
| Pelaku dan waktu | `CreateBy`, `CreateDateTime` pada baris baru |

Berlaku untuk pengkajian, tanda vital, catatan perkembangan, penilaian triase, dan kejadian
kepergian.

### 5.2 Kejadian kepergian

Setiap kejadian menyimpan: jenis, waktu sebenarnya, waktu server, pelaku, unit pelaku, alasan,
penunjuk kejadian yang digantikan, dan penyetuju bila berupa pembalikan.

### 5.3 Apa yang **tidak** boleh masuk log

| Larangan | Sebab |
| --- | --- |
| Isi klinis lengkap — SBAR, keluhan, ringkasan pemeriksaan | `IGD-DEC-006` melarang PHI masuk mutation logging |
| Nama pasien dan nomor rekam medis pada berkas log | `IGD-DEC-006` |
| Nama sementara pasien tanpa identitas | Sama |

Yang boleh dicatat pada `LoggerService`: identitas baris, nama aksi, pelaku, dan waktu.

### 5.4 Keterbatasan yang tetap ada

`LoggerService.AuditAsync` menulis ke Serilog, **bukan** ke tabel yang dapat ditelusuri per
baris data. Untuk tabel non-klinis, jejak audit tetap hanya `IdentityModel` — pelaku terakhir
saja. `IGD-DEC-080` sengaja **tidak** memperluas ini ke tabel non-klinis.

Akibatnya: pertanyaan "siapa yang mengubah target waktu triase pada master, dan kapan" **tetap
tidak dapat dijawab**. Ini keterbatasan yang disadari, bukan kelalaian.

---

## 6. Gerbang yang belum terpenuhi

| Gerbang | Menunggu | Akibat |
| --- | --- | --- |
| Pemisahan kewenangan SuperAdmin | Security/Privacy owner | Boleh dibangun dan diuji; **tidak boleh diaktifkan di produksi** |
| Break-glass akses darurat | Security/Privacy owner, `IGD-OQ-037` | Wajib tersedia sebelum pemisahan SuperAdmin diaktifkan |
| Perilaku unit tanpa simpul organisasi | Security/Privacy owner, `IGD-OQ-071` | Penjagaan kewenangan unit **tidak boleh dinyalakan** sebelum diputuskan |
| Pengisian data penugasan unit | Corporate/HR | Sama |
| Kewenangan sementara perawat bantuan | `IGD-OQ-067` | Sama |

Prinsip yang tidak berubah: **gerbang menolak tindakan privileged, integrasi, dan finansial.
Gerbang tidak pernah memblokir pelayanan klinis darurat.**

---

## 7. Encounter-first — baru pada `0.5.0`, **Rencana (belum tersedia)**

### 7.1 Aksi dan resource baru

| Resource | Aksi | Pasangan atribut pada method (persis) | Dipakai | Keputusan |
| --- | --- | --- | --- | --- |
| `EmergencyVisit` | `Read` *(sudah ada)* | `[AccessPermission("EmergencyVisit", "Read")]` | `GET /triage-queue` | `IGD-DEC-139` |
| `EmergencyVisit` | `Create` *(sudah ada)* | `[AccessPermission("EmergencyVisit", "Create")]` | `POST /start-triage` (kedua mode) | `IGD-DEC-139`, `143` |
| `EmergencyVisit` | **`NoShow`** — **baru** | `[AccessAction("NoShow", "Mark Emergency Encounter No Show", Description = "Menandai pasien IGD pergi sebelum ditriage", AccessType = AccessTypes.Update)]` + `[AccessPermission("EmergencyVisit", "NoShow")]` | `POST /no-show` | `IGD-DEC-142` — hak akses IGD tersendiri |
| `EmergencyVisit` | `Update` *(sudah ada)* | `[AccessPermission("EmergencyVisit", "Update")]` | `PATCH /{id}/arrival-time` | `IGD-DEC-147` |
| **`EmergencyEncounterReconciliation`** — resource **baru** | `Read` | `[AccessPermission("EmergencyEncounterReconciliation", "Read")]` | `GET /preview`, `GET /runs`, `GET /runs/{id}` | `IGD-DEC-148` |
| `EmergencyEncounterReconciliation` | **`Process`** | `[AccessAction("Process", "Execute Emergency Encounter Reconciliation", AccessType = AccessTypes.Update)]` + `[AccessPermission("EmergencyEncounterReconciliation", "Process")]` | `POST /runs` | `IGD-DEC-148` — admin khusus |
| `EmergencyEncounterReconciliation` | **`Reverse`** | `[AccessAction("Reverse", "Reverse Emergency Encounter Reconciliation", AccessType = AccessTypes.Update)]` + `[AccessPermission("EmergencyEncounterReconciliation", "Reverse")]` | `POST /runs/{id}/reverse` | `IGD-DEC-148` |
| `PatientEncounter` *(milik Registrasi)* | `Create`, `Update` *(sudah ada)* | Tidak berubah | Override pendaftaran ganda **tanpa** permission baru | `IGD-DEC-145` |

Nama aksi khusus berjenis `Update` sudah lazim di repository (`Cancel`, `Reverse`, `Process`,
`Approve`). Setiap method baru **wajib** memuat pasangan `[AccessAction]` + `[AccessPermission]` pada method
yang sama dengan nama aksi identik huruf demi huruf.

**Pemberian hak ke peran** ada di basis data dan berbeda antar rumah sakit (`CONFIGURABLE_DEFAULT`,
gate §5.3). Rekomendasi awal: `NoShow` untuk perawat triage; `EmergencyEncounterReconciliation : Process` dan
`Reverse` **hanya** untuk peran admin data, **terpisah** dari peran klinis.

**Keterbatasan yang diterima — `IGD-DEC-158` (B3).** Pra-cek loket `GET /active-episode` dan `POST /start-triage`
sama-sama memakai `EmergencyVisit : Create`. Petugas loket yang memegang izin itu untuk pra-cek karena itu **secara
teknis** dapat memanggil Mulai Triage lewat API, walau `IGD-DEC-147` menetapkan pengisi waktu tiba adalah perawat
triage. *Contoh:* petugas loket Sari memanggil `start-triage` dari Postman; kunjungan lahir dengan waktu tiba
`Confirmed` atas nama Sari. Pelakunya terekam, dan layar Mulai Triage hanya ada di menu Triage. Aksi tersendiri
`EmergencyVisit : StartTriage` boleh ditambahkan kemudian secara **aditif**. Wajib ditinjau ulang bila Nursing
authority ditunjuk.

**Status bagian ini: `approved`** — `IGD-DEC-157`, 22 September 2026; terkunci hash (manifest bagian 2).

### 7.2 Jejak audit

| Kejadian | Yang tersimpan tahan lama | Tempat |
| --- | --- | --- |
| Pendaftaran ganda dengan override | Encounter baru, episode yang dilangkahi, alasan, pelaku (token), waktu (server) | Tabel IGD baru `EmgDuplicateEpisodeOverride` — tambah-saja |
| Pasien pergi sebelum ditriage | Pelaku, waktu, alasan | `RegPatientEncounter.NoShowByUserId` / `NoShowAt` / `NoShowReason` |
| Konfirmasi/koreksi waktu tiba | Sumber nilai, pelaku, waktu konfirmasi | `EmgVisit.ArrivalTimeSource` / `ArrivalConfirmedByUserId` / `ArrivalConfirmedAt` — **hanya nilai terakhir**; riwayat koreksi tidak disimpan (`IGD-DEC-152` tidak memintanya) |
| Run rekonsiliasi | Pelaku, waktu, alasan, jumlah per kelas, baris yang ditulis beserta nilai **sebelum** dan **sesudah** | Tabel IGD baru `EmgEncounterReconciliationRun` + `EmgEncounterReconciliationItem` |
| Pembalikan run | Pelaku, waktu, alasan, baris yang dikembalikan dan yang dilewati | Tabel yang sama |
| Encounter ditutup mengikuti kunjungan | Pelaku = pelaku aksi kunjungan; waktu server | Kolom status akhir `RegPatientEncounter` + `UpdateBy`/`UpdateDateTime` |

**Tidak boleh masuk log** (tambahan pada bagian 5.3): alasan override, alasan NoShow, dan alasan
rekonsiliasi dapat memuat keterangan klinis atau identitas — dicatat di tabel audit di atas, **bukan** di
custom logger.

## 8. Penutupan lewat disposisi — baru pada `0.6.0`, **Rencana (belum tersedia)**

### 8.1 Hak akses

**Nol resource baru, nol aksi baru.** Penutupan kunjungan yang terjadi sebagai akibat disposisi, observasi,
kepergian, atau sikap pesanan **menumpang** izin yang sudah dimiliki petugas untuk aksi itu sendiri.

| Aksi petugas | Izin yang sudah dipakai | Efek samping penutupan |
| --- | --- | --- |
| Menandai disposisi `Executed` | `EmergencyDisposition : Update` | Kunjungan ditutup bila penjaga lolos |
| Menutup observasi | `EmergencyObservation : Update` | Sama, bila observasi itu penahan terakhir |
| Menerima/menolak/membatalkan serah terima | `EmergencyDeparture : Update` | Sama |
| Menetapkan sikap pesanan | `EmergencyDeparture : Update` | Sama |
| Melihat daftar menunggu penutupan | `EmergencyVisit : Read` | — (baca-saja) |

**Keterbatasan yang diterima.** Petugas yang hanya memegang izin observasi atau kepergian secara **tidak langsung**
dapat menyebabkan kunjungan tertutup, walaupun ia tidak memegang `EmergencyVisit : Update`. Ini disengaja:
penutupan itu bukan tindakan terpisah, melainkan akibat dari aksi yang memang wewenangnya, dan penjaga penutupan
tetap memastikan seluruh kewajiban klinis sudah tuntas lebih dulu. Pelakunya tetap terekam.

### 8.2 Jejak audit

| Kejadian | Yang tersimpan tahan lama | Tempat |
| --- | --- | --- |
| Kunjungan ditutup lewat disposisi | Disposisi pemicunya, pelaku, dan waktu | `EmgVisit.ClosedByDispositionId`, `VisitCompletedAt`, `UpdateBy`, `UpdateDateTime` |
| Kunjungan ditutup manual | Pelaku dan waktu; `ClosedByDispositionId` kosong | `EmgVisit.VisitCompletedAt`, `UpdateBy` |
| Encounter ikut tertutup | Sama dengan `BE-IGD-051` — kolom status akhir `RegPatientEncounter` | Daftar tertutup integration §5.2 |

Alasan penahan **tidak** disimpan tahan lama: ia dihitung ulang saat dibaca, karena keadaannya berubah begitu
penahannya dibereskan. Menyimpannya akan menghasilkan alasan basi yang terbaca sebagai fakta.
